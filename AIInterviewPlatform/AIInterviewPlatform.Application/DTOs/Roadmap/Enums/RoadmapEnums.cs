namespace AIInterviewPlatform.Application.DTOs.Roadmap.Enums;

public static class RoadmapEnums
{
    public static class RoadmapStatus
    {
        public const string Active = "ACTIVE";
        public const string Completed = "COMPLETED";
        public const string Archived = "ARCHIVED";
    }

    public static class ActivityType
    {
        public const string Reading = "READING";
        public const string Practice = "PRACTICE";
        public const string MockInterview = "MOCK_INTERVIEW";
        public const string Quiz = "QUIZ";
        public const string Other = "OTHER";
    }

    public static class DifficultyLevel
    {
        public const string Beginner = "BEGINNER";
        public const string Intermediate = "INTERMEDIATE";
        public const string Advanced = "ADVANCED";
    }

    public static class GapLevel
    {
        public const string Low = "LOW";
        public const string Medium = "MEDIUM";
        public const string High = "HIGH";
    }
}

public enum RoadmapStatusEnum
{
    Active,
    Completed,
    Archived
}

public enum ActivityTypeEnum
{
    Reading,
    Practice,
    MockInterview,
    Quiz,
    Other
}

public enum DifficultyLevelEnum
{
    Beginner,
    Intermediate,
    Advanced
}
