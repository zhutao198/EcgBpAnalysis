using AnalysisApp.Commons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp.CommonControls
{
    enum RectType
    {
        Unanalyzable,           // 0 不可分析段
        AF,                     // 1 房颤、房扑
    }

    public partial class pageScanWave : BaseDrawingControl
    {
        private List<WaveInfo> ecgData = new List<WaveInfo>(); // 每个子列表代表一个导联的数据
        private List<int> leadIdxs = new List<int>();
        private List<int> MultiLeads; //多导联显示时相关的导联序号
        public List<AfInfo> afs = new List<AfInfo>();

        // 波形时间偏移
        private float waveformTimeOffsetSeconds = 0; // 波形起始时间偏移量（秒）

        // 导联显示设置
        private int currentLeadCount = 1; // 当前显示的导联数量（1或3）

        // ECG显示参数常量
        private int Rows = 23;        // 显示25行ECG波形
        private int currentRows = 23;       // 当前显示25行ECG波形
        private int rowSeconds = 60;        // 每行绘制的秒数
        private int rowPoints = 0; // 每行数据点
        private const int leftMargin = 60;
        private float speed = 25;                       //纸速 25mm/s
        private float gain = 2.5f;                       //增益 2.5mm/mV
        private float minGain = 2.5f;                   //最小增益
        private const float pixMM = 3.777f;             //每毫米3.777个像素点
        private bool bShowGrid = false;

        private int width = 0;          //绘图宽度
        private int height = 0;          //绘图高度度

        // 矩形绘制相关变量
        private bool isDrawingRect = false;
        private SKPoint rectStartPoint;
        private SKPoint rectEndPoint;
        private SKRect bigRect = SKRect.Empty;
        private List<SKRect> drawnRects = new List<SKRect>(); // 存储已绘制的矩形
        private List<SKRect> cutRects = new List<SKRect>(); // 存储删除波的矩形
        private BeatInfo ClickHeartbeat = BeatInfo.Empty;

        private int mouseDownSample = 0;
        private int mouseUpSample = 0;

        // 鼠标滚动防抖动相关变量
        private int pendingScrollSeconds;
        private DateTime _lastWheelTime = DateTime.MinValue;

        // 滚动条防抖动相关变量
        private int pendingScrollValue;
        private DateTime _lastScrollTime = DateTime.MinValue;

        private List<BeatInfo> cachedAllBeats = null; // 心搏数据缓存

        private bool showCommonWave = true;


        private List<SegmentRange> _unanalyzableRanges = new List<SegmentRange>();

        DrawEcgWave drawEcgWave = null;

        // 存储当前页心搏信息的列表
        private List<BeatInfo> pageHeartbeatInfos = new List<BeatInfo>();

        /// <summary>
        /// 波形起始时间设置事件
        /// </summary>
        public event Action<int> OnPageScanClickSampleChanged;

        public event Action<List<int>> OnPageScanLeadChanged;

        public event Action<bool> OnCommonWaveShowChanged;


        public pageScanWave()
        {
            InitializeComponent();
        }

        public void RefreshData()
        {
            waveformTimeOffsetSeconds = 0;
            pendingScrollSeconds = 0;
            vScrollBarTime.Value = 0;
            //InitBeatInfos();
            InitializeEcgData();
            _graphicsControl?.Refresh();
        }

        private void pageScanWave_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                try
                {
                    HolterConfig cfg = AnaValues.holterConfig;
                    leadIdxs = new List<int>() { cfg.MainChannel };
                    MultiLeads = new List<int>() { cfg.MainChannel, cfg.SecondChannel1, cfg.SecondChannel2 };

                    initUI();
                    initControl();

                    //// 初始化ECG数据
                    //InitializeEcgData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载错误: {ex.Message}\n\n堆栈跟踪: {ex.StackTrace}");
                }
            }

            //int colIdx = tableLayoutPanel2.GetColumn(comboBox4);
            //tableLayoutPanel2.ColumnStyles[colIdx].Width = 0;
            //comboBox4.Hide();
        }

        /// <summary>
        /// 窗体大小改变时重新调整SKGLControl大小
        /// </summary>
        private void pageScanWave_SizeChanged(object sender, EventArgs e)
        {
            // 延迟处理，确保布局已完成
            System.Threading.Thread.Sleep(50);
            if (panel1 == null) return;
            width = panel1.Width;
            height = panel1.Height;

            rowSeconds = Convert.ToInt32(this.cmbTimes.SelectedValue);

            Rows = Convert.ToInt32(height / pixMM / (minGain * 4));
            currentRows = Rows / currentLeadCount;
            speed = width / pixMM / rowSeconds;

            rowPoints = rowSeconds * AnaValues.holterConfig.SampleRate;

            _graphicsControl?.Refresh();
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            // 只有在左键点击时才执行选择和绘制矩形的操作
            if (e.Button == MouseButtons.Left)
            {
                if (pageHeartbeatInfos.Count == 0) return;
                drawnRects.Clear();

                int sample = AxisToSample(e.X, e.Y);
                mouseDownSample = sample;
                mouseUpSample = sample;

                OnPageScanClickSampleChanged?.Invoke(sample);


                // 开始绘制矩形模式
                isDrawingRect = true;
                rectStartPoint = new SKPoint(e.X, e.Y);
                rectEndPoint = rectStartPoint;
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            // 只有在绘制矩形模式下（左键点击开始）才更新矩形
            if (isDrawingRect)
            {
                // 更新矩形结束点
                rectEndPoint = new SKPoint(e.X, e.Y);
                //lastHeartbeatEndIdx = -1;
                //Console.WriteLine("----OnCanvasMouseMove  heartbeatEndIdx: " + heartbeatEndIdx);
                AddDrawRects();
                // 强制重绘波形
                reRendWave();
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawingRect)
            {
                if (pageHeartbeatInfos.Count == 0) return;

                rectEndPoint = new SKPoint(e.X, e.Y);

                if (rectStartPoint.X == rectEndPoint.X)
                {
                    ClickHeartbeat = GetClickBeat(e.X, e.Y);
                    AddClickRect(ClickHeartbeat, e.X);
                }

                mouseUpSample = AxisToSample(rectEndPoint.X, rectEndPoint.Y);

                //结束绘制矩形
                isDrawingRect = false;

                if (drawnRects.Count > 0)
                {
                    int endSample = AxisToSample(e.X, e.Y);
                    int startIdx = Math.Min(mouseDownSample, endSample);
                    int endIdx = Math.Max(mouseDownSample, endSample);
                    _selectedBeats = getSelectedBeats(startIdx, endIdx);
                }

                // 强制重绘波形
                reRendWave();
            }
        }

        private void OnCanvasMouseDblClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (pageHeartbeatInfos.Count == 0) return;

                drawnRects.Clear();
                int sample = AxisToSample(e.X, e.Y);
                FullWaveForm.Instance.Show(sample, this.FindForm());
            }
        }

        // 鼠标滚动事件处理
        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastWheelTime < TimeSpan.FromMilliseconds(20)) return;

            // 计算滚动的秒数（根据滚动方向和当前显示行数）
            int scrollSeconds = e.Delta > 0 ? -currentRows * rowSeconds : currentRows * rowSeconds;

            // 累积待处理的滚动秒数
            pendingScrollSeconds += scrollSeconds;
            if (pendingScrollSeconds != 0)
            {
                // 更新时间偏移
                waveformTimeOffsetSeconds += pendingScrollSeconds;
                waveformTimeOffsetSeconds = Math.Max(0, Math.Min(waveformTimeOffsetSeconds, vScrollBarTime.Maximum));
                //Console.WriteLine($"----vScrollBarTime.Maximum: {vScrollBarTime.Maximum} holterConfig.Total: {holterConfig.Total} total seconds: {holterConfig.Total / SamplingRate}");
                // 更新滚动条位置
                vScrollBarTime.Value = Convert.ToInt32(waveformTimeOffsetSeconds);

                drawnRects.Clear();

                UpdateEcgData();

                AddWheelDrawRect();

                // 重绘当前可见的渲染控件
                reRendWave();

                // 重置待处理滚动秒数
                pendingScrollSeconds = 0;

                if (pageHeartbeatInfos.Count > 0)
                {
                    int heartbeatIdx = pageHeartbeatInfos.Count > 5 ? 5 : 0;
                    OnPageScanClickSampleChanged?.Invoke(pageHeartbeatInfos[heartbeatIdx].Sample);
                }
            }
            _lastWheelTime = DateTime.Now;

        }

        protected override int OnControlKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (_selectedBeats != null && _selectedBeats.Count > 0)
            {
                int ret = base.OnControlKeyPressed(sender, e);

                if (ret == 0)
                {
                    //mouseDownSample = 0;
                    //mouseUpSample = 0;
                    rectStartPoint = rectEndPoint;
                    if (char.ToUpper(e.KeyChar) == 'K' || char.ToUpper(e.KeyChar) == 'M' || _selectedBeats.Count > 1) drawnRects.Clear();
                }
            }
            return 0;
        }

        private void cmbGain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (drawEcgWave == null) return;

            gain = (float)Convert.ToDouble(this.cmbGain.SelectedValue);

            // 强制重绘波形
            reRendWave();
        }

        private void cmbTimes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (drawEcgWave == null) return;
            ForceWaveformRedraw();
        }

        private void cmbLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (drawEcgWave == null) return;
            Rows = Convert.ToInt32(height / pixMM / (minGain * 4));
            currentLeadCount = Convert.ToInt32(this.cmbLayout.SelectedValue);
            if (currentLeadCount == 1)
            {
                this.leadSet.Hide();
                this.lblLead.Show();
                this.cmbLead.Show();

                leadIdxs = new List<int>() { Convert.ToInt32(this.cmbLead.SelectedValue) };
            }
            else
            {
                this.leadSet.Show();
                this.lblLead.Hide();
                this.cmbLead.Hide();

                leadIdxs = new List<int>(MultiLeads);
            }
            currentRows = Rows / currentLeadCount;
            ForceWaveformRedraw();
        }

        private void cmbLead_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (drawEcgWave == null) return;
            int lead = Convert.ToInt32(this.cmbLead.SelectedValue);
            leadIdxs.Clear();
            leadIdxs.Add(lead);
            // 强制重绘波形
            ForceWaveformRedraw();
        }

        private void leadSet_Click(object sender, EventArgs e)
        {
            LeadSettingForm leadSetting = new LeadSettingForm();

            if (leadSetting.selectedIndices.Count == 0)
            {
                leadSetting.selectedIndices.AddRange(leadIdxs);
            }

            // 订阅导联选择事件
            leadSetting.OnLeadsSelected += OnLeadsSelected;

            leadSetting.ShowDialog();
        }

        // 滚动条滚动事件处理
        private void vScrollBarTime_Scroll(object sender, ScrollEventArgs e)
        {
            if (DateTime.Now - _lastScrollTime < TimeSpan.FromMilliseconds(20)) return;

            // 根据滚动事件的类型处理
            if (e.Type == ScrollEventType.SmallIncrement) // 向下箭头
            {
                pendingScrollValue = Math.Min(Convert.ToInt32(waveformTimeOffsetSeconds) + rowSeconds, vScrollBarTime.Maximum);
            }
            else if (e.Type == ScrollEventType.SmallDecrement) // 向上箭头
            {
                pendingScrollValue = Math.Max(Convert.ToInt32(waveformTimeOffsetSeconds) - rowSeconds, 0);
            }
            else if (e.Type == ScrollEventType.LargeDecrement) // 点击滚动条上方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = vScrollBarTime.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.Y - 17) * 1.0f / vScrollBarTime.Height) * vScrollBarTime.Maximum);
                pendingScrollValue = e.NewValue;

                //Console.WriteLine($"[vScrollBarTime_Scroll] LargeIncrement 鼠标位置: ({relativeMousePos.X}, {relativeMousePos.Y}) 滚动条高度: {vScrollBarTime.Height}");
                //Console.WriteLine($"scroll value: {e.NewValue} totalSeconds: {vScrollBarTime.Maximum}");
            }
            else if (e.Type == ScrollEventType.LargeIncrement) // 点击滚动条下方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = vScrollBarTime.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.Y + 17) * 1.0f / vScrollBarTime.Height) * vScrollBarTime.Maximum);
                pendingScrollValue = e.NewValue;

                //Console.WriteLine($"[vScrollBarTime_Scroll] LargeIncrement 鼠标位置: ({relativeMousePos.X}, {relativeMousePos.Y}) 滚动条高度: {vScrollBarTime.Height}");
                //Console.WriteLine($"scroll value: {e.NewValue} totalSeconds: {vScrollBarTime.Maximum}");
            }
            else if (e.Type == ScrollEventType.ThumbTrack || e.Type == ScrollEventType.ThumbPosition)
            {
                // 拖动滑块时，直接设置待处理值为滑块位置
                pendingScrollValue = e.NewValue > vScrollBarTime.Maximum - currentRows && e.NewValue < vScrollBarTime.Maximum
                    ? vScrollBarTime.Maximum
                    : Math.Min(e.NewValue, vScrollBarTime.Maximum);

            }

            ScrollBarScroll();

            _lastScrollTime = DateTime.Now;

        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            // 清除画布
            canvas.Clear(SKColors.White);

            drawEcgWave.WaveformTimeOffsetSeconds = waveformTimeOffsetSeconds;
            drawEcgWave.speed = speed;
            drawEcgWave.gain = gain;
            drawEcgWave.CurrentLeadCount = currentLeadCount;
            drawEcgWave.CurrentRows = currentRows;
            drawEcgWave.RowPoints = rowPoints;
            drawEcgWave.EcgData = ecgData;
            drawEcgWave.pageHeartbeatInfos = pageHeartbeatInfos;
            drawEcgWave.ShowRrHrInfo = false;
            drawEcgWave.TopMargin = 0;
            drawEcgWave.bShowGrid = bShowGrid;
            drawEcgWave.step = 2;
            drawEcgWave.leadIdxs = leadIdxs;

            // 绘制背景
            drawEcgWave.DrawBackground(canvas, width, height);
            // 绘制ECG波形
            drawEcgWave.DrawEcgWaveform(canvas, width, height);
            ////绘制时间轴
            drawEcgWave.DrawTimeAxis(canvas, width, height, rowSeconds);

            if (currentLeadCount == 3)
            {
                //绘制导联名称
                drawEcgWave.DrawLeadNames(canvas, width, height, rowSeconds);
            }

            // 绘制矩形
            DrawRectangles(canvas);

            DrawBeatInfo(canvas);

            drawUnanalyzableSections(canvas);

            drawAfSections(canvas);
        }


        /// <summary>
        /// 波形起始时间改变事件处理
        /// </summary>
        public void OnHRTrendWaveTimeChanged(float timeOffsetSeconds)
        {
            // 设置波形起始时间
            waveformTimeOffsetSeconds = timeOffsetSeconds;

            // 更新滚动条位置
            if (vScrollBarTime != null)
            {
                vScrollBarTime.Value = Math.Max(0, Math.Min(Convert.ToInt32(timeOffsetSeconds), vScrollBarTime.Maximum));
            }

            // 强制重绘波形
            ForceWaveformRedraw();
        }

        /// <summary>
        /// 波形起始时间改变事件处理
        /// </summary>
        public void OnWaveformStartTimeChanged(float rowSeconds, float timeOffsetSeconds, int sample)
        {
            // 设置波形起始时间
            waveformTimeOffsetSeconds = timeOffsetSeconds;

            // 更新滚动条位置
            if (vScrollBarTime != null)
            {
                vScrollBarTime.Value = Math.Max(0, Math.Min(Convert.ToInt32(timeOffsetSeconds), vScrollBarTime.Maximum));
            }

            // 强制重绘波形
            ForceWaveformRedraw();
        }

        public void OnEcgCommonWaveClickChanged(float waveformTimeOffsetSeconds, int startIndex, int sample, int endIndex)
        {
            int leftBound = startIndex;
            int rightBound = endIndex;

            // 计算心搏矩形框
            if (leftBound >= 0 && rightBound > leftBound)
            {
                //如果起始时间发生了变化则要更新波形数据
                if (this.waveformTimeOffsetSeconds != waveformTimeOffsetSeconds)
                {
                    this.waveformTimeOffsetSeconds = waveformTimeOffsetSeconds;
                    UpdateEcgData();
                }
                // 清除之前的矩形
                drawnRects.Clear();
                // 计算心搏所在的行
                int totalSamples = rightBound - Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
                int rowIndex = totalSamples / rowPoints;
                rowIndex = Math.Max(0, Math.Min(rowIndex, currentRows - 1));

                // 计算心搏在当前行的起始和结束X坐标
                int rowStartSamples = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + (rowIndex * rowPoints);
                int rowRelativeStart = leftBound - rowStartSamples;
                int rowRelativeEnd = rightBound - rowStartSamples;

                float x1 = leftMargin + (rowRelativeStart * (speed * pixMM / AnaValues.holterConfig.SampleRate));
                float x2 = leftMargin + (rowRelativeEnd * (speed * pixMM / AnaValues.holterConfig.SampleRate));

                // 计算心搏矩形的Y坐标和高度
                float rowHeight = (float)height / currentRows;
                float y1 = rowIndex * rowHeight;
                float y2 = y1 + rowHeight;
                //Console.WriteLine($"---OnEcgCommonWaveClickChanged x1: {x1} x2: {x2} rowRelativeStart:{rowRelativeStart} rowRelativeEnd: {rowRelativeEnd} leftMargin:{leftMargin}");

                foreach (var heartbeat in pageHeartbeatInfos)
                {
                    if (heartbeat.Sample == sample)
                    {
                        ClickHeartbeat = heartbeat;
                        break;
                    }
                }
                // 创建心搏矩形
                SKRect heartbeatRect = new SKRect(x1, y1, x2, y2);
                drawnRects.Add(heartbeatRect);

                float bigLeftX = Math.Max(leftMargin, x1 - (AnaValues.holterConfig.SampleRate * 4) * (speed * pixMM / AnaValues.holterConfig.SampleRate));
                float bigRightX = Math.Min(width, x1 + (AnaValues.holterConfig.SampleRate * 4) * (speed * pixMM / AnaValues.holterConfig.SampleRate));
                bigRect = new SKRect(bigLeftX, y1, bigRightX, y2);

                // 强制重绘波形
                reRendWave();
            }
        }

        private void drawSections(SKCanvas canvas, SKPaint rectPaint, SKPaint textPaint, SKFont textFont, int startSample, int endSample, RectType rectType, string afTypeName = "")
        {
            if (pageHeartbeatInfos.Count == 0) return;
            // 计算心搏所在的行
            int startSamples = startSample - Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            int startRowIndex = startSamples / rowPoints < 0 ? 0 : startSamples / rowPoints;

            int endSamples = endSample - Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            int endRowIndex = endSamples / rowPoints;

            // 计算心搏矩形的X坐标
            int rowStartSamples = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + (startRowIndex * rowPoints);
            int rowRelativeStart = startSample - rowStartSamples < 0 ? 0 : startSample - rowStartSamples;
            float startX = leftMargin + (rowRelativeStart * (speed * pixMM / AnaValues.holterConfig.SampleRate));
            if (startSample == pageHeartbeatInfos[0].StartIndex)
            {
                startX = leftMargin;
            }
            int rowEndSamples = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + (endRowIndex * rowPoints);
            int rowRelativeEnd = endSample - rowEndSamples;
            float endX = leftMargin + (rowRelativeEnd * (speed * pixMM / AnaValues.holterConfig.SampleRate));

            if (startRowIndex == endRowIndex)
            {
                // 计算心搏矩形的Y坐标和高度
                float rowHeight = (float)height / currentRows;
                float y1 = startRowIndex * rowHeight;
                float y2 = y1 + rowHeight;

                // 创建心搏矩形并添加到drawnRects列表
                SKRect rect = new SKRect(startX, y1, endX, y2);
                if (rectType == RectType.Unanalyzable)
                {
                    cutRects.Add(rect);
                }
                canvas.DrawRect(rect, rectPaint);

                float textW = textFont.MeasureText(afTypeName, out SKRect bound, textPaint);
                SKPoint textPos = new SKPoint(rect.Left + 4, rect.Top + bound.Height);
                canvas.DrawText(afTypeName, textPos, SKTextAlign.Left, textFont, textPaint);
            }
            else
            {
                for (int i = startRowIndex; i <= endRowIndex; i++)
                {
                    if (i == startRowIndex)
                    {
                        // 计算心搏矩形的Y坐标和高度
                        float rowHeight = (float)height / currentRows;
                        float y1 = startRowIndex * rowHeight;
                        float y2 = y1 + rowHeight;
                        // 创建心搏矩形并添加到drawnRects列表
                        SKRect rect = new SKRect(startX, y1, width, y2);
                        if (rectType == RectType.Unanalyzable)
                        {
                            cutRects.Add(rect);
                        }
                        canvas.DrawRect(rect, rectPaint);
                        float textW = textFont.MeasureText(afTypeName, out SKRect bound, textPaint);
                        SKPoint textPos = new SKPoint(rect.Left + 4, rect.Top + bound.Height);
                        canvas.DrawText(afTypeName, textPos, SKTextAlign.Left, textFont, textPaint);
                    }
                    else if (i == endRowIndex)
                    {
                        // 计算心搏矩形的Y坐标和高度
                        float rowHeight = (float)height / currentRows;
                        float y1 = endRowIndex * rowHeight;
                        float y2 = y1 + rowHeight;
                        // 创建心搏矩形并添加到drawnRects列表
                        SKRect rect = new SKRect(leftMargin, y1, endX, y2);
                        if (rectType == RectType.Unanalyzable)
                        {
                            cutRects.Add(rect);
                        }
                        canvas.DrawRect(rect, rectPaint);

                    }
                    else
                    {
                        // 计算心搏矩形的Y坐标和高度
                        float rowHeight = (float)height / currentRows;
                        float y1 = i * rowHeight;
                        float y2 = y1 + rowHeight;
                        // 创建心搏矩形并添加到drawnRects列表
                        SKRect rect = new SKRect(leftMargin, y1, width, y2);
                        if (rectType == RectType.Unanalyzable)
                        {
                            cutRects.Add(rect);
                        }
                        canvas.DrawRect(rect, rectPaint);

                    }
                }
            }
        }

        private void drawAfSections(SKCanvas canvas)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Normal),
                Size = 10.5f
            })
            using (SKPaint rectPaint = new SKPaint()
            {
                IsStroke = false,
                Color = new SKColor(74, 122, 196, 100),
                BlendMode = SKBlendMode.SrcOver
            })
            {
                DateTime startTime = AnaValues.holterConfig.StartDateTime;
                float pageEndSecond = waveformTimeOffsetSeconds + rowSeconds * currentRows;
                foreach (AfInfo af in afs)
                {
                    double afStartSecond = af.StartTime.Subtract(startTime).TotalSeconds;
                    double afEndSecond = af.EndTime.Subtract(startTime).TotalSeconds;
                    if (afStartSecond < pageEndSecond && afEndSecond > waveformTimeOffsetSeconds)
                    {
                        int startSampleIdx = pageHeartbeatInfos.FindIndex(h => h.Sample == af.StartSample);
                        startSampleIdx = startSampleIdx == -1 ? 0 : startSampleIdx;
                        int endSampleIdx = pageHeartbeatInfos.FindIndex(h => h.Sample == af.EndSample);
                        endSampleIdx = endSampleIdx == -1 ? pageHeartbeatInfos.Count - 1 : endSampleIdx;

                        drawSections(canvas, rectPaint, textPaint, textFont, pageHeartbeatInfos[startSampleIdx].StartIndex, pageHeartbeatInfos[endSampleIdx].EndIndex, RectType.AF, af.TypeName);
                    }
                }
            }

        }

        private void drawUnanalyzableSections(SKCanvas canvas)
        {
            cutRects.Clear();
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Normal),
                Size = 10.5f
            })
            using (SKPaint rectPaint = new SKPaint()
            {
                IsStroke = false,
                Color = new SKColor(128, 128, 128, 64),
                BlendMode = SKBlendMode.SrcOver
            })
            {
                DateTime startTime = AnaValues.holterConfig.StartDateTime;
                float pageEndSecond = waveformTimeOffsetSeconds + rowSeconds * currentRows;
                foreach (SegmentRange range in _unanalyzableRanges)
                {
                    double rangeStartSecond = (double)range.StartIndex / AnaValues.holterConfig.SampleRate;
                    double rangeEndSecond = (double)range.EndIndex / AnaValues.holterConfig.SampleRate;
                    if (rangeStartSecond < pageEndSecond && rangeEndSecond > waveformTimeOffsetSeconds)
                    {
                        drawSections(canvas, rectPaint, textPaint, textFont, range.StartIndex, range.EndIndex, RectType.Unanalyzable);
                    }
                }
            }
        }

        public void InitializeEcgData()
        {
            UpdateEcgData();
            UpdateScrollBarMax();
        }

        private void UpdateEcgData()
        {
            if (drawEcgWave == null) return;
            long startTime = DateTime.Now.Ticks;

            updateUnanalyzableRanges();

            ecgData.Clear();

            _selectedBeats?.Clear();
            drawnRects.Clear();

            rowSeconds = Convert.ToInt32(this.cmbTimes.SelectedValue);
            rowPoints = rowSeconds * AnaValues.holterConfig.SampleRate;
            speed = width / pixMM / rowSeconds;

            int startIdx = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            int endIdx = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + currentRows * AnaValues.holterConfig.SampleRate * rowSeconds - 1;
            endIdx = Math.Min(endIdx, AnaValues.holterConfig.Total - 1);
            if (endIdx < startIdx) return;
            ecgData = AnaCode.ReadAllWaveData(leadIdxs, startIdx, endIdx);

            if (ecgData == null || ecgData.Count == 0)
            {
                return;
            }

            pageHeartbeatInfos.Clear();
            pageHeartbeatInfos = AnaValues.CachedAllBeats
            .Where(h => h.EndIndex > startIdx && h.StartIndex < endIdx)
            .OrderBy(h => h.StartIndex)
            .ToList();

            //用于处理最后一段数据全部都是不可分析段
            if (pageHeartbeatInfos.Count == 0)
            {
                BeatInfo beat = BeatInfo.Empty;
                beat.StartIndex = startIdx;
                beat.EndIndex = endIdx;
                beat.BeatUiType = 'N';
                pageHeartbeatInfos.Add(beat);
            }

            if (drawnRects.Count == 0)
            {
                SKPoint mouseUpPoint = SampleToAxis(mouseUpSample);

                rectStartPoint = rectEndPoint = mouseUpPoint;

                ClickHeartbeat = GetClickBeat(mouseUpPoint.X, mouseUpPoint.Y);

                AddClickRect(ClickHeartbeat, Convert.ToInt32(mouseUpPoint.X));
                //setDefaultSelect();
            }
        }

        // 更新滚动条的最大值
        public void UpdateScrollBarMax()
        {
            if (ecgData.Count > 0 && ecgData[leadIdxs[0]].DataLength > 0)
            {
                int page = (AnaValues.holterConfig.TotalSeconds + (rowSeconds * currentRows) - 1) / (rowSeconds * currentRows);

                vScrollBarTime.Maximum = (page - 1) * rowSeconds * currentRows;
                if (page > 10)
                {
                    vScrollBarTime.LargeChange = page / 10;
                }
                else
                {
                    vScrollBarTime.LargeChange = 1;
                }
                // 调试信息输出
                //Console.WriteLine($"[UpdateScrollBarMax] 总采样点: {holterConfig.Total:N0}, 采样率: {SamplingRate}Hz, 总秒数: {totalSeconds}, 滚动条最大值: {vScrollBarTime.Maximum}");
            }

        }

        private void updateUnanalyzableRanges()
        {
            if (_holterConfig.StartReanalysis)
            {
                _unanalyzableRanges.Clear();
                for (int i = 0; i < AnaValues.UnanalyzableSegments.Count; i++)
                {
                    var unaSegment = AnaValues.UnanalyzableSegments[i];
                    SegmentRange range = new SegmentRange(unaSegment.StartIndex, unaSegment.EndIndex);
                    _unanalyzableRanges.Add(range);
                }
            }
        }

        private int AxisToSample(float x, float y)
        {
            // 计算每行的高度和宽度
            float rowHeight = height / (float)currentRows;

            // 计算点击位置所在的行和数据点
            int clickedRow = (int)(y / rowHeight);
            float clickedXInRow = x - leftMargin;

            // 计算点击位置对应的数据点索引
            int startIndex = (clickedRow * rowPoints) + Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            int sample = startIndex + (int)(clickedXInRow / (speed * pixMM / AnaValues.holterConfig.SampleRate));
            return sample;
        }

        private SKPoint SampleToAxis(int sample)
        {
            int pageStartSample = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            SKPoint point = new SKPoint();
            float rowHeight = height / (float)currentRows;
            int rowIdx = (sample - pageStartSample) / rowPoints;
            point.Y = rowIdx * rowHeight;
            point.X = leftMargin + (float)(sample - pageStartSample - rowIdx * rowPoints) / AnaValues.holterConfig.SampleRate * speed * pixMM;

            return point;
        }

        private BeatInfo GetClickBeat(float X, float Y)
        {
            BeatInfo clickHeartbeat = BeatInfo.Empty;
            if (pageHeartbeatInfos.Count > 0)
            {
                int clickSample = AxisToSample(X, Y);
                if (clickSample < pageHeartbeatInfos[0].Sample)
                {
                    clickSample = pageHeartbeatInfos[0].Sample;
                }
                if (clickSample > pageHeartbeatInfos[pageHeartbeatInfos.Count - 1].Sample)
                {
                    clickSample = pageHeartbeatInfos[pageHeartbeatInfos.Count - 1].Sample;
                }

                foreach (var heartbeat in pageHeartbeatInfos)
                {
                    if (heartbeat.StartIndex <= clickSample && heartbeat.EndIndex >= clickSample)
                    {
                        clickHeartbeat = heartbeat;
                        break;
                    }
                }
            }

            return clickHeartbeat;
        }

        private List<BeatInfo> getSelectedBeats(int startSample, int endSample)
        {
            List<BeatInfo> lstBeats = new List<BeatInfo>();
            foreach (BeatInfo heartbeat in AnaValues.CachedAllBeats)
            {
                if (heartbeat.EndIndex > startSample && heartbeat.StartIndex < endSample)
                {
                    lstBeats.Add(heartbeat);
                }
            }
            return lstBeats;
        }

        // 初始化右键菜单
        private void InitializeContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem deleteWaveItem = new ToolStripMenuItem(CommonValues.Langs["删除波"]);
            deleteWaveItem.Click += DeleteWaveItem_Click;
            _ = contextMenu.Items.Add(deleteWaveItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // 添加修改心搏类型子菜单
            //ToolStripMenuItem changeHeartbeatTypeItem = new ToolStripMenuItem("修改心搏类型");

            // 为每种心搏类型添加子菜单项
            foreach (char beatType in AnaValues.BeatTypes.Keys)
            {
                ToolStripMenuItem changeHeartbeatTypeItem = new ToolStripMenuItem($"({beatType})" + CommonValues.Langs[AnaValues.BeatTypes[beatType]])
                {
                    Tag = beatType // 将心搏类型存储在Tag中
                };
                changeHeartbeatTypeItem.Click += ChangeHeartbeatTypeItem_Click;
                _ = contextMenu.Items.Add(changeHeartbeatTypeItem);
                //_ = changeHeartbeatTypeItem.DropDownItems.Add(typeItem);
            }

            _graphicsControl.ContextMenuStrip = contextMenu;
        }

        private void AddClickRect(BeatInfo clickHeartbeat, int mouseClickX)
        {
            drawnRects.Clear();
            bigRect = SKRect.Empty;
            if (AnaCode.IsInvalidBeat(clickHeartbeat)) return;

            float rowHeight = height / (float)currentRows;

            SKPoint startPoint = SampleToAxis(clickHeartbeat.StartIndex);
            SKPoint endPoint = SampleToAxis(clickHeartbeat.EndIndex);
            if (endPoint.Y > startPoint.Y)
            {
                SKRect rect = new SKRect(leftMargin, endPoint.Y, endPoint.X, endPoint.Y + rowHeight);
                drawnRects.Add(rect);
            }
            else
            {
                SKRect rect = new SKRect(startPoint.X, startPoint.Y, endPoint.X, startPoint.Y + rowHeight);
                //Console.WriteLine("AddClickRect---Left: "+ rect.Left+ "-Right: "+ rect.Right);
                drawnRects.Add(rect);
            }

            SKPoint startBigPoint = SampleToAxis(Convert.ToInt32(clickHeartbeat.Sample - 3.5 * _holterConfig.SampleRate));
            SKPoint endBigPoint = SampleToAxis(Convert.ToInt32(clickHeartbeat.Sample + 3.5 * _holterConfig.SampleRate));
            if (endBigPoint.Y > startBigPoint.Y)
            {
                if (mouseClickX < width / 2)
                {
                    bigRect = new SKRect(leftMargin, endBigPoint.Y, endBigPoint.X, endBigPoint.Y + rowHeight);
                }
                else
                {
                    bigRect = new SKRect(startBigPoint.X, startBigPoint.Y, width, startBigPoint.Y + rowHeight);
                }
            }
            else
            {
                float left = Math.Max(startBigPoint.X, leftMargin);
                bigRect = new SKRect(left, startBigPoint.Y, endBigPoint.X, startBigPoint.Y + rowHeight);
            }

        }

        private void AddDrawRects()
        {
            // 检查是否有实际的拖动（移动距离超过阈值）
            const float dragThreshold = 5.0f;
            float distance = (float)Math.Sqrt(
                Math.Pow(rectEndPoint.X - rectStartPoint.X, 2) +
                Math.Pow(rectEndPoint.Y - rectStartPoint.Y, 2));

            // 只有当有实际拖动时，才绘制临时的拖动矩形
            if (distance > dragThreshold)
            {
                bigRect = SKRect.Empty;
                drawnRects = null;
                drawnRects = new List<SKRect>(1);
                // 计算每行的高度
                float rowHeight = height / (float)currentRows;

                // 计算拖选范围的起始行和结束行
                int startRow = (int)(Math.Min(rectStartPoint.Y, rectEndPoint.Y) / rowHeight);
                int endRow = (int)(Math.Max(rectStartPoint.Y, rectEndPoint.Y) / rowHeight);

                // 确保行号在有效范围内
                startRow = Math.Max(0, Math.Min(startRow, currentRows - 1));
                endRow = Math.Max(0, Math.Min(endRow, currentRows - 1));

                if (startRow == endRow)
                {
                    float topY = startRow * rowHeight;
                    float bottomY = (startRow + 1) * rowHeight;
                    float left = Math.Min(rectStartPoint.X, rectEndPoint.X);
                    float right = Math.Max(rectStartPoint.X, rectEndPoint.X);

                    SKRect rowRect = new SKRect(left, topY, right, bottomY);
                    //Console.WriteLine("AddDrawRects---Left: " + rowRect.Left + "-Right: " + rowRect.Right);
                    drawnRects.Add(rowRect);
                }
                else
                {
                    // 确定拖动方向，判断哪个是起始行，哪个是结束行
                    bool isDraggingDown = rectEndPoint.Y > rectStartPoint.Y;
                    float startMouseX = isDraggingDown ? rectStartPoint.X : rectEndPoint.X;
                    float endMouseX = isDraggingDown ? rectEndPoint.X : rectStartPoint.X;
                    bool bChangePage = false;
                    if (mouseDownSample < waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate)
                    {
                        startRow = 0;
                        bChangePage = true;
                    }
                    drawnRects.Capacity = endRow - startRow + 1;

                    // 绘制实时选中的行
                    for (int row = startRow; row <= endRow; row++)
                    {
                        float topY = row * rowHeight;
                        float bottomY = (row + 1) * rowHeight;

                        SKRect rowRect;
                        if (row == startRow) // 起始行
                        {
                            if (bChangePage)
                            {
                                rowRect = new SKRect(leftMargin, topY, width, bottomY);
                            }
                            else
                            {
                                // 只绘制鼠标所在位置到行结束的区域
                                rowRect = startRow == endRow ?
                                    new SKRect(startMouseX, topY, endMouseX, bottomY)
                                    : (startRow < endRow ? new SKRect(startMouseX, topY, width, bottomY) : new SKRect(leftMargin, topY, startMouseX, bottomY));
                            }
                        }
                        else if (row == endRow) // 结束行
                        {
                            // 只绘制行开始到鼠标所在位置的区域
                            rowRect = new SKRect(leftMargin, topY, endMouseX, bottomY);
                        }
                        else // 中间行
                        {
                            // 创建覆盖整行波形区域的矩形（从行开始到结束）
                            rowRect = new SKRect(leftMargin, topY, width, bottomY);
                        }
                        //Console.WriteLine("AddDrawRects---Left: " + rowRect.Left + "-Right: " + rowRect.Right);

                        drawnRects.Add(rowRect);
                    }
                }
            }
        }

        private void DrawRectangles(SKCanvas canvas)
        {
            // 创建矩形画笔
            using (SKPaint rectPaint = new SKPaint())
            {
                rectPaint.IsAntialias = true;

                rectPaint.Color = new SKColor(0, 0, 255, 255);
                rectPaint.Style = SKPaintStyle.Stroke;
                rectPaint.StrokeWidth = 1.5f;
                canvas.DrawRect(bigRect, rectPaint);

                rectPaint.Color = new SKColor(74, 122, 196, 100);
                rectPaint.Style = SKPaintStyle.Fill;

                // 绘制已保存的矩形
                foreach (SKRect rect in drawnRects)
                {
                    canvas.DrawRect(rect, rectPaint);
                }
            }
        }

        private void DrawBeatInfo(SKCanvas canvas)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                if (ClickHeartbeat != BeatInfo.Empty)
                {
                    string timeStr = AnaCode.getDataTimeFromSample(ClickHeartbeat.StartIndex).ToString("HH:mm:ss");
                    canvas.DrawText(timeStr, leftMargin, height - pixMM, SKTextAlign.Left, textFont, textPaint);

                    float textW = textFont.MeasureText(timeStr, out SKRect bound, textPaint);
                    string beatTypeStr = CommonValues.Langs[AnaValues.BeatTypes[ClickHeartbeat.BeatUiType]];
                    canvas.DrawText(beatTypeStr, leftMargin + textW + 2 * pixMM, height - pixMM, SKTextAlign.Left, textFont, textPaint);

                }
            }
        }

        private void setDefaultSelect()
        {
            if (pageHeartbeatInfos.Count == 0)
            {
                if (!bigRect.IsEmpty)
                {
                    bigRect = SKRect.Empty;
                }
                return;
            }
            int heartbeatIdx = pageHeartbeatInfos.Count > 5 ? 5 : 0;
            float x1 = leftMargin + ((pageHeartbeatInfos[heartbeatIdx].StartIndex - waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) * (speed * pixMM / AnaValues.holterConfig.SampleRate));
            float x2 = leftMargin + ((pageHeartbeatInfos[heartbeatIdx].EndIndex - waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) * (speed * pixMM / AnaValues.holterConfig.SampleRate));

            // 计算心搏矩形的Y坐标和高度
            float rowHeight = (float)height / currentRows;
            float y1 = 0;
            float y2 = y1 + rowHeight;

            ClickHeartbeat = pageHeartbeatInfos[heartbeatIdx];

            // 创建心搏矩形
            SKRect heartbeatRect = new SKRect(x1, y1, x2, y2);

            drawnRects.Add(heartbeatRect);

            float bigLeftX = Math.Max(leftMargin, x1 - (AnaValues.holterConfig.SampleRate * 4) * (speed * pixMM / AnaValues.holterConfig.SampleRate));
            float bigRightX = Math.Min(width, x1 + (AnaValues.holterConfig.SampleRate * 4) * (speed * pixMM / AnaValues.holterConfig.SampleRate));
            bigRect = new SKRect(bigLeftX, y1, bigRightX, y2);
        }

        public void reRendWave()
        {
            _graphicsControl?.Invalidate();
        }

        // 强制重绘波形
        public void ForceWaveformRedraw()
        {
            UpdateEcgData();
            reRendWave();
        }

        private void AddWheelDrawRect()
        {
            const float dragThreshold = 5.0f;
            float distance = (float)Math.Sqrt(
                Math.Pow(rectEndPoint.X - rectStartPoint.X, 2) +
                Math.Pow(rectEndPoint.Y - rectStartPoint.Y, 2));

            // 只有当有实际拖动时，才绘制临时的拖动矩形
            if (distance > dragThreshold)
            {
                drawnRects.Clear();

                if (mouseUpSample < waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate)
                {
                    return;
                }

                // 计算每行的高度
                float rowHeight = height / (float)currentRows;

                // 计算拖选范围的起始行和结束行
                int startRow = (int)(Math.Min(rectStartPoint.Y, rectEndPoint.Y) / rowHeight);
                int endRow = (int)(Math.Max(rectStartPoint.Y, rectEndPoint.Y) / rowHeight);

                // 确保行号在有效范围内
                startRow = Math.Max(0, Math.Min(startRow, currentRows - 1));
                endRow = Math.Max(0, Math.Min(endRow, currentRows - 1));

                // 确定拖动方向，判断哪个是起始行，哪个是结束行
                bool isDraggingDown = rectEndPoint.Y > rectStartPoint.Y;
                float startMouseX = isDraggingDown ? rectStartPoint.X : rectEndPoint.X;
                float endMouseX = isDraggingDown ? rectEndPoint.X : rectStartPoint.X;

                //Console.WriteLine("---startHeartBeat: " + startHeartBeat + "-----endHeartBeat: " + endHeartBeat);
                //Console.WriteLine("---startPagePoint: " + waveformTimeOffsetMinutes * rowSeconds * SamplingRate + "-----endPagePoint: " + (waveformTimeOffsetMinutes + rows) * rowSeconds * SamplingRate);

                if (mouseDownSample < waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate)
                {
                    if (mouseUpSample > ((waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + (currentRows * rowSeconds * AnaValues.holterConfig.SampleRate)))
                    {

                        for (int row = 0; row < currentRows; row++)
                        {
                            float topY = row * rowHeight;
                            float bottomY = (row + 1) * rowHeight;
                            SKRect rowRect;
                            rowRect = new SKRect(leftMargin, topY, width, bottomY);
                            drawnRects.Add(rowRect);
                        }
                    }
                    else if (mouseUpSample > waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate && mouseUpSample <= ((waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + (currentRows * rowSeconds * AnaValues.holterConfig.SampleRate)))
                    {
                        for (int row = 0; row <= endRow; row++)
                        {
                            float topY = row * rowHeight;
                            float bottomY = (row + 1) * rowHeight;

                            SKRect rowRect = row == endRow ? new SKRect(leftMargin, topY, endMouseX, bottomY) : new SKRect(leftMargin, topY, width, bottomY);
                            drawnRects.Add(rowRect);

                        }
                    }
                }
                else if (mouseDownSample >= waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate && mouseDownSample < ((waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + (currentRows * rowSeconds * AnaValues.holterConfig.SampleRate)))
                {
                    if (mouseUpSample > ((waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + (currentRows * rowSeconds * AnaValues.holterConfig.SampleRate)))
                    {
                        // 为每一行创建矩形并添加到选中列表
                        for (int row = startRow; row < currentRows; row++)
                        {
                            float topY = row * rowHeight;
                            float bottomY = (row + 1) * rowHeight;

                            SKRect rowRect;

                            if (row == startRow) // 起始行
                            {
                                rowRect = new SKRect(startMouseX, topY, width, bottomY);
                            }
                            else // 中间行
                            {
                                // 创建覆盖整行波形区域的矩形（从行开始到结束）
                                rowRect = new SKRect(leftMargin, topY, width, bottomY);
                            }

                            drawnRects.Add(rowRect);
                        }
                    }
                    else if (mouseUpSample > waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate && mouseUpSample < ((waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate) + (currentRows * rowSeconds * AnaValues.holterConfig.SampleRate)))
                    {
                        // 为每一行创建矩形并添加到选中列表
                        for (int row = startRow; row <= endRow; row++)
                        {
                            float topY = row * rowHeight;
                            float bottomY = (row + 1) * rowHeight;

                            SKRect rowRect;

                            if (row == startRow) // 起始行
                            {
                                rowRect = new SKRect(startMouseX, topY, width, bottomY);
                            }
                            else if (row == endRow) // 中间行
                            {
                                // 创建覆盖整行波形区域的矩形（从行开始到结束）
                                rowRect = new SKRect(leftMargin, topY, endMouseX, bottomY);
                            }
                            else
                            {
                                rowRect = new SKRect(leftMargin, topY, width, bottomY);
                            }

                            drawnRects.Add(rowRect);
                        }
                    }
                }
            }
        }

        // 滚动条防抖动计时器事件处理
        private void ScrollBarScroll()
        {
            // 更新时间偏移
            waveformTimeOffsetSeconds = pendingScrollValue;

            drawnRects.Clear();

            UpdateEcgData();
            AddWheelDrawRect();
            reRendWave();


            if (pageHeartbeatInfos.Count > 0)
            {
                int heartbeatIdx = pageHeartbeatInfos.Count > 5 ? 5 : 0;
                float timeOffsetSeconds = (float)pageHeartbeatInfos[heartbeatIdx].Sample / AnaValues.holterConfig.SampleRate;
                OnPageScanClickSampleChanged?.Invoke(pageHeartbeatInfos[heartbeatIdx].Sample);

            }
        }

        private void DeleteWaveItem_Click(object sender, EventArgs e)
        {
            // 确保有选中的心搏
            if (drawnRects.Count == 0)
            {
                //_ = MessageBox.Show("请先选择要修改的心搏", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int startSample = _selectedBeats[0].StartIndex;
            int endSample = _selectedBeats[_selectedBeats.Count - 1].EndIndex;
            int ret = AEcgHandler.MarkSegmentUnanalyzable(startSample, endSample);
            if (ret == 0)
            {
                drawnRects.Clear();
                rectEndPoint = rectStartPoint;
                base.ChangeDataSource(null);
            }

        }

        // 修改心搏类型菜单项点击事件
        private void ChangeHeartbeatTypeItem_Click(object sender, EventArgs e)
        {
            // 确保有选中的心搏
            if (drawnRects.Count == 0)
            {
                //_ = MessageBox.Show("请先选择要修改的心搏", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 获取选中的新心搏类型
            if (!(sender is ToolStripMenuItem clickedItem) || !(clickedItem.Tag is char))
            {
                return;
            }

            char newType = (char)clickedItem.Tag;

            if (!isValidSelectionRange())
            {
                ScreenCenterTip.Show(CommonValues.Langs["选择的区域包含无效的波形数据"]);
                return;
            }

            List<int> samples = new List<int>();
            // 修改heartbeatInfos中对应范围的心搏类型
            for (int i = 0; i < _selectedBeats.Count; i++)
            {
                samples.Add(_selectedBeats[i].Sample);
            }

            int retK = AEcgHandler.ModifyBeats(samples.ToArray(), newType);
            if (retK != 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["操作失败"]);
                return;
            }

            if (newType == 'K' || newType == 'M' || _selectedBeats.Count > 1) drawnRects.Clear();

            mouseDownSample = 0;
            mouseUpSample = 0;
            rectStartPoint = rectEndPoint;
            base.ChangeDataSource(null);
        }
        private bool isValidSelectionRange()
        {
            foreach (SKRect selectRect in drawnRects)
            {
                foreach (SKRect cutRect in cutRects)
                {
                    if (selectRect.Contains(cutRect) || selectRect.IntersectsWith(cutRect))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void initUI()
        {
            initUIControl();

            #region 初始化下拉框选项
            fillGainCombobox();
            fillTimesComboBox();
            fillLayoutComboBox();
            //fillLeadComboBox();
            fillLeadComboBox(this.cmbLead);
            #endregion

        }

        private void initUIControl()
        {
            if (!DesignMode)
            {
                //rowPoints = rowSeconds * AnaValues.holterConfig.SampleRate;

                //Rows = Convert.ToInt32(height / pixMM / (minGain * 4));
                //currentRows = Rows;

                this.leadSet.Hide();
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

            drawEcgWave = new DrawEcgWave();

        }

        private void fillGainCombobox()
        {
            this.cmbGain.ValueMember = "ItemValue";
            this.cmbGain.DisplayMember = "ItemText";

            List<ListBoxItem> items = new List<ListBoxItem>
            {
                new ListBoxItem(2.5, "2.5mm/mV"),
                new ListBoxItem(5, "5mm/mV"),
                new ListBoxItem(10, "10mm/mV"),
                new ListBoxItem(20, "20mm/mV"),
                new ListBoxItem(40, "40mm/mV")
            };
            this.cmbGain.DataSource = items;
            this.cmbGain.SelectedValue = 2.5;
        }

        private void fillTimesComboBox()
        {
            this.cmbTimes.ValueMember = "ItemValue";
            this.cmbTimes.DisplayMember = "ItemText";

            List<ListBoxItem> items = new List<ListBoxItem>
            {
                new ListBoxItem(30, "30s/" + CommonValues.Langs["行"]),
                new ListBoxItem(60, "60s/" + CommonValues.Langs["行"])
            };
            this.cmbTimes.DataSource = items;
            this.cmbTimes.SelectedValue = 60;
        }

        private void fillLayoutComboBox()
        {
            this.cmbLayout.ValueMember = "ItemValue";
            this.cmbLayout.DisplayMember = "ItemText";

            List<ListBoxItem> items = new List<ListBoxItem>
            {
                new ListBoxItem(1, CommonValues.Langs["单导图"]),
                new ListBoxItem(3, CommonValues.Langs["三导图"])
            };
            this.cmbLayout.DataSource = items;
            this.cmbLayout.SelectedValue = 1;
        }

        private void fillLeadComboBox()
        {
            this.cmbLead.ValueMember = "ItemValue";
            this.cmbLead.DisplayMember = "ItemText";

            List<ListBoxItem> items = new List<ListBoxItem>
            {
                new ListBoxItem(0, "I"),
                new ListBoxItem(1, "II"),
                new ListBoxItem(2, "III"),
                new ListBoxItem(3, "aVR"),
                new ListBoxItem(4, "aVL"),
                new ListBoxItem(5, "aVF"),
                new ListBoxItem(6, "V1"),
                new ListBoxItem(7, "V2"),
                new ListBoxItem(8, "V3"),
                new ListBoxItem(9, "V4"),
                new ListBoxItem(10, "V5"),
                new ListBoxItem(11, "V6")
            };
            this.cmbLead.DataSource = items;
            this.cmbLead.SelectedValue = AnaValues.holterConfig.MainChannel;
        }

        //重置界面元素显示
        public void ResetUIState(bool refresh = false)
        {
            HolterConfig cfg = AnaValues.holterConfig;
            MultiLeads = new List<int>() { cfg.MainChannel, cfg.SecondChannel1, cfg.SecondChannel2 };
            if (currentLeadCount == 1)
            {
                leadIdxs = new List<int>() { cfg.MainChannel };
                this.cmbLead.SelectedValue = AnaValues.holterConfig.MainChannel;
            }
            else if (currentLeadCount == 3)
                leadIdxs = new List<int>(MultiLeads);


            if (refresh)
            {
                drawnRects.Clear();
                ForceWaveformRedraw();
            }
        }

        private void OnLeadsSelected(List<int> selectedIndices)
        {
            // 更新导联索引
            MultiLeads = selectedIndices;
            leadIdxs = new List<int>(MultiLeads);

            if (leadIdxs.Count == 3)
            {
                OnPageScanLeadChanged?.Invoke(leadIdxs);
            }
            // 强制重绘波形
            ForceWaveformRedraw();
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("----keyCode: " + e.KeyCode);
        }

        private void btnCommonWave_Click(object sender, EventArgs e)
        {
            showCommonWave = !showCommonWave;
            SetCommonWave();

            // 强制重绘波形
            ForceWaveformRedraw();
        }

        private void SetCommonWave()
        {
            if (showCommonWave == true)
            {
                this.btnCommonWave.BaseColor = Color.FromArgb(159, 200, 255);
            }
            else
            {
                this.btnCommonWave.BaseColor = Color.FromArgb(221, 231, 245);
            }

            OnCommonWaveShowChanged?.Invoke(showCommonWave);
        }


        public void SelectBeatChanged(Keys keyData)
        {
            if (pageHeartbeatInfos.Count == 0) return;

            if (keyData == Keys.Up)
            {
                mouseUpSample -= rowPoints;
            }
            else if (keyData == Keys.Down)
            {
                mouseUpSample += rowPoints;
            }
            else if (keyData == Keys.Left)
            {
                mouseUpSample -= 7*AnaValues.holterConfig.SampleRate;
            }
            else if (keyData == Keys.Right)
            {
                mouseUpSample += 7 * AnaValues.holterConfig.SampleRate;
            }

            int pageStartSample = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            int pageEndSample = pageStartSample + currentRows * rowPoints;

            if (mouseUpSample < pageStartSample || mouseUpSample > pageEndSample) return;

            SKPoint mouseUpPoint = SampleToAxis(mouseUpSample);
            
            rectStartPoint = rectEndPoint = mouseUpPoint;

            ClickHeartbeat = GetClickBeat(mouseUpPoint.X, mouseUpPoint.Y);

            AddClickRect(ClickHeartbeat, Convert.ToInt32(mouseUpPoint.X));

            _selectedBeats = getSelectedBeats(mouseUpSample, mouseUpSample);

            // 强制重绘波形
            reRendWave();

            OnPageScanClickSampleChanged?.Invoke(mouseUpSample);
        }

        protected override void OnControlKeyDown(Keys keyData)
        {
            SelectBeatChanged(keyData);
            return;
        }



    }
}
