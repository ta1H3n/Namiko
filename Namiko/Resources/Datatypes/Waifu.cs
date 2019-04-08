using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Namiko.Resources.Datatypes
{
    public class Waifu
    {
        [Key]
        public string Name { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public int Tier { get; set; }
        public int TimesBought { get; set; }
        public string ImageUrl { get; set; }
        public ulong AddedByUserId { get; set; }
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

    public class ShopWaifu
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public DateTime GeneratedDate { get; set; }
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
