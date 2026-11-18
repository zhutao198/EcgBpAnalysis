namespace Zoncare.Component.Widgets
{
    partial class InputPanel
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
            this.tlpTop = new System.Windows.Forms.TableLayoutPanel();
            this.tlpControls = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTip = new System.Windows.Forms.Label();
            this.picClose = new System.Windows.Forms.PictureBox();
            this.picMin = new System.Windows.Forms.PictureBox();
            this.tlpTop.SuspendLayout();
            this.tlpControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpTop
            // 
            this.tlpTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(122)))), ((int)(((byte)(196)))));
            this.tlpTop.ColumnCount = 3;
            this.tlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpTop.Controls.Add(this.tlpControls, 2, 0);
            this.tlpTop.Controls.Add(this.lblTitle, 1, 0);
            this.tlpTop.Controls.Add(this.lblTip, 0, 0);
            this.tlpTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpTop.Location = new System.Drawing.Point(0, 0);
            this.tlpTop.Name = "tlpTop";
            this.tlpTop.RowCount = 1;
            this.tlpTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTop.Size = new System.Drawing.Size(517, 32);
            this.tlpTop.TabIndex = 18;
            this.tlpTop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tlpTop_MouseDown);
            // 
            // tlpControls
            // 
            this.tlpControls.ColumnCount = 2;
            this.tlpControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControls.Controls.Add(this.picClose, 1, 0);
            this.tlpControls.Controls.Add(this.picMin, 0, 0);
            this.tlpControls.Dock = System.Windows.Forms.DockStyle.Right;
            this.tlpControls.Location = new System.Drawing.Point(459, 0);
            this.tlpControls.Margin = new System.Windows.Forms.Padding(0);
            this.tlpControls.Name = "tlpControls";
            this.tlpControls.RowCount = 1;
            this.tlpControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControls.Size = new System.Drawing.Size(58, 32);
            this.tlpControls.TabIndex = 15;
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTitle.AutoSize = true;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(247, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(23, 12);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "   ";
            // 
            // lblTip
            // 
            this.lblTip.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTip.AutoSize = true;
            this.lblTip.ForeColor = System.Drawing.Color.White;
            this.lblTip.Location = new System.Drawing.Point(3, 10);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(23, 12);
            this.lblTip.TabIndex = 0;
            this.lblTip.Text = "   ";
            // 
            // picClose
            // 
            this.picClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picClose.Cursor = System.Windows.Forms.Cursors.Default;
            this.picClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picClose.Image = global::Zoncare.Component.Properties.Resources.close;
            this.picClose.Location = new System.Drawing.Point(29, 1);
            this.picClose.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.picClose.Name = "picClose";
            this.picClose.Size = new System.Drawing.Size(29, 30);
            this.picClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picClose.TabIndex = 8;
            this.picClose.TabStop = false;
            // 
            // picMin
            // 
            this.picMin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picMin.Cursor = System.Windows.Forms.Cursors.Default;
            this.picMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picMin.Location = new System.Drawing.Point(0, 1);
            this.picMin.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.picMin.Name = "picMin";
            this.picMin.Size = new System.Drawing.Size(29, 30);
            this.picMin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picMin.TabIndex = 13;
            this.picMin.TabStop = false;
            // 
            // InputPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tlpTop);
            this.Name = "InputPanel";
            this.Size = new System.Drawing.Size(517, 202);
            this.Load += new System.EventHandler(this.InputPanel_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.InputPanel_Paint);
            this.tlpTop.ResumeLayout(false);
            this.tlpTop.PerformLayout();
            this.tlpControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.TableLayoutPanel tlpTop;
        private System.Windows.Forms.TableLayoutPanel tlpControls;
        public System.Windows.Forms.PictureBox picClose;
        private System.Windows.Forms.PictureBox picMin;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblTip;
    }
}
