using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Model.Migrations
{
    public partial class PremiumCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExpireSent",
                table: "Premiums",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Premiums",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "PremiumCodes",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Type = table.Column<decimal>(nullable: false),
                    DurationDays = table.Column<int>(nullable: false),
                    UsesLeft = table.Column<int>(nullable: false),
                    ExpiresAt = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PremiumCodeUses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    PremiumCodeId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumCodeUses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PremiumCodeUses_PremiumCodes_PremiumCodeId",
                        column: x => x.PremiumCodeId,
                        principalTable: "PremiumCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PremiumCodeUses_PremiumCodeId",
                table: "PremiumCodeUses",
                column: "PremiumCodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PremiumCodeUses");

            migrationBuilder.DropTable(
                name: "PremiumCodes");

            migrationBuilder.DropColumn(
                name: "ExpireSent",
                table: "Premiums");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Premiums");
        }
    }
}
