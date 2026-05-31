using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("readiness")]
    public async Task<IActionResult> GetReadiness()
    {
        var userId = GetUserId();
        var result = await _dashboardService.GetReadinessDashboardAsync(userId);
        return Ok(result);
    }

    [HttpGet("skill-gaps")]
    public async Task<IActionResult> GetSkillGaps()
    {
        var userId = GetUserId();
        var result = await _dashboardService.GetSkillGapsDashboardAsync(userId);

        if (result == null)
        {
            return NotFound(new { message = "No skill gap analyses found" });
        }

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        var result = await _dashboardService.GetHistoryDashboardAsync(userId);
        return Ok(result);
    }

    private long GetUserId()
    {
        return long.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
