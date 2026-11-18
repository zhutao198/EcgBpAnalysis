namespace AnalysisApp.CommonControls
{
    partial class BpCompareControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BpCompareControl));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.nameList = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.bpTrendWaveControl1 = new AnalysisApp.CommonControls.BpTrendWaveControl();
            this.dgvBP = new Zoncare.Component.Controls.DataGridViewEx();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBP)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.nameList, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.dgvBP, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(760, 552);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // nameList
            // 
            this.nameList.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nameList.CheckedListBox = null;
            this.nameList.Control = null;
            this.nameList.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.nameList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.nameList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nameList.FormattingEnabled = true;
            this.nameList.IsShown = false;
            this.nameList.IsUnresponsive = false;
            this.nameList.Location = new System.Drawing.Point(3, 6);
            this.nameList.Name = "nameList";
            this.nameList.PlaceHolderStr = null;
            this.nameList.Size = new System.Drawing.Size(249, 27);
            this.nameList.TabIndex = 8;
            this.nameList.TreeView = null;
            this.nameList.SelectedIndexChanged += new System.EventHandler(this.nameList_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.bpTrendWaveControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 193);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(754, 359);
            this.panel1.TabIndex = 9;
            // 
            // bpTrendWaveControl1
            // 
            this.bpTrendWaveControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bpTrendWaveControl1.IsCompare = true;
            this.bpTrendWaveControl1.Location = new System.Drawing.Point(0, 0);
            this.bpTrendWaveControl1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.bpTrendWaveControl1.Name = "bpTrendWaveControl1";
            this.bpTrendWaveControl1.Size = new System.Drawing.Size(752, 357);
            this.bpTrendWaveControl1.TabIndex = 0;
            // 
            // dgvBP
            // 
            this.dgvBP.ActionColumnHeaderText = null;
            this.dgvBP.ActionColumnWidth = 0;
            this.dgvBP.AllowUserToAddRows = false;
            this.dgvBP.AllowUserToDeleteRows = false;
            this.dgvBP.AllowUserToResizeRows = false;
            this.dgvBP.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBP.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvBP.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvBP.ColumnHeadersHeight = 36;
            this.dgvBP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvBP.CustomDateTimeFormat = null;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Lavender;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvBP.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvBP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBP.EnableHeadersVisualStyles = false;
            this.dgvBP.GridColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.dgvBP.Location = new System.Drawing.Point(3, 43);
            this.dgvBP.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvBP.MergeColumnNames")));
            this.dgvBP.Name = "dgvBP";
            this.dgvBP.RowHeadersVisible = false;
            this.dgvBP.RowTemplate.Height = 30;
            this.dgvBP.Size = new System.Drawing.Size(754, 144);
            this.dgvBP.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvBP.TabIndex = 4;
            // 
            // BpCompareControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "BpCompareControl";
            this.Size = new System.Drawing.Size(760, 552);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBP)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Zoncare.Component.Controls.ZoncareComboBox nameList;
        private System.Windows.Forms.Panel panel1;
        private BpTrendWaveControl bpTrendWaveControl1;
        private Zoncare.Component.Controls.DataGridViewEx dgvBP;
    }
}
