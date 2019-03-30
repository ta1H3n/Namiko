using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Namiko.Core;
using Namiko.Resources.Datatypes;
using Namiko.Resources.Database;
using System.Threading.Tasks;

namespace Namiko.Resources.Database
{
    public static class ToastieDb
    {
        public static int GetToasties(ulong UserId, ulong GuildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (!DbContext.Toasties.Any(x => x.UserId == UserId && x.GuildId == GuildId))
                    return 0;
                return DbContext.Toasties.Where(x => x.UserId == UserId && x.GuildId == GuildId).Select(x => x.Amount).FirstOrDefault();
            }
        }
        public static async Task SetToasties(ulong UserId, int Amount, ulong GuildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var toasties = DbContext.Toasties.FirstOrDefault(x => x.UserId == UserId && x.GuildId == GuildId);

                if (toasties == null)
                {
                    DbContext.Add(new Toastie { UserId = UserId, Amount = Amount, GuildId = GuildId });
                } else
                {
                    toasties.Amount = Amount;
                    DbContext.Update(toasties);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task AddToasties(ulong UserId, int Amount, ulong GuildId)
        {
            var amount = GetToasties(UserId, GuildId) + Amount;
            if (amount < 0)
                throw new Exception("You don't have enough toasties... qq");
            else
                await SetToasties(UserId, amount, GuildId);
        }
        public static List<Toastie> GetAllToasties(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Toasties.Where(x => x.Amount > 0 && x.GuildId == guildId).ToList();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Toasties.RemoveRange(db.Toasties.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
        public static long TotalToasties(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Toasties.Where(x => x.Amount > 0 && x.GuildId == guildId).Sum(x => Convert.ToInt64(x.Amount));
            }
        }
    }

    public static class DailyDb
    {
        public static Daily GetDaily(ulong UserId, ulong GuildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (!DbContext.Dailies.Any(x => x.UserId == UserId && x.GuildId == GuildId))
                    return null;
                return DbContext.Dailies.Where(x => x.UserId == UserId && x.GuildId == GuildId).FirstOrDefault();
            }
        }
        public static async Task SetDaily(Daily Daily)
        {
            using (var DbContext = new SqliteDbContext())
            {
                var daily = DbContext.Dailies.FirstOrDefault(x => x.UserId == Daily.UserId && x.GuildId == Daily.GuildId);
                if (daily == null)
                {
                    DbContext.Add(Daily);
                }
                else
                {
                    daily.Streak = Daily.Streak;
                    daily.Date = Daily.Date;
                    DbContext.Update(daily);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        public static List<Daily> GetAll(ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Dailies.Where(x => (x.GuildId == GuildId) && ((x.Date + 48*60*60*1000) > DateTimeOffset.Now.ToUnixTimeMilliseconds())).ToList();
            }
        }
        public static int GetHighest(ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                try
                {
                    return db.Dailies.Where(x => x.GuildId == GuildId).Max(x => x.Streak);
                }
                catch { return 1; }
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Dailies.RemoveRange(db.Dailies.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
    }

    public static class WeeklyDb
    {
        public static Weekly GetWeekly(ulong UserId, ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                var weekly = db.Weeklies.FirstOrDefault(x => x.GuildId == GuildId && x.UserId == UserId);
                return weekly;
            }
        }
        public static async Task SetWeekly(Weekly Weekly)
        {
            using (var db = new SqliteDbContext())
            {
                var weekly = db.Weeklies.FirstOrDefault(x => x.UserId == Weekly.UserId && x.GuildId == Weekly.GuildId);
                if (weekly == null)
                {
                    db.Add(Weekly);
                }
                else
                {
                    weekly.Date = Weekly.Date;
                    db.Update(weekly);
                }
                await db.SaveChangesAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Weeklies.RemoveRange(db.Weeklies.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
    }

    public static class LootBoxDb
    {
        public static int GetAmount(ulong UserId, LootBoxType type, ulong GuildId = 0)
        {
            using (var db = new SqliteDbContext())
            {
                var box = db.LootBoxes.Where(x => x.UserId == UserId && x.Type == type && x.GuildId == GuildId).FirstOrDefault();

                if (box == null)
                    return 0;

                return box.Amount;
            }
        }
        public static async Task AddLootbox(ulong UserId, LootBoxType type, int amount, ulong GuildId = 0)
        {
            using (var db = new SqliteDbContext())
            {
                var box = db.LootBoxes.Where(x => x.UserId == UserId && x.Type == type && x.GuildId == GuildId).FirstOrDefault();

                if (box == null)
                {
                    db.LootBoxes.Add(new LootBox
                    {
                        UserId = UserId,
                        GuildId = GuildId,
                        Amount = amount,
                        Type = type
                    });
                }
                else
                {
                    box.Amount += amount;
                    db.LootBoxes.Update(box);
                }

                await db.SaveChangesAsync();
            }
        }
        public static List<LootBox> GetAll(ulong UserId, ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                var box = db.LootBoxes.Where(x => x.UserId == UserId && (x.GuildId == GuildId || x.GuildId == 0)).ToList();
                
                return box;
            }
        }
    }
    

    // public static class ShopItemDb
    // {
    //     public static List<ShopRole> GetByGuild(ulong GuildId)
    //     {
    //         using (var db = new SqliteDbContext())
    //         {
    //             return db.ShopRoles.Where(x => x.GuildId == GuildId).ToList();
    //         }
    //     }
    //     public static List<ShopRole> GetAll()
    //     {
    //         using (var db = new SqliteDbContext())
    //         {
    //             return db.ShopRoles.ToList();
    //         }
    //     }
    //     public static ShopRole GetByRoleId(ulong RoleId)
    //     {
    //         using (var db = new SqliteDbContext())
    //         {
    //             return db.ShopRoles.Where(x => x.RoleId == RoleId).FirstOrDefault();
    //         }
    //     }
    //     public static async Task AddShopRole(ShopRole role)
    //     {
    //         using (var db = new SqliteDbContext())
    //         {
    //             db.ShopRoles.Add(role);
    //             await db.SaveChangesAsync();
    //         }
    //     }
    //     public static async Task DeleteShopRole(ShopRole role)
    //     {
    //         using (var db = new SqliteDbContext())
    //         {
    //             db.Remove(role);
    //             await db.SaveChangesAsync();
    //         }
    //     }
    // }
}
