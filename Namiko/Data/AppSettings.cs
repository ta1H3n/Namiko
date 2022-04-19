using Namiko.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Namiko
{
    public sealed class AppSettings
    {
        [JsonProperty]
        public static string Token { get; set; }
        [JsonProperty]
        public static string DefaultPrefix { get; set; }


        [JsonProperty]
        public static ulong OwnerId { get; set; }
        [JsonProperty]
        public static ulong InsiderRoleId { get; set; }


        [JsonProperty]
        public static string NamikoBannerUrl { get; set; }
        [JsonProperty]
        public static string ConnectionString { get; set; }


        [JsonProperty]
        public static string SentryWebhook { get; set; }
        [JsonProperty]
        public static string SauceNaoApi { get; set; }
        [JsonProperty]
        public static string DiscordBotListToken { get; set; }


        [JsonProperty]
        public static string ImageHost { get; set; }
        [JsonProperty]
        public static string ImageHostKey { get; set; }
        [JsonProperty]
        public static string ImageUrlPath { get; set; }


        [JsonProperty]
        public static LogWebhookAppsettings LogWebhooks { get; set; }


        [JsonProperty]
        public static ApiSettings Imgur { get; set; }
        [JsonProperty]
        public static ApiSettings Reddit { get; set; }


        static AppSettings()
        {
            try
            {
                string json = "";
                using (var stream = new FileStream(GetPath(), FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                    reader.Close();
                }

                JsonConvert.DeserializeObject<AppSettings>(json);
                Logger.Log("AppSettings initialised");
            }
            catch (Exception ex)
            {
                Logger.Log("AppSettings threw an exception on first set up");
                Logger.Log(ex.Message);
            }
            finally
            {
                TestAppsettings();
            }
        }

        public static void Save()
        {
            var config = new AppSettings();
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
            return Locations.AppSettingsJson;
        }

        private static void TestAppsettings()
        {
            LogMissingProperties(typeof(AppSettings).GetProperties());
            if (AppSettings.Imgur != default)
            {
                LogMissingProperties(AppSettings.Imgur.GetProperties(), nameof(Imgur));
            }
            if (AppSettings.Reddit != default)
            {
                LogMissingProperties(AppSettings.Reddit.GetProperties(), nameof(Reddit));
            }
            if (AppSettings.LogWebhooks != default)
            {
                LogMissingProperties(AppSettings.LogWebhooks.GetProperties(), nameof(LogWebhooks));
            }
        }

        private static void LogMissingProperties(PropertyInfo[] properties)
        {
            foreach (var prop in properties)
            {
                if (prop.GetValue(null) == default)
                {
                    Logger.Log($"AppSettings missing: {prop.Name}", Discord.LogSeverity.Warning);
                }
            }
        }
        private static void LogMissingProperties(List<KeyValuePair<string, object>> properties, string parent = "")
        {
            foreach (var prop in properties)
            {
                if (prop.Value == default || (prop.Value is string && prop.Value as string == ""))
                {
                    Logger.Log($"AppSettings missing: {parent}.{prop.Key}", Discord.LogSeverity.Warning);
                }
            }
        }
    }

    public class ApiSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
    }
}
