using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using vkaudioposter_Console.Tools;

namespace vkaudioposter_Console.Parsers
{
    class PhotoParser
    {

        //private string PhotoParserAuto(string photostock, int max_photo, int i, string music_style, int pickedStockPage)
        //{
        //    int dig = i % max_photo; //Остаток от деления (кратность числу макс фото на странице стока)
        //    string url = null; //Прямая ссылка на фото
        //                       //find url photo
        //    HtmlAgilityPack.HtmlDocument doc = null;

        //    ///выбираем сток, соответствующий стилю, надо сопоставить в БД каждому стилю свой сток
        //    ///Пробуем найти страницу со стилем по tag
        //    ///

        //    //switch (music_style)
        //    //{
        //    //    case var someVal when new Regex(@"metal|(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //    //        photostock = "https://www.deviantart.com/tag/brutal";
        //    //        break;
        //    //    case var someVal when new Regex(@"trance|(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //    //        photostock = "https://www.deviantart.com/whats-hot/?q=trance";
        //    //        break;
        //    //    case var someVal when new Regex(@"adrenaline(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //    //        photostock = "https://www.deviantart.com/whats-hot/?q=adrenaline";
        //    //        break;
        //    //    case var someVal when new Regex(@"house(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //    //        photostock = "https://www.pexels.com/search/music/";
        //    //        break;
        //    //}
        //    //int pagecounter = photostockQueue.Peek().Page;

        //    changeStock = false;
        //    try
        //    {
        //        //скачиваем страницу
        //        try
        //        {
        //            HtmlWeb web = new HtmlWeb();
        //            doc = web.Load(photostock);
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorLogging(ex);
        //            ReadError();
        //        }
        //        //получение ссылки на фото
        //        string nodContainer = null;
        //        switch (photostock)
        //        {
        //            case var someVal when new Regex(@"https://www.deviantart.com/topic/(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //                nodContainer = "//*[@id=\"root\"]/div[1]/div/div/article/div/div/div[2]";//Контейнер с картинками на странице (последний grid)
        //                try
        //                {
        //                    url = DevianPageParser(doc, nodContainer, i);
        //                }
        //                catch (System.NullReferenceException ex)
        //                {
        //                    if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
        //                    {
        //                        //https://www.deviantart.com/?order=whats-hot&page=1
        //                        pickedStockPage++; //Перешли на след. страницу
        //                                           //Сбросить счётчик фото
        //                        postcounter = 1;
        //                        i = postcounter;
        //                        //StringBuilder photostock_SB = new StringBuilder(photostock);
        //                        //photostock_SB.Append(new string { "&page=" + pagecounter });
        //                        photostock += "&page=" + pickedStockPage;

        //                        //скачиваем страницу
        //                        try
        //                        {
        //                            photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });

        //                        }
        //                        catch (Exception ex1)
        //                        {
        //                            ErrorLogging(ex1);
        //                            ReadError();
        //                        }
        //                        finally { changeStock = true; }
        //                    }
        //                    else { changeStock = true; }
        //                }
        //                break;

        //            case "https://picjumbo.com/latest-free-stock-photos/":
        //                try
        //                {
        //                    url = doc.DocumentNode.SelectNodes("/html/body/div[5]/div/div[" + i + "]/a/picture/img")[0].Attributes[2].Value;
        //                }
        //                catch (System.NullReferenceException ex)
        //                {
        //                    if (pickedStockPage <= stockMaxPages)
        //                    {
        //                        pagecounter++;
        //                        postcounter = 1;
        //                        i = postcounter;
        //                        photostock += "page/" + pickedStockPage + "/";

        //                        //скачиваем страницу
        //                        try
        //                        {
        //                            photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });
        //                        }
        //                        catch (Exception ex1)
        //                        {
        //                            //MessageBox.Show(ex.Message);
        //                            ErrorLogging(ex1);
        //                            ReadError();
        //                            changeStock = true;

        //                        }
        //                        finally { changeStock = true; }
        //                    }
        //                    else { changeStock = true; }
        //                }

        //                break;
        //            case "https://picjumbo.com/":
        //                try
        //                {
        //                    url = doc.DocumentNode.SelectNodes("/html/body/div[6]/div/div[" + i + "]/a[1]/picture/img")[0].Attributes[2].Value;
        //                }
        //                catch (System.NullReferenceException ex)
        //                {
        //                    if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
        //                    {
        //                        pickedStockPage++;
        //                        postcounter = 1;
        //                        i = postcounter;
        //                        photostock += "page/" + pickedStockPage + "/";
        //                        //скачиваем страницу
        //                        try
        //                        {
        //                            photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });

        //                        }
        //                        catch (Exception ex1)
        //                        {
        //                            ErrorLogging(ex1);
        //                            ReadError();

