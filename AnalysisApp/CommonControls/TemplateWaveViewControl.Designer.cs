namespace AnalysisApp.CommonControls
{
    partial class TemplateWaveViewControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.leadSet = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblLead = new System.Windows.Forms.Label();
            this.lblResort = new System.Windows.Forms.Label();
            this.cmbLayout = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cmbGain = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cmbLead = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cmbResort = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.btnCommonWave = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnSelectCurPage = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnSelectAll = new Zoncare.Component.Controls.RoundButton(this.components);
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.leadSet)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1261, 658);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 16;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.leadSet, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblLead, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblResort, 8, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbLayout, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbGain, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbLead, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbResort, 9, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCommonWave, 15, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSelectCurPage, 13, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSelectAll, 14, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblPageInfo, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1255, 37);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(123, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "增益";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // leadSet
            // 
            this.leadSet.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.leadSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(222)))), ((int)(((byte)(235)))));
            this.leadSet.ErrorImage = null;
            this.leadSet.Image = global::AnalysisApp.Properties.Resources.templateLeadSet;
            this.leadSet.InitialImage = null;
            this.leadSet.Location = new System.Drawing.Point(545, 6);
            this.leadSet.Margin = new System.Windows.Forms.Padding(0);
            this.leadSet.Name = "leadSet";
            this.leadSet.Padding = new System.Windows.Forms.Padding(3);
            this.leadSet.Size = new System.Drawing.Size(24, 24);
            this.leadSet.TabIndex = 2;
            this.leadSet.TabStop = false;
            this.leadSet.Click += new System.EventHandler(this.leadSet_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(278, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "布局";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLead
            // 
            this.lblLead.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLead.AutoSize = true;
            this.lblLead.Location = new System.Drawing.Point(433, 12);
            this.lblLead.Name = "lblLead";
            this.lblLead.Size = new System.Drawing.Size(29, 12);
            this.lblLead.TabIndex = 0;
            this.lblLead.Text = "导联";
            this.lblLead.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblResort
            // 
            this.lblResort.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblResort.AutoSize = true;
            this.lblResort.Location = new System.Drawing.Point(572, 12);
            this.lblResort.Name = "lblResort";
            this.lblResort.Size = new System.Drawing.Size(41, 12);
            this.lblResort.TabIndex = 0;
            this.lblResort.Text = "重排序";
            this.lblResort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.cmbLayout.Location = new System.Drawing.Point(313, 7);
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
            this.cmbGain.Location = new System.Drawing.Point(158, 7);
            this.cmbGain.Name = "cmbGain";
            this.cmbGain.PlaceHolderStr = null;
            this.cmbGain.Size = new System.Drawing.Size(114, 22);
            this.cmbGain.TabIndex = 4;
            this.cmbGain.TreeView = null;
            // 
            // cmbLead
            // 
            this.cmbLead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLead.CheckedListBox = null;
            this.cmbLead.Control = null;
            this.cmbLead.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cmbLead.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbLead.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLead.FormattingEnabled = true;
            this.cmbLead.IsShown = false;
            this.cmbLead.IsUnresponsive = false;
            this.cmbLead.Location = new System.Drawing.Point(468, 7);
            this.cmbLead.Name = "cmbLead";
            this.cmbLead.PlaceHolderStr = null;
            this.cmbLead.Size = new System.Drawing.Size(74, 22);
            this.cmbLead.TabIndex = 4;
            this.cmbLead.TreeView = null;
            // 
            // cmbResort
            // 
            this.cmbResort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbResort.CheckedListBox = null;
            this.cmbResort.Control = null;
            this.cmbResort.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cmbResort.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbResort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbResort.FormattingEnabled = true;
            this.cmbResort.IsShown = false;
            this.cmbResort.IsUnresponsive = false;
            this.cmbResort.Location = new System.Drawing.Point(619, 7);
            this.cmbResort.Name = "cmbResort";
            this.cmbResort.PlaceHolderStr = null;
            this.cmbResort.Size = new System.Drawing.Size(74, 22);
            this.cmbResort.TabIndex = 4;
            this.cmbResort.TreeView = null;
            // 
            // btnCommonWave
            // 
            this.btnCommonWave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCommonWave.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnCommonWave.DataFlag = null;
            this.btnCommonWave.FollowBaseColor = false;
            this.btnCommonWave.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnCommonWave.Image = global::AnalysisApp.Properties.Resources.showThreeWave;
            this.btnCommonWave.IsHighlight = true;
            this.btnCommonWave.Location = new System.Drawing.Point(1227, 7);
            this.btnCommonWave.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnCommonWave.Name = "btnCommonWave";
            this.btnCommonWave.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCommonWave.Size = new System.Drawing.Size(28, 23);
            this.btnCommonWave.TabIndex = 10;
            this.btnCommonWave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCommonWave.UseVisualStyleBackColor = true;
            this.btnCommonWave.Click += new System.EventHandler(this.btnCommonWave_Click);
            // 
            // btnSelectCurPage
            // 
            this.btnSelectCurPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectCurPage.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnSelectCurPage.DataFlag = null;
            this.btnSelectCurPage.FollowBaseColor = false;
            this.btnSelectCurPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnSelectCurPage.IsHighlight = true;
            this.btnSelectCurPage.Location = new System.Drawing.Point(1137, 6);
            this.btnSelectCurPage.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnSelectCurPage.Name = "btnSelectCurPage";
            this.btnSelectCurPage.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSelectCurPage.Size = new System.Drawing.Size(48, 24);
            this.btnSelectCurPage.TabIndex = 11;
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
            this.btnSelectAll.Location = new System.Drawing.Point(1188, 6);
            this.btnSelectAll.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSelectAll.Size = new System.Drawing.Size(36, 24);
            this.btnSelectAll.TabIndex = 12;
            this.btnSelectAll.Text = "全选";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(149)))), ((int)(((byte)(197)))));
            this.lblPageInfo.Location = new System.Drawing.Point(3, 12);
            this.lblPageInfo.Margin = new System.Windows.Forms.Padding(3, 0, 10, 0);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(41, 12);
            this.lblPageInfo.TabIndex = 5;
            this.lblPageInfo.Text = "label4";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.vScrollBar1, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 40);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1255, 615);
            this.tableLayoutPanel3.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1237, 609);
            this.panel1.TabIndex = 0;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vScrollBar1.Location = new System.Drawing.Point(1243, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(12, 615);
            this.vScrollBar1.TabIndex = 1;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // TemplateWaveViewControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TemplateWaveViewControl";
            this.Size = new System.Drawing.Size(1261, 658);
            this.Load += new System.EventHandler(this.TemplateWaveViewControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.leadSet)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox leadSet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblLead;
        private System.Windows.Forms.Label lblResort;
        private Zoncare.Component.Controls.ZoncareComboBox cmbGain;
        private Zoncare.Component.Controls.ZoncareComboBox cmbLayout;
        private Zoncare.Component.Controls.ZoncareComboBox cmbLead;
        private Zoncare.Component.Controls.ZoncareComboBox cmbResort;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private Zoncare.Component.Controls.RoundButton btnCommonWave;
        private Zoncare.Component.Controls.RoundButton btnSelectCurPage;
        private Zoncare.Component.Controls.RoundButton btnSelectAll;
    }
}
