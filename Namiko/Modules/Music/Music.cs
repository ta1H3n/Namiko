﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Namiko.Addons.Handlers;
using Namiko.Addons.Handlers.Dialogue;
using Namiko.Addons.Handlers.Paginator;
using Namiko.Handlers.Attributes;
using Namiko.Handlers.Attributes.Preconditions;
using Namiko.Modules.Basic;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using Namiko.Handlers.Services;
using Victoria;
using Victoria.Decoder;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Resolvers;

namespace Namiko
{
    [RequireGuild]
    public class Music : CustomModuleBase<ICustomContext>
    {
        public BaseSocketClient Client { get; set; }
        public MusicService MusicService { get; set; }
        private LavaPlayer Player { get => MusicService.Node.GetPlayer(Context.Guild); }


        [PermissionRole(RoleType.Music)]
        [Command("Join"), Alias("Music"), Description("Namiko joins your voice channel.\n**Usage**: `!join`")]
        [SlashCommand("join", "Namiko joins your voice channel")]
        public async Task Join()
        {
            if (!(PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus) || PremiumDb.IsPremium(Context.Guild.Id, ProType.Guild)))
            {
                await ReplyAsync($"Gomen, Senpai, Music is currently limited to my Pro Guilds :star2:\n" +
                    $"\n" +
                    $"• Play music from any of these sources: youtube, soundcloud, bandcamp, twitch, vimeo, mixer and any http stream, such as radio stations!\n" +
                    $"• Save and load your playlist directly in/from my database!\n" +
                    $"• Up to 500 song queues!\n" +
                    $"• Repeat songs, loop and shuffle playlists!\n" +
                    $"• Set the volume!\n" +
                    $"• Limit control of the player to roles of your choice!\n" +
                    $"• Look up lyrics of the playing song!\n" +
                    $"• And... I... I will talk to you in voice chat~ <:Awooo:582888496793124866>\n" +
                    $"\n" +
                    $"Type `{GetPrefix()}pro` for more info! Get all these features and more from 5$/month!\n" +
                    $"Or join my [Support Server]({LinkHelper.SupportServerInvite}) and try!");
                return;
            }

            if (!MusicService.Node.IsConnected)
            {
                await ReplyAsync("I'm not connected to Lavalink, please try again in a few seconds...");
                return;
            }

            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You need to join a voice channel first, Senpai!");
                return;
            }

            var player = Player;
            var current = (Context.Guild.CurrentUser as IGuildUser)?.VoiceChannel;

            var perms = Context.Guild.CurrentUser.GetPermissions(user.VoiceChannel);
            if (!perms.Connect)
            {
                await ReplyAsync($"I don't have the permission to **Connect** to **{user.VoiceChannel.Name}**, Senpai...\n");
            }
            if (!perms.Speak)
            {
                await ReplyAsync($"I don't have the permission to **Speak** in **{user.VoiceChannel.Name}**, Senpai...\n");
            }

            if (player == null)
            {
                if (current != null)
                    await current.DisconnectAsync();
                player = await MusicService.Node.JoinAsync(user.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Hellooo~ I joined **{user.VoiceChannel.Name}** <:NekoHi:620711213826834443>");
                await player.PlayLocal("join", MusicService.Node);
                return;
            }

            if (user.VoiceChannel == player.VoiceChannel && user.VoiceChannel == current)
            {
                await ReplyAsync($"I'm already in **{user.VoiceChannel.Name}**, Senpai...");
                return;
            }

            await MusicService.Node.MoveChannelAsync(user.VoiceChannel);
            await ReplyAsync($"Moving over to **{user.VoiceChannel.Name}**");
        }

