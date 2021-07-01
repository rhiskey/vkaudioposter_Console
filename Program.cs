using HtmlAgilityPack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vkaudioposter;
using vkaudioposter.Classes;
using vkaudioposter.MySQL;
using vkaudioposter_Console.Classes;
using vkaudioposter_Console.Parsers;
using vkaudioposter_Console.Tools;
using vkaudioposter_Console.VKUtills;
using vkaudioposter_ef.Model;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using HubConnection = Microsoft.AspNetCore.SignalR.Client.HubConnection;
using Track = vkaudioposter.Track;


namespace vkaudioposter_Console
{
    class Program
    {
        #region StaticVars
        private static bool firstRun; //create DB, initial seed
        public static bool useProxy; //To download images
        public static bool debug; //logs from EF
        //ПолучаетсяВ приложении своем
        //Приложение группы доступ к группе, получается в самом приложении в ручную Standalone приложение		

        //логи в личку
        public static string kateMobileToken;
        public static string rollbarToken;

        public static long adminID; //для отправки ЛС
        public static string torHost;
        public static int torPort;

        public static bool threadstopflag = false;
        public static bool saveLogs = true;

        //Авторизация для публикаций https://vkhost.github.io - сообщество + приложение
        //Получается https://oauth.vk.com/authorize?client_id=7361627&scope=notify,photos,friends,audio,video,notes,pages,docs,status,questions,offers,wall,groups,notifications,stats,ads,offline&redirect_uri=http://api.vk.com/blank.html&display=page&response_type=token                                                                                                               
        public static string Token; //kateMobileToken; //Загрузка фото + публикация треков (сделать через свое приложение)
        public static string accesstoken; //add tracks to attachments =добавить фото и музыку вложения на стену. Приложение группы доступ к группе, получается в самом приложении в ручную Standalone приложение	
        public static ulong? ownerid3; //для сохранения фото на стене
        public static ulong? groupid3;//для  сохранения фото на стене
        public static long groupid; //для загрузки на сервер

        //public static string pusherAppSecret;
        public static long ownid;
        public static int hoursperiod = 2; //период отложенных записей
        public static int minutesperiod = 20; //период отложенных записей в минутах
        public static bool parser_finished = false;

        public static string HashTags = "#HighVolumeMusic #HVM @hvmlabel #edm #music #электронная #музыка #танцевальная #новинкимузыки #свежая_музыка";
        //Spotify
        public static string clientId;
        public static string clientSecret;
        public static Random random = new();
        public static string signalrConsoleHub;

        private static readonly int searchCoolDown = 700; //ms для поиска по apiяws

        private static bool startOnce; //For Sheduler

        private static readonly string url1 = "https://api-vk.com/api.php?method=search&q=";
        private static string url3; //new Api https://api-vk.com

        private static DateTime publication_date; //для wall.post
        private static string photofilename = "tempimage.jpg"; //скачанное фото
        private static readonly int trackscount = 100; //100
        private static DateTime LastDatePosted;
        private static string userAccessToken; //Скачать музыку если не apiяws
        private static bool apiWS = true; //Использовать при поиске APIЯWS или VK API (бесплатно)

        private static List<FormattedPlaylist> allPlaylists;
        public static List<SpotyTrack> ChartList = new();

        private static string VKLogin, VKPass;
        #endregion

        public HubConnection connection;

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

        public List<string> SelectedGenre = new();
        public List<string> PhotoStock = new();

        private List<SpotyVKTrack> SearchingList = new(); //Список найденных треков

        private List<Track> SelectedTrackList = new();
        private List<MediaAttachment> attachments = new();
        //For COnsole
        private List<string> CB_PhotoStock = new();

        private List<string> LstBox_AddedTracks = new();

        Queue<Photostock_class> photostockQueue = new();
        Queue<WallPostParams> wallPostQueue = new();



