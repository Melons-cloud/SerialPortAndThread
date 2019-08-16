using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
namespace Server
{
    public partial class Server : Form
    {



        int time;
       

        SerialPort sp = null;    //声明一个串口类
        bool isOpen = false;    //打开串口标志位
        bool isSetProperty = false;   //属性设置标志位
        bool isHex = false;   //十六进制显示标志位

        
        public Server()
        {
            InitializeComponent();

            //关闭对文本框的非法线程操作检查
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }

        Thread threadWatch = null; //负责监听客户端的线程
        Socket socketWatch = null; //负责监听客户端的套接字

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;

            for (int i = 0; i < 10; i++) {

                cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
            }
            cbxCOMPort.SelectedIndex = 0;

            //列出常用的波特率
            cbxBaudBate.Items.Add("1200");
            cbxBaudBate.Items.Add("2400");
            cbxBaudBate.Items.Add("4800");
            cbxBaudBate.Items.Add("9600");
            cbxBaudBate.Items.Add("19200");
            cbxBaudBate.Items.Add("38400");
            cbxBaudBate.Items.Add("43000");
            cbxBaudBate.Items.Add("56000");
            cbxBaudBate.Items.Add("57600");
            cbxBaudBate.Items.Add("115200");
            cbxBaudBate.SelectedIndex = 5;
            //列出停止位
            cbxStopBits.Items.Add("0");
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 1;
            //列出数据位
            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;

            //列出奇偶校验位
            cbxParity.Items.Add("无");
            cbxParity.Items.Add("奇校验");
            cbxParity.Items.Add("偶校验");
            cbxParity.SelectedIndex = 0;

            //默认为Char显示
            rbnChar.Checked = true;
            
            

        }

        private void BtnServerConn_Click(object sender, EventArgs e)
        {
            //定义一个套接字用于监听客户端发来的信息  包含3个参数(IP4寻址协议,流式连接,TCP协议)
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //服务端发送信息 需要1个IP地址和端口号
            IPAddress ipaddress = IPAddress.Parse(txtIP.Text.Trim()); //获取文本框输入的IP地址
            //将IP地址和端口号绑定到网络节点endpoint上 
            IPEndPoint endpoint = new IPEndPoint(ipaddress, int.Parse(txtPORT.Text.Trim())); //获取文本框上输入的端口号
            //监听绑定的网络节点
            socketWatch.Bind(endpoint);
            //将套接字的监听队列长度限制为20
            socketWatch.Listen(20);
            //创建一个监听线程 
            threadWatch = new Thread(WatchConnecting);
            //将窗体线程设置为与后台同步
            threadWatch.IsBackground = true;
            //启动线程
            threadWatch.Start();
            //启动线程后 txtMsg文本框显示相应提示
            txtMsg.AppendText("开始监听客户端传来的信息!" + "\r\n");
        }
        //创建一个负责和客户端通信的套接字 
        Socket socConnection = null;

        //监听客户端发来的请求


