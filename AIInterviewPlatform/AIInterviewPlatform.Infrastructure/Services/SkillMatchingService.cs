using AIInterviewPlatform.Application.Common;
using AIInterviewPlatform.Application.DTOs.SkillMatching;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class SkillMatchingService : ISkillMatchingService
{
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

        var normalizedResumeSkills = SkillNormalizer.NormalizeCollection(request.ResumeSkills);
        var normalizedRequiredSkills = SkillNormalizer.NormalizeCollection(request.RequiredSkills);

        var matchedSkills = new List<string>();
        var missingSkills = new List<string>();

        foreach (var required in normalizedRequiredSkills)
        {
            var isMatched = normalizedResumeSkills.Any(resume =>
                SkillNormalizer.AreEquivalent(resume, required));

            if (isMatched)
            {
                matchedSkills.Add(required);
            }
            else
            {
                missingSkills.Add(required);
            }
        }

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
            MatchPercentage = Math.Round(matchPercentage, 2),
            ReadinessScore = Math.Round(readinessScore, 2)
        };
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
            MatchPercentage = 0,
            ReadinessScore = 0
        };
    }
}
