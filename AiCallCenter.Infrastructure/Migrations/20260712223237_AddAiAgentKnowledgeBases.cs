using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAgentKnowledgeBases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiAgentKnowledgeBases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AiAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KnowledgeBaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAgentKnowledgeBases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiAgentKnowledgeBases_AiAgents_AiAgentId",
                        column: x => x.AiAgentId,
                        principalTable: "AiAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiAgentKnowledgeBases_KnowledgeBases_KnowledgeBaseId",
                        column: x => x.KnowledgeBaseId,
                        principalTable: "KnowledgeBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAgentKnowledgeBases_AiAgentId_KnowledgeBaseId",
                table: "AiAgentKnowledgeBases",
                columns: new[] { "AiAgentId", "KnowledgeBaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiAgentKnowledgeBases_KnowledgeBaseId",
                table: "AiAgentKnowledgeBases",
                column: "KnowledgeBaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAgentKnowledgeBases");
        }
    }
}
