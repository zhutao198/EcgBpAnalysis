namespace AnalysisSystem.Base
{
    partial class BaseDialogForm
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblTip = new System.Windows.Forms.Label();
            this.tlpTop = new System.Windows.Forms.TableLayoutPanel();
            this.tlpControls = new System.Windows.Forms.TableLayoutPanel();
            this.picClose = new System.Windows.Forms.PictureBox();
            this.picMin = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.picBox = new System.Windows.Forms.PictureBox();
            this.tlpTop.SuspendLayout();
            this.tlpControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).BeginInit();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 50;
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 50;
            this.toolTip1.ReshowDelay = 10;
            // 
            // lblTip
            // 
            this.lblTip.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTip.AutoSize = true;
            this.lblTip.ForeColor = System.Drawing.Color.White;
            this.lblTip.Location = new System.Drawing.Point(3, 6);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(21, 20);
            this.lblTip.TabIndex = 0;
            this.lblTip.Text = "   ";
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
            this.tlpTop.Size = new System.Drawing.Size(876, 32);
            this.tlpTop.TabIndex = 17;
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
            this.tlpControls.Location = new System.Drawing.Point(818, 0);
            this.tlpControls.Margin = new System.Windows.Forms.Padding(0);
            this.tlpControls.Name = "tlpControls";
            this.tlpControls.RowCount = 1;
            this.tlpControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpControls.Size = new System.Drawing.Size(58, 32);
            this.tlpControls.TabIndex = 15;
            // 
            // picClose
            // 
            this.picClose.Cursor = System.Windows.Forms.Cursors.Default;
            this.picClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picClose.Image = global::AnalysisSystem.Base.Properties.Resources.close;
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
            this.picMin.Cursor = System.Windows.Forms.Cursors.Default;
            this.picMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picMin.Image = global::AnalysisSystem.Base.Properties.Resources.minimize;
            this.picMin.Location = new System.Drawing.Point(0, 1);
            this.picMin.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.picMin.Name = "picMin";
            this.picMin.Size = new System.Drawing.Size(29, 30);
            this.picMin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picMin.TabIndex = 13;
            this.picMin.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTitle.AutoSize = true;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(427, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(21, 20);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "   ";
            // 
            // picBox
            // 
            this.picBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.picBox.BackColor = System.Drawing.Color.Transparent;
            this.picBox.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.picBox.Location = new System.Drawing.Point(870, 569);
            this.picBox.Name = "picBox";
            this.picBox.Size = new System.Drawing.Size(6, 6);
            this.picBox.TabIndex = 14;
            this.picBox.TabStop = false;
            this.picBox.Visible = false;
            this.picBox.Paint += new System.Windows.Forms.PaintEventHandler(this.picBox_Paint);
            // 
            // BaseDialogForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(876, 575);
            this.Controls.Add(this.tlpTop);
            this.Controls.Add(this.picBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "BaseDialogForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Activated += new System.EventHandler(this.BaseDialogForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BaseDialogForm_FormClosing);
            this.Load += new System.EventHandler(this.BaseDialogForm_Load);
            this.Shown += new System.EventHandler(this.BaseDialogForm_Shown);
            this.SizeChanged += new System.EventHandler(this.BaseDialogForm_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.BaseDialogForm_Paint);
            this.tlpTop.ResumeLayout(false);
            this.tlpTop.PerformLayout();
            this.tlpControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox picBox;
        private System.Windows.Forms.Label lblTip;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TableLayoutPanel tlpControls;
        private System.Windows.Forms.PictureBox picMin;
        protected System.Windows.Forms.TableLayoutPanel tlpTop;
        private System.Windows.Forms.PictureBox picClose;
    }
}