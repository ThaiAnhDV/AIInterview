using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_SkillGapAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "skill_gap_analyses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    resume_id = table.Column<long>(type: "bigint", nullable: false),
                    job_description_id = table.Column<long>(type: "bigint", nullable: false),
                    analysis_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "COMPLETED"),
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
                name: "readiness_scores",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    score_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "OVERALL"),
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    gap_level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gap_description = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    skill_gap_analysis_id = table.Column<long>(type: "bigint", nullable: false),
                    report_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "IX_strength_weakness_reports_skill_gap_analysis_id",
                table: "strength_weakness_reports",
                column: "skill_gap_analysis_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "readiness_scores");

            migrationBuilder.DropTable(
                name: "skill_gaps");

            migrationBuilder.DropTable(
                name: "strength_weakness_reports");

            migrationBuilder.DropTable(
                name: "skill_gap_analyses");
        }
    }
}
