using AnalysisApp.Commons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Utilities;

namespace AnalysisApp.CommonControls
{
    public partial class BpWaveChart : BaseDrawingControl
    {
        private int _maxScaleX = 10; //X轴最大刻度值(s)
        private int _maxScaleY = 220; //Y轴最大压力值(mmHg)
        private float _perWidth = 1;
        private string _bpUnit = "mmHg";
        private BPDataInfo _bpDataRecord;
        private PulseWaveData _waveData;
        private DataPoint _sbpPos, _dbpPos, _mapPos;
        private SKPaint rawPaint, simPaint, bpPaint;

        private List<SKPoint> _rawWavePoints;
        private List<SKPoint> _fittingWavePoints;
        private List<SKPoint> _bpWavePoints;
        private RectangleF _innerRect;
        private bool _isSelecting;

        private static readonly string[] Editable8337 = { "iE6S", "iE6T", "iE6Pro", "iE7S", "iE7T", "iE7Pro", "iE75" };
        private const int DIVIDE_COUNT_X = 10, DIVIDE_COUNT_Y = 10;
        private const int BASE_Y = 20; //y轴最小值
        private const int SAMPLE_RATE = 500;

        public BPDataInfo BpDataRecord
        {
            get => _bpDataRecord;
            set
            {
                _bpDataRecord = value;
                this.btnUpdate.Enabled = false;
                if (AnaValues.CurrentCase.deviceType == "4" || Editable8337.Contains(AnaValues.CurrentCase.deviceModel))
                {
                    this.btnUpdate.Enabled = true;
                }

                if (_bpDataRecord != null)
                {
                    this.btnUpdate.Visible = true;
                    if (_bpDataRecord.SBP == 0 || _bpDataRecord.DBP == 0 || _bpDataRecord.MAP == 0)
                    {
                        this.btnUpdate.Visible = false;
                    }
                }
            }
        }

        public event Action<int, int, int> OnEnsureDataUpdate;

        public BpWaveChart()
        {
            InitializeComponent();
        }

