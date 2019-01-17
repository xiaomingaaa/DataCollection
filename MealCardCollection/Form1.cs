using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using MealCardCollection.Entity;
using MealCardCollection.Controller;
namespace MealCardCollection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 30);// 设置行高 20 //分别是宽和高  
            listView1.SmallImageList = imgList; //这里设置listView的SmallImageList ,用imgList将其撑大
        }
        Thread thread = null;//更新listView
        Thread receiveThread = null;//处理消费数据使用
        Thread sendThread = null;//发送消费数据
        private void Form1_Load(object sender, EventArgs e)
        {
            thread = new Thread(RefreshData);//刷新ListView
            thread.Start();
            receiveThread = new Thread(StartReceive);//接受消费数据
            receiveThread.Start();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            LocalConfig local = new LocalConfig();
            local.ShowDialog();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            RemoteConfig remote = new RemoteConfig();
            remote.ShowDialog();
        }

        private void 挂失名单ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowXfData xf = new ShowXfData();
            xf.ShowDialog();
        }
        delegate void UpdateListView(string ip,int macid,bool isconn);
        private void RefreshListView(string ip,int macid,bool isconn)
        {
            //listView1.BeginUpdate();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[1].Text.Trim() == macid.ToString())
                {
                    if (isconn)
                    {
                        listView1.Items[i].SubItems[2].Text = "连通";
                        listView1.Items[i].SubItems[2].ForeColor = Color.Green;
                       // listView1.Items[i].SubItems[2].ForeColor = Color.Green;
                    }
                    else
                    {
                        listView1.Items[i].ForeColor = Color.Red;
                        listView1.Items[i].SubItems[2].Text = "不连通";
                    }
                return;
                }
                
            }
            ListViewItem listitem = new ListViewItem();
            Font f = new Font("宋体",(float)14.25,FontStyle.Regular);
            listitem.Font = f;
            listitem.Text = ip;
            listitem.SubItems.Add(macid.ToString());
            if (isconn)
            {
                listitem.SubItems.Add("连通");
                listitem.ForeColor = Color.Green;
                //listitem.SubItems["state"].ForeColor = Color.Green;
            }
            else
            {
                listitem.SubItems.Add("不连通");
                listitem.ForeColor = Color.Red;
                //listitem.SubItems["state"].ForeColor = Color.Red;
            }
            listView1.Items.Add(listitem);

            //listView1.EndUpdate();
        }
        private void RefreshDatas(string ip, int macid,bool isconn)
        {
            if (listView1.InvokeRequired)
            {
                UpdateListView update = new UpdateListView(RefreshListView);
                listView1.Invoke(update, ip, macid,isconn);
            }
            else
            {
                RefreshListView(ip, macid, isconn);//刷新listView
            }
        }
        private void RefreshData()
        {
            while (true)
            {
                NetController net = new NetController();
                List<NetConfig> list = net.GetDevsConfig();
                foreach (NetConfig temp in list)
                {
                    bool isconn = net.IsConnect(temp.Ipaddr);
                    RefreshDatas(temp.Ipaddr, temp.Macid, isconn);
                }
                Thread.Sleep(5000);//5秒更新一次
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (thread != null)
            {
                thread.Abort();
            }
            if (receiveThread != null)
            {
                receiveThread.Abort();
            }
            System.Environment.Exit(0);//彻底关闭进程
        }

        
        private void StartReceive()
        {
            HandleXfDataController handle = new HandleXfDataController();
            handle.ReceiveStart();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UploadController upload = new UploadController();
            upload.Send();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }
    }
}