        [PermissionRole(RoleType.Music)]
        [Command("Leave"), Description("Namiko leaves your voice channel.\n**Usage**: `!leave`")]
        [SlashCommand("leave", "Namiko leaves your voice channel")]
        public async Task Leave()
        {
            var player = Player;
            var current = (Context.Guild.CurrentUser as IGuildUser)?.VoiceChannel;

            if (player == null && current == null)
            {
                await ReplyAsync($"I'm not connected to a voice channel, Senpai...");
                return;
            }

            if (player != null && current != null && player?.VoiceChannel == current)
            {
                if (await player.PlayLocal("leave", MusicService.Node))
                {
                    await Context.TriggerTypingAsync();
                    await Task.Delay(2000);
                }
            }

            if (player != null)
            {
                await player.DisposeAsync();
                MusicService.Node._playerCache.TryRemove(Context.Guild.Id, out _);
            }

            if (current != null)
                await current.DisconnectAsync();

            await ReplyAsync($"Bye bye! <:NekoHi:620711213826834443>");
            return;
        }
        
        
        public enum PlayOption { 
            [ChoiceDisplay("Start of queue")] ToStartOfQueue, 
            [ChoiceDisplay("End of queue")] ToEndOfQueue, 
            [ChoiceDisplay("Auto pick first search result")] PickFirstResult }

        [SlashCommand("play", "Play a song/playlist or add it to the end of a queue")]
        public Task Play(string song, PlayOption option = PlayOption.ToEndOfQueue) => option switch
        {
            PlayOption.PickFirstResult => PlayFirst(song),
            PlayOption.ToEndOfQueue => Play(song),
            PlayOption.ToStartOfQueue => PlayNext(song)
        };

        [PermissionRole(RoleType.Music)]
        [Command("Play"), Alias("p"), Description("Play a song/playlist or add it to the end of a queue.\n**Usage**: `!play [link_or_search]`")]
        public async Task Play([Remainder]string query)
        {
            if (!(PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus) || PremiumDb.IsPremium(Context.Guild.Id, ProType.Guild)))
            {
                await ReplyAsync($"Gomen, Senpai, Music is currently limited to my Pro Guilds :star2:\n" +
                    $"\n" +
                    $"• Play music from any of these sources: youtube, soundcloud, bandcamp, twitch, vimeo, mixer and any http stream, such as radio stations!\n" +
                    $"• Save and load your playlist directly in/from my database!\n" +
                    $"• Up to 500 song queues!\n" +
                    $"• Repeat songs, loop and shuffle playlists!\n" +
                    $"• Set the volume!\n" +
                    $"• Limit control of the player to roles of your choice!\n" +
                    $"• Look up lyrics of the playing song!\n" +
                    $"• And... I... I will talk to you in voice chat~ <:Awooo:582888496793124866>\n" +
                    $"\n" +
                    $"Type `{GetPrefix()}pro` for more info! Get all these features and more from 5$/month!\n" +
                    $"Or join my [Support Server]({LinkHelper.SupportServerInvite}) and try!");
                return;
            }

