using Microsoft.AspNetCore.Mvc;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaifuShopController : ControllerBase
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<List<WaifuView>> Get([FromRoute] ulong id)
        {
            var res = await WaifuShopDb.GetWaifus(id).ConfigureAwait(false);
            var waifus = res.Select(x => new WaifuView
            {
                Bought = x.Bought,
                Description = x.Description,
                ImageLarge = "images/waifus/" + x.ImageLarge,
                ImageMedium = "images/waifus/" + x.ImageMedium,
                ImageRaw = "images/waifus/" + x.ImageRaw,
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
                foreach (var w in waifus)
                {
                    int i = rnd.Next(4);
                    w.ImageLarge = "images/" + images[i];
                    w.ImageMedium = "images/" + images[i];
                    w.ImageRaw = "images/" + images[i];
                }
            }

            return waifus;
        }
    }
}