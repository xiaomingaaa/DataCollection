using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealCardCollection.Entity
{
    /// <summary>
    /// 本地配置说明实体类
    /// </summary>
    class LocalConfigEntity
    {
        private string server;
        private string database;
        private string username;
        private string password;
        private string machport;
        private string localIP;
        public string Server { get => server; set => server = value; }
        public string Database { get => database; set => database = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Machport { get => machport; set => machport = value; }
        public string LocalIP { get => localIP; set => localIP = value; }

        public LocalConfigEntity(string server,string database,string username,string password,string machport,string localip)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
            Machport = machport;
            LocalIP = localip;
        }
        public override string ToString()
        {
            string sqlConn = string.Format(@"server={0};database={1};user={2};pwd={3}",Server,Database,Username,Password);
            return sqlConn;
        }
    }
}
