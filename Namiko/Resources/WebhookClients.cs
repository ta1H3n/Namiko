using System;
using System.Collections.Generic;
using System.Text;
using Discord.Webhook;

namespace Namiko
{
    class WebhookClients
    {
        public static DiscordWebhookClient NamikoLogChannel { get; }
        public static DiscordWebhookClient GuildJoinLogChannel { get; }
        public static DiscordWebhookClient ErrorLogChannel { get; }
        public static DiscordWebhookClient LavalinkChannel { get; }
        public static DiscordWebhookClient SauceChannel { get; }

        static WebhookClients()
        {
            NamikoLogChannel = new DiscordWebhookClient(Config.LogWebhook);
            GuildJoinLogChannel = new DiscordWebhookClient(Config.GuildJoinWebhook);
            ErrorLogChannel = new DiscordWebhookClient(Config.ErrorLogWebhook);
            LavalinkChannel = new DiscordWebhookClient(Config.LavalinkWebhook);
            SauceChannel = new DiscordWebhookClient(Config.SauceWebhook);
        }
    }
}
