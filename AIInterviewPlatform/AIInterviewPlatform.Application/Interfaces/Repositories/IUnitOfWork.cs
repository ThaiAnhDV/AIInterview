using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Domain.Enities;

namespace AIInterviewPlatform.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<User> Users { get; }

        IGenericRepository<AuthenticationAccount> AuthenticationAccounts { get; }

        IGenericRepository<UserProfile> UserProfiles { get; }

        IGenericRepository<Resume> Resumes { get; }

        IGenericRepository<TargetJob> TargetJobs { get; }

        IGenericRepository<JobDescription> JobDescriptions { get; }

        IGenericRepository<Skill> Skills { get; }

        IGenericRepository<RequiredSkill> RequiredSkills { get; }

        IGenericRepository<SkillGapAnalysis> SkillGapAnalyses { get; }

        IGenericRepository<SkillGap> SkillGaps { get; }

        IGenericRepository<ReadinessScore> ReadinessScores { get; }

        IGenericRepository<StrengthWeaknessReport> StrengthWeaknessReports { get; }

        IGenericRepository<QuestionCategory> QuestionCategories { get; }

        IGenericRepository<QuestionTemplate> QuestionTemplates { get; }

        IGenericRepository<InterviewSession> InterviewSessions { get; }

        IGenericRepository<InterviewQuestion> InterviewQuestions { get; }

        IGenericRepository<InterviewAnswer> InterviewAnswers { get; }

        IGenericRepository<AnswerEvaluation> AnswerEvaluations { get; }

        IGenericRepository<Feedback> Feedbacks { get; }

        IGenericRepository<ImprovementSuggestion> ImprovementSuggestions { get; }

        IGenericRepository<WeakCommunicationPattern> WeakCommunicationPatterns { get; }

        IGenericRepository<Recommendation> Recommendations { get; }

        IGenericRepository<LearningRoadmap> LearningRoadmaps { get; }

        IGenericRepository<RoadmapMilestone> RoadmapMilestones { get; }

        IGenericRepository<LearningActivity> LearningActivities { get; }

        IGenericRepository<RoadmapProgress> RoadmapProgresses { get; }

        IGenericRepository<RoadmapRecommendation> RoadmapRecommendations { get; }

        IGenericRepository<PracticeHistory> PracticeHistories { get; }

        IGenericRepository<ProgressRecord> ProgressRecords { get; }

        IGenericRepository<SkillImprovementTrend> SkillImprovementTrends { get; }

        IGenericRepository<Notification> Notifications { get; }

        IGenericRepository<UsageStatistic> UsageStatistics { get; }

        IGenericRepository<SystemLog> SystemLogs { get; }

        Task<int> SaveChangesAsync();
    }
}
