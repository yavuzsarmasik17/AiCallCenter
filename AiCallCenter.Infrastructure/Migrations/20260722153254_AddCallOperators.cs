using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCallOperators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Calls",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 10000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "Calls",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderCallId",
                table: "Calls",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AiAgentId1",
                table: "Calls",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId1",
                table: "Calls",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CallOperators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ExtensionNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentCallId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxConcurrentCalls = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ActiveCallCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CanReceiveCalls = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStatusChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_CallOperators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallOperators_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallOperators_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallRecordings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Transcript = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_CallRecordings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallRecordings_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CallEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AiAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_CallEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallEvents_AiAgents_AiAgentId",
                        column: x => x.AiAgentId,
                        principalTable: "AiAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallEvents_CallOperators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "CallOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallEvents_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CallEvents_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_CallNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallNotes_CallOperators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "CallOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CallNotes_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CallNotes_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CallQueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedOperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    QueueName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransferReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WaitingSince = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_CallQueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallQueueItems_CallOperators_AssignedOperatorId",
                        column: x => x.AssignedOperatorId,
                        principalTable: "CallOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallQueueItems_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CallQueueItems_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calls_AiAgentId1",
                table: "Calls",
                column: "AiAgentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_CompanyId1",
                table: "Calls",
                column: "CompanyId1");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_Status",
                table: "Calls",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CallEvents_AiAgentId",
                table: "CallEvents",
                column: "AiAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_CallEvents_CallId",
                table: "CallEvents",
                column: "CallId");

            migrationBuilder.CreateIndex(
                name: "IX_CallEvents_CallId_OccurredAt",
                table: "CallEvents",
                columns: new[] { "CallId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CallEvents_CompanyId",
                table: "CallEvents",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CallEvents_EventType",
                table: "CallEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_CallEvents_OccurredAt",
                table: "CallEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_CallEvents_OperatorId",
                table: "CallEvents",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CallNotes_CallId",
                table: "CallNotes",
                column: "CallId");

            migrationBuilder.CreateIndex(
                name: "IX_CallNotes_CompanyId",
                table: "CallNotes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CallNotes_OperatorId",
                table: "CallNotes",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CallOperators_CompanyId",
                table: "CallOperators",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CallOperators_CompanyId_Status",
                table: "CallOperators",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CallOperators_Status",
                table: "CallOperators",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CallOperators_UserId",
                table: "CallOperators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_AssignedOperatorId",
                table: "CallQueueItems",
                column: "AssignedOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_CallId",
                table: "CallQueueItems",
                column: "CallId");

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_CompanyId",
                table: "CallQueueItems",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_CompanyId_Status_Priority_WaitingSince",
                table: "CallQueueItems",
                columns: new[] { "CompanyId", "Status", "Priority", "WaitingSince" });

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_Status",
                table: "CallQueueItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CallQueueItems_WaitingSince",
                table: "CallQueueItems",
                column: "WaitingSince");

            migrationBuilder.CreateIndex(
                name: "IX_CallRecordings_CallId",
                table: "CallRecordings",
                column: "CallId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_AiAgents_AiAgentId1",
                table: "Calls",
                column: "AiAgentId1",
                principalTable: "AiAgents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Calls_Companies_CompanyId1",
                table: "Calls",
                column: "CompanyId1",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calls_AiAgents_AiAgentId1",
                table: "Calls");

            migrationBuilder.DropForeignKey(
                name: "FK_Calls_Companies_CompanyId1",
                table: "Calls");

            migrationBuilder.DropTable(
                name: "CallEvents");

            migrationBuilder.DropTable(
                name: "CallNotes");

            migrationBuilder.DropTable(
                name: "CallQueueItems");

            migrationBuilder.DropTable(
                name: "CallRecordings");

            migrationBuilder.DropTable(
                name: "CallOperators");

            migrationBuilder.DropIndex(
                name: "IX_Calls_AiAgentId1",
                table: "Calls");

            migrationBuilder.DropIndex(
                name: "IX_Calls_CompanyId1",
                table: "Calls");

            migrationBuilder.DropIndex(
                name: "IX_Calls_Status",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "AiAgentId1",
                table: "Calls");

            migrationBuilder.DropColumn(
                name: "CompanyId1",
                table: "Calls");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Calls",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "Calls",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderCallId",
                table: "Calls",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
