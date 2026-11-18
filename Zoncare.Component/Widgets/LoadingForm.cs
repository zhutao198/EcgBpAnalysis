using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Utilities;

namespace Zoncare.Component.Widgets
{
    public partial class LoadingForm : Form
    {
        private Func<object, Task> workAction = null;
        private object workActionArg = null;
        private Thread workThread = null;
        private bool isFirst = true;
        private Image backgroundImage;

        public Rectangle FormRectangle { get; set; }
        public bool ShowGifOnly { get; set; }
        public Exception WorkException { get; private set; }
        public bool WorkCompleted = false;
        public string Message
        {
            get
            {
                return lbMessage.Text;
            }
            set
            {
                lbMessage.Text = value;

                if (!isFirst)
                {
                    //面板可能根据提示文本长度自动调整, 需重置面板位置
                    if (!string.IsNullOrEmpty(lbMessage.Text))
                    {
                        this.panelContent.MinimumSize = new Size(174, 0);
                    }
                    else
                    {
                        this.panelContent.MinimumSize = new Size(
                            pictureBox1.Width + pictureBox1.Margin.Left + pictureBox1.Margin.Right,
                            pictureBox1.Height + pictureBox1.Margin.Top + pictureBox1.Margin.Bottom);
                    }
                    //this.panelContent.Left = (this.Width - this.panelContent.Width) / 2;
                    //this.panelContent.Top = (this.Height - this.panelContent.Height) / 2;
                }
            }
        }

        public void SetWorkAction(Func<object, Task> workAction, object arg)
        {
            this.workAction = workAction;
            this.workActionArg = arg;
        }

        public LoadingForm(string msg)
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            this.Message = msg;
        }

        private void LoadingForm_Load(object sender, EventArgs e)
        {
            if (this.Owner != null)
            {
                this.StartPosition = FormStartPosition.Manual;
                Rectangle displayRect = this.Owner.ClientRectangle;
                if (FormRectangle != Rectangle.Empty) displayRect = FormRectangle;
                this.Size = displayRect.Size;
                this.Location = displayRect.Location;
            }
            else
            {
                this.StartPosition = FormStartPosition.CenterScreen;
                Rectangle screenRect = Screen.PrimaryScreen.WorkingArea;
                this.Size = new Size(screenRect.Width, screenRect.Height);
                //this.Location = new Point((screenRect.Width - this.Width) / 2, (screenRect.Height - this.Height) / 2);
            }

            this.panelContent.Left = (this.Width - this.panelContent.Width) / 2;
            this.panelContent.Top = (this.Height - this.panelContent.Height) / 2;

            if (ShowGifOnly)
            {
                this.panelContent.BackColor = SystemColors.Control;
                this.panelContent.Top = this.Height / 4;
            }
            else
            {
                //设置半透明背景
                using (Bitmap bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(this.PointToScreen(new Point(0, 0)), new Point(0, 0), this.ClientSize);
                    }
                    backgroundImage = PubFuns.GetTransparentImage(bmp, 0.5f);
                }
            }

            setLoadingImage(this.pictureBox1);
            this.BringToFront();
        }

        private void LoadingForm_Paint(object sender, PaintEventArgs e)
        {
            if (backgroundImage != null)
            {
                e.Graphics.DrawImage(backgroundImage, 0, 0);
            }
        }

        private void LoadingForm_Shown(object sender, EventArgs e)
        {
            if (this.workAction != null)
            {
                workThread = new Thread(ExecWorkAction)
                {
                    CurrentCulture = Thread.CurrentThread.CurrentCulture,
                    CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                };
                workThread.IsBackground = true;
                workThread.Start();
            }
            isFirst = false;
        }

        private void LoadingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭前强制主窗体重绘（避免黑色残影）
            //if (this.Owner != null)
            //{
            //    this.Owner.Invalidate();
            //    this.Owner.Update();
            //}
        }


        private async void ExecWorkAction()
        {
            try
            {
                //Task workTask = new Task(
                //    (arg) =>
                //    {
                //        this.workAction(arg);
                //    },
                //    this.workActionArg);
                //workTask.Start();

                Task workTask = Task.Run(async () =>
                {
                    Thread.CurrentThread.CurrentCulture = this.workThread.CurrentCulture;
                    Thread.CurrentThread.CurrentUICulture = this.workThread.CurrentUICulture;
                    await workAction(workActionArg);
                });

                await Task.Delay(50);
                await workTask.ContinueWith(t => { closeLoadingForm(); });
            }
            catch (Exception ex)
            {
                this.WorkException = ex;
                closeLoadingForm();
            }
            finally
            {
                this.WorkCompleted = true;
            }
        }

        private void closeLoadingForm()
        {
            if (this.IsHandleCreated && this.InvokeRequired)
            {
                this.Invoke(new Action(() => { if (this.Created) this.Close(); }));
            }
            else
                return;
        }

        private void setLoadingImage(PictureBox picBox)
        {
            Bitmap imgResource = Properties.Resources.download;

            MemoryStream ms = new MemoryStream();
            imgResource.Save(ms, ImageFormat.Gif);
            Image img = Image.FromStream(ms);
            picBox.Image = img;
            imgResource.Dispose();
        }

    }
}
