using AnalysisApp.Commons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Utilities;

namespace AnalysisApp.CommonControls
{
    public partial class DrawEcgCommonWave : BaseDrawingControl
    {
        // 波形时间偏移
        public float waveformTimeOffsetSeconds = 0; // 波形起始时间偏移量（秒）
        // 导联显示设置
        public int currentLeadCount = 3; // 当前显示的导联数量
        // ECG显示参数常量
        private readonly int currentRows = 1;                // 每导联显示1行ECG波形
        public int rowPoints = 0; // 每行数据点
        public float rowSeconds = 0;// 每行秒数
        private float wheelSeconds = 20;   //滚动鼠标的秒数
        private readonly int clickSeconds = 1;   //滚动鼠标的秒数
        public float speed = 25;                       //纸速 25mm/s
        private float gain = 10f;                       //增益 2.5mm/mV
        private const float pixMM = SysConstants.PIX_SCALE;
        private bool bShowGrid = true;
        private float LeftMargin { get; set; } = 8 * pixMM;
        private float TopMargin { get; set; } = 8 * pixMM;

        private int width = 0;          //绘图宽度
        private int height = 0;          //绘图高度度

        private int SelectSample = 0;
        private bool bMiddle = true;            //是否在波形中间
        private int clickSample = 0;            //上层界面传入的鼠标点击sample

        private bool _isMousedown = false;
        private int selectStartSample, selectEndSample; //区间选择的开始与结束位置

        // 鼠标滚动防抖动相关变量
        private DateTime _lastWheelTime = DateTime.MinValue;
        public float pendingScrollSeconds;

        // 滚动条防抖动相关变量
        private int pendingScrollValue;
        private DateTime _lastScrollTime = DateTime.MinValue;


        // ECG数据读取相关变量
        public List<WaveInfo> ecgData = new List<WaveInfo>(); // 每个子项代表一个导联的数据

        private DrawEcgWave drawEcgWave = null;

        // 拖选时间范围的矩形绘制相关变量
        private bool isDrawingRect = false;
        private bool isClickLine = false;
        private SKPoint clickPoint;

        private SKPoint rectStartPoint;              // 矩形起始点
        private SKPoint rectEndPoint;                // 矩形结束点
        private SKRect selectedRect;

        private Dictionary<int, int> LastBeatMap;

        private readonly List<SKRect> drawnDelectRects = new List<SKRect>(); // 存储删除波的矩形

        // 临时矩形绘制相关变量（用于pictureBox1选中状态）
        private bool isDrawingTempRect = false;          // 是否正在绘制临时矩形
        private SKPoint tempRectStartPoint;              // 临时矩形起始点
        private SKPoint tempRectEndPoint;                // 临时矩形结束点
        private SKRect tempRect = SKRect.Empty;          // 临时矩形区域

        // 垂直直线绘制相关变量（用于pictureBox2选中状态）
        private List<float> verticalLines = new List<float>(); // 垂直直线位置列表
        private const float LINE_DISTANCE = 70;          // 直线间距离（70px）

        // 直线拖动相关变量
        private bool isDraggingLine = false;             // 是否正在拖动直线
        private int draggedLineIndex = -1;               // 被拖动的直线索引
        private int fixedLineIndex = -1;                 // 固定不动的直线索引
        private float lineDragStartX;                    // 直线拖动起始位置
        private float fixedLineOriginalX;                // 固定直线的原始位置

        // 整体直线组移动相关变量
        private bool isDraggingAllLines = false;         // 是否正在拖动所有直线
        private float allLinesDragStartX;                // 整体拖动起始位置
        private List<float> allLinesOriginalPositions;   // 所有直线的原始位置列表

        // 矩形拖动相关变量
        private bool isDraggingTempRect = false;         // 是否正在拖动临时矩形
        private bool isResizingTempRect = false;         // 是否正在调整临时矩形大小
        private SKPoint dragStartPoint;                  // 拖动起始点
        private SKPoint tempRectOffset;                  // 矩形偏移量
        private SKPoint tempRectOriginalSize;            // 矩形原始尺寸

        //设置起点终点相关变量
        private float StartPointX = 0f;                 //设置起点X坐标
        private float EndPointX = 0f;                   //设置终点X坐标

        private bool isSetStartPoint = false;           //是否已设置起点
        private int startSelectSample = -1;                  //起点
        private int endSelectSample = -1;                      //终点

        public bool bModifyBeat = false;               //是否为心搏操作
        private bool bNext = true;

        private DateTime _lastClickTime = DateTime.MinValue;

        private ToolStripMenuItem mnuDeleteAF = null;
        private ToolStripMenuItem mnuSetAFL = null;
        private ToolStripMenuItem mnuSetAF = null;
        private ToolStripMenuItem mnuEndPoint = null;
        private ToolStripMenuItem mnuStartPoint = null;
        private ToolStripMenuItem mnuAnalyzable = null;
        private ToolStripMenuItem mnuUnanalyzable = null;
        private ToolStripMenuItem mnuDeleteBeat = null;
        private ToolStripMenuItem mnuSetLead = null;
        private ToolStripMenuItem mnuAddNca = null;
        private ToolStripSeparator toolStripSeparator1 = null;

        // 心搏信息类，用于存储每个心搏的起点、终点和类型
        // 存储所有心搏信息的列表
        //private List<DrawEcgWave.HeartbeatInfo> heartbeatInfos = new List<DrawEcgWave.HeartbeatInfo>();
        // 存储当前页心搏信息的列表
        private List<BeatInfo> pageHeartbeatInfos = new List<BeatInfo>();

        private ContextMenuStrip gainMenuStrip; // 增益设置菜单
        private ContextMenuStrip speedMenuStrip; // 纸速设置菜单

        public List<int> leadIdxs = new List<int>();
        private List<int> MultiLeads; //多导联显示时相关的导联序号

        //不可分析段
        private List<SegmentRange> _unanalyzableRanges = new List<SegmentRange>();
        private List<SegmentRange> _cutRanges = new List<SegmentRange>();
        private Dictionary<PictureBox, ToolButton> _toolButtons;
        private LeadChangePopup _LeadChangePopup;

        /// <summary>
        /// 无效片段
        /// </summary>
        public List<SegmentRange> CutRanges => _cutRanges;

        /// <summary>
        /// 肢体导联
        /// </summary>
        public List<string> Limbleads = new List<string>() { "R", "L", "F" };

        /// <summary>
        /// 是否用于重分析界面
        /// </summary>
        [DefaultValue(false), Category("自定义属性"), Description("是否用于重分析界面"), Browsable(true)]
        public bool UsedForReanalyze { get; set; } = false;

        /// <summary>
        /// 是否用于ST界面
        /// </summary>
        [DefaultValue(false), Category("自定义属性"), Description("是否用于ST界面"), Browsable(true)]
        public bool UsedForST { get; set; } = false;

        /// <summary>
        /// 波形点击设置事件
        /// </summary>
        public event Action<float, int, int, int> OnEcgCommonWaveClickChanged;

        /// <summary>
        /// 波形起始时间设置事件
        /// </summary>
        public event Action<float, float, int> OnEcgCommonWaveStartTimeChanged;

        /// <summary>
        /// 纸速设置事件
        /// </summary>
        public event Action<float> OnEcgCommonRowSecondsChanged;

        public DrawEcgCommonWave()
        {
            InitializeComponent();

            clickPoint = new SKPoint(0, 0);

            // 添加界面大小改变事件处理
            Resize += DrawEcgCommonWave_Resize;

            // 初始化增益设置菜单
            InitializeGainMenu();

            // 初始化纸速设置菜单
            InitializeSpeedMenu();

        }

