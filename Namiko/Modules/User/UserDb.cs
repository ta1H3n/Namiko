﻿using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Namiko {
    public static class UserDb {

         //
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
    }

    public static class MarriageDb {
        public static bool GetMarriageState(ulong userID, out ulong wife, ulong guildID) {

            //the C# compiler should wrap this into a single Db call
            wife = GetWife(userID, guildID);
            return GetIsMarried(userID, guildID);
        }
        
        //Methods: wife basics 
        public static async Task SetWife(ulong userID, ulong wifeID, ulong guildID) {
            using (var DbContext = new SqliteDbContext()) {
                var wife = DbContext.Marriages.Where(x => x.UserId == userID).Where(x => x.GuildId == guildID).FirstOrDefault();

                //basic null check
                if (wife == null) {
                    DbContext.Add(new Marriage { UserId = userID, WifeId = wifeID, GuildId = guildID });
                    await DbContext.SaveChangesAsync();
                    return;
                }

                //if marriage exists, just change wives 
                wife.WifeId = wifeID;
                DbContext.Update(wife);
                await DbContext.SaveChangesAsync();
            }
        }
        public static ulong GetWife(ulong userID, ulong guildID) { using (var DbContext = new SqliteDbContext()) return DbContext.Marriages.Where(x => x.UserId == userID).Where(x => x.GuildId == guildID).Select(x => x.WifeId).FirstOrDefault(); }

        //Methods: marriage status, isMarried should *only* be used after SetWife
        public static async Task SetIsMarried(ulong userID, ulong guildID, bool isMarried = false) {
            using (var DbContext = new SqliteDbContext()) {
                var wife = DbContext.Marriages.Where(x => x.UserId == userID).Where(x => x.GuildId == guildID).FirstOrDefault();

                //basic null check
                if(wife == null) return;
                wife.IsMarried = isMarried;
                DbContext.Update(wife);
                await DbContext.SaveChangesAsync();
            }
        }
        public static bool GetIsMarried(ulong userID, ulong guildID) { using (var DbContext = new SqliteDbContext()) return DbContext.Marriages.Where(x => x.UserId == userID).Where(x => x.GuildId == guildID).Select(x => x.IsMarried).FirstOrDefault(); }
    }

    public static class VoteDb
    {
        public static List<Voter> GetVoters()
        {
            using (var db = new SqliteDbContext())
            {
                return db.Voters.ToList();
            }
        }

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
            foreach(var x in voters)
            {
                await AddVoter(new Voter { UserId = x });
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
        public static List<Voter> GetVoters(int amount)
        {
            using (var db = new SqliteDbContext())
            {
                return db.Voters.Skip(db.Voters.Count() - amount).Take(amount).ToList();
            }
        }
    }
}
