using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Zoncare.Component.Widgets
{
    public partial class SplashForm : Form
    {
        //Delegate for cross thread call to close
        private delegate void CloseDelegate();

        private static SplashForm splashForm;
        private static IWin32Window ownerForm;

        public SplashForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 按指定方式显示一个浮层(窗口)
        /// </summary>
        /// <param name="showAsLoadingLayer">是否显示为顶层窗口(模态)</param>
        /// <param name="owner">拥有本浮层的父窗口</param>
        public static void ShowSplashScreen(bool showAsLoadingLayer = false, IWin32Window owner = null)
        {
            // Make sure it is only launched once.    
            if (splashForm != null) return;

            splashForm = new SplashForm();
            ownerForm = owner;
            Thread thread = new Thread(new ThreadStart(delegate { showForm(showAsLoadingLayer); }));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            do
            {
                Application.DoEvents();
            }
            while (!splashForm.IsHandleCreated);
        }

        public static void CloseForm()
        {
            if (splashForm != null && splashForm.IsHandleCreated)
            {
                splashForm.Invoke(new CloseDelegate(CloseFormInternal));
            }
            if (ownerForm != null)
            {
                Thread.Sleep(50);
                Control owner = ownerForm as Control;
                if (owner.Visible) owner.Focus();
            }
        }

        private static void showForm(bool showAsModal)
        {
            if (splashForm != null)
            {
                setLoadingImage(splashForm.pictureBox1);
                if (showAsModal)
                {
                    splashForm.ShowDialog();
                }
                else
                {
                    Application.Run(splashForm);
                    if (splashForm != null && splashForm.IsHandleCreated) splashForm.Focus();
                }
            }
        }

        private static void CloseFormInternal()
        {
            splashForm.Close();
            splashForm.Dispose();
            splashForm = null;
        }

        private static void setLoadingImage(PictureBox picBox)
        {
            Bitmap imgResource = Properties.Resources.loading;

            MemoryStream ms = new MemoryStream();
            imgResource.Save(ms, ImageFormat.Gif);
            Image img = Image.FromStream(ms);
            picBox.Image = img;
            imgResource.Dispose();
        }
    }
}
