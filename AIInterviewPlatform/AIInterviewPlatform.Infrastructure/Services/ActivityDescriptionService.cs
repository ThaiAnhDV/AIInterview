using System.Linq;
using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class ActivityDescriptionService : IActivityDescriptionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ActivityDescriptionService> _logger;
    private readonly IPromptProviderFactory _promptProviderFactory;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public ActivityDescriptionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ActivityDescriptionService> logger,
        IPromptProviderFactory promptProviderFactory)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
        _promptProviderFactory = promptProviderFactory;
    }

    public async Task<ActivityDescriptionResponse?> GenerateActivityDescriptionAsync(string skillName, string? languageCode = null)
    {
        return await GenerateActivityDescriptionAsync(skillName, "BEGINNER", languageCode);
    }

    public async Task<ActivityDescriptionResponse?> GenerateActivityDescriptionAsync(
        string skillName, 
        string difficultyLevel,
        string? languageCode = null)
    {
        _logger.LogInformation(
            "[LANG_AUDIT] Service={Service} Method={Method} LanguageCode={LanguageCode}",
            nameof(ActivityDescriptionService), "GenerateActivityDescriptionAsync", languageCode);

        if (string.IsNullOrWhiteSpace(skillName))
        {
            _logger.LogWarning("Skill name is empty");
            _logger.LogWarning("[ROADMAP_AUDIT] ACTIVITY_GENERATION_FAILED Reason=EmptySkillName");
            return null;
        }

        var provider = _promptProviderFactory.Get(languageCode);
        var prompt = provider.BuildActivityDescriptionPrompt(skillName, difficultyLevel);
        
        try
        {
            _logger.LogInformation(
                "[ROADMAP_AI] ActivityDescriptionService executing Skill={Skill} Difficulty={Difficulty}",
                skillName,
                difficultyLevel);

            _logger.LogInformation(
                "[ROADMAP_AUDIT] ActivityDescriptionService.GenerateActivityDescriptionAsync Skill={Skill} Difficulty={Difficulty}",
                skillName,
                difficultyLevel);

            var response = await SendGeminiRequestAsync(prompt);
            if (response == null)
            {
                _logger.LogWarning("Gemini request returned null for skill: {Skill}", skillName);
                _logger.LogWarning("[ROADMAP_AUDIT] ACTIVITY_GENERATION_FAILED Reason=NullGeminiResponse Skill={Skill}", skillName);
                return null;
            }
            
            var parsed = ParseGeminiResponse(response);
            if (parsed == null)
            {
                _logger.LogWarning("[ROADMAP_AUDIT] ACTIVITY_GENERATION_FAILED Reason=ParseReturnedNull Skill={Skill}", skillName);
                return null;
            }

            _logger.LogInformation(
                "[ROADMAP_AI] ACTIVITY_GENERATION_SUCCESS Skill={Skill} Title={Title} Type={Type}",
                skillName,
                parsed.ActivityTitle,
                parsed.ActivityType);

            _logger.LogInformation(
                "[ROADMAP_AUDIT] Parsed Activity Skill={Skill} Title={Title} Description={Description} Type={Type}",
                skillName,
                parsed.ActivityTitle,
                parsed.ActivityDescription,
                parsed.ActivityType);

            return parsed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate activity description for skill: {Skill}. Returning null for fallback.", skillName);
            _logger.LogWarning("[ROADMAP_AI] ACTIVITY_GENERATION_FAILED Skill={Skill} Reason=Exception", skillName);
            return null;
        }
    }

    private async Task<string?> SendGeminiRequestAsync(string prompt)
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
                temperature = 0.7,
                maxOutputTokens = 200,
                responseMimeType = "application/json"
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("[ROADMAP_AI] Calling Gemini URL={GeminiApiUrl}", GeminiApiUrl);
            _logger.LogInformation("[ROADMAP_AUDIT] ActivityDescriptionService.SendGeminiRequestAsync prompt prepared");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            var response = await _httpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent,
                cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Gemini API returned {StatusCode} ({ReasonPhrase}). Using fallback activity description.",
                    response.StatusCode,
                    response.ReasonPhrase);
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("[ROADMAP_AI] RAW_RESPONSE={RawResponse}", responseBody);
            _logger.LogInformation("[ROADMAP_AI] Gemini response received URL={GeminiApiUrl} StatusCode={StatusCode}", GeminiApiUrl, response.StatusCode);
            _logger.LogInformation("[ROADMAP_AUDIT] Gemini Response Received in ActivityDescriptionService StatusCode={StatusCode}", response.StatusCode);
            return responseBody;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !string.IsNullOrEmpty(ex.Message))
        {
            _logger.LogWarning("Gemini API request timed out for skill. Using fallback activity description.");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Gemini API HTTP request failed with status {StatusCode}. Using fallback activity description.",
                ex.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error calling Gemini API. Using fallback activity description.");
            return null;
        }
    }

    private ActivityDescriptionResponse? ParseGeminiResponse(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            _logger.LogWarning("Empty response body from Gemini API");
            _logger.LogWarning("[ROADMAP_AUDIT] Parse Failed Reason=EmptyResponseBody");
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.ValueKind == JsonValueKind.Array &&
                candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];

                if (candidate.TryGetProperty("finishReason", out var finishReasonElement))
                {
                    _logger.LogInformation("[ROADMAP_AI] FINISH_REASON={FinishReason}", finishReasonElement.GetString() ?? string.Empty);
                }

                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.ValueKind == JsonValueKind.Array &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString() ?? string.Empty;
                    _logger.LogInformation("[ROADMAP_AI] GENERATED_TEXT={Text}", text);
                    return ExtractActivityFromJsonText(text);
                }
            }

            _logger.LogWarning("Unexpected Gemini response format - no candidates found");
            _logger.LogWarning("[ROADMAP_AUDIT] Parse Failed Reason=NoCandidatesFound");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Gemini JSON response");
            _logger.LogWarning("[ROADMAP_AUDIT] Parse Failed Reason=EnvelopeJsonException");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing Gemini response");
            _logger.LogWarning("[ROADMAP_AUDIT] Parse Failed Reason=EnvelopeException");
            return null;
        }
    }

    private ActivityDescriptionResponse? ExtractActivityFromJsonText(string? jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            _logger.LogWarning("Empty text from Gemini response");
            return null;
        }

        var cleanedText = jsonText.Trim();
        _logger.LogInformation("[ROADMAP_AI] CLEANED_TEXT={CleanedText}", cleanedText);

        if (cleanedText.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleanedText = cleanedText[7..].Trim();
        }
        else if (cleanedText.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleanedText = cleanedText[3..].Trim();
        }

        if (cleanedText.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleanedText = cleanedText[..^3].Trim();
        }

        var candidateJson = ExtractJsonObject(cleanedText);
        if (string.IsNullOrWhiteSpace(candidateJson))
        {
            _logger.LogWarning("[ROADMAP_AI] JSON_PARSE_FAILED Reason=NoJsonObjectFound");
            return null;
        }

        _logger.LogInformation("[ROADMAP_AI] CLEANED_TEXT={CleanedText}", candidateJson);
        _logger.LogInformation("[ROADMAP_AI] JSON_PARSE_START");

        try
        {
            using var parsed = JsonDocument.Parse(candidateJson);
            var root = parsed.RootElement;

            var activityTitle = root.TryGetProperty("activityTitle", out var titleElement)
                ? titleElement.GetString() ?? string.Empty
                : string.Empty;

            var activityDescription = root.TryGetProperty("activityDescription", out var descriptionElement)
                ? descriptionElement.GetString() ?? string.Empty
                : string.Empty;

            var activityType = root.TryGetProperty("activityType", out var activityTypeElement)
                ? activityTypeElement.GetString() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(activityTitle) || string.IsNullOrWhiteSpace(activityDescription))
            {
                _logger.LogWarning("[ROADMAP_AI] JSON_PARSE_FAILED Reason=MissingRequiredFields");
                _logger.LogWarning("[ROADMAP_AUDIT] Parse Failed Reason=MissingRequiredFields Json={Json}", candidateJson);
                return null;
            }

            var result = new ActivityDescriptionResponse
            {
                ActivityTitle = activityTitle.Trim(),
                ActivityDescription = TruncateToWords(activityDescription.Trim(), 50),
                ActivityType = activityType.Trim()
            };

            _logger.LogInformation("[ROADMAP_AI] JSON_PARSE_SUCCESS");
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "[ROADMAP_AI] JSON_PARSE_FAILED");
            _logger.LogWarning("[ROADMAP_AUDIT] Parse Failed Reason=JsonException Json={Json}", candidateJson);
            return null;
        }
    }

    private static string? ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var firstBrace = text.IndexOf('{');
        var lastBrace = text.LastIndexOf('}');

        if (firstBrace < 0 || lastBrace <= firstBrace)
        {
            return null;
        }

        return text[firstBrace..(lastBrace + 1)].Trim();
    }

    private static string TruncateToWords(string? text, int maxWords)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Length <= maxWords)
        {
            return text;
        }

        return string.Join(" ", words.Take(maxWords));
    }
}
