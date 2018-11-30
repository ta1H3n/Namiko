using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Namiko.Resources.Datatypes
{
    public class Banroulette
    {
        [Key]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public ulong ServerId { get; set; }
        public ulong RoleReqId { get; set; }
        public int BanLengthHours { get; set; }
        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public int RewardPool { get; set; }
        public bool Active { get; set; }
    }

    public class BanrouletteParticipant
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public Banroulette Banroulette { get; set; }
    }

    public class BannedUser
    {
        [Key]
        public int Id { get; set; }
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public DateTime DateBanStart { get; set; }
        public DateTime DateBanEnd { get; set; }
        public bool Active { get; set; }
    }
}
