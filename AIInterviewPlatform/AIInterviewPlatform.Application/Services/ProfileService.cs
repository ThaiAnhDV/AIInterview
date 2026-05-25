using AIInterviewPlatform.Application.DTOs.Profile;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;

namespace AIInterviewPlatform.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetProfileResponse> GetMyProfileAsync(long userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var accounts = await _unitOfWork.AuthenticationAccounts
                .FindAsync(x => x.UserId == userId);

            var account = accounts.FirstOrDefault();

            var profiles = await _unitOfWork.UserProfiles
                .FindAsync(x => x.UserId == userId);

            var profile = profiles.FirstOrDefault();

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    FullName = "",
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.UserProfiles.AddAsync(profile);
                await _unitOfWork.SaveChangesAsync();
            }

            return new GetProfileResponse
            {
                UserId = user.Id,
                Email = account?.Email ?? "",
                FullName = profile.FullName,
                Phone = profile.Phone,
                EducationLevel = profile.EducationLevel,
                CareerGoal = profile.CareerGoal
            };
        }

        public async Task<bool> UpdateMyProfileAsync(
            long userId,
            UpdateProfileRequest request)
        {
            var profiles = await _unitOfWork.UserProfiles
                .FindAsync(x => x.UserId == userId);

            var profile = profiles.FirstOrDefault();

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.UserProfiles.AddAsync(profile);
            }

            profile.FullName = request.FullName;
            profile.Phone = request.Phone;
            profile.EducationLevel = request.EducationLevel;
            profile.CareerGoal = request.CareerGoal;
            profile.UpdatedAt = DateTime.Now;

            _unitOfWork.UserProfiles.Update(profile);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}