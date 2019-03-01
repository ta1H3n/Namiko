using Namiko.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Core
{
    public static class Stats
    {
        public static Dictionary<ulong, int> ServerCommandCalls = new Dictionary<ulong, int>();
        public static Dictionary<string, int> CommandCalls = new Dictionary<string, int>();
        public static int TotalCalls = 0;

        public static void IncrementServer(ulong GuildId)
        {
            int val = ServerCommandCalls.GetValueOrDefault(GuildId);
            ServerCommandCalls[GuildId] = val + 1;
        }

        public static void IncrementCommand(string CommandName)
        {
            int val = CommandCalls.GetValueOrDefault(CommandName);
            CommandCalls[CommandName] = val + 1;
        }

        public static void IncrementCalls()
        {
            TotalCalls++;
        }


        public static List<ServerStat> ParseServerStats()
        {
            var list = new List<ServerStat>();
            DateTime date = System.DateTime.Now.Date;
            foreach(var pair in ServerCommandCalls)
            {
                list.Add(new ServerStat { GuildId = pair.Key, Date = date, Count = pair.Value });
            }

            return list;
        }

        public static List<CommandStat> ParseCommandStats()
        {
            var list = new List<CommandStat>();
            DateTime date = System.DateTime.Now.Date;
            foreach (var pair in CommandCalls)
            {
                list.Add(new CommandStat { Name = pair.Key, Date = date, Count = pair.Value });
            }

            return list;
        }
    }
}
