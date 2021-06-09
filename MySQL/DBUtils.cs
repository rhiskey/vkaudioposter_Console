using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using vkaudioposter_Console;
using vkaudioposter_Console.Classes;
using vkaudioposter_Console.Tools;
using vkaudioposter_Console.VKUtills;
using vkaudioposter_ef.Model;
using vkaudioposter_ef.parser;
using VkNet.Model.Attachments;

namespace vkaudioposter.MySQL
{
    class DBUtils
    {
        public static void CountPublishedTracksInStyles()
        {
            using var context = new vkaudioposter_ef.AppContext();
            var countPerPlaylists =
                from track in context.PostedTracks
                group track by track.PlaylistId into trackGroup
                orderby trackGroup.Count() descending
                select new
                {
                    ID = trackGroup.Key,
                    Count = trackGroup.Count(),
                };

            foreach (var s in countPerPlaylists)
                AddToDb(s);
        }
        protected static async void AddToDb(dynamic t)
        {
            using var context = new vkaudioposter_ef.AppContext();
            var id = (int)t.ID;
            var playlist = context.Playlists.Where(y => y.Id == id).FirstOrDefault();
            playlist.Count = Convert.ToInt32(t.Count);
            await context.SaveChangesAsync();
        }

        public static List<FormattedPlaylist> GetAllPlaylists()
        {
            List<vkaudioposter_ef.parser.Playlist> playlists;
            List<FormattedPlaylist> formattedList = new();

            using (var context = new vkaudioposter_ef.AppContext())
            {
                playlists = context.Playlists.Where(p => p.Status == 1).ToList();
                foreach (var elem in playlists)
                {
                    //Get only ID 
                    string pattern = @"^spotify:\w*";
                    string plID = null;
                    if (Regex.IsMatch(elem.PlaylistId, pattern, RegexOptions.IgnoreCase))
                    {
                        int index = elem.PlaylistId.IndexOf("playlist:");
                        string subs = elem.PlaylistId.Remove(0, index + 9);
                        plID = subs;
                    }
                    FormattedPlaylist fmt = new(elem, plID);
                    formattedList.Add(fmt);
                }
            }
            return formattedList;
        }

        public static void UpdatePublicationDateOfTracksAndInsertToDB(FormattedPlaylist formattedPlaylist,
            DateTime publish_date, long postId = 0, List<MediaAttachment> atts = null, long ownerId = 0, string message = null,
            List<SpotyVKTrack> searchList = null, string photourl = null)
        {
            using var context = new vkaudioposter_ef.AppContext();

            context.Database.EnsureCreated();

            ////TODO check curr timezone
            //try
            //{
            //    TimeZoneInfo moZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            //    var newDt = TimeZoneInfo.ConvertTime(publish_date, moZone, TimeZoneInfo.Local);
            //}
            //catch (TimeZoneNotFoundException)
            //{
            //    Console.WriteLine("The registry does not define the Russian Standard Time zone.");
            //}
            //catch (InvalidTimeZoneException)
            //{
            //    Console.WriteLine("Registry data on the Russian Standard Time zone has been corrupted.");
            //}
            DateTime GMTfixTime = publish_date.AddHours(3); //BAD

            vkaudioposter_ef.Model.Post post = new();
            post.PostId = postId;
            post.OwnerId = ownerId;
            post.Message = message;
            post.PublishDate = GMTfixTime;
            post.PostedTracks = new List<PostedTrack>();
            post.PostedPhotos = new List<PostedPhoto>();
            context.Posts.Add(post);

            foreach (var at in atts)
                foreach (var el in searchList)
                {
                    // if audio ->; if photo->
                    try
                    {
                        PostedTrack postedTrack = new();

                        var mIdSl = el.GetMediaId();
                        var oIdSl = el.GetOwnerId();
                        var titleSl = el.GetTitle();
                        if (mIdSl == at.Id && oIdSl == at.OwnerId)
                            postedTrack.Trackname = titleSl;
                        else continue;

                        var preUrl = el.PreviewUrl;
                        var urls = el.Urls;

                        postedTrack.PlaylistId = formattedPlaylist.Id;
                        postedTrack.Date = GMTfixTime;
                        postedTrack.MediaId = at.Id;
                        postedTrack.OwnerId = at.OwnerId;
                        postedTrack.PreviewUrl = preUrl;
                        postedTrack.Url = urls.First().Value.ToString();

                        List<TrackUrl> trackUrls = new();
                        foreach (var url in urls)
                            trackUrls.Add(new TrackUrl { Key = url.Key, Value = url.Value });

                        postedTrack.TrackUrls = trackUrls;


                        post.PostedTracks.Add(postedTrack);
                    }
                    catch (Exception ex) { Console.WriteLine(ex); }

                }

            PostedPhoto postedPhoto = new();
            postedPhoto.Url = photourl;
            post.PostedPhotos.Add(postedPhoto);

            var countInStyle =
                from playlist in context.Playlists
                where playlist.Id == formattedPlaylist.Id
                select playlist.Count;

            var playlist2 = context.Playlists.Where(y => y.Id == formattedPlaylist.Id).FirstOrDefault();
            int newCount = Convert.ToInt32(countInStyle.First()) + searchList.Count;
            playlist2.Count = newCount;

            context.SaveChanges();

        }

