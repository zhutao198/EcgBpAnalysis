using AnalysisApp.Commons;
using SkiaSharp;
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
    public partial class ScatterPlot : BaseDrawingControl
    {
        private bool _hideToolStrip;
        private bool _showColorMark;
        private int _selectMode = 0;    //选框类型
        private int _scatterType = 0;   //散点图类型(0:RR间期, 1:RR间期差值)
        private bool _isochrone = false; //是否显示等心率线
        private int _maxScale = 2000; //最大RR间期值
        private List<BeatInfo> _sourceBeats;

        private bool _isScatterTypeBased = false; //是否基于心搏类型检索数据(全时散点图)
        private bool _isScatterPlotValid = false;   //整个散点图区域是否无效
        private bool _isBeatsValid = false;         //散点图中心搏是否无效
        private Dictionary<char, List<InnerPoint>> _pointGroups;
        private SKPaint selectionPaint;
        private SKPaint selectionInnerPaint;
        private SKPaint markPaint;
        private List<SKPoint> selectionPath;
        private List<SKPoint> markPositions;
        //private List<BeatInfo> selectedBeats;
        private bool _isSelecting;

        private Dictionary<string, Image> _toolImages;
        private const int MAX_RR = 4000;

        public string BaseBeatType = "";
        public bool _bTemplateOverlay = false;

        public event Action<int> OnSelectModeChanged;
        public event Action<object, List<BeatInfo>, bool> OnBeatSelected;


        [DefaultValue(false), Category("自定义属性"), Description("是否隐藏工具条"), Browsable(true)]
        public bool HideToolStrip
        {
            get => _hideToolStrip;
            set
            {
                _hideToolStrip = value;
                this.tlpTool.Visible = !_hideToolStrip;
                if (_hideToolStrip)
                    this.Width = this.panel2.Width;
                else
                    this.Width = this.panel2.Width + this.tlpTool.Width;
            }
        }

        public ScatterPlot()
        {
            InitializeComponent();

            _sourceBeats = new List<BeatInfo>();
            _pointGroups = new Dictionary<char, List<InnerPoint>>();
            selectionPath = new List<SKPoint>();
        }

        #region 事件
        private void ScatterPlot_Load(object sender, EventArgs e)
        {
            init();

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
            this.btnScatterType.Click += (o, args) =>
            {
                _scatterType ^= 1;
                if (_scatterType == 0)
                    _maxScale = 2000;
                else
                    _maxScale = 1000;

                //this.btnScatterType.Image = this.imageList1.Images[_scatterType + 2];
                if (_scatterType == 0)
                    this.btnScatterType.Image = _toolImages["scatterNormal"];
                else
                    this.btnScatterType.Image = _toolImages["scatterDiff"];

                this.btnHrLine.Enabled = (_scatterType != 1);

                RefreshUI(o);
            };
            this.btnHrLine.Click += (o, args) =>
            {
                _isochrone = !_isochrone;
                createOffscreenSurface();
                _graphicsControl.Invalidate();
            };
        }

        private void ScatterPlot_SizeChanged(object sender, EventArgs e)
        {
            if (this.FindForm() != null && this.FindForm().WindowState != FormWindowState.Minimized)
            {
                if (this.Size != _firstActualSize)
                {
                    int h = this.panel2.Height;
                    if (this.panel2.Width != h)
                    {
                        this.panel2.Width = this.panel2.Height;
                        this.HideToolStrip = this.HideToolStrip; //使自动调整整个控件宽度
                        if (_graphicsControl != null)
                        {
                            RefreshUI(sender);
                        }
                    }
                    _firstActualSize = this.Size;
                }
            }
        }

        private void mnuBeatModify_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (_selectedBeats != null && _selectedBeats.Count > 0)
            {
                char newType = (char)e.ClickedItem.Tag;

                //foreach(int i in selectedBeatIndices)
                //{
                //    BeatInfo beatInfo = _sourceBeats[i];
                //    beatInfo.SetBeatType(newType);
                //    _sourceBeats[i] = beatInfo;
                //}
                int[] samples = _selectedBeats?.Select(o => o.Sample).ToArray();
                ModifyBeatType(samples, newType);
            }
        }

        private void mnuBeatDelete_Click(object sender, EventArgs e)
        {
            if (_selectedBeats != null && _selectedBeats.Count > 0)
            {
                int[] samples = _selectedBeats.Select(b => b.Sample).ToArray();
                int ret = AEcgHandler.DeleteBeats(samples);
                if (ret == 0)
                {
                    if (_isScatterTypeBased == true)
                    {
                        UpdateBeatSource(sender, BaseBeatType, null);
                        ChangeDataSource(samples);
                    }
                }
            }
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            int scale = 0;
            if (e.Delta > 0) scale = 1;
            else if (e.Delta < 0) scale = -1;
            if (_scatterType == 0)
            {
                _maxScale = _maxScale - 500 * scale;
                if (_maxScale < 1000) _maxScale = 1000;
                if (_maxScale > MAX_RR) _maxScale = MAX_RR;
            }
            else
            {
                _maxScale = _maxScale - 250 * scale;
                if (_maxScale < 500) _maxScale = 500;
                if (_maxScale > 2000) _maxScale = 2000;
            }
            RefreshUI(sender);
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;

                selectionPath.Clear();
                markPositions?.Clear();
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
                if (_selectMode == 0)
                {
                    if (selectionPath.Count == 0 ||
                    SKPoint.Distance(selectionPath[selectionPath.Count - 1], newPoint) > 3.0f)
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

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                _selectedBeats = null;

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
            RestoreOriginalView();
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            try
            {
                // 检查是否是部分重绘（仅重绘圈选路径区域）
                bool isPartialRedraw = imageWidth < _graphicsControl.Width || imageWidth < _graphicsControl.Height;
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

                //绘制选择路径
                if (_selectMode == 0)
                    drawPolygon(canvas);
                else
                    drawRectangle(canvas);

                //绘制心拍列表所选心拍的位置标注
                if (markPositions != null)
                {
                    foreach (SKPoint pos in markPositions)
                    {
                        canvas.DrawCircle(pos, 4, markPaint);
                    }
                }
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
                // 重新创建surface以恢复正常状态
                createOffscreenSurface();
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


        /// <summary>
        /// 设置要显示到散点图中的散点类型
        /// </summary>
        public void SetBeatType(string beatType)
        {
            _isScatterTypeBased = true;
            BaseBeatType = beatType;
            if (!DesignMode)
            {
                //List<BeatInfo> sourceBeats = AnaCode.GetBeatsWithType(AnaValues.AllValidBeats, beatType);
                List<BeatInfo> sourceBeats = AnaCode.GetBeatsWithType(AnaValues.CachedAllBeats, beatType);
                SetBeatSource(sourceBeats);
            }
        }

        public void SetBeatSource(List<BeatInfo> beats)
        {
            //List<BeatInfo> srcBeats = beats.FindAll(b => b.RRLatter > 0);
            //_sourceBeats = srcBeats;
            _sourceBeats = beats;

            RefreshUI(null);
        }

        /// <summary>
        /// 更新当前组件的数据源
        /// </summary>
        public void UpdateBeatSource(object sender, string beatType, List<BeatInfo> newBeats = null)
        {
            List<BeatInfo> srcBeats = newBeats;
            if (newBeats == null)
            {
                //srcBeats = AnaCode.GetBeatsWithType(AnaValues.AllValidBeats, beatType);
                srcBeats = AnaCode.GetBeatsWithType(AnaValues.CachedAllBeats, beatType);
            }

            _sourceBeats = srcBeats.FindAll(b => b.RRLatter > 0);
            RefreshUI(sender, true);
        }

        protected override int ModifyBeatType(int[] samples, char beatUiType)
        {
            int ret = base.ModifyBeatType(samples, beatUiType);
            if (ret == 0)
            {
                if (_isScatterTypeBased == true)
                {
                    if (_bTemplateOverlay)
                    {
                        List<BeatInfo> newBeats = _sourceBeats.FindAll(b => !samples.Contains(b.Sample));
                        UpdateBeatSource(null, BaseBeatType, newBeats);
                    }
                    else
                    {
                        UpdateBeatSource(null, BaseBeatType, null);
                    }
                }
            }
            else
            {
                CommonFuns.PromptError("操作失败");
            }

            return ret;
        }

        //刷新显示
        public void RefreshUI(object sender, bool keepSelection = false)
        {
            if (!keepSelection)
            {
                selectionPath.Clear();
                markPositions?.Clear();
            }

            //生成界面散点集
            genBeatPoints();

            //重新创建离屏缓存
            createOffscreenSurface();

            _graphicsControl?.Invalidate();

            if (_sourceBeats != null)
            {
                if (keepSelection)
                {
                    executeSelect(sender, true);
                }
                else
                {
                    if (_bTemplateOverlay)
                    {
                        OnBeatSelected?.Invoke(sender, _sourceBeats, false);
                    }
                    else
                    {
                        OnBeatSelected?.Invoke(sender, null, false);
                    }
                }
            }
        }

        public void DataSourceChanged(int[] samples)
        {
            List<BeatInfo> newList = _sourceBeats.FindAll(b => !samples.Contains(b.Sample));
            UpdateBeatSource(null, "", newList);
        }

        //标记心拍列表内所选心搏在散点图中的位置
        public void MarkPositionOfBeats(List<BeatInfo> beats)
        {
            markPositions = new List<SKPoint>(beats.Count);

            foreach (BeatInfo beat in beats)
            {
                SKPoint pos = getBeatPosition(beat);
                markPositions.Add(pos);
            }
            _graphicsControl.Invalidate();
        }

        //恢复到分时散点图视图
        public void RestoreOriginalView()
        {
            List<ScatterPlotSplit> scatters = CommonFuns.RetriveSubControls<ScatterPlotSplit>(this.Parent);
            if (scatters.Count > 0)
            {
                this.Parent.SuspendLayout();
                scatters[0].Width = this.Width;
                scatters[0].Visible = true;
                this.Width = 0;
                this.Hide();
                this.Parent.ResumeLayout();
            }
        }

        //隐藏右键菜单
        public void HideContextMenuStrip()
        {
            this.panel2.ContextMenuStrip = null;
        }

        private void init()
        {
            _toolImages = new Dictionary<string, Image>()
            {
                { "polyline", CommonFuns.GetResizedImage(Properties.Resources.polyline, 16, 16)},
                { "rect", CommonFuns.GetResizedImage(Properties.Resources.rect, 16, 16)},
                { "scatterNormal", CommonFuns.GetResizedImage(Properties.Resources.scatterNormal, 16, 16)},
                { "scatterDiff", CommonFuns.GetResizedImage(Properties.Resources.scatterDiff, 16, 16)},
                { "hrLine", CommonFuns.GetResizedImage(Properties.Resources.hrLine, 16, 16)}
            };
            //this.picSelect.Image = this.imageList1.Images["polyline.png"];
            //this.picScatterType.Image = this.imageList1.Images["scatterNormal.png"];
            //this.picHrLine.Image = this.imageList1.Images["hrLine.png"];
            this.btnSelect.Image = _toolImages["polyline"];
            this.btnScatterType.Image = _toolImages["scatterNormal"];
            this.btnHrLine.Image = _toolImages["hrLine"];
            this.toolTipScatter.SetToolTip(btnSelect, _langs["选框方式"]);
            this.toolTipScatter.SetToolTip(btnScatterType, _langs["散点图类型"]);
            this.toolTipScatter.SetToolTip(btnHrLine, _langs["等心率线"]);

            initMenuItems();

            if (!DesignMode)
            {
                // 初始化圈选/框选画笔
                selectionPaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1f,
                    Color = SKColors.Black,
                    StrokeJoin = SKStrokeJoin.Round,
                    StrokeCap = SKStrokeCap.Round
                };
                selectionInnerPaint = new SKPaint()
                {
                    IsStroke = false,
                    Color = new SKColor(128, 128, 128, 128),
                    BlendMode = SKBlendMode.SrcOver
                };
                markPaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.DarkOrange
                };

                _graphicsControl.MouseDown += OnCanvasMouseDown;
                _graphicsControl.MouseMove += OnCanvasMouseMove;
                _graphicsControl.MouseUp += OnCanvasMouseUp;
                _graphicsControl.MouseWheel += OnCanvasMouseWheel;
                _graphicsControl.MouseDoubleClick += OnCanvasMouseDoubleClick;
                _graphicsControl.ContextMenuStrip = this.ctxMenuStrip;
                this.panel2.Controls.Add(_graphicsControl);
            }
        }

        private void initMenuItems()
        {
            //foreach (char beatType in AnaValues.BeatTypes.Keys)
            //{
            //    ToolStripMenuItem typeItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.BeatTypes[beatType]])
            //    {
            //        Tag = beatType // 将心搏类型存储在Tag中
            //    };
            //    this.mnuBeatModify.DropDownItems.Add(typeItem);
            //}
            //this.mnuBeatModify.DropDownItemClicked += mnuBeatModify_DropDownItemClicked;
            List<char> types = AnaValues.BeatTypes.Keys.ToList();
            for (int i = 0; i < types.Count; i++)
            {
                char beatType = types[i];
                ToolStripMenuItem mnuItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.BeatTypes[beatType]]);
                mnuItem.Tag = beatType;
                mnuItem.Click += (o, e) =>
                {
                    if (_selectedBeats != null && _selectedBeats.Count > 0)
                    {
                        int[] samples = _selectedBeats.Select(b => b.Sample).ToArray();
                        ModifyBeatType(samples, beatType);
                    }
                };
                this.ctxMenuStrip.Items.Insert(i, mnuItem);
            }

            this.mnuBeatDelete.Text = "(D)" + _langs["删除心搏"];
            this.mnuBeatDelete.Click += mnuBeatDelete_Click;

            this.mnuShowColor.Click += (o, e) =>
            {
                _showColorMark = !_showColorMark;
                if (_showColorMark)
                    this.mnuShowColor.Text = _langs["隐藏颜色标识"];
                else
                    this.mnuShowColor.Text = _langs["显示颜色标识"];

                RefreshUI(o, true);
            };
            this.mnuAddBlockedPAC.Click += (o, e) =>
            {
                if (_selectedBeats != null && _selectedBeats.Count > 0)
                {
                    int[] samples = new int[_selectedBeats.Count];

                    for (int i = 0; i < _selectedBeats.Count; i++)
                    {
                        samples[i] = _selectedBeats[i].Sample;
                    }

                    AnaEvent.AddNcaOrFBeat(samples, 280, "房早未下传");
                    OnBeatSelected?.Invoke(o, _selectedBeats.ToList(), true);
                }
            };
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
                _pointGroups['\0'] = new List<InnerPoint>();

                for (int i = 0; i < _sourceBeats.Count; i++)
                {
                    BeatInfo beat = _sourceBeats[i];
                    SKPoint pos = getBeatPosition(beat);
                    if (beat.RRInterval <= 0 || beat.RRLatter <= 0)
                        _pointGroups['\0'].Add(new InnerPoint(i, pos));
                    else
                        _pointGroups[beat.BeatUiType].Add(new InnerPoint(i, pos));
                }
            }
        }

        protected override void drawOffscreenImage(SKCanvas canvas, int imageWidth, int imageHeight)
        {
            if (!_isScatterPlotValid)
            {
                drawCoordinateSystem(canvas, imageWidth, imageHeight);
            }
            if (!_isBeatsValid)
            {
                drawScatterPoints(canvas);
            }
            if (_showColorMark)
            {
                drawColorMarks(canvas, imageWidth, imageHeight);
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
                StrokeWidth = 1,
                IsAntialias = true
            })
            using (SKPaint gridPaint = new SKPaint
            {
                Color = SKColor.Parse("#313848"),
                StrokeWidth = 0.5f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
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
                SKPoint originalPos = new SKPoint(0, height);
                float x0 = 0, y0 = originalPos.Y;

                float intervalPx = width / 4F, intervalPy = height / 4F;
                int interval = _scatterType == 0 ? _maxScale / 4 : _maxScale / 2;
                int originValue = _scatterType == 0 ? 0 : -_maxScale;

                gridPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 5 }, 0);
                // 绘制X轴刻度、网格线及刻度值
                for (int i = 0; i <= 4; i++)
                {
                    canvas.DrawLine(i * intervalPx, y0, i * intervalPx, y0 - 6, axisPaint);
                    canvas.DrawLine(i * intervalPx, y0, i * intervalPx, 0, gridPaint);
                    SKTextAlign align = SKTextAlign.Center;
                    if (i == 0) align = SKTextAlign.Left;
                    else if (i == 4) align = SKTextAlign.Right;

                    int markValue = originValue + i * interval;
                    canvas.DrawText(markValue >= MAX_RR ? MAX_RR + "+" : markValue.ToString(), i * intervalPx, y0 - 8, align, textFont, textPaint);
                }
                // 绘制Y轴刻度、网格线及刻度值
                for (int i = 1; i <= 4; i++)
                {
                    textFont.Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold);
                    canvas.DrawLine(0, y0 - i * intervalPy, 4, y0 - i * intervalPy, axisPaint);
                    canvas.DrawLine(0, y0 - i * intervalPy, width, y0 - i * intervalPy, gridPaint);
                    int markValue = originValue + i * interval;
                    canvas.DrawText(markValue >= MAX_RR ? MAX_RR + "+" : markValue.ToString(), 1, y0 - i * intervalPy + 10, SKTextAlign.Left, textFont, textPaint);
                    //绘制等心率线
                    if (_scatterType == 0 && _isochrone)
                    {
                        //左下方
                        textFont.Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Normal);
                        canvas.DrawLine(0, y0 - i * intervalPy, i * intervalPx, y0, gridPaint);
                        SKPoint middlePosL = new SKPoint(i * intervalPx / 2, (y0 - i * intervalPy / 2));
                        SKPoint p1 = new SKPoint(middlePosL.X - 30, middlePosL.Y - 30), p2 = new SKPoint(middlePosL.X + 30, middlePosL.Y + 30);
                        canvas.DrawText((60000 / (i * interval) * 2).ToString(), p1, SKTextAlign.Left, textFont, textPaint); //心率
                        canvas.DrawText((60000 / (i * interval) * 2).ToString(), p2, SKTextAlign.Left, textFont, textPaint);
                        //右上方
                        if (i < 4)
                        {
                            canvas.DrawLine(i * intervalPx, 0, width, y0 - i * intervalPy, gridPaint);
                            SKPoint middlePosR = new SKPoint(width / 2 + i * intervalPx / 2, (y0 - i * intervalPy) / 2);
                            SKPoint p3 = new SKPoint(middlePosR.X - 30, middlePosR.Y - 30), p4 = new SKPoint(middlePosR.X + 30, middlePosR.Y + 30);
                            canvas.DrawText((60000 / (_maxScale / 2 + i * interval / 2)).ToString(), p3, SKTextAlign.Left, textFont, textPaint); //心率
                            canvas.DrawText((60000 / (_maxScale / 2 + i * interval / 2)).ToString(), p4, SKTextAlign.Left, textFont, textPaint);
                        }
                    }
                }
                //绘制对角斜线
                canvas.DrawLine(0, y0, width, 0, gridPaint);
            }
        }

        //绘制心搏散点
        private void drawScatterPoints(SKCanvas canvas)
        {
            if (_pointGroups.Count <= 0) return;

            using (var pointPaint = new SKPaint())
            {
                pointPaint.IsAntialias = true;
                pointPaint.Style = SKPaintStyle.Stroke;
                pointPaint.StrokeWidth = 3.0f;
                pointPaint.FilterQuality = SKFilterQuality.None;

                // 按颜色组批量绘制点
                foreach (var group in _pointGroups)
                {
                    if (group.Value.Count > 0)
                    {
                        if (group.Key == '\0')
                            pointPaint.Color = SKColors.Thistle;
                        else
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
                    using (SKPath path = new SKPath())
                    {
                        path.AddPoly(pathArray);
                        canvas.DrawPath(path, selectionInnerPaint);
                    }
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

        private void drawColorMarks(SKCanvas canvas, int width, int height)
        {
            int i = 0;
            float marginLeft = width * 0.07F, marginTop = height * 0.05F;
            using (SKPaint colorPaint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont()
            {
                Size = 10.5f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            {
                SKFontMetrics metrics = textFont.Metrics;
                float fontHeight = metrics.Descent - metrics.Ascent + metrics.Leading;
                float lineHeight = fontHeight + 3;
                foreach (char key in AnaValues.BeatTypes.Keys)
                {
                    if (key == 'K' || key == 'M') continue;
                    float top = marginTop + i * lineHeight;
                    colorPaint.Color = _beatColorMap[key];
                    string text = _langs[AnaValues.BeatTypes[key]] + $"[{key}]";
                    canvas.DrawRect(marginLeft, top, fontHeight, fontHeight, colorPaint);
                    canvas.DrawText(text, marginLeft + lineHeight, top + Math.Abs(metrics.Ascent), SKTextAlign.Left, textFont, textPaint);
                    i++;
                }
            }
        }

        //仅重绘圈选路径区域
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

                _graphicsControl.Invalidate(rect);
            }
            else
            {
                _graphicsControl.Invalidate();
            }
        }

        //获取单个心搏的位置
        private SKPoint getBeatPosition(BeatInfo beat)
        {
            int w = _graphicsControl.Width, h = _graphicsControl.Height;
            float xPerMs = (float)w / _maxScale, yPerMs = (float)h / _maxScale;

            float xValue, yValue;
            if (_scatterType == 0)
            {
                if (beat.RRInterval > 0)
                    xValue = beat.RRInterval * xPerMs;
                else
                    xValue = 1;

                if (beat.RRLatter > 0)
                    yValue = h - beat.RRLatter * yPerMs;
                else
                    yValue = h - 1;
            }
            else
            {
                xPerMs = (float)w / (_maxScale * 2);
                yPerMs = (float)h / (_maxScale * 2);

                int xVal = beat.RRInterval - beat.RRPrevious, yVal = beat.RRLatter - beat.RRInterval;
                xValue = w / 2 + xVal * xPerMs;
                yValue = h / 2 - yVal * yPerMs;
            }
            return new SKPoint(xValue, yValue);
        }

        //切割(新增心搏)
        public void addBeats(List<BeatInfo> cutBeats)
        {
            _sourceBeats.AddRange(cutBeats);

            RefreshUI(null);
        }

        //切割(移除心搏)
        public void removeBeats(List<BeatInfo> cutBeats)
        {
            HashSet<int> samplesToRemove = new HashSet<int>(cutBeats.Select(o => o.Sample));
            _sourceBeats.RemoveAll(b => samplesToRemove.Contains(b.Sample));

            RefreshUI(null);
        }

        //清空
        public void removeAllBeats()
        {
            _sourceBeats.Clear();
            ////if (!keepSelection)
            ////{
            //    selectionPath.Clear();
            //    markPositions?.Clear();
            ////}

            //生成界面散点集
            genBeatPoints();

            //重新创建离屏缓存
            createOffscreenSurface();

            _graphicsControl?.Refresh();
        }

        /// <summary>
        /// 执行选中后的动作
        /// </summary>
        /// <param name="keepSelection">向后指示是否需要保持心搏的选中状态</param>
        private void executeSelect(object sender, bool keepSelection = false)
        {
            ConcurrentBag<BeatInfo> tmpBeats = new ConcurrentBag<BeatInfo>();
            if (_selectMode == 0)
            {
                // 确保路径至少有3个点才能形成封闭区域
                if (selectionPath.Count >= 3)
                {
                    // 添加最后一个点，确保路径闭合
                    selectionPath.Add(selectionPath[0]);

                    try
                    {
                        Parallel.ForEach(_pointGroups, group =>
                        {
                            foreach (var point in group.Value)
                            {
                                if (AnaCode.IsPointInPolygon(point.Location.X, point.Location.Y, selectionPath))
                                {
                                    tmpBeats.Add(_sourceBeats[point.Index]);
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
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
                                    tmpBeats.Add(_sourceBeats[point.Index]);
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

            _selectedBeats = tmpBeats.OrderBy(o => o.Sample).ToList();

            // 更新叠加图
            OnBeatSelected?.Invoke(sender, _selectedBeats, keepSelection);
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
