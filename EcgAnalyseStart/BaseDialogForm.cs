using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    public partial class BaseDialogForm0 : Form
    {
        private const int WM_NCLBUTTONDOWN = 0x00A1; //当光标在窗口的非客户区并按下鼠标左键时发送此消息
        private const int WM_NCLBUTTONDBLCLK = 0x00A3; //当光标在窗口的非客户区并双击鼠标左键时发送此消息
        private const int HT_CAPTION = 0x2;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SIZE = 0xF000;
        private const int RIGHTBOTTOM = 0x0008;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_MINIMIZEBOX = 0x00020000;

        private bool _allowMaxsize = false; //是否允许最大化及还原
        private bool _isShown = false;
        private FormWindowState _originalState;

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hMenu, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        public IntPtr BodyHandle { get { return this.pnlBody.Handle; } }

        public BaseDialogForm0()
        {
            InitializeComponent();

            this.ControlBox = false;
            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;

            _allowMaxsize = false;
        }

        private void BaseDialogForm_Load(object sender, EventArgs e)
        {
            string iconPath = PubFuns.GetBaseDirectory() + @"\images\popup.ico";
            this.Icon = new Icon(iconPath);
            //string mainIconPath = Application.StartupPath + @"\images\icon\ecgstation.ico";

            //if (this.WindowState != FormWindowState.Maximized && this.Name != "LoginForm" && this.Name != "UserManagement" && this.Name != "SoftwareSettings")
            //{
            //    if (File.Exists(iconPath)) { this.Icon = new Icon(iconPath); }
            //}
            //else
            //{
            //    if (File.Exists(mainIconPath)) { this.Icon = new Icon(mainIconPath); }
            //}

            this.DoubleBuffered = true;

            //disableMaxControlBox();
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

            this.Invalidate();
        }

        private void BaseDialogForm_Shown(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                _originalState = this.WindowState;
                _isShown = true;

                ResetWindowState();
            }
        }

        private void picMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void disableMaxControlBox()
        {
            if (DesignMode) return;

            if (_allowMaxsize == false && this.FormBorderStyle != FormBorderStyle.None)
            {
                //窗口最大化而最大化按钮禁用时, 任务栏会被遮挡
                this.MaximizeBox = true;

                //禁用最大化按钮
                int GWL_STYLE = -16;
                int nStyle = GetWindowLong(this.Handle, GWL_STYLE);
                nStyle &= ~(WS_MAXIMIZEBOX); //禁用最大化按钮 (启用: nStyle |= WS_MAXIMIZEBOX)

                SetWindowLong(this.Handle, GWL_STYLE, nStyle);
                SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0020); //禁用后进行界面的刷新，否则禁用状态不会立即显示在界面上
            }
        }

        public void SetTitle(string title)
        {
            this.lblTitle.Text = title;
        }

        protected void ResetWindowState()
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
                if (this.WindowState == FormWindowState.Maximized && this.FormBorderStyle != FormBorderStyle.None)
                {
                    if (m.Msg == 0x128) return; //禁用WM_UPDATEUISTATE消息, 避免窗口切换时清除绘制的图面

                    if (m.Msg == 0x112)
                    {
                        switch ((int)m.WParam)
                        {
                            //禁止拖拽标题栏还原窗体
                            case 0xF012:
                            case 0xF010:
                                if (_allowMaxsize == false) m.WParam = IntPtr.Zero;
                                break;
                        }
                    }
                    else if (m.Msg == WM_NCLBUTTONDBLCLK)
                    {
                        if (_allowMaxsize == false) m.WParam = IntPtr.Zero;
                    }
                }
            }

            base.WndProc(ref m);
        }

    }
}
