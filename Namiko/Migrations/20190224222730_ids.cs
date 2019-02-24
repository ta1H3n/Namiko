using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class ids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WelcomeChannels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Toasties",
                table: "Toasties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dailies",
                table: "Dailies");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Toasties",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Toasties",
                nullable: false,
                oldClrType: typeof(ulong))
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Dailies",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Dailies",
                nullable: false,
                oldClrType: typeof(ulong))
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Toasties",
                table: "Toasties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dailies",
                table: "Dailies",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Toasties",
                table: "Toasties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dailies",
                table: "Dailies");

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Toasties",
                nullable: false,
                oldClrType: typeof(ulong))
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Toasties",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Dailies",
                nullable: false,
                oldClrType: typeof(ulong))
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Dailies",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Toasties",
                table: "Toasties",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dailies",
                table: "Dailies",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "WelcomeChannels",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelcomeChannels", x => x.GuildId);
                });
        }
    }
}
