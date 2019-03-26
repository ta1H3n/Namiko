﻿using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Namiko.Data;
using System.Threading.Tasks;
using Namiko.Resources.Datatypes;

namespace Namiko.Resources.Database
{
    public class SqliteStatsDbContext : DbContext
    {
        public DbSet<ServerStat> ServerStats { get; set; }
        public DbSet<CommandStat> CommandStats { get; set; }
        public DbSet<UsageStat> UsageStats { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string DbLocation = Locations.SqliteDb;
            Options.UseSqlite($"Data Source={DbLocation}stats.sqlite");
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