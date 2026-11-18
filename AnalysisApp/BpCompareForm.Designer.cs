namespace AnalysisApp
{
    partial class BpCompareForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.bpCompareControl1 = new AnalysisApp.CommonControls.BpCompareControl();
            this.bpCompareControl2 = new AnalysisApp.CommonControls.BpCompareControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.bpCompareControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.bpCompareControl2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1184, 561);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // bpCompareControl1
            // 
            this.bpCompareControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.bpCompareControl1.DataSourceTracked = false;
            this.bpCompareControl1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.bpCompareControl1.IsActive = false;
            this.bpCompareControl1.Location = new System.Drawing.Point(3, 3);
            this.bpCompareControl1.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.bpCompareControl1.Name = "bpCompareControl1";
            this.bpCompareControl1.ShouldReset = false;
            this.bpCompareControl1.Size = new System.Drawing.Size(589, 549);
            this.bpCompareControl1.TabIndex = 0;
            // 
            // bpCompareControl2
            // 
            this.bpCompareControl2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.bpCompareControl2.CaseIndex = 1;
            this.bpCompareControl2.DataSourceTracked = false;
            this.bpCompareControl2.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.bpCompareControl2.IsActive = false;
            this.bpCompareControl2.Location = new System.Drawing.Point(595, 3);
            this.bpCompareControl2.Name = "bpCompareControl2";
            this.bpCompareControl2.ShouldReset = false;
            this.bpCompareControl2.Size = new System.Drawing.Size(583, 549);
            this.bpCompareControl2.TabIndex = 1;
            // 
            // BpCompareForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1184, 561);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BpCompareForm";
            this.Text = "BpCompareForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BpCompareForm_FormClosing);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private CommonControls.BpCompareControl bpCompareControl1;
        private CommonControls.BpCompareControl bpCompareControl2;
    }
}