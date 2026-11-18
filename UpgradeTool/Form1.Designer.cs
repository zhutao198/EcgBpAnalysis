namespace UpgradeTool
{
    partial class Form1
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnMigration = new System.Windows.Forms.Button();
            this.btnUpgradeDB = new System.Windows.Forms.Button();
            this.btnBrowse1 = new System.Windows.Forms.Button();
            this.tbxSourceFilePath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnBrowse2 = new System.Windows.Forms.Button();
            this.tbxTargetFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnMigration
            // 
            this.btnMigration.BackColor = System.Drawing.SystemColors.Control;
            this.btnMigration.ForeColor = System.Drawing.Color.Black;
            this.btnMigration.Location = new System.Drawing.Point(478, 20);
            this.btnMigration.Name = "btnMigration";
            this.btnMigration.Size = new System.Drawing.Size(75, 53);
            this.btnMigration.TabIndex = 0;
            this.btnMigration.Text = "数据迁移";
            this.btnMigration.UseVisualStyleBackColor = false;
            // 
            // btnUpgradeDB
            // 
            this.btnUpgradeDB.Location = new System.Drawing.Point(85, 93);
            this.btnUpgradeDB.Name = "btnUpgradeDB";
            this.btnUpgradeDB.Size = new System.Drawing.Size(84, 28);
            this.btnUpgradeDB.TabIndex = 1;
            this.btnUpgradeDB.Text = "升级数据库";
            this.btnUpgradeDB.UseVisualStyleBackColor = true;
            this.btnUpgradeDB.Click += new System.EventHandler(this.btnUpgradeDB_Click);
            // 
            // btnBrowse1
            // 
            this.btnBrowse1.Location = new System.Drawing.Point(406, 21);
            this.btnBrowse1.Name = "btnBrowse1";
            this.btnBrowse1.Size = new System.Drawing.Size(54, 23);
            this.btnBrowse1.TabIndex = 54;
            this.btnBrowse1.Text = "浏览";
            this.btnBrowse1.UseVisualStyleBackColor = true;
            // 
            // tbxSourceFilePath
            // 
            this.tbxSourceFilePath.Location = new System.Drawing.Point(85, 23);
            this.tbxSourceFilePath.Name = "tbxSourceFilePath";
            this.tbxSourceFilePath.Size = new System.Drawing.Size(315, 21);
            this.tbxSourceFilePath.TabIndex = 53;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 52;
            this.label6.Text = "源文件目录";
            // 
            // btnBrowse2
            // 
            this.btnBrowse2.Location = new System.Drawing.Point(406, 48);
            this.btnBrowse2.Name = "btnBrowse2";
            this.btnBrowse2.Size = new System.Drawing.Size(54, 23);
            this.btnBrowse2.TabIndex = 57;
            this.btnBrowse2.Text = "浏览";
            this.btnBrowse2.UseVisualStyleBackColor = true;
            // 
            // tbxTargetFilePath
            // 
            this.tbxTargetFilePath.Location = new System.Drawing.Point(85, 50);
            this.tbxTargetFilePath.Name = "tbxTargetFilePath";
            this.tbxTargetFilePath.Size = new System.Drawing.Size(315, 21);
            this.tbxTargetFilePath.TabIndex = 56;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 55;
            this.label1.Text = "新文件目录";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 136);
            this.Controls.Add(this.btnBrowse2);
            this.Controls.Add(this.tbxTargetFilePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBrowse1);
            this.Controls.Add(this.tbxSourceFilePath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnUpgradeDB);
            this.Controls.Add(this.btnMigration);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "升级工具";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnMigration;
        private System.Windows.Forms.Button btnUpgradeDB;
        private System.Windows.Forms.Button btnBrowse1;
        private System.Windows.Forms.TextBox tbxSourceFilePath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnBrowse2;
        private System.Windows.Forms.TextBox tbxTargetFilePath;
        private System.Windows.Forms.Label label1;
    }
}

