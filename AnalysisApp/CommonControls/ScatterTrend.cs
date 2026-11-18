using AnalysisApp.Commons;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class ScatterTrend : BaseDrawingControl
    {
        private int _maxScale = 2000; //最大RR间期值
        private List<BeatInfo> _sourceBeats;

        private bool _isScatterTypeBased = false;   //是否基于心搏类型检索数据
        private bool _isScatterPlotValid = false;   //整个散点图区域是否无效
        private bool _isBeatsValid = false;         //散点图中心搏是否无效
        private Dictionary<char, List<InnerPoint>> _pointGroups;
        private SKPaint selectionPaint;
        private SKPaint selectionInnerPaint;
        private SKPaint locatePaint;
        private float selectStartX;
        private SKRect selectionRect;
        private DateTime selectionStartTime, selectionEndTime; //所选区域起止时间

        private bool _isSelecting;
        private RectangleF _innerRect;
        private float _duration = 24 * 60 * 60;     // 绘图区数据长度(s)
        private float _realTimeSpan;                //实际采样时间(s)
        private float _defaultSelectLength = 3600;  //秒
        private float _maxX;
        private List<AfInfo> _afList;

        public string BaseBeatType = "";

        public event Action<BeatInfo> OnBeatClicked;
        public event Action<object, DateTime, DateTime, List<BeatInfo>, bool> OnRangeSelected;

        [Browsable(false), Category("自定义属性"), Description("心搏趋势图开始时间")]
        public DateTime StartTime { get; set; }
        [Browsable(false), Category("自定义属性"), Description("心搏趋势图结束时间")]
        public DateTime EndTime { get; set; }

        [Browsable(true), Category("自定义属性"), Description("是否显示右键菜单"), DefaultValue(true)]
        public bool ShowMenuStrip { get; set; } = true;

        /// <summary>
        /// 显示与选择方式(0:显示24小时心搏, 1:显示指定时间段的心搏)
        /// </summary>
        [DefaultValue(0), Category("自定义属性"), Description("心搏趋势图类型(用来区分显示与选择方式)")]
        public int SelectMode { get; set; } = 0;

        /// <summary>
        /// 房颤/房扑信息数据
        /// </summary>
        public List<AfInfo> AFList
        {
            get => _afList;
            set
            {
                _afList = value;
                //RefreshUI();
            }
        }

        /// <summary>
        /// 设置选择范围的区间长度(s)
        /// </summary>
        public float DefaultSelectLength
        {
            get => _defaultSelectLength;
            set
            {
                _defaultSelectLength = value;
                if (selectionRect != SKRect.Empty)
                {
                    float xPerS = _innerRect.Width / _duration;
                    float newSelLength = _defaultSelectLength * xPerS;
                    float midX = selectionRect.Left + selectionRect.Width / 2;
                    selectionRect = adjustSelectionRect(midX - newSelLength / 2, newSelLength, midX, true);
                    RefreshUI(null, true);
                }
            }
        }

        public ScatterTrend()
        {
            InitializeComponent();
        }

        #region 事件
        private void ScatterTrend_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                init();

                deriveDrawSize(_graphicsControl.Width, _graphicsControl.Height);

                if (SelectMode == 0)
                {
                    //加载全部心搏散点
                    SetBeatType("");

                    ////默认点击选择
                    //_isSelecting = true;
                    //OnCanvasMouseUp(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                }
            }
        }

        private void ScatterTrend_SizeChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (_graphicsControl != null && this.FindForm().WindowState != FormWindowState.Minimized)
                {
                    if (this.Size != _firstActualSize)
                    {
                        deriveDrawSize(_graphicsControl.Width, _graphicsControl.Height);

                        RefreshUI(sender);
                        _firstActualSize = this.Size;
                    }
                }
            }
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                selectStartX = e.X;

                if (SelectMode == 0)
                {
                    selectionRect = SKRect.Empty;
                }
                else if (SelectMode == 1) // 触发点击(定位波形)事件
                {
                    int idx = 0;
                    float w = _innerRect.Width, h = _innerRect.Height;
                    float xPerS = w / _duration, yPerMs = h / _maxScale;
                    float offset = (float)StartTime.Subtract(_holterConfig.StartDateTime).TotalSeconds;

                    if (_sourceBeats != null && _sourceBeats.Count > 0)
                    {
                        //Task.Run(() =>
                        //{
                        for (int i = 0; i < _sourceBeats.Count; i++)
                        {
                            BeatInfo bInfo = _sourceBeats[i];
                            float xVal = (float)bInfo.Sample / _holterConfig.SampleRate;
                            float xValue = _innerRect.Left + (xVal - offset) * xPerS;
                            if (xValue >= e.X)
                            {
                                idx = i;
                                break;
                            }
                            if (i == _sourceBeats.Count - 1) idx = i;
                        }
                        //});

                        OnBeatClicked?.Invoke(_sourceBeats[idx]);
                    }
                }

                _graphicsControl.Invalidate();
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting && Math.Abs(e.X - selectStartX) > 0)
            {
                float left = selectStartX, right = e.X;
                if (selectStartX > e.X)
                {
                    left = e.X;
                    right = selectStartX;
                }

                selectionRect = adjustSelectionRect(left, right - left, right);
                _graphicsControl.Invalidate();
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;

                float xPerS = _innerRect.Width / _duration;

                if (SelectMode == 0)
                {
                    if (selectionRect == SKRect.Empty)
                    {
                        //未拖选时默认选中一段
                        float selLength = _defaultSelectLength * xPerS;
                        selectionRect = adjustSelectionRect(e.X - selLength / 2, selLength, e.X, true);
                    }

                    if (selectionRect != SKRect.Empty) executeSelect(sender);
                }
                else
                {
                    if (e.X != selectStartX)
                    {
                        executeSelect(sender);
                    }
                }

                // 优化：圈选完成后重绘整个控件（包括背景色变化）
                _graphicsControl.Invalidate();
            }

            //显示右键菜单
            if (e.Button == MouseButtons.Right)
            {

            }
        }

        protected override void OnControlKeyDown(Keys keyData)
        {
            if (keyData == Keys.Left)
            {
                MoveSelection(-3);
            }
            else if (keyData == Keys.Right)
            {
                MoveSelection(3);
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

                if (SelectMode == 0)
                {
                    if (selectionRect != SKRect.Empty) canvas.DrawRect(selectionRect, selectionPaint);
                }
                else
                {
                    if (selectStartX > 0) canvas.DrawLine(selectStartX, _innerRect.Top, selectStartX, _innerRect.Bottom, locatePaint);
                    if (selectionRect != SKRect.Empty) canvas.DrawRect(selectionRect, selectionInnerPaint);
                }
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
        public void SetBeatType(string beatType, bool keepSelection = false)
        {
            _isScatterTypeBased = true;
            BaseBeatType = beatType;
            List<BeatInfo> allBeats = AnaValues.CachedAllBeats;
            List<BeatInfo> sourceBeats = AnaCode.GetBeatsWithType(allBeats, beatType);
            _duration = 24 * 60 * 60;
            StartTime = _holterConfig.StartDateTime;
            EndTime = StartTime.AddHours(24);

            SetBeatSource(StartTime, EndTime, sourceBeats, true, keepSelection);
        }

        public void SetBeatSource(DateTime startTime, DateTime endTime, List<BeatInfo> beats, bool fulltime = false, bool keepSelection = false)
        {
            _sourceBeats = beats;
            if (!fulltime)
            {
                //int cnt = _sourceBeats.Count;
                //int sampleRate = _holterConfig.SampleRate;
                //_duration = (float)(_sourceBeats[cnt - 1].Sample - _sourceBeats[0].Sample) / sampleRate;
                //_startTime = _holterConfig.StartDateTime.AddSeconds(_sourceBeats[0].Sample / sampleRate);
                //_endTime = _holterConfig.StartDateTime.AddSeconds(_sourceBeats[cnt - 1].Sample / sampleRate);
                StartTime = startTime;
                EndTime = endTime;
                _duration = (float)EndTime.Subtract(StartTime).TotalSeconds;
            }

            setScalarValues();

            RefreshUI(null, keepSelection);
        }

        /// <summary>
        /// 更新当前组件的数据源
        /// </summary>
        public void UpdateBeatSource(object sender, DateTime startTime, DateTime endTime, string beatType, List<BeatInfo> newBeats = null)
        {
            StartTime = startTime;
            EndTime = endTime;
            _duration = (float)EndTime.Subtract(StartTime).TotalSeconds;
            if (newBeats == null)
            {
                _sourceBeats = AnaCode.GetBeatsWithType(AnaValues.CachedAllBeats, beatType);
            }
            else
            {
                _sourceBeats = newBeats;
            }
            RefreshUI(sender, true);
        }

        private void init()
        {
            _sourceBeats = new List<BeatInfo>(AnaValues.CachedAllBeats.Count);
            _pointGroups = new Dictionary<char, List<InnerPoint>>();

            // 初始化圈选/框选画笔
            selectionPaint = new SKPaint()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                Color = SKColors.Blue
            };
            selectionInnerPaint = new SKPaint()
            {
                IsStroke = false,
                Color = new SKColor(128, 128, 128, 64),
                BlendMode = SKBlendMode.SrcOver
            };
            locatePaint = new SKPaint()
            {
                Color = SKColors.Black
            };

            initMenuItems();
            AFList = AnaCode.GetAfList();

            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            if (ShowMenuStrip)
            {
                _graphicsControl.ContextMenuStrip = this.ctxMenuStrip;
            }
            this.Controls.Add(_graphicsControl);
        }

        private void initMenuItems()
        {
            this.mnuSetAF.Click += (o, e) =>
            {
                if (selectionRect != SKRect.Empty && selectionRect.Width > 0)
                {
                    SegmentRange segment = AnaCode.GetSegment(selectionStartTime, selectionEndTime);
                    int ret = AEcgHandler.AddAf(segment.StartIndex, segment.EndIndex, 'K');
                    if (ret == 0)
                    {
                        //if (_isScatterTypeBased == true)
                        //{
                        //    AFList = AnaCode.GetAfList();
                        //    UpdateBeatSource(BaseBeatType, null);
                        //}
                        base.ChangeDataSource(null);
                    }
                    else { CommonFuns.PromptError("操作失败"); }
                }
            };
            this.mnuSetAFL.Click += (o, e) =>
            {
                if (selectionRect != SKRect.Empty && selectionRect.Width > 0)
                {
                    SegmentRange segment = AnaCode.GetSegment(selectionStartTime, selectionEndTime);
                    int ret = AEcgHandler.AddAf(segment.StartIndex, segment.EndIndex, 'M');
                    if (ret == 0)
                        base.ChangeDataSource(null);
                    else
                        CommonFuns.PromptError("操作失败");
                }
            };
            this.mnuSetFullAF.Click += (o, e) =>
            {
                int ret = AEcgHandler.AddAf(0, _holterConfig.Total, 'K');
                if (ret == 0)
                    base.ChangeDataSource(null);
                else
                    CommonFuns.PromptError("操作失败");
            };
            this.mnuSetFullAFL.Click += (o, e) =>
            {
                int ret = AEcgHandler.AddAf(0, _holterConfig.Total, 'M');
                if (ret == 0)
                    base.ChangeDataSource(null);
                else
                    CommonFuns.PromptError("操作失败");
            };
            this.mnuDelAF.Click += (o, e) =>
            {
                if (selectionRect != SKRect.Empty && selectionRect.Width > 0)
                {
                    SegmentRange segment = AnaCode.GetSegment(selectionStartTime, selectionEndTime);
                    var segments = new SegmentRange[] { segment };
                    int ret75 = AEcgHandler.DeleteAFEvents(segments, 'K');
                    int ret77 = AEcgHandler.DeleteAFEvents(segments, 'M');
                    if (ret75 == 0 && ret77 == 0)
                        base.ChangeDataSource(null);
                    else
                        CommonFuns.PromptError("操作失败");
                }
            };
            this.mnuSelectAll.Click += (o, e) =>
            {
                selectionRect = adjustSelectionRect(_innerRect.Left, _innerRect.Width, 0);
                _graphicsControl.Invalidate();
                executeSelect(o);
            };

            if (SelectMode == 0)
            {
                int[] arrMinute = new int[] { 10, 20, 30, 60, 90, 120, 240 };
                for (int i = 0; i < arrMinute.Length; i++)
                {
                    int minutes = arrMinute[i];
                    ToolStripMenuItem item = new ToolStripMenuItem(minutes + _langs["分钟"]);
                    item.Click += (o, e) =>
                    {
                        DefaultSelectLength = minutes * 60;
                        item.Check();
                    };
                    this.mnuSetRange.DropDownItems.Add(item);
                    if (_defaultSelectLength == minutes * 60) item.Check();
                }
            }
            else
            {
                this.mnuSetRange.Visible = false;
                this.mnuSetFullAF.Visible = false;
                this.mnuSetFullAFL.Visible = false;
            }
        }

        public void RefreshUI(object sender, bool keepSelection = false)
        {
            if (DesignMode || !IsLoaded) return;

            if (!keepSelection)
            {
                selectStartX = 0;
                selectionRect = SKRect.Empty;
            }

            //生成界面散点集
            genBeatPoints();

            //重新创建离屏缓存
            createOffscreenSurface();

            _graphicsControl.Invalidate();

            if (_sourceBeats != null)
            {
                if (keepSelection)
                {
                    executeSelect(sender, keepSelection);
                }
                else
                {
                    if (SelectMode == 0)  //默认点击选择
                    {
                        LocateToTime(StartTime, _defaultSelectLength);
                        //_isSelecting = true;
                        //OnCanvasMouseUp(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    }
                    else
                    {
                        OnRangeSelected?.Invoke(sender, StartTime, EndTime, _sourceBeats, false);
                    }
                }
            }
        }

        /// <summary>
        /// 将选择位置定位到指定的时间(秒)
        /// </summary>
        /// <param name="time">需定位到的时间</param>
        /// <param name="selectLength">选择的时长(秒)</param>
        public void LocateToTime(DateTime time, float selectLength, bool afSelected = false)
        {
            if (SelectMode == 0)
            {
                _isSelecting = true;
                selectionRect = SKRect.Empty;
                float xPerS = _innerRect.Width / _duration;
                float offset = (float)time.Subtract(StartTime).TotalSeconds + selectLength / 2;
                float xValue = _innerRect.Left + offset * xPerS;
                float xSelLength = selectLength * xPerS;
                selectionRect = adjustSelectionRect(xValue - xSelLength / 2, xSelLength, xValue, !afSelected);

                OnCanvasMouseUp(null, new MouseEventArgs(MouseButtons.Left, 1, (int)xValue, 0, 0));
            }
        }

        /// <summary>
        /// 将当前选择的区间方框增加指定的偏移量
        /// </summary>
        /// <param name="offset"></param>
        public void MoveSelection(float offset)
        {
            if (selectionRect == SKRect.Empty) return;

            //selectionRect.Offset(offset, 0);
            float newLeft = selectionRect.Left + offset;
            selectionRect = adjustSelectionRect(newLeft, selectionRect.Width, newLeft + selectionRect.Width / 2, true);
            executeSelect(this);
            _graphicsControl.Invalidate();
        }

        //生成界面散点集
        private void genBeatPoints()
        {
            _pointGroups.Clear();
            if (_sourceBeats != null && _sourceBeats.Count > 0)
            {
                foreach (char key in AnaValues.WaveformColorMap.Keys)
                {
                    _pointGroups[key] = new List<InnerPoint>();
                }

                float w = _innerRect.Width, h = _innerRect.Height;
                float xPerS = w / _duration, yPerMs = h / _maxScale;

                float offset = (float)StartTime.Subtract(_holterConfig.StartDateTime).TotalSeconds; ; //偏移量(s)
                for (int i = 0; i < _sourceBeats.Count; i++)
                {
                    BeatInfo bInfo = _sourceBeats[i];
                    float xValue, yValue;

                    float xVal = (float)bInfo.Sample / _holterConfig.SampleRate;
                    int yVal = bInfo.RRInterval;
                    xValue = _innerRect.Left + (xVal - offset) * xPerS;
                    yValue = _innerRect.Bottom - yVal * yPerMs;

                    _pointGroups[bInfo.BeatUiType].Add(new InnerPoint(i, new SKPoint(xValue, yValue)));
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
                        drawScatterPoints(surface.Canvas);
                    }

                    drawAfInfo(surface.Canvas);
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
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsAntialias = true
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold),
                Size = 10.5f
            })
            {
                SKPoint originalPos = new SKPoint(_innerRect.Left, _innerRect.Bottom);
                float x0 = originalPos.X;
                float y0 = originalPos.Y;

                float intervalPx = _innerRect.Width / 12, intervalPy = _innerRect.Height / 4;
                float intervalX = _duration / 12;
                int intervalY = _maxScale / 4;

                canvas.DrawLine(x0, y0, x0 + _innerRect.Width, y0, axisPaint);
                // 绘制X轴刻度、刻度值
                for (int i = 0; i <= 12; i++)
                {
                    canvas.DrawLine(x0 + i * intervalPx, y0, x0 + i * intervalPx, y0 + 5, axisPaint);
                    if (i % 2 == 0)
                    {
                        DateTime markValue = StartTime.AddSeconds(i * intervalX);
                        canvas.DrawText(markValue.ToString("HH:mm:ss"), x0 + i * intervalPx, y0 + 15, SKTextAlign.Center, textFont, textPaint);
                    }
                }

                canvas.DrawLine(x0, y0, x0, y0 - _innerRect.Height, axisPaint);
                // 绘制Y轴刻度、刻度值
                for (int i = 0; i <= 4; i++)
                {
                    canvas.DrawLine(x0, y0 - i * intervalPy, x0 - 5, y0 - i * intervalPy, axisPaint);
                    int markValue = i * intervalY;
                    canvas.DrawText(markValue.ToString(), x0 - 8, y0 - i * intervalPy + 5, SKTextAlign.Right, textFont, textPaint);
                }
                // 绘制Y轴标题
                canvas.DrawText("ms", x0, _innerRect.Top - 5, SKTextAlign.Right, textFont, textPaint);
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
                pointPaint.StrokeWidth = SelectMode == 0 ? 1.2f : 3f;
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

        //绘制房颤/房扑/室颤/室扑相关信息
        private void drawAfInfo(SKCanvas canvas)
        {
            if (_afList != null && _afList.Count > 0)
            {
                using (SKPaint afPaint = new SKPaint
                {
                    StrokeWidth = 1,
                    StrokeJoin = SKStrokeJoin.Round,
                    Style = SKPaintStyle.StrokeAndFill,
                    IsAntialias = true
                })
                using (SKPaint textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true
                })
                using (SKFont textFont = new SKFont
                {
                    Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Normal),
                    Size = 9f
                })
                {
                    float xPerS = _innerRect.Width / _duration;
                    foreach (AfInfo af in _afList)
                    {
                        bool inGraphicRect = false;
                        float left = 0, right = 0;
                        if (af.StartTime >= StartTime && af.StartTime < EndTime)
                        {
                            inGraphicRect = true;
                            double xVal = af.StartTime.Subtract(StartTime).TotalSeconds;
                            left = _innerRect.Left + (float)(xVal * xPerS);
                            right = _innerRect.Left + Math.Min((float)(af.EndTime.Subtract(StartTime).TotalSeconds * xPerS), _innerRect.Width);
                        }
                        else if (af.EndTime >= StartTime && af.EndTime < EndTime)
                        {
                            inGraphicRect = true;
                            double xVal = Math.Max((float)af.StartTime.Subtract(StartTime).TotalSeconds, 0);
                            left = _innerRect.Left + (float)(xVal * xPerS);
                            right = _innerRect.Left + (float)(af.EndTime.Subtract(StartTime).TotalSeconds * xPerS);
                        }
                        else if (af.StartTime < StartTime && af.EndTime > EndTime)
                        {
                            inGraphicRect = true;
                            left = _innerRect.Left;
                            right = _innerRect.Right;
                        }
                        if (inGraphicRect)
                        {
                            int rectHeight = 10;
                            if (left == right)
                            {
                                rectHeight = 15;
                                afPaint.Color = SKColors.Black;
                            }
                            else
                                afPaint.Color = SKColors.LightGray;

                            SKRect rect = new SKRect(left, _innerRect.Bottom - rectHeight, right, _innerRect.Bottom - 1);
                            canvas.DrawRect(rect, afPaint);

                            float textW = textFont.MeasureText(af.TypeName, out SKRect bound, textPaint);
                            if (textW < rect.Width)
                            {
                                SKPoint textPos = new SKPoint(rect.Left + (rect.Width - textW) / 2, rect.Top + rect.Height / 2 + bound.Height / 2);
                                canvas.DrawText(af.TypeName, textPos, SKTextAlign.Left, textFont, textPaint);
                            }
                        }
                    }

                }
            }
        }

        // 仅重绘圈选路径区域
        private void redrawSelectionPathOnly(SKRect selectionRect)
        {
            // 计算圈选路径的边界区域
            if (selectionRect != SKRect.Empty)
            {
                // 添加一些边距确保路径完全可见
                Rectangle rect = Rectangle.Truncate(RectangleF.Inflate(selectionRect.ToDrawingRect(), 3, 3));

                // 仅重绘圈选路径区域
                _graphicsControl.Invalidate(rect);
            }
            else
            {
                // 如果没有路径，重绘整个控件
                _graphicsControl.Invalidate();
            }
        }

        // 获取内部波形绘制区
        private void deriveDrawSize(int width, int height)
        {
            int marginTop = 15, marginBottom = 20;
            int marginLeft = 35, marginRight = 35;
            _innerRect = new RectangleF(marginLeft, marginTop, width - marginLeft - marginRight, height - marginTop - marginBottom);

            setScalarValues();
        }

        // 根据当前绘图区计算标量
        private void setScalarValues()
        {
            float xPerS = _innerRect.Width / _duration;
            _realTimeSpan = _duration;
            _maxX = _innerRect.Right;
            if (SelectMode == 0 && AnaValues.CachedAllBeats.Count > 0)
            {
                float minSec = (float)AnaValues.CachedAllBeats[0].Sample / _holterConfig.SampleRate;
                float maxSec = AnaValues.CachedAllBeats[AnaValues.CachedAllBeats.Count - 1].Sample / _holterConfig.SampleRate;
                _realTimeSpan = maxSec;
                _maxX = _innerRect.Left + (maxSec - minSec) * xPerS;
            }
        }

        //调整选择区域使其在有效显示范围内
        private SKRect adjustSelectionRect(float left, float width, float cursorX, bool fixedWidth = false)
        {
            if (left < _innerRect.Left) left = _innerRect.Left;

            float right = left + width;
            float xPerS = _innerRect.Width / _duration;

            if (fixedWidth)
            {
                if (cursorX + width / 2 > _maxX)
                {
                    right = _maxX;
                    left = Math.Max(_innerRect.Left, _maxX - width);
                }
            }
            if (right > _maxX) right = _maxX;

            return new SKRect(left, _innerRect.Top, right, _innerRect.Bottom);
        }

        //执行选中后的动作
        private void executeSelect(object sender, bool keepSelection = false)
        {
            if (selectionRect != SKRect.Empty)
            {
                float xPerS = _innerRect.Width / _duration;
                float startS = (selectionRect.Left - _innerRect.Left) / xPerS;
                float endS = (selectionRect.Right - _innerRect.Left) / xPerS;
                selectionStartTime = StartTime.AddSeconds(startS);
                selectionEndTime = StartTime.AddSeconds(endS);
                if (selectionRect.Left == _innerRect.Left) selectionStartTime = StartTime;
                if (selectionRect.Right == _maxX) selectionEndTime = StartTime.AddSeconds(_realTimeSpan + (SelectMode == 0 ? 1 : 0)); //适当增加时长避免无法选取所有

                ConcurrentBag<BeatInfo> selectedBeats = new ConcurrentBag<BeatInfo>();
                try
                {
                    Parallel.ForEach(_pointGroups, group =>
                    {
                        foreach (var point in group.Value)
                        {
                            if (selectionRect.Contains(point.Location))
                            {
                                selectedBeats.Add(_sourceBeats[point.Index]);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("拖选过程中发生错误: " + ex.Message);
                }
                List<BeatInfo> selectedList = selectedBeats.OrderBy(o => o.Sample).ToList();
                OnRangeSelected?.Invoke(sender, selectionStartTime, selectionEndTime, selectedList, keepSelection);  // 触发选中事件
            }
            else if (SelectMode == 1)
            {
                List<BeatInfo> selectedList = _sourceBeats;
                OnRangeSelected?.Invoke(sender, StartTime, EndTime, selectedList, keepSelection);
            }
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
