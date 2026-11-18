using AnalysisApp.Commons;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;

namespace AnalysisApp.CommonControls
{
    public partial class EcgDataSummaryControl : UserControl
    {
        public static string SummaryFilePath = "";

        private bool _isLoadingData = false;

        private string DateFormat = "";

        public EcgDataSummaryControl()
        {
            InitializeComponent();
        }

        private void initControl()
        {
            bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;
            if (bPace)
            {
                this.lblTitle.Text = "起搏";
                this.lblLeft1.Text = "起搏总数(次)";
                this.lblLeft2.Text = "房性起搏(次)";
                this.lblLeft3.Text = "室性起搏(次)";
                this.lblLeft4.Text = "房室顺序起搏(次)";
                this.lblLeft5.Visible = false;
                this.lblLeft6.Visible = false;
                this.tbxpNN50.Visible = false;
                this.tbxTriindex.Visible = false;
                this.lblRight1.Text = "占比(%)";
                this.lblRight2.Text = "占比(%)";
                this.lblRight3.Text = "占比(%)";
                this.lblRight4.Text = "占比(%)";
                this.lblRight4.Visible = true;
                this.lblRight5.Visible = false;
                this.lblRight6.Visible = false;
                this.tbxTP.Visible = true;
                this.tbxULF.Visible = false;
                this.tbxVLF.Visible = false;
            }
            else
            {
                this.lblTitle.Text = "HRV";
                this.lblLeft1.Text = "SDNN(ms)";
                this.lblLeft2.Text = "SDANN(ms)";
                this.lblLeft3.Text = "SDNN Index(ms)";
                this.lblLeft4.Text = "rMSSD(ms)";
                this.lblLeft5.Text = "pNN50(%)";
                this.lblLeft6.Text = "Tri.index";
                this.lblLeft5.Visible = true;
                this.lblLeft6.Visible = true;
                this.tbxpNN50.Visible = true;
                this.tbxTriindex.Visible = true;
                this.lblRight1.Text = "LF(ms²)";
                this.lblRight2.Text = "HF(ms²)";
                this.lblRight3.Text = "LF/HF";
                this.lblRight4.Text = "TP(ms²)";
                this.lblRight5.Text = "ULF(ms²)";
                this.lblRight6.Text = "VLF(ms²)";

                this.lblRight4.Visible = false;
                this.lblRight5.Visible = false;
                this.lblRight6.Visible = false;
                this.tbxTP.Visible = false;
                this.tbxULF.Visible = false;
                this.tbxVLF.Visible = false;
            }
            DateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
            List<DateTimePicker> dataTimeList = CommonFuns.RetriveSubControls<DateTimePicker>(this.tableLayoutPanel2);
            foreach (DateTimePicker dataTime in dataTimeList)
            {
                dataTime.CustomFormat = DateFormat;
            }

            fillLeadComboBox(this.lowestLead);
            fillLeadComboBox(this.upestLead);
        }

        private void fillLeadComboBox(ComboBox cbx)
        {
            cbx.ValueMember = "Value";
            cbx.DisplayMember = "Text";

            List<ListItem> items = new List<ListItem>();
            string leads = SysConstants.ECG_LT_STANDARD[1];
            if (AnaValues.holterConfig.Lead == 3) leads = SysConstants.ECG_LT_STANDARD[5];
            string[] arrLead = leads.Split(',');
            for (int i = 0; i < arrLead.Length; i++)
            {
                items.Add(new ListItem(i, arrLead[i]));
            };
            cbx.DataSource = items;
            cbx.SelectedValue = AnaValues.holterConfig.MainChannel;
        }

        public async void initDataAsync()
        {
            initControl();
            if (_isLoadingData) return;

            _isLoadingData = true;
            try
            {
                if (!string.IsNullOrEmpty(AnaValues.RootPath))
                {
                    SummaryFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SummaryFileName);
                }
                if (!File.Exists(SummaryFilePath))
                {
                    while (AnaEvent.IsCalculating)
                    {
                        await Task.Delay(10);
                    }

                    CalcData();
                }
                else
                {
                    ReadData();
                }
            }
            finally
            {
                _isLoadingData = false;
            }
        }

