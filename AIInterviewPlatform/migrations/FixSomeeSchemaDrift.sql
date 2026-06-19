-- Aligns the existing Somee database with the current EF model.
-- Safe to run multiple times. It only adds missing columns/indexes/constraints.

IF COL_LENGTH('recommendations', 'skill_id') IS NULL
BEGIN
    ALTER TABLE recommendations ADD skill_id BIGINT NULL;
END;

IF COL_LENGTH('recommendations', 'recommendation_title') IS NULL
BEGIN
    ALTER TABLE recommendations ADD recommendation_title NVARCHAR(255) NULL;
END;

IF COL_LENGTH('recommendations', 'recommendation_title') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE recommendations
        SET recommendation_title = LEFT(COALESCE(NULLIF(recommendation_content, N''''), N''Recommendation''), 255)
        WHERE recommendation_title IS NULL;
    ');

    EXEC(N'
        IF NOT EXISTS (SELECT 1 FROM recommendations WHERE recommendation_title IS NULL)
            ALTER TABLE recommendations ALTER COLUMN recommendation_title NVARCHAR(255) NOT NULL;
    ');
END;

IF COL_LENGTH('recommendations', 'priority_level') IS NULL
BEGIN
    ALTER TABLE recommendations ADD priority_level NVARCHAR(50) NOT NULL
        CONSTRAINT DF_recommendations_priority_level DEFAULT N'MEDIUM';
END;

IF COL_LENGTH('recommendations', 'skill_id') IS NOT NULL
BEGIN
    EXEC(N'
        IF NOT EXISTS (SELECT 1 FROM recommendations WHERE skill_id IS NULL)
            ALTER TABLE recommendations ALTER COLUMN skill_id BIGINT NOT NULL;
    ');
END;

IF COL_LENGTH('roadmap_milestones', 'estimated_days') IS NULL
BEGIN
    ALTER TABLE roadmap_milestones ADD estimated_days INT NOT NULL
        CONSTRAINT DF_roadmap_milestones_estimated_days DEFAULT 7;
END;

IF COL_LENGTH('roadmap_milestones', 'start_date') IS NULL
BEGIN
    ALTER TABLE roadmap_milestones ADD start_date DATETIME2 NULL;
END;

IF COL_LENGTH('roadmap_milestones', 'end_date') IS NULL
BEGIN
    ALTER TABLE roadmap_milestones ADD end_date DATETIME2 NULL;
END;

IF COL_LENGTH('recommendations', 'skill_id') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = N'idx_recommendations_skill_id'
         AND object_id = OBJECT_ID(N'recommendations')
   )
BEGIN
    EXEC(N'CREATE INDEX idx_recommendations_skill_id ON recommendations(skill_id);');
END;

IF COL_LENGTH('recommendations', 'skill_gap_analysis_id') IS NOT NULL
   AND COL_LENGTH('recommendations', 'skill_id') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = N'uq_recommendation_analysis_skill'
         AND object_id = OBJECT_ID(N'recommendations')
   )
BEGIN
    EXEC(N'
        CREATE UNIQUE INDEX uq_recommendation_analysis_skill
            ON recommendations(skill_gap_analysis_id, skill_id)
    ');
END;

IF COL_LENGTH('recommendations', 'skill_id') IS NOT NULL
   AND OBJECT_ID(N'fk_recommendation_skill', N'F') IS NULL
BEGIN
    EXEC(N'
        ALTER TABLE recommendations WITH CHECK ADD CONSTRAINT fk_recommendation_skill
            FOREIGN KEY (skill_id) REFERENCES skills(id);
    ');
END;

IF COL_LENGTH('recommendations', 'priority_level') IS NOT NULL
   AND OBJECT_ID(N'chk_recommendation_priority', N'C') IS NULL
BEGIN
    EXEC(N'
        ALTER TABLE recommendations ADD CONSTRAINT chk_recommendation_priority
            CHECK (priority_level IN (''LOW'', ''MEDIUM'', ''HIGH''));
    ');
END;

PRINT 'Somee schema drift fix completed.';
