using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Namiko.Resources.Datatypes;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Namiko.Resources.Datatypes
{
    public static class WelcomeMessageDb
    {
        public static async Task AddMessage(String message)
        {
            using (var DbContext = new SqliteDbContext())
            {
                DbContext.Add(new WelcomeMessage { Message = message });
                await DbContext.SaveChangesAsync();
            }
        }
        public static List<WelcomeMessage> GetMessages()
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.WelcomeMessages.ToList();
            }

        }
        public static async Task DeleteMessage(int id)
        {
            using (var DbContext = new SqliteDbContext())
            {
                WelcomeMessage message = DbContext.WelcomeMessages.Where(x => x.Id == id).First();
                DbContext.WelcomeMessages.Remove(message);
                await DbContext.SaveChangesAsync();
            }
        }
        public static WelcomeMessage GetMessage(int id)
        {
            using (var DbContext = new SqliteDbContext())
            {
                WelcomeMessage message = DbContext.WelcomeMessages.Where(x => x.Id == id).First();
                return message;
            }
        }
        public static string GetRandomMessage()
        {
            using (var DbContext = new SqliteDbContext())
            {
                int count = DbContext.WelcomeMessages.Count();
                if (count < 1)
                {
                    return null;
                }
                else
                {
                    Random rand = new Random();
                    int random = rand.Next(count);
                    return DbContext.WelcomeMessages.ToArray<WelcomeMessage>().ElementAt(random).Message;
                }
            }
        }
        public static List<WelcomeChannel> GetWelcomeChannels()
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.WelcomeChannels.ToList();
            }
        }
        public static ulong GetWelcomeChannel(ulong guildId)
        {
            using (var db = new SqliteDbContext())
            {
                return db.WelcomeChannels.FirstOrDefault(x => x.GuildId == guildId).ChannelId;
            }
        }
        public static async Task SetWelcomeChannel(WelcomeChannel ch)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.WelcomeChannels.Where(x => x.GuildId == ch.GuildId).Count() < 1)
                {
                    DbContext.Add(ch);
                }
                else
                {
                    DbContext.Update(ch);
                }
                await DbContext.SaveChangesAsync();
            }
        }
    }
}
