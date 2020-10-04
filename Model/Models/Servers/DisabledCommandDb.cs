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
            return await db.DisabledCommands.Where(x => x.GuildId == GuildId && x.Name.Equals(name) && x.Type == type).FirstOrDefaultAsync();
        }
        public static async Task DeleteDisabledCommand(ulong GuildId, string name, DisabledCommandType type)
        {
            using var db = new NamikoDbContext();
            var command = await db.DisabledCommands.Where(x => x.GuildId == GuildId && x.Name.Equals(name) && x.Type == type).FirstOrDefaultAsync();
            if (command != null)
            {
                db.Remove(command);
                await db.SaveChangesAsync();
            }
        }
        public static async Task<int> AddNewDisabledCommand(ulong GuildId, string name, DisabledCommandType type)
        {
            using var db = new NamikoDbContext();
            var cmd = await db.DisabledCommands.Where(x => x.GuildId == GuildId && x.Name.Equals(name) && x.Type == type).FirstOrDefaultAsync();
            if (cmd != null)
                return -1;

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
