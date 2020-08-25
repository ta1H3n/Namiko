using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class PermissionRoleDb
    {
        public static bool IsRole(ulong roleId, RoleType type)
        {
            using var DbContext = new NamikoDbContext();
            return DbContext.PermissionRoles.Any(x => x.RoleId == roleId && x.Type == type);
        }
        public static HashSet<ulong> Get(ulong guildId, RoleType type)
        {
            using var DbContext = new NamikoDbContext();
            return DbContext.PermissionRoles.Where(x => x.GuildId == guildId && x.Type == type).Select(x => x.RoleId).ToHashSet();
        }
        public static async Task Add(ulong roleId, ulong guildId, RoleType type)
        {
            using var DbContext = new NamikoDbContext();
            DbContext.PermissionRoles.Add(new PermissionRole { RoleId = roleId, GuildId = guildId, Type = type });
            await DbContext.SaveChangesAsync();
        }
        public static async Task Delete(ulong roleId, RoleType type)
        {
            using var DbContext = new NamikoDbContext();
            DbContext.PermissionRoles.RemoveRange(DbContext.PermissionRoles.Where(x => x.RoleId == roleId && x.Type == type));
            await DbContext.SaveChangesAsync();
        }
        public static async Task Delete(ulong roleId)
        {
            using var DbContext = new NamikoDbContext();
            DbContext.PermissionRoles.RemoveRange(DbContext.PermissionRoles.Where(x => x.RoleId == roleId));
            await DbContext.SaveChangesAsync();
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.PermissionRoles.RemoveRange(db.PermissionRoles.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
        public static List<PermissionRole> GetAll(ulong guildId, RoleType type)
        {
            using var DbContext = new NamikoDbContext();
            return DbContext.PermissionRoles
.Where(x => x.GuildId == guildId)
.Where(x => x.Type == type)
.ToList();
        }
    }
}