        //                        }
        //                        finally { changeStock = true; }
        //                    }
        //                    else { changeStock = true; }

        //                }
        //                break;

        //            case "https://pixabay.com/ru/editors_choice":
        //                url = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[2]/div/div/div[" + i + "]/a/img")[0].Attributes[1].Value;
        //                break;

        //            ///----------- BEGIN DEPRECIATED (УСТАРЕЛО)-----------
        //            case var someVal when new Regex(@"https://www.deviantart.com/?order=whats-hot(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //                nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/section/div/div[2]/div[2]";//Контейнер с картинками на странице 
        //                try
        //                {
        //                    url = DevianPageParser(doc, nodContainer, i);
        //                }
        //                catch (System.NullReferenceException ex)
        //                {
        //                    if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
        //                    {
        //                        //Если нет фотки
        //                        pickedStockPage++; //Перешли на след. страницу
        //                        postcounter = 1;
        //                        i = postcounter;
        //                        photostock += "&page=" + pickedStockPage;
        //                        try
        //                        {
        //                            photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });
        //                        }
        //                        catch (Exception ex1)
        //                        {
        //                            ErrorLogging(ex1);
        //                            ReadError();
        //                        }
        //                        finally { changeStock = true; }
        //                    }
        //                    else { changeStock = true; }


        //                }
        //                break;

        //            case var someVal when new Regex(@"https://www.deviantart.com/tag/(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //                nodContainer = "//*[@id=\"root\"]/div[2]/div/div/div/div[2]/div[2]";
        //                try
        //                {
        //                    url = DevianPageParser(doc, nodContainer, i);
        //                }
        //                catch (System.NullReferenceException ex)
        //                {
        //                    if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
        //                    {
        //                        //Если нет фотки
        //                        pickedStockPage++; //Перешли на след. страницу
        //                        postcounter = 1;
        //                        i = postcounter;
        //                        photostock += "&page=" + pickedStockPage;
        //                        try
        //                        {
        //                            photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });
        //                        }
        //                        catch (Exception ex1)
        //                        {
        //                            ErrorLogging(ex1);
        //                            ReadError();
        //                        }
        //                        finally { changeStock = true; }
        //                    }
        //                    else { changeStock = true; }
        //                }
        //                break;

        //            case var someVal when new Regex(@"https://www.deviantart.com/search?q=(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //                nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[3]/div[2]";//Контейнер с картинками на странице
        //                url = DevianPageParser(doc, nodContainer, i);
        //                break;
        //            case "https://www.tumblr.com/search/cool+photo/recent":
        //                url = doc.DocumentNode.SelectNodes("//*[@id=\"search_posts\"]/article[" + i + "]/section[1]/div[1]/img")[0].Attributes[0].Value;
        //                break;
        //            case "https://www.stockvault.net/latest-photos/?li=3":
        //                string url1 = doc.DocumentNode.SelectNodes("//*[@id=\"flexgrid\"]/div[" + i + "]/a/img")[0].Attributes[0].Value;
        //                url = "https://www.stockvault.net" + url1;
        //                break;
        //            case "https://www.pexels.com/search/music/":
        //                url = doc.DocumentNode.SelectNodes("/html/body/div[1]/div[3]/div[3]/div[1]/div[" + i + "]/article/a[1]/img")[0].Attributes[5].Value;
        //                break;

        //            case "https://nos.twnsnd.co/":
        //                url = doc.DocumentNode.SelectNodes("//*[@id=\"posts\"]/div/article[" + i + "]/div/section[1]/figure/div/div/a/img")[0].Attributes[1].Value;
        //                break;
        //            case "https://littlevisuals.co/":
        //                url = doc.DocumentNode.SelectNodes("//*[@id=\"main\"]/article[" + i + 1 + "]/a/img")[0].Attributes[0].Value;
        //                break;

        //            case var someVal when new Regex(@"https://www.canstockphoto.com/stock-photo-images/(\w*)", RegexOptions.IgnoreCase).IsMatch(someVal):
        //                url = doc.DocumentNode.SelectNodes("/html/body/div[3]/div[1]/div/div/div[2]/section/article[" + i + "]/a/span[1]/img")[0].Attributes[5].Value;
        //                break;
        //            ///-----------END DEPRECIATED (УСТАРЕЛО)-----------

        //            case null:
        //                // Do nothing for a null.
        //                break;
        //            default:
        //                Console.ForegroundColor = ConsoleColor.Red;
        //                Console.WriteLine("Null passed to this method.\n No SWITCH IN STOCK");
        //                break;
        //        }


        //        ///-----------BEGIN DEPRECIATED (УСТАРЕЛО)-----------

