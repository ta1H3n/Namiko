using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Model;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Victoria;
using Victoria.Decoder;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Interfaces;
using Victoria.Resolvers;

namespace Namiko
{
    [RequireGuild]
    public class Music : InteractiveBase<ShardedCommandContext>
    {
        public static readonly LavaNode Node;
        public static readonly HashSet<LavaPlayer> ReconnectPlayer;
        private static DiscordShardedClient Client { get { return Program.GetClient(); } }

        private LavaPlayer Player { get => Node.GetPlayer(Context.Guild); }

        static Music()
        {
            Node = new LavaNode(Client, new LavaConfig
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

            Program.GetClient().ShardConnected += Shard_ReconnectPlayer;
            Node.OnLog += LavaClient_Log;
            Node.OnTrackException += TrackException;
            Node.OnTrackStuck += TrackStuck;
            Node.OnTrackEnded += TrackEnded;
            //Node.OnStatsReceived += StatsReceived;
            Node.OnWebSocketClosed += WebSocketClosed;
        }

        public static async Task<bool> Initialize(DiscordShardedClient client)
        {
            if (!Node.IsConnected)
            {
                await Node.ConnectAsync();
                return true;
            }
            return false;
        }

        [Command("Join"), Alias("Music"), Summary("Namiko joins your voice channel.\n**Usage**: `!join`"), PermissionRole(RoleType.Music)]
        public async Task Join([Remainder]string str = "")
        {
            if (!(PremiumDb.IsPremium(Context.Guild.Id, PremiumType.GuildPlus) || PremiumDb.IsPremium(Context.Guild.Id, PremiumType.Guild)))
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
                    $"Type `{Program.GetPrefix(Context)}pro` for more info! Get all these features and more from 5$/month!\n" +
                    $"Or join my [Support Server](https://discord.gg/W6Ru5sM) and try!");
                return;
            }

            if (!Node.IsConnected)
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
                player = await Node.JoinAsync(user.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Hellooo~ I joined **{user.VoiceChannel.Name}** <:NekoHi:620711213826834443>");
                await player.PlayLocal("join");
                return;
            }

            if (user.VoiceChannel == player.VoiceChannel && user.VoiceChannel == current)
            {
                await ReplyAsync($"I'm already in **{user.VoiceChannel.Name}**, Senpai...");
                return;
            }

            await Node.MoveChannelAsync(user.VoiceChannel);
            await ReplyAsync($"Moving over to **{user.VoiceChannel.Name}**");
        }

