using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using Namiko.Resources.Attributes;
using Namiko.Resources.Xml;

namespace Namiko.Core.Basic
{
    public class SpookMode : ModuleBase<SocketCommandContext>
    {
        public static bool Enable { get; set; }
        public static int Rate { get; set; }
        private static List<string> Lines { get; set; }
        
        [Command("ToggleSpookMode"), Alias("tsm"), Summary("Enables spook mode, 0 to disable.\n**Usage**: `!spookmode [chance per message 1/n]`"), OwnerPrecondition]
        public async Task SpookModeEnable(int rate, [Remainder] string str = "")
        {
            if(rate == 0)
            {
                Enable = false;
                await Context.Channel.SendMessageAsync("Spook Mode disabled. I'll chill out.");
                return;
            }

            Rate = rate;
            Enable = true;

            await Context.Channel.SendMessageAsync($"Spook mode enabled at rate {rate}. It's on.");
        }

        [Command("NewSpook"), Alias("ns"), Summary("Adds a spook to the spook list."), OwnerPrecondition]
        public async Task AddSpook([Remainder] string spook)
        {
            var lines = XmlHelper.GetSpookLines();
            if (lines == null)
                lines = new List<string>();

            lines.Add(spook);
            Lines = lines;
            XmlHelper.ToXmlFile(lines, XmlPaths.GetSpookLinesPath());
            await Context.Channel.SendMessageAsync($"Added '{spook}'");
        }

        [Command("AllSpooks"), Alias("as"), Summary("Lists all spooks."), OwnerPrecondition]
        public async Task AllSpooks()
        {
            var lines = XmlHelper.GetSpookLines();
            string list = "";
            for(int i=0; i<lines.Count; i++)
            {
                list += $"{i}. {lines[i]}\n";
            }

            if (list == "")
                return;

            await Context.Channel.SendMessageAsync($"```{list}```");
        }

        [Command("DeleteSpook"), Alias("ds"), Summary("Deletes a spook.\n**Usage**: `!deletespook [no.]`"), OwnerPrecondition]
        public async Task DeleteSpook(int id, [Remainder] string str = "")
        {
            var lines = XmlHelper.GetSpookLines();
            lines.RemoveAt(id);

            if (!lines.Equals(Lines)) {
                Lines = lines;
                XmlHelper.ToXmlFile(lines, XmlPaths.GetSpookLinesPath());
                await Context.Channel.SendMessageAsync("If you hadn't messed up you wouldn't have had to delete it.");
                return;
            }

            await Context.Channel.SendMessageAsync("Sure I'd remove it ... if it existed.");
        }


        public async Task Spook(SocketCommandContext context)
        {
            if (!Enable)
                return;

            var rnd = new Random();
            if (rnd.Next(Rate) != 0)
                return;

            if (Lines == null)
                Lines = XmlHelper.FromXmlFile<List<string>>(XmlPaths.GetSpookLinesPath());

            string spook = Lines[rnd.Next(Lines.Count())];
            await context.Channel.SendMessageAsync(spook);
        }
    }
}
