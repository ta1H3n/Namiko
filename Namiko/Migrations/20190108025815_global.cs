using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class global : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopItems");

            migrationBuilder.DropTable(
                name: "ShopRoles");

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "WaifuStores",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "UserInventories",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Toasties",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Toasties",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Teams",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "PublicRoles",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Invites",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "FeaturedWaifus",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Dailies",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Dailies",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "WaifuStores");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "UserInventories");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Toasties");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Toasties");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "PublicRoles");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Invites");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "FeaturedWaifus");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Dailies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Dailies");

            migrationBuilder.CreateTable(
                name: "ShopItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateToRemoveOn = table.Column<DateTime>(nullable: false),
                    RoleId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopRoles",
                columns: table => new
                {
                    RoleId = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DaysLength = table.Column<int>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false),
                    IfLimitedUserId = table.Column<ulong>(nullable: false),
                    Price = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopRoles", x => x.RoleId);
                });
        }
    }
}
