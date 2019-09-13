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

namespace Namiko
{
    public static class MusicUtil
    {
        public static async Task<LavaTrack> SelectTrack(List<LavaTrack> tracks, InteractiveBase<ShardedCommandContext> interactive)
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
        public static async Task<List<LavaTrack>> SearchAndSelect(this LavaRestClient client, string query, InteractiveBase<ShardedCommandContext> interactive)
        {
            List<LavaTrack> tracks = null;
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                tracks = (await client.SearchTracksAsync(query, true)).Tracks.Take(100).ToList();
                return tracks;
            }

            var res = await client.SearchYouTubeAsync(query);
            tracks = res.Tracks.Take(15).ToList();
            if (tracks.Count == 1)
                return tracks;

            var track = await SelectTrack(tracks, interactive);
            tracks = new List<LavaTrack>();
            tracks.Add(track);
            return tracks;
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
            string str = loop ? ":repeat: Looping Playlist\n" : "";
            foreach (var track in tracks)
            {
                i++;
                str += $"`#{i}` {track.Title.ShortenString(70, 65)}\n";
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
                emote = ":repeat_one:";

            string desc = "";
            if (player.CurrentTrack.IsStream)
                desc = $"{emote} `Live Stream`";
            else

                desc = $"{emote} `{track.Position.ToString(@"mm\:ss")}`/`{track.Length.ToString(@"mm\:ss")}`";

            if (track.User != null)
                desc += $" - {track.User?.Mention ?? ""}";

            eb.WithAuthor(track.Title, track.User?.GetAvatarUrl(), track.Uri.ToString());
            eb.WithDescription(desc);

            if (next && player.Queue.Count > 0)
                eb.AddField("Next up:", $"{player.Queue.Peek().Title}");

            eb.WithThumbnailUrl(await track.FetchThumbnailAsync());
            eb.WithFooter($"Volume: {player.CurrentVolume} ⚬ Powered by: 🌋 Victoria - Lavalink");
            eb.WithColor(BasicUtil.RandomColor());
            return eb;
        }
    }
}
