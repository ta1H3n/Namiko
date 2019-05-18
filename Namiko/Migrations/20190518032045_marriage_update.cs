using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class marriage_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMarried",
                table: "Marriages",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMarried",
                table: "Marriages");
        }
    }
}
