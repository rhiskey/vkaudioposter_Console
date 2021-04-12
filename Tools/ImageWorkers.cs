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

            FileStream stream = new FileStream(file, FileMode.CreateNew);
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

            //Use VPN
            var proxy = new HttpToSocks5Proxy(TorProxy.Host, TorProxy.Port);

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    ///VPN
                    if(Program.useProxy == true)
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

        public Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }

        public Image DrawTextOnImage(string bgPath, string text, Font font, Brush textColor)
        {
            Bitmap myBitmap = new Bitmap(bgPath);
            Graphics g = Graphics.FromImage(myBitmap);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            StringFormat strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Center;
            strFormat.LineAlignment = StringAlignment.Center;
            g.DrawString(text, font, textColor,
                new RectangleF(0, 0, 1280, 720), strFormat);

            Image bmp = new Bitmap(1280,720, g); //pass size from BG
            return bmp;
            //g.DrawString(text, font, textColor, new PointF(0, 0));
        }
    }
}
