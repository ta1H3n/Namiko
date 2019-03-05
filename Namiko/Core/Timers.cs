using Namiko.Data;
using Namiko.Resources.Database;
using Namiko.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Resources.Datatypes;

namespace Namiko.Core
{
    public static class Timers
    {
        private static Timer Minute;
        private static Timer Minute5;
        private static Timer Hour;
        private static int CommandCallTick;

        public static void SetUp()
        {
            Minute = new Timer(1000 * 60);
            Minute.AutoReset = true;
            Minute.Enabled = true;
            Minute.Elapsed += Timer_TimeoutBlackjack;
            Minute.Elapsed += Timer_HourlyStats;

            Minute5 = new Timer(1000 * 60 * 5);
            Minute5.AutoReset = true;
            Minute5.Enabled = true;
            Minute5.Elapsed += Timer_Unban;
            Minute5.Elapsed += Timer_DailyStats;

            Hour = new Timer(1000 * 60 * 60);
            Hour.AutoReset = true;
            Hour.Enabled = true;
            Hour.Elapsed += Timer_BackupData;
            Hour.Elapsed += Timer_ExpireTeamInvites;
            Hour.Elapsed += Timer_CleanData;

            Console.WriteLine("Timers Ready.");
        }

        public static async void Timer_CleanData(object sender, ElapsedEventArgs e)
        {
            var servers = ServerDb.GetOld();
            foreach(var x in servers)
            {
                await PublicRoleDb.DeleteByGuild(x.GuildId);
                await TeamDb.DeleteByGuild(x.GuildId);
                await ToastieDb.DeleteByGuild(x.GuildId);
                await DailyDb.DeleteByGuild(x.GuildId);
                await WeeklyDb.DeleteByGuild(x.GuildId);
                await UserInventoryDb.DeleteByGuild(x.GuildId);
                await WaifuShopDb.DeleteByGuild(x.GuildId);
                await FeaturedWaifuDb.DeleteByGuild(x.GuildId);
                await ServerDb.DeleteServer(x.GuildId);
                Console.WriteLine($"Cleared server {x.GuildId}");
            }
        }
        private static async void Timer_ExpireTeamInvites(object sender, ElapsedEventArgs e)
        {
            await InviteDb.DeleteOlder(DateTime.Now.AddDays(-1));
        }
        private static async void Timer_TimeoutBlackjack(object sender, ElapsedEventArgs e)
        {
            var games = Blackjack.games;

            foreach (var x in games.Where(x => x.Value.Refresh.AddMinutes(1) < DateTime.Now).ToList())
            {
                await Blackjack.GameTimeout(x.Key, x.Value);
            }
        }
        private static void Timer_BackupData(object sender, ElapsedEventArgs e)
        {
            try
            {
                string backupLocation = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"backups/");
                string date = DateTime.Now.ToString("yyyy-MM-dd");

                File.Copy(Locations.SqliteDb + "Database.sqlite", Locations.SqliteDb + "backups/Database" + date + ".sqlite");
                File.Copy(Locations.SpookyLinesXml, Locations.SpookyLinesXml.Replace("SpookyLines.xml", "backups/SpookyLines") + date + ".xml");
                Console.WriteLine("Backups made.");
            }
            catch { }
        }
        private static async void Timer_Unban(object sender, ElapsedEventArgs e)
        {
            var bans = BanDb.GetBans();
            foreach(var x in bans)
            {
                if (x.DateBanEnd.CompareTo(System.DateTime.Now) == -1)
                {
                    Console.WriteLine("Unbanning " + x.UserId);
                    await BanDb.EndBan(x.UserId, x.ServerId);
                    try
                    {
                        await Program.GetClient().GetGuild(x.ServerId).RemoveBanAsync(x.UserId);
                    }
                    catch { }
                }
            }
        }



