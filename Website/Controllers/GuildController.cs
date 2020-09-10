using Discord.Net;
using Discord.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
                Id = res.Id.ToString(),
                Name = res.Name
            };

            return Ok(guild);
        }

        [Authorize]
        [HttpGet("{guildId}/{userId}")]
        public async Task<IActionResult> GetGuildUser([FromRoute] ulong guildId, [FromRoute] ulong userId)
        {
            var client = await HttpContext.GetBotClient();
            Task<RestGuildUser> currentUser = client.GetGuildUserAsync(guildId, HttpContext.GetUserId());
            Task<RestGuildUser> searchUser = client.GetGuildUserAsync(guildId, userId);
            var guild = client.GetGuildAsync(guildId);
            var tasks = new List<Task>();
            try
            {
                tasks.Add(currentUser);
                tasks.Add(searchUser);
                tasks.Add(guild);
                await Task.WhenAll(tasks);
            }
            catch (HttpException) 
            {
                return StatusCode(4011, "Bot not in guild");
            }
            if (currentUser.Result == null)
            {
                return StatusCode(403, "Unauthorized");
            }
            if (searchUser.Result == null)
            {
                return StatusCode(404, "No user in guild");
            }

            var profile = await ProfileDb.GetProfile(searchUser.Result.Id);
            var bal = await BalanceDb.GetToastiesAsync(userId, guildId);
            var dailyRes = await DailyDb.GetDailyAsync(userId, guildId);
            var daily = dailyRes == null ? 0 : dailyRes.Streak;
            var waifus = (await UserInventoryDb.GetWaifusAsync(userId, guildId)).OrderBy(x => x.Source).ThenBy(x => x.Name).ToView();
            var waifu = FeaturedWaifuDb.GetFeaturedWaifu(userId, guildId).ToView();

            var user = new GuildUserView
            {
                AvatarUrl = searchUser.Result.GetAvatarUrl(size: 256),
                Id = searchUser.Result.Id.ToString(),
                Name = searchUser.Result.Username,
                Discriminator = searchUser.Result.Discriminator,
                ImageUrl = profile.Image,
                Quote = profile.Quote.CleanQuote(),
                LootboxesOpened = profile.LootboxesOpened,
                Rep = profile.Rep,
                Balance = bal,
                Daily = daily,
                Waifus = waifus,
                JoinedAt = searchUser.Result.JoinedAt,
                Waifu = waifu,
                Guild = new GuildSummaryView
                {
                    ImageUrl = guild.Result.IconUrl,
                    Id = guild.Result.Id.ToString(),
                    Name = guild.Result.Name
                }
            };

            return Ok(user);
        }
    }
}
