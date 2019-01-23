using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MealCardCollection.Entity;
using MealCardCollection.Util;
using System.Data;
using System.Net;
namespace MealCardCollection.Controller
{
    /// <summary>
    /// 处理消费数据的控制层类
    /// </summary>
    class HandleXfDataController
    {
        /**
         * 1.从消费机获取数据
         * 2.将消费数据存储到数据库
         * 3.使用Redis缓存做处理（暂时不启用）
         * 获取未上传的数据
         */
        
        public List<XfDataEntity> GetXfDatas()
        {
            List<XfDataEntity> datas = new List<XfDataEntity>();
            string sqlText = "select *  from dlc_upload where  created like '"+DateTime.Now.ToString("yyyy-MM-dd")+"%'  ";
            DataTable dataTable = new DataTable();
            dataTable = SQLHelper.GetAllResult(sqlText);
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow dr in dataTable.Rows)
                {
                    XfDataEntity xfdata = new XfDataEntity(dr["info"].ToString(),Convert.ToInt32(dr["id"]),dr["created"].ToString(),Convert.ToInt32(dr["is_upload"]));
                    datas.Add(xfdata);
                }
                
            }
            return datas;
        }
        MachineCtro dlcManage;
        public void ReceiveStart()
        {
            IPEndPoint point = new IPEndPoint(IPAddress.Any,Convert.ToInt32(ConfigUtil.GetLocalConfig().Machport));
            dlcManage = new MachineCtro(point);
            dlcManage.DataReceived += new MachineCtro.MachineDataReceived(DataReceived);
            dlcManage.MachineDumped += new MachineCtro.MachineDump(dlcsocket_MachineDumped);
        }
        private void dlcsocket_MachineDumped(string ip)
        {
            Log.WriteDumpData(ip);
        }
        private bool DataReceived(string data)
        {
            bool flag = false;
            try
            {
                //先保存接收到的原始数据
                Log.WriteLog(data);
                //string sqlstring = "Data Source=" + tbServer.Text + ";Database=" + tbDatabase.Text + ";User id=" + tbLogID.Text + ";PWD=" + tbLogPass.Text;
                string cardid = "", ip = "", datetime = "", occur = "", result = "", before = "", after = "", mdno = "", cdno = "", dealtype = "", macno = "", serno = "", status = "";
                string[] items = data.Split(',');
                /**
                 * datetime=20190114191341,ip=192.168.0.240,macno=240,serno=5200817092702010,
                 * result=15,cardx4=615126,status=0,dealtype=1,before=3000,after=2500,occur=500,
                 * mdno=141394,cdno=2,auto=0,bagtype=1,bag2bef=0,bag2aft=0,opcard=3,
                 * 127105,192.168.1.6,20180817182232,15,1,15280,200,15080,134307,793,0,5200817033000813,6
                   128392,192.168.1.6,20180817182239,15,1,10160,100,10060,134308,641,0,5200817033000813,6
                   3807,192.168.1.31,20180817182146,15,1,12100,200,11900,177866,966,0,5200817032900770,31
                   4378,192.168.1.31,20180817182149,15,1,13802,100,13702,177867,936,0,5200817032900770,31

                 */
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].IndexOf("cardid", 0) >= 0 || items[i].IndexOf("cardx", 0) >= 0)
                    {
                        cardid = items[i].Substring(7);
                    }
                    else if (items[i].IndexOf("ip", 0) >= 0)
                    {
                        ip = items[i].Substring(3);
                    }
                    else if (items[i].IndexOf("datetime", 0) >= 0)
                    {
                        datetime = items[i].Substring(9);
                    }
                    else if (items[i].IndexOf("occur", 0) >= 0)
                    {
                        occur = items[i].Substring(6);
                    }
                    else if (items[i].IndexOf("result", 0) >= 0)
                    {
                        result = items[i].Substring(7);
                    }
                    else if (items[i].IndexOf("before", 0) >= 0)
                    {
                        before = items[i].Substring(7);
                    }
                    else if (items[i].IndexOf("after", 0) >= 0)
                    {
                        after = items[i].Substring(6);
                    }
                    else if (items[i].IndexOf("event", 0) >= 0)
                    {
                        after = items[i].Substring(6);
                    }
                    else if (items[i].IndexOf("doorno", 0) >= 0)
                    {
                        after = items[i].Substring(7);
                    }
                    else if (items[i].IndexOf("readerno", 0) >= 0)
                    {
                        after = items[i].Substring(9);
                    }
                    else if (items[i].IndexOf("doorpass", 0) >= 0)
                    {
                        after = items[i].Substring(9);
                    }
                    else if (items[i].IndexOf("passtype", 0) >= 0)
                    {
                        after = items[i].Substring(9);
                    }
                    else if (items[i].IndexOf("realstatus", 0) >= 0)
                    {
                        after = items[i].Substring(11);
                    }
                    else if (items[i].IndexOf("workstatus", 0) >= 0)
                    {
                        after = items[i].Substring(11);
                    }
                    else if (items[i].IndexOf("mdno", 0) >= 0)
                    {
                        mdno = items[i].Substring(5);
                    }
                    else if (items[i].IndexOf("cdno", 0) >= 0)
                    {
                        cdno = items[i].Substring(5);
                    }
                    else if (items[i].IndexOf("dealtype", 0) >= 0)
                    {
                        dealtype = items[i].Substring(9);
                    }
                    else if (items[i].IndexOf("status", 0) >= 0)
                    {
                        status = items[i].Substring(7);
                    }
                    else if (items[i].IndexOf("macno", 0) >= 0)
                    {
                        macno = items[i].Substring(6);
                    }
                    else if (items[i].IndexOf("serno", 0) >= 0)
                    {
                        serno = items[i].Substring(6);
                    }
                }
                string presedure = string.Concat(new string[]
                    {
                        "exec sp_consumdatareceived '",
                        cardid,
                        "','",
                        ip,
                        "','",
                        datetime,
                        "',",
                        result,
                        ",",
                        dealtype,
                        ",",
                        before,
                        ",",
                        occur,
                        ",",
                        after,
                        ",",
                        mdno,
                        ",",
                        cdno,
                        ",",
                        status,
                        ",'",
                        serno,
                        "',",
                        macno
                    });
                object obj = SQLHelper.GetOneResult(presedure);
                string _result = "";
                if (obj != null)
                {
                    _result =Convert.ToString(obj);
                }
                if (_result == "1")
                {
                    string local_base_data_ = presedure.Replace("exec sp_consumdatareceived ", "");
                    string local_base_data = local_base_data_.Replace("'", "");
                    string insertText= string.Concat(new string[]
                    {
                            "insert dlc_upload (info,created,is_upload) values ('",
                            local_base_data,
                            "','",
                            DateTime.Now.ToString(),
                            "','0')"
                    });
                    SQLHelper.Update(insertText);
                    flag = true;
                }
                else if (_result == "2")
                {
                    flag = true;
                }
                else
                {
                    Log.WriteIgnoreData(presedure);
                    flag = false;
                }

                //string sendmsg = cardid + "," + ip + ","+datetime+","+result+","+dealtype+","+before+","+occur+","+after+","+mdno+","+cdno+","+status+","+serno+","+macno;
                //XfDataEntity dataEntity = new XfDataEntity(sendmsg,0,DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),0);
                //SaveToLocalDB(dataEntity);
                //string sqlText1 = "select empno,empname,deptname,kqtime from hr_employee where cardnum="+cardid;
                //DataTable table = SQLHelper.GetAllResult(sqlText1);
                //if (table.Rows.Count > 0)
                //{
                //    DataRow dr = table.Rows[0];
                //    string empno = dr["empno"].ToString();
                //    string empname = dr["empname"].ToString();
                //    string deptname = dr["deptname"].ToString();
                //    string kqtime = dr["kqtime"].ToString();
                //    string insertText = "insert into dlc_record_xf(empno,empname,deptname,phyid,ipaddr,skdate,sktime,premoney,xfmoney,aftmoney,xfcode,rtype,empuid,macuid,remark,logname,logtime,result,tstatus,snumber,macbh,xftime)";
                //}
                return flag;
                    
            }
            catch (Exception ex)
            {//接收到数据分析出错保存记录
                Log.WriteError("处理接收到的数据出现错误："+ex.Message);
                return false;
            }
        }
        private void SaveToLocalDB(XfDataEntity dataEntity)
        {
            //暂时不用，因为存在存储过程
            string sqlText =string.Format("insert into dlc_upload(info,created,is_upload) values('{0}','{1}',{2})",dataEntity.Info,dataEntity.Xftime,dataEntity.Isupload);
            int flag= SQLHelper.Update(sqlText);
            if (flag <= 0)
            {
                Log.WriteIgnoreData(dataEntity.Info);
            }
        }       
    }
}
