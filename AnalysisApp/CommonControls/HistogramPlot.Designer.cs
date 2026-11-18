namespace AnalysisApp.CommonControls
{
    partial class HistogramPlot
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
            this.ctxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuBeatDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctxMenuStrip
            // 
            this.ctxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuBeatDelete});
            this.ctxMenuStrip.Name = "ctxMenuStrip";
            this.ctxMenuStrip.Size = new System.Drawing.Size(181, 48);
            // 
            // mnuBeatDelete
            // 
            this.mnuBeatDelete.Name = "mnuBeatDelete";
            this.mnuBeatDelete.Size = new System.Drawing.Size(180, 22);
            this.mnuBeatDelete.Text = "删除心搏";
            // 
            // HistogramPlot
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "HistogramPlot";
            this.Size = new System.Drawing.Size(973, 221);
            this.Load += new System.EventHandler(this.HistogramPlot_Load);
            this.SizeChanged += new System.EventHandler(this.HistogramPlot_SizeChanged);
            this.ctxMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip ctxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuBeatDelete;
    }
}
