using AIInterviewPlatform.Application.DTOs.Dashboard;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Application.Common.Exceptions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers;

/// <summary>
/// Dashboard API endpoints for retrieving user dashboard data.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IDashboardQueryService _queryService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        IDashboardQueryService queryService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _queryService = queryService;
        _logger = logger;
        Console.WriteLine("███████████████████████████████████████████████████");
        Console.WriteLine("█ DASHBOARDCONTROLLER CONSTRUCTOR CALLED");
        Console.WriteLine("███████████████████████████████████████████████████");
    }

    /// <summary>
    /// Get the full dashboard with all aggregated data.
    /// </summary>
    /// <remarks>
    /// Retrieves comprehensive dashboard including:
    /// - Readiness scores and trends
    /// - Missing skills from latest analysis
    /// - Interview statistics
    /// - Roadmap progress
    /// - Recent feedback
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Complete dashboard data.</returns>
    /// <response code="200">Dashboard retrieved successfully.</response>
    /// <response code="401">Unauthorized - user not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation("=== DASHBOARD DEBUG ===");
            _logger.LogInformation("Authenticated UserId from token: {UserId}", userId);
            _logger.LogInformation("Getting dashboard for user {UserId}", userId);

            var result = await _dashboardService.GetDashboardAsync(userId);

            Console.WriteLine("███████████████████████████████████████████████████");
            Console.WriteLine("█ CONTROLLER - FINAL DASHBOARD DTO");
            Console.WriteLine($"█ Readiness - CurrentScore: {result.Readiness?.CurrentScore}, Trend: {result.Readiness?.Trend}");
            Console.WriteLine($"█ Interviews - Total: {result.Interviews?.TotalInterviews}, Completed: {result.Interviews?.CompletedInterviews}, Pending: {result.Interviews?.PendingInterviews}");
            Console.WriteLine($"█ Interviews - Avg: {result.Interviews?.AverageScore}, Highest: {result.Interviews?.HighestScore}, Lowest: {result.Interviews?.LowestScore}");
            Console.WriteLine($"█ Roadmap - TotalRoadmaps: {result.RoadmapProgress?.TotalRoadmaps}, Progress: {result.RoadmapProgress?.OverallProgressPercentage}%");
            Console.WriteLine($"█ Roadmap - ActiveTitle: {result.RoadmapProgress?.ActiveRoadmapTitle ?? "N/A"}, ActiveProgress: {result.RoadmapProgress?.ActiveRoadmapProgress}");
            Console.WriteLine($"█ SkillGaps - MissingSkills: {result.SkillGaps?.TotalMissingSkills}");
            Console.WriteLine($"█ Feedbacks count: {result.RecentFeedbacks?.Count ?? 0}");
            Console.WriteLine("███████████████████████████████████████████████████");

            _logger.LogInformation("Dashboard Response: {@Dashboard}", result);
            _logger.LogInformation("Dashboard retrieved successfully for user {UserId}", userId);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error getting dashboard");
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving the dashboard.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get readiness dashboard with scores and trends.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Readiness dashboard with latest and previous scores.</returns>
    /// <response code="200">Readiness dashboard retrieved successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("readiness")]
    [ProducesResponseType(typeof(ReadinessDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReadinessDashboardResponse>> GetReadiness(CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            var result = await _dashboardService.GetReadinessDashboardAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting readiness dashboard");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving readiness data.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get skill gaps dashboard with missing skills.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Skill gaps analysis with missing skills list.</returns>
    /// <response code="200">Skill gaps dashboard retrieved successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">No skill gap analyses found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("skill-gaps")]
    [ProducesResponseType(typeof(SkillGapsDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SkillGapsDashboardResponse>> GetSkillGaps(CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            var result = await _dashboardService.GetSkillGapsDashboardAsync(userId);

            if (result == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = "No skill gap analyses found for this user.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skill gaps dashboard");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving skill gaps data.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get history dashboard with analysis timeline.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>History dashboard with readiness timeline.</returns>
    /// <response code="200">History dashboard retrieved successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(HistoryDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HistoryDashboardResponse>> GetHistory(CancellationToken ct)
    {
        try
        {
            var userId = GetUserId();
            var result = await _dashboardService.GetHistoryDashboardAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history dashboard");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving history data.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            throw new ValidationException("Invalid user identifier");
        }
        
        return userId;
    }
}
