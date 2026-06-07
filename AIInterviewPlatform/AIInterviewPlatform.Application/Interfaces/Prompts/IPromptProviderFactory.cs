namespace AIInterviewPlatform.Application.Interfaces.Prompts;

public interface IPromptProviderFactory
{
    IPromptProvider Get(string? languageCode);
}
