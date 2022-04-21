using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers
{
    public class CustomInteractionContext : ICustomContext, IInteractionContext
    {
        public IDiscordClient Client { get; }
        public IGuild Guild { get; }
        public IMessageChannel Channel { get; }
        public IUser User { get; }
        public IDiscordInteraction Interaction { get; }


        public DateTimeOffset CreatedAt => Interaction.CreatedAt;


        public CustomInteractionContext(IDiscordClient client, IDiscordInteraction interaction, IMessageChannel channel = null)
        {
            Client = client;
            Guild = (interaction.User as IGuildUser)?.Guild;
            Channel = channel;
            User = interaction.User;
            Interaction = interaction;
        }


        public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, bool ephemeral = false)
            => await Interaction.FollowupAsync(text: text, isTTS: isTTS, embed: embed, options: options, allowedMentions: allowedMentions, components: components, embeds: embeds, ephemeral: ephemeral);

    }
}
