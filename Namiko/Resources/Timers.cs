﻿using Namiko.Data;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList.Api.Objects;
using Reddit.Controllers;

namespace Namiko
{
    public static class Timers
    {
        private static Timer Minute;
        private static Timer Minute5;
        private static Timer Hour;

        public static void SetUp()
        {
            Minute = new Timer(1000 * 60);
            Minute.AutoReset = true;
            Minute.Enabled = true;
            Minute.Elapsed += Timer_TimeoutBlackjack;

            Minute5 = new Timer(1000 * 60 * 5);
            Minute5.AutoReset = true;
            Minute5.Enabled = true;

            Hour = new Timer(1000 * 60 * 60);
            Hour.AutoReset = true;
            Hour.Enabled = true;
            Hour.Elapsed += Timer_ExpireTeamInvites;
            Hour.Elapsed += Timer_NamikoSteal;
        }

        public static void SetUpRelease()
        {
            SetUp();

            Minute.Elapsed += Timer_HourlyStats;
            Minute.Elapsed += Timer_Voters2;

            Minute5.Elapsed += Timer_Unban;
            Minute5.Elapsed += Timer_DailyStats;
            Minute5.Elapsed += Timer_RedditPost;
            Minute5.Elapsed += Timer_RemindVote;

            Hour.Elapsed += Timer_BackupData;
            Hour.Elapsed += Timer_CleanData;
            Hour.Elapsed += Timer_UpdateDBLGuildCount;
            Hour.Elapsed += Timer_ExpirePremium;
            Hour.Elapsed += Timer_PlayingStatus;
        }

