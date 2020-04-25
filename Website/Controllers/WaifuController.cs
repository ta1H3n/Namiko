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
        public async Task<IEnumerable<Waifu>> Get()
        {
            var waifus = await WaifuDb.SearchWaifus("azur");
            return waifus;
        }
    }
}