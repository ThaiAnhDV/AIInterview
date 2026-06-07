using AIInterviewPlatform.Application.DTOs.AnswerEvaluation;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterviewPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnswerEvaluationController : ControllerBase
    {
        private readonly IAnswerEvaluationService _service;

        public AnswerEvaluationController(
            IAnswerEvaluationService service)
        {
            _service = service;
        }

        [HttpPost("{answerId}/evaluate")]
        public async Task<IActionResult> Evaluate(long answerId)
        {
            var result =
                await _service.EvaluateAnswerAsync(answerId);

            return Ok(result);
        }

        [HttpGet("{answerId}")]
        public async Task<IActionResult> Get(long answerId)
        {
            var result =
                await _service.GetEvaluationAsync(answerId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetSessionFeedback(
            long sessionId)
        {
            var result =
                await _service.GetSessionFeedbackAsync(sessionId);

            return Ok(result);
        }
    }
}
