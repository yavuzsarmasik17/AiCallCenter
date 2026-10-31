using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCallQueueManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CallQueueItems_CompanyId_Status_Priority_WaitingSince",
                table: "CallQueueItems");

            migrationBuilder.AlterColumn<string>(
                name: "QueueName",
                table: "CallQueueItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "CallQueueId",
                table: "CallQueueItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CallQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AiAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MaxWaitingSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 300),
                    MaxQueueSize = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_CallQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallQueues_AiAgents_AiAgentId",
                        column: x => x.AiAgentId,
                        principalTable: "AiAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallQueues_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallQueueOperators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallQueueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallOperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_CallQueueOperators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallQueueOperators_CallOperators_CallOperatorId",
                        column: x => x.CallOperatorId,
                        principalTable: "CallOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallQueueOperators_CallQueues_CallQueueId",
                        column: x => x.CallQueueId,
                        principalTable: "CallQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CallQueueOperators_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_CallQueueId",
                table: "CallQueueItems",
                column: "CallQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_CompanyId_CallQueueId_Status_Priority_WaitingSince",
                table: "CallQueueItems",
                columns: new[] { "CompanyId", "CallQueueId", "Status", "Priority", "WaitingSince" });

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueOperators_CallOperatorId",
                table: "CallQueueOperators",
                column: "CallOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueOperators_CallQueueId_CallOperatorId",
                table: "CallQueueOperators",
                columns: new[] { "CallQueueId", "CallOperatorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueOperators_CompanyId_CallQueueId_IsActive",
                table: "CallQueueOperators",
                columns: new[] { "CompanyId", "CallQueueId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CallQueues_AiAgentId",
                table: "CallQueues",
                column: "AiAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_CallQueues_CompanyId_IsActive",
                table: "CallQueues",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CallQueues_CompanyId_Name",
                table: "CallQueues",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CallQueueItems_CallQueues_CallQueueId",
                table: "CallQueueItems",
                column: "CallQueueId",
                principalTable: "CallQueues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CallQueueItems_CallQueues_CallQueueId",
                table: "CallQueueItems");

            migrationBuilder.DropTable(
                name: "CallQueueOperators");

            migrationBuilder.DropTable(
                name: "CallQueues");

            migrationBuilder.DropIndex(
                name: "IX_CallQueueItems_CallQueueId",
                table: "CallQueueItems");

            migrationBuilder.DropIndex(
                name: "IX_CallQueueItems_CompanyId_CallQueueId_Status_Priority_WaitingSince",
                table: "CallQueueItems");

            migrationBuilder.DropColumn(
                name: "CallQueueId",
                table: "CallQueueItems");

            migrationBuilder.AlterColumn<string>(
                name: "QueueName",
                table: "CallQueueItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_CompanyId_Status_Priority_WaitingSince",
                table: "CallQueueItems",
                columns: new[] { "CompanyId", "Status", "Priority", "WaitingSince" });
        }
    }
}
