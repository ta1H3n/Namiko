using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Namiko
{
    public class BanroyaleGame
    {
        public static Dictionary<ulong, BanroyaleGame> BanroyaleMessageDict = new Dictionary<ulong, BanroyaleGame>();
        public Timer Timer;
        public Banroyale Banroyale;
        public RestUserMessage Message;
        public Emote Emote;
        public ISocketMessageChannel Channel;
        public SocketRole Role;
        public int KickLast = 0;
        public bool Waiting;

        public BanroyaleGame(Banroyale banroyale, ISocketMessageChannel channel, SocketRole role)
        {
            Banroyale = banroyale;
            Channel = channel;
            Role = role;
            Waiting = false;

            double interval = new Random().Next(Banroyale.MaxFrequency - Banroyale.MinFrequency) + Banroyale.MinFrequency;
            Timer = new Timer(interval * 1000);
            Timer.Elapsed += Timer_SendNextMessageEvent;
            Timer.AutoReset = false;
            Timer.Start();
        }

        private async void Timer_SendNextMessageEvent(object sender, ElapsedEventArgs e)
        {
            if (Waiting)
            {
                if (KickLast > 0)
                    BanroyaleMessageDict.Remove(this.Message.Id);

                var usersReac = (await Message.GetReactionUsersAsync(Emote, 100).FlattenAsync()).Select(x => x.Id).ToHashSet();
                var users = Role.Members.Where(x => !usersReac.Contains(x.Id)).ToList();

                BanroyaleMessageDict.Remove(this.Message.Id);
                Waiting = false;

                if (await LostUsers(this, users))
                    return;
            }

            var rnd = new Random();
            int x = rnd.Next(3);
            KickLast = Role.Members.Count();
            if (KickLast > 20)
                KickLast = KickLast / 10 + x;
            else if (KickLast > 5)
                KickLast = x;
            else
                KickLast = x - 1 >= 0 ? x - 1 : 0;

            int emoteCount = 4;
            var emotes = BanroyaleUtil.DrawEmotes(emoteCount);
            int r = rnd.Next(emoteCount);
            Emote = emotes[r];

            Message = await Channel.SendMessageAsync($"{Role.Mention}", embed: new EmbedBuilderPrepared($"Click the {Emote} reaction to stay in the game!\n" +
                $"{(KickLast > 0 ? $"Last **{KickLast}** participants to react will lose!" : "")}")
                .WithColor(Color.Blue)
                .Build());

            foreach (var emote in emotes)
            {
                _ = Message.AddReactionAsync(emote);
            }

            await BanroyaleMessageDb.AddMessage(new BanroyaleMessage
            {
                Active = true,
                BanroyaleId = Banroyale.Id,
                MessageId = Message.Id,
                EmoteId = Emote.Id
            });

            if (KickLast > 0)
            {
                BanroyaleMessageDict.Add(Message.Id, this);
            }

            Waiting = true;

            double interval = new Random().Next(Banroyale.MaxFrequency - Banroyale.MinFrequency) + Banroyale.MinFrequency;
            Timer.Interval = interval * 1000;
            Timer.Start();
        }

        internal static async Task HandleBanroyaleReactionAsync(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> ch, SocketReaction reaction)
        {
            if (!BanroyaleMessageDict.TryGetValue(reaction.MessageId, out var br))
                return;
            if (br.Emote.Id != ((Emote)reaction.Emote).Id)
                return;

            var usersReac = (await br.Message.GetReactionUsersAsync(br.Emote, 100).FlattenAsync()).Select(x => x.Id).ToHashSet();
            var users = br.Role.Members.Where(x => !usersReac.Contains(x.Id)).ToList();
            if (br.KickLast >= users.Count())
            {
                BanroyaleMessageDict.Remove(br.Message.Id);
                br.Waiting = false;
                await LostUsers(br, users);
            }
        }

        private static async Task<bool> LostUsers(BanroyaleGame br, IEnumerable<SocketGuildUser> users)
        {
            if (users.Count() == 0)
                return false;

            var failed = new List<SocketGuildUser>();
            string desc = "These users lose: ";
            var tasks = new List<Task>();
            foreach (var user in users)
            {
                try
                {
                    if (br.Banroyale.BanLengthHours > 0)
                        tasks.Add(user.BanAsync());
                    else if (br.Banroyale.Kick)
                        tasks.Add(user.KickAsync());
                    else
                        tasks.Add(user.RemoveRoleAsync(br.Role));

                    desc += user.Mention + " ";
                }
                catch
                {
                    failed.Add(user);
                    desc += $"({user.Mention} - failed to kick from the game, user too powerful) ";
                }
            }

            await Task.WhenAll(tasks);
            desc += "\nStill in the game: ";
            var remainingUsers = br.Role.Members.Where(x => !users.Contains(x));
            foreach (var user in remainingUsers)
            {
                desc += user.Mention + " ";
            }
            await br.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(desc).WithTitle("Ban Royale - Round Results")
                .WithColor(Color.DarkRed)
                .Build());
            return await EndBanroyale(br, remainingUsers);
        }

        private static async Task<bool> EndBanroyale(BanroyaleGame br, IEnumerable<SocketGuildUser> users)
        {
            if (users.Count() <= br.Banroyale.WinnerAmount)
            {
                br.Timer.Stop();
                br.Timer.Close();
                await BanroyaleDb.EndBanroyale(br.Banroyale.Id);

                string desc = "Winners: ";
                int split = 0;
                if (br.Role.Members.Count() == 0)
                {
                    desc += "Everyone lost! Well done guys, I'm proud of you.";
                }
                else
                {
                    split = br.Banroyale.RewardPool / users.Count();
                    foreach (var user in users)
                    {
                        _ = BalanceDb.AddToasties(user.Id, split, br.Banroyale.GuildId);
                        _ = user.RemoveRoleAsync(br.Role);
                        desc += user.Mention + " ";
                    }
                }
                if (split > 0)
                    desc += $"\nSplitting **{br.Banroyale.RewardPool}** toasties from the reward pool! ({split} each)";

                await br.Channel.SendMessageAsync(embed: new EmbedBuilderPrepared(desc).WithTitle("Ban Royale Over!")
                    .WithColor(Color.Gold)
                    .Build());
                return true;
            }
            return false;
        }
    }
}
