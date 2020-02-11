using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Namiko {
    public static class UserDb {
        
        //Methods: hex code check (also gets hex colour)
        public static bool GetHex(out string color, ulong UserId) {
            using (var DbContext = new SqliteDbContext()) {
                var profileColour = DbContext.Profiles.Where(x => x.UserId == UserId).Select(x => x.ColorHex).FirstOrDefault();
                if (profileColour == null)
                    color = "";
                else
                    color = profileColour.ToString();
                return !String.IsNullOrEmpty(profileColour);
            }
        }
        public static async Task SetHex(Color color, ulong UserId) {
            using (var DbContext = new SqliteDbContext()) {
                var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

                //checking hex validity (makes sure hex colour.len ALWAYS = 6)
                string colour = color.Name;
                int boundary = colour.Length;
                if (boundary < 6) colour = new string('0', 6 - boundary) + colour;

                //if this user has not set a custom colour
                if (profile == null) {
                    DbContext.Add(new Profile { UserId = UserId, ColorHex = colour});
                    await DbContext.SaveChangesAsync();
                    return;
                }
                
                //setting profile values + updating stack
                profile.PriorColorHexStack = PushStackManagement(profile.ColorHex, profile.PriorColorHexStack);
                profile.ColorHex = colour;
                DbContext.Update(profile);
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task HexDefault(ulong UserId) {
            using (var DbContext = new SqliteDbContext()) {
                var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

                //if this user is not in the database
                if (profile == null) {
                    DbContext.Add(new Profile { UserId = UserId });
                    await DbContext.SaveChangesAsync();
                    return;
                }
                
                //updating profile + stack change
                profile.PriorColorHexStack = PushStackManagement(profile.ColorHex, profile.PriorColorHexStack);
                profile.ColorHex = "";
                DbContext.Update(profile);
                await DbContext.SaveChangesAsync();
            }
        }

        //colour stack
        public static string GetHexStack(ulong UserId) {
            using (var DbContext = new SqliteDbContext()) {
                var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();
                return profile.PriorColorHexStack;
            }
        }
        public static async Task PopStack(ulong UserId) {
            using (var DbContext = new SqliteDbContext()) {
                var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();
                string stack = profile.PriorColorHexStack;
                string popped_colour;

                //cant be smaller than 6 here, checks equal
                if ( stack.Length == 6 ){
                    popped_colour = stack;
                    stack = "";

                //provided there's more
                } else {
                    popped_colour = stack.Split(";").FirstOrDefault();
                    int boundary = popped_colour.Length + 1;
                    stack = stack.Substring(boundary, stack.Length - boundary);
                }

                //profile update
                profile.PriorColorHexStack = stack;
                profile.ColorHex = popped_colour;
                DbContext.Update(profile);
                await DbContext.SaveChangesAsync();
            }
        }
        private static string PushStackManagement(string current, string stack) {
            stack = (( String.IsNullOrEmpty(current) ) ? "" : current + ";") + stack;
            return (stack.Split(";").Length > 3)? stack.Substring(0, stack.LastIndexOf(";")) : stack;
        }
        
        //Methods: quotes
        public static string GetQuote(ulong UserId) {
            using (var DbContext = new SqliteDbContext()) {
                string quote = DbContext.Profiles.Where(x => x.UserId == UserId).Select(x => x.Quote).FirstOrDefault();
                return String.IsNullOrEmpty(quote)? null : quote;
            }
        }
        public static async Task SetQuote(ulong UserId, string quote) {
            using (var DbContext = new SqliteDbContext()) {
                var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

                //if user does not exist
                if (profile == null) {
                    DbContext.Add(new Profile { UserId = UserId, Quote = quote });
                    await DbContext.SaveChangesAsync();
                    return;
                }

                //setting quote
                profile.Quote = quote;
                DbContext.Update(profile);
                await DbContext.SaveChangesAsync();
            }
        }
        
        //Methods: Images
        public static string GetImage(ulong userId) {
            using (var DbContext = new SqliteDbContext()) {
                string image = DbContext.Profiles.Where(x => x.UserId == userId).Select(x => x.Image).FirstOrDefault();
                return String.IsNullOrEmpty(image)? null : image;
            }
        }
        public static async Task SetImage(ulong UserId, string image) {
            using (var DbContext = new SqliteDbContext()) {
                var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

                //if user does not exist
                if (profile == null) {
                    DbContext.Add(new Profile { UserId = UserId, Image = image});
                    await DbContext.SaveChangesAsync();
                    return;
                }

                //setting image
                profile.Image = image;
                DbContext.Update(profile);
                await DbContext.SaveChangesAsync();
            }
        }

        //Lootboxes
        public static int GetLootboxOpenedAmount(ulong userId)
        {
            using (var db = new SqliteDbContext())
            {
                var user = db.Profiles.FirstOrDefault(x => x.UserId == userId);
                if (user == null)
                    return 0;

                return user.LootboxesOpened;
            }
        }
        public static async Task<List<KeyValuePair<ulong, int>>> GetAllLootboxOpenedAmount()
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Profiles
                    .Where(x => x.LootboxesOpened > 0)
                    .Select(x => new KeyValuePair<ulong, int>(x.UserId, x.LootboxesOpened))
                    .ToListAsync();
            }
        }
        public static async Task IncrementLootboxOpened(ulong userId, int amount = 1)
        {
            using (var db = new SqliteDbContext())
            {
                var user = db.Profiles.FirstOrDefault(x => x.UserId == userId);
                if (user == null)
                {
                    db.Profiles.Add(new Profile { UserId = userId, LootboxesOpened = amount });
                }
                else
                {
                    user.LootboxesOpened += amount;
                    db.Profiles.Update(user);
                }

                await db.SaveChangesAsync();
            }
        }

        //Reputation
        public static int GetRepAmount(ulong userId)
        {
            using (var db = new SqliteDbContext())
            {
                var user = db.Profiles.FirstOrDefault(x => x.UserId == userId);
                if (user == null)
                    return 0;

                return user.Rep;
            }
        }
        public static async Task<int> IncrementRep(ulong userId, int amount = 1)
        {
            using (var db = new SqliteDbContext())
            {
                var user = db.Profiles.FirstOrDefault(x => x.UserId == userId);
                if (user == null)
                {
                    db.Profiles.Add(new Profile { UserId = userId, Rep = amount });
                }
                else
                {
                    user.Rep += amount;
                    db.Profiles.Update(user);
                }

                await db.SaveChangesAsync();
                return user?.Rep ?? amount;
            }
        }
        public static async Task<List<KeyValuePair<ulong, int>>> GetAllRep()
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Profiles
                    .Where(x => x.Rep > 0)
                    .Select(x => new KeyValuePair<ulong, int>(x.UserId, x.Rep))
                    .ToListAsync();
            }
        }

        //Profile
        public static async Task<Profile> GetProfile(ulong userId)
        {
            using (var db = new SqliteDbContext())
            {
                var profile = db.Profiles.FirstOrDefault(x => x.UserId == userId);
                if(profile == null)
                {
                    profile = new Profile { UserId = userId };
                    db.Profiles.Add(profile);
                    await db.SaveChangesAsync();
                }

                return profile;
            }
        }
        public static async Task UpdateProfile(Profile profile)
        {
            using (var db = new SqliteDbContext())
            {
                db.Profiles.Update(profile);
                await db.SaveChangesAsync();
            }
        }
    }

    public static class MarriageDb {

          // tai's
         // Returns empty lists if none are found
        // Works both ways, returs a list of all active marriages a user has in a guild
        public static List<Marriage> GetMarriages(ulong userId, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Marriages.Where(x => x.IsMarried == true && (x.UserId == userId || x.WifeId == userId) && x.GuildId == guildId).ToList();
            }
        }
        public static List<Marriage> GetProposalsReceived(ulong userId, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Marriages.Where(x => x.IsMarried == false && x.WifeId == userId && x.GuildId == guildId).ToList();
            }
        }
        public static List<Marriage> GetProposalsSent(ulong userId, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Marriages.Where(x => x.IsMarried == false && x.UserId == userId && x.GuildId == guildId).ToList();
            }
        }

        //Method: get specific proposal or marriage
        public static Marriage GetMarriageOrProposal(ulong userId, ulong wifeId, ulong guildId) {
            using (var db = new SqliteDbContext()) {
                return db.Marriages.Where(x => (x.UserId == userId && x.WifeId == wifeId) && x.GuildId == guildId).FirstOrDefault();
            }
        }
        // userId is the user who proposed, wifeId is the user who received the proposal, date = now, married = false
        public static async Task Propose(ulong userId, ulong wifeId, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
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
        }
        // You already have the marriage obj at the point you want to set married to true, so you can do it manually and just update
        public static async Task UpdateMarriage(Marriage marriage)
        {
            using (var db = new SqliteDbContext())
            {
                db.Marriages.Update(marriage);
                await db.SaveChangesAsync();

            }
        }
        // Deletes all matching results, userId and wifeId order doesn't matter
        public static async Task DeleteMarriageOrProposal(ulong userId, ulong wifeId, ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                var items = db.Marriages.Where(x => ((x.UserId == userId && x.WifeId == wifeId) || (x.UserId == wifeId && x.WifeId == userId)) && x.GuildId == guildId);
                db.Marriages.RemoveRange(items);
                await db.SaveChangesAsync();
            }
        }
        // To delete the marriage you want to delete and not all of them
        public static async Task DeleteMarriageOrProposal(Marriage marriage)
        {
            using (var db = new SqliteDbContext())
            {
                db.Marriages.Remove(marriage);
                await db.SaveChangesAsync();
            }
        }
        public static async Task DeleteByGuild(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                db.Marriages.RemoveRange(db.Marriages.Where(x => x.GuildId == guildId));
                await db.SaveChangesAsync();
            }
        }
    }

    public static class VoteDb
    {

        public static async Task AddVoters(IEnumerable<Voter> voters)
        {
            using (var db = new SqliteDbContext())
            {
                db.Voters.AddRange(voters);
                await db.SaveChangesAsync();
            }
        }
        public static async Task AddVoters(IEnumerable<ulong> voters)
        {
            var date = System.DateTime.Now;
            foreach(var x in voters)
            {
                await AddVoter(new Voter { UserId = x, Date = date });
            }
        }
        public static async Task AddVoter(Voter voter)
        {
            using (var db = new SqliteDbContext())
            {
                db.Voters.Add(voter);
                await db.SaveChangesAsync();
            }
        }

        public static async Task DeleteLast(ulong UserId)
        {
            using (var db = new SqliteDbContext())
            {
                var delete = db.Voters.Where(x => x.UserId == UserId).FirstOrDefault();

                if (delete == null)
                    return;

                db.Voters.Remove(delete);
                await db.SaveChangesAsync();
            }
        }
        public static List<Voter> GetVoters()
        {
            using (var db = new SqliteDbContext())
            {
                return db.Voters.ToList();
            }
        }
        public static async Task<List<ulong>> GetVoters(int amount)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Voters.Skip(await db.Voters.CountAsync() - amount).Take(amount).Select(x => x.UserId).ToListAsync();
            }
        }
        public static async Task<List<ulong>> GetVoters(DateTime dateFrom, DateTime dateTo)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Voters.Where(x => x.Date > dateFrom && x.Date < dateTo).Select(x => x.UserId).ToListAsync();
            }
        }
        public static async Task<int> VoteCount(ulong userId)
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Voters.CountAsync(x => x.UserId == userId);
            }
        }
        public static async Task<List<KeyValuePair<ulong, int>>> GetAllVotes()
        {
            using (var db = new SqliteDbContext())
            {
                return await db.Voters
                    .GroupBy(x => x.UserId)
                    .Select(x => new KeyValuePair<ulong, int>(x.Key, x.Count()))
                    .ToListAsync();
            }
        }
    }

    public static class PremiumDb
    {
        public static List<Premium> GetUserPremium(ulong UserId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Premiums.Where(x => x.UserId == UserId).ToList();
            }
        }
        public static List<Premium> GetGuildPremium(ulong GuildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Premiums.Where(x => x.GuildId == GuildId).ToList();
            }
        }
        public static bool IsPremium(ulong Id, PremiumType type)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Premiums.Any(x => x.Type == type && (x.GuildId == Id || x.UserId == Id));
            }
        }
        public static List<Premium> GetAllPremiums()
        {
            using (var db = new SqliteDbContext())
            {
                return db.Premiums.ToList();
            }
        }
        public static List<Premium> GetAllPremiums(DateTime ClaimedBefore)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Premiums.Where(x => x.ClaimDate < ClaimedBefore).ToList();
            }
        }

        public async static Task<int> UpdatePremium(Premium premium)
        {
            using (var db = new SqliteDbContext())
            {
                db.Premiums.Update(premium);
                return await db.SaveChangesAsync();
            }
        }
        public async static Task<int> DeletePremium(Premium premium)
        {
            using (var db = new SqliteDbContext())
            {
                db.Premiums.Remove(premium);
                return await db.SaveChangesAsync();
            }
        }
        public async static Task<int> AddPremium(ulong userId, PremiumType type, ulong guildId = 0)
        {
            using (var db = new SqliteDbContext())
            {
                db.Premiums.Add(new Premium
                {
                    GuildId = guildId,
                    UserId = userId,
                    Type = type,
                    ClaimDate = System.DateTime.Now
                });
                return await db.SaveChangesAsync();
            }
        }
    }
}
