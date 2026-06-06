using AIInterviewPlatform.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AIInterviewPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TestController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("skills")]
    public async Task<IActionResult> GetSkills()
    {
        var skills = await _unitOfWork.Skills.GetAllAsync();

        return Ok(skills);
    }

    [HttpGet("question-categories")]
    public async Task<IActionResult> GetQuestionCategories()
    {
        var categories = await _unitOfWork.QuestionCategories.GetAllAsync();

        return Ok(categories);
    }
}