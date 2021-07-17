using Discord;
using Discord.WebSocket;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Namiko
{
    class BanroyaleUtil
    {
        public static List<Emote> Emotes;

        public static EmbedBuilder BanroyaleDetailsEmbed(Banroyale banroyale, SocketRole role, SocketRole reqRole, int userCount = 0)
        {
            string desc = "";
            if (banroyale.BanLengthHours > 0)
                desc += $":calendar_spiral: Ban Length: *{banroyale.BanLengthHours} hours.*\n";
            if (banroyale.Kick)
                desc += $":hiking_boot: Losers kicked.\n";
            if (banroyale.RewardPool > 0)
                desc += $"<:toastie3:454441133876183060> Reward Pool: **{banroyale.RewardPool}**\n";
            if (banroyale.RoleReqId != 0)
                desc += $":star: Required Role: **{reqRole.Mention}**\n";
            desc += $":star: Participant Role: **{role.Mention}**\n";

            desc += $":hammer: Participants:  ";
            if (banroyale.MinParticipants > 0)
                desc += $"`Min: {banroyale.MinParticipants}`  ";
            if (banroyale.MaxParticipants > 0)
                desc += $"`Max: {banroyale.MaxParticipants}`  ";
            desc += $"`Current: {userCount}`\n";
            desc += $":star2: Number of winners: **{banroyale.WinnerAmount}**\n";

            desc += $"\n:timer: Message frequency: **{banroyale.MinFrequency} - {banroyale.MaxFrequency} seconds**";

            var eb = new EmbedBuilderPrepared(desc)
                .WithTitle("Ban Royale");
            return eb;
        }

        public static string BanroyaleParticipants(IList<string> usernames)
        {
            string desc = "```java\n";
            for (int i = 0; i < usernames.Count; i++)
            {
                desc += $"#{i + 1} {usernames[i]}\n";
            }
            desc += "```";
            return desc;
        }

        public static List<Emote> DrawEmotes(int amount)
        {
            if (Emotes == null)
            {
                Emotes = new List<Emote> {
                    Emote.Parse("<:pat4:435886736225337344>"),
                    Emote.Parse("<:NadeYay:564880253382819860>"),
                    Emote.Parse("<:MeguExploded:627470499278094337>"),
                    Emote.Parse("<:KannaWant:419214048064831508>"),
                    Emote.Parse("<:KaeriThumbsUp:582902255884173315>"),
                    Emote.Parse("<:pat:431601376796213278>"),
                    Emote.Parse("<:toastie3:454441133876183060>"),
                    Emote.Parse("<:Awooo:582888496793124866>"),
                    Emote.Parse("<:MiaHug:536580304018735135>"),
                    Emote.Parse("<:NekoHi:620711213826834443>"),
                    Emote.Parse("<:NamikoSmug:806967747673980938>"),
                    Emote.Parse("<:NamikoSip:804435685834096681>"),
                    Emote.Parse("<:NamikoYeah:804702236482994196>"),
                    Emote.Parse("<:NamikoTickNo:806967748189356052>"),
                    Emote.Parse("<:NamikoTickYes:806967747951067177>"),
                };
            }

            if (Emotes.Count < amount)
                amount = Emotes.Count;

            var rnd = new Random();
            return Emotes.OrderBy(x => rnd.Next()).Take(amount).ToList();
        }
    }
}
