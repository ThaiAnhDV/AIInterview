using AIInterviewPlatform.Application.Common.Exceptions;
using AIInterviewPlatform.Application.DTOs.Auth;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
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

            var isValidPassword = BCrypt.Net.BCrypt.Verify(
                request.Password,
                account.PasswordHash);

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