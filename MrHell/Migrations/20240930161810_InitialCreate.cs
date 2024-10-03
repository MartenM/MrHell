using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MrHell.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    PixelId = table.Column<string>(type: "TEXT", unicode: false, maxLength: 16, nullable: false),
                    Username = table.Column<string>(type: "TEXT", unicode: false, maxLength: 64, nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Default"),
                    FirstSeen = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "now()"),
                    LastSeen = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "now()"),
                    Banned = table.Column<bool>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Coins = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.PixelId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Username",
                table: "Profiles",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
