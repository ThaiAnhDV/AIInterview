/* =========================================================
   AI INTERVIEW PLATFORM DATABASE SCRIPT
   DBMS: Microsoft SQL Server
   ========================================================= */

-- =========================================================
-- 1. CREATE DATABASE
-- =========================================================

IF DB_ID('AIInterviewPlatformDB') IS NOT NULL
BEGIN
    ALTER DATABASE AIInterviewPlatformDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AIInterviewPlatformDB;
END
GO

CREATE DATABASE AIInterviewPlatformDB;
GO

USE AIInterviewPlatformDB;
GO

-- =========================================================
-- 2. USERS & ACCOUNT MANAGEMENT
-- =========================================================

CREATE TABLE users (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_type NVARCHAR(50) NOT NULL DEFAULT 'USER',
    status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT chk_users_user_type
        CHECK (user_type IN ('USER', 'ADMIN')),

    CONSTRAINT chk_users_status
        CHECK (status IN ('ACTIVE', 'INACTIVE', 'LOCKED', 'DELETED'))
);
GO

CREATE TABLE authentication_accounts (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL UNIQUE,
    email NVARCHAR(255) NOT NULL UNIQUE,
    password_hash NVARCHAR(500) NOT NULL,
    is_verified BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT fk_auth_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);
GO

CREATE TABLE user_profiles (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL UNIQUE,
    full_name NVARCHAR(255) NULL,
    phone NVARCHAR(50) NULL,
    education_level NVARCHAR(255) NULL,
    career_goal NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT fk_profile_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);
GO

CREATE TABLE resumes (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    file_name NVARCHAR(255) NOT NULL,
    file_url NVARCHAR(500) NOT NULL,
    parsed_content NVARCHAR(MAX) NULL,
    is_active BIT NOT NULL DEFAULT 1,
    uploaded_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_resume_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);
GO

-- =========================================================
-- 3. TARGET JOB & JOB DESCRIPTION
-- =========================================================

CREATE TABLE target_jobs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    job_title NVARCHAR(255) NOT NULL,
    industry NVARCHAR(255) NULL,
    experience_level NVARCHAR(100) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT fk_target_job_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);
GO

CREATE TABLE job_descriptions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    target_job_id BIGINT NOT NULL,
    content NVARCHAR(MAX) NOT NULL,
    source_type NVARCHAR(50) NOT NULL DEFAULT 'MANUAL',
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_jd_target_job
        FOREIGN KEY (target_job_id) REFERENCES target_jobs(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_jd_source_type
        CHECK (source_type IN ('MANUAL', 'UPLOAD', 'URL', 'AI_GENERATED'))
);
GO

