using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.Assistant;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class AssistantChatService : IAssistantChatService
{
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private const string ModelName = "gemini-2.0-flash";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<AssistantChatService> _logger;

    public AssistantChatService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AssistantChatService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<AssistantChatResponse> AskAsync(
        AssistantChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = request.Message.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            return new AssistantChatResponse
            {
                Reply = "Bạn hãy nhập câu hỏi để trợ lý có thể hỗ trợ nhé.",
                IsFallback = true,
                Model = "fallback"
            };
        }

        if (string.IsNullOrWhiteSpace(_apiKey) ||
            _apiKey.Equals("YOUR_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            return request.IsAdmin
                ? CreateMissingApiKeyResponse()
                : CreateFallbackResponse(request);
        }

        var prompt = BuildPrompt(request);

        try
        {
            using var httpContent = new StringContent(
                JsonSerializer.Serialize(new
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
                }),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Assistant Gemini request failed: {StatusCode} {Body}",
                    response.StatusCode,
                    responseBody.Length > 300 ? responseBody[..300] : responseBody);

                return CreateFallbackResponse(request);
            }

            var reply = ExtractText(responseBody);
            if (string.IsNullOrWhiteSpace(reply))
            {
                _logger.LogWarning("Assistant Gemini response did not contain text.");
                return CreateFallbackResponse(request);
            }

            return new AssistantChatResponse
            {
                Reply = reply.Trim(),
                IsFallback = false,
                Model = ModelName
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Assistant Gemini request failed. Returning fallback response.");
            return CreateFallbackResponse(request);
        }
    }

    private static string BuildPrompt(AssistantChatRequest request)
    {
        var page = string.IsNullOrWhiteSpace(request.Page) ? "không xác định" : request.Page.Trim();

        return
            "Bạn là trợ lý AI thân thiện trong nền tảng luyện phỏng vấn AI.\n" +
            "Nhiệm vụ: giải thích ngắn gọn, dễ hiểu, bằng tiếng Việt, giúp người dùng biết nên làm gì tiếp theo trong sản phẩm.\n" +
            "Không hỏi thông tin nhạy cảm, không bịa dữ liệu cá nhân, không trả lời như đang phỏng vấn trực tiếp.\n" +
            "Nếu câu hỏi không liên quan nền tảng, vẫn hỗ trợ ngắn gọn nhưng ưu tiên hướng người dùng quay lại mục tiêu luyện phỏng vấn.\n\n" +
            $"Trang hiện tại: {page}\n" +
            $"Câu hỏi người dùng: {request.Message.Trim()}\n\n" +
            "Trả lời trong 2-5 câu, có thể đưa 1-3 bước cụ thể nếu phù hợp.";
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

    private static AssistantChatResponse CreateFallbackResponse(AssistantChatRequest request)
    {
        var page = (request.Page ?? string.Empty).ToLowerInvariant();
        var reply = "Hiện tại AI chưa kết nối được, nhưng bạn có thể tiếp tục theo luồng chính: tải hồ sơ, tạo công việc mục tiêu và JD, phân tích kỹ năng, rồi luyện phỏng vấn.";

        if (page.Contains("resume"))
        {
            reply = "Bạn đang ở phần hồ sơ. Hãy tải CV lên trước, sau đó chọn hồ sơ chính để hệ thống có dữ liệu phân tích kỹ năng.";
        }
        else if (page.Contains("target"))
        {
            reply = "Bạn đang ở phần công việc mục tiêu. Hãy tạo vị trí muốn ứng tuyển, rồi thêm JD để AI biết yêu cầu kỹ năng cần so sánh.";
        }
        else if (page.Contains("skill"))
        {
            reply = "Bạn đang ở phần phân tích kỹ năng. Hãy chọn hồ sơ và công việc đã có JD để xem kỹ năng phù hợp, kỹ năng còn thiếu và hướng ưu tiên học tập.";
        }
        else if (page.Contains("roadmap"))
        {
            reply = "Bạn đang ở phần lộ trình. Hãy chọn kết quả phân tích kỹ năng để tạo kế hoạch học tập cá nhân hóa và theo dõi tiến độ từng hoạt động.";
        }

        return new AssistantChatResponse
        {
            Reply = reply,
            IsFallback = true,
            Model = "fallback"
        };
    }

    private static AssistantChatResponse CreateMissingApiKeyResponse()
    {
        return new AssistantChatResponse
        {
            Reply = "Trợ lý AI chưa kết nối được vì Gemini API key chưa được cấu hình. Hãy thêm API key thật vào GeminiSettings:ApiKey rồi khởi động lại API.",
            IsFallback = true,
            Model = "fallback"
        };
    }
}
