using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public static class WelcomeMessageDb
    {
        public static async Task AddMessage(String message)
        {
            using var DbContext = new NamikoDbContext();
            DbContext.Add(new WelcomeMessage { Message = message });
            await DbContext.SaveChangesAsync();
        }
        public static List<WelcomeMessage> GetMessages()
        {
            using var DbContext = new NamikoDbContext();
            return DbContext.WelcomeMessages.ToList();

        }
        public static async Task DeleteMessage(int id)
        {
            using var DbContext = new NamikoDbContext();
            WelcomeMessage message = DbContext.WelcomeMessages.Where(x => x.Id == id).FirstOrDefault();
            DbContext.WelcomeMessages.Remove(message);
            await DbContext.SaveChangesAsync();
        }
        public static WelcomeMessage GetMessage(int id)
        {
            using var DbContext = new NamikoDbContext();
            WelcomeMessage message = DbContext.WelcomeMessages.Where(x => x.Id == id).FirstOrDefault();
            return message;
        }
        public static string GetRandomMessage()
        {
            using var DbContext = new NamikoDbContext();
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
}
