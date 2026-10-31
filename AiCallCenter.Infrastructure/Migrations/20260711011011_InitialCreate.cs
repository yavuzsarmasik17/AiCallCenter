using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubscriptionPlan = table.Column<int>(type: "int", nullable: false),
                    MaxUserCount = table.Column<int>(type: "int", nullable: false),
                    MaxAiAgentCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiAgents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false),
                    WelcomeMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    VoiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxCallDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    TransferToHumanEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AiAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiAgents_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Calls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AiAgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCallId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CallerNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CalledNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TransferredToHuman = table.Column<bool>(type: "bit", nullable: false),
                    TransferNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("PK_Calls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calls_AiAgents_AiAgentId",
                        column: x => x.AiAgentId,
                        principalTable: "AiAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Calls_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Speaker = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpokenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: true),
                    AudioFileUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_CallMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallMessages_Calls_CallId",
                        column: x => x.CallId,
                        principalTable: "Calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAgents_CompanyId",
                table: "AiAgents",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AiAgents_CompanyId_Name",
                table: "AiAgents",
                columns: new[] { "CompanyId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CallMessages_CallId",
                table: "CallMessages",
                column: "CallId");

            migrationBuilder.CreateIndex(
                name: "IX_CallMessages_CallId_SequenceNumber",
                table: "CallMessages",
                columns: new[] { "CallId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calls_AiAgentId",
                table: "Calls",
                column: "AiAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_CallerNumber",
                table: "Calls",
                column: "CallerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_CompanyId",
                table: "Calls",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_ProviderCallId",
                table: "Calls",
                column: "ProviderCallId");

            migrationBuilder.CreateIndex(
                name: "IX_Calls_StartedAt",
                table: "Calls",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Slug",
                table: "Companies",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallMessages");

            migrationBuilder.DropTable(
                name: "Calls");

            migrationBuilder.DropTable(
                name: "AiAgents");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
