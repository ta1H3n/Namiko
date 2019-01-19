using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Namiko.Resources.Datatypes;
namespace Namiko.Resources.Database {
    class UserDb {

         //
        //Methods: hex code check (also gets hex colour)
        public static bool CheckHex(out string color, ulong UserId) {
            using (var DbContext = new SqliteDbContext()) {

                //verafying user existance
                var profileColour = DbContext.Profiles.Where(x => x.UserId == UserId).Select(x => x.ColorHex).FirstOrDefault();

                //default colour just incase
                color = "";
                if (profileColour == null || profileColour == "")
                    return false;
                
                //getting colour
                color = profileColour.ToString();
                return true;
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
                    DbContext.Add(new Profile { UserId = UserId, ColorHex = colour, Quote = "" });
                    await DbContext.SaveChangesAsync();
                    return;
                }

                //if colour exists
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
                    DbContext.Add(new Profile { UserId = UserId, ColorHex = "", Quote = "" });
                    await DbContext.SaveChangesAsync();
                    return;
                }

                //if colour exists
                profile.ColorHex = "";
                DbContext.Update(profile);
                await DbContext.SaveChangesAsync();
            }
        }

        //Methods: quotes
        public static string GetQuote(ulong UserId) {
            using (var DbContext = new SqliteDbContext()) {

                //verafying quote existance
                string quote = DbContext.Profiles.Where(x => x.UserId == UserId).Select(x => x.Quote).FirstOrDefault();
                if(quote == "" || quote == null)
                    return null;

                //if exists
                return quote;
            }
        }
        public static async Task SetQuote(ulong UserId, string quote) {
            using (var DbContext = new SqliteDbContext()) {
                var profile = DbContext.Profiles.Where(x => x.UserId == UserId).FirstOrDefault();

                //if user does not exist
                if (profile == null) {
                    DbContext.Add(new Profile { UserId = UserId, ColorHex = "", Quote = quote });
                    await DbContext.SaveChangesAsync();
                    return;
                }

                //setting quote
                profile.Quote = quote;
                DbContext.Update(profile);
                await DbContext.SaveChangesAsync();
            }
        }
    }
}
