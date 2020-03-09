using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ulong GuildId { get; set; }
        public List<Track> Tracks { get; set; }
        public ulong UserId { get; set; }
    }
}
