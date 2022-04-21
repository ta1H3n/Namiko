using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.Interactions;
using Discord.WebSocket;
using Namiko.Addons.Handlers.Criteria;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Addons.Handlers.Select;
using System;
using System.Threading.Tasks;

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
        public Task<IUserMessage> SelectMenuReplyAsync<T2>(SelectMenu<T2> menu, bool fromSourceUser = true)
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
        public Task<IUserMessage> SelectMenuReplyAsync<T2>(SelectMenu<T2> menu, ICriterion<SocketMessageComponent> criterion)
            => Interactive.SendSelectMenuAsync(Context, menu, criterion);


        #region IModuleBase
        protected virtual void SetContext(ICustomContext context)
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
        protected virtual void BeforeExecute(CommandInfo command)
        {
        }
        protected virtual void AfterExecute(CommandInfo command)
        {
        }
        protected virtual void OnModuleBuilding(CommandService commandService, ModuleBuilder builder)
        {
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
        void IModuleBase.BeforeExecute(CommandInfo command) => BeforeExecute(command);
        void IModuleBase.AfterExecute(CommandInfo command) => AfterExecute(command);
        void IModuleBase.OnModuleBuilding(CommandService commandService, ModuleBuilder builder) => OnModuleBuilding(commandService, builder);
        #endregion IModuleBase

        #region IInteractionModuleBase
        public virtual void AfterExecute(ICommandInfo command) { }
        public virtual void BeforeExecute(ICommandInfo command) { }
        public virtual Task BeforeExecuteAsync(ICommandInfo command) => Task.CompletedTask;
        public virtual Task AfterExecuteAsync(ICommandInfo command) => Task.CompletedTask;
        public virtual void OnModuleBuilding(InteractionService commandService, Discord.Interactions.ModuleInfo module) { }
        public virtual void Construct(Discord.Interactions.Builders.ModuleBuilder builder, InteractionService commandService) { }

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
