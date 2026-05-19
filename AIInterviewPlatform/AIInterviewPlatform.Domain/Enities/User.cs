using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enum;

namespace AIInterviewPlatform.Domain.Enities
{
    public class User
    {
        public long Id { get; set; }

        public UserType UserType { get; set; } = UserType.USER;

        public UserStatus Status { get; set; } = UserStatus.ACTIVE;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public AuthenticationAccount? AuthenticationAccount { get; set; }

        public UserProfile? UserProfile { get; set; }

        public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
        public ICollection<TargetJob> TargetJobs { get; set; } = new List<TargetJob>();
        public ICollection<SkillGapAnalysis> SkillGapAnalyses { get; set; } = new List<SkillGapAnalysis>();

        public ICollection<ReadinessScore> ReadinessScores { get; set; } = new List<ReadinessScore>();
        public ICollection<InterviewSession> InterviewSessions { get; set; } = new List<InterviewSession>();

        public ICollection<QuestionTemplate> CreatedQuestionTemplates { get; set; } = new List<QuestionTemplate>();
        public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
        public ICollection<LearningRoadmap> LearningRoadmaps { get; set; } = new List<LearningRoadmap>();
        public ICollection<PracticeHistory> PracticeHistories { get; set; } = new List<PracticeHistory>();

public ICollection<ProgressRecord> ProgressRecords { get; set; } = new List<ProgressRecord>();

public ICollection<SkillImprovementTrend> SkillImprovementTrends { get; set; } = new List<SkillImprovementTrend>();

public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

public ICollection<UsageStatistic> UsageStatistics { get; set; } = new List<UsageStatistic>();

public ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
    }
}
