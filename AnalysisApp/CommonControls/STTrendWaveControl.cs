using AnalysisApp.Commons;
using AnalysisSystem.Base;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalysisApp.CommonControls
{
    public partial class STTrendWaveControl : BaseDrawingControl
    {
        public class MinStxInfo
        {
            public DateTime StartTime { get; set; }
            public double STAmp { get; set; } = 0;
            public double TAmp { get; set; } = 0;
        }

        private List<List<MinStxInfo>> AllLeadStxInfo = new List<List<MinStxInfo>>();
        private int RectPadding { get; set; } = 1;      //每个矩形之间的间隙
        private int WavePadding { get; set; } = 6;
        private int WaveCount { get; set; } = 13;      //波形个数
        private int HRWaveHeight { get; set; } = 80;    //心率趋势图高度
        private int BottomTimeHeight { get; set; } = 10;    //底部时间刻度高度
        private static object _lockObj = new object();

        public string type { get; set; } = "";
        List<float> HRTicks = new List<float>();
        List<float> Ticks = new List<float>();

        private DrawSTTrendWave drawSTTrendWave = new DrawSTTrendWave();
        private bool bMouseDown = false;

        public bool IsActive = true;
        public int ClickMiniute = -1;
        public event Action<float> OnStartTimeChanged;


        public STTrendWaveControl()
        {
            InitializeComponent();
        }

        private void STTrendWaveControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                initControl();

                this.BeginInvoke(new MethodInvoker(() =>
                {
                    initData();
                    reRendWave();
                }));
            }
        }

        public async Task RefreshData()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    initData();
                    reRendWave();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));

            await tcs.Task;
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            // 清除画布
            canvas.Clear(SKColors.Transparent);

            drawSTTrendWave.DrawBackground(canvas, imageWidth, imageHeight);
            drawSTTrendWave.WaveBottomMargin = WavePadding;
            int rectWidth = imageWidth - RectPadding * 2;
            for (int i = 0; i < WaveCount; i++)
            {
                drawSTTrendWave.bLastRect = false;

                SKRect waveRect = new SKRect();
                waveRect.Left = RectPadding;
                waveRect.Right = RectPadding + rectWidth;
                string strLabel = "";
                if (i == 0)
                {
                    int rectHeight = HRWaveHeight;

                    waveRect.Top = RectPadding;
                    waveRect.Bottom = RectPadding + rectHeight;

                    drawSTTrendWave.WaveRect = waveRect;

                    drawSTTrendWave.DrawHRTrend(canvas);

                    drawSTTrendWave.DrawCoordinateSystem(canvas, "bpm", HRTicks);

                    if (ClickMiniute != -1)
                    {
                        drawSTTrendWave.DrawClickLine(canvas, "bpm", ClickMiniute);
                    }
                }
                else
                {
                    int rectHeight = (imageHeight - RectPadding * WaveCount - HRWaveHeight - BottomTimeHeight) / (WaveCount - 1);

                    waveRect.Top = HRWaveHeight + (i - 1) * rectHeight + (i + 1) * RectPadding;

                    if (i == 12)
                    {
                        rectHeight += BottomTimeHeight;
                        drawSTTrendWave.WaveBottomMargin = RectPadding + BottomTimeHeight;
                        drawSTTrendWave.bLastRect = true;
                    }

                    waveRect.Bottom = waveRect.Top + rectHeight;
                    strLabel = AnaValues.LeadIdxToNameMap[i - 1];
                    drawSTTrendWave.WaveRect = waveRect;
                    drawSTTrendWave.DrawRect(canvas);
                    if(AllLeadStxInfo.Count>0 && AllLeadStxInfo[i - 1].Count > 0)
                    {
                        drawSTTrendWave.DrawTTrend(canvas, type, Ticks, AllLeadStxInfo[i - 1]);
                    }
                    drawSTTrendWave.DrawCoordinateSystem(canvas, strLabel, Ticks);
                    if (ClickMiniute != -1)
                    {
                        drawSTTrendWave.DrawClickLine(canvas, type, ClickMiniute);
                    }
                }
            }

        }
        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                bMouseDown = true;
            }
        }
        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (bMouseDown)
            {
                ClickMiniute = CalculateTimeOffsetFromClick(e.X);
                float totalMinutes = (float)AnaValues.holterConfig.Total / AnaValues.holterConfig.SampleRate;
                float clickSecond = Math.Min(ClickMiniute * 60, totalMinutes);
                OnStartTimeChanged?.Invoke(clickSecond);

                bMouseDown = false;

                reRendWave();
            }
        }


        private void initControl()
        {
            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            this.panel1.Controls.Add(_graphicsControl);
            _graphicsControl.Refresh();
        }

        private async void initData()
        {
            ClickMiniute = -1;
            HRTicks.Clear();
            float[] hrTicksArr = { 0, 100, 200 };
            HRTicks.AddRange(hrTicksArr);

            type = "ST";
            updataTicks();

            AllLeadStxInfo.Clear();

            while (AnaCode.IsAnalysisST) { await Task.Delay(10); }

            //if (IsActive)
            //{
            //    Form parentForm = this.FindForm();
            //    AppCode.ShowProcessing(
            //        "",
            //        parentForm,
            //        async (args) =>
            //        {
            //            while (!analysingTask.IsCompleted) { await Task.Delay(10); }
            //        },
            //        this,
            //        new Rectangle(PointToScreen(this.panel1.Location), this.panel1.Size),
            //        true
            //    );
            //}

            //analysingTask.Wait();
            //if (analysingTask.Result == 0)
            //{
            if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
            {
                startTime = DateTime.Today;
            }
            List<BeatInfo> beats = AnaValues.CachedAllBeats;
            BeatInfo lastBeat = beats[beats.Count - 1];
            int minCount = AnaValues.minuteHR.Count;
            for (int i = 0; i < AnaValues.holterConfig.Lead; i++)
            {
                var aecgStxInfo = AEcgHandler.GetStxInfo(i);
                if(aecgStxInfo==null || aecgStxInfo.Count == 0)
                {
                    continue;
                }
                List<short> stBeatPosList = new List<short>();
                List<short> tBeatPosList = new List<short>();
                int beatIdx = 0;
                List<MinStxInfo> leadStxInfo = new List<MinStxInfo>();
                for (int j = 0; j < minCount; j++)
                {
                    int startSample = AnaCode.getSampleFromDateTime(startTime.AddSeconds(j * 60));
                    int endSample = AnaCode.getSampleFromDateTime(startTime.AddSeconds((j + 1) * 60));
                    if (startSample > lastBeat.StartIndex)
                    {
                        MinStxInfo minStxInfo = new MinStxInfo();
                        minStxInfo.StartTime = startTime.AddSeconds(j * 60);
                        minStxInfo.STAmp = 0;
                        minStxInfo.TAmp = 0;
                        leadStxInfo.Add(minStxInfo);
                        continue;
                    }

                    int stAmpCount = 0;
                    int stAmpTotal = 0;
                    int tAmpTotal = 0;
                    for (int p = beatIdx; p < aecgStxInfo.Count; p++)
                    {
                        if (aecgStxInfo[p].stBeatPos >= startSample && aecgStxInfo[p].stBeatPos < endSample)
                        {
                            stBeatPosList.Add(aecgStxInfo[p].stAmp);
                            tBeatPosList.Add(aecgStxInfo[p].tWaveAmp);

                            stAmpTotal += aecgStxInfo[p].stAmp;
                            tAmpTotal += aecgStxInfo[p].tWaveAmp;
                            ++stAmpCount;
                        }
                        else
                        {
                            if (stAmpCount != 0)
                            {
                                beatIdx = p;
                            }

                            break;
                        }
                    }

                    if (stAmpCount != 0)
                    {
                        MinStxInfo minStxInfo = new MinStxInfo();
                        minStxInfo.StartTime = startTime.AddSeconds(j * 60);

                        minStxInfo.STAmp = (double)stAmpTotal / stAmpCount;
                        minStxInfo.TAmp = (double)tAmpTotal / stAmpCount;
                        //minStxInfo.STAmp = AnaCode.Median(stBeatPosList);
                        //minStxInfo.TAmp = AnaCode.Median(tBeatPosList);

                        leadStxInfo.Add(minStxInfo);
                    }
                    else
                    {
                        MinStxInfo minStxInfo = new MinStxInfo();
                        minStxInfo.StartTime = startTime.AddSeconds(j * 60);

                        minStxInfo.STAmp = 0;
                        minStxInfo.TAmp = 0;

                        leadStxInfo.Add(minStxInfo);
                    }

                }

                AllLeadStxInfo.Add(leadStxInfo);
            }
            //}
            drawSTTrendWave.WaveLeftMargin = WavePadding * 6;
            drawSTTrendWave.WaveTopMargin = WavePadding;
            drawSTTrendWave.WaveRightMargin = WavePadding * 2.5f;
        }

        /// <summary>
        /// 计算点击位置对应的时间偏移（秒）
        /// </summary>
        private int CalculateTimeOffsetFromClick(float x)
        {
            if (drawSTTrendWave == null) return 0;
            // 计算点击位置对应的时间（分钟）
            float plotWidth = drawSTTrendWave.WaveRect.Width - drawSTTrendWave.WaveLeftMargin - drawSTTrendWave.WaveRightMargin;
            float relativeX = x - drawSTTrendWave.WaveLeftMargin;

            if (relativeX < 0) relativeX = 0;
            if (relativeX > plotWidth) relativeX = plotWidth;

            int clickMiniute = Convert.ToInt32((float)(24 * 60 / plotWidth) * relativeX);
            int totalMinutes = (int)Math.Ceiling((double)AnaValues.holterConfig.Total / AnaValues.holterConfig.SampleRate / 60);

            clickMiniute = Math.Min(clickMiniute, totalMinutes);
            return clickMiniute;
        }

        public void updataTicks()
        {
            Ticks.Clear();
            if (type == "ST")
            {
                float[] tickArr = { -0.5f, -0.4f, -0.3f, -0.2f, -0.1f, 0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };
                Ticks.AddRange(tickArr);
            }
            else if (type == "T")
            {
                float[] tickArr = { -1f, -0.8f, -0.6f, -0.4f, -0.2f, 0, 0.2f, 0.4f, 0.6f, 0.8f, 1f };
                Ticks.AddRange(tickArr);
            }
        }

        public void reRendWave()
        {
            if (_graphicsControl != null)
            {
                _graphicsControl?.Refresh();
            }
        }
    }
}
