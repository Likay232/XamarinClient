using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TestUserNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "test_users");

            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "test_users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MistakesCount",
                table: "test_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "tests",
                columns: new[] { "Id", "ModifiedAt", "Title" },
                values: new object[] { 1, new DateTime(2026, 2, 15, 4, 7, 58, 823, DateTimeKind.Utc).AddTicks(925), "Generated Test" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tests",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "test_users");

            migrationBuilder.DropColumn(
                name: "MistakesCount",
                table: "test_users");

            migrationBuilder.AddColumn<string>(
                name: "Score",
                table: "test_users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
