using System.Reflection;

namespace Namiko.Data
{
    public static class Locations
    {
        public static string SettingsJSON { get; set; }
        public static string SqliteDb { get; set; }
        public static string SpookyLinesXml { get; set; }
        public static string ImgurJSON { get; set; }
        public static string DblTokenTxt { get; set; }
        public static string RedditJSON { get; set; }
        public static string LootboxStatsJSON { get; set; }

        public static void SetUpRelease()
        {
            SettingsJSON = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/Settings.json");
            ImgurJSON = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/Imgur.json");
            RedditJSON = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/Reddit.json");
            LootboxStatsJSON = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/BoxStats.json");
            DblTokenTxt = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/DblToken.txt");
            SqliteDb = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/");
            SpookyLinesXml = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"xml/SpookyLines.xml");
        }
        public static void SetUpDebug()
        {
            SettingsJSON = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\net6.0\Namiko.dll", @"Data\Settings.json");
            ImgurJSON = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\net6.0\Namiko.dll", @"Data\Imgur.json");
            RedditJSON = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\net6.0\Namiko.dll", @"Data\Reddit.json");
            LootboxStatsJSON = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\net6.0\Namiko.dll", @"Data\BoxStats.json");
            DblTokenTxt = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\net6.0\Namiko.dll", @"Data\DblToken.txt");
            SqliteDb = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\net6.0\Namiko.dll", @"Data\");
            SpookyLinesXml =  Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\net6.0\Namiko.dll", @"Resources\Xml\SpookyLines.xml");
        }
    }
}
