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
namespace Client
{

    public partial class Client : Form

    {


        int time;
        SerialPort sp = null;   //声明一个串口类
        bool isOpen = false;   //打开串口标志位
        bool isSetProperty = false;  //属性设置标志位
        bool isHex = false;   //十六进制显示位标志
        public Client()
        {
            InitializeComponent();
            //关闭对文本框的非法线程操作检查
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }
        //创建 1个客户端套接字 和1个负责监听服务端请求的线程  
        Socket socketClient = null;
        Thread threadClient = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            for (int i = 0; i < 10; i++)
            {
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

        private void BtnBeginListen_Click(object sender, EventArgs e)
        {
            //定义一个套字节监听  包含3个参数(IP4寻址协议,流式连接,TCP协议)
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //需要获取文本框中的IP地址
            IPAddress ipaddress = IPAddress.Parse(txtIP.Text.Trim());
            //将获取的ip地址和端口号绑定到网络节点endpoint上
            IPEndPoint endpoint = new IPEndPoint(ipaddress, int.Parse(txtPort.Text.Trim()));
            //这里客户端套接字连接到网络节点(服务端)用的方法是Connect 而不是Bind
            socketClient.Connect(endpoint);
            //创建一个线程 用于监听服务端发来的消息
            threadClient = new Thread(RecMsg);
            //将窗体线程设置为与后台同步
            threadClient.IsBackground = true;
            //启动线程
            threadClient.Start();
        }

        //接收服务端发来信息的方法
        private void RecMsg()
        {
            while (true) //持续监听服务端发来的消息
            {
                //定义一个1M的内存缓冲区 用于临时性存储接收到的信息
                byte[] arrRecMsg = new byte[1024 * 1024];
                //将客户端套接字接收到的数据存入内存缓冲区, 并获取其长度
                int length = socketClient.Receive(arrRecMsg);
                //将套接字获取到的字节数组转换为人可以看懂的字符串
                string strRecMsg = Encoding.UTF8.GetString(arrRecMsg, 0, length);
                //将发送的信息追加到聊天内容文本框中
                txtMsg.AppendText("服务器:" + strRecMsg + "\r\n");
                //将发送的信息追加到串口数据发送文本中
                tbxSendData.AppendText(strRecMsg + "\r\n");
                btnSendNew.PerformClick();
            }
        }

        //发送字符串信息到服务器端的方法
        private void ClientSendMsg(string sendMsg)
        {
            //将输入的内容字符串转换为机器可以识别的字节数组
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组
            socketClient.Send(arrClientSendMsg);
            //将发送的信息追加到聊天内容文本框中
            txtMsg.AppendText("客户端:"  + sendMsg + "\r\n");

        }

        //获取当前系统时间的方法
     


        //点击按钮btnSend向服务器发送信息
        private void BtnSend_Click(object sender, EventArgs e)
        {
            string str = txtSecond.Text;   //将下拉框内容添加到一个变量中
           // MessageBox.Show(txtSecond.Text);
         //   MessageBox.Show(txtSecond.SelectedText);
            time = int.Parse(str); ;   //得到设定定时值(整型)
            System.Threading.Thread.Sleep(time);

            //调用ClientSendMsg方法,将文本框中输入的信息发送给服务器
            ClientSendMsg(txtCMsg.Text.Trim());
            if (cbTimeSend.Checked)
            {


                tmSend.Enabled = true;

               
            }
            else
            {

                tmSend.Enabled = false;
            }

        }


        //快捷键Enter发送信息

        private void txtCMsg_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {

                //则调用客户端向服务器发送信息的方法
                ClientSendMsg(txtCMsg.Text.Trim());
            }
        }

        private void BtnCheckCOM_Click(object sender, EventArgs e)
        {
            bool comExistence = false;  //有可用串口标志位
            cbxCOMPort.Items.Clear(); //清除当前串口号中所有串口名称
            for (int i = 0; i < 10; i++)
            {

                try
                {

                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Items.Add("COM" + (i + 1).ToString());  //Items()方法下拉框里面添加数列
                    comExistence = true;
                }
                catch
                {

                    continue;
                }

            }
            if (comExistence)
            {
                cbxCOMPort.SelectedIndex = 0;   //使ListBox显示第一个添加的索引,索引值从0开始
            }
            else
            {
                MessageBox.Show("没有找到可用串口!*.*错误提示");
            }
        }

