using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class Tasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeLogId",
                table: "TimeLogs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TaskItemId",
                table: "TaskItems",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TimeLogs",
                newName: "TimeLogId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TaskItems",
                newName: "TaskItemId");
        }
    }
}
