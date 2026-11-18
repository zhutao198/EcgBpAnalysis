namespace Zoncare.Component.Controls
{
    partial class ParameterEditor2
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
            this.txtMeasure1 = new System.Windows.Forms.TextBox();
            this.txtMeasure2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labUnit = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMeasure1
            // 
            this.txtMeasure1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMeasure1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtMeasure1.Location = new System.Drawing.Point(0, 0);
            this.txtMeasure1.Margin = new System.Windows.Forms.Padding(0);
            this.txtMeasure1.Name = "txtMeasure1";
            this.txtMeasure1.Size = new System.Drawing.Size(53, 23);
            this.txtMeasure1.TabIndex = 0;
            this.txtMeasure1.WordWrap = false;
            this.txtMeasure1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMeasure_KeyPress);
            // 
            // txtMeasure2
            // 
            this.txtMeasure2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMeasure2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtMeasure2.Location = new System.Drawing.Point(65, 0);
            this.txtMeasure2.Margin = new System.Windows.Forms.Padding(0);
            this.txtMeasure2.Name = "txtMeasure2";
            this.txtMeasure2.Size = new System.Drawing.Size(53, 23);
            this.txtMeasure2.TabIndex = 1;
            this.txtMeasure2.WordWrap = false;
            this.txtMeasure2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMeasure_KeyPress);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(53, 2);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "/";
            // 
            // labUnit
            // 
            this.labUnit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labUnit.AutoSize = true;
            this.labUnit.Location = new System.Drawing.Point(118, 2);
            this.labUnit.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.labUnit.Name = "labUnit";
            this.labUnit.Size = new System.Drawing.Size(26, 17);
            this.labUnit.TabIndex = 3;
            this.labUnit.Text = "ms";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.txtMeasure1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labUnit, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtMeasure2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(154, 21);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // ParameterEditor2
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
            this.Name = "ParameterEditor2";
            this.Size = new System.Drawing.Size(154, 21);
            this.Load += new System.EventHandler(this.ParameterEditor2_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtMeasure1;
        private System.Windows.Forms.TextBox txtMeasure2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labUnit;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
