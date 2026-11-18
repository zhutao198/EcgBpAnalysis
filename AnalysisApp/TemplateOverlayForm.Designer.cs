namespace AnalysisApp
{
    partial class TemplateOverlayForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TemplateOverlayForm));
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.ecgCommonWave1 = new AnalysisApp.CommonControls.DrawEcgCommonWave();
            this.tlpScatter1 = new System.Windows.Forms.TableLayoutPanel();
            this.tlpOverlay1 = new System.Windows.Forms.TableLayoutPanel();
            this.overlayPlot01 = new AnalysisApp.CommonControls.OverlayPlot();
            this.overlayPlot02 = new AnalysisApp.CommonControls.OverlayPlot();
            this.overlayPlot03 = new AnalysisApp.CommonControls.OverlayPlot();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.scatterPlot1 = new AnalysisApp.CommonControls.ScatterPlot();
            this.scatterPlot2 = new AnalysisApp.CommonControls.ScatterPlot();
            this.scatterPlot3 = new AnalysisApp.CommonControls.ScatterPlot();
            this.beatWaveList1 = new AnalysisApp.CommonControls.BeatWaveList();
            this.tableLayoutPanel7.SuspendLayout();
            this.tlpScatter1.SuspendLayout();
            this.tlpOverlay1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Controls.Add(this.ecgCommonWave1, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.tlpScatter1, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 2;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(1261, 697);
            this.tableLayoutPanel7.TabIndex = 4;
            // 
            // ecgCommonWave1
            // 
            this.ecgCommonWave1.BackColor = System.Drawing.Color.White;
            this.ecgCommonWave1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ecgCommonWave1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ecgCommonWave1.Location = new System.Drawing.Point(3, 456);
            this.ecgCommonWave1.Name = "ecgCommonWave1";
            this.ecgCommonWave1.Size = new System.Drawing.Size(1255, 238);
            this.ecgCommonWave1.TabIndex = 2;
            // 
            // tlpScatter1
            // 
            this.tlpScatter1.ColumnCount = 3;
            this.tlpScatter1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpScatter1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpScatter1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpScatter1.Controls.Add(this.tlpOverlay1, 0, 0);
            this.tlpScatter1.Controls.Add(this.tableLayoutPanel1, 1, 0);
            this.tlpScatter1.Controls.Add(this.beatWaveList1, 2, 0);
            this.tlpScatter1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpScatter1.Location = new System.Drawing.Point(0, 0);
            this.tlpScatter1.Margin = new System.Windows.Forms.Padding(0);
            this.tlpScatter1.Name = "tlpScatter1";
            this.tlpScatter1.RowCount = 1;
            this.tlpScatter1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpScatter1.Size = new System.Drawing.Size(1261, 453);
            this.tlpScatter1.TabIndex = 1;
            // 
            // tlpOverlay1
            // 
            this.tlpOverlay1.AutoSize = true;
            this.tlpOverlay1.ColumnCount = 1;
            this.tlpOverlay1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOverlay1.Controls.Add(this.overlayPlot01, 0, 0);
            this.tlpOverlay1.Controls.Add(this.overlayPlot02, 0, 1);
            this.tlpOverlay1.Controls.Add(this.overlayPlot03, 0, 2);
            this.tlpOverlay1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOverlay1.Location = new System.Drawing.Point(3, 3);
            this.tlpOverlay1.Name = "tlpOverlay1";
            this.tlpOverlay1.RowCount = 3;
            this.tlpOverlay1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpOverlay1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpOverlay1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpOverlay1.Size = new System.Drawing.Size(205, 447);
            this.tlpOverlay1.TabIndex = 2;
            // 
            // overlayPlot01
            // 
            this.overlayPlot01.Amplitude = 1000;
            this.overlayPlot01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(19)))), ((int)(((byte)(92)))));
            this.overlayPlot01.BeatsPerWave = 1;
            this.overlayPlot01.Dock = System.Windows.Forms.DockStyle.Fill;
            this.overlayPlot01.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.overlayPlot01.GainRatio = 1F;
            this.overlayPlot01.IsActived = false;
            this.overlayPlot01.IsMainOverlay = false;
            this.overlayPlot01.LeadIndex = 1;
            this.overlayPlot01.Location = new System.Drawing.Point(3, 3);
            this.overlayPlot01.Name = "overlayPlot01";
            this.overlayPlot01.SelectMode = 0;
            this.overlayPlot01.Size = new System.Drawing.Size(199, 143);
            this.overlayPlot01.SpeedRatio = 1F;
            this.overlayPlot01.TabIndex = 0;
            this.overlayPlot01.Title = "II-[F1]-0";
            // 
            // overlayPlot02
            // 
            this.overlayPlot02.Amplitude = 1000;
            this.overlayPlot02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(19)))), ((int)(((byte)(92)))));
            this.overlayPlot02.BeatsPerWave = 1;
            this.overlayPlot02.Dock = System.Windows.Forms.DockStyle.Fill;
            this.overlayPlot02.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.overlayPlot02.GainRatio = 1F;
            this.overlayPlot02.IsActived = false;
            this.overlayPlot02.IsMainOverlay = false;
            this.overlayPlot02.LeadIndex = 1;
            this.overlayPlot02.Location = new System.Drawing.Point(3, 152);
            this.overlayPlot02.Name = "overlayPlot02";
            this.overlayPlot02.SelectMode = 0;
            this.overlayPlot02.Size = new System.Drawing.Size(199, 143);
            this.overlayPlot02.SpeedRatio = 1F;
            this.overlayPlot02.TabIndex = 1;
            this.overlayPlot02.Title = "II-[F2]-0";
            // 
            // overlayPlot03
            // 
            this.overlayPlot03.Amplitude = 1000;
            this.overlayPlot03.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(19)))), ((int)(((byte)(92)))));
            this.overlayPlot03.BeatsPerWave = 1;
            this.overlayPlot03.Dock = System.Windows.Forms.DockStyle.Fill;
            this.overlayPlot03.Font = new System.Drawing.Font("微软雅黑", 10.5F);
            this.overlayPlot03.GainRatio = 1F;
            this.overlayPlot03.IsActived = false;
            this.overlayPlot03.IsMainOverlay = false;
            this.overlayPlot03.LeadIndex = 1;
            this.overlayPlot03.Location = new System.Drawing.Point(3, 301);
            this.overlayPlot03.Name = "overlayPlot03";
            this.overlayPlot03.SelectMode = 0;
            this.overlayPlot03.Size = new System.Drawing.Size(199, 143);
            this.overlayPlot03.SpeedRatio = 1F;
            this.overlayPlot03.TabIndex = 2;
            this.overlayPlot03.Title = "II-[F3]-0";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.scatterPlot1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.scatterPlot2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.scatterPlot3, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(214, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(466, 447);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.AutoSize = true;
            this.scatterPlot1.BackColor = System.Drawing.Color.White;
            this.scatterPlot1.Dock = System.Windows.Forms.DockStyle.Left;
            this.scatterPlot1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.scatterPlot1.HideToolStrip = true;
            this.scatterPlot1.Location = new System.Drawing.Point(3, 3);
            this.scatterPlot1.Name = "scatterPlot1";
            this.scatterPlot1.Size = new System.Drawing.Size(460, 143);
            this.scatterPlot1.TabIndex = 0;
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.AutoSize = true;
            this.scatterPlot2.BackColor = System.Drawing.Color.White;
            this.scatterPlot2.Dock = System.Windows.Forms.DockStyle.Left;
            this.scatterPlot2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.scatterPlot2.HideToolStrip = true;
            this.scatterPlot2.Location = new System.Drawing.Point(3, 152);
            this.scatterPlot2.Name = "scatterPlot2";
            this.scatterPlot2.Size = new System.Drawing.Size(460, 143);
            this.scatterPlot2.TabIndex = 1;
            // 
            // scatterPlot3
            // 
            this.scatterPlot3.AutoSize = true;
            this.scatterPlot3.BackColor = System.Drawing.Color.White;
            this.scatterPlot3.Dock = System.Windows.Forms.DockStyle.Left;
            this.scatterPlot3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.scatterPlot3.HideToolStrip = true;
            this.scatterPlot3.Location = new System.Drawing.Point(3, 301);
            this.scatterPlot3.Name = "scatterPlot3";
            this.scatterPlot3.Size = new System.Drawing.Size(460, 143);
            this.scatterPlot3.TabIndex = 2;
            // 
            // beatWaveList1
            // 
            this.beatWaveList1.BackColor = System.Drawing.Color.White;
            this.beatWaveList1.ColCount = 4;
            this.beatWaveList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.beatWaveList1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.beatWaveList1.Gain = 5F;
            this.beatWaveList1.LeadCount = 1;
            this.beatWaveList1.Location = new System.Drawing.Point(686, 3);
            this.beatWaveList1.MultiLeads = ((System.Collections.Generic.List<int>)(resources.GetObject("beatWaveList1.MultiLeads")));
            this.beatWaveList1.Name = "beatWaveList1";
            this.beatWaveList1.RowCount = 5;
            this.beatWaveList1.ShowBatchAddToggle = false;
            this.beatWaveList1.ShowCommonWaveToggle = false;
            this.beatWaveList1.ShowHistogramToggle = false;
            this.beatWaveList1.ShowTime = false;
            this.beatWaveList1.Size = new System.Drawing.Size(572, 447);
            this.beatWaveList1.Speed = 25F;
            this.beatWaveList1.TabIndex = 4;
            this.beatWaveList1.WaveLeads = ((System.Collections.Generic.List<int>)(resources.GetObject("beatWaveList1.WaveLeads")));
            // 
            // TemplateOverlayForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1261, 697);
            this.Controls.Add(this.tableLayoutPanel7);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "TemplateOverlayForm";
            this.ShowInTaskbar = false;
            this.Text = "TemplateOverlayForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TemplateOverlayForm_FormClosing);
            this.Load += new System.EventHandler(this.TemplateOverlayForm_Load);
            this.Controls.SetChildIndex(this.tableLayoutPanel7, 0);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tlpScatter1.ResumeLayout(false);
            this.tlpScatter1.PerformLayout();
            this.tlpOverlay1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private CommonControls.DrawEcgCommonWave ecgCommonWave1;
        private System.Windows.Forms.TableLayoutPanel tlpScatter1;
        private System.Windows.Forms.TableLayoutPanel tlpOverlay1;
        private CommonControls.OverlayPlot overlayPlot03;
        private CommonControls.OverlayPlot overlayPlot02;
        private CommonControls.OverlayPlot overlayPlot01;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private CommonControls.ScatterPlot scatterPlot3;
        private CommonControls.ScatterPlot scatterPlot2;
        private CommonControls.ScatterPlot scatterPlot1;
        private CommonControls.BeatWaveList beatWaveList1;
    }
}