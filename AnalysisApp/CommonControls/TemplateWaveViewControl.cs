using AnalysisApp.Commons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class TemplateWaveViewControl : BaseDrawingControl
    {
        public char subTemplateType = 'N';
        public List<BeatInfo> beats = new List<BeatInfo>();
        private int rows { get; set; } = 10;              //行数
        private int cols { get; set; } = 1;              //列数

        private int Pages { get; set; } = 0;                      //总页数
        public int PageIdx { get; set; } = 0;                    //当前页索引，从0开始
        public int PageWaveCount = 0;
        // 鼠标滚动防抖动相关变量
        private int pendingScrollPages;
        private DateTime _lastWheelTime = DateTime.MinValue;

        private DateTime _lastMouseDownTime = DateTime.MinValue;


        // 滚动条防抖动相关变量
        private int pendingScrollValue;
        private DateTime _lastScrollTime = DateTime.MinValue;

        private float Speed = 25f;
        private float Gain = 5.0f;
        private float PixMM = 3.777f;

        private float rectWidth { get; set; } = 92f;         //每个矩形区域的宽度
        private float rectHeight { get; set; } = 92f;        //每个矩形区域的高度
        private int rectPadding { get; set; } = 4;      //每个矩形之间的间隙

        private BeatWaveStyle _layoutStyle = BeatWaveStyle.OneLead;
        private int _leadCount = 1;
        private List<int> leadIdxs;
        private List<int> MultiLeads; //多导联显示时相关的导联序号

        private List<bool> PageSubTemplateSelectStatus { get; set; } = new List<bool>();
        private List<SKRect> PageSubTemplateRects { get; set; } = new List<SKRect>();
        private List<int> PageTemplateSamples { get; set; } = new List<int>();
        private List<BeatInfo> PageBeats { get; set; } = new List<BeatInfo>();
        private List<int> PageCalcV { get; set; } = new List<int>();

        //待删除心搏标记：存已标记删除的心搏 Sample(采样点)。
        //只做标记，不立即删除；切换子模板/切换类型模板时由 CommitPendingDeletes() 统一真正删除。
        //用 Sample 而非列表下标，免疫 SortBeats() 重排/翻页偏移，且跨子模板跟随心搏；
        //心搏类型被修改后标记会被移除(RemoveDeleteFlag)。
        private HashSet<int> DeleteBeatFlags = new HashSet<int>();


        private bool showCommonWave = true;
        private RectangleWaveType WaveType { get; set; } = RectangleWaveType.MultipleHeartbeat;//条图类型


        private DrawRectangleWave drawRectangleWave;

        private int ShiftStartRectIdx = -1;
        private bool bShowGrid = false;
        private bool bShowMiddleLine = true;

        private int SubTemplateIdx = 0; //子模板索引

        private bool _isSelectAll = false; //是否选择当前页/所有心搏

        public int ClickRectIdx = 0;   //鼠标点击的模板当前页索引
        private int LastCheckIdx = 0;   //切换布局前，包含翻页的索引

        private int ResortByType = 1;   //0：相似度排序，1：时间排序，2：提前量排序
        private bool _suppressComboEvents = false;  //程序化设置下拉框选中值时抑制事件，避免触发重聚类/级联刷新
        /// <summary>
        /// 点击事件
        /// </summary>
        public event Action<int> OnSampleChanged;

        public event Action<int> OnSampleIdxChanged;

        public event Action<List<int>> OnResort;
        public event Action OnQRSResort;

        public event Action<List<int>> OnTemplateLeadChanged;
        public event Action<List<int>, int> OnSingleLeadChanged;
        public event Action<float, int> OnGainChanged;



        public event Action<bool> OnCommonWaveShowChanged;


        public TemplateWaveViewControl()
        {
            InitializeComponent();
        }

        public void InitDeletedFlag()
        {
            DeleteBeatFlags.Clear();
        }

        public int GetDeletedFlagCount()
        {
            return DeleteBeatFlags.Count;
        }

        /// <summary>
        /// 收集所有已标记待删除的心搏 Sample(只标记，未实际删除)
        /// </summary>
        public List<int> GetPendingDeleteSamples()
        {
            return new List<int>(DeleteBeatFlags);
        }

        /// <summary>
        /// 提交删除：真正调用 AEcgHandler.DeleteBeats 删除所有已标记的心搏，成功后清空标记。
        /// 在切换子模板/切换类型模板时由 TemplatePageControl 调用。返回 0 表示成功(或没有待删除标记)。
        /// </summary>
        public int CommitPendingDeletes()
        {
            if (DeleteBeatFlags.Count == 0) return 0;

            //只删除当前病例中真实存在的心搏，防止跨病例/重分析后的残留标记误删
            Dictionary<int, int> beatMap = AnaCode.GetBeatMap(AnaValues.CachedAllBeats);
            List<int> samples = new List<int>();
            foreach (int sample in DeleteBeatFlags)
            {
                if (beatMap.ContainsKey(sample)) samples.Add(sample);
            }

            if (samples.Count == 0)
            {
                DeleteBeatFlags.Clear();   //标记已全部失效，直接丢弃
                return 0;
            }

            int ret = AEcgHandler.DeleteBeats(samples.ToArray());
            if (ret == 0)
            {
                DeleteBeatFlags.Clear();
            }
            return ret;
        }

        /// <summary>
        /// 移除指定心搏的删除标记(心搏类型被修改后调用，按需求取消其待删除状态)
        /// </summary>
        public void RemoveDeleteFlag(IEnumerable<int> samples)
        {
            if (samples == null) return;
            foreach (int sample in samples)
            {
                DeleteBeatFlags.Remove(sample);
            }
        }

        public void RefreshData()
        {
            //注意：此处不清除删除标记——标记需保留到切换界面离开模板页时统一提交删除
            resetSelect();
            UpdateDataForceWaveformRedraw();
            if (beats.Count > 0)
            {
                int sample = beats[0].Sample;
                OnSampleChanged?.Invoke(sample);
            }

            InitializeContextMenu();

        }

        public void initData()
        {
            SortBeats();
            initPageData();
        }

        public void SortBeats()
        {
            if (beats.Count == 0) return;
            var beatMap = AnaCode.GetBeatMap(AnaValues.CachedAllBeats);
            switch (ResortByType)
            {
                case 0:
                    BeatsSimilarity();
                    break;
                case 1:
                    beats = beats.OrderBy(b => b.Sample).ToList();
                    break;
                case 2:
                    beats = beats.OrderByDescending(b => AnaCode.CalValueAhead(b, beatMap)).ToList();
                    break;
                default:
                    beats = beats.OrderBy(b => b.Sample).ToList();
                    break;
            }
        }

        public void BeatsSimilarity()
        {
            int[] samples = beats.Select(b => b.Sample).ToArray();

            double[] similaritys = AEcgHandler.GetPaceSimilarity(leadIdxs[0],samples);
            if (similaritys == null) return;
            // 4. 安全回填
            int count = Math.Min(samples.Length, similaritys.Length);
            for (int i = 0; i < count; i++)
            {
                beats[i] = new BeatInfo(beats[i]) { Similarity = similaritys[i] };
            }

            // 5. 原地排序（避免创建新列表）
            beats = beats.OrderByDescending(b => b.Similarity).ToList();
        }

        public void initPageData()
        {
            if (beats.Count == 0)
            {
                //数据为空时复位页索引，避免残留的 PageIdx 指向已不存在的页
                PageIdx = 0;
                return;
            }
            var beatMap = AnaCode.GetBeatMap(AnaValues.CachedAllBeats);
            PageSubTemplateSelectStatus.Clear();
            PageCalcV.Clear();

            PageWaveCount = rows * cols;

            Pages = beats.Count % PageWaveCount != 0 ? (beats.Count / PageWaveCount + 1) : beats.Count / PageWaveCount;

            //数据收缩(如提交删除后分组变小)时 PageIdx 可能越出最后一页：
            //不钳位会导致绘制循环整页空白、PageSubTemplateRects 为空，
            //之后任何键盘导航都会在 PageBeats[ClickRectIdx] 处抛越界异常
            if (PageIdx > Pages - 1)
            {
                PageIdx = Math.Max(0, Pages - 1);
            }
            if (PageIdx < 0)
            {
                PageIdx = 0;
            }

            UpdateScrollBarMax();
            for (int i = PageIdx * PageWaveCount; i < (PageIdx + 1) * PageWaveCount; i++)
            {
                if (i >= beats.Count)
                {
                    break;
                }
                if (_isSelectAll)
                {
                    PageSubTemplateSelectStatus.Add(true);
                }
                else
                {
                    if (i == ClickRectIdx + PageIdx * PageWaveCount)
                    {
                        PageSubTemplateSelectStatus.Add(true);
                    }
                    else
                    {
                        PageSubTemplateSelectStatus.Add(false);
                    }
                }

                PageCalcV.Add(AnaCode.CalValueAhead(beats[i], beatMap));
            }
        }

        public void UpdatePageDataForceWaveformRedraw()
        {
            initPageData();
            vScrollBar1.Value = Convert.ToInt32(PageIdx);
            _graphicsControl?.Refresh();

        }

        public void UpdateDataForceWaveformRedraw()
        {
            initData();
            ForceWaveformRedraw();
        }

        public void ForceWaveformRedraw()
        {
            vScrollBar1.Value = Convert.ToInt32(PageIdx);
            _graphicsControl?.Refresh();
        }

        protected override int OnControlKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (_selectedBeats == null || _selectedBeats.Count == 0)
            {
                return 0;
            }

            char beatType = char.ToUpper(e.KeyChar);
            if (beatType == 'D' || beatType == '.')
            {
                //只添加删除标记，不立即删除；切换子模板/切换类型模板时统一提交删除。
                //已标记的心搏仍可被修改心搏类型，修改成功后标记会被移除(RemoveDeleteFlag)。
                if (_isSelectAll)
                {
                    for (int i = 0; i < beats.Count; i++)
                    {
                        DeleteBeatFlags.Add(beats[i].Sample);
                    }
                }
                else
                {
                    int count = Math.Min(PageSubTemplateSelectStatus.Count, PageTemplateSamples.Count);
                    for (int i = 0; i < count; i++)
                    {
                        if (PageSubTemplateSelectStatus[i] == true)
                        {
                            DeleteBeatFlags.Add(PageTemplateSamples[i]);
                        }
                    }
                }

                _graphicsControl?.Invalidate();
                return 0;
            }

            return base.OnControlKeyPressed(sender, e);
        }

        /// <summary>
        /// 心搏类型修改成功后，移除这些心搏的删除标记
        /// </summary>
        protected override int ModifyBeatType(int[] samples, char beatUiType)
        {
            int ret = base.ModifyBeatType(samples, beatUiType);
            if (ret == 0 && samples != null && samples.Length > 0)
            {
                RemoveDeleteFlag(samples);
                _graphicsControl?.Invalidate();
            }
            return ret;
        }

        public void SelectBeatChanged(Keys keyData)
        {
            if (PageSubTemplateSelectStatus.Count == 0 || PageBeats.Count == 0)
            {
                return;
            }

            if (keyData == Keys.Up)
            {
                if((ClickRectIdx <= cols && ClickRectIdx / cols == 0) && PageIdx == 0)
                {
                    return;
                }
                ClickRectIdx -= cols;
            }
            else if (keyData == Keys.Down)
            {
                ClickRectIdx += cols;
            }
            else if (keyData == Keys.Left)
            {
                ClickRectIdx--;
            }
            else if (keyData == Keys.Right)
            {
                ClickRectIdx++;
            }

            bool pageChanged = false;
            if (ClickRectIdx >= rows * cols)
            {
                ClickRectIdx -= rows * cols;
                //已是最后一页则不再翻页，停留在当前页(否则 PageIdx 越界后新页无数据，
                //PageSubTemplateSelectStatus 越界、PageBeats[ClickRectIdx] 抛异常)
                if (PageIdx < Pages - 1)
                {
                    PageIdx++;
                    pageChanged = true;
                }
            }
            else if(ClickRectIdx < 0)
            {
                if(PageIdx > 0)
                {
                    ClickRectIdx += rows * cols;
                    PageIdx--;
                    pageChanged = true;
                }
                else
                {
                    ClickRectIdx = 0;
                }
            }

            if (pageChanged)
            {
                //先重建新页数据(不重绘)，再按新页实际条目数钳位选中位置
                ClickRectIdx = Math.Max(0, Math.Min(ClickRectIdx, rows * cols - 1));
                initData();
            }

            //统一钳位：最后一页条目可能不满，索引必须落在有效范围内
            PageIdx = Math.Max(0, Math.Min(PageIdx, Math.Max(0, Pages - 1)));
            if (PageSubTemplateSelectStatus.Count == 0)
            {
                ForceWaveformRedraw();
                return;
            }
            ClickRectIdx = Math.Max(0, Math.Min(ClickRectIdx, PageSubTemplateSelectStatus.Count - 1));

            for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
            {
                PageSubTemplateSelectStatus[i] = false;
            }
            PageSubTemplateSelectStatus[ClickRectIdx] = true;
            ForceWaveformRedraw();

            List<BeatInfo> selectBeats = new List<BeatInfo>();
            selectBeats.Add(PageBeats[ClickRectIdx]);

            _selectedBeats = selectBeats;

            OnSampleChanged?.Invoke(PageBeats[ClickRectIdx].Sample);
            OnSampleIdxChanged?.Invoke(ClickRectIdx + PageIdx * PageWaveCount);
        }

        public void BeatTypeChanged()
        {
            if (PageSubTemplateSelectStatus.Count == 0 || PageBeats.Count == 0)
            {
                return;
            }

            ClickRectIdx++;
            bool pageChanged = false;
            if(ClickRectIdx >= rows * cols)
            {
                ClickRectIdx = 0;
                //已是最后一页则不翻页，避免 PageIdx 越界导致后续索引异常
                if (PageIdx < Pages - 1)
                {
                    PageIdx++;
                    pageChanged = true;
                }
            }

            if (pageChanged)
            {
                initData();
            }

            PageIdx = Math.Max(0, Math.Min(PageIdx, Math.Max(0, Pages - 1)));
            if (PageSubTemplateSelectStatus.Count == 0)
            {
                ForceWaveformRedraw();
                return;
            }
            ClickRectIdx = Math.Max(0, Math.Min(ClickRectIdx, PageSubTemplateSelectStatus.Count - 1));

            for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
            {
                PageSubTemplateSelectStatus[i] = false;
            }
            PageSubTemplateSelectStatus[ClickRectIdx] = true;
            ForceWaveformRedraw();

            List<BeatInfo> selectBeats = new List<BeatInfo>();
            selectBeats.Add(PageBeats[ClickRectIdx]);

            _selectedBeats = selectBeats;

            OnSampleChanged?.Invoke(PageBeats[ClickRectIdx].Sample);
            OnSampleIdxChanged?.Invoke(ClickRectIdx + PageIdx * PageWaveCount);
        }
        private void TemplateWaveViewControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                resetSelect();
                init();
                initControl();
                InitDeletedFlag();
                UpdateDataForceWaveformRedraw();
            }
        }

        private void initControl()
        {
            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            _graphicsControl.MouseDoubleClick += OnCanvasMouseDblClick;
            _graphicsControl.MouseWheel += OnCanvasMouseWheel;
            panel1.Controls.Add(_graphicsControl);

            // 初始化右键菜单
            InitializeContextMenu();

            drawRectangleWave = new DrawRectangleWave();
        }

        // 初始化右键菜单
        //注意：RefreshData 每次刷新都会调用本方法，菜单只能构建一次——
        //否则每次都 new 一个约 20 项的 ContextMenuStrip 且旧菜单不 Dispose，造成内存泄漏，
        //且"显示网格/隐藏定位轴线"等切换状态的菜单文字会被重置
        private bool _contextMenuInitialized = false;
        private void InitializeContextMenu()
        {
            if (_contextMenuInitialized)
            {
                return;
            }
            _contextMenuInitialized = true;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // 添加修改心搏类型子菜单
            //ToolStripMenuItem changeHeartbeatTypeMenuItem = new ToolStripMenuItem("修改心搏类型");
            // 为每种心搏类型添加子菜单项
            foreach (char beatType in AnaValues.BeatTypes.Keys)
            {
                ToolStripMenuItem changeHeartbeatTypeMenuItem = new ToolStripMenuItem($"({beatType})" + CommonValues.Langs[AnaValues.BeatTypes[beatType]])
                {
                    Tag = beatType // 将心搏类型存储在Tag中
                };
                changeHeartbeatTypeMenuItem.Click += ChangeHeartbeatTypeItem_Click;
                contextMenu.Items.Add(changeHeartbeatTypeMenuItem);

                //_ = changeHeartbeatTypeMenuItem.DropDownItems.Add(subMenuItem);
            }

            //contextMenu.Items.Add(changeHeartbeatTypeMenuItem);
            contextMenu.Items.Add(new ToolStripSeparator());

            //删除心搏
            ToolStripMenuItem deleteBeatMenuItem = new ToolStripMenuItem("(D)" + CommonValues.Langs["删除心搏"]);
            deleteBeatMenuItem.Click += DeleteBeatMenuItem_Click;
            contextMenu.Items.Add(deleteBeatMenuItem);

            // 添加修改心搏类型子菜单
            ToolStripMenuItem QRSSortMenuItem = new ToolStripMenuItem(CommonValues.Langs["QRS形态再分类"]);
            // 为每种心搏类型添加子菜单项
            foreach (char beatType in AnaValues.QRSBeatTypes.Keys)
            {
                ToolStripMenuItem subMenuItem = new ToolStripMenuItem($"({beatType})" + CommonValues.Langs[AnaValues.QRSBeatTypes[beatType]])
                {
                    Tag = beatType // 将心搏类型存储在Tag中
                };
                subMenuItem.Click += QRSSortTypeItem_Click;
                _ = QRSSortMenuItem.DropDownItems.Add(subMenuItem);
            }
            contextMenu.Items.Add(QRSSortMenuItem);

            ////重排序
            //ToolStripMenuItem reSortMenuItem = new ToolStripMenuItem("重排序");
            //// 为每种心搏类型添加子菜单项
            //foreach (char type in AnaValues.resortType.Keys)
            //{
            //    ToolStripMenuItem subMenuItem = new ToolStripMenuItem(AnaValues.resortType[type])
            //    {
            //        Tag = type // 将心搏类型存储在Tag中
            //    };
            //    subMenuItem.Click += ReSortTypeItem_Click;
            //    _ = reSortMenuItem.DropDownItems.Add(subMenuItem);
            //}
            //contextMenu.Items.Add(reSortMenuItem);
            ToolStripMenuItem resortBySimilarityMenuItem = new ToolStripMenuItem(CommonValues.Langs["按相似度排序"]);
            resortBySimilarityMenuItem.Click += ResortBySimilarityMenuItem_Click;
            _ = contextMenu.Items.Add(resortBySimilarityMenuItem);

            ToolStripMenuItem resortByTimeMenuItem = new ToolStripMenuItem(CommonValues.Langs["按时间排序"]);
            resortByTimeMenuItem.Click += ResortByTimeMenuItem_Click;
            _ = contextMenu.Items.Add(resortByTimeMenuItem);

            ToolStripMenuItem resortByCalcVMenuItem = new ToolStripMenuItem(CommonValues.Langs["按提前量排序"]);
            resortByCalcVMenuItem.Click += ResortByCalcVMenuItem_Click;
            _ = contextMenu.Items.Add(resortByCalcVMenuItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem hideMiddleLineMenuItem = new ToolStripMenuItem(CommonValues.Langs["隐藏定位轴线"]);
            hideMiddleLineMenuItem.Click += HideMiddleLineMenuItem_Click;
            _ = contextMenu.Items.Add(hideMiddleLineMenuItem);

            ToolStripMenuItem showGridMenuItem = new ToolStripMenuItem(CommonValues.Langs["显示网格"]);
            showGridMenuItem.Click += ShowGridMenuItem_Click;
            _ = contextMenu.Items.Add(showGridMenuItem);

            _graphicsControl.ContextMenuStrip = contextMenu;
        }

        private void changeBeatType(char newType)
        {
            List<int> samples = new List<int>();
            if (_isSelectAll)
            {
                for (int i = 0; i < beats.Count; i++)
                {
                    samples.Add(beats[i].Sample);
                }
            }
            else
            {
                for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                {
                    if (PageSubTemplateSelectStatus[i] == true)
                    {
                        samples.Add(PageTemplateSamples[i]);
                    }
                }
            }


            //未选中任何心搏时直接返回，否则空数组修改后仍会触发 ChangeDataSource 全量级联刷新
            if (samples.Count == 0)
            {
                return;
            }

            int retK = AEcgHandler.ModifyBeats(samples.ToArray(), newType);
            if (retK != 0)
            {
                CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
                return;
            }

            //心搏类型修改成功：按需求移除这些心搏的删除标记(取消其待删除状态)
            RemoveDeleteFlag(samples);
            _graphicsControl?.Invalidate();

            if (newType == 'F')
            {
                AnaEvent.AddNcaOrFBeat(samples.ToArray(), 280, "室性融合波");
            }

            base.ChangeDataSource(null);
        }

        //删除心搏(只标记，切换界面时统一提交删除)
        private void DeleteBeatMenuItem_Click(object sender, EventArgs e)
        {
            // 获取选中的新心搏类型
            if (!(sender is ToolStripMenuItem clickedItem))
            {
                return;
            }

            //只添加删除标记，不立即删除；切换子模板/切换类型模板时统一提交删除。
            if(_isSelectAll)
            {
                for(int i=0; i<beats.Count; i++)
                {
                    DeleteBeatFlags.Add(beats[i].Sample);
                }

            }
            else
            {
                int count = Math.Min(PageSubTemplateSelectStatus.Count, PageTemplateSamples.Count);
                for (int i = 0; i < count; i++)
                {
                    if (PageSubTemplateSelectStatus[i] == true)
                    {
                        DeleteBeatFlags.Add(PageTemplateSamples[i]);
                    }
                }
            }

            _graphicsControl?.Invalidate();
        }

        // 修改心搏类型菜单项点击事件
        private void ChangeHeartbeatTypeItem_Click(object sender, EventArgs e)
        {
            // 获取选中的新心搏类型
            if (!(sender is ToolStripMenuItem clickedItem) || !(clickedItem.Tag is char))
            {
                return;
            }

            char newType = (char)clickedItem.Tag;

            changeBeatType(newType);
        }

        private void QRSSortType(char newType)
        {
            BeatGroup beatGroup = QueryBeatGroup.beatGroupByType(newType.ToString());
            Dictionary<string, List<BeatInfo>> beatGroupData = beatGroup.Data;
            var keys = beatGroupData.Keys.Where(k => Convert.ToInt32(k) >= 200);
            int maxKey = 200;
            foreach(string key in keys)
            {
                maxKey = Math.Max(maxKey, Convert.ToInt32(key));
            }

            int sample = 0;
            bool hasSelection = false;
            for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
            {
                if (PageSubTemplateSelectStatus[i] == true)
                {
                    sample = PageTemplateSamples[i];
                    hasSelection = true;
                    break;
                }
            }
            //未选中任何心搏时直接返回，否则会以 sample=0(第一条心搏)为目标误执行重分类
            if (!hasSelection)
            {
                return;
            }

            AEcgHandler.ReclassifySimilarBeatsInSubTemplate(sample, newType, Convert.ToInt32(this.cmbLead.SelectedValue), (short)++maxKey);
            OnQRSResort?.Invoke();
        }

        //QRS形态再分类
        private void QRSSortTypeItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                resetSelect();
                char newType = (char)menuItem.Tag;

                QRSSortType(newType);
            }
        }

        private void ResortBySimilarityMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                ResortByType = 0;
                ClickRectIdx = 0;
                PageIdx = 0;
                UpdateDataForceWaveformRedraw();
            }
        }

        private void ResortByTimeMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                ResortByType = 1;
                ClickRectIdx = 0;
                PageIdx = 0;
                UpdateDataForceWaveformRedraw();
            }
        }

        private void ResortByCalcVMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                ResortByType = 2;
                ClickRectIdx = 0;
                PageIdx = 0;
                UpdateDataForceWaveformRedraw();
            }
        }

        //定位轴线显示隐藏
        private void HideMiddleLineMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                bShowMiddleLine = !bShowMiddleLine;
                // 根据bShowMiddleLine状态设置菜单文字
                menuItem.Text = bShowMiddleLine ? CommonValues.Langs["隐藏定位轴线"] : CommonValues.Langs["显示定位轴线"];

                _graphicsControl.Invalidate();
            }
        }

        //网格隐藏显示
        private void ShowGridMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                bShowGrid = !bShowGrid;
                // 根据bShowGrid状态设置菜单文字
                menuItem.Text = bShowGrid ? CommonValues.Langs["隐藏网格"] : CommonValues.Langs["显示网格"];

                _graphicsControl.Invalidate();
            }
        }

        private void init()
        {
            HolterConfig cfg = AnaValues.holterConfig;
            leadIdxs = new List<int>() { cfg.MainChannel };
            MultiLeads = new List<int>() { cfg.MainChannel, cfg.SecondChannel1, cfg.SecondChannel2 };

            this.leadSet.Visible = false;

            CommonFuns.InitCommboBoxItems(this.cmbGain, SysConstants.Gains, defaultValue: 5.0F);
            CommonFuns.InitCommboBoxItems(this.cmbLayout, AnaValues.WaveLayouts, defaultValue: BeatWaveStyle.OneLead);
            fillLeadComboBox(this.cmbLead);
            fillLeadComboBox(this.cmbResort);
            LayoutStyle = BeatWaveStyle.OneLead;

            this.cmbGain.SelectedValueChanged += (o, e) =>
            {
                Gain = Convert.ToSingle(this.cmbGain.SelectedValue);
                OnGainChanged?.Invoke(Gain, ClickRectIdx + PageIdx * PageWaveCount);
                _graphicsControl.Invalidate();
            };
            this.cmbLayout.SelectedValueChanged += (o, e) =>
            {
                if (this.cmbLayout.SelectedIndex == -1) return;
                BeatWaveStyle layout = (BeatWaveStyle)this.cmbLayout.SelectedValue;
                bool multiLeads = (layout == BeatWaveStyle.ThreeLeads || layout == BeatWaveStyle.ThreeLongLeads);
                this.leadSet.Visible = multiLeads;
                this.lblLead.Visible = !multiLeads;
                this.cmbLead.Visible = !multiLeads;
                this.lblResort.Visible = !multiLeads;
                this.cmbResort.Visible = !multiLeads;
                if (!multiLeads)
                    leadIdxs = new List<int>() { Convert.ToInt32(this.cmbLead.SelectedValue) };
                else
                    leadIdxs = new List<int>(MultiLeads);

                LayoutStyle = layout;
            };
            this.cmbLead.SelectedValueChanged += (o, e) =>
            {
                if (_suppressComboEvents) return;
                leadIdxs = new List<int>() { Convert.ToInt32(this.cmbLead.SelectedValue) };
                OnSingleLeadChanged?.Invoke(leadIdxs, ClickRectIdx + PageIdx * PageWaveCount);
                _graphicsControl.Invalidate();
            };
            this.cmbResort.SelectedValueChanged += (o, e) =>
            {
                if (_suppressComboEvents) return;
                AEcgHandler.ReclusterSubTemplatesByLead(Convert.ToInt32(this.cmbResort.SelectedValue), beats[0].BeatUiType);
                this.cmbLead.SelectedValue = this.cmbResort.SelectedValue;
                leadIdxs = new List<int>() { Convert.ToInt32(this.cmbLead.SelectedValue) };
                OnResort?.Invoke(leadIdxs);
            };
        }


        //重置界面元素显示
        public void ResetUIState(bool refresh = false)
        {
            HolterConfig cfg = AnaValues.holterConfig;
            MultiLeads = new List<int>() { cfg.MainChannel, cfg.SecondChannel1, cfg.SecondChannel2 };
            if (_leadCount == 1)
                leadIdxs = new List<int>() { cfg.MainChannel };
            else if (_leadCount == 3)
                leadIdxs = new List<int>(MultiLeads);

            if (refresh)
            {
                ClickRectIdx = 0;
                PageIdx = 0;
                //程序化设置下拉框选中值：抑制事件，避免触发 ReclusterSubTemplatesByLead(原生重聚类)
                //以及 OnResort 级联的整页刷新
                _suppressComboEvents = true;
                try
                {
                    if (_leadCount == 1)
                    {
                        this.cmbLead.SelectedValue = _holterConfig.MainChannel;
                    }
                    this.cmbResort.SelectedValue = _holterConfig.MainChannel;
                }
                finally
                {
                    _suppressComboEvents = false;
                }
                //注意：此处不再调用 UpdateDataForceWaveformRedraw()，
                //由调用方(TemplatePageControl 的 refreshUI)统一刷新一次。
                //否则相似度排序(GetPaceSimilarity)等重计算会执行两次，导致进入界面卡顿
            }
        }

        public BeatWaveStyle LayoutStyle
        {
            get { return _layoutStyle; }
            set
            {
                _layoutStyle = value;
                int formHeight = 0;
                Form parentForm = this.FindForm();

                if (parentForm == null)
                {
                    return;
                }
                formHeight = parentForm.Height;

                LastCheckIdx = PageIdx * rows * cols + ClickRectIdx;
                switch (_layoutStyle)
                {
                    case BeatWaveStyle.OneBeat:
                        rectWidth = 92f;
                        if (showCommonWave)
                        {
                            if (formHeight < 1000)
                            {
                                rows = 5;
                            }
                            else
                            {
                                rows = 8;
                            }
                        }
                        else
                        {
                            if (formHeight < 1000)
                            {
                                rows = 9;
                            }
                            else
                            {
                                rows = 12;
                            }

                        }
                        cols = Convert.ToInt32(this.panel1.Width / (rectWidth + rectPadding));
                        _leadCount = 1;
                        WaveType = RectangleWaveType.SingleHeartbeat;

                        break;
                    case BeatWaveStyle.OneLead:
                        if (showCommonWave)
                        {
                            if (formHeight < 1000)
                            {
                                rows = 4;
                            }
                            else
                            {
                                rows = 6;
                            }
                        }
                        else
                        {
                            if (formHeight < 1000)
                            {
                                rows = 8;
                            }
                            else
                            {
                                rows = 10;
                            }
                        }
                        cols = 3;
                        _leadCount = 1;
                        WaveType = RectangleWaveType.MultipleHeartbeat;
                        break;
                    case BeatWaveStyle.ThreeLeads:
                        if (showCommonWave)
                        {
                            if (formHeight < 1000)
                            {
                                rows = 2;
                            }
                            else
                            {
                                rows = 3;
                            }
                        }
                        else
                        {
                            if (formHeight < 1000)
                            {
                                rows = 4;
                            }
                            else
                            {
                                rows = 5;
                            }
                        }
                        cols = 3;
                        _leadCount = 3;
                        WaveType = RectangleWaveType.MultipleHeartbeat;
                        break;
                }
                if (_leadCount == 3) leadIdxs = new List<int>(MultiLeads);

                //if (_leadCount == 1)
                //{
                //    this.cmbLead.SelectedValue = _holterConfig.MainChannel;
                //    this.cmbResort.SelectedValue = _holterConfig.MainChannel;
                //    leadIdxs = new List<int>() { Convert.ToInt32(this.cmbLead.SelectedValue) };
                //}
                //else if (_leadCount == 3) leadIdxs = new List<int>(MultiLeads);


                PageIdx = LastCheckIdx / (rows*cols);
                ClickRectIdx = LastCheckIdx % (rows * cols);

                UpdateDataForceWaveformRedraw();
            }
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            PageSubTemplateRects.Clear();
            PageTemplateSamples.Clear();
            PageBeats.Clear();
            rectWidth = (imageWidth - rectPadding * (cols + 1)) / cols;
            float allWidth = (rectWidth + rectPadding) * cols;
            if (allWidth < imageWidth)
            {
                rectWidth += (imageWidth - allWidth) / (cols + 1);
            }

            rectHeight = (imageHeight - rectPadding * (rows + 1)) / rows;

            drawRectangleWave.WaveType = WaveType;
            drawRectangleWave.Speed = Speed;
            drawRectangleWave.Gain = Gain;
            // 清除画布为白色背景
            canvas.Clear(SKColors.White);

            for (int i = PageIdx * PageWaveCount; i < (PageIdx + 1) * PageWaveCount; i++)
            {
                if (i >= beats.Count)
                {
                    break;
                }
                int rowIdx = (i - PageIdx * PageWaveCount) / cols;
                int colIdx = (i - PageIdx * PageWaveCount) % cols;
                SKRect waveRect = new SKRect(colIdx * rectWidth + (colIdx + 1) * rectPadding, rowIdx * rectHeight + (rowIdx + 1) * rectPadding, (colIdx + 1) * rectWidth + (colIdx + 1) * rectPadding, (rowIdx + 1) * rectHeight + (rowIdx + 1) * rectPadding);
                drawRectangleWave.WaveRect = waveRect;
                drawRectangleWave.DrawRect(canvas, PageSubTemplateSelectStatus[i - PageIdx * PageWaveCount]);
                drawRectangleWave.leadIdxs = leadIdxs;
                //按 Sample 判断是否已标记删除，与 SortBeats() 的排序结果无关
                drawRectangleWave.bDeleted = DeleteBeatFlags.Contains(beats[i].Sample);
                BeatInfo beatInfo = beats[i];
                PageSubTemplateRects.Add(waveRect);
                PageTemplateSamples.Add(beatInfo.Sample);
                PageBeats.Add(beatInfo);

                drawRectangleWave.CalcV = PageCalcV[i - PageIdx * PageWaveCount];
                int sample = beatInfo.Sample;
                drawRectangleWave.Sample = sample;
                if (WaveType == RectangleWaveType.SingleHeartbeat)
                {
                    int startSample = Math.Max(0, Convert.ToInt32(sample - 0.4 * AnaValues.holterConfig.SampleRate));
                    int endSample = Math.Min(Convert.ToInt32(sample + 0.4 * AnaValues.holterConfig.SampleRate), AnaValues.holterConfig.Total - 1);

                    List<WaveInfo> EcgData = AnaCode.ReadAllWaveData(leadIdxs, startSample, endSample);
                    List<float[]> waveData = EcgData.Select(o => o.WaveDataF).ToList();
                    drawRectangleWave.EcgData = waveData;
                    drawRectangleWave.StartSample = startSample;

                    int points = drawRectangleWave.EcgData[leadIdxs[0]].Length;
                    drawRectangleWave.WaveLeftMargin = (float)(rectWidth - (points * 1.0f / AnaValues.holterConfig.SampleRate) * Speed * PixMM) / 2;
                    drawRectangleWave.WaveRightMargin = (float)(rectWidth - (points * 1.0f / AnaValues.holterConfig.SampleRate) * Speed * PixMM) / 2;
                    //int beatIdx = AnaValues.cachedAllBeats.FindIndex(p => p.Sample == beatInfo.Sample);
                    //if(beatIdx != -1)
                    //{
                    //    drawRectangleWave.subTemplateType = AnaValues.cachedAllBeats[beatIdx].BeatUiType;
                    //}
                    //else
                    //{
                    //    drawRectangleWave.subTemplateType = 'N';
                    //}
                }
                else
                {
                    drawRectangleWave.WaveLeftMargin = 3 * PixMM;
                    drawRectangleWave.WaveRightMargin = 2;

                    int points = Convert.ToInt32((rectWidth - drawRectangleWave.WaveLeftMargin - drawRectangleWave.WaveRightMargin) / PixMM / Speed * AnaValues.holterConfig.SampleRate);
                    int startSample = sample - points / 2 > 0 ? sample - points / 2 : 0;
                    int endSample = sample + points / 2 < AnaValues.holterConfig.Total ? sample + points / 2 : AnaValues.holterConfig.Total - 1;
                    List<WaveInfo> EcgData = AnaCode.ReadAllWaveData(leadIdxs, startSample, endSample);
                    List<float[]> waveData = EcgData.Select(o => o.WaveDataF).ToList();
                    drawRectangleWave.EcgData = waveData;
                    drawRectangleWave.StartSample = startSample;

                }
                if (WaveType == RectangleWaveType.MultipleHeartbeat)
                {
                    string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
                    string startTime = AnaCode.getDataTimeFromSample(sample).ToString(dateFormat);
                    List<string> bottomInfoList = new List<string>();
                    bottomInfoList.Add(startTime.Substring(11));
                    drawRectangleWave.BottomInfoList = bottomInfoList;

                }
                else
                {
                    drawRectangleWave.BottomInfoList.Clear();
                }

                if (bShowGrid)
                {
                    drawRectangleWave.DrawGrid(canvas);
                }
                if (bShowMiddleLine)
                {
                    drawRectangleWave.DrawMiddleLine(canvas);
                }

                drawRectangleWave.DrawEcgWave(canvas);

            }


        }
        #region 鼠标事件
        private bool isMouseDown = false;
        private Point mouseDownPoint;
        private Rectangle selectionRect = Rectangle.Empty;
        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastMouseDownTime < TimeSpan.FromMilliseconds(20)) return;

            isMouseDown = true;
            mouseDownPoint = e.Location;
            selectionRect = new Rectangle(e.X, e.Y, 0, 0);
            if (e.Button == MouseButtons.Left)
            {
                ClickRectIdx = FindRectIdx(e.X, e.Y);
                if (ClickRectIdx >= 0)
                {
                    // 检查是否按住Ctrl键
                    bool isCtrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                    // 检查是否按住Shift键
                    bool isShiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

                    if (isCtrlPressed)
                    {
                        // Ctrl+点击：切换选择状态（多选）
                        PageSubTemplateSelectStatus[ClickRectIdx] = !PageSubTemplateSelectStatus[ClickRectIdx];
                    }
                    else if (isShiftPressed)
                    {
                        ShiftMultiSelection(ClickRectIdx);
                    }
                    else
                    {
                        ShiftStartRectIdx = ClickRectIdx;

                        int sample = beats[ClickRectIdx + PageIdx * PageWaveCount].Sample;

                        OnSampleChanged?.Invoke(sample);
                        OnSampleIdxChanged?.Invoke(ClickRectIdx + PageIdx * PageWaveCount);

                        // 普通点击：清除所有选择，只选择当前项
                        for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                        {
                            PageSubTemplateSelectStatus[i] = false;
                        }
                        PageSubTemplateSelectStatus[ClickRectIdx] = true;
                    }

                }
                //else
                //{
                //    // 点击空白区域：如果没有按住Ctrl键，清除所有选择
                //    bool isCtrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                //    if (!isCtrlPressed)
                //    {
                //        for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                //        {
                //            PageSubTemplateSelectStatus[i] = false;
                //        }
                //    }
                //}
                _graphicsControl?.Invalidate();

            }
            _lastMouseDownTime = DateTime.Now;

        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && e.Button == MouseButtons.Left)
            {
                // 更新选择矩形
                int x = Math.Min(mouseDownPoint.X, e.X);
                int y = Math.Min(mouseDownPoint.Y, e.Y);
                int width = Math.Abs(e.X - mouseDownPoint.X);
                int height = Math.Abs(e.Y - mouseDownPoint.Y);

                selectionRect = new Rectangle(x, y, width, height);

                // 执行多选逻辑
                PerformMultiSelection(e);

                // 强制重绘以显示选择矩形
                _graphicsControl?.Invalidate();

            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (isMouseDown && e.Button == MouseButtons.Left)
            {
                resetSelect();

                isMouseDown = false;
                selectionRect = Rectangle.Empty;
                _graphicsControl?.Invalidate();

                List<BeatInfo> selectBeats = new List<BeatInfo>();
                for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                {
                    if (PageSubTemplateSelectStatus[i] == true)
                    {
                        selectBeats.Add(PageBeats[i]);
                    }
                }

                _selectedBeats = selectBeats;

            }
        }

        private void OnCanvasMouseDblClick(object sender, MouseEventArgs e)
        {
            //注意：双击不参与拖拽框选，不设置 isMouseDown/mouseDownPoint——
            //否则非左键双击后没有对应的左键 MouseUp 复位，isMouseDown 卡在 true，
            //之后的左键拖拽会从过期的 mouseDownPoint 开始错误多选
            if (e.Button == MouseButtons.Left)
            {
                resetSelect();
                int clickRectIdx = FindRectIdx(e.X, e.Y);
                if (clickRectIdx >= 0)
                {

                    ShiftStartRectIdx = clickRectIdx;
                    int sample = beats[clickRectIdx + PageIdx * PageWaveCount].Sample;
                    FullWaveForm.Instance.Show(sample, this.FindForm());

                    // 普通点击：清除所有选择，只选择当前项
                    for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                    {
                        PageSubTemplateSelectStatus[i] = false;
                    }
                    PageSubTemplateSelectStatus[clickRectIdx] = true;

                }
                _graphicsControl.Invalidate();

            }
        }

        private int FindRectIdx(float x, float y)
        {
            int rectIdx = -1;
            for (int i = 0; i < PageSubTemplateRects.Count; i++)
            {
                SKRect rect = PageSubTemplateRects[i];
                if ((x > rect.Left && x < rect.Right) && (y > rect.Top && y < rect.Bottom))
                {
                    rectIdx = i;
                    break;
                }
            }
            return rectIdx;
        }

        private void ShiftMultiSelection(int clickRectIdx)
        {
            if (clickRectIdx < ShiftStartRectIdx)
            {
                for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                {
                    if (i < clickRectIdx || i > ShiftStartRectIdx)
                    {
                        PageSubTemplateSelectStatus[i] = false;
                    }
                    else
                    {
                        PageSubTemplateSelectStatus[i] = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                {
                    if (i > clickRectIdx || i < ShiftStartRectIdx)
                    {
                        PageSubTemplateSelectStatus[i] = false;
                    }
                    else
                    {
                        PageSubTemplateSelectStatus[i] = true;
                    }
                }
            }
        }

        /// <summary>
        /// 执行多选逻辑
        /// </summary>
        private void PerformMultiSelection(MouseEventArgs e)
        {
            // 如果拖拽距离很小，认为是单击，不执行多选
            if (Math.Abs(e.X - mouseDownPoint.X) < 5 && Math.Abs(e.Y - mouseDownPoint.Y) < 5)
            {
                return;
            }

            // 清除所有选择状态
            for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
            {
                PageSubTemplateSelectStatus[i] = false;
            }

            // 选择与选择矩形相交的所有矩形
            for (int i = 0; i < PageSubTemplateRects.Count; i++)
            {
                SKRect rect = PageSubTemplateRects[i];
                Rectangle winRect = new Rectangle((int)rect.Left, (int)rect.Top,
                                                 (int)(rect.Right - rect.Left),
                                                 (int)(rect.Bottom - rect.Top));

                if (selectionRect.IntersectsWith(winRect))
                {
                    PageSubTemplateSelectStatus[i] = true;
                }
            }
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastWheelTime < TimeSpan.FromMilliseconds(20)) return;

            if (Pages < 2)
            {
                return;
            }
            // 计算滚动的秒数（根据滚动方向和当前显示行数）
            int scrollPages = e.Delta > 0 ? -1 : 1;

            // 累积待处理的滚动秒数
            pendingScrollPages += scrollPages;

            if (pendingScrollPages != 0)
            {
                // 更新时间偏移
                PageIdx += pendingScrollPages;
                PageIdx = Math.Max(0, Math.Min(PageIdx, vScrollBar1.Maximum));
                // 更新滚动条位置
                vScrollBar1.Value = Convert.ToInt32(PageIdx);

                ClickRectIdx = 0;

                UpdatePageDataForceWaveformRedraw();

                // 重置待处理滚动页数
                pendingScrollPages = 0;

                int beatIndex = PageIdx * PageWaveCount < beats.Count ? PageIdx * PageWaveCount : beats.Count - 1;
                int sample = beats[beatIndex].Sample;
                OnSampleChanged?.Invoke(sample);
                OnSampleIdxChanged?.Invoke(PageIdx * PageWaveCount);

                this.btnSelectCurPage.BaseColor = Color.FromArgb(221, 231, 245);

            }
            _lastWheelTime = DateTime.Now;
        }
        #endregion

        // 滚动条防抖动计时器事件处理
        private void ScrollBarScroll()
        {
            // 更新时间偏移
            PageIdx = pendingScrollValue;
            ClickRectIdx = 0;

            UpdatePageDataForceWaveformRedraw();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (DateTime.Now - _lastScrollTime < TimeSpan.FromMilliseconds(20)) return;

            // 根据滚动事件的类型处理
            if (e.Type == ScrollEventType.SmallIncrement) // 向下箭头
            {
                //注意：不能写 Math.Min(--Pages, ...)——那会把总页数 Pages 永久减 1
                PageIdx = Math.Min(Pages - 1, ++PageIdx);
                pendingScrollValue = Math.Max(0, Math.Min(PageIdx, vScrollBar1.Maximum));
            }
            else if (e.Type == ScrollEventType.SmallDecrement) // 向上箭头
            {
                PageIdx = Math.Max(--PageIdx, 0);
                pendingScrollValue = Math.Max(PageIdx, 0);
            }
            else if (e.Type == ScrollEventType.LargeDecrement) // 点击滚动条上方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = vScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.Y - 17) * 1.0f / vScrollBar1.Height) * vScrollBar1.Maximum);
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.LargeIncrement) // 点击滚动条下方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = vScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.Y + 17) * 1.0f / vScrollBar1.Height) * vScrollBar1.Maximum);
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.ThumbTrack || e.Type == ScrollEventType.ThumbPosition)
            {
                // 拖动滑块时，直接设置待处理值为滑块位置
                pendingScrollValue = Math.Max(0, Math.Min(e.NewValue, vScrollBar1.Maximum));
            }

            ScrollBarScroll();
            _lastScrollTime = DateTime.Now;

            int beatIndex = PageIdx * PageWaveCount < beats.Count ? PageIdx * PageWaveCount : beats.Count - 1;
            int sample = beats[beatIndex].Sample;
            OnSampleChanged?.Invoke(sample);
            OnSampleIdxChanged?.Invoke(PageIdx * PageWaveCount);

            this.btnSelectCurPage.BaseColor = Color.FromArgb(221, 231, 245);



        }

        // 更新滚动条的最大值
        private void UpdateScrollBarMax()
        {
            if (Pages > 1)
            {
                vScrollBar1.Show();
            }
            else
            {
                vScrollBar1.Hide();
            }

            vScrollBar1.Maximum = Math.Max(0, Pages - 1);
            if (Pages > 10)
            {
                vScrollBar1.LargeChange = Pages / 10;
            }
            else
            {
                vScrollBar1.LargeChange = 1;
            }
            this.lblPageInfo.Text = CommonValues.Langs["第"] + $"{PageIdx + 1}/{Pages}"+ CommonValues.Langs["页"];

        }

        public void OnChangeSubtemplate(List<BeatInfo> beatInfo, int subTemplateIdx)
        {
            this.PageIdx = 0;
            ClickRectIdx = 0;
            SubTemplateIdx = subTemplateIdx;
            //注意：换心搏列表不清除删除标记——标记按 Sample 跟随心搏，保留到切换界面统一提交
            this.beats = beatInfo ?? new List<BeatInfo>();
            resetSelect();
            //BeatsSimilarity();

            UpdateDataForceWaveformRedraw();
            if (beats.Count > 0)
            {
                int sample = beats[0].Sample;
                OnSampleChanged?.Invoke(sample);
            }
        }

        private void leadSet_Click(object sender, EventArgs e)
        {
            LeadSettingForm frmLeadSetting = new LeadSettingForm();

            if (frmLeadSetting.selectedIndices.Count == 0)
            {
                frmLeadSetting.selectedIndices.AddRange(leadIdxs);
            }

            frmLeadSetting.OnLeadsSelected += (leadIndices) =>
            {
                MultiLeads = leadIndices;
                leadIdxs = new List<int>(MultiLeads);
                _graphicsControl.Invalidate();

                if (leadIdxs.Count == 3)
                {
                    OnTemplateLeadChanged?.Invoke(leadIdxs);
                }
            };

            frmLeadSetting.ShowDialog();
        }

        //重置选择
        public void resetSelect()
        {
            _isSelectAll = false;
            this.btnSelectCurPage.BaseColor = Color.FromArgb(221, 231, 245);
            this.btnSelectAll.BaseColor = Color.FromArgb(221, 231, 245);

            _selectedBeats?.Clear();

            SetCommonWave();
        }

        private void SetCommonWave()
        {
            if (showCommonWave == true)
            {
                this.btnCommonWave.BaseColor = Color.FromArgb(159, 200, 255);
            }
            else
            {
                this.btnCommonWave.BaseColor = Color.FromArgb(221, 231, 245);
            }

            OnCommonWaveShowChanged?.Invoke(showCommonWave);
        }

        private void btnSelectCurPage_Click(object sender, EventArgs e)
        {
            resetSelect();

            this.btnSelectCurPage.BaseColor = Color.FromArgb(159, 200, 255);

            List<BeatInfo> selectBeats = new List<BeatInfo>();
            for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
            {
                PageSubTemplateSelectStatus[i] = true;
                selectBeats.Add(PageBeats[i]);
            }
            _selectedBeats = selectBeats;

            _graphicsControl?.Focus();
            _graphicsControl?.Invalidate();

        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            resetSelect();
            _isSelectAll = true;
            this.btnSelectAll.BaseColor = Color.FromArgb(159, 200, 255);

            for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
            {
                PageSubTemplateSelectStatus[i] = true;
            }

            if (beats != null)
            {
                _selectedBeats = new List<BeatInfo>(beats);

                _graphicsControl?.Focus();

                _graphicsControl?.Invalidate();
            }

        }

        private void btnCommonWave_Click(object sender, EventArgs e)
        {
            showCommonWave = !showCommonWave;
            SetCommonWave();
            LayoutStyle = (BeatWaveStyle)this.cmbLayout.SelectedValue;
            _graphicsControl?.Invalidate();
        }

        protected override void OnControlKeyDown(Keys keyData)
        {
            SelectBeatChanged(keyData);
            return;
        }

        protected override void OnMixKeyDown(Keys keyData)
        {
            switch(keyData)
            {
                case Keys.Control | Keys.N:
                    QRSSortType('N');
                    break;
                case Keys.Control | Keys.V:
                    QRSSortType('V');
                    break;
                case Keys.Control | Keys.S:
                    QRSSortType('S');
                    break;
                case Keys.Control | Keys.X:
                    QRSSortType('X');
                    break;
            }
            return;
        }
    }

}
