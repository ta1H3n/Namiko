﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Namiko.Handlers.Autocomplete;
using Namiko.Handlers.ComplexParameters;

namespace Namiko
{
    [Name("Reaction Images")]
    public class Images : CustomModuleBase<ICustomContext>
    {
        private readonly BaseSocketClient _client;
        public static Dictionary<ulong, HashSet<string>> ReactionImageCommands;

        public Images(BaseSocketClient client)
        {
            _client = client;
            if (ReactionImageCommands == null)
            {
                ReactionImageCommands = ImageDb.GetReactionImageDictionary().Result;
            }
        }

        public async Task<bool> SendRandomImage(ICommandContext Context)
        {
            if (Context.Guild != null && DisabledCommandHandler.IsDisabled("", Context.Guild.Id, DisabledCommandType.Images))
            {
                return false;
            }

            if (!RateLimit.CanExecute(Context.Channel.Id))
            {
                await ReplyAsync($"Woah there, Senpai, calm down! I locked this channel for **{RateLimit.InvokeLockoutPeriod.Seconds}** seconds <:MeguExploded:627470499278094337>\n" +
                                 $"You can only use **{RateLimit.InvokeLimit}** commands per **{RateLimit.InvokeLimitPeriod.Seconds}** seconds per channel.");
                return false;
            }

            string text = Context.Message.Content;
            text = text.Replace(GetPrefix(), "");
            text = text.Split(' ')[0].ToLower();

            ReactionImage image;
            if (Context?.Guild.Id != null && ReactionImageCommands.ContainsKey(Context.Guild.Id) && ReactionImageCommands[Context.Guild.Id].Contains(text))
            {
                image = ImageDb.GetRandomImage(text, Context.Guild.Id);
            }
            else if (ReactionImageCommands[0].Contains(text))
            {
                image = ImageDb.GetRandomImage(text);
            }
            else
            {
                await ReplyAsync("Not an image command", ephemeral: true);
                return false;
            }

            var embed = ImageUtil.ToEmbed(image).Build();
            await ReplyAsync("", false, embed);
            return true;
        }

        [SlashCommand("image", "Send a reaction image")]
        public async Task SendImage([Autocomplete(typeof(ReactionImageAutocomplete))] string name, [ComplexParameter]UserListParams usersListParams = null, int imageId = 0)
        {
            if (imageId != 0)
            {
                Image(imageId);
                return;
            }

            if (name == null)
            {
                await ReplyAsync("Not an image command", ephemeral: true);
                return;
            }
            
            name = name.ToLower();

            ReactionImage image;
            if (Context?.Guild.Id != null && ReactionImageCommands.ContainsKey(Context.Guild.Id) && ReactionImageCommands[Context.Guild.Id].Contains(name))
            {
                image = ImageDb.GetRandomImage(name, Context.Guild.Id);
            }
            else if (ReactionImageCommands[0].Contains(name))
            {
                image = ImageDb.GetRandomImage(name);
            }
            else
            {
                await ReplyAsync("Not an image command", ephemeral: true);
                return;
            }

            string mentions = string.Join(' ', usersListParams.GetUsers().Select(x => x.Mention));
            
            var embed = ImageUtil.ToEmbed(image).Build();
            await ReplyAsync(mentions, false, embed);
        }

        [Command("List"), Alias("ListAll", "Images", "Albums"), Description("List of all image commands and how many images there are.\n**Usage**: `!list`")]
        [SlashCommand("images", "List of image commands")]
        public async Task List([Description("Show all images from a command")] string album = null)
        {
            if (album != null)
            {
                All(album);
                return;
            }
            
            var images = await ImageDb.GetImages();
            List<ImageCount> names = new List<ImageCount>();

            foreach (ReactionImage x in images)
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

            names = names.OrderBy(x => x.Name).ToList();
            var eb = ImageUtil.ListAllEmbed(names, GetPrefix(), Context.User);
            eb = ImageUtil.AddGuildImagesToEmbed(eb, (await ImageDb.GetImages(null, Context.Guild.Id)).Select(x => x.Name).Distinct().OrderBy(x => x));
            await ReplyAsync(embed: eb.Build());
        }

        [Command("Album"), Alias("All"), Description("All reaction images from a single command.\n**Usage**: `!all [image_name]`")]
        public async Task All(string name)
        {
            var album = Context.Guild == null ? null : ImageDb.GetAlbum(name + Context.Guild.Id);
            var album2 = ImageDb.GetAlbum(name);

            string albums = album == null ? "" : $"<{ImgurAPI.ParseAlbumLink(album.AlbumId)}>";
            if (album2 != null)
                albums += $"\n<{ImgurAPI.ParseAlbumLink(album2.AlbumId)}>";

            if (albums == "")
            {
                await ReplyAsync($"Album **{name}** doesn't exist.");
                return;
            }

            await ReplyAsync(albums);
        }

