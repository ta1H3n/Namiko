using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Model.Models.Commands
{
    public class CommandDb
    {
        public async static Task<List<Module>> GetModulesAsync()
        {
            using var db = new NamikoDbContext();
            var res = await db.Modules.Include(x => x.Commands).ToListAsync();
            return res;
        }

        public static async Task<Module> GetModule(string name)
        {
            using var db = new NamikoDbContext();
            var res = await db.Modules.Include(x => x.Commands).FirstOrDefaultAsync(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return res;
        }
    }
}
