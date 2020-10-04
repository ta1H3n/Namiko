using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class DisabledCommandDb
    {
        public static async Task<DisabledCommand> GetDisabledCommand(ulong GuildId, string name, DisabledCommandType type)
        {
            using var db = new NamikoDbContext();
            return await db.DisabledCommands.Where(x => x.GuildId == GuildId && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && x.Type == type).FirstOrDefaultAsync();
        }
        public static async Task DeleteDisabledCommand(ulong GuildId, string name, DisabledCommandType type)
        {
            using var db = new NamikoDbContext();
            var server = db.DisabledCommands.Where(x => x.GuildId == GuildId && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && x.Type == type).FirstOrDefault();
            if (server != null)
            {
                db.Remove(server);
                await db.SaveChangesAsync();
            }
        }
        public static async Task<int> AddNewDisabledCommand(ulong GuildId, string name, DisabledCommandType type)
        {
            using var db = new NamikoDbContext();
            var DisabledCommand = new DisabledCommand
            {
                GuildId = GuildId,
                Type = type,
                Name = name
            };

            db.DisabledCommands.Add(DisabledCommand);
            return await db.SaveChangesAsync();
        }
        public static async Task<List<DisabledCommand>> GetAll()
        {
            using var db = new NamikoDbContext();
            return await db.DisabledCommands.ToListAsync();
        }
        public static async Task<List<DisabledCommand>> GetAll(ulong GuildId)
        {
            using var db = new NamikoDbContext();
            return await db.DisabledCommands.Where(x => x.GuildId == GuildId).ToListAsync();
        }
    }
}
