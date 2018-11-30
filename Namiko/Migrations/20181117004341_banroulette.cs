using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class banroulette : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BannedUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServerId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    DateBanStart = table.Column<DateTime>(nullable: false),
                    DateBanEnd = table.Column<DateTime>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banroulettes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<ulong>(nullable: false),
                    ServerId = table.Column<ulong>(nullable: false),
                    RoleReqId = table.Column<ulong>(nullable: false),
                    BanLengthHours = table.Column<int>(nullable: false),
                    MinParticipants = table.Column<int>(nullable: false),
                    MaxParticipants = table.Column<int>(nullable: false),
                    RewardPool = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banroulettes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BanrouletteParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(nullable: false),
                    BanrouletteId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanrouletteParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BanrouletteParticipants_Banroulettes_BanrouletteId",
                        column: x => x.BanrouletteId,
                        principalTable: "Banroulettes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BanrouletteParticipants_BanrouletteId",
                table: "BanrouletteParticipants",
                column: "BanrouletteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannedUsers");

            migrationBuilder.DropTable(
                name: "BanrouletteParticipants");

            migrationBuilder.DropTable(
                name: "Banroulettes");
        }
    }
}
