using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class server : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Invites",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "BlacklistedChannels",
                columns: table => new
                {
                    ChannelId = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WelcomeChannelId = table.Column<ulong>(nullable: false),
                    JoinLogChannelId = table.Column<ulong>(nullable: false),
                    TeamLogChannelId = table.Column<ulong>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    JoinDate = table.Column<DateTime>(nullable: false),
                    LeaveDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.GuildId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistedChannels");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Invites");
        }
    }
}
