using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Victoria;
using Victoria.Entities;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Reflection;

namespace Namiko
{
    public static class MusicUtil
    {
        private static Random rnd = new Random();

        public static async Task<LavaTrack> SelectTrack(List<LavaTrack> tracks, Music interactive)
        {
            var msg = await interactive.Context.Channel.SendMessageAsync(embed: TrackSelectEmbed(tracks, (SocketGuildUser)interactive?.Context.User).Build());

            var response = await interactive.NextMessageAsync(
                new Criteria<IMessage>()
                .AddCriterion(new EnsureSourceUserCriterion())
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureRangeCriterion(tracks.Count, Program.GetPrefix(interactive.Context))),
                new TimeSpan(0, 0, 23));

            _ = msg.DeleteAsync();
            int i = 0;
            try
            {
                i = int.Parse(response.Content);
            }
            catch
            {
                _ = interactive.Context.Message.DeleteAsync();
                return null;
            }
            _ = response.DeleteAsync();

            return tracks[i - 1];
        }
        public static async Task<List<LavaTrack>> SearchAndSelect(this LavaRestClient client, string query, Music interactive, int limit = 500)
        {
            List<LavaTrack> tracks = null;
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                tracks = (await client.SearchTracksAsync(query, true)).Tracks.Take(limit).ToList();
                return tracks;
            }

            var res = await client.SearchYouTubeAsync(query);
            tracks = res.Tracks.Take(15).ToList();
            if (tracks.Count == 1)
                return tracks;

            var player = interactive.GetPlayer();
            await player.PlayLocal("select");
            var track = await SelectTrack(tracks, interactive);
            tracks = new List<LavaTrack>();
            tracks.Add(track);
            return tracks;
        }

        // LOCAL FILES

        public static async Task<bool> PlayLocal(this LavaPlayer player, string folder, bool leave = false)
        {
            if (Program.Debug == true)
                return false;

            if (player.IsPlaying)
                return false;

            var tracks = await Music.RestClient.SearchTracksAsync(RandomFilePath(folder));
            if (tracks.LoadType == LoadType.TrackLoaded)
            {
                var track = tracks.Tracks.FirstOrDefault();
                if (track != null)
                {
                    track.IsVoice = true;
                    await player.PlayAsync(track);
                    return true;
                }
            }
            return false;
        }

        public static string RandomFilePath(string folder)
        {
            string root = Assembly.GetEntryAssembly().Location.Replace("Namiko.dll", "VoiceLines/");
            string[] paths = Directory.GetFiles(root + folder, "*.mp3");
            return paths[rnd.Next(paths.Length)];
        }


        // EMBEDS

        public static EmbedBuilder TrackSelectEmbed(List<LavaTrack> tracks, IUser author = null)
        {
            var eb = new EmbedBuilderLava(author);

            string str = "";
            int i = 0;

            foreach (var track in tracks)
            {
                i++;
                str += $"`#{i}` {track.Title.ShortenString(66, 61)}\n";
            }

            eb.AddField($"Search Results :musical_note:", str);
            eb.WithDescription("Enter the number of the song you wish to select __");
            eb.WithFooter("Times out in 23 seconds");
            return eb;
        }
        public static EmbedBuilder TrackListEmbed(IEnumerable<LavaTrack> tracks, IUser author = null, bool loop = false)
        {
            var eb = new EmbedBuilderLava(author);

            int i = 0;
            if (loop)
                eb.WithTitle("Looping Playlist - :repeat:");
            //string str = loop ? ":repeat: - **Looping Playlist**\n" : "";
            string str = "";
            foreach (var track in tracks)
            {
                i++;
                str += $"`#{i}` [{track.Title.ShortenString(70, 65)}]({track.Uri})\n";
            }

            eb.WithAuthor($"Song Queue", author?.GetAvatarUrl(), BasicUtil._patreon);
            eb.WithDescription(str);
            return eb;
        }
        public async static Task<EmbedBuilder> NowPlayingEmbed(LavaPlayer player, bool next = false)
        {
            var eb = new EmbedBuilder();
            var track = player.CurrentTrack;

            string emote = "▷";
            if (player.Repeat)
                emote = ":repeat_one: ▷";

            string desc = "";
            if (player.CurrentTrack.IsStream)
                desc = $"{emote} `Live Stream`";
            else

                desc = $"{emote} `{track.Position.ToString(@"mm\:ss")}`/`{track.Length.ToString(@"mm\:ss")}`";

            if (track.User != null)
                desc += $" - {track.User?.Mention ?? ""}";

            var url = track.Uri.ToString().StartsWith("file:") ? "" : track.Uri.ToString();
            eb.WithAuthor(track.Title, track.User?.GetAvatarUrl(), url);
            eb.WithDescription(desc);

            if (next && player.Queue.Count > 0)
                eb.AddField("Next up:", $"[{player.Queue.Peek().Title}]({player.Queue.Peek().Uri})");

            eb.WithThumbnailUrl(await track.FetchThumbnailAsync());
            eb.WithFooter($"Volume: {player.CurrentVolume} ⚬ Powered by: 🌋 Victoria - Lavalink");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
        public static EmbedBuilder PlaylistListEmbed(List<Playlist> playlists, IUser author = null, bool select = false)
        {
            var eb = new EmbedBuilderLava(author);
            if (playlists.Count < 1)
            {
                eb.WithDescription("*~ No Playlists Found ~*");
                return eb;
            }
            string list = "";

            int i = 0;
            int p = 1;
            foreach (var playlist in playlists)
            {
                if (list.Length + playlist.Name.Length > 1000)
                {
                    eb.AddField($"Playlists#{p++}", list);
                    list = "";
                }
                list += $"`#{++i}` {playlist.Name} - {playlist.UserId.IdToMention()}\n";
            }
            eb.AddField($"Playlists#{p++}", list);

            if (select)
            {
                eb.WithDescription("Enter the number of the playlist you wish to select");
                eb.WithFooter("Times out in 23 seconds");
            }

            return eb;
        }
    }
}
