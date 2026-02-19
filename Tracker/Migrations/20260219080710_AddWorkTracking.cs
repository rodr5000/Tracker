using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWorking",
                table: "TaskItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkStartTime",
                table: "TaskItems",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWorking",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "WorkStartTime",
                table: "TaskItems");
        }
    }
}
