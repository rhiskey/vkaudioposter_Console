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
using static vkaudioposter_Console.Classes.Music;
using vkaudioposter_Console.API;
using vkaudioposter_Console.Classes;

namespace vkaudioposter_Console
{
    class Program
    {
        //TODO: read from .env
        public static string DB_HOST; //local
        public static string DB_NAME;
        public static string DB_USER;
        public static string DB_PASS;
        private List<Track> SearchingList = new List<Track>(); //Список найденных треков
        //string debug = "Список треков для поста";
        public List<string> SelectedGenre = new List<string>();
        private List<Track> SelectedTrackList = new List<Track>();
        public static List<Chart> ChartList = new List<Chart>();

        //ПолучаетсяВ приложении своем
        //Приложение группы доступ к группе, получается в самом приложении в ручную Standalone приложение		
        public static string accesstoken; //добавить фото и музыку вложения на стену
        //string accesskeypub = "";
        //логи в личку
        public static string kateMobileToken;
        private string MessageToAttach;
        //Авторизация для публикаций https://vkhost.github.io - сообщество + приложение
        //Получается https://oauth.vk.com/authorize?client_id=7361627&scope=notify,photos,friends,audio,video,notes,pages,docs,status,questions,offers,wall,groups,notifications,stats,ads,offline&redirect_uri=http://api.vk.com/blank.html&display=page&response_type=token                                                                                                               
        public static string Token; //kateMobileToken; //Загрузка фото + публикация треков (сделать через свое приложение)
        //Для поиска
        //private static string filepath = "response.dll"; // файл с ответом запроса
        private static readonly string url1 = "https://api-vk.com/api.php?method=search&q=";
        //API.Я.WS сделать пункт в настройках
        //private static readonly string url3 = "&key=eba768b60d6ce9dc418e8adef62605e3"; //apiяws
        private static string url3; //new Api https://api-vk.com
        public static ulong? ownerid3; //для сохранения фото на стене
        public static ulong? groupid3;//для  сохранения фото на стене
        public static long groupid = (long)31640582; //для загрузки на сервер

        private static DateTime publication_date; //для wall.post
        //private static string filepath_archive = "posted_tracks.dll";

        public static long ownid; //может быть косяк, нужно сразу наверное owni
        //private static readonly long number_of_strok = 2000000000; //миллион символов

        private bool posted = false; //опубликован?
        private bool tracks_attached = false;
        private bool tracksfound = false;
        public static int hoursperiod = 2; //период отложенных записей
        public static int minutesperiod = 20; //период отложенных записей в минутах
        private string switcher = null;
        public static bool parser_finished = false;
        private int postcounter = 1; //номер скачанной фотки в стоке
        private List<MediaAttachment> attachments = new List<MediaAttachment>();

        private string photourl = null; //Прямая ссылка на фото
        private static string photofilename = "tempimage.jpg"; //скачанное фото

        //public static string stylefilename = "style.dll";

        private static readonly int trackscount = 100; //100
        public static string HashTags = "#HighVolumeMusic #HVM @hvmlabel #edm #music #электронная #музыка #танцевальная #новинкимузыки #свежая_музыка";
        private int cleared = 0;

        //Genres Sync
        //private static string genres_file = "playlist.json";

        public List<string> PhotoStock = new List<string>();

        //private static readonly string JSONphotostock = "photostock.json";

        List<TrackToDB> trackToDB_list = new List<TrackToDB>();

        //For COnsole
        private List<string> CB_PhotoStock = new List<string>();
        private string postMessage = null; //richTextBox1

        //Spotify
        public static string clientId;
        public static string clientSecret;

        //private List<string> LSTBOX_Genres = new List<string>();
        private List<string> listBox1 = new List<string>();
        private List<string> LstBox_AddedTracks = new List<string>();
        private static DateTime LastDatePosted = new DateTime();
        Queue<Photostock_class> photostockQueue = new Queue<Photostock_class>();

