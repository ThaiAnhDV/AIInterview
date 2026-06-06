using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitSupabasePostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "question_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    skill_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    skill_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "USER"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("chk_users_status", "[status] IN ('ACTIVE', 'INACTIVE', 'LOCKED', 'DELETED')");
                    table.CheckConstraint("chk_users_user_type", "[user_type] IN ('USER', 'ADMIN')");
                });

            migrationBuilder.CreateTable(
                name: "authentication_accounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authentication_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_auth_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    notification_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    message = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.CheckConstraint("chk_notification_type", "[notification_type] IS NULL OR [notification_type] IN ('ROADMAP', 'INTERVIEW', 'SYSTEM', 'REMINDER')");
                    table.ForeignKey(
                        name: "fk_notification_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "progress_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    overall_progress = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    recorded_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_progress_records", x => x.id);
                    table.CheckConstraint("chk_progress_record_range", "[overall_progress] >= 0 AND [overall_progress] <= 100");
                    table.ForeignKey(
                        name: "fk_progress_record_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "question_templates",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    template_content = table.Column<string>(type: "text", nullable: false),
                    difficulty_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_by_admin_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_templates", x => x.id);
                    table.CheckConstraint("chk_template_difficulty", "[difficulty_level] IS NULL OR [difficulty_level] IN ('EASY', 'MEDIUM', 'HARD')");
                    table.ForeignKey(
                        name: "fk_template_admin",
                        column: x => x.created_by_admin_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_template_category",
                        column: x => x.category_id,
                        principalTable: "question_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "resumes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    parsed_content = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resumes", x => x.id);
                    table.ForeignKey(
                        name: "fk_resume_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skill_improvement_trends",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    improvement_score = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    recorded_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_improvement_trends", x => x.id);
                    table.CheckConstraint("chk_skill_trend_score_range", "[improvement_score] >= 0 AND [improvement_score] <= 100");
                    table.ForeignKey(
                        name: "fk_skill_trend_skill",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_skill_trend_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "system_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_system_log_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "target_jobs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    job_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    industry = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    experience_level = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_target_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_target_job_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usage_statistics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    total_sessions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_questions_answered = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    average_score = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    last_updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_statistics", x => x.id);
                    table.CheckConstraint("chk_usage_average_score", "[average_score] >= 0 AND [average_score] <= 100");
                    table.CheckConstraint("chk_usage_total_questions", "[total_questions_answered] >= 0");
                    table.CheckConstraint("chk_usage_total_sessions", "[total_sessions] >= 0");
                    table.ForeignKey(
                        name: "fk_usage_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    education_level = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    career_goal = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_profile_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "interview_sessions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    target_job_id = table.Column<long>(type: "bigint", nullable: false),
                    session_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "IN_PROGRESS"),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_sessions", x => x.id);
                    table.CheckConstraint("chk_session_status", "[session_status] IN ('IN_PROGRESS', 'COMPLETED', 'CANCELLED')");
                    table.ForeignKey(
                        name: "fk_session_target_job",
                        column: x => x.target_job_id,
                        principalTable: "target_jobs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_session_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_descriptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    target_job_id = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    source_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "MANUAL"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_descriptions", x => x.id);
                    table.CheckConstraint("chk_jd_source_type", "[source_type] IN ('MANUAL', 'UPLOAD', 'URL', 'AI_GENERATED')");
                    table.ForeignKey(
                        name: "fk_jd_target_job",
                        column: x => x.target_job_id,
                        principalTable: "target_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "interview_questions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    interview_session_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    question_template_id = table.Column<long>(type: "bigint", nullable: true),
                    question_content = table.Column<string>(type: "text", nullable: false),
                    skill_focus = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    generated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "AI"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_questions", x => x.id);
                    table.CheckConstraint("chk_question_generated_by", "[generated_by] IN ('AI', 'TEMPLATE', 'ADMIN')");
                    table.ForeignKey(
                        name: "fk_question_category",
                        column: x => x.category_id,
                        principalTable: "question_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_question_session",
                        column: x => x.interview_session_id,
                        principalTable: "interview_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_question_template",
                        column: x => x.question_template_id,
                        principalTable: "question_templates",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "required_skills",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_description_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    importance_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_required_skills", x => x.id);
                    table.CheckConstraint("chk_required_skill_importance", "[importance_level] IS NULL OR [importance_level] IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL')");
                    table.ForeignKey(
                        name: "fk_required_skill_jd",
                        column: x => x.job_description_id,
                        principalTable: "job_descriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_required_skill_skill",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "skill_gap_analyses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    resume_id = table.Column<long>(type: "bigint", nullable: false),
                    job_description_id = table.Column<long>(type: "bigint", nullable: false),
                    analysis_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "COMPLETED"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_gap_analyses", x => x.id);
                    table.CheckConstraint("chk_sga_status", "[analysis_status] IN ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED')");
                    table.ForeignKey(
                        name: "fk_sga_jd",
                        column: x => x.job_description_id,
                        principalTable: "job_descriptions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_sga_resume",
                        column: x => x.resume_id,
                        principalTable: "resumes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_sga_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "interview_answers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    interview_session_id = table.Column<long>(type: "bigint", nullable: false),
                    interview_question_id = table.Column<long>(type: "bigint", nullable: false),
                    answer_text = table.Column<string>(type: "text", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_answer_question",
                        column: x => x.interview_question_id,
                        principalTable: "interview_questions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_answer_session",
                        column: x => x.interview_session_id,
                        principalTable: "interview_sessions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "learning_roadmaps",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    target_job_id = table.Column<long>(type: "bigint", nullable: true),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: true),
                    roadmap_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    roadmap_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_roadmaps", x => x.id);
                    table.CheckConstraint("chk_roadmap_status", "[roadmap_status] IN ('ACTIVE', 'COMPLETED', 'ARCHIVED')");
                    table.ForeignKey(
                        name: "fk_roadmap_analysis",
                        column: x => x.skill_gap_analysis_id,
                        principalTable: "skill_gap_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_roadmap_target_job",
                        column: x => x.target_job_id,
                        principalTable: "target_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_roadmap_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "readiness_scores",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    score = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    score_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "OVERALL"),
                    calculated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_readiness_scores", x => x.id);
                    table.CheckConstraint("chk_readiness_score_range", "[score] >= 0 AND [score] <= 100");
                    table.CheckConstraint("chk_readiness_score_type", "[score_type] IN ('OVERALL', 'TECHNICAL', 'COMMUNICATION', 'BEHAVIORAL')");
                    table.ForeignKey(
                        name: "fk_score_analysis",
                        column: x => x.skill_gap_analysis_id,
                        principalTable: "skill_gap_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_score_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "skill_gaps",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    gap_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    gap_description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_gaps", x => x.id);
                    table.CheckConstraint("chk_skill_gap_level", "[gap_level] IS NULL OR [gap_level] IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL')");
                    table.ForeignKey(
                        name: "fk_skill_gap_analysis",
                        column: x => x.skill_gap_analysis_id,
                        principalTable: "skill_gap_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_skill_gap_skill",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "strength_weakness_reports",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: false),
                    report_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_strength_weakness_reports", x => x.id);
                    table.CheckConstraint("chk_sw_report_type", "[report_type] IN ('STRENGTH', 'WEAKNESS')");
                    table.ForeignKey(
                        name: "fk_sw_report_analysis",
                        column: x => x.skill_gap_analysis_id,
                        principalTable: "skill_gap_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "answer_evaluations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    interview_answer_id = table.Column<long>(type: "bigint", nullable: false),
                    clarity_score = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    structure_score = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    relevance_score = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    overall_score = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    evaluated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answer_evaluations", x => x.id);
                    table.CheckConstraint("chk_eval_clarity_score", "[clarity_score] IS NULL OR ([clarity_score] >= 0 AND [clarity_score] <= 100)");
                    table.CheckConstraint("chk_eval_overall_score", "[overall_score] IS NULL OR ([overall_score] >= 0 AND [overall_score] <= 100)");
                    table.CheckConstraint("chk_eval_relevance_score", "[relevance_score] IS NULL OR ([relevance_score] >= 0 AND [relevance_score] <= 100)");
                    table.CheckConstraint("chk_eval_structure_score", "[structure_score] IS NULL OR ([structure_score] >= 0 AND [structure_score] <= 100)");
                    table.ForeignKey(
                        name: "fk_evaluation_answer",
                        column: x => x.interview_answer_id,
                        principalTable: "interview_answers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roadmap_milestones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    learning_roadmap_id = table.Column<long>(type: "bigint", nullable: false),
                    milestone_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    milestone_order = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roadmap_milestones", x => x.id);
                    table.ForeignKey(
                        name: "fk_milestone_roadmap",
                        column: x => x.learning_roadmap_id,
                        principalTable: "learning_roadmaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roadmap_progress",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    learning_roadmap_id = table.Column<long>(type: "bigint", nullable: false),
                    completion_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    last_updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roadmap_progress", x => x.id);
                    table.CheckConstraint("chk_roadmap_progress_percentage", "[completion_percentage] >= 0 AND [completion_percentage] <= 100");
                    table.ForeignKey(
                        name: "fk_progress_roadmap",
                        column: x => x.learning_roadmap_id,
                        principalTable: "learning_roadmaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    answer_evaluation_id = table.Column<long>(type: "bigint", nullable: false),
                    feedback_content = table.Column<string>(type: "text", nullable: false),
                    feedback_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedbacks", x => x.id);
                    table.CheckConstraint("chk_feedback_type", "[feedback_type] IS NULL OR [feedback_type] IN ('CLARITY', 'STRUCTURE', 'RELEVANCE', 'COMMUNICATION', 'OVERALL')");
                    table.ForeignKey(
                        name: "fk_feedback_evaluation",
                        column: x => x.answer_evaluation_id,
                        principalTable: "answer_evaluations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learning_activities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    roadmap_milestone_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: true),
                    activity_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    activity_description = table.Column<string>(type: "text", nullable: true),
                    activity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_activities", x => x.id);
                    table.CheckConstraint("chk_activity_type", "[activity_type] IS NULL OR [activity_type] IN ('READING', 'PRACTICE', 'MOCK_INTERVIEW', 'QUIZ', 'OTHER')");
                    table.ForeignKey(
                        name: "fk_activity_milestone",
                        column: x => x.roadmap_milestone_id,
                        principalTable: "roadmap_milestones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_skill",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "improvement_suggestions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    feedback_id = table.Column<long>(type: "bigint", nullable: false),
                    suggestion_content = table.Column<string>(type: "text", nullable: false),
                    priority_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_improvement_suggestions", x => x.id);
                    table.CheckConstraint("chk_suggestion_priority", "[priority_level] IS NULL OR [priority_level] IN ('LOW', 'MEDIUM', 'HIGH')");
                    table.ForeignKey(
                        name: "fk_suggestion_feedback",
                        column: x => x.feedback_id,
                        principalTable: "feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recommendations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: true),
                    feedback_id = table.Column<long>(type: "bigint", nullable: true),
                    recommendation_content = table.Column<string>(type: "text", nullable: false),
                    recommendation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendations", x => x.id);
                    table.CheckConstraint("chk_recommendation_type", "[recommendation_type] IS NULL OR [recommendation_type] IN ('SKILL', 'INTERVIEW', 'COMMUNICATION', 'ROADMAP', 'GENERAL')");
                    table.ForeignKey(
                        name: "fk_recommendation_analysis",
                        column: x => x.skill_gap_analysis_id,
                        principalTable: "skill_gap_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_recommendation_feedback",
                        column: x => x.feedback_id,
                        principalTable: "feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_recommendation_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "weak_communication_patterns",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    feedback_id = table.Column<long>(type: "bigint", nullable: false),
                    pattern_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    pattern_description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weak_communication_patterns", x => x.id);
                    table.ForeignKey(
                        name: "fk_pattern_feedback",
                        column: x => x.feedback_id,
                        principalTable: "feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "practice_histories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    interview_session_id = table.Column<long>(type: "bigint", nullable: true),
                    learning_activity_id = table.Column<long>(type: "bigint", nullable: true),
                    activity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    practiced_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_practice_histories", x => x.id);
                    table.CheckConstraint("chk_practice_activity_type", "[activity_type] IS NULL OR [activity_type] IN ('READING', 'PRACTICE', 'MOCK_INTERVIEW', 'QUIZ', 'OTHER')");
                    table.ForeignKey(
                        name: "fk_practice_activity",
                        column: x => x.learning_activity_id,
                        principalTable: "learning_activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_practice_session",
                        column: x => x.interview_session_id,
                        principalTable: "interview_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_practice_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "roadmap_recommendations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    learning_roadmap_id = table.Column<long>(type: "bigint", nullable: false),
                    recommendation_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roadmap_recommendations", x => x.id);
                    table.ForeignKey(
                        name: "fk_rr_recommendation",
                        column: x => x.recommendation_id,
                        principalTable: "recommendations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_rr_roadmap",
                        column: x => x.learning_roadmap_id,
                        principalTable: "learning_roadmaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "question_categories",
                columns: new[] { "id", "category_name" },
                values: new object[,]
                {
                    { 1L, "Behavioral" },
                    { 2L, "Technical" },
                    { 3L, "Communication" },
                    { 4L, "Problem Solving" }
                });

            migrationBuilder.InsertData(
                table: "skills",
                columns: new[] { "id", "created_at", "skill_name", "skill_type" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Communication", "Soft Skill" },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Problem Solving", "Soft Skill" },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Teamwork", "Soft Skill" },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SQL", "Technical Skill" },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Java", "Technical Skill" },
                    { 6L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Python", "Technical Skill" },
                    { 7L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System Design", "Technical Skill" },
                    { 8L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Data Analysis", "Technical Skill" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_answer_evaluations_interview_answer_id",
                table: "answer_evaluations",
                column: "interview_answer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_auth_email",
                table: "authentication_accounts",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_authentication_accounts_user_id",
                table: "authentication_accounts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_feedbacks_evaluation_id",
                table: "feedbacks",
                column: "answer_evaluation_id");

            migrationBuilder.CreateIndex(
                name: "idx_suggestions_feedback_id",
                table: "improvement_suggestions",
                column: "feedback_id");

            migrationBuilder.CreateIndex(
                name: "idx_interview_answers_question_id",
                table: "interview_answers",
                column: "interview_question_id");

            migrationBuilder.CreateIndex(
                name: "idx_interview_answers_session_id",
                table: "interview_answers",
                column: "interview_session_id");

            migrationBuilder.CreateIndex(
                name: "idx_interview_questions_category_id",
                table: "interview_questions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_interview_questions_session_id",
                table: "interview_questions",
                column: "interview_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_interview_questions_question_template_id",
                table: "interview_questions",
                column: "question_template_id");

            migrationBuilder.CreateIndex(
                name: "idx_interview_sessions_target_job_id",
                table: "interview_sessions",
                column: "target_job_id");

            migrationBuilder.CreateIndex(
                name: "idx_interview_sessions_user_id",
                table: "interview_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_job_descriptions_target_job_id",
                table: "job_descriptions",
                column: "target_job_id");

            migrationBuilder.CreateIndex(
                name: "idx_activities_milestone_id",
                table: "learning_activities",
                column: "roadmap_milestone_id");

            migrationBuilder.CreateIndex(
                name: "idx_activities_skill_id",
                table: "learning_activities",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "idx_learning_roadmaps_target_job_id",
                table: "learning_roadmaps",
                column: "target_job_id");

            migrationBuilder.CreateIndex(
                name: "idx_learning_roadmaps_user_id",
                table: "learning_roadmaps",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_roadmaps_skill_gap_analysis_id",
                table: "learning_roadmaps",
                column: "skill_gap_analysis_id");

            migrationBuilder.CreateIndex(
                name: "idx_notifications_user_read",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "idx_practice_histories_activity_id",
                table: "practice_histories",
                column: "learning_activity_id");

            migrationBuilder.CreateIndex(
                name: "idx_practice_histories_session_id",
                table: "practice_histories",
                column: "interview_session_id");

            migrationBuilder.CreateIndex(
                name: "idx_practice_histories_user_id",
                table: "practice_histories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_progress_records_user_time",
                table: "progress_records",
                columns: new[] { "user_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_question_categories_category_name",
                table: "question_categories",
                column: "category_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_question_templates_category_id",
                table: "question_templates",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_templates_created_by_admin_id",
                table: "question_templates",
                column: "created_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "idx_readiness_scores_user_time",
                table: "readiness_scores",
                columns: new[] { "user_id", "calculated_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_readiness_scores_skill_gap_analysis_id",
                table: "readiness_scores",
                column: "skill_gap_analysis_id");

            migrationBuilder.CreateIndex(
                name: "idx_recommendations_analysis_id",
                table: "recommendations",
                column: "skill_gap_analysis_id");

            migrationBuilder.CreateIndex(
                name: "idx_recommendations_user_id",
                table: "recommendations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_recommendations_feedback_id",
                table: "recommendations",
                column: "feedback_id");

            migrationBuilder.CreateIndex(
                name: "idx_required_skills_jd_id",
                table: "required_skills",
                column: "job_description_id");

            migrationBuilder.CreateIndex(
                name: "idx_required_skills_skill_id",
                table: "required_skills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "uq_required_skill",
                table: "required_skills",
                columns: new[] { "job_description_id", "skill_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_resumes_user_id",
                table: "resumes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_milestones_roadmap_id",
                table: "roadmap_milestones",
                column: "learning_roadmap_id");

            migrationBuilder.CreateIndex(
                name: "uq_roadmap_milestone_order",
                table: "roadmap_milestones",
                columns: new[] { "learning_roadmap_id", "milestone_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roadmap_progress_learning_roadmap_id",
                table: "roadmap_progress",
                column: "learning_roadmap_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roadmap_recommendations_recommendation_id",
                table: "roadmap_recommendations",
                column: "recommendation_id");

            migrationBuilder.CreateIndex(
                name: "uq_roadmap_recommendation",
                table: "roadmap_recommendations",
                columns: new[] { "learning_roadmap_id", "recommendation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_skill_gap_analyses_jd_id",
                table: "skill_gap_analyses",
                column: "job_description_id");

            migrationBuilder.CreateIndex(
                name: "idx_skill_gap_analyses_resume_id",
                table: "skill_gap_analyses",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "idx_skill_gap_analyses_user_id",
                table: "skill_gap_analyses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_skill_gaps_analysis_id",
                table: "skill_gaps",
                column: "skill_gap_analysis_id");

            migrationBuilder.CreateIndex(
                name: "idx_skill_gaps_skill_id",
                table: "skill_gaps",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "uq_skill_gap",
                table: "skill_gaps",
                columns: new[] { "skill_gap_analysis_id", "skill_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_skill_trends_user_skill_time",
                table: "skill_improvement_trends",
                columns: new[] { "user_id", "skill_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_skill_improvement_trends_skill_id",
                table: "skill_improvement_trends",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_skills_skill_name",
                table: "skills",
                column: "skill_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_strength_weakness_reports_skill_gap_analysis_id",
                table: "strength_weakness_reports",
                column: "skill_gap_analysis_id");

            migrationBuilder.CreateIndex(
                name: "idx_system_logs_created_at",
                table: "system_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_system_logs_user_id",
                table: "system_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_target_jobs_user_id",
                table: "target_jobs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_usage_statistics_user",
                table: "usage_statistics",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_user_id",
                table: "user_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_patterns_feedback_id",
                table: "weak_communication_patterns",
                column: "feedback_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authentication_accounts");

            migrationBuilder.DropTable(
                name: "improvement_suggestions");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "practice_histories");

            migrationBuilder.DropTable(
                name: "progress_records");

            migrationBuilder.DropTable(
                name: "readiness_scores");

            migrationBuilder.DropTable(
                name: "required_skills");

            migrationBuilder.DropTable(
                name: "roadmap_progress");

            migrationBuilder.DropTable(
                name: "roadmap_recommendations");

            migrationBuilder.DropTable(
                name: "skill_gaps");

            migrationBuilder.DropTable(
                name: "skill_improvement_trends");

            migrationBuilder.DropTable(
                name: "strength_weakness_reports");

            migrationBuilder.DropTable(
                name: "system_logs");

            migrationBuilder.DropTable(
                name: "usage_statistics");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "weak_communication_patterns");

            migrationBuilder.DropTable(
                name: "learning_activities");

            migrationBuilder.DropTable(
                name: "recommendations");

            migrationBuilder.DropTable(
                name: "roadmap_milestones");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "feedbacks");

            migrationBuilder.DropTable(
                name: "learning_roadmaps");

            migrationBuilder.DropTable(
                name: "answer_evaluations");

            migrationBuilder.DropTable(
                name: "skill_gap_analyses");

            migrationBuilder.DropTable(
                name: "interview_answers");

            migrationBuilder.DropTable(
                name: "job_descriptions");

            migrationBuilder.DropTable(
                name: "resumes");

            migrationBuilder.DropTable(
                name: "interview_questions");

            migrationBuilder.DropTable(
                name: "interview_sessions");

            migrationBuilder.DropTable(
                name: "question_templates");

            migrationBuilder.DropTable(
                name: "target_jobs");

            migrationBuilder.DropTable(
                name: "question_categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
