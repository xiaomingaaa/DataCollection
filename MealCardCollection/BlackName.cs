using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MealCardCollection.Util;
using MealCardCollection.Entity;
namespace MealCardCollection
{
    public partial class BlackName : Form
    {
        public BlackName()
        {
            InitializeComponent();

        }
        private List<string> list;
        public BlackName(List<string> list)
        {
            InitializeComponent();
            this.list = list;
            
        }
        private void UpdateList()
        {
            toolStripStatusLabel2.Text = list.Count + "个";
            for (int j=0;j<list.Count;j++)
            {
                string[] ips =list[j].Split(':');
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].SubItems[0].Text.Trim() == ips[0])
                    {
                        listView1.Items[i].ForeColor = Color.Red;
                        listView1.Items[i].SubItems[2].Text = "不可接受指令";
                    }
                }
            }
            toolStripStatusLabel5.Text = DateTime.Now.ToString();
        }
        private void BlackName_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel5.Text = DateTime.Now.ToString();
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 30);// 设置行高 20 //分别是宽和高  
            listView1.SmallImageList = imgList; //这里设置listView的SmallImageList ,用imgList将其撑大
            List<NetConfig> list = GetIpAddrs();
            if (list.Count > 0)
            {
                foreach (NetConfig temp in list)
                {
                    ListViewItem listitem = new ListViewItem();
                    Font f = new Font("宋体", (float)12.5, FontStyle.Regular);
                    listitem.Font = f;
                    listitem.Text = temp.Ipaddr;
                    listitem.SubItems.Add(temp.Macid.ToString());
                    if (temp.Macid == -1)
                    {
                        listitem.SubItems.Add("缺少机号");
                        listView1.Items.Add(listitem).ForeColor = Color.Red;
                    }
                    else
                    {
                        listitem.SubItems.Add("可接受指令");
                        listView1.Items.Add(listitem).ForeColor = Color.Green;
                    }                    
                    
                }
                UpdateList();//更新列表
            }
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
                            Log.WriteError("黑名单控制器中出现获取IP列表错误：" + e.Message);
                        }

                    }
                }
            }
            return list;
        }
    }
}
