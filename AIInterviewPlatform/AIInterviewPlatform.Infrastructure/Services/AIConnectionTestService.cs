using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class AIConnectionTestService : IAIConnectionTestService
{
    private const string Model = "gemini-2.5-flash";
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<AIConnectionTestService> _logger;

    public AIConnectionTestService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AIConnectionTestService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<AIConnectionTestResponse> PingAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            const string message = "Gemini API key is not configured.";
            _logger.LogError("AI ping failed. ExceptionType={ExceptionType}. GeminiMessage={GeminiMessage}",
                "ConfigurationException",
                message);

            return new AIConnectionTestResponse
            {
                Success = false,
                Connected = false,
                Model = Model,
                ResponseTimeMs = 0,
                ErrorType = "ConfigurationException",
                GeminiMessage = message,
                Message = message
            };
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = "Reply only with OK" }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 10,
                responseMimeType = "text/plain"
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            var geminiMessage = ExtractGeminiErrorMessage(responseBody);
            var responseText = ExtractCandidateText(responseBody);
            var connected = response.IsSuccessStatusCode &&
                            !string.IsNullOrWhiteSpace(responseText) &&
                            responseText.Contains("OK", StringComparison.OrdinalIgnoreCase);

            if (!connected)
            {
                _logger.LogError(
                    "AI ping failed. ExceptionType={ExceptionType}. HttpStatus={HttpStatus}. ResponseBody={ResponseBody}. GeminiMessage={GeminiMessage}",
                    "GeminiApiError",
                    (int)response.StatusCode,
                    responseBody,
                    geminiMessage ?? responseText ?? "No Gemini message returned");
            }

            return new AIConnectionTestResponse
            {
                Success = connected,
                Connected = connected,
                Model = Model,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorType = connected ? null : "GeminiApiError",
                HttpStatus = (int)response.StatusCode,
                GeminiMessage = connected ? null : geminiMessage ?? responseText,
                ResponseBody = connected ? null : responseBody,
                Message = connected
                    ? "Gemini connection successful"
                    : geminiMessage ?? responseText ?? "Unable to connect to Gemini"
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();

            int? statusCode = ex.StatusCode.HasValue
    ? (int)ex.StatusCode.Value
    : null;
            _logger.LogError(ex,
                "AI ping HTTP request failed. ExceptionType={ExceptionType}. HttpStatus={HttpStatus}. GeminiMessage={GeminiMessage}",
                ex.GetType().Name,
                statusCode,
                ex.Message);

            return new AIConnectionTestResponse
            {
                Success = false,
                Connected = false,
                Model = Model,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorType = ex.GetType().Name,
                HttpStatus = statusCode,
                GeminiMessage = ex.Message,
                Message = ex.Message
            };
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            const string message = "Gemini request timed out.";

            _logger.LogError(ex,
                "AI ping timed out. ExceptionType={ExceptionType}. GeminiMessage={GeminiMessage}",
                ex.GetType().Name,
                message);

            return new AIConnectionTestResponse
            {
                Success = false,
                Connected = false,
                Model = Model,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorType = ex.GetType().Name,
                GeminiMessage = message,
                Message = message
            };
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            const string message = "Gemini request was cancelled.";

            _logger.LogError(ex,
                "AI ping cancelled. ExceptionType={ExceptionType}. GeminiMessage={GeminiMessage}",
                ex.GetType().Name,
                message);

            return new AIConnectionTestResponse
            {
                Success = false,
                Connected = false,
                Model = Model,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorType = ex.GetType().Name,
                GeminiMessage = message,
                Message = message
            };
        }
    }

    private static string? ExtractGeminiErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("error", out var error))
            {
                if (error.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetString();
                }

                if (error.TryGetProperty("status", out var statusElement))
                {
                    return statusElement.GetString();
                }
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ExtractCandidateText(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

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

            return parts[0].TryGetProperty("text", out var textElement)
                ? textElement.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
