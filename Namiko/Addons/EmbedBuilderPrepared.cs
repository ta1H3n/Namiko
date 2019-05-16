using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace Namiko
{
    public class EmbedBuilderPrepared : EmbedBuilder
    {
        public EmbedBuilderPrepared(IUser author = null) : base()
        {
            WithColor(BasicUtil.RandomColor());
            if (author != null)
                WithAuthor(author.Username, author.GetAvatarUrl(), BasicUtil._patreon);

            var eb = new EmbedBuilder();
        }
    }
}
