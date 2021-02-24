using MihaZupan;
using System;
using System.IO;
using System.Net;
using vkaudioposter_Console.Classes;

namespace vkaudioposter_Console.Tools
{
    class ImageWorkers
    {
        public static void TempFile(string file)
        {
            File.Delete(file);

            FileStream stream = new FileStream(file, FileMode.CreateNew);
            stream.Close();
        }

        //Скачать фотку по ссылке в "название фото"
        public static bool DownloadImage(string photourl, string photofilename)
        {
            bool downloaded = false;
            //Change VPN Every N use times

            //Use VPN
            var proxy = new HttpToSocks5Proxy(TorProxy.Host, TorProxy.Port);

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    ///VPN
                    webClient.Proxy = proxy;
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

        public static string AddWatermark(string photofilename)
        {
            string watermarked = "watermarked_image.jpg";

        //    using (Image image = Image.FromFile(photofilename))
        //    using (Image watermarkImage = Image.FromFile("icon_hvm_300x300_50opc.png"))
        //    using (Graphics imageGraphics = Graphics.FromImage(image))
        //    using (TextureBrush watermarkBrush = new TextureBrush(watermarkImage))
        //    {
        //        int x = (image.Width / 2 - watermarkImage.Width / 2);
        //        int y = (image.Height / 2 - watermarkImage.Height / 2);
        //        watermarkBrush.TranslateTransform(x, y);

        //        imageGraphics.FillRectangle(watermarkBrush, new Rectangle(new Point(x, y), new Size(watermarkImage.Width + 1, watermarkImage.Height)));
        //        image.Save(watermarked);
        //    }
            return watermarked;
        }
    }
}
