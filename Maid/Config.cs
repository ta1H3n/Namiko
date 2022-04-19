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
        public static HashSet<ulong> ImageChannelIds { get; set; } = new HashSet<ulong>();

        [JsonProperty("logChannelId")]
        public static ulong LogChannelId { get; set; }

        static Config()
        {
            string json = "";
            using (var stream = new FileStream(GetPath(), FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
                reader.Close();
            }

            JsonConvert.DeserializeObject<Config>(json);
        }

        public static void Save()
        {
            var config = new Config();
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            using (var stream = new FileStream(GetPath(), FileMode.Open, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(json);
                writer.Close();
            }
        }

        private static string GetPath()
        {
            return Directory.GetCurrentDirectory() + @"/appsettings.json";
        }
    }
}
