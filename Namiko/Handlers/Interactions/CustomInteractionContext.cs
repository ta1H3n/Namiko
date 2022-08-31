using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
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
        public IReadOnlyCollection<Attachment> Attachments => throw new NotImplementedException();

        private IUserMessage Response { get; set; }

        public DateTimeOffset CreatedAt => Interaction.CreatedAt;


        public CustomInteractionContext(DiscordShardedClient client, IDiscordInteraction interaction, ISocketMessageChannel channel = null)
        {
            if (interaction is SocketInteraction)
            {
                Guild = (((SocketInteraction)interaction).User as SocketGuildUser)?.Guild;
                User = ((SocketInteraction)interaction).User;
                Channel = ((SocketInteraction)interaction).Channel;
            }

            Client = client;
            Channel = channel ?? Channel;
            Interaction = interaction;
        }




        public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, bool ephemeral = false)
        {
            if (Interaction.Type == InteractionType.ModalSubmit)
            {
                await Interaction.RespondAsync(text: text, isTTS: isTTS, embed: embed, options: options, allowedMentions: allowedMentions, components: components, embeds: embeds, ephemeral: ephemeral);
            }
            else if (Response == null)
            {
                Response = await Interaction.FollowupAsync(text: text, isTTS: isTTS, embed: embed, options: options, allowedMentions: allowedMentions, components: components, embeds: embeds, ephemeral: ephemeral);
            }
            else
            {
                try
                {
                    await Response.ModifyAsync(x =>
                    {
                        x.Embed = embed;
                        x.Content = text;
                        x.Components = components;
                        x.AllowedMentions = allowedMentions;
                    });
                }
                catch
                {
                    Response = await Interaction.FollowupAsync(text: text, isTTS: isTTS, embed: embed, options: options, allowedMentions: allowedMentions, components: components, embeds: embeds, ephemeral: ephemeral);
                }
            }
            return Response;
        }

        public Task TriggerTypingAsync() => Task.CompletedTask;


        #region Interface lambdas

        IDiscordClient IInteractionContext.Client => Client;
        IGuild IInteractionContext.Guild => Guild;
        IMessageChannel IInteractionContext.Channel => Channel;
        IUser IInteractionContext.User => User;
        IDiscordInteraction IInteractionContext.Interaction => Interaction;

        #endregion Interface lambas
    }
}
