using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Namiko
{
    public class PlayerChannelAttribute : CustomPrecondition
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            SocketGuildUser user = (SocketGuildUser)context.User;
            var player = Music.Node.GetPlayer(context.Guild);
            var vc = user.VoiceChannel;

            if (player == null)
                return Task.FromResult(PreconditionResult.FromError($"I'm not in a voice channel, Senpai. Try `{Program.GetPrefix(context.Guild.Id)}join`"));

            else if (vc != null && player.VoiceChannel == vc)
                return Task.FromResult(PreconditionResult.FromSuccess());

            else
                return Task.FromResult(PreconditionResult.FromError("We're not in the same voice channel, Senpai..."));
        }

        public override string GetName()
        {
            return "VoiceChannel";
        }
    }
}
