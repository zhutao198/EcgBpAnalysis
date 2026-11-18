namespace AnalysisApp
{
    partial class RRDiscreteWavePage
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
            this.scatterTrend1 = new AnalysisApp.CommonControls.ScatterTrend();
            this.rrDiscreteWaveControl1 = new AnalysisApp.CommonControls.RRDiscreteWaveControl();
            this.drawEcgCommonWave1 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.scatterTrend1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.rrDiscreteWaveControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.drawEcgCommonWave1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1040, 640);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // scatterTrend1
            // 
            this.scatterTrend1.AFList = null;
            this.scatterTrend1.BackColor = System.Drawing.Color.White;
            this.scatterTrend1.DefaultSelectLength = 3600F;
            this.scatterTrend1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scatterTrend1.EndTime = new System.DateTime(((long)(0)));
            this.scatterTrend1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.scatterTrend1.Location = new System.Drawing.Point(3, 3);
            this.scatterTrend1.Name = "scatterTrend1";
            this.scatterTrend1.Size = new System.Drawing.Size(1034, 114);
            this.scatterTrend1.StartTime = new System.DateTime(((long)(0)));
            this.scatterTrend1.TabIndex = 0;
            // 
            // rrDiscreteWaveControl1
            // 
            this.rrDiscreteWaveControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rrDiscreteWaveControl1.Location = new System.Drawing.Point(3, 123);
            this.rrDiscreteWaveControl1.Name = "rrDiscreteWaveControl1";
            this.rrDiscreteWaveControl1.Size = new System.Drawing.Size(1034, 358);
            this.rrDiscreteWaveControl1.TabIndex = 1;
            // 
            // drawEcgCommonWave1
            // 
            this.drawEcgCommonWave1.BackColor = System.Drawing.Color.White;
            this.drawEcgCommonWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawEcgCommonWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.drawEcgCommonWave1.Location = new System.Drawing.Point(3, 487);
            this.drawEcgCommonWave1.Name = "drawEcgCommonWave1";
            this.drawEcgCommonWave1.Size = new System.Drawing.Size(1034, 150);
            this.drawEcgCommonWave1.TabIndex = 2;
            // 
            // RRDiscreteWavePage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "RRDiscreteWavePage";
            this.Size = new System.Drawing.Size(1040, 640);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private CommonControls.ScatterTrend scatterTrend1;
        private CommonControls.RRDiscreteWaveControl rrDiscreteWaveControl1;
        private CommonControls.DrawEcgCommonWave drawEcgCommonWave1;
    }
}
