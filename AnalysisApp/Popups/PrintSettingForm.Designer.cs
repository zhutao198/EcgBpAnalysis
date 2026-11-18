namespace AnalysisApp
{
    partial class PrintSettingForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cbxLeadI = new System.Windows.Forms.CheckBox();
            this.cbxLeadII = new System.Windows.Forms.CheckBox();
            this.cbxLeadIII = new System.Windows.Forms.CheckBox();
            this.cbxLeadaVR = new System.Windows.Forms.CheckBox();
            this.cbxLeadaVL = new System.Windows.Forms.CheckBox();
            this.cbxLeadaVF = new System.Windows.Forms.CheckBox();
            this.cbxLeadV1 = new System.Windows.Forms.CheckBox();
            this.cbxLeadV2 = new System.Windows.Forms.CheckBox();
            this.cbxLeadV3 = new System.Windows.Forms.CheckBox();
            this.cbxLeadV4 = new System.Windows.Forms.CheckBox();
            this.cbxLeadV5 = new System.Windows.Forms.CheckBox();
            this.cbxLeadV6 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnOK = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnPrint = new Zoncare.Component.Controls.RoundButton(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.cbxGain = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cbxDirection = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lblLength = new System.Windows.Forms.Label();
            this.cbxDuration = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.tbxStartTime = new Zoncare.Component.Controls.TextBoxEx();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbxLabel = new System.Windows.Forms.ComboBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tbxAvgHR = new Zoncare.Component.Controls.TextBoxEx();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.groupBox6, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.33149F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.90411F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.71233F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.19178F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.72603F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(484, 419);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(6, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(472, 82);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "打印通道";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadI, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadII, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadIII, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadaVR, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadaVL, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadaVF, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadV1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadV2, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadV3, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadV4, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadV5, 4, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbxLeadV6, 5, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 18);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(457, 58);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // cbxLeadI
            // 
            this.cbxLeadI.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadI.Checked = true;
            this.cbxLeadI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadI.Location = new System.Drawing.Point(8, 4);
            this.cbxLeadI.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadI.Name = "cbxLeadI";
            this.cbxLeadI.Size = new System.Drawing.Size(60, 20);
            this.cbxLeadI.TabIndex = 0;
            this.cbxLeadI.Text = "I";
            this.cbxLeadI.UseVisualStyleBackColor = true;
            // 
            // cbxLeadII
            // 
            this.cbxLeadII.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadII.Checked = true;
            this.cbxLeadII.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadII.Location = new System.Drawing.Point(84, 4);
            this.cbxLeadII.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadII.Name = "cbxLeadII";
            this.cbxLeadII.Size = new System.Drawing.Size(60, 20);
            this.cbxLeadII.TabIndex = 1;
            this.cbxLeadII.Text = "II";
            this.cbxLeadII.UseVisualStyleBackColor = true;
            // 
            // cbxLeadIII
            // 
            this.cbxLeadIII.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadIII.Checked = true;
            this.cbxLeadIII.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadIII.Location = new System.Drawing.Point(160, 4);
            this.cbxLeadIII.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadIII.Name = "cbxLeadIII";
            this.cbxLeadIII.Size = new System.Drawing.Size(60, 20);
            this.cbxLeadIII.TabIndex = 2;
            this.cbxLeadIII.Text = "III";
            this.cbxLeadIII.UseVisualStyleBackColor = true;
            // 
            // cbxLeadaVR
            // 
            this.cbxLeadaVR.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadaVR.Checked = true;
            this.cbxLeadaVR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadaVR.Location = new System.Drawing.Point(236, 4);
            this.cbxLeadaVR.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadaVR.Name = "cbxLeadaVR";
            this.cbxLeadaVR.Size = new System.Drawing.Size(60, 20);
            this.cbxLeadaVR.TabIndex = 3;
            this.cbxLeadaVR.Text = "aVR";
            this.cbxLeadaVR.UseVisualStyleBackColor = true;
            // 
            // cbxLeadaVL
            // 
            this.cbxLeadaVL.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadaVL.Checked = true;
            this.cbxLeadaVL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadaVL.Location = new System.Drawing.Point(312, 4);
            this.cbxLeadaVL.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadaVL.Name = "cbxLeadaVL";
            this.cbxLeadaVL.Size = new System.Drawing.Size(60, 20);
            this.cbxLeadaVL.TabIndex = 4;
            this.cbxLeadaVL.Text = "aVL";
            this.cbxLeadaVL.UseVisualStyleBackColor = true;
            // 
            // cbxLeadaVF
            // 
            this.cbxLeadaVF.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadaVF.Checked = true;
            this.cbxLeadaVF.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadaVF.Location = new System.Drawing.Point(388, 4);
            this.cbxLeadaVF.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadaVF.Name = "cbxLeadaVF";
            this.cbxLeadaVF.Size = new System.Drawing.Size(60, 20);
            this.cbxLeadaVF.TabIndex = 5;
            this.cbxLeadaVF.Text = "aVF";
            this.cbxLeadaVF.UseVisualStyleBackColor = true;
            // 
            // cbxLeadV1
            // 
            this.cbxLeadV1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadV1.Checked = true;
            this.cbxLeadV1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadV1.Location = new System.Drawing.Point(8, 33);
            this.cbxLeadV1.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadV1.Name = "cbxLeadV1";
            this.cbxLeadV1.Size = new System.Drawing.Size(60, 21);
            this.cbxLeadV1.TabIndex = 6;
            this.cbxLeadV1.Text = "V1";
            this.cbxLeadV1.UseVisualStyleBackColor = true;
            // 
            // cbxLeadV2
            // 
            this.cbxLeadV2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadV2.Checked = true;
            this.cbxLeadV2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadV2.Location = new System.Drawing.Point(84, 33);
            this.cbxLeadV2.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadV2.Name = "cbxLeadV2";
            this.cbxLeadV2.Size = new System.Drawing.Size(60, 21);
            this.cbxLeadV2.TabIndex = 7;
            this.cbxLeadV2.Text = "V2";
            this.cbxLeadV2.UseVisualStyleBackColor = true;
            // 
            // cbxLeadV3
            // 
            this.cbxLeadV3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadV3.Checked = true;
            this.cbxLeadV3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadV3.Location = new System.Drawing.Point(160, 33);
            this.cbxLeadV3.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadV3.Name = "cbxLeadV3";
            this.cbxLeadV3.Size = new System.Drawing.Size(60, 21);
            this.cbxLeadV3.TabIndex = 8;
            this.cbxLeadV3.Text = "V3";
            this.cbxLeadV3.UseVisualStyleBackColor = true;
            // 
            // cbxLeadV4
            // 
            this.cbxLeadV4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadV4.Checked = true;
            this.cbxLeadV4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadV4.Location = new System.Drawing.Point(236, 33);
            this.cbxLeadV4.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadV4.Name = "cbxLeadV4";
            this.cbxLeadV4.Size = new System.Drawing.Size(60, 21);
            this.cbxLeadV4.TabIndex = 9;
            this.cbxLeadV4.Text = "V4";
            this.cbxLeadV4.UseVisualStyleBackColor = true;
            // 
            // cbxLeadV5
            // 
            this.cbxLeadV5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadV5.Checked = true;
            this.cbxLeadV5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadV5.Location = new System.Drawing.Point(312, 33);
            this.cbxLeadV5.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadV5.Name = "cbxLeadV5";
            this.cbxLeadV5.Size = new System.Drawing.Size(60, 21);
            this.cbxLeadV5.TabIndex = 10;
            this.cbxLeadV5.Text = "V5";
            this.cbxLeadV5.UseVisualStyleBackColor = true;
            // 
            // cbxLeadV6
            // 
            this.cbxLeadV6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLeadV6.Checked = true;
            this.cbxLeadV6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxLeadV6.Location = new System.Drawing.Point(388, 33);
            this.cbxLeadV6.Margin = new System.Windows.Forms.Padding(0);
            this.cbxLeadV6.Name = "cbxLeadV6";
            this.cbxLeadV6.Size = new System.Drawing.Size(60, 21);
            this.cbxLeadV6.TabIndex = 11;
            this.cbxLeadV6.Text = "V6";
            this.cbxLeadV6.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(71, -4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "12导联连续片段图";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Visible = false;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 4;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Controls.Add(this.btnCancel, 2, 0);
            this.tableLayoutPanel5.Controls.Add(this.btnOK, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.btnPrint, 3, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(6, 371);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(472, 42);
            this.tableLayoutPanel5.TabIndex = 4;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCancel.DataFlag = null;
            this.btnCancel.FollowBaseColor = false;
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(132)))), ((int)(((byte)(207)))));
            this.btnCancel.Location = new System.Drawing.Point(242, 7);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCancel.Size = new System.Drawing.Size(75, 28);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnOK.DataFlag = null;
            this.btnOK.FollowBaseColor = false;
            this.btnOK.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnOK.IsHighlight = true;
            this.btnOK.Location = new System.Drawing.Point(155, 7);
            this.btnOK.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnOK.Size = new System.Drawing.Size(75, 28);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnPrint.DataFlag = null;
            this.btnPrint.FollowBaseColor = false;
            this.btnPrint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(132)))), ((int)(((byte)(207)))));
            this.btnPrint.Location = new System.Drawing.Point(394, 7);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnPrint.Size = new System.Drawing.Size(75, 28);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "即时打印";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel4);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(6, 85);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(472, 69);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "其它设置";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.cbxGain, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.cbxDirection, 3, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 22);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(466, 44);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // cbxGain
            // 
            this.cbxGain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxGain.CheckedListBox = null;
            this.cbxGain.Control = null;
            this.cbxGain.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxGain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxGain.FormattingEnabled = true;
            this.cbxGain.IsShown = false;
            this.cbxGain.IsUnresponsive = false;
            this.cbxGain.Location = new System.Drawing.Point(46, 11);
            this.cbxGain.Margin = new System.Windows.Forms.Padding(3, 0, 10, 0);
            this.cbxGain.Name = "cbxGain";
            this.cbxGain.PlaceHolderStr = null;
            this.cbxGain.Size = new System.Drawing.Size(163, 27);
            this.cbxGain.TabIndex = 3;
            this.cbxGain.TreeView = null;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 20);
            this.label4.TabIndex = 1;
            this.label4.Text = "增益";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(222, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 20);
            this.label5.TabIndex = 2;
            this.label5.Text = "打印方向";
            // 
            // cbxDirection
            // 
            this.cbxDirection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxDirection.CheckedListBox = null;
            this.cbxDirection.Control = null;
            this.cbxDirection.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxDirection.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDirection.FormattingEnabled = true;
            this.cbxDirection.IsShown = false;
            this.cbxDirection.IsUnresponsive = false;
            this.cbxDirection.Location = new System.Drawing.Point(293, 11);
            this.cbxDirection.Margin = new System.Windows.Forms.Padding(3, 0, 10, 0);
            this.cbxDirection.Name = "cbxDirection";
            this.cbxDirection.PlaceHolderStr = null;
            this.cbxDirection.Size = new System.Drawing.Size(163, 27);
            this.cbxDirection.TabIndex = 4;
            this.cbxDirection.TreeView = null;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(6, 218);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(472, 75);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "打印时长（秒）";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.lblLength, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.cbxDuration, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 22);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(466, 50);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // lblLength
            // 
            this.lblLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLength.AutoSize = true;
            this.lblLength.Location = new System.Drawing.Point(135, 15);
            this.lblLength.Name = "lblLength";
            this.lblLength.Size = new System.Drawing.Size(107, 20);
            this.lblLength.TabIndex = 1;
            this.lblLength.Text = "当前选定的区间";
            this.lblLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbxDuration
            // 
            this.cbxDuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxDuration.CheckedListBox = null;
            this.cbxDuration.Control = null;
            this.cbxDuration.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxDuration.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxDuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDuration.FormattingEnabled = true;
            this.cbxDuration.IsShown = false;
            this.cbxDuration.IsUnresponsive = false;
            this.cbxDuration.Location = new System.Drawing.Point(3, 14);
            this.cbxDuration.Margin = new System.Windows.Forms.Padding(3, 0, 10, 0);
            this.cbxDuration.Name = "cbxDuration";
            this.cbxDuration.PlaceHolderStr = null;
            this.cbxDuration.Size = new System.Drawing.Size(119, 27);
            this.cbxDuration.TabIndex = 5;
            this.cbxDuration.TreeView = null;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.tbxStartTime);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Location = new System.Drawing.Point(6, 157);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox6.Size = new System.Drawing.Size(472, 55);
            this.groupBox6.TabIndex = 5;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "起始时间";
            // 
            // tbxStartTime
            // 
            this.tbxStartTime.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxStartTime.BackColor = System.Drawing.Color.White;
            this.tbxStartTime.BorderColor = System.Drawing.Color.Empty;
            this.tbxStartTime.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxStartTime.BorderSize = 1;
            this.tbxStartTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxStartTime.CornerRadius = 2;
            this.tbxStartTime.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxStartTime.Location = new System.Drawing.Point(9, 23);
            this.tbxStartTime.MaxLength = 32767;
            this.tbxStartTime.Multiline = false;
            this.tbxStartTime.Name = "tbxStartTime";
            this.tbxStartTime.PasswordChar = '\0';
            this.tbxStartTime.PlaceHolderStr = null;
            this.tbxStartTime.ReadOnly = true;
            this.tbxStartTime.SelectionStart = 0;
            this.tbxStartTime.Size = new System.Drawing.Size(200, 26);
            this.tbxStartTime.TabIndex = 0;
            this.tbxStartTime.TabStop = false;
            this.tbxStartTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxStartTime.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.groupBox4, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.groupBox5, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(6, 299);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(472, 66);
            this.tableLayoutPanel6.TabIndex = 3;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cbxLabel);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 3);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox4.Size = new System.Drawing.Size(233, 60);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "标签";
            // 
            // cbxLabel
            // 
            this.cbxLabel.FormattingEnabled = true;
            this.cbxLabel.Location = new System.Drawing.Point(9, 22);
            this.cbxLabel.Name = "cbxLabel";
            this.cbxLabel.Size = new System.Drawing.Size(200, 28);
            this.cbxLabel.TabIndex = 3;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tbxAvgHR);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(239, 3);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox5.Size = new System.Drawing.Size(233, 60);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "心率";
            // 
            // tbxAvgHR
            // 
            this.tbxAvgHR.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxAvgHR.BackColor = System.Drawing.Color.White;
            this.tbxAvgHR.BorderColor = System.Drawing.Color.Empty;
            this.tbxAvgHR.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxAvgHR.BorderSize = 1;
            this.tbxAvgHR.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxAvgHR.CornerRadius = 2;
            this.tbxAvgHR.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxAvgHR.Location = new System.Drawing.Point(9, 23);
            this.tbxAvgHR.MaxLength = 32767;
            this.tbxAvgHR.Multiline = false;
            this.tbxAvgHR.Name = "tbxAvgHR";
            this.tbxAvgHR.PasswordChar = '\0';
            this.tbxAvgHR.PlaceHolderStr = null;
            this.tbxAvgHR.ReadOnly = false;
            this.tbxAvgHR.SelectionStart = 0;
            this.tbxAvgHR.Size = new System.Drawing.Size(210, 26);
            this.tbxAvgHR.TabIndex = 0;
            this.tbxAvgHR.TabStop = false;
            this.tbxAvgHR.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxAvgHR.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // PrintSettingForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(484, 419);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PrintSettingForm";
            this.ShowInTaskbar = false;
            this.Text = "PrintSettingForm";
            this.Load += new System.EventHandler(this.PrintSettingForm_Load);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox cbxLeadI;
        private System.Windows.Forms.CheckBox cbxLeadII;
        private System.Windows.Forms.CheckBox cbxLeadIII;
        private System.Windows.Forms.CheckBox cbxLeadaVR;
        private System.Windows.Forms.CheckBox cbxLeadaVL;
        private System.Windows.Forms.CheckBox cbxLeadV1;
        private System.Windows.Forms.CheckBox cbxLeadaVF;
        private System.Windows.Forms.CheckBox cbxLeadV2;
        private System.Windows.Forms.CheckBox cbxLeadV3;
        private System.Windows.Forms.CheckBox cbxLeadV4;
        private System.Windows.Forms.CheckBox cbxLeadV5;
        private System.Windows.Forms.CheckBox cbxLeadV6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Zoncare.Component.Controls.RoundButton btnOK;
        private Zoncare.Component.Controls.RoundButton btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label4;
        private Zoncare.Component.Controls.RoundButton btnPrint;
        private System.Windows.Forms.ComboBox cbxLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox6;
        private Zoncare.Component.Controls.TextBoxEx tbxAvgHR;
        private Zoncare.Component.Controls.TextBoxEx tbxStartTime;
        private Zoncare.Component.Controls.ZoncareComboBox cbxGain;
        private Zoncare.Component.Controls.ZoncareComboBox cbxDirection;
        private Zoncare.Component.Controls.ZoncareComboBox cbxDuration;
    }
}