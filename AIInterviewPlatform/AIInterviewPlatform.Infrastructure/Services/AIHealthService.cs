using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.Interview.Gemini;
using AIInterviewPlatform.Application.DTOs.Roadmap.Requests;
using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AIInterviewPlatform.Infrastructure.Services;

public class AIHealthService : IAIHealthService
{
    private readonly HttpClient _geminiHttpClient;
    private readonly string _apiKey;
    private readonly ILogger<AIHealthService> _logger;
    private readonly ISkillExtractionService _skillExtractionService;
    private readonly IJobDescriptionSkillExtractionService _jdExtractionService;
    private readonly IInterviewQuestionGeneratorService _questionGeneratorService;
    private readonly IAnswerEvaluationService _answerEvaluationService;
    private readonly IRoadmapApplicationService _roadmapService;

    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public AIHealthService(
        HttpClient geminiHttpClient,
        IConfiguration configuration,
        ILogger<AIHealthService> logger,
        ISkillExtractionService skillExtractionService,
        IJobDescriptionSkillExtractionService jdExtractionService,
        IInterviewQuestionGeneratorService questionGeneratorService,
        IAnswerEvaluationService answerEvaluationService,
        IRoadmapApplicationService roadmapService)
    {
        _geminiHttpClient = geminiHttpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
        _skillExtractionService = skillExtractionService;
        _jdExtractionService = jdExtractionService;
        _questionGeneratorService = questionGeneratorService;
        _answerEvaluationService = answerEvaluationService;
        _roadmapService = roadmapService;
    }

    public async Task<AIHealthResponse> CheckHealthAsync()
    {
        var response = new AIHealthResponse
        {
            Timestamp = DateTime.UtcNow,
            Model = "gemini-2.5-flash"
        };

        _logger.LogInformation("[AI HEALTH] Starting health check...");

        var geminiTask = CheckGeminiConnectivityAsync();
        var skillExtractionTask = CheckSkillExtractionAsync();
        var jdExtractionTask = CheckJDExtractionAsync();
        var questionGenTask = CheckQuestionGenerationAsync();
        var evaluationTask = CheckEvaluationAsync();
        var roadmapTask = CheckRoadmapGenerationAsync();

        await Task.WhenAll(geminiTask, skillExtractionTask, jdExtractionTask,
            questionGenTask, evaluationTask, roadmapTask);

        response.Gemini = await geminiTask;
        response.SkillExtraction = await skillExtractionTask;
        response.JDExtraction = await jdExtractionTask;
        response.QuestionGeneration = await questionGenTask;
        response.Evaluation = await evaluationTask;
        response.RoadmapGeneration = await roadmapTask;

        LogHealthResults(response);

        return response;
    }

