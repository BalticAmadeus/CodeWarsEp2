namespace CodeManPoc
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            this.boardPic = new System.Windows.Forms.PictureBox();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.p1TargetRadio = new System.Windows.Forms.RadioButton();
            this.p1RandomRadio = new System.Windows.Forms.RadioButton();
            this.playBtn = new System.Windows.Forms.Button();
            this.turnLbl = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.scoreLbl = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.p2TargetRadio = new System.Windows.Forms.RadioButton();
            this.p2RandomRadio = new System.Windows.Forms.RadioButton();
            label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.boardPic)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // boardPic
            // 
            this.boardPic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.boardPic.Location = new System.Drawing.Point(16, 15);
            this.boardPic.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.boardPic.Name = "boardPic";
            this.boardPic.Size = new System.Drawing.Size(800, 492);
            this.boardPic.TabIndex = 0;
            this.boardPic.TabStop = false;
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 300;
            this.gameTimer.Tick += new System.EventHandler(this.gameTimer_Tick);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.p1TargetRadio);
            this.groupBox1.Controls.Add(this.p1RandomRadio);
            this.groupBox1.Location = new System.Drawing.Point(824, 246);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(180, 124);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Player 1";
            // 
            // p1TargetRadio
            // 
            this.p1TargetRadio.AutoSize = true;
            this.p1TargetRadio.Location = new System.Drawing.Point(24, 52);
            this.p1TargetRadio.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.p1TargetRadio.Name = "p1TargetRadio";
            this.p1TargetRadio.Size = new System.Drawing.Size(123, 21);
            this.p1TargetRadio.TabIndex = 2;
            this.p1TargetRadio.TabStop = true;
            this.p1TargetRadio.Text = "Random target";
            this.p1TargetRadio.UseVisualStyleBackColor = true;
            // 
            // p1RandomRadio
            // 
            this.p1RandomRadio.AutoSize = true;
            this.p1RandomRadio.Checked = true;
            this.p1RandomRadio.Location = new System.Drawing.Point(24, 23);
            this.p1RandomRadio.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.p1RandomRadio.Name = "p1RandomRadio";
            this.p1RandomRadio.Size = new System.Drawing.Size(82, 21);
            this.p1RandomRadio.TabIndex = 0;
            this.p1RandomRadio.TabStop = true;
            this.p1RandomRadio.Text = "Random";
            this.p1RandomRadio.UseVisualStyleBackColor = true;
            // 
            // playBtn
            // 
            this.playBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.playBtn.Location = new System.Drawing.Point(352, 532);
            this.playBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.playBtn.Name = "playBtn";
            this.playBtn.Size = new System.Drawing.Size(149, 57);
            this.playBtn.TabIndex = 8;
            this.playBtn.Text = "START";
            this.playBtn.UseVisualStyleBackColor = true;
            this.playBtn.Click += new System.EventHandler(this.playBtn_Click);
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(839, 30);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(42, 17);
            label1.TabIndex = 9;
            label1.Text = "Turn:";
            // 
            // turnLbl
            // 
            this.turnLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.turnLbl.AutoSize = true;
            this.turnLbl.Location = new System.Drawing.Point(887, 30);
            this.turnLbl.Name = "turnLbl";
            this.turnLbl.Size = new System.Drawing.Size(16, 17);
            this.turnLbl.TabIndex = 10;
            this.turnLbl.Text = "0";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(839, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 17);
            this.label3.TabIndex = 11;
            this.label3.Text = "Score:";
            // 
            // scoreLbl
            // 
            this.scoreLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.scoreLbl.AutoSize = true;
            this.scoreLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scoreLbl.Location = new System.Drawing.Point(894, 58);
            this.scoreLbl.Name = "scoreLbl";
            this.scoreLbl.Size = new System.Drawing.Size(41, 17);
            this.scoreLbl.TabIndex = 12;
            this.scoreLbl.Text = "0 : 0";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.p2TargetRadio);
            this.groupBox2.Controls.Add(this.p2RandomRadio);
            this.groupBox2.Location = new System.Drawing.Point(824, 383);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(180, 124);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Player 2";
            // 
            // p2TargetRadio
            // 
            this.p2TargetRadio.AutoSize = true;
            this.p2TargetRadio.Location = new System.Drawing.Point(24, 52);
            this.p2TargetRadio.Margin = new System.Windows.Forms.Padding(4);
            this.p2TargetRadio.Name = "p2TargetRadio";
            this.p2TargetRadio.Size = new System.Drawing.Size(123, 21);
            this.p2TargetRadio.TabIndex = 2;
            this.p2TargetRadio.TabStop = true;
            this.p2TargetRadio.Text = "Random target";
            this.p2TargetRadio.UseVisualStyleBackColor = true;
            // 
            // p2RandomRadio
            // 
            this.p2RandomRadio.AutoSize = true;
            this.p2RandomRadio.Checked = true;
            this.p2RandomRadio.Location = new System.Drawing.Point(24, 23);
            this.p2RandomRadio.Margin = new System.Windows.Forms.Padding(4);
            this.p2RandomRadio.Name = "p2RandomRadio";
            this.p2RandomRadio.Size = new System.Drawing.Size(82, 21);
            this.p2RandomRadio.TabIndex = 0;
            this.p2RandomRadio.TabStop = true;
            this.p2RandomRadio.Text = "Random";
            this.p2RandomRadio.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 622);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.scoreLbl);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.turnLbl);
            this.Controls.Add(label1);
            this.Controls.Add(this.playBtn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.boardPic);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainForm";
            this.Text = "CodeMap";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.boardPic)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox boardPic;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton p1RandomRadio;
        private System.Windows.Forms.RadioButton p1TargetRadio;
        private System.Windows.Forms.Button playBtn;
        private System.Windows.Forms.Label turnLbl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label scoreLbl;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton p2TargetRadio;
        private System.Windows.Forms.RadioButton p2RandomRadio;
    }
}

