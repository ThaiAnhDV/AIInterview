using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_MockInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "interview_sessions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    target_job_id = table.Column<long>(type: "bigint", nullable: false),
                    session_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "IN_PROGRESS"),
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
                name: "question_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "question_templates",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    template_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    difficulty_level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                name: "interview_questions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    interview_session_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    question_template_id = table.Column<long>(type: "bigint", nullable: true),
                    question_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    generated_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "AI"),
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
                name: "interview_answers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    interview_session_id = table.Column<long>(type: "bigint", nullable: false),
                    interview_question_id = table.Column<long>(type: "bigint", nullable: false),
                    answer_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "answer_evaluations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    interview_answer_id = table.Column<long>(type: "bigint", nullable: false),
                    clarity_score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    structure_score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    relevance_score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    overall_score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_answer_evaluations_interview_answer_id",
                table: "answer_evaluations",
                column: "interview_answer_id",
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "answer_evaluations");

            migrationBuilder.DropTable(
                name: "interview_answers");

            migrationBuilder.DropTable(
                name: "interview_questions");

            migrationBuilder.DropTable(
                name: "interview_sessions");

            migrationBuilder.DropTable(
                name: "question_templates");

            migrationBuilder.DropTable(
                name: "question_categories");
        }
    }
}
