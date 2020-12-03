using System.Net.Http;
using System.Threading.Tasks;

namespace GamesShell.Services
{
    public class RawgApiClient
    {
        public async Task<string> GetGameInfo(string fileName)
        {
            var handler = new HttpClientHandler();
            var httpClient = new HttpClient(handler);
            var response = httpClient.GetAsync("https://api.rawg.io/api/games?page_size=1&search=" + fileName.Replace(" ", "%20")).Result;

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
