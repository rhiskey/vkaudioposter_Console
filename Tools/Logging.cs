using System;
using System.IO;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace vkaudioposter_Console.Tools
{
    class Logging
    {
        public static void ErrorLogging(Exception ex)
        {
            if (Program.saveLogs == true)
            {
                string strPath = "Log.txt";

                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }

                FileInfo file = new FileInfo(strPath);
                if (file.Length > 5e+6)
                {
                    // Clear File
                    System.IO.File.WriteAllText(strPath, string.Empty);
                }

                using StreamWriter sw = File.AppendText(strPath);
                sw.WriteLine("=============Error Logging ===========");
                sw.WriteLine("===========Start============= " + DateTime.Now);
                sw.WriteLine("Error Message: " + ex.Message);
                sw.WriteLine("Stack Trace: " + ex.StackTrace);
                sw.WriteLine("===========End============= " + DateTime.Now);

                //Rabbit.NewError(ex.StackTrace, ex.Message, DateTime.Now);
            }
            else
            {
                Console.WriteLine("=============Error Logging ===========");
                Console.WriteLine("===========Start============= " + DateTime.Now);
                Console.WriteLine("Error Message: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                Console.WriteLine("===========End============= " + DateTime.Now);
            }

            try
            {

                var api = new VkApi();

                api.Authorize(new ApiAuthParams
                {
                    AccessToken = Program.kateMobileToken 
                });
  
                api.Messages.SendAsync(new MessagesSendParams
                {
                    UserId = Program.adminID,
                    Message = ex.Message,
                    RandomId = DateTime.Now.Millisecond
                });

                ///TODO:
                //Rabbit.NewLog(ex.Message, 2);
            }
            catch (Exception exVK) { ErrorLogging(exVK); }
        }

        public static void ErrorLogging(string ex)
        {
            if (Program.saveLogs == true)
            {
                string strPath = "Log.txt";
                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }

                FileInfo file = new FileInfo(strPath);
                if (file.Length > 5e+6)
                {
                    // Clear File
                    System.IO.File.WriteAllText(strPath, string.Empty);
                }

                using StreamWriter sw = File.AppendText(strPath);
                sw.WriteLine("=============Error Logging ===========");
                sw.WriteLine("===========Start============= " + DateTime.Now);
                sw.WriteLine("Error Message: " + ex);
                sw.WriteLine("===========End============= " + DateTime.Now);
            }
            else
            {
                Console.WriteLine("=============Error Logging ===========");
                Console.WriteLine("===========Start============= " + DateTime.Now);
                Console.WriteLine("Error Message: " + ex);
                Console.WriteLine("===========End============= " + DateTime.Now);
            }

            var api = new VkApi();

            api.Authorize(new ApiAuthParams
            {
                AccessToken = Program.kateMobileToken 
            });

            api.Messages.SendAsync(new MessagesSendParams
            {
                UserId = Program.adminID,
                Message = ex,
                RandomId = DateTime.Now.Millisecond
            });

            ///TODO:               
            //Rabbit.NewLog(ex, 2);
        }
        public static void ReadError()
        {
            if (Program.saveLogs == true)
            {
                string strPath = "Log.txt";
                using StreamReader sr = new StreamReader(strPath);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
            else return;
        }

    }
}
