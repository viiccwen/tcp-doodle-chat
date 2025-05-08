using System;
using System.Text;
using System.Windows.Forms;
using System.Net;         //匯入網路通訊協定相關函數
using System.Net.Sockets; //匯入網路插座功能函數
using System.Threading;   //匯入多執行緒功能函數
using System.Collections;
using WindowsFormsApp1.Managers; //匯入集合物件

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        TcpListener Server;             //伺服端網路監聽器(相當於電話總機)
        Socket Client;                  //給客戶用的連線物件(相當於電話分機)
        Thread Th_Svr;                  //伺服器監聽用執行緒(電話總機開放中)
        Thread Th_Clt;                  //客戶用的通話執行緒(電話分機連線中)
        Hashtable HT = new Hashtable(); //客戶名稱與通訊物件的集合(雜湊表)(key:Name, Socket)
        //顯示本機IP 
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = MyIP();                            //呼叫函數找本機IP
            Text = "四資工二甲_B11215005_温冠華";
        }
        //找出本機IP
        private string MyIP()
        {
            string hn = Dns.GetHostName();
            IPAddress[] ip = Dns.GetHostEntry(hn).AddressList; //取得本機IP陣列
            foreach (IPAddress it in ip)
            {
                if (it.AddressFamily == AddressFamily.InterNetwork)
                {
                    return it.ToString(); //如果是IPv4回傳此IP字串
                }
            }
            return "";                    //找不到合格IP回傳空字串
        }

        //開啟 Server：用 Server Thread 來監聽 Client 
        private void button1_Click(object sender, EventArgs e)
        {
            //忽略跨執行緒處理的錯誤(允許跨執行緒存取變數)
            CheckForIllegalCrossThreadCalls = false;
            Th_Svr = new Thread(ServerSub);     //宣告監聽執行緒(副程式ServerSub)
            Th_Svr.IsBackground = true;         //設定為背景執行緒
            Th_Svr.Start();                     //啟動監聽執行緒
            button1.Enabled = false;            //讓按鍵無法使用(不能重複啟動伺服器) 
        }
        
        //接受客戶連線要求的程式(如同電話總機)，針對每一客戶會建立一個連線，以及獨立執行緒
        private void ServerSub()
        {
            //Server IP 和 Port
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text));
            Server = new TcpListener(EP);       //建立伺服端監聽器(總機)
            Server.Start(100);                  //啟動監聽設定允許最多連線數100人
            while (true)                        //無限迴圈監聽連線要求
            {
                Client = Server.AcceptSocket(); //建立此客戶的連線物件Client
                Th_Clt = new Thread(Listen);    //建立監聽這個客戶連線的獨立執行緒
                Th_Clt.IsBackground = true;     //設定為背景執行緒
                Th_Clt.Start();                 //開始執行緒的運作
            }
        }
        //監聽客戶訊息的程式
        private void Listen()
        {
            Socket Sck = Client; //複製Client通訊物件到個別客戶專用物件Sck
            Thread Th = Th_Clt;  //複製執行緒Th_Clt到區域變數Th
            while (true)         //持續監聽客戶傳來的訊息
            {
                try                //用 Sck 來接收此客戶訊息，inLen 是接收訊息的 byte 數目
                {
                    byte[] B = new byte[1023];   //建立接收資料用的陣列，長度須大於可能的訊息
                    int inLen = Sck.Receive(B);  //接收網路資訊(byte陣列)
                    string Msg = Encoding.Default.GetString(B, 0, inLen); //翻譯實際訊息(長度inLen)

                    // TODO
                    Console.WriteLine($"Received: {Msg}");

                    DrawAction action = DrawAction.Deserialize(Msg);
                    if (action.Tool == Tool.ConnectServer)
                    {
                        HT.Add(action.UserName, Sck); //連線加入雜湊表，Key:使用者，Value:連線物件(Socket)
                        listBox1.Items.Add(action.UserName); //加入上線者名單

                        DrawAction response = new DrawAction
                        {
                            Tool = Tool.UpdateUserList,
                            UserName = OnlineList(),
                        };
                        string data = response.Serialize();
                        SendAll(data);   //將目前上線人名單回傳剛剛登入的人(包含他自己) 
                    }
                    else if (action.Tool == Tool.DisconnectServer)
                    {
                        HT.Remove(action.UserName);             //移除使用者名稱為Name的連線物件
                        listBox1.Items.Remove(action.UserName); //自上線者名單移除Name

                        DrawAction response = new DrawAction
                        {
                            Tool = Tool.UpdateUserList,
                            UserName = OnlineList()
                        };
                        string data = response.Serialize();
                        SendAll(data);              //將目前上線人名單回傳剛剛登入的人(不包含他自己) 
                        Th.Abort();                 //結束此客戶的監聽執行緒
                    }
                    else if (action.Tool == Tool.SendMessage)
                    {
                        DrawAction response = new DrawAction
                        {
                            Tool = Tool.SendMessage,
                            UserName = action.UserName,
                            Message = action.Message
                        };
                        string data = response.Serialize();
                        SendAll(data);               //廣播訊息
                    }
                    else if (action.Tool == Tool.SendPrivateMessage)
                    {
                        DrawAction response = new DrawAction
                        {
                            Tool = Tool.SendPrivateMessage,
                            UserName = action.UserName,
                            Message = action.Message,
                            TargetUser = action.TargetUser
                        };
                        string data = response.Serialize();
                        SendTo(data, action.TargetUser);   //C[0]是訊息，C[1]是收件者
                    }
                    else
                    {
                        //處理繪圖的動作
                        DrawAction response = new DrawAction
                        {
                            Tool = action.Tool,
                            Start = action.Start,
                            End = action.End,
                            Color = action.Color,
                            PenSize = action.PenSize,
                            UserName = action.UserName
                        };
                        if (action.Tool == Tool.PrivatePencil ||
                            action.Tool == Tool.PrivateEraser)
                        {
                            response.TargetUser = action.TargetUser; //私密繪圖的對象
                        }

                            string data = response.Serialize();
                        
                        if (action.Tool == Tool.PrivatePencil ||
                            action.Tool == Tool.PrivateEraser)
                        {
                            
                            SendTo(data, action.TargetUser);   //C[0]是訊息，C[1]是收件者
                        }
                        else
                        {
                            SendAll(data);               //廣播訊息
                        }
                    }
                }
                catch (Exception)
                {
                    //有錯誤時忽略，通常是客戶端無預警強制關閉程式，測試階段常發生
                }
            }
        }
        //建立線上名單
        private string OnlineList()
        {
            string UserLists = "";
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                UserLists += listBox1.Items[i]; //逐一將成員名單加入L字串
                //不是最後一個成員要加上","區隔
                if (i < listBox1.Items.Count - 1)
                {
                    UserLists += ",";
                }
            }
            return UserLists;
        }
        //傳送訊息給指定的客戶
        private void SendTo(string Str, string User)
        {
            // TODO
            Console.WriteLine($"Send to {User}: {Str}");

            byte[] B = Encoding.Default.GetBytes(Str);  //訊息轉譯為byte陣列
            Socket Sck = (Socket)HT[User];              //取出發送對象User的通訊物件
            Sck.Send(B, 0, B.Length, SocketFlags.None); //發送訊息
        }
        //傳送訊息給所有的線上客戶
        private void SendAll(string Str)
        {
            // TODO
            Console.WriteLine($"Send: {Str}");

            byte[] B = Encoding.Default.GetBytes(Str);   //訊息轉譯為Byte陣列
            foreach (Socket s in HT.Values)              //HT雜湊表內所有的Socket
                s.Send(B, 0, B.Length, SocketFlags.None);//傳送資料
        }
        //關閉視窗時 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread(); //關閉所有執行緒 
        }
    }
}
