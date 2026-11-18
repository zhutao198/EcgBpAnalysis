namespace EcgBpAnalysis.FuncComponent
{
    partial class CaseList
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
            _refreshTimer?.Stop();
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CaseList));
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
            this.btnSearch = new Zoncare.Component.Controls.RoundButton(this.components);
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
            this.tlpDataGrid = new System.Windows.Forms.TableLayoutPanel();
            this.dgvData = new Zoncare.Component.Controls.DataGridViewEx();
            this.zoncarePage = new Zoncare.Component.ZoncarePage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnRefresh = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnEdit = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnDelete = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnCancelFavorite = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tlpFilter.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tlpDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
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
            this.tlpFilter.Controls.Add(this.btnSearch, 8, 0);
            this.tlpFilter.Controls.Add(this.tbxConclusion, 7, 1);
            this.tlpFilter.Controls.Add(this.tableLayoutPanel2, 1, 1);
            this.tlpFilter.Controls.Add(this.tableLayoutPanel4, 3, 1);
            this.tlpFilter.Controls.Add(this.cbxSex, 5, 0);
            this.tlpFilter.Controls.Add(this.cbxStatus, 5, 1);
            this.tlpFilter.Controls.Add(this.cbxCheckItem, 7, 0);
            this.tlpFilter.Controls.Add(this.tbxCaseNo, 1, 0);
            this.tlpFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFilter.Location = new System.Drawing.Point(0, 0);
            this.tlpFilter.Name = "tlpFilter";
            this.tlpFilter.RowCount = 2;
            this.tlpFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFilter.Size = new System.Drawing.Size(1152, 81);
            this.tlpFilter.TabIndex = 0;
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
            // btnSearch
            // 
            this.btnSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSearch.DataFlag = null;
            this.btnSearch.FollowBaseColor = false;
            this.btnSearch.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSearch.Image = global::EcgBpAnalysis.Properties.Resources.search;
            this.btnSearch.ImageHeight = 24;
            this.btnSearch.ImageWidth = 24;
            this.btnSearch.IsHighlight = true;
            this.btnSearch.Location = new System.Drawing.Point(1071, 5);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(5);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tlpFilter.SetRowSpan(this.btnSearch, 2);
            this.btnSearch.Size = new System.Drawing.Size(75, 71);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
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
            // tlpDataGrid
            // 
            this.tlpDataGrid.ColumnCount = 1;
            this.tlpDataGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDataGrid.Controls.Add(this.dgvData, 0, 1);
            this.tlpDataGrid.Controls.Add(this.zoncarePage, 0, 2);
            this.tlpDataGrid.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tlpDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDataGrid.Location = new System.Drawing.Point(0, 81);
            this.tlpDataGrid.Name = "tlpDataGrid";
            this.tlpDataGrid.RowCount = 3;
            this.tlpDataGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpDataGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDataGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpDataGrid.Size = new System.Drawing.Size(1152, 507);
            this.tlpDataGrid.TabIndex = 2;
            // 
            // dgvData
            // 
            this.dgvData.ActionColumnHeaderText = null;
            this.dgvData.ActionColumnWidth = 0;
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            this.dgvData.AlternateRowColor = true;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvData.BackgroundColor = System.Drawing.Color.White;
            this.dgvData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvData.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvData.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.ColumnHeadersHeight = 36;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvData.CustomDateTimeFormat = null;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Lavender;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.EnableHeadersVisualStyles = false;
            this.dgvData.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgvData.HighlightRowHover = true;
            this.dgvData.Location = new System.Drawing.Point(3, 47);
            this.dgvData.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvData.MergeColumnNames")));
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 30;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.ShowSelectionColor = true;
            this.dgvData.Size = new System.Drawing.Size(1146, 415);
            this.dgvData.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvData.TabIndex = 1;
            this.dgvData.RowAction_ImageButtonClick += new Zoncare.Component.Controls.DataGridViewEx.RowDataActionEventHandler(this.dgvData_RowAction_ImageButtonClick);
            this.dgvData.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvData_CellFormatting);
            this.dgvData.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_CellMouseClick);
            this.dgvData.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_CellMouseDoubleClick);
            this.dgvData.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_CellMouseUp);
            this.dgvData.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvData_CellPainting);
            this.dgvData.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_ColumnHeaderMouseClick);
            this.dgvData.SelectionChanged += new System.EventHandler(this.dgvData_SelectionChanged);
            // 
            // zoncarePage
            // 
            this.zoncarePage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(113)))), ((int)(((byte)(191)))));
            this.zoncarePage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoncarePage.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.zoncarePage.Langs = null;
            this.zoncarePage.Location = new System.Drawing.Point(3, 468);
            this.zoncarePage.Name = "zoncarePage";
            this.zoncarePage.OriginalData = null;
            this.zoncarePage.RecordCount = 0;
            this.zoncarePage.Size = new System.Drawing.Size(1146, 36);
            this.zoncarePage.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.btnRefresh, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnEdit, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnDelete, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnCancelFavorite, 4, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1146, 40);
            this.tableLayoutPanel1.TabIndex = 3;
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
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnEdit.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnEdit.DataFlag = null;
            this.btnEdit.FollowBaseColor = false;
            this.btnEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnEdit.IsHighlight = true;
            this.btnEdit.Location = new System.Drawing.Point(900, 7);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnEdit.Size = new System.Drawing.Size(72, 26);
            this.btnEdit.TabIndex = 7;
            this.btnEdit.Text = "编辑";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDelete.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnDelete.DataFlag = null;
            this.btnDelete.FollowBaseColor = false;
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnDelete.IsHighlight = true;
            this.btnDelete.Location = new System.Drawing.Point(981, 7);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDelete.Size = new System.Drawing.Size(72, 26);
            this.btnDelete.TabIndex = 8;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnCancelFavorite
            // 
            this.btnCancelFavorite.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCancelFavorite.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnCancelFavorite.DataFlag = null;
            this.btnCancelFavorite.FollowBaseColor = false;
            this.btnCancelFavorite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnCancelFavorite.IsHighlight = true;
            this.btnCancelFavorite.Location = new System.Drawing.Point(1059, 7);
            this.btnCancelFavorite.Name = "btnCancelFavorite";
            this.btnCancelFavorite.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCancelFavorite.Size = new System.Drawing.Size(84, 26);
            this.btnCancelFavorite.TabIndex = 9;
            this.btnCancelFavorite.Text = "取消收藏";
            this.btnCancelFavorite.UseVisualStyleBackColor = true;
            this.btnCancelFavorite.Visible = false;
            // 
            // CaseList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.Controls.Add(this.tlpDataGrid);
            this.Controls.Add(this.tlpFilter);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "CaseList";
            this.Size = new System.Drawing.Size(1152, 588);
            this.Load += new System.EventHandler(this.CaseList_Load);
            this.tlpFilter.ResumeLayout(false);
            this.tlpFilter.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tlpDataGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private Zoncare.Component.Controls.RoundButton btnSearch;
        private System.Windows.Forms.TextBox tbxName;
        private System.Windows.Forms.TextBox tbxCaseNo;
        private System.Windows.Forms.TextBox tbxConclusion;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private Zoncare.Component.Controls.DataGridViewEx dgvData;
        private System.Windows.Forms.TableLayoutPanel tlpDataGrid;
        private Zoncare.Component.ZoncarePage zoncarePage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TextBox tbxToAge;
        private System.Windows.Forms.TextBox tbxFromAge;
        private System.Windows.Forms.Label label10;
        private Zoncare.Component.Controls.ZoncareComboBox cbxAgeUnit;
        private Zoncare.Component.Controls.ZoncareComboBox cbxSex;
        private Zoncare.Component.Controls.ZoncareComboBox cbxStatus;
        private Zoncare.Component.Controls.ZoncareComboBox cbxCheckItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Zoncare.Component.Controls.RoundButton btnEdit;
        private Zoncare.Component.Controls.RoundButton btnDelete;
        private Zoncare.Component.Controls.RoundButton btnCancelFavorite;
        private Zoncare.Component.Controls.RoundButton btnRefresh;
    }
}
