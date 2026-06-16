using AIInterviewPlatform.Application.DTOs.Recommendation;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RecommendationService> _logger;

    private static readonly Dictionary<string, (string Title, string Content, PriorityLevel Priority)> RuleBasedRecommendations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["docker"] = (
            "Learn Docker Fundamentals",
            "Docker is essential for modern deployment. Start with container basics, then move to multi-stage builds and docker-compose.",
            PriorityLevel.HIGH
        ),
        ["azure"] = (
            "Study Azure Fundamentals",
            "Azure is a leading cloud platform. Focus on App Services, Azure SQL, and Azure DevOps for .NET applications.",
            PriorityLevel.HIGH
        ),
        ["kubernetes"] = (
            "Learn Kubernetes Basics",
            "Kubernetes enables container orchestration at scale. Start with pods, services, and deployments.",
            PriorityLevel.MEDIUM
        ),
        ["sql"] = (
            "Master SQL Query Optimization",
            "SQL is critical for backend development. Focus on indexing, joins, stored procedures, and query performance.",
            PriorityLevel.HIGH
        ),
        ["csharp"] = (
            "Deep Dive into C# Advanced Features",
            "Strengthen your C# skills with async/await, LINQ, generics, and design patterns.",
            PriorityLevel.MEDIUM
        ),
        [".net"] = (
            "Build REST APIs with ASP.NET Core",
            "ASP.NET Core is the modern .NET framework. Focus on Web API best practices, middleware, and dependency injection.",
            PriorityLevel.HIGH
        ),
        ["asp.net"] = (
            "Master ASP.NET Core Development",
            "ASP.NET Core enables cross-platform web development. Learn MVC pattern, Web API, and Entity Framework Core.",
            PriorityLevel.HIGH
        ),
        ["python"] = (
            "Learn Python Programming",
            "Python is versatile and widely used. Focus on syntax, data structures, and common libraries.",
            PriorityLevel.MEDIUM
        ),
        ["java"] = (
            "Master Java Development",
            "Java is enterprise-grade. Focus on OOP, Spring Framework, and JVM internals.",
            PriorityLevel.MEDIUM
        ),
        ["javascript"] = (
            "Strengthen JavaScript Skills",
            "JavaScript is essential for full-stack development. Focus on ES6+, async patterns, and frameworks.",
            PriorityLevel.MEDIUM
        ),
        ["react"] = (
            "Learn React for Frontend Development",
            "React is popular for building user interfaces. Start with components, state, and hooks.",
            PriorityLevel.MEDIUM
        ),
        ["aws"] = (
            "Study AWS Cloud Services",
            "AWS is the leading cloud provider. Focus on EC2, S3, Lambda, and RDS for backend applications.",
            PriorityLevel.HIGH
        ),
        ["git"] = (
            "Master Git Version Control",
            "Git is essential for collaborative development. Learn branching strategies, merge, and pull requests.",
            PriorityLevel.LOW
        ),
        ["devops"] = (
            "Learn DevOps Practices",
            "DevOps bridges development and operations. Focus on CI/CD pipelines, Docker, and monitoring.",
            PriorityLevel.MEDIUM
        ),
        ["microservices"] = (
            "Understand Microservices Architecture",
            "Microservices enable scalable systems. Learn service decomposition, API gateways, and inter-service communication.",
            PriorityLevel.MEDIUM
        ),
        ["mongodb"] = (
            "Learn MongoDB NoSQL Database",
            "MongoDB provides flexible document storage. Focus on CRUD operations, aggregation pipeline, and indexing.",
            PriorityLevel.MEDIUM
        ),
        ["postgresql"] = (
            "Master PostgreSQL Database",
            "PostgreSQL is a powerful relational database. Focus on advanced queries, indexing strategies, and performance tuning.",
            PriorityLevel.HIGH
        ),
        ["redis"] = (
            "Learn Redis for Caching",
            "Redis accelerates applications with in-memory caching. Focus on data structures and cache invalidation strategies.",
            PriorityLevel.MEDIUM
        ),
        ["rest api"] = (
            "Build RESTful APIs Best Practices",
            "REST APIs are the backbone of web services. Learn HTTP methods, status codes, versioning, and authentication.",
            PriorityLevel.HIGH
        ),
        ["graphql"] = (
            "Explore GraphQL API Development",
            "GraphQL offers flexible data querying. Learn schema definition, resolvers, and mutations.",
            PriorityLevel.MEDIUM
        ),
        ["ci/cd"] = (
            "Implement CI/CD Pipelines",
            "Continuous integration and deployment automate software delivery. Focus on automated testing and deployment workflows.",
            PriorityLevel.MEDIUM
        ),
        ["agile"] = (
            "Understand Agile Methodologies",
            "Agile drives iterative development. Learn Scrum, Kanban, sprints, and user stories.",
            PriorityLevel.LOW
        ),
        ["tdd"] = (
            "Practice Test-Driven Development",
            "TDD improves code quality. Write tests before code, refactor, and maintain test coverage.",
            PriorityLevel.MEDIUM
        ),
        ["security"] = (
            "Learn Application Security",
            "Security is critical. Focus on OWASP Top 10, authentication, authorization, and secure coding practices.",
            PriorityLevel.HIGH
        ),
        ["communication"] = (
            "Improve Technical Communication Skills",
            "Communication is key for interviews. Practice explaining technical concepts clearly and concisely.",
            PriorityLevel.MEDIUM
        ),
        ["system design"] = (
            "Study System Design Principles",
            "System design is crucial for senior roles. Learn scalability, load balancing, caching, and distributed systems.",
            PriorityLevel.MEDIUM
        ),
        ["data analysis"] = (
            "Develop Data Analysis Skills",
            "Data analysis drives decisions. Learn SQL analytics, data visualization, and statistical basics.",
            PriorityLevel.MEDIUM
        ),
        ["machine learning"] = (
            "Introduction to Machine Learning",
            "ML is increasingly important. Start with fundamentals, algorithms, and practical applications.",
            PriorityLevel.LOW
        )
    };

    public RecommendationService(
        ApplicationDbContext context,
        ILogger<RecommendationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<RecommendationResponse>> GenerateAndSaveRecommendationsAsync(
        long userId,
        long skillGapAnalysisId,
        List<MissingSkillInput> missingSkills)
    {
        _logger.LogInformation(
            "[Recommendation] Generating recommendations for User={UserId}, Analysis={AnalysisId}, MissingSkills={Count}",
            userId, skillGapAnalysisId, missingSkills.Count);

        var recommendations = new List<Recommendation>();

        foreach (var skill in missingSkills)
        {
            var recommendation = GenerateRuleBasedRecommendation(userId, skillGapAnalysisId, skill);
            recommendations.Add(recommendation);
        }

        _context.Recommendations.AddRange(recommendations);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "[Recommendation] Saved {Count} recommendations for User={UserId}, Analysis={AnalysisId}",
            recommendations.Count, userId, skillGapAnalysisId);

        return recommendations.Select(MapToResponse).ToList();
    }

    public async Task<List<RecommendationResponse>> GetMyRecommendationsAsync(long userId)
    {
        var recommendations = await _context.Recommendations
            .AsNoTracking()
            .Include(r => r.Skill)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return recommendations.Select(MapToResponse).ToList();
    }

    public async Task<List<RecommendationResponse>> GetRecommendationsByAnalysisIdAsync(
        long userId,
        long skillGapAnalysisId)
    {
        var recommendations = await _context.Recommendations
            .AsNoTracking()
            .Include(r => r.Skill)
            .Where(r => r.UserId == userId && r.SkillGapAnalysisId == skillGapAnalysisId)
            .OrderBy(r => r.PriorityLevel)
            .ThenBy(r => r.Skill != null ? r.Skill.SkillName : string.Empty)
            .ToListAsync();

        return recommendations.Select(MapToResponse).ToList();
    }

    public async Task<RecommendationResponse?> GetRecommendationByIdAsync(
        long userId,
        long recommendationId)
    {
        var recommendation = await _context.Recommendations
            .AsNoTracking()
            .Include(r => r.Skill)
            .FirstOrDefaultAsync(r => r.Id == recommendationId && r.UserId == userId);

        return recommendation == null ? null : MapToResponse(recommendation);
    }

    private Recommendation GenerateRuleBasedRecommendation(
        long userId,
        long skillGapAnalysisId,
        MissingSkillInput skill)
    {
        var skillName = skill.SkillName ?? string.Empty;
        var rule = FindMatchingRule(skillName);

        return new Recommendation
        {
            UserId = userId,
            SkillGapAnalysisId = skillGapAnalysisId,
            SkillId = skill.SkillId,
            RecommendationTitle = rule.Title,
            RecommendationContent = rule.Content,
            RecommendationType = RecommendationType.SKILL,
            PriorityLevel = rule.Priority,
            CreatedAt = DateTime.Now
        };
    }

    private static (string Title, string Content, PriorityLevel Priority) FindMatchingRule(string skillName)
    {
        foreach (var kvp in RuleBasedRecommendations)
        {
            if (skillName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains(skillName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return (
            $"Learn {skillName}",
            $"Focus on learning {skillName} fundamentals and practical applications. " +
            "Practice with real-world examples and build projects to reinforce understanding.",
            PriorityLevel.MEDIUM
        );
    }

    private static RecommendationResponse MapToResponse(Recommendation recommendation)
    {
        return new RecommendationResponse
        {
            Id = recommendation.Id,
            SkillGapAnalysisId = recommendation.SkillGapAnalysisId,
            SkillId = recommendation.SkillId,
            SkillName = recommendation.Skill?.SkillName ?? string.Empty,
            RecommendationTitle = recommendation.RecommendationTitle,
            RecommendationContent = recommendation.RecommendationContent,
            RecommendationType = recommendation.RecommendationType?.ToString() ?? string.Empty,
            PriorityLevel = recommendation.PriorityLevel.ToString(),
            CreatedAt = recommendation.CreatedAt
        };
    }
}
