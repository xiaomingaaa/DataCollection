using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MealCardCollection.Entity;
using MealCardCollection.Util;
namespace MealCardCollection.Controller
{
    class UploadController
    {
        public UploadController()
        {
            config = ConfigUtil.GetRemoteConfig();
        }
        private List<XfDataEntity> Query()
        {
            List<XfDataEntity> lists = new List<XfDataEntity>();
            string sqlText = "select top 20 * from dlc_upload where is_upload=0";
            DataTable table = SQLHelper.GetAllResult(sqlText);
            if (table.Rows.Count > 0)
            {
                foreach (DataRow dr in table.Rows)
                {
                    XfDataEntity dataEntity = new XfDataEntity(dr["info"]==null?"":dr["info"].ToString(),Convert.ToInt32(dr["id"]),dr["created"].ToString(),Convert.ToInt32(dr["is_upload"]));
                    lists.Add(dataEntity);
                }
            }
            return lists;
        }
        private void UpdateLocal(List<XfDataEntity> models)
        {
            if (models != null)
            {

                using (SqlConnection conn = new SqlConnection(SQLHelper.sqlconstring))
                {
                    SqlCommand com = new SqlCommand();

                    try
                    {
                        conn.Open();
                        SqlTransaction transaction = conn.BeginTransaction();
                        com.Transaction = transaction;
                        com.Connection = conn;
                        com.CommandType = CommandType.Text;

                        foreach (XfDataEntity temp in models)
                        {
                            string sqlText = "update dlc_upload set is_upload=1 where id=" + temp.Id;
                            com.CommandText = sqlText;
                            com.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        conn.Close();
                        Log.WriteError("更新数据时出现错误：" + ex.Message);
                    }

                }

            }
        }
        static RemoteConfigEntity config;
        public List<XfDataEntity> Send()
        {
            string ip = "123.206.45.159";
            int port = 45223;
            string sid = "56744";
            if (config != null)
            {
                ip = config.Remoteip;
                port = Convert.ToInt32(config.Remoteport);
                sid = config.Schoolid;
            }
            string pre = "hnzf55030687!...";
            string content =sid+ "\r\n";
            List<XfDataEntity> datas = Query();
            if (datas.Count>0)
            {
                foreach (XfDataEntity model in datas)
                {
                    if (model.Info != "")
                    {
                        content += model.Info + "\r\n";
                    }
                }
                content += "\r\n";
                string sendmsg = EncryptionUtil.Md5Encryption(pre + content) + "\r\n" + content;
                
                SocketUtil socket = new SocketUtil(ip, port);
                string recv = socket.SendMsg(sendmsg);

                UpdateLocal(datas);
                return datas;
            }
            return null;
        }

    }
}
