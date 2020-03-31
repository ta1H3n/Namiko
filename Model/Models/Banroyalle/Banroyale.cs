using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Banroyale
    {
        [Key]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleReqId { get; set; }
        public ulong ParticipantRoleId { get; set; }
        public int BanLengthHours { get; set; }
        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public int RewardPool { get; set; }
        public int WinnerAmount { get; set; }
        public int MinFrequency { get; set; }
        public int MaxFrequency { get; set; }
        public bool Kick { get; set; }
        public bool Running { get; set; }
        public bool Active { get; set; }
    }
}
