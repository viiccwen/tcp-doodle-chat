using System;
using System.Windows.Forms;
using System.Drawing;     //匯入圖形相關函數
using WindowsFormsApp1.Managers;  

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly DrawingManager drawingManager;
        private readonly NetworkManager networkManager;
        private DateTime lastSendTime = DateTime.MinValue;
        private readonly string Title = "四資工二甲_B11215005_温冠華";
        private string UserName;

        public Form1()
        {
            InitializeComponent();
            Text = Title;

            // Initialize DrawingManager and NetworkManager
            drawingManager = new DrawingManager(pictureBox1.Width, pictureBox1.Height);
            networkManager = new NetworkManager();
            drawingManager.CurrentTool = Tool.PublicPencil; //預設畫筆為公開畫筆
            pictureBox1.Image = drawingManager.Canvas;

            KeyPreview = true;
            pictureBox1.Enabled = false;
            textBox4.Enabled = false; //訊息框失效
            textBox5.Enabled = false;
            button2.Enabled = false; //發送訊息按鍵失效

            UpdateBtnUndo();
            UpdateBtnRedo();

            // Set up event handlers
            networkManager.OnDrawActionReceived += (DrawAction action) =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    drawingManager.ApplyRemoteDrawAction(action);
                    pictureBox1.Invalidate();
                }));
            };

            networkManager.OnUserListUpdated += (users) =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    listBox1.Items.Clear();
                    listBox1.Items.AddRange(users);
                }));
            };

            networkManager.OnPublicMessageReceived += (user, msg) =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    textBox4.AppendText($"(公開) {user}：{msg}\r\n");
                }));
            };

            networkManager.OnPrivateMessageReceived += (user, msg) =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    textBox4.AppendText($"(私密) {user}：{msg}\r\n");
                }));
            };

            networkManager.OnStatusChanged += (msg) =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    textBox4.AppendText(msg + "\r\n");
                }));
            };

            networkManager.OnDisconnected += () =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    listBox1.Items.Clear();
                    button1.Enabled = true;
                    MessageBox.Show("伺服器斷線了！");
                }));
            };

            drawingManager.OnStackChanged += () =>
            {
                UpdateBtnUndo();
                UpdateBtnRedo();
                pictureBox1.Invalidate();
            };
        }

        #region Button Click Events

        //登入伺服器 
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "") return; //未輸入IP、Port或使用者名稱不傳送資料

            CheckForIllegalCrossThreadCalls = false;                   //忽略跨執行緒錯誤 
            string IP = textBox1.Text;                                 //伺服器IP
            int Port = int.Parse(textBox2.Text);                       //伺服器Port
            UserName = textBox3.Text;                             //使用者名稱

            try
            {
                networkManager.Connect(IP, Port, UserName);           //連上伺服器的端點EP
            }
            catch (Exception)
            {
                textBox4.Text = "無法連上伺服器！" + "\r\n"; //連線失敗時顯示訊息
                return;
            }

            button1.Enabled = false; //讓連線按鍵失效，避免重複連線 
            button2.Enabled = true;  //如連線成功可以開始發送訊息 
            textBox4.Enabled = true; //訊息框啟用
            textBox5.Enabled = true; //發送訊息框啟用
            pictureBox1.Enabled = true; //畫布啟用
        }
        
        //送出訊息 
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == "") return; //未輸入訊息不傳送資料

            // //未選取傳送對象(廣播)，命令碼：1
            if (listBox1.SelectedIndex < 0)  
            {
                DrawAction action = new DrawAction
                {
                    Tool = Tool.SendMessage,
                    Message = textBox5.Text,
                    UserName = UserName,
                };
                networkManager.Send(action);
            }
            //有選取傳送對象(私密訊息)，命令碼：2
            else
            {
                DrawAction action = new DrawAction
                {
                    Tool = Tool.SendPrivateMessage,
                    Message = textBox5.Text,
                    UserName = UserName,
                    TargetUser = listBox1.SelectedItem.ToString(), //選取的使用者名稱
                };
                networkManager.Send(action);
            }

            textBox5.Text = "";             //清除發言框 
        }

        //廣播
        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.ClearSelected(); //清除選取 
        }

        private void BtnPencil_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
                drawingManager.CurrentTool = Tool.PublicPencil;
            else
                drawingManager.CurrentTool = Tool.PrivatePencil;
        }

        private void BtnEraser_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
             drawingManager.CurrentTool = Tool.PublicEraser;
            else
              drawingManager.CurrentTool = Tool.PrivateEraser;
        }

        private void BtnPlatte_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    drawingManager.CurrentColor = dlg.Color;
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            drawingManager.SaveUndo();
            drawingManager.Clear();
            pictureBox1.Invalidate();
            DrawAction action = new DrawAction
            {
                Tool = Tool.Clear,
                UserName = UserName
            };
            networkManager.Send(action);
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            drawingManager.Undo();
        }

        private void BtnRedo_Click(object sender, EventArgs e)
        {
            drawingManager.Redo();
        }

        private void BtnPenSize_Click(object sender, EventArgs e)
        {
            using (PenSizeForm penSizeForm = new PenSizeForm(drawingManager.PenSize))
            {
                if (penSizeForm.ShowDialog() == DialogResult.OK)
                {
                    drawingManager.PenSize = penSizeForm.PenSize;
                }
            }
        }

        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
            {   if(drawingManager.CurrentTool == Tool.PrivateEraser || drawingManager.CurrentTool == Tool.PrivatePencil)
                {
                    drawingManager.CurrentTool = drawingManager.CurrentTool == Tool.PrivateEraser 
                        ? Tool.PublicEraser 
                        : Tool.PublicPencil;
                }
            }
            else
            {
                if (drawingManager.CurrentTool == Tool.PublicEraser || drawingManager.CurrentTool == Tool.PublicPencil)
                {
                    drawingManager.CurrentTool = drawingManager.CurrentTool == Tool.PublicEraser
                        ? Tool.PrivateEraser
                        : Tool.PrivatePencil;
                }
            }

        }

        #endregion

        #region Canvas Event
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            drawingManager.SaveUndo();
            drawingManager.IsDrawing = true;
            drawingManager.PreviousPoint = e.Location;
            drawingManager.ShapeStart = e.Location;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!drawingManager.IsDrawing) return;

            // limit sending draw action to 50ms
            if ((DateTime.Now - lastSendTime).TotalMilliseconds >= 50)
            {
                if (drawingManager.CurrentTool == Tool.PublicPencil ||
                    drawingManager.CurrentTool == Tool.PublicEraser ||
                    drawingManager.CurrentTool == Tool.PrivatePencil ||
                    drawingManager.CurrentTool == Tool.PrivateEraser)
                {
                    Point startPoint = drawingManager.PreviousPoint;
                    Point endPoint = e.Location;

                    drawingManager.DrawPencil(endPoint);
                    pictureBox1.Invalidate();

                    DrawAction action = new DrawAction
                    {
                        Tool = drawingManager.CurrentTool,
                        UserName = UserName,
                        Start = startPoint,
                        End = endPoint,
                        Color = drawingManager.CurrentColor,
                        PenSize = drawingManager.PenSize
                    };

                    action.TargetUser = listBox1.SelectedIndex < 0 ? "" : listBox1.SelectedItem.ToString(); //選取的使用者名稱

                    networkManager.Send(action);

                    drawingManager.PreviousPoint = endPoint;
                    lastSendTime = DateTime.Now;
                }
            }
            else
            {
                drawingManager.ShapeEnd = e.Location;
                pictureBox1.Invalidate();
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!drawingManager.IsDrawing) return;
            drawingManager.IsDrawing = false;
            pictureBox1.Invalidate();
            
        }
        #endregion

        #region UI updates
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (drawingManager != null && drawingManager.Canvas != null)
            {
                e.Graphics.DrawImage(drawingManager.Canvas, 0, 0);
            }
        }
        private void UpdateBtnUndo()
        {
            btnUndo.Enabled = drawingManager.UndoStackCount > 0;
        }

        private void UpdateBtnRedo()
        {
            BtnRedo.Enabled = drawingManager.RedoStackCount > 0;
        }

        #endregion

        //關閉視窗代表離線登出 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (button1.Enabled == false)
            {
                DrawAction action = new DrawAction()
                {
                    Tool = Tool.DisconnectServer,
                    UserName = UserName
                };
                networkManager.Send(action); //傳送離線訊息給伺服器
                networkManager.Disconnect(); //關閉通訊器
            }
        }
    }
}
