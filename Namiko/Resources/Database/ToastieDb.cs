using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
                if (!DbContext.Toasties.Any(x => x.UserId == UserId))
                {
                    DbContext.Add(new Toastie { UserId = UserId, Amount = Amount, GuildId = GuildId });
                } else
                {
                    DbContext.Update(new Toastie { UserId = UserId, Amount = Amount, GuildId = GuildId });
                }
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task AddToasties(ulong UserId, int Amount, ulong GuildId)
        {
            if ((GetToasties(UserId, GuildId) + Amount) < 0)
                throw new Exception("You don't have enough toasties... qq");
            else
                await SetToasties(UserId, GetToasties(UserId, GuildId) + Amount, GuildId);
        }
        public static List<Toastie> GetAllToasties()
        {
            using (var db = new SqliteDbContext())
            {
                return db.Toasties.Where(x => x.Amount > 0).ToList();
            }
        }
    }

    public static class DailyDb
    {
        public static Daily GetDaily(ulong UserId, ulong GuildId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Dailies.Any(x => x.UserId == UserId && x.GuildId == GuildId))
                    return null;
                return DbContext.Dailies.Where(x => x.UserId == UserId && x.GuildId == GuildId).FirstOrDefault();
            }
        }
        public static async Task SetDaily(Daily Daily)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Dailies.Any(x => x.UserId == Daily.UserId && x.GuildId == Daily.GuildId))
                {
                    DbContext.Add(Daily);
                }
                else
                {
                    DbContext.Update(Daily);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        public static List<Daily> GetAll()
        {
            using (var db = new SqliteDbContext())
            {
                return db.Dailies.ToList();
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
