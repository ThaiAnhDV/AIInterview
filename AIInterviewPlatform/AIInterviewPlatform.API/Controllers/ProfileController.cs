using AIInterviewPlatform.Application.DTOs.Common;
using AIInterviewPlatform.Application.DTOs.Profile;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AIInterviewPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _profileService.GetMyProfileAsync(userId);

                return Ok(ApiResponse<ProfileResponseDto>.Ok(result, "Get profile successfully."));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(ApiResponse<object>.Fail("Unauthorized."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _profileService.UpdateMyProfileAsync(userId, request);

                return Ok(ApiResponse<ProfileResponseDto>.Ok(result, "Update profile successfully."));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(ApiResponse<object>.Fail("Unauthorized."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        private long GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();

            return long.Parse(userId);
        }
    }
}
