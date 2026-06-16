using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<AuthenticationAccount> AuthenticationAccounts => Set<AuthenticationAccount>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<TargetJob> TargetJobs => Set<TargetJob>();
    public DbSet<JobDescription> JobDescriptions => Set<JobDescription>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<RequiredSkill> RequiredSkills => Set<RequiredSkill>();
    public DbSet<SkillGapAnalysis> SkillGapAnalyses => Set<SkillGapAnalysis>();
    public DbSet<SkillGap> SkillGaps => Set<SkillGap>();
    public DbSet<MatchedSkill> MatchedSkills => Set<MatchedSkill>();
    public DbSet<ReadinessScore> ReadinessScores => Set<ReadinessScore>();
    public DbSet<StrengthWeaknessReport> StrengthWeaknessReports => Set<StrengthWeaknessReport>();
    public DbSet<QuestionCategory> QuestionCategories => Set<QuestionCategory>();
    public DbSet<QuestionTemplate> QuestionTemplates => Set<QuestionTemplate>();
    public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
    public DbSet<InterviewQuestion> InterviewQuestions => Set<InterviewQuestion>();
    public DbSet<InterviewAnswer> InterviewAnswers => Set<InterviewAnswer>();
    public DbSet<AnswerEvaluation> AnswerEvaluations => Set<AnswerEvaluation>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<ImprovementSuggestion> ImprovementSuggestions => Set<ImprovementSuggestion>();
    public DbSet<WeakCommunicationPattern> WeakCommunicationPatterns => Set<WeakCommunicationPattern>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<LearningRoadmap> LearningRoadmaps => Set<LearningRoadmap>();
    public DbSet<RoadmapMilestone> RoadmapMilestones => Set<RoadmapMilestone>();
    public DbSet<LearningActivity> LearningActivities => Set<LearningActivity>();
    public DbSet<RoadmapProgress> RoadmapProgresses => Set<RoadmapProgress>();
    public DbSet<RoadmapRecommendation> RoadmapRecommendations => Set<RoadmapRecommendation>();
    public DbSet<PracticeHistory> PracticeHistories => Set<PracticeHistory>();
    public DbSet<ProgressRecord> ProgressRecords => Set<ProgressRecord>();
    public DbSet<SkillImprovementTrend> SkillImprovementTrends => Set<SkillImprovementTrend>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UsageStatistic> UsageStatistics => Set<UsageStatistic>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureAuthenticationAccounts(modelBuilder);
        ConfigureUserProfiles(modelBuilder);
        ConfigureResumes(modelBuilder);

        ConfigureTargetJobs(modelBuilder);
        ConfigureJobDescriptions(modelBuilder);
        ConfigureSkills(modelBuilder);
        ConfigureRequiredSkills(modelBuilder);

        ConfigureSkillGapAnalyses(modelBuilder);
        ConfigureSkillGaps(modelBuilder);
        ConfigureMatchedSkills(modelBuilder);
        ConfigureReadinessScores(modelBuilder);
        ConfigureStrengthWeaknessReports(modelBuilder);

        ConfigureQuestionCategories(modelBuilder);
        ConfigureQuestionTemplates(modelBuilder);
        ConfigureInterviewSessions(modelBuilder);
        ConfigureInterviewQuestions(modelBuilder);
        ConfigureInterviewAnswers(modelBuilder);
        ConfigureAnswerEvaluations(modelBuilder);

        ConfigureFeedbacks(modelBuilder);
        ConfigureImprovementSuggestions(modelBuilder);
        ConfigureWeakCommunicationPatterns(modelBuilder);
        ConfigureRecommendations(modelBuilder);

        ConfigureLearningRoadmaps(modelBuilder);
        ConfigureRoadmapMilestones(modelBuilder);
        ConfigureLearningActivities(modelBuilder);
        ConfigureRoadmapProgress(modelBuilder);
        ConfigureRoadmapRecommendations(modelBuilder);

        ConfigurePracticeHistories(modelBuilder);
        ConfigureProgressRecords(modelBuilder);
        ConfigureSkillImprovementTrends(modelBuilder);
        ConfigureNotifications(modelBuilder);
        ConfigureUsageStatistics(modelBuilder);
        ConfigureSystemLogs(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserType)
                .HasColumnName("user_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(UserType.USER)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(UserStatus.ACTIVE)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2");

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.HasCheckConstraint(
                "chk_users_user_type",
                "[user_type] IN ('USER', 'ADMIN')"
            );

            entity.HasCheckConstraint(
                "chk_users_status",
                "[status] IN ('ACTIVE', 'INACTIVE', 'LOCKED', 'DELETED')"
            );
        });
    }

    private static void ConfigureAuthenticationAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthenticationAccount>(entity =>
        {
            entity.ToTable("authentication_accounts");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.IsVerified)
                .HasColumnName("is_verified")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2");

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.HasIndex(e => e.UserId)
                .IsUnique();

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("idx_auth_email");

            entity.HasOne(e => e.User)
                .WithOne(e => e.AuthenticationAccount)
                .HasForeignKey<AuthenticationAccount>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_auth_user");
        });
    }

    private static void ConfigureUserProfiles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(255);

            entity.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasMaxLength(50);

            entity.Property(e => e.EducationLevel)
                .HasColumnName("education_level")
                .HasMaxLength(255);

            entity.Property(e => e.CareerGoal)
                .HasColumnName("career_goal");

            entity.Property(e => e.PreferredLanguageCode)
                .HasColumnName("preferred_language_code")
                .HasMaxLength(10);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2");

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.HasIndex(e => e.UserId)
                .IsUnique();

            entity.HasOne(e => e.User)
                .WithOne(e => e.UserProfile)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_profile_user");
        });
    }

    private static void ConfigureResumes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resume>(entity =>
        {
            entity.ToTable("resumes");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.FileUrl)
                .HasColumnName("file_url")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.ParsedContent)
                .HasColumnName("parsed_content");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(e => e.UploadedAt)
                .HasColumnName("uploaded_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_resumes_user_id");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Resumes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_resume_user");
        });
    }
    private static void ConfigureTargetJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TargetJob>(entity =>
        {
            entity.ToTable("target_jobs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.JobTitle)
                .HasColumnName("job_title")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Industry)
                .HasColumnName("industry")
                .HasMaxLength(255);

            entity.Property(e => e.ExperienceLevel)
                .HasColumnName("experience_level")
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2");

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_target_jobs_user_id");

            entity.HasOne(e => e.User)
                .WithMany(e => e.TargetJobs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_target_job_user");
        });
    }
    private static void ConfigureJobDescriptions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobDescription>(entity =>
        {
            entity.ToTable("job_descriptions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.TargetJobId)
                .HasColumnName("target_job_id")
                .IsRequired();

            entity.Property(e => e.Content)
                .HasColumnName("content")
                .IsRequired();

            entity.Property(e => e.SourceType)
                .HasColumnName("source_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(JobDescriptionSourceType.MANUAL)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.TargetJobId)
                .HasDatabaseName("idx_job_descriptions_target_job_id");

            entity.HasCheckConstraint(
                "chk_jd_source_type",
                "[source_type] IN ('MANUAL', 'UPLOAD', 'URL', 'AI_GENERATED')"
            );

            entity.HasOne(e => e.TargetJob)
                .WithMany(e => e.JobDescriptions)
                .HasForeignKey(e => e.TargetJobId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_jd_target_job");
        });
    }
    private static void ConfigureSkills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("skills");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.SkillName)
                .HasColumnName("skill_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.SkillType)
                .HasColumnName("skill_type")
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.SkillName)
                .IsUnique();
            entity.HasData(
    new Skill { Id = 1, SkillName = "Communication", SkillType = "Soft Skill", CreatedAt = new DateTime(2026, 1, 1) },
    new Skill { Id = 2, SkillName = "Problem Solving", SkillType = "Soft Skill", CreatedAt = new DateTime(2026, 1, 1) },
    new Skill { Id = 3, SkillName = "Teamwork", SkillType = "Soft Skill", CreatedAt = new DateTime(2026, 1, 1) },
    new Skill { Id = 4, SkillName = "SQL", SkillType = "Technical Skill", CreatedAt = new DateTime(2026, 1, 1) },
    new Skill { Id = 5, SkillName = "Java", SkillType = "Technical Skill", CreatedAt = new DateTime(2026, 1, 1) },
    new Skill { Id = 6, SkillName = "Python", SkillType = "Technical Skill", CreatedAt = new DateTime(2026, 1, 1) },
    new Skill { Id = 7, SkillName = "System Design", SkillType = "Technical Skill", CreatedAt = new DateTime(2026, 1, 1) },
    new Skill { Id = 8, SkillName = "Data Analysis", SkillType = "Technical Skill", CreatedAt = new DateTime(2026, 1, 1) }
);
        });
    }
    private static void ConfigureRequiredSkills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequiredSkill>(entity =>
        {
            entity.ToTable("required_skills");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.JobDescriptionId)
                .HasColumnName("job_description_id")
                .IsRequired();

            entity.Property(e => e.SkillId)
                .HasColumnName("skill_id")
                .IsRequired();

            entity.Property(e => e.ImportanceLevel)
                .HasColumnName("importance_level")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasIndex(e => new { e.JobDescriptionId, e.SkillId })
                .IsUnique()
                .HasDatabaseName("uq_required_skill");

            entity.HasIndex(e => e.JobDescriptionId)
                .HasDatabaseName("idx_required_skills_jd_id");

            entity.HasIndex(e => e.SkillId)
                .HasDatabaseName("idx_required_skills_skill_id");

            entity.HasCheckConstraint(
                "chk_required_skill_importance",
                "[importance_level] IS NULL OR [importance_level] IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL')"
            );

            entity.HasOne(e => e.JobDescription)
                .WithMany(e => e.RequiredSkills)
                .HasForeignKey(e => e.JobDescriptionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_required_skill_jd");

            entity.HasOne(e => e.Skill)
                .WithMany(e => e.RequiredSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_required_skill_skill");
        });
    }
    private static void ConfigureSkillGapAnalyses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SkillGapAnalysis>(entity =>
        {
            entity.ToTable("skill_gap_analyses");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.ResumeId)
                .HasColumnName("resume_id")
                .IsRequired();

            entity.Property(e => e.JobDescriptionId)
                .HasColumnName("job_description_id")
                .IsRequired();

            entity.Property(e => e.AnalysisStatus)
                .HasColumnName("analysis_status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(AnalysisStatus.COMPLETED)
                .IsRequired();

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_skill_gap_analyses_user_id");

            entity.HasIndex(e => e.ResumeId)
                .HasDatabaseName("idx_skill_gap_analyses_resume_id");

            entity.HasIndex(e => e.JobDescriptionId)
                .HasDatabaseName("idx_skill_gap_analyses_jd_id");

            entity.HasCheckConstraint(
                "chk_sga_status",
                "[analysis_status] IN ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED')"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.SkillGapAnalyses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_sga_user");

            entity.HasOne(e => e.Resume)
                .WithMany(e => e.SkillGapAnalyses)
                .HasForeignKey(e => e.ResumeId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_sga_resume");

            entity.HasOne(e => e.JobDescription)
                .WithMany(e => e.SkillGapAnalyses)
                .HasForeignKey(e => e.JobDescriptionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_sga_jd");
        });
    }
    private static void ConfigureSkillGaps(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SkillGap>(entity =>
        {
            entity.ToTable("skill_gaps");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.SkillGapAnalysisId)
                .HasColumnName("skill_gap_analysis_id")
                .IsRequired();

            entity.Property(e => e.SkillId)
                .HasColumnName("skill_id")
                .IsRequired();

            entity.Property(e => e.GapLevel)
                .HasColumnName("gap_level")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.GapDescription)
                .HasColumnName("gap_description");

            entity.HasIndex(e => new { e.SkillGapAnalysisId, e.SkillId })
                .IsUnique()
                .HasDatabaseName("uq_skill_gap");

            entity.HasIndex(e => e.SkillGapAnalysisId)
                .HasDatabaseName("idx_skill_gaps_analysis_id");

            entity.HasIndex(e => e.SkillId)
                .HasDatabaseName("idx_skill_gaps_skill_id");

            entity.HasCheckConstraint(
                "chk_skill_gap_level",
                "[gap_level] IS NULL OR [gap_level] IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL')"
            );

            entity.HasOne(e => e.SkillGapAnalysis)
                .WithMany(e => e.SkillGaps)
                .HasForeignKey(e => e.SkillGapAnalysisId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_skill_gap_analysis");

            entity.HasOne(e => e.Skill)
                .WithMany(e => e.SkillGaps)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_skill_gap_skill");
        });
    }
    private static void ConfigureMatchedSkills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatchedSkill>(entity =>
        {
            entity.ToTable("matched_skills");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.SkillGapAnalysisId)
                .HasColumnName("skill_gap_analysis_id")
                .IsRequired();

            entity.Property(e => e.SkillId)
                .HasColumnName("skill_id")
                .IsRequired();

            entity.Property(e => e.MatchScore)
                .HasColumnName("match_score")
                .HasColumnType("decimal(5,4)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => new { e.SkillGapAnalysisId, e.SkillId })
                .IsUnique()
                .HasDatabaseName("uq_matched_skill");

            entity.HasIndex(e => e.SkillGapAnalysisId)
                .HasDatabaseName("idx_matched_skills_analysis_id");

            entity.HasIndex(e => e.SkillId)
                .HasDatabaseName("idx_matched_skills_skill_id");

            entity.HasCheckConstraint(
                "chk_matched_skill_score",
                "[match_score] >= 0 AND [match_score] <= 1"
            );

            entity.HasOne(e => e.SkillGapAnalysis)
                .WithMany(e => e.MatchedSkills)
                .HasForeignKey(e => e.SkillGapAnalysisId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_matched_skill_analysis");

            entity.HasOne(e => e.Skill)
                .WithMany(e => e.MatchedSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_matched_skill_skill");
        });
    }
    private static void ConfigureReadinessScores(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReadinessScore>(entity =>
        {
            entity.ToTable("readiness_scores");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.SkillGapAnalysisId)
                .HasColumnName("skill_gap_analysis_id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.Score)
                .HasColumnName("score")
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.ScoreType)
                .HasColumnName("score_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(ScoreType.OVERALL)
                .IsRequired();

            entity.Property(e => e.CalculatedAt)
                .HasColumnName("calculated_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => new { e.UserId, e.CalculatedAt })
                .HasDatabaseName("idx_readiness_scores_user_time")
                .IsDescending(false, true);

            entity.HasIndex(e => e.SkillGapAnalysisId)
                .HasDatabaseName("uq_readiness_score_analysis")
                .IsUnique()
                .HasFilter("[skill_gap_analysis_id] IS NOT NULL");

            entity.HasCheckConstraint(
                "chk_readiness_score_range",
                "[score] >= 0 AND [score] <= 100"
            );

            entity.HasCheckConstraint(
                "chk_readiness_score_type",
                "[score_type] IN ('OVERALL', 'TECHNICAL', 'COMMUNICATION', 'BEHAVIORAL')"
            );

            entity.HasOne(e => e.SkillGapAnalysis)
                .WithMany(e => e.ReadinessScores)
                .HasForeignKey(e => e.SkillGapAnalysisId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_score_analysis");

            entity.HasOne(e => e.User)
                .WithMany(e => e.ReadinessScores)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_score_user");
        });
    }
    private static void ConfigureStrengthWeaknessReports(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StrengthWeaknessReport>(entity =>
        {
            entity.ToTable("strength_weakness_reports");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.SkillGapAnalysisId)
                .HasColumnName("skill_gap_analysis_id")
                .IsRequired();

            entity.Property(e => e.ReportType)
                .HasColumnName("report_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Content)
                .HasColumnName("content")
                .IsRequired();

            entity.HasCheckConstraint(
                "chk_sw_report_type",
                "[report_type] IN ('STRENGTH', 'WEAKNESS')"
            );

            entity.HasOne(e => e.SkillGapAnalysis)
                .WithMany(e => e.StrengthWeaknessReports)
                .HasForeignKey(e => e.SkillGapAnalysisId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_sw_report_analysis");
        });
    }
    private static void ConfigureQuestionCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuestionCategory>(entity =>
        {
            entity.ToTable("question_categories");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.CategoryName)
                .HasColumnName("category_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.CategoryName)
                .IsUnique();

            entity.HasData(
                new QuestionCategory { Id = 1, CategoryName = "Behavioral" },
                new QuestionCategory { Id = 2, CategoryName = "Technical" },
                new QuestionCategory { Id = 3, CategoryName = "Communication" },
                new QuestionCategory { Id = 4, CategoryName = "Problem Solving" }
            );
        });
    }
    private static void ConfigureQuestionTemplates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuestionTemplate>(entity =>
        {
            entity.ToTable("question_templates");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            entity.Property(e => e.TemplateContent)
                .HasColumnName("template_content")
                .IsRequired();

            entity.Property(e => e.DifficultyLevel)
                .HasColumnName("difficulty_level")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(e => e.CreatedByAdminId)
                .HasColumnName("created_by_admin_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasCheckConstraint(
                "chk_template_difficulty",
                "[difficulty_level] IS NULL OR [difficulty_level] IN ('EASY', 'MEDIUM', 'HARD')"
            );

            entity.HasOne(e => e.Category)
                .WithMany(e => e.QuestionTemplates)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_template_category");

            entity.HasOne(e => e.CreatedByAdmin)
                .WithMany(e => e.CreatedQuestionTemplates)
                .HasForeignKey(e => e.CreatedByAdminId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_template_admin");
        });
    }
    private static void ConfigureInterviewSessions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InterviewSession>(entity =>
        {
            entity.ToTable("interview_sessions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.TargetJobId)
                .HasColumnName("target_job_id")
                .IsRequired();

            entity.Property(e => e.SessionStatus)
                .HasColumnName("session_status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(SessionStatus.IN_PROGRESS)
                .IsRequired();

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at")
                .HasColumnType("datetime2");

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_interview_sessions_user_id");

            entity.HasIndex(e => e.TargetJobId)
                .HasDatabaseName("idx_interview_sessions_target_job_id");

            entity.HasCheckConstraint(
                "chk_session_status",
                "[session_status] IN ('IN_PROGRESS', 'COMPLETED', 'CANCELLED')"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.InterviewSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_session_user");

            entity.HasOne(e => e.TargetJob)
                .WithMany(e => e.InterviewSessions)
                .HasForeignKey(e => e.TargetJobId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_session_target_job");
        });
    }
    private static void ConfigureInterviewQuestions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InterviewQuestion>(entity =>
        {
            entity.ToTable("interview_questions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.InterviewSessionId)
                .HasColumnName("interview_session_id")
                .IsRequired();

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            entity.Property(e => e.QuestionTemplateId)
                .HasColumnName("question_template_id");

            entity.Property(e => e.QuestionContent)
                .HasColumnName("question_content")
                .IsRequired();

            entity.Property(e => e.SkillFocus)
                .HasColumnName("skill_focus")
                .HasMaxLength(255);

            entity.Property(e => e.GeneratedBy)
                .HasColumnName("generated_by")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(QuestionGeneratedBy.AI)
                .IsRequired();

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.InterviewSessionId)
                .HasDatabaseName("idx_interview_questions_session_id");

            entity.HasIndex(e => e.CategoryId)
                .HasDatabaseName("idx_interview_questions_category_id");

            entity.HasCheckConstraint(
                "chk_question_generated_by",
                "[generated_by] IN ('AI', 'TEMPLATE', 'ADMIN')"
            );

            entity.HasOne(e => e.InterviewSession)
                .WithMany(e => e.InterviewQuestions)
                .HasForeignKey(e => e.InterviewSessionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_question_session");

            entity.HasOne(e => e.Category)
                .WithMany(e => e.InterviewQuestions)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_question_category");

            entity.HasOne(e => e.QuestionTemplate)
                .WithMany(e => e.InterviewQuestions)
                .HasForeignKey(e => e.QuestionTemplateId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_question_template");
        });
    }
    private static void ConfigureInterviewAnswers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InterviewAnswer>(entity =>
        {
            entity.ToTable("interview_answers");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.InterviewSessionId)
                .HasColumnName("interview_session_id")
                .IsRequired();

            entity.Property(e => e.InterviewQuestionId)
                .HasColumnName("interview_question_id")
                .IsRequired();

            entity.Property(e => e.AnswerText)
                .HasColumnName("answer_text")
                .IsRequired();

            entity.Property(e => e.SubmittedAt)
                .HasColumnName("submitted_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.InterviewSessionId)
                .HasDatabaseName("idx_interview_answers_session_id");

            entity.HasIndex(e => e.InterviewQuestionId)
                .HasDatabaseName("idx_interview_answers_question_id");

            entity.HasOne(e => e.InterviewSession)
                .WithMany(e => e.InterviewAnswers)
                .HasForeignKey(e => e.InterviewSessionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_answer_session");

            entity.HasOne(e => e.InterviewQuestion)
                .WithMany(e => e.InterviewAnswers)
                .HasForeignKey(e => e.InterviewQuestionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_answer_question");
        });
    }
    private static void ConfigureAnswerEvaluations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerEvaluation>(entity =>
        {
            entity.ToTable("answer_evaluations");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.InterviewAnswerId)
                .HasColumnName("interview_answer_id")
                .IsRequired();

            entity.Property(e => e.ClarityScore)
                .HasColumnName("clarity_score")
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.StructureScore)
                .HasColumnName("structure_score")
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.RelevanceScore)
                .HasColumnName("relevance_score")
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.OverallScore)
                .HasColumnName("overall_score")
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.Property(e => e.EvaluatedAt)
                .HasColumnName("evaluated_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.InterviewAnswerId)
                .IsUnique();

            entity.HasCheckConstraint(
                "chk_eval_clarity_score",
                "[clarity_score] IS NULL OR ([clarity_score] >= 0 AND [clarity_score] <= 100)"
            );

            entity.HasCheckConstraint(
                "chk_eval_structure_score",
                "[structure_score] IS NULL OR ([structure_score] >= 0 AND [structure_score] <= 100)"
            );

            entity.HasCheckConstraint(
                "chk_eval_relevance_score",
                "[relevance_score] IS NULL OR ([relevance_score] >= 0 AND [relevance_score] <= 100)"
            );

            entity.HasCheckConstraint(
                "chk_eval_overall_score",
                "[overall_score] IS NULL OR ([overall_score] >= 0 AND [overall_score] <= 100)"
            );

            entity.HasOne(e => e.InterviewAnswer)
                .WithOne(e => e.AnswerEvaluation)
                .HasForeignKey<AnswerEvaluation>(e => e.InterviewAnswerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_evaluation_answer");
        });
    }
    private static void ConfigureFeedbacks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ToTable("feedbacks");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.AnswerEvaluationId)
                .HasColumnName("answer_evaluation_id")
                .IsRequired();

            entity.Property(e => e.FeedbackContent)
                .HasColumnName("feedback_content")
                .IsRequired();

            entity.Property(e => e.FeedbackType)
                .HasColumnName("feedback_type")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.AnswerEvaluationId)
                .HasDatabaseName("idx_feedbacks_evaluation_id");

            entity.HasCheckConstraint(
                "chk_feedback_type",
                "[feedback_type] IS NULL OR [feedback_type] IN ('CLARITY', 'STRUCTURE', 'RELEVANCE', 'COMMUNICATION', 'OVERALL')"
            );

            entity.HasOne(e => e.AnswerEvaluation)
                .WithMany(e => e.Feedbacks)
                .HasForeignKey(e => e.AnswerEvaluationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_feedback_evaluation");
        });
    }
    private static void ConfigureImprovementSuggestions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImprovementSuggestion>(entity =>
        {
            entity.ToTable("improvement_suggestions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.FeedbackId)
                .HasColumnName("feedback_id")
                .IsRequired();

            entity.Property(e => e.SuggestionContent)
                .HasColumnName("suggestion_content")
                .IsRequired();

            entity.Property(e => e.PriorityLevel)
                .HasColumnName("priority_level")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasIndex(e => e.FeedbackId)
                .HasDatabaseName("idx_suggestions_feedback_id");

            entity.HasCheckConstraint(
                "chk_suggestion_priority",
                "[priority_level] IS NULL OR [priority_level] IN ('LOW', 'MEDIUM', 'HIGH')"
            );

            entity.HasOne(e => e.Feedback)
                .WithMany(e => e.ImprovementSuggestions)
                .HasForeignKey(e => e.FeedbackId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_suggestion_feedback");
        });
    }
    private static void ConfigureWeakCommunicationPatterns(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeakCommunicationPattern>(entity =>
        {
            entity.ToTable("weak_communication_patterns");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.FeedbackId)
                .HasColumnName("feedback_id")
                .IsRequired();

            entity.Property(e => e.PatternName)
                .HasColumnName("pattern_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.PatternDescription)
                .HasColumnName("pattern_description");

            entity.HasIndex(e => e.FeedbackId)
                .HasDatabaseName("idx_patterns_feedback_id");

            entity.HasOne(e => e.Feedback)
                .WithMany(e => e.WeakCommunicationPatterns)
                .HasForeignKey(e => e.FeedbackId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_pattern_feedback");
        });
    }
    private static void ConfigureRecommendations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.ToTable("recommendations");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.SkillGapAnalysisId)
                .HasColumnName("skill_gap_analysis_id")
                .IsRequired();

            entity.Property(e => e.SkillId)
                .HasColumnName("skill_id")
                .IsRequired();

            entity.Property(e => e.FeedbackId)
                .HasColumnName("feedback_id");

            entity.Property(e => e.RecommendationTitle)
                .HasColumnName("recommendation_title")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.RecommendationContent)
                .HasColumnName("recommendation_content")
                .IsRequired();

            entity.Property(e => e.RecommendationType)
                .HasColumnName("recommendation_type")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.PriorityLevel)
                .HasColumnName("priority_level")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(PriorityLevel.MEDIUM)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_recommendations_user_id");

            entity.HasIndex(e => e.SkillGapAnalysisId)
                .HasDatabaseName("idx_recommendations_analysis_id");

            entity.HasIndex(e => e.SkillId)
                .HasDatabaseName("idx_recommendations_skill_id");

            entity.HasIndex(e => new { e.SkillGapAnalysisId, e.SkillId })
                .IsUnique()
                .HasDatabaseName("uq_recommendation_analysis_skill");

            entity.HasCheckConstraint(
                "chk_recommendation_type",
                "[recommendation_type] IS NULL OR [recommendation_type] IN ('SKILL', 'INTERVIEW', 'COMMUNICATION', 'ROADMAP', 'GENERAL')"
            );

            entity.HasCheckConstraint(
                "chk_recommendation_priority",
                "[priority_level] IN ('LOW', 'MEDIUM', 'HIGH')"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.Recommendations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_recommendation_user");

            entity.HasOne(e => e.SkillGapAnalysis)
                .WithMany(e => e.Recommendations)
                .HasForeignKey(e => e.SkillGapAnalysisId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_recommendation_analysis");

            entity.HasOne(e => e.Skill)
                .WithMany()
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_recommendation_skill");

            entity.HasOne(e => e.Feedback)
                .WithMany(e => e.Recommendations)
                .HasForeignKey(e => e.FeedbackId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_recommendation_feedback");
        });
    }
    private static void ConfigureLearningRoadmaps(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningRoadmap>(entity =>
        {
            entity.ToTable("learning_roadmaps");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.TargetJobId)
                .HasColumnName("target_job_id");

            entity.Property(e => e.SkillGapAnalysisId)
                .HasColumnName("skill_gap_analysis_id");

            entity.Property(e => e.RoadmapTitle)
                .HasColumnName("roadmap_title")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.RoadmapStatus)
                .HasColumnName("roadmap_status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(RoadmapStatus.ACTIVE)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2");

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_learning_roadmaps_user_id");

            entity.HasIndex(e => e.TargetJobId)
                .HasDatabaseName("idx_learning_roadmaps_target_job_id");

            entity.HasCheckConstraint(
                "chk_roadmap_status",
                "[roadmap_status] IN ('ACTIVE', 'COMPLETED', 'ARCHIVED')"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.LearningRoadmaps)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_roadmap_user");

            entity.HasOne(e => e.TargetJob)
                .WithMany(e => e.LearningRoadmaps)
                .HasForeignKey(e => e.TargetJobId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_roadmap_target_job");

            entity.HasOne(e => e.SkillGapAnalysis)
                .WithMany(e => e.LearningRoadmaps)
                .HasForeignKey(e => e.SkillGapAnalysisId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_roadmap_analysis");
        });
    }
    private static void ConfigureRoadmapMilestones(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoadmapMilestone>(entity =>
        {
            entity.ToTable("roadmap_milestones");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.LearningRoadmapId)
                .HasColumnName("learning_roadmap_id")
                .IsRequired();

            entity.Property(e => e.MilestoneTitle)
                .HasColumnName("milestone_title")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.MilestoneOrder)
                .HasColumnName("milestone_order")
                .IsRequired();

            entity.Property(e => e.IsCompleted)
                .HasColumnName("is_completed")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.EstimatedDays)
                .HasColumnName("estimated_days")
                .HasDefaultValue(7)
                .IsRequired();

            entity.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("datetime2");

            entity.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("datetime2");

            entity.HasIndex(e => e.LearningRoadmapId)
                .HasDatabaseName("idx_milestones_roadmap_id");

            entity.HasIndex(e => new { e.LearningRoadmapId, e.MilestoneOrder })
                .IsUnique()
                .HasDatabaseName("uq_roadmap_milestone_order");

            entity.HasOne(e => e.LearningRoadmap)
                .WithMany(e => e.RoadmapMilestones)
                .HasForeignKey(e => e.LearningRoadmapId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_milestone_roadmap");
        });
    }
    private static void ConfigureLearningActivities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningActivity>(entity =>
        {
            entity.ToTable("learning_activities");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.RoadmapMilestoneId)
                .HasColumnName("roadmap_milestone_id")
                .IsRequired();

            entity.Property(e => e.SkillId)
                .HasColumnName("skill_id");

            entity.Property(e => e.ActivityTitle)
                .HasColumnName("activity_title")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.ActivityDescription)
                .HasColumnName("activity_description");

            entity.Property(e => e.ActivityType)
                .HasColumnName("activity_type")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.IsCompleted)
                .HasColumnName("is_completed")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.LanguageCode)
                .HasColumnName("language_code")
                .HasMaxLength(10);

            entity.HasIndex(e => e.RoadmapMilestoneId)
                .HasDatabaseName("idx_activities_milestone_id");

            entity.HasIndex(e => e.SkillId)
                .HasDatabaseName("idx_activities_skill_id");

            entity.HasCheckConstraint(
                "chk_activity_type",
                "[activity_type] IS NULL OR [activity_type] IN ('READING', 'PRACTICE', 'MOCK_INTERVIEW', 'QUIZ', 'OTHER')"
            );

            entity.HasOne(e => e.RoadmapMilestone)
                .WithMany(e => e.LearningActivities)
                .HasForeignKey(e => e.RoadmapMilestoneId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_activity_milestone");

            entity.HasOne(e => e.Skill)
                .WithMany(e => e.LearningActivities)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_activity_skill");
        });
    }
    private static void ConfigureRoadmapProgress(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoadmapProgress>(entity =>
        {
            entity.ToTable("roadmap_progress");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.LearningRoadmapId)
                .HasColumnName("learning_roadmap_id")
                .IsRequired();

            entity.Property(e => e.CompletionPercentage)
                .HasColumnName("completion_percentage")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.LastUpdatedAt)
                .HasColumnName("last_updated_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.LearningRoadmapId)
                .IsUnique();

            entity.HasCheckConstraint(
                "chk_roadmap_progress_percentage",
                "[completion_percentage] >= 0 AND [completion_percentage] <= 100"
            );

            entity.HasOne(e => e.LearningRoadmap)
                .WithOne(e => e.RoadmapProgress)
                .HasForeignKey<RoadmapProgress>(e => e.LearningRoadmapId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_progress_roadmap");
        });
    }
    private static void ConfigureRoadmapRecommendations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoadmapRecommendation>(entity =>
        {
            entity.ToTable("roadmap_recommendations");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.LearningRoadmapId)
                .HasColumnName("learning_roadmap_id")
                .IsRequired();

            entity.Property(e => e.RecommendationId)
                .HasColumnName("recommendation_id")
                .IsRequired();

            entity.HasIndex(e => new { e.LearningRoadmapId, e.RecommendationId })
                .IsUnique()
                .HasDatabaseName("uq_roadmap_recommendation");

            entity.HasOne(e => e.LearningRoadmap)
                .WithMany(e => e.RoadmapRecommendations)
                .HasForeignKey(e => e.LearningRoadmapId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_rr_roadmap");

            entity.HasOne(e => e.Recommendation)
                .WithMany(e => e.RoadmapRecommendations)
                .HasForeignKey(e => e.RecommendationId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_rr_recommendation");
        });
    }
    private static void ConfigurePracticeHistories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PracticeHistory>(entity =>
        {
            entity.ToTable("practice_histories");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.InterviewSessionId)
                .HasColumnName("interview_session_id");

            entity.Property(e => e.LearningActivityId)
                .HasColumnName("learning_activity_id");

            entity.Property(e => e.ActivityType)
                .HasColumnName("activity_type")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.PracticedAt)
                .HasColumnName("practiced_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_practice_histories_user_id");

            entity.HasIndex(e => e.InterviewSessionId)
                .HasDatabaseName("idx_practice_histories_session_id");

            entity.HasIndex(e => e.LearningActivityId)
                .HasDatabaseName("idx_practice_histories_activity_id");

            entity.HasCheckConstraint(
                "chk_practice_activity_type",
                "[activity_type] IS NULL OR [activity_type] IN ('READING', 'PRACTICE', 'MOCK_INTERVIEW', 'QUIZ', 'OTHER')"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.PracticeHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_practice_user");

            entity.HasOne(e => e.InterviewSession)
                .WithMany(e => e.PracticeHistories)
                .HasForeignKey(e => e.InterviewSessionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_practice_session");

            entity.HasOne(e => e.LearningActivity)
                .WithMany(e => e.PracticeHistories)
                .HasForeignKey(e => e.LearningActivityId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_practice_activity");
        });
    }
    private static void ConfigureProgressRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProgressRecord>(entity =>
        {
            entity.ToTable("progress_records");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.OverallProgress)
                .HasColumnName("overall_progress")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.RecordedAt)
                .HasColumnName("recorded_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => new { e.UserId, e.RecordedAt })
                .HasDatabaseName("idx_progress_records_user_time");

            entity.HasCheckConstraint(
                "chk_progress_record_range",
                "[overall_progress] >= 0 AND [overall_progress] <= 100"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.ProgressRecords)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_progress_record_user");
        });
    }
    private static void ConfigureSkillImprovementTrends(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SkillImprovementTrend>(entity =>
        {
            entity.ToTable("skill_improvement_trends");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.SkillId)
                .HasColumnName("skill_id")
                .IsRequired();

            entity.Property(e => e.ImprovementScore)
                .HasColumnName("improvement_score")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.RecordedAt)
                .HasColumnName("recorded_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => new { e.UserId, e.SkillId, e.RecordedAt })
                .HasDatabaseName("idx_skill_trends_user_skill_time");

            entity.HasCheckConstraint(
                "chk_skill_trend_score_range",
                "[improvement_score] >= 0 AND [improvement_score] <= 100"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.SkillImprovementTrends)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_skill_trend_user");

            entity.HasOne(e => e.Skill)
                .WithMany(e => e.SkillImprovementTrends)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_skill_trend_skill");
        });
    }
    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.NotificationType)
                .HasColumnName("notification_type")
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Message)
                .HasColumnName("message")
                .IsRequired();

            entity.Property(e => e.IsRead)
                .HasColumnName("is_read")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => new { e.UserId, e.IsRead })
                .HasDatabaseName("idx_notifications_user_read");

            entity.HasCheckConstraint(
                "chk_notification_type",
                "[notification_type] IS NULL OR [notification_type] IN ('ROADMAP', 'INTERVIEW', 'SYSTEM', 'REMINDER')"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_notification_user");
        });
    }
    private static void ConfigureUsageStatistics(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsageStatistic>(entity =>
        {
            entity.ToTable("usage_statistics");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.TotalSessions)
                .HasColumnName("total_sessions")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.TotalQuestionsAnswered)
                .HasColumnName("total_questions_answered")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.AverageScore)
                .HasColumnName("average_score")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.LastUpdatedAt)
                .HasColumnName("last_updated_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.UserId)
                .IsUnique()
                .HasDatabaseName("uq_usage_statistics_user");

            entity.HasCheckConstraint(
                "chk_usage_total_sessions",
                "[total_sessions] >= 0"
            );

            entity.HasCheckConstraint(
                "chk_usage_total_questions",
                "[total_questions_answered] >= 0"
            );

            entity.HasCheckConstraint(
                "chk_usage_average_score",
                "[average_score] >= 0 AND [average_score] <= 100"
            );

            entity.HasOne(e => e.User)
                .WithMany(e => e.UsageStatistics)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_usage_user");
        });
    }
    private static void ConfigureSystemLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.ToTable("system_logs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.Action)
                .HasColumnName("action")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_system_logs_user_id");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_system_logs_created_at");

            entity.HasOne(e => e.User)
                .WithMany(e => e.SystemLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_system_log_user");
        });
    }
}