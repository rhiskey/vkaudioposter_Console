using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using vkaudioposter_Console.Tools;
using vkaudioposter_Console.VKUtills;
using static vkaudioposter_Console.Program;
//using vkaudioposter_Console.Tools;
//using static vkaudioposter_Console.Program;

namespace vkaudioposter_Console.API
{
    public class PublishTrack
    {
        public string Name { get; set; }
        public string Style { get; set; }
        public DateTime Date { get; set; }

        public PublishTrack(string n, string s, DateTime d)
        {
            this.Name = n;
            this.Style = s;
            this.Date = d;
        }

    }

    public class ErrorLog
    {
        public string StackTrace { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public ErrorLog(string st, string m, DateTime d)
        {
            this.StackTrace = st;
            this.Message = m;
            this.Date = d;
        }

    }

    public enum Severity
    {
        info,
        warning,
        error
    }

    public class Rabbit
    {
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Program.random.Next(s.Length)]).ToArray());
        }


        public static void CommandsReciever()
        {
            DotNetEnv.Env.Load();
            string rabbitHost = DotNetEnv.Env.GetString("RABBIT_HOST");

            var factory = new ConnectionFactory() { HostName = rabbitHost };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "hvm_commands",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0} from {1}", message, sender);

                if (message == "start")
                {
                    Program.threadstopflag = false;
                    StatusChecker.ApiStart();
                    //Program.SendTestTrackMessages(); //DEMO 
                }
                if (message == "stop")
                {
                    Program.threadstopflag = true;
                }

                if (message == "postponed")
                {
                    var ppCount = VkTools.CheckPostponedAndGetCount();
                    // Send to rabbit message with count of postponed or get from database
                    //write to redis
                    //Redis.WriteCountToRedis(ppCount);

                }

                Console.WriteLine(" [x] Done");

                // Note: it is possible to access the channel via
                //       ((EventingBasicConsumer)sender).Model here
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            channel.BasicConsume(queue: "hvm_commands",
                                 autoAck: false,
                                 consumer: consumer);

            //Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        public static void NewPostedTrack(string trackname, string style, DateTime publDate)
        {
            try
            {
                DotNetEnv.Env.Load();
                string rabbitHost = DotNetEnv.Env.GetString("RABBIT_HOST");

                var factory = new ConnectionFactory() { HostName = rabbitHost };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                channel.QueueDeclare(queue: "posted_tracks",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                PublishTrack pT = new PublishTrack(trackname, style, publDate);
                string jsonString;
                jsonString = JsonSerializer.Serialize(pT);

                //var message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(jsonString);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: "posted_tracks",
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", jsonString);

                //Console.WriteLine(" Press [enter] to exit.");
                //Console.ReadLine();
            }
            catch (Exception ex) { Logging.ErrorLogging(ex); }
        }

        /// <summary>
        /// Логи по разным каналам RabbitMQ, канал важности выбирается в соответствии с параметром sever = 0 - инфо, 1 - warning, 2 - error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sever"></param>
        public static void NewLog(string message, int sever = 0)
        {
            try
            {
                DotNetEnv.Env.Load();
                string rabbitHost = DotNetEnv.Env.GetString("RABBIT_HOST");
                var factory = new ConnectionFactory() { HostName = rabbitHost };

                Severity info;

                switch (sever)
                {
                    case 0:
                        info = 0;
                        break;
                    case 1:
                        info = (Severity)1;
                        break;
                    case 2:
                        info = (Severity)2;
                        break;
                    default:
                        info = 0;
                        break;
                }

                string dateTimeNow = DateTime.Now.ToString("G");
                message = "[" + dateTimeNow + "] " + message;

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: "direct_logs",
                                     type: "direct");

                var severity = info.ToString();
                //var severity = (args.Length > 0) ? args[0] : "info";
                //var message = (args.Length > 1)
                //              ? string.Join(" ", args.Skip(1).ToArray())
                //              : "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "direct_logs",
                                     routingKey: severity,
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
            }
            catch (Exception ex) { Logging.ErrorLogging(ex); }
        }

    }

    //public static class Receive
    //{
    //    public static void Main()
    //    {
    //        var factory = new ConnectionFactory() { HostName = "localhost" };
    //        using (var connection = factory.CreateConnection())
    //        {
    //            using (var channel = connection.CreateModel())
    //            {
    //                channel.QueueDeclare(queue: "hello",
    //                                     durable: false,
    //                                     exclusive: false,
    //                                     autoDelete: false,
    //                                     arguments: null);

    //                var consumer = new EventingBasicConsumer(channel);
    //                consumer.Received += (model, ea) =>
    //                {
    //                    var body = ea.Body.ToArray();
    //                    var message = Encoding.UTF8.GetString(body);
    //                    Console.WriteLine(" [x] Received {0}", message);
    //                };
    //                channel.BasicConsume(queue: "hello",
    //                                     autoAck: true,
    //                                     consumer: consumer);

    //                Console.WriteLine(" Press [enter] to exit.");
    //                Console.ReadLine();
    //            }
    //        }
    //    }
    //}
}
