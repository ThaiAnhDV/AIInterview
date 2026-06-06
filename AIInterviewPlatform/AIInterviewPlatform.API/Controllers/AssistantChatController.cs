using AIInterviewPlatform.Application.DTOs.Assistant;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterviewPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AssistantChatController : ControllerBase
{
    private readonly IAssistantChatService _assistantChatService;

    public AssistantChatController(IAssistantChatService assistantChatService)
    {
        _assistantChatService = assistantChatService;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        AssistantChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new
            {
                message = "Vui lòng nhập câu hỏi."
            });
        }

        var result = await _assistantChatService.AskAsync(request, cancellationToken);
        return Ok(result);
    }
}