        #region 事件
        private void BpWaveChart_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                try
                {
                    init();
                    deriveDrawSize(_graphicsControl.Width, _graphicsControl.Height);
                    if (BpDataRecord != null)
                    {
                        LoadWave();
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "血压数据分析");
                }
            }
        }

        private void BpWaveChart_SizeChanged(object sender, EventArgs e)
        {
            if (this.FindForm() != null && this.FindForm().WindowState != FormWindowState.Minimized)
            {
                if (this.Size != _firstActualSize)
                {
                    if (_graphicsControl != null)
                    {
                        deriveDrawSize(_graphicsControl.Width, _graphicsControl.Height);
                        _graphicsControl.Invalidate();
                    }
                    _firstActualSize = this.Size;
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                int sbp = getBpValueOfPoint(_sbpPos.Location.Y);
                int dbp = getBpValueOfPoint(_dbpPos.Location.Y);
                int map = getBpValueOfPoint(_mapPos.Location.Y);
                OnEnsureDataUpdate?.Invoke(sbp, dbp, map);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压数据分析");
            }
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                resetSelectState();
                SKPoint curPoint = new SKPoint(e.X, e.Y);
                if (_sbpPos != null && SKPoint.Distance(curPoint, _sbpPos.Location) < 5)
                {
                    _sbpPos.IsSelected = true;
                    _isSelecting = true;
                }
                else if (_dbpPos != null && SKPoint.Distance(curPoint, _dbpPos.Location) < 5)
                {
                    _dbpPos.IsSelected = true;
                    _isSelecting = true;
                }
                else if (_mapPos != null && SKPoint.Distance(curPoint, _mapPos.Location) < 5)
                {
                    _mapPos.IsSelected = true;
                    _isSelecting = true;
                }
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_graphicsControl == null) return;

            SKPoint curPoint = new SKPoint(e.X, e.Y);
            if (_sbpPos != null && SKPoint.Distance(curPoint, _sbpPos.Location) < 5)
            {
                _graphicsControl.Cursor = Cursors.Hand;
            }
            else if (_dbpPos != null && SKPoint.Distance(curPoint, _dbpPos.Location) < 5)
            {
                _graphicsControl.Cursor = Cursors.Hand;
            }
            else if (_mapPos != null && SKPoint.Distance(curPoint, _mapPos.Location) < 5)
            {
                _graphicsControl.Cursor = Cursors.Hand;
            }
            else { _graphicsControl.Cursor = Cursors.Default; }

            if (_isSelecting)
            {
                Action<DataPoint> redrawPoint = (dataPos) =>
                {
                    for (int i = 0; i < _fittingWavePoints.Count; i++)
                    {
                        if (Math.Abs(e.X - _fittingWavePoints[i].X) <= 1)
                        {
                            dataPos.Location = new SKPoint(e.X, _fittingWavePoints[i].Y);
                            _graphicsControl?.Invalidate();
                            break;
                        }
                    }
                };

                if (_sbpPos != null && _sbpPos.IsSelected)
                {
                    redrawPoint(_sbpPos);
                }
                else if (_dbpPos != null && _dbpPos.IsSelected)
                {
                    redrawPoint(_dbpPos);
                }
                else if (_mapPos != null && _mapPos.IsSelected)
                {
                    redrawPoint(_mapPos);
                }
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;

                _graphicsControl.Invalidate();
            }
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            try
            {
                canvas.Clear(SKColors.White);
                drawCoordinateSystem(canvas, imageWidth, imageHeight);
                drawWaves(canvas);
                drawDataPoints(canvas);
            }
            catch (AccessViolationException)
            {
                // GPU内存访问冲突，记录但不抛出异常
                Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("绘制对象已释放，已安全处理");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPaintSurface错误: {ex.GetType().Name}: {ex.Message}");
            }
        }
        #endregion


        // 获取内部波形绘制区
        private void deriveDrawSize(int width, int height)
        {
            int marginTop = 30, marginBottom = 20;
            int marginLeft = 35, marginRight = 35;
            _innerRect = new RectangleF(marginLeft, marginTop, width - marginLeft - marginRight, height - marginTop - marginBottom);
        }

        public void LoadWave()
        {
            if (BpDataRecord == null) return;

            _bpUnit = CommonValues.SystemSettings.bpSets.bpDiagnoseSets.unit;
            resetData();

            if (!Directory.Exists(AnaValues.RootPath)) return;

            string nibpIniPath = Path.Combine(AnaValues.RootPath, "nibp.ini");
            var nibpINI = INIFile.ReadIni(nibpIniPath);
            bool hasSwitch = nibpINI.TryGetValue("Pulsewave", out string pulseWaveSwitch);
            if (!hasSwitch || pulseWaveSwitch == "Off")
            {
                //脉搏波开关关闭，不存在脉搏波数据
                return;
            }

            string waveDataFile = string.Empty;
            DirectoryInfo dirInfo = new DirectoryInfo(AnaValues.RootPath);
            foreach (FileInfo fi in dirInfo.GetFiles("nibp-*.dat"))
            {
                string timeString = fi.Name.Substring("nibp-".Length, 14);
                bool timeParsed = DateTime.TryParseExact(timeString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime fileTime);
                if (timeParsed && fileTime == BpDataRecord.bpDateTime)
                {
                    waveDataFile = fi.FullName;
                    break;
                };
            }

            if (!string.IsNullOrEmpty(waveDataFile))
            {
                _waveData = CalcBPInfo.GetPulseWaveData(waveDataFile, BpDataRecord.Pulse);
                if (_waveData != null)
                {
                    _maxScaleX = (int)Math.Ceiling((double)_waveData.ac.Length / (SAMPLE_RATE * 10)) * 10;
                    _perWidth = _innerRect.Width / _maxScaleX / SAMPLE_RATE;
                    _maxScaleY = BASE_Y + (int)(Math.Ceiling(_waveData.stepDc.Max() / 20) * 20);

                    genWavePoints();
                    genBpDataMarkPosition();
                }
            }
            _graphicsControl?.Invalidate();
        }

        private void init()
        {
            // 初始化圈选/框选画笔
            rawPaint = new SKPaint()
            {
                Color = new SKColor(170, 170, 170),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1F,
                IsAntialias = true
            };
            simPaint = new SKPaint()
            {
                Color = new SKColor(255, 176, 0),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2F,
                IsAntialias = true
            };
            bpPaint = new SKPaint()
            {
                Color = new SKColor(136, 162, 242),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5F,
                IsAntialias = true
            };

            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            this.panel1.Controls.Add(_graphicsControl);
        }

        private void genWavePoints()
        {
            //1.原始压力曲线
            float yPerS = _innerRect.Height / (_maxScaleY - BASE_Y);
            _rawWavePoints = new List<SKPoint>(_waveData.dc.Length);
            for (int i = 0; i < _waveData.dc.Length; i++)
            {
                _rawWavePoints.Add(new SKPoint(_innerRect.Left + _perWidth * i, _innerRect.Bottom - (float)(_waveData.dc[i] - BASE_Y) * yPerS));
            }

            //2.拟合压力曲线
            int stepCount = _waveData.stepDc.Length;
            int startIndex = _waveData.stepDcLen[0] / 2, endIndex = _waveData.dc.Length - _waveData.stepDcLen[stepCount - 1] / 2;
            int stepIndex = 0;
            int stepStart = 0; //台阶数据的开始序号(对所有数据)
            int lastMidStep = 0; //上一台阶的中点序号
            _fittingWavePoints = new List<SKPoint>(_waveData.dc.Length);
            for (int i = 0; i < _waveData.dc.Length; i++)
            {
                if (i >= startIndex && i <= endIndex)
                {
                    int halfStepLen = _waveData.stepDcLen[stepIndex] / 2;
                    if (i == stepStart + halfStepLen)
                    {
                        float x = _innerRect.Left + i * _perWidth;
                        float y = _innerRect.Bottom - (float)(_waveData.stepDc[stepIndex] - BASE_Y) * yPerS;
                        _fittingWavePoints.Add(new SKPoint(x, y));
                        //填充相邻两个台阶中点之间的拟合数据序列
                        if (stepIndex > 0)
                        {
                            Random rand = new Random();
                            double smaller = _waveData.stepDc[stepIndex], larger = _waveData.stepDc[stepIndex - 1];
                            if (_waveData.stepDc[stepIndex - 1] < _waveData.stepDc[stepIndex])
                            {
                                smaller = _waveData.stepDc[stepIndex - 1];
                                larger = _waveData.stepDc[stepIndex];
                            }

                            int fillCount = i - lastMidStep - 1;
                            double increment = (larger - smaller) / fillCount;
                            int n = 0;
                            for (int j = lastMidStep + 1; j < i; j++)
                            {
                                double simValue = larger - increment * n + increment * rand.Next(-1, 1);
                                float y1 = _innerRect.Bottom - (float)(simValue - BASE_Y) * yPerS;
                                _fittingWavePoints.Add(new SKPoint(_innerRect.Left + j * _perWidth, y1));
                                n++;
                            }
                        }
                        lastMidStep = i;

                        if (stepIndex < stepCount - 1)
                        {
                            stepStart = stepStart + _waveData.stepDcLen[stepIndex];
                            stepIndex++;
                        }
                    }
                }
            }
            _fittingWavePoints.Sort(new Comparison<SKPoint>((point1, point2) => point1.X.CompareTo(point2.X)));

            //3.血压波形
            double maxBpAmp = _waveData.ac.Max(d => Math.Abs(d));
            yPerS = (float)(_innerRect.Height / (maxBpAmp * 4));
            float middleY = _innerRect.Top + _innerRect.Height / 2;
            _bpWavePoints = new List<SKPoint>(_waveData.ac.Length);
            for (int i = 0; i < _waveData.ac.Length; i++)
            {
                _bpWavePoints.Add(new SKPoint(_innerRect.Left + _perWidth * i, middleY - (float)(_waveData.ac[i]) * yPerS));
            }
        }

        private void genBpDataMarkPosition()
        {
            float yPerS = _innerRect.Height / (_maxScaleY - BASE_Y);
            float sbpY = _innerRect.Bottom - (float)(BpDataRecord.SBP - BASE_Y) * yPerS;
            float dbpY = _innerRect.Bottom - (float)(BpDataRecord.DBP - BASE_Y) * yPerS;
            float mapY = _innerRect.Bottom - (float)(BpDataRecord.MAP - BASE_Y) * yPerS;
            for (int i = 0; i < _fittingWavePoints.Count; i++)
            {
                if (Math.Abs(sbpY - _fittingWavePoints[i].Y) < 1)
                {
                    if (_sbpPos == null) _sbpPos = new DataPoint(_fittingWavePoints[i]);
                }
                if (Math.Abs(dbpY - _fittingWavePoints[i].Y) < 1)
                {
                    if (_dbpPos == null) _dbpPos = new DataPoint(_fittingWavePoints[i]);
                }
                if (Math.Abs(mapY - _fittingWavePoints[i].Y) < 1)
                {
                    if (_mapPos == null) _mapPos = new DataPoint(_fittingWavePoints[i]);
                }
            }
        }

        //绘制坐标系(包含内部网格线)
        private void drawCoordinateSystem(SKCanvas canvas, int width, int height)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            {
                using (SKPaint axisPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 1,
                    IsAntialias = true
                })
                using (SKFont textFont = new SKFont
                {
                    Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold),
                    Size = 12f
                })
                {
                    float x0 = _innerRect.Left;
                    float y0 = _innerRect.Bottom;

                    float intervalPx = _innerRect.Width / DIVIDE_COUNT_X, intervalPy = _innerRect.Height / DIVIDE_COUNT_Y;
                    int intervalX = _maxScaleX / DIVIDE_COUNT_X; //2s
                    int intervalY = (int)((double)(_maxScaleY - BASE_Y) / DIVIDE_COUNT_Y);

                    // 绘制X轴
                    canvas.DrawLine(x0, y0, x0 + _innerRect.Width, y0, axisPaint);
                    canvas.DrawLine(x0, _innerRect.Top, x0 + _innerRect.Width, _innerRect.Top, axisPaint);
                    for (int i = 0; i <= DIVIDE_COUNT_X; i++)
                    {
                        //刻度线
                        canvas.DrawLine(x0 + i * intervalPx, y0, x0 + i * intervalPx, y0 + 5, axisPaint);
                        //刻度值
                        canvas.DrawText((i * intervalX).ToString(), x0 + i * intervalPx, y0 + 15, SKTextAlign.Center, textFont, textPaint);
                    }
                    //x轴单位
                    canvas.DrawText("s", _innerRect.Right + 15, y0 + 15, SKTextAlign.Center, textFont, textPaint);

                    // 绘制Y轴
                    canvas.DrawLine(x0, y0, x0, y0 - _innerRect.Height, axisPaint);
                    canvas.DrawLine(_innerRect.Right, y0, _innerRect.Right, y0 - _innerRect.Height, axisPaint);
                    for (int i = 0; i <= DIVIDE_COUNT_Y; i++)
                    {
                        canvas.DrawLine(x0, y0 - i * intervalPy, x0 - 5, y0 - i * intervalPy, axisPaint);

                        string yMarkText = (BASE_Y + i * intervalY).ToString();
                        if (_bpUnit == "kPa")
                        {
                            yMarkText = CommonFuns.mmHg2KPa(BASE_Y + i * intervalY, 1).ToString();
                            //intervalY = (int)CommonFuns.mmHg2KPa(intervalY);
                            //baseValueY = (int)CommonFuns.mmHg2KPa(baseValueY);
                        }
                        canvas.DrawText(yMarkText, x0 - 8, y0 - i * intervalPy + 5, SKTextAlign.Right, textFont, textPaint);
                    }
                    canvas.DrawText(_langs[_bpUnit], x0 - 5, _innerRect.Top - 12, SKTextAlign.Center, textFont, textPaint);
                }

                //绘制图例(自右向左)
                using (SKPaint dashPaint = new SKPaint
                {
                    Color = new SKColor(252, 128, 128),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2f
                })
                using (SKFont textFont = new SKFont
                {
                    Typeface = SKTypeface.FromFamilyName("微软雅黑"),
                    Size = 12f
                })
                {
                    dashPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0);
                    float legendY = _innerRect.Top - 15;
                    float lineWidth = 30;
                    float xPos = _innerRect.Right;
                    float textWidth = textFont.MeasureText(_langs["舒张压"], textPaint);

                    //textFont.Typeface = SKTypeface.FromFamilyName(textFont.Typeface.FamilyName);
                    canvas.DrawLine(xPos, legendY, xPos - lineWidth, legendY, dashPaint);
                    canvas.DrawText(_langs["舒张压"], xPos - lineWidth - 5, legendY + 5, SKTextAlign.Right, textFont, textPaint);

                    xPos = xPos - lineWidth - textWidth - 15;
                    textWidth = textFont.MeasureText(_langs["平均动脉压"], textPaint);
                    dashPaint.Color = new SKColor(255, 177, 76);
                    canvas.DrawLine(xPos, legendY, xPos - lineWidth, legendY, dashPaint);
                    canvas.DrawText(_langs["平均动脉压"], xPos - lineWidth - 5, legendY + 5, SKTextAlign.Right, textFont, textPaint);

                    xPos = xPos - lineWidth - textWidth - 15;
                    textWidth = textFont.MeasureText(_langs["收缩压"], textPaint);
                    dashPaint.Color = new SKColor(84, 112, 198);
                    canvas.DrawLine(xPos, legendY, xPos - lineWidth, legendY, dashPaint);
                    canvas.DrawText(_langs["收缩压"], xPos - lineWidth - 5, legendY + 5, SKTextAlign.Right, textFont, textPaint);
                }

                //绘制波形颜色标识
                using (SKPaint linePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2f
                })
                using (SKFont textFont = new SKFont
                {
                    Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold),
                    Size = 12f
                })
                {
                    float xPos = _innerRect.Right - 6;
                    float legendY = _innerRect.Top + 15;
                    float lineWidth = 30;
                    linePaint.Color = rawPaint.Color;
                    //textFont.Typeface = SKTypeface.FromFamilyName(textFont.Typeface.FamilyName);
                    canvas.DrawLine(xPos, legendY, xPos - lineWidth, legendY, linePaint);
                    canvas.DrawText(_langs["原始压力曲线"], xPos - lineWidth - 5, legendY + 5, SKTextAlign.Right, textFont, textPaint);

                    legendY = legendY + 20;
                    linePaint.Color = simPaint.Color;
                    canvas.DrawLine(xPos, legendY, xPos - lineWidth, legendY, linePaint);
                    canvas.DrawText(_langs["拟合压力曲线"], xPos - lineWidth - 5, legendY + 5, SKTextAlign.Right, textFont, textPaint);

                    legendY = legendY + 20;
                    linePaint.Color = bpPaint.Color;
                    canvas.DrawLine(xPos, legendY, xPos - lineWidth, legendY, linePaint);
                    canvas.DrawText(_langs["血压波形曲线"], xPos - lineWidth - 5, legendY + 5, SKTextAlign.Right, textFont, textPaint);
                }
            }
        }

        //绘制波形
        private void drawWaves(SKCanvas canvas)
        {
            if (_waveData == null) return;

            using (SKPath path1 = new SKPath())
            {
                path1.MoveTo(_rawWavePoints[0]);
                for (int i = 0; i < _rawWavePoints.Count; i++)
                {
                    path1.LineTo(_rawWavePoints[i]);
                }
                canvas.DrawPath(path1, rawPaint);
            }

            using (SKPath path2 = new SKPath())
            {
                path2.MoveTo(_fittingWavePoints[0]);
                for (int i = 0; i < _fittingWavePoints.Count; i++)
                {
                    path2.LineTo(_fittingWavePoints[i]);
                }
                canvas.DrawPath(path2, simPaint);
            }

            using (SKPath path3 = new SKPath())
            {
                path3.MoveTo(_bpWavePoints[0]);
                for (int i = 0; i < _bpWavePoints.Count; i++)
                {
                    path3.LineTo(_bpWavePoints[i]);
                }
                canvas.DrawPath(path3, bpPaint);
            }
        }

        //绘制收缩压、舒张压、平均动脉压数据点
        private void drawDataPoints(SKCanvas canvas)
        {
            if (_fittingWavePoints != null)
            {
                Action<SKPaint, SKPaint, SKPoint> drawPoint = (pointPaint, dashPaint, pos) =>
                {
                    canvas.DrawPoint(pos, pointPaint);
                    canvas.DrawLine(pos.X, _innerRect.Top, pos.X, _innerRect.Bottom, dashPaint);
                    canvas.DrawLine(_innerRect.Left, pos.Y, pos.X, pos.Y, dashPaint);

                    int bpValue = getBpValueOfPoint(pos.Y);
                    string valueString = bpValue.ToString();
                    if (_bpUnit == "kPa") { valueString = CommonFuns.mmHg2KPa(bpValue, 2).ToString(); }
                    ZqDrawer.DrawValueLabel(canvas, pos, valueString, new SKPoint(15, -30), pointPaint.Color);
                };

                using (SKPaint pointPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeWidth = 10F,
                    IsAntialias = true
                })
                using (SKPaint dashPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.0f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
                })
                {
                    pointPaint.Color = new SKColor(84, 112, 198);
                    dashPaint.Color = pointPaint.Color;
                    drawPoint(pointPaint, dashPaint, _sbpPos.Location);

                    pointPaint.Color = new SKColor(252, 128, 128);
                    dashPaint.Color = pointPaint.Color;
                    drawPoint(pointPaint, dashPaint, _dbpPos.Location);

                    pointPaint.Color = new SKColor(255, 177, 76);
                    dashPaint.Color = pointPaint.Color;
                    drawPoint(pointPaint, dashPaint, _mapPos.Location);
                }
            }
        }

        //获取纵坐标对应的血压值(mmHg)
        private int getBpValueOfPoint(float y)
        {
            float yPerS = _innerRect.Height / (_maxScaleY - BASE_Y);
            float ps = BASE_Y + (_innerRect.Bottom - y) / yPerS;
            return (int)Math.Round(ps, MidpointRounding.AwayFromZero);
        }

        //重置界面渲染相关数据
        private void resetData()
        {
            _waveData = null;
            _rawWavePoints = null;
            _fittingWavePoints = null;
            _bpWavePoints = null;
            _sbpPos = null;
            _dbpPos = null;
            _mapPos = null;
        }

        //重置血压数据标识选中状态
        private void resetSelectState()
        {
            if (_sbpPos != null) _sbpPos.IsSelected = false;
            if (_dbpPos != null) _dbpPos.IsSelected = false;
            if (_mapPos != null) _mapPos.IsSelected = false;
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

        private class DataPoint
        {
            internal bool IsSelected { get; set; }
            //internal int Value => getBpValueOfPoint(Location.Y);
            internal SKPoint Location { get; set; }
            public DataPoint(SKPoint pos)
            {
                Location = pos;
            }
        }

    }
}
