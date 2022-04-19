using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namiko
{
    public static class ImgurAPI
    {
        private static ImgurClient Client;
        public static IRateLimit RateLimit { get { return Client.RateLimit; } }

        public async static Task ImgurSetup()
        {
            ApiSettings settings = GetApiLogin();

            Client = new ImgurClient(settings.ClientId, settings.ClientSecret);
            var endpoint = new OAuth2Endpoint(Client);
            IOAuth2Token token = null;

            while(token == null)
            {
                try
                {
                    token = await endpoint.GetTokenByRefreshTokenAsync(settings.RefreshToken);
                } catch (Exception ex)
                {
                    token = null;
                    Console.WriteLine(ex.Message);
                    await Task.Delay(5000);
                }
                break;
            }

            Client.SetOAuth2Token(token);
            
            Console.WriteLine("Imgur Ready.");
        }

        public static async Task<IAlbum> CreateAlbumAsync(string name, string description = null)
        {
            var endpoint = new AlbumEndpoint(Client);
            var album = await endpoint.CreateAlbumAsync(name, description);
            return album;
        }

        public static async Task<IImage> UploadImageAsync(string url, string albumId = null, string name = null, string description = null)
        {
            var endpoint = new ImageEndpoint(Client);
            var image = await endpoint.UploadImageUrlAsync(url, albumId, name, description);
            return image;
        }

        public static async Task<bool> EditImageAsync(string imageId, string title = null, string description = null)
        {
            var endpoint = new ImageEndpoint(Client);
            return await endpoint.UpdateImageAsync(imageId, title, description);
        }

        public static async Task<IAlbum> GetAlbumAsync(string id)
        {
            var endpoint = new AlbumEndpoint(Client);
            var album = await endpoint.GetAlbumAsync(id);
            return album;
        }

        public static async Task<IAccount> GetAccountAsync()
        {
            var endpoint = new AccountEndpoint(Client);
            var account = await endpoint.GetAccountAsync();
            return account;
        }

        public static string ParseAlbumLink(string id)
        {
            return "https://imgur.com/a/" + id;
        }

        public static string ParseId(string imgurUrl)
        {
            return imgurUrl.Split('/').Last().Split('.').First();
        }

        public static string GetAuthorizationUrl()
        {
            ApiSettings settings = GetApiLogin();
            Client = new ImgurClient(settings.ClientId, settings.ClientSecret);
            var endpoint = new OAuth2Endpoint(Client);

            return endpoint.GetAuthorizationUrl(Imgur.API.Enums.OAuth2ResponseType.Token);
        }

        public static void SetRefreshToken(string refreshToken)
        {
            AppSettings.Imgur.RefreshToken = refreshToken;
            AppSettings.Save();
        }

        public static ApiSettings GetApiLogin()
        {
            return AppSettings.Imgur;
        }
    }
}
