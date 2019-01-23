using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MealCardCollection.Util;
using System.Net;
using System.Data;
using MealCardCollection.Entity;
namespace MealCardCollection.Controller
{
    class BlackNameController
    {
        private MachineCtro dlcCtro;
        public BlackNameController()
        {
            //对MachineCtro对象做初始化
            
            IPEndPoint point = new IPEndPoint(IPAddress.Parse(ConfigUtil.GetLocalConfig().LocalIP), 8007);
            this.dlcCtro = new MachineCtro(point);
        }
        private int BlackDownList(string ip)
        {
            
            string text = null;
            try
            {
                text = this.dlcCtro.GetTime(ip);
            }
            catch (Exception e)
            {
                Log.WriteError("错误：" + e.Message);
                return 0;
            }
            if (text == null)
            {
                Log.WriteError("text为空,机器出现故障" + ip);
                return 1;
            }
            int result=-2;
            try
            {
                MachineCtro.ListField[] lf = new MachineCtro.ListField[1];
                ///最近三天的挂失名单
                string sqlText = "select empno,empname,cardnum  from hr_blackname where DATEDIFF(day, logtime, GETDATE())<=20 and DATEDIFF(day, logtime, GETDATE())>=0";
                DataTable table = SQLHelper.GetAllResult(sqlText);
                if (table != null)
                {
                    int rowCount = table.Rows.Count;
                    if (rowCount > 0)
                    {
                        lf = new MachineCtro.ListField[rowCount];
                        for (int i = 0; i < rowCount; i++)
                        {
                            lf[i].cardid = Convert.ToInt64(table.Rows[i][2]);
                            lf[i].empno = Convert.ToString(table.Rows[i][0]);
                            lf[i].empname = Convert.ToString(table.Rows[i][1]);
                            lf[i].subsidyformat += "00000000000000";// +cbSubsidyType.SelectedIndex.ToString();
                            lf[i].startsubsidy = "20170101";// tbstartsubsidy.Text;
                            lf[i].endsubsidy = "20991231";// tbendsubsidy.Text;
                            lf[i].amountend = "20291231";// tbamountend.Text;
                            lf[i].blackcount = int.Parse("0");//tbBlackCount.Text);
                            lf[i].listtype = 1;   //默认为白名单，可以不写此行代码    
                        }
                        if (this.dlcCtro.DownList(ip, lf) == 0)
                        {
                            Log.WriteLog(DateTime.Now.ToString() + ":" + ip + "下载黑名单成功！");
                            result = 0;

                        }
                        else
                        {
                            Log.WriteLog(DateTime.Now.ToString() + ":" + ip + "下载黑名单失败！");
                            result = -1;
                        }
                    }
                    else
                    {
                        result = 0;//没有挂失名单
                    }
                }
            }
            catch (Exception ex)
            {
                //显示信息在textbox中
                Log.WriteError(string.Concat(new string[]
                {
                    DateTime.Now.ToString(),
                    ":",
                    ip,
                    "下载黑名单失败:",
                    ex.Message
                }));
                result = -1;
            }
            return result;
        }
        private List<NetConfig> GetIpAddrs()
        {
            List<NetConfig> list = new List<NetConfig>();
            string sqlText = "select ipaddr,macid from dlc_sys003";
            DataTable table = SQLHelper.GetAllResult(sqlText);
            if (table != null)
            {
                if (table.Rows.Count > 0)
                {
                    foreach (DataRow dr in table.Rows)
                    {
                        try
                        {
                            int macid = -1;
                            if (dr["macid"] is DBNull)
                            {
                                macid = -1;
                            }
                            else
                            {
                                macid = Convert.ToInt32(dr["macid"]);
                            }
                            NetConfig ip = new NetConfig(dr["ipaddr"].ToString(), macid);
                            list.Add(ip);
                        }
                        catch (Exception e)
                        {
                            Log.WriteError("黑名单控制器中出现获取IP列表错误："+e.Message);
                        }
                        
                    }
                }
            }
            return list;
        }
        public List<string> PushBlackName()
        {
            List<string> failed = new List<string>();
            List<NetConfig> ips = GetIpAddrs();
            if (ips.Count <= 0)
            {
                return failed;
            }
            foreach (NetConfig temp in ips)
            {
                if (temp.Macid == -1)
                {
                    failed.Add(temp.Ipaddr + ":" + temp.Macid);
                    continue;
                }
                string ip = temp.Ipaddr + ":";
                string macid = temp.Macid.ToString("x2").PadLeft(4,'0');
                ip = ip + macid;
                int result = BlackDownList(ip);
                if (result != 0)
                {
                    failed.Add(temp.Ipaddr+":"+temp.Macid);
                }
            }
            return failed;
        }
        public void SetTimes()
        {
            List<NetConfig> list = GetIpAddrs();
            foreach (NetConfig temp in list)
            {
                if (temp.Macid == -1)
                {
                    continue;//如果为-1则跳过本次循环
                }
                string ip = "";
                try
                {
                    ip = temp.Ipaddr + ":";
                    string macid = temp.Macid.ToString("x2").PadLeft(4, '0');
                    ip = ip + macid;
                    dlcCtro.SetTime(ip);
                }
                catch (Exception e)
                {
                    Log.WriteError(ip+":设置时间时出现错误(黑名单控制层)："+e.Message);
                }
            }
        }
    }
}
