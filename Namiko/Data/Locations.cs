using System.IO;

namespace Namiko.Data
{
    public static class Locations
    {
        public static string SqliteDb { get; set; }
        public static string SpookyLinesXml { get; set; }
        public static string LootboxStatsJSON { get; set; }
        public static string AppSettingsJson { get; set; }

        static Locations()
        {
            LootboxStatsJSON = Directory.GetCurrentDirectory() + @"/Data/BoxStats.json";
            SqliteDb = Directory.GetCurrentDirectory() + @"/Data/stats.sqlite";
            SpookyLinesXml = Directory.GetCurrentDirectory() + @"/Data/SpookyLines.xml";
            AppSettingsJson = Directory.GetCurrentDirectory() + @"/Data/appsettings.json";
        }
    }
}
