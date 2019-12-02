using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Namiko
{
    public class Waifu
    {
        [Key]
        public string Name { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public int Tier { get; set; }
        public string ImageUrl { get; set; }

        public virtual MalWaifu Mal { get; set; }
    }

    public class MalWaifu
    {
        [Key]
        [ForeignKey("Waifu")]
        public string WaifuName { get; set; }
        public long MalId { get; set; }
        public bool MalConfirmed { get; set; }
        public DateTime LastUpdated { get; set; }

        public virtual Waifu Waifu { get; set; }
    }

    public class UserInventory
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public Waifu Waifu { get; set; }
        public DateTime DateBought { get; set; }
    }

    public class WaifuShop
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public DateTime GeneratedDate { get; set; }
        public ShopType Type { get; set; }
        public List<ShopWaifu> ShopWaifus { get; set; }
    }

    public class ShopWaifu
    {
        [Key]
        public int Id { get; set; }
        public WaifuShop WaifuShop { get; set; }
        public Waifu Waifu { get; set; }
        public int Discount { get; set; }
        public int Limited { get; set; }
        public ulong BoughtBy { get; set; }
    }

    public class FeaturedWaifu
    {
        [Key]
        public int id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public Waifu Waifu { get; set; }
    }

    public class WaifuWish
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public Waifu Waifu { get; set; }
    }
}
