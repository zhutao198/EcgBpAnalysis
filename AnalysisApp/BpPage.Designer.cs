namespace AnalysisApp
{
    partial class BpAnalysisPage
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BpAnalysisPage));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvBP = new Zoncare.Component.Controls.DataGridViewEx();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.drawBpTrendWave1 = new AnalysisApp.CommonControls.BpTrendWaveControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.notes = new System.Windows.Forms.RichTextBox();
            this.label43 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.bpHistogramControl1 = new AnalysisApp.CommonControls.BpHistogramControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.bpPieChatControl1 = new AnalysisApp.CommonControls.BpPieChatControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.bpFitPlotControl1 = new AnalysisApp.CommonControls.BpFitPlotControl();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.bpWaveChart1 = new AnalysisApp.CommonControls.BpWaveChart();
            this.panel2 = new System.Windows.Forms.Panel();
            this.drawEcgCommonWave1 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnDiscard = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnRestore = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnCompare = new Zoncare.Component.Controls.RoundButton(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBP)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvBP
            // 
            this.dgvBP.ActionColumnHeaderText = null;
            this.dgvBP.ActionColumnWidth = 0;
            this.dgvBP.AllowUserToAddRows = false;
            this.dgvBP.AllowUserToDeleteRows = false;
            this.dgvBP.AllowUserToResizeRows = false;
            this.dgvBP.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBP.BackgroundColor = System.Drawing.Color.White;
            this.dgvBP.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvBP.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvBP.ColumnHeadersHeight = 36;
            this.dgvBP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvBP.CustomDateTimeFormat = null;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.Lavender;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvBP.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvBP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBP.EnableHeadersVisualStyles = false;
            this.dgvBP.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgvBP.HighlightRowHover = true;
            this.dgvBP.Location = new System.Drawing.Point(0, 34);
            this.dgvBP.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvBP.MergeColumnNames")));
            this.dgvBP.Name = "dgvBP";
            this.dgvBP.RowHeadersVisible = false;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.White;
            this.dgvBP.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvBP.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgvBP.RowTemplate.Height = 30;
            this.dgvBP.ShowSelectionColor = true;
            this.dgvBP.Size = new System.Drawing.Size(658, 734);
            this.dgvBP.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvBP.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(704, 768);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tabPage1.Controls.Add(this.tableLayoutPanel3);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(696, 735);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "趋势图";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.drawBpTrendWave1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(690, 729);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // drawBpTrendWave1
            // 
            this.drawBpTrendWave1.BackColor = System.Drawing.Color.White;
            this.drawBpTrendWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawBpTrendWave1.Location = new System.Drawing.Point(3, 3);
            this.drawBpTrendWave1.Name = "drawBpTrendWave1";
            this.drawBpTrendWave1.Size = new System.Drawing.Size(684, 577);
            this.drawBpTrendWave1.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.notes);
            this.panel1.Controls.Add(this.label43);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 586);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(684, 140);
            this.panel1.TabIndex = 3;
            // 
            // notes
            // 
            this.notes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.notes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notes.Location = new System.Drawing.Point(0, 26);
            this.notes.Name = "notes";
            this.notes.Size = new System.Drawing.Size(684, 114);
            this.notes.TabIndex = 4;
            this.notes.Text = "";
            this.notes.TextChanged += new System.EventHandler(this.notes_TextChanged);
            // 
            // label43
            // 
            this.label43.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.label43.Dock = System.Windows.Forms.DockStyle.Top;
            this.label43.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label43.Location = new System.Drawing.Point(0, 0);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(684, 26);
            this.label43.TabIndex = 3;
            this.label43.Text = "备注";
            this.label43.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.bpHistogramControl1);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(696, 735);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "直方图";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // bpHistogramControl1
            // 
            this.bpHistogramControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bpHistogramControl1.Location = new System.Drawing.Point(3, 3);
            this.bpHistogramControl1.Name = "bpHistogramControl1";
            this.bpHistogramControl1.Size = new System.Drawing.Size(690, 729);
            this.bpHistogramControl1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.bpPieChatControl1);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(696, 735);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "饼图";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // bpPieChatControl1
            // 
            this.bpPieChatControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bpPieChatControl1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.bpPieChatControl1.Location = new System.Drawing.Point(3, 3);
            this.bpPieChatControl1.Name = "bpPieChatControl1";
            this.bpPieChatControl1.Size = new System.Drawing.Size(690, 729);
            this.bpPieChatControl1.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.bpFitPlotControl1);
            this.tabPage4.Location = new System.Drawing.Point(4, 29);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(696, 735);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "拟合图";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // bpFitPlotControl1
            // 
            this.bpFitPlotControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bpFitPlotControl1.Location = new System.Drawing.Point(3, 3);
            this.bpFitPlotControl1.Name = "bpFitPlotControl1";
            this.bpFitPlotControl1.Size = new System.Drawing.Size(690, 729);
            this.bpFitPlotControl1.TabIndex = 0;
            // 
            // tabPage5
            // 
            this.tabPage5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tabPage5.Controls.Add(this.bpWaveChart1);
            this.tabPage5.Controls.Add(this.panel2);
            this.tabPage5.Location = new System.Drawing.Point(4, 29);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(696, 735);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "血压波形";
            // 
            // bpWaveChart1
            // 
            this.bpWaveChart1.AutoSize = true;
            this.bpWaveChart1.BackColor = System.Drawing.Color.White;
            this.bpWaveChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bpWaveChart1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bpWaveChart1.Location = new System.Drawing.Point(3, 3);
            this.bpWaveChart1.Name = "bpWaveChart1";
            this.bpWaveChart1.Size = new System.Drawing.Size(690, 487);
            this.bpWaveChart1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.drawEcgCommonWave1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(3, 490);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.panel2.Size = new System.Drawing.Size(690, 242);
            this.panel2.TabIndex = 2;
            // 
            // drawEcgCommonWave1
            // 
            this.drawEcgCommonWave1.BackColor = System.Drawing.Color.White;
            this.drawEcgCommonWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawEcgCommonWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.drawEcgCommonWave1.Location = new System.Drawing.Point(0, 6);
            this.drawEcgCommonWave1.Name = "drawEcgCommonWave1";
            this.drawEcgCommonWave1.Size = new System.Drawing.Size(690, 236);
            this.drawEcgCommonWave1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel2.Controls.Add(this.btnDiscard, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnRestore, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCompare, 3, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(658, 34);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // btnDiscard
            // 
            this.btnDiscard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDiscard.AutoSize = true;
            this.btnDiscard.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnDiscard.DataFlag = null;
            this.btnDiscard.FollowBaseColor = false;
            this.btnDiscard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnDiscard.IsHighlight = true;
            this.btnDiscard.Location = new System.Drawing.Point(88, 3);
            this.btnDiscard.Name = "btnDiscard";
            this.btnDiscard.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDiscard.Size = new System.Drawing.Size(64, 28);
            this.btnDiscard.TabIndex = 12;
            this.btnDiscard.Text = "排除";
            this.btnDiscard.UseVisualStyleBackColor = true;
            this.btnDiscard.Click += new System.EventHandler(this.btnDiscard_Click);
            // 
            // btnRestore
            // 
            this.btnRestore.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnRestore.AutoSize = true;
            this.btnRestore.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnRestore.DataFlag = null;
            this.btnRestore.FollowBaseColor = false;
            this.btnRestore.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnRestore.IsHighlight = true;
            this.btnRestore.Location = new System.Drawing.Point(3, 3);
            this.btnRestore.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnRestore.Size = new System.Drawing.Size(64, 28);
            this.btnRestore.TabIndex = 11;
            this.btnRestore.Text = "恢复";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnCompare
            // 
            this.btnCompare.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCompare.AutoSize = true;
            this.btnCompare.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnCompare.DataFlag = null;
            this.btnCompare.FollowBaseColor = false;
            this.btnCompare.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnCompare.IsHighlight = true;
            this.btnCompare.Location = new System.Drawing.Point(591, 3);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCompare.Size = new System.Drawing.Size(64, 28);
            this.btnCompare.TabIndex = 13;
            this.btnCompare.Text = "比较";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgvBP);
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(1366, 768);
            this.splitContainer1.SplitterDistance = 658;
            this.splitContainer1.TabIndex = 2;
            // 
            // BpAnalysisPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.splitContainer1);
            this.Name = "BpAnalysisPage";
            this.Size = new System.Drawing.Size(1366, 768);
            this.Load += new System.EventHandler(this.BpAnalysisPage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBP)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Zoncare.Component.Controls.DataGridViewEx dgvBP;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Zoncare.Component.Controls.RoundButton btnDiscard;
        private Zoncare.Component.Controls.RoundButton btnRestore;
        private Zoncare.Component.Controls.RoundButton btnCompare;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.RichTextBox notes;
        private CommonControls.BpHistogramControl bpHistogramControl1;
        private CommonControls.BpPieChatControl bpPieChatControl1;
        private CommonControls.BpFitPlotControl bpFitPlotControl1;
        private System.Windows.Forms.Panel panel1;
        private CommonControls.DrawEcgCommonWave drawEcgCommonWave1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private CommonControls.BpWaveChart bpWaveChart1;
        private System.Windows.Forms.Panel panel2;
        private CommonControls.BpTrendWaveControl drawBpTrendWave1;
    }
}
