using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Zoncare.Utilities;

namespace Zoncare.Component.Controls
{
    /// <summary>
    /// 用 Button 模拟 ToolStripDropDownButton 下拉选项显示风格的分割按钮。
    /// 右侧为下拉箭头操作区, 与主按钮操作区之间以短竖线分隔;
    /// 鼠标位于箭头区域时竖线同步突出显示, 点击箭头区域弹出下拉选项。
    /// </summary>
    public class DropDownButton : Button
    {
        private ContextMenuStrip _dropDownMenu; //下拉选项菜单
        private Color _baseColor = Color.FromArgb(84, 132, 207); //基颜色(与RoundButton保持一致)
        private ControlState _controlState; //控件状态
        private int _arrowWidth = 18; //右侧箭头区域宽度
        private int _radius = 4; //圆角半径
        private bool _isHightlight; //是否突出显示按钮
        private bool _hoverArrow; //鼠标是否悬浮在箭头区域
        private bool _hoverMain; //鼠标是否悬浮在主按钮操作区
        private bool _clickInArrow; //本次点击是否发生在箭头区域(用于区分主操作/下拉操作)
        private bool _pressInArrow; //当前按下动作是否开始于箭头区域(用于绘制按下态)

        public DropDownButton() : base()
        {
            this.SetStyle(ControlStyles.UserPaint | //控件自行绘制，而不使用操作系统的绘制
                          ControlStyles.AllPaintingInWmPaint | //忽略擦出的消息，减少闪烁。
                          ControlStyles.OptimizedDoubleBuffer | //在缓冲区上绘制，不直接绘制到屏幕上，减少闪烁。
                          ControlStyles.ResizeRedraw | //控件大小发生变化时，重绘。
                          ControlStyles.SupportsTransparentBackColor, true); //支持透明背景颜色
        }

        #region --属性--
        /// <summary>
        /// 下拉选项菜单(点击箭头区域弹出)
        /// </summary>
        [DefaultValue(null), Description("点击箭头区域时弹出的下拉菜单")]
        public ContextMenuStrip DropDownMenu
        {
            get { return _dropDownMenu; }
            set { _dropDownMenu = value; }
        }

        [Description("主基调颜色"), DefaultValue(typeof(Color), "84, 132, 207")]
        public Color BaseColor
        {
            get { return _baseColor; }
            set
            {
                _baseColor = value;
                base.Invalidate();
            }
        }

        [Description("右侧箭头区域的宽度(最小值为12px)"), DefaultValue(18)]
        public int ArrowWidth
        {
            get { return _arrowWidth; }
            set
            {
                if (_arrowWidth != value)
                {
                    _arrowWidth = value < 12 ? 12 : value;
                    base.Invalidate();
                }
            }
        }

        [Description("指定按钮的圆角半径(最小值为4px)"), DefaultValue(4)]
        public int Radius
        {
            get { return _radius; }
            set
            {
                if (_radius != value)
                {
                    _radius = value < 4 ? 4 : value;
                    base.Invalidate();
                }
            }
        }

        [Browsable(true), Description("是否突出显示按钮"), DefaultValue(false)]
        public bool IsHighlight
        {
            get { return _isHightlight; }
            set
            {
                _isHightlight = value;
                base.Invalidate();
            }
        }

        /// <summary>
        /// 控件的鼠标状态(正常状态/鼠标指针悬浮其上/鼠标点击)
        /// </summary>
        protected ControlState ControlState
        {
            get { return _controlState; }
            set
            {
                if (_controlState != value)
                {
                    _controlState = value;
                    base.Invalidate();
                }
            }
        }

        /// <summary>
        /// 鼠标是否悬浮在箭头区域
        /// </summary>
        protected bool HoverArrow
        {
            get { return _hoverArrow; }
            set
            {
                if (_hoverArrow != value)
                {
                    _hoverArrow = value;
                    base.Invalidate();
                }
            }
        }

        /// <summary>
        /// 鼠标是否悬浮在主按钮操作区
        /// </summary>
        protected bool HoverMain
        {
            get { return _hoverMain; }
            set
            {
                if (_hoverMain != value)
                {
                    _hoverMain = value;
                    base.Invalidate();
                }
            }
        }

        /// <summary>
        /// 主按钮操作区(不含箭头区域)
        /// </summary>
        [Browsable(false)]
        public Rectangle MainRectangle
        {
            get { return new Rectangle(0, 0, Math.Max(this.Width - _arrowWidth, 0), this.Height); }
        }

        /// <summary>
        /// 下拉箭头操作区
        /// </summary>
        [Browsable(false)]
        public Rectangle ArrowRectangle
        {
            get
            {
                int mainWidth = MainRectangle.Width;
                return new Rectangle(mainWidth, 0, Math.Max(this.Width - mainWidth, 0), this.Height);
            }
        }
        #endregion

        #region --事件--
        /// <summary>
        /// 点击下拉箭头区域时触发(若已设置DropDownMenu, 弹出菜单之后触发)
        /// </summary>
        [Description("点击下拉箭头区域时触发")]
        public event EventHandler ArrowClick;
        #endregion

        #region --鼠标对应的几个事件--
        protected override void OnMouseEnter(EventArgs e)//鼠标进入时
        {
            base.OnMouseEnter(e);
            ControlState = ControlState.Hover;

            //按鼠标进入时的实际位置初始化分区悬浮状态
            Point mousePoint = this.PointToClient(Control.MousePosition);
            bool overArrow = ArrowRectangle.Contains(mousePoint);
            HoverArrow = overArrow;
            HoverMain = !overArrow && MainRectangle.Contains(mousePoint);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool overArrow = ArrowRectangle.Contains(e.Location);
            HoverArrow = overArrow;
            HoverMain = !overArrow && MainRectangle.Contains(e.Location);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            HoverArrow = false;
            HoverMain = false;
            _pressInArrow = false;
            ControlState = ControlState.Normal;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                _clickInArrow = ArrowRectangle.Contains(e.Location);
                _pressInArrow = _clickInArrow;
                ControlState = ControlState.Pressed;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                _pressInArrow = false;
                ControlState = ClientRectangle.Contains(e.Location) ? ControlState.Hover : ControlState.Normal;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            ControlState = ControlState.Hover;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            ControlState = ControlState.Normal;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!this.Enabled) ControlState = ControlState.Normal;
        }

        protected override void OnClick(EventArgs e)//点击: 箭头区域弹下拉菜单, 主区域触发Click
        {
            if (_clickInArrow) //点击发生在箭头区域, 触发下拉
            {
                ShowDropDown();
                OnArrowClick(EventArgs.Empty);
                _clickInArrow = false; //复位, 避免键盘(空格)触发Click时误判为箭头区域
            }
            else //点击发生在主按钮区域, 按普通按钮处理
            {
                base.OnClick(e);
            }
        }

        /// <summary>
        /// 触发ArrowClick事件
        /// </summary>
        protected virtual void OnArrowClick(EventArgs e)
        {
            ArrowClick?.Invoke(this, e);
        }
        #endregion

