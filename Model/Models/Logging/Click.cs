using System;
using System.ComponentModel.DataAnnotations;

namespace Model.Models.Logging
{
    public class Click
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public string RedirectUrl { get; set; }
        public DateTime Date { get; set; }
        public ulong DiscordId { get; set; }
        public string Ip { get; set; }
        public string Referer { get; set; }
    }
}
