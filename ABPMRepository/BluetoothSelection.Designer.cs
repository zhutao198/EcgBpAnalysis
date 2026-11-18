namespace ABPMRepository
{
    partial class BluetoothConnection
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.picConn = new System.Windows.Forms.PictureBox();
            this.cbxBluetooth = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picConn)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(110, 72);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(60, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // picConn
            // 
            this.picConn.Location = new System.Drawing.Point(171, 28);
            this.picConn.Name = "picConn";
            this.picConn.Size = new System.Drawing.Size(16, 16);
            this.picConn.TabIndex = 17;
            this.picConn.TabStop = false;
            // 
            // cbxBluetooth
            // 
            this.cbxBluetooth.Enabled = false;
            this.cbxBluetooth.FormattingEnabled = true;
            this.cbxBluetooth.Location = new System.Drawing.Point(12, 26);
            this.cbxBluetooth.Name = "cbxBluetooth";
            this.cbxBluetooth.Size = new System.Drawing.Size(158, 20);
            this.cbxBluetooth.TabIndex = 15;
            this.cbxBluetooth.DropDown += new System.EventHandler(this.cbxBluetooth_DropDown);
            this.cbxBluetooth.SelectedIndexChanged += new System.EventHandler(this.cbxBluetooth_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 11);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(89, 12);
            this.label12.TabIndex = 16;
            this.label12.Text = "选择蓝牙设备：";
            // 
            // BluetoothConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(191, 102);
            this.Controls.Add(this.picConn);
            this.Controls.Add(this.cbxBluetooth);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "BluetoothConnection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "蓝牙连接";
            this.Load += new System.EventHandler(this.BluetoothConnection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picConn)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox picConn;
        private System.Windows.Forms.ComboBox cbxBluetooth;
        private System.Windows.Forms.Label label12;
    }
}