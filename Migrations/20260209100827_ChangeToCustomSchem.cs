using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToCustomSchem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TodoApi");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "TodoApi");

            migrationBuilder.RenameTable(
                name: "staticTasks",
                newName: "staticTasks",
                newSchema: "TodoApi");

            migrationBuilder.RenameTable(
                name: "pushSubscriptions",
                newName: "pushSubscriptions",
                newSchema: "TodoApi");

            migrationBuilder.RenameTable(
                name: "families",
                newName: "families",
                newSchema: "TodoApi");

            migrationBuilder.RenameTable(
                name: "dynamicTasks",
                newName: "dynamicTasks",
                newSchema: "TodoApi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "users",
                schema: "TodoApi",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "staticTasks",
                schema: "TodoApi",
                newName: "staticTasks");

            migrationBuilder.RenameTable(
                name: "pushSubscriptions",
                schema: "TodoApi",
                newName: "pushSubscriptions");

            migrationBuilder.RenameTable(
                name: "families",
                schema: "TodoApi",
                newName: "families");

            migrationBuilder.RenameTable(
                name: "dynamicTasks",
                schema: "TodoApi",
                newName: "dynamicTasks");
        }
    }
}
