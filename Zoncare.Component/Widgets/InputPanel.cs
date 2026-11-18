using System;
using System.Drawing;
using System.Windows.Forms;

namespace Zoncare.Component.Widgets
{
    public partial class InputPanel : UserControl
    {
        private const int WM_NCLBUTTONDOWN = 0x00A1; //当光标在窗口的非客户区并按下鼠标左键时发送此消息
        private const int WM_NCLBUTTONDBLCLK = 0x00A3; //当光标在窗口的非客户区并双击鼠标左键时发送此消息
        private const int HT_CAPTION = 0x2;
        private Form _owner;

        public InputPanel()
        {
            InitializeComponent();
        }

        public InputPanel(string tip, string title)
        {
            InitializeComponent();

            this.lblTip.Text = tip;
            this.lblTitle.Text = title;
        }

        public void ShowCentrally(Form owner)
        {
            _owner = owner;
            Left = (_owner.Width - Width) / 2;
            Top = (_owner.Height - Height) / 2;
            owner.Controls.Add(this);
            this.BringToFront();
            this.Show();
        }

        public void Close()
        {
            this.Hide();
            _owner.Controls.Remove(this);
            _owner.Refresh();
        }

        private void InputPanel_Load(object sender, EventArgs e)
        {
            if (DesignMode) { return; }

            this.lblTip.MouseDown += tlpTop_MouseDown;
            this.lblTitle.MouseDown += tlpTop_MouseDown;

            this.picClose.Click += (o, args) =>
            {
                Close();
            };
            this.picClose.MouseEnter += (o, args) => { this.picClose.BackColor = Color.FromArgb(200, 232, 35, 35); };
            this.picClose.MouseLeave += (o, args) => { this.picClose.BackColor = this.tlpTop.BackColor; };
        }

        private void InputPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Silver,
               new Rectangle(base.ClientRectangle.Location.X, base.ClientRectangle.Location.Y, base.ClientRectangle.Width - 1, base.ClientRectangle.Height - 1));
        }

        private void tlpTop_MouseDown(object sender, MouseEventArgs e)
        {
            (sender as Control).Capture = false;

            //发送点击窗体边框的windows消息
            Message msg = Message.Create(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            base.WndProc(ref msg);
        }

    }
}
