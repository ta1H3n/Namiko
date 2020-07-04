using Newtonsoft.Json;

namespace Namiko
{
    public class Config
    {
        [JsonProperty("token")]
        public static string Token { get; set; }

        [JsonProperty("defaultPrefix")]
        public static string DefaultPrefix { get; set; }

        [JsonProperty("ownerId")]
        public static ulong OwnerId { get; set; }

        [JsonProperty("insiderRoleId")]
        public static ulong InsiderRoleId { get; set; }

        [JsonProperty("namikoBannerUrl")]
        public static string NamikoBannerUrl { get; set; }

        [JsonProperty("sentryWebhook")]
        public static string SentryWebhook { get; set; }

        [JsonProperty("connectionString")]
        public static string ConnectionString { get; set; }

        [JsonProperty("imagePath")]
        public static string ImagePath { get; set; }

        [JsonProperty("sauceNaoApi")]
        public static string SauceNaoApi { get; set; }


        [JsonProperty("logWebhook")]
        public static string LogWebhook { get; set; }

        [JsonProperty("guildJoinWebhook")]
        public static string GuildJoinWebhook { get; set; }

        [JsonProperty("premiumWebhook")]
        public static string PremiumWebhook { get; set; }

        [JsonProperty("usageReportWebhook")]
        public static string UsageReportWebhook { get; set; }

        [JsonProperty("errorLogWebhook")]
        public static string ErrorLogWebhook { get; set; }

        [JsonProperty("lavalinkWebhook")]
        public static string LavalinkWebhook { get; set; }

        [JsonProperty("sauceWebhook")]
        public static string SauceWebhook { get; set; }
    }
}
