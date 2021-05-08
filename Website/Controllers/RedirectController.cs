using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Models.Logging;
using System;

namespace Website.Controllers
{
    [Route("[controller]")]
    public class RedirectController : Controller
    {
        private readonly NamikoDbContext _context;

        public RedirectController(NamikoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<IActionResult> IndexAsync(
            [FromQuery] string redirectUrl, 
            [FromQuery] string tag,
            [FromQuery] string type)
        {
            var click = new Click
            {
                Date = DateTime.Now,
                DiscordId = HttpContext.GetUserId(),
                OriginTag = tag,
                Referer = Request.Headers["Referrer"],
                Ip = Request.Host.Host,
                RedirectUrl = redirectUrl,
                Type = type
            };

            _context.Clicks.Add(click);
            await _context.SaveChangesAsync();

            return Redirect(redirectUrl);
        }
    }
}
