using System;
using StackExchange.Redis;

namespace vkaudioposter_Console.API
{
    class Redis
    {
        public static void WriteCountToRedis(ulong count)
        {
            ///<summary>
            ///open a connection to Redis
            ///</summary>
            ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("" + Program.redisHost + ":" + Program.redisPort.ToString() + ",password=" + Program.redisPassword);
            IDatabase conn = muxer.GetDatabase();

            //Reading and writing data with StackExchange.Redis 
            conn.StringSet("ppcount", count);
            var postponed = conn.StringGet("ppcount");
            Console.WriteLine(postponed);
        }
    }
}
