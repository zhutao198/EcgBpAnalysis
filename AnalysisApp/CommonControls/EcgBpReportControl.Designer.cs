namespace AnalysisApp.CommonControls
{
    partial class EcgBpReportControl
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
            this.headerlessTabControl1 = new Zoncare.Component.Controls.ZqTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ecgReportControl1 = new AnalysisApp.CommonControls.EcgReportControl();
            this.bpReportControl1 = new AnalysisApp.CommonControls.BpReportControl();
            this.headerlessTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // headerlessTabControl1
            // 
            this.headerlessTabControl1.Controls.Add(this.tabPage1);
            this.headerlessTabControl1.Controls.Add(this.tabPage2);
            this.headerlessTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerlessTabControl1.Location = new System.Drawing.Point(0, 0);
            this.headerlessTabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.headerlessTabControl1.Name = "headerlessTabControl1";
            this.headerlessTabControl1.SelectedIndex = 0;
            this.headerlessTabControl1.Size = new System.Drawing.Size(987, 572);
            this.headerlessTabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.White;
            this.tabPage1.Controls.Add(this.ecgReportControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(979, 546);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "动态心电";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.bpReportControl1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(979, 546);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "动态血压";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ecgReportControl1
            // 
            this.ecgReportControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.ecgReportControl1.DataSourceTracked = false;
            this.ecgReportControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ecgReportControl1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.ecgReportControl1.IsActive = false;
            this.ecgReportControl1.Location = new System.Drawing.Point(3, 3);
            this.ecgReportControl1.Name = "ecgReportControl1";
            this.ecgReportControl1.ShouldReset = false;
            this.ecgReportControl1.Size = new System.Drawing.Size(973, 540);
            this.ecgReportControl1.TabIndex = 0;
            // 
            // bpReportControl1
            // 
            this.bpReportControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bpReportControl1.Location = new System.Drawing.Point(3, 3);
            this.bpReportControl1.Name = "bpReportControl1";
            this.bpReportControl1.Size = new System.Drawing.Size(973, 540);
            this.bpReportControl1.TabIndex = 0;
            // 
            // EcgBpReportControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.headerlessTabControl1);
            this.Name = "EcgBpReportControl";
            this.Size = new System.Drawing.Size(987, 572);
            this.headerlessTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Zoncare.Component.Controls.ZqTabControl headerlessTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private EcgReportControl ecgReportControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private BpReportControl bpReportControl1;
    }
}