        Queue<WallPostParams> wallPostQueue = new Queue<WallPostParams>();

        private static string userAccessToken; //Скачать музыку если не apiяws
        private static bool apiWS = true; //Использовать при поиске APIЯWS или VK API (бесплатно)

        private const int max_photo = 10; //Макс фоток в контейнере (для девиан = 24) Defalt:10

        private const int stockMaxPages = 5; //Макс число страниц для парсинга 


        public static long adminID; //для отправки ЛС
        private static readonly int searchCoolDown = 700; //ms для поиска по apiяws
        //// Notifications
        //public static string pusherAppId;
        //public static string pusherAppKey;
        //public static string pusherAppSecret;

        private static bool autoStart; //OLD method with timer
        private static bool startOnce; //For Sheduler

        private static Random random = new Random();
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

        private static List<FormattedPlaylist> allPlaylists;

        private static void LoadConfigsFromEnv()
        {
            //LoadEnv:
            // Config
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

            autoStart = DotNetEnv.Env.GetBool("AUTO_START");
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

        private static void Main(string[] args)
        {
            //Thread.Sleep(2000);
            Thread rabbitReciever = new Thread(new ThreadStart(Rabbit.CommandsReciever))
            {
                IsBackground = false
            };

            // Start the thread.
            rabbitReciever.Start();

            //SendTestTrackMessages();

            try
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = false;
                }).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
            }


            LoadConfigsFromEnv();
            // Create Database with schema 
            vkaudioposter_ef.Program.LoadConfig();
            vkaudioposter_ef.Program.CreateStoredProceduresViewsAndFunctions(false); //recreate onChange

            if (autoStart == true)
            {
                Console.WriteLine($"{DateTime.Now:g} Creating timer\n CTRL+C to stop");
                //For Scheduler
                int hours1 = 72;
                long koeffmillisec = 3600000;
                Int64 period = hours1 * koeffmillisec;
                Int64 period2 = 2 * period;

                // Create an AutoResetEvent to signal the timeout threshold in the
                // timer callback has been reached.
                var autoEvent = new AutoResetEvent(false);

                //Число запусков потока
                var statusChecker = new StatusChecker(999999999);

                var stateTimer = new Timer(statusChecker.CheckStatus,
                                           autoEvent, 1000, period); //Sheduler 
                autoEvent.WaitOne();
                stateTimer.Change(0, period2); //86400000 = one day 24h
                Console.WriteLine("\nChanging period to 24 hours.\n");
            }

