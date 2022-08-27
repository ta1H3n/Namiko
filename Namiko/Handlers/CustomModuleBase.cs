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
using System.Collections.Generic;
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
        public async Task<IUserMessage> ReplyAsync(Embed embed)
            => await ReplyAsync(null, false, embed);
        public async Task<IUserMessage> ReplyAsync(string text, Color color)
            => await ReplyAsync(new EmbedBuilderPrepared(Context.User).WithDescription(text).WithColor(color).Build());
        public async Task<IUserMessage> ReplyAsync(string text)
            => await ReplyAsync(new EmbedBuilderPrepared(Context.User).WithDescription(text).Build());



        public Task<IUserMessage> DialogueReplyAsync(DialogueBox dialogue, bool fromSourceUser = true, int timeout = 60)
            => DialogueReplyAsync(dialogue, GetSourceUserCriterion(fromSourceUser), timeout);
        public Task<IUserMessage> DialogueReplyAsync(DialogueBox dialogue, ICriterion<SocketMessageComponent> criterion, int timeout = 60)
            => Interactive.SendDialogueBoxAsync(Context, dialogue, criterion, timeout);
        public Task<bool> Confirm(string question)
            => Interactive.Confirm(Context, question, GetSourceUserCriterion(true));



        public async Task SendModal<T>(T modal) where T : IModalBase
        {
            if (Context is not IInteractionContext)
            {
                throw new InvalidCastException("Modal sender does not support context type");
            }

            var context = Context as IInteractionContext;
            await context.Interaction.RespondWithModalAsync(modal.ToModal());
        }

            
            
        public Task<T2> SelectMenuReplyAsync<T2>(SelectMenu<T2> menu, bool fromSourceUser = true)
            => SelectMenuReplyAsync(menu, GetSourceUserCriterion(fromSourceUser));
        public Task<T2> SelectMenuReplyAsync<T2>(SelectMenu<T2> menu, ICriterion<SocketMessageComponent> criterion)
            => Interactive.SendSelectMenuAsync(Context, menu, criterion); 
        public Task<T2> Select<T2>(IEnumerable<T2> items, string placeholder = null, Embed embed = null, Func<T2, string> label = null, Func<T2, string> description = null, Func<T2, IEmote> emote = null, bool fromSourceUser = true)
        {
            label ??= (x) => x.ToString();

            int i = 0;
            var options = new Dictionary<string, SelectMenuOption<T2>>();
            foreach (var item in items)
            {
                var builder = new SelectMenuOptionBuilder(label(item), i.ToString());
                builder.WithLabel(label(item));
                if (description != null)
                    builder.WithDescription(description(item));
                if (emote != null)
                    builder.WithEmote(emote(item));

                options.Add(i++.ToString(), new SelectMenuOption<T2>(builder, item));
            }

            var menu = new SelectMenu<T2>(embed, options, placeholder);

            return SelectMenuReplyAsync(menu, fromSourceUser);
        }
        public Task<T2> Select<T2>(IEnumerable<T2> items, string message, string name = null, Func<T2, string> label = null, Func<T2, string> description = null, Func<T2, IEmote> emote = null, bool fromSourceUser = true)
            => Select(items, name, new EmbedBuilderPrepared(Context.User).WithDescription(message).Build(), label, description, emote, fromSourceUser);
        public Task<SocketRole> SelectRole(IEnumerable<SocketRole> roles, string roleNameFilter = null, IEnumerable<ulong> roleIdsFilter = null, string msg = "Role")
        {
            if (roleIdsFilter != null)
            {
                roles = roles.Where(x => roleIdsFilter.Contains(x.Id));
            }
            if (roleNameFilter != null)
            {
                roles = roles.Where(x => x.Name.Contains(roleNameFilter));
            }

            return Select(roles, placeholder: msg);
        }
        public Task<SocketRole> SelectRole(IEnumerable<ulong> roleIds, string roleNameFilter = null, string msg = "Role")
            => SelectRole(roleIds.Select(x => Context.Guild.GetRole(x)), roleNameFilter, null, msg);

    
        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ICriterion<SocketMessageComponent> criterion)
            => Interactive.SendPaginatedMessageAsync(Context, pager, criterion);
        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, bool fromSourceUser = true)
            => PagedReplyAsync(pager, GetSourceUserCriterion(fromSourceUser));


        private ICriterion<SocketMessageComponent> GetSourceUserCriterion(bool sourceUser)
        {
            var criteria = new Criteria<SocketMessageComponent>();
            if (sourceUser)
                criteria.AddCriterion(new EnsureComponentFromSourceUser());
            return criteria;
        }


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
