using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Model;
using Sentry;

namespace Namiko.Handlers.Services;

public class DiscordService
{
    public bool Development = true;
    public readonly DiscordShardedClient Client;
    public bool GuildLeaveEvent = true;

    private int _startup = 0;
    private int _foundLeftGuilds = 0;

    public DiscordService(DiscordShardedClient client, Logger logger)
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Production")
        {
            Development = false;
        }
        
        Client = client;
        
        Client.Log += logger.Console_Log;
        Client.ShardConnected += SetUp_FirstShardConnected;
        
        Client.ShardReady += MarkGuildsAsLeft;
        Client.ShardReady += CheckJoinedGuilds;
        
        Client.ShardConnected += Client_LogShardConnected;
        Client.ShardDisconnected += Client_LogShardDisconnected;
        Client.ShardReady += Client_LogShardReady;
        
        Client.JoinedGuild += NamikoJoinedGuild;
        Client.LeftGuild += NamikoLeftGuild;

        Client.ReactionAdded += AMFWT_VerificationReaction;

        if (!Development)
            Client.UserJoined += Client_UserJoinedWelcome;
        Client.UserJoined += Client_UserJoinedLog;
        Client.UserLeft += Client_UserLeftLog;
        Client.UserBanned += Client_UserBannedLog;
    }

    private async Task SetUp_FirstShardConnected(DiscordSocketClient arg)
    {
        // Making sure this part only runs once, unless an exception is thrown. Thread safe.
        if (Interlocked.Exchange(ref _startup, 1) == 0)
        {
            try
            {
                WebUtil.SetUpDbl(arg.CurrentUser.Id);
                await Client.SetActivityAsync(new Game($"with your waifu", ActivityType.Playing));
            }
            catch (Exception ex)
            {
                _startup = 0;
                SentrySdk.CaptureException(ex);
            }
        }
    }
    
    // EVENTS

    private async Task AMFWT_VerificationReaction(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        if (arg3.MessageId != 700399700196458546)
            return;

        SocketTextChannel sch = (await arg2.GetOrDownloadAsync()) as SocketTextChannel;
        var user = sch.Guild.GetUser(arg3.UserId);
        var role = sch.Guild.GetRole(697234413360119808);

        if (RoleUtil.HasRole(user, role))
            return;

        await user.AddRoleAsync(role);

        var chid = ServerDb.GetServer(sch.Guild.Id).WelcomeChannelId;
        var ch = sch.Guild.GetTextChannel(chid);
        await ch.SendMessageAsync(GetWelcomeMessageString(user));
    }
    private async Task Client_Log(LogMessage arg)
    {
        if (arg.Exception.Message.Contains("403") || arg.Exception.Message.Contains("500"))
            return;

        string shortdate = DateTime.Now.ToString("HH:mm:ss");
        string longdate = DateTime.Now.ToString();

        string exc = arg.Exception == null
            ? ""
            : $"\n`{arg.Exception.Message}- ` ```cs\n{arg.Exception.StackTrace ?? "..."}- ``` At: `{arg.Exception.TargetSite?.Name ?? "..."}- `";
        switch (arg.Severity)
        {
            case LogSeverity.Info:
                Console.WriteLine($"I3 {longdate} at {arg.Source}] {arg.Message}{exc}");
                break;
            case LogSeverity.Warning:
                Console.WriteLine($"W2 {longdate} at {arg.Source}] {arg.Message}{exc}");
                await WebhookClients.ErrorLogChannel.SendMessageAsync(
                    $":warning:`{shortdate}` - `{arg.Message}` s{exc}");
                break;
            case LogSeverity.Error:
                Console.WriteLine($"E1 {longdate} at {arg.Source}] {arg.Message}{exc}");
                await WebhookClients.ErrorLogChannel.SendMessageAsync(
                    $"<:TickNo:577838859077943306>`{shortdate}` - `{arg.Message}` {exc}");
                break;
            case LogSeverity.Critical:
                Console.WriteLine($"C0 {longdate} at {arg.Source}] {arg.Message}{exc}");
                await WebhookClients.ErrorLogChannel.SendMessageAsync(
                    $"<:TickNo:577838859077943306><:TickNo:577838859077943306>`{shortdate}` - `-{arg.Message}` {exc}");
                break;
            default:
                break;
        }

        if (arg.Severity == LogSeverity.Critical ||
            ((arg.Message.Contains("Connected") || arg.Message.Contains("Disconnected")) &&
             arg.Source.Contains("Shard")))
            await WebhookClients.NamikoLogChannel.SendMessageAsync(
                $":information_source:`{shortdate} {arg.Source}]` {arg.Message}{exc}");
    }
    private async Task Client_LogShardConnected(DiscordSocketClient arg)
    {
        Console.WriteLine($"{DateTime.Now} - Shard {arg.ShardId} Connected");
        _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
            $"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `Shard {arg.ShardId} Connected`");
    }
    private async Task Client_LogShardDisconnected(Exception arg1, DiscordSocketClient arg2)
    {
        try
        {
            if (arg1.Message.Equals("The operation has timed out."))
                return;

            await WebhookClients.NamikoLogChannel.SendMessageAsync(
                $"<:TickNo:577838859077943306> `{DateTime.Now.ToString("HH:mm:ss")}` - `Shard {arg2.ShardId} Disconnected` - `{arg1.Message}`");
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }
    private async Task Client_LogShardReady(DiscordSocketClient arg)
    {
        try
        {
            Console.WriteLine($"{DateTime.Now} - Shard {arg.ShardId} ready. {arg.Guilds.Count} guilds.");
            _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
                $":european_castle: `{DateTime.Now.ToString("HH:mm:ss")}` - `Shard {arg.ShardId} ready - {arg.Guilds.Count} guilds`");
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }
    
    
    private async Task MarkGuildsAsLeft(DiscordSocketClient arg)
    {
        // Making sure this part only runs once, unless an exception is thrown. Thread safe.
        if (Client.Shards.All(x => x.ConnectionState == ConnectionState.Connected) &&
            Interlocked.Exchange(ref _foundLeftGuilds, 1) == 0)
        {
            try
            {
                int res = await CheckLeftGuilds();
                if (res > 0)
                {
                    Console.WriteLine($"{DateTime.Now} - Left {res} guilds.");
                    _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
                        $"<:TickNo:577838859077943306> `{DateTime.Now.ToString("HH:mm:ss")}` - `Left {res} guilds`");
                }

                res = Client.Guilds.Count;
                Console.WriteLine($"{DateTime.Now} - Loaded {res} guilds.");
                _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
                    $"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `{res} guilds ready`");
            }
            catch (Exception ex)
            {
                _foundLeftGuilds = 0;
                SentrySdk.CaptureException(ex);
            }
        }
    }
    private async Task CheckJoinedGuilds(DiscordSocketClient shard)
    {
        try
        {
            IReadOnlyCollection<SocketGuild> guilds;
            if (shard == null)
                guilds = Client.Guilds;
            else
                guilds = shard.Guilds;

            var existingIds = ServerDb.GetNotLeft();
            var newIds = guilds.Where(x => !existingIds.Contains(x.Id)).Select(x => x.Id);

            int addedBal = await BalanceDb.AddNewServerBotBalance(newIds, Client.CurrentUser.Id);
            int added = await ServerDb.AddNewServers(newIds, AppSettings.DefaultPrefix);

            if (added > 0)
            {
                Console.WriteLine($"{DateTime.Now} - Joined {added} guilds.");
                _ = WebhookClients.NamikoLogChannel.SendMessageAsync(
                    $"<:TickYes:577838859107303424> `{DateTime.Now.ToString("HH:mm:ss")}` - `Joined {added} guilds`");
            }
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }

    // NAMIKO JOIN

    private async Task NamikoJoinedGuild(SocketGuild arg)
    {
        DateTime now = DateTime.Now;
        Server server = ServerDb.GetServer(arg.Id) ?? new Server
        {
            GuildId = arg.Id,
            JoinDate = now
        };
        server.LeaveDate = null;
        server.Prefix = AppSettings.DefaultPrefix;
        await ServerDb.UpdateServer(server);

        if (server.JoinDate.Equals(now))
        {
            await BalanceDb.SetToasties(Client.CurrentUser.Id, 1000000, arg.Id);
        }

        SocketTextChannel ch = arg.SystemChannel ?? arg.DefaultChannel;
        try
        {
            await ch?.SendMessageAsync("Hi! Please take good care of me!", false,
                BasicUtil.GuildJoinEmbed(Client).Build());
        }
        catch
        {
        }

        await WebhookClients.GuildJoinLogChannel.SendMessageAsync(
            $"<:TickYes:577838859107303424> {Client.CurrentUser.Username} joined `{arg.Id}` **{arg.Name}**.\nOwner: `{arg.Owner.Id}` **{arg.Owner}**");
    }
    private async Task NamikoLeftGuild(SocketGuild arg)
    {
        if (!GuildLeaveEvent)
            return;

        var server = ServerDb.GetServer(arg.Id);
        server.LeaveDate = DateTime.Now;
        await ServerDb.UpdateServer(server);

        await WebhookClients.GuildJoinLogChannel.SendMessageAsync(
            $"<:TickNo:577838859077943306> {Client.CurrentUser.Username} left `{arg.Id}` **{arg.Name}**.\nOwner: `{arg.Owner.Id}` **{arg.Owner}**");
    }

    // USER JOIN

    private async Task Client_UserJoinedWelcome(SocketGuildUser arg)
    {
        if (arg?.Guild?.Id == 417064769309245471)
            return;

        var server = ServerDb.GetServer(arg.Guild.Id);
        if (server != null && server.WelcomeChannelId != 0)
        {
            var ch = arg.Guild.GetTextChannel(server.WelcomeChannelId);
            if (ch != null)
                await ch.SendMessageAsync(GetWelcomeMessageString(arg));
        }
    }
    private async Task Client_UserBannedLog(SocketUser arg1, SocketGuild arg2)
    {
        var ch = GetJoinLogChannel(arg2);
        if (ch != null)
            await ch.SendMessageAsync($":hammer: {UserInfo(arg1)} was banned.");
    }
    private async Task Client_UserLeftLog(SocketGuild guild, SocketUser user)
    {
        var ch = GetJoinLogChannel(guild);
        if (ch != null)
            await ch.SendMessageAsync($"<:TickNo:577838859077943306> {UserInfo(user)} left the server.");
    }
    private async Task Client_UserJoinedLog(SocketGuildUser arg)
    {
        var ch = GetJoinLogChannel(arg.Guild);
        if (ch != null)
            await ch.SendMessageAsync($"<:TickYes:577838859107303424> {UserInfo(arg)} joined the server.");
    }

    
    
    private string UserInfo(SocketUser user)
    {
        return $"`{user.Id}` {user} {user.Mention}";
    }
    private SocketTextChannel GetJoinLogChannel(SocketGuild guild)
    {
        return (SocketTextChannel)guild.GetChannel(ServerDb.GetServer(guild.Id).JoinLogChannelId);
    }
    
    
    private string GetWelcomeMessageString(SocketUser user)
    {
        string message = WelcomeMessageDb.GetRandomMessage();
        message = message.Replace("@_", user.Mention);
        return message;
    }
    private async Task<int> CheckLeftGuilds()
    {
        var guilds = Client.Guilds;
        HashSet<ulong> existingIds = new HashSet<ulong>(guilds.Select(x => x.Id));
        int left = 0;

        using (var db = new NamikoDbContext())
        {
            var zerotime = new DateTime(0);
            var now = DateTime.Now;
            IQueryable<Server> servers = db.Servers.AsQueryable().Where(x => x.LeaveDate == null && !existingIds.Contains(x.GuildId));

            await servers.ForEachAsync(x => x.LeaveDate = now);

            left = servers.Count();
            db.UpdateRange(servers);
            await db.SaveChangesAsync();
        }

        return left;
    }
}