using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace TestMachProject
{
    public class MachineCtro
    {
        #region 内部变量
        /// <summary>
        /// 帧头标志SYN(固定值为0x3AA3)
        /// </summary>
        private string m_syn = "3AA3";

        /// <summary>
        /// 保留字节RES(4字节，保留以作后续扩展适用。默认为0x00000000)
        /// </summary>
        private string m_res = "00000000";

        /// <summary>
        /// 设备地址ADDR(2字节)
        /// </summary>
        private string m_macaddr = "0001";

        /// <summary>
        /// 命令长度SLEN(2字节)
        /// </summary>
        private string m_slen = "000A";

        /// <summary>
        /// 命令码COMMAND(2字节)
        /// </summary>
        private string m_command = "0001";

        /// <summary>
        /// 命令数据APPDATA(长度0-1100字节)
        /// </summary>
        private string m_appdata = "0001414243444546";
        int ctypeindex = 10, cnoindex = 11;
        private struct RecordField
        {
            public int typeno;
            public int len;
            public string type;
        }
        private RecordField[] recordfield = new RecordField[70];
        private struct ProtocolField
        {
            public int typeno;
            public int len;
            public string type;
        }
        private ProtocolField[] protocolfield = new ProtocolField[32];
        private byte[] commandline;
        private ushort m_port = 10009, m_mport = 4001, m_diff = 3;
        private Socket m_socket = null;
        private IPEndPoint m_local = null;
        private EndPoint m_localep = null;
        Thread _receiveThread = null;

        private SerialPort serialPort = new SerialPort();
        Thread thread;
        volatile bool _keepReading = false;
        private int commtype = 0;//通讯方式：0为UDP；1为TCP/IP；2为串口

        private List<TcpSocket> sockets = new List<TcpSocket>();//2017-06-25添加

        private byte[] photo;
        /// <summary>
        /// ListBox 用于实时显示接收到的信息及产生的错误信息
        /// </summary>
        public ushort Port
        {
            get { return m_port; }
            //set{m_port=value;;}
        }
        #endregion
        #region 结构声明
        /// <summary>
        /// 管理员卡
        /// </summary>
        public struct ManageCard
        {
            public int index;
            public int usetype;
            public int usertype;
            public long card;
            public int password;
        }
        /// <summary>
        /// 时段参数
        /// </summary>
        public struct TimeSeperate
        {
            public string start;
            public string end;
            public int xtype;
            public string workparm;
        }
        /// <summary>
        /// 刷卡结果
        /// </summary>
        public struct CardBlashResult
        {
            public int result;
            public int sysflow;
            public int money;
            public int occur;
            public string message;
            public string name;
            public string subdate;
            public int subbatch;
        }
        /// <summary>
        /// IC联动刷卡结果
        /// <summary>
        //public struct CardBlashResultIC
        //{
        //    public int result;
        //}
        /// <summary>
        /// 名单存储格式
        /// </summary>
        public struct ListStoreFormat
        {
            public int len;
            public string format;
        }
        public struct ListField
        {
            /// <summary>
            /// 卡号
            /// </summary>
            public Int64 cardid;
            /// <summary>
            /// 工号
            /// </summary>
            public string empno;
            /// <summary>
            /// 姓名
            /// </summary>
            public string empname;
            /// <summary>
            /// 刷卡时间控制
            /// </summary>
            public string timecontrol;
            /// <summary>
            /// 补贴格式
            /// </summary>
            public string subsidyformat;
            /// <summary>
            /// 黑名单计数
            /// </summary>
            public int blackcount;
            /// <summary>
            /// 开始补贴日期
            /// </summary>
            public string startsubsidy;
            /// <summary>
            /// 结束补贴日期
            /// </summary>
            public string endsubsidy;
            /// <summary>
            /// 补贴金额到期日期
            /// </summary>
            public string amountend;
            /// <summary>
            /// 个人密码（个人开门密码），3字节BCD格式存储
            /// </summary>
            public string password;
            /// <summary>
            /// 名单权限
            /// </summary>
            public int listright;
            /// <summary>
            /// 名单读头属性
            /// </summary>
            public int readerright;
            /// <summary>
            /// 名单日期段
            /// </summary>
            public string listperiod;
            /// <summary>
            /// 名单失效日期
            /// </summary>
            public string listvalid;
            /// <summary>
            /// 节假日是否有效
            /// </summary>
            public int holidayvalid;
            /// <summary>
            /// 名单格式
            /// </summary>
            public ListStoreFormat liststoredformat;
            /// <summary>
            /// 名单类型：0白名单1黑名单；2补贴名单
            /// </summary>
            public int listtype;
        }
        /// <summary>
        /// 打铃时间
        /// </summary>
        public struct BellTime
        {
            /// <summary>
            /// 周(每位代表一周的每一天，7bit保留)
            /// </summary>
            public int week;
            /// <summary>
            /// 时
            /// </summary>
            public int hour;
            /// <summary>
            /// 分
            /// </summary>
            public int minute;
            /// <summary>
            /// 秒
            /// </summary>
            public int second;
            /// <summary>
            /// 响铃时长
            /// </summary>
            public int holdbell;
        }
        public struct Protocol
        {
            public int index;
            public int len;
            public int group;
            public string region;
            public ProtocolDetail[] protocol;
        }
        public struct ProtocolDetail
        {
            public int type;
            public string value;
        }
        public class MonitorParm
        {
            public byte[] data;
            public IPEndPoint ep;
            public int len;
            public MonitorParm(byte[] adata, IPEndPoint aep, int alen)
            {
                data = adata;
                ep = aep;
                len = alen;
            }
        }
        public struct TcpSocket
        {
            public Socket socket;
            public string ip;
            public List<byte[]> received;
            public List<byte[]> command;
            public Thread thread;
        }


        #endregion



        #region 初始化
        /// <summary>
        /// 创建一个本地默认端口的实例，端口号：10009
        /// </summary>
        public MachineCtro()
        {
            ini();
        }
        /// <summary>
        /// 创建一个指定端口号的实例
        /// </summary>
        /// <param name="port">指定的端口号</param>
        public MachineCtro(ushort port)
        {
            m_port = port;
            ini();
        }
        public MachineCtro(IPEndPoint ip)
        {
            m_local = ip;
            m_localep = ip as EndPoint;
            m_port = ushort.Parse(ip.Port.ToString());
            ini();
        }
        public MachineCtro(string portname, int rate)
        {
            serialPort.PortName = portname;
            serialPort.BaudRate = rate;
            commtype = 2;
            ini();
        }
        public MachineCtro(bool type)
        {
            if (type)
                commtype = 2;
            else
                commtype = 1;
            ini();
        }
        private void ini()
        {
            RecordFieldIni();
            ProtocolFieldIni();
            switch (commtype)
            {
                case 1://TCP/IP

                    break;
                case 2://串口
                    serialPort.Open();
                    StartReading();
                    break;
                default:
                    if (m_local == null)
                    {
                        m_local = new IPEndPoint(IPAddress.Parse(GetLocalIp()), m_port);
                        m_localep = new IPEndPoint(IPAddress.Parse(GetLocalIp()), m_port) as EndPoint;
                    }
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    m_socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                    m_socket.Bind(m_localep);
                    StartReceive();
                    break;
            }
        }
        private void RecordFieldIni()
        {
            recordfield[0].typeno = 1;//返回1字节年数据（16进制码）
            recordfield[0].len = 1;
            recordfield[0].type = "Y1";
            recordfield[1].typeno = 2;//返回2字节年数据（16进制码）
            recordfield[1].len = 2;
            recordfield[1].type = "Y2";
            recordfield[2].typeno = 3;//返回4字节mdhm‘月日时分’时间数据（16进制码）
            recordfield[2].len = 4;
            recordfield[2].type = "Mdhm";
            recordfield[3].typeno = 4;//返回1字节秒数据（16进制码）
            recordfield[3].len = 1;
            recordfield[3].type = "second";
            recordfield[4].typeno = 6;//返回打卡结果数据（1字节）
            recordfield[4].len = 1;
            recordfield[4].type = "result";
            recordfield[5].typeno = 18;//交易状态(1字节)	0x00：表示正常刷卡记录	0x13：表示灰记录
            recordfield[5].len = 1;
            recordfield[5].type = "status";
            recordfield[6].typeno = 19;//交易类型（1字节）0x01：消费；0x02：充值；0x03：联机交易；0x04：记账交易，0x05：表示消费撤销0x06:充值撤销，0x07:清零补贴， 0x08：累加补贴， 0x09:订餐记录，0x0A:验餐
            recordfield[6].len = 1;
            recordfield[6].type = "dealtype";
            recordfield[7].typeno = 20;//交易前余额(4字节，整形数字，高字节在前)
            recordfield[7].len = 4;
            recordfield[7].type = "before";
            recordfield[8].typeno = 21;//交易后余额(4字节，整形数字，高字节在前)
            recordfield[8].len = 4;
            recordfield[8].type = "after";
            recordfield[9].typeno = 22;//交易金额(4字节，整形数字，高字节在前)
            recordfield[9].len = 4;
            recordfield[9].type = "occur";
            recordfield[10].typeno = 23;//终端机交易流水号(4字节，整形数字)
            recordfield[10].len = 4;
            recordfield[10].type = "mdno";
            recordfield[11].typeno = 24;//企业代码 (2字节)
            recordfield[11].len = 2;
            recordfield[11].type = "companyno";
            recordfield[12].typeno = 25;//卡交易序号(4字节，整形数字)
            recordfield[12].len = 4;
            recordfield[12].type = "cdno";
            recordfield[13].typeno = 26;//自动补贴批次（2字节，整数0-65535）高字节在前，低字节在后
            recordfield[13].len = 2;
            recordfield[13].type = "auto";
            recordfield[14].typeno = 27;//钱包类型(1字节) 0x01：有限余额；0x02：不限余额  0x11：有限次；0x12：不限次
            recordfield[14].len = 1;
            recordfield[14].type = "bagtype";
            recordfield[15].typeno = 28;//操作员卡号或编号（4字节）
            recordfield[15].len = 4;
            recordfield[15].type = "opcard";
            recordfield[16].typeno = 29;//存读卡模式(1字节)  0x00:读卡物理序列号  0x01：读卡逻辑卡号
            recordfield[16].len = 1;
            recordfield[16].type = "readtype";
            recordfield[17].typeno = 30;//记录的读卡器标识（1个字节），表示读卡器编号。=0x01：内部读卡器第1读头；	=0x02：内部读卡器第2读头；	=0x81：外接第1读头。	=0x82：外接第2读头。
            recordfield[17].len = 1;
            recordfield[17].type = "reader";
            recordfield[18].typeno = 32;//补贴钱包交易前余额(4个字节，HEX码，高字节在前)
            recordfield[18].len = 4;
            recordfield[18].type = "bag2bef";
            recordfield[19].typeno = 33;//补贴钱包交易后余额(4个字节，HEX码，高字节在前)
            recordfield[19].len = 4;
            recordfield[19].type = "bag2aft";
            recordfield[20].typeno = 130;//0x82—0x85,0x88:	返回16进制卡号长度(2—5,8字节)。
            recordfield[20].len = 2;
            recordfield[20].type = "cardx2";
            recordfield[21].typeno = 131;//0x82—0x85,0x88:	返回16进制卡号长度(2—5,8字节)。
            recordfield[21].len = 3;
            recordfield[21].type = "cardx3";
            recordfield[22].typeno = 132;//0x82—0x85,0x88:	返回16进制卡号长度(2—5,8字节)。
            recordfield[22].len = 4;
            recordfield[22].type = "cardx4";
            recordfield[23].typeno = 133;//0x82—0x85,0x88:	返回16进制卡号长度(2—5,8字节)。
            recordfield[23].len = 5;
            recordfield[23].type = "cardx5";
            recordfield[24].typeno = 136;//0x82—0x85,0x88:	返回16进制卡号长度(2—5,8字节)。
            recordfield[24].len = 8;
            recordfield[24].type = "cardx8";
            recordfield[25].typeno = 144;//返回工号(长度1—16字节)。
            recordfield[25].len = 1;
            recordfield[25].type = "empid01";
            recordfield[26].typeno = 145;//返回工号(长度1—16字节)。
            recordfield[26].len = 2;
            recordfield[26].type = "empid02";
            recordfield[27].typeno = 146;//返回工号(长度1—16字节)。
            recordfield[27].len = 3;
            recordfield[27].type = "empid03";
            recordfield[28].typeno = 147;//返回工号(长度1—16字节)。
            recordfield[28].len = 4;
            recordfield[28].type = "empid04";
            recordfield[29].typeno = 148;//返回工号(长度1—16字节)。
            recordfield[29].len = 5;
            recordfield[29].type = "empid05";
            recordfield[30].typeno = 149;//返回工号(长度1—16字节)。
            recordfield[30].len = 6;
            recordfield[30].type = "empid06";
            recordfield[31].typeno = 150;//返回工号(长度1—16字节)。
            recordfield[31].len = 7;
            recordfield[31].type = "empid07";
            recordfield[32].typeno = 151;//返回工号(长度1—16字节)。
            recordfield[32].len = 8;
            recordfield[32].type = "empid08";
            recordfield[33].typeno = 152;//返回工号(长度1—16字节)。
            recordfield[33].len = 9;
            recordfield[33].type = "empid09";
            recordfield[34].typeno = 153;//返回工号(长度1—16字节)。
            recordfield[34].len = 10;
            recordfield[34].type = "empid10";
            recordfield[35].typeno = 154;//返回工号(长度1—16字节)。
            recordfield[35].len = 11;
            recordfield[35].type = "empid11";
            recordfield[36].typeno = 155;//返回工号(长度1—16字节)。
            recordfield[36].len = 12;
            recordfield[36].type = "empid12";
            recordfield[37].typeno = 156;//返回工号(长度1—16字节)。
            recordfield[37].len = 13;
            recordfield[37].type = "empid13";
            recordfield[38].typeno = 157;//返回工号(长度1—16字节)。
            recordfield[38].len = 14;
            recordfield[38].type = "empid14";
            recordfield[39].typeno = 158;//返回工号(长度1—16字节)。
            recordfield[39].len = 15;
            recordfield[39].type = "empid15";
            recordfield[40].typeno = 159;//返回工号(长度1—16字节)。
            recordfield[40].len = 16;
            recordfield[40].type = "empid16";

            recordfield[41].typeno = 88;//返回机器记录号(长度4字节)。
            recordfield[41].len = 4;
            recordfield[41].type = "mrno";
            recordfield[42].typeno = 8;//事件名称（1字节）
            recordfield[42].len = 1;
            recordfield[42].type = "event";
            recordfield[43].typeno = 34;//红外通道的进出方向，0x01:进、0x02：出、其他：未知
            recordfield[43].len = 1;
            recordfield[43].type = "redio";
            recordfield[44].typeno = 35;//照片流水
            recordfield[44].len = 4;
            recordfield[44].type = "photono";
            recordfield[45].typeno = 31;//物理卡ID号(4个字节，HEX码)
            recordfield[45].len = 4;
            recordfield[45].type = "cardid";
            recordfield[46].typeno = 9;//存门编号。0-7，0xFF表示门编号未知
            recordfield[46].len = 1;
            recordfield[46].type = "doorno";
            recordfield[47].typeno = 10;//存读头编号。0-7，0xFF表示读头编号未知
            recordfield[47].len = 1;
            recordfield[47].type = "readerno";
            recordfield[48].typeno = 11;//输入输出通道编号：表示该输入输出事件产生的输入输出端口，当为0xFF表示输入输出通道未知
            recordfield[48].len = 1;
            recordfield[48].type = "pno";
            recordfield[49].typeno = 12;//返回开门密码,BCD码（3字节）
            recordfield[49].len = 3;
            recordfield[49].type = "doorpass";
            recordfield[50].typeno = 13;//密码类型（1字节）
            recordfield[50].len = 1;
            recordfield[50].type = "passtype";
            recordfield[51].typeno = 14;//门实时的状态（1字节）
            recordfield[51].len = 1;
            recordfield[51].type = "realstatus";
            recordfield[52].typeno = 15;//门动作的状态（1字节）
            recordfield[52].len = 1;
            recordfield[52].type = "workstatus";
            recordfield[53].typeno = 36;//存重量值(8个字节，ASCII码，带小数点，单位为公斤) 如：“1234.678”
            recordfield[53].len = 8;
            recordfield[53].type = "carryvalue";
            recordfield[54].typeno = 37;//称重物品编号(2字节)
            recordfield[54].len = 2;
            recordfield[54].type = "itemno";
            recordfield[55].typeno = 38;//时间段消费次数，1字节，HEX
            recordfield[55].len = 1;
            recordfield[55].type = "xftimes";
            recordfield[56].typeno = 39;//1字节性别：0x30女，0x31男
            recordfield[56].len = 1;
            recordfield[56].type = "sex";
            recordfield[57].typeno = 40;//2字节民族：ASCII码例：汉族0x3130
            recordfield[57].len = 2;
            recordfield[57].type = "nation";
            recordfield[58].typeno = 41;//8字节出生日期ASCII码
            recordfield[58].len = 8;
            recordfield[58].type = "birthday";
            recordfield[59].typeno = 42;//70字节汉字住址，后补0
            recordfield[59].len = 70;
            recordfield[59].type = "famiaddr";
            recordfield[60].typeno = 43;//18字节身份证号ASCII码
            recordfield[60].len = 18;
            recordfield[60].type = "idcardno";
            recordfield[61].typeno = 44;//30字节签发机关
            recordfield[61].len = 30;
            recordfield[61].type = "certorg";
            recordfield[62].typeno = 45;//有效起始日期8字节ASCII
            recordfield[62].len = 8;
            recordfield[62].type = "startdt";
            recordfield[63].typeno = 46;//有效截止日期8字节ASCII
            recordfield[63].len = 8;
            recordfield[63].type = "enddt";
            recordfield[64].typeno = 47;//姓名16字节
            recordfield[64].len = 16;
            recordfield[64].type = "empname";
            //事件名称(8)说明：
            //0x00: 未定义，不可使用
            //读头事件：0x01—0x1F
            //0x01:正常白名单卡刷卡事件
            //0x02:特权卡刷卡事件
            //0x03:刷卡+密码事件
            //0x04:任意多卡开门事件
            //0x05:指定多卡开门事件
            //0x06:普通密码输入事件
            //0x07：超级密码输入事件
            //0x08: 布防密码输入事件
            //0x09: 撤防密码输入事件
            //0x0A: 控制器布防事件
            //0x0B: 控制器撤防事件
            //0x0C：指定权限组别卡开门事件
            //门事件：0x20-0x2F
            //0x20：门开事件
            //0x21：门关事件
            //输入输出事件：0x30-0x3F
            //0x30：开门按钮事件
            //0x31：门磁输入有效事件
            //0x32：辅助信号1输入有效事件（开关闭合或低电平）
            //0x33：辅助信号2输入有效事件（开关闭合或低电平）
            //0x34：辅助信号3输入有效事件（开关闭合或低电平）
            //0x35：辅助信号4输入有效事件（开关闭合或低电平）
            //0x38：辅助信号输出有效事件
            //0x3F：红外信号输入事件
            //管理软件事件：0x40-0x4F
            //0x40：管理软件开门
            //0x41：管理软件关门
            //0x42：管理软件布防
            //0x43：管理软件撤防
            //控制器本身事件：0x50-0x7F
            //0x50—0x5F：联动事件1-16生效
            //-----非正常事件---------------
            //读头事件：0x80-0x9F
            //0x80: 非法卡刷卡事件
            //0x81：黑名单刷卡事件
            //0x82：非允许时间段的白名单刷卡事件
            //0x83：读头防撬报警事件
            //0x84：胁迫报警事件
            //0x85：非允许时段的密码输入事件
            //门事件：0xA0–0xAF
            //0xA0：门开超时事件
            //0xA1：门关超时事件
            //0xA2：非法入侵事件
            //0xA3：门点互锁开门失败事件
            //0xA4：APB开门失败事件
            //0xA5: 电锁被撬事件
            //输入输出事件：0xB0–0xBF
            //管理软件事件：0xC0–0xCF
            //控制器本身事件：0xD0–0xFF
            //0xD0：控制器防撬报警事件

        }
        private void ProtocolFieldIni()
        {
            protocolfield[0].typeno = 0x01;//开始时间:执行策略的开始时间）
            protocolfield[0].len = 2;
            protocolfield[0].type = "start";
            protocolfield[1].typeno = 0x02;//结束时间:执行策略的结束时间
            protocolfield[1].len = 2;
            protocolfield[1].type = "end";
            protocolfield[2].typeno = 0x03;//段限次（min）:段消费小于这个数值不执行策略
            protocolfield[2].len = 1;
            protocolfield[2].type = "Mdhm";
            protocolfield[3].typeno = 0x04;//段限次(max):段消费大于这个次数不执行策略
            protocolfield[3].len = 1;
            protocolfield[3].type = "second";
            protocolfield[4].typeno = 0x05;//日限次（min）:日消费小于这个次数不执行策略
            protocolfield[4].len = 1;
            protocolfield[4].type = "result";
            protocolfield[5].typeno = 0x06;//日限次（max）:日消费大于这个次数不执行策略
            protocolfield[5].len = 1;
            protocolfield[5].type = "status";
            protocolfield[6].typeno = 0x07;//月限次（min）:月消费小于这个次数不执行策略
            protocolfield[6].len = 2;
            protocolfield[6].type = "dealtype";
            protocolfield[7].typeno = 0x08;//月限次（max）:月消费大于这个次数不执行策略
            protocolfield[7].len = 2;
            protocolfield[7].type = "before";
            protocolfield[8].typeno = 0x09;//次限额（min）：当次消费小于该值不执行策略
            protocolfield[8].len = 4;
            protocolfield[8].type = "after";
            protocolfield[9].typeno = 0x0A;//次限额（max）：当次消费大于该值不执行策略
            protocolfield[9].len = 4;
            protocolfield[9].type = "occur";
            protocolfield[10].typeno = 0x0B;//段限额（min）：段消费小于该值不执行策略
            protocolfield[10].len = 4;
            protocolfield[10].type = "mdno";
            protocolfield[11].typeno = 0x0C;//段限额（max）：段消费大于该值不执行策略
            protocolfield[11].len = 4;
            protocolfield[11].type = "companyno";
            protocolfield[12].typeno = 0x0D;//日限额（min）：日消费小于该值不执行策略
            protocolfield[12].len = 4;
            protocolfield[12].type = "cdno";
            protocolfield[13].typeno = 0x0E;//日限额（max）：日消费大于该值不执行策略
            protocolfield[13].len = 4;
            protocolfield[13].type = "auto";
            protocolfield[14].typeno = 0x0F;//月限额（min）：消费小于该值不执行策略
            protocolfield[14].len = 4;
            protocolfield[14].type = "bagtype";
            protocolfield[15].typeno = 0x10;//月限额（max）：消费大于该值不执行策略
            protocolfield[15].len = 4;
            protocolfield[15].type = "opcard";
            protocolfield[16].typeno = 0x50;//折扣率：对交易金额进行折扣
            protocolfield[16].len = 1;
            protocolfield[16].type = "readtype";
            protocolfield[17].typeno = 0x51;//优惠金额：对交易金额执行数额调整
            protocolfield[17].len = 4;
            protocolfield[17].type = "reader";
            protocolfield[18].typeno = 0x52;//四舍五入：从第N位执行四舍五入操作
            protocolfield[18].len = 1;
            protocolfield[18].type = "cardid";
            protocolfield[19].typeno = 0x53;//最低消费：交易金额在主钱包的扣款比例，其他在补贴钱包扣除
            protocolfield[19].len = 1;
            protocolfield[19].type = "bag2bef";
            protocolfield[20].typeno = 0x54;//最高消费：交易金额在主钱包的扣款金额，其他在补贴钱包中扣除
            protocolfield[20].len = 4;
            protocolfield[20].type = "bag2aft";
            protocolfield[21].typeno = 0x55;//主钱包扣款比例：交易金额在补贴钱包的扣款金额，其他在主钱包中扣除
            protocolfield[21].len = 4;
            protocolfield[21].type = "cardx2";
            protocolfield[22].typeno = 0x56;//主钱包扣款金额：低于最低交易时按照最低交易执行交易
            protocolfield[22].len = 4;
            protocolfield[22].type = "cardx3";
            protocolfield[23].typeno = 0x57;//补贴钱包扣款金额：大于最高交易时按照最高交易执行交易
            protocolfield[23].len = 4;
            protocolfield[23].type = "cardx4";
            protocolfield[24].typeno = 0x58;//段限次：段限次次数大于此值，不允许消费，交易退出，0表示不限制
            protocolfield[24].len = 1;
            protocolfield[24].type = "cardx4";
            protocolfield[25].typeno = 0x59;//日限次：日限次次数大于此值，不允许消费，交易退出，0表示不限制
            protocolfield[25].len = 1;
            protocolfield[25].type = "cardx4";
            protocolfield[26].typeno = 0x5A;//月限次：月限次次数大于此值，不允许消费，交易退出，0表示不限制
            protocolfield[26].len = 2;
            protocolfield[26].type = "cardx4";
            protocolfield[27].typeno = 0x5B;//每次限额：每次消费金额大于此值，不允许消费，交易退出，0表示不限制
            protocolfield[27].len = 4;
            protocolfield[27].type = "cardx4";
            protocolfield[28].typeno = 0x5C;//段限额：时间段消费金额大于此值，不允许消费，交易退出，0表示不限制
            protocolfield[28].len = 4;
            protocolfield[28].type = "cardx4";
            protocolfield[29].typeno = 0x5D;//日限额：日消费金额大于此值，不允许消费，交易退出，0表示不限制
            protocolfield[29].len = 4;
            protocolfield[29].type = "cardx4";
            protocolfield[30].typeno = 0x5E;//月限额：月消费金额大于此值，不允许消费，交易退出，0表示不限制
            protocolfield[30].len = 4;
            protocolfield[30].type = "cardx4";
        }
        private string GetLocalIp()
        {
            string hostname = Dns.GetHostName();
            IPAddress[] localaddr = Dns.GetHostAddresses(hostname);
            for (int index = 0; index < localaddr.Length; index++)
            {
                if (localaddr[index].ToString().IndexOf(".") > 0)
                    return localaddr[index].ToString();
            }
            return localaddr[0].ToString();
        }
        #endregion

        public void Close()
        {
            if (m_socket != null && m_socket.Connected)
            {
                m_socket.Shutdown(SocketShutdown.Both);
            }
            if (m_socket != null) m_socket.Close();
            if (_receiveThread != null)
            {
                _receiveThread.Abort();
                _receiveThread = null;
            }
        }
        public List<int> bufferlen = new List<int>();
        public List<byte[]> savebuffer = new List<byte[]>();
        //CardBlashResult cbr = CardBlashed(ep.Address, type, flowno, cardno, amount, dealtype);
        // CardBlashResult ret = CardBlashedIC(ep.Address, ref parms);
        #region 内部函数
        private void StartReading()
        {
            if (!_keepReading)
            {
                _keepReading = true;
                thread = new Thread(new ThreadStart(ReadPort));
                thread.Start();
            }
        }
        private void ReadPort()
        {
            while (_keepReading)
            {
                if (serialPort.IsOpen)
                {
                    int count = serialPort.BytesToRead;
                    Thread.Sleep(20);


                    //延时等待数据接收完毕。
                    while ((count < serialPort.BytesToRead) && (serialPort.BytesToRead < 4800))
                    {
                        count = serialPort.BytesToRead;
                        Thread.Sleep(20);
                    }
                    if (count > 0)
                    {
                        byte[] readBuffer = new byte[count];

                        try
                        {
                            Application.DoEvents();
                            serialPort.Read(readBuffer, 0, count);

                            savebuffer.Add(readBuffer);
                            bufferlen.Add(count);
                            Thread.Sleep(100);
                        }
                        catch (TimeoutException)
                        {
                        }
                    }
                }
            }
        }



        // 2017-06-25 新增TCP协议   star 
        private void StartListen()
        {
            if (_receiveThread == null)
            {
                _receiveThread = new Thread(MainListen);
                _receiveThread.Start();
            }
            else
            {
                _receiveThread.Abort();
                _receiveThread = null;
                _receiveThread = new Thread(MainListen);
                _receiveThread.Start();
            }
        }
        private void MainListen()
        {
            if (m_local == null)
            {
                m_local = new IPEndPoint(IPAddress.Parse(GetLocalIp()), m_port);
                m_localep = new IPEndPoint(IPAddress.Parse(GetLocalIp()), m_port) as EndPoint;
            }
            //m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //m_socket.Bind(m_localep);
            //m_socket.Listen(100000);
            TcpListener tcpl = new TcpListener(m_local.Address, m_port);
            tcpl.Start();
            while (true)
            {
                try
                {
                    // 得到包含客户端信息的套接字
                    //Socket client = m_socket.Accept();
                    Socket client = tcpl.AcceptSocket();
                    byte[] buffer = new byte[256];
                    byte[] macno = new byte[2];
                    int len = client.Receive(buffer);
                    Array.Copy(buffer, 6, macno, 0, 2);
                    if (MachineFinded != null)
                    {
                        MachineFinded(client.RemoteEndPoint.ToString().Split(':')[0] + "|" + client.RemoteEndPoint.ToString().Split(':')[1] + ":" + byteTostring(macno));
                    }
                    bool isnew = true;
                    string ip = client.RemoteEndPoint.ToString();
                    for (int i = 0; i < sockets.Count; i++)
                    {
                        if (sockets[i].ip == ip)
                        {
                            isnew = false;
                            break;
                        }
                    }
                    if (isnew)
                    {
                        //把ClientThread 类的ClientService方法委托给线程
                        Thread newthread = new Thread(Listen);
                        TcpSocket socket = new TcpSocket();
                        socket.socket = client;
                        socket.ip = ip;
                        socket.thread = newthread;
                        socket.received = new List<byte[]>();
                        socket.command = new List<byte[]>();
                        sockets.Add(socket);
                        // 启动消息服务线程
                        newthread.Start(client);
                    }
                    SynBuffer(buffer, new IPEndPoint(IPAddress.Parse(client.RemoteEndPoint.ToString().Split(':')[0]), int.Parse(client.RemoteEndPoint.ToString().Split(':')[1])), len);
                }
                catch (Exception e) { string s = e.Message; }
                Thread.Sleep(100);
            }
        }
        private void Listen(object o)
        {
            Socket server = o as Socket;
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[2048];
                    int len;
                    len = server.Receive(buffer);
                    if (len > 0) SynBuffer(buffer, new IPEndPoint(IPAddress.Parse(server.RemoteEndPoint.ToString().Split(':')[0]), int.Parse(server.RemoteEndPoint.ToString().Split(':')[1])), len);
                    foreach (TcpSocket ts in sockets)
                    {
                        if (ts.ip == server.RemoteEndPoint.ToString())
                        {
                            DateTime start = DateTime.Now;
                            while (DateTime.Compare(start.AddMilliseconds(100), DateTime.Now) > 0)
                            {
                                if (ts.command.Count > 0)
                                {
                                    foreach (byte[] command in ts.command)
                                    {
                                        server.Send(command);
                                        ts.command.Remove(command);
                                    }
                                    break;
                                }
                                Thread.Sleep(10);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ReceivedError != null)
                    ReceivedError(ex.Message);
            }
        }
        private void DestroySocket(Socket socket)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            socket.Close();
        }


        //   end













        private void StartReceive()
        {
            if (_receiveThread == null)
            {
                _receiveThread = new Thread(Receive);
                _receiveThread.Start();
            }
            else
            {
                _receiveThread.Abort();
                _receiveThread = null;
                _receiveThread = new Thread(Receive);
                _receiveThread.Start();
            }
        }
        private delegate void mSynBuffer(byte[] buffer, EndPoint remoteEP, int len);


        private void Receive()
        {

            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    EndPoint remoteEP = (EndPoint)(new IPEndPoint(IPAddress.Any, 8005));
                    int len;
                    len = m_socket.ReceiveFrom(buffer, ref remoteEP);
                    mSynBuffer m1 = new mSynBuffer(SynBuffer);
                    m1.BeginInvoke(buffer, remoteEP, len, null, null);
                    // SynBuffer(buffer, remoteEP, len);
                }
                catch (Exception ex)
                {
                    if (ReceivedError != null)
                        ReceivedError(ex.Message);
                }
            }

        }
        private void SynBuffer(byte[] buffer, EndPoint remoteEP, int len)
        {
            if (buffer[2] != 0x00 || buffer[3] != 0x00)
            {
                byte[] data = new byte[4], macno = new byte[2], mport = new byte[2];
                string strdata;
                Array.Copy(buffer, 17, data, 0, 4);
                Array.Copy(buffer, 2, macno, 0, 2);
                Array.Copy(buffer, 29, mport, 0, 2);
                strdata = GetIP(data) + "|" + byteToint(mport).ToString() + ":" + byteTostring(macno);
                if (MachineFinded != null)
                {
                    MachineFinded(strdata);
                }
            }
            else
            {
                if (buffer[4] == 0x00 && buffer[5] == 0x01)
                {
                    MonitorParm mp = new MonitorParm(buffer, (IPEndPoint)remoteEP, len);
                    Thread tmonitor = new Thread(MonitorX);
                    tmonitor.Start(mp);
                    //MonitorX(buffer, (IPEndPoint)remoteEP, len);
                }
                else
                {
                    if (buffer[2] == 0x00 && buffer[3] == 0x00 && buffer[10] == 0x09)
                    {
                        //Monitor(buffer, (IPEndPoint)remoteEP, len);
                        MonitorParm mp = new MonitorParm(buffer, (IPEndPoint)remoteEP, len);
                        Thread tmonitor = new Thread(Monitor);
                        tmonitor.Start(mp);
                    }
                    else
                    {
                        savebuffer.Add(buffer);
                        bufferlen.Add(len);
                    }
                }
            }
        }











        public void SendData(byte[] data, string remote)
        {
            switch (commtype)
            {
                case 1://TCP/IP
                    foreach (TcpSocket soc in sockets)
                    {
                        if (soc.ip.Split(':')[0] == remote.Split('|')[0])
                        {
                            soc.command.Add(data);
                        }
                    }
                    break;
                case 2://串口
                    if (serialPort.IsOpen)
                        serialPort.Write(data, 0, data.Length);
                    break;
                default:
                    if (remote.Split('|').Length > 1)
                        m_socket.SendTo(data, new IPEndPoint(IPAddress.Parse(remote.Split('|')[0]), int.Parse(remote.Split('|')[1])));
                    else
                        m_socket.SendTo(data, new IPEndPoint(IPAddress.Parse(remote), m_mport));
                    break;
            }
        }
        private void SendData(byte[] data, IPAddress remoteIP, int port)
        {
            SendData(data, remoteIP.ToString() + "|" + port.ToString());
        }
        //private void Receive()  //2017-06-25注销
        //{
        //    try
        //    {
        //        byte[] buffer = new byte[2048];
        //        EndPoint remoteEP = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
        //        int len;
        //        while (true)
        //        {
        //            len = m_socket.ReceiveFrom(buffer, ref remoteEP);
        //            if (buffer[2] != 0x00 || buffer[3] != 0x00)
        //            {
        //                byte[] data = new byte[4], macno = new byte[2],mport=new byte[2];
        //                string strdata;
        //                Array.Copy(buffer, 17, data, 0, 4);
        //                Array.Copy(buffer, 2, macno, 0, 2);
        //                Array.Copy(buffer, 29, mport, 0, 2);
        //                strdata = GetIP(data) +"|"+byteToint(mport).ToString()+ ":" + byteTostring(macno);
        //                if (MachineFinded != null)
        //                {
        //                    MachineFinded(strdata);
        //                }
        //            }
        //            else
        //            {
        //                if (buffer[4] == 0x00 && buffer[5] == 0x01)
        //                {
        //                    MonitorParm mp = new MonitorParm(buffer, (IPEndPoint)remoteEP, len);
        //                    Thread tmonitor = new Thread(MonitorX);
        //                    tmonitor.Start(mp);
        //                    //MonitorX(buffer, (IPEndPoint)remoteEP, len);
        //                }
        //                else
        //                {
        //                    if (buffer[2] == 0x00 && buffer[3] == 0x00 && buffer[10] == 0x09)
        //                    {
        //                        //Monitor(buffer, (IPEndPoint)remoteEP, len);
        //                        MonitorParm mp = new MonitorParm(buffer, (IPEndPoint)remoteEP, len);
        //                        Thread tmonitor = new Thread(Monitor);
        //                        tmonitor.Start(mp);
        //                    }
        //                    else
        //                    {
        //                        savebuffer.Add(buffer);
        //                        bufferlen.Add(len);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ReceivedError != null)
        //            ReceivedError(ex.Message);
        //    }
        //}
        private int pullbuffer()     //这个方法在新的服务中取消   没有用
        {
            int pulled = 0;
            Thread.Sleep(100);
            while (bufferlen.Count > 0)
            {
                //rtbShow.AppendText(Encoding.UTF8.GetString(savebuffer[0], 0, bufferlen[0]) + '\n');
                //rtbShow.AppendText(Encoding.ASCII.GetString(savebuffer[0]));
                savebuffer.RemoveAt(0);
                bufferlen.RemoveAt(0);
                pulled++;
            }
            return pulled;
        }



        private int GetFixSum(byte[] data)
        {
            int sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }
            string sumstr = sum.ToString("X2");
            return Convert.ToInt32(sumstr.Substring(sumstr.Length - 2), 16);
        }
        private byte[] stringTobyte(string str)
        {
            byte[] bytes = new byte[str.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(str.Substring(i * 2, 2),
                System.Globalization.NumberStyles.HexNumber);
            }
            return bytes;
        }
        private string byteTostring(byte[] data)
        {
            string str = "";
            for (int i = 0; i < data.Length; i++)
            {
                byte[] idata = new byte[2];
                Array.Copy(data, i, idata, 0, 1);
                str = str + intTohexstring(BitConverter.ToInt16(idata, 0));
            }
            return str;
        }
        private int byteToint(byte[] data)
        {
            int ret = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte[] idata = new byte[2];
                Array.Copy(data, i, idata, 0, 1);
                ret = ret + BitConverter.ToInt16(idata, 0) * (int)(Math.Pow(256, data.Length - i - 1));
            }
            return ret;
        }
        private long byteTolong(byte[] data)
        {
            long ret = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte[] idata = new byte[2];
                Array.Copy(data, i, idata, 0, 1);
                ret = ret + BitConverter.ToInt16(idata, 0) * (long)(Math.Pow(256, data.Length - i - 1));
            }
            return ret;
        }
        private string intTohexstring(int i)
        {
            string str = "";
            if (i / 256 > 0)
            {
                str = intTohexstring(i / 256);
                i = i % 256;
            }
            switch (i / 16)
            {
                case 10:
                    str = str + "A";
                    break;
                case 11:
                    str = str + "B";
                    break;
                case 12:
                    str = str + "C";
                    break;
                case 13:
                    str = str + "D";
                    break;
                case 14:
                    str = str + "E";
                    break;
                case 15:
                    str = str + "F";
                    break;
                default:
                    str = str + (i / 16).ToString();
                    break;
            }
            i = i % 16;
            switch (i)
            {
                case 10:
                    str = str + "A";
                    break;
                case 11:
                    str = str + "B";
                    break;
                case 12:
                    str = str + "C";
                    break;
                case 13:
                    str = str + "D";
                    break;
                case 14:
                    str = str + "E";
                    break;
                case 15:
                    str = str + "F";
                    break;
                default:
                    str = str + i.ToString();
                    break;
            }
            return str;
        }
        private string GetIP(byte[] datas)
        {
            byte[] data = new byte[8];
            Array.Copy(datas, 0, data, 0, 1);
            Array.Copy(datas, 1, data, 2, 1);
            Array.Copy(datas, 2, data, 4, 1);
            Array.Copy(datas, 3, data, 6, 1);
            string strdata = BitConverter.ToInt16(data, 0).ToString() + "." + BitConverter.ToInt16(data, 2).ToString() + "." + BitConverter.ToInt16(data, 4).ToString() + "." + BitConverter.ToInt16(data, 6).ToString();
            return strdata;
        }
        private byte[] IPstringTobyte(string ip)
        {
            string[] str = ip.Split('.');
            byte[] data = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(int.Parse(str[i])), 0, data, i, 1);
            }
            return data;
        }
        private string GetTime(byte[] datas)
        {
            string strTime = (datas[0] * 256 + datas[1]).ToString() + "-" + datas[2].ToString().PadLeft(2, '0') + "-"
                + datas[3].ToString().PadLeft(2, '0') + " " + datas[4].ToString().PadLeft(2, '0') + ":" + datas[5].ToString().PadLeft(2, '0')
                + ":" + datas[6].ToString().PadLeft(2, '0') + ",星期:" + datas[7].ToString();
            return strTime;
        }
        private string GetByteTime()
        {
            string time = null;
            DateTime dt = DateTime.Now;//.AddHours(-1);
            time = intTohexstring(dt.Year) + intTohexstring(dt.Month) + intTohexstring(dt.Day) + intTohexstring(dt.Hour)
                + intTohexstring(dt.Minute) + intTohexstring(dt.Second);
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    time = time + intTohexstring(5);
                    break;
                case DayOfWeek.Monday:
                    time = time + intTohexstring(1);
                    break;
                case DayOfWeek.Saturday:
                    time = time + intTohexstring(6);
                    break;
                case DayOfWeek.Sunday:
                    time = time + intTohexstring(7);
                    break;
                case DayOfWeek.Thursday:
                    time = time + intTohexstring(4);
                    break;
                case DayOfWeek.Tuesday:
                    time = time + intTohexstring(2);
                    break;
                case DayOfWeek.Wednesday:
                    time = time + intTohexstring(3);
                    break;
                default:
                    break;
            }
            return time;
        }
        private byte[] Getcommandline()
        {
            string commands = m_syn + m_res + m_macaddr + m_slen + m_command + m_appdata;
            commandline = new byte[14 + m_appdata.Length];
            commandline = stringTobyte(commands + getCRC16(commands));
            return commandline;
        }
        private byte[] Getcommandline(string syn, string res, string macaddr, string slen, string command, string appdata)
        {
            string commands = syn + res + macaddr + slen + command + appdata;
            return stringTobyte(commands + getCRC16(commands));
        }
        private byte[] Getcommandline(string sierialNO)
        {
            string commands = m_syn + "00000001" + sierialNO + m_slen + m_command + m_appdata;
            commandline = new byte[14 + m_appdata.Length];
            commandline = stringTobyte(commands + getCRC16(commands));
            return commandline;
        }
        private byte[] Getcommandline(string syn, string sierialNO, string slen, string command, string appdata)
        {
            string commands = syn + "0000000" + (sierialNO.Length > 4 ? "1" : "0") + sierialNO + slen + command + appdata;
            return stringTobyte(commands + getCRC16(commands));
        }

        private int GetReturn(string ip, byte[] command, int type, int sno, ref byte[] rdata)
        {
            SendData(command, ip.Split(':')[0]);
            DateTime start = DateTime.Now;
            int diff = m_diff;
            if (type == 0x0F && sno == 0x02) diff = 10;
            while (DateTime.Compare(start.AddSeconds(m_diff), DateTime.Now) > 0)
            {
                try
                {
                    for (int index = 0; index < savebuffer.Count; index++)
                    {
                        if (savebuffer[index][2] == 0x00 && savebuffer[index][3] == 0x00 && savebuffer[index][ctypeindex] == type && savebuffer[index][cnoindex] == sno)
                        {
                            rdata = savebuffer[index];
                            int len = bufferlen[index];
                            savebuffer.RemoveAt(index);
                            bufferlen.RemoveAt(index);
                            return len;
                        }
                    }
                }
                catch { }
            }
            return 0;
        }
        private string getmacno(byte[] data)
        {
            int len = getaddrlength(data);
            byte[] mac = new byte[len];
            Array.Copy(data, 6, mac, 0, len);
            string macno = "";
            if (len > 4)
                macno = byteTostring(mac);
            else
                macno = byteToint(mac).ToString("X2").PadLeft(len * 2, '0');
            return macno;
        }
        private int getaddrlength(byte[] data)
        {
            switch (data[5])
            {
                case 1:
                    return 8;
                case 2:
                    return 4;
                default:
                    return 2;
            }

        }



        #endregion
        #region 事件说明
        public delegate bool MachineDataReceived(string data);
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event MachineDataReceived DataReceived;
        public delegate void MachineDataReceivedError(string errmsg);
        /// <summary>
        /// 接收错误事件
        /// </summary>
        public event MachineDataReceivedError ReceivedError;
        public delegate CardBlashResult MachineCardBlashed(IPAddress ip, int type, int flowno, long cardno, int amount, int dealtype);
        /// <summary>
        /// 刷卡事件
        /// </summary>
        public event MachineCardBlashed CardBlashed;
        public delegate int MachineCardBlashedIC(IPAddress ip, ref string[] parms);
        public event MachineCardBlashedIC CardBlashedIC;
        public delegate bool MachineDataReceivedX(string[] data);
        /// <summary>
        /// 校情通接收记录事件
        /// </summary>
        public event MachineDataReceivedX DataReceivedX;
        public delegate void MachinePhotoReceivedX(int macno, int flowno);
        /// <summary>
        /// 校情通接收照片事件
        /// </summary>
        public event MachinePhotoReceivedX PhotoReceivedX;
        public delegate void MachineBeFinded(string ip);
        public event MachineBeFinded MachineFinded;
        public delegate void MachineLogoned(string ip, int index);
        public event MachineLogoned MachineLoged;
        public delegate void MachineDump(string ip);
        public event MachineDump MachineDumped;
        public delegate int QueryApply(string ip, ref int[] row, ref int[] col, ref string[] message, ref int seconds);
        public event QueryApply QueryApplyed;

        public delegate void DataCompare(string ip, byte[] data);
        public event DataCompare DataCompareUpload;
        public delegate int DealApplyJZ(string ip, string data, byte[] secdata, ref int[] com, ref string[] content);
        public event DealApplyJZ DealApplyedJ;


        #endregion
        #region CRC检验码生成
        private String getCRC16(String Source)
        {
            int crc = 0xA1EC;          // initial value
            int polynomial = 0x1021;   // 0001 0000 0010 0001  (0, 5, 12) 
            String tmp = "";
            byte[] bytes = new byte[Source.Length / 2];
            for (int i = 0; i < Source.Length - 1; i++)
            {
                if (i % 2 == 0)
                {
                    tmp = Source.Substring(i, 2);
                    bytes[i / 2] = (byte)Int16.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
                }
            }
            foreach (byte b in bytes)
            {
                for (int i = 0; i < 8; i++)
                {
                    bool bit = ((b >> (7 - i) & 1) == 1);
                    bool c15 = ((crc >> 15 & 1) == 1);
                    crc <<= 1;
                    if (c15 ^ bit) crc ^= polynomial;
                }
            }
            crc &= 0xffff;
            string strDest = crc.ToString("X").PadLeft(4, '0');
            return strDest;
        }

        #endregion
        /// <summary>
        /// 搜索机器   多机器连接下不适用
        /// </summary>
        /// <returns>返回机器列表（每个机器为ip:机号的格式）</returns>
        public void SearchMachine()//搜机
        {
            try
            {
                // 连接后传送一个消息给ip主机 
                Byte[] sendBytes = stringTobyte("68");

                SendData(sendBytes, new IPAddress(0xFFFFFFFF), 0xFFFF);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        #region IP设置部分(00)
        /// <summary>
        /// 与机器握手
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>整型，0为机器无密钥，1为握手成功，其他为失败</returns>
        public int ShakeHand(string ip)//握手
        {
            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, m_macaddr, m_slen, m_command, m_appdata), 0, 1, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 19, data, 0, 1);
                return BitConverter.ToInt16(data, 0);
            }
            return 2;
        }
        /// <summary>
        /// 取机器地址
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>返回的机器号</returns>
        public string GetMacAddr(string ip)//取机器地址0002
        {
            string slen = "0002", command = "0002", appdata = "", macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0, 2, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[8];
                Array.Copy(rdata, cnoindex + 3, data, 0, 8);
                return byteTostring(data);
            }
            return "";
        }
        /// <summary>
        /// 设置机器地址
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="addr">设置的机器地址</param>
        /// <returns>返回的机器地址</returns>
        public string SetMacAddr(string ip, int addr)//设置机器地址0082
        {
            string slen = "0004", command = "0082", appdata = intTohexstring(addr);
            appdata = ("00" + appdata).Substring(appdata.Length - 2);
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x82, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[8];
                Array.Copy(rdata, cnoindex + 3, data, 0, 8);
                return byteTostring(data);
            }
            return "";
        }
        /// <summary>
        /// 取机器IP地址
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>返回的结果依次为：IP地址、子网掩码、网关、端口、MAC地址、是否自动(00表示指定，01表示自动)</returns>
        public string[] GetIP(string ip)//取机器IP0003
        {
            string[] addr = new string[6];
            string slen = "0002";
            string command = "0003";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4], mac = new byte[6], port = new byte[2], auto = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                addr[0] = GetIP(data);
                Array.Copy(rdata, cnoindex + 6, data, 0, 4);
                addr[1] = GetIP(data);
                Array.Copy(rdata, cnoindex + 10, data, 0, 4);
                addr[2] = GetIP(data);
                Array.Copy(rdata, cnoindex + 14, port, 0, 2);
                addr[3] = byteToint(port).ToString();
                Array.Copy(rdata, cnoindex + 16, mac, 0, 6);
                addr[4] = byteTostring(mac);
                Array.Copy(rdata, cnoindex + 1, auto, 0, 1);
                addr[5] = byteTostring(auto);

            }
            return addr;
        }
        /// <summary>
        /// 设置机器IP地址
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="ips">设置的内容依次为：IP地址、子网掩码、网关、端口、MAC地址、是否自动(00表示指定，01表示自动)</param>
        /// <returns>返回的结果依次为：IP地址、子网掩码、网关、端口、MAC地址、是否自动(00表示指定，01表示自动)</returns>
        public string SetIP(string ip, string[] ips)//设置机器IP0083
        {
            string slen = "0011";
            string command = "0083";
            string appdata = ips[5];
            appdata = appdata + byteTostring(IPstringTobyte(ips[0]));
            appdata = appdata + byteTostring(IPstringTobyte(ips[1]));
            appdata = appdata + byteTostring(IPstringTobyte(ips[2]));
            appdata = appdata + intTohexstring(int.Parse(ips[3]));
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x83, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 16, data, 0, 6);
                return byteTostring(data);
            }
            return "";
        }
        /// <summary>
        /// 取服务器IP地址
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>返回数组，0为IP，1为端口，2为服务器名</returns>
        public string[] GetRemoteIP(string ip)//取服务器IP0004
        {
            string[] addr = new string[3];
            string slen = "0002";
            string command = "0004";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x04, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4], mac = new byte[64], port = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                addr[0] = GetIP(data);
                Array.Copy(rdata, cnoindex + 5, port, 0, 2);
                addr[1] = byteToint(port).ToString();
                if (rlen > 20)
                {
                    Array.Copy(rdata, cnoindex + 7, mac, 0, rlen - 20);
                    //addr[2] = byteTostring(mac);
                    addr[2] = Encoding.ASCII.GetString(mac);
                }
            }
            return addr;
        }
        /// <summary>
        /// 设置服务器IP
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="ips">设置的IP数组：0为IP，1为端口</param>
        /// <returns>返回的IP</returns>
        public string SetRemoteIP(string ip, string[] ips)//设置服务器IP0084
        {
            string command = "0084";
            string appdata = "";
            appdata = appdata + byteTostring(IPstringTobyte(ips[0]));
            appdata = appdata + intTohexstring(int.Parse(ips[1]));
            appdata = appdata + "00";// byteTostring(Encoding.ASCII.GetBytes(ips[2]));
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x84, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                return GetIP(data);
            }
            return "";
        }
        /// <summary>
        /// 取交易服务器IP
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>返回数组，0为IP，1为端口，2为服务器名</returns>
        public string[] GetDealIP(string ip)//取交易IP0005
        {
            string[] addr = new string[3];
            string slen = "0002";
            string command = "0005";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x05, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4], mac = new byte[64], port = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                addr[0] = GetIP(data);
                Array.Copy(rdata, cnoindex + 5, port, 0, 2);
                addr[1] = byteToint(port).ToString();
                if (rlen > 20)
                {
                    Array.Copy(rdata, cnoindex + 7, mac, 0, rlen - 20);
                    //addr[2] = byteTostring(mac);
                    addr[2] = Encoding.ASCII.GetString(mac);
                }
                return addr;
            }
            return addr;
        }
        /// <summary>
        /// 设置交易服务器IP
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="ips">设置的IP数组：0为IP，1为端口</param>
        /// <returns>返回的IP</returns>
        public string SetDealIP(string ip, string[] ips)//设置交易IP0085
        {
            string command = "0085";
            string appdata = "";
            appdata = appdata + byteTostring(IPstringTobyte(ips[0]));
            appdata = appdata + intTohexstring(int.Parse(ips[1]));
            appdata = appdata + "00";//byteTostring(Encoding.ASCII.GetBytes(ips[2]));
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x85, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                return GetIP(data);
            }
            return "";
        }
        /// <summary>
        /// 取DNS
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>数组，0为主DNS，1为备用DNS</returns>
        public string[] GetCommDNS(string ip)//取DNS0006
        {
            string[] addr = new string[2];
            string slen = "0002";
            string command = "0006";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x06, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                addr[0] = GetIP(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                addr[1] = GetIP(data);
            }
            return addr;
        }
        /// <summary>
        /// 设置DNS
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="ips">指定的DNS，0为主DNS，1为备用DNS</param>
        /// <returns>数组，0为主DNS，1为备用DNS</returns>
        public string[] SetCommDNS(string ip, string[] ips)//设置DNS0086
        {
            string[] addr = new string[2];

            string command = "0086";
            string appdata = byteTostring(IPstringTobyte(ips[0]));
            appdata = appdata + byteTostring(IPstringTobyte(ips[1]));
            string macaddr = ip.Split(':')[1];
            string slen = "000A";

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x86, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                addr[0] = GetIP(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                addr[1] = GetIP(data);
                return addr;
            }
            return addr;
        }
        /// <summary>
        /// 取传输协议
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>机器返回协议字符串：00000100为联机，00000000为脱机</returns>
        public string GetCommProtocol(string ip)//取传输协议0007
        {
            string addr = "";
            string slen = "0002";
            string command = "0007";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x07, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                return Convert.ToString(data[0], 2).PadLeft(8, '0');
            }
            return addr;
        }
        /// <summary>
        /// 设置传输协议
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="pro">要设置的协议字符串：00000100为联机，00000000为脱机</param>
        /// <returns>机器返回协议字符串：的00000100为联机，00000000为脱机</returns>
        public string SetCommProtocol(string ip, string pro)//设置传输协议(接收2进制字符串)0087
        {

            string command = "0087";
            string appdata = Convert.ToInt16(pro, 2).ToString("X2");
            string macaddr = ip.Split(':')[1];
            string slen = "0003";

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x87, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                return Convert.ToString(data[0], 2).PadLeft(8, '0');
            }
            return "";
        }
        public int[] GetBSCommProtocol(string ip)//取BS协议通信参数0008
        {
            int[] bscp = new int[3];
            string slen = "0002";
            string command = "0008";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x08, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                bscp[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 2);
                bscp[1] = byteToint(data);
                data = new byte[1];
                Array.Copy(rdata, cnoindex + 5, data, 0, 1);
                bscp[3] = byteToint(data);
            }
            return bscp;
        }
        /// <summary>
        /// 设置BS协议通信参数
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="pro">要设置的协议通信参数：依次为心跳阀值、超时重发时间、超时重发次数</param>
        /// <returns>机器返回协议通信参数：依次为心跳阀值、超时重发时间、超时重发次数</returns>
        public int[] SetBSCommProtocol(string ip, string pro)//设置BS协议通信参数0088
        {
            int[] bscp = new int[3];

            string command = "0088";
            string appdata = Convert.ToInt16(pro, 2).ToString("X2");
            string macaddr = ip.Split(':')[1];
            string slen = "0003";

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x88, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                bscp[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 2);
                bscp[1] = byteToint(data);
                data = new byte[1];
                Array.Copy(rdata, cnoindex + 5, data, 0, 1);
                bscp[3] = byteToint(data);
            }
            return bscp;
        }
        /// <summary>
        /// 取机器类型
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>数组：0为设备类型，1为软件版本号，2为字符串</returns>
        public string[] GetType(string ip)//取机器类型0009
        {
            string[] addr = new string[3];
            string slen = "0002";
            string command = "0009";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x09, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                addr[0] = byteTostring(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                addr[1] = byteTostring(data);
                data = new byte[rlen - 22];
                Array.Copy(rdata, cnoindex + 9, data, 0, data.Length);
                addr[2] = Encoding.Default.GetString(data);
            }
            return addr;
        }
        /// <summary>
        /// 设置机器类型
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="type">机器类型</param>
        /// <returns>数组：0为设备类型，1为软件版本号，2为字符串</returns>
        public string SetType(string ip, string type)//设置机器类型0089
        {
            string command = "0089";
            string appdata = Convert.ToInt16(type, 2).ToString("X2");
            string macaddr = ip.Split(':')[1];
            string slen = "0006";

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x89, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                return Convert.ToString(data[0], 2).PadLeft(8, '0');
            }
            return "";
        }
        public string SetKey(string ip, string key)//设置通信验证密钥00C0
        {
            string command = "00C0";
            string appdata = key;
            string macaddr = ip.Split(':')[1];
            string slen = "000E";

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0xC0, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                return Convert.ToString(data[0], 2).PadLeft(8, '0');
            }
            return "";
        }
        /// <summary>
        /// 读串口通信波特率
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="port">串口号</param>
        /// <returns>整型波特率</returns>
        public int GetComRate(string ip, int port)//读串口通信波特率000A
        {
            string slen = "0003";
            string command = "000A";
            string appdata = port.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x0A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                return byteToint(data);
            }
            return -1;
        }
        /// <summary>
        /// 设置串口通信波特率
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="port">串口号</param>
        /// <param name="rate">整型波特率</param>
        /// <returns>整型波特率</returns>
        public int SetComRate(string ip, int port, int rate)//设置串口通信波特率008A
        {
            string command = "008A";
            string appdata = port.ToString("X2").PadLeft(2, '0') + Convert.ToInt32(rate).ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];
            string slen = "0007";

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x8A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                return byteToint(data);
            }
            return -1;
        }
        /// <summary>
        /// 读能龙设备系列号
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>能龙设备系列号</returns>
        public string GetSeriesNo(string ip)//读能龙设备系列号000B
        {
            string slen = "0002";
            string command = "000B";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x0B, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[18];
                Array.Copy(rdata, cnoindex + 1, data, 0, 18);
                return Encoding.ASCII.GetString(data);
            }
            return null;
        }
        /// <summary>
        /// 设置能龙设备系列号
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="series">系列号</param>
        /// <returns>系列号</returns>
        public string SetSeriesNo(string ip, string series)//设置能龙设备系列号000B
        {
            string command = "008B";
            string appdata = byteTostring(Encoding.ASCII.GetBytes(series.PadRight(18, ' ')));
            string macaddr = ip.Split(':')[1];
            string slen = "0014";

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x00, 0x8B, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[18];
                Array.Copy(rdata, cnoindex + 1, data, 0, 18);
                return Encoding.ASCII.GetString(data);
            }
            return null;
        }
        #endregion

        #region 机器通用部分(01)
        /// <summary>
        /// 取时间
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>机器时间</returns>
        public string GetTime(string ip)//取时间0101
        {
            string slen = "0002";
            string command = "0101";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[8];
                Array.Copy(rdata, cnoindex + 1, data, 0, 8);
                return GetTime(data);
            }
            return null;
        }
        /// <summary>
        /// 设置时间  以电脑当前时间更新至卡机
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <returns>布尔值，表示是否成功</returns>
        public int SetTime(string ip)//设置时间0181
        {
            string slen = "000A";
            string command = "0181";
            string appdata = GetByteTime();
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x81, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[8];
                Array.Copy(rdata, cnoindex + 1, data, 0, 8);
                string time = byteTostring(data);
                if (time == appdata)
                    return 0;
                else
                    return 1;
            }
            return -1;
        }
        public int[] GetInterval(string ip)//读打卡时间间隔控制0102
        {
            int[] interval = new int[2];
            string slen = "0002";
            string command = "0102";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                interval[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 2);
                interval[1] = byteToint(data);
                return interval;
            }
            return interval;
        }
        public int[] SetInterval(string ip, ushort[] inter)//设置打卡时间间隔控制0182
        {
            int[] interval = new int[2];
            string slen = "0006";
            string command = "0182";
            string appdata = intTohexstring(inter[0]).PadLeft(4, '0') + intTohexstring(inter[1]).PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x82, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                interval[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 2);
                interval[1] = byteToint(data);
                return interval;
            }
            return interval;
        }
        public string[] GetWorkType(string ip)//读设备工作模式0103
        {
            string[] worktype = new string[8];
            string slen = "0002";
            string command = "0103";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                worktype[0] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                worktype[1] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 3, data, 0, 1);
                worktype[2] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 4, data, 0, 1);
                worktype[3] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 5, data, 0, 1);
                worktype[4] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 6, data, 0, 1);
                worktype[5] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 7, data, 0, 1);
                worktype[6] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 8, data, 0, 1);
                worktype[7] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                return worktype;
            }
            return worktype;
        }
        public string[] SetWorkType(string ip, string[] type)//设置设备工作模式0183
        {
            string[] worktype = new string[8];
            string slen = "000A";
            string command = "0183";
            string appdata = Convert.ToInt16(type[0], 2).ToString("X2") + Convert.ToInt16(type[1], 2).ToString("X2");
            appdata += Convert.ToInt16(type[2], 2).ToString("X2") + Convert.ToInt16(type[3], 2).ToString("X2");
            appdata += Convert.ToInt16(type[4], 2).ToString("X2") + Convert.ToInt16(type[5], 2).ToString("X2");
            appdata += Convert.ToInt16(type[6], 2).ToString("X2") + Convert.ToInt16(type[7], 2).ToString("X2");
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x83, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                worktype[0] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                worktype[1] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 3, data, 0, 1);
                worktype[2] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 4, data, 0, 1);
                worktype[3] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 5, data, 0, 1);
                worktype[4] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 6, data, 0, 1);
                worktype[5] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 7, data, 0, 1);
                worktype[6] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                Array.Copy(rdata, cnoindex + 8, data, 0, 1);
                worktype[7] = Convert.ToString(byteToint(data), 2).PadLeft(8, '0');
                return worktype;
            }
            return worktype;
        }
        public ManageCard GetManageCard(string ip, int cardindex)//读管理员卡号0104
        {
            ManageCard card = new ManageCard();
            string slen = "0003";
            string command = "0104";
            string appdata = cardindex.ToString("X2");
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x04, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                card.index = byteToint(data);
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                card.usetype = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 1);
                card.usertype = byteToint(data);
                data = new byte[8];
                Array.Copy(rdata, cnoindex + 4, data, 0, 8);
                card.card = byteToint(data);
                return card;
            }
            return card;
        }
        public ManageCard SetManageCard(string ip, ManageCard mcard)//设置管理员卡号0184
        {
            ManageCard card = new ManageCard();
            if (mcard.password.ToString().Length < 6) return card;
            string slen = "0010";
            string command = "0184";
            string appdata = mcard.index.ToString("X2") + mcard.password.ToString();
            appdata += mcard.usetype.ToString("X2") + mcard.usertype.ToString("X2");
            appdata += mcard.card.ToString("X2").PadLeft(16, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x84, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                card.index = byteToint(data);
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                card.usetype = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 1);
                card.usertype = byteToint(data);
                data = new byte[8];
                Array.Copy(rdata, cnoindex + 4, data, 0, 8);
                card.card = byteToint(data);
                return card;
            }
            return card;
        }
        public TimeSeperate[] GetTimeSeperate(string ip)//读时间段参数0105        
        {
            TimeSeperate[] time = new TimeSeperate[0];
            string slen = "0002";
            string command = "0105";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x05, ref rdata);
            if (rdata.Length > 0)
            {
                int len;
                if (rdata[cnoindex + 1] == 0)
                    len = 0;
                else
                    len = (rlen - 15) / rdata[cnoindex + 1];
                time = new TimeSeperate[rdata[cnoindex + 1]];
                for (int count = 0; count < rdata[cnoindex + 1]; count++)
                {
                    byte[] data = new byte[2];
                    Array.Copy(rdata, cnoindex + 2 + count * len, data, 0, 2);
                    time[count].start = data[0].ToString().PadLeft(2, '0') + data[1].ToString().PadLeft(2, '0');
                    Array.Copy(rdata, cnoindex + 4 + count * len, data, 0, 2);
                    time[count].end = data[0].ToString().PadLeft(2, '0') + data[1].ToString().PadLeft(2, '0');
                    data = new byte[1];
                    Array.Copy(rdata, cnoindex + 6 + count * len, data, 0, 1);
                    time[count].xtype = byteToint(data);
                    data = new byte[len - 5];
                    Array.Copy(rdata, cnoindex + 7 + count * len, data, 0, len - 5);
                    time[count].workparm = byteTostring(data);
                }
                return time;
            }
            return null;
        }
        public TimeSeperate[] SetTimeSeperate(string ip, TimeSeperate[] times)//设置时间段参数0185
        {
            TimeSeperate[] time = new TimeSeperate[0];
            string command = "0185";
            string appdata = times.Length.ToString("X2").PadLeft(2, '0');
            for (int aa = 0; aa < times.Length; aa++)
            {
                appdata += int.Parse(times[aa].start.Substring(0, 2)).ToString("X2").PadLeft(2, '0');
                if (times[aa].start.Length == 4)
                    appdata += int.Parse(times[aa].start.Substring(2, 2)).ToString("X2").PadLeft(2, '0');
                else
                    appdata += int.Parse(times[aa].start.Substring(3, 2)).ToString("X2").PadLeft(2, '0');
                appdata += int.Parse(times[aa].end.Substring(0, 2)).ToString("X2").PadLeft(2, '0');
                if (times[aa].end.Length == 4)
                    appdata += int.Parse(times[aa].end.Substring(2, 2)).ToString("X2").PadLeft(2, '0');
                else
                    appdata += int.Parse(times[aa].end.Substring(3, 2)).ToString("X2").PadLeft(2, '0');
                appdata += times[aa].xtype.ToString("X2").PadLeft(2, '0');
                appdata += times[aa].workparm;
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x85, ref rdata);
            if (rdata.Length > 0)
            {
                int len;
                if (rdata[cnoindex + 1] == 0)
                    len = 0;
                else
                    len = (rlen - 15) / rdata[cnoindex + 1];
                time = new TimeSeperate[rdata[cnoindex + 1]];
                for (int count = 0; count < rdata[cnoindex + 1]; count++)
                {
                    byte[] data = new byte[2];
                    Array.Copy(rdata, cnoindex + 2 + count * len, data, 0, 2);
                    time[count].start = data[0].ToString().PadLeft(2, '0') + data[1].ToString().PadLeft(2, '0');
                    Array.Copy(rdata, cnoindex + 4 + count * len, data, 0, 2);
                    time[count].end = data[0].ToString().PadLeft(2, '0') + data[1].ToString().PadLeft(2, '0');
                    data = new byte[1];
                    Array.Copy(rdata, cnoindex + 6 + count * len, data, 0, 1);
                    time[count].xtype = byteToint(data);
                    data = new byte[len - 5];
                    Array.Copy(rdata, cnoindex + 7 + count * len, data, 0, len - 5);
                    time[count].workparm = byteTostring(data);
                }
                return time;

            }
            return null;
        }
        public string GetReaderFlag(string ip)//读读头进出标识0106
        {
            string slen = "0002";
            string command = "0106";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x06, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                return Convert.ToString(data[0], 2).PadLeft(8, '0') + Convert.ToString(data[1], 2).PadLeft(8, '0');
            }
            return null;
        }
        public string SetReaderFlag(string ip, string readerflag)//读读头进出标识0186
        {
            string slen = "0004";
            string command = "0186";
            string appdata = Convert.ToInt32(readerflag, 2).ToString("X2");
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x86, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                return Convert.ToString(data[0], 2).PadLeft(8, '0') + Convert.ToString(data[1], 2).PadLeft(8, '0');
            }
            return null;
        }
        public int GetAutoSwitchDate(string ip, int num, ref string[] parms)//读设备工作模式自动切换日期段属性0107
        {
            string slen = "0003";
            string command = "0107";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x07, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                parms = new string[10];
                parms[0] = rdata[cnoindex + 2].ToString();//有效读头
                Array.Copy(rdata, cnoindex + 3, data, 0, 4);
                parms[1] = byteTostring(data); //起始日期
                Array.Copy(rdata, cnoindex + 7, data, 0, 4);
                parms[2] = byteTostring(data);//结束日期
                parms[3] = rdata[cnoindex + 11].ToString();//有效星期
                data = new byte[5];
                for (int j = 0; j < 6; j++)
                {
                    Array.Copy(rdata, cnoindex + 12 + j * 5, data, 0, 5);
                    parms[4 + j] = byteTostring(data);//工作时区
                }
                return 0;
            }
            return -1;
        }
        public int SetAutoSwitchDate(string ip, int num, ref string[] parms)//读设备工作模式自动切换日期段属性0187
        {
            string command = "0187";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            appdata += int.Parse(parms[0]).ToString("X2").PadLeft(2, '0');
            appdata += parms[1] + parms[2];
            appdata += int.Parse(parms[3]).ToString("X2").PadLeft(2, '0');
            appdata += parms[4] + parms[5] + parms[6] + parms[7] + parms[8] + parms[9];

            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x87, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                parms = new string[10];
                parms[0] = rdata[cnoindex + 2].ToString();//有效读头
                Array.Copy(rdata, cnoindex + 3, data, 0, 4);
                parms[1] = byteTostring(data); //起始日期
                Array.Copy(rdata, cnoindex + 7, data, 0, 4);
                parms[2] = byteTostring(data);//结束日期
                parms[3] = rdata[cnoindex + 11].ToString();//有效星期
                data = new byte[5];
                for (int j = 0; j < 6; j++)
                {
                    Array.Copy(rdata, cnoindex + 12 + j * 5, data, 0, 5);
                    parms[4 + j] = byteTostring(data);//工作时区
                }
                return 0;
            }
            return -1;
        }
        public int GetAutoSwitchWeek(string ip, int num, ref int reader, ref string[] parms)//读设备工作模式自动切换周属性0108
        {
            string slen = "0003";
            string command = "0108";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x08, ref rdata);
            if (rdata.Length > 0)
            {
                parms = new string[42];
                reader = rdata[cnoindex + 2];//有效读头
                byte[] data = new byte[5];
                for (int j = 0; j < 42; j++)
                {
                    Array.Copy(rdata, cnoindex + 3 + j * 5, data, 0, 5);
                    parms[j] = byteTostring(data);//工作时区
                }
                return 0;
            }
            return -1;
        }
        public int SetAutoSwitchWeek(string ip, int num, ref int reader, ref string[] parms)//读设备工作模式自动切换周属性0188
        {
            string command = "0188";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            appdata += reader.ToString("X2").PadLeft(2, '0');
            for (int i = 0; i < 42; i++)
            {
                appdata += parms[i];
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x88, ref rdata);
            if (rdata.Length > 0)
            {
                parms = new string[42];
                reader = rdata[cnoindex + 2];//有效读头
                byte[] data = new byte[5];
                for (int j = 0; j < 42; j++)
                {
                    Array.Copy(rdata, cnoindex + 3 + j * 5, data, 0, 5);
                    parms[j] = byteTostring(data);//工作时区
                }
                return 0;
            }
            return -1;
        }
        public int GetListDate(string ip, int num, ref string[] parms)//读名单日期段0109
        {
            string slen = "0003";
            string command = "0109";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x09, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                parms = new string[9];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                parms[0] = byteTostring(data); //起始日期
                Array.Copy(rdata, cnoindex + 6, data, 0, 4);
                parms[1] = byteTostring(data);//结束日期
                parms[2] = rdata[cnoindex + 10].ToString();//有效星期
                for (int j = 0; j < 6; j++)
                {
                    Array.Copy(rdata, cnoindex + 11 + j * 4, data, 0, 4);
                    parms[3 + j] = byteTostring(data);//工作时区
                }
                return 0;
            }
            return -1;
        }
        public int SetListDate(string ip, int num, ref string[] parms)//读名单日期段0189
        {
            string command = "0189";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            appdata += parms[0] + parms[1];
            appdata += int.Parse(parms[2]).ToString("X2").PadLeft(2, '0');
            appdata += parms[3] + parms[4] + parms[5] + parms[6] + parms[7] + parms[8];

            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x89, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                parms = new string[9];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                parms[0] = byteTostring(data); //起始日期
                Array.Copy(rdata, cnoindex + 6, data, 0, 4);
                parms[1] = byteTostring(data);//结束日期
                parms[2] = rdata[cnoindex + 10].ToString();//有效星期
                for (int j = 0; j < 6; j++)
                {
                    Array.Copy(rdata, cnoindex + 11 + j * 4, data, 0, 4);
                    parms[3 + j] = byteTostring(data);//工作时区
                }
                return 0;
            }
            return -1;
        }
        public int GetHolidayDate(string ip, int num, ref string[] parms)//读节假日010A
        {
            string slen = "0003";
            string command = "010A";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x0A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                parms = new string[10];
                parms[0] = rdata[cnoindex + 2].ToString();//有效读头
                Array.Copy(rdata, cnoindex + 3, data, 0, 4);
                parms[1] = byteTostring(data); //起始日期
                Array.Copy(rdata, cnoindex + 7, data, 0, 4);
                parms[2] = byteTostring(data);//结束日期
                parms[3] = rdata[cnoindex + 11].ToString();//有效星期
                for (int j = 0; j < 6; j++)
                {
                    Array.Copy(rdata, cnoindex + 12 + j * 4, data, 0, 4);
                    parms[4 + j] = byteTostring(data);//工作时区
                }
                return 0;
            }
            return -1;
        }
        public int SetHolidayDate(string ip, int num, ref string[] parms)//读节假日018A
        {
            string command = "018A";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            appdata += int.Parse(parms[0]).ToString("X2").PadLeft(2, '0');
            appdata += parms[1] + parms[2];
            appdata += int.Parse(parms[3]).ToString("X2").PadLeft(2, '0');
            appdata += parms[4] + parms[5] + parms[6] + parms[7] + parms[8] + parms[9];

            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x8A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                parms = new string[10];
                parms[0] = rdata[cnoindex + 2].ToString();//有效读头
                Array.Copy(rdata, cnoindex + 3, data, 0, 4);
                parms[1] = byteTostring(data); //起始日期
                Array.Copy(rdata, cnoindex + 7, data, 0, 4);
                parms[2] = byteTostring(data);//结束日期
                parms[3] = rdata[cnoindex + 11].ToString();//有效星期
                for (int j = 0; j < 6; j++)
                {
                    Array.Copy(rdata, cnoindex + 12 + j * 4, data, 0, 4);
                    parms[4 + j] = byteTostring(data);//工作时区
                }
                return 0;
            }
            return -1;
        }
        public int GetProtectDate(string ip, int num, ref string[] parms)//读自动布防时间段010B
        {
            string slen = "0003";
            string command = "010B";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x0B, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                parms = new string[11];
                parms[0] = rdata[cnoindex + 2].ToString();//有效读头
                Array.Copy(rdata, cnoindex + 4, data, 0, 4);
                parms[1] = byteTostring(data); //起始日期
                Array.Copy(rdata, cnoindex + 8, data, 0, 4);
                parms[2] = byteTostring(data);//结束日期
                parms[3] = rdata[cnoindex + 12].ToString();//有效星期
                for (int j = 0; j < 6; j++)
                {
                    Array.Copy(rdata, cnoindex + 13 + j * 4, data, 0, 4);
                    parms[4 + j] = byteTostring(data);//工作时区
                }
                parms[10] = rdata[14].ToString();//有效输入端口
                return 0;
            }
            return -1;
        }
        public int SetProtectDate(string ip, int num, ref string[] parms)//读自动布防时间段018B
        {
            string command = "018B";
            string appdata = num.ToString("X2").PadLeft(2, '0');
            appdata += int.Parse(parms[0]).ToString("X2").PadLeft(2, '0');
            appdata += int.Parse(parms[10]).ToString("X2").PadLeft(2, '0');
            appdata += parms[1] + parms[2];
            appdata += int.Parse(parms[3]).ToString("X2").PadLeft(2, '0');
            appdata += parms[4] + parms[5] + parms[6] + parms[7] + parms[8] + parms[9];

            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0x8B, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                parms = new string[11];
                parms[0] = rdata[cnoindex + 2].ToString();//有效读头
                Array.Copy(rdata, cnoindex + 4, data, 0, 4);
                parms[1] = byteTostring(data); //起始日期
                Array.Copy(rdata, cnoindex + 8, data, 0, 4);
                parms[2] = byteTostring(data);//结束日期
                parms[3] = rdata[cnoindex + 12].ToString();//有效星期
                data = new byte[4];
                for (int j = 0; j < 6; j++)
                {
                    Array.Copy(rdata, cnoindex + 13 + j * 4, data, 0, 4);
                    parms[4 + j] = byteTostring(data);//工作时区
                }
                parms[10] = rdata[14].ToString();//有效输入端口
                return 0;
            }
            return -1;
        }

        public bool Initial(string ip, int type)//初始化机器01C1
        {
            string slen = "0003";
            string command = "01C1";
            string appdata = type.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == 0x00)
                    return true;
            }
            return false;
        }
        public bool Reset(string ip)//重启机器01C2
        {
            string slen = "0006";
            string command = "01C2";
            string appdata = "8A96B9F5";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0xC2, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == 0x00)
                    return true;
            }
            return false;
        }
        public int SwitchWifi(string ip, int wireless, int wifi)
        {
            string command = "01C3";
            string appdata = wireless.ToString("X2").PadLeft(2, '0') + wifi.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x01, 0xC3, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == wireless && rdata[cnoindex + 2] == wifi)
                    return 0;
                else
                    return 1;
            }
            return -1;
        }
        #endregion

        #region 门禁消费部分(02)
        public string GetFactoryNo(string ip)//读企业代码0201
        {
            string no = null;
            string slen = "0002";
            string command = "0201";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                no = byteTostring(data);
                return no;
            }
            return no;
        }
        public string SetFactoryNo(string ip, string fno)//设置企业代码0281
        {
            string no = null;
            string slen = "0004";
            string command = "0281";
            string appdata = fno.PadLeft(4, '0');// byteTostring(Encoding.Default.GetBytes(fno)).PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x81, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                no = byteTostring(data);
            }
            return no;
        }
        public string GetWorkPeriod(string ip)//读设备工作时间段0202
        {
            string slen = "0002";
            string command = "0202";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 1, data, 0, 6);
                string wp = "";
                for (int i = 0; i < 6; i++)
                {
                    char[] arr = Convert.ToString(data[i], 2).PadLeft(8, '0').ToCharArray();
                    Array.Reverse(arr);
                    wp += new string(arr);
                }
                return wp;
            }
            return null;
        }
        public string SetWorkPeriod(string ip, string workperiod)//设置设备工作时间段0282
        {
            string wp1 = "";
            string slen = "0008";
            string command = "0282";
            for (int i = 0; i < 6; i++)
            {
                char[] arr = workperiod.Substring(i * 8, 8).ToCharArray();
                Array.Reverse(arr);
                wp1 += new string(arr);
            }
            string appdata = Convert.ToInt64(wp1, 2).ToString("X2").PadLeft(12, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x82, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 1, data, 0, 6);
                string wp = "";
                for (int i = 0; i < 6; i++)
                {
                    char[] arr = Convert.ToString(data[i], 2).PadLeft(8, '0').ToCharArray();
                    Array.Reverse(arr);
                    wp += new string(arr);
                }
                return wp;
            }
            return null;
        }
        public string GetMachineGroup(string ip)//读设备分组识别参数0203
        {
            string mg = null;
            string slen = "0002";
            string command = "0203";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[32];
                Array.Copy(rdata, cnoindex + 1, data, 0, 32);
                mg = "";
                for (int i = 0; i < data.Length; i++)
                {
                    mg += Convert.ToString(data[i], 2).PadLeft(8, '0');
                }
                string mg1 = "";
                for (int i = 0; i < 32; i++)
                {
                    char[] arr = mg.Substring(i * 8, 8).ToCharArray();
                    Array.Reverse(arr);
                    mg1 += new string(arr);
                }
                return mg1;
            }
            return mg;
        }
        public string SetMachineGroup(string ip, string machinegroup)//设置设备分组识别参数0283
        {
            string mg1 = "";
            for (int i = 0; i < 32; i++)
            {
                char[] arr = machinegroup.Substring(i * 8, 8).ToCharArray();
                Array.Reverse(arr);
                mg1 += new string(arr);
            }
            string mg = null;
            string slen = "0022";
            string command = "0283";
            string appdata = Convert.ToInt64(mg1.Substring(0, 64), 2).ToString("X2").PadLeft(16, '0');
            appdata += Convert.ToInt64(mg1.Substring(64, 64), 2).ToString("X2").PadLeft(16, '0');
            appdata += Convert.ToInt64(mg1.Substring(128, 64), 2).ToString("X2").PadLeft(16, '0');
            appdata += Convert.ToInt64(mg1.Substring(172, 64), 2).ToString("X2").PadLeft(16, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x83, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[32];
                Array.Copy(rdata, cnoindex + 1, data, 0, 32);
                mg = "";
                for (int i = 0; i < data.Length; i++)
                {
                    mg += Convert.ToString(data[i], 2).PadLeft(8, '0');
                }
                mg1 = "";
                for (int i = 0; i < 32; i++)
                {
                    char[] arr = mg.Substring(i * 8, 8).ToCharArray();
                    Array.Reverse(arr);
                    mg1 += new string(arr);
                }
                return mg1;
            }
            return mg;
        }
        public int GetFixedMoney(string ip)//读消费机定值消费金额0204
        {
            int fm = 0;
            string slen = "0002";
            string command = "0204";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x04, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int SetFixedMoney(string ip, int fixedmoney)//设置消费机定值消费金额0284
        {
            int fm = 0;
            string slen = "0006";
            string command = "0284";
            string appdata = fixedmoney.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x84, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int[] GetNumberAmount(string ip)//读消费机菜号消费0205
        {
            int[] fm = new int[10];
            string slen = "0002";
            string command = "0205";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x05, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                fm[1] = byteToint(data);
                Array.Copy(rdata, cnoindex + 9, data, 0, 4);
                fm[2] = byteToint(data);
                Array.Copy(rdata, cnoindex + 13, data, 0, 4);
                fm[3] = byteToint(data);
                Array.Copy(rdata, cnoindex + 17, data, 0, 4);
                fm[4] = byteToint(data);
                Array.Copy(rdata, cnoindex + 21, data, 0, 4);
                fm[5] = byteToint(data);
                Array.Copy(rdata, cnoindex + 25, data, 0, 4);
                fm[6] = byteToint(data);
                Array.Copy(rdata, cnoindex + 29, data, 0, 4);
                fm[7] = byteToint(data);
                Array.Copy(rdata, cnoindex + 33, data, 0, 4);
                fm[8] = byteToint(data);
                Array.Copy(rdata, cnoindex + 37, data, 0, 4);
                fm[9] = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int[] SetNumberAmount(string ip, int[] numberamount)//设置消费机菜号消费0285
        {
            int[] fm = new int[10];
            string slen = "002A";
            string command = "0285";
            string appdata = numberamount[0].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[1].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[2].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[3].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[4].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[5].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[6].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[7].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[8].ToString("X2").PadLeft(8, '0');
            appdata += numberamount[9].ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x85, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                fm[1] = byteToint(data);
                Array.Copy(rdata, cnoindex + 9, data, 0, 4);
                fm[2] = byteToint(data);
                Array.Copy(rdata, cnoindex + 13, data, 0, 4);
                fm[3] = byteToint(data);
                Array.Copy(rdata, cnoindex + 17, data, 0, 4);
                fm[4] = byteToint(data);
                Array.Copy(rdata, cnoindex + 21, data, 0, 4);
                fm[5] = byteToint(data);
                Array.Copy(rdata, cnoindex + 25, data, 0, 4);
                fm[6] = byteToint(data);
                Array.Copy(rdata, cnoindex + 29, data, 0, 4);
                fm[7] = byteToint(data);
                Array.Copy(rdata, cnoindex + 33, data, 0, 4);
                fm[8] = byteToint(data);
                Array.Copy(rdata, cnoindex + 37, data, 0, 4);
                fm[9] = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int GetDayLimitedTimes(string ip)//读消费机日消费限制次数0206
        {
            int fm = 0;
            string slen = "0002";
            string command = "0206";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x06, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int SetDayLimitedTimes(string ip, int limitedtimes)//设置消费机日消费限制次数0286
        {
            int fm = 0;
            string slen = "0003";
            string command = "0286";
            string appdata = limitedtimes.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x86, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int GetMonthLimitedTimes(string ip)//读消费机月消费限制次数0207
        {
            int fm = 0;
            string slen = "0002";
            string command = "0207";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x07, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];

                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int SetMonthLimitedTimes(string ip, int limitedtimes)//设置消费机日消费限制次数0287
        {
            int fm = 0;
            string slen = "0004";
            string command = "0287";
            string appdata = limitedtimes.ToString("X2").PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x87, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int GetLimitedAmount(string ip)//读单笔消费限制金额0208
        {
            int fm = 0;
            string slen = "0002";
            string command = "0208";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x08, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int SetLimitedAmount(string ip, int limitedamount)//设置单笔消费限制金额0288
        {
            int fm = 0;
            string slen = "0006";
            string command = "0288";
            string appdata = limitedamount.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x88, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int GetDayLimitedAmount(string ip)//读日消费限制金额0209
        {
            int fm = 0;
            string slen = "0002";
            string command = "0209";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x09, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int SetDayLimitedAmount(string ip, int limitedamount)//设置日消费限制金额0289
        {
            int fm = 0;
            string slen = "0006";
            string command = "0289";
            string appdata = limitedamount.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x89, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int GetMonthLimitedAmount(string ip)//读月消费限制金额020A
        {
            int fm = 0;
            string slen = "0002";
            string command = "020A";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x0A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int SetMonthLimitedAmount(string ip, int limitedamount)//设置月消费限制金额028A
        {
            int fm = 0;
            string slen = "0006";
            string command = "028A";
            string appdata = limitedamount.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x8A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int GetMaxBalance(string ip)//读系统最大限制余额020B
        {
            int fm = 0;
            string slen = "0002";
            string command = "020B";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x0B, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int SetMaxBalance(string ip, int maxbalance)//设置系统最大限制余额028B
        {
            int fm = 0;
            string slen = "0006";
            string command = "028B";
            string appdata = maxbalance.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x8B, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int[] GetDealType(string ip)//读消费机交易模式020C
        {
            int[] fm = new int[2];
            string slen = "0002";
            string command = "020C";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x0C, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                fm[1] = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int[] SetDealType(string ip, int[] dealtype)//设置消费机交易模式028C
        {
            int[] fm = new int[2];
            string slen = "0004";
            string command = "028C";
            string appdata = dealtype[0].ToString("X2").PadLeft(2, '0') + dealtype[1].ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x8C, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                fm[1] = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int[] GetTimeControl(string ip)//读消费机时间段控制参数020D
        {
            int[] fm = new int[2];
            string slen = "0002";
            string command = "020D";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x0D, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                fm[1] = byteToint(data);
                return fm;
            }
            return null;
        }
        public int[] SetTimeControl(string ip, int[] timecontrol)//设置消费机时间段控制参数028D
        {
            int[] fm = new int[2];
            string slen = "0004";
            string command = "028D";
            string appdata = timecontrol[0].ToString("X2").PadLeft(2, '0') + timecontrol[1].ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x8D, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                fm[1] = byteToint(data);
                return fm;
            }
            return null;
        }
        public Protocol GetProtocol(string ip, int pindex)//读消费策略020E
        {
            string slen = "0003";
            string command = "020E";
            string appdata = pindex.ToString("X2");
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x0E, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 14];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                return ProtocolGet(data);
            }
            return new Protocol();
        }
        public int GetProtocol(string ip, ref int pindex, ref int group, ref int len, ref string region, ref int[] type, ref string[] value)//读消费策略020E
        {
            string slen = "0003";
            string command = "020E";
            string appdata = pindex.ToString("X2");
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x0E, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 14];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                Protocol p = ProtocolGet(data);
                group = p.group;
                pindex = p.index;
                len = p.len;
                region = p.region;
                for (int i = 0; i < p.protocol.Length; i++)
                {
                    type[i] = p.protocol[i].type;
                    value[i] = p.protocol[i].value;
                }
                return 0;
            }
            return 1;
        }
        public Protocol SetProtocol(string ip, Protocol protocol)//设置消费策略028E
        {
            string command = "028E";
            string appdata = ProtocolSet(protocol);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x8E, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 14];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                return ProtocolGet(data);
            }
            return new Protocol();
        }
        public int SetProtocol(string ip, ref int pindex, ref int group, ref int len, ref string region, ref int[] type, ref string[] value)//设置消费策略028E
        {
            Protocol protocol = new Protocol();
            protocol.group = group;
            protocol.index = pindex;
            protocol.len = len;
            protocol.region = region;
            for (int i = 0; i < type.Length; i++)
            {
                protocol.protocol[i].type = type[i];
                protocol.protocol[i].value = value[i];
            }
            string command = "028E";
            string appdata = ProtocolSet(protocol);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x8E, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 14];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                Protocol p = ProtocolGet(data);
                group = p.group;
                pindex = p.index;
                len = p.len;
                region = p.region;
                for (int i = 0; i < p.protocol.Length; i++)
                {
                    type[i] = p.protocol[i].type;
                    value[i] = p.protocol[i].value;
                }
                return 0;
            }
            return 1;
        }
        private Protocol ProtocolGet(byte[] protocol)
        {
            Protocol pf = new Protocol();
            pf.index = protocol[0];
            if (protocol.Length > 1) pf.len = protocol[1];
            if (protocol.Length > 2)
            {
                pf.group = protocol[2];
                pf.region = Convert.ToString(protocol[3], 2).PadLeft(8, '0');
                int startindex = 4, protocollen = 0;
                ProtocolDetail[] pdall = new ProtocolDetail[24];
                while (startindex < protocol.Length - 1)
                {
                    int type = protocol[startindex];
                    int len = 0;
                    if (type == 0) break;
                    pdall[protocollen].type = type;
                    if (type <= 2)
                    {
                        pdall[protocollen].value = protocol[startindex + 1].ToString().PadLeft(2, '0') + ":" + protocol[startindex + 2].ToString().PadLeft(2, '0');
                        len = 2;
                    }
                    else
                    {
                        for (int i = 0; i < protocolfield.Length; i++)
                        {
                            if (protocolfield[i].typeno == type)
                            {
                                len = protocolfield[i].len;
                                byte[] data = new byte[len];
                                Array.Copy(protocol, startindex + 1, data, 0, len);
                                pdall[protocollen].value = byteToint(data).ToString();
                                break;
                            }
                        }
                    }
                    startindex += len + 1;
                    protocollen += 1;
                }
                pf.protocol = new ProtocolDetail[protocollen];
                Array.Copy(pdall, 0, pf.protocol, 0, protocollen);
            }
            return pf;
        }
        private string ProtocolSet(Protocol protocol)
        {
            string p = "";
            if (protocol.protocol.Length > 0)
            {
                p = protocol.group.ToString("X2").PadLeft(2, '0');
                p += Convert.ToInt16(protocol.region, 2).ToString("X2").PadLeft(2, '0');
                for (int i = 0; i < protocol.protocol.Length; i++)
                {
                    p += protocol.protocol[i].type.ToString("X2").PadLeft(2, '0');
                    if (protocol.protocol[i].type <= 2)
                    {
                        string[] ptime = protocol.protocol[i].value.Split(':');
                        p += int.Parse(ptime[0]).ToString("X2").PadLeft(2, '0') + int.Parse(ptime[1]).ToString("X2").PadLeft(2, '0');
                    }
                    else
                    {
                        for (int j = 0; j < protocolfield.Length; j++)
                        {
                            if (protocolfield[j].typeno == protocol.protocol[i].type)
                            {
                                p += int.Parse(protocol.protocol[i].value).ToString("X2").PadLeft(protocolfield[j].len * 2, '0');
                            }
                        }
                    }
                }
            }

            return protocol.index.ToString("X2").PadLeft(2, '0') + (p.Length / 2 + 1).ToString("X2").PadLeft(2, '0') + p + "00";
        }
        public bool DeleteProtocol(string ip)//删除所有消费策略02C0
        {
            string slen = "0002";
            string command = "02C0";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0xC0, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                if (data[0] == 0)
                    return true;
                else
                    return false;
            }
            return false;
        }
        public int GetDeductType(string ip)//读取消费扣款模式020F
        {
            int fm = 0;
            string slen = "0002";
            string command = "020F";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x0F, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return -1;
        }
        public int SetDeductType(string ip, int deducttype)//设置消费扣款模式028F
        {
            int fm = 0;
            string slen = "0003";
            string command = "028F";
            string appdata = deducttype.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x8F, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return -1;
        }
        public int GetReaderProperty(string ip, int readerno, ref int[] parms, ref string passnos)//读取门禁读头工作属性0210
        {
            int fm = 0;
            string slen = "0003";
            string command = "0210";
            string appdata = readerno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x10, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[16];
                Array.Copy(rdata, cnoindex + 2, data, 0, 16);
                parms = new int[6];
                parms[0] = Convert.ToInt16(Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(0, 4), 2);
                parms[1] = Convert.ToInt16(Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(4, 4), 2);
                parms[2] = data[1];
                parms[3] = data[2] * 256 + data[3];
                parms[4] = data[8];
                parms[5] = data[12];
                byte[] pass = new byte[4];
                Array.Copy(data, 4, pass, 0, 4);
                passnos = Convert.ToString(byteTolong(pass), 2);
                return fm;
            }
            return -1;
        }
        public int SetReaderProperty(string ip, int readerno, ref int[] parms, ref string passnos)//设置门禁读头工作属性0290
        {
            int fm = 0;
            string command = "0290";
            string appdata = readerno.ToString("X2").PadLeft(2, '0');
            appdata += (parms[0] * 16 + parms[1]).ToString("X2").PadLeft(2, '0');
            appdata += parms[2].ToString("X2").PadLeft(2, '0');
            appdata += parms[3].ToString("X2").PadLeft(4, '0');
            appdata += passnos.PadLeft(8, '0');
            appdata += parms[4].ToString("X2").PadLeft(2, '0') + "000000";
            appdata += parms[5].ToString("X2").PadLeft(2, '0') + "000000";
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[16];
                Array.Copy(rdata, cnoindex + 2, data, 0, 16);
                parms = new int[6];
                parms[0] = Convert.ToInt16(Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(0, 4), 2);
                parms[1] = Convert.ToInt16(Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(4, 4), 2);
                parms[2] = data[1];
                parms[3] = data[2] * 256 + data[3];
                parms[4] = data[8];
                parms[5] = data[12];
                byte[] pass = new byte[4];
                Array.Copy(data, 4, pass, 0, 4);
                passnos = Convert.ToString(byteTolong(pass), 2);
                return fm;
            }
            return -1;
        }
        public int GetDoorControlParm(string ip, int doorno, ref int[] parms)//读取门禁机门控参数0211
        {
            int fm = 0;
            string slen = "0003";
            string command = "0211";
            string appdata = doorno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x11, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[26];
                parms = new int[15];
                Array.Copy(rdata, cnoindex + 2, data, 0, 26);
                parms[0] = data[0];
                parms[1] = data[1];
                parms[2] = data[2];
                parms[3] = data[3];
                parms[4] = data[6];
                parms[5] = data[7];
                parms[6] = data[8];
                parms[7] = data[9];
                parms[8] = data[10];
                parms[9] = data[11];
                parms[10] = data[14] * 256 + data[15];
                parms[11] = data[16] * 256 + data[17];
                parms[12] = data[18] * 256 + data[19];
                parms[13] = data[20] * 256 + data[21];
                parms[14] = data[24] * 256 + data[25];
                return fm;
            }
            return -1;
        }
        public int SetDoorControlParm(string ip, int doorno, ref int[] parms)//设置门禁机门控参数0291
        {
            int fm = 0;
            string command = "0291";
            string appdata = doorno.ToString("X2").PadLeft(2, '0');
            appdata += parms[0].ToString("X2").PadLeft(2, '0');
            appdata += parms[1].ToString("X2").PadLeft(2, '0');
            appdata += parms[2].ToString("X2").PadLeft(2, '0');
            appdata += parms[3].ToString("X2").PadLeft(2, '0') + "0000";
            appdata += parms[4].ToString("X2").PadLeft(2, '0');
            appdata += parms[5].ToString("X2").PadLeft(2, '0');
            appdata += parms[6].ToString("X2").PadLeft(2, '0');
            appdata += parms[7].ToString("X2").PadLeft(2, '0');
            appdata += parms[8].ToString("X2").PadLeft(2, '0');
            appdata += parms[9].ToString("X2").PadLeft(2, '0') + "0000";
            appdata += parms[10].ToString("X2").PadLeft(4, '0');
            appdata += parms[11].ToString("X2").PadLeft(4, '0');
            appdata += parms[12].ToString("X2").PadLeft(4, '0');
            appdata += parms[13].ToString("X2").PadLeft(4, '0') + "0000";
            appdata += parms[14].ToString("X2").PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x91, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[26];
                parms = new int[15];
                Array.Copy(rdata, cnoindex + 2, data, 0, 26);
                parms[0] = data[0];
                parms[1] = data[1];
                parms[2] = data[2];
                parms[3] = data[3];
                parms[4] = data[6];
                parms[5] = data[7];
                parms[6] = data[8];
                parms[7] = data[9];
                parms[8] = data[10];
                parms[9] = data[11];
                parms[10] = data[14] * 256 + data[15];
                parms[11] = data[16] * 256 + data[17];
                parms[12] = data[18] * 256 + data[19];
                parms[13] = data[20] * 256 + data[21];
                parms[14] = data[24] * 256 + data[25];
                return fm;
            }
            return -1;
        }
        public int GetDoorPass(string ip, int passno, ref int passtype)//读取门禁密码0212
        {
            string slen = "0003";
            string command = "0212";
            string appdata = passno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x12, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                passtype = byteToint(data);
                return passtype;
            }
            return -1;
        }
        public int SetDoorPass(string ip, int passno, ref int passtype, ref string password)//设置门禁密码0292
        {
            int fm = 0;
            string command = "0292";
            string appdata = passno.ToString("X2").PadLeft(2, '0') + passtype.ToString("X2").PadLeft(2, '0') + password.PadRight(6, 'F');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x92, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                passtype = byteToint(data);
                data = new byte[3];
                Array.Copy(rdata, cnoindex + 3, data, 0, 3);
                password = byteTostring(data);
                return fm;
            }
            return -1;
        }
        public int GetMultiCard(string ip, int cardgroupno, ref int flag, ref int count, ref Int64[] cards)//读取指定多卡开门信息0213
        {
            string slen = "0003";
            string command = "0213";
            string appdata = cardgroupno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x13, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                flag = byteToint(data);
                count = Convert.ToInt16(Convert.ToString(flag, 2).PadLeft(8, '0').Substring(4, 4), 2);
                flag = Convert.ToInt16(Convert.ToString(flag, 2).PadLeft(8, '0').Substring(0, 1), 2);
                cards = new long[count];
                data = new byte[4];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(rdata, cnoindex + 3 + i * 4, data, 0, 4);
                    cards[i] = (long)data[0] * 16777216 + (long)data[1] * 65536 + (long)data[2] * 256 + (long)data[3];// byteTolong(data);
                }
                return 0;
            }
            return -1;
        }
        public int SetMultiCard(string ip, int cardgroupno, ref int flag, ref int count, ref Int64[] cards)//设置指定多卡开门信息0293
        {
            string command = "0293";
            string appdata = cardgroupno.ToString("X2").PadLeft(2, '0') + Convert.ToInt16(flag.ToString() + "000" + Convert.ToString(count, 2).PadLeft(4, '0'), 2).ToString("X2").PadLeft(2, '0');
            for (int i = 0; i < cards.Length; i++)
            {
                appdata += cards[i].ToString("X2").PadLeft(8, '0');
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x93, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                flag = byteToint(data);
                count = Convert.ToInt16(Convert.ToString(flag, 2).PadLeft(8, '0').Substring(4, 4), 2);
                flag = Convert.ToInt16(Convert.ToString(flag, 2).PadLeft(8, '0').Substring(0, 1), 2);
                cards = new long[count];
                data = new byte[4];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(rdata, cnoindex + 3 + i * 4, data, 0, 4);
                    cards[i] = (long)data[0] * 16777216 + (long)data[1] * 65536 + (long)data[2] * 256 + (long)data[3];// byteTolong(data);
                }
                return 0;
            }
            return -1;
        }
        public int GetLinkParm(string ip, int linkno, ref int event1, ref int reader, ref int[] signal)//读取联动参数0214
        {
            string slen = "0003";
            string command = "0214";
            string appdata = linkno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x14, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                event1 = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 1);
                reader = byteToint(data);
                signal = new int[4];
                data = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    Array.Copy(rdata, cnoindex + 4 + i * 2, data, 0, 2);
                    signal[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int SetLinkParm(string ip, int linkno, ref int event1, ref int reader, ref int[] signal)//设置联动参数0294
        {
            string command = "0294";
            string appdata = linkno.ToString("X2").PadLeft(2, '0') + event1.ToString("X2").PadLeft(2, '0') + reader.ToString("X2").PadLeft(2, '0');
            for (int i = 0; i < signal.Length; i++)
            {
                appdata += signal[i].ToString("X2").PadLeft(4, '0');
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x94, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                event1 = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 1);
                reader = byteToint(data);
                signal = new int[4];
                data = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    Array.Copy(rdata, cnoindex + 4 + i * 2, data, 0, 2);
                    signal[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int GetLockGroup(string ip, int groupno, ref int count, ref int[] doors)//读取门点互锁门组0215
        {
            string slen = "0003";
            string command = "0215";
            string appdata = groupno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x15, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                count = byteToint(data);
                doors = new int[count];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(rdata, cnoindex + 3 + i, data, 0, 1);
                    doors[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int SetLockGroup(string ip, int groupno, ref int count, ref int[] doors)//设置门点互锁门组0295
        {
            string command = "0295";
            string appdata = groupno.ToString("X2").PadLeft(2, '0') + count.ToString("X2").PadLeft(2, '0');
            for (int i = 0; i < doors.Length; i++)
            {
                appdata += doors[i].ToString("X2").PadLeft(2, '0');
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                count = byteToint(data);
                doors = new int[count];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(rdata, cnoindex + 3 + i, data, 0, 1);
                    doors[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int GetTailGroup(string ip, int groupno, ref int count, ref int[] doors)//读取防尾随门组（双向APB和区域APB）0216
        {
            string slen = "0003";
            string command = "0216";
            string appdata = groupno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x16, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                count = byteToint(data);
                doors = new int[count];
                data = new byte[1];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(rdata, cnoindex + 3 + i, data, 0, 1);
                    doors[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int SetTailGroup(string ip, int groupno, ref int count, ref int[] doors)//设置防尾随门组（双向APB和区域APB）0296
        {
            string command = "0296";
            string appdata = groupno.ToString("X2").PadLeft(2, '0') + count.ToString("X2").PadLeft(2, '0');
            for (int i = 0; i < doors.Length; i++)
            {
                appdata += doors[i].ToString("X2").PadLeft(2, '0');
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x96, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                count = byteToint(data);
                doors = new int[count];
                data = new byte[1];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(rdata, cnoindex + 3 + i, data, 0, 1);
                    doors[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int GetProtectPort(string ip, int readerno, ref int port)//读取密码布撤防有效的输入端口0217
        {
            string slen = "0003";
            string command = "0217";
            string appdata = readerno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x17, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                port = byteToint(data);
                return 0;
            }
            return -1;
        }
        public int SetProtectPort(string ip, int readerno, ref int port)//设置密码布撤防有效的输入端口0297
        {
            string command = "0297";
            string appdata = readerno.ToString("X2").PadLeft(2, '0') + port.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x97, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                port = byteToint(data);
                return 0;
            }
            return -1;
        }
        public int GetRightGroup(string ip, ref int[] port)//读取指定权限组别开门的组别信息0218
        {
            string slen = "0002";
            string command = "0218";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x18, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                for (int i = 0; i < port.Length; i++)
                {
                    Array.Copy(rdata, cnoindex + 2 + i, data, 0, 1);
                    port[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int SetRightGroup(string ip, ref int[] port)//设置指定权限组别开门的组别信息0298
        {
            string command = "0298";
            string appdata = "";
            for (int i = 0; i < port.Length; i++)
            {
                appdata += port[i].ToString("X2").PadLeft(2, '0');
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x98, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                for (int i = 0; i < port.Length; i++)
                {
                    Array.Copy(rdata, cnoindex + 2 + i, data, 0, 1);
                    port[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int GetDisplaySecond(string ip)//读打卡界面显示时间参数0219
        {
            int fm = 0;
            string command = "0219";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x19, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                if (fm > 240) fm = 0;
                return fm;
            }
            return -1;
        }
        public int SetDisplaySecond(string ip, int seconds)//设置打卡界面显示时间参数0299
        {
            int fm = 0;
            string command = "0299";
            string appdata = seconds.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x99, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return -1;
        }
        public int GetSubsidyAmount(string ip, ref int[] amount)//读图飞旭补贴金额参数021A
        {
            string command = "021A";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x1A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                amount = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    Array.Copy(rdata, cnoindex + 1 + i * 4, data, 0, 4);
                    amount[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int SetSubsidyAmount(string ip, ref int[] amount)//设置图飞旭补贴金额参数029A
        {
            string command = "029A";
            string appdata = amount[0].ToString("X2").PadLeft(8, '0');
            appdata += amount[1].ToString("X2").PadLeft(8, '0');
            appdata += amount[2].ToString("X2").PadLeft(8, '0');
            appdata += amount[3].ToString("X2").PadLeft(8, '0');
            appdata += amount[4].ToString("X2").PadLeft(8, '0');
            appdata += amount[5].ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x9A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                amount = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    Array.Copy(rdata, cnoindex + 1 + i * 4, data, 0, 4);
                    amount[i] = byteToint(data);
                }
                return 0;
            }
            return -1;
        }
        public int GetWeightItem(string ip, ref int[] no, ref string[] dispname)//读称重项目物品名称参数0220
        {
            string command = "0220";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x20, ref rdata);
            if (rdata.Length > 0)
            {
                no = new int[20];
                dispname = new string[20];
                for (int i = 0; i < 20; i++)
                {
                    byte[] data = new byte[2];
                    Array.Copy(rdata, cnoindex + 1 + i * 18, data, 0, 2);
                    no[i] = byteToint(data);
                    data = new byte[16];
                    Array.Copy(rdata, cnoindex + 3 + i * 18, data, 0, 16);
                    dispname[i] = Encoding.Default.GetString(data);
                }
                return 0;
            }
            return -1;
        }
        public int SetWeightItem(string ip, ref int[] no, ref string[] dispname)//设置称重项目物品名称参数02A0
        {
            string command = "02A0";
            string appdata = "";
            for (int i = 0; i < 20; i++)
            {
                string disp = byteTostring(Encoding.Default.GetBytes(dispname[i]));
                appdata += no[i].ToString("X2").PadLeft(4, '0');
                appdata += (disp.Length > 30 ? disp.Substring(0, 30) : disp).PadRight(32, '0');
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0xA0, ref rdata);
            if (rdata.Length > 0)
            {
                no = new int[20];
                dispname = new string[20];
                for (int i = 0; i < 20; i++)
                {
                    byte[] data = new byte[2];
                    Array.Copy(rdata, cnoindex + 1 + i * 18, data, 0, 2);
                    no[i] = byteToint(data);
                    data = new byte[16];
                    Array.Copy(rdata, cnoindex + 3 + i * 18, data, 0, 16);
                    dispname[i] = Encoding.Default.GetString(data);
                }
                return 0;
            }
            return -1;
        }
        public int GetTempLimit(string ip, ref string[] templimit)//读温湿度监控项目温湿度限制参数0221
        {
            string command = "0221";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0x21, ref rdata);
            if (rdata.Length > 0)
            {
                templimit = new string[rdata[cnoindex + 1]];
                for (int i = 0; i < templimit.Length; i++)
                {
                    byte[] data = new byte[8];
                    Array.Copy(rdata, cnoindex + 2 + i * 8, data, 0, 8);
                    templimit[i] = byteTostring(data);
                }
                return 0;
            }
            return -1;
        }
        public int SetTempLimit(string ip, ref string[] templimit)//设置温湿度监控项目温湿度限制参数02A1
        {
            string command = "02A1";
            string appdata = templimit.Length.ToString("X2").PadLeft(2, '0');
            for (int i = 0; i < templimit.Length; i++)
            {
                appdata += templimit[i];
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0xA1, ref rdata);
            if (rdata.Length > 0)
            {
                templimit = new string[rdata[cnoindex + 1]];
                for (int i = 0; i < templimit.Length; i++)
                {
                    byte[] data = new byte[8];
                    Array.Copy(rdata, cnoindex + 2 + i * 8, data, 0, 8);
                    templimit[i] = byteTostring(data);
                }
                return 0;
            }
            return -1;
        }

        public int ControlDoorWork(string ip, int type, int work)//控制多门门禁实时动作02C0
        {
            string command = "02C0";
            string appdata = type.ToString("X2").PadLeft(2, '0') + work.ToString("X2").PadLeft(6, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0xC0, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                return byteToint(data);
            }
            return -1;
        }
        public int GetReaderWorkMode(string ip, ref int count, ref int[] workmode)//读所有读头当前的工作模式02C1
        {
            string command = "02C1";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                count = rdata[cnoindex + 1];
                workmode = new int[count];
                for (int x = 0; x < count; x++)
                {
                    workmode[x] = rdata[cnoindex + 2 + x];
                }
                return 0;
            }
            return -1;
        }
        public int GetProtectStatus(string ip)//读辅助输入端口的布撤防状态02C2
        {
            int fm = 0;
            string command = "02C2";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0xC2, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return -1;
        }
        public int GetDoorStatus(string ip, ref int[] doorstatus)//读门当前的开关状态02C3
        {
            string command = "02C3";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x02, 0xC3, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                doorstatus = new int[6];
                Array.Copy(rdata, cnoindex + 1, data, 0, 6);
                doorstatus[0] = data[0];
                doorstatus[1] = data[1];
                doorstatus[2] = data[2];
                doorstatus[3] = data[3];
                doorstatus[4] = data[4];
                doorstatus[5] = data[5];
                return 0;
            }
            return -1;
        }
        #endregion

        #region 显示控制(03)
        private string GetDisplay(byte[] data)
        {
            string rs = "";
            int index = 0;
            byte[] rdata = new byte[2];
            while (index < data.Length)
            {
                if (data[index] >= 0x80)//汉字
                {
                    rdata = new byte[2];
                    Array.Copy(data, index, rdata, 0, 2);
                    index += 2;
                    rs += Encoding.Default.GetString(rdata);
                }
                else if (data[index] >= 0x20)//ASCII
                {
                    rdata = new byte[1];
                    Array.Copy(data, index, rdata, 0, 1);
                    rs += Encoding.Default.GetString(rdata);
                    index += 1;
                }
                else if (data[index] >= 0x15)//ASCII
                {
                    rdata = new byte[1];
                    Array.Copy(data, index, rdata, 0, 1);
                    rs += byteTostring(rdata);
                    index += 1;
                }
                else//特殊内容
                {
                    rdata = new byte[2];
                    Array.Copy(data, index, rdata, 0, 2);
                    index += 2;
                    rs += byteTostring(rdata);
                }
            }
            return rs;
        }
        private string SetDisplay(int[] row, int[] col, string[] ds)
        {
            string rds = "";
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] < 1) break;
                rds += row[i].ToString("X2").PadLeft(2, '0');
                rds += col[i].ToString("X2").PadLeft(2, '0');
                int sindex = 0, eindex = 0;
                while (eindex < ds[i].Length)
                {
                    if (ds[i][eindex] == '0' && i < row.Length - 1 && ds[i + 1][eindex] > '0' && ds[i][eindex] < '9')
                    {
                        if (eindex != sindex)
                        {
                            rds += byteTostring(Encoding.Default.GetBytes(ds[i].Substring(sindex, eindex - sindex)));
                        }
                        rds += ds[i].Substring(eindex, 4);
                        sindex = eindex + 4;
                        eindex += 4;
                    }
                    else
                    {
                        if (ds[i][eindex] == '1' && eindex < ds[i].Length - 1 && ds[i][eindex + 1] == '5')
                        {
                            rds += ds[i][eindex].ToString() + ds[i][eindex + 1].ToString();
                            rds += ds[i].Substring(eindex + 2, 4);
                            sindex = eindex + 6;
                            eindex += 6;
                        }
                        else
                            eindex++;
                    }
                }
                if (sindex < eindex)
                    rds += byteTostring(Encoding.Default.GetBytes(ds[i].Substring(sindex)));
                rds += "00";
            }
            return rds + "00";
        }
        private string SetMenuDisplay(bool[] hadchild, bool[] hadnext, int[] flag, int[] editindex, string[] ds)
        {
            string rds = "", str2;
            for (int i = 0; i < flag.Length; i++)
            {
                if (ds[i] == null) break;
                str2 = hadchild[i] ? "1" : "0";
                str2 += hadnext[i] ? "1" : "0";
                str2 += Convert.ToString(flag[i], 2).PadLeft(6, '0');
                rds += Convert.ToInt16(str2, 2).ToString("X2").PadLeft(2, '0');
                rds += editindex[i].ToString("X2").PadLeft(2, '0');
                rds += byteTostring(Encoding.Default.GetBytes(ds[i]));
                rds += "00";
            }
            return rds + "00";
        }
        public int GetDisplaySet(string ip, int displayno, ref int[] row, ref int[] col, ref string[] ds)//读显示屏显示设置0301
        {
            string command = "0301";
            string appdata = displayno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 16], ndata;
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                int count = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == 0) count++;
                }
                ds = new string[count];
                row = new int[count];
                col = new int[count];
                count = 0;
                int sindex = 2, eindex = 2;
                row[0] = data[0];
                col[0] = data[1];
                while (eindex <= data.Length)
                {
                    if (data[eindex + 1] == 0)
                    {
                        ndata = new byte[eindex - sindex + 1];
                        Array.Copy(data, sindex, ndata, 0, ndata.Length);
                        ds[count] = GetDisplay(ndata);
                        count++;
                        if (eindex + 2 == data.Length)
                            break;
                        else
                        {
                            row[count] = data[eindex + 2];
                            col[count] = data[eindex + 3];
                            sindex = eindex + 4;
                            eindex = eindex + 4;
                        }
                    }
                    else
                    {
                        eindex++;
                    }
                }
                return 0;
            }
            return 1;
        }
        public int SetDisplaySet(string ip, int displayno, ref int[] row, ref int[] col, ref string[] ds)//设置显示屏显示设置0381
        {
            string command = "0381";
            string appdata = displayno.ToString("X2").PadLeft(2, '0') + SetDisplay(row, col, ds);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x81, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 16], ndata;
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                int count = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == 0) count++;
                }
                ds = new string[count];
                row = new int[count];
                col = new int[count];
                count = 0;
                int sindex = 2, eindex = 2;
                row[0] = data[0];
                col[0] = data[1];
                while (eindex <= data.Length)
                {
                    if (data[eindex + 1] == 0)
                    {
                        ndata = new byte[eindex - sindex + 1];
                        Array.Copy(data, sindex, ndata, 0, ndata.Length);
                        ds[count] = GetDisplay(ndata);
                        count++;
                        if (eindex + 2 == data.Length)
                            break;
                        else
                        {
                            row[count] = data[eindex + 2];
                            col[count] = data[eindex + 3];
                            sindex = eindex + 4;
                            eindex = eindex + 4;
                        }
                    }
                    else
                    {
                        eindex++;
                    }
                }
                return 0;
            }
            return 1;
        }
        public int GetStringDisplay(string ip, int stringselect, ref string stringdisplay)//读时间显示字符串0302
        {
            string command = "0302";
            string appdata = stringselect.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int SetStringDisplay(string ip, int stringselect, ref string stringdisplay)//设置时间显示字符串0382
        {
            string command = "0382";
            byte[] ldata = Encoding.Default.GetBytes(stringdisplay);
            for (int i = 0; i < ldata.Length; i++)
                if (ldata[i] == 47) ldata[i] = 0;
            string appdata = stringselect.ToString("X2").PadLeft(2, '0') + byteTostring(ldata);
            if (ldata[ldata.Length - 1] != 0) appdata += "00";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x82, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int GetCardString(string ip, ref string stringdisplay)//读打卡结果字符串0303
        {
            string command = "0303";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int SetCardString(string ip, ref string stringdisplay)//设置打卡结果字符串0383
        {
            string command = "0383";
            byte[] ldata = Encoding.Default.GetBytes(stringdisplay);
            int count = 0;
            for (int i = 0; i < ldata.Length; i++)
                if (ldata[i] == 47)
                {
                    ldata[i] = 0;
                    count++;
                }
            string appdata = byteTostring(ldata);
            if (ldata[ldata.Length - 1] != 0)
            {
                count++;
                appdata += "00";
            }
            appdata = count.ToString("X2").PadLeft(2, '0') + appdata;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x83, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int GetWorkString(string ip, ref string stringdisplay)//读工作状态字符串0304
        {
            string command = "0304";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x04, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int SetWorkString(string ip, ref string stringdisplay)//设置工作状态字符串0384
        {
            string command = "0384";
            byte[] ldata = Encoding.Default.GetBytes(stringdisplay);
            int count = 0;
            for (int i = 0; i < ldata.Length; i++)
                if (ldata[i] == 47)
                {
                    ldata[i] = 0;
                    count++;
                }
            string appdata = byteTostring(ldata);
            if (ldata[ldata.Length - 1] != 0)
            {
                count++;
                appdata += "00";
            }
            appdata = count.ToString("X2").PadLeft(2, '0') + appdata;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x84, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int GetMenuDisplay(string ip, ref bool[] hadchild, ref bool[] hadnext, ref int[] flag, ref int[] editindex, ref string[] ds)//读选择界面显示设置0305
        {
            string command = "0305";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x05, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15], ndata;
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                int count = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == 0) count++;
                }
                hadchild = new bool[count];
                hadnext = new bool[count];
                ds = new string[count];
                flag = new int[count];
                editindex = new int[count];
                count = 0;
                int sindex = 2, eindex = 2;
                hadchild[0] = Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(0, 1) == "1";
                hadnext[0] = Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(1, 1) == "1";
                flag[0] = Convert.ToInt16(Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(2), 2);
                editindex[0] = data[1];
                while (eindex <= data.Length)
                {
                    if (data[eindex + 1] == 0)
                    {
                        ndata = new byte[eindex - sindex + 1];
                        Array.Copy(data, sindex, ndata, 0, ndata.Length);
                        ds[count] = Encoding.Default.GetString(ndata);
                        count++;
                        if (eindex + 2 == data.Length)
                            break;
                        else
                        {
                            hadchild[count] = Convert.ToString(data[eindex + 2], 2).PadLeft(8, '0').Substring(0, 1) == "1";
                            hadnext[count] = Convert.ToString(data[eindex + 2], 2).PadLeft(8, '0').Substring(1, 1) == "1";
                            flag[count] = Convert.ToInt16(Convert.ToString(data[eindex + 2], 2).PadLeft(8, '0').Substring(2), 2);
                            editindex[count] = data[eindex + 3];
                            sindex = eindex + 4;
                            eindex = eindex + 4;
                        }
                    }
                    else
                    {
                        eindex++;
                    }
                }
                return 0;
            }
            return 1;
        }
        public int SetMenuDisplay(string ip, ref bool[] hadchild, ref bool[] hadnext, ref int[] flag, ref int[] editindex, ref string[] ds)//设置选择界面显示设置0385
        {
            string command = "0385";
            string appdata = SetMenuDisplay(hadchild, hadnext, flag, editindex, ds);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x85, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15], ndata;
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                int count = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == 0) count++;
                }
                hadchild = new bool[count];
                hadnext = new bool[count];
                ds = new string[count];
                flag = new int[count];
                editindex = new int[count];
                count = 0;
                int sindex = 2, eindex = 2;
                hadchild[0] = Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(0, 1) == "1";
                hadnext[0] = Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(1, 1) == "1";
                flag[0] = Convert.ToInt16(Convert.ToString(data[0], 2).PadLeft(8, '0').Substring(2), 2);
                editindex[0] = data[1];
                while (eindex <= data.Length)
                {
                    if (data[eindex + 1] == 0)
                    {
                        ndata = new byte[eindex - sindex + 1];
                        Array.Copy(data, sindex, ndata, 0, ndata.Length);
                        ds[count] = Encoding.Default.GetString(ndata);
                        count++;
                        if (eindex + 2 == data.Length)
                            break;
                        else
                        {
                            hadchild[count] = Convert.ToString(data[eindex + 2], 2).PadLeft(8, '0').Substring(0, 1) == "1";
                            hadnext[count] = Convert.ToString(data[eindex + 2], 2).PadLeft(8, '0').Substring(1, 1) == "1";
                            flag[count] = Convert.ToInt16(Convert.ToString(data[eindex + 2], 2).PadLeft(8, '0').Substring(2), 2);
                            editindex[count] = data[eindex + 3];
                            sindex = eindex + 4;
                            eindex = eindex + 4;
                        }
                    }
                    else
                    {
                        eindex++;
                    }
                }
                return 0;
            }
            return 1;
        }
        public int GetDealTypeString(string ip, ref string stringdisplay)//读消费机交易方式字符串0306
        {
            string command = "0306";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x06, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int SetDealTypeString(string ip, ref string stringdisplay)//设置消费机交易方式字符串0386
        {
            string command = "0386";
            byte[] ldata = Encoding.Default.GetBytes(stringdisplay);
            int count = 0;
            for (int i = 0; i < ldata.Length; i++)
                if (ldata[i] == 47)
                {
                    ldata[i] = 0;
                    count++;
                }
            string appdata = byteTostring(ldata);
            if (ldata[ldata.Length - 1] != 0)
            {
                count++;
                appdata += "00";
            }
            appdata = count.ToString("X2").PadLeft(2, '0') + appdata;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x86, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int GetDealString(string ip, ref string stringdisplay)//读消费机交易模式字符串0307
        {
            string command = "0307";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x07, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int SetDealString(string ip, ref string stringdisplay)//设置消费机交易模式字符串0387
        {
            string command = "0387";
            byte[] ldata = Encoding.Default.GetBytes(stringdisplay);
            int count = 0;
            for (int i = 0; i < ldata.Length; i++)
                if (ldata[i] == 47)
                {
                    ldata[i] = 0;
                    count++;
                }
            string appdata = byteTostring(ldata);
            if (ldata[ldata.Length - 1] != 0)
            {
                count++;
                appdata += "00";
            }
            appdata = count.ToString("X2").PadLeft(2, '0') + appdata;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0x87, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 0) data[i] = Convert.ToByte('/');
                stringdisplay = Encoding.Default.GetString(data);
                return 0;
            }
            return 1;
        }
        public int SetDirectDisplay(string ip, int[] row, int[] col, string[] message, int seconds)//实时显示一屏信息03C1
        {
            string command = "03C1";
            string appdata = seconds.ToString("X2").PadLeft(4, '0');
            for (int i = 0; i < row.Length; i++)
            {
                appdata += row[i].ToString("X2").PadLeft(2, '0') + col[i].ToString("X2").PadLeft(2, '0') + byteTostring(Encoding.Default.GetBytes(message[i])) + "00";
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 2, data, 0, 2);
                int fm = byteToint(data);
                return 0;
            }
            return 1;
        }
        public int SetRollDisplay(string ip, int[] row, int[] col, string[] message, int seconds)//滚动显示一屏信息03C2
        {
            string command = "03C2";
            string appdata = seconds.ToString("X2").PadLeft(4, '0');
            for (int i = 0; i < row.Length; i++)
            {
                appdata += row[i].ToString("X2").PadLeft(2, '0') + col[i].ToString("X2").PadLeft(2, '0') + byteTostring(Encoding.Default.GetBytes(message[i])) + "00";
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x03, 0xC2, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 2, data, 0, 2);
                int fm = byteToint(data);
                return 0;
            }
            return 1;
        }
        #endregion

        #region 记录控制(04)
        public string GetRecordFormatS(string ip)//读记录存储格式0401
        {
            string rf = null;
            string command = "0401";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                rf = byteTostring(data);
                return DecFields(rf);
            }
            return rf;
        }
        public string SetRecordFormatS(string ip, string recordformat)//设置记录存储格式0481
        {
            string rf = null;
            string command = "0481";
            string appdata = EncFields(recordformat);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0x81, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                rf = byteTostring(data);
                return rf;
            }
            return rf;
        }
        public string GetRecordFormatR(string ip)//读记录返回格式0402
        {
            return DecFields(GetRecordFormatRP(ip));
        }
        public string SetRecordFormatR(string ip, string recordformat)//设置记录返回格式0482
        {
            string rf = null;
            string command = "0482";
            string appdata = EncFields(recordformat);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                rf = byteTostring(data);
                return rf;
            }
            return rf;
        }
        private string DecFields(string fields)
        {
            string str = "";
            for (int index = 0; index < fields.Length / 2; index++)
            {
                int type = Convert.ToInt16(fields.Substring(index * 2, 2), 16);
                if (type == 0) break;
                for (int i = 0; i < recordfield.Length; i++)
                {
                    if (recordfield[i].typeno == type)
                    {
                        str += recordfield[i].type + ",";
                        break;
                    }
                }
            }
            return str;
        }
        private string EncFields(string fields)
        {
            string str = "";
            string[] f = fields.Split(',');
            for (int index = 0; index < f.Length; index++)
            {
                for (int i = 0; i < recordfield.Length; i++)
                {
                    if (recordfield[i].type == f[index])
                    {
                        str += recordfield[i].typeno.ToString("X2").PadLeft(2, '0');
                        break;
                    }
                }
            }
            return str + "00";
        }
        private string GetRecordFormatRP(string ip)
        {
            string rf = null;
            string command = "0402";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, 13, data, 0, data.Length);
                rf = byteTostring(data);
                return rf;
            }
            return rf;
        }
        public string[] GetRecords(string ip, int type, int sum, int startindex, bool delete)//读记录0441
        {
            if (sum == 0) sum = 65535;
            string rtype = GetRecordFormatRP(ip);
            string[] allrecords = new string[0], newrecords, oldrecords;
            byte[] data = GetRecord(ip, Convert.ToInt16(type.ToString() + "1", 16), sum > 16 ? 16 : sum, startindex);
            while (data != null)
            {
                newrecords = FetchData(ip.Split(':')[0], data, rtype, Convert.ToInt32(ip.Split(':')[1], 16).ToString(), "");
                oldrecords = allrecords;
                allrecords = new string[allrecords.Length + newrecords.Length];
                Array.Copy(oldrecords, 0, allrecords, 0, oldrecords.Length);
                Array.Copy(newrecords, 0, allrecords, oldrecords.Length, newrecords.Length);
                if (allrecords.Length >= sum || newrecords.Length == 0) return allrecords;
                startindex += newrecords.Length;
                if (delete)
                    data = GetRecord(ip, Convert.ToInt16(type.ToString() + "2", 16), sum > 16 ? 16 : sum, startindex);
                else
                    data = GetRecord(ip, Convert.ToInt16(type.ToString() + "3", 16), sum > 16 ? 16 : sum, startindex);
            }
            return allrecords;
        }
        private byte[] GetRecord(string ip, int type, int sum, int startindex)//读记录0441
        {
            //string records = null;
            string command = "0441";
            string appdata = type.ToString("X2").PadLeft(2, '0');
            appdata += sum.ToString("X2").PadLeft(2, '0');
            appdata += startindex.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0x41, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 14];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                //records = byteTostring(data);
                return data;
            }
            return null;
        }
        //private string[] SynRecord(string ip, byte[] data)
        //{
        //    string rtype = GetRecordFormatRP(ip);
        //    return FetchData(ip.Split(':')[0], data, rtype, Convert.ToInt32(ip.Split(':')[1],16).ToString(),"");
        //}
        private string[] FetchData(string ip, byte[] data, string rtype, string macno, string serno)
        {
            int count = data[0];
            string[] datastr = new string[count];
            string str = "";
            int type, length = 0, startpoint = 2;
            byte[] bdata;
            try
            {
                for (int rindex = 0; rindex < count; rindex++)
                {
                    datastr[rindex] = "ip=" + ip + ",macno=" + macno + ",serno=" + serno + ",";
                    for (int index = 0; index < rtype.Length / 2; index++)
                    {
                        type = Convert.ToInt16(rtype.Substring(index * 2, 2), 16);
                        if (type == 0) break;
                        for (int i = 0; i < recordfield.Length; i++)
                        {
                            if (recordfield[i].typeno == type)
                            {
                                length = recordfield[i].len;
                                str = recordfield[i].type;
                                break;
                            }
                        }
                        bdata = new byte[length];
                        Array.Copy(data, startpoint, bdata, 0, length);
                        if (type == 3)//月日时分
                        {
                            datastr[rindex] += str + "=" + Convert.ToInt16(bdata[0]).ToString().PadLeft(2, '0') + Convert.ToInt16(bdata[1]).ToString().PadLeft(2, '0') + Convert.ToInt16(bdata[2]).ToString().PadLeft(2, '0') + Convert.ToInt16(bdata[3]).ToString().PadLeft(2, '0') + ",";
                        }
                        else
                        {
                            if (type == 4)
                                datastr[rindex] += str + "=" + Convert.ToInt16(bdata[0]).ToString().PadLeft(2, '0') + ",";
                            else if (type == 36 || type == 39 || type == 40 || type == 41 || type == 43 || type == 45 || type == 46)
                                datastr[rindex] += str + "=" + Encoding.ASCII.GetString(bdata) + ",";
                            else if (type == 37)
                                datastr[rindex] += str + "=" + (bdata[1] * 256 + bdata[0]).ToString() + ",";
                            else if (type >= 144 && type <= 175 || type == 42 || type == 44)
                                datastr[rindex] += str + "=" + Encoding.Default.GetString(bdata) + ",";
                            else
                                datastr[rindex] += str + "=" + byteTolong(bdata).ToString() + ",";
                        }
                        startpoint += length;
                    }
                    datastr[rindex] = SynDateTime(datastr[rindex]);
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                return new string[0];
            }
            return datastr;
        }
        private string SynDateTime(string data)
        {
            string str = "";
            if (data.IndexOf("Y2", 0) >= 0)
            {
                str = str + data.Substring(data.IndexOf("Y2", 0) + 3, 4);
                data = data.Substring(0, data.IndexOf("Y2", 0)) + data.Substring(data.IndexOf("Y2", 0) + 8);
            }
            if (data.IndexOf("Mdhm", 0) >= 0)
            {
                str = str + data.Substring(data.IndexOf("Mdhm", 0) + 5, 8);
                data = data.Substring(0, data.IndexOf("Mdhm", 0)) + data.Substring(data.IndexOf("Mdhm", 0) + 14);
            }
            if (data.IndexOf("second", 0) >= 0)
            {
                str = str + data.Substring(data.IndexOf("second", 0) + 7, 2);
                data = data.Substring(0, data.IndexOf("second", 0)) + data.Substring(data.IndexOf("second", 0) + 10);
            }
            return "datetime=" + str + "," + data;
        }
        public int[] GetRecordInfo(string ip)//读记录信息0442
        {
            int[] fm = new int[2];
            string command = "0442";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0x42, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                fm[1] = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int[] GetPhotoInfo(string ip)//读照片信息0443
        {
            int[] fm = new int[2];
            string command = "0443";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0x43, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                fm[1] = byteToint(data);
                return fm;
            }
            return null;
        }
        public bool ClearRecord(string ip)//记录删除04C1
        {
            DateTime start = DateTime.Now;

            string slen = "0003";
            string command = "04C1";
            string appdata = "01";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                bool ret = rdata[cnoindex + 1] == 0x00 ? true : false;
                return ret;
            }
            return false;
        }
        public bool DeleteRecord(string ip, int count)//记录删除04C1
        {
            string slen = "0007";
            string command = "04C1";
            string appdata = "02" + count.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                bool ret = rdata[cnoindex + 1] == 0x00 ? true : false;
                return ret;
            }
            return false;
        }
        public bool DeleteRecord(string ip, string date)//记录删除04C1
        {
            string slen = "0007";
            string command = "04C1";
            string appdata = "03" + intTohexstring(int.Parse(date.Substring(0, 4))).PadLeft(4, '0');//年
            appdata += intTohexstring(int.Parse(date.Substring(5, 2))).PadLeft(2, '0');//月
            appdata += intTohexstring(int.Parse(date.Substring(8, 2))).PadLeft(2, '0');//日
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                bool ret = rdata[cnoindex + 1] == 0x00 ? true : false;
                return ret;
            }
            return false;
        }
        public bool ClearPhoto(string ip)//照片删除04C2
        {
            DateTime start = DateTime.Now;

            string slen = "0003";
            string command = "04C2";
            string appdata = "01";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x04, 0xC2, ref rdata);
            if (rdata.Length > 0)
            {
                bool ret = rdata[cnoindex + 1] == 0x00 ? true : false;
                return ret;
            }
            return false;
        }
        #endregion

        #region 名单控制(05)
        public ListStoreFormat GetListStoredFormat(string ip)//读名单存储格式0501
        {
            ListStoreFormat lsf = new ListStoreFormat();
            string command = "0501";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 2, data, 0, data.Length);
                lsf.format = byteTostring(data);
                data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                lsf.len = Convert.ToInt16(data[0]);
                return lsf;
            }
            return lsf;
        }
        public ListStoreFormat SetListStoredFormat(string ip, string recordformat)//设置名单存储格式0581
        {
            ListStoreFormat lsf = new ListStoreFormat();
            string command = "0581";
            string appdata = recordformat;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0x81, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 15];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);
                lsf.format = byteTostring(data);
                data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                lsf.len = Convert.ToInt16(data[0]);
                return lsf;
            }
            return lsf;
        }
        public int[] GetListStoredParm(string ip)//读名单存储参数0502
        {
            int[] sp = new int[7];
            string command = "0502";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                sp[0] = byteToint(data);
                data = new byte[4];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                sp[1] = byteToint(data);
                Array.Copy(rdata, cnoindex + 6, data, 0, 4);
                sp[2] = byteToint(data);
                Array.Copy(rdata, cnoindex + 10, data, 0, 4);
                sp[3] = byteToint(data);
                Array.Copy(rdata, cnoindex + 14, data, 0, 4);
                sp[4] = byteToint(data);
                Array.Copy(rdata, cnoindex + 18, data, 0, 4);
                sp[5] = byteToint(data);
                Array.Copy(rdata, cnoindex + 22, data, 0, 4);
                sp[6] = byteToint(data);
                return sp;
            }
            return sp;
        }
        public int[] SetListStoredParm(string ip, int maxliststroed)//设置名单存储参数0582
        {
            int[] sp = new int[7];
            string command = "0582";
            string appdata = maxliststroed.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0x82, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                sp[0] = byteToint(data);
                data = new byte[4];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                sp[1] = byteToint(data);
                Array.Copy(rdata, cnoindex + 6, data, 0, 4);
                sp[2] = byteToint(data);
                Array.Copy(rdata, cnoindex + 10, data, 0, 4);
                sp[3] = byteToint(data);
                Array.Copy(rdata, cnoindex + 14, data, 0, 4);
                sp[4] = byteToint(data);
                Array.Copy(rdata, cnoindex + 18, data, 0, 4);
                sp[5] = byteToint(data);
                Array.Copy(rdata, cnoindex + 22, data, 0, 4);
                sp[6] = byteToint(data);
                return sp;
            }
            return sp;
        }
        public string GetListStoredControl(string ip)//读名单存储控制参数0503
        {
            string sc = null;
            string command = "0503";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                sc = Convert.ToString(data[0], 2).PadLeft(8, '0');
                return sc;
            }
            return sc;
        }
        public string SetListStoredControl(string ip, string storedcontrol)//设置名单存储控制参数0583
        {
            string sc = null;
            string command = "0583";
            string appdata = Convert.ToInt16(storedcontrol, 2).ToString("X2");
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0x83, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                sc = Convert.ToString(data[0], 2).PadLeft(8, '0');
                return sc;
            }
            return sc;
        }
        public bool DeleteWhiteList(string ip)//白名单删除05C1
        {
            string slen = "0003";
            string command = "05C1";
            string appdata = "11";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == 0x00)
                    return true;
                else
                    return false;
            }
            return false;
        }
        public bool DeleteWhiteList(string ip, Int64 cardno)//白名单删除05C1
        {
            string slen = "0007";
            string command = "05C1";
            string appdata = "01" + cardno.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == 0x00)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public bool DeleteBlackList(string ip)//黑名单删除05C1
        {
            string slen = "0003";
            string command = "05C1";
            string appdata = "14";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == 0x00)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public bool DeleteBlackList(string ip, Int64 cardno)//黑名单删除05C1
        {
            string slen = "0007";
            string command = "05C1";
            string appdata = "04" + cardno.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == 0x00)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public bool DeleteList(string ip)//名单删除05C1
        {
            DateTime start = DateTime.Now;

            string slen = "0003";
            string command = "05C1";
            string appdata = "17";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == 0x00)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public bool DeleteList(string ip, Int64 cardno)//名单删除05C1
        {
            DateTime start = DateTime.Now;

            string slen = "0007";
            string command = "05C1";
            string appdata = "07" + cardno.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                if (rdata[cnoindex + 1] == 0x00)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public int TestList(string ip, Int64 cardno)//测试名单05C2
        {
            int tl = -1;
            string command = "05C2";
            string appdata = cardno.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC2, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                tl = Convert.ToInt16(data[0]);
                if (tl > 0)
                {
                    Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                    string il = Convert.ToString(data[0], 2);
                    if (il[7] == '0') tl = -1;
                    if (il[5] == '0') tl = -2;
                }
                return tl;
            }
            return tl;
        }
        public void GetList(string ip, int count, ref string[] empno, ref string[] empname, ref long[] cardid)
        {
            ListField[] lf = GetList(ip, count);
            empno = new string[lf.Length];
            empname = new string[lf.Length];
            cardid = new long[lf.Length];
            for (int i = 0; i < lf.Length; i++)
            {
                empno[i] = lf[i].empno;
                empname[i] = lf[i].empname;
                cardid[i] = lf[i].cardid;
            }
        }
        public ListField[] GetList(string ip, int count)
        {
            ListField[] ret, rec, inter;
            if (count == 0)
            {
                count = 65535;
            }
            if (count < 16)
                ret = GetList(ip, count, 0);
            else
                ret = GetList(ip, 16, 0);
            int fcount = 16;// ret.Length;
            while (fcount < count)
            {
                rec = GetList(ip, 16, fcount);
                if (rec.Length == 0) break;
                fcount += rec.Length;
                inter = new ListField[ret.Length + rec.Length];
                Array.Copy(ret, 0, inter, 0, ret.Length);
                Array.Copy(rec, 0, inter, ret.Length, rec.Length);
                ret = inter;
            }
            return ret;
        }
        public ListField[] GetList(string ip, int count, Int64 startindex)//读名单05C3
        {
            ListField[] ls = new ListField[0];
            string command = "05C3";
            string appdata = count.ToString("X2").PadLeft(2, '0') + startindex.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC3, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[rlen - 14];
                Array.Copy(rdata, cnoindex + 1, data, 0, data.Length);

                ListStoreFormat lsf = GetListStoredFormat(ip);
                ls = GetList(data, lsf);
                return ls;
            }
            return ls;
        }
        private ListField[] GetList(byte[] bytedata, ListStoreFormat lsf)
        {
            int count = bytedata[0], counts = 0;
            int len = bytedata[1], startpoint = 2;
            byte[] data;
            for (int j = 0; j < count; j++)
            {
                if (Convert.ToString(bytedata[startpoint + j * len], 16).Substring(0, 1) == "7")
                    counts += 1;
            }
            ListField[] lists = new ListField[counts];
            int i = 0;
            while (i < counts)
            {
                if (Convert.ToString(bytedata[startpoint], 16).Substring(0, 1) == "7")
                {
                    int usedlen = 3;
                    for (int index = 0; index < lsf.format.Length / 2; index++)
                    {
                        switch (lsf.format.Substring(index * 2, 2))
                        {
                            case "84"://卡号4字节
                                data = new byte[4];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 4);
                                lists[i].cardid += byteTolong(data);
                                usedlen += 4;
                                break;
                            case "93"://工号4字节
                                data = new byte[4];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 4);
                                lists[i].empno += Encoding.GetEncoding("gb2312").GetString(data);
                                usedlen += 4;
                                break;
                            case "95"://工号6字节
                                data = new byte[6];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 6);
                                lists[i].empno += Encoding.GetEncoding("gb2312").GetString(data);
                                usedlen += 6;
                                break;
                            case "A7"://姓名8字节
                                data = new byte[8];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 8);
                                lists[i].empname += Encoding.GetEncoding("gb2312").GetString(data).Replace("\0", "");
                                usedlen += 8;
                                break;
                            case "11"://开门时间控制参数7字节
                                data = new byte[7];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 7);
                                lists[i].timecontrol += byteTostring(data);
                                usedlen += 7;
                                break;
                            case "F0"://补贴批次7字节
                                data = new byte[7];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 7);
                                lists[i].subsidyformat += byteTostring(data);
                                usedlen += 7;
                                break;
                            case "F1"://黑名单计数1字节
                                lists[i].blackcount = Convert.ToInt16(bytedata[startpoint + usedlen]);
                                usedlen += 1;
                                break;
                            case "F2"://补贴名单开始使用日期4字节
                                data = new byte[4];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 4);
                                lists[i].startsubsidy += byteTostring(data);
                                usedlen += 4;
                                break;
                            case "F3"://补贴名单有效截止日期4字节
                                data = new byte[4];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 4);
                                lists[i].endsubsidy += byteTostring(data);
                                usedlen += 4;
                                break;
                            case "F4"://补贴名单金额截止日期4字节
                                data = new byte[4];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 4);
                                lists[i].amountend += byteTostring(data);
                                usedlen += 4;
                                break;
                            case "0C"://个人密码（个人开门密码），3字节BCD格式存储
                                data = new byte[3];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 3);
                                lists[i].password += byteTostring(data);
                                usedlen += 3;
                                break;
                            case "60"://名单权限
                                data = new byte[1];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 1);
                                lists[i].listright += data[0];
                                usedlen += 1;
                                break;
                            case "61"://名单读头属性
                                data = new byte[1];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 1);
                                lists[i].readerright += data[0];
                                usedlen += 1;
                                break;
                            case "62"://名单日期段
                                data = new byte[8];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 8);
                                lists[i].listperiod += byteTostring(data);
                                usedlen += 8;
                                break;
                            case "63"://名单日期段
                                data = new byte[5];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 5);
                                lists[i].listvalid += byteTostring(data);
                                usedlen += 8;
                                break;
                            case "65"://节假日是否有效
                                data = new byte[1];
                                Array.Copy(bytedata, startpoint + usedlen, data, 0, 1);
                                lists[i].holidayvalid += data[0];
                                usedlen += 1;
                                break;
                            default:
                                break;
                        }
                    }
                    i += 1;
                }
                startpoint += len;
            }
            return lists;
        }
        /// <summary>
        /// 下载白名单
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="cardno">卡号</param>
        /// <param name="empno">工号</param>
        /// <param name="empname">姓名</param>
        /// <returns>0表示成功，其他失败</returns>
        public int DownList(string ip, ListField[] lf)
        {
            ListStoreFormat lsf = GetListStoredFormat(ip);
            for (int i = 0; i < lf.Length; i++)
            {
                lf[i].liststoredformat = lsf;
                if (lf[i].timecontrol == null) lf[i].timecontrol = "1".PadRight(56, '1');
            }
            int downloaded = 0, thisdowns = 0;
            while (downloaded < lf.Length)
            {
                thisdowns = lf.Length - downloaded;
                if (downloaded + 16 < lf.Length) thisdowns = 16;
                ListField[] downlf = new ListField[thisdowns];
                Array.Copy(lf, downloaded, downlf, 0, thisdowns);

                if (DownList(ip, lsf, downlf) != 0) return 1;
                downloaded += thisdowns;
            }
            return 0;
        }
        public int DownFinger(string ip, ListField[] lf, byte[][] fingertmp)
        {
            ListStoreFormat lsf = GetListStoredFormat(ip);
            for (int i = 0; i < lf.Length; i++)
            {
                lf[i].liststoredformat = lsf;
                if (lf[i].timecontrol == null) lf[i].timecontrol = "1".PadRight(56, '1');
                ListField[] downlf = new ListField[1];
                downlf[0] = lf[i];
                if (DownFinger(ip, lsf, downlf, fingertmp[i]) != 0) return 1;
            }
            return 0;
        }
        public int DownList(string ip, ref string[] empno, ref string[] empname, ref long[] cardid, int listtype)
        {
            try
            {
                ListField[] lf = new ListField[cardid.Length];
                for (int i = 0; i < lf.Length; i++)
                {
                    lf[i].cardid = cardid[i];
                    lf[i].empno = empno[i];
                    lf[i].empname = empname[i];
                    lf[i].listtype = listtype;
                }
                return DownList(ip, lf);
            }
            catch
            {
                return 1;
            }
        }
        /// <summary>
        /// 下载白名单
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="liststoredformat">名单存储格式</param>
        /// <param name="cardno">卡号</param>
        /// <param name="empno">工号</param>
        /// <param name="empname">姓名</param>
        /// <param name="timecontrol">打卡时间控制参数</param>
        /// <returns>0表示成功，其他失败</returns>
        private int DownList(string ip, ListStoreFormat liststoredformat, ListField[] lfs)//下载名单05C4
        {
            int r = 1;
            string command = "05C4";
            string appdata = lfs.Length.ToString("X2").PadLeft(2, '0') + liststoredformat.len.ToString("X2").PadLeft(2, '0') + ListDataLink(liststoredformat, lfs);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC4, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                r = Convert.ToInt16(data[0]);
                return r;
            }
            return r;
        }
        private int DownFinger(string ip, ListStoreFormat liststoredformat, ListField[] lfs, byte[] fingertmp)//下载指纹05C5
        {
            int r = 1;
            string command = "05C5";
            string appdata = lfs.Length.ToString("X2").PadLeft(2, '0') + liststoredformat.len.ToString("X2").PadLeft(2, '0') + ListDataLink(liststoredformat, lfs) + byteTostring(fingertmp);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC5, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                r = Convert.ToInt16(data[0]);
                return r;
            }
            return r;
        }
        private string ListDataLink(ListStoreFormat liststoreformat, ListField[] lfs)
        {
            string listdatas = "";
            for (int i = 0; i < lfs.Length; i++)
            {
                listdatas += ListDataLink(liststoreformat, lfs[i]);
            }
            return listdatas;
        }
        private string ListDataLink(ListStoreFormat liststoreformat, ListField lf)
        {
            string listdata = "0DFFFF";//"0DFFFF"考勤和门禁前三字节内容
            if (lf.listtype == 1) listdata = "7EFFFF";//黑名单前三字节内容
            if (lf.listtype == 2) listdata = "7CFFFF";//补贴名单前三字节内容
            for (int index = 0; index < liststoreformat.format.Length / 2; index++)
            {
                switch (liststoreformat.format.Substring(index * 2, 2))
                {
                    case "84":
                        listdata += lf.cardid.ToString("X2").PadLeft(8, '0');
                        break;
                    case "95":
                        listdata += byteTostring(Encoding.GetEncoding("gb2312").GetBytes(lf.empno)).PadLeft(12, '0');
                        break;
                    case "A7":
                        listdata += byteTostring(Encoding.GetEncoding("gb2312").GetBytes(lf.empname)).PadRight(16, '0').Substring(0, 16);
                        break;
                    case "11":
                        listdata += Convert.ToInt64(lf.timecontrol, 2).ToString("X2").PadLeft(14, 'F');
                        break;
                    case "F0":
                        listdata += lf.subsidyformat;
                        break;
                    case "F1":
                        listdata += lf.blackcount.ToString("X2").PadLeft(2, '0');
                        break;
                    case "F2":
                        listdata += lf.startsubsidy;
                        break;
                    case "F3":
                        listdata += lf.endsubsidy;
                        break;
                    case "F4":
                        listdata += lf.amountend;
                        break;
                    case "0C":
                        listdata += lf.password;
                        break;
                    case "60":
                        listdata += lf.listright.ToString("X2").PadLeft(2, '0');
                        break;
                    case "61":
                        listdata += lf.readerright.ToString("X2").PadLeft(2, '0');
                        break;
                    case "62":
                        listdata += lf.listperiod;
                        break;
                    case "63":
                        listdata += lf.listvalid;
                        break;
                    case "65":
                        listdata += lf.holidayvalid.ToString("X2").PadLeft(2, '0');
                        break;
                    default:
                        break;
                }

            }
            return listdata.PadRight(liststoreformat.len * 2, 'F');
        }
        public int TestFinger(string ip, Int64 cardno)//测试名单05C6
        {
            int tl = -1;
            string command = "05C6";
            string appdata = cardno.ToString("X2").PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC6, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 1);
                tl = Convert.ToInt16(data[0]);
                if (tl > 0)
                {
                    Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                    string il = Convert.ToString(data[0], 2);
                    if (il[7] == '0') tl = -1;
                    if (il[5] == '0') tl = -2;
                }
                return tl;
            }
            return tl;
        }
        public int GetFingerCount(string ip, ref int count)//测试名单05C7
        {
            string command = "05C7";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x05, 0xC7, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 2, data, 0, 2);
                count = byteToint(data);
                return rdata[cnoindex + 1];
            }
            return -1;
        }
        #endregion

        #region M1卡操作(06)
        public int SetM1Pass(string ip, string password, int passtype)//设置机具读M1卡的密码0601
        {
            int sc = -1;
            if (password.Length != 12) return 1;
            string command = "0601";
            byte[] pass1 = stringTobyte(password);
            byte[] pass2 = Encoding.ASCII.GetBytes("CM1520");
            byte[] pass3 = new byte[pass1.Length];
            for (int i = 0; i < pass1.Length; i++)
            {
                pass3[i] = (byte)~(pass1[i] ^ pass2[i]);
            }
            password = byteTostring(pass3);
            string appdata = passtype.ToString().PadLeft(2, '0') + password;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 2, data, 0, 6);
                string scstr = byteTostring(data);
                return 0;
            }
            return sc;
        }
        public int[] GetM1PassSet(string ip)//读机具Mifare卡密码选择0602
        {
            int[] ps = null;
            string command = "0602";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                ps = new int[3];
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                string psstring = Convert.ToString(data[0], 2).PadLeft(8, '0');
                ps[0] = int.Parse(psstring[7].ToString());
                ps[1] = int.Parse(psstring[0].ToString());
                ps[2] = int.Parse(psstring[6].ToString());
                return ps;
            }
            return ps;
        }
        public int SetM1PassSet(string ip, int passset)//设置机具Mifare卡密码选择0682
        {
            int ps = 0;
            string command = "0682";
            string appdata = passset.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x82, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                ps = Convert.ToInt32(data[0]);
                return ps;
            }
            return ps;
        }
        public int GetM1BaseSec(string ip)//读基本信息扇区号参数0603
        {
            int sec = 0;
            string command = "0603";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                sec = Convert.ToInt16(data[0]);
                return sec;
            }
            return sec;
        }
        public int SetM1BaseSec(string ip, int section)//设置基本信息扇区号参数0683
        {
            int sec = 0;
            string command = "0683";
            string appdata = section.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x83, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                sec = Convert.ToInt16(data[0]);
                return sec;
            }
            return sec;
        }
        public string[] GetM1CardValue(string ip)//读ID卡卡号取值方式0604
        {
            string[] sec = null;
            string command = "0604";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x04, ref rdata);
            if (rdata.Length > 0)
            {
                sec = new string[2];
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                sec[0] = Convert.ToString(data[0], 16);
                sec[1] = Convert.ToString(data[1], 16);
                return sec;
            }
            return sec;
        }
        public string[] SetM1CardValue(string ip, string[] valuetype)//设置ID卡卡号取值方式0684
        {
            string[] sec = null;
            string command = "0684";
            string appdata = valuetype[0] + valuetype[1];
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x84, ref rdata);
            if (rdata.Length > 0)
            {
                sec = new string[2];
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                sec[0] = Convert.ToString(data[0], 16);
                sec[1] = Convert.ToString(data[1], 16);
                return sec;
            }
            return sec;
        }
        public string SetM1CardPassword(string ip, string password)//设置M1卡密钥加密因子0685
        {
            string sec = null;
            string command = "0685";
            string appdata = int.Parse(password).ToString("X2").PadLeft(12, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x85, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 1, data, 0, 6);
                sec = Convert.ToInt64(data).ToString();
                return sec;
            }
            return sec;
        }

        public int GetM1PassKey(string ip, ref string password)//读卡尔M1卡格式金额加密密钥0640
        {
            string command = "0640";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0x40, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[16];
                Array.Copy(rdata, cnoindex + 2, data, 0, 16);
                password = Encoding.ASCII.GetString(data);
                return 0;
            }
            return -1;
        }
        public int SetM1PassKey(string ip, ref string password)//设置卡尔M1卡格式金额加密密钥06C0
        {
            string command = "06C0";
            byte[] pass = Encoding.ASCII.GetBytes(password);
            byte lastkey = (byte)(pass[0] ^ pass[1]);
            for (int i = 2; i < pass.Length; i++)
            {
                lastkey = (byte)(lastkey ^ pass[i]);
            }
            string appdata = "10" + byteTostring(Encoding.ASCII.GetBytes(password)) + Convert.ToString(lastkey, 16).PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x06, 0xC0, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[16];
                Array.Copy(rdata, cnoindex + 2, data, 0, 16);
                password = Encoding.ASCII.GetString(data);
                return 0;
            }
            return -1;
        }
        #endregion
        #region 字库操作(07)
        public int GetFontType(string ip)//读汉字库参数0701
        {
            int fm = 0;
            string slen = "0002";
            string command = "0701";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x07, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int SetFontType(string ip, int fonttype)//读汉字库参数0781
        {
            int fm = 0;
            string slen = "0003";
            string command = "0781";
            string appdata = fonttype.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x07, 0x81, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int DownFont(string ip, int filelength, int startindex, byte[] fontdata)//读汉字库参数07C1
        {
            int sum = 0;
            string command = "07C1";
            string appdata = filelength.ToString("X2").PadLeft(8, '0');
            appdata += startindex.ToString("X2").PadLeft(8, '0');
            appdata += fontdata.Length.ToString("X2").PadLeft(4, '0');
            for (int i = 0; i < fontdata.Length; i++)
            {
                sum += Convert.ToInt16(fontdata[i]);
            }
            appdata += sum.ToString("X2").PadLeft(8, '0');
            appdata += byteTostring(fontdata);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x07, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 6, data, 0, 4);
                if (startindex == byteToint(data))
                {
                    Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                    //savebuffer.RemoveAt(index);
                    //bufferlen.RemoveAt(index);
                    savebuffer.Clear();
                    bufferlen.Clear();
                    return data[0];
                }
            }
            return 1;
        }
        public int DownFont(string ip, byte[] fontdata)
        {
            byte[] data = new byte[1024];
            int len = fontdata.Length / 1024, last = fontdata.Length % 1024, iret = 0, wrong = 0;
            if (last > 0)
                len++;
            else
                last = 1024;
            for (int i = 0; i < len; i++)
            {
                if (i == len - 1) data = new byte[last];
                Array.Copy(fontdata, i * 1024, data, 0, data.Length);
                iret = DownFont(ip, fontdata.Length, i * 1024, data);
                if (iret != 0)
                {
                    i = i - i % 4 - 1;
                    wrong++;
                }
                else
                {
                    wrong = 0;
                }
                if (wrong > 3) return 0;
            }
            return 1;
        }
        #endregion

        #region 接口控制(08)
        public BellTime[] GetBellTimes(string ip)//读打铃时间参数0801
        {
            BellTime[] bt = null;
            string command = "0801";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x08, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                bt = new BellTime[rdata[cnoindex + 1]];
                byte[] data = new byte[5];
                for (int i = 0; i < bt.Length; i++)
                {
                    Array.Copy(rdata, cnoindex + 2 + i * 5, data, 0, 5);
                    bt[i].week = Convert.ToInt16(data[0]);
                    bt[i].hour = Convert.ToInt16(data[1]);
                    bt[i].minute = Convert.ToInt16(data[2]);
                    bt[i].second = Convert.ToInt16(data[3]);
                    bt[i].holdbell = Convert.ToInt16(data[4]);
                }
                return bt;
            }
            return bt;
        }
        public BellTime[] SetBellTime(string ip, BellTime[] belltime)//设置打铃时间参数0881
        {
            BellTime[] bt = null;
            string command = "0881";
            string appdata = belltime.Length.ToString("X2");
            for (int j = 0; j < belltime.Length; j++)
            {
                appdata += belltime[j].week.ToString("X2").PadLeft(2, '0');
                appdata += belltime[j].hour.ToString("X2").PadLeft(2, '0');
                appdata += belltime[j].minute.ToString("X2").PadLeft(2, '0');
                appdata += belltime[j].second.ToString("X2").PadLeft(2, '0');
                appdata += belltime[j].holdbell.ToString("X2").PadLeft(2, '0');
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x08, 0x81, ref rdata);
            if (rdata.Length > 0)
            {
                bt = new BellTime[rdata[cnoindex + 1]];
                byte[] data = new byte[5];
                for (int i = 0; i < bt.Length; i++)
                {
                    Array.Copy(rdata, cnoindex + 2 + i * 5, data, 0, 5);
                    bt[i].week = Convert.ToInt16(data[0]);
                    bt[i].hour = Convert.ToInt16(data[1]);
                    bt[i].minute = Convert.ToInt16(data[2]);
                    bt[i].second = Convert.ToInt16(data[3]);
                    bt[i].holdbell = Convert.ToInt16(data[4]);
                }
                return bt;
            }
            return bt;
        }
        /// <summary>
        /// 读外接指示灯控制参数
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>整形数组，0为Good灯亮时间，1为Error灯亮时间</returns>
        public int[] GetLightControl(string ip)//读外接指示灯控制参数0802
        {
            int[] lt = new int[2];
            string command = "0802";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x08, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                lt[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 2);
                lt[1] = byteToint(data);
                return lt;
            }
            return lt;
        }
        public int[] SetLightControl(string ip, int[] lighttime)//设置外接指示灯控制参数0882
        {
            int[] lt = new int[2];
            string command = "0882";
            string appdata = lighttime[0].ToString("X2").PadLeft(4, '0');
            appdata += lighttime[1].ToString("X2").PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x08, 0x82, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                lt[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 3, data, 0, 2);
                lt[1] = byteToint(data);
                return lt;
            }
            return lt;
        }
        public int GetLockHoldTime(string ip)//读电锁开门保持时间0803
        {
            int sec = 0;
            string command = "0803";
            string appdata = "";
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x08, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                sec = byteToint(data);
                return sec;
            }
            return sec;
        }
        public int SetLockHoldTime(string ip, int time)//设置电锁开门保持时间0883
        {
            int sec = 0;
            string command = "0883";
            string appdata = time.ToString("X2").PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x08, 0x83, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                sec = byteToint(data);
                return sec;
            }
            return sec;
        }
        /// <summary>
        /// 直接使外部接口做相应动作
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="control">16进制字符串形式的字节控制，2位表示一个字节</param>
        /// <returns>0成功，1~127其他成功，128+失败</returns>
        public int LinkControl(string ip, string control)//控制联机操作08C1
        {
            int sec = 0;
            string command = "08C1";
            string appdata = control;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x08, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                sec = byteToint(data);
                return sec;
            }
            return sec;
        }
        #endregion
        #region 设备语音操作(0A)
        public int SoundSpeak(string ip, int outtype, string sound)//控制联机操作08C1
        {
            string command = "0AC1";
            string appdata = outtype.ToString().PadLeft(2, '0');
            if (outtype == 0)
            {
                appdata += int.Parse(sound).ToString("X2").PadLeft(2, '0');
            }
            else
            {
                appdata += byteTostring(Encoding.Default.GetBytes(sound));
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x0A, 0xC1, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                return data[0];
            }
            return 2;
        }
        #endregion
        #region 升级操作(0F)
        public int[] UpdateHand(string ip)//升级握手0F01
        {
            int[] fm = null;
            string slen = "0008";
            string command = "0F01";
            string appdata = "655AA1255AA1";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x0F, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                fm = new int[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                fm[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                fm[1] = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int UpdateGrade(string ip, int filelength, int filesum, int startindex, byte[] updatedata)//升级0F02
        {
            int fm = 1, sum = 0;
            string command = "0F02";
            string appdata = filelength.ToString("X2").PadLeft(8, '0');
            appdata += filesum.ToString("X2").PadLeft(8, '0');
            appdata += startindex.ToString("X2").PadLeft(8, '0');
            appdata += updatedata.Length.ToString("X2").PadLeft(4, '0');
            for (int i = 0; i < updatedata.Length; i++)
            {
                sum += Convert.ToInt16(updatedata[i]);
            }
            appdata += sum.ToString("X2").PadLeft(8, '0');
            appdata += byteTostring(updatedata);
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x0F, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 19, data, 0, 1);
                fm = byteToint(data);
                return fm;
            }
            return fm;
        }
        public int UpdateGrade(string ip, byte[] updatedata)
        {
            byte[] data = new byte[1024];
            int len = updatedata.Length / 1024, last = updatedata.Length % 1024, iret = 0, wrong = 0, sum = 0;
            if (last > 0)
                len++;
            else
                last = 1024;
            for (int i = 0; i < updatedata.Length; i++)
            {
                sum += Convert.ToInt16(updatedata[i]);
            }
            int[] uh = UpdateHand(ip);
            for (int i = 0; i < len; i++)
            {
                if (i == len - 1) data = new byte[last];
                Array.Copy(updatedata, i * 1024, data, 0, data.Length);
                iret = UpdateGrade(ip, updatedata.Length, sum, i * 1024, data);
                if (iret != 0)
                {
                    i = i - i % 4 - 1;
                    wrong++;
                }
                else
                {
                    wrong = 0;
                }
                if (wrong > 3) return 0;
            }
            return 1;
        }





        #endregion

        #region 实时分析(09)
        private static string[] onlinemac = new string[500], recordtype = new string[500], machineno = new string[500], seriesno = new string[500];
        private static int maccount = 0;
        private List<PhotoData> pPhotoData = new List<PhotoData>();
        private struct PhotoData
        {
            public byte[] photo;
            public string ip;
            public int flowno;
        }
        private int Findstring(string[] arr, string value)
        {
            for (int i = 0; i < maccount; i++)
            {
                if (arr[i] == value) return i;
            }
            return -1;
        }
        private void Monitor(object o)
        {
            byte[] data = (o as MonitorParm).data; ;
            IPEndPoint ep = (o as MonitorParm).ep; ;
            int len = (o as MonitorParm).len; ;
            if (data[cnoindex] == 0x01)//设备登录
            {
                Logon(data, ep, len);
            }
            if (data[cnoindex] == 0x02)//心跳包
            {
                Dump(data, ep, len);
            }
            if (data[cnoindex] == 0x03)//数据包
            {
                SynData(data, ep, len);
            }
            if (data[cnoindex] == 0x04)//照片
            {
                SynPhoto(data, ep, len);
            }
            if (data[cnoindex] == 0xC0)//ID联机交易请求
            {
                DealApplyID(data, ep, len);
            }
            if (data[cnoindex] == 0xC1)//IC联机交易请求
            {
                DealApplyIC(data, ep, len);
            }
            if (data[cnoindex] == 0xC8)//联机交易请求
            {
                QueryCount(data, ep, len);
            }
            if (data[cnoindex] == 0xC9)//指纹/刷卡上传：由后台判断控制，无应答
            {
                DataCompared(data, ep, len);
            }
        }
        private void DataCompared(byte[] data, IPEndPoint ep, int len)
        {
            int index = Findstring(onlinemac, ep.Address.ToString());
            if (index < 0) return;
            byte[] length = new byte[2];
            Array.Copy(data, cnoindex + 10, length, 0, 2);
            byte[] fdata = new byte[byteToint(length)];
            Array.Copy(data, cnoindex + 12, fdata, 0, fdata.Length);

            if (DataCompareUpload != null)
            {
                DataCompareUpload(ep.Address.ToString() + ":" + machineno[index].PadLeft(4, '0'), fdata);
            }
            Thread.CurrentThread.Abort();
        }
        private void Logon(byte[] data, IPEndPoint ep, int len)
        {
            string slen = "0003";
            string command = "0901";
            string appdata = "00";
            string macaddr = getmacno(data);

            SendData(Getcommandline(m_syn, macaddr, slen, command, appdata), ep.Address, ep.Port);
            int index = Findstring(onlinemac, ep.Address.ToString());
            byte[] type = new byte[len - 38], serno = new byte[8];
            Array.Copy(data, cnoindex + 25, type, 0, len - 38);
            Array.Copy(data, cnoindex + 11, serno, 0, 8);

            if (index == -1)
            {
                onlinemac[maccount] = ep.Address.ToString();
                recordtype[maccount] = (byteTostring(type));
                machineno[maccount] = macaddr;
                seriesno[maccount] = byteTostring(serno);
                maccount++;
            }
            else
            {
                recordtype[index] = (byteTostring(type));
                machineno[index] = macaddr;
                seriesno[index] = byteTostring(serno);
            }
            if (MachineLoged != null)
            {
                MachineLoged(ep.Address.ToString() + "|" + ep.Port.ToString() + ":" + macaddr, index);
            }
            Thread.CurrentThread.Abort();
        }
        private void Dump(byte[] data, IPEndPoint ep, int len)
        {
            string slen = "000B";
            string command = "0902";
            string appdata = "01" + GetByteTime();
            string macaddr = getmacno(data);

            SendData(Getcommandline(m_syn, macaddr, slen, command, appdata), ep.Address, ep.Port);
            if (MachineDumped != null)
            {
                int addrlen = getaddrlength(data);
                string temp = "";
                if (len > addrlen + 16)
                {
                    switch (data[addrlen + 15])
                    {
                        case 1:
                            byte[] tdata = new byte[2];
                            Array.Copy(data, addrlen + 16, tdata, 0, 2);
                            int datalen = Convert.ToInt16(tdata);
                            for (int i = 0; i < datalen / 2; i++)
                            {
                                Array.Copy(data, addrlen + 18 + i * 2, tdata, 0, 2);
                                temp += ":" + Convert.ToInt16(tdata).ToString();
                            }
                            break;
                    }
                }
                MachineDumped(ep.Address.ToString() + ":" + macaddr + temp);
            }
            Thread.CurrentThread.Abort();
        }
        private void SynData(byte[] data, IPEndPoint ep, int len)
        {
            byte[] redata = new byte[2];
            Array.Copy(data, cnoindex + 1, redata, 0, 2);
            string slen = "0005";
            string command = "0903";
            string appdata = byteTostring(redata) + "00";
            string macaddr = (Convert.ToInt16(data[6]) * 256 + Convert.ToInt16(data[7])).ToString("X2").PadLeft(4, '0');
            if (data[5] == 2) macaddr = (Convert.ToInt16(macaddr, 16) * 65536 + Convert.ToInt16(data[8]) * 256 + Convert.ToInt16(data[9])).ToString("X2").PadLeft(8, '0');
            if (SaveData(data, ep, len))
            {
                SendData(Getcommandline(m_syn, macaddr, slen, command, appdata), ep.Address, ep.Port);
            }
            Thread.CurrentThread.Abort();
        }
        private bool SaveData(byte[] data, IPEndPoint ep, int len)
        {
            int index = Findstring(onlinemac, ep.Address.ToString());
            if (index >= 0)
            {
                string rtype = recordtype[index].Substring(2), macno = Convert.ToInt32(machineno[index], 16).ToString(), serno = seriesno[index];
                byte[] sdata = new byte[len - 15];
                Array.Copy(data, cnoindex + 2, sdata, 0, sdata.Length);
                string[] records = FetchData(ep.Address.ToString(), sdata, rtype, macno, serno);
                if (records.Length > 0)
                {
                    for (int i = 0; i < records.Length; i++)
                    {
                        if (DataReceived != null)
                        {
                            if (!DataReceived(records[i]))
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        private void SynPhoto(byte[] data, IPEndPoint ep, int len)
        {
            byte[] redata = new byte[18];
            Array.Copy(data, cnoindex + 1, redata, 0, 18);
            string slen = "0015";
            string command = "0904";
            string appdata = byteTostring(redata) + "00";
            string macaddr = (Convert.ToInt16(data[6]) * 256 + Convert.ToInt16(data[7])).ToString("X2").PadLeft(4, '0');
            if (data[5] == 2) macaddr = (Convert.ToInt16(macaddr, 16) * 65536 + Convert.ToInt16(data[8]) * 256 + Convert.ToInt16(data[9])).ToString("X2").PadLeft(8, '0');

            if (SavePhoto(data, ep, len))
            {
                SendData(Getcommandline(m_syn, macaddr, slen, command, appdata), ep.Address, ep.Port);
            }
            Thread.CurrentThread.Abort();
        }
        private bool SavePhoto(byte[] data, IPEndPoint ep, int len)
        {
            byte[] photoflowno = new byte[4], totallength = new byte[4], photoindex = new byte[4], sendlength = new byte[4];
            Array.Copy(data, cnoindex + 3, photoflowno, 0, 4);
            Array.Copy(data, cnoindex + 7, totallength, 0, 4);
            Array.Copy(data, cnoindex + 11, photoindex, 0, 4);
            Array.Copy(data, cnoindex + 15, sendlength, 0, 4);
            int flowno = byteToint(photoflowno);
            int total = byteToint(totallength);
            int index = byteToint(photoindex);
            int length = byteToint(sendlength);
            int macno = Convert.ToInt16(data[6]) * 256 + Convert.ToInt16(data[7]);
            if (data[5] == 2) macno = macno * 65536 + Convert.ToInt16(data[8]) * 256 + Convert.ToInt16(data[9]);

            if (index == 0)
            {
                PhotoData pdata = new PhotoData();
                pdata.photo = new byte[total];
                pdata.ip = ep.Address.ToString();
                pdata.flowno = flowno;
                Array.Copy(data, cnoindex + 19, pdata.photo, index, length);
                pPhotoData.Add(pdata);
            }
            else
            {
                foreach (PhotoData pd in pPhotoData)
                {
                    if (pd.ip == ep.Address.ToString() && pd.flowno == flowno)
                    {
                        Array.Copy(data, cnoindex + 19, pd.photo, index, length);
                        if (total == index + length)
                        {
                            bool f = SavePhoto(pd.photo, macno, flowno, total);
                            pPhotoData.Remove(pd);
                            if (f) return true;
                            else return false;
                        }
                    }
                }
            }

            return true;
        }
        private void DealApplyID(byte[] data, IPEndPoint ep, int len)
        {
            if (CardBlashed != null)
            {
                try
                {
                    byte[] bflowno = new byte[4], bcardno = new byte[8], bamount = new byte[4], bdealtype = new byte[1];
                    byte btype = data[cnoindex + 1];
                    if (btype == 1)
                    {
                        Array.Copy(data, cnoindex + 2, bflowno, 0, 4);
                        Array.Copy(data, cnoindex + 7, bcardno, 0, 8);
                        Array.Copy(data, cnoindex + 17, bdealtype, 0, 1);
                        Array.Copy(data, cnoindex + 18, bamount, 0, 4);
                    }
                    else
                    {
                        bamount = new byte[2];
                        Array.Copy(data, cnoindex + 3, bflowno, 0, 4);
                        Array.Copy(data, cnoindex + 7, bcardno, 0, 8);
                        Array.Copy(data, cnoindex + 15, bamount, 0, 2);
                    }
                    int type = Convert.ToInt16(btype);
                    int flowno = byteToint(bflowno);
                    long cardno = Convert.ToInt64(byteTostring(bcardno));
                    int amount = byteToint(bamount);
                    int dealtype = byteToint(bdealtype);
                    CardBlashResult cbr = CardBlashed(ep.Address, type, flowno, cardno, amount, dealtype);
                    if (type == 1)//申请才应签，确认不应答
                    {
                        string command = "09C0";
                        string appdata = "";
                        if (cbr.message == null)//存储过程调用出错
                        {
                            appdata = "01";
                            appdata += flowno.ToString("X2").PadLeft(8, '0');
                            appdata += cardno.ToString("X2").PadLeft(16, '0');
                            appdata += cbr.sysflow.ToString("X2").PadLeft(4, '0');
                            appdata += amount.ToString("X2").PadLeft(8, '0');
                            appdata += 0.ToString("X2").PadLeft(8, '0');
                            appdata += byteTostring(Encoding.GetEncoding("gb2312").GetBytes("数据库调用出错")) + "00";
                            appdata += byteTostring(Encoding.GetEncoding("gb2312").GetBytes(cbr.name)).PadRight(32, '0');
                            appdata += cbr.subdate.PadRight(8, '0');
                            appdata += cbr.subbatch.ToString("X2").PadLeft(4, '0');
                            string macaddr = (Convert.ToInt16(data[6]) * 256 + Convert.ToInt16(data[7])).ToString("X2").PadLeft(4, '0');
                            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

                            SendData(Getcommandline(m_syn, macaddr, slen, command, appdata), ep.Address, ep.Port);
                        }
                        else//正常返回值
                        {
                            appdata = cbr.result.ToString().PadLeft(2, '0');
                            appdata += flowno.ToString("X2").PadLeft(8, '0');
                            appdata += cardno.ToString().PadLeft(16, '0');
                            appdata += cbr.sysflow.ToString("X2").PadLeft(4, '0');
                            appdata += cbr.occur.ToString("X2").PadLeft(8, '0');
                            appdata += cbr.money.ToString("X2").PadLeft(8, '0');
                            appdata += byteTostring(Encoding.GetEncoding("gb2312").GetBytes(cbr.message)) + "00";
                            appdata += byteTostring(Encoding.GetEncoding("gb2312").GetBytes(cbr.name)).PadRight(32, '0');
                            appdata += cbr.subdate.PadRight(8, '0');
                            appdata += cbr.subbatch.ToString("X2").PadLeft(4, '0');
                            string macaddr = (Convert.ToInt16(data[6]) * 256 + Convert.ToInt16(data[7])).ToString("X2").PadLeft(4, '0');
                            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

                            SendData(Getcommandline(m_syn, macaddr, slen, command, appdata), ep.Address, ep.Port);
                        }
                    }
                }
                catch { }
            }
            Thread.CurrentThread.Abort();
        }
        private void DealApplyIC(byte[] data, IPEndPoint ep, int len)
        {
            try
            {
                byte[] tdata;
                byte btype = data[cnoindex + 1];
                string[] parms;
                int ret;
                if (btype == 1)//申请
                {
                    parms = new string[17];
                    tdata = new byte[4];
                    Array.Copy(data, cnoindex + 2, tdata, 0, 4);//终端机交易流水
                    parms[0] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 6, tdata, 0, 4);//卡号
                    parms[1] = byteToint(tdata).ToString();
                    tdata = new byte[1];
                    Array.Copy(data, cnoindex + 10, tdata, 0, 1);//黑名单标志
                    parms[2] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 11, tdata, 0, 1);//黑名单累计次数
                    parms[3] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 12, tdata, 0, 1);//钱包类型
                    parms[4] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 13, tdata, 0, 1);//交易类型
                    parms[5] = byteToint(tdata).ToString();
                    tdata = new byte[2];
                    Array.Copy(data, cnoindex + 14, tdata, 0, 2);//卡交易流水
                    parms[6] = byteToint(tdata).ToString();
                    tdata = new byte[4];
                    Array.Copy(data, cnoindex + 16, tdata, 0, 4);//交易金额
                    parms[7] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 20, tdata, 0, 4);//现金钱包金额
                    parms[8] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 24, tdata, 0, 4);//补贴钱包金额
                    parms[9] = byteToint(tdata).ToString();
                    tdata = new byte[2];
                    Array.Copy(data, cnoindex + 28, tdata, 0, 2);//补贴钱包批次号
                    parms[10] = byteToint(tdata).ToString();
                    tdata = new byte[4];
                    Array.Copy(data, cnoindex + 30, tdata, 0, 4);//补贴钱包有效日期
                    parms[11] = byteTostring(tdata);
                    tdata = new byte[6];
                    Array.Copy(data, cnoindex + 34, tdata, 0, 6);//最后消费时间
                    parms[12] = byteTostring(tdata);
                    tdata = new byte[4];
                    Array.Copy(data, cnoindex + 40, tdata, 0, 4);//日消费累加金额
                    parms[13] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 44, tdata, 0, 4);//月消费累加金额
                    parms[14] = byteToint(tdata).ToString();
                    tdata = new byte[1];
                    Array.Copy(data, cnoindex + 48, tdata, 0, 1);//日消费累加次数
                    parms[15] = byteToint(tdata).ToString();
                    tdata = new byte[2];
                    Array.Copy(data, cnoindex + 49, tdata, 0, 2);//月消费累加次数
                    parms[16] = byteToint(tdata).ToString();
                    if (CardBlashedIC != null)//外部处理
                    {
                        ret = CardBlashedIC(ep.Address, ref parms);
                    }
                    else//内部处理
                    {
                        ret = ICCardBlashDeal(ref parms);
                    }
                    string command = "09C1";
                    string appdata = ret.ToString("X2").PadLeft(2, '0');
                    appdata += uint.Parse(parms[0]).ToString("X2").PadLeft(8, '0');
                    appdata += uint.Parse(parms[1]).ToString("X2").PadLeft(8, '0');
                    appdata += uint.Parse(parms[2]).ToString("X2").PadLeft(2, '0');
                    appdata += uint.Parse(parms[3]).ToString("X2").PadLeft(2, '0');
                    appdata += uint.Parse(parms[4]).ToString("X2").PadLeft(2, '0');
                    appdata += uint.Parse(parms[5]).ToString("X2").PadLeft(2, '0');
                    appdata += uint.Parse(parms[6]).ToString("X2").PadLeft(4, '0');
                    appdata += uint.Parse(parms[7]).ToString("X2").PadLeft(8, '0');
                    appdata += uint.Parse(parms[8]).ToString("X2").PadLeft(8, '0');
                    appdata += uint.Parse(parms[9]).ToString("X2").PadLeft(8, '0');
                    appdata += uint.Parse(parms[10]).ToString("X2").PadLeft(4, '0');
                    appdata += parms[11].PadLeft(8, '0');
                    appdata += parms[12].PadLeft(12, '0');
                    appdata += uint.Parse(parms[13]).ToString("X2").PadLeft(8, '0');
                    appdata += uint.Parse(parms[14]).ToString("X2").PadLeft(8, '0');
                    appdata += uint.Parse(parms[15]).ToString("X2").PadLeft(2, '0');
                    appdata += uint.Parse(parms[16]).ToString("X2").PadLeft(4, '0');
                    appdata += uint.Parse(parms[17]).ToString("X2").PadLeft(8, '0');//交易前可用余额
                    appdata += uint.Parse(parms[18]).ToString("X2").PadLeft(8, '0');//交易后可用余额
                    appdata += parms[19].PadLeft(20, '0');//预留10字节
                    appdata += byteTostring(Encoding.GetEncoding("gb2312").GetBytes(parms[20])) + "00";
                    string macaddr = (Convert.ToInt16(data[6]) * 256 + Convert.ToInt16(data[7])).ToString("X2").PadLeft(4, '0');
                    string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

                    SendData(Getcommandline(m_syn, macaddr, slen, command, appdata), ep.Address, ep.Port);
                }
                else//确认
                {
                    parms = new string[7];
                    tdata = new byte[1];
                    Array.Copy(data, cnoindex + 2, tdata, 0, 1);//交易结果
                    parms[0] = byteToint(tdata).ToString();
                    tdata = new byte[4];
                    Array.Copy(data, cnoindex + 3, tdata, 0, 4);//终端机交易流水
                    parms[1] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 7, tdata, 0, 4);//卡号
                    parms[2] = byteToint(tdata).ToString();
                    tdata = new byte[1];
                    Array.Copy(data, cnoindex + 11, tdata, 0, 1);//黑名单标志
                    parms[3] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 12, tdata, 0, 1);//黑名单次数
                    parms[4] = byteToint(tdata).ToString();
                    tdata = new byte[2];
                    Array.Copy(data, cnoindex + 13, tdata, 0, 2);//系统卡交易流水
                    parms[5] = byteToint(tdata).ToString();
                    Array.Copy(data, cnoindex + 15, tdata, 0, 2);//补贴批次
                    parms[6] = byteToint(tdata).ToString();
                    if (CardBlashedIC != null)//外部处理
                    {
                        ret = CardBlashedIC(ep.Address, ref parms);
                    }
                }
            }
            catch { }
            Thread.CurrentThread.Abort();
        }
        private int ICCardBlashDeal(ref string[] parms)
        {
            string[] ret = new string[21];
            for (int i = 0; i < parms.Length; i++) ret[i] = parms[i];
            switch (parms[5])
            {
                case "1"://消费
                    if (uint.Parse(ret[8]) > uint.Parse(ret[7]))
                    {
                        ret[17] = ret[8];
                        ret[8] = (uint.Parse(ret[8]) - uint.Parse(ret[7])).ToString();
                        ret[18] = ret[8];
                        ret[19] = "";
                        ret[20] = "消费成功!";
                    }
                    else
                    {
                        ret[17] = ret[8];
                        ret[18] = "0";
                        ret[19] = "";
                        ret[20] = "余额不足!";
                    }
                    break;
                case "2"://充值
                    ret[17] = ret[8];
                    ret[8] = (uint.Parse(ret[8]) + uint.Parse(ret[7])).ToString();
                    ret[18] = ret[8];
                    ret[19] = "";
                    ret[20] = "充值成功!";
                    break;
                case "5"://撤消
                    ret[17] = ret[8];
                    ret[8] = (uint.Parse(ret[8]) + uint.Parse(ret[7])).ToString();
                    ret[18] = ret[8];
                    ret[19] = "";
                    ret[20] = "撤消成功!";
                    break;
                case "7"://清零补贴
                    ret[17] = ret[9];
                    ret[9] = uint.Parse(ret[7]).ToString();
                    ret[10] = (uint.Parse(ret[10]) + 1).ToString();
                    ret[18] = uint.Parse(ret[7]).ToString();
                    ret[19] = "";
                    ret[20] = "补贴成功!";
                    break;
                case "8"://累加补贴
                    ret[17] = ret[9];
                    ret[9] = (uint.Parse(ret[9]) + uint.Parse(ret[7])).ToString();
                    ret[10] = (uint.Parse(ret[10]) + 1).ToString();
                    ret[18] = ret[9];
                    ret[19] = "";
                    ret[20] = "补贴成功!";
                    break;
            }
            parms = ret;
            return 0;
        }
        private void QueryCount(byte[] data, IPEndPoint ep, int len)
        {
            byte[] macno = new byte[2];
            Array.Copy(data, cnoindex + 9, macno, 0, 2);
            string ip = ep.Address.ToString() + ":" + byteTostring(macno).PadLeft(4, '0');
            string[] message = new string[0];
            int[] row = new int[0], col = new int[0];
            int seconds = 0;
            if (QueryApplyed != null)
            {
                int r = QueryApplyed(ip, ref row, ref col, ref message, ref seconds);
                SetDirectDisplay(ip, row, col, message, seconds);
            }
            Thread.CurrentThread.Abort();
        }


        private bool SavePhoto(byte[] photo, int macno, int flowno, int len)
        {
            try
            {
                string filename = Application.StartupPath + "\\" + macno.ToString().PadLeft(4, '0') + flowno.ToString().PadLeft(6, '0') + ".jpg";// Application.StartupPath + "\\520PHOTO.jpg";
                MemoryStream ms = new MemoryStream(photo);
                Image image = System.Drawing.Image.FromStream(ms);
                System.IO.FileInfo info = new System.IO.FileInfo(filename);
                System.IO.Directory.CreateDirectory(info.Directory.FullName);
                File.WriteAllBytes(filename, photo);
                //GenerateHighThumbnail(filename, Application.StartupPath + "\\" + flowno.ToString() + ".jpg", 160, 120);
                if (PhotoReceivedX != null)
                {
                    PhotoReceivedX(macno, flowno);
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                return false;
            }
            return true;
        }
        #endregion

        #region 校情通部分
        public bool BroadcastX(string ip, string str)
        {
            string slen = "000A";
            string command = "0950";
            string appdata = byteTostring(Encoding.UTF8.GetBytes(str)).PadRight(16, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x09, 0x50, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                if (data[0] != 0)
                    return false;
            }
            return true;
        }
        private void MonitorX(object o)
        {
            byte[] data = (o as MonitorParm).data; ;
            IPEndPoint ep = (o as MonitorParm).ep; ;
            int len = (o as MonitorParm).len; ;
            switch (data[16])
            {
                case 0x09:
                    if (data[cnoindex + 6] == 0x01)//正常登录
                    {
                        Logon(data, ep, len);
                    }
                    if (data[cnoindex + 6] == 0x02)//正常心跳
                    {
                        Dump(data, ep, len);
                    }
                    if (data[cnoindex + 6] == 0x40)//心跳
                    {
                        DumpX(data, ep, len);
                    }
                    if (data[cnoindex + 6] == 0x41)//记录数据
                    {
                        SynDataX(data, ep, len);
                    }
                    if (data[cnoindex + 6] == 0x42)//照片数据
                    {
                        SynPhotoX(data, ep, len);
                    }
                    break;
                case 0x0B://京兆无线消费机
                    switch (data[cnoindex + 6])
                    {
                        case 0x01:
                            LogonJ(data, ep, len);
                            break;
                        case 0x02:
                            DumpJ(data, ep, len);
                            break;
                        case 0x03:
                            DealApplyJ(data, ep, len);
                            break;
                        default:
                            savebuffer.Add(data);
                            bufferlen.Add(len);
                            break;
                    }
                    break;
                case 0x0F:
                    UpdateAppX(data, ep, len);
                    break;
                case 0x0A:
                    ParamDownAppX(data, ep, len);
                    break;
                case 0x05:
                    ListDownAppX(data, ep, len);
                    break;
                default:
                    break;
            }
        }
        private void LogonJ(byte[] data, IPEndPoint ep, int len)
        {
            string slen = "0003";
            string command = "0B01";
            string appdata = "00";
            byte[] sierialno = new byte[8];
            Array.Copy(data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);
            if (MachineLoged != null) MachineLoged(ep.Address.ToString() + "|" + ep.Port.ToString() + ":0001", 0);
            SendData(Getcommandline(m_syn, "00000001", macaddr, slen, command, appdata), ep.Address, ep.Port);
        }
        private void DumpX(byte[] data, IPEndPoint ep, int len)
        {
            string slen = "000A";
            string command = "0940";
            string appdata = GetByteTime();
            byte[] sierialno = new byte[8];
            Array.Copy(data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);

            SendData(Getcommandline(m_syn, "00000001", macaddr, slen, command, appdata), ep.Address, ep.Port);
        }
        private void DumpJ(byte[] data, IPEndPoint ep, int len)
        {
            string slen = "000B";
            string command = "0B02";
            string appdata = "0A" + GetByteTime();
            byte[] sierialno = new byte[8];
            Array.Copy(data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);

            SendData(Getcommandline(m_syn, "00000001", macaddr, slen, command, appdata), ep.Address, ep.Port);
        }
        private void SynDataX(byte[] data, IPEndPoint ep, int len)
        {
            byte[] redata = new byte[3];
            Array.Copy(data, 18, redata, 0, 3);
            string slen = "0006";
            string command = "0941";
            string appdata = byteTostring(redata) + "00";
            byte[] sierialno = new byte[8];
            Array.Copy(data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);
            byte[] cmd = Getcommandline(m_syn, macaddr, slen, command, appdata);
            if (SaveDataX(data, ep, len))
            {
                SendData(cmd, ep.Address, ep.Port);
            }
        }
        private void SynDataX(object o)
        {
            MonitorParm rd = (MonitorParm)o;
            byte[] redata = new byte[3];
            Array.Copy(rd.data, 18, redata, 0, 3);
            string slen = "0006";
            string command = "0941";
            string appdata = byteTostring(redata) + "00";
            byte[] sierialno = new byte[8];
            Array.Copy(rd.data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);
            byte[] cmd = Getcommandline(m_syn, macaddr, slen, command, appdata);
            if (SaveDataX(rd.data, rd.ep, rd.len))
            {
                SendData(cmd, rd.ep.Address, rd.ep.Port);
            }
        }
        private void SynPhotoX(byte[] data, IPEndPoint ep, int len)
        {
            byte[] redata = new byte[18];
            Array.Copy(data, 18, redata, 0, 18);
            string slen = "0015";
            string command = "0942";
            string appdata = byteTostring(redata) + "00";
            byte[] sierialno = new byte[8];
            Array.Copy(data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);
            byte[] cmd = Getcommandline(m_syn, macaddr, slen, command, appdata);
            if (SavePhotoX(data, ep, len))
            {
                SendData(cmd, ep.Address, ep.Port);
            }
        }
        private void ParamDownX(byte[] data, IPEndPoint ep, int len)
        {
            byte[] redata = new byte[5];
            Array.Copy(data, 18, redata, 0, 5);
            string slen = "0008";
            string command = "0A41";
            string appdata = byteTostring(redata) + "00";
            byte[] sierialno = new byte[8];
            Array.Copy(data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);
            byte[] cmd = Getcommandline(m_syn, macaddr, slen, command, appdata);
            if (SavePhotoX(data, ep, len))
            {
                SendData(cmd, ep.Address, ep.Port);
            }
        }
        private void ParamDownAppX(byte[] data, IPEndPoint ep, int len)
        {
            byte[] redata = new byte[len];
            Array.Copy(data, 0, redata, 0, len);

            SendData(redata, ep.Address, ep.Port);
        }
        private void ListDownAppX(byte[] data, IPEndPoint ep, int len)
        {
            byte[] redata = new byte[4];
            Array.Copy(data, 18, redata, 0, 4);
            byte[] sum = new byte[4];
            Array.Copy(data, 26, sum, 0, 4);
            string slen = "000A";
            string command = "0540";
            string appdata = byteTostring(redata) + byteTostring(sum);
            byte[] sierialno = new byte[8];
            Array.Copy(data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);

            byte[] cmd = Getcommandline(m_syn, macaddr, slen, command, appdata);
            if (len > 20)
                SendData(cmd, ep.Address, ep.Port);
        }
        private void UpdateAppX(byte[] data, IPEndPoint ep, int len)
        {
            byte[] redata = new byte[4];
            Array.Copy(data, 22, redata, 0, 4);
            string slen = "000E";
            string command = "0F41";
            string appdata = byteTostring(redata) + "0000000000000000";
            byte[] sierialno = new byte[8];
            Array.Copy(data, 6, sierialno, 0, 8);
            string macaddr = byteTostring(sierialno);
            byte[] cmd = Getcommandline(m_syn, macaddr, slen, command, appdata);
            SendData(cmd, ep.Address, ep.Port);
        }
        private bool SaveDataX(byte[] data, IPEndPoint ep, int len)
        {
            int length = data[21], startpoint = 22, records = data[20], index = 0;
            string[] datastr = new string[records];
            byte[] bdata = new byte[4];
            try
            {
                while (index < records)
                {
                    datastr[index] = ep.Address.ToString() + ",";
                    Array.Copy(data, startpoint + index * length, bdata, 0, 4);
                    Int64 cardid = Convert.ToInt64(byteTostring(bdata), 16);
                    datastr[index] += cardid.ToString() + ",";
                    Array.Copy(data, startpoint + index * length + 4, bdata, 0, 4);
                    int photoflow = byteToint(bdata);
                    datastr[index] += photoflow.ToString() + ",";
                    Array.Copy(data, startpoint + index * length + 8, bdata, 0, 4);
                    int year = Convert.ToInt16(bdata[0]) * 256 + Convert.ToInt16(bdata[1]);
                    datastr[index] += year.ToString() + "-" + Convert.ToInt16(data[startpoint + index * length + 10]).ToString().PadLeft(2, '0') + "-";
                    datastr[index] += Convert.ToInt16(data[startpoint + index * length + 11]).ToString().PadLeft(2, '0') + " ";
                    datastr[index] += Convert.ToInt16(data[startpoint + index * length + 12]).ToString().PadLeft(2, '0') + ":";
                    datastr[index] += Convert.ToInt16(data[startpoint + index * length + 13]).ToString().PadLeft(2, '0') + ":";
                    datastr[index] += Convert.ToInt16(data[startpoint + index * length + 14]).ToString().PadLeft(2, '0') + ",";

                    //startpoint += length;
                    index += 1;
                }
                if (DataReceivedX != null)
                {
                    if (!DataReceivedX(datastr))
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private bool SavePhotoX(byte[] data, IPEndPoint ep, int len)
        {
            byte[] photoflowno = new byte[4], totallength = new byte[4], photoindex = new byte[4], sendlength = new byte[4];
            Array.Copy(data, 20, photoflowno, 0, 4);
            Array.Copy(data, 24, totallength, 0, 4);
            Array.Copy(data, 28, photoindex, 0, 4);
            Array.Copy(data, 32, sendlength, 0, 4);
            int flowno = byteToint(photoflowno);
            int total = byteToint(totallength);
            int index = byteToint(photoindex);
            int length = byteToint(sendlength);
            if (index == 0)
            {
                PhotoData pdata = new PhotoData();
                pdata.photo = new byte[total];
                pdata.ip = ep.Address.ToString();
                pdata.flowno = flowno;
                Array.Copy(data, 36, pdata.photo, index, length);
                pPhotoData.Add(pdata);
            }
            else
            {
                foreach (PhotoData pd in pPhotoData)
                {
                    if (pd.ip == ep.Address.ToString() && pd.flowno == flowno)
                    {
                        Array.Copy(data, 36, pd.photo, index, length);
                        if (total == index + length)
                        {
                            bool f = SavePhoto(pd.photo, 0, flowno, total);
                            pPhotoData.Remove(pd);
                            if (!f) return false;
                        }
                    }
                }
            }
            return true;
        }
        private void DealApplyJ(byte[] data, IPEndPoint ep, int len)
        {
            byte[] down = new byte[15], secdata = new byte[48];
            Array.Copy(data, 18, down, 0, 15);
            Array.Copy(data, 33, secdata, 0, 48);

            if (DealApplyedJ != null)
            {
                int[] com = new int[0];
                string[] content = new string[0];
                byte[] sierialno = new byte[8];
                Array.Copy(data, 6, sierialno, 0, 8);
                string macaddr = byteTostring(sierialno);
                int ret = DealApplyedJ(ep.Address.ToString() + "|" + ep.Port.ToString() + ":" + macaddr, byteTostring(down), secdata, ref com, ref content);
                if (ret == 0)
                {
                    string appdata = ret.ToString("X2").PadLeft(2, '0') + byteTostring(down) + com.Length.ToString("X2").PadLeft(2, '0');
                    for (int i = 0; i < com.Length; i++)
                    {
                        switch (com[i])
                        {
                            case 0x01:
                                appdata += "01" + content[i];
                                break;
                            case 0x02:
                                appdata += "02" + content[i];
                                break;
                            case 0x20:
                                appdata += "20" + content[i];
                                break;
                            case 0x30:
                                appdata += "30" + content[i].Substring(0, 4) + SetDisplayJ(content[i].Substring(4));
                                break;
                        }
                    }
                    string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');
                    string command = "0B03";
                    byte[] commandline = Getcommandline(m_syn, macaddr, slen, command, appdata);
                    SendData(commandline, ep.Address, ep.Port);
                }
            }
        }
        private string SetDisplayJ(string disp)
        {
            string[] dis = disp.Split(';');
            string rds = "";
            for (int i = 0; i < dis.Length; i++)
            {
                rds += int.Parse(dis[i].Substring(0, 2)).ToString("X2").PadLeft(2, '0');
                rds += int.Parse(dis[i].Substring(2, 2)).ToString("X2").PadLeft(2, '0');
                rds += byteTostring(Encoding.Default.GetBytes(dis[i].Substring(4)));
                rds += "00";
            }
            rds += "00";
            return rds;
        }
        public int SetControlMac(string ip, int[] com, string[] content)
        {
            string command = "0B04";
            string appdata = com.Length.ToString("X2").PadLeft(2, '0');
            for (int i = 0; i < com.Length; i++)
            {
                switch (com[i])
                {
                    case 0x01:
                        appdata += "01" + content[i];
                        break;
                    case 0x02:
                        appdata += "02" + content[i];
                        break;
                    case 0x20:
                        appdata += "20" + content[i];
                        break;
                    case 0x30:
                        appdata += "30" + content[i].Substring(0, 4) + SetDisplayJ(content[i].Substring(4));
                        break;
                }
            }
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x0B, 0x04, ref rdata);
            if (rdata.Length > 0)
            {
                return rdata[cnoindex + 1];
            }
            return -1;
        }
        public int SetDimentionJ(string ip, int time, byte[] dim)
        {
            string slen = "0204";
            string command = "0B05";
            string appdata = time.ToString("X2").PadLeft(4, '0') + byteTostring(dim);
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x0B, 0x05, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                return byteToint(data);
            }
            return -1;
        }

        #endregion

        #region 生产部分(80)
        public string GetMacAddrPro(string ip)//读设备地址，序列号8001
        {
            string slen = "0002";
            string command = "8001";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[8], macno = new byte[2];
                Array.Copy(rdata, cnoindex + 1, macno, 0, 2);
                string addr = byteToint(macno).ToString();
                Array.Copy(rdata, cnoindex + 3, data, 0, 8);
                addr += ":" + byteTostring(data);
                return addr;
            }
            return "";
        }
        public string SetMacAddrPro(string ip, int addr, string bcd)//设置设备地址，序列号8081
        {
            string slen = "000C";
            string command = "8081";
            string appdata = intTohexstring(addr);
            appdata = ("00" + appdata).Substring(appdata.Length - 2) + bcd;
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x81, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[8];
                Array.Copy(rdata, cnoindex + 3, data, 0, 8);
                return byteTostring(data);
            }
            return "";
        }
        public string[] GetIPPro(string ip)//读设备IP地址，MAC地址8002
        {
            string[] addr = new string[6];
            string slen = "0002";
            string command = "8002";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x02, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4], mac = new byte[6], port = new byte[2], auto = new byte[1];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                addr[0] = GetIP(data);
                Array.Copy(rdata, cnoindex + 6, data, 0, 4);
                addr[1] = GetIP(data);
                Array.Copy(rdata, cnoindex + 10, data, 0, 4);
                addr[2] = GetIP(data);
                Array.Copy(rdata, cnoindex + 14, port, 0, 2);
                addr[3] = byteToint(port).ToString();
                Array.Copy(rdata, cnoindex + 16, mac, 0, 6);
                addr[4] = byteTostring(mac);
                Array.Copy(rdata, cnoindex + 1, auto, 0, 1);
                addr[5] = byteTostring(auto);
                return addr;
            }
            return addr;
        }
        public string SetIPPro(string ip, string[] ips)//设置设备IP地址，MAC地址8082
        {
            string slen = "0017";
            string command = "8082";
            string appdata = ips[5];
            appdata += byteTostring(IPstringTobyte(ips[0]));
            appdata += byteTostring(IPstringTobyte(ips[1]));
            appdata += byteTostring(IPstringTobyte(ips[2]));
            appdata += intTohexstring(int.Parse(ips[3]));
            appdata += ips[4];
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x82, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 16, data, 0, 6);
                return byteTostring(data);
            }
            return "";
        }
        public string[] GetTypePro(string ip)//取机器类型8003
        {
            string[] addr = new string[2];
            string slen = "0002";
            string command = "8003";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                addr[0] = byteTostring(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                addr[1] = byteTostring(data);
                return addr;
            }
            return null;
        }
        public string[] SetTypePro(string ip, string type)//设置机器类型8083
        {
            string[] addr = new string[2];
            string slen = "0006";
            string command = "8083";
            string appdata = type.PadLeft(8, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x03, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 1, data, 0, 4);
                addr[0] = byteTostring(data);
                Array.Copy(rdata, cnoindex + 5, data, 0, 4);
                addr[1] = byteTostring(data);
                return addr;
            }
            return addr;
        }
        public int[] GetFlashInfoPro(string ip, int flashno)//取系统存储信息8004
        {
            int[] addr = new int[2];
            string slen = "0003";
            string command = "8004";
            string appdata = flashno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x04, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[4];
                Array.Copy(rdata, cnoindex + 2, data, 0, 4);
                addr[0] = byteToint(data);
                Array.Copy(rdata, cnoindex + 6, data, 0, 4);
                addr[1] = byteToint(data);
                return addr;
            }
            return null;
        }
        public byte[] GetFlashMemPro(string ip, int flashtype, int address, int len)//取存储器内容8005
        {
            string command = "8005";
            string appdata = flashtype.ToString("X2").PadLeft(2, '0');
            appdata += address.ToString("X2").PadLeft(8, '0');
            appdata += len.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x05, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 2, data, 0, 2);
                int length = byteToint(data);
                data = new byte[length];
                Array.Copy(rdata, cnoindex + 4, data, 0, length);
                return data;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip">IP地址(包括4位机号，中间用冒号隔开)</param>
        /// <param name="flashtype">存储器类型：0外部FLASH；1内部FLASH</param>
        /// <param name="address">存储器地址</param>
        /// <param name="optype">操作类型</param>
        /// <param name="len">数据长度</param>
        /// <param name="data">要写入的字节数据</param>
        /// <returns>0成功；其他失败,-1无返回结果</returns>
        public int SetFlashMemPro(string ip, int flashtype, int address, int optype, int len, byte[] data)//写存储器内容8085
        {
            int ret = -1;
            string command = "8085";
            string appdata = flashtype.ToString("X2").PadLeft(2, '0');
            appdata += address.ToString("X2").PadLeft(8, '0');
            appdata += optype.ToString("X2").PadLeft(2, '0');
            appdata += len.ToString("X2").PadLeft(2, '0');
            appdata += byteTostring(data) + GetFixSum(data).ToString("X2").PadLeft(2, '0');

            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x85, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] ndata = new byte[1];
                Array.Copy(rdata, cnoindex + 1, ndata, 0, 1);
                ret = ndata[0];
                return ret;
            }
            return ret;
        }
        private byte[] GetAuthorizedPro(string ip)//取产品唯一识别码8006
        {
            byte[] authorized = null;
            string slen = "0002";
            string command = "8006";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x06, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[12];
                Array.Copy(rdata, cnoindex + 1, data, 0, 12);
                authorized = data;
                return authorized;
            }
            return authorized;
        }
        public string SetAuthorizedPro(string ip)//设置产品授权码8086
        {
            byte[] author = GetAuthorizedPro(ip);
            if (author == null)
            {
                return null;
            }
            string addr = null;
            byte[] pass1 = new byte[16];
            Array.Copy(author, 0, pass1, 0, 12);
            Array.Copy(Encoding.ASCII.GetBytes("D520"), 0, pass1, 12, 4);
            byte[] pass2 = Encoding.ASCII.GetBytes("DLC520CANNOTCOPY");
            byte[] pass3 = Encoding.ASCII.GetBytes(GetMacAddrPro(ip).Split(':')[1]);
            byte[] pass4 = new byte[pass1.Length];
            for (int i = 0; i < pass1.Length; i++)
            {
                pass4[i] = (byte)(pass3[i] + (pass1[i] ^ pass2[i]));
            }
            string appdata = byteTostring(pass4);
            string slen = "0012";
            string command = "8086";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x86, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[16];
                Array.Copy(rdata, cnoindex + 1, data, 0, 16);
                addr = byteTostring(data);
                return addr;
            }
            return addr;
        }
        public int TestAuthorizedPro(string ip)//测试产品授权8087
        {
            int authorized = 0;
            string slen = "0002";
            string command = "8087";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x87, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                authorized = data[0];
                return authorized;
            }
            return -1;
        }
        public string[] GetPasswordPro(string ip)//取通信密钥8008
        {
            string[] addr = new string[2];
            string slen = "0002";
            string command = "8008";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x08, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 1, data, 0, 6);
                addr[0] = byteTostring(data);
                Array.Copy(rdata, cnoindex + 7, data, 0, 6);
                addr[1] = byteTostring(data);
                return addr;
            }
            return null;
        }
        public string[] SetPasswordPro(string ip, string password, string passkey)//设置通信密钥8088
        {
            if (password.Length != 12 || passkey.Length != 12) return null;
            string command = "8088";
            byte[] pass1 = stringTobyte(password);
            byte[] pass2 = stringTobyte(passkey);
            byte[] pass3 = new byte[pass1.Length];
            for (int i = 0; i < pass1.Length; i++)
            {
                pass3[i] = (byte)(pass1[i] ^ pass2[i]);
            }
            password = byteTostring(pass3);
            string appdata = byteTostring(pass2) + password;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x88, ref rdata);
            if (rdata.Length > 0)
            {
                string[] sc = new string[2];
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 1, data, 0, 6);
                sc[0] = byteTostring(data);
                Array.Copy(rdata, cnoindex + 7, data, 0, 6);
                sc[0] = byteTostring(data);
                return sc;
            }
            return null;
        }
        public string GetM1PassPro(string ip)//取M1卡密钥加密因子8009
        {
            string addr = null;
            string slen = "0002";
            string command = "8009";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x09, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 1, data, 0, 6);
                addr = Encoding.ASCII.GetString(data);
                return addr;
            }
            return addr;
        }
        public string SetM1PassPro(string ip, string pass)//设置M1卡密钥加密因子8089
        {
            if (pass.Length != 6) return null;
            string addr = null;
            string command = "8089";
            string appdata = byteTostring(Encoding.ASCII.GetBytes(pass));
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x89, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[6];
                Array.Copy(rdata, cnoindex + 1, data, 0, 6);
                return Encoding.ASCII.GetString(data);
            }
            return addr;
        }
        public int GetComMacTypePro(string ip, int comno)//取串口设备类型800A
        {
            string slen = "0003";
            string command = "800A";
            string appdata = comno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x0A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                int ret = data[1];
                return ret;
            }
            return -1;
        }
        public int SetComMacTypePro(string ip, int comno, int macno)//设置串口设备类型808A
        {
            string command = "808A";
            string appdata = comno.ToString("X2").PadLeft(2, '0') + macno.ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x8A, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[2];
                Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                int ret = data[1];
                return ret;
            }
            return -1;
        }
        public int GetLEDSetPro(string ip, ref int displayno, ref int displaytype)//取系统液晶类型及显示模式800B
        {
            string slen = "0002";
            string command = "800B";
            string appdata = "";
            string macaddr = ip.Split(':')[1];

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x0B, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                string strdata = Convert.ToString(data[0], 2).PadLeft(8, '0');
                displayno = Convert.ToInt16(strdata.Substring(0, 4), 2);
                displaytype = int.Parse(strdata[7].ToString());
                return 0;
            }
            return -1;
        }
        public int SetLEDSetPro(string ip, ref int displayno, ref int displaytype)//设置系统液晶类型及显示模式808B
        {
            string command = "808B";
            string appdata = Convert.ToInt16(Convert.ToString(displayno, 2).PadLeft(4, '0') + "000" + displaytype.ToString(), 2).ToString("X2").PadLeft(2, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0x80, 0x8B, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                string strdata = Convert.ToString(data[0], 2).PadLeft(8, '0');
                displayno = Convert.ToInt16(strdata.Substring(0, 4), 2);
                displaytype = int.Parse(strdata[7].ToString());
                return 0;
            }
            return -1;
        }
        #endregion

        #region 班播机FD
        public int BroadCastMessage(string ip, int macno, string info, int chartype)//播放语音FD01
        {
            string command = "FD01";
            string appdata;
            switch (chartype)
            {
                case 0:
                    appdata = chartype.ToString("X2") + byteTostring(Encoding.GetEncoding("GB2312").GetBytes(info));
                    break;
                case 1:
                    appdata = chartype.ToString("X2") + byteTostring(Encoding.GetEncoding("GBK").GetBytes(info));
                    break;
                case 2:
                    appdata = chartype.ToString("X2") + byteTostring(Encoding.GetEncoding("BIG5").GetBytes(info));
                    break;
                case 3:
                    appdata = chartype.ToString("X2") + byteTostring(Encoding.Unicode.GetBytes(info));
                    break;
                default:
                    appdata = chartype.ToString("X2") + byteTostring(Encoding.GetEncoding("GB2312").GetBytes(info));
                    break;
            }
            appdata = macno.ToString("X2").PadLeft(4, '0') + (appdata.Length / 2).ToString("X2").PadLeft(4, '0') + appdata;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0xFD, 0x01, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                return data[0];
            }
            return -1;
        }
        public int StopBroadCast(string ip, int macno)//停止语音FD05
        {
            string command = "FD05";
            string appdata = macno.ToString("X2").PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0xFD, 0x05, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                return data[0];
            }
            return -1;
        }
        public int MachineStatus(string ip, int macno)//机器状态FD10
        {
            string command = "FD10";
            string appdata = macno.ToString("X2").PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0xFD, 0x10, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                return data[0];
            }
            return -1;
        }
        public int MachineStatus100(string ip, int macno, ref int lossrate)//机器状态FD11(100次通信测试)
        {
            int received = 0;
            string command = "FD11";
            string appdata = macno.ToString("X2").PadLeft(4, '0');
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0xFD, 0x11, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                if (rlen == 20)
                {
                    data = new byte[2];
                    Array.Copy(rdata, cnoindex + 1, data, 0, 2);
                    lossrate = byteToint(data);
                    return 0;
                }
                else
                {
                    Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                    if (data[0] != 0) lossrate++;
                    if (received == 100)
                    {
                        return 0;
                    }
                }
            }
            return -1;
        }
        public int SendMessage(string ip, int macno, string info)//透传语音FD20
        {
            string command = "FD20";
            string appdata = info;// byteTostring(Encoding.Default.GetBytes(info));

            appdata = macno.ToString("X2").PadLeft(4, '0') + (appdata.Length / 2).ToString("X2").PadLeft(4, '0') + appdata;
            string macaddr = ip.Split(':')[1];
            string slen = (appdata.Length / 2 + 2).ToString("X2").PadLeft(4, '0');

            byte[] rdata = new byte[0];
            int rlen = GetReturn(ip.Split(':')[0], Getcommandline(m_syn, macaddr, slen, command, appdata), 0xFD, 0x20, ref rdata);
            if (rdata.Length > 0)
            {
                byte[] data = new byte[1];
                Array.Copy(rdata, cnoindex + 1, data, 0, 1);
                return data[0];
            }
            return -1;
        }

        #endregion
        #region 图片缩放
        //使用方法调用GenerateHighThumbnail()方法即可
        //参数oldImagePath表示要被缩放的图片路径
        //参数newImagePath表示缩放后保存的图片路径
        //参数width和height分别是缩放范围宽和高
        public void GenerateHighThumbnail(string oldImagePath, string newImagePath, int width, int height)
        {
            System.Drawing.Image oldImage = System.Drawing.Image.FromFile(oldImagePath);
            int newWidth = AdjustSize(width, height, oldImage.Width, oldImage.Height).Width;
            int newHeight = AdjustSize(width, height, oldImage.Width, oldImage.Height).Height;

            //。。。。。。。。。。。

            System.Drawing.Image thumbnailImage = oldImage.GetThumbnailImage(newWidth, newHeight, new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(thumbnailImage);


            //处理JPG质量的函数
            System.Drawing.Imaging.ImageCodecInfo ici = GetEncoderInfo("image/jpeg");
            if (ici != null)
            {
                System.Drawing.Imaging.EncoderParameters ep = new System.Drawing.Imaging.EncoderParameters(1);
                ep.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);
                bm.Save(newImagePath, ici, ep);

                //释放所有资源，不释放，可能会出错误。
                ep.Dispose();
                ep = null;
            }
            ici = null;

            bm.Dispose();
            bm = null;

            thumbnailImage.Dispose();
            thumbnailImage = null;
            oldImage.Dispose();
            oldImage = null;
        }
        private bool ThumbnailCallback()
        {
            return false;
        }
        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        public struct PicSize
        {
            public int Width;
            public int Height;
        }
        public PicSize AdjustSize(int spcWidth, int spcHeight, int orgWidth, int orgHeight)
        {
            PicSize size = new PicSize();
            // 原始宽高在指定宽高范围内，不作任何处理 
            if (orgWidth <= spcWidth && orgHeight <= spcHeight)
            {
                size.Width = orgWidth;
                size.Height = orgHeight;
            }
            else
            {
                // 取得比例系数 
                float w = orgWidth / (float)spcWidth;
                float h = orgHeight / (float)spcHeight;
                // 宽度比大于高度比 
                if (w > h)
                {
                    size.Width = spcWidth;
                    size.Height = (int)(w >= 1 ? Math.Round(orgHeight / w) : Math.Round(orgHeight * w));
                }
                // 宽度比小于高度比 
                else if (w < h)
                {
                    size.Height = spcHeight;
                    size.Width = (int)(h >= 1 ? Math.Round(orgWidth / h) : Math.Round(orgWidth * h));
                }
                // 宽度比等于高度比 
                else
                {
                    size.Width = spcWidth;
                    size.Height = spcHeight;
                }
            }
            return size;
        }
        #endregion
    }
}
