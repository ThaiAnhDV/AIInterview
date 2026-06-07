using AIInterviewPlatform.Application.DTOs.Interview.Gemini;
using AIInterviewPlatform.Application.DTOs.Interview.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.AspNetCore.Mvc;

namespace AIInterviewPlatform.API.Controllers;

[ApiController]
[Route("api/test")]
public class TestInterviewQuestionsController : ControllerBase
{
    private readonly IInterviewQuestionGeneratorService _questionGenerator;

    public TestInterviewQuestionsController(IInterviewQuestionGeneratorService questionGenerator)
    {
        _questionGenerator = questionGenerator;
    }

    [HttpPost("interview-questions")]
    public async Task<ActionResult<InterviewQuestionGenerationResult>> GenerateQuestions(
        [FromBody] TestInterviewQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _questionGenerator.GenerateQuestionsFromJobAsync(
            request.TargetJob,
            request.TargetJob,
            request.RequiredSkills,
            request.MissingSkills,
            request.LanguageCode,
            cancellationToken);

        return Ok(result);
    }
}

public class TestInterviewQuestionsRequest
{
    public string TargetJob { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = [];
    public List<string> MissingSkills { get; set; } = [];
    public string? LanguageCode { get; set; }
}
