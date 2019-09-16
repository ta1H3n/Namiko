using Discord;

using System;
using System.Collections.Generic;
using System.Text;
using static Namiko.Images;
using System.Linq;

namespace Namiko
{
    public static class ImageUtil
    {

        public static EmbedBuilder ToEmbed(ReactionImage image)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithImageUrl(image.Url);
            embed.WithFooter($"{image.Name} id: {image.Id}");
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
                list += "~ Add Server exclusive reaction images using the `ni` command. Requires Server premium. ~";

            eb.AddField("Server images", list);
            
            return eb;
        }
    }
}