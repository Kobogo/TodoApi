using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeBonusToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeBonusMinutes",
                schema: "TodoApi",
                table: "staticTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeBonusMinutes",
                schema: "TodoApi",
                table: "dynamicTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 1,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 2,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 3,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 4,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 5,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 6,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 7,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 8,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 9,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 10,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 11,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 12,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 13,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 14,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 15,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 16,
                column: "TimeBonusMinutes",
                value: 15);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 17,
                column: "TimeBonusMinutes",
                value: 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeBonusMinutes",
                schema: "TodoApi",
                table: "staticTasks");

            migrationBuilder.DropColumn(
                name: "TimeBonusMinutes",
                schema: "TodoApi",
                table: "dynamicTasks");
        }
    }
}
