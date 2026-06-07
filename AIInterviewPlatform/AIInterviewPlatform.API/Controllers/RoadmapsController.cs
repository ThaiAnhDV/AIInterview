using System;
using AIInterviewPlatform.Application.DTOs.Roadmap;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Obsolete("Legacy template-based roadmap generation. Use /api/Roadmap endpoints backed by RoadmapApplicationService.")]
    public class RoadmapsController : ControllerBase
    {
        private readonly ILearningRoadmapService _roadmapService;

        public RoadmapsController(
            ILearningRoadmapService roadmapService)
        {
            _roadmapService = roadmapService;
        }

        private long GetUserId()
        {
            return long.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRoadmap(
            GenerateRoadmapRequest request)
        {
            var result =
                await _roadmapService.GenerateRoadmapAsync(
                    GetUserId(),
                    request);

            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyRoadmaps()
        {
            var result =
                await _roadmapService.GetMyRoadmapsAsync(
                    GetUserId());

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoadmap(
            long id)
        {
            var result =
                await _roadmapService.GetRoadmapByIdAsync(
                    GetUserId(),
                    id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}