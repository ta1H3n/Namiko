﻿using System.Threading.Tasks;
using System.Linq;

using Discord.Commands;
using Discord;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System.Collections.Generic;
using System;
using Namiko.Resources.Attributes;
using Discord.WebSocket;
using Namiko.Core.Util;

namespace Namiko.Core.Modules
{
    public class Web : ModuleBase<SocketCommandContext>
    {
        [Command("Source"), Summary("Finds the source of an image in iqdb.\n**Usage**: `!source [image_url]` or `!source` with attached image.")]
        public async Task Source(string url = "", [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();

            url = !url.Equals("") ? url : Context.Message.Attachments.FirstOrDefault() != null ? 
                Context.Message.Attachments.FirstOrDefault().Url : null;
            
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

            await Context.Channel.SendMessageAsync("", false, WebUtil.IqdbSourceResultEmbed(result, url));
        }
    }
}