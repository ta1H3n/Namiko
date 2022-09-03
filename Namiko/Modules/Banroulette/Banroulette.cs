using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    [RequireGuild]
    [Name("Banroulette")]
    public class Banroulettes : CustomModuleBase<ICustomContext>
    {
        [BotPermission(GuildPermission.BanMembers), UserPermission(GuildPermission.BanMembers)]
        [Command("NewBanroulette"), Alias("nbr"), Description("Starts a new game of ban roulette, where one participant is randomly banned from the server. Winners split toasties from the reward pool.\n" +
            "**Usage**: `!nbr [ban_length_in_hours] [required_role_name-optional]`\n")]
        [SlashCommand("new-banroulette", "Starts a new game of ban roulette, where one participant is randomly banned from the server")]
        public async Task NewBanroulette([Description("Ban length in hours")] int hours, [Description("Only allow members with this role to join")][Remainder] IRole role = null)
        {
            if (hours < 0)
                throw new IndexOutOfRangeException();

            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            string prefix = TextCommandService.GetPrefix(Context);
            if (banroulette != null)
            {
                await ReplyAsync($":x: There is already a running Ban Roulette in this channel. Type `{prefix}ebr` to end it.");
                return;
            }

            banroulette = new Banroulette
            {
                Active = true,
                BanLengthHours = hours,
                ChannelId = Context.Channel.Id,
                MaxParticipants = 0,
                MinParticipants = 0,
                RewardPool = 0,
                ServerId = Context.Guild.Id,
                RoleReqId = role == null ? 0 : role.Id
            };

            await BanrouletteDb.NewBanroulette(banroulette);
            await ReplyAsync("Started a new game of Ban Roulette! It's on.\n\n" + BanrouletteUtil.BanrouletteDetails(banroulette, role) +
                $"\n\n**More settings:**" +
                $"\n`{prefix}sbrrp` - set reward pool" +
                $"\n`{prefix}sbrmin` - minimum participants" +
                $"\n`{prefix}sbrmax` - maximum participants" +
                $"\n\n*Type `{prefix}jbr` to join the game.*");
        }

        [Command("Banroulette"), Alias("br"), Description("Shows details of the current Ban Roulette.\n**Usage**: `!br`")]
        [SlashCommand("banroulette", "Check current banroulette details")]
        public async Task Banroulette()
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if(banroulette == null)
            {
                await ReplyAsync($"There is no running Ban Roulette in this channel. `{TextCommandService.GetPrefix(Context)}nbr` to start a new one.");
                return;
            }

            var users = await BasicUtil.UserListAsync(Context.Client, BanrouletteDb.GetParticipants(banroulette));
            var role = Context.Guild.GetRole(banroulette.RoleReqId);
            string participants = users.Count > 0 ? $"\n\nParticipants:\n{BanrouletteUtil.BanrouletteParticipants(users)}" : "";
            await ReplyAsync($"{BanrouletteUtil.BanrouletteDetails(banroulette, role, users.Count)}" + participants);
        }

        [Command("JoinBanroulette"), Alias("jbr"), Description("Join the current Ban Roulette. Must be in the same channel.\n**Usage**: `!jbr`")]
        [SlashCommand("join-banroulette", "Join the current banroulette")]
        public async Task JoinBanroulette()
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if(banroulette == null)
            {
                await ReplyAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            if (banroulette.RoleReqId != 0)
            {
                var user = Context.User as SocketGuildUser;
                if (!user.Roles.Any(x => x.Id == banroulette.RoleReqId))
                {
                    await ReplyAsync(":x: You do not have the required role to join!");
                    return;
                }
            }
                    

            var userIds = BanrouletteDb.GetParticipants(banroulette);
            if (userIds.Count >= banroulette.MaxParticipants - 1 && banroulette.MaxParticipants != 0)
            {
                await ReplyAsync("Ban Roulette is full!");
                return;
            }
            
            bool joined = await BanrouletteDb.AddParticipant(Context.User.Id, banroulette);
            if(!joined)
            {
                await ReplyAsync("You are already participating! Eager to get smoked, aren't you?");
                return;
            }

            var users = await BasicUtil.UserListAsync(Context.Client, userIds);
            users.Add(Context.User);
            string response = "You joined the Ban Roulette. *Heh.*" + (users.Count > 10 ? "" : "\n\nList of Participants:\n" + BanrouletteUtil.BanrouletteParticipants(users));
            await ReplyAsync(response);
        }

        [UserPermission(GuildPermission.BanMembers)]
        [Command("CancelBanroulette"), Alias("cbr"), Description("Cancels the current Ban Roulette.\n**Usage**: `!cbr`")]
        [SlashCommand("cancel-banroulette", "Cancel the current banroulette")]
        public async Task CancelBanroulette()
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if(banroulette == null)
            {
                await ReplyAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            await BanrouletteDb.EndBanroulette(banroulette.Id);
            await ReplyAsync("*Tch...* Cancelling ban roulette.");
        }

        [BotPermission(GuildPermission.BanMembers), UserPermission(GuildPermission.BanMembers)]
        [Command("RunBanroulette"), Alias("ebr", "EndBanroulette"), Description("Ends the current Ban Roulette, banning a random participant and splitting the reward pool between the others.\n**Usage**: `!ebr`")]
        [SlashCommand("run-banroulette", "Runs the current banroulette")]
        public async Task EndBanroulette()
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await ReplyAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            var userIds = BanrouletteDb.GetParticipants(banroulette);
            if (userIds.Count < banroulette.MinParticipants)
            {
                await ReplyAsync($"Not enough participants! {userIds.Count}/{banroulette.MinParticipants}");
                return;
            }

            var users = await BasicUtil.UserListAsync(Context.Client, userIds);
            var user = users[new Random().Next(users.Count)];

            var ban = new BannedUser
            {
                Active = true,
                ServerId = Context.Guild.Id,
                UserId = user.Id,
                DateBanStart = DateTime.UtcNow,
                DateBanEnd = DateTime.UtcNow.AddHours(banroulette.BanLengthHours)
            };

            string msg = $"{user.Mention} won! Bai baaaaaai! See you in {banroulette.BanLengthHours} hours!\n\n";

            users.Remove(user);
            if (users.Count > 0 && banroulette.RewardPool > 0)
            {
                int prize = banroulette.RewardPool / users.Count;
                msg += $"Prize pool of {banroulette.RewardPool} ({prize} each) split between: ";
                foreach (var x in users)
                {
                    await BalanceDb.AddToasties(x.Id, prize, Context.Guild.Id);
                    msg += x.Mention + " ";
                }
            }
            await ReplyAsync(msg);
            await user.SendMessageAsync($"You won the ban roulette! You are banned from **{Context.Guild.Name}** for {banroulette.BanLengthHours} hours! Bai baaaai!");

            await BanrouletteDb.EndBanroulette(banroulette.Id);

            if (banroulette.BanLengthHours > 0)
            {
                await BanDb.AddBan(ban);
                try
                {
                    await Context.Guild.AddBanAsync(user, 0, $"Banroulette {banroulette.Id}, {banroulette.BanLengthHours} hours.");
                } catch { }
                {
                    await ReplyAsync($"I couldn't ban {user}, they are too powerful. *Wipes off blood.* This usually means that their role is higher than mine.");
                }
            }
        }

        [Command("BrRewardPool"), Alias("brrp"), Description("Add toasties to the reward pool from your account.\n**Usage**: `!brrp [amount]`")]
        [SlashCommand("banroulette-reward-pool", "Add to the reward pool from your balance")]
        public async Task BRRewardPool([Description("amount")] string amountStr)
        {
            int amount = CurrencyUtil.ParseAmount(amountStr, (SocketGuildUser)Context.User);
            if (amount < 0)
            {
                await ReplyAsync("Pick an amount! number, all, half, or x/y.");
                return;
            }
            if (amount == 0)
            {
                await ReplyAsync("You have no toasties...");
                return;
            }

            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await ReplyAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            try
            {
                await BalanceDb.AddToasties(Context.User.Id, -amount, Context.Guild.Id);
            }
            catch(Exception ex)
            {
                await ReplyAsync(ex.Message);
                return;
            }

            banroulette.RewardPool += amount;
            await BanrouletteDb.UpdateBanroulette(banroulette);
            await ReplyAsync($"Added {amount} to the reward pool!");
        }

        [UserPermission(GuildPermission.Administrator)]
        [Command("SetBrRewardPool"), Alias("sbrrp"), Description("Set the reward pool.\n**Usage**: `!sbrrp [amount]`")]
        [SlashCommand("banroulette-set-reward-pool", "Set the reward pool")]
        public async Task SetBRRewardPool(int amount)
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await ReplyAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            banroulette.RewardPool = amount;
            await BanrouletteDb.UpdateBanroulette(banroulette);
            await ReplyAsync($"Set the reward pool to {amount}!");
        }

        [UserPermission(GuildPermission.BanMembers)]
        [Command("SetBrMinParticipants"), Alias("sbrmin"), Description("Set minimum participants.\n**Usage**: `!sbrmin [amount]`")]
        [SlashCommand("banroulette-set-min-participants", "Set minimum participants")]
        public async Task SetBRMinParticipants(int amount)
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await ReplyAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            banroulette.MinParticipants = amount;
            await BanrouletteDb.UpdateBanroulette(banroulette);
            await ReplyAsync($"Set the minimum participants to {amount}!");
        }

        [UserPermission(GuildPermission.BanMembers)]
        [Command("SetBrMaxParticipants"), Alias("sbrmax"), Description("Set maximum participants.\n**Usage**: `!sbrmax [amount]`")]
        [SlashCommand("banroulette-set-max-participants", "Set maximum participants")]
        public async Task SetBRMaxParticipants(int amount)
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await ReplyAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            banroulette.MaxParticipants = amount;
            await BanrouletteDb.UpdateBanroulette(banroulette);
            await ReplyAsync($"Set the maximum participants to {amount}!");
        }
    }
}
