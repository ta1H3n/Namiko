using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Maid
{
    public sealed class Config
    {
        [JsonProperty("token")]
        public static string Token { get; set; }

        [JsonProperty("imageChannelIds")]
        public static HashSet<ulong> ImageChannelIds { get; set; }

        [JsonProperty("logChannelId")]
        public static ulong LogChannelId { get; set; }

        static Config()
        {
            string JSON = "";
            var JSONLocation = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")) switch
            {
                "Development" => Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp3.1\Maid.dll", @"appsettings.json"),
                _ => Assembly.GetEntryAssembly().Location.Replace(@"Maid.dll", @"appsettings.json"),
            };
            using (var Stream = new FileStream(JSONLocation, FileMode.Open, FileAccess.Read))
            using (var ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            JsonConvert.DeserializeObject<Config>(JSON);
        }
    }
}
