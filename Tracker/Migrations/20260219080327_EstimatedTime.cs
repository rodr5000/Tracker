using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Migrations
{
    /// <inheritdoc />
    public partial class EstimatedTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EstimatedTimeTicks",
                table: "TaskItems",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedTimeTicks",
                table: "TaskItems");
        }
    }
}
