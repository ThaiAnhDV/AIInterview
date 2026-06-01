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

    /// <summary>
    /// Evaluate an interview answer using AI.
    /// </summary>
    /// <param name="request">The evaluation request containing the answer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Evaluation results including scores and feedback.</returns>
    /// <response code="200">Evaluation completed successfully.</response>
    /// <response code="400">Invalid request or answer already evaluated.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Answer or question not found.</response>
    /// <response code="500">Internal server error.</response>
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
                cancellationToken);

            var response = new EvaluationResponse
            {
                AnswerId = request.AnswerId,
                Clarity = result.Clarity,
                Structure = result.Structure,
                Relevance = result.Relevance,
                Overall = result.Overall,
                Feedback = result.Feedback,
                Improvement = result.Improvement,
                Message = "Evaluation completed successfully."
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Evaluation failed for answer {AnswerId}", request.AnswerId);
            return BadRequest(new ProblemDetails
            {
                Title = "Evaluation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to answer {AnswerId}", request.AnswerId);
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            });
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
