﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Namiko.Resources.Datatypes;

namespace Namiko.Migrations
{
    [DbContext(typeof(SqliteDbContext))]
    [Migration("20181116005143_FeaturedWaifu")]
    partial class FeaturedWaifu
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("Namiko.Resources.Datatypes.Daily", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Date");

                    b.Property<int>("Streak");

                    b.HasKey("UserId");

                    b.ToTable("Dailies");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.FeaturedWaifu", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

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

                    b.Property<ulong>("TeamId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.PublicRole", b =>
                {
                    b.Property<ulong>("RoleId")
                        .ValueGeneratedOnAdd();

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

            modelBuilder.Entity("Namiko.Resources.Datatypes.ShopRole", b =>
                {
                    b.Property<ulong>("RoleId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DaysLength");

                    b.Property<ulong>("GuildId");

                    b.Property<ulong>("IfLimitedUserId");

                    b.Property<int>("Price");

                    b.HasKey("RoleId");

                    b.ToTable("ShopRoles");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.ShopWaifu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("BoughtBy");

                    b.Property<int>("Discount");

                    b.Property<DateTime>("GeneratedDate");

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

                    b.Property<ulong>("MemberRoleId");

                    b.HasKey("LeaderRoleId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.Toastie", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.HasKey("UserId");

                    b.ToTable("Toasties");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.UserInventory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateBought");

                    b.Property<ulong>("UserId");

                    b.Property<string>("WaifuName");

                    b.HasKey("Id");

                    b.HasIndex("WaifuName");

                    b.ToTable("UserInventories");
                });

            modelBuilder.Entity("Namiko.Resources.Datatypes.UserRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateToRemoveOn");

                    b.Property<ulong>("RoleId");

                    b.Property<ulong>("UserId");

                    b.HasKey("Id");

                    b.ToTable("ShopItems");
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
