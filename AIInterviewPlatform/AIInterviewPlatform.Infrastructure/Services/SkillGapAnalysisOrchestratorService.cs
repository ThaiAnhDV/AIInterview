using AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;
using AIInterviewPlatform.Application.DTOs.SkillMatching;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class SkillGapAnalysisOrchestratorService : ISkillGapAnalysisOrchestratorService
{
    private readonly ApplicationDbContext _context;
    private readonly ISkillExtractionService _resumeSkillExtractionService;
    private readonly IJobDescriptionSkillExtractionService _jdSkillExtractionService;
    private readonly ISkillMatchingService _skillMatchingService;
    private readonly ILogger<SkillGapAnalysisOrchestratorService> _logger;

    public SkillGapAnalysisOrchestratorService(
        ApplicationDbContext context,
        ISkillExtractionService resumeSkillExtractionService,
        IJobDescriptionSkillExtractionService jdSkillExtractionService,
        ISkillMatchingService skillMatchingService,
        ILogger<SkillGapAnalysisOrchestratorService> logger)
    {
        _context = context;
        _resumeSkillExtractionService = resumeSkillExtractionService;
        _jdSkillExtractionService = jdSkillExtractionService;
        _skillMatchingService = skillMatchingService;
        _logger = logger;
    }

    public async Task<ComprehensiveSkillGapAnalysisResponse> AnalyzeSkillGapAsync(
        long userId,
        ComprehensiveSkillGapAnalysisRequest request)
    {
        _logger.LogInformation(
            "Starting comprehensive skill gap analysis for user {UserId}, Resume: {ResumeId}, JD: {JobDescriptionId}",
            userId, request.ResumeId, request.JobDescriptionId);

        _logger.LogInformation(
            "[LANG_AUDIT] Orchestrator={Service} Call=ExtractResumeSkills LanguageCode={LanguageCode}",
            nameof(SkillGapAnalysisOrchestratorService), request.LanguageCode);

        var resume = await _context.Resumes
            .FirstOrDefaultAsync(x => x.Id == request.ResumeId && x.UserId == userId);

        if (resume == null)
        {
            return new ComprehensiveSkillGapAnalysisResponse
            {
                Success = false,
                ErrorCode = "RESUME_NOT_FOUND",
                Message = "Resume not found.",
                ResumeId = request.ResumeId,
                JobDescriptionId = request.JobDescriptionId
            };
        }

        var jobDescription = await _context.JobDescriptions
            .FirstOrDefaultAsync(x => x.Id == request.JobDescriptionId);

        if (jobDescription == null)
        {
            return new ComprehensiveSkillGapAnalysisResponse
            {
                Success = false,
                ErrorCode = "JOB_DESCRIPTION_NOT_FOUND",
                Message = "Job description not found.",
                ResumeId = request.ResumeId,
                JobDescriptionId = request.JobDescriptionId
            };
        }

        _logger.LogInformation(
            "[LANG_AUDIT] Orchestrator={Service} Call=ExtractResumeSkills LanguageCode={LanguageCode}",
            nameof(SkillGapAnalysisOrchestratorService), request.LanguageCode);

        var resumeSkillsResult = await _resumeSkillExtractionService
            .ExtractSkillsFromResumeAsync(resume.ParsedContent ?? string.Empty, request.LanguageCode);

        if (!resumeSkillsResult.Success)
        {
            _logger.LogWarning("[SkillGap] ANALYSIS_ABORTED");
            return new ComprehensiveSkillGapAnalysisResponse
            {
                Success = false,
                ErrorCode = resumeSkillsResult.Error?.ErrorCode ?? "GEMINI_ERROR",
                Message = resumeSkillsResult.Error?.Message ?? "AI service is temporarily busy. Please retry in a few minutes.",
                ResumeId = request.ResumeId,
                JobDescriptionId = request.JobDescriptionId
            };
        }

        _logger.LogInformation(
            "[LANG_AUDIT] Orchestrator={Service} Call=ExtractJDSkills LanguageCode={LanguageCode}",
            nameof(SkillGapAnalysisOrchestratorService), request.LanguageCode);

        var jdSkillsResult = await _jdSkillExtractionService
            .ExtractRequiredSkillsAsync(jobDescription.Content, request.LanguageCode);

        if (!jdSkillsResult.Success)
        {
            _logger.LogWarning("[SkillGap] ANALYSIS_ABORTED");
            return new ComprehensiveSkillGapAnalysisResponse
            {
                Success = false,
                ErrorCode = jdSkillsResult.Error?.ErrorCode ?? "GEMINI_ERROR",
                Message = jdSkillsResult.Error?.Message ?? "AI service is temporarily busy. Please retry in a few minutes.",
                ResumeId = request.ResumeId,
                JobDescriptionId = request.JobDescriptionId
            };
        }

        var resumeSkills = resumeSkillsResult.Data?.Skills ?? [];
        var requiredSkills = jdSkillsResult.Data?.RequiredSkills ?? [];

        var matchRequest = new SkillMatchRequest
        {
            ResumeSkills = resumeSkills,
            RequiredSkills = requiredSkills
        };

        var matchResult = _skillMatchingService.MatchSkills(matchRequest);

        var analysis = await SaveSkillGapAnalysisAsync(
            userId, request.ResumeId, request.JobDescriptionId, matchResult);

        await SaveSkillGapsAsync(analysis.Id, matchResult.MissingSkills);
        await SaveReadinessScoreAsync(userId, analysis.Id, matchResult);

        _logger.LogInformation(
            "Skill gap analysis completed. AnalysisId: {AnalysisId}, Matched: {MatchedCount}, Missing: {MissingCount}",
            analysis.Id, matchResult.MatchedSkills.Count, matchResult.MissingSkills.Count);

        return new ComprehensiveSkillGapAnalysisResponse
        {
            Success = true,
            AnalysisId = analysis.Id,
            ResumeId = request.ResumeId,
            JobDescriptionId = request.JobDescriptionId,
            ReadinessScore = (decimal)matchResult.ReadinessScore,
            ResumeSkills = resumeSkills,
            RequiredSkills = requiredSkills,
            MatchedSkills = matchResult.MatchedSkills,
            MissingSkills = matchResult.MissingSkills
                .Select(s => new SkillGapItemResponse
                {
                    SkillName = s,
                    GapLevel = GapLevel.HIGH.ToString(),
                    GapDescription = $"Missing skill: {s}"
                })
                .ToList(),
            CreatedAt = analysis.CreatedAt
        };
    }

    private async Task<SkillGapAnalysis> SaveSkillGapAnalysisAsync(
        long userId,
        long resumeId,
        long jobDescriptionId,
        SkillMatchResult matchResult)
    {
        var analysis = new SkillGapAnalysis
        {
            UserId = userId,
            ResumeId = resumeId,
            JobDescriptionId = jobDescriptionId,
            AnalysisStatus = AnalysisStatus.COMPLETED,
            CreatedAt = DateTime.UtcNow
        };

        _context.SkillGapAnalyses.Add(analysis);
        await _context.SaveChangesAsync();

        return analysis;
    }

    private async Task SaveSkillGapsAsync(long analysisId, List<string> missingSkills)
    {
        foreach (var skillName in missingSkills)
        {
            var normalizedSkillName = NormalizeSkillName(skillName);

            var skill = await _context.Skills
                .FirstOrDefaultAsync(x => x.SkillName.ToLower() == normalizedSkillName.ToLower());

            if (skill == null)
            {
                skill = new Skill
                {
                    SkillName = normalizedSkillName,
                    SkillType = DetermineSkillType(normalizedSkillName)
                };

                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();
            }

            var skillGap = new SkillGap
            {
                SkillGapAnalysisId = analysisId,
                SkillId = skill.Id,
                GapLevel = GapLevel.HIGH,
                GapDescription = $"Missing skill: {skill.SkillName}"
            };

            _context.SkillGaps.Add(skillGap);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SaveReadinessScoreAsync(
        long userId,
        long analysisId,
        SkillMatchResult matchResult)
    {
        var readinessScore = new ReadinessScore
        {
            UserId = userId,
            SkillGapAnalysisId = analysisId,
            Score = (decimal)matchResult.ReadinessScore,
            ScoreType = ScoreType.OVERALL,
            CalculatedAt = DateTime.UtcNow
        };

        _context.ReadinessScores.Add(readinessScore);
        await _context.SaveChangesAsync();
    }

    private static string NormalizeSkillName(string skillName)
    {
        var normalized = skillName.Trim();

        var knownMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "c#", "C#" },
            { "c++", "C++" },
            { ".net", ".NET" },
            { "asp.net", "ASP.NET Core" },
            { "nodejs", "Node.js" },
            { "typescript", "TypeScript" },
            { "javascript", "JavaScript" },
            { "postgresql", "PostgreSQL" },
            { "mongodb", "MongoDB" },
            { "kubernetes", "Kubernetes" },
            { "microservices", "Microservices" },
            { "ci/cd", "CI/CD" },
            { "rest api", "REST API" },
            { "graphql", "GraphQL" },
            { "docker", "Docker" },
            { "aws", "AWS" },
            { "gcp", "GCP" },
            { "react.js", "React" },
            { "reactjs", "React" },
            { "vue.js", "Vue.js" },
            { "vuejs", "Vue.js" },
            { "angular.js", "Angular" },
            { "angularjs", "Angular" }
        };

        return knownMappings.TryGetValue(normalized, out var canonical)
            ? canonical
            : char.ToUpper(normalized[0]) + normalized[1..].ToLower();
    }

    private static string DetermineSkillType(string skillName)
    {
        var languages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "C#", "Java", "Python", "JavaScript", "TypeScript", "Go", "Rust", "Ruby", "PHP", "Swift", "Kotlin",
            "C++", "SQL", "HTML", "CSS", "Scala", "F#", "R"
        };

        var frameworks = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".NET", "ASP.NET Core", "Spring", "Django", "Flask", "React", "Angular", "Vue.js", "Next.js",
            "Node.js", "Express", "FastAPI", "Laravel", "Rails"
        };

        var databases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SQL Server", "PostgreSQL", "MySQL", "MongoDB", "Redis", "Elasticsearch", "Cassandra", "DynamoDB",
            "Oracle", "SQLite", "MariaDB"
        };

        var cloudPlatforms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AWS", "Azure", "GCP", "Heroku", "DigitalOcean"
        };

        var devOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Docker", "Kubernetes", "Jenkins", "CI/CD", "DevOps", "Terraform", "Ansible", "GitLab CI", "GitHub Actions"
        };

        var tools = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Git", "GitHub", "GitLab", "Jira", "Confluence", "Slack", "Postman", "Swagger"
        };

        if (languages.Contains(skillName)) return "Language";
        if (frameworks.Contains(skillName)) return "Framework";
        if (databases.Contains(skillName)) return "Database";
        if (cloudPlatforms.Contains(skillName)) return "Cloud";
        if (devOps.Contains(skillName)) return "DevOps";
        if (tools.Contains(skillName)) return "Tool";

        return "Technology";
    }
}