        public static void InsertFoundTrackInDB(string trackname, FormattedPlaylist formattedPlaylist, DateTime publish_date, bool? isFirstTime, int ownerId = 0, int mediaId = 0)
        {
            if (Program.debug == false)
            {
                try
                {
                    using var context = new vkaudioposter_ef.AppContext();
                    if (isFirstTime == true)
                        context.Database.EnsureDeleted();

                    // Creates the database if not exists
                    context.Database.EnsureCreated();


                    var pt1 = new PostedTrack
                    {
                        Trackname = trackname,
                        Date = publish_date,
                        //Playlist = formattedPlaylist,
                        PlaylistId = formattedPlaylist.Id,
                        OwnerId = ownerId,
                        MediaId = mediaId
                    };

                    context.PostedTracks.Add(pt1);

                    context.SaveChanges();
                }
                catch (Exception ex) { var err = ex; }
            }
            else
            {
                using var context = new vkaudioposter_ef.AppContext();
                if (isFirstTime == true)
                    context.Database.EnsureDeleted();

                // Creates the database if not exists
                context.Database.EnsureCreated();


                var pt1 = new PostedTrack
                {
                    Trackname = trackname,
                    Date = publish_date,
                    //Playlist = formattedPlaylist,
                    PlaylistId = formattedPlaylist.Id,
                    OwnerId = ownerId,
                    MediaId = mediaId
                };

                context.PostedTracks.Add(pt1);

                context.SaveChanges();
            }

        }

        public static bool CheckFoundTrack(string trackname, bool? isFirstTime)
        {

            using var context = new vkaudioposter_ef.AppContext();
            if (isFirstTime == true)
                context.Database.EnsureDeleted();

            // Creates the database if not exists
            context.Database.EnsureCreated();

            var track = context.PostedTracks.Where(a => a.Trackname == trackname).FirstOrDefault();
            if (track != null)
                return true;
            else return false;


        }

        public static void InsertUnfoundTrackInDB(string trackname, FormattedPlaylist formattedPlaylist, bool? isFirstTime)
        {
            if (Program.debug == false)
            {
                try
                {
                    using var context = new vkaudioposter_ef.AppContext();
                    if (isFirstTime == true)
                        context.Database.EnsureDeleted();

                    // Creates the database if not exists
                    context.Database.EnsureCreated();

                    var pt1 = new UnfoundTrack
                    {
                        Trackname = trackname,
                        PlaylistId = formattedPlaylist.Id
                    };

                    context.UnfoundTracks.Add(pt1);

                    context.SaveChanges();
                }
                catch (Exception ex) { var err = ex; }
            }
            else
            {
                using var context = new vkaudioposter_ef.AppContext();
                if (isFirstTime == true)
                    context.Database.EnsureDeleted();

                // Creates the database if not exists
                context.Database.EnsureCreated();

                var pt1 = new UnfoundTrack
                {
                    Trackname = trackname,
                    PlaylistId = formattedPlaylist.Id
                };

                context.UnfoundTracks.Add(pt1);

                context.SaveChanges();
            }
        }

