namespace Zoncare.Component.Controls
{
    partial class AutoCompleteTextBox
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
            this.textBoxEx1 = new Zoncare.Component.Controls.TextBoxEx();
            this.picDown = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDown)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanel1.Controls.Add(this.textBoxEx1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.picDown, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 1);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(181, 31);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // textBoxEx1
            // 
            this.textBoxEx1.AlphaBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.textBoxEx1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxEx1.AutoCompleteDataSource = null;
            this.textBoxEx1.BackColor = System.Drawing.Color.White;
            this.textBoxEx1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxEx1.DropdownPosition = new System.Drawing.Point(0, 21);
            this.textBoxEx1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.textBoxEx1.Location = new System.Drawing.Point(3, 6);
            this.textBoxEx1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.textBoxEx1.Name = "textBoxEx1";
            this.textBoxEx1.PlaceHolderStr = "";
            this.textBoxEx1.Size = new System.Drawing.Size(178, 19);
            this.textBoxEx1.TabIndex = 0;
            this.textBoxEx1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxEx1_KeyPress);
            // 
            // picDown
            // 
            this.picDown.BackColor = System.Drawing.Color.White;
            this.picDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picDown.Image = global::Zoncare.Component.Properties.Resources.tr;
            this.picDown.Location = new System.Drawing.Point(181, 0);
            this.picDown.Margin = new System.Windows.Forms.Padding(0);
            this.picDown.Name = "picDown";
            this.picDown.Size = new System.Drawing.Size(1, 31);
            this.picDown.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picDown.TabIndex = 1;
            this.picDown.TabStop = false;
            this.picDown.Paint += new System.Windows.Forms.PaintEventHandler(this.picDown_Paint);
            this.picDown.MouseEnter += new System.EventHandler(this.picDown_MouseEnter);
            this.picDown.MouseLeave += new System.EventHandler(this.picDown_MouseLeave);
            this.picDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picDown_MouseUp);
            // 
            // AutoCompleteTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.Name = "AutoCompleteTextBox";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(183, 33);
            this.Load += new System.EventHandler(this.AutoCompleteTextBox_Load);
            this.EnabledChanged += new System.EventHandler(this.AutoCompleteTextBox_EnabledChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.AutoCompleteTextBox_Paint);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.TextBoxEx textBoxEx1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox picDown;
    }
}
