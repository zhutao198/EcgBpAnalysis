namespace SystemSettings
{
    partial class BaseInfo
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
            this.baseInfoPanel = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.hospitalLogo = new System.Windows.Forms.PictureBox();
            this.clearBtn = new Zoncare.Component.Controls.RoundButton(this.components);
            this.uploadBtn = new Zoncare.Component.Controls.RoundButton(this.components);
            this.label7 = new System.Windows.Forms.Label();
            this.hospitalName = new Zoncare.Component.Controls.TextBoxEx();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.hospitalAddress = new Zoncare.Component.Controls.TextBoxEx();
            this.hospitalPhone = new Zoncare.Component.Controls.TextBoxEx();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.holterDataPath = new Zoncare.Component.Controls.TextBoxEx();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.dateFormat = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.language = new Zoncare.Component.Controls.ZoncareComboBox(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.saveBtn = new Zoncare.Component.Controls.RoundButton(this.components);
            this.resetBtn = new Zoncare.Component.Controls.RoundButton(this.components);
            this.baseInfoPanel.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hospitalLogo)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // baseInfoPanel
            // 
            this.baseInfoPanel.ColumnCount = 1;
            this.baseInfoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.baseInfoPanel.Controls.Add(this.groupBox3, 0, 2);
            this.baseInfoPanel.Controls.Add(this.groupBox1, 0, 0);
            this.baseInfoPanel.Controls.Add(this.groupBox2, 0, 1);
            this.baseInfoPanel.Controls.Add(this.tableLayoutPanel1, 0, 4);
            this.baseInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseInfoPanel.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.baseInfoPanel.Location = new System.Drawing.Point(0, 0);
            this.baseInfoPanel.Name = "baseInfoPanel";
            this.baseInfoPanel.RowCount = 5;
            this.baseInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.baseInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.baseInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 270F));
            this.baseInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.baseInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.baseInfoPanel.Size = new System.Drawing.Size(862, 568);
            this.baseInfoPanel.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel4);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.groupBox3.Location = new System.Drawing.Point(3, 130);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(856, 267);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "医院信息";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.hospitalLogo, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.clearBtn, 3, 2);
            this.tableLayoutPanel4.Controls.Add(this.uploadBtn, 2, 2);
            this.tableLayoutPanel4.Controls.Add(this.label7, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.hospitalName, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.hospitalAddress, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.hospitalPhone, 3, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 22);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(850, 242);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // hospitalLogo
            // 
            this.hospitalLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.hospitalLogo.BackColor = System.Drawing.SystemColors.Window;
            this.hospitalLogo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hospitalLogo.Location = new System.Drawing.Point(104, 83);
            this.hospitalLogo.Name = "hospitalLogo";
            this.hospitalLogo.Size = new System.Drawing.Size(317, 156);
            this.hospitalLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.hospitalLogo.TabIndex = 4;
            this.hospitalLogo.TabStop = false;
            // 
            // clearBtn
            // 
            this.clearBtn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.clearBtn.DataFlag = null;
            this.clearBtn.FollowBaseColor = false;
            this.clearBtn.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.clearBtn.Location = new System.Drawing.Point(531, 146);
            this.clearBtn.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.clearBtn.Size = new System.Drawing.Size(75, 30);
            this.clearBtn.TabIndex = 6;
            this.clearBtn.Text = "清空";
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.clearBtn_Click);
            // 
            // uploadBtn
            // 
            this.uploadBtn.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.uploadBtn.BackColor = System.Drawing.Color.White;
            this.uploadBtn.DataFlag = null;
            this.uploadBtn.FollowBaseColor = false;
            this.uploadBtn.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uploadBtn.Location = new System.Drawing.Point(444, 146);
            this.uploadBtn.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.uploadBtn.Name = "uploadBtn";
            this.uploadBtn.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.uploadBtn.Size = new System.Drawing.Size(75, 30);
            this.uploadBtn.TabIndex = 5;
            this.uploadBtn.Text = "上传";
            this.uploadBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.uploadBtn.UseVisualStyleBackColor = false;
            this.uploadBtn.Click += new System.EventHandler(this.uploadBtn_Click);
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(26, 151);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 20);
            this.label7.TabIndex = 3;
            this.label7.Text = "医院Logo";
            // 
            // hospitalName
            // 
            this.hospitalName.AlphaBackColor = System.Drawing.Color.Empty;
            this.hospitalName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.hospitalName.BackColor = System.Drawing.SystemColors.Window;
            this.hospitalName.BorderColor = System.Drawing.Color.Empty;
            this.hospitalName.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.hospitalName.BorderSize = 1;
            this.hospitalName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.hospitalName.CornerRadius = 2;
            this.hospitalName.DropdownPosition = new System.Drawing.Point(0, 21);
            this.hospitalName.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.hospitalName.Location = new System.Drawing.Point(103, 6);
            this.hospitalName.MaxLength = 32767;
            this.hospitalName.Multiline = false;
            this.hospitalName.Name = "hospitalName";
            this.hospitalName.PasswordChar = '\0';
            this.hospitalName.PlaceHolderStr = null;
            this.hospitalName.ReadOnly = false;
            this.hospitalName.SelectionStart = 0;
            this.hospitalName.Size = new System.Drawing.Size(319, 28);
            this.hospitalName.TabIndex = 3;
            this.hospitalName.TabStop = false;
            this.hospitalName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.hospitalName.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(32, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 20);
            this.label6.TabIndex = 2;
            this.label6.Text = "医院地址";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(32, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 20);
            this.label4.TabIndex = 1;
            this.label4.Text = "医院名称";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(457, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 20);
            this.label5.TabIndex = 2;
            this.label5.Text = "医院电话";
            // 
            // hospitalAddress
            // 
            this.hospitalAddress.AlphaBackColor = System.Drawing.Color.Empty;
            this.hospitalAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.hospitalAddress.BackColor = System.Drawing.SystemColors.Window;
            this.hospitalAddress.BorderColor = System.Drawing.Color.Empty;
            this.hospitalAddress.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.hospitalAddress.BorderSize = 1;
            this.hospitalAddress.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanel4.SetColumnSpan(this.hospitalAddress, 3);
            this.hospitalAddress.CornerRadius = 2;
            this.hospitalAddress.DropdownPosition = new System.Drawing.Point(0, 21);
            this.hospitalAddress.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.hospitalAddress.Location = new System.Drawing.Point(103, 46);
            this.hospitalAddress.MaxLength = 32767;
            this.hospitalAddress.Multiline = false;
            this.hospitalAddress.Name = "hospitalAddress";
            this.hospitalAddress.PasswordChar = '\0';
            this.hospitalAddress.PlaceHolderStr = null;
            this.hospitalAddress.ReadOnly = false;
            this.hospitalAddress.SelectionStart = 0;
            this.hospitalAddress.Size = new System.Drawing.Size(744, 28);
            this.hospitalAddress.TabIndex = 4;
            this.hospitalAddress.TabStop = false;
            this.hospitalAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.hospitalAddress.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // hospitalPhone
            // 
            this.hospitalPhone.AlphaBackColor = System.Drawing.Color.Empty;
            this.hospitalPhone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.hospitalPhone.BackColor = System.Drawing.SystemColors.Window;
            this.hospitalPhone.BorderColor = System.Drawing.Color.Empty;
            this.hospitalPhone.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.hospitalPhone.BorderSize = 1;
            this.hospitalPhone.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.hospitalPhone.CornerRadius = 2;
            this.hospitalPhone.DropdownPosition = new System.Drawing.Point(0, 21);
            this.hospitalPhone.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.hospitalPhone.Location = new System.Drawing.Point(528, 6);
            this.hospitalPhone.MaxLength = 32767;
            this.hospitalPhone.Multiline = false;
            this.hospitalPhone.Name = "hospitalPhone";
            this.hospitalPhone.NumericOnly = true;
            this.hospitalPhone.PasswordChar = '\0';
            this.hospitalPhone.PlaceHolderStr = null;
            this.hospitalPhone.ReadOnly = false;
            this.hospitalPhone.SelectionStart = 0;
            this.hospitalPhone.Size = new System.Drawing.Size(319, 28);
            this.hospitalPhone.TabIndex = 4;
            this.hospitalPhone.TabStop = false;
            this.hospitalPhone.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.hospitalPhone.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.groupBox1.Location = new System.Drawing.Point(3, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(856, 62);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "目录设置";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.holterDataPath, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 22);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(850, 37);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.label1.Location = new System.Drawing.Point(4, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "数据文件目录";
            // 
            // holterDataPath
            // 
            this.holterDataPath.AlphaBackColor = System.Drawing.Color.Empty;
            this.holterDataPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.holterDataPath.BackColor = System.Drawing.SystemColors.Window;
            this.holterDataPath.BorderColor = System.Drawing.Color.Empty;
            this.holterDataPath.BorderFocusColor = System.Drawing.Color.DodgerBlue;
            this.holterDataPath.BorderSize = 1;
            this.holterDataPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.holterDataPath.CornerRadius = 2;
            this.holterDataPath.DropdownPosition = new System.Drawing.Point(0, 21);
            this.holterDataPath.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.holterDataPath.Location = new System.Drawing.Point(103, 4);
            this.holterDataPath.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.holterDataPath.MaxLength = 32767;
            this.holterDataPath.Multiline = false;
            this.holterDataPath.Name = "holterDataPath";
            this.holterDataPath.PasswordChar = '\0';
            this.holterDataPath.PlaceHolderStr = null;
            this.holterDataPath.ReadOnly = false;
            this.holterDataPath.SelectionStart = 0;
            this.holterDataPath.Size = new System.Drawing.Size(319, 28);
            this.holterDataPath.TabIndex = 1;
            this.holterDataPath.TabStop = false;
            this.holterDataPath.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.holterDataPath.TextBackgroundColor = System.Drawing.Color.White;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.groupBox2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(56)))), ((int)(((byte)(72)))));
            this.groupBox2.Location = new System.Drawing.Point(3, 65);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(856, 62);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "区域设置";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.dateFormat, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.language, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 22);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(850, 37);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // dateFormat
            // 
            this.dateFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateFormat.CheckedListBox = null;
            this.dateFormat.Control = null;
            this.dateFormat.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.dateFormat.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.dateFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dateFormat.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.dateFormat.FormattingEnabled = true;
            this.dateFormat.IsShown = false;
            this.dateFormat.IsUnresponsive = false;
            this.dateFormat.Location = new System.Drawing.Point(528, 5);
            this.dateFormat.Name = "dateFormat";
            this.dateFormat.PlaceHolderStr = null;
            this.dateFormat.Size = new System.Drawing.Size(319, 27);
            this.dateFormat.TabIndex = 4;
            this.dateFormat.TreeView = null;
            // 
            // language
            // 
            this.language.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.language.CheckedListBox = null;
            this.language.Control = null;
            this.language.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.language.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.language.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.language.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.language.FormattingEnabled = true;
            this.language.IsShown = false;
            this.language.IsUnresponsive = false;
            this.language.Location = new System.Drawing.Point(103, 5);
            this.language.Name = "language";
            this.language.PlaceHolderStr = null;
            this.language.Size = new System.Drawing.Size(319, 27);
            this.language.TabIndex = 3;
            this.language.TreeView = null;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(32, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "系统语言";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(457, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "日期格式";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.saveBtn, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.resetBtn, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 527);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(856, 38);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.saveBtn.DataFlag = null;
            this.saveBtn.FollowBaseColor = false;
            this.saveBtn.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.saveBtn.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.saveBtn.IsHighlight = true;
            this.saveBtn.Location = new System.Drawing.Point(434, 3);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.saveBtn.Size = new System.Drawing.Size(75, 30);
            this.saveBtn.TabIndex = 1;
            this.saveBtn.Text = "保存";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // resetBtn
            // 
            this.resetBtn.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.resetBtn.DataFlag = null;
            this.resetBtn.FollowBaseColor = false;
            this.resetBtn.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.resetBtn.Location = new System.Drawing.Point(347, 3);
            this.resetBtn.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.resetBtn.Name = "resetBtn";
            this.resetBtn.RoundStyle = Zoncare.Component.RoundStyle.All;
            this.resetBtn.Size = new System.Drawing.Size(75, 30);
            this.resetBtn.TabIndex = 0;
            this.resetBtn.Text = "重置";
            this.resetBtn.UseVisualStyleBackColor = true;
            this.resetBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // BaseInfo
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.baseInfoPanel);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "BaseInfo";
            this.Size = new System.Drawing.Size(862, 568);
            this.Load += new System.EventHandler(this.BaseInfo_Load);
            this.baseInfoPanel.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hospitalLogo)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel baseInfoPanel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private Zoncare.Component.Controls.TextBoxEx holterDataPath;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private Zoncare.Component.Controls.TextBoxEx hospitalAddress;
        private System.Windows.Forms.Label label6;
        private Zoncare.Component.Controls.TextBoxEx hospitalName;
        private Zoncare.Component.Controls.TextBoxEx hospitalPhone;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.PictureBox hospitalLogo;
        private Zoncare.Component.Controls.RoundButton uploadBtn;
        private Zoncare.Component.Controls.RoundButton clearBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Zoncare.Component.Controls.RoundButton resetBtn;
        private Zoncare.Component.Controls.RoundButton saveBtn;
        private Zoncare.Component.Controls.ZoncareComboBox dateFormat;
        private Zoncare.Component.Controls.ZoncareComboBox language;
    }
}
