using AIInterviewPlatform.Application.DTOs.Roadmap.Requests;
using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Application.Mappings;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoadmapController : ControllerBase
{
    private readonly IRoadmapApplicationService _roadmapService;
    private readonly IMilestoneGeneratorService _milestoneGenerator;
    private readonly IActivityDescriptionService _activityDescriptionService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RoadmapController> _logger;

    public RoadmapController(
        IRoadmapApplicationService roadmapService,
        IMilestoneGeneratorService milestoneGenerator,
        IActivityDescriptionService activityDescriptionService,
        ApplicationDbContext context,
        ILogger<RoadmapController> logger)
    {
        _roadmapService = roadmapService;
        _milestoneGenerator = milestoneGenerator;
        _activityDescriptionService = activityDescriptionService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateRoadmap([FromBody] GenerateRoadmapApiRequest request)
    {
        var userId = GetUserId();

        _logger.LogInformation(
            "Generating roadmap for user {UserId} from analysis {AnalysisId}",
            userId, request.SkillGapAnalysisId);

        var roadmap = await _roadmapService.GenerateRoadmapFromAnalysisAsync(
            userId,
            new GenerateRoadmapFromAnalysisRequest
            {
                SkillGapAnalysisId = request.SkillGapAnalysisId,
                MilestonesPerSkill = request.MilestonesPerSkill,
                ActivitiesPerMilestone = request.ActivitiesPerMilestone
            });

        return Ok(new ApiResponse<RoadmapDto>
        {
            Success = roadmap.Success,
            Message = roadmap.Message ?? (roadmap.Success ? "Roadmap generated successfully" : "Roadmap generation failed"),
            Data = roadmap
        });
    }

    [HttpPost("generate-from-skills")]
    public async Task<IActionResult> GenerateRoadmapFromSkills([FromBody] GenerateRoadmapFromSkillsRequest request)
    {
        var userId = GetUserId();

        _logger.LogInformation(
            "Generating roadmap for user {UserId} with {Count} skills",
            userId, request.Skills?.Count ?? 0);

        if (request.Skills == null || request.Skills.Count == 0)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Skills list cannot be empty"
            });
        }

        var roadmap = await _roadmapService.GenerateRoadmapFromMissingSkillsAsync(
            userId,
            new GenerateRoadmapFromMissingSkillsRequest
            {
                MissingSkills = request.Skills,
                TargetJobId = request.TargetJobId
            });

        return Ok(new ApiResponse<RoadmapDto>
        {
            Success = roadmap.Success,
            Message = roadmap.Message ?? (roadmap.Success ? "Roadmap generated successfully" : "Roadmap generation failed"),
            Data = roadmap
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRoadmaps()
    {
        try
        {
            var userId = GetUserId();
            var roadmaps = await _roadmapService.GetUserRoadmapsAsync(userId);

            return Ok(new ApiResponse<List<RoadmapSummaryDto>>
            {
                Success = true,
                Data = roadmaps
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roadmaps");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving roadmaps"
            });
        }
    }

    [HttpGet("{roadmapId}")]
    public async Task<IActionResult> GetRoadmap(long roadmapId)
    {
        try
        {
            var userId = GetUserId();
            var roadmap = await _roadmapService.GetRoadmapByIdAsync(userId, roadmapId);

            if (roadmap == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Roadmap not found"
                });
            }

            return Ok(new ApiResponse<RoadmapDto>
            {
                Success = true,
                Data = roadmap
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roadmap {RoadmapId}", roadmapId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the roadmap"
            });
        }
    }

    [HttpPost("complete-activity/{activityId}")]
    public async Task<IActionResult> CompleteActivity(long activityId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _roadmapService.CompleteActivityAsync(userId, activityId);

            if (!result.Success)
            {
                return Ok(new ApiResponse<ActivityCompletionResultDto>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result
                });
            }

            return Ok(new ApiResponse<ActivityCompletionResultDto>
            {
                Success = true,
                Message = "Activity completed successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing activity {ActivityId}", activityId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while completing the activity"
            });
        }
    }

    [HttpGet("skill-gap-analysis/{analysisId}")]
    public async Task<IActionResult> GetSkillGapAnalysisForRoadmap(long analysisId)
    {
        try
        {
            var userId = GetUserId();

            var analysis = await _context.SkillGapAnalyses
                .Include(x => x.SkillGaps)
                    .ThenInclude(x => x.Skill)
                .Include(x => x.JobDescription)
                .FirstOrDefaultAsync(x =>
                    x.Id == analysisId &&
                    x.UserId == userId);

            if (analysis == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Skill gap analysis not found"
                });
            }

            var response = new SkillGapAnalysisPreviewDto
            {
                AnalysisId = analysis.Id,
                JobDescriptionTitle = analysis.JobDescription?.Content ?? "Unknown Job",
                MissingSkillsCount = analysis.SkillGaps.Count,
                MissingSkills = analysis.SkillGaps.Select(g => new SkillPreviewDto
                {
                    SkillId = g.SkillId,
                    SkillName = g.Skill?.SkillName ?? "Unknown Skill",
                    GapLevel = g.GapLevel?.ToString() ?? string.Empty
                }).ToList(),
                ReadinessScore = 0
            };

            var score = await _context.ReadinessScores
                .Where(x => x.SkillGapAnalysisId == analysisId)
                .OrderByDescending(x => x.CalculatedAt)
                .FirstOrDefaultAsync();

            if (score != null)
            {
                response.ReadinessScore = score.Score;
            }

            return Ok(new ApiResponse<SkillGapAnalysisPreviewDto>
            {
                Success = true,
                Data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skill gap analysis for roadmap {AnalysisId}", analysisId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while getting the skill gap analysis"
            });
        }
    }

    [HttpPost("preview-from-skills")]
    public IActionResult PreviewRoadmapFromSkills([FromBody] GenerateRoadmapFromSkillsRequest request)
    {
        try
        {
            if (request.Skills == null || request.Skills.Count == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Skills list cannot be empty"
                });
            }

            var milestones = _milestoneGenerator.GenerateMilestones(request.Skills);
            var preview = new RoadmapPreviewDto
            {
                Milestones = milestones,
                TotalSkills = request.Skills.Count,
                EstimatedActivities = milestones.Sum(m => m.Activities.Count)
            };

            return Ok(new ApiResponse<RoadmapPreviewDto>
            {
                Success = true,
                Data = preview
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing roadmap from skills");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while previewing the roadmap"
            });
        }
    }

    private long GetUserId()
    {
        return long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}

#region Request/Response DTOs

public class GenerateRoadmapApiRequest
{
    public long SkillGapAnalysisId { get; set; }
    public int MilestonesPerSkill { get; set; } = 2;
    public int ActivitiesPerMilestone { get; set; } = 3;
}

public class GenerateRoadmapFromSkillsRequest
{
    public List<SkillGapForRoadmapDto> Skills { get; set; } = [];
    public long? TargetJobId { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class SkillGapAnalysisPreviewDto
{
    public long AnalysisId { get; set; }
    public string JobDescriptionTitle { get; set; } = string.Empty;
    public int MissingSkillsCount { get; set; }
    public List<SkillPreviewDto> MissingSkills { get; set; } = [];
    public decimal ReadinessScore { get; set; }
}

public class SkillPreviewDto
{
    public long SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string GapLevel { get; set; } = string.Empty;
}

public class RoadmapPreviewDto
{
    public List<MilestoneDto> Milestones { get; set; } = [];
    public int TotalSkills { get; set; }
    public int EstimatedActivities { get; set; }
}

#endregion
