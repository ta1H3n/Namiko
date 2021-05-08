using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;

namespace Website.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private NamikoDbContext _context;
        public UserController(NamikoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMe()
        {
            var client = await HttpContext.GetUserDiscordClientAsync();
            var guilds = await client.GetGuildSummariesAsync().FlattenAsync();
            var profile = await ProfileDb.GetProfile(client.CurrentUser.Id);

            var user = new UserView
            {
                AvatarUrl = client.CurrentUser.GetAvatarUrl(size: 256),
                Id = client.CurrentUser.Id.ToString(),
                Name = client.CurrentUser.Username,
                Discriminator = client.CurrentUser.Discriminator,
                ImageUrl = profile.Image,
                Quote = profile.Quote,
                LootboxesOpened = profile.LootboxesOpened,
                Rep = profile.Rep,
                Guilds = guilds.Select(x => new GuildSummaryView
                {
                    ImageUrl = x.IconUrl,
                    Id = x.Id.ToString(),
                    Name = x.Name,
                }).OrderBy(x => x.Name).ToList()
            };

            return Ok(user);
        }
    }
}
