using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Website.Services
{
    public static class DiscordRestClientService
    {
        public static Dictionary<ulong, DiscordRestClient> DiscordRestClients { get; set; }

        static DiscordRestClientService()
        {
            DiscordRestClients = new Dictionary<ulong, DiscordRestClient>();
        }

        public static async Task<DiscordRestClient> GetClient(ulong userId, string accessToken)
        {
            if (DiscordRestClients.TryGetValue(userId, out var client))
            {
                ((TimedDiscordRestClient)client).ResetTimer();
                return client;
            }
            else
            {
                client = new TimedDiscordRestClient();
                await client.LoginAsync(Discord.TokenType.Bearer, accessToken);
                DiscordRestClients.Add(userId, client);
                return client;
            }
        }

        private class TimedDiscordRestClient : DiscordRestClient
        {
            private Timer Timer;
            private TimeSpan LifeTime;

            public TimedDiscordRestClient() : this(TimeSpan.FromMinutes(10)) { }
            public TimedDiscordRestClient(TimeSpan lifeTime) : base()
            {
                LifeTime = lifeTime;
                Timer = new Timer(Timer_Dispose, null, (long)LifeTime.TotalMilliseconds, Timeout.Infinite);
            }

            public void ResetTimer()
            {
                Timer.Change((long)LifeTime.TotalMilliseconds, Timeout.Infinite);
            }

            public void Timer_Dispose(object state)
            {
                DiscordRestClientService.DiscordRestClients.Remove(CurrentUser.Id);
                Timer.Dispose();
                Dispose();
            }
        }
    }

}
