-- Adds the matched_skills table required by the current Skill Gap flow.
-- Safe to run multiple times.

IF OBJECT_ID(N'matched_skills', N'U') IS NULL
BEGIN
    CREATE TABLE matched_skills (
        id BIGINT IDENTITY(1,1) NOT NULL,
        skill_gap_analysis_id BIGINT NOT NULL,
        skill_id BIGINT NOT NULL,
        match_score DECIMAL(5,4) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_matched_skills_created_at DEFAULT GETDATE(),
        CONSTRAINT PK_matched_skills PRIMARY KEY (id),
        CONSTRAINT chk_matched_skill_score CHECK (match_score >= 0 AND match_score <= 1),
        CONSTRAINT fk_matched_skill_analysis FOREIGN KEY (skill_gap_analysis_id)
            REFERENCES skill_gap_analyses(id) ON DELETE CASCADE,
        CONSTRAINT fk_matched_skill_skill FOREIGN KEY (skill_id)
            REFERENCES skills(id)
    );

    CREATE UNIQUE INDEX uq_matched_skill
        ON matched_skills(skill_gap_analysis_id, skill_id);

    CREATE INDEX idx_matched_skills_analysis_id
        ON matched_skills(skill_gap_analysis_id);

    CREATE INDEX idx_matched_skills_skill_id
        ON matched_skills(skill_id);
END;

PRINT 'matched_skills schema fix completed.';