        /// <summary>
        /// 在按钮下方弹出下拉选项菜单
        /// </summary>
        public void ShowDropDown()
        {
            if (_dropDownMenu == null || !_dropDownMenu.Enabled || !this.Enabled) return;

            //下拉菜单与按钮右边缘对齐, 显示于按钮下方
            _dropDownMenu.Show(this, new Point(this.Width - _dropDownMenu.Width, this.Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); //触发Paint事件(后续绘制会整体覆盖基类绘制的界面)
            base.OnPaintBackground(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle contentRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            Rectangle arrowRect = ArrowRectangle;

            //根据控件状态计算背景颜色及边框颜色
            Color baseColor, borderColor, separatorColor, arrowColor;
            getRenderColors(out baseColor, out borderColor, out separatorColor, out arrowColor);

            //计算需要加深显示的分区: 鼠标位于哪个操作区, 就只加深哪个操作区
            Rectangle highlightRect;
            Color highlightColor;
            getHighlightArea(arrowRect, out highlightRect, out highlightColor);

            //绘制圆角背景及边框
            using (GraphicsPath path = GraphicsPathHelper.CreatePath(contentRect, _radius, RoundStyle.All, 1))
            {
                if (!this.IsHighlight) //非突出显示: 仅以背景色绘制边框(与RoundButton保持一致)
                {
                    using (Pen borderPen = new Pen(baseColor))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(baseColor))
                    {
                        g.FillPath(brush, path);
                    }

                    using (Pen borderPen = new Pen(borderColor))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }

                //只对鼠标所在的分区加深(两种模式均生效, 裁剪至圆角范围内, 避免破坏圆角)
                if (highlightRect.Width > 0 && highlightRect.Height > 0)
                {
                    Region originalClip = g.Clip;
                    try
                    {
                        g.SetClip(path);
                        using (SolidBrush highlightBrush = new SolidBrush(highlightColor))
                        {
                            g.FillRectangle(highlightBrush, highlightRect);
                        }
                    }
                    finally
                    {
                        g.Clip = originalClip; //恢复原来的裁剪区域
                    }
                }
            }

            //绘制分隔竖线(上下两端各留出圆角边距)
            int separatorX = arrowRect.X;
            int separatorTop = _radius / 2 + 1;
            int separatorBottom = this.Height - separatorTop - 1;
            using (Pen separatorPen = new Pen(separatorColor))
            {
                g.DrawLine(separatorPen, separatorX, separatorTop, separatorX, separatorBottom);
            }

            //绘制下拉箭头(按下箭头区域时整体下移1px, 模拟按压效果)
            drawArrow(g, arrowRect, arrowColor, _controlState == ControlState.Pressed && _pressInArrow ? 1 : 0);

            //绘制主区域文本及图标(按下主按钮操作区时内容整体下移1px, 模拟按压效果)
            Rectangle mainRect = MainRectangle;
            if (_controlState == ControlState.Pressed && !_pressInArrow)
            {
                mainRect.Offset(0, 1);
            }

            if (this.Image != null)
            {
                drawImage(g, mainRect, this.Image);
            }
            Color foreColor = this.Enabled ? this.ForeColor : SystemColors.GrayText;
            TextRenderer.DrawText(g, this.Text, this.Font, mainRect, foreColor, GetTextFormatFlags());
        }

        /// <summary>
        /// 在主区域内绘制图标(有文本时显示于文本左侧, 无文本时居中显示)
        /// </summary>
        private void drawImage(Graphics g, Rectangle mainRect, Image image)
        {
            int imageHeight = Math.Min(mainRect.Height - 6, image.Height);
            int imageWidth = (int)(imageHeight * (float)image.Width / image.Height);
            int imageY = mainRect.Y + (mainRect.Height - imageHeight) / 2; //相对主区域垂直居中(含按下偏移)

            Rectangle imageRect;
            if (string.IsNullOrEmpty(this.Text)) //无文本时图标居中
            {
                imageRect = new Rectangle(
                    mainRect.X + (mainRect.Width - imageWidth) / 2,
                    imageY, imageWidth, imageHeight);
            }
            else //有文本时图标显示于文本左侧
            {
                using (Graphics lg = this.CreateGraphics())
                {
                    int textWidth = (int)lg.MeasureString(this.Text, this.Font, mainRect.Width - imageWidth - 2).Width;
                    imageRect = new Rectangle(
                        mainRect.X + Math.Max((mainRect.Width - textWidth - imageWidth - 2) / 2, 1),
                        imageY, imageWidth, imageHeight);
                }
            }

            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.DrawImage(image, imageRect);
        }

        /// <summary>
        /// 计算需要加深显示的分区及其颜色(鼠标位于哪个操作区, 就只加深哪个操作区)
        /// </summary>
        /// <param name="arrowRect">箭头区域</param>
        /// <param name="highlightRect">需要加深的分区, 为空时表示不加深</param>
        /// <param name="highlightColor">加深后的颜色</param>
        private void getHighlightArea(Rectangle arrowRect, out Rectangle highlightRect, out Color highlightColor)
        {
            highlightRect = Rectangle.Empty;
            highlightColor = _baseColor;

            if (!this.Enabled) return;

            //突出显示时加深颜色基于BaseColor, 非突出显示时基于当前BackColor
            Color sourceColor = this.IsHighlight ? _baseColor : this.BackColor;

            bool pressed = _controlState == ControlState.Pressed;
            bool pressInArrow = pressed && _pressInArrow;   //按下的分区
            bool pressInMain = pressed && !_pressInArrow;

            if (HoverArrow || pressInArrow) //悬停/按下在箭头区域: 只加深箭头区域
            {
                highlightRect = arrowRect;
                highlightColor = pressInArrow
                    ? PubFuns.GetColorPlus(sourceColor, 0, -35, -24, -9)
                    : PubFuns.GetColorPlus(sourceColor, 0, -35, -24, -30);
            }
            else if (HoverMain || pressInMain) //悬停/按下在主按钮操作区: 只加深主操作区
            {
                highlightRect = MainRectangle;
                highlightColor = pressInMain
                    ? PubFuns.GetColorPlus(sourceColor, 0, -35, -24, -9)
                    : PubFuns.GetColorPlus(sourceColor, 0, -35, -24, -30);
            }
        }

        /// <summary>
        /// 根据控件状态计算背景、边框、分隔线及箭头颜色
        /// </summary>
        private void getRenderColors(out Color baseColor, out Color borderColor, out Color separatorColor, out Color arrowColor)
        {
            if (!this.Enabled)
            {
                baseColor = SystemColors.Control;
                borderColor = SystemColors.ControlDark;
                separatorColor = SystemColors.ControlDark;
                arrowColor = SystemColors.GrayText;
                return;
            }

            //底色始终保持基色, 由getHighlightArea按分区叠加加深颜色
            baseColor = _baseColor;
            borderColor = _baseColor;

            //鼠标位于箭头区域时, 竖线突出显示
            bool highlightArrow = HoverArrow || (_controlState == ControlState.Pressed && _pressInArrow);
            separatorColor = highlightArrow
                ? SystemColors.ControlDarkDark
                : PubFuns.GetColorPlus(_baseColor, 0, -60, -60, -60);
            arrowColor = this.ForeColor;
        }

        /// <summary>
        /// 在指定区域中心绘制向下三角箭头
        /// </summary>
        private void drawArrow(Graphics g, Rectangle arrowRect, Color color, int offsetDown)
        {
            float centerX = arrowRect.X + arrowRect.Width / 2f;
            float centerY = arrowRect.Y + arrowRect.Height / 2f + offsetDown;
            float halfWidth = 5.2f;
            float halfHeight = 3.5f;

            PointF[] points = new PointF[]
            {
                new PointF(centerX - halfWidth, centerY - halfHeight),
                new PointF(centerX + halfWidth, centerY - halfHeight),
                new PointF(centerX, centerY + halfHeight)
            };

            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }
        }

        private TextFormatFlags GetTextFormatFlags()
        {
            TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;

            switch (this.TextAlign)
            {
                case ContentAlignment.BottomCenter:
                    flags |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.BottomLeft:
                    flags |= TextFormatFlags.Bottom | TextFormatFlags.Left;
                    break;
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Bottom | TextFormatFlags.Right;
                    break;
                case ContentAlignment.MiddleCenter:
                    flags |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.MiddleLeft:
                    flags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                    break;
                case ContentAlignment.MiddleRight:
                    flags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                    break;
                case ContentAlignment.TopCenter:
                    flags |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.TopLeft:
                    flags |= TextFormatFlags.Top | TextFormatFlags.Left;
                    break;
                case ContentAlignment.TopRight:
                    flags |= TextFormatFlags.Top | TextFormatFlags.Right;
                    break;
            }

            return flags;
        }

    }
}
