using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace Namiko.Handlers.Services;

public class MusicService
{
    public readonly LavaNode Node;
    public readonly HashSet<LavaPlayer> ReconnectPlayer;

    private readonly DiscordShardedClient _client;

    public MusicService(DiscordShardedClient client, Logger logger)
    {
        _client = client;
        
        Node = new LavaNode(_client, new LavaConfig
        {
            SelfDeaf = true,
            DefaultVolume = 40,
            LogSeverity = LogSeverity.Info,
            Hostname = "127.0.0.1",
            Port = 2333,
            Authorization = "NamikoLove",
            EnableResume = true
        });

        ReconnectPlayer = new HashSet<LavaPlayer>();

        Node.OnLog += logger.Console_Log;
        Node.OnLog += LavaClient_Log;
        Node.OnTrackException += TrackException;
        Node.OnTrackStuck += TrackStuck;
        Node.OnTrackEnded += TrackEnded;
        Node.OnWebSocketClosed += WebSocketClosed;

        _client.ShardReady += Initialize;
        _client.ShardConnected += Shard_ReconnectPlayer;
        _client.UserVoiceStateUpdated += Client_UserVoiceChannel;
    }


    private async Task Initialize(DiscordSocketClient client)
    {
        if (!Node.IsConnected)
        {
            await Node.ConnectAsync();
        }
    }
    private async Task Shard_ReconnectPlayer(DiscordSocketClient arg)
    {
        if (!Node.IsConnected)
        {
            return;
        }
        
        var players = new List<LavaPlayer>(ReconnectPlayer);
        foreach (var player in players)
        {
            try
            {
                var guild = arg.Guilds.FirstOrDefault(x => x.Id == player.GuildId);
                if (guild == null)
                {
                    Console.WriteLine(
                        $"[LAVALINK] [{DateTime.Now.ToString("HH:mm:ss")}] Guild mismatch: {player.GuildId} not in shard {arg.ShardId}.");
                    break;
                }

                if (player == null || player.VoiceChannel == null)
                {
                    if (ReconnectPlayer.Contains(player))
                        ReconnectPlayer.Remove(player);

                    await WebhookClients.LavalinkChannel.SendMessageAsync(
                        $"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Reconnect `{guild.Id}` failed, cancelling...");
                    await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava()
                        .WithDescription(
                            "Gomen, Senpai... Failed to reconnect to voice channel. Use the join command to reinvite me.")
                        .Build());
                    break;
                }

                var current = guild.CurrentUser.VoiceChannel;
                if (current == null || player.VoiceChannel != current)
                {
                    if (ReconnectPlayer.Contains(player))
                        ReconnectPlayer.Remove(player);

                    await Node.MoveChannelAsync(player.VoiceChannel);
                    await WebhookClients.LavalinkChannel.SendMessageAsync(
                        $"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Succesfully reconnected to `{guild.Id}`");
                    await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava()
                        .WithDescription($"Reconnected player to **{player.VoiceChannel.Name}**")
                        .Build());
                    break;
                }

                else
                {
                    if (ReconnectPlayer.Contains(player))
                        ReconnectPlayer.Remove(player);

                    await WebhookClients.LavalinkChannel.SendMessageAsync(
                        $"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Already reconnected `{guild.Id}`, cancelling...");
                    break;
                }
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                    return;

                SentrySdk.CaptureException(ex);
                if (ReconnectPlayer.Contains(player))
                    ReconnectPlayer.Remove(player);

                await WebhookClients.LavalinkChannel.SendMessageAsync(
                    $"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Reconnect `{player.GuildId}` threw an exception.");
            }
        }
    }
    private async Task Client_UserVoiceChannel(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        if (user.IsBot)
            return;
        if (Node == null)
            return;
        if (user == null || !(user is SocketGuildUser))
            return;
        if (((SocketGuildUser)user).Guild == null)
            return;

        var player = Node.GetPlayer(((SocketGuildUser)user).Guild);
        if (player == null)
            return;

        if (after.VoiceChannel != null && after.VoiceChannel == player.VoiceChannel)
        {
            if (before.VoiceChannel == after.VoiceChannel)
                return;

            if (after.VoiceChannel.Users.Count == 2)
            {
                await player.PlayLocal("someonejoinalone", Node);
                return;
            }

            await player.PlayLocal("someonejoin", Node);
            return;
        }

        if (before.VoiceChannel == player.VoiceChannel && after.VoiceChannel != player.VoiceChannel)
        {
            await player.PlayLocal("someoneleft", Node);
            return;
        }
    }

    
    private async Task LavaClient_Log(LogMessage arg)
    {
        string message = $"🌋 `{arg.Severity}` `{DateTime.Now.ToString("HH:mm:ss")}` - `{arg.Message}`";
        await WebhookClients.LavalinkChannel.SendMessageAsync(message);
    }
    private async Task WebSocketClosed(WebSocketClosedEventArgs arg)
    {
        try
        {
            var player = Node.GetPlayer(arg.GuildId);

            if (arg.Code == 4014 && player != null && !ReconnectPlayer.Contains(player))
            {
                ReconnectPlayer.Add(player);
                await WebhookClients.LavalinkChannel.SendMessageAsync(
                    $"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Lost connection to `{arg.GuildId}`, queueing reconnect.");
            }
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
        }
    }
    private async Task StatsReceived(StatsEventArgs arg)
    {
        if (DateTime.Now.Minute % 10 != 0)
            return;
        if (arg.Players < 1)
            return;

        var eb = new EmbedBuilder();
        eb.WithColor(BasicUtil.RandomColor());
        eb.WithAuthor(DateTime.Now.ToString("HH:mm:ss"), _client.CurrentUser.GetAvatarUrl());
        eb.WithFooter($"🌋 Lavalink running for {Math.Round(arg.Uptime.TotalMinutes, 2)} minutes");

        eb.WithDescription($"Connected: {arg.Players}\nPlaying: {arg.PlayingPlayers}");
        eb.AddField("Memory",
            $"Allocated: {arg.Memory.Allocated / 1000000}MB\nUsed: {arg.Memory.Used / 1000000}MB\nFree: {arg.Memory.Free / 1000000}MB\nReservable: {arg.Memory.Reservable / 1000000}MB\n",
            true);
        eb.AddField("Frames", $"Sent: {arg.Frames.Sent}\nDeficit: {arg.Frames.Deficit}\nNulled: {arg.Frames.Nulled}",
            true);
        eb.AddField("Cpu",
            $"Cores: {arg.Cpu.Cores}\nSystem Load: {Math.Round(arg.Cpu.SystemLoad, 4) * 100}%\nLavalink Load: {Math.Round(arg.Cpu.LavalinkLoad, 4) * 100}%",
            true);

        await WebhookClients.LavalinkChannel.SendMessageAsync(embeds: new List<Embed> { eb.Build() });
    }
    private async Task TrackEnded(TrackEndedEventArgs arg)
    {
        var player = arg.Player;
        var track = arg.Track;
        var reason = arg.Reason;

        if (!reason.ShouldPlayNext())
            return;

        if (player.Repeat && !track.Url.ToString().Contains("VoiceLines") && reason != TrackEndReason.LoadFailed)
        {
            player.Queue.EnqueueFirst(track);
        }

        else if (player.Loop && !track.Url.ToString().Contains("VoiceLines") && reason != TrackEndReason.LoadFailed)
        {
            player.Queue.Enqueue(track);
        }

        if (!player.Queue.TryDequeue(out var nextTrack) || !(nextTrack is LavaTrack))
        {
            if (!track.Url.ToString().Contains("VoiceLines"))
            {
                await player.PlayLocal("empty", Node);
                await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava(track.User)
                    .WithDescription("There are no more tracks in the queue. We should fix that.")
                    .Build());
            }

            return;
        }

        await player.PlayAsync(nextTrack as LavaTrack);
        await player.TextChannel.SendMessageAsync(embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
    }
    private async Task TrackStuck(TrackStuckEventArgs arg)
    {
        var player = arg.Player;
        var track = arg.Track;

        if (player?.Track == track)
        {
            await player.SkipAsync();
            await player.TextChannel.SendMessageAsync(
                $"Track `{track.Title}` is stuck for `{arg.Threshold}ms`. Skipping...",
                embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
        }
    }
    private async Task TrackException(TrackExceptionEventArgs arg)
    {
        var player = arg.Player;

        if (player?.Track == arg.Track)
        {
            await player.SkipAsync();
            if (player.Queue.Count == 0)
            {
                await player.TextChannel.SendMessageAsync(
                    "Gomen, Senpai... *Coughs blood* ... The player broke! Try starting a new one?\n" +
                    $"Error: `{arg.ErrorMessage}`");
                await player.DisposeAsync();
            }
        }
    }
}