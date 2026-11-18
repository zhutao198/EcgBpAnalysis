using AnalysisApp.Commons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class SegmentWaveViewControl : BaseDrawingControl
    {
        private DrawRectangleWave drawRectangleWave = null;

        private const float PixMM = 3.777f;             //每毫米3.777个像素点
        private bool bShowGrid = false;

        private float WaveLeftMargin = 3 * PixMM;
        private float WaveRightMargin = 2;

        public string segmentFilePath = "";                     // 片段图文件路径
        List<JObject> SegmentJsonArray = new List<JObject>();   // 片段图信息

        private List<List<float[]>> PageSegmentEcgData = new List<List<float[]>>(); //当前页面所有片段图数据 每个子列表代表一个片段图数据
        private List<List<int>> PageSegmentLeadIdxs = new List<List<int>>();
        private List<int> PageSegmentStartSamples = new List<int>();
        private List<int> PageSegmentShowStartSamples = new List<int>();
        private List<int> PageSegmentDurations = new List<int>();
        private List<float> PageSegmentGains { get; set; } = new List<float>();                       //所有片段图增益信息
        private List<List<string>> PageSegmentBottomInfoLists { get; set; } = new List<List<string>>();
        private List<SKRect> PageSegmentRects { get; set; } = new List<SKRect>();                            //心搏信息
        private List<bool> PageSegmentSelectStatus { get; set; } = new List<bool>();
        private List<int> SegmentMiddleSamples = new List<int>();


        private int width { get; set; } = 0;            //绘图宽度
        private int height { get; set; } = 0;          //绘图高度度

        private int rows { get; set; } = 2;              //行数
        private int cols { get; set; } = 3;              //列数
        private float rectWidth { get; set; } = 240f;         //每个矩形区域的宽度
        private float rectHeight { get; set; } = 60f;        //每个矩形区域的高度
        private int rectPadding { get; set; } = 4;      //每个矩形之间的间隙

        private int Pages { get; set; } = 0;                      //总页数
        private int PageIdx { get; set; } = 0;                    //当前页索引，从0开始
        private int PageWaveCount { get; set; } = 0;
        private int ClickRectIdx { get; set; } = 0;
        // 鼠标滚动防抖动相关变量
        private int pendingScrollPages;
        private DateTime _lastWheelTime = DateTime.MinValue;

        // 滚动条防抖动相关变量
        private int pendingScrollValue;
        private DateTime _lastScrollTime = DateTime.MinValue;

        private int ShiftStartRectIdx = -1;

        ContextMenuStrip contextMenu = new ContextMenuStrip();


        public event Action OnDeleteSegment;

        public SegmentWaveViewControl()
        {
            InitializeComponent();
        }

        private void SegmentWaveViewControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (AnaValues.CurrentCase == null) return;
                try
                {
                    segmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
                    initControl();

                    readPicInfo();
                    InitializeEcgData();
                    //drawRectangleWave = new DrawRectangleWave();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载错误: {ex.Message}\n\n堆栈跟踪: {ex.StackTrace}");
                }
            }
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


        // 滚动条防抖动计时器事件处理
        private void ScrollBarScroll()
        {
            // 更新时间偏移
            PageIdx = pendingScrollValue;
            ClickRectIdx = 0;

            ForceWaveformRedraw();
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            // 清除画布
            canvas.Clear(SKColors.Transparent);

            SegmentMiddleSamples.Clear();
            // 确保离屏缓冲区已更新
            width = imageWidth;
            height = imageHeight;

            rectWidth = (width - rectPadding * (cols + 1)) / cols;
            rectHeight = (height - rectPadding * (rows + 1)) / rows;
            drawRectangleWave.WaveType = RectangleWaveType.Segment;
            drawRectangleWave.DrawBackground(canvas, width, height);
            for (int i = 0; i < PageSegmentEcgData.Count; i++)
            {
                int rowIdx = i / cols;
                int colIdx = i % cols;
                int dataCount = 0;
                for (int j = 0; j < PageSegmentEcgData[i].Count; j++)
                {
                    if (PageSegmentEcgData[i][j].Length > 0)
                    {
                        dataCount = PageSegmentEcgData[i][j].Length;
                        break;
                    }
                }
                PageSegmentRects[i] = new SKRect(colIdx * rectWidth + (colIdx + 1) * rectPadding, rowIdx * rectHeight + (rowIdx + 1) * rectPadding, (colIdx + 1) * rectWidth + (colIdx + 1) * rectPadding, (rowIdx + 1) * rectHeight + (rowIdx + 1) * rectPadding);
                drawRectangleWave.WaveRect = PageSegmentRects[i];
                drawRectangleWave.DrawRect(canvas, PageSegmentSelectStatus[i]);
                drawRectangleWave.Speed = (rectWidth - WaveLeftMargin - WaveRightMargin) / PixMM / (dataCount / AnaValues.holterConfig.SampleRate);
                drawRectangleWave.Gain = PageSegmentGains[i];
                drawRectangleWave.WaveLeftMargin = WaveLeftMargin;
                drawRectangleWave.WaveRightMargin = WaveRightMargin;
                drawRectangleWave.EcgData = PageSegmentEcgData[i];
                drawRectangleWave.leadIdxs = PageSegmentLeadIdxs[i];
                drawRectangleWave.StartSample = PageSegmentShowStartSamples[i];
                drawRectangleWave.BottomInfoList = PageSegmentBottomInfoLists[i];

                SegmentMiddleSamples.Add(Convert.ToInt32(PageSegmentShowStartSamples[i] + PageSegmentDurations[i]/2.0f * AnaValues.holterConfig.SampleRate));
                if (bShowGrid)
                {
                    drawRectangleWave.DrawGrid(canvas);
                }

                drawRectangleWave.DrawEcgWave(canvas);
            }
        }

        private int FindRectIdx(float x, float y)
        {
            int rectIdx = -1;
            for (int i = 0; i < PageSegmentRects.Count; i++)
            {
                SKRect rect = PageSegmentRects[i];
                if ((x > rect.Left && x < rect.Right) && (y > rect.Top && y < rect.Bottom))
                {
                    rectIdx = i;
                    break;
                }
            }
            return rectIdx;
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

            // 如果是单击（非拖拽），执行选择逻辑
            if (e.Button == MouseButtons.Left)
            {
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
                        PageSegmentSelectStatus[ClickRectIdx] = !PageSegmentSelectStatus[ClickRectIdx];
                    }
                    else if (isShiftPressed)
                    {
                        ShiftMultiSelection(ClickRectIdx);
                    }
                    else
                    {
                        ShiftStartRectIdx = ClickRectIdx;
                        // 普通点击：清除所有选择，只选择当前项
                        for (int i = 0; i < PageSegmentSelectStatus.Count; i++)
                        {
                            PageSegmentSelectStatus[i] = false;
                        }
                        PageSegmentSelectStatus[ClickRectIdx] = true;
                    }
                }
                //else
                //{
                //    // 点击空白区域：如果没有按住Ctrl键，清除所有选择
                //    bool isCtrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                //    if (!isCtrlPressed)
                //    {
                //        for (int i = 0; i < PageSegmentSelectStatus.Count; i++)
                //        {
                //            PageSegmentSelectStatus[i] = false;
                //        }
                //    }
                //}
                reRendWave();
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
                reRendWave();
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (isMouseDown && e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
                selectionRect = Rectangle.Empty;
                reRendWave(); // 清除选择矩形
            }
        }

        private void OnCanvasMouseDblClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int clickRectIdx = FindRectIdx(e.X, e.Y);
                if (clickRectIdx >= 0)
                {
                    int sample = SegmentMiddleSamples[clickRectIdx];
                    FullWaveForm.Instance.Show(sample, this.FindForm(), this.Parent.Parent.Parent);
                }
            }
        }

        private void ShiftMultiSelection(int clickRectIdx)
        {
            if (clickRectIdx < ShiftStartRectIdx)
            {
                for (int i = 0; i < PageSegmentSelectStatus.Count; i++)
                {
                    if (i < clickRectIdx || i > ShiftStartRectIdx)
                    {
                        PageSegmentSelectStatus[i] = false;
                    }
                    else
                    {
                        PageSegmentSelectStatus[i] = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < PageSegmentSelectStatus.Count; i++)
                {
                    if (i > clickRectIdx || i < ShiftStartRectIdx)
                    {
                        PageSegmentSelectStatus[i] = false;
                    }
                    else
                    {
                        PageSegmentSelectStatus[i] = true;
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
            for (int i = 0; i < PageSegmentSelectStatus.Count; i++)
            {
                PageSegmentSelectStatus[i] = false;
            }

            // 选择与选择矩形相交的所有矩形
            for (int i = 0; i < PageSegmentRects.Count; i++)
            {
                SKRect rect = PageSegmentRects[i];
                Rectangle winRect = new Rectangle((int)rect.Left, (int)rect.Top,
                                                 (int)(rect.Right - rect.Left),
                                                 (int)(rect.Bottom - rect.Top));

                if (selectionRect.IntersectsWith(winRect))
                {
                    PageSegmentSelectStatus[i] = true;
                }
            }
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastWheelTime < TimeSpan.FromMilliseconds(20)) return;

            if (Pages < 2)
            {
                return;
            }
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
                ForceWaveformRedraw();

                // 重置待处理滚动页数
                pendingScrollPages = 0;
            }

            _lastWheelTime = DateTime.Now;

        }
        #endregion

        #region 鼠标右键菜单功能
        // 初始化右键菜单
        private void InitializeContextMenu()
        {
            // 添加菜单项
            ToolStripMenuItem deleteSegmentItem = new ToolStripMenuItem("删除片段图");
            deleteSegmentItem.Click += DeleteSegmentItem_Click;
            _ = contextMenu.Items.Add(deleteSegmentItem);

            ToolStripMenuItem printSettingItem = new ToolStripMenuItem("打印属性");
            printSettingItem.Click += PrintSettingItem_Click;
            _ = contextMenu.Items.Add(printSettingItem);

            ToolStripMenuItem showGridItem = new ToolStripMenuItem("显示网格");
            showGridItem.Click += ShowGridItem_Click;
            _ = contextMenu.Items.Add(showGridItem);

        }

        private void DeleteSegmentItem_Click(object sender, EventArgs e)
        {
            // 确认删除操作
            DialogResult result = MessageBox.Show(
                "确定删除片段图吗？此操作不可恢复。",
                "删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }
            List<int> startSamples = new List<int>();
            for (int i = 0; i < PageSegmentSelectStatus.Count; i++)
            {
                if (PageSegmentSelectStatus[i] == true)
                {
                    startSamples.Add(PageSegmentStartSamples[i]);
                }
            }

            OperateSegment.deleteSegment(segmentFilePath, startSamples);

            // 更新界面数据
            UpdateDataForceWaveformRedraw();
            OnDeleteSegment?.Invoke();
        }

        private void PrintSettingItem_Click(object sender, EventArgs e)
        {
            List<int> startSamples = new List<int>();

            for (int i = 0; i < PageSegmentSelectStatus.Count; i++)
            {
                if (PageSegmentSelectStatus[i] == true)
                {
                    int startSample = PageSegmentStartSamples[i];
                    startSamples.Add(startSample);
                }
            }
            if(startSamples.Count > 1)
            {
                using (PrintSettingForm printSetting = new PrintSettingForm(3, startSamples, "片段图属性"))
                {
                    printSetting.bSelectSingleHeartbeat = true;
                    printSetting.segmentWaveView = this;
                    printSetting.ShowDialog();
                }
            }
            else
            {
                using (PrintSettingForm printSetting = new PrintSettingForm(2, startSamples, "片段图属性"))
                {
                    printSetting.bSelectSingleHeartbeat = true;
                    printSetting.segmentWaveView = this;
                    printSetting.ShowDialog();
                }
            }

        }

        private void ShowGridItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                bShowGrid = !bShowGrid;
                // 根据bShowGrid状态设置菜单文字
                menuItem.Text = bShowGrid ? "隐藏网格" : "显示网格";

                reRendWave();
            }
        }
        #endregion

        private void readPicInfo()
        {
            // 判断文件是否存在
            if (!File.Exists(segmentFilePath))
            {
                return;
            }

            // 文件存在，读取现有JSON数组
            string jsonContent = File.ReadAllText(segmentFilePath);
            SegmentJsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

            Pages = SegmentJsonArray.Count % (rows * cols) != 0 ? (SegmentJsonArray.Count / (rows * cols) + 1) : SegmentJsonArray.Count / (rows * cols);
            if (PageIdx == Pages)
            {
                PageIdx = Math.Max(0, Pages - 1);
            }
            UpdateScrollBarMax();
        }

        private void InitializeEcgData()
        {
            PageSegmentStartSamples.Clear();
            PageSegmentShowStartSamples.Clear();
            PageSegmentEcgData.Clear();
            PageSegmentLeadIdxs.Clear();
            PageSegmentGains.Clear();
            PageSegmentBottomInfoLists.Clear();
            PageSegmentRects.Clear();
            PageSegmentSelectStatus.Clear();

            segmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);

            if (PageIdx == -1)
            {
                return;
            }
            if(SegmentJsonArray.Count == 0)
            {
                _graphicsControl.ContextMenuStrip = null;
            }
            else
            {
                _graphicsControl.ContextMenuStrip = contextMenu;
            }

            PageWaveCount = rows * cols;
            for (int i = PageIdx * PageWaveCount; i < (PageIdx + 1) * PageWaveCount; i++)
            {
                if (i >= SegmentJsonArray.Count)
                {
                    break;
                }
                var segmentJsonObj = SegmentJsonArray[i];
                int startSample = segmentJsonObj["startSample"]?.Value<int>() ?? 0;
                int showStartSample = startSample;
                if (segmentJsonObj["cbxDirection"]?.Value<int>() == 0)
                {
                    showStartSample = (startSample - Convert.ToInt32(1.5 * AnaValues.holterConfig.SampleRate)) > 0 ? startSample - Convert.ToInt32(1.5 * AnaValues.holterConfig.SampleRate) : startSample;
                }
                int duration = segmentJsonObj["cbxDuration"]?.Value<int>() ?? 0;
                int gain = segmentJsonObj["cbxGain"]?.Value<int>() ?? 0;
                int endSample = startSample + duration * AnaValues.holterConfig.SampleRate;
                List<int> leadIdxs = segmentJsonObj["leadIdxs"].ToObject<List<int>>() ?? new List<int>();
                int[] allLeads = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                List<WaveInfo> EcgData = AnaCode.ReadAllWaveData(allLeads.ToList(), showStartSample, showStartSample + duration * AnaValues.holterConfig.SampleRate);

                if (EcgData == null || EcgData.Count == 0)
                {
                    return;
                }

                List<float[]> waveData = EcgData.Select(o => o.WaveDataF).ToList();
                SKRect waveRect = new SKRect(rectPadding, rectPadding, rectWidth, rectHeight);
                PageSegmentRects.Add(waveRect);
                if(ClickRectIdx >= SegmentJsonArray.Count - PageIdx * PageWaveCount)
                {
                    if (i == PageIdx * PageWaveCount)
                    {
                        PageSegmentSelectStatus.Add(true);
                    }
                    else
                    {
                        PageSegmentSelectStatus.Add(false);
                    }
                }
                else
                {
                    if (ClickRectIdx == i % PageWaveCount)
                    {
                        PageSegmentSelectStatus.Add(true);
                    }
                    else
                    {
                        PageSegmentSelectStatus.Add(false);
                    }
                }


                string startTime = segmentJsonObj["tbxStartTime"]?.Value<string>() ?? "";
                string avgHR = segmentJsonObj["tbxAvgHR"]?.Value<string>() ?? "";
                string label = segmentJsonObj["cbxLabel"]?.Value<string>() ?? "";

                List<string> bottomInfoList = new List<string>();
                bottomInfoList.Add(startTime);
                bottomInfoList.Add(CommonValues.Langs["心率"] + $":{avgHR} bpm");
                bottomInfoList.Add(label);
                PageSegmentDurations.Add(duration);
                PageSegmentShowStartSamples.Add(showStartSample);
                PageSegmentStartSamples.Add(startSample);
                PageSegmentLeadIdxs.Add(leadIdxs);
                PageSegmentGains.Add(gain);
                PageSegmentEcgData.Add(waveData);
                PageSegmentBottomInfoLists.Add(bottomInfoList);
            }




        }
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

        private void reRendWave()
        {
            _graphicsControl?.Invalidate();
        }

        // 重新加载数据及强制重绘波形
        public void UpdateDataForceWaveformRedraw()
        {
            readPicInfo();
            InitializeEcgData();
            _graphicsControl?.Refresh();
        }

        // 强制重绘波形
        public void ForceWaveformRedraw()
        {
            InitializeEcgData();
            _graphicsControl?.Refresh();
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
            ScrollBarScroll();
            _lastScrollTime = DateTime.Now;

        }

        public void OnSegmentTimeChanged(List<JObject> segmentList)
        {
            SegmentJsonArray = segmentList;

            Pages = SegmentJsonArray.Count % (rows * cols) != 0 ? (SegmentJsonArray.Count / (rows * cols) + 1) : SegmentJsonArray.Count / (rows * cols);
            if (PageIdx == Pages)
            {
                PageIdx = Math.Max(0, Pages - 1);
            }
            ClickRectIdx = 0;
            UpdateScrollBarMax();
            InitializeEcgData();
            reRendWave();
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
                PageIdx++;
                // 更新滚动条位置
                vScrollBar1.Value = Convert.ToInt32(PageIdx);
                UpdateDataForceWaveformRedraw();
            }
            else if (ClickRectIdx < 0)
            {
                if (PageIdx > 0)
                {
                    ClickRectIdx += rows * cols;
                    PageIdx--;
                    // 更新滚动条位置
                    vScrollBar1.Value = Convert.ToInt32(PageIdx);
                    UpdateDataForceWaveformRedraw();
                }
                //else
                //{
                //    ClickRectIdx = PageSegmentSelectStatus.Count - 1;
                //    UpdateDataForceWaveformRedraw();
                //}
            }
            else
            {
                ClickRectIdx = Math.Min(PageSegmentSelectStatus.Count - 1, ClickRectIdx);
                ForceWaveformRedraw();
            }
        }

        protected override void OnControlKeyDown(Keys keyData)
        {
            SelectBeatChanged(keyData);
            return;
        }
    }
}
