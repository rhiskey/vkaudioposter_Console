using System;

namespace vkaudioposter_Console.Tools
{
    class StringWorkers
    {
        private static string curDate = DateTime.Now.ToShortDateString();

        public static string GetPostMessageFromStyle(string style)
        {
            string postMessage = null;
            if (style != null)
            {
                style = style.Replace("/", "");
                style = style.Replace("'", "");
                style = style.Replace("&", "n");

                style = style.Replace(" ", "_");
                style = style.Replace("-", "_");
                //style = style.Replace("/", "");
                //style = style.Replace(" ", " #");
                //Properties.Settings.Default.TextMessage;

                postMessage = /*"Genre: " +*/ "#" + style + "@hvmlabel"  /*+ "\nRelease: " + curDate*/;
                return postMessage;
            }
            else
            {
                postMessage = "Release: " + curDate + "\n#HighVolumeMusic #HVM @hvmlabel #top #edm #fresh #music #new #свежак #электронная #музыка #топ";
                return postMessage;
            }
        }



        //Переписать под v3: "audio_id":"371745438_456430000" перед найденным названием, должны прочитать до совпадения и найти айди
        /// <summary>
        /// [ВСПОМОГАТЕЛЬНО] Получение перовго найденного ID трека из поиска [Содержится в MakeUrlFromLines] v/2
        /// </summary>
        /// <param name="SearchingName"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string GetFullIdFromString(string SearchingName, string json)
        {
            string s = json;
            string subs2 = "0";

            string[] charsToRemove = new string[] { ",", "\u0022", "\\", "//" };
            foreach (string c in charsToRemove)
            {
                s = s.Replace(c, string.Empty);
            }
            int diff = Tools.Metrics.LevenshteinDistance(SearchingName, s); 

            int errind = -1;
            int errind2 = -1;
            errind = s.IndexOf("error: audio_search");
            errind2 = s.IndexOf("no key"); 
           
            if ((diff != -1) && (errind == -1) && (errind2 == -1))
            {
                try
                {

                    if (s.IndexOf("audio_id") != -1)
                    {
                        int ind = s.IndexOf("audio_id");
                        string subs = s.Remove(0, ind + 9);
                        int ind2 = subs.IndexOf(" artist");
                        subs2 = subs.Substring(0, ind2); 
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Чистый айди= " + subs2);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Logging.ErrorLogging(ex);
                }

            }

            return subs2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullId"></param>
        /// <returns>ownerId and mediaId</returns>
        public static (int, int) GetOwnIdAndMediaIdFromFullId(string fullId)
        {
            int ownId = 0, mediaId = 0;
            int lowSpaceIndex = fullId.IndexOf('_');
            if (Int32.TryParse(fullId.Substring(0, lowSpaceIndex), out int ownIdParsed))
            {
                ownId = ownIdParsed;
                Int32.TryParse(fullId.Remove(0, lowSpaceIndex + 1), out int mediaIdParsed);
                mediaId = mediaIdParsed;
            }
            else
            {
                Console.WriteLine($"Int32.TryParse could not parse '{fullId.Substring(0, lowSpaceIndex)}' to an int.");              
            }
            return (ownId, mediaId);
        }
    }
}
