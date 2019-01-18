using Namiko.Data;
using Namiko.Resources.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Namiko.Core
{
    public static class Timers
    {
        private static Timer Minute5;
        private static Timer Hour;
        private static int CommandCallTick;

        public static void SetUp()
        {
            Minute5 = new Timer(1000 * 60 * 5);
            Minute5.AutoReset = true;
            Minute5.Enabled = true;
            Minute5.Elapsed += Timer_Unban;

            Hour = new Timer(1000 * 60 * 60);
            Hour.AutoReset = true;
            Hour.Enabled = true;
            Hour.Elapsed += Timer_BackupData;
            Hour.Elapsed += Timer_ResetCommandCallTick;

            Console.WriteLine("Timers Ready.");
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
        private static void Timer_ResetCommandCallTick(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(e.SignalTime + " - " + CommandCallTick + " command calls.");
            CommandCallTick = 0;
        }
        public static void CommandCallTickIncrement()
        {
            CommandCallTick++;
        }
    }
}
