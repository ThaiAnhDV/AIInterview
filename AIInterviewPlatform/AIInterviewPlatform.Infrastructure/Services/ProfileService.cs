using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AIInterviewPlatform.Application.DTOs.Profile;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileResponseDto> GetMyProfileAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.AuthenticationAccount)
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            if (user.AuthenticationAccount == null)
                throw new Exception("Authentication account not found.");

            return new ProfileResponseDto
            {
                UserId = user.Id,
                Email = user.AuthenticationAccount.Email,
                FullName = user.UserProfile?.FullName,
                Phone = user.UserProfile?.Phone,
                EducationLevel = user.UserProfile?.EducationLevel,
                CareerGoal = user.UserProfile?.CareerGoal,
                UserType = user.UserType.ToString(),
                Status = user.Status.ToString()
            };
        }

        public async Task<ProfileResponseDto> UpdateMyProfileAsync(long userId, UpdateProfileRequestDto request)
        {
            var user = await _context.Users
                .Include(u => u.AuthenticationAccount)
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            if (user.UserProfile == null)
            {
                user.UserProfile = new UserProfile
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserProfiles.AddAsync(user.UserProfile);
            }

            user.UserProfile.FullName = request.FullName;
            user.UserProfile.Phone = request.Phone;
            user.UserProfile.EducationLevel = request.EducationLevel;
            user.UserProfile.CareerGoal = request.CareerGoal;
            user.UserProfile.UpdatedAt = DateTime.UtcNow;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ProfileResponseDto
            {
                UserId = user.Id,
                Email = user.AuthenticationAccount?.Email ?? string.Empty,
                FullName = user.UserProfile.FullName,
                Phone = user.UserProfile.Phone,
                EducationLevel = user.UserProfile.EducationLevel,
                CareerGoal = user.UserProfile.CareerGoal,
                UserType = user.UserType.ToString(),
                Status = user.Status.ToString()
            };
        }
    }
}
