-- Migration: AddSkillFocusToInterviewQuestion
-- Description: Adds the skill_focus column to the interview_questions table

-- Add skill_focus column (nullable nvarchar(255))
ALTER TABLE interview_questions
ADD skill_focus NVARCHAR(255) NULL;

PRINT 'Migration completed: skill_focus column added to interview_questions table';
