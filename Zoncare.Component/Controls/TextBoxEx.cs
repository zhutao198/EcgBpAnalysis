using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    /// <summary>
    /// 提供可自定义输入提示的数据源、具有内容描述占位符的文本框
    /// </summary>
    public partial class TextBoxEx : TextBox, IMessageFilter
    {
        private const int
            WM_LBUTTONDOWN = 0x201,
            WM_RBUTTONDOWN = 0x0204,
            WM_MBUTTONDOWN = 0x0207,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_ACTIVATEAPP = 0x001C;

        private List<ListBoxItem> m_AutoCompleteSource;
        private HsToolStripDropDown m_DropdownContainer;
        private Color m_AlphaBackColor;
        private ListBox _ListBox;
        private ListBoxItem _SelectedItem;
        private RoundStyle _roundStyle = RoundStyle.All; //圆角
        private int _radius = 4; //圆角半径*2(直径)
        private bool _numericOnly = false; //是否仅支持输入数字

        [DllImport("user32", EntryPoint = "HideCaret")]
        private static extern bool HideCaret(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // 定义用于设置文本边距的常量
        private const uint EM_SETMARGINS = 0xD3;
        private const uint EC_LEFTMARGIN = 0x1;
        private const uint EC_RIGHTMARGIN = 0x2;
        private const uint EC_USEFONTINFO = 0xFFFF;

        [Description("指定文本框的圆角特性, 默认每个角均为圆角")]
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
        //public Color BackgroundColor { get; set; }

        [Description("是否启用自定义输入提示"), Category("自定义属性"), DefaultValue(false)]
        public bool EnableCustomAutoComplete { get; set; }

        [Description("圆角文本框外部范围颜色"), Category("自定义属性")]
        public Color BorderColor { get; set; }

        [Description("当文本框获取焦点后, 若当前文本为空是否显示输入提示"), Category("自定义属性"), DefaultValue(false)]
        public bool AutoCompleteShowOnFocus { get; set; }

        [Description("自定义输入提示的数据源"), Category("自定义属性")]
        public List<ListBoxItem> AutoCompleteDataSource
        {
            get
            {
                return m_AutoCompleteSource;
            }
            set
            {
                m_AutoCompleteSource = value;
                //if (_ListBox != null) _ListBox.DataSource = value;
            }
        }

        private string _placeHolderStr;
        [Description("文本框内容描述占位符"), Category("自定义属性")]
        public string PlaceHolderStr
        {
            get => _placeHolderStr;
            set
            {
                _placeHolderStr = value;
                this.Invalidate();
            }
        }

        [Description("自定义的半透明效果的背景色(模拟)"), Category("自定义属性")]
        public Color AlphaBackColor
        {
            get { return m_AlphaBackColor; }
            set
            {
                m_AlphaBackColor = value;
                if (m_AlphaBackColor != Color.Empty)
                {
                    Color fixedColor = Color.Empty;
                    using (Bitmap bmp = new Bitmap(1, 1))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.FromArgb(m_AlphaBackColor.R, m_AlphaBackColor.G, m_AlphaBackColor.B));
                            using (SolidBrush br = new SolidBrush(m_AlphaBackColor))
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, 1, 1));
                            }
                        }
                        fixedColor = bmp.GetPixel(0, 0);
                    }
                    this.BackColor = fixedColor;
                }
            }
        }

        /// <summary>
        /// 下拉列表左上角所在位置
        /// </summary>
        [Browsable(false)]
        public Point DropdownPosition { get; set; }

        /// <summary>
        /// 下拉列表当前是否已展开
        /// </summary>
        [Browsable(false)]
        public bool DropdownIsShown { get { return (m_DropdownContainer != null && m_DropdownContainer.Visible); } }

        /// <summary>
        /// 引发下拉提示列表关闭的控件句柄
        /// </summary>
        [Browsable(false)]
        public IntPtr HiddenInvoker { get; private set; }

        /// <summary>
        /// 当前选中的输入提示项
        /// </summary>
        [Browsable(false)]
        public ListBoxItem SelectedItem
        {
            get { return _SelectedItem; }
            private set
            {
                ListBoxItem oldItem = _SelectedItem;
                _SelectedItem = value;
                if (_SelectedItem != null)
                {
                    ItemSelectedEventArgs arg = new ItemSelectedEventArgs(_SelectedItem, _ListBox.SelectedIndex);
                    if (!object.Equals(_SelectedItem.ItemValue, oldItem?.ItemValue)) OnItemSelected?.Invoke(this, arg);
                }
            }
        }

        /// <summary>
        /// 当前输入文本匹配到自动填充项时发生
        /// </summary>
        public event ItemSelectedEventHandler OnItemSelected;

        /// <summary>
        /// 下拉列表显示时发生
        /// </summary>
        public event EventHandler OnDropdownShown;

        /// <summary>
        /// 当向编辑区域粘贴文本时发生
        /// </summary>
        public event OperationEventHandler OnPasteOccurred;

        public TextBoxEx()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            this.DropdownPosition = new Point(0, this.Height);

            //Application.AddMessageFilter(this);
            //this.Disposed += new EventHandler((o, e) => { Application.RemoveMessageFilter(this); });
        }

        /// <summary>
        /// 显示所有可选作填充文本框内容的列表项
        /// </summary>
        public void ShowAllSourceItems(bool doShow = true)
        {
            initAutoCompleteListPanel();
            _ListBox.Font = this.Font;

            if (doShow)
            {
                OnDropdownShown?.Invoke(this, new EventArgs());
                resetListSource(false);
                if (_ListBox.Items.Count > 0)
                {
                    showDropdown();
                }
            }
            else
            {
                hideDropdown(IntPtr.Zero);
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            //checkForScrollbars();
            base.OnClientSizeChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            checkForScrollbars();
            base.OnTextChanged(e);

            if (_ListBox != null)
            {
                if (this.EnableCustomAutoComplete == true && _ListBox.SelectedIndex < 0)
                {
                    _ListBox.Items.Clear();
                    resetListSource();

                    if (_ListBox.Items.Count > 0)
                    {
                        showDropdown();
                    }
                    else
                    {
                        hideDropdown(IntPtr.Zero);
                    }
                }
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            if (this.ReadOnly) HideCaret(this.Handle);

            if (this.EnableCustomAutoComplete)
            {
                Application.AddMessageFilter(this);
                initAutoCompleteListPanel();

                if (this.Text.Trim().Length == 0 && this.AutoCompleteShowOnFocus == true)
                {
                    //if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
                    //{
                    //    if (m_AutoCompleteSource != null && m_AutoCompleteSource.Count > 0)
                    //    {
                    //        setListBoxItems(m_AutoCompleteSource);
                    //        m_DropdownContainer.Show(this, 0, this.Height);
                    //    }
                    //}
                }
            }

            if (_roundStyle == RoundStyle.All) this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Application.RemoveMessageFilter(this);
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
                    if (MinValue.HasValue && thisValue < MinValue) this.Text = MinValue.ToString();
                    if (MaxValue.HasValue && thisValue > MaxValue) this.Text = MaxValue.ToString();
                }
            }
            base.OnValidating(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!EnableCustomAutoComplete)
            {
                base.OnKeyDown(e);
                if (_numericOnly && e.KeyCode == Keys.Enter) this.OnValidating(new CancelEventArgs());
            }

            if (m_DropdownContainer != null && m_DropdownContainer.Visible)
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (_ListBox.SelectedIndex > 0)
                        _ListBox.SelectedIndex--;
                    else
                        _ListBox.SelectedIndex = _ListBox.Items.Count - 1;

                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (_ListBox.SelectedIndex < _ListBox.Items.Count - 1)
                        _ListBox.SelectedIndex++;
                    else
                        _ListBox.SelectedIndex = 0;

                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    hideDropdown(IntPtr.Zero);
                }
                else
                {
                    _ListBox.SelectedIndex = -1;
                }
                //m_ListBox.Focus();
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // 如果启用了数字限制，只允许输入数字、退格、删除和负号
            if (_numericOnly)
            {
                if (FloatAllowed)
                {
                    bool isSelectAll = (this.SelectionLength == this.Text.Length);
                    if (e.KeyChar != '\b' && e.KeyChar != '/' && e.KeyChar != '.' && e.KeyChar != '-' && !char.IsDigit(e.KeyChar))
                    {
                        e.Handled = true;
                        return;
                    }
                    if (e.KeyChar == '.')
                    {
                        //小数点不能在第一位，且只能有一个
                        if (this.SelectionStart == 0)
                        {
                            e.Handled = true;
                        }
                        else if (!isSelectAll)
                        {
                            int selEnd = this.SelectionStart + this.SelectionLength;
                            int dotPos = this.Text.IndexOf('.');
                            if (dotPos > 0)
                            {
                                if (dotPos >= selEnd || dotPos < this.SelectionStart)
                                    e.Handled = true;
                            }
                        }
                    }

                    if (e.KeyChar == '-')
                    {
                        if (!isSelectAll)
                        {
                            //负号只能第一位，且只能有一个
                            if (this.SelectionStart > 0) e.Handled = true;
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
                        if (this.SelectionStart != 0 || this.Text.Contains('-'))
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }

            base.OnKeyPress(e);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            initAutoCompleteListPanel();

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
        }

        // 重写OnHandleCreated事件，确保在控件句柄创建后设置边距
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_roundStyle == RoundStyle.All) SetTextMargins();
        }

        private void SetTextMargins()
        {
            // 确保控件句柄有效
            if (this.IsHandleCreated)
            {
                // 设置文本框的左右边距
                int leftMargin = 5; // 左侧边距（像素）
                int rightMargin = 2; // 右侧边距（像素）
                SendMessage(this.Handle, EM_SETMARGINS, (IntPtr)(EC_LEFTMARGIN | EC_RIGHTMARGIN), (IntPtr)((rightMargin << 16) | leftMargin));

                //// 设置输入时文本显示区域（调整顶部和底部边距）
                //Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
                //rect.Inflate(-1, -1);
                //rect.Y += (this.Height - this.Font.Height) / 2 +3; // 调整这个值
                //rect.Height = this.Font.Height + 2;

                //IntPtr lparam = Marshal.AllocHGlobal(Marshal.SizeOf(rect));
                //Marshal.StructureToPtr(rect, lparam, false);
                //SendMessage(this.Handle, 0x00B3, IntPtr.Zero, lparam);
                //Marshal.FreeHGlobal(lparam);
            }
        }

        // 重写OnSizeChanged事件，仅在尺寸变化时设置窗口区域
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_roundStyle != RoundStyle.None)
            {
                setWindowRegion();
            }
        }

        private void setWindowRegion()
        {
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            if (BorderStyle != BorderStyle.None)
                rect.Inflate(-1, -1);

            using (GraphicsPath path = GraphicsPathHelper.CreatePath(rect, _radius, _roundStyle, 0))
            {
                this.Region = new Region(path);
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0302: //WM_PASTE
                    CancelEventArgs arg = new CancelEventArgs(false);
                    OnPasteOccurred?.Invoke(this, arg);
                    if (arg.Cancel == false)
                    {
                        // 如果启用了数字限制，处理粘贴内容
                        if (_numericOnly)
                        {
                            try
                            {
                                string clipboardText = Clipboard.GetText();
                                if (!string.IsNullOrEmpty(clipboardText))
                                {
                                    // 过滤粘贴内容，只保留数字和负号
                                    string filteredText = new string(clipboardText.Where(c => char.IsDigit(c) || c == '-').ToArray());

                                    // 进一步处理负号：只能有一个，且必须在开头
                                    if (filteredText.Contains('-'))
                                    {
                                        // 如果文本中已经有负号，或者当前光标不在开头，则移除负号
                                        if (this.Text.Contains('-') || this.SelectionStart > 0)
                                        {
                                            filteredText = filteredText.Replace("-", "");
                                        }
                                        else
                                        {
                                            // 确保负号在开头
                                            filteredText = "-" + filteredText.Replace("-", "");
                                        }
                                    }

                                    // 插入过滤后的文本
                                    this.SelectedText = filteredText;
                                }
                            }
                            catch
                            {
                                // 如果剪贴板操作失败，回退到默认行为
                                base.WndProc(ref m);
                            }
                        }
                        else
                        {
                            base.WndProc(ref m);
                        }
                    }
                    break;
                case 0x85:  //WM_NCPAINT
                    base.WndProc(ref m);
                    // 叠加自定义边框
                    //Rectangle borderRect = Rectangle.Inflate(this.ClientRectangle, -2, -2);
                    //using (Graphics g = Graphics.FromHwnd(this.Handle))
                    //{
                    //    using (GraphicsPath borderPath = GraphicsPathHelper.CreatePath(borderRect, _radius, _roundStyle, 1))
                    //    {
                    //        using (Pen borderPen = new Pen(Color.LightGray, 1))
                    //        {
                    //            g.DrawPath(borderPen, borderPath);
                    //        }
                    //    }
                    //}
                    // 拦截非客户区绘制消息，阻止Windows绘制默认边框
                    m.Result = IntPtr.Zero;
                    return;
                case 0xF:   //WM_PAINT
                case 0x133: //WM_CTLCOLOREDIT
                    base.WndProc(ref m);
                    WmPaint(ref m);
                    break;
                case 0x0030: //WM_SETFONT
                    base.WndProc(ref m);
                    // 当字体改变时重新设置边距
                    SetTextMargins();
                    break;
                //case 0x0014: //WM_ERASEBKGND
                //    if (this.Text == "")
                //        m.Msg = (int)IntPtr.Zero;
                //    else
                //        base.WndProc(ref m);
                //    break;
                case 0x0201: // WM_LBUTTONDOWN
                    _isMouseOver = true;
                    this.Invalidate(); // 强制重绘以更新边框
                    base.WndProc(ref m); // 调用基类处理（如设置焦点）
                    return;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }


        //构造文本输入自动完成提示的下拉列表
        private void initAutoCompleteListPanel()
        {
            if (!this.EnableCustomAutoComplete) return;

            if (_ListBox == null)
            {
                _ListBox = new ListBox()
                {
                    Font = this.Font,
                    Width = this.Width,
                    ValueMember = "ItemValue",
                    DisplayMember = "ItemText",
                    DrawMode = DrawMode.OwnerDrawVariable
                };
                Control autoCompleteCtl = getSpecifiedParent(this, typeof(AutoCompleteTextBox));
                if (autoCompleteCtl != null)
                {
                    _ListBox.Width = autoCompleteCtl.Width;
                }

                _ListBox.SelectedValueChanged += ListBox_SelectedValueChanged;
                _ListBox.DrawItem += ListBox_DrawItem;
                _ListBox.MouseMove += ListBox_MouseMove;
                _ListBox.MeasureItem += ListBox_MeasureItem;
                setListBoxItems(m_AutoCompleteSource);
            }

            if (m_DropdownContainer == null)
            {
                //Panel panel = new Panel()
                //{
                //    Size = new Size(this.Width, 100),
                //    Padding = new Padding(1, 1, 1, 13),
                //    BackColor = Color.Gainsboro
                //};
                //m_ListBox.Parent = panel;
                ToolStripControlHost toolStripControlHost = new ToolStripControlHost(_ListBox)
                {
                    Margin = Padding.Empty,
                    Padding = Padding.Empty,
                    AutoSize = false
                };
                m_DropdownContainer = new HsToolStripDropDown();
                m_DropdownContainer.Padding = Padding.Empty;
                m_DropdownContainer.Items.Add(toolStripControlHost);
                m_DropdownContainer.VisibleChanged += new EventHandler((o, arg) => { if (m_DropdownContainer.Visible == false) _ListBox.SelectedIndex = -1; });
                m_DropdownContainer.ActiveChanged += new EventHandler((o, arg) => { hideDropdown(IntPtr.Zero); });

                #region 使用Label实现右下角拖动按钮
                //Label label = new Label();
                //label.Text = "◢";
                //label.Font = new Font("宋体", 9);
                //label.Parent = m_DropdownContainer;

                //label.AutoSize = true;
                //label.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
                //label.Location = new Point(this.panel.Location.X + this.panel.Size.Width - label.Width + 3,
                //    this.panel.Location.Y + this.panel.Size.Height - label.Height - 1);
                ////实现缩放功能
                //label.MouseDown += delegate (object sender, MouseEventArgs e1)
                //{
                //    this.pPoint = Cursor.Position;
                //    this.isMouseDown = true;
                //};
                //label.MouseLeave += delegate (object sender, EventArgs e1)
                //{
                //    this.isMouseDown = false;
                //};
                //label.MouseMove += delegate (object sender, MouseEventArgs e1)
                //{
                //    this.cPoint = Cursor.Position;
                //    if (e1.Button == MouseButtons.Left && isMouseDown)
                //    {
                //        this.panel.Height = Math.Max(this.panel.Height + cPoint.Y - pPoint.Y, 23);
                //        this.panel.Width = Math.Max(this.panel.Width + cPoint.X - pPoint.X, this.Width);
                //        pPoint = Cursor.Position;
                //    }
                //    else
                //    {
                //        label.Cursor = Cursors.SizeNWSE;
                //    }
                //};
                #endregion
            }
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            int itemIndex = _ListBox.IndexFromPoint(e.Location);
            if (itemIndex >= 0)
            {
                _ListBox.Tag = "1";
                _ListBox.SelectedIndex = itemIndex;
                _ListBox.Tag = "0";
            }
        }

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color backColor = e.BackColor;
            if (e.Index < 0) return;
            if (e.State == DrawItemState.Selected)
                backColor = SystemColors.Highlight;
            else if (e.State == DrawItemState.HotLight)
                backColor = SystemColors.HotTrack;

            using (StringFormat textFormat = new StringFormat(StringFormatFlags.NoWrap) { LineAlignment = StringAlignment.Center })
            {
                Font itemFont = new Font(e.Font.FontFamily, e.Font.Size - 1); //减小字体大小以显示各项的间隔
                e.Graphics.FillRectangle(new SolidBrush(backColor), e.Bounds);
                e.Graphics.DrawString(_ListBox.Items[e.Index].ToString(), itemFont, new SolidBrush(e.ForeColor), e.Bounds, textFormat);
                e.DrawFocusRectangle();
            }
        }

        private void ListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            Font itemFont = new Font(_ListBox.Font.FontFamily, _ListBox.Font.Size - 1);
            SizeF textSize = e.Graphics.MeasureString(_ListBox.Items[e.Index].ToString(), itemFont, _ListBox.Width, StringFormat.GenericTypographic);
            e.ItemHeight = (int)(textSize.Height) + 3;

            int itemCount = _ListBox.Items.Count;
            _ListBox.Height = e.ItemHeight * (itemCount > 8 ? 8 : (itemCount < 4 ? 4 : itemCount + 1));
        }

        private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_ListBox.Tag?.ToString() == "1") return;

            if (_ListBox.SelectedItems.Count > 0)
            {
                this.Text = _ListBox.SelectedItem.ToString();
                this.SelectionStart = this.Text.Length;
                this.SelectedItem = (_ListBox.SelectedItem as ListBoxItem);
                if (_ListBox.Focused)
                {
                    _ListBox.SelectedIndex = -1;
                    hideDropdown(IntPtr.Zero);
                }
            }
        }

        //重置下拉列表数据源
        private void resetListSource(bool filterItem = true)
        {
            if (m_AutoCompleteSource != null)
            {
                List<ListBoxItem> lstFiltered = new List<ListBoxItem>();
                if (filterItem)
                {
                    this.SelectedItem = null;
                    if (this.Text.Trim().Length > 0)
                    {
                        ListBoxItem matchedItem = m_AutoCompleteSource.FirstOrDefault(o => o.ItemText == this.Text);
                        if (matchedItem == null)
                        {
                            lstFiltered = m_AutoCompleteSource.Where(o => o.ItemText.StartsWith(this.Text.Trim(), StringComparison.InvariantCultureIgnoreCase)).ToList();
                        }
                        else
                        {
                            this.SelectedItem = matchedItem;
                        }
                    }
                    else
                    {
                        //lstFiltered = m_AutoCompleteSource;
                    }
                }
                else
                {
                    lstFiltered = m_AutoCompleteSource;
                }

                setListBoxItems(lstFiltered);
            }
        }

        private void setListBoxItems(List<ListBoxItem> lstItems)
        {
            if (lstItems == null) return;

            _ListBox.Items.Clear();
            foreach (ListBoxItem item in lstItems)
            {
                _ListBox.Items.Add(item);
                if (item.ItemText == this.Text) _ListBox.SelectedItem = item;
            }
        }

        //显示自动提示的下拉列表
        private void showDropdown()
        {
            m_DropdownContainer.AutoClose = false;
            m_DropdownContainer.Show(this, this.DropdownPosition.X, this.DropdownPosition.Y);
        }

        //隐藏自动提示的下拉列表
        private void hideDropdown(IntPtr invokerHWnd)
        {
            if (m_DropdownContainer != null && m_DropdownContainer.AutoClose == false)
            {
                m_DropdownContainer.AutoClose = true;
                m_DropdownContainer.Hide();
                this.HiddenInvoker = invokerHWnd;
            }
        }

        //是否有指定类型的父级控件
        private bool containedInParent<T>(Control ctl)
        {
            if (ctl.Parent == null) return false;

            if (ctl.Parent is T)
                return true;
            else
                return containedInParent<T>(ctl.Parent);
        }

        /// <summary>
        /// 获取指定类型的外部控件
        /// </summary>
        /// <param name="ctl">当前控件</param>
        /// <param name="parentType">外部/上级控件类型</param>
        /// <returns>指定类型的外部控件</returns>
        private Control getSpecifiedParent(Control ctl, Type parentType)
        {
            if (ctl.Parent == null) return null;

            if (parentType.IsInstanceOfType(ctl.Parent))
                return ctl.Parent;
            else
                return getSpecifiedParent(ctl.Parent, parentType);
        }

        /// <summary>
        /// 自定义消息筛选器(使工具栏容器AutoClose设置为false之后依然能自动隐藏)
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_MBUTTONDOWN:
                case WM_NCLBUTTONDOWN:
                case WM_NCRBUTTONDOWN:
                case WM_NCMBUTTONDOWN:
                    //case WM_ACTIVATEAPP:
                    //if (m.HWnd != this.Handle)
                    if (_ListBox != null && m.HWnd != _ListBox.Handle)
                    {
                        if (m_DropdownContainer.Visible)
                        {
                            hideDropdown(m.HWnd);
                        }
                    }
                    break;
                default:
                    break;
            }

            return false;
        }


        private void WmPaint(ref Message m)
        {
            using (Graphics g = Graphics.FromHwnd(base.Handle))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 绘制背景和边框，但不覆盖系统绘制的实际文本
                renderInnerBackground(g, this.ClientRectangle);

                // 只在文本为空时绘制占位符文本
                if (!string.IsNullOrEmpty(_placeHolderStr) && string.IsNullOrEmpty(this.Text))
                {
                    Rectangle contentRect = new Rectangle(5, 2, this.Width - 8, this.Height - 6); // 左侧边距与EM_SETMARGINS保持一致
                    using (StringFormat textFormat = new StringFormat(StringFormatFlags.NoWrap) { LineAlignment = StringAlignment.Center })
                    {
                        using (SolidBrush brush = new SolidBrush(Color.Gray))
                        {
                            g.DrawString(_placeHolderStr, this.Font, brush, contentRect, textFormat);
                        }
                    }
                }
            }
        }

        // 跟踪鼠标是否在控件上
        private bool _isMouseOver = false;

        // 重写MouseEnter事件
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isMouseOver = true;
            this.Invalidate(); // 只在状态变化时重绘
        }

        // 重写MouseLeave事件
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isMouseOver = false;
            this.Invalidate(); // 只在状态变化时重绘
        }

        private void renderInnerBackground(Graphics g, Rectangle rect)
        {
            if (_roundStyle != RoundStyle.None)
            {
                // 使用ControlPaint.DrawBorder来覆盖原始边框，保持文本显示 (无效--覆盖不到Region以外的区域)
                //Rectangle outRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                //using (Pen clearPen = new Pen(this.BackColor, 2))  // 使用与背景色相同的笔
                //{
                //    //// 绘制与背景色相同的边框，覆盖原始的系统边框
                //    //ControlPaint.DrawBorder(g, rect, this.BackColor, 2, ButtonBorderStyle.Solid,
                //    //    this.BackColor, 2, ButtonBorderStyle.Solid, this.BackColor, 2, ButtonBorderStyle.Solid,
                //    //    this.BackColor, 2, ButtonBorderStyle.Solid);

                //    g.DrawRectangle(clearPen, outRect);
                //}

                // 绘制圆角边框 - 根据焦点状态和鼠标悬停状态
                Rectangle borderRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                if (this.BorderStyle == BorderStyle.None)
                {
                    borderRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);
                }

                int innerBorderWidth = 1;
                using (GraphicsPath borderPath = GraphicsPathHelper.CreatePath(borderRect, _radius, _roundStyle, innerBorderWidth))
                {
                    //using (Pen clearPen = new Pen(this.BackColor, innerBorderWidth))
                    //{
                    //    g.DrawPath(clearPen, borderPath);
                    //}
                    if (this.Enabled)
                    {
                        if (this.Focused)
                        {
                            //获得焦点时显示蓝色边框
                            using (Pen borderPen = new Pen(Color.FromArgb(0, 120, 215), innerBorderWidth))
                            {
                                g.DrawPath(borderPen, borderPath);
                            }
                        }
                        else if (_isMouseOver)
                        {
                            //鼠标悬停时显示橙色边框
                            using (Pen borderPen = new Pen(Color.SandyBrown, innerBorderWidth))
                            {
                                g.DrawPath(borderPen, borderPath);
                            }
                        }
                        else
                        {
                            //普通状态显示浅灰色边框
                            using (Pen borderPen = new Pen(SystemColors.ActiveBorder, innerBorderWidth))
                            {
                                g.DrawPath(borderPen, borderPath);
                            }
                        }
                    }
                    else
                    {
                        using (Pen borderPen = new Pen(Color.LightGray, innerBorderWidth))
                        {
                            g.DrawPath(borderPen, borderPath);
                        }
                    }
                }
            }
        }

        private void checkForScrollbars()
        {
            if (string.IsNullOrEmpty(this.Text)) return;

            bool scroll = false;
            int cnt = 0;
            for (int i = 0; i < this.Text.ToArray().Length; i++)
            {
                if (this.GetFirstCharIndexFromLine(i) < 0)
                {
                    cnt = i;
                    break;
                }
            }

            if (cnt > 1)
            {
                int pos0 = this.GetPositionFromCharIndex(this.GetFirstCharIndexFromLine(0)).Y;
                if (pos0 >= 32768) pos0 -= 65536;
                int pos1 = this.GetPositionFromCharIndex(this.GetFirstCharIndexFromLine(1)).Y;
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
                    if (this.ScrollBars != ScrollBars.Vertical) this.ScrollBars = ScrollBars.Vertical;
                }
                else
                {
                    if (this.ScrollBars != ScrollBars.None) this.ScrollBars = ScrollBars.None;
                }
            }
            catch (Win32Exception)
            {
                // this sometimes throws a "failure to create window handle" error.
                // This might happen if the TextBox is unvisible and/or
                // too small to display a toolbar.
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}
