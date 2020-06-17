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
    public class WaifuController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<Waifu>> Get([FromQuery] string search = "")
        {
            var waifus = await WaifuDb.SearchWaifus(search, perPage: 50);
            string[] images = { "sample0.gif", "sample1.jpg", "sample2.gif", "sample3.gif" };
            var rnd = new Random();
            waifus.ForEach(x => x.ImageUrl = "images/" + images[rnd.Next(4)]);
            return waifus;
        }
    }
}