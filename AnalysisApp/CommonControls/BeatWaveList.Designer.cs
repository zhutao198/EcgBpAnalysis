namespace AnalysisApp.CommonControls
{
    partial class BeatWaveList
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cbxLead = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.cbxLayout = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblPage = new System.Windows.Forms.Label();
            this.cbxGain = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.btnSelectCurPage = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnSelectAll = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnCommonWave = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnLeadSet = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnHistogram = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnBatchAddBeat = new Zoncare.Component.Controls.RoundButton(this.components);
            this.ctxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuBeatDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuModifyPrevious = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuModifyLatter = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuInsertBefore = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuInsertAfter = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddCustomEvent = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOrderbyTime = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOrderbyValueAhead = new System.Windows.Forms.ToolStripMenuItem();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.ctxMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(925, 515);
            this.panel1.TabIndex = 1;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.Location = new System.Drawing.Point(925, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(12, 515);
            this.vScrollBar1.TabIndex = 2;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            this.vScrollBar1.ValueChanged += new System.EventHandler(this.vScrollBar1_ValueChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.vScrollBar1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 30);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(937, 515);
            this.panel2.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 14;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.cbxLead, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.label4, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.cbxLayout, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblPage, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cbxGain, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSelectCurPage, 9, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSelectAll, 10, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnCommonWave, 12, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnLeadSet, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnHistogram, 13, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnBatchAddBeat, 11, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(937, 30);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // cbxLead
            // 
            this.cbxLead.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLead.CheckedListBox = null;
            this.cbxLead.Control = null;
            this.cbxLead.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxLead.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxLead.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxLead.FormattingEnabled = true;
            this.cbxLead.IsShown = false;
            this.cbxLead.IsUnresponsive = false;
            this.cbxLead.Location = new System.Drawing.Point(343, 4);
            this.cbxLead.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.cbxLead.Name = "cbxLead";
            this.cbxLead.PlaceHolderStr = null;
            this.cbxLead.Size = new System.Drawing.Size(90, 24);
            this.cbxLead.TabIndex = 2;
            this.cbxLead.TreeView = null;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(311, 6);
            this.label4.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 17);
            this.label4.TabIndex = 5;
            this.label4.Text = "导联";
            // 
            // cbxLayout
            // 
            this.cbxLayout.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxLayout.CheckedListBox = null;
            this.cbxLayout.Control = null;
            this.cbxLayout.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxLayout.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxLayout.FormattingEnabled = true;
            this.cbxLayout.IsShown = false;
            this.cbxLayout.IsUnresponsive = false;
            this.cbxLayout.Location = new System.Drawing.Point(211, 4);
            this.cbxLayout.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.cbxLayout.Name = "cbxLayout";
            this.cbxLayout.PlaceHolderStr = null;
            this.cbxLayout.Size = new System.Drawing.Size(90, 24);
            this.cbxLayout.TabIndex = 1;
            this.cbxLayout.TreeView = null;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(179, 6);
            this.label3.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "布局";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "增益";
            // 
            // lblPage
            // 
            this.lblPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPage.AutoSize = true;
            this.lblPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.lblPage.Location = new System.Drawing.Point(0, 6);
            this.lblPage.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.lblPage.Name = "lblPage";
            this.lblPage.Size = new System.Drawing.Size(27, 17);
            this.lblPage.TabIndex = 0;
            this.lblPage.Text = "1/1";
            // 
            // cbxGain
            // 
            this.cbxGain.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbxGain.CheckedListBox = null;
            this.cbxGain.Control = null;
            this.cbxGain.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxGain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxGain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxGain.FormattingEnabled = true;
            this.cbxGain.IsShown = false;
            this.cbxGain.IsUnresponsive = false;
            this.cbxGain.Location = new System.Drawing.Point(79, 4);
            this.cbxGain.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.cbxGain.Name = "cbxGain";
            this.cbxGain.PlaceHolderStr = null;
            this.cbxGain.Size = new System.Drawing.Size(90, 24);
            this.cbxGain.TabIndex = 0;
            this.cbxGain.TreeView = null;
            // 
            // btnSelectCurPage
            // 
            this.btnSelectCurPage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnSelectCurPage.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnSelectCurPage.DataFlag = null;
            this.btnSelectCurPage.FollowBaseColor = false;
            this.btnSelectCurPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnSelectCurPage.IsHighlight = true;
            this.btnSelectCurPage.Location = new System.Drawing.Point(701, 3);
            this.btnSelectCurPage.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnSelectCurPage.Name = "btnSelectCurPage";
            this.btnSelectCurPage.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSelectCurPage.Size = new System.Drawing.Size(48, 24);
            this.btnSelectCurPage.TabIndex = 4;
            this.btnSelectCurPage.Text = "当前页";
            this.btnSelectCurPage.UseVisualStyleBackColor = true;
            this.btnSelectCurPage.Click += new System.EventHandler(this.btnSelectCurPage_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnSelectAll.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnSelectAll.DataFlag = null;
            this.btnSelectAll.FollowBaseColor = false;
            this.btnSelectAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnSelectAll.IsHighlight = true;
            this.btnSelectAll.Location = new System.Drawing.Point(752, 3);
            this.btnSelectAll.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSelectAll.Size = new System.Drawing.Size(36, 24);
            this.btnSelectAll.TabIndex = 5;
            this.btnSelectAll.Text = "全选";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnCommonWave
            // 
            this.btnCommonWave.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCommonWave.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnCommonWave.DataFlag = null;
            this.btnCommonWave.FollowBaseColor = false;
            this.btnCommonWave.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnCommonWave.Image = global::AnalysisApp.Properties.Resources.showThreeWave;
            this.btnCommonWave.IsHighlight = true;
            this.btnCommonWave.Location = new System.Drawing.Point(878, 3);
            this.btnCommonWave.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnCommonWave.Name = "btnCommonWave";
            this.btnCommonWave.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCommonWave.Size = new System.Drawing.Size(28, 23);
            this.btnCommonWave.TabIndex = 7;
            this.btnCommonWave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCommonWave.UseVisualStyleBackColor = true;
            // 
            // btnLeadSet
            // 
            this.btnLeadSet.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnLeadSet.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnLeadSet.DataFlag = null;
            this.btnLeadSet.FollowBaseColor = false;
            this.btnLeadSet.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnLeadSet.Image = global::AnalysisApp.Properties.Resources.templateLeadSet;
            this.btnLeadSet.IsHighlight = true;
            this.btnLeadSet.Location = new System.Drawing.Point(436, 3);
            this.btnLeadSet.Name = "btnLeadSet";
            this.btnLeadSet.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnLeadSet.Size = new System.Drawing.Size(28, 23);
            this.btnLeadSet.TabIndex = 3;
            this.btnLeadSet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLeadSet.UseVisualStyleBackColor = true;
            this.btnLeadSet.Click += new System.EventHandler(this.btnLeadSet_Click);
            // 
            // btnHistogram
            // 
            this.btnHistogram.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnHistogram.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnHistogram.DataFlag = null;
            this.btnHistogram.FollowBaseColor = false;
            this.btnHistogram.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnHistogram.Image = global::AnalysisApp.Properties.Resources.eventHis;
            this.btnHistogram.IsHighlight = true;
            this.btnHistogram.Location = new System.Drawing.Point(909, 3);
            this.btnHistogram.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnHistogram.Name = "btnHistogram";
            this.btnHistogram.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnHistogram.Size = new System.Drawing.Size(28, 23);
            this.btnHistogram.TabIndex = 8;
            this.btnHistogram.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHistogram.UseVisualStyleBackColor = true;
            // 
            // btnBatchAddBeat
            // 
            this.btnBatchAddBeat.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBatchAddBeat.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnBatchAddBeat.DataFlag = null;
            this.btnBatchAddBeat.FollowBaseColor = false;
            this.btnBatchAddBeat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnBatchAddBeat.IsHighlight = true;
            this.btnBatchAddBeat.Location = new System.Drawing.Point(791, 3);
            this.btnBatchAddBeat.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnBatchAddBeat.Name = "btnBatchAddBeat";
            this.btnBatchAddBeat.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnBatchAddBeat.Size = new System.Drawing.Size(84, 24);
            this.btnBatchAddBeat.TabIndex = 6;
            this.btnBatchAddBeat.Text = "批量插入心搏";
            this.btnBatchAddBeat.UseVisualStyleBackColor = true;
            // 
            // ctxMenuStrip
            // 
            this.ctxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.mnuBeatDelete,
            this.mnuModifyPrevious,
            this.mnuModifyLatter,
            this.mnuInsertBefore,
            this.mnuInsertAfter,
            this.mnuAddCustomEvent,
            this.mnuOrderbyTime,
            this.mnuOrderbyValueAhead});
            this.ctxMenuStrip.Name = "ctxMenuStrip";
            this.ctxMenuStrip.Size = new System.Drawing.Size(185, 186);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
            // 
            // mnuBeatDelete
            // 
            this.mnuBeatDelete.Name = "mnuBeatDelete";
            this.mnuBeatDelete.Size = new System.Drawing.Size(184, 22);
            this.mnuBeatDelete.Text = "删除心搏";
            // 
            // mnuModifyPrevious
            // 
            this.mnuModifyPrevious.Name = "mnuModifyPrevious";
            this.mnuModifyPrevious.Size = new System.Drawing.Size(184, 22);
            this.mnuModifyPrevious.Text = "修改前一个心搏属性";
            // 
            // mnuModifyLatter
            // 
            this.mnuModifyLatter.Name = "mnuModifyLatter";
            this.mnuModifyLatter.Size = new System.Drawing.Size(184, 22);
            this.mnuModifyLatter.Text = "修改后一个心搏属性";
            // 
            // mnuInsertBefore
            // 
            this.mnuInsertBefore.Name = "mnuInsertBefore";
            this.mnuInsertBefore.Size = new System.Drawing.Size(184, 22);
            this.mnuInsertBefore.Text = "向前插入心搏";
            // 
            // mnuInsertAfter
            // 
            this.mnuInsertAfter.Name = "mnuInsertAfter";
            this.mnuInsertAfter.Size = new System.Drawing.Size(184, 22);
            this.mnuInsertAfter.Text = "向后插入心搏";
            // 
            // mnuAddCustomEvent
            // 
            this.mnuAddCustomEvent.Name = "mnuAddCustomEvent";
            this.mnuAddCustomEvent.Size = new System.Drawing.Size(184, 22);
            this.mnuAddCustomEvent.Text = "添加自定义事件";
            // 
            // mnuOrderbyTime
            // 
            this.mnuOrderbyTime.Name = "mnuOrderbyTime";
            this.mnuOrderbyTime.Size = new System.Drawing.Size(184, 22);
            this.mnuOrderbyTime.Text = "按时间排序";
            // 
            // mnuOrderbyValueAhead
            // 
            this.mnuOrderbyValueAhead.Name = "mnuOrderbyValueAhead";
            this.mnuOrderbyValueAhead.Size = new System.Drawing.Size(184, 22);
            this.mnuOrderbyValueAhead.Text = "按提前量排序";
            // 
            // BeatWaveList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "BeatWaveList";
            this.Size = new System.Drawing.Size(937, 545);
            this.Load += new System.EventHandler(this.BeatWaveList_Load);
            this.SizeChanged += new System.EventHandler(this.BeatWaveList_SizeChanged);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ctxMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblPage;
        private Zoncare.Component.Controls.ZoncareComboBox cbxGain;
        private Zoncare.Component.Controls.ZoncareComboBox cbxLead;
        private System.Windows.Forms.Label label4;
        private Zoncare.Component.Controls.ZoncareComboBox cbxLayout;
        private System.Windows.Forms.Label label3;
        private Zoncare.Component.Controls.RoundButton btnSelectCurPage;
        private Zoncare.Component.Controls.RoundButton btnSelectAll;
        private Zoncare.Component.Controls.RoundButton btnCommonWave;
        private Zoncare.Component.Controls.RoundButton btnLeadSet;
        private System.Windows.Forms.ContextMenuStrip ctxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuModifyPrevious;
        private System.Windows.Forms.ToolStripMenuItem mnuModifyLatter;
        private System.Windows.Forms.ToolStripMenuItem mnuInsertBefore;
        private System.Windows.Forms.ToolStripMenuItem mnuInsertAfter;
        private System.Windows.Forms.ToolStripMenuItem mnuAddCustomEvent;
        private System.Windows.Forms.ToolStripMenuItem mnuBeatDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuOrderbyTime;
        private System.Windows.Forms.ToolStripMenuItem mnuOrderbyValueAhead;
        private Zoncare.Component.Controls.RoundButton btnHistogram;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Zoncare.Component.Controls.RoundButton btnBatchAddBeat;
    }
}
