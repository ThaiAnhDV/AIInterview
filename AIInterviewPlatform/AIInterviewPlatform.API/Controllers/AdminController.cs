using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AdminController(
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpGet("system-status")]
    public async Task<IActionResult> GetSystemStatus(CancellationToken cancellationToken)
    {
        var geminiApiKey = _configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        var isGeminiConfigured = !string.IsNullOrWhiteSpace(geminiApiKey) &&
            !geminiApiKey.Equals("YOUR_API_KEY", StringComparison.OrdinalIgnoreCase);

        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalAdmins = await _context.Users.CountAsync(
            user => user.UserType == UserType.ADMIN,
            cancellationToken);

        return Ok(new
        {
            gemini = new
            {
                configured = isGeminiConfigured,
                model = "gemini-2.0-flash",
                keyPreview = isGeminiConfigured
                    ? $"{geminiApiKey[..Math.Min(6, geminiApiKey.Length)]}..."
                    : "Chưa cấu hình"
            },
            users = new
            {
                total = totalUsers,
                admins = totalAdmins
            },
            server = new
            {
                checkedAt = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            }
        });
    }
}
