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
                WithAuthor(author.Username + "#" + author.Discriminator, author.GetAvatarUrl(), BasicUtil._patreon);
        }

        public EmbedBuilderPrepared(string desc) : base()
        {
            WithColor(BasicUtil.RandomColor());
            WithDescription(desc);
        }
    }

    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithAuthor(this EmbedBuilder eb, string name, string iconUrl = null, string url = null)
        {
            eb.WithAuthor(name, iconUrl, url ?? BasicUtil._patreon);
            return eb;
        }
        public static EmbedBuilder WithAuthor(this EmbedBuilder eb, IUser author)
        {
            eb.WithAuthor(author.Username + "#" + author.Discriminator, author.GetAvatarUrl(), BasicUtil._patreon);
            return eb;
        }
        public static EmbedBuilder WithAuthor(this EmbedBuilder eb, IGuildUser author)
        {
            eb.WithAuthor(author.Username + "#" + author.Discriminator, author.GetAvatarUrl(), BasicUtil._patreon);
            return eb;
        }


    }
}
