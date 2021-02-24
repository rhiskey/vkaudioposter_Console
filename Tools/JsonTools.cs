using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using vkaudioposter_Console.Tools;
using static vkaudioposter_Console.Classes.Music;

namespace vkaudioposter
{
    class JsonTools
    {

        //[ВСПОМОГАТЕЛЬНО]Скачивание результата запроса поиска [Содержится в MakeUrlFromLines]
        public static string SearchWeb(string url)
        {
            string json, response;
            using (WebClient webClient = new WebClient())
            {
                webClient.QueryString.Add("format", "json");
                try
                {
                    response = webClient.DownloadString(url);
                }
                catch (WebException ex)
                {
                    response = null;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Слишком много поисковых запросов! Повтори позднее");

                    string err = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd().ToString();
                    Logging.ErrorLogging(ex);
                    Logging.ReadError();
                    Console.WriteLine(err);

                    return response;
                }

                json = JsonConvert.SerializeObject(response);

            }
            return json;
        }


        //[ВСПОМОГАТЕЛЬНО] запрос поиска аудио [Содержится в MakeUrlFromLines]
        public static string ConcatSearchReq(string url1, string url2, string url3)
        {
            string url = url1 + url2 + url3;
            return (url);
        }

        public static List<string> AddListItemMethod(int i, List<Track> SearchingList, List<string> listBox1)
        {
            string lst1elem = i + 1 + ") " + SearchingList[i].GetTitle();
 
            listBox1.Add(lst1elem);
            return listBox1;

        }
    }
}
