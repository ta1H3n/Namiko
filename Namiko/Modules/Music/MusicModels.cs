using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Namiko
{
    public class Track
    {
        public int Id { get; set; }
        public Playlist Playlist { get; set; }
        public int PlaylistId { get; set; }
        public string SongHash { get; set; }
        public ulong UserId { get; set; }
    }

    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ulong GuildId { get; set; }
        public List<Track> Tracks { get; set; }
        public ulong UserId { get; set; }
    }

    public class MusicRoles
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
    }
}
