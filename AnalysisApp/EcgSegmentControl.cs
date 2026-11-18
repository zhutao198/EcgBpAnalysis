using System;
using System.Threading.Tasks;
using Zoncare.Common;

namespace AnalysisApp
{
    public partial class EcgSegmentControl : BaseUserControl
    {
        public EcgSegmentControl()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            this.segmentChartControl1.RefreshDataAndDisplay();
            this.segmentListControl1.initData();
            this.segmentWaveViewControl1.ForceWaveformRedraw();
        }

        private void EcgSegmentControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (!IsUiInitialized) InitUI();

                    Console.WriteLine("片段图界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "片段图");
            }
        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                if (this.segmentChartControl1 != null && this.segmentWaveViewControl1 != null && this.segmentWaveViewControl1 != null)
                {
                    this.segmentWaveViewControl1.OnDeleteSegment += this.segmentChartControl1.OnDeleteSegment;
                    this.segmentWaveViewControl1.OnDeleteSegment += this.segmentListControl1.OnControlDeleteSegment;
                    this.segmentListControl1.OnModifySegment += this.segmentChartControl1.OnDeleteSegment;
                    this.segmentChartControl1.OnSegmentTimeChanged += this.segmentWaveViewControl1.OnSegmentTimeChanged;
                    this.segmentChartControl1.OnChartDeleteSegment += this.segmentListControl1.OnControlDeleteSegment;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "片段图");
            }
        }
    }
}
