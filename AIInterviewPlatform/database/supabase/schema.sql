-- AI Interview Platform schema for Supabase/PostgreSQL
-- Generated from SQL Server script. Run this in Supabase SQL Editor or via a PostgreSQL client.
-- =========================================================
-- 2. USERS & ACCOUNT MANAGEMENT
-- =========================================================

CREATE TABLE users (
    id BIGSERIAL PRIMARY KEY,
    user_type VARCHAR(50) NOT NULL DEFAULT 'USER',
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NULL,

    CONSTRAINT chk_users_user_type
        CHECK (user_type IN ('USER', 'ADMIN')),

    CONSTRAINT chk_users_status
        CHECK (status IN ('ACTIVE', 'INACTIVE', 'LOCKED', 'DELETED'))
);

CREATE TABLE authentication_accounts (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NULL,

    CONSTRAINT fk_auth_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);

CREATE TABLE user_profiles (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL UNIQUE,
    full_name VARCHAR(255) NULL,
    phone VARCHAR(50) NULL,
    education_level VARCHAR(255) NULL,
    career_goal TEXT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NULL,

    CONSTRAINT fk_profile_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);

CREATE TABLE resumes (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_url VARCHAR(500) NOT NULL,
    parsed_content TEXT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    uploaded_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_resume_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);

-- =========================================================
-- 3. TARGET JOB & JOB DESCRIPTION
-- =========================================================

CREATE TABLE target_jobs (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    job_title VARCHAR(255) NOT NULL,
    industry VARCHAR(255) NULL,
    experience_level VARCHAR(100) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NULL,

    CONSTRAINT fk_target_job_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
);

