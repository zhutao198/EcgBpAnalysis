namespace EcgAnalyseStart
{
    partial class Form1
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
            this.skiaControl = new SkiaSharp.Views.Desktop.SKGLControl();
            this.button1 = new System.Windows.Forms.Button();
            this.overlayControl = new SkiaSharp.Views.Desktop.SKGLControl();
            this.SuspendLayout();
            // 
            // skiaControl
            // 
            this.skiaControl.BackColor = System.Drawing.Color.White;
            this.skiaControl.Location = new System.Drawing.Point(3, 12);
            this.skiaControl.Name = "skiaControl";
            this.skiaControl.Size = new System.Drawing.Size(560, 644);
            this.skiaControl.TabIndex = 1;
            this.skiaControl.VSync = false;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(569, 591);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 65);
            this.button1.TabIndex = 2;
            this.button1.Text = "加载散点\r\n**模拟生成心电波形数据";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // overlayControl
            // 
            this.overlayControl.BackColor = System.Drawing.Color.White;
            this.overlayControl.Location = new System.Drawing.Point(673, 12);
            this.overlayControl.Name = "overlayControl";
            this.overlayControl.Size = new System.Drawing.Size(482, 644);
            this.overlayControl.TabIndex = 3;
            this.overlayControl.VSync = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1168, 675);
            this.Controls.Add(this.overlayControl);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.skiaControl);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private SkiaSharp.Views.Desktop.SKGLControl skiaControl;
        private System.Windows.Forms.Button button1;
        private SkiaSharp.Views.Desktop.SKGLControl overlayControl;
    }
}