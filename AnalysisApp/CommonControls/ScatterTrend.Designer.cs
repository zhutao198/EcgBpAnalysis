namespace AnalysisApp.CommonControls
{
    partial class ScatterTrend
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
            this.mnuSetAF = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSetAFL = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelAF = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSetRange = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSetFullAF = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSetFullAFL = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctxMenuStrip
            // 
            this.ctxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSetAF,
            this.mnuSetAFL,
            this.mnuDelAF,
            this.mnuSetRange,
            this.mnuSetFullAF,
            this.mnuSetFullAFL,
            this.toolStripSeparator1,
            this.mnuSelectAll});
            this.ctxMenuStrip.Name = "ctxMenuStrip";
            this.ctxMenuStrip.Size = new System.Drawing.Size(181, 186);
            // 
            // mnuSetAF
            // 
            this.mnuSetAF.Name = "mnuSetAF";
            this.mnuSetAF.Size = new System.Drawing.Size(180, 22);
            this.mnuSetAF.Text = "设为房颤";
            // 
            // mnuSetAFL
            // 
            this.mnuSetAFL.Name = "mnuSetAFL";
            this.mnuSetAFL.Size = new System.Drawing.Size(180, 22);
            this.mnuSetAFL.Text = "设为房扑";
            // 
            // mnuDelAF
            // 
            this.mnuDelAF.Name = "mnuDelAF";
            this.mnuDelAF.Size = new System.Drawing.Size(180, 22);
            this.mnuDelAF.Text = "删除房颤/房扑";
            // 
            // mnuSetRange
            // 
            this.mnuSetRange.Name = "mnuSetRange";
            this.mnuSetRange.Size = new System.Drawing.Size(180, 22);
            this.mnuSetRange.Text = "选择范围";
            // 
            // mnuSetFullAF
            // 
            this.mnuSetFullAF.Name = "mnuSetFullAF";
            this.mnuSetFullAF.Size = new System.Drawing.Size(180, 22);
            this.mnuSetFullAF.Text = "设为全程房颤";
            // 
            // mnuSetFullAFL
            // 
            this.mnuSetFullAFL.Name = "mnuSetFullAFL";
            this.mnuSetFullAFL.Size = new System.Drawing.Size(180, 22);
            this.mnuSetFullAFL.Text = "设为全程房扑";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // mnuSelectAll
            // 
            this.mnuSelectAll.Name = "mnuSelectAll";
            this.mnuSelectAll.Size = new System.Drawing.Size(180, 22);
            this.mnuSelectAll.Text = "全选";
            // 
            // ScatterTrend
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "ScatterTrend";
            this.Size = new System.Drawing.Size(875, 188);
            this.Load += new System.EventHandler(this.ScatterTrend_Load);
            this.SizeChanged += new System.EventHandler(this.ScatterTrend_SizeChanged);
            this.ctxMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip ctxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuSetAF;
        private System.Windows.Forms.ToolStripMenuItem mnuSetAFL;
        private System.Windows.Forms.ToolStripMenuItem mnuDelAF;
        private System.Windows.Forms.ToolStripMenuItem mnuSetRange;
        private System.Windows.Forms.ToolStripMenuItem mnuSetFullAF;
        private System.Windows.Forms.ToolStripMenuItem mnuSetFullAFL;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectAll;
    }
}
