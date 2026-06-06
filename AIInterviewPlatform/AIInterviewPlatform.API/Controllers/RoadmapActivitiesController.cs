using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoadmapActivitiesController : ControllerBase
    {
        private readonly ILearningRoadmapService _roadmapService;

        public RoadmapActivitiesController(
            ILearningRoadmapService roadmapService)
        {
            _roadmapService = roadmapService;
        }

        private long GetUserId()
        {
            return long.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteActivity(
            long id)
        {
            var success =
                await _roadmapService.CompleteActivityAsync(
                    GetUserId(),
                    id);

            if (!success)
            {
                return NotFound();
            }

            return Ok(new
            {
                message = "Activity completed."
            });
        }
    }
}