using Discord.WebSocket;
using Namiko.Resources.Datatypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko.Core.Util
{
    public static class WelcomeUtil
    {
        public static string GetWelcomeMessageString(SocketUser user)
        {
            string message = WelcomeMessageDb.GetRandomMessage();
            message = message.Replace("@_", user.Mention);
            return message;
        }
    }
}
