using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_LearningRoadmap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "learning_roadmaps",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    target_job_id = table.Column<long>(type: "bigint", nullable: true),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: true),
                    roadmap_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    roadmap_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
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
                name: "roadmap_milestones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    learning_roadmap_id = table.Column<long>(type: "bigint", nullable: false),
                    milestone_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    milestone_order = table.Column<int>(type: "int", nullable: false),
                    is_completed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    learning_roadmap_id = table.Column<long>(type: "bigint", nullable: false),
                    completion_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
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
                name: "roadmap_recommendations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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

            migrationBuilder.CreateTable(
                name: "learning_activities",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roadmap_milestone_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: true),
                    activity_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    activity_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    activity_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_completed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "learning_activities");

            migrationBuilder.DropTable(
                name: "roadmap_progress");

            migrationBuilder.DropTable(
                name: "roadmap_recommendations");

            migrationBuilder.DropTable(
                name: "roadmap_milestones");

            migrationBuilder.DropTable(
                name: "learning_roadmaps");
        }
    }
}
