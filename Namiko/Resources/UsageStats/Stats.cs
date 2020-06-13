
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Namiko
{
    public static class Stats
    {
        public async static Task LogCommand(string commandName, ICommandContext context, bool success)
        {
            var log = new CommandLog
            {
                Date = DateTime.Now,
                Attachment = context.Message.Attachments.FirstOrDefault()?.ProxyUrl,
                Message = context.Message.Content,
                ChannelId = context.Channel.Id,
                GuildId = context.Guild?.Id ?? 0,
                Command = commandName,
                MessageId = context.Message.Id,
                UserId = context.User.Id,
                Success = success
            };

            await SaveCommandLog(log);
        }

        public async static Task SaveCommandLog(CommandLog log)
        {
            using (var db = new StatsDbContext())
            {
                db.CommandLogs.Add(log);
                await db.SaveChangesAsync();
            }
        }
    }
}
