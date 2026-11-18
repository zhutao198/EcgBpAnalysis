namespace AnalysisApp
{
    partial class EnlargeWaveForm
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.smallWaveViewControl1 = new AnalysisApp.CommonControls.SmallWaveViewControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.enlargeWaveControl1 = new AnalysisApp.CommonControls.EnlargeWaveControl();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.smallWaveViewControl1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 473);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(964, 57);
            this.panel2.TabIndex = 1;
            // 
            // smallWaveViewControl1
            // 
            this.smallWaveViewControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.smallWaveViewControl1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.smallWaveViewControl1.Location = new System.Drawing.Point(0, 0);
            this.smallWaveViewControl1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.smallWaveViewControl1.Name = "smallWaveViewControl1";
            this.smallWaveViewControl1.Size = new System.Drawing.Size(964, 57);
            this.smallWaveViewControl1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.enlargeWaveControl1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(970, 533);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // enlargeWaveControl1
            // 
            this.enlargeWaveControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.enlargeWaveControl1.Location = new System.Drawing.Point(3, 3);
            this.enlargeWaveControl1.Name = "enlargeWaveControl1";
            this.enlargeWaveControl1.Size = new System.Drawing.Size(964, 467);
            this.enlargeWaveControl1.TabIndex = 2;
            // 
            // EnlargeWaveForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(970, 533);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Name = "EnlargeWaveForm";
            this.ShowInTaskbar = false;
            this.Text = "EnlargeWaveForm";
            this.Load += new System.EventHandler(this.EnlargeWaveForm_Load);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private CommonControls.SmallWaveViewControl smallWaveViewControl1;
        private CommonControls.EnlargeWaveControl enlargeWaveControl1;
    }
}