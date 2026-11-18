using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    public partial class SimplePopup : GroupBox
    {
        [Browsable(false)]
        public DialogResult WinResult { get; set; }

        public Action<DialogResult> OpCallBack;

        public SimplePopup()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            PictureBox picOK = new PictureBox() { Image = Properties.Resources.ok16, Width = 16, Height = 16 };
            PictureBox picCancel = new PictureBox() { Image = Properties.Resources.cancel16, Width = 16, Height = 16 };
            this.Controls.Add(picOK);
            this.Controls.Add(picCancel);

            picCancel.Location = new Point(this.Width - picCancel.Width, 0);
            picOK.Location = new Point(picCancel.Left - picOK.Width - 4, 0);

            picOK.Click += pictureBox1_Click;
            picCancel.Click += pictureBox2_Click;
            picOK.Paint += PictureBox_Paint;
            picCancel.Paint += PictureBox_Paint;
            picOK.MouseEnter += PictureBox_MouseEnter;
            picCancel.MouseEnter += PictureBox_MouseEnter;
            picOK.MouseLeave += PictureBox_MouseLeave;
            picCancel.MouseLeave += PictureBox_MouseLeave;
        }

        private void PictureBox_MouseEnter(object sender, EventArgs e)
        {
            PictureBox picBox = sender as PictureBox;
            picBox.BackColor = SystemColors.ScrollBar; //SystemColors.ActiveBorder;
        }

        private void PictureBox_MouseLeave(object sender, EventArgs e)
        {
            PictureBox picBox = sender as PictureBox;
            picBox.BackColor = SystemColors.Control;
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            using (Pen dashPen = new Pen(Color.DarkGray, 1))
            {
                dashPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                // 绘制虚线边框
                e.Graphics.DrawRectangle(dashPen, 0, 0, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            WinResult = DialogResult.OK;
            OpCallBack(WinResult);
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            WinResult = DialogResult.Cancel;
            OpCallBack(WinResult);
            this.Hide();
        }

    }
}
