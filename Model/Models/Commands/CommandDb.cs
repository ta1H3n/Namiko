using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class CommandDb
    {
        public async static Task<List<Module>> GetModulesAsync(string[] exclude = null)
        {
            using var db = new NamikoDbContext();
            var query = db.Modules.Include(x => x.Commands).AsQueryable();
            if (exclude != null)
            {
                foreach (var filter in exclude)
                {
                    query = query.Where(x => x.Name.ToUpper() != filter.ToUpper());
                }
            }

            var res = await query.ToListAsync();
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
