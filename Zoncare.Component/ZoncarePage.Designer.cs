namespace Zoncare.Component
{
    partial class ZoncarePage
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
            this.tlpPaging = new System.Windows.Forms.TableLayoutPanel();
            this.btnLast = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnNext = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnPrevious = new Zoncare.Component.Controls.RoundButton(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.lblTotalPages = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblTotalRecords = new System.Windows.Forms.Label();
            this.lblRecord = new System.Windows.Forms.Label();
            this.btnFirst = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tbxPageIndex = new Zoncare.Component.Controls.TextBoxEx();
            this.cbxPageSize = new System.Windows.Forms.ComboBox();
            this.tlpPaging.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpPaging
            // 
            this.tlpPaging.BackColor = System.Drawing.Color.White;
            this.tlpPaging.ColumnCount = 12;
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpPaging.Controls.Add(this.btnLast, 7, 0);
            this.tlpPaging.Controls.Add(this.btnNext, 6, 0);
            this.tlpPaging.Controls.Add(this.btnPrevious, 2, 0);
            this.tlpPaging.Controls.Add(this.label1, 4, 0);
            this.tlpPaging.Controls.Add(this.lblTotalPages, 5, 0);
            this.tlpPaging.Controls.Add(this.lblTotal, 9, 0);
            this.tlpPaging.Controls.Add(this.lblTotalRecords, 10, 0);
            this.tlpPaging.Controls.Add(this.lblRecord, 11, 0);
            this.tlpPaging.Controls.Add(this.btnFirst, 1, 0);
            this.tlpPaging.Controls.Add(this.tbxPageIndex, 3, 0);
            this.tlpPaging.Controls.Add(this.cbxPageSize, 0, 0);
            this.tlpPaging.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPaging.ForeColor = System.Drawing.Color.Black;
            this.tlpPaging.Location = new System.Drawing.Point(0, 0);
            this.tlpPaging.Name = "tlpPaging";
            this.tlpPaging.RowCount = 1;
            this.tlpPaging.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPaging.Size = new System.Drawing.Size(970, 36);
            this.tlpPaging.TabIndex = 31;
            // 
            // btnLast
            // 
            this.btnLast.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnLast.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.btnLast.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnLast.DataFlag = null;
            this.btnLast.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnLast.Image = global::Zoncare.Component.Properties.Resources.nextAll;
            this.btnLast.IsHighlight = true;
            this.btnLast.Location = new System.Drawing.Point(395, 5);
            this.btnLast.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnLast.Name = "btnLast";
            this.btnLast.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnLast.Size = new System.Drawing.Size(36, 26);
            this.btnLast.TabIndex = 47;
            this.btnLast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLast.UseVisualStyleBackColor = false;
            this.btnLast.Click += new System.EventHandler(this.btnLast_Click);
            // 
            // btnNext
            // 
            this.btnNext.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.btnNext.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnNext.DataFlag = null;
            this.btnNext.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnNext.Image = global::Zoncare.Component.Properties.Resources.nextBtn;
            this.btnNext.IsHighlight = true;
            this.btnNext.Location = new System.Drawing.Point(346, 5);
            this.btnNext.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnNext.Size = new System.Drawing.Size(36, 26);
            this.btnNext.TabIndex = 46;
            this.btnNext.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnPrevious.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.btnPrevious.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnPrevious.DataFlag = null;
            this.btnPrevious.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnPrevious.Image = global::Zoncare.Component.Properties.Resources.prevBtn;
            this.btnPrevious.IsHighlight = true;
            this.btnPrevious.Location = new System.Drawing.Point(169, 5);
            this.btnPrevious.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnPrevious.Size = new System.Drawing.Size(36, 26);
            this.btnPrevious.TabIndex = 45;
            this.btnPrevious.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPrevious.UseVisualStyleBackColor = false;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(269, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 20);
            this.label1.TabIndex = 39;
            this.label1.Text = "/";
            // 
            // lblTotalPages
            // 
            this.lblTotalPages.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTotalPages.AutoSize = true;
            this.lblTotalPages.Location = new System.Drawing.Point(284, 8);
            this.lblTotalPages.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lblTotalPages.Name = "lblTotalPages";
            this.lblTotalPages.Size = new System.Drawing.Size(49, 20);
            this.lblTotalPages.TabIndex = 40;
            this.lblTotalPages.Text = "pages";
            // 
            // lblTotal
            // 
            this.lblTotal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(851, 8);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(23, 20);
            this.lblTotal.TabIndex = 41;
            this.lblTotal.Text = "共";
            // 
            // lblTotalRecords
            // 
            this.lblTotalRecords.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTotalRecords.AutoSize = true;
            this.lblTotalRecords.Location = new System.Drawing.Point(880, 8);
            this.lblTotalRecords.Name = "lblTotalRecords";
            this.lblTotalRecords.Size = new System.Drawing.Size(58, 20);
            this.lblTotalRecords.TabIndex = 42;
            this.lblTotalRecords.Text = "records";
            // 
            // lblRecord
            // 
            this.lblRecord.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblRecord.AutoSize = true;
            this.lblRecord.Location = new System.Drawing.Point(944, 8);
            this.lblRecord.Name = "lblRecord";
            this.lblRecord.Size = new System.Drawing.Size(23, 20);
            this.lblRecord.TabIndex = 43;
            this.lblRecord.Text = "条";
            // 
            // btnFirst
            // 
            this.btnFirst.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnFirst.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.btnFirst.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnFirst.DataFlag = null;
            this.btnFirst.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnFirst.Image = global::Zoncare.Component.Properties.Resources.prevFirst;
            this.btnFirst.IsHighlight = true;
            this.btnFirst.Location = new System.Drawing.Point(120, 5);
            this.btnFirst.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this.btnFirst.Name = "btnFirst";
            this.btnFirst.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnFirst.Size = new System.Drawing.Size(36, 26);
            this.btnFirst.TabIndex = 44;
            this.btnFirst.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFirst.UseVisualStyleBackColor = false;
            this.btnFirst.Click += new System.EventHandler(this.btnFirst_Click);
            // 
            // tbxPageIndex
            // 
            this.tbxPageIndex.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxPageIndex.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.tbxPageIndex.BorderColor = System.Drawing.Color.Empty;
            this.tbxPageIndex.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxPageIndex.Location = new System.Drawing.Point(218, 5);
            this.tbxPageIndex.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.tbxPageIndex.MinimumSize = new System.Drawing.Size(32, 0);
            this.tbxPageIndex.MinValue = 1D;
            this.tbxPageIndex.Name = "tbxPageIndex";
            this.tbxPageIndex.NumericOnly = true;
            this.tbxPageIndex.PlaceHolderStr = null;
            this.tbxPageIndex.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tbxPageIndex.Size = new System.Drawing.Size(51, 26);
            this.tbxPageIndex.TabIndex = 49;
            this.tbxPageIndex.Text = "1";
            this.tbxPageIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tbxPageIndex.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbxPageIndex_KeyDown);
            // 
            // cbxPageSize
            // 
            this.cbxPageSize.FormattingEnabled = true;
            this.cbxPageSize.Items.AddRange(new object[] {
            "15",
            "20",
            "30",
            "50",
            "100"});
            this.cbxPageSize.Location = new System.Drawing.Point(3, 3);
            this.cbxPageSize.Name = "cbxPageSize";
            this.cbxPageSize.Size = new System.Drawing.Size(99, 28);
            this.cbxPageSize.TabIndex = 50;
            this.cbxPageSize.SelectedIndexChanged += new System.EventHandler(this.cbxPageSize_SelectedIndexChanged);
            this.cbxPageSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbxPageSize_KeyPress);
            // 
            // ZoncarePage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(113)))), ((int)(((byte)(191)))));
            this.Controls.Add(this.tlpPaging);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.Name = "ZoncarePage";
            this.Size = new System.Drawing.Size(970, 36);
            this.Load += new System.EventHandler(this.ZoncarePage_Load);
            this.tlpPaging.ResumeLayout(false);
            this.tlpPaging.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpPaging;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTotalPages;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblTotalRecords;
        private System.Windows.Forms.Label lblRecord;
        private Controls.RoundButton btnFirst;
        private Controls.RoundButton btnLast;
        private Controls.RoundButton btnNext;
        private Controls.RoundButton btnPrevious;
        private Controls.TextBoxEx tbxPageIndex;
        private System.Windows.Forms.ComboBox cbxPageSize;
    }
}
