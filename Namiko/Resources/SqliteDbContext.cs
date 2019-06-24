using Namiko.Data;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;

namespace Namiko
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<Balance> Toasties { get; set; }
        public DbSet<Daily> Dailies { get; set; }
        public DbSet<WelcomeMessage> WelcomeMessages { get; set; }
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
        public DbSet<Premium> Premiums { get; set; }
        public DbSet<Param> Params { get; set; }

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

    public static class SqliteHelper
    {
        public static IEnumerable<dynamic> DynamicListFromSql(this DbContext db, string Sql, Dictionary<string, object> Params)
        {
            using (var cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = Sql;
                if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }

                foreach (KeyValuePair<string, object> p in Params)
                {
                    DbParameter dbParameter = cmd.CreateParameter();
                    dbParameter.ParameterName = p.Key;
                    dbParameter.Value = p.Value;
                    cmd.Parameters.Add(dbParameter);
                }

                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var row = new ExpandoObject() as IDictionary<string, object>;
                        for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                        {
                            row.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);
                        }
                        yield return row;
                    }
                }
            }
        }
    }
}
