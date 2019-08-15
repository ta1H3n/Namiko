using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Namiko.Migrations
{
    public partial class gachashop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaifuStores_Waifus_WaifuName",
                table: "WaifuStores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WaifuStores",
                table: "WaifuStores");

            migrationBuilder.DropColumn(
                name: "AddedByUserId",
                table: "Waifus");

            migrationBuilder.DropColumn(
                name: "TimesBought",
                table: "Waifus");

            migrationBuilder.DropColumn(
                name: "GeneratedDate",
                table: "WaifuStores");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "WaifuStores");

            migrationBuilder.RenameTable(
                name: "WaifuStores",
                newName: "ShopWaifus");

            migrationBuilder.RenameIndex(
                name: "IX_WaifuStores_WaifuName",
                table: "ShopWaifus",
                newName: "IX_ShopWaifus_WaifuName");

            migrationBuilder.AddColumn<int>(
                name: "WaifuShopId",
                table: "ShopWaifus",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShopWaifus",
                table: "ShopWaifus",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "WaifuShops",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuildId = table.Column<ulong>(nullable: false),
                    GeneratedDate = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaifuShops", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopWaifus_WaifuShopId",
                table: "ShopWaifus",
                column: "WaifuShopId");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopWaifus_Waifus_WaifuName",
                table: "ShopWaifus");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopWaifus_WaifuShops_WaifuShopId",
                table: "ShopWaifus");

            migrationBuilder.DropTable(
                name: "WaifuShops");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShopWaifus",
                table: "ShopWaifus");

            migrationBuilder.DropIndex(
                name: "IX_ShopWaifus_WaifuShopId",
                table: "ShopWaifus");

            migrationBuilder.DropColumn(
                name: "WaifuShopId",
                table: "ShopWaifus");

            migrationBuilder.RenameTable(
                name: "ShopWaifus",
                newName: "WaifuStores");

            migrationBuilder.RenameIndex(
                name: "IX_ShopWaifus_WaifuName",
                table: "WaifuStores",
                newName: "IX_WaifuStores_WaifuName");

            migrationBuilder.AddColumn<ulong>(
                name: "AddedByUserId",
                table: "Waifus",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<int>(
                name: "TimesBought",
                table: "Waifus",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedDate",
                table: "WaifuStores",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "WaifuStores",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WaifuStores",
                table: "WaifuStores",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WaifuStores_Waifus_WaifuName",
                table: "WaifuStores",
                column: "WaifuName",
                principalTable: "Waifus",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
