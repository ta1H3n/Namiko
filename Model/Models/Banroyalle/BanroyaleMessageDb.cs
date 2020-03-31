using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class BanroyaleMessageDb
    {
        public static async Task AddMessage(BanroyaleMessage banroyaleMessage)
        {
            using var db = new SqliteDbContext();
            db.BanroyaleMessages.Add(banroyaleMessage);
            await db.SaveChangesAsync();
        }
        public static BanroyaleMessage GetLastActiveMessage(int banroyaleId)
        {
            using var db = new SqliteDbContext();
            return db.BanroyaleMessages.OrderByDescending(x => x.Id).FirstOrDefault(x => x.BanroyaleId == banroyaleId && x.Active);
        }
        public static async Task EndMessage(int banroyaleMessageId)
        {
            using var db = new SqliteDbContext();
            var br = db.BanroyaleMessages.FirstOrDefault(x => x.Id == banroyaleMessageId);
            if (br == null)
                return;
            br.Active = false;
            db.Update(br);
            await db.SaveChangesAsync();
        }
        public static async Task UpdateMessage(BanroyaleMessage banroyaleMessage)
        {
            using var db = new SqliteDbContext();
            db.BanroyaleMessages.Update(banroyaleMessage);
            await db.SaveChangesAsync();
        }
    }
}
