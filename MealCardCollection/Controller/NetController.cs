using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MealCardCollection.Entity;
using System.Data;
using MealCardCollection.Util;
namespace MealCardCollection.Controller
{
    class NetController
    {
        public List<NetConfig> GetDevsConfig()
        {
            List<NetConfig> list = new List<NetConfig>();
            string sqlText = "select ipaddr,macid from dlc_sys003 ";
            try
            {
                DataTable table = SQLHelper.GetAllResult(sqlText);
                foreach (DataRow dr in table.Rows)
                {
                    NetConfig config = new NetConfig(dr["ipaddr"].ToString(), Convert.ToInt32(dr["macid"]));
                    list.Add(config);
                }
            }
            catch (Exception e)
            {
                Log.WriteError("获取设备IP信息时出现错误："+e.Message);
            }
            
            return list;
        }
        public bool IsConnect(string ip)
        {
            bool flag = false;
            try
            {
                flag = NetUtil.ConnectEnable(ip);
            }
            catch (Exception e)
            {
                Log.WriteError("对设备IP检测时出现错误："+e.Message);
            }
            return flag;
        }
    }
}
