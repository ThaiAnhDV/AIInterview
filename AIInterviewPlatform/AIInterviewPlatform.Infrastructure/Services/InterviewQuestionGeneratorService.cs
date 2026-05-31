using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.Interview.Enums;
using AIInterviewPlatform.Application.DTOs.Interview.Gemini;
using AIInterviewPlatform.Application.DTOs.Interview.Models;
using AIInterviewPlatform.Application.DTOs.Interview.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class InterviewQuestionGeneratorService : IInterviewQuestionGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<InterviewQuestionGeneratorService> _logger;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public InterviewQuestionGeneratorService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<InterviewQuestionGeneratorService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured - will use default questions");
        }
    }

    public async Task<InterviewQuestionGenerationResult> GenerateQuestionsAsync(
        GeminiInterviewQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateRequest(request);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request - returning empty result");
            return CreateEmptyResult(request);
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Gemini API key not configured - using fallback questions");
            return GetDefaultQuestions(request);
        }

        var prompt = BuildGeminiPrompt(request);
        var responseJson = await SendGeminiRequestAsync(prompt, cancellationToken);

        if (responseJson == null)
        {
            _logger.LogWarning("Gemini unavailable. Using fallback interview questions.");
            return GetDefaultQuestions(request);
        }

        var geminiResponse = ParseGeminiResponse(responseJson);

        if (geminiResponse == null || geminiResponse.Questions.Count == 0)
        {
            _logger.LogWarning("Failed to parse Gemini response. Using fallback questions.");
            return GetDefaultQuestions(request);
        }

        return ConvertToResult(request, geminiResponse, isFallback: false);
    }

    public async Task<InterviewQuestionGenerationResult> GenerateQuestionsFromJobAsync(
        string targetJobTitle,
        string targetJobDescription,
        List<string> requiredSkills,
        List<string> missingSkills,
        CancellationToken cancellationToken = default)
    {
        var request = new GeminiInterviewQuestionRequest
        {
            TargetJob = new TargetJobInfo
            {
                Title = targetJobTitle,
                Description = targetJobDescription
            },
            RequiredSkills = requiredSkills,
            MissingSkills = missingSkills
        };

        return await GenerateQuestionsAsync(request, cancellationToken);
    }

    private static void ValidateRequest(GeminiInterviewQuestionRequest request)
    {
        if (request.TargetJob == null)
            throw new ArgumentException("Target job information is required");

        if (string.IsNullOrWhiteSpace(request.TargetJob.Title))
            throw new ArgumentException("Target job title is required");

        if (request.RequiredSkills == null || request.RequiredSkills.Count == 0)
            throw new ArgumentException("At least one required skill is needed");
    }

    private InterviewQuestionGenerationResult CreateEmptyResult(GeminiInterviewQuestionRequest request)
    {
        return new InterviewQuestionGenerationResult
        {
            TargetJob = new TargetJobSummary
            {
                Title = request.TargetJob?.Title ?? "Unknown",
                Company = request.TargetJob?.Company
            },
            Summary = new GenerationSummary
            {
                TotalQuestionsGenerated = 0,
                ByType = new QuestionCountByType()
            },
            Questions = [],
            Status = GenerationStatusEnum.Completed,
            IsFallback = true
        };
    }

    private string BuildGeminiPrompt(GeminiInterviewQuestionRequest request)
    {
        var missingSkillsText = request.MissingSkills.Count > 0 
            ? string.Join(", ", request.MissingSkills) 
            : string.Join(", ", request.RequiredSkills);

        var prompt = "Generate 10 interview questions for a candidate applying for:\n" +
            "Position: " + request.TargetJob.Title + "\n" +
            "Description: " + request.TargetJob.Description + "\n\n" +
            "Focus areas (prioritized): " + missingSkillsText + "\n\n" +
            "Return ONLY valid JSON - no markdown, no explanation:\n" +
            @"{""questions"":[
  {""question"":"""",""category"":"""",""skillFocus"":""""}
]}\n\n" +
            "Categories: Technical, Behavioral, Communication\n" +
            "Rules:\n" +
            "- Prioritize missing skills\n" +
            "- Match job requirements\n" +
            "- JSON only\n" +
            "- No additional fields";

        return prompt;
    }

    private async Task<string?> SendGeminiRequestAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 8192,
                responseMimeType = "application/json"
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _logger.LogDebug("Sending request to Gemini API for interview questions");

        try
        {
            var response = await _httpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Gemini returned {StatusCode}. Using fallback questions.",
                    response.StatusCode);
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseBody;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error calling Gemini API. Using fallback questions.");
            return null;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            _logger.LogWarning(ex, "Gemini API request timed out. Using fallback questions.");
            return null;
        }
    }

    private GeminiInterviewQuestionResponse? ParseGeminiResponse(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!TryExtractTextFromResponse(root, out var jsonText))
            {
                _logger.LogWarning("Unexpected Gemini response format - no candidates found");
                return null;
            }

            var cleanedText = CleanJsonResponse(jsonText);

            try
            {
                var response = JsonSerializer.Deserialize<GeminiInterviewQuestionResponse>(cleanedText, JsonOptions);
                if (response == null || response.Questions.Count == 0)
                {
                    var manualResult = TryExtractQuestionsManually(cleanedText);
                    if (manualResult.Questions.Count == 0)
                    {
                        _logger.LogWarning("No questions found in Gemini response");
                        return null;
                    }
                    return manualResult;
                }
                return response;
            }
            catch (JsonException)
            {
                var result = TryExtractQuestionsManually(cleanedText);
                if (result.Questions.Count == 0)
                {
                    _logger.LogWarning("Failed to parse Gemini response JSON");
                    return null;
                }
                return result;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON in Gemini response");
            return null;
        }
    }

    private static bool TryExtractTextFromResponse(JsonElement root, out string text)
    {
        text = string.Empty;

        if (root.TryGetProperty("candidates", out var candidates) &&
            candidates.ValueKind == JsonValueKind.Array &&
            candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.ValueKind == JsonValueKind.Array &&
                parts.GetArrayLength() > 0)
            {
                text = parts[0].GetProperty("text").GetString() ?? string.Empty;
                return true;
            }
        }

        return false;
    }

    private static string CleanJsonResponse(string jsonText)
    {
        var cleaned = jsonText.Trim();
        
        if (cleaned.StartsWith("```json"))
            cleaned = cleaned[7..];
        else if (cleaned.StartsWith("```"))
            cleaned = cleaned[3..];

        if (cleaned.EndsWith("```"))
            cleaned = cleaned[..^3];

        return cleaned.Trim();
    }

    private GeminiInterviewQuestionResponse TryExtractQuestionsManually(string text)
    {
        var result = new GeminiInterviewQuestionResponse();

        if (TryExtractArray(text, "questions", out var questionsArray))
        {
            result.Questions = ParseQuestions(questionsArray);
        }

        return result;
    }

    private static bool TryExtractArray(string text, string arrayName, out string arrayContent)
    {
        arrayContent = string.Empty;
        
        var pattern = $@"""{arrayName}"":\s*\[(.*?)\](?=\s*,?\s*""[a-zA-Z]+Questions""|\s*,?\s*\}})";
        var match = System.Text.RegularExpressions.Regex.Match(text, pattern, 
            System.Text.RegularExpressions.RegexOptions.Singleline);

        if (match.Success)
        {
            arrayContent = match.Groups[1].Value;
            return true;
        }

        return false;
    }

    private static List<GeminiQuestion> ParseQuestions(string arrayContent)
    {
        var questions = new List<GeminiQuestion>();
        
        var questionMatches = System.Text.RegularExpressions.Regex.Matches(
            arrayContent, 
            @"\{([^}]+)\}");

        foreach (System.Text.RegularExpressions.Match match in questionMatches)
        {
            var questionBlock = match.Groups[1].Value;
            
            var question = new GeminiQuestion
            {
                Question = ExtractValue(questionBlock, "question") ?? string.Empty,
                Category = ExtractValue(questionBlock, "category"),
                SkillFocus = ExtractValue(questionBlock, "skillFocus") 
            };

            if (!string.IsNullOrWhiteSpace(question.Question))
            {
                questions.Add(question);
            }
        }

        return questions;
    }

    private static string? ExtractValue(string block, string propertyName)
    {
        var pattern = $@"""{propertyName}"":\s*""([^""]*)""";
        var match = System.Text.RegularExpressions.Regex.Match(block, pattern);
        return match.Success ? match.Groups[1].Value : null;
    }

    private InterviewQuestionGenerationResult ConvertToResult(
        GeminiInterviewQuestionRequest request,
        GeminiInterviewQuestionResponse geminiResponse,
        bool isFallback)
    {
        var questions = new List<QuestionDto>();

        foreach (var q in geminiResponse.Questions)
        {
            var questionType = MapCategoryToQuestionType(q.Category);
            questions.Add(CreateQuestionDto(q, questionType));
        }

        var techCount = questions.Count(q => q.QuestionType == InterviewEnums.QuestionType.Technical);
        var behavCount = questions.Count(q => q.QuestionType == InterviewEnums.QuestionType.Behavioral);
        var commCount = questions.Count(q => q.QuestionType == InterviewEnums.QuestionType.Communication);

        var summary = new GenerationSummary
        {
            TotalQuestionsGenerated = questions.Count,
            ByType = new QuestionCountByType
            {
                Technical = techCount,
                Behavioral = behavCount,
                Communication = commCount
            },
            SkillsCovered = request.RequiredSkills,
            SkillsToFocus = request.MissingSkills,
            EstimatedDuration = TimeSpan.FromMinutes(questions.Count * 5)
        };

        return new InterviewQuestionGenerationResult
        {
            TargetJob = new TargetJobSummary
            {
                Title = request.TargetJob.Title,
                Company = request.TargetJob.Company
            },
            Summary = summary,
            Questions = questions,
            Status = GenerationStatusEnum.Completed,
            IsFallback = isFallback,
            Metadata = new GenerationMetadata
            {
                AiModelUsed = isFallback ? "fallback" : "gemini-2.0-flash",
                GenerationVersion = "1.0"
            }
        };
    }

    private static string MapCategoryToQuestionType(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return InterviewEnums.QuestionType.Technical;

        var lowerCategory = category.ToUpperInvariant();
        
        if (lowerCategory.Contains("TECHNICAL"))
            return InterviewEnums.QuestionType.Technical;
        if (lowerCategory.Contains("BEHAVIORAL") || lowerCategory.Contains("TEAM") || lowerCategory.Contains("LEADERSHIP"))
            return InterviewEnums.QuestionType.Behavioral;
        if (lowerCategory.Contains("COMMUNICATION") || lowerCategory.Contains("PRESENTATION"))
            return InterviewEnums.QuestionType.Communication;

        return InterviewEnums.QuestionType.Technical;
    }

    private static QuestionDto CreateQuestionDto(GeminiQuestion geminiQuestion, string questionType)
    {
        return new QuestionDto
        {
            QuestionContent = geminiQuestion.Question,
            QuestionType = questionType,
            Difficulty = InterviewEnums.QuestionDifficulty.Intermediate,
            Category = geminiQuestion.Category ?? "General",
            SkillArea = geminiQuestion.SkillFocus ?? "General"
        };
    }

    public InterviewQuestionGenerationResult GetDefaultQuestions(
        string targetJob,
        List<string> requiredSkills,
        List<string> missingSkills)
    {
        _logger.LogInformation("Generating default interview questions for: {TargetJob}", targetJob);
        
        var request = new GeminiInterviewQuestionRequest
        {
            TargetJob = new TargetJobInfo
            {
                Title = targetJob,
                Description = targetJob
            },
            RequiredSkills = requiredSkills,
            MissingSkills = missingSkills
        };

        return GetDefaultQuestions(request);
    }

    private InterviewQuestionGenerationResult GetDefaultQuestions(GeminiInterviewQuestionRequest request)
    {
        var questions = new List<QuestionDto>();
        
        var missingSkills = request.MissingSkills.Count > 0 
            ? request.MissingSkills 
            : request.RequiredSkills;
        
        var allSkills = request.RequiredSkills.Count > 0 
            ? request.RequiredSkills 
            : new List<string> { "General Skills" };

        // Technical Questions - 5 questions, prioritize missing skills
        for (int i = 0; i < 5; i++)
        {
            string skill;
            if (i < missingSkills.Count)
            {
                skill = missingSkills[i];
            }
            else
            {
                var requiredIndex = i - missingSkills.Count;
                if (requiredIndex < allSkills.Count)
                    skill = allSkills[requiredIndex];
                else
                    skill = allSkills[i % allSkills.Count];
            }

            questions.Add(new QuestionDto
            {
                QuestionContent = $"Describe your experience with {skill}. What projects have you built using this technology?",
                QuestionType = InterviewEnums.QuestionType.Technical,
                Difficulty = InterviewEnums.QuestionDifficulty.Intermediate,
                Category = "Technical",
                SkillArea = skill
            });
        }

        // Behavioral Questions - 3 questions
        var behavioralCategories = new[] { "Teamwork", "Leadership", "Problem Solving" };
        for (int i = 0; i < 3; i++)
        {
            questions.Add(new QuestionDto
            {
                QuestionContent = $"Describe a situation where you demonstrated {behavioralCategories[i].ToLower()} in your work.",
                QuestionType = InterviewEnums.QuestionType.Behavioral,
                Difficulty = InterviewEnums.QuestionDifficulty.Intermediate,
                Category = "Behavioral",
                SkillArea = behavioralCategories[i]
            });
        }

        // Communication Questions - 2 questions
        questions.Add(new QuestionDto
        {
            QuestionContent = "How would you explain a complex technical concept to a non-technical stakeholder?",
            QuestionType = InterviewEnums.QuestionType.Communication,
            Difficulty = InterviewEnums.QuestionDifficulty.Intermediate,
            Category = "Communication",
            SkillArea = "Communication"
        });

        questions.Add(new QuestionDto
        {
            QuestionContent = "Describe a time you had to present technical findings to a diverse audience.",
            QuestionType = InterviewEnums.QuestionType.Communication,
            Difficulty = InterviewEnums.QuestionDifficulty.Intermediate,
            Category = "Communication",
            SkillArea = "Communication"
        });

        var summary = new GenerationSummary
        {
            TotalQuestionsGenerated = 10,
            ByType = new QuestionCountByType
            {
                Technical = 5,
                Behavioral = 3,
                Communication = 2
            },
            SkillsCovered = request.RequiredSkills,
            SkillsToFocus = request.MissingSkills,
            EstimatedDuration = TimeSpan.FromMinutes(50)
        };

        _logger.LogInformation(
            "Generated {Count} default questions. Technical: {Tech}, Behavioral: {Behav}, Communication: {Comm}",
            10, 5, 3, 2);

        return new InterviewQuestionGenerationResult
        {
            TargetJob = new TargetJobSummary
            {
                Title = request.TargetJob.Title,
                Company = request.TargetJob.Company
            },
            Summary = summary,
            Questions = questions,
            Status = GenerationStatusEnum.Completed,
            IsFallback = true,
            Metadata = new GenerationMetadata
            {
                AiModelUsed = "fallback",
                GenerationVersion = "1.0"
            }
        };
    }
}

public class GeminiApiException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }

    public GeminiApiException(System.Net.HttpStatusCode statusCode) 
        : base($"Gemini API returned {statusCode}")
    {
        StatusCode = statusCode;
    }

    public bool IsRetryable => StatusCode == System.Net.HttpStatusCode.TooManyRequests 
                            || StatusCode == System.Net.HttpStatusCode.InternalServerError;
}
