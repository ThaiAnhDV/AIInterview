using AIInterviewPlatform.Application.Common.Exceptions;
using AIInterviewPlatform.Application.DTOs.Auth;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Invalid register request.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.FullName))
                    errors.Add("Full name is required.");

                if (string.IsNullOrWhiteSpace(request.Email))
                    errors.Add("Email is required.");
                else if (!Regex.IsMatch(request.Email.Trim(),
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    errors.Add("Invalid email format.");

                if (string.IsNullOrWhiteSpace(request.Password))
                    errors.Add("Password is required.");
                else if (request.Password.Length < 6)
                    errors.Add("Password must be at least 6 characters.");
            }

            if (errors.Any())
                throw new ValidationException(errors);

            var email = request.Email.Trim().ToLower();

            var existedAccount = await _context.AuthenticationAccounts
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (existedAccount != null)
                throw new ValidationException("Email already exists.");

            var user = new User
            {
                UserType = UserType.USER,
                Status = UserStatus.ACTIVE,
                CreatedAt = DateTime.Now
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var account = new AuthenticationAccount
            {
                UserId = user.Id,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            await _context.AuthenticationAccounts.AddAsync(account);

            var profile = new UserProfile
            {
                UserId = user.Id,
                FullName = request.FullName.Trim(),
                CreatedAt = DateTime.Now
            };

            await _context.UserProfiles.AddAsync(profile);

            await _context.SaveChangesAsync();

            var role = user.UserType.ToString();

            var token = GenerateJwtToken(
                user.Id,
                profile.FullName,
                account.Email,
                role);

            return new AuthResponseDto
            {
                UserId = user.Id,
                FullName = profile.FullName,
                Email = account.Email,
                Role = role,
                Token = token
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Invalid login request.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    errors.Add("Email is required.");
                else if (!Regex.IsMatch(request.Email.Trim(),
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    errors.Add("Invalid email format.");

                if (string.IsNullOrWhiteSpace(request.Password))
                    errors.Add("Password is required.");
            }

            if (errors.Any())
                throw new ValidationException(errors);

            var email = request.Email.Trim().ToLower();

            var account = await _context.AuthenticationAccounts
                .Include(x => x.User)
                .ThenInclude(x => x.UserProfile)
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (account == null)
                throw new ValidationException("Invalid email or password.");

            if (account.User.Status != UserStatus.ACTIVE)
                throw new ValidationException("Account is inactive.");

            bool isValidPassword;

            try
            {
                isValidPassword = BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    account.PasswordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                throw new ValidationException("Invalid email or password.");
            }

            if (!isValidPassword)
                throw new ValidationException("Invalid email or password.");

            var fullName = account.User.UserProfile?.FullName ?? account.Email;
            var role = account.User.UserType.ToString();

            var token = GenerateJwtToken(
                account.UserId,
                fullName,
                account.Email,
                role);

            return new AuthResponseDto
            {
                UserId = account.UserId,
                FullName = fullName,
                Email = account.Email,
                Role = role,
                Token = token
            };
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Credential))
                throw new ValidationException("Google credential is required.");

            var clientId = _configuration["GoogleAuth:ClientId"];

            if (string.IsNullOrWhiteSpace(clientId))
                throw new ValidationException("Google login is not configured.");

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.Credential,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
            }
            catch (InvalidJwtException)
            {
                throw new ValidationException("Google login failed. Please try again.");
            }

            if (!payload.EmailVerified)
                throw new ValidationException("Google email is not verified.");

            var email = payload.Email.Trim().ToLowerInvariant();
            var fullName = GetGoogleDisplayName(payload);

            var account = await _context.AuthenticationAccounts
                .Include(x => x.User)
                .ThenInclude(x => x.UserProfile)
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (account == null)
            {
                account = await CreateGoogleAccountAsync(email, fullName);
            }
            else if (account.User.Status != UserStatus.ACTIVE)
            {
                throw new ValidationException("Account is inactive.");
            }

            if (account.User.UserProfile == null)
            {
                account.User.UserProfile = new UserProfile
                {
                    UserId = account.UserId,
                    FullName = fullName,
                    CreatedAt = DateTime.Now
                };

                await _context.UserProfiles.AddAsync(account.User.UserProfile);
                await _context.SaveChangesAsync();
            }
            else if (string.IsNullOrWhiteSpace(account.User.UserProfile.FullName))
            {
                account.User.UserProfile.FullName = fullName;
                account.User.UserProfile.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            var role = account.User.UserType.ToString();
            var token = GenerateJwtToken(
                account.UserId,
                account.User.UserProfile?.FullName ?? fullName,
                account.Email,
                role);

            return new AuthResponseDto
            {
                UserId = account.UserId,
                FullName = account.User.UserProfile?.FullName ?? fullName,
                Email = account.Email,
                Role = role,
                Token = token
            };
        }

        private async Task<AuthenticationAccount> CreateGoogleAccountAsync(
            string email,
            string fullName)
        {
            var user = new User
            {
                UserType = UserType.USER,
                Status = UserStatus.ACTIVE,
                CreatedAt = DateTime.Now
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var account = new AuthenticationAccount
            {
                UserId = user.Id,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                IsVerified = true,
                CreatedAt = DateTime.Now,
                User = user
            };

            var profile = new UserProfile
            {
                UserId = user.Id,
                FullName = fullName,
                CreatedAt = DateTime.Now,
                User = user
            };

            await _context.AuthenticationAccounts.AddAsync(account);
            await _context.UserProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();

            user.AuthenticationAccount = account;
            user.UserProfile = profile;

            return account;
        }

        private static string GetGoogleDisplayName(GoogleJsonWebSignature.Payload payload)
        {
            if (!string.IsNullOrWhiteSpace(payload.Name))
                return payload.Name.Trim();

            var parts = new[] { payload.GivenName, payload.FamilyName }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part!.Trim());

            var fullName = string.Join(" ", parts);
            return string.IsNullOrWhiteSpace(fullName) ? payload.Email : fullName;
        }

        private string GenerateJwtToken(
            long userId,
            string fullName,
            string email,
            string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
