using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class MalWaifus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MalWaifus",
                columns: table => new
                {
                    WaifuName = table.Column<string>(nullable: false),
                    MalId = table.Column<long>(nullable: false),
                    MalConfirmed = table.Column<bool>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalWaifus", x => x.WaifuName);
                    table.ForeignKey(
                        name: "FK_MalWaifus_Waifus_WaifuName",
                        column: x => x.WaifuName,
                        principalTable: "Waifus",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MalWaifus");
        }
    }
}
