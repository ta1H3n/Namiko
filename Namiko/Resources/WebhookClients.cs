using Discord.Webhook;

namespace Namiko
{
    class WebhookClients
    {
        public static DiscordWebhookClient NamikoLogChannel { get; }
        public static DiscordWebhookClient GuildJoinLogChannel { get; }
        public static DiscordWebhookClient PremiumLogChannel { get; }
        public static DiscordWebhookClient CodeRedeemChannel { get; }
        public static DiscordWebhookClient UsageReportChannel { get; }
        public static DiscordWebhookClient ErrorLogChannel { get; }
        public static DiscordWebhookClient LavalinkChannel { get; }
        public static DiscordWebhookClient SauceChannel { get; }
        public static DiscordWebhookClient SauceRequestChannel { get; }

        static WebhookClients()
        {
            NamikoLogChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.Log);
            GuildJoinLogChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.GuildJoin);
            PremiumLogChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.Premium);
            CodeRedeemChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.CodeRedeem);
            UsageReportChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.UsageReport);
            ErrorLogChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.ErrorLog);
            LavalinkChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.Lavalink);
            SauceChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.Sauce);
            SauceRequestChannel = new DiscordWebhookClient(AppSettings.LogWebhooks.SauceRequest);
        }
    }

    public class LogWebhookAppsettings
    {
        public string Log { get; set; }
        public string GuildJoin { get; set; }
        public string Premium { get; set; }
        public string CodeRedeem { get; set; }
        public string UsageReport { get; set; }
        public string ErrorLog { get; set; }
        public string Lavalink { get; set; }
        public string Sauce { get; set; }
        public string SauceRequest { get; set; }
    }
}
