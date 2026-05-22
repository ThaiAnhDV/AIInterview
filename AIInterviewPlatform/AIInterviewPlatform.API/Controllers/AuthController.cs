using AIInterviewPlatform.Application.DTOs.Auth;
using AIInterviewPlatform.Application.DTOs.Common;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(ApiResponse<object>.Ok(result, "Register successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(ApiResponse<object>.Ok(result, "Login successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.Fail("Unauthorized."));
            }

            var data = new
            {
                UserId = userId,
                Email = email,
                Role = role
            };

            return Ok(ApiResponse<object>.Ok(data, "Get current user successfully."));
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(ApiResponse<object>.Ok(null!, "Logout successfully. Please remove token on client side."));
        }
    }
}