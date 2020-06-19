using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;

namespace Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaifuShopController : ControllerBase
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<List<Waifu>> Get([FromRoute] ulong id)
        {
            var waifus = await WaifuShopDb.GetWaifus(id);
            string[] images = { "sample0.gif", "sample1.jpg", "sample2.gif", "sample3.gif" };
            var rnd = new Random();
            waifus.ForEach(x => x.ImageUrl = "images/" + images[rnd.Next(4)]);
            return waifus;
        }
    }
}