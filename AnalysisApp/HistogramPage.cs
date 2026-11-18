using AnalysisApp.CommonControls;
using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component.Controls;

namespace AnalysisApp
{
    public partial class HistogramPage : BaseUserControl
    {
        private HistogramType _curCategory = HistogramType.RRInterval;

        private List<HistogramInfo> _allHistogramTypes = new List<HistogramInfo>
        {
            new HistogramInfo(HistogramType.RRInterval) { TypeId="NN", Description="NN间期", BeatsTypeCode="NN" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="NS", Description="NS间期", BeatsTypeCode="NS" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="NV", Description="NV间期", BeatsTypeCode="NV" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="RR", Description="RR间期", BeatsTypeCode="**" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="RS", Description="RS间期", BeatsTypeCode="*S" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="RV", Description="RV间期", BeatsTypeCode="*V" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="SN", Description="SN间期", BeatsTypeCode="SN" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="SR", Description="SR间期", BeatsTypeCode="S*" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="SS", Description="SS间期", BeatsTypeCode="SS" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="VN", Description="VN间期", BeatsTypeCode="VN" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="VR", Description="VR间期", BeatsTypeCode="V*" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="VV", Description="VV间期", BeatsTypeCode="VV" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="NJ1", Description="交界早间期", BeatsTypeCode="NJ" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="NJe", Description="交界性逸搏间期", BeatsTypeCode="NG" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="vE", Description="室性逸搏间期", BeatsTypeCode="E" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="RR_Long", Description="长RR间期", BeatsTypeCode="**" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="NAf", Description="N-Af间期", BeatsTypeCode="NK" }, //房颤
            new HistogramInfo(HistogramType.RRInterval) { TypeId="AfAf", Description="Af-Af间期", BeatsTypeCode="KK" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="AfN", Description="Af-N间期", BeatsTypeCode="KN" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="NAF", Description="N-AF间期", BeatsTypeCode="NM" }, //房扑
            new HistogramInfo(HistogramType.RRInterval) { TypeId="AFAF", Description="AF-AF间期", BeatsTypeCode="MM" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="AFN", Description="AF-N间期", BeatsTypeCode="MN" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="PV", Description="PV间期", BeatsTypeCode="PV" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="PS", Description="PS间期", BeatsTypeCode="PS" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="PN", Description="PN间期", BeatsTypeCode="PN" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="VP", Description="VP间期", BeatsTypeCode="VP" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="SP", Description="SP间期", BeatsTypeCode="SP" },
            new HistogramInfo(HistogramType.RRInterval) { TypeId="NP", Description="NP间期", BeatsTypeCode="NP" },
            new HistogramInfo(HistogramType.EarlyRate) { TypeId="S2", Description="房早提早率", BeatsTypeCode="NS" },
            new HistogramInfo(HistogramType.EarlyRate) { TypeId="V2", Description="室早提早率", BeatsTypeCode="NV" },
            new HistogramInfo(HistogramType.EarlyRate) { TypeId="NJ2", Description="交界早提早率", BeatsTypeCode="NJ" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NNN_Ratio", Description="3N间期比率", BeatsTypeCode="NNN" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NN_Ratio", Description="NN间期比率", BeatsTypeCode="NN" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NS_Ratio", Description="NS间期比率", BeatsTypeCode="NS" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NV_Ratio", Description="NV间期比率", BeatsTypeCode="NV" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="RR_Ratio", Description="RR间期比率", BeatsTypeCode="**" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="RS_Ratio", Description="RS间期比率", BeatsTypeCode="*S" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="RV_Ratio", Description="RV间期比率", BeatsTypeCode="*V" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="SN_Ratio", Description="SN间期比率", BeatsTypeCode="SN" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="SR_Ratio", Description="SR间期比率", BeatsTypeCode="S*" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="SS_Ratio", Description="SS间期比率", BeatsTypeCode="SS" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="VN_Ratio", Description="VN间期比率", BeatsTypeCode="VN" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="VR_Ratio", Description="VR间期比率", BeatsTypeCode="V*" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="VV_Ratio", Description="VV间期比率", BeatsTypeCode="VV" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="PV_Ratio", Description="PV间期比率", BeatsTypeCode="PV" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="PS_Ratio", Description="PS间期比率", BeatsTypeCode="PS" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="PN_Ratio", Description="PN间期比率", BeatsTypeCode="PN" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="VP_Ratio", Description="VP间期比率", BeatsTypeCode="VP" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="SP_Ratio", Description="SP间期比率", BeatsTypeCode="SP" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NP_Ratio", Description="NP间期比率", BeatsTypeCode="NP" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NJ_Ratio", Description="NJ间期比率", BeatsTypeCode="NJ" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NJe_Ratio", Description="N-Je间期比率", BeatsTypeCode="NG" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NAf_Ratio", Description="N-Af间期比率", BeatsTypeCode="NK" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="AfAf_Ratio", Description="Af-Af间期比率", BeatsTypeCode="KK" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="AfN_Ratio", Description="Af-N间期比率", BeatsTypeCode="KN" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="NAF_Ratio", Description="N-AF间期比率", BeatsTypeCode="NM" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="AFAF_Ratio", Description="AF-AF间期比率", BeatsTypeCode="MM" },
            new HistogramInfo(HistogramType.RRIntervalRatio) { TypeId="AFN_Ratio", Description="AF-N间期比率", BeatsTypeCode="MN" },
            new HistogramInfo(HistogramType.AverageHR) { TypeId="NNNavg", Description="窦性心率", BeatsTypeCode="NNN" },
            new HistogramInfo(HistogramType.AverageHR) { TypeId="SSSavg", Description="房速心率", BeatsTypeCode="SSS" },
            new HistogramInfo(HistogramType.AverageHR) { TypeId="RRavg", Description="平均心率", BeatsTypeCode="**" },
            new HistogramInfo(HistogramType.AverageHR) { TypeId="VVVavg", Description="室速心率", BeatsTypeCode="VVV" },
            new HistogramInfo(HistogramType.AverageHR) { TypeId="PPPavg", Description="起搏心率", BeatsTypeCode="PPP" }
        };

