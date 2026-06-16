using AIInterviewPlatform.Application.DTOs.Recommendation;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    private long GetUserId()
    {
        return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<RecommendationResponse>>> GetMyRecommendations()
    {
        var userId = GetUserId();
        var recommendations = await _recommendationService.GetMyRecommendationsAsync(userId);
        return Ok(recommendations);
    }

    [HttpGet("analysis/{analysisId}")]
    public async Task<ActionResult<List<RecommendationResponse>>> GetRecommendationsByAnalysis(
        long analysisId)
    {
        var userId = GetUserId();
        var recommendations = await _recommendationService.GetRecommendationsByAnalysisIdAsync(userId, analysisId);
        return Ok(recommendations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecommendationResponse>> GetRecommendationById(long id)
    {
        var userId = GetUserId();
        var recommendation = await _recommendationService.GetRecommendationByIdAsync(userId, id);

        if (recommendation == null)
        {
            return NotFound(new { errorCode = "NOT_FOUND", message = "Recommendation not found." });
        }

        return Ok(recommendation);
    }
}
