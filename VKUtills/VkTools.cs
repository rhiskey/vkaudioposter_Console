using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using vkaudioposter;
using vkaudioposter_Console.Tools;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using System.Linq;


namespace vkaudioposter_Console.VKUtills
{
    class VkTools
    {
        public static ulong CheckPostponedAndGetCount()
        {
            VkApi api = new();
            api.Authorize(new ApiAuthParams
            {
                AccessToken = Program.Token
            });

            // Check postponed
            VkNet.Enums.SafetyEnums.WallFilter wallFilterPostponed = VkNet.Enums.SafetyEnums.WallFilter.Postponed;
            var postponedCount = api.Wall.Get(new WallGetParams
            {
                Filter = wallFilterPostponed,
                OwnerId = Program.ownid,
                //Offset
            });
            var wallTotal = postponedCount.TotalCount;
            return wallTotal;
        }

        /// <summary>
        /// Add photos to VK attachments from URL
        /// </summary>
        /// <param name="photofilename"></param>
        /// <param name="attachments"></param>
        /// <param name="postMessage"></param>
        /// <param name="trackNames"></param>
        /// <returns></returns>
        public static List<MediaAttachment> AddPhotoToAttachFromUrl(string photofilename, List<MediaAttachment> attachments, string postMessage, List<string> trackNames)
        {
            string filename2 = Path.GetFullPath(photofilename);
            var photolist = SendOnServer(filename2, postMessage, trackNames);  //отправляем на сервак, получаем ответ с картинкой
            attachments.AddRange(photolist);

            return attachments;
        }

        public static (List<MediaAttachment>, int) AddPhotoToAttachFromPC(string imagefile, List<MediaAttachment> attachments, string postMessage)
        {
            var sendTuple = SendOnServerOld(imagefile, postMessage);

            attachments.AddRange(sendTuple);
            //bool isStyleAdded = sendTuple.Item2; 
            //Poster.Enqueue(new PostParams() { Atts = attachments });
            int flag = 2;
            return (attachments, flag);
        }

        //Загрузка фото на стену и добавление во вложенные
        public static IReadOnlyCollection<Photo> SendOnServerOld(string photoFilename, string postMessage)
        {
            VkApi api = new();

            //Авторизация
            api.Authorize(new ApiAuthParams
            {
                AccessToken = Program.Token
            });

            //VkNet.Utils.VkCollection<Group> res = api.Groups.Get(new GroupsGetParams());
            Console.WriteLine("Авторизировались для загрузки фото на стену");
            UploadServerInfo getWallUploadServer = api.Photo.GetWallUploadServer(Program.groupid);
            string uploadurl = getWallUploadServer.UploadUrl;
            IReadOnlyCollection<Photo> photolist = null;
            // Загрузить фотографию.
            try
            {
                WebClient wc = new();
                //long? userid = getWallUploadServer.UserId;
                //long? albumid = getWallUploadServer.AlbumId;
                //string responseImg = null;
                //if (downloaded == false)
                //{
                //    responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadurl, filename));
                //    Thread.Sleep(100);
                //    responseImg.GetHashCode();
                //}
                //else
                // {
                string responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadurl, photoFilename));
                responseImg.GetHashCode();

                var msg = postMessage;

                msg = msg.Replace(" ", " #");
                msg = msg.Replace("-", "_");
                msg = msg.Replace("/", "_");
                msg = msg.Replace("&", " and ");


                string HashTags_style = Program.HashTags + " #" + msg;

