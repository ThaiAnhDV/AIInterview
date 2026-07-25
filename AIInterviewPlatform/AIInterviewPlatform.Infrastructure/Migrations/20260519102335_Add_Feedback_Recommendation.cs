using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Feedback_Recommendation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feedbacks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    answer_evaluation_id = table.Column<long>(type: "bigint", nullable: false),
                    feedback_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    feedback_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                name: "improvement_suggestions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    feedback_id = table.Column<long>(type: "bigint", nullable: false),
                    suggestion_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    priority_level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: true),
                    feedback_id = table.Column<long>(type: "bigint", nullable: true),
                    recommendation_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    recommendation_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    feedback_id = table.Column<long>(type: "bigint", nullable: false),
                    pattern_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    pattern_description = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "idx_feedbacks_evaluation_id",
                table: "feedbacks",
                column: "answer_evaluation_id");

            migrationBuilder.CreateIndex(
                name: "idx_suggestions_feedback_id",
                table: "improvement_suggestions",
                column: "feedback_id");

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
                name: "idx_patterns_feedback_id",
                table: "weak_communication_patterns",
                column: "feedback_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "improvement_suggestions");

            migrationBuilder.DropTable(
                name: "recommendations");

            migrationBuilder.DropTable(
                name: "weak_communication_patterns");

            migrationBuilder.DropTable(
                name: "feedbacks");
        }
    }
}