        [Command("Image"), Alias("i"), Description("Sends a reaction image by id.\n**Usage**: `!i [id]`")]
        public async Task Image(int id)
        {
            var image = ImageDb.GetImage(id);
            if (image == null || (image.GuildId != 0 && image.GuildId != Context.Guild.Id))
            {
                await ReplyAsync($"There is no image with id: {id}");
                return;
            }
            var embed = ImageUtil.ToEmbed(image).Build();
            await ReplyAsync("", false, embed);
        }

        [HomeOrT1GuildPrecondition, UserPermission(GuildPermission.ManageMessages)]
        [Command("NewImage"), Alias("ni"), Description("Adds a new image to the database.\n**Usage**: `!ni [name] [url_or_attachment]`")]
        [SlashCommand("image-new", "Add a new image with a url or attachment")]
        public async Task NewImage(string name, string url = null)
        {
            await Context.TriggerTypingAsync();
            bool insider = Context.Guild.Id == 418900885079588884;

            url ??= ((ICommandContext)Context).Message.Attachments.FirstOrDefault()?.Url;

            if (url == null)
            {
                await ReplyAsync("Can't get your attachment, there probably isn't one. *Heh, dummy...*");
                return;
            }

            if (!insider)
            {
                if(!PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
                {
                    await ReplyAsync($"This server does not have Pro Guild+. `{GetPrefix()}pro`");
                    return;
                }

                if ((await ImageDb.GetImages(name, 0)).Any())
                {
                    await ReplyAsync($"There is already a default image command called **{name}**. It will be replaced with your custom one.");
                }
            }

            url = url.EndsWith(".gifv") ? url.Replace(".gifv", ".gif") : url;
            url = url.EndsWith(".mp4") ? url.Replace(".mp4", ".gif") : url;

            if (ImgurAPI.RateLimit.ClientRemaining < 50)
            {
                await ReplyAsync("Not enough imgur credits to upload. Please try again later.");
                return;
            }

            string albumId;
            string albumName = insider ? name : name + Context.Guild.Id;
            if (!ImageDb.AlbumExists(albumName))
            {
                albumId = (await ImgurAPI.CreateAlbumAsync(albumName)).Id;
                await ImageDb.CreateAlbum(albumName, albumId);
            }
            else albumId = ImageDb.GetAlbum(albumName).AlbumId;

            var iImage = await ImgurAPI.UploadImageAsync(url, albumId);
            var img = await ImageDb.AddImage(name.ToLower(), iImage.Link, insider ? 0 : Context.Guild.Id);

            if (insider)
            {
                if (!ReactionImageCommands[0].Contains(name.ToLower()))
                {
                    ReactionImageCommands[0].Add(name.ToLower());
                }
            }
            else
            {
                if (!ReactionImageCommands.ContainsKey(Context.Guild.Id))
                {
                    ReactionImageCommands.Add(Context.Guild.Id, new HashSet<string>());
                }
                if (!ReactionImageCommands[Context.Guild.Id].Contains(name.ToLower()))
                {
                    ReactionImageCommands[Context.Guild.Id].Add(name.ToLower());
                }
            }

            await ImgurAPI.EditImageAsync(iImage.Id.ToString(), null, img.Id.ToString());
            var rl = ImgurAPI.RateLimit;
            await ImageUtil.UploadReactionImage(img, Context.Channel, _client);
            await ReplyAsync($"{rl.ClientRemaining-20}/{rl.ClientLimit} imgur credits remaining.", false, ImageUtil.ToEmbed(img).Build());
        }

        [HomeOrT1GuildPrecondition, UserPermission(GuildPermission.ManageMessages)]
        [Command("DeleteImage"), Alias("di"), Description("Deletes image from the database using the id.\n**Usage**: `di [id]`")]
        [SlashCommand("image-delete", "Delete an image by id")]
        public async Task DeleteImage(int id)
        {
            bool insider = Context.Guild.Id == 418900885079588884;

            var image = ImageDb.GetImage(id);
            if (image == null)
            {
                await ReplyAsync($"There is no image with id: **{id}**");
                return;
            }
            if(!insider && image.GuildId != Context.Guild.Id)
            {
                await ReplyAsync($"There is no image with id **{id}** in your guild.");
                return;
            }

            await ImageDb.DeleteImage(id);
            await ReplyAsync($"Image **{id}** is gone forever. Why have you done this?");
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
