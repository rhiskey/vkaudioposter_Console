using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
