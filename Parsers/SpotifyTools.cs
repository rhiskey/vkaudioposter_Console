using Newtonsoft.Json;
using SpotifyAPI.Web;
//using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using vkaudioposter;
using vkaudioposter_Console.Tools;

namespace vkaudioposter_Console.Parsers
{
    class SpotifyTools
    {
        /// <summary>
        /// DEPRECIATED SOON!
        /// </summary>
        /// <param name="playlistid"></param>
        /// <param name="fields"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="market"></param>
        /// <param name="api"></param>
        //Spotify Parser
        public static async Task SpotyParser(string playlistid, SpotifyClient api)
        {
            string fullartists;
            string trackname;

            try
            {
                var playlist = await api.Playlists.Get(playlistid);

                if (playlist.Uri != null)
                {

                    foreach (PlaylistTrack<IPlayableItem> item in playlist.Tracks.Items)
                    {
                        if (item.Track is FullTrack track)
                        {
                            fullartists = null;

                            List<SimpleArtist> artists = new();
                            artists = track.Artists;
                            trackname = track.Name;

                            if (artists.Count != 0)
                            {
                                foreach (var artist in artists)
                                    fullartists += artist.Name.ToString() + " ";

                                var eUrl = track.ExternalUrls;
                                var preUrl = track.PreviewUrl;
                                Program.ChartList.Add(new SpotyTrack(trackname, fullartists, eUrl, preUrl));
                                //Program.ChartList.Add(new SpotyTrack(trackname, fullartists, eUrl, preUrl, track.Popularity));
                            }
                            else continue;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ErrorLogging(ex);
                Logging.ReadError();
            }

        }
        public static async Task<vkaudioposter.AccessToken> GetToken()
        {
            Console.WriteLine("Getting Token");
            string credentials = string.Format("{0}:{1}", Program.clientId, Program.clientSecret);

            using HttpClient client = new();
            //Define Headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));
            //Prepare Request Body
            List<KeyValuePair<string, string>> requestData = new()
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            FormUrlEncodedContent requestBody = new(requestData);
            //Request Token
            HttpResponseMessage request = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
            string response = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<vkaudioposter.AccessToken>(response);
        }
    }
}
