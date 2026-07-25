using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_TargetJob_JobDescription_Skill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    skill_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    skill_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "target_jobs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    job_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    industry = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    experience_level = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                name: "job_descriptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    target_job_id = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "MANUAL"),
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
                name: "required_skills",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_description_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    importance_level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
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

            migrationBuilder.InsertData(
                table: "skills",
                columns: new[] { "id", "created_at", "skill_name", "skill_type" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Communication", "Soft Skill" },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Problem Solving", "Soft Skill" },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Teamwork", "Soft Skill" },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SQL", "Technical Skill" },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Java", "Technical Skill" },
                    { 6L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Python", "Technical Skill" },
                    { 7L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System Design", "Technical Skill" },
                    { 8L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Data Analysis", "Technical Skill" }
                });

            migrationBuilder.CreateIndex(
                name: "idx_job_descriptions_target_job_id",
                table: "job_descriptions",
                column: "target_job_id");

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
                name: "IX_skills_skill_name",
                table: "skills",
                column: "skill_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_target_jobs_user_id",
                table: "target_jobs",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "required_skills");

            migrationBuilder.DropTable(
                name: "job_descriptions");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "target_jobs");
        }
    }
}
