using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class PublicRoleDb
    {
        public static bool IsPublic(ulong roleId)
        {
            using var DbContext = new SqliteDbContext();
            return DbContext.PublicRoles.Any(x => x.RoleId == roleId);
        }
        public static async Task Add(ulong roleId, ulong guildId)
        {
            using var DbContext = new SqliteDbContext();
            DbContext.Add(new PublicRole { RoleId = roleId, GuildId = guildId });
            await DbContext.SaveChangesAsync();
        }
        public static async Task Delete(ulong roleId)
        {
            using var DbContext = new SqliteDbContext();
            var publicRole = DbContext.PublicRoles.Where(x => x.RoleId == roleId).FirstOrDefault();
            DbContext.PublicRoles.Remove(publicRole);
            await DbContext.SaveChangesAsync();
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new SqliteDbContext();
            db.PublicRoles.RemoveRange(db.PublicRoles.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
        public static List<PublicRole> GetAll(ulong guildId)
        {
            using var DbContext = new SqliteDbContext();
            return DbContext.PublicRoles.Where(x => x.GuildId == guildId).ToList();
        }
    }
}
