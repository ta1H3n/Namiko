using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using System.Threading.Tasks;
using System.Linq;
using Victoria;
using Victoria.Entities;
using System;

namespace Namiko
{
    public class Music : InteractiveBase<ShardedCommandContext>
    {
        private static readonly LavaRestClient RestClient;
        private static readonly LavaShardClient LavaClient;
        private static DiscordShardedClient Client;

        private LavaPlayer Player { get => LavaClient.GetPlayer(Context.Guild.Id); }

        static Music()
        {
            LavaClient = new LavaShardClient();
            RestClient = new LavaRestClient();

            LavaClient.Log += LavaClient_Log;
            LavaClient.OnTrackException += TrackException;
            LavaClient.OnTrackStuck += TrackStuck;
            LavaClient.OnTrackFinished += TrackFinished;
        }

        public static async Task<bool> Initialize(DiscordShardedClient client)
        {
            Client = client;
            await LavaClient.StartAsync(Client, new Configuration
            {
                AutoDisconnect = true,
                DefaultVolume = 40,
                LogSeverity = LogSeverity.Info
            });
            return true;
        }

        [Command("Join")]
        public async Task Join([Remainder]string str = "")
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You're not in a voice channel... Baaaaka.");
                return;
            }

            var player = Player;
            if (player == null)
            {
                await LavaClient.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Hellooo~ I joined **{user.VoiceChannel.Name}**");
                return;
            }

            if(user.VoiceChannel == player.VoiceChannel)
            {
                await ReplyAsync($"I'm already in **{user.VoiceChannel.Name}**, Senpai...");
                return;
            }

