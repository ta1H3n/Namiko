using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers
{
    public interface ICustomContext
    {
        BaseSocketClient Client { get; }
        SocketGuild Guild { get; }
        ISocketMessageChannel Channel { get; }
        SocketUser User { get; }

        DateTimeOffset CreatedAt { get; }


        Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, bool ephemeral = false);
        Task TriggerTypingAsync();
    }
}
