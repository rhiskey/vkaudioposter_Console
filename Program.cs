using System;
using HtmlAgilityPack;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using vkaudioposter.MySQL;
using vkaudioposter.Classes;
using VkNet.AudioBypassService.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using vkaudioposter;
using VkNet.Enums;
using vkaudioposter_Console.VKUtills;
using vkaudioposter_Console.Parsers;
using System.Data;
using vkaudioposter_Console.Tools;
using vkaudioposter_Console.API;
using vkaudioposter_Console.Classes;

namespace vkaudioposter_Console
{
    class Program
    {
        #region StaticVars
        //TODO: read from .env
        public static string DB_HOST; //local
        public static string DB_NAME;
        public static string DB_USER;
        public static string DB_PASS;

        //ПолучаетсяВ приложении своем
        //Приложение группы доступ к группе, получается в самом приложении в ручную Standalone приложение		
        //s static string accesstoken; //добавить фото и музыку вложения на стену
        //логи в личку
        public static string kateMobileToken;

        public static long adminID; //для отправки ЛС
        public static string torHost;
        public static int torPort;

        public static bool threadstopflag = false;
        public static bool saveLogs = true;

        public static string redisHost;
        public static int redisPort;
        public static string redisPassword;

        public static string efUser;
        public static string efPass;
        public static string efDB;

        //Авторизация для публикаций https://vkhost.github.io - сообщество + приложение
        //Получается https://oauth.vk.com/authorize?client_id=7361627&scope=notify,photos,friends,audio,video,notes,pages,docs,status,questions,offers,wall,groups,notifications,stats,ads,offline&redirect_uri=http://api.vk.com/blank.html&display=page&response_type=token                                                                                                               
        public static string Token; //kateMobileToken; //Загрузка фото + публикация треков (сделать через свое приложение)
        public static string accesstoken; //add tracks to attachments
        public static ulong? ownerid3; //для сохранения фото на стене
        public static ulong? groupid3;//для  сохранения фото на стене
        public static long groupid; //для загрузки на сервер
                                                     //public static string pusherAppId;
                                                     //public static string pusherAppKey;

        //public static string pusherAppSecret;
        public static long ownid;
        public static int hoursperiod = 2; //период отложенных записей
        public static int minutesperiod = 20; //период отложенных записей в минутах
        public static bool parser_finished = false;

        public static string HashTags = "#HighVolumeMusic #HVM @hvmlabel #edm #music #электронная #музыка #танцевальная #новинкимузыки #свежая_музыка";
        //Spotify
        public static string clientId;
        public static string clientSecret;
        public static Random random = new Random();

        private static readonly int searchCoolDown = 700; //ms для поиска по apiяws
        
        private static bool startOnce; //For Sheduler

        private static readonly string url1 = "https://api-vk.com/api.php?method=search&q=";
        private static string url3; //new Api https://api-vk.com

        private static DateTime publication_date; //для wall.post
        private static string photofilename = "tempimage.jpg"; //скачанное фото
        private static readonly int trackscount = 100; //100
        private static DateTime LastDatePosted = new DateTime();
        private static string userAccessToken; //Скачать музыку если не apiяws
        private static bool apiWS = true; //Использовать при поиске APIЯWS или VK API (бесплатно)

        private static List<FormattedPlaylist> allPlaylists;
        public static List<Chart> ChartList = new List<Chart>();
        #endregion


        private string MessageToAttach;
        private bool posted = false; //опубликован?
        private bool tracks_attached = false;
        private bool tracksfound = false;

        private string switcher = null;
        private int postcounter = 1; //номер скачанной фотки в стоке
        private string photourl = null; //Прямая ссылка на фото

        private int cleared = 0;
        private string postMessage = null; //richTextBox1

        private const int max_photo = 10; //Макс фоток в контейнере (для девиан = 24) Defalt:10
        private const int stockMaxPages = 5; //Макс число страниц для парсинга 

        public List<string> SelectedGenre = new List<string>();
        public List<string> PhotoStock = new List<string>();

        private List<Track> SearchingList = new List<Track>(); //Список найденных треков

        private List<Track> SelectedTrackList = new List<Track>();
        private List<MediaAttachment> attachments = new List<MediaAttachment>();
        //For COnsole
        private List<string> CB_PhotoStock = new List<string>();
        //private List<string> LSTBOX_Genres = new List<string>();

        private List<string> LstBox_AddedTracks = new List<string>();

        Queue<Photostock_class> photostockQueue = new Queue<Photostock_class>();
        Queue<WallPostParams> wallPostQueue = new Queue<WallPostParams>();


