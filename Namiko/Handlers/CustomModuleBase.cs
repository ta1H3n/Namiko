using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.Interactions;
using Discord.WebSocket;
using Namiko.Addons.Handlers.Criteria;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Addons.Handlers.Select;
using Namiko.Handlers;
using Namiko.Handlers.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using PreconditionAttribute = Namiko.Handlers.Attributes.PreconditionAttribute;

namespace Namiko.Addons.Handlers
{
    public abstract class CustomModuleBase<T> : IModuleBase, IInteractionModuleBase where T : ICustomContext
    {
        public T Context { get; private set; }
        public InteractiveService Interactive { get; set; }

        public async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null, MessageFlags flags = MessageFlags.None, bool ephemeral = false)
            => await Context.ReplyAsync(text, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds, flags, ephemeral);

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, bool fromSourceUser = true)
        {
            var criteria = new Criteria<SocketMessageComponent>();
            if (fromSourceUser)
                criteria.AddCriterion(new EnsureComponentFromSourceUser());
            return PagedReplyAsync(pager, criteria);
        }
        public Task<IUserMessage> DialogueReplyAsync(DialogueBox dialogue, bool fromSourceUser = true)
        {
            var criteria = new Criteria<SocketMessageComponent>();
            if (fromSourceUser)
                criteria.AddCriterion(new EnsureComponentFromSourceUser());
            return DialogueReplyAsync(dialogue, criteria);
        }
        public Task<T2> SelectMenuReplyAsync<T2>(SelectMenu<T2> menu, bool fromSourceUser = true)
        {
            var criteria = new Criteria<SocketMessageComponent>();
            if (fromSourceUser)
                criteria.AddCriterion(new EnsureComponentFromSourceUser());
            return SelectMenuReplyAsync(menu, criteria);
        }
        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ICriterion<SocketMessageComponent> criterion)
            => Interactive.SendPaginatedMessageAsync(Context, pager, criterion);
        public Task<IUserMessage> DialogueReplyAsync(DialogueBox dialogue, ICriterion<SocketMessageComponent> criterion)
            => Interactive.SendDialogueBoxAsync(Context, dialogue, criterion);
        public Task<T2> SelectMenuReplyAsync<T2>(SelectMenu<T2> menu, ICriterion<SocketMessageComponent> criterion)
            => Interactive.SendSelectMenuAsync(Context, menu, criterion);


        #region IModuleBase
        public virtual void BeforeExecute(CommandInfo command)
        {
        }
        public virtual void AfterExecute(CommandInfo command)
        {
        }
        public virtual void OnModuleBuilding(CommandService commandService, ModuleBuilder builder)
        {
            foreach (var cmd in builder.Commands)
            {
                var desc = cmd.Attributes.FirstOrDefault(x => x is DescriptionAttribute);
                if (desc != default)
                {
                    cmd.WithSummary((desc as DescriptionAttribute).Description);
                }
                foreach (var prec in cmd.Attributes.Where(x => x is PreconditionAttribute).Select(x => x as PreconditionAttribute))
                {
                    cmd.AddPrecondition(prec.ReturnAttribute(Handler.Commands) as Discord.Commands.PreconditionAttribute);
                }
            }
        }

        void IModuleBase.SetContext(ICommandContext context)
        {
            if (context is not T)
            {
                throw new InvalidOperationException($"Invalid context type. Expected {typeof(T).Name}, got {context.GetType().Name}.");
            }
            else
            {
                Context = (T)context;
            }
        }
        #endregion IModuleBase

        #region IInteractionModuleBase
        public virtual void AfterExecute(ICommandInfo command) { }
        public virtual void BeforeExecute(ICommandInfo command) { }
        public virtual Task BeforeExecuteAsync(ICommandInfo command) => Task.CompletedTask;
        public virtual Task AfterExecuteAsync(ICommandInfo command) => Task.CompletedTask;
        public virtual void OnModuleBuilding(InteractionService commandService, Discord.Interactions.ModuleInfo module) { }
        public virtual void Construct(Discord.Interactions.Builders.ModuleBuilder builder, InteractionService commandService)
        {
            foreach (var cmd in builder.SlashCommands)
            {
                //var desc = cmd.Attributes.FirstOrDefault(x => x is DescriptionAttribute);
                //if (desc != default)
                //{
                //    cmd.WithSu((desc as DescriptionAttribute).Description);
                //}
                var atr = cmd.Attributes.Where(x => x is PreconditionAttribute).Select(x => x as PreconditionAttribute);
                cmd.WithPreconditions(atr.Select(x => x.ReturnAttribute(Handler.Interactions) as Discord.Interactions.PreconditionAttribute).ToArray());
            }
        }

        void IInteractionModuleBase.SetContext(IInteractionContext context)
        {
            if (context is not T)
            {
                throw new InvalidOperationException($"Invalid context type. Expected {typeof(T).Name}, got {context.GetType().Name}.");
            }
            else
            {
                Context = (T)context;
            }
        }
        #endregion IInteractionModuleBase
    }
}
