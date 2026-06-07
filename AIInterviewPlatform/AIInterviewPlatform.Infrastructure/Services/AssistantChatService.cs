using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.Assistant;
using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class AssistantChatService : IAssistantChatService
{
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    private const string ModelName = "gemini-2.5-flash";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<AssistantChatService> _logger;
    private readonly IPromptProviderFactory _promptProviderFactory;

    public AssistantChatService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AssistantChatService> logger,
        IPromptProviderFactory promptProviderFactory)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
        _promptProviderFactory = promptProviderFactory;
    }

    public async Task<AssistantChatResponse> AskAsync(
        AssistantChatRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[ASSISTANT_AUDIT] ========================================");
        _logger.LogInformation("[ASSISTANT_AUDIT] REQUEST RECEIVED");
        _logger.LogInformation("[ASSISTANT_AUDIT] RawLanguageCode: {RawLangCode}", request.LanguageCode ?? "(null)");
        _logger.LogInformation("[ASSISTANT_AUDIT] Message: {Message}", request.Message);
        _logger.LogInformation("[ASSISTANT_AUDIT] Page: {Page}", request.Page);
        _logger.LogInformation("[ASSISTANT_AUDIT] IsAdmin: {IsAdmin}", request.IsAdmin);

        var languageCode = SupportedLanguageCodes.NormalizeOrDefault(request.LanguageCode);
        _logger.LogInformation("[ASSISTANT_AUDIT] ResolvedLanguageCode: {ResolvedLang}", languageCode);

        var promptProvider = _promptProviderFactory.Get(languageCode);
        _logger.LogInformation("[ASSISTANT_AUDIT] Provider: {ProviderType}", promptProvider.GetType().Name);

        var message = request.Message.Trim();
        _logger.LogInformation("[ASSISTANT_AUDIT] Trimmed Message: {TrimmedMsg}", message);

        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("[ASSISTANT_AUDIT] Empty message - returning fallback");
            return new AssistantChatResponse
            {
                Reply = promptProvider.GetAssistantEmptyMessage(),
                IsFallback = true,
                Model = "fallback",
                LanguageCode = languageCode
            };
        }

        _logger.LogInformation("[ASSISTANT_AUDIT] API Key loaded: {HasKey}", !string.IsNullOrWhiteSpace(_apiKey));
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("[ASSISTANT_AUDIT] API Key is NULL or EMPTY");
        }
        else if (_apiKey.Equals("YOUR_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[ASSISTANT_AUDIT] API Key is placeholder 'YOUR_API_KEY'");
        }
        else
        {
            _logger.LogInformation("[ASSISTANT_AUDIT] API Key (first 10 chars): {KeyPrefix}...", _apiKey[..Math.Min(10, _apiKey.Length)]);
        }

        if (string.IsNullOrWhiteSpace(_apiKey) ||
            _apiKey.Equals("YOUR_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[ASSISTANT_AUDIT] Invalid API Key - returning fallback response");
            return request.IsAdmin
                ? CreateMissingApiKeyResponse(promptProvider, languageCode)
                : CreateFallbackResponse(request, promptProvider, languageCode);
        }

        var page = string.IsNullOrWhiteSpace(request.Page)
            ? (languageCode == SupportedLanguageCodes.English ? "unknown" : "không xác định")
            : request.Page.Trim();

        _logger.LogInformation("[ASSISTANT_AUDIT] Building prompt for page: {Page}", page);

        var prompt = promptProvider.BuildAssistantChatPrompt(page, message);
        _logger.LogInformation("[ASSISTANT_AUDIT] ========== GENERATED PROMPT ==========");
        _logger.LogInformation("[ASSISTANT_AUDIT] Prompt: {Prompt}", prompt);
        _logger.LogInformation("[ASSISTANT_AUDIT] ========================================");

        try
        {
            var requestPayload = new
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
                    temperature = 0.45,
                    maxOutputTokens = 900
                }
            };

            var requestJson = JsonSerializer.Serialize(requestPayload);
            _logger.LogInformation("[ASSISTANT_AUDIT] ========== REQUEST PAYLOAD ==========");
            _logger.LogInformation("[ASSISTANT_AUDIT] RequestJson: {RequestJson}", requestJson);
            _logger.LogInformation("[ASSISTANT_AUDIT] ========================================");

            using var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var apiEndpoint = $"{GeminiApiUrl}?key={_apiKey[..10]}...";
            _logger.LogInformation("[ASSISTANT_AUDIT] API Endpoint: {Endpoint}", apiEndpoint);
            _logger.LogInformation("[ASSISTANT_AUDIT] Model: {Model}", ModelName);

            _logger.LogInformation("[ASSISTANT_AUDIT] Sending HTTP POST to Gemini API...");
            var startTime = DateTime.UtcNow;

            using var response = await _httpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent,
                cancellationToken);

            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation("[ASSISTANT_AUDIT] HTTP Request completed in {ElapsedMs}ms", elapsed.TotalMilliseconds);
            _logger.LogInformation("[ASSISTANT_AUDIT] HTTP Status Code: {StatusCode}", (int)response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("[ASSISTANT_AUDIT] ========== RAW GEMINI RESPONSE ==========");
            _logger.LogInformation("[ASSISTANT_AUDIT] RawResponse: {RawResponse}", responseBody);
            _logger.LogInformation("[ASSISTANT_AUDIT] ========================================");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "[ASSISTANT_AUDIT] ==================== HTTP FAILURE ====================");
                _logger.LogError(
                    "[ASSISTANT_AUDIT] HTTP Status Code: {StatusCode} ({StatusText})",
                    (int)response.StatusCode,
                    response.StatusCode);
                _logger.LogError(
                    "[ASSISTANT_AUDIT] Response Body: {Body}",
                    responseBody.Length > 500 ? responseBody[..500] + "..." : responseBody);
                _logger.LogWarning(
                    "[ASSISTANT_AUDIT] Gemini request FAILED: {StatusCode}",
                    (int)response.StatusCode);

                return CreateFallbackResponse(request, promptProvider, languageCode);
            }

            _logger.LogInformation("[ASSISTANT_AUDIT] Parsing JSON response...");
            var reply = ExtractText(responseBody);

            _logger.LogInformation("[ASSISTANT_AUDIT] ========== PARSED RESPONSE ==========");
            _logger.LogInformation("[ASSISTANT_AUDIT] Extracted Text Length: {TextLen}", reply?.Length ?? 0);
            _logger.LogInformation("[ASSISTANT_AUDIT] Extracted Text: {Text}", reply ?? "(null)");
            _logger.LogInformation("[ASSISTANT_AUDIT] ========================================");

            if (string.IsNullOrWhiteSpace(reply))
            {
                _logger.LogWarning("[ASSISTANT_AUDIT] Parsed text is empty - returning fallback");
                return CreateFallbackResponse(request, promptProvider, languageCode);
            }

            var finalResponse = new AssistantChatResponse
            {
                Reply = reply.Trim(),
                IsFallback = false,
                Model = ModelName,
                LanguageCode = languageCode
            };

            _logger.LogInformation("[ASSISTANT_AUDIT] ========== FINAL API RESPONSE ==========");
            _logger.LogInformation("[ASSISTANT_AUDIT] Reply: {Reply}", finalResponse.Reply);
            _logger.LogInformation("[ASSISTANT_AUDIT] IsFallback: {IsFallback}", finalResponse.IsFallback);
            _logger.LogInformation("[ASSISTANT_AUDIT] Model: {Model}", finalResponse.Model);
            _logger.LogInformation("[ASSISTANT_AUDIT] LanguageCode: {LangCode}", finalResponse.LanguageCode);
            _logger.LogInformation("[ASSISTANT_AUDIT] ========================================");
            _logger.LogInformation("[ASSISTANT_AUDIT] EXECUTION COMPLETE - Returning 200 OK");

            return finalResponse;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogError(ex, "[ASSISTANT_AUDIT] ==================== AI FAILURE ====================");
            _logger.LogError(ex, "[ASSISTANT_AUDIT] EXCEPTION TYPE: {ExceptionType}", ex.GetType().FullName);
            _logger.LogError(ex, "[ASSISTANT_AUDIT] EXCEPTION MESSAGE: {Message}", ex.Message);
            _logger.LogError(ex, "[ASSISTANT_AUDIT] INNER EXCEPTION: {Inner}", ex.InnerException?.Message ?? "(none)");
            _logger.LogError(ex, "[ASSISTANT_AUDIT] STACK TRACE: {StackTrace}", ex.StackTrace);
            _logger.LogError(ex, "[ASSISTANT_AUDIT] Full exception details logged above");
            _logger.LogWarning("[ASSISTANT_AUDIT] Returning fallback response due to exception");
            return CreateFallbackResponse(request, promptProvider, languageCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ASSISTANT_AUDIT] ==================== UNEXPECTED AI FAILURE ====================");
            _logger.LogError(ex, "[ASSISTANT_AUDIT] EXCEPTION TYPE: {ExceptionType}", ex.GetType().FullName);
            _logger.LogError(ex, "[ASSISTANT_AUDIT] EXCEPTION MESSAGE: {Message}", ex.Message);
            _logger.LogError(ex, "[ASSISTANT_AUDIT] STACK TRACE: {StackTrace}", ex.StackTrace);
            _logger.LogWarning("[ASSISTANT_AUDIT] Returning fallback response due to unexpected exception");
            return CreateFallbackResponse(request, promptProvider, languageCode);
        }
    }

    private static string ExtractText(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array ||
            parts.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        return parts[0].TryGetProperty("text", out var textElement)
            ? textElement.GetString() ?? string.Empty
            : string.Empty;
    }

    private static AssistantChatResponse CreateFallbackResponse(
        AssistantChatRequest request,
        IPromptProvider promptProvider,
        string languageCode)
    {
        return new AssistantChatResponse
        {
            Reply = promptProvider.GetAssistantFallbackMessage(request.Page ?? string.Empty),
            IsFallback = true,
            Model = "fallback",
            LanguageCode = languageCode
        };
    }

    private static AssistantChatResponse CreateMissingApiKeyResponse(
        IPromptProvider promptProvider,
        string languageCode)
    {
        return new AssistantChatResponse
        {
            Reply = promptProvider.GetAssistantMissingApiKeyMessage(),
            IsFallback = true,
            Model = "fallback",
            LanguageCode = languageCode
        };
    }
}