            if (startOnce == true)
                StatusChecker.ApiStart();

        }


        public static void SendTestTrackMessages()
        {
            //for (int i = 0; i < 50; i++)
            //{
            while (true)
            {
                //Random rand = new Random(DateTime.Now.Second);
                Rabbit.NewPostedTrack(RandomString(10), RandomString(5), DateTime.Now);
                //Rabbit.NewPostedTrack("TRFN", "BASS", DateTime.Now);
                Thread.Sleep(2000);
            }
            //}
        }

        public Thread Automatica(List<FormattedPlaylist> playlists)
        {
            try
            {
                Rabbit.NewLog("Start Parser");

                //automatica = true;
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
                    Rabbit.NewLog($"Отложенных {wallTotal} постов");

                    // Остановка
                    if (threadstopflag == true || wallTotal == 150)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");
                        Logging.ErrorLogging($"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");
                        Rabbit.NewLog($"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");
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

                    //--------------------------------------DEBUG-------------------------
                    ////ПОДУМАТЬ
                    //if (offsetchanged == true)
                    //{
                    //    postcounter += (int)NUD_PhotoCount.Value;
                    //}
                    //--------------------------------------DEBUG-------------------------
                    Rabbit.NewLog($"Search Tracks in VK: {style.PlaylistName}");
                    SearchTracksVk(style);
                    do
                    {
                        Thread.Sleep(100);
                    }
                    while (tracksfound == false); //

                    if (SearchingList.Count > threshhold)
                    {
                        //if (changeStock == true) //Если сработала ошибка скачки фото
                        if (postcounter >= max_photo) //Если число постов больше макс числа фото, сбрасываем счетчик, удаляем из очереди фотосток, добавляем в конец его же
                        {
                            postcounter = 0; //Сброс
                                             //++photostock_count; //сместили фотосток

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

                            Rabbit.NewLog("Photo Parser started");
                            photourl = PhotoParserAuto(photostock_new, postcounter, style.PlaylistName, stockPage);

                            // photourl = DeviaPhotoParsenParser(postcounter, multiplier); //прямая ссылка на фото, генерировать с devianart автоматически (вызывать одтельную функцию
                            //try
                            //{
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
                            //postcounter++;

                            postcounter = 0; //Сброс
                                             //++photostock_count; //сместили фотосток

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
                            Rabbit.NewLog("Download Photo");
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

                        for (int j = 0; j < listBox1.Count; j++)
                        {
                            listBox1.RemoveAt(j);
                        }
                        listBox1.Clear();
                        Console.WriteLine("Размещаем пост на стену");
                        Rabbit.NewLog("Размещаем пост на стену");
                        //ErrorLogging(String.Format("Размещаем пост на стену: сток {0}, стиль: {1}", photostock_new, style));

                        if (photo_exist == true) //если вообще скачалась фотка
                        {
                            var attsTuple = VkTools.AddPhotoToAttachFromUrl(photofilename, attachments, postMessage, LstBox_AddedTracks);
                            attachments = attsTuple;
                            //flag = attsTuple.Item2;
                        }

                        PosterOnWall(attachments, style);
                        do
                        {
                            Thread.Sleep(100);

                        } while (posted == false);
                        ind++;

                        //postcounter++; //меняем фотку

                        //if (posted == true)

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
                Rabbit.NewLog("Процесс завершен!");
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

        private void OnLoad()
        {
            LoadConfigsFromEnv();
            //Добавляем названия жанров
            allPlaylists = DBUtils.GetAllPlaylists();

            //foreach (var pl in allPlaylists)
            //{
            //    LSTBOX_Genres.Add(pl.PlaylistName);
            //}

            allPlaylists = Tools.Shufflers.ShuffleList(allPlaylists);

            //LSTBOX_Genres = Tools.Shufflers.ShufflePlaylist(LSTBOX_Genres);

            //Загружает в JSONphotostock
            CB_PhotoStock = DBUtils.LoadPhotoStocksFromDB();

            //Shuffle Photostock List
            CB_PhotoStock = Tools.Shufflers.ShuffleList(CB_PhotoStock);

            foreach (var item in CB_PhotoStock)
            {
                //Добавили в очередь
                photostockQueue.Enqueue(new Photostock_class()
                {
                    URL = item.ToString()
                });
            }

            //Console.WriteLine("Перемешанный список фотостоков:\n");
            foreach (var stock in photostockQueue)
            {
                Console.WriteLine(stock.URL);
            }
        }

        // Parsers
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

                    string fields = "items(added_by.id,track(name,artists))";
                    int limit = 100; //default
                    int offset = 0; //смещение = 0
                    string market = "US"; //Сделать RU
                    SpotifyTools.SpotyParser(playlistId, fields, limit, offset, market, spotify);
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


                    HtmlWeb web = new HtmlWeb();
                    HtmlAgilityPack.HtmlDocument doc = web.Load(Url);

                    //F12 - Elements -> выделяем текст, посмотреть код элемента, затем в Elements ПКМ - Copy - Xpath и отделяем \"
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

                        Program.ChartList.Add(new Chart(trackname, remixer, author));

                        //FileStream file2 = new FileStream(Program.filechartname, FileMode.Append);
                        //StreamWriter writer2 = new StreamWriter(file2, Encoding.UTF8); //создаем «потоковый писатель» и связываем его с файловым потоком
                        //writer2.WriteLine(trackname + " " + author + " " + remixer); //записываем в файл (раньше было json)
                        //writer2.Close();
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

        // TODO: перенести в отдельный класс
        //Создает из файла чарта Коллекцию вложенных аудио
        public void MakeUrlFromLines(List<Chart> tracksToFind, FormattedPlaylist styletoDB)
        {
            VkApi api = new VkApi();

            if (apiWS == false)
                try
                {
                    var services = new ServiceCollection();
                    services.AddAudioBypass();
                    api = new VkApi(services);
                    //Авторизация
                    api.Authorize(new ApiAuthParams
                    {
                        AccessToken = userAccessToken
                    });
                }
                catch (Exception ex)
                {
                    //TODO
                    Logging.ErrorLogging(ex);
                }

            string url2;
            int existcounter = 0;
            //string[] lines = null;
            //lines = File.ReadAllLines(filename); //список треков
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

            // список хранить в памяти а не в файле

            //для каждой строки с названием
            foreach (var trackobj in tracksToFind)
            {
                string K = trackobj.GetTrackAndAuthors();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Текущий трек: {K}");


                //Что делать, если в базе нет такого жанра?!
                //trackname = trackname.Replace("\'", "");

                //Обращаться к БД каждый раз как пытаемся новый K трек найти, Если в БД нет такого - то ищем
                var postedTracks = DBUtils.GetPostedTracksFromDB(styletoDB);
                var unfoundTracks = DBUtils.GetUnfoundTracksFromDB(styletoDB);


                //Output = Output.Replace("/", "");
                //Преобразовать строки в файл и затем работать
                //styletoDB = styletoDB.Replace("/", "");

                //filepath_archive = styletoDB + ".dll";
                //FileStream file1 = new FileStream(filepath_archive, FileMode.Create); //открытие файла на дозапись в конец файла
                //StreamWriter writer = new StreamWriter(file1, Encoding.UTF8); //создаем «потоковый писатель» и связываем его с файловым потоком
                //writer.Write(Output); //записываем в файл 
                //writer.Close();

                //file1.Close();


                //string s = File.ReadAllText(filepath_archive);

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


                //ЧТО-то НЕ ТО, не чекает иногда
                if ((fmtPostedTracks.IndexOf(current_track) != -1) /*&& (Output.Length != 0))*/ || (fmtUnfoundTracks.IndexOf(current_track) != -1)) //пока не дошли до конца ищем первое совпадение и если архив не пустой    
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
                        //Console.WriteLine("Запрос трек: {0}", current_track);

                        //---------------Old-------------------------
                        string json = JsonTools.SearchWeb(url);

                        if (json != null)
                        {
                            //FileWorkers.FileWrite(json, filepath); //записали в файл, очистили после выполнения
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
                                    catch (Exception ex2) { Console.ForegroundColor = ConsoleColor.DarkRed; Console.WriteLine("Dublicate in UnfoundTracks...skip"); Console.WriteLine($"{ ex2.StackTrace}"); continue; }

                                    continue;
                                }
                                finally
                                {
                                    Rabbit.NewPostedTrack(current_track, styletoDB.PlaylistName, publication_date);
                                }

                                SearchingList.Add(new Track(url2, FullId));

                                //Добавить треки в Quue очередь или класс при публикации заливать, очищать при нажатии отмена
                                listBox1 = JsonTools.AddListItemMethod(existcounter, SearchingList, listBox1);
                                existcounter++;
                            }
                            else
                            {
                                unsearchtracks++;
                                try
                                {
                                    DBUtils.InsertUnfoundTrackInDB(current_track, styletoDB, false);
                                }
                                catch (Exception e) { Console.ForegroundColor = ConsoleColor.DarkGray; Console.WriteLine("Dublicate in UnfoundTracks...skip"); Logging.ErrorLogging(e); };
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
                                //string FullId = null;

                                foreach (var audio in audios)
                                {
                                    string allArtists = null;
                                    //string fullTrackName = null;

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
                            listBox1 = JsonTools.AddListItemMethod(existcounter, SearchingList, listBox1);
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

            }


            tracksfound = true;
            return;
        }

        private void ClearAll()
        {
            threadstopflag = false;
            postMessage = null;

            //File.WriteAllText(stylefilename, string.Empty); //очистили файл
            //File.WriteAllText(filechartname, string.Empty); //очистили файл

            tracksfound = false;

            //downloaded = false; //фотка не скачана - скачать заново тип

            //Добавляем трек в историю
            for (int i = 0; i < listBox1.Count; i++)
            {
                listBox1.RemoveAt(i);
            }

            listBox1.Clear();
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

        //Пост на стену
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

            //// Check postponed
            //VkNet.Enums.SafetyEnums.WallFilter wallFilterPostponed = VkNet.Enums.SafetyEnums.WallFilter.Postponed;
            //var postponedCount = api.Wall.Get(new WallGetParams
            //{
            //    Filter = wallFilterPostponed,
            //    OwnerId = ownid,
            //    //Offset
            //});
            //var wallTotal = postponedCount.TotalCount;
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine($"Отложенных {wallTotal} постов");


            //Отложенная запись добавить
            //Текущие дата/время хранятся в свойстве Now класса DateTime
            //flag показывает заполнен ли список вложений, 

            //нужна проверка на дату публикации, чтобы она прибавлялась на 5 минут вперед если отложка
            bool changed_time = false;

            //The following method call displays 1 / 1 / 0001 12:00:00 AM.
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
                //Дата публикации в прошлом
                System.Console.WriteLine("{0:d} is in the past.", publication_date);
                //Ставим её на сейчас и добавляем N минут
                publication_date = DateTime.Now;
                publication_date = publication_date.AddMinutes(minutesperiod);
            }
            else if (compareValue == 0)
            {
                //Дата публикации совпадает с текущим временем
                System.Console.WriteLine("{0:d} is today!", publication_date);
                publication_date = publication_date.AddMinutes(minutesperiod);
            }
            else // compareValue > 0
            {
                //Дата публикации в будущем
                System.Console.WriteLine("{0:d} has not come yet.", publication_date);
            }

            //если до этого не был опубликован
            if (cleared == 0)
            {
                ;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Будет опубликован: {publication_date}");

            }
            //Если уже 1 раз опубликовали, publication_date.AddHours(1)
            //увеличили на Час
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
            //Убрать flag
            //if (
            //    ((MessageToAttach != null) && (flag == 2)) ||
            //    ((flag == 2) && (MessageToAttach == null)) ||
            //   ((flag == 0) && (MessageToAttach != null))
            //    )
            {
                try
                {
                    //PostParams pp = Poster.Peek();
                    //Poster.Dequeue();

                    long post = api.Wall.Post(new WallPostParams
                    {
                        Attachments = attachments,
                        // Attachments = pp.Atts,
                        // OwnerId=pp.Owner_Id,
                        OwnerId = ownid,
                        //FromGroup = 1,
                        Message = MessageToAttach,
                        //Message = pp.Message_To_Attach,
                        PublishDate = publication_date,
                        //CloseComments = true
                        //Services=
                        //PublishDate = pp.Publication_Date
                    });

                    
                    //do { Thread.Sleep(100); }while()

                    //// TODO: Pass style in memory instead file? 
                    //var styleTuple = FileWorkers.GetStylec(stylefilename, postMessage);
                    ////GetStylec();
                    //style = styleTuple.Item1;

                    ////Check public playlists ERROR: Unknown method passed
                    //var playlistsPublic = api.Audio.GetPlaylists(ownid);
                    //foreach (var pl in playlistsPublic)
                    //{
                    //    var playlist = pl;
                    //    //TODO
                    //    if(pl.Title != style)
                    //    {
                    //        //Create new playlist + append
                    //        playlist = api.Audio.CreatePlaylist(ownid, style);
                    //    }

                    //    List<string> trackIds = new List<string>();
                    //    foreach (var track in attachments)
                    //    {
                    //        string format = track.OwnerId + "_" + track.Id;
                    //        trackIds.Add(format);
                    //    }
                    //    // Append all tracks to playlist
                    //    api.Audio.AddToPlaylist((long)playlist.OwnerId, (long)playlist.Id, trackIds);
                    //    Thread.Sleep(700);
                    //}

                    //SaveDate
                    LastDatePosted = publication_date;

                    DBUtils.UpdatePublicationDateOfTracks(LstBox_AddedTracks, fmtPlaylist, LastDatePosted);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Пост опубликован! {MessageToAttach}");
                    Rabbit.NewLog($"Пост опубликован! {MessageToAttach}");

                    

                    ClearAll();
                    posted = true;

                }
                //catch (Exception ex)
                //{

                //    ErrorLogging(ex);
                //    ReadError();

                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Console.WriteLine(string.Format("Ошибка, на это время уже отложена запись, увеличиваем время на {0} минут. Или ошибка публикации", 5));
                //    publication_date = publication_date.AddMinutes(5);

                //    changed_time = true;

                //}
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

                    //SaveDate
                    LastDatePosted = publication_date;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Пост Добавлен в очередь!");
                    Console.WriteLine($"Число постов в очереди: {wallPostQueue.Count}");
                    Console.WriteLine("Текущее время: {0}, {1:G}", localDate.ToString(culture), localDate.Kind);
                    //ErrorLogging(String.Format("Пост добавлен в очередь. Число постов в очереди: {0}", wallPostQueue.Count));
                    ////??????
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
                            // Attachments = pp.Atts,
                            // OwnerId=pp.Owner_Id,
                            OwnerId = ownid,
                            //FromGroup = 1,
                            Message = MessageToAttach,
                            //Message = pp.Message_To_Attach,
                            PublishDate = publication_date
                            //PublishDate = pp.Publication_Date
                        });

                        //SaveDate
                        LastDatePosted = publication_date;
                        DBUtils.UpdatePublicationDateOfTracks(LstBox_AddedTracks, fmtPlaylist, LastDatePosted);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Пост, увеличила дату {LastDatePosted}, опубликован! {MessageToAttach}");
                        Rabbit.NewLog($"Пост, увеличила дату {LastDatePosted}, опубликован! {MessageToAttach}");

                        //Console.WriteLine("Пост, увеличили дату, опубликован!");
                        ClearAll();
                        posted = true;

                        //changed_time = false;
                    }
                }

            }

        }

        private Task SearchTracksVk(FormattedPlaylist styletothread)
        {

            attachments.Clear();
            //Poster.Dequeue();
            SearchingList.Clear();
            listBox1.Clear();
            LstBox_AddedTracks.Clear();
            //butflag = 1;
            posted = false;

            //MakeUrl From Lines
            try
            {
                trackToDB_list.Clear();

                //Передать вместо файла переменную
                MakeUrlFromLines(ChartList, styletothread);

            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\nSearch canceled.\r\n");
            }


            return null;
        }

        public static Thread StartTheParserThread(int trackscount, vkaudioposter_ef.parser.Playlist style, string playlistId, string trackstop)
        {
            Parser(trackscount, style, playlistId, trackstop);
            return null;
        }

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
                        nodContainer = "//*[@id=\"root\"]/div[1]/div/div/article/div/div/div[2]";//Контейнер с картинками на странице (последний grid)
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
                            url = doc.DocumentNode.SelectNodes("/html/body/div[5]/div/div[" + i + "]/a/picture/img")[0].Attributes[2].Value;
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
                                    //MessageBox.Show(ex.Message);
                                    Logging.ErrorLogging(ex1);
                                    Logging.ReadError();
                                }
                            }
                        }

                        break;
                    case "https://picjumbo.com/":
                        try
                        {
                            url = doc.DocumentNode.SelectNodes("/html/body/div[6]/div/div[" + i + "]/a[1]/picture/img")[0].Attributes[2].Value;
                        }
                        catch (System.NullReferenceException ex)
                        {
                            Logging.ErrorLogging(ex);
                            if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
                            {
                                pickedStockPage++;
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
                            if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
                            {
                                //Если нет фотки
                                pickedStockPage++; //Перешли на след. страницу
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
                            if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
                            {
                                //Если нет фотки
                                pickedStockPage++; //Перешли на след. страницу
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
                        nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[3]/div[2]";//Контейнер с картинками на странице
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
                        // Do nothing for a null.
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
                    nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[2]/div[2]";//Контейнер с картинками на странице 
                    url = PhotoParser.DevianPageParser(doc, nodContainer, i);

                }

                if ((photostock == "https://www.deviantart.com/search/deviations?order=most-recent&q=wallpaper") ||
                    (photostock == "https://www.deviantart.com/?order=whats-hot") ||
                    (photostock == "https://www.deviantart.com/search/deviations?q=wallpaper&order=whats-hot"))
                {
                    //24 фото на странице
                    nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[2]/div[2]/div";//Контейнер с картинками на странице
                    try
                    {
                        url = PhotoParser.DevianPageParser(doc, nodContainer, i);
                    }
                    catch (System.NullReferenceException ex)
                    {
                        Logging.ErrorLogging(ex);
                        if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
                        {
                            pickedStockPage++; //Перешли на след. страницу
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

                }


                if ((photostock == "https://www.deviantart.com/luckynumber113/favourites/76516524/Pretty-Wallpapers-Backgrounds-and-Designs") ||
                    (photostock == "https://www.deviantart.com/james-is-james/favourites/60347448/Abstract") ||
                    (photostock == "https://www.deviantart.com/psychospartanex/favourites/77875659/Amazing-Wallpapers"))
                {

                    nodContainer = "//*[@id=sub-folder-gallery]/div/div[2]/div/div/div[2]/div";//Контейнер с картинками на странице (почти крайний блок)
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

        //Timer
        public class StatusChecker
        {
            private int invokeCount;
            private int maxCount;

            public StatusChecker(int count)
            {
                invokeCount = 0;
                maxCount = count;
            }

            // This method is called by the timer delegate.
            public void CheckStatus(Object stateInfo)
            {
                AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
                Console.WriteLine($"{DateTime.Now:h:mm:ss.fff} Checking status {++invokeCount}.");

                if (invokeCount == maxCount)
                {
                    // Reset the counter and signal the waiting thread.
                    invokeCount = 0;
                    autoEvent.Set();
                }


                Program P = new Program();

                P.OnLoad();

                // publication_date, LastDatePosted
                var dateTuple = DBUtils.GetLastDateFromDB(); //saving to datetimepicker
                publication_date = dateTuple.Item1;
                LastDatePosted = dateTuple.Item2;

                P.cleared = 0;

                if (P.wallPostQueue.Count != 0) //Если в очереди есть посты
                {
                    VkApi api = new VkApi();
                    //Авторизация
                    api.Authorize(new ApiAuthParams
                    {
                        AccessToken = Token
                    });
                    bool publError = false;

                    // Для каждого поста из БД
                    foreach (var postQ in P.wallPostQueue) //Для каждлого поста в очереди
                    {
                        //var postParams = P.wallPostQueue.Dequeue();                    
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
                    //var shuffledPlaylist = ShufflePlaylist(P.LSTBOX_Genres);
                    P.Automatica(allPlaylists);
                }

            }


            // This method is called by the timer delegate.
            public static void ApiStart()
            {
                Program P = new Program();

                P.OnLoad();

                var dateTuple = DBUtils.GetLastDateFromDB(); //saving to datetimepicker
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

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