        public static DateTime? ConvertToDateTime(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;

            string[] formats = {
                "dd-MM-yyyy HH:mm:ss",
                "MM-dd-yyyy HH:mm:ss"
            };

            if (DateTime.TryParseExact(
                dateString,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }

            return null;
        }

        private void CalcData()
        {
            AnaEvent.CalcAvgHR(AnaValues.CachedAllBeats);
            this.tbxTotalBeats.Text = AnaEvent.TotalBeatCount.ToString();
            this.tbxAvgHR.Text = AnaEvent.TotalAvgHR.ToString();
            this.tbxMaxHR.Text = AnaEvent.AllEvents.ContainsKey("最快心率") && AnaEvent.AllEvents["最快心率"].Count > 0 ? AnaEvent.AllEvents["最快心率"][0].AvgHr.ToString() : "0";
            string dateStringMaxHR = AnaEvent.AllEvents.ContainsKey("最快心率") && AnaEvent.AllEvents["最快心率"].Count > 0 ? AnaEvent.AllEvents["最快心率"][0].StartTime : "";
            if (dateStringMaxHR != "")
            {
                this.MaxHRTime.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.MaxHRTime.Value = DateTime.ParseExact(dateStringMaxHR, DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                this.MaxHRTime.CustomFormat = " ";
            }

            this.tbxMinHR.Text = AnaEvent.AllEvents.ContainsKey("最慢心率") && AnaEvent.AllEvents["最慢心率"].Count > 0 ? AnaEvent.AllEvents["最慢心率"][0].AvgHr.ToString() : "0";
            string dateStringMinHR = AnaEvent.AllEvents.ContainsKey("最慢心率") && AnaEvent.AllEvents["最慢心率"].Count > 0 ? AnaEvent.AllEvents["最慢心率"][0].StartTime : "";
            if (dateStringMinHR != "")
            {
                this.MinHRTime.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.MinHRTime.Value = DateTime.ParseExact(dateStringMinHR, DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                this.MinHRTime.CustomFormat = " ";
            }

            this.tbxTachycardia.Text = AnaEvent.AllEvents.ContainsKey("心动过速") ? AnaEvent.AllEvents["心动过速"].Count.ToString() : "0";
            this.tbxBradycardia.Text = AnaEvent.AllEvents.ContainsKey("心动过缓") ? AnaEvent.AllEvents["心动过缓"].Count.ToString() : "0";
            if (AnaEvent.AllEvents.ContainsKey("停搏") && AnaEvent.AllEvents["停搏"].Count>0)
            {
                this.tbxAsystoleCount.Text = AnaEvent.AllEvents["停搏"].Count.ToString();
                this.tbxMaxAsystole.Text = (AnaEvent.AllEvents["停搏"][0].AvgRR / 1000f).ToString("F2");
                string dateStringMaxAsystole = AnaEvent.AllEvents["停搏"][0].StartTime;

                this.MaxAsystoleTime.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.MaxAsystoleTime.Value = DateTime.ParseExact(dateStringMaxAsystole, DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                this.MaxAsystoleTime.CustomFormat = " ";
            }

            int totalSeconds = AnaValues.holterConfig.Total / AnaValues.holterConfig.SampleRate;
            TimeSpan totalTime = TimeSpan.FromSeconds(totalSeconds);

            string recordTimeStr = (int)totalTime.TotalHours + CommonValues.Langs["小时"] + totalTime.Minutes + CommonValues.Langs["分钟"];
            this.tbxRecordTime.Text = recordTimeStr;

            int segmentUnanalyzableTotal = 0;
            for (int i = 0; i < AnaValues.UnanalyzableSegments.Count; i++)
            {
                segmentUnanalyzableTotal += AnaValues.UnanalyzableSegments[i].EndIndex - AnaValues.UnanalyzableSegments[i].StartIndex;
            }
            double totalAnaSeconds = (double)(AnaValues.holterConfig.Total - segmentUnanalyzableTotal) / AnaValues.holterConfig.SampleRate;
            TimeSpan AnaTime = TimeSpan.FromSeconds(totalAnaSeconds);

            string anaTimeStr = (int)AnaTime.TotalHours + CommonValues.Langs["小时"] + AnaTime.Minutes + CommonValues.Langs["分钟"];
            this.tbxAnaTime.Text = anaTimeStr;

            this.tbxTotalBeatsV.Text = (AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'V') + AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'T')).ToString();
            this.tbxSingleV.Text = AnaEvent.AllEvents.ContainsKey("单发室早") ? AnaEvent.AllEvents["单发室早"].Count.ToString() : "0";
            this.tbxPairedV.Text = AnaEvent.AllEvents.ContainsKey("成对室早") ? AnaEvent.AllEvents["成对室早"].Count.ToString() : "0";
            this.tbxBigeminyV.Text = AnaEvent.AllEvents.ContainsKey("室性二联律") ? AnaEvent.AllEvents["室性二联律"].Count.ToString() : "0";
            this.tbxTrigeminyV.Text = AnaEvent.AllEvents.ContainsKey("室性三联律") ? AnaEvent.AllEvents["室性三联律"].Count.ToString() : "0";
            this.tbxTachycardiaV.Text = AnaEvent.AllEvents.ContainsKey("室速") ? AnaEvent.AllEvents["室速"].Count.ToString() : "0";
            this.tbxLongestTachycardiaV.Text = AnaEvent.AllEvents["室速"].Count > 0 ? AnaEvent.AllEvents["室速"][0].BeatSize.ToString() : "0";

            if (AnaEvent.AllEvents.ContainsKey("室速") && AnaEvent.AllEvents["室速"].Count > 0)
            {
                string dateStringLongestTachycardiaV = AnaEvent.AllEvents["室速"][0].StartTime;
                this.tbxLongestTachycardiaTimeV.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.tbxLongestTachycardiaTimeV.Value = DateTime.ParseExact(dateStringLongestTachycardiaV, DateFormat, CultureInfo.InvariantCulture);

                double LongestTachycardiaDurationV = AnaEvent.AllEvents["室速"][0].Duration;
                this.tbxLongestTachycardiaDurationV.Text = LongestTachycardiaDurationV.ToString();

                int MaxHR = AnaEvent.AllEvents["室速"][0].AvgHr;
                int MinHR = AnaEvent.AllEvents["室速"][0].AvgHr;
                for (int i = 0; i < AnaEvent.AllEvents["室速"].Count; i++)
                {
                    MaxHR = Math.Max(MaxHR, AnaEvent.AllEvents["室速"][i].AvgHr);
                    MinHR = Math.Min(MinHR, AnaEvent.AllEvents["室速"][i].AvgHr);
                }
                this.tbxMaxTachycardiaV.Text = MaxHR.ToString();
                this.tbxMinTachycardiaV.Text = MinHR.ToString();

                int maxHRIndex = AnaEvent.AllEvents["室速"].FindIndex(h => h.AvgHr == MaxHR);
                int minHRIndex = AnaEvent.AllEvents["室速"].FindIndex(h => h.AvgHr == MinHR);
                string dateStringMaxHRV = AnaEvent.AllEvents["室速"][maxHRIndex].StartTime;
                this.tbxMaxTachycardiaTimeV.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.tbxMaxTachycardiaTimeV.Value = DateTime.ParseExact(dateStringMaxHRV, DateFormat, CultureInfo.InvariantCulture);

                string dateStringMinHRV = AnaEvent.AllEvents["室速"][minHRIndex].StartTime;
                this.tbxMinTachycardiaTimeV.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.tbxMinTachycardiaTimeV.Value = DateTime.ParseExact(dateStringMinHRV, DateFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                this.tbxLongestTachycardiaTimeV.CustomFormat = " ";
                this.tbxMaxTachycardiaTimeV.CustomFormat = " ";
                this.tbxMinTachycardiaTimeV.CustomFormat = " ";
            }
            this.EscapeV.Text = AnaEvent.AllEvents.ContainsKey("室性逸搏") ? AnaEvent.AllEvents["室性逸搏"].Count.ToString() : "0";
            this.tbxRonT.Text = AnaEvent.AllEvents.ContainsKey("R on T") ? AnaEvent.AllEvents["R on T"].Count.ToString() : "0";

            this.tbxTotalBeatsS.Text = AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'S').ToString();
            this.tbxSingleS.Text = AnaEvent.AllEvents.ContainsKey("单发房早") ? AnaEvent.AllEvents["单发房早"].Count.ToString() : "0";
            this.tbxPairedS.Text = AnaEvent.AllEvents.ContainsKey("成对房早") ? AnaEvent.AllEvents["成对房早"].Count.ToString() : "0";
            this.tbxBigeminyS.Text = AnaEvent.AllEvents.ContainsKey("房性二联律") ? AnaEvent.AllEvents["房性二联律"].Count.ToString() : "0";
            this.tbxTrigeminyS.Text = AnaEvent.AllEvents.ContainsKey("房性三联律") ? AnaEvent.AllEvents["房性三联律"].Count.ToString() : "0";
            this.tbxTachycardiaS.Text = AnaEvent.AllEvents.ContainsKey("房速") ? AnaEvent.AllEvents["房速"].Count.ToString() : "0";
            this.tbxLongestTachycardiaS.Text = AnaEvent.AllEvents["房速"].Count > 0 ? AnaEvent.AllEvents["房速"][0].BeatSize.ToString() : "0";

            if (AnaEvent.AllEvents.ContainsKey("房速") && AnaEvent.AllEvents["房速"].Count > 0)
            {
                string dateStringLongestTachycardiaS = AnaEvent.AllEvents["房速"][0].StartTime;
                this.tbxLongestTachycardiaTimeS.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.tbxLongestTachycardiaTimeS.Value = DateTime.ParseExact(dateStringLongestTachycardiaS, DateFormat, CultureInfo.InvariantCulture);

                double LongestTachycardiaDurationS = AnaEvent.AllEvents["房速"][0].Duration;
                this.tbxLongestTachycardiaDurationS.Text = LongestTachycardiaDurationS.ToString();

                int MaxHR = AnaEvent.AllEvents["房速"][0].AvgHr;
                int MinHR = AnaEvent.AllEvents["房速"][0].AvgHr;
                for (int i = 0; i < AnaEvent.AllEvents["房速"].Count; i++)
                {
                    MaxHR = Math.Max(MaxHR, AnaEvent.AllEvents["房速"][i].AvgHr);
                    MinHR = Math.Min(MinHR, AnaEvent.AllEvents["房速"][i].AvgHr);
                }
                this.tbxMaxTachycardiaS.Text = MaxHR.ToString();
                this.tbxMinTachycardiaS.Text = MinHR.ToString();

                int maxHRIndex = AnaEvent.AllEvents["房速"].FindIndex(h => h.AvgHr == MaxHR);
                int minHRIndex = AnaEvent.AllEvents["房速"].FindIndex(h => h.AvgHr == MinHR);
                string dateStringMaxHRS = AnaEvent.AllEvents["房速"][maxHRIndex].StartTime;
                this.tbxMaxTachycardiaTimeS.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.tbxMaxTachycardiaTimeS.Value = DateTime.ParseExact(dateStringMaxHRS, DateFormat, CultureInfo.InvariantCulture);

                string dateStringMinHRS = AnaEvent.AllEvents["房速"][minHRIndex].StartTime;
                this.tbxMinTachycardiaTimeS.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.tbxMinTachycardiaTimeS.Value = DateTime.ParseExact(dateStringMinHRS, DateFormat, CultureInfo.InvariantCulture);

            }
            else
            {
                this.tbxLongestTachycardiaTimeS.CustomFormat = " ";
                this.tbxMaxTachycardiaTimeS.CustomFormat = " ";
                this.tbxMinTachycardiaTimeS.CustomFormat = " ";
            }
            this.EscapeS.Text = AnaEvent.AllEvents["房性逸搏"].Count>0 ? AnaEvent.AllEvents["房性逸搏"].Count.ToString() : "0";

            bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;
            if (bPace)
            {
                List<BeatInfo> beatInfoList = AnaValues.CachedAllBeats;
                string beatSequence = AnaEvent.GetBeatSequence(beatInfoList);
                PaceReportInfo paceReportInfo = AnaCode.GetPaceReportInfo(beatSequence);

                this.tbxSDNN.Text = paceReportInfo.TotalCount.ToString();
                this.tbxSDANN.Text = paceReportInfo.APCount.ToString();
                this.tbxSDNNindex.Text = paceReportInfo.VPCount.ToString();
                this.tbxRMSSD.Text = paceReportInfo.AVPCount.ToString();

                this.tbxLF.Text = paceReportInfo.TotalPercent.ToString();
                this.tbxHF.Text = paceReportInfo.APPercent.ToString();
                this.tbxLFHF.Text = paceReportInfo.VPPercent.ToString();
                this.tbxTP.Text = paceReportInfo.AVPPercent.ToString();
            }
            else
            {
                HRVReportInfo hrvReportInfo = AnaCode.GetHRVReportInfo();
                if (hrvReportInfo != null)
                {
                    string LF_HF = Math.Round(Convert.ToDouble(hrvReportInfo.lf) / Convert.ToDouble(hrvReportInfo.hf), 1).ToString();
                    this.tbxSDNN.Text = hrvReportInfo.sdnn;
                    this.tbxSDANN.Text = hrvReportInfo.sdann;
                    this.tbxSDNNindex.Text = hrvReportInfo.sdnnIndex;
                    this.tbxRMSSD.Text = hrvReportInfo.rmssd;
                    this.tbxpNN50.Text = hrvReportInfo.pnn50;
                    this.tbxTriindex.Text = hrvReportInfo.hrvTriangularIndex;
                    this.tbxLF.Text = hrvReportInfo.lf;
                    this.tbxHF.Text = hrvReportInfo.hf;
                    this.tbxLFHF.Text = LF_HF;
                    this.tbxTP.Text = hrvReportInfo.total_power;
                    this.tbxULF.Text = hrvReportInfo.ulf;
                    this.tbxVLF.Text = hrvReportInfo.vlf;
                }
            }


            STReportInfo stReportInfo = AnaCode.GetSTReportInfo();
            if (stReportInfo.stLowCount > 0)
            {
                this.tbxSTLowCount.Text = stReportInfo.stLowCount.ToString();
                this.tbxLowest.Text = stReportInfo.stLowest;

                this.lowestTime.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                this.lowestTime.Value = DateTime.ParseExact(stReportInfo.stLowestTimeLong, DateFormat, CultureInfo.InvariantCulture);

                this.lowestLead.Text = stReportInfo.stLowestLead;
                this.tbxLowestDuration.Text = stReportInfo.stLowLeadDuration;
            }
            else
            {
                this.lowestTime.CustomFormat = " ";
                this.lowestLead.SelectedIndex = -1;
            }


            if (stReportInfo.stUpCount > 0)
            {
                this.tbxSTUpCount.Text = stReportInfo.stUpCount.ToString();
                this.tbxUpest.Text = stReportInfo.stUpest;
                this.upestTime.CustomFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss"; 
                this.upestTime.Value = DateTime.ParseExact(stReportInfo.stUpestTimeLong, DateFormat, CultureInfo.InvariantCulture);
                this.upestLead.Text = stReportInfo.stUpestLead;
                this.tbxUpestDuration.Text = stReportInfo.stUpLeadDuration;
            }
            else
            {
                this.upestTime.CustomFormat = " ";
                this.upestLead.SelectedIndex = -1;
            }

            AFReportInfo afReportInfo = AnaCode.GetAfReportInfo();
            this.tbxAFPrecent.Text = afReportInfo.AFPercent;
            // 时长文本的小时位可大于24（如 26:13:45），用 SetTimeText 规范化显示
            this.tbxAFDuration.SetTimeText(afReportInfo.AFDuration);
            this.tbxAFCount.Text = afReportInfo.AFCount.ToString();
            this.tbxAFLPrecent.Text = afReportInfo.AFPercent2;
            this.tbxAFLDuration.SetTimeText(afReportInfo.AFDuration2);
            this.tbxAFLCount.Text = afReportInfo.AFCount2.ToString();
        }

        private void WriteData()
        {
            var summaryValuesJson = new JObject();
            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel2);
            foreach (TextBoxEx textBox in textBoxList)
            {
                summaryValuesJson[textBox.Name] = textBox.Text;
            }

            List<DateTimePicker> dataTimeList = CommonFuns.RetriveSubControls<DateTimePicker>(this.tableLayoutPanel2);
            foreach (DateTimePicker dataTime in dataTimeList)
            {
                summaryValuesJson[dataTime.Name] = dataTime.Value;
            }

            // 时长输入控件（tbxAFDuration/tbxAFLDuration）：保存 "HH:mm:ss" 文本，小时位可大于24
            List<TimeDurationBox> durationBoxList = CommonFuns.RetriveSubControls<TimeDurationBox>(this.tableLayoutPanel2);
            foreach (TimeDurationBox durationBox in durationBoxList)
            {
                summaryValuesJson[durationBox.Name] = durationBox.Text;
            }

            List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(this.tableLayoutPanel2);
            foreach (ComboBox comboBox in comboboxList)
            {
                summaryValuesJson[comboBox.Name] = comboBox.Text;
            }

            summaryValuesJson["longestRRStr"] = AnaEvent.AllEvents["最长RR间期"].Count > 0 ? Math.Round((double)AnaEvent.AllEvents["最长RR间期"][0].AvgRR / 1000f, 2) : 0;
            summaryValuesJson["dateStringLongestRR"] = AnaEvent.AllEvents["最长RR间期"].Count > 0 ? AnaEvent.AllEvents["最长RR间期"][0].StartTime.Split(' ')[1] : "";

            // 将summaryValuesJson写入SummaryFilePath
            try
            {
                string jsonString = summaryValuesJson.ToString();
                File.WriteAllText(SummaryFilePath, jsonString, Encoding.UTF8);
                ScreenCenterTip.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReadData()
        {
            // 检查文件是否存在
            if (!File.Exists(SummaryFilePath))
            {
                return;
            }

            try
            {
                // 读取JSON文件内容
                string jsonString = File.ReadAllText(SummaryFilePath, Encoding.UTF8);
                var summaryValuesJson = JObject.Parse(jsonString);

                // 读取TextBoxEx控件数据
                List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel2);
                foreach (TextBoxEx textBox in textBoxList)
                {
                    if (summaryValuesJson[textBox.Name] != null)
                    {
                        textBox.Text = summaryValuesJson[textBox.Name].ToString();
                    }
                }

                // 读取DateTimePicker控件数据
                List<DateTimePicker> dataTimeList = CommonFuns.RetriveSubControls<DateTimePicker>(this.tableLayoutPanel2);
                foreach (DateTimePicker dataTime in dataTimeList)
                {
                    if (summaryValuesJson[dataTime.Name] != null)
                    {
                        DateTime dateValue;
                        if (DateTime.TryParse(summaryValuesJson[dataTime.Name].ToString(), out dateValue))
                        {
                            dataTime.Value = dateValue;
                        }
                    }
                }

                // 读取时长输入控件数据（tbxAFDuration/tbxAFLDuration）
                List<TimeDurationBox> durationBoxList = CommonFuns.RetriveSubControls<TimeDurationBox>(this.tableLayoutPanel2);
                foreach (TimeDurationBox durationBox in durationBoxList)
                {
                    if (summaryValuesJson[durationBox.Name] == null)
                    {
                        continue;
                    }
                    string text = summaryValuesJson[durationBox.Name].ToString();
                    // 用 TimeDurationBox 的分段解析判断新格式（"HH:mm:ss"，小时位可大于24）；
                    // 不能用 TimeSpan.TryParse，它会把 "24:00:00" 当成 24 天
                    if (TimeDurationBox.TryParseDuration(text, double.MaxValue, out _))
                    {
                        // 新格式："HH:mm:ss" 文本（小时位可大于24）
                        durationBox.SetTimeText(text);
                    }
                    else if (DateTime.TryParse(text, out DateTime dtValue))
                    {
                        // 兼容旧版本：旧数据以 DateTime 序列化保存，取时间部分
                        durationBox.SetTimeText(dtValue.ToString("HH:mm:ss"));
                    }
                    else
                    {
                        durationBox.SetTimeText("");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            WriteData();
            AnaValues.DataSourceState = 1;
        }
    }
}
