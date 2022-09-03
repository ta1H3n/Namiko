using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Namiko.Addons.Handlers;

namespace Namiko.Handlers.Attributes.Preconditions
{
    public class PlayerChannelAttribute : PreconditionAttribute
    {
        public override string ErrorMessage => null;
        public override string Name => "VoiceChannel";

        public override Attribute ReturnAttribute(HandlerType handlerType)
            => handlerType switch
            {
                HandlerType.Commands => new PlayerChannelCommandsAttribute(ErrorMessage),
                HandlerType.Interactions => new PlayerChannelInteractionsAttribute(),
                _ => throw new NotImplementedException(),
            };
        private class PlayerChannelCommandsAttribute : Discord.Commands.PreconditionAttribute
        {
            public override Task<Discord.Commands.PreconditionResult> CheckPermissionsAsync(Discord.Commands.ICommandContext context, Discord.Commands.CommandInfo command, IServiceProvider services)
            {
                SocketGuildUser user = (SocketGuildUser)context.User;
                var player = Music.Node.GetPlayer(context.Guild);
                var vc = user.VoiceChannel;

                if (player == null)
                    return Task.FromResult(Discord.Commands.PreconditionResult.FromError($"I'm not in a voice channel, Senpai. Try `{TextCommandService.GetPrefix(context.Guild.Id)}join`"));

                else if (vc != null && player.VoiceChannel == vc)
                    return Task.FromResult(Discord.Commands.PreconditionResult.FromSuccess());

                else
                    return Task.FromResult(Discord.Commands.PreconditionResult.FromError("We're not in the same voice channel, Senpai..."));
            }
            public PlayerChannelCommandsAttribute(string errorMessage)
            {
                ErrorMessage = errorMessage;
            }
        }
        private class PlayerChannelInteractionsAttribute : Discord.Interactions.PreconditionAttribute
        {
            public override Task<Discord.Interactions.PreconditionResult> CheckRequirementsAsync(IInteractionContext context, Discord.Interactions.ICommandInfo commandInfo, IServiceProvider services)
            {
                SocketGuildUser user = (SocketGuildUser)context.User;
                var player = Music.Node.GetPlayer(context.Guild);
                var vc = user.VoiceChannel;

                if (player == null)
                    return Task.FromResult(Discord.Interactions.PreconditionResult.FromError($"I'm not in a voice channel, Senpai. Try `{TextCommandService.GetPrefix(context.Guild.Id)}join`"));

                else if (vc != null && player.VoiceChannel == vc)
                    return Task.FromResult(Discord.Interactions.PreconditionResult.FromSuccess());

                else
                    return Task.FromResult(Discord.Interactions.PreconditionResult.FromError("We're not in the same voice channel, Senpai..."));
            }
            public PlayerChannelInteractionsAttribute()
            {
            }
        }
    }
}
