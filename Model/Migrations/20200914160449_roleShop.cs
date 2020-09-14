using Microsoft.EntityFrameworkCore.Migrations;

namespace Model.Migrations
{
    public partial class roleShop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopRoles",
                columns: table => new
                {
                    RoleId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Price = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopRoles", x => x.RoleId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopRoles");
        }
    }
}
