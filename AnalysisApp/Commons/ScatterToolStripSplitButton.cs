using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    public class ScatterToolStripSplitButton : ToolStripSplitButton, IScatterToolStripItem
    {
        public ScatterType ZqScatterType { get; set; }

        public bool UserSelected
        {
            get => _userSelected;
            set
            {
                ToolStrip tsParent = this.GetCurrentParent();
                if (value == true)
                {
                    foreach (ToolStripItem item in tsParent.Items)
                    {
                        IScatterToolStripItem scatterItem = item as IScatterToolStripItem;
                        if (scatterItem != null)
                        {
                            scatterItem.UserSelected = false;
                        }
                    }
                }
                else
                {
                    ZqScatterType.Title = ZqScatterType.Title.TrimEnd('\'');
                }

                _userSelected = value;
                tsParent.Invalidate();
            }
        }

        public RectangleF CodeRect { get; set; }

        public int TitleFontSize { get; set; }

        private const int ArrowPadding = 2; // 箭头与主体之间的间距

        private static Dictionary<char, Color> _gdiColorMap = AnaCode.ConvertToGDIColor(AnaValues.WaveformColorMap);
        private bool _userSelected;

        public ScatterToolStripSplitButton() { }

        public ScatterToolStripSplitButton(string text) : base(text)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DesignMode) return;
            if (ZqScatterType == null) return;

            Graphics g = e.Graphics;
            int arrowWidth = ContentRectangle.Width - ButtonBounds.Width;
            Rectangle buttonRect = getButtonRectangle(this.ContentRectangle);

            if (Selected)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(220, 220, 240)))
                    g.FillRectangle(brush, buttonRect);
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                    g.FillRectangle(brush, buttonRect);
            }

            //bool subItemChecked = false;
            //foreach (ToolStripItem item in this.DropDownItems)
            //{
            //    ToolStripMenuItem menuItem = item as ToolStripMenuItem;
            //    if (menuItem!=null && menuItem.Checked)
            //    {
            //        subItemChecked = true;
            //    }
            //}
            //if (subItemChecked) this.BackColor = Color.Blue;

            Color textColor = Color.Black;
            string beatType = ZqScatterType.Code;
            if (beatType.Length > 0)
            {
                char typeCode = beatType[0];
                textColor = _gdiColorMap.ContainsKey(typeCode) ? _gdiColorMap[typeCode] : Color.Black;
            }

            //填充背景色
            if (CodeRect == RectangleF.Empty)
            {
                int codeWidth = (Width - arrowWidth) / 2 + 1;
                CodeRect = new RectangleF((this.Owner.Width - arrowWidth - codeWidth) / 2F, 2, codeWidth, Height / 2);
            }
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(177, 208, 255)))
            {
                if (!UserSelected)
                    g.FillRectangle(Brushes.White, CodeRect);
                else
                    g.FillRectangle(brush, CodeRect);
            }

            //绘制图标
            using (Font typeFont = new Font(this.Font.FontFamily, TitleFontSize, FontStyle.Bold, GraphicsUnit.Pixel))
            using (SolidBrush brush = new SolidBrush(textColor))
            using (StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None,
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
            })
            {
                g.DrawString(ZqScatterType.Title, typeFont, brush, CodeRect, sf);
            }

            //绘制下方文本或数字
            using (StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Far,
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
            })
            {
                float fontSize = 9F * Math.Max(0.75F, CommonValues.HeightScale);
                using (Font textFont = new Font(this.Font.FontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Point))
                {
                    RectangleF layoutRect = new RectangleF(-100F, ContentRectangle.Top, ContentRectangle.Width + 100 - arrowWidth + 4, ContentRectangle.Height);
                    g.DrawString(ZqScatterType.Count.ToString(), textFont, Brushes.Black, layoutRect, sf);
                }
            }

            //调用基类绘制箭头区域, 为了不影响主体, 需要裁剪绘制区域
            Rectangle arrowRect = getArrowRectangle(this.ContentRectangle, arrowWidth);
            Region clipRegion = new Region(arrowRect);
            Region originalClip = g.Clip;
            try
            {
                // 设置裁剪区域仅为箭头区域
                g.SetClip(arrowRect);
                // 调用基类绘制箭头
                this.Text = string.Empty;
                this.Image = null;
                base.OnPaint(e);
            }
            finally
            {
                g.Clip = originalClip; //恢复原来的裁剪区域
            }
        }

        private Rectangle getButtonRectangle(Rectangle contentRect)
        {
            //int arrowWidth = ArrowWidth + ArrowPadding;
            //return new Rectangle(
            //    contentRect.X,
            //    contentRect.Y,
            //    contentRect.Width - arrowWidth + 4,
            //    contentRect.Height
            //);
            return new Rectangle(contentRect.X, contentRect.Y, ButtonBounds.Width, contentRect.Height);
        }

        private Rectangle getArrowRectangle(Rectangle contentRect, int arrowWidth)
        {
            //int arrowWidth = ArrowWidth + ArrowPadding;
            return new Rectangle(
                contentRect.X + contentRect.Width - arrowWidth,
                contentRect.Y,
                arrowWidth,
                contentRect.Height
            );
        }

    }
}
