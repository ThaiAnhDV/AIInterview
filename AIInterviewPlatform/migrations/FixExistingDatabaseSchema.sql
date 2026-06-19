-- Fix an existing AIInterviewPlatformDB that was created before EF migrations
-- were recorded. Safe to run multiple times.

IF COL_LENGTH('users', 'language_code') IS NULL
    ALTER TABLE users ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('authentication_accounts', 'language_code') IS NULL
    ALTER TABLE authentication_accounts ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('user_profiles', 'preferred_language_code') IS NULL
    ALTER TABLE user_profiles ADD preferred_language_code NVARCHAR(10) NULL;

IF COL_LENGTH('user_profiles', 'language_code') IS NULL
    ALTER TABLE user_profiles ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('target_jobs', 'language_code') IS NULL
    ALTER TABLE target_jobs ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('skill_gap_analyses', 'language_code') IS NULL
    ALTER TABLE skill_gap_analyses ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('interview_sessions', 'language_code') IS NULL
    ALTER TABLE interview_sessions ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('interview_questions', 'skill_focus') IS NULL
    ALTER TABLE interview_questions ADD skill_focus NVARCHAR(255) NULL;

IF COL_LENGTH('interview_questions', 'language_code') IS NULL
    ALTER TABLE interview_questions ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('answer_evaluations', 'language_code') IS NULL
    ALTER TABLE answer_evaluations ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('feedbacks', 'language_code') IS NULL
    ALTER TABLE feedbacks ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('learning_roadmaps', 'language_code') IS NULL
    ALTER TABLE learning_roadmaps ADD language_code NVARCHAR(10) NULL;

IF COL_LENGTH('learning_activities', 'language_code') IS NULL
    ALTER TABLE learning_activities ADD language_code NVARCHAR(10) NULL;

IF OBJECT_ID('__EFMigrationsHistory', 'U') IS NULL
BEGIN
    CREATE TABLE __EFMigrationsHistory (
        MigrationId NVARCHAR(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
        ProductVersion NVARCHAR(32) NOT NULL
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM __EFMigrationsHistory
    WHERE MigrationId = N'20260615070905_InitialCreate'
)
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260615070905_InitialCreate', N'8.0.10');
END;

PRINT 'Existing database schema fix completed.';