    private async Task<bool> CheckGeminiConnectivityAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("[AI HEALTH] Gemini: FAILED (API key not configured)");
                return false;
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = "Reply only with OK" } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1,
                    maxOutputTokens = 10
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _geminiHttpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                if (responseBody.Contains("OK", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[AI HEALTH] Gemini: OK");
                    return true;
                }
            }

            _logger.LogWarning("[AI HEALTH] Gemini: FAILED (unexpected response)");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[AI HEALTH] Gemini: FAILED");
            return false;
        }
    }

    private async Task<bool> CheckSkillExtractionAsync()
    {
        var testResume = "Experienced C# Developer with SQL and ASP.NET Core";
        var result = await _skillExtractionService.ExtractSkillsFromResumeAsync(testResume);

        if (result.Success && result.Data != null && result.Data.Skills.Count > 0)
        {
            _logger.LogInformation("[AI HEALTH] Skill Extraction: OK (extracted {Count} skills)", result.Data.Skills.Count);
            return true;
        }

        _logger.LogWarning("[AI HEALTH] Skill Extraction: FAILED");
        return false;
    }

    private async Task<bool> CheckJDExtractionAsync()
    {
        var testJD = "We are looking for a Backend Developer with C#, SQL and REST APIs.";
        var result = await _jdExtractionService.ExtractRequiredSkillsAsync(testJD);

        if (result.Success && result.Data != null && result.Data.RequiredSkills.Count > 0)
        {
            _logger.LogInformation("[AI HEALTH] JD Extraction: OK (extracted {Count} skills)", result.Data.RequiredSkills.Count);
            return true;
        }

        _logger.LogWarning("[AI HEALTH] JD Extraction: FAILED");
        return false;
    }

    private async Task<bool> CheckQuestionGenerationAsync()
    {
        try
        {
            var request = new GeminiInterviewQuestionRequest
            {
                TargetJob = new TargetJobInfo { Title = "Backend Developer" },
                RequiredSkills = new List<string> { "C#", "SQL" },
                MissingSkills = new List<string>()
            };

            var result = await _questionGeneratorService.GenerateQuestionsAsync(request);

            if (result != null && result.Questions.Count > 0)
            {
                _logger.LogInformation("[AI HEALTH] Question Generation: OK (generated {Count} questions)", result.Questions.Count);
                return true;
            }

            _logger.LogWarning("[AI HEALTH] Question Generation: FAILED (no questions generated)");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[AI HEALTH] Question Generation: FAILED");
            return false;
        }
    }

    private async Task<bool> CheckEvaluationAsync()
    {
        var result = await _answerEvaluationService.EvaluateAnswerAsync(0);
        var healthy = result.Success || result.ErrorCode == "ANSWER_NOT_FOUND";
        _logger.LogInformation("[AI HEALTH] Evaluation: {Status}", healthy ? "OK" : "FAILED");
        return healthy;
    }

    private async Task<bool> CheckRoadmapGenerationAsync()
    {
        var testRequest = new GenerateRoadmapFromMissingSkillsRequest
        {
            MissingSkills = new List<SkillGapForRoadmapDto>
            {
                new SkillGapForRoadmapDto
                {
                    SkillId = 0,
                    SkillName = "Docker",
                    SkillType = "Technology",
                    GapLevel = "Intermediate"
                },
                new SkillGapForRoadmapDto
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

        var result = await _roadmapService.GenerateRoadmapFromMissingSkillsAsync(
            userId: 0,
            request: testRequest);

        if (result.Success && result.Milestones.Count > 0)
        {
            _logger.LogInformation("[AI HEALTH] Roadmap Generation: OK (generated {Count} milestones)", result.Milestones.Count);
            return true;
        }

        _logger.LogWarning("[AI HEALTH] Roadmap Generation: FAILED");
        return false;
    }

    private void LogHealthResults(AIHealthResponse response)
    {
        _logger.LogInformation("[AI HEALTH] ========== HEALTH CHECK RESULTS ==========");
        _logger.LogInformation("[AI HEALTH] Gemini: {GeminiStatus}", response.Gemini ? "OK" : "FAILED");
        _logger.LogInformation("[AI HEALTH] Model: {Model}", response.Model);
        _logger.LogInformation("[AI HEALTH] Skill Extraction: {Status}", response.SkillExtraction ? "OK" : "FAILED");
        _logger.LogInformation("[AI HEALTH] JD Extraction: {Status}", response.JDExtraction ? "OK" : "FAILED");
        _logger.LogInformation("[AI HEALTH] Question Generation: {Status}", response.QuestionGeneration ? "OK" : "FAILED");
        _logger.LogInformation("[AI HEALTH] Evaluation: {Status}", response.Evaluation ? "OK" : "FAILED");
        _logger.LogInformation("[AI HEALTH] Roadmap Generation: {Status}", response.RoadmapGeneration ? "OK" : "FAILED");
        _logger.LogInformation("[AI HEALTH] Timestamp: {Timestamp}", response.Timestamp.ToString("O"));
        _logger.LogInformation("[AI HEALTH] ===========================================");
    }
}
