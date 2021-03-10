using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static void SpotyParser(string playlistid, string fields, int limit, int offset, string market, SpotifyWebAPI api)
        {
            string fullartists;
            string trackname;

            try
            {
                Paging<PlaylistTrack> res = api.GetPlaylistTracks(playlistid, fields, limit, offset, market);

                if (res.Items != null)
                {

                    foreach (PlaylistTrack obj in res.Items)
                    {
                        fullartists = null;

                        List<SimpleArtist> artists = new List<SimpleArtist>();

                        artists = null;
                        if (obj.Track != null)
                            artists = obj.Track.Artists;

                        trackname = obj.Track.Name.ToString();
                        //TODO: (update spotiapi)
                        //string prevURL = obj.Track.PreviewUrl;
                        //var id = obj.Track.Id;
         
                        if (artists != null)
                        {

                            foreach (SimpleArtist artist in artists)
                            {
                                fullartists += artist.Name.ToString() + " ";
                            }

                            Program.ChartList.Add(new Chart(trackname, fullartists));  
                        }
                        else continue;

                    }
                }
            }
            catch (Exception ex)
            {
                Logging.ErrorLogging(ex);
                Logging.ReadError();
            }

        }
        //Spotify Authorisation
        public static async Task<vkaudioposter.AccessToken> GetToken()
        {
            Console.WriteLine("Getting Token");
            string credentials = string.Format("{0}:{1}", Program.clientId, Program.clientSecret);

            using HttpClient client = new HttpClient();
            //Define Headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));
            //Prepare Request Body
            List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                };
            FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);
            //Request Token
            HttpResponseMessage request = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
            string response = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<vkaudioposter.AccessToken>(response);
        }
    }
}
