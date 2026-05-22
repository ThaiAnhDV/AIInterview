using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Application.DTOs.Profile;

namespace AIInterviewPlatform.Application.Interfaces.Services
{
    public interface IProfileService
    {
        Task<ProfileResponseDto> GetMyProfileAsync(long userId);
        Task<ProfileResponseDto> UpdateMyProfileAsync(long userId, UpdateProfileRequestDto request);
    }
}
