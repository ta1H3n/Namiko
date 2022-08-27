using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers
{
    public class CustomCommandContext : ICustomContext, ICommandContext
    {
        public DiscordShardedClient Client { get; }
        public SocketGuild Guild { get; }
        public ISocketMessageChannel Channel { get; }
        public SocketUser User { get; }
        public SocketUserMessage Message { get; }

        private IUserMessage Response { get; set; }

        public DateTimeOffset CreatedAt => Message.CreatedAt;


        public CustomCommandContext(DiscordShardedClient client, SocketUserMessage msg)
        {
            Client = client;
            Guild = (msg.Channel as SocketGuildChannel)?.Guild;
            Channel = msg.Channel;
            User = msg.Author;
            Message = msg;
        }


        public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, bool ephemeral = false)
        {
            if (Response == null)
            {
                Response = await Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags);
            }
            else
            {
                await Response.ModifyAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = text;
                    x.Components = components;
                    x.AllowedMentions = allowedMentions;
                });
            }
            return Response;
        }

        public async Task TriggerTypingAsync() => await Channel.TriggerTypingAsync();


        #region Interface lambdas

        IDiscordClient ICommandContext.Client => Client;
        IGuild ICommandContext.Guild => Guild;
        IMessageChannel ICommandContext.Channel => Channel;
        IUser ICommandContext.User => User;
        IUserMessage ICommandContext.Message => Message;


        #endregion Interface lambas
    }
}
