using AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SkillGapAnalysisController : ControllerBase
    {
        private readonly ISkillGapAnalysisService _skillGapAnalysisService;
        private readonly ISkillGapAnalysisOrchestratorService _skillGapAnalysisOrchestratorService;

        public SkillGapAnalysisController(
            ISkillGapAnalysisService skillGapAnalysisService,
            ISkillGapAnalysisOrchestratorService skillGapAnalysisOrchestratorService)
        {
            _skillGapAnalysisService = skillGapAnalysisService;
            _skillGapAnalysisOrchestratorService = skillGapAnalysisOrchestratorService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze(
            CreateSkillGapAnalysisRequest request)
        {
            var userId = GetUserId();

            var result =
                await _skillGapAnalysisService.AnalyzeAsync(
                    userId,
                    request);

            return Ok(result);
        }

        [HttpPost("analyze-detailed")]
        public async Task<IActionResult> AnalyzeDetailed(
            ComprehensiveSkillGapAnalysisRequest request)
        {
            var userId = GetUserId();
            var result = await _skillGapAnalysisOrchestratorService.AnalyzeSkillGapAsync(userId, request);
            return Ok(result);
        }

        [HttpGet("my-analyses")]
        public async Task<IActionResult> GetMyAnalyses()
        {
            var userId = GetUserId();

            var result =
                await _skillGapAnalysisService
                    .GetMyAnalysesAsync(userId);

            return Ok(result);
        }

        [HttpGet("{analysisId}")]
        public async Task<IActionResult> GetById(
            long analysisId)
        {
            var userId = GetUserId();

            var result =
                await _skillGapAnalysisService
                    .GetByIdAsync(
                        userId,
                        analysisId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        private long GetUserId()
        {
            return long.Parse(
                User.FindFirst(
                    ClaimTypes.NameIdentifier)!.Value);
        }
    }
}