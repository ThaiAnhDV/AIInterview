using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterviewPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToTargetJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "target_jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "target_jobs");
        }
    }
}
