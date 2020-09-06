using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Website.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        [HttpGet]
        public object Info()
        {
            try
            {
                var values = new List<KeyValuePair<string, string>>();
                var claims = HttpContext.User.Claims;
                var user = new UserInfo
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
            catch { return new UserInfo { LoggedIn = false }; }
        }

        [HttpGet("login")]
        [HttpPost("login")]
        public IActionResult LogIn([FromQuery] string returnUrl = null)
        {
            if (returnUrl == null)
                returnUrl = "/authenticationcallback";
            else
                returnUrl = "/authenticationcallback&#63;returnUrl=" + returnUrl;

            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "Discord");
        }

        [HttpGet("logout")]
        [HttpPost("logout")]
        public IActionResult LogOut([FromQuery] string returnUrl = null)
        {
            if (returnUrl == null)
                returnUrl = "/authenticationcallback";
            else
                returnUrl = "/authenticationcallback&#63;returnUrl=" + returnUrl;

            return SignOut(new AuthenticationProperties { RedirectUri = returnUrl }, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private class UserInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Discriminator { get; set; }
            public string AvatarHash { get; set; }
            public string AvatarUrl { get; set; }
            public bool LoggedIn { get; set; }
        }
    }
}
