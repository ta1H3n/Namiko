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

        static WebhookClients()
        {
            NamikoLogChannel = new DiscordWebhookClient(Config.LogWebhook);
            GuildJoinLogChannel = new DiscordWebhookClient(Config.GuildJoinWebhook);
        }
    }
}
