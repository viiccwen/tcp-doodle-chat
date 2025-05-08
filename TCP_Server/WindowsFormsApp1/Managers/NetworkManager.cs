using System;
using System.Drawing;

namespace WindowsFormsApp1.Managers
{
    class NetworkManager
    {
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

            // for updating userlists
            if (Tool == Tool.UpdateUserList)
                return $"{(int)Tool}|{UserName}"; // UserName contains the list of online users

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
            else if (ToolType == Tool.UpdateUserList || ToolType == Tool.ConnectServer || ToolType == Tool.DisconnectServer)
            {
                return new DrawAction
                {
                    Tool = ToolType,
                    UserName = parts[1]
                };
            }
            else if(ToolType == Tool.PrivatePencil || ToolType == Tool.PrivateEraser)
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

        private static Point ParsePoint(string data)
        {
            string[] coords = data.Split(',');
            return new Point(int.Parse(coords[0]), int.Parse(coords[1]));
        }
    }
}
