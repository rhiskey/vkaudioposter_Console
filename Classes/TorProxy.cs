using System;
using System.Collections.Generic;
using System.Text;

namespace vkaudioposter_Console.Classes
{
    public static class TorProxy
    {

        public readonly static string Host = Program.torHost;

        public readonly static int Port = Program.torPort;
        //public readonly static int Port = 8118;

        public static string GetProxyString()
        {
            return Host + ":" + Port;
        }
    }
}
