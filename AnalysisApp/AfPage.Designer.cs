namespace AnalysisApp
{
    partial class AfPage
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AfPage));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dgvData = new Zoncare.Component.Controls.DataGridViewEx();
            this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            this.btnExcluedAPB = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnDetect = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnDel = new Zoncare.Component.Controls.RoundButton(this.components);
            this.scatterPlot2 = new AnalysisApp.CommonControls.ScatterPlot();
            this.tableLayoutPanel12 = new System.Windows.Forms.TableLayoutPanel();
            this.scatterTrend2 = new AnalysisApp.CommonControls.ScatterTrend();
            this.scatterTrend1 = new AnalysisApp.CommonControls.ScatterTrend();
            this.ecgCommonWave2 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tableLayoutPanel11.SuspendLayout();
            this.tableLayoutPanel12.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 365F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.scatterPlot2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel12, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1366, 406);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dgvData);
            this.panel2.Controls.Add(this.tableLayoutPanel11);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(359, 400);
            this.panel2.TabIndex = 1;
            // 
            // dgvData
            // 
            this.dgvData.ActionColumnHeaderText = null;
            this.dgvData.ActionColumnWidth = 0;
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            this.dgvData.AlternateRowColor = true;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvData.BackgroundColor = System.Drawing.Color.White;
            this.dgvData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvData.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvData.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.ColumnHeadersHeight = 36;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvData.CustomDateTimeFormat = null;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.Lavender;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.EnableHeadersVisualStyles = false;
            this.dgvData.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgvData.HighlightRowHover = true;
            this.dgvData.Location = new System.Drawing.Point(0, 49);
            this.dgvData.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvData.MergeColumnNames")));
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 30;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.ShowSelectionColor = true;
            this.dgvData.Size = new System.Drawing.Size(359, 351);
            this.dgvData.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvData.TabIndex = 2;
            this.dgvData.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvData_CellFormatting);
            this.dgvData.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_CellMouseClick);
            this.dgvData.SelectionChanged += new System.EventHandler(this.dgvData_SelectionChanged);
            // 
            // tableLayoutPanel11
            // 
            this.tableLayoutPanel11.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel11.ColumnCount = 3;
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel11.Controls.Add(this.btnExcluedAPB, 0, 0);
            this.tableLayoutPanel11.Controls.Add(this.btnDetect, 0, 0);
            this.tableLayoutPanel11.Controls.Add(this.btnDel, 0, 0);
            this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel11.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel11.Name = "tableLayoutPanel11";
            this.tableLayoutPanel11.RowCount = 1;
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel11.Size = new System.Drawing.Size(359, 49);
            this.tableLayoutPanel11.TabIndex = 0;
            // 
            // btnExcluedAPB
            // 
            this.btnExcluedAPB.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnExcluedAPB.AutoSize = true;
            this.btnExcluedAPB.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnExcluedAPB.DataFlag = null;
            this.btnExcluedAPB.FollowBaseColor = false;
            this.btnExcluedAPB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnExcluedAPB.IsHighlight = true;
            this.btnExcluedAPB.Location = new System.Drawing.Point(225, 9);
            this.btnExcluedAPB.Name = "btnExcluedAPB";
            this.btnExcluedAPB.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnExcluedAPB.Size = new System.Drawing.Size(131, 30);
            this.btnExcluedAPB.TabIndex = 10;
            this.btnExcluedAPB.Text = "去除房颤内的房早";
            this.btnExcluedAPB.UseVisualStyleBackColor = true;
            this.btnExcluedAPB.Click += new System.EventHandler(this.btnExcluedAPB_Click);
            // 
            // btnDetect
            // 
            this.btnDetect.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDetect.AutoSize = true;
            this.btnDetect.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnDetect.DataFlag = null;
            this.btnDetect.FollowBaseColor = false;
            this.btnDetect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnDetect.IsHighlight = true;
            this.btnDetect.Location = new System.Drawing.Point(144, 9);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDetect.Size = new System.Drawing.Size(75, 30);
            this.btnDetect.TabIndex = 9;
            this.btnDetect.Text = "检测房颤";
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // btnDel
            // 
            this.btnDel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnDel.AutoSize = true;
            this.btnDel.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnDel.DataFlag = null;
            this.btnDel.FollowBaseColor = false;
            this.btnDel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnDel.IsHighlight = true;
            this.btnDel.Location = new System.Drawing.Point(3, 9);
            this.btnDel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.btnDel.Name = "btnDel";
            this.btnDel.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnDel.Size = new System.Drawing.Size(47, 30);
            this.btnDel.TabIndex = 8;
            this.btnDel.Text = "删除";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.AutoSize = true;
            this.scatterPlot2.BackColor = System.Drawing.Color.White;
            this.scatterPlot2.Dock = System.Windows.Forms.DockStyle.Right;
            this.scatterPlot2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.scatterPlot2.HideToolStrip = true;
            this.scatterPlot2.Location = new System.Drawing.Point(903, 3);
            this.scatterPlot2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.scatterPlot2.Name = "scatterPlot2";
            this.scatterPlot2.Size = new System.Drawing.Size(460, 400);
            this.scatterPlot2.TabIndex = 1;
            // 
            // tableLayoutPanel12
            // 
            this.tableLayoutPanel12.ColumnCount = 1;
            this.tableLayoutPanel12.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.Controls.Add(this.scatterTrend2, 0, 1);
            this.tableLayoutPanel12.Controls.Add(this.scatterTrend1, 0, 0);
            this.tableLayoutPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel12.Location = new System.Drawing.Point(365, 0);
            this.tableLayoutPanel12.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel12.Name = "tableLayoutPanel12";
            this.tableLayoutPanel12.RowCount = 2;
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel12.Size = new System.Drawing.Size(538, 406);
            this.tableLayoutPanel12.TabIndex = 2;
            // 
            // scatterTrend2
            // 
            this.scatterTrend2.AFList = null;
            this.scatterTrend2.BackColor = System.Drawing.Color.White;
            this.scatterTrend2.DefaultSelectLength = 3600F;
            this.scatterTrend2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scatterTrend2.EndTime = new System.DateTime(((long)(0)));
            this.scatterTrend2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.scatterTrend2.Location = new System.Drawing.Point(3, 206);
            this.scatterTrend2.Name = "scatterTrend2";
            this.scatterTrend2.SelectMode = 1;
            this.scatterTrend2.Size = new System.Drawing.Size(532, 197);
            this.scatterTrend2.StartTime = new System.DateTime(((long)(0)));
            this.scatterTrend2.TabIndex = 2;
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
            this.scatterTrend1.Size = new System.Drawing.Size(532, 197);
            this.scatterTrend1.StartTime = new System.DateTime(((long)(0)));
            this.scatterTrend1.TabIndex = 1;
            // 
            // ecgCommonWave2
            // 
            this.ecgCommonWave2.BackColor = System.Drawing.Color.White;
            this.ecgCommonWave2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ecgCommonWave2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ecgCommonWave2.Location = new System.Drawing.Point(0, 406);
            this.ecgCommonWave2.Name = "ecgCommonWave2";
            this.ecgCommonWave2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.ecgCommonWave2.Size = new System.Drawing.Size(1366, 362);
            this.ecgCommonWave2.TabIndex = 4;
            // 
            // AfPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ecgCommonWave2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AfPage";
            this.Size = new System.Drawing.Size(1366, 768);
            this.Load += new System.EventHandler(this.ScatterPage_Load);
            this.SizeChanged += new System.EventHandler(this.ScatterPage_SizeChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tableLayoutPanel11.ResumeLayout(false);
            this.tableLayoutPanel11.PerformLayout();
            this.tableLayoutPanel12.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private CommonControls.ScatterPlot scatterPlot2;
        private CommonControls.DrawEcgCommonWave ecgCommonWave2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel11;
        private Zoncare.Component.Controls.DataGridViewEx dgvData;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel12;
        private CommonControls.ScatterTrend scatterTrend2;
        private CommonControls.ScatterTrend scatterTrend1;
        private Zoncare.Component.Controls.RoundButton btnExcluedAPB;
        private Zoncare.Component.Controls.RoundButton btnDetect;
        private Zoncare.Component.Controls.RoundButton btnDel;
    }
}
