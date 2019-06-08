﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Imgur.API.Models;



using System.Diagnostics.Contracts;


using Discord.Addons.Interactive;

namespace Namiko
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
                image = ImageDb.GetRandomImage(text, Context.Guild.Id);
                if(image == null)
                    return false;
            }
            await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(image).Build());
            return true;
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
            var eb = ImageUtil.ListAllEmbed(names, Program.GetPrefix(Context), Context.User);
            eb = ImageUtil.AddGuildImagesToEmbed(eb, ImageDb.GetImages(null, Context.Guild.Id).Select(x => x.Name).Distinct().OrderBy(x => x));
            await Context.Channel.SendMessageAsync(embed: eb.Build());
        }

        [Command("Album"), Alias("All"), Summary("All reaction images from a single command.\n**Usage**: `!all [image_name]`")]
        public async Task All(string name, [Remainder] string str = "")
        {
            var album = ImageDb.GetAlbum(name);
            if(album == null)
                album = ImageDb.GetAlbum(name + Context.Guild.Id);

            if(album == null)
                await Context.Channel.SendMessageAsync($"Album **{name}** doesn't exist.");

            await Context.Channel.SendMessageAsync($"<{ImgurAPI.ParseAlbumLink(album.AlbumId)}>");
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

        [Command("NewImage"), Alias("ni"), Summary("Adds a new image to the database.\n**Usage**: `!ni [name] [url_or_attachment]`"), HomeOrT1GuildPrecondition, CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task NewImage(string name, string url = null, [Remainder] string str = "")
        {
            await Context.Channel.TriggerTypingAsync();
            bool insider = Context.Guild.Id == 418900885079588884;

            url = url ?? Context.Message.Attachments.FirstOrDefault()?.Url;

            if (!insider && ImageDb.AlbumExists(name))
            {
                await Context.Channel.SendMessageAsync("There is already a default image command called **{name}**");
                return;
            }

            if (url == null)
            {
                await Context.Channel.SendMessageAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            url = url.EndsWith(".gifv") ? url.Replace(".gifv", ".gif") : url;
            url = url.EndsWith(".mp4") ? url.Replace(".mp4", ".gif") : url;

            string albumId = null;
            string albumName = insider ? name : name + Context.Guild.Id;
            if (!ImageDb.AlbumExists(albumName))
            {
                albumId = (await ImgurAPI.CreateAlbumAsync(albumName)).Id;
                await ImageDb.CreateAlbum(albumName, albumId);
            }
            else albumId = ImageDb.GetAlbum(albumName).AlbumId;

            var iImage = await ImgurAPI.UploadImageAsync(url, albumId);
            await ImageDb.AddImage(name.ToLower(), iImage.Link);

            //Test
            var image = ImageDb.GetLastImage();
            await ImgurAPI.EditImageAsync(iImage.Id.ToString(), null, image.Id.ToString());
            await Context.Channel.SendMessageAsync("", false, ImageUtil.ToEmbed(image).Build());
        }

        [Command("DeleteImage"), Alias("di"), Summary("Deletes image from the database using the id.\n**Usage**: `di [id]`"), HomeOrT1GuildPrecondition, CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteImage(int id, [Remainder] string str = "")
        {
            bool insider = Context.Guild.Id == 418900885079588884;

            var image = ImageDb.GetImage(id);
            if (image == null)
            {
                await Context.Channel.SendMessageAsync($"There is no image with id: **{id}**");
                return;
            }
            if(!insider && image.GuildId != Context.Guild.Id)
            {
                await Context.Channel.SendMessageAsync($"There is no image with id **{id}** in your guild.");
                return;
            }

            await ImageDb.DeleteImage(id);
            await Context.Channel.SendMessageAsync($"Image **{id}** is gone forever. Why have you done this?");
            await ImgurAPI.EditImageAsync(ImgurAPI.ParseId(image.Url), null, image.Id.ToString() + " [DELETED]");
        }

        public class ImageCount
        {
            public string Name { get; set; }
            public int Count { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}