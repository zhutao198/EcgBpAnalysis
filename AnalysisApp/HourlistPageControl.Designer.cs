namespace AnalysisApp
{
    partial class HourlistPageControl
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HourlistPageControl));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.headerlessTabControl1 = new Zoncare.Component.Controls.ZqTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvEcg = new Zoncare.Component.Controls.DataGridViewEx();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvBP = new Zoncare.Component.Controls.DataGridViewEx();
            this.headerlessTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEcg)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBP)).BeginInit();
            this.SuspendLayout();
            // 
            // headerlessTabControl1
            // 
            this.headerlessTabControl1.Controls.Add(this.tabPage1);
            this.headerlessTabControl1.Controls.Add(this.tabPage2);
            this.headerlessTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerlessTabControl1.Location = new System.Drawing.Point(0, 0);
            this.headerlessTabControl1.Name = "headerlessTabControl1";
            this.headerlessTabControl1.SelectedIndex = 0;
            this.headerlessTabControl1.Size = new System.Drawing.Size(1125, 617);
            this.headerlessTabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgvEcg);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1117, 584);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "动态心电";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvEcg
            // 
            this.dgvEcg.ActionColumnHeaderText = null;
            this.dgvEcg.ActionColumnWidth = 0;
            this.dgvEcg.AllowUserToAddRows = false;
            this.dgvEcg.AllowUserToDeleteRows = false;
            this.dgvEcg.AllowUserToResizeRows = false;
            this.dgvEcg.AlternateRowColor = true;
            this.dgvEcg.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvEcg.BackgroundColor = System.Drawing.Color.White;
            this.dgvEcg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvEcg.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvEcg.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvEcg.ColumnHeadersHeight = 36;
            this.dgvEcg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvEcg.CustomDateTimeFormat = null;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Lavender;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvEcg.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvEcg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEcg.EnableHeadersVisualStyles = false;
            this.dgvEcg.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgvEcg.HighlightRowHover = true;
            this.dgvEcg.Location = new System.Drawing.Point(3, 3);
            this.dgvEcg.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvEcg.MergeColumnNames")));
            this.dgvEcg.MultiSelect = false;
            this.dgvEcg.Name = "dgvEcg";
            this.dgvEcg.ReadOnly = true;
            this.dgvEcg.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvEcg.RowHeadersVisible = false;
            this.dgvEcg.RowTemplate.Height = 30;
            this.dgvEcg.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvEcg.Size = new System.Drawing.Size(1111, 578);
            this.dgvEcg.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvEcg.TabIndex = 2;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvBP);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1117, 584);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "动态血压";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvBP
            // 
            this.dgvBP.ActionColumnHeaderText = null;
            this.dgvBP.ActionColumnWidth = 0;
            this.dgvBP.AllowUserToAddRows = false;
            this.dgvBP.AllowUserToDeleteRows = false;
            this.dgvBP.AllowUserToResizeRows = false;
            this.dgvBP.AlternateRowColor = true;
            this.dgvBP.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBP.BackgroundColor = System.Drawing.Color.White;
            this.dgvBP.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvBP.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvBP.ColumnHeadersHeight = 36;
            this.dgvBP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvBP.CustomDateTimeFormat = null;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.Lavender;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvBP.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvBP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBP.EnableHeadersVisualStyles = false;
            this.dgvBP.GridColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.dgvBP.Location = new System.Drawing.Point(3, 3);
            this.dgvBP.MergeColumnNames = ((System.Collections.Generic.List<string>)(resources.GetObject("dgvBP.MergeColumnNames")));
            this.dgvBP.Name = "dgvBP";
            this.dgvBP.RowHeadersVisible = false;
            this.dgvBP.RowTemplate.Height = 30;
            this.dgvBP.Size = new System.Drawing.Size(1111, 578);
            this.dgvBP.SpanColumnHeaderBackColor = System.Drawing.Color.Empty;
            this.dgvBP.TabIndex = 0;
            // 
            // HourlistPageControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.headerlessTabControl1);
            this.Name = "HourlistPageControl";
            this.Size = new System.Drawing.Size(1125, 617);
            this.Load += new System.EventHandler(this.HourlistPageControl_Load);
            this.headerlessTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvEcg)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBP)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Zoncare.Component.Controls.ZqTabControl headerlessTabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private Zoncare.Component.Controls.DataGridViewEx dgvBP;
        private System.Windows.Forms.TabPage tabPage1;
        private Zoncare.Component.Controls.DataGridViewEx dgvEcg;
    }
}
