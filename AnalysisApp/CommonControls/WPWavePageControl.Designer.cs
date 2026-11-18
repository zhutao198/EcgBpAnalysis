namespace AnalysisApp.CommonControls
{
    partial class WPWavePageControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cmbLead = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textDuration = new Zoncare.Component.Controls.TextBoxEx();
            this.label3 = new System.Windows.Forms.Label();
            this.textRPos = new Zoncare.Component.Controls.TextBoxEx();
            this.label4 = new System.Windows.Forms.Label();
            this.textLines = new Zoncare.Component.Controls.TextBoxEx();
            this.label5 = new System.Windows.Forms.Label();
            this.enlarge = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.btnApply = new Zoncare.Component.Controls.RoundButton(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1161, 655);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel2.ColumnCount = 12;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.cmbLead, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.textDuration, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.textRPos, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.label4, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.textLines, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this.label5, 8, 0);
            this.tableLayoutPanel2.Controls.Add(this.enlarge, 9, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnApply, 10, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1155, 34);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // cmbLead
            // 
            this.cmbLead.CheckedListBox = null;
            this.cmbLead.Control = null;
            this.cmbLead.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cmbLead.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbLead.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbLead.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLead.FormattingEnabled = true;
            this.cmbLead.IsShown = false;
            this.cmbLead.IsUnresponsive = false;
            this.cmbLead.Location = new System.Drawing.Point(71, 3);
            this.cmbLead.Name = "cmbLead";
            this.cmbLead.PlaceHolderStr = null;
            this.cmbLead.Size = new System.Drawing.Size(74, 27);
            this.cmbLead.TabIndex = 5;
            this.cmbLead.TreeView = null;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "显示通道";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(158, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(170, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "显示带宽(ms)(800~1200)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textDuration
            // 
            this.textDuration.AlphaBackColor = System.Drawing.Color.Empty;
            this.textDuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textDuration.BackColor = System.Drawing.Color.White;
            this.textDuration.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.textDuration.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.textDuration.BorderSize = 1;
            this.textDuration.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textDuration.CornerRadius = 2;
            this.textDuration.DropdownPosition = new System.Drawing.Point(0, 21);
            this.textDuration.FloatAllowed = true;
            this.textDuration.Location = new System.Drawing.Point(331, 4);
            this.textDuration.MaxLength = 32767;
            this.textDuration.MaxValue = 1200D;
            this.textDuration.MinValue = 800D;
            this.textDuration.Multiline = false;
            this.textDuration.Name = "textDuration";
            this.textDuration.NumericOnly = true;
            this.textDuration.PasswordChar = '\0';
            this.textDuration.PlaceHolderStr = "";
            this.textDuration.ReadOnly = false;
            this.textDuration.SelectionStart = 0;
            this.textDuration.Size = new System.Drawing.Size(74, 26);
            this.textDuration.TabIndex = 6;
            this.textDuration.TabStop = false;
            this.textDuration.Text = "1000";
            this.textDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textDuration.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(418, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(165, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "R峰位置(ms)(400~1000)";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRPos
            // 
            this.textRPos.AlphaBackColor = System.Drawing.Color.Empty;
            this.textRPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRPos.BackColor = System.Drawing.Color.White;
            this.textRPos.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.textRPos.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.textRPos.BorderSize = 1;
            this.textRPos.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textRPos.CornerRadius = 2;
            this.textRPos.DropdownPosition = new System.Drawing.Point(0, 21);
            this.textRPos.Location = new System.Drawing.Point(586, 4);
            this.textRPos.MaxLength = 4;
            this.textRPos.MaxValue = 1000D;
            this.textRPos.MinValue = 400D;
            this.textRPos.Multiline = false;
            this.textRPos.Name = "textRPos";
            this.textRPos.NumericOnly = true;
            this.textRPos.PasswordChar = '\0';
            this.textRPos.PlaceHolderStr = "";
            this.textRPos.ReadOnly = false;
            this.textRPos.SelectionStart = 0;
            this.textRPos.Size = new System.Drawing.Size(74, 26);
            this.textRPos.TabIndex = 6;
            this.textRPos.TabStop = false;
            this.textRPos.Text = "500";
            this.textRPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textRPos.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(673, 7);
            this.label4.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 20);
            this.label4.TabIndex = 0;
            this.label4.Text = "显示行数(3~10)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLines
            // 
            this.textLines.AlphaBackColor = System.Drawing.Color.Empty;
            this.textLines.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLines.BackColor = System.Drawing.Color.White;
            this.textLines.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.textLines.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.textLines.BorderSize = 1;
            this.textLines.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textLines.CornerRadius = 2;
            this.textLines.DropdownPosition = new System.Drawing.Point(0, 21);
            this.textLines.Location = new System.Drawing.Point(785, 4);
            this.textLines.MaxLength = 2;
            this.textLines.MaxValue = 10D;
            this.textLines.MinValue = 3D;
            this.textLines.Multiline = false;
            this.textLines.Name = "textLines";
            this.textLines.NumericOnly = true;
            this.textLines.PasswordChar = '\0';
            this.textLines.PlaceHolderStr = "";
            this.textLines.ReadOnly = false;
            this.textLines.SelectionStart = 0;
            this.textLines.Size = new System.Drawing.Size(74, 26);
            this.textLines.TabIndex = 6;
            this.textLines.TabStop = false;
            this.textLines.Text = "3";
            this.textLines.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textLines.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(872, 7);
            this.label5.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 20);
            this.label5.TabIndex = 0;
            this.label5.Text = "放大倍数";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // enlarge
            // 
            this.enlarge.CheckedListBox = null;
            this.enlarge.Control = null;
            this.enlarge.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.enlarge.Dock = System.Windows.Forms.DockStyle.Fill;
            this.enlarge.FormattingEnabled = true;
            this.enlarge.IsShown = false;
            this.enlarge.IsUnresponsive = false;
            this.enlarge.Location = new System.Drawing.Point(940, 3);
            this.enlarge.Name = "enlarge";
            this.enlarge.PlaceHolderStr = null;
            this.enlarge.Size = new System.Drawing.Size(74, 28);
            this.enlarge.TabIndex = 7;
            this.enlarge.TreeView = null;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnApply.AutoSize = true;
            this.btnApply.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnApply.DataFlag = null;
            this.btnApply.FollowBaseColor = false;
            this.btnApply.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnApply.IsHighlight = true;
            this.btnApply.Location = new System.Drawing.Point(1031, 3);
            this.btnApply.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnApply.Name = "btnApply";
            this.btnApply.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnApply.Size = new System.Drawing.Size(50, 28);
            this.btnApply.TabIndex = 14;
            this.btnApply.Text = "应用";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnAPPly_Click);
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 43);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1155, 609);
            this.panel1.TabIndex = 4;
            // 
            // WPWavePageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "WPWavePageControl";
            this.Size = new System.Drawing.Size(1161, 655);
            this.Load += new System.EventHandler(this.WPWavePageControl_Load);
            this.SizeChanged += new System.EventHandler(this.WPWavePageControl_SizeChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Zoncare.Component.Controls.ZoncareComboBox cmbLead;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private Zoncare.Component.Controls.TextBoxEx textDuration;
        private System.Windows.Forms.Label label3;
        private Zoncare.Component.Controls.TextBoxEx textRPos;
        private System.Windows.Forms.Label label4;
        private Zoncare.Component.Controls.TextBoxEx textLines;
        private System.Windows.Forms.Label label5;
        private Zoncare.Component.Controls.ZoncareComboBox enlarge;
        private System.Windows.Forms.Panel panel1;
        private Zoncare.Component.Controls.RoundButton btnApply;
    }
}