        [Command("Leave"), Summary("Namiko leaves your voice channel.\n**Usage**: `!leave`"), PermissionRole(RoleType.Music)]
        public async Task Leave([Remainder]string str = "")
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
                if (await player.PlayLocal("leave"))
                {
                    await Context.Channel.TriggerTypingAsync();
                    await Task.Delay(2000);
                }
            }

            if (player != null)
            {
                await player.DisposeAsync();
                Node._playerCache.TryRemove(Context.Guild.Id, out _);
            }

            if (current != null)
                await current.DisconnectAsync();

            await ReplyAsync($"Bye bye! <:NekoHi:620711213826834443>");
            return;
        }

        [Command("Play"), Alias("p"), Summary("Play a song/playlist or add it to the end of a queue.\n**Usage**: `!play [link_or_search]`"), PermissionRole(RoleType.Music)]
        public async Task Play([Remainder]string query)
        {
            if (!(PremiumDb.IsPremium(Context.Guild.Id, PremiumType.GuildPlus) || PremiumDb.IsPremium(Context.Guild.Id, PremiumType.Guild)))
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
                    $"Type `{Program.GetPrefix(Context)}pro` for more info! Get all these features and more from 5$/month!\n" +
                    $"Or join my [Support Server](https://discord.gg/W6Ru5sM) and try!");
                return;
            }

            var player = GetPlayer();
            if (player == null)
            {
                await ReplyAsync($"I'm not in a voice channel... Type `{Program.GetPrefix(Context)}join` to invite me to yours!");
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
            if (PremiumDb.IsPremium(Context.Guild.Id, PremiumType.GuildPlus))
                max = 500;
            if (player.Queue.Count >= max)
            {
                await ReplyAsync($"Playlist size is limited to **{max}**, Senpai.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();
            var tracks = await Node.SearchAndSelect(query, this, max);
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
                    await player.SkipAsync();
                    await NowPlaying();
                }
                return;
            }

            var track = tracks.First();
            track.User = Context.User;
            await AddTrack(track, player);
        }

        [Command("PlayNext"), Alias("pn"), Summary("Play a song/playlist or add it to the start of a queue.\n**Usage**: `!playnext [link_or_search]`"), PermissionRole(RoleType.Music)]
        public async Task PlayNext([Remainder]string query)
        {
            if (!(PremiumDb.IsPremium(Context.Guild.Id, PremiumType.GuildPlus) || PremiumDb.IsPremium(Context.Guild.Id, PremiumType.Guild)))
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
                    $"Type `{Program.GetPrefix(Context)}pro` for more info! Get all these features and more from 5$/month!\n" +
                    $"Or join my [Support Server](https://discord.gg/W6Ru5sM) and try!");
                return;
            }

            var player = GetPlayer();
            if (player == null)
            {
                await ReplyAsync($"I'm not in a voice channel... Type `{Program.GetPrefix(Context)}join` to invite me to yours!");
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
            if (PremiumDb.IsPremium(Context.Guild.Id, PremiumType.GuildPlus))
                max = 500;
            if (player.Queue.Count >= max)
            {
                await ReplyAsync($"Playlist size is limited to **{max}**, Senpai.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();
            var tracks = await Node.SearchAndSelect(query, this, max);
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

        [Command("QuickPlay"), Alias("qp"), Summary("Play a song/playlist or add it to the end of a queue. Automatically selects the first result from the search.\n**Usage**: `!qp [link_or_search]`"), PermissionRole(RoleType.Music)]
        public async Task PlayFirst([Remainder]string query)
        {
            if (!(PremiumDb.IsPremium(Context.Guild.Id, PremiumType.GuildPlus) || PremiumDb.IsPremium(Context.Guild.Id, PremiumType.Guild)))
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
                    $"Type `{Program.GetPrefix(Context)}pro` for more info! Get all these features and more from 5$/month!\n" +
                    $"Or join my [Support Server](https://discord.gg/W6Ru5sM) and try!");
                return;
            }

            var player = GetPlayer();
            if (player == null)
            {
                await ReplyAsync($"I'm not in a voice channel... Type `{Program.GetPrefix(Context)}join` to invite me to yours!");
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
            if (PremiumDb.IsPremium(Context.Guild.Id, PremiumType.GuildPlus))
                max = 500;
            if (player.Queue.Count >= max)
            {
                await ReplyAsync($"Playlist size is limited to **{max}**, Senpai.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();
            var res = await Node.SearchYouTubeAsync(query);
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

        [Command("Skip"), Summary("Skip the current song.\n**Usage**: `!skip`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Skip([Remainder]string str = "")
        {
            var player = Player;

            if (player?.Track == null)
            {
                await ReplyAsync("I have nothing to skip, Senpai.");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await player.StopAsync();
                await player.PlayLocal("skip");
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

        [Command("Remove"), Summary("Remove a song/songs in the specified position from the queue. Works with a range.\n**Usage**: `!remove [from] [to]`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Remove(int from, int to = -1, [Remainder]string str = "")
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

        [Command("Pause"), Summary("Pauses music playback.\n**Usage**: `!pause`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Pause([Remainder]string str = "")
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

        [Command("Resume"), Summary("Resumes music playback.\n**Usage**: `!resume`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Resume([Remainder]string str = "")
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

        [Command("Repeat"), Summary("Repeats the current song.\n**Usage**: `!repeat`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Repeat([Remainder]string str = "")
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

        [Command("Loop"), Summary("Loops the current playlist.\n**Usage**: `!loop`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Loop([Remainder]string str = "")
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

        [Command("Shuffle"), Summary("Shuffles the playlist.\n**Usage**: `!shuffle`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Shuffle([Remainder]string str = "")
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

        [Command("Volume"), Summary("Sets the volume of the music playback, default is 40.\n**Usage**: `!volume [2-150]`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Volume(ushort vol, [Remainder]string str = "")
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

        [Command("Seek"), Summary("Seeks to a specific part of the current song.\n**Usage**: `!seek [timestamp, e.g. 01:30]`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task Seek(string timeStr, [Remainder]string str = "")
        {
            if (!TimeSpan.TryParseExact(timeStr, @"mm\:ss", null, out var time))
            {
                if (!TimeSpan.TryParseExact(timeStr, @"hh\:mm\:ss", null, out time))
                {
                    await ReplyAsync($"Couldn't parse the time. Try `mm:ss` or `hh:mm:ss` e.g. `{Program.GetPrefix(Context)}seek 01:30` or `{Program.GetPrefix(Context)}seek 01:15:30`");
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

        [Command("NowPlaying"), Alias("Playing", "np"), Summary("Shows the current playing song.\n**Usage**: `!np`")]
        public async Task NowPlaying([Remainder]string str = "")
        {
           if (Player?.Track == null)
            {
                await ReplyAsync("I'm not playing anything, Senpai.");
                return;
            }

            await ReplyAsync(embed: (await MusicUtil.NowPlayingEmbed(Player, true)).Build());
        }

        [Command("Queue"), Summary("Lists all the songs currently in queue.\n**Usage**: `!queue`")]
        public async Task Queue([Remainder]string str = "")
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

            var msg = new CustomPaginatedMessage();
            msg.Author = new EmbedAuthorBuilder
            {
                Name = $"Song Queue",
                IconUrl = Context.User.GetAvatarUrl(),
                Url = BasicUtil._patreon
            };
            var pages = CustomPaginatedMessage.PagesArray(player.Queue.Items, 10, x => $"[{x.Title.ShortenString(70, 65)}]({x.Url})\n");
            if (player.Loop)
                msg.Title = "Looping Playlist - :repeat:";

            msg.Pages = pages;
            msg.PageCount = pages.Count();
            msg.Footer = $"Volume: {player.Volume} ⚬ Powered by: 🌋 Victoria - Lavalink ⚬ ";

            await PagedReplyAsync(msg);
        }

        [Command("Clear"), Summary("Cleares the queue.\n**Usage**: `!clear`"), PlayerChannel, PermissionRole(RoleType.Music)] 
        public async Task Clear([Remainder]string str = "")
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

        [Command("Lyrics"), Summary("Looks up the lyrics of the current song or searches if specified.\n**Usage**: `!lyrics [author] [title]`")]
        public async Task Lyrics(string author = "", [Remainder] string title = "")
        {
            var track = Player?.Track;
            if (track == null && (author == "" || title == ""))
            {
                await ReplyAsync("I'm not playing anything, Senpai.\n" +
                    "What song lyrics do you want me to look up? Example:\n" +
                    @"`!lyrics ""Kana Hanazawa"" ""Renai Circulation""`");
                return;
            }

            if (author == "")
                (author, title) = track.GetAuthorAndTitle();

            var lyrics = await LyricsResolver.SearchGeniusAsync(author, title);

            if (lyrics?.DefaultIfEmpty() == null)
            {
                lyrics = await LyricsResolver.SearchOVHAsync(author, title);
                if (lyrics?.DefaultIfEmpty() == null)
                {
                    await Context.Channel.SendMessageAsync($"Gomen, senpai. I can't find lyrics for `{author} - {title}`");
                    return;
                }
            }

            var lines = lyrics.Split('\n');
            string section = "";
            foreach(var line in lines)
            {
                if ((section.Length + line.Length) > 2047)
                {
                    await ReplyAsync(section);
                    section = "";
                }
                section += line + '\n';
            }
            await ReplyAsync(section);
        }

        [Command("SavePlaylist"), Summary("Saves the current playlist to be loaded later.\n**Usage**: `!saveplaylist [name]`"), PermissionRole(RoleType.Music)]
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
                    $"Remove it with `{Program.GetPrefix(Context)}DeletePlaylist`");
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

        [Command("Playlists"), Summary("Lists all your saved playlists.\n**Usage**: `!playlists`")]
        public async Task Playlists([Remainder]string str = "")
        {
            var playlists = await MusicDb.GetPlaylists(Context.Guild.Id);
            playlists.AddRange(await MusicDb.GetPlaylists(0));
            await ReplyAsync(embed: MusicUtil.PlaylistListEmbed(playlists, Context.User).Build());
        }

        [Command("LoadPlaylist"), Alias("lp"), Summary("Loads a saved playlist to the queue.\n**Usage**: `!lp`"), PlayerChannel, PermissionRole(RoleType.Music)]
        public async Task LoadPlaylist([Remainder]string str = "")
        {
            var player = Player;
            var playlists = await MusicDb.GetPlaylists(Context.Guild.Id);
            playlists.AddRange(await MusicDb.GetPlaylists(0));

            var playlist = await this.SelectItem(playlists, MusicUtil.PlaylistListEmbed(playlists, Context.User, true));
            if (playlist == null)
                return;

            playlist = await MusicDb.GetPlaylist(playlist.Id);
            var tracks = new List<LavaTrack>();
            if (player.Queue.Count > 0)
            {
                var load = new DialogueBoxOption();
                load.Action = async (IUserMessage message) =>
                {
                    player.Queue.Clear();
                    foreach (var x in playlist.Tracks)
                    {
                        var track = TrackDecoder.Decode($"{x.SongHash}");
                        track.User = Context.Guild.GetUser(x.UserId);
                        tracks.Add(track);
                    }
                    player.Queue.EnqueueRange(tracks);
                    await message.ModifyAsync(x => {
                        x.Embed = new EmbedBuilderLava(Context.User).WithDescription($"Loaded **{playlist.Tracks.Count}** songs from **{playlist.Name}**").Build();
                    });

                    if (player.PlayerState != PlayerState.Playing && player.Queue.Count > 0)
                    {
                        await player.PlayAsync(player.Queue.Dequeue() as LavaTrack);
                        await player.TextChannel.SendMessageAsync(embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
                    }
                };
                load.After = OnExecute.RemoveReactions;

                var cancel = new DialogueBoxOption();
                cancel.After = OnExecute.Delete;

                var dia = new DialogueBox();
                dia.Options.Add(Emote.Parse("<:TickYes:577838859107303424>"), load);
                dia.Options.Add(Emote.Parse("<:TickNo:577838859077943306>"), cancel);
                dia.Timeout = new TimeSpan(0, 1, 0);
                dia.Embed = new EmbedBuilderLava(Context.User)
                    .WithDescription($"Are you sure, senpai?\nLoading **{playlist.Name}** will overwrite the current queue.")
                    .Build();

                await DialogueReplyAsync(dia);
            }
            else
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

        [Command("DeletePlaylist"), Alias("dp"), Summary("Deletes a saved playlist.\n**Usage**: `!dp`"), PermissionRole(RoleType.Music)]
        public async Task DeletePlaylist([Remainder]string str = "")
        {
            var playlists = await MusicDb.GetPlaylists(Context.Guild.Id);

            var playlist = await this.SelectItem(playlists, MusicUtil.PlaylistListEmbed(playlists, Context.User, true));
            if (playlist == null)
                return;

            await MusicDb.DeletePlaylist(playlist.Id);
            await ReplyAsync($"**{playlist.Name}** deleted <:KannaSad:625348483968401419>");
            if (playlist.UserId != Context.User.Id)
            {
                var ch = await Program.GetClient().GetUser(playlist.UserId).GetOrCreateDMChannelAsync();
                await ch.SendMessageAsync($"Your playlist ({playlist.Name}) in {Context.Guild.Name} has been deleted by {Context.User}");
            }
        }

        [Command("SetMusicRole"), Alias("smr"), Summary("Adds or removes a role that is required for controlling music.\n**Usage**: `!smr [role_name]`"), CustomUserPermission(GuildPermission.Administrator)]
        public async Task MusicRole([Remainder]string roleName = "")
        {
            var role = await this.SelectRole(roleName);
            if (role == null)
            {
                return;
            }

            if (PermissionRoleDb.IsRole(role.Id, RoleType.Music))
            {
                await PermissionRoleDb.Delete(role.Id, RoleType.Music);
                await Context.Channel.SendMessageAsync($"Role **{role.Name}** removed from Music Roles. <:NadeYay:564880253382819860>");
                return;
            }
            else
            {
                await PermissionRoleDb.Add(role.Id, Context.Guild.Id, RoleType.Music);
                await Context.Channel.SendMessageAsync($"Users who have **{role.Name}** will now be able to control music. <:NadeYay:564880253382819860>");
                return;
            }
        }

        [Command("MusicRoles"), Alias("mr"), Summary("Lists roles that are able to control music.\n**Usage**: `!mr`")]
        public async Task MusicRoles([Remainder]string str = "")
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

        private static async Task LavaClient_Log(LogMessage arg)
        {
            string message = $"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` - `{arg.Message}`";
            Console.WriteLine(message);

            await WebhookClients.LavalinkChannel.SendMessageAsync(message);
        }
        private static async Task WebSocketClosed(WebSocketClosedEventArgs arg)
        {
            try
            {
                var player = Node.GetPlayer(arg.GuildId);

                if (arg.Code == 4014 && player != null && !ReconnectPlayer.Contains(player))
                {
                    ReconnectPlayer.Add(player);
                    await WebhookClients.LavalinkChannel.SendMessageAsync($"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Lost connection to `{arg.GuildId}`, queueing reconnect.");
                }
            } catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
        private static async Task StatsReceived(StatsEventArgs arg)
        {
            if (DateTime.Now.Minute % 10 != 0)
                return;
            if (arg.Players < 1)
                return;

            var eb = new EmbedBuilder();
            eb.WithColor(BasicUtil.RandomColor());
            eb.WithAuthor(DateTime.Now.ToString("HH:mm:ss"), Program.GetClient().CurrentUser.GetAvatarUrl());
            eb.WithFooter($"🌋 Lavalink running for {Math.Round(arg.Uptime.TotalMinutes, 2)} minutes");

            eb.WithDescription($"Connected: {arg.Players}\nPlaying: {arg.PlayingPlayers}");
            eb.AddField("Memory", $"Allocated: {arg.Memory.Allocated / 1000000}MB\nUsed: {arg.Memory.Used / 1000000}MB\nFree: {arg.Memory.Free / 1000000}MB\nReservable: {arg.Memory.Reservable / 1000000}MB\n", true);
            eb.AddField("Frames", $"Sent: {arg.Frames.Sent}\nDeficit: {arg.Frames.Deficit}\nNulled: {arg.Frames.Nulled}", true);
            eb.AddField("Cpu", $"Cores: {arg.Cpu.Cores}\nSystem Load: {Math.Round(arg.Cpu.SystemLoad, 4) * 100}%\nLavalink Load: {Math.Round(arg.Cpu.LavalinkLoad, 4) * 100}%", true);

            await WebhookClients.LavalinkChannel.SendMessageAsync(embeds: new List<Embed> { eb.Build() });
        }
        private static async Task TrackEnded(TrackEndedEventArgs arg)
        {
            var player = arg.Player;
            var track = arg.Track;
            var reason = arg.Reason;

            if (!reason.ShouldPlayNext())
                return;

            if (player.Repeat && !track.Url.ToString().Contains("VoiceLines"))
            {
                player.Queue.EnqueueFirst(track);
            }

            else if (player.Loop && !track.Url.ToString().Contains("VoiceLines"))
            {
                player.Queue.Enqueue(track);
            }

            if (!player.Queue.TryDequeue(out var nextTrack) || !(nextTrack is LavaTrack))
            {
                if (!track.Url.ToString().Contains("VoiceLines"))
                {
                    await player.PlayLocal("empty");
                    await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava(track.User)
                        .WithDescription("There are no more tracks in the queue. We should fix that.")
                        .Build());
                }
                return;
            }

            await player.PlayAsync(nextTrack as LavaTrack);
            await player.TextChannel.SendMessageAsync(embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
        }
        private static async Task TrackStuck(TrackStuckEventArgs arg)
        {
            var player = arg.Player;
            var track = arg.Track;

            if (player?.Track == track)
            {
                await player.SkipAsync();
                await player.TextChannel.SendMessageAsync($"Track `{track.Title}` is stuck for `{arg.Threshold}ms`. Skipping...", embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
            }
        }
        private static async Task TrackException(TrackExceptionEventArgs arg)
        {
            var player = arg.Player;

            if (player?.Track == arg.Track)
            {
                await player.SkipAsync();
                if (player.Queue.Count == 0)
                {
                    await player.TextChannel.SendMessageAsync("Gomen, Senpai... *Coughs blood* ... The player broke! Try starting a new one?\n" +
                        $"Error: `{arg.ErrorMessage}`");
                    await player.DisposeAsync();
                }
            }
        }

        private static async Task Shard_ReconnectPlayer(DiscordSocketClient arg)
        {
            var players = new List<LavaPlayer>(ReconnectPlayer);
            foreach (var player in players)
            {
                try
                {
                    var guild = arg.Guilds.FirstOrDefault(x => x.Id == player.GuildId);
                    if (guild == null)
                    {
                        Console.WriteLine($"[LAVALINK] [{DateTime.Now.ToString("HH:mm:ss")}] Guild mismatch: {player.GuildId} not in shard {arg.ShardId}.");
                        break;
                    }

                    if (player == null || player.VoiceChannel == null)
                    {
                        if (ReconnectPlayer.Contains(player))
                            ReconnectPlayer.Remove(player);

                        await WebhookClients.LavalinkChannel.SendMessageAsync($"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Reconnect `{guild.Id}` failed, cancelling...");
                        await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava()
                                .WithDescription("Gomen, Senpai... Failed to reconnect to voice channel. Use the join command to reinvite me.")
                                .Build());
                        break;
                    }

                    var current = guild.CurrentUser.VoiceChannel;
                    if (current == null || player.VoiceChannel != current)
                    {
                        if (ReconnectPlayer.Contains(player))
                            ReconnectPlayer.Remove(player);

                        await Node.MoveChannelAsync(player.VoiceChannel);
                        await WebhookClients.LavalinkChannel.SendMessageAsync($"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Succesfully reconnected to `{guild.Id}`");
                        await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava()
                            .WithDescription($"Reconnected player to **{player.VoiceChannel.Name}**")
                            .Build());
                        break;
                    }

                    else
                    {
                        if (ReconnectPlayer.Contains(player))
                            ReconnectPlayer.Remove(player);

                        await WebhookClients.LavalinkChannel.SendMessageAsync($"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Already reconnected `{guild.Id}`, cancelling...");
                        break;
                    }
                } catch (Exception ex)
                {
                    if (ex is NullReferenceException)
                        return;

                    SentrySdk.CaptureException(ex);
                    if (ReconnectPlayer.Contains(player))
                        ReconnectPlayer.Remove(player);

                    await WebhookClients.LavalinkChannel.SendMessageAsync($"`🌋` `{DateTime.Now.ToString("HH:mm:ss")}` -  Reconnect `{player.GuildId}` threw an exception.");
                }
            }
        }

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
            return await Context.Channel.SendMessageAsync(embed: eb.Build());
        }
        public LavaPlayer GetPlayer()
        {
            return Node.GetPlayer(Context.Guild);
        }
        public SocketVoiceChannel GetVoiceChannel()
        {
            var user = Context.User as SocketGuildUser;
            return user.VoiceChannel;
        }

        // LOCAL FILES

        public async Task<LavaTrack> LocalTrackFirstOrDefaultAsync(string path)
        {
            var res = await Node.SearchAsync(path);
            return res.Tracks.FirstOrDefault();
        }
    }

    //public class PlayerChannel : PreconditionAttribute
    //{
    //    public PlayerChannel
    //    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    //    {
    //        var user = context.User as IGuildUser;
    //        if (user.VoiceChannel == null)
    //        {
    //            await context.Channel.SendMessageAsync(embed: new EmbedBuilderLava(user)
    //            .WithDescription("You're not in my voice channel, Senpai.")
    //            .Build());
    //        }

    //        return await Task.FromResult(PreconditionResult.FromSuccess());
    //    }
    //}
}