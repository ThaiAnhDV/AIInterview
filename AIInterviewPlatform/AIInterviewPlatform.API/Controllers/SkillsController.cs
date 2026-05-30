using AIInterviewPlatform.Application.DTOs.Skill;
using AIInterviewPlatform.Application.Interfaces.Services;
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
        await _skillService
            .ExtractSkillsFromJobDescriptionAsync(
                jobDescriptionId);

        return Ok();
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