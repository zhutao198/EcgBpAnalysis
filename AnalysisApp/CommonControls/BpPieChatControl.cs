using AnalysisApp.Commons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class BpPieChatControl : BaseDrawingControl
    {
        private float PixMM = 3.777f;

        private float PaddingLeft = 0;
        private float PaddingTop = 0;
        private float PaddingRight = 0;
        private float PaddingBottom = 0;

        DrawBpPieChat drawBpPieChat = null;

        bpDiagnoseSettingValues SettingValues;

        private List<int> SbpMaxMin = new List<int>(3);
        private List<Double> SbpMaxMinPercent = new List<Double>(3);

        private List<int> DbpMaxMin = new List<int>(3);
        private List<Double> DbpMaxMinPercent = new List<Double>(3);

        private List<int> PulseMaxMin = new List<int>(3);
        private List<Double> PulseMaxMinPercent = new List<Double>(3);

        public BpPieChatControl()
        {
            InitializeComponent();

            PaddingLeft = PixMM;
            PaddingTop = PixMM;
            PaddingRight = PixMM;
            PaddingBottom = PixMM;

            drawBpPieChat = new DrawBpPieChat();
        }

        private void BpPieChatControl_Load(object sender, EventArgs e)
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
            double minValue = 0;
            double maxValue = 0;


            SettingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            drawBpPieChat.settingValues = SettingValues;

            List<BPDataInfo> bPDataInfoList = CalcBPInfo.GetBPDataList(2);
            DateTime DayTimeStart = DateTime.Parse(SettingValues.daytimeStart);
            DateTime DayTimeEnd = DateTime.Parse(SettingValues.daytimeEnd);

            List<BPDataInfo> dayDataInfoList = bPDataInfoList.FindAll(b => (CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            List<BPDataInfo> nightDataInfoList = bPDataInfoList.FindAll(b => (!CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            if (this.radioAll.Checked)
            {
                List<int> sbpMaxMin = new List<int>(3);
                List<Double> sbpMaxMinPercent = new List<Double>(3);

                List<int> dbpMaxMin = new List<int>(3);
                List<Double> dbpMaxMinPercent = new List<Double>(3);

                List<int> pulseMaxMin = new List<int>(3);
                List<Double> pulseMaxMinPercent = new List<Double>(3);

                drawBpPieChat.BPDataInfoList = bPDataInfoList;
                if (SettingValues.unit == "mmHg")
                {
                    minValue = SettingValues.minWakeSys;
                    maxValue = SettingValues.maxWakeSys;
                }
                else
                {
                    minValue = CommonFuns.mmHg2KPa(SettingValues.minWakeSys, 2);
                    maxValue = CommonFuns.mmHg2KPa(SettingValues.maxWakeSys, 2);
                }
                GetMinMaxCount(dayDataInfoList, minValue, maxValue, 0);
                if (SettingValues.unit == "mmHg")
                {
                    minValue = SettingValues.minWakeDia;
                    maxValue = SettingValues.maxWakeDia;
                }
                else
                {
                    minValue = CommonFuns.mmHg2KPa(SettingValues.minWakeDia, 2);
                    maxValue = CommonFuns.mmHg2KPa(SettingValues.maxWakeDia, 2);
                }
                GetMinMaxCount(dayDataInfoList, minValue, maxValue, 1);

                Double.TryParse(this.txtPulseMin.Text, out minValue);
                Double.TryParse(this.txtPulseMax.Text, out maxValue);
                GetMinMaxCount(bPDataInfoList, minValue, maxValue, 2);

                sbpMaxMin.AddRange(SbpMaxMin);
                dbpMaxMin.AddRange(DbpMaxMin);
                pulseMaxMin.AddRange(PulseMaxMin);
                pulseMaxMinPercent.AddRange(PulseMaxMinPercent);

                if (SettingValues.unit == "mmHg")
                {
                    minValue = SettingValues.minSleepSys;
                    maxValue = SettingValues.maxSleepSys;
                }
                else
                {
                    minValue = CommonFuns.mmHg2KPa(SettingValues.minSleepSys, 2);
                    maxValue = CommonFuns.mmHg2KPa(SettingValues.maxSleepSys, 2);
                }
                GetMinMaxCount(nightDataInfoList, minValue, maxValue, 0);
                if (SettingValues.unit == "mmHg")
                {
                    minValue = SettingValues.minSleepDia;
                    maxValue = SettingValues.maxSleepDia;
                }
                else
                {
                    minValue = CommonFuns.mmHg2KPa(SettingValues.minSleepDia, 2);
                    maxValue = CommonFuns.mmHg2KPa(SettingValues.maxSleepDia, 2);
                }
                GetMinMaxCount(nightDataInfoList, minValue, maxValue, 1);

                for(int i=0; i<3; i++)
                {
                    sbpMaxMin[i] += SbpMaxMin[i];
                    dbpMaxMin[i] += DbpMaxMin[i];

                    sbpMaxMinPercent.Add((double)sbpMaxMin[i] / bPDataInfoList.Count);
                    dbpMaxMinPercent.Add((double)dbpMaxMin[i] / bPDataInfoList.Count);
                }

                drawBpPieChat.sbpMaxMin = sbpMaxMin;
                drawBpPieChat.sbpMaxMinPercent = sbpMaxMinPercent;
                drawBpPieChat.dbpMaxMin = dbpMaxMin;
                drawBpPieChat.dbpMaxMinPercent = dbpMaxMinPercent;
                drawBpPieChat.pulseMaxMin = pulseMaxMin;
                drawBpPieChat.pulseMaxMinPercent = pulseMaxMinPercent;

            }
            else if (this.radioDay.Checked)
            {
                drawBpPieChat.BPDataInfoList = dayDataInfoList;
                Double.TryParse(this.txtSBPMin.Text, out minValue);
                Double.TryParse(this.txtSBPMax.Text, out maxValue);
                GetMinMaxCount(dayDataInfoList, minValue, maxValue, 0);
                Double.TryParse(this.txtDBPMin.Text, out minValue);
                Double.TryParse(this.txtDBPMax.Text, out maxValue);
                GetMinMaxCount(dayDataInfoList, minValue, maxValue, 1);
                Double.TryParse(this.txtPulseMin.Text, out minValue);
                Double.TryParse(this.txtPulseMax.Text, out maxValue);
                GetMinMaxCount(dayDataInfoList, minValue, maxValue, 2);
                drawBpPieChat.sbpMaxMin = SbpMaxMin;
                drawBpPieChat.sbpMaxMinPercent = SbpMaxMinPercent;
                drawBpPieChat.dbpMaxMin = DbpMaxMin;
                drawBpPieChat.dbpMaxMinPercent = DbpMaxMinPercent;
                drawBpPieChat.pulseMaxMin = PulseMaxMin;
                drawBpPieChat.pulseMaxMinPercent = PulseMaxMinPercent;
            }
            else
            {
                drawBpPieChat.BPDataInfoList = nightDataInfoList;
                Double.TryParse(this.txtSBPMin.Text, out minValue);
                Double.TryParse(this.txtSBPMax.Text, out maxValue);
                GetMinMaxCount(nightDataInfoList, minValue, maxValue, 0);
                Double.TryParse(this.txtDBPMin.Text, out minValue);
                Double.TryParse(this.txtDBPMax.Text, out maxValue);
                GetMinMaxCount(nightDataInfoList, minValue, maxValue, 1);
                Double.TryParse(this.txtPulseMin.Text, out minValue);
                Double.TryParse(this.txtPulseMax.Text, out maxValue);
                GetMinMaxCount(nightDataInfoList, minValue, maxValue, 2);
                drawBpPieChat.sbpMaxMin = SbpMaxMin;
                drawBpPieChat.sbpMaxMinPercent = SbpMaxMinPercent;
                drawBpPieChat.dbpMaxMin = DbpMaxMin;
                drawBpPieChat.dbpMaxMinPercent = DbpMaxMinPercent;
                drawBpPieChat.pulseMaxMin = PulseMaxMin;
                drawBpPieChat.pulseMaxMinPercent = PulseMaxMinPercent;
            }

            drawBpPieChat.SBPMinStr = this.txtSBPMin.Text;
            drawBpPieChat.SBPMaxStr = this.txtSBPMax.Text;
            drawBpPieChat.DBPMinStr = this.txtDBPMin.Text;
            drawBpPieChat.DBPMaxStr = this.txtDBPMax.Text;
            drawBpPieChat.PulseMinStr = this.txtPulseMin.Text;
            drawBpPieChat.PulseMaxStr = this.txtPulseMax.Text;
        }

        private void GetMinMaxCount(List<BPDataInfo> dataInfoList, double minValue, double maxValue, int flag)
        {
            if (flag == 0)
            {
                SbpMaxMin.Clear();
                SbpMaxMinPercent.Clear();

                if (SettingValues.unit == "kPa")
                {
                    minValue = CommonFuns.kPa2mmHg(minValue);
                    maxValue = CommonFuns.kPa2mmHg(maxValue);
                }
                int minCount = dataInfoList.Count(x => x.SBP < minValue);
                int middleCount = dataInfoList.Count(x => x.SBP >= minValue && x.SBP < maxValue);
                int maxCount = dataInfoList.Count(x => x.SBP >= maxValue);
                SbpMaxMin.Add(minCount);
                SbpMaxMin.Add(middleCount);
                SbpMaxMin.Add(maxCount);
                SbpMaxMinPercent.Add((double)minCount / dataInfoList.Count);
                SbpMaxMinPercent.Add((double)middleCount / dataInfoList.Count);
                SbpMaxMinPercent.Add((double)maxCount / dataInfoList.Count);
            }
            else if (flag == 1)
            {
                DbpMaxMin.Clear();
                DbpMaxMinPercent.Clear();

                if (SettingValues.unit == "kPa")
                {
                    minValue = CommonFuns.kPa2mmHg(minValue);
                    maxValue = CommonFuns.kPa2mmHg(maxValue);
                }

                int minCount = dataInfoList.Count(x => x.DBP < minValue);
                int middleCount = dataInfoList.Count(x => x.DBP >= minValue && x.DBP < maxValue);
                int maxCount = dataInfoList.Count(x => x.DBP >= maxValue);

                DbpMaxMin.Add(minCount);
                DbpMaxMin.Add(middleCount);
                DbpMaxMin.Add(maxCount);
                DbpMaxMinPercent.Add((double)minCount / dataInfoList.Count);
                DbpMaxMinPercent.Add((double)middleCount / dataInfoList.Count);
                DbpMaxMinPercent.Add((double)maxCount / dataInfoList.Count);
            }
            else
            {
                PulseMaxMin.Clear();
                PulseMaxMinPercent.Clear();

                int minCount = dataInfoList.Count(x => x.Pulse < minValue);
                int middleCount = dataInfoList.Count(x => x.Pulse >= minValue && x.Pulse < maxValue);
                int maxCount = dataInfoList.Count(x => x.Pulse >= maxValue);

                PulseMaxMin.Add(minCount);
                PulseMaxMin.Add(middleCount);
                PulseMaxMin.Add(maxCount);
                PulseMaxMinPercent.Add((double)minCount / dataInfoList.Count);
                PulseMaxMinPercent.Add((double)middleCount / dataInfoList.Count);
                PulseMaxMinPercent.Add((double)maxCount / dataInfoList.Count);
            }
        }

        private void SetMinMaxVisible(bool flag)
        {
            this.txtSBPMin.Visible = flag;
            this.txtSBPMax.Visible = flag;
            this.txtDBPMin.Visible = flag;
            this.txtDBPMax.Visible = flag;
            this.txtPulseMin.Visible = flag;
            this.txtPulseMax.Visible = flag;
            this.label4.Visible = flag;
            this.label6.Visible = flag;
            this.label8.Visible = flag;

            drawBpPieChat.ShowMinMaxValue = flag;
        }

        public void InitRadioCheckStatus()
        {
            bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;

            this.radioAll.Checked = true;
            SetMinMaxVisible(false);

            if (settingValues.unit == "mmHg")
            {
                decimal maxValue = (decimal)280;
                txtSBPMin.Maximum = txtSBPMax.Maximum = txtDBPMin.Maximum = txtDBPMax.Maximum = maxValue;
                txtSBPMin.DecimalPlaces = txtSBPMax.DecimalPlaces = txtDBPMin.DecimalPlaces = txtDBPMax.DecimalPlaces = 0;
                txtSBPMin.Increment = txtSBPMax.Increment = txtDBPMin.Increment = txtDBPMax.Increment = 1;

                this.txtSBPMin.Value = (decimal)settingValues.minValidSys;
                this.txtSBPMax.Value = (decimal)settingValues.maxValidSys;
                this.txtDBPMin.Value = (decimal)settingValues.minValidDia;
                this.txtDBPMax.Value = (decimal)settingValues.maxValidDia;
            }
            else
            {
                decimal maxValue = (decimal)CommonFuns.mmHg2KPa(280, 2);
                txtSBPMin.Maximum = txtSBPMax.Maximum = txtDBPMin.Maximum = txtDBPMax.Maximum = maxValue;
                txtSBPMin.DecimalPlaces = txtSBPMax.DecimalPlaces = txtDBPMin.DecimalPlaces = txtDBPMax.DecimalPlaces = 2;
                txtSBPMin.Increment = txtSBPMax.Increment = txtDBPMin.Increment = txtDBPMax.Increment = 0.01M;

                this.txtSBPMin.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.minValidSys, 2);
                this.txtSBPMax.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.maxValidSys, 2);
                this.txtDBPMin.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.minValidDia, 2);
                this.txtDBPMax.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.maxValidDia, 2);
            }

            this.txtPulseMin.Value = 60;
            this.txtPulseMax.Value = 100;

            UpdateDataRefresh();
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            // 只处理选中状态（取消选中时不处理）
            if (!rb.Checked) return;
            bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            if(settingValues.unit == "mmHg")
            {
                decimal maxValue = (decimal)280;
                txtSBPMin.Maximum = txtSBPMax.Maximum = txtDBPMin.Maximum = txtDBPMax.Maximum = maxValue;
                txtSBPMin.DecimalPlaces = txtSBPMax.DecimalPlaces = txtDBPMin.DecimalPlaces = txtDBPMax.DecimalPlaces = 0;
                txtSBPMin.Increment = txtSBPMax.Increment = txtDBPMin.Increment = txtDBPMax.Increment = 1;
            }
            else
            {
                decimal maxValue = (decimal)CommonFuns.mmHg2KPa(280, 2);
                txtSBPMin.Maximum = txtSBPMax.Maximum = txtDBPMin.Maximum = txtDBPMax.Maximum = maxValue;
                txtSBPMin.DecimalPlaces = txtSBPMax.DecimalPlaces = txtDBPMin.DecimalPlaces = txtDBPMax.DecimalPlaces = 2;
                txtSBPMin.Increment = txtSBPMax.Increment = txtDBPMin.Increment = txtDBPMax.Increment = 0.01M;
            }
            // 更新内容
            switch (rb.Name)
            {
                case "radioAll":
                    SetMinMaxVisible(false);

                    if (settingValues.unit == "mmHg")
                    {
                        this.txtSBPMin.Value = (decimal)settingValues.minValidSys;
                        this.txtSBPMax.Value = (decimal)settingValues.maxValidSys;
                        this.txtDBPMin.Value = (decimal)settingValues.minValidDia;
                        this.txtDBPMax.Value = (decimal)settingValues.maxValidDia;
                    }
                    else
                    {
                        this.txtSBPMin.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.minValidSys, 2);
                        this.txtSBPMax.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.maxValidSys, 2);
                        this.txtDBPMin.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.minValidDia, 2);
                        this.txtDBPMax.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.maxValidDia, 2);
                    }
                    break;
                case "radioDay":
                    SetMinMaxVisible(true);

                    if (settingValues.unit == "mmHg")
                    {
                        this.txtSBPMin.Value = (decimal)settingValues.minWakeSys;
                        this.txtSBPMax.Value = (decimal)settingValues.maxWakeSys;
                        this.txtDBPMin.Value = (decimal)settingValues.minWakeDia;
                        this.txtDBPMax.Value = (decimal)settingValues.maxWakeDia;
                    }
                    else
                    {
                        this.txtSBPMin.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.minWakeSys, 2);
                        this.txtSBPMax.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.maxWakeSys, 2);
                        this.txtDBPMin.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.minWakeDia, 2);
                        this.txtDBPMax.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.maxWakeDia, 2);
                    }
                    break;
                case "radioNight":
                    SetMinMaxVisible(true);

                    if (settingValues.unit == "mmHg")
                    {
                        this.txtSBPMin.Value = (decimal)settingValues.minSleepSys;
                        this.txtSBPMax.Value = (decimal)settingValues.maxSleepSys;
                        this.txtDBPMin.Value = (decimal)settingValues.minSleepDia;
                        this.txtDBPMax.Value = (decimal)settingValues.maxSleepDia;
                    }
                    else
                    {
                        this.txtSBPMin.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.minSleepSys, 2);
                        this.txtSBPMax.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.maxSleepSys, 2);
                        this.txtDBPMin.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.minSleepDia, 2);
                        this.txtDBPMax.Value = (decimal)CommonFuns.mmHg2KPa(settingValues.maxSleepDia, 2);
                    }
                    break;
            }

            this.txtPulseMin.Value = 60;
            this.txtPulseMax.Value = 100;
            UpdateDataRefresh();
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            canvas.Clear(SKColors.White);
            drawBpPieChat.WaveLeft = PaddingLeft;
            drawBpPieChat.WaveTop = PaddingTop;
            drawBpPieChat.WaveWidth = imageWidth - PaddingLeft - PaddingRight;
            drawBpPieChat.WaveHeight = imageHeight - PaddingTop - PaddingBottom;
            drawBpPieChat.DrawCoordinate(canvas);
            drawBpPieChat.DrawPieChat(canvas);
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

        private void txtSBPMin_ValueChanged(object sender, EventArgs e)
        {
            drawBpPieChat.SBPMinStr = this.txtSBPMin.Value.ToString();
            _graphicsControl?.Invalidate();
        }

        private void txtSBPMax_ValueChanged(object sender, EventArgs e)
        {
            drawBpPieChat.SBPMaxStr = this.txtSBPMax.Value.ToString();
            _graphicsControl?.Invalidate();
        }

        private void txtDBPMin_ValueChanged(object sender, EventArgs e)
        {
            drawBpPieChat.DBPMinStr = this.txtDBPMin.Value.ToString();
            _graphicsControl?.Invalidate();
        }

        private void txtDBPMax_ValueChanged(object sender, EventArgs e)
        {
            drawBpPieChat.DBPMaxStr = this.txtDBPMax.Value.ToString();
            _graphicsControl?.Invalidate();
        }

        private void txtPulseMin_ValueChanged(object sender, EventArgs e)
        {
            drawBpPieChat.PulseMinStr = this.txtPulseMin.Value.ToString();
            _graphicsControl?.Invalidate();
        }

        private void txtPulseMax_ValueChanged(object sender, EventArgs e)
        {
            drawBpPieChat.PulseMaxStr = this.txtPulseMax.Value.ToString();
            _graphicsControl?.Invalidate();
        }

        private void BpPieChatControl_SizeChanged(object sender, EventArgs e)
        {
            int baseX = this.panel1.Left;
            int height = this.panel1.Height;
            this.tableLayout1.Top = 24;
            this.tableLayout1.Left = baseX + 6;
            this.tableLayout2.Top = 22 + height / 3;
            this.tableLayout2.Left = baseX + 6;
            this.tableLayout3.Top = 18 + height * 2 / 3;
            this.tableLayout3.Left = baseX + 6;
        }
    }
}
