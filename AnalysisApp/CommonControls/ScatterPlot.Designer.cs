namespace AnalysisApp.CommonControls
{
    partial class ScatterPlot
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScatterPlot));
            this.tlpTool = new System.Windows.Forms.TableLayoutPanel();
            this.btnHrLine = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnScatterType = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ctxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuBeatDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuShowColor = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddBlockedPAC = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTipScatter = new System.Windows.Forms.ToolTip(this.components);
            this.tlpTool.SuspendLayout();
            this.ctxMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpTool
            // 
            this.tlpTool.ColumnCount = 1;
            this.tlpTool.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTool.Controls.Add(this.btnHrLine, 0, 2);
            this.tlpTool.Controls.Add(this.btnSelect, 0, 0);
            this.tlpTool.Controls.Add(this.btnScatterType, 0, 1);
            this.tlpTool.Dock = System.Windows.Forms.DockStyle.Right;
            this.tlpTool.Location = new System.Drawing.Point(460, 0);
            this.tlpTool.Name = "tlpTool";
            this.tlpTool.RowCount = 4;
            this.tlpTool.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpTool.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpTool.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tlpTool.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTool.Size = new System.Drawing.Size(34, 460);
            this.tlpTool.TabIndex = 0;
            // 
            // btnHrLine
            // 
            this.btnHrLine.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnHrLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.btnHrLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHrLine.FlatAppearance.BorderSize = 0;
            this.btnHrLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHrLine.Location = new System.Drawing.Point(4, 67);
            this.btnHrLine.Name = "btnHrLine";
            this.btnHrLine.Size = new System.Drawing.Size(26, 26);
            this.btnHrLine.TabIndex = 3;
            this.toolTipScatter.SetToolTip(this.btnHrLine, "等心率线");
            this.btnHrLine.UseVisualStyleBackColor = false;
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.btnSelect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelect.FlatAppearance.BorderSize = 0;
            this.btnSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelect.Location = new System.Drawing.Point(4, 3);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(26, 26);
            this.btnSelect.TabIndex = 4;
            this.toolTipScatter.SetToolTip(this.btnSelect, "选框方式");
            this.btnSelect.UseVisualStyleBackColor = false;
            // 
            // btnScatterType
            // 
            this.btnScatterType.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnScatterType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.btnScatterType.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScatterType.FlatAppearance.BorderSize = 0;
            this.btnScatterType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScatterType.Location = new System.Drawing.Point(4, 35);
            this.btnScatterType.Name = "btnScatterType";
            this.btnScatterType.Size = new System.Drawing.Size(26, 26);
            this.btnScatterType.TabIndex = 5;
            this.toolTipScatter.SetToolTip(this.btnScatterType, "散点图类型");
            this.btnScatterType.UseVisualStyleBackColor = false;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(460, 460);
            this.panel2.TabIndex = 1;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "polyline.png");
            this.imageList1.Images.SetKeyName(1, "rect.png");
            this.imageList1.Images.SetKeyName(2, "scatterNormal.png");
            this.imageList1.Images.SetKeyName(3, "scatterDiff.png");
            this.imageList1.Images.SetKeyName(4, "hrLine.png");
            // 
            // ctxMenuStrip
            // 
            this.ctxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.mnuBeatDelete,
            this.mnuShowColor,
            this.mnuAddBlockedPAC});
            this.ctxMenuStrip.Name = "ctxMenuStrip";
            this.ctxMenuStrip.Size = new System.Drawing.Size(161, 76);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(157, 6);
            // 
            // mnuBeatDelete
            // 
            this.mnuBeatDelete.Name = "mnuBeatDelete";
            this.mnuBeatDelete.Size = new System.Drawing.Size(160, 22);
            this.mnuBeatDelete.Text = "删除心搏";
            // 
            // mnuShowColor
            // 
            this.mnuShowColor.Name = "mnuShowColor";
            this.mnuShowColor.Size = new System.Drawing.Size(160, 22);
            this.mnuShowColor.Text = "显示颜色标识";
            // 
            // mnuAddBlockedPAC
            // 
            this.mnuAddBlockedPAC.Name = "mnuAddBlockedPAC";
            this.mnuAddBlockedPAC.Size = new System.Drawing.Size(160, 22);
            this.mnuAddBlockedPAC.Text = "添加房早未下传";
            // 
            // ScatterPlot
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tlpTool);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "ScatterPlot";
            this.Size = new System.Drawing.Size(494, 460);
            this.Load += new System.EventHandler(this.ScatterPlot_Load);
            this.SizeChanged += new System.EventHandler(this.ScatterPlot_SizeChanged);
            this.tlpTool.ResumeLayout(false);
            this.ctxMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tlpTool;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btnHrLine;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnScatterType;
        private System.Windows.Forms.ContextMenuStrip ctxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuBeatDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuShowColor;
        private System.Windows.Forms.ToolStripMenuItem mnuAddBlockedPAC;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolTip toolTipScatter;
    }
}
