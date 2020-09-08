using Microsoft.AspNetCore.Mvc;
using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Extensions;
using Website.Models;

namespace Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaifuController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<WaifuView>> Search([FromQuery] string search)
        {
            var res = await WaifuDb.SearchWaifus(search, perPage: 50).ConfigureAwait(false);
            var waifus = res.ToView();

            waifus = waifus.OrderByDescending(x => x.Bought).ToList();
            return waifus;
        }

        [HttpGet("{name}")]
        public async Task<WaifuDetailedView> Get([FromRoute] string name)
        {
            var res = await WaifuDb.GetWaifu(name);
            return res.ToView();
        }
    }
}