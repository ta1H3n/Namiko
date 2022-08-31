using System.Web;

namespace Namiko.Modules.Basic
{
    public static class LinkHelper
    {
        public const string RedirectUrl = "https://namiko.moe/redirect";
        public const string Patreon = "https://www.patreon.com/taiHen";
        public const string Pro = "https://namiko.moe/Pro";
        public const string SupportServerInvite = "https://discord.gg/W6Ru5sM";
        public const string Guide = "https://namiko.moe/Guide";
        public const string Commands = "https://namiko.moe/Commands";
        public const string AmfwtServerInvite = "https://discord.gg/jQUgQb4f6c";
        public const string BotInvite = "https://discordapp.com/oauth2/authorize?client_id=418823684459855882&scope=bot&permissions=268707844";
        public const string Repository = "https://github.com/ta1H3n/Namiko";
        public const string Vote = "https://discordbots.org/bot/418823684459855882/vote";

        public static string GetRedirectUrl(string url, string type, string tag)
        {
            bool first = true;
            string link = RedirectUrl + 
                AddQueryParam(ref first, "type", type) + 
                AddQueryParam(ref first, "tag", tag) + 
                AddQueryParam(ref first, "redirectUrl", url);
            return link;
        }

        private static string AddQueryParam(ref bool first, string paramName, string param)
        {
            if (param == null || param == "")
            {
                return "";
            }
            else if (first)
            {
                first = false;
                return "?" + paramName + "=" + HttpUtility.UrlEncode(param);
            }
            else
            {
                return "&" + paramName + "=" + HttpUtility.UrlEncode(param);
            }
        }
    }
}
