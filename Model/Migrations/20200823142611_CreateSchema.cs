using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Model.Migrations
{
    public partial class CreateSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BannedUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServerId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    DateBanStart = table.Column<DateTime>(nullable: false),
                    DateBanEnd = table.Column<DateTime>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banroulettes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<decimal>(nullable: false),
                    ServerId = table.Column<decimal>(nullable: false),
                    RoleReqId = table.Column<decimal>(nullable: false),
                    BanLengthHours = table.Column<int>(nullable: false),
                    MinParticipants = table.Column<int>(nullable: false),
                    MaxParticipants = table.Column<int>(nullable: false),
                    RewardPool = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banroulettes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banroyales",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    RoleReqId = table.Column<decimal>(nullable: false),
                    ParticipantRoleId = table.Column<decimal>(nullable: false),
                    BanLengthHours = table.Column<int>(nullable: false),
                    MinParticipants = table.Column<int>(nullable: false),
                    MaxParticipants = table.Column<int>(nullable: false),
                    RewardPool = table.Column<int>(nullable: false),
                    WinnerAmount = table.Column<int>(nullable: false),
                    MinFrequency = table.Column<int>(nullable: false),
                    MaxFrequency = table.Column<int>(nullable: false),
                    Kick = table.Column<bool>(nullable: false),
                    Running = table.Column<bool>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banroyales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blacklist",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklist", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlacklistedChannels",
                columns: table => new
                {
                    ChannelId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "Dailies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Date = table.Column<long>(nullable: false),
                    Streak = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dailies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImgurAlbums",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    AlbumId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImgurAlbums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invites",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeamId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LootBoxes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootBoxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Marriages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    WifeId = table.Column<decimal>(nullable: false),
                    IsMarried = table.Column<bool>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marriages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Params",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    Args = table.Column<string>(nullable: true),
                    Num = table.Column<decimal>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Params", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Premiums",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Type = table.Column<decimal>(nullable: false),
                    ClaimDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Premiums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    UserId = table.Column<decimal>(nullable: false),
                    ColorHex = table.Column<string>(nullable: true),
                    PriorColorHexStack = table.Column<string>(nullable: true),
                    Quote = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    LootboxesOpened = table.Column<int>(nullable: false),
                    Rep = table.Column<int>(nullable: false),
                    RepDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "PublicRoles",
                columns: table => new
                {
                    RoleId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicRoles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "RedditPosts",
                columns: table => new
                {
                    PermaLink = table.Column<string>(nullable: false),
                    Upvotes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditPosts", x => x.PermaLink);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(nullable: false),
                    WelcomeChannelId = table.Column<decimal>(nullable: false),
                    JoinLogChannelId = table.Column<decimal>(nullable: false),
                    TeamLogChannelId = table.Column<decimal>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    JoinDate = table.Column<DateTime>(nullable: false),
                    LeaveDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "SpecialChannels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<decimal>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Args = table.Column<string>(nullable: true),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    LeaderRoleId = table.Column<decimal>(nullable: false),
                    MemberRoleId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.LeaderRoleId);
                });

            migrationBuilder.CreateTable(
                name: "Toasties",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Amount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Toasties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Voters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Waifus",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    LongName = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    Tier = table.Column<int>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    Bought = table.Column<int>(nullable: false),
                    ImageSource = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waifus", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "WaifuShops",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    GeneratedDate = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaifuShops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Weeklies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weeklies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WelcomeMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WelcomeMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BanrouletteParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    BanrouletteId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanrouletteParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BanrouletteParticipants_Banroulettes_BanrouletteId",
                        column: x => x.BanrouletteId,
                        principalTable: "Banroulettes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BanroyaleMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<decimal>(nullable: false),
                    EmoteId = table.Column<decimal>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    BanroyaleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanroyaleMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BanroyaleMessages_Banroyales_BanroyaleId",
                        column: x => x.BanroyaleId,
                        principalTable: "Banroyales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Aliases = table.Column<string>(nullable: true),
                    Example = table.Column<string>(nullable: true),
                    Conditions = table.Column<string>(nullable: true),
                    ModuleName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commands_Modules_ModuleName",
                        column: x => x.ModuleName,
                        principalTable: "Modules",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaylistId = table.Column<int>(nullable: false),
                    SongHash = table.Column<string>(nullable: true),
                    UserId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tracks_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeaturedWaifus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    WaifuName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedWaifus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeaturedWaifus_Waifus_WaifuName",
                        column: x => x.WaifuName,
                        principalTable: "Waifus",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "UserInventories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    WaifuName = table.Column<string>(nullable: true),
                    DateBought = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInventories_Waifus_WaifuName",
                        column: x => x.WaifuName,
                        principalTable: "Waifus",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WaifuWishlist",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    WaifuName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaifuWishlist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaifuWishlist_Waifus_WaifuName",
                        column: x => x.WaifuName,
                        principalTable: "Waifus",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopWaifus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WaifuShopId = table.Column<int>(nullable: true),
                    WaifuName = table.Column<string>(nullable: true),
                    Discount = table.Column<int>(nullable: false),
                    Limited = table.Column<int>(nullable: false),
                    BoughtBy = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopWaifus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopWaifus_Waifus_WaifuName",
                        column: x => x.WaifuName,
                        principalTable: "Waifus",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopWaifus_WaifuShops_WaifuShopId",
                        column: x => x.WaifuShopId,
                        principalTable: "WaifuShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BanrouletteParticipants_BanrouletteId",
                table: "BanrouletteParticipants",
                column: "BanrouletteId");

            migrationBuilder.CreateIndex(
                name: "IX_BanroyaleMessages_BanroyaleId",
                table: "BanroyaleMessages",
                column: "BanroyaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_ModuleName",
                table: "Commands",
                column: "ModuleName");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedWaifus_WaifuName",
                table: "FeaturedWaifus",
                column: "WaifuName");

            migrationBuilder.CreateIndex(
                name: "IX_ShopWaifus_WaifuName",
                table: "ShopWaifus",
                column: "WaifuName");

            migrationBuilder.CreateIndex(
                name: "IX_ShopWaifus_WaifuShopId",
                table: "ShopWaifus",
                column: "WaifuShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_PlaylistId",
                table: "Tracks",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_WaifuName",
                table: "UserInventories",
                column: "WaifuName");

            migrationBuilder.CreateIndex(
                name: "IX_WaifuWishlist_WaifuName",
                table: "WaifuWishlist",
                column: "WaifuName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannedUsers");

            migrationBuilder.DropTable(
                name: "BanrouletteParticipants");

            migrationBuilder.DropTable(
                name: "BanroyaleMessages");

            migrationBuilder.DropTable(
                name: "Blacklist");

            migrationBuilder.DropTable(
                name: "BlacklistedChannels");

            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Dailies");

            migrationBuilder.DropTable(
                name: "FeaturedWaifus");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "ImgurAlbums");

            migrationBuilder.DropTable(
                name: "Invites");

            migrationBuilder.DropTable(
                name: "LootBoxes");

            migrationBuilder.DropTable(
                name: "MalWaifus");

            migrationBuilder.DropTable(
                name: "Marriages");

            migrationBuilder.DropTable(
                name: "Params");

            migrationBuilder.DropTable(
                name: "PermissionRoles");

            migrationBuilder.DropTable(
                name: "Premiums");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "PublicRoles");

            migrationBuilder.DropTable(
                name: "RedditPosts");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "ShopWaifus");

            migrationBuilder.DropTable(
                name: "SpecialChannels");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Toasties");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "UserInventories");

            migrationBuilder.DropTable(
                name: "Voters");

            migrationBuilder.DropTable(
                name: "WaifuWishlist");

            migrationBuilder.DropTable(
                name: "Weeklies");

            migrationBuilder.DropTable(
                name: "WelcomeMessages");

            migrationBuilder.DropTable(
                name: "Banroulettes");

            migrationBuilder.DropTable(
                name: "Banroyales");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "WaifuShops");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Waifus");
        }
    }
}
