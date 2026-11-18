using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Zoncare.Common;

namespace Zoncare.Component.Controls
{
    [ToolboxItem(true)]
    [DesignerCategory("Code")]  // 重要：标记为代码控件，避免设计时复杂操作
    public class TextBoxEx : Control
    {
        [DllImport("user32", EntryPoint = "HideCaret")]
        private static extern bool HideCaret(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // 定义用于设置文本边距的常量
        private const uint EM_SETMARGINS = 0xD3;
        private const uint EC_LEFTMARGIN = 0x1;
        private const uint EC_RIGHTMARGIN = 0x2;

        private TextBox innerTextBox;
        private int cornerRadius = 2;
        private Color borderColor = SystemColors.InactiveCaption;
        private Color borderFocusColor = Color.DodgerBlue;
        private Color textBackgroundColor = Color.White;
        private int borderSize = 1;
        private bool isResizing = false;
        private bool _numericOnly = false; //是否仅支持输入数字
        private bool _isMouseOver = false;
        private int paddingLeft = 4, paddingRight = 4, paddingTop = 2, paddingBottom = 2;

        #region 属性定义
        [Category("自定义外观")]
        [Description("圆角半径（像素）")]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                if (cornerRadius != value)
                {
                    cornerRadius = Math.Max(1, value);
                    this.Invalidate();
                    if (!DesignMode) AdjustInnerTextBoxSize();
                }
            }
        }