CREATE TABLE job_descriptions (
    id BIGSERIAL PRIMARY KEY,
    target_job_id BIGINT NOT NULL,
    content TEXT NOT NULL,
    source_type VARCHAR(50) NOT NULL DEFAULT 'MANUAL',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_jd_target_job
        FOREIGN KEY (target_job_id) REFERENCES target_jobs(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_jd_source_type
        CHECK (source_type IN ('MANUAL', 'UPLOAD', 'URL', 'AI_GENERATED'))
);

CREATE TABLE skills (
    id BIGSERIAL PRIMARY KEY,
    skill_name VARCHAR(255) NOT NULL UNIQUE,
    skill_type VARCHAR(100) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE required_skills (
    id BIGSERIAL PRIMARY KEY,
    job_description_id BIGINT NOT NULL,
    skill_id BIGINT NOT NULL,
    importance_level VARCHAR(50) NULL,

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

-- =========================================================
-- 4. SKILL GAP ANALYSIS
-- =========================================================

CREATE TABLE skill_gap_analyses (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    resume_id BIGINT NOT NULL,
    job_description_id BIGINT NOT NULL,
    analysis_status VARCHAR(50) NOT NULL DEFAULT 'COMPLETED',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_sga_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_sga_resume
        FOREIGN KEY (resume_id) REFERENCES resumes(id),

    CONSTRAINT fk_sga_jd
        FOREIGN KEY (job_description_id) REFERENCES job_descriptions(id),

    CONSTRAINT chk_sga_status
        CHECK (analysis_status IN ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED'))
);

CREATE TABLE skill_gaps (
    id BIGSERIAL PRIMARY KEY,
    skill_gap_analysis_id BIGINT NOT NULL,
    skill_id BIGINT NOT NULL,
    gap_level VARCHAR(50) NULL,
    gap_description TEXT NULL,

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

CREATE TABLE readiness_scores (
    id BIGSERIAL PRIMARY KEY,
    skill_gap_analysis_id BIGINT NULL,
    user_id BIGINT NOT NULL,
    score NUMERIC(5,2) NOT NULL,
    score_type VARCHAR(50) NOT NULL DEFAULT 'OVERALL',
    calculated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

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

CREATE TABLE strength_weakness_reports (
    id BIGSERIAL PRIMARY KEY,
    skill_gap_analysis_id BIGINT NOT NULL,
    report_type VARCHAR(50) NOT NULL,
    content TEXT NOT NULL,

    CONSTRAINT fk_sw_report_analysis
        FOREIGN KEY (skill_gap_analysis_id) REFERENCES skill_gap_analyses(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_sw_report_type
        CHECK (report_type IN ('STRENGTH', 'WEAKNESS'))
);

-- =========================================================
-- 5. MOCK INTERVIEW
-- =========================================================

CREATE TABLE interview_sessions (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    target_job_id BIGINT NOT NULL,
    session_status VARCHAR(50) NOT NULL DEFAULT 'IN_PROGRESS',
    started_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,

    CONSTRAINT fk_session_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_session_target_job
        FOREIGN KEY (target_job_id) REFERENCES target_jobs(id),

    CONSTRAINT chk_session_status
        CHECK (session_status IN ('IN_PROGRESS', 'COMPLETED', 'CANCELLED'))
);

CREATE TABLE question_categories (
    id BIGSERIAL PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE question_templates (
    id BIGSERIAL PRIMARY KEY,
    category_id BIGINT NOT NULL,
    template_content TEXT NOT NULL,
    difficulty_level VARCHAR(50) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_by_admin_id BIGINT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_template_category
        FOREIGN KEY (category_id) REFERENCES question_categories(id),

    CONSTRAINT fk_template_admin
        FOREIGN KEY (created_by_admin_id) REFERENCES users(id),

    CONSTRAINT chk_template_difficulty
        CHECK (difficulty_level IS NULL OR difficulty_level IN ('EASY', 'MEDIUM', 'HARD'))
);

CREATE TABLE interview_questions (
    id BIGSERIAL PRIMARY KEY,
    interview_session_id BIGINT NOT NULL,
    category_id BIGINT NOT NULL,
    question_template_id BIGINT NULL,
    question_content TEXT NOT NULL,
    generated_by VARCHAR(50) NOT NULL DEFAULT 'AI',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

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

CREATE TABLE interview_answers (
    id BIGSERIAL PRIMARY KEY,
    interview_session_id BIGINT NOT NULL,
    interview_question_id BIGINT NOT NULL,
    answer_text TEXT NOT NULL,
    submitted_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_answer_session
        FOREIGN KEY (interview_session_id) REFERENCES interview_sessions(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_answer_question
        FOREIGN KEY (interview_question_id) REFERENCES interview_questions(id)
);

CREATE TABLE answer_evaluations (
    id BIGSERIAL PRIMARY KEY,
    interview_answer_id BIGINT NOT NULL UNIQUE,
    clarity_score NUMERIC(5,2) NULL,
    structure_score NUMERIC(5,2) NULL,
    relevance_score NUMERIC(5,2) NULL,
    overall_score NUMERIC(5,2) NULL,
    evaluated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

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

-- =========================================================
-- 6. FEEDBACK & RECOMMENDATION
-- =========================================================

CREATE TABLE feedbacks (
    id BIGSERIAL PRIMARY KEY,
    answer_evaluation_id BIGINT NOT NULL,
    feedback_content TEXT NOT NULL,
    feedback_type VARCHAR(50) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_feedback_evaluation
        FOREIGN KEY (answer_evaluation_id) REFERENCES answer_evaluations(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_feedback_type
        CHECK (feedback_type IS NULL OR feedback_type IN ('CLARITY', 'STRUCTURE', 'RELEVANCE', 'COMMUNICATION', 'OVERALL'))
);

CREATE TABLE improvement_suggestions (
    id BIGSERIAL PRIMARY KEY,
    feedback_id BIGINT NOT NULL,
    suggestion_content TEXT NOT NULL,
    priority_level VARCHAR(50) NULL,

    CONSTRAINT fk_suggestion_feedback
        FOREIGN KEY (feedback_id) REFERENCES feedbacks(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_suggestion_priority
        CHECK (priority_level IS NULL OR priority_level IN ('LOW', 'MEDIUM', 'HIGH'))
);

CREATE TABLE weak_communication_patterns (
    id BIGSERIAL PRIMARY KEY,
    feedback_id BIGINT NOT NULL,
    pattern_name VARCHAR(255) NOT NULL,
    pattern_description TEXT NULL,

    CONSTRAINT fk_pattern_feedback
        FOREIGN KEY (feedback_id) REFERENCES feedbacks(id)
        ON DELETE CASCADE
);

CREATE TABLE recommendations (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    skill_gap_analysis_id BIGINT NULL,
    feedback_id BIGINT NULL,
    recommendation_content TEXT NOT NULL,
    recommendation_type VARCHAR(50) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

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

-- =========================================================
-- 7. LEARNING ROADMAP
-- =========================================================

CREATE TABLE learning_roadmaps (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    target_job_id BIGINT NULL,
    skill_gap_analysis_id BIGINT NULL,
    roadmap_title VARCHAR(255) NOT NULL,
    roadmap_status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NULL,

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

CREATE TABLE roadmap_milestones (
    id BIGSERIAL PRIMARY KEY,
    learning_roadmap_id BIGINT NOT NULL,
    milestone_title VARCHAR(255) NOT NULL,
    milestone_order INT NOT NULL,
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_milestone_roadmap
        FOREIGN KEY (learning_roadmap_id) REFERENCES learning_roadmaps(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_roadmap_milestone_order
        UNIQUE (learning_roadmap_id, milestone_order)
);

CREATE TABLE learning_activities (
    id BIGSERIAL PRIMARY KEY,
    roadmap_milestone_id BIGINT NOT NULL,
    skill_id BIGINT NULL,
    activity_title VARCHAR(255) NOT NULL,
    activity_description TEXT NULL,
    activity_type VARCHAR(50) NULL,
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT fk_activity_milestone
        FOREIGN KEY (roadmap_milestone_id) REFERENCES roadmap_milestones(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_activity_skill
        FOREIGN KEY (skill_id) REFERENCES skills(id),

    CONSTRAINT chk_activity_type
        CHECK (activity_type IS NULL OR activity_type IN ('READING', 'PRACTICE', 'MOCK_INTERVIEW', 'QUIZ', 'OTHER'))
);

CREATE TABLE roadmap_progress (
    id BIGSERIAL PRIMARY KEY,
    learning_roadmap_id BIGINT NOT NULL UNIQUE,
    completion_percentage NUMERIC(5,2) NOT NULL DEFAULT 0,
    last_updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_progress_roadmap
        FOREIGN KEY (learning_roadmap_id) REFERENCES learning_roadmaps(id)
        ON DELETE CASCADE,

    CONSTRAINT chk_roadmap_progress_percentage
        CHECK (completion_percentage >= 0 AND completion_percentage <= 100)
);

CREATE TABLE roadmap_recommendations (
    id BIGSERIAL PRIMARY KEY,
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

-- =========================================================
-- 8. PROGRESS TRACKING
-- =========================================================

CREATE TABLE practice_histories (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    interview_session_id BIGINT NOT NULL,
    practiced_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_history_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_history_session
        FOREIGN KEY (interview_session_id) REFERENCES interview_sessions(id)
);

CREATE TABLE progress_records (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    readiness_score_id BIGINT NULL,
    progress_note TEXT NULL,
    recorded_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_progress_record_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_progress_record_score
        FOREIGN KEY (readiness_score_id) REFERENCES readiness_scores(id)
        ON DELETE SET NULL
);

CREATE TABLE skill_improvement_trends (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    skill_id BIGINT NOT NULL,
    trend_score NUMERIC(5,2) NULL,
    measured_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_trend_user
        FOREIGN KEY (user_id) REFERENCES users(id),

    CONSTRAINT fk_trend_skill
        FOREIGN KEY (skill_id) REFERENCES skills(id),

    CONSTRAINT chk_trend_score
        CHECK (trend_score IS NULL OR (trend_score >= 0 AND trend_score <= 100))
);

-- =========================================================
-- 9. NOTIFICATION
-- =========================================================

CREATE TABLE notifications (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    notification_type VARCHAR(50) NOT NULL,
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    delivery_channel VARCHAR(50) NOT NULL DEFAULT 'IN_APP',
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

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

-- =========================================================
-- 10. ADMIN & MONITORING
-- =========================================================

CREATE TABLE usage_statistics (
    id BIGSERIAL PRIMARY KEY,
    statistic_type VARCHAR(100) NOT NULL,
    statistic_value NUMERIC(18,2) NOT NULL,
    measured_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE system_logs (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NULL,
    action_type VARCHAR(100) NOT NULL,
    action_description TEXT NULL,
    ip_address VARCHAR(100) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_system_log_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE SET NULL
);

-- =========================================================
-- 11. INDEXES
-- =========================================================

-- User & account
CREATE UNIQUE INDEX idx_auth_email
ON authentication_accounts(email);

CREATE INDEX idx_resumes_user_id
ON resumes(user_id);

CREATE INDEX idx_target_jobs_user_id
ON target_jobs(user_id);

-- Job description & skills
CREATE INDEX idx_job_descriptions_target_job_id
ON job_descriptions(target_job_id);

CREATE INDEX idx_required_skills_jd_id
ON required_skills(job_description_id);

CREATE INDEX idx_required_skills_skill_id
ON required_skills(skill_id);

-- Skill gap analysis
CREATE INDEX idx_skill_gap_analyses_user_id
ON skill_gap_analyses(user_id);

CREATE INDEX idx_skill_gap_analyses_resume_id
ON skill_gap_analyses(resume_id);

CREATE INDEX idx_skill_gap_analyses_jd_id
ON skill_gap_analyses(job_description_id);

CREATE INDEX idx_skill_gaps_analysis_id
ON skill_gaps(skill_gap_analysis_id);

CREATE INDEX idx_skill_gaps_skill_id
ON skill_gaps(skill_id);

CREATE INDEX idx_readiness_scores_user_time
ON readiness_scores(user_id, calculated_at DESC);

-- Interview
CREATE INDEX idx_interview_sessions_user_id
ON interview_sessions(user_id);

CREATE INDEX idx_interview_sessions_target_job_id
ON interview_sessions(target_job_id);

CREATE INDEX idx_interview_questions_session_id
ON interview_questions(interview_session_id);

CREATE INDEX idx_interview_questions_category_id
ON interview_questions(category_id);

CREATE INDEX idx_interview_answers_session_id
ON interview_answers(interview_session_id);

CREATE INDEX idx_interview_answers_question_id
ON interview_answers(interview_question_id);

-- Feedback & recommendation
CREATE INDEX idx_feedbacks_evaluation_id
ON feedbacks(answer_evaluation_id);

CREATE INDEX idx_suggestions_feedback_id
ON improvement_suggestions(feedback_id);

CREATE INDEX idx_patterns_feedback_id
ON weak_communication_patterns(feedback_id);

CREATE INDEX idx_recommendations_user_id
ON recommendations(user_id);

CREATE INDEX idx_recommendations_analysis_id
ON recommendations(skill_gap_analysis_id);

-- Roadmap
CREATE INDEX idx_learning_roadmaps_user_id
ON learning_roadmaps(user_id);

CREATE INDEX idx_learning_roadmaps_target_job_id
ON learning_roadmaps(target_job_id);

CREATE INDEX idx_milestones_roadmap_id
ON roadmap_milestones(learning_roadmap_id);

CREATE INDEX idx_activities_milestone_id
ON learning_activities(roadmap_milestone_id);

CREATE INDEX idx_activities_skill_id
ON learning_activities(skill_id);

-- Progress tracking
CREATE INDEX idx_practice_histories_user_time
ON practice_histories(user_id, practiced_at DESC);

CREATE INDEX idx_progress_records_user_time
ON progress_records(user_id, recorded_at DESC);

CREATE INDEX idx_skill_trends_user_skill_time
ON skill_improvement_trends(user_id, skill_id, measured_at DESC);

-- Notification
CREATE INDEX idx_notifications_user_read
ON notifications(user_id, is_read, created_at DESC);

CREATE INDEX idx_notifications_type
ON notifications(notification_type);

-- Admin monitoring
CREATE INDEX idx_system_logs_created_at
ON system_logs(created_at DESC);

CREATE INDEX idx_system_logs_user_id
ON system_logs(user_id);

CREATE INDEX idx_usage_statistics_type_time
ON usage_statistics(statistic_type, measured_at DESC);

-- =========================================================
-- 12. SEED DATA
-- =========================================================

INSERT INTO question_categories (category_name)
VALUES
    ('Behavioral'),
    ('Technical'),
    ('Communication'),
    ('Problem Solving');

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

-- =========================================================
-- 13. SAMPLE ADMIN ACCOUNT
-- Default admin login:
-- Email: admin@aiinterview.local
-- Password: Admin@123456
-- Password hash is generated with BCrypt.Net-Next.
-- =========================================================

WITH admin_user AS (
    INSERT INTO users (user_type, status)
    VALUES ('ADMIN', 'ACTIVE')
    RETURNING id
), admin_account AS (
    INSERT INTO authentication_accounts (
        user_id,
        email,
        password_hash,
        is_verified
    )
    SELECT
        id,
        'admin@aiinterview.local',
        '$2a$11$OyGY89bvQ8GiaKJkFxScce9RKyp/.D7RR0b7PoPm8Qy/FHU8YOTdS',
        TRUE
    FROM admin_user
)
INSERT INTO user_profiles (
    user_id,
    full_name,
    education_level,
    career_goal
)
SELECT
    id,
    'System Administrator',
    NULL,
    'Manage AI Interview Platform'
FROM admin_user;

