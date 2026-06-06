using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIInterviewPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIConnectionTestService _connectionTestService;
    private readonly IAIFunctionValidationService _functionValidationService;

    public AIController(
        IAIConnectionTestService connectionTestService,
        IAIFunctionValidationService functionValidationService)
    {
        _connectionTestService = connectionTestService;
        _functionValidationService = functionValidationService;
    }

    [HttpGet("ping")]
    [ProducesResponseType(typeof(AIConnectionTestResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Test Gemini connectivity")]
    [EndpointDescription("Verify that Gemini API is reachable.")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken)
    {
        var result = await _connectionTestService.PingAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("validate")]
    [ProducesResponseType(typeof(AIFunctionValidationResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Validate AI functionality")]
    [EndpointDescription("Verify all AI business modules are working correctly.")]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken)
    {
        var result = await _functionValidationService.ValidateAsync(cancellationToken);
        return Ok(result);
    }
}