        #region Config
        /// <summary>
        /// Loads config from env or DB
        /// </summary>
        private static void LoadConfigsFromEnv()
        {
            //LoadConfig from db
            var cfg = DBUtils.GetConfigFromDb();
            string searchApiUrlPrefix = "&key=";
            string searchApiUrlPostfix = "&v=3";

            debug = cfg.Debug;
            saveLogs = cfg.SaveLogs;
            firstRun = cfg.FirstRun;
            useProxy = cfg.UseProxy;
            startOnce = cfg.StartOnce;

            hoursperiod = cfg.HoursPeriod;
            minutesperiod = cfg.MinutesPeriod;

            clientId = cfg.SpotifyClientId;
            clientSecret = cfg.SpotifyClientSecret;

            accesstoken = cfg.AccessToken;
            kateMobileToken = cfg.KateMobileToken;
            Token = cfg.Token;
            userAccessToken = cfg.UserAccesToken;
            adminID = cfg.AdminId;

            rollbarToken = cfg.RollbarToken;

            url3 = searchApiUrlPrefix + cfg.ApiUrl + searchApiUrlPostfix;

            var env_ownerid3 = cfg.OwnerId;
            var env_groupid3 = cfg.GroupId;
            ownerid3 = Convert.ToUInt64(env_ownerid3);
            groupid3 = Convert.ToUInt64(env_groupid3);

            torHost = cfg.TorHost;
            torPort = cfg.TorPort;
            rollbarToken = cfg.RollbarToken;
            groupid = (long)groupid3;
            ownid = -(long)groupid3;

            VKLogin = cfg.VKLogin;
            VKPass = cfg.VKPassword;
            signalrConsoleHub = DotNetEnv.Env.GetString("CONSOLE_HUB");
        }

        private async void OnLoad()
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

            connection = new HubConnectionBuilder()
            .WithUrl(
            new Uri(signalrConsoleHub), options =>
            {

                var handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                };
                options.HttpMessageHandlerFactory = _ => handler;
                options.WebSocketConfiguration = sockets =>
                {
                    sockets.RemoteCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
                };
            })
            .WithAutomaticReconnect()
            .Build();
            try
            {
                await connection.StartAsync();
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        #endregion

        private static void Main(string[] args)
        {
            //CircularDoublyLinkedList<string> circularList = new CircularDoublyLinkedList<string>();
            //circularList.Add("Tom");
            //circularList.Add("Bob");
            //circularList.Add("Alice");
            //circularList.Add("Sam");

            //foreach (var item in circularList)
            //{
            //    Console.WriteLine(item);
            //}

            //circularList.Remove("Bob");
            //Console.WriteLine("\n После удаления: \n");
            //foreach (var item in circularList)
            //{
            //    Console.WriteLine(item);
            //}

            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = false;
            }).Start();


            LoadConfigsFromEnv();

