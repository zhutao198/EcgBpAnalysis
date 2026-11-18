namespace AnalysisApp.CommonControls
{
    partial class pageScanWave
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.vScrollBarTime = new System.Windows.Forms.VScrollBar();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cmbGain = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblLead = new System.Windows.Forms.Label();
            this.leadSet = new System.Windows.Forms.PictureBox();
            this.cmbTimes = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cmbLayout = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.cmbLead = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.btnCommonWave = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.leadSet)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.vScrollBarTime, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1822, 1280);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 45);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1802, 1235);
            this.panel1.TabIndex = 4;
            // 
            // vScrollBarTime
            // 
            this.vScrollBarTime.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBarTime.Location = new System.Drawing.Point(1810, 45);
            this.vScrollBarTime.Name = "vScrollBarTime";
            this.vScrollBarTime.Size = new System.Drawing.Size(12, 1235);
            this.vScrollBarTime.TabIndex = 6;
            this.vScrollBarTime.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBarTime_Scroll);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 11;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 2);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 124F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.cmbGain, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblLead, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this.leadSet, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbTimes, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbLayout, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.cmbLead, 8, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCommonWave, 10, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 5);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1816, 35);
            this.tableLayoutPanel2.TabIndex = 7;
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
            this.cmbGain.Location = new System.Drawing.Point(46, 6);
            this.cmbGain.Name = "cmbGain";
            this.cmbGain.PlaceHolderStr = null;
            this.cmbGain.Size = new System.Drawing.Size(154, 27);
            this.cmbGain.TabIndex = 6;
            this.cmbGain.TreeView = null;
            this.cmbGain.SelectedIndexChanged += new System.EventHandler(this.cmbGain_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "增益";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(206, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "时长";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(409, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "布局";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLead
            // 
            this.lblLead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLead.AutoSize = true;
            this.lblLead.Location = new System.Drawing.Point(636, 7);
            this.lblLead.Name = "lblLead";
            this.lblLead.Size = new System.Drawing.Size(37, 20);
            this.lblLead.TabIndex = 0;
            this.lblLead.Text = "导联";
            this.lblLead.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // leadSet
            // 
            this.leadSet.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.leadSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(222)))), ((int)(((byte)(235)))));
            this.leadSet.ErrorImage = null;
            this.leadSet.Image = global::AnalysisApp.Properties.Resources.templateLeadSet;
            this.leadSet.InitialImage = null;
            this.leadSet.Location = new System.Drawing.Point(609, 5);
            this.leadSet.Margin = new System.Windows.Forms.Padding(0);
            this.leadSet.Name = "leadSet";
            this.leadSet.Padding = new System.Windows.Forms.Padding(3);
            this.leadSet.Size = new System.Drawing.Size(24, 24);
            this.leadSet.TabIndex = 2;
            this.leadSet.TabStop = false;
            this.leadSet.Click += new System.EventHandler(this.leadSet_Click);
            // 
            // cmbTimes
            // 
            this.cmbTimes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTimes.CheckedListBox = null;
            this.cmbTimes.Control = null;
            this.cmbTimes.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cmbTimes.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbTimes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTimes.FormattingEnabled = true;
            this.cmbTimes.IsShown = false;
            this.cmbTimes.IsUnresponsive = false;
            this.cmbTimes.Location = new System.Drawing.Point(249, 6);
            this.cmbTimes.Name = "cmbTimes";
            this.cmbTimes.PlaceHolderStr = null;
            this.cmbTimes.Size = new System.Drawing.Size(154, 27);
            this.cmbTimes.TabIndex = 6;
            this.cmbTimes.TreeView = null;
            this.cmbTimes.SelectedIndexChanged += new System.EventHandler(this.cmbTimes_SelectedIndexChanged);
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
            this.cmbLayout.Location = new System.Drawing.Point(452, 6);
            this.cmbLayout.Name = "cmbLayout";
            this.cmbLayout.PlaceHolderStr = null;
            this.cmbLayout.Size = new System.Drawing.Size(154, 27);
            this.cmbLayout.TabIndex = 6;
            this.cmbLayout.TreeView = null;
            this.cmbLayout.SelectedIndexChanged += new System.EventHandler(this.cmbLayout_SelectedIndexChanged);
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
            this.cmbLead.Location = new System.Drawing.Point(679, 6);
            this.cmbLead.Name = "cmbLead";
            this.cmbLead.PlaceHolderStr = null;
            this.cmbLead.Size = new System.Drawing.Size(118, 27);
            this.cmbLead.TabIndex = 6;
            this.cmbLead.TreeView = null;
            this.cmbLead.SelectedIndexChanged += new System.EventHandler(this.cmbLead_SelectedIndexChanged);
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
            this.btnCommonWave.Location = new System.Drawing.Point(1788, 6);
            this.btnCommonWave.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnCommonWave.Name = "btnCommonWave";
            this.btnCommonWave.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCommonWave.Size = new System.Drawing.Size(28, 23);
            this.btnCommonWave.TabIndex = 11;
            this.btnCommonWave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCommonWave.UseVisualStyleBackColor = true;
            this.btnCommonWave.Click += new System.EventHandler(this.btnCommonWave_Click);
            // 
            // pageScanWave
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "pageScanWave";
            this.Size = new System.Drawing.Size(1822, 1280);
            this.Load += new System.EventHandler(this.pageScanWave_Load);
            this.SizeChanged += new System.EventHandler(this.pageScanWave_SizeChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.leadSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.VScrollBar vScrollBarTime;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblLead;
        private System.Windows.Forms.PictureBox leadSet;
        private Zoncare.Component.Controls.ZoncareComboBox cmbGain;
        private Zoncare.Component.Controls.ZoncareComboBox cmbTimes;
        private Zoncare.Component.Controls.ZoncareComboBox cmbLayout;
        private Zoncare.Component.Controls.ZoncareComboBox cmbLead;
        private Zoncare.Component.Controls.RoundButton btnCommonWave;
    }
}
