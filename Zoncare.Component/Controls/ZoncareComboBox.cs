using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    public partial class ZoncareComboBox : ComboBox
    {
        //用于模拟键盘输入
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd); //返回hWnd参数所指定的窗口的设备环境。

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC); //函数释放设备上下文环境（DC）

        //下拉面板
        private Panel panel;
        //绘制面板用变量 - 光标前一位置
        private Point pPoint;
        //绘制面板用变量 - 光标当前位置
        private Point cPoint;
        //绘制面板用变量 - 鼠标是否已按下
        private bool isMouseDown = false;
        //关闭下拉时光标是否在ComboBox上
        private bool isCursorOnComboBox = false;

        private Rectangle rectDown; //预定义一个与下拉框高度相同的正方形区域用于绘制"下拉"或"清除"图标
        private Rectangle rectIcon; //"下拉"或"清除"图标实际显示区域
        private int dropDownWidth = 1, dropDownHeight = 1; //下拉列表的宽度及高度

        [Browsable(false)]
        public Control Control { get; set; }
        [Browsable(false)]
        public CheckedListBox CheckedListBox { get; set; }
        [Browsable(false)]
        public TreeView TreeView { get; set; }

        //控件类型
        public enum TypeC
        {
            /// <summary>
            /// 默认普通ComboBox
            /// </summary>
            ComboBox = 0,
            /// <summary>
            /// 具有复选框的下拉列表
            /// </summary>
            CheckedListBox = 1,
            /// <summary>
            /// 树形结构列表
            /// </summary>
            TreeView = 2,
            /// <summary>
            /// 具有占位字符串的下拉列表
            /// </summary>
            WithPlaceHolder = 3,
            /// <summary>
            /// 可变大小ComboBox(通过改变ItemHeight属性来改变高度)
            /// </summary>
            SizeChangeable = 4,
            /// <summary>
            /// 自定义展开图标
            /// </summary>
            CustomIcon = 5,
            /// <summary>
            /// 其它普通控件
            /// </summary>
            Control = 99
        }

        private TypeC _CtlType = TypeC.ComboBox;
        private bool _isUnresponsive;

        [Description("设置下拉框类型"), DefaultValue(0)]
        public TypeC CtlType
        {
            get
            {
                return _CtlType;
            }
            set
            {
                _CtlType = value;
                switch (_CtlType)
                {
                    case TypeC.ComboBox:
                        this.DrawMode = DrawMode.OwnerDrawFixed;
                        this.DropDownHeight = 106;
                        break;
                    case TypeC.CustomIcon:
                    case TypeC.WithPlaceHolder:
                        this.DrawMode = DrawMode.OwnerDrawVariable;
                        SetStyle(ControlStyles.AllPaintingInWmPaint
                            | ControlStyles.UserPaint
                            | ControlStyles.OptimizedDoubleBuffer, true);
                        break;
                    case TypeC.SizeChangeable:
                        this.DrawMode = DrawMode.OwnerDrawVariable;
                        break;
                    case TypeC.CheckedListBox:
                        this.DrawMode = DrawMode.OwnerDrawFixed;
                        this.drawPanel();
                        this.SetDropDown(new CheckedListBox());
                        break;
                    case TypeC.TreeView:
                        this.DrawMode = DrawMode.OwnerDrawFixed;
                        this.drawPanel();
                        this.SetDropDown(new TreeView());
                        break;
                    default:
                        this.DrawMode = DrawMode.OwnerDrawFixed;
                        this.drawPanel();
                        this.SetDropDown(new Panel());
                        break;
                }

                this.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }

        [Description("未选择下拉列表项时控件显示区域的文本"), Category("Data"), Browsable(true)]
        public String PlaceHolderStr { get; set; }

        [Description("选定项的集合"), Category("Data"), Browsable(false)]
        public List<object> SelectedItems { get; } = new List<object>();

        [Browsable(false), Description("是否正在绑定数据源")]
        public bool IsDataBinding { get; private set; }
        [Browsable(false), Description("是否已显示到界面")]
        public bool IsShown { get; set; }
        [Browsable(false), Description("是否不响应用户操作")]
        public bool IsUnresponsive
        {
            get => _isUnresponsive;
            set
            {
                _isUnresponsive = value;
                if (_isUnresponsive)
                    this.Cursor = Cursors.No;
                else
                    this.Cursor = Cursors.Arrow;
                this.Invalidate();
            }
        }

        public ZoncareComboBox()
        {
            InitializeComponent();
        }

        public ZoncareComboBox(IContainer container)
        {
            container.Add(this);
            InitializeComponent();

            //this.BackColor = Color.White;
            this.DoubleBuffered = true;
            dropDownHeight = this.DropDownHeight;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

            float scaleW = Screen.PrimaryScreen.WorkingArea.Width / 1920f;
            int w = Convert.ToInt32(18 * scaleW);
            int iconWH = w - 3; //分辨率在1920*1080下, 图标宽度和高度为15像素

            this.rectDown = new Rectangle(Width - w, 1, w - 1, Height - 2); //排除边框的宽度
            this.rectIcon = new Rectangle(rectDown.X + (rectDown.Width - iconWH) / 2, rectDown.Y + (rectDown.Height - iconWH) / 2, iconWH, iconWH);
            if (DropDownHeight > 1) dropDownHeight = this.DropDownHeight;
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            IsDataBinding = true;
            base.OnDataSourceChanged(e);
            IsDataBinding = false;
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (IsDataBinding) return;

            base.OnSelectedIndexChanged(e);
            if (_CtlType != TypeC.ComboBox) this.Invalidate();
        }

        protected override void OnSelectedItemChanged(EventArgs e)
        {
            base.OnSelectedItemChanged(e);
            if (_CtlType != TypeC.ComboBox) this.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this._CtlType == TypeC.WithPlaceHolder &&
                this.SelectedItem != null && rectIcon.Contains(this.PointToClient(Cursor.Position)))
            {
                this.Invalidate(rectDown);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!this.Focused)
            {
                ((HandledMouseEventArgs)e).Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (_CtlType == TypeC.ComboBox)
            {
                if (_isUnresponsive)
                {
                    //模拟Enter键，取消掉下拉状态
                    keybd_event(0xD, 0, 0, 0);
                    keybd_event(0xD, 0, 0x0002, 0);
                }
            }
            else if (_CtlType == TypeC.Control || _CtlType == TypeC.TreeView || _CtlType == TypeC.CheckedListBox)
            {
                if (isCursorOnComboBox)
                {
                    isCursorOnComboBox = false;
                    //模拟Enter键，取消掉下拉状态
                    keybd_event(0xD, 0, 0, 0);
                    keybd_event(0xD, 0, 0x0002, 0);
                }
                else
                {
                    //创建下拉窗口
                    ToolStripControlHost toolStripControlHost = new ToolStripControlHost(this.panel);
                    HsToolStripDropDown toolStripDropDown = new HsToolStripDropDown();
                    //设置边框
                    toolStripControlHost.Margin = Padding.Empty;
                    toolStripControlHost.Padding = Padding.Empty;
                    toolStripControlHost.AutoSize = false;
                    toolStripDropDown.Padding = Padding.Empty;
                    //添加
                    toolStripDropDown.Items.Add(toolStripControlHost);
                    toolStripDropDown.Show(this, 0, this.Height);
                    //设置宽度最小值
                    if (this.panel.Width < this.Width)
                    {
                        this.panel.Size = new Size(this.Width, this.panel.Height);
                    }
                    //判断关闭时光标在ComboBox组件内
                    toolStripDropDown.Closed += delegate (object sender, ToolStripDropDownClosedEventArgs e1)
                    {
                        Rectangle rec = new Rectangle(0, 0, this.Width, this.Height);
                        this.isCursorOnComboBox = rec.Contains(this.PointToClient(Cursor.Position));
                    };
                    //设置焦点
                    toolStripDropDown.Focus();
                }
            }
        }

        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);

            if (_CtlType == TypeC.ComboBox)
            {
                if (_isUnresponsive)
                    this.DropDownHeight = 1;
                else if (this.DropDownHeight == 1)
                    this.DropDownHeight = dropDownHeight;
            }

            if ((_CtlType == TypeC.ComboBox ||
                _CtlType == TypeC.SizeChangeable ||
                _CtlType == TypeC.WithPlaceHolder ||
                _CtlType == TypeC.CustomIcon) && dropDownWidth <= 1)
            {
                resetDropDownWidth();
            }

            if (_CtlType == TypeC.WithPlaceHolder)
            {
                if (this.SelectedItem != null)
                {
                    if (rectIcon.Contains(this.PointToClient(Cursor.Position))) //判断是否点击了关闭图标
                    {
                        //此情形鼠标左键点击消息将会被拦截, 因此以下代码将不会执行.
                        this.DropDownHeight = 1; //隐藏原始下拉框
                        this.SelectedItem = null;

                        SendKeys.Send("{F4}"); //使下拉框收起
                    }
                }
                else
                {
                    if (this.DropDownHeight <= 1)
                    {
                        this.DropDownHeight = dropDownHeight;
                    }
                }
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            //base.OnDrawItem(e);
            //if (IsShown == false) return;

            Graphics g = e.Graphics;

            if (_CtlType == TypeC.ComboBox && _isUnresponsive)
            {
                //e.Graphics.FillRectangle(Brushes.Gainsboro, e.Bounds);
                using (SolidBrush textBrush = new SolidBrush(SystemColors.ControlDark))
                {
                    g.DrawString(this.Items[e.Index].ToString(), e.Font, textBrush, e.Bounds);
                }
                return;
            }

            if (e.Index >= 0)
            {
                if (_CtlType == TypeC.ComboBox)
                {
                    bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                    bool isFocused = (e.State & DrawItemState.Focus) == DrawItemState.Focus;
                    bool isInEdit = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
                    Color backColor = isSelected && !isInEdit ? e.BackColor : this.BackColor; // 可自定义背景色 (SystemColors.Highlight)
                    Color textColor = isSelected && !isInEdit ? e.ForeColor : this.ForeColor; // 文字颜色 (SystemColors.HighlightText)

                    if (this.Enabled == false)
                    {
                        backColor = SystemColors.ControlLight;
                        textColor = SystemColors.GrayText;
                    }
                    // 绘制背景
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        g.FillRectangle(brush, e.Bounds);
                    }
                    // 绘制文字
                    Rectangle textBounds = e.Bounds;
                    if (isFocused)
                    {
                        if (!isSelected) textBounds.X += 1; // 轻微偏移，避免文字紧贴边框

                        //绘制虚线框突出焦点状态
                        if (!this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
                            ControlPaint.DrawFocusRectangle(g, e.Bounds, Color.Black, Color.Black);
                    }

                    using (SolidBrush textBrush = new SolidBrush(textColor))
                    {
                        g.DrawString(this.Items[e.Index].ToString(), e.Font, textBrush, e.Bounds);
                    }
                }
                else if (this._CtlType == TypeC.SizeChangeable || this._CtlType == TypeC.WithPlaceHolder || this._CtlType == TypeC.CustomIcon)
                {
                    using (StringFormat textFormat = new StringFormat(StringFormatFlags.NoWrap) { LineAlignment = StringAlignment.Center })
                    {
                        object item = this.Items[e.Index];
                        //string s = this.Items[e.Index].ToString();
                        //if (!string.IsNullOrEmpty(this.DisplayMember))
                        //{
                        //    //s = item.GetType().GetProperty(this.DisplayMember).GetValue(item, null).ToString();
                        //    s = getPropertyValue(item, this.DisplayMember);
                        //}
                        string itemText = this.GetItemText(item);

                        //e.DrawBackground();
                        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus || (e.State & DrawItemState.Selected) == DrawItemState.Selected)
                            g.FillRectangle(Brushes.Green, e.Bounds);
                        else
                            g.FillRectangle(Brushes.CornflowerBlue, e.Bounds);

                        Font itemFont = new Font(e.Font.FontFamily, e.Font.Size - 1);
                        g.DrawString(itemText, itemFont, new SolidBrush(e.ForeColor), e.Bounds, textFormat);
                        e.DrawFocusRectangle();
                    }
                }
            }
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            //if (this.CtlType != TypeC.SizeChangeable)
            //    this.FindForm().ActiveControl = null;
        }


        protected override void WndProc(ref Message m)
        {
            //WM_PAINT = 0xf; 要求一个窗口重画自己, 即Paint事件时
            //WM_CTLCOLOREDIT = 0x133; 当一个编辑型控件将要被绘制时发送此消息给它的父窗口；
            //通过响应这条消息, 所有者窗口可以通过使用给定的相关显示设备的句柄来设置编辑框的文本和背景颜色

            //const int WM_ERASEBKGND = 0x14; //窗体消息拦截(程序刷新时可能会出现清除背景)
            //if (m.Msg == (int)WM_ERASEBKGND)
            //{
            //    m.Msg = (int)IntPtr.Zero;
            //}

            try
            {
                if (_CtlType == TypeC.ComboBox || _CtlType == TypeC.Control)
                {
                    base.WndProc(ref m);
                }
                else
                {
                    if (_CtlType == TypeC.WithPlaceHolder)
                    {
                        //if (DesignMode) //设计模式时控件自动绘制
                        //{
                        //    this.SetStyle(ControlStyles.UserPaint, false);
                        //    base.WndProc(ref m);
                        //    return;
                        //}
                        //else
                        //{
                        this.SetStyle(ControlStyles.UserPaint, true);
                        //}

                        if (m.Msg == 0x201) //拦截 WM_LBUTTONDOWN(鼠标按下)消息, 阻止展开下拉列表
                        {
                            if (this.SelectedItem != null && rectIcon.Contains(this.PointToClient(Cursor.Position)))
                            {
                                this.SelectedItem = null;
                                drawEditArea_ph(m.HWnd);
                                return;
                            }
                        }

                        base.WndProc(ref m);
                        if (m.Msg == 0xF || m.Msg == 0x133) //WM_PAINT, WM_CTLCOLOREDIT
                        {
                            drawEditArea_ph(m.HWnd);
                        }

                        if (m.Msg == 0x0007) //WM_SETFOCUS 获得焦点
                        {
                            //drawEditArea(m.HWnd);
                        }
                    }
                    else
                    {
                        this.SetStyle(ControlStyles.UserPaint, true);
                        base.WndProc(ref m);
                        if (m.Msg == 0xF || m.Msg == 0x133) //WM_PAINT, WM_CTLCOLOREDIT
                        {
                            drawEditArea_custom(m.HWnd);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("下拉框windows错误消息: " + ex.Message); }
        }

        //绘制文本区域及右侧图标 -- 自定义右侧图标
        private void drawEditArea_custom(IntPtr hWnd)
        {
            IntPtr hDC = GetWindowDC(hWnd);
            if (hDC.ToInt32() == 0) return;

            //建立Graphics对像
            Graphics graphics = Graphics.FromHdc(hDC);
            string displayText = this.GetItemText(this.SelectedItem);

            Rectangle rect = this.DisplayRectangle;
            RectangleF rectText = new RectangleF(1, 1, this.Width - rectDown.Width, Height - 2);

            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            BufferedGraphics gBuffer = currentContext.Allocate(graphics, rect);
            Graphics g = gBuffer.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.Clear(this.BackColor);

            //画边框
            Color borderColor = Color.Empty;
            if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
                borderColor = Color.WhiteSmoke;
            else
                borderColor = Color.White;

            ControlPaint.DrawBorder(g, rect, borderColor, 2, ButtonBorderStyle.Solid, borderColor, 2, ButtonBorderStyle.Solid,
                borderColor, 2, ButtonBorderStyle.Solid, borderColor, 2, ButtonBorderStyle.Solid);

            //绘制显示文本及右侧图标
            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Near,       //水平向左对齐
                LineAlignment = StringAlignment.Center, //垂直向下对齐
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces
            };
            Image img = null;
            g.DrawString(displayText, this.Font, new SolidBrush(this.ForeColor), rectText, sf);
            if (this.DroppedDown == false)
            {
                img = Properties.Resources.ResourceManager.GetObject("combobox_expand_arrow") as Image;
            }
            else
            {
                img = Properties.Resources.ResourceManager.GetObject("combobox_down_arrow") as Image;
            }
            g.DrawImage(img, rectIcon);

            gBuffer.Render(graphics);
            gBuffer.Dispose();
            sf.Dispose();

            ReleaseDC(hWnd, hDC);
        }

        //绘制文本区域及右侧图标 -- 占位符形式下拉框
        private void drawEditArea_ph(IntPtr hWnd)
        {
            IntPtr hDC = GetWindowDC(hWnd);
            if (hDC.ToInt32() == 0) return; //若获取设备上下文失败则返回

            //建立Graphics对像
            Graphics graphics = Graphics.FromHdc(hDC);
            string displayText = this.PlaceHolderStr;
            if (!string.IsNullOrEmpty(this.Text)) displayText = this.Text;
            if (this.SelectedItem != null) displayText = this.GetItemText(this.SelectedItem);

            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Near,       //水平向左对齐
                LineAlignment = StringAlignment.Center, //垂直向下对齐
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces
            };

            Rectangle rect = this.DisplayRectangle;
            RectangleF rectText = new RectangleF(1, 1, this.Width - rectDown.Width, Height - 2);

            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            BufferedGraphics gBuffer = currentContext.Allocate(graphics, rect);
            Graphics g = gBuffer.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.Clear(this.BackColor);

            //画边框
            if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
                ControlPaint.DrawBorder(g, rect, SystemColors.HotTrack, ButtonBorderStyle.Solid);
            else
                ControlPaint.DrawBorder(g, rect, Color.SlateGray, ButtonBorderStyle.Solid);

            //if (this.DropDownStyle == ComboBoxStyle.DropDown)
            //{
            //    Rectangle rectEdit = new Rectangle(1, 1, this.Width - rectDown.Width - 2, this.Height - 2);
            //    bool cursorInEditArea = rectEdit.Contains(this.PointToClient(Cursor.Position));
            //    if (this.ContainsFocus)
            //        ComboBoxRenderer.DrawTextBox(g, rectEdit, System.Windows.Forms.VisualStyles.ComboBoxState.Hot);
            //    else //if (cursorInEditArea)
            //        ComboBoxRenderer.DrawTextBox(g, rectEdit, System.Windows.Forms.VisualStyles.ComboBoxState.Normal);
            //}

            //绘制显示文本及右侧图标
            Image img = null;
            if (this.SelectedItem == null)
            {
                g.DrawString(displayText, this.Font, new SolidBrush(Color.Gray), rectText, sf);

                img = Properties.Resources.ResourceManager.GetObject("arrow_down") as Image;
                //ComboBoxRenderer.DrawDropDownButton(g, rectDown, System.Windows.Forms.VisualStyles.ComboBoxState.Normal);
            }
            else
            {
                g.DrawString(displayText, this.Font, new SolidBrush(this.ForeColor), rectText, sf);

                if (rectIcon.Contains(this.PointToClient(Cursor.Position)))
                    img = Properties.Resources.ResourceManager.GetObject("close_on") as Image;
                else
                    img = Properties.Resources.ResourceManager.GetObject("close_normal") as Image;
            }
            g.DrawImage(img, rectIcon);

            gBuffer.Render(graphics);
            gBuffer.Dispose();
            sf.Dispose();

            ReleaseDC(hWnd, hDC); //释放DC
        }

        /// <summary>
        /// 绘制下拉面板
        /// </summary>
        private void drawPanel()
        {
            if (this.panel != null) return;

            this.panel = new Panel();
            this.panel.Size = new Size(this.Width, 200);
            this.panel.Padding = new Padding(1, 1, 1, 13);
            this.panel.BackColor = Color.Gainsboro;
            //绘制边线
            this.panel.Paint += delegate (object sender, PaintEventArgs e)
            {
                ControlPaint.DrawBorder(e.Graphics,
                               this.panel.ClientRectangle,
                               Color.DarkGray,
                               1,
                               ButtonBorderStyle.Solid,
                               Color.DarkGray,
                               1,
                               ButtonBorderStyle.Solid,
                               Color.DarkGray,
                               1,
                               ButtonBorderStyle.Solid,
                               Color.DarkGray,
                               1,
                               ButtonBorderStyle.Solid);
            };
            //使用Label实现右下角拖动按钮
            Label label = new Label();
            label.Text = "◢";
            label.Font = new Font("宋体", 9);
            label.Parent = this.panel;
            label.AutoSize = true;
            label.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
            label.Location = new Point(this.panel.Location.X + this.panel.Size.Width - label.Width + 3,
                this.panel.Location.Y + this.panel.Size.Height - label.Height - 1);
            //实现缩放功能
            label.MouseDown += delegate (object sender, MouseEventArgs e1)
            {
                this.pPoint = Cursor.Position;
                this.isMouseDown = true;
            };
            label.MouseLeave += delegate (object sender, EventArgs e1)
            {
                this.isMouseDown = false;
            };
            label.MouseMove += delegate (object sender, MouseEventArgs e1)
            {
                this.cPoint = Cursor.Position;
                if (e1.Button == MouseButtons.Left && isMouseDown)
                {
                    this.panel.Height = Math.Max(this.panel.Height + cPoint.Y - pPoint.Y, 23);
                    this.panel.Width = Math.Max(this.panel.Width + cPoint.X - pPoint.X, this.Width);
                    pPoint = Cursor.Position;
                }
                else
                {
                    label.Cursor = Cursors.SizeNWSE;
                }
            };
        }

        /// <summary>
        /// 设置下拉内容-普通Control
        /// </summary>
        public void SetDropDown(Control control)
        {
            //需要将此前的Control注销掉，否则会一直显示在Panel中
            if (this.Control != null)
            {
                this.Control.Dispose();
            }
            this.Control = control;
            this.Control.Location = new Point(0, 0);
            this.Control.Dock = DockStyle.Fill;
            this.Control.Parent = this.panel;
            //将下拉高度设为1,实现隐藏效果
            this.DropDownHeight = 1;
        }

        /// <summary>
        /// 设置下拉内容-复选框列表
        /// </summary>
        public void SetDropDown(CheckedListBox checkedListBox)
        {
            //单击可选中
            checkedListBox.CheckOnClick = true;
            //边框格式
            checkedListBox.BorderStyle = BorderStyle.None;
            //去掉强制的高度修改,CheckedListBox的一个特殊高度判断，如果为true则会在底端多出一段空白
            checkedListBox.IntegralHeight = false;
            //选中事件添加监听（此时还未选中，刚Check还没有Checked）
            checkedListBox.ItemCheck += delegate (object sender, ItemCheckEventArgs e)
            {
                CheckedListBox chkListBox = this.Control as CheckedListBox;
                String text = "";
                for (int i = 0; i < chkListBox.Items.Count; i++)
                {
                    //使用异或特殊处理当前正在check的条目
                    if ((i == e.Index) != chkListBox.GetItemChecked(i))
                    {
                        text += chkListBox.Items[i].ToString() + ",";
                    }
                }
                text = text.Substring(0, Math.Max(text.Length - 1, 0));
                //显示所有内容
                showText(text);
            };
            this.CheckedListBox = checkedListBox;
            this.SetDropDown((Control)checkedListBox);
        }

        /// <summary>
        /// 设置下拉内容-树形列表
        /// </summary>
        public void SetDropDown(TreeView treeView)
        {
            //整行选择
            treeView.FullRowSelect = true;
            //边框格式
            treeView.BorderStyle = BorderStyle.None;
            //选中事件添加监听
            treeView.AfterSelect += delegate (object sender, TreeViewEventArgs e)
            {
                showText(((TreeView)(this.Control)).SelectedNode.Text);
            };
            this.TreeView = treeView;
            this.SetDropDown((Control)treeView);
        }

        /// <summary>
        /// 显示复选框列表或树形列表选中项的文本信息
        /// </summary>
        /// <param name="Text">信息内容</param>
        private void showText(string text)
        {
            //当DropDownStyle = DropDownList时不能直接对Text赋值

            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(this.PlaceHolderStr))
            {
                text = this.PlaceHolderStr;
            }
            this.Items.Clear();
            this.Items.Add(text);
            this.SelectedIndex = 0;
        }

        //重置下拉列表宽度
        private void resetDropDownWidth()
        {
            dropDownWidth = this.DropDownWidth = getMaxWidthOfComboxItems();
        }

        //获取下拉选项的最大长度
        private int getMaxWidthOfComboxItems()
        {
            int maxLen = this.Width;

            if (this.Items.Count > 0)
            {
                using (Graphics g = this.CreateGraphics())
                {
                    g.PageUnit = GraphicsUnit.Pixel;
                    for (int i = 0; i < this.Items.Count; i++)
                    {
                        object item = this.Items[i];
                        string itemText = this.GetItemText(item);

                        SizeF itemTextSize = g.MeasureString(itemText, this.Font);
                        if (maxLen < itemTextSize.Width) maxLen = Convert.ToInt32(itemTextSize.Width);
                    }
                }
            }

            return maxLen;
        }

    }

    /// <summary>
    /// 重写ToolStripDropDown
    /// 使用双缓存减少闪烁
    /// </summary>
    public class HsToolStripDropDown : ToolStripDropDown
    {
        /// <summary>
        /// 在当前程序激活或即将失去激活状态时发生
        /// </summary>
        public event EventHandler ActiveChanged;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case 0x001C: //激活或即将失去激活状态
                    this.ActiveChanged?.Invoke(this, null);
                    break;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; //双缓存
                return cp;
            }
        }
    }
}
