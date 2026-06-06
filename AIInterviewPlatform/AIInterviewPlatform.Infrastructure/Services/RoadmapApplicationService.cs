using AIInterviewPlatform.Application.DTOs.Roadmap.Requests;
using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Mappings;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace AIInterviewPlatform.Infrastructure.Services;

public class RoadmapApplicationService : IRoadmapApplicationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMilestoneGeneratorService _milestoneGenerator;
    private readonly IActivityDescriptionService _activityDescriptionService;
    private readonly IRoadmapUnitOfWork _roadmapUnitOfWork;
    private readonly ILogger<RoadmapApplicationService> _logger;

    public RoadmapApplicationService(
        ApplicationDbContext context,
        IMilestoneGeneratorService milestoneGenerator,
        IActivityDescriptionService activityDescriptionService,
        IRoadmapUnitOfWork roadmapUnitOfWork,
        ILogger<RoadmapApplicationService> logger)
    {
        _context = context;
        _milestoneGenerator = milestoneGenerator;
        _activityDescriptionService = activityDescriptionService;
        _roadmapUnitOfWork = roadmapUnitOfWork;
        _logger = logger;
    }

    public async Task<RoadmapDto> GenerateRoadmapFromMissingSkillsAsync(
        long userId,
        GenerateRoadmapFromMissingSkillsRequest request)
    {
        _logger.LogInformation(
            "Generating roadmap for user {UserId} with {Count} missing skills",
            userId, request.MissingSkills.Count);

        if (request.MissingSkills == null || request.MissingSkills.Count == 0)
        {
            return CreateFailureRoadmap("INVALID_REQUEST", "Missing skills list cannot be empty");
        }

        var milestones = _milestoneGenerator.GenerateMilestones(request.MissingSkills);

        var roadmap = await CreateRoadmapWithMilestonesAsync(
            userId,
            milestones,
            null,
            null);

        if (!roadmap.Success)
        {
            return roadmap;
        }

        _logger.LogInformation(
            "Roadmap {RoadmapId} generated with {MilestoneCount} milestones",
            roadmap.Id, milestones.Count);

        return roadmap;
    }

    public async Task<RoadmapDto> GenerateRoadmapFromAnalysisAsync(
        long userId,
        GenerateRoadmapFromAnalysisRequest request)
    {
        _logger.LogInformation(
            "Generating roadmap for user {UserId} from analysis {AnalysisId}",
            userId, request.SkillGapAnalysisId);

        var analysis = await _context.SkillGapAnalyses
            .Include(x => x.SkillGaps)
                .ThenInclude(x => x.Skill)
            .Include(x => x.JobDescription)
            .FirstOrDefaultAsync(x =>
                x.Id == request.SkillGapAnalysisId &&
                x.UserId == userId);

        if (analysis == null)
        {
            return CreateFailureRoadmap("SKILL_GAP_ANALYSIS_NOT_FOUND", "Skill gap analysis not found");
        }

        var skillGaps = analysis.SkillGaps
            .Select(g => g.ToRoadmapDto())
            .ToList();

        var milestones = _milestoneGenerator.GenerateMilestones(skillGaps);

        return await CreateRoadmapWithMilestonesAsync(
            userId,
            milestones,
            request.SkillGapAnalysisId,
            analysis.JobDescription.TargetJobId);
    }

    public async Task<RoadmapDto?> GetRoadmapByIdAsync(long userId, long roadmapId)
    {
        var roadmap = await _context.LearningRoadmaps
            .Include(x => x.RoadmapProgress)
            .Include(x => x.RoadmapMilestones)
                .ThenInclude(x => x.LearningActivities)
                    .ThenInclude(x => x.Skill)
            .Include(x => x.TargetJob)
            .FirstOrDefaultAsync(x =>
                x.Id == roadmapId &&
                x.UserId == userId);

        return roadmap?.ToDto();
    }

    public async Task<List<RoadmapSummaryDto>> GetUserRoadmapsAsync(long userId)
    {
        var roadmaps = await _context.LearningRoadmaps
            .Include(x => x.RoadmapProgress)
            .Include(x => x.RoadmapMilestones)
                .ThenInclude(x => x.LearningActivities)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return roadmaps.Select(r => r.ToSummaryDto()).ToList();
    }

    public async Task<ActivityCompletionResultDto> CompleteActivityAsync(
        long userId,
        long activityId)
    {
        var (success, errorMessage, milestoneId, milestoneCompleted, milestoneProgress, roadmapProgress) =
            await _roadmapUnitOfWork.CompleteActivityAsync(activityId);

        if (!success)
        {
            return new ActivityCompletionResultDto
            {
                MilestoneId = milestoneId,
                IsMilestoneCompleted = false,
                MilestoneProgress = milestoneProgress,
                RoadmapProgress = roadmapProgress,
                RemainingActivities = 0,
                Success = false,
                Message = errorMessage ?? "Failed to complete activity"
            };
        }

        return new ActivityCompletionResultDto
        {
            MilestoneId = milestoneId,
            IsMilestoneCompleted = milestoneCompleted,
            MilestoneProgress = milestoneProgress,
            RoadmapProgress = roadmapProgress,
            RemainingActivities = 0,
            Success = true
        };
    }

    private async Task<RoadmapDto> CreateRoadmapWithMilestonesAsync(
        long userId,
        List<MilestoneDto> milestones,
        long? skillGapAnalysisId,
        long? targetJobId)
    {
        var roadmap = new LearningRoadmap
        {
            UserId = userId,
            SkillGapAnalysisId = skillGapAnalysisId,
            TargetJobId = targetJobId,
            RoadmapTitle = GenerateRoadmapTitle(skillGapAnalysisId, targetJobId),
            RoadmapStatus = RoadmapStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };

        var milestonesToSave = new List<RoadmapMilestone>();
        var activitiesToSave = new List<LearningActivity>();
        var geminiFailureCount = 0;

        foreach (var milestoneDto in milestones)
        {
            var milestone = new RoadmapMilestone
            {
                LearningRoadmapId = roadmap.Id,
                MilestoneTitle = milestoneDto.MilestoneTitle,
                MilestoneOrder = milestoneDto.MilestoneOrder,
                IsCompleted = false
            };

            milestonesToSave.Add(milestone);

            foreach (var activityDto in milestoneDto.Activities)
            {
                var activityTitle = activityDto.ActivityTitle;
                var activityDescription = activityDto.ActivityDescription;

                if (ShouldEnhanceWithGemini(activityDto))
                {
                    var geminiResult = await TryGenerateWithGeminiAsync(
                        milestoneDto.Metadata?.TargetSkill ?? "Unknown",
                        milestoneDto.Metadata?.DifficultyLevel ?? "BEGINNER");

                    if (geminiResult != null)
                    {
                        activityTitle = geminiResult.ActivityTitle;
                        activityDescription = geminiResult.ActivityDescription;
                        _logger.LogDebug(
                            "Generated activity for {Skill} using Gemini: {Title}",
                            milestoneDto.Metadata?.TargetSkill, activityTitle);
                    }
                    else
                    {
                        geminiFailureCount++;
                    }
                }

                var activity = new LearningActivity
                {
                    RoadmapMilestoneId = milestone.Id,
                    SkillId = await GetOrCreateSkillIdAsync(milestoneDto.Metadata?.TargetSkill),
                    ActivityTitle = activityTitle,
                    ActivityDescription = activityDescription,
                    ActivityType = ParseActivityType(activityDto.ActivityType),
                    IsCompleted = false
                };

                activitiesToSave.Add(activity);
            }
        }

        if (geminiFailureCount > 0)
        {
            _logger.LogWarning(
                "Gemini API failed for {Count} activities. Using default activities. Roadmap will be generated with fallback content.",
                geminiFailureCount);
        }

        var progress = new RoadmapProgress
        {
            CompletionPercentage = 0,
            LastUpdatedAt = DateTime.UtcNow
        };

        var (success, errorMessage, roadmapId) = await _roadmapUnitOfWork.SaveRoadmapWithDetailsAsync(
            roadmap, milestonesToSave, activitiesToSave, progress);

        if (!success)
        {
            return CreateFailureRoadmap("ROADMAP_SAVE_FAILED", $"Failed to save roadmap: {errorMessage}");
        }

        _logger.LogInformation(
            "Roadmap {RoadmapId} saved successfully with {MilestoneCount} milestones and {ActivityCount} activities",
            roadmapId, milestonesToSave.Count, activitiesToSave.Count);

        var createdRoadmap = await GetRoadmapByIdAsync(userId, roadmapId);
        if (createdRoadmap == null)
        {
            return CreateFailureRoadmap("ROADMAP_LOAD_FAILED", "Failed to retrieve created roadmap");
        }

        createdRoadmap.Success = true;
        createdRoadmap.IsAiFallback = geminiFailureCount > 0;
        if (geminiFailureCount > 0)
        {
            createdRoadmap.Message = "Roadmap generated with fallback activity content due to temporary AI issues.";
        }

        return createdRoadmap;
    }

    private async Task<ActivityDescriptionResponse?> TryGenerateWithGeminiAsync(
        string skillName,
        string difficultyLevel)
    {
        try
        {
            return await _activityDescriptionService.GenerateActivityDescriptionAsync(skillName, difficultyLevel);
        }
        catch (HttpRequestException ex)
        {
            LogGeminiFailure(ex, skillName, "HTTP error");
            return null;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            LogGeminiFailure(ex, skillName, "Timeout");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            LogGeminiFailure(ex, skillName, "Invalid response");
            return null;
        }
        catch (Exception ex)
        {
            LogGeminiFailure(ex, skillName, "Unknown error");
            return null;
        }
    }

    private void LogGeminiFailure(Exception ex, string skillName, string errorType)
    {
        var statusCode = GetHttpStatusCode(ex);

        if (statusCode.HasValue)
        {
            _logger.LogWarning(
                "Gemini API {ErrorType} for skill '{Skill}': Status={StatusCode}. Using default activity. Error: {Message}",
                errorType, skillName, statusCode, ex.Message);
        }
        else
        {
            _logger.LogWarning(
                "Gemini API {ErrorType} for skill '{Skill}'. Using default activity. Error: {Message}",
                errorType, skillName, ex.Message);
        }
    }

    private static int? GetHttpStatusCode(Exception ex)
    {
        if (ex is HttpRequestException httpEx && httpEx.StatusCode.HasValue)
        {
            return (int)httpEx.StatusCode.Value;
        }

        if (ex.InnerException is HttpRequestException innerHttpEx && innerHttpEx.StatusCode.HasValue)
        {
            return (int)innerHttpEx.StatusCode.Value;
        }

        return null;
    }

    private static bool ShouldEnhanceWithGemini(ActivityDto activity)
    {
        var defaultKeywords = new[] { "fundamentals", "practice", "project", "Study", "Build" };
        return defaultKeywords.Any(k =>
            activity.ActivityTitle.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<long> GetOrCreateSkillIdAsync(string? skillName)
    {
        if (string.IsNullOrWhiteSpace(skillName))
        {
            return 0;
        }

        var skill = await _context.Skills
            .FirstOrDefaultAsync(x =>
                x.SkillName.ToLower() == skillName.ToLower());

        if (skill == null)
        {
            skill = new Skill
            {
                SkillName = NormalizeSkillName(skillName),
                SkillType = "Technology"
            };

            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
        }

        return skill.Id;
    }

    private static string GenerateRoadmapTitle(long? skillGapAnalysisId, long? targetJobId)
    {
        return $"Learning Roadmap - {DateTime.UtcNow:MMM yyyy}";
    }

    private static string NormalizeSkillName(string skill)
    {
        var knownMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "c#", "C#" },
            { "c++", "C++" },
            { ".net", ".NET" },
            { "nodejs", "Node.js" },
            { "typescript", "TypeScript" },
            { "javascript", "JavaScript" },
            { "postgresql", "PostgreSQL" },
            { "mongodb", "MongoDB" },
            { "kubernetes", "Kubernetes" }
        };

        return knownMappings.TryGetValue(skill, out var normalized)
            ? normalized
            : skill;
    }

    private static Domain.Enum.ActivityType ParseActivityType(string? type)
    {
        return type?.ToUpperInvariant() switch
        {
            "READING" => Domain.Enum.ActivityType.READING,
            "PRACTICE" => Domain.Enum.ActivityType.PRACTICE,
            "MOCK_INTERVIEW" => Domain.Enum.ActivityType.MOCK_INTERVIEW,
            "QUIZ" => Domain.Enum.ActivityType.QUIZ,
            _ => Domain.Enum.ActivityType.OTHER
        };
    }

    private static RoadmapDto CreateFailureRoadmap(string errorCode, string message)
    {
        return new RoadmapDto
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            RoadmapStatus = "FAILED"
        };
    }
}
