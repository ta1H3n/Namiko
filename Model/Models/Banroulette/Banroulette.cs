using System;
using System.ComponentModel.DataAnnotations;

namespace Model
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
}
