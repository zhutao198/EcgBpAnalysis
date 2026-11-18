namespace AnalysisApp
{
    partial class EcgSegmentControl
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
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.segmentWaveViewControl1 = new AnalysisApp.CommonControls.SegmentWaveViewControl();
            this.segmentChartControl1 = new AnalysisApp.CommonControls.SegmentChartControl();
            this.segmentListControl1 = new AnalysisApp.CommonControls.SegmentListControl();
            this.userEventListControl1 = new AnalysisApp.CommonControls.UserEventListControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1082, 649);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.segmentWaveViewControl1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.segmentChartControl1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(808, 643);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.segmentListControl1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.userEventListControl1, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(811, 3);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(268, 643);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // segmentWaveViewControl1
            // 
            this.segmentWaveViewControl1.BackColor = System.Drawing.Color.White;
            this.segmentWaveViewControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.segmentWaveViewControl1.Location = new System.Drawing.Point(3, 203);
            this.segmentWaveViewControl1.Name = "segmentWaveViewControl1";
            this.segmentWaveViewControl1.Size = new System.Drawing.Size(802, 437);
            this.segmentWaveViewControl1.TabIndex = 0;
            // 
            // segmentChartControl1
            // 
            this.segmentChartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.segmentChartControl1.Location = new System.Drawing.Point(3, 3);
            this.segmentChartControl1.Name = "segmentChartControl1";
            this.segmentChartControl1.Size = new System.Drawing.Size(802, 194);
            this.segmentChartControl1.TabIndex = 1;
            // 
            // segmentListControl1
            // 
            this.segmentListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.segmentListControl1.Location = new System.Drawing.Point(3, 3);
            this.segmentListControl1.Name = "segmentListControl1";
            this.segmentListControl1.Size = new System.Drawing.Size(262, 379);
            this.segmentListControl1.TabIndex = 0;
            // 
            // userEventListControl1
            // 
            this.userEventListControl1.BackColor = System.Drawing.Color.White;
            this.userEventListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userEventListControl1.Location = new System.Drawing.Point(3, 388);
            this.userEventListControl1.Name = "userEventListControl1";
            this.userEventListControl1.Size = new System.Drawing.Size(262, 252);
            this.userEventListControl1.TabIndex = 1;
            // 
            // EcgSegmentControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EcgSegmentControl";
            this.Size = new System.Drawing.Size(1082, 649);
            this.Load += new System.EventHandler(this.EcgSegmentControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private CommonControls.SegmentWaveViewControl segmentWaveViewControl1;
        private CommonControls.SegmentChartControl segmentChartControl1;
        private CommonControls.SegmentListControl segmentListControl1;
        private CommonControls.UserEventListControl userEventListControl1;
    }
}
