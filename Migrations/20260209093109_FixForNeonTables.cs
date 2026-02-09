using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class FixForNeonTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    familyName = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    passwordHash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    familyId = table.Column<int>(type: "integer", nullable: true),
                    totalPoints = table.Column<int>(type: "integer", nullable: false),
                    savingsBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    familyName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_families_familyId",
                        column: x => x.familyId,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "dynamicTasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    userId = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    isCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    timeOfDay = table.Column<TimeSpan>(type: "interval", nullable: true),
                    lastCompletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastShownDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    repeatDays = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dynamicTasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_dynamicTasks_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pushSubscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    userId = table.Column<int>(type: "integer", nullable: false),
                    endpoint = table.Column<string>(type: "text", nullable: false),
                    p256dh = table.Column<string>(type: "text", nullable: false),
                    auth = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pushSubscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_pushSubscriptions_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staticTasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    userId = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    isCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    timeOfDay = table.Column<TimeSpan>(type: "interval", nullable: true),
                    lastCompletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastShownDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    repeatDays = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staticTasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_staticTasks_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "families",
                columns: new[] { "id", "created_at", "familyName" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bang" });

            migrationBuilder.InsertData(
                table: "staticTasks",
                columns: new[] { "id", "isCompleted", "lastCompletedDate", "lastShownDate", "repeatDays", "timeOfDay", "title", "userId" },
                values: new object[,]
                {
                    { 1, false, null, null, null, null, "Tømme opvaskemaskine & flyde den igen", null },
                    { 2, false, null, null, null, null, "Tørre støv af", null },
                    { 3, false, null, null, null, null, "Dække bord + tørre bord af", null },
                    { 4, false, null, null, null, null, "Støvsuge hele huset", null },
                    { 5, false, null, null, null, null, "Vaske gulv", null },
                    { 6, false, null, null, null, null, "Hænge vasketøj op med en voksen", null },
                    { 7, false, null, null, null, null, "Skylle af efter aftensmaden", null },
                    { 8, false, null, null, null, null, "Pille tøj ned af tørrestativet", null },
                    { 9, false, null, null, null, null, "Lægge tøj sammen + lægge tøj på plads med en voksen", null },
                    { 10, false, null, null, null, null, "Tømme skraldespande", null },
                    { 11, false, null, null, null, null, "Ordne badeværelser med en voksen", null },
                    { 12, false, null, null, null, null, "Slå græs", null },
                    { 13, false, null, null, null, null, "Fejrne ukrudt (min. 1 spand)", null },
                    { 14, false, null, null, null, null, "Være med til at lave aftensmad", null },
                    { 15, false, null, null, null, null, "Fylde op i køleskab med sodavand", null },
                    { 16, false, null, null, null, null, "Give kattene mad", null },
                    { 17, false, null, null, null, null, "Rede senge (Alle)", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_dynamicTasks_userId",
                table: "dynamicTasks",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_pushSubscriptions_userId",
                table: "pushSubscriptions",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_staticTasks_userId",
                table: "staticTasks",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_users_familyId",
                table: "users",
                column: "familyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dynamicTasks");

            migrationBuilder.DropTable(
                name: "pushSubscriptions");

            migrationBuilder.DropTable(
                name: "staticTasks");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "families");
        }
    }
}
