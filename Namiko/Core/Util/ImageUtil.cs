using Discord;
using Namiko.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Text;
using static Namiko.Core.Modules.Images;
using System.Linq;

namespace Namiko.Core.Util
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
            return imageName.Contains("post") && !(guildId == 417064769309245471 || guildId == 418900885079588884);
        }
    }
}