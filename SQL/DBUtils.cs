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
        public static Configuration GetConfigFromDb()
        {
            vkaudioposter_ef.Model.Configuration configs;

            using (var context = new vkaudioposter_ef.AppContext())
            {
                configs = context.Configurations.FirstOrDefault();
            }
            return configs;
        }

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
