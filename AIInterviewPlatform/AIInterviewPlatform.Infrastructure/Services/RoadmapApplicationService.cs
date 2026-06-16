using AIInterviewPlatform.Application.DTOs.Recommendation;
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
            userId, request.MissingSkills?.Count ?? 0);

        if (request.MissingSkills == null || request.MissingSkills.Count == 0)
        {
            return CreateFailureRoadmap("INVALID_REQUEST", "Missing skills list cannot be empty");
        }

        var recommendations = await GetOrCreateRecommendationsAsync(userId, request.MissingSkills, null);

        var milestones = _milestoneGenerator.GenerateMilestones(
            request.MissingSkills.Select(s => new SkillGapForRoadmapDto
            {
                SkillId = s.SkillId,
                SkillName = s.SkillName,
                GapDescription = s.GapDescription,
                GapLevel = s.GapLevel
            }).ToList());

        var roadmap = await CreateRoadmapWithMilestonesAsync(
            userId,
            milestones,
            null,
            null,
            request.LanguageCode,
            recommendations);

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
            "[ROADMAP_INTEGRATION] Generate roadmap from analysis UserId={UserId} AnalysisId={AnalysisId}",
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

        var existingRoadmap = await _roadmapUnitOfWork.Roadmaps.GetActiveByAnalysisIdAsync(userId, request.SkillGapAnalysisId);
        if (existingRoadmap != null)
        {
            _logger.LogInformation(
                "[ROADMAP_INTEGRATION] Returning existing roadmap {RoadmapId} for AnalysisId={AnalysisId}",
                existingRoadmap.Id, request.SkillGapAnalysisId);

            return existingRoadmap.ToDto();
        }

        var skillGaps = analysis.SkillGaps
            .Select(g => g.ToRoadmapDto())
            .ToList();

        var recommendations = await GetOrCreateRecommendationsAsync(userId, null, analysis.SkillGaps.ToList());

        var milestones = _milestoneGenerator.GenerateMilestones(skillGaps);

        return await CreateRoadmapWithMilestonesAsync(
            userId,
            milestones,
            request.SkillGapAnalysisId,
            analysis.JobDescription.TargetJobId,
            request.LanguageCode,
            recommendations);
    }

    private async Task<List<Recommendation>> GetOrCreateRecommendationsAsync(
        long userId,
        List<SkillGapForRoadmapDto>? directSkills,
        List<SkillGap>? analysisSkillGaps)
    {
        var skillIds = new List<long>();
        var skillNames = new List<string>();

        if (directSkills != null)
        {
            skillIds = directSkills.Select(s => s.SkillId).Where(id => id > 0).ToList();
            skillNames = directSkills.Select(s => s.SkillName).ToList();
        }
        else if (analysisSkillGaps != null)
        {
            skillIds = analysisSkillGaps.Select(g => g.SkillId).ToList();
            skillNames = analysisSkillGaps.Select(g => g.Skill?.SkillName ?? "").ToList();
        }

        var recommendations = await _context.Recommendations
            .Include(r => r.Skill)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        var missingSkillIds = skillIds.Where(id => id > 0 && !recommendations.Any(r => r.SkillId == id)).ToList();
        var missingSkillNames = skillNames.Where(n => !string.IsNullOrEmpty(n) && !recommendations.Any(r => r.Skill?.SkillName == n)).ToList();

        if (missingSkillIds.Any() || missingSkillNames.Any())
        {
            _logger.LogWarning(
                "[ROADMAP_INTEGRATION] {Count} skills have no recommendations. Roadmap will be generated without full recommendation linking.",
                missingSkillIds.Count + missingSkillNames.Count);
        }

        return recommendations;
    }

    public async Task<RoadmapDto?> GetRoadmapByIdAsync(long userId, long roadmapId)
    {
        var roadmap = await _context.LearningRoadmaps
            .Include(x => x.RoadmapProgress)
            .Include(x => x.RoadmapMilestones)
                .ThenInclude(x => x.LearningActivities)
                    .ThenInclude(x => x.Skill)
            .Include(x => x.RoadmapRecommendations)
                .ThenInclude(rr => rr.Recommendation)
                    .ThenInclude(r => r.Skill)
            .Include(x => x.TargetJob)
            .Include(x => x.SkillGapAnalysis)
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
            .Include(x => x.RoadmapRecommendations)
                .ThenInclude(rr => rr.Recommendation)
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
        long? targetJobId,
        string? languageCode,
        List<Recommendation>? recommendations = null)
    {
        var roadmap = new LearningRoadmap
        {
            UserId = userId,
            SkillGapAnalysisId = skillGapAnalysisId,
            TargetJobId = targetJobId,
            RoadmapTitle = GenerateRoadmapTitle(skillGapAnalysisId, targetJobId),
            RoadmapStatus = RoadmapStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow,
            LanguageCode = languageCode
        };

        var milestonesToSave = new List<RoadmapMilestone>();
        var activitiesToSave = new List<LearningActivity>();
        var geminiFailureCount = 0;
        var startDate = DateTime.UtcNow;

        for (int i = 0; i < milestones.Count; i++)
        {
            var milestoneDto = milestones[i];
            var milestoneStartDate = startDate.AddDays(i > 0 ? milestones.Take(i).Sum(m => m.Metadata?.EstimatedDays ?? 7) : 0);

            var milestone = new RoadmapMilestone
            {
                LearningRoadmapId = roadmap.Id,
                MilestoneTitle = milestoneDto.MilestoneTitle,
                MilestoneOrder = milestoneDto.MilestoneOrder,
                IsCompleted = false,
                EstimatedDays = milestoneDto.Metadata?.EstimatedDays ?? 7,
                StartDate = milestoneStartDate,
                EndDate = milestoneStartDate.AddDays(milestoneDto.Metadata?.EstimatedDays ?? 7)
            };

            milestonesToSave.Add(milestone);

            foreach (var activityDto in milestoneDto.Activities)
            {
                activityDto.SourceMilestoneOrder ??= milestoneDto.MilestoneOrder;

                var activityTitle = activityDto.ActivityTitle;
                var activityDescription = activityDto.ActivityDescription;
                var activityType = ParseActivityType(activityDto.ActivityType);
                var activitySource = "Template";

                if (ShouldEnhanceWithGemini(activityDto))
                {
                    var geminiResult = await TryGenerateWithGeminiAsync(
                        milestoneDto.Metadata?.TargetSkill ?? "Unknown",
                        milestoneDto.Metadata?.DifficultyLevel ?? "BEGINNER",
                        languageCode);

                    if (geminiResult != null)
                    {
                        activityTitle = geminiResult.ActivityTitle;
                        activityDescription = geminiResult.ActivityDescription;
                        activityType = ParseActivityType(geminiResult.ActivityType);
                        activitySource = "Gemini";
                    }
                    else
                    {
                        geminiFailureCount++;
                        activitySource = "Fallback";
                    }
                }

                var activity = new LearningActivity
                {
                    RoadmapMilestoneId = milestone.Id,
                    SkillId = await GetOrCreateSkillIdAsync(milestoneDto.Metadata?.TargetSkill),
                    ActivityTitle = activityTitle,
                    ActivityDescription = activityDescription,
                    ActivityType = activityType,
                    IsCompleted = false
                };

                activity.RoadmapMilestone = milestone;
                activitiesToSave.Add(activity);
            }
        }

        var progress = new RoadmapProgress
        {
            CompletionPercentage = 0,
            LastUpdatedAt = DateTime.UtcNow
        };

        var roadmapRecommendations = recommendations?.Select(r => new RoadmapRecommendation
        {
            LearningRoadmapId = roadmap.Id,
            RecommendationId = r.Id
        }).ToList();

        var (success, errorMessage, roadmapId) = await _roadmapUnitOfWork.SaveRoadmapWithDetailsAsync(
            roadmap, milestonesToSave, activitiesToSave, progress, roadmapRecommendations);

        if (!success)
        {
            return CreateFailureRoadmap("ROADMAP_SAVE_FAILED", $"Failed to save roadmap: {errorMessage}");
        }

        _logger.LogInformation(
            "[ROADMAP_INTEGRATION] Roadmap {RoadmapId} saved with {MilestoneCount} milestones, {ActivityCount} activities, {RecommendationCount} recommendation links",
            roadmapId, milestonesToSave.Count, activitiesToSave.Count, roadmapRecommendations?.Count ?? 0);

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
        string difficultyLevel,
        string? languageCode)
    {
        try
        {
            return await _activityDescriptionService.GenerateActivityDescriptionAsync(skillName, difficultyLevel, languageCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[ROADMAP_INTEGRATION] Gemini API failed for skill '{Skill}'", skillName);
            return null;
        }
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
