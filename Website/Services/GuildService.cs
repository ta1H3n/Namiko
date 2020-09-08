using Model;
using System.Collections.Generic;

namespace Website.Services
{
    public static class GuildService
    {
        public static HashSet<ulong> BotGuilds { get; }

        static GuildService()
        {
            BotGuilds = ServerDb.GetAll();
        }
    }
}