        //        if ((photostock == "https://pixabay.com/ru/") || (photostock == "https://pixabay.com/ru/photos/"))
        //        {
        //            url = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div[2]/div[1]/div[1]/div[" + i + "]/a/img")[0].Attributes[2].Value;
        //        }

        //        if ((photostock == "https://pixabay.com/images/search/?order=latest") || (photostock == "https://pixabay.com/images/search/"))
        //        {
        //            nodContainer = "//*[@id=\"content\"]/div/div[2]/div/div[2]/div[" + i + "]/a/img";
        //            url = PixabayParser(doc, nodContainer, i);
        //        }

        //        if (photostock == "https://www.deviantart.com/")
        //        {
        //            nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[2]/div[2]";//Контейнер с картинками на странице 
        //            url = DevianPageParser(doc, nodContainer, i);

        //        }

        //        if ((photostock == "https://www.deviantart.com/search/deviations?order=most-recent&q=wallpaper") ||
        //            (photostock == "https://www.deviantart.com/?order=whats-hot") ||
        //            (photostock == "https://www.deviantart.com/search/deviations?q=wallpaper&order=whats-hot"))
        //        {
        //            //24 фото на странице
        //            nodContainer = "//*[@id=\"root\"]/div[2]/div[2]/div/div/div[2]/div[2]/div";//Контейнер с картинками на странице
        //            try
        //            {
        //                url = DevianPageParser(doc, nodContainer, i);
        //            }
        //            catch (System.NullReferenceException ex)
        //            {
        //                if (pickedStockPage <= stockMaxPages) //Чтобы не было парсинга только этого сайта, загрузим только первые 5 страниц
        //                {
        //                    pickedStockPage++; //Перешли на след. страницу
        //                    postcounter = 1;
        //                    i = postcounter;
        //                    switch (photostock)
        //                    {
        //                        case "https://www.deviantart.com/?order=whats-hot":
        //                            photostock = "https://www.deviantart.com/search/deviations?order=whats-hot&page=" + pickedStockPage;
        //                            break;
        //                        case "https://www.deviantart.com/search/deviations?order=most-recent&q=wallpaper":
        //                            photostock = "https://www.deviantart.com/search/deviations?order=most-recent&page=" + pickedStockPage + "&q=wallpaper";
        //                            break;
        //                        case "https://www.deviantart.com/search/deviations?q=wallpaper&order=whats-hot":
        //                            photostock = "https://www.deviantart.com/search/deviations?order=whats-hot&page=" + pickedStockPage + "&q=wallpaper";
        //                            break;
        //                    }
        //                    //скачиваем страницу
        //                    try
        //                    {
        //                        photostockQueue.Enqueue(new Photostock_class { URL = photostock, Page = pickedStockPage });

        //                    }
        //                    catch (Exception ex1)
        //                    {

        //                        ErrorLogging(ex1);
        //                        ReadError();
        //                    }
        //                    finally { changeStock = true; }
        //                }
        //                else { changeStock = true; }

        //            }

        //        }


        //        if ((photostock == "https://www.deviantart.com/luckynumber113/favourites/76516524/Pretty-Wallpapers-Backgrounds-and-Designs") ||
        //            (photostock == "https://www.deviantart.com/james-is-james/favourites/60347448/Abstract") ||
        //            (photostock == "https://www.deviantart.com/psychospartanex/favourites/77875659/Amazing-Wallpapers"))
        //        {

        //            nodContainer = "//*[@id=sub-folder-gallery]/div/div[2]/div/div/div[2]/div";//Контейнер с картинками на странице (почти крайний блок)
        //            url = DevianPageParser(doc, nodContainer, i);

        //        }

        //        //-------TEST-------
        //        if (photostock == "https://www.rawpixel.com/free-images?sort=curated&photo=1&premium=free&page=1")
        //        {
        //            url = doc.DocumentNode.SelectNodes("//*[@id=\"page\"]/div/main/div/section/div/figure[" + i + "]/a")[0].Attributes[0].Value;
        //        }

        //        ///-----------END DEPRECIATED (УСТАРЕЛО)-----------

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogging(ex);
        //        ReadError();
        //        changeStock = true;
        //        //Default Image
        //        url = "https://sun9-21.userapi.com/c626627/v626627659/5c68f/DWvyDcKOAJI.jpg";
        //    }
        //    return url;
        //}

