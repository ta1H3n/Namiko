using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace Namiko
{
    class EmbedBuilderLava : EmbedBuilder
    {
        public EmbedBuilderLava(IUser author = null) : base()
        {
            WithColor(BasicUtil.RandomColor());
            WithFooter($"Namiko 🌋 Lavalink");
            string title = author != null ? $"{author.Username}#{author.Discriminator}" : "Player";
            WithAuthor(title, author?.GetAvatarUrl(), BasicUtil._patreon);
        }
    }
}
