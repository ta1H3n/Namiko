using Microsoft.AspNetCore.Mvc;
using Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website.Extensions;
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
            return res.ToView();
        }
    }
}