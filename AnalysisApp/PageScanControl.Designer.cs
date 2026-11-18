namespace AnalysisApp
{
    partial class PageScanControl
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
            this.pageScanWave1 = new AnalysisApp.CommonControls.pageScanWave();
            this.hrTrendWaveControl1 = new AnalysisApp.CommonControls.HRTrendWaveControl();
            this.drawEcgCommonWave1 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pageScanWave1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.hrTrendWaveControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.drawEcgCommonWave1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1366, 768);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pageScanWave1
            // 
            this.pageScanWave1.BackColor = System.Drawing.Color.White;
            this.pageScanWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pageScanWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pageScanWave1.Location = new System.Drawing.Point(3, 145);
            this.pageScanWave1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.pageScanWave1.Name = "pageScanWave1";
            this.pageScanWave1.Size = new System.Drawing.Size(1360, 398);
            this.pageScanWave1.TabIndex = 1;
            // 
            // hrTrendWaveControl1
            // 
            this.hrTrendWaveControl1.BackColor = System.Drawing.Color.White;
            this.hrTrendWaveControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hrTrendWaveControl1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.hrTrendWaveControl1.Location = new System.Drawing.Point(3, 3);
            this.hrTrendWaveControl1.Name = "hrTrendWaveControl1";
            this.hrTrendWaveControl1.Size = new System.Drawing.Size(1360, 134);
            this.hrTrendWaveControl1.TabIndex = 2;
            // 
            // drawEcgCommonWave1
            // 
            this.drawEcgCommonWave1.BackColor = System.Drawing.Color.White;
            this.drawEcgCommonWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawEcgCommonWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.drawEcgCommonWave1.Location = new System.Drawing.Point(3, 551);
            this.drawEcgCommonWave1.Name = "drawEcgCommonWave1";
            this.drawEcgCommonWave1.Size = new System.Drawing.Size(1360, 214);
            this.drawEcgCommonWave1.TabIndex = 3;
            // 
            // PageScanControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "PageScanControl";
            this.Size = new System.Drawing.Size(1366, 768);
            this.Load += new System.EventHandler(this.PageScanControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private CommonControls.pageScanWave pageScanWave1;
        private CommonControls.HRTrendWaveControl hrTrendWaveControl1;
        private CommonControls.DrawEcgCommonWave drawEcgCommonWave1;
    }
}
