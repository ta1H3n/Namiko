using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Model
{
    public static class ShopRoleDb
    {
        public static async Task<List<ShopRole>> GetRoles(ulong guildId)
        {
            using var db = new NamikoDbContext();
            return await db.ShopRoles.Where(x => x.GuildId == guildId).OrderByDescending(x => x.Price).ToListAsync();
        }

        public static async Task AddRole(ulong guildId, ulong roleId, int price)
        {
            using var db = new NamikoDbContext();
            db.ShopRoles.Add(new ShopRole
            {
                GuildId = guildId,
                Price = price,
                RoleId = roleId
            });
            await db.SaveChangesAsync();
        }

        public static async Task RemoveRole(ulong roleId)
        {
            using var db = new NamikoDbContext();
            db.ShopRoles.Remove(db.ShopRoles.FirstOrDefault(x => x.RoleId == roleId));
            await db.SaveChangesAsync();
        }

        public static async Task RemoveByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.ShopRoles.RemoveRange(db.ShopRoles.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }

        public static async Task<bool> IsRole(ulong roleId)
        {
            using var db = new NamikoDbContext();
            return await db.ShopRoles.AnyAsync(x => x.RoleId == roleId);
        }
    }
}
