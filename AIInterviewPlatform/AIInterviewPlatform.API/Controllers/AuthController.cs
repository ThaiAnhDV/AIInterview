using AIInterviewPlatform.Application.Common.Exceptions;
using AIInterviewPlatform.Application.DTOs.Auth;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

            return Ok(new
            {
                success = true,
                message = "Register successfully.",
                data = result
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new
            {
                success = false,
                errors = ex.Errors
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                errors = new[] { ex.Message }
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);

            return Ok(new
            {
                success = true,
                message = "Login successfully.",
                data = result
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new
            {
                success = false,
                errors = ex.Errors
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                errors = new[] { ex.Message }
            });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            success = true,
            data = new
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                fullName = User.FindFirstValue(ClaimTypes.Name),
                email = User.FindFirstValue(ClaimTypes.Email),
                role = User.FindFirstValue(ClaimTypes.Role)
            }
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new
        {
            success = true,
            message = "Logout successfully. Please remove token on client side."
        });
    }
}