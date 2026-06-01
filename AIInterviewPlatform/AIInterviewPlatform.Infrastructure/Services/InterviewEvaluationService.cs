using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class InterviewEvaluationService : IInterviewEvaluationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<InterviewEvaluationService> _logger;

    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private const string EvaluationPromptTemplate = """
        Evaluate interview answer.

        Question: {question}
        Category: {category}
        Skill: {skillFocus}
        Answer: {answer}

        Scoring guide (0-100):
        0-30: Incorrect, irrelevant, or missing
        31-60: Partially correct with significant gaps
        61-80: Mostly correct with minor weaknesses
        81-100: Accurate, complete, and clearly explained

        Score criteria:
        - clarity: communication and understanding
        - structure: organization and flow
        - relevance: alignment with question
        - overall: general quality

        Rules: Be objective. Do not inflate scores. Feedback max 30 words. Improvement max 20 words.

        JSON:
        {{"clarity":0,"structure":0,"relevance":0,"overall":0,"feedback":"","improvement":""}}
        """;

    public InterviewEvaluationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<InterviewEvaluationService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;

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
            return await EvaluateAsync(request.Question, request.Answer, request.Category, request.SkillFocus, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InterviewEvaluationService: Unexpected error - returning fallback");
            return CreateFallbackResult();
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
                _logger.LogWarning("InterviewEvaluationService: Empty question or answer");
                return CreateFallbackResult();
            }

            return await EvaluateAsync(question, answer, null, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InterviewEvaluationService: Unexpected error - returning fallback");
            return CreateFallbackResult();
        }
    }

    private async Task<EvaluationResultDto> EvaluateAsync(
        string question,
        string answer,
        string? category,
        string? skillFocus,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("InterviewEvaluationService: API key missing - using fallback");
            return CreateFallbackResult();
        }

        try
        {
            var prompt = BuildPrompt(question, answer, category, skillFocus);
            var responseJson = await SendGeminiRequestAsync(prompt, cancellationToken);

            if (string.IsNullOrEmpty(responseJson))
            {
                _logger.LogWarning("InterviewEvaluationService: Gemini request failed - using fallback");
                return CreateFallbackResult();
            }

            var result = ParseEvaluationResponse(responseJson);
            return result ?? CreateFallbackResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InterviewEvaluationService: Error during evaluation - using fallback");
            return CreateFallbackResult();
        }
    }

    private static string BuildPrompt(string question, string answer, string? category, string? skillFocus)
    {
        return EvaluationPromptTemplate
            .Replace("{question}", question)
            .Replace("{category}", category ?? "General")
            .Replace("{skillFocus}", skillFocus ?? "Not specified")
            .Replace("{answer}", answer);
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
                    temperature = 0.3,
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
            _logger.LogWarning(ex, "InterviewEvaluationService: HTTP error");
            return null;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            _logger.LogWarning(ex, "InterviewEvaluationService: Timeout");
            return null;
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            _logger.LogWarning(ex, "InterviewEvaluationService: Request cancelled");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InterviewEvaluationService: Unexpected error in HTTP request");
            return null;
        }
    }

    private async Task<string?> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;

        if (statusCode == 429)
        {
            _logger.LogWarning("InterviewEvaluationService: Rate limited (429)");
            return null;
        }

        if (statusCode >= 500 && statusCode < 600)
        {
            _logger.LogWarning("InterviewEvaluationService: Server error ({StatusCode})", statusCode);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("InterviewEvaluationService: API error ({StatusCode})", statusCode);
            return null;
        }

        try
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return ExtractTextFromResponse(responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InterviewEvaluationService: Error reading response");
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
            return !string.IsNullOrEmpty(text) ? text : null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (KeyNotFoundException)
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

            if (result == null)
            {
                return TryParseManually(cleanedText);
            }

            return NormalizeResult(result);
        }
        catch (JsonException)
        {
            return TryParseManually(responseBody);
        }
        catch (Exception)
        {
            return TryParseManually(responseBody);
        }
    }

    private static EvaluationResultDto NormalizeResult(GeminiEvaluationResponseDto result)
    {
        return new EvaluationResultDto
        {
            Clarity = NormalizeScore(result.Clarity),
            Structure = NormalizeScore(result.Structure),
            Relevance = NormalizeScore(result.Relevance),
            Overall = NormalizeScore(result.Overall),
            Feedback = !string.IsNullOrWhiteSpace(result.Feedback) ? result.Feedback : "Good answer.",
            Improvement = !string.IsNullOrWhiteSpace(result.Improvement) ? result.Improvement : "Keep practicing."
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

            var clarity = ExtractScore(cleanedText, "clarity");
            var structure = ExtractScore(cleanedText, "structure");
            var relevance = ExtractScore(cleanedText, "relevance");
            var overall = ExtractScore(cleanedText, "overall");

            if (clarity == 0 && structure == 0 && relevance == 0)
            {
                return null;
            }

            return new EvaluationResultDto
            {
                Clarity = clarity,
                Structure = structure,
                Relevance = relevance,
                Overall = overall,
                Feedback = ExtractString(cleanedText, "feedback") is { } fb && !string.IsNullOrWhiteSpace(fb) ? fb : "Good answer.",
                Improvement = ExtractString(cleanedText, "improvement") is { } imp && !string.IsNullOrWhiteSpace(imp) ? imp : "Keep practicing."
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static decimal ExtractScore(string text, string fieldName)
    {
        var pattern = $@"""{fieldName}"":\s*(-?\d+(?:\.\d+)?)";
        var match = System.Text.RegularExpressions.Regex.Match(text, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var score))
        {
            return Math.Clamp(score, 0, 100);
        }

        return 0;
    }

    private static string ExtractString(string text, string fieldName)
    {
        var pattern = $@"""{fieldName}"":\s*""([^""\\]*(?:\\.[^""\\]*)*)""";
        var match = System.Text.RegularExpressions.Regex.Match(text, pattern, System.Text.RegularExpressions.RegexOptions.Singleline);

        if (match.Success)
        {
            return match.Groups[1].Value
                .Replace("\\\"", "\"")
                .Replace("\\n", "\n")
                .Replace("\\\\", "\\");
        }

        return string.Empty;
    }

    private static EvaluationResultDto CreateFallbackResult()
    {
        return new EvaluationResultDto
        {
            Clarity = 70,
            Structure = 70,
            Relevance = 70,
            Overall = 70,
            Feedback = "Evaluation service temporarily unavailable.",
            Improvement = "Try submitting again later."
        };
    }
}
