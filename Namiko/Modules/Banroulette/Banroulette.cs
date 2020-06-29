using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    [RequireGuild]
    [Name("Banroulette")]
    public class Banroulettes : InteractiveBase<ShardedCommandContext>
    {
        [Command("NewBanroulette"), Alias("nbr"), Summary("Starts a new game of ban roulette, where one participant is randomly banned from the server. Winners split toasties from the reward pool.\n" +
            "**Usage**: `!nbr [ban_length_in_hours] [required_role_name-optional]`\n"), CustomBotPermission(GuildPermission.BanMembers), CustomUserPermission(GuildPermission.BanMembers), RequireGuild]
        public async Task NewBanroulette(int hours, [Remainder] string roleName = "")
        {
            if (hours < 0)
                throw new IndexOutOfRangeException();

            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if(banroulette != null)
            {
                await Context.Channel.SendMessageAsync(":x: There is already a running Ban Roulette in this channel. Type `!ebr` to end it.");
                return;
            }

            SocketRole role = null;
            if (roleName != "")
            {
                role = await this.SelectRole(roleName);
                if (role == null)
                {
                    return;
                }
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

            string prefix = Program.GetPrefix(Context);
            await BanrouletteDb.NewBanroulette(banroulette);
            await Context.Channel.SendMessageAsync("Started a new game of Ban Roulette! It's on.\n\n" + BanrouletteUtil.BanrouletteDetails(banroulette, role) +
                $"\n\n**More settings:**" +
                $"\n`{prefix}sbrrp` - set reward pool" +
                $"\n`{prefix}sbrmin` - minimum participants" +
                $"\n`{prefix}sbrmax` - maximum participants" +
                $"\n\n*Type `{prefix}jbr` to join the game.*");
        }

        [Command("Banroulette"), Alias("br"), Summary("Shows details of the current Ban Roulette.\n**Usage**: `!br`")]
        public async Task Banroulette()
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if(banroulette == null)
            {
                await Context.Channel.SendMessageAsync($"There is no running Ban Roulette in this channel. `{Program.GetPrefix(Context)}nbr` to start a new one.");
                return;
            }

            var users = BasicUtil.UserList(Context.Client, BanrouletteDb.GetParticipants(banroulette));
            var role = Context.Guild.GetRole(banroulette.RoleReqId);
            string participants = users.Count > 0 ? $"\n\nParticipants:\n{BanrouletteUtil.BanrouletteParticipants(users)}" : "";
            await Context.Channel.SendMessageAsync($"{BanrouletteUtil.BanrouletteDetails(banroulette, role, users.Count)}" + participants);
        }

        [Command("JoinBanroulette"), Alias("jbr"), Summary("Join the current Ban Roulette. Must be in the same channel.\n**Usage**: `!jbr`")]
        public async Task JoinBanroulette()
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if(banroulette == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            if (banroulette.RoleReqId != 0)
            {
                var user = Context.User as SocketGuildUser;
                if (!user.Roles.Any(x => x.Id == banroulette.RoleReqId))
                {
                    await Context.Channel.SendMessageAsync(":x: You do not have the required role to join!");
                    return;
                }
            }
                    

            var userIds = BanrouletteDb.GetParticipants(banroulette);
            if (userIds.Count >= banroulette.MaxParticipants - 1 && banroulette.MaxParticipants != 0)
            {
                await Context.Channel.SendMessageAsync("Ban Roulette is full!");
                return;
            }
            
            bool joined = await BanrouletteDb.AddParticipant(Context.User.Id, banroulette);
            if(!joined)
            {
                await Context.Channel.SendMessageAsync("You are already participating! Eager to get smoked, aren't you?");
                return;
            }

            var users = BasicUtil.UserList(Context.Client, userIds);
            users.Add(Context.User);
            string response = "You joined the Ban Roulette. *Heh.*" + (users.Count > 10 ? "" : "\n\nList of Participants:\n" + BanrouletteUtil.BanrouletteParticipants(users));
            await Context.Channel.SendMessageAsync(response);
        }

        [Command("CancelBanroulette"), Alias("cbr"), Summary("Cancels the current Ban Roulette.\n**Usage**: `!cbr`"), CustomUserPermission(GuildPermission.BanMembers)]
        public async Task CancelBanroulette()
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if(banroulette == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            await BanrouletteDb.EndBanroulette(banroulette.Id);
            await Context.Channel.SendMessageAsync("*Tch...* Cancelling ban roulette.");
        }

        [Command("EndBanroulette"), Alias("ebr"), Summary("Ends the current Ban Roulette, banning a random participant and splitting the reward pool between the others.\n**Usage**: `!ebr`"), CustomBotPermission(GuildPermission.BanMembers), CustomUserPermission(GuildPermission.BanMembers)]
        public async Task EndBanroulette()
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            var userIds = BanrouletteDb.GetParticipants(banroulette);
            if (userIds.Count < banroulette.MinParticipants)
            {
                await Context.Channel.SendMessageAsync($"Not enough participants! {userIds.Count}/{banroulette.MinParticipants}");
                return;
            }

            var users = BasicUtil.UserList(Context.Client, userIds);
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
            await Context.Channel.SendMessageAsync(msg);
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
                    await Context.Channel.SendMessageAsync($"I couldn't ban {user}, they are too powerful. *Wipes off blood.* This usually means that their role is higher than mine.");
                }
            }
        }

        [Command("BrRewardPool"), Alias("brrp"), Summary("Add toasties to the Ban Roulette reward pool from your account.\n**Usage**: `!brrp [amount]`")]
        public async Task BRRewardPool(string amountStr)
        {
            int amount = ToastieUtil.ParseAmount(amountStr, (SocketGuildUser)Context.User);
            if (amount < 0)
            {
                await Context.Channel.SendMessageAsync("Pick an amount! number, all, half, or x/y.");
                return;
            }
            if (amount == 0)
            {
                await Context.Channel.SendMessageAsync("You have no toasties...");
                return;
            }

            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            try
            {
                await BalanceDb.AddToasties(Context.User.Id, -amount, Context.Guild.Id);
            }
            catch(Exception ex)
            {
                await Context.Channel.SendMessageAsync(ex.Message);
                return;
            }

            banroulette.RewardPool += amount;
            await BanrouletteDb.UpdateBanroulette(banroulette);
            await Context.Channel.SendMessageAsync($"Added {amount} to the reward pool!");
        }

        [Command("SetBrRewardPool"), Alias("sbrrp"), Summary("Set the reward pool.\n**Usage**: `!sbrrp [amount]`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task SetBRRewardPool(int amount)
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            banroulette.RewardPool = amount;
            await BanrouletteDb.UpdateBanroulette(banroulette);
            await Context.Channel.SendMessageAsync($"Set the reward pool to {amount}!");
        }

        [Command("SetBrMinParticipants"), Alias("sbrmin"), Summary("Set minimum participants.\n**Usage**: `!sbrmin [amount]`"), CustomUserPermission(GuildPermission.BanMembers)]
        public async Task SetBRMinParticipants(int amount)
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            banroulette.MinParticipants = amount;
            await BanrouletteDb.UpdateBanroulette(banroulette);
            await Context.Channel.SendMessageAsync($"Set the minimum participants to {amount}!");
        }

        [Command("SetBrMaxParticipants"), Alias("sbrmax"), Summary("Set maximum participants.\n**Usage**: `!sbrmax [amount]`"), CustomUserPermission(GuildPermission.BanMembers)]
        public async Task SetBRMaxParticipants(int amount)
        {
            var banroulette = BanrouletteDb.GetBanroulette(Context.Channel.Id);
            if (banroulette == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Roulette in this channel.");
                return;
            }

            banroulette.MaxParticipants = amount;
            await BanrouletteDb.UpdateBanroulette(banroulette);
            await Context.Channel.SendMessageAsync($"Set the maximum participants to {amount}!");
        }
    }
}
