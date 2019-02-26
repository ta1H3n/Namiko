using System.Threading.Tasks;
using System.Linq;

using Discord.Commands;
using Discord;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System.Collections.Generic;
using System;
using Namiko.Resources.Preconditions;
using Discord.WebSocket;
using Namiko.Core.Util;
using Discord.Addons.Interactive;

namespace Namiko.Core.Modules
{
    public class Web : InteractiveBase<SocketCommandContext>
    {
        [Command("IQDB"), Summary("Finds the source of an image in iqdb.\n**Usage**: `!iqdb [image_url]` or `!iqdb` with attached image.")]
        public async Task Iqdb(string url = "", [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();

            url = !url.Equals("") ? url : Context.Message.Attachments.FirstOrDefault()?.Url;
            
            if(url == null)
            {
                await Context.Channel.SendMessageAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            var result = await WebUtil.IqdbUrlSearchAsync(url);

            if(!result.IsFound)
            {
                await Context.Channel.SendMessageAsync("No results. Too bad.");
                return;
            }
            
            await Context.Channel.SendMessageAsync("", false, WebUtil.IqdbSourceResultEmbed(result, url).Build());
        }

        [Command("Source"), Alias("SauceNao", "Sauce"), Summary("Finds the source of an image with SauceNao.\n**Usage**: `!source [image_url]` or `!source` with attached image.")]
        public async Task SaceNao(string url = null, [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();

            url = !url.Equals("") ? url : Context.Message.Attachments.FirstOrDefault()?.Url;

            if (url == null)
            {
                await Context.Channel.SendMessageAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            var sauce = await WebUtil.SauceNETSearchAsync(url);

            if (sauce.Request.Status != 0)
            {
                await Context.Channel.SendMessageAsync($"An error occured. Server response: `{sauce.Message}`");
                return;
            }

            for (int i = sauce.Results.Count - 1; i >= 0; i--)
            {
                if (Double.Parse(sauce.Results[i].Similarity) < 50)
                    sauce.Results.RemoveAt(i);
            }

            if (sauce.Results.Count == 0)
            {
                await Context.Channel.SendMessageAsync("No matches. Sorry~");
                return;
            }

            await Context.Channel.SendMessageAsync("", false, WebUtil.SauceEmbed(sauce, url).Build());
        }
    }
}