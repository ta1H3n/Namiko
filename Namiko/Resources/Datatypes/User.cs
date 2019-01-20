﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Namiko.Resources.Datatypes
{
    public class Profile
    {
        [Key]
        public ulong UserId { get; set; }
        public string ColorHex { get; set; }
        public string Quote { get; set; }
    }

    public class Marriage
    {
        [Key]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong WifeId { get; set; }
        public ulong GuildId { get; set; }
    }
}