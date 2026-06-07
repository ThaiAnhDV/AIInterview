using AIInterviewPlatform.Application.DTOs.Skill;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(
            await _skillService.GetAllSkillsAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateSkillRequest request)
    {
        return Ok(
            await _skillService.CreateSkillAsync(request));
    }

    [HttpPost("extract/{jobDescriptionId}")]
    public async Task<IActionResult> Extract(
        long jobDescriptionId)
    {
        var result = await _skillService
            .ExtractSkillsFromJobDescriptionAsync(
                jobDescriptionId);

        if (!result.Success)
        {
            if (result.ErrorMessage == "Job Description not found")
            {
                return NotFound(result);
            }

            return StatusCode(
                result.Diagnostics.HttpStatus ?? StatusCodes.Status502BadGateway,
                result);
        }

        return Ok(result);
    }

    [HttpGet("required/{jobDescriptionId}")]
    public async Task<IActionResult> RequiredSkills(
        long jobDescriptionId)
    {
        return Ok(
            await _skillService
                .GetRequiredSkillsByJobDescriptionAsync(
                    jobDescriptionId));
    }
}
