using Rollbar;
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
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(Program.rollbarToken));

            if (Program.saveLogs == true)
            {
                string strPath = "Log.txt";

                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }

                FileInfo file = new(strPath);
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
                RollbarLocator.RollbarInstance
                .Error(ex);

                //var api = new VkApi();

                //api.Authorize(new ApiAuthParams
                //{
                //    AccessToken = Program.kateMobileToken
                //});

                //api.Messages.SendAsync(new MessagesSendParams
                //{
                //    UserId = Program.adminID,
                //    Message = ex.Message,
                //    RandomId = DateTime.Now.Millisecond
                //});

            }
            catch (Exception exVK) { ErrorLogging(exVK); }
        }

        public static void ErrorLogging(string ex)
        {
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(Program.rollbarToken));
            if (Program.saveLogs == true)
            {
                string strPath = "Log.txt";
                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }

                FileInfo file = new(strPath);
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

            RollbarLocator.RollbarInstance
                .Error(ex);

            //var api = new VkApi();

            //api.Authorize(new ApiAuthParams
            //{
            //    AccessToken = Program.kateMobileToken
            //});

            //api.Messages.SendAsync(new MessagesSendParams
            //{
            //    UserId = Program.adminID,
            //    Message = ex,
            //    RandomId = DateTime.Now.Millisecond
            //});

        }
        public static void ReadError()
        {
            if (Program.saveLogs == true)
            {
                string strPath = "Log.txt";
                using StreamReader sr = new(strPath);
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
