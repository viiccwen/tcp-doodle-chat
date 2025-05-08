using System;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace WindowsFormsApp1.Managers
{
    class NetworkManager
    {
        Socket T;    //通訊物件

        public event Action<string> OnStatusChanged;
        public event Action<DrawAction> OnDrawActionReceived;
        public event Action<string, string> OnPublicMessageReceived;
        public event Action<string, string> OnPrivateMessageReceived;
        public event Action<string[]> OnUserListUpdated;
        public event Action OnDisconnected;

        public void Connect(string ip, int port, string user)
        {
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(ip), port); //伺服器的連線端點資訊
            //建立可以雙向通訊的TCP連線
            T = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);                                    //使用者名稱 
            try
            {
                T.Connect(EP);           //連上伺服器的端點EP(類似撥號給電話總機)
                Task.Run(() => Listen());  //建立監聽執行緒
                OnStatusChanged?.Invoke("已連線伺服器！" + "\r\n");

                // 傳送使用者名稱給伺服器
                DrawAction action = new DrawAction
                {
                    Tool = Tool.ConnectServer,
                    UserName = user
                };

                Send(action);
                
            }
            catch (Exception)
            {
                OnStatusChanged?.Invoke("無法連上伺服器！" + "\r\n"); //連線失敗時顯示訊息
                throw new Exception();
            }
        }

        private async Task Listen()
        {
            EndPoint serverEP = (EndPoint)T.RemoteEndPoint;
            byte[] buffer = new byte[1023];

            try
            {
                while (true)
                {
                    var result = await T.ReceiveFromAsync(new ArraySegment<byte>(buffer), SocketFlags.None, serverEP);
                    int inLen = result.ReceivedBytes;
                    EndPoint remoteEP = result.RemoteEndPoint;

                    if (inLen == 0)
                    {
                        // 伺服器斷線
                        OnStatusChanged?.Invoke("伺服器斷線了！");
                        OnDisconnected?.Invoke();
                        return;
                    }

                    string msg = Encoding.Default.GetString(buffer, 0, inLen);
                    DrawAction command = DrawAction.Deserialize(msg);
                    Tool commandType = command.Tool;

                    // 判斷命令類型
                    switch (commandType)
                    {
                        case Tool.SendMessage:
                            OnPublicMessageReceived?.Invoke(command.UserName, command.Message);
                            break;
                        case Tool.SendPrivateMessage:
                            OnPrivateMessageReceived?.Invoke(command.UserName, command.Message);
                            break;
                        case Tool.UpdateUserList:
                            string[] userList = command.UserName.Split(',');
                            OnUserListUpdated?.Invoke(userList);
                            break;

                        // Drawing commands
                        default:
                            if (commandType != Tool.ConnectServer && commandType != Tool.DisconnectServer)
                            {
                                OnDrawActionReceived?.Invoke(command);
                            }
                            break;
                    }
                }
            }
            catch (SocketException)
            {
                // 連線中斷
                OnStatusChanged?.Invoke("伺服器斷線了！");
                OnDisconnected?.Invoke();
            }
            catch (ObjectDisposedException)
            {
                // socket 已經被關閉（正常關閉時會發生）
                OnStatusChanged?.Invoke("連線已關閉！");
            }
        }

        public void Send(DrawAction action)
        {
            string data = action.Serialize();

            byte[] B = Encoding.Default.GetBytes(data);//翻譯字串Str為Byte陣列B
            T.Send(B, 0, B.Length, SocketFlags.None); //使用連線物件傳送資料
        }

        public void Disconnect()
        {
            if (T != null)
            {
                T.Close(); //關閉通訊器
                OnStatusChanged?.Invoke("已斷線伺服器！" + "\r\n");
            }
        }
    }

    

    [Serializable]
    public class DrawAction
    {
        public Tool Tool { get; set; }
        public Point Start { get; set; }
        public Point End { get; set; }
        public Color Color { get; set; }
        public int PenSize { get; set; }
        public string UserName { get; set; }
        public string TargetUser { get; set; } // for private message
        public string Message { get; set; } // for send message (broadcast)

        public string Serialize()
        {
            // for connect/disconnect server
            if (Tool == Tool.ConnectServer || Tool == Tool.DisconnectServer)
                return $"{(int)Tool}|{UserName}";

            // for send message (broadcast)
            if (Tool == Tool.SendMessage)
                return $"{(int)Tool}|{UserName}|{Message}";

            // for private message
            if (Tool == Tool.SendPrivateMessage)
                return $"{(int)Tool}|{UserName}|{Message}|{TargetUser}";

            // for private draw
            if (Tool == Tool.PrivatePencil || Tool == Tool.PrivateEraser)
                return $"{(int)Tool}|{TargetUser}|{Start.X},{Start.Y}|{End.X},{End.Y}|{Color.ToArgb()}|{PenSize}";

            // for public draw
            return $"{(int)Tool}|{Start.X},{Start.Y}|{End.X},{End.Y}|{Color.ToArgb()}|{PenSize}";
        }

        public static DrawAction Deserialize(string data)
        {
            string[] parts = data.Split('|');
            Tool ToolType = (Tool)int.Parse(parts[0]);

            if (ToolType == Tool.SendMessage || ToolType == Tool.SendPrivateMessage)
            {
                return new DrawAction
                {
                    Tool = ToolType,
                    UserName = parts[1],
                    Message = parts[2],
                    TargetUser = parts.Length > 3 ? parts[3] : null,
                };

            }
            else if (ToolType == Tool.UpdateUserList || ToolType == Tool.DisconnectServer)
            {
                return new DrawAction
                {
                    Tool = ToolType,
                    UserName = parts[1]
                };
            }
            else
            {
                if (ToolType == Tool.PrivatePencil || ToolType == Tool.PrivateEraser)
                {
                    return new DrawAction
                    {
                        Tool = ToolType,
                        TargetUser = parts[1],
                        Start = ParsePoint(parts[2]),
                        End = ParsePoint(parts[3]),
                        Color = Color.FromArgb(int.Parse(parts[4])),
                        PenSize = int.Parse(parts[5])
                    };
                }
                else
                {
                    return new DrawAction
                    {
                        Tool = ToolType,
                        Start = ParsePoint(parts[1]),
                        End = ParsePoint(parts[2]),
                        Color = Color.FromArgb(int.Parse(parts[3])),
                        PenSize = int.Parse(parts[4])
                    };
                }
            }
        }

        private static Point ParsePoint(string data)
        {
            string[] coords = data.Split(',');
            return new Point(int.Parse(coords[0]), int.Parse(coords[1]));
        }
    }
}
