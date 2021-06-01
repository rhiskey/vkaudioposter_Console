using MySql.Data.MySqlClient;
using System;

namespace vkaudioposter.MySQL
{
    class DBMySQLUtils
    {
        public static MySqlConnection
         GetDBConnection(string host, int port, string database, string username, string password)
        {

            String connString = "Server=" + host + ";Port=" + port + ";Database=" + database
                + ";Uid=" + username + ";Pwd=" + password;

            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }
    }
}