        public HistogramPage()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            _allHistogramTypes.ForEach(o =>
            {
                o.IsValid = false;
                if (o.ChartType == HistogramType.AverageHR)
                {
                    if (o.TypeId == "NNNavg")
                    {
                        int refNum = AnaValues.holterConfig.ParamsSetting.P14.Frequency;
                        if (refNum % 2 == 0) refNum += 1;
                        o.BeatsTypeCode = new string(o.BeatsTypeCode[0], refNum);
                    }
                }
            });

            if (resetUI)
                this.tsBtnInterval.PerformClick();
            else
                loadHistogramItems(_curCategory);

            if (this.ecgCommonWave1.IsLoaded)
            {
                this.ecgCommonWave1.RefreshData(resetUI);
            }

            this.beatWaveList1.LeadCount = 3;
            if (resetUI)
            {
                this.beatWaveList1.ResetUIState(true);
            }
        }

        private void HistogramPage_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsUiInitialized) InitUI();

                this.beatWaveList1.LayoutStyle = BeatWaveStyle.ThreeLeads;
                //loadHistogramTypes();

                Console.WriteLine("直方图界面已加载");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "直方图");
            }
        }

        private void lvHistogramType_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem item = this.lvHistogramType.HitTest(e.Location).Item;
            if (item != null)
            {
                if (item.ForeColor == SystemColors.GrayText)
                {
                    this.lvHistogramType.Cursor = Cursors.No;
                    return;
                }
            }
            this.lvHistogramType.Cursor = Cursors.Default;
        }

        private void lvHistogramType_MouseDown(object sender, MouseEventArgs e)
        {
            var item = lvHistogramType.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                if (item.ForeColor == SystemColors.GrayText) // 如果项被禁用
                {
                    if (lvHistogramType.SelectedIndices.Count > 0)
                    {
                        int selectedIndex = this.lvHistogramType.SelectedIndices[0];
                        item.Tag = selectedIndex;
                    }
                }
            }
        }

        private void lvHistogramType_MouseUp(object sender, MouseEventArgs e)
        {
            var item = lvHistogramType.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                if (item.ForeColor == SystemColors.GrayText) // 如果项被禁用
                {
                    if (item.Tag != null)
                    {
                        int selectedIndex = Convert.ToInt32(item.Tag);
                        lvHistogramType.Items[selectedIndex].Selected = true;
                        item.Tag = null;
                    }
                }
            }
        }

        private void lvHistogramType_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.Item.Selected) return;

            string histogramType = e.Item.Name;
            HistogramInfo histInfo = _allHistogramTypes.Find(o => o.TypeId == histogramType);
            if (!histInfo.IsValid) e.Item.Selected = false;
        }

        private void lvHistogramType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lvHistogramType.SelectedItems.Count > 0)
            {
                string histogramType = lvHistogramType.SelectedItems[0].Name;
                HistogramInfo histInfo = _allHistogramTypes.Find(o => o.TypeId == histogramType);
                List<BeatInfo> histData = AnaCode.QueryHistogramData(histInfo);
                updateHistogram(sender, histInfo, histData);
            }
        }

        private void beatWaveList_OnLayoutChanged(BeatWaveList ctlBeatList, BeatWaveStyle waveLayout)
        {
            switch (waveLayout)
            {
                case BeatWaveStyle.OneBeat:
                    ctlBeatList.RowCount = 4;
                    ctlBeatList.ColCount = (ctlBeatList.Width - 12) / 95;
                    break;
                case BeatWaveStyle.OneLead:
                    ctlBeatList.RowCount = 4;
                    ctlBeatList.ColCount = 4;
                    break;
                case BeatWaveStyle.ThreeLeads:
                    ctlBeatList.RowCount = 2;
                    ctlBeatList.ColCount = 4;
                    break;
                case BeatWaveStyle.OneLongLead:
                    ctlBeatList.RowCount = 4;
                    ctlBeatList.ColCount = 4;
                    break;
                case BeatWaveStyle.ThreeLongLeads:
                    ctlBeatList.RowCount = 2;
                    ctlBeatList.ColCount = 4;
                    break;
            }
            ctlBeatList.RefreshUI(ctlBeatList, true);
        }


        public override void InitUI()
        {
            base.InitUI();

            try
            {
                //this.beatWaveList1.LeadCount = 3;
                //this.beatWaveList1.HideSettingTools();
                this.beatWaveList1.ShowTime = true;
                this.lvHistogramType.Columns.Add("");
                this.lvHistogramType.MouseDown += lvHistogramType_MouseDown;
                this.lvHistogramType.MouseUp += lvHistogramType_MouseUp;

                registerEvents();

                Console.WriteLine("直方图界面已初始化完成");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "直方图");
            }
        }

        private void registerEvents()
        {
            this.histogramPlot1.OnDataChanged += (d) =>
            {
                lvHistogramType_SelectedIndexChanged(this.histogramPlot1, null);
            };
            this.histogramPlot1.OnBeatSelected += (sender, beats, keepSelection) =>
            {
                if (keepSelection)
                    this.beatWaveList1.UpdateBeatSource(sender, beats, keepSelection);
                else
                    this.beatWaveList1.SetBeatSource(beats);
            };

            this.beatWaveList1.OnLayoutChanged += beatWaveList_OnLayoutChanged;
            this.beatWaveList1.OnBeatSelected += (sender, beats, selBeatsInPage) =>
            {
                if (beats.Count > 0)
                {
                    if (!(sender == this.ecgCommonWave1 && this.ecgCommonWave1.DataSourceIsChanged == false))
                    {
                        int locateSample = beats[0].Sample;
                        if (this.histogramPlot1.ChartInfo != null && this.histogramPlot1.ChartInfo.TypeId == "NNNavg")
                        {
                            //窦性心率将定位向后推移到用于计算平均心率的中间心搏的位置
                            int refNum = AnaValues.holterConfig.ParamsSetting.P14.Frequency;
                            locateSample = AnaValues.CachedAllBeats[beats[0].BeatIndex + refNum / 2].Sample;
                        }
                        this.ecgCommonWave1.OnWaveformStartTimeChanged(locateSample);
                    }
                }
            };
            this.beatWaveList1.OnDataChanged += (d) =>
            {
                lvHistogramType_SelectedIndexChanged(this.beatWaveList1, null);
            };
            this.beatWaveList1.OnLeadsChanged += (leads) =>
            {
                this.ecgCommonWave1.leadIdxs = leads;
                this.ecgCommonWave1.forceWaveformRedraw();
            };

            this.ecgCommonWave1.OnDataChanged += (d) =>
            {
                lvHistogramType_SelectedIndexChanged(this.ecgCommonWave1, null);
            };

            TableLayoutControlCollection toopControls = this.tlpCategory.Controls;
            foreach (Control ctl in toopControls)
            {
                if (ctl is Button btn)
                {
                    btn.Click += (o, e) =>
                    {
                        if (btn.Name == "tsBtnInterval")
                            _curCategory = HistogramType.RRInterval;
                        else if (btn.Name == "tsBtnIntervalRatio")
                            _curCategory = HistogramType.RRIntervalRatio;
                        else if (btn.Name == "tsBtnAverageHR")
                            _curCategory = HistogramType.AverageHR;
                        else if (btn.Name == "tsBtnEarlyRate")
                            _curCategory = HistogramType.EarlyRate;

                        foreach (Control tsItem in toopControls)
                        {
                            if (!(tsItem is RoundButton rdButton)) continue;
                            if (rdButton == btn)
                                rdButton.BaseColor = Color.FromArgb(159, 200, 255);
                            else
                                rdButton.BaseColor = Color.FromArgb(221, 231, 245);
                        }

                        loadHistogramItems(_curCategory);
                    };
                }
            }
        }

        //加载所有直方图类型及其状态
        private void loadHistogramItems(HistogramType histType)
        {
            int earlyRate = AnaValues.holterConfig.ParamsSetting.P19.Before;
            List<BeatInfo> sc = AnaCode.FilterOutCustomEventsBeats(AnaValues.CachedAllBeats);
            Dictionary<int, int> beatMap = AnaCode.GetBeatMap(sc);
            string allKeys = AnaCode.GetKeysOfBeats(sc);

            List<HistogramInfo> charts = _allHistogramTypes.FindAll(o => o.ChartType == histType);
            this.lvHistogramType.BeginUpdate();
            this.lvHistogramType.Items.Clear();
            foreach (HistogramInfo histInfo in charts)
            {
                string keys = allKeys;
                if (histInfo.BeatsTypeCode.IndexOf('P') >= 0)
                {
                    //房室起搏、室性起搏、双腔起搏 均归为起搏
                    keys = allKeys.Replace('O', 'P').Replace('H', 'P');
                }

                string[] queryKeys = AnaCode.GetQueryKeys(histInfo.BeatsTypeCode);
                if (sc != null && sc.Count > 0)
                {
                    if (queryKeys == null)
                    {
                        if (histInfo.TypeId == "RR_Long")
                            histInfo.IsValid = sc.Any(e => e.BeatUiType != 'X' && e.BeatUiType != 'Q' && e.RRInterval > AnaValues.holterConfig.ParamsSetting.P8.Rr);
                        else
                            histInfo.IsValid = sc.Any(e => e.BeatUiType != 'X' && e.BeatUiType != 'Q');
                    }
                    else
                    {
                        List<BeatInfo> scS2 = null;
                        bool isSequential = (histInfo.ChartType == HistogramType.AverageHR && histInfo.TypeId != "RRavg");
                        foreach (string queryKey in queryKeys)
                        {
                            //if (keys.IndexOf(queryKey, StringComparison.Ordinal) >= 0)
                            if (Regex.IsMatch(keys, queryKey))
                            {
                                if (histInfo.TypeId == "S2")
                                {
                                    int calCount = 0;

                                    if (scS2 == null) scS2 = AnaCode.GetValidBeatsByCondition(sc, keys, queryKeys, isSequential);
                                    var earlyrates = from item in scS2
                                                         //let avgRR = avg4RR(sc, beatMap[item.Sample])
                                                     let avgRR = AnaCode.CalcAvgRR(sc, item, 4, out calCount, true, beatMap)
                                                     select new
                                                     {
                                                         item,
                                                         avgRR,
                                                         rate = (double)(avgRR - item.RRInterval) / avgRR * 100
                                                     };
                                    histInfo.IsValid = earlyrates.Any(o => o.rate >= earlyRate);
                                }
                                else { histInfo.IsValid = true; }

                                if (histInfo.IsValid) break;
                            }
                        }
                    }
                }

                string itemText = _langs[histInfo.Description];
                Match hyphenMatch = Regex.Match(histInfo.Description, @"\w+-\w+", RegexOptions.IgnoreCase);
                if (hyphenMatch != null) { itemText = hyphenMatch.Value + _langs[histInfo.Description.Substring(hyphenMatch.Value.Length)]; }

                ListViewItem lvItem = new ListViewItem(itemText);
                lvItem.Name = histInfo.TypeId;
                lvItem.ForeColor = histInfo.IsValid ? Color.Black : SystemColors.GrayText;
                this.lvHistogramType.Items.Add(lvItem);
            }
            this.lvHistogramType.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.lvHistogramType.EndUpdate();

            //选择默认项
            bool isValid = false;
            if (histType == HistogramType.RRInterval)
            {
                ListViewItem[] histRR = lvHistogramType.Items.Find("RR", false);
                if (histRR.Length > 0)
                {
                    isValid = true;
                    histRR[0].Selected = true;
                }
            }
            else
            {
                foreach (ListViewItem item in lvHistogramType.Items)
                {
                    HistogramInfo histInfo = charts.Find(o => o.TypeId == item.Name);
                    if (histInfo.IsValid)
                    {
                        item.Selected = true;
                        isValid = true;
                        break;
                    }
                }
            }

            if (!isValid)
            {
                updateHistogram(this, charts[0], new List<BeatInfo>());
            }
        }

        //计算平均RR间期
        private int avg4RR(List<BeatInfo> srcBeats, int index)
        {
            int count = 0;
            int sum4RR = 0;
            int breakCount = 0;

            for (int j = index; j >= 0 && count < 4; j--)
            {
                if (breakCount == 10) { break; }
                if (srcBeats[j].BeatUiType != 'N')
                {
                    breakCount++;
                    continue;
                }
                sum4RR += srcBeats[j].RRInterval;
                count++;
            }
            int avgRR = (int)Math.Round((double)sum4RR / count, MidpointRounding.AwayFromZero);
            return avgRR;
        }

        private void updateHistogram(object sender, HistogramInfo histInfo, List<BeatInfo> beats)
        {
            HistogramType chartType = histInfo.ChartType;
            this.beatWaveList1.ShowAverageHR = false;
            this.beatWaveList1.UseForNormalSinusRhythm = false;
            this.beatWaveList1.UseForArrhythmia = false;
            if (chartType == HistogramType.RRInterval)
            {
                histInfo.MaxScaleX = 4000;
                //chartInfo.MaxScaleY = 200;
                histInfo.AxisXTitle = _langs["RR间期"] + "(ms)";
                histInfo.AxisX2Title = _langs["心率"] + "(bpm)";
                histInfo.AxisYTitle = _langs["心搏数"];
            }
            else if (chartType == HistogramType.AverageHR)
            {
                histInfo.MaxScaleX = 300;
                histInfo.AxisXTitle = _langs["心率"] + "(bpm)";
                histInfo.AxisX2Title = "";
                histInfo.AxisYTitle = _langs["心搏数"];
                this.beatWaveList1.ShowAverageHR = true;
                if (histInfo.TypeId == "NNNavg") this.beatWaveList1.UseForNormalSinusRhythm = true;
                if (histInfo.TypeId == "SSSavg" || histInfo.TypeId == "VVVavg") this.beatWaveList1.UseForArrhythmia = true;
            }
            else if (chartType == HistogramType.EarlyRate)
            {
                histInfo.MaxScaleX = 100;
                histInfo.AxisXTitle = _langs["提前量"];
                histInfo.AxisX2Title = "";
                histInfo.AxisX2Title = "";
                histInfo.AxisYTitle = _langs["心搏数"];
                //histInfo.SubdivisionCount = 5;
            }
            else if (chartType == HistogramType.RRIntervalRatio)
            {
                histInfo.MaxScaleX = 300;
                histInfo.AxisXTitle = _langs["间期比率"];
                histInfo.AxisX2Title = "";
                histInfo.AxisX2Title = "";
                histInfo.AxisYTitle = _langs["心搏数"];
            }
            histInfo.IntervalX = histInfo.MaxScaleX / 20;

            bool keepSelection = sender == this.beatWaveList1 || sender == this.ecgCommonWave1;
            this.histogramPlot1.UpdateDataSource(sender, histInfo, HistogramDrawer.GetData(histInfo, beats), keepSelection);
        }

    }
}
