using Microsoft.AspNetCore.Mvc;
using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<Module>> Get()
        {
            var modules = await CommandDb.GetModulesAsync(new string[4] { "SpecialModes", "Special", "Basic", "WaifuEditing" }).ConfigureAwait(false);
            modules.ForEach(x => x.Commands.ForEach(y => y.Module = null));
            return modules;
        }
    }
}