        #region 界面元素事件
        private void DrawEcgCommonWave_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                try
                {
                    init();
                    RefreshData(true);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "通用心电图");
                }
            }
        }

        private void DrawEcgCommonWave_Resize(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                // 延迟处理，确保布局已完成
                System.Threading.Thread.Sleep(50);
                width = panel1.Width;
                height = panel1.Height;

                // 强制重绘波形
                forceWaveformRedraw();

                // 如果pictureBox2处于选中状态，重新生成垂直直线位置
                if (_toolButtons != null && _toolButtons[picCaliper].PressedDown)
                {
                    GenerateVerticalLines();
                }

                OnEcgCommonRowSecondsChanged?.Invoke(rowSeconds);
            }
        }

        #region 绘图控件鼠标事件
        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            // 只有在左键点击时才执行选择和绘制矩形的操作
            if (e.Button == MouseButtons.Left)
            {
                _isMousedown = true;
                if (_toolButtons[picCutStart].PressedDown == false && _toolButtons[picAnaStart].PressedDown == false)
                    selectStartSample = xAxisToSample(e.X);
                else
                    selectEndSample = xAxisToSample(e.X);

                if (_toolButtons[picMeasure].PressedDown) //测量
                {
                    // 检查是否点击在已有的临时矩形的右下角
                    if (!tempRect.IsEmpty && IsPointInBottomRightCorner(e.X, e.Y, tempRect))
                    {
                        // 开始调整临时矩形大小
                        isResizingTempRect = true;
                        dragStartPoint = new SKPoint(e.X, e.Y);
                        tempRectOriginalSize = new SKPoint(tempRect.Width, tempRect.Height);
                    }
                    // 检查是否点击在已有的临时矩形内（但不是右下角）
                    else if (!tempRect.IsEmpty && IsPointInRect(e.X, e.Y, tempRect))
                    {
                        // 开始拖动临时矩形
                        isDraggingTempRect = true;
                        dragStartPoint = new SKPoint(e.X, e.Y);
                        tempRectOffset = new SKPoint(tempRect.Left, tempRect.Top);
                    }
                    else
                    {
                        // 开始绘制新的临时矩形
                        isDrawingTempRect = true;
                        tempRectStartPoint = new SKPoint(e.X, e.Y);
                        tempRectEndPoint = tempRectStartPoint;
                        tempRect = SKRect.Empty;
                    }
                }
                else if (_toolButtons[picCaliper].PressedDown) //测径器
                {
                    // 检测鼠标是否在中间两条直线上
                    int lineIndex = GetMiddleLineIndexAtMouse(e.X, e.Y);
                    if (lineIndex != -1)
                    {
                        // 开始拖动直线
                        isDraggingLine = true;
                        draggedLineIndex = lineIndex;
                        lineDragStartX = e.X;

                        // 确定并保存固定直线的索引和原始位置
                        int middleIndex1 = (verticalLines.Count / 2) - 1;
                        int middleIndex2 = verticalLines.Count / 2;

                        fixedLineIndex = lineIndex == middleIndex1 ? middleIndex2 : middleIndex1;

                        fixedLineOriginalX = verticalLines[fixedLineIndex];
                    }
                    else
                    {
                        // 开始拖动所有直线（整体移动）
                        isDraggingAllLines = true;
                        allLinesDragStartX = e.X;
                        allLinesOriginalPositions = new List<float>(verticalLines);
                    }

                    // 确保在开始任何操作前两条中间直线的位置关系正确
                    EnsureMiddleLinesCorrectOrder();
                }
                else if (_toolButtons[picEnlarge].PressedDown) //放大
                {
                    // pictureBox7选中状态：放大波形功能
                    using (EnlargeWaveForm enlargeWave = new EnlargeWaveForm())
                    {
                        float timeOffsetSeconds = waveformTimeOffsetSeconds + (e.X - LeftMargin) / speed / pixMM;
                        float halfSeconds = (Screen.PrimaryScreen.WorkingArea.Size.Width - LeftMargin) / enlargeWave.speed / enlargeWave.scale / pixMM / 2;
                        enlargeWave.waveformTimeOffsetSeconds = timeOffsetSeconds - halfSeconds > 0 ? timeOffsetSeconds - halfSeconds : 0;
                        // 计算每行的高度和宽度
                        float rowHeight = height / (float)currentLeadCount;

                        // 计算点击位置所在的行和数据点
                        enlargeWave.leadIdxs.Clear();
                        enlargeWave.leadIdxs.Add(leadIdxs[(int)(e.Y / rowHeight)]);


                        enlargeWave.SampleChanged();
                        Form frmParent = this.FindForm();
                        enlargeWave.ShowDialog(frmParent);
                    }
                }
                else
                {
                    // 正常模式(拖选)：使用原有逻辑
                    clickPoint.X = 0;

                    selectedRect = SKRect.Empty;

                    // 开始绘制矩形模式
                    isDrawingRect = true;
                    rectStartPoint = new SKPoint(e.X, e.Y);
                    rectEndPoint = rectStartPoint;

                    int clickSample = xAxisToSample(e.X);
                    int beatSample = clickSample, beatStartSample = 0, beatEndSample = 0;
                    foreach (var heartbeat in pageHeartbeatInfos)
                    {
                        if (heartbeat.StartIndex <= clickSample && clickSample < heartbeat.EndIndex)
                        {
                            beatSample = heartbeat.Sample;
                            beatStartSample = heartbeat.StartIndex;
                            beatEndSample = heartbeat.EndIndex;
                            break;
                        }
                    }
                    OnEcgCommonWaveClickChanged?.Invoke(waveformTimeOffsetSeconds, beatStartSample, beatSample, beatEndSample);

                    if (!isSetStartPoint)
                    {
                        if (startSelectSample > 0 && endSelectSample > 0)
                        {
                            startSelectSample = -1;
                            endSelectSample = -1;
                        }
                    }
                }
            }
            // 右键点击时强制重新绘制波形和矩形框，确保矩形框被正确保存
            else if (e.Button == MouseButtons.Right)
            {

            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            // 检查是否在拖动直线(测径器)
            if (isDraggingLine)
            {
                // 计算鼠标移动的偏移量
                float deltaX = e.X - lineDragStartX;

                // 获取中间两条直线的索引
                int middleIndex1 = (verticalLines.Count / 2) - 1;
                int middleIndex2 = verticalLines.Count / 2;

                // 计算移动后的新位置
                float newPosition = verticalLines[draggedLineIndex] + deltaX;

                // 获取两条中间直线的当前位置
                float leftLinePosition = verticalLines[middleIndex1];
                float rightLinePosition = verticalLines[middleIndex2];

                // 判断移动是否合法：两条直线不能互相交叉
                bool isMoveValid = true;

                if (draggedLineIndex == middleIndex1) // 左边竖线
                {
                    // 左边竖线只能在左侧区域移动，不能超过右边竖线的位置
                    // 设置最大移动边界：不能超过右侧直线-40像素的位置
                    float maxPosition = rightLinePosition - 40;

                    if (newPosition > maxPosition) // 试图超过右边竖线的边界
                    {
                        isMoveValid = false;
                        Console.WriteLine($"左侧竖线移动被阻止：新位置({newPosition}) > 最大边界({maxPosition})");
                    }
                    else
                    {
                        Console.WriteLine($"左侧竖线移动允许：新位置({newPosition}) <= 最大边界({maxPosition})");
                    }
                }
                else if (draggedLineIndex == middleIndex2) // 右边竖线
                {
                    // 右边竖线只能在右侧区域移动，不能超过左边竖线的位置
                    // 设置最小移动边界：不能低于左侧直线+40像素的位置
                    float minPosition = leftLinePosition + 40;

                    if (newPosition < minPosition) // 试图超过左边竖线的边界
                    {
                        isMoveValid = false;
                        Console.WriteLine($"右侧竖线移动被阻止：新位置({newPosition}) < 最小边界({minPosition})");
                    }
                    else
                    {
                        Console.WriteLine($"右侧竖线移动允许：新位置({newPosition}) >= 最小边界({minPosition})");
                    }
                }

                if (isMoveValid)
                {
                    // 更新被拖动的直线位置
                    verticalLines[draggedLineIndex] = newPosition;

                    // 强制确保两条中间直线的正确位置关系
                    EnsureMiddleLinesCorrectOrder();

                    // 更新拖动起始位置
                    lineDragStartX = e.X;

                    // 根据中间两条直线的新距离重绘所有直线
                    RedrawLinesWithNewDistance();

                    invokeRendering();
                }
            }
            // 检查是否在拖动所有直线（测径器, 整体移动）
            else if (isDraggingAllLines)
            {
                // 计算鼠标移动的偏移量
                float deltaX = e.X - allLinesDragStartX;

                // 计算中间两条直线的距离（作为基准间距）
                int middleIndex1 = (verticalLines.Count / 2) - 1;
                int middleIndex2 = verticalLines.Count / 2;
                float baseDistance = Math.Abs(verticalLines[middleIndex2] - verticalLines[middleIndex1]);

                // 以中间两条直线的新位置为中心，保持间距重绘所有直线
                float newCenterX = (verticalLines[middleIndex1] + verticalLines[middleIndex2]) / 2 + deltaX;

                // 检查中间两条直线是否会超出边界
                float leftMargin = 20; // 左边距
                float rightMargin = width - 20; // 右边距

                // 计算中间两条直线的新位置
                float newLeftLineX = newCenterX - baseDistance / 2;
                float newRightLineX = newCenterX + baseDistance / 2;

                // 限制中间两条直线不超出边界
                if (newLeftLineX < leftMargin)
                {
                    newCenterX = leftMargin + baseDistance / 2;
                }
                else if (newRightLineX > rightMargin)
                {
                    newCenterX = rightMargin - baseDistance / 2;
                }

                // 计算起始位置（以中间两条直线为中心）
                float startX = newCenterX - (middleIndex1 + 0.5f) * baseDistance;

                // 重新生成所有直线位置，保持原有间距
                for (int i = 0; i < verticalLines.Count; i++)
                {
                    verticalLines[i] = startX + i * baseDistance;
                }

                // 确保所有直线都在界面范围内
                EnsureAllLinesInBounds();

                // 更新拖动起始位置
                allLinesDragStartX = e.X;

                invokeRendering();
            }
            // 检查是否在调整临时矩形大小(测量)
            else if (isResizingTempRect)
            {
                // 计算鼠标移动的偏移量
                float deltaX = e.X - dragStartPoint.X;
                float deltaY = e.Y - dragStartPoint.Y;

                // 计算新的宽度和高度（保持左上角固定）
                float newWidth = tempRectOriginalSize.X + deltaX;
                float newHeight = tempRectOriginalSize.Y + deltaY;

                // 限制最小尺寸
                newWidth = Math.Max(newWidth, 10);
                newHeight = Math.Max(newHeight, 10);

                // 更新临时矩形（保持左上角位置不变，只改变右下角）并限制在波形区域内
                SKRect newRect = new SKRect(tempRect.Left, tempRect.Top, tempRect.Left + newWidth, tempRect.Top + newHeight);
                tempRect = ClampRectToWaveformArea(newRect);

                invokeRendering();
            }
            // 检查是否在拖动临时矩形(测量)
            else if (isDraggingTempRect)
            {
                // 计算拖动偏移量
                float deltaX = e.X - dragStartPoint.X;
                float deltaY = e.Y - dragStartPoint.Y;

                // 更新临时矩形位置
                float newLeft = tempRectOffset.X + deltaX;
                float newTop = tempRectOffset.Y + deltaY;
                float newRight = newLeft + tempRect.Width;
                float newBottom = newTop + tempRect.Height;

                // 更新临时矩形并限制在波形区域内
                SKRect newRect = new SKRect(newLeft, newTop, newRight, newBottom);
                tempRect = ClampRectToWaveformArea(newRect);

                invokeRendering();
            }
            // 检查是否在绘制临时矩形(测量)
            else if (isDrawingTempRect)
            {

                // 更新临时矩形结束点，并限制在波形区域内
                float clampedX = Math.Max(LeftMargin, Math.Min(e.X, width));
                float clampedY = Math.Max(TopMargin, Math.Min(e.Y, height));
                tempRectEndPoint = new SKPoint(clampedX, clampedY);

                invokeRendering();
            }
            // 只有在绘制拖选矩形模式下（左键点击开始）才更新矩形
            else if (isDrawingRect)
            {
                // 更新矩形结束点
                rectEndPoint = new SKPoint(e.X, e.Y);
                float left = Math.Min(rectStartPoint.X, rectEndPoint.X);
                float top = TopMargin;
                float right = Math.Max(rectStartPoint.X, rectEndPoint.X);
                float bottom = _graphicsControl.Height;
                selectedRect = new SKRect(left, top, right, bottom);
                invokeRendering();
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                updateMenuState();
                // 重新计算菜单的大小避免因调整菜单项引起的出现位置偏差
                this.ctxMenuStrip.AutoSize = false;
                this.ctxMenuStrip.AutoSize = true;
                this.ctxMenuStrip.Show(Cursor.Position);
                return;
            }

            if (e.Button != MouseButtons.Left) return;
            _isMousedown = false;

            // 检查是否在拖动直线
            if (isDraggingLine)
            {
                // 结束拖动直线
                isDraggingLine = false;
                draggedLineIndex = -1;
                fixedLineIndex = -1;
                Console.WriteLine($"直线拖动完成: 新距离 {verticalLines[verticalLines.Count / 2] - verticalLines[(verticalLines.Count / 2) - 1]:F1}px");
            }
            // 检查是否在调整临时矩形大小
            else if (isResizingTempRect)
            {
                // 结束调整临时矩形大小
                isResizingTempRect = false;
                Console.WriteLine($"临时矩形大小调整完成: 新尺寸 ({tempRect.Width:F0}, {tempRect.Height:F0})");
            }
            // 检查是否在拖动临时矩形
            else if (isDraggingTempRect)
            {
                // 结束拖动临时矩形
                isDraggingTempRect = false;
                Console.WriteLine($"临时矩形拖动完成: 新位置 ({tempRect.Left:F0}, {tempRect.Top:F0}) - ({tempRect.Right:F0}, {tempRect.Bottom:F0})");
            }
            // 检查是否在绘制临时矩形（pictureBox1选中状态）
            else if (isDrawingTempRect)
            {
                // 结束绘制临时矩形
                tempRectEndPoint = new SKPoint(e.X, e.Y);
                isDrawingTempRect = false;

                // 计算临时矩形区域
                float left = Math.Min(tempRectStartPoint.X, tempRectEndPoint.X);
                float top = Math.Min(tempRectStartPoint.Y, tempRectEndPoint.Y);
                float right = Math.Max(tempRectStartPoint.X, tempRectEndPoint.X);
                float bottom = Math.Max(tempRectStartPoint.Y, tempRectEndPoint.Y);
                tempRect = new SKRect(left, top, right, bottom);

                // 临时矩形不会保存到drawnRects中
                Console.WriteLine($"临时矩形绘制完成: ({left:F0}, {top:F0}) - ({right:F0}, {bottom:F0})");
            }
            else if (isDrawingRect)
            {
                rectEndPoint = new SKPoint(e.X, e.Y);

                if (rectStartPoint.X == rectEndPoint.X)
                {
                    BeatInfo clickHeartbeat = BeatInfo.Empty;
                    int clickSample = xAxisToSample(e.X);
                    foreach (var heartbeat in pageHeartbeatInfos)
                    {
                        if (heartbeat.StartIndex <= clickSample && heartbeat.EndIndex >= clickSample)
                        {
                            clickHeartbeat = heartbeat;
                            break;
                        }
                    }
                    // 尝试检测点击位置的心搏波形
                    if (Math.Abs(clickSample - clickHeartbeat.Sample) < 3.5 / speed * AnaValues.holterConfig.SampleRate)
                    {
                        // 如果找到心搏波形，添加覆盖该区域的矩形
                        float left = sampleToAxisX(Convert.ToInt32(clickHeartbeat.Sample - 3.5 / speed * AnaValues.holterConfig.SampleRate));
                        float top = TopMargin;
                        float right = sampleToAxisX(Convert.ToInt32(clickHeartbeat.Sample + 3.5 / speed * AnaValues.holterConfig.SampleRate));
                        float bottom = _graphicsControl.Height;

                        selectedRect = new SKRect(left, top, right, bottom);
                        clickPoint.X = 0;
                        isClickLine = false;

                    }
                    else
                    {
                        selectedRect = SKRect.Empty;
                        isClickLine = true;
                        clickPoint.X = e.X;
                    }
                }

                //结束绘制矩形
                isDrawingRect = false;
                _selectedBeats = getSelectedBeats();

                if (startSelectSample > 0 || clickPoint.X != e.X)
                {
                    isClickLine = false;
                    clickPoint.X = 0;
                }

            }
            else if (isDraggingAllLines)
            {
                isDraggingAllLines = false;
            }
            bModifyBeat = false;
            invokeRendering();
        }

        private void OnCanvasMouseDblClick(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastClickTime < TimeSpan.FromMilliseconds(40)) return;

            if (!_toolButtons[picEnlarge].PressedDown)
            {
                if (this.FindForm() is TemplateOverlayForm || (this.Parent.Parent.Name != "EcgDiagnosisControl" && this.Parent.Parent.Parent.Name != "FullWaveForm"))
                {
                    float timeOffsetSeconds = waveformTimeOffsetSeconds + (e.X - LeftMargin) / speed / pixMM;
                    int sample = (int)(timeOffsetSeconds * AnaValues.holterConfig.SampleRate);

                    FullWaveForm.Instance.Show(sample, this.FindForm());
                }
                if (this.Parent.Parent.Parent != null)
                {
                    if (this.Parent.Parent.Parent.Name == "FullWaveForm")
                    {
                        this.FindForm().Close();
                    }
                }

            }

            _lastClickTime = DateTime.Now;

        }

        // 鼠标滚动事件处理
        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastWheelTime < TimeSpan.FromMilliseconds(20)) return;

            // 计算滚动的秒数（根据滚动方向和当前显示行数）
            float scrollSeconds = e.Delta > 0 ? -currentRows * wheelSeconds : currentRows * wheelSeconds;

            // 累积待处理的滚动秒数
            pendingScrollSeconds += scrollSeconds;

            if (pendingScrollSeconds != 0)
            {
                // 更新时间偏移
                waveformTimeOffsetSeconds += pendingScrollSeconds;
                if ((waveformTimeOffsetSeconds + rowSeconds) * _holterConfig.SampleRate > _holterConfig.Total)
                {
                    waveformTimeOffsetSeconds = (float)_holterConfig.Total / _holterConfig.SampleRate - rowSeconds;
                }
                waveformTimeOffsetSeconds = Math.Max(0, waveformTimeOffsetSeconds);

                //waveformTimeOffsetSeconds = Math.Max(0, Math.Min(waveformTimeOffsetSeconds, hScrollBar1.Maximum - hScrollBar1.LargeChange));
                // 更新滚动条位置
                hScrollBar1.Value = Convert.ToInt32(waveformTimeOffsetSeconds);
                int middleSample = Convert.ToInt32(waveformTimeOffsetSeconds * _holterConfig.SampleRate + rowPoints / 2);
                OnEcgCommonWaveStartTimeChanged?.Invoke(rowSeconds, waveformTimeOffsetSeconds, middleSample);
                SelectSample = 0;
                clickPoint.X = 0;
                bMiddle = true;


                forceWaveformRedraw();

                // 重置待处理滚动秒数
                pendingScrollSeconds = 0;
            }

            selectedRect = SKRect.Empty;

            _lastWheelTime = DateTime.Now;

        }

        protected override int OnControlKeyPressed(object sender, KeyPressEventArgs e)
        {
            char beatType = char.ToUpper(e.KeyChar);
            switch (beatType)
            {
                case '0':
                    beatType = 'X';
                    break;
                case '1':
                    beatType = 'V';
                    break;
                case '2':
                    beatType = 'S';
                    break;
                case '3':
                    beatType = 'N';
                    break;
                case '4':
                    beatType = 'P';
                    break;
                case '5':
                    beatType = 'O';
                    break;
                case '6':
                    beatType = 'H';
                    break;
                case '.':
                    beatType = 'D';
                    break;
            }
            if (beatType == 'D' || AnaValues.BeatTypes.ContainsKey(beatType))
            {
                return HeartHeatOperate(beatType);
            }
            else
            {
                return -1;
            }
        }

        protected override void OnControlKeyDown(Keys keyData)
        {
            if (keyData == Keys.Left)
            {
                bNext = false;
                bModifyBeat = true;
                _graphicsControl?.Invalidate();
            }
            else if (keyData == Keys.Right)
            {
                bModifyBeat = true;
                _graphicsControl?.Invalidate();
            }
            return;
        }
        #endregion

        #region 右键菜单事件
        // 修改心搏类型菜单项点击事件
        private void ChangeHeartbeatTypeItem_Click(object sender, EventArgs e)
        {
            // 确保有选中的心搏
            if (selectedRect.IsEmpty && clickPoint.X == 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["请至少选择一个心搏"]);
                return;
            }

            if (!isValidSelectionRange())
            {
                ScreenCenterTip.Show(CommonValues.Langs["选择的区域包含无效的波形数据"]);
                return;
            }

            // 获取选中的新心搏类型
            if (!(sender is ToolStripMenuItem clickedItem) || !(clickedItem.Tag is char))
            {
                return;
            }

            char newType = (char)clickedItem.Tag;
            HeartHeatOperate(newType);
        }

        // 插入、删除心搏或修改心搏属性
        private int HeartHeatOperate(char newType)
        {
            newType = char.ToUpper(newType);
            int ret = -1;
            if (_selectedBeats.Count == 0)
            {
                if (newType != 'D')
                {
                    isClickLine = false;
                    float timeOffsetSeconds = waveformTimeOffsetSeconds + (clickPoint.X - LeftMargin) / speed / pixMM;
                    int sample = (int)(timeOffsetSeconds * AnaValues.holterConfig.SampleRate);
                    ret = AddBeats(new int[] { sample }, newType);
                }
            }
            else
            {
                if (_selectedBeats != null && _selectedBeats.Count > 0)
                {
                    int[] samples = _selectedBeats.Select(o => o.Sample).ToArray();
                    if (newType == 'D')
                    {
                        LastBeatMap = AnaValues.CurrentBeatMap;
                        ret = DeleteBeats(samples);
                    }
                    else if (AnaValues.BeatTypes.ContainsKey(newType))
                    {
                        ret = ModifyBeatType(samples, newType);
                    }
                }
            }

            if (ret == 0)
            {
                ContextMenuRectReset();
                invokeRendering(2); // 重绘当前可见的渲染控件
                AnaValues.DataSourceState = 1;
            }
            else
            {
                ScreenCenterTip.Show(CommonValues.Langs["操作失败"]);
            }

            return ret;
        }

        private void DeleteHeartbeatItem_Click(object sender, EventArgs e)
        {
            // 确保有选中的心搏
            if (selectedRect.IsEmpty && clickPoint.X == 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["请至少选择一个心搏"]);
                return;
            }

            if (!isValidSelectionRange())
            {
                ScreenCenterTip.Show(CommonValues.Langs["选择的区域包含无效的波形数据"]);
                return;
            }

            LastBeatMap = AnaValues.CurrentBeatMap;

            List<int> samplesK = new List<int>();
            for (int i = 0; i < _selectedBeats.Count; i++)
            {
                samplesK.Add(_selectedBeats[i].Sample);
            }
            int retK = AEcgHandler.DeleteBeats(samplesK.ToArray());
            if (retK != 0 && samplesK.Count > 0)
            {
                CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
                return;
            }
            ContextMenuRectReset();

            // 重绘当前可见的渲染控件
            invokeRendering(2);
            AnaValues.DataSourceState = 1;
            base.ChangeDataSource(null);
        }

        private void SectionReanalyzeItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem clickedItem))
            {
                return;
            }

            SegmentRange selRange = getSelectedRange();
            if (selRange.StartIndex == 0 && selRange.EndIndex == 0)
            {
                //_ = MessageBox.Show("请先选择要修改的心搏", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!isValidSelectionRange())
            {
                ScreenCenterTip.Show(CommonValues.Langs["选择的区域包含无效的波形数据"]);
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            string leadName = (string)clickedItem.Tag;
            int leadIdx = AnaValues.LeadNameToIdxMap[leadName];
            int startSample = selRange.StartIndex, endSample = selRange.EndIndex;
            int retK = AEcgHandler.ReanalyzeSegment(startSample, endSample, leadIdx);
            if (retK != 0)
            {
                CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
                return;
            }

            float interval = (float)(endSample - startSample) / _holterConfig.SampleRate;
            Logger.LogRuntimeInfo($"区间重分析(病历号:{AnaValues.CurrentCase.caseNo}，区间长度{interval.ToString("F3")}秒)", sw.ElapsedMilliseconds);

            ContextMenuRectReset();
            invokeRendering(1);
            AnaValues.DataSourceState = 1;
            base.ChangeDataSource(null);
        }

        private void mnuSetAF_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                SegmentRange selRange = getSelectedRange();
                if (selRange.StartIndex == 0 && selRange.EndIndex == 0)
                {
                    return;
                }

                if (!isValidSelectionRange())
                {
                    ScreenCenterTip.Show(CommonValues.Langs["选择的区域包含无效的波形数据"]);
                    return;
                }

                int ret = AEcgHandler.AddAf(selRange.StartIndex, selRange.EndIndex, 'K');
                if (ret == 0)
                {
                    ContextMenuRectReset();

                    // 重绘当前可见的渲染控件
                    invokeRendering(1);
                    AnaValues.DataSourceState = 1;
                    base.ChangeDataSource(null);
                }
                else
                {
                    CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
                }

            }
        }

        private void mnuSetAFL_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                SegmentRange selRange = getSelectedRange();
                if (selRange.StartIndex == 0 && selRange.EndIndex == 0)
                {
                    return;
                }

                if (!isValidSelectionRange())
                {
                    ScreenCenterTip.Show(CommonValues.Langs["选择的区域包含无效的波形数据"]);
                    return;
                }

                int ret = AEcgHandler.AddAf(selRange.StartIndex, selRange.EndIndex, 'M');
                if (ret == 0)
                {
                    ContextMenuRectReset();

                    // 重绘当前可见的渲染控件
                    invokeRendering(1);
                    AnaValues.DataSourceState = 1;
                    base.ChangeDataSource(null);
                }
                else
                {
                    CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
                }
            }
        }

        private void mnuDeleteAF_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                SegmentRange selRange = getSelectedRange();
                if (selRange.StartIndex == 0 && selRange.EndIndex == 0)
                {
                    return;
                }

                if (!isValidSelectionRange())
                {
                    ScreenCenterTip.Show(CommonValues.Langs["选择的区域包含无效的波形数据"]);
                    return;
                }

                SegmentRange segment = new SegmentRange(selRange.StartIndex, selRange.EndIndex);
                var segments = new SegmentRange[] { segment };
                int ret75 = AEcgHandler.DeleteAFEvents(segments, 'K');
                int ret77 = AEcgHandler.DeleteAFEvents(segments, 'M');
                if (ret75 == 0 && ret77 == 0)
                {
                    ContextMenuRectReset();

                    // 重绘当前可见的渲染控件
                    invokeRendering(1);
                    AnaValues.DataSourceState = 1;
                    base.ChangeDataSource(null);
                }
                else
                {
                    CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
                }

            }
        }

        //设为无效片段
        private void mnuUnanalyzable_Click(object sender, EventArgs e)
        {
            SegmentRange selRange = getSelectedRange();
            if (selRange.EndIndex > 0)
            {
                if (!_holterConfig.StartReanalysis)
                {
                    _cutRanges.Add(selRange);
                }
                else
                {
                    int ret = AEcgHandler.MarkSegmentUnanalyzable(selRange.StartIndex, selRange.EndIndex);
                    if (ret == 0)
                    {
                        //this.mnuAnalyzable.Enabled = true;
                    }
                }
                forceWaveformRedraw();
                AnaValues.DataSourceState = 1;
                base.ChangeDataSource(null);

            }
        }

        //接收无效片段
        private void mnuAnalyzable_Click(object sender, EventArgs e)
        {
            //若当前控件用于重分析, 则将剪切或无效片段从无效片段移除, 
            //否则调用算法接口移除所选无效片段
            SegmentRange selRange = getSelectedRange();
            if (selRange.EndIndex > 0)
            {
                int selStart = selRange.StartIndex;
                int selEnd = selRange.EndIndex;
                List<SegmentRange> sections = _cutRanges;
                if (_holterConfig.StartReanalysis) sections = _unanalyzableRanges;

                List<SegmentRange> intersectList = sections.FindAll(r => selStart < r.EndIndex && selEnd > r.StartIndex);
                for (int i = intersectList.Count - 1; i >= 0; i--)
                {
                    SegmentRange range = intersectList[i];
                    if (!_holterConfig.StartReanalysis)
                    {
                        if (selStart == selEnd)
                        {
                            sections.Remove(range);
                            continue;
                        }

                        if (selStart < range.StartIndex && selEnd >= range.StartIndex && selEnd < range.EndIndex) //左侧重叠
                        {
                            sections[i] = new SegmentRange(selEnd, range.EndIndex);
                        }
                        else if (selStart >= range.EndIndex && selStart < range.EndIndex && selEnd >= range.EndIndex) //右侧重叠
                        {
                            sections[i] = new SegmentRange(range.StartIndex, selStart);
                        }
                        else if (selStart >= range.StartIndex && selEnd < range.EndIndex) //选区包含于无效片段
                        {
                            sections[i] = new SegmentRange(range.StartIndex, selStart);
                            sections.Add(new SegmentRange(selEnd, range.EndIndex));
                        }
                        else //选区覆盖无效片段
                        {
                            sections.Remove(range);
                        }
                    }
                    else
                    {
                        if (selStart == selEnd)
                        {
                            AEcgHandler.MarkSegmentAnalyzable(range.StartIndex, range.EndIndex);
                            continue;
                        }

                        int start = Math.Max(range.StartIndex, selStart); //重叠区域起点
                        int end = Math.Min(range.EndIndex, selEnd); //重叠区域终点
                        AEcgHandler.MarkSegmentAnalyzable(start, end);
                    }
                }

                forceWaveformRedraw();
                AnaValues.DataSourceState = 1;
                base.ChangeDataSource(null);
            }
        }

        //设为起点
        private void mnuStartPoint_Click(object sender, EventArgs e)
        {
            if (clickPoint.X != 0)
            {
                StartPointX = clickPoint.X;
            }
            else
            {
                StartPointX = selectedRect.Left;
            }

            startSelectSample = xAxisToSample(StartPointX);

            isSetStartPoint = true;
            this.mnuEndPoint.Enabled = true;
            invokeRendering();
        }

        //设为终点
        private void mnuEndPoint_Click(object sender, EventArgs e)
        {
            if (clickPoint.X != 0)
            {
                EndPointX = clickPoint.X;
            }
            else
            {
                EndPointX = selectedRect.Left + selectedRect.Width;
            }

            endSelectSample = xAxisToSample(EndPointX);
            _selectedBeats = getSelectedBeats();
            isClickLine = false;

            isSetStartPoint = false;
            this.mnuEndPoint.Enabled = false;
            invokeRendering();
        }

        private void mnuSetLead_Click(object sender, EventArgs e)
        {
            LeadSettingForm leadSetting = new LeadSettingForm();
            leadSetting.MaxSelectedLeads = 12;
            if (leadSetting.selectedIndices.Count == 0)
            {
                leadSetting.selectedIndices.AddRange(leadIdxs);
            }

            // 订阅导联选择事件
            leadSetting.OnLeadsSelected += OnLeadsSelected;

            leadSetting.ShowDialog();
        }

        private void OnLeadsSelected(List<int> selectedIndices)
        {
            // 更新导联索引
            MultiLeads = selectedIndices;
            leadIdxs = new List<int>(MultiLeads);

            // 强制重绘波形
            invokeRendering(1);
        }

        private void mnuAddNca_Click(object sender, EventArgs e)
        {
            int clickSample = xAxisToSample(clickPoint.X);
            int beatIdx = pageHeartbeatInfos.FindIndex(b => b.Sample > clickSample);
            if (beatIdx > 0)
            {
                int[] samples = new int[1];
                int sample = pageHeartbeatInfos[beatIdx].Sample;
                int lastSample = pageHeartbeatInfos[beatIdx - 1].Sample;
                samples[0] = sample;
                AnaEvent.AddNcaOrFBeat(samples, Convert.ToInt32((double)(clickSample - lastSample) / AnaValues.holterConfig.SampleRate * 1000), "房早未下传");
                invokeRendering(0);
            }

        }
        #endregion

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (DateTime.Now - _lastScrollTime < TimeSpan.FromMilliseconds(20)) return;

            // 根据滚动事件的类型处理
            if (e.Type == ScrollEventType.SmallIncrement) // 向下箭头
            {
                pendingScrollValue = Math.Min(Convert.ToInt32(waveformTimeOffsetSeconds) + clickSeconds, hScrollBar1.Maximum - hScrollBar1.LargeChange);
            }
            else if (e.Type == ScrollEventType.SmallDecrement) // 向上箭头
            {
                pendingScrollValue = Math.Max(Convert.ToInt32(waveformTimeOffsetSeconds) - clickSeconds, 0);
            }
            else if (e.Type == ScrollEventType.LargeDecrement) // 点击滚动条上方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = hScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.X - 17) * 1.0f / hScrollBar1.Width) * (hScrollBar1.Maximum - hScrollBar1.LargeChange));
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.LargeIncrement) // 点击滚动条下方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = hScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.X + 17) * 1.0f / hScrollBar1.Width) * (hScrollBar1.Maximum - hScrollBar1.LargeChange));
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.ThumbTrack || e.Type == ScrollEventType.ThumbPosition)
            {
                // 拖动滑块时，直接设置待处理值为滑块位置
                pendingScrollValue = Math.Max(0, Math.Min(e.NewValue, hScrollBar1.Maximum - hScrollBar1.LargeChange));
            }

            ScrollBarScroll();
        }

        #endregion


        /// <summary>
        /// 右键操作后选中区域相关信息重置
        /// </summary>
        private void ContextMenuRectReset()
        {
            startSelectSample = -1;
            endSelectSample = -1;

            //clickPoint.X = 0;

            //selectedRect = SKRect.Empty;
        }

        /// <summary>
        /// 生成等距离的垂直直线位置
        /// </summary>
        private void GenerateVerticalLines()
        {
            verticalLines.Clear();

            // 计算需要绘制的直线数量（确保足够多，以便在距离缩小时有扩展空间）
            int lineCount = (int)(width / LINE_DISTANCE) + 40; // 多绘制10条确保有足够的扩展空间

            // 确保直线数量为奇数，以便有明确的中间两条直线
            if (lineCount % 2 == 0)
            {
                lineCount++;
            }

            // 计算起始位置（居中显示）
            float centerX = width / 2;
            float startX = centerX - (lineCount / 2 * LINE_DISTANCE);

            // 生成等距离直线位置
            for (int i = 0; i < lineCount; i++)
            {
                verticalLines.Add(startX + (i * LINE_DISTANCE));
            }

            // 确保中间两条直线的正确位置关系
            EnsureMiddleLinesCorrectOrder();
        }

        /// <summary>
        /// 确保中间两条直线保持正确的左右关系
        /// </summary>
        private void EnsureMiddleLinesCorrectOrder()
        {
            if (verticalLines.Count < 2)
            {
                return;
            }

            int middleIndex1 = (verticalLines.Count / 2) - 1;
            int middleIndex2 = verticalLines.Count / 2;

            float leftLineX = verticalLines[middleIndex1];
            float rightLineX = verticalLines[middleIndex2];

            // 如果左侧直线在右侧直线的右边，交换它们的位置
            if (leftLineX > rightLineX)
            {
                verticalLines[middleIndex1] = rightLineX;
                verticalLines[middleIndex2] = leftLineX;
                Console.WriteLine("中间直线位置关系已修正：左侧直线在右侧直线的左边");
            }
        }

        /// <summary>
        /// 确保所有直线都在界面范围内
        /// </summary>
        private void EnsureAllLinesInBounds()
        {
            if (verticalLines.Count == 0 || width <= 0)
            {
                return;
            }

            // 检查每条直线是否在界面范围内
            for (int i = 0; i < verticalLines.Count; i++)
            {
                // 如果直线超出左边界，调整到左边界
                if (verticalLines[i] < 0)
                {
                    verticalLines[i] = 0;
                }
                // 如果直线超出右边界，调整到右边界
                else if (verticalLines[i] > width)
                {
                    verticalLines[i] = width;
                }
            }
        }

        /// <summary>
        /// 获取鼠标所在的中间直线索引
        /// </summary>
        private int GetMiddleLineIndexAtMouse(float mouseX, float mouseY)
        {
            if (verticalLines.Count < 2 || !_toolButtons[picCaliper].PressedDown)
            {
                return -1;
            }

            int middleIndex1 = (verticalLines.Count / 2) - 1;
            int middleIndex2 = verticalLines.Count / 2;

            float line1X = verticalLines[middleIndex1];
            float line2X = verticalLines[middleIndex2];

            const float tolerance = 5.0f;

            if (Math.Abs(mouseX - line1X) <= tolerance && mouseY >= TopMargin && mouseY <= height)
            {
                return middleIndex1;
            }
            else if (Math.Abs(mouseX - line2X) <= tolerance && mouseY >= TopMargin && mouseY <= height)
            {
                return middleIndex2;
            }

            return -1;
        }

        /// <summary>
        /// 根据中间两条直线的新距离重绘所有直线
        /// </summary>
        private void RedrawLinesWithNewDistance()
        {
            if (verticalLines.Count < 2 || fixedLineIndex == -1)
            {
                return;
            }

            int middleIndex1 = (verticalLines.Count / 2) - 1;
            int middleIndex2 = verticalLines.Count / 2;

            // 使用保存的固定直线原始位置，确保绝对不动
            float fixedLineX = fixedLineOriginalX;
            float movedLineX = verticalLines[draggedLineIndex];

            // 计算中间两条直线的新距离
            float newDistance = Math.Abs(movedLineX - fixedLineX);

            // 确保两条中间直线保持正确的左右关系
            EnsureMiddleLinesCorrectOrder();

            // 保存两条中间直线的当前位置
            float savedLeftLineX = verticalLines[middleIndex1];
            float savedRightLineX = verticalLines[middleIndex2];

            // 简化重绘逻辑：只重新生成非中间直线
            // 以两条中间直线为中心，向两边生成直线
            List<float> newLines = new List<float>();

            // 计算需要向两边生成的直线数量
            int linesToLeft = Math.Max(middleIndex1, 10);  // 至少生成10条左边的直线
            int linesToRight = Math.Max(verticalLines.Count - middleIndex2 - 1, 10);  // 至少生成10条右边的直线

            // 生成左边的直线（从左侧直线向左）
            for (int i = linesToLeft; i > 0; i--)
            {
                newLines.Add(savedLeftLineX - (i * newDistance));
            }

            // 添加两条中间直线
            newLines.Add(savedLeftLineX);
            newLines.Add(savedRightLineX);

            // 生成右边的直线（从右侧直线向右）
            for (int i = 1; i <= linesToRight; i++)
            {
                newLines.Add(savedRightLineX + (i * newDistance));
            }

            verticalLines = newLines;

            // 确保所有直线都在界面范围内
            EnsureAllLinesInBounds();

            // 最终确保两条中间直线的正确位置关系
            EnsureMiddleLinesCorrectOrder();
        }


        /// <summary>
        /// 初始化增益设置菜单
        /// </summary>
        private void InitializeGainMenu()
        {
            // 创建上下文菜单
            gainMenuStrip = new ContextMenuStrip();

            // 添加菜单项
            string[] gainOptions = { "2.5mm/mV", "5mm/mV", "10mm/mV", "20mm/mV", "40mm/mV" };

            foreach (string gainOption in gainOptions)
            {
                float gainValue = Convert.ToSingle(gainOption.Replace("mm/mV", "").Trim());
                ToolStripMenuItem menuItem = new ToolStripMenuItem(gainOption);
                menuItem.Click += (sender, e) =>
                {
                    // 处理增益设置选择
                    HandleGainSelection(gainOption);
                    menuItem.Check();
                };
                gainMenuStrip.Items.Add(menuItem);
                if (gain == gainValue) menuItem.Check();
            }

            // 设置菜单样式
            gainMenuStrip.BackColor = Color.White;
            gainMenuStrip.ForeColor = Color.Black;
            gainMenuStrip.Font = new Font("微软雅黑", 9);
        }

        /// <summary>
        /// 处理增益设置选择
        /// </summary>
        private void HandleGainSelection(string gainOption)
        {
            // 从菜单项文本中提取纯数字增益值（去掉"mm/mV"单位）
            string gainValue = gainOption.Replace("mm/mV", "").Trim();

            // 尝试转换为浮点数
            if (float.TryParse(gainValue, out float newGain))
            {
                // 根据选择的增益设置更新显示
                Console.WriteLine($"选择的增益数值: {newGain}");

                if (gain != newGain)
                {
                    gain = newGain;
                    forceWaveformRedraw();
                }
            }
            else
            {
                Console.WriteLine($"无法解析增益值: {gainOption}");
            }

            picGain.BackColor = SystemColors.Control;
            picGain.Invalidate();
            HideGainMenu();
        }

        /// <summary>
        /// 显示增益设置菜单
        /// </summary>
        private void ShowGainMenu()
        {
            if (gainMenuStrip != null)
            {
                // 计算菜单显示位置（在pictureBox5上方）
                Point menuLocation = new Point(
                    picGain.Right - gainMenuStrip.Width,
                    picGain.Top - gainMenuStrip.Height
                );

                // 显示菜单
                gainMenuStrip.Show(picGain.Parent, menuLocation);
            }
        }

        /// <summary>
        /// 隐藏增益设置菜单
        /// </summary>
        private void HideGainMenu()
        {
            if (gainMenuStrip != null && gainMenuStrip.Visible)
            {
                gainMenuStrip.Hide();
            }
        }

        /// <summary>
        /// 初始化纸速设置菜单
        /// </summary>
        private void InitializeSpeedMenu()
        {
            // 创建上下文菜单
            speedMenuStrip = new ContextMenuStrip();

            // 添加菜单项
            string[] speedOptions = { "6.25mm/s", "12.5mm/s", "25mm/s", "50mm/s" };

            foreach (string speedOption in speedOptions)
            {
                float speedValue = Convert.ToSingle(speedOption.Replace("mm/s", "").Trim());
                ToolStripMenuItem menuItem = new ToolStripMenuItem(speedOption);
                menuItem.Click += (sender, e) =>
                {
                    // 处理纸速设置选择
                    HandleSpeedSelection(speedOption);
                    menuItem.Check();
                };
                speedMenuStrip.Items.Add(menuItem);
                if (speed == speedValue) menuItem.Check();
            }

            // 设置菜单样式
            speedMenuStrip.BackColor = Color.White;
            speedMenuStrip.ForeColor = Color.Black;
            speedMenuStrip.Font = new Font("微软雅黑", 9);
        }

        /// <summary>
        /// 处理纸速设置选择
        /// </summary>
        private void HandleSpeedSelection(string speedOption)
        {
            // 从菜单项文本中提取纯数字纸速值（去掉"mm/s"单位）
            string speedValue = speedOption.Replace("mm/s", "").Trim();

            // 尝试转换为浮点数
            if (float.TryParse(speedValue, out float newSpeed))
            {
                // 根据选择的纸速设置更新显示
                Console.WriteLine($"选择的纸速数值: {newSpeed}");

                if (speed != newSpeed)
                {
                    speed = newSpeed;
                    forceWaveformRedraw();

                    OnEcgCommonRowSecondsChanged?.Invoke(rowSeconds);
                }
            }
            else
            {
                Console.WriteLine($"无法解析纸速值: {speedOption}");
            }

            picSpeed.BackColor = SystemColors.Control;
            picSpeed.Invalidate();
            HideSpeedMenu();
        }

        /// <summary>
        /// 显示纸速设置菜单
        /// </summary>
        private void ShowSpeedMenu()
        {
            if (speedMenuStrip != null)
            {
                // 计算菜单显示位置（在pictureBox6上方）
                Point menuLocation = new Point(
                    picSpeed.Right - speedMenuStrip.Width,
                    picSpeed.Top - speedMenuStrip.Height
                );

                // 显示菜单
                speedMenuStrip.Show(picSpeed.Parent, menuLocation);
            }
        }

        /// <summary>
        /// 隐藏纸速设置菜单
        /// </summary>
        private void HideSpeedMenu()
        {
            if (speedMenuStrip != null && speedMenuStrip.Visible)
            {
                speedMenuStrip.Hide();
            }
        }


        public void RefreshData(bool resetToInitialState = false)
        {
            //_holterConfig = AnaValues.holterConfig;
            _cutRanges.Clear();
            resetToolButtons();
            if (resetToInitialState)
            {
                resetLeads();
                waveformTimeOffsetSeconds = 0;
                pendingScrollSeconds = 0;
                hScrollBar1.Value = 0;
                //_LeadChangePopup?.Close();
                //_LeadChangePopup = null;

                InitializeContextMenu();
            }
            _unanalyzableRanges.Clear();

            forceWaveformRedraw();
        }

        private void invokeRendering(int updateData = 0)
        {
            bModifyBeat = false;
            if (updateData > 0)
            {
                updateUnanalyzableRanges();
                updateEcgData();
                if (updateData == 2)
                {
                    bModifyBeat = true;
                }
            }
            _graphicsControl?.Invalidate();
        }

        // 强制重绘波形
        public void forceWaveformRedraw()
        {
            if (IsLoaded)
            {
                updateUnanalyzableRanges();
                updateEcgData();
                _graphicsControl?.Refresh();
            }
        }

        private void init()
        {
            initGraphicControl();

            initUIControl();

            drawEcgWave = new DrawEcgWave();
            _unanalyzableRanges.Clear();

        }

        // 绘图控件初始化
        private void initGraphicControl()
        {
            // 添加鼠标事件处理程序到主渲染控件
            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            _graphicsControl.MouseDoubleClick += OnCanvasMouseDblClick;
            _graphicsControl.MouseWheel += OnCanvasMouseWheel;

            this.panel1.Controls.Add(_graphicsControl);

            // 初始化右键菜单
            InitializeContextMenu();
        }

        // 初始化右键菜单
        public void InitializeContextMenu()
        {
            this.ctxMenuStrip.Items.Clear();

            mnuDeleteBeat = new ToolStripMenuItem("(D)" + _langs["删除心搏"]);
            this.ctxMenuStrip.Items.Add(mnuDeleteBeat);

            toolStripSeparator1 = new ToolStripSeparator();
            this.ctxMenuStrip.Items.Add(toolStripSeparator1);

            mnuAddNca = new ToolStripMenuItem(_langs["添加房早未下传"]);
            this.ctxMenuStrip.Items.Add(mnuAddNca);

            mnuStartPoint = new ToolStripMenuItem(_langs["设为起点"]);
            this.ctxMenuStrip.Items.Add(mnuStartPoint);

            mnuEndPoint = new ToolStripMenuItem(_langs["设为终点"]);
            this.ctxMenuStrip.Items.Add(mnuEndPoint);

            mnuUnanalyzable = new ToolStripMenuItem(_langs["设为非分析区间"]);
            this.ctxMenuStrip.Items.Add(mnuUnanalyzable);

            mnuAnalyzable = new ToolStripMenuItem(_langs["恢复为可分析区间"]);
            this.ctxMenuStrip.Items.Add(mnuAnalyzable);

            mnuSetAF = new ToolStripMenuItem(_langs["设为房颤"]);
            this.ctxMenuStrip.Items.Add(mnuSetAF);

            mnuSetAFL = new ToolStripMenuItem(_langs["设为房扑"]);
            this.ctxMenuStrip.Items.Add(mnuSetAFL);

            mnuDeleteAF = new ToolStripMenuItem(_langs["删除房颤/房扑"]);
            this.ctxMenuStrip.Items.Add(mnuDeleteAF);

            // 添加修改心搏类型子菜单
            List<char> types = AnaValues.BeatTypes.Keys.ToList();
            for (int i = 0; i < types.Count; i++)
            {
                char beatType = types[i];
                ToolStripMenuItem mnuItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.BeatTypes[beatType]]);
                mnuItem.Tag = beatType;
                mnuItem.Click += ChangeHeartbeatTypeItem_Click;
                this.ctxMenuStrip.Items.Insert(i, mnuItem);
            }

            //删除心搏
            this.mnuDeleteBeat.Click += DeleteHeartbeatItem_Click;

            // 添加区间重分析子菜单
            ToolStripMenuItem SectionReanalyzeMenuItem = new ToolStripMenuItem(_langs["区间重分析"]);
            foreach (string leadName in AnaValues.LeadNameToIdxMap.Keys)
            {
                ToolStripMenuItem subMenuItem = new ToolStripMenuItem($"{leadName}")
                {
                    Tag = leadName
                };
                subMenuItem.Click += SectionReanalyzeItem_Click;
                SectionReanalyzeMenuItem.DropDownItems.Add(subMenuItem);
            }
            this.ctxMenuStrip.Items.Add(SectionReanalyzeMenuItem);

            mnuSetLead = new ToolStripMenuItem(_langs["设置导联"]);
            this.ctxMenuStrip.Items.Add(mnuSetLead);

            this.mnuUnanalyzable.Click += mnuUnanalyzable_Click;
            this.mnuAnalyzable.Click += mnuAnalyzable_Click;
            this.mnuStartPoint.Click += mnuStartPoint_Click;
            this.mnuEndPoint.Click += mnuEndPoint_Click;
            this.mnuSetAF.Click += mnuSetAF_Click;
            this.mnuSetAFL.Click += mnuSetAFL_Click;
            this.mnuDeleteAF.Click += mnuDeleteAF_Click;
            this.mnuSetLead.Click += mnuSetLead_Click;
            this.mnuAddNca.Click += mnuAddNca_Click;
            // 将右键菜单关联到绘图控件
            //_graphicsControl.ContextMenuStrip = ctxMenuStrip;
        }

        private void initUIControl()
        {
            if (!DesignMode)
            {
                // 初始化工具栏按钮
                initializeToolButtons();

                if (UsedForST == true)
                {
                    tableLayoutPanel3.Visible = false;
                }

                resetLeads();
            }
        }

        // 初始化工具栏按钮
        private void initializeToolButtons()
        {
            _toolButtons = new Dictionary<PictureBox, ToolButton>()
            {
                { picCutStart, new ToolButton(){ PicControl=this.picCutStart, ForReanalyze=true, KeepState=false, IsPairButton=true }},
                { picCutEnd, new ToolButton(){ PicControl=this.picCutEnd, ForReanalyze=true, KeepState=false, IsPairButton=true, Disabled=true }},
                { picAnaStart, new ToolButton(){ PicControl=this.picAnaStart, ForReanalyze=true, KeepState=false, IsPairButton=true }},
                { picAnaEnd, new ToolButton(){ PicControl=this.picAnaEnd, ForReanalyze=true, KeepState=false, IsPairButton=true, Disabled=true }},
                { picLeadChange, new ToolButton(){ PicControl=this.picLeadChange, ForReanalyze=true, KeepState=false }},
                { picReset, new ToolButton(){ PicControl=this.picReset, ForReanalyze=true, KeepState=false }},
                { picMeasure, new ToolButton(){ PicControl=this.picMeasure, ForReanalyze=false, KeepState=true }}, //测量
                { picCaliper, new ToolButton(){ PicControl=this.picCaliper, ForReanalyze=false, KeepState=true }}, //测径器
                { picPrint, new ToolButton(){ PicControl=this.picPrint, IsCommonButton=true, ForReanalyze=false, KeepState=false }},  //打印
                { picSaveSegment, new ToolButton(){ PicControl=this.picSaveSegment, ForReanalyze=false, KeepState=false }}, //保存片段图
                { picGain, new ToolButton(){ PicControl=this.picGain, IsCommonButton=true, ForReanalyze=false, KeepState=false }},    //增益
                { picSpeed, new ToolButton(){ PicControl=this.picSpeed, IsCommonButton=true, ForReanalyze=false, KeepState=false }},  //纸速
                { picEnlarge, new ToolButton(){ PicControl=this.picEnlarge, ForReanalyze=false, KeepState=true }}, //放大
            };

            this.toolTip1.SetToolTip(picCutStart, _langs["设为剪切起点"]);
            this.toolTip1.SetToolTip(picCutEnd, _langs["设为剪切终点"]);
            this.toolTip1.SetToolTip(picAnaStart, _langs["设为分析起点"]);
            this.toolTip1.SetToolTip(picAnaEnd, _langs["设为分析终点"]);
            this.toolTip1.SetToolTip(picLeadChange, _langs["导联纠错"]);
            this.toolTip1.SetToolTip(picReset, _langs["复位"]);
            this.toolTip1.SetToolTip(picMeasure, _langs["测量"]);
            this.toolTip1.SetToolTip(picCaliper, _langs["测径器"]);
            this.toolTip1.SetToolTip(picPrint, _langs["打印"]);
            this.toolTip1.SetToolTip(picSaveSegment, _langs["保存片段图"]);
            this.toolTip1.SetToolTip(picGain, _langs["增益"]);
            this.toolTip1.SetToolTip(picSpeed, _langs["纸速"]);
            this.toolTip1.SetToolTip(picEnlarge, _langs["放大"]);

            this.picMeasure.Image = CommonFuns.GetResizedImage(Properties.Resources.measure, 18, 18);
            this.picCaliper.Image = CommonFuns.GetResizedImage(Properties.Resources.designation, 18, 18);
            this.picPrint.Image = CommonFuns.GetResizedImage(Properties.Resources.printSet, 18, 18);
            this.picSaveSegment.Image = CommonFuns.GetResizedImage(Properties.Resources.setSegment, 18, 18);
            this.picGain.Image = CommonFuns.GetResizedImage(Properties.Resources.gain, 18, 18);
            this.picSpeed.Image = CommonFuns.GetResizedImage(Properties.Resources.speed, 18, 18);
            this.picEnlarge.Image = CommonFuns.GetResizedImage(Properties.Resources.ecgScale, 18, 18);
            this.picCutStart.Image = CommonFuns.GetResizedImage(Properties.Resources.splitstart, 18, 18);
            this.picCutEnd.Image = CommonFuns.GetResizedImage(Properties.Resources.splitend, 18, 18);
            this.picAnaStart.Image = CommonFuns.GetResizedImage(Properties.Resources.anastart, 18, 18);
            this.picAnaEnd.Image = CommonFuns.GetResizedImage(Properties.Resources.anaend, 18, 18);
            this.picReset.Image = CommonFuns.GetResizedImage(Properties.Resources.reset, 18, 18);
            this.picLeadChange.Image = CommonFuns.GetResizedImage(Properties.Resources.leadChange, 18, 18);

            foreach (ToolButton btn in _toolButtons.Values)
            {
                PictureBox picBox = btn.PicControl;
                if (UsedForReanalyze)
                    picBox.Visible = (btn.ForReanalyze || btn.IsCommonButton);
                else
                    picBox.Visible = !btn.ForReanalyze;

                picBox.SizeMode = PictureBoxSizeMode.CenterImage;
                picBox.Click += (o, e) => { updateToolButtonState(btn); refreshToolButtons(); };
                btn.OriginalImage = picBox.Image;
                btn.GrayScaleImage = PubFuns.GetGrayScaleImage(picBox.Image);

                if (UsedForST == true)
                {
                    picBox.Visible = false;
                }
            }

            //点击事件
            this.picCutStart.Click += (o, e) =>
            {
                if (_toolButtons[picCutStart].PressedDown)
                {
                    isSetStartPoint = false;
                    forceWaveformRedraw();
                    ScreenCenterTip.Show(CommonValues.Langs["设置成功"]);
                }
            };
            this.picCutEnd.Click += (o, e) =>
            {
                if (_toolButtons[picCutEnd].PressedDown)
                {
                    int start = Math.Min(selectStartSample, selectEndSample);
                    int end = Math.Max(selectStartSample, selectEndSample);
                    _cutRanges.Add(new SegmentRange(start, end));
                    forceWaveformRedraw();
                    ScreenCenterTip.Show(CommonValues.Langs["设置成功"]);
                }
            };
            this.picAnaStart.Click += (o, e) =>
            {
                if (_toolButtons[picAnaStart].PressedDown) ScreenCenterTip.Show(CommonValues.Langs["设置成功"]);
            };
            this.picAnaEnd.Click += (o, e) =>
            {
                if (_toolButtons[picAnaEnd].PressedDown)
                {
                    int start = Math.Min(selectStartSample, selectEndSample);
                    int end = Math.Max(selectStartSample, selectEndSample);
                    //_anaRange = new SegmentRange(start, end);
                    _cutRanges.Clear();
                    _cutRanges.Add(new SegmentRange(0, start));
                    _cutRanges.Add(new SegmentRange(end, _holterConfig.Total - 1));
                    forceWaveformRedraw();
                    ScreenCenterTip.Show(CommonValues.Langs["设置成功"]);
                }
            };
            this.picReset.Click += (o, e) =>
            {
                _cutRanges.Clear();
                resetToolButtons();
                forceWaveformRedraw();
            };
            this.picLeadChange.Click += (o, e) =>
            {
                if (_LeadChangePopup == null) _LeadChangePopup = new LeadChangePopup();

                _LeadChangePopup.ShowDialog();
                if (_LeadChangePopup.DialogResult == DialogResult.OK)
                {
                    //int setConf = AEcgHandler.SetConfig(AnaValues.RootPath);
                    //if (setConf == 0)
                    {
                        int ret = AEcgHandler.CorrectLeads(_LeadChangePopup.LimbLeadChanged, null);
                        if (ret == 0)
                        {
                            //更新配置文件
                            Limbleads = _LeadChangePopup.Limbleads;
                            HolterConfig cfg = AnaValues.holterConfig;
                            cfg.LeadConfig.LfLatest = Limbleads;
                            cfg.SaveToFile(Path.Combine(AnaValues.RootPath, "holter.info"));
                            AnaValues.holterConfig = cfg;

                            //重新渲染各导联波形
                            forceWaveformRedraw();

                            int clickPos = 0;
                            if (_selectedBeats != null && _selectedBeats.Count > 0)
                            {
                                //OnEcgCommonWaveClickChanged?.Invoke(waveformTimeOffsetSeconds, _selectedBeats[0].StartIndex, _selectedBeats[0].Sample, _selectedBeats[0].EndIndex);
                                clickPos = _selectedBeats[0].Sample;
                            }
                            else
                            {
                                clickPos = xAxisToSample(clickPoint.X);
                            }
                            OnEcgCommonWaveClickChanged?.Invoke(0, 0, clickPos, 0);
                        }
                    }
                }
            };
            this.picMeasure.Click += picMeasure_Click;
            this.picCaliper.Click += picCaliper_Click;
            this.picPrint.Click += picPrint_Click;
            this.picSaveSegment.Click += picSaveSegment_Click;
            this.picGain.Click += picGain_Click;
            this.picSpeed.Click += picSpeed_Click;
            this.picEnlarge.Click += picEnlarge_Click;
        }

        private void resetToolButtons()
        {
            //foreach (ToolButton btn in _toolButtons.Values) { btn.PressedDown = false; }
            _toolButtons[picCutStart].Disabled = false;
            _toolButtons[picCutEnd].Disabled = true;
            _toolButtons[picAnaStart].Disabled = false;
            _toolButtons[picAnaEnd].Disabled = true;
            _toolButtons[picMeasure].PressedDown = false;
            _toolButtons[picCaliper].PressedDown = false;
            refreshToolButtons();
        }

        private void resetLeads()
        {
            leadIdxs.Clear();
            if (currentLeadCount == 3)
            {
                leadIdxs.Add(_holterConfig.MainChannel);
                leadIdxs.Add(_holterConfig.SecondChannel1);
                leadIdxs.Add(_holterConfig.SecondChannel2);
            }
            else if (currentLeadCount == 12)
            {
                for (int i = 0; i < 12; i++) { leadIdxs.Add(i); }
            }
            MultiLeads = leadIdxs;
        }

        private void updateWaveform(SKCanvas canvas)
        {
            setDefaultSelect();

            drawEcgWave.WaveformTimeOffsetSeconds = waveformTimeOffsetSeconds;
            drawEcgWave.speed = speed;
            drawEcgWave.gain = gain;
            drawEcgWave.LeftMargin = LeftMargin;
            drawEcgWave.TopMargin = TopMargin;
            drawEcgWave.CurrentLeadCount = leadIdxs.Count;
            drawEcgWave.CurrentRows = currentRows;
            drawEcgWave.RowPoints = rowPoints;
            drawEcgWave.EcgData = ecgData;
            drawEcgWave.pageHeartbeatInfos = pageHeartbeatInfos;
            drawEcgWave.leadIdxs = leadIdxs;

            drawEcgWave.ShowRrHrInfo = true;
            drawEcgWave.bShowGrid = bShowGrid;
            drawEcgWave.step = 1;

            try
            {
                // 绘制背景
                drawEcgWave.DrawBackground(canvas, width, height);
                //绘制剪切片段
                drawCutSections(canvas);
                //绘制无效片段
                drawUnanalyzableSections(canvas);
                //绘制心搏信息
                drawEcgWave.DrawBeatHeartInfo(canvas, width, TopMargin);
                // 绘制ECG波形
                drawEcgWave.DrawEcgWaveform(canvas, width, height);
                // 绘制导联名称
                drawEcgWave.DrawLeadNames(canvas, width, height, rowSeconds);
                // 绘制定标电压
                drawEcgWave.DrawCalibration(canvas, width, height);
                // 绘制当前波形起始时间
                drawEcgWave.DrawCurrentWaveformStartTime(canvas, width, height);
                // 绘制纸速增益信息
                drawEcgWave.DrawSpeedGainInfo(canvas, width, height);
                // 绘制起搏钉
                drawEcgWave.DrawPaceStatus(canvas, width, height);

                drawStartEndPoint(canvas);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用心电图");
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

        private void updateEcgData()
        {
            if (drawEcgWave == null) { return; }
            if (leadIdxs.Count <= 0) return;

            _ = DateTime.Now.Ticks;

            // 清空数据并重置标志
            ecgData.Clear();

            rowSeconds = (float)(width - LeftMargin) / pixMM / speed;
            rowPoints = Convert.ToInt32((width - LeftMargin) / pixMM / speed * AnaValues.holterConfig.SampleRate);
            wheelSeconds = rowSeconds - 2.0f;
            int sampleOffset = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            ecgData = AnaCode.ReadAllWaveData(leadIdxs, sampleOffset, sampleOffset + currentRows * rowPoints - 1);

            //获取并存储每个采样点的状态位
            //_ = AnaValues.HolterDataReader.GetSampleStatusBits();

            updateScrollBarMax();
            pageHeartbeatInfos.Clear();
            drawnDelectRects.Clear();

            if (ecgData.Count > 0)
            {
                int startSample = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
                int endSample = startSample + ecgData[leadIdxs[0]].DataLength;
                pageHeartbeatInfos = AnaValues.CachedAllBeats
                    .Where(h => (h.EndIndex > startSample && h.StartIndex < endSample))
                    .OrderBy(h => h.StartIndex)
                    .ToList();
            }
        }

        private void updateScrollBarMax()
        {
            if (ecgData.Count > 0 && ecgData[leadIdxs[0]].DataLength > 0)
            {
                // 计算总秒数
                int totalSeconds = (_holterConfig.Total + AnaValues.holterConfig.SampleRate - 1) / AnaValues.holterConfig.SampleRate;
                int pageCount = (totalSeconds + clickSeconds - 1) / clickSeconds;
                hScrollBar1.LargeChange = Math.Max(0, pageCount / 10);
                int max = (_holterConfig.Total - clickSeconds * AnaValues.holterConfig.SampleRate + AnaValues.holterConfig.SampleRate - 1) / AnaValues.holterConfig.SampleRate;
                hScrollBar1.Maximum = Math.Max(0, max);
                hScrollBar1.Maximum += hScrollBar1.LargeChange;
                // 调试信息输出
                //Console.WriteLine($"[UpdateScrollBarMax] 总采样点: {holterConfig.Total:N0}, 采样率: {AnaValues.holterConfig.SampleRate}Hz, 总秒数: {totalSeconds}, 滚动条最大值: {hScrollBar1.Maximum}");
            }
        }

        //根据当前点击的位置更新右键菜单项的状态
        private void updateMenuState()
        {
            List<ToolStripItem> beatTypeItems = new List<ToolStripItem>();
            int separatorIndex = this.ctxMenuStrip.Items.IndexOf(this.toolStripSeparator1);
            for (int i = 0; i < this.ctxMenuStrip.Items.Count; i++)
            {
                ToolStripItem item = this.ctxMenuStrip.Items[i];
                item.Visible = false;
                if (i <= separatorIndex) { beatTypeItems.Add(item); }
            }
            this.mnuUnanalyzable.Enabled = this.mnuAnalyzable.Enabled = true;

            SegmentRange selectRange = getSelectedRange();

            if (selectRange.StartIndex == selectRange.EndIndex)
            {
                beatTypeItems.ForEach(item => item.Visible = true); //用于插入心搏
                this.mnuStartPoint.Visible = this.mnuEndPoint.Visible = true;
            }
            else  //if (selectRange.StartIndex != 0 && selectRange.EndIndex != 0)
            {
                List<SegmentRange> sections = _cutRanges;
                if (_holterConfig.StartReanalysis) sections = _unanalyzableRanges;

                if (sections.Any(r => r.StartIndex <= selectRange.StartIndex && selectRange.EndIndex < r.EndIndex))
                {
                    //左键点击的位置或选择的区间在不可分析段内
                    this.mnuUnanalyzable.Visible = this.mnuAnalyzable.Visible = true;
                    this.mnuUnanalyzable.Enabled = false;
                    this.mnuStartPoint.Visible = this.mnuEndPoint.Visible = true;
                }
                else
                {
                    foreach (ToolStripItem tsItem in this.ctxMenuStrip.Items) { tsItem.Visible = true; }
                    this.mnuStartPoint.Visible = this.mnuEndPoint.Visible = false;

                    if (sections.Exists(r => selectRange.StartIndex < r.EndIndex && selectRange.EndIndex > r.StartIndex))
                    {
                        //选择的区间与不可分析段部分重叠
                        this.mnuUnanalyzable.Enabled = this.mnuAnalyzable.Enabled = true;
                    }
                    else
                    {
                        //选择的区间与不可分析段部分无交集
                        this.mnuUnanalyzable.Enabled = true;
                        this.mnuAnalyzable.Enabled = false;
                    }
                }
            }

            if (clickPoint.X == 0)
            {
                this.mnuAddNca.Visible = false;
            }
            else
            {
                this.mnuAddNca.Visible = true;
                this.mnuDeleteBeat.Visible = false;
            }

            this.mnuSetLead.Visible = true;

        }


        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            // 清除画布
            canvas.Clear(SKColors.Transparent);

            // 确保离屏缓冲区已更新
            width = imageWidth;
            height = imageHeight;
            updateWaveform(canvas);

            // 绘制矩形
            drawRectangles(canvas);

            // 绘制垂直直线（当pictureBox2选中时）
            drawVerticalLines(canvas);

        }

        private void drawStartEndPoint(SKCanvas canvas)
        {
            if (isSetStartPoint)
            {
                if (startSelectSample >= waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate && startSelectSample <= waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate + rowPoints)
                {
                    using (SKPaint linePaint = new SKPaint()
                    {
                        Color = SKColors.LightPink,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 2.5f,
                        IsAntialias = true,
                    })
                    {
                        canvas.DrawLine(StartPointX, TopMargin, StartPointX, height, linePaint);
                    }
                }

            }
            else
            {
                if (endSelectSample > 0)
                {
                    using (SKPaint rectPaint = new SKPaint()
                    {
                        Color = new SKColor(74, 122, 196, 100), // 半透明浅蓝色
                        StrokeWidth = 1,
                        StrokeJoin = SKStrokeJoin.Round,
                        Style = SKPaintStyle.StrokeAndFill,
                        IsAntialias = true,
                        BlendMode = SKBlendMode.SrcOver
                    })
                    {

                        if (endSelectSample >= waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate)
                        {
                            //SKRect selectedRect = SKRect.Empty;
                            if (endSelectSample < waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate + rowPoints)
                            {
                                if (startSelectSample <= waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate)
                                {
                                    float left = LeftMargin;
                                    float top = TopMargin;
                                    float right = sampleToAxisX(endSelectSample);
                                    float bottom = _graphicsControl.Height;
                                    selectedRect = new SKRect(left, top, right, bottom);
                                }
                                else
                                {
                                    float left = sampleToAxisX(startSelectSample);
                                    float top = TopMargin;
                                    float right = sampleToAxisX(endSelectSample);
                                    float bottom = _graphicsControl.Height;
                                    selectedRect = new SKRect(left, top, right, bottom);
                                }
                            }
                            else
                            {
                                if (startSelectSample <= waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate)
                                {
                                    float left = LeftMargin;
                                    float top = TopMargin;
                                    float right = width;
                                    float bottom = _graphicsControl.Height;
                                    selectedRect = new SKRect(left, top, right, bottom);
                                }
                                else
                                {
                                    float left = sampleToAxisX(startSelectSample);
                                    float top = TopMargin;
                                    float right = width;
                                    float bottom = _graphicsControl.Height;
                                    selectedRect = new SKRect(left, top, right, bottom);
                                }
                            }

                        }
                    }
                }
            }
        }

        private void drawCutSections(SKCanvas canvas)
        {
            using (SKPaint cutPaint = new SKPaint()
            {
                IsStroke = false,
                Color = new SKColor(128, 128, 128, 64),
                BlendMode = SKBlendMode.SrcOver
            })
            {
                if (_toolButtons[picCutStart].PressedDown)
                {
                    cutPaint.Color = SKColors.Pink;
                    cutPaint.StrokeWidth = 2.5F;
                    float cutStartPos = sampleToAxisX(selectStartSample);
                    canvas.DrawLine(cutStartPos, TopMargin, cutStartPos, height, cutPaint);
                }

                foreach (SegmentRange range in _cutRanges)
                {
                    SKRect rect = new SKRect(sampleToAxisX(range.StartIndex), TopMargin, sampleToAxisX(range.EndIndex), height);
                    canvas.DrawRect(rect, cutPaint);
                }
            }
        }

        private void drawUnanalyzableSections(SKCanvas canvas)
        {
            using (SKPaint cutPaint = new SKPaint()
            {
                IsStroke = false,
                Color = new SKColor(128, 128, 128, 64),
                BlendMode = SKBlendMode.SrcOver
            })
            {
                int waveStartIndex = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
                int waveEndIndex = waveStartIndex + rowPoints;
                foreach (SegmentRange range in _unanalyzableRanges)
                {
                    if (range.StartIndex <= waveEndIndex && range.EndIndex >= waveStartIndex)
                    {
                        SKRect rect = new SKRect(sampleToAxisX(range.StartIndex), TopMargin, sampleToAxisX(range.EndIndex), height);
                        canvas.DrawRect(rect, cutPaint);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制垂直直线(测径器)
        /// </summary>
        private void drawVerticalLines(SKCanvas canvas)
        {
            if (verticalLines.Count == 0 || !_toolButtons[picCaliper].PressedDown)
            {
                return;
            }

            // 创建直线画笔
            using (SKPaint linePaint = new SKPaint())
            {
                linePaint.Color = SKColors.Blue;
                linePaint.Style = SKPaintStyle.Stroke;
                linePaint.StrokeWidth = 1.5f; // 普通直线宽度
                linePaint.IsAntialias = true;

                // 绘制所有直线
                for (int i = 0; i < verticalLines.Count; i++)
                {
                    float x = verticalLines[i];

                    // 如果是中间两条直线，加粗显示
                    if (verticalLines.Count >= 2)
                    {
                        int middleIndex1 = (verticalLines.Count / 2) - 1;
                        int middleIndex2 = verticalLines.Count / 2;

                        if (i == middleIndex1 || i == middleIndex2)
                        {
                            linePaint.StrokeWidth = 3.0f; // 加粗显示
                        }
                        else
                        {
                            linePaint.StrokeWidth = 1.5f; // 普通宽度
                        }
                    }

                    canvas.DrawLine(x, TopMargin, x, height, linePaint);
                }
            }

            // 绘制中间两条竖线之间的宽度信息
            if (verticalLines.Count >= 2)
            {
                int middleIndex1 = (verticalLines.Count / 2) - 1;
                int middleIndex2 = verticalLines.Count / 2;

                float leftLineX = verticalLines[middleIndex1];
                float rightLineX = verticalLines[middleIndex2];
                float widthBetweenLines = Math.Abs(rightLineX - leftLineX);
                int betweenLinesMs = Convert.ToInt32(widthBetweenLines / speed / pixMM * 1000);
                int betweenLinesHR = 60000 / betweenLinesMs;

                // 创建文本画笔
                using (SKPaint textPaint = new SKPaint())
                {
                    textPaint.Color = SKColors.Black;
                    textPaint.Style = SKPaintStyle.Fill;
                    textPaint.IsAntialias = true;

                    // 创建字体
                    using (SKFont textFont = new SKFont())
                    {
                        textFont.Size = 10.5f;
                        textFont.Typeface = SKTypeface.FromFamilyName("微软雅黑");

                        // 显示宽度信息
                        string drawText = $"{betweenLinesMs}ms {betweenLinesHR}bpm";

                        // 计算文本位置（在两条竖线中间上方显示）
                        float centerX = leftLineX + 2 * pixMM;
                        float textY = height - 10; // 在直线上方显示

                        // 绘制宽度文本
                        canvas.DrawText(drawText, centerX, textY, SKTextAlign.Left, textFont, textPaint);
                    }
                }
            }
        }

        private void drawRectangles(SKCanvas canvas)
        {
            // 创建矩形画笔
            using (SKPaint rectPaint = new SKPaint())
            {
                rectPaint.IsAntialias = true;

                if (UsedForST == false)
                {
                    //绘制打印框
                    float bigLeftX = Math.Max((width / 2) - (3.5f * speed * pixMM), LeftMargin);
                    float bigRightX = Math.Min(width, (width / 2) + (3.5f * speed * pixMM));
                    SKRect heartbeatBigRect = new SKRect(bigLeftX, TopMargin, bigRightX, height);

                    rectPaint.Color = new SKColor(255, 140, 0);
                    rectPaint.Style = SKPaintStyle.Stroke;
                    rectPaint.StrokeWidth = 1.5f;
                    //rectPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 3 }, 0);
                    canvas.DrawRect(heartbeatBigRect, rectPaint);
                }
                rectPaint.StrokeWidth = 1.5f;
                rectPaint.Color = new SKColor(74, 122, 196, 100); // 半透明浅蓝色
                rectPaint.Style = SKPaintStyle.Fill;
                rectPaint.PathEffect = null;

                foreach (SKRect rect in drawnDelectRects)
                {
                    canvas.DrawRect(rect, rectPaint);
                }

                // 绘制临时矩形（pictureBox1选中状态）
                if (_toolButtons[picMeasure].PressedDown)
                {
                    // 绘制正在绘制过程中的临时矩形
                    if (isDrawingTempRect)
                    {
                        float left = Math.Min(tempRectStartPoint.X, tempRectEndPoint.X);
                        float top = Math.Min(tempRectStartPoint.Y, tempRectEndPoint.Y);
                        float right = Math.Max(tempRectStartPoint.X, tempRectEndPoint.X);
                        float bottom = Math.Max(tempRectStartPoint.Y, tempRectEndPoint.Y);
                        SKRect currentTempRect = new SKRect(left, top, right, bottom);

                        using (SKPaint tempRectPaint = new SKPaint
                        {
                            Color = new SKColor(0, 0, 255, 128),
                            StrokeWidth = 2,
                            IsAntialias = true,
                            Style = SKPaintStyle.Stroke
                        })
                        using (SKPaint textPaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            IsAntialias = true,
                        })
                        using (SKFont textFont = new SKFont
                        {
                            Size = 10.5f,
                            Typeface = SKTypeface.FromFamilyName("微软雅黑")
                        })
                        {
                            string logStr = $"currentTempRect.Width: {currentTempRect.Width}";
                            //Logger.CreateLog(logStr, "通用心电图");
                            canvas.DrawRect(currentTempRect, tempRectPaint);
                            int RRMs = Convert.ToInt32(currentTempRect.Width / speed / pixMM * 1000);
                            if (RRMs != 0)
                            {
                                int HR = 60 * 1000 / RRMs;
                                float gainMv = currentTempRect.Height / pixMM / gain;
                                string RRMsText = $"{RRMs} ms";
                                string HRText = $"{HR} bpm";
                                string gainMvText = $"{gainMv:F3} mV";
                                float RRMsY = currentTempRect.Bottom + (3 * pixMM) > height - (2 * pixMM) ? currentTempRect.Bottom - (2 * pixMM) : currentTempRect.Bottom + (3 * pixMM);
                                float gainMvX = currentTempRect.Left - pixMM < LeftMargin + (5 * pixMM) ? currentTempRect.Left + (15 * pixMM) : currentTempRect.Left - pixMM;
                                float HRY = currentTempRect.Top - pixMM < TopMargin + (3 * pixMM) ? currentTempRect.Top + (3 * pixMM) : currentTempRect.Top - pixMM;
                                canvas.DrawText(RRMsText, currentTempRect.Left + (currentTempRect.Width / 2), RRMsY, SKTextAlign.Center, textFont, textPaint);
                                canvas.DrawText(gainMvText, gainMvX, currentTempRect.Top + (currentTempRect.Height / 2), SKTextAlign.Right, textFont, textPaint);
                                canvas.DrawText(HRText, currentTempRect.Left + (currentTempRect.Width / 2), HRY, SKTextAlign.Center, textFont, textPaint);
                            }
                        }
                    }
                    // 绘制已完成的临时矩形
                    else if (!tempRect.IsEmpty)
                    {
                        using (SKPaint tempRectPaint = new SKPaint
                        {
                            Color = new SKColor(0, 0, 255, 128),
                            StrokeWidth = 2,
                            IsAntialias = true,
                            Style = SKPaintStyle.Stroke
                        })
                        using (SKPaint textPaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            IsAntialias = true,
                        })
                        using (SKFont textFont = new SKFont
                        {
                            Size = 10.5f,
                            Typeface = SKTypeface.FromFamilyName("微软雅黑")
                        })
                        {
                            canvas.DrawRect(tempRect, tempRectPaint);

                            int RRMs = Convert.ToInt32(tempRect.Width / speed / pixMM * 1000);
                            if (RRMs != 0)
                            {
                                int HR = 60 * 1000 / RRMs;
                                float gainMv = tempRect.Height / pixMM / gain;
                                string RRMsText = $"{RRMs} ms";
                                string HRText = $"{HR} bpm";
                                string gainMvText = $"{gainMv:F3} mV";
                                float RRMsY = tempRect.Bottom + (3 * pixMM) > height - (2 * pixMM) ? tempRect.Bottom - (2 * pixMM) : tempRect.Bottom + (3 * pixMM);
                                float gainMvX = tempRect.Left - pixMM < LeftMargin + (5 * pixMM) ? tempRect.Left + (15 * pixMM) : tempRect.Left - pixMM;
                                float HRY = tempRect.Top - pixMM < TopMargin + (3 * pixMM) ? tempRect.Top + (3 * pixMM) : tempRect.Top - pixMM;

                                canvas.DrawText(RRMsText, tempRect.Left + (tempRect.Width / 2), RRMsY, SKTextAlign.Center, textFont, textPaint);
                                canvas.DrawText(gainMvText, gainMvX, tempRect.Top + (tempRect.Height / 2), SKTextAlign.Right, textFont, textPaint);
                                canvas.DrawText(HRText, tempRect.Left + (tempRect.Width / 2), HRY, SKTextAlign.Center, textFont, textPaint);

                            }
                        }
                    }
                }

                if (!selectedRect.IsEmpty)
                {
                    canvas.DrawRect(selectedRect, rectPaint);
                }
            }

            if (clickPoint.X != 0)
            {
                using (SKPaint linePaint = new SKPaint())
                {
                    linePaint.Color = new SKColor(0, 0, 255, 128); // 半透明浅蓝色
                    linePaint.Style = SKPaintStyle.Stroke;
                    linePaint.StrokeWidth = 1.5f;
                    canvas.DrawLine(clickPoint.X, TopMargin, clickPoint.X, height, linePaint);

                }
            }
        }

        // 滚动条防抖动计时器事件处理
        private void ScrollBarScroll()
        {
            // 更新时间偏移
            waveformTimeOffsetSeconds = pendingScrollValue;
            if ((waveformTimeOffsetSeconds + rowSeconds) * _holterConfig.SampleRate > _holterConfig.Total)
            {
                waveformTimeOffsetSeconds = (float)_holterConfig.Total / _holterConfig.SampleRate - rowSeconds;
            }
            waveformTimeOffsetSeconds = Math.Max(0, waveformTimeOffsetSeconds);

            hScrollBar1.Value = Convert.ToInt32(waveformTimeOffsetSeconds);
            int middleSample = Convert.ToInt32(waveformTimeOffsetSeconds * _holterConfig.SampleRate + rowPoints / 2);
            OnEcgCommonWaveStartTimeChanged?.Invoke(rowSeconds, waveformTimeOffsetSeconds, middleSample);
            SelectSample = 0;
            clickPoint.X = 0;
            bMiddle = true;

            forceWaveformRedraw();

            selectedRect = SKRect.Empty;
            _lastScrollTime = DateTime.Now;
        }

        /// <summary>
        /// 检测点是否在矩形内
        /// </summary>
        private bool IsPointInRect(float x, float y, SKRect rect)
        {
            return x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom;
        }

        /// <summary>
        /// 检测点是否在矩形右下角区域（可调整大小的区域）
        /// </summary>
        private bool IsPointInBottomRightCorner(float x, float y, SKRect rect, float cornerSize = 10)
        {
            // 检查是否在右下角区域
            float cornerRight = rect.Right;
            float cornerLeft = rect.Right - cornerSize;
            float cornerBottom = rect.Bottom;
            float cornerTop = rect.Bottom - cornerSize;

            return x >= cornerLeft && x <= cornerRight && y >= cornerTop && y <= cornerBottom;
        }

        /// <summary>
        /// 限制矩形在波形区域内（保持尺寸不变）
        /// </summary>
        private SKRect ClampRectToWaveformArea(SKRect rect)
        {
            // 保存原始尺寸
            float originalWidth = rect.Width;
            float originalHeight = rect.Height;

            // 计算矩形可以移动的最大范围
            float maxLeft = width - originalWidth;
            float maxTop = height - originalHeight;

            // 限制位置但不改变尺寸
            float left = Math.Max(LeftMargin, Math.Min(rect.Left, maxLeft));
            float top = Math.Max(TopMargin, Math.Min(rect.Top, maxTop));

            // 根据原始尺寸计算右边界和下边界
            float right = left + originalWidth;
            float bottom = top + originalHeight;

            return new SKRect(left, top, right, bottom);
        }

        public int FindNearestHeartbeatIndex(int clickedDataIndex)
        {
            // 第一步：查找精确匹配的元素
            int exactIndex = -1;
            for (int i = 0; i < pageHeartbeatInfos.Count; i++)
            {
                if (pageHeartbeatInfos[i].StartIndex <= clickedDataIndex && pageHeartbeatInfos[i].EndIndex > clickedDataIndex)
                {
                    exactIndex = i;
                    break;
                }
            }
            if (exactIndex == -1)
            {
                for (int i = 0; i < pageHeartbeatInfos.Count; i++)
                {
                    if (clickedDataIndex <= pageHeartbeatInfos[i].Sample)
                    {
                        exactIndex = i;
                        break;
                    }
                }
            }

            return exactIndex; // 找到精确匹配，直接返回
        }

        /// <summary>
        /// 波形起始时间改变事件处理
        /// </summary>
        public void OnPageScanStartTimeChanged(float timeOffsetSeconds)
        {
            // 设置波形起始时间
            waveformTimeOffsetSeconds = timeOffsetSeconds - rowSeconds / 2 > 0 ? timeOffsetSeconds - rowSeconds / 2 : 0;

            // 更新滚动条位置
            if (hScrollBar1 != null)
            {
                hScrollBar1.Value = Math.Max(0, Math.Min(Convert.ToInt32(waveformTimeOffsetSeconds), hScrollBar1.Maximum - hScrollBar1.LargeChange));
            }
            selectedRect = SKRect.Empty;
            clickPoint.X = 0;
            //强制重绘波形
            forceWaveformRedraw();
        }

        public void OnPageScanLeadChanged(List<int> leadIdx)
        {
            this.leadIdxs = leadIdx;
            //强制重绘波形
            forceWaveformRedraw();
        }

        public void OnTemplateLeadChanged(List<int> leadIdx)
        {
            this.leadIdxs = leadIdx;
            //强制重绘波形
            forceWaveformRedraw();
        }

        /// <summary>
        /// 波形起始时间改变事件处理
        /// </summary>
        public void OnSmallWaveStartTimeChanged(float timeOffsetSeconds)
        {
            // 设置波形起始时间
            waveformTimeOffsetSeconds = timeOffsetSeconds;

            // 更新滚动条位置
            if (hScrollBar1 != null)
            {
                hScrollBar1.Value = Math.Max(0, Math.Min(Convert.ToInt32(waveformTimeOffsetSeconds), hScrollBar1.Maximum - hScrollBar1.LargeChange));
            }

            selectedRect = SKRect.Empty;

            // 强制重绘波形
            forceWaveformRedraw();
        }

        public void OnWaveformStartTimeChanged(int selectSample)
        {
            rowSeconds = (float)(width - LeftMargin) / pixMM / speed;

            float startSecond = (float)selectSample * 1.0f / AnaValues.holterConfig.SampleRate - rowSeconds / 2;
            if (startSecond > 0)
            {
                SelectSample = selectSample;
                bMiddle = true;
            }
            else
            {
                clickSample = selectSample;
                bMiddle = false;
            }
            // 设置波形起始时间
            waveformTimeOffsetSeconds = startSecond > 0 ? startSecond : 0;

            // 更新滚动条位置
            if (hScrollBar1 != null)
            {
                hScrollBar1.Value = Math.Max(0, Math.Min(Convert.ToInt32(waveformTimeOffsetSeconds), hScrollBar1.Maximum - hScrollBar1.LargeChange));
            }

            selectedRect = SKRect.Empty;
            clickPoint.X = 0;

            // 强制重绘波形
            forceWaveformRedraw();
        }

        //public void OnKeyDown(object sender, KeyEventArgs e)
        //{
        //    Console.WriteLine("----keyCode" + e.KeyCode);
        //}
        private void selectPreviousBeat()
        {
            BeatInfo lastSelectBeat = _selectedBeats[_selectedBeats.Count - 1];
            int previousIdx = 0;
            if (AnaValues.CurrentBeatMap.ContainsKey(lastSelectBeat.Sample))
            {
                previousIdx = Math.Max(0, AnaValues.CurrentBeatMap[lastSelectBeat.Sample] - 1);
            }
            else
            {
                //previousIdx = Math.Max(0, LastBeatMap[lastSelectBeat.Sample] - 2);
                previousIdx = AnaCode.FindPreviousBeatIndex(lastSelectBeat.Sample);
            }

            BeatInfo preivousBeat = AnaValues.CachedAllBeats[previousIdx];
            if (preivousBeat.Sample <= pageHeartbeatInfos[0].Sample)
            {
                waveformTimeOffsetSeconds -= 2;
                if ((waveformTimeOffsetSeconds + rowSeconds) * _holterConfig.SampleRate > _holterConfig.Total)
                {
                    waveformTimeOffsetSeconds = (float)_holterConfig.Total / _holterConfig.SampleRate - rowSeconds;
                }
                waveformTimeOffsetSeconds = Math.Max(0, waveformTimeOffsetSeconds);

                // 更新滚动条位置
                hScrollBar1.Value = Convert.ToInt32(waveformTimeOffsetSeconds);
                updateEcgData();
            }

            // 如果找到心搏波形，添加覆盖该区域的矩形
            float left = sampleToAxisX(Convert.ToInt32(preivousBeat.Sample - 3.5 / speed * AnaValues.holterConfig.SampleRate));
            float top = TopMargin;
            float right = sampleToAxisX(Convert.ToInt32(preivousBeat.Sample + 3.5 / speed * AnaValues.holterConfig.SampleRate));
            float bottom = _graphicsControl.Height;
            selectedRect = new SKRect(left, top, right, bottom);

            _selectedBeats?.Clear();
            _selectedBeats.Add(preivousBeat);
            clickPoint.X = 0;
        }

        private void selectNextBeat()
        {
            BeatInfo lastSelectBeat = _selectedBeats[_selectedBeats.Count - 1];
            int nextIdx = 0;
            if (AnaValues.CurrentBeatMap.ContainsKey(lastSelectBeat.Sample))
            {
                nextIdx = Math.Min(AnaValues.CachedAllBeats.Count - 1, AnaValues.CurrentBeatMap[lastSelectBeat.Sample] + 1);
            }
            else
            {
                //nextIdx = Math.Min(AnaValues.CachedAllBeats.Count - 1, LastBeatMap[lastSelectBeat.Sample]);
                nextIdx = AnaCode.FindLatterBeatIndex(lastSelectBeat.Sample);
            }

            BeatInfo nextBeat = AnaValues.CachedAllBeats[nextIdx];
            if (nextBeat.Sample >= pageHeartbeatInfos[pageHeartbeatInfos.Count - 1].Sample)
            {
                waveformTimeOffsetSeconds += 2;
                if ((waveformTimeOffsetSeconds + rowSeconds) * _holterConfig.SampleRate > _holterConfig.Total)
                {
                    waveformTimeOffsetSeconds = (float)_holterConfig.Total / _holterConfig.SampleRate - rowSeconds;
                }
                waveformTimeOffsetSeconds = Math.Max(0, waveformTimeOffsetSeconds);

                // 更新滚动条位置
                hScrollBar1.Value = Convert.ToInt32(waveformTimeOffsetSeconds);
                updateEcgData();
            }

            // 如果找到心搏波形，添加覆盖该区域的矩形
            float left = sampleToAxisX(Convert.ToInt32(nextBeat.Sample - 3.5 / speed * AnaValues.holterConfig.SampleRate));
            float top = TopMargin;
            float right = sampleToAxisX(Convert.ToInt32(nextBeat.Sample + 3.5 / speed * AnaValues.holterConfig.SampleRate));
            float bottom = _graphicsControl.Height;
            selectedRect = new SKRect(left, top, right, bottom);

            rectStartPoint.X = sampleToAxisX(nextBeat.Sample);

            rectEndPoint.X = sampleToAxisX(nextBeat.Sample);
            _selectedBeats?.Clear();
            _selectedBeats.Add(nextBeat);
            clickPoint.X = 0;
        }

        private void setDefaultSelect()
        {
            if (bModifyBeat && _selectedBeats.Count > 0)
            {
                if (bNext)
                {
                    selectNextBeat();
                }
                else
                {
                    selectPreviousBeat();
                    bNext = true;
                }
                //bModifyBeat = false;
            }
            else
            {
                if (selectedRect.IsEmpty && clickPoint.X == 0)
                {
                    int middleSample = SelectSample > 0 ? SelectSample : Convert.ToInt32(waveformTimeOffsetSeconds * _holterConfig.SampleRate + rowPoints / 2);
                    if (!_toolButtons[picCutStart].PressedDown && !_toolButtons[picAnaStart].PressedDown)
                        selectStartSample = selectEndSample = middleSample;

                    if (pageHeartbeatInfos.Count > 0)
                    {
                        float x1 = 0;
                        float x2 = 0;
                        int heartbeatIdx = -1;
                        if (bMiddle)
                        {
                            heartbeatIdx = FindNearestHeartbeatIndex(middleSample);
                        }
                        else
                        {
                            heartbeatIdx = FindNearestHeartbeatIndex(clickSample);
                        }

                        if (heartbeatIdx >= 0)
                        {
                            x1 = (float)(LeftMargin + ((pageHeartbeatInfos[heartbeatIdx].Sample - waveformTimeOffsetSeconds * _holterConfig.SampleRate) * (speed * pixMM / AnaValues.holterConfig.SampleRate)) - 3.5 * pixMM);
                            x2 = (float)(LeftMargin + ((pageHeartbeatInfos[heartbeatIdx].Sample - waveformTimeOffsetSeconds * _holterConfig.SampleRate) * (speed * pixMM / AnaValues.holterConfig.SampleRate)) + 3.5 * pixMM);

                            // 计算心搏矩形的Y坐标和高度
                            float rowHeight = (float)height;
                            float y1 = TopMargin;
                            float y2 = y1 + rowHeight;

                            // 创建心搏矩形
                            SKRect heartbeatRect = new SKRect(x1, y1, x2, y2);
                            selectedRect = heartbeatRect;
                            //drawnRects.Add(heartbeatRect);


                            _selectedBeats?.Clear();
                            List<BeatInfo> selectBeats = new List<BeatInfo>();

                            int sampleIdx = AnaValues.CachedAllBeats.FindIndex(o => o.Sample == pageHeartbeatInfos[heartbeatIdx].Sample);
                            selectBeats.Add(AnaValues.CachedAllBeats[sampleIdx]);

                            _selectedBeats = selectBeats;
                        }
                    }
                }
                //else
                //{
                //    if (rectStartPoint.X == rectEndPoint.X && rectStartPoint.X > 0)
                //    {
                //        BeatInfo clickHeartbeat = BeatInfo.Empty;
                //        int clickSample = xAxisToSample(rectStartPoint.X);
                //        foreach (var heartbeat in pageHeartbeatInfos)
                //        {
                //            if (heartbeat.StartIndex <= clickSample && heartbeat.EndIndex >= clickSample)
                //            {
                //                clickHeartbeat = heartbeat;
                //                break;
                //            }
                //        }

                //        // 尝试检测点击位置的心搏波形
                //        if (Math.Abs(clickSample - clickHeartbeat.Sample) < 3.5 / speed * AnaValues.holterConfig.SampleRate)
                //        {
                //            // 如果找到心搏波形，添加覆盖该区域的矩形
                //            float left = sampleToAxisX(Convert.ToInt32(clickHeartbeat.Sample - 3.5 / speed * AnaValues.holterConfig.SampleRate));
                //            float top = TopMargin;
                //            float right = sampleToAxisX(Convert.ToInt32(clickHeartbeat.Sample + 3.5 / speed * AnaValues.holterConfig.SampleRate));
                //            float bottom = _graphicsControl.Height;

                //            _selectedBeats?.Clear();
                //            selectedRect = new SKRect(left, top, right, bottom);
                //            _selectedBeats.Add(clickHeartbeat);
                //            clickPoint.X = 0;
                //        }
                //        //else
                //        //{
                //        //    selectedRect = SKRect.Empty;
                //        //    isClickLine = true;
                //        //    clickPoint.X = rectStartPoint.X;
                //        //}
                //    }
                //}
            }
            bModifyBeat = false;
        }

        private void picMeasure_Click(object sender, EventArgs e)
        {
            ToolButton btn = _toolButtons[picMeasure];
            if (btn.PressedDown)
            {
                selectedRect = SKRect.Empty;
                clickPoint.X = 0;
            }
            else { tempRect = SKRect.Empty; }

            forceWaveformRedraw();
        }

        private void picCaliper_Click(object sender, EventArgs e)
        {
            ToolButton btn = _toolButtons[picCaliper];
            if (btn.PressedDown)
            {
                selectedRect = SKRect.Empty;

                GenerateVerticalLines();
            }
            else { verticalLines.Clear(); }

            forceWaveformRedraw();
        }

        private void picPrint_Click(object sender, EventArgs e)
        {
            int startSample = 0;
            if (rectStartPoint.X == rectEndPoint.X)
            {
                startSample = Convert.ToInt32((waveformTimeOffsetSeconds + rowSeconds / 2.0f - 3.5f) * AnaValues.holterConfig.SampleRate);
            }
            else
            {
                startSample = xAxisToSample(selectedRect.Left);
            }
            List<int> startSamples = new List<int>();
            startSamples.Add(startSample);

            using (PrintSettingForm printSetting = new PrintSettingForm(0, startSamples, CommonValues.Langs["打印"]))
            {
                if (rectStartPoint.X == rectEndPoint.X)
                {
                    printSetting.bSelectSingleHeartbeat = true;
                }
                else
                {
                    printSetting.bSelectSingleHeartbeat = false;

                    DateTime startTime = AnaCode.getDataTimeFromSample(xAxisToSample(selectedRect.Left));
                    string startTimeStr = startTime.ToString("HH:mm:ss");

                    DateTime endTime = AnaCode.getDataTimeFromSample(xAxisToSample(selectedRect.Right));
                    string endTimeStr = endTime.ToString("HH:mm:ss");

                    printSetting.printLen = CommonValues.Langs["打印当前选定的区域"] + $":[{startTimeStr}-{endTimeStr}]";
                    printSetting.printEndSample = xAxisToSample(selectedRect.Right);
                }

                Form parentForm = this.FindForm();
                printSetting.ShowDialog(parentForm);
            }
        }

        private void picSaveSegment_Click(object sender, EventArgs e)
        {
            //当没有选中心搏时，保存打印框内的波形
            if (_selectedBeats.Count == 0)
            {
                int middleSample = Convert.ToInt32((waveformTimeOffsetSeconds + rowSeconds / 2.0f) * AnaValues.holterConfig.SampleRate);
                _selectedBeats = pageHeartbeatInfos.Where(h => h.EndIndex >= middleSample && h.StartIndex < middleSample)
                                .OrderBy(h => h.StartIndex)
                                .ToList();
            }


            List<int> startSamples = new List<int>(_selectedBeats.Count);

            for (int i = 0; i < _selectedBeats.Count; i++)
            {
                int startSample = 0;

                if (_selectedBeats.Count == 1)
                {
                    startSample = _selectedBeats[i].Sample - Convert.ToInt32(3.5f * AnaValues.holterConfig.SampleRate);
                    startSample = startSample > 0 ? startSample : pageHeartbeatInfos[0].StartIndex;
                }
                else
                {
                    if (i == 0)
                    {
                        startSample = _selectedBeats[i].Sample - Convert.ToInt32(0.5f * AnaValues.holterConfig.SampleRate);
                        startSample = startSample > 0 ? startSample : pageHeartbeatInfos[0].StartIndex;
                    }
                    else
                    {
                        startSample = _selectedBeats[i].StartIndex;
                    }
                }

                startSamples.Add(startSample);
            }
            using (PrintSettingForm printSetting = new PrintSettingForm(1, startSamples, CommonValues.Langs["保存片段"]))
            {
                printSetting.bSelectSingleHeartbeat = true;
                printSetting.ShowDialog();
            }
        }

        private void picGain_Click(object sender, EventArgs e)
        {
            ToolButton btn = _toolButtons[picGain];
            if (btn.PressedDown)
                ShowGainMenu();
            else
                HideGainMenu();
            //// 切换pictureBox5的背景色状态
            //isPictureBox5Highlighted = !isPictureBox5Highlighted;

            //// 根据状态设置背景色和显示/隐藏菜单
            //if (isPictureBox5Highlighted)
            //{
            //    // 背景色加深（使用较深的颜色）
            //    pictureBox5.BackColor = Color.FromArgb(159, 200, 255); // 深蓝色

            //    // 显示增益设置菜单（在pictureBox5上方）
            //    ShowGainMenu();
            //}
            //else
            //{
            //    // 恢复原始背景色
            //    pictureBox5.BackColor = SystemColors.Control; // 默认控件背景色

            //    // 隐藏增益设置菜单
            //    HideGainMenu();
            //}

            //// 强制重绘以显示效果
            //pictureBox5.Invalidate();
        }

        private void picSpeed_Click(object sender, EventArgs e)
        {
            ToolButton btn = _toolButtons[picSpeed];
            if (btn.PressedDown)
                ShowSpeedMenu();
            else
                HideSpeedMenu();
        }

        private void picEnlarge_Click(object sender, EventArgs e)
        {
            //// 如果pictureBox5处于选中状态，先取消其选中
            //if (isPictureBox5Highlighted)
            //{
            //    isPictureBox5Highlighted = false;
            //    pictureBox5.BackColor = SystemColors.Control;
            //    pictureBox5.Invalidate();
            //    HideGainMenu();
            //}

            //// 如果pictureBox6处于选中状态，先取消其选中
            //if (isPictureBox6Highlighted)
            //{
            //    isPictureBox6Highlighted = false;
            //    pictureBox6.BackColor = SystemColors.Control;
            //    pictureBox6.Invalidate();
            //    HideSpeedMenu();
            //}

            //// 切换pictureBox7的背景色状态
            //isPictureBox7Highlighted = !isPictureBox7Highlighted;

            //// 根据状态设置背景色
            //if (isPictureBox7Highlighted)
            //{
            //    // 背景色加深（使用较深的颜色）
            //    pictureBox7.BackColor = Color.FromArgb(159, 200, 255); // 深蓝色
            //}
            //else
            //{
            //    // 恢复原始背景色
            //    pictureBox7.BackColor = SystemColors.Control; // 默认控件背景色
            //}

            //// 强制重绘以显示效果
            //pictureBox7.Invalidate();
        }

        private void updateToolButtonState(ToolButton clickedItem)
        {
            if (clickedItem.Disabled)
            {
                clickedItem.PressedDown = false;
                return;
            }

            PictureBox picBox = clickedItem.PicControl;
            if (clickedItem.KeepState)
            {
                clickedItem.PressedDown = !clickedItem.PressedDown;
                if (clickedItem.PressedDown)
                {
                    foreach (ToolButton btn in _toolButtons.Values)
                    {
                        if (btn.KeepState && btn != clickedItem) btn.PressedDown = false;
                    }
                }
                else
                {
                    picBox.BackColor = SystemColors.Control;
                }
            }
            else
            {
                //处理成对的操作按钮
                if (picBox.Name == "picCutStart")
                {
                    _toolButtons[picCutEnd].Disabled = false;
                    foreach (ToolButton btn in _toolButtons.Values)
                    {
                        if (btn.IsPairButton && btn.PicControl != picCutEnd) btn.Disabled = true;
                    }
                }
                else if (picBox.Name == "picCutEnd")
                {
                    if (Math.Abs(selectStartSample - selectEndSample) < 4 * _holterConfig.SampleRate)
                    {
                        ScreenCenterTip.Show(CommonValues.Langs["剪切时长过短请重新选择终点位置"]);
                        return;
                    }
                    _toolButtons[picCutStart].Disabled = false;
                    _toolButtons[picCutEnd].Disabled = true;
                    _toolButtons[picAnaStart].Disabled = false;
                    _toolButtons[picAnaEnd].Disabled = true;
                }
                else if (picBox.Name == "picAnaStart")
                {
                    _toolButtons[picAnaEnd].Disabled = false;
                    foreach (ToolButton btn in _toolButtons.Values)
                    {
                        if (btn.IsPairButton && btn.PicControl != picAnaEnd) btn.Disabled = true;
                    }
                }
                else if (picBox.Name == "picAnaEnd")
                {
                    if (Math.Abs(selectStartSample - selectEndSample) < 4 * _holterConfig.SampleRate)
                    {
                        ScreenCenterTip.Show(CommonValues.Langs["分析时长过短请重新选择终点位置"]);
                        return;
                    }
                    _toolButtons[picCutStart].Disabled = false;
                    foreach (ToolButton btn in _toolButtons.Values)
                    {
                        if (btn.IsPairButton && btn.PicControl != picCutStart) btn.Disabled = true;
                    }
                }

                clickedItem.PressedDown = true;
                foreach (ToolButton btn in _toolButtons.Values)
                {
                    if (btn != clickedItem) btn.PressedDown = false;
                }
            }
        }

        private void refreshToolButtons()
        {
            List<ToolButton> buttons = _toolButtons.Values.ToList();
            bool pressedOne = buttons.Any(o => o.PressedDown);
            foreach (ToolButton btn in _toolButtons.Values)
            {
                PictureBox picBox = btn.PicControl;

                if (btn.KeepState)
                {
                    if (pressedOne)
                    {
                        if (btn.PressedDown)
                        {
                            picBox.BackColor = Color.FromArgb(159, 200, 255); //加深背景颜色
                            picBox.Cursor = Cursors.Hand;
                        }
                        else
                        {
                            picBox.BackColor = SystemColors.Control;
                            picBox.Cursor = Cursors.Arrow;
                        }
                    }
                    else
                    {
                        picBox.BackColor = Color.FromArgb(230, 236, 248);
                    }
                }
                else if (btn.IsPairButton)
                {
                    if (!btn.Disabled)
                    {
                        picBox.BackColor = Color.FromArgb(230, 236, 248);
                        picBox.Image = btn.OriginalImage;
                        picBox.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        picBox.BackColor = Color.Silver;
                        picBox.Image = btn.GrayScaleImage;
                        picBox.Cursor = Cursors.Arrow;
                    }
                }
            }
        }

        private SegmentRange getSelectedRange(bool checkSingleBeat = false)
        {
            int selStart = 0, selEnd = 0;
            if (startSelectSample > 0 && endSelectSample > 0)
            {
                selStart = startSelectSample;
                selEnd = endSelectSample;
            }
            else if (selectedRect != SKRect.Empty)
            {
                if (rectStartPoint.X == rectEndPoint.X) //点击选中了心搏
                {
                    if (checkSingleBeat)
                    {
                        selStart = xAxisToSample(selectedRect.Left);
                        selEnd = xAxisToSample(selectedRect.Right);
                    }
                }
                else
                {
                    selStart = xAxisToSample(selectedRect.Left);
                    selEnd = xAxisToSample(selectedRect.Right);
                }
            }

            return new SegmentRange(selStart, selEnd);
        }

        private List<BeatInfo> getSelectedBeats()
        {
            List<BeatInfo> lstBeats = new List<BeatInfo>();
            SegmentRange selRange = getSelectedRange(true);

            foreach (BeatInfo heartbeat in AnaValues.CachedAllBeats)
            {
                if (heartbeat.Sample >= selRange.StartIndex && heartbeat.Sample < selRange.EndIndex)
                {
                    lstBeats.Add(heartbeat);
                }
            }
            return lstBeats;
        }

        private bool isValidSelectionRange()
        {
            List<SegmentRange> sections = _cutRanges;
            if (_holterConfig.StartReanalysis) sections = _unanalyzableRanges;
            //检查单击选中的心搏是否在非分析区间内
            if (_selectedBeats.Count > 0)
            {
                for (int i = 0; i < _selectedBeats.Count; i++)
                {
                    if (sections.Any(r => r.StartIndex <= _selectedBeats[i].Sample && _selectedBeats[i].Sample < r.EndIndex))
                    {
                        return false;
                    }
                }
            }
            //检查拖选的范围是否与非分析区间有交集
            if (selectedRect != SKRect.Empty)
            {
                if (sections.Exists(r => selectedRect.Left < sampleToAxisX(r.EndIndex) && selectedRect.Right > sampleToAxisX(r.StartIndex)))
                {
                    return false;
                }
            }

            return true;
        }

        //将横坐标转换为采样点序号
        private int xAxisToSample(float x)
        {
            float clickedXInRow = x - LeftMargin;
            int startIndex = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            return startIndex + Convert.ToInt32(clickedXInRow / pixMM / speed * AnaValues.holterConfig.SampleRate);
        }

        //将采样点序号转换为横坐标
        private float sampleToAxisX(int sample)
        {
            int startIndex = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            float left = (float)(sample - startIndex) / AnaValues.holterConfig.SampleRate * speed * pixMM;
            left = Math.Max(left, 0);
            return LeftMargin + left;
        }


        private class ToolButton
        {
            internal PictureBox PicControl { get; set; }
            internal Image OriginalImage { get; set; }
            internal Image GrayScaleImage { get; set; }

            internal bool IsCommonButton { get; set; } //通用(各界面均显示)
            internal bool ForReanalyze { get; set; }
            internal bool KeepState { get; set; } //是否可保持(点击)状态
            internal bool PressedDown { get; set; } //是否已点击按下

            internal bool IsPairButton { get; set; }
            internal bool Disabled { get; set; }
        }
    }
}