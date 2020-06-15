using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class BanroyaleDb
    {
        public static async Task AddBanroyale(Banroyale banroyale)
        {
            using var db = new NamikoDbContext();
            db.Banroyales.Add(banroyale);
            await db.SaveChangesAsync();
        }
        public static async Task<Banroyale> GetBanroyale(ulong channelId)
        {
            using var db = new NamikoDbContext();
            return await db.Banroyales.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.ChannelId == channelId && x.Active);
        }
        public static async Task EndBanroyale(int banroyaleId)
        {
            using var db = new NamikoDbContext();
            var br = db.Banroyales.FirstOrDefault(x => x.Id == banroyaleId);
            if (br == null)
                return;
            br.Active = false;
            br.Running = false;
            db.Update(br);
            await db.SaveChangesAsync();
        }
        public static async Task UpdateBanroyale(Banroyale banroyale)
        {
            using var db = new NamikoDbContext();
            db.Banroyales.Update(banroyale);
            await db.SaveChangesAsync();
        }
        public static async Task<ulong> GetRoleId(ulong guildId)
        {
            using var db = new NamikoDbContext();
            return await db.Banroyales.OrderByDescending(x => x.Id).Where(x => x.GuildId == guildId).Select(x => x.ParticipantRoleId).FirstOrDefaultAsync();
        }
    }
}
