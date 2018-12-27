using Discord;
using Namiko.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
