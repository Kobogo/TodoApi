using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Points",
                schema: "TodoApi",
                table: "staticTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Points",
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
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 2,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 3,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 4,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 5,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 6,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 7,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 8,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 9,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 10,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 11,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 12,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 13,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 14,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 15,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 16,
                column: "Points",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 17,
                column: "Points",
                value: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                schema: "TodoApi",
                table: "staticTasks");

            migrationBuilder.DropColumn(
                name: "Points",
                schema: "TodoApi",
                table: "dynamicTasks");
        }
    }
}
