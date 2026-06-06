using AIInterviewPlatform.Application.DTOs.Profile;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface IProfileService
    {
        Task<GetProfileResponse> GetMyProfileAsync(long userId);

        Task<bool> UpdateMyProfileAsync(
            long userId,
            UpdateProfileRequest request
        );
    }
}