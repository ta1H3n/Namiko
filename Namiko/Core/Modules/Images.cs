using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Imgur.API.Models;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System.Diagnostics.Contracts;
using Namiko.Resources.Preconditions;
using Namiko.Core.Util;
using Discord.Addons.Interactive;

namespace Namiko.Core.Modules
{
    public class Images : InteractiveBase<ShardedCommandContext>
    {
        public async Task<bool> SendRandomImage(SocketCommandContext Context)
        {
            string text = Context.Message.Content;
            text = text.Replace(Program.GetPrefix(Context), "");
            text = text.Split(' ')[0];
            text = text.ToLower();

            var image = ImageDb.GetRandomImage(text);
            if (image == null || ImageUtil.IsAMFWT(Context.Guild.Id, image.Name))
            {
                return false;
            }
            await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(image).Build());
            return true;
        }

        [Command("All"), Summary("All reaction images from a single command.\n**Usage**: `!all [name]`")]
        public async Task All(string name = null, [Remainder] string str = "")
        {
            if (name == null)
            {
                await Context.Channel.SendMessageAsync("https://namikolove.imgur.com/");
                return;
            }

            var album = ImageDb.GetAlbum(name);
            await Context.Channel.SendMessageAsync(ImgurAPI.ParseAlbumLink(album.AlbumId));
        }

        [Command("Image"), Alias("i"), Summary("Sends a reaction image by id.\n**Usage**: `!i [id]`")]
        public async Task Image(int id, [Remainder] string str = "")
        {
            var image = ImageDb.GetImage(id);
            if (image == null || ImageUtil.IsAMFWT(Context.Guild.Id, image.Name))
            {
                await Context.Channel.SendMessageAsync($"There is no image with id: {id}");
                return;
            }
            var user = Context.Guild.GetUser(Context.User.Id);
            await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(image).Build());
        }

        [Command("List"), Alias("ListAll"), Summary("List of all image commands and how many images there are.\n**Usage**: `listall`")]
        public async Task List([Remainder] string str = "")
        {
            var images = ImageDb.GetImages();
            List<ImageCount> names = new List<ImageCount>();

            foreach (ReactionImage x in images)
            {
                if (!ImageUtil.IsAMFWT(Context.Guild.Id, x.Name))
                {
                    Boolean flag = true;
                    foreach (ImageCount a in names)
                    {
                        if (a.Name.Equals(x.Name))
                        {
                            a.Count++;
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        names.Add(new ImageCount { Name = x.Name, Count = 1 });
                    }
                }
            }

            names = names.OrderBy(x => x.Name).ToList();

            //  string stringList = "```cs\n";
            //  foreach(ImageCount x in names)
            //  {
            //      if(x.Count > 9)
            //         stringList += String.Format("{0,-10} - {1}\n", x.Name, x.Count);
            //      else
            //         stringList += $"{x.Name} ";
            //  }
            //  stringList += "```";

            await Context.Channel.SendMessageAsync(ImageUtil.ListAllBlock(names));
        }

        [Command("NewImage"), Alias("ni"), Summary("Adds a new image to the database.\n**Usage**: `!ni [name] [url]`"), HomePrecondition]
        public async Task NewImage(string name, string url = null, [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();

            url = url ?? Context.Message.Attachments.FirstOrDefault()?.Url;

            if (url == null)
            {
                await Context.Channel.SendMessageAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            url = url.EndsWith(".gifv") ? url.Replace(".gifv", ".gif") : url;
            url = url.EndsWith(".mp4") ? url.Replace(".mp4", ".gif") : url;

            string albumId = null;
            if (!ImageDb.Exists(name))
            {
                albumId = (await ImgurAPI.CreateAlbumAsync(name)).Id;
                await ImageDb.CreateAlbum(name, albumId);
            }
            else albumId = ImageDb.GetAlbum(name).AlbumId;

            var iImage = await ImgurAPI.UploadImageAsync(url, albumId);
            await ImageDb.AddImage(name.ToLower(), iImage.Link);

            //Test
            var image = ImageDb.GetLastImage();
            await ImgurAPI.EditImageAsync(iImage.Id.ToString(), null, image.Id.ToString());
            await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(image).Build());
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
            await ImgurAPI.EditImageAsync(ImgurAPI.ParseId(image.Url), null, image.Id.ToString() + " [DELETED]");
        }

        public class ImageCount
        {
            public string Name { get; set; }
            public int Count { get; set; }

            public override string ToString()
            {
                return $"{Name}";
            }
        }
    }
}
