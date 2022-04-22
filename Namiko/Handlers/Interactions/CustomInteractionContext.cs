using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Namiko.Addons.Handlers
{
    public class CustomInteractionContext : ICustomContext, IInteractionContext
    {
        public DiscordShardedClient Client { get; }
        public SocketGuild Guild { get; }
        public ISocketMessageChannel Channel { get; }
        public SocketUser User { get; }
        public IDiscordInteraction Interaction { get; }

        private IUserMessage Response { get; set; }

        public DateTimeOffset CreatedAt => Interaction.CreatedAt;


        public CustomInteractionContext(DiscordShardedClient client, SocketSlashCommand interaction, ISocketMessageChannel channel = null)
        {
            Client = client;
            Guild = (interaction.User as SocketGuildUser)?.Guild;
            Channel = channel;
            User = interaction.User;
            Interaction = interaction;
        }




        public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, bool ephemeral = false)
        {
            if (Response == null)
            {
                Response = await Interaction.FollowupAsync(text: text, isTTS: isTTS, embed: embed, options: options, allowedMentions: allowedMentions, components: components, embeds: embeds, ephemeral: ephemeral);
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


        #region Interface lambdas

        IDiscordClient IInteractionContext.Client => Client;
        IGuild IInteractionContext.Guild => Guild;
        IMessageChannel IInteractionContext.Channel => Channel;
        IUser IInteractionContext.User => User;
        IDiscordInteraction IInteractionContext.Interaction => Interaction;

        #endregion Interface lambas
    }
}
