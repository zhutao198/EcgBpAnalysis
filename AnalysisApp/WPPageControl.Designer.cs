namespace AnalysisApp
{
    partial class WPPageControl
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
            this.drawEcgCommonWave1 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.wpWavePageControl1 = new AnalysisApp.CommonControls.WPWavePageControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.scatterTrend1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.drawEcgCommonWave1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.wpWavePageControl1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1117, 645);
            this.tableLayoutPanel1.TabIndex = 0;
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
            this.scatterTrend1.ShowMenuStrip = false;
            this.scatterTrend1.Size = new System.Drawing.Size(1111, 114);
            this.scatterTrend1.StartTime = new System.DateTime(((long)(0)));
            this.scatterTrend1.TabIndex = 0;
            // 
            // drawEcgCommonWave1
            // 
            this.drawEcgCommonWave1.BackColor = System.Drawing.Color.White;
            this.drawEcgCommonWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawEcgCommonWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.drawEcgCommonWave1.Location = new System.Drawing.Point(3, 464);
            this.drawEcgCommonWave1.Name = "drawEcgCommonWave1";
            this.drawEcgCommonWave1.Size = new System.Drawing.Size(1111, 178);
            this.drawEcgCommonWave1.TabIndex = 1;
            // 
            // wpWavePageControl1
            // 
            this.wpWavePageControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpWavePageControl1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.wpWavePageControl1.Location = new System.Drawing.Point(3, 123);
            this.wpWavePageControl1.Name = "wpWavePageControl1";
            this.wpWavePageControl1.Size = new System.Drawing.Size(1111, 335);
            this.wpWavePageControl1.TabIndex = 2;
            // 
            // WPPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "WPPageControl";
            this.Size = new System.Drawing.Size(1117, 645);
            this.Load += new System.EventHandler(this.WPPageControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private CommonControls.ScatterTrend scatterTrend1;
        private CommonControls.DrawEcgCommonWave drawEcgCommonWave1;
        private CommonControls.WPWavePageControl wpWavePageControl1;
    }
}