        #region Config
        /// <summary>
        /// 
        /// </summary>
        private static void LoadConfigsFromEnv()
        {
            DotNetEnv.Env.Load();

            hoursperiod = DotNetEnv.Env.GetInt("HOURS_PERIOD");
            minutesperiod = DotNetEnv.Env.GetInt("MINUTES_PERIOD");
            DB_HOST = DotNetEnv.Env.GetString("DB_HOST");
            DB_USER = DotNetEnv.Env.GetString("DB_USER");
            DB_PASS = DotNetEnv.Env.GetString("DB_PASS");
            DB_NAME = DotNetEnv.Env.GetString("DB_NAME");
            accesstoken = DotNetEnv.Env.GetString("ACCESS_TOKEN");
            kateMobileToken = DotNetEnv.Env.GetString("KATE_MOBILE_TOKEN");
            Token = DotNetEnv.Env.GetString("TOKEN");
            string searchApiUrlPrefix = "&key=";
            string searchApiUrlPostfix = "&v=3";
            url3 = searchApiUrlPrefix + DotNetEnv.Env.GetString("URL") + searchApiUrlPostfix;
            var env_ownerid3 = DotNetEnv.Env.GetInt("OWNER_ID");
            ownerid3 = Convert.ToUInt64(env_ownerid3);
            var env_groupid3 = DotNetEnv.Env.GetInt("GROUP_ID");
            groupid3 = Convert.ToUInt64(env_groupid3);
            groupid = (long)groupid3;
            ownid = -(long)groupid3;
            //HashTags = DotNetEnv.Env.GetString("HASH_TAGS");
            clientId = DotNetEnv.Env.GetString("CLIENT_ID");
            clientSecret = DotNetEnv.Env.GetString("CLIENT_SECRET");
            userAccessToken = DotNetEnv.Env.GetString("USER_ACCESS_TOKEN");
            adminID = DotNetEnv.Env.GetInt("ADMIN_ID");

            //pusherAppId = DotNetEnv.Env.GetString("PUSHER_APP_ID");
            //pusherAppKey = DotNetEnv.Env.GetString("PUSHER_APP_KEY");
            //pusherAppSecret = DotNetEnv.Env.GetString("PUSHER_APP_SECRET");

            startOnce = DotNetEnv.Env.GetBool("START_ONCE");

            torHost = DotNetEnv.Env.GetString("TOR_HOST");
            torPort = DotNetEnv.Env.GetInt("TOR_PORT");
            saveLogs = DotNetEnv.Env.GetBool("SAVE_LOGS");

            redisHost = DotNetEnv.Env.GetString("REDIS_HOST");
            redisPort = DotNetEnv.Env.GetInt("REDIS_PORT");
            redisPassword = DotNetEnv.Env.GetString("REDIS_PASSWORD");

            efDB = DotNetEnv.Env.GetString("EF_DATABASE");
            efUser = DotNetEnv.Env.GetString("EF_USER");
            efPass = DotNetEnv.Env.GetString("EF_PASSWORD");
        }

        private void OnLoad()
        {
            LoadConfigsFromEnv();

            allPlaylists = DBUtils.GetAllPlaylists();
            allPlaylists = Tools.Shufflers.ShuffleList(allPlaylists);

            CB_PhotoStock = DBUtils.LoadPhotoStocksFromDB();
            CB_PhotoStock = Tools.Shufflers.ShuffleList(CB_PhotoStock);

            foreach (var item in CB_PhotoStock)
            {
                photostockQueue.Enqueue(new Photostock_class()
                {
                    URL = item.ToString()
                });
            }

            foreach (var stock in photostockQueue)
            {
                Console.WriteLine(stock.URL);
            }
        }

        #endregion

