using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Utilities;

namespace Zoncare.Component.Widgets
{
    public partial class ProgressForm : Form
    {
        private Func<object, Task> workAction = null;
        private object workActionArg = null;
        private Thread workThread = null;
        private Image backgroundImage;

        public Rectangle FormRectangle { get; set; }
        public Exception WorkException { get; private set; }
        public bool WorkCompleted = false;

        public bool UseMarqueeStyle
        {
            get => _useMarqueeStyle;
            set
            {
                _useMarqueeStyle = value;
                if (value == true)
                    this.progressBar.Style = ProgressBarStyle.Marquee;
                else
                    this.progressBar.Style = ProgressBarStyle.Blocks;
            }
        }
        private bool _useMarqueeStyle;

        /// <summary>
        /// 当前进度描述信息
        /// </summary>
        public string Message
        {
            get
            {
                return lbMessage.Text;
            }
            set
            {
                lbMessage.Text = value;
            }
        }

        /// <summary>
        /// 进度的值(0~100)
        /// </summary>
        public int Progress
        {
            get
            {
                return progressBar.Value;
            }
            set
            {
                if (0 <= value && value <= 100)
                {
                    progressBar.Value = value;
                    this.lblPercent.Text = $"{value}%";
                }
            }
        }

        public void SetWorkAction(Func<object, Task> workAction, object arg)
        {
            this.workAction = workAction;
            this.workActionArg = arg;
        }

        public ProgressForm(string msg)
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
                this.Owner.Activate();
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
            }
            this.panelContent.Width = this.Width * 4 / 10;
            this.panelContent.Left = (this.Width - this.panelContent.Width) / 2;
            this.panelContent.Top = (this.Height - this.panelContent.Height) / 2;

            //设置半透明背景
            using (Bitmap bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(this.PointToScreen(new Point(0, 0)), new Point(0, 0), this.ClientSize);
                }
                backgroundImage = PubFuns.GetTransparentImage(bmp, 0.5f);
            }

            this.BringToFront();

            this.VisibleChanged += (o, arg) => { };
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

    }
}