        private static async void Timer_PlayingStatus(object sender, ElapsedEventArgs e)
        {
            await Program.GetClient().SetActivityAsync(new Game("Chinese Cartoons. Try @Namiko help", ActivityType.Watching));
        }
        private static async void Timer_ExpirePremium(object sender, ElapsedEventArgs e)
        {
            var now = System.DateTime.Now;
            var expired = PremiumDb.GetAllPremiums(now.AddMonths(-1).AddDays(-1));
            var client = Program.GetClient();

            foreach(var premium in expired)
            {
                var ntr = client.GetGuild((ulong)PremiumType.HomeGuildId_NOTAPREMIUMTYPE);
                SocketGuildUser user = null;
                try
                {
                    user = ntr.GetUser(premium.UserId);
                } catch { }

                if (user == null)
                {
                    await PremiumDb.DeletePremium(premium);
                }

                else if (!user.Roles.Any(x => x.Id == (ulong)premium.Type))
                {
                    await PremiumDb.DeletePremium(premium);
                }

                else
                {
                    premium.ClaimDate = now;
                    await PremiumDb.UpdatePremium(premium);
                }
            }
        }
        public static async void Timer_NamikoSteal(object sender, ElapsedEventArgs e)
        {
            if (new Random().Next(5) != 1)
                return;

            using (var db = new SqliteDbContext())
            {
                foreach(ulong id in db.Servers.Select(x => x.GuildId))
                {
                    var nam = db.Toasties.Where(x => x.UserId == Program.GetClient().CurrentUser.Id && x.GuildId == id).FirstOrDefault();

                    if (nam == null)
                        continue;

                    if (nam.Amount > 200000)
                        continue;

                    var sum = db.Toasties.Where(x => x.Amount > 0 && x.GuildId == id).Sum(x => Convert.ToInt64(x.Amount));
                    if (sum / 10 > nam.Amount)
                    {
                        int actualsum = 0;
                        foreach (var user in db.Toasties.Where(x => x.GuildId == id && x.Amount > 100))
                        {
                            int take = user.Amount / 100;
                            actualsum += take;
                            user.Amount -= take;
                            db.Toasties.Update(user);
                        }
                        nam.Amount += actualsum;
                        db.Update(nam);
                    }
                }
                await db.SaveChangesAsync();
            }
        }
        private static async void Timer_CleanData(object sender, ElapsedEventArgs e)
        {
            var servers = ServerDb.GetOld();
            foreach(var x in servers)
            {
                await TeamDb.DeleteByGuild(x.GuildId);
                await DailyDb.DeleteByGuild(x.GuildId);
                await ServerDb.DeleteServer(x.GuildId);
                await WeeklyDb.DeleteByGuild(x.GuildId);
                await ToastieDb.DeleteByGuild(x.GuildId);
                await MarriageDb.DeleteByGuild(x.GuildId);
                await WaifuShopDb.DeleteByGuild(x.GuildId);
                await PublicRoleDb.DeleteByGuild(x.GuildId);
                await FeaturedWaifuDb.DeleteByGuild(x.GuildId);
                await UserInventoryDb.DeleteByGuild(x.GuildId);
                await WaifuWishlistDb.DeleteByGuild(x.GuildId);
                await SpecialChannelDb.DeleteByGuild(x.GuildId);
                Console.WriteLine($"Cleared server {x.GuildId}");
                await Task.Delay(5000);
            }
        }
        private static async void Timer_ExpireTeamInvites(object sender, ElapsedEventArgs e)
        {
            await InviteDb.DeleteOlder(DateTime.Now.AddDays(-1));
        }
        private static async void Timer_TimeoutBlackjack(object sender, ElapsedEventArgs e)
        {
            var games = Blackjack.games;

            foreach (var x in games.Where(x => x.Value.Refresh.AddMinutes(1) < DateTime.Now).ToList())
            {
                await Blackjack.GameTimeout(x.Key, x.Value);
            }
        }
        private static void Timer_BackupData(object sender, ElapsedEventArgs e)
        {
            try
            {
                string backupLocation = Assembly.GetEntryAssembly().Location.Replace(@"Namiko.dll", @"backups/");
                string date = DateTime.Now.ToString("yyyy-MM-dd");

                File.Copy(Locations.SqliteDb + "Database.sqlite", Locations.SqliteDb + "backups/Database" + date + ".sqlite");
                File.Copy(Locations.SpookyLinesXml, Locations.SpookyLinesXml.Replace("SpookyLines.xml", "backups/SpookyLines") + date + ".xml");
                Console.WriteLine("Backups made.");
            }
            catch { }
        }
        private static async void Timer_Unban(object sender, ElapsedEventArgs e)
        {
            var bans = BanDb.GetBans();
            foreach(var x in bans)
            {
                if (x.DateBanEnd.CompareTo(System.DateTime.Now) == -1)
                {
                    Console.WriteLine("Unbanning " + x.UserId);
                    await BanDb.EndBan(x.UserId, x.ServerId);
                    try
                    {
                        await Program.GetClient().GetGuild(x.ServerId).RemoveBanAsync(x.UserId);
                    }
                    catch { }
                }
            }
        }


        // STATS
        public static async void Timer_DailyStats(object sender, ElapsedEventArgs e)
        {
            if (Program.GetClient().CurrentUser.Id != 418823684459855882)
                return;

            var date = System.DateTime.Now.Date;
            bool cool = false;
            List<ServerStat> servers = null;
            List<CommandStat> commands = null;

            using (SqliteStatsDbContext db = new SqliteStatsDbContext())
            {
                var sample = db.ServerStats.LastOrDefault();
                if (sample == null || sample.Date.Date < date)
                {
                    servers = Stats.ParseServerStats();
                    commands = Stats.ParseCommandStats();

                    db.ServerStats.AddRange(servers);
                    db.CommandStats.AddRange(commands);

                    await db.SaveChangesAsync();
                    cool = true;
                }
            }

            if (cool)
            {
                List<UsageStat> usage = new SqliteStatsDbContext().UsageStats.Where(x => x.Date > date.AddDays(-1) && x.Date < date).ToList();
                await UsageReport(servers, commands, usage);
                Stats.CommandCalls.Clear();
                Stats.ServerCommandCalls.Clear();
            }
        }
        public static async void Timer_HourlyStats(object sender, ElapsedEventArgs e)
        {
            var date = System.DateTime.Now;
            bool cool = false;

            using (SqliteStatsDbContext db = new SqliteStatsDbContext())
            {
                var sample = db.UsageStats.LastOrDefault();
                if (sample == null || sample.Date.AddHours(2) < date)
                {
                    db.UsageStats.Add(new UsageStat { Date = date.AddHours(-1).AddMinutes(-date.Minute).AddSeconds(-date.Second), Count = Stats.TotalCalls });

                    await db.SaveChangesAsync();
                    cool = true;
                }
            }

            if (cool)
            {
                Stats.TotalCalls = 0;
            }
        }