        private void WatchConnecting()
        {
            while (true)  //持续不断监听客户端发来的请求
            {
                socConnection = socketWatch.Accept();
                txtMsg.AppendText("客户端连接成功" + "\r\n");
                //创建一个通信线程 
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ServerRecMsg);
                Thread thr = new Thread(pts);
                thr.IsBackground = true;
                //启动线程
                thr.Start(socConnection);
            }
        }
        //发送信息到客户端的方法
        private void ServerSendMsg(string sendMsg) {

            //将输入的字符串转化成机器可以识别的字节数组
            byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //向客户端发送字节数组信息
            socConnection.Send(arrSendMsg);
            //将发送的字符串信息附加到文本框txtMsg上
            txtMsg.AppendText("服务器:" + sendMsg + "\r\n");

        }

        //接收客户端发来的信息

        private void ServerRecMsg(object socketClientPara)
        {

            Socket socketServer = socketClientPara as Socket;
            while (true) {

                //创建一个内存缓冲区 其大小为1024*1024字节  即1M
                byte[] arrServerRecMsg = new byte[1024 * 1024];
                //将接收到的信息存入到内存缓冲区,并返回其字节数组的长度
                int length = socketServer.Receive(arrServerRecMsg);
                //将机器接受到的字节数组转换为人可以读懂的字符串
                string strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
                //将发送的字符串信息附加到文本框txtMsg上  
                txtMsg.AppendText("客户端:"  + strSRecMsg + "\r\n");
                //将发送的信息追加到串口数据发送文本中
                tbxSendData.AppendText(strSRecMsg + "\r\n");
                btnSend.PerformClick();


            }
        }

        //发送信息到客户端


        //获取当前系统时间
       
        //发送信息到客户端
        private void BtnSendMsg_Click(object sender, EventArgs e)
        {
            string str = txtSecond.Text;   //将下拉框内容添加到一个变量中
           // MessageBox.Show(txtSecond.Text);
            //MessageBox.Show(txtSecond.SelectedText);
            time = int.Parse(str);   //得到设定定时值(整型)
            System.Threading.Thread.Sleep(time);
            //调用ServerSendMsg方法,发送信息到客户端
            ServerSendMsg(txtSendMsg.Text.Trim());
            if (cbTimeSend.Checked)
            {
                tmSend.Enabled = true;

                
            }
            else {

                tmSend.Enabled = false;
            }
        }

        private void txtSendMsg_KeyDown(object sender, KeyEventArgs e) {
            //如果用户按下了Enter键
            if (e.KeyCode == Keys.Enter) {
                //则调用服务器向客户端发送信息的方法
                ServerSendMsg(txtSendMsg.Text.Trim());
            }

        }

        private void Label3_Click(object sender, EventArgs e)
        {

        }

        private void BtnCheckCOM_Click(object sender, EventArgs e)
        {
            bool comExistence = false;   //有可用串口标志位
            cbxCOMPort.Items.Clear();   //清除当前串口号中的所有串口名称
            for (int i = 0; i < 10; i++) {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Items.Add("COM" + (i + 1).ToString());  //Items方法往下拉里面添加的列数
                    comExistence = true;
                }
                catch {

                    continue;
                }

            }
            if (comExistence)
            {

                cbxCOMPort.SelectedIndex = 0;    //使ListBox显示第一个添加的索引，索引值从0开始
            }
            else {

                MessageBox.Show("没有找到可用串口!*.*错误提示");
            }
        }
        private bool CheckPortSetting()   //检查串口是否设置
        {
            if (cbxCOMPort.Text.Trim() == "") return false;
            if (cbxBaudBate.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            if (cbxParity.Text.Trim() == "") return false;
            if (cbxStopBits.Text.Trim() == "") return false;
            return true;

        }

        private bool CheckSendData() {
            if (tbxSendData.Text.Trim() == "") return false;
            return true;

        }
        private void SetPortProperty() {     //设置串口属性

            sp = new SerialPort();
            sp.PortName = cbxCOMPort.Text.Trim(); //设置串口名
            sp.BaudRate = Convert.ToInt32(cbxBaudBate.Text.Trim());   //设置串口的波特率

            float f = Convert.ToSingle(cbxStopBits.Text.Trim());   //设置停止位

            if (f == 0)
            {

                sp.StopBits = StopBits.None;
            }
            else if (f == 1.5)
            {

                sp.StopBits = StopBits.OnePointFive;

            }
            else if (f == 2)
            {
                sp.StopBits = StopBits.Two;
            }
            else {

                sp.StopBits = StopBits.One;
            }
            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim());   //设置数据位
            string s = cbxParity.Text.Trim();   //设置奇偶校验位
            if (s.CompareTo("无") == 0)
            {

                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {

                sp.Parity = Parity.Odd;    //奇校验

            }
            else if (s.CompareTo("偶校验") == 0)
            {

                sp.Parity = Parity.Even;   //偶校验
            }
            else {

                sp.Parity = Parity.None;

            }
            sp.ReadTimeout = -1;  //设置超时读取时间
            sp.RtsEnable = true;

            //定义DataReceived事件,当串口收到数据后触发事件
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            if (rbnHex.Checked)
            {

                isHex = true;
            }
            else {
                isHex = false;
            }
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);   //延时100ms等待接收完数据
            //this.Invoke夸线程访问UI的方法
            this.Invoke((EventHandler)delegate 
            
            {
                if (isHex == false)
                {

                    tbxRecvData.Text = sp.ReadExisting();     //如果是ASCII码
                }
                else {
                    Byte[] ReceivedData = new byte[sp.BytesToRead];   //创建接收字节数组
                    sp.Read(ReceivedData, 0, ReceivedData.Length);   //读取所接收到的数据
                    string RecvDataText = null;
                    for (int i = 0; i < ReceivedData.Length - 1; i++)

                    {
                        RecvDataText += ("0x" + ReceivedData[i].ToString("X2") + "");  //Hex显示


                    }

                 tbxRecvData.Text += RecvDataText;   //把上一次的数据内容和下一次发送的数据内容进行拼接。
                }
                sp.DiscardInBuffer();   //丢弃接收缓冲区数据
                //SerialPort.DiscardOutBuffer方法清除串行驱动程序发送缓冲区的数据;
                //SerialPort.DiscardInBuffer方法清除串行驱动程序的接收缓冲区的数据;


            });
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            


            if (isOpen)
            {

                try
                {

                    sp.WriteLine(tbxSendData.Text);

                }
                catch
                {

                    MessageBox.Show("发送数据时发生错误!*.*错误提示!");
                    return;

                }
            }
            else {
                MessageBox.Show("串口未打开!*.*错误提示!");
                return;
            }
            if (!CheckSendData())     //检测要发送的数据
            {
                MessageBox.Show("请输入要发送的数据!*.*错误提示");
                return;
            }
        }

        private void BtnOpenCom_Click(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                if (!CheckPortSetting())     //检测串口设置
                {
                    MessageBox.Show("串口未设置!*.*错误提示!");
                }
                if (!isSetProperty)   //串口未设置则设置串口
                {
                    SetPortProperty();
                    isSetProperty = true;

                }
                try      //打开串口
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCom.Text = "关闭串口";

                    //串口打开后则相关的串口设置按钮不可用
                    cbxCOMPort.Enabled = false;
                    cbxBaudBate.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParity.Enabled = false;
                    cbxStopBits.Enabled = false;
                    rbnChar.Enabled = false;
                    rbnHex.Enabled = false;


                }
                catch
                {

                    //打开串口失败后，相应标志位取消
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或者已被占用!*.*错误提示!");
                }
            }
            else {
                try

                {   //打开串口
                    sp.Close();
                    isOpen = false;
                    isSetProperty = false;
                    btnOpenCom.Text = "打开串口";

                    //关闭串口后，串口设置选项便可以继续使用
                    cbxCOMPort.Enabled = true;
                    cbxBaudBate.Enabled = true;
                    cbxDataBits.Enabled = true;
                    cbxParity.Enabled = true;
                    cbxStopBits.Enabled = true;
                    rbnChar.Enabled = true;
                    rbnHex.Enabled = true;



                }
                catch {

                    //libStatus.Text="关闭串口时发生错误";

                }

            }
        }

        private void BtnCleanData_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";
           // tbxSendData.Text = "";
        }

        private void TmSend_Tick(object sender, EventArgs e)
        {
            
            
            try
            {
               
                if (tmSend.Enabled == true)
                {
                    string s1 = txtSecondcount.Text;
                    int count1 = Int32.Parse(s1);
                    int i1 = 1;
                    /* for 循环执行 */
                    while (i1 < count1)
                    {
                       // System.Threading.Thread.Sleep(isecond);
                        btnSendMsg.PerformClick();   //产生"发送事件"的click事件
                       

                        i1++;

                        if (i1 >= count1)
                        {

                            tmSend.Stop();
                        }

                    }

                    if (count1 == 0) {
                       

                            btnSendMsg.PerformClick();

                           // System.Threading.Thread.Sleep(isecond);

                        }

                    }

                  
                 





                }


            
            catch
            {

                MessageBox.Show("错误的定时输入!", "Error");
            }

        }

        private void CbTimeSend_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void TxtSecondcount_TextChanged(object sender, EventArgs e)
        {
           
        }
    }
}
