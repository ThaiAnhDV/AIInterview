using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class ActivityDescriptionService : IActivityDescriptionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ActivityDescriptionService> _logger;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public ActivityDescriptionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ActivityDescriptionService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<ActivityDescriptionResponse?> GenerateActivityDescriptionAsync(string skillName)
    {
        return await GenerateActivityDescriptionAsync(skillName, "BEGINNER");
    }

    public async Task<ActivityDescriptionResponse?> GenerateActivityDescriptionAsync(
        string skillName, 
        string difficultyLevel)
    {
        if (string.IsNullOrWhiteSpace(skillName))
        {
            _logger.LogWarning("Skill name is empty");
            return null;
        }

        var prompt = BuildPrompt(skillName, difficultyLevel);
        
        try
        {
            var response = await SendGeminiRequestAsync(prompt);
            if (response == null)
            {
                _logger.LogWarning("Gemini request returned null for skill: {Skill}", skillName);
                return null;
            }
            
            return ParseGeminiResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate activity description for skill: {Skill}. Returning null for fallback.", skillName);
            return null;
        }
    }

    private static string BuildPrompt(string skillName, string difficultyLevel)
    {
        return $@"Generate ONE practical learning activity for a software engineer who wants to learn {skillName}.

            Requirements:
            - Beginner friendly (suitable for someone with basic programming knowledge)
            - Maximum 50 words for the description
            - Software engineering focus
            - Practical and hands-on

            Return ONLY a valid JSON object with this exact format (no markdown, no explanation):
            {{""activityTitle"": ""short action-oriented title"", ""activityDescription"": ""concise description under 50 words""}}

            Example activityTitle: ""Build a REST API Calculator"" or ""Create a Docker container""
            Example activityDescription: ""Create a simple calculator REST API using Express.js. Implement basic operations: add, subtract, multiply, divide. Test endpoints with Postman.""

            Skill: {skillName}
            Difficulty: {difficultyLevel}

            IMPORTANT: Return ONLY the JSON object.";
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
                maxOutputTokens = 200
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
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
                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.ValueKind == JsonValueKind.Array &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString() ?? string.Empty;
                    return ExtractActivityFromJsonText(text);
                }
            }

            _logger.LogWarning("Unexpected Gemini response format - no candidates found");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Gemini JSON response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing Gemini response");
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

        try
        {
            var result = JsonSerializer.Deserialize<ActivityDescriptionResponse>(cleanedText);
            if (result != null && !string.IsNullOrWhiteSpace(result.ActivityTitle))
            {
                result.ActivityDescription = TruncateToWords(result.ActivityDescription, 50);
                return result;
            }
        }
        catch (JsonException)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                cleanedText, 
                @"\{.*?""activityTitle""\s*:\s*""([^""]+)"".*?""activityDescription""\s*:\s*""([^""]+)"".*\}");

            if (match.Success)
            {
                var description = TruncateToWords(match.Groups[2].Value, 50);
                return new ActivityDescriptionResponse
                {
                    ActivityTitle = match.Groups[1].Value,
                    ActivityDescription = description
                };
            }
        }

        _logger.LogWarning("Could not extract activity from Gemini response text");
        return null;
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
