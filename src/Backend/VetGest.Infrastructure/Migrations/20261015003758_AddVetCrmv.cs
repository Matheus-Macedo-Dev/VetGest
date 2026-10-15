using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetGest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVetCrmv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CrmvCheckedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrmvCheckedByUserId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrmvNumber",
                table: "Users",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrmvState",
                table: "Users",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CrmvState_CrmvNumber",
                table: "Users",
                columns: new[] { "CrmvState", "CrmvNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CrmvState_CrmvNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CrmvCheckedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CrmvCheckedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CrmvNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CrmvState",
                table: "Users");
        }
    }
}
