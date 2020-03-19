using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class WaifuShop
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public DateTime GeneratedDate { get; set; }
        public ShopType Type { get; set; }
        public List<ShopWaifu> ShopWaifus { get; set; }
    }
}
