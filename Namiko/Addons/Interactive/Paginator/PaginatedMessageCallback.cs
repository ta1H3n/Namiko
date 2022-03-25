using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessageCallback : IReactionCallback
    {
        public SocketCommandContext Context { get; }
        public InteractiveService Interactive { get; private set; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion { get; }
        public TimeSpan? Timeout => options.Timeout;

        private readonly PaginatedMessage _pager;

        private PaginatedAppearanceOptions options => _pager.Options;
        private readonly int pages;
        private int page = 1;
        

        public PaginatedMessageCallback(InteractiveService interactive,
            SocketCommandContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            _pager = pager;

            pages = _pager.PageCount;
            if (pages == -1)
            {
                try
                {
                    pages = _pager.Fields.Max(x => x.Pages.Count());
                } catch { }
                try
                {
                    pages = pages > _pager.Pages.Count() ? pages : _pager.Pages.Count();
                } catch { }
            }
            _pager.PageCount = pages;
        }

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(_pager.MessageText, embed: embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                if (_pager.PageCount > 4)
                    await message.AddReactionAsync(options.First);

                await message.AddReactionAsync(options.Back);
                await message.AddReactionAsync(options.Next);

                if (_pager.PageCount > 4)
                    await message.AddReactionAsync(options.Last);

                if (_pager.PageCount > 4)
                    await message.AddReactionAsync(options.Jump);
            });
            // TODO: (Next major version) timeouts need to be handled at the service-level!
            if (Timeout.HasValue)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(message);
                    message.RemoveAllReactionsAsync();
                });
            }
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(options.First))
                page = 1;
            else if (emote.Equals(options.Next))
            {
                if (page >= pages)
                    return false;
                ++page;
            }
            else if (emote.Equals(options.Back))
            {
                if (page <= 1)
                    return false;
                --page;
            }
            else if (emote.Equals(options.Last))
                page = pages;
            else if (emote.Equals(options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var msg = await Context.Channel.SendMessageAsync("Which page do you wish to jump to? <:KannaHype:571690048001671238>");
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion())
                        .AddCriterion(new EnsureRangeCriterion(pages));
                    var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(60));
                    var request = int.Parse(response.Content);
                    page = request;
                    await RenderAsync().ConfigureAwait(false);
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await msg.DeleteAsync();
                });
                return false;
            }
            else if (emote.Equals(options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, options.InformationText, timeout: options.InfoTimeout);
                return false;
            }
            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }
        
        protected Embed BuildEmbed()
        {
            var eb = new EmbedBuilder()
                .WithAuthor(_pager.Author)
                .WithColor(_pager.Color)
                .WithFooter(_pager.Footer + string.Format(options.FooterFormat, page, pages))
                .WithTitle(_pager.Title)
                .WithImageUrl(_pager.ImageUrl)
                .WithThumbnailUrl(_pager.ThumbnailUrl);

            if (_pager.Pages != null && page <= _pager.Pages.Count())
            try
            {
                eb.WithDescription(_pager.Pages.ElementAt(page - 1).ToString());
            } catch { }
            
            if(_pager.Fields != null)
            foreach(var x in _pager.Fields)
            {
                try
                {
                    eb.AddField(x.Title, x.Pages.ElementAt(page - 1).ToString(), x.Inline);
                } catch { }
            }

            return eb.Build();
        }
        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}