        private bool CheckPortSetting()  //检查串口是否设置
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
        private void SetPortProperty() {  //设置串口属性

            sp = new SerialPort();
            sp.PortName = cbxCOMPort.Text.Trim();   //设置串口名
            sp.BaudRate = Convert.ToInt32(cbxBaudBate.Text.Trim());  //设置串口的波特率

            float f = Convert.ToSingle(cbxStopBits.Text.Trim());  //设置停止位

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
                sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim()); //设置数据位
                string s = cbxParity.Text.Trim();   //设置奇偶校验
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
                sp.ReadTimeout = -1;   //设置超时读取时间
                sp.RtsEnable = true;

                //定义DataReceived事件，当串口收到数据后触发事件
                sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
                if (rbnHex.Checked)
                {
                    isHex = true;
                }
                else {

                    isHex = false;
                }
            }

        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);    //延时100ms等待接收完数据
            //this.Invoke夸线程访问UI的方法
            this.Invoke((EventHandler)delegate
            {
                if (isHex == false)
                {
                    tbxRecvData.Text = sp.ReadExisting();   //如果是ASCII码
                }
                else {

                    Byte[] ReceivedData = new byte[sp.BytesToRead];  //创建接收字节数组
                    sp.Read(ReceivedData, 0, ReceivedData.Length);    //读取所接收到的数据
                    string RecvDataText = null;
                    for (int i = 0; i < ReceivedData.Length - 1; i++) {
                        RecvDataText += ("0x" + ReceivedData[i].ToString("X2") + "");  //Hex显示

                    }
                    tbxRecvData.Text += RecvDataText;   //把上一次的数据内容和下一次发送的数据内容进行拼接


                }
                sp.DiscardInBuffer();   //丢弃接收缓冲区数据
                //SerialPort.DiscardOutBuffer方法清除串行驱动程序发送缓冲区的数据
                //SerialPort.DiscardInBuffer方法清除串行驱动程序接收缓冲区的数据
            });
        }

        private void BtnSendNew_Click(object sender, EventArgs e)
        {
            

            if (isOpen)
            {    //写入串口数据
                try
                {

                    sp.WriteLine(tbxSendData.Text);
                }
                catch
                {

                    MessageBox.Show("发送数据时发生错误!*.*错误提示");
                    return;
                }
            }
            else {

                MessageBox.Show("串口未打开!*.*错误提示!");
                return;
            }
            if (!CheckSendData()) {       //检测要发送的数据

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
                    MessageBox.Show("串口未设置!*.*错误提示");
                    return;

                }
                if (!isSetProperty)       //串口未设置则设置串口
                {
                    SetPortProperty();
                    isSetProperty = true;

                }
                try
                {      //打开串口
                    sp.Open();
                    isOpen = true;
                    btnOpenCom.Text = "关闭串口";

                    //串口打开后则相关的串口设置按钮不可再用

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
                    MessageBox.Show("串口无效或已被占用!*.*错误提示");


                }


            }
            else {
                try
                {      //打开串口
                    sp.Close();
                    isOpen = false;
                    isSetProperty = false;
                    btnOpenCom.Text = "打开串口";

                    //关闭串口后，串口设置选项可以继续使用
                    cbxCOMPort.Enabled = true;
                    cbxBaudBate.Enabled = true;
                    cbxDataBits.Enabled = true;
                    cbxParity.Enabled = true;
                    cbxStopBits.Enabled = true;
                    rbnChar.Enabled = true;
                    rbnHex.Enabled = true;

                }
                catch {

                    //lblStatus.Text="关闭串口时发生错误";

                }

            }
        }

        private void BtnCleanData_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";
           // tbxSendData.Text = "";
        }

       

        private void TbxRecvData_TextChanged(object sender, EventArgs e)
        {

        }

        private void CbTimeSend_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void TxtSecond_TextChanged(object sender, EventArgs e)
        {

        }

        private void TmSend_Tick(object sender, EventArgs e)
        {
            //转换时间间隔
           
            try
            {
                
                if (tmSend.Enabled == true)
                {
                    string s2 = txtSecondcount.Text;
                    int count2 = Int32.Parse(s2);
                    int i2=1;
                    /* for 循环执行 */
                    while (i2 < count2) {

                      
                        
                        btnSend.PerformClick();   //产生"发送事件"的click事件
                      
                        i2++;
                      
                       
                        if (i2 >= count2)
                        {

                            tmSend.Stop();
                        }
                    
                    }



                    if (count2 == 0) {

                       
                            btnSend.PerformClick();
                        

                       
                    
                    }
                    
                   

                   
                    
                }


            }
            catch
            {

                MessageBox.Show("错误的定时输入!", "Error");
            }

        }

        private void TxtSecondcount_TextChanged(object sender, EventArgs e)
        {
          
        }
    }
    }
