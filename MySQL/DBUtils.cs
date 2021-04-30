using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using vkaudioposter_Console.Tools;
using VkNet.Model.Attachments;
using vkaudioposter_ef.parser;
using System.Linq;
using vkaudioposter_Console.Classes;
using vkaudioposter_Console.VKUtills;
using vkaudioposter_ef.Model;
using vkaudioposter_Console;

namespace vkaudioposter.MySQL
{
    class DBUtils
    {
        //private static readonly string efHost = vkaudioposter_Console.Program.DB_HOST;
        //private static readonly string efUser = vkaudioposter_Console.Program.efUser;
        //private static readonly string efPass = vkaudioposter_Console.Program.efPass;
        //private static readonly string efDB = vkaudioposter_Console.Program.efDB;
        //public static MySqlConnection GetDBConnection()
        //{
        //    //CREATE USER 'myuser'@'%' IDENTIFIED BY 'mypass';
        //    //GRANT ALL ON databasename.* TO 'myuser'@'%';

        //    string host = vkaudioposter_Console.Program.DB_HOST; 
        //    int port = 3306;
        //    string database = vkaudioposter_Console.Program.DB_NAME;
        //    string username = vkaudioposter_Console.Program.DB_USER;
        //    string password = vkaudioposter_Console.Program.DB_PASS;

        //    return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        //}

        public static List<FormattedPlaylist> GetAllPlaylists()
        {
            List<vkaudioposter_ef.parser.Playlist> playlists;
            List<FormattedPlaylist> formattedList = new List<FormattedPlaylist>();

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
                    FormattedPlaylist fmt = new FormattedPlaylist(elem, plID);
                    formattedList.Add(fmt);
                }          
            }
            return formattedList;
        }

        public static void UpdatePublicationDateOfTracks(List<string> tracknames, FormattedPlaylist formattedPlaylist, DateTime publish_date, long postId = 0 )
        {
            try
            {
                using var context = new vkaudioposter_ef.AppContext();

                // Creates the database if not exists
                context.Database.EnsureCreated();

                foreach (var name in tracknames)
                {
                    var postedTrackInDate = (from track in context.PostedTracks
                                             where track.PlaylistId == formattedPlaylist.Id
                                             where track.Trackname == name
                                             select track).First();
                    if (postedTrackInDate != null)
                    {
                        postedTrackInDate.Id = postedTrackInDate.Id;
                        postedTrackInDate.PlaylistId = postedTrackInDate.PlaylistId;
                        postedTrackInDate.Trackname = postedTrackInDate.Trackname;
                        postedTrackInDate.Date = publish_date;
                        postedTrackInDate.MediaId = postedTrackInDate.MediaId;
                        postedTrackInDate.OwnerId = postedTrackInDate.OwnerId;
                        postedTrackInDate.PostId = postId;
                        context.SaveChanges();
                        //context.Update(postedTrackInDate);
                        //context.PostedTracks.UpdateRange();
                    }
                }
            }catch (Exception ex) { Console.WriteLine(ex); }

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
                catch (Exception ex) { }
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
                } catch (Exception ex) { }
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
            List<string> unfoundTracksNames = new List<string>();

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
            List<string> postedTracksNames = new List<string>();
      
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
            List<string> urls = new List<string>();
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
            ParserXpath xPath = new ParserXpath();

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
        /// 
        /// </summary>
        /// <param name="checkLastPostedInVK"></param>
        /// <returns></returns>
        public static (DateTime, DateTime) GetLastDateFromDB(bool checkLastPostedInVK = false)
        {
            vkaudioposter_ef.parser.PostedTrack lastPostedTrack = null;
            DateTime publication_date = DateTime.Now;

            if (checkLastPostedInVK)
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
                } catch(System.InvalidOperationException ex)
                {
                    Console.WriteLine($"{ex.Message}");                
                }
            }

            DateTime LastDatePosted = new DateTime();
            //Возможна ошибка когда последняя дата раньше чем сейчас
            try
            {
                LastDatePosted = publication_date;
                publication_date = publication_date.AddHours(vkaudioposter_Console.Program.hoursperiod);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Дата отложенной публикации: {publication_date}");
                Console.WriteLine($"Дата публикации из БАЗЫ ДАННЫХ: {LastDatePosted}");
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
