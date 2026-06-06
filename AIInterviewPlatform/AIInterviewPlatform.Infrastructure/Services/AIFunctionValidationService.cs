using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;
using AIInterviewPlatform.Application.DTOs.Interview.Gemini;
using AIInterviewPlatform.Application.DTOs.Interview.Responses;
using AIInterviewPlatform.Application.DTOs.Roadmap.Requests;
using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class AIFunctionValidationService : IAIFunctionValidationService
{
    private readonly ISkillExtractionService _skillExtractionService;
    private readonly IJobDescriptionSkillExtractionService _jobDescriptionExtractionService;
    private readonly IInterviewQuestionGeneratorService _questionGeneratorService;
    private readonly IInterviewEvaluationService _interviewEvaluationService;
    private readonly IMilestoneGeneratorService _milestoneGeneratorService;
    private readonly ILogger<AIFunctionValidationService> _logger;

    public AIFunctionValidationService(
        ISkillExtractionService skillExtractionService,
        IJobDescriptionSkillExtractionService jobDescriptionExtractionService,
        IInterviewQuestionGeneratorService questionGeneratorService,
        IInterviewEvaluationService interviewEvaluationService,
        IMilestoneGeneratorService milestoneGeneratorService,
        ILogger<AIFunctionValidationService> logger)
    {
        _skillExtractionService = skillExtractionService;
        _jobDescriptionExtractionService = jobDescriptionExtractionService;
        _questionGeneratorService = questionGeneratorService;
        _interviewEvaluationService = interviewEvaluationService;
        _milestoneGeneratorService = milestoneGeneratorService;
        _logger = logger;
    }

    public async Task<AIFunctionValidationResponse> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var response = new AIFunctionValidationResponse
        {
            Timestamp = DateTime.UtcNow
        };

        response.SkillExtraction = await ValidateSkillExtractionAsync();
        response.JDExtraction = await ValidateJobDescriptionExtractionAsync();
        response.QuestionGeneration = await ValidateQuestionGenerationAsync(cancellationToken);
        response.Evaluation = await ValidateEvaluationAsync(cancellationToken);
        response.RoadmapGeneration = ValidateRoadmapGeneration();

        response.Success = response.SkillExtraction
            && response.JDExtraction
            && response.QuestionGeneration
            && response.Evaluation
            && response.RoadmapGeneration;

        response.OverallStatus = response.Success ? "HEALTHY" : "PARTIAL_FAILURE";
        return response;
    }

    private async Task<bool> ValidateSkillExtractionAsync()
    {
        try
        {
            var result = await _skillExtractionService.ExtractSkillsFromResumeAsync(
                "Experienced C# Developer with SQL and ASP.NET Core");

            var passed = result.Success
                && result.Data != null
                && result.Data.Skills.Count > 0;

            LogModuleResult("Skill Extraction", passed, result.Error?.ErrorCode);
            return passed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI functional validation failed for Skill Extraction");
            return false;
        }
    }

    private async Task<bool> ValidateJobDescriptionExtractionAsync()
    {
        try
        {
            var result = await _jobDescriptionExtractionService.ExtractRequiredSkillsAsync(
                "We need a Backend Developer with C#, SQL and REST APIs.");

            var passed = result.Success
                && result.Data != null
                && result.Data.RequiredSkills.Count > 0;

            LogModuleResult("JD Extraction", passed, result.Error?.ErrorCode);
            return passed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI functional validation failed for JD Extraction");
            return false;
        }
    }

    private async Task<bool> ValidateQuestionGenerationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = new GeminiInterviewQuestionRequest
            {
                TargetJob = new TargetJobInfo
                {
                    Title = "Backend Developer",
                    Description = "Backend Developer role focused on APIs and data access"
                },
                RequiredSkills = new List<string> { "C#", "SQL", "REST APIs" },
                MissingSkills = new List<string>()
            };

            var result = await _questionGeneratorService.GenerateQuestionsAsync(request, cancellationToken);

            var passed = result.Status != Application.DTOs.Interview.Enums.GenerationStatusEnum.Failed
                && result.Questions.Count > 0;

            LogModuleResult("Question Generation", passed, result.ErrorMessage);
            return passed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI functional validation failed for Question Generation");
            return false;
        }
    }

    private async Task<bool> ValidateEvaluationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = new EvaluationRequestDto
            {
                Question = "Tell me about your SQL experience",
                Answer = "I used SQL Server and Entity Framework in multiple projects.",
                Category = "Backend",
                SkillFocus = "SQL"
            };

            var result = await _interviewEvaluationService.EvaluateAnswerAsync(request, cancellationToken);

            var passed = result.Success
                && result.Overall >= 0
                && !string.IsNullOrWhiteSpace(result.Feedback);

            LogModuleResult("Evaluation", passed, result.ErrorCode ?? result.Message);
            return passed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI functional validation failed for Evaluation");
            return false;
        }
    }

    private bool ValidateRoadmapGeneration()
    {
        try
        {
            var request = new GenerateRoadmapFromMissingSkillsRequest
            {
                MissingSkills = new List<SkillGapForRoadmapDto>
                {
                    new()
                    {
                        SkillId = 0,
                        SkillName = "Docker",
                        SkillType = "Technology",
                        GapLevel = "Intermediate"
                    },
                    new()
                    {
                        SkillId = 0,
                        SkillName = "System Design",
                        SkillType = "Knowledge",
                        GapLevel = "Intermediate"
                    }
                },
                MilestonesPerSkill = 1,
                ActivitiesPerMilestone = 1
            };

            var milestones = _milestoneGeneratorService.GenerateMilestones(request.MissingSkills);
            var passed = milestones.Count > 0 && milestones.Any(m => m.Activities.Count > 0);

            LogModuleResult("Roadmap Generation", passed, null);
            return passed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI functional validation failed for Roadmap Generation");
            return false;
        }
    }

    private void LogModuleResult(string module, bool passed, string? detail)
    {
        if (passed)
        {
            _logger.LogInformation("AI functional validation passed for {Module}", module);
            return;
        }

        _logger.LogWarning("AI functional validation failed for {Module}. Detail: {Detail}", module, detail ?? "n/a");
    }
}
