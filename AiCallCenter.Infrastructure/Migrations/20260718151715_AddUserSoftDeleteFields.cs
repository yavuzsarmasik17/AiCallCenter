using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCallCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSoftDeleteFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Users");
        }
    }
}
