using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MealCardCollection.Util;
namespace MealCardCollection.Controller
{
    class LocalConfigController
    {
        public string[] ReadLocalConfig()
        {
            string[] values = new string[5];
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\config.ini";
            string[] keys = { "Server", "Database", "Username", "Password", "Machport" };
            for (int i = 0; i < keys.Length; i++)
            {
                values[i] = Readini.ReadIniFile("LocalConfig",keys[i],path);
            }
            return values;
        }
        public void SaveLocalConfig(string[] parms)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\config.ini";
            string[] keys = { "Server", "Database", "Username", "Password", "Machport" };
            for (int i = 0; i < parms.Length; i++)
            {
                Readini.WriteIniFile("LocalConfig",keys[i],parms[i],path);
            }
        }
    }
}
