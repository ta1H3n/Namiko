using Namiko.Data;
using System.Threading.Tasks;
using Namiko.Resources.Datatypes;
using Microsoft.EntityFrameworkCore;

namespace Namiko.Resources.Database
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<Toastie> Toasties { get; set; }
        public DbSet<Daily> Dailies { get; set; }
        public DbSet<WelcomeMessage> WelcomeMessages { get; set; }
       // public DbSet<WelcomeChannel> WelcomeChannels { get; set; }
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
        public DbSet<ImgurAlbumLink> ImgurAlbums { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Marriage> Marriages { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<BlacklistedChannel> BlacklistedChannels { get; set; }
        public DbSet<Weekly> Weeklies { get; set; }
        public DbSet<LootBox> LootBoxes { get; set; }
        public DbSet<Voter> Voters { get; set; }
        public DbSet<WaifuWish> WaifuWishlist { get; set; }
        public DbSet<SpecialChannel> SpecialChannels { get; set; }
        public DbSet<RedditPost> RedditPosts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string DbLocation = Locations.SqliteDb;
            Options.UseSqlite($"Data Source={DbLocation}Database.sqlite");
        }

        public async static Task<int> ExecuteSQL(string query)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Database.ExecuteSqlCommandAsync(query);
            }
        }
    }
}
