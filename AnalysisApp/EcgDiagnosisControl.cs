using System;
using System.Threading.Tasks;
using Zoncare.Common;

namespace AnalysisApp
{
    public partial class EcgDiagnosisControl : BaseUserControl
    {
        public EcgDiagnosisControl()
        {
            InitializeComponent();
            if (!DesignMode) this.drawEcgCommonWave1.currentLeadCount = 12;
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            RefreshUI(resetUI);
        }

        private void EcgDiagnosisControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (!IsUiInitialized) InitUI();

                    Console.WriteLine("诊断图界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "诊断图");
            }
        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                if (smallWaveViewControl1 != null && drawEcgCommonWave1 != null)
                {
                    //drawEcgCommonWave1发送事件，smallWaveViewControl1响应事件
                    drawEcgCommonWave1.OnEcgCommonWaveStartTimeChanged += smallWaveViewControl1.OnWaveformStartTimeChanged;
                    drawEcgCommonWave1.OnEcgCommonRowSecondsChanged += smallWaveViewControl1.OnWaveformSpeedChanged;
                    drawEcgCommonWave1.OnDataChanged += (d) => { RefreshUI(false); };

                    smallWaveViewControl1.OnSmallWaveSelectStartTimeChanged += drawEcgCommonWave1.OnSmallWaveStartTimeChanged;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "诊断图");
            }
        }

        private void RefreshUI(bool resetUI)
        {
            if (smallWaveViewControl1 != null && drawEcgCommonWave1 != null)
            {
                if (this.drawEcgCommonWave1.IsLoaded)
                {
                    drawEcgCommonWave1.RefreshData(resetUI);
                }
                //if (resetUI || drawEcgCommonWave1.DataSourceIsChanged)
                //{
                //    drawEcgCommonWave1.RefreshData(resetUI);
                //}
                this.smallWaveViewControl1.changeRectPoints(drawEcgCommonWave1.rowSeconds);
                smallWaveViewControl1.reRendWave();
            }
        }

        public void SampleChanged(int sample)
        {
            if (DesignMode) return;
            this.drawEcgCommonWave1.InitializeContextMenu();
            this.drawEcgCommonWave1.OnWaveformStartTimeChanged(sample);
            this.smallWaveViewControl1.waveformTimeOffsetSeconds = this.drawEcgCommonWave1.waveformTimeOffsetSeconds;
            this.smallWaveViewControl1.changeRectPoints(this.drawEcgCommonWave1.rowSeconds);
            this.smallWaveViewControl1.reRendWave();
        }
    }
}
