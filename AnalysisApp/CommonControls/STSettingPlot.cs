using AnalysisApp.Commons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class STSettingPlot : BaseDrawingControl
    {
        private List<BeatInfo> _sourceBeats;

        private SKPaint selectionPaint;
        private SKPaint selectionInnerPaint;
        private SKPaint locatePaint;
        private SKPaint _wavePaint;
        private float selectStartX;
        private SKRect selectionRect;

        private bool _isSelecting;
        private RectangleF _innerRect;
        private float _duration; // 绘图区数据长度(s)
        private const int MAX_RR = 4000;
        private const int MIN_GAP = 10; //E、J及ST之间的最小距离(ms)
        private DateTime _lastMouseMoveTime = DateTime.MinValue;

        private int _leadIndex;
        private BeatInfo _sourceBeat;
        private short[] _leadData;
        private float _rPos; //R波位置
        private DeviationInfo _baselineDev, _jDev, _stDev;
        private List<DeviationInfo> _deviations;
        public string BaseBeatType = "";

        public event Action<int, int, int> OnDeviationAdjusted;

        public STSettingPlot()
        {
            InitializeComponent();
        }

        #region 事件
        private void STSettingPlot_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                init();

                deriveDrawSize(_graphicsControl.Width, _graphicsControl.Height);
            }
        }

        private void STSettingPlot_SizeChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (_graphicsControl != null && this.FindForm().WindowState != FormWindowState.Minimized)
                {
                    if (this.Size != _firstActualSize)
                    {
                        deriveDrawSize(_graphicsControl.Width, _graphicsControl.Height);
                        RefreshUI();
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

                DeviationInfo devInfo = findClosestDevLine(e.X);
                if (devInfo != null) devInfo.Selected = true;
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting)
            {
                DeviationInfo devInfo = findClosestDevLine(e.X);
                if (devInfo == null)
                    this.Cursor = Cursors.Arrow;
                else
                    this.Cursor = Cursors.Hand;

                return;
            }
            else
            {
                if (DateTime.Now - _lastMouseMoveTime < TimeSpan.FromMilliseconds(10)) return;
                if (_deviations.Any(d => d.Selected))
                    this.Cursor = Cursors.Hand;
                else
                    this.Cursor = Cursors.Arrow;

                _lastMouseMoveTime = DateTime.Now;
                if (Math.Abs(e.X - selectStartX) > 0)
                {
                    if (e.X < _innerRect.Left || e.X > _innerRect.Right) return;

                    float xPerS = _innerRect.Width / _duration;
                    float minGap = xPerS / 1000 * MIN_GAP;
                    if (_baselineDev.Selected)
                    {
                        _baselineDev.XCoordinate = Math.Min(e.X, _rPos - minGap);
                    }
                    else if (_jDev.Selected)
                    {
                        _jDev.XCoordinate = Math.Max(e.X, _rPos + minGap);
                        _jDev.XCoordinate = Math.Min(_stDev.XCoordinate - minGap, _jDev.XCoordinate);
                    }
                    else if (_stDev.Selected)
                    {
                        _stDev.XCoordinate = Math.Max(e.X, _jDev.XCoordinate + minGap);
                    }

                    calculateDeviation();
                    OnDeviationAdjusted?.Invoke(_baselineDev.OffsetValue, _jDev.OffsetValue, _stDev.OffsetValue);
                    _graphicsControl.Invalidate();
                }
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                if (_deviations != null)
                {
                    foreach (DeviationInfo devInfo in _deviations) { devInfo.Selected = false; }

                }
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

                drawDeviationLines(canvas);

                if (selectionRect != SKRect.Empty) canvas.DrawRect(selectionRect, selectionInnerPaint);
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

        public void SetBeatSource(BeatInfo beat, int refLead)
        {
            _sourceBeat = beat;
            _duration = (float)(beat.EndIndex - beat.StartIndex) / _holterConfig.SampleRate;
            _leadIndex = refLead;

            readWaveData();
            RefreshUI();
        }

        private void init()
        {
            _sourceBeats = new List<BeatInfo>(AnaValues.CachedAllBeats.Count);

            AnalysisSetting anaSettings = _holterConfig.AnalysisSetting;
            _baselineDev = new DeviationInfo() { Key = "Base", OffsetValue = anaSettings.BaseOffset };
            _jDev = new DeviationInfo() { Key = "J", OffsetValue = anaSettings.BaseOffset };
            _stDev = new DeviationInfo() { Key = "ST", OffsetValue = anaSettings.BaseOffset };
            _deviations = new List<DeviationInfo>() { _baselineDev, _jDev, _stDev };

            _wavePaint = new SKPaint
            {
                StrokeWidth = 1.2f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

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

            //_graphicsControl.ContextMenuStrip = this.ctxMenuStrip;
            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            this.Controls.Add(_graphicsControl);
        }

        private void readWaveData()
        {
            if (_sourceBeat != BeatInfo.Empty)
                _leadData = AnaCode.ReadWaveData(_leadIndex, _sourceBeat.StartIndex, _sourceBeat.EndIndex).WaveData;
            else
                _leadData = null;
        }

        public void RefreshUI(bool keepSelection = false)
        {
            if (DesignMode || !IsLoaded) return;

            if (!keepSelection)
            {
                selectStartX = 0;
                selectionRect = SKRect.Empty;
            }

            float xPerS = _innerRect.Width / _duration;
            float xPerSample = _innerRect.Width / (_sourceBeat.EndIndex - _sourceBeat.StartIndex);
            _rPos = _innerRect.Left + (_sourceBeat.Sample - _sourceBeat.StartIndex) * xPerSample;

            //计算界面上各偏移标线位置
            genDeviationPos();

            //重新创建离屏缓存
            createOffscreenSurface();

            _graphicsControl?.Refresh();
        }

        //计算各偏移标线位置
        private void genDeviationPos()
        {
            float xPerS = _innerRect.Width / _duration;
            _baselineDev.XCoordinate = _innerRect.Left + xPerS * ((float)(_sourceBeat.Sample - _sourceBeat.StartIndex) / _holterConfig.SampleRate - _baselineDev.OffsetValue / 1000F);
            _jDev.XCoordinate = _innerRect.Left + xPerS * ((float)(_sourceBeat.Sample - _sourceBeat.StartIndex) / _holterConfig.SampleRate + _jDev.OffsetValue / 1000F);
            _stDev.XCoordinate = _jDev.XCoordinate + xPerS * _stDev.OffsetValue / 1000F;
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
                    drawGrid(surface.Canvas, imgInfo.Width, imgInfo.Height);
                    drawWave(surface.Canvas);
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


        //绘制网格线
        private void drawGrid(SKCanvas canvas, int width, int height)
        {
            float pixMM = SysConstants.PIX_SCALE;
            using (SKPaint gridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#9FC0E0"),
                StrokeWidth = 0.3f,
                IsAntialias = true
            })
            using (SKPaint majorGridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#C1D6EB"),
                StrokeWidth = 0.6f,
                IsAntialias = true
            })
            {
                if (true) //绘制内部细网格
                {
                    // 水平1mm网格线
                    for (float y = 1; y < height - 1; y += pixMM)
                    {
                        if (Math.Abs(y % (5 * pixMM)) > 0.1f) // 跳过5mm的位置
                        {
                            canvas.DrawLine(0, y, width, y, gridPaint);
                        }
                    }

                    // 垂直1mm网格线
                    for (float x = 1; x < width - 1; x += pixMM)
                    {
                        if (Math.Abs(x % (5 * pixMM)) > 0.1f) // 跳过5mm的位置
                        {
                            canvas.DrawLine(x, 0, x, height, gridPaint);
                        }
                    }
                }

                // 水平网格线（5mm间隔）
                for (float y = 0; y < height; y += 5 * pixMM)
                {
                    canvas.DrawLine(0, y, width, y, majorGridPaint);
                }

                // 垂直网格线（5mm间隔）
                for (float x = 0; x < width; x += 5 * pixMM)
                {
                    canvas.DrawLine(x, 0, x, height, majorGridPaint);
                }
            }
        }

        //绘制波形
        private void drawWave(SKCanvas canvas)
        {
            if (_leadData == null) return;

            float xPerP = _innerRect.Width / _leadData.Length;
            float yPerP = _innerRect.Height / 4; //±2mV
            float baseY = _innerRect.Top + _innerRect.Height / 2;
            using (SKPath path = new SKPath())
            {
                //path.MoveTo(_innerRect.Left, baseY);
                for (int i = 0; i < _leadData.Length; i++)
                {
                    float x = _innerRect.Left + xPerP * i;
                    float y = baseY - yPerP * _leadData[i] / 1000;
                    if (path.PointCount == 0)
                    {
                        path.MoveTo(x, y);
                        continue;
                    }
                    if (_innerRect.Contains(x, y))
                        path.LineTo(x, y);
                    else
                        path.MoveTo(x, y);
                }
                canvas.DrawPath(path, _wavePaint);
            }
        }

        //绘制E,J与ST点偏移标线
        private void drawDeviationLines(SKCanvas canvas)
        {
            if (_leadData == null) return;

            float xPerS = _innerRect.Width / _duration;
            using (SKPaint linePaint = new SKPaint() { Style = SKPaintStyle.Stroke, StrokeWidth = 1 })
            using (SKPaint textPaint = new SKPaint() { Color = SKColors.Black, IsAntialias = true })
            using (SKFont textFont = new SKFont
            {
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold),
                Size = 10.5f
            })
            {
                //基线点偏移
                linePaint.Color = SKColors.Red;
                float baseOffset = _baselineDev.XCoordinate + linePaint.StrokeWidth;
                canvas.DrawLine(baseOffset, _innerRect.Top, baseOffset, _innerRect.Bottom, linePaint);
                canvas.DrawText("E", baseOffset, _innerRect.Bottom + 10, SKTextAlign.Center, textFont, textPaint);

                //J点偏移
                linePaint.Color = SKColors.Green;
                float jOffset = _jDev.XCoordinate + linePaint.StrokeWidth;
                canvas.DrawLine(jOffset, _innerRect.Top, jOffset, _innerRect.Bottom, linePaint);
                canvas.DrawText("J", jOffset, _innerRect.Bottom + 10, SKTextAlign.Center, textFont, textPaint);

                //基线点偏移
                linePaint.Color = SKColors.Green;
                float stOffset = _stDev.XCoordinate + linePaint.StrokeWidth;
                canvas.DrawLine(stOffset, _innerRect.Top, stOffset, _innerRect.Bottom, linePaint);
                canvas.DrawText("ST", stOffset, _innerRect.Bottom + 10, SKTextAlign.Center, textFont, textPaint);
            }
        }

        // 获取内部波形绘制区
        private void deriveDrawSize(int width, int height)
        {
            int marginTop = 15, marginBottom = 20;
            int marginLeft = 35, marginRight = 35;
            _innerRect = new RectangleF(marginLeft, marginTop, width - marginLeft - marginRight, height - marginTop - marginBottom);
        }

        //调整选择区域使其在有效显示范围内
        private SKRect adjustSelectionRect(float left, float width, float cursorX, bool forDefaultRect = false)
        {
            if (left < _innerRect.Left) left = _innerRect.Left;

            float xPerS = _innerRect.Width / _duration;
            float minSec = (float)_sourceBeats[0].Sample / _holterConfig.SampleRate;
            float maxSec = _sourceBeats[_sourceBeats.Count - 1].Sample / _holterConfig.SampleRate;
            float maxX = _innerRect.Left + (maxSec - minSec) * xPerS;

            float right = left + width;
            if (forDefaultRect)
            {
                if (cursorX + width / 2 > maxX) left = maxX - width;
                right = left + width;
            }
            else
            {
                right = cursorX;
            }

            if (right > maxX) right = maxX;

            return new SKRect(left, _innerRect.Top, right, _innerRect.Bottom);
        }

        //获取与光标接近的偏移标线
        private DeviationInfo findClosestDevLine(int cursorX)
        {
            if (_deviations != null)
            {
                float dist = 0;
                DeviationInfo closestDev = null;
                foreach (DeviationInfo devInfo in _deviations)
                {
                    float dist1 = Math.Abs(devInfo.XCoordinate - cursorX);
                    if (dist1 <= 3)
                    {
                        if (closestDev == null) closestDev = devInfo;
                        if (dist1 < dist)
                        {
                            dist = dist1;
                            closestDev = devInfo;
                        }
                    }
                }
                return closestDev;
            }
            return null;
        }

        private void calculateDeviation()
        {
            float xPerS = _innerRect.Width / _duration;
            _baselineDev.OffsetValue = (int)((_rPos - _baselineDev.XCoordinate) / xPerS * 1000);
            _jDev.OffsetValue = (int)((_jDev.XCoordinate - _rPos) / xPerS * 1000);
            _stDev.OffsetValue = (int)((_stDev.XCoordinate - _jDev.XCoordinate) / xPerS * 1000);
        }

        //基线点、J点或ST点偏移信息
        private class DeviationInfo
        {
            internal string Key { get; set; }
            //横坐标
            internal float XCoordinate { get; set; }
            //偏移量(ms)
            internal int OffsetValue { get; set; }
            //是否被鼠标选中
            internal bool Selected { get; set; }
        }
    }
}
