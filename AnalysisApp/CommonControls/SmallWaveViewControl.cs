using AnalysisApp.Commons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class SmallWaveViewControl : BaseDrawingControl
    {
        // 波形时间偏移
        public float waveformTimeOffsetSeconds = 0; // 波形起始时间偏移量（秒）

        // 导联显示设置
        private int currentLeadCount = 1;                       // 当前显示的导联数量
        private List<int> leadIdxs = new List<int>();

        // ECG显示参数常量
        private readonly int currentRows = 1;                   // 每导联显示1行ECG波形
        private int SamplingRate = 250;                         // 从Holter配置获取采样率，默认250Hz
        private int rowPoints = 0;                              // 每行数据点
        public float rowSeconds = 0;                            // 每行的秒数
        private float speed = 6.25f;                            //纸速 6.25mm/s
        private float gain = 5.0f;                              //增益 5mm/mV
        private const float pixMM = 3.777f;                     //每毫米3.777个像素点
        private readonly bool bShowGrid = true;
        private int LeftMargin { get; set; } = 12;
        private int TopMargin { get; set; } = 0;

        public int width = 0;          //绘图宽度
        private int height = 0;          //绘图高度度

        public float scale = 0.5f;          //放大倍数
        private bool bDrawSmallGrid = false;

        // ECG数据读取相关变量
        public List<WaveInfo> ecgData = new List<WaveInfo>(); // 每个子列表代表一个导联的数据
        // 存储当前页心搏信息的列表
        private List<BeatInfo> pageHeartbeatInfos = new List<BeatInfo>();

        private DrawEcgWave drawEcgWave = null;

        // 矩形绘制相关变量
        private SKRect bigRect = SKRect.Empty;

        // 拖动相关变量
        private bool isDraggingBigRect = false; // 是否正在拖动bigRect
        private SKPoint dragStartPoint; // 拖动起始点
        private float bigRectStartX; // bigRect拖动起始X坐标

        /// <summary>
        /// 选中心搏起始时间设置事件
        /// </summary>
        public event Action<float> OnSmallWaveSelectStartTimeChanged;

        public SmallWaveViewControl()
        {
            InitializeComponent();
        }

        private void SmallWaveViewControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                initControl();
            }
        }

        private void initControl()
        {
            width = panel1.Width;
            height = panel1.Height;

            SamplingRate = AnaValues.holterConfig.SampleRate;
            leadIdxs.Add(AnaValues.holterConfig.MainChannel);

            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            panel1.Controls.Add(_graphicsControl);

            drawEcgWave = new DrawEcgWave();

        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            // 清除画布
            canvas.Clear(SKColors.Transparent);

            // 确保离屏缓冲区已更新
            width = imageWidth;
            height = imageHeight;

            InitEcgData();

            drawEcgWave.WaveformTimeOffsetSeconds = waveformTimeOffsetSeconds;
            drawEcgWave.speed = speed;
            drawEcgWave.gain = gain;
            drawEcgWave.LeftMargin = LeftMargin;
            drawEcgWave.TopMargin = TopMargin;
            drawEcgWave.CurrentLeadCount = currentLeadCount;
            drawEcgWave.CurrentRows = currentRows;
            drawEcgWave.RowPoints = rowPoints;
            drawEcgWave.EcgData = ecgData;
            drawEcgWave.pageHeartbeatInfos = pageHeartbeatInfos;
            drawEcgWave.leadIdxs = leadIdxs;

            drawEcgWave.ShowRrHrInfo = true;

            drawEcgWave.bShowGrid = bShowGrid;

            drawEcgWave.scale = scale;
            drawEcgWave.step = 1;
            drawEcgWave.bDrawSmallGrid = bDrawSmallGrid;

            try
            {
                // 绘制背景
                drawEcgWave.DrawBackground(canvas, width, height);
                // 绘制ECG波形
                drawEcgWave.DrawEcgWaveform(canvas, width, height);
                // 绘制导联名称
                drawEcgWave.DrawLeadNames(canvas, width, height, rowSeconds);

                // 绘制矩形
                DrawRectangles(canvas);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "绘制波形");
            }

        }

        #region 鼠标事件

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            // 只有在左键点击时才执行操作
            if (e.Button == MouseButtons.Left)
            {
                // 获取鼠标点击的坐标
                float mouseX = e.X;
                float mouseY = e.Y;

                // 确保bigRect有有效宽度
                if (!bigRect.IsEmpty && bigRect.Width > 0)
                {
                    // 检查鼠标是否在bigRect内
                    if (mouseX >= bigRect.Left && mouseX <= bigRect.Right && mouseY >= bigRect.Top && mouseY <= bigRect.Bottom)
                    {
                        // 开始拖动bigRect
                        isDraggingBigRect = true;
                        dragStartPoint = new SKPoint(mouseX, mouseY);
                        bigRectStartX = bigRect.Left;
                    }
                    else
                    {
                        // 鼠标不在bigRect内，设置bigRect中心点到点击位置
                        // 计算bigRect的宽度
                        float rectWidth = bigRect.Width;

                        // 计算新的Left坐标，使得中心点X坐标等于鼠标点击的位置
                        float newLeft = mouseX - (rectWidth / 2);

                        // 确保bigRect不会超出左边界
                        newLeft = Math.Max(0, newLeft);

                        // 确保bigRect不会超出右边界
                        float maxLeft = width - rectWidth;
                        newLeft = Math.Min(maxLeft, newLeft);

                        float selectStartSeconds = waveformTimeOffsetSeconds + Convert.ToInt32((newLeft - LeftMargin) / pixMM / speed / scale);
                        selectStartSeconds = Math.Max(0, selectStartSeconds);
                        OnSmallWaveSelectStartTimeChanged?.Invoke(selectStartSeconds);

                        // 创建新的bigRect，保持高度和Top不变
                        bigRect = new SKRect(newLeft, bigRect.Top, newLeft + rectWidth, bigRect.Bottom);

                        // 强制重绘波形
                        reRendWave();


                    }
                }
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            // 只有在左键按下且正在拖动bigRect时才执行操作
            if (e.Button == MouseButtons.Left && isDraggingBigRect)
            {
                // 获取当前鼠标位置
                float currentX = e.X;

                // 计算鼠标移动的距离
                float deltaX = currentX - dragStartPoint.X;

                // 计算bigRect的新位置
                float newLeft = bigRectStartX + deltaX;

                // 确保bigRect不会超出左边界
                newLeft = Math.Max(0, newLeft);

                // 确保bigRect不会超出右边界
                if (bigRect.Width > 0)
                {
                    float maxLeft = width - bigRect.Width;
                    newLeft = Math.Min(maxLeft, newLeft);

                    // 更新bigRect的位置
                    bigRect = new SKRect(newLeft, bigRect.Top, newLeft + bigRect.Width, bigRect.Bottom);

                    // 强制重绘波形
                    reRendWave();

                    float selectStartSeconds = waveformTimeOffsetSeconds + (newLeft - LeftMargin) / speed / scale / pixMM;
                    selectStartSeconds = Math.Max(0, selectStartSeconds);
                    OnSmallWaveSelectStartTimeChanged?.Invoke(selectStartSeconds);
                }
            }
        }
        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            // 停止拖动bigRect
            isDraggingBigRect = false;
        }

        #endregion


        private void InitEcgData()
        {
            if (drawEcgWave == null || width < 2)
            {
                return;
            }

            _ = DateTime.Now.Ticks;

            // 清空数据并重置标志
            ecgData.Clear();

            rowSeconds = (width - LeftMargin) / pixMM / speed / scale;
            rowPoints = Convert.ToInt32((width - LeftMargin) / pixMM / speed / scale * SamplingRate);

            ecgData = AnaCode.ReadAllWaveData(leadIdxs, Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate), Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate) + currentRows * rowPoints);

            ////获取并存储每个采样点的状态位
            //_ = AnaValues.HolterDataReader.GetSampleStatusBits();
            pageHeartbeatInfos.Clear();

            int startSample = Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate);
            int endSample = startSample + ecgData[leadIdxs[0]].DataLength;
            pageHeartbeatInfos = AnaValues.CachedAllBeats
            .Where(h => h.EndIndex > startSample && h.StartIndex < endSample)
            .OrderBy(h => h.StartIndex)
            .ToList();
        }

        private void DrawRectangles(SKCanvas canvas)
        {
            // 创建矩形画笔
            using (SKPaint rectPaint = new SKPaint())
            {
                rectPaint.IsAntialias = true;
                rectPaint.Color = new SKColor(255, 175, 113, 128); // 半透明浅蓝色
                rectPaint.Style = SKPaintStyle.Fill;

                canvas.DrawRect(bigRect, rectPaint);
            }
        }

        // 强制重绘波形
        public void reRendWave()
        {
            if(_graphicsControl != null)
            {
                _graphicsControl.Invalidate();
            }
        }

        public void changeRectPoints(float rectSeconds)
        {
            float rectWidth = rectSeconds * speed * pixMM * scale;
            bigRect = new SKRect(LeftMargin, TopMargin, LeftMargin + rectWidth, this.panel1.Height);
            reRendWave();
        }

        /// <summary>
        /// 波形起始时间改变事件处理
        /// </summary>
        public void OnWaveformStartTimeChanged(float rowSeconds, float timeOffsetSeconds, int sample)
        {
            // 设置波形起始时间
            waveformTimeOffsetSeconds = timeOffsetSeconds;

            float rectWidth = rowSeconds * speed * pixMM * scale;

            bigRect = new SKRect(LeftMargin, TopMargin, LeftMargin + rectWidth, height);

            // 强制重绘波形
            reRendWave();
        }

        public void OnWaveformSpeedChanged(float rectSeconds)
        {
            changeRectPoints(rectSeconds);
        }

        /// <summary>
        /// 波形起始时间改变事件处理
        /// </summary>
        public void OnEnlargeWaveformStartTimeChanged(float timeOffsetSeconds, float rowSeconds)
        {
            // 设置波形起始时间
            waveformTimeOffsetSeconds = timeOffsetSeconds;

            float rectWidth = rowSeconds * speed * pixMM * scale;

            bigRect = new SKRect(LeftMargin, TopMargin, LeftMargin + rectWidth, height);

            // 强制重绘波形
            reRendWave();
        }
    }
}
