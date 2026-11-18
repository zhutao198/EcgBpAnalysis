using AnalysisApp.Commons;
using AnalysisSystem.Base;
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
    public partial class OverlayPlot : BaseDrawingControl
    {
        private string _overlaytitle;
        private List<BeatInfo> _sourceBeats;
        private int _leadIndex = 1; //要显示的导联序号
        private int _beatsPerWave = 1; //几个心搏叠加
        private float _speedRatio = 1F;
        private float _gainRatio = 1F;
        private int _waveLen = 250;
        private int _amplitude = 1000; //波形在垂直方向的幅度
        private Dictionary<SKColor, List<InnerPoint>> _pointGroups;
        private List<LeadDataOfBeats> _beatDataList;

        private List<OverlayPlot> siblings;
        private SKColor enableColor = new SKColor(13, 19, 92);
        private SKColor disabledColor = new SKColor(11, 4, 64);
        private SKPaint pointPaint;
        private SKPaint selectionPaint;
        private List<SKPoint> selectionPath;
        //private ConcurrentBag<BeatInfo> selectedBeats;
        private ConcurrentBag<int> selectedBeatIndices; //选中的心拍索引, 后续可移除此变量
        private bool _isSelecting;
        private Size _originalSize;

        public event Action<object, List<BeatInfo>, bool> OnSelected;

        public event Action<List<BeatInfo>, bool, string> OnCuted;


        [Browsable(false), Description("选框类型")]
        public int SelectMode { get; set; }

        [Description("叠加图标题"), Category("自定义属性"), Browsable(true)]
        public string Title
        {
            get
            {
                return _overlaytitle;
            }
            set
            {
                _overlaytitle = value;
                this.lblTitle.Text = value;
            }
        }

        [Browsable(false)]
        public bool IsActived { get; set; }

        [Browsable(false)]
        public bool IsMainOverlay { get; set; }

        [Browsable(false)]
        public List<BeatInfo> Beats
        {
            get { return _sourceBeats; }
            private set { _sourceBeats = value; }
        }

        [Browsable(false)]
        public int LeadIndex
        {
            get { return _leadIndex; }
            set
            {
                _leadIndex = value;
                readWaveData();
                RefreshUI(null);
            }
        }

        //表示几个心搏叠加
        [Browsable(false)]
        public int BeatsPerWave
        {
            get => _beatsPerWave;
            set
            {
                _beatsPerWave = value;
                readWaveData();
                RefreshUI(null);
            }
        }

        [Browsable(false)]
        public float GainRatio
        {
            get => _gainRatio;
            set { _gainRatio = value; RefreshUI(null); }
        }

        [Browsable(false)]
        public float SpeedRatio
        {
            get => _speedRatio;
            set { _speedRatio = value; RefreshUI(null); }
        }

        //振幅
        [Browsable(false)]
        public int Amplitude { get => _amplitude; set => _amplitude = value; }

        public OverlayPlot()
        {
            InitializeComponent();

            _pointGroups = new Dictionary<SKColor, List<InnerPoint>>();
            selectionPath = new List<SKPoint>();
        }

        #region 界面元素事件
        private void OverlayPlot_Load(object sender, EventArgs e)
        {
            init();
            _originalSize = this.Size;
        }

        protected override void OnControlKeyDown(Keys keyData)
        {
            foreach (ToolStripItem mnu in this.mnuCut.DropDownItems)
            {
                if (mnu.Text.Contains(keyData.ToString()))
                {
                    mnu.PerformClick();
                }
            }
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            if (_sourceBeats == null || _sourceBeats.Count == 0)
            {
                canvas.Clear(disabledColor);
                return;
            }

            try
            {
                // 检查是否是部分重绘（仅重绘圈选路径区域）
                bool isPartialRedraw = imageWidth < _graphicsControl.Width || imageHeight < _graphicsControl.Height;
                if (!isPartialRedraw)
                {
                    canvas.Clear(IsActived ? enableColor : disabledColor);

                    // 检查离屏surface状态，必要时重新创建
                    if (offscreenSurface == null || offscreenImage == null)
                    {
                        createOffscreenSurface(checkSouceBeats);
                    }
                    if (offscreenImage != null)
                    {
                        canvas.DrawImage(offscreenImage, 0, 0);
                    }
                }

                if (SelectMode == 0)
                    drawPolygon(canvas);
                else
                    drawRectangle(canvas);
            }
            catch (AccessViolationException)
            {
                // GPU内存访问冲突，记录但不抛出异常
                Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
                // 重新创建surface以恢复正常状态
                createOffscreenSurface(checkSouceBeats);
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

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;

                selectionPath.Clear();
                selectionPath.Add(new SKPoint(e.X, e.Y));

                // 触发重绘（仅绘制圈选路径）
                redrawSelectionPathOnly(selectionPath);
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                // 限制添加点的频率，提高性能
                SKPoint newPoint = new SKPoint(e.X, e.Y);
                if (SelectMode == 0)
                {
                    if (selectionPath.Count == 0 ||
                    SKPoint.Distance(selectionPath[selectionPath.Count - 1], newPoint) > 2.0f)
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
            if (e.Button == MouseButtons.Left && _isSelecting)
            {
                _isSelecting = false;
                _selectedBeats = null;
                selectedBeatIndices = null;

                if (SelectMode == 0)
                {
                    // 确保路径至少有3个点才能形成封闭区域
                    if (selectionPath.Count >= 3)
                    {
                        // 添加最后一个点，确保路径闭合
                        selectionPath.Add(selectionPath[0]);

                        // 计算选中的点
                        ConcurrentBag<InnerPoint> selectedPoints = new ConcurrentBag<InnerPoint>();
                        try
                        {
                            // 优化性能：使用并行计算
                            Parallel.ForEach(_pointGroups, group =>
                            {
                                foreach (var point in group.Value)
                                {
                                    if (AnaCode.IsPointInPolygon(point.Location.X, point.Location.Y, selectionPath))
                                    {
                                        selectedPoints.Add(point);
                                    }
                                }
                            });
                            _selectedBeats = getSelectedBeats(selectedPoints);
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
                        ConcurrentBag<InnerPoint> selectedPoints = new ConcurrentBag<InnerPoint>();
                        try
                        {
                            Parallel.ForEach(_pointGroups, group =>
                            {
                                foreach (var point in group.Value)
                                {
                                    if (point.Location.X >= topLeft.X && point.Location.X <= bottomRight.X &&
                                        point.Location.Y >= topLeft.Y && point.Location.Y <= bottomRight.Y)
                                    {
                                        selectedPoints.Add(point);
                                    }
                                }
                            });
                            _selectedBeats = getSelectedBeats(selectedPoints);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("框过程中发生错误: " + ex.Message);
                        }
                    }
                }

                if (_sourceBeats != null && _sourceBeats.Count > 0)
                {
                    this.IsActived = true;
                    siblings.ForEach(o => { o.IsActived = false; o.RefreshUI(sender); });
                }

                // 优化：圈选完成后重绘整个控件（包括背景色变化）
                _graphicsControl.Invalidate();

                //选中事件--> 绘制各心搏波形
                if (_sourceBeats != null) OnSelected?.Invoke(sender, _sourceBeats, false);
            }
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            int scale = 0;
            if (e.Delta > 0) scale = 1;
            else if (e.Delta < 0) scale = -1;

            _gainRatio = _gainRatio * (1 + scale * 0.125F);
            if (_gainRatio > 3) _gainRatio = 3;
            if (_gainRatio < 0.25) _gainRatio = 0.25F;

            RefreshUI(sender);
        }

        // 修改心搏类型
        private void mnuBeatModify_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (_selectedBeats != null && _selectedBeats.Count > 0)
            {
                char newType = (char)e.ClickedItem.Tag;
                ////修改心拍数据源, 后续调用算法修改
                //List<BeatInfo> fileBeats = AnaValues.cachedAllBeats;
                //foreach (BeatInfo beat in selectedBeats)
                //{
                //    int idx = fileBeats.FindIndex(b => b.Sample == beat.Sample);
                //    BeatInfo newBeat = fileBeats[idx];
                //    newBeat.SetBeatType(newType);
                //    fileBeats[idx] = newBeat;
                //}
                int[] samples = _selectedBeats?.Select(o => o.Sample).ToArray();
                ModifyBeatType(samples, newType);
            }
        }

        #endregion


        public void SetBeatSource(List<BeatInfo> beats, bool activateIt = true)
        {
            IsActived = activateIt;
            if (activateIt && beats != null)
            {
                siblings.ForEach(o =>
                {
                    o.IsActived = false;
                    o.SetBeatSource(null, false);
                });
            }

            _sourceBeats = beats;
            readWaveData();
            RefreshUI(null);
        }

        /// <summary>
        /// 更新当前组件的数据源
        /// </summary>
        /// <param name="srcBeats">心搏数据来源</param>
        /// <param name="newSamples">可能需要新增的心搏</param>
        /// <returns>受影响心搏的sample点位置集合</returns>
        public List<int> UpdateBeatSource(object sender, List<BeatInfo> srcBeats, List<BeatInfo> newBeats = null)
        {
            List<int> involved = new List<int>();

            if (_sourceBeats != null && _sourceBeats.Count > 0)
            {
                var beatMap = AnaCode.GetBeatMap(srcBeats);
                for (int i = _sourceBeats.Count - 1; i >= 0; i--)
                {
                    BeatInfo beat = _sourceBeats[i];
                    if (beatMap.TryGetValue(beat.Sample, out int idx))
                        _sourceBeats[i] = srcBeats[idx];
                    else
                        _sourceBeats.RemoveAt(i);

                    involved.Add(beat.Sample);
                }
            }

            if (newBeats != null && newBeats.Count > 0)
            {
                if (_sourceBeats == null) _sourceBeats = new List<BeatInfo>(newBeats.Count);

                var beatMap = AnaCode.GetBeatMap(_sourceBeats);
                foreach (BeatInfo beat in newBeats)
                {
                    if (!beatMap.TryGetValue(beat.Sample, out int idx))
                    {
                        _sourceBeats.Add(beat);
                    }
                }
            }

            readWaveData();
            RefreshUI(sender, true);

            return involved;
        }

        public void Selected()
        {
            if (_sourceBeats != null && _sourceBeats.Count > 0)
            {
                this.IsActived = true;
                siblings.ForEach(o => { o.IsActived = false; o.RefreshUI(null); });
            }

            // 优化：圈选完成后重绘整个控件（包括背景色变化）
            _graphicsControl.Invalidate();

            //选中事件--> 绘制各心搏波形
            if (_sourceBeats != null) OnSelected?.Invoke(null, _sourceBeats, false);
        }

        //切割(添加心搏)
        public void AddBeats(List<BeatInfo> beats)
        {
            if (_sourceBeats == null)
                _sourceBeats = beats;
            else
                _sourceBeats.AddRange(beats);

            readWaveData();
            RefreshUI(null);
        }

        //切割(移除心搏)
        private void removeBeats()
        {
            //List<BeatInfo> newList = new List<BeatInfo>(_sourceBeats.Count);
            //foreach (BeatInfo beat in _selectedBeats)
            //{  

            //}
            HashSet<int> samplesToRemove = new HashSet<int>(_selectedBeats.Select(o => o.Sample));
            _sourceBeats.RemoveAll(b => samplesToRemove.Contains(b.Sample));

            readWaveData();
            RefreshUI(null);
        }


        private void init()
        {
            _leadIndex = _holterConfig.MainChannel;
            _waveLen = _holterConfig.SampleRate;

            List<OverlayPlot> overlays = CommonFuns.RetriveSubControls<OverlayPlot>(this.Parent);
            siblings = overlays.Where(o => o.Name != this.Name).OrderBy(o => o.Name).ToList();

            initMenuItems();
            if (!DesignMode)
            {
                pointPaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.5f,
                    FilterQuality = SKFilterQuality.None
                };
                // 初始化圈选/框选画笔
                selectionPaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1f,
                    Color = SKColors.White,
                    StrokeJoin = SKStrokeJoin.Round,
                    StrokeCap = SKStrokeCap.Round
                };

                _graphicsControl.MouseDown += OnCanvasMouseDown;
                _graphicsControl.MouseMove += OnCanvasMouseMove;
                _graphicsControl.MouseUp += OnCanvasMouseUp;
                _graphicsControl.MouseWheel += OnCanvasMouseWheel;
                this.Controls.Add(_graphicsControl);
                _graphicsControl.SendToBack();
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

            //叠加拆分
            foreach (OverlayPlot overlayPlot in siblings)
            {
                string[] titleParts = overlayPlot.Title.Split('-');
                string sectionName = titleParts[1].Trim(new char[] { '[', ']' });
                ToolStripMenuItem cutItem = new ToolStripMenuItem(sectionName + _langs["窗口"]);
                cutItem.Click += (o, e) =>
                {
                    if (_selectedBeats != null)
                    {
                        this.IsActived = false;
                        removeBeats();
                        overlayPlot.IsActived = true;
                        overlayPlot.AddBeats(_selectedBeats.ToList());

                        if (_selectedBeats != null) OnCuted?.Invoke(_selectedBeats, true, overlayPlot.Name);

                    }
                };
                this.mnuCut.DropDownItems.Add(cutItem);
            }
            //导联
            string leads = SysConstants.ECG_LT_STANDARD[1];
            if (_holterConfig.Lead == 3) leads = SysConstants.ECG_LT_STANDARD[5];
            string[] arrLead = leads.Split(',');
            for (int i = 0; i < arrLead.Length; i++)
            {
                int leadIndex = i;
                ToolStripMenuItem item = new ToolStripMenuItem(arrLead[i]) { Tag = leadIndex };
                item.Click += (o, e) =>
                {
                    LeadIndex = leadIndex;
                    if (siblings != null) siblings.ForEach(s => { s.LeadIndex = _leadIndex; });
                };
                this.mnuLead.DropDownItems.Add(item);
            };
            this.mnuLead.DropDownOpened += (o, e) => { this.mnuLead.CheckChildItem(item => LeadIndex.Equals(item.Tag)); };

            //增益
            float[] arrGain = new float[] { 0.5F, 1.0F, 2.0F };
            for (int i = 0; i < arrGain.Length; i++)
            {
                float gain = arrGain[i];
                ToolStripMenuItem item = new ToolStripMenuItem(gain.ToString("F1") + "x") { Tag = gain };
                item.Click += (o, e) =>
                {
                    GainRatio = gain;
                    if (siblings != null) siblings.ForEach(s => { s.GainRatio = gain; });
                };
                this.mnuGain.DropDownItems.Add(item);
            }
            this.mnuGain.DropDownOpened += (o, e) => { this.mnuGain.CheckChildItem(item => GainRatio.Equals(item.Tag)); };

            //纸速
            float[] arrSpeed = new float[] { 0.5F, 1.0F };
            for (int i = 0; i < arrSpeed.Length; i++)
            {
                float speed = arrSpeed[i];
                ToolStripMenuItem item = new ToolStripMenuItem(arrSpeed[i].ToString("F1") + "x") { Tag = speed };
                item.Click += (o, e) =>
                {
                    _speedRatio = speed;
                    if (siblings != null) siblings.ForEach(s => { s.SpeedRatio = speed; });
                };
                this.mnuSpeed.DropDownItems.Add(item);
            }
            this.mnuSpeed.DropDownOpened += (o, e) => { this.mnuSpeed.CheckChildItem(item => SpeedRatio.Equals(item.Tag)); };

            this.mnuSingleBeat.Click += (o, e) =>
            {
                BeatsPerWave = 1;
                if (siblings != null) siblings.ForEach(s => { s.BeatsPerWave = _beatsPerWave; });
            };
            this.mnuMultiBeats.Click += (o, e) =>
            {
                BeatsPerWave = 3;
                if (siblings != null) siblings.ForEach(s => { s.BeatsPerWave = _beatsPerWave; });
            };
        }

        public void DataSourceChanged(int[] samples)
        {
            List<BeatInfo> newList = _sourceBeats.FindAll(b => !samples.Contains(b.Sample));
            UpdateBeatSource(null, newList);
        }

        public int GetBeatSourceCount()
        {
            return _sourceBeats != null ? _sourceBeats.Count : 0;
        }

        public void RefreshUI(object sender, bool keepSelection = false)
        {
            //更新标题
            string sectionName = string.Empty;
            string[] titleParts = this.Title?.Split('-');
            if (titleParts != null && titleParts.Length > 1)
            {
                sectionName = titleParts[1].Trim(new char[] { '[', ']' });
                this.Title = $"{AppCode.GetLeadName(_leadIndex)}-[{sectionName}]-{_sourceBeats?.Count ?? 0}";
            }
            else
            {
                this.Title = $"{AppCode.GetLeadName(_leadIndex)}-[F1]-{_sourceBeats?.Count ?? 0}";
            }

            //菜单显示控制
            if (_sourceBeats != null && _sourceBeats.Count > 0)
                this.ContextMenuStrip = this.ctxMenuStrip;
            else
                this.ContextMenuStrip = null;

            if (!keepSelection) selectionPath.Clear();

            getOverlayWaveData();
            createOffscreenSurface(checkSouceBeats);
            _graphicsControl?.Invalidate();

            if (IsActived) OnSelected?.Invoke(sender, _sourceBeats, keepSelection);
        }

        private bool checkSouceBeats()
        {
            return _sourceBeats != null && _sourceBeats.Count > 0;
        }

        protected override void drawOffscreenImage(SKCanvas canvas, int imageWidth, int imageHeight)
        {
            drawOverlayWaves(canvas);
        }

        private void drawOverlayWaves(SKCanvas canvas)
        {
            if (_pointGroups.Count <= 0) return;

            // 按颜色组批量绘制点
            foreach (var group in _pointGroups)
            {
                if (group.Value.Count > 0)
                {
                    pointPaint.Color = group.Key;
                    SKPoint[] points = group.Value.ConvertAll(o => o.Location).ToArray();
                    canvas.DrawPoints(SKPointMode.Points, points, pointPaint);
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

        private void readWaveData()
        {
            if (_sourceBeats == null || _sourceBeats.Count == 0) return;

            int sampleLen = _beatsPerWave * _waveLen;
            ConcurrentBag<LeadDataOfBeats> beatsData = new ConcurrentBag<LeadDataOfBeats>();

            Parallel.For(0, _sourceBeats.Count, i =>
            {
                short[] filteredData = new short[sampleLen];
                int startIndex = Math.Max(0, _sourceBeats[i].Sample - sampleLen / 2);
                WaveInfo waveInfo = AnaCode.ReadWaveData(_leadIndex, startIndex, startIndex + sampleLen - 1);
                optimizedBaselineCorrection(waveInfo.WaveData, filteredData);
                //var correction = new OverlayWaveAdjust(_holterConfig);
                //correction.adaptiveBaselineCorrectionWithEnhancement(waveData, filteredData);

                LeadDataOfBeats beatData = new LeadDataOfBeats(i, filteredData) { BeatPos = _sourceBeats[i].Sample };
                beatsData.Add(beatData);
            });
            _beatDataList = beatsData.ToList();
            _beatDataList.Sort((a, b) => { return a.RefBeatIndex - b.RefBeatIndex; });

            //基于形态拟合
            doFormFitting(sampleLen);
        }

        //基于心搏获取叠加图数据
        private void getOverlayWaveData0()
        {
            if (_sourceBeats == null || _sourceBeats.Count == 0) return;

            short minValue = short.MaxValue, maxValue = short.MinValue;
            int maxX = _graphicsControl.Width, maxY = _graphicsControl.Height;
            int sampleLen = _beatsPerWave * _waveLen;

            //读取各sample点的数值队列
            List<LeadDataOfBeats> beatsData = new List<LeadDataOfBeats>(_sourceBeats.Count);
            int[][] sampleValues = new int[sampleLen][];

            for (int i = 0; i < _sourceBeats.Count; i++)
            {
                short[] leadData = new short[sampleLen];
                var lstData = AnaValues.HolterDataReader.ReadEcgDataBySample(_sourceBeats[i].Sample - sampleLen / 2, sampleLen);
                optimizedBaselineCorrectionF(lstData[_leadIndex], leadData);

                for (int j = 0; j < leadData.Length; j++)
                {
                    short rawValue = leadData[j];
                    if (rawValue < minValue) minValue = rawValue;
                    if (rawValue > maxValue) maxValue = rawValue;
                }

                LeadDataOfBeats beatData = new LeadDataOfBeats(i, leadData) { BeatPos = _sourceBeats[i].Sample };
                beatsData.Add(beatData);
            };

            //统计sample点的数值重复次数
            int totalH = maxY, baselineY = maxY / 2;
            _amplitude = Math.Max(Math.Abs(minValue), Math.Abs(maxValue));
            float pxPerValue = totalH / 2F / _amplitude * _gainRatio;
            int vArrayLength = maxY + 1; //波形显示区域垂直方向最大(像素)容量
            int yOffset = 0; //负值对齐到y轴最小值(0)的偏移量
            for (int i = 0; i < beatsData.Count; i++)
            {
                short[] beatData = beatsData[i].Data;
                for (int j = 0; j < beatData.Length; j++)
                {
                    int dataVal = beatData[j];
                    int valueIndex = (int)(baselineY + dataVal * pxPerValue);
                    if (sampleValues[j] == null)
                    {
                        sampleValues[j] = new int[vArrayLength];
                    }
                    if (valueIndex >= 0 && valueIndex < vArrayLength)
                    {
                        sampleValues[j][valueIndex]++;
                    }
                }
            }

            //转换为屏幕上的点
            _pointGroups.Clear();
            _pointGroups[SKColors.LimeGreen] = new List<InnerPoint>();
            _pointGroups[SKColors.Yellow] = new List<InnerPoint>();
            _pointGroups[SKColors.Red] = new List<InnerPoint>();
            float xOffset = _speedRatio < 1 ? (maxX * (1 - _speedRatio) / 2) : 1;
            for (int i = 0; i < sampleValues.Length; i++)
            {
                int[] arrValue = sampleValues[i];
                for (int j = 0; j < arrValue.Length; j++) //j即为上文中的valueIndex --> 纵坐标(px)
                {
                    var yCount = arrValue[j];
                    if (yCount > 0)
                    {
                        SKColor pointColor = SKColors.LimeGreen;
                        if (yCount >= 20 && yCount <= 50)
                            pointColor = SKColors.Yellow;
                        else if (yCount > 50)
                            pointColor = SKColors.Red;

                        float x = xOffset + (float)maxX * i / sampleValues.Length * _speedRatio;
                        _pointGroups[pointColor].Add(new InnerPoint(i, new SKPoint(x, maxY - j)));
                    }
                }
            }
        }

        private void getOverlayWaveData()
        {
            if (_sourceBeats == null || _sourceBeats.Count == 0) return;

            short minValue = short.MaxValue, maxValue = short.MinValue;
            int maxX = _graphicsControl.Width, maxY = _graphicsControl.Height;
            int sampleLen = _beatsPerWave * _waveLen;

            //读取各sample点的数值队列
            //ConcurrentBag<LeadDataOfBeats> beatsData = new ConcurrentBag<LeadDataOfBeats>();
            //Parallel.For(0, _sourceBeats.Count, i =>
            //{
            //    short[] leadData = new short[sampleLen];
            //    short[] waveData = AnaCode.ReadWaveData(_leadIndex, _sourceBeats[i].Sample - sampleLen / 2, _sourceBeats[i].Sample + sampleLen / 2 - 1);
            //    optimizedBaselineCorrection(waveData, leadData);

            //    for (int j = 0; j < leadData.Length; j++)
            //    {
            //        short rawValue = leadData[j];
            //        if (rawValue < minValue) minValue = rawValue;
            //        if (rawValue > maxValue) maxValue = rawValue;
            //    }

            //    LeadDataOfBeats beatData = new LeadDataOfBeats()
            //    {
            //        StartBeatIndex = i,
            //        Data = leadData
            //    };
            //    beatsData.Add(beatData);
            //});


            if (IsMainOverlay) //振幅由主叠加图显示组件确定
            {
                Parallel.For(0, _beatDataList.Count, i =>
                {
                    LeadDataOfBeats beatData = _beatDataList[i];
                    short[] leadData = beatData.Data;
                    for (int j = 0; j < leadData.Length; j++)
                    {
                        short rawValue = leadData[j];
                        if (rawValue < minValue) minValue = rawValue;
                        if (rawValue > maxValue) maxValue = rawValue;
                    }
                });
                _amplitude = Math.Max(Math.Abs(minValue + 1), Math.Abs(maxValue));
                if (_amplitude > 2000) _amplitude = 2000;
            }

            int totalH = maxY, baselineY = maxY / 2;
            float pxPerValue = totalH / 2F / _amplitude * _gainRatio;
            int vArrayLength = maxY + 1; //波形显示区域垂直方向最大(像素)容量
            int yOffset = 0; //负值对齐到y轴最小值(0)的偏移量
            List<LeadDataOfBeats> beatDataList = _beatDataList;
            //beatDataList.Sort((a, b) => { return a.StartBeatIndex - b.StartBeatIndex; });

            //统计sample点的数值重复次数
            int[][] sampleValues = new int[sampleLen][];
            for (int i = 0; i < beatDataList.Count; i++)
            {
                short[] leadData = beatDataList[i].Data;
                for (int j = 0; j < sampleLen; j++)
                {
                    int dataVal = leadData[j];
                    int valueIndex = (int)(baselineY + dataVal * pxPerValue); //转换为屏幕Y值
                    if (sampleValues[j] == null)
                    {
                        sampleValues[j] = new int[vArrayLength];
                    }
                    if (valueIndex >= 0 && valueIndex < vArrayLength)
                    {
                        sampleValues[j][valueIndex]++;
                    }
                }
            }

            SKColor midColor = new SKColor(255, 235, 0);
            _pointGroups.Clear();
            _pointGroups[SKColors.LimeGreen] = new List<InnerPoint>();
            _pointGroups[midColor] = new List<InnerPoint>();
            _pointGroups[SKColors.Red] = new List<InnerPoint>();
            float xOffset = _speedRatio < 1 ? (maxX * (1 - _speedRatio) / 2) : 1;
            for (int i = 0; i < sampleValues.Length; i++)
            {
                int[] arrValue = sampleValues[i];
                for (int j = 0; j < arrValue.Length; j++) //j即为上文中的valueIndex --> 纵坐标(px)
                {
                    var yCount = arrValue[j];
                    if (yCount > 0)
                    {
                        SKColor pointColor = SKColors.LimeGreen;
                        if (yCount >= 20 && yCount <= 35)
                            pointColor = midColor;
                        else if (yCount > 35)
                            pointColor = SKColors.Red;

                        float x = xOffset + (float)maxX * i / sampleValues.Length * _speedRatio;
                        _pointGroups[pointColor].Add(new InnerPoint(i, new SKPoint(x, maxY - j)));
                    }
                }
            }
        }

        private List<BeatInfo> getSelectedBeats(ConcurrentBag<InnerPoint> selectedPoints)
        {
            if (_pointGroups.Count == 0) return null;

            selectedBeatIndices = new ConcurrentBag<int>();
            ConcurrentBag<BeatInfo> beatSet = new ConcurrentBag<BeatInfo>();

            int maxX = _graphicsControl.Width, maxY = _graphicsControl.Height;
            int sampleLen = _beatsPerWave * _waveLen;

            //先获取所选范围内每个sample点的最小、最大坐标值
            int startIndex = sampleLen - 1, endIndex = 0;
            (float min, float max)[] sampleRangles = new (float min, float max)[sampleLen];
            for (int i = 0; i < sampleLen; i++)
            {
                float minValue = maxY, maxValue = 0;
                foreach (InnerPoint p in selectedPoints)
                {
                    if (p.Index == i)
                    {
                        if (p.Location.Y < minValue) minValue = p.Location.Y;
                        if (p.Location.Y > maxValue) maxValue = p.Location.Y;

                        if (i < startIndex) startIndex = i;
                        if (i > endIndex) endIndex = i;
                    }
                }
                sampleRangles[i] = (minValue, maxValue);
            }

            //获取在所选范围内的点
            int totalH = maxY, baselineY = maxY / 2;
            float pxPerValue = totalH / 2F / _amplitude * _gainRatio;

            //if (_beatsPerWave == 1)
            //{
            //    ConcurrentBag<LeadDataOfBeats> beatsData = new ConcurrentBag<LeadDataOfBeats>();
            //    Parallel.For(0, _sourceBeats.Count, i =>
            //    {
            //        short[] waveData = AnaCode.ReadWaveData(_leadIndex, _sourceBeats[i].Sample - sampleLen / 2, _sourceBeats[i].Sample + sampleLen / 2);
            //        beatsData.Add(new LeadDataOfBeats() { RefBeatIndex = i, Data = waveData });
            //    });
            //    var dataList = beatsData.ToList();
            //    dataList.Sort((a, b) => { return a.RefBeatIndex - b.RefBeatIndex; });
            //    var beatDataList = MorphologyClustering.morphologicalClusteringForOverlay(dataList, sampleLen, _holterConfig.SampleRate);

            //    Parallel.For(0, beatDataList.Count, i =>
            //    {
            //        short[] waveData = beatDataList[i].Data;
            //        for (int j = startIndex; j <= endIndex; j++)
            //        {
            //            int dataVal = waveData[j];
            //            int screenVal = (int)(baselineY + dataVal * pxPerValue); //转换为屏幕Y值
            //            if (maxY - screenVal >= sampleRangles[j].min - 1 && maxY - screenVal <= sampleRangles[j].max + 1)
            //            {
            //                beatSet.Add(_sourceBeats[beatDataList[i].RefBeatIndex]);
            //                selectedBeatIndices.Add(i);
            //                break;
            //            }
            //        }
            //    });
            //}
            //else
            {
                //Parallel.For(0, _sourceBeats.Count, i =>
                //{
                //    short[] filteredData = new short[sampleLen];
                //    WaveInfo waveInfo = AnaCode.ReadWaveData(_leadIndex, _sourceBeats[i].Sample - sampleLen / 2, _sourceBeats[i].Sample + sampleLen / 2);
                //    optimizedBaselineCorrection(waveInfo.WaveData, filteredData);
                //    //var correction = new OverlayWaveAdjust(_holterConfig);
                //    //correction.adaptiveBaselineCorrectionWithEnhancement(waveData, filteredData);

                //    for (int j = startIndex; j <= endIndex; j++)
                //    {
                //        int dataVal = filteredData[j];
                //        int screenVal = (int)(baselineY + dataVal * pxPerValue); //转换为屏幕Y值
                //        if (maxY - screenVal >= sampleRangles[j].min - 1 && maxY - screenVal <= sampleRangles[j].max + 1)
                //        {
                //            beatSet.Add(_sourceBeats[i]);
                //            selectedBeatIndices.Add(i);
                //            break;
                //        }
                //    }
                //});

                Parallel.For(0, _beatDataList.Count, i =>
                {
                    short[] filteredData = _beatDataList[i].Data;
                    int beatIndex = _beatDataList[i].RefBeatIndex;
                    for (int j = startIndex; j <= endIndex; j++)
                    {
                        int dataVal = filteredData[j];
                        int screenVal = (int)(baselineY + dataVal * pxPerValue); //转换为屏幕Y值
                        if (maxY - screenVal >= sampleRangles[j].min - 1 && maxY - screenVal <= sampleRangles[j].max + 1)
                        {
                            beatSet.Add(_sourceBeats[beatIndex]);
                            selectedBeatIndices.Add(beatIndex);
                            break;
                        }
                    }
                });
            }

            return beatSet.ToList();
        }

        //参照形态均值进行拟合
        private void doFormFitting(int sampleLen)
        {
            //按形态分组并求均值
            short[] arrBeatSubType = AEcgHandler.ReclusterBeatsByLead(_leadIndex, _beatDataList.Select(o => o.BeatPos).ToArray());
            Dictionary<short, SumInfo> formSum = new Dictionary<short, SumInfo>();
            for (int n = 0; n < _beatDataList.Count; n++)
            {
                LeadDataOfBeats beatData = _beatDataList[n];
                //short waveform = _sourceBeats[beatData.RefBeatIndex].MorphologyTypeEx;
                short waveform = arrBeatSubType[n];
                if (waveform == -1) //形态不明确的不做拟合
                {
                }
                else
                {
                    bool hasForm = formSum.TryGetValue(waveform, out SumInfo sumData);
                    if (!hasForm) { formSum[waveform] = new SumInfo() { BeatCount = 0, SampleSums = new int[sampleLen] }; }

                    SumInfo sumInfo = formSum[waveform];
                    sumInfo.BeatCount++;
                    for (int i = 0; i < beatData.Data.Length; i++)
                    {
                        sumInfo.SampleSums[i] += beatData.Data[i];
                    }
                }
            }

            //求每个采样点位置的波形幅度均值
            Dictionary<short, short[]> formAvg = new Dictionary<short, short[]>();
            foreach (short key in formSum.Keys)
            {
                SumInfo sumInfo = formSum[key];
                short[] arrAvg = formAvg[key] = new short[sampleLen];
                for (int i = 0; i < sumInfo.SampleSums.Length; i++)
                {
                    double avg = (double)sumInfo.SampleSums[i] / sumInfo.BeatCount;
                    if (avg > short.MaxValue) arrAvg[i] = short.MaxValue;
                    else if (avg < short.MinValue) arrAvg[i] = short.MinValue;
                    else arrAvg[i] = (short)avg;
                }
            }

            //将波形数据向形态均值拟合
            for (int n = 0; n < _beatDataList.Count; n++)
            {
                short[] arrSource = _beatDataList[n].Data;
                //short waveform = _sourceBeats[_beatDataList[n].RefBeatIndex].MorphologyTypeEx;
                short waveform = arrBeatSubType[n];
                if (waveform != -1)
                {
                    short[] arrAvg = formAvg[waveform];
                    for (int i = 0; i < arrSource.Length; i++)
                    {
                        double deviation = arrSource[i] - arrAvg[i];
                        double absDev = Math.Abs(deviation);
                        // Sigmoid: 偏差越大，纠偏系数越接近1（但永远<1）
                        double correctionFactor = 1 - 1 / (1 + Math.Exp(0.001 * absDev));
                        //double correctionFactor = Math.Pow(absDev, -0.1);

                        arrSource[i] = (short)(arrSource[i] - deviation * correctionFactor);
                    }
                }
            }
        }

        //基线校正
        private void optimizedBaselineCorrectionF(List<float> srcData, short[] sharedBuffer, int windowSize = 20)
        {
            var tStart = DateTime.Now;

            int len = srcData.Count;
            float[] corrected = new float[len];

            // 滑动窗口求和
            float windowSum = 0;
            float[] slidingWindow = new float[len];

            for (int i = 0; i < len; i++)
            {
                int start = Math.Max(0, i - windowSize);
                int end = Math.Min(len - 1, i + windowSize);

                if (i == 0)
                {
                    // 初始化窗口
                    for (int j = start; j <= end; j++)
                        windowSum += srcData[j] * 1000;
                }
                else
                {
                    // 滑动更新：移除左边界，添加右边界
                    if (start > 0) windowSum -= srcData[start - 1] * 1000;
                    if (end < len - 1) windowSum += srcData[end + 1] * 1000;
                }

                slidingWindow[i] = windowSum / (end - start + 1);
            }

            // 校正信号
            for (int i = 0; i < len; i++)
                corrected[i] = srcData[i] * 1000 - slidingWindow[i];

            // 写入共享缓冲区
            for (int i = 0; i < len; i++)
            {
                // 保证 short 范围 [-32768, 32767]
                int value = (int)Math.Round(corrected[i]);
                if (value > short.MaxValue) value = short.MaxValue;
                if (value < short.MinValue) value = short.MinValue;
                sharedBuffer[i] = (short)value;
            }

            //var elapsed = (DateTime.Now - tStart).TotalMilliseconds;
            //Console.WriteLine("Baseline correction time: " + elapsed + " ms");
        }

        private void optimizedBaselineCorrection(short[] srcData, short[] sharedBuffer)
        {
            DateTime tStart = DateTime.Now;

            //1000Hz 采样率：200ms, 500Hz 采样率：100ms, 250Hz 采样率：50ms
            int windowSize = (int)(250F * _holterConfig.SampleRate / 1000);

            int len = srcData.Length;
            float[] corrected = new float[len];

            // 滑动窗口均值滤波
            float windowSum = 0;
            float[] slidingWindow = new float[len];

            for (int i = 0; i < len; i++)
            {
                int start = Math.Max(0, i - windowSize);
                int end = Math.Min(len - 1, i + windowSize);

                if (i == 0)
                {
                    // 初始化窗口
                    for (int j = start; j <= end; j++)
                        windowSum += srcData[j];
                }
                else
                {
                    // 滑动更新：移除左边界，添加右边界
                    if (start > 0) windowSum -= srcData[start - 1];
                    if (end < len - 1) windowSum += srcData[end + 1];
                }

                slidingWindow[i] = windowSum / (end - start + 1);
            }

            // 校正信号
            for (int i = 0; i < len; i++)
                corrected[i] = srcData[i] - slidingWindow[i];

            // 写入共享缓冲区
            for (int i = 0; i < len; i++)
            {
                // 确保数据范围[-32768, 32767]
                int value = (int)Math.Round(corrected[i]);
                if (value > short.MaxValue) value = short.MaxValue;
                if (value < short.MinValue) value = short.MinValue;
                sharedBuffer[i] = (short)value;
            }

            var elapsed = (DateTime.Now - tStart).TotalMilliseconds;
            //Console.WriteLine("Baseline correction time: " + elapsed + " ms");
        }

        private void optimizedBaselineCorrection_gt(short[] srcData, short[] sharedBuffer)
        {
            int len = srcData.Length;

            // 步骤1：高通滤波去除基线漂移（保持波形在零线附近）
            float[] highPassed = highPassFilterInternal(srcData, len, _holterConfig.SampleRate, 0.5);

            // 步骤2：计算增强信号（不改变基线）
            float[] enhanced = new float[len];

            // 方法A：简单差分增强（最安全，不破坏基线）
            for (int i = 1; i < len - 1; i++)
            {
                // 计算局部变化率（一阶导数）
                float derivative = (highPassed[i + 1] - highPassed[i - 1]) * 0.5f;

                // 只增强变化剧烈的区域（QRS波），平坦区域不变
                float enhancement = 0f;
                if (Math.Abs(derivative) > 100)  // 阈值可调
                {
                    // 变化越剧烈，增强越多
                    float strength = Math.Min(0.6f, Math.Abs(derivative) / 1000f);
                    enhancement = derivative * strength;
                }

                enhanced[i] = highPassed[i] + enhancement;
            }

            // 边界处理
            enhanced[0] = highPassed[0];
            enhanced[len - 1] = highPassed[len - 1];

            // 步骤3：统一增益（整体缩放，不破坏基线）
            //float maxAbs = 0;
            //for (int i = 0; i < len; i++)
            //    maxAbs = Math.Max(maxAbs, Math.Abs(enhanced[i]));

            //float targetMax = 30000f;  // 目标峰值，留余量避免削顶
            //float gain = targetMax / maxAbs;
            //gain = Math.Clamp(gain, 0.5f, 2.5f);

            // 写入输出
            for (int i = 0; i < len; i++)
            {
                int value = (int)Math.Round(enhanced[i] * _gainRatio);
                sharedBuffer[i] = (short)Math.Min(Math.Max(value, short.MinValue), short.MaxValue);
            }
        }

        //高通滤波
        private float[] highPassFilterInternal(short[] data, int len, int sampleRate, double cutoffHz)
        {
            float[] result = new float[len];

            double RC = 1.0 / (2 * Math.PI * cutoffHz);
            double dt = 1.0 / sampleRate;
            double alpha = RC / (RC + dt);

            float prevOutput = 0;
            float prevInput = data[0];

            for (int i = 0; i < len; i++)
            {
                float output = (float)(alpha * (prevOutput + data[i] - prevInput));
                result[i] = output;
                prevOutput = output;
                prevInput = data[i];
            }

            return result;
        }


        private class LeadDataOfBeats
        {
            internal int RefBeatIndex { get; set; }
            internal int BeatPos { get; set; }
            internal short[] Data { get; set; }
            public LeadDataOfBeats(int referBeatIndex, short[] waveData)
            {
                RefBeatIndex = referBeatIndex;
                Data = waveData;
            }
        }

        private class InnerPoint
        {
            /// <summary>
            /// Sample点序号(在显示的心搏范围内)
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

        private class SumInfo
        {
            /// <summary>
            /// 相关心搏数量
            /// </summary>
            internal int BeatCount { get; set; }
            /// <summary>
            /// 每个采样点位置各心搏幅值的和
            /// </summary>
            internal int[] SampleSums { get; set; }
        }
    }
}
