namespace ABPMRepository
{
    partial class DeviceSettings
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
            this.gpTime = new System.Windows.Forms.GroupBox();
            this.gridTimeSettings = new System.Windows.Forms.DataGridView();
            this.picConn = new System.Windows.Forms.PictureBox();
            this.gbFrontPanel = new System.Windows.Forms.GroupBox();
            this.cbxDisplay = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxKeyboard = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gpAlarm = new System.Windows.Forms.GroupBox();
            this.cbxNight = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbxDaytime = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbxUnit = new System.Windows.Forms.ComboBox();
            this.btnProgramming = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpSystemTime = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.gpThreshold = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtMaxSystolic = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMinSystolic = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtMaxDiastolic = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtMinDiastolic = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtMaxPressure = new System.Windows.Forms.TextBox();
            this.chbSysTimeSync = new System.Windows.Forms.CheckBox();
            this.cbxBluetooth = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.gpTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTimeSettings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picConn)).BeginInit();
            this.gbFrontPanel.SuspendLayout();
            this.gpAlarm.SuspendLayout();
            this.gpThreshold.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // gpTime
            // 
            this.gpTime.Controls.Add(this.gridTimeSettings);
            this.gpTime.Location = new System.Drawing.Point(5, 30);
            this.gpTime.Name = "gpTime";
            this.gpTime.Size = new System.Drawing.Size(513, 246);
            this.gpTime.TabIndex = 0;
            this.gpTime.TabStop = false;
            this.gpTime.Text = "时间分配";
            // 
            // gridTimeSettings
            // 
            this.gridTimeSettings.AllowUserToAddRows = false;
            this.gridTimeSettings.AllowUserToDeleteRows = false;
            this.gridTimeSettings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTimeSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTimeSettings.Location = new System.Drawing.Point(3, 17);
            this.gridTimeSettings.Name = "gridTimeSettings";
            this.gridTimeSettings.RowTemplate.Height = 23;
            this.gridTimeSettings.Size = new System.Drawing.Size(507, 226);
            this.gridTimeSettings.TabIndex = 99;
            this.gridTimeSettings.TabStop = false;
            this.gridTimeSettings.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridTimeSettings_CellEnter);
            this.gridTimeSettings.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridTimeSettings_CellLeave);
            this.gridTimeSettings.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridTimeSettings_KeyDown);
            // 
            // picConn
            // 
            this.picConn.Location = new System.Drawing.Point(499, 14);
            this.picConn.Name = "picConn";
            this.picConn.Size = new System.Drawing.Size(16, 16);
            this.picConn.TabIndex = 14;
            this.picConn.TabStop = false;
            this.picConn.Paint += new System.Windows.Forms.PaintEventHandler(this.picConn_Paint);
            // 
            // gbFrontPanel
            // 
            this.gbFrontPanel.Controls.Add(this.cbxDisplay);
            this.gbFrontPanel.Controls.Add(this.label3);
            this.gbFrontPanel.Controls.Add(this.cbxKeyboard);
            this.gbFrontPanel.Controls.Add(this.label2);
            this.gbFrontPanel.Location = new System.Drawing.Point(494, 376);
            this.gbFrontPanel.Name = "gbFrontPanel";
            this.gbFrontPanel.Size = new System.Drawing.Size(277, 69);
            this.gbFrontPanel.TabIndex = 1;
            this.gbFrontPanel.TabStop = false;
            this.gbFrontPanel.Visible = false;
            // 
            // cbxDisplay
            // 
            this.cbxDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDisplay.FormattingEnabled = true;
            this.cbxDisplay.Location = new System.Drawing.Point(139, 35);
            this.cbxDisplay.Name = "cbxDisplay";
            this.cbxDisplay.Size = new System.Drawing.Size(103, 20);
            this.cbxDisplay.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(137, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "显示";
            // 
            // cbxKeyboard
            // 
            this.cbxKeyboard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxKeyboard.FormattingEnabled = true;
            this.cbxKeyboard.Location = new System.Drawing.Point(13, 35);
            this.cbxKeyboard.Name = "cbxKeyboard";
            this.cbxKeyboard.Size = new System.Drawing.Size(103, 20);
            this.cbxKeyboard.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "键盘";
            // 
            // gpAlarm
            // 
            this.gpAlarm.Controls.Add(this.cbxNight);
            this.gpAlarm.Controls.Add(this.label4);
            this.gpAlarm.Controls.Add(this.cbxDaytime);
            this.gpAlarm.Controls.Add(this.label5);
            this.gpAlarm.Location = new System.Drawing.Point(238, 282);
            this.gpAlarm.Name = "gpAlarm";
            this.gpAlarm.Size = new System.Drawing.Size(277, 69);
            this.gpAlarm.TabIndex = 2;
            this.gpAlarm.TabStop = false;
            // 
            // cbxNight
            // 
            this.cbxNight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxNight.FormattingEnabled = true;
            this.cbxNight.Location = new System.Drawing.Point(139, 35);
            this.cbxNight.Name = "cbxNight";
            this.cbxNight.Size = new System.Drawing.Size(103, 20);
            this.cbxNight.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(137, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "夜间报警";
            // 
            // cbxDaytime
            // 
            this.cbxDaytime.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDaytime.FormattingEnabled = true;
            this.cbxDaytime.Location = new System.Drawing.Point(13, 35);
            this.cbxDaytime.Name = "cbxDaytime";
            this.cbxDaytime.Size = new System.Drawing.Size(103, 20);
            this.cbxDaytime.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "白天报警";
            // 
            // cbxUnit
            // 
            this.cbxUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxUnit.FormattingEnabled = true;
            this.cbxUnit.Location = new System.Drawing.Point(238, 430);
            this.cbxUnit.Name = "cbxUnit";
            this.cbxUnit.Size = new System.Drawing.Size(103, 20);
            this.cbxUnit.TabIndex = 5;
            // 
            // btnProgramming
            // 
            this.btnProgramming.Location = new System.Drawing.Point(26, 460);
            this.btnProgramming.Name = "btnProgramming";
            this.btnProgramming.Size = new System.Drawing.Size(75, 23);
            this.btnProgramming.TabIndex = 6;
            this.btnProgramming.Text = "设置";
            this.btnProgramming.UseVisualStyleBackColor = true;
            this.btnProgramming.Click += new System.EventHandler(this.btnProgramming_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(132, 460);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(236, 414);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "压力的单位";
            // 
            // dtpSystemTime
            // 
            this.dtpSystemTime.CustomFormat = "yyyy/MM/dd HH:mm";
            this.dtpSystemTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpSystemTime.Location = new System.Drawing.Point(238, 382);
            this.dtpSystemTime.Name = "dtpSystemTime";
            this.dtpSystemTime.Size = new System.Drawing.Size(138, 21);
            this.dtpSystemTime.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(236, 367);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 9;
            this.label6.Text = "系统时间";
            // 
            // gpThreshold
            // 
            this.gpThreshold.Controls.Add(this.label8);
            this.gpThreshold.Controls.Add(this.txtMaxSystolic);
            this.gpThreshold.Controls.Add(this.label9);
            this.gpThreshold.Controls.Add(this.txtMinSystolic);
            this.gpThreshold.Controls.Add(this.label10);
            this.gpThreshold.Controls.Add(this.txtMaxDiastolic);
            this.gpThreshold.Controls.Add(this.label11);
            this.gpThreshold.Controls.Add(this.txtMinDiastolic);
            this.gpThreshold.Controls.Add(this.label7);
            this.gpThreshold.Controls.Add(this.txtMaxPressure);
            this.gpThreshold.Location = new System.Drawing.Point(5, 282);
            this.gpThreshold.Name = "gpThreshold";
            this.gpThreshold.Size = new System.Drawing.Size(218, 168);
            this.gpThreshold.TabIndex = 10;
            this.gpThreshold.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 12);
            this.label8.TabIndex = 2;
            this.label8.Text = "收缩压最大阈值";
            // 
            // txtMaxSystolic
            // 
            this.txtMaxSystolic.Location = new System.Drawing.Point(102, 20);
            this.txtMaxSystolic.Name = "txtMaxSystolic";
            this.txtMaxSystolic.Size = new System.Drawing.Size(100, 21);
            this.txtMaxSystolic.TabIndex = 3;
            this.txtMaxSystolic.Text = "140";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 60);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(89, 12);
            this.label9.TabIndex = 4;
            this.label9.Text = "收缩压最小阈值";
            // 
            // txtMinSystolic
            // 
            this.txtMinSystolic.Location = new System.Drawing.Point(102, 57);
            this.txtMinSystolic.Name = "txtMinSystolic";
            this.txtMinSystolic.Size = new System.Drawing.Size(100, 21);
            this.txtMinSystolic.TabIndex = 5;
            this.txtMinSystolic.Text = "90";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 97);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(89, 12);
            this.label10.TabIndex = 6;
            this.label10.Text = "舒张压最大阈值";
            // 
            // txtMaxDiastolic
            // 
            this.txtMaxDiastolic.Location = new System.Drawing.Point(102, 94);
            this.txtMaxDiastolic.Name = "txtMaxDiastolic";
            this.txtMaxDiastolic.Size = new System.Drawing.Size(100, 21);
            this.txtMaxDiastolic.TabIndex = 7;
            this.txtMaxDiastolic.Text = "90";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 134);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(89, 12);
            this.label11.TabIndex = 8;
            this.label11.Text = "舒张压最小阈值";
            // 
            // txtMinDiastolic
            // 
            this.txtMinDiastolic.Location = new System.Drawing.Point(102, 131);
            this.txtMinDiastolic.Name = "txtMinDiastolic";
            this.txtMinDiastolic.Size = new System.Drawing.Size(100, 21);
            this.txtMinDiastolic.TabIndex = 9;
            this.txtMinDiastolic.Text = "60";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 0;
            this.label7.Text = "最大充气压力";
            this.label7.Visible = false;
            // 
            // txtMaxPressure
            // 
            this.txtMaxPressure.Location = new System.Drawing.Point(102, 13);
            this.txtMaxPressure.Name = "txtMaxPressure";
            this.txtMaxPressure.Size = new System.Drawing.Size(100, 21);
            this.txtMaxPressure.TabIndex = 1;
            this.txtMaxPressure.Text = "280";
            this.txtMaxPressure.Visible = false;
            // 
            // chbSysTimeSync
            // 
            this.chbSysTimeSync.AutoSize = true;
            this.chbSysTimeSync.Checked = true;
            this.chbSysTimeSync.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbSysTimeSync.Location = new System.Drawing.Point(384, 387);
            this.chbSysTimeSync.Name = "chbSysTimeSync";
            this.chbSysTimeSync.Size = new System.Drawing.Size(96, 16);
            this.chbSysTimeSync.TabIndex = 11;
            this.chbSysTimeSync.Text = "与PC时间同步";
            this.chbSysTimeSync.UseVisualStyleBackColor = true;
            this.chbSysTimeSync.CheckedChanged += new System.EventHandler(this.chbSysTimeSync_CheckedChanged);
            // 
            // cbxBluetooth
            // 
            this.cbxBluetooth.Enabled = false;
            this.cbxBluetooth.FormattingEnabled = true;
            this.cbxBluetooth.Location = new System.Drawing.Point(343, 12);
            this.cbxBluetooth.Name = "cbxBluetooth";
            this.cbxBluetooth.Size = new System.Drawing.Size(154, 20);
            this.cbxBluetooth.TabIndex = 12;
            this.cbxBluetooth.SelectedIndexChanged += new System.EventHandler(this.cbxBluetooth_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(280, 15);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(65, 12);
            this.label12.TabIndex = 13;
            this.label12.Text = "蓝牙设备：";
            // 
            // errorProvider1
            // 
            this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider1.ContainerControl = this;
            // 
            // DeviceSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 495);
            this.Controls.Add(this.picConn);
            this.Controls.Add(this.cbxBluetooth);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.chbSysTimeSync);
            this.Controls.Add(this.gpThreshold);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.dtpSystemTime);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnProgramming);
            this.Controls.Add(this.cbxUnit);
            this.Controls.Add(this.gpAlarm);
            this.Controls.Add(this.gbFrontPanel);
            this.Controls.Add(this.gpTime);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DeviceSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "血压计参数设置";
            this.Load += new System.EventHandler(this.DeviceSettings_Load);
            this.gpTime.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTimeSettings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picConn)).EndInit();
            this.gbFrontPanel.ResumeLayout(false);
            this.gbFrontPanel.PerformLayout();
            this.gpAlarm.ResumeLayout(false);
            this.gpAlarm.PerformLayout();
            this.gpThreshold.ResumeLayout(false);
            this.gpThreshold.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gpTime;
        private System.Windows.Forms.DataGridView gridTimeSettings;
        private System.Windows.Forms.GroupBox gbFrontPanel;
        private System.Windows.Forms.ComboBox cbxDisplay;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbxKeyboard;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox gpAlarm;
        private System.Windows.Forms.ComboBox cbxNight;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbxDaytime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbxUnit;
        private System.Windows.Forms.Button btnProgramming;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpSystemTime;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox gpThreshold;
        private System.Windows.Forms.TextBox txtMinDiastolic;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtMaxDiastolic;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtMinSystolic;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtMaxSystolic;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtMaxPressure;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chbSysTimeSync;
        private System.Windows.Forms.ComboBox cbxBluetooth;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.PictureBox picConn;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}