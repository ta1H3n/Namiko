using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class ParamDb
    {
        public static async Task AddParam(Param param)
        {
            using var db = new SqliteDbContext();
            db.Params.Add(param);
            await db.SaveChangesAsync();
        }

        public static List<Param> GetParam(int id = 0, string name = "")
        {
            using var db = new SqliteDbContext();
            var res = db.Params.Where(x => x.Id == id && x.Name == name).ToList();
            if (!res.Any())
            {
                res = db.Params.Where(x => x.Id == id || x.Name == name).ToList();
            }

            return res;
        }

        public static async Task UpdateParam(Param param)
        {
            using var db = new SqliteDbContext();
            if (!db.Params.Any(x => x.Id == param.Id))
                db.Params.Add(param);

            else
                db.Params.Update(param);

            await db.SaveChangesAsync();
        }
    }
}
