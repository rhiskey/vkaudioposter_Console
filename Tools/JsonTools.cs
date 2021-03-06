﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using vkaudioposter_Console.Tools;

namespace vkaudioposter
{
    class JsonTools
    {

        /// <summary>
        /// [ВСПОМОГАТЕЛЬНО]Скачивание результата запроса поиска [Содержится в MakeUrlFromLines]
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string SearchWeb(string url)
        {
            string json, response;
            using (WebClient webClient = new())
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


        /// <summary>
        /// [ВСПОМОГАТЕЛЬНО] запрос поиска аудио [Содержится в MakeUrlFromLines]
        /// </summary>
        /// <param name="url1"></param>
        /// <param name="url2"></param>
        /// <param name="url3"></param>
        /// <returns></returns>
        public static string ConcatSearchReq(string url1, string url2, string url3)
        {
            StringBuilder sb = new("", 250);
            sb.Append(url1); sb.Append(url2); sb.Append(url3);
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="SearchingList"></param>
        /// <param name="listBox1"></param>
        /// <returns></returns>
        public static List<string> AddListItemMethod(int i, List<Track> SearchingList, List<string> listBox1)
        {
            string lst1elem = i + 1 + ") " + SearchingList[i].GetTitle();

            listBox1.Add(lst1elem);
            return listBox1;

        }
    }
}
