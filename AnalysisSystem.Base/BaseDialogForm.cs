using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisSystem.Base
{
    public partial class BaseDialogForm : BaseForm
    {
        private const int WM_NCLBUTTONDOWN = 0x00A1; //当光标在窗口的非客户区并按下鼠标左键时发送此消息
        private const int WM_NCLBUTTONDBLCLK = 0x00A3; //当光标在窗口的非客户区并双击鼠标左键时发送此消息
        private const int HT_CAPTION = 0x2;
        private const int SC_SIZE = 0xF000;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;
        private const int RIGHTBOTTOM = 0x0008;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_MINIMIZEBOX = 0x00020000;

        private const int WM_SYSCOMMAND = 0x0112;
        protected const int WM_SETREDRAW = 0x000B;

        protected const int GWL_HWNDPARENT = -8; // 设置所有者窗口的标志
        protected const int SW_SHOW = 5;
        protected const int SW_HIDE = 0;

        private IWin32Window _owner;
        private bool _showMinimize = false; //是否显示最小化按钮
        private bool _allowMaxsize = false; //是否允许最大化及还原
        private bool _isShown = false;
        private FormWindowState _originalState;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hMenu, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// 位数安全的 SetWindowLong：GWL_HWNDPARENT(-8) 属于指针宽度的索引，
        /// 在 x64 进程中必须改用 SetWindowLongPtr，否则 32 位版 SetWindowLong 会静默失败，
        /// 导致 Owner 关系根本建立不起来（子窗口随后会被激活的主窗体压住）。
        /// </summary>
        protected static IntPtr SetWindowLongPtrSafe(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }
            return new IntPtr(SetWindowLong(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        public BaseDialogForm()
        {
            InitializeComponent();

            this.tlpTop.Height = 0;
            this.tlpTop.Visible = false;

            _showMinimize = false;
            _allowMaxsize = false;
        }

        public BaseDialogForm(bool allowMaxsize, bool showMinimize = false)
        {
            InitializeComponent();

            this.ControlBox = false;
            this.tlpTop.Visible = true;
            this.tlpTop.Height = 32;

            _showMinimize = showMinimize;
            _allowMaxsize = allowMaxsize;

            //if (this.WindowState != FormWindowState.Maximized) this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tip">功能提示</param>
        /// <param name="title">窗口标题</param>
        /// <param name="allowMaxsize">是否允许最大化及还原</param>
        public BaseDialogForm(string tip, string title, bool allowMaxsize = false, bool showMinimize = false)
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.tlpTop.Visible = true;
            this.tlpTop.Height = 32;

            _showMinimize = showMinimize;
            _allowMaxsize = allowMaxsize;

            this.Padding = new Padding(1);
            SetTitle(title);
            SetTipText(tip);

            //if (!string.IsNullOrEmpty(this.lblTitle.Text)) this.Height += this.tlpTop.Height;
        }

        /// <summary>
        /// 以模态窗口显示
        /// </summary>
        /// <param name="owner">窗口所属的父窗口</param>
        /// <param name="canResize">是否可以拖动以改变窗口大小</param>
        /// <param name="showLayer">是否在父窗口显示遮罩层</param>
        /// <param name="displayRegion">遮罩层位于所属窗口的区域</param>
        /// <returns></returns>
        public DialogResult ShowDialogEx0(IWin32Window owner, bool canResize = false, bool showLayer = false, Rectangle displayRegion = default(Rectangle))
        {
            _owner = owner;

            if (canResize) this.picBox.Visible = true;

            Form ownerForm = owner as Form;
            if (ownerForm != null)
            {
                DialogResult dlgResult = DialogResult.None;
                if (ownerForm.IsHandleCreated)
                {
                    if (showLayer)
                    {
                        PictureBox picLayer = null;
                        var arrLayer = ownerForm.Controls.Find("picLayer", false);
                        if (arrLayer.Length > 0)
                        {
                            picLayer = arrLayer[0] as PictureBox;
                        }
                        else
                        {
                            picLayer = new PictureBox() { Name = "picLayer", BackColor = SystemColors.ControlDark };
                            ownerForm.Controls.Add(picLayer);
                        }

                        if (displayRegion == Rectangle.Empty) { displayRegion = ownerForm.ClientRectangle; }
                        using (Bitmap bmp = new Bitmap(displayRegion.Width, displayRegion.Height))
                        {
                            //ownerForm.DrawToBitmap(bmp, new Rectangle(0, 0, ownerForm.Width, ownerForm.Height));
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.CopyFromScreen(ownerForm.PointToScreen(displayRegion.Location), new Point(0, 0), displayRegion.Size);
                            }

                            Image imgNew = PubFuns.GetTransparentImage(bmp, 0.5F);
                            picLayer.Location = displayRegion.Location;
                            picLayer.Size = ownerForm.ClientSize;
                            picLayer.Image = imgNew;
                            picLayer.Visible = true;
                            picLayer.BringToFront();
                            dlgResult = this.ShowDialog(owner);

                            SendMessage(ownerForm.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);  // 禁止重绘
                            picLayer.Visible = false;
                            SendMessage(ownerForm.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);  // 恢复重绘
                            ownerForm.Refresh();
                        }
                    }
                    else
                    {
                        dlgResult = this.ShowDialog(owner);
                    }

                    //ownerForm.Activate();
                }
                return dlgResult;
            }
            else
            {
                return this.ShowDialog();
            }
        }

        public DialogResult ShowDialogEx(IWin32Window owner, bool canResize = false, bool showLayer = false, Rectangle displayRegion = default(Rectangle))
        {
            _owner = owner;

            if (canResize) this.picBox.Visible = true;

            Form ownerForm = owner as Form;
            if (ownerForm != null)
            {
                DialogResult dlgResult = DialogResult.None;
                if (ownerForm.IsHandleCreated)
                {
                    if (showLayer)
                    {
                        if (displayRegion == Rectangle.Empty) { displayRegion = ownerForm.ClientRectangle; }

                        var overlay = new SemiTransparentOverlay(ownerForm);
                        overlay.Show(ownerForm);
                        this.Shown += (o, e) =>
                        {
                            this.BeginInvoke(new MethodInvoker(() =>
                            {
                                //overlay.Show(ownerForm);
                                this.Owner = overlay;
                                this.BringToFront();
                            }));
                        };
                        dlgResult = this.ShowDialog();

                        overlay.Close();
                        overlay.Dispose();
                    }
                    else
                    {
                        dlgResult = this.ShowDialog(owner);
                    }

                    //ownerForm.Activate();
                }
                return dlgResult;
            }
            else
            {
                return this.ShowDialog();
            }
        }

        protected void SetTitle(string title)
        {
            this.lblTitle.Text = title;
            this.Text = title;
        }

        protected void SetTipText(string tipText)
        {
            this.lblTip.Text = tipText;
        }

        private void BaseDialogForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) { return; }

            if (this.tlpTop.Visible && !string.IsNullOrEmpty(this.lblTitle.Text))
            {
                this.Height += this.tlpTop.Height;
            }

            this.lblTip.MouseDown += tlpTop_MouseDown;
            this.lblTitle.MouseDown += tlpTop_MouseDown;

            this.picMin.Click += (o, args) => { this.WindowState = FormWindowState.Minimized; };
            this.picMin.MouseEnter += (o, args) => { this.picMin.BackColor = PubFuns.GetColorPlus(this.tlpTop.BackColor, 0, 40, 40, 40); };
            this.picMin.MouseLeave += (o, args) => { this.picMin.BackColor = this.tlpTop.BackColor; };
            this.picClose.Click += (o, args) => { this.Close(); };
            this.picClose.MouseEnter += (o, args) => { this.picClose.BackColor = Color.FromArgb(200, 232, 35, 35); };
            this.picClose.MouseLeave += (o, args) => { this.picClose.BackColor = this.tlpTop.BackColor; };

            disableMaxControlBox();
            this.picMin.Visible = _showMinimize;
        }

        private void BaseDialogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_owner != null)
            {
                //if (_owner is UnifiedContainer)
                //{
                //    (_owner as UnifiedContainer).DisplayShadowLayer?.Invoke(false);
                //}
                //else if (_owner is Form)
                //{
                //    Form ownerForm = _owner as Form;
                //    //if (ownerForm.Name == "MainForm" && ownerForm.WindowState == FormWindowState.Minimized)
                //    //{
                //    //    e.Cancel = true; //取消子窗口的关闭操作
                //    //}
                //}
            }
        }

        private void BaseDialogForm_Activated(object sender, EventArgs e)
        {
            if (_isShown) //处理shown事件发生之后再次触发的情形
            {
                if (_originalState == FormWindowState.Maximized && this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = _originalState;
                }
            }
        }

        private void BaseDialogForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Silver,
                new Rectangle(base.ClientRectangle.Location.X, base.ClientRectangle.Location.Y, base.ClientRectangle.Width - 1, base.ClientRectangle.Height - 1));
        }

        private void BaseDialogForm_SizeChanged(object sender, EventArgs e)
        {
            //disableMaxControlBox();

            this.picBox.Left = this.ClientSize.Width - this.Padding.Right - this.picBox.Width;
            this.picBox.Top = this.ClientSize.Height - this.Padding.Bottom - this.picBox.Height;

            this.Invalidate();
        }

        private void BaseDialogForm_Shown(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                this.picBox.BringToFront();
                this.picBox.MouseDown += picBox_MouseDown;

                _originalState = this.WindowState;
                _isShown = true;
            }
        }

        private void picBox_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as Control).Capture = false;

            Message msg = Message.Create(this.Handle, WM_SYSCOMMAND, (IntPtr)(SC_SIZE + RIGHTBOTTOM), IntPtr.Zero); //拖动右下角改变窗体大小
            base.WndProc(ref msg);
        }

        private void picBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics gc = e.Graphics;
            using (Brush brush = new SolidBrush(Color.Black))
            {
                Point p1 = new Point(picBox.ClientSize.Width, 0);
                Point p2 = new Point(0, picBox.ClientSize.Height);
                Point p3 = new Point(picBox.ClientSize.Width, picBox.ClientSize.Height);
                gc.FillPolygon(brush, new Point[] { p1, p2, p3 });
            }
        }

        private void tlpTop_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as Control).Capture = false;

            //发送点击窗体边框的windows消息
            Message msg = Message.Create(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            base.WndProc(ref msg);
        }

        private void disableMaxControlBox()
        {
            if (DesignMode) return;

            if (_allowMaxsize == false)
            {
                //若最大化按钮禁用, 将窗口最大化时任务栏会被遮挡
                this.MaximizeBox = true;
                //this.MinimizeBox = false;

                //禁用最大化按钮
                int GWL_STYLE = -16;
                int nStyle = GetWindowLong(this.Handle, GWL_STYLE);
                nStyle &= ~(WS_MAXIMIZEBOX); //禁用最大化按钮 (启用: nStyle |= WS_MAXIMIZEBOX)
                //nStyle &= ~(WS_MINIMIZEBOX | WS_MAXIMIZEBOX); //实际效果是最小化和最大化按钮都隐藏了

                SetWindowLong(this.Handle, GWL_STYLE, nStyle);
                SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0020); //禁用后进行界面的刷新，否则禁用状态不会立即显示在界面上
            }
        }

        protected void ResetMaxWindowState()
        {
            if (this.WindowState == FormWindowState.Minimized) return;

            if (this.Width < Screen.PrimaryScreen.WorkingArea.Width)
            {
                if (this.WindowState == FormWindowState.Maximized) this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
            }
            else if (this.Width > Screen.PrimaryScreen.WorkingArea.Width)
            {
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (!DesignMode)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    if (m.Msg == 0x128) return; //禁用WM_UPDATEUISTATE消息, 避免窗口切换时清除绘制的图面

                    if (m.Msg == WM_SYSCOMMAND)
                    {
                        switch ((int)m.WParam)
                        {
                            //禁止拖拽标题栏还原窗体
                            case 0xF012:
                            case 0xF010:
                                if (_allowMaxsize == false) m.WParam = IntPtr.Zero;
                                break;
                            case SC_MINIMIZE: //最小化
                                if (this.Owner != null)
                                {
                                    //this.Owner.WindowState = FormWindowState.Minimized;
                                    //this.Visible = true;

                                    //// 强制重绘模态窗口
                                    //this.Refresh();
                                }
                                break;
                        }
                    }
                    else if (m.Msg == WM_NCLBUTTONDBLCLK)
                    {
                        if (_allowMaxsize == false) m.WParam = IntPtr.Zero;
                    }
                }
                //else if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == 0xF120) //还原窗口
                //{
                //    this.Owner.Visible = true; // 还原拥有者窗口可见性
                //}
            }

            base.WndProc(ref m);
        }
    }

    //遮罩层窗口
    public partial class SemiTransparentOverlay : Form
    {
        private Form parentForm;

        public SemiTransparentOverlay(Form parent)
        {
            parentForm = parent;

            // 关键设置
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.Black;
            this.Opacity = 0.3; // 半透明度
            //this.TopMost = true;

            // 设置大小和位置
            this.Size = parent.ClientSize;
            this.Location = parent.PointToScreen(Point.Empty);

            // 跟随父窗体
            parent.LocationChanged += (s, e) => UpdatePosition();
            parent.SizeChanged += (s, e) => UpdateSizeAndPosition();
            parent.FormClosing += (s, e) => this.Close();

            //// 添加关闭或提示内容
            //Label messageLabel = new Label
            //{
            //    Text = "请稍候...",
            //    ForeColor = Color.White,
            //    Font = new Font("微软雅黑", 16),
            //    AutoSize = true,
            //    BackColor = Color.Transparent
            //};

            //this.Load += (s, e) =>
            //{
            //    messageLabel.Location = new Point(
            //        (this.Width - messageLabel.Width) / 2,
            //        (this.Height - messageLabel.Height) / 2
            //    );
            //};

            //this.Controls.Add(messageLabel);
        }

        private void UpdatePosition()
        {
            if (parentForm != null && !parentForm.IsDisposed)
            {
                this.Location = parentForm.PointToScreen(Point.Empty);
            }
        }

        private void UpdateSizeAndPosition()
        {
            if (parentForm != null && !parentForm.IsDisposed)
            {
                this.Size = parentForm.ClientSize;
                UpdatePosition();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UpdatePosition();
        }
    }

}
