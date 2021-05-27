using HtmlAgilityPack;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vkaudioposter;
using vkaudioposter_Console.Tools;

namespace vkaudioposter_Console.Parsers
{
    public class MusicParser
    {
        /// <summary>
        /// Music parser Spotify or Beatport
        /// </summary>
        /// <param name="trackscount"></param>
        /// <param name="style"></param>
        /// <param name="playlistId"></param>
        /// <param name="trackstop"></param>
        /// <returns></returns>
        public static Task Parser(int trackscount, vkaudioposter_ef.parser.Playlist style, string playlistId, string trackstop)
        {
            ///v 6.0.x
            //var config = SpotifyClientConfig
            //    .CreateDefault()
            //    .WithAuthenticator(new ClientCredentialsAuthenticator(clientId, clientSecret)); // takes care of access tokens
            //var spotify = new SpotifyClient(config);

            //AccessToken token = SpotifyTools.GetToken().Result;

            var config = SpotifyClientConfig
            .CreateDefault()
            .WithAuthenticator(new ClientCredentialsAuthenticator("CLIENT_ID", "CLIENT_SECRET"));//from env

            var spotify = new SpotifyClient(config);

            Console.WriteLine("Parsing: " + style.PlaylistName);
            //ГОВНО, если не находит свежих, перескакивает быстро -> не тот стиль (не успевает записать и прочесть из файла)
            //Текст в поле + для поста
            //File.WriteAllText(Program.stylefilename, style.PlaylistName);


            //style = styleTuple.Item1;

            //postMessage = styleTuple.Item3;
            //string playlist_id = style.PlaylistId;
            try
            {
                //For Spotify
                if (playlistId != null)
                // if (user_id != null || playlist_id != null) //Если Spotify Only
                {
                    //Не добавляем в плейлисты

                    //string fields = "items(added_by.id,track(name,artists))";
                    //int limit = 100; //default
                    //int offset = 0; //смещение = 0
                    //string market = "US"; //Сделать RU
                    SpotifyTools.SpotyParser(playlistId, spotify);
                }
                else //Если пустые user_id и playlist_id
                {

                    string style_search = null;

                    switch (style.PlaylistName)
                    {
                        case "All styles":
                            style_search = "all";
                            break;
                        case "AFRO HOUSE":
                            style_search = "afro-house/89/";
                            break;
                        case "BIG ROOM":
                            style_search = "big-room/79/";
                            break;
                        case "BREAKS":
                            style_search = "breaks/9/";
                            break;
                        case "DANCE":
                            style_search = "dance/39/";
                            break;
                        case "DEEP HOUSE":
                            style_search = "deep-house/12/";
                            break;
                        case "DRUM & BASS":
                            style_search = "drum-and-bass/1/";
                            break;
                        case "DUBSTEP":
                            style_search = "dubstep/18/";
                            break;
                        case "HARDCORE / HARD TECHNO":
                            style_search = "hardcore-hard-techno/2/";
                            break;
                        case "HIP-HOP / R&B":
                            style_search = "hip-hop-r-and-b/38/";
                            break;
                        case "HOUSE":
                            style_search = "house/5/";
                            break;
                        case "INDIE DANCE / NU DISCO":
                            style_search = "indie-dance-nu-disco/37/";
                            break;
                        case "LEFTFIELD BASS":
                            style_search = "leftfield-bass/85/";
                            break;
                        case "LEFTFIELD HOUSE & TECHNO":
                            style_search = "leftfield-house-and-techno/80/";
                            break;
                        case "MELODIC HOUSE & TECHNO":
                            style_search = "melodic-house-and-techno/90/";
                            break;
                        case "MINIMAL / DEEP TECH":
                            style_search = "minimal-deep-tech/14/";
                            break;
                        case "ELECTRO HOUSE":
                            style_search = "electro-house/17/";
                            break;
                        case "ELECTRONICA / DOWNTEMPO":
                            style_search = "electronica-downtempo/3/";
                            break;
                        case "FUNK / SOUL / DISCO":
                            style_search = "funk-soul-disco/40/";
                            break;
                        case "FUNKY / GROOVE / JACKIN' HOUSE":
                            style_search = "funky-groove-jackin-house/81/";
                            break;
                        case "FUTURE HOUSE":
                            style_search = "future-house/65/";
                            break;
                        case "GARAGE / BASSLINE / GRIME":
                            style_search = "garage-bassline-grime/86/";
                            break;
                        case "HARD DANCE":
                            style_search = "hard-dance/8/";
                            break;
                        case "PROGRESSIVE HOUSE":
                            style_search = "progressive-house/15/";
                            break;
                        case "PSY-TRANCE":
                            style_search = "psy-trance/13/";
                            break;
                        case "REGGAE / DANCEHALL / DUB":
                            style_search = "reggae-dancehall-dub/41/";
                            break;
                        case "TECH HOUSE":
                            style_search = "tech-house/11/";
                            break;
                        case "TECHNO":
                            style_search = "techno/6/";
                            break;
                        case "TRANCE":
                            style_search = "trance/7/";
                            break;
                        case "TRAP / FUTURE BASS":
                            style_search = "trap-future-bass/87/";
                            break;
                    }


                    string Url = "https://www.beatport.com/genre/" + style_search + trackstop; // +"?per-page=150"; //можно добавить


                    HtmlWeb web = new();
                    HtmlAgilityPack.HtmlDocument doc = web.Load(Url);

                    int sw = 2;
                    if (trackstop == "tracks")
                    {
                        sw = 2;
                    }
                    else
                    {
                        sw = 3;
                    }

                    //цикл для парсинга количества треков Beatport СДЕЛАТЬ ЧЕРЕЗ API!
                    for (int i = 1; i <= trackscount; i++)

                    {
                        string trackname = null, author = null, remixer = null;
                        try
                        {
                            trackname = doc.DocumentNode.SelectNodes("//*[@id=\"pjax-inner-wrapper\"]/section/main/div[2]/div[2]/ul/li[" + i + "]/div[" + sw + "]/div[1]/p[1]/a")[0].InnerText;
                            author = doc.DocumentNode.SelectNodes("//*[@id=\"pjax-inner-wrapper\"]/section/main/div[2]/div[2]/ul/li[" + i + "]/div[" + sw + "]/div[1]/p[2]")[0].InnerText;
                        }
                        catch (Exception ex)
                        {
                            Logging.ErrorLogging(ex);
                        }

                        Program.ChartList.Add(new SpotyTrack(trackname, remixer, author));
                    }

                }
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\nDownload tracks canceled.\r\n");
            }
            Program.parser_finished = true;
            return null;
        }

    }
}
