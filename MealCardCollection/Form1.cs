using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MealCardCollection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ImageList imgList = new ImageList();

            imgList.ImageSize = new Size(1, 30);// 设置行高 20 //分别是宽和高  

            listView1.SmallImageList = imgList; //这里设置listView的SmallImageList ,用imgList将其撑大  
            this.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度  

            for (int i = 0; i < 10; i++)   //添加10行数据  
            {
                ListViewItem lvi = new ListViewItem();

                lvi.ImageIndex = i;     //通过与imageList绑定，显示imageList中第i项图标  

                lvi.Text = "subitem" + i;

                lvi.SubItems.Add("第2列,第" + i + "行");

                lvi.SubItems.Add("第3列,第" + i + "行");

                this.listView1.Items.Add(lvi);
            }

            this.listView1.EndUpdate();  //结束数据处理，UI界面一次性绘制。  
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
    }
}
