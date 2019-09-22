using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Namiko
{
    public static class InteractiveExtensions
    {
        public static async Task<T> SelectItem<T>(this InteractiveBase<ShardedCommandContext> interactive, List<T> items, EmbedBuilder embed)
        {
            var msg = await interactive.Context.Channel.SendMessageAsync(embed: embed.Build());
            if (items.Count == 0)
                return default;

            var response = await interactive.NextMessageAsync(
                new Criteria<IMessage>()
                .AddCriterion(new EnsureSourceUserCriterion())
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureRangeCriterion(items.Count, Program.GetPrefix(interactive.Context))),
                new TimeSpan(0, 0, 23));

            _ = msg.DeleteAsync();
            int i = 0;
            try
            {
                i = int.Parse(response.Content);
            }
            catch
            {
                _ = interactive.Context.Message.DeleteAsync();
                return default;
            }
            _ = response.DeleteAsync();

            return items[i - 1];
        }
    }
}
