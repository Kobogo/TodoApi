using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AchievementModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "achievementPoints",
                schema: "TodoApi",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "currentMultiplier",
                schema: "TodoApi",
                table: "users",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "multiplierExpiry",
                schema: "TodoApi",
                table: "users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "totalTasksCompleted",
                schema: "TodoApi",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "achievements",
                schema: "TodoApi",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    requirementValue = table.Column<int>(type: "integer", nullable: false),
                    rewardAchievementPoints = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "userAchievements",
                schema: "TodoApi",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    userId = table.Column<int>(type: "integer", nullable: false),
                    achievementId = table.Column<int>(type: "integer", nullable: false),
                    unlockedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userAchievements", x => x.id);
                    table.ForeignKey(
                        name: "FK_userAchievements_achievements_achievementId",
                        column: x => x.achievementId,
                        principalSchema: "TodoApi",
                        principalTable: "achievements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_userAchievements_achievementId",
                schema: "TodoApi",
                table: "userAchievements",
                column: "achievementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userAchievements",
                schema: "TodoApi");

            migrationBuilder.DropTable(
                name: "achievements",
                schema: "TodoApi");

            migrationBuilder.DropColumn(
                name: "achievementPoints",
                schema: "TodoApi",
                table: "users");

            migrationBuilder.DropColumn(
                name: "currentMultiplier",
                schema: "TodoApi",
                table: "users");

            migrationBuilder.DropColumn(
                name: "multiplierExpiry",
                schema: "TodoApi",
                table: "users");

            migrationBuilder.DropColumn(
                name: "totalTasksCompleted",
                schema: "TodoApi",
                table: "users");
        }
    }
}
