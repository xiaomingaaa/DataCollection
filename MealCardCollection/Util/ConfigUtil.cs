using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MealCardCollection.Entity;
namespace MealCardCollection.Util
{
    /// <summary>
    /// 读取配置文件的工具类
    /// </summary>
    class ConfigUtil
    {
        public static LocalConfigEntity GetLocalConfig()
        {
            string[] values = new string[5];
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\config.ini";
            string[] keys = { "Server", "Database", "Username", "Password", "Machport" };
            for (int i = 0; i < keys.Length; i++)
            {
                values[i] = Readini.ReadIniFile("LocalConfig", keys[i], path);
            }
            LocalConfigEntity local = new LocalConfigEntity(values[0],values[1],values[2],values[3],values[4]);
            return local;
        }
        public static RemoteConfigEntity GetRemoteConfig()
        {
            string[] values = new string[3];
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\config.ini";
            string[] keys = { "Ip", "Port", "Schoolid" };
            for (int i = 0; i < keys.Length; i++)
            {
                values[i] = Readini.ReadIniFile("RemoteConfig", keys[i], path);
            }
            RemoteConfigEntity remote = new RemoteConfigEntity(values[0], values[1], values[2]);
            return remote;
        }
    }
}
