using System;
using System.Collections.Generic;
using System.Text;
using Booru;

namespace Namiko.Core.Web
{
    public static class BooruUtil
    {
        public async static void Test()
        {
            var booru = new Booru.Net.BooruClient();
            var kona = await booru.GetKonaChanImagesAsync("zerotwo", "order:score");
            foreach(var x in kona)
            {
                Console.WriteLine($"PostURL:   {x.PostUrl}");
                Console.WriteLine($"Rating:    {x.Rating}");
                Console.WriteLine($"Score:     {x.Score}");
                Console.WriteLine($"Tags:      ");
                foreach(var t in x.Tags)
                    Console.Write(t + " ");

                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("Count: " + kona.Count);
        }
    }
}
