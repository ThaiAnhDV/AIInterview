using System.Text.RegularExpressions;

using AIInterviewPlatform.Application.Common;
using AIInterviewPlatform.Application.DTOs.SkillMatching;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class SkillMatchingService : ISkillMatchingService
{
    private const double FuzzyMatchThreshold = 0.60;
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
        var matchedSkillsWithScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
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
                matchedSkillsWithScores[required.Original] = bestScore;
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
            MatchedSkillsWithScores = matchedSkillsWithScores,
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

        // Exact match (canonical or normalized)
        if (string.Equals(resume.Canonical, required.Canonical, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(resume.Normalized, required.Normalized, StringComparison.Ordinal))
        {
            return 1.0;
        }

        // Skill Hierarchy Check
        // Resume has CHILD skill, Required has PARENT skill => MATCH (candidate knows specific)
        // Resume has PARENT skill, Required has CHILD skill => NO MATCH (candidate doesn't know specific)
        var hierarchyResult = CheckSkillHierarchy(resume.Canonical, required.Canonical);
        if (hierarchyResult.HasValue)
        {
            return hierarchyResult.Value;
        }

        // Unidirectional Containment: Only resume contains required
        // e.g., "SQL Server" contains "SQL" => candidate knows more than required
        var containsBoost = IsUnidirectionalContainment(resume.Normalized, required.Normalized) ? 0.85 : 0;

        // Token overlap with penalty for asymmetric matches
        var tokenOverlap = CalculateTokenOverlapWithPenalty(resume.Tokens, required.Tokens);

        // Levenshtein similarity
        var similarity = SkillNormalizer.CalculateSimilarity(resume.Normalized, required.Normalized);

        return Math.Max(containsBoost, Math.Max(tokenOverlap, similarity));
    }

    private static double? CheckSkillHierarchy(string resumeCanonical, string requiredCanonical)
    {
        // Check if resume skill is a CHILD of required skill (candidate knows more specific)
        if (IsChildOf(resumeCanonical, requiredCanonical))
        {
            // Resume: Child, Required: Parent => MATCH
            return 1.0;
        }

        // Check if resume skill is a PARENT of required skill (candidate knows less specific)
        if (IsChildOf(requiredCanonical, resumeCanonical))
        {
            // Resume: Parent, Required: Child => NO MATCH
            return 0.0;
        }

        return null; // No hierarchy relationship
    }

    private static bool IsChildOf(string skill, string potentialParent)
    {
        if (string.Equals(skill, potentialParent, StringComparison.OrdinalIgnoreCase))
        {
            return false; // Same skill is not a child
        }

        var parentChildren = SkillHierarchy.GetChildren(potentialParent);
        return parentChildren.Contains(skill, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsUnidirectionalContainment(string resumeNormalized, string requiredNormalized)
    {
        // Only boost if RESUME contains REQUIRED (candidate knows more)
        // "SQL Server" contains "SQL" => true (candidate knows SQL Server, satisfies SQL requirement)
        // "JavaScript" contains "Java" => false (candidate knows JS, doesn't satisfy Java requirement)
        return resumeNormalized.Contains(requiredNormalized, StringComparison.OrdinalIgnoreCase);
    }

    private static double CalculateTokenOverlapWithPenalty(
        IReadOnlyCollection<string> resumeTokens,
        IReadOnlyCollection<string> requiredTokens)
    {
        if (resumeTokens.Count == 0 || requiredTokens.Count == 0)
        {
            return 0;
        }

        var intersectionCount = resumeTokens.Intersect(requiredTokens, StringComparer.OrdinalIgnoreCase).Count();
        if (intersectionCount == 0)
        {
            return 0;
        }

        // Base overlap calculation
        var baseOverlap = (double)intersectionCount / Math.Max(resumeTokens.Count, requiredTokens.Count);

        // Penalty for asymmetric token counts
        // Required: 1 token, Resume: N tokens => partial match at most
        if (requiredTokens.Count == 1 && resumeTokens.Count > 1)
        {
            // "SQL" required, "SQL Server" in resume => 0.5 max (too generic)
            return baseOverlap * 0.5;
        }

        // Resume: 1 token, Required: N tokens => poor match
        if (resumeTokens.Count == 1 && requiredTokens.Count > 1)
        {
            // "SQL" in resume, "SQL Server" required => 0.0 (doesn't know SQL Server)
            return 0;
        }

        return baseOverlap;
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
            MatchedSkillsWithScores = new Dictionary<string, double>(),
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
