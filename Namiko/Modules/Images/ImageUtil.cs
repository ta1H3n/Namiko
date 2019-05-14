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
        public static readonly string[] localImages = { "post", "propaganda", "servermeme", "cabbage", "darkpink", "succ", "manko", "porpaganda" };

        public static EmbedBuilder ToEmbed(ReactionImage image)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithImageUrl(image.Url);
            embed.WithFooter($"{image.Name} id: {image.Id}");
            embed.WithColor(BasicUtil.RandomColor());
            return embed;
        }

        public static string ListAllBlock(List<ImageCount> images)
        {
            string block = "";
            if (images.Count > 0)
                block += "```cs\n";

            if (images.Any(x => x.Count >= 20))
            {
                block += "20+ images:\n";
                foreach (var x in images.Where(x => x.Count >= 20))
                {
                    block += x.ToString() + " ";
                }
                block += "\n\n";
            }

            if (images.Any(x => x.Count >= 10 && x.Count < 20))
            {
                block += "10+ images:\n";
                foreach (var x in images.Where(x => x.Count >= 10 && x.Count < 20))
                {
                    block += x.ToString() + " ";
                }
                block += "\n\n";
            }

            if (images.Any(x => x.Count >= 5 && x.Count < 10))
            {
                block += "5+ images:\n";
                foreach (var x in images.Where(x => x.Count >= 5 && x.Count < 10))
                {
                    block += x.ToString() + " ";
                }
                block += "\n\n";
            }

            if (images.Any(x => x.Count >= 1 && x.Count < 5))
            {
                block += "<5 images:\n";
                foreach (var x in images.Where(x => x.Count >= 1 && x.Count < 5))
                {
                    block += x.ToString() + " ";
                }
                block += "\n\n";
            }

            return block + "```";
        }
        public static bool IsAMFWT(ulong guildId, string imageName)
        {
            return IsLocalImage(imageName) && !(guildId == 417064769309245471 || guildId == 418900885079588884);
        }
        public static bool IsLocalImage(string name)
        {
            return localImages.Any(x => name.Contains(x));
        }
    }
}