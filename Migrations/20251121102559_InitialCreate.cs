using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DynamicTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    TimeOfDay = table.Column<TimeSpan>(type: "interval", nullable: true),
                    LastCompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastShownDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RepeatDays = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaticTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    TimeOfDay = table.Column<TimeSpan>(type: "interval", nullable: true),
                    LastCompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastShownDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RepeatDays = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticTasks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "StaticTasks",
                columns: new[] { "Id", "IsCompleted", "LastCompletedDate", "LastShownDate", "RepeatDays", "TimeOfDay", "Title" },
                values: new object[,]
                {
                    { 1, false, null, null, "[]", null, "Tømme opvaskemaskine & flyde den igen" },
                    { 2, false, null, null, "[]", null, "Tørre støv af" },
                    { 3, false, null, null, "[]", null, "Dække bord + tørre bord af" },
                    { 4, false, null, null, "[]", null, "Støvsuge hele huset" },
                    { 5, false, null, null, "[]", null, "Vaske gulv" },
                    { 6, false, null, null, "[]", null, "Hænge vasketøj op med en voksen" },
                    { 7, false, null, null, "[]", null, "Skylle af efter aftensmaden" },
                    { 8, false, null, null, "[]", null, "Pille tøj ned af tørrestativet" },
                    { 9, false, null, null, "[]", null, "Lægge tøj sammen + lægge tøj på plads med en voksen" },
                    { 10, false, null, null, "[]", null, "Tømme skraldespande" },
                    { 11, false, null, null, "[]", null, "Ordne badeværelser med en voksen" },
                    { 12, false, null, null, "[]", null, "Slå græs" },
                    { 13, false, null, null, "[]", null, "Fejrne ukrudt (min. 1 spand)" },
                    { 14, false, null, null, "[]", null, "Være med til at lave aftensmad" },
                    { 15, false, null, null, "[]", null, "Fylde op i køleskab med sodavand" },
                    { 16, false, null, null, "[]", null, "Give kattene mad" },
                    { 17, false, null, null, "[]", null, "Rede senge (Alle)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicTasks");

            migrationBuilder.DropTable(
                name: "StaticTasks");
        }
    }
}
