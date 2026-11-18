using AnalysisApp.CommonControls;
using AnalysisApp.Commons;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp
{
    public partial class AfPage : BaseUserControl
    {
        private bool _isFirstLoad = true;

        public AfPage()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            await base.RefreshData(resetUI);
            if (this.scatterTrend1.IsLoaded) refreshPage(this, resetUI);
            if (this.ecgCommonWave2.IsLoaded)
            {
                this.ecgCommonWave2.RefreshData(resetUI);
            }
            //await Task.CompletedTask;
        }

        #region 事件
        private void ScatterPage_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (!IsUiInitialized) InitUI();
                    //loadAfTable(true);

                    Console.WriteLine("房颤界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "房颤房扑");
            }
        }

        private void ScatterPage_SizeChanged(object sender, EventArgs e)
        {
            //this.scatterPlot1.Width = this.scatterPlot1.Height + 34;
            //this.overlayPlot01.Width = this.overlayPlot02.Width = this.overlayPlot03.Width = (int)(this.overlayPlot01.Height * 1.8);

            //this.scatterPlot2.Width = this.scatterPlot2.Height + 34;
            //this.overlayPlot11.Width = this.overlayPlot12.Width = this.overlayPlot13.Width = (int)(this.overlayPlot11.Height * 1.8);

        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex >= 0)
            {
                DataGridViewCell curCell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                AfInfo rowData = dgv.Rows[e.RowIndex].DataBoundItem as AfInfo;
                if (e.ColumnIndex == 0) //复选框列
                {
                    if (curCell is DataGridViewCheckBoxCell)
                    {
                        if (curCell.Value == null)
                        {
                            curCell.Value = true;
                        }
                        else
                        {
                            bool isChecked = (bool)curCell.Value;
                            if (isChecked)
                            {
                                curCell.Value = false;
                                dgv.Columns[0].HeaderCell.Value = false;
                            }
                            else
                            {
                                curCell.Value = true;
                            }
                        }
                        rowData.IsSelected = (bool)curCell.Value;

                        dgv.InvalidateCell(e.ColumnIndex, e.RowIndex);
                    }
                }
            }
        }

        private void dgvData_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow dgvRow = this.dgvData.Rows[e.RowIndex];
            AfInfo afInfo = dgvRow.DataBoundItem as AfInfo;
            this.scatterTrend1.LocateToTime(afInfo.StartTime, (float)afInfo.EndTime.Subtract(afInfo.StartTime).TotalSeconds, true);
            this.ecgCommonWave2.OnWaveformStartTimeChanged(AnaCode.getSampleFromDateTime(afInfo.StartTime));
        }

        private void dgvData_SelectionChanged(object sender, EventArgs e)
        {
            //if (this.dgvData.SelectedRows.Count == 0) return;
            //if (!this.dgvData.ContainsFocus) return;

            //DataGridViewRow dgvRow = this.dgvData.SelectedRows[0];
            //AfInfo afInfo = dgvRow.DataBoundItem as AfInfo;
            //this.scatterTrend1.LocateToTime(afInfo.StartTime, (float)afInfo.EndTime.Subtract(afInfo.StartTime).TotalSeconds, true);
            //this.ecgCommonWave2.OnWaveformStartTimeChanged(AnaCode.getSampleFromDateTime(afInfo.StartTime));
        }

        //删除房颤
        private void btnDel_Click(object sender, EventArgs e)
        {
            List<AfInfo> lstSelected = new List<AfInfo>();
            foreach (DataGridViewRow row in dgvData.Rows)
            {
                DataGridViewCell cell0 = row.Cells[0];
                if (cell0 is DataGridViewCheckBoxCell && cell0.Value is bool)
                {
                    if ((bool)cell0.Value == true) lstSelected.Add(row.DataBoundItem as AfInfo);
                }
            }

            if (lstSelected.Count == 0)
            {
                //CommonFuns.PromptError("请至少选择一条记录", isWarning: true);
                ScreenCenterTip.Show("请至少选择一条记录");
                return;
            }

            var segments = new SegmentRange[lstSelected.Count];
            for (int i = 0; i < lstSelected.Count; i++)
            {
                AfInfo af = lstSelected[i];
                //segments[i] = AnaCode.GetSegment(af.StartTime, af.EndTime);
                segments[i] = new SegmentRange(af.StartSample, af.EndSample);
            }
            int ret75 = AEcgHandler.DeleteAFEvents(segments, 'K');
            int ret77 = AEcgHandler.DeleteAFEvents(segments, 'M');
            if (ret75 == 0 && ret77 == 0)
            {
                refreshPage(sender);
            }
            else
            {
                CommonFuns.PromptError("操作失败");
            }
        }

        //检测房颤
        private void btnDetect_Click(object sender, EventArgs e)
        {
            //AEcgHandler.FunctionPercents[AEcgHandler.AEcgFuncType.AecgDetectAF] = 0;
            //Func<object, Task> funAnalysis = async (args) =>
            //{
            //    dynamic paraInfo = args as ExpandoObject;
            //    ProgressForm frm = paraInfo.Form as ProgressForm;

            //    int ret = AEcgHandler.DetectAF();
            //    if (ret == 0)
            //    {
            //        while (true)
            //        {
            //            await Task.Delay(100);
            //            int percent = AEcgHandler.FunctionPercents[AEcgHandler.AEcgFuncType.AecgDetectAF];

            //            frm.InvokeIfRequired(() =>
            //            {
            //                if (percent < 100)
            //                {
            //                    frm.Progress = percent;
            //                    frm.Refresh();
            //                }
            //                else
            //                {
            //                    frm.Hide();
            //                }
            //            });
            //            if (percent == 100)
            //            {
            //                refreshPage();
            //                break;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        ScreenCenterTooltip.Show("操作失败");
            //    }
            //};
            //AppCode.ShowProgress(_langs["房颤检测中···"], this.FindForm(), funAnalysis, null,
            //        new Rectangle(PointToScreen(this.Location), this.Size));

            int ret = AEcgHandler.DetectAF();
            if (ret == 0)
            {
                refreshPage(sender);
                if (this.scatterTrend1.AFList == null || this.scatterTrend1.AFList.Count == 0)
                {
                    ScreenCenterTip.Show("未检测到房颤");
                }
            }
            else
            {
                ScreenCenterTip.Show("操作失败");
            }
        }

        //去除房颤内的房早
        private void btnExcluedAPB_Click(object sender, EventArgs e)
        {
            ConcurrentBag<int> samplesK = new ConcurrentBag<int>();
            var lstAF = AEcgHandler.GetAfSegments('K');
            Parallel.ForEach(lstAF, af =>
            {
                foreach (BeatInfo beat in AnaValues.CachedAllBeats)
                {
                    if (beat.Sample >= af.StartIndex && beat.Sample <= af.EndIndex)
                    {
                        if (beat.BeatUiType == 'S') samplesK.Add(beat.Sample);
                    }
                }
            });
            if (samplesK.Count > 0)
            {
                int retK = AEcgHandler.ModifyBeats(samplesK.ToArray(), 'K');
                if (retK != 0)
                {
                    ScreenCenterTip.Show("操作失败");
                    return;
                }
            }

            ConcurrentBag<int> samplesM = new ConcurrentBag<int>();
            var lstAFL = AEcgHandler.GetAfSegments('M');
            Parallel.ForEach(lstAFL, af =>
            {
                foreach (BeatInfo beat in AnaValues.CachedAllBeats)
                {
                    if (beat.Sample >= af.StartIndex && beat.Sample <= af.EndIndex)
                    {
                        if (beat.BeatUiType == 'S') samplesM.Add(beat.Sample);
                    }
                }
            });
            if (samplesM.Count > 0)
            {
                int retM = AEcgHandler.ModifyBeats(samplesM.ToArray(), 'M');
                if (retM != 0)
                {
                    ScreenCenterTip.Show("操作失败");
                    return;
                }
            }

            ScreenCenterTip.Show("操作成功");
        }

        #endregion

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                initGrid();

                this.scatterTrend1.SelectMode = 0; //显示24小时心搏
                this.scatterTrend2.SelectMode = 1; //基于时间段显示

                this.scatterPlot2.Width = this.scatterPlot2.Height;
                this.scatterPlot2.HideContextMenuStrip();

                registerEvents();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "房颤房扑");
            }
        }

        private void initGrid()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvData);
            this.dgvData.ColumnHeadersHeight = 28;
            this.dgvData.RowTemplate.Height = 28;

            //复选框
            DataGridViewCheckBoxColumn col0 = this.dgvData.AddCheckBoxHeaderColumn((colIndex, state) =>
            {
                this.dgvData.EndEdit();
                this.dgvData.Rows.OfType<DataGridViewRow>().ToList().ForEach(t => { t.Cells[colIndex].Value = state; });
            });

            //添加内容列
            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="StartTime", HeaderText="起始时间", DataPropertyName="StartTime" },
                //new DataGridViewTextBoxColumn { Name="EndTime",  HeaderText="终止时间", DataPropertyName="EndTime" },
                new DataGridViewTextBoxColumn { Name="Duration", HeaderText=_langs["持续时间"], DataPropertyName="Duration" },
                new DataGridViewTextBoxColumn { Name="TypeName", HeaderText="事件类型", DataPropertyName="TypeName" },
            };
            foreach (DataGridViewColumn col in cols) { col.SortMode = DataGridViewColumnSortMode.NotSortable; }
            this.dgvData.Columns.AddRange(cols);

            //this.dgvData.Columns["ExaminationTime"].DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;
            //this.dgvData.Columns["Duration"].DefaultCellStyle.Format = "F2";
            this.dgvData.CustomDateTimeFormat = "MM-dd HH:mm:ss";

            BindingCollection<AfInfo> bindingList = new BindingCollection<AfInfo>(new List<AfInfo>(), this);
            this.dgvData.DataSource = bindingList;

            this.dgvData.CellContentClick += dgvData_CellContentClick;
        }


        //注册事件
        private void registerEvents()
        {
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
                //this.scatterPlot2.RefreshUI();
                this.ecgCommonWave2.OnWaveformStartTimeChanged(beat.Sample);
            };

            BaseDrawingControl[] extendedDrawers2 = new BaseDrawingControl[] { scatterTrend1, scatterTrend2, scatterPlot2, ecgCommonWave2 };
            foreach (BaseDrawingControl ctl in extendedDrawers2)
            {
                ctl.OnDataChanged += (d) => { refreshPage(ctl); };
            }
        }

        private List<AfInfo> loadAfTable()
        {
            var afs = AnaCode.GetAfList();

            BindingCollection<AfInfo> bindingList = new BindingCollection<AfInfo>(afs, this);
            this.dgvData.DataSource = bindingList;

            if (_isFirstLoad)
            {
                //this.dgvData.AutoResizeColumns();
                this.dgvData.ResizeColumnWidth(new List<int>() { 12, 50, 40, 30 });
            }
            _isFirstLoad = false;

            return afs;
        }

        private void refreshPage(object sender, bool reset = false)
        {
            List<AfInfo> afList = loadAfTable();
            this.scatterTrend1.AFList = afList;
            this.scatterTrend2.AFList = afList;
            DateTime startTime = scatterTrend1.StartTime, endTime = scatterTrend1.EndTime;
            if (reset)
            {
                this.scatterTrend1.SetBeatType("");
            }
            else
            {
                this.scatterTrend1.UpdateBeatSource(sender, startTime, endTime, scatterTrend1.BaseBeatType, null);
            }

            this.ecgCommonWave2.forceWaveformRedraw();
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // 格式化日期
            if (e.ColumnIndex == dgvData.Columns["Duration"].Index)
            {
                if (e.Value != null)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds((double)e.Value);
                    e.Value = AnaCode.FormatTimeSpanTo24Hour(timeSpan);
                }
            }
        }
    }
}
