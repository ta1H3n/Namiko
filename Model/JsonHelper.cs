using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class JsonHelper
    {
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
