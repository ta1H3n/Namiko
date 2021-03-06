﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class ProfileDb
    {
        //Methods: hex code check (also gets hex colour)
        public static bool GetHex(out string color, ulong UserId)
        {
            using var DbContext = new NamikoDbContext();
            var profileColour = DbContext.Profiles.Where(x => x.UserId == UserId).Select(x => x.ColorHex).FirstOrDefault();
            if (profileColour == null)
                color = "";
            else
                color = profileColour.ToString();
            return !String.IsNullOrEmpty(profileColour);
        }
        public static async Task SetHex(Color color, ulong UserId)
        {
            using var DbContext = new NamikoDbContext();
            var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

            //checking hex validity (makes sure hex colour.len ALWAYS = 6)
            string colour = color.Name;
            int boundary = colour.Length;
            if (boundary < 6) colour = new string('0', 6 - boundary) + colour;

            //if this user has not set a custom colour
            if (profile == null)
            {
                DbContext.Add(new Profile { UserId = UserId, ColorHex = colour });
                await DbContext.SaveChangesAsync();
                return;
            }

            //setting profile values + updating stack
            profile.PriorColorHexStack = PushStackManagement(profile.ColorHex, profile.PriorColorHexStack);
            profile.ColorHex = colour;
            DbContext.Update(profile);
            await DbContext.SaveChangesAsync();
        }
        public static async Task HexDefault(ulong UserId)
        {
            using var DbContext = new NamikoDbContext();
            var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

            //if this user is not in the database
            if (profile == null)
            {
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

        //colour stack
        public static string GetHexStack(ulong UserId)
        {
            using var DbContext = new NamikoDbContext();
            var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();
            return profile.PriorColorHexStack;
        }
        public static async Task PopStack(ulong UserId)
        {
            using var DbContext = new NamikoDbContext();
            var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();
            string stack = profile.PriorColorHexStack;
            string popped_colour;

            //cant be smaller than 6 here, checks equal
            if (stack.Length == 6)
            {
                popped_colour = stack;
                stack = "";

                //provided there's more
            }
            else
            {
                popped_colour = stack.Split(";").FirstOrDefault();
                int boundary = popped_colour.Length + 1;
                stack = stack[boundary..];
            }

            //profile update
            profile.PriorColorHexStack = stack;
            profile.ColorHex = popped_colour;
            DbContext.Update(profile);
            await DbContext.SaveChangesAsync();
        }
        private static string PushStackManagement(string current, string stack)
        {
            stack = (String.IsNullOrEmpty(current) ? "" : current + ";") + stack;
            return (stack.Split(";").Length > 3) ? stack.Substring(0, stack.LastIndexOf(";")) : stack;
        }

        //Methods: quotes
        public static string GetQuote(ulong UserId)
        {
            using var DbContext = new NamikoDbContext();
            string quote = DbContext.Profiles.Where(x => x.UserId == UserId).Select(x => x.Quote).FirstOrDefault();
            return String.IsNullOrEmpty(quote) ? null : quote;
        }
        public static async Task SetQuote(ulong UserId, string quote)
        {
            using var DbContext = new NamikoDbContext();
            var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

            //if user does not exist
            if (profile == null)
            {
                DbContext.Add(new Profile { UserId = UserId, Quote = quote });
                await DbContext.SaveChangesAsync();
                return;
            }

            //setting quote
            profile.Quote = quote;
            DbContext.Update(profile);
            await DbContext.SaveChangesAsync();
        }

        //Methods: Images
        public static string GetImage(ulong userId)
        {
            using var DbContext = new NamikoDbContext();
            string image = DbContext.Profiles.Where(x => x.UserId == userId).Select(x => x.Image).FirstOrDefault();
            return String.IsNullOrEmpty(image) ? null : image;
        }
        public static async Task SetImage(ulong UserId, string image)
        {
            using var DbContext = new NamikoDbContext();
            var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

            //if user does not exist
            if (profile == null)
            {
                DbContext.Add(new Profile { UserId = UserId, Image = image });
                await DbContext.SaveChangesAsync();
                return;
            }

            //setting image
            profile.Image = image;
            DbContext.Update(profile);
            await DbContext.SaveChangesAsync();
        }

        //Lootboxes
        public static int GetLootboxOpenedAmount(ulong userId)
        {
            using var db = new NamikoDbContext();
            var user = db.Profiles.FirstOrDefault(x => x.UserId == userId);
            if (user == null)
                return 0;

            return user.LootboxesOpened;
        }
        public static async Task<List<KeyValuePair<ulong, int>>> GetAllLootboxOpenedAmount()
        {
            using var db = new NamikoDbContext();
            return await db.Profiles
.Where(x => x.LootboxesOpened > 0)
.Select(x => new KeyValuePair<ulong, int>(x.UserId, x.LootboxesOpened))
.ToListAsync();
        }
        public static async Task IncrementLootboxOpened(ulong userId, int amount = 1)
        {
            using var db = new NamikoDbContext();
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

        //Reputation
        public static int GetRepAmount(ulong userId)
        {
            using var db = new NamikoDbContext();
            var user = db.Profiles.FirstOrDefault(x => x.UserId == userId);
            if (user == null)
                return 0;

            return user.Rep;
        }
        public static async Task<int> IncrementRep(ulong userId, int amount = 1)
        {
            using var db = new NamikoDbContext();
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
        public static async Task<List<KeyValuePair<ulong, int>>> GetAllRep()
        {
            using var db = new NamikoDbContext();
            return await db.Profiles
.Where(x => x.Rep > 0)
.Select(x => new KeyValuePair<ulong, int>(x.UserId, x.Rep))
.ToListAsync();
        }

        //Profile
        public static async Task<Profile> GetProfile(ulong userId)
        {
            using var db = new NamikoDbContext();
            var profile = db.Profiles.FirstOrDefault(x => x.UserId == userId);
            if (profile == null)
            {
                profile = new Profile { UserId = userId };
                db.Profiles.Add(profile);
                await db.SaveChangesAsync();
            }

            return profile;
        }
        public static async Task UpdateProfile(Profile profile)
        {
            using var db = new NamikoDbContext();
            db.Profiles.Update(profile);
            await db.SaveChangesAsync();
        }
    }
}
