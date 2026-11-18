namespace SystemSettings
{
    partial class UserManagement
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserManagement));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSearch = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tbxUserName = new Zoncare.Component.Controls.TextBoxEx();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.btnAdd = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnDelete = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tlpDataGrid = new System.Windows.Forms.TableLayoutPanel();
            this.dgvData = new Zoncare.Component.Controls.DataGridViewEx();
            this.zoncarePage = new Zoncare.Component.ZoncarePage();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tlpDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 222F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 69.1412F));
            this.tableLayoutPanel1.Controls.Add(this.btnSearch, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbxUserName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(862, 47);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSearch.DataFlag = null;
            this.btnSearch.FollowBaseColor = false;
            this.btnSearch.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSearch.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSearch.IsHighlight = true;
            this.btnSearch.Location = new System.Drawing.Point(302, 8);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(6, 3, 20, 3);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSearch.Size = new System.Drawing.Size(75, 30);
            this.btnSearch.TabIndex = 4;
            this.btnSearch.Text = "搜索";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // tbxUserName
            // 
            this.tbxUserName.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxUserName.BackColor = System.Drawing.SystemColors.Window;
            this.tbxUserName.BorderColor = System.Drawing.Color.Empty;
            this.tbxUserName.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxUserName.BorderSize = 1;
            this.tbxUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxUserName.CornerRadius = 2;
            this.tbxUserName.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxUserName.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.tbxUserName.Location = new System.Drawing.Point(77, 9);
            this.tbxUserName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxUserName.MaxLength = 32767;
            this.tbxUserName.Multiline = false;
            this.tbxUserName.Name = "tbxUserName";
            this.tbxUserName.PasswordChar = '\0';
            this.tbxUserName.PlaceHolderStr = null;
            this.tbxUserName.ReadOnly = false;
            this.tbxUserName.SelectionStart = 0;
            this.tbxUserName.Size = new System.Drawing.Size(216, 28);
            this.tbxUserName.TabIndex = 2;
            this.tbxUserName.TabStop = false;
            this.tbxUserName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxUserName.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 13);
            this.label1.Margin = new System.Windows.Forms.Padding(20, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "用户名";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel5.ColumnCount = 4;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.btnAdd, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.btnDelete, 3, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 47);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(3, 3, 3, 1);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(862, 40);
            this.tableLayoutPanel5.TabIndex = 5;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnAdd.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnAdd.DataFlag = null;
            this.btnAdd.FollowBaseColor = false;
            this.btnAdd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnAdd.IsHighlight = true;
            this.btnAdd.Location = new System.Drawing.Point(10, 7);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(10, 3, 6, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnAdd.Size = new System.Drawing.Size(72, 26);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.Text = "新增";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDelete.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnDelete.DataFlag = null;
            this.btnDelete.FollowBaseColor = false;
            this.btnDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnDelete.IsHighlight = true;
            this.btnDelete.Location = new System.Drawing.Point(770, 7);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDelete.Size = new System.Drawing.Size(72, 26);
            this.btnDelete.TabIndex = 8;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // tlpDataGrid
            // 
            this.tlpDataGrid.ColumnCount = 1;
            this.tlpDataGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDataGrid.Controls.Add(this.dgvData, 0, 0);
            this.tlpDataGrid.Controls.Add(this.zoncarePage, 0, 1);
            this.tlpDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDataGrid.Location = new System.Drawing.Point(0, 87);
            this.tlpDataGrid.Name = "tlpDataGrid";
            this.tlpDataGrid.RowCount = 2;
            this.tlpDataGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDataGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpDataGrid.Size = new System.Drawing.Size(862, 481);
            this.tlpDataGrid.TabIndex = 6;
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
            this.dgvData.Location = new System.Drawing.Point(3, 3);
            this.dgvData.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvData.MergeColumnNames")));
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 30;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.ShowSelectionColor = true;
            this.dgvData.Size = new System.Drawing.Size(856, 439);
            this.dgvData.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvData.TabIndex = 1;
            this.dgvData.RowAction_ImageButtonClick += new Zoncare.Component.Controls.DataGridViewEx.RowDataActionEventHandler(this.dgvData_RowAction_ImageButtonClick);
            this.dgvData.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_CellMouseClick);
            // 
            // zoncarePage
            // 
            this.zoncarePage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(113)))), ((int)(((byte)(191)))));
            this.zoncarePage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoncarePage.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.zoncarePage.Langs = null;
            this.zoncarePage.Location = new System.Drawing.Point(0, 448);
            this.zoncarePage.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.zoncarePage.Name = "zoncarePage";
            this.zoncarePage.OriginalData = null;
            this.zoncarePage.RecordCount = 0;
            this.zoncarePage.Size = new System.Drawing.Size(862, 33);
            this.zoncarePage.TabIndex = 2;
            // 
            // UserManagement
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tlpDataGrid);
            this.Controls.Add(this.tableLayoutPanel5);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "UserManagement";
            this.Size = new System.Drawing.Size(862, 568);
            this.Load += new System.EventHandler(this.UserManagement_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tlpDataGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Zoncare.Component.Controls.RoundButton btnAdd;
        private Zoncare.Component.Controls.RoundButton btnDelete;
        private System.Windows.Forms.TableLayoutPanel tlpDataGrid;
        private Zoncare.Component.Controls.DataGridViewEx dgvData;
        private Zoncare.Component.ZoncarePage zoncarePage;
        private Zoncare.Component.Controls.TextBoxEx tbxUserName;
        private System.Windows.Forms.Label label1;
        private Zoncare.Component.Controls.RoundButton btnSearch;
    }
}
