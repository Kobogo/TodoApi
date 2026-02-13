using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTimerFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isTimerRunning",
                schema: "TodoApi",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastTimerUpdate",
                schema: "TodoApi",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "minutesLeftToday",
                schema: "TodoApi",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "saturdayBonusPot",
                schema: "TodoApi",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isTimerRunning",
                schema: "TodoApi",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lastTimerUpdate",
                schema: "TodoApi",
                table: "users");

            migrationBuilder.DropColumn(
                name: "minutesLeftToday",
                schema: "TodoApi",
                table: "users");

            migrationBuilder.DropColumn(
                name: "saturdayBonusPot",
                schema: "TodoApi",
                table: "users");
        }
    }
}
