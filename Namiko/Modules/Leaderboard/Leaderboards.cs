using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Namiko.Modules.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Namiko.Addons.Handlers.Paginator;

namespace Namiko.Modules.Leaderboard
{
    [RequireGuild]
    [Name("Leaderboards")]
    public class Leaderboards : CustomModuleBase<ICustomContext>
    {

        [SlashCommand("leaderboard", "Show various leaderboards")]
        public async Task RepLeaderboard(LeaderboardType type, LeaderboardScope scope = LeaderboardScope.Server,
            [Discord.Interactions.Summary(description: "Filter by waifu name or source in the TopWaifus leaderboard")] string filter = "")
        {
            var res = type switch
            {
                LeaderboardType.Rep when scope == LeaderboardScope.Global => RepLeaderboard(),
                LeaderboardType.Rep when scope == LeaderboardScope.Server => ServerRepLeaderboard(),
                LeaderboardType.Vote when scope == LeaderboardScope.Global => VoteLeaderboard(),
                LeaderboardType.Vote when scope == LeaderboardScope.Server => ServerVoteLeaderboard(),

                LeaderboardType.Toastie => ToastieLeaderboard(),
                LeaderboardType.DailyStreak => DailyLeaderboard(),
                LeaderboardType.WaifuValue => WaifuLeaderboard(),
                
                LeaderboardType.TopWaifus when scope == LeaderboardScope.Global => TopWaifus(filter),
                LeaderboardType.TopWaifus when scope == LeaderboardScope.Server => ServerTopWaifus(filter),
                _ => throw new NotImplementedException()
            };

            await res;
        }
        
        
        [Command("ServerRepLeaderboard"), Alias("srlb"), Description("Highest rep users in this server.\n**Usage**: `!tw`")]
        public async Task ServerRepLeaderboard()
        {
            IEnumerable<KeyValuePair<ulong, int>> rep = await ProfileDb.GetAllRep();
            rep = rep.Where(x => Context.Guild.Users.Select(u => u.Id).Contains(x.Key)).OrderByDescending(x => x.Value);
            var msg = new PaginatedMessage();

            msg.Title = ":star: Rep Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Users Here",
                    Pages = PaginatedMessage.PagesArray(rep, 10, (x) => $"**{BasicUtil.IdToMention(x.Key)}** - {x.Value}\n")
                }
            };
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }

        [Command("RepLeaderboard"), Alias("rlb"), Description("Highest rep users.\n**Usage**: `!tw`")]
        public async Task RepLeaderboard()
        {
            IEnumerable<KeyValuePair<ulong, int>> repRaw = await ProfileDb.GetAllRep();
            List<KeyValuePair<string, int>> rep = new List<KeyValuePair<string, int>>();
            int i = 0;
            foreach (var x in repRaw)
            {
                SocketUser user = Context.Client.GetUser(x.Key);
                if (user == null)
                    continue;

                rep.Add(new KeyValuePair<string, int>(user.Username + "#" + user.Discriminator, x.Value));
            }
            rep = rep.OrderByDescending(x => x.Value).ToList();
            var msg = new PaginatedMessage();

            msg.Title = ":star: Rep Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "All Users",
                    Pages = PaginatedMessage.PagesArray(rep, 10, (x) => $"`#{++i}` {x.Key} - {x.Value}\n", false)
                }
            };
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }

        [Command("ServerVoteLeaderboard"), Alias("svlb"), Description("Highest rep users in this server.\n**Usage**: `!tw`")]
        public async Task ServerVoteLeaderboard()
        {
            IEnumerable<KeyValuePair<ulong, int>> votes = await VoteDb.GetAllVotes();
            votes = votes.Where(x => Context.Guild.Users.Select(u => u.Id).Contains(x.Key)).OrderByDescending(x => x.Value);
            var msg = new PaginatedMessage();

            msg.Title = ":star: Vote Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Users Here",
                    Pages = PaginatedMessage.PagesArray(votes, 10, (x) => $"**{BasicUtil.IdToMention(x.Key)}** - {x.Value}\n")
                }
            };
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }

        [Command("VoteLeaderboard"), Alias("vlb"), Description("Highest rep users.\n**Usage**: `!tw`")]
        public async Task VoteLeaderboard()
        {
            IEnumerable<KeyValuePair<ulong, int>> votesRaw = await VoteDb.GetAllVotes();
            List<KeyValuePair<string, int>> votes = new List<KeyValuePair<string, int>>();
            foreach (var x in votesRaw)
            {
                SocketUser user = Context.Client.GetUser(x.Key);
                if (user == null)
                    continue;

                votes.Add(new KeyValuePair<string, int>(user.Username + "#" + user.Discriminator, x.Value));
            }
            votes = votes.OrderByDescending(x => x.Value).ToList();
            var msg = new PaginatedMessage();

            msg.Title = ":star: Vote Leaderboards";
            var fields = new List<FieldPages>();
            int i = 0;
            fields.Add(new FieldPages
            {
                Title = "All Users",
                Pages = PaginatedMessage.PagesArray(votes, 10, (x) => $"`#{++i}` {x.Key} - {x.Value}\n", false)
            });
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }
        
        

        [Command("ToastieLeaderboard"), Alias("tlb"), Description("Toastie Leaderboard.\n**Usage**: `!tlb`")]
        public async Task ToastieLeaderboard()
        {
            var toasties = await BalanceDb.GetAllToasties(Context.Guild.Id);
            var parsed = toasties.Select(x => 
                {
                    try
                    {
                        return new UserAmountView()
                        {
                            User = Context.Guild.GetUser(x.Id),
                            Amount = x.Count
                        };
                    }
                    catch
                    { return null; }
                })
                .Where(x => x != null && x.User != null);

            var msg = new PaginatedMessage();
            
            msg.Title = "User Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Toasties <:toastie3:454441133876183060>",
                    Pages = PaginatedMessage.PagesArray(parsed, 10),
                    Inline = true
                }
            };
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }

        [Command("DailyLeaderboard"), Alias("dlb"), Description("Daily Leaderboard.\n**Usage**: `!dlb`")]
        public async Task DailyLeaderboard()
        {
            var dailies = await DailyDb.GetLeaderboard(Context.Guild.Id);
            var parsed = dailies.Select(x =>
                {
                    try
                    {
                        return new UserAmountView()
                        {
                            User = Context.Guild.GetUser(x.Id),
                            Amount = x.Count
                        };
                    }
                    catch
                    { return null; }
                })
                .Where(x => x != null && x.User != null);

            var msg = new PaginatedMessage();

            msg.Author = new EmbedAuthorBuilder() { Name = "User Leaderboards" };
            msg.Title = "Daily Streak :calendar_spiral:";
            msg.Pages = PaginatedMessage.PagesArray(parsed, 10);

            await PagedReplyAsync(msg);
        }

        [Command("WaifuLeaderboard"), Alias("wlb"), Description("Shows waifu worth of each person.\n**Usage**: `!wlb`")]
        public async Task WaifuLeaderboard()
        {
            var AllWaifus = await UserInventoryDb.GetAllWaifuItems(Context.Guild.Id);
            var users = new Dictionary<SocketUser, int>();

            foreach (var x in AllWaifus)
            {
                var user = Context.Guild.GetUser(x.UserId);
                if (user != null)
                    if (!users.ContainsKey(user))
                        users.Add(user, WaifuUtil.WaifuValue(AllWaifus.Where(x => x.UserId == user.Id).Select(x => x.Waifu)));
            }

            var ordUsers = users.OrderByDescending(x => x.Value);

            var msg = new PaginatedMessage();

            msg.Title = "User Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Waifu Value <:toastie3:454441133876183060>",
                    Pages = PaginatedMessage.PagesArray(ordUsers, 10, (x) => $"{x.Key.Mention} - {x.Value}\n")
                }
            };
            msg.Fields = fields;

            await PagedReplyAsync(msg);
        }
        
        

        [Command("TopWaifus"), Alias("tw"), Description("Shows most popular waifus.\n**Usage**: `!tw`")]
        public async Task TopWaifus([Remainder] string search = "")
        {
            var waifus = await UserInventoryDb.CountWaifus(0, search.Split(' '));
            var msg = new PaginatedMessage();

            msg.Title = ":two_hearts: Waifu Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Globally Bought",
                    Pages = PaginatedMessage.PagesArray(waifus, 10, (x) => $"**{x.Key}** - {x.Value}\n")
                }
            };
            msg.Fields = fields;
            if (waifus.Any())
            {
                msg.ThumbnailUrl = (await WaifuDb.GetWaifu(waifus.First().Key)).HostImageUrl;
            }

            await PagedReplyAsync(msg);
        }

        [Command("ServerTopWaifus"), Alias("stw"), Description("Shows most popular waifus in the server.\n**Usage**: `!stw`")]
        public async Task ServerTopWaifus([Remainder] string search = "")
        {
            var waifus = await UserInventoryDb.CountWaifus(Context.Guild.Id, search.Split(' '));
            var msg = new PaginatedMessage();

            msg.Title = ":two_hearts: Waifu Leaderboards";
            var fields = new List<FieldPages>
            {
                new FieldPages
                {
                    Title = "Bought Here",
                    Pages = PaginatedMessage.PagesArray(waifus, 10, (x) => $"**{x.Key}** - {x.Value}\n")
                }
            };
            msg.Fields = fields;
            msg.ThumbnailUrl = (await WaifuDb.GetWaifu(waifus.First().Key)).HostImageUrl;

            await PagedReplyAsync(msg);
        }
    }
}