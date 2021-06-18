using MihaZupan;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using vkaudioposter_Console.Classes;

namespace vkaudioposter_Console.Tools
{
    class ImageWorkers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public static void TempFile(string file)
        {
            File.Delete(file);

            FileStream stream = new(file, FileMode.CreateNew);
            stream.Close();
        }

        /// <summary>
        /// Скачать фотку по ссылке в "название фото"
        /// </summary>
        /// <param name="photourl"></param>
        /// <param name="photofilename"></param>
        /// <returns></returns>
        public static bool DownloadImage(string photourl, string photofilename)
        {
            bool downloaded = false;

            using (WebClient webClient = new())
            {
                //Use VPN
                //HttpToSocks5Proxy proxy = new HttpToSocks5Proxy(TorProxy.Host, TorProxy.Port);
                try
                {
                    if (Program.useProxy == true)
                        webClient.Proxy = new HttpToSocks5Proxy(TorProxy.Host, TorProxy.Port);
                    webClient.DownloadFile(photourl, photofilename);
                    webClient.Dispose();
                    downloaded = true;
                }
                catch (Exception ex)
                {
                    downloaded = false;
                    Logging.ErrorLogging(ex);
                    Logging.ReadError();
                }

                ///without VPN
                //finally
                //{
                //    try
                //    {
                //        var webClient2 = new WebClient();
                //        webClient2.DownloadFile(photourl, photofilename);
                //        webClient.Dispose();
                //        downloaded = true;
                //    }
                //    catch (Exception ex)
                //    {
                //        downloaded = false;
                //        ErrorLogging(ex);
                //        ReadError();
                //        threadstopflag = true;
                //    }
                //}
            }

            return downloaded;
        }

    }
}
