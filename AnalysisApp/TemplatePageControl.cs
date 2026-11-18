using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp
{
    public partial class TemplatePageControl : BaseUserControl
    {
        private BeatGroup beatGroup = new BeatGroup();
        private List<ScatterType> BeatGroup = new List<ScatterType>();

        private string beatType = "N";
        private int clickIdx = 0;

        //防止重入：后台刷新进行期间收到的刷新请求不再丢弃，而是记下来在本轮结束后补跑一次
        private bool _busy = false;
        private bool _pendingReload = false;          //等待补跑的整页重载(LoadDataAsync)
        private bool _pendingReloadResetUI = false;
        private bool _pendingPageRefresh = false;     //等待补跑的页面刷新(TemplatePageRefresh)
        private int _pendingDrainDepth = 0;           //补跑层数，异常情况下避免"补跑又触发补跑"无限循环

        //上一次工具栏构建时的类型计数，用于计数未变化时跳过工具栏重建
        private List<ScatterType> _lastToolbarTypes = new List<ScatterType>();

        //心搏类型串缓存(GetKeysOfBeats 需全量遍历，按数据版本缓存，避免每次进入界面都重建)
        private static string _cachedKeys = null;
        private static int _cachedKeysVersion = -1;

        public TemplatePageControl()
        {
            InitializeComponent();
        }

        //提交待删除心搏期间置 true：AEcgHandler.DeleteBeats 内部会 refreshDataState → DataSourceState=1
        //→ 级联调用本页 RefreshData，而提交方马上会自己重建计数与分组，这次级联重载完全重复，需跳过
        private bool _suppressDataSourceReload = false;

        public override async Task RefreshData(bool resetUI = false)
        {
            if (!resetUI && _suppressDataSourceReload)
            {
                return;
            }
            this.subTemplateControl1.reSetLead();
            await LoadDataAsync(resetUI);
        }

        private void TemplatePageControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (!IsUiInitialized) InitUI();

                    //LoadData(true);
                    Console.WriteLine("模板界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心拍模板");
            }
        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                //this.subTemplateControl1.OnChangeSubTemplate += this.templateWaveViewControl1.OnChangeSubtemplate;
                this.subTemplateControl1.OnChangeSubTemplate += () => { TemplatePageRefresh(); };
                this.templateWaveViewControl1.OnSampleIdxChanged += this.subTemplateControl1.OnSampleIdxChanged;
                this.templateWaveViewControl1.OnSampleChanged += this.drawEcgCommonWave1.OnWaveformStartTimeChanged;
                this.templateWaveViewControl1.OnTemplateLeadChanged += this.drawEcgCommonWave1.OnTemplateLeadChanged;
                this.templateWaveViewControl1.OnSingleLeadChanged += this.subTemplateControl1.OnLeadChanged;
                this.templateWaveViewControl1.OnGainChanged += this.subTemplateControl1.OnGainChanged;
                this.templateWaveViewControl1.OnCommonWaveShowChanged += this.OnCommonWaveShowChanged;
                this.templateWaveViewControl1.OnDataChanged += (d) => { TemplateDataChanged(); };
                this.templateWaveViewControl1.OnResort += (d) => { TemplateResortChanged(d); };
                this.templateWaveViewControl1.OnQRSResort += () => { TemplatePageRefresh(); };
                this.drawEcgCommonWave1.OnDataChanged += (d) =>
                {
                    this.drawEcgCommonWave1.forceWaveformRedraw();
                };
                //this.drawEcgCommonWave1.OnDataChanged += (d) => {
                //    this.templateWaveViewControl1.BeatTypeChanged();
                //    this.subTemplateControl1.UpdateDataForceWaveformRedraw();
                //};
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心拍模板");
            }
        }

        private async Task LoadDataAsync(bool resetUI = false)
        {
            //切换病例/重分析(resetUI)时清空待删除标记，防止旧病例的标记误删新病例心搏；
            //普通进入则保留标记，在统计/重建分组前先提交上次遗留的待删除心搏
            if (resetUI)
            {
                this.templateWaveViewControl1?.InitDeletedFlag();
            }
            CommitPendingBeatDeletes();

            if (_busy)
            {
                //正在刷新：记录本次请求，本轮结束后补跑，避免静默丢失导致界面停在上一次的数据上
                _pendingReload = true;
                _pendingReloadResetUI |= resetUI;
                return;
            }
            _busy = true;
            try
            {
                //整页重载默认从第一个有数据的类型开始；工具栏刷新时会按 beatType 重算 clickIdx
                clickIdx = 0;

                //类型计数放到后台线程计算，避免阻塞UI
                Dictionary<string, int> typeCounts = await Task.Run(() => getScatterTypeCounts());
                BeatGroup = buildScatterTypes(typeCounts);

                beatType = "N";
                for (int i = 0; i < BeatGroup.Count; i++)
                {
                    if (BeatGroup[i].Count > 0)
                    {
                        beatType = BeatGroup[i].Code;
                        break;
                    }
                }

                rebuildBeatTypeToolbar();

                this.templateWaveViewControl1.ResetUIState(true);
                //分组重计算在后台线程执行，UI线程只做赋值与重绘
                await refreshUIAsync(beatType, 0, remapClickedGroup: false);

                if (this.drawEcgCommonWave1.IsLoaded)
                {
                    this.drawEcgCommonWave1.RefreshData(resetUI);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心拍模板");
            }
            finally
            {
                _busy = false;
                await RunPendingAsync();
            }
        }

        /// <summary>
        /// 补跑本轮刷新期间被挡下的请求（整页重载优先，其次页面刷新）
        /// </summary>
        private async Task RunPendingAsync()
        {
            if (!_pendingReload && !_pendingPageRefresh)
            {
                return;
            }

            if (_pendingDrainDepth >= 3)
            {
                //异常情况下(补跑过程又不断产生新请求)直接丢弃积压，避免无限循环卡死界面
                _pendingReload = false;
                _pendingReloadResetUI = false;
                _pendingPageRefresh = false;
                return;
            }

            _pendingDrainDepth++;
            try
            {
                if (_pendingReload)
                {
                    _pendingReload = false;
                    bool resetUI = _pendingReloadResetUI;
                    _pendingReloadResetUI = false;
                    await LoadDataAsync(resetUI);
                    return;
                }

                _pendingPageRefresh = false;
                await RefreshPageCoreAsync();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心拍模板");
            }
            finally
            {
                _pendingDrainDepth--;
            }
        }

        private void TemplateDataChanged()
        {
            this.drawEcgCommonWave1.forceWaveformRedraw();

            this.templateWaveViewControl1.BeatTypeChanged();
            this.subTemplateControl1.UpdateDataForceWaveformRedraw();
        }

        private async void TemplateResortChanged(List<int> leadIdxs)
        {
            beatGroup = await Task.Run(() => QueryBeatGroup.beatGroupByType(beatType));
            this.subTemplateControl1.ClickRectIdx = 0;
            this.subTemplateControl1.CurrentHeartBeatIdx = 0;
            this.subTemplateControl1.BeatType = beatType;
            this.subTemplateControl1.beatGroup = beatGroup;
            this.subTemplateControl1.PageSubTemplateLeadIdxs = leadIdxs;
            this.subTemplateControl1.UpdateDataForceWaveformRedraw();

            Dictionary<string, List<BeatInfo>> beatGroupData = beatGroup.Data;
            List<KeyValuePair<string, List<BeatInfo>>> beatGroupDataList = beatGroupData.ToList();
            //分组可能为空(如该类型心搏被全部删除)，直接取 [0] 会抛越界异常
            if (beatGroupDataList.Count == 0)
            {
                return;
            }
            this.templateWaveViewControl1.PageIdx = 0;
            this.templateWaveViewControl1.ClickRectIdx = 0;
            //注意：不清除删除标记——标记需保留到切换子模板/切换类型模板时统一提交删除
            this.templateWaveViewControl1.beats = beatGroupDataList[0].Value;
            this.templateWaveViewControl1.UpdateDataForceWaveformRedraw();
        }

        public async void TemplatePageRefresh()
        {
            if (_busy)
            {
                //本轮刷新结束后补跑，避免切换子模板/重分类后的刷新被丢弃、界面停在上一次的分组上
                _pendingPageRefresh = true;
                return;
            }
            await RefreshPageCoreAsync();
        }

        private async Task RefreshPageCoreAsync()
        {
            _busy = true;
            try
            {
                //切换子模板：先提交已标记删除的心搏，再统计计数/重建分组
                CommitPendingBeatDeletes();
                getScatterTypeCount();
                await refreshUIAsync(beatType, this.subTemplateControl1.ClickRectIdx + this.subTemplateControl1.PageIdx * this.subTemplateControl1.PageWaveCount);
            }
            finally
            {
                _busy = false;
                await RunPendingAsync();
            }
        }

        private void OnCommonWaveShowChanged(bool bShow)
        {
            if (bShow)
            {
                this.drawEcgCommonWave1.Show();
                this.tableLayoutPanel2.RowStyles[1].Height = 30;

            }
            else
            {
                this.drawEcgCommonWave1.Hide();
                this.tableLayoutPanel2.RowStyles[1].Height = 0;

            }
        }

        //获取心搏类型及其个数(拆分为：后台统计 + UI构建；计数未变化时跳过工具栏重建)
        private void getScatterTypeCount()
        {
            Dictionary<string, int> counts = getScatterTypeCounts();
            BeatGroup = buildScatterTypes(counts);
            rebuildBeatTypeToolbar();
        }

        //统计各心搏类型的个数：单次遍历替代原先的16次 Regex.Matches 扫描，
        //并且类型串(GetKeysOfBeats 需全量遍历)按数据版本缓存
        private Dictionary<string, int> getScatterTypeCounts()
        {
            if (AnaValues.CachedAllBeats == null || AnaValues.CachedAllBeats.Count == 0)
            {
                return new Dictionary<string, int>();
            }

            int version = AnaValues.BeatDataVersion;
            string keys = _cachedKeys;
            if (keys == null || _cachedKeysVersion != version)
            {
                keys = AnaCode.GetKeysOfBeats(AnaValues.CachedAllBeats);
                _cachedKeys = keys;
                _cachedKeysVersion = version;
            }

            Dictionary<char, int> charCounts = new Dictionary<char, int>();
            foreach (char c in keys)
            {
                if (charCounts.ContainsKey(c))
                {
                    charCounts[c]++;
                }
                else
                {
                    charCounts[c] = 1;
                }
            }

            Dictionary<string, int> counts = new Dictionary<string, int>(charCounts.Count);
            foreach (KeyValuePair<char, int> kvp in charCounts)
            {
                counts[kvp.Key.ToString()] = kvp.Value;
            }
            return counts;
        }

        private static int GetCount(Dictionary<string, int> counts, string code)
        {
            int n;
            return counts.TryGetValue(code, out n) ? n : 0;
        }

        //构建类型列表(UI线程，仅15项，开销极小)
        private List<ScatterType> buildScatterTypes(Dictionary<string, int> counts)
        {
            List<ScatterType> types = new List<ScatterType>();
            if (AnaValues.CachedAllBeats == null || AnaValues.CachedAllBeats.Count == 0)
            {
                return types;
            }

            // 计算V类型总数（合并V、V2、V3）
            int vTotalCount = GetCount(counts, "V");

            // 计算S类型总数（合并S、S2、S3）
            int sTotalCount = GetCount(counts, "S");

            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["N"], Code = "N", Desc = _langs["正常"], Count = GetCount(counts, "N") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["V"], Code = "V", Desc = _langs["室早"], Count = vTotalCount });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["T"], Code = "T", Desc = _langs["R on T"], Count = GetCount(counts, "T") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["S"], Code = "S", Desc = _langs["房早"], Count = sTotalCount });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["J"], Code = "J", Desc = _langs["交界性早搏"], Count = GetCount(counts, "J") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["E"], Code = "E", Desc = _langs["室性逸博"], Count = GetCount(counts, "E") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["G"], Code = "G", Desc = _langs["交界性逸博"], Count = GetCount(counts, "G") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["W"], Code = "W", Desc = _langs["房性逸博"], Count = GetCount(counts, "W") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["X"], Code = "X", Desc = _langs["伪差"], Count = GetCount(counts, "X") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["K"], Code = "K", Desc = _langs["房颤"], Count = GetCount(counts, "K") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["M"], Code = "M", Desc = _langs["房扑"], Count = GetCount(counts, "M") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["O"], Code = "O", Desc = _langs["心房起搏"], Count = GetCount(counts, "O") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["P"], Code = "P", Desc = _langs["心室起搏"], Count = GetCount(counts, "P") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["H"], Code = "H", Desc = _langs["房室顺序起搏"], Count = GetCount(counts, "H") });
            types.Add(new ScatterType { Title = AnaValues.heartbeatTypeMap["F"], Code = "F", Desc = _langs["室性融合波"], Count = GetCount(counts, "F") });
            return types;
        }

        //类型计数与上次相同则不重建工具栏，只更新选中态
        private bool IsSameToolbarTypes(List<ScatterType> types)
        {
            if (_lastToolbarTypes.Count == 0 || types.Count != _lastToolbarTypes.Count)
            {
                return false;
            }
            for (int i = 0; i < types.Count; i++)
            {
                if (types[i].Code != _lastToolbarTypes[i].Code || types[i].Count != _lastToolbarTypes[i].Count)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 按当前心搏类型(beatType)刷新工具栏选中态。
        /// 工具栏只显示 Count&gt;0 的类型；某类型心搏被全部删除并提交后数据版本变化，
        /// 工具栏会重建，排在它后面的按钮整体前移。此时若仍用位置索引(clickIdx)标记，
        /// 高亮会错位到后面的类型，因此必须以类型 Code 定位。
        /// </summary>
        private void updateBeatTypeSelection()
        {
            //先在(可能已重建的)工具栏中按类型 Code 找到当前类型的新位置
            int selectedIdx = -1;
            for (int i = 0; i < this.tsBeatType.Items.Count; i++)
            {
                ScatterToolStripButton it = this.tsBeatType.Items[i] as ScatterToolStripButton;
                if (it != null && it.ZqScatterType != null && it.ZqScatterType.Code == beatType)
                {
                    selectedIdx = i;
                    break;
                }
            }

            //当前类型已被删空(按钮不再存在)：回退到第一个可用类型并同步 beatType，
            //避免"显示空数据、却把后面的类型标成选中"
            if (selectedIdx < 0 && this.tsBeatType.Items.Count > 0)
            {
                ScatterToolStripButton first = this.tsBeatType.Items[0] as ScatterToolStripButton;
                if (first != null && first.ZqScatterType != null)
                {
                    selectedIdx = 0;
                    beatType = first.ZqScatterType.Code;
                }
            }

            for (int i = 0; i < this.tsBeatType.Items.Count; i++)
            {
                ScatterToolStripButton it = this.tsBeatType.Items[i] as ScatterToolStripButton;
                if (it != null)
                {
                    it.UserSelected = (i == selectedIdx);
                }
            }

            //clickIdx 降级为当前选中位置的镜像，不再是选中态的判断依据
            clickIdx = selectedIdx >= 0 ? selectedIdx : 0;
        }

        private void rebuildBeatTypeToolbar()
        {
            List<ScatterType> types = BeatGroup;
            if (types == null || types.Count == 0)
            {
                return;
            }

            if (IsSameToolbarTypes(types))
            {
                updateBeatTypeSelection();
                return;
            }
            _lastToolbarTypes = new List<ScatterType>(types);

            this.tsBeatType.SuspendLayout();
            this.tsBeatType.Items.Clear();
            float fontSize = 9F * Math.Max(0.75F, CommonValues.HeightScale);
            using (Font textFont = new Font(this.tsBeatType.Font.FontFamily, fontSize))
            {
                int maxCount = BeatGroup.Count > 0 ? BeatGroup.Max(g => g.Count) : 0;
                ScatterToolStripButton tsItem = new ScatterToolStripButton(maxCount.ToString());
                tsItem.Font = textFont;
                int maxWidth = Math.Max(46, tsItem.GetPreferredSize(new Size(100, 40)).Width);

                for (int i = 0; i < BeatGroup.Count; i++)
                {
                    ScatterType beatType = BeatGroup[i];
                    if (beatType.Count > 0)
                    {
                        ScatterToolStripButton item = new ScatterToolStripButton(beatType.Count.ToString())
                        {
                            Name = "tsBtn" + beatType.Code,
                            ToolTipText = beatType.Desc,
                            ZqScatterType = beatType,
                            AutoSize = false,
                            ImageScaling = ToolStripItemImageScaling.None,
                            TextAlign = ContentAlignment.MiddleRight,
                            TextImageRelation = TextImageRelation.ImageAboveText,
                            TitleFontSize = 14,
                        };
                        item.Size = new Size(maxWidth, 40);
                        //item.CodeRect = new RectangleF((tsBeatType.Width - item.Width / 2) / 2F, 2, item.Width / 2, item.Height / 2);
                        item.Click += beatType_Click;
                        this.tsBeatType.Items.Add(item);
                    }
                }
            }
            this.tsBeatType.ResumeLayout(true);
            this.tsBeatType.Refresh();

            updateBeatTypeSelection();
        }

        private async void beatType_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                _pendingPageRefresh = true;
                return;
            }
            _busy = true;
            try
            {
                //切换类型模板：先提交已标记删除的心搏，再统计计数/重建分组
                CommitPendingBeatDeletes();

                ScatterToolStripButton curItem = sender as ScatterToolStripButton;
                if (curItem == null || curItem.ZqScatterType == null)
                {
                    return;
                }

                //先取出被点击的类型并赋值 beatType，再重建工具栏。
                //提交删除会让某类型计数归零(数据版本递增)，工具栏按最新计数重建后
                //项数与顺序都可能变化：旧按钮对象被移出 Items、旧位置索引整体前移，
                //故不再在此处直接标记/记录索引，统一由 updateBeatTypeSelection 按类型 Code 定位。
                beatType = curItem.ZqScatterType.Code;
                //即时反馈(此时按钮仍在工具栏中)；最终选中态以重建后的 updateBeatTypeSelection 为准
                curItem.UserSelected = true;
                getScatterTypeCount();

                //类型已切换，无需按旧分组键重新定位
                await refreshUIAsync(beatType, 0, remapClickedGroup: false);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心拍模板");
            }
            finally
            {
                _busy = false;
                await RunPendingAsync();
            }
        }

        public int GetDeletedFlagCount()
        {
            return this.templateWaveViewControl1.GetDeletedFlagCount();
        }

        /// <summary>
        /// 把模板区已标记待删除的心搏真正删除。
        /// 在切换子模板/切换类型模板重建分组之前调用；删除失败时标记保留，下次切换时重试。
        /// </summary>
        public void CommitPendingBeatDeletes()
        {
            try
            {
                //级联抑制必须覆盖整个 DeleteBeats 调用(事件在同一线程同步重入)
                _suppressDataSourceReload = true;
                int ret = this.templateWaveViewControl1?.CommitPendingDeletes() ?? 0;
                if (ret != 0)
                {
                    CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心拍模板");
            }
            finally
            {
                _suppressDataSourceReload = false;
            }
        }

        private async Task refreshUIAsync(string beatType, int clickRectIdx, bool remapClickedGroup = true)
        {
            //切换子模板/类型模板：先提交删除标记对应的心搏，再按删除后的数据重建分组
            CommitPendingBeatDeletes();

            //记录提交前点击的分组键(同一类型内切换子模板时，用于删除后重新定位，避免索引漂移)
            string clickedKey = null;
            if (remapClickedGroup && clickRectIdx >= 0 && beatGroup?.Data != null)
            {
                var oldList = beatGroup.Data.ToList();
                if (clickRectIdx < oldList.Count)
                {
                    clickedKey = oldList[clickRectIdx].Key;
                }
            }

            //分组重计算放后台线程，避免阻塞UI
            BeatGroup bg = await Task.Run(() => QueryBeatGroup.beatGroupByType(beatType));
            beatGroup = bg;

            Dictionary<string, List<BeatInfo>> beatGroupData = beatGroup.Data;
            List<KeyValuePair<string, List<BeatInfo>>> beatGroupDataList = beatGroupData.ToList();

            //按提交前点击的分组键重新定位(分组可能因删除而变小、被合并甚至消失)
            if (clickedKey != null && beatGroupData.ContainsKey(clickedKey))
            {
                int remapped = beatGroupDataList.FindIndex(kvp => kvp.Key == clickedKey);
                if (remapped >= 0)
                {
                    clickRectIdx = remapped;
                }
            }
            if (clickRectIdx < 0 || clickRectIdx > beatGroupDataList.Count - 1)
            {
                clickRectIdx = 0;   //原分组已不存在，回退到第一组
            }

            this.subTemplateControl1.ClickRectIdx = clickRectIdx;
            this.subTemplateControl1.CurrentHeartBeatIdx = 0;
            this.subTemplateControl1.BeatType = beatType;
            this.subTemplateControl1.beatGroup = beatGroup;
            this.subTemplateControl1.UpdateDataForceWaveformRedraw();

            this.templateWaveViewControl1.PageIdx = 0;
            this.templateWaveViewControl1.ClickRectIdx = 0;
            this.templateWaveViewControl1.beats = beatGroupDataList[clickRectIdx].Value;
            this.templateWaveViewControl1.RefreshData();

        }
    }


    public class BeatGroup
    {
        public Dictionary<string, List<BeatInfo>> Data { get; set; }
        public int Count { get; set; }

        public BeatGroup()
        {
            Data = new Dictionary<string, List<BeatInfo>>();
        }
    }

    public static class QueryBeatGroup
    {
        public static List<int> smallSubType = new List<int>();

        //分组结果缓存：QueryGroup 需全量扫描所有心搏(O(N×M))，开销大。
        //按数据版本(AnaValues.BeatDataVersion)缓存扫描结果，数据未变化时直接复用
        private static readonly object _syncRoot = new object();
        private static int _cacheVersion = -1;
        private static readonly Dictionary<string, BeatGroup> _rawGroupCache = new Dictionary<string, BeatGroup>();

        private static BeatGroup getRawGroupData(string type)
        {
            if (_cacheVersion != AnaValues.BeatDataVersion)
            {
                _rawGroupCache.Clear();
                _cacheVersion = AnaValues.BeatDataVersion;
            }

            BeatGroup cached;
            if (_rawGroupCache.TryGetValue(type, out cached))
            {
                return cached;
            }

            BeatGroup group = QueryGroup(type);
            _rawGroupCache[type] = group;
            return group;
        }

        public static BeatGroup beatGroupByType(string type)
        {
            lock (_syncRoot)
            {
                smallSubType.Clear();

                BeatGroup raw = getRawGroupData(type);

                //在副本上做合并/排序，避免污染缓存中的原始分组数据
                Dictionary<string, List<BeatInfo>> groupData = new Dictionary<string, List<BeatInfo>>(raw.Data.Count);
                foreach (var kvp in raw.Data)
                {
                    groupData[kvp.Key] = new List<BeatInfo>(kvp.Value);
                }

                if (!groupData.ContainsKey("-1"))
                {
                    groupData["-1"] = new List<BeatInfo>();
                }

                // 处理count小于10的键值对，将其值合并到"-1"中
                List<string> keysToRemove = new List<string>();

                foreach (var kvp in groupData.ToList()) // 使用ToList避免修改集合时的异常
                {
                    string key = kvp.Key;
                    List<BeatInfo> valueList = kvp.Value;

                    // 跳过"-1"键本身
                    if (key == "-1")
                    {
                        smallSubType.Add(Convert.ToInt32(key));

                        continue;
                    }

                    // 如果count小于10，则合并到"-1"中
                    if (valueList.Count < 10 && Convert.ToInt32(key) < 200)
                    {
                        smallSubType.Add(Convert.ToInt32(key));
                        // 将当前键的值添加到"-1"中
                        groupData["-1"].AddRange(valueList);
                        // 标记当前键需要移除
                        keysToRemove.Add(key);
                    }
                }

                // 移除已合并的键
                foreach (string key in keysToRemove)
                {
                    groupData.Remove(key);
                }

                // 按照 List 数据个数由大到小排序
                var sortedGroupData = groupData.OrderByDescending(kvp => kvp.Value.Count)
                                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return new BeatGroup { Count = raw.Count, Data = sortedGroupData };
            }
        }

        public static BeatGroup QueryGroup(string type)
        {
            BeatGroup group = new BeatGroup { Count = 0 };
            List<BeatInfo> cachedAllBeats = AnaValues.CachedAllBeats;
            List<AEcgHandler.SegmentInfo> unanalyzableSegments = AnaValues.UnanalyzableSegments;
            bool hasSegments = unanalyzableSegments != null && unanalyzableSegments.Count > 0;

            for (int i = 0; i < cachedAllBeats.Count; i++)
            {
                BeatInfo item = cachedAllBeats[i];
                //// 跳过X类型的数据
                //if (item.Length > 7 && item[7]?.ToString() == "X")
                //{
                //    continue;
                //}
                bool isValid = false;
                //无不可分析段时直接跳过内层循环(原先每拍都要遍历段落列表，O(N×M))
                if (hasSegments)
                {
                    for (int idx = 0; idx < unanalyzableSegments.Count; idx++)
                    {
                        if (item.StartIndex < unanalyzableSegments[idx].EndIndex && item.EndIndex > unanalyzableSegments[idx].StartIndex)
                        {
                            isValid = true;
                            break;
                        }
                    }
                }
                if (isValid) continue;
                string itemType = item.BeatUiType.ToString();

                if (type.Contains(itemType))
                {
                    int morphologyType = item.MorphologyType;
                    string key = morphologyType.ToString();
                    if (!group.Data.ContainsKey(key))
                    {
                        group.Data[key] = new List<BeatInfo>();
                    }
                    group.Data[key].Add(item);
                    group.Count++;
                }
            }
            return group;
        }
    }
}
