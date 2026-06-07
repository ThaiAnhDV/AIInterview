using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageCodeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "target_jobs");

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "user_profiles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_language_code",
                table: "user_profiles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "target_jobs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "skill_gap_analyses",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "learning_roadmaps",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "learning_activities",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "interview_sessions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "skill_focus",
                table: "interview_questions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "interview_questions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "feedbacks",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "authentication_accounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language_code",
                table: "answer_evaluations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "language_code",
                table: "users");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "preferred_language_code",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "target_jobs");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "skill_gap_analyses");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "learning_roadmaps");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "learning_activities");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "interview_sessions");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "interview_questions");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "feedbacks");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "authentication_accounts");

            migrationBuilder.DropColumn(
                name: "language_code",
                table: "answer_evaluations");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "target_jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "skill_focus",
                table: "interview_questions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
