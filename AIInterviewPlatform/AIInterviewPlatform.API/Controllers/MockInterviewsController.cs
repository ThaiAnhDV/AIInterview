using AIInterviewPlatform.Application.DTOs.Interview.Enums;
using AIInterviewPlatform.Application.DTOs.Interview.Requests;
using AIInterviewPlatform.Application.DTOs.Interview.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class MockInterviewsController : ControllerBase
{
    private readonly IMockInterviewApplicationService _mockInterviewService;
    private readonly ILogger<MockInterviewsController> _logger;

    public MockInterviewsController(
        IMockInterviewApplicationService mockInterviewService,
        ILogger<MockInterviewsController> logger)
    {
        _mockInterviewService = mockInterviewService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a new mock interview session with AI-generated questions.
    /// </summary>
    /// <param name="request">The mock interview request containing target job and skill gap analysis IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated interview session with questions.</returns>
    /// <response code="200">Interview session generated successfully.</response>
    /// <response code="400">Invalid request or validation failed.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Target job or skill gap analysis not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(InterviewQuestionGenerationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateMockInterview(
        [FromBody] StartMockInterviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();

        _logger.LogInformation(
            "Generate mock interview request received. UserId: {UserId}, TargetJobId: {TargetJobId}, SkillGapAnalysisId: {SkillGapAnalysisId}",
            userId, request.TargetJobId, request.SkillGapAnalysisId);

        try
        {
            var result = await _mockInterviewService.StartMockInterviewAsync(
                userId,
                request,
                cancellationToken);

            if (result.Status == GenerationStatusEnum.Failed)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Resource Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return BadRequest(new ProblemDetails
                {
                    Title = "Interview Generation Failed",
                    Detail = result.ErrorMessage ?? "Failed to generate interview questions.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            _logger.LogInformation(
                "Mock interview generated successfully. SessionId: {SessionId}, Questions: {QuestionCount}",
                result.SessionId, result.Questions.Count);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Category"))
        {
            _logger.LogWarning(
                "Category validation failed. UserId: {UserId}, Error: {Error}",
                userId, ex.Message);

            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Category",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error generating mock interview. UserId: {UserId}",
                userId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while generating the interview.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Validate that the user has access to the specified resources.
    /// </summary>
    /// <param name="targetJobId">Target job ID.</param>
    /// <param name="skillGapAnalysisId">Skill gap analysis ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    [HttpPost("validate-access")]
    [ProducesResponseType(typeof(AccessValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateAccess(
        [FromQuery] long targetJobId,
        [FromQuery] long skillGapAnalysisId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var hasAccess = await _mockInterviewService.ValidateUserOwnsResourcesAsync(
            userId,
            targetJobId,
            skillGapAnalysisId,
            cancellationToken);

        return Ok(new AccessValidationResult
        {
            HasAccess = hasAccess,
            TargetJobId = targetJobId,
            SkillGapAnalysisId = skillGapAnalysisId
        });
    }

    private long GetUserId()
    {
        return long.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}

public class AccessValidationResult
{
    public bool HasAccess { get; init; }
    public long TargetJobId { get; init; }
    public long SkillGapAnalysisId { get; init; }
}
