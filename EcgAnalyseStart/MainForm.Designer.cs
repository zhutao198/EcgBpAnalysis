namespace EcgAnalyseStart
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
            _refreshTimer?.Stop();
            _timeoutTimer?.Stop();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip1 = new Zoncare.Component.Controls.ZqToolStrip();
            this.btnRegister = new Zoncare.Component.CustomToolStripButton();
            this.btnReadCard = new Zoncare.Component.CustomToolStripButton();
            this.btnInit = new Zoncare.Component.CustomToolStripButton();
            this.btnSetting = new Zoncare.Component.CustomToolStripButton();
            this.btnHelp = new Zoncare.Component.CustomToolStripButton();
            this.btnLogout = new Zoncare.Component.CustomToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ddlUser = new System.Windows.Forms.ToolStripDropDownButton();
            this.tabControl1 = new Zoncare.Component.Controls.ZqTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.btnRefresh = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnEdit = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnDelete = new Zoncare.Component.Controls.RoundButton(this.components);
            this.caseList1 = new EcgBpAnalysis.FuncComponent.CaseList();
            this.tlpFilter = new System.Windows.Forms.TableLayoutPanel();
            this.tbxName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnSearch1 = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tbxConclusion = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.dtpToDate = new System.Windows.Forms.DateTimePicker();
            this.label9 = new System.Windows.Forms.Label();
            this.dtpFromDate = new System.Windows.Forms.DateTimePicker();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.cbxAgeUnit = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.tbxToAge = new System.Windows.Forms.TextBox();
            this.tbxFromAge = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cbxSex = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cbxStatus = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cbxCheckItem = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.tbxCaseNo = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tlpDataGrid = new System.Windows.Forms.TableLayoutPanel();
            this.caseList2 = new EcgBpAnalysis.FuncComponent.CaseList();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnRestore = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnReallyDelete = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.Panel();
            this.caseList3 = new EcgBpAnalysis.FuncComponent.CaseList();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnCancelCollect = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tlpFilter2 = new System.Windows.Forms.TableLayoutPanel();
            this.tbxName2 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.btnSearch3 = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tbxConclusion2 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.dtpToDate2 = new System.Windows.Forms.DateTimePicker();
            this.label19 = new System.Windows.Forms.Label();
            this.dtpFromDate2 = new System.Windows.Forms.DateTimePicker();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.cbxAgeUnit2 = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.tbxToAge2 = new System.Windows.Forms.TextBox();
            this.tbxFromAge2 = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.cbxSex2 = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cbxStatus2 = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cbxCheckItem2 = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.tbxCaseNo2 = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsDriveUsed = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlpMenu = new System.Windows.Forms.TableLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.tlpControls = new System.Windows.Forms.TableLayoutPanel();
            this.picClose = new System.Windows.Forms.PictureBox();
            this.picMin = new System.Windows.Forms.PictureBox();
            this.toolStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tlpFilter.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tlpDataGrid.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tlpFilter2.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tlpMenu.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tlpControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(245)))));
            this.toolStrip1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRegister,
            this.btnReadCard,
            this.btnInit,
            this.btnSetting,
            this.btnHelp,
            this.btnLogout,
            this.toolStripSeparator1,
            this.ddlUser});
            this.toolStrip1.Location = new System.Drawing.Point(0, 30);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1350, 77);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnRegister
            // 
            this.btnRegister.ActiveImage = null;
            this.btnRegister.AutoSize = false;
            this.btnRegister.ImageTransparentColor = System.Drawing.Color.White;
            this.btnRegister.InactiveImage = null;
            this.btnRegister.Margin = new System.Windows.Forms.Padding(80, 3, 25, 10);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(64, 64);
            this.btnRegister.Text = "登记";
            this.btnRegister.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnRegister.TextColor = System.Drawing.Color.Empty;
            this.btnRegister.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnRegister.UserSelected = false;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // btnReadCard
            // 
            this.btnReadCard.ActiveImage = null;
            this.btnReadCard.AutoSize = false;
            this.btnReadCard.ImageTransparentColor = System.Drawing.Color.White;
            this.btnReadCard.InactiveImage = null;
            this.btnReadCard.Margin = new System.Windows.Forms.Padding(0, 3, 25, 10);
            this.btnReadCard.Name = "btnReadCard";
            this.btnReadCard.Size = new System.Drawing.Size(64, 64);
            this.btnReadCard.Text = "读卡";
            this.btnReadCard.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnReadCard.TextColor = System.Drawing.Color.Empty;
            this.btnReadCard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnReadCard.UserSelected = false;
            this.btnReadCard.Click += new System.EventHandler(this.btnReadCard_Click);
            // 
            // btnInit
            // 
            this.btnInit.ActiveImage = null;
            this.btnInit.AutoSize = false;
            this.btnInit.ImageTransparentColor = System.Drawing.Color.White;
            this.btnInit.InactiveImage = null;
            this.btnInit.Margin = new System.Windows.Forms.Padding(0, 3, 25, 10);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(78, 64);
            this.btnInit.Text = "初始化设备";
            this.btnInit.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnInit.TextColor = System.Drawing.Color.Empty;
            this.btnInit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnInit.UserSelected = false;
            this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.ActiveImage = null;
            this.btnSetting.AutoSize = false;
            this.btnSetting.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSetting.ImageTransparentColor = System.Drawing.Color.White;
            this.btnSetting.InactiveImage = null;
            this.btnSetting.Margin = new System.Windows.Forms.Padding(0, 3, 25, 10);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(64, 64);
            this.btnSetting.Text = "设置";
            this.btnSetting.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSetting.TextColor = System.Drawing.Color.Empty;
            this.btnSetting.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSetting.UserSelected = false;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.ActiveImage = null;
            this.btnHelp.AutoSize = false;
            this.btnHelp.ImageTransparentColor = System.Drawing.Color.White;
            this.btnHelp.InactiveImage = null;
            this.btnHelp.Margin = new System.Windows.Forms.Padding(0, 3, 25, 10);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(64, 64);
            this.btnHelp.Text = "用户指南";
            this.btnHelp.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnHelp.TextColor = System.Drawing.Color.Empty;
            this.btnHelp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnHelp.UserSelected = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnLogout
            // 
            this.btnLogout.ActiveImage = null;
            this.btnLogout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnLogout.AutoSize = false;
            this.btnLogout.ImageTransparentColor = System.Drawing.Color.White;
            this.btnLogout.InactiveImage = null;
            this.btnLogout.Margin = new System.Windows.Forms.Padding(0, 3, 25, 10);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(64, 64);
            this.btnLogout.Text = "注销";
            this.btnLogout.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnLogout.TextColor = System.Drawing.Color.Empty;
            this.btnLogout.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnLogout.UserSelected = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.AutoSize = false;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 30);
            // 
            // ddlUser
            // 
            this.ddlUser.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ddlUser.Image = ((System.Drawing.Image)(resources.GetObject("ddlUser.Image")));
            this.ddlUser.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ddlUser.Name = "ddlUser";
            this.ddlUser.Size = new System.Drawing.Size(81, 74);
            this.ddlUser.Text = "admin";
            this.ddlUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl1.Location = new System.Drawing.Point(184, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1166, 600);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tabPage1.Controls.Add(this.tableLayoutPanel3);
            this.tabPage1.Controls.Add(this.tlpFilter);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.tabPage1.Size = new System.Drawing.Size(1158, 567);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.caseList1, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 84);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1152, 483);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel5.ColumnCount = 4;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Controls.Add(this.btnRefresh, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.btnEdit, 2, 0);
            this.tableLayoutPanel5.Controls.Add(this.btnDelete, 3, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1146, 40);
            this.tableLayoutPanel5.TabIndex = 4;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRefresh.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnRefresh.DataFlag = null;
            this.btnRefresh.FollowBaseColor = false;
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnRefresh.Image = global::EcgBpAnalysis.Properties.Resources.refresh;
            this.btnRefresh.IsHighlight = true;
            this.btnRefresh.Location = new System.Drawing.Point(3, 7);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnRefresh.Size = new System.Drawing.Size(56, 26);
            this.btnRefresh.TabIndex = 10;
            this.btnRefresh.Tag = "15";
            this.btnRefresh.Text = "15s";
            this.btnRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnEdit.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnEdit.DataFlag = null;
            this.btnEdit.FollowBaseColor = false;
            this.btnEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnEdit.IsHighlight = true;
            this.btnEdit.Location = new System.Drawing.Point(990, 7);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnEdit.Size = new System.Drawing.Size(72, 26);
            this.btnEdit.TabIndex = 7;
            this.btnEdit.Text = "编辑";
            this.btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDelete.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnDelete.DataFlag = null;
            this.btnDelete.FollowBaseColor = false;
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnDelete.IsHighlight = true;
            this.btnDelete.Location = new System.Drawing.Point(1071, 7);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDelete.Size = new System.Drawing.Size(72, 26);
            this.btnDelete.TabIndex = 8;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // caseList1
            // 
            this.caseList1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.caseList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.caseList1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.caseList1.Location = new System.Drawing.Point(3, 47);
            this.caseList1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.caseList1.Name = "caseList1";
            this.caseList1.Size = new System.Drawing.Size(1146, 436);
            this.caseList1.TabIndex = 0;
            this.caseList1.Usage = EcgBpAnalysis.FuncComponent.CaseList.CaseListUsage.MainWindow;
            // 
            // tlpFilter
            // 
            this.tlpFilter.ColumnCount = 9;
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tlpFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter.Controls.Add(this.tbxName, 3, 0);
            this.tlpFilter.Controls.Add(this.label6, 0, 1);
            this.tlpFilter.Controls.Add(this.label1, 0, 0);
            this.tlpFilter.Controls.Add(this.label2, 2, 0);
            this.tlpFilter.Controls.Add(this.label3, 4, 0);
            this.tlpFilter.Controls.Add(this.label4, 6, 0);
            this.tlpFilter.Controls.Add(this.label5, 2, 1);
            this.tlpFilter.Controls.Add(this.label7, 4, 1);
            this.tlpFilter.Controls.Add(this.label8, 6, 1);
            this.tlpFilter.Controls.Add(this.btnSearch1, 8, 0);
            this.tlpFilter.Controls.Add(this.tbxConclusion, 7, 1);
            this.tlpFilter.Controls.Add(this.tableLayoutPanel2, 1, 1);
            this.tlpFilter.Controls.Add(this.tableLayoutPanel4, 3, 1);
            this.tlpFilter.Controls.Add(this.cbxSex, 5, 0);
            this.tlpFilter.Controls.Add(this.cbxStatus, 5, 1);
            this.tlpFilter.Controls.Add(this.cbxCheckItem, 7, 0);
            this.tlpFilter.Controls.Add(this.tbxCaseNo, 1, 0);
            this.tlpFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFilter.Location = new System.Drawing.Point(3, 3);
            this.tlpFilter.Name = "tlpFilter";
            this.tlpFilter.RowCount = 2;
            this.tlpFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFilter.Size = new System.Drawing.Size(1152, 81);
            this.tlpFilter.TabIndex = 1;
            // 
            // tbxName
            // 
            this.tbxName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxName.Location = new System.Drawing.Point(362, 7);
            this.tbxName.MaxLength = 20;
            this.tbxName.Name = "tbxName";
            this.tbxName.Size = new System.Drawing.Size(215, 26);
            this.tbxName.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 50);
            this.label6.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 20);
            this.label6.TabIndex = 5;
            this.label6.Text = "检查日期";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "病历号";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(319, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "姓名";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(595, 10);
            this.label3.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "性别";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(824, 10);
            this.label4.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 20);
            this.label4.TabIndex = 3;
            this.label4.Text = "检查项目";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(319, 50);
            this.label5.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 20);
            this.label5.TabIndex = 4;
            this.label5.Text = "年龄";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(595, 50);
            this.label7.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 20);
            this.label7.TabIndex = 6;
            this.label7.Text = "状态";
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(824, 50);
            this.label8.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 20);
            this.label8.TabIndex = 7;
            this.label8.Text = "报告结论";
            // 
            // btnSearch1
            // 
            this.btnSearch1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSearch1.DataFlag = null;
            this.btnSearch1.FollowBaseColor = false;
            this.btnSearch1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSearch1.Image = global::EcgBpAnalysis.Properties.Resources.search;
            this.btnSearch1.ImageHeight = 24;
            this.btnSearch1.ImageWidth = 24;
            this.btnSearch1.IsHighlight = true;
            this.btnSearch1.Location = new System.Drawing.Point(1071, 5);
            this.btnSearch1.Margin = new System.Windows.Forms.Padding(5);
            this.btnSearch1.Name = "btnSearch1";
            this.btnSearch1.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tlpFilter.SetRowSpan(this.btnSearch1, 2);
            this.btnSearch1.Size = new System.Drawing.Size(75, 71);
            this.btnSearch1.TabIndex = 8;
            this.btnSearch1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch1.UseVisualStyleBackColor = true;
            this.btnSearch1.Click += new System.EventHandler(this.btnSearch1_Click);
            // 
            // tbxConclusion
            // 
            this.tbxConclusion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxConclusion.Location = new System.Drawing.Point(895, 47);
            this.tbxConclusion.MaxLength = 50;
            this.tbxConclusion.Name = "tbxConclusion";
            this.tbxConclusion.Size = new System.Drawing.Size(168, 26);
            this.tbxConclusion.TabIndex = 7;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.dtpToDate, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.label9, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.dtpFromDate, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(83, 43);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(221, 35);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // dtpToDate
            // 
            this.dtpToDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpToDate.Checked = false;
            this.dtpToDate.CustomFormat = "yyyy-MM-dd";
            this.dtpToDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpToDate.Location = new System.Drawing.Point(118, 4);
            this.dtpToDate.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.ShowCheckBox = true;
            this.dtpToDate.Size = new System.Drawing.Size(100, 26);
            this.dtpToDate.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(103, 7);
            this.label9.Margin = new System.Windows.Forms.Padding(0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(15, 20);
            this.label9.TabIndex = 0;
            this.label9.Text = "-";
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpFromDate.Checked = false;
            this.dtpFromDate.CustomFormat = "yyyy-MM-dd";
            this.dtpFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFromDate.Location = new System.Drawing.Point(3, 4);
            this.dtpFromDate.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.ShowCheckBox = true;
            this.dtpFromDate.Size = new System.Drawing.Size(100, 26);
            this.dtpFromDate.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel4.Controls.Add(this.cbxAgeUnit, 3, 0);
            this.tableLayoutPanel4.Controls.Add(this.tbxToAge, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.tbxFromAge, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label10, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(359, 43);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(221, 35);
            this.tableLayoutPanel4.TabIndex = 5;
            // 
            // cbxAgeUnit
            // 
            this.cbxAgeUnit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxAgeUnit.CheckedListBox = null;
            this.cbxAgeUnit.Control = null;
            this.cbxAgeUnit.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxAgeUnit.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxAgeUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAgeUnit.FormattingEnabled = true;
            this.cbxAgeUnit.IsShown = false;
            this.cbxAgeUnit.IsUnresponsive = false;
            this.cbxAgeUnit.Location = new System.Drawing.Point(162, 6);
            this.cbxAgeUnit.Name = "cbxAgeUnit";
            this.cbxAgeUnit.PlaceHolderStr = null;
            this.cbxAgeUnit.Size = new System.Drawing.Size(56, 27);
            this.cbxAgeUnit.TabIndex = 2;
            this.cbxAgeUnit.TreeView = null;
            // 
            // tbxToAge
            // 
            this.tbxToAge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxToAge.Location = new System.Drawing.Point(87, 4);
            this.tbxToAge.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.tbxToAge.MaxLength = 20;
            this.tbxToAge.Name = "tbxToAge";
            this.tbxToAge.Size = new System.Drawing.Size(69, 26);
            this.tbxToAge.TabIndex = 1;
            // 
            // tbxFromAge
            // 
            this.tbxFromAge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxFromAge.Location = new System.Drawing.Point(3, 4);
            this.tbxFromAge.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tbxFromAge.MaxLength = 20;
            this.tbxFromAge.Name = "tbxFromAge";
            this.tbxFromAge.Size = new System.Drawing.Size(69, 26);
            this.tbxFromAge.TabIndex = 0;
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(72, 7);
            this.label10.Margin = new System.Windows.Forms.Padding(0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(15, 20);
            this.label10.TabIndex = 1;
            this.label10.Text = "-";
            // 
            // cbxSex
            // 
            this.cbxSex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxSex.CheckedListBox = null;
            this.cbxSex.Control = null;
            this.cbxSex.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxSex.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxSex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSex.FormattingEnabled = true;
            this.cbxSex.IsShown = false;
            this.cbxSex.IsUnresponsive = false;
            this.cbxSex.Location = new System.Drawing.Point(638, 9);
            this.cbxSex.Name = "cbxSex";
            this.cbxSex.PlaceHolderStr = null;
            this.cbxSex.Size = new System.Drawing.Size(168, 27);
            this.cbxSex.TabIndex = 2;
            this.cbxSex.TreeView = null;
            // 
            // cbxStatus
            // 
            this.cbxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxStatus.CheckedListBox = null;
            this.cbxStatus.Control = null;
            this.cbxStatus.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxStatus.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxStatus.FormattingEnabled = true;
            this.cbxStatus.IsShown = false;
            this.cbxStatus.IsUnresponsive = false;
            this.cbxStatus.Location = new System.Drawing.Point(638, 49);
            this.cbxStatus.Name = "cbxStatus";
            this.cbxStatus.PlaceHolderStr = null;
            this.cbxStatus.Size = new System.Drawing.Size(168, 27);
            this.cbxStatus.TabIndex = 6;
            this.cbxStatus.TreeView = null;
            // 
            // cbxCheckItem
            // 
            this.cbxCheckItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxCheckItem.CheckedListBox = null;
            this.cbxCheckItem.Control = null;
            this.cbxCheckItem.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxCheckItem.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxCheckItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCheckItem.FormattingEnabled = true;
            this.cbxCheckItem.IsShown = false;
            this.cbxCheckItem.IsUnresponsive = false;
            this.cbxCheckItem.Location = new System.Drawing.Point(895, 9);
            this.cbxCheckItem.Name = "cbxCheckItem";
            this.cbxCheckItem.PlaceHolderStr = null;
            this.cbxCheckItem.Size = new System.Drawing.Size(168, 27);
            this.cbxCheckItem.TabIndex = 3;
            this.cbxCheckItem.TreeView = null;
            // 
            // tbxCaseNo
            // 
            this.tbxCaseNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxCaseNo.Location = new System.Drawing.Point(86, 7);
            this.tbxCaseNo.MaxLength = 20;
            this.tbxCaseNo.Name = "tbxCaseNo";
            this.tbxCaseNo.Size = new System.Drawing.Size(215, 26);
            this.tbxCaseNo.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 74);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(186, 68);
            this.panel2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.tlpDataGrid);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(192, 74);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tlpDataGrid
            // 
            this.tlpDataGrid.ColumnCount = 1;
            this.tlpDataGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDataGrid.Controls.Add(this.caseList2, 0, 1);
            this.tlpDataGrid.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tlpDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDataGrid.Location = new System.Drawing.Point(0, 0);
            this.tlpDataGrid.Name = "tlpDataGrid";
            this.tlpDataGrid.RowCount = 2;
            this.tlpDataGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpDataGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDataGrid.Size = new System.Drawing.Size(192, 74);
            this.tlpDataGrid.TabIndex = 3;
            // 
            // caseList2
            // 
            this.caseList2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.caseList2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.caseList2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.caseList2.Location = new System.Drawing.Point(3, 43);
            this.caseList2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.caseList2.Name = "caseList2";
            this.caseList2.Size = new System.Drawing.Size(186, 31);
            this.caseList2.TabIndex = 4;
            this.caseList2.Usage = EcgBpAnalysis.FuncComponent.CaseList.CaseListUsage.Recycle;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.btnRestore, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnReallyDelete, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(186, 36);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // btnRestore
            // 
            this.btnRestore.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRestore.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnRestore.DataFlag = null;
            this.btnRestore.FollowBaseColor = false;
            this.btnRestore.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnRestore.IsHighlight = true;
            this.btnRestore.Location = new System.Drawing.Point(30, 5);
            this.btnRestore.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnRestore.Size = new System.Drawing.Size(72, 26);
            this.btnRestore.TabIndex = 7;
            this.btnRestore.Text = "还原";
            this.btnRestore.UseVisualStyleBackColor = true;
            // 
            // btnReallyDelete
            // 
            this.btnReallyDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnReallyDelete.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnReallyDelete.DataFlag = null;
            this.btnReallyDelete.FollowBaseColor = false;
            this.btnReallyDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnReallyDelete.IsHighlight = true;
            this.btnReallyDelete.Location = new System.Drawing.Point(111, 5);
            this.btnReallyDelete.Name = "btnReallyDelete";
            this.btnReallyDelete.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnReallyDelete.Size = new System.Drawing.Size(72, 26);
            this.btnReallyDelete.TabIndex = 8;
            this.btnReallyDelete.Text = "删除";
            this.btnReallyDelete.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.panel4);
            this.tabPage4.Controls.Add(this.tlpFilter2);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.tabPage4.Size = new System.Drawing.Size(192, 74);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.Controls.Add(this.caseList3);
            this.panel4.Controls.Add(this.panel3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 84);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(186, 0);
            this.panel4.TabIndex = 1;
            // 
            // caseList3
            // 
            this.caseList3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.caseList3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.caseList3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.caseList3.Location = new System.Drawing.Point(0, 45);
            this.caseList3.Name = "caseList3";
            this.caseList3.Size = new System.Drawing.Size(186, 0);
            this.caseList3.TabIndex = 1;
            this.caseList3.Usage = EcgBpAnalysis.FuncComponent.CaseList.CaseListUsage.Favorite;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnCancelCollect);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(186, 45);
            this.panel3.TabIndex = 2;
            // 
            // btnCancelCollect
            // 
            this.btnCancelCollect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelCollect.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnCancelCollect.DataFlag = null;
            this.btnCancelCollect.FollowBaseColor = false;
            this.btnCancelCollect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnCancelCollect.IsHighlight = true;
            this.btnCancelCollect.Location = new System.Drawing.Point(98, 11);
            this.btnCancelCollect.Name = "btnCancelCollect";
            this.btnCancelCollect.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCancelCollect.Size = new System.Drawing.Size(80, 26);
            this.btnCancelCollect.TabIndex = 9;
            this.btnCancelCollect.Text = "取消收藏";
            this.btnCancelCollect.UseVisualStyleBackColor = true;
            // 
            // tlpFilter2
            // 
            this.tlpFilter2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tlpFilter2.ColumnCount = 9;
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tlpFilter2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFilter2.Controls.Add(this.tbxName2, 3, 0);
            this.tlpFilter2.Controls.Add(this.label11, 0, 1);
            this.tlpFilter2.Controls.Add(this.label12, 0, 0);
            this.tlpFilter2.Controls.Add(this.label13, 2, 0);
            this.tlpFilter2.Controls.Add(this.label14, 4, 0);
            this.tlpFilter2.Controls.Add(this.label15, 6, 0);
            this.tlpFilter2.Controls.Add(this.label16, 2, 1);
            this.tlpFilter2.Controls.Add(this.label17, 4, 1);
            this.tlpFilter2.Controls.Add(this.label18, 6, 1);
            this.tlpFilter2.Controls.Add(this.btnSearch3, 8, 0);
            this.tlpFilter2.Controls.Add(this.tbxConclusion2, 7, 1);
            this.tlpFilter2.Controls.Add(this.tableLayoutPanel7, 1, 1);
            this.tlpFilter2.Controls.Add(this.tableLayoutPanel8, 3, 1);
            this.tlpFilter2.Controls.Add(this.cbxSex2, 5, 0);
            this.tlpFilter2.Controls.Add(this.cbxStatus2, 5, 1);
            this.tlpFilter2.Controls.Add(this.cbxCheckItem2, 7, 0);
            this.tlpFilter2.Controls.Add(this.tbxCaseNo2, 1, 0);
            this.tlpFilter2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFilter2.Location = new System.Drawing.Point(3, 3);
            this.tlpFilter2.Name = "tlpFilter2";
            this.tlpFilter2.RowCount = 2;
            this.tlpFilter2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFilter2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFilter2.Size = new System.Drawing.Size(186, 81);
            this.tlpFilter2.TabIndex = 2;
            // 
            // tbxName2
            // 
            this.tbxName2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxName2.Location = new System.Drawing.Point(92, 7);
            this.tbxName2.MaxLength = 20;
            this.tbxName2.Name = "tbxName2";
            this.tbxName2.Size = new System.Drawing.Size(1, 26);
            this.tbxName2.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(15, 50);
            this.label11.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 20);
            this.label11.TabIndex = 5;
            this.label11.Text = "检查日期";
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(29, 10);
            this.label12.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(51, 20);
            this.label12.TabIndex = 0;
            this.label12.Text = "病历号";
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(49, 10);
            this.label13.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(37, 20);
            this.label13.TabIndex = 1;
            this.label13.Text = "姓名";
            // 
            // label14
            // 
            this.label14.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(55, 10);
            this.label14.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(37, 20);
            this.label14.TabIndex = 2;
            this.label14.Text = "性别";
            // 
            // label15
            // 
            this.label15.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(72, 10);
            this.label15.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(65, 20);
            this.label15.TabIndex = 3;
            this.label15.Text = "检查项目";
            // 
            // label16
            // 
            this.label16.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(49, 50);
            this.label16.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(37, 20);
            this.label16.TabIndex = 4;
            this.label16.Text = "年龄";
            // 
            // label17
            // 
            this.label17.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(55, 50);
            this.label17.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(37, 20);
            this.label17.TabIndex = 6;
            this.label17.Text = "状态";
            // 
            // label18
            // 
            this.label18.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(72, 50);
            this.label18.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(65, 20);
            this.label18.TabIndex = 7;
            this.label18.Text = "报告结论";
            // 
            // btnSearch3
            // 
            this.btnSearch3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSearch3.DataFlag = null;
            this.btnSearch3.FollowBaseColor = false;
            this.btnSearch3.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSearch3.Image = global::EcgBpAnalysis.Properties.Resources.search;
            this.btnSearch3.ImageHeight = 24;
            this.btnSearch3.ImageWidth = 24;
            this.btnSearch3.IsHighlight = true;
            this.btnSearch3.Location = new System.Drawing.Point(107, 5);
            this.btnSearch3.Margin = new System.Windows.Forms.Padding(5);
            this.btnSearch3.Name = "btnSearch3";
            this.btnSearch3.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tlpFilter2.SetRowSpan(this.btnSearch3, 2);
            this.btnSearch3.Size = new System.Drawing.Size(75, 71);
            this.btnSearch3.TabIndex = 8;
            this.btnSearch3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch3.UseVisualStyleBackColor = true;
            // 
            // tbxConclusion2
            // 
            this.tbxConclusion2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxConclusion2.Location = new System.Drawing.Point(143, 47);
            this.tbxConclusion2.MaxLength = 50;
            this.tbxConclusion2.Name = "tbxConclusion2";
            this.tbxConclusion2.Size = new System.Drawing.Size(1, 26);
            this.tbxConclusion2.TabIndex = 7;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 3;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.dtpToDate2, 2, 0);
            this.tableLayoutPanel7.Controls.Add(this.label19, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.dtpFromDate2, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(83, 43);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(1, 35);
            this.tableLayoutPanel7.TabIndex = 4;
            // 
            // dtpToDate2
            // 
            this.dtpToDate2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpToDate2.Checked = false;
            this.dtpToDate2.CustomFormat = "yyyy-MM-dd";
            this.dtpToDate2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpToDate2.Location = new System.Drawing.Point(8, 4);
            this.dtpToDate2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.dtpToDate2.Name = "dtpToDate2";
            this.dtpToDate2.ShowCheckBox = true;
            this.dtpToDate2.Size = new System.Drawing.Size(1, 26);
            this.dtpToDate2.TabIndex = 1;
            // 
            // label19
            // 
            this.label19.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(-7, 7);
            this.label19.Margin = new System.Windows.Forms.Padding(0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(15, 20);
            this.label19.TabIndex = 0;
            this.label19.Text = "-";
            // 
            // dtpFromDate2
            // 
            this.dtpFromDate2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpFromDate2.Checked = false;
            this.dtpFromDate2.CustomFormat = "yyyy-MM-dd";
            this.dtpFromDate2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFromDate2.Location = new System.Drawing.Point(3, 4);
            this.dtpFromDate2.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.dtpFromDate2.Name = "dtpFromDate2";
            this.dtpFromDate2.ShowCheckBox = true;
            this.dtpFromDate2.Size = new System.Drawing.Size(1, 26);
            this.dtpFromDate2.TabIndex = 0;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 4;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel8.Controls.Add(this.cbxAgeUnit2, 3, 0);
            this.tableLayoutPanel8.Controls.Add(this.tbxToAge2, 2, 0);
            this.tableLayoutPanel8.Controls.Add(this.tbxFromAge2, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.label20, 1, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(89, 43);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(1, 35);
            this.tableLayoutPanel8.TabIndex = 5;
            // 
            // cbxAgeUnit2
            // 
            this.cbxAgeUnit2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxAgeUnit2.CheckedListBox = null;
            this.cbxAgeUnit2.Control = null;
            this.cbxAgeUnit2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxAgeUnit2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxAgeUnit2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAgeUnit2.FormattingEnabled = true;
            this.cbxAgeUnit2.IsShown = false;
            this.cbxAgeUnit2.IsUnresponsive = false;
            this.cbxAgeUnit2.Location = new System.Drawing.Point(10, 4);
            this.cbxAgeUnit2.Name = "cbxAgeUnit2";
            this.cbxAgeUnit2.PlaceHolderStr = null;
            this.cbxAgeUnit2.Size = new System.Drawing.Size(1, 27);
            this.cbxAgeUnit2.TabIndex = 2;
            this.cbxAgeUnit2.TreeView = null;
            // 
            // tbxToAge2
            // 
            this.tbxToAge2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxToAge2.Location = new System.Drawing.Point(11, 4);
            this.tbxToAge2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.tbxToAge2.MaxLength = 20;
            this.tbxToAge2.Name = "tbxToAge2";
            this.tbxToAge2.Size = new System.Drawing.Size(1, 26);
            this.tbxToAge2.TabIndex = 1;
            // 
            // tbxFromAge2
            // 
            this.tbxFromAge2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxFromAge2.Location = new System.Drawing.Point(3, 4);
            this.tbxFromAge2.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tbxFromAge2.MaxLength = 20;
            this.tbxFromAge2.Name = "tbxFromAge2";
            this.tbxFromAge2.Size = new System.Drawing.Size(1, 26);
            this.tbxFromAge2.TabIndex = 0;
            // 
            // label20
            // 
            this.label20.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(-4, 7);
            this.label20.Margin = new System.Windows.Forms.Padding(0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(15, 20);
            this.label20.TabIndex = 1;
            this.label20.Text = "-";
            // 
            // cbxSex2
            // 
            this.cbxSex2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxSex2.CheckedListBox = null;
            this.cbxSex2.Control = null;
            this.cbxSex2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxSex2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxSex2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSex2.FormattingEnabled = true;
            this.cbxSex2.IsShown = false;
            this.cbxSex2.IsUnresponsive = false;
            this.cbxSex2.Location = new System.Drawing.Point(98, 6);
            this.cbxSex2.Name = "cbxSex2";
            this.cbxSex2.PlaceHolderStr = null;
            this.cbxSex2.Size = new System.Drawing.Size(1, 27);
            this.cbxSex2.TabIndex = 2;
            this.cbxSex2.TreeView = null;
            // 
            // cbxStatus2
            // 
            this.cbxStatus2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxStatus2.CheckedListBox = null;
            this.cbxStatus2.Control = null;
            this.cbxStatus2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxStatus2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxStatus2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxStatus2.FormattingEnabled = true;
            this.cbxStatus2.IsShown = false;
            this.cbxStatus2.IsUnresponsive = false;
            this.cbxStatus2.Location = new System.Drawing.Point(98, 47);
            this.cbxStatus2.Name = "cbxStatus2";
            this.cbxStatus2.PlaceHolderStr = null;
            this.cbxStatus2.Size = new System.Drawing.Size(1, 27);
            this.cbxStatus2.TabIndex = 6;
            this.cbxStatus2.TreeView = null;
            // 
            // cbxCheckItem2
            // 
            this.cbxCheckItem2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxCheckItem2.CheckedListBox = null;
            this.cbxCheckItem2.Control = null;
            this.cbxCheckItem2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxCheckItem2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxCheckItem2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCheckItem2.FormattingEnabled = true;
            this.cbxCheckItem2.IsShown = false;
            this.cbxCheckItem2.IsUnresponsive = false;
            this.cbxCheckItem2.Location = new System.Drawing.Point(143, 6);
            this.cbxCheckItem2.Name = "cbxCheckItem2";
            this.cbxCheckItem2.PlaceHolderStr = null;
            this.cbxCheckItem2.Size = new System.Drawing.Size(1, 27);
            this.cbxCheckItem2.TabIndex = 3;
            this.cbxCheckItem2.TreeView = null;
            // 
            // tbxCaseNo2
            // 
            this.tbxCaseNo2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxCaseNo2.Location = new System.Drawing.Point(86, 7);
            this.tbxCaseNo2.MaxLength = 20;
            this.tbxCaseNo2.Name = "tbxCaseNo2";
            this.tbxCaseNo2.Size = new System.Drawing.Size(1, 26);
            this.tbxCaseNo2.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.tsDriveUsed});
            this.statusStrip1.Location = new System.Drawing.Point(0, 707);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1350, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(1228, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "© 2024 武汉中旗生物医疗电子有限公司版权所有    服务电话4000-400-499";
            this.toolStripStatusLabel1.ToolTipText = " ";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(83, 17);
            this.toolStripStatusLabel2.Text = "磁盘已用空间:";
            // 
            // tsDriveUsed
            // 
            this.tsDriveUsed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsDriveUsed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.tsDriveUsed.Name = "tsDriveUsed";
            this.tsDriveUsed.Size = new System.Drawing.Size(24, 17);
            this.tsDriveUsed.Text = "    ";
            this.tsDriveUsed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpMenu
            // 
            this.tlpMenu.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tlpMenu.ColumnCount = 1;
            this.tlpMenu.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMenu.Controls.Add(this.button1, 0, 0);
            this.tlpMenu.Controls.Add(this.button2, 0, 1);
            this.tlpMenu.Controls.Add(this.button3, 0, 2);
            this.tlpMenu.Controls.Add(this.button4, 0, 3);
            this.tlpMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.tlpMenu.Location = new System.Drawing.Point(0, 0);
            this.tlpMenu.Name = "tlpMenu";
            this.tlpMenu.RowCount = 5;
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpMenu.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMenu.Size = new System.Drawing.Size(184, 600);
            this.tlpMenu.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.ImageKey = "shujuwenjian.png";
            this.button1.ImageList = this.imageList1;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.button1.Size = new System.Drawing.Size(184, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "病例管理";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "shujuwenjianA.png");
            this.imageList1.Images.SetKeyName(1, "xueshujiansuo.png");
            this.imageList1.Images.SetKeyName(2, "xueshujiansuoA.png");
            this.imageList1.Images.SetKeyName(3, "huishouzhan.png");
            this.imageList1.Images.SetKeyName(4, "huishouzhanA.png");
            this.imageList1.Images.SetKeyName(5, "shoucangjia.png");
            this.imageList1.Images.SetKeyName(6, "shoucangjiaA.png");
            this.imageList1.Images.SetKeyName(7, "shujuwenjian.png");
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.ImageKey = "xueshujiansuo.png";
            this.button2.ImageList = this.imageList1;
            this.button2.Location = new System.Drawing.Point(0, 32);
            this.button2.Margin = new System.Windows.Forms.Padding(0);
            this.button2.Name = "button2";
            this.button2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.button2.Size = new System.Drawing.Size(184, 32);
            this.button2.TabIndex = 2;
            this.button2.Text = "学术检索";
            this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            // 
            // button3
            // 
            this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button3.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button3.ImageKey = "huishouzhan.png";
            this.button3.ImageList = this.imageList1;
            this.button3.Location = new System.Drawing.Point(0, 64);
            this.button3.Margin = new System.Windows.Forms.Padding(0);
            this.button3.Name = "button3";
            this.button3.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.button3.Size = new System.Drawing.Size(184, 32);
            this.button3.TabIndex = 3;
            this.button3.Text = "回收站";
            this.button3.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button4.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlLight;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button4.ImageKey = "shoucangjia.png";
            this.button4.ImageList = this.imageList1;
            this.button4.Location = new System.Drawing.Point(0, 96);
            this.button4.Margin = new System.Windows.Forms.Padding(0);
            this.button4.Name = "button4";
            this.button4.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.button4.Size = new System.Drawing.Size(184, 32);
            this.button4.TabIndex = 4;
            this.button4.Text = "收藏夹";
            this.button4.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button4.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Controls.Add(this.tlpMenu);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 107);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1350, 600);
            this.panel1.TabIndex = 4;
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.pnlTop.Controls.Add(this.picLogo);
            this.pnlTop.Controls.Add(this.tlpControls);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.pnlTop.Size = new System.Drawing.Size(1350, 30);
            this.pnlTop.TabIndex = 14;
            // 
            // picLogo
            // 
            this.picLogo.Dock = System.Windows.Forms.DockStyle.Left;
            this.picLogo.Location = new System.Drawing.Point(0, 1);
            this.picLogo.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(140, 28);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 16;
            this.picLogo.TabStop = false;
            // 
            // tlpControls
            // 
            this.tlpControls.ColumnCount = 2;
            this.tlpControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControls.Controls.Add(this.picClose, 1, 0);
            this.tlpControls.Controls.Add(this.picMin, 0, 0);
            this.tlpControls.Dock = System.Windows.Forms.DockStyle.Right;
            this.tlpControls.Location = new System.Drawing.Point(1292, 1);
            this.tlpControls.Name = "tlpControls";
            this.tlpControls.RowCount = 1;
            this.tlpControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControls.Size = new System.Drawing.Size(58, 28);
            this.tlpControls.TabIndex = 14;
            // 
            // picClose
            // 
            this.picClose.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.picClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picClose.Image = global::EcgBpAnalysis.Properties.Resources.maxNew;
            this.picClose.Location = new System.Drawing.Point(33, 5);
            this.picClose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picClose.Name = "picClose";
            this.picClose.Size = new System.Drawing.Size(20, 18);
            this.picClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picClose.TabIndex = 8;
            this.picClose.TabStop = false;
            // 
            // picMin
            // 
            this.picMin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.picMin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picMin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picMin.Image = global::EcgBpAnalysis.Properties.Resources.minNew;
            this.picMin.Location = new System.Drawing.Point(4, 5);
            this.picMin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picMin.Name = "picMin";
            this.picMin.Size = new System.Drawing.Size(20, 18);
            this.picMin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picMin.TabIndex = 13;
            this.picMin.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1350, 729);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.Text = "";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tlpFilter.ResumeLayout(false);
            this.tlpFilter.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tlpDataGrid.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.tlpFilter2.ResumeLayout(false);
            this.tlpFilter2.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel8.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tlpMenu.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tlpControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Zoncare.Component.Controls.ZqToolStrip toolStrip1;
        private Zoncare.Component.CustomToolStripButton btnRegister;
        private Zoncare.Component.CustomToolStripButton btnReadCard;
        private Zoncare.Component.CustomToolStripButton btnInit;
        private Zoncare.Component.CustomToolStripButton btnSetting;
        private Zoncare.Component.CustomToolStripButton btnHelp;
        private Zoncare.Component.CustomToolStripButton btnLogout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton ddlUser;
        private Zoncare.Component.Controls.ZqTabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TableLayoutPanel tlpMenu;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ToolStripStatusLabel tsDriveUsed;
        private EcgBpAnalysis.FuncComponent.CaseList caseList1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        public System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.TableLayoutPanel tlpControls;
        public System.Windows.Forms.PictureBox picClose;
        private System.Windows.Forms.PictureBox picMin;
        private System.Windows.Forms.TableLayoutPanel tlpDataGrid;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Zoncare.Component.Controls.RoundButton btnRestore;
        private Zoncare.Component.Controls.RoundButton btnReallyDelete;
        private EcgBpAnalysis.FuncComponent.CaseList caseList3;
        private System.Windows.Forms.TableLayoutPanel tlpFilter;
        private System.Windows.Forms.TextBox tbxName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private Zoncare.Component.Controls.RoundButton btnSearch1;
        private System.Windows.Forms.TextBox tbxConclusion;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private Zoncare.Component.Controls.ZoncareComboBox cbxAgeUnit;
        private System.Windows.Forms.TextBox tbxToAge;
        private System.Windows.Forms.TextBox tbxFromAge;
        private System.Windows.Forms.Label label10;
        private Zoncare.Component.Controls.ZoncareComboBox cbxSex;
        private Zoncare.Component.Controls.ZoncareComboBox cbxStatus;
        private Zoncare.Component.Controls.ZoncareComboBox cbxCheckItem;
        private System.Windows.Forms.TextBox tbxCaseNo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Zoncare.Component.Controls.RoundButton btnRefresh;
        private Zoncare.Component.Controls.RoundButton btnEdit;
        private Zoncare.Component.Controls.RoundButton btnDelete;
        private EcgBpAnalysis.FuncComponent.CaseList caseList2;
        private System.Windows.Forms.TableLayoutPanel tlpFilter2;
        private System.Windows.Forms.TextBox tbxName2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private Zoncare.Component.Controls.RoundButton btnSearch3;
        private System.Windows.Forms.TextBox tbxConclusion2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.DateTimePicker dtpToDate2;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.DateTimePicker dtpFromDate2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private Zoncare.Component.Controls.ZoncareComboBox cbxAgeUnit2;
        private System.Windows.Forms.TextBox tbxToAge2;
        private System.Windows.Forms.TextBox tbxFromAge2;
        private System.Windows.Forms.Label label20;
        private Zoncare.Component.Controls.ZoncareComboBox cbxSex2;
        private Zoncare.Component.Controls.ZoncareComboBox cbxStatus2;
        private Zoncare.Component.Controls.ZoncareComboBox cbxCheckItem2;
        private System.Windows.Forms.TextBox tbxCaseNo2;
        private System.Windows.Forms.Panel panel3;
        private Zoncare.Component.Controls.RoundButton btnCancelCollect;
    }
}