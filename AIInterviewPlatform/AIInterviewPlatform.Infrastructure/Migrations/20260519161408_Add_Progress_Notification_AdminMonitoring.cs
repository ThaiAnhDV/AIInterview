using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Progress_Notification_AdminMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    notification_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                name: "practice_histories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    interview_session_id = table.Column<long>(type: "bigint", nullable: true),
                    learning_activity_id = table.Column<long>(type: "bigint", nullable: true),
                    activity_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                name: "progress_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    overall_progress = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
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
                name: "skill_improvement_trends",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    improvement_score = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                name: "usage_statistics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    total_sessions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_questions_answered = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    average_score = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
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
                name: "idx_skill_trends_user_skill_time",
                table: "skill_improvement_trends",
                columns: new[] { "user_id", "skill_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_skill_improvement_trends_skill_id",
                table: "skill_improvement_trends",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "idx_system_logs_created_at",
                table: "system_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_system_logs_user_id",
                table: "system_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_usage_statistics_user",
                table: "usage_statistics",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "practice_histories");

            migrationBuilder.DropTable(
                name: "progress_records");

            migrationBuilder.DropTable(
                name: "skill_improvement_trends");

            migrationBuilder.DropTable(
                name: "system_logs");

            migrationBuilder.DropTable(
                name: "usage_statistics");
        }
    }
}
