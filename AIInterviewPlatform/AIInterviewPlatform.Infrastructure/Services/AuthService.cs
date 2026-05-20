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
            var email = request.Email.Trim().ToLower();

            var existedAccount = await _context.AuthenticationAccounts
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (existedAccount != null)
            {
                throw new Exception("Email already exists.");
            }

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

            // Nếu UserProfile của bạn có FullName thì giữ đoạn này.
            // Nếu UserProfile không có FullName, hãy comment đoạn này lại.
            var profile = new UserProfile
            {
                UserId = user.Id,
                FullName = request.FullName,
                CreatedAt = DateTime.Now
            };

            await _context.UserProfiles.AddAsync(profile);

            await _context.SaveChangesAsync();

            var role = user.UserType.ToString();
            var token = GenerateJwtToken(
                user.Id,
                request.FullName,
                account.Email,
                role
            );

            return new AuthResponseDto
            {
                UserId = user.Id,
                FullName = request.FullName,
                Email = account.Email,
                Role = role,
                Token = token
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var email = request.Email.Trim().ToLower();

            var account = await _context.AuthenticationAccounts
                .Include(x => x.User)
                    .ThenInclude(x => x.UserProfile)
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (account == null)
            {
                throw new Exception("Invalid email or password.");
            }

            if (account.User.Status != UserStatus.ACTIVE)
            {
                throw new Exception("Account is inactive.");
            }

            var isValidPassword = BCrypt.Net.BCrypt.Verify(
                request.Password,
                account.PasswordHash
            );

            if (!isValidPassword)
            {
                throw new Exception("Invalid email or password.");
            }

            var fullName = account.User.UserProfile?.FullName ?? account.Email;
            var role = account.User.UserType.ToString();

            var token = GenerateJwtToken(
                account.UserId,
                fullName,
                account.Email,
                role
            );

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
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}