using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vkaudioposter.Classes;
using vkaudioposter.MySQL;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using vkaudioposter_ef.Model;
using Track = vkaudioposter.Track;
using vkaudioposter_Console.Classes;
using vkaudioposter;

namespace vkaudioposter_Console
{
    public class Config
    {
        #region StaticVars
        public static bool firstRun; //create DB, initial seed
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
        public static Random random = new();

        //private static readonly int searchCoolDown = 700; //ms для поиска по apiяws

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
        private static List<Chart> ChartList = new List<Chart>();
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

        public List<string> SelectedGenre = new();
        public List<string> PhotoStock = new();

        private List<Track> SearchingList = new(); //Список найденных треков

        private List<Track> SelectedTrackList = new();
        private List<MediaAttachment> attachments = new();
        //For COnsole
        private List<string> CB_PhotoStock = new();
        //private List<string> LSTBOX_Genres = new List<string>();

        private List<string> LstBox_AddedTracks = new();

        Queue<Photostock_class> photostockQueue = new();
        Queue<WallPostParams> wallPostQueue = new();

        #region Config
        /// <summary>
        /// 
        /// </summary>
        public static void LoadConfigsFromEnv()
        {
            DotNetEnv.Env.Load();

            debug = DotNetEnv.Env.GetBool("DEBUG");
            hoursperiod = DotNetEnv.Env.GetInt("HOURS_PERIOD");
            minutesperiod = DotNetEnv.Env.GetInt("MINUTES_PERIOD");

            rollbarToken = DotNetEnv.Env.GetString("ROLLBAR_TOKEN");
            //DB_HOST = DotNetEnv.Env.GetString("DB_HOST");
            //DB_USER = DotNetEnv.Env.GetString("DB_USER");
            //DB_PASS = DotNetEnv.Env.GetString("DB_PASS");
            //DB_NAME = DotNetEnv.Env.GetString("DB_NAME");
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

            //redisHost = DotNetEnv.Env.GetString("REDIS_HOST");
            //redisPort = DotNetEnv.Env.GetInt("REDIS_PORT");
            //redisPassword = DotNetEnv.Env.GetString("REDIS_PASSWORD");

            //efDB = DotNetEnv.Env.GetString("EF_DATABASE");
            //efUser = DotNetEnv.Env.GetString("EF_USER");
            //efPass = DotNetEnv.Env.GetString("EF_PASSWORD");
            firstRun = DotNetEnv.Env.GetBool("FIRST_RUN");
            useProxy = DotNetEnv.Env.GetBool("USE_PROXY");
        }

        public void OnLoad()
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
    }
}
