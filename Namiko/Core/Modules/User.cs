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

        [Command("setcolour"), Alias("setcolor", "sc"), Summary("Allows user to set profile colour for a smol cost.\n**Usage**: `!sc [colour name or hex value] [shade, optional]`")]
        public async Task CustomColour(string colour, string shade = "", [Remainder] string str = "") {

            //setting starting values
            shade = shade.ToLower();
            colour = colour.ToLower();
            System.Drawing.Color color;

            //checking for possible name or hex value, 
            if (colour.StartsWith('#')) colour = colour.Remove(0, 1);
            if (UserUtil.GetNamedColour(ref color, colour, shade) || UserUtil.GetHexColour(ref color, colour)) {

                //toastie try
                try { await ToastieDb.AddToasties(Context.User.Id, -Cost.colour);

                    //color.Name --> will need to store hex code in DB .Name *should* do this
                    //method for setting colour in DB

                    

                    //creating comfermation embed
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithAuthor("Profile Colour");
                    embed.WithDescription($"{ Context.User.Username } changed primary colour!");
                    embed.WithColor((Discord.Color)color); 

                    //sending embed + exception & error confermations 
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                } catch (Exception ex) {await Context.Channel.SendMessageAsync(ex.Message); } return;
            } await Context.Channel.SendMessageAsync("\"" + colour + "\" is an invalid colour");
        }
    }
}
