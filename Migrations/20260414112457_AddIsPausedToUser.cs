using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPausedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isPaused",
                schema: "TodoApi",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isPaused",
                schema: "TodoApi",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "timeBonusMinutes",
                schema: "TodoApi",
                table: "staticTasks",
                newName: "TimeBonusMinutes");

            migrationBuilder.RenameColumn(
                name: "points",
                schema: "TodoApi",
                table: "staticTasks",
                newName: "Points");

            migrationBuilder.RenameColumn(
                name: "timeBonusMinutes",
                schema: "TodoApi",
                table: "dynamicTasks",
                newName: "TimeBonusMinutes");

            migrationBuilder.RenameColumn(
                name: "points",
                schema: "TodoApi",
                table: "dynamicTasks",
                newName: "Points");

            migrationBuilder.AlterColumn<int>(
                name: "TimeBonusMinutes",
                schema: "TodoApi",
                table: "staticTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TimeBonusMinutes",
                schema: "TodoApi",
                table: "dynamicTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
