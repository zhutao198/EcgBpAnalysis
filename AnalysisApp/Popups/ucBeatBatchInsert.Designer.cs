namespace AnalysisApp.Popups
{
    partial class ucBeatBatchInsert
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.tbxInterval = new Zoncare.Component.Controls.TextBoxEx();
            this.label27 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnOK = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnCancel = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Controls.Add(this.label3, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbxInterval, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label27, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 32);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(251, 64);
            this.tableLayoutPanel1.TabIndex = 21;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(205, 22);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "ms";
            // 
            // tbxInterval
            // 
            this.tbxInterval.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxInterval.AutoCompleteDataSource = null;
            this.tbxInterval.BorderColor = System.Drawing.Color.Empty;
            this.tbxInterval.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxInterval.Location = new System.Drawing.Point(86, 18);
            this.tbxInterval.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tbxInterval.MaxLength = 4;
            this.tbxInterval.MaxValue = 2000D;
            this.tbxInterval.MinimumSize = new System.Drawing.Size(4, 30);
            this.tbxInterval.MinValue = 200D;
            this.tbxInterval.Name = "tbxInterval";
            this.tbxInterval.NumericOnly = true;
            this.tbxInterval.PlaceHolderStr = null;
            this.tbxInterval.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.tbxInterval.Size = new System.Drawing.Size(119, 30);
            this.tbxInterval.TabIndex = 3;
            // 
            // label27
            // 
            this.label27.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.label27.Location = new System.Drawing.Point(18, 22);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(65, 20);
            this.label27.TabIndex = 5;
            this.label27.Text = "心搏距离";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnOK, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 96);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(251, 41);
            this.tableLayoutPanel2.TabIndex = 22;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnOK.DataFlag = null;
            this.btnOK.FollowBaseColor = false;
            this.btnOK.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnOK.IsHighlight = true;
            this.btnOK.Location = new System.Drawing.Point(37, 4);
            this.btnOK.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnOK.Size = new System.Drawing.Size(82, 32);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCancel.DataFlag = null;
            this.btnCancel.FollowBaseColor = false;
            this.btnCancel.Location = new System.Drawing.Point(131, 4);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCancel.Size = new System.Drawing.Size(82, 32);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ucBeatBatchInsert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "ucBeatBatchInsert";
            this.Size = new System.Drawing.Size(251, 137);
            this.Controls.SetChildIndex(this.tableLayoutPanel2, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label3;
        private Zoncare.Component.Controls.TextBoxEx tbxInterval;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Zoncare.Component.Controls.RoundButton btnOK;
        private Zoncare.Component.Controls.RoundButton btnCancel;
    }
}
