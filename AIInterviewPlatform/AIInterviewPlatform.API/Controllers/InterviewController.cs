using AIInterviewPlatform.Application.DTOs.Interview;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewService _interviewService;

        public InterviewController(
            IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartInterview(
            StartInterviewRequest request)
        {
            var userId = GetUserId();

            var result =
                await _interviewService.StartInterviewAsync(
                    userId,
                    request);

            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMySessions()
        {
            var userId = GetUserId();

            var result =
                await _interviewService.GetMySessionsAsync(
                    userId);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSession(
            long id)
        {
            var userId = GetUserId();

            var result =
                await _interviewService.GetByIdAsync(
                    userId,
                    id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteSession(
            long id)
        {
            var userId = GetUserId();

            var result =
                await _interviewService.CompleteSessionAsync(
                    userId,
                    id);

            if (!result)
            {
                return NotFound();
            }

            return Ok(new
            {
                message = "Interview completed."
            });
        }

        private long GetUserId()
        {
            return long.Parse(
                User.FindFirst(
                    ClaimTypes.NameIdentifier)!.Value);
        }
    }
}