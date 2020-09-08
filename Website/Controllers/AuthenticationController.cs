using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Website.Models;

namespace Website.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        [HttpGet]
        public User UserInfo()
        {
            return HttpContext.GetUser();
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
    }
}
