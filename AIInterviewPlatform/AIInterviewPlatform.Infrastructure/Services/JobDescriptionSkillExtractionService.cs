using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.JobDescriptionSkillExtraction;
using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class JobDescriptionSkillExtractionService : IJobDescriptionSkillExtractionService
{
    private static readonly Dictionary<string, string> SkillNormalizationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["auditing processes"] = "Auditing",
        ["auditing process"] = "Auditing",
        ["financial data analysis"] = "Data Analysis",
        ["audit documentation preparation"] = "Audit Documentation",
        ["microsoft word professional reporting"] = "Microsoft Word",
        ["excel data analysis reporting"] = "Microsoft Excel",
        ["bookkeeping and reporting tools"] = "Accounting"
    };

    private static readonly string[] ExcludedSkillPhrases =
    [
        "responsible for",
        "worked on",
        "assisted with",
        "supported",
        "handled",
        "performed",
        "activities",
        "tasks",
        "achievements",
        "project"
    ];

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<JobDescriptionSkillExtractionService> _logger;
    private readonly IPromptProviderFactory _promptProviderFactory;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    private const string GeminiModel = "gemini-2.5-flash";
    private const int RetryCount = 3;

    public string ModelName => "gemini-2.5-flash";
    public string Endpoint => GeminiApiUrl;

    public JobDescriptionSkillExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<JobDescriptionSkillExtractionService> logger,
        IPromptProviderFactory promptProviderFactory)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
        _promptProviderFactory = promptProviderFactory;
    }

    public async Task<AIServiceResult<JobDescriptionSkillExtractionResult>> ExtractRequiredSkillsAsync(
        string jobDescriptionContent,
        string? languageCode = null)
    {
        _logger.LogInformation(
            "[LANG_AUDIT] Service={Service} Method={Method} LanguageCode={LanguageCode}",
            nameof(JobDescriptionSkillExtractionService), "ExtractRequiredSkillsAsync", languageCode);

        if (string.IsNullOrWhiteSpace(jobDescriptionContent))
        {
            _logger.LogWarning("Job description content is empty");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Succeeded(
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("Gemini API key is not configured for job description extraction");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                "GEMINI_NOT_CONFIGURED",
                "AI service temporarily unavailable.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }

        var provider = _promptProviderFactory.Get(languageCode);
        var prompt = provider.BuildJobDescriptionSkillExtractionPrompt(jobDescriptionContent);
        _logger.LogInformation("=== JD EXTRACTION DEBUG ===");
        _logger.LogInformation("Prompt: {Prompt}", prompt);
        var response = await SendGeminiRequestAsync(prompt);

        if (!response.Success)
        {
            _logger.LogInformation("Raw Gemini Response: <request failed>");
            _logger.LogInformation("Parsed DTO: <none>");
            _logger.LogInformation("Validation Result: Failed with {ErrorCode} - {Message}", response.Error?.ErrorCode, response.Error?.Message);
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                response.Error?.ErrorCode ?? "GEMINI_ERROR",
                response.Error?.Message ?? "Unable to process request at this time.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }

        _logger.LogInformation("Raw Gemini Response: {Response}", response.Data);
        var parsedResult = ParseGeminiResponse(response.Data ?? string.Empty);
        _logger.LogInformation("Parsed DTO: {@ParsedDto}", parsedResult.Data);
        _logger.LogInformation("Validation Result: {ValidationResult}", parsedResult.Success);
        return parsedResult;
    }

    private async Task<AIServiceResult<string>> SendGeminiRequestAsync(string prompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                topP = 0.95,
                topK = 40,
                maxOutputTokens = 2048,
                responseMimeType = "application/json"
            }
        };

        _logger.LogInformation("[JDExtraction] MAX_OUTPUT_TOKENS={Value}", 2048);

        var jsonContent = JsonSerializer.Serialize(requestBody);

        try
        {
            for (var attempt = 1; attempt <= RetryCount; attempt++)
            {
                using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(
                    $"{GeminiApiUrl}?key={_apiKey}",
                    httpContent);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return AIServiceResult<string>.Succeeded(responseBody);
                }

                if (IsTransientStatusCode(response.StatusCode) && attempt < RetryCount)
                {
                    _logger.LogWarning("[JDExtraction] RETRY_ATTEMPT={Attempt}", attempt);
                    _logger.LogWarning("[JDExtraction] RETRY_REASON={StatusCode}", (int)response.StatusCode);
                    _logger.LogWarning("[JDExtraction] GEMINI_UNAVAILABLE");

                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                    continue;
                }

                _logger.LogError("Gemini API returned {StatusCode} for job description extraction: {Response}", response.StatusCode, responseBody);

                if (IsTransientStatusCode(response.StatusCode))
                {
                    _logger.LogError("[JDExtraction] GEMINI_UNAVAILABLE");
                    _logger.LogError("[JDExtraction] RETRIES_EXHAUSTED");
                    return CreateGeminiUnavailableFailure(response.StatusCode, responseBody);
                }

                return AIServiceResult<string>.Failed(
                    response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden
                        ? "GEMINI_AUTH_ERROR"
                        : "GEMINI_ERROR",
                    $"Gemini request failed with HTTP {(int)response.StatusCode} {response.StatusCode}.",
                    responseBody);
            }

            _logger.LogError("[JDExtraction] RETRIES_EXHAUSTED");
            return CreateGeminiUnavailableFailure(null, string.Empty);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Gemini API timeout for job description skill extraction");
            _logger.LogError("[JDExtraction] GEMINI_UNAVAILABLE");
            _logger.LogError("[JDExtraction] RETRIES_EXHAUSTED");
            return CreateGeminiUnavailableFailure(HttpStatusCode.GatewayTimeout, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call Gemini API for job description skill extraction");
            _logger.LogError("[JDExtraction] GEMINI_UNAVAILABLE");
            _logger.LogError("[JDExtraction] RETRIES_EXHAUSTED");
            return CreateGeminiUnavailableFailure(null, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during job description skill extraction");
            _logger.LogError("[JDExtraction] GEMINI_UNAVAILABLE");
            _logger.LogError("[JDExtraction] RETRIES_EXHAUSTED");
            return CreateGeminiUnavailableFailure(null, ex.Message);
        }
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }

    private static AIServiceResult<string> CreateGeminiUnavailableFailure(HttpStatusCode? statusCode, string rawResponse)
    {
        var diagnostics = new
        {
            Success = false,
            AiUsed = false,
            GeneratedBy = "FAILED",
            ErrorMessage = "Gemini service unavailable. Please try again later.",
            Diagnostics = new
            {
                HttpStatus = statusCode.HasValue ? (int)statusCode.Value : (int?)null,
                RawResponse = rawResponse,
                Model = GeminiModel
            }
        };

        return AIServiceResult<string>.Failed(
            "GEMINI_UNAVAILABLE",
            "AI service is temporarily busy. Please retry in a few minutes.",
            JsonSerializer.Serialize(diagnostics));
    }

    private AIServiceResult<JobDescriptionSkillExtractionResult> ParseGeminiResponse(string responseBody)
    {
        try
        {
            _logger.LogInformation(
                "[JDExtraction] RAW_GEMINI_RESPONSE={Response}",
                responseBody);

            _logger.LogInformation(
                "[JDExtraction] RESPONSE_LENGTH={Length}",
                responseBody?.Length ?? 0);

            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.ValueKind == JsonValueKind.Array &&
                candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];

                var finishReason = candidate.TryGetProperty("finishReason", out var finishReasonElement)
                    ? finishReasonElement.GetString() ?? "OTHER"
                    : "OTHER";

                _logger.LogInformation(
                    "[JDExtraction] FINISH_REASON={Reason}",
                    finishReason);

                if (string.Equals(finishReason, "MAX_TOKENS", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("[JDExtraction] OUTPUT_TRUNCATED");
                    return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                        "OUTPUT_TRUNCATED",
                        "Gemini output was truncated due to token limit",
                        new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
                }

                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.ValueKind == JsonValueKind.Array &&
                    parts.GetArrayLength() > 0)
                {
                    var generatedText = parts[0].GetProperty("text").GetString() ?? string.Empty;

                    _logger.LogInformation(
                        "[JDExtraction] GENERATED_TEXT={Text}",
                        generatedText);

                    return ExtractSkillsFromJsonText(generatedText);
                }
            }

            _logger.LogWarning("Unexpected Gemini response format for job description");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an unexpected response.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response for job description");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an invalid response.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }
    }

    private AIServiceResult<JobDescriptionSkillExtractionResult> ExtractSkillsFromJsonText(string jsonText)
    {
        var cleanedText = jsonText.Trim();

        if (cleanedText.StartsWith("```json"))
        {
            cleanedText = cleanedText[7..];
        }
        else if (cleanedText.StartsWith("```"))
        {
            cleanedText = cleanedText[3..];
        }

        if (cleanedText.EndsWith("```"))
        {
            cleanedText = cleanedText[..^3];
        }

        cleanedText = cleanedText.Trim();

        var result = TryDeserialize(cleanedText);

        if (result == null)
        {
            result = TryExtractWithRegex(cleanedText);
        }

        if (result == null)
        {
            _logger.LogWarning("Could not extract required skills from Gemini response text");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an invalid response.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }

        return AIServiceResult<JobDescriptionSkillExtractionResult>.Succeeded(result);
    }

    private JobDescriptionSkillExtractionResult? TryDeserialize(string jsonText)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<JsonElement>(jsonText);

            if (parsed.TryGetProperty("requiredSkills", out var skillsElement) &&
                skillsElement.ValueKind == JsonValueKind.Array)
            {
                var skills = new List<string>();

                foreach (var skill in skillsElement.EnumerateArray())
                {
                    var skillValue = skill.GetString();
                    if (!string.IsNullOrWhiteSpace(skillValue))
                    {
                        var normalized = NormalizeSkill(skillValue);
                        if (!string.IsNullOrWhiteSpace(normalized))
                        {
                            skills.Add(normalized);
                        }
                    }
                }

                var normalizedSkills = NormalizeExtractedSkills(skills);
                _logger.LogInformation("[JDExtraction] RAW_SKILLS={Skills}", string.Join(", ", skills));
                _logger.LogInformation("[JDExtraction] NORMALIZED_SKILLS={Skills}", string.Join(", ", normalizedSkills));

                return new JobDescriptionSkillExtractionResult
                {
                    RequiredSkills = normalizedSkills
                };
            }

            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "JSON deserialization failed, trying regex extraction");
            return null;
        }
    }

    private JobDescriptionSkillExtractionResult? TryExtractWithRegex(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            text,
            @"\{.*?""requiredSkills""\s*:\s*\[(.*?)\].*\}",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        if (match.Success)
        {
            var skillsArray = match.Groups[1].Value;
            var skillMatches = System.Text.RegularExpressions.Regex.Matches(
                skillsArray,
                @"""([^""]+)""");

            var skills = skillMatches
                .Select(m => NormalizeSkill(m.Groups[1].Value))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var normalizedSkills = NormalizeExtractedSkills(skills);
            _logger.LogInformation("[JDExtraction] RAW_SKILLS={Skills}", string.Join(", ", skills));
            _logger.LogInformation("[JDExtraction] NORMALIZED_SKILLS={Skills}", string.Join(", ", normalizedSkills));

            return new JobDescriptionSkillExtractionResult { RequiredSkills = normalizedSkills };
        }

        _logger.LogWarning("Could not extract skills using regex from response: {Text}", text);
        return null;
    }

    private static List<string> NormalizeExtractedSkills(IEnumerable<string> skills)
    {
        return skills
            .Select(skill => skill.Trim())
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(RemoveParentheticalDetails)
            .Select(skill => Regex.Replace(skill, @"[^\p{L}\p{Nd}\s]", " "))
            .Select(skill => Regex.Replace(skill, @"\s+", " ").Trim())
            .Select(skill => SkillNormalizationMap.TryGetValue(skill, out var mapped) ? mapped : skill)
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Where(skill => !ExcludedSkillPhrases.Any(phrase => skill.Contains(phrase, StringComparison.OrdinalIgnoreCase)))
            .Select(skill => NormalizeSkill(skill))
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string RemoveParentheticalDetails(string skill)
    {
        return Regex.Replace(skill, @"\([^)]*\)", string.Empty).Trim();
    }

    private static string NormalizeSkill(string skill)
    {
        var normalized = skill.Trim();

        normalized = normalized.ToUpperInvariant() switch
        {
            "C#" or "C SHARP" => "C#",
            "C++" or "C PLUS PLUS" => "C++",
            ".NET" or "DOT NET" or "DOTNET" => ".NET",
            "NODE.JS" or "NODEJS" or "NODE JS" => "Node.js",
            "JAVASCRIPT" => "JavaScript",
            "TYPESCRIPT" => "TypeScript",
            "PYTHON" => "Python",
            "JAVA" => "Java",
            "POSTGRESQL" or "POSTGRES" => "PostgreSQL",
            "MONGODB" => "MongoDB",
            "DOCKER" => "Docker",
            "KUBERNETES" or "K8S" => "Kubernetes",
            "REDIS" => "Redis",
            "RABBITMQ" => "RabbitMQ",
            "GRAPHQL" => "GraphQL",
            "REST API" or "RESTAPI" or "REST" => "REST API",
            "GIT" => "Git",
            "AWS" => "AWS",
            "AZURE" => "Azure",
            "GCP" or "GOOGLE CLOUD" => "GCP",
            "MICROSERVICES" => "Microservices",
            "SQL SERVER" or "MSSQL" => "SQL Server",
            "SQL" => "SQL",
            "HTML" => "HTML",
            "CSS" => "CSS",
            "REACT" => "React",
            "ANGULAR" => "Angular",
            "VUE" or "VUE.JS" => "Vue.js",
            "ASP.NET CORE" or "ASP.NET" or "ASPNETCORE" => "ASP.NET Core",
            "ENTITY FRAMEWORK" or "EF" => "Entity Framework",
            "LINQ" => "LINQ",
            "WEB API" => "Web API",
            "CI/CD" or "CICD" => "CI/CD",
            "AGILE" => "Agile",
            "SCRUM" => "Scrum",
            _ => normalized
        };

        return normalized;
    }
}