CREATE TABLE skills (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    skill_name NVARCHAR(255) NOT NULL UNIQUE,
    skill_type NVARCHAR(100) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE required_skills (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    job_description_id BIGINT NOT NULL,
    skill_id BIGINT NOT NULL,
    importance_level NVARCHAR(50) NULL,

    CONSTRAINT fk_required_skill_jd
        FOREIGN KEY (job_description_id) REFERENCES job_descriptions(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_required_skill_skill
        FOREIGN KEY (skill_id) REFERENCES skills(id),

    CONSTRAINT uq_required_skill
        UNIQUE (job_description_id, skill_id),

    CONSTRAINT chk_required_skill_importance
        CHECK (importance_level IS NULL OR importance_level IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL'))
);
GO

-- =========================================================
-- 4. SKILL GAP ANALYSIS
-- =========================================================

CREATE TABLE skill_gap_analyses (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    resume_id BIGINT NOT NULL,
    job_description_id BIGINT NOT NULL,
    analysis_status NVARCHAR(50) NOT NULL DEFAULT 'COMPLETED',
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_sga_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_sga_resume
        FOREIGN KEY (resume_id) REFERENCES resumes(id),

    CONSTRAINT fk_sga_jd
        FOREIGN KEY (job_description_id) REFERENCES job_descriptions(id),

    CONSTRAINT chk_sga_status
        CHECK (analysis_status IN ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED'))
);
GO

CREATE TABLE skill_gaps (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    skill_gap_analysis_id BIGINT NOT NULL,
    skill_id BIGINT NOT NULL,
    gap_level NVARCHAR(50) NULL,
    gap_description NVARCHAR(MAX) NULL,

    CONSTRAINT fk_skill_gap_analysis
        FOREIGN KEY (skill_gap_analysis_id) REFERENCES skill_gap_analyses(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_skill_gap_skill
        FOREIGN KEY (skill_id) REFERENCES skills(id),

    CONSTRAINT uq_skill_gap
        UNIQUE (skill_gap_analysis_id, skill_id),

    CONSTRAINT chk_skill_gap_level
        CHECK (gap_level IS NULL OR gap_level IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL'))
);
GO

CREATE TABLE readiness_scores (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    skill_gap_analysis_id BIGINT NULL,
    user_id BIGINT NOT NULL,
    score DECIMAL(5,2) NOT NULL,
    score_type NVARCHAR(50) NOT NULL DEFAULT 'OVERALL',
    calculated_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_score_analysis
        FOREIGN KEY (skill_gap_analysis_id) REFERENCES skill_gap_analyses(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_score_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT chk_readiness_score_range
        CHECK (score >= 0 AND score <= 100),

    CONSTRAINT chk_readiness_score_type
        CHECK (score_type IN ('OVERALL', 'TECHNICAL', 'COMMUNICATION', 'BEHAVIORAL'))
);
GO

CREATE TABLE strength_weakness_reports (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    skill_gap_analysis_id BIGINT NOT NULL,
    report_type NVARCHAR(50) NOT NULL,
    content NVARCHAR(MAX) NOT NULL,

    CONSTRAINT fk_sw_report_analysis
        FOREIGN KEY (skill_gap_analysis_id) REFERENCES skill_gap_analyses(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_sw_report_type
        CHECK (report_type IN ('STRENGTH', 'WEAKNESS'))
);
GO

-- =========================================================
-- 5. MOCK INTERVIEW
-- =========================================================

CREATE TABLE interview_sessions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    target_job_id BIGINT NOT NULL,
    session_status NVARCHAR(50) NOT NULL DEFAULT 'IN_PROGRESS',
    started_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    completed_at DATETIME2 NULL,

    CONSTRAINT fk_session_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_session_target_job
        FOREIGN KEY (target_job_id) REFERENCES target_jobs(id),

    CONSTRAINT chk_session_status
        CHECK (session_status IN ('IN_PROGRESS', 'COMPLETED', 'CANCELLED'))
);
GO

CREATE TABLE question_categories (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    category_name NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE question_templates (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    category_id BIGINT NOT NULL,
    template_content NVARCHAR(MAX) NOT NULL,
    difficulty_level NVARCHAR(50) NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_by_admin_id BIGINT NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_template_category
        FOREIGN KEY (category_id) REFERENCES question_categories(id),

    CONSTRAINT fk_template_admin
        FOREIGN KEY (created_by_admin_id) REFERENCES users(id),

    CONSTRAINT chk_template_difficulty
        CHECK (difficulty_level IS NULL OR difficulty_level IN ('EASY', 'MEDIUM', 'HARD'))
);
GO

CREATE TABLE interview_questions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    interview_session_id BIGINT NOT NULL,
    category_id BIGINT NOT NULL,
    question_template_id BIGINT NULL,
    question_content NVARCHAR(MAX) NOT NULL,
    generated_by NVARCHAR(50) NOT NULL DEFAULT 'AI',
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_question_session
        FOREIGN KEY (interview_session_id) REFERENCES interview_sessions(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_question_category
        FOREIGN KEY (category_id) REFERENCES question_categories(id),

    CONSTRAINT fk_question_template
        FOREIGN KEY (question_template_id) REFERENCES question_templates(id),

    CONSTRAINT chk_question_generated_by
        CHECK (generated_by IN ('AI', 'TEMPLATE', 'ADMIN'))
);
GO

CREATE TABLE interview_answers (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    interview_session_id BIGINT NOT NULL,
    interview_question_id BIGINT NOT NULL,
    answer_text NVARCHAR(MAX) NOT NULL,
    submitted_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_answer_session
        FOREIGN KEY (interview_session_id) REFERENCES interview_sessions(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_answer_question
        FOREIGN KEY (interview_question_id) REFERENCES interview_questions(id)
);
GO

CREATE TABLE answer_evaluations (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    interview_answer_id BIGINT NOT NULL UNIQUE,
    clarity_score DECIMAL(5,2) NULL,
    structure_score DECIMAL(5,2) NULL,
    relevance_score DECIMAL(5,2) NULL,
    overall_score DECIMAL(5,2) NULL,
    evaluated_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_evaluation_answer
        FOREIGN KEY (interview_answer_id) REFERENCES interview_answers(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_eval_clarity_score
        CHECK (clarity_score IS NULL OR (clarity_score >= 0 AND clarity_score <= 100)),

    CONSTRAINT chk_eval_structure_score
        CHECK (structure_score IS NULL OR (structure_score >= 0 AND structure_score <= 100)),

    CONSTRAINT chk_eval_relevance_score
        CHECK (relevance_score IS NULL OR (relevance_score >= 0 AND relevance_score <= 100)),

    CONSTRAINT chk_eval_overall_score
        CHECK (overall_score IS NULL OR (overall_score >= 0 AND overall_score <= 100))
);
GO

-- =========================================================
-- 6. FEEDBACK & RECOMMENDATION
-- =========================================================

CREATE TABLE feedbacks (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    answer_evaluation_id BIGINT NOT NULL,
    feedback_content NVARCHAR(MAX) NOT NULL,
    feedback_type NVARCHAR(50) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_feedback_evaluation
        FOREIGN KEY (answer_evaluation_id) REFERENCES answer_evaluations(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_feedback_type
        CHECK (feedback_type IS NULL OR feedback_type IN ('CLARITY', 'STRUCTURE', 'RELEVANCE', 'COMMUNICATION', 'OVERALL'))
);
GO

CREATE TABLE improvement_suggestions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    feedback_id BIGINT NOT NULL,
    suggestion_content NVARCHAR(MAX) NOT NULL,
    priority_level NVARCHAR(50) NULL,

    CONSTRAINT fk_suggestion_feedback
        FOREIGN KEY (feedback_id) REFERENCES feedbacks(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_suggestion_priority
        CHECK (priority_level IS NULL OR priority_level IN ('LOW', 'MEDIUM', 'HIGH'))
);
GO

CREATE TABLE weak_communication_patterns (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    feedback_id BIGINT NOT NULL,
    pattern_name NVARCHAR(255) NOT NULL,
    pattern_description NVARCHAR(MAX) NULL,

    CONSTRAINT fk_pattern_feedback
        FOREIGN KEY (feedback_id) REFERENCES feedbacks(id)
        ON DELETE CASCADE
);
GO

CREATE TABLE recommendations (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    skill_gap_analysis_id BIGINT NULL,
    feedback_id BIGINT NULL,
    recommendation_content NVARCHAR(MAX) NOT NULL,
    recommendation_type NVARCHAR(50) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_recommendation_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_recommendation_analysis
        FOREIGN KEY (skill_gap_analysis_id) REFERENCES skill_gap_analyses(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_recommendation_feedback
        FOREIGN KEY (feedback_id) REFERENCES feedbacks(id)
        ON DELETE SET NULL,

    CONSTRAINT chk_recommendation_type
        CHECK (recommendation_type IS NULL OR recommendation_type IN ('SKILL', 'INTERVIEW', 'COMMUNICATION', 'ROADMAP', 'GENERAL'))
);
GO

-- =========================================================
-- 7. LEARNING ROADMAP
-- =========================================================

CREATE TABLE learning_roadmaps (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    target_job_id BIGINT NULL,
    skill_gap_analysis_id BIGINT NULL,
    roadmap_title NVARCHAR(255) NOT NULL,
    roadmap_status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT fk_roadmap_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_roadmap_target_job
        FOREIGN KEY (target_job_id) REFERENCES target_jobs(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_roadmap_analysis
        FOREIGN KEY (skill_gap_analysis_id) REFERENCES skill_gap_analyses(id)
        ON DELETE SET NULL,

    CONSTRAINT chk_roadmap_status
        CHECK (roadmap_status IN ('ACTIVE', 'COMPLETED', 'ARCHIVED'))
);
GO

CREATE TABLE roadmap_milestones (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    learning_roadmap_id BIGINT NOT NULL,
    milestone_title NVARCHAR(255) NOT NULL,
    milestone_order INT NOT NULL,
    is_completed BIT NOT NULL DEFAULT 0,

    CONSTRAINT fk_milestone_roadmap
        FOREIGN KEY (learning_roadmap_id) REFERENCES learning_roadmaps(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_roadmap_milestone_order
        UNIQUE (learning_roadmap_id, milestone_order)
);
GO

CREATE TABLE learning_activities (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    roadmap_milestone_id BIGINT NOT NULL,
    skill_id BIGINT NULL,
    activity_title NVARCHAR(255) NOT NULL,
    activity_description NVARCHAR(MAX) NULL,
    activity_type NVARCHAR(50) NULL,
    is_completed BIT NOT NULL DEFAULT 0,

    CONSTRAINT fk_activity_milestone
        FOREIGN KEY (roadmap_milestone_id) REFERENCES roadmap_milestones(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_activity_skill
        FOREIGN KEY (skill_id) REFERENCES skills(id),

    CONSTRAINT chk_activity_type
        CHECK (activity_type IS NULL OR activity_type IN ('READING', 'PRACTICE', 'MOCK_INTERVIEW', 'QUIZ', 'OTHER'))
);
GO

CREATE TABLE roadmap_progress (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    learning_roadmap_id BIGINT NOT NULL UNIQUE,
    completion_percentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    last_updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_progress_roadmap
        FOREIGN KEY (learning_roadmap_id) REFERENCES learning_roadmaps(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_roadmap_progress_percentage
        CHECK (completion_percentage >= 0 AND completion_percentage <= 100)
);
GO

CREATE TABLE roadmap_recommendations (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    learning_roadmap_id BIGINT NOT NULL,
    recommendation_id BIGINT NOT NULL,

    CONSTRAINT fk_rr_roadmap
        FOREIGN KEY (learning_roadmap_id) REFERENCES learning_roadmaps(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_rr_recommendation
        FOREIGN KEY (recommendation_id) REFERENCES recommendations(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_roadmap_recommendation
        UNIQUE (learning_roadmap_id, recommendation_id)
);
GO

-- =========================================================
-- 8. PROGRESS TRACKING
-- =========================================================

CREATE TABLE practice_histories (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    interview_session_id BIGINT NOT NULL,
    practiced_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_history_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_history_session
        FOREIGN KEY (interview_session_id) REFERENCES interview_sessions(id)
);
GO

CREATE TABLE progress_records (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    readiness_score_id BIGINT NULL,
    progress_note NVARCHAR(MAX) NULL,
    recorded_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_progress_record_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_progress_record_score
        FOREIGN KEY (readiness_score_id) REFERENCES readiness_scores(id)
        ON DELETE SET NULL
);
GO

CREATE TABLE skill_improvement_trends (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    skill_id BIGINT NOT NULL,
    trend_score DECIMAL(5,2) NULL,
    measured_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_trend_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_trend_skill
        FOREIGN KEY (skill_id) REFERENCES skills(id),

    CONSTRAINT chk_trend_score
        CHECK (trend_score IS NULL OR (trend_score >= 0 AND trend_score <= 100))
);
GO

-- =========================================================
-- 9. NOTIFICATION
-- =========================================================

CREATE TABLE notifications (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NOT NULL,
    notification_type NVARCHAR(50) NOT NULL,
    title NVARCHAR(255) NOT NULL,
    message NVARCHAR(MAX) NOT NULL,
    delivery_channel NVARCHAR(50) NOT NULL DEFAULT 'IN_APP',
    is_read BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    related_roadmap_progress_id BIGINT NULL,
    related_readiness_score_id BIGINT NULL,

    CONSTRAINT fk_notification_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_notification_roadmap_progress
        FOREIGN KEY (related_roadmap_progress_id) REFERENCES roadmap_progress(id)
        ON DELETE SET NULL,

    CONSTRAINT fk_notification_readiness_score
        FOREIGN KEY (related_readiness_score_id) REFERENCES readiness_scores(id)
        ON DELETE SET NULL,

    CONSTRAINT chk_notification_type
        CHECK (notification_type IN ('REMINDER', 'READINESS_SCORE_IMPROVEMENT', 'SYSTEM')),

    CONSTRAINT chk_delivery_channel
        CHECK (delivery_channel IN ('IN_APP', 'EMAIL'))
);
GO

-- =========================================================
-- 10. ADMIN & MONITORING
-- =========================================================

CREATE TABLE usage_statistics (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    statistic_type NVARCHAR(100) NOT NULL,
    statistic_value DECIMAL(18,2) NOT NULL,
    measured_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE system_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id BIGINT NULL,
    action_type NVARCHAR(100) NOT NULL,
    action_description NVARCHAR(MAX) NULL,
    ip_address NVARCHAR(100) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_system_log_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE SET NULL
);
GO

-- =========================================================
-- 11. INDEXES
-- =========================================================

-- User & account
CREATE UNIQUE INDEX idx_auth_email
ON authentication_accounts(email);
GO

CREATE INDEX idx_resumes_user_id
ON resumes(user_id);
GO

CREATE INDEX idx_target_jobs_user_id
ON target_jobs(user_id);
GO

-- Job description & skills
CREATE INDEX idx_job_descriptions_target_job_id
ON job_descriptions(target_job_id);
GO

CREATE INDEX idx_required_skills_jd_id
ON required_skills(job_description_id);
GO

CREATE INDEX idx_required_skills_skill_id
ON required_skills(skill_id);
GO

-- Skill gap analysis
CREATE INDEX idx_skill_gap_analyses_user_id
ON skill_gap_analyses(user_id);
GO

CREATE INDEX idx_skill_gap_analyses_resume_id
ON skill_gap_analyses(resume_id);
GO

CREATE INDEX idx_skill_gap_analyses_jd_id
ON skill_gap_analyses(job_description_id);
GO

CREATE INDEX idx_skill_gaps_analysis_id
ON skill_gaps(skill_gap_analysis_id);
GO

CREATE INDEX idx_skill_gaps_skill_id
ON skill_gaps(skill_id);
GO

CREATE INDEX idx_readiness_scores_user_time
ON readiness_scores(user_id, calculated_at DESC);
GO

-- Interview
CREATE INDEX idx_interview_sessions_user_id
ON interview_sessions(user_id);
GO

CREATE INDEX idx_interview_sessions_target_job_id
ON interview_sessions(target_job_id);
GO

CREATE INDEX idx_interview_questions_session_id
ON interview_questions(interview_session_id);
GO

CREATE INDEX idx_interview_questions_category_id
ON interview_questions(category_id);
GO

CREATE INDEX idx_interview_answers_session_id
ON interview_answers(interview_session_id);
GO

CREATE INDEX idx_interview_answers_question_id
ON interview_answers(interview_question_id);
GO

-- Feedback & recommendation
CREATE INDEX idx_feedbacks_evaluation_id
ON feedbacks(answer_evaluation_id);
GO

CREATE INDEX idx_suggestions_feedback_id
ON improvement_suggestions(feedback_id);
GO

CREATE INDEX idx_patterns_feedback_id
ON weak_communication_patterns(feedback_id);
GO

CREATE INDEX idx_recommendations_user_id
ON recommendations(user_id);
GO

CREATE INDEX idx_recommendations_analysis_id
ON recommendations(skill_gap_analysis_id);
GO

-- Roadmap
CREATE INDEX idx_learning_roadmaps_user_id
ON learning_roadmaps(user_id);
GO

CREATE INDEX idx_learning_roadmaps_target_job_id
ON learning_roadmaps(target_job_id);
GO

CREATE INDEX idx_milestones_roadmap_id
ON roadmap_milestones(learning_roadmap_id);
GO

CREATE INDEX idx_activities_milestone_id
ON learning_activities(roadmap_milestone_id);
GO

CREATE INDEX idx_activities_skill_id
ON learning_activities(skill_id);
GO

-- Progress tracking
CREATE INDEX idx_practice_histories_user_time
ON practice_histories(user_id, practiced_at DESC);
GO

CREATE INDEX idx_progress_records_user_time
ON progress_records(user_id, recorded_at DESC);
GO

CREATE INDEX idx_skill_trends_user_skill_time
ON skill_improvement_trends(user_id, skill_id, measured_at DESC);
GO

-- Notification
CREATE INDEX idx_notifications_user_read
ON notifications(user_id, is_read, created_at DESC);
GO

CREATE INDEX idx_notifications_type
ON notifications(notification_type);
GO

-- Admin monitoring
CREATE INDEX idx_system_logs_created_at
ON system_logs(created_at DESC);
GO

CREATE INDEX idx_system_logs_user_id
ON system_logs(user_id);
GO

CREATE INDEX idx_usage_statistics_type_time
ON usage_statistics(statistic_type, measured_at DESC);
GO

-- =========================================================
-- 12. SEED DATA
-- =========================================================

INSERT INTO question_categories (category_name)
VALUES
    ('Behavioral'),
    ('Technical'),
    ('Communication'),
    ('Problem Solving');
GO

INSERT INTO skills (skill_name, skill_type)
VALUES
    ('Communication', 'Soft Skill'),
    ('Problem Solving', 'Soft Skill'),
    ('Teamwork', 'Soft Skill'),
    ('SQL', 'Technical Skill'),
    ('Java', 'Technical Skill'),
    ('Python', 'Technical Skill'),
    ('System Design', 'Technical Skill'),
    ('Data Analysis', 'Technical Skill');
GO

-- =========================================================
-- 13. SAMPLE ADMIN ACCOUNT
-- Password hash below is placeholder only.
-- In real system, password must be hashed by application layer.
-- =========================================================

INSERT INTO users (user_type, status)
VALUES ('ADMIN', 'ACTIVE');
GO

DECLARE @AdminUserId BIGINT;
SET @AdminUserId = SCOPE_IDENTITY();

INSERT INTO authentication_accounts (
    user_id,
    email,
    password_hash,
    is_verified
)
VALUES (
    @AdminUserId,
    'admin@aiinterview.local',
    'PLACEHOLDER_HASHED_PASSWORD',
    1
);

INSERT INTO user_profiles (
    user_id,
    full_name,
    education_level,
    career_goal
)
VALUES (
    @AdminUserId,
    'System Administrator',
    NULL,
    'Manage AI Interview Platform'
);
GO

-- =========================================================
-- 14. VERIFY TABLES
-- =========================================================

SELECT 
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO