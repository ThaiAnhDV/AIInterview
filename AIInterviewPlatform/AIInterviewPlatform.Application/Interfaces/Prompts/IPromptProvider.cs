namespace AIInterviewPlatform.Application.Interfaces.Prompts;

public interface IPromptProvider
{
    string LanguageCode { get; }

    string BuildResumeSkillExtractionPrompt(string resumeContent);

    string BuildJobDescriptionSkillExtractionPrompt(string jobDescriptionContent);

    string BuildInterviewQuestionPrompt(
        string targetJobTitle,
        string targetJobDescription,
        IReadOnlyCollection<string> requiredSkills,
        IReadOnlyCollection<string> missingSkills);

    string BuildInterviewEvaluationPrompt(
        string question,
        string answer,
        string? category,
        string? skillFocus);

    string BuildActivityDescriptionPrompt(string skillName, string difficultyLevel);

    string BuildAssistantChatPrompt(string page, string message);

    string GetAssistantFallbackMessage(string page);

    string GetAssistantEmptyMessage();

    string GetAssistantMissingApiKeyMessage();
}
