﻿using Microsoft.EntityFrameworkCore;
using Model;
using Namiko.Data;
using System.Threading.Tasks;

namespace Namiko
{
    public class StatsDbContext : DbContext
    {
        public DbSet<CommandLog> CommandLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string DbLocation = Locations.SqliteDb;
            Options.UseSqlite($"Data Source={DbLocation}stats.sqlite");
        }

        public async static Task<int> ExecuteSQL(string query)
        {
            using var db = new NamikoDbContext();
            return await db.Database.ExecuteSqlRawAsync(query);
        }
    }
}