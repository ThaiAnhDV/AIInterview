namespace AIInterviewPlatform.Application.DTOs.Interview.Enums;

public static class InterviewEnums
{
    public static class QuestionType
    {
        public const string Technical = "TECHNICAL";
        public const string Behavioral = "BEHAVIORAL";
        public const string Communication = "COMMUNICATION";
    }

    public static class QuestionDifficulty
    {
        public const string Beginner = "BEGINNER";
        public const string Intermediate = "INTERMEDIATE";
        public const string Advanced = "ADVANCED";
    }

    public static class GenerationStatus
    {
        public const string Pending = "PENDING";
        public const string InProgress = "IN_PROGRESS";
        public const string Completed = "COMPLETED";
        public const string Failed = "FAILED";
    }
}

public enum QuestionTypeEnum
{
    Technical,
    Behavioral,
    Communication
}

public enum QuestionDifficultyEnum
{
    Beginner,
    Intermediate,
    Advanced
}

public enum GenerationStatusEnum
{
    Pending,
    InProgress,
    Completed,
    Failed
}