        public static List<string> GetUnfoundTracksFromDB(FormattedPlaylist playlist)
        {
            List<string> unfoundTracksNames = new();

            using (var context = new vkaudioposter_ef.AppContext())
            {
                unfoundTracksNames = (from track in context.UnfoundTracks
                                      where track.Playlist == playlist
                                      select track.Trackname).ToList();
            }

            return unfoundTracksNames;
        }

        public static List<string> GetPostedTracksFromDB(FormattedPlaylist playlist)
        {
            List<string> postedTracksNames = new();

            using (var context = new vkaudioposter_ef.AppContext())
            {
                postedTracksNames = (from track in context.PostedTracks
                                     where track.Playlist == playlist
                                     select track.Trackname).ToList();
            }

            return postedTracksNames;
        }

        public static List<string> LoadPhotoStocksFromDB()
        {
            List<vkaudioposter_ef.parser.ConsolePhotostock> photostocks;
            List<string> urls = new();
            using (var context = new vkaudioposter_ef.AppContext())
            {
                photostocks = context.Photostocks.Where(p => p.Status == 1).ToList();
                foreach (var stock in photostocks)
                    urls.Add(stock.Url);
            }
            return urls;
        }

        public static ParserXpath GetPhotostockNodContainer(string photostockName)
        {
            ParserXpath xPath = new();

            using (var context = new vkaudioposter_ef.AppContext())
            {

                var xPathId = (from stock in context.Photostocks
                               where stock.Url == photostockName
                               select stock.ParserXpathId).FirstOrDefault();

                xPath = (from xp in context.ParserXpaths
                         where xp.Id == xPathId
                         select xp).FirstOrDefault();
            }

            return xPath;
        }

        //// IN - Parameters of attachments and POST ownId, message, publDate
        //public static void AddPostInDB(List<MediaAttachment> mediaAttachments, long? ownerId, string message, DateTime publishDate)
        //{
        //    // Insert in Postponed_Posts through Procedure DB
        //    string DateTimeMySQL = publishDate.ToString("yyyy-MM-dd H:mm:ss");
        //    // Generate AttachmentsId int(11)
        //    Random rnd = new Random();

        //    int postId = rnd.Next(1, 99999999);

        //    string sql = "sp_insert_postponed_post";
        //    MySqlCommand command = new MySqlCommand(); ;
        //    // MySqlDataAdapter adapter = new MySqlDataAdapter();

        //    MySqlConnection connection = DBUtils.GetDBConnection();
        //    command.Connection = connection;

        //    command = new MySqlCommand(sql, connection)
        //    {
        //        CommandType = CommandType.StoredProcedure
        //    };

        //    command.Parameters.AddWithValue("in_attachments", postId);
        //    command.Parameters.AddWithValue("in_ownerID", ownerId);
        //    command.Parameters.AddWithValue("in_message", message);
        //    command.Parameters.AddWithValue("in_PublishDate", publishDate);


        //    command.Connection.Open();

        //    var result = command.ExecuteNonQuery(); //result = # of rows affected       
        //    if (result == 0)
        //    {
        //        throw new Exception(String.Format($"Невозможно выполнить процедуру вставки поста DateTimeMySQL= {DateTimeMySQL} "));
        //    }

        //    Console.WriteLine("Inserted postponed post: " + DateTimeMySQL);
        //    //Notifications.PostponedPost("Inserted postponed post: " + DateTimeMySQL + " message: " + message);

        //    command.Dispose();
        //    connection.Close();

        //    List<int> attIdList = new List<int>();

