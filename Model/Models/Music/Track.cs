using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Track
    {
        public int Id { get; set; }
        public Playlist Playlist { get; set; }
        public int PlaylistId { get; set; }
        public string SongHash { get; set; }
        public ulong UserId { get; set; }
    }
}
