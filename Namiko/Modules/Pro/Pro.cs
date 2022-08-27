using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Model;
using Model.Models.Users;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Namiko.Modules.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko.Modules.Pro
{
    [RequireGuild]
    [Name("Pro")]
    public class Pro : CustomModuleBase<ICustomContext>
    {
        [Command("ActivateProGuild"), Alias("asp", "ActivateServerPremium", "apg"), Description("Activates pro guild in the current server.\n**Usage**: `!asp [tier]`")]
        [SlashCommand("activate-pro-guild", "Activate Pro Guild benefits. Upgrades for your Server.")]
        public async Task ActivateServerPremium()
        {
            var ntr = Context.Client.GetGuild((ulong)ProType.HomeGuildId_NOTAPREMIUMTYPE);
            SocketGuildUser user = ntr.GetUser(Context.User.Id);

            if (user == null)
            {
                await ReplyAsync($"You are not in my server! {LinkHelper.SupportServerInvite}");
                return;
            }

            var current = PremiumDb.GetGuildPremium(Context.Guild.Id);
            var roles = user.Roles;

            bool log = false;
            string text = "";
            foreach (var role in roles)
            {
                if (role.Id == (ulong)ProType.GuildPlus)
                {
                    if (current.Any(x => x.Type == ProType.GuildPlus))
                    {
                        text += "**Pro Guild+** is already activated in this server!\n";
                    }
                    else
                    {
                        if (PremiumDb.IsPremium(user.Id, ProType.GuildPlus))
                            text += "You used your **Pro Guild+** premium upgrade in another server...\n";

                        else
                        {
                            await PremiumDb.AddPremium(user.Id, ProType.GuildPlus, Context.Guild.Id);
                            try
                            {
                                await PremiumDb.DeletePremium(PremiumDb.GetGuildPremium(Context.Guild.Id).FirstOrDefault(x => x.Type == ProType.Guild));
                            }
                            catch { }
                            text += "**Pro Guild+** activated!\n";
                            log = true;
                        }
                    }
                    break;
                }
                if (role.Id == (ulong)ProType.Guild)
                {
                    if (current.Any(x => x.Type == ProType.GuildPlus))
                        text += "**Pro Guild+** is already activated in this server!\n";
                    else if (current.Any(x => x.Type == ProType.Guild))
                        text += "**Pro Guild** is already activated in this server!\n";
                    else
                    {
                        if (PremiumDb.IsPremium(user.Id, ProType.Guild))
                            text += "You used your **Pro Guild** premium upgrade in another server...\n";

                        else
                        {
                            await PremiumDb.AddPremium(user.Id, ProType.Guild, Context.Guild.Id);
                            text += "**Pro Guild** activated!\n";
                            log = true;
                        }
                    }
                    break;
                }
            }
            if (text == "")
                text += $"You have no Pro Guild... Try `{Program.GetPrefix(Context)}Pro`";

            await ReplyAsync(text);
            if (log)
            {
                await WebhookClients.PremiumLogChannel.SendMessageAsync(embeds: new List<Embed>
                    {
                        new EmbedBuilderPrepared(Context.User)
                            .WithDescription($"{Context.User.Mention} `{Context.User.Id}`\n{Context.Guild.Name} `{Context.Guild.Id}`\n{text}")
                            .WithFooter(System.DateTime.Now.ToLongDateString())
                            .Build()
                    });
            }
        }

        [Command("ActivatePro"), Alias("ap", "ActivatePremium"), Description("Activates premium subscriptions associated with this account.\n**Usage**: `!ap`")]
        [SlashCommand("activate-pro", "Activate Pro benefits. Upgrades for your account.")]
        public async Task ActivatePremium()
        {
            var ntr = Context.Client.GetGuild((ulong)ProType.HomeGuildId_NOTAPREMIUMTYPE);
            SocketGuildUser user = ntr.GetUser(Context.User.Id);

            if (user == null)
            {
                await ReplyAsync($"You are not in my server! {LinkHelper.SupportServerInvite}");
                return;
            }

            var current = PremiumDb.GetUserPremium(user.Id);
            var roles = user.Roles;

            bool log = false;
            string text = "";
            foreach (var role in roles)
            {
                if (role.Id == (ulong)ProType.ProPlus)
                {
                    if (current.Any(x => x.Type == ProType.ProPlus))
                        text += "You already have **Pro+**!\n";
                    else
                    {
                        await PremiumDb.AddPremium(user.Id, ProType.ProPlus);
                        text += "**Pro+** activated!\n";
                        log = true;
                    }
                }
                if (role.Id == (ulong)ProType.Pro)
                {
                    if (current.Any(x => x.Type == ProType.Pro))
                        text += "You already have **Pro**!\n";
                    else
                    {
                        await PremiumDb.AddPremium(user.Id, ProType.Pro);
                        text += "**Pro** activated!\n";
                        log = true;
                    }
                }
            }
            if (text == "")
                text += $"You have no user premium... Try `{Program.GetPrefix(Context)}donate`";

            await ReplyAsync(text);
            if (log)
            {
                await WebhookClients.PremiumLogChannel.SendMessageAsync(embeds: new List<Embed>
                    {
                        new EmbedBuilderPrepared(Context.User)
                            .WithDescription($"{Context.User.Mention} `{Context.User.Id}`\n{text}")
                            .WithFooter(System.DateTime.Now.ToLongDateString())
                            .Build()
                    });
            }
        }

        [Command("RedeemCode"), Alias("Redeem"), Description("Redeem a premium trial code.\n**Usage**: `!redeem [code]`")]
        [SlashCommand("redeem-code", "Redeem a Pro or Pro Guild code.")]
        public async Task RedeemCode(string code)
        {
            Premium res = await PremiumCodeDb.RedeemCode(code, Context.User.Id, Context.Guild.Id);

            await ReplyAsync(embed: new EmbedBuilderPrepared(Context.User).WithDescription($"**{res.Type}** activated until {res.ExpiresAt.ToShortDateString()}").Build());

            await WebhookClients.CodeRedeemChannel.SendMessageAsync(embeds: new List<Embed>
            {
                new EmbedBuilderPrepared(Context.User)
                    .WithDescription($"{Context.User.Mention} `{Context.User.Id}`\n**{res.Type}** activated until {res.ExpiresAt.ToString("yyyy-MM-dd")} with code {code}")
                    .WithFooter(System.DateTime.Now.ToLongDateString())
                    .Build()
            });
        }
        
        

        [Command("Pro"), Alias("Premium", "Support", "Patreon", "Paypal", "Donate"), Description("Donation Links.")]
        [SlashCommand("pro", "Info about pro features")]
        public async Task Donate()
        {
            await ReplyAsync("", false, BasicUtil.DonateEmbed(Program.GetPrefix(Context)).Build());
        }

        [Command("InfoPro"), Description("Donation Links.")]
        public async Task InfoPro()
        {
            string prefix = Program.GetPrefix(Context);
            var eb = new EmbedBuilder()
                .WithAuthor(Program.GetClient().CurrentUser)
                .WithDescription($"Namiko Pro Upgrades :star:\n" +
                    $"\n" +
                    $"- Waifus from your wishlist have a much higher chance to appear in the `{prefix}waifushop`\n" +
                    $"- Much higher chance to find waifus from your wishlist in your lootboxes\n" +
                    $"- Get upgraded lootboxes for voting: chance to get **T1** and **T2** waifus, more toasties.\n" +
                    $"- Marry up to **3** users\n" +
                    $"\n" +
                    $":star: Subscribe on [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info-pro")})")
                .WithImageUrl("https://i.imgur.com/NIAP5QC.png")
                .WithFooter("-What are you? Twelve?")
                .WithColor(BasicUtil.RandomColor());

            await ReplyAsync("", false, eb.Build());
        }

        [Command("InfoProPlus"), Description("Donation Links.")]
        public async Task InfoProPlus()
        {
            string prefix = Program.GetPrefix(Context);
            var eb = new EmbedBuilder()
                .WithAuthor(Program.GetClient().CurrentUser)
                .WithDescription($"Namiko Pro+ Upgrades :star2:\n" +
                    $"\n" +
                    $"- All upgrades in Pro\n" +
                    $"- **Halved** `{prefix}daily` cooldown in all servers\n" +
                    $"- Get a **T1 Waifu** Lootbox when using `{prefix}weekly`\n" +
                    $"- Request a **custom waifu** that is only obtainable by you (Stays after premium ends)\n" +
                    $"- Waifu **wishlist** limit increased to **12**\n" +
                    $"- Marry up to **10** users\n" +
                    $"- Behind the scenes access in *Namiko Test Realm*\n" +
                    $"\n" +
                    $":star: Subscribe on [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info-proplus")})")
                .WithImageUrl("https://i.imgur.com/uq9eip4.png")
                .WithFooter("-What are you? Twelve?")
                .WithColor(BasicUtil.RandomColor());

            await ReplyAsync("", false, eb.Build());
        }

        [Command("InfoGuild"), Description("Donation Links.")]
        public async Task InfoGuild()
        {
            string prefix = Program.GetPrefix(Context);
            var eb = new EmbedBuilder()
                .WithAuthor(Program.GetClient().CurrentUser)
                .WithDescription($"Namiko Pro Guild Upgrades :star:\n" +
                    $"\n" +
                    $"- **Music** with a lot of features! Try `{prefix}join`\n" +
                    $"- Halved `{prefix}weekly` cooldown\n" +
                    $"- **3** times more waifus in waifu shop and gacha shop\n" +
                    $"- Subreddit follow limit increased to **5**\n" +
                    $"\n" +
                    $":star: Subscribe on [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info-guild")})")
                .WithImageUrl("https://i.imgur.com/XxFJ1gH.png")
                .WithFooter("-What are you? Twelve?")
                .WithColor(BasicUtil.RandomColor());

            await ReplyAsync("", false, eb.Build());
        }

        [Command("InfoGuildPlus"), Description("Donation Links.")]
        public async Task InfoGuildPlus()
        {
            var eb = new EmbedBuilder()
                .WithAuthor(Program.GetClient().CurrentUser)
                .WithDescription($"Namiko Pro Guild+ Upgrades :star2:\n" +
                    $"\n" +
                    $"- All upgrades in Guild\n" +
                    $"- Music queue limit increased from **100** to **500**\n" +
                    $"- A new waifu shop that can be controlled by your mods\n" +
                    $"- Waifu **lootboxes** will be up for purchase for users\n" +
                    $"- Create **reaction image commands** exclusive to your server (Stays after premium ends)\n" +
                    $"- Weekly increased by **1000** for everyone\n" +
                    $"- Subreddit follow limit increased to **10**\n" +
                    $"\n" +
                    $":star: Subscribe on [Patreon]({LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-info-guildplus")})")
                .WithImageUrl("https://i.imgur.com/f8XYQxU.png")
                .WithFooter("-What are you? Twelve?")
                .WithColor(BasicUtil.RandomColor());

            await ReplyAsync("", false, eb.Build());
        }
    }
}