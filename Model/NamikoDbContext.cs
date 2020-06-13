using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Model
{
    public class NamikoDbContext : DbContext
    {
        public static string ConnectionString { get; set; }

        public DbSet<Balance> Toasties { get; set; }
        public DbSet<Daily> Dailies { get; set; }
        public DbSet<WelcomeMessage> WelcomeMessages { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<PublicRole> PublicRoles { get; set; }
        public DbSet<ReactionImage> Images { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<Waifu> Waifus { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }
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
        public DbSet<Premium> Premiums { get; set; }
        public DbSet<Param> Params { get; set; }
        public DbSet<ShopWaifu> ShopWaifus { get; set; }
        public DbSet<WaifuShop> WaifuShops { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Blacklisted> Blacklist { get; set; }
        public DbSet<PermissionRole> PermissionRoles { get; set; }
        public DbSet<MalWaifu> MalWaifus { get; set; }
        public DbSet<Banroyale> Banroyales { get; set; }
        public DbSet<BanroyaleMessage> BanroyaleMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            Options.UseNpgsql("Server=157.245.26.12;Port=5432;Database=NamikoDev;User Id=postHen;Password=ta1gres;");
            //Options.UseSqlite(ConnectionString);
        }

        public async static Task<int> ExecuteSQL(string query)
        {
            using var db = new NamikoDbContext();
            return await db.Database.ExecuteSqlRawAsync(query);
        }
    }

    public static class SqliteHelper
    {
        public static IEnumerable<IEnumerable<KeyValuePair<string, object>>> DynamicListFromSql(this DbContext db, string Sql, Dictionary<string, object> Params)
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = Sql;
            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }

            foreach (KeyValuePair<string, object> p in Params)
            {
                DbParameter dbParameter = cmd.CreateParameter();
                dbParameter.ParameterName = p.Key;
                dbParameter.Value = p.Value;
                cmd.Parameters.Add(dbParameter);
            }

            using var dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                var row = new List<KeyValuePair<string, object>>();
                for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                {
                    row.Add(new KeyValuePair<string, object>(dataReader.GetName(fieldCount), dataReader[fieldCount]));
                }
                yield return row;
            }
        }
    }
}
