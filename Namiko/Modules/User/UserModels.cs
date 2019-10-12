﻿using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Namiko
{
    public class Profile
    {
        [Key]
        public ulong UserId { get; set; }
        public string ColorHex { get; set; }
        public string PriorColorHexStack { get; set; }
        public string Quote { get; set; }
        public string Image { get; set; }
        public int LootboxesOpened { get; set; }
        public int Rep { get; set; }
        public DateTime RepDate { get; set; }
    }

    public class Marriage
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong WifeId { get; set; }
        public bool IsMarried { get; set; }
        public ulong GuildId { get; set; }
        public DateTime Date { get; set; }
    }

    public class Voter
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public DateTime Date { get; set; }
    }

    public class Premium
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public PemiumType Type { get; set; }
        public DateTime ClaimDate { get; set; }
    }
}
