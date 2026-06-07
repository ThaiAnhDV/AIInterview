using AIInterviewPlatform.Application.DTOs.Assistant;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AssistantChatController : ControllerBase
{
    private readonly IAssistantChatService _assistantChatService;
    private readonly ILogger<AssistantChatController> _logger;

    public AssistantChatController(
        IAssistantChatService assistantChatService,
        ILogger<AssistantChatController> logger)
    {
        _assistantChatService = assistantChatService;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        AssistantChatRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[CONTROLLER] Received POST /api/AssistantChat/message");
        _logger.LogInformation("[CONTROLLER] Request: Message={Message}, Page={Page}, LanguageCode={LangCode}",
            request?.Message, request?.Page, request?.LanguageCode);

        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            _logger.LogWarning("[CONTROLLER] Returning 400 - Empty or null request");
            return BadRequest(new
            {
                message = "Vui lòng nhập câu hỏi."
            });
        }

        request.IsAdmin = User.FindFirstValue(ClaimTypes.Role) == "ADMIN";
        _logger.LogInformation("[CONTROLLER] IsAdmin={IsAdmin}", request.IsAdmin);

        _logger.LogInformation("[CONTROLLER] Calling service AskAsync...");
        var result = await _assistantChatService.AskAsync(request, cancellationToken);
        _logger.LogInformation("[CONTROLLER] Service returned - IsFallback={IsFallback}, ReplyLength={Len}",
            result.IsFallback, result.Reply?.Length ?? 0);

        _logger.LogInformation("[CONTROLLER] Returning 200 OK with response");
        return Ok(result);
    }
}
