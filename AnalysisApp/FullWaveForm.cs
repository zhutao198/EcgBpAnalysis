using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalysisApp
{
    public partial class FullWaveForm : BaseDialogForm
    {
        private EcgSegmentControl _control = null;
        private EventPageControl _Event = null;
        public int _sample = 0;
        public static FullWaveForm Instance { get; private set; }

        public FullWaveForm() : base("", "全览图")
        {
            InitializeComponent();
            // 设置窗口为全屏模式但不遮挡任务栏
            this.FormBorderStyle = FormBorderStyle.None;
            //// 设置窗口大小为工作区大小（不包括任务栏）
            //this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            this.WindowState = FormWindowState.Maximized;
            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            // 设置窗口位置为屏幕左上角
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;

            Instance = this;
        }

        private void FullWaveForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_control != null)
            {
                _control.RefreshData();
            }
            if(_Event != null)
            {
                _Event.UpdateUIAfterLoading(true);
            }
            this.Hide();
            e.Cancel = true;
        }

        private void FullWaveForm_Load(object sender, System.EventArgs e)
        {
            //this.ecgDiagnosisControl1.SampleChanged(Sample);
        }

        public async void Show(int sample, IWin32Window owner = null, Control control = null, Control eventCtl = null)
        {
            _sample = sample;
            
            if (owner != null)
            {
                if (this.Owner != null) Show();
                else Show(owner);
            }
            else
                Show();

            if(control != null)
            {
                _control = control as EcgSegmentControl;
            }
            if(eventCtl != null)
            {
                _Event = eventCtl as EventPageControl;
            }

            while (this.ecgDiagnosisControl1.IsShown)
            {
                await Task.Delay(1);
                //Application.DoEvents();
            }
            this.ecgDiagnosisControl1.SampleChanged(_sample);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Hide();
                return true; // 表示已处理
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
