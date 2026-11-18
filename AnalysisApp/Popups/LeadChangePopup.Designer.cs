namespace AnalysisApp
{
    partial class LeadChangePopup
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLimb = new System.Windows.Forms.Panel();
            this.limbLeadContainer = new System.Windows.Forms.Panel();
            this.lblLimbFoot = new Zoncare.Component.Controls.RoundLabel();
            this.lblLimbLeft = new Zoncare.Component.Controls.RoundLabel();
            this.lblLimbRight = new Zoncare.Component.Controls.RoundLabel();
            this.picChangeRF = new System.Windows.Forms.PictureBox();
            this.picChangeLF = new System.Windows.Forms.PictureBox();
            this.picChangeRL = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnReset = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnSave = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlLimb.SuspendLayout();
            this.limbLeadContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lblLimbFoot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblLimbLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblLimbRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChangeRF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChangeLF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChangeRL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.21066F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.pnlLimb, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(472, 543);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // pnlLimb
            // 
            this.pnlLimb.Controls.Add(this.limbLeadContainer);
            this.pnlLimb.Controls.Add(this.pictureBox1);
            this.pnlLimb.Controls.Add(this.label3);
            this.pnlLimb.Controls.Add(this.label2);
            this.pnlLimb.Controls.Add(this.label1);
            this.pnlLimb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLimb.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.pnlLimb.Location = new System.Drawing.Point(3, 3);
            this.pnlLimb.Name = "pnlLimb";
            this.pnlLimb.Size = new System.Drawing.Size(466, 537);
            this.pnlLimb.TabIndex = 0;
            // 
            // limbLeadContainer
            // 
            this.limbLeadContainer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.limbLeadContainer.Controls.Add(this.lblLimbFoot);
            this.limbLeadContainer.Controls.Add(this.lblLimbLeft);
            this.limbLeadContainer.Controls.Add(this.lblLimbRight);
            this.limbLeadContainer.Controls.Add(this.picChangeRF);
            this.limbLeadContainer.Controls.Add(this.picChangeLF);
            this.limbLeadContainer.Controls.Add(this.picChangeRL);
            this.limbLeadContainer.Location = new System.Drawing.Point(104, 48);
            this.limbLeadContainer.Name = "limbLeadContainer";
            this.limbLeadContainer.Size = new System.Drawing.Size(260, 480);
            this.limbLeadContainer.TabIndex = 4;
            // 
            // lblLimbFoot
            // 
            this.lblLimbFoot.BackColor = System.Drawing.Color.Transparent;
            this.lblLimbFoot.CircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.lblLimbFoot.DisplayText = "F";
            this.lblLimbFoot.Location = new System.Drawing.Point(158, 396);
            this.lblLimbFoot.Name = "lblLimbFoot";
            this.lblLimbFoot.Size = new System.Drawing.Size(20, 20);
            this.lblLimbFoot.TabIndex = 9;
            this.lblLimbFoot.TabStop = false;
            this.lblLimbFoot.Visible = false;
            // 
            // lblLimbLeft
            // 
            this.lblLimbLeft.BackColor = System.Drawing.Color.Transparent;
            this.lblLimbLeft.CircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.lblLimbLeft.DisplayText = "L";
            this.lblLimbLeft.Location = new System.Drawing.Point(212, 228);
            this.lblLimbLeft.Name = "lblLimbLeft";
            this.lblLimbLeft.Size = new System.Drawing.Size(20, 20);
            this.lblLimbLeft.TabIndex = 8;
            this.lblLimbLeft.TabStop = false;
            // 
            // lblLimbRight
            // 
            this.lblLimbRight.BackColor = System.Drawing.Color.Transparent;
            this.lblLimbRight.CircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.lblLimbRight.DisplayText = "R";
            this.lblLimbRight.Location = new System.Drawing.Point(28, 228);
            this.lblLimbRight.Name = "lblLimbRight";
            this.lblLimbRight.Size = new System.Drawing.Size(20, 20);
            this.lblLimbRight.TabIndex = 7;
            this.lblLimbRight.TabStop = false;
            // 
            // picChangeRF
            // 
            this.picChangeRF.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picChangeRF.Image = global::AnalysisApp.Properties.Resources.right_foot_change;
            this.picChangeRF.Location = new System.Drawing.Point(49, 306);
            this.picChangeRF.Margin = new System.Windows.Forms.Padding(0);
            this.picChangeRF.Name = "picChangeRF";
            this.picChangeRF.Size = new System.Drawing.Size(32, 32);
            this.picChangeRF.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picChangeRF.TabIndex = 6;
            this.picChangeRF.TabStop = false;
            this.picChangeRF.Visible = false;
            // 
            // picChangeLF
            // 
            this.picChangeLF.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picChangeLF.Image = global::AnalysisApp.Properties.Resources.left_foot_change;
            this.picChangeLF.Location = new System.Drawing.Point(181, 306);
            this.picChangeLF.Margin = new System.Windows.Forms.Padding(0);
            this.picChangeLF.Name = "picChangeLF";
            this.picChangeLF.Size = new System.Drawing.Size(32, 32);
            this.picChangeLF.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picChangeLF.TabIndex = 5;
            this.picChangeLF.TabStop = false;
            this.picChangeLF.Visible = false;
            // 
            // picChangeRL
            // 
            this.picChangeRL.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picChangeRL.Image = global::AnalysisApp.Properties.Resources.right_left_change;
            this.picChangeRL.Location = new System.Drawing.Point(112, 222);
            this.picChangeRL.Margin = new System.Windows.Forms.Padding(0);
            this.picChangeRL.Name = "picChangeRL";
            this.picChangeRL.Size = new System.Drawing.Size(32, 32);
            this.picChangeRL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picChangeRL.TabIndex = 4;
            this.picChangeRL.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AnalysisApp.Properties.Resources.right_left_change;
            this.pictureBox1.Location = new System.Drawing.Point(38, 24);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(58, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "电极交换";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "点击";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "肢导纠错";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnCancel, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnReset, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSave, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 543);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(472, 42);
            this.tableLayoutPanel2.TabIndex = 19;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCancel.DataFlag = null;
            this.btnCancel.FollowBaseColor = false;
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(132)))), ((int)(((byte)(207)))));
            this.btnCancel.Location = new System.Drawing.Point(285, 7);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCancel.Size = new System.Drawing.Size(75, 28);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnReset.DataFlag = null;
            this.btnReset.FollowBaseColor = false;
            this.btnReset.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnReset.IsHighlight = true;
            this.btnReset.Location = new System.Drawing.Point(111, 7);
            this.btnReset.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.btnReset.Name = "btnReset";
            this.btnReset.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnReset.Size = new System.Drawing.Size(75, 28);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "重置";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnSave.DataFlag = null;
            this.btnSave.FollowBaseColor = false;
            this.btnSave.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSave.IsHighlight = true;
            this.btnSave.Location = new System.Drawing.Point(198, 7);
            this.btnSave.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSave.Size = new System.Drawing.Size(75, 28);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "确定";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // LeadChangePopup
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(472, 585);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "LeadChangePopup";
            this.ShowInTaskbar = false;
            this.Text = "导联纠错";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LeadChangePopup_FormClosing);
            this.Load += new System.EventHandler(this.AddSTEventForm_Load);
            this.Controls.SetChildIndex(this.tableLayoutPanel2, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlLimb.ResumeLayout(false);
            this.pnlLimb.PerformLayout();
            this.limbLeadContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lblLimbFoot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblLimbLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblLimbRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChangeRF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChangeLF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChangeRL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pnlLimb;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private Zoncare.Component.Controls.RoundButton btnCancel;
        private Zoncare.Component.Controls.RoundButton btnSave;
        private System.Windows.Forms.Panel limbLeadContainer;
        private Zoncare.Component.Controls.RoundButton btnReset;
        private System.Windows.Forms.PictureBox picChangeRF;
        private System.Windows.Forms.PictureBox picChangeLF;
        private System.Windows.Forms.PictureBox picChangeRL;
        private Zoncare.Component.Controls.RoundLabel lblLimbRight;
        private Zoncare.Component.Controls.RoundLabel lblLimbFoot;
        private Zoncare.Component.Controls.RoundLabel lblLimbLeft;
    }
}