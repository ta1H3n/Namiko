using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Namiko.Resources.Database;
using Namiko.Resources.Datatypes;
using System.Diagnostics.Contracts;

namespace Namiko.Core.Util
{
    static class BasicUtil
    {
        public static Color RandomColor()
        {
            List<Color> colors = new List<Color>();
            colors.Add(new Color(250, 202, 48));
            colors.Add(new Color(255, 165, 229));
            colors.Add(new Color(112, 255, 65));
            colors.Add(new Color(90, 161, 255));
            colors.Add(new Color(255, 69, 0));
            colors.Add(new Color(123, 104, 238));
            return colors.ElementAt(new Random().Next(colors.Count()));
        }
        public static Color GetColor(SocketGuildUser user)
        {
            var roles = user.Guild.Roles;
            for (int i = 0; i < roles.Count; i++)
            {
                if (user.Roles.Contains(roles.ElementAt(i)))
                {
                    Color color = roles.ElementAt(i).Color;
                    if (color.ToString() != "#0")
                        return color;
                }
            }
            return new Color(255, 255, 255);
        }
        public static List<SocketUser> UserList(DiscordSocketClient client, List<ulong> ids)
        {
            var users = new List<SocketUser>();
            foreach(var x in ids)
            {
                var user = client.GetUser(x);
                if (user != null)
                    users.Add(user);
            }
            return users;
        }
    }
}
