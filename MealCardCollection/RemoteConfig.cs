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
using System.Text.RegularExpressions;
namespace MealCardCollection
{
    public partial class RemoteConfig : Form
    {
        public RemoteConfig()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsValidate())
            {
                if (Regex.IsMatch(ipTbx.Text.Trim(), @"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))"))
                {
                    SaveConfig();
                    MessageBox.Show("保存成功！");
                    Close();
                }
                else
                {
                    MessageBox.Show("请输入正确的IP地址格式");
                }
            }
            else
            {
                MessageBox.Show("配置内容不能为空！");
            }
        }

        private void RemoteConfig_Load(object sender, EventArgs e)
        {
            InitRemoteConfig();
            RemoteEnableAsync();//异步判断socket的可连接性
        }
        private void InitRemoteConfig()
        {
            RemoteConfigController configController = new RemoteConfigController();
            string[] temp = configController.ReadRemoteConfig();
            ipTbx.Text = temp[0];
            portTbx.Text = temp[1];
            sidTbx.Text = temp[2];
        }
        private bool IsValidate()
        {
            if (ipTbx.Text == "" | portTbx.Text == "" | sidTbx.Text == "")
            {
                return false;
            }
            return true;                                   
        }
        private void SaveConfig()
        {
            RemoteConfigController configController = new RemoteConfigController();
            string[] temp = { ipTbx.Text.Trim(),portTbx.Text.Trim(),sidTbx.Text.Trim()};
            configController.SaveRemoteConfig(temp);
        }
        private async Task RemoteEnableAsync()
        {
            RemoteConfigController configController = new RemoteConfigController();
            bool flag = await Task.Run(() => configController.RemoteEnable());
            if (flag)
            {
                toolStripStatusLabel2.Text = "网络可连接！";
                toolStripStatusLabel2.ForeColor = Color.ForestGreen;
            }
            else
            {
                toolStripStatusLabel2.Text = "网络不可连接！";
                toolStripStatusLabel2.ForeColor = Color.Red;
            }
        }
    }
}
