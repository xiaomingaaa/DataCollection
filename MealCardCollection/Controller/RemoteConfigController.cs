using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MealCardCollection.Util;
using MealCardCollection.Entity;
namespace MealCardCollection.Controller
{
    class RemoteConfigController
    {
        public string[] ReadRemoteConfig()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\config.ini";
            string[] temp =new string[3];
            string[] keys = { "Ip", "Port", "Schoolid" };
            for (int i = 0; i < keys.Length; i++)
            {
                temp[i] = Readini.ReadIniFile("RemoteConfig",keys[i],path);
            }
            return temp;
        }
        public void SaveRemoteConfig(string[] parms)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\config.ini";
            string[] keys = { "Ip", "Port", "Schoolid" };
            for (int i = 0; i < keys.Length; i++)
            {
                Readini.WriteIniFile("RemoteConfig", keys[i],parms[i], path);
            }
        }
        public bool RemoteEnable()
        {
            RemoteConfigEntity remote = ConfigUtil.GetRemoteConfig();
            SocketUtil socket = new SocketUtil(remote.Remoteip,Convert.ToInt32(remote.Remoteport));
            try
            {
                if (socket.TestConnect())
                {
                    socket.DisConnected();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteError("网络连接错误！！IP:"+remote.Remoteip+"端口："+remote.Remoteport);
            }
            
            return false;
        }
    }
}
