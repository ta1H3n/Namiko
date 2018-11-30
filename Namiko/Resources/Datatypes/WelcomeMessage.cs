using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Namiko.Resources.Datatypes
{
    public class WelcomeMessage
    {
        [Key]
        public int Id { get; set; }
        public String Message { get; set; }
    }

    public class WelcomeChannel
    {
        [Key]
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
