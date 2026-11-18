namespace AnalysisApp.CommonControls
{
    partial class BpPieChatControl
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.radioAll = new System.Windows.Forms.RadioButton();
            this.radioDay = new System.Windows.Forms.RadioButton();
            this.radioNight = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayout3 = new System.Windows.Forms.TableLayoutPanel();
            this.txtPulseMax = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPulseMin = new System.Windows.Forms.NumericUpDown();
            this.tableLayout2 = new System.Windows.Forms.TableLayoutPanel();
            this.txtDBPMax = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDBPMin = new System.Windows.Forms.NumericUpDown();
            this.tableLayout1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtSBPMax = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSBPMin = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayout3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPulseMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPulseMin)).BeginInit();
            this.tableLayout2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDBPMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDBPMin)).BeginInit();
            this.tableLayout1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSBPMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSBPMin)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(952, 595);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.radioAll, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.radioDay, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.radioNight, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(946, 34);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // radioAll
            // 
            this.radioAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.radioAll.AutoSize = true;
            this.radioAll.Checked = true;
            this.radioAll.Location = new System.Drawing.Point(3, 5);
            this.radioAll.Name = "radioAll";
            this.radioAll.Size = new System.Drawing.Size(55, 24);
            this.radioAll.TabIndex = 0;
            this.radioAll.TabStop = true;
            this.radioAll.Text = "全天";
            this.radioAll.UseVisualStyleBackColor = true;
            // 
            // radioDay
            // 
            this.radioDay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.radioDay.AutoSize = true;
            this.radioDay.Location = new System.Drawing.Point(64, 5);
            this.radioDay.Name = "radioDay";
            this.radioDay.Size = new System.Drawing.Size(55, 24);
            this.radioDay.TabIndex = 0;
            this.radioDay.Text = "白天";
            this.radioDay.UseVisualStyleBackColor = true;
            // 
            // radioNight
            // 
            this.radioNight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.radioNight.AutoSize = true;
            this.radioNight.Location = new System.Drawing.Point(125, 5);
            this.radioNight.Name = "radioNight";
            this.radioNight.Size = new System.Drawing.Size(55, 24);
            this.radioNight.TabIndex = 0;
            this.radioNight.Text = "夜间";
            this.radioNight.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayout3);
            this.panel1.Controls.Add(this.tableLayout2);
            this.panel1.Controls.Add(this.tableLayout1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 43);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(946, 549);
            this.panel1.TabIndex = 1;
            // 
            // tableLayout3
            // 
            this.tableLayout3.ColumnCount = 3;
            this.tableLayout3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayout3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout3.Controls.Add(this.txtPulseMax, 2, 0);
            this.tableLayout3.Controls.Add(this.label8, 1, 0);
            this.tableLayout3.Controls.Add(this.txtPulseMin, 0, 0);
            this.tableLayout3.Location = new System.Drawing.Point(6, 355);
            this.tableLayout3.Name = "tableLayout3";
            this.tableLayout3.RowCount = 1;
            this.tableLayout3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayout3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayout3.Size = new System.Drawing.Size(200, 32);
            this.tableLayout3.TabIndex = 1;
            // 
            // txtPulseMax
            // 
            this.txtPulseMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPulseMax.Location = new System.Drawing.Point(113, 3);
            this.txtPulseMax.Maximum = new decimal(new int[] {
            280,
            0,
            0,
            0});
            this.txtPulseMax.Name = "txtPulseMax";
            this.txtPulseMax.Size = new System.Drawing.Size(84, 26);
            this.txtPulseMax.TabIndex = 0;
            this.txtPulseMax.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.txtPulseMax.ValueChanged += new System.EventHandler(this.txtPulseMax_ValueChanged);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(93, 6);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(14, 20);
            this.label8.TabIndex = 2;
            this.label8.Text = "-";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtPulseMin
            // 
            this.txtPulseMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPulseMin.Location = new System.Drawing.Point(3, 3);
            this.txtPulseMin.Maximum = new decimal(new int[] {
            280,
            0,
            0,
            0});
            this.txtPulseMin.Name = "txtPulseMin";
            this.txtPulseMin.Size = new System.Drawing.Size(84, 26);
            this.txtPulseMin.TabIndex = 0;
            this.txtPulseMin.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.txtPulseMin.ValueChanged += new System.EventHandler(this.txtPulseMin_ValueChanged);
            // 
            // tableLayout2
            // 
            this.tableLayout2.ColumnCount = 3;
            this.tableLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout2.Controls.Add(this.txtDBPMax, 2, 0);
            this.tableLayout2.Controls.Add(this.label6, 1, 0);
            this.tableLayout2.Controls.Add(this.txtDBPMin, 0, 0);
            this.tableLayout2.Location = new System.Drawing.Point(6, 201);
            this.tableLayout2.Name = "tableLayout2";
            this.tableLayout2.RowCount = 1;
            this.tableLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayout2.Size = new System.Drawing.Size(200, 32);
            this.tableLayout2.TabIndex = 1;
            // 
            // txtDBPMax
            // 
            this.txtDBPMax.DecimalPlaces = 2;
            this.txtDBPMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDBPMax.Location = new System.Drawing.Point(113, 3);
            this.txtDBPMax.Maximum = new decimal(new int[] {
            280,
            0,
            0,
            0});
            this.txtDBPMax.Name = "txtDBPMax";
            this.txtDBPMax.Size = new System.Drawing.Size(84, 26);
            this.txtDBPMax.TabIndex = 0;
            this.txtDBPMax.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.txtDBPMax.ValueChanged += new System.EventHandler(this.txtDBPMax_ValueChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(93, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(14, 20);
            this.label6.TabIndex = 2;
            this.label6.Text = "-";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtDBPMin
            // 
            this.txtDBPMin.DecimalPlaces = 2;
            this.txtDBPMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDBPMin.Location = new System.Drawing.Point(3, 3);
            this.txtDBPMin.Maximum = new decimal(new int[] {
            280,
            0,
            0,
            0});
            this.txtDBPMin.Name = "txtDBPMin";
            this.txtDBPMin.Size = new System.Drawing.Size(84, 26);
            this.txtDBPMin.TabIndex = 0;
            this.txtDBPMin.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.txtDBPMin.ValueChanged += new System.EventHandler(this.txtDBPMin_ValueChanged);
            // 
            // tableLayout1
            // 
            this.tableLayout1.ColumnCount = 3;
            this.tableLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout1.Controls.Add(this.txtSBPMax, 2, 0);
            this.tableLayout1.Controls.Add(this.label4, 1, 0);
            this.tableLayout1.Controls.Add(this.txtSBPMin, 0, 0);
            this.tableLayout1.Location = new System.Drawing.Point(6, 6);
            this.tableLayout1.Name = "tableLayout1";
            this.tableLayout1.RowCount = 1;
            this.tableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayout1.Size = new System.Drawing.Size(200, 32);
            this.tableLayout1.TabIndex = 1;
            // 
            // txtSBPMax
            // 
            this.txtSBPMax.DecimalPlaces = 2;
            this.txtSBPMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSBPMax.Location = new System.Drawing.Point(113, 3);
            this.txtSBPMax.Maximum = new decimal(new int[] {
            280,
            0,
            0,
            0});
            this.txtSBPMax.Name = "txtSBPMax";
            this.txtSBPMax.Size = new System.Drawing.Size(84, 26);
            this.txtSBPMax.TabIndex = 0;
            this.txtSBPMax.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.txtSBPMax.ValueChanged += new System.EventHandler(this.txtSBPMax_ValueChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(93, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 20);
            this.label4.TabIndex = 2;
            this.label4.Text = "-";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtSBPMin
            // 
            this.txtSBPMin.DecimalPlaces = 2;
            this.txtSBPMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSBPMin.Location = new System.Drawing.Point(3, 3);
            this.txtSBPMin.Maximum = new decimal(new int[] {
            280,
            0,
            0,
            0});
            this.txtSBPMin.Name = "txtSBPMin";
            this.txtSBPMin.Size = new System.Drawing.Size(84, 26);
            this.txtSBPMin.TabIndex = 0;
            this.txtSBPMin.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.txtSBPMin.ValueChanged += new System.EventHandler(this.txtSBPMin_ValueChanged);
            // 
            // BpPieChatControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.Name = "BpPieChatControl";
            this.Size = new System.Drawing.Size(952, 595);
            this.Load += new System.EventHandler(this.BpPieChatControl_Load);
            this.SizeChanged += new System.EventHandler(this.BpPieChatControl_SizeChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tableLayout3.ResumeLayout(false);
            this.tableLayout3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPulseMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPulseMin)).EndInit();
            this.tableLayout2.ResumeLayout(false);
            this.tableLayout2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDBPMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDBPMin)).EndInit();
            this.tableLayout1.ResumeLayout(false);
            this.tableLayout1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSBPMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSBPMin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton radioAll;
        private System.Windows.Forms.RadioButton radioDay;
        private System.Windows.Forms.RadioButton radioNight;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.NumericUpDown txtSBPMin;
        private System.Windows.Forms.TableLayoutPanel tableLayout2;
        private System.Windows.Forms.NumericUpDown txtDBPMax;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown txtDBPMin;
        private System.Windows.Forms.TableLayoutPanel tableLayout1;
        private System.Windows.Forms.NumericUpDown txtSBPMax;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TableLayoutPanel tableLayout3;
        private System.Windows.Forms.NumericUpDown txtPulseMax;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown txtPulseMin;
    }
}
