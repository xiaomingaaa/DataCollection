using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MealCardCollection.Entity;
using MealCardCollection.Controller;
using Comm;
namespace MealCardCollection
{
    public partial class ShowXfData : Form
    {
        public ShowXfData()
        {
            InitializeComponent();
        }

        private void ShowXfData_Load(object sender, EventArgs e)
        {
            UpdateXfData();
            
        }
        /// <summary>
        /// 异步处理刷新消费数据
        /// </summary>
        /// <returns></returns>
        private async Task UpdateXfData()
        {
            HandleXfDataController handle = new HandleXfDataController();
            List<XfDataEntity> datas =await Task.Run( ()=> handle.GetXfDatas());
            if (datas.Count > 0)
            {
                ImageList imgList = new ImageList();
                imgList.ImageSize = new Size(1, 30);// 设置行高 20 //分别是宽和高  
                listView1.SmallImageList = imgList; //这里设置listView的SmallImageList ,用imgList将其撑大  
                this.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
                int count = 1;
                foreach (XfDataEntity temp in datas)
                {
                    ListViewItem lvi = new ListViewItem();

                    //lvi.ImageIndex = i;     //通过与imageList绑定，显示imageList中第i项图标  
                    string info = temp.Info;
                    string time = temp.Xftime;
                    int isupload = temp.Isupload;
                    lvi.Text = count.ToString();
                    //45678 , 0.0.0.0 , 2018-09-05 16:50:23 , 15 , 1 , 37000 , 100 , 36900 ,0,0,0,0,29
                    string[] infos = info.Split(',');
                    string cardno = infos[0];
                    double money =Convert.ToDouble(infos[6])/100;
                    lvi.SubItems.Add(cardno);
                    lvi.SubItems.Add(money.ToString()+"元");
                    lvi.SubItems.Add(time);
                    lvi.SubItems.Add(isupload==0?"未上传":"已上传");
                    //lvi.SubItems.Add("第3列,第" + i + "行");

                    this.listView1.Items.Add(lvi);
                    count++;
                }
                this.listView1.EndUpdate();
            }
        }
        int count = 22;
        private void refress()
        {
            ListViewItem[] temp= listView1.Items.Find("2", false);
            int length = temp.Length;
            temp[0].SubItems[1].Text = count+"元";
            listView1.Refresh();
            count++;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //
            refress();
        }
    }
}
