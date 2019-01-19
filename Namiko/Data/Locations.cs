using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Namiko.Data
{
    public static class Locations
    {
        public static string SettingsJSON { get; set; }
        public static string SqliteDb { get; set; }
        public static string SpookyLinesXml { get; set; }
        public static string ImgurJSON { get; set; }

        public static void SetUpRelease()
        {
            SettingsJSON = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/Settings.json");
            ImgurJSON = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/Imgur.json");
            SqliteDb = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"data/");
            SpookyLinesXml = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"xml/SpookyLines.xml");
        }
        public static void SetUpDebug()
        {
            SettingsJSON = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp2.1\Namiko.dll", @"Data\Settings.json");
            ImgurJSON = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp2.1\Namiko.dll", @"Data\Imgur.json");
            SqliteDb = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp2.1\Namiko.dll", @"Data\");
            SpookyLinesXml =  Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp2.1\Namiko.dll", @"Resources\Xml\SpookyLines.xml");
        }
    }
}
