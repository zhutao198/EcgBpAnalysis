namespace AnalysisApp.CommonControls
{
    partial class OverlayPlot
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.ctxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuCut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLead = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGain = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSpeed = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSingleBeat = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMultiBeats = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(3, 3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(62, 20);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "II-[F1]-0";
            // 
            // ctxMenuStrip
            // 
            this.ctxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.mnuCut,
            this.mnuLead,
            this.mnuGain,
            this.mnuSpeed,
            this.mnuSingleBeat,
            this.mnuMultiBeats});
            this.ctxMenuStrip.Name = "ctxMenuStrip";
            this.ctxMenuStrip.Size = new System.Drawing.Size(181, 164);
            // 
            // mnuCut
            // 
            this.mnuCut.Name = "mnuCut";
            this.mnuCut.Size = new System.Drawing.Size(180, 22);
            this.mnuCut.Text = "切割至";
            // 
            // mnuLead
            // 
            this.mnuLead.Name = "mnuLead";
            this.mnuLead.Size = new System.Drawing.Size(180, 22);
            this.mnuLead.Text = "导联";
            // 
            // mnuGain
            // 
            this.mnuGain.Name = "mnuGain";
            this.mnuGain.Size = new System.Drawing.Size(180, 22);
            this.mnuGain.Text = "增益";
            // 
            // mnuSpeed
            // 
            this.mnuSpeed.Name = "mnuSpeed";
            this.mnuSpeed.Size = new System.Drawing.Size(180, 22);
            this.mnuSpeed.Text = "纸速";
            // 
            // mnuSingleBeat
            // 
            this.mnuSingleBeat.Name = "mnuSingleBeat";
            this.mnuSingleBeat.Size = new System.Drawing.Size(180, 22);
            this.mnuSingleBeat.Text = "单心搏叠加";
            // 
            // mnuMultiBeats
            // 
            this.mnuMultiBeats.Name = "mnuMultiBeats";
            this.mnuMultiBeats.Size = new System.Drawing.Size(180, 22);
            this.mnuMultiBeats.Text = "多心搏叠加";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // OverlayPlot
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(19)))), ((int)(((byte)(92)))));
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.Name = "OverlayPlot";
            this.Size = new System.Drawing.Size(426, 206);
            this.Load += new System.EventHandler(this.OverlayPlot_Load);
            this.ctxMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ContextMenuStrip ctxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuCut;
        private System.Windows.Forms.ToolStripMenuItem mnuLead;
        private System.Windows.Forms.ToolStripMenuItem mnuGain;
        private System.Windows.Forms.ToolStripMenuItem mnuSpeed;
        private System.Windows.Forms.ToolStripMenuItem mnuSingleBeat;
        private System.Windows.Forms.ToolStripMenuItem mnuMultiBeats;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}
