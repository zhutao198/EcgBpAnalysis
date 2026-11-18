using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnalysisApp.Commons;
using Zoncare.Common;
using SkiaSharp;

namespace AnalysisApp.CommonControls
{
    public partial class BpFitPlotControl : BaseDrawingControl
    {
        DrawBpFitPlot drawBpFitPlot = null;
        private float PixMM = 3.777f;

        public BpFitPlotControl()
        {
            InitializeComponent();
            drawBpFitPlot = new DrawBpFitPlot();
        }

        private void BpFitPlotControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                initControl();
                initData();
            }
        }

        public void UpdateDataRefresh()
        {
            initData();
            RefreshWave();
        }

        public void RefreshWave()
        {
            _graphicsControl?.Invalidate();
        }

        private void initControl()
        {
            panel1.Controls.Add(_graphicsControl);
        }

        private void initData()
        {
            bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            drawBpFitPlot.settingValues = settingValues;

            List<BPDataInfo> bPDataInfoList = CalcBPInfo.GetBPDataList(2);
            int allDataCount = bPDataInfoList.Count;
            List<double> allSbpList = new List<double>(allDataCount);
            List<double> allDbpList = new List<double>(allDataCount);
            List<int> allPulseList = new List<int>(allDataCount);
            for (int i = 0; i < allDataCount; i++)
            {
                allSbpList.Add(bPDataInfoList[i].SBP);
                allDbpList.Add(bPDataInfoList[i].DBP);
                allPulseList.Add(bPDataInfoList[i].Pulse);
            }

            DateTime DayTimeStart = DateTime.Parse(settingValues.daytimeStart);
            DateTime DayTimeEnd = DateTime.Parse(settingValues.daytimeEnd);
            List<BPDataInfo> dayBPDataInfoList = bPDataInfoList.FindAll(b => (CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            List<BPDataInfo> nightBPDataInfoList = bPDataInfoList.FindAll(b => (!CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));

            int dayDataCount = dayBPDataInfoList.Count;
            List<double> daySbpList = new List<double>(dayDataCount);
            List<double> dayDbpList = new List<double>(dayDataCount);
            List<int> dayPulseList = new List<int>(dayDataCount);
            for (int i = 0; i < dayDataCount; i++)
            {
                daySbpList.Add(dayBPDataInfoList[i].SBP);
                dayDbpList.Add(dayBPDataInfoList[i].DBP);
                dayPulseList.Add(dayBPDataInfoList[i].Pulse);
            }

            int nightDataCount = nightBPDataInfoList.Count;
            List<double> nightSbpList = new List<double>(nightDataCount);
            List<double> nightDbpList = new List<double>(nightDataCount);
            List<int> nightPulseList = new List<int>(nightDataCount);
            for (int i = 0; i < nightDataCount; i++)
            {
                nightSbpList.Add(nightBPDataInfoList[i].SBP);
                nightDbpList.Add(nightBPDataInfoList[i].DBP);
                nightPulseList.Add(nightBPDataInfoList[i].Pulse);
            }

            int dataCount = bPDataInfoList.Count;
            List<double> sbpList = new List<double>(dataCount);
            List<double> dbpList = new List<double>(dataCount);
            List<double> pulseList = new List<double>(dataCount);
            for (int i=0; i<dataCount; i++)
            {
                sbpList.Add(bPDataInfoList[i].SBP);
                dbpList.Add(bPDataInfoList[i].DBP);
                pulseList.Add(bPDataInfoList[i].Pulse);
            }
            List<SKPoint> sbpSlopePoints = new List<SKPoint>(2);
            double y0 = CalcBPInfo.GetY0(sbpList, dbpList, 0);
            double y1 = CalcBPInfo.GetY1(sbpList, dbpList, 0, y0);
            sbpSlopePoints.Add(CalcBPInfo.GetSlopePoint1(y0, y1));
            sbpSlopePoints.Add(CalcBPInfo.GetSlopePoint2(y0, y1));

            List<SKPoint> pulseSlopePoints = new List<SKPoint>(2);
            double pulseY0 = CalcBPInfo.GetY0(sbpList, pulseList, 0);
            double pulseY1 = CalcBPInfo.GetY1(sbpList, pulseList, 0, pulseY0);
            pulseSlopePoints.Add(CalcBPInfo.GetSlopePoint1(pulseY0, pulseY1));
            pulseSlopePoints.Add(CalcBPInfo.GetSlopePoint2(pulseY0, pulseY1));

            drawBpFitPlot.DaySbpList.Clear();
            drawBpFitPlot.DayDbpList.Clear();
            drawBpFitPlot.DayPulseList.Clear();
            drawBpFitPlot.NightSbpList.Clear();
            drawBpFitPlot.NightDbpList.Clear();
            drawBpFitPlot.NightPulseList.Clear();
            drawBpFitPlot.SbpSlopePoints.Clear();
            drawBpFitPlot.PulseSlopePoints.Clear();

            drawBpFitPlot.DaySbpList.AddRange(daySbpList);
            drawBpFitPlot.DayDbpList.AddRange(dayDbpList);
            drawBpFitPlot.DayPulseList.AddRange(dayPulseList);
            drawBpFitPlot.NightSbpList.AddRange(nightSbpList);
            drawBpFitPlot.NightDbpList.AddRange(nightDbpList);
            drawBpFitPlot.NightPulseList.AddRange(nightPulseList);
            drawBpFitPlot.SbpSlopePoints.AddRange(sbpSlopePoints);
            drawBpFitPlot.PulseSlopePoints.AddRange(pulseSlopePoints);
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            canvas.Clear(SKColors.White);
            drawBpFitPlot.WaveWidth = imageWidth;
            drawBpFitPlot.WaveHeight = imageHeight;
            drawBpFitPlot.CoorPaddingLeft = imageWidth / 4;
            drawBpFitPlot.CoorPaddingRight = imageWidth / 4;
            drawBpFitPlot.CoorPaddingTop = 10 * PixMM;
            drawBpFitPlot.CoorPaddingBottom = 5 * PixMM;
            drawBpFitPlot.DrawCoordinate(canvas);
            drawBpFitPlot.DrawFitPlot(canvas);
        }
    }
}
