namespace AnalysisApp.CommonControls
{
    partial class EventWaveViewControl
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
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbLayout = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cmbGain = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.btnSelectCurPage = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnSelectAll = new Zoncare.Component.Controls.RoundButton(this.components);
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.btnDelete = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnDeletePage = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnDeleteAll = new Zoncare.Component.Controls.RoundButton(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1046, 649);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.vScrollBar1, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 40);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1046, 609);
            this.tableLayoutPanel5.TabIndex = 10;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vScrollBar1.Location = new System.Drawing.Point(1034, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(12, 609);
            this.vScrollBar1.TabIndex = 0;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1028, 603);
            this.panel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 13;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbLayout, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbGain, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSelectCurPage, 10, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSelectAll, 11, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblPageInfo, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnDelete, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnDeletePage, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnDeleteAll, 8, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel2, 9, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1046, 40);
            this.tableLayoutPanel2.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "增益";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(212, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "布局";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbLayout
            // 
            this.cmbLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLayout.CheckedListBox = null;
            this.cmbLayout.Control = null;
            this.cmbLayout.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cmbLayout.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLayout.FormattingEnabled = true;
            this.cmbLayout.IsShown = false;
            this.cmbLayout.IsUnresponsive = false;
            this.cmbLayout.Location = new System.Drawing.Point(247, 9);
            this.cmbLayout.Name = "cmbLayout";
            this.cmbLayout.PlaceHolderStr = null;
            this.cmbLayout.Size = new System.Drawing.Size(114, 22);
            this.cmbLayout.TabIndex = 4;
            this.cmbLayout.TreeView = null;
            // 
            // cmbGain
            // 
            this.cmbGain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbGain.CheckedListBox = null;
            this.cmbGain.Control = null;
            this.cmbGain.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cmbGain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGain.FormattingEnabled = true;
            this.cmbGain.IsShown = false;
            this.cmbGain.IsUnresponsive = false;
            this.cmbGain.Location = new System.Drawing.Point(92, 9);
            this.cmbGain.Name = "cmbGain";
            this.cmbGain.PlaceHolderStr = null;
            this.cmbGain.Size = new System.Drawing.Size(114, 22);
            this.cmbGain.TabIndex = 4;
            this.cmbGain.TreeView = null;
            // 
            // btnSelectCurPage
            // 
            this.btnSelectCurPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectCurPage.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnSelectCurPage.DataFlag = null;
            this.btnSelectCurPage.FollowBaseColor = false;
            this.btnSelectCurPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnSelectCurPage.IsHighlight = true;
            this.btnSelectCurPage.Location = new System.Drawing.Point(935, 7);
            this.btnSelectCurPage.Name = "btnSelectCurPage";
            this.btnSelectCurPage.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSelectCurPage.Size = new System.Drawing.Size(50, 26);
            this.btnSelectCurPage.TabIndex = 12;
            this.btnSelectCurPage.Text = "当前页";
            this.btnSelectCurPage.UseVisualStyleBackColor = true;
            this.btnSelectCurPage.Click += new System.EventHandler(this.btnSelectCurPage_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectAll.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnSelectAll.DataFlag = null;
            this.btnSelectAll.FollowBaseColor = false;
            this.btnSelectAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnSelectAll.IsHighlight = true;
            this.btnSelectAll.Location = new System.Drawing.Point(991, 7);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSelectAll.Size = new System.Drawing.Size(40, 26);
            this.btnSelectAll.TabIndex = 13;
            this.btnSelectAll.Text = "全选";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(149)))), ((int)(((byte)(197)))));
            this.lblPageInfo.Location = new System.Drawing.Point(3, 14);
            this.lblPageInfo.Margin = new System.Windows.Forms.Padding(3, 0, 10, 0);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(41, 12);
            this.lblPageInfo.TabIndex = 5;
            this.lblPageInfo.Text = "label4";
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDelete.BackColor = System.Drawing.Color.White;
            this.btnDelete.DataFlag = null;
            this.btnDelete.FollowBaseColor = false;
            this.btnDelete.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnDelete.IsHighlight = true;
            this.btnDelete.Location = new System.Drawing.Point(693, 7);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDelete.Size = new System.Drawing.Size(56, 26);
            this.btnDelete.TabIndex = 14;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnDeletePage
            // 
            this.btnDeletePage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDeletePage.BackColor = System.Drawing.Color.White;
            this.btnDeletePage.DataFlag = null;
            this.btnDeletePage.FollowBaseColor = false;
            this.btnDeletePage.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnDeletePage.IsHighlight = true;
            this.btnDeletePage.Location = new System.Drawing.Point(755, 7);
            this.btnDeletePage.Name = "btnDeletePage";
            this.btnDeletePage.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDeletePage.Size = new System.Drawing.Size(74, 26);
            this.btnDeletePage.TabIndex = 14;
            this.btnDeletePage.Text = "删除本页";
            this.btnDeletePage.UseVisualStyleBackColor = false;
            this.btnDeletePage.Visible = false;
            this.btnDeletePage.Click += new System.EventHandler(this.btnDeletePage_Click);
            // 
            // btnDeleteAll
            // 
            this.btnDeleteAll.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDeleteAll.BackColor = System.Drawing.Color.White;
            this.btnDeleteAll.DataFlag = null;
            this.btnDeleteAll.FollowBaseColor = false;
            this.btnDeleteAll.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnDeleteAll.IsHighlight = true;
            this.btnDeleteAll.Location = new System.Drawing.Point(835, 7);
            this.btnDeleteAll.Name = "btnDeleteAll";
            this.btnDeleteAll.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDeleteAll.Size = new System.Drawing.Size(74, 26);
            this.btnDeleteAll.TabIndex = 14;
            this.btnDeleteAll.Text = "删除所有";
            this.btnDeleteAll.UseVisualStyleBackColor = false;
            this.btnDeleteAll.Visible = false;
            this.btnDeleteAll.Click += new System.EventHandler(this.btnDeleteAll_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panel2.BackColor = System.Drawing.Color.Silver;
            this.panel2.Location = new System.Drawing.Point(921, 10);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1, 20);
            this.panel2.TabIndex = 15;
            // 
            // EventWaveViewControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EventWaveViewControl";
            this.Size = new System.Drawing.Size(1046, 649);
            this.Load += new System.EventHandler(this.EventWaveViewControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private Zoncare.Component.Controls.ZoncareComboBox cmbLayout;
        private Zoncare.Component.Controls.ZoncareComboBox cmbGain;
        private System.Windows.Forms.Label lblPageInfo;
        private Zoncare.Component.Controls.RoundButton btnSelectCurPage;
        private Zoncare.Component.Controls.RoundButton btnSelectAll;
        private Zoncare.Component.Controls.RoundButton btnDelete;
        private Zoncare.Component.Controls.RoundButton btnDeletePage;
        private Zoncare.Component.Controls.RoundButton btnDeleteAll;
        private System.Windows.Forms.Panel panel2;
    }
}
