using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Zoncare.Common;

namespace AnalysisApp
{
    public partial class WPPageControl : BaseUserControl
    {
        public WPPageControl()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            await base.RefreshData(resetUI);

            if (this.scatterTrend1.IsLoaded) refreshPage(resetUI);
            if (this.drawEcgCommonWave1.IsLoaded)
            {
                this.drawEcgCommonWave1.RefreshData(resetUI);
            }
        }

        private void refreshPage(bool reset = false)
        {
            if (reset)
            {
                this.scatterTrend1.SetBeatType("");

                DateTime startTime = AnaValues.holterConfig.StartDateTime;
                DateTime endTime = startTime.AddHours(1);
                int startSample = AnaValues.CachedAllBeats[0].StartIndex;
                int endSample = startSample + AnaValues.holterConfig.SampleRate*3600;
                List <BeatInfo> selectBeats = new List<BeatInfo>();
                selectBeats.AddRange(AnaValues.CachedAllBeats.Where(b => b.EndIndex < endSample && b.StartIndex >= startSample).ToList());

                this.wpWavePageControl1.SetBeatSource(startTime, endTime, selectBeats);
            }
            else
            {
                DateTime startTime = scatterTrend1.StartTime, endTime = scatterTrend1.EndTime;
                this.scatterTrend1.UpdateBeatSource(this, startTime, endTime, scatterTrend1.BaseBeatType, null);
            }

            this.drawEcgCommonWave1.forceWaveformRedraw();
        }

        private void WPPageControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (!IsUiInitialized) InitUI();

                    Console.WriteLine("瀑布图界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "瀑布图");
            }
        }

        public override void InitUI()
        {
            base.InitUI();
            try
            {
                this.scatterTrend1.SelectMode = 0; //显示24小时心搏

                registerEvents();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "瀑布图");
            }
        }

        private void registerEvents()
        {
            this.scatterTrend1.OnRangeSelected += (sender, startTime, endTime, beats, keepSelection) =>
            {
                this.wpWavePageControl1.SetBeatSource(startTime, endTime, beats, false);
            };

            this.wpWavePageControl1.OnClickSampleChanged += drawEcgCommonWave1.OnWaveformStartTimeChanged;
        }

    }
}
