using Discord;
using Discord.WebSocket;
using Model;
using System.Collections.Generic;

namespace Namiko
{
    public static class BanrouletteUtil
    {
        public static string BanrouletteDetails(Banroulette banroulette, IRole role = null, int userCount = 0)
        {
            string desc = "";
            desc += $":calendar_spiral: Ban Length: *{banroulette.BanLengthHours} hours.*\n";
            desc += $"<:toastie3:454441133876183060> Toastie Reward Pool: *{banroulette.RewardPool} (you can add more by using the `brrp` command)*\n";

            if (role != null)
                desc += $":star: Required Role: *{role.Name}*\n";

            desc += $":hammer: Participants:  ";
            if (banroulette.MinParticipants > 0)
                desc += $"`Min: {banroulette.MinParticipants}`  ";
            if (banroulette.MaxParticipants > 0)
                desc += $"`Max: {banroulette.MaxParticipants}`  ";
            desc += $"`Current: {userCount}`";

            return desc;
        }

        public static string BanrouletteParticipants(List<IUser> users)
        {
            string desc = "```java\n";
            for (int i = 0; i < users.Count; i++) {
                desc += $"#{i + 1} {users[i]}\n";
            }
            desc += "```";
            return desc;
        }
    }
}
