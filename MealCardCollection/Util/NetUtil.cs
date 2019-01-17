using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
namespace MealCardCollection.Util
{
    /// <summary>
    /// 网络测试工具类
    /// </summary>
    class NetUtil
    {
        /// <summary>
        /// 测试网络是否能够联通
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool ConnectEnable(string ip)
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(ip);
            if (!Regex.IsMatch(ip, @"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))"))
            {
                return false;
            }
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