        // Parser of Devian. На вход номер фото, на выходе ссылка на фото на сайте 
        public static string DevianPageParser(HtmlAgilityPack.HtmlDocument doc, string container, int postcounter)
        {
            string url = null;
            try
            {
                //container = //*[@id="root"]/main/div/div[1]/div[1]/div/div[2]/div[1]/img
                var nodes = doc.DocumentNode.SelectNodes(container); //Контейнер с картинками на странице
                List<string> urlList = new List<string>(nodes.Count); //Список ссылок на полные страницы фоток

                if (nodes != null)
                {
                    foreach (var node in nodes) //div[1]/div/div[2]/div/a
                    ///div[3]/div/div[2]/section/a  -смотрим дочерние блоки до фотки 
                    ///(из контейнера - разница между полным путем до фотки и тем что пришло в этот метож)
                    {
                        //*[@id="76516524"]/div/div[2]/div/div/div[2]/div (пришло в этот метод)
                        //var nodestoadd = node.SelectNodes("//div/a[contains(@data-hook, 'deviation_link')]");
                        var nodestoadd = node.SelectNodes("//section/a[contains(@data-hook, 'deviation_link')]");
                        foreach (var nod in nodestoadd)
                            urlList.Add(nod.Attributes[1].Value); //node.SelectNodes("//div/a[contains(@data-hook, 'deviation_link')]")[0].Attributes[1].Value
                    }
                }

                url = urlList[postcounter - 1];
                //Open url
                HtmlWeb web_img = new HtmlWeb();
                doc = web_img.Load(url);

                url = doc.DocumentNode.SelectNodes("//*[@id=\"root\"]/main/div/div[1]/div[1]/div/div[2]/div[1]/div/img")[0].Attributes[3].Value; //Ссылка на 1 фото new
            }
            catch (Exception ex) { Logging.ErrorLogging(ex); Logging.ReadError(); }
            return url;
        }


        public static string PicjumboParser(HtmlAgilityPack.HtmlDocument doc, string container, int postcounter)
        {
            //Вход /html/body/div[5]/div
            //doc.DocumentNode.SelectNodes("/html/body/div[5]/div/div[" + i + "]/a/picture/img")[0].Attributes[2].Value;

            string url = null;
            var nodes = doc.DocumentNode.SelectNodes(container); //Контейнер с картинками на странице
            List<string> urlList = new List<string>(nodes.Count); //Список ссылок на полные страницы фоток

            if (nodes != null)
            {
                foreach (var node in nodes) // html / body / div[5]
                { //html/body/div[5]/div/div[4]  /html/body/div[5]/div/div[8]/a[1]
                    var nodestoadd = node.SelectNodes("//div[contains(@class, 'tri_img')]"); //Перебор по class="tri_img_one masonry_item"
                    foreach (var nod in nodestoadd)
                    {
                        //var child = nod.ChildNodes;
                        //var img = child.SelectNodes("//div[contains(@class, 'tri_img')]");// class="tri_img"
                        urlList.Add(nod.Attributes[2].Value);
                    }
                }
            }

            url = urlList[postcounter - 1];
            //Open url
            HtmlWeb web_img = new HtmlWeb();
            doc = web_img.Load(url);
            //Find 
            url = doc.DocumentNode.SelectNodes("/html/body/div[4]/div/div[1]/article/div/div[1]/picture/a/img")[0].Attributes[3].Value; //Ссылка на 1 фото
            //string title = doc.DocumentNode.SelectNodes("/html/body/div[4]/div/div[1]/article/div/div[1]/picture/a/img")[0].Attributes[4].Value;

            return url;
        }

        public static string PixabayParser(HtmlAgilityPack.HtmlDocument doc, string container, int postcounter)
        {

            string url = null;
            var nodes = doc.DocumentNode.SelectNodes(container); //Контейнер с картинками на странице
            List<string> urlList = new List<string>(nodes.Count); //Список ссылок на полные страницы фоток

            if (nodes != null)
            {
                foreach (var node in nodes) // html / body / div[5]
                { //*[@id="content"]/div/div[2]/div/div[2]     /div[7]/a
                    var nodestoadd = node.SelectNodes("//div[contains(@class, 'item')]");
                    foreach (var nod in nodestoadd)
                    {//*[@id="content"]/div/div[2]/div/div[2]/div[7]/a
                        var fC = nod.FirstChild;
                        //var child = nod.ChildNodes;
                        //var img = child.SelectNodes("//div[contains(@class, 'tri_img')]");// class="tri_img"
                        urlList.Add(fC.Attributes[0].Value);
                    }
                }
            }

            url = urlList[postcounter - 1];
            //Open url
            HtmlWeb web_img = new HtmlWeb();
            doc = web_img.Load(url);
            //Find 
            url = doc.DocumentNode.SelectNodes("//*[@id=\"media_container\"]/img")[0].Attributes[2].Value; //Ссылка на 1 фото
            //string title = doc.DocumentNode.SelectNodes("/html/body/div[4]/div/div[1]/article/div/div[1]/picture/a/img")[0].Attributes[4].Value;

            return url;
        }

    }
}
