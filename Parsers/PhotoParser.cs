﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using vkaudioposter_Console.Tools;
using vkaudioposter_ef.Model;

namespace vkaudioposter_Console.Parsers
{
    class PhotoParser
    {
        public static string DevianPageParser(HtmlAgilityPack.HtmlDocument doc, string container, int postcounter, ParserXpath pxp = null)
        {
            string url = null;
            try
            {
                container = container.Replace("\u0022", "\"");
                var nodes = doc.DocumentNode.SelectNodes(container); //Контейнер с картинками на странице
                List<string> urlList = new(nodes.Count); //Список ссылок на полные страницы фоток

                if (nodes != null)
                {
                    foreach (var node in nodes) //div[1]/div/div[2]/div/a
                    ///div[3]/div/div[2]/section/a  -смотрим дочерние блоки до фотки 
                    ///(из контейнера - разница между полным путем до фотки и тем что пришло в этот метож)
                    {
                        //*[@id="76516524"]/div/div[2]/div/div/div[2]/div (пришло в этот метод)
                        var nodestoadd = node.SelectNodes("//section/a[contains(@data-hook, 'deviation_link')]");
                        foreach (var nod in nodestoadd)
                            urlList.Add(nod.Attributes[1].Value); //node.SelectNodes("//div/a[contains(@data-hook, 'deviation_link')]")[0].Attributes[1].Value
                    }
                }

                url = urlList[postcounter - 1];

                HtmlWeb web_img = new();
                doc = web_img.Load(url);

                //if (pxp != null)
                //{
                //    string inner = pxp.XpathInner;
                //    url = doc.DocumentNode.SelectNodes(pxp.XpathInner)[0].Attributes[3].Value; //One photo direct link
                //}
                //else 
                url = doc.DocumentNode.SelectNodes("//*[@id=\"root\"]/main/div/div[1]/div[1]/div/div[2]/div[1]/div/img")[0].Attributes[3].Value; //Ссылка на 1 фото new
            }
            catch (Exception ex) { Logging.ErrorLogging(ex); Logging.ReadError(); }
            return url;
        }


        public static string PicjumboParser(HtmlAgilityPack.HtmlDocument doc, string container, int postcounter)
        {

            string url;
            var nodes = doc.DocumentNode.SelectNodes(container);
            List<string> urlList = new(nodes.Count);

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var nodestoadd = node.SelectNodes("//div[contains(@class, 'tri_img')]");
                    foreach (var nod in nodestoadd)
                    {
                        urlList.Add(nod.Attributes[2].Value);
                    }
                }
            }

            url = urlList[postcounter - 1];

            HtmlWeb web_img = new();
            doc = web_img.Load(url);

            url = doc.DocumentNode.SelectNodes("/html/body/div[4]/div/div[1]/article/div/div[1]/picture/a/img")[0].Attributes[3].Value;

            return url;
        }

        public static string PixabayParser(HtmlAgilityPack.HtmlDocument doc, string container, int postcounter)
        {
            string url;
            var nodes = doc.DocumentNode.SelectNodes(container);
            List<string> urlList = new(nodes.Count);

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var nodestoadd = node.SelectNodes("//div[contains(@class, 'item')]");
                    foreach (var nod in nodestoadd)
                    {
                        var fC = nod.FirstChild;

                        urlList.Add(fC.Attributes[0].Value);
                    }
                }
            }

            url = urlList[postcounter - 1];

            HtmlWeb web_img = new();
            doc = web_img.Load(url);

            url = doc.DocumentNode.SelectNodes("//*[@id=\"media_container\"]/img")[0].Attributes[2].Value;
            return url;
        }

    }
}