        private static void Main(string[] args)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = false;
            }).Start();

            try
            {
                Thread rabbitReciever = new Thread(new ThreadStart(Rabbit.CommandsReciever))
                {
                    IsBackground = false
                };
                //rabbitReciever.Start();

                //SendTestTrackMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                LoadConfigsFromEnv();
                // Create Database with schema 
                vkaudioposter_ef.Program.LoadConfig();

                ///If want to delete -> pass TRUE             
                //vkaudioposter_ef.CreateInitialSchema.CreateSchema(false);
                //vkaudioposter_ef.Program.InsertData(false);
                //vkaudioposter_ef.Program.CreateStoredProceduresViewsAndFunctions(false);

                if (startOnce == true)
                    StatusChecker.ApiStart();
            }
        }

        #region Appcycle
        /// <summary>
        /// Autostart or api
        /// </summary>
        public class StatusChecker
        {

            public static void ApiStart()
            {
                Program P = new Program();

                P.OnLoad();

                var dateTuple = DBUtils.GetLastDateFromDB(); 
                
                publication_date = dateTuple.Item1;
                LastDatePosted = dateTuple.Item2;

                P.cleared = 0; //обнуление счетчика срабатываний ClearAll()

                if (P.wallPostQueue.Count != 0) //Если в очереди есть посты
                {
                    VkApi api = new VkApi();

                    api.Authorize(new ApiAuthParams
                    {
                        AccessToken = Token
                    });
                    bool publError = false;

                    // Для каждого поста из БД
                    foreach (var postQ in P.wallPostQueue) //Для каждлого поста в очереди
                    {
                        try
                        {
                            var post = api.Wall.Post(new WallPostParams
                            {
                                Attachments = postQ.Attachments,
                                OwnerId = postQ.OwnerId,
                                Message = postQ.Message,
                                PublishDate = postQ.PublishDate
                            });

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Отложенный Пост (перед парсингом) опубликован!");
                            P.ClearAll();

                        }
                        catch (Exception ex)
                        {
                            Logging.ErrorLogging(ex);
                            Logging.ReadError();
                            publError = true;
                            Console.WriteLine($"Ошибка публикации. Число постов в очереди: {P.wallPostQueue.Count}");
                        }
                        finally
                        {
                            //Если без ошибок опубликовали, значит удаляем из очереди
                            if (publError == false)
                            {
                                // Удалили пост из БД
                                P.wallPostQueue.Dequeue(); //Удалили этот элем из очереди
                            }
                            Console.WriteLine($"Число постов в очереди: {P.wallPostQueue.Count}");

                            Thread.Sleep(2000);
                        }
                    }
                }
                else //Если очередь пуста
                {
                    P.Automatica(allPlaylists);
                }

            }
        }

        /// <summary>
        /// Logic of app
        /// </summary>
        /// <param name="playlists"></param>
        /// <returns></returns>
        public Thread Automatica(List<FormattedPlaylist> playlists)
        {
            try
            {
                //Rabbit.NewLog("Start Parser");

                string trackstop = "tracks";
                if (switcher == "fresh")
                { trackstop = "tracks"; }
                if (switcher == "top100")
                { trackstop = "top-100"; }

                postcounter = 1;
                int ind = 0;

                int threshhold = 0; //порог количества треков для публикации
                string photostock_new = null;
                bool photo_exist = false;


                foreach (var style in playlists)
               {
                    photo_exist = false;

                    var wallTotal = VkTools.CheckPostponedAndGetCount();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Отложенных {wallTotal} постов");
                    //Rabbit.NewLog($"Отложенных {wallTotal} постов");

                    // TODO: Остановка
                    if (threadstopflag == true || wallTotal == 150)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");
                        Logging.ErrorLogging($"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");
                        //Rabbit.NewLog($"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");
                        break;
                    }

                    Console.WriteLine(style.PlaylistName);
                    Console.WriteLine(trackscount);
                    Console.WriteLine(trackstop);

                    try
                    {
                        Console.WriteLine($"playlist_id: {style.PlaylistId} ");

                        Thread result = StartTheParserThread(trackscount, style, style.trueID, trackstop);
                        postMessage = StringWorkers.GetPostMessageFromStyle(style.PlaylistName);
                        do { Thread.Sleep(100); }
                        while (parser_finished == false);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }

                    //Rabbit.NewLog($"Search Tracks in VK: {style.PlaylistName}");
                    SearchTracksVk(style);
                    do
                    {
                        Thread.Sleep(100);
                    }
                    while (tracksfound == false); //

                    if (SearchingList.Count > threshhold)
                    {
                        if (postcounter >= max_photo) //Если число постов больше макс числа фото, сбрасываем счетчик, удаляем из очереди фотосток, добавляем в конец его же
                        {
                            postcounter = 0; //Сброс

                            // получаем первый элемент стоков из очереди без его извлечения
                            Photostock_class pickedPhotostock = photostockQueue.Peek();

                            //Добавляем в конец очереди этот же фотосток
                            photostockQueue.Enqueue(pickedPhotostock);

                            // извлекаем первый элемент очереди
                            Photostock_class queuePhotostock = photostockQueue.Dequeue(); //теперь очередь из оставшихся стоков + в конце тот же сток
                            photostock_new = queuePhotostock.URL;
                        }
                        else // если меньше, просто берем элемент след. фото из текущего стока
                        {
                            // получаем первый элемент без его извлечения
                            Photostock_class pickedPhotostock = photostockQueue.Peek();
                            photostock_new = pickedPhotostock.URL;
                        }

                        int stockPage = 1; //только с 1ой страницы
                        try
                        {
                            //Меняем фотку
                            postcounter++;

                            //Rabbit.NewLog("Photo Parser started");
                            photourl = PhotoParserAuto(photostock_new, postcounter, style.PlaylistName, stockPage);

                            if (photourl == null)
                            {
                                // Девка дефолт
                                photourl = "https://sun9-60.userapi.com/c638422/v638422659/24de8/rdpXft1B6Pw.jpg";
                                Console.ForegroundColor = ConsoleColor.Red;
                                Logging.ErrorLogging(String.Format("Пустая ссылка на фото, сток: {0}", photostock_new));
                                throw new Exception(String.Format("Пустая ссылка на фото, сток: {0}", photostock_new));

                            }
                            else photo_exist = true;
                        }
                        catch (Exception ex)
                        {
                            Logging.ErrorLogging(ex); Logging.ReadError(); photo_exist = false;

                            postcounter = 0; //Сброс

                            // получаем первый элемент стоков из очереди без его извлечения
                            Photostock_class pickedPhotostock = photostockQueue.Peek();

                            //Добавляем в конец очереди этот же фотосток
                            photostockQueue.Enqueue(pickedPhotostock);

                            // извлекаем первый элемент очереди
                            Photostock_class queuePhotostock = photostockQueue.Dequeue(); //теперь очередь из оставшихся стоков + в конце тот же сток
                            photostock_new = queuePhotostock.URL;

                            photourl = PhotoParserAuto(photostock_new, postcounter, style.PlaylistName, stockPage);
                            if (photourl == null)
                            { photourl = "https://sun9-48.userapi.com/c638422/v638422659/24e71/pWGAQj9rKgk.jpg"; photo_exist = true; }// Default photo 2 High Volume Music 

                        }
                        finally
                        {
                            //Rabbit.NewLog("Download Photo");
                            try
                            {
                                bool isImageExist = ImageWorkers.DownloadImage(photourl, photofilename);
                                if (!isImageExist)
                                {
                                    //TODO (BAD)
                                    throw new Exception();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.ErrorLogging(ex);
                                Logging.ReadError();
                                // Если не смогли скачать основную и заглушку
                                using WebClient webClient = new WebClient();
                                //Качаем заглушку 3 - pink girl
                                webClient.DownloadFile(@"https://sun9-68.userapi.com/impg/alHziWJBnm2jUWkW4F0CNnsC1nTmpjrE38Xlmg/0AE3-4o5K6M.jpg?size=1200x1414&quality=96&proxy=1&sign=7b63d1207aa5e2de667afd982d14937c&type=album", photofilename);
                                //webClient.DownloadFile("https://sun9-71.userapi.com/c638422/v638422659/24dde/CrNKNnDTC1M.jpg", photofilename);
                                webClient.Dispose();
                                photo_exist = true;
                            }

                        }

                        var autoAddTuple = VkTools.AutoAddTracksToAttachments(SearchingList, LstBox_AddedTracks, attachments);
                        LstBox_AddedTracks = autoAddTuple.Item1;
                        attachments = autoAddTuple.Item2;
                        tracks_attached = autoAddTuple.Item3;

                        do
                        {
                            Thread.Sleep(100);

                        } while (tracks_attached == false);

                        Console.WriteLine("Добавили треки в список вложений");

                        Console.WriteLine("Размещаем пост на стену");
                        //Rabbit.NewLog("Размещаем пост на стену");

                        if (photo_exist == true) //если вообще скачалась фотка
                        {
                            var attsTuple = VkTools.AddPhotoToAttachFromUrl(photofilename, attachments, postMessage, LstBox_AddedTracks);
                            attachments = attsTuple;
                        }

                        PosterOnWall(attachments, style);
                        do
                        {
                            Thread.Sleep(100);

                        } while (posted == false);
                        ind++;
                    }
                    else //SearchingList=0 если не найдено в поиске треков, что делать? Переступать на след. шаг, нужно ли очищать что-то? Вроде нет
                    {
                        parser_finished = false;
                        tracksfound = false;
                        continue;
                    }

                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Процесс завершен!");
                //Rabbit.NewLog("Процесс завершен!");
                postcounter = 1;
            }
            catch (ThreadAbortException exc)
            {
                Console.WriteLine("Поток прерван, код завершения "
                         + exc.ExceptionState);
            }
            Console.WriteLine($"Процесс завершен. Текущее время: {DateTime.Now:g}");
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackscount"></param>
        /// <param name="style"></param>
        /// <param name="playlistId"></param>
        /// <param name="trackstop"></param>
        /// <returns></returns>
        public static Thread StartTheParserThread(int trackscount, vkaudioposter_ef.parser.Playlist style, string playlistId, string trackstop)
        {
            Parser(trackscount, style, playlistId, trackstop);
            return null;
        }
        
        /// <summary>
        /// Music patsing from Spotify or Beatport
        /// </summary>
        /// <param name="trackscount"></param>
        /// <param name="style"></param>
        /// <param name="playlistId"></param>
        /// <param name="trackstop"></param>
        /// <returns></returns>
        private static Task Parser(int trackscount, vkaudioposter_ef.parser.Playlist style, string playlistId, string trackstop)
        {
            ///v 6.0.x
            //var config = SpotifyClientConfig
            //    .CreateDefault()
            //    .WithAuthenticator(new ClientCredentialsAuthenticator(clientId, clientSecret)); // takes care of access tokens
            //var spotify = new SpotifyClient(config);


            AccessToken token = SpotifyTools.GetToken().Result;

            //Нужно каждый раз получать токеn новый
            var spotify = new SpotifyWebAPI
            {
                AccessToken = token.access_token,
                TokenType = "Bearer"
            };

            Console.WriteLine("Parsing: " + style.PlaylistName);

            try
            {
                //For Spotify
                if (playlistId != null)
                // if (user_id != null || playlist_id != null) //Если Spotify Only
                {
                    string fields = "items(added_by.id,track(name,artists))";
                    int limit = 100; //default
                    int offset = 0; //смещение = 0
                    string market = "US"; //Сделать RU
                    SpotifyTools.SpotyParser(playlistId, fields, limit, offset, market, spotify);
                }
                else //Если пустые user_id и playlist_id -> Beatport (DEPRECIATED)
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

                    string Url = "https://www.beatport.com/genre/" + style_search + trackstop; // +"?per-page=150"; 

                    HtmlWeb web = new HtmlWeb();
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

                        Program.ChartList.Add(new Chart(trackname, remixer, author));
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="styletothread"></param>
        /// <returns></returns>
        private Task SearchTracksVk(FormattedPlaylist styletothread)
        {
            attachments.Clear();
            SearchingList.Clear();

            LstBox_AddedTracks.Clear();
            posted = false;

            try
            {
                MakeUrlFromLines(ChartList, styletothread);
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\nSearch canceled.\r\n");
            }

            return null;
        }

        /// <summary>
        /// Создает из чарта Коллекцию вложенных аудио
        /// </summary>
        /// <param name="tracksToFind"></param>
        /// <param name="styletoDB"></param>
        public void MakeUrlFromLines(List<Chart> tracksToFind, FormattedPlaylist styletoDB)
        {
            VkApi api = new VkApi();

            if (apiWS == false)
                try
                {
                    var services = new ServiceCollection();
                    services.AddAudioBypass();
                    api = new VkApi(services);

                    api.Authorize(new ApiAuthParams
                    {
                        AccessToken = userAccessToken
                    });
                }
                catch (Exception ex)
                {

                    Logging.ErrorLogging(ex);
                }

            string url2;
            int existcounter = 0;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Список треков из парсера:\n");

            foreach (var line in tracksToFind)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{line.GetTrackAndAuthors()}");

            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nЖанр: {styletoDB.PlaylistName}");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Ищем в поиске треки");


            int unsearchtracks = 0; //не найденоы
            int publishedtracks = 0; //уже опубликовано


            //для каждой строки с названием
            foreach (var trackobj in tracksToFind)
            {
                string K = trackobj.GetTrackAndAuthors();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Текущий трек: {K}");

                //trackname = trackname.Replace("\'", "");

                //Обращаться к БД каждый раз как пытаемся новый K трек найти, Если в БД нет такого - то ищем
                var postedTracks = DBUtils.GetPostedTracksFromDB(styletoDB);
                var unfoundTracks = DBUtils.GetUnfoundTracksFromDB(styletoDB);

                //Приводим название к видукак В БД
                string current_track = K.Replace("\'", "\"");

                List<string> fmtUnfoundTracks = new List<string>();
                foreach (var uT in unfoundTracks)
                {
                    var newT = uT.Replace("%20", "");
                    fmtUnfoundTracks.Add(newT);
                }

                //unfoundTracks = unfoundTracks.Replace("%20", "");

                List<string> fmtPostedTracks = new List<string>();
                foreach (var pT in postedTracks)
                {
                    var newT = pT.Replace("%20", "");
                    fmtPostedTracks.Add(newT);
                }


                if ((fmtPostedTracks.IndexOf(current_track) != -1) /*&& (Output.Length != 0))*/ || (fmtUnfoundTracks.IndexOf(current_track) != -1))    
                {
                    //Нашли такой же трек- он был опубликован, значит пропускаем
                    publishedtracks++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Нашли опубликованный трек");
                    continue;

                }
                else //Если не нашли совпадения в архиве или архив пустой (новый жанр), то
                {
                    if (apiWS == true)
                    {
                        //Если такого трека нет в файле, выполняем поиск в ВК 
                        url2 = current_track;
                        string url = JsonTools.ConcatSearchReq(url1, url2, url3);
  

                        //---------------Old-------------------------
                        string json = JsonTools.SearchWeb(url);

                        if (json != null)
                        {
                            string strWithoutSpaces = url2.Replace("%20", " "); //Запрос поиска Имя+трек+микс
                            string FullId = StringWorkers.GetFullIdFromString(strWithoutSpaces, json);//нашли в запросе ID первой песни 

                            if (FullId != "0") //если нашли трек в поиске
                            {
                                //Попытаться трек в БД записать
                                try
                                {
                                    DBUtils.InsertFoundTrackInDB(current_track, styletoDB, publication_date, false);
                                }
                                catch (Exception ex) //Если любая ошибка, перейти к след.треку!
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Dublicate in PostedTracks: {current_track}");
                                    Console.WriteLine($"{ ex.Message}");
                                    Console.WriteLine($"{ ex.StackTrace}");
                                    Console.WriteLine($"{ ex.InnerException}");
                                    Console.WriteLine($"{ ex.Data}");
                                    try
                                    {
                                        DBUtils.InsertUnfoundTrackInDB(current_track, styletoDB, false);
                                    }
                                    catch (Exception ex2) { Console.ForegroundColor = ConsoleColor.DarkRed; Console.WriteLine("Dublicate in UnfoundTracks...skip"); continue; }

                                    continue;
                                }
         
                                //Rabbit.NewPostedTrack(current_track, styletoDB.PlaylistName, publication_date);
                                SearchingList.Add(new Track(url2, FullId));

                                //Добавить треки в Quue очередь или класс при публикации заливать, очищать при нажатии отмена
                                existcounter++;
                            }
                            else
                            {
                                unsearchtracks++;
                                try
                                {
                                    DBUtils.InsertUnfoundTrackInDB(current_track, styletoDB, false);
                                }
                                catch (Exception e) { Console.ForegroundColor = ConsoleColor.DarkGray; Console.WriteLine("Dublicate in UnfoundTracks...skip");/* Logging.ErrorLogging(e); */ continue; };
                            }
                            //если не нашли не добавляем в массив
                            //если счетчик достиг 9 треков, остановить поиск!
                            if (existcounter == 9)
                            {
                                break;
                            }
                            //----------------------OLD------------
                        }
                        else { break; }
                    }
                    else //Если используем API VK (Бесплатно)
                    {
                        string fullTrackName = null;
                        Uri mp3Url = null;
                        long? mediaID = 0;
                        long? ownID = 0;

                        // Ищем аудиозаписи, сортированных по дате добавления.
                        int totalCount = 30;
                        try
                        {
                            var audios = api.Audio.Search(new AudioSearchParams
                            {
                                Autocomplete = false,
                                Query = current_track,
                                Count = totalCount,
                                //Offset = 0,
                                SearchOwn = false,
                                Sort = AudioSort.AddedDate
                            });

                            if (audios.Count != 0)
                            {
                                foreach (var audio in audios)
                                {
                                    string allArtists = null;
                                    mp3Url = audio.Url;
                                    var mainArtists = audio.MainArtists;
                                    string oneArtist = audio.Artist;
                                    string trackName = audio.Title;
                                    if (mainArtists.Count() > 1)
                                        foreach (var artist in mainArtists)
                                        {
                                            if (artist.Name != null)
                                            {
                                                allArtists += " " + artist.Name;
                                                fullTrackName = trackName + " " + allArtists;
                                            }
                                            else continue;
                                        }
                                    else
                                    {
                                        if (mainArtists != null || oneArtist != null)
                                            fullTrackName = trackName + " " + oneArtist;
                                        else continue;
                                    }

                                    //Сравнить название трека+исполнитель с тем что искали
                                    int diff = Tools.Metrics.LevenshteinDistance(current_track, fullTrackName); //Получение расстояния между строками исходной и результатов
                                    if (diff < 4 && diff != -1)
                                    {
                                        ownID = audio.OwnerId;
                                        mediaID = audio.Id;
                                        SearchingList.Add(new Track(fullTrackName, mediaID, ownID));
                                        break;
                                    }
                                    else continue;
                                }
                            }

                        }
                        catch (VkNet.Exception.ParameterMissingOrInvalidException ex)
                        {
                            Console.WriteLine(ex);
                        }
                        catch (System.NullReferenceException nullEx)
                        {
                            Console.WriteLine(nullEx);
                        }
                        //Попытаться трек в БД записать
                        try
                        {
                            DBUtils.InsertFoundTrackInDB(fullTrackName, styletoDB, publication_date, false);
                        }
                        catch (Exception ex) //Если любая ошибка, перейти к след.треку!
                        {
                            SearchingList.Remove(new Track(fullTrackName, mediaID, ownID));
                            Logging.ErrorLogging(ex);
                            continue;
                        }
                        finally
                        {
                            existcounter++;
                        }

                        //если счетчик достиг 9 треков, остановить поиск!
                        if (existcounter == 9) break;
                    }
                }
                Thread.Sleep(searchCoolDown); //Задержка поиска
            }

            if (SearchingList.Count == 0)
            {
                //TODO
            }

            tracksfound = true;
            return;
        }

        /// <summary>
        /// Parsing photos from Devianart, etc.
        /// </summary>
        /// <param name="photostock"></param>
        /// <param name="i"></param>
        /// <param name="music_style"></param>
        /// <param name="pickedStockPage"></param>
        /// <returns></returns>
        private string PhotoParserAuto(string photostock, int i, string music_style, int pickedStockPage) //i=postcounter
        {
            //int dig = i % max_photo; //Остаток от деления (кратность числу макс фото на странице стока)
            string url = null; //Прямая ссылка на фото
            //find url photo
            HtmlAgilityPack.HtmlDocument doc = null;

            ///выбираем сток, соответствующий стилю, надо сопоставить в БД каждому стилю свой сток
            ///Пробуем найти страницу со стилем по tag
            ///

            //switch (music_style)
            //{
            //    case var someVal when new Regex(@"metal|(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
            //        photostock = "https://www.deviantart.com/tag/brutal";
            //        break;
            //    case var someVal when new Regex(@"trance|(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
            //        photostock = "https://www.deviantart.com/whats-hot/?q=trance";
            //        break;
            //    case var someVal when new Regex(@"adrenaline(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
            //        photostock = "https://www.deviantart.com/whats-hot/?q=adrenaline";
            //        break;
            //    case var someVal when new Regex(@"house(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
            //        photostock = "https://www.pexels.com/search/music/";
            //        break;
            //}
            //int pagecounter = photostockQueue.Peek().Page;

            try
            {
                //скачиваем страницу
                try
                {
                    HtmlWeb web = new HtmlWeb();
                    doc = web.Load(photostock);
                }
                catch (Exception ex)
                {
                    Logging.ErrorLogging(ex);
                    Logging.ReadError();
                }
                //получение ссылки на фото
                string nodContainer = null;
                switch (photostock)
                {
                    case var someVal when new Regex(@"https://www.deviantart.com/topic/(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
                        nodContainer = "//*[@id=\"root\"]/div[1]/div/div/article/div/div[2]/div";//Контейнер с картинками на странице (последний grid)
                        try
                        {
                            url = PhotoParser.DevianPageParser(doc, nodContainer, i);
                        }
                        catch (System.NullReferenceException ex)
                        {
                            Logging.ErrorLogging(ex);
                            if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
                            {
                                //https://www.deviantart.com/?order=whats-hot&page=1
                                pickedStockPage++; //Перешли на след. страницу
                                                   //Сбросить счётчик фото
                                postcounter = 1;
                                i = postcounter;
                                //StringBuilder photostock_SB = new StringBuilder(photostock);
                                //photostock_SB.Append(new string { "&page=" + pagecounter });
                                photostock += "&page=" + pickedStockPage;

                                //скачиваем страницу
                                try
                                {
                                    photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });

                                }
                                catch (Exception ex1)
                                {
                                    Logging.ErrorLogging(ex1);
                                    Logging.ReadError();
                                }
                            }
                        }
                        break;

                    case "https://picjumbo.com/latest-free-stock-photos/":
                        try
                        {
                            url = doc.DocumentNode.SelectNodes("/html/body/div[5]/div/div[" + i + "]/a/picture/img")[0].Attributes[3].Value;
                        }
                        catch (System.NullReferenceException ex)
                        {
                            Logging.ErrorLogging(ex);
                            if (pickedStockPage <= stockMaxPages)
                            {
                                postcounter = 1;
                                i = postcounter;
                                photostock += "page/" + pickedStockPage + "/";

                                //скачиваем страницу
                                try
                                {
                                    photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });
                                }
                                catch (Exception ex1)
                                {
                                    Logging.ErrorLogging(ex1);
                                    Logging.ReadError();
                                }
                            }
                        }

                        break;
                    case "https://picjumbo.com/":
                        try
                        {
                            url = doc.DocumentNode.SelectNodes("/html/body/div[6]/div/div[" + i + "]/a[1]/picture/img")[0].Attributes[3].Value;
                        }
                        catch (System.NullReferenceException ex)
                        {
                            Logging.ErrorLogging(ex);
                            if (pickedStockPage <= stockMaxPages)
                            {
                                pickedStockPage++;
                                postcounter = 1;
                                i = postcounter;
                                photostock += "page/" + pickedStockPage + "/";

                                try
                                {
                                    photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });

                                }
                                catch (Exception ex1)
                                {
                                    Logging.ErrorLogging(ex1);
                                    Logging.ReadError();

                                }
                            }

                        }
                        break;

                    case "https://pixabay.com/ru/editors_choice":
                        url = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[2]/div/div/div[" + i + "]/a/img")[0].Attributes[1].Value;
                        break;

                    ///----------- BEGIN DEPRECIATED (УСТАРЕЛО)-----------
                    case var someVal when new Regex(@"https://www.deviantart.com/?order=whats-hot(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
                        nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/section/div/div[2]/div[2]";//Контейнер с картинками на странице 
                        try
                        {
                            url = PhotoParser.DevianPageParser(doc, nodContainer, i);
                        }
                        catch (System.NullReferenceException ex)
                        {
                            Logging.ErrorLogging(ex);
                            if (pickedStockPage <= stockMaxPages) 
                            {
                                //Если нет фотки
                                pickedStockPage++; 
                                postcounter = 1;
                                i = postcounter;
                                photostock += "&page=" + pickedStockPage;
                                try
                                {
                                    photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });
                                }
                                catch (Exception ex1)
                                {
                                    Logging.ErrorLogging(ex1);
                                    Logging.ReadError();
                                }
                            }

                        }
                        break;

                    case var someVal when new Regex(@"https://www.deviantart.com/tag/(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
                        nodContainer = "//*[@id=\"root\"]/div[2]/div/div/div/div[2]/div[2]";
                        try
                        {
                            url = PhotoParser.DevianPageParser(doc, nodContainer, i);
                        }
                        catch (System.NullReferenceException ex)
                        {
                            Logging.ErrorLogging(ex);
                            if (pickedStockPage <= stockMaxPages) 
                            {

                                pickedStockPage++; 
                                postcounter = 1;
                                i = postcounter;
                                photostock += "&page=" + pickedStockPage;
                                try
                                {
                                    photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });
                                }
                                catch (Exception ex1)
                                {
                                    Logging.ErrorLogging(ex1);
                                    Logging.ReadError();
                                }
                            }
                        }
                        break;

                    case var someVal when new Regex(@"https://www.deviantart.com/search?q=(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
                        nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[3]/div[2]";
                        url = PhotoParser.DevianPageParser(doc, nodContainer, i);
                        break;
                    case "https://www.tumblr.com/search/cool+photo/recent":
                        url = doc.DocumentNode.SelectNodes("//*[@id=\"search_posts\"]/article[" + i + "]/section[1]/div[1]/img")[0].Attributes[0].Value;
                        break;
                    case "https://www.stockvault.net/latest-photos/?li=3":
                        string url1 = doc.DocumentNode.SelectNodes("//*[@id=\"flexgrid\"]/div[" + i + "]/a/img")[0].Attributes[0].Value;
                        url = "https://www.stockvault.net" + url1;
                        break;
                    case "https://www.pexels.com/search/music/":
                        url = doc.DocumentNode.SelectNodes("/html/body/div[1]/div[3]/div[3]/div[1]/div[" + i + "]/article/a[1]/img")[0].Attributes[5].Value;
                        break;

                    case "https://nos.twnsnd.co/":
                        url = doc.DocumentNode.SelectNodes("//*[@id=\"posts\"]/div/article[" + i + "]/div/section[1]/figure/div/div/a/img")[0].Attributes[1].Value;
                        break;
                    case "https://littlevisuals.co/":
                        url = doc.DocumentNode.SelectNodes("//*[@id=\"main\"]/article[" + i + 1 + "]/a/img")[0].Attributes[0].Value;
                        break;

                    case var someVal when new Regex(@"https://www.canstockphoto.com/stock-photo-images/(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
                        url = doc.DocumentNode.SelectNodes("/html/body/div[3]/div[1]/div/div/div[2]/section/article[" + i + "]/a/span[1]/img")[0].Attributes[5].Value;
                        break;
                    ///-----------END DEPRECIATED (УСТАРЕЛО)-----------

                    case null:           
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Null passed to this method.\n No SWITCH IN STOCK");
                        break;
                }


                ///-----------BEGIN DEPRECIATED (УСТАРЕЛО)-----------

                if ((photostock == "https://pixabay.com/ru/") || (photostock == "https://pixabay.com/ru/photos/"))
                {
                    url = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[2]/div[1]/div[1]/div[" + i + "]/a/img")[0].Attributes[2].Value;
                }

                if ((photostock == "https://pixabay.com/images/search/?order=latest") || (photostock == "https://pixabay.com/images/search/"))
                {
                    nodContainer = "//*[@id=\"content\"]/div/div[2]/div/div[2]/div[" + i + "]/a/img";
                    url = PhotoParser.PixabayParser(doc, nodContainer, i);
                }

                if (photostock == "https://www.deviantart.com/")
                {
                    nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[2]/div[2]";
                    url = PhotoParser.DevianPageParser(doc, nodContainer, i);

                }

                if ((photostock == "https://www.deviantart.com/search/deviations?order=most-recent&q=wallpaper") ||
                    (photostock == "https://www.deviantart.com/?order=whats-hot") ||
                    (photostock == "https://www.deviantart.com/search/deviations?q=wallpaper&order=whats-hot"))
                {
                    //24 фото на странице
                    nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[2]/div[2]/div";
                    try
                    {
                        url = PhotoParser.DevianPageParser(doc, nodContainer, i);
                    }
                    catch (System.NullReferenceException ex)
                    {
                        Logging.ErrorLogging(ex);
                        if (pickedStockPage <= stockMaxPages) 
                        {
                            pickedStockPage++; 
                            postcounter = 1;
                            i = postcounter;
                            switch (photostock)
                            {
                                case "https://www.deviantart.com/?order=whats-hot":
                                    photostock = "https://www.deviantart.com/search/deviations?order=whats-hot&page=" + pickedStockPage;
                                    break;
                                case "https://www.deviantart.com/search/deviations?order=most-recent&q=wallpaper":
                                    photostock = "https://www.deviantart.com/search/deviations?order=most-recent&page=" + pickedStockPage + "&q=wallpaper";
                                    break;
                                case "https://www.deviantart.com/search/deviations?q=wallpaper&order=whats-hot":
                                    photostock = "https://www.deviantart.com/search/deviations?order=whats-hot&page=" + pickedStockPage + "&q=wallpaper";
                                    break;
                            }
                            try
                            {
                                photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });

                            }
                            catch (Exception ex1)
                            {

                                Logging.ErrorLogging(ex1);
                                Logging.ReadError();
                            }
                        }

                    }

                }


                if ((photostock == "https://www.deviantart.com/luckynumber113/favourites/76516524/Pretty-Wallpapers-Backgrounds-and-Designs") ||
                    (photostock == "https://www.deviantart.com/james-is-james/favourites/60347448/Abstract") ||
                    (photostock == "https://www.deviantart.com/psychospartanex/favourites/77875659/Amazing-Wallpapers"))
                {

                    nodContainer = "//*[@id=sub-folder-gallery]/div/div[2]/div/div/div[2]/div";
                    url = PhotoParser.DevianPageParser(doc, nodContainer, i);

                }

                //-------TEST-------
                if (photostock == "https://www.rawpixel.com/free-images?sort=curated&photo=1&premium=free&page=1")
                {
                    url = doc.DocumentNode.SelectNodes("//*[@id=\"page\"]/div/main/div/section/div/figure[" + i + "]/a")[0].Attributes[0].Value;
                }

                ///-----------END DEPRECIATED (УСТАРЕЛО)-----------

            }
            catch (Exception ex)
            {
                Logging.ErrorLogging(ex);
                Logging.ReadError();
                //Default Image
                url = "https://sun9-21.userapi.com/c626627/v626627659/5c68f/DWvyDcKOAJI.jpg";
            }
            return url;
        }

        /// <summary>
        /// Posting on wall
        /// </summary>
        /// <param name="attachments"></param>
        /// <param name="fmtPlaylist"></param>
        public void PosterOnWall(List<MediaAttachment> attachments, FormattedPlaylist? fmtPlaylist)
        {
            DateTime localDate = DateTime.Now;
            string cultureName = "ru-RU";
            var culture = new CultureInfo(cultureName);

            MessageToAttach = postMessage;

            Console.WriteLine("Авторизация для поста");
            VkApi api = new VkApi();
            //Авторизация
            api.Authorize(new ApiAuthParams
            {
                AccessToken = Token
            });

            bool changed_time = false;

            int compareValue = -1;
            try
            {
                compareValue = publication_date.CompareTo(DateTime.Now);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Value is not a DateTime");
            }

            if (compareValue < 0)
            {
                System.Console.WriteLine("{0:d} is in the past.", publication_date);
                publication_date = DateTime.Now;
                publication_date = publication_date.AddMinutes(minutesperiod);
            }
            else if (compareValue == 0)
            {
                System.Console.WriteLine("{0:d} is today!", publication_date);
                publication_date = publication_date.AddMinutes(minutesperiod);
            }
            else 
            {
                System.Console.WriteLine("{0:d} has not come yet.", publication_date);
            }

            if (cleared == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Будет опубликован: {publication_date}");

            }

            //Если уже 1 раз опубликовали, publication_date.AddHours(1) увеличили на Час
            else
            {
                if (hoursperiod != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Пост 1 раз опубликован, увеличиваем дату на {hoursperiod} час");
                    publication_date = publication_date.AddHours(hoursperiod);
                }
                if (minutesperiod != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Пост 1 раз опубликован, увеличиваем дату на {minutesperiod} минут");
                    publication_date = publication_date.AddMinutes(minutesperiod);
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Будет опубликован: {publication_date}  {cleared}й пост");
            }

            {
                try
                {

                    long post = api.Wall.Post(new WallPostParams
                    {
                        Attachments = attachments,
                        OwnerId = ownid,
                        Message = MessageToAttach,
                        PublishDate = publication_date,
                    });

                    LastDatePosted = publication_date;

                    DBUtils.UpdatePublicationDateOfTracks(LstBox_AddedTracks, fmtPlaylist, LastDatePosted);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Пост опубликован! {MessageToAttach}");
                    //Rabbit.NewLog($"Пост опубликован! {MessageToAttach}");

                    ClearAll();
                    posted = true;

                }

                catch (TooManyRequestsException vk_req)
                {
                    Logging.ErrorLogging(vk_req);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Превышено число запрсов в секунду к ВК, ждем 5 сек...");
                    Thread.Sleep(5000);
                    changed_time = true;
                }

                catch (VkNet.Exception.PostLimitException vk_lim)
                {
                    Logging.ErrorLogging(vk_lim);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Лимит запланированных записей! Добавили в очередь публикаций!");

                    //Добавление в БД
                    try
                    {
                        DBUtils.AddPostInDB(attachments, ownid, MessageToAttach, publication_date);
                    }
                    catch (Exception dbEX) { Console.WriteLine(dbEX.Message); Logging.ErrorLogging(dbEX); }

                    wallPostQueue.Enqueue(new WallPostParams
                    {
                        Attachments = attachments,
                        OwnerId = ownid,
                        Message = MessageToAttach,
                        PublishDate = publication_date
                    });


                    LastDatePosted = publication_date;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Пост Добавлен в очередь!");
                    Console.WriteLine($"Число постов в очереди: {wallPostQueue.Count}");
                    Console.WriteLine("Текущее время: {0}, {1:G}", localDate.ToString(culture), localDate.Kind);

                    ClearAll();
                    posted = true;
                }

                catch (VkNet.Exception.UnknownException unknown)
                {
                    Logging.ErrorLogging(unknown);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка, на это время уже отложена запись, увеличиваем время на {5} минут. Или ошибка публикации");

                    publication_date = publication_date.AddMinutes(5);
                    changed_time = true;
                }
                catch (Exception ex) //TODO подумать что делать в случае любой ошибки
                {
                    Logging.ErrorLogging(ex);
                    Logging.ReadError();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка {ex}");

                    publication_date = publication_date.AddMinutes(5);

                    //SaveDate
                    LastDatePosted = publication_date;
                    Console.WriteLine("Текущее время: {0}, {1:G}", localDate.ToString(culture), localDate.Kind);

                    ClearAll();
                    changed_time = true;
                }

                finally
                {
                    //если ошибка ограничение 25 постов в день - изменить день
                    if (changed_time == true)
                    {
                        _ = api.Wall.Post(new WallPostParams
                        {
                            Attachments = attachments,
                            OwnerId = ownid,
                            Message = MessageToAttach,
                            PublishDate = publication_date
                        });

                        LastDatePosted = publication_date;
                        DBUtils.UpdatePublicationDateOfTracks(LstBox_AddedTracks, fmtPlaylist, LastDatePosted);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Пост, увеличила дату {LastDatePosted}, опубликован! {MessageToAttach}");
                        //Rabbit.NewLog($"Пост, увеличила дату {LastDatePosted}, опубликован! {MessageToAttach}");

                        ClearAll();
                        posted = true;
                    }
                }

            }

        }

        #endregion

        /// <summary>
        /// Clearing vars after post or fail
        /// </summary>
        private void ClearAll()
        {
            threadstopflag = false;
            postMessage = null;

            tracksfound = false;
            tracks_attached = false;

            //Чистим массив списка поиска
            for (int i = 0; i < SearchingList.Count; i++)
            {
                SearchingList.RemoveAt(i);
            }

            ChartList.Clear();
            SearchingList.Clear();

            for (int i = 0; i < LstBox_AddedTracks.Count; i++)
            {
                LstBox_AddedTracks.RemoveAt(i);
            }

            for (int i = 0; i < SelectedTrackList.Count; i++)
            {
                SelectedTrackList.RemoveAt(i);
            }

            SelectedTrackList.Clear();
            LstBox_AddedTracks.Clear();

            //Надо очистить attachments
            for (int i = 0; i < attachments.Count; i++)
            {
                attachments.RemoveAt(i);
            }

            attachments.Clear();

            //Сброс поста после публикации
            posted = false;

            parser_finished = false;
            cleared++;
        }


        /// <summary>
        /// Test
        /// </summary>
        public static void SendTestTrackMessages()
        {
            //for (int i = 0; i < 50; i++)
            //{
            while (true)
            {                
                //Rabbit.NewPostedTrack(Rabbit.RandomString(10), Rabbit.RandomString(5), DateTime.Now);

                Thread.Sleep(2000);
            }
            //}
        }
    }
}