            var player = GetPlayer();
            if (player == null)
            {
                await ReplyAsync($"I'm not in a voice channel... Type `{GetPrefix()}join` to invite me to yours!");
                return;
            }

            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You're not in a voice channel... Baaaaka.");
                return;
            }

            if (player.VoiceChannel != user.VoiceChannel)
            {
                await ReplyAsync("We're not in the same voice channel, Senpai.");
                return;
            }

            int max = 100;
            if (PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
                max = 500;
            if (player.Queue.Count >= max)
            {
                await ReplyAsync($"Playlist size is limited to **{max}**, Senpai.");
                return;
            }

            await Context.TriggerTypingAsync();
            var tracks = await MusicService.Node.SearchAndSelect(query, this, max);
            if (tracks == null)
                return;

            if (tracks.Count > 1)
            {
                int amount = max - player.Queue.Count;
                amount = amount > tracks.Count ? tracks.Count : amount;
                player.Queue.EnqueueRange(tracks.Take(amount).Select(t => { t.User = Context.User; return t; } ));
                await ReplyAsync($"Queued **{amount}** tracks :musical_note:");
                if (player.PlayerState != PlayerState.Playing)
                {
                    await PlayNext(player);
                }
                return;
            }

            var track = tracks.First();
            track.User = Context.User;
            await AddTrack(track, player);
        }

        [PermissionRole(RoleType.Music)]
        [Command("PlayNext"), Alias("pn"), Description("Play a song/playlist or add it to the start of a queue.\n**Usage**: `!playnext [link_or_search]`")]
        public async Task PlayNext([Remainder]string query)
        {
            if (!(PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus) || PremiumDb.IsPremium(Context.Guild.Id, ProType.Guild)))
            {
                await ReplyAsync($"Gomen, Senpai, Music is currently limited to my Pro Guilds :star2:\n" +
                    $"\n" +
                    $"• Play music from any of these sources: youtube, soundcloud, bandcamp, twitch, vimeo, mixer and any http stream, such as radio stations!\n" +
                    $"• Save and load your playlist directly in/from my database!\n" +
                    $"• Up to 500 song queues!\n" +
                    $"• Repeat songs, loop and shuffle playlists!\n" +
                    $"• Set the volume!\n" +
                    $"• Limit control of the player to roles of your choice!\n" +
                    $"• Look up lyrics of the playing song!\n" +
                    $"• And... I... I will talk to you in voice chat~ <:Awooo:582888496793124866>\n" +
                    $"\n" +
                    $"Type `{GetPrefix()}pro` for more info! Get all these features and more from 5$/month!\n" +
                    $"Or join my [Support Server]({LinkHelper.SupportServerInvite}) and try!");
                return;
            }

            var player = GetPlayer();
            if (player == null)
            {
                await ReplyAsync($"I'm not in a voice channel... Type `{GetPrefix()}join` to invite me to yours!");
                return;
            }

            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You're not in a voice channel... Baaaaka.");
                return;
            }

            if (player.VoiceChannel != user.VoiceChannel)
            {
                await ReplyAsync("We're not in the same voice channel, Senpai.");
                return;
            }

            int max = 100;
            if (PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
                max = 500;
            if (player.Queue.Count >= max)
            {
                await ReplyAsync($"Playlist size is limited to **{max}**, Senpai.");
                return;
            }

            await Context.TriggerTypingAsync();
            var tracks = await MusicService.Node.SearchAndSelect(query, this, max);
            if (tracks == null)
                return;

            if (tracks.Count > 1)
            {
                int amount = max - player.Queue.Count;
                amount = amount > tracks.Count ? tracks.Count : amount;
                player.Queue.EnqueueRange(tracks.Take(amount).Select(t => { t.User = Context.User; return t; }));
                await ReplyAsync($"Queued **{amount}** tracks :musical_note:");
                if (player.PlayerState != PlayerState.Playing)
                {
                    await player.SkipAsync();
                    await NowPlaying();
                }
                return;
            }

            var track = tracks.First();
            track.User = Context.User;

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                player.Queue.EnqueueFirst(track);
                await ReplyAsync(embed: new EmbedBuilderLava(Context.User)
                    .WithAuthor("Next Up", track.User?.GetAvatarUrl(), track.Url.ToString())
                    .WithDescription($"{track.Title} :musical_note:")
                    .Build());
                return;
            }
            else
            {
                await player.PlayAsync(track);
                await ReplyAsync(embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
                return;
            }
        }

        [PermissionRole(RoleType.Music)]
        [Command("QuickPlay"), Alias("qp"), Description("Play a song/playlist or add it to the end of a queue. Automatically selects the first result from the search.\n**Usage**: `!qp [link_or_search]`")]
        public async Task PlayFirst([Remainder]string query)
        {
            if (!(PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus) || PremiumDb.IsPremium(Context.Guild.Id, ProType.Guild)))
            {
                await ReplyAsync($"Gomen, Senpai, Music is currently limited to my Pro Guilds :star2:\n" +
                    $"\n" +
                    $"• Play music from any of these sources: youtube, soundcloud, bandcamp, twitch, vimeo, mixer and any http stream, such as radio stations!\n" +
                    $"• Save and load your playlist directly in/from my database!\n" +
                    $"• Up to 500 song queues!\n" +
                    $"• Repeat songs, loop and shuffle playlists!\n" +
                    $"• Set the volume!\n" +
                    $"• Limit control of the player to roles of your choice!\n" +
                    $"• Look up lyrics of the playing song!\n" +
                    $"• And... I... I will talk to you in voice chat~ <:Awooo:582888496793124866>\n" +
                    $"\n" +
                    $"Type `{GetPrefix()}pro` for more info! Get all these features and more from 5$/month!\n" +
                    $"Or join my [Support Server]({LinkHelper.SupportServerInvite}) and try!");
                return;
            }

            var player = GetPlayer();
            if (player == null)
            {
                await ReplyAsync($"I'm not in a voice channel... Type `{GetPrefix()}join` to invite me to yours!");
                return;
            }

            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You're not in a voice channel... Baaaaka.");
                return;
            }

            if (player.VoiceChannel != user.VoiceChannel)
            {
                await ReplyAsync("We're not in the same voice channel, Senpai.");
                return;
            }

            int max = 100;
            if (PremiumDb.IsPremium(Context.Guild.Id, ProType.GuildPlus))
                max = 500;
            if (player.Queue.Count >= max)
            {
                await ReplyAsync($"Playlist size is limited to **{max}**, Senpai.");
                return;
            }

            await Context.TriggerTypingAsync();
            var res = await MusicService.Node.SearchYouTubeAsync(query);
            if (res.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync("*~ No Results ~*", Color.DarkRed.RawValue);
                return;
            }
            if (res.LoadStatus == LoadStatus.LoadFailed)
            {
                await ReplyAsync("*Coughing blood*. I-I... failed looking it up... M-mind trying again, Senpai?", Color.DarkRed.RawValue);
                return;
            }

            var track = res.Tracks.FirstOrDefault();
            track.User = Context.User;
            await AddTrack(track, player);
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Skip"), Description("Skip the current song.\n**Usage**: `!skip`")]
        [SlashCommand("skip", "skips the current playing song")]
        public async Task Skip()
        {
            var player = Player;

            if (player.PlayerState != PlayerState.Playing)
            {
                await PlayNext(player);
                return;
            }

            if (player.Queue.Count == 0)
            {
                await player.StopAsync();
                await player.PlayLocal("skip", MusicService.Node);
                await ReplyAsync("The queue is empty, Senpai.");
                return;
            }

            var track = await player.SkipAsync();
            await ReplyAsync(embed: (await MusicUtil.NowPlayingEmbed(Player, true)).Build());

            if (player.Loop)
            {
                track.WithPosition(new TimeSpan(0));
                player.Queue.Enqueue(track);
            }
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Remove"), Description("Remove a song/songs in the specified position from the queue. Works with a range.\n**Usage**: `!remove [from] [to]`")]
        [SlashCommand("remove", "Remove a song/songs in the specified position from the queue. Works with a range.")]
        public async Task Remove(int from, int to = -1)
        {
            var player = Player;
            if (player?.Queue == null)
            {
                await ReplyAsync("I have nothing to skip, Senpai.");
                return;
            }

            if (to != -1)
            {
                if (from > to)
                {
                    int num = from;
                    from = to;
                    to = num;
                }
                if (from < 1)
                    from = 1;
                if (to > player.Queue.Count)
                    to = player.Queue.Count;

                player.Queue.RemoveRange(from-1, to-1);
                await ReplyAsync($"Removed songs **{from}-{to}** from the playlist. <a:Cleaning:621047051999903768>");
                return;
            }

            if (from < 1 || player.Queue.Count < from)
            {
                await ReplyAsync($"There is no song in position **{from}**, baaaka.");
                return;
            }

            player.Queue.RemoveAt(from-1);
            await ReplyAsync($"Removed the song at position {from} from the playlist. <a:Cleaning:621047051999903768>");
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Pause"), Description("Pauses music playback.\n**Usage**: `!pause`")]
        [SlashCommand("pause", "Pause music player")]
        public async Task Pause()
        {
            var player = Player;
            if (player is null)
            {
                await ReplyAsync("There is nothing to pause.");
                return;
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                await player.PauseAsync();
                await ReplyAsync("Pausing...");
                return;
            }
            else
            {
                await player.ResumeAsync();
                await ReplyAsync("Resuming...");
                return;
            }
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Resume"), Description("Resumes music playback.\n**Usage**: `!resume`")]
        [SlashCommand("resume", "Resume music player")]
        public async Task Resume()
        {
            var player = Player;
            if (player is null)
            {
                await ReplyAsync("There is nothing to resume.");
                return;
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                await ReplyAsync("It's not paused, Senpai...");
                return;
            }
            else
            {
                await player.ResumeAsync();
                await ReplyAsync("Resuming...");
                return;
            }
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Repeat"), Description("Repeats the current song.\n**Usage**: `!repeat`")]
        [SlashCommand("repeat", "Play the current song on repeat on/off")]
        public async Task Repeat()
        {
            var player = Player;
            if (player?.Track == null)
            {
                await ReplyAsync("There is nothing to repeat, Senpai.");
                return;
            }

            var track = player.Track;

            if (player.Repeat)
            {
                player.Repeat = false;
                await ReplyAsync("▷ I'll stop repeating this track. Finally got tired of it, Senpai?");
                return;
            }

            player.Repeat = true;
            await ReplyAsync($":repeat_one: Repeating *{track.Title}*.\nHere we go again...");
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Loop"), Description("Loops the current playlist.\n**Usage**: `!loop`")]
        [SlashCommand("loop", "Loop the playlist on/off")]
        public async Task Loop()
        {
            var player = Player;
            if (player == null)
            {
                await ReplyAsync("There is nothing to loop, Senpai.");
                return;
            }

            if (player.Loop)
            {
                player.Loop = false;
                await ReplyAsync("▷ Looping off...");
                return;
            }

            player.Loop = true;
            await ReplyAsync($":repeat: Looping the playlist!");
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Shuffle"), Description("Shuffles the playlist.\n**Usage**: `!shuffle`")]
        [SlashCommand("shuffle", "Shuffle the playlist")]
        public async Task Shuffle()
        {
            var player = Player;
            if (player?.Queue == null || player.Queue.Count <= 1)
            {
                await ReplyAsync("There is nothing to shuffle, Senpai...");
                return;
            }

            await Task.Run(() => player.Queue.Shuffle());
            await ReplyAsync("Playlist shuffled... My head is spinning :dizzy:");
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Volume"), Description("Sets the volume of the music playback, default is 40.\n**Usage**: `!volume [2-150]`")]
        [SlashCommand("volume", "Set player volume. Default - 40.")]
        public async Task Volume(ushort vol)
        {
            if (vol > 150 || vol < 2)
            {
                await ReplyAsync("Please use a number between 2 - 150");
                return;
            }
            string comment = "";

            if (vol < 10)
                comment = "Shhh... :zzz:";
            else if (vol > 140)
                comment = ":mega: You're crazy!";
            else if (vol > 80)
                comment = "Bring it on! <:Awooo:582888496793124866>";

            await Player.UpdateVolumeAsync(vol);
            await ReplyAsync($"Volume set to: **{vol}**\n{comment}");
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Seek"), Description("Seeks to a specific part of the current song.\n**Usage**: `!seek [timestamp, e.g. 01:30]`")]
        [SlashCommand("seek", "Seek to a part of song")]
        public async Task Seek([Discord.Interactions.Summary(description: "Timestamp - e.g. 01:30")] string timeStr, [Remainder]string str = "")
        {
            if (!TimeSpan.TryParseExact(timeStr, @"mm\:ss", null, out var time))
            {
                if (!TimeSpan.TryParseExact(timeStr, @"hh\:mm\:ss", null, out time))
                {
                    await ReplyAsync($"Couldn't parse the time. Try `mm:ss` or `hh:mm:ss` e.g. `{GetPrefix()}seek 01:30` or `{GetPrefix()}seek 01:15:30`");
                    return;
                }
            }

            var player = Player;
            if (player?.Track == null)
            {
                await ReplyAsync("There is nothing to seek, Senpai...");
                return;
            }

            if (player.Track.IsStream)
            {
                await ReplyAsync("The current track is not seekable, Senpai...");
                return;
            }

            if (time > player.Track.Duration)
            {
                await ReplyAsync($"Your selected time is greater than the track length, baaaka.");
                return;
            }

            await player.SeekAsync(time);
            string format = time.TotalSeconds > 3600 ? @"hh\:mm\:ss" : @"mm\:ss";
            await ReplyAsync($"Moving to {time.ToString(format)}");
        }

        [Command("NowPlaying"), Alias("Playing", "np"), Description("Shows the current playing song.\n**Usage**: `!np`")]
        [SlashCommand("now-playing", "Currently playing song")]
        public async Task NowPlaying()
        {
            if (Player?.Track == null)
            {
                await ReplyAsync("I'm not playing anything, Senpai.");
                return;
            }

            await ReplyAsync(embed: (await MusicUtil.NowPlayingEmbed(Player, true)).Build());
        }

        [Command("Queue"), Description("Lists all the songs currently in queue.\n**Usage**: `!queue`")]
        [SlashCommand("queue", "Show the song queue")]
        public async Task Queue()
        {
            var player = Player;
            if (player?.Queue == null || player.Queue.Count == 0)
            {
                await ReplyAsync("The song queue is empty. We should change that.");
                return;
            }

            if (player.Queue.Count < 15)
            {
                await ReplyAsync(embed: MusicUtil.TrackListEmbed(player.Queue.Items as IEnumerable<LavaTrack>, Context.User, player.Loop).Build());
                return;
            }

            var msg = new PaginatedMessage();
            msg.Author = new EmbedAuthorBuilder
            {
                Name = $"Song Queue",
                IconUrl = Context.User.GetAvatarUrl(),
                Url = LinkHelper.GetRedirectUrl(LinkHelper.Patreon, "Patreon", "cmd-embed-queue")
            };
            var pages = PaginatedMessage.PagesArray(player.Queue.Items, 10, x => $"[{x.Title.ShortenString(70, 65)}]({x.Url})\n");
            if (player.Loop)
                msg.Title = "Looping Playlist - :repeat:";

            msg.Pages = pages;
            msg.Footer = $"Volume: {player.Volume} ⚬ Powered by: 🌋 Victoria - Lavalink ⚬ ";

            await PagedReplyAsync(msg);
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("Clear"), Description("Clears the queue.\n**Usage**: `!clear`")] 
        [SlashCommand("clear", "Clear the song queue")]
        public async Task Clear()
        {
            var player = Player;
            if (player?.Queue == null || player.Queue.Count == 0)
            {
                await ReplyAsync("There is nothing to clear, Senpai...");
                return;
            }

            Player.Queue.Clear();
            await ReplyAsync("Queue cleared... <a:Cleaning:621047051999903768>");
        }

        [PermissionRole(RoleType.Music)]
        [Command("SavePlaylist"), Description("Saves the current playlist to be loaded later.\n**Usage**: `!saveplaylist [name]`")]
        [SlashCommand("playlist-save", "Saves the current queue to a playlist.")]
        public async Task SavePlaylist([Remainder]string name)
        {
            var player = Player;
            if (player == null || player.Queue.Count <= 0)
            {
                await ReplyAsync("There is nothing to save, Senpai...");
                return;
            }

            if (MusicDb.IsPlaylist(name, Context.Guild.Id))
            {
                await ReplyAsync($"There already is a playlist called **{name}**.\n" +
                    $"Remove it with `{GetPrefix()}DeletePlaylist`");
                return;
            }

            var tracks = new List<LavaTrack>();
            if (player.Track != null)
                tracks.Add(player.Track);

            tracks.AddRange(player.Queue.Items as IEnumerable<LavaTrack>);
            var playlist = new Playlist
            {
                GuildId = Context.Guild.Id,
                Name = name,
                UserId = Context.User.Id
            };

            var playlistTracks = new List<Track>();
            foreach (var x in tracks)
            {
                try
                {
                    playlistTracks.Add(new Track
                    {
                        Playlist = playlist,
                        SongHash = x.Hash,
                        UserId = x.User.Id
                    });
                } catch { }
            }

            playlist.Tracks = playlistTracks;

            var res = await MusicDb.AddPlaylist(playlist);
            Console.WriteLine($"{res} rows affected.");
            await ReplyAsync($"Playlist **{playlist.Name}** with **{playlist.Tracks.Count}** tracks has been saved!");
        }

        [Command("Playlists"), Description("Lists all your saved playlists.\n**Usage**: `!playlists`")]
        [SlashCommand("playlists", "Show all playlists")]
        public async Task Playlists()
        {
            var playlists = await MusicDb.GetPlaylists(Context.Guild.Id);
            playlists.AddRange(await MusicDb.GetPlaylists(0));
            await ReplyAsync(embed: MusicUtil.PlaylistListEmbed(playlists, Context.User).Build());
        }

        [PermissionRole(RoleType.Music), PlayerChannel]
        [Command("LoadPlaylist"), Alias("lp"), Description("Loads a saved playlist to the queue.\n**Usage**: `!lp`")]
        [SlashCommand("playlist-load", "Load a playlist")]
        public async Task LoadPlaylist()
        {
            var player = Player;
            var playlists = await MusicDb.GetPlaylists(Context.Guild.Id);
            playlists.AddRange(await MusicDb.GetPlaylists(0));

            var playlist = await Select(playlists, "Playlist", MusicUtil.PlaylistListEmbed(playlists, Context.User, true).Build());
            if (playlist == null)
                return;

            playlist = await MusicDb.GetPlaylist(playlist.Id);
            var tracks = new List<LavaTrack>();

            if (player.Queue.Count == 0 || await Confirm($"Are you sure, senpai?\nLoading **{playlist.Name}** will overwrite the current queue."))
            {
                foreach (var x in playlist.Tracks)
                {
                    var track = TrackDecoder.Decode($"{x.SongHash}");
                    track.User = Context.Guild.GetUser(x.UserId);
                    tracks.Add(track);
                }
                player.Queue.EnqueueRange(tracks);
                await ReplyAsync($"Loaded **{playlist.Tracks.Count}** songs from **{playlist.Name}**");

                if (player.PlayerState != PlayerState.Playing && player.Queue.Count > 0)
                {
                    await player.PlayAsync(player.Queue.Dequeue() as LavaTrack);
                    await player.TextChannel.SendMessageAsync(embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
                }
            }
        }

        [PermissionRole(RoleType.Music)]
        [Command("DeletePlaylist"), Alias("dp"), Description("Deletes a saved playlist.\n**Usage**: `!dp`")]
        [SlashCommand("playlist-delete", "Delete a playlist")]
        public async Task DeletePlaylist()
        {
            var playlists = await MusicDb.GetPlaylists(Context.Guild.Id);

            var playlist = await Select(playlists, "Playlist", MusicUtil.PlaylistListEmbed(playlists, Context.User, true).Build());
            if (playlist == null)
                return;

            await MusicDb.DeletePlaylist(playlist.Id);
            await ReplyAsync($"**{playlist.Name}** deleted <:KannaSad:625348483968401419>");
            if (playlist.UserId != Context.User.Id)
            {
                var ch = await Client.GetUser(playlist.UserId).CreateDMChannelAsync();
                await ch.SendMessageAsync($"Your playlist ({playlist.Name}) in {Context.Guild.Name} has been deleted by {Context.User}");
            }
        }

        [PermissionRole(RoleType.Music), UserPermission(GuildPermission.Administrator)]
        [Command("SetMusicRole"), Alias("smr"), Description("Adds or removes a role that is required for controlling music.\n**Usage**: `!smr [role_name]`")]
        public async Task MusicRole(IRole role)
        {
            if (role == null)
            {
                return;
            }

            if (PermissionRoleDb.IsRole(role.Id, RoleType.Music))
            {
                await PermissionRoleDb.Delete(role.Id, RoleType.Music);
                await ReplyAsync($"Role **{role.Name}** removed from Music Roles. <:NadeYay:564880253382819860>");
                return;
            }
            else
            {
                await PermissionRoleDb.Add(role.Id, Context.Guild.Id, RoleType.Music);
                await ReplyAsync($"Users who have **{role.Name}** will now be able to control music. <:NadeYay:564880253382819860>");
                return;
            }
        }

        [Command("MusicRoles"), Alias("mr"), Description("Lists roles that are able to control music.\n**Usage**: `!mr`")]
        public async Task MusicRoles()
        {
            var dbRoles = PermissionRoleDb.GetAll(Context.Guild.Id, RoleType.Music);
            if (!dbRoles.Any())
            {
                await ReplyAsync("*~ No music roles ~*");
                return;
            }

            var desc = "";
            foreach (var r in dbRoles)
            {
                try
                {
                    var role = Context.Guild.GetRole(r.RoleId);
                    if (role != null)
                        desc += role.Mention + "\n";
                } catch { }
            }

            await ReplyAsync(desc);
        }

        // EVENTS

        // HELPERS

        public async Task AddTrack(LavaTrack track, LavaPlayer player)
        {
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                player.Queue.Enqueue(track);
                await ReplyAsync(embed: new EmbedBuilderLava(Context.User)
                    .WithAuthor("Track Queued", track.User?.GetAvatarUrl(), track.Url.ToString())
                    .WithDescription($"{track.Title} :musical_note:")
                    .Build());
                return;
            }
            else
            {
                await player.PlayAsync(track);
                await ReplyAsync(embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
                return;
            }
        }
        public async Task<IUserMessage> ReplyAsync(string msg, uint color = 0)
        {
            var eb = new EmbedBuilderLava(Context.User)
                .WithDescription(msg);

            if(color != 0)
            {
                eb.WithColor(color);
            }
            return await ReplyAsync(embed: eb.Build());
        }
        public LavaPlayer GetPlayer()
        {
            return MusicService.Node.GetPlayer(Context.Guild);
        }
        public SocketVoiceChannel GetVoiceChannel()
        {
            var user = Context.User as SocketGuildUser;
            return user.VoiceChannel;
        }
        public async Task PlayNext(LavaPlayer player)
        {
            if (player.Queue.TryDequeue(out var nextTrack) && (nextTrack is LavaTrack))
            {
                await player.PlayAsync(nextTrack);
                await NowPlaying();
            }
            else
            {
                await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava()
                    .WithDescription("The queue is empty, Senpai...")
                    .Build());
            }
        }

        // LOCAL FILES

        public async Task<LavaTrack> LocalTrackFirstOrDefaultAsync(string path)
        {
            var res = await MusicService.Node.SearchAsync(path);
            return res.Tracks.FirstOrDefault();
        }
    }
}