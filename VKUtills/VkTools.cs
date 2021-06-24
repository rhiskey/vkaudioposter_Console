using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using vkaudioposter;
using vkaudioposter_Console.Tools;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

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
            int flag = 2;
            return (attachments, flag);
        }

        //Загрузка фото на стену и добавление во вложенные
        public static IReadOnlyCollection<Photo> SendOnServerOld(string photoFilename, string postMessage)
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
                string responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadurl, photoFilename));
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
                    formatted = formatted.Remove(formatted.Length - 1);
                    formatted = formatted.Replace(" ", "_");
                    formatted = formatted.Replace("-", "");
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

        public static DateTime DatePick(string onlydate, string curTimeLong)
        {


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

            WallGetParams wallGetParams = new();
            var wallFilter = VkNet.Enums.SafetyEnums.WallFilter.Postponed;

            var wallObj = api.Wall.Get(new WallGetParams { Offset = 50, Count = 100, OwnerId = Program.ownid, Filter = wallFilter });
            var allPosts = wallObj.WallPosts;
            var pCount = allPosts.Count;
            var lastPost = allPosts.ElementAt(pCount - 1);

            return lastPost.Date.Value.AddHours(3); //TODO
        }
    }

}
