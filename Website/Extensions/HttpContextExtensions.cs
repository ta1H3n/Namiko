using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Website.Models;
using Website.Services;

namespace Website
{
    public static class HttpContextExtensions
    {
        public static ulong GetUserId(this HttpContext http)
        {
            var claim = http.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (claim == null || claim.Value == null)
            {
                return 0;
            }
            else
            {
                return UInt64.Parse(claim.Value);
            }
        }

        public static User GetUser(this HttpContext http)
        {
            try
            {
                var claims = http.User.Claims;
                var user = new User
                {
                    Id = claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value,
                    Name = claims.First(x => x.Type == ClaimTypes.Name).Value,
                    Discriminator = claims.First(x => x.Type == "urn:discord:user:discriminator").Value,
                    AvatarHash = claims.First(x => x.Type == "urn:discord:avatar:hash").Value,
                    AvatarUrl = claims.First(x => x.Type == "urn:discord:avatar:url").Value,
                    LoggedIn = true
                };
                return user;
            }
            catch { return new User { LoggedIn = false }; }
        }

        public static async Task<DiscordRestClient> GetUserDiscordClientAsync(this HttpContext http)
        {
            var id = http.GetUserId();
            var token = await http.GetTokenAsync("access_token");

            if (id == 0 || token == null || token == "")
                return null;

            else
                return await DiscordRestClientService.GetClient(id, token);
        }
    }
}
