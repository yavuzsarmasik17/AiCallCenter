using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCallOperatorAiAgents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CallOperatorAiAgents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallOperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AiAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_CallOperatorAiAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallOperatorAiAgents_AiAgents_AiAgentId",
                        column: x => x.AiAgentId,
                        principalTable: "AiAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallOperatorAiAgents_CallOperators_CallOperatorId",
                        column: x => x.CallOperatorId,
                        principalTable: "CallOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CallOperatorAiAgents_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallOperatorAiAgents_AiAgentId",
                table: "CallOperatorAiAgents",
                column: "AiAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_CallOperatorAiAgents_CallOperatorId_AiAgentId",
                table: "CallOperatorAiAgents",
                columns: new[] { "CallOperatorId", "AiAgentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CallOperatorAiAgents_CallOperatorId_IsPrimary",
                table: "CallOperatorAiAgents",
                columns: new[] { "CallOperatorId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_CallOperatorAiAgents_CompanyId_CallOperatorId_IsActive",
                table: "CallOperatorAiAgents",
                columns: new[] { "CompanyId", "CallOperatorId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallOperatorAiAgents");
        }
    }
}
