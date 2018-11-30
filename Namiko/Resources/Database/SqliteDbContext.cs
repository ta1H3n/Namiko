using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Namiko.Data;

namespace Namiko.Resources.Datatypes
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<Toastie> Toasties { get; set; }
        public DbSet<Daily> Dailies { get; set; }
        public DbSet<UserRole> ShopItems { get; set; }
        public DbSet<ShopRole> ShopRoles { get; set; }
        public DbSet<WelcomeMessage> WelcomeMessages { get; set; }
        public DbSet<WelcomeChannel> WelcomeChannels { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<PublicRole> PublicRoles { get; set; }
        public DbSet<ReactionImage> Images { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<Waifu> Waifus { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }
        public DbSet<ShopWaifu> WaifuStores { get; set; }
        public DbSet<FeaturedWaifu> FeaturedWaifus { get; set; }
        public DbSet<Banroulette> Banroulettes { get; set; }
        public DbSet<BanrouletteParticipant> BanrouletteParticipants { get; set; }
        public DbSet<BannedUser> BannedUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string DbLocation = Locations.SqliteDb;
            Options.UseSqlite($"Data Source={DbLocation}Database.sqlite");
        }
    }
}
