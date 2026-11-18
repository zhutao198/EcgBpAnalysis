using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp.Views.Desktop;
using AnalysisApp.Commons;
using SkiaSharp;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class EnlargeWaveControl : BaseDrawingControl
    {
        // 波形时间偏移
        public float waveformTimeOffsetSeconds = 0; // 波形起始时间偏移量（秒）

        // 导联显示设置
        private int currentLeadCount = 1; // 当前显示的导联数量
        public List<int> leadIdxs = new List<int>();

        // ECG显示参数常量
        private readonly int currentRows = 1;                // 每导联显示1行ECG波形
        private int SamplingRate = 250; // 从Holter配置获取采样率，默认250Hz
        private int rowPoints = 0; // 每行数据点
        private float rowSeconds = 0; // 每行秒数

        private readonly int wheelSeconds = 2;   //滚动鼠标的秒数
        public float speed = 25;                       //纸速 25mm/s
        private float gain = 10f;                       //增益 2.5mm/mV
        private const float pixMM = 3.777f;             //每毫米3.777个像素点
        private readonly bool bShowGrid = true;
        private float LeftMargin { get; set; } = 50;
        private float TopMargin { get; set; } = 8 * pixMM;

        public int width = 0;          //绘图宽度
        private int height = 0;          //绘图高度度

        public float scale = 5;          //放大倍数

        // 鼠标滚动防抖动相关变量
        private int pendingScrollSeconds;
        private DateTime _lastWheelTime = DateTime.MinValue;


        // 滚动条防抖动相关变量
        private int pendingScrollValue;
        private DateTime _lastScrollTime = DateTime.MinValue;

        public List<WaveInfo> ecgData = new List<WaveInfo>(); // 每个子项代表一个导联的数据
        // 心搏数据缓存
        private List<BeatInfo> cachedAllBeats = null;
        // 存储当前页心搏信息的列表
        private List<BeatInfo> pageHeartbeatInfos = new List<BeatInfo>();

        private DrawEcgWave drawEcgWave = null;

        // 矩形绘制相关变量
        private bool isDrawingRect = false;
        private SKPoint rectStartPoint;
        private SKPoint rectEndPoint;
        private SKRect selectedRect;

        private readonly List<SKRect> drawnRects = new List<SKRect>(); // 存储已绘制的矩形
        private SKPoint clickPoint;

        // pictureBox1背景色状态
        private bool isPictureBox1Highlighted = false;

        // 临时矩形绘制相关变量（用于pictureBox1选中状态）
        private bool isDrawingTempRect = false;          // 是否正在绘制临时矩形
        private SKPoint tempRectStartPoint;              // 临时矩形起始点
        private SKPoint tempRectEndPoint;                // 临时矩形结束点
        private SKRect tempRect = SKRect.Empty;          // 临时矩形区域

        // 矩形拖动相关变量
        private bool isDraggingTempRect = false;         // 是否正在拖动临时矩形
        private bool isResizingTempRect = false;         // 是否正在调整临时矩形大小
        private SKPoint dragStartPoint;                  // 拖动起始点
        private SKPoint tempRectOffset;                  // 矩形偏移量
        private SKPoint tempRectOriginalSize;            // 矩形原始尺寸

        /// <summary>
        /// 波形起始时间设置事件
        /// </summary>
        public event Action<float, float> OnEnlargeWaveStartTimeChanged;

        public EnlargeWaveControl()
        {
            InitializeComponent();
        }

        private void EnlargeWaveControl_Load(object sender, EventArgs e)
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
            clickPoint = new SKPoint(0, 0);

            SamplingRate = AnaValues.holterConfig.SampleRate;

            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            _graphicsControl.MouseWheel += OnCanvasMouseWheel;

            panel1.Controls.Add(_graphicsControl);

            // 初始化ECG数据
            InitializeEcgData();

            int sample = xAxisToSample((width - LeftMargin) / 2);
            BeatInfo clickHeartbeat = BeatInfo.Empty;
            foreach (var heartbeat in pageHeartbeatInfos)
            {
                if (heartbeat.StartIndex <= sample && heartbeat.EndIndex >= sample)
                {
                    clickHeartbeat = heartbeat;
                    break;
                }
            }

            // 如果找到心搏波形，添加覆盖该区域的矩形
            float left = sampleToAxisX(Convert.ToInt32(clickHeartbeat.Sample - 3.5 / speed * AnaValues.holterConfig.SampleRate));
            float top = TopMargin;
            float right = sampleToAxisX(Convert.ToInt32(clickHeartbeat.Sample + 3.5 / speed * AnaValues.holterConfig.SampleRate));
            float bottom = _graphicsControl.Height;

            selectedRect = new SKRect(left, top, right, bottom);

            drawEcgWave = new DrawEcgWave();

        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            // 清除画布
            canvas.Clear(SKColors.Transparent);

            // 确保离屏缓冲区已更新
            width = imageWidth;
            height = imageHeight;

            rowSeconds = (width - LeftMargin) / pixMM / speed / scale;
            rowPoints = Convert.ToInt32((width - LeftMargin) / pixMM / speed / scale * SamplingRate);

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

            try
            {
                // 绘制背景
                drawEcgWave.DrawBackground(canvas, width, height);
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
            // 只有在左键点击时才执行选择和绘制矩形的操作
            if (e.Button == MouseButtons.Left)
            {
                // 检查pictureBox1是否处于选中状态
                if (isPictureBox1Highlighted)
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
                else
                {
                    // 正常模式：使用原有逻辑
                    clickPoint.X = 0;
                    selectedRect = SKRect.Empty;

                    // 开始绘制矩形模式
                    isDrawingRect = true;
                    rectStartPoint = new SKPoint(e.X, e.Y);
                    rectEndPoint = rectStartPoint;
                }
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            // 检查是否在调整临时矩形大小
            if (isResizingTempRect)
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

                // 重绘当前可见的渲染控件
                reRendWave();

            }
            // 检查是否在拖动临时矩形
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

                // 重绘当前可见的渲染控件
                reRendWave();
            }
            // 检查是否在绘制临时矩形（pictureBox1选中状态）
            else if (isDrawingTempRect)
            {
                // 更新临时矩形结束点，并限制在波形区域内
                float clampedX = Math.Max(LeftMargin, Math.Min(e.X, width));
                float clampedY = Math.Max(TopMargin, Math.Min(e.Y, height));
                tempRectEndPoint = new SKPoint(clampedX, clampedY);

                // 重绘当前可见的渲染控件
                reRendWave();
            }
            // 只有在绘制矩形模式下（左键点击开始）才更新矩形
            else if (isDrawingRect)
            {
                // 更新矩形结束点
                rectEndPoint = new SKPoint(e.X, e.Y);
                float left = Math.Min(rectStartPoint.X, rectEndPoint.X);
                float top = TopMargin;
                float right = Math.Max(rectStartPoint.X, rectEndPoint.X);
                float bottom = _graphicsControl.Height;
                selectedRect = new SKRect(left, top, right, bottom);

                reRendWave();
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            // 检查是否在调整临时矩形大小
            if (isResizingTempRect)
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
                    }
                    else
                    {
                        selectedRect = SKRect.Empty;
                        clickPoint.X = e.X;
                    }
                }

                //结束绘制矩形
                isDrawingRect = false;

            }

            reRendWave();

        }

        // 鼠标滚动事件处理
        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastWheelTime < TimeSpan.FromMilliseconds(20)) return;

            // 计算滚动的秒数（根据滚动方向和当前显示行数）
            int scrollSeconds = e.Delta > 0 ? -currentRows * wheelSeconds : currentRows * wheelSeconds;

            // 累积待处理的滚动秒数
            pendingScrollSeconds += scrollSeconds;

            if (pendingScrollSeconds != 0)
            {
                // 更新时间偏移
                waveformTimeOffsetSeconds += pendingScrollSeconds;
                waveformTimeOffsetSeconds = Math.Max(0, Math.Min(waveformTimeOffsetSeconds, hScrollBar1.Maximum));
                // 更新滚动条位置
                hScrollBar1.Value = Convert.ToInt32(waveformTimeOffsetSeconds);

                ForceWaveformRedraw();

                // 重置待处理滚动秒数
                pendingScrollSeconds = 0;

                OnEnlargeWaveStartTimeChanged?.Invoke(waveformTimeOffsetSeconds, rowSeconds);

            }
            _lastWheelTime = DateTime.Now;

        }

        #endregion

        //将横坐标转换为采样点序号
        private int xAxisToSample(float x)
        {
            float clickedXInRow = x - LeftMargin;

            int startIndex = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            return startIndex + Convert.ToInt32(clickedXInRow / pixMM / scale / speed * AnaValues.holterConfig.SampleRate);
        }

        //将采样点序号转换为横坐标
        private float sampleToAxisX(int sample)
        {
            int startIndex = Convert.ToInt32(waveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            return LeftMargin + (float)(sample - startIndex) / AnaValues.holterConfig.SampleRate * speed * pixMM * scale;
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

        private void DrawRectangles(SKCanvas canvas)
        {
            // 创建矩形画笔
            using (SKPaint rectPaint = new SKPaint())
            {
                rectPaint.IsAntialias = true;

                rectPaint.Color = new SKColor(0, 0, 255, 128); // 半透明浅蓝色
                rectPaint.Style = SKPaintStyle.Stroke;
                rectPaint.StrokeWidth = 1.5f;

                rectPaint.Color = new SKColor(74, 122, 196, 100); // 半透明浅蓝色
                rectPaint.Style = SKPaintStyle.Fill;

                // 绘制已保存的矩形
                foreach (SKRect rect in drawnRects)
                {
                    canvas.DrawRect(rect, rectPaint);
                }

                // 绘制临时矩形（pictureBox1选中状态）
                if (isPictureBox1Highlighted)
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
                            canvas.DrawRect(currentTempRect, tempRectPaint);
                            int RRMs = Convert.ToInt32(currentTempRect.Width / speed / scale / pixMM * 1000);
                            if(RRMs > 0)
                            {
                                int HR = 60 * 1000 / RRMs;
                                float gainMv = currentTempRect.Height / pixMM / gain / scale;
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

                            int RRMs = Convert.ToInt32(tempRect.Width / speed / scale / pixMM * 1000);
                            if(RRMs > 0)
                            {
                                int HR = 60 * 1000 / RRMs;
                                float gainMv = tempRect.Height / pixMM / gain / scale;
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
            ForceWaveformRedraw();

            OnEnlargeWaveStartTimeChanged?.Invoke(waveformTimeOffsetSeconds, rowSeconds);
        }

        private void reRendWave()
        {
            if(_graphicsControl != null)
            {
                _graphicsControl.Invalidate();

            }
        }

        // 强制重绘波形
        public void ForceWaveformRedraw()
        {
            UpdateEcgData();
            reRendWave();
        }

        private void InitializeEcgData()
        {
            ecgData.Clear();
            pageHeartbeatInfos.Clear();

            rowSeconds = (width - LeftMargin) / pixMM / speed / scale;
            rowPoints = Convert.ToInt32((width - LeftMargin) / pixMM / speed / scale * SamplingRate);

            ecgData = AnaCode.ReadAllWaveData(leadIdxs, Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate), Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate) + currentRows * rowPoints);

            cachedAllBeats = AnaValues.CachedAllBeats;

            int startSample = Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate);
            int endSample = startSample + ecgData[leadIdxs[0]].DataLength;

            pageHeartbeatInfos = AnaValues.CachedAllBeats
            .Where(h => h.EndIndex > startSample && h.StartIndex < endSample)
            .OrderBy(h => h.StartIndex)
            .ToList();

            UpdateScrollBarMax();

        }

        private void UpdateEcgData()
        {
            if (drawEcgWave == null)
            {
                return;
            }

            _ = DateTime.Now.Ticks;

            // 清空数据并重置标志
            ecgData.Clear();
            rowSeconds = (width - LeftMargin) / pixMM / speed / scale;
            rowPoints = Convert.ToInt32((width - LeftMargin) / pixMM / speed / scale * SamplingRate);

            ecgData = AnaCode.ReadAllWaveData(leadIdxs, Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate), Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate) + currentRows * rowPoints);


            //获取并存储每个采样点的状态位
            //_ = AnaValues.HolterDataReader.GetSampleStatusBits();


            pageHeartbeatInfos.Clear();
            int startSample = Convert.ToInt32(waveformTimeOffsetSeconds * SamplingRate);
            int endSample = startSample + ecgData[leadIdxs[0]].DataLength;

            pageHeartbeatInfos = AnaValues.CachedAllBeats
            .Where(h => h.EndIndex > startSample && h.StartIndex < endSample)
            .OrderBy(h => h.StartIndex)
            .ToList();
        }

        // 更新滚动条的最大值
        private void UpdateScrollBarMax()
        {
            if (ecgData.Count > 0 && ecgData[leadIdxs[0]].DataLength > 0)
            {
                // 计算总秒数
                int totalSeconds = AnaValues.holterConfig.Total / SamplingRate;

                hScrollBar1.Maximum = Math.Max(0, (AnaValues.holterConfig.Total - (currentRows * rowPoints)) / SamplingRate);
                // 调试信息输出
                //Console.WriteLine($"[UpdateScrollBarMax] 总采样点: {AnaValues.holterConfig.Total:N0}, 采样率: {SamplingRate}Hz, 总秒数: {totalSeconds}, 滚动条最大值: {hScrollBar1.Maximum}");
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // 切换pictureBox1的背景色状态
            isPictureBox1Highlighted = !isPictureBox1Highlighted;

            // 根据状态设置背景色
            if (isPictureBox1Highlighted)
            {
                // 清空drawnRects列表
                if (drawnRects.Count > 0)
                {
                    drawnRects.Clear();
                }
                clickPoint.X = 0;

                // 背景色加深（使用较深的颜色）
                pictureBox1.BackColor = Color.FromArgb(159, 200, 255); // 深灰色
            }
            else
            {
                tempRect = SKRect.Empty;
                // 恢复原始背景色
                pictureBox1.BackColor = SystemColors.Control; // 默认控件背景色
            }
            // 立即强制重绘界面
            reRendWave();
            // 强制重绘以显示效果
            pictureBox1.Invalidate();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            // 根据滚动事件的类型处理
            if (e.Type == ScrollEventType.SmallIncrement) // 向下箭头
            {
                pendingScrollValue = Math.Min(Convert.ToInt32(waveformTimeOffsetSeconds) + wheelSeconds, hScrollBar1.Maximum);
            }
            else if (e.Type == ScrollEventType.SmallDecrement) // 向上箭头
            {
                pendingScrollValue = Math.Max(Convert.ToInt32(waveformTimeOffsetSeconds) - wheelSeconds, 0);
            }
            else if (e.Type == ScrollEventType.LargeDecrement) // 点击滚动条上方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = hScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.X - 17) * 1.0f / hScrollBar1.Width) * hScrollBar1.Maximum);
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.LargeIncrement) // 点击滚动条下方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = hScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.X + 17) * 1.0f / hScrollBar1.Width) * hScrollBar1.Maximum);
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.ThumbTrack || e.Type == ScrollEventType.ThumbPosition)
            {
                // 拖动滑块时，直接设置待处理值为滑块位置
                pendingScrollValue = Math.Max(0, Math.Min(e.NewValue, hScrollBar1.Maximum));
            }

            ScrollBarScroll();
            _lastScrollTime = DateTime.Now;

        }

        /// <summary>
        /// 波形起始时间改变事件处理
        /// </summary>
        public void OnWaveformStartTimeChanged(float timeOffsetSeconds)
        {
            // 设置波形起始时间
            waveformTimeOffsetSeconds = timeOffsetSeconds;

            // 更新滚动条位置
            if (hScrollBar1 != null)
            {
                hScrollBar1.Value = Math.Max(0, Math.Min(Convert.ToInt32(timeOffsetSeconds), hScrollBar1.Maximum));
            }

            // 强制重绘波形
            ForceWaveformRedraw();
        }
    }
}
