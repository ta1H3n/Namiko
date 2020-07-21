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
    [Name("Ban Royale - Alpha")]
    public class Banroyales : InteractiveBase<ShardedCommandContext>
    {
        [Command("NewBanroyale"), Alias("nbrl"), Summary("Starts a new game of Ban Royale, where participants play a reaction game until last one standing. Losers can get optionally kicked/banned and winners can get toasties as a reward.\n" +
            "**Usage**: `!nbrl [entry_fee-default_0] [required_role_name-optional]`\n"), CustomBotPermission(GuildPermission.ManageRoles), CustomUserPermission(GuildPermission.ManageMessages), RequireGuild]
        public async Task NewBanroyale([Remainder] string roleName = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale != null)
            {
                await Context.Channel.SendMessageAsync($":x: There is already a running Ban Royale in this channel. Type `{Program.GetPrefix(Context)}cbrl` to cancel it.");
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

            var roleId = await BanroyaleDb.GetRoleId(Context.Guild.Id);
            if (roleId != 0)
            {
                try
                {
                    roleId = Context.Guild.GetRole(roleId).Id;
                } catch 
                {
                    roleId = 0;
                }
            }

            if (roleId == 0)
            {
                var newRole = await Context.Guild.CreateRoleAsync("Namiko-Banroyale", color: Color.Red);
                roleId = newRole.Id;
                await ReplyAsync($"Creating a role - {newRole.Mention}. It will be used to track the Ban Royale participants automatically by assigning/removing the role to/from them.\n" +
                    $"You can change the name and the color of the role. But make sure it is lower in the role list than my bot role, Senpai.");
            }

            banroyale = new Banroyale
            {
                Active = true,
                BanLengthHours = 0,
                ChannelId = Context.Channel.Id,
                MaxParticipants = 0,
                MinParticipants = 0,
                RewardPool = 0,
                GuildId = Context.Guild.Id,
                RoleReqId = role == null ? 0 : role.Id,
                Kick = false,
                WinnerAmount = 1,
                ParticipantRoleId = roleId,
                MinFrequency = 10,
                MaxFrequency = 20
            };

            string prefix = Program.GetPrefix(Context);
            await BanroyaleDb.AddBanroyale(banroyale);
            await Context.Channel.SendMessageAsync("Setting up a new game of Ban Royale! It's on." +
                $"\n\n**More settings:**" +
                $"\n`{prefix}sbrlrp` - set reward pool" +
                $"\n`{prefix}sbrlw` - set amount of winners" +
                $"\n`{prefix}sbrlminp` - minimum participants" +
                $"\n`{prefix}sbrlmaxp` - maximum participants" +
                $"\n`{prefix}sbrlban` - set loser ban duration" +
                $"\n`{prefix}sbrlkick` - set loser kick" +
                $"\n`{prefix}sbrlminf` - set min message frequency in seconds" +
                $"\n`{prefix}sbrlmaxf` - set max message frequency in seconds" +
                $"\n\n*Type `{prefix}jbrl` to join the game.*" +
                $"\n*Type `{prefix}startbrl` to start the game after some players join.*" +
                $"\n*Type `{prefix}brl` to view current settings.*",
                embed: BanroyaleUtil.BanroyaleDetailsEmbed(banroyale, Context.Guild.GetRole(banroyale.ParticipantRoleId), Context.Guild.GetRole(banroyale.RoleReqId)).Build());
        }

        [Command("Banroyale"), Alias("brl"), Summary("Shows details of the current Ban Royale.\n**Usage**: `!brl`")]
        public async Task Banroyale([Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync($"There is no running Ban Royale in this channel. `{Program.GetPrefix(Context)}nbrl` to start a new one.");
                return;
            }

            var reqRole = Context.Guild.GetRole(banroyale.RoleReqId);
            var role = Context.Guild.GetRole(banroyale.ParticipantRoleId);
            var users = role.Members.ToList();
            string participants = users.Count > 0 ? $"\n\nParticipants:\n{BanroyaleUtil.BanroyaleParticipants(users.Select(x => x.Username).ToList())}" : "";
            await Context.Channel.SendMessageAsync(embed: BanroyaleUtil.BanroyaleDetailsEmbed(banroyale, Context.Guild.GetRole(banroyale.ParticipantRoleId), Context.Guild.GetRole(banroyale.RoleReqId), users.Count).Build());
        }

        [Command("JoinBanroyale"), Alias("jbrl"), Summary("Join the current Ban Royale. Must be in the same channel.\n**Usage**: `!jbrl`"), CustomBotPermission(GuildPermission.ManageRoles)]
        public async Task JoinBanroyale([Remainder] string str = "")
        {
            var user = Context.User as SocketGuildUser;
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: The ban royale already started. You're late to the party.");
                return;
            }

            if (banroyale.RoleReqId != 0)
            {
                if (!user.Roles.Any(x => x.Id == banroyale.RoleReqId))
                {
                    await Context.Channel.SendMessageAsync(":x: You do not have the required role to join!");
                    return;
                }
            }

            var role = Context.Guild.GetRole(banroyale.ParticipantRoleId);
            var users = role.Members.ToList();
            if (users.Count >= banroyale.MaxParticipants - 1 && banroyale.MaxParticipants != 0)
            {
                await Context.Channel.SendMessageAsync("Ban Royale is full!");
                return;
            }

            if (users.Contains(user))
            {
                await Context.Channel.SendMessageAsync("You are already participating! Eager to get smoked, aren't you?");
                return;
            }

            await user.AddRoleAsync(role);
            users.Add(user);
            string response = "You joined the Ban Royale. *Heh.*" + (users.Count > 10 ? "" : "\n\nList of Participants:\n" + BanroyaleUtil.BanroyaleParticipants(users.Select(x => x.Username).ToList()));
            await Context.Channel.SendMessageAsync(response);
        }

        [Command("LeaveBanroyale"), Alias("lbrl"), Summary("Leave the current Ban Royale. Must be in the same channel.\n**Usage**: `!lbrl`"), CustomBotPermission(GuildPermission.ManageRoles)]
        public async Task LeaveBanroyale([Remainder] string str = "")
        {
            var user = Context.User as SocketGuildUser;
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: The ban royale already started. You can't leave now ^^");
                return;
            }

            var role = Context.Guild.GetRole(banroyale.ParticipantRoleId);
            if (!role.Members.Contains(user))
            {
                await Context.Channel.SendMessageAsync($"You're not a participant!");
                return;
            }

            await user.RemoveRoleAsync(role);
            await Context.Channel.SendMessageAsync($"Tch... You left the Ban Royale...");
        }

        [Command("StartBanroyale"), Alias("sbrl"), Summary("Starts the current Ban Royale. Must be in the same channel.\n**Usage**: `!sbrl`"), CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task StartBanroyale([Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: The Ban Royale has already started.");
                return;
            }

            banroyale.Running = true;
            await BanroyaleDb.UpdateBanroyale(banroyale);

            var br = new BanroyaleGame(banroyale, Context.Channel, Context.Guild.GetRole(banroyale.ParticipantRoleId));

            await ReplyAsync($"Starting Ban Royale! I will send a new message every **{banroyale.MinFrequency}-{banroyale.MaxFrequency}s** and leave a reaction on them. The last users to click these reactions will lose!");
        }

        [Command("CancelBanroyale"), Alias("cbrl"), Summary("Cancels the current Ban Royale.\n**Usage**: `!cbrl`"), CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task CancelBanroyale([Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }

            await BanroyaleDb.EndBanroyale(banroyale.Id);
            await Context.Channel.SendMessageAsync("*Tch...* Cancelling ban royale.");
        }

        [Command("SetBrlWinners"), Alias("sbrlw"), Summary("Set the amount of winners.\n**Usage**: `!sbrlw [amount]`"), CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task SetBrlWinners(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }

            banroyale.WinnerAmount = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await Context.Channel.SendMessageAsync($"Set the amount of winners to **{amount}**!");
        }

        [Command("SetBrlRewardPool"), Alias("sbrlrp"), Summary("Set the reward pool.\n**Usage**: `!sbrlrp [amount]`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task SetBrlRewardPool(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }

            banroyale.RewardPool = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await Context.Channel.SendMessageAsync($"Set the reward pool to **{amount}**!");
        }

        [Command("SetBrlMinParticipants"), Alias("sbrlminp"), Summary("Set minimum participants.\n**Usage**: `!sbrlminp [amount]`"), CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task SetBrlMinParticipants(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }

            banroyale.MinParticipants = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await Context.Channel.SendMessageAsync($"Set the minimum participants to **{amount}**!");
        }

        [Command("SetBrlMaxParticipants"), Alias("sbrlmaxp"), Summary("Set maximum participants.\n**Usage**: `!sbrlmaxp [amount]`"), CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task SetBrlMaxParticipants(int amount, [Remainder] string str = "")
        {
            if (amount > 80)
            {
                await Context.Channel.SendMessageAsync(":x: There is an upper limit of 80 players.");
                return;
            }

            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }

            banroyale.MaxParticipants = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await Context.Channel.SendMessageAsync($"Set the maximum participants to **{amount}**!");
        }

        [Command("SetBrlBanDuration"), Alias("sbrlban"), Summary("Set ban duration in hours.\n**Usage**: `!sbrlban [hours]`"), CustomUserPermission(GuildPermission.BanMembers), CustomBotPermission(GuildPermission.BanMembers)]
        public async Task SetBrlBanDuration(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }
            if (amount < 0)
            {
                await Context.Channel.SendMessageAsync(":x: Ban duration can't be less than 0, baaaaka.");
                return;
            }

            banroyale.BanLengthHours = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await Context.Channel.SendMessageAsync($"Set the ban duration to **{amount} hours**!");
        }

        [Command("SetBrlKick"), Alias("sbrlkick"), Summary("Set maximum participants.\n**Usage**: `!sbrlkick [amount]`"), CustomUserPermission(GuildPermission.KickMembers), CustomBotPermission(GuildPermission.KickMembers)]
        public async Task SetBrlKick([Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }

            banroyale.Kick = banroyale.Kick ? false : true;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await Context.Channel.SendMessageAsync(banroyale.Kick ? "Losers will be kicked from the server!" : "Losers will not be kicked...");
        }

        [Command("SetBrlMinFrequency"), Alias("sbrlminf"), Summary("Set maximum participants.\n**Usage**: `!sbrlminf [amount]`"), CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task SetBrlMinFrequency(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }
            if (banroyale.MaxFrequency < amount)
            {
                await Context.Channel.SendMessageAsync($":x: Minimum frequency can't be higher than the maximum, baaaaka. Current max frequency: **{banroyale.MaxFrequency}**.");
                return;
            }
            if (amount < 10 || amount > 21600)
            {
                await Context.Channel.SendMessageAsync($":x: Minimum frequency must be in the range of **10** (1m) to **21600** (6h) seconds.");
                return;
            }

            banroyale.MinFrequency = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await Context.Channel.SendMessageAsync($"Set the minimum message frequency to **{amount} seconds**!");
        }

        [Command("SetBrlMaxFrequency"), Alias("sbrlmaxf"), Summary("Set maximum participants.\n**Usage**: `!sbrlmaxf [amount]`"), CustomUserPermission(GuildPermission.ManageMessages)]
        public async Task SetBrlMaxFrequency(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await Context.Channel.SendMessageAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await Context.Channel.SendMessageAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }
            if (banroyale.MinFrequency > amount)
            {
                await Context.Channel.SendMessageAsync($":x: Minimum frequency can't be higher than the maximum, baaaaka. Current min frequency: **{banroyale.MinFrequency}**.");
                return;
            }
            if (amount < 20 || amount > 21600)
            {
                await Context.Channel.SendMessageAsync($":x: Maximum frequency must be in the range of **20** (2m) to **21600** (6h) seconds.");
                return;
            }

            banroyale.MaxFrequency = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await Context.Channel.SendMessageAsync($"Set the maximum message frequency to **{amount} seconds**!");
        }
    }
}