            await LavaClient.MoveChannelsAsync(user.VoiceChannel);
            await ReplyAsync($"Moving over to **{user.VoiceChannel.Name}**");
        }

        [Command("Leave")]
        public async Task Leave([Remainder]string str = "")
        {
            if (Player != null && Player.IsPlaying)
                await Player.StopAsync();

            await LavaClient.DisconnectAsync(GetVoiceChannel());
            await ReplyAsync($"See you next time <:NekoHi:620711213826834443>");
        }

        [Command("Play")]
        public async Task Play([Remainder]string query)
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You're not in a voice channel... Baaaaka.");
                return;
            }

            var player = GetPlayer();
            if (player == null)
                player = await LavaClient.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);

            if (player.VoiceChannel != user.VoiceChannel)
            {
                await ReplyAsync("We're not in the same voice channel, Senpai.");
                return;
            }

            if (player.Queue.Count >= 100)
            {
                await ReplyAsync("Playlist size is limited to 100, Senpai.");
                return;
            }

            var tracks = await RestClient.SearchAndSelect(query, this);
            if (tracks.Count <= 0)
            {
                await ReplyAsync("*~ No Results ~*", Color.DarkRed.RawValue);
                return;
            }
            if (tracks.Count > 1)
            {
                int amount = 100 - player.Queue.Count;
                amount = amount > tracks.Count ? tracks.Count : amount;
                player.Queue.EnqueueRange(tracks.Take(amount).Select(t => { t.User = Context.User; return t; } ));
                await ReplyAsync($"Queued **{amount}** tracks :musical_note:");
                if (!player.IsPlaying)
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

        [Command("PlayFirst")]
        public async Task PlayFirst([Remainder]string query)
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You're not in a voice channel... Baaaaka.");
                return;
            }

            var player = GetPlayer();
            if (player == null)
                player = await LavaClient.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);

            if (player.VoiceChannel != user.VoiceChannel)
            {
                await ReplyAsync("We're not in the same voice channel, Senpai.");
                return;
            }

            if (player.Queue.Count == 100)
            {
                await ReplyAsync("Playlist size is limited to 100, Senpai.");
                return;
            }

            var res = await RestClient.SearchYouTubeAsync(query);
            if (res.LoadType == LoadType.NoMatches)
            {
                await ReplyAsync("*~ No Results ~*", Color.DarkRed.RawValue);
                return;
            }
            if (res.LoadType == LoadType.LoadFailed)
            {
                await ReplyAsync("*Coughing blood*. I-I... failed looking it up... M-mind trying again, Senpai?", Color.DarkRed.RawValue);
                return;
            }

            var track = res.Tracks.FirstOrDefault();
            track.User = Context.User;
            await AddTrack(track, player);
        }

        [Command("PlayNext")]
        public async Task PlayNext([Remainder]string query)
        {
            var user = Context.User as SocketGuildUser;
            if (user.VoiceChannel is null)
            {
                await ReplyAsync("You're not in a voice channel... Baaaaka.");
                return;
            }

            var player = GetPlayer();
            if (player == null)
                player = await LavaClient.ConnectAsync(user.VoiceChannel, Context.Channel as ITextChannel);

            if (player.VoiceChannel != user.VoiceChannel)
            {
                await ReplyAsync("We're not in the same voice channel, Senpai.");
                return;
            }

            if (player.Queue.Count >= 100)
            {
                await ReplyAsync("Playlist size is limited to 100, Senpai.");
                return;
            }

            var tracks = await RestClient.SearchAndSelect(query, this);
            if (tracks.Count <= 0)
            {
                await ReplyAsync("*~ No Results ~*", Color.DarkRed.RawValue);
                return;
            }
            if (tracks.Count > 1)
            {
                int amount = 100 - player.Queue.Count;
                amount = amount > tracks.Count ? tracks.Count : amount;
                player.Queue.EnqueueRange(tracks.Take(amount).Select(t => { t.User = Context.User; return t; }));
                await ReplyAsync($"Queued **{amount}** tracks :musical_note:");
                if (!player.IsPlaying)
                {
                    await player.SkipAsync();
                    await NowPlaying();
                }
                return;
            }

            var track = tracks.First();
            track.User = Context.User;

            if (player.IsPlaying)
            {
                player.Queue.EnqueueFirst(track);
                await ReplyAsync(embed: new EmbedBuilderLava(Context.User)
                    .WithAuthor("Next Up", track.User?.GetAvatarUrl(), track.Uri.ToString())
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

        [Command("Skip")]
        public async Task Skip([Remainder]string str = "")
        {
            var player = Player;

            if (player?.CurrentTrack == null)
            {
                await ReplyAsync("I have nothing to skip, Senpai.");
                return;
            }

            if (player.Queue.Count == 0)
            {
                await player.StopAsync();
                await ReplyAsync("The queue is empty, Senpai.");
                return;
            }

            var track = await player.SkipAsync();
            await ReplyAsync(embed: (await MusicUtil.NowPlayingEmbed(Player, true)).Build());

            if (player.Loop)
            {
                track.ResetPosition();
                player.Queue.Enqueue(track);
            }
        }

        [Command("Remove")]
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
                await ReplyAsync($"Removed songs **{from}-{to}** from the playlist. :fire:");
                return;
            }

            if (from < 1 || player.Queue.Count < from)
            {
                await ReplyAsync($"There is no song in position **{from}**, baaaka.");
                return;
            }

            player.Queue.RemoveAt(from-1);
            await ReplyAsync($"Removed the song at position {from} from the playlist. :fire:");
        }

        [Command("Pause")]
        public async Task Pause([Remainder]string str = "")
        {
            var player = Player;
            if (player is null || !player.IsPlaying)
            {
                await ReplyAsync("There is nothing to pause.");
                return;
            }

            if (!player.IsPaused)
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

        [Command("Resume")]
        public async Task Resume([Remainder]string str = "")
        {
            var player = Player;
            if (player is null || !player.IsPlaying)
            {
                await ReplyAsync("There is nothing to resume.");
                return;
            }

            if (!player.IsPaused)
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

        [Command("Repeat")]
        public async Task Repeat([Remainder]string str = "")
        {
            var player = Player;
            if (player?.CurrentTrack == null)
            {
                await ReplyAsync("There is nothing to repeat, Senpai.");
                return;
            }

            var track = player.CurrentTrack;

            if (player.Repeat)
            {
                player.Repeat = false;
                await ReplyAsync("▷ I'll stop repeating this track. Finally got tired of it, Senpai?");
                return;
            }

            player.Repeat = true;
            await ReplyAsync($":repeat_one: Repeating *{track.Title}*.\nHere we go again...");
        }

        [Command("Loop")]
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

        [Command("Shuffle")]
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

        [Command("Volume")]
        public async Task Volume(int vol, [Remainder]string str = "")
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

            await Player.SetVolumeAsync(vol);
            await ReplyAsync($"Volume set to: **{vol}**\n{comment}");
        }

        [Command("Seek")]
        public async Task Seek(string timeStr, [Remainder]string str = "")
        {
            if (!TimeSpan.TryParseExact(timeStr, @"mm\:ss", null, out var time))
            {
                await ReplyAsync("Couldn't parse the time. Try `mm:ss` e.g. `01:30`");
                return;
            }

            var player = Player;
            if (player?.CurrentTrack == null)
            {
                await ReplyAsync("There is nothing to seek, Senpai...");
                return;
            }

            if (!player.CurrentTrack.IsSeekable)
            {
                await ReplyAsync("The current track is not seekable, Senpai...");
                return;
            }

            if (time > player.CurrentTrack.Length)
            {
                await ReplyAsync($"Your selected time is greater than the track length, baaaka.");
                return;
            }

            await player.SeekAsync(time);
            await ReplyAsync($"Moving to {time.ToString(@"mm\:ss")}");
        }

        [Command("NowPlaying"), Alias("Playing", "np")]
        public async Task NowPlaying([Remainder]string str = "")
        {
            if (Player?.CurrentTrack == null)
            {
                await ReplyAsync("I'm not playing anything, Senpai.");
                return;
            }

            await ReplyAsync(embed: (await MusicUtil.NowPlayingEmbed(Player, true)).Build());
        }

        [Command("Queue")]
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
                await ReplyAsync(embed: MusicUtil.TrackListEmbed(player.Queue.Items, Context.User, player.Loop).Build());
                return;
            }

            var msg = new CustomPaginatedMessage();
            msg.Author = new EmbedAuthorBuilder
            {
                Name = $"Song Queue",
                IconUrl = Context.User.GetAvatarUrl(),
                Url = BasicUtil._patreon
            };
            var pages = CustomPaginatedMessage.PagesArray(player.Queue.Items, 10, (x) => x.Title.ShortenString(70, 65) + "\n");
            var first = pages.First();
            if (player.Loop)
                first = first.Insert(0, ":repeat: Looping Playlist\n");

            //msg.Pages = pages.Select((x, i) => index == i ? value : x);
            msg.Pages = pages; 

            msg.Footer = $"Volume: {player.CurrentVolume} ⚬ Powered by: 🌋 Victoria - Lavalink ⚬ ";

            await PagedReplyAsync(msg);
        }

        [Command("Clear")] 
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

        [Command("TestLocal")]
        public async Task TestLocal([Remainder]string str = "")
        {
            await LocalTrackFirstOrDefaultAsync(str);
        }


        // EVENTS

        private static async Task TrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (!reason.ShouldPlayNext())
                return;

            if (player.Repeat)
            {
                track.IsSeekable = true;
                player.Queue.EnqueueFirst(track);
            }

            else if (player.Loop)
            {
                track.IsSeekable = true;
                player.Queue.Enqueue(track);
            }

            if (!player.Queue.TryDequeue(out var nextTrack) || !(nextTrack is LavaTrack))
            {
                await player.TextChannel.SendMessageAsync(embed: new EmbedBuilderLava(track.User)
                    .WithDescription("There are no more tracks in the queue. We should fix that.")
                    .Build());
                return;
            }

            await player.PlayAsync(nextTrack);
            await player.TextChannel.SendMessageAsync(embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
        }
        private static async Task TrackStuck(LavaPlayer player, LavaTrack track, long arg3)
        {
            if (player?.CurrentTrack == track)
            {
                await player.SkipAsync();
                await player.TextChannel.SendMessageAsync($"Track `{track.Title}` is stuck for `{arg3}ms`. Skipping...", embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
            }
        }
        private static async Task TrackException(LavaPlayer player, LavaTrack track, string arg3)
        {
            await player.TextChannel.SendMessageAsync($"Track `{track.Title}` threw an Error. You were never supposed to see this, Senpai.\n" +
                $"Please report this to taiHen#2839 at https://discord.gg/W6Ru5sM \n" +
                $"Error: `{arg3}`");

            if (player?.CurrentTrack == track)
            {
                await player.SkipAsync();
                await player.TextChannel.SendMessageAsync($"Track `{track.Title}` is stuck. Skipping...", embed: (await MusicUtil.NowPlayingEmbed(player)).Build());
            }
        }
        private static Task LavaClient_Log(Discord.LogMessage arg)
        {
            string message = $"{DateTime.Now} at {arg.Source}] {arg.Message}";
            Console.WriteLine(message);
            return Task.CompletedTask;
        }


        // HELPERS

        public async Task AddTrack(LavaTrack track, LavaPlayer player)
        {
            if (player.IsPlaying)
            {
                player.Queue.Enqueue(track);
                await ReplyAsync(embed: new EmbedBuilderLava(Context.User)
                    .WithAuthor("Track Queued", track.User?.GetAvatarUrl(), track.Uri.ToString())
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
            return LavaClient.GetPlayer(Context.Guild.Id);
        }
        public SocketVoiceChannel GetVoiceChannel()
        {
            var user = Context.User as SocketGuildUser;
            return user.VoiceChannel;
        }

        // LOCAL FILES

        public async Task<LavaTrack> LocalTrackFirstOrDefaultAsync(string path)
        {
            var res = await RestClient.SearchTracksAsync(path);
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