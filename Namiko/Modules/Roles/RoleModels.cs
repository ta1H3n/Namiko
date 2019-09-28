using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.ComponentModel.DataAnnotations;

namespace Namiko
{
    public class PublicRole
    {
        [Key]
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
    }

    public class Team
    {
        [Key]
        public ulong LeaderRoleId { get; set; }
        public ulong MemberRoleId { get; set; }
        public ulong GuildId { get; set; }
    }

    public class Invite
    {
        [Key]
        public int Id { get; set; }
        public ulong TeamId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Date { get; set; }
    }

    public class PermissionRole
    {
        [Key]
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
        public RoleType Type { get; set; }
    }
}