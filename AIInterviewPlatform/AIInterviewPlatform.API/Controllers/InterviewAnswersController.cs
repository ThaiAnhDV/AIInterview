using AIInterviewPlatform.Application.DTOs.InterviewAnswer;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class InterviewAnswersController : ControllerBase
    {
        private readonly IInterviewAnswerService _interviewAnswerService;

        public InterviewAnswersController(IInterviewAnswerService interviewAnswerService)
        {
            _interviewAnswerService = interviewAnswerService;
        }

        [HttpPost("interviews/{sessionId}/answers")]
        public async Task<IActionResult> SubmitAnswer(
            long sessionId,
            SubmitInterviewAnswerRequest request)
        {
            try
            {
                var userId = GetUserId();

                var result = await _interviewAnswerService
                    .SubmitAnswerAsync(userId, sessionId, request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("interviews/{sessionId}/answers")]
        public async Task<IActionResult> GetAnswersBySession(long sessionId)
        {
            try
            {
                var userId = GetUserId();

                var result = await _interviewAnswerService
                    .GetAnswersBySessionAsync(userId, sessionId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("answers/{answerId}")]
        public async Task<IActionResult> UpdateAnswer(
            long answerId,
            UpdateInterviewAnswerRequest request)
        {
            try
            {
                var userId = GetUserId();

                var result = await _interviewAnswerService
                    .UpdateAnswerAsync(userId, answerId, request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private long GetUserId()
        {
            return long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }
    }
}