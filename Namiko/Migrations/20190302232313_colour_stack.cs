using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class colour_stack : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PriorColorHexStack",
                table: "Profiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriorColorHexStack",
                table: "Profiles");
        }
    }
}
