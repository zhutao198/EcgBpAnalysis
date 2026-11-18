namespace Zoncare.Component.Controls
{
    partial class ParameterEditor1
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
            this.txtMeasure = new System.Windows.Forms.TextBox();
            this.labUnit = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMeasure
            // 
            this.txtMeasure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMeasure.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtMeasure.Location = new System.Drawing.Point(0, 1);
            this.txtMeasure.Margin = new System.Windows.Forms.Padding(0);
            this.txtMeasure.Name = "txtMeasure";
            this.txtMeasure.Size = new System.Drawing.Size(138, 23);
            this.txtMeasure.TabIndex = 0;
            this.txtMeasure.WordWrap = false;
            this.txtMeasure.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMeasure_KeyPress);
            // 
            // labUnit
            // 
            this.labUnit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labUnit.AutoSize = true;
            this.labUnit.Location = new System.Drawing.Point(138, 4);
            this.labUnit.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.labUnit.Name = "labUnit";
            this.labUnit.Size = new System.Drawing.Size(26, 17);
            this.labUnit.TabIndex = 1;
            this.labUnit.Text = "ms";
            this.labUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.labUnit, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtMeasure, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(174, 26);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // ParameterEditor1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
            this.Name = "ParameterEditor1";
            this.Size = new System.Drawing.Size(174, 26);
            this.Load += new System.EventHandler(this.MeasureEditor_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtMeasure;
        private System.Windows.Forms.Label labUnit;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
