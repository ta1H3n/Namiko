﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class LootboxStats
    {
        public static Dictionary<LootBoxType, LootboxStat> Lootboxes;
        
        public static async Task Reload(string path)
        {
            Lootboxes = new Dictionary<LootBoxType, LootboxStat>();
            var stats = await JsonHelper.ReadJson<List<LootboxStat>>(path);
            foreach (var item in stats)
            {
                Lootboxes.Add((LootBoxType)item.TypeId, item);
            }
        }

        public static LootboxStat GetLootboxStats(this LootBoxType type) => LootboxStats.Lootboxes[type];
    }
}
