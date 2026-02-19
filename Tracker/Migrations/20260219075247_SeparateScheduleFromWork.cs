using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Migrations
{
    /// <inheritdoc />
    public partial class SeparateScheduleFromWork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "TaskItems",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "TaskItems",
                newName: "DueDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "TaskItems",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "TaskItems",
                newName: "EndTime");
        }
    }
}
