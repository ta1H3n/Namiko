using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class MarriageDb
    {
        public static List<Marriage> GetMarriages(ulong userId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            return db.Marriages.Where(x => x.IsMarried == true && (x.UserId == userId || x.WifeId == userId) && x.GuildId == guildId).ToList();
        }
        public static List<Marriage> GetProposalsReceived(ulong userId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            return db.Marriages.Where(x => x.IsMarried == false && x.WifeId == userId && x.GuildId == guildId).ToList();
        }
        public static List<Marriage> GetProposalsSent(ulong userId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            return db.Marriages.Where(x => x.IsMarried == false && x.UserId == userId && x.GuildId == guildId).ToList();
        }

        //Method: get specific proposal or marriage
        public static Marriage GetMarriageOrProposal(ulong userId, ulong wifeId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            return db.Marriages.Where(x => x.UserId == userId && x.WifeId == wifeId && x.GuildId == guildId).FirstOrDefault();
        }
        // userId is the user who proposed, wifeId is the user who received the proposal, date = now, married = false
        public static async Task Propose(ulong userId, ulong wifeId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.Marriages.Add(new Marriage
            {
                GuildId = guildId,
                UserId = userId,
                WifeId = wifeId,
                IsMarried = false,
                Date = System.DateTime.Now
            });
            await db.SaveChangesAsync();
        }
        // You already have the marriage obj at the point you want to set married to true, so you can do it manually and just update
        public static async Task UpdateMarriage(Marriage marriage)
        {
            using var db = new NamikoDbContext();
            db.Marriages.Update(marriage);
            await db.SaveChangesAsync();
        }
        // Deletes all matching results, userId and wifeId order doesn't matter
        public static async Task DeleteMarriageOrProposal(ulong userId, ulong wifeId, ulong guildId)
        {
            using var db = new NamikoDbContext();
            var items = db.Marriages.Where(x => ((x.UserId == userId && x.WifeId == wifeId) || (x.UserId == wifeId && x.WifeId == userId)) && x.GuildId == guildId);
            db.Marriages.RemoveRange(items);
            await db.SaveChangesAsync();
        }
        // To delete the marriage you want to delete and not all of them
        public static async Task DeleteMarriageOrProposal(Marriage marriage)
        {
            using var db = new NamikoDbContext();
            db.Marriages.Remove(marriage);
            await db.SaveChangesAsync();
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using var db = new NamikoDbContext();
            db.Marriages.RemoveRange(db.Marriages.Where(x => x.GuildId == guildId));
            await db.SaveChangesAsync();
        }
    }
}
