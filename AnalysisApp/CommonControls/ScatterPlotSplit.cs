using AnalysisApp.Commons;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class ScatterPlotSplit : BaseDrawingControl
    {
        private int _selectMode = 0;    //选框类型
        private int _scatterType = 0;   //散点图类型
        private int _maxScale = 2000; //最大RR间期值
        private int _unitSeconds = 1800; //30分钟
        private int _pageCount = 1;
        private int _curPageIndex = 0;
        private List<BeatInfo> _sourceBeats;
        private RectangleInfo[] _rects = new RectangleInfo[16];

        private bool _isScatterPlotValid = false;   //整个散点图区域是否无效
        private bool _isBeatsValid = false;         //散点图中心搏是否无效
        private Dictionary<char, List<InnerPoint>> _pointGroups;
        private SKPaint selectionPaint;
        private List<SKPoint> selectionPath;
        private bool _isSelecting;

        private Dictionary<string, Image> _toolImages;
        private Size _originalSize;
        private const int MAX_RR = 4000;

        //每页可显示的时长(s)
        private int PageSize { get { return 16 * _unitSeconds; } }
        //当前页之前的时间跨度(s)
        private int PageBeatOffset { get { return _curPageIndex * PageSize; } }

        public string BaseBeatType = "";
        private RectangleInfo _zoomedRect;
        public List<BeatInfo> TimeSpanBeats;

        public event Action<int> OnSelectModeChanged;
        public event Action<object, List<BeatInfo>, bool> OnBeatSelected;
        public event Action<bool> OnVisibleChanged;

        public ScatterPlotSplit()
        {
            InitializeComponent();

            _sourceBeats = new List<BeatInfo>(AnaValues.CachedAllBeats.Count);
            _pointGroups = new Dictionary<char, List<InnerPoint>>();
            selectionPath = new List<SKPoint>();
        }

        #region 事件
        private void ScatterPlot_Load(object sender, EventArgs e)
        {
            init();

            _originalSize = this.Size;
            SetBeatType(""); //加载全部心搏散点

            this.btnSelect.Click += (o, args) =>
            {
                _selectMode ^= 1;
                selectionPath.Clear();
                //this.btnSelect.Image = this.imageList1.Images[_selectMode];
                if (_selectMode == 0)
                    this.btnSelect.Image = _toolImages["polyline"];
                else
                    this.btnSelect.Image = _toolImages["rect"];

                _graphicsControl.Invalidate();
                OnSelectModeChanged?.Invoke(_selectMode);
            };
        }

        private void ScatterPlot_SizeChanged(object sender, EventArgs e)
        {
            if (this.FindForm() != null && this.FindForm().WindowState != FormWindowState.Minimized)
            {
                if (this.Size != _originalSize && _graphicsControl != null)
                {
                    this.panel2.Width = this.panel2.Height;
                    //if (this.Width > this.Height + tableLayoutPanel2.Width)
                    //    this.Width = this.Height + tableLayoutPanel2.Width;
                    RefreshUI(sender);
                }
            }
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0) //鼠标滚轮向下滚动
            {
                _curPageIndex++;
                if (_curPageIndex > _pageCount - 1)
                {
                    _curPageIndex = _pageCount - 1;
                }
                else { RefreshUI(sender); }
            }
            else if (e.Delta > 0)
            {
                _curPageIndex--;
                if (_curPageIndex < 0)
                {
                    _curPageIndex = 0;
                }
                else { RefreshUI(sender); }
            }
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                _zoomedRect = null;
                TimeSpanBeats = null;

                selectionPath.Clear();
                selectionPath.Add(new SKPoint(e.X, e.Y));

                _graphicsControl.Invalidate();
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                // 限制添加点的频率，提高性能
                SKPoint newPoint = new SKPoint(e.X, e.Y);

                SKRect relRect = SKRect.Empty;
                foreach (RectangleInfo rectInfo in _rects)
                {
                    if (rectInfo.Rect.Contains(selectionPath[0]))
                    {
                        relRect = rectInfo.Rect;
                        break;
                    }
                }

                if (relRect.Contains(newPoint))
                {
                    if (_selectMode == 0)
                    {
                        if (SKPoint.Distance(selectionPath[selectionPath.Count - 1], newPoint) > 2.0f)
                        {
                            selectionPath.Add(newPoint);

                            // 仅重绘圈选路径（不重绘散点）
                            redrawSelectionPathOnly(selectionPath);
                        }
                    }
                    else
                    {
                        if (selectionPath.Count == 1)
                            selectionPath.Add(newPoint);
                        else if (selectionPath.Count > 1)
                            selectionPath[1] = newPoint;

                        redrawSelectionPathOnly(selectionPath);
                    }
                }
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;

                executeSelect(sender);

                // 优化：圈选完成后重绘整个控件（包括背景色变化）
                _graphicsControl.Invalidate();
            }

            //显示右键菜单
            if (e.Button == MouseButtons.Right)
            {

            }
        }

        private void OnCanvasMouseDoubleClick(object sender, MouseEventArgs e)
        {
            List<ScatterPlot> scatters = CommonFuns.RetriveSubControls<ScatterPlot>(this.Parent);
            if (scatters.Count > 0)
            {
                this.Parent.SuspendLayout();
                SKPoint clickPos = new SKPoint(e.X, e.Y);
                foreach (RectangleInfo rectInfo in _rects)
                {
                    if (rectInfo.Rect.Contains(clickPos))
                    {
                        _zoomedRect = rectInfo;
                        TimeSpanBeats = rectInfo.Beats;
                        ScatterPlot scatterPlot = scatters[0];
                        scatterPlot.Width = this.Width;
                        scatterPlot.Visible = true;
                        scatterPlot.SetBeatSource(TimeSpanBeats);
                        this.Width = 0;
                        this.Hide();
                        break;
                    }
                }
                this.Parent.ResumeLayout();
                OnVisibleChanged?.Invoke(this.Visible);
            }
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            try
            {
                // 检查是否是部分重绘（仅重绘圈选路径区域）
                bool isPartialRedraw = imageWidth < _graphicsControl.Width || imageHeight < _graphicsControl.Height;
                if (!isPartialRedraw)
                {
                    canvas.Clear(SKColors.White);

                    // 检查离屏surface状态，必要时重新创建
                    if (offscreenSurface == null || offscreenImage == null)
                    {
                        createOffscreenSurface();
                    }
                    if (offscreenImage != null)
                    {
                        canvas.DrawImage(offscreenImage, 0, 0);
                    }
                }

                if (_selectMode == 0)
                    drawPolygon(canvas);
                else
                    drawRectangle(canvas);
            }
            catch (AccessViolationException)
            {
                // GPU内存访问冲突，记录但不抛出异常
                Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
                // 重新创建surface以恢复正常状态
                createOffscreenSurface();
            }
            catch (ObjectDisposedException)
            {
                // 对象已释放，记录但不抛出异常
                Console.WriteLine("绘制对象已释放，已安全处理");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPaintSurface错误: {ex.GetType().Name}: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 设置要显示到散点图中的散点类型
        /// </summary>
        public void SetBeatType(string beatType)
        {
            BaseBeatType = beatType;
            if (!DesignMode)
            {
                List<BeatInfo> allBeats = AnaValues.CachedAllBeats;
                _sourceBeats = AnaCode.GetBeatsWithType(allBeats, beatType);

                RefreshUI(null, true);
            }
        }

        /// <summary>
        /// 更新当前组件的数据源
        /// </summary>
        public void UpdateBeatSource(object sender, string beatType, List<BeatInfo> newBeats = null)
        {
            if (newBeats == null)
            {
                _sourceBeats = AnaCode.GetBeatsWithType(AnaValues.CachedAllBeats, beatType);
            }
            else
            {
                _sourceBeats = newBeats;
            }
            RefreshUI(sender, false, true);
            if (_zoomedRect != null) { TimeSpanBeats = _zoomedRect.Beats; }

            executeSelect(sender, true);
        }

        //刷新显示
        public void RefreshUI(object sender, bool resetPages = false, bool keepSelection = false)
        {
            if (resetPages)
            {
                //计算显示所需页数
                _pageCount = (int)Math.Ceiling(24F * 60 * 60 / PageSize);
                _curPageIndex = 0;
            }

            if (!keepSelection)
            {
                selectionPath.Clear();
                _pointGroups.Clear();
            }
            if (_sourceBeats != null && _sourceBeats.Count > 0)
            {
                foreach (char key in AnaValues.WaveformColorMap.Keys)
                {
                    _pointGroups[key] = new List<InnerPoint>();
                }
            }

            //生成各显示区及相关散点集
            setScatterRects();

            //重新创建离屏缓存
            createOffscreenSurface();

            _graphicsControl.Invalidate();

            if (_sourceBeats != null && _sourceBeats.Count > 0)
            {
                if (keepSelection)
                {
                    executeSelect(sender, true);
                }
            }
        }

        private void init()
        {
            _toolImages = new Dictionary<string, Image>()
            {
                { "polyline", CommonFuns.GetResizedImage(Properties.Resources.polyline, 16, 16)},
                { "rect", CommonFuns.GetResizedImage(Properties.Resources.rect, 16, 16)},
            };
            this.btnSelect.Image = _toolImages["polyline"];

            int[] arrMinute = new int[] { 30, 60, 120, 180, 360 };
            for (int i = 0; i < arrMinute.Length; i++)
            {
                int minutes = arrMinute[i];
                string unitText = minutes < 60 ? _langs["分钟"] : _langs["小时"];
                ToolStripMenuItem item = new ToolStripMenuItem((minutes < 60 ? minutes : minutes / 60) + unitText);
                item.Click += (o, e) =>
                {
                    _unitSeconds = minutes * 60;
                    RefreshUI(o, true);
                    item.Check();
                };
                this.ctxMenuStrip.Items.Add(item);
                if (_unitSeconds == minutes * 60) item.Check();
            }
            //this.panel2.ContextMenuStrip = this.ctxMenuStrip;

            if (!DesignMode)
            {
                // 初始化圈选/框选画笔
                selectionPaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 0.8f,
                    Color = SKColors.Black,
                    StrokeJoin = SKStrokeJoin.Round,
                    StrokeCap = SKStrokeCap.Round
                };

                _graphicsControl.ContextMenuStrip = this.ctxMenuStrip;
                _graphicsControl.MouseDown += OnCanvasMouseDown;
                _graphicsControl.MouseMove += OnCanvasMouseMove;
                _graphicsControl.MouseUp += OnCanvasMouseUp;
                _graphicsControl.MouseWheel += OnCanvasMouseWheel;
                _graphicsControl.MouseDoubleClick += OnCanvasMouseDoubleClick;
                this.panel2.Controls.Add(_graphicsControl);
            }
        }


        //设置分区的显示区域和内容
        private void setScatterRects()
        {
            if (DesignMode) return;

            if (_graphicsControl != null)
            {
                float w = _graphicsControl.Width / 4F;
                float h = _graphicsControl.Height / 4F;
                float xPerMs = (float)w / _maxScale, yPerMs = (float)h / _maxScale;
                int searchStart = 0;
                DateTime startTime = _holterConfig.StartDateTime;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int idx = i * 4 + j;
                        RectangleInfo rectInfo = new RectangleInfo();
                        DateTime dtStart = startTime.AddSeconds(PageBeatOffset + idx * _unitSeconds);
                        DateTime dtEnd = startTime.AddSeconds(PageBeatOffset + (idx + 1) * _unitSeconds);
                        rectInfo.StartTime = dtStart;
                        rectInfo.EndTime = dtEnd;

                        float left = w * j, top = h * i;

                        rectInfo.Rect = new SKRect(left, top, left + w, top + h);
                        for (int n = searchStart; n < _sourceBeats.Count; n++)
                        {
                            BeatInfo beat = _sourceBeats[n];
                            DateTime beatTime = startTime.AddSeconds((double)beat.Sample / _holterConfig.SampleRate);
                            if (beatTime >= dtStart)
                            {
                                if (beatTime < dtEnd)
                                {
                                    rectInfo.Beats.Add(beat);

                                    SKPoint pos = getBeatPosition(beat, rectInfo.Rect, xPerMs, yPerMs);
                                    if (pos != SKPoint.Empty) //不显示可能超出显示区域的心拍
                                    {
                                        _pointGroups[beat.BeatUiType].Add(new InnerPoint(n, pos));
                                    }
                                }
                                else
                                {
                                    searchStart = n;
                                    break;
                                }
                            }
                        }
                        _rects[idx] = rectInfo;
                    }
                }
            }
        }

        //创建离屏缓存
        private void createOffscreenSurface()
        {
            try
            {
                // 先释放现有的surface和image
                if (offscreenSurface != null)
                {
                    offscreenSurface.Dispose();
                    offscreenSurface = null;
                }
                if (offscreenImage != null)
                {
                    offscreenImage.Dispose();
                    offscreenImage = null;
                }

                // 检查控件是否有效
                if (_graphicsControl == null || _graphicsControl.IsDisposed || _graphicsControl.Width <= 0 || _graphicsControl.Height <= 0)
                {
                    return;
                }

                // 创建新的surface
                SKImageInfo imgInfo = new SKImageInfo(_graphicsControl.Width, _graphicsControl.Height);
                SKSurface surface = SKSurface.Create(imgInfo);
                if (surface != null)
                {
                    if (!_isScatterPlotValid)
                    {
                        drawCoordinateSystem(surface.Canvas, imgInfo.Width, imgInfo.Height);
                    }
                    if (!_isBeatsValid)
                    {
                        drawTimes(surface.Canvas);
                        drawScatterPoints(surface.Canvas);
                    }
                    surface.Canvas.Flush();

                    // 保存surface引用和创建快照
                    offscreenSurface = surface;
                    offscreenImage = offscreenSurface.Snapshot();
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出，避免影响UI线程
                System.Diagnostics.Debug.WriteLine($"CreateOffscreenSurface错误: {ex.GetType().Name}: {ex.Message}");

                // 在异常情况下清理所有资源
                if (offscreenImage != null)
                {
                    offscreenImage.Dispose();
                    offscreenImage = null;
                }
                if (offscreenSurface != null)
                {
                    offscreenSurface.Dispose();
                    offscreenSurface = null;
                }
            }
        }

        //绘制坐标系(包含内部网格线)
        private void drawCoordinateSystem(SKCanvas canvas, int width, int height)
        {
            // 计算绘图区域
            //int plotWidth = width - LeftMargin - RightMargin;
            //int plotHeight = height - TopMargin - BottomMargin - HourLabelHeight;

            // 创建画笔
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1F
            })
            using (SKPaint gridPaint = new SKPaint
            {
                Color = SKColors.Silver,
                StrokeWidth = 1F,
                Style = SKPaintStyle.Stroke,
            })
            {
                SKPoint originalPos = new SKPoint(0, height);
                float x0 = 0, y0 = originalPos.Y;
                float intervalPx = width / 4F, intervalPy = height / 4F;
                int interval = _scatterType == 0 ? _maxScale / 4 : _maxScale / 2;
                int originValue = _scatterType == 0 ? 0 : -_maxScale;

                // 绘制X轴、网格线
                for (int i = 0; i < 4; i++)
                {
                    if (i == 0)
                    {
                        canvas.DrawLine(intervalPx / 2, y0, intervalPx / 2, 0, gridPaint);
                    }
                    else
                    {
                        canvas.DrawLine(i * intervalPx, y0, i * intervalPx, 0, axisPaint);
                        canvas.DrawLine(i * intervalPx + intervalPx / 2, y0, i * intervalPx + intervalPx / 2, 0, gridPaint);
                    }
                }

                // 绘制Y轴、网格线
                for (int i = 0; i < 4; i++)
                {
                    if (i == 0)
                    {
                        canvas.DrawLine(0, y0 - intervalPy / 2, width, y0 - intervalPy / 2, gridPaint);
                    }
                    else
                    {
                        canvas.DrawLine(0, y0 - i * intervalPy, width, y0 - i * intervalPy, axisPaint);
                        canvas.DrawLine(0, y0 - i * intervalPy - intervalPy / 2, width, y0 - i * intervalPy - intervalPy / 2, gridPaint);
                    }
                }
            }
        }

        //绘制各散点图的开始时间
        private void drawTimes(SKCanvas canvas)
        {
            //绘制时间
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont(SKTypeface.FromFamilyName("微软雅黑"), 10.5F))
            {
                for (int i = 0; i < _rects.Length; i++)
                {
                    SKRect rect = _rects[i].Rect;
                    string timeText = _rects[i].StartTime.ToString("HH:mm");
                    canvas.DrawText(timeText, rect.Left + 6, rect.Bottom - 3, textFont, textPaint);
                }
            }
        }

        //绘制心搏散点
        private void drawScatterPoints(SKCanvas canvas)
        {
            if (_pointGroups.Count <= 0) return;

            using (var pointPaint = new SKPaint())
            {
                // 配置画笔参数
                pointPaint.IsAntialias = true;
                pointPaint.Style = SKPaintStyle.Stroke;
                pointPaint.StrokeWidth = 3.0f;
                pointPaint.FilterQuality = SKFilterQuality.None;

                // 按颜色组批量绘制点
                foreach (var group in _pointGroups)
                {
                    if (group.Value.Count > 0)
                    {
                        pointPaint.Color = _beatColorMap[group.Key];
                        SKPoint[] points = group.Value.ConvertAll(o => o.Location).ToArray();
                        canvas.DrawPoints(SKPointMode.Points, points, pointPaint);
                    }
                }
            }
        }

        private void drawPolygon(SKCanvas canvas)
        {
            if (selectionPath.Count > 1) //绘制圈选路径（动态内容）
            {
                // 绘制开放路径
                var pathArray = selectionPath.ToArray();
                if (pathArray.Length > 0)
                {
                    canvas.DrawPoints(SKPointMode.Polygon, pathArray, selectionPaint);
                }

                // 如果正在圈选，绘制从最后一个点到当前鼠标位置的线
                if (_isSelecting && MousePosition != null)
                {
                    try
                    {
                        var mousePoint = _graphicsControl.PointToClient(MousePosition);
                        var lastPoint = selectionPath[selectionPath.Count - 1];
                        canvas.DrawLine(lastPoint.X, lastPoint.Y, mousePoint.X, mousePoint.Y, selectionPaint);
                    }
                    catch (ArgumentException)
                    {
                        // 坐标值无效，忽略此绘制操作
                    }
                }
                // 如果圈选已完成，闭合路径
                else if (!_isSelecting && selectionPath.Count >= 3)
                {
                    try
                    {
                        var firstPoint = selectionPath[0];
                        var lastPoint = selectionPath[selectionPath.Count - 1];
                        canvas.DrawLine(lastPoint.X, lastPoint.Y, firstPoint.X, firstPoint.Y, selectionPaint);
                    }
                    catch (ArgumentException)
                    {
                        // 坐标值无效，忽略此绘制操作
                    }
                }
            }
        }

        private void drawRectangle(SKCanvas canvas)
        {
            if (selectionPath.Count > 1)
            {
                SKPoint point1 = selectionPath[0], point2 = selectionPath[1];
                float left = Math.Min(point1.X, point2.X), top = Math.Min(point1.Y, point2.Y);
                float right = Math.Max(point1.X, point2.X), bottom = Math.Max(point1.Y, point2.Y);
                canvas.DrawRect(left, top, right - left, bottom - top, selectionPaint);
            }
        }

        // 仅重绘圈选路径区域
        private void redrawSelectionPathOnly(List<SKPoint> selectionPath)
        {
            // 计算圈选路径的边界区域
            if (selectionPath.Count > 1)
            {
                float minX = selectionPath.Min(p => p.X);
                float maxX = selectionPath.Max(p => p.X);
                float minY = selectionPath.Min(p => p.Y);
                float maxY = selectionPath.Max(p => p.Y);

                // 添加一些边距确保路径完全可见
                var rect = new Rectangle((int)(minX - 10), (int)(minY - 10),
                                        (int)(maxX - minX + 20), (int)(maxY - minY + 20));

                // 仅重绘圈选路径区域
                _graphicsControl.Invalidate(rect);
            }
            else
            {
                // 如果没有路径，重绘整个控件
                _graphicsControl.Invalidate();
            }
        }

        //获取单个心搏的位置
        private SKPoint getBeatPosition(BeatInfo beat, SKRect rect, float xPerMs, float yPerMs)
        {
            float xValue, yValue;
            float w = rect.Width, h = rect.Height;
            if (_scatterType == 0)
            {
                int xVal = beat.RRInterval, yVal = beat.RRLatter;
                xValue = xVal * xPerMs;
                yValue = h - yVal * yPerMs;
            }
            else
            {
                int xVal = beat.RRInterval - beat.RRPrevious, yVal = beat.RRLatter - beat.RRInterval;
                xValue = w / 2 + xVal * xPerMs;
                yValue = h / 2 - yVal * yPerMs;
            }

            SKPoint pos = new SKPoint(xValue, yValue);
            if (xValue > 0 && yValue > 0)
            {
                pos.Offset(rect.Left, rect.Top);
                return pos;
            }
            else
            {
                return SKPoint.Empty;
            }
        }

        //执行选中后的操作
        private void executeSelect(object sender, bool keepSelection = false)
        {
            ConcurrentBag<BeatInfo> selectedBeats = new ConcurrentBag<BeatInfo>();
            if (_selectMode == 0)
            {
                // 确保路径至少有3个点才能形成封闭区域
                if (selectionPath.Count >= 3)
                {
                    // 添加最后一个点，确保路径闭合
                    selectionPath.Add(selectionPath[0]);

                    try
                    {
                        // 计算选中的点, 优化性能：使用并行计算
                        Parallel.ForEach(_pointGroups, group =>
                        {
                            foreach (var point in group.Value)
                            {
                                if (AnaCode.IsPointInPolygon(point.Location.X, point.Location.Y, selectionPath))
                                {
                                    selectedBeats.Add(_sourceBeats[point.Index]);
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // 处理并行计算中的异常
                        Console.WriteLine("圈选过程中发生错误: " + ex.Message);
                    }
                }
            }
            else
            {
                if (selectionPath.Count >= 2)
                {
                    SKPoint point1 = selectionPath[0], point2 = selectionPath[1];
                    SKPoint topLeft = new SKPoint(Math.Min(point1.X, point2.X), Math.Min(point1.Y, point2.Y));
                    SKPoint bottomRight = new SKPoint(Math.Max(point1.X, point2.X), Math.Max(point1.Y, point2.Y));
                    try
                    {
                        Parallel.ForEach(_pointGroups, group =>
                        {
                            foreach (var point in group.Value)
                            {
                                if (point.Location.X >= topLeft.X && point.Location.X <= bottomRight.X &&
                                    point.Location.Y >= topLeft.Y && point.Location.Y <= bottomRight.Y)
                                {
                                    selectedBeats.Add(_sourceBeats[point.Index]);
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("框选过程中发生错误: " + ex.Message);
                    }
                }
            }

            // 更新叠加图
            OnBeatSelected?.Invoke(sender, selectedBeats.ToList(), keepSelection);
        }

        private class RectangleInfo
        {
            public SKRect Rect { get; set; }
            public List<BeatInfo> Beats { get; set; } = new List<BeatInfo>();
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        private class InnerPoint
        {
            /// <summary>
            /// 心拍序号
            /// </summary>
            internal int Index { get; set; }
            /// <summary>
            /// 心拍在绘图区内的位置
            /// </summary>
            internal SKPoint Location { get; set; }

            public InnerPoint(int index, SKPoint pos)
            {
                Index = index;
                Location = pos;
            }
        }
    }
}
