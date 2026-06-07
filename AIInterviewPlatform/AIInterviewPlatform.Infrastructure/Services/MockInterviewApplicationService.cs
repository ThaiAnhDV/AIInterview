using AIInterviewPlatform.Application.DTOs.Interview.Enums;
using AIInterviewPlatform.Application.DTOs.Interview.Gemini;
using AIInterviewPlatform.Application.DTOs.Interview.Models;
using AIInterviewPlatform.Application.DTOs.Interview.Requests;
using AIInterviewPlatform.Application.DTOs.Interview.Responses;
using AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;
using AIInterviewPlatform.Application.DTOs.TargetJob;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class MockInterviewApplicationService : IMockInterviewApplicationService
{
    private readonly ITargetJobService _targetJobService;
    private readonly ISkillGapAnalysisService _skillGapAnalysisService;
    private readonly IInterviewQuestionGeneratorService _questionGenerator;
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly ILogger<MockInterviewApplicationService> _logger;

    public MockInterviewApplicationService(
        ITargetJobService targetJobService,
        ISkillGapAnalysisService skillGapAnalysisService,
        IInterviewQuestionGeneratorService questionGenerator,
        IInterviewSessionRepository sessionRepository,
        ILogger<MockInterviewApplicationService> logger)
    {
        _targetJobService = targetJobService;
        _skillGapAnalysisService = skillGapAnalysisService;
        _questionGenerator = questionGenerator;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<InterviewQuestionGenerationResult> StartMockInterviewAsync(
        long userId,
        StartMockInterviewRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting mock interview. UserId: {UserId}, TargetJobId: {TargetJobId}, SkillGapAnalysisId: {SkillGapAnalysisId}",
            userId, request.TargetJobId, request.SkillGapAnalysisId);

        // Validate ownership
        var hasAccess = await ValidateUserOwnsResourcesAsync(
            userId,
            request.TargetJobId,
            request.SkillGapAnalysisId,
            cancellationToken);

        if (!hasAccess)
        {
            _logger.LogWarning(
                "Access denied. UserId: {UserId}, TargetJobId: {TargetJobId}, SkillGapAnalysisId: {SkillGapAnalysisId}",
                userId, request.TargetJobId, request.SkillGapAnalysisId);
            return CreateUnauthorizedResult();
        }

        // Load Target Job
        var targetJob = await _targetJobService.GetTargetJobByIdAsync(
            userId,
            request.TargetJobId);

        // Load Skill Gap Analysis
        var skillGapAnalysis = await _skillGapAnalysisService.GetByIdAsync(
            userId,
            request.SkillGapAnalysisId);

        if (skillGapAnalysis == null)
        {
            _logger.LogWarning(
                "Skill Gap Analysis not found. UserId: {UserId}, SkillGapAnalysisId: {SkillGapAnalysisId}",
                userId, request.SkillGapAnalysisId);
            return CreateNotFoundResult("Skill Gap Analysis");
        }

        // Extract skills from SkillGapAnalysis
        var (requiredSkills, missingSkills) = ExtractSkillsFromAnalysis(skillGapAnalysis);

        _logger.LogInformation(
            "Skills loaded. MissingSkills count: {MissingSkillsCount}, RequiredSkills count: {RequiredSkillsCount}",
            missingSkills.Count, requiredSkills.Count);

        _logger.LogInformation(
            "[LANG_AUDIT] MockInterview=StartMockInterviewAsync LanguageCode={LanguageCode}",
            request.LanguageCode);

        // Generate Questions
        var result = await _questionGenerator.GenerateQuestionsFromJobAsync(
            targetJob.JobTitle,
            targetJob.JobTitle,
            requiredSkills,
            missingSkills,
            request.LanguageCode,
            cancellationToken);

        // Log fallback usage
        if (result.IsFallback)
        {
            _logger.LogWarning(
                "Using fallback interview questions. UserId: {UserId}, TargetJobId: {TargetJobId}, SkillGapAnalysisId: {SkillGapAnalysisId}",
                userId, request.TargetJobId, request.SkillGapAnalysisId);
        }

        // Create session with questions in database
        var questionsTuple = result.Questions
            .Select(q => (q.QuestionContent, q.Category, q.SkillArea))
            .ToList();

        var session = await _sessionRepository.CreateSessionWithQuestionsAsync(
            userId,
            request.TargetJobId,
            request.SkillGapAnalysisId,
            questionsTuple,
            result.IsFallback,
            cancellationToken);

        _logger.LogInformation(
            "Interview session created. SessionId: {SessionId}, Questions: {QuestionCount}",
            session.Id, session.InterviewQuestions.Count);

        // Map questions with their database IDs
        var questionsWithIds = MapQuestionsWithIds(result.Questions, session.InterviewQuestions);

        // Add resource IDs to result
        result = AddResourceIdsToResult(result, request.TargetJobId, request.SkillGapAnalysisId, session.Id, questionsWithIds);

        _logger.LogInformation(
            "Mock interview completed. SessionId: {SessionId}, Questions: {QuestionCount}, IsFallback: {IsFallback}",
            session.Id, result.Questions.Count, result.IsFallback);

        return result;
    }

    public async Task<bool> ValidateUserOwnsResourcesAsync(
        long userId,
        long targetJobId,
        long skillGapAnalysisId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetJob = await _targetJobService.GetTargetJobByIdAsync(userId, targetJobId);
            if (targetJob == null)
                return false;

            var skillGapAnalysis = await _skillGapAnalysisService.GetByIdAsync(userId, skillGapAnalysisId);
            if (skillGapAnalysis == null)
                return false;

            if (skillGapAnalysis.ResumeId <= 0)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static (List<string> RequiredSkills, List<string> MissingSkills) ExtractSkillsFromAnalysis(
        SkillGapAnalysisResponse skillGapAnalysis)
    {
        var missingSkills = skillGapAnalysis.MissingSkills
            .Select(s => s.SkillName)
            .ToList();

        var requiredSkills = skillGapAnalysis.MatchedSkills
            .Concat(missingSkills)
            .ToList();

        return (requiredSkills, missingSkills);
    }

    private static List<QuestionDto> MapQuestionsWithIds(
        List<QuestionDto> generatedQuestions,
        ICollection<Domain.Enities.InterviewQuestion> savedQuestions)
    {
        var result = new List<QuestionDto>();
        var savedQuestionsList = savedQuestions.ToList();

        for (int i = 0; i < generatedQuestions.Count && i < savedQuestionsList.Count; i++)
        {
            var generated = generatedQuestions[i];
            var saved = savedQuestionsList[i];

            result.Add(new QuestionDto
            {
                QuestionId = saved.Id,
                QuestionGuid = generated.QuestionGuid,
                QuestionContent = generated.QuestionContent,
                QuestionType = generated.QuestionType,
                Difficulty = generated.Difficulty,
                Category = generated.Category,
                SkillArea = generated.SkillArea,
                ExpectedAnswerFramework = generated.ExpectedAnswerFramework,
                SampleAnswer = generated.SampleAnswer,
                SuggestedTimeMinutes = generated.SuggestedTimeMinutes,
                FollowUpQuestions = generated.FollowUpQuestions,
                EvaluationCriteria = generated.EvaluationCriteria,
                Metadata = generated.Metadata
            });
        }

        return result;
    }

    private static InterviewQuestionGenerationResult AddResourceIdsToResult(
        InterviewQuestionGenerationResult result,
        long targetJobId,
        long skillGapAnalysisId,
        long sessionId,
        List<QuestionDto> questionsWithIds)
    {
        return new InterviewQuestionGenerationResult
        {
            GenerationId = result.GenerationId,
            SessionId = sessionId,
            TargetJob = new TargetJobSummary
            {
                JobId = targetJobId,
                Title = result.TargetJob.Title,
                Company = result.TargetJob.Company
            },
            Summary = result.Summary,
            Questions = questionsWithIds,
            Metadata = new GenerationMetadata
            {
                GeneratedAt = result.Metadata.GeneratedAt,
                AiModelUsed = result.Metadata.AiModelUsed,
                PromptTokens = result.Metadata.PromptTokens,
                CompletionTokens = result.Metadata.CompletionTokens,
                GenerationVersion = result.Metadata.GenerationVersion,
                CustomMetadata = new Dictionary<string, object>
                {
                    ["SkillGapAnalysisId"] = skillGapAnalysisId
                }
            },
            Status = result.Status,
            ErrorMessage = result.ErrorMessage,
            IsFallback = result.IsFallback
        };
    }

    private static InterviewQuestionGenerationResult CreateUnauthorizedResult()
    {
        return new InterviewQuestionGenerationResult
        {
            TargetJob = new TargetJobSummary { Title = "Unauthorized" },
            Summary = new GenerationSummary
            {
                TotalQuestionsGenerated = 0,
                ByType = new QuestionCountByType()
            },
            Questions = [],
            Status = GenerationStatusEnum.Failed,
            IsFallback = true
        };
    }

    private static InterviewQuestionGenerationResult CreateNotFoundResult(string resourceName)
    {
        return new InterviewQuestionGenerationResult
        {
            TargetJob = new TargetJobSummary { Title = "Not Found" },
            Summary = new GenerationSummary
            {
                TotalQuestionsGenerated = 0,
                ByType = new QuestionCountByType()
            },
            Questions = [],
            Status = GenerationStatusEnum.Failed,
            ErrorMessage = $"{resourceName} not found",
            IsFallback = true
        };
    }
}
