using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers
{
    public interface ICustomContext
    {
        IDiscordClient Client { get; }
        IGuild Guild { get; }
        IMessageChannel Channel { get; }
        IUser User { get; }


        DateTimeOffset CreatedAt { get; }


        Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, bool ephemeral = false);
    }
}
