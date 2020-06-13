using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class BlacklistDb
    {
        public static bool IsBlacklisted(ulong id)
        {
            using var db = new NamikoDbContext();
            return db.Blacklist.Any(x => x.Id == id);
        }

        public static async Task Add(ulong id)
        {
            using var db = new NamikoDbContext();
            db.Blacklist.Add(new Blacklisted { Id = id });
            await db.SaveChangesAsync();
        }

        public static async Task Remove(ulong id)
        {
            using var db = new NamikoDbContext();
            db.Blacklist.Remove(db.Blacklist.FirstOrDefault(x => x.Id == id));
            await db.SaveChangesAsync();
        }

        public static HashSet<ulong> GetAll()
        {
            using var db = new NamikoDbContext();
            return db.Blacklist.Select(x => x.Id).ToHashSet();
        }
    }
}
