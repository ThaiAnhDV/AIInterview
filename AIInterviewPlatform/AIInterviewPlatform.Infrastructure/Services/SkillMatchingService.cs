using System.Text.RegularExpressions;

using AIInterviewPlatform.Application.Common;
using AIInterviewPlatform.Application.DTOs.SkillMatching;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class SkillMatchingService : ISkillMatchingService
{
    private const double FuzzyMatchThreshold = 0.55;
    private readonly ILogger<SkillMatchingService> _logger;

    public SkillMatchingService(ILogger<SkillMatchingService> logger)
    {
        _logger = logger;
    }

    public SkillMatchResult MatchSkills(SkillMatchRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("SkillMatchRequest is null");
            return CreateEmptyResult();
        }

        var normalizedResumeSkills = request.ResumeSkills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(skill => CreateSkillEntry(skill))
            .DistinctBy(skill => skill.Normalized)
            .ToList();

        var normalizedRequiredSkills = request.RequiredSkills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(skill => CreateSkillEntry(skill))
            .DistinctBy(skill => skill.Normalized)
            .ToList();

        var matchedSkills = new List<string>();
        var missingSkills = new List<string>();
        var matchedResumeSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var required in normalizedRequiredSkills)
        {
            SkillEntry? bestResumeMatch = null;
            var bestScore = 0d;

            foreach (var resume in normalizedResumeSkills)
            {
                var score = CalculateMatchScore(resume, required);

                _logger.LogInformation("[SkillMatch] ResumeSkill={ResumeSkill}", resume.Original);
                _logger.LogInformation("[SkillMatch] JDSkill={JDSkill}", required.Original);
                _logger.LogInformation("[SkillMatch] MatchScore={MatchScore}", Math.Round(score, 4));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestResumeMatch = resume;
                }
            }

            if (bestResumeMatch != null && bestScore >= FuzzyMatchThreshold)
            {
                matchedSkills.Add(required.Original);
                matchedResumeSkills.Add(bestResumeMatch.Original);
            }
            else
            {
                missingSkills.Add(required.Original);
            }
        }

        var unmatchedResumeSkills = normalizedResumeSkills
            .Where(skill => !matchedResumeSkills.Contains(skill.Original))
            .Select(skill => skill.Original)
            .ToList();

        var matchPercentage = CalculateMatchPercentage(matchedSkills.Count, normalizedRequiredSkills.Count);
        var readinessScore = CalculateReadinessScore(matchPercentage, normalizedResumeSkills.Count, normalizedRequiredSkills.Count);

        _logger.LogInformation(
            "Skill matching completed: {MatchedCount}/{TotalRequired} matched ({MatchPercentage:F1}%)",
            matchedSkills.Count,
            normalizedRequiredSkills.Count,
            matchPercentage);

        return new SkillMatchResult
        {
            MatchedSkills = matchedSkills,
            MissingSkills = missingSkills,
            UnmatchedResumeSkills = unmatchedResumeSkills,
            MatchPercentage = Math.Round(matchPercentage, 2),
            ReadinessScore = Math.Round(readinessScore, 2)
        };
    }

    private static SkillEntry CreateSkillEntry(string skill)
    {
        var canonical = SkillNormalizer.Normalize(skill);
        var normalized = NormalizeForMatching(canonical);
        var tokens = Tokenize(normalized);

        return new SkillEntry(skill.Trim(), canonical, normalized, tokens);
    }

    private static double CalculateMatchScore(SkillEntry resume, SkillEntry required)
    {
        if (string.IsNullOrWhiteSpace(resume.Normalized) || string.IsNullOrWhiteSpace(required.Normalized))
        {
            return 0;
        }

        if (string.Equals(resume.Canonical, required.Canonical, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(resume.Normalized, required.Normalized, StringComparison.Ordinal))
        {
            return 1.0;
        }

        var tokenOverlap = CalculateTokenOverlap(resume.Tokens, required.Tokens);
        var containsBoost = IsContainmentMatch(resume.Normalized, required.Normalized) ? 0.9 : 0;
        var similarity = SkillNormalizer.CalculateSimilarity(resume.Normalized, required.Normalized);

        return Math.Max(containsBoost, Math.Max(tokenOverlap, similarity));
    }

    private static bool IsContainmentMatch(string left, string right)
    {
        return left.Contains(right, StringComparison.Ordinal) || right.Contains(left, StringComparison.Ordinal);
    }

    private static double CalculateTokenOverlap(IReadOnlyCollection<string> leftTokens, IReadOnlyCollection<string> rightTokens)
    {
        if (leftTokens.Count == 0 || rightTokens.Count == 0)
        {
            return 0;
        }

        var intersectionCount = leftTokens.Intersect(rightTokens, StringComparer.Ordinal).Count();
        if (intersectionCount == 0)
        {
            return 0;
        }

        var denominator = Math.Min(leftTokens.Count, rightTokens.Count);
        return (double)intersectionCount / denominator;
    }

    private static string NormalizeForMatching(string skill)
    {
        var lower = skill.Trim().ToLowerInvariant();
        var withoutPunctuation = Regex.Replace(lower, @"[^\p{L}\p{Nd}\s]", " ");
        var collapsedSpaces = Regex.Replace(withoutPunctuation, @"\s+", " ");
        return collapsedSpaces.Trim();
    }

    private static IReadOnlyCollection<string> Tokenize(string skill)
    {
        return skill.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static double CalculateMatchPercentage(int matchedCount, int totalRequired)
    {
        if (totalRequired == 0)
        {
            return 0;
        }

        return (double)matchedCount / totalRequired * 100;
    }

    private static double CalculateReadinessScore(
        double matchPercentage,
        int resumeSkillsCount,
        int requiredSkillsCount)
    {
        if (requiredSkillsCount == 0)
        {
            return 0;
        }

        double baseScore = matchPercentage;

        double relevanceFactor = 1.0;

        if (resumeSkillsCount > 0 && requiredSkillsCount > 0)
        {
            var ratio = (double)requiredSkillsCount / resumeSkillsCount;

            if (ratio >= 0.5 && ratio <= 1.5)
            {
                relevanceFactor = 1.0;
            }
            else if (ratio < 0.5)
            {
                relevanceFactor = 0.7 + (ratio * 0.6);
            }
            else
            {
                relevanceFactor = 1.0 - Math.Min(0.3, (ratio - 1.5) * 0.1);
            }
        }

        double proficiencyBonus = 0;
        if (matchPercentage >= 80)
        {
            proficiencyBonus = 5;
        }
        else if (matchPercentage >= 60)
        {
            proficiencyBonus = 2;
        }

        var rawScore = baseScore * relevanceFactor + proficiencyBonus;

        return Math.Min(100, Math.Max(0, rawScore));
    }

    private static SkillMatchResult CreateEmptyResult()
    {
        return new SkillMatchResult
        {
            MatchedSkills = [],
            MissingSkills = [],
            UnmatchedResumeSkills = [],
            MatchPercentage = 0,
            ReadinessScore = 0
        };
    }

    private sealed record SkillEntry(
        string Original,
        string Canonical,
        string Normalized,
        IReadOnlyCollection<string> Tokens);
}
