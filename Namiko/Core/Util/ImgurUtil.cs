﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Imgur;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Namiko.Data;
using Newtonsoft.Json;

namespace Namiko.Core.Util
{
    public static class ImgurUtil
    {
        private static ImgurClient Client;

        public async static Task ImgurSetup()
        {
            string JSON = "";
            string JSONLocation = Locations.ImgurJSON;
            using (var Stream = new FileStream(JSONLocation, FileMode.Open, FileAccess.Read))
            using (var ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            ImgurLogin settings = JsonConvert.DeserializeObject<ImgurLogin>(JSON);

            Client = new ImgurClient(settings.ClientId, settings.ClientSecret);
            var endpoint = new OAuth2Endpoint(Client);
            Imgur.API.Models.IOAuth2Token token = null;

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
    }
}
