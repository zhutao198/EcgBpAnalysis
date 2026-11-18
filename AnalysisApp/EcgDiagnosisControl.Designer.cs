namespace AnalysisApp
{
    partial class EcgDiagnosisControl
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
            this.drawEcgCommonWave1 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.smallWaveViewControl1 = new AnalysisApp.CommonControls.SmallWaveViewControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.drawEcgCommonWave1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.smallWaveViewControl1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1138, 659);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // drawEcgCommonWave1
            // 
            this.drawEcgCommonWave1.BackColor = System.Drawing.Color.White;
            this.drawEcgCommonWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawEcgCommonWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.drawEcgCommonWave1.Location = new System.Drawing.Point(3, 3);
            this.drawEcgCommonWave1.Name = "drawEcgCommonWave1";
            this.drawEcgCommonWave1.Size = new System.Drawing.Size(1132, 593);
            this.drawEcgCommonWave1.TabIndex = 0;
            // 
            // smallWaveViewControl1
            // 
            this.smallWaveViewControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.smallWaveViewControl1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.smallWaveViewControl1.Location = new System.Drawing.Point(3, 602);
            this.smallWaveViewControl1.Name = "smallWaveViewControl1";
            this.smallWaveViewControl1.Size = new System.Drawing.Size(1132, 54);
            this.smallWaveViewControl1.TabIndex = 1;
            // 
            // EcgDiagnosisControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EcgDiagnosisControl";
            this.Size = new System.Drawing.Size(1138, 659);
            this.Load += new System.EventHandler(this.EcgDiagnosisControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private AnalysisApp.CommonControls.DrawEcgCommonWave drawEcgCommonWave1;
        private AnalysisApp.CommonControls.SmallWaveViewControl smallWaveViewControl1;
    }
}