        //    // Foreach attach Insert in Media_Attachment
        //    foreach (var attach in mediaAttachments)
        //    {
        //        // Insert In Procedure DB: sp_insert_media_attachment
        //        sql = "func_insert_media_attachment";
        //        command = new MySqlCommand(); ;
        //        // MySqlDataAdapter adapter = new MySqlDataAdapter();

        //        connection = DBUtils.GetDBConnection();
        //        command.Connection = connection;

        //        command = new MySqlCommand(sql, connection)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        command.Parameters.AddWithValue("in_mediaID", attach.Id);
        //        command.Parameters.AddWithValue("in_ownerID", attach.OwnerId);
        //        command.Parameters.AddWithValue("in_accessKey", attach.AccessKey);
        //        command.Parameters.AddWithValue("in_postID", postId);

        //        //Attempt to call stored function '`parser`.`func_insert_media_attachment`' without specifying a return parameter
        //        var id_out = new MySqlParameter("@out_id", SqlDbType.Int)
        //        {
        //            Direction = ParameterDirection.ReturnValue
        //        }; //@out_id
        //        command.Parameters.Add(id_out);

        //        command.Connection.Open();

        //        result = command.ExecuteNonQuery(); //result = # of rows affected       
        //        //if (result == 0)
        //        //{
        //        //    throw new Exception(String.Format("Невозможно выполнить процедуру вставки вложения mediaId= {0} ", attach.Id));
        //        //}
        //        var id = id_out.Value;
        //        Console.WriteLine("Inserted media attachment ID: " + id);

        //        //Add ids to list <int> - TODO check if not null?
        //        attIdList.Add((int)id);

        //        command.Dispose();
        //        connection.Close();
        //    }
        //}

        //       // Получение из базы отложенных постов - формирование списка постов + вложений
        //       private List<WallPostParams> GetPostponedPromDB()
        //       {
        //           List<WallPostParams> postponedPostList = new List<WallPostParams>();
        //           //TODO
        //// SELECT * From vw_countposts()

        //           // TODO Получить список постов из БД, получить список вложений, сопоставить в классе

        //           //for (int i = 0; i<= ppCount; i++)
        //           //{
        //           //    postponedPostList[i].Attachments = ; //TODO SELECT * FROM vw_attachments => where 
        //           //    postponedPostList[i].Message = ;
        //           //    postponedPostList[i].OwnerId = ;
        //           //    postponedPostList[i].PublishDate = ;
        //           //}

        //           return postponedPostList;
        //       }

        /// <summary>
        /// Checking last post date in DB or VK. If pass true -> get From VK
        /// </summary>
        /// <param name="checkLastPostedInVK"></param>
        /// <returns></returns>
        public static (DateTime, DateTime) GetLastDateFromDB(bool checkLastPostedInVK = false)
        {
            vkaudioposter_ef.parser.PostedTrack lastPostedTrack = null;
            DateTime publication_date = DateTime.Now;

            if (checkLastPostedInVK == true)
            {
                // Get last date from postponed wall post
                //VkTools vkTools = new();
                try
                {
                    publication_date = VkTools.GetLastPostponedPostDate();
                }
                catch (Exception ex)
                {
                    // TODO:
                    Console.WriteLine($"{ex.Message}");
                }
            }
            else
                using (var context = new vkaudioposter_ef.AppContext())
                {
                    try
                    {
                        lastPostedTrack = context.PostedTracks.OrderBy(d => d.Date).Last();
                        publication_date = lastPostedTrack.Date;
                    }
                    catch (System.InvalidOperationException ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                    }
                }

            DateTime LastDatePosted = new();
            //Возможна ошибка когда последняя дата раньше чем сейчас
            try
            {
                LastDatePosted = publication_date;
                publication_date = publication_date.AddHours(vkaudioposter_Console.Program.hoursperiod);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Дата отложенной публикации: {publication_date}");
                Console.WriteLine($"Дата публикации из БАЗЫ ДАННЫХ/VK: {LastDatePosted}");
            }
            catch (Exception ex)
            {
                Logging.ErrorLogging(ex);
                Logging.ReadError();
            }
            var result = (publication_date, LastDatePosted);
            return result;
        }
    }
}
