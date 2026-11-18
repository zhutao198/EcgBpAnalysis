using AnalysisApp.CommonControls;
using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AnalysisApp
{
    public partial class TemplateOverlayForm : BaseDialogForm
    {
        public static TemplateOverlayForm Instance { get; private set; }
        private List<BeatInfo> Beats;

        TemplatePageControl templatePageControl = null;
        char BeatType = 'N';

        public TemplateOverlayForm() : base("", "叠加图")
        {
            InitializeComponent();
            // 设置窗口为全屏模式但不遮挡任务栏
            this.FormBorderStyle = FormBorderStyle.None;
            //// 设置窗口大小为工作区大小（不包括任务栏）
            //this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            this.WindowState = FormWindowState.Maximized;
            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            // 设置窗口位置为屏幕左上角
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;
            Instance = this;
        }

        public void Show(List<BeatInfo> beats, char beatType, Control templatePageControl, IWin32Window owner = null)
        {
            Beats = beats;
            if (owner != null)
                Show(owner);
            else
                Show();
            if (this.overlayPlot01.IsLoaded)
            {
                this.overlayPlot01.SetBeatSource(beats, true);
            }
            if (this.scatterPlot1.IsLoaded)
            {
                this.scatterPlot1.SetBeatType(beatType.ToString());
                this.scatterPlot1.SetBeatSource(beats);

            }
            if (this.scatterPlot2.IsLoaded)
            {
                this.scatterPlot2.SetBeatType(beatType.ToString());
                this.scatterPlot2.removeAllBeats();
            }
            if (this.scatterPlot3.IsLoaded)
            {
                this.scatterPlot3.SetBeatType(beatType.ToString());
                this.scatterPlot3.removeAllBeats();
            }
            if (this.beatWaveList1.IsLoaded)
            {
                this.beatWaveList1.RowCount = 6;
                this.beatWaveList1.ColCount = 4;
                this.beatWaveList1.SetBeatSource(beats);
            }
            if (this.ecgCommonWave1.IsLoaded)
            {
                this.ecgCommonWave1.RefreshData(true);
                this.ecgCommonWave1.OnWaveformStartTimeChanged(beats[0].Sample);
            }
            this.BeatType = beatType;
            this.templatePageControl = templatePageControl as TemplatePageControl;
        }

        private void TemplateOverlayForm_Load(object sender, EventArgs e)
        {
            this.overlayPlot01.IsMainOverlay = true;
            this.overlayPlot01.IsActived = true;

            this.scatterPlot1._bTemplateOverlay = true;
            this.scatterPlot2._bTemplateOverlay = true;
            this.scatterPlot3._bTemplateOverlay = true;


            this.scatterPlot1.Width = this.scatterPlot1.Height;
            this.scatterPlot2.Width = this.scatterPlot1.Height;
            this.scatterPlot3.Width = this.scatterPlot1.Height;
            this.overlayPlot01.Width = this.overlayPlot02.Width = this.overlayPlot03.Width = (int)(this.overlayPlot01.Height * 2.0);

            registerEvents();
        }

        private void TemplateOverlayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            this.templatePageControl.TemplatePageRefresh();
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
                    ctlBeatList.RowCount = 6;
                    ctlBeatList.ColCount = 4;
                    break;
                case BeatWaveStyle.ThreeLeads:
                    ctlBeatList.RowCount = 4;
                    ctlBeatList.ColCount = 4;
                    break;
                case BeatWaveStyle.OneLongLead:
                    ctlBeatList.RowCount = 8;
                    ctlBeatList.ColCount = 4;
                    break;
                case BeatWaveStyle.ThreeLongLeads:
                    ctlBeatList.RowCount = 4;
                    ctlBeatList.ColCount = 4;
                    break;
            }
            ctlBeatList.RefreshUI(ctlBeatList, true);
        }

        private void scatterPlot_BeatSelected(object sender, List<BeatInfo> beats, bool isModify, BeatWaveList ctlWavelist)
        {
            if (isModify)
                ctlWavelist.UpdateBeatSource(sender, beats, isModify);
            else
                ctlWavelist.SetBeatSource(beats);
        }
        private void overlayPlot_Cuted(List<BeatInfo> beats, bool isModify, string dstPlotName, ScatterPlot srcScatterPlot)
        {
            srcScatterPlot.removeBeats(beats);

            if (dstPlotName.Contains("1"))
            {
                this.scatterPlot1.addBeats(beats);
            }
            else if (dstPlotName.Contains("2"))
            {
                this.scatterPlot2.addBeats(beats);
            }
            else
            {
                this.scatterPlot3.addBeats(beats);
            }
        }

        //注册事件
        private void registerEvents()
        {
            this.scatterPlot1.OnSelectModeChanged += (selectMode) =>
            {
                this.overlayPlot01.SelectMode = selectMode;
                this.overlayPlot02.SelectMode = selectMode;
                this.overlayPlot03.SelectMode = selectMode;
            };
            this.scatterPlot1.OnBeatSelected += (sender, beats, keepSelection) => { scatterPlot_BeatSelected(this.scatterPlot1, beats, keepSelection, this.beatWaveList1); };
            this.scatterPlot2.OnBeatSelected += (sender, beats, keepSelection) => { scatterPlot_BeatSelected(this.scatterPlot2, beats, keepSelection, this.beatWaveList1); };
            this.scatterPlot3.OnBeatSelected += (sender, beats, keepSelection) => { scatterPlot_BeatSelected(this.scatterPlot3, beats, keepSelection, this.beatWaveList1); };

            this.overlayPlot01.OnSelected += (sender, beats, keepSelection) => { scatterPlot_BeatSelected(sender, beats, keepSelection, this.beatWaveList1); };
            this.overlayPlot02.OnSelected += (sender, beats, keepSelection) => { scatterPlot_BeatSelected(sender, beats, keepSelection, this.beatWaveList1); };
            this.overlayPlot03.OnSelected += (sender, beats, keepSelection) => { scatterPlot_BeatSelected(sender, beats, keepSelection, this.beatWaveList1); };

            this.overlayPlot01.OnCuted += (beats, keepSelection, dstPlotName) => { overlayPlot_Cuted(beats, keepSelection, dstPlotName, this.scatterPlot1); };
            this.overlayPlot02.OnCuted += (beats, keepSelection, dstPlotName) => { overlayPlot_Cuted(beats, keepSelection, dstPlotName, this.scatterPlot2); };
            this.overlayPlot03.OnCuted += (beats, keepSelection, dstPlotName) => { overlayPlot_Cuted(beats, keepSelection, dstPlotName, this.scatterPlot3); };


            this.beatWaveList1.OnLayoutChanged += beatWaveList_OnLayoutChanged;
            this.beatWaveList1.OnBeatSelected += (sender, beats, selBeatsInPage) =>
            {
                if (beats.Count > 0) { this.ecgCommonWave1.OnWaveformStartTimeChanged(beats[0].Sample); }
                //this.scatterPlot1.MarkPositionOfBeats(selBeatsInPage);
            };
            this.beatWaveList1.OnLeadsChanged += (leads) =>
            {
                this.ecgCommonWave1.leadIdxs = leads;
                this.ecgCommonWave1.forceWaveformRedraw();
            };

            this.beatWaveList1.OnDataChanged += (d) =>
            {
                int[] samples = d as int[];
                OverlayPlot selectOverlay = null;
                int activedIndex = 0;
                int i = 0;
                foreach (OverlayPlot child in tlpOverlay1.Controls)
                {
                    // 处理每个子控件
                    if (child.IsActived)
                    {
                        child.DataSourceChanged(samples);
                        activedIndex = i;
                        selectOverlay = child;
                        break;
                    }
                    ++i;
                }

                //当前叠加图中没有心搏时，选中其它有心搏的叠加图
                if (selectOverlay.GetBeatSourceCount() == 0)
                {
                    foreach (OverlayPlot child in tlpOverlay1.Controls)
                    {
                        // 处理每个子控件
                        if (child.GetBeatSourceCount() != 0)
                        {
                            child.Selected();
                            break;
                        }

                    }
                }

                int j = 0;
                foreach (ScatterPlot child in tableLayoutPanel1.Controls)
                {
                    // 处理每个子控件
                    if (j == activedIndex)
                    {
                        child.DataSourceChanged(samples);
                        break;
                    }
                    ++j;
                }

            };

            //通知从最上级控件开始更新界面数据
            this.overlayPlot01.OnDataChanged += (d) =>
            {
                int[] samples = d as int[];
                this.overlayPlot01.DataSourceChanged(samples);
                this.scatterPlot1.DataSourceChanged(samples);
                this.beatWaveList1.DataSourceIsChanged = true;
                this.beatWaveList1.DataSourceChanged(samples);

                if (overlayPlot01.GetBeatSourceCount() == 0)
                {
                    foreach (OverlayPlot child in tlpOverlay1.Controls)
                    {
                        // 处理每个子控件
                        if (child.GetBeatSourceCount() != 0)
                        {
                            child.Selected();
                            break;
                        }
                    }
                }

            };

            this.overlayPlot02.OnDataChanged += (d) =>
            {
                int[] samples = d as int[];
                this.overlayPlot02.DataSourceChanged(samples);
                this.scatterPlot2.DataSourceChanged(samples);
                this.beatWaveList1.DataSourceIsChanged = true;
                this.beatWaveList1.DataSourceChanged(samples);

                if (overlayPlot02.GetBeatSourceCount() == 0)
                {
                    foreach (OverlayPlot child in tlpOverlay1.Controls)
                    {
                        // 处理每个子控件
                        if (child.GetBeatSourceCount() != 0)
                        {
                            child.Selected();
                            break;
                        }
                    }
                }

            };

            this.overlayPlot03.OnDataChanged += (d) =>
            {
                int[] samples = d as int[];
                this.overlayPlot03.DataSourceChanged(samples);
                this.scatterPlot3.DataSourceChanged(samples);
                this.beatWaveList1.DataSourceIsChanged = true;
                this.beatWaveList1.DataSourceChanged(samples);

                if (overlayPlot03.GetBeatSourceCount() == 0)
                {
                    foreach (OverlayPlot child in tlpOverlay1.Controls)
                    {
                        // 处理每个子控件
                        if (child.GetBeatSourceCount() != 0)
                        {

                            child.Selected();
                            break;
                        }
                    }
                }
            };

            this.scatterPlot1.OnDataChanged += (d) =>
            {
                int[] samples = d as int[];
                this.overlayPlot01.DataSourceChanged(samples);
                this.beatWaveList1.DataSourceIsChanged = true;
                this.beatWaveList1.DataSourceChanged(samples);
            };

            this.scatterPlot2.OnDataChanged += (d) =>
            {
                int[] samples = d as int[];
                this.overlayPlot02.DataSourceChanged(samples);
                this.beatWaveList1.DataSourceIsChanged = true;
                this.beatWaveList1.DataSourceChanged(samples);
            };

            this.scatterPlot3.OnDataChanged += (d) =>
            {
                int[] samples = d as int[];
                this.overlayPlot03.DataSourceChanged(samples);
                this.beatWaveList1.DataSourceIsChanged = true;
                this.beatWaveList1.DataSourceChanged(samples);
            };

            //BaseDrawingControl[] extendedDrawers1 = new BaseDrawingControl[] { overlayPlot01, overlayPlot02, overlayPlot03, beatWaveList1, ecgCommonWave1 };
            //foreach (BaseDrawingControl ctl in extendedDrawers1)
            //{
            //    ctl.OnDataChanged += () =>
            //    {
            //        if (ctl != beatWaveList1 &&
            //            (ctl != ecgCommonWave1 || AEcgHandler.UpdateType != EcgUpdateType.ModifyBeats))
            //        {
            //            beatWaveList1.DataSourceIsChanged = true;
            //        }

            //        this.scatterPlot1.UpdateBeatSource(scatterPlot1.BaseBeatType, null);

            //    };
            //}
        }
    }
}
