using System;
using System.Drawing;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    public partial class StepTabControl : TabControl
    {
        private Color activeTabColor = Color.FromArgb(74, 122, 196);
        private Color inactiveTabColor = Color.Silver;
        private Color lineColor = Color.LightGray;
        private int lineYOffset = 10; // 直线距离选项卡底部的偏移量
        
        private bool _allowTabChange = false;

        public StepTabControl()
        {
            // 设置绘图样式
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.DoubleBuffer |
                          ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            // 设置选项卡尺寸
            this.ItemSize = new Size(120, 40);
            this.SizeMode = TabSizeMode.Fixed;
            this.Padding = new Point(10, 5);

            // 默认选中第一个
            this.SelectedIndex = 0;
        }

        public void SwitchTabPage(int pageIndex)
        {
            _allowTabChange = true;
            try
            {
                this.SelectedIndex = pageIndex;
            }
            finally
            {
                _allowTabChange = false;
            }
        }

        protected override void OnSelecting(TabControlCancelEventArgs e)
        {
            base.OnSelecting(e);

            if (!DesignMode)
            {
                if (!_allowTabChange) e.Cancel = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 清空背景
            g.Clear(this.Parent?.BackColor ?? SystemColors.Control);

            // 绘制连接线
            DrawConnectingLine(g);

            // 绘制选项卡头部
            for (int i = 0; i < this.TabPages.Count; i++)
            {
                bool isSelected = (i == this.SelectedIndex);
                Rectangle tabRect = this.GetTabRect(i);

                // 绘制选项卡背景
                //DrawTabBackground(g, tabRect, isSelected);

                // 绘制选项卡标题
                DrawTabTitle(g, tabRect, this.TabPages[i].Text, isSelected);

                // 绘制数字
                DrawTabNumber(g, tabRect, i + 1, isSelected);
            }

            // 绘制选项卡内容区域边框（可选）
            //DrawTabPageBorder(g);
        }

        /// <summary>
        /// 绘制选项卡背景
        /// </summary>
        private void DrawTabBackground(Graphics g, Rectangle tabRect, bool isSelected)
        {
            // 使用渐变或纯色背景
            Color backColor = isSelected ? Color.White : Color.LightGray;

            using (Brush brush = new SolidBrush(backColor))
            {
                g.FillRectangle(brush, tabRect);
            }

            // 绘制边框（只有选中的选项卡有边框效果）
            if (isSelected)
            {
                using (Pen pen = new Pen(Color.Black, 1))
                {
                    // 绘制除底部外的边框，避免与直线重叠
                    g.DrawLine(pen, tabRect.Left, tabRect.Bottom - 1, tabRect.Right, tabRect.Bottom - 1);
                }
            }
        }

        /// <summary>
        /// 绘制选项卡标题
        /// </summary>
        private void DrawTabTitle(Graphics g, Rectangle tabRect, string title, bool isSelected)
        {
            Color textColor = isSelected ? activeTabColor : SystemColors.InactiveCaptionText;

            using (Brush brush = new SolidBrush(textColor))
            using (Font font = new Font(this.Font, isSelected ? FontStyle.Bold : FontStyle.Regular))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                // 标题位置稍微上移，为数字留出空间
                Rectangle titleRect = new Rectangle(
                    tabRect.X,
                    tabRect.Y,
                    tabRect.Width,
                    tabRect.Height - 18);

                g.DrawString(title, font, brush, titleRect, sf);
            }
        }

        /// <summary>
        /// 绘制选项卡下方的数字
        /// </summary>
        private void DrawTabNumber(Graphics g, Rectangle tabRect, int number, bool isSelected)
        {
            Color numberColor = isSelected ? activeTabColor : inactiveTabColor;

            using (Brush brush = new SolidBrush(numberColor))
            using (Font font = new Font("Arial", 9, FontStyle.Bold))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Far
                };

                // 数字位置在选项卡底部
                Rectangle numberRect = new Rectangle(
                    tabRect.X,
                    tabRect.Bottom - 18,
                    tabRect.Width,
                    16);

                int middleX = tabRect.Left + tabRect.Width / 2;
                int middleY = numberRect.Top + numberRect.Height / 2;
                g.FillEllipse(brush, middleX - 8, middleY - 8, 16, 16);
                g.DrawString(number.ToString(), font, Brushes.White, numberRect, sf);
            }
        }

        /// <summary>
        /// 绘制从第一个选项卡延伸到第二个选项卡的直线
        /// </summary>
        private void DrawConnectingLine(Graphics g)
        {
            if (this.TabPages.Count < 2) return;

            // 获取第一个和最后一个选项卡的位置
            Rectangle firstTabRect = this.GetTabRect(0);
            Rectangle lastTabRect = this.GetTabRect(this.TabPages.Count - 1);

            // 计算直线的Y坐标（在选项卡底部稍下方）
            int lineY = firstTabRect.Bottom - lineYOffset;

            // 计算直线的起始和结束X坐标
            int startX = firstTabRect.Left + 20;  // 从第一个选项卡内部开始
            int endX = lastTabRect.Right - 20;    // 到最后一个选项卡内部结束

            using (Pen pen = new Pen(lineColor, 1))
            {
                // 绘制主直线
                g.DrawLine(pen, startX, lineY, endX, lineY);
            }
        }

        /// <summary>
        /// 绘制选项卡内容区域的边框（可选）
        /// </summary>
        private void DrawTabPageBorder(Graphics g)
        {
            // 获取第一个选项卡的底部Y坐标
            if (this.TabPages.Count > 0)
            {
                Rectangle firstTabRect = this.GetTabRect(0);
                int lineY = firstTabRect.Bottom + lineYOffset;

                // 绘制内容区域的顶部边框（与直线区分）
                using (Pen pen = new Pen(Color.Gray, 1))
                {
                    int topY = lineY + 5;
                    g.DrawLine(pen, this.ClientRectangle.Left, topY, this.ClientRectangle.Right, topY);
                }
            }
        }

        // 重写 OnSelectedIndexChanged 以确保选中状态变化时重绘
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            this.Invalidate(); // 强制重绘
        }

        // 处理选项卡大小变化
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        // 可选的属性：允许外部设置颜色
        public Color ActiveTabColor
        {
            get => activeTabColor;
            set { activeTabColor = value; this.Invalidate(); }
        }

        public Color InactiveTabColor
        {
            get => inactiveTabColor;
            set { inactiveTabColor = value; this.Invalidate(); }
        }

        public Color LineColor
        {
            get => lineColor;
            set { lineColor = value; this.Invalidate(); }
        }
    }
}
