using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Website.Models;

namespace Website.Extensions
{
    public static class ConvertersExtensions
    {
        public static string ImageHost;

        public static List<WaifuView> ToView(this IEnumerable<Waifu> waifus)
        {
            var res = waifus.Select(x => new WaifuView
            {
                Bought = x.Bought,
                Description = x.Description,
                ImageLarge = ImageHost + "images/waifus/" + x.ImageLarge,
                ImageMedium = ImageHost + "images/waifus/" + x.ImageMedium,
                ImageRaw = ImageHost + "images/waifus/" + x.ImageRaw,
                ImageSource = x.ImageSource,
                LongName = x.LongName,
                Name = x.Name,
                Source = x.Source,
                Tier = x.Tier
            }).ToList();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                string[] images = { "sample0.gif", "sample1.jpg", "sample2.gif", "sample3.gif" };
                var rnd = new Random();
                foreach (var w in res)
                {
                    int i = rnd.Next(4);
                    w.ImageLarge = "images/" + images[i];
                    w.ImageMedium = "images/" + images[i];
                    w.ImageRaw = "images/" + images[i];
                }
            }

            return res;
        }

        public static WaifuDetailedView ToView(this Waifu waifu)
        {
            if (waifu == null)
                return null;

            var res = new WaifuDetailedView
            {
                Bought = waifu.Bought,
                Description = waifu.Description,
                ImageLarge = ImageHost + "images/waifus/" + waifu.ImageLarge,
                ImageMedium = ImageHost + "images/waifus/" + waifu.ImageMedium,
                ImageRaw = ImageHost + "images/waifus/" + waifu.ImageRaw,
                ImageSource = waifu.ImageSource,
                LongName = waifu.LongName,
                Name = waifu.Name,
                Source = waifu.Source,
                Tier = waifu.Tier,
                MalId = waifu.Mal == null ? 0 : waifu.Mal.MalId
            };

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                string[] images = { "sample0.gif", "sample1.jpg", "sample2.gif", "sample3.gif" };
                var rnd = new Random();
                int i = rnd.Next(4);
                res.ImageLarge = "images/" + images[i];
                res.ImageMedium = "images/" + images[i];
                res.ImageRaw = "images/" + images[i];
            }

            return res;
        }
    }
}
