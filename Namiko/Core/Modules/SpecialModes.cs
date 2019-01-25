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
using Namiko.Resources.Preconditions;
using Namiko.Resources.Xml;
using Namiko.Core.Util;
using Discord.Addons.Interactive;

namespace Namiko.Core.Modules
{
    public class SpecialModes : InteractiveBase<SocketCommandContext>
    {
        public static bool ChristmasModeEnable { get; set; }
        public static int ChristmasModeRate { get; set; }
        public string[] ChristmasEmotes = { "<:ChristmasAwoo:523987349743599616> ",  "<:XmasYeah:523986875497578522>", "<:ChristmasOwO:523983823591964699>", "<:ChristmasPanda:523987350313762816>", "<:Padoru:523993285275156480>", "<:XmasSip:523995761742970901>", "<:MikuPresent:523995762028445701>", "<:OwOPresent:523995763559235594>" };
        public static bool SpookModeEnable { get; set; }
        public static int SpookModeRate { get; set; }
        private static List<string> Lines { get; set; }
        
        [Command("ToggleSpookMode"), Alias("tsm"), Summary("Enables spook mode, 0 to disable.\n**Usage**: `!tsm [chance per message 1/n]`"), OwnerPrecondition]
        public async Task ToggleSpookMode(int rate, [Remainder] string str = "")
        {
            if(rate == 0)
            {
                SpookModeEnable = false;
                await Context.Channel.SendMessageAsync("Spook Mode disabled. I'll chill out.");
                return;
            }

            SpookModeRate = rate;
            SpookModeEnable = true;

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

        [Command("DeleteSpook"), Alias("ds"), Summary("Deletes a spook.\n**Usage**: `!ds [no.]`"), OwnerPrecondition]
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

        [Command("ToggleChristmasMode"), Alias("tcm"), Summary("Enables chistmas mode, 0 to disable.\n**Usage**: `!tcm [chance per message 1/n]`"), OwnerPrecondition]
        public async Task ToggleChristmasMode(int rate, [Remainder] string sts = "")
        {
            if (rate == 0)
            {
                ChristmasModeEnable = false;
                await Context.Channel.SendMessageAsync("Christmas Mode disabled... :c");
                return;
            }

            ChristmasModeRate = rate;
            ChristmasModeEnable = true;

            await Context.Channel.SendMessageAsync($"Christmas Rate enabled at rate {rate}. *Yaayy.*");
        }

        public async Task Spook(SocketCommandContext context)
        {
            if (!SpookModeEnable)
                return;

            var rnd = new Random();
            if (rnd.Next(SpookModeRate) != 0)
                return;

            if (Lines == null)
                Lines = XmlHelper.FromXmlFile<List<string>>(XmlPaths.GetSpookLinesPath());

            string spook = Lines[rnd.Next(Lines.Count())];
            await context.Channel.SendMessageAsync(spook);
        }

        public async Task Christmas(SocketCommandContext context)
        {
            if (!ChristmasModeEnable)
                return;

            var rnd = new Random();
            if (rnd.Next(ChristmasModeRate) != 0)
                return;
            
            await context.Message.AddReactionAsync(Emote.Parse(ChristmasEmotes[rnd.Next(ChristmasEmotes.Length)]));
        }
    }
}
