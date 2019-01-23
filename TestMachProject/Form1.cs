using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Timers;
namespace TestMachProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static IPEndPoint point = new IPEndPoint(IPAddress.Parse("172.18.1.238"), 8005);
        MachineCtro dlcManage ;
        MachineCtro dlcCtro;
        private void button1_Click(object sender, EventArgs e)
        {
            dlcManage = new MachineCtro(point);
            start();          
            SetValue("开始采集！");
        }
        void AutoCollect()
        {
            while (true)
            {
                SetValue("开始获取数据！！！！");
                //string[] records = dlcManage.GetRecords("192.168.0.240:240", 0, 10, 0, true);
                //string[] records1 = dlcManage.GetRecords("192.168.0.241:241", 0, 10, 0, false);
                int[] rs = dlcManage.GetRecordInfo("192.168.0.240|8005:0001");
                int[] rs2 = dlcManage.GetRecordInfo("192.168.0.241|8005:241");
                SetValue("240最大可存数:" + rs[0].ToString());
                SetValue("240已存记录数:" + rs[1].ToString());
                SetValue("241最大可存数:" + rs2[0].ToString());
                SetValue("241已存记录数:" + rs2[1].ToString());
                //if (records.Length <= 0)
                //{
                //    SetValue("240出现错误！\r\n");
                //}
                //if (records1.Length <= 0)
                //{
                //    SetValue("241出现错误！\r\n");
                //}
                //for (int i = 0; i < records.Length; i++)
                //{
                //    SetValue("240:" + records[i] + "\r\n");
                //}
                //for (int i = 0; i < records1.Length; i++)
                //{
                //    SetValue("241:" + records1[i] + "\r\n");
                //}
                Thread.Sleep(1000);//子线程睡眠1秒
            }
        }
        private int BlackDownList(string ip)
        {
            MessageBox.Show(ip);
            int text = 101;
            try
            {
                text = this.dlcManage.SetTime(ip);
            }
            catch (Exception e)
            {
                SetValue("错误：" + e.Message);
                SaveLog(Application.StartupPath + "\\errorlog.txt", "错误："+e.Message);
                return -1;
            }
            if (text != 0)
            {
                SetValue("错误：获取到的时间为空" + text);
                SaveLog(Application.StartupPath + "\\errorlog.txt", "错误：获取到的时间为空"+text);
                return 1;
            }
            int result;
            try
            {
                MachineCtro.ListField[] lf = new MachineCtro.ListField[1];
                try
                {
                    lf[0].cardid = Int64.Parse("615126");
                }
                catch
                { MessageBox.Show("卡号不可为空或字母"); return 1; }
                lf[0].empno = "615126";//tbEmpno.Text;
                lf[0].empname = "马腾飞";//tbEmpname.Text;
                                      //lf[0].subsidyformat = (int.Parse(tbSubsidyAmount.Text) * 100).ToString("X2").PadLeft(8, '0');
                                      //lf[0].subsidyformat += Convert.ToString(int.Parse(tbSubsidyno.Text), 16).PadLeft(4, '0');
                                      // lf[0].subsidyformat += "0"+ cbSubsidyType.SelectedIndex.ToString();
                lf[0].subsidyformat += "00000000000000";// +cbSubsidyType.SelectedIndex.ToString();
                lf[0].startsubsidy = "20170101";// tbstartsubsidy.Text;
                lf[0].endsubsidy = "20991231";// tbendsubsidy.Text;
                lf[0].amountend = "20191231";// tbamountend.Text;
                lf[0].blackcount = int.Parse("0");//tbBlackCount.Text);
                lf[0].listtype = 1;   //默认为白名单，可以不写此行代码
                if (this.dlcManage.DownList(ip, lf) == 0)
                {
                    SetValue(DateTime.Now.ToString() + ":" + ip + "下载黑名单成功！");
                    SaveLog(Application.StartupPath + "\\errorlog.txt", DateTime.Now.ToString() + ":" + ip + "下载黑名单成功！");
                    result = 0;
                }
                else
                {
                    SetValue(DateTime.Now.ToString() + ":" + ip + "下载黑名单失败！");
                    SaveLog(Application.StartupPath + "\\errorlog.txt", DateTime.Now.ToString() + ":" + ip + "下载黑名单失败！");
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                //显示信息在textbox中
                SetValue(string.Concat(new string[]
                {
                    DateTime.Now.ToString(),
                    ":",
                    ip,
                    "下载黑名单失败:",
                    ex.Message
                }));
                SaveLog(Application.StartupPath + "\\errorlog.txt", string.Concat(new string[]
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
        private void timer1_Tick(object sender, EventArgs e)
        {
            BlackDownList("192.168.0.241:0");
            
            BlackDownList("192.168.0.240:0001");
            //richTextBox1.Text += "开始获取数据！！！！\r\n";
            //timer1.Enabled = false;
            //richTextBox1.Text += "时钟开始";
            //string[] records = dlcManage.GetRecords("192.168.0.240|4001:240", 0, 10, 0, true);
            //string[] records1 = dlcManage.GetRecords("192.168.0.241|4001:241", 0, 10, 0, false);
            //int[] rs = dlcManage.GetRecordInfo("192.168.0.241|4001:240");
            //string a = "最大可存数:" + rs[0].ToString();
            //string b = "已存记录数:" + rs[1].ToString();
            ////MessageBox.Show(a + ":" + b);
            //if (records.Length <= 0)
            //{
            //    richTextBox1.Text+="240出现错误！\r\n";
            //}
            //if (records1.Length <= 0)
            //{
            //    richTextBox1.Text += "241出现错误！\r\n";
            //}
            //for (int i = 0; i < records.Length; i++)
            //{
            //    richTextBox1.Text += "240:" + records[i] + "\r\n";
            //}
            //for (int i = 0; i < records1.Length; i++)
            //{
            //    richTextBox1.Text += "241:" + records1[i] + "\r\n";
            //}

            //timer1.Enabled = true;
            //richTextBox1.Text += "时钟结束";
        }
        delegate void ChangeTextBox(object obj);

        void DelegateSetValue(object obj)
        {
            richTextBox1.Text += obj.ToString() + "\r\n";
        }
        void SetValue(object obj)
        {
            if (richTextBox1.InvokeRequired)
            {
                ChangeTextBox temp = new ChangeTextBox(DelegateSetValue);
                richTextBox1.Invoke(temp, obj);
            }
            else
            {
                richTextBox1.Text = obj.ToString()+"\r\n";
            }
        }

        //更新心跳包
        private void dlcsocket_MachineDumped(string ip)
        {

            DataModify(dataList1, ip);
        }
        delegate void DataModifyDelegate(Comm.DataList datalist, string ip);
        private void DataModify(Comm.DataList datalist, string ip)
        {
            SetValue("心跳数据："+ip);
            //if (datalist.InvokeRequired)
            //{
            //    DataModifyDelegate d = DataModify;
            //    datalist.Invoke(d, new object[] { datalist, ip });
            //}
            //else
            //{
            //    for (int row = 0; row < datalist.Rows.Count; row++)
            //    {
            //        if (datalist.Rows[row].Cells["ipaddr"].Value.ToString() == ip)
            //        {
            //            int count;
            //            if (datalist.Rows[row].Cells["dumpcount"].Value == null)
            //                count = 0;
            //            else
            //                count = int.Parse(datalist.Rows[row].Cells["dumpcount"].Value.ToString());
            //            count += 1;
            //            datalist.Rows[row].Cells["dumpcount"].Value = count;
            //            datalist.Rows[row].Cells["lastdump"].Value = DateTime.Now;
            //            return;
            //        }
            //    }
            //    int r = datalist.Rows.Add();
            //    datalist.Rows[r].Cells["ipaddr"].Value = ip;
            //    datalist.Rows[r].Cells["dumpcount"].Value = 1;
            //    datalist.Rows[r].Cells["lastdump"].Value = DateTime.Now;
            //}
        }
        private void dlcsocket_MachineLoged(string ip, int loged)
        {
        }  //ok
        delegate void SetItemValueDelegate(Comm.DataList datalist, string item, int row, string value);
        private void SetItemValue(Comm.DataList datalist, string item, int row, string value)
        {
            if (datalist.InvokeRequired)
            {
                SetItemValueDelegate d = SetItemValue;
                datalist.Invoke(d, new object[] { datalist, item, row, value });
            }
            else
            {
                if (row > datalist.Rows.Count - 1)
                {
                    datalist.AddRow();
                    datalist.FirstDisplayedScrollingRowIndex = row;
                }

                datalist.Rows[row].Cells[item].Value = value;
            }
        }
        //IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 8004);
        //MachineCtro dlcDown = null;
        void start()
        {
            dlcManage.MachineDumped += new MachineCtro.MachineDump(dlcsocket_MachineDumped);
            dlcManage.DataReceived += new MachineCtro.MachineDataReceived(DataReceived);
            dlcManage.MachineLoged += new MachineCtro.MachineLogoned(dlcsocket_MachineLoged);
            dlcManage.QueryApplyed += new MachineCtro.QueryApply(dlcsocket_QueryApplyed);
            dlcManage.ReceivedError += new MachineCtro.MachineDataReceivedError(ReceivedError);
            //dlcManage.PhotoReceivedX += this.dlcsocket_PhotoReceivedX;
            //dlcManage.CardBlashedIC += new MachineCtro.MachineCardBlashedIC(CardBlashedIC);
            initBlackTicker();
        }
        private System.Timers.Timer tDownList = new System.Timers.Timer();
        public void initBlackTicker()
        {
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("172.18.1.238"),8007);
            this.dlcCtro = new MachineCtro(point);
            this.tDownList.Enabled = true;
            this.tDownList.Interval = 5000.0;
            this.tDownList.Elapsed += new ElapsedEventHandler(this.BlacktickerBase);
        }
        public void BlacktickerBase(object sender, EventArgs e)
        {
            this.tDownList.Stop();
            this.tDownList.Enabled = false;
            this.tDownList_Tick();
            this.tDownList.Enabled = true;
            this.tDownList.Start();
        }
        private void tDownList_Tick()
        {
            string ipaddr = "172.18.1.111:0010";
            int num = this.DownBlackList(ipaddr);
            if (num == 1)
            {
                SetValue("机器16不可用"+ipaddr);
            }
            string ipaddr1 = "172.18.1.112:0011";
            int num1 = DownBlackList(ipaddr1);
            if (num1 == 1)
            {
                SetValue("机器17不可用" + ipaddr1);
            }
        }
    private int DownBlackList(string ip)
    {
            int flag = -2;
            flag = dlcCtro.SetTime(ip);
            SetValue(flag.ToString());
        string text = null;
        try
        {
            text = this.dlcCtro.GetTime(ip);
        }
        catch (Exception e)
        {
                SetValue("错误："+e.Message);
            return 0;
        }
        if (text == null)
        {
                SetValue("text为空"+ip);
            return 1;
        }
        int result;
        try
        {
                MachineCtro.ListField[] lf = new MachineCtro.ListField[1];
                try
                {
                    lf[0].cardid = Int64.Parse("615126");
                }
                catch
                { MessageBox.Show("卡号不可为空或字母"); return 1; }
                lf[0].empno = "615126";//tbEmpno.Text;
                lf[0].empname = "马腾飞";//tbEmpname.Text;
                                      //lf[0].subsidyformat = (int.Parse(tbSubsidyAmount.Text) * 100).ToString("X2").PadLeft(8, '0');
                                      //lf[0].subsidyformat += Convert.ToString(int.Parse(tbSubsidyno.Text), 16).PadLeft(4, '0');
                                      // lf[0].subsidyformat += "0"+ cbSubsidyType.SelectedIndex.ToString();
                lf[0].subsidyformat += "00000000000000";// +cbSubsidyType.SelectedIndex.ToString();
                lf[0].startsubsidy = "20170101";// tbstartsubsidy.Text;
                lf[0].endsubsidy = "20991231";// tbendsubsidy.Text;
                lf[0].amountend = "20191231";// tbamountend.Text;
                lf[0].blackcount = int.Parse("0");//tbBlackCount.Text);
                lf[0].listtype = 1;   //默认为白名单，可以不写此行代码
                if (this.dlcCtro.DownList(ip, lf) == 0)
            {
                    SetValue(DateTime.Now.ToString() + ":" + ip + "下载黑名单成功！");
                SaveLog(Application.StartupPath + "\\errorlog.txt", DateTime.Now.ToString() + ":" + ip + "下载黑名单成功！");
                result = 0;
            }
            else
            {
                    SetValue(DateTime.Now.ToString() + ":" + ip + "下载黑名单失败！");
                SaveLog(Application.StartupPath + "\\errorlog.txt", DateTime.Now.ToString() + ":" + ip + "下载黑名单失败！");
                result = -1;
            }
        }
        catch (Exception ex)
        {
                SetValue(string.Concat(new string[]
            {
                    DateTime.Now.ToString(),
                    ":",
                    ip,
                    "下载黑名单失败:",
                    ex.Message
            }));
            SaveLog(Application.StartupPath + "\\errorlog.txt", string.Concat(new string[]
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
    private void dlcsocket_PhotoReceivedX(int flowno)
        {
        }
        private int dlcsocket_QueryApplyed(string ip, ref int[] row, ref int[] col, ref string[] message, ref int seconds)
        {
            return 0;
            //string sqlstring = "Data Source=" + tbServer.Text + ";Database=" + tbDatabase.Text + ";User id=" + tbLogID.Text + ";PWD=" + tbLogPass.Text;
            //if (nosql != "1")
            //{
            //    SqlConnection sql = new SqlConnection(sqlstring);
            //    sql.Open();
            //    sqlstring = "exec sp_queryapplyed '" + ip + "'";
            //    SqlDataAdapter SQLda = new SqlDataAdapter(sqlstring, sql);
            //    DataSet ds = new DataSet();
            //    SQLda.Fill(ds);
            //    sql.Close();
            //    if (ds.Tables[0].Rows[0][0].ToString() == "1")
            //    {//存储过程执行正确记录此次调用
            //        int count = (ds.Tables[0].Columns.Count - 2) / 3;
            //        row = new int[count];
            //        col = new int[count];
            //        message = new string[count];
            //        for (int i = 0; i < count; i++)
            //        {
            //            row[i] = int.Parse(ds.Tables[0].Rows[0][i * 3 + 2].ToString());
            //            col[i] = int.Parse(ds.Tables[0].Rows[0][i * 3 + 3].ToString());
            //            message[i] = ds.Tables[0].Rows[0][i * 3 + 4].ToString();
            //        }
            //        seconds = int.Parse(ds.Tables[0].Rows[0][1].ToString());
            //        path = Application.StartupPath + "\\sqloklog.txt";
            //        SaveLog(path, DateTime.Now.ToString() + ":" + sqlstring);
            //        return 0;
            //    }
            //    else
            //    {//存储过程执行返回错误记录此次调用
            //        path = Application.StartupPath + "\\sqlerrorlog.txt";
            //        SaveLog(path, DateTime.Now.ToString() + ":" + sqlstring);
            //        return 1;
            //    }
            //}
            //else
            //{
            //    int count = 2;
            //    row = new int[count];
            //    col = new int[count];
            //    message = new string[count];
            //    row[0] = 4;
            //    col[1] = 128;
            //    message[0] = "在线无数据";
            //    row[1] = 6;
            //    col[0] = 128;
            //    message[1] = "查询成功";
            //    seconds = 10;
            //    return 1;
            //}
        }
        private void ReceivedError(string errmsg)
        {
            try
            {
                string path = Application.StartupPath + "\\errorlog.txt";
                SaveLog(path, DateTime.Now.ToString() + ":" + errmsg);
            }
            catch { }
        }
        private bool DataReceived(string data)
        {
            try
            {
                //先保存接收到的原始数据
                string path = Application.StartupPath + "\\datalog.txt";
                SaveLog(path, DateTime.Now.ToString() + ":" + data);

                //string sqlstring = "Data Source=" + tbServer.Text + ";Database=" + tbDatabase.Text + ";User id=" + tbLogID.Text + ";PWD=" + tbLogPass.Text;
                string cardid = "", ip = "", datetime = "", occur = "", result = "", before = "", after = "", mdno = "", cdno = "", dealtype = "", macno = "", serno = "", status = "";
                string[] items = data.Split(',');
                for (int i = 0; i < items.Length;i++)
                {
                    SetValue(items[i]);
                }
                //int row = dataRecord.Rows.Count;
                for (int index = 0; index < items.Length; index++)
                {
                    if (items[index].IndexOf("cardid", 0) >= 0 || items[index].IndexOf("cardx", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "phyid", row, items[index].Substring(7));
                        cardid = items[index].Substring(7);
                    }
                    if (items[index].IndexOf("ip", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "ip", row, items[index].Substring(3));
                        ip = items[index].Substring(3);
                    }
                    if (items[index].IndexOf("datetime", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "datetime", row, items[index].Substring(9));
                        datetime = items[index].Substring(9);
                    }
                    if (items[index].IndexOf("occur", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "occur", row, items[index].Substring(6));
                        occur = items[index].Substring(6);
                    }
                    if (items[index].IndexOf("result", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "result", row, items[index].Substring(7));
                        result = items[index].Substring(7);
                    }
                    if (items[index].IndexOf("before", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "before", row, items[index].Substring(7));
                        before = items[index].Substring(7);
                    }
                    if (items[index].IndexOf("after", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "after", row, items[index].Substring(6));
                        after = items[index].Substring(6);
                    }
                    if (items[index].IndexOf("event", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "event1", row, GetDoorEventStr(items[index].Substring(6)));
                        after = items[index].Substring(6);
                    }
                    if (items[index].IndexOf("doorno", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "doorno", row, items[index].Substring(7));
                        after = items[index].Substring(7);
                    }
                    if (items[index].IndexOf("readerno", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "readerno", row, items[index].Substring(9));
                        after = items[index].Substring(9);
                    }
                    if (items[index].IndexOf("doorpass", 0) >= 0)
                    {
                        ///SetItemValue(dataRecord, "doorpass", row, items[index].Substring(9));
                        after = items[index].Substring(9);
                    }
                    if (items[index].IndexOf("passtype", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "passtype", row, items[index].Substring(9));
                        after = items[index].Substring(9);
                    }
                    if (items[index].IndexOf("realstatus", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "realstatus", row, items[index].Substring(11));
                        after = items[index].Substring(11);
                    }
                    if (items[index].IndexOf("workstatus", 0) >= 0)
                    {
                        //SetItemValue(dataRecord, "workstatus", row, items[index].Substring(11));
                        after = items[index].Substring(11);
                    }
                    if (items[index].IndexOf("mdno", 0) >= 0)
                    {
                        mdno = items[index].Substring(5);
                    }
                    if (items[index].IndexOf("cdno", 0) >= 0)
                    {
                        cdno = items[index].Substring(5);
                    }
                    if (items[index].IndexOf("dealtype", 0) >= 0)
                    {
                        dealtype = items[index].Substring(9);
                    }
                    if (items[index].IndexOf("status", 0) >= 0)
                    {
                        status = items[index].Substring(7);
                    }
                    if (items[index].IndexOf("macno", 0) >= 0)
                    {
                        macno = items[index].Substring(6);
                    }
                    if (items[index].IndexOf("serno", 0) >= 0)
                    {
                        serno = items[index].Substring(6);
                    }
                }
                //if (nosql != "1")
                //{
                //    SqlConnection sql = new SqlConnection(sqlstring);
                //    sql.Open();
                //    if (occur == "")//无消费金额
                //    {
                //        sqlstring = "exec sp_attdatareceived '" + cardid + "','" + ip + "','" + datetime + "'," + mdno + "," + result;
                //    }
                //    else
                //    {
                //        sqlstring = "exec sp_consumdatareceived '" + cardid + "','" + ip + "','" + datetime + "'," + result + "," + dealtype + "," + before + "," + occur + "," + after + "," + mdno + "," + cdno + "," + status + ",'" + serno + "'," + macno;
                //    }
                //    SqlDataAdapter SQLda = new SqlDataAdapter(sqlstring, sql);
                //    DataSet ds = new DataSet();
                //    SQLda.Fill(ds);
                //    sql.Close();
                //    if (ds.Tables[0].Rows[0][0].ToString() == "1")
                //    {//存储过程执行正确记录此次调用
                //        path = Application.StartupPath + "\\sqloklog.txt";
                //        SaveLog(path, DateTime.Now.ToString() + ":" + sqlstring);
                //        return true;
                //    }
                //    else
                //    {//存储过程执行返回错误记录此次调用
                //        path = Application.StartupPath + "\\sqlerrorlog.txt";
                //        SaveLog(path, DateTime.Now.ToString() + ":" + sqlstring);
                //        return false;
                //    }
                //}

                return true;
               
            }
            catch (Exception ex)
            {//接收到数据分析出错保存记录
                string path = Application.StartupPath + "\\errorlog.txt";
                SaveLog(path, DateTime.Now.ToString() + ":" + ex.Message);
                return false;
            }
        }
        void SaveLog(string path,string text)
        {
            try
            {
                FileStream fs;
                //fs = File.Open(path, FileMode.Append, FileAccess.ReadWrite);
                //fs = new FileStream(path, FileMode.OpenOrCreate);
                //StreamWriter sw = new StreamWriter(fs);
                //获得字节数组
                byte[] data = System.Text.Encoding.Default.GetBytes(text + "\r\n");
                //开始写入
                //fs.Seek(0, SeekOrigin.Begin);
                //sw.WriteLine(str);
                //sw.Close();
                fs = File.OpenWrite(path);
                //设定书写的开始位置为文件的末尾  
                fs.Position = fs.Length;
                //将待写入内容追加到文件末尾  
                fs.Write(data, 0, data.Length);
                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //dlcManage.MachineDumped += new MachineCtro.MachineDump(dlcsocket_MachineDumped);
            //dlcManage.DataReceived += new MachineCtro.MachineDataReceived(DataReceived);
            IPEndPoint point = new IPEndPoint(IPAddress.Parse("172.18.1.238"), 8007);
            this.dlcCtro = new MachineCtro(point);
            DeleteBlackList("172.18.1.111:0010",615126);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);//彻底关闭进程
        }
        private int DeleteBlackList(string ip,long cardno)
        {

            bool flag = false;
            
            string text = null;
            try
            {
                text = this.dlcCtro.GetTime(ip);
            }
            catch (Exception e)
            {
                SetValue("错误：" + e.Message);
                return 0;
            }
            if (text == null)
            {
                SetValue("text为空" + ip);
                return 1;
            }
            try
            {
                flag = dlcCtro.DeleteBlackList(ip,cardno);
                if (flag)
                {
                    SetValue("解挂成功：" + ip);
                }
                else
                {
                    SetValue("解挂失败："+ip);
                }
            }
            catch (Exception e)
            {
                SetValue("解挂错误："+e.Message);
                return 1;
            }
            return 0;
        }
    }
}
