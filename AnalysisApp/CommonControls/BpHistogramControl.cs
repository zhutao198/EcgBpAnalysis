using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using AnalysisApp.Commons;
using SkiaSharp;

namespace AnalysisApp.CommonControls
{
    public partial class BpHistogramControl : BaseDrawingControl
    {
        private float PixMM = 3.777f;

        private float PaddingLeft = 0;
        private float PaddingTop = 0;
        private float PaddingRight = 0;
        private float PaddingBottom = 0;

        DrawBpHistogramWave drawBpHistogramWave = null;

        public BpHistogramControl()
        {
            InitializeComponent();

            PaddingLeft = PixMM;
            PaddingTop = PixMM;
            PaddingRight = PixMM;
            PaddingBottom = PixMM;

            drawBpHistogramWave = new DrawBpHistogramWave();
        }

        private void BpHistogramControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                registerEvent();
                initControl();
                initData();
            }
        }

        private void registerEvent()
        {
            this.radioAll.CheckedChanged += RadioButton_CheckedChanged;
            this.radioDay.CheckedChanged += RadioButton_CheckedChanged;
            this.radioNight.CheckedChanged += RadioButton_CheckedChanged;
        }

        private void initControl()
        {
            panel1.Controls.Add(_graphicsControl);
        }

        private void initData()
        {
            bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            drawBpHistogramWave.settingValues = settingValues;
            List<BPDataInfo> bPDataInfoList = CalcBPInfo.GetBPDataList(2);
            DateTime DayTimeStart = DateTime.Parse(settingValues.daytimeStart);
            DateTime DayTimeEnd = DateTime.Parse(settingValues.daytimeEnd);
            if (this.radioAll.Checked)
            {
                drawBpHistogramWave.BPDataInfoList = bPDataInfoList;
            }
            else if(this.radioDay.Checked)
            {
                drawBpHistogramWave.BPDataInfoList = bPDataInfoList.FindAll(b => (CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            }
            else
            {
                drawBpHistogramWave.BPDataInfoList = bPDataInfoList.FindAll(b => (!CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            }
        }

        public void InitRadioCheckStatus()
        {
            this.radioAll.Checked = true;
            UpdateDataRefresh();
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            // 只处理选中状态（取消选中时不处理）
            if (!rb.Checked) return;
            UpdateDataRefresh();
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            canvas.Clear(SKColors.White);
            drawBpHistogramWave.WaveLeft = PaddingLeft;
            drawBpHistogramWave.WaveTop = PaddingTop;
            drawBpHistogramWave.WaveWidth = imageWidth - PaddingLeft - PaddingRight;
            drawBpHistogramWave.WaveHeight = imageHeight - PaddingTop - PaddingBottom;
            drawBpHistogramWave.DrawCoordinate(canvas);
            drawBpHistogramWave.DrawHistogramWave(canvas);
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
    }
}
