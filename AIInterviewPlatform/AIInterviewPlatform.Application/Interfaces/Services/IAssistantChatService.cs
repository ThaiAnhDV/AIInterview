using AIInterviewPlatform.Application.DTOs.Assistant;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IAssistantChatService
{
    Task<AssistantChatResponse> AskAsync(
        AssistantChatRequest request,
        CancellationToken cancellationToken = default);
}
