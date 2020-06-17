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
            waifus.ForEach(x => x.ImageUrl = "images/sample.gif");
            return waifus;
        }
    }
}