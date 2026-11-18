using AnalysisApp.Commons;
using AnalysisSystem.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisApp
{
    public partial class AnalysisWindow : BaseDialogForm
    {
        private PipeServer _pipeServer;
        private int _ownerHandle;
        private int _userId;
        private examCase _caseInfo;

        private Dictionary<CustomToolStripButton, int> _tabMapping; //按钮与Tab页映射集合
        private Dictionary<CustomToolStripButton, BaseUserControl> _pageMapping; //按钮与功能页映射集

        private bool _isLoaded = false, _isInitialized = false;
        private static bool? _gpuProbeResult = null; //GPU 探测结果缓存, 整个进程只探测一次
        private bool _allowVisible = false; // 控制窗口是否允许显示的标志
        private bool _laidOut = false; // 窗口中的控件是否已初始化完成,包括布局
        private System.Threading.Timer _monitorTimer;
        private DateTime _lastActivityTime = DateTime.Now;

        public static AnalysisWindow Instance { get; private set; }

        public AnalysisWindow(int userId, int ownerHandle) : base("", "", false, false)
        {
            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
            //             ControlStyles.AllPaintingInWmPaint |
            //             ControlStyles.UserPaint, true);

            InitializeComponent();

            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            this.Size = new System.Drawing.Size(MaximizedBounds.Width, MaximizedBounds.Height);
            // 强制创建句柄
            if (!this.IsHandleCreated) this.CreateHandle();

            this.tabControl1.ApplyHeaderlessStyle = true;
            this.tabControl1.SelectedIndex = 0;

            _userId = userId;
            _ownerHandle = ownerHandle;
            Instance = this;
#if STANDALONE
            _allowVisible = true;
#else
            // 先将WindowState设置为Normal，避免Maximized导致的自动显示
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = false;
#endif  

            _pipeServer = new PipeServer("zoncare-holter-bp");
            _pipeServer.OnClientConnected += () => { _pipeServer.SendMessage($"subprogramHwd|{this.Handle.ToInt32()}|false"); };
            _pipeServer.OnMessageReceived += processMainProgramMessage;
            _pipeServer.Start();

            if (!DesignMode)
            {
                AppCode.InitializeSystemSettings();
                PubFuns.AsyncDoWork(prepareDBContext);
                PubFuns.DelayDoWork(() =>
                {
                    this.InvokeIfRequired(() => base.OnLoad(null));
                    return true;
                }, 200, 1, null);
            }
        }

        //public new void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);

        //    this.Visible = false;

        //    //Timer testTimer = new Timer() { Interval = 3000 };
        //    //testTimer.Tick += (o, args) =>
        //    //{
        //    //    this.Visible = true;
        //    //    initData();
        //    //    testTimer.Stop();
        //    //};
        //    //testTimer.Start();
        //}

        private void AnalysisApp_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            if (_isLoaded) return;

            try
            {
                init();

                this.Deactivate += (o, args) => { setParent(); };

                // 添加键盘事件处理程序
                //this.KeyPreview = true;
                this.KeyDown += new KeyEventHandler(OnKeyDown);
                Application.Idle += (o, args) => { _lastActivityTime = DateTime.Now; };

                AnaValues.OnDataSourceChanged += (shouldReset) =>
                {
                    foreach (var key in _pageMapping.Keys)
                    {
                        _pageMapping[key].DataSourceTracked = false;

                        //页面需要重置的情况下，不改变其状态
                        if (_pageMapping[key].ShouldReset) { }
                        else
                        {
                            _pageMapping[key].ShouldReset = shouldReset;
                        }
                    }
                };

                _isLoaded = true;

#if STANDALONE
            loadData();
            showMe();
#else
                this.Hide();
#endif
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "分析模块");
            }
        }

        private void AnalysisWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _allowVisible = false;
            _monitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
            AnaCode.SaveAnalyzeData(true);

#if !STANDALONE
            // 先激活主窗体、再隐藏自己：
            if (_ownerHandle > 0)
            {
                IntPtr hWndOwner = new IntPtr(_ownerHandle);
                WindowUtils.ActivateAndShowWindow(hWndOwner);
            }
            this.Hide();
            _pipeServer.SendMessage($"subprogramHwd|{this.Handle.ToInt32()}|false");
            e.Cancel = true;
