using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Namiko.Addons.Handlers.Criteria;

namespace Namiko.Addons.Handlers.Paginator
{
    public class PaginatedMessageCallback : CallbackBase<SocketMessageComponent>
    {
        private readonly PaginatedMessage _pager;

        private PaginatedAppearanceOptions options => _pager.Options;
        private readonly int pages;
        private int page = 1;

        public PaginatedMessageCallback(PaginatedMessage pager, ICustomContext context, ICriterion<SocketMessageComponent> criterion = null) : base(context, criterion)
        {
            _pager = pager;

            pages = _pager.CountPages();
            _pager.PageCount = pages;
        }

        public override async Task<IUserMessage> DisplayAsync()
        {
            var builder = new ComponentBuilder();
            if (_pager.PageCount > 100)
                builder = builder.WithButton(options.BackHundred);
            if (_pager.PageCount > 4)
                builder = builder.WithButton(options.BackTen);
            builder = builder.WithButton(options.Back);
            builder = builder.WithButton(options.Next);
            if (_pager.PageCount > 4)
                builder = builder.WithButton(options.NextTen);
            if (_pager.PageCount > 100)
                builder = builder.WithButton(options.NextHundred);

            var embed = BuildEmbed();
            var message = await Context.ReplyAsync(_pager.MessageText, embed: embed, components: builder.Build()).ConfigureAwait(false);
            Message = message;

            return message;
        }

        public async override Task<object> HandleCallbackAsync(SocketMessageComponent arg)
        {
            int x;
            if (arg.Data.CustomId == options.BackHundred.CustomId)
                x = -100;
            else if (arg.Data.CustomId == options.BackTen.CustomId)
                x = -10;
            else if (arg.Data.CustomId == options.Back.CustomId)
                x = -1;
            else if (arg.Data.CustomId == options.Next.CustomId)
                x = 1;
            else if (arg.Data.CustomId == options.NextTen.CustomId)
                x = 10;
            else if (arg.Data.CustomId == options.NextHundred.CustomId)
                x = 100;
            else
                x = 0;

            page += x;
            page = page < 1 ? 1 : page > _pager.PageCount ? _pager.PageCount : page;

            await RenderAsync().ConfigureAwait(false);
            return null;
        }

        public async override Task TimeoutAsync()
        {
            await Message.ModifyAsync(x => x.Components = null);
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
                }
                catch { }

            if (_pager.Fields != null)
                foreach (var x in _pager.Fields)
                {
                    try
                    {
                        eb.AddField(x.Title, x.Pages.ElementAt(page - 1).ToString(), x.Inline);
                    }
                    catch { }
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
