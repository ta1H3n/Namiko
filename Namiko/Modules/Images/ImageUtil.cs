using Discord;
using Discord.WebSocket;
using Model;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Namiko.Images;

namespace Namiko
{
    public static class ImageUtil
    {
        public static HttpClient _client = new HttpClient();

        public static EmbedBuilder ToEmbed(ReactionImage img)
        {
            EmbedBuilder embed = new EmbedBuilder();

            string path = $"{AppSettings.ImageUrlPath + "reaction/"}{img.Name}{(img.GuildId > 0 ? "/" + img.GuildId.ToString() : "")}/{img.Id}.{img.ImageFileType}";

            embed.WithImageUrl(path);
            embed.WithFooter($"{img.Name} id: {img.Id}");
            embed.WithColor(BasicUtil.RandomColor());
            return embed;
        }

        public static EmbedBuilder ListAllEmbed(List<ImageCount> images, string prefix = "!", IUser author = null)
        {
            var eb = new EmbedBuilderPrepared(author);
            eb.WithDescription($"<:Awooo:582888496793124866> `{prefix}pat` - use reaction images!\n" +
                $"<:NadeYay:564880253382819860> `{prefix}album pat` - link to an imgur album with all the images!\n" +
                $"<:KannaHype:571690048001671238> `{prefix}i 20` - use a specific reaction image by id!");
            
            if (images.Any(x => x.Count >= 20))
            {
                string list = "";
                foreach (var x in images.Where(x => x.Count >= 20))
                {
                    list += "`" + x.ToString() + "` ";
                }
                eb.AddField("20+ images", list);
            }

            if (images.Any(x => x.Count >= 10 && x.Count < 20))
            {
                string list = "";
                foreach (var x in images.Where(x => x.Count >= 10 && x.Count < 20))
                {
                    list += "`" + x.ToString() + "` ";
                }
                eb.AddField("10+ images", list);
            }

            if (images.Any(x => x.Count >= 5 && x.Count < 10))
            {
                string list = "";
                foreach (var x in images.Where(x => x.Count >= 5 && x.Count < 10))
                {
                    list += "`" + x.ToString() + "` ";
                }
                eb.AddField("5+ images", list);
            }

            if (images.Any(x => x.Count > 0 && x.Count < 5))
            {
                string list = "";
                foreach (var x in images.Where(x => x.Count >= 1 && x.Count < 5))
                {
                    list += "`" + x.ToString() + "` ";
                }
                eb.AddField("<5 images", list);
            }

            return eb;
        }
        public static EmbedBuilder AddGuildImagesToEmbed(EmbedBuilder eb, IEnumerable<string> imageNames)
        {
            string list = "";
            if (imageNames.Count() > 0)
            {
                foreach (var x in imageNames)
                {
                    list += "`" + x + "` ";
                }
            }
            else
                list += "~ Add Server exclusive reaction images using the `ni` command. Requires Pro Guild+ ~";

            eb.AddField("Server images", list);
            
            return eb;
        }


        //Image handlers
        public static async Task UploadReactionImage(ReactionImage img, ISocketMessageChannel ch, BaseSocketClient client)
        {
            try
            {
                if (img.Url == null || img.Url == "")
                    return;

                string to = "reaction";
                string path = Path.Combine(to, img.Name);
                if (img.GuildId > 0)
                {
                    path = Path.Combine(path, img.GuildId.ToString());
                }

                string fileName = $"{img.Id}.{img.ImageFileType}";

                var res = await UploadImage(path, fileName, img.Url);
                if (res != null)
                {
                    await ch.SendMessageAsync($"Failed to upload image to host: `{res}`");
                }
            }
            catch (Exception ex)
            {
                await ch.SendMessageAsync($"{client.GetUser(AppSettings.OwnerId).Mention} Error while uploading image to host.");
                SentrySdk.WithScope(scope =>
                {
                    scope.SetExtras(img.GetProperties());
                    SentrySdk.CaptureException(ex);
                });
            }
        }

        public static void CreateIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static async Task<string> UploadImage(string path, string name, string imageUrl)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, AppSettings.ImageHost + "Image/Upload"))
            {
                var json = JsonConvert.SerializeObject(new { imageUrl, path, name });
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;
                    request.Headers.Add("authorization", AppSettings.ImageHostKey);

                    using (var response = await _client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        .ConfigureAwait(false))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return null;
                        }
                        else
                        {
                            return response.ReasonPhrase;
                        }
                    }
                }
            }
        }
    }
}