        private static async Task UsageReport(List<ServerStat> servers, List<CommandStat> commands, List<UsageStat> usage)
        {
            servers = servers.OrderByDescending(x => x.Count).ToList();
            commands = commands.OrderByDescending(x => x.Count).ToList();
            string small = SmallReport(servers, commands, usage);
            string big = BigReport(servers, commands, usage).Replace("*", "").Replace("`", "");

            using (var stream = GenerateStreamFromString(big)) {
                await ((SocketTextChannel)Program.GetClient().GetChannel(550619142105989131)).SendFileAsync(stream, System.DateTime.Now.AddDays(-1).Date.ToString("yyyy-MM-dd") + "_Namiko.txt", small);
            }
        }
        private static string SmallReport(List<ServerStat> servers, List<CommandStat> commands, List<UsageStat> usage)
        {
            string text = System.DateTime.Now.Date.ToString() + "\n\n";
            usage = usage.OrderByDescending(x => x.Count).ToList();

            text += "   Servers most used in:\n";
            for(int i = 0; i<3; i++)
            {
                try
                {
                    text += ToString(servers[i]) + "\n";
                } catch { break; }
            }
            
            text += "\n   Most used commands:\n";
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    text += ToString(commands[i]) + "\n";
                } catch { break; }
            }

            text += "\n   Peak usage:\n";
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    text += ToString(usage[i]) + "\n";
                } catch { break; }
            }

            text += $"\n Total command calls: **{usage.Sum(x => x.Count)}**";

            return text;
        }
        private static string BigReport(List<ServerStat> servers, List<CommandStat> commands, List<UsageStat> usage)
        {
            string text = System.DateTime.Now.Date.ToString() + "\n\n";

            text += "   Servers usage:\n";
            foreach(var x in servers)
            {
                text += ToString(x) + "\n";
            }

            text += "\n   Command usage:\n";
            foreach (var x in commands)
            {
                text += ToString(x) + "\n";
            }

            text += "\n   Command usage:\n";
            foreach (var x in usage)
            {
                text += ToString(x) + "\n";
            }

            return text;
        }

        private static string ToString(ServerStat serverStat)
        {
            string guildName = "";
            try
            {
                guildName = Program.GetClient().GetGuild(serverStat.GuildId).Name;
            } catch { }

            return $"`{serverStat.GuildId}` - **{serverStat.Count}** - *{guildName}*";
        }
        private static string ToString(CommandStat commandStat)
        {
            return $"`{commandStat.Name}` - **{commandStat.Count}**";
        }
        private static string ToString(UsageStat usageStat)
        {
            return $"`{usageStat.Date.ToString("hhtt")}` - **{usageStat.Count}**";
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        // DISCORBBOTLIST
        private static bool VoteLock = false;
        private static bool ReminderLock = false;
        public static void Timer_UpdateDBLGuildCount(object sender, ElapsedEventArgs e)
        {
            int amount = 0;
            amount = Program.GetClient().Guilds.Count;
            WebUtil.UpdateGuildCount(amount);
        }
        public static async void Timer_Voters2(object sender, ElapsedEventArgs e)
        {
            if (VoteLock)
                return;

            try
            {
                VoteLock = true;
                IList<IDblEntity> voters = null;
                try
                {
                    voters = await WebUtil.GetVotersAsync();
                }
                catch { return; }
                var old = VoteDb.GetVoters(500);
                var votersParsed = voters.Select(x => x.Id).ToList();
                votersParsed.Reverse();

                List<ulong> add = NewEntries(old.Select(x => x.UserId).ToList(), votersParsed);

                if (add.Count > 500)
                {
                    var ch = await Program.GetClient().GetUser(StaticSettings.owner).GetOrCreateDMChannelAsync();
                    await ch.SendMessageAsync($"Found {add.Count} new voters.");
                    return;
                }

                await VoteDb.AddVoters(add);
                await SendRewards(add);
            }
            catch { }
            finally
            {
                VoteLock = false;
            }
        }
        public static List<T> NewEntries<T>(List<T> oldList, List<T> newList, Func<T, T, bool> equal = null)
        {
            equal = equal ?? delegate (T x, T y) { return x.Equals(y); };
            List<T> list = new List<T>();

            bool done = false;
            while (!done)
            {
                done = true;
                int diff = newList.Count - oldList.Count;
                for (int i = newList.Count-1; i >= 0 && (i - diff) >= 0; i--)
                {
                    T x = newList[i];
                    T y = oldList[i - diff];
                    if (!equal(x, y))
                    {
                        int j = newList.Count - 1;
                        list.Add(newList[j]);
                        newList.RemoveAt(j);
                        done = false;
                        break;
                    }
                }
            }

            list.Reverse();
            return list;
        }
        public static async Task SendRewards(List<ulong> voters)
        {
            foreach(var x in voters)
            {
                try
                {
                    var type = LootBoxType.Vote;
                    if (PremiumDb.IsPremium(x, PremiumType.Toastie))
                        type = LootBoxType.Premium;
                    
                    await LootBoxDb.AddLootbox(x, type, 1);
                    var user = Program.GetClient().GetUser(x);
                    var ch = await user.GetOrCreateDMChannelAsync();
                    await ch.SendMessageAsync(embed: new EmbedBuilderPrepared(user)
                         .WithDescription($"Thank you for voting for me! I have given you a **{type.ToString()} Lootbox**! :star:\n" +
                         $"You can open it in a server of your choice by typing `!open`")
                         .Build());
                }
                catch { }
            }
        }
        public static async Task SendReminders(List<ulong> voters)
        {
            foreach (var x in voters)
            {
                try
                {
                    var user = Program.GetClient().GetUser(x);
                    var ch = await user.GetOrCreateDMChannelAsync();
                    await ch.SendMessageAsync(embed: new EmbedBuilderPrepared(user)
                        .WithDescription("You can now vote for me again and receive another lootbox! [Discord Bots](https://discordbots.org/bot/418823684459855882/vote)")
                        .Build());
                }
                catch { }
            }
        }
        public static async void Timer_RemindVote(object sender, ElapsedEventArgs e) 
        {
            if (ReminderLock)
                return;

            try
            {
                ReminderLock = true;
                var paramL = ParamDb.GetParam(0, "VoteReminder");
                var param = paramL?.FirstOrDefault() ?? new Param { Name = "VoteReminder", Date = System.DateTime.Now };

                var dateTo = System.DateTime.Now;
                dateTo = dateTo.AddHours(-12);
                var votes = VoteDb.GetVoters(param.Date, dateTo);

                param.Date = dateTo;
                await ParamDb.UpdateParam(param);
                await SendReminders(votes.Select(x => x.UserId).ToList());
            }
            catch { }
            finally
            {
                ReminderLock = false;
            }
        }


        // REDDIT POST
        private static bool RedditLock = false;
        private static async void Timer_RedditPost(object sender, ElapsedEventArgs e)
        {
            if (RedditLock)
                return;

            try
            {
                RedditLock = true;
                var ids = SpecialChannelDb.GetChannelsByType(ChannelType.Reddit);
                var channels = await GetChannels(ids);
                var grouped = channels.GroupBy(x => x.Subreddit);

                foreach (var sub in grouped)
                {
                    await Post(sub);
                    await Task.Delay(5000);
                }
            }
            catch { }
            finally
            {
                RedditLock = false;
            }
        }
        public static async Task Post(IGrouping<string, RedditChannel> sub)
        {
            var hot = await RedditAPI.GetHot(sub.Key);

            foreach(var post in hot)
            {
                var dbUpvotes = RedditDb.GetUpvotes(post.Permalink);
                var channels = sub.Where(ch => ch.Upvotes < post.UpVotes && ch.Upvotes > dbUpvotes && !(post.NSFW && !ch.Channel.IsNsfw));
                if (!channels.Any())
                    continue;

                await RedditDb.AddPost(post.Permalink, post.UpVotes);
                var eb = RedditPostEmbed(post, sub.Key);
                if (eb == null)
                    continue;
                var embed = eb.Build();

                foreach (var ch in channels)
                {
                    try
                    {
                        var msg = await ch.Channel.SendMessageAsync(embed: embed);
                        _ = Task.Run(async () =>
                        {
                            await msg.AddReactionAsync(Emote.Parse("<:SignUpvote:577919849250947072>"));
                            await msg.AddReactionAsync(Emote.Parse("<:SignDownvote:577919848554823680>"));
                        });
                    }
                    catch { }
                }

                return;
            }
        }
        public static EmbedBuilder RedditPostEmbed(Post post, string sub)
        {
            var eb = new EmbedBuilder()
                        .WithColor(BasicUtil.RandomColor())
                        .WithAuthor(post.Title, "https://i.imgur.com/GthCice.png", "https://www.reddit.com" + post.Permalink)
                        .WithFooter("r/" + sub + " - " + post.UpVotes + " upvotes");
            try
            {
                eb.WithDescription(((SelfPost)post).SelfText);
            }
            catch { }
            try
            {
                eb.WithImageUrl(((LinkPost)post).URL);
            }
            catch { }
            try
            {
                if (eb.Description == null && post.Comments.Top[0].UpVotes > 40)
                    eb.WithDescription(post.Comments.Top[0].Body);
            }
            catch { }

            if (eb.Description == null && eb.ImageUrl == null)
                return null;

            return eb;
        }
        public static async Task<List<RedditChannel>> GetChannels(IEnumerable<SpecialChannel> ids)
        {
            var client = Program.GetClient();
            var channels = new List<RedditChannel>();
            await Task.Run(() =>
            {
                foreach (var x in ids)
                {
                    try
                    {
                        var ch = client.GetChannel(x.ChannelId);
                        if (ch.GetType() == typeof(SocketTextChannel))
                            channels.Add(new RedditChannel
                            {
                                Channel = (SocketTextChannel)ch,
                                Subreddit = x.Args.Split(',')[0],
                                Upvotes = Int32.Parse(x.Args.Split(',')[1])
                            });
                    }
                    catch { }
                }
            });
            return channels;
        }


        // RANDOM
        public static async Task<List<SocketTextChannel>> GetChannels(IEnumerable<ulong> ids)
        {
            var client = Program.GetClient();
            var channels = new List<SocketTextChannel>();
            await Task.Run(() =>
            {
                foreach (var id in ids)
                {
                    try
                    {
                        var ch = client.GetChannel(id);
                        if (ch.GetType() == typeof(SocketTextChannel))
                            channels.Add((SocketTextChannel)ch);
                    }
                    catch { }
                }
            });
            return channels;
        }
    }


    public class RedditChannel
    {
        public string Subreddit { get; set; }
        public SocketTextChannel Channel { get; set; }
        public int Upvotes { get; set; }
    }
}