                photolist = api.Photo.SaveWallPhoto(responseImg, Program.ownerid3, Program.groupid3, HashTags_style); //Сохранили фото на стену
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Logging.ErrorLogging(ex);
                Logging.ReadError();

            }
            return photolist;
        }

        public static IReadOnlyCollection<Photo> SendOnServer(string photoFilename, string postMessage, List<string> trackNamesList)
        {
            VkApi api = new();

            api.Authorize(new ApiAuthParams
            {
                AccessToken = Program.Token
            });

            Console.WriteLine("Авторизировались для загрузки фото на стену");
            UploadServerInfo getWallUploadServer = api.Photo.GetWallUploadServer(Program.groupid);
            string uploadurl = getWallUploadServer.UploadUrl;
            IReadOnlyCollection<Photo> photolist = null;

            try
            {
                WebClient wc = new();

                //long? userid = getWallUploadServer.UserId;
                //long? albumid = getWallUploadServer.AlbumId;
                string responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadurl, photoFilename));
                //Thread.Sleep(200);
                responseImg.GetHashCode();

                var msg = postMessage;

                msg = msg.Replace(" ", " #");
                msg = msg.Replace("-", "_");
                msg = msg.Replace("/", "_");
                msg = msg.Replace("&", " and ");

                string tracksHashtags = null;

                for (int i = 0; i < trackNamesList.Count; i++)
                {
                    string track = trackNamesList[i];
                    string formatted = track.ToLower();
                    formatted = formatted.Remove(formatted.Length - 1); //remove last space
                    formatted = formatted.Replace(" ", "_");
                    formatted = formatted.Replace("-", "");
                    //formatted = formatted.Replace('%20', "_")
                    if (i < (trackNamesList.Count - 1))
                        tracksHashtags += formatted + " #";
                    else tracksHashtags += formatted;
                }
                string HashTags_style = Program.HashTags + msg + " #" + tracksHashtags;

                photolist = api.Photo.SaveWallPhoto(responseImg, Program.ownerid3, Program.groupid3, HashTags_style);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Logging.ErrorLogging(ex);
                Logging.ReadError();

            }
            return photolist;
        }



        //Отложенная запись  выбор даты-Конвертация
        public static DateTime DatePick(string onlydate, string curTimeLong)
        {

            // 06.03.2014 14:10:00
            int day = int.Parse(onlydate.Split('.')[0]);
            int month = int.Parse(onlydate.Split('.')[1]);
            int year = int.Parse(onlydate.Split('.')[2]);

            int hour = int.Parse(curTimeLong.Split(':')[0]);
            int min = int.Parse(curTimeLong.Split(':')[1]);
            if (hour != 23 && ((min != 55) || (min != 56) || (min != 57) || (min != 58) || (min != 59)))
            {
                if ((min % 5) != 0)
                {
                    min += 5 - (min % 5);
                }

                if (min == 60)
                {
                    hour++;
                    min = 0;
                }
            }

            DateTime data_time = new(year, month, day, hour, min, 0);
            return data_time;
        }

        /// <summary>
        /// Авто добавление треков из Searching List
        /// </summary>
        /// <param name="SearchingList"></param>
        /// <param name="LstBox_AddedTracks"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public static (List<string>, List<MediaAttachment>, bool) AutoAddTracksToAttachments(List<SpotyVKTrack> SearchingList, List<string> LstBox_AddedTracks, List<MediaAttachment> attachments)
        {
            bool tracks_attached = false;
            int maximum_tracks = 9;
            if (SearchingList.Count < 9)
            {
                maximum_tracks = SearchingList.Count;
            }

            //Получение РЕАЛЬНЫх найденных названий из поиска и добавление в конечный список названий
            //var api = new VkApi();
            ////Авторизация
            //api.Authorize(new ApiAuthParams
            //{
            //    AccessToken = Token
            //});
            //string trackList = null;
            //Заполнить массив найденных ID и вывести инфу о треках

            for (int i = 0; i < maximum_tracks; i++)
            {
                string autoelem = SearchingList[i].GetTitle();

                LstBox_AddedTracks.Add(autoelem);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Вложка треков: " + LstBox_AddedTracks.Count);
            }

            for (int curobj = 0; curobj < maximum_tracks; curobj++)
            {
                attachments.Add(
                   new Audio
                   {
                       OwnerId = SearchingList[curobj].GetOwnerId(),
                       Id = SearchingList[curobj].GetMediaId(),
                       AccessKey = Program.accesstoken,
                   });
                //Poster.Enqueue(new PostParams() { Atts = attachments });
            }

            tracks_attached = true;
            return (LstBox_AddedTracks, attachments, tracks_attached);

        }

        /// <summary>
        /// Returns VK Community Wall latest postponed post date
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastPostponedPostDate()
        {
            DateTime dt = DateTime.Now;

            VkApi api = new();
            api.Authorize(new ApiAuthParams
            {
                AccessToken = Program.Token
            });

            //api.Wall.GetById();
            WallGetParams wallGetParams = new();
            var wallFilter = VkNet.Enums.SafetyEnums.WallFilter.Postponed;

            var wallObj = api.Wall.Get(new WallGetParams { Offset = 50, Count = 100, OwnerId = Program.ownid, Filter = wallFilter });
            var allPosts = wallObj.WallPosts;
            var pCount = allPosts.Count;
            var lastPost = allPosts.ElementAt(pCount - 1);

            //Console.WriteLine(lastPost.Date);

            return lastPost.Date.Value.AddHours(3); //TODO
        }

        /*        public static List<Track>SerachTracksVKApi(string query2search, VkApi api)
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
                            Query = query2search,
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
                                int diff = LevenshteinDistance(current_track, fullTrackName); //Получение расстояния между строками исходной и результатов
                                if (diff < 4 && diff != -1)
                                {
                                    ownID = audio.OwnerId;
                                    mediaID = audio.Id;
                                    //FullId = ownID.ToString() + "_" + selfID.ToString();

                                    SearchingList.Add(new Track(fullTrackName, mediaID, ownID));

                                    //lB_tracksTable.Items.Add("[" + countTracks + "] " + fullTrackName);
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
                                    InsertFoundTrackInDB(fullTrackName, styletoDB, publication_date, mp3Url);
                                    TCPClientSend(string.Format("Внесли трек {0} в базу {1} дата: {2} \n", fullTrackName, styletoDB, publication_date));
                                    //Thread.Sleep(100);
                                }
                                catch (Exception ex) //Если любая ошибка, перейти к след.треку!
                                {
                                    SearchingList.Remove(new Track(fullTrackName, mediaID, ownID));
                                    continue;
                                }
                                finally
                                {
                                    AddListItemMethod(existcounter);
                                    existcounter++;
                                }

                                //если счетчик достиг 9 треков, остановить поиск!
                                if (existcounter == 9) break;

                }*/
    }


    // TODO:
    abstract class Creator
    {
        // Обратите внимание, что Создатель может также обеспечить реализацию
        // фабричного метода по умолчанию.
        public abstract IProduct FactoryMethod();

        // Также заметьте, что, несмотря на название, основная обязанность
        // Создателя не заключается в создании продуктов. Обычно он содержит
        // некоторую базовую бизнес-логику, которая основана  на объектах
        // Продуктов, возвращаемых фабричным методом.  Подклассы могут косвенно
        // изменять эту бизнес-логику, переопределяя фабричный метод и возвращая
        // из него другой тип продукта.
        public string SomeOperation()
        {
            // Вызываем фабричный метод, чтобы получить объект-продукт.
            var product = FactoryMethod();
            // Далее, работаем с этим продуктом.
            var result = "Creator: The same creator's code has just worked with "
                + product.Operation() + product.CheckPostponed();

            return result;
        }
    }

    // Конкретные Создатели переопределяют фабричный метод для того, чтобы
    // изменить тип результирующего продукта.
    class ConcreteCreator1 : Creator
    {
        // Обратите внимание, что сигнатура метода по-прежнему использует тип
        // абстрактного продукта, хотя  фактически из метода возвращается
        // конкретный продукт. Таким образом, Создатель может оставаться
        // независимым от конкретных классов продуктов.
        public override IProduct FactoryMethod()
        {
            return new ConcreteProduct1();
        }
    }

    class ConcreteCreator2 : Creator
    {
        public override IProduct FactoryMethod()
        {
            return new ConcreteProduct2();
        }
    }

    // Интерфейс Продукта объявляет операции, которые должны выполнять все
    // конкретные продукты.
    public interface IProduct
    {
        string Operation();
        int CheckPostponed();
    }

    // Конкретные Продукты предоставляют различные реализации интерфейса
    // Продукта.
    class ConcreteProduct1 : IProduct
    {
        public string Operation()
        {
            return "{Result of ConcreteProduct1}";
        }
        public int CheckPostponed()
        {
            VkApi api = new();
            //Авторизация
            api.Authorize(new ApiAuthParams
            {
                AccessToken = Program.Token
            });


            return 0;
        }

    }

    class ConcreteProduct2 : IProduct
    {
        public int CheckPostponed()
        {
            throw new NotImplementedException();
        }

        public string Operation()
        {
            return "{Result of ConcreteProduct2}";
        }
    }

}
