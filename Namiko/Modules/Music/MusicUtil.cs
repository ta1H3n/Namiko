﻿using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Namiko
{
    public static class MusicUtil
    {
        private static readonly Random rnd = new Random();

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
            int i;
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
        public static async Task<List<LavaTrack>> SearchAndSelect(this LavaNode client, string query, Music interactive, int limit = 500)
        {
            List<LavaTrack> tracks;
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                tracks = (await client.SearchAsync(query)).Tracks.Take(limit).ToList();
                return tracks;
            }

            var res = await client.SearchYouTubeAsync(query);
            tracks = res.Tracks.Take(15).ToList();
            if (tracks.Count == 1)
                return tracks;

            if (tracks.Count <= 0)
            {
                await interactive.ReplyAsync("*~ No Results ~*", Color.DarkRed.RawValue);
                return null;
            }

            var player = interactive.GetPlayer();
            await player.PlayLocal("select");
            var track = await SelectTrack(tracks, interactive);
            return track == null ? null : new List<LavaTrack>
            {
                track
            };
        }

        // LOCAL FILES

        public static async Task<bool> PlayLocal(this LavaPlayer player, string folder)
        {
            if (Program.Debug == true)
                return false;

            if (player.PlayerState == PlayerState.Playing)
                return false;

            var tracks = await Music.Node.SearchAsync(RandomFilePath(folder));
            if (tracks.LoadStatus != LoadStatus.NoMatches && tracks.LoadStatus != LoadStatus.LoadFailed)
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
                str += $"`#{i}` {track.Title.ShortenString(60, 55)}\n";
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
                str += $"`#{i}` [{track.Title.ShortenString(70, 65)}]({track.Url})\n";
            }

            eb.WithAuthor($"Song Queue", author?.GetAvatarUrl(), BasicUtil._patreon);
            eb.WithDescription(str);
            return eb;
        }
        public async static Task<EmbedBuilder> NowPlayingEmbed(LavaPlayer player, bool next = false)
        {
            var eb = new EmbedBuilder();
            var track = player.Track;

            string emote = "▷";
            if (player.Repeat)
                emote = ":repeat_one: ▷";

            string desc;
            if (player.Track.IsStream)
                desc = $"{emote} `Live Stream`";
            else

                desc = $"{emote} `{track.Position.ToString(@"hh\:mm\:ss")}`/`{track.Duration.ToString(@"hh\:mm\:ss")}`";

            if (track.User != null)
                desc += $" - {track.User?.Mention ?? ""}";

            var url = track.Url.ToString().StartsWith("file:") ? "" : track.Url.ToString();
            eb.WithAuthor(track.Title, track.User?.GetAvatarUrl(), url);
            eb.WithDescription(desc);

            if (next && player.Queue.Count > 0)
                eb.AddField("Next up:", $"[{player.Queue.Peek().Title}]({player.Queue.Peek().Url})");

            eb.WithThumbnailUrl(await track.FetchArtworkAsync());
            eb.WithFooter($"Volume: {player.Volume} ⚬ Powered by: 🌋 Victoria - Lavalink");
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
