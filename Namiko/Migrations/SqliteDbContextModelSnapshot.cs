﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Namiko;

namespace Namiko.Migrations
{
    [DbContext(typeof(SqliteDbContext))]
    partial class SqliteDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("Namiko.Balance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Toasties");
                });

            modelBuilder.Entity("Namiko.BannedUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<DateTime>("DateBanEnd");

                    b.Property<DateTime>("DateBanStart");

                    b.Property<ulong>("ServerId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("BannedUsers");
                });

            modelBuilder.Entity("Namiko.Banroulette", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<int>("BanLengthHours");

                    b.Property<ulong>("ChannelId");

                    b.Property<int>("MaxParticipants");

                    b.Property<int>("MinParticipants");

                    b.Property<int>("RewardPool");

                    b.Property<ulong>("RoleReqId");

                    b.Property<ulong>("ServerId");

                    b.HasKey("Id");

                    b.ToTable("Banroulettes");
                });

            modelBuilder.Entity("Namiko.BanrouletteParticipant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BanrouletteId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("BanrouletteId");

                    b.ToTable("BanrouletteParticipants");
                });

            modelBuilder.Entity("Namiko.BlacklistedChannel", b =>
                {
                    b.Property<ulong>("ChannelId")
                        .ValueGeneratedOnAdd();

                    b.HasKey("ChannelId");

                    b.ToTable("BlacklistedChannels");
                });

            modelBuilder.Entity("Namiko.Daily", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Date");

                    b.Property<ulong>("GuildId");

                    b.Property<int>("Streak");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Dailies");
                });

            modelBuilder.Entity("Namiko.FeaturedWaifu", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("UserId");

                    b.Property<string>("WaifuName");

                    b.HasKey("id");

                    b.HasIndex("WaifuName");

                    b.ToTable("FeaturedWaifus");
                });

            modelBuilder.Entity("Namiko.ImgurAlbumLink", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AlbumId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("ImgurAlbums");
                });

            modelBuilder.Entity("Namiko.Invite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<ulong>("TeamId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("Namiko.LootBox", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.Property<ulong>("GuildId");

                    b.Property<int>("Type");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("LootBoxes");
                });

            modelBuilder.Entity("Namiko.Marriage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<ulong>("GuildId");

                    b.Property<bool>("IsMarried");

                    b.Property<ulong>("UserId");

                    b.Property<ulong>("WifeId");

                    b.HasKey("Id");

                    b.ToTable("Marriages");
                });

            modelBuilder.Entity("Namiko.Param", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Args");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Name");

                    b.Property<ulong>("Num");

                    b.HasKey("Id");

                    b.ToTable("Params");
                });

            modelBuilder.Entity("Namiko.Playlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.Property<string>("Name");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("Namiko.Premium", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("ClaimDate");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("Type");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Premiums");
                });

            modelBuilder.Entity("Namiko.Profile", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ColorHex");

                    b.Property<string>("Image");

                    b.Property<int>("LootboxesOpened");

                    b.Property<string>("PriorColorHexStack");

                    b.Property<string>("Quote");

                    b.Property<int>("Rep");

                    b.Property<DateTime>("RepDate");

                    b.HasKey("UserId");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("Namiko.PublicRole", b =>
                {
                    b.Property<ulong>("RoleId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.HasKey("RoleId");

                    b.ToTable("PublicRoles");
                });

            modelBuilder.Entity("Namiko.ReactionImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.Property<string>("Name");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Namiko.RedditPost", b =>
                {
                    b.Property<string>("PermaLink")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Upvotes");

                    b.HasKey("PermaLink");

                    b.ToTable("RedditPosts");
                });

            modelBuilder.Entity("Namiko.Server", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("JoinDate");

                    b.Property<ulong>("JoinLogChannelId");

                    b.Property<DateTime>("LeaveDate");

                    b.Property<string>("Prefix");

                    b.Property<ulong>("TeamLogChannelId");

                    b.Property<ulong>("WelcomeChannelId");

                    b.HasKey("GuildId");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Namiko.ShopWaifu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("BoughtBy");

                    b.Property<int>("Discount");

                    b.Property<int>("Limited");

                    b.Property<string>("WaifuName");

                    b.Property<int?>("WaifuShopId");

                    b.HasKey("Id");

                    b.HasIndex("WaifuName");

                    b.HasIndex("WaifuShopId");

                    b.ToTable("ShopWaifus");
                });

            modelBuilder.Entity("Namiko.SpecialChannel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Args");

                    b.Property<ulong>("ChannelId");

                    b.Property<ulong>("GuildId");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("SpecialChannels");
                });

            modelBuilder.Entity("Namiko.Team", b =>
                {
                    b.Property<ulong>("LeaderRoleId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("MemberRoleId");

                    b.HasKey("LeaderRoleId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("Namiko.Track", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PlaylistId");

                    b.Property<string>("SongHash");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("PlaylistId");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("Namiko.UserInventory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateBought");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("UserId");

                    b.Property<string>("WaifuName");

                    b.HasKey("Id");

                    b.HasIndex("WaifuName");

                    b.ToTable("UserInventories");
                });

            modelBuilder.Entity("Namiko.Voter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Voters");
                });

            modelBuilder.Entity("Namiko.Waifu", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("LongName");

                    b.Property<string>("Source");

                    b.Property<int>("Tier");

                    b.HasKey("Name");

                    b.ToTable("Waifus");
                });

            modelBuilder.Entity("Namiko.WaifuShop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("GeneratedDate");

                    b.Property<ulong>("GuildId");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("WaifuShops");
                });

            modelBuilder.Entity("Namiko.WaifuWish", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("UserId");

                    b.Property<string>("WaifuName");

                    b.HasKey("Id");

                    b.HasIndex("WaifuName");

                    b.ToTable("WaifuWishlist");
                });

            modelBuilder.Entity("Namiko.Weekly", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Weeklies");
                });

            modelBuilder.Entity("Namiko.WelcomeMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Message");

                    b.HasKey("Id");

                    b.ToTable("WelcomeMessages");
                });

            modelBuilder.Entity("Namiko.BanrouletteParticipant", b =>
                {
                    b.HasOne("Namiko.Banroulette", "Banroulette")
                        .WithMany()
                        .HasForeignKey("BanrouletteId");
                });

            modelBuilder.Entity("Namiko.FeaturedWaifu", b =>
                {
                    b.HasOne("Namiko.Waifu", "Waifu")
                        .WithMany()
                        .HasForeignKey("WaifuName");
                });

            modelBuilder.Entity("Namiko.ShopWaifu", b =>
                {
                    b.HasOne("Namiko.Waifu", "Waifu")
                        .WithMany()
                        .HasForeignKey("WaifuName");

                    b.HasOne("Namiko.WaifuShop", "WaifuShop")
                        .WithMany("ShopWaifus")
                        .HasForeignKey("WaifuShopId");
                });

            modelBuilder.Entity("Namiko.Track", b =>
                {
                    b.HasOne("Namiko.Playlist", "Playlist")
                        .WithMany("Tracks")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Namiko.UserInventory", b =>
                {
                    b.HasOne("Namiko.Waifu", "Waifu")
                        .WithMany()
                        .HasForeignKey("WaifuName");
                });

            modelBuilder.Entity("Namiko.WaifuWish", b =>
                {
                    b.HasOne("Namiko.Waifu", "Waifu")
                        .WithMany()
                        .HasForeignKey("WaifuName");
                });
#pragma warning restore 612, 618
        }
    }
}
