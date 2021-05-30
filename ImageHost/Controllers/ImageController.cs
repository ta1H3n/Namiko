using ImageHost.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ImageHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public ImageController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> Upload([FromBody] ImageUploadModel img)
        {
            if (!Request.Headers.TryGetValue("authorization", out var key) || key != _config["authorization"])
            {
                return NotFound();
            }

            if (img.ImageUrl == null || img.ImageUrl == "")
            {
                return BadRequest("Url missing");
            }

            string path = Path.Combine(_env.WebRootPath, "images", img.Path);
            CreateIfNotExists(path);
            if (Directory.Exists(Path.Combine(path, img.Name)))
            {
                return BadRequest($"Image already exists at {Path.Combine(img.Path, img.Name)}");
            }

            using WebClient client = new WebClient();
            await client.DownloadFileTaskAsync(img.ImageUrl, Path.Combine(path, img.Name));

            return Ok();
        }

        public static void CreateIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
