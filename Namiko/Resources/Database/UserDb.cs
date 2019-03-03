using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Namiko.Resources.Datatypes;
namespace Namiko.Resources.Database {
    class UserDb {

         //
        //Methods: hex code check (also gets hex colour)
        public static bool GetHex(out string color, ulong UserId) {
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
                    DbContext.Add(new Profile { UserId = UserId, ColorHex = colour});
                    await DbContext.SaveChangesAsync();
                    return;
                }

                //if colour exists, update stack
                //string stack = profile.ColorHex + " " + profile.PriorColorHexStack;
                //if( stack.Split(" ").Length > 5) stack = stack.Substring(0, stack.LastIndexOf(" "));
                string stack = PushStackManagement(profile.ColorHex, profile.PriorColorHexStack);

                //setting profile values
                profile.PriorColorHexStack = stack; //manualStack;
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

                //if colour exists, modify stack
                string stack = PushStackManagement(profile.ColorHex, profile.PriorColorHexStack);

                //string current = profile.ColorHex;
                //string stack = ((current != null || current != "")? current + " " : "" ) + profile.PriorColorHexStack;
                //if (stack.Split(" ").Length > 5) stack = stack.Substring(0, stack.LastIndexOf(" "));

                //
                profile.PriorColorHexStack = stack;
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
            stack = ((current == null || current == "") ? "" : current + ";") + stack;
            return (stack.Split(";").Length > 3)? stack.Substring(0, stack.LastIndexOf(";")) : stack;
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

                //verafying image existance
                string image = DbContext.Profiles.Where(x => x.UserId == userId).Select(x => x.Image).FirstOrDefault();
                if (image == "" || image == null)
                    return null;

                //if exists
                return image;
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
}
