using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System.Diagnostics.Contracts;
using Namiko.Resources.Attributes;
using Namiko.Core.Util;

namespace Namiko.Core.Modules
{
    public class Images : ModuleBase<SocketCommandContext>
    {
         public async Task SendRandomImage(SocketCommandContext Context)
         {
            string text = Context.Message.Content;
            text = text.Replace(StaticSettings.prefix, "");
            text = text.Split(' ')[0];
            text = text.ToLower();

            var image = ImageDb.GetRandomImage(text);
            if (image == null)
            {
                return;
            }
             await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(image));
         }

         [Command("NewImage"), Alias("ni"), Summary("Adds a new image to the database.\n**Usage**: `!ni [name] [url]`"), HomePrecondition]
         public async Task NewImage(string name, string url, [Remainder] string str = "")
         {

            if (!((url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") || url.EndsWith(".gif") || url.EndsWith(".gifv")) && (Uri.TryCreate(url, UriKind.Absolute, out var outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))))
             {
                 await Context.Channel.SendMessageAsync("URL is invalid. Note: URL has to end with .jpg .jpeg .png .gif or .gifv");
                 return;
             }

             await ImageDb.AddImage(name.ToLower(), url);

             //Test
             await Task.Delay(50);
             var user = Context.Guild.GetUser(Context.User.Id);
             var image = ImageDb.GetLastImage();
             await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(image));
         }

         [Command("DeleteImage"), Alias("di"), Summary("Deletes image from the database using the id.\n**Usage**: `di [id]`"), HomePrecondition]
         public async Task DeleteImage(int id, [Remainder] string str = "")
         {

            var image = ImageDb.GetImage(id);
             if (image == null)
             {
                 await Context.Channel.SendMessageAsync($"There is no image with id: {id}");
                 return;
             }

             await ImageDb.DeleteImage(id);
             await Context.Channel.SendMessageAsync($"Image {id} is gone forever. Why have you done this?");
         }

         [Command("Image"), Alias("i"), Summary("Sends a reaction image by id.\n**Usage**: `!i [id]`")]
         public async Task Image(int id, [Remainder] string str = "")
         {
             var image = ImageDb.GetImage(id);
             if (image == null)
             {
                 await Context.Channel.SendMessageAsync($"There is no image with id: {id}");
                 return;
             }
             var user = Context.Guild.GetUser(Context.User.Id);
             await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(image));
         }

         [Command("All"), Summary("All reaction images from a single command.\n**Usage**: `!all [name]`"), HomePrecondition]
         public async Task All(string name, [Remainder] string str = "")
         {
            var images = ImageDb.GetImages(name);
             foreach (ReactionImage x in images)
             {
                 await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(x));
             }
         }

        [Command("ListAll"), Summary("List of all image commands and how many images there are.\n**Usage**: `listall`")]
         public async Task List([Remainder] string str = "")
         {
             var images = ImageDb.GetImages();
             List<ImageCount> names = new List<ImageCount>();

             foreach(ReactionImage x in images)
             {
                 Boolean flag = true;
                 foreach(ImageCount a in names)
                 {
                     if(a.Name.Equals(x.Name))
                     {
                         a.Count++;
                         flag = false;
                         break;
                     }
                 }
                 if(flag)
                 {
                     names.Add(new ImageCount{ Name = x.Name, Count = 1});
                 }
             }

             names = names.OrderByDescending(x=> x.Count).ToList();

             string stringList = "```";
             foreach(ImageCount x in names)
             {
                 stringList += String.Format("{0,-10} - {1}\n", x.Name, x.Count);
             }
             stringList += "```";
             await Context.Channel.SendMessageAsync(stringList);
         }

        [Command("List"), Summary("List of all image IDs under an image reaction command.\n**Usage**: `list [name]`")]
        public async Task List(string name, [Remainder] string str = "")
        {
            var list = ImageDb.GetImages(name);

            string stringList = "```";
            foreach (ReactionImage x in list)
            {
                stringList += String.Format("{0} - {1}\n", x.Name, x.Id);
            }
            stringList += "```";
            await Context.Channel.SendMessageAsync(stringList);
        }
        

        private class ImageCount
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }
    }
}
