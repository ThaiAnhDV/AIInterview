using AIInterviewPlatform.Application.DTOs.Interview.Gemini;
using AIInterviewPlatform.Application.DTOs.Interview.Responses;

namespace AIInterviewPlatform.Application.Interfaces.Services;

public interface IInterviewQuestionGeneratorService
{
    Task<InterviewQuestionGenerationResult> GenerateQuestionsAsync(
        GeminiInterviewQuestionRequest request,
        CancellationToken cancellationToken = default);

    Task<InterviewQuestionGenerationResult> GenerateQuestionsFromJobAsync(
        string targetJobTitle,
        string targetJobDescription,
        List<string> requiredSkills,
        List<string> missingSkills,
        CancellationToken cancellationToken = default);

    InterviewQuestionGenerationResult GetDefaultQuestions(
        string targetJob,
        List<string> requiredSkills,
        List<string> missingSkills);
}
