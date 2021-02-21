using Discord;
using Discord.WebSocket;
using Model;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Namiko.Images;

namespace Namiko
{
    public static class ImageUtil
    {

        public static EmbedBuilder ToEmbed(ReactionImage img)
        {
            EmbedBuilder embed = new EmbedBuilder();
            string path = $"{Config.ImagePath}{img.Name}{(img.GuildId > 0 ? img.GuildId.ToString() : "")}/{img.Id}";
            embed.WithImageUrl(path);
            embed.WithFooter($"{img.Name} id: {img.Id}");
            embed.WithColor(BasicUtil.RandomColor());
            return embed;
        }

        public static EmbedBuilder ListAllEmbed(List<ImageCount> images, string prefix = "!", IUser author = null)
        {
            var eb = new EmbedBuilderPrepared(author);
            eb.WithDescription($"<:Awooo:582888496793124866> `{prefix}pat` - use reaction images!\n" +
                $"<:NadeYay:564880253382819860> `{prefix}album pat` - link to an imgur album with all the images!\n" +
                $"<:KannaHype:571690048001671238> `{prefix}i 20` - use a specific reaction image by id!");
            
            if (images.Any(x => x.Count >= 20))
            {
                string list = "";
                foreach (var x in images.Where(x => x.Count >= 20))
                {
                    list += "`" + x.ToString() + "` ";
                }
                eb.AddField("20+ images", list);
            }

            if (images.Any(x => x.Count >= 10 && x.Count < 20))
            {
                string list = "";
                foreach (var x in images.Where(x => x.Count >= 10 && x.Count < 20))
                {
                    list += "`" + x.ToString() + "` ";
                }
                eb.AddField("10+ images", list);
            }

            if (images.Any(x => x.Count >= 5 && x.Count < 10))
            {
                string list = "";
                foreach (var x in images.Where(x => x.Count >= 5 && x.Count < 10))
                {
                    list += "`" + x.ToString() + "` ";
                }
                eb.AddField("5+ images", list);
            }

            if (images.Any(x => x.Count > 0 && x.Count < 5))
            {
                string list = "";
                foreach (var x in images.Where(x => x.Count >= 1 && x.Count < 5))
                {
                    list += "`" + x.ToString() + "` ";
                }
                eb.AddField("<5 images", list);
            }

            return eb;
        }
        public static EmbedBuilder AddGuildImagesToEmbed(EmbedBuilder eb, IEnumerable<string> imageNames)
        {
            string list = "";
            if (imageNames.Count() > 0)
            {
                foreach (var x in imageNames)
                {
                    list += "`" + x + "` ";
                }
            }
            else
                list += "~ Add Server exclusive reaction images using the `ni` command. Requires Pro Guild+ ~";

            eb.AddField("Server images", list);
            
            return eb;
        }


        //Image handlers
        public static async Task DownloadImageToServer(ReactionImage img, ISocketMessageChannel ch)
        {
            try
            {
                if (img.Url == null || img.Url == "")
                    return;

                using WebClient client = new WebClient();

                string path = $"{Config.ImagePath}{img.Name}{(img.GuildId > 0 ? img.GuildId.ToString() : "")}/{img.Id}";
                await client.DownloadFileTaskAsync(img.Url, path);
            }
            catch (Exception ex)
            {
                await ch.SendMessageAsync($"{Program.GetClient().GetUser(Config.OwnerId).Mention} Error while downloading image to server.");
                SentrySdk.WithScope(scope =>
                {
                    scope.SetExtras(img.GetProperties());
                    SentrySdk.CaptureException(ex);
                });
            }
        }
    }
}