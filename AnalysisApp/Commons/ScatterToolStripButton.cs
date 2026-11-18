using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    public class ScatterToolStripButton : ToolStripButton, IScatterToolStripItem
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

                            if (scatterItem is ScatterToolStripSplitButton dropdownItem)
                            {
                                dropdownItem.CheckChildItem((subItem) => { return false; });
                            }
                        }
                    }
                }

                _userSelected = value;
                tsParent.Invalidate();
            }
        }

        public RectangleF CodeRect { get; set; }

        public int TitleFontSize { get; set; }

        private static Dictionary<char, Color> _gdiColorMap = AnaCode.ConvertToGDIColor(AnaValues.WaveformColorMap);
        private bool _userSelected;

        public ScatterToolStripButton() { }

        public ScatterToolStripButton(string text) : base(text)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DesignMode) return;

            Graphics g = e.Graphics;
            if (Selected)
                g.Clear(Color.FromArgb(220, 220, 240));
            else
                g.Clear(this.BackColor);


            if (ZqScatterType == null) return;

            int clientWidth = this.ContentRectangle.Width;
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
                int codeWidth = Width / 2 + 1;
                CodeRect = new RectangleF((this.Owner.Width - codeWidth) / 2F, 2, codeWidth, Height / 2);
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
            using (StringFormat sf2 = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
            {
                if (ZqScatterType.Count == -1) //用于ST扫描
                    g.DrawString(ZqScatterType.Title, typeFont, brush, CodeRect, sf2);
                else
                    g.DrawString(ZqScatterType.Title, typeFont, brush, CodeRect, sf);
            }

            //绘制下方文本或数字
            using (StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Far,
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
            })
            using (StringFormat sf2 = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Far
            })
            {
                float fontSize = 9F * Math.Max(0.75F, CommonValues.HeightScale);
                if (ZqScatterType.Count == -1)
                {
                    using (Font textFont = new Font(this.Font.FontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Point))
                    {
                        g.DrawString(ZqScatterType.Footer, textFont, Brushes.Black, ContentRectangle, sf2);
                    }
                }
                else
                {
                    using (Font textFont = new Font(this.Font.FontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Point))
                    {
                        RectangleF layoutRect = new RectangleF(-100F, ContentRectangle.Top, ContentRectangle.Width + 100, ContentRectangle.Height);
                        g.DrawString(ZqScatterType.Count.ToString(), textFont, Brushes.Black, layoutRect, sf);
                    }
                }
            }
        }
    }
}
