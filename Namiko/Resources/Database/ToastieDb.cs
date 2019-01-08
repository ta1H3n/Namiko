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
        public static int GetToasties(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Toasties.Where(x => x.UserId == UserId).Count() < 1)
                    return 0;
                return DbContext.Toasties.Where(x => x.UserId == UserId).Select(x => x.Amount).FirstOrDefault();
            }
        }
        public static async Task SetToasties(ulong UserId, int Amount)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Toasties.Where(x => x.UserId == UserId).Count() < 1)
                {
                    DbContext.Add(new Toastie { UserId = UserId, Amount = Amount });
                } else
                {
                    DbContext.Update(new Toastie { UserId = UserId, Amount = Amount });
                }
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task AddToasties(ulong UserId, int Amount)
        {
            if ((GetToasties(UserId) + Amount) < 0)
                throw new Exception("You don't have enough toasties... qq");
            else
                await SetToasties(UserId, GetToasties(UserId) + Amount);
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
        public static Daily GetDaily(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Dailies.Where(x => x.UserId == UserId).Count() < 1)
                    return null;
                return DbContext.Dailies.Where(x => x.UserId == UserId).FirstOrDefault();
            }
        }
        public static async Task SetDaily(Daily Daily)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Dailies.Where(x => x.UserId == Daily.UserId).Count() < 1)
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
