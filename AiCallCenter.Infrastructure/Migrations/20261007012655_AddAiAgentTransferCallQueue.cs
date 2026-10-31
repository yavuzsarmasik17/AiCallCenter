using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAgentTransferCallQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransferCallQueueId",
                table: "AiAgents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiAgents_CompanyId_TransferCallQueueId",
                table: "AiAgents",
                columns: new[] { "CompanyId", "TransferCallQueueId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiAgents_CompanyId_TransferCallQueueId",
                table: "AiAgents");

            migrationBuilder.DropColumn(
                name: "TransferCallQueueId",
                table: "AiAgents");
        }
    }
}
