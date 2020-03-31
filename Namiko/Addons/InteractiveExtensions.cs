using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Namiko
{
    public static class InteractiveExtensions
    {
        public static async Task<T> SelectItem<T>(this InteractiveBase<ShardedCommandContext> interactive, IList<T> items, EmbedBuilder embed)
        {
            if (items.Count <= 1)
                return items.FirstOrDefault();

            var msg = await interactive.Context.Channel.SendMessageAsync(embed: embed.Build());

            var response = await interactive.NextMessageAsync(
                new Criteria<IMessage>()
                .AddCriterion(new EnsureSourceUserCriterion())
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureRangeCriterion(items.Count, Program.GetPrefix(interactive.Context))),
                new TimeSpan(0, 0, 23));

            _ = msg.DeleteAsync();
            int i;
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

        public static async Task<SocketRole> SelectRole(this InteractiveBase<ShardedCommandContext> interactive, IList<SocketRole> roles, IEnumerable<ulong> roleIdsFilter = null, bool respond = true, string msg = null)
        {
            if (roleIdsFilter != null)
            {
                roles = roles.Where(x => roleIdsFilter.Contains(x.Id)).ToList();
            }

            if (!roles.Any())
            {
                if (respond)
                    await interactive.ReplyAsync($"*~ No Results ~*", Color.DarkRed.RawValue);
                return null;
            }

            if (roles.Count == 1)
                return roles.FirstOrDefault();

            string desc = $"Enter the number of the role you wish to select...\n\n";
            int i = 0;
            foreach (var role in roles)
            {
                i++;
                desc += $"`#{i}` {role.Mention}\n";
                if (desc.Length > 1900)
                {
                    desc = desc.Substring(0, 1900) + "...";
                    break;
                }
            }
            var eb = new EmbedBuilderPrepared(desc)
                .WithFooter("Times out in 23 seconds")
                .WithTitle(msg ?? "Roles Found");

            return await SelectItem<SocketRole>(interactive, roles, eb);
        }
        public static async Task<SocketRole> SelectRole(this InteractiveBase<ShardedCommandContext> interactive, string roleName, IEnumerable<ulong> roleIdsFilter = null, bool respond = true, string msg = null)
        {
            return await SelectRole(interactive, interactive.Context.Guild.Roles.Where(x => x.Name.Contains(roleName, StringComparison.OrdinalIgnoreCase)).ToList(), roleIdsFilter, respond, msg);
        }
        public static async Task<SocketRole> SelectRole(this InteractiveBase<ShardedCommandContext> interactive, IList<SocketRole> roles, string roleName, IEnumerable<ulong> roleIdsFilter = null, bool respond = true, string msg = null)
        {
            return await SelectRole(interactive, roles.Where(x => x.Name.Contains(roleName, StringComparison.OrdinalIgnoreCase)).ToList(), roleIdsFilter, respond, msg);
        }

        public static async Task<IUserMessage> ReplyAsync(this InteractiveBase<ShardedCommandContext> interactive, string msg, uint color = 0)
        {
            var eb = new EmbedBuilderPrepared(msg);

            if (color != 0)
            {
                eb.WithColor(color);
            }

            return await interactive.Context.Channel.SendMessageAsync(embed: eb.Build());
        }
    }
}
