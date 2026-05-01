using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class isRepeatable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isRepeatable",
                schema: "TodoApi",
                table: "staticTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 1,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 2,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 3,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 4,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 5,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 6,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 7,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 8,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 9,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 10,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 11,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 12,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 13,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 14,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 15,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 16,
                column: "isRepeatable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "TodoApi",
                table: "staticTasks",
                keyColumn: "id",
                keyValue: 17,
                column: "isRepeatable",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isRepeatable",
                schema: "TodoApi",
                table: "staticTasks");
        }
    }
}
