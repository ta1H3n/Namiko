using Discord.Net;
using Discord.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using System.Threading.Tasks;
using Website.Extensions;
using Website.Models;

namespace Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuildController : ControllerBase
    {
        [Authorize]
        [HttpGet("{guildId}")]
        public async Task<IActionResult> GetGuild([FromRoute] ulong guildId)
        {
            var client = await HttpContext.GetBotClient();
            RestGuildUser currentUser = null;
            try
            {
                currentUser = await client.GetGuildUserAsync(guildId, HttpContext.GetUserId());
            }
            catch (HttpException)
            {
                return StatusCode(4011, "Bot not in guild");
            }
            if (currentUser == null)
            {
                return StatusCode(403, "Unauthorized");
            }

            var res = await client.GetGuildAsync(guildId);

            var guild = new GuildView
            {
                ImageUrl = res.IconUrl,
                Id = res.Id,
                Name = res.Name
            };

            return Ok(guild);
        }

        [Authorize]
        [HttpGet("{guildId}/{userId}")]
        public async Task<IActionResult> GetGuildUser([FromRoute] ulong guildId, [FromRoute] ulong userId)
        {
            var client = await HttpContext.GetBotClient();
            RestGuildUser currentUser = null;
            RestGuildUser searchUser = null;
            try
            {
                currentUser = await client.GetGuildUserAsync(guildId, HttpContext.GetUserId());
                searchUser = await client.GetGuildUserAsync(guildId, userId);
            }
            catch (HttpException)
            {
                return StatusCode(4011, "Bot not in guild");
            }
            if (currentUser == null)
            {
                return StatusCode(403, "Unauthorized");
            }
            if (searchUser == null)
            {
                return StatusCode(404, "No user in guild");
            }

            var guild = await client.GetGuildAsync(guildId);
            var profile = await ProfileDb.GetProfile(searchUser.Id);

            var user = new GuildUserView
            {
                AvatarUrl = searchUser.GetAvatarUrl(size: 256),
                Id = searchUser.Id,
                Name = searchUser.Nickname,
                ImageUrl = profile.Image,
                Quote = profile.Quote,
                LootboxesOpened = profile.LootboxesOpened,
                Rep = profile.Rep,
                Balance = BalanceDb.GetToasties(userId, guildId),
                Daily = DailyDb.GetDaily(userId, guildId).Streak,
                Waifus = UserInventoryDb.GetWaifus(userId, guildId).ToView(),
                JoinedAt = searchUser.JoinedAt,
                Guild = new GuildSummaryView
                {
                    ImageUrl = guild.IconUrl,
                    Id = guild.Id,
                    Name = guild.Name
                }
            };

            return Ok(user);
        }
    }
}
