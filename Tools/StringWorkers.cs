﻿using System;
using System.IO;
using System.Text;

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


        //[ВСПОМОГАТЕЛЬНО] Получение перовго найденного ID трека из поиска [Содержится в MakeUrlFromLines] v/2
        //Переписать под v3: "audio_id":"371745438_456430000" перед найденным названием, должны прочитать до совпадения и найти айди
        public static string GetFullIdFromString(string SearchingName, string json)
        {

            string s = json;//файл json ответа
            string subs2 = "0";

            //Поиск названия по алгоритму Левенштейна (сравнение расстояний между строками - примерный поиск)
            string[] charsToRemove = new string[] { ",", "\u0022", "\\", "//" }; //удаляем лишние символы и заменяем на пробелы или пустоту
            foreach (string c in charsToRemove)
            {
                s = s.Replace(c, string.Empty);
            }
            int diff = Tools.Metrics.LevenshteinDistance(SearchingName, s); //Получение расстояния между строками исходной и результатов

            int errind = -1;
            int errind2 = -1;
            errind = s.IndexOf("error: audio_search"); //ошибка поиска может вылететь, проверка
            errind2 = s.IndexOf("no key"); //ошибка поиска может вылететь, проверка


            //if ((diff != -1) && (diff <= 1000) && (diff > 35) && (diff != 0) && (errind == -1) && (errind2 == -1)) //проверка на пустоту и расстояние чтобы поиск был точным (расхождение символов)
            if ((diff != -1) && (errind == -1) && (errind2 == -1))
            {
                try
                {
                    //string subsname = s.Remove(0, diff); //удалили все что до этого найденного совпадения

                    //DEBUG
                    //Console.WriteLine("Удаление до совпадения= " + subsname);

                    if (s.IndexOf("audio_id") != -1)//пока не дошли до индекса первого найденного слова content_id (v3)
                                                    //if (s.IndexOf("content_id") != -1)//пока не дошли до индекса первого найденного слова content_id (OLD)
                    {
                        //int ind = s.IndexOf("content_id"); //позиция content id
                        int ind = s.IndexOf("audio_id");
                        //string subs = s.Remove(0, ind + 11); //сместились, удалили все что до цифр, включая content_id это 15 элементов с кракозябрами
                        string subs = s.Remove(0, ind + 9);

                        //DEBUG
                        //Console.WriteLine("Айди+все что далее= " + subs);

                        //int ind2 = subs.IndexOf("s:"); //нашли символ "\" после цифр
                        int ind2 = subs.IndexOf(" artist");
                        subs2 = subs.Substring(0, ind2); //вычли подстроку до символа "\" 
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Чистый айди= " + subs2);

                        //FullId = subs2;
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                    Logging.ErrorLogging(ex);
                    //ReadError();
                }

            }

            return subs2;
        }


    }
}
