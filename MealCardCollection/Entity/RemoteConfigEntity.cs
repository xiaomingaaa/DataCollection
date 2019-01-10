using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealCardCollection.Entity
{
    /// <summary>
    /// 远程服务器配置实体
    /// </summary>
    class RemoteConfigEntity
    {
        private string remoteip;
        private string remoteport;
        private string schoolid;
        public string Remoteip { get => remoteip; set => remoteip = value; }
        public string Remoteport { get => remoteport; set => remoteport = value; }
        public string Schoolid { get => schoolid; set => schoolid = value; }

        public RemoteConfigEntity(string ip,string port,string schoolid)
        {
            Remoteip = ip;
            Remoteport = port;
            Schoolid = schoolid;
        }
    }
}
