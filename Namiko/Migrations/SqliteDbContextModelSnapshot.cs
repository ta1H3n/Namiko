﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Namiko.Resources.Database;

namespace Namiko.Migrations
{
    [DbContext(typeof(SqliteDbContext))]
    partial class SqliteDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity("Namiko.Resources.Datatypes.BannedUser", b =>
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

            modelBuilder.Entity("Namiko.Resources.Datatypes.Banroulette", b =>
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

            modelBuilder.Entity("Namiko.Resources.Datatypes.BanrouletteParticipant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BanrouletteId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("BanrouletteId");

                    b.ToTable("BanrouletteParticipants");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.BlacklistedChannel", b =>
                {
                    b.Property<ulong>("ChannelId")
                        .ValueGeneratedOnAdd();

                    b.HasKey("ChannelId");

                    b.ToTable("BlacklistedChannels");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.Daily", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Date");

                    b.Property<ulong>("GuildId");

                    b.Property<int>("Id");

                    b.Property<int>("Streak");

                    b.HasKey("UserId");

                    b.ToTable("Dailies");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.FeaturedWaifu", b =>
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

            modelBuilder.Entity("Namiko.Resources.Datatypes.Invite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("TeamId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.PublicRole", b =>
                {
                    b.Property<ulong>("RoleId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.HasKey("RoleId");

                    b.ToTable("PublicRoles");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.ReactionImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.Server", b =>
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

            modelBuilder.Entity("Namiko.Resources.Datatypes.ShopWaifu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("BoughtBy");

                    b.Property<int>("Discount");

                    b.Property<DateTime>("GeneratedDate");

                    b.Property<ulong>("GuildId");

                    b.Property<int>("Limited");

                    b.Property<string>("WaifuName");

                    b.HasKey("Id");

                    b.HasIndex("WaifuName");

                    b.ToTable("WaifuStores");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.Team", b =>
                {
                    b.Property<ulong>("LeaderRoleId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("MemberRoleId");

                    b.HasKey("LeaderRoleId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.Toastie", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.Property<ulong>("GuildId");

                    b.Property<int>("Id");

                    b.HasKey("UserId");

                    b.ToTable("Toasties");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.UserInventory", b =>
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

            modelBuilder.Entity("Namiko.Resources.Datatypes.Waifu", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("AddedByUserId");

                    b.Property<string>("Description");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("LongName");

                    b.Property<string>("Source");

                    b.Property<int>("Tier");

                    b.Property<int>("TimesBought");

                    b.HasKey("Name");

                    b.ToTable("Waifus");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.WelcomeChannel", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("ChannelId");

                    b.HasKey("GuildId");

                    b.ToTable("WelcomeChannels");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.WelcomeMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Message");

                    b.HasKey("Id");

                    b.ToTable("WelcomeMessages");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.BanrouletteParticipant", b =>
                {
                    b.HasOne("Namiko.Resources.Datatypes.Banroulette", "Banroulette")
                        .WithMany()
                        .HasForeignKey("BanrouletteId");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.FeaturedWaifu", b =>
                {
                    b.HasOne("Namiko.Resources.Datatypes.Waifu", "Waifu")
                        .WithMany()
                        .HasForeignKey("WaifuName");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.ShopWaifu", b =>
                {
                    b.HasOne("Namiko.Resources.Datatypes.Waifu", "Waifu")
                        .WithMany()
                        .HasForeignKey("WaifuName");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.UserInventory", b =>
                {
                    b.HasOne("Namiko.Resources.Datatypes.Waifu", "Waifu")
                        .WithMany()
                        .HasForeignKey("WaifuName");
                });
#pragma warning restore 612, 618
        }
    }
}
