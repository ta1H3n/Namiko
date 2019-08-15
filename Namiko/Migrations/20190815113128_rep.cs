using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class rep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rep",
                table: "Profiles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepDate",
                table: "Profiles",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rep",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "RepDate",
                table: "Profiles");
        }
    }
}
