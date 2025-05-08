using System;
using System.Windows.Forms;

public partial class PenSizeForm : Form
{
    private Button BtnOK;
    private Label LabelPenSize;
    private TrackBar TrackBarPenSize;

    public int PenSize { get; private set; }

    public PenSizeForm(int currentPenSize)
    {
        InitializeComponent();

        // initialize trackbar
        TrackBarPenSize.Minimum = 1;
        TrackBarPenSize.Maximum = 20;
        TrackBarPenSize.Value = currentPenSize;
        LabelPenSize.Text = "筆刷尺寸: " + currentPenSize.ToString();
    }

    private void TrackBarPenSize_Scroll(object sender, EventArgs e)
    {
        PenSize = TrackBarPenSize.Value;
        LabelPenSize.Text = "筆刷尺寸: " + TrackBarPenSize.Value.ToString();
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
        PenSize = TrackBarPenSize.Value;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void InitializeComponent()
    {
            this.TrackBarPenSize = new System.Windows.Forms.TrackBar();
            this.BtnOK = new System.Windows.Forms.Button();
            this.LabelPenSize = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBarPenSize)).BeginInit();
            this.SuspendLayout();
            // 
            // TrackBarPenSize
            // 
            this.TrackBarPenSize.Location = new System.Drawing.Point(85, 129);
            this.TrackBarPenSize.Name = "TrackBarPenSize";
            this.TrackBarPenSize.Size = new System.Drawing.Size(614, 114);
            this.TrackBarPenSize.TabIndex = 1;
            this.TrackBarPenSize.Scroll += new System.EventHandler(this.TrackBarPenSize_Scroll);
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(766, 85);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(218, 143);
            this.BtnOK.TabIndex = 2;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // LabelPenSize
            // 
            this.LabelPenSize.AutoSize = true;
            this.LabelPenSize.Location = new System.Drawing.Point(80, 237);
            this.LabelPenSize.Name = "LabelPenSize";
            this.LabelPenSize.Size = new System.Drawing.Size(186, 38);
            this.LabelPenSize.TabIndex = 3;
            this.LabelPenSize.Text = "筆刷尺寸: ";
            // 
            // PenSizeForm
            // 
            this.ClientSize = new System.Drawing.Size(1038, 331);
            this.Controls.Add(this.LabelPenSize);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.TrackBarPenSize);
            this.Name = "PenSizeForm";
            this.Text = "Pen Size";
            ((System.ComponentModel.ISupportInitialize)(this.TrackBarPenSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
}
