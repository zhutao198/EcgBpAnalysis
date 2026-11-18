namespace SystemSettings
{
    partial class UserDetailForm
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
            this.btnSave = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnCancel = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxUserName = new Zoncare.Component.Controls.TextBoxEx();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbxPwd = new Zoncare.Component.Controls.TextBoxEx();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnUpload = new Zoncare.Component.Controls.RoundButton(this.components);
            this.btnClear = new Zoncare.Component.Controls.RoundButton(this.components);
            this.tbxEmpNo = new Zoncare.Component.Controls.TextBoxEx();
            this.label12 = new System.Windows.Forms.Label();
            this.tbxRealName = new Zoncare.Component.Controls.TextBoxEx();
            this.cbxRole = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.tbxDepartment = new Zoncare.Component.Controls.TextBoxEx();
            this.tbxEmail = new Zoncare.Component.Controls.TextBoxEx();
            this.tbxTel = new Zoncare.Component.Controls.TextBoxEx();
            this.label13 = new System.Windows.Forms.Label();
            this.picSign = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSign)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnSave.DataFlag = null;
            this.btnSave.FollowBaseColor = false;
            this.btnSave.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnSave.IsHighlight = true;
            this.btnSave.Location = new System.Drawing.Point(156, 8);
            this.btnSave.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCancel.DataFlag = null;
            this.btnCancel.FollowBaseColor = false;
            this.btnCancel.Location = new System.Drawing.Point(243, 8);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnSave, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCancel, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 423);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(475, 46);
            this.tableLayoutPanel2.TabIndex = 20;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 189F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbxUserName, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label5, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.tbxPwd, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label7, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label6, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label10, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.label9, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.label8, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.label11, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.tbxEmpNo, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label12, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.tbxRealName, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.cbxRole, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.tbxDepartment, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.tbxEmail, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.tbxTel, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.label13, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.picSign, 2, 8);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 113F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(475, 423);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(129, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "用户名";
            // 
            // tbxUserName
            // 
            this.tbxUserName.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxUserName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(237)))), ((int)(((byte)(240)))));
            this.tbxUserName.BorderColor = System.Drawing.Color.Empty;
            this.tbxUserName.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxUserName.BorderSize = 1;
            this.tbxUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxUserName.CornerRadius = 2;
            this.tbxUserName.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxUserName.Location = new System.Drawing.Point(186, 10);
            this.tbxUserName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxUserName.MaxLength = 32;
            this.tbxUserName.Multiline = false;
            this.tbxUserName.Name = "tbxUserName";
            this.tbxUserName.PasswordChar = '\0';
            this.tbxUserName.PlaceHolderStr = null;
            this.tbxUserName.ReadOnly = false;
            this.tbxUserName.SelectionStart = 0;
            this.tbxUserName.Size = new System.Drawing.Size(183, 28);
            this.tbxUserName.TabIndex = 0;
            this.tbxUserName.TabStop = false;
            this.tbxUserName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxUserName.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Red;
            this.label5.Location = new System.Drawing.Point(372, 14);
            this.label5.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(15, 20);
            this.label5.TabIndex = 7;
            this.label5.Text = "*";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(143, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "密码";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(372, 47);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "*";
            // 
            // tbxPwd
            // 
            this.tbxPwd.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxPwd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxPwd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(237)))), ((int)(((byte)(240)))));
            this.tbxPwd.BorderColor = System.Drawing.Color.Empty;
            this.tbxPwd.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxPwd.BorderSize = 1;
            this.tbxPwd.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxPwd.CornerRadius = 2;
            this.tbxPwd.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxPwd.Location = new System.Drawing.Point(186, 43);
            this.tbxPwd.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxPwd.MaxLength = 32;
            this.tbxPwd.Multiline = false;
            this.tbxPwd.Name = "tbxPwd";
            this.tbxPwd.PasswordChar = '*';
            this.tbxPwd.PlaceHolderStr = null;
            this.tbxPwd.ReadOnly = false;
            this.tbxPwd.SelectionStart = 0;
            this.tbxPwd.Size = new System.Drawing.Size(183, 28);
            this.tbxPwd.TabIndex = 1;
            this.tbxPwd.TabStop = false;
            this.tbxPwd.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxPwd.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(143, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 20);
            this.label7.TabIndex = 13;
            this.label7.Text = "工号";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(143, 113);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 20);
            this.label6.TabIndex = 12;
            this.label6.Text = "姓名";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(143, 146);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 20);
            this.label4.TabIndex = 11;
            this.label4.Text = "角色";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(143, 179);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 20);
            this.label10.TabIndex = 16;
            this.label10.Text = "科室";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(143, 212);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(37, 20);
            this.label9.TabIndex = 15;
            this.label9.Text = "邮箱";
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(143, 245);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 20);
            this.label8.TabIndex = 14;
            this.label8.Text = "电话";
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(143, 318);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(37, 20);
            this.label11.TabIndex = 17;
            this.label11.Text = "签名";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.43478F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.btnUpload, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnClear, 2, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(186, 385);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(183, 36);
            this.tableLayoutPanel3.TabIndex = 18;
            // 
            // btnUpload
            // 
            this.btnUpload.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnUpload.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnUpload.DataFlag = null;
            this.btnUpload.FollowBaseColor = false;
            this.btnUpload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnUpload.IsHighlight = true;
            this.btnUpload.Location = new System.Drawing.Point(72, 5);
            this.btnUpload.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnUpload.Size = new System.Drawing.Size(48, 26);
            this.btnUpload.TabIndex = 8;
            this.btnUpload.Text = "上传";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnClear.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(231)))), ((int)(((byte)(245)))));
            this.btnClear.DataFlag = null;
            this.btnClear.FollowBaseColor = false;
            this.btnClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.btnClear.IsHighlight = true;
            this.btnClear.Location = new System.Drawing.Point(130, 5);
            this.btnClear.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.btnClear.Size = new System.Drawing.Size(48, 26);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "清空";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // tbxEmpNo
            // 
            this.tbxEmpNo.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxEmpNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxEmpNo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(237)))), ((int)(((byte)(240)))));
            this.tbxEmpNo.BorderColor = System.Drawing.Color.Empty;
            this.tbxEmpNo.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxEmpNo.BorderSize = 1;
            this.tbxEmpNo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxEmpNo.CornerRadius = 2;
            this.tbxEmpNo.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxEmpNo.Location = new System.Drawing.Point(186, 76);
            this.tbxEmpNo.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxEmpNo.MaxLength = 50;
            this.tbxEmpNo.Multiline = false;
            this.tbxEmpNo.Name = "tbxEmpNo";
            this.tbxEmpNo.PasswordChar = '\0';
            this.tbxEmpNo.PlaceHolderStr = null;
            this.tbxEmpNo.ReadOnly = false;
            this.tbxEmpNo.SelectionStart = 0;
            this.tbxEmpNo.Size = new System.Drawing.Size(183, 28);
            this.tbxEmpNo.TabIndex = 2;
            this.tbxEmpNo.TabStop = false;
            this.tbxEmpNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxEmpNo.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label12.AutoSize = true;
            this.label12.ForeColor = System.Drawing.Color.Red;
            this.label12.Location = new System.Drawing.Point(372, 113);
            this.label12.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(15, 20);
            this.label12.TabIndex = 20;
            this.label12.Text = "*";
            // 
            // tbxRealName
            // 
            this.tbxRealName.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxRealName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxRealName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(237)))), ((int)(((byte)(240)))));
            this.tbxRealName.BorderColor = System.Drawing.Color.Empty;
            this.tbxRealName.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxRealName.BorderSize = 1;
            this.tbxRealName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxRealName.CornerRadius = 2;
            this.tbxRealName.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxRealName.Location = new System.Drawing.Point(186, 109);
            this.tbxRealName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxRealName.MaxLength = 20;
            this.tbxRealName.Multiline = false;
            this.tbxRealName.Name = "tbxRealName";
            this.tbxRealName.PasswordChar = '\0';
            this.tbxRealName.PlaceHolderStr = null;
            this.tbxRealName.ReadOnly = false;
            this.tbxRealName.SelectionStart = 0;
            this.tbxRealName.Size = new System.Drawing.Size(183, 28);
            this.tbxRealName.TabIndex = 3;
            this.tbxRealName.TabStop = false;
            this.tbxRealName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxRealName.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // cbxRole
            // 
            this.cbxRole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxRole.CheckedListBox = null;
            this.cbxRole.Control = null;
            this.cbxRole.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cbxRole.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbxRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRole.FormattingEnabled = true;
            this.cbxRole.IsShown = false;
            this.cbxRole.IsUnresponsive = false;
            this.cbxRole.Location = new System.Drawing.Point(186, 145);
            this.cbxRole.Name = "cbxRole";
            this.cbxRole.PlaceHolderStr = null;
            this.cbxRole.Size = new System.Drawing.Size(183, 27);
            this.cbxRole.TabIndex = 4;
            this.cbxRole.TreeView = null;
            // 
            // tbxDepartment
            // 
            this.tbxDepartment.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxDepartment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxDepartment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(237)))), ((int)(((byte)(240)))));
            this.tbxDepartment.BorderColor = System.Drawing.Color.Empty;
            this.tbxDepartment.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxDepartment.BorderSize = 1;
            this.tbxDepartment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxDepartment.CornerRadius = 2;
            this.tbxDepartment.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxDepartment.Location = new System.Drawing.Point(186, 175);
            this.tbxDepartment.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxDepartment.MaxLength = 50;
            this.tbxDepartment.Multiline = false;
            this.tbxDepartment.Name = "tbxDepartment";
            this.tbxDepartment.PasswordChar = '\0';
            this.tbxDepartment.PlaceHolderStr = null;
            this.tbxDepartment.ReadOnly = false;
            this.tbxDepartment.SelectionStart = 0;
            this.tbxDepartment.Size = new System.Drawing.Size(183, 28);
            this.tbxDepartment.TabIndex = 5;
            this.tbxDepartment.TabStop = false;
            this.tbxDepartment.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxDepartment.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // tbxEmail
            // 
            this.tbxEmail.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxEmail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(237)))), ((int)(((byte)(240)))));
            this.tbxEmail.BorderColor = System.Drawing.Color.Empty;
            this.tbxEmail.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxEmail.BorderSize = 1;
            this.tbxEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxEmail.CornerRadius = 2;
            this.tbxEmail.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxEmail.Location = new System.Drawing.Point(186, 208);
            this.tbxEmail.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxEmail.MaxLength = 50;
            this.tbxEmail.Multiline = false;
            this.tbxEmail.Name = "tbxEmail";
            this.tbxEmail.PasswordChar = '\0';
            this.tbxEmail.PlaceHolderStr = null;
            this.tbxEmail.ReadOnly = false;
            this.tbxEmail.SelectionStart = 0;
            this.tbxEmail.Size = new System.Drawing.Size(183, 28);
            this.tbxEmail.TabIndex = 6;
            this.tbxEmail.TabStop = false;
            this.tbxEmail.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxEmail.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // tbxTel
            // 
            this.tbxTel.AlphaBackColor = System.Drawing.Color.Empty;
            this.tbxTel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxTel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(237)))), ((int)(((byte)(240)))));
            this.tbxTel.BorderColor = System.Drawing.Color.Empty;
            this.tbxTel.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.tbxTel.BorderSize = 1;
            this.tbxTel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxTel.CornerRadius = 2;
            this.tbxTel.DropdownPosition = new System.Drawing.Point(0, 21);
            this.tbxTel.Location = new System.Drawing.Point(186, 241);
            this.tbxTel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tbxTel.MaxLength = 11;
            this.tbxTel.Multiline = false;
            this.tbxTel.Name = "tbxTel";
            this.tbxTel.PasswordChar = '\0';
            this.tbxTel.PlaceHolderStr = null;
            this.tbxTel.ReadOnly = false;
            this.tbxTel.SelectionStart = 0;
            this.tbxTel.Size = new System.Drawing.Size(183, 28);
            this.tbxTel.TabIndex = 7;
            this.tbxTel.TabStop = false;
            this.tbxTel.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.tbxTel.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label13.AutoSize = true;
            this.label13.ForeColor = System.Drawing.Color.Red;
            this.label13.Location = new System.Drawing.Point(372, 146);
            this.label13.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(15, 20);
            this.label13.TabIndex = 26;
            this.label13.Text = "*";
            // 
            // picSign
            // 
            this.picSign.BackColor = System.Drawing.SystemColors.Window;
            this.picSign.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picSign.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSign.Location = new System.Drawing.Point(186, 275);
            this.picSign.Name = "picSign";
            this.picSign.Size = new System.Drawing.Size(183, 107);
            this.picSign.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSign.TabIndex = 27;
            this.picSign.TabStop = false;
            // 
            // UserDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 469);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "UserDetailForm";
            this.Text = "用户";
            this.Load += new System.EventHandler(this.UserDetailForm_Load);
            this.Controls.SetChildIndex(this.tableLayoutPanel2, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picSign)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Zoncare.Component.Controls.RoundButton btnSave;
        private Zoncare.Component.Controls.RoundButton btnCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private Zoncare.Component.Controls.TextBoxEx tbxUserName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Zoncare.Component.Controls.TextBoxEx tbxPwd;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Zoncare.Component.Controls.RoundButton btnUpload;
        private Zoncare.Component.Controls.RoundButton btnClear;
        private Zoncare.Component.Controls.TextBoxEx tbxEmpNo;
        private System.Windows.Forms.Label label12;
        private Zoncare.Component.Controls.TextBoxEx tbxRealName;
        private Zoncare.Component.Controls.ZoncareComboBox cbxRole;
        private Zoncare.Component.Controls.TextBoxEx tbxDepartment;
        private Zoncare.Component.Controls.TextBoxEx tbxEmail;
        private Zoncare.Component.Controls.TextBoxEx tbxTel;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.PictureBox picSign;
    }
}