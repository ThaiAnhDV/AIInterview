using AIInterviewPlatform.Application.DTOs.Roadmap.Responses;
using AIInterviewPlatform.Application.Interfaces.Services;

namespace AIInterviewPlatform.Infrastructure.Services;

public class MilestoneGeneratorService : IMilestoneGeneratorService, IMilestoneGenerator
{
    private readonly IMilestoneGenerator _milestoneGenerator;

    public MilestoneGeneratorService() : this(new DefaultMilestoneGenerator())
    {
    }

    public MilestoneGeneratorService(IMilestoneGenerator milestoneGenerator)
    {
        _milestoneGenerator = milestoneGenerator;
    }

    public List<MilestoneDto> GenerateMilestones(List<string> missingSkills)
    {
        if (missingSkills == null || missingSkills.Count == 0)
        {
            return [];
        }

        var normalizedSkills = missingSkills
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(NormalizeSkillName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .ToList();

        var milestones = new List<MilestoneDto>();
        var order = 1;

        foreach (var skill in normalizedSkills)
        {
            var milestone = _milestoneGenerator.GenerateSkillMilestone(skill, order++);
            milestone.Activities.ForEach(activity => activity.SourceMilestoneOrder = milestone.MilestoneOrder);
            milestones.Add(milestone);
        }

        var mockInterviewMilestone = _milestoneGenerator.GenerateMockInterviewMilestone(order);
        mockInterviewMilestone.Activities.ForEach(activity => activity.SourceMilestoneOrder = mockInterviewMilestone.MilestoneOrder);
        milestones.Add(mockInterviewMilestone);

        return milestones;
    }

    public List<MilestoneDto> GenerateMilestones(List<SkillGapForRoadmapDto> skillGaps)
    {
        if (skillGaps == null || skillGaps.Count == 0)
        {
            return [];
        }

        var sortedGaps = skillGaps
            .Where(g => !string.IsNullOrWhiteSpace(g.SkillName))
            .OrderBy(g => GetGapPriority(g.GapLevel))
            .ThenBy(g => g.SkillName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var milestones = new List<MilestoneDto>();
        var order = 1;

        foreach (var gap in sortedGaps)
        {
            var milestone = _milestoneGenerator.GenerateSkillMilestone(gap.SkillName, order++);
            milestone.Metadata = new MilestoneMetadataDto
            {
                TargetSkill = gap.SkillName,
                SkillType = gap.SkillType,
                GapLevel = gap.GapLevel,
                DifficultyLevel = DetermineDifficulty(gap.GapLevel)
            };
            milestone.Activities.ForEach(activity => activity.SourceMilestoneOrder = milestone.MilestoneOrder);
            milestones.Add(milestone);
        }

        var mockInterviewMilestone = _milestoneGenerator.GenerateMockInterviewMilestone(order);
        mockInterviewMilestone.Activities.ForEach(activity => activity.SourceMilestoneOrder = mockInterviewMilestone.MilestoneOrder);
        milestones.Add(mockInterviewMilestone);

        return milestones;
    }

    public MilestoneDto GenerateSkillMilestone(string skillName, int order)
    {
        return _milestoneGenerator.GenerateSkillMilestone(skillName, order);
    }

    public MilestoneDto GenerateMockInterviewMilestone(int order)
    {
        return _milestoneGenerator.GenerateMockInterviewMilestone(order);
    }

    private static string NormalizeSkillName(string skill)
    {
        var trimmed = skill.Trim();
        
        var knownMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "c#", "C#" },
            { "c++", "C++" },
            { ".net", ".NET" },
            { "asp.net core", "ASP.NET Core" },
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
            { "vue.js", "Vue.js" },
            { "angular.js", "Angular" }
        };

        return knownMappings.TryGetValue(trimmed, out var normalized) 
            ? normalized 
            : char.ToUpper(trimmed[0]) + trimmed[1..].ToLower();
    }

    private static int GetGapPriority(string? gapLevel)
    {
        return gapLevel?.ToUpperInvariant() switch
        {
            "HIGH" => 1,
            "MEDIUM" => 2,
            "LOW" => 3,
            _ => 4
        };
    }

    private static string DetermineDifficulty(string? gapLevel)
    {
        return gapLevel?.ToUpperInvariant() switch
        {
            "HIGH" => "ADVANCED",
            "MEDIUM" => "INTERMEDIATE",
            "LOW" => "BEGINNER",
            _ => "INTERMEDIATE"
        };
    }
}

public class DefaultMilestoneGenerator : IMilestoneGenerator
{
    private const string LearnMilestonePrefix = "Learn";
    private const string MockInterviewTitle = "Mock Interview";
    private const string MockInterviewDescription = "Practice your interview skills with mock interview sessions";

    public MilestoneDto GenerateSkillMilestone(string skillName, int order)
    {
        var activities = GenerateDefaultActivities(skillName);

        return new MilestoneDto
        {
            Id = 0,
            LearningRoadmapId = 0,
            MilestoneTitle = $"{LearnMilestonePrefix} {skillName}",
            MilestoneOrder = order,
            IsCompleted = false,
            CompletionPercentage = 0,
            Activities = activities,
            CompletedAt = null,
            Metadata = new MilestoneMetadataDto
            {
                TargetSkill = skillName,
                EstimatedDuration = EstimateDuration(skillName),
                DifficultyLevel = "INTERMEDIATE",
                LearningObjectives = GenerateLearningObjectives(skillName)
            }
        };
    }

    public MilestoneDto GenerateMockInterviewMilestone(int order)
    {
        var activities = GenerateMockInterviewActivities();

        return new MilestoneDto
        {
            Id = 0,
            LearningRoadmapId = 0,
            MilestoneTitle = MockInterviewTitle,
            MilestoneOrder = order,
            IsCompleted = false,
            CompletionPercentage = 0,
            Activities = activities,
            CompletedAt = null,
            Metadata = new MilestoneMetadataDto
            {
                TargetSkill = "Interview Preparation",
                EstimatedDuration = "3 days",
                DifficultyLevel = "ADVANCED",
                LearningObjectives = new List<string>
                {
                    "Prepare for technical questions",
                    "Practice behavioral questions",
                    "Simulate real interview environment"
                }
            }
        };
    }

    private static List<ActivityDto> GenerateDefaultActivities(string skillName)
    {
        return
        [
            new ActivityDto
            {
                Id = 0,
                RoadmapMilestoneId = 0,
                ActivityTitle = $"Study {skillName} fundamentals",
                ActivityDescription = $"Learn the core concepts and basics of {skillName}.",
                ActivityType = "READING",
                IsCompleted = false,
                Metadata = new ActivityMetadataDto
                {
                    EstimatedDuration = GetReadingDuration(skillName),
                    DifficultyLevel = "BEGINNER",
                    Tags = ["fundamentals", "basics"]
                }
            },
            new ActivityDto
            {
                Id = 0,
                RoadmapMilestoneId = 0,
                ActivityTitle = $"Practice {skillName}",
                ActivityDescription = $"Complete hands-on exercises and practical tasks with {skillName}.",
                ActivityType = "PRACTICE",
                IsCompleted = false,
                Metadata = new ActivityMetadataDto
                {
                    EstimatedDuration = GetPracticeDuration(skillName),
                    DifficultyLevel = "INTERMEDIATE",
                    Tags = ["practice", "hands-on"]
                }
            },
            new ActivityDto
            {
                Id = 0,
                RoadmapMilestoneId = 0,
                ActivityTitle = $"Build a {skillName} project",
                ActivityDescription = $"Apply your knowledge by building a real-world project using {skillName}.",
                ActivityType = "PRACTICE",
                IsCompleted = false,
                Metadata = new ActivityMetadataDto
                {
                    EstimatedDuration = GetProjectDuration(skillName),
                    DifficultyLevel = "ADVANCED",
                    Tags = ["project", "real-world"]
                }
            }
        ];
    }

    private static List<ActivityDto> GenerateMockInterviewActivities()
    {
        return
        [
            new ActivityDto
            {
                Id = 0,
                RoadmapMilestoneId = 0,
                ActivityTitle = "Technical interview preparation",
                ActivityDescription = "Review common technical questions and prepare answers.",
                ActivityType = "READING",
                IsCompleted = false,
                Metadata = new ActivityMetadataDto
                {
                    EstimatedDuration = "2 days",
                    DifficultyLevel = "INTERMEDIATE",
                    Tags = ["technical", "preparation"]
                }
            },
            new ActivityDto
            {
                Id = 0,
                RoadmapMilestoneId = 0,
                ActivityTitle = "Behavioral questions practice",
                ActivityDescription = "Practice STAR method for behavioral questions.",
                ActivityType = "PRACTICE",
                IsCompleted = false,
                Metadata = new ActivityMetadataDto
                {
                    EstimatedDuration = "1 day",
                    DifficultyLevel = "BEGINNER",
                    Tags = ["behavioral", "STAR"]
                }
            },
            new ActivityDto
            {
                Id = 0,
                RoadmapMilestoneId = 0,
                ActivityTitle = "Mock interview session",
                ActivityDescription = "Conduct a full mock interview session.",
                ActivityType = "MOCK_INTERVIEW",
                IsCompleted = false,
                Metadata = new ActivityMetadataDto
                {
                    EstimatedDuration = "1 day",
                    DifficultyLevel = "ADVANCED",
                    Tags = ["mock", "interview", "simulation"]
                }
            }
        ];
    }

    private static List<string> GenerateLearningObjectives(string skillName)
    {
        return
        [
            $"Understand core concepts of {skillName}",
            $"Complete practical exercises",
            $"Build a project using {skillName}",
            $"Evaluate your understanding through quiz"
        ];
    }

    private static string EstimateDuration(string skillName)
    {
        var lower = skillName.ToLowerInvariant();
        
        var complexSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Microservices", "Kubernetes", "Docker", "AWS", "Azure", "GCP"
        };

        var mediumSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "C#", "Java", "Python", "Node.js", "React", "Angular"
        };

        if (complexSkills.Contains(skillName)) return "14 days";
        if (mediumSkills.Contains(skillName)) return "7 days";
        return "5 days";
    }

    private static string GetReadingDuration(string skillName) => "2-3 days";
    private static string GetPracticeDuration(string skillName) => "2-3 days";
    private static string GetProjectDuration(string skillName) => "3-5 days";
}
