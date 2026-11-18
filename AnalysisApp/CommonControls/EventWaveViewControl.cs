using AnalysisApp.Commons;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class EventWaveViewControl : BaseDrawingControl
    {
        private int rows { get; set; } = 3;              //行数
        private int cols { get; set; } = 3;              //列数
        private float rectWidth { get; set; } = 240f;         //每个矩形区域的宽度
        private float rectHeight { get; set; } = 60f;        //每个矩形区域的高度
        private int rectPadding { get; set; } = 4;      //每个矩形之间的间隙

        private int Pages { get; set; } = 0;                      //总页数
        public int PageIdx { get; set; } = 0;                    //当前页索引，从0开始
        private int PageWaveCount = 0;
        public int ClickRectIdx = 0;   //鼠标点击的模板当前页索引


        // 鼠标滚动防抖动相关变量
        private int pendingScrollPages;
        private DateTime _lastWheelTime = DateTime.MinValue;


        // 滚动条防抖动相关变量
        private int pendingScrollValue;
        private DateTime _lastScrollTime = DateTime.MinValue;


        private RectangleWaveType WaveType { get; set; } = RectangleWaveType.MultipleHeartbeat;//条图类型

        private float Gain = 5.0f;
        private float PixMM = 3.777f;

        private List<int> leadIdxs = new List<int>();

        private int ShiftStartRectIdx = -1;


        private List<bool> EventSelectStatus { get; set; } = new List<bool>();
        private List<SKRect> EventRects { get; set; } = new List<SKRect>();
        public List<AnaEvent.EventInfo> EventList = new List<AnaEvent.EventInfo>();
        public List<AnaEvent.EventInfo> PageEventList = new List<AnaEvent.EventInfo>();
        public string EventName = "";
        public Dictionary<string, int> EventCount = new Dictionary<string, int>();
        private List<int> EventMiddleSamples = new List<int>();

        private DrawRectangleWave drawRectangleWave = null;

        private string SegmentFilePath = "";
        private string DeletedEventsFilePath = "";

        private bool _isSelectAll = false;



        /// <summary>
        /// 点击事件
        /// </summary>
        public event Action<int> OnSampleChanged;

        public EventWaveViewControl()
        {
            InitializeComponent();
        }

        private void EventWaveViewControl_Load(object sender, EventArgs e)
        {
            WaveType = RectangleWaveType.MultipleHeartbeat;
            PageIdx = 0;
            leadIdxs.Clear();
            leadIdxs.Add(AnaValues.holterConfig.MainChannel);
            leadIdxs.Add(AnaValues.holterConfig.SecondChannel1);
            leadIdxs.Add(AnaValues.holterConfig.SecondChannel2);
            if (!DesignMode)
            {
                init();
                initControl();
                GetFocus();
            }
        }

        private void initData()
        {
            BeatWaveStyle layout = (BeatWaveStyle)this.cmbLayout.SelectedValue;
            leadIdxs.Clear();
            if (layout == BeatWaveStyle.OneBeat)
            {
                leadIdxs.Add(AnaValues.holterConfig.MainChannel);
            }
            else if (layout == BeatWaveStyle.OneLead)
            {
                leadIdxs.Add(AnaValues.holterConfig.MainChannel);
                leadIdxs.Add(AnaValues.holterConfig.SecondChannel1);
                leadIdxs.Add(AnaValues.holterConfig.SecondChannel2);
            }

            SegmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            DeletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);

            EventSelectStatus.Clear();

            Pages = EventList.Count % (rows * cols) != 0 ? (EventList.Count / (rows * cols) + 1) : EventList.Count / (rows * cols);
            if (PageIdx >= Pages)
            {
                PageIdx = Math.Max(0, Pages - 1);
            }
            UpdateScrollBarMax();

            PageWaveCount = rows * cols;

            InitPageData();
            InitPageStatus();

            this.lblPageInfo.Text = CommonValues.Langs["第"] + $"{PageIdx + 1}/{Pages}" + CommonValues.Langs["页"];

        }

        private void InitPageData()
        {
            PageEventList.Clear();

            for (int i = PageIdx * PageWaveCount; i < (PageIdx + 1) * PageWaveCount; i++)
            {
                if (i == EventList.Count) break;
                PageEventList.Add(EventList[i]);
            }

            if (ClickRectIdx > PageEventList.Count - 1)
            {
                ClickRectIdx = PageEventList.Count - 1;
            }
        }

        private void InitPageStatus()
        {
            for (int i = 0; i < PageEventList.Count; i++)
            {
                if (_isSelectAll)
                {
                    EventSelectStatus.Add(true);
                }
                else
                {
                    if (i == ClickRectIdx)
                    {
                        EventSelectStatus.Add(true);
                    }
                    else
                    {
                        EventSelectStatus.Add(false);
                    }
                }

            }
        }

        private void init()
        {
            if (!DesignMode)
            {
                CommonFuns.InitCommboBoxItems(this.cmbGain, SysConstants.Gains, defaultValue: 5.0F);
                CommonFuns.InitCommboBoxItems(this.cmbLayout, AnaValues.EventWaveLayouts, defaultValue: BeatWaveStyle.OneLead);

            }

            this.cmbGain.SelectedValueChanged += (o, e) =>
            {
                Gain = Convert.ToSingle(this.cmbGain.SelectedValue);
                _graphicsControl.Invalidate();
            };
            this.cmbLayout.SelectedValueChanged += (o, e) =>
            {
                BeatWaveStyle layout = (BeatWaveStyle)this.cmbLayout.SelectedValue;
                bool multiLeads = (layout == BeatWaveStyle.ThreeLeads || layout == BeatWaveStyle.ThreeLongLeads);

                LayoutStyle(layout);
            };
        }

        private void LayoutStyle(BeatWaveStyle layout)
        {
            if (layout == BeatWaveStyle.OneBeat)
            {
                rows = 6;
                cols = 3;
            }
            else if (layout == BeatWaveStyle.OneLead)
            {
                rows = 3;
                cols = 3;
            }

            UpdateDataForceWaveformRedraw();

        }

        public void ForceWaveformRedraw()
        {
            vScrollBar1.Value = Convert.ToInt32(PageIdx);
            _graphicsControl?.Refresh();

            List<BeatInfo> selectBeats = new List<BeatInfo>();

            for (int i = 0; i < EventSelectStatus.Count; i++)
            {
                if (EventSelectStatus[i] == true)
                {
                    int sample = EventMiddleSamples[i];
                    if (selectBeats.Count == 0)
                    {
                        OnSampleChanged?.Invoke(sample);
                    }
                    selectBeats.AddRange(AnaValues.CachedAllBeats.Where(b => b.EndIndex > sample && b.StartIndex <= sample).ToList());
                }
            }
            _selectedBeats = selectBeats;
        }

        public void UpdateDataForceWaveformRedraw()
        {
            initData();
            vScrollBar1.Value = Convert.ToInt32(PageIdx);
            _graphicsControl?.Refresh();

            List<BeatInfo> selectBeats = new List<BeatInfo>();
            for (int i = 0; i < EventSelectStatus.Count; i++)
            {
                if (EventSelectStatus[i] == true)
                {
                    if (EventMiddleSamples.Count - 1 < i)
                    {
                        break;
                    }
                    int sample = EventMiddleSamples[i];
                    //if(selectBeats.Count == 0)
                    //{
                    //    OnSampleChanged?.Invoke(sample);
                    //}
                    selectBeats.AddRange(AnaValues.CachedAllBeats.Where(b => b.EndIndex > sample && b.StartIndex <= sample).ToList());
                }
            }
            _selectedBeats = selectBeats;
        }

        private void initControl()
        {
            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            _graphicsControl.MouseDoubleClick += OnCanvasMouseDblClick;
            _graphicsControl.MouseWheel += OnCanvasMouseWheel;
            panel1.Controls.Add(_graphicsControl);

            // 初始化右键菜单
            InitializeContextMenu();

            drawRectangleWave = new DrawRectangleWave();

        }

        // 初始化右键菜单
        private void InitializeContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            //删除事件
            ToolStripMenuItem deleteEventMenuItem = new ToolStripMenuItem(CommonValues.Langs["删除"]);
            deleteEventMenuItem.Click += DeleteEventMenuItem_Click;
            contextMenu.Items.Add(deleteEventMenuItem);

            //接受事件
            ToolStripMenuItem acceptEventMenuItem = new ToolStripMenuItem(CommonValues.Langs["接受"]);
            acceptEventMenuItem.Click += AcceptEventMenuItem_Click;
            contextMenu.Items.Add(acceptEventMenuItem);

            ToolStripSeparator toolStripSeparator1 = new ToolStripSeparator();
            contextMenu.Items.Add(toolStripSeparator1);

            //保存事件
            ToolStripMenuItem saveEventMenuItem = new ToolStripMenuItem(CommonValues.Langs["保存片段"]);
            saveEventMenuItem.Click += SaveEventMenuItem_Click;
            _ = contextMenu.Items.Add(saveEventMenuItem);

            //取消保存事件
            ToolStripMenuItem cancelSaveEventMenuItem = new ToolStripMenuItem(CommonValues.Langs["取消保存"]);
            cancelSaveEventMenuItem.Click += CancelSaveEventMenuItem_Click;
            _ = contextMenu.Items.Add(cancelSaveEventMenuItem);


            // 添加右键菜单打开时的事件处理程序
            contextMenu.Opening += (sender, e) =>
            {
                // 对于"最快心率"、"最慢心率"、"最长RR间期"，将删除和接受菜单项置灰
                if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
                {
                    //deleteEventMenuItem.Enabled = false;
                    if (EventName == "最快心率" || EventName == "最慢心率")
                    {
                        acceptEventMenuItem.Enabled = false;
                    }
                    else
                    {
                        acceptEventMenuItem.Enabled = true;
                    }
                    cancelSaveEventMenuItem.Enabled = false;
                    // 获取当前选中的事件信息
                    if (PageEventList.Count > 0)
                    {
                        for (int i = 0; i < PageEventList.Count; i++)
                        {
                            if (EventSelectStatus[i] == true)
                            {
                                //是否已保存片段
                                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, PageEventList[i].Sample))
                                {
                                    saveEventMenuItem.Enabled = false;
                                    cancelSaveEventMenuItem.Enabled = true;
                                }
                                else
                                {
                                    saveEventMenuItem.Enabled = true;
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    cancelSaveEventMenuItem.Enabled = false;
                    acceptEventMenuItem.Enabled = false;

                    // 获取当前选中的事件信息
                    if (PageEventList.Count > 0)
                    {
                        for (int i = 0; i < PageEventList.Count; i++)
                        {
                            if (EventSelectStatus[i] == true)
                            {
                                // 获取当前选中事件的信息
                                AnaEvent.EventInfo currentEvent = PageEventList[i];
                                //if(currentEvent.Deleted)
                                //{
                                //    deleteEventMenuItem.Enabled = false;
                                //    saveEventMenuItem.Enabled = false;
                                //    cancelSaveEventMenuItem.Enabled = false;
                                //    acceptEventMenuItem.Enabled = true;
                                //}
                                //else
                                //{
                                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, PageEventList[i].Sample))
                                {
                                    deleteEventMenuItem.Enabled = true;
                                    saveEventMenuItem.Enabled = false;
                                    cancelSaveEventMenuItem.Enabled = true;
                                    //acceptEventMenuItem.Enabled = false;
                                }
                                else
                                {
                                    deleteEventMenuItem.Enabled = true;
                                    saveEventMenuItem.Enabled = true;
                                }
                                //}

                                break;
                            }
                        }
                    }

                }

            };

            _graphicsControl.ContextMenuStrip = contextMenu;
        }

        private void DeleteSelectST()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            int selectCount = 0;
            foreach (bool status in EventSelectStatus)
            {
                if (status)
                {
                    ++selectCount;
                }
            }
            var STEvents = new STEventRange[selectCount];
            int eventIdx = 0;
            for (int i = 0; i < PageEventList.Count; i++)
            {
                if (EventSelectStatus[i] == true)
                {
                    AnaEvent.EventInfo eventInfo = PageEventList[i];
                    STEvents[eventIdx] = new STEventRange((short)eventInfo.STLead, eventInfo.StartSample, eventInfo.EndSample);
                    ++eventIdx;
                    //eventInfo.Deleted = true;

                    if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, eventInfo.Sample))
                    {
                        segmentSamples.Add(eventInfo.Sample);
                    }
                }
            }
            AEcgHandler.DeleteSTEvents(STEvents);
            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void DeletePageST()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            var STEvents = new STEventRange[EventSelectStatus.Count];
            for (int i = 0; i < PageEventList.Count; i++)
            {
                AnaEvent.EventInfo eventInfo = PageEventList[i];
                STEvents[i] = new STEventRange((short)eventInfo.STLead, eventInfo.StartSample, eventInfo.EndSample);
                //eventInfo.Deleted = true;

                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, eventInfo.Sample))
                {
                    segmentSamples.Add(eventInfo.Sample);
                }
            }
            AEcgHandler.DeleteSTEvents(STEvents);
            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void DeleteAllST()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            var STEvents = new STEventRange[EventList.Count];
            for (int i = 0; i < EventList.Count; i++)
            {
                STEvents[i] = new STEventRange((short)EventList[i].STLead, EventList[i].StartSample, EventList[i].EndSample);

                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, EventList[i].Sample))
                {
                    segmentSamples.Add(EventList[i].Sample);
                }
            }
            AEcgHandler.DeleteSTEvents(STEvents);
            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void ModifySelectBeats()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < PageEventList.Count; i++)
            {
                if (EventSelectStatus[i] == true)
                {
                    AnaEvent.EventInfo eventInfo = PageEventList[i];

                    for (int j = 0; j < eventInfo.Samples.Count; j++)
                    {
                        samples.Add(eventInfo.Samples[j]);
                    }

                    if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, eventInfo.Sample))
                    {
                        segmentSamples.Add(eventInfo.Sample);
                    }
                }
            }
            AEcgHandler.ModifyBeats(samples.ToArray(), 'N');
            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void ModifyPageBeats()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < PageEventList.Count; i++)
            {
                AnaEvent.EventInfo eventInfo = PageEventList[i];

                for (int j = 0; j < eventInfo.Samples.Count; j++)
                {
                    samples.Add(eventInfo.Samples[j]);
                }
                //eventInfo.Deleted = true;

                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, eventInfo.Sample))
                {
                    segmentSamples.Add(eventInfo.Sample);
                }
            }
            AEcgHandler.ModifyBeats(samples.ToArray(), 'N');
            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void ModifyAllBeats()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < EventList.Count; i++)
            {
                for (int j = 0; j < EventList[i].Samples.Count; j++)
                {
                    samples.Add(EventList[i].Samples[j]);
                }
                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, EventList[i].Sample))
                {
                    segmentSamples.Add(EventList[i].Sample);
                }
            }

            AEcgHandler.ModifyBeats(samples.ToArray(), 'N');
            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }

        }

        //private void SetEventDeleteState(List<int> samples, string eventName)
        //{
        //    if (AnaEvent.AllEvents.ContainsKey(eventName))
        //    {
        //        List<AnaEvent.EventInfo> eventInfo = AnaEvent.AllEvents[eventName];
        //        for (int i = 0; i < eventInfo.Count; i++)
        //        {
        //            for (int j = 0; j < samples.Count; j++)
        //            {
        //                if (eventInfo[i].Sample == samples[j])
        //                {
        //                    eventInfo[i].Deleted = true;
        //                }
        //            }
        //        }
        //    }

        //}

        private void DeleteSelectHREvent()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < PageEventList.Count; i++)
            {
                if (EventSelectStatus[i] == true)
                {
                    AnaEvent.EventInfo eventInfo = PageEventList[i];

                    samples.Add(eventInfo.Sample);

                    if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, eventInfo.Sample))
                    {
                        segmentSamples.Add(eventInfo.Sample);
                    }
                }
            }
            if (EventName == "长RR间期" || EventName == "停搏" || EventName == "最长RR间期")
            {
                WriteDeleteInfo(samples, "长RR间期_D");
                WriteDeleteInfo(samples, "停搏_D");
                WriteDeleteInfo(samples, "最长RR间期_D");
            }
            else
            {
                WriteDeleteInfo(samples, EventName + "_D");
            }

            if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
            {
                AnaEvent.DelMaxEventInfo(EventName);
            }

            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void DeletePageHREvent()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < PageEventList.Count; i++)
            {
                AnaEvent.EventInfo eventInfo = PageEventList[i];

                samples.Add(eventInfo.Sample);

                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, eventInfo.Sample))
                {
                    segmentSamples.Add(eventInfo.Sample);
                }
            }

            if (EventName == "长RR间期" || EventName == "停搏" || EventName == "最长RR间期")
            {
                WriteDeleteInfo(samples, "长RR间期_D");
                WriteDeleteInfo(samples, "停搏_D");
                WriteDeleteInfo(samples, "最长RR间期_D");
            }
            else
            {
                WriteDeleteInfo(samples, EventName + "_D");
            }

            if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
            {
                AnaEvent.DelMaxEventInfo(EventName);
            }


            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void DeleteAllHREvent()
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < EventList.Count; i++)
            {
                samples.Add(EventList[i].Sample);

                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, EventList[i].Sample))
                {
                    segmentSamples.Add(EventList[i].Sample);
                }
            }

            if (EventName == "长RR间期" || EventName == "停搏" || EventName == "最长RR间期")
            {
                WriteDeleteInfo(samples, "长RR间期_D");
                WriteDeleteInfo(samples, "停搏_D");
                WriteDeleteInfo(samples, "最长RR间期_D");
            }
            else
            {
                WriteDeleteInfo(samples, EventName + "_D");
            }

            if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
            {
                AnaEvent.DelMaxEventInfo(EventName);
            }

            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void DeleteSelectNcaOrFBeatEvent(string eventName)
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < PageEventList.Count; i++)
            {
                if (EventSelectStatus[i] == true)
                {
                    AnaEvent.EventInfo eventInfo = PageEventList[i];

                    samples.Add(eventInfo.Sample);

                    if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, eventInfo.Sample))
                    {
                        segmentSamples.Add(eventInfo.Sample);
                    }
                }
            }
            AnaEvent.DeleteNcaOrFBeat(samples, eventName);
            if (eventName == "室性融合波")
            {
                //如果是室性融合波，删除操作还需要将心搏类型改为N
                AEcgHandler.ModifyBeats(samples.ToArray(), 'N');
            }

            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void DeletePageNcaOrFBeatEvent(string eventName)
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < PageEventList.Count; i++)
            {
                AnaEvent.EventInfo eventInfo = PageEventList[i];

                samples.Add(eventInfo.Sample);

                //eventInfo.Deleted = (delete == 1 ? true : false);

                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, eventInfo.Sample))
                {
                    segmentSamples.Add(eventInfo.Sample);
                }
            }
            AnaEvent.DeleteNcaOrFBeat(samples, eventName);
            if (eventName == "室性融合波")
            {
                //如果是室性融合波，删除操作还需要将心搏类型改为N
                AEcgHandler.ModifyBeats(samples.ToArray(), 'N');
            }

            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void DeleteAllNcaOrFBeatEvent(string eventName)
        {
            List<int> segmentSamples = new List<int>();
            List<int> samples = new List<int>();
            for (int i = 0; i < EventList.Count; i++)
            {
                samples.Add(EventList[i].Sample);

                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, EventList[i].Sample))
                {
                    segmentSamples.Add(EventList[i].Sample);
                }
            }

            AnaEvent.DeleteNcaOrFBeat(samples, eventName);
            if (eventName == "室性融合波")
            {
                //如果是室性融合波，删除操作还需要将心搏类型改为N
                AEcgHandler.ModifyBeats(samples.ToArray(), 'N');
            }

            if (segmentSamples.Count > 0)
            {
                OperateSegment.deleteSegmentBySample(SegmentFilePath, segmentSamples);
            }
        }

        private void WriteDeleteInfo(List<int> samples, string keyStr)
        {
            // 将删除的事件样本写入到DeletedEventsFilePath文件中
            if (samples.Count > 0 && !string.IsNullOrEmpty(DeletedEventsFilePath))
            {
                try
                {
                    // 读取现有JSON文件内容
                    JObject jsonData = new JObject();
                    if (File.Exists(DeletedEventsFilePath))
                    {
                        string existingContent = File.ReadAllText(DeletedEventsFilePath);
                        if (!string.IsNullOrEmpty(existingContent))
                        {
                            jsonData = JObject.Parse(existingContent);
                        }
                    }

                    // 添加当前事件类型的样本
                    if (!jsonData.ContainsKey(keyStr))
                    {
                        jsonData[keyStr] = new JArray();
                    }

                    JArray eventSamples = (JArray)jsonData[keyStr];
                    foreach (int sample in samples)
                    {
                        // 检查是否已存在相同样本值，避免重复写入
                        bool exists = false;
                        foreach (var existingSample in eventSamples)
                        {
                            if (existingSample.Value<int>() == sample)
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            eventSamples.Add(sample);
                        }
                    }

                    // 写入文件
                    File.WriteAllText(DeletedEventsFilePath, jsonData.ToString());
                }
                catch (Exception ex)
                {
                    // 记录错误但不中断程序执行
                    System.Diagnostics.Debug.WriteLine($"写入删除事件文件失败: {ex.Message}");
                }
            }
        }



        public void DeleteSelectEvents()
        {
            AnaEvent.UpdateEvent = EventName;

            switch (EventName)
            {
                case "ST":
                    DeleteSelectST();
                    break;
                case "单发房早":
                case "单发室早":
                case "单发交界早":
                case "成对房早":
                case "房性二联律":
                case "房性三联律":
                case "房速":
                case "成对室早":
                case "室性二联律":
                case "室性三联律":
                case "室速":
                case "成对交界早":
                case "交界二联律":
                case "交界三联律":
                case "交界速":
                case "室性逸搏":
                case "房性逸搏":
                case "交界性逸搏":
                case "R on T":
                    ModifySelectBeats();
                    break;
                case "停搏":
                case "长RR间期":
                case "最长RR间期":
                case "最快心率":
                case "最慢心率":
                case "心动过速":
                case "心动过缓":
                    DeleteSelectHREvent();
                    AnaCode.LoadStatisticsData();
                    break;
                case "房早未下传":
                case "室性融合波":
                    DeleteSelectNcaOrFBeatEvent(EventName);
                    AnaCode.LoadStatisticsData();
                    break;
            }
            base.ChangeDataSource(null);
        }

        public void DeletePageEvents()
        {
            AnaEvent.UpdateEvent = EventName;

            switch (EventName)
            {
                case "ST":
                    DeletePageST();
                    break;
                case "单发房早":
                case "单发室早":
                case "单发交界早":
                case "成对房早":
                case "房性二联律":
                case "房性三联律":
                case "房速":
                case "成对室早":
                case "室性二联律":
                case "室性三联律":
                case "室速":
                case "成对交界早":
                case "交界二联律":
                case "交界三联律":
                case "交界速":
                case "室性逸搏":
                case "房性逸搏":
                case "交界性逸搏":
                case "R on T":
                    ModifyPageBeats();
                    break;
                case "停搏":
                case "长RR间期":
                case "最长RR间期":
                case "最快心率":
                case "最慢心率":
                case "心动过速":
                case "心动过缓":
                    DeletePageHREvent();
                    AnaCode.LoadStatisticsData();
                    break;
                case "房早未下传":
                case "室性融合波":
                    DeletePageNcaOrFBeatEvent(EventName);
                    AnaCode.LoadStatisticsData();
                    break;
            }

            base.ChangeDataSource(null);
        }

        public void DeleteAllEvents()
        {
            AnaEvent.UpdateEvent = EventName;
            switch (EventName)
            {
                case "ST":
                    DeleteAllST();
                    break;
                case "单发房早":
                case "单发室早":
                case "单发交界早":
                case "成对房早":
                case "房性二联律":
                case "房性三联律":
                case "房速":
                case "成对室早":
                case "室性二联律":
                case "室性三联律":
                case "室速":
                case "成对交界早":
                case "交界二联律":
                case "交界三联律":
                case "交界速":
                case "室性逸搏":
                case "房性逸搏":
                case "交界性逸搏":
                case "R on T":
                    ModifyAllBeats();
                    break;
                case "停搏":
                case "长RR间期":
                case "最长RR间期":
                case "最快心率":
                case "最慢心率":
                case "心动过速":
                case "心动过缓":
                    DeleteAllHREvent();
                    AnaCode.LoadStatisticsData();
                    break;
                case "房早未下传":
                case "室性融合波":
                    DeleteAllNcaOrFBeatEvent(EventName);
                    AnaCode.LoadStatisticsData();
                    break;
            }
            base.ChangeDataSource(null);
        }

        private void DeleteEventMenuItem_Click(object sender, EventArgs e)
        {
            AnaEvent.UpdateEvent = EventName;

            switch (EventName)
            {
                case "ST":
                    if (_isSelectAll)
                    {
                        DeleteAllST();
                    }
                    else
                    {
                        DeleteSelectST();
                    }
                    break;
                case "单发房早":
                case "单发室早":
                case "单发交界早":
                case "成对房早":
                case "房性二联律":
                case "房性三联律":
                case "房速":
                case "成对室早":
                case "室性二联律":
                case "室性三联律":
                case "室速":
                case "成对交界早":
                case "交界二联律":
                case "交界三联律":
                case "交界速":
                case "室性逸搏":
                case "房性逸搏":
                case "交界性逸搏":
                case "R on T":
                    if (_isSelectAll)
                    {
                        ModifyAllBeats();
                    }
                    else
                    {
                        ModifySelectBeats();
                    }
                    break;
                case "停搏":
                case "长RR间期":
                case "最长RR间期":
                case "最快心率":
                case "最慢心率":
                case "心动过速":
                case "心动过缓":
                    if (_isSelectAll)
                    {
                        DeleteAllHREvent();
                    }
                    else
                    {
                        DeleteSelectHREvent();
                    }
                    AnaCode.LoadStatisticsData();
                    AnaValues.DataSourceState = 2;
                    break;
                case "房早未下传":
                case "室性融合波":
                    if (_isSelectAll)
                    {
                        DeleteAllNcaOrFBeatEvent(EventName);
                    }
                    else
                    {
                        DeleteSelectNcaOrFBeatEvent(EventName);
                    }
                    AnaCode.LoadStatisticsData();
                    break;
            }
            base.ChangeDataSource(null);
        }

        private async void SaveEventMenuItem_Click(object sender, EventArgs e)
        {
            if (EventSelectStatus.Count == 0) return;
            if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
            {
                OperateSegment.deleteSegmentByEventName(EventName);
            }
            if (_isSelectAll)
            {
                for (int i = 0; i < EventList.Count; i++)
                {
                    AnaEvent.EventInfo eventInfo = EventList[i];
                    SaveSegment(eventInfo);
                }
            }
            else
            {
                for (int i = 0; i < EventSelectStatus.Count; i++)
                {
                    AnaEvent.EventInfo eventInfo = PageEventList[i];
                    if (EventSelectStatus[i] == true)
                    {
                        SaveSegment(eventInfo);
                        if (i == 0 && (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期"))
                        {
                            AnaCode.WriteSaveInfo(EventName, eventInfo.Sample, eventInfo.AvgHr, eventInfo.AvgRR);
                        }
                    }
                }
            }

            if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
            {
                AnaEvent.UpdateEvent = EventName;
                AnaCode.LoadStatisticsData();
                while (AnaEvent.IsCalculating)
                {
                    await Task.Delay(10);
                }
                EventList = AnaEvent.AllEvents[EventName];
                PageIdx = 0;
                UpdateDataForceWaveformRedraw();
            }
            else
            {
                _graphicsControl?.Invalidate();
            }
            AnaValues.DataSourceState = 1;

        }

        private void SaveSegment(AnaEvent.EventInfo eventInfo)
        {

            JObject segmentInfoJson = new JObject();
            segmentInfoJson["cbxLabel"] = EventName;
            segmentInfoJson["tbxStartTime"] = eventInfo.StartTime;
            segmentInfoJson["cbxDuration"] = 7;

            int points = 7 * AnaValues.holterConfig.SampleRate;
            int startSample = 0;
            if (eventInfo.BeatSize == 1)
            {
                int sample = 0;
                if (EventName == "停搏" || EventName == "长RR间期" || EventName == "最长RR间期")
                {
                    sample = eventInfo.Sample - Convert.ToInt32((eventInfo.AvgRR / 1000f) * AnaValues.holterConfig.SampleRate / 2);
                }
                else
                {
                    sample = eventInfo.Sample;
                }
                startSample = sample - points / 2 > 0 ? sample - points / 2 : 0;
            }
            else
            {
                int eventStartSample = eventInfo.StartSample;
                int eventEndSample = eventStartSample + Convert.ToInt32(eventInfo.Duration * AnaValues.holterConfig.SampleRate);
                if (eventInfo.Duration > 4.5 && EventName.Contains("速"))
                {
                    startSample = eventStartSample - Convert.ToInt32(AnaValues.holterConfig.SampleRate * 2.0);
                }
                else
                {
                    startSample = eventStartSample - (points - (eventEndSample - eventStartSample)) / 2 > 0 ? eventStartSample - (points - (eventEndSample - eventStartSample)) / 2 : eventStartSample;
                }
            }
            startSample = Math.Max(0, startSample);
            segmentInfoJson["startSample"] = startSample;
            segmentInfoJson["cbxGain"] = 10.0f;
            segmentInfoJson["cbxDirection"] = 1;
            List<int> leadIdxs = new List<int>();
            for (int j = 0; j < 12; j++)
            {
                leadIdxs.Add(j);
            }
            segmentInfoJson["leadIdxs"] = JArray.FromObject(leadIdxs);
            segmentInfoJson["sample"] = eventInfo.Sample;
            if (EventName == "最快心率" || EventName == "最慢心率")
            {
                segmentInfoJson["tbxAvgHR"] = eventInfo.AvgHr;
            }
            else
            {
                List<BeatInfo> cachedAllBeats = AnaValues.CachedAllBeats;
                List<BeatInfo> relevantHeartbeats = cachedAllBeats
                .Where(h => (h.EndIndex > startSample && h.StartIndex < startSample + points) && !AnaCode.IsInvalidBeat(h))
                .OrderBy(h => h.StartIndex)
                .ToList();
                int AvgRR = 0;
                int TotalRR = 0;
                for (int j = 0; j < relevantHeartbeats.Count; j++)
                {
                    BeatInfo beatInfo = relevantHeartbeats[j];
                    TotalRR += beatInfo.RRInterval;
                }
                AvgRR = TotalRR / relevantHeartbeats.Count;
                segmentInfoJson["tbxAvgHR"] = 60000 / AvgRR;
            }
            segmentInfoJson["eventName"] = EventName;
            OperateSegment.ProcessHolterPicFile(segmentInfoJson);

            if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
            {
                AnaCode.WriteSaveInfo(EventName, eventInfo.Sample, eventInfo.AvgHr, eventInfo.AvgRR);
            }
        }

        private void refresh(string eventName)
        {
            if (eventName == "最快心率")
            {
                AnaEvent.GetFastestSlowestHrEvent(AnaValues.CachedAllBeats, AnaEvent.EventType.fastestHr);
            }
            else if (eventName == "最慢心率")
            {
                AnaEvent.GetFastestSlowestHrEvent(AnaValues.CachedAllBeats, AnaEvent.EventType.slowestHr);
            }
            else if (eventName == "最长RR间期")
            {
                AnaEvent.GetLongestRREvent(AnaValues.CachedAllBeats, AnaEvent.EventType.longestRR);
                AnaEvent.GetLongRRAndAsystoleEvent(AnaValues.CachedAllBeats, AnaEvent.EventType.longRR);
                AnaEvent.GetLongRRAndAsystoleEvent(AnaValues.CachedAllBeats, AnaEvent.EventType.asystole);
            }
            AnaValues.DataSourceState = 1;
        }

        private async void CancelSaveEventMenuItem_Click(object sender, EventArgs e)
        {
            if (EventSelectStatus.Count == 0) return;

            List<int> samples = new List<int>();
            if (_isSelectAll)
            {
                for (int i = 0; i < EventCount[EventName]; i++)
                {
                    samples.Add(EventList[i].Sample);
                }
            }
            else
            {
                for (int i = 0; i < EventSelectStatus.Count; i++)
                {
                    if (EventSelectStatus[i] == true)
                    {
                        samples.Add(PageEventList[i].Sample);
                    }
                }
            }


            OperateSegment.deleteSegmentBySample(SegmentFilePath, samples);

            //if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
            //{
            //    AnaCode.DeleteSaveInfo(EventName);
            //}

            if (EventName == "最快心率" || EventName == "最慢心率")
            {
                AnaEvent.DelMaxEventInfo(EventName);
            }

            if (EventName == "最快心率" || EventName == "最慢心率" || EventName == "最长RR间期")
            {
                AnaEvent.UpdateEvent = EventName;
                AnaCode.LoadStatisticsData();
                while (AnaEvent.IsCalculating)
                {
                    await Task.Delay(10);
                }
                EventList = AnaEvent.AllEvents[EventName];
                PageIdx = 0;
                UpdateDataForceWaveformRedraw();
            }
            else
            {
                _graphicsControl?.Invalidate();
            }
            AnaValues.DataSourceState = 1;

            //AnaCode.LoadStatisticsData();
            //_graphicsControl?.Invalidate();

        }

        private async void AcceptEventMenuItem_Click(object sender, EventArgs e)
        {
            if (EventSelectStatus.Count == 0) return;
            OperateSegment.deleteSegmentByEventName(EventName);

            for (int i = 0; i < EventSelectStatus.Count; i++)
            {
                AnaEvent.EventInfo eventInfo = PageEventList[i];

                if (EventSelectStatus[i] == true && EventName == "最长RR间期")
                {
                    AnaCode.WriteSaveInfo(EventName, eventInfo.Sample, eventInfo.AvgHr, eventInfo.AvgRR);
                    break;
                }
            }
            AnaEvent.UpdateEvent = EventName;
            AnaCode.LoadStatisticsData();

            while (AnaEvent.IsCalculating)
            {
                await Task.Delay(10);
            }

            PageIdx = 0;

            base.ChangeDataSource(null);
        }

        // 滚动条防抖动计时器事件处理
        private void ScrollBarScroll()
        {

            // 更新时间偏移
            PageIdx = pendingScrollValue;
            ClickRectIdx = 0;

            UpdateDataForceWaveformRedraw();

            this.btnSelectCurPage.BaseColor = Color.FromArgb(221, 231, 245);

        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

            if (DateTime.Now - _lastScrollTime < TimeSpan.FromMilliseconds(20)) return;

            // 根据滚动事件的类型处理
            if (e.Type == ScrollEventType.SmallIncrement) // 向下箭头
            {
                pendingScrollValue = Math.Min(++PageIdx, vScrollBar1.Maximum);
            }
            else if (e.Type == ScrollEventType.SmallDecrement) // 向上箭头
            {
                pendingScrollValue = Math.Max(--PageIdx, 0);
            }
            else if (e.Type == ScrollEventType.LargeDecrement) // 点击滚动条上方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = vScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.Y - 17) * 1.0f / vScrollBar1.Height) * vScrollBar1.Maximum);
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.LargeIncrement) // 点击滚动条下方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = vScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.Y + 17) * 1.0f / vScrollBar1.Height) * vScrollBar1.Maximum);
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.ThumbTrack || e.Type == ScrollEventType.ThumbPosition)
            {
                // 拖动滑块时，直接设置待处理值为滑块位置
                pendingScrollValue = Math.Max(0, Math.Min(e.NewValue, vScrollBar1.Maximum));
            }

            int eventIdx = PageIdx * PageWaveCount < EventList.Count ? PageIdx * PageWaveCount : EventList.Count - 1;
            int sample = EventList[eventIdx].Sample;
            OnSampleChanged?.Invoke(sample);

            this.btnSelectCurPage.BaseColor = Color.FromArgb(221, 231, 245);

            ScrollBarScroll();

            _lastScrollTime = DateTime.Now;

        }

        #region 鼠标事件
        private bool isMouseDown = false;
        private Point mouseDownPoint;
        private Rectangle selectionRect = Rectangle.Empty;
        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            mouseDownPoint = e.Location;
            selectionRect = new Rectangle(e.X, e.Y, 0, 0);
            if (e.Button == MouseButtons.Left)
            {
                resetSelect();
                ClickRectIdx = FindRectIdx(e.X, e.Y);
                if (ClickRectIdx >= 0)
                {
                    // 检查是否按住Ctrl键
                    bool isCtrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                    // 检查是否按住Shift键
                    bool isShiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

                    if (isCtrlPressed)
                    {
                        // Ctrl+点击：切换选择状态（多选）
                        EventSelectStatus[ClickRectIdx] = !EventSelectStatus[ClickRectIdx];
                    }
                    else if (isShiftPressed)
                    {
                        ShiftMultiSelection(ClickRectIdx);
                    }
                    else
                    {
                        ShiftStartRectIdx = ClickRectIdx;
                        int sample = EventMiddleSamples[ClickRectIdx];
                        OnSampleChanged?.Invoke(sample);

                        // 普通点击：清除所有选择，只选择当前项
                        for (int i = 0; i < EventSelectStatus.Count; i++)
                        {
                            EventSelectStatus[i] = false;
                        }
                        EventSelectStatus[ClickRectIdx] = true;
                    }

                }
                //else
                //{
                //    // 点击空白区域：如果没有按住Ctrl键，清除所有选择
                //    bool isCtrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                //    if (!isCtrlPressed)
                //    {
                //        for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                //        {
                //            PageSubTemplateSelectStatus[i] = false;
                //        }
                //    }
                //}
                _graphicsControl.Invalidate();

            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && e.Button == MouseButtons.Left)
            {
                // 更新选择矩形
                int x = Math.Min(mouseDownPoint.X, e.X);
                int y = Math.Min(mouseDownPoint.Y, e.Y);
                int width = Math.Abs(e.X - mouseDownPoint.X);
                int height = Math.Abs(e.Y - mouseDownPoint.Y);

                selectionRect = new Rectangle(x, y, width, height);

                // 执行多选逻辑
                PerformMultiSelection(e);

                // 强制重绘以显示选择矩形
                _graphicsControl.Invalidate();

            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (isMouseDown && e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
                selectionRect = Rectangle.Empty;
                _graphicsControl?.Invalidate();
                List<BeatInfo> selectBeats = new List<BeatInfo>();

                for (int i = 0; i < EventSelectStatus.Count; i++)
                {
                    if (EventSelectStatus[i] == true)
                    {
                        AnaEvent.EventInfo eventInfo = PageEventList[i];
                        for (int j = 0; j < eventInfo.Samples.Count; j++)
                        {
                            int beatIndex = AnaValues.CurrentBeatMap[eventInfo.Samples[j]];
                            selectBeats.Add(AnaValues.CachedAllBeats[beatIndex]);
                        }
                    }
                }
                _selectedBeats = selectBeats;

            }
        }

        private void OnCanvasMouseDblClick(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            mouseDownPoint = e.Location;
            selectionRect = new Rectangle(e.X, e.Y, 0, 0);
            if (e.Button == MouseButtons.Left)
            {
                resetSelect();
                int clickRectIdx = FindRectIdx(e.X, e.Y);
                if (clickRectIdx >= 0)
                {

                    ShiftStartRectIdx = clickRectIdx;

                    int eventIdx = clickRectIdx + PageIdx * PageWaveCount;
                    int sample = EventMiddleSamples[clickRectIdx];

                    FullWaveForm.Instance.Show(sample, this.FindForm(), null, this.Parent.Parent.Parent.Parent);

                    // 普通点击：清除所有选择，只选择当前项
                    for (int i = 0; i < EventSelectStatus.Count; i++)
                    {
                        EventSelectStatus[i] = false;
                    }
                    EventSelectStatus[clickRectIdx] = true;

                }
                _graphicsControl.Invalidate();

            }
        }

        private int FindRectIdx(float x, float y)
        {
            int rectIdx = -1;
            for (int i = 0; i < EventRects.Count; i++)
            {
                SKRect rect = EventRects[i];
                if ((x > rect.Left && x < rect.Right) && (y > rect.Top && y < rect.Bottom))
                {
                    rectIdx = i;
                    break;
                }
            }
            return rectIdx;
        }

        private void ShiftMultiSelection(int clickRectIdx)
        {
            if (clickRectIdx < ShiftStartRectIdx)
            {
                for (int i = 0; i < EventSelectStatus.Count; i++)
                {
                    if (i < clickRectIdx || i > ShiftStartRectIdx)
                    {
                        EventSelectStatus[i] = false;
                    }
                    else
                    {
                        EventSelectStatus[i] = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < EventSelectStatus.Count; i++)
                {
                    if (i > clickRectIdx || i < ShiftStartRectIdx)
                    {
                        EventSelectStatus[i] = false;
                    }
                    else
                    {
                        EventSelectStatus[i] = true;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多选逻辑
        /// </summary>
        private void PerformMultiSelection(MouseEventArgs e)
        {
            // 如果拖拽距离很小，认为是单击，不执行多选
            if (Math.Abs(e.X - mouseDownPoint.X) < 5 && Math.Abs(e.Y - mouseDownPoint.Y) < 5)
            {
                return;
            }

            // 清除所有选择状态
            for (int i = 0; i < EventSelectStatus.Count; i++)
            {
                EventSelectStatus[i] = false;
            }

            // 选择与选择矩形相交的所有矩形
            for (int i = 0; i < EventRects.Count; i++)
            {
                SKRect rect = EventRects[i];
                Rectangle winRect = new Rectangle((int)rect.Left, (int)rect.Top,
                                                 (int)(rect.Right - rect.Left),
                                                 (int)(rect.Bottom - rect.Top));

                if (selectionRect.IntersectsWith(winRect))
                {
                    EventSelectStatus[i] = true;
                }
            }
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (Pages < 2)
            {
                return;
            }

            if (DateTime.Now - _lastWheelTime < TimeSpan.FromMilliseconds(20)) return;

            // 计算滚动的秒数（根据滚动方向和当前显示行数）
            int scrollPages = e.Delta > 0 ? -1 : 1;

            // 累积待处理的滚动秒数
            pendingScrollPages += scrollPages;

            if (pendingScrollPages != 0)
            {
                // 更新时间偏移
                PageIdx += pendingScrollPages;
                PageIdx = Math.Max(0, Math.Min(PageIdx, vScrollBar1.Maximum));
                // 更新滚动条位置
                vScrollBar1.Value = Convert.ToInt32(PageIdx);

                ClickRectIdx = 0;

                UpdateDataForceWaveformRedraw();

                // 重置待处理滚动页数
                pendingScrollPages = 0;

                int eventIdx = PageIdx * PageWaveCount < EventList.Count ? PageIdx * PageWaveCount : EventList.Count - 1;
                int sample = EventList[eventIdx].Sample;
                OnSampleChanged?.Invoke(sample);

            }

            _lastWheelTime = DateTime.Now;

        }
        #endregion

        // 更新滚动条的最大值
        private void UpdateScrollBarMax()
        {
            if (Pages > 1)
            {
                vScrollBar1.Show();
            }
            else
            {
                vScrollBar1.Hide();
            }

            vScrollBar1.Maximum = Math.Max(0, Pages - 1);
            if (Pages > 10)
            {
                vScrollBar1.LargeChange = Pages / 10;
            }
            else
            {
                vScrollBar1.LargeChange = 1;
            }

        }

        public void GetFocus()
        {
            _graphicsControl?.Focus();
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            EventRects.Clear();
            EventMiddleSamples.Clear();


            rectWidth = (imageWidth - rectPadding * (cols + 1)) / cols;
            float allWidth = (rectWidth + rectPadding) * cols;
            if (allWidth < imageWidth)
            {
                rectWidth += (imageWidth - allWidth) / (cols + 1);
            }

            rectHeight = (imageHeight - rectPadding * (rows + 1)) / rows;
            drawRectangleWave.WaveType = WaveType;
            drawRectangleWave.Gain = Gain;
            // 清除画布为白色背景
            canvas.Clear(SKColors.White);
            drawRectangleWave.BottomInfoList.Clear();
            drawRectangleWave.BottomSTInfoList.Clear();
            int eventIdx = 0;
            //Dictionary<int, bool> hasPic = OperateSegment.EventSegmentExist(SegmentFilePath, EventList);
            for (int i = PageIdx * PageWaveCount; i < (PageIdx + 1) * PageWaveCount; i++)
            {
                if (eventIdx >= PageEventList.Count)
                {
                    break;
                }
                int rowIdx = (i - PageIdx * PageWaveCount) / cols;
                int colIdx = (i - PageIdx * PageWaveCount) % cols;
                SKRect waveRect = new SKRect(colIdx * rectWidth + (colIdx + 1) * rectPadding, rowIdx * rectHeight + (rowIdx + 1) * rectPadding, (colIdx + 1) * rectWidth + (colIdx + 1) * rectPadding, (rowIdx + 1) * rectHeight + (rowIdx + 1) * rectPadding);
                drawRectangleWave.WaveRect = waveRect;
                drawRectangleWave.DrawRect(canvas, EventSelectStatus[i - PageIdx * PageWaveCount]);
                drawRectangleWave.leadIdxs = leadIdxs;

                EventRects.Add(waveRect);

                drawRectangleWave.WaveLeftMargin = 3 * PixMM;
                drawRectangleWave.WaveRightMargin = 2;

                drawRectangleWave.Speed = (rectWidth - drawRectangleWave.WaveLeftMargin - drawRectangleWave.WaveRightMargin) / PixMM / 7;

                int points = 7 * AnaValues.holterConfig.SampleRate;
                int sample = 0;
                int startSample = 0;
                int endSample = 0;

                if (PageEventList[eventIdx].BeatSize == 1)
                {
                    if (EventName == "停搏" || EventName == "长RR间期" || EventName == "最长RR间期")
                    {
                        sample = PageEventList[eventIdx].Sample - Convert.ToInt32((PageEventList[eventIdx].AvgRR / 1000f) * AnaValues.holterConfig.SampleRate / 2);
                    }
                    else
                    {
                        sample = PageEventList[eventIdx].Sample;
                    }
                    startSample = sample - points / 2 > 0 ? sample - points / 2 : 0;
                    endSample = sample + points / 2 < AnaValues.holterConfig.Total ? sample + points / 2 : AnaValues.holterConfig.Total - 1;
                }
                else
                {
                    int eventStartSample = PageEventList[eventIdx].StartSample;
                    int eventEndSample = eventStartSample + Convert.ToInt32(PageEventList[eventIdx].Duration * AnaValues.holterConfig.SampleRate);
                    if (PageEventList[eventIdx].Duration > 4.5 && EventName.Contains("速"))
                    {
                        startSample = eventStartSample - Convert.ToInt32(AnaValues.holterConfig.SampleRate * 2.0);
                    }
                    else
                    {
                        startSample = eventStartSample - (points - (eventEndSample - eventStartSample)) / 2 > 0 ? eventStartSample - (points - (eventEndSample - eventStartSample)) / 2 : eventStartSample;
                    }
                    startSample = Math.Max(0, startSample);
                    endSample = startSample + points;
                }

                endSample = Math.Min(endSample, AnaValues.holterConfig.Total - 1);

                EventMiddleSamples.Add(startSample + points / 2);

                var leadsWaveInfo = AnaCode.ReadAllWaveData(leadIdxs, startSample, endSample);
                List<float[]> EcgData = leadsWaveInfo.Select(o => o.WaveDataF).ToList(); ;

                drawRectangleWave.EcgData = EcgData;
                drawRectangleWave.StartSample = startSample;

                List<string> bottomInfoList = new List<string>();
                bottomInfoList.Add(PageEventList[eventIdx].StartTime);
                switch (EventName)
                {
                    case "停搏":
                    case "长RR间期":
                    case "最长RR间期":
                        double avgRR = Math.Round((double)PageEventList[eventIdx].AvgRR / 1000f, 2);
                        //double avgRR = PageEventList[eventIdx].AvgRR / 1000f;
                        bottomInfoList.Add($"时长：{avgRR.ToString()} s");
                        break;
                    case "最快心率":
                    case "最慢心率":
                    case "心动过速":
                    case "心动过缓":
                    case "ST":
                        bottomInfoList.Add($"心率：{PageEventList[eventIdx].AvgHr} bpm");
                        break;
                    default:
                        bottomInfoList.Add("");
                        break;
                }
                if (OperateSegment.SegmentExistByNameSample(SegmentFilePath, EventName, PageEventList[eventIdx].Sample))
                {
                    bottomInfoList.Add(CommonValues.Langs["已保存"]);
                }
                //else
                //{
                //    if(EventName == "最长RR间期" || EventName == "最快心率" || EventName == "最慢心率")
                //    {
                //        if(i != 0)
                //        {
                //            bottomInfoList.Add(CommonValues.Langs["已删除"]);
                //        }
                //    }
                //    else
                //    {
                //        if(PageEventList[eventIdx].Deleted)
                //        {
                //            bottomInfoList.Add(CommonValues.Langs["已删除"]);
                //        }
                //    }
                //}
                drawRectangleWave.BottomInfoList = bottomInfoList;
                if (EventName == "ST")
                {
                    List<string> bottomSTInfoList = new List<string>();
                    bottomSTInfoList.Add($"ST：{PageEventList[eventIdx].STPointVoltage.ToString()} mV");
                    bottomSTInfoList.Add(CommonValues.Langs["通道"] + $":{AnaValues.LeadIdxToNameMap[PageEventList[eventIdx].STLead]}");
                    drawRectangleWave.BottomSTInfoList = bottomSTInfoList;
                }

                drawRectangleWave.DrawEcgWave(canvas);
                ++eventIdx;

            }
        }

        //重置选择
        public void resetSelect()
        {
            _isSelectAll = false;
            this.btnSelectCurPage.BaseColor = Color.FromArgb(221, 231, 245);
            this.btnSelectAll.BaseColor = Color.FromArgb(221, 231, 245);

            _selectedBeats = null;
        }

        private void btnSelectCurPage_Click(object sender, EventArgs e)
        {
            resetSelect();

            this.btnSelectCurPage.BaseColor = Color.FromArgb(159, 200, 255);

            for (int i = 0; i < EventSelectStatus.Count; i++)
            {
                EventSelectStatus[i] = true;
            }

            _graphicsControl?.Invalidate();
            GetFocus();

            List<BeatInfo> selectBeats = new List<BeatInfo>();
            for (int i = 0; i < PageEventList.Count; i++)
            {
                AnaEvent.EventInfo eventInfo = PageEventList[i];
                for (int j = 0; j < eventInfo.Samples.Count; j++)
                {
                    selectBeats.Add(AnaValues.CachedAllBeats.Find(b => b.Sample == eventInfo.Samples[j]));
                }
            }
            _selectedBeats = selectBeats;
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            resetSelect();
            _isSelectAll = true;
            this.btnSelectAll.BaseColor = Color.FromArgb(159, 200, 255);

            for (int i = 0; i < EventSelectStatus.Count; i++)
            {
                EventSelectStatus[i] = true;
            }

            _graphicsControl?.Invalidate();
            GetFocus();
            List<BeatInfo> selectBeats = new List<BeatInfo>();
            for (int i = 0; i < EventList.Count; i++)
            {
                for (int j = 0; j < EventList[i].Samples.Count; j++)
                {
                    selectBeats.Add(AnaValues.CachedAllBeats.Find(b => b.Sample == EventList[i].Samples[j]));
                }
            }

            _selectedBeats = selectBeats;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isSelectAll)
                {
                    DeleteAllEvents();
                }
                else
                {
                    DeleteSelectEvents();
                }
                resetSelect();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "事件");
            }

        }

        private void btnDeletePage_Click(object sender, EventArgs e)
        {
            DeletePageEvents();
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            DeleteAllEvents();
        }

        public void SelectBeatChanged(Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                if ((ClickRectIdx <= cols && ClickRectIdx / cols == 0) && PageIdx == 0)
                {
                    return;
                }
                ClickRectIdx -= cols;
            }
            else if (keyData == Keys.Down)
            {
                ClickRectIdx += cols;

            }
            else if (keyData == Keys.Left)
            {
                ClickRectIdx--;
            }
            else if (keyData == Keys.Right)
            {
                ClickRectIdx++;
            }


            if (ClickRectIdx >= rows * cols)
            {
                ClickRectIdx -= rows * cols;

                for (int i = 0; i < EventSelectStatus.Count; i++)
                {
                    EventSelectStatus[i] = false;
                }
                EventSelectStatus[ClickRectIdx] = true;
                PageIdx++;
                UpdateDataForceWaveformRedraw();
            }
            else if (ClickRectIdx < 0)
            {
                if (PageIdx > 0)
                {
                    ClickRectIdx += rows * cols;
                    PageIdx--;

                    initData();
                    for (int i = 0; i < EventSelectStatus.Count; i++)
                    {
                        EventSelectStatus[i] = false;
                    }
                    EventSelectStatus[ClickRectIdx] = true;
                    ForceWaveformRedraw();

                }
            }
            else
            {
                ClickRectIdx = Math.Min(EventSelectStatus.Count - 1, ClickRectIdx);

                for (int i = 0; i < EventSelectStatus.Count; i++)
                {
                    EventSelectStatus[i] = false;
                }
                EventSelectStatus[ClickRectIdx] = true;
                ForceWaveformRedraw();
            }
        }

        protected override void OnControlKeyDown(Keys keyData)
        {
            SelectBeatChanged(keyData);
            return;
        }

        public void UpdateCommonWave()
        {
            if (ClickRectIdx < 0 || EventMiddleSamples.Count == 0) return;
            int sample = EventMiddleSamples[ClickRectIdx];
            OnSampleChanged?.Invoke(sample);
        }
    }
}
