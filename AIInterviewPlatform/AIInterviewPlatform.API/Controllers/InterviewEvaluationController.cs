using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.Interview.Evaluation;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterviewPlatform.API.Controllers;

[ApiController]
[Route("api/interviews")]
[Authorize]
[Produces("application/json")]
public class InterviewEvaluationController : ControllerBase
{
    private readonly IInterviewEvaluationApplicationService _evaluationService;
    private readonly ILogger<InterviewEvaluationController> _logger;

    public InterviewEvaluationController(
        IInterviewEvaluationApplicationService evaluationService,
        ILogger<InterviewEvaluationController> logger)
    {
        _evaluationService = evaluationService;
        _logger = logger;
    }

    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EvaluationResponse>> EvaluateAnswer(
        [FromBody] EvaluateInterviewAnswerRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _evaluationService.EvaluateAnswerAsync(
                request.AnswerId,
                request.LanguageCode,
                cancellationToken);

            var response = new EvaluationResponse
            {
                AnswerId = request.AnswerId,
                Success = result.Success,
                ErrorCode = result.ErrorCode,
                Clarity = result.Clarity,
                TechnicalAccuracy = result.TechnicalAccuracy,
                Completeness = result.Completeness,
                Overall = result.Overall,
                Strengths = result.Strengths,
                Weaknesses = result.Weaknesses,
                Feedback = result.Feedback,
                IsFallback = result.IsFallback,
                AiUsed = result.AiUsed,
                GeneratedBy = result.GeneratedBy,
                ErrorMessage = result.ErrorMessage,
                Message = result.Message ?? (result.Success ? "Evaluation completed successfully." : "Evaluation failed")
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during evaluation for answer {AnswerId}", request.AnswerId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred during evaluation.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
