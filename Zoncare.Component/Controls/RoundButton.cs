using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    public partial class RoundButton : Button
    {
        private Color _baseColor = Color.FromArgb(84, 132, 207); //基颜色
        private ControlState _controlState; //控件状态
        private int _imageWidth = 18, _imageHeight = 18;
        private RoundStyle _roundStyle = RoundStyle.All; //圆角
        private int _radius = 4; //圆角半径*2(直径)
        private bool _isHightlight;
        private static readonly ContentAlignment _rightAlignment = ContentAlignment.TopRight | ContentAlignment.BottomRight | ContentAlignment.MiddleRight;
        private static readonly ContentAlignment _leftAlignment = ContentAlignment.TopLeft | ContentAlignment.BottomLeft | ContentAlignment.MiddleLeft;

        #region --构造函数--
        public RoundButton(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            this.Cursor = Cursors.Hand;
        }

        public RoundButton() : base()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.UserPaint | //控件自行绘制，而不使用操作系统的绘制
                          ControlStyles.AllPaintingInWmPaint | //忽略擦出的消息，减少闪烁。
                          ControlStyles.OptimizedDoubleBuffer |//在缓冲区上绘制，不直接绘制到屏幕上，减少闪烁。
                          ControlStyles.ResizeRedraw | //控件大小发生变化时，重绘。
                          ControlStyles.SupportsTransparentBackColor, true);//支持透明背景颜色
        }
        #endregion

        #region 属性
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

        [Description("图像区域的宽度"), DefaultValue(18)] //默认值为18px，最小12px
        public int ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                if (value != _imageWidth)
                {
                    _imageWidth = value < 12 ? 12 : value;
                    base.Invalidate();
                }
            }
        }

        [Description("图像区域的宽度"), DefaultValue(18)]
        public int ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                if (value != _imageHeight)
                {
                    _imageHeight = value < 12 ? 12 : value;
                    base.Invalidate();
                }
            }
        }

        [Description("指定按钮的圆角特性, 默认每个角均为圆角")]
        public RoundStyle RoundStyle
        {
            get { return _roundStyle; }
            set
            {
                if (_roundStyle != value)
                {
                    _roundStyle = value;
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
                if (FollowBaseColor)
                {
                    if (_isHightlight)
                        this.ForeColor = SystemColors.ButtonHighlight;
                    else
                        this.ForeColor = BaseColor;
                }
            }
        }

        [Browsable(true), Description("字体颜色是否跟随主基调颜色变化"), DefaultValue(true)]
        public bool FollowBaseColor { get; set; }

        /// <summary>
        /// 自定义的与按钮数据状态关联的对象
        /// </summary>
        [Browsable(false)]
        public object DataFlag { get; set; }

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

        #endregion

        #region 鼠标对应的几个事件
        protected override void OnMouseEnter(EventArgs e)//鼠标进入时
        {
            base.OnMouseEnter(e);
            ControlState = ControlState.Hover;//正常
        }
        protected override void OnMouseLeave(EventArgs e)//鼠标离开
        {
            base.OnMouseLeave(e);
            ControlState = ControlState.Normal;//正常
        }
        protected override void OnMouseDown(MouseEventArgs e)//鼠标按下
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Clicks == 1)//鼠标左键且点击次数为1
            {
                ControlState = ControlState.Pressed;//按下的状态
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)//鼠标弹起
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                if (ClientRectangle.Contains(e.Location))//控件区域包含鼠标的位置
                {
                    ControlState = ControlState.Hover;
                }
                else
                {
                    ControlState = ControlState.Normal;
                }
            }
        }
        #endregion

        /// <summary>
        /// 设置按钮上的显示文本
        /// </summary>
        /// <param name="text">与按钮关联的文本</param>
        /// <param name="flag">与按钮状态关联的标识对象</param>
        /// <param name="useHighlightStyle">是否突出显示按钮</param>
        public void SetText(string text, object flag = null, bool useHighlightStyle = false)
        {
            this.Text = text;
            this.DataFlag = flag;
            this.IsHighlight = useHighlightStyle;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            base.OnPaintBackground(e);

            Graphics g = e.Graphics;
            Rectangle imageRect;    //图像区域
            Rectangle textRect;     //文字区域

            this.CalculateRect(out imageRect, out textRect);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color baseColor;
            Color borderColor;
            Color innerBorderColor = _baseColor; //Color.FromArgb(200, 255, 255, 255); ;

            if (Enabled)
            {
                switch (ControlState)
                {
                    case ControlState.Hover:
                        baseColor = GetColor(_baseColor, 0, -35, -24, -30);
                        borderColor = _baseColor;
                        break;
                    case ControlState.Pressed:
                        baseColor = GetColor(_baseColor, 0, -35, -24, -9);
                        borderColor = Color.Lime;
                        break;
                    default:
                        baseColor = _baseColor;
                        borderColor = _baseColor;
                        break;
                }
            }
            else
            {
                baseColor = SystemColors.ControlDark;
                borderColor = SystemColors.ControlDark;
            }

            float position = 0.35f;
            position = 0.0f;

            renderInnerBackground(g, ClientRectangle, baseColor, borderColor, RoundStyle, _radius, position, false, true, LinearGradientMode.Vertical);

            if (Image != null)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                //g.DrawImage(Image, imageRect, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel);
                g.DrawImage(Image, imageRect);
            }
            else
            {
                //textRect.X = imageRect.X + 4;
                //textRect.Width += imageRect.Width;
                textRect = this.ClientRectangle;
            }

            Color foreColor = this.ForeColor;
            //if (this.IsHighlight)
            //{
            //    foreColor = SystemColors.ButtonHighlight;
            //}
            //else
            //{
            //    foreColor = baseColor;
            //}
            TextRenderer.DrawText(g, Text, Font, textRect, foreColor, GetTextFormatFlags(TextAlign, RightToLeft == RightToLeft.Yes));
        }

        /// <summary>
        /// 计算图像及文字显示区域的大小和位置
        /// </summary>
        /// <param name="imageRect">图像区域</param>
        /// <param name="textRect">文字区域</param>
        private void CalculateRect(out Rectangle imageRect, out Rectangle textRect)
        {
            imageRect = new Rectangle(0, 0, _imageWidth, _imageHeight);
            textRect = Rectangle.Empty;
            ContentAlignment CheckAlign = ContentAlignment.MiddleLeft; //指定以居中左对齐方式绘图

            bool bCheckAlignLeft = (int)(_leftAlignment & CheckAlign) != 0;
            bool bCheckAlignRight = (int)(_rightAlignment & CheckAlign) != 0;
            bool bRightToLeft = (RightToLeft == RightToLeft.Yes);

            if ((bCheckAlignLeft && !bRightToLeft) || (bCheckAlignRight && bRightToLeft))
            {
                switch (CheckAlign)
                {
                    case ContentAlignment.TopRight:
                    case ContentAlignment.TopLeft:
                        imageRect.Y = 2;
                        break;
                    case ContentAlignment.MiddleRight:
                    case ContentAlignment.MiddleLeft:
                        imageRect.Y = (Height - _imageHeight) / 2;
                        break;
                    case ContentAlignment.BottomRight:
                    case ContentAlignment.BottomLeft:
                        imageRect.Y = Height - _imageHeight - 2;
                        break;
                }

                imageRect.X = 1;
                if (this.Image != null) //custom use(JinJ)
                {
                    if (!string.IsNullOrEmpty(this.Text))
                    {
                        int h = (int)(_imageHeight * (Font.Size / 10.5f)); //根据当前字体大小重置图标大小
                        int w = (int)(h * (double)_imageWidth / _imageHeight);
                        int x = imageRect.X;
                        using (Graphics g = this.CreateGraphics())
                        {
                            int textWidth = (int)g.MeasureString(Text, Font, Width - w - 1).Width; //图标和文字 间隔1px
                            if (textWidth == 0)
                            {
                                x = (int)((Width - w) / 2f);
                            }
                            else
                            {
                                x = (int)((Width - textWidth - w - 1) / 2f);
                            }
                        }
                        imageRect = new Rectangle(x, (Height - h) / 2, w, h);
                    }
                    else
                    {
                        imageRect.X = (Width - _imageWidth) / 2;
                    }

                    this.TextAlign = ContentAlignment.MiddleLeft;
                }

                textRect = new Rectangle(imageRect.Right + 1, 0, Width - imageRect.Right - 1, Height);
            }
            else if ((bCheckAlignRight && !bRightToLeft) || (bCheckAlignLeft && bRightToLeft))
            {
                switch (CheckAlign)
                {
                    case ContentAlignment.TopLeft:
                    case ContentAlignment.TopRight:
                        imageRect.Y = 2;
                        break;
                    case ContentAlignment.MiddleLeft:
                    case ContentAlignment.MiddleRight:
                        imageRect.Y = (Height - _imageHeight) / 2;
                        break;
                    case ContentAlignment.BottomLeft:
                    case ContentAlignment.BottomRight:
                        imageRect.Y = Height - _imageHeight - 2;
                        break;
                }

                imageRect.X = Width - _imageWidth - 1;

                textRect = new Rectangle(2, 0, Width - _imageWidth - 6, Height);
            }
            else
            {
                switch (CheckAlign)
                {
                    case ContentAlignment.TopCenter:
                        imageRect.Y = 2;
                        textRect.Y = imageRect.Bottom + 2;
                        textRect.Height = Height - _imageHeight - 6;
                        break;
                    case ContentAlignment.MiddleCenter:
                        imageRect.Y = (Height - _imageHeight) / 2;
                        textRect.Y = 0;
                        textRect.Height = Height;
                        break;
                    case ContentAlignment.BottomCenter:
                        imageRect.Y = Height - _imageHeight - 2;
                        textRect.Y = 0;
                        textRect.Height = Height - _imageHeight - 6;
                        break;
                }

                imageRect.X = (Width - _imageWidth) / 2;

                textRect.X = 2;
                textRect.Width = Width - 4;
            }

        }

        private void renderInnerBackground(Graphics g, Rectangle rect, Color baseColor, Color borderColor, RoundStyle style,
            int roundWidth, float basePosition, bool drawBorder, bool drawGlass, LinearGradientMode mode)
        {
            if (drawBorder) //是否画边框
            {
                rect.Width--;
                rect.Height--;
            }

            using (SolidBrush brush = new SolidBrush(baseColor))
            {
                if (style != RoundStyle.None)
                {
                    using (GraphicsPath path = GraphicsPathHelper.CreatePath(rect, roundWidth, style, 1))
                    {
                        if (this.IsHighlight == false)
                        {
                            //以背景色绘制边框
                            using (Pen borderPen = new Pen(baseColor))
                            {
                                g.DrawPath(borderPen, path);
                            }
                        }
                        else
                        {
                            if (this.ControlState == ControlState.Pressed)
                            {
                                using (Pen borderPen = new Pen(borderColor, 2))
                                {
                                    g.DrawPath(borderPen, path); //绘制边框

                                    rect.Inflate(-2, -2);
                                    using (GraphicsPath innerPath = GraphicsPathHelper.CreatePath(rect, roundWidth, style, 1))
                                    {
                                        g.FillPath(brush, innerPath); //填充内部区域颜色
                                    }
                                }
                            }
                            else
                            {
                                //按钮内部区域填充颜色
                                g.FillPath(brush, path);
                            }
                        }
                    }
                }
                else
                {
                    if (this.ControlState == ControlState.Hover) //绘制鼠标悬浮时的背景
                    {
                        using (GraphicsPath path = GraphicsPathHelper.CreatePath(rect, roundWidth, style, 1))
                        {
                            g.FillPath(Brushes.Lavender, path);
                        }
                    }
                }
            }
        }

        private TextFormatFlags GetTextFormatFlags(ContentAlignment alignment, bool rightToleft)
        {
            TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.SingleLine;

            if (rightToleft)
            {
                flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
            }

            switch (alignment)
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
                    flags |= TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter;
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

        private Color GetColor(Color colorBase, int a, int r, int g, int b)
        {
            int a0 = colorBase.A;
            int r0 = colorBase.R;
            int g0 = colorBase.G;
            int b0 = colorBase.B;
            if (a + a0 > 255) { a = 255; } else { a = Math.Max(a + a0, 0); }
            if (r + r0 > 255) { r = 255; } else { r = Math.Max(r + r0, 0); }
            if (g + g0 > 255) { g = 255; } else { g = Math.Max(g + g0, 0); }
            if (b + b0 > 255) { b = 255; } else { b = Math.Max(b + b0, 0); }

            return Color.FromArgb(a, r, g, b);
        }
    }

    public enum ControlState
    {
        Normal,
        Hover,
        Pressed
    }
}