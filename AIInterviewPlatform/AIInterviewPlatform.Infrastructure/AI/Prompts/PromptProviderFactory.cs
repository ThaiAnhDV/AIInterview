using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Domain.Common;

using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.AI.Prompts;

public sealed class PromptProviderFactory : IPromptProviderFactory
{
    private readonly IReadOnlyDictionary<string, IPromptProvider> _providers;
    private readonly ILogger<PromptProviderFactory> _logger;

    public PromptProviderFactory(
        IEnumerable<IPromptProvider> providers,
        ILogger<PromptProviderFactory> logger)
    {
        _providers = providers.ToDictionary(
            provider => SupportedLanguageCodes.NormalizeOrDefault(provider.LanguageCode),
            provider => provider,
            StringComparer.Ordinal);
        _logger = logger;
    }

    public IPromptProvider Get(string? languageCode)
    {
        var normalized = SupportedLanguageCodes.NormalizeOrDefault(languageCode);
        if (_providers.TryGetValue(normalized, out var provider))
        {
            _logger.LogInformation(
                "[LANG_AUDIT] PromptProviderFactory.Get Input={Input} Resolved={Resolved} Provider={Provider}",
                languageCode, normalized, provider.LanguageCode);
            return provider;
        }

        var fallback = _providers[SupportedLanguageCodes.Default];
        _logger.LogWarning(
            "[LANG_AUDIT] PromptProviderFactory.Get Input={Input} Resolved={Resolved} PROVIDER_NOT_FOUND UsingFallback={Fallback}",
            languageCode, normalized, fallback.LanguageCode);
        return fallback;
    }
}
