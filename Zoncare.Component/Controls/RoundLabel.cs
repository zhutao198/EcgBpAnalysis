using System.Drawing;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    public class RoundLabel : PictureBox
    {
        //背景
        public Color CircleColor { get; set; } = Color.FromArgb(74, 122, 196);
        //显示文本
        public string DisplayText { get; set; } = "";

        public RoundLabel()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(32, 32);
            // 确保背景色透明，让我们自己绘制背景
            this.BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. 绘制圆形背景
            // 使用 FillEllipse 绘制填充圆 [citation:12]
            using (Brush brush = new SolidBrush(this.CircleColor))
            {
                // 绘制在控件的客户区矩形内
                g.FillEllipse(brush, this.ClientRectangle);
            }

            // 2. 在圆形中心绘制字符
            // 使用 DrawString 绘制文本 [citation:9][citation:12]
            using (Font font = new Font("Arial", this.Width * 0.5f, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.White))
            {
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                // 计算用于绘制文字的矩形，留出内边距
                RectangleF rect = new RectangleF(2, 2, this.Width - 4, this.Height - 4);

                g.DrawString(this.DisplayText, font, brush, rect, stringFormat);
            }
        }
    }
}