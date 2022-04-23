﻿using Discord;
using Discord.Commands;
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
    [Name("Ban Royale - Alpha")]
    public class Banroyales : CustomModuleBase<ICustomContext>
    {
        [BotPermission(GuildPermission.ManageRoles), UserPermission(GuildPermission.ManageMessages)]
        [Command("NewBanroyale"), Alias("nbrl"), Description("Starts a new game of Ban Royale, where participants play a reaction game until last one standing. Losers can get optionally kicked/banned and winners can get toasties as a reward.\n" +
            "**Usage**: `!nbrl [entry_fee-default_0] [required_role_name-optional]`\n")]
        public async Task NewBanroyale([Remainder] IRole role = null)
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale != null)
            {
                await ReplyAsync($":x: There is already a running Ban Royale in this channel. Type `{Program.GetPrefix(Context)}cbrl` to cancel it.");
                return;
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
                var newRole = await Context.Guild.CreateRoleAsync("Namiko-Banroyale", null, Color.Red, false, false, null);
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
            await ReplyAsync("Setting up a new game of Ban Royale! It's on." +
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

        [Command("Banroyale"), Alias("brl"), Description("Shows details of the current Ban Royale.\n**Usage**: `!brl`")]
        public async Task Banroyale([Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync($"There is no running Ban Royale in this channel. `{Program.GetPrefix(Context)}nbrl` to start a new one.");
                return;
            }

            var reqRole = Context.Guild.GetRole(banroyale.RoleReqId);
            var role = Context.Guild.GetRole(banroyale.ParticipantRoleId);
            var users = role.Members.ToList();
            string participants = users.Count > 0 ? $"\n\nParticipants:\n{BanroyaleUtil.BanroyaleParticipants(users.Select(x => x.Username).ToList())}" : "";
            await ReplyAsync(embed: BanroyaleUtil.BanroyaleDetailsEmbed(banroyale, Context.Guild.GetRole(banroyale.ParticipantRoleId), Context.Guild.GetRole(banroyale.RoleReqId), users.Count).Build());
        }

        [Command("JoinBanroyale"), Alias("jbrl"), Description("Join the current Ban Royale. Must be in the same channel.\n**Usage**: `!jbrl`"), BotPermission(GuildPermission.ManageRoles)]
        public async Task JoinBanroyale([Remainder] string str = "")
        {
            var user = Context.User as SocketGuildUser;
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: The ban royale already started. You're late to the party.");
                return;
            }

            if (banroyale.RoleReqId != 0)
            {
                if (!user.Roles.Any(x => x.Id == banroyale.RoleReqId))
                {
                    await ReplyAsync(":x: You do not have the required role to join!");
                    return;
                }
            }

            var role = Context.Guild.GetRole(banroyale.ParticipantRoleId);
            var users = role.Members.ToList();
            if (users.Count >= banroyale.MaxParticipants - 1 && banroyale.MaxParticipants != 0)
            {
                await ReplyAsync("Ban Royale is full!");
                return;
            }

            if (users.Contains(user))
            {
                await ReplyAsync("You are already participating! Eager to get smoked, aren't you?");
                return;
            }

            await user.AddRoleAsync(role);
            users.Add(user);
            string response = "You joined the Ban Royale. *Heh.*" + (users.Count > 10 ? "" : "\n\nList of Participants:\n" + BanroyaleUtil.BanroyaleParticipants(users.Select(x => x.Username).ToList()));
            await ReplyAsync(response);
        }

        [Command("LeaveBanroyale"), Alias("lbrl"), Description("Leave the current Ban Royale. Must be in the same channel.\n**Usage**: `!lbrl`"), BotPermission(GuildPermission.ManageRoles)]
        public async Task LeaveBanroyale([Remainder] string str = "")
        {
            var user = Context.User as SocketGuildUser;
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: The ban royale already started. You can't leave now ^^");
                return;
            }

            var role = Context.Guild.GetRole(banroyale.ParticipantRoleId);
            if (!role.Members.Contains(user))
            {
                await ReplyAsync($"You're not a participant!");
                return;
            }

            await user.RemoveRoleAsync(role);
            await ReplyAsync($"Tch... You left the Ban Royale...");
        }

        [UserPermission(GuildPermission.ManageMessages)]
        [Command("StartBanroyale"), Alias("sbrl"), Description("Starts the current Ban Royale. Must be in the same channel.\n**Usage**: `!sbrl`")]
        public async Task StartBanroyale([Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: The Ban Royale has already started.");
                return;
            }

            banroyale.Running = true;
            await BanroyaleDb.UpdateBanroyale(banroyale);

            var br = new BanroyaleGame(banroyale, Context.Channel, Context.Guild.GetRole(banroyale.ParticipantRoleId));

            await ReplyAsync($"Starting Ban Royale! I will send a new message every **{banroyale.MinFrequency}-{banroyale.MaxFrequency}s** and leave a reaction on them. The last users to click these reactions will lose!");
        }

        [UserPermission(GuildPermission.ManageMessages)]
        [Command("CancelBanroyale"), Alias("cbrl"), Description("Cancels the current Ban Royale.\n**Usage**: `!cbrl`")]
        public async Task CancelBanroyale([Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }

            await BanroyaleDb.EndBanroyale(banroyale.Id);
            await ReplyAsync("*Tch...* Cancelling ban royale.");
        }

        [UserPermission(GuildPermission.ManageMessages)]
        [Command("SetBrlWinners"), Alias("sbrlw"), Description("Set the amount of winners.\n**Usage**: `!sbrlw [amount]`")]
        public async Task SetBrlWinners(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }

            banroyale.WinnerAmount = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await ReplyAsync($"Set the amount of winners to **{amount}**!");
        }

        [UserPermission(GuildPermission.Administrator)]
        [Command("SetBrlRewardPool"), Alias("sbrlrp"), Description("Set the reward pool.\n**Usage**: `!sbrlrp [amount]`")]
        public async Task SetBrlRewardPool(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }

            banroyale.RewardPool = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await ReplyAsync($"Set the reward pool to **{amount}**!");
        }

        [UserPermission(GuildPermission.ManageMessages)]
        [Command("SetBrlMinParticipants"), Alias("sbrlminp"), Description("Set minimum participants.\n**Usage**: `!sbrlminp [amount]`")]
        public async Task SetBrlMinParticipants(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }

            banroyale.MinParticipants = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await ReplyAsync($"Set the minimum participants to **{amount}**!");
        }

        [UserPermission(GuildPermission.ManageMessages)]
        [Command("SetBrlMaxParticipants"), Alias("sbrlmaxp"), Description("Set maximum participants.\n**Usage**: `!sbrlmaxp [amount]`")]
        public async Task SetBrlMaxParticipants(int amount, [Remainder] string str = "")
        {
            if (amount > 80)
            {
                await ReplyAsync(":x: There is an upper limit of 80 players.");
                return;
            }

            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }

            banroyale.MaxParticipants = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await ReplyAsync($"Set the maximum participants to **{amount}**!");
        }

        [UserPermission(GuildPermission.BanMembers), BotPermission(GuildPermission.BanMembers)]
        [Command("SetBrlBanDuration"), Alias("sbrlban"), Description("Set ban duration in hours.\n**Usage**: `!sbrlban [hours]`")]
        public async Task SetBrlBanDuration(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }
            if (amount < 0)
            {
                await ReplyAsync(":x: Ban duration can't be less than 0, baaaaka.");
                return;
            }

            banroyale.BanLengthHours = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await ReplyAsync($"Set the ban duration to **{amount} hours**!");
        }

        [UserPermission(GuildPermission.KickMembers), BotPermission(GuildPermission.KickMembers)]
        [Command("SetBrlKick"), Alias("sbrlkick"), Description("Set maximum participants.\n**Usage**: `!sbrlkick [amount]`")]
        public async Task SetBrlKick([Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }

            banroyale.Kick = banroyale.Kick ? false : true;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await ReplyAsync(banroyale.Kick ? "Losers will be kicked from the server!" : "Losers will not be kicked...");
        }

        [UserPermission(GuildPermission.ManageMessages)]
        [Command("SetBrlMinFrequency"), Alias("sbrlminf"), Description("Set maximum participants.\n**Usage**: `!sbrlminf [amount]`")]
        public async Task SetBrlMinFrequency(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }
            if (banroyale.MaxFrequency < amount)
            {
                await ReplyAsync($":x: Minimum frequency can't be higher than the maximum, baaaaka. Current max frequency: **{banroyale.MaxFrequency}**.");
                return;
            }
            if (amount < 10 || amount > 21600)
            {
                await ReplyAsync($":x: Minimum frequency must be in the range of **10** (1m) to **21600** (6h) seconds.");
                return;
            }

            banroyale.MinFrequency = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await ReplyAsync($"Set the minimum message frequency to **{amount} seconds**!");
        }

        [UserPermission(GuildPermission.ManageMessages)]
        [Command("SetBrlMaxFrequency"), Alias("sbrlmaxf"), Description("Set maximum participants.\n**Usage**: `!sbrlmaxf [amount]`")]
        public async Task SetBrlMaxFrequency(int amount, [Remainder] string str = "")
        {
            var banroyale = await BanroyaleDb.GetBanroyale(Context.Channel.Id);
            if (banroyale == null)
            {
                await ReplyAsync(":x: There is no running Ban Royale in this channel.");
                return;
            }
            if (banroyale.Running == true)
            {
                await ReplyAsync(":x: Cannot change settings after the ban royale has started.");
                return;
            }
            if (banroyale.MinFrequency > amount)
            {
                await ReplyAsync($":x: Minimum frequency can't be higher than the maximum, baaaaka. Current min frequency: **{banroyale.MinFrequency}**.");
                return;
            }
            if (amount < 20 || amount > 21600)
            {
                await ReplyAsync($":x: Maximum frequency must be in the range of **20** (2m) to **21600** (6h) seconds.");
                return;
            }

            banroyale.MaxFrequency = amount;
            await BanroyaleDb.UpdateBanroyale(banroyale);
            await ReplyAsync($"Set the maximum message frequency to **{amount} seconds**!");
        }
    }
}
