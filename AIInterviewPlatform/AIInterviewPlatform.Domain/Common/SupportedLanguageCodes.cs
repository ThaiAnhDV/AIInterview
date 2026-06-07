namespace AIInterviewPlatform.Domain.Common;

public static class SupportedLanguageCodes
{
    public const string Vietnamese = "vi";
    public const string English = "en";
    public const string Japanese = "ja";
    public const string Korean = "ko";
    public const string Chinese = "zh";

    public const string Default = Vietnamese;

    public static readonly string[] All =
    [
        Vietnamese,
        English,
        Japanese,
        Korean,
        Chinese
    ];

    public static bool IsSupported(string? languageCode)
    {
        return Normalize(languageCode) is not null;
    }

    public static string NormalizeOrDefault(string? languageCode)
    {
        return Normalize(languageCode) ?? Default;
    }

    public static string? Normalize(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return null;
        }

        var normalized = languageCode.Trim().ToLowerInvariant();
        return All.Contains(normalized, StringComparer.Ordinal) ? normalized : null;
    }
}
