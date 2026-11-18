namespace SystemSettings
{
    partial class SystemSetting
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
            this.tabTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new Zoncare.Component.Controls.ZqTabControl();
            this.baseInfoTabPage = new System.Windows.Forms.TabPage();
            this.baseInfo = new SystemSettings.BaseInfo();
            this.ecgSettingTabPage = new System.Windows.Forms.TabPage();
            this.ecgSetting = new SystemSettings.ecgSetting();
            this.bpSettingTabPage = new System.Windows.Forms.TabPage();
            this.bpSetting1 = new SystemSettings.bpSetting();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.reportSetting = new SystemSettings.ReportSetting();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tlpMenu = new System.Windows.Forms.TableLayoutPanel();
            this.baseInfoBtn = new System.Windows.Forms.Button();
            this.ecgSettingBtn = new System.Windows.Forms.Button();
            this.reportSettingBtn = new System.Windows.Forms.Button();
            this.operateSettingBtn = new Zoncare.Component.Controls.RoundButton(this.components);
            this.bpSettingBtn = new System.Windows.Forms.Button();
            this.btnUserMgmt = new System.Windows.Forms.Button();
            this.userManagement1 = new SystemSettings.UserManagement();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabTableLayoutPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.baseInfoTabPage.SuspendLayout();
            this.ecgSettingTabPage.SuspendLayout();
            this.bpSettingTabPage.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tlpMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel1.Controls.Add(this.tabTableLayoutPanel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tlpMenu, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1039, 612);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // tabTableLayoutPanel
            // 
            this.tabTableLayoutPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tabTableLayoutPanel.ColumnCount = 1;
            this.tabTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabTableLayoutPanel.Controls.Add(this.tabControl1, 0, 0);
            this.tabTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabTableLayoutPanel.Location = new System.Drawing.Point(155, 0);
            this.tabTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.tabTableLayoutPanel.Name = "tabTableLayoutPanel";
            this.tabTableLayoutPanel.RowCount = 1;
            this.tabTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tabTableLayoutPanel.Size = new System.Drawing.Size(884, 612);
            this.tabTableLayoutPanel.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.baseInfoTabPage);
            this.tabControl1.Controls.Add(this.ecgSettingTabPage);
            this.tabControl1.Controls.Add(this.bpSettingTabPage);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl1.ItemSize = new System.Drawing.Size(100, 20);
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(880, 608);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 1;
            // 
            // baseInfoTabPage
            // 
            this.baseInfoTabPage.BackColor = System.Drawing.Color.White;
            this.baseInfoTabPage.Controls.Add(this.baseInfo);
            this.baseInfoTabPage.Location = new System.Drawing.Point(4, 24);
            this.baseInfoTabPage.Margin = new System.Windows.Forms.Padding(2);
            this.baseInfoTabPage.Name = "baseInfoTabPage";
            this.baseInfoTabPage.Padding = new System.Windows.Forms.Padding(2);
            this.baseInfoTabPage.Size = new System.Drawing.Size(872, 580);
            this.baseInfoTabPage.TabIndex = 0;
            this.baseInfoTabPage.Text = "tabPage1";
            // 
            // baseInfo
            // 
            this.baseInfo.BackColor = System.Drawing.Color.White;
            this.baseInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseInfo.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.baseInfo.Location = new System.Drawing.Point(2, 2);
            this.baseInfo.Name = "baseInfo";
            this.baseInfo.Size = new System.Drawing.Size(868, 576);
            this.baseInfo.TabIndex = 0;
            // 
            // ecgSettingTabPage
            // 
            this.ecgSettingTabPage.Controls.Add(this.ecgSetting);
            this.ecgSettingTabPage.Location = new System.Drawing.Point(4, 24);
            this.ecgSettingTabPage.Margin = new System.Windows.Forms.Padding(2);
            this.ecgSettingTabPage.Name = "ecgSettingTabPage";
            this.ecgSettingTabPage.Padding = new System.Windows.Forms.Padding(2);
            this.ecgSettingTabPage.Size = new System.Drawing.Size(872, 580);
            this.ecgSettingTabPage.TabIndex = 1;
            this.ecgSettingTabPage.Text = "tabPage2";
            // 
            // ecgSetting
            // 
            this.ecgSetting.BackColor = System.Drawing.Color.White;
            this.ecgSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ecgSetting.Location = new System.Drawing.Point(2, 2);
            this.ecgSetting.Name = "ecgSetting";
            this.ecgSetting.Size = new System.Drawing.Size(868, 576);
            this.ecgSetting.TabIndex = 0;
            // 
            // bpSettingTabPage
            // 
            this.bpSettingTabPage.Controls.Add(this.bpSetting1);
            this.bpSettingTabPage.Location = new System.Drawing.Point(4, 24);
            this.bpSettingTabPage.Margin = new System.Windows.Forms.Padding(2);
            this.bpSettingTabPage.Name = "bpSettingTabPage";
            this.bpSettingTabPage.Padding = new System.Windows.Forms.Padding(2);
            this.bpSettingTabPage.Size = new System.Drawing.Size(872, 580);
            this.bpSettingTabPage.TabIndex = 3;
            this.bpSettingTabPage.Text = "tabPage4";
            // 
            // bpSetting1
            // 
            this.bpSetting1.BackColor = System.Drawing.Color.White;
            this.bpSetting1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bpSetting1.Location = new System.Drawing.Point(2, 2);
            this.bpSetting1.Name = "bpSetting1";
            this.bpSetting1.Size = new System.Drawing.Size(868, 576);
            this.bpSetting1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.reportSetting);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(872, 580);
            this.tabPage1.TabIndex = 4;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // reportSetting
            // 
            this.reportSetting.BackColor = System.Drawing.Color.White;
            this.reportSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportSetting.Location = new System.Drawing.Point(3, 3);
            this.reportSetting.Name = "reportSetting";
            this.reportSetting.Size = new System.Drawing.Size(866, 574);
            this.reportSetting.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(872, 580);
            this.tabPage2.TabIndex = 5;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.userManagement1);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(872, 580);
            this.tabPage3.TabIndex = 6;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tlpMenu
            // 
            this.tlpMenu.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tlpMenu.ColumnCount = 1;
            this.tlpMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMenu.Controls.Add(this.baseInfoBtn, 0, 0);
            this.tlpMenu.Controls.Add(this.ecgSettingBtn, 0, 1);
            this.tlpMenu.Controls.Add(this.reportSettingBtn, 0, 3);
            this.tlpMenu.Controls.Add(this.operateSettingBtn, 0, 4);
            this.tlpMenu.Controls.Add(this.bpSettingBtn, 0, 2);
            this.tlpMenu.Controls.Add(this.btnUserMgmt, 0, 5);
            this.tlpMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMenu.Location = new System.Drawing.Point(0, 0);
            this.tlpMenu.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMenu.Name = "tlpMenu";
            this.tlpMenu.RowCount = 7;
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMenu.Size = new System.Drawing.Size(155, 612);
            this.tlpMenu.TabIndex = 1;
            // 
            // baseInfoBtn
            // 
            this.baseInfoBtn.BackColor = System.Drawing.Color.WhiteSmoke;
            this.baseInfoBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseInfoBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.baseInfoBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.baseInfoBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.baseInfoBtn.Location = new System.Drawing.Point(0, 0);
            this.baseInfoBtn.Margin = new System.Windows.Forms.Padding(0);
            this.baseInfoBtn.Name = "baseInfoBtn";
            this.baseInfoBtn.Size = new System.Drawing.Size(155, 40);
            this.baseInfoBtn.TabIndex = 0;
            this.baseInfoBtn.Text = "基本信息";
            this.baseInfoBtn.UseVisualStyleBackColor = false;
            // 
            // ecgSettingBtn
            // 
            this.ecgSettingBtn.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ecgSettingBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ecgSettingBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.ecgSettingBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ecgSettingBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.ecgSettingBtn.Location = new System.Drawing.Point(0, 40);
            this.ecgSettingBtn.Margin = new System.Windows.Forms.Padding(0);
            this.ecgSettingBtn.Name = "ecgSettingBtn";
            this.ecgSettingBtn.Size = new System.Drawing.Size(155, 40);
            this.ecgSettingBtn.TabIndex = 1;
            this.ecgSettingBtn.Text = "心电参数";
            this.ecgSettingBtn.UseVisualStyleBackColor = false;
            // 
            // reportSettingBtn
            // 
            this.reportSettingBtn.BackColor = System.Drawing.Color.WhiteSmoke;
            this.reportSettingBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportSettingBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.reportSettingBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.reportSettingBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.reportSettingBtn.Location = new System.Drawing.Point(0, 120);
            this.reportSettingBtn.Margin = new System.Windows.Forms.Padding(0);
            this.reportSettingBtn.Name = "reportSettingBtn";
            this.reportSettingBtn.Size = new System.Drawing.Size(155, 40);
            this.reportSettingBtn.TabIndex = 3;
            this.reportSettingBtn.Text = "报告参数";
            this.reportSettingBtn.UseVisualStyleBackColor = false;
            // 
            // operateSettingBtn
            // 
            this.operateSettingBtn.BackColor = System.Drawing.Color.WhiteSmoke;
            this.operateSettingBtn.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.operateSettingBtn.DataFlag = null;
            this.operateSettingBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.operateSettingBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.operateSettingBtn.FollowBaseColor = false;
            this.operateSettingBtn.Location = new System.Drawing.Point(2, 162);
            this.operateSettingBtn.Margin = new System.Windows.Forms.Padding(2);
            this.operateSettingBtn.Name = "operateSettingBtn";
            this.operateSettingBtn.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.operateSettingBtn.Size = new System.Drawing.Size(128, 36);
            this.operateSettingBtn.TabIndex = 4;
            this.operateSettingBtn.Text = "习惯操作";
            this.operateSettingBtn.UseVisualStyleBackColor = false;
            this.operateSettingBtn.Visible = false;
            // 
            // bpSettingBtn
            // 
            this.bpSettingBtn.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bpSettingBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bpSettingBtn.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.bpSettingBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bpSettingBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.bpSettingBtn.Location = new System.Drawing.Point(0, 80);
            this.bpSettingBtn.Margin = new System.Windows.Forms.Padding(0);
            this.bpSettingBtn.Name = "bpSettingBtn";
            this.bpSettingBtn.Size = new System.Drawing.Size(155, 40);
            this.bpSettingBtn.TabIndex = 2;
            this.bpSettingBtn.Text = "血压参数";
            this.bpSettingBtn.UseVisualStyleBackColor = false;
            // 
            // btnUserMgmt
            // 
            this.btnUserMgmt.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnUserMgmt.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.btnUserMgmt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserMgmt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.btnUserMgmt.Location = new System.Drawing.Point(0, 200);
            this.btnUserMgmt.Margin = new System.Windows.Forms.Padding(0);
            this.btnUserMgmt.Name = "btnUserMgmt";
            this.btnUserMgmt.Size = new System.Drawing.Size(155, 40);
            this.btnUserMgmt.TabIndex = 5;
            this.btnUserMgmt.Text = "账户管理";
            this.btnUserMgmt.UseVisualStyleBackColor = false;
            // 
            // userManagement1
            // 
            this.userManagement1.BackColor = System.Drawing.Color.White;
            this.userManagement1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userManagement1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.userManagement1.Location = new System.Drawing.Point(0, 0);
            this.userManagement1.Name = "userManagement1";
            this.userManagement1.Size = new System.Drawing.Size(872, 580);
            this.userManagement1.TabIndex = 0;
            // 
            // SystemSetting
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1039, 612);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SystemSetting";
            this.Text = "SystemSetting";
            this.Load += new System.EventHandler(this.SystemSetting_Load);
            this.Shown += new System.EventHandler(this.SystemSetting_Shown);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabTableLayoutPanel.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.baseInfoTabPage.ResumeLayout(false);
            this.ecgSettingTabPage.ResumeLayout(false);
            this.bpSettingTabPage.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tlpMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabPage bpSettingTabPage;
        private Zoncare.Component.Controls.ZqTabControl tabControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tabTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tlpMenu;
        private System.Windows.Forms.Button baseInfoBtn;
        private System.Windows.Forms.Button ecgSettingBtn;
        private System.Windows.Forms.Button reportSettingBtn;
        private System.Windows.Forms.TabPage baseInfoTabPage;
        private BaseInfo baseInfo;
        private System.Windows.Forms.TabPage ecgSettingTabPage;
        private ecgSetting ecgSetting;
        private System.Windows.Forms.Button bpSettingBtn;
        private System.Windows.Forms.TabPage tabPage1;
        private ReportSetting reportSetting;
        private bpSetting bpSetting1;
        private Zoncare.Component.Controls.RoundButton operateSettingBtn;
        private System.Windows.Forms.Button btnUserMgmt;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private UserManagement userManagement1;
    }
}