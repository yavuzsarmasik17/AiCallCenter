using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiProviderToAgents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "AiAgents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Provider",
                table: "AiAgents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "AiAgents");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "AiAgents");
        }
    }
}
