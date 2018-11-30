using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.ComponentModel.DataAnnotations;

namespace Namiko.Resources.Datatypes
{
    public class ShopRole
    {
        [Key]
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
        public int Price { get; set; }
        public ulong IfLimitedUserId { get; set; }
        public int DaysLength { get; set; }
    }

    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong RoleId { get; set; }
        public DateTime DateToRemoveOn { get; set; }
    }
}
