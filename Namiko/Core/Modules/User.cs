using System.Threading.Tasks;
using Namiko.Core.Util;
using System.Linq;
using Discord;
using System;
using System.Drawing;
using Discord.Commands;
using Discord.WebSocket;
using System.Globalization;
using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using Namiko.Resources.Attributes;
using System.Collections.Generic;
namespace Namiko.Core.Modules {
    public class User : ModuleBase<SocketCommandContext> {

        [Command("Inventory"), Alias("waifus", "profile"), Summary("Shows user waifus.\n**Usage**: `!inventory [user_optional]`")]
        public async Task Inventory(IUser user = null, [Remainder] string str = "") {
            if (user == null) user = Context.User;
            await Context.Channel.SendMessageAsync("", false, UserUtil.ProfileEmbed(user).Build());
        }

        [Command("setcolour"), Alias("setcolor", "sc"), Summary("Allows user to set profile colour for a smol cost.\n**Usage**: `!sc [shade - optional as dark/light ] [colour name or hex value]`")]
        public async Task CustomColour(string shade = "", string colour = "",[Remainder] string str = "") {

             //
            //way to set it back to default
            shade = shade.ToLower();
            if (shade.Equals("") || shade.Equals("default")) {
                await UserDb.HexDefault(Context.User.Id);                    
                
                //creating comfermation embed
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor("Profile Colour");
                embed.WithDescription($"{ Context.User.Username } set colour to **Default**\nSwitching to Dafult incurs no additional costs");
                embed.WithColor(BasicUtil.RandomColor());

                //sending embed + exception & error confermations 
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;


            //if no shade specified but colour value exists
            } if (colour.Equals("")) colour = shade;
            else colour = colour.ToLower();



             //
            //setting start values
            System.Drawing.Color color;

            //checking for possible name or hex value, 
            if (colour.StartsWith('#')) colour = colour.Remove(0, 1);
            if (UserUtil.GetNamedColour(ref color, colour, shade) || UserUtil.GetHexColour(ref color, colour)) {

                //toastie + saving hex color try
                try { await ToastieDb.AddToasties(Context.User.Id, -Cost.colour);
                    await UserDb.SetHex(color, Context.User.Id);

                    //creating comfermation embed
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithAuthor("Profile Colour");
                    embed.WithDescription($"{ Context.User.Username } changed primary colour!");
                    embed.WithColor((Discord.Color)color); 

                    //sending embed + exception & error confermations 
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                } catch (Exception ex) {await Context.Channel.SendMessageAsync(ex.Message); }
            } 
        }

        [Command("setquote"), Alias("sq"), Summary("Allows user to set a personal quote.\n**Usage**: `!sq [quote]`")]
        public async Task CustomQuote([Remainder] string quote) {

            //setting quote + re-getting quote
            await UserDb.SetQuote(Context.User.Id, quote);

            //getting embed
            quote = UserDb.GetQuote(Context.User.Id);
            EmbedBuilder embed = UserUtil.QuoteEmbed(Context.User.Id, quote);
            embed.WithAuthor("Personal Quote Updated");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("quote"), Alias("q"), Summary("Allows user to see their personal quote.\n**Usage**: `!q`")]
        public async Task PersonalQuote([Remainder] string str = "") {
            string quote = UserDb.GetQuote(Context.User.Id);

            //checking quote
            if(quote == null || quote == ""){
                await Context.Channel.SendMessageAsync("You haven't added a quote yet qq");
                return;
            }

            //sending quote
            EmbedBuilder embed = UserUtil.QuoteEmbed(Context.User.Id, quote);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
