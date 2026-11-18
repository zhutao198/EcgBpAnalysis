namespace AnalysisApp
{
    partial class BeatBatchInsertPopup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.tbxInterval = new Zoncare.Component.Controls.TextBoxEx();
            this.label27 = new System.Windows.Forms.Label();
            this.btnOK = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnCancel = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(273, 64);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(227, 22);
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
            this.tbxInterval.BackColor = System.Drawing.Color.White;
            this.tbxInterval.BorderColor = System.Drawing.Color.Empty;
            this.tbxInterval.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxInterval.BorderSize = 1;
            this.tbxInterval.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxInterval.CornerRadius = 2;
            this.tbxInterval.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxInterval.Location = new System.Drawing.Point(86, 18);
            this.tbxInterval.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tbxInterval.MaxLength = 4;
            this.tbxInterval.MaxValue = 2000D;
            this.tbxInterval.MinimumSize = new System.Drawing.Size(4, 30);
            this.tbxInterval.MinValue = 200D;
            this.tbxInterval.Multiline = false;
            this.tbxInterval.Name = "tbxInterval";
            this.tbxInterval.NumericOnly = true;
            this.tbxInterval.PasswordChar = '\0';
            this.tbxInterval.PlaceHolderStr = null;
            this.tbxInterval.ReadOnly = false;
            this.tbxInterval.SelectionStart = 0;
            this.tbxInterval.Size = new System.Drawing.Size(141, 30);
            this.tbxInterval.TabIndex = 3;
            this.tbxInterval.TabStop = false;
            this.tbxInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxInterval.TextBackgroundColor = System.Drawing.Color.White;
            this.tbxInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbxInterval_KeyPress);
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
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnOK.DataFlag = null;
            this.btnOK.FollowBaseColor = false;
            this.btnOK.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnOK.IsHighlight = true;
            this.btnOK.Location = new System.Drawing.Point(48, 4);
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
            this.btnCancel.Location = new System.Drawing.Point(142, 4);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCancel.Size = new System.Drawing.Size(82, 32);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnOK, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 64);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(273, 41);
            this.tableLayoutPanel2.TabIndex = 20;
            // 
            // BeatBatchInsertPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 105);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.DoubleBuffered = true;
            this.Name = "BeatBatchInsertPopup";
            this.ShowInTaskbar = false;
            this.Text = "批量插入心搏";
            this.Load += new System.EventHandler(this.BeatBatchAddPopup_Load);
            this.Controls.SetChildIndex(this.tableLayoutPanel2, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Zoncare.Component.Controls.RoundButton btnOK;
        private Zoncare.Component.Controls.RoundButton btnCancel;
        private Zoncare.Component.Controls.TextBoxEx tbxInterval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}