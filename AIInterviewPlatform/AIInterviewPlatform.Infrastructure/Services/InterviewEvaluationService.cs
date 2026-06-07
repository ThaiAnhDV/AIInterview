using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;
using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class InterviewEvaluationService : IInterviewEvaluationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<InterviewEvaluationService> _logger;
    private readonly IPromptProviderFactory _promptProviderFactory;

    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public InterviewEvaluationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<InterviewEvaluationService> logger,
        IPromptProviderFactory promptProviderFactory)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
        _promptProviderFactory = promptProviderFactory;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("InterviewEvaluationService: Gemini API key not configured");
        }
    }

    public async Task<EvaluationResultDto> EvaluateAnswerAsync(
        EvaluationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await EvaluateAsync(request.Question, request.Answer, request.Category, request.SkillFocus, request.LanguageCode, cancellationToken);
        }
        catch (Exception ex)
        {
            return CreateFailureResult("UNEXPECTED_ERROR", "Evaluation failed", ex.Message);
        }
    }

    public async Task<EvaluationResultDto> EvaluateAnswerAsync(
        string question,
        string answer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
            {
                return CreateFailureResult("INVALID_INPUT", "Evaluation failed", "Question or answer was empty.");
            }

            return await EvaluateAsync(question, answer, null, null, null, cancellationToken);
        }
        catch (Exception ex)
        {
            return CreateFailureResult("UNEXPECTED_ERROR", "Evaluation failed", ex.Message);
        }
    }

    private async Task<EvaluationResultDto> EvaluateAsync(
        string question,
        string answer,
        string? category,
        string? skillFocus,
        string? languageCode,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[LANG_AUDIT] Service={Service} Method={Method} LanguageCode={LanguageCode}",
            nameof(InterviewEvaluationService), "EvaluateAsync", languageCode);

        LogEvaluationInput(question, answer, category, skillFocus);

        if (string.IsNullOrEmpty(_apiKey))
        {
            return CreateFailureResult("MISSING_API_KEY", "Evaluation failed", "Gemini API key not configured.");
        }

        try
        {
            var provider = _promptProviderFactory.Get(languageCode);
            var prompt = provider.BuildInterviewEvaluationPrompt(question, answer, category, skillFocus);
            _logger.LogInformation("[Evaluation] PROMPT_SENT\n{Prompt}", prompt);

            var rawGeminiResponse = await SendGeminiRequestAsync(prompt, cancellationToken);

            if (string.IsNullOrWhiteSpace(rawGeminiResponse))
            {
                return CreateFailureResult("EMPTY_GEMINI_RESPONSE", "Evaluation failed", "Gemini returned an empty response.");
            }

            _logger.LogInformation("[Evaluation] RAW_GEMINI_RESPONSE\n{Response}", rawGeminiResponse);

            var result = ParseEvaluationResponse(rawGeminiResponse);
            if (result == null)
            {
                return CreateFailureResult("PARSE_FAILED", "Evaluation failed", "Gemini response could not be parsed.");
            }

            _logger.LogInformation(
                "[Evaluation] PARSED_RESULT {@ParsedResult}",
                new
                {
                    result.Clarity,
                    result.TechnicalAccuracy,
                    result.Completeness,
                    result.Overall,
                    result.Strengths,
                    result.Weaknesses,
                    result.Feedback
                });

            _logger.LogInformation("[Evaluation] FINAL_SCORE {OverallScore}", result.Overall);
            return result;
        }
        catch (Exception ex)
        {
            return CreateFailureResult("EVALUATION_EXCEPTION", "Evaluation failed", ex.Message);
        }
    }

    private void LogEvaluationInput(string question, string answer, string? category, string? skillFocus)
    {
        _logger.LogInformation("[Evaluation] QUESTION\n{Question}", question);
        _logger.LogInformation("[Evaluation] ANSWER\n{Answer}", answer);
        _logger.LogInformation("[Evaluation] CATEGORY {Category}", category ?? "General");
        _logger.LogInformation("[Evaluation] SKILL {Skill}", skillFocus ?? "Not specified");
    }

    private async Task<string?> SendGeminiRequestAsync(string prompt, CancellationToken cancellationToken)
    {
        try
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
                    temperature = 0.2,
                    maxOutputTokens = 2048,
                    responseMimeType = "application/json"
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent,
                cancellationToken);

            return await HandleResponseAsync(response, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "[Evaluation] FALLBACK_REASON HTTP error while calling Gemini");
            return null;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            _logger.LogWarning(ex, "[Evaluation] FALLBACK_REASON Gemini request timeout");
            return null;
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            _logger.LogWarning(ex, "[Evaluation] FALLBACK_REASON Gemini request cancelled");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Evaluation] FALLBACK_REASON Unexpected Gemini request error");
            return null;
        }
    }

    private async Task<string?> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("[Evaluation] FALLBACK_REASON Gemini API error {StatusCode}", statusCode);
            return null;
        }

        try
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return ExtractTextFromResponse(responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Evaluation] FALLBACK_REASON Error reading Gemini response");
            return null;
        }
    }

    private static string? ExtractTextFromResponse(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) ||
                candidates.ValueKind != JsonValueKind.Array ||
                candidates.GetArrayLength() == 0)
            {
                return null;
            }

            var candidate = candidates[0];
            if (!candidate.TryGetProperty("content", out var content) ||
                !content.TryGetProperty("parts", out var parts) ||
                parts.ValueKind != JsonValueKind.Array ||
                parts.GetArrayLength() == 0)
            {
                return null;
            }

            var text = parts[0].GetProperty("text").GetString();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch
        {
            return null;
        }
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

    private EvaluationResultDto? ParseEvaluationResponse(string responseBody)
    {
        try
        {
            var cleanedText = CleanJsonResponse(responseBody);
            if (string.IsNullOrWhiteSpace(cleanedText))
            {
                return null;
            }

            var result = JsonSerializer.Deserialize<GeminiEvaluationResponseDto>(cleanedText, JsonOptions);
            return result == null ? TryParseManually(cleanedText) : NormalizeResult(result);
        }
        catch (JsonException)
        {
            return TryParseManually(responseBody);
        }
        catch
        {
            return TryParseManually(responseBody);
        }
    }

    private static EvaluationResultDto NormalizeResult(GeminiEvaluationResponseDto result)
    {
        return new EvaluationResultDto
        {
            Success = true,
            IsFallback = false,
            AiUsed = true,
            GeneratedBy = "GEMINI",
            Clarity = NormalizeScore(result.ClarityScore),
            TechnicalAccuracy = NormalizeScore(result.TechnicalAccuracyScore),
            Completeness = NormalizeScore(result.CompletenessScore),
            Overall = NormalizeScore(result.OverallScore),
            Strengths = NormalizeList(result.Strengths),
            Weaknesses = NormalizeList(result.Weaknesses),
            Feedback = !string.IsNullOrWhiteSpace(result.Feedback) ? result.Feedback.Trim() : "No feedback provided."
        };
    }

    private static decimal NormalizeScore(decimal score)
    {
        if (score < 0) return 0;
        if (score > 100) return 100;
        return Math.Round(score, 1);
    }

    private EvaluationResultDto? TryParseManually(string text)
    {
        try
        {
            var cleanedText = CleanJsonResponse(text);

            var clarity = ExtractScore(cleanedText, "clarityScore");
            var technicalAccuracy = ExtractScore(cleanedText, "technicalAccuracyScore");
            var completeness = ExtractScore(cleanedText, "completenessScore");
            var overall = ExtractScore(cleanedText, "overallScore");

            if (clarity is null || technicalAccuracy is null || completeness is null || overall is null)
            {
                return null;
            }

            return new EvaluationResultDto
            {
                Success = true,
                IsFallback = false,
                AiUsed = true,
                GeneratedBy = "GEMINI",
                Clarity = clarity.Value,
                TechnicalAccuracy = technicalAccuracy.Value,
                Completeness = completeness.Value,
                Overall = overall.Value,
                Strengths = ExtractStringArray(cleanedText, "strengths"),
                Weaknesses = ExtractStringArray(cleanedText, "weaknesses"),
                Feedback = ExtractString(cleanedText, "feedback") is { } fb && !string.IsNullOrWhiteSpace(fb)
                    ? fb
                    : "No feedback provided."
            };
        }
        catch
        {
            return null;
        }
    }

    private static decimal? ExtractScore(string text, string fieldName)
    {
        var pattern = $@"""{Regex.Escape(fieldName)}""\s*:\s*(-?\d+(?:\.\d+)?)";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var score))
        {
            return Math.Clamp(score, 0, 100);
        }

        return null;
    }

    private static string ExtractString(string text, string fieldName)
    {
        var pattern = $@"""{Regex.Escape(fieldName)}""\s*:\s*""([^""\\]*(?:\\.[^""\\]*)*)""";
        var match = Regex.Match(text, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            return match.Groups[1].Value
                .Replace("\\\"", "\"")
                .Replace("\\n", "\n")
                .Replace("\\\\", "\\");
        }

        return string.Empty;
    }

    private static List<string> ExtractStringArray(string text, string fieldName)
    {
        var pattern = $@"""{Regex.Escape(fieldName)}""\s*:\s*\[(.*?)\]";
        var match = Regex.Match(text, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return new List<string>();
        }

        var body = match.Groups[1].Value;
        var itemMatches = Regex.Matches(body, "\"([^\"]*)\"");
        return itemMatches.Select(m => m.Groups[1].Value.Trim()).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
    }

    private static List<string> NormalizeList(List<string>? values)
    {
        return values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList() ?? new List<string>();
    }

    private EvaluationResultDto CreateFailureResult(string errorCode, string message, string fallbackReason)
    {
        _logger.LogWarning("[Evaluation] GEMINI_FAILURE {Reason}", fallbackReason);
        return new EvaluationResultDto
        {
            Success = false,
            IsFallback = false,
            AiUsed = false,
            GeneratedBy = "FAILED",
            ErrorCode = errorCode,
            Message = message,
            ErrorMessage = fallbackReason,
            Feedback = "Evaluation failed"
        };
    }

    private class GeminiEvaluationResponseDto
    {
        public decimal ClarityScore { get; set; }
        public decimal TechnicalAccuracyScore { get; set; }
        public decimal CompletenessScore { get; set; }
        public decimal OverallScore { get; set; }
        public List<string>? Strengths { get; set; }
        public List<string>? Weaknesses { get; set; }
        public string? Feedback { get; set; }
    }
}