        [Category("自定义外观")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                this.Invalidate();
            }
        }

        [Category("自定义外观")]
        [Description("获得焦点时的边框颜色"), Browsable(false)]
        public Color BorderFocusColor
        {
            get => borderFocusColor;
            set
            {
                borderFocusColor = value;
                this.Invalidate();
            }
        }

        [Category("自定义外观")]
        [Description("文本背景颜色")]
        public Color TextBackgroundColor
        {
            get => textBackgroundColor;
            set
            {
                textBackgroundColor = value;
                this.Invalidate();
            }
        }

        [Category("自定义外观")]
        [Description("边框宽度")]
        public int BorderSize
        {
            get => borderSize;
            set
            {
                borderSize = Math.Max(1, value);
                if (!DesignMode) AdjustInnerTextBoxSize();
                this.Invalidate();
            }
        }

        // 暴露文本框常用属性
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => innerTextBox?.Text ?? "";
            set
            {
                if (innerTextBox != null)
                    innerTextBox.Text = value ?? "";
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Color ForeColor
        {
            get => innerTextBox?.ForeColor ?? SystemColors.WindowText;
            set
            {
                if (innerTextBox != null)
                    innerTextBox.ForeColor = value;
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                if (innerTextBox != null)
                {
                    innerTextBox.Font = value;
                    if (!DesignMode)
                    {
                        this.Height = innerTextBox.Height + (borderSize + 2) * 2;
                        AdjustInnerTextBoxSize();
                    }
                }
            }
        }

        [Browsable(true)]
        public HorizontalAlignment TextAlign
        {
            get => innerTextBox?.TextAlign ?? HorizontalAlignment.Left;
            set
            {
                if (innerTextBox != null)
                    innerTextBox.TextAlign = value;
            }
        }

        [Browsable(false)]
        public BorderStyle BorderStyle
        {
            get => innerTextBox?.BorderStyle ?? BorderStyle.None;
            set
            {
                if (innerTextBox != null)
                    innerTextBox.BorderStyle = value;
            }
        }

        [Browsable(true)]
        public bool Multiline
        {
            get => innerTextBox?.Multiline ?? false;
            set
            {
                if (innerTextBox != null)
                    innerTextBox.Multiline = value;
            }
        }

        [Browsable(true)]
        public bool ReadOnly
        {
            get => innerTextBox?.ReadOnly ?? false;
            set
            {
                if (innerTextBox != null)
                    innerTextBox.ReadOnly = value;
            }
        }

        [Browsable(true)]
        public int SelectionStart
        {
            get => innerTextBox?.SelectionStart ?? 0;
            set
            {
                if (innerTextBox != null)
                    innerTextBox.SelectionStart = value;
            }
        }

        [Browsable(true)]
        public int MaxLength
        {
            get => innerTextBox?.MaxLength ?? 32767;
            set
            {
                if (innerTextBox != null)
                    innerTextBox.MaxLength = value;
            }
        }

        [Browsable(true)]
        public char PasswordChar
        {
            get => innerTextBox?.PasswordChar ?? '\0';
            set
            {
                if (innerTextBox != null)
                    innerTextBox.PasswordChar = value;
            }
        }

        [Description("是否仅支持输入数字"), Category("自定义属性"), DefaultValue(false)]
        public bool NumericOnly
        {
            get { return _numericOnly; }
            set
            {
                if (_numericOnly != value)
                {
                    _numericOnly = value;
                    // 如果启用数字限制，清除现有文本中的非数字字符
                    if (_numericOnly && !string.IsNullOrEmpty(this.Text))
                    {
                        string cleanedText = new string(this.Text.Where(c => char.IsDigit(c)).ToArray());
                        if (cleanedText != this.Text)
                        {
                            this.Text = cleanedText;
                        }
                    }
                }
            }
        }

        [Description("是否支持浮点型数值输入"), Category("自定义属性"), DefaultValue(false)]
        public bool FloatAllowed { get; set; }

        [Description("最小值"), Category("自定义属性"), DefaultValue(null)]
        public double? MinValue { get; set; }

        [Description("最大值"), Category("自定义属性"), DefaultValue(null)]
        public double? MaxValue { get; set; }

        [Description("圆角显示样式"), Category("自定义属性"), DefaultValue(RoundStyle.All)]
        public RoundStyle RoundStyle { get; set; } = RoundStyle.All;

        [Browsable(false)]
        public Point DropdownPosition { get; set; } = new Point(0, 21);
        [Browsable(false)]
        public Color AlphaBackColor { get; set; } = Color.Empty;
        [Browsable(false)]
        public string PlaceHolderStr { get; set; } = string.Empty;
        [Browsable(false)]
        public List<ListBoxItem> AutoCompleteDataSource = null;
        #endregion

        public TextBoxEx()
        {
            // 基础设置
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.SupportsTransparentBackColor, true);

            this.BackColor = Color.White;
            this.Size = new Size(150, 26);
            this.TabStop = false;

            setPaddings();

            // 延迟初始化内部TextBox，避免设计时问题
            InitializeInnerTextBox();

            // 使用HandleCreated事件，确保在控件完全创建后才进行复杂操作
            this.HandleCreated += (sender, e) =>
            {
                if (!DesignMode)
                {
                    AdjustInnerTextBoxSize();
                }
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!isResizing)
            {
                AdjustInnerTextBoxSize();
            }
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                if (e.Graphics == null) return;

                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // 设计时显示提示文字
                if (DesignMode && string.IsNullOrEmpty(innerTextBox?.Text))
                {
                    using (var brush = new SolidBrush(Color.LightGray))
                    {
                        g.DrawString("RoundedTextBox", this.Font, brush, 5, this.Height / 2 - 8);
                    }
                }

                // 确定边框颜色
                Color currentBorderColor = (borderColor == Color.Empty ? SystemColors.InactiveCaption : borderColor);
                if (this.Enabled)
                {
                    if (innerTextBox != null && innerTextBox.Focused)
                        currentBorderColor = borderFocusColor;
                    else if (_isMouseOver)
                        currentBorderColor = Color.SandyBrown;
                }
                else
                {
                    currentBorderColor = Color.LightGray;
                }

                // 绘制背景
                Rectangle rect = this.ClientRectangle;
                if (rect.Width <= 0 || rect.Height <= 0) return;

                using (SolidBrush backgroundBrush = new SolidBrush(textBackgroundColor))
                using (GraphicsPath path = GraphicsPathHelper.GetRoundedRectanglePath(rect, cornerRadius))
                {
                    if (RoundStyle == RoundStyle.None)
                        g.FillRectangle(backgroundBrush, rect);
                    else
                        g.FillPath(backgroundBrush, path);
                }

                // 绘制边框
                using (Pen borderPen = new Pen(currentBorderColor, borderSize))
                {
                    Rectangle borderRect = new Rectangle(0, 0, this.Width - borderSize, this.Height - borderSize);
                    if (borderRect.Width > 0 && borderRect.Height > 0)
                    {
                        if (RoundStyle == RoundStyle.None)
                        {
                            g.DrawRectangle(borderPen, borderRect);
                        }
                        else
                        {
                            using (GraphicsPath borderPath = GraphicsPathHelper.GetRoundedRectanglePath(borderRect, cornerRadius, borderSize > 1 ? borderSize / 2 : 0))
                            {
                                g.DrawPath(borderPen, borderPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 防止设计时异常导致IDE崩溃
                System.Diagnostics.Debug.WriteLine($"RoundedTextBox绘制错误: {ex.Message}");
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode)
            {
                AdjustInnerTextBoxSize();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (innerTextBox != null) innerTextBox.Font = new Font(this.Font.FontFamily, this.Font.Size);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            checkForScrollbars();
            base.OnTextChanged(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (innerTextBox == null) return;

            // 如果启用了数字限制，只允许输入数字、退格、删除和负号
            if (_numericOnly)
            {
                if (FloatAllowed)
                {
                    bool isSelectAll = (innerTextBox.SelectionLength == this.Text.Length);
                    if (e.KeyChar != '\b' && e.KeyChar != '/' && e.KeyChar != '.' && e.KeyChar != '-' && !char.IsDigit(e.KeyChar))
                    {
                        e.Handled = true;
                        return;
                    }
                    if (e.KeyChar == '.')
                    {
                        //小数点不能在第一位，且只能有一个
                        if (innerTextBox.SelectionStart == 0)
                        {
                            e.Handled = true;
                        }
                        else if (!isSelectAll)
                        {
                            int selEnd = innerTextBox.SelectionStart + innerTextBox.SelectionLength;
                            int dotPos = this.Text.IndexOf('.');
                            if (dotPos > 0)
                            {
                                if (dotPos >= selEnd || dotPos < innerTextBox.SelectionStart)
                                    e.Handled = true;
                            }
                        }
                    }

                    if (e.KeyChar == '-')
                    {
                        if (!isSelectAll)
                        {
                            //负号只能第一位，且只能有一个
                            if (innerTextBox.SelectionStart > 0) e.Handled = true;
                            if (this.Text.IndexOf('-') >= 0) e.Handled = true;
                        }
                    }
                }
                else
                {
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
                    {
                        e.Handled = true;
                        return;
                    }

                    // 检查负号位置：只能在开头输入，且只能有一个
                    if (e.KeyChar == '-')
                    {
                        if (innerTextBox.SelectionStart != 0 || this.Text.Contains('-'))
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }
            base.OnKeyPress(e);
        }

        protected override void OnValidating(CancelEventArgs e)
        {
            if (_numericOnly)
            {
                if (string.IsNullOrWhiteSpace(this.Text))
                {
                    //if (MinValue.HasValue) { this.Text = MinValue.ToString(); }
                }
                else
                {
                    double thisValue = double.Parse(this.Text);
                    if (MinValue.HasValue && thisValue < MinValue)
                    {
                        this.Text = MinValue.ToString();
                    }
                    if (MaxValue.HasValue && thisValue > MaxValue)
                    {
                        this.Text = MaxValue.ToString();
                    }
                }
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            // 如果焦点是从外部通过 Tab 进来的，让内部文本框获得焦点
            if (!innerTextBox.Focused)
            {
                innerTextBox.Focus();
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (!innerTextBox.Focused)
            {
                innerTextBox.Focus();
            }
        }

        //重写 Select 方法，让焦点直接进入内部文本框
        public new void Select()
        {
            // 先调用基类，然后激活内部控件
            base.Select();
            innerTextBox.Focus();
        }

        /// <summary>
        /// 当向编辑区域粘贴文本时发生
        /// </summary>
        public event OperationEventHandler OnPasteOccurred;

        /// <summary>
        /// 获取可让内部文本框正常显示的高度
        /// </summary>
        /// <returns></returns>
        public int GetPreferredHeight()
        {
            return innerTextBox.PreferredHeight + paddingTop * 2 + paddingBottom * 2 + 2;
        }

        private void InitializeInnerTextBox()
        {
            if (innerTextBox != null) return;

            innerTextBox = new TextBox();
            innerTextBox.AutoSize = false;
            innerTextBox.Margin = new Padding(1);
            innerTextBox.BorderStyle = BorderStyle.None;
            innerTextBox.BackColor = Color.White;

            // 只在运行时订阅事件
            if (!DesignMode)
            {
                innerTextBox.TextChanged += (sender, e) => this.OnTextChanged(e);
                innerTextBox.Enter += (sender, e) =>
                {
                    this.OnEnter(e);
                    this.Invalidate();
                };
                innerTextBox.Leave += (sender, e) =>
                {
                    this.OnLeave(e);
                    this.Invalidate();
                };
                innerTextBox.GotFocus += (sender, e) =>
                {
                    if (this.ReadOnly) HideCaret(this.Handle);
                    this.Invalidate();
                };
                innerTextBox.Validating += (sender, e) =>
                {
                    this.OnValidating(e);
                };
                innerTextBox.KeyDown += (sender, e) =>
                {
                    if (_numericOnly && e.KeyCode == Keys.Enter) OnValidating(new CancelEventArgs());
                };
                innerTextBox.KeyPress += (sender, e) =>
                {
                    this.OnKeyPress(e);
                };
                innerTextBox.KeyDown += (sender, e) =>
                {
                    this.OnKeyDown(e);
                };
                innerTextBox.KeyUp += (sender, e) =>
                {
                    this.OnKeyUp(e);
                };
                innerTextBox.HandleCreated += (sender, e) =>
                {
                    if (this.IsHandleCreated)
                    {
                        // 设置文本框的左右边距
                        int leftMargin = 5; // 左侧边距（像素）
                        int rightMargin = 2; // 右侧边距（像素）
                        SendMessage(this.Handle, EM_SETMARGINS, (IntPtr)(EC_LEFTMARGIN | EC_RIGHTMARGIN), (IntPtr)((rightMargin << 16) | leftMargin));
                    }

                    //仅限整数输入时,自动设定最大输入长度
                    if (NumericOnly && !FloatAllowed)
                    {
                        if (MaxValue.HasValue) this.MaxLength = MaxValue.Value.ToString().Length;
                        if (MinValue.HasValue)
                        {
                            int minValueLen = MinValue.Value.ToString().Length;
                            if (this.MaxLength < minValueLen) this.MaxLength = minValueLen;
                        }
                    }
                };
                innerTextBox.MouseEnter += (sender, e) =>
                {
                    _isMouseOver = true;
                    this.Invalidate();
                };
                innerTextBox.MouseLeave += (sender, e) =>
                {
                    _isMouseOver = false;
                    this.Invalidate();
                };
            }

            this.Controls.Add(innerTextBox);
        }

        private void AdjustInnerTextBoxSize()
        {
            if (innerTextBox == null || innerTextBox.IsDisposed) return;

            try
            {
                isResizing = true;

                int newWidth = Math.Max(20, this.Width - paddingLeft - paddingRight);
                int newHeight = Math.Max(20, this.Height - paddingTop - paddingBottom);

                innerTextBox.Location = new Point(paddingLeft, paddingTop);
                innerTextBox.Width = newWidth;
                innerTextBox.Height = newHeight;
            }
            finally
            {
                isResizing = false;
            }
        }

        private void setPaddings()
        {
            paddingLeft = borderSize + UIFuns.GetNaturalFloor(4 * CommonValues.WidthScale);
            paddingRight = borderSize + UIFuns.GetNaturalFloor(4 * CommonValues.WidthScale);
            paddingTop = borderSize + UIFuns.GetNaturalFloor(2 * CommonValues.HeightScale);
            paddingBottom = borderSize + UIFuns.GetNaturalFloor(2 * CommonValues.HeightScale);
        }

        private void checkForScrollbars()
        {
            if (innerTextBox == null || !innerTextBox.IsHandleCreated) return;
            if (string.IsNullOrEmpty(this.Text)) return;

            bool scroll = false;
            int cnt = 0;
            for (int i = 0; i < this.Text.ToArray().Length; i++)
            {
                if (innerTextBox.GetFirstCharIndexFromLine(i) < 0)
                {
                    cnt = i;
                    break;
                }
            }

            if (cnt > 1)
            {
                int pos0 = innerTextBox.GetPositionFromCharIndex(innerTextBox.GetFirstCharIndexFromLine(0)).Y;
                if (pos0 >= 32768) pos0 -= 65536;
                int pos1 = innerTextBox.GetPositionFromCharIndex(innerTextBox.GetFirstCharIndexFromLine(1)).Y;
                if (pos1 >= 32768) pos1 -= 65536;
                int h = pos1 - pos0;
                scroll = cnt * h > (this.ClientSize.Height - 6);  // 6 = padding
            }

            //Size textBoxRect = TextRenderer.MeasureText(this.Text, this.Font, new Size(this.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            try
            {
                //this.ScrollBars = textBoxRect.Height > this.Height ? ScrollBars.Vertical : ScrollBars.None;

                if (scroll)
                {
                    if (innerTextBox.ScrollBars != ScrollBars.Vertical) innerTextBox.ScrollBars = ScrollBars.Vertical;
                }
                else
                {
                    if (innerTextBox.ScrollBars != ScrollBars.None) innerTextBox.ScrollBars = ScrollBars.None;
                }
            }
            catch (Win32Exception)
            {
                // this sometimes throws a "failure to create window handle" error.
                // This might happen if the TextBox is unvisible and/or
                // too small to display a toolbar.
            }
        }


        // 清理资源
        protected override void Dispose(bool disposing)
        {
            if (disposing && innerTextBox != null && !innerTextBox.IsDisposed)
            {
                innerTextBox.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
