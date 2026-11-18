namespace AnalysisApp
{
    partial class FullWaveForm
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
            this.ecgDiagnosisControl1 = new AnalysisApp.EcgDiagnosisControl();
            this.SuspendLayout();
            // 
            // ecgDiagnosisControl1
            // 
            this.ecgDiagnosisControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(236)))), ((int)(((byte)(248)))));
            this.ecgDiagnosisControl1.DataSourceTracked = false;
            this.ecgDiagnosisControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ecgDiagnosisControl1.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.ecgDiagnosisControl1.IsActive = false;
            this.ecgDiagnosisControl1.Location = new System.Drawing.Point(0, 0);
            this.ecgDiagnosisControl1.Name = "ecgDiagnosisControl1";
            this.ecgDiagnosisControl1.Size = new System.Drawing.Size(1161, 715);
            this.ecgDiagnosisControl1.TabIndex = 18;
            // 
            // FullWaveForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1161, 715);
            this.Controls.Add(this.ecgDiagnosisControl1);
            this.Name = "FullWaveForm";
            this.ShowInTaskbar = false;
            this.Text = "FullWaveForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FullWaveForm_FormClosing);
            this.Load += new System.EventHandler(this.FullWaveForm_Load);
            this.Controls.SetChildIndex(this.ecgDiagnosisControl1, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private EcgDiagnosisControl ecgDiagnosisControl1;
    }
}