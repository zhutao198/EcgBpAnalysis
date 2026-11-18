namespace AnalysisApp
{
    partial class HistogramPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HistogramPage));
            this.histogramPlot1 = new AnalysisApp.CommonControls.HistogramPlot();
            this.ecgCommonWave1 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lvHistogramType = new System.Windows.Forms.ListView();
            this.tlpCategory = new System.Windows.Forms.TableLayoutPanel();
            this.tsBtnAverageHR = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tsBtnEarlyRate = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tsBtnInterval = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tsBtnIntervalRatio = new Zoncare.Component.Controls.RoundButton(this.components);
            this.beatWaveList1 = new AnalysisApp.CommonControls.BeatWaveList();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tlpCategory.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // histogramPlot1
            // 
            this.histogramPlot1.BackColor = System.Drawing.Color.White;
            this.histogramPlot1.ChartInfo = null;
            this.histogramPlot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.histogramPlot1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.histogramPlot1.Location = new System.Drawing.Point(3, 3);
            this.histogramPlot1.Name = "histogramPlot1";
            this.histogramPlot1.Size = new System.Drawing.Size(1189, 200);
            this.histogramPlot1.TabIndex = 0;
            // 
            // ecgCommonWave1
            // 
            this.ecgCommonWave1.BackColor = System.Drawing.Color.White;
            this.ecgCommonWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ecgCommonWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ecgCommonWave1.Location = new System.Drawing.Point(3, 597);
            this.ecgCommonWave1.Name = "ecgCommonWave1";
            this.ecgCommonWave1.Size = new System.Drawing.Size(1411, 251);
            this.ecgCommonWave1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 216F));
            this.tableLayoutPanel1.Controls.Add(this.histogramPlot1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1411, 206);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.lvHistogramType);
            this.panel1.Controls.Add(this.tlpCategory);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(1195, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(216, 206);
            this.panel1.TabIndex = 3;
            // 
            // lvHistogramType
            // 
            this.lvHistogramType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvHistogramType.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lvHistogramType.FullRowSelect = true;
            this.lvHistogramType.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvHistogramType.HideSelection = false;
            this.lvHistogramType.Location = new System.Drawing.Point(0, 28);
            this.lvHistogramType.MultiSelect = false;
            this.lvHistogramType.Name = "lvHistogramType";
            this.lvHistogramType.Size = new System.Drawing.Size(216, 178);
            this.lvHistogramType.TabIndex = 1;
            this.lvHistogramType.UseCompatibleStateImageBehavior = false;
            this.lvHistogramType.View = System.Windows.Forms.View.Details;
            this.lvHistogramType.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvHistogramType_ItemSelectionChanged);
            this.lvHistogramType.SelectedIndexChanged += new System.EventHandler(this.lvHistogramType_SelectedIndexChanged);
            this.lvHistogramType.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lvHistogramType_MouseMove);
            // 
            // tlpCategory
            // 
            this.tlpCategory.ColumnCount = 5;
            this.tlpCategory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCategory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCategory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCategory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpCategory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCategory.Controls.Add(this.tsBtnAverageHR, 0, 0);
            this.tlpCategory.Controls.Add(this.tsBtnEarlyRate, 0, 0);
            this.tlpCategory.Controls.Add(this.tsBtnInterval, 0, 0);
            this.tlpCategory.Controls.Add(this.tsBtnIntervalRatio, 0, 0);
            this.tlpCategory.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpCategory.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.tlpCategory.Location = new System.Drawing.Point(0, 0);
            this.tlpCategory.Name = "tlpCategory";
            this.tlpCategory.RowCount = 1;
            this.tlpCategory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCategory.Size = new System.Drawing.Size(216, 28);
            this.tlpCategory.TabIndex = 2;
            // 
            // tsBtnAverageHR
            // 
            this.tsBtnAverageHR.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tsBtnAverageHR.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.tsBtnAverageHR.DataFlag = null;
            this.tsBtnAverageHR.FollowBaseColor = false;
            this.tsBtnAverageHR.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.tsBtnAverageHR.IsHighlight = true;
            this.tsBtnAverageHR.Location = new System.Drawing.Point(99, 3);
            this.tsBtnAverageHR.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tsBtnAverageHR.Name = "tsBtnAverageHR";
            this.tsBtnAverageHR.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tsBtnAverageHR.Size = new System.Drawing.Size(45, 22);
            this.tsBtnAverageHR.TabIndex = 8;
            this.tsBtnAverageHR.Text = "心率";
            this.tsBtnAverageHR.UseVisualStyleBackColor = true;
            // 
            // tsBtnEarlyRate
            // 
            this.tsBtnEarlyRate.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tsBtnEarlyRate.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.tsBtnEarlyRate.DataFlag = null;
            this.tsBtnEarlyRate.FollowBaseColor = false;
            this.tsBtnEarlyRate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.tsBtnEarlyRate.IsHighlight = true;
            this.tsBtnEarlyRate.Location = new System.Drawing.Point(147, 3);
            this.tsBtnEarlyRate.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tsBtnEarlyRate.Name = "tsBtnEarlyRate";
            this.tsBtnEarlyRate.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tsBtnEarlyRate.Size = new System.Drawing.Size(45, 22);
            this.tsBtnEarlyRate.TabIndex = 7;
            this.tsBtnEarlyRate.Text = "提前量";
            this.tsBtnEarlyRate.UseVisualStyleBackColor = true;
            // 
            // tsBtnInterval
            // 
            this.tsBtnInterval.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tsBtnInterval.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.tsBtnInterval.DataFlag = null;
            this.tsBtnInterval.FollowBaseColor = false;
            this.tsBtnInterval.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.tsBtnInterval.IsHighlight = true;
            this.tsBtnInterval.Location = new System.Drawing.Point(3, 3);
            this.tsBtnInterval.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tsBtnInterval.Name = "tsBtnInterval";
            this.tsBtnInterval.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tsBtnInterval.Size = new System.Drawing.Size(45, 22);
            this.tsBtnInterval.TabIndex = 6;
            this.tsBtnInterval.Text = "间期";
            this.tsBtnInterval.UseVisualStyleBackColor = true;
            // 
            // tsBtnIntervalRatio
            // 
            this.tsBtnIntervalRatio.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tsBtnIntervalRatio.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.tsBtnIntervalRatio.DataFlag = null;
            this.tsBtnIntervalRatio.FollowBaseColor = false;
            this.tsBtnIntervalRatio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.tsBtnIntervalRatio.IsHighlight = true;
            this.tsBtnIntervalRatio.Location = new System.Drawing.Point(51, 3);
            this.tsBtnIntervalRatio.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tsBtnIntervalRatio.Name = "tsBtnIntervalRatio";
            this.tsBtnIntervalRatio.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tsBtnIntervalRatio.Size = new System.Drawing.Size(45, 22);
            this.tsBtnIntervalRatio.TabIndex = 5;
            this.tsBtnIntervalRatio.Text = "间期比";
            this.tsBtnIntervalRatio.UseVisualStyleBackColor = true;
            // 
            // beatWaveList1
            // 
            this.beatWaveList1.BackColor = System.Drawing.Color.White;
            this.beatWaveList1.ColCount = 4;
            this.beatWaveList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.beatWaveList1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.beatWaveList1.Gain = 5F;
            this.beatWaveList1.LeadCount = 1;
            this.beatWaveList1.Location = new System.Drawing.Point(3, 218);
            this.beatWaveList1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.beatWaveList1.MultiLeads = ((System.Collections.Generic.List<int>)(resources.GetObject("beatWaveList1.MultiLeads")));
            this.beatWaveList1.Name = "beatWaveList1";
            this.beatWaveList1.RowCount = 2;
            this.beatWaveList1.ShowAverageHR = false;
            this.beatWaveList1.ShowBatchAddToggle = true;
            this.beatWaveList1.ShowCommonWaveToggle = false;
            this.beatWaveList1.ShowHistogramToggle = false;
            this.beatWaveList1.ShowTime = false;
            this.beatWaveList1.Size = new System.Drawing.Size(1411, 370);
            this.beatWaveList1.Speed = 25F;
            this.beatWaveList1.TabIndex = 3;
            this.beatWaveList1.UseForArrhythmia = false;
            this.beatWaveList1.UseForNormalSinusRhythm = false;
            this.beatWaveList1.WaveLeads = ((System.Collections.Generic.List<int>)(resources.GetObject("beatWaveList1.WaveLeads")));
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.beatWaveList1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.ecgCommonWave1, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1417, 851);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // HistogramPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "HistogramPage";
            this.Size = new System.Drawing.Size(1417, 851);
            this.Load += new System.EventHandler(this.HistogramPage_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tlpCategory.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CommonControls.HistogramPlot histogramPlot1;
        private CommonControls.DrawEcgCommonWave ecgCommonWave1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListView lvHistogramType;
        private CommonControls.BeatWaveList beatWaveList1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tlpCategory;
        private Zoncare.Component.Controls.RoundButton tsBtnIntervalRatio;
        private Zoncare.Component.Controls.RoundButton tsBtnAverageHR;
        private Zoncare.Component.Controls.RoundButton tsBtnEarlyRate;
        private Zoncare.Component.Controls.RoundButton tsBtnInterval;
    }
}
