namespace AnalysisApp
{
    partial class HRVPageControl
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HRVPageControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dgvData = new Zoncare.Component.Controls.DataGridViewEx();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lblSDNN = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.lblSDANN = new System.Windows.Forms.Label();
            this.lblSDANNIndex = new System.Windows.Forms.Label();
            this.lblrMSSD = new System.Windows.Forms.Label();
            this.lblpNN50 = new System.Windows.Forms.Label();
            this.lblTriindex = new System.Windows.Forms.Label();
            this.lblTP = new System.Windows.Forms.Label();
            this.lblVLF = new System.Windows.Forms.Label();
            this.lblLF = new System.Windows.Forms.Label();
            this.lblHF = new System.Windows.Forms.Label();
            this.lblLFHF = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.richTbxAdvice = new System.Windows.Forms.RichTextBox();
            this.btnSave = new Zoncare.Component.Controls.RoundButton(this.components);
            this.dgvDataFreq = new Zoncare.Component.Controls.DataGridViewEx();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataFreq)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1105, 736);
            this.tabControl1.TabIndex = 3;
            this.tabControl1.Click += new System.EventHandler(this.tabControl1_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(1097, 703);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "HRV时域分析";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.dgvData, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1097, 703);
            this.tableLayoutPanel1.TabIndex = 0;
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
            this.dgvData.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvData.ColumnHeadersHeight = 36;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvData.CustomDateTimeFormat = null;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.Lavender;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.EnableHeadersVisualStyles = false;
            this.dgvData.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgvData.HighlightRowHover = true;
            this.dgvData.Location = new System.Drawing.Point(3, 3);
            this.dgvData.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvData.MergeColumnNames")));
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 30;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1091, 697);
            this.dgvData.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvData.TabIndex = 4;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tabPage2.Controls.Add(this.tableLayoutPanel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(1097, 703);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "HRV频域分析";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.dgvDataFreq, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1097, 703);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.tableLayoutPanel3);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1097, 703);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "心率变异性结论";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel3.Controls.Add(this.lblSDNN, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.label5, 0, 5);
            this.tableLayoutPanel3.Controls.Add(this.label6, 0, 7);
            this.tableLayoutPanel3.Controls.Add(this.label7, 0, 9);
            this.tableLayoutPanel3.Controls.Add(this.label8, 0, 11);
            this.tableLayoutPanel3.Controls.Add(this.label9, 0, 13);
            this.tableLayoutPanel3.Controls.Add(this.label10, 0, 15);
            this.tableLayoutPanel3.Controls.Add(this.label11, 0, 17);
            this.tableLayoutPanel3.Controls.Add(this.label12, 0, 19);
            this.tableLayoutPanel3.Controls.Add(this.label13, 0, 21);
            this.tableLayoutPanel3.Controls.Add(this.label14, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.label15, 1, 4);
            this.tableLayoutPanel3.Controls.Add(this.label16, 1, 6);
            this.tableLayoutPanel3.Controls.Add(this.label17, 1, 8);
            this.tableLayoutPanel3.Controls.Add(this.label18, 1, 10);
            this.tableLayoutPanel3.Controls.Add(this.label19, 1, 12);
            this.tableLayoutPanel3.Controls.Add(this.label20, 1, 14);
            this.tableLayoutPanel3.Controls.Add(this.label21, 1, 16);
            this.tableLayoutPanel3.Controls.Add(this.label22, 1, 18);
            this.tableLayoutPanel3.Controls.Add(this.label23, 1, 20);
            this.tableLayoutPanel3.Controls.Add(this.lblSDANN, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.lblSDANNIndex, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.lblrMSSD, 0, 6);
            this.tableLayoutPanel3.Controls.Add(this.lblpNN50, 0, 8);
            this.tableLayoutPanel3.Controls.Add(this.lblTriindex, 0, 10);
            this.tableLayoutPanel3.Controls.Add(this.lblTP, 0, 12);
            this.tableLayoutPanel3.Controls.Add(this.lblVLF, 0, 14);
            this.tableLayoutPanel3.Controls.Add(this.lblLF, 0, 16);
            this.tableLayoutPanel3.Controls.Add(this.lblHF, 0, 18);
            this.tableLayoutPanel3.Controls.Add(this.lblLFHF, 0, 20);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 22);
            this.tableLayoutPanel3.Controls.Add(this.richTbxAdvice, 0, 23);
            this.tableLayoutPanel3.Controls.Add(this.btnSave, 2, 23);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 24;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1091, 697);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // lblSDNN
            // 
            this.lblSDNN.AutoSize = true;
            this.lblSDNN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSDNN.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblSDNN.Location = new System.Drawing.Point(3, 0);
            this.lblSDNN.Name = "lblSDNN";
            this.lblSDNN.Size = new System.Drawing.Size(430, 26);
            this.lblSDNN.TabIndex = 0;
            this.lblSDNN.Text = "label1";
            this.lblSDNN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(439, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(154, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "(参考值：141±39 ms)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(430, 26);
            this.label3.TabIndex = 2;
            this.label3.Text = "R-R间期标准差，反映24h总体心率变异性，包含交感和副交感神经的全部调节信息。";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(430, 26);
            this.label4.TabIndex = 3;
            this.label4.Text = "平均R-R间期标准差，反映长周期(>5min)心率变化，与昼夜节律和神经内分泌节律相关。";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 130);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(430, 26);
            this.label5.TabIndex = 4;
            this.label5.Text = "5min R-R间期标准差的均值，反映5min内心率变异性的平均水平，去除长周期趋势后的短周期变异性。";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 182);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(430, 26);
            this.label6.TabIndex = 4;
            this.label6.Text = "相邻R-R间期差值均方根，反映短周期心率变化，主要由迷走神经(副交感)张力驱动。";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 234);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(430, 26);
            this.label7.TabIndex = 4;
            this.label7.Text = "相邻R-R间期差值>50ms的百分比，直接计数相邻心跳间期变化>50ms的比例，反映副交感张力。";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 286);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(430, 26);
            this.label8.TabIndex = 4;
            this.label8.Text = "HRV三角指数，基于R-R间期直方图几何形态的总体HRV指标，反映R-R间期分布的离散程度。";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(3, 338);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(430, 26);
            this.label9.TabIndex = 4;
            this.label9.Text = "总功率，反映整体自主神经调节活动，是HRV的\'全景指标\'。";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(3, 390);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(430, 26);
            this.label10.TabIndex = 4;
            this.label10.Text = "极低频功率，与体温调节、肾素-血管紧张素系统及内分泌节律相关，部分含交感成分。";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label11.Location = new System.Drawing.Point(3, 442);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(430, 26);
            this.label11.TabIndex = 4;
            this.label11.Text = "低频功率，反映交感与副交感神经的混合调节(交感占主导),主要与压力反射(baroreflex)活动相关。";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label12.Location = new System.Drawing.Point(3, 494);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(430, 26);
            this.label12.TabIndex = 4;
            this.label12.Text = "高频功率，主要反映迷走神经(副交感)张力，与呼吸性窦性心律不齐(RSA)直接相关。";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label13.Location = new System.Drawing.Point(3, 546);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(430, 26);
            this.label13.TabIndex = 4;
            this.label13.Text = "LF/HF比值，反映交感-迷走平衡状态；比值升高→交感占优，降低→迷走占优。";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label14.Location = new System.Drawing.Point(439, 52);
            this.label14.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(154, 26);
            this.label14.TabIndex = 5;
            this.label14.Text = "(参考值：127±35 ms)";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label15.Location = new System.Drawing.Point(439, 104);
            this.label15.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(154, 26);
            this.label15.TabIndex = 5;
            this.label15.Text = "(参考值：81±24ms)";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label16.Location = new System.Drawing.Point(439, 156);
            this.label16.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(154, 26);
            this.label16.TabIndex = 5;
            this.label16.Text = "(参考值：27±12 ms)";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label17.Location = new System.Drawing.Point(439, 208);
            this.label17.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(154, 26);
            this.label17.TabIndex = 5;
            this.label17.Text = "(参考值：11±8%(3-27%))";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label18.Location = new System.Drawing.Point(439, 260);
            this.label18.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(154, 26);
            this.label18.TabIndex = 5;
            this.label18.Text = "(参考值：37±15)";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label19.Location = new System.Drawing.Point(439, 312);
            this.label19.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(154, 26);
            this.label19.TabIndex = 5;
            this.label19.Text = "(参考值：3466±1018 ms2)";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label20.Location = new System.Drawing.Point(439, 364);
            this.label20.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(154, 26);
            this.label20.TabIndex = 5;
            this.label20.Text = "(参考值：2621±922 ms²)";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label21.Location = new System.Drawing.Point(439, 416);
            this.label21.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(154, 26);
            this.label21.TabIndex = 5;
            this.label21.Text = "(参考值：519±275 ms²)";
            this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label22.Location = new System.Drawing.Point(439, 468);
            this.label22.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(154, 26);
            this.label22.TabIndex = 5;
            this.label22.Text = "(参考值：654±339 ms²)";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label23.Location = new System.Drawing.Point(439, 520);
            this.label23.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(154, 26);
            this.label23.TabIndex = 5;
            this.label23.Text = "(参考值：1.1-4.0)";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSDANN
            // 
            this.lblSDANN.AutoSize = true;
            this.lblSDANN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSDANN.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblSDANN.Location = new System.Drawing.Point(3, 52);
            this.lblSDANN.Name = "lblSDANN";
            this.lblSDANN.Size = new System.Drawing.Size(430, 26);
            this.lblSDANN.TabIndex = 6;
            this.lblSDANN.Text = "label1";
            this.lblSDANN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSDANNIndex
            // 
            this.lblSDANNIndex.AutoSize = true;
            this.lblSDANNIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSDANNIndex.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblSDANNIndex.Location = new System.Drawing.Point(3, 104);
            this.lblSDANNIndex.Name = "lblSDANNIndex";
            this.lblSDANNIndex.Size = new System.Drawing.Size(430, 26);
            this.lblSDANNIndex.TabIndex = 6;
            this.lblSDANNIndex.Text = "label1";
            this.lblSDANNIndex.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblrMSSD
            // 
            this.lblrMSSD.AutoSize = true;
            this.lblrMSSD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblrMSSD.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblrMSSD.Location = new System.Drawing.Point(3, 156);
            this.lblrMSSD.Name = "lblrMSSD";
            this.lblrMSSD.Size = new System.Drawing.Size(430, 26);
            this.lblrMSSD.TabIndex = 6;
            this.lblrMSSD.Text = "label1";
            this.lblrMSSD.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblpNN50
            // 
            this.lblpNN50.AutoSize = true;
            this.lblpNN50.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblpNN50.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblpNN50.Location = new System.Drawing.Point(3, 208);
            this.lblpNN50.Name = "lblpNN50";
            this.lblpNN50.Size = new System.Drawing.Size(430, 26);
            this.lblpNN50.TabIndex = 6;
            this.lblpNN50.Text = "label1";
            this.lblpNN50.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTriindex
            // 
            this.lblTriindex.AutoSize = true;
            this.lblTriindex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTriindex.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblTriindex.Location = new System.Drawing.Point(3, 260);
            this.lblTriindex.Name = "lblTriindex";
            this.lblTriindex.Size = new System.Drawing.Size(430, 26);
            this.lblTriindex.TabIndex = 6;
            this.lblTriindex.Text = "label1";
            this.lblTriindex.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTP
            // 
            this.lblTP.AutoSize = true;
            this.lblTP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTP.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblTP.Location = new System.Drawing.Point(3, 312);
            this.lblTP.Name = "lblTP";
            this.lblTP.Size = new System.Drawing.Size(430, 26);
            this.lblTP.TabIndex = 6;
            this.lblTP.Text = "label1";
            this.lblTP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVLF
            // 
            this.lblVLF.AutoSize = true;
            this.lblVLF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVLF.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblVLF.Location = new System.Drawing.Point(3, 364);
            this.lblVLF.Name = "lblVLF";
            this.lblVLF.Size = new System.Drawing.Size(430, 26);
            this.lblVLF.TabIndex = 6;
            this.lblVLF.Text = "label1";
            this.lblVLF.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLF
            // 
            this.lblLF.AutoSize = true;
            this.lblLF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLF.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblLF.Location = new System.Drawing.Point(3, 416);
            this.lblLF.Name = "lblLF";
            this.lblLF.Size = new System.Drawing.Size(430, 26);
            this.lblLF.TabIndex = 6;
            this.lblLF.Text = "label1";
            this.lblLF.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblHF
            // 
            this.lblHF.AutoSize = true;
            this.lblHF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHF.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblHF.Location = new System.Drawing.Point(3, 468);
            this.lblHF.Name = "lblHF";
            this.lblHF.Size = new System.Drawing.Size(430, 26);
            this.lblHF.TabIndex = 6;
            this.lblHF.Text = "label1";
            this.lblHF.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLFHF
            // 
            this.lblLFHF.AutoSize = true;
            this.lblLFHF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLFHF.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lblLFHF.Location = new System.Drawing.Point(3, 520);
            this.lblLFHF.Name = "lblLFHF";
            this.lblLFHF.Size = new System.Drawing.Size(430, 26);
            this.lblLFHF.TabIndex = 6;
            this.lblLFHF.Text = "label1";
            this.lblLFHF.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(3, 572);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(430, 26);
            this.label1.TabIndex = 7;
            this.label1.Text = "结论";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // richTbxAdvice
            // 
            this.richTbxAdvice.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel3.SetColumnSpan(this.richTbxAdvice, 2);
            this.richTbxAdvice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTbxAdvice.Location = new System.Drawing.Point(6, 604);
            this.richTbxAdvice.Margin = new System.Windows.Forms.Padding(6);
            this.richTbxAdvice.Name = "richTbxAdvice";
            this.richTbxAdvice.Size = new System.Drawing.Size(587, 87);
            this.richTbxAdvice.TabIndex = 8;
            this.richTbxAdvice.Text = "";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.BackColor = System.Drawing.Color.White;
            this.btnSave.DataFlag = null;
            this.btnSave.FollowBaseColor = false;
            this.btnSave.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSave.IsHighlight = true;
            this.btnSave.Location = new System.Drawing.Point(605, 663);
            this.btnSave.Margin = new System.Windows.Forms.Padding(6);
            this.btnSave.Name = "btnSave";
            this.btnSave.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSave.Size = new System.Drawing.Size(74, 28);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // dgvDataFreq
            // 
            this.dgvDataFreq.ActionColumnHeaderText = null;
            this.dgvDataFreq.ActionColumnWidth = 0;
            this.dgvDataFreq.AllowUserToAddRows = false;
            this.dgvDataFreq.AllowUserToDeleteRows = false;
            this.dgvDataFreq.AllowUserToResizeRows = false;
            this.dgvDataFreq.AlternateRowColor = true;
            this.dgvDataFreq.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDataFreq.BackgroundColor = System.Drawing.Color.White;
            this.dgvDataFreq.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvDataFreq.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDataFreq.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvDataFreq.ColumnHeadersHeight = 36;
            this.dgvDataFreq.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvDataFreq.CustomDateTimeFormat = null;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.Lavender;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDataFreq.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvDataFreq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDataFreq.EnableHeadersVisualStyles = false;
            this.dgvDataFreq.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgvDataFreq.HighlightRowHover = true;
            this.dgvDataFreq.Location = new System.Drawing.Point(3, 3);
            this.dgvDataFreq.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvDataFreq.MergeColumnNames")));
            this.dgvDataFreq.MultiSelect = false;
            this.dgvDataFreq.Name = "dgvDataFreq";
            this.dgvDataFreq.ReadOnly = true;
            this.dgvDataFreq.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvDataFreq.RowHeadersVisible = false;
            this.dgvDataFreq.RowTemplate.Height = 30;
            this.dgvDataFreq.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDataFreq.Size = new System.Drawing.Size(1091, 697);
            this.dgvDataFreq.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvDataFreq.TabIndex = 5;
            // 
            // HRVPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tabControl1);
            this.Name = "HRVPageControl";
            this.Size = new System.Drawing.Size(1105, 736);
            this.Load += new System.EventHandler(this.HRVPageControl_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataFreq)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lblSDNN;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label lblSDANN;
        private System.Windows.Forms.Label lblSDANNIndex;
        private System.Windows.Forms.Label lblrMSSD;
        private System.Windows.Forms.Label lblpNN50;
        private System.Windows.Forms.Label lblTriindex;
        private System.Windows.Forms.Label lblTP;
        private System.Windows.Forms.Label lblVLF;
        private System.Windows.Forms.Label lblLF;
        private System.Windows.Forms.Label lblHF;
        private System.Windows.Forms.Label lblLFHF;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richTbxAdvice;
        private Zoncare.Component.Controls.RoundButton btnSave;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Zoncare.Component.Controls.DataGridViewEx dgvData;
        private Zoncare.Component.Controls.DataGridViewEx dgvDataFreq;
    }
}
