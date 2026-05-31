using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Infrastructure.Data;

namespace AIInterviewPlatform.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Users = new GenericRepository<User>(_context);
            AuthenticationAccounts = new GenericRepository<AuthenticationAccount>(_context);
            UserProfiles = new GenericRepository<UserProfile>(_context);
            Resumes = new GenericRepository<Resume>(_context);

            TargetJobs = new GenericRepository<TargetJob>(_context);
            JobDescriptions = new GenericRepository<JobDescription>(_context);
            Skills = new GenericRepository<Skill>(_context);
            RequiredSkills = new GenericRepository<RequiredSkill>(_context);

            SkillGapAnalyses = new GenericRepository<SkillGapAnalysis>(_context);
            SkillGaps = new GenericRepository<SkillGap>(_context);
            ReadinessScores = new GenericRepository<ReadinessScore>(_context);
            StrengthWeaknessReports = new GenericRepository<StrengthWeaknessReport>(_context);

            QuestionCategories = new GenericRepository<QuestionCategory>(_context);
            QuestionTemplates = new GenericRepository<QuestionTemplate>(_context);
            InterviewSessions = new GenericRepository<InterviewSession>(_context);
            InterviewQuestions = new GenericRepository<InterviewQuestion>(_context);
            InterviewAnswers = new GenericRepository<InterviewAnswer>(_context);
            AnswerEvaluations = new GenericRepository<AnswerEvaluation>(_context);

            Feedbacks = new GenericRepository<Feedback>(_context);
            ImprovementSuggestions = new GenericRepository<ImprovementSuggestion>(_context);
            WeakCommunicationPatterns = new GenericRepository<WeakCommunicationPattern>(_context);
            Recommendations = new GenericRepository<Recommendation>(_context);

            LearningRoadmaps = new RoadmapRepository(_context);
            RoadmapMilestones = new MilestoneRepository(_context);
            LearningActivities = new ActivityRepository(_context);
            RoadmapProgresses = new ProgressRepository(_context);
            RoadmapRecommendations = new GenericRepository<RoadmapRecommendation>(_context);

            PracticeHistories = new GenericRepository<PracticeHistory>(_context);
            ProgressRecords = new GenericRepository<ProgressRecord>(_context);
            SkillImprovementTrends = new GenericRepository<SkillImprovementTrend>(_context);
            Notifications = new GenericRepository<Notification>(_context);
            UsageStatistics = new GenericRepository<UsageStatistic>(_context);
            SystemLogs = new GenericRepository<SystemLog>(_context);
            DashboardRepository = new DashboardRepository(_context);
        }

        public IGenericRepository<User> Users { get; }
        public IGenericRepository<AuthenticationAccount> AuthenticationAccounts { get; }
        public IGenericRepository<UserProfile> UserProfiles { get; }
        public IGenericRepository<Resume> Resumes { get; }

        public IGenericRepository<TargetJob> TargetJobs { get; }
        public IGenericRepository<JobDescription> JobDescriptions { get; }
        public IGenericRepository<Skill> Skills { get; }
        public IGenericRepository<RequiredSkill> RequiredSkills { get; }

        public IGenericRepository<SkillGapAnalysis> SkillGapAnalyses { get; }
        public IGenericRepository<SkillGap> SkillGaps { get; }
        public IGenericRepository<ReadinessScore> ReadinessScores { get; }
        public IGenericRepository<StrengthWeaknessReport> StrengthWeaknessReports { get; }

        public IGenericRepository<QuestionCategory> QuestionCategories { get; }
        public IGenericRepository<QuestionTemplate> QuestionTemplates { get; }
        public IGenericRepository<InterviewSession> InterviewSessions { get; }
        public IGenericRepository<InterviewQuestion> InterviewQuestions { get; }
        public IGenericRepository<InterviewAnswer> InterviewAnswers { get; }
        public IGenericRepository<AnswerEvaluation> AnswerEvaluations { get; }

        public IGenericRepository<Feedback> Feedbacks { get; }
        public IGenericRepository<ImprovementSuggestion> ImprovementSuggestions { get; }
        public IGenericRepository<WeakCommunicationPattern> WeakCommunicationPatterns { get; }
        public IGenericRepository<Recommendation> Recommendations { get; }

        public IGenericRepository<LearningRoadmap> LearningRoadmaps { get; }
        public IGenericRepository<RoadmapMilestone> RoadmapMilestones { get; }
        public IGenericRepository<LearningActivity> LearningActivities { get; }
        public IGenericRepository<RoadmapProgress> RoadmapProgresses { get; }
        public IGenericRepository<RoadmapRecommendation> RoadmapRecommendations { get; }

        public IGenericRepository<PracticeHistory> PracticeHistories { get; }
        public IGenericRepository<ProgressRecord> ProgressRecords { get; }
        public IGenericRepository<SkillImprovementTrend> SkillImprovementTrends { get; }
        public IGenericRepository<Notification> Notifications { get; }
        public IGenericRepository<UsageStatistic> UsageStatistics { get; }
        public IGenericRepository<SystemLog> SystemLogs { get; }

        public IDashboardRepository DashboardRepository { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
