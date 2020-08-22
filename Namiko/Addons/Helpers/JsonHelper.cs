using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Namiko
{
    public static class JsonHelper
    {
        public static async Task<bool> SaveJson<T>(T item, string path)
        {
            await Task.Run(() =>
            {
                var json = JsonConvert.SerializeObject(item, Formatting.Indented);
                using var Stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                Stream.Write(Encoding.ASCII.GetBytes(json));
            });

            return true;
        }

        public static async Task<T> ReadJson<T>(string path)
        {
            T item = default;
            await Task.Run(() => 
            {
                string JSON = "";
                using (var Stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var ReadSettings = new StreamReader(Stream))
                {
                    JSON = ReadSettings.ReadToEnd();
                }
                item = JsonConvert.DeserializeObject<T>(JSON);
            });

            return item;
        }
    }
}
