using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GameShell.Models;
using Newtonsoft.Json.Linq;

namespace GamesShell.Services
{
    public class GameLibraryService
    {
        public static async Task<BitmapImage> ReadLibraryAsync(string fileName)
        {
            try
            {
                var _apiClient = new RawgApiClient();
                var gameInfoJson = await _apiClient.GetGameInfo(fileName);

                var resultObject = AllChildren(JObject.Parse(gameInfoJson))
                    .First(c => c.Type == JTokenType.Array && c.Path.Contains("results"))
                    .Children<JObject>().FirstOrDefault();

                var gameInfo = resultObject.ToObject<GameInfo>();

                var index = gameInfo.BackgroundImageUrl.IndexOf("games");

                using (var stream = DownloadImage(new Uri(gameInfo.BackgroundImageUrl.Insert(index, "crop/600/400/"))))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return bitmap;
                }
            }
            catch {
                return new BitmapImage();
            }
        }

        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }

        public static Stream DownloadImage(Uri uri)
        {
            var request = WebRequest.Create(uri);
            var response = request.GetResponse();
            using (var stream = response.GetResponseStream())
            {
                byte[] buffer = new byte[response.ContentLength];
                int offset = 0, actuallyRead = 0;
                do
                {
                    actuallyRead = stream.Read(buffer, offset, buffer.Length - offset);
                    offset += actuallyRead;
                }
                while (actuallyRead > 0);
                return new MemoryStream(buffer);
            }
        }
    }
}
