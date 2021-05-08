using System;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class ShopWaifu
    {
        [Key]
        public int Id { get; set; }
        public int Discount { get; set; }
        public int Limited { get; set; }
        public ulong BoughtBy { get; set; }

        [Required]
        public WaifuShop WaifuShop { get; set; }
        public int WaifuShopId { get; set; }

        [Required]
        public Waifu Waifu { get; set; }
        public string WaifuName { get; set; }
    }
}
