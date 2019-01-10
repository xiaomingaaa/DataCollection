using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MealCardCollection.Controller;
namespace MealCardCollection
{
    public partial class LocalConfig : Form
    {
        public LocalConfig()
        {
            InitializeComponent();
        }

        private void LocalConfig_Load(object sender, EventArgs e)
        {
            InitLocalConfig();//初始化参数
        }
        /// <summary>
        /// 初始化配置
        /// </summary>
        private void InitLocalConfig()
        {
            LocalConfigController controller = new LocalConfigController();
            string[] temp = controller.ReadLocalConfig();
            serverTbx.Text = temp[0];
            dbnameTbx.Text = temp[1];
            userTbx.Text = temp[2];
            pwdTbx.Text = temp[3];
            machTbx.Text = temp[4];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsValidate())
            {
                SaveConfig();
                MessageBox.Show("保存成功！");
                Close();
            }
            else
                MessageBox.Show("配置内容不能为空！");
        }
        /// <summary>
        /// 验证格式
        /// </summary>
        /// <returns></returns>
        private bool IsValidate()
        {
            if (serverTbx.Text == "" | dbnameTbx.Text == "" | userTbx.Text == "" | pwdTbx.Text == "" | machTbx.Text == "")
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        private void SaveConfig()
        {
            LocalConfigController configController = new LocalConfigController();
            string[] temp = { serverTbx.Text.Trim(),dbnameTbx.Text.Trim(),userTbx.Text.Trim(),pwdTbx.Text.Trim(),machTbx.Text.Trim()};
            configController.SaveLocalConfig(temp);
        }
    }
}
