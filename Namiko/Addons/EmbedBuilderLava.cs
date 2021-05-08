using Discord;
using Namiko.Modules.Basic;

namespace Namiko
{
    class EmbedBuilderLava : EmbedBuilder
    {
        public EmbedBuilderLava(IUser author = null) : base()
        {
            WithColor(BasicUtil.RandomColor());
            WithFooter($"Namiko 🌋 Lavalink");
            string title = author != null ? $"{author.Username}#{author.Discriminator}" : "Player";
            WithAuthor(title, author?.GetAvatarUrl(), LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-lavalink"));
        }
    }
}
