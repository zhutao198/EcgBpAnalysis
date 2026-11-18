using AnalysisApp.CommonControls;
using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp
{
    public partial class ScatterPage : BaseUserControl
    {
        private int selectedTypeIndex = 0;

        public ScatterPage()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            var beatGroups = getScatterTypeCount();
            //全时散点图
            if (resetUI)
            {
                (this.tsBeatType.Items[0] as ScatterToolStripButton).UserSelected = true;
                this.scatterPlot1.SetBeatType("");
            }
            else
            {
                if (selectedTypeIndex < this.tsBeatType.Items.Count)
                {
                    var btnScatter = this.tsBeatType.Items[selectedTypeIndex] as ScatterToolStripButton;
                    if (beatGroups.Any(o => o.Code == btnScatter.ZqScatterType.Code))
                    {
                        btnScatter.UserSelected = true;
                        this.scatterPlot1.SetBeatType(btnScatter.ZqScatterType.Code);
                    }
                    else { this.scatterPlot1.SetBeatType(""); }
                }
                else
                {
                    this.scatterPlot1.SetBeatType("");
                }
            }

            //时间散点图
            List<AfInfo> afList = AnaCode.GetAfList();
            this.scatterTrend1.AFList = afList;
            this.scatterTrend2.AFList = afList;
            DateTime startTime = scatterTrend1.StartTime, endTime = scatterTrend1.EndTime;
            if (resetUI)
            {
                this.scatterTrend1.SetBeatType("");
            }
            else
            {
                if (beatGroups.Any(o => o.Code == scatterTrend1.BaseBeatType))
                    this.scatterTrend1.UpdateBeatSource(this, startTime, endTime, scatterTrend1.BaseBeatType, null);
                else
                    this.scatterTrend1.SetBeatType("");
            }

            //分时散点图
            if (this.scatterPlotSplit1.IsLoaded) { this.scatterPlotSplit1.SetBeatType(""); }
            if (this.ecgCommonWave1.IsLoaded) { this.ecgCommonWave1.RefreshData(resetUI); }
            if (this.ecgCommonWave2.IsLoaded) { this.ecgCommonWave2.RefreshData(resetUI); }
            if (this.ecgCommonWave3.IsLoaded) { this.ecgCommonWave3.RefreshData(resetUI); }

            if (resetUI)
            {
                this.tabControl1.SelectedIndex = 0;
                this.beatWaveList1.ResetUIState(true);
                this.beatWaveList2.ResetUIState(true);
                this.beatWaveList3.ResetUIState(true);
            }
        }

        #region 事件定义
        private void ScatterPage_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                try
                {
                    if (!IsUiInitialized) InitUI();

                    //getScatterTypeCount();
                    //this.scatterPlot1.SetBeatType(""); //加载全部心搏散点
                    Console.WriteLine("散点图界面已加载");
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "散点图");
                }
            }
        }

        private void ScatterPage_SizeChanged(object sender, EventArgs e)
        {
            //this.scatterPlot1.Width = this.scatterPlot1.Height + 34;
            //this.overlayPlot01.Width = this.overlayPlot02.Width = this.overlayPlot03.Width = (int)(this.overlayPlot01.Height * 1.8);

            //this.scatterPlot2.Width = this.scatterPlot2.Height + 34;
            //this.overlayPlot11.Width = this.overlayPlot12.Width = this.overlayPlot13.Width = (int)(this.overlayPlot11.Height * 1.8);
        }

        private void beatType_Click(object sender, EventArgs e)
        {
            ToolStripItem tsItem = sender as ToolStripItem;
            var scItem = sender as IScatterToolStripItem;
            scItem.UserSelected = true;
            selectedTypeIndex = this.tsBeatType.Items.IndexOf(tsItem);
            getScatterTypeCount();

            refreshTabpageView(scItem.ZqScatterType.Code);
        }

        private void beatType_Normal_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsItem = sender as ToolStripMenuItem;

            string beatTypeCode = tsItem.Text;
            refreshTabpageView(beatTypeCode);

            var parentItem = tsItem.OwnerItem as IScatterToolStripItem;
            if (parentItem != null)
            {
                parentItem.UserSelected = true;
            }

            tsItem.Check();
        }

        private void scatterPlot_BeatSelected(object sender, List<BeatInfo> beats, bool isModify, OverlayPlot overlay1, OverlayPlot overlay2, OverlayPlot overlay3)
        {
            //this.BeginInvoke(new MethodInvoker(() =>
            //{
            if (isModify)
            {
                List<int> affected3 = overlay3.UpdateBeatSource(sender, beats);
                List<int> affected2 = overlay2.UpdateBeatSource(sender, beats);
                List<BeatInfo> remains = new List<BeatInfo>();
                beats.ForEach(o =>
                {
                    if (!affected2.Contains(o.Sample) && !affected3.Contains(o.Sample)) remains.Add(o);
                });
                overlay1.UpdateBeatSource(sender, beats, remains);
            }
            else
            {
                overlay1.SetBeatSource(beats, true);
                overlay2.Amplitude = overlay1.Amplitude;
                overlay3.Amplitude = overlay1.Amplitude;
            }
            //}));
        }

        private void scatterPlotSplit_BeatSelected(object sender, List<BeatInfo> beats, bool isModify, OverlayPlot overlay1, OverlayPlot overlay2, OverlayPlot overlay3)
        {
            if (isModify)
            {
                List<int> affected3 = overlay3.UpdateBeatSource(sender, beats);
                List<int> affected2 = overlay2.UpdateBeatSource(sender, beats);
                List<BeatInfo> remains = new List<BeatInfo>();
                beats.ForEach(o =>
                {
                    if (!affected2.Contains(o.Sample) && !affected3.Contains(o.Sample)) remains.Add(o);
                });
                overlay1.UpdateBeatSource(sender, beats, remains);
            }
            else
            {
                overlay1.SetBeatSource(beats, true);
            }
        }

        private void overlayPlot_Click(object sender, EventArgs e)
        {
            OverlayPlot ctlOverlay = sender as OverlayPlot;
            this.beatWaveList1.SetBeatSource(ctlOverlay.Beats);
        }

        private void overlayPlot_Time_Click(object sender, EventArgs e)
        {
            OverlayPlot ctlOverlay = sender as OverlayPlot;
            this.beatWaveList2.SetBeatSource(ctlOverlay.Beats);
        }

        private void overlayPlot_TimeSplit_Click(object sender, EventArgs e)
        {
        }

        private void overlayPlot_Selected(object sender, List<BeatInfo> beats, bool isModify, BeatWaveList ctlWavelist)
        {
            if (isModify)
                ctlWavelist.UpdateBeatSource(sender, beats, isModify);
            else
                ctlWavelist.SetBeatSource(beats);
        }

        private void beatWaveList_OnLayoutChanged(BeatWaveList ctlBeatList, BeatWaveStyle waveLayout)
        {
            switch (waveLayout)
            {
                case BeatWaveStyle.OneBeat:
                    ctlBeatList.RowCount = 6;
                    ctlBeatList.ColCount = (ctlBeatList.Width - 12) / 95;
                    break;
                case BeatWaveStyle.OneLead:
                    ctlBeatList.RowCount = 5;
                    ctlBeatList.ColCount = 2;
                    break;
                case BeatWaveStyle.ThreeLeads:
                    ctlBeatList.RowCount = 4;
                    ctlBeatList.ColCount = 2;
                    break;
                case BeatWaveStyle.OneLongLead:
                    ctlBeatList.RowCount = 8;
                    ctlBeatList.ColCount = 2;
                    break;
                case BeatWaveStyle.ThreeLongLeads:
                    ctlBeatList.RowCount = 4;
                    ctlBeatList.ColCount = 2;
                    break;
            }
            ctlBeatList.RefreshUI(ctlBeatList, true);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedIndex == 0)
            {
                string beatType = this.scatterPlot1.BaseBeatType;
                foreach (ToolStripItem tsItem in this.tsBeatType.Items)
                {
                    ScatterToolStripButton scatterItem = tsItem as ScatterToolStripButton;
                    if (scatterItem != null)
                    {
                        if (scatterItem.ZqScatterType.Code == beatType)
                        {
                            scatterItem.UserSelected = true;
                            break;
                        }
                    }
                    else if (tsItem is ScatterToolStripSplitButton dropdownItem)
                    {
                        dropdownItem.CheckChildItem((subItem) => { return subItem.Text == beatType; });
                    }
                }
            }
            else if (this.tabControl1.SelectedIndex == 1)
            {
                string beatType = this.scatterTrend1.BaseBeatType;
                //if (this.scatterTrend1.IsLoaded)
                {
                    foreach (ToolStripItem tsItem in this.tsBeatType.Items)
                    {
                        ScatterToolStripButton scatterItem = tsItem as ScatterToolStripButton;
                        if (scatterItem != null)
                        {
                            if (scatterItem.ZqScatterType.Code == beatType)
                            {
                                scatterItem.UserSelected = true;
                                break;
                            }
                        }
                        else if (tsItem is ScatterToolStripSplitButton dropdownItem)
                        {
                            dropdownItem.CheckChildItem((subItem) => { return subItem.Text == beatType; });
                        }
                    }
                }
            }
            else if (this.tabControl1.SelectedIndex == 2)
            {
                string beatType = this.scatterPlotSplit1.BaseBeatType;
                foreach (ToolStripItem tsItem in this.tsBeatType.Items)
                {
                    ScatterToolStripButton scatterItem = tsItem as ScatterToolStripButton;
                    if (scatterItem != null)
                    {
                        if (scatterItem.ZqScatterType.Code == beatType)
                        {
                            scatterItem.UserSelected = true;
                            break;
                        }
                    }
                    else if (tsItem is ScatterToolStripSplitButton dropdownItem)
                    {
                        dropdownItem.CheckChildItem((subItem) => { return subItem.Text == beatType; });
                    }
                }
            }
        }

        #endregion


        public override void InitUI()
        {
            base.InitUI();

            try
            {
                this.scatterTrend1.SelectMode = 0; //显示24小时心搏
                this.scatterTrend2.SelectMode = 1; //基于时间段显示

                this.overlayPlot01.IsMainOverlay = true;
                this.overlayPlot01.IsActived = true;
                this.overlayPlot11.IsMainOverlay = true;
                this.overlayPlot11.IsActived = true;
                this.overlayPlot21.IsMainOverlay = true;
                this.overlayPlot21.IsActived = true;

                this.scatterPlot1.Width = this.scatterPlot1.Height + 34;
                this.overlayPlot01.Width = this.overlayPlot02.Width = this.overlayPlot03.Width = (int)(this.overlayPlot01.Height * 1.8);

                this.scatterPlotSplit1.Width = this.scatterPlotSplit1.Height + 34;
                this.scatterPlot2.Width = this.scatterPlot2.Height + 34;
                this.overlayPlot11.Width = this.overlayPlot12.Width = this.overlayPlot13.Width = (int)(this.overlayPlot11.Height * 1.8);

                this.scatterPlot3.Width = 0;
                this.overlayPlot21.Width = this.overlayPlot22.Width = this.overlayPlot23.Width = (int)(this.overlayPlot21.Height * 1.8);

                this.tsBeatType.Renderer = new CustomToolStripRenderer();

                registerEvents();

                Console.WriteLine("散点图界面已初始化完成");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "散点图");
            }
        }

        //注册事件
        private void registerEvents()
        {
            #region 全时散点图
            this.tabPage1.SizeChanged += (o, e) =>
            {
                this.scatterPlot1.Width = this.scatterPlot1.Height + 34;
                this.overlayPlot01.Width = this.overlayPlot02.Width = this.overlayPlot03.Width = (int)(this.overlayPlot01.Height * 1.8);
            };

            this.scatterPlot1.OnDataChanged += (d) => { getScatterTypeCount(); };
            this.scatterPlot1.OnBeatSelected += (sender, beats, keepSelection) =>
            {
                scatterPlot_BeatSelected(sender, beats, keepSelection, this.overlayPlot01, this.overlayPlot02, this.overlayPlot03);
            };
            this.scatterPlot1.OnSelectModeChanged += (selectMode) =>
            {
                this.overlayPlot01.SelectMode = selectMode;
                this.overlayPlot02.SelectMode = selectMode;
                this.overlayPlot03.SelectMode = selectMode;
            };
            this.overlayPlot01.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList1); };
            this.overlayPlot02.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList1); };
            this.overlayPlot03.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList1); };

            this.beatWaveList1.OnLayoutChanged += beatWaveList_OnLayoutChanged;
            this.beatWaveList1.OnBeatSelected += (sender, beats, selBeatsInPage) =>
            {
                if (beats.Count > 0)
                {
                    if (!(sender == this.ecgCommonWave1 && this.ecgCommonWave1.DataSourceIsChanged == false))
                    {
                        this.ecgCommonWave1.OnWaveformStartTimeChanged(beats[0].Sample);
                    }
                }
                this.scatterPlot1.MarkPositionOfBeats(selBeatsInPage);
            };
            this.beatWaveList1.OnLeadsChanged += (leads) =>
            {
                this.ecgCommonWave1.leadIdxs = leads;
                this.ecgCommonWave1.forceWaveformRedraw();
            };

            //通知从最上级控件开始更新界面数据
            BaseDrawingControl[] extendedDrawers1 = new BaseDrawingControl[] { overlayPlot01, overlayPlot02, overlayPlot03, beatWaveList1, ecgCommonWave1 };
            foreach (BaseDrawingControl ctl in extendedDrawers1)
            {
                ctl.OnDataChanged += (d) =>
                {
                    if (ctl != beatWaveList1 && ctl != ecgCommonWave1)
                    {
                        beatWaveList1.DataSourceIsChanged = true;
                    }

                    this.scatterPlot1.UpdateBeatSource(ctl, scatterPlot1.BaseBeatType, null);
                    getScatterTypeCount();
                };
            }
            #endregion

            #region 时间散点图
            this.tabPage2.SizeChanged += (o, e) =>
            {
                this.scatterPlot2.Width = this.scatterPlot2.Height + 34;
                this.overlayPlot11.Width = this.overlayPlot12.Width = this.overlayPlot13.Width = (int)(this.overlayPlot11.Height * 1.8);
            };
            //心拍趋势图
            this.scatterTrend1.OnRangeSelected += (sender, startTime, endTime, beats, keepSelection) =>
            {
                if (keepSelection)
                    this.scatterTrend2.UpdateBeatSource(sender, startTime, endTime, "", beats);
                else
                    this.scatterTrend2.SetBeatSource(startTime, endTime, beats, false);
            };
            this.scatterTrend2.OnRangeSelected += (sender, startTime, endTime, beats, keepSelection) =>
            {
                if (keepSelection)
                    this.scatterPlot2.UpdateBeatSource(sender, "", beats);
                else
                    this.scatterPlot2.SetBeatSource(beats);

                this.ecgCommonWave2.Invalidate();
            };
            this.scatterTrend2.OnBeatClicked += (beat) =>
            {
                //this.scatterPlot2.RefreshUI(true);
                this.ecgCommonWave2.OnWaveformStartTimeChanged(beat.Sample);
            };
            //散点图
            this.scatterPlot2.OnBeatSelected += (sender, beats, keepSelection) =>
            {
                scatterPlot_BeatSelected(sender, beats, keepSelection, this.overlayPlot11, this.overlayPlot12, this.overlayPlot13);
            };
            this.scatterPlot2.OnSelectModeChanged += (selectMode) =>
            {
                this.overlayPlot11.SelectMode = selectMode;
                this.overlayPlot12.SelectMode = selectMode;
                this.overlayPlot13.SelectMode = selectMode;
            };
            //叠加图
            this.overlayPlot11.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList2); };
            this.overlayPlot12.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList2); };
            this.overlayPlot13.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList2); };
            //心拍列表
            this.beatWaveList2.OnLayoutChanged += beatWaveList_OnLayoutChanged;
            this.beatWaveList2.OnBeatSelected += (sender, beats, pageSelBeats) =>
            {
                if (beats.Count > 0)
                {
                    if (!(sender == this.ecgCommonWave2 && this.ecgCommonWave2.DataSourceIsChanged == false))
                    {
                        this.ecgCommonWave2.OnWaveformStartTimeChanged(beats[0].Sample);
                    }
                }
                this.scatterPlot2.MarkPositionOfBeats(pageSelBeats);
            };
            this.beatWaveList2.OnCommonWaveToggled += (isVisible) =>
            {
                this.ecgCommonWave2.Visible = isVisible;
                this.beatWaveList2.RowCount = isVisible ? 5 : 8;
                this.beatWaveList2.RefreshUI(this.beatWaveList2, true);
            };
            this.beatWaveList2.OnLeadsChanged += (leads) =>
            {
                this.ecgCommonWave2.leadIdxs = leads;
                this.ecgCommonWave2.forceWaveformRedraw();
            };

            BaseDrawingControl[] extendedDrawers2 = new BaseDrawingControl[] { scatterTrend1, scatterTrend2, scatterPlot2, overlayPlot11, overlayPlot12, overlayPlot13, beatWaveList2, ecgCommonWave2 };
            foreach (BaseDrawingControl ctl in extendedDrawers2)
            {
                ctl.OnDataChanged += (d) =>
                {
                    if (ctl == scatterTrend1 || ctl == scatterTrend2)
                    {
                        List<AfInfo> afList = AnaCode.GetAfList();
                        this.scatterTrend1.AFList = afList;
                        this.scatterTrend2.AFList = afList;
                    }

                    if (ctl != beatWaveList2 && ctl != ecgCommonWave2)
                    {
                        beatWaveList2.DataSourceIsChanged = true;
                    }

                    //if (ctl == ecgCommonWave2) {
                    //    if (AEcgHandler.UpdateType != EcgUpdateType.ModifyBeats) {
                    //        this.scatterTrend1.UpdateBeatSource(scatterTrend1.StartTime, scatterTrend1.EndTime, scatterTrend1.BaseBeatType, null);
                    //        getScatterTypeCount();
                    //    }
                    //}
                    this.scatterTrend1.UpdateBeatSource(ctl, scatterTrend1.StartTime, scatterTrend1.EndTime, scatterTrend1.BaseBeatType, null);
                    getScatterTypeCount();
                };
            }
            #endregion

            #region 分时散点图
            this.tlpScatter3.SizeChanged += (o, e) =>
            {
                if (this.scatterPlotSplit1.Visible) this.scatterPlotSplit1.Width = this.scatterPlotSplit1.Height + 34;
                this.overlayPlot21.Width = this.overlayPlot22.Width = this.overlayPlot23.Width = (int)(this.overlayPlot21.Height * 1.8);
            };
            this.scatterPlotSplit1.OnVisibleChanged += (visible) =>
            {
                if (visible) this.scatterPlotSplit1.Width = this.scatterPlotSplit1.Height + 34;
                //this.overlayPlot21.Width = this.overlayPlot22.Width = this.overlayPlot23.Width = (int)(this.overlayPlot21.Height * 1.8);
                this.tlpScatter3.PerformLayout();
            };

            this.scatterPlotSplit1.OnBeatSelected += (sender, beats, keepSelection) =>
            {
                scatterPlotSplit_BeatSelected(sender, beats, keepSelection, this.overlayPlot21, this.overlayPlot22, this.overlayPlot23);
            };
            this.scatterPlotSplit1.OnSelectModeChanged += (selectMode) =>
            {
                this.overlayPlot21.SelectMode = selectMode;
                this.overlayPlot22.SelectMode = selectMode;
                this.overlayPlot23.SelectMode = selectMode;
            };

            this.scatterPlot3.OnDataChanged += (d) => { getScatterTypeCount(); };
            this.scatterPlot3.OnBeatSelected += (sender, beats, keepSelection) =>
            {
                scatterPlot_BeatSelected(sender, beats, keepSelection, this.overlayPlot21, this.overlayPlot22, this.overlayPlot23);
            };
            this.scatterPlot3.OnSelectModeChanged += (selectMode) =>
            {
                this.overlayPlot21.SelectMode = selectMode;
                this.overlayPlot22.SelectMode = selectMode;
                this.overlayPlot23.SelectMode = selectMode;
            };

            this.overlayPlot21.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList3); };
            this.overlayPlot22.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList3); };
            this.overlayPlot23.OnSelected += (sender, beats, keepSelection) => { overlayPlot_Selected(sender, beats, keepSelection, this.beatWaveList3); };

            this.beatWaveList3.OnLayoutChanged += beatWaveList_OnLayoutChanged;
            this.beatWaveList3.OnBeatSelected += (sender, beats, selBeatsInPage) =>
            {
                if (beats.Count > 0)
                {
                    if (!(sender == this.ecgCommonWave3 && this.ecgCommonWave3.DataSourceIsChanged == false))
                    {
                        this.ecgCommonWave3.OnWaveformStartTimeChanged(beats[0].Sample);
                    }
                }
                if (this.scatterPlot3.Visible) this.scatterPlot3.MarkPositionOfBeats(selBeatsInPage);
            };
            this.beatWaveList3.OnLeadsChanged += (leads) =>
            {
                this.ecgCommonWave3.leadIdxs = leads;
                this.ecgCommonWave3.forceWaveformRedraw();
            };

            BaseDrawingControl[] extendedDrawers3 = new BaseDrawingControl[] { overlayPlot21, overlayPlot22, overlayPlot23, beatWaveList3, ecgCommonWave3 };
            foreach (BaseDrawingControl ctl in extendedDrawers3)
            {
                ctl.OnDataChanged += (d) =>
                {
                    if (ctl != beatWaveList3 && ctl != ecgCommonWave3)
                    {
                        beatWaveList3.DataSourceIsChanged = true;
                    }

                    this.scatterPlotSplit1.UpdateBeatSource(ctl, scatterPlotSplit1.BaseBeatType, null);
                    if (this.scatterPlot3.Visible)
                    {
                        this.scatterPlot3.UpdateBeatSource(ctl, "", scatterPlotSplit1.TimeSpanBeats);
                    }
                    getScatterTypeCount();
                };
            }
            #endregion
        }

        //获取心搏类型及其个数
        private List<ScatterType> getScatterTypeCount()
        {
            var beatGroup = new List<ScatterType>();
            if (AnaValues.CachedAllBeats != null && AnaValues.CachedAllBeats.Count > 0)
            {
                string keys = AnaCode.GetKeysOfBeats(AnaValues.CachedAllBeats);

                int xCount = Regex.Matches(keys, "X").Count;
                //int aCount = keys.Length - xCount;
                int aCount = keys.Length;
                beatGroup.Add(new ScatterType { Title = "A", Code = "", Desc = _langs["全部"], Count = aCount });
                beatGroup.Add(new ScatterType { Title = "N", Code = "N", Desc = _langs["正常"], Count = Regex.Matches(keys, "N").Count });
                beatGroup.Add(new ScatterType { Title = "V", Code = "V", Desc = _langs["室早"], Count = Regex.Matches(keys, "[VT]").Count });
                beatGroup.Add(new ScatterType { Title = "V1", Code = "V1", Desc = _langs["单个室早"], Count = Regex.Matches(keys, @"(?<!V)V(?!V)").Count });
                beatGroup.Add(new ScatterType { Title = "V2", Code = "V2", Desc = _langs["成对室早"], Count = Regex.Matches(keys, @"(?<!V)VV(?!V)").Count });
                beatGroup.Add(new ScatterType { Title = "V3", Code = "V3", Desc = _langs["室速"], Count = Regex.Matches(keys, @"(?<!V)V{3,}(?!V)").Count });
                beatGroup.Add(new ScatterType { Title = "S", Code = "S", Desc = _langs["房早"], Count = Regex.Matches(keys, "S").Count });
                beatGroup.Add(new ScatterType { Title = "S1", Code = "S1", Desc = _langs["单个房早"], Count = Regex.Matches(keys, @"(?<!S)S(?!S)").Count });
                beatGroup.Add(new ScatterType { Title = "S2", Code = "S2", Desc = _langs["成对房早"], Count = Regex.Matches(keys, @"(?<!S)SS(?!S)").Count });
                beatGroup.Add(new ScatterType { Title = "S3", Code = "S3", Desc = _langs["房速"], Count = Regex.Matches(keys, @"(?<!S)S{3,}(?!S)").Count });
                beatGroup.Add(new ScatterType { Title = "Af", Code = "K", Desc = _langs["房颤"], Count = Regex.Matches(keys, "K").Count + Regex.Matches(keys, "M").Count });
                beatGroup.Add(new ScatterType { Title = "X", Code = "X", Desc = _langs["伪差"], Count = xCount });
                beatGroup.Add(new ScatterType { Title = "J", Code = "J", Desc = _langs["交界性早搏"], Count = Regex.Matches(keys, "J").Count });
                beatGroup.Add(new ScatterType { Title = "Je", Code = "W", Desc = _langs["房性逸博"], Count = Regex.Matches(keys, "W").Count });
                beatGroup.Add(new ScatterType { Title = "Ve", Code = "E", Desc = _langs["室性逸博"], Count = Regex.Matches(keys, "E").Count });
                beatGroup.Add(new ScatterType { Title = "Je", Code = "G", Desc = _langs["交界性逸博"], Count = Regex.Matches(keys, "G").Count });
                beatGroup.Add(new ScatterType { Title = "P", Code = "Pace", Desc = _langs["起搏"], Count = Regex.Matches(keys, "[OPH]").Count });
                beatGroup.Add(new ScatterType { Title = "aP", Code = "O", Desc = _langs["心房起搏"], Count = Regex.Matches(keys, "O").Count });
                beatGroup.Add(new ScatterType { Title = "vP", Code = "P", Desc = _langs["心室起搏"], Count = Regex.Matches(keys, "P").Count });
                beatGroup.Add(new ScatterType { Title = "avP", Code = "H", Desc = _langs["房室顺序起搏"], Count = Regex.Matches(keys, "H").Count });
            }

            this.tsBeatType.SuspendLayout();
            this.tsBeatType.Items.Clear();
            float fontSize = 9F * Math.Max(0.75F, CommonValues.HeightScale);
            using (Font textFont = new Font(this.tsBeatType.Font.FontFamily, fontSize))
            {
                int maxCount = beatGroup.Count > 0 ? beatGroup.Max(g => g.Count) : 0;
                ScatterToolStripButton tsItem = new ScatterToolStripButton(maxCount.ToString());
                tsItem.Font = textFont;
                int maxWidth = Math.Max(46, tsItem.GetPreferredSize(new Size(100, 40)).Width);

                for (int i = 0; i < beatGroup.Count; i++)
                {
                    ScatterType scatterType = beatGroup[i];
                    if (scatterType.Count > 0)
                    {
                        ToolStripItem item = null;
                        if (scatterType.Code == "N")
                        {
                            item = new ScatterToolStripSplitButton(scatterType.Count.ToString())
                            {
                                ZqScatterType = scatterType,
                                TitleFontSize = 14
                            };
                            var normalItem = item as ScatterToolStripSplitButton;
                            normalItem.ButtonClick += beatType_Click;
                            //normalItem.DropDownOpened += (o, e) =>
                            //{
                            //    var dropDownMenu = normalItem.DropDown as ToolStripDropDownMenu;
                            //    dropDownMenu.ShowImageMargin = false;
                            //    //dropDownMenu.ShowCheckMargin = false;
                            //    dropDownMenu.PerformLayout();
                            //};

                            string[] arrNormal = new string[] { "NN", "NV", "NS", "VN", "SN" };
                            foreach (string normalType in arrNormal)
                            {
                                ToolStripMenuItem subItem = new ToolStripMenuItem(normalType);
                                subItem.Click += beatType_Normal_Click;
                                normalItem.DropDownItems.Add(subItem);
                            }
                        }
                        else
                        {
                            item = new ScatterToolStripButton(scatterType.Count.ToString())
                            {
                                ZqScatterType = scatterType,
                                TitleFontSize = 14
                            };
                            item.Click += beatType_Click;
                        }
                        item.Name = "tsBtn" + scatterType.Code;
                        item.ToolTipText = scatterType.Desc;
                        item.ImageScaling = ToolStripItemImageScaling.None;
                        item.TextAlign = scatterType.Count >= 100000 ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleRight;
                        item.TextImageRelation = TextImageRelation.ImageAboveText;
                        item.AutoSize = false;
                        item.Size = new Size(maxWidth, 40);

                        this.tsBeatType.Items.Add(item);
                    }
                }
            }
            this.tsBeatType.ResumeLayout();
            this.tsBeatType.Refresh();

            if (selectedTypeIndex < this.tsBeatType.Items.Count)
            {
                (this.tsBeatType.Items[selectedTypeIndex] as IScatterToolStripItem).UserSelected = true;
            }

            return beatGroup;
        }

        private void refreshTabpageView(string BeatTypeCode)
        {
            if (this.tabControl1.SelectedIndex == 0)
            {
                this.scatterPlot1.SetBeatType(BeatTypeCode);
                this.overlayPlot01.SetBeatSource(null);
                this.overlayPlot02.SetBeatSource(null);
                this.overlayPlot03.SetBeatSource(null);
            }
            else if (this.tabControl1.SelectedIndex == 1)
            {
                this.beatWaveList2.DataSourceIsChanged = true;
                this.scatterTrend1.SetBeatType(BeatTypeCode, true);
                //this.scatterPlot2.SetBeatType(beatType);
                //this.overlayPlot11.SetBeatSource(null);
                this.overlayPlot12.SetBeatSource(null, false);
                this.overlayPlot13.SetBeatSource(null, false);
            }
            else if (this.tabControl1.SelectedIndex == 2)
            {
                this.scatterPlotSplit1.SetBeatType(BeatTypeCode);
                this.overlayPlot21.SetBeatSource(null);
                this.overlayPlot22.SetBeatSource(null);
                this.overlayPlot23.SetBeatSource(null);
                if (this.scatterPlot3.Visible) this.scatterPlot3.RestoreOriginalView();
            }
        }
    }
}
