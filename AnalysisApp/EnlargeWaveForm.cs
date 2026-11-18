using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AnalysisApp
{
    public partial class EnlargeWaveForm : BaseDialogForm
    {
        // 波形时间偏移
        public float waveformTimeOffsetSeconds = 0; // 波形起始时间偏移量（秒）
        public List<int> leadIdxs = new List<int>();
        public float speed = 25;                       //纸速 25mm/s
        public float scale = 5;          //放大倍数

        private const float pixMM = 3.777f;             //每毫米3.777个像素点
        private int LeftMargin { get; set; } = 50;

        public EnlargeWaveForm() : base("", "波形放大")
        {
            InitializeComponent();
            // 设置窗口为全屏模式但不遮挡任务栏
            this.FormBorderStyle = FormBorderStyle.None;
            // 设置窗口大小为工作区大小（不包括任务栏）
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            // 设置窗口位置为屏幕左上角
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;

            this.enlargeWaveControl1.leadIdxs = leadIdxs;
            this.enlargeWaveControl1.waveformTimeOffsetSeconds = waveformTimeOffsetSeconds;
            this.enlargeWaveControl1.speed = speed;
            this.enlargeWaveControl1.scale = scale;
        }

        private void EnlargeWaveForm_Load(object sender, EventArgs e)
        {
            // 添加键盘事件处理程序
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(EnlargeWaveForm_KeyDown);

            enlargeWaveControl1.OnEnlargeWaveStartTimeChanged += smallWaveViewControl1.OnEnlargeWaveformStartTimeChanged;
            smallWaveViewControl1.OnSmallWaveSelectStartTimeChanged += enlargeWaveControl1.OnWaveformStartTimeChanged;
        }


        /// <summary>
        /// 键盘事件处理程序 - 按ESC键退出全屏模式
        /// </summary>
        private void EnlargeWaveForm_KeyDown(object sender, KeyEventArgs e)
        {
            // 按ESC键关闭窗口
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        public void SampleChanged()
        {
            this.enlargeWaveControl1.leadIdxs = leadIdxs;
            this.enlargeWaveControl1.waveformTimeOffsetSeconds = waveformTimeOffsetSeconds;
            this.enlargeWaveControl1.ForceWaveformRedraw();

            if (smallWaveViewControl1 != null && this.panel2.Width > 0)
            {
                int rowSeconds = Convert.ToInt32((this.panel2.Width) / pixMM / speed / scale);
                smallWaveViewControl1.waveformTimeOffsetSeconds = waveformTimeOffsetSeconds;
                smallWaveViewControl1.changeRectPoints(rowSeconds);
            }
        }
    }
}
