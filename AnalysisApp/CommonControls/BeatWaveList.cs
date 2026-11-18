using AnalysisApp.Commons;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp.CommonControls
{
    public partial class BeatWaveList : BaseDrawingControl
    {
        private List<BeatInfo> _sourceBeats;
        private SKPaint selectionPaint;
        private bool _isSelecting;
        private RectangleInfo[] _rects;

        private SKPoint _selectStartPos;
        private SKPoint _lastMovePos;
        private List<int> _selectedIndexes; //选中的波形序号(当前页)
        private bool _isSelectPage = false, _isSelectAll = false; //是否选择当前页/所有心搏

        private DateTime _lastScrollTime = DateTime.MinValue;
        private BeatWaveStyle _layoutStyle = BeatWaveStyle.OneLead;
        private List<int> _waveLeads; //当前显示数据的相关导联
        private int _rowCount = 5, _colCount = 2;
        private readonly float _marginLeft = 3.6F, _marginTop = 3.6F, _marginRight = 3.6F, _marginBottom = 3.6F;
        private bool _showCommonWaveToggle = false, _showCommonWave = true;
        private bool _showHistogramToggle = false, _showHistogram = true;
        private bool _showBatchAddToggle = false;
        private int _pageCount = 1;
        private int _curPageIndex = 0;
        private int _leadCount = 1;
        private Dictionary<int, int> _currentBeatMap = new Dictionary<int, int>();
        private const int MAX_RR = 4000;

        public event Action<BeatWaveList, BeatWaveStyle> OnLayoutChanged;
        public event Action<object, List<BeatInfo>, List<BeatInfo>> OnBeatSelected;
        public event Action<List<int>> OnLeadsChanged;
        public event Action<bool> OnCommonWaveToggled;
        public event Action<bool> OnHistogramToggled;

        [Browsable(false)]
        public float Speed { get; set; } = 25; //25mm/s
        [Browsable(false)]
        public float Gain { get; set; } = 5F; //5mm/mV
        [Browsable(false), DefaultValue(BeatWaveStyle.OneLead)]
        public BeatWaveStyle LayoutStyle
        {
            get { return _layoutStyle; }
            set
            {
                _layoutStyle = value;
                LeadCount = ((int)_layoutStyle > 3) ? 3 : 1;
                if (!_layoutStyle.Equals(this.cbxLayout.SelectedValue)) { this.cbxLayout.SelectedValue = _layoutStyle; }

                OnLayoutChanged?.Invoke(this, _layoutStyle);
            }
        }

        public int LeadCount
        {
            get => _leadCount;
            set
            {
                _leadCount = value;

                if (DesignMode) return;
                if (_leadCount == 1) WaveLeads = new List<int>() { Convert.ToInt32(this.cbxLead.SelectedValue) };
                else if (_leadCount == 3) WaveLeads = new List<int>(MultiLeads);
            }
        }

        public List<int> WaveLeads
        {
            get => _waveLeads;
            set
            {
                _waveLeads = value;
                if (WaveLeads.Count > 1) OnLeadsChanged?.Invoke(_waveLeads);
            }
        }

        /// <summary>
        /// 以多导联形式显示时的相关导联序号
        /// </summary>
        public List<int> MultiLeads { get; set; }

        public int RowCount
        {
            get
            {
                return _rowCount;
            }
            set
            {
                _rowCount = value;
                //if (_sourceBeats != null && _sourceBeats.Count > 0)
                //{
                //    _pageCount = (int)Math.Ceiling((double)_sourceBeats.Count / (_rowCount * _colCount));
                //    setWaveRects();
                //}
            }
        }

        public int ColCount
        {
            get { return _colCount; }
            set
            {
                _colCount = value;
                //if (_sourceBeats != null && _sourceBeats.Count > 0)
                //{
                //    _pageCount = (int)Math.Ceiling((double)_sourceBeats.Count / (_rowCount * _colCount));
                //    setWaveRects();
                //}
            }
        }

        /// <summary>
        /// 是否显示通用心电图切换显示的工具栏按钮
        /// </summary>
        public bool ShowCommonWaveToggle
        {
            get { return _showCommonWaveToggle; }
            set
            {
                _showCommonWaveToggle = value;
                this.btnCommonWave.Visible = _showCommonWaveToggle;
            }
        }

        /// <summary>
        /// 是否显示直方图切换显示的工具栏按钮
        /// </summary>
        public bool ShowHistogramToggle
        {
            get { return _showHistogramToggle; }
            set
            {
                _showHistogramToggle = value;
                this.btnHistogram.Visible = _showHistogramToggle;
            }
        }

        /// <summary>
        /// 是否显示[批量添加心搏]按钮
        /// </summary>
        public bool ShowBatchAddToggle
        {
            get { return _showBatchAddToggle; }
            set
            {
                _showBatchAddToggle = value;
                this.btnBatchAddBeat.Visible = _showBatchAddToggle;
            }
        }

        /// <summary>
        /// 是否显示当前心搏时间
        /// </summary>
        [Browsable(false)]
        public bool ShowTime { get; set; }

        /// <summary>
        /// 是否显示平均心率
        /// </summary>
        [Browsable(false)]
        public bool ShowAverageHR { get; set; }

        /// <summary>
        /// 是否用于显示窦性心率
        /// </summary>
        [Browsable(false)]
        public bool UseForNormalSinusRhythm { get; set; }

        /// <summary>
        /// 是否用于显示心率失常(房速/室速)
        /// </summary>
        [Browsable(false)]
        public bool UseForArrhythmia { get; set; }


        //当前布局下每页可展示的波形数量
        private int PageSize { get { return RowCount * ColCount; } }

        //当前页之前的心拍个数
        private int PageBeatOffset { get { return _curPageIndex * PageSize; } }


        public BeatWaveList()
        {
            InitializeComponent();

            _sourceBeats = new List<BeatInfo>();
            _selectedIndexes = new List<int>();
            _selectedBeats = new List<BeatInfo>();
        }

        #region 事件
        private void BeatWaveList_Load(object sender, EventArgs e)
        {
            init();

            //SetBeatType(""); //加载全部心搏散点
            setWaveRects();
        }

        private void BeatWaveList_SizeChanged(object sender, EventArgs e)
        {

        }

        //三导联设置
        private void btnLeadSet_Click(object sender, EventArgs e)
        {
            LeadSettingForm frmLeadSetting = new LeadSettingForm();
            frmLeadSetting.selectedIndices.AddRange(WaveLeads);
            frmLeadSetting.OnLeadsSelected += (leadIndices) =>
            {
                MultiLeads = leadIndices;
                WaveLeads = new List<int>(MultiLeads);
                setWaveRects(); //重新获取数据
                _graphicsControl.Invalidate();
            };

            frmLeadSetting.ShowDialog(this.FindForm());
        }

        //选择当前页
        private void btnSelectCurPage_Click(object sender, EventArgs e)
        {
            resetSelect();
            this.btnSelectCurPage.BaseColor = Color.FromArgb(159, 200, 255);

            _isSelectPage = true;
            for (int i = 0; i < _rects.Length; i++) { _selectedIndexes.Add(i); }
            _selectedBeats = getSelectedBeatsOfCurPage();

            List<BeatInfo> beatList = _selectedBeats.ToList();
            OnBeatSelected?.Invoke(sender, beatList, beatList);

            _graphicsControl.Focus();
            _graphicsControl.Invalidate();
        }

        //选择所有心搏
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            resetSelect();
            this.btnSelectAll.BaseColor = Color.FromArgb(159, 200, 255);

            _isSelectAll = true;
            for (int i = 0; i < _rects.Length; i++) _selectedIndexes.Add(i);

            if (_sourceBeats != null)
            {
                _selectedBeats = new List<BeatInfo>(_sourceBeats);

                OnBeatSelected?.Invoke(sender, _selectedBeats.ToList(), getSelectedBeatsOfCurPage());

                _graphicsControl.Focus();
                _graphicsControl.Invalidate();
            }
        }

        private void graphicsControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;

                if (Control.ModifierKeys == Keys.Control)
                {
                    for (int i = 0; i < _rects.Length; i++)
                    {
                        if (_rects[i].Rect.Contains(e.X, e.Y))
                        {
                            if (!_selectedIndexes.Contains(i))
                            {
                                _selectedIndexes.Add(i);
                            }
                            else
                            {
                                _selectedIndexes.Remove(i);
                            }
                            break;
                        }
                    }
                }
                else if (Control.ModifierKeys == Keys.Shift)
                {
                    if (_selectedIndexes.Count == 0)
                    {
                        _selectStartPos = new SKPoint(e.X, e.Y);
                    }
                    else
                    {
                        int startIndex = -1, endIndex = -1;
                        SKPoint curPos = new SKPoint(e.X, e.Y);
                        for (int i = 0; i < _rects.Length; i++)
                        {
                            if (_rects[i].Rect.Contains(_selectStartPos))
                            {
                                startIndex = i;
                            }
                            if (_rects[i].Rect.Contains(curPos))
                            {
                                endIndex = i;
                            }
                        }
                        if (startIndex >= 0 && endIndex >= 0)
                        {
                            if (startIndex > endIndex) { (startIndex, endIndex) = (endIndex, startIndex); } //大小互换

                            _selectedIndexes.Clear();
                            for (int j = startIndex; j <= endIndex; j++) _selectedIndexes.Add(j);
                        }
                    }
                }
                else //普通点击
                {
                    resetSelect();
                    _selectStartPos = new SKPoint(e.X, e.Y);
                    for (int i = 0; i < _rects.Length; i++)
                    {
                        if (_rects[i].Rect.Contains(e.X, e.Y))
                        {
                            _selectedIndexes.Add(i);
                            break;
                        }
                    }
                }
                _graphicsControl.Invalidate();

            }
        }

        private void graphicsControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting && _selectStartPos != SKPoint.Empty)
            {
                SKPoint newPoint = new SKPoint(e.X, e.Y);
                if (SKPoint.Distance(_lastMovePos, newPoint) < 3.0f) return;

                SKRect selectRect = new SKRect(Math.Min(_selectStartPos.X, e.X), Math.Min(_selectStartPos.Y, e.Y), Math.Max(_selectStartPos.X, e.X), Math.Max(_selectStartPos.Y, e.Y));

                if (Control.ModifierKeys == Keys.Control)
                {
                    for (int i = 0; i < _rects.Length; i++)
                    {
                        if (_rects[i].Rect.Contains(newPoint))
                        {
                            if (!_selectedIndexes.Contains(i)) _selectedIndexes.Add(i);
                        }
                    }
                }
                else if (Control.ModifierKeys != Keys.Shift)
                {
                    for (int i = 0; i < _rects.Length; i++)
                    {
                        SKRect rect = _rects[i].Rect;
                        if (selectRect.IntersectsWith(rect))
                        {
                            if (!_selectedIndexes.Contains(i)) _selectedIndexes.Add(i);
                        }
                        //if (selectRect.Contains(rect.Location) || selectRect.Contains(rect.Right, rect.Top)
                        //    || selectRect.Contains(rect.Left, rect.Bottom) || selectRect.Contains(rect.Right, rect.Bottom))
                        //{
                        //    if (!_selectedIndexes.Contains(i)) _selectedIndexes.Add(i);
                        //}
                        //else if (rect.Contains(newPoint))
                        //{
                        //    if (_selectedIndexes.Count > 0)
                        //    {
                        //        int firstIdx = _selectedIndexes[0];
                        //        if (e.X > _rects[firstIdx].Rect.Right) //向右拖动
                        //            for (int m = firstIdx + 1; m <= i; m++) { if (!_selectedIndexes.Contains(i)) _selectedIndexes.Add(m); }
                        //        else if (e.X < _rects[firstIdx].Rect.Left) //向左拖动
                        //            for (int m = i; m < firstIdx; m++) { if (!_selectedIndexes.Contains(i)) _selectedIndexes.Add(m); }
                        //        else if (e.Y > _rects[firstIdx].Rect.Bottom) //向下拖动
                        //            for (int m = firstIdx + ColCount; m <= i; m += ColCount) { if (!_selectedIndexes.Contains(i)) _selectedIndexes.Add(m); }
                        //        else if (e.Y < _rects[firstIdx].Rect.Top) //向上拖动
                        //            for (int m = i; m < firstIdx; m += ColCount) { if (!_selectedIndexes.Contains(i)) _selectedIndexes.Add(m); }
                        //    }
                        //}

                    }
                }
                _lastMovePos = newPoint;

                _graphicsControl.Invalidate();
            }
        }

        private void graphicsControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;

                var selectedList = _selectedBeats as List<BeatInfo>;
                if (_isSelectPage || _isSelectAll)
                {
                    for (int j = 0; j < _rects.Length; j++)
                    {
                        if (_selectedIndexes.Contains(j))
                        {
                            if (!_selectedBeats.Contains(_sourceBeats[PageBeatOffset + j])) { selectedList.Add(_sourceBeats[PageBeatOffset + j]); }
                        }
                        else
                        {
                            selectedList.Remove(_sourceBeats[PageBeatOffset + j]);
                        }
                    }
                }
                else
                {
                    if (_selectedIndexes.Count > 0)
                    {
                        foreach (int idx in _selectedIndexes.OrderBy(o => o))
                        {
                            if (!_selectedBeats.Contains(_sourceBeats[PageBeatOffset + idx])) { selectedList.Add(_sourceBeats[PageBeatOffset + idx]); }
                        }
                    }
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                OnBeatSelected?.Invoke(sender, _selectedBeats.ToList(), getSelectedBeatsOfCurPage()); //点击后更新通用波形显示
            }
            else if (e.Button == MouseButtons.Right) //显示右键菜单
            {

            }
        }

        private void graphicsControl_MouseDblClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_selectedIndexes.Count > 0)
                {
                    int idx = _selectedIndexes[0];
                    int sample = _sourceBeats[PageBeatOffset + idx].Sample;
                    FullWaveForm.Instance.Show(sample, this.FindForm());
                }

            }
        }

        private void graphicsControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_sourceBeats.Count > 0 && this.vScrollBar1.Visible)
            {
                int scale = 0;
                if (e.Delta < 0) scale = 1;
                else if (e.Delta > 0) scale = -1;

                _curPageIndex = this.vScrollBar1.Value + scale * this.vScrollBar1.SmallChange;

                if (_curPageIndex < this.vScrollBar1.Minimum) _curPageIndex = this.vScrollBar1.Minimum;
                if (_curPageIndex > this.vScrollBar1.Maximum) _curPageIndex = this.vScrollBar1.Maximum;
                this.vScrollBar1.Value = _curPageIndex;

                while (_curPageIndex * PageSize >= _sourceBeats.Count) _curPageIndex--;

                RefreshUI(this);
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (this.vScrollBar1.Visible == false) return;
            if (DateTime.Now - _lastScrollTime < TimeSpan.FromMilliseconds(20)) return;

            int arrowHeight = SystemInformation.VerticalScrollBarArrowHeight;
            int trackHeight = vScrollBar1.Height - 2 * arrowHeight;
            if (e.Type == ScrollEventType.LargeDecrement || e.Type == ScrollEventType.LargeIncrement) // 点击滚动条上方或下方空白区域
            {
                // 获取当前鼠标位置
                int innerY = this.vScrollBar1.PointToClient(MousePosition).Y - arrowHeight;
                int newValue = (int)((double)innerY / trackHeight * _pageCount);
                newValue = Math.Max(vScrollBar1.Minimum, Math.Min(vScrollBar1.Maximum, newValue));
                e.NewValue = newValue;
            }

            _curPageIndex = e.NewValue;
            if (_curPageIndex < this.vScrollBar1.Minimum) _curPageIndex = this.vScrollBar1.Minimum;
            if (_curPageIndex > this.vScrollBar1.Maximum) _curPageIndex = this.vScrollBar1.Maximum;
            if (_curPageIndex * PageSize >= _sourceBeats.Count) _curPageIndex = _pageCount - 1;

            _lastScrollTime = DateTime.Now;
            RefreshUI(this);
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            //_scrollValueChanged = true;
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            try
            {
                canvas.Clear(SKColors.White);
                for (int i = 0; i < _rects.Length; i++)
                {
                    RectangleInfo rectInfo = _rects[i];
                    BeatInfo refBeat = rectInfo.ReferBeat;
                    List<float[]> waveData = rectInfo.Data.Select(o => o.WaveDataF).ToList();
                    DrawRectangleWave rectWaveDrawer = new DrawRectangleWave()
                    {
                        WaveType = LayoutStyle == BeatWaveStyle.OneBeat ? RectangleWaveType.SingleHeartbeat : RectangleWaveType.MultipleHeartbeat,
                        leadIdxs = WaveLeads,
                        WaveRect = rectInfo.Rect,
                        WaveLeftMargin = rectInfo.InnerMarginLeft,
                        WaveRightMargin = rectInfo.InnerMarginRight,
                        Speed = Speed,
                        Gain = Gain,
                        EcgData = waveData,
                        StartSample = rectInfo.StartDataIndex,
                        Sample = refBeat.Sample,
                    };
                    rectWaveDrawer.CalcV = rectInfo.ValueAhead;
                    rectWaveDrawer.BottomInfoList = new List<string>();
                    if (ShowTime) rectWaveDrawer.BottomInfoList.Add(AnaCode.getDataTimeFromSample(refBeat.StartIndex).ToString("HH:mm:ss"));
                    if (ShowAverageHR) rectWaveDrawer.BottomInfoList.Add(_langs["心率"] + "：" + rectInfo.AverageHR + " bpm");

                    bool isSelected = _selectedIndexes.Contains(i);
                    rectWaveDrawer.DrawRect(canvas, isSelected);
                    rectWaveDrawer.DrawEcgWave(canvas);
                    rectWaveDrawer.DrawMiddleLine(canvas);
                }
            }
            catch (AccessViolationException)
            {
                // GPU内存访问冲突，记录但不抛出异常
                Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
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


        public void SetBeatSource(List<BeatInfo> beats)
        {
            _currentBeatMap = AnaCode.GetBeatMap(AnaValues.CachedAllBeats);
            if (beats != null && beats.Count > 0)
            {
                _sourceBeats = new List<BeatInfo>(beats);
                _selectedBeats = new List<BeatInfo>(beats.Count);
                RefreshUI(this, true);
            }
            else
            {
                resetAsEmpty();
            }
        }

        /// <summary>
        /// 更新当前组件的数据源
        /// </summary>
        public void UpdateBeatSource(object sender, List<BeatInfo> newBeats = null, bool keepSelection = false)
        {
            _currentBeatMap = AnaCode.GetBeatMap(AnaValues.CachedAllBeats);
            if (keepSelection && !DataSourceIsChanged)
            {
                RefreshUI(sender, false, true);
                return;
            }
            else
            {
                int originalCnt = _sourceBeats == null ? 0 : _sourceBeats.Count;
                if (newBeats != null && newBeats.Count > 0)
                {
                    _sourceBeats = new List<BeatInfo>(newBeats);
                    RefreshUI(sender, originalCnt != newBeats.Count);
                }
                else
                {
                    resetAsEmpty();
                }
            }
        }

        public void DataSourceChanged(int[] samples)
        {
            List<BeatInfo> newList = _sourceBeats.FindAll(b => !samples.Contains(b.Sample));
            UpdateBeatSource(this, newList);
        }

        //刷新显示
        public void RefreshUI(object sender, bool resetPages = false, bool keepSelection = false)
        {
            if (!IsLoaded) return;

            if (resetPages)
            {
                //计算显示所需页数
                if (_sourceBeats != null && _sourceBeats.Count > 0)
                    _pageCount = (int)Math.Ceiling((double)_sourceBeats.Count / PageSize);
                else
                    _pageCount = 1;

                this.vScrollBar1.LargeChange = Math.Max(1, _pageCount / 10);
                this.vScrollBar1.Maximum = _pageCount - 1;
                if (vScrollBar1.LargeChange > 1) this.vScrollBar1.Maximum += this.vScrollBar1.LargeChange; // 确保可以滚动到最后
                this.vScrollBar1.Value = _curPageIndex = 0;
                this.vScrollBar1.Visible = _pageCount > 1;
            }

            setWaveRects();

            if (_sourceBeats != null && _sourceBeats.Count > 0)
            {
                _graphicsControl.ContextMenuStrip = this.ctxMenuStrip;
                List<BeatInfo> pageSelBeats = new List<BeatInfo>() { _sourceBeats[PageBeatOffset] };
                if (!_isSelectAll && !keepSelection)
                {
                    resetSelect();
                    _selectedIndexes.Add(0);
                    _selectedBeats.Add(_sourceBeats[PageBeatOffset]);
                }
                else
                {
                    _selectedIndexes.Clear();
                    pageSelBeats = new List<BeatInfo>(_rects.Length);
                    for (int i = 0; i < _rects.Length; i++)
                    {
                        if (_selectedBeats.Any(o => o.Sample == _sourceBeats[PageBeatOffset + i].Sample))
                        {
                            _selectedIndexes.Add(i);
                            pageSelBeats.Add(_sourceBeats[PageBeatOffset + i]);
                        }
                    }
                }

                this.lblPage.Text = $"{_curPageIndex + 1}/{_pageCount}{_langs["页"]}";
                _graphicsControl.Invalidate();

                OnBeatSelected?.Invoke(sender, pageSelBeats, pageSelBeats);
            }
            else
            {
                resetAsEmpty();
                resetSelect();
            }
        }

        //重置界面元素显示
        public void ResetUIState(bool refresh = false)
        {
            HolterConfig cfg = AnaValues.holterConfig;
            MultiLeads = new List<int>() { cfg.MainChannel, cfg.SecondChannel1, cfg.SecondChannel2 };
            if (LeadCount == 1)
            {
                WaveLeads = new List<int>() { cfg.MainChannel };
                this.cbxLead.SelectedValue = cfg.MainChannel;
            }
            else if (LeadCount == 3)
            {
                WaveLeads = new List<int>(MultiLeads);
            }

            if (refresh) RefreshUI(this);
        }

        //隐藏设置选项工具
        public void HideSettingTools()
        {
            this.label2.Visible = this.cbxGain.Visible = false;
            this.label3.Visible = this.cbxLayout.Visible = false;
            this.label4.Visible = this.cbxLead.Visible = false;
        }


        private void init()
        {
            ResetUIState();

            this.btnLeadSet.Visible = false;
            this.vScrollBar1.Visible = false;

            initMenuItems();

            if (!DesignMode)
            {
                CommonFuns.InitCommboBoxItems(this.cbxGain, SysConstants.Gains, defaultValue: 5F);
                CommonFuns.InitCommboBoxItems(this.cbxLayout, AnaValues.WaveLayouts, defaultValue: BeatWaveStyle.OneLead);
                fillLeadComboBox(this.cbxLead);

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

                _graphicsControl.MouseDown += graphicsControl_MouseDown;
                _graphicsControl.MouseMove += graphicsControl_MouseMove;
                _graphicsControl.MouseUp += graphicsControl_MouseUp;
                _graphicsControl.MouseDoubleClick += graphicsControl_MouseDblClick;
                _graphicsControl.MouseWheel += graphicsControl_MouseWheel;
                this.panel1.Controls.Add(_graphicsControl);
            }

            this.cbxGain.SelectedValueChanged += (o, e) =>
            {
                Gain = Convert.ToSingle(this.cbxGain.SelectedValue);
                _graphicsControl.Invalidate();
            };
            this.cbxLayout.SelectedValueChanged += (o, e) =>
            {
                if (this.cbxLayout.SelectedIndex >= 0)
                {
                    BeatWaveStyle layout = (BeatWaveStyle)this.cbxLayout.SelectedValue;
                    bool multiLeads = (layout == BeatWaveStyle.ThreeLeads || layout == BeatWaveStyle.ThreeLongLeads);
                    this.btnLeadSet.Visible = multiLeads;
                    this.label4.Visible = this.cbxLead.Visible = !multiLeads;
                    Application.DoEvents();
                    LayoutStyle = layout;
                }
            };
            this.cbxLead.SelectedValueChanged += (o, e) =>
            {
                WaveLeads = new List<int>() { Convert.ToInt32(this.cbxLead.SelectedValue) };
                RefreshUI(this, false, true);
            };

            this.btnCommonWave.Click += (o, e) =>
            {
                _showCommonWave = !_showCommonWave;
                OnCommonWaveToggled?.Invoke(_showCommonWave);
            };
            this.btnHistogram.Click += (o, e) =>
            {
                _showHistogram = !_showHistogram;
                OnHistogramToggled?.Invoke(_showHistogram);
            };
            this.btnBatchAddBeat.Click += (o, e) =>
            {
                if (_selectedBeats != null && _selectedBeats.Count > 0)
                {
                    Form parentForm = this.FindForm();
                    BeatBatchAddPopup frmBatchAdd = new BeatBatchAddPopup(_selectedBeats);
                    frmBatchAdd.ShowDialog(parentForm);
                    if (frmBatchAdd.DialogResult == DialogResult.OK)
                    {
                        //RefreshUI(false, true);
                        base.ChangeDataSource(null);
                        OnBeatSelected?.Invoke(o, _selectedBeats.ToList(), getSelectedBeatsOfCurPage());
                    }
                }
            };
        }

        private void initMenuItems()
        {
            Control parentCtl = CommonFuns.GetSpecifiedParent(this, typeof(BaseUserControl));
            if (parentCtl is HistogramPage)
            {
                this.ctxMenuStrip.Items.Clear();

                createBeatModifyMenuItems();
                this.ctxMenuStrip.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem mnuBeatDel = new ToolStripMenuItem("(D)" + _langs["删除心搏"], null, (o, e) => { deleteBeats(); });
                this.ctxMenuStrip.Items.Add(mnuBeatDel);

                //创建直方图界面相关菜单
                ToolStripMenuItem mnuFastest = new ToolStripMenuItem(_langs["设为最快心率"]);
                mnuFastest.Name = "mnuFastest";
                mnuFastest.Click += (o, e) => { setMinOrMaxHR(1); };
                this.ctxMenuStrip.Items.Add(mnuFastest);

                ToolStripMenuItem mnuSlowest = new ToolStripMenuItem(_langs["设为最慢心率"]);
                mnuSlowest.Name = "mnuSlowest";
                mnuSlowest.Click += (o, e) => { setMinOrMaxHR(2); };
                this.ctxMenuStrip.Items.Add(mnuSlowest);
                return;
            }

            createBeatModifyMenuItems();

            this.mnuBeatDelete.Text = "(D)" + _langs["删除心搏"];
            this.mnuBeatDelete.Click += (o, e) => { deleteBeats(); };
            if (this.FindForm() is TemplateOverlayForm)
            {
                //创建模板页菜单
                this.ctxMenuStrip.Items.Clear();

                createBeatModifyMenuItems();
                this.ctxMenuStrip.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem mnuBeatDel = new ToolStripMenuItem("(D)" + _langs["删除心搏"], null, (o, e) => { deleteBeats(); });
                this.ctxMenuStrip.Items.Add(mnuBeatDel);
                return;
            }
            //修改前一心搏
            foreach (char beatType in AnaValues.BeatTypes.Keys)
            {
                ToolStripMenuItem typeItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.BeatTypes[beatType]]);
                typeItem.Click += (o, e) => { modifyBeats(beatType, -1); };
                this.mnuModifyPrevious.DropDownItems.Add(typeItem);
            }
            this.mnuModifyPrevious.DropDownItems.Add(new ToolStripSeparator());
            this.mnuModifyPrevious.DropDownItems.Add(new ToolStripMenuItem(_langs["删除心搏"], null, (o, e) => { deleteBeats(-1); }));

            //修改后一心搏
            foreach (char beatType in AnaValues.BeatTypes.Keys)
            {
                ToolStripMenuItem typeItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.BeatTypes[beatType]]);
                typeItem.Click += (o, e) => { modifyBeats(beatType, 1); };
                this.mnuModifyLatter.DropDownItems.Add(typeItem);
            }
            this.mnuModifyLatter.DropDownItems.Add(new ToolStripSeparator());
            this.mnuModifyLatter.DropDownItems.Add(new ToolStripMenuItem(_langs["删除心搏"], null, (o, e) => { deleteBeats(1); }));

            //向前插入心搏
            List<char> types = AnaValues.QRSBeatTypes.Keys.ToList();
            for (int i = 0; i < types.Count; i++)
            {
                char beatType = types[i];
                ToolStripMenuItem mnuItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.QRSBeatTypes[beatType]]);
                mnuItem.Tag = beatType;
                mnuItem.Click += (o, e) => insertBeats(beatType, -1);
                this.mnuInsertBefore.DropDownItems.Add(mnuItem);
            }
            //向后插入心搏
            for (int i = 0; i < types.Count; i++)
            {
                char beatType = types[i];
                ToolStripMenuItem mnuItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.QRSBeatTypes[beatType]]);
                mnuItem.Tag = beatType;
                mnuItem.Click += (o, e) => insertBeats(beatType, 1);
                this.mnuInsertAfter.DropDownItems.Add(mnuItem);
            }

            //重排序
            this.mnuOrderbyTime.Click += (o, e) =>
            {
                _sourceBeats = _sourceBeats.OrderBy(b => b.Sample).ToList();
                RefreshUI(this);
            };
            this.mnuOrderbyValueAhead.Click += (o, e) =>
            {
                _sourceBeats = _sourceBeats.OrderByDescending(b => AnaCode.CalValueAhead(b, _currentBeatMap)).ToList();
                RefreshUI(this);
            };

            //添加自定义事件
        }

        private void createBeatModifyMenuItems()
        {
            List<char> types = AnaValues.BeatTypes.Keys.ToList();
            for (int i = 0; i < types.Count; i++)
            {
                char beatType = types[i];
                ToolStripMenuItem mnuItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.BeatTypes[beatType]]);
                mnuItem.Tag = beatType;
                mnuItem.Click += (o, e) => { modifyBeats(beatType); };
                this.ctxMenuStrip.Items.Insert(i, mnuItem);
            }
        }

        protected override int OnControlKeyPressed(object sender, KeyPressEventArgs e)
        {
            char beatType = char.ToUpper(e.KeyChar);
            int[] samples = _selectedBeats?.Select(o => o.Sample).ToArray();

            if (beatType == 'D')
            {
                deleteBeats();
                return 0;
            }

            modifyBeats(beatType);
            return 0;
        }

        protected override void OnControlKeyDown(Keys keyData)
        {
            int firstIndex = _selectedIndexes[0];
            int offset = 0;

            if (keyData == Keys.Left)
            {
                offset = -1;
            }
            else if (keyData == Keys.Right)
            {
                offset = 1;
            }
            else if (keyData == Keys.Up)
            {
                offset = -ColCount;
            }
            else if (keyData == Keys.Down)
            {
                offset = ColCount;
            }
            locateToBeat(offset);
        }

        /// <summary>
        /// 更新选中心搏位置
        /// </summary>
        /// <param name="offset"></param>
        private void locateToBeat(int offset)
        {
            int firstIndex = _selectedIndexes[0];
            int targetIndex = firstIndex + offset;
            int realIndex = firstIndex;
            if (targetIndex >= 0)
            {
                if (targetIndex >= PageSize)
                {
                    if (_curPageIndex < _pageCount - 1)
                    {
                        //超出当前页则滚动到下一页
                        graphicsControl_MouseWheel(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, -1));
                        realIndex = Math.Min(targetIndex - PageSize, _rects.Length - 1);
                    }
                }
                else
                {
                    if (targetIndex <= _rects.Length - 1)
                    {
                        realIndex = targetIndex;
                    }
                }
            }
            else
            {
                if (targetIndex < 0)
                {
                    if (_curPageIndex > 0)
                    {
                        graphicsControl_MouseWheel(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 1));
                        realIndex = targetIndex + PageSize;
                    }
                }
                else { realIndex = targetIndex; }
            }
            _selectedIndexes = new List<int>() { realIndex };
            _selectedBeats = new List<BeatInfo>() { _rects[realIndex].ReferBeat };
            _graphicsControl?.Invalidate();

            var pageSelBeats = new List<BeatInfo>(_selectedBeats);
            OnBeatSelected?.Invoke(this, pageSelBeats, pageSelBeats);
        }

        /// <summary>
        /// 修改心搏属性
        /// </summary>
        /// <param name="beatUiType"></param>
        /// <param name="sequenceFlag">修改前一个还是后一个心搏属性</param>
        private void modifyBeats(char beatUiType, int sequenceFlag = 0)
        {
            if (_selectedBeats != null && _selectedBeats.Count() > 0)
            {
                if (!AnaValues.BeatTypes.ContainsKey(beatUiType)) return;

                List<BeatInfo> selectCopy = new List<BeatInfo>(_selectedBeats);
                List<int> selectIdxCopy = new List<int>(_selectedIndexes);
                List<BeatInfo> updateBeats = new List<BeatInfo>(_selectedBeats.Count);
                if (sequenceFlag == -1)
                {
                    foreach (BeatInfo beat in _selectedBeats)
                    {
                        if (beat.BeatIndex > 0) updateBeats.Add(AnaValues.CachedAllBeats[beat.BeatIndex - 1]);
                    }
                }
                else if (sequenceFlag == 1)
                {
                    foreach (BeatInfo beat in _selectedBeats)
                    {
                        if (beat.BeatIndex < AnaValues.CachedAllBeats.Count - 1) updateBeats.Add(AnaValues.CachedAllBeats[beat.BeatIndex + 1]);
                    }
                }
                else
                {
                    updateBeats = selectCopy;
                }

                if (updateBeats.Count > 0)
                {
                    List<int> samplesOfUpdate = new List<int>();
                    List<int> samplesOfAdd = new List<int>();
                    foreach (BeatInfo bInfo in updateBeats)
                    {
                        if (_currentBeatMap.ContainsKey(bInfo.Sample))
                            samplesOfUpdate.Add(bInfo.Sample);
                        else
                            samplesOfAdd.Add(bInfo.Sample);
                    }

                    int ret1 = 0, ret2 = 0;
                    if (samplesOfUpdate.Count > 0)
                    {
                        ret1 = base.ModifyBeatType(samplesOfUpdate.ToArray(), beatUiType);
                    }
                    if (samplesOfAdd.Count > 0)
                    {
                        ret2 = base.AddBeats(samplesOfAdd.ToArray(), beatUiType);
                        //if (ret2 == 0)
                        //{
                        //    _deletedSamples.RemoveAll(o => samplesOfAdd.Contains(o));
                        //}
                    }

                    if (ret1 == 0 && ret2 == 0)
                    {
                        _selectedBeats = selectCopy;
                        _selectedIndexes = selectIdxCopy;
                        if (sequenceFlag == 0 && selectIdxCopy.Count == 1)
                        {
                            locateToBeat(1);
                        }
                        RefreshUI(this, false, true);
                    }
                    else
                    {
                        CommonFuns.PromptError("操作失败");
                    }
                }
            }
        }

        /// <summary>
        /// 删除心搏
        /// </summary>
        /// <param name="sequenceFlag"></param>
        private void deleteBeats(int sequenceFlag = 0)
        {
            if (_selectedBeats != null && _selectedBeats.Count > 0)
            {
                List<BeatInfo> selectCopy = new List<BeatInfo>(_selectedBeats);
                List<int> selectIdxCopy = new List<int>(_selectedIndexes);
                List<BeatInfo> delBeats = new List<BeatInfo>(_selectedBeats.Count);
                if (sequenceFlag == -1)
                {
                    foreach (BeatInfo beat in _selectedBeats)
                    {
                        if (beat.BeatIndex > 0) delBeats.Add(AnaValues.CachedAllBeats[beat.BeatIndex - 1]);
                    }
                }
                else if (sequenceFlag == 1)
                {
                    foreach (BeatInfo beat in _selectedBeats)
                    {
                        if (beat.BeatIndex < AnaValues.CachedAllBeats.Count - 1) delBeats.Add(AnaValues.CachedAllBeats[beat.BeatIndex - 1]);
                    }
                }
                else
                {
                    delBeats = selectCopy;
                }

                if (delBeats.Count > 0)
                {
                    int[] delSamples = delBeats.Select(o => o.Sample).ToArray();
                    int ret = base.DeleteBeats(delSamples);
                    if (ret == 0)
                    {
                        refreshSourceBeats();
                        _selectedBeats = selectCopy;
                        _selectedIndexes = selectIdxCopy;
                        //if (sequenceFlag == 0) _deletedSamples.AddRange(delSamples);
                    }
                    else
                    {
                        CommonFuns.PromptError("操作失败");
                    }
                }
            }
        }

        /// <summary>
        /// 插入心搏
        /// </summary>
        /// <param name="beatUiType"></param>
        /// <param name="direction">向前还是向后插入心搏</param>
        private void insertBeats(char beatUiType, int direction)
        {
            //Form parentForm = this.FindForm();
            //string title = _langs[direction == -1 ? "向前插入心搏" : "向后插入心搏"];
            //BeatBatchInsertPopup frmBatchInsert = new BeatBatchInsertPopup(_selectedBeats, beatUiType, title, direction);
            //frmBatchInsert.ShowDialog(parentForm);
            //if (frmBatchInsert.DialogResult == DialogResult.OK)
            //{
            //    RefreshUI(false, true);
            //    base.ChangeDataSource(null);
            //}

            if (_selectedBeats != null && _selectedBeats.Count > 0)
            {
                int[] insertSamples = _selectedBeats.Select(o => o.Sample).ToArray();
                int ret = -1;
                if (direction == -1)
                {
                    ret = AEcgHandler.InsertBeatsBefore(insertSamples, WaveLeads[0], beatUiType);
                }
                else if (direction == 1)
                {
                    ret = AEcgHandler.InsertBeatsAfter(insertSamples, WaveLeads[0], beatUiType);
                }

                if (ret == 0)
                {
                    refreshSourceBeats();
                    RefreshUI(this, false, true);
                    base.ChangeDataSource(null);
                }
                else
                {
                    if (insertSamples.Length == 1)
                    {
                        CommonFuns.PromptError("操作失败");
                    }
                }
            }
        }


        //设为最快/最慢心率
        private void setMinOrMaxHR(int flag)
        {
            if (_selectedBeats != null && _selectedBeats.Count == 1)
            {
                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

                BeatInfo beat = _selectedBeats[0];
                JObject segmentInfoJson = new JObject();
                segmentInfoJson["cbxLabel"] = flag == 1 ? "最快心率" : "最慢心率";
                segmentInfoJson["tbxStartTime"] = AnaCode.getDataTimeFromSample(beat.StartIndex).ToString(dateFormat);
                segmentInfoJson["cbxDuration"] = 7;
                int leftOffset = (int)(_holterConfig.SampleRate * 7F / 2);
                int startSample = beat.StartIndex > leftOffset ? beat.StartIndex - leftOffset : 0;
                segmentInfoJson["startSample"] = startSample;
                segmentInfoJson["cbxGain"] = 5.0f;
                segmentInfoJson["cbxDirection"] = 1;

                int[] leadIndices = new int[_holterConfig.Lead];
                for (int i = 0; i < _holterConfig.Lead; i++) { leadIndices[i] = i; }
                segmentInfoJson["leadIdxs"] = JArray.FromObject(leadIndices);
                segmentInfoJson["sample"] = beat.Sample;
                segmentInfoJson["eventName"] = flag == 1 ? "最快心率" : "最慢心率";

                int refNum = AnaValues.holterConfig.ParamsSetting.P14.Frequency;
                double avgRR = AnaCode.CalcAvgRR(AnaValues.CachedAllBeats, beat, refNum, out int calCount);
                segmentInfoJson["tbxAvgHR"] = Math.Floor(60 * 1000 / avgRR).ToString("F0");
                OperateSegment.ProcessHolterPicFile(segmentInfoJson);
                AnaCode.WriteSaveInfo(flag == 1 ? "最快心率" : "最慢心率", beat.Sample, Convert.ToInt32(60 * 1000 / avgRR), avgRR);
                AnaValues.DataSourceState = 1;
                base.ChangeDataSource(null);
                AnaCode.LoadStatisticsData();
                ScreenCenterTip.Show("保存成功");
            }
        }

        //设置各心拍的显示区域及相关信息
        private void setWaveRects()
        {
            if (DesignMode) return;

            if (_graphicsControl != null && _sourceBeats != null)
            {
                int targetLen = 0; //每个导联的波形数据点个数
                if (_curPageIndex < _pageCount - 1)
                    _rects = new RectangleInfo[PageSize];
                else
                    _rects = new RectangleInfo[_sourceBeats.Count - PageBeatOffset];

                float w = (_graphicsControl.Width - _marginRight) / ColCount - _marginLeft;
                float h = (_graphicsControl.Height - _marginBottom) / RowCount - _marginTop;
                var filteredBeats = AnaCode.GetValidBeats(AnaCode.FilterOutCustomEventsBeats(AnaValues.CachedAllBeats));
                var avgCalculationBeatMap = AnaCode.GetBeatMap(filteredBeats);
                int refNum = AnaValues.holterConfig.ParamsSetting.P14.Frequency;
                if (UseForNormalSinusRhythm) { if (refNum % 2 == 0) refNum += 1; }

                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < ColCount; j++)
                    {
                        int idx = i * ColCount + j;
                        if (idx < _rects.Length)
                        {
                            float left = _marginLeft + (_marginLeft + w) * j;
                            float top = _marginTop + (_marginTop + h) * i;
                            BeatInfo refBeat = _sourceBeats[PageBeatOffset + idx];

                            RectangleInfo rectInfo = _rects[idx] = new RectangleInfo();
                            rectInfo.Rect = new SKRect(left, top, left + w, top + h);
                            //rectInfo.InnerMarginLeft = 10;
                            rectInfo.ReferBeat = refBeat;

                            if (UseForArrhythmia)
                            {
                                int sequentialLen = AnaCode.GetSequentialLength(refBeat.BeatIndex, refBeat.BeatUiType);
                                refNum = sequentialLen;
                            }
                            double avgRR = AnaCode.CalcAvgRR(filteredBeats, refBeat, refNum, out int calCount, false, avgCalculationBeatMap);
                            rectInfo.AverageHR = (int)(60 * 1000 / avgRR);
                            rectInfo.ValueAhead = AnaCode.CalValueAhead(refBeat, _currentBeatMap);

                            if (targetLen == 0)
                            {
                                targetLen = (int)((w - rectInfo.InnerMarginLeft - rectInfo.InnerMarginRight) / SysConstants.PIX_SCALE / 25 * _holterConfig.SampleRate);
                            }
                            int realLen = targetLen;
                            int centerSample = rectInfo.ReferBeat.Sample;
                            if (UseForNormalSinusRhythm)
                            {
                                //将连续的几个心搏的中间位置显示在波形绘制区最中间
                                BeatInfo centerBeat = AnaValues.CachedAllBeats[rectInfo.ReferBeat.BeatIndex + refNum / 2];
                                centerSample = centerBeat.Sample;
                            }
                            rectInfo.StartDataIndex = centerSample - targetLen / 2;
                            if (centerSample - targetLen / 2 < 0)
                            {
                                realLen = centerSample + targetLen / 2;
                                rectInfo.StartDataIndex = 0;
                            }

                            //var leadDataList = AnaValues.HolterDataReader.ReadEcgDataBySample(_rects[idx].StartDataIndex, realLen);
                            var leadDataList = AnaCode.ReadAllWaveData(WaveLeads, rectInfo.StartDataIndex, rectInfo.StartDataIndex + realLen - 1);
                            rectInfo.Data = leadDataList;
                        }
                        else { break; }
                    }
                }
            }
        }

        //获取当前页选中的心拍
        private List<BeatInfo> getSelectedBeatsOfCurPage()
        {
            List<BeatInfo> curpageSelBeats = new List<BeatInfo>(_rects.Length);
            foreach (int idx in _selectedIndexes)
            {
                curpageSelBeats.Add(_sourceBeats[PageBeatOffset + idx]);
            }
            return curpageSelBeats;
        }

        private void refreshSourceBeats()
        {
            if (_sourceBeats != null && _sourceBeats.Count > 0)
            {
                _currentBeatMap = AnaCode.GetBeatMap(AnaValues.CachedAllBeats);
                for (int i = 0; i < _sourceBeats.Count; i++)
                {
                    BeatInfo beat = _sourceBeats[i];
                    bool keyExists = _currentBeatMap.TryGetValue(beat.Sample, out int beatIndex);
                    if (keyExists)
                    {
                        _sourceBeats[i] = new BeatInfo(AnaValues.CachedAllBeats[beatIndex]);
                    }
                }
            }
        }

        //重置选择
        private void resetSelect()
        {
            _isSelectPage = false;
            _isSelectAll = false;
            _selectedIndexes.Clear();

            int beatNum = _sourceBeats == null ? 0 : _sourceBeats.Count;
            _selectedIndexes.Capacity = beatNum;
            _selectedBeats = new List<BeatInfo>(beatNum);

            this.btnSelectCurPage.BaseColor = Color.FromArgb(221, 231, 245);
            this.btnSelectAll.BaseColor = Color.FromArgb(221, 231, 245);
        }

        //置空组件相关元素数据
        private void resetAsEmpty()
        {
            _sourceBeats = new List<BeatInfo>();
            _selectedBeats = new List<BeatInfo>();
            _rects = new RectangleInfo[0];
            _pageCount = 1;
            this.lblPage.Text = "";
            _graphicsControl.ContextMenuStrip = null;
            _graphicsControl.Invalidate();
        }


        private class RectangleInfo
        {
            internal SKRect Rect { get; set; }
            internal float InnerMarginLeft { get; set; } = 12;
            internal float InnerMarginRight { get; set; } = 5;
            internal int StartDataIndex { get; set; }
            internal BeatInfo ReferBeat { get; set; }
            internal int AverageHR { get; set; }
            internal int ValueAhead { get; set; } //提前量
            internal List<WaveInfo> Data { get; set; }
        }
    }
}
