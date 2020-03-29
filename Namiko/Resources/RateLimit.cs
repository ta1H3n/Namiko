using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Namiko
{
    public class RateLimit
    {
        public static readonly uint InvokeLimit = 4;
        public static readonly TimeSpan InvokeLimitPeriod = new TimeSpan(0, 0, 10);
        public static readonly TimeSpan InvokeLockoutPeriod = new TimeSpan(0, 0, 10);
        private static readonly Dictionary<ulong, CommandTimeout> InvokeTracker = new Dictionary<ulong, CommandTimeout>();
        public static readonly Dictionary<ulong, DateTime> InvokeLockout = new Dictionary<ulong, DateTime>();

        public static bool CanExecute(ulong channelId)
        {
            var now = DateTime.Now;

            var timeout = (InvokeTracker.TryGetValue(channelId, out var t) && ((now - t.FirstInvoke) < InvokeLimitPeriod)) ? t : new CommandTimeout(now);
            timeout.TimesInvoked++;

            if (timeout.TimesInvoked <= InvokeLimit)
            {
                InvokeTracker[channelId] = timeout;
                return true;
            }
            else
            {
                InvokeLockout[channelId] = DateTime.Now.Add(InvokeLockoutPeriod);
                return false;
            }
        }

        private sealed class CommandTimeout
        {
            public int TimesInvoked { get; set; } = 0;
            public DateTime FirstInvoke { get; }

            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }
        }
    }
}