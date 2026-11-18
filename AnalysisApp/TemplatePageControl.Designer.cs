namespace AnalysisApp
{
    partial class TemplatePageControl
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
            this.tsBeatType = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.drawEcgCommonWave1 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.templateWaveViewControl1 = new AnalysisApp.CommonControls.TemplateWaveViewControl();
            this.subTemplateControl1 = new AnalysisApp.CommonControls.SubTemplateControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.tsBeatType.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tsBeatType, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.subTemplateControl1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1196, 667);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tsBeatType
            // 
            this.tsBeatType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tsBeatType.Dock = System.Windows.Forms.DockStyle.Left;
            this.tsBeatType.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsBeatType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.tsBeatType.Location = new System.Drawing.Point(0, 0);
            this.tsBeatType.MinimumSize = new System.Drawing.Size(48, 0);
            this.tsBeatType.Name = "tsBeatType";
            this.tsBeatType.Size = new System.Drawing.Size(53, 667);
            this.tsBeatType.TabIndex = 2;
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.AutoSize = false;
            this.toolStripButton1.Image = global::AnalysisApp.Properties.Resources.templateLeadSet;
            this.toolStripButton1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(52, 39);
            this.toolStripButton1.Text = "0";
            this.toolStripButton1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStripButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.drawEcgCommonWave1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.templateWaveViewControl1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(182, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1011, 661);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // drawEcgCommonWave1
            // 
            this.drawEcgCommonWave1.BackColor = System.Drawing.Color.White;
            this.drawEcgCommonWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawEcgCommonWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.drawEcgCommonWave1.Location = new System.Drawing.Point(3, 465);
            this.drawEcgCommonWave1.Name = "drawEcgCommonWave1";
            this.drawEcgCommonWave1.Size = new System.Drawing.Size(1005, 193);
            this.drawEcgCommonWave1.TabIndex = 0;
            // 
            // templateWaveViewControl1
            // 
            this.templateWaveViewControl1.BackColor = System.Drawing.Color.White;
            this.templateWaveViewControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.templateWaveViewControl1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.templateWaveViewControl1.LayoutStyle = AnalysisApp.Commons.BeatWaveStyle.OneLead;
            this.templateWaveViewControl1.Location = new System.Drawing.Point(3, 3);
            this.templateWaveViewControl1.Name = "templateWaveViewControl1";
            this.templateWaveViewControl1.PageIdx = 0;
            this.templateWaveViewControl1.Size = new System.Drawing.Size(1005, 456);
            this.templateWaveViewControl1.TabIndex = 1;
            // 
            // subTemplateControl1
            // 
            this.subTemplateControl1.BackColor = System.Drawing.Color.White;
            this.subTemplateControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.subTemplateControl1.Location = new System.Drawing.Point(53, 3);
            this.subTemplateControl1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.subTemplateControl1.Name = "subTemplateControl1";
            this.subTemplateControl1.Size = new System.Drawing.Size(126, 661);
            this.subTemplateControl1.TabIndex = 4;
            // 
            // TemplatePageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TemplatePageControl";
            this.Size = new System.Drawing.Size(1196, 667);
            this.Load += new System.EventHandler(this.TemplatePageControl_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tsBeatType.ResumeLayout(false);
            this.tsBeatType.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip tsBeatType;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private CommonControls.DrawEcgCommonWave drawEcgCommonWave1;
        private CommonControls.SubTemplateControl subTemplateControl1;
        private CommonControls.TemplateWaveViewControl templateWaveViewControl1;
    }
}