#endif
        }

        private void AnalysisWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            _pipeServer?.Stop();
            AEcgHandler.DeInit();
        }

        private void AnalysisWindow_Activated(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(setParent));
            ResetMaxWindowState();
        }

        private void AnalysisWindow_Shown(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Maximized) return;

            PubFuns.AsyncDoWork(checkSkiaGpuAcceleration);
            //_pipeServer.SendMessage($"subprogramHwd|{this.Handle.ToInt32()}|{this.Visible}");

            foreach (BaseUserControl ucPage in _pageMapping.Values)
            {
                ucPage.OnParentFormShown(e);
            }
            ResetMaxWindowState();

            Console.WriteLine($"Analysis app main window is shown");
        }

        /// <summary>
        /// 键盘事件处理程序 - 按ESC键退出全屏模式
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            int tabIndex = this.tabControl1.SelectedIndex;
            if (tabIndex == 1 && _pageMapping != null && _pageMapping.TryGetValue(btnPageScan, out var mappedPage))
            {
                PageScanControl ctlPageScan = mappedPage as PageScanControl;
                ctlPageScan?.OnKeyDown(sender, e);
            }
        }


        private void init()
        {
            if (_isInitialized == false)
            {
                _tabMapping = new Dictionary<CustomToolStripButton, int>() {
                    { this.btnInfo, 0 },
                    { this.btnPageScan, 1 },
                    { this.btnScatter, 2 },
                    {this.btnHistogram, 3 },
                    {this.btnAf, 4 },
                    {this.btnTemplate, 5 },
                    {this.btnEvent, 6 },
                    {this.btnSegment, 7 },
                    {this.btnEcg, 8 },
                    {this.btnAnalyse, 9 },
                    {this.btnResetana, 10 },
                    {this.btnBlood, 11 },
                    {this.btnHourlist, 12 },
                    {this.btnSummary, 13 },
                    {this.btnReport, 14 }
                };
                _pageMapping = new Dictionary<CustomToolStripButton, BaseUserControl>();

                _monitorTimer = new System.Threading.Timer(checkDataAndAppState, null, 0, 1000);

                CommonValues.MainTreadContext = SynchronizationContext.Current;
                AEcgHandler.Init();
                FullWaveForm fullwaveForm = new FullWaveForm();
                BpCompareForm bpCompareForm = new BpCompareForm();

                TemplateOverlayForm templateOverlayForm = new TemplateOverlayForm();
                initUI();

                _isInitialized = true;
            }
        }

        private void initUI()
        {
            this.SuspendLayout();
            //导航栏按钮
            this.toolStrip1.Renderer = new ToolStripBackcolorRenderer();
            //减少 SwitchTabPage 中 toolStrip1.Refresh() 刷新时的闪烁
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(this.toolStrip1, true, null);

            foreach (ToolStripItem item in this.toolStrip1.Items)
            {
                if (item is CustomToolStripButton buttonItem)
                {
                    buttonItem.TextColor = System.Drawing.Color.FromArgb(49, 56, 72);

                    string imgName = item.Name.Substring(3).ToLower();
                    string imgNameA = item.Name.Substring(3).ToLower() + "A";
                    buttonItem.InactiveImage = CommonFuns.GetImageFromFile(PubFuns.GetBaseDirectory() + "\\images\\" + imgName + ".png", false, false, 20, 20);
                    buttonItem.ActiveImage = CommonFuns.GetImageFromFile(PubFuns.GetBaseDirectory() + "\\images\\" + imgNameA + ".png", false, false, 20, 20);
                    item.Image = buttonItem.InactiveImage;
                    //int preferredHeight = item.Height;
                    //if (item.Width < 60)
                    //{
                    //    item.Width = 60;
                    //    item.Height = preferredHeight;
                    //}

                    buttonItem.Click += (sender, e) => { SwitchTabPage(buttonItem.Name); };
                }
            }

            this.InvokeIfRequired(() =>
            {
                tabControl1.SuspendLayout();
                foreach (ToolStripItem item in this.toolStrip1.Items)
                {
                    if (item is CustomToolStripButton buttonItem) SwitchTabPage(buttonItem.Name, true, false);
                }
                tabControl1.ResumeLayout(true);
                tabControl1.SelectedIndex = _tabMapping[btnResetana];
                Console.WriteLine($"当前Tab页索引: {tabControl1.SelectedIndex}");

                this.ResumeLayout(true);
            });
        }

        private int loadData()
        {
            int result = -1;
            checkDbConnction();

            CommonValues.SystemSettings = ZqQuery.loadSettings(_userId);
            AnaValues.CurrentBpDetails = ZoncareEntities.Instance.BpDetails.AsNoTracking().Where(o => o.caseId == _caseInfo.id).ToList();
#if STANDALONE
            int caseId = 13;
            _caseInfo = AnaValues.CurrentCase = ZoncareEntities.Instance.ExamCases.AsNoTracking().FirstOrDefault(o => o.id == caseId);
#endif
            AnaValues.RootPath = _caseInfo.filePath;
            AnaValues.RootPath = Path.GetFullPath(AnaValues.RootPath);
            if (_caseInfo.checkItem == 1 || _caseInfo.checkItem == 3)
            {
                string holterinfoPath = Path.Combine(AnaValues.RootPath, "holter.info");
                if (File.Exists(holterinfoPath))
                {
                    HolterConfig holterConfig = HolterConfig.LoadFromFile(holterinfoPath);
                    AnaValues.holterConfig = holterConfig;

                    int confSet = AEcgHandler.SetConfig(AnaValues.RootPath);
                    int loadStatu = AEcgHandler.LoadData();
                    string beatDataFilePath = Path.Combine(AnaValues.RootPath, "beats.dat");
                    //if (File.Exists(beatDataFilePath))
                    {
                        if (loadStatu == 0)
                        {
                            //获取不可分析段
                            AnaValues.UnanalyzableSegments = AEcgHandler.GetSegmentUnanalyzable();
                            //获取所有心搏
                            List<AEcgHandler.AEcgBeatInfo> beats = AEcgHandler.GetBeatsInfo();
                            AnaValues.CachedAllBeats = beats.ConvertToSystemBeats(holterConfig.SampleRate);
                            result = 0;
                        }
                        else
                        {
                            BeatDataReader beatDataReader = new BeatDataReader(beatDataFilePath, holterConfig.SampleRate);
                            AnaValues.CachedAllBeats = beatDataReader.ReadAllBeats();
                        }
                        AnaValues.CurrentBeatMap = AnaCode.GetBeatMap(AnaValues.CachedAllBeats);

                        AnaCode.ResolveLeadQuality();

                        if (holterConfig.StartReanalysis) { AnaCode.LoadStatisticsData(); }
                    }

                    //string ecgDataFilePath = Path.Combine(AnaValues.RootPath, "ecg_revised.dat");
                    //if (File.Exists(ecgDataFilePath))
                    //{
                    //    AnaValues.HolterDataReader?.Dispose();
                    //    AnaValues.HolterDataReader = new EcgDataReader(ecgDataFilePath);
                    //}
                }
            }
            else
            {
                result = 0;
            }

            return result;
        }

        /// <summary>
        /// 根据工具栏按钮显示相应的设置界面
        /// </summary>
        /// <param name="toolButtonName">工具栏按钮名称</param>
        /// <param name="enableAll">是否开启所有页面功能</param>
        /// <param name="showTargetPage">是否跳转到目标页面</param>
        public void SwitchTabPage(string toolButtonName, bool enableAll = true, bool showTargetPage = true)
        {
            CustomToolStripButton curBtn = _tabMapping.Keys.First(o => o.Name == toolButtonName);
            if (showTargetPage)
            {
                foreach (ToolStripItem tsItem in this.toolStrip1.Items)
                {
                    if (_caseInfo.checkItem == 1 || _caseInfo.checkItem == 3) //含心电检查项
                    {
                        if (AnaValues.holterConfig.StartReanalysis)
                        {
                            tsItem.Enabled = true;
                        }
                        else
                        {
                            if (!enableAll)
                            {
                                tsItem.Enabled = (tsItem == this.btnResetana || tsItem == this.btnInfo);
                            }
                            else
                            {
                                //tsItem.Enabled = true;
                            }
                        }
                    }
                    CustomToolStripButton btn = tsItem as CustomToolStripButton;
                    if (btn != null)
                    {
                        btn.UserSelected = false;
                        if (_pageMapping.ContainsKey(btn)) _pageMapping[btn].IsActive = false;
                    }
                }

                curBtn.UserSelected = true;
                this.toolStrip1.Refresh();
            }

            int targetIndex = _tabMapping[curBtn];
            bool mapped = _pageMapping.TryGetValue(curBtn, out BaseUserControl funcControl);
            if (!mapped)
            {
                funcControl = CreatePageWithFallback(curBtn, targetIndex);
                if (funcControl == null)
                {
                    //页面创建失败(已记录日志并回退软件渲染重试), 本次切换中止
                    AnaCode.SaveAnalyzeData();
                    return;
                }
            }

            if (showTargetPage)
            {
                try
                {
                    this.tabControl1.SelectedIndex = targetIndex;
                    if (enableAll) _pageMapping[curBtn].IsActive = true;
                }
                catch (Exception ex)
                {
                    //切换显示时句柄重建可能再次触发 GL 上下文创建失败:
                    //记录日志并关闭 GPU 渲染, 避免异常冒泡成未处理异常
                    Logger.CreateLog(ex, $"分析模块:切换到页面[{curBtn.Name}]失败");
                    CommonValues.IsGpuAvailable = false;
                }
            }
            if (AnaValues.DataSourceState > 0 || curBtn.Name == "btnTemplate")
            {
                if (!funcControl.DataSourceTracked || curBtn.Name == "btnTemplate")
                {
                    funcControl.OnDataSourceChanged(AnaValues.DataSourceState == 2 || funcControl.ShouldReset);
                    funcControl.DataSourceTracked = true;
                    funcControl.ShouldReset = false;
                }

                if (_pageMapping.All(o => o.Value.DataSourceTracked == true))
                {
                    AnaValues.DataSourceState = 0;
                }
            }

            if(curBtn.Name != "btnTemplate")
            {
                CustomToolStripButton templateBtn = _tabMapping.Keys.First(o => o.Name == "btnTemplate");
                if (_pageMapping.ContainsKey(templateBtn))
                {
                    TemplatePageControl templatePage = _pageMapping[templateBtn] as TemplatePageControl;
                    if(templatePage.GetDeletedFlagCount() > 0)
                    {
                        templatePage.CommitPendingBeatDeletes();
                        funcControl.OnDataSourceChanged(AnaValues.DataSourceState == 2 || funcControl.ShouldReset);
                        funcControl.DataSourceTracked = true;
                        funcControl.ShouldReset = false;
                    }
                }

            }

            AnaCode.SaveAnalyzeData();
        }

        /// <summary>
        /// 创建功能页, 若 GL 上下文创建失败则关闭 GPU 渲染并回退软件渲染重试一次
        /// </summary>
        private BaseUserControl CreatePageWithFallback(CustomToolStripButton curBtn, int targetIndex)
        {
            TabPage targetPage = this.tabControl1.TabPages[targetIndex];

            for (int attempt = 0; attempt < 2; attempt++)
            {
                BaseUserControl page = CreateFuncPage(curBtn);
                if (page == null) return null;

                page.Dock = DockStyle.Fill;
                try
                {
                    targetPage.Controls.Add(page);
                    PreloadTabPage(targetPage);
                    _pageMapping[curBtn] = page;
                    return page;
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, $"分析模块:页面[{curBtn.Name}]创建失败(第{attempt + 1}次尝试)");

                    //多见于 wglCreateContext 失败(系统资源不足):
                    //关闭 GPU 渲染, 释放半成品页面及其 GL 上下文, 第二次尝试改用软件渲染
                    CommonValues.IsGpuAvailable = false;
                    try
                    {
                        targetPage.Controls.Remove(page);
                        page.Dispose(); //级联释放页内 SKGLControl 及其 GL 上下文
                    }
                    catch (Exception ex2)
                    {
                        Logger.CreateLog(ex2, "分析模块");
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 按按钮名称创建对应的功能页实例
        /// </summary>
        private BaseUserControl CreateFuncPage(CustomToolStripButton curBtn)
        {
            switch (curBtn.Name)
            {
                case "btnInfo": //患者信息
                    return new PatientInfo(AnaValues.CurrentCase);
                case "btnPageScan": //页扫描
                    return new PageScanControl();
                case "btnScatter":  //散点图
                    return new ScatterPage();
                case "btnHistogram": //直方图
                    return new HistogramPage();
                case "btnAf":       //房颤房扑
                    return new AfPage();
                case "btnTemplate": //模板编辑
                    return new TemplatePageControl();
                case "btnEvent":    //事件分析
                    return new EventPageControl();
                case "btnSegment":  //片段图
                    return new EcgSegmentControl();
                case "btnEcg":      //诊断图
                    return new EcgDiagnosisControl();
                case "btnAnalyse": //分析工具
                    return new AnalyseToolsPage();
                case "btnResetana": //重分析
                    return new ReanalysisPage();
                case "btnBlood":    //血压分析
                    return new BpAnalysisPage();
                case "btnHourlist": //小时数据表
                    return new HourlistPageControl();
                case "btnSummary": //汇总
                    return new SummaryPageControl();
                case "btnReport": //报告
                    return new ReportPageControl();
                default:
                    return null;
            }
        }

        private void PreloadTabPage(TabPage page)
        {
            page.SuspendLayout();

            // 强制创建所有控件句柄
            page.CreateControl();
            IntPtr pageHandle = page.Handle;

            // 遍历所有子控件
            foreach (Control ctl in page.Controls)
            {
                // 确保控件已创建句柄
                if (!ctl.IsHandleCreated)
                {
                    IntPtr handle = ctl.Handle;
                    ctl.CreateControl();
                    if (ctl is BaseUserControl uc && ctl.Handle != IntPtr.Zero)
                    {
                        Console.WriteLine(ctl.Name + " handle created");

                        if (!uc.IsLoaded)
                        {
                            //typeof(BaseUserControl).InvokeMember(
                            //    "OnLoad",
                            //    System.Reflection.BindingFlags.NonPublic |
                            //    System.Reflection.BindingFlags.Instance |
                            //    System.Reflection.BindingFlags.InvokeMethod,
                            //    null, uc, new object[] { EventArgs.Empty });

                            typeof(BaseUserControl).InvokeMember(
                                "InitUI",
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.InvokeMethod,
                                null, uc, null);
                        }
                    }
                }

                // 如果是容器控件，递归预加载
                if (ctl.HasChildren)
                {
                    PreloadControls(ctl);
                }
                //ctl.PerformLayout();
            }

            page.ResumeLayout(false);
            page.PerformLayout();
        }

        private void PreloadControls(Control parent)
        {
            parent.SuspendLayout();
            foreach (Control ctl in parent.Controls)
            {
                if (!ctl.IsHandleCreated)
                {
                    ctl.CreateControl();
                    IntPtr handle = ctl.Handle;
                }

                if (ctl.HasChildren)
                {
                    PreloadControls(ctl);
                }
            }
            parent.ResumeLayout(false);
            parent.PerformLayout();
        }

        private void processMainProgramMessage(string msg)
        {
            if (msg.StartsWith("loadData")) //数据加载
            {
                _pipeServer.SendMessage($"dataloaded|{202}|{this.Handle.ToInt32()}");
                int caseId = Convert.ToInt32(msg.Split('|')[1]);

                this.InvokeIfRequired(() => init());

                //数据加载是重活(DB查询/心电数据解析/心搏统计), 移到后台线程执行,
                //避免阻塞 UI 线程导致首次打开界面卡顿
                int hwnd = this.Handle.ToInt32(); //句柄已在构造函数强制创建, 先取好供后台线程使用
                Task.Run(() =>
                {
                    bool loaded = false;
                    try
                    {
                        checkDbConnction();
                        ZoncareEntities.Instance.RefreshCache();

                        _caseInfo = AnaValues.CurrentCase = ZoncareEntities.Instance.ExamCases.AsNoTracking().FirstOrDefault(o => o.id == caseId);
                        if (_caseInfo != null)
                        {
                            int ret = loadData();
                            if (ret == 0)
                            {
                                AnaValues.DataSourceState = 2;
                                loaded = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex, "分析模块");
                    }

                    //加载完成后统一回传结果(200成功/400失败), 主进程依赖此消息时序
                    if (loaded)
                        _pipeServer.SendMessage($"dataloaded|{200}|{hwnd}");
                    else
                        _pipeServer.SendMessage($"dataloaded|{400}|{hwnd}");
                });
            }
            else if (msg.StartsWith("caseId")) //显示分析模块界面
            {
                int caseId = Convert.ToInt32(msg.Split('|')[1]);
                _caseInfo = AnaValues.CurrentCase = ZoncareEntities.Instance.ExamCases.AsNoTracking().FirstOrDefault(o => o.id == caseId);

                if (_caseInfo != null)
                {
                    this.InvokeIfRequired(async () =>
                    {
                        showMe();

                        _allowVisible = true;
                        this.TopMost = true;
                        this.Visible = true;
                        this.WindowState = FormWindowState.Maximized;
                        this.Activate(); //BaseUserControl第一次加载时将触发Load事件

                        while (!_laidOut) await Task.Delay(1);
                        _pipeServer.SendMessage($"subprogramHwd|{this.Handle.ToInt32()}|true");

                        await Task.Delay(200);
                        if (_allowVisible && this.IsHandleCreated && !this.IsDisposed)
                        {
                            this.TopMost = false;
                        }
                    });
                }
            }
        }

        private void showMe()
        {
            this.Text = string.Concat(_caseInfo.name, " - ", _caseInfo.caseNo);
            SetTipText(this.Text);
            setToolMenuItems();

            _monitorTimer.Change(0, 1000);

            if (_caseInfo.checkItem == 2) //血压
            {
                SwitchTabPage(btnBlood.Name, false);
            }
            else
            {
                //显示重分析界面
                SwitchTabPage(btnResetana.Name, false);
            }
        }

        private void setToolMenuItems()
        {
            if (_caseInfo.checkItem == 1) //心电
            {
                foreach (ToolStripItem item in this.toolStrip1.Items)
                {
                    item.Visible = !item.Name.Equals("btnBlood");
                }
            }
            else if (_caseInfo.checkItem == 2) //血压
            {
                foreach (ToolStripItem item in this.toolStrip1.Items)
                {
                    if ((new string[] { "btnInfo", "btnBlood", "btnHourlist", "btnSummary", "btnReport" }).Contains(item.Name))
                        item.Visible = true;
                    else
                        item.Visible = false;
                }
            }
            else //二合一
            {
                foreach (ToolStripItem item in this.toolStrip1.Items)
                {
                    item.Visible = true;
                }
            }
        }

        private void setParent()
        {
            if (_ownerHandle > 0)
            {
                IntPtr hWndOwner = new IntPtr(_ownerHandle);

                this.SuspendLayout();
                try
                {
                    if (_allowVisible && !IsIconic(hWndOwner))
                    {
                        // 设置所有者窗口（使子窗口始终位于主窗口上方）
                        SetWindowLongPtrSafe(this.Handle, GWL_HWNDPARENT, hWndOwner);
                        // 确保窗口显示（跟随主窗口）
                        ShowWindow(this.Handle, SW_SHOW);
                    }
                    else
                    {
                        // 调整所有者关系
                        SetWindowLongPtrSafe(this.Handle, GWL_HWNDPARENT, hWndOwner);
                    }
                }
                finally
                {
                    //强制无效化区域，触发一次性的完整重绘
                    this.Invalidate(true);
                    this.Update();
                    this.ResumeLayout(true);
                    _laidOut = true;
                }
            }
        }

        //激活framework的数据库连接
        private void prepareDBContext()
        {
            string dbFileName = PubFuns.GetAbsolutePath("ecg.db");

            try
            {
                ZoncareEntities.Init(dbFileName);

                ZoncareEntities zqEntities = ZoncareEntities.Instance;
                AnaValues.CurrentUser = zqEntities.User.AsNoTracking().FirstOrDefault(u => u.id == _userId);

                ZoncareEntities.IsReadyDbContext = true;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "分析模块");
            }
        }

        //检查数据库连接
        private void checkDbConnction()
        {
            Stopwatch watcher = Stopwatch.StartNew();
            while (!ZoncareEntities.IsReadyDbContext)
            {
                Thread.Sleep(1);
                if (watcher.ElapsedMilliseconds > 5000)
                {
                    CommonFuns.PromptError("数据库连接失败");
                    break;
                }
            }
        }

        //检查是否支持 Skia GPU渲染
        private void checkSkiaGpuAcceleration()
        {
            if (_gpuProbeResult.HasValue)
            {
                CommonValues.IsGpuAvailable = _gpuProbeResult.Value;
                return;
            }

            this.InvokeIfRequired(() =>
            {
                SafeSKGLControl skglControl = null;
                try
                {
                    // 创建 SKGLControl 并确保它已加载
                    skglControl = new SafeSKGLControl();
                    this.Controls.Add(skglControl);

                    // 确保窗体显示后触发渲染
                    skglControl.Refresh();
                    CommonValues.IsGpuAvailable = skglControl.IsGpuAccelerated;
                }
                catch (Exception ex)
                {
                    //探测失败(如 wglCreateContext 失败)时回退软件渲染, 不向外抛出
                    Logger.CreateLog(ex, "分析模块");
                    CommonValues.IsGpuAvailable = false;
                }
                finally
                {
                    _gpuProbeResult = CommonValues.IsGpuAvailable;

                    //探测控件用完即销毁, 释放其占用的 GL 上下文与句柄, 避免资源累积
                    if (skglControl != null)
                    {
                        try
                        {
                            this.Controls.Remove(skglControl);
                            skglControl.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex, "分析模块");
                        }
                    }
                }

                Console.WriteLine("gpu可用性: " + CommonValues.IsGpuAvailable.ToString());
            });
        }

        //检查数据(保存)状态 以及当前分析进程是否闲置超时
        private void checkDataAndAppState(object state)
        {
            while (AEcgHandler.IsBusy) { Thread.Sleep(1); }

            try
            {
                if (AEcgHandler.UpdatesCount > 5)
                    AEcgHandler.SaveAnalyzeData();
                else if (DateTime.Now.Subtract(AEcgHandler.LastUpdateTime).TotalSeconds > 5 && AEcgHandler.UpdatesCount > 0)
                    AEcgHandler.SaveAnalyzeData();

                TimeSpan idleTime = DateTime.Now - _lastActivityTime;
                if (idleTime.TotalSeconds >= SysConstants.TIMEOUT_SECONDS)
                {
                    _monitorTimer.Change(Timeout.Infinite, 1000);
                    _pipeServer.SendMessage($"timeout");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "分析模块");
            }
        }

        /// <summary>
        /// 重写SetVisibleCore方法，完全控制窗口可见性
        /// 确保即使窗口不可见，句柄也能正常创建
        /// </summary>
        /// <param name="value">是否可见</param>
        protected override void SetVisibleCore(bool value)
        {
            // 关键调整：
            // 1. 如果句柄尚未创建，允许基础实现创建句柄
            // 2. 但只在_allowVisible为true时才实际显示窗口
            if (!IsHandleCreated)
            {
                // 强制创建句柄
                CreateHandle();
                // 创建句柄后立即隐藏
                base.SetVisibleCore(false);
            }
            else
            {
                // 句柄已存在，根据_allowVisible控制可见性
                base.SetVisibleCore(_allowVisible && value);
            }
        }

    }
}