        // STATS
        public static async void Timer_DailyStats(object sender, ElapsedEventArgs e)
        {
            var date = System.DateTime.Now.Date;
            bool cool = false;
            List<ServerStat> servers = null;
            List<CommandStat> commands = null;

            using (SqliteStatsDbContext db = new SqliteStatsDbContext())
            {
                var sample = db.ServerStats.LastOrDefault();
                if (sample == null || sample.Date.Date < date)
                {
                    servers = Stats.ParseServerStats();
                    commands = Stats.ParseCommandStats();

                    db.ServerStats.AddRange(servers);
                    db.CommandStats.AddRange(commands);

                    await db.SaveChangesAsync();
                    cool = true;
                }
            }

            if (cool)
            {
                List<UsageStat> usage = new SqliteStatsDbContext().UsageStats.Where(x => x.Date > date.AddDays(-1) && x.Date < date).ToList();
                await UsageReport(servers, commands, usage);
                Stats.CommandCalls.Clear();
                Stats.ServerCommandCalls.Clear();
            }
        }
        public static async void Timer_HourlyStats(object sender, ElapsedEventArgs e)
        {
            var date = System.DateTime.Now;
            bool cool = false;

            using (SqliteStatsDbContext db = new SqliteStatsDbContext())
            {
                var sample = db.UsageStats.LastOrDefault();
                if (sample == null || sample.Date.AddHours(2) < date)
                {
                    db.UsageStats.Add(new UsageStat { Date = date.AddHours(-1).AddMinutes(-date.Minute).AddSeconds(-date.Second), Count = Stats.TotalCalls });

                    await db.SaveChangesAsync();
                    cool = true;
                }
            }

            if (cool)
            {
                Stats.TotalCalls = 0;
            }
        }

        private static async Task UsageReport(List<ServerStat> servers, List<CommandStat> commands, List<UsageStat> usage)
        {
            servers = servers.OrderByDescending(x => x.Count).ToList();
            commands = commands.OrderByDescending(x => x.Count).ToList();
            string small = SmallReport(servers, commands, usage);
            string big = BigReport(servers, commands, usage).Replace("*", "").Replace("`", "");

            using (var stream = GenerateStreamFromString(big)) {
                await ((SocketTextChannel)Program.GetClient().GetChannel(550619142105989131)).SendFileAsync(stream, System.DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd") + "_Namiko.txt", small);
            }
        }
        private static string SmallReport(List<ServerStat> servers, List<CommandStat> commands, List<UsageStat> usage)
        {
            string text = System.DateTime.Now.Date.ToString() + "\n\n";
            usage = usage.OrderByDescending(x => x.Count).ToList();

            text += "   Servers most used in:\n";
            for(int i = 0; i<3; i++)
            {
                try
                {
                    text += ToString(servers[i]) + "\n";
                } catch { break; }
            }
            
            text += "\n   Most used commands:\n";
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    text += ToString(commands[i]) + "\n";
                } catch { break; }
            }

            text += "\n   Peak usage:\n";
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    text += ToString(usage[i]) + "\n";
                } catch { break; }
            }

            text += $"\n Total command calls: **{usage.Sum(x => x.Count)}**";

            return text;
        }
        private static string BigReport(List<ServerStat> servers, List<CommandStat> commands, List<UsageStat> usage)
        {
            string text = System.DateTime.Now.Date.ToString() + "\n\n";

            text += "   Servers usage:\n";
            foreach(var x in servers)
            {
                text += ToString(x) + "\n";
            }

            text += "\n   Command usage:\n";
            foreach (var x in commands)
            {
                text += ToString(x) + "\n";
            }

            text += "\n   Command usage:\n";
            foreach (var x in usage)
            {
                text += ToString(x) + "\n";
            }

            return text;
        }

        private static string ToString(ServerStat serverStat)
        {
            string guildName = "";
            try
            {
                guildName = Program.GetClient().GetGuild(serverStat.GuildId).Name;
            } catch { }

            return $"`{serverStat.GuildId}` - **{serverStat.Count}** - *{guildName}*";
        }
        private static string ToString(CommandStat commandStat)
        {
            return $"`{commandStat.Name}` - **{commandStat.Count}**";
        }
        private static string ToString(UsageStat usageStat)
        {
            return $"`{usageStat.Date.ToString("hhtt")}` - **{usageStat.Count}**";
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}