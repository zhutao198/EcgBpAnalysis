namespace AnalysisApp
{
    partial class AddSTEventForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbLead = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.pickerStartTime = new System.Windows.Forms.DateTimePicker();
            this.tbxDuration = new Zoncare.Component.Controls.TextBoxEx();
            this.tbxSlope = new Zoncare.Component.Controls.TextBoxEx();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnSubmit = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.cmbLead, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.pickerStartTime, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbxDuration, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbxSlope, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(347, 199);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(61, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "发生导联";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(61, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "起始时间";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "持续时间(分)";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 20);
            this.label4.TabIndex = 0;
            this.label4.Text = "抬高/压低(mV)";
            // 
            // cmbLead
            // 
            this.cmbLead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLead.CheckedListBox = null;
            this.cmbLead.Control = null;
            this.cmbLead.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cmbLead.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbLead.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLead.FormattingEnabled = true;
            this.cmbLead.IsShown = false;
            this.cmbLead.IsUnresponsive = false;
            this.cmbLead.Location = new System.Drawing.Point(132, 8);
            this.cmbLead.Name = "cmbLead";
            this.cmbLead.PlaceHolderStr = null;
            this.cmbLead.Size = new System.Drawing.Size(191, 27);
            this.cmbLead.TabIndex = 6;
            this.cmbLead.TreeView = null;
            // 
            // pickerStartTime
            // 
            this.pickerStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pickerStartTime.CustomFormat = "MM-dd HH:mm:ss";
            this.pickerStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.pickerStartTime.Location = new System.Drawing.Point(132, 45);
            this.pickerStartTime.Name = "pickerStartTime";
            this.pickerStartTime.Size = new System.Drawing.Size(191, 26);
            this.pickerStartTime.TabIndex = 7;
            // 
            // tbxDuration
            // 
            this.tbxDuration.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxDuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxDuration.BackColor = System.Drawing.Color.White;
            this.tbxDuration.BorderColor = System.Drawing.Color.Empty;
            this.tbxDuration.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxDuration.BorderSize = 1;
            this.tbxDuration.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxDuration.CornerRadius = 3;
            this.tbxDuration.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxDuration.FloatAllowed = true;
            this.tbxDuration.Location = new System.Drawing.Point(132, 84);
            this.tbxDuration.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxDuration.MaxLength = 4;
            this.tbxDuration.Multiline = false;
            this.tbxDuration.Name = "tbxDuration";
            this.tbxDuration.NumericOnly = true;
            this.tbxDuration.PasswordChar = '\0';
            this.tbxDuration.PlaceHolderStr = null;
            this.tbxDuration.ReadOnly = false;
            this.tbxDuration.SelectionStart = 0;
            this.tbxDuration.Size = new System.Drawing.Size(191, 26);
            this.tbxDuration.TabIndex = 8;
            this.tbxDuration.TabStop = false;
            this.tbxDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxDuration.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // tbxSlope
            // 
            this.tbxSlope.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxSlope.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxSlope.BackColor = System.Drawing.Color.White;
            this.tbxSlope.BorderColor = System.Drawing.Color.Empty;
            this.tbxSlope.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxSlope.BorderSize = 1;
            this.tbxSlope.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxSlope.CornerRadius = 3;
            this.tbxSlope.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxSlope.FloatAllowed = true;
            this.tbxSlope.Location = new System.Drawing.Point(132, 123);
            this.tbxSlope.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxSlope.MaxLength = 5;
            this.tbxSlope.Multiline = false;
            this.tbxSlope.Name = "tbxSlope";
            this.tbxSlope.NumericOnly = true;
            this.tbxSlope.PasswordChar = '\0';
            this.tbxSlope.PlaceHolderStr = null;
            this.tbxSlope.ReadOnly = false;
            this.tbxSlope.SelectionStart = 0;
            this.tbxSlope.Size = new System.Drawing.Size(191, 26);
            this.tbxSlope.TabIndex = 8;
            this.tbxSlope.TabStop = false;
            this.tbxSlope.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxSlope.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 4);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnSubmit, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 159);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(341, 37);
            this.tableLayoutPanel2.TabIndex = 19;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.DataFlag = null;
            this.btnCancel.FollowBaseColor = false;
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(132)))), ((int)(((byte)(207)))));
            this.btnCancel.Location = new System.Drawing.Point(176, 6);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSubmit
            // 
            this.btnSubmit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSubmit.BackColor = System.Drawing.Color.White;
            this.btnSubmit.DataFlag = null;
            this.btnSubmit.FollowBaseColor = false;
            this.btnSubmit.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSubmit.IsHighlight = true;
            this.btnSubmit.Location = new System.Drawing.Point(89, 6);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSubmit.Size = new System.Drawing.Size(75, 25);
            this.btnSubmit.TabIndex = 5;
            this.btnSubmit.Text = "确定";
            this.btnSubmit.UseVisualStyleBackColor = false;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // AddSTEventForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(347, 199);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AddSTEventForm";
            this.ShowInTaskbar = false;
            this.Text = "AddSTEventForm";
            this.Load += new System.EventHandler(this.AddSTEventForm_Load);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private Zoncare.Component.Controls.RoundButton btnCancel;
        private Zoncare.Component.Controls.RoundButton btnSubmit;
        private Zoncare.Component.Controls.ZoncareComboBox cmbLead;
        private System.Windows.Forms.DateTimePicker pickerStartTime;
        private Zoncare.Component.Controls.TextBoxEx tbxDuration;
        private Zoncare.Component.Controls.TextBoxEx tbxSlope;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}