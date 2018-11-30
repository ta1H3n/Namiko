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

namespace Namiko.Core.Banroulettes
{
    public static class BanrouletteUtil
    {
        public static string BanrouletteDetails(Banroulette banroulette, SocketRole role = null)
        {
            string desc = "";
            desc += $"**Ban Length**: {banroulette.BanLengthHours} hours.\n";
            desc += $"**Toastie Reward Pool**: {banroulette.RewardPool} *(you can add more by using the !brrewardpool command)*\n";
            desc += $"**Minimum Participants**: {banroulette.MinParticipants}\n";
            desc += $"**Maximum Participants**: {banroulette.MaxParticipants}\n";
            if(role != null)
            {
                desc += $"**Required Role**: {role.Name}";
            }
            return desc;
        }

        public static string BanrouletteParticipants(List<SocketUser> users)
        {
            string desc = "";
            for (int i = 0; i < users.Count; i++) {
                desc += $"#{i + 1} {users[i].Username}\n";
            }
            return desc;
        }
    }
}
