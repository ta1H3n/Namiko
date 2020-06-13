using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class VoteDb
    {

        public static async Task AddVoters(IEnumerable<Voter> voters)
        {
            using var db = new NamikoDbContext();
            db.Voters.AddRange(voters);
            await db.SaveChangesAsync();
        }
        public static async Task AddVoters(IEnumerable<ulong> voters)
        {
            var date = System.DateTime.Now;
            foreach (var x in voters)
            {
                await AddVoter(new Voter { UserId = x, Date = date });
            }
        }
        public static async Task AddVoter(Voter voter)
        {
            using var db = new NamikoDbContext();
            db.Voters.Add(voter);
            await db.SaveChangesAsync();
        }

        public static async Task DeleteLast(ulong UserId)
        {
            using var db = new NamikoDbContext();
            var delete = db.Voters.Where(x => x.UserId == UserId).FirstOrDefault();

            if (delete == null)
                return;

            db.Voters.Remove(delete);
            await db.SaveChangesAsync();
        }
        public static List<Voter> GetVoters()
        {
            using var db = new NamikoDbContext();
            return db.Voters.ToList();
        }
        public static async Task<List<ulong>> GetVoters(int amount)
        {
            using var db = new NamikoDbContext();
            return await db.Voters.Skip(await db.Voters.CountAsync() - amount).Take(amount).Select(x => x.UserId).ToListAsync();
        }
        public static async Task<List<ulong>> GetVoters(DateTime dateFrom, DateTime dateTo)
        {
            using var db = new NamikoDbContext();
            return await db.Voters.Where(x => x.Date > dateFrom && x.Date < dateTo).Select(x => x.UserId).ToListAsync();
        }
        public static async Task<int> VoteCount(ulong userId)
        {
            using var db = new NamikoDbContext();
            return await db.Voters.CountAsync(x => x.UserId == userId);
        }
        public static async Task<List<KeyValuePair<ulong, int>>> GetAllVotes()
        {
            using var db = new NamikoDbContext();
            return await db.Voters
.GroupBy(x => x.UserId)
.Select(x => new KeyValuePair<ulong, int>(x.Key, x.Count()))
.ToListAsync();
        }
    }
}
