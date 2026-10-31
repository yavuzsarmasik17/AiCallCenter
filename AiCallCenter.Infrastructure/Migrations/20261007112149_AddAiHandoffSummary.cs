using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiHandoffSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiHandoffSummary",
                table: "CallQueueItems",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiHandoffSummary",
                table: "CallQueueItems");
        }
    }
}