            // Create Database with schema 
            //vkaudioposter_ef.Program.InsertRoles();
            if (firstRun == true)
            {
                vkaudioposter_ef.Program.LoadConfig();

                ///If want to delete -> pass TRUE             
                vkaudioposter_ef.CreateInitialSchema.CreateSchema(true); //TODO:recreate
                vkaudioposter_ef.Program.InsertData(true);
                //vkaudioposter_ef.Program.CreateStoredProceduresViewsAndFunctions(true);
            }
            if (startOnce == true)
                StatusChecker.ApiStart();

        }

        #region Appcycle
        /// <summary>
        /// Autostart or api
        /// </summary>
        public class StatusChecker
        {
            public static void ApiStart()
            {
                Program P = new();

                P.OnLoad();


                P.connection.InvokeAsync("SendMessage",
                  "Console", $"Started...");

                DBUtils.CountPublishedTracksInStyles();

                var dateTuple = DBUtils.GetLastDateFromDB(true);

                publication_date = dateTuple.Item1;
                LastDatePosted = dateTuple.Item2;

                P.connection.InvokeAsync("SendMessage",
                  "Console", $"Last date: {LastDatePosted}");

                P.cleared = 0;

                if (P.wallPostQueue.Count != 0)
                {
                    VkApi api = new();

                    api.Authorize(new ApiAuthParams
                    {
                        AccessToken = Token
                    });
                    bool publError = false;

                    foreach (var postQ in P.wallPostQueue)
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
                                P.wallPostQueue.Dequeue();
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

                string trackstop = "tracks";
                if (switcher == "fresh")
                { trackstop = "tracks"; }
                if (switcher == "top100")
                { trackstop = "top-100"; }

                postcounter = 1;
                int ind = 0;

                int threshhold = 0; //порог количества треков для публикации
                string photostock_new = null;


                foreach (var style in playlists.ToList())
                {

                    var wallTotal = VkTools.CheckPostponedAndGetCount();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Отложенных {wallTotal} постов");

                    connection.InvokeAsync("SendMessage",
                      "Console", $"Отложенных {wallTotal} постов");

                    // TODO: Остановка
                    if (threadstopflag == true || wallTotal == 150)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");
                        Logging.ErrorLogging($"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");

                        connection.InvokeAsync("SendMessage",
                          "Console", $"Остановили поток: {threadstopflag} или лимит отложенных постов = {wallTotal}");
                        break;
                    }

                    Console.WriteLine(style.PlaylistName);
                    Console.WriteLine(trackscount);
                    Console.WriteLine(trackstop);

                    try
                    {
                        Console.WriteLine($"playlist_id: {style.PlaylistId} ");
                        connection.InvokeAsync("SendMessage", "Console", $"playlist_id: {style.PlaylistId} ");
                        Thread result = StartTheParserThread(trackscount, style, style.trueID, trackstop);
                        postMessage = StringWorkers.GetPostMessageFromStyle(style.PlaylistName);
                        do { Thread.Sleep(100); }
                        while (parser_finished == false);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                        connection.InvokeAsync("SendMessage", "Console", $"{exc} ");
                    }

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
                            try
                            {
                                // получаем первый элемент без его извлечения
                                Photostock_class pickedPhotostock = photostockQueue.Peek();
                                photostock_new = pickedPhotostock.URL;
                            }
                            catch (Exception ex) { Console.WriteLine(ex); photostock_new = null; connection.InvokeAsync("SendMessage", "Console", $"{ex} "); }
                        }

                        int stockPage = 1; //только с 1ой страницы
                        try
                        {
                            //Меняем фотку
                            postcounter++;

                            photourl = PhotoParserAuto(photostock_new, postcounter, style.PlaylistName, stockPage);

                            if (String.IsNullOrEmpty(photourl) == true)
                            {
                                //Get PlaylistImage
                                photourl = style.ImageUrl;

                                Console.ForegroundColor = ConsoleColor.Red;
                                Logging.ErrorLogging(String.Format("Пустая ссылка на фото, сток: {0} счетчик постов: {2}, замена обложкой плейлиста {1}", photostock_new, photourl, postcounter));
                                connection.InvokeAsync("SendMessage", "Console", String.Format("Пустая ссылка на фото, сток: {0} счетчик постов: {2}, замена обложкой плейлиста {1}", photostock_new, photourl, postcounter));
                                throw new Exception(String.Format("Пустая ссылка на фото, сток: {0} счетчик постов: {2}, замена обложкой плейлиста {1}", photostock_new, photourl, postcounter));

                            }
                        }
                        catch (Exception ex)
                        {

                            postcounter = 0; //Сброс

                            if (photostockQueue.Count > 0)
                            {
                                // получаем первый элемент стоков из очереди без его извлечения
                                Photostock_class pickedPhotostock = photostockQueue.Peek();

                                //Добавляем в конец очереди этот же фотосток
                                photostockQueue.Enqueue(pickedPhotostock);

                                // извлекаем первый элемент очереди
                                Photostock_class queuePhotostock = photostockQueue.Dequeue(); //теперь очередь из оставшихся стоков + в конце тот же сток
                                photostock_new = queuePhotostock.URL;
                                photourl = PhotoParserAuto(photostock_new, postcounter, style.PlaylistName, stockPage);
                            }
                            else photostock_new = null;

                            if (String.IsNullOrEmpty(photourl) == true)
                            {
                                photourl = style.ImageUrl;
                            }

                        }
                        finally
                        {
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
                                connection.InvokeAsync("SendMessage", "Console", $" {ex} ");
                                photofilename = "background-image.png";
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
                        connection.InvokeAsync("SendMessage", "Console", $"Размещаем пост на стену");

                        var attsTuple = VkTools.AddPhotoToAttachFromUrl(photofilename, attachments, postMessage, LstBox_AddedTracks);
                        attachments = attsTuple;
                        photofilename = "tempimage.png";
                        PosterOnWall(attachments, style, photourl, SearchingList);
                        do
                        {
                            Thread.Sleep(100);

                        } while (posted == false);
                        ind++;
                    }
                    else //SearchingList=0 если не найдено в поиске треков, что делать?
                    {
                        parser_finished = false;
                        tracksfound = false;
                        continue;
                    }

                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Процесс завершен!");
                connection.InvokeAsync("SendMessage", "Console", $"Процесс завершен!");
                postcounter = 1;
            }
            catch (ThreadAbortException exc)
            {
                Console.WriteLine("Поток прерван, код завершения "
                         + exc.ExceptionState);
            }
            Console.WriteLine($"Процесс завершен. Текущее время: {DateTime.Now:g}");
            connection.InvokeAsync("SendMessage", "Console", $"Процесс завершен. Текущее время: {DateTime.Now:g}");
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
        /// <include file='docParser.xml' path='docs/members[@name="parser"]/Parser/*'/>
        private static async Task Parser(int trackscount, vkaudioposter_ef.parser.Playlist style, string playlistId, string trackstop)
        {
            var config = SpotifyClientConfig
            .CreateDefault()
            .WithAuthenticator(new ClientCredentialsAuthenticator(clientId, clientSecret));//from env

            var spotify = new SpotifyClient(config);
            Console.WriteLine("Parsing: " + style.PlaylistName);

            try
            {
                //For Spotify
                if (playlistId != null)
                // if (user_id != null || playlist_id != null) //Если Spotify Only
                {
                    await SpotifyTools.SpotyParser(playlistId, spotify);
                }
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\nDownload tracks canceled.\r\n");
            }
            Program.parser_finished = true;
        }

        /// <summary>
        /// Searching tracks in VK 
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
        public void MakeUrlFromLines(List<SpotyTrack> tracksToFind, FormattedPlaylist styletoDB)
        {
            VkApi api = new();

            if (apiWS == false)
                try
                {
                    var services = new ServiceCollection();
                    services.AddAudioBypass();
                    api = new VkApi(services);

                    api.Authorize(new ApiAuthParams
                    {
                        Login = VKLogin,
                        Password = VKPass
                    });
                }
                catch (Exception ex)
                {

                    Logging.ErrorLogging(ex);
                }

            string trackNameAndAuthors;
            int existcounter = 0;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Список треков из парсера:\n");

            foreach (var track in tracksToFind)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{track.GetTrackAndAuthors()}");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nЖанр: {styletoDB.PlaylistName}");
            connection.InvokeAsync("SendMessage", "Console", $"Genre: {styletoDB.PlaylistName}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Ищем в поиске треки");
            connection.InvokeAsync("SendMessage", "Console", $"Searching tracks..");

            int unsearchtracks = 0;
            int publishedtracks = 0;

            foreach (var trackobj in tracksToFind.ToList())
            {
                string nameAndAuthors = trackobj.GetTrackAndAuthors();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Текущий трек: {nameAndAuthors}");

                //Обращаться к БД каждый раз как пытаемся новый K трек найти, Если в БД нет такого - то ищем
                var postedTracks = DBUtils.GetPostedTracksFromDB(styletoDB);
                var unfoundTracks = DBUtils.GetUnfoundTracksFromDB(styletoDB);

                //Приводим название к виду как В БД
                string current_track = nameAndAuthors.Replace("\'", "\"");

                List<string> fmtUnfoundTracks = new();
                foreach (var uT in unfoundTracks.ToList())
                {
                    var newT = uT.Replace("%20", "");
                    fmtUnfoundTracks.Add(newT);
                }

                List<string> fmtPostedTracks = new();
                foreach (var pT in postedTracks.ToList())
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
                        trackNameAndAuthors = current_track;
                        string url = JsonTools.ConcatSearchReq(url1, trackNameAndAuthors.Trim(), url3);


                        //---------------Old-------------------------
                        string json = JsonTools.SearchWeb(url);

                        if (json != null)
                        {
                            string strWithoutSpaces = trackNameAndAuthors.Replace("%20", " "); //Запрос поиска Имя+трек+микс
                            string FullId = StringWorkers.GetFullIdFromString(strWithoutSpaces, json);//нашли в запросе ID первой песни -111111111_222222222

                            if (FullId != "0") //если нашли трек в поиске
                            {
                                var mediaOwnId = StringWorkers.GetOwnIdAndMediaIdFromFullId(FullId);
                                int ownId = mediaOwnId.Item1;
                                int mediaId = mediaOwnId.Item2;

                                bool isExist = DBUtils.CheckFoundTrack(current_track, false);

                                if (isExist == true)
                                {
                                    continue;
                                }

                                SearchingList.Add(new SpotyVKTrack(trackNameAndAuthors.Trim(), mediaId, ownId, trackobj.Urls, trackobj.PreviewUrl));

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
                                catch (Exception e) { var err = e; Console.ForegroundColor = ConsoleColor.DarkGray; continue; };
                            }
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
                                SearchOwn = false,
                                Sort = AudioSort.AddedDate
                            });

                            if (audios.Count != 0)
                            {
                                foreach (var audio in audios.ToList())
                                {
                                    string allArtists = null;
                                    mp3Url = audio.Url;
                                    var mainArtists = audio.MainArtists;
                                    string oneArtist = audio.Artist;
                                    string trackName = audio.Title;
                                    string subTitle = audio.Subtitle;

                                    if (mainArtists.Count() > 1)
                                        foreach (var artist in mainArtists)
                                        {
                                            if (artist.Name != null)
                                            {
                                                allArtists += " " + artist.Name;
                                                fullTrackName = allArtists + " " + trackName + " " + subTitle;
                                            }
                                            else continue;
                                        }
                                    else
                                    {
                                        if (mainArtists != null || oneArtist != null)
                                            fullTrackName = oneArtist + " " + trackName + " " + subTitle;
                                        else continue;
                                    }

                                    //Сравнить название трека+исполнитель с тем что искали
                                    int diff = Tools.Metrics.LevenshteinDistance(current_track, fullTrackName); //Получение расстояния между строками исходной и результатов
                                    if (diff != -1)
                                    {
                                        ownID = audio.OwnerId;
                                        mediaID = audio.Id;
                                        SearchingList.Add(new SpotyVKTrack(fullTrackName.Trim(), mediaID, ownID));
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
                            DBUtils.InsertFoundTrackInDB(fullTrackName, styletoDB, publication_date, false, (int)ownID, (int)mediaID);
                        }
                        catch (Exception ex) //Если любая ошибка, перейти к след.треку!
                        {
                            SearchingList.Remove(new SpotyVKTrack(fullTrackName.Trim(), mediaID, ownID));
                            Logging.ErrorLogging(ex);
                            continue;
                        }
                        finally
                        {
                            existcounter++;
                        }

                        //если счетчик достиг 9 треков, остановить поиск!
                        if (existcounter == 9) break; //9
                    }
                }
                Thread.Sleep(searchCoolDown); //Задержка поиска
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
            string url = null; //Прямая ссылка на фото
            HtmlAgilityPack.HtmlDocument doc = null;

            try
            {
                //скачиваем страницу
                try
                {
                    HtmlWeb web = new();
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
                        ParserXpath pXp = DBUtils.GetPhotostockNodContainer(photostock);
                        nodContainer = pXp.Xpath;//Контейнер с картинками на странице (последний grid)
                                                 //*[@id="root"]/div[1]/div/div/div/article/div/div[2]/div/div
                                                 //nodContainer = "//*[@id=\"root\"]/div[1]/div/div/div/article/div/div[2]/div/div";
                        try
                        {
                            url = PhotoParser.DevianPageParser(doc, nodContainer, i, pXp);
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

                    case null:
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Null passed to this method.\n No SWITCH IN STOCK");
                        break;
                }

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
        /// Posting on VK wall
        /// </summary>
        /// <param name="attachments"></param>
        /// <param name="fmtPlaylist"></param>

#nullable enable
        public void PosterOnWall(List<MediaAttachment> attachments, FormattedPlaylist? fmtPlaylist, string photourl, List<SpotyVKTrack> SearchingList)
        {
            DateTime localDate = DateTime.Now;
            string cultureName = "ru-RU";
            var culture = new CultureInfo(cultureName);

            MessageToAttach = postMessage;

            Console.WriteLine("Авторизация для поста");
            connection.InvokeAsync("SendMessage", "Console", "Авторизация для поста");
            VkApi api = new();
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
                connection.InvokeAsync("SendMessage", "Console", $"Будет опубликован: {publication_date}");

            }

            //Если уже 1 раз опубликовали, publication_date.AddHours(1) увеличили на Час
            else
            {
                if (hoursperiod != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Пост 1 раз опубликован, увеличиваем дату на {hoursperiod} час");
                    connection.InvokeAsync("SendMessage", "Console", $"Пост 1 раз опубликован, увеличиваем дату на {hoursperiod} час");
                    publication_date = publication_date.AddHours(hoursperiod);
                }
                if (minutesperiod != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Пост 1 раз опубликован, увеличиваем дату на {minutesperiod} минут");
                    connection.InvokeAsync("SendMessage", "Console", $"Пост 1 раз опубликован, увеличиваем дату на {minutesperiod} минут");
                    publication_date = publication_date.AddMinutes(minutesperiod);
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Будет опубликован: {publication_date}  {cleared}й пост");
            }
            connection.InvokeAsync("SendMessage", "Console", $"Будет опубликован: {publication_date}  {cleared}й пост");

            {
                try
                {
                    long postId = api.Wall.Post(new WallPostParams
                    {
                        Attachments = attachments,
                        OwnerId = ownid,
                        Message = MessageToAttach,
                        PublishDate = publication_date,
                    });

                    LastDatePosted = publication_date;

                    DBUtils.UpdatePublicationDateOfTracksAndInsertToDB(fmtPlaylist, LastDatePosted, postId, attachments, ownid, MessageToAttach, SearchingList, photourl);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Пост опубликован! {MessageToAttach}");
                    connection.InvokeAsync("SendMessage", "Console", $"Пост опубликован! {MessageToAttach}");

                    ClearAll();
                    posted = true;

                }

                catch (TooManyRequestsException vk_req)
                {
                    Logging.ErrorLogging(vk_req);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Превышено число запрсов в секунду к ВК, ждем 5 сек...");
                    connection.InvokeAsync("SendMessage", "Console", "Превышено число запрсов в секунду к ВК, ждем 5 сек...");
                    Thread.Sleep(5000);
                    changed_time = true;
                }

                catch (VkNet.Exception.PostLimitException vk_lim)
                {
                    Logging.ErrorLogging(vk_lim);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Лимит запланированных записей! Добавили в очередь публикаций!");
                    connection.InvokeAsync("SendMessage", "Console", "Лимит запланированных записей! Добавили в очередь публикаций!");

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
                    connection.InvokeAsync("SendMessage", "Console", $"Число постов в очереди: {wallPostQueue.Count}");

                    ClearAll();
                    posted = true;
                }

                catch (VkNet.Exception.UnknownException unknown)
                {
                    Logging.ErrorLogging(unknown);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка, на это время уже отложена запись, увеличиваем время на {5} минут. Или ошибка публикации");
                    connection.InvokeAsync("SendMessage", "Console", $"Ошибка, на это время уже отложена запись, увеличиваем время на {5} минут. Или ошибка публикации");
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
                    connection.InvokeAsync("SendMessage", "Console", $"Ошибка {ex}\nТекущее время: {localDate.ToString(culture)}, {localDate.Kind}");
                    ClearAll();
                    changed_time = true;
                }

                finally
                {
                    //если ошибка ограничение 25 постов в день - изменить день
                    if (changed_time == true)
                    {
                        var postId = api.Wall.Post(new WallPostParams
                        {
                            Attachments = attachments,
                            OwnerId = ownid,
                            Message = MessageToAttach,
                            PublishDate = publication_date
                        });

                        LastDatePosted = publication_date;

                        DBUtils.UpdatePublicationDateOfTracksAndInsertToDB(fmtPlaylist, LastDatePosted, postId, attachments, ownid, MessageToAttach, SearchingList, photourl);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Пост {MessageToAttach}\n, увеличила дату {LastDatePosted}, опубликован!");
                        connection.InvokeAsync("SendMessage", "Console", $"Пост {MessageToAttach}\n, увеличила дату {LastDatePosted}, опубликован!");
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
            connection.InvokeAsync("SendMessage", "Console", $"Очистка...");
        }

    }
}
