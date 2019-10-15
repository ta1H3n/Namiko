using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Namiko.Data;

namespace Namiko
{
    public class LootboxStat
    {
        public string Name { get; set; }
        public int TypeId { get; set; }
        public Dictionary<int, int> WaifuChance { get; set; }
        public int DefaultWaifutier { get; set; } = 3;
        public int ToastieChance { get; set; }
        public int ToastiesFrom { get; set; }
        public int ToastiesTo { get; set; }
        public string Emote { get; set; } = "";
        public int Price { get; set; }

        public bool IsWaifu()
        {
            var rnd = new Random();
            return rnd.Next(TotalChance()) > ToastieChance;

        }
        public int TotalChance()
        {
            int val = ToastieChance;
            foreach (var chance in WaifuChance)
            {
                val += chance.Value;
            }
            return val;
        }
        public int GetRandomTier()
        {
            var rnd = new Random();
            var total = TotalChance() - ToastieChance;
            var val = rnd.Next(total);

            int chance = 0;
            foreach (var item in WaifuChance)
            {
                chance += item.Value;
                if (val < chance)
                    return item.Key;
            }

            return DefaultWaifutier;
        }
        public int GetRandomToasties()
        {
            var add = new Random().Next(ToastiesTo - ToastiesFrom);
            add -= ((add + 50) % 100) - 50;

            return ToastiesFrom + add;
        }
    }

    public static class LootboxStats
    {
        public static Dictionary<LootBoxType, LootboxStat> Lootboxes;

        public async static Task Reload()
        {
            Lootboxes = new Dictionary<LootBoxType, LootboxStat>();
            var stats = await JsonHelper.ReadJson<List<LootboxStat>>(Locations.LootboxStatsJSON);
            foreach (var item in stats)
            {
                Lootboxes.Add((LootBoxType)item.TypeId, item);
            }
        }
    }
}
