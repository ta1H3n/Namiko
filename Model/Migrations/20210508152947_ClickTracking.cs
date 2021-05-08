using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Model.Migrations
{
    public partial class ClickTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopWaifus_Waifus_WaifuName",
                table: "ShopWaifus");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopWaifus_WaifuShops_WaifuShopId",
                table: "ShopWaifus");

            migrationBuilder.AlterColumn<int>(
                name: "WaifuShopId",
                table: "ShopWaifus",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Clicks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(nullable: true),
                    Tag = table.Column<string>(nullable: true),
                    RedirectUrl = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    DiscordId = table.Column<decimal>(nullable: false),
                    Ip = table.Column<string>(nullable: true),
                    Referer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clicks", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ShopWaifus_Waifus_WaifuName",
                table: "ShopWaifus",
                column: "WaifuName",
                principalTable: "Waifus",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopWaifus_WaifuShops_WaifuShopId",
                table: "ShopWaifus",
                column: "WaifuShopId",
                principalTable: "WaifuShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopWaifus_Waifus_WaifuName",
                table: "ShopWaifus");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopWaifus_WaifuShops_WaifuShopId",
                table: "ShopWaifus");

            migrationBuilder.DropTable(
                name: "Clicks");

            migrationBuilder.AlterColumn<int>(
                name: "WaifuShopId",
                table: "ShopWaifus",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_ShopWaifus_Waifus_WaifuName",
                table: "ShopWaifus",
                column: "WaifuName",
                principalTable: "Waifus",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopWaifus_WaifuShops_WaifuShopId",
                table: "ShopWaifus",
                column: "WaifuShopId",
                principalTable: "WaifuShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
