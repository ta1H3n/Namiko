using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class reddit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RedditPosts",
                columns: table => new
                {
                    FullName = table.Column<string>(nullable: false),
                    PermaLink = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditPosts", x => x.FullName);
                });

            migrationBuilder.CreateTable(
                name: "SpecialChannels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<ulong>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialChannels", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedditPosts");

            migrationBuilder.DropTable(
                name: "SpecialChannels");
        }
    }
}
