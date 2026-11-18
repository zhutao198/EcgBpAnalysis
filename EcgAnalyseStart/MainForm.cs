using AnalysisSystem.Base;
using EcgBpAnalysis.FuncComponent;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemSettings;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.Component.Widgets;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    public partial class MainForm : BaseForm
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;
        private const uint WS_VISIBLE = 0x10000000;

        private PipeClient _pipeClient;
        private SubProgramState _subAppSate; //分析模块子进程相关信息及状态
        private Timer _timeoutTimer;
        private DateTime _lastActivityTime = DateTime.Now;

        private Timer _refreshTimer;
        private static object _lockObject = new object();

        public MainForm()
        {
            InitializeComponent();

            this.ControlBox = false;
            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            this.tabControl1.ApplyHeaderlessStyle = true;
            this.tabControl1.Visible = false;
            this.tabPage1.SuspendLayout();

            if (!DesignMode)
            {
                this.tlpFilter.SuspendLayout();
                this.tlpFilter.Visible = false;
            }
        }

        #region 事件
        private void MainForm_Load(object sender, EventArgs e)
        {
            init();

            CommonValues.SystemSettings = ZqQuery.loadSettings(user.Loginer.id);
            this.ddlUser.Text = user.Loginer.userName;
            this.ddlUser.AutoSize = true;

            loadDriveInfo();

            int hWnd = this.Handle.ToInt32();
            PubFuns.AsyncDoWork(() => prepareEnvironment(hWnd));

            this.picLogo.Image = CommonFuns.GetImageFromFile(PubFuns.GetAbsolutePath("images\\logo.png"), true);
            this.picMin.Click += (o, args) => { this.WindowState = FormWindowState.Minimized; };
            this.picClose.Click += (o, args) => { this.Close(); };
            this.tabControl1.Visible = true;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            //挂起窗体级布局：把容器布局恢复、筛选区显示、下拉框初始化引发的多次布局计算合并为一次
            this.SuspendLayout();

            this.tabPage1.ResumeLayout();
            this.tlpFilter.Visible = true;
            this.tlpFilter.ResumeLayout();

            initDataControls();

            this.ResumeLayout(true);
            this.PerformLayout();

            this.caseList1.OnControlShown();

            if (!string.IsNullOrEmpty(CommonValues.WebCaseSource)) { _refreshTimer.Start(); }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (_subAppSate != null && _subAppSate.Handle != IntPtr.Zero)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    //ShowWindow(_subAppSate.Handle, 0x06);
                }
                else
                {
                    if (WindowState == FormWindowState.Normal)
                    {
                        WindowState = FormWindowState.Maximized;
                    }
                    else
                    {
                        //if (_subApp.Visible)
                        //    ShowWindow(_subApp.Handle, 0x03);
                        //else
                        //    ShowWindow(_subApp.Handle, 0x06);
                    }

                    //int style = GetWindowLong(_subAppSate.Handle, GWL_STYLE);
                    //if ((style & WS_VISIBLE) != 0)
                    if (_subAppSate.ShouldDisplay)
                    {
                        ShowWindow(_subAppSate.Handle, 0x03);
                    }
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!IsLoggingOut)
            {
                if (CommonFuns.PromptMessage("确认退出？", 1, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _timeoutTimer?.Stop();
            _pipeClient?.Disconnect();
            backupDatabase();

            this.Dispose();
            if (!IsLoggingOut) Application.Exit();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedIndex == 0) //病例管理
            {
                this.caseList1.RefreshData?.Invoke(sender, -1, true);
            }
            else if (this.tabControl1.SelectedIndex == 1) //学术检索
            {

            }
            else if (this.tabControl1.SelectedIndex == 2) //回收站
            {
                this.caseList2.RefreshData?.Invoke(sender, -1, true);
            }
            else if (this.tabControl1.SelectedIndex == 3) //收藏夹
            {
                this.caseList3.RefreshData?.Invoke(sender, -1, true);
            }
        }

        //登记
        private void btnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm frmRegister = new RegisterForm();
            frmRegister.OnDataSaved += (patInfo, deviceType, isReadCard, startAnalysis) =>
            {
                this.caseList1.ResetGridSelection();
                this.caseList1.RefreshData?.Invoke(sender, -1, true);
            };

            Rectangle displayRegion = new Rectangle(toolStrip1.Left, toolStrip1.Top, this.ClientSize.Width, this.ClientSize.Height - toolStrip1.Top);
            frmRegister.ShowDialogEx(this, showLayer: true, displayRegion: displayRegion);
        }

        //读卡
        private void btnReadCard_Click(object sender, EventArgs e)
        {
            bool result = false;
            int checkItem = 1;
            int zqDeviceType = 4;
            string errMsg = string.Empty;
            examCase patientInfo = null;

            AppCode.ShowProcessing("正在查找设备...",
                this,
                async (obj) =>
                {
                    (result, zqDeviceType, checkItem, errMsg, patientInfo) = await AppCode.checkDeviceForRead();
                    await Task.Delay(200);
                },
                null,
                new Rectangle(PointToScreen(this.panel1.Location), this.panel1.Size));

            if (result == false)
            {
                //CommonFuns.PromptError(this, errMsg);
                ScreenCenterTip.Show(errMsg);
                return;
            }

            RegisterForm frmRegister = new RegisterForm(patientInfo, zqDeviceType, "");
            frmRegister.OnDataSaved += (caseInfo, deviceInfo, isReadCard, autoEnterAnalysis) =>
            {
                uploads(caseInfo, deviceInfo, isReadCard, autoEnterAnalysis, null, "");
            };
            frmRegister.ShowDialogEx(this, showLayer: true);
        }

        //初始化设备
        private void btnInit_Click(object sender, EventArgs e)
        {
            if (CommonFuns.PromptMessage("记录卡初始化后将不可恢复，确认要初始化记录卡吗？", 1) != DialogResult.OK) { return; }

            var driveInfo = CommonFuns.CheckDrive("ZQ_HOLTER");
            string drive = driveInfo.drive, driveErr = driveInfo.driveErr;
            if (string.IsNullOrEmpty(drive))
            {
                CommonFuns.PromptError(string.IsNullOrEmpty(driveErr) ? "未检测到设备或设备通讯失败" : driveErr);
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            bool result = AppCode.InitDevice(drive, "1");
            if (result == true)
            {
                ScreenCenterTip.Show("记录卡初始化成功");
            }
            else
            {
                CommonFuns.PromptError("记录卡初始化失败");
            }
            OperateLog.logParam("初始化记录卡", $"devicePath:{drive}", sw.ElapsedMilliseconds, user.Loginer.userName);
        }

        //设置
        private void btnSetting_Click(object sender, EventArgs e)
        {
            var originalSettings = PubFuns.DeepClone(CommonValues.SystemSettings);

            SystemSetting sytemSettingForm = new SystemSetting();
            sytemSettingForm.OnSettingChanged += (changeType) =>
            {
                if (changeType == SettingChangeType.BaseInfoChanged)
                {
                    if (CommonValues.SystemSettings.baseSets.holterDataPath != originalSettings.baseSets.holterDataPath)
                    {
                        this.caseList1.RetrieveData();
                    }
                }
            };
            sytemSettingForm.ShowDialogEx(this, showLayer: true);
            if (sytemSettingForm.DialogResult == DialogResult.OK)
            {
                tabControl1_SelectedIndexChanged(sender, e);
            }
        }

        //用户指南
        private void btnHelp_Click(object sender, EventArgs e)
        {
            UserGuideForm frmUserGuige = new UserGuideForm();
            frmUserGuige.ShowDialogEx(this, showLayer: true);
        }

        //注销
        private void btnLogout_Click(object sender, EventArgs e)
        {
            IsLoggingOut = true;
            OnLogout?.Invoke();
            Close();
        }

        private void tbxAge_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btnSearch1_Click(sender, e);
                return;
            }
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }
        }

        #region 病例列表
        private void btnSearch1_Click(object sender, EventArgs e)
        {
            this.caseList1.RefreshList(getCaseListQuery(), this.tbxName.Text.Trim(), 1);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            retrieveWebCases();
            this.btnRefresh.Tag = 15;
            this.btnRefresh.Text = "15";
            this.btnRefresh.Invalidate();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            //List<int> lstSelected = getSelectedCaseId();
            List<examCase> selectedCases = this.caseList1.GetSelectedCases();
            if (selectedCases.Count <= 0)
            {
                ScreenCenterTip.Show("请至少选择一条记录。");
                return;
            }
            if (selectedCases.Count > 1)
            {
                ScreenCenterTip.Show("只能选中一条记录。");
                return;
            }

            examCase caseInfo = selectedCases[0];
            RegisterForm frmPatient = new RegisterForm(caseInfo.id, caseInfo.checkItem);
            frmPatient.OnDataSaved += (patInfo, deviceType, isReadCard, startAnalysis) => { this.caseList1.RefreshData?.Invoke(sender, -1, false); };
            frmPatient.ShowDialogEx(this.FindForm(), showLayer: true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //List<int> lstSelected = getSelectedCaseId();
            List<int> lstSelected = this.caseList1.GetSelectedCases().Select(o => o.id).ToList();
            if (lstSelected.Count <= 0)
            {
                ScreenCenterTip.Show("请至少选择一条记录。");
                return;
            }

            CancelForm frmCancel = new CancelForm(lstSelected);
            frmCancel.OnCaseCancelled += () => { this.caseList1.RefreshData?.Invoke(sender, -1, true); };
            frmCancel.ShowDialog(this.FindForm());
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            int countdown = Convert.ToInt32(this.btnRefresh.Tag);
            if (countdown == 0) { retrieveWebCases(); }

            int nextCount = (countdown - 1 >= 0 ? countdown - 1 : 15);
            this.btnRefresh.Tag = nextCount;
            this.btnRefresh.Text = nextCount.ToString();
            this.btnRefresh.Invalidate();
        }
        #endregion

        #region 回收站
        //还原
        private void btnRestore_Click(object sender, EventArgs e)
        {
            List<examCase> selectedCases = this.caseList2.GetSelectedCases();
            if (selectedCases.Count <= 0)
            {
                ScreenCenterTip.Show("请至少选择一条记录。");
                return;
            }

            var selectedIds = selectedCases.Select(o => o.id);
            List<examCase> cases = _MyEntities.ExamCases.Where(o => selectedIds.Contains(o.id)).ToList();
            foreach (examCase caseInfo in cases)
            {
                caseInfo.delFlag = 0;
                caseInfo.cancelReason = null;
            }
            _MyEntities.SaveChanges();

            this.BeginInvoke(new MethodInvoker(() =>
            {
                this.caseList2.RefreshData?.Invoke(sender, -1, true);
                this.caseList1.RefreshData?.Invoke(sender, -1, false);
            }));
        }

        //永久删除
        private void btnReallyDelete_Click(object sender, EventArgs e)
        {
            List<examCase> selectedCases = this.caseList2.GetSelectedCases();
            if (selectedCases.Count <= 0)
            {
                ScreenCenterTip.Show("请至少选择一条记录。");
                return;
            }

            if (CommonFuns.PromptMessage("确实要永久删除吗?", 1) != DialogResult.OK) return;

            var selectedIds = selectedCases.Select(o => o.id);
            List<examCase> cases = _MyEntities.ExamCases.Where(o => selectedIds.Contains(o.id)).ToList();
            _MyEntities.RemoveRange(cases);
            _MyEntities.SaveChanges();

            var paraList = selectedCases.Select(o => new { o.id, o.caseNo });
            OperateLog.logParam("病例永久删除", paraList, 0, user.Loginer.userName);

            //删除病例数据文件
            Task.Run(() =>
            {
                foreach (examCase caseInfo in selectedCases)
                {
                    string rootPath = caseInfo.filePath;
                    Directory.Delete(rootPath, recursive: true);
                }
            });

            this.caseList2.RefreshData?.Invoke(sender, -1, true);
        }
        #endregion

        #region 收藏夹
        private void btnSearch3_Click(object sender, EventArgs e)
        {
            IQueryable<examCase> query = getCaseListQuery3();
            query = query.Where(o => o.collectCases != null && o.collectCases.Any(c => c.userId == user.Loginer.id));
            this.caseList3.RefreshList(query, this.tbxName2.Text.Trim(), 1);
        }

        private void btnCancelCollect_Click(object sender, EventArgs e)
        {
            List<examCase> selectedCases = this.caseList3.GetSelectedCases();
            if (selectedCases.Count <= 0)
            {
                ScreenCenterTip.Show("请至少选择一条记录。");
                return;
            }

            var selectedIds = selectedCases.Select(o => o.id);
            List<collectCase> collectedCases = _MyEntities.CollectCases.Where(o => o.userId == user.Loginer.id && o.ecgId != null && selectedIds.Contains(o.ecgId.Value)).ToList();
            _MyEntities.RemoveRange(collectedCases);
            _MyEntities.SaveChanges();

            this.caseList3.RefreshData?.Invoke(sender, -1, true);
        }
        #endregion
        #endregion


        private void init()
        {
            // 初始化计时器
            _timeoutTimer = new Timer();
            _timeoutTimer.Interval = 1000; // 每秒检查一次
            _timeoutTimer.Tick += checkTimeout;
            _timeoutTimer.Start();

            _refreshTimer = new Timer();
            _refreshTimer.Interval = 1000;
            _refreshTimer.Tick += refreshTimer_Tick;

            initUI();

            _pipeClient = new PipeClient("zoncare-holter-bp");
            _pipeClient.OnMessageReceived += processSubprogramMessage;

            this.caseList1.UploadDataFile += (entity) => { uploads(entity, null); };
            this.caseList1.EnterAnalysis += (entity) => { startAnalysis(entity); };

            this.caseList3.EnterAnalysis += (entity) => { startAnalysis(entity); };

            // 监听 Application.Idle 事件（用户无操作时触发）
            Application.Idle += (s, e) => { _lastActivityTime = DateTime.Now; };
        }

        private void initUI()
        {
            this.tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            List<Button> menuButtons = CommonFuns.RetriveSubControls<Button>(this.tlpMenu);
            for (int i = 0; i < this.tlpMenu.RowCount; i++)
            {
                Button cellButton = menuButtons.FirstOrDefault(o => tlpMenu.GetCellPosition(o) != null && tlpMenu.GetCellPosition(o).Row == i);
                //Control ctl = tlpMenu.GetControlFromPosition(0, i);
                //if (ctl is Button cellButton)
                {
                    if (cellButton == null) continue;
                    int idx = Convert.ToInt32(i);
                    if (!cellButton.Visible) //暂隐藏学术检索
                    {
                        this.tlpMenu.RowStyles[i].Height = 0;
                    }

                    cellButton.Click += (o, e) =>
                    {
                        this.tabControl1.SelectedIndex = idx;
                        cellButton.BackColor = SystemColors.GradientInactiveCaption;
                        cellButton.ForeColor = Color.FromArgb(74, 122, 196);
                        cellButton.ImageKey = Path.GetFileNameWithoutExtension(cellButton.ImageKey).TrimEnd('A') + "A" + Path.GetExtension(cellButton.ImageKey);
                        foreach (Button btn1 in menuButtons)
                        {
                            if (btn1 != cellButton)
                            {
                                btn1.BackColor = Color.WhiteSmoke;
                                btn1.ForeColor = Color.Black;
                                btn1.ImageKey = btn1.ImageKey.Replace("A.png", ".png");
                            }
                        }
                    };
                }
            }
            this.button1.PerformClick();

            #region 病例列表
            this.tbxFromAge.KeyPress += tbxAge_KeyPress;
            this.tbxToAge.KeyPress += tbxAge_KeyPress;
            this.btnRefresh.Click += btnRefresh_Click;
            this.btnSearch1.Click += btnSearch1_Click;
            this.btnEdit.Click += btnEdit_Click;
            this.btnDelete.Click += btnDelete_Click;
            foreach (Control ctl in this.tlpFilter.Controls)
            {
                if (ctl is TextBox tbx)
                {
                    tbx.KeyPress += (sender, e) =>
                    {
                        if (e.KeyChar == 13) btnSearch1_Click(sender, e);
                    };
                }
            }
            this.caseList1.RefreshData += (sender, pageIndex, resetCheck) =>
            {
                IQueryable<examCase> query = getCaseListQuery();
                this.caseList1.RefreshList(query, this.tbxName.Text.Trim(), pageIndex, resetCheck, sender == this.btnSetting);
            };
            #endregion

            #region 回收站
            this.btnRestore.Click += btnRestore_Click;
            this.btnReallyDelete.Click += btnReallyDelete_Click;

            this.caseList2.RefreshData += (sender, pageIndex, resetCheck) =>
            {
                string rootPath = CommonValues.SystemSettings.baseSets.holterDataPath;
                IQueryable<examCase> query = ZoncareEntities.Instance.ExamCases.AsNoTracking().Where(o => o.fileDirectory.Replace("/", "\\") == rootPath && o.delFlag == 1);
                this.caseList2.RefreshList(query, "", pageIndex, resetCheck, sender == this.btnSetting);
            };
            #endregion

            #region 收藏夹
            this.tbxFromAge2.KeyPress += tbxAge_KeyPress;
            this.tbxToAge2.KeyPress += tbxAge_KeyPress;
            this.btnSearch3.Click += btnSearch3_Click;
            this.btnCancelCollect.Click += btnCancelCollect_Click;
            foreach (Control ctl in this.tlpFilter2.Controls)
            {
                if (ctl is TextBox tbx)
                {
                    tbx.KeyPress += (sender, e) =>
                    {
                        if (e.KeyChar == 13) btnSearch3_Click(sender, e);
                    };
                }
            }
            this.caseList3.RefreshData += (sender, pageIndex, resetCheck) =>
            {
                IQueryable<examCase> query = getCaseListQuery3();
                query = query.Where(o => o.collectCases != null && o.collectCases.Any(c => c.userId == user.Loginer.id));
                this.caseList3.RefreshList(query, this.tbxName2.Text.Trim(), pageIndex, resetCheck, sender == this.btnSetting);
            };
            #endregion
        }

        private void initDataControls()
        {
            CommonFuns.InitCommboBoxItems(this.cbxSex, SysConstants.Genders, true, "", "全部");
            CommonFuns.InitCommboBoxItems(this.cbxCheckItem, SysConstants.CheckItems, true, -1, "全部");
            if (CommonValues.SystemType == "ecg")
            {
                this.cbxCheckItem.SelectedValue = 1;
                this.cbxCheckItem.Enabled = false;
            }
            else if (CommonValues.SystemType == "bp")
            {
                this.cbxCheckItem.SelectedValue = 2;
                this.cbxCheckItem.Enabled = false;
            }

            if (string.IsNullOrEmpty(CommonValues.WebCaseSource))
            {
                this.btnRefresh.Visible = false;
            }

            CommonFuns.InitCommboBoxItems(this.cbxAgeUnit, SysConstants.AgeUnits);
            CommonFuns.InitCommboBoxItems(this.cbxStatus, SysConstants.CaseStatus, true, -1, "全部");

            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat];
            this.dtpFromDate.CustomFormat = this.dtpToDate.CustomFormat = dateFormat;
            this.dtpFromDate.Value = Convert.ToDateTime(DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") + " 00:00:00");
            this.dtpFromDate.Checked = false;
            this.dtpToDate.Value = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
            this.dtpToDate.Checked = false;

            //初始化收藏夹页相关控件内容
            this.BeginInvoke(new MethodInvoker(() =>
            {
                CommonFuns.InitCommboBoxItems(this.cbxSex2, SysConstants.Genders, true, "", "全部");
                CommonFuns.InitCommboBoxItems(this.cbxCheckItem2, SysConstants.CheckItems, true, -1, "全部");
                if (CommonValues.SystemType == "ecg")
                {
                    this.cbxCheckItem2.SelectedValue = 1;
                    this.cbxCheckItem2.Enabled = false;
                }
                else if (CommonValues.SystemType == "bp")
                {
                    this.cbxCheckItem2.SelectedValue = 2;
                    this.cbxCheckItem2.Enabled = false;
                }

                CommonFuns.InitCommboBoxItems(this.cbxAgeUnit2, SysConstants.AgeUnits);
                CommonFuns.InitCommboBoxItems(this.cbxStatus2, SysConstants.CaseStatus, true, -1, "全部");

                this.dtpFromDate2.CustomFormat = this.dtpToDate2.CustomFormat = dateFormat;
                this.dtpFromDate2.Value = Convert.ToDateTime(DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") + " 00:00:00");
                this.dtpFromDate2.Checked = false;
                this.dtpToDate2.Value = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
                this.dtpToDate2.Checked = false;
            }));
        }

        private async Task prepareEnvironment(int hWnd)
        {
            //1. 获取用户相关配置
            user curUser = user.Loginer;

            //TODO: 检查当前用户的相关配置, 没有就用默认参数生成一条记录
            //holterSetting settings = null;
            //using (var db = ZoncareEntities.CreateIndependent())
            //{
            //    settings = db.HolterSettings.FirstOrDefault(o => o.userId == user.Loginer.id);
            //}
            //if (settings == null)
            //{
            //    //生成用户相关的配置
            //}

            //CommonValues.SystemSettings = ... ZqQuery.loadSettings();

            //2. 启动分析模块相关进程
            CommonFuns.ExitProcess("AnalysisApp");
            string subprogramPath = PubFuns.GetAbsolutePath("AnalysisApp.exe");
            if (File.Exists(subprogramPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(subprogramPath, $"{hWnd} {curUser.id}");
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.Verb = "runas";
                Process analysisProcess = Process.Start(startInfo);

                analysisProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) { if (!e.Data.Contains("ecgAnalysis")) Console.WriteLine($"子进程输出: {e.Data}"); }
                };
                analysisProcess.BeginOutputReadLine();
                analysisProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) { Console.WriteLine($"子进程错误: {e.Data}"); }
                };
                analysisProcess.BeginErrorReadLine();

                AppDomain.CurrentDomain.ProcessExit += (sender, e) => { if (!analysisProcess.HasExited) { analysisProcess.Kill(); } };

                //连接通讯管道
                Stopwatch sw = Stopwatch.StartNew();
                while (true)
                {
                    if (Process.GetProcessesByName("AnalysisApp").Length <= 0)
                    {
                        if (sw.ElapsedMilliseconds > 10000) { break; }
                        await Task.Delay(50);
                    }
                    else
                    {
                        await Task.Delay(100);
                        if (!_pipeClient.IsConnected) _pipeClient.Connect();
                        break;
                    }
                }
            }
        }

        //构造查询
        private IQueryable<examCase> getCaseListQuery()
        {
            string rootPath = CommonValues.SystemSettings.baseSets.holterDataPath;

            IQueryable<examCase> query = ZoncareEntities.Instance.ExamCases.AsNoTracking().Where(o => o.fileDirectory.Replace("/", "\\") == rootPath && o.delFlag == 0);

            string caseNo = this.tbxCaseNo.Text.Trim();
            if (!string.IsNullOrEmpty(caseNo))
            {
                query = query.Where(o => o.caseNo.Contains(caseNo));
            }

            //注意：query 的实际枚举在后台线程执行（见 CaseList.RefreshList），
            //表达式树中不能引用界面控件（后台求值会跨线程访问控件），
            //控件值必须先在此处（UI 线程）取出为局部变量，闭包捕获局部量即为常量
            if (this.cbxSex.SelectedIndex > 0)
            {
                object selectedSex = this.cbxSex.SelectedValue;
                query = query.Where(o => selectedSex.Equals(o.sex));
            }
            if (this.cbxCheckItem.SelectedIndex > 0)
            {
                object selectedCheckItem = this.cbxCheckItem.SelectedValue;
                query = query.Where(o => o.checkItem.Equals(selectedCheckItem));
            }

            if (this.dtpFromDate.Checked)
            {
                DateTime fromTime = this.dtpFromDate.Value.Date;
                query = query.Where(o => o.appointmentTime >= fromTime);
            }
            if (this.dtpToDate.Checked)
            {
                DateTime toTime = this.dtpToDate.Value.Date.AddDays(1);
                query = query.Where(o => o.appointmentTime < toTime);
            }

            string ageUnit = this.cbxAgeUnit.SelectedValue.ToString();
            if (!string.IsNullOrEmpty(this.tbxFromAge.Text.Trim()))
            {
                int fromAge = CommonFuns.GetAgeDays(Convert.ToInt32(this.tbxFromAge.Text), ageUnit);
                query = query.Where(o => o.ageDays >= fromAge);
            }
            if (!string.IsNullOrEmpty(this.tbxToAge.Text.Trim()))
            {
                int toAge = CommonFuns.GetAgeDays(Convert.ToInt32(this.tbxToAge.Text), ageUnit);
                query = query.Where(o => o.ageDays < toAge);
            }

            if (this.cbxStatus.SelectedIndex > 0)
            {
                object selectedStatus = this.cbxStatus.SelectedValue;
                query = query.Where(o => o.status.Equals(selectedStatus));
            }
            string conclusion = this.tbxConclusion.Text.Trim();
            if (!string.IsNullOrEmpty(conclusion))
            {
                query = query.Where(o => o.holterReportConclusion.Contains(conclusion) || o.bpReportConclusion.Contains(conclusion));
            }

            return query.Include(e => e.collectCases);
        }

        //收藏夹相关查询
        private IQueryable<examCase> getCaseListQuery3()
        {
            string rootPath = CommonValues.SystemSettings.baseSets.holterDataPath;

            IQueryable<examCase> query = ZoncareEntities.Instance.ExamCases.AsNoTracking().Where(o => o.fileDirectory.Replace("/", "\\") == rootPath && o.delFlag == 0);

            string caseNo = this.tbxCaseNo2.Text.Trim();
            if (!string.IsNullOrEmpty(caseNo))
            {
                query = query.Where(o => o.caseNo.Contains(caseNo));
            }

            if (this.cbxSex2.SelectedIndex > 0)
            {
                object selectedSex = this.cbxSex2.SelectedValue;
                query = query.Where(o => selectedSex.Equals(o.sex));
            }
            if (this.cbxCheckItem2.SelectedIndex > 0)
            {
                object selectedCheckItem = this.cbxCheckItem2.SelectedValue;
                query = query.Where(o => o.checkItem.Equals(selectedCheckItem));
            }

            if (this.dtpFromDate2.Checked)
            {
                DateTime fromTime = this.dtpFromDate2.Value.Date;
                query = query.Where(o => o.appointmentTime >= fromTime);
            }
            if (this.dtpToDate2.Checked)
            {
                DateTime toTime = this.dtpToDate2.Value.Date.AddDays(1);
                query = query.Where(o => o.appointmentTime < toTime);
            }

            string ageUnit = this.cbxAgeUnit2.SelectedValue.ToString();
            if (!string.IsNullOrEmpty(this.tbxFromAge2.Text.Trim()))
            {
                int fromAge = CommonFuns.GetAgeDays(Convert.ToInt32(this.tbxFromAge2.Text), ageUnit);
                query = query.Where(o => o.ageDays >= fromAge);
            }
            if (!string.IsNullOrEmpty(this.tbxToAge2.Text.Trim()))
            {
                int toAge = CommonFuns.GetAgeDays(Convert.ToInt32(this.tbxToAge2.Text), ageUnit);
                query = query.Where(o => o.ageDays < toAge);
            }

            if (this.cbxStatus2.SelectedIndex > 0)
            {
                object selectedStatus = this.cbxStatus2.SelectedValue;
                query = query.Where(o => o.status.Equals(selectedStatus));
            }
            string conclusion = this.tbxConclusion2.Text.Trim();
            if (!string.IsNullOrEmpty(conclusion))
            {
                query = query.Where(o => o.holterReportConclusion.Contains(conclusion) || o.bpReportConclusion.Contains(conclusion));
            }

            return query.Include(e => e.collectCases);
        }

        //从远程网络获取病例
        private void retrieveWebCases()
        {
            var reqParas = new List<KeyValuePair<string, string>>();
            reqParas.Add(new KeyValuePair<string, string>("hospitalCode", CommonValues.HospitalCode));

            Task.Run(() =>
            {
                lock (_lockObject)
                {
                    using (ZoncareEntities dbEntities = ZoncareEntities.CreateIndependent())
                    {
                        try
                        {
                            string resp = HttpService.GetWebRequest(CommonValues.WebCaseSource + "/holter/client/patient/list", "GET", reqParas, out string errMsg);
                            if (!string.IsNullOrEmpty(resp))
                            {
                                JObject jo = JObject.Parse(resp);
                                if (jo["code"].ToString() == "0")
                                {
                                    string dataString = jo["data"]["rows"].ToString();
                                    List<examCase> lstCase = JsonConvert.DeserializeObject<List<examCase>>(dataString);
                                    var sourceIds = lstCase.Select(o => o.id.ToString());
                                    var existingIds = dbEntities.ExamCases.AsNoTracking().Where(p => sourceIds.Contains(p.webSourceId) && p.delFlag == 0).Select(p => p.webSourceId).ToList();
                                    //List<int> missingIds = sourceIds.Except(existingIds).ToList();

                                    List<examCase> newCases = new List<examCase>(lstCase.Count);
                                    for (int i = 0; i < lstCase.Count; i++)
                                    {
                                        examCase caseInfo = lstCase[i];
                                        if (existingIds.Contains(caseInfo.id.ToString())) continue;

                                        caseInfo.webSourceId = caseInfo.id.ToString();
                                        caseInfo.id = 0;
                                        caseInfo.status = 1;
                                        newCases.Add(caseInfo);
                                    }

                                    if (newCases.Count > 0)
                                    {
                                        //先进行批量新增
                                        dbEntities.ChangeTracker.AutoDetectChangesEnabled = false;
                                        dbEntities.ExamCases.AddRange(newCases);
                                        dbEntities.SaveChanges();

                                        foreach (examCase caseInfo in newCases)
                                        {
                                            try
                                            {
                                                string rootPath = AppCode.CreateCaseDirectory(caseInfo);
                                                AppCode.SaveArchiveInfo(caseInfo, null);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.CreateLog(ex, "患者信息");
                                                dbEntities.ExamCases.Remove(caseInfo);
                                            }
                                        }
                                        dbEntities.ChangeTracker.DetectChanges();
                                        dbEntities.SaveChanges();

                                        this.InvokeIfRequired(() => this.caseList1.RefreshData?.Invoke(this.btnRefresh, -1, true), true);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex, "病例管理");
                        }
                        finally
                        {
                            dbEntities.ChangeTracker.AutoDetectChangesEnabled = true;
                        }
                    }
                }
            });
        }


        /// <summary>
        /// 上传数据文件
        /// </summary>
        /// <param name="caseInfo">病例信息</param>
        /// <param name="deviceType">监测设备类型</param>
        /// <param name="isReadCard">当前是否为读卡操作</param>
        /// <param name="autoEnterAnalysis">是否自动进入分析模块</param>
        /// <param name="frmProgress">进度相关界面</param>
        /// <param name="sourcePath">文件的源路径</param>
        private async Task<bool> uploads(examCase caseInfo, DeviceInfo deviceInfo, bool isReadCard = false, bool autoEnterAnalysis = false, ProgressForm frmProgress = null, string sourcePath = "")
        {
            int deviceType = 0;
            if (deviceInfo != null)
            {
                deviceType = deviceInfo.DeviceType;
            }
            if (deviceInfo == null)
            {
                if (!string.IsNullOrEmpty(caseInfo.deviceType))
                {
                    int.TryParse(caseInfo.deviceType, out deviceType);
                }
            }

            bool uploadResult = false;
            try
            {
                if (deviceType == 1) //心电设备IE95
                {
                    uploadResult = await uploadOldHolter(caseInfo, isReadCard, autoEnterAnalysis, frmProgress, sourcePath);
                }
                else if (deviceType == 2) //心电设备8333(使用8377相同的方法)
                {
                    uploadResult = await uploadDataFiles8377(caseInfo, deviceInfo, isReadCard, autoEnterAnalysis, frmProgress, sourcePath);
                }
                else if (deviceType == 3) //血压设备8337
                {
                    uploadResult = await uploadDataFiles8337(caseInfo, deviceInfo, isReadCard, autoEnterAnalysis, frmProgress, sourcePath);
                }
                else if (deviceType == 4) //二合一设备8377
                {
                    uploadResult = await uploadDataFiles8377(caseInfo, deviceInfo, isReadCard, autoEnterAnalysis, frmProgress, sourcePath);
                }
                else //数据库deviceType为空
                {
                    DeviceStatus statu = await AppCode.CheckDeviceStatus(caseInfo.checkItem);
                    if (statu.IsReady)
                    {
                        uploadResult = await uploads(caseInfo, statu.DeviceInfo, isReadCard, autoEnterAnalysis, frmProgress, sourcePath);
                    }
                    else
                    {
                        CommonFuns.PromptError("未检测到设备或设备通讯失败");
                    }
                }
            }
            catch (Exception ex)
            {
                CommonFuns.PromptError("操作失败");
                Logger.CreateLog(ex, "文件上传");
                if (caseInfo != null && caseInfo.id > 0)
                {
                    caseInfo.ProcessState.Code = 500;
                }
            }
            finally
            {
                caseInfo.Processing = false;

                if (uploadResult && !string.IsNullOrEmpty(sourcePath))
                {
                    //删除下载生成的临时文件
                    try
                    {
                        Directory.Delete(sourcePath, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex, "文件上传");
                    }
                }
                //删除读卡过程出错产生的病例记录
                if (isReadCard)
                {
                    if (caseInfo != null && caseInfo.id > 0 &&
                        (caseInfo.ProcessState.Code == 500 || caseInfo.ProcessState.Code == 409))
                    {
                        examCase dbCase = ZoncareEntities.Instance.ExamCases.First(o => o.id == caseInfo.id);
                        ZoncareEntities.Instance.ExamCases.Remove(dbCase);
                        ZoncareEntities.Instance.SaveChanges();
                    }

                    if (caseInfo.ProcessState.Code == 0) this.caseList1.ResetGridSelection();
                }

                //var entity = ZoncareEntities.Instance.ExamCases.Local.FirstOrDefault(x => x.id == caseInfo.id);
                //if (entity != null) { ZoncareEntities.Instance.Entry(entity).Reload(); }
                this.caseList1.RefreshData?.Invoke(null, -1, true);
            }

            //上传进度框在此处才真正关闭、主窗体被激活；若本次上传自动进入了分析模块
            //(autoEnterAnalysis)，需要再次把子窗口提到最前，避免被主窗体压住
            bringSubAppToFront();
            return uploadResult;
        }

        private async Task<bool> uploadOldHolter(examCase caseInfo, bool isReadCard = false, bool autoEnterAnalysis = false, ProgressForm frmProgress = null, string sourcePath = "")
        {
            bool result = false;
            if (string.IsNullOrEmpty(sourcePath))
            {
                caseInfo.ResetProcessState();
                setProgressWeights(caseInfo, false, false, autoEnterAnalysis);
            }

            Func<object, Task> funUpload = async (args) =>
            {
                if (args != null)
                {
                    dynamic paraInfo = args as ExpandoObject;
                    frmProgress = paraInfo.Form as ProgressForm;
                }

                caseInfo.Processing = true;

                Task uploadIE95 = AppCode.UploadOldHolter(caseInfo, sourcePath).ContinueWith(t =>
                {
                    if (t.Result != RETURN_CODE.OK)
                    {
                        frmProgress.InvokeIfRequired(frmProgress.Hide);
                        if (t.Result == RETURN_CODE.VerfifyFail)
                        {
                            CommonFuns.PromptError("文件校验失败");
                            caseInfo.ProcessState.Code = 409;
                        }
                        else if (t.Result == RETURN_CODE.NotFound)
                            CommonFuns.PromptError("未检测到设备或设备通讯失败");
                        else if (t.Result == RETURN_CODE.FileNotFound)
                            CommonFuns.PromptError("文件未找到");
                        else
                            CommonFuns.PromptError("上传失败");

                        caseInfo.Processing = false;
                    }
                });

                bool analysisProgressStarted = false;
                string errorMessage = string.Empty, successMessage = string.Empty;
                CaseProcessState opState = caseInfo.ProcessState;
                WorkProgress uploadProgress = opState.SubProgresses["upload"];

                Task<bool> enterAnalysisTask = null;
                while (caseInfo.Processing)
                {
                    await Task.Delay(200);

                    frmProgress.InvokeIfRequired(async () =>
                    {
                        if (opState.Code == -1)
                        {
                            //frm.Message = _langs["正在读取血压数据，请稍后..."];
                        }
                        else if (opState.Code == -2)
                        {
                            frmProgress.Message = _langs["正在校验数据文件的CRC..."];
                        }
                        else if (opState.Code >= 400 && opState.Code != 409)
                        {
                            errorMessage = "操作失败";
                            caseInfo.Processing = false;
                        }
                        else if (opState.Code == 0)
                        {
                            if (uploadProgress.percent == 100)
                            {
                                if (autoEnterAnalysis) //自动进入分析模块
                                {
                                    if (!analysisProgressStarted)
                                    {
                                        analysisProgressStarted = true;
                                        enterAnalysisTask = enterAnalysisModule(caseInfo, isReadCard, frmProgress);
                                        result = await enterAnalysisTask;
                                        if (!result) caseInfo.Processing = false;
                                    }
                                }
                                else
                                {
                                    result = true;
                                    successMessage = "上传成功";
                                    caseInfo.Processing = false;
                                }
                            }
                            else
                            {
                                frmProgress.Message = _langs["上传中，请稍后..."];
                                frmProgress.Progress = opState.ProgressValue;
                            }
                        }
                        frmProgress.Refresh();
                    });
                }

                //轮询可能因 enterAnalysisModule 内部置 caseInfo.Processing=false 而提前退出，
                //此处必须等它真正返回，否则 result 仍是旧值 false（上传成功的临时目录不会被清理）
                if (enterAnalysisTask != null)
                {
                    result = await enterAnalysisTask;
                }

                frmProgress.InvokeIfRequired(() => frmProgress.Hide());
                if (!string.IsNullOrEmpty(errorMessage)) CommonFuns.PromptError(errorMessage);
                if (!string.IsNullOrEmpty(successMessage)) ScreenCenterTip.Show(successMessage);
            };

            Stopwatch sw = Stopwatch.StartNew();
            if (frmProgress != null)
                await funUpload.Invoke(null);
            else
                AppCode.ShowProgress(_langs["上传中，请稍后..."], this, funUpload, null,
                    new Rectangle(PointToScreen(toolStrip1.Location), new Size(this.ClientSize.Width, this.ClientSize.Height - toolStrip1.Top)));

            object para = new { CaseNo = caseInfo.caseNo, PatientName = caseInfo.name };
            OperateLog.logParam("病例数据上传", para, sw.ElapsedMilliseconds, user.Loginer.userName);
            return result;
        }

        private async Task<bool> uploadDataFiles8377(examCase caseInfo, DeviceInfo deviceInfo, bool isReadCard = false, bool autoEnterAnalysis = false, ProgressForm frmProgress = null, string sourcePath = "")
        {
            bool result = false;

            if (!isReadCard && string.IsNullOrEmpty(sourcePath))
            {
                RETURN_CODE checkResult = AppCode.UploadCheck(caseInfo);
                if (checkResult != RETURN_CODE.OK)
                {
                    if (checkResult == RETURN_CODE.NotFound)
                    {
                        CommonFuns.PromptError("未检测到设备或设备通讯失败");
                        return false;
                    }
                    else if (checkResult == RETURN_CODE.FileNotFound)
                    {
                        CommonFuns.PromptError("文件未找到");
                        return false;
                    }
                    else if (checkResult == RETURN_CODE.MisMatching)
                    {
                        if (CommonFuns.PromptMessage("上传数据与当前病例不符，是否继续上传？", 1, MessageBoxButtons.YesNo) != DialogResult.Yes)
                        {
                            return false;
                        }
                    }
                    else if (checkResult == RETURN_CODE.Err)
                    {
                        CommonFuns.PromptError("上传失败");
                        return false;
                    }
                }
            }

            if (string.IsNullOrEmpty(sourcePath))
            {
                caseInfo.ResetProcessState();
                setProgressWeights(caseInfo, false, false, autoEnterAnalysis);
            }

            Func<object, Task> funUpload = async (args) =>
            {
                if (args != null)
                {
                    dynamic state = args as ExpandoObject;
                    frmProgress = state.Form as ProgressForm;
                }

                performUpload8377(frmProgress, caseInfo, deviceInfo, false, sourcePath);

                bool analysisProgressStarted = false;
                string errorMessage = string.Empty, successMessage = string.Empty;
                CaseProcessState opState = caseInfo.ProcessState;
                WorkProgress uploadProgress = opState.SubProgresses["upload"];

                Task<bool> enterAnalysisTask = null;
                while (caseInfo.Processing)
                {
                    await Task.Delay(200);

                    frmProgress.InvokeIfRequired(async () =>
                    {
                        if (opState.Code == -1)
                        {
                            //frm.Message = _langs["正在读取血压数据，请稍后..."];
                        }
                        else if (opState.Code == -2)
                        {
                            frmProgress.Message = _langs["正在校验数据文件的CRC..."];
                        }
                        else if (opState.Code >= 400 && opState.Code != 409)
                        {
                            errorMessage = "操作失败";
                            caseInfo.Processing = false;
                        }
                        else if (opState.Code == 0)
                        {
                            if (uploadProgress.percent == 100)
                            {
                                if (autoEnterAnalysis) //自动进入分析模块
                                {
                                    if (!analysisProgressStarted)
                                    {
                                        analysisProgressStarted = true;
                                        enterAnalysisTask = enterAnalysisModule(caseInfo, isReadCard, frmProgress);
                                        result = await enterAnalysisTask;
                                        if (!result) caseInfo.Processing = false;
                                    }
                                }
                                else
                                {
                                    result = true;
                                    successMessage = "上传成功";
                                    caseInfo.Processing = false;
                                }
                            }
                            else
                            {
                                frmProgress.Message = _langs["上传中，请稍后..."];
                                frmProgress.Progress = opState.ProgressValue;
                            }
                        }
                        frmProgress.Refresh();
                    });
                }

                //轮询可能因 enterAnalysisModule 内部置 caseInfo.Processing=false 而提前退出，
                //此处必须等它真正返回，否则 result 仍是旧值 false（上传成功的临时目录不会被清理）
                if (enterAnalysisTask != null)
                {
                    result = await enterAnalysisTask;
                }

                frmProgress.InvokeIfRequired(() => frmProgress.Hide());
                if (!string.IsNullOrEmpty(errorMessage)) CommonFuns.PromptError(errorMessage);
                if (!string.IsNullOrEmpty(successMessage)) ScreenCenterTip.Show(successMessage);
            };

            Stopwatch sw = Stopwatch.StartNew();
            if (frmProgress != null)
                await funUpload.Invoke(null);
            else
                AppCode.ShowProgress(_langs["上传中，请稍后..."], this, funUpload, null,
                new Rectangle(PointToScreen(toolStrip1.Location), new Size(this.ClientSize.Width, this.ClientSize.Height - toolStrip1.Top)));

            object paraInfo = new { CaseNo = caseInfo.caseNo, PatientName = caseInfo.name };
            OperateLog.logParam("病例数据上传", paraInfo, sw.ElapsedMilliseconds, user.Loginer.userName);
            return result;
        }

        private void performUpload8377(ProgressForm frmProgress, examCase caseInfo, DeviceInfo deviceInfo, bool ignoreCRC = false, string sourcePath = "")
        {
            caseInfo.Processing = true;

            Stopwatch sw = Stopwatch.StartNew();
            AppCode.UploadHolterBp(caseInfo, deviceInfo, ignoreCRC, sourcePath).ContinueWith(t =>
            {
                if (t.Result == RETURN_CODE.OK)
                {
                }
                else
                {
                    frmProgress.InvokeIfRequired(frmProgress.Hide);
                    if (!ignoreCRC && t.Result == RETURN_CODE.VerfifyFail)
                    {
                        if (CommonFuns.PromptMessage("文件校验失败，是否继续上传？", 1, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            frmProgress.InvokeIfRequired(() => frmProgress.Visible = true);
                            performUpload8377(frmProgress, caseInfo, deviceInfo, true, sourcePath);
                            return;
                        }
                        else
                        {
                            caseInfo.ProcessState.Code = 409;
                            caseInfo.Processing = false;
                        }
                    }
                    else if (t.Result == RETURN_CODE.FileNotFound)
                    {
                        CommonFuns.PromptError("文件未找到");
                    }
                    else
                    {
                        CommonFuns.PromptError("上传失败");
                    }
                }
                Logger.LogRuntimeInfo($"病例数据上传(病历号:{caseInfo.caseNo})", sw.ElapsedMilliseconds);
            });
        }

        private async Task<bool> uploadDataFiles8337(examCase caseInfo, DeviceInfo deviceInfo, bool isReadCard = false, bool autoEnterAnalysis = false, ProgressForm frmProgress = null, string sourcePath = "")
        {
            bool result = false;

            if (string.IsNullOrEmpty(sourcePath))
            {
                RETURN_CODE checkResult = AppCode.UploadCheck(caseInfo);
                if (checkResult != RETURN_CODE.OK)
                {
                    if (checkResult == RETURN_CODE.NotFound)
                    {
                        CommonFuns.PromptError("未检测到设备或设备通讯失败");
                        return false;
                    }
                    else if (checkResult == RETURN_CODE.FileNotFound)
                    {
                        CommonFuns.PromptError("文件未找到");
                        return false;
                    }
                    else if (checkResult == RETURN_CODE.MisMatching)
                    {
                        if (!isReadCard)
                        {
                            if (CommonFuns.PromptMessage("上传数据与当前病例不符，是否继续上传？", 1, MessageBoxButtons.YesNo) != DialogResult.Yes)
                            {
                                return false;
                            }
                        }
                    }
                    else if (checkResult == RETURN_CODE.Err)
                    {
                        CommonFuns.PromptError("上传失败");
                        return false;
                    }
                }

                caseInfo.ResetProcessState();
                setProgressWeights(caseInfo, false, false, autoEnterAnalysis);
            }

            Func<object, Task> funUpload = async (args) =>
            {
                if (args != null)
                {
                    dynamic paraInfo = args as ExpandoObject;
                    frmProgress = paraInfo.Form as ProgressForm;
                }

                performUpload8337(frmProgress, deviceInfo, caseInfo, false, sourcePath);

                bool analysisProgressStarted = false;
                string errorMessage = string.Empty, successMessage = string.Empty;
                CaseProcessState opState = caseInfo.ProcessState;
                WorkProgress uploadProgress = opState.SubProgresses["upload"];

                Task<bool> enterAnalysisTask = null;
                while (caseInfo.Processing)
                {
                    await Task.Delay(200);

                    frmProgress.InvokeIfRequired(async () =>
                    {
                        if (opState.Code == -1)
                        {
                            //frm.Message = _langs["正在读取血压数据，请稍后..."];
                        }
                        else if (opState.Code == -2)
                        {
                            frmProgress.Message = _langs["正在校验数据文件的CRC..."];
                        }
                        else if (opState.Code >= 400 && opState.Code != 409)
                        {
                            errorMessage = "操作失败";
                            caseInfo.Processing = false;
                        }
                        else if (opState.Code == 0)
                        {
                            if (uploadProgress.percent == 100)
                            {
                                if (autoEnterAnalysis) //自动进入分析模块
                                {
                                    if (!analysisProgressStarted)
                                    {
                                        analysisProgressStarted = true;
                                        enterAnalysisTask = enterAnalysisModule(caseInfo, isReadCard, frmProgress);
                                        result = await enterAnalysisTask;
                                        if (!result) caseInfo.Processing = false;
                                    }
                                }
                                else
                                {
                                    result = true;
                                    successMessage = "上传成功";
                                    caseInfo.Processing = false;
                                }
                            }
                            else
                            {
                                frmProgress.Message = _langs["上传中，请稍后..."];
                                frmProgress.Progress = opState.ProgressValue;
                            }
                        }
                        frmProgress.Refresh();
                    });
                }

                //轮询可能因 enterAnalysisModule 内部置 caseInfo.Processing=false 而提前退出，
                //此处必须等它真正返回，否则 result 仍是旧值 false
                if (enterAnalysisTask != null)
                {
                    result = await enterAnalysisTask;
                }

                frmProgress.InvokeIfRequired(() => frmProgress.Hide());
                if (!string.IsNullOrEmpty(errorMessage)) CommonFuns.PromptError(errorMessage);
                if (!string.IsNullOrEmpty(successMessage)) ScreenCenterTip.Show(successMessage);
            };

            Stopwatch sw = Stopwatch.StartNew();
            if (frmProgress != null)
                await funUpload.Invoke(null);
            else
                AppCode.ShowProgress(_langs["上传中，请稍后..."], this, funUpload, null,
                    new Rectangle(PointToScreen(toolStrip1.Location), new Size(this.ClientSize.Width, this.ClientSize.Height - toolStrip1.Top)));

            object para = new { CaseNo = caseInfo.caseNo, PatientName = caseInfo.name };
            OperateLog.logParam("病例数据上传", para, sw.ElapsedMilliseconds, user.Loginer.userName);
            return result;
        }

        private void performUpload8337(ProgressForm frmProgress, DeviceInfo deviceInfo, examCase caseInfo, bool ignoreCRC = false, string sourcePath = "")
        {
            caseInfo.Processing = true;

            Stopwatch sw = Stopwatch.StartNew();
            AppCode.UploadBpFor8337(caseInfo, deviceInfo, ignoreCRC, sourcePath).ContinueWith(t =>
            {
                if (t.Result == RETURN_CODE.OK)
                {
                }
                else
                {
                    frmProgress.InvokeIfRequired(frmProgress.Hide);
                    if (!ignoreCRC && t.Result == RETURN_CODE.VerfifyFail)
                    {
                        if (CommonFuns.PromptMessage("文件校验失败，是否继续上传？", 1, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            frmProgress.InvokeIfRequired(() => frmProgress.Visible = true);
                            performUpload8337(frmProgress, deviceInfo, caseInfo, true, sourcePath);
                            return;
                        }
                        else
                        {
                            caseInfo.ProcessState.Code = 409;
                            caseInfo.Processing = false;
                        }
                    }
                    else if (t.Result == RETURN_CODE.FileNotFound)
                    {
                        CommonFuns.PromptError("文件未找到");
                    }
                    else
                    {
                        CommonFuns.PromptError("上传失败");
                    }
                }
                Logger.LogRuntimeInfo($"病例数据上传(病历号:{caseInfo.caseNo})", sw.ElapsedMilliseconds);
            });
        }

        private void startAnalysis(examCase caseInfo)
        {
            if (!string.IsNullOrEmpty(caseInfo.webSourceId))
            {
                CaseProcessState opState = caseInfo.ProcessState;
                //下载文件
                bool filesComplete = checkFileCompleteness(caseInfo);
                setProgressWeights(caseInfo, true, filesComplete, true);
                accessWebCaseAnalysis(caseInfo, !filesComplete);
            }
            else
            {
                enterAnalysisModule(caseInfo, false);
            }
        }

        private async Task<bool> enterAnalysisModule(examCase caseInfo, bool isReadCard, ProgressForm frmProgress = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (Process.GetProcessesByName("AnalysisApp").Length <= 0)
            {
                _pipeClient.Disconnect();
                while (Process.GetProcessesByName("AnalysisApp").Length <= 0)
                {
                    await Task.Delay(10);
                    if (sw.ElapsedMilliseconds >= 2000)
                    {
                        await prepareEnvironment(this.Handle.ToInt32());
                        break;
                    }
                }
            }

            sw.Restart();
            while (!_pipeClient.IsConnected)
            {
                await Task.Delay(10);
                if (sw.ElapsedMilliseconds >= 3000)
                {
                    ScreenCenterTip.Show("数据加载出错");
                    frmProgress?.Hide();
                    return false;
                }
            }

            if (caseInfo.checkItem != (int)CheckItem.BP)
            {
                HolterConfig holterConf = AppCode.GetHolterInfo(caseInfo);
                if (holterConf == null)
                {
                    ScreenCenterTip.Show("文件未找到");
                    frmProgress?.Hide();
                    return false;
                }
                if (holterConf.TotalSeconds < 300)
                {
                    ScreenCenterTip.Show("采集时间过短，请重新采集");
                    frmProgress?.Hide();
                    return false;
                }
            }

            //文件转换及加载
            if (!isReadCard && string.IsNullOrEmpty(caseInfo.webSourceId)) caseInfo.ResetProcessState();

            bool result = false;
            sw.Restart();
            CaseProcessState opState = caseInfo.ProcessState;
            bool isLoading = opState.SubProgresses.TryGetValue("loadData", out WorkProgress loadingProgress);
            if (!isLoading)
            {
                loadingProgress = opState.SubProgresses["loadData"] = new WorkProgress();
            }

            Func<object, Task> funAnalysis = async (args) =>
            {
                if (args != null)
                {
                    dynamic paraInfo = args as ExpandoObject;
                    frmProgress = paraInfo.Form as ProgressForm;
                }

                performAnalyse(caseInfo, loadingProgress);

                while (caseInfo.Processing)
                {
                    if (loadingProgress.percent < 99) loadingProgress.percent += 1;
                    frmProgress.InvokeIfRequired(() =>
                    {
                        frmProgress.Message = _langs["数据加载..."];
                        frmProgress.Progress = opState.ProgressValue;
                        frmProgress.Refresh();
                    });

                    if (loadingProgress.code != "200")
                    {
                        frmProgress.InvokeIfRequired(() => frmProgress.Hide());
                        caseInfo.Processing = false;
                    }
                    if (loadingProgress.percent == 100 || loadingProgress.completed) //增加completed判断以防percent被多个线程同时更新无法达到100
                    {
                        //frm.Hide();
                        if (caseInfo.Processing && loadingProgress.code == "200")
                        {
                            AppCode.EnterAnalysisApp(_pipeClient, caseInfo.id);
                            //等待子程序界面控件布局与加载；若整个流程已被外部终止则不再空等，避免调用方一直等下去
                            while (!_subAppSate.ShouldDisplay && caseInfo.Processing)
                            {
                                if (loadingProgress.percent < 99)
                                {
                                    loadingProgress.percent += 1;
                                    frmProgress.InvokeIfRequired(() =>
                                    {
                                        frmProgress.Progress = opState.ProgressValue;
                                        frmProgress.Refresh();
                                    });
                                }
                                await Task.Delay(200);
                            }
                            result = _subAppSate.ShouldDisplay;
                        }
                        caseInfo.Processing = false;
                    }
                    await Task.Delay(200);
                }
            };

            if (frmProgress != null)
                await funAnalysis.Invoke(null);
            else
                AppCode.ShowProgress(_langs["数据加载..."], this, funAnalysis, null,
                    new Rectangle(PointToScreen(toolStrip1.Location), new Size(this.ClientSize.Width, this.ClientSize.Height - toolStrip1.Top)));

            Logger.LogRuntimeInfo($"病例数据加载(病历号:{caseInfo.caseNo})", sw.ElapsedMilliseconds);
            if (loadingProgress.code != "200")
            {
                ScreenCenterTip.Show("数据加载出错");
            }
            else if (loadingProgress.code == "200" && loadingProgress.percent == 100)
            {
                frmProgress?.Hide();
            }

            //进度框已关闭、主窗体被激活为前台，此时把子窗口提到最前并激活，
            //否则最大化的主窗体会把子窗口压在后面（子窗口不在任务栏显示，表现为"界面出现后又不见了"）
            bringSubAppToFront();
            return result;
        }

        private void performAnalyse(examCase caseInfo, WorkProgress progress)
        {
            caseInfo.Processing = true;

            _subAppSate.DataIsReady = false;

            Task.Run(async () =>
            {
                await Task.Delay(300);
                try
                {
                    _pipeClient.SendMessage($"loadData|{caseInfo.id}");
                    while (!_subAppSate.DataIsReady)
                    {
                        if (!string.IsNullOrEmpty(_subAppSate.ErrorCode) && _subAppSate.ErrorCode != "200" && _subAppSate.ErrorCode != "202")
                        {
                            progress.code = _subAppSate.ErrorCode;
                            Logger.CreateLog($"加载状态码：{progress.code}", "ErrorLoading");
                            return;
                        }
                        await Task.Delay(100);
                    }

                    //直接等待加载完成(可新增消息标识用来反映加载进度)
                    progress.code = "200";
                    //progress.percent = 100;
                    progress.completed = true;
                }
                catch (Exception ex)
                {
                    progress.code = "400";
                    Logger.CreateLog(ex, "ErrorLoading");
                }
            });
        }

        //设置下载、上传、加载分析数据的进度权重
        private void setProgressWeights(examCase caseInfo, bool needDownload, bool isFileLocallyAvailable = false, bool autoEnterAnalysis = false)
        {
            caseInfo.ResetProcessState();
            CaseProcessState opState = caseInfo.ProcessState;
            if (needDownload)
            {
                if (!isFileLocallyAvailable)
                {
                    opState.SubProgresses["download"] = new WorkProgress(0.6F);
                    opState.SubProgresses["upload"] = new WorkProgress(0.2F);
                    opState.SubProgresses["loadData"] = new WorkProgress(0.2F);
                }
                else
                {
                    opState.SubProgresses["download"] = new WorkProgress(0);
                    opState.SubProgresses["upload"] = new WorkProgress(0.3F);
                    opState.SubProgresses["loadData"] = new WorkProgress(0.7F);
                }
            }
            else if (autoEnterAnalysis)
            {
                if (caseInfo.checkItem == 2)
                {
                    opState.SubProgresses["upload"] = new WorkProgress(0.8F);
                    opState.SubProgresses["loadData"] = new WorkProgress(0.2F);
                }
                else
                {
                    opState.SubProgresses["upload"] = new WorkProgress(0.3F);
                    opState.SubProgresses["loadData"] = new WorkProgress(0.7F);
                }
            }
            else
            {
                opState.SubProgresses["upload"] = new WorkProgress();
            }
        }

        //检查是否需要下载病例相关网络资源文件
        private bool checkFileCompleteness(examCase caseInfo)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(caseInfo.filePath);
            FileInfo[] files = dirInfo.GetFiles();
            bool fileIsMissing = false;
            if (caseInfo.checkItem == 1)
            {
                if (!files.Any(f => f.Name == "holter.ini")
                    || !files.Any(f => f.Name.StartsWith("holter") && f.Name.EndsWith("dat")))
                {
                    fileIsMissing = true;
                }
            }
            else if (caseInfo.checkItem == 2)
            {
                if (!files.Any(f => f.Name == "nibp.ini")
                    || !files.Any(f => f.Name.StartsWith("nibp-") && f.Name.EndsWith("dat")))
                {
                    fileIsMissing = true;
                }
            }
            else if (caseInfo.checkItem == 3)
            {
                if (!files.Any(f => f.Name == "holter.ini")
                    || !files.Any(f => f.Name.StartsWith("holter-") && f.Name.EndsWith("dat"))
                    || !files.Any(f => f.Name.StartsWith("nibp-") && f.Name.EndsWith("dat")))
                {
                    fileIsMissing = true;
                }
            }

            return !fileIsMissing;
        }

        //开始对网络病例进行分析
        private void accessWebCaseAnalysis(examCase caseInfo, bool fileIsMissing = false)
        {
            if (!Directory.Exists(caseInfo.filePath))
            {
                Directory.CreateDirectory(caseInfo.filePath);
            }

            if (fileIsMissing)
            {
                Func<object, Task> funAccess = async (args) =>
                {
                    dynamic para = args as ExpandoObject;
                    ProgressForm frm = para.Form as ProgressForm;

                    Stopwatch sw = Stopwatch.StartNew();
                    caseInfo.Processing = true;
                    CaseProcessState opState = caseInfo.ProcessState;
                    WorkProgress downloadProgress = opState.SubProgresses["download"];

                    string downloadUri = CommonValues.WebCaseSource + $"/holter/client/patient/download?id={caseInfo.webSourceId}";
                    string savePath = Path.Combine(CommonValues.SystemSettings.baseSets.holterDataPath, "download");
                    if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

                    string downloadGuid = Guid.NewGuid().ToString();
                    string downloadDir = Path.Combine(savePath, caseInfo.caseNo + "_" + downloadGuid);
                    DataTransferHandlerNew.DownloadFile(downloadUri, Path.Combine(downloadDir, downloadGuid + ".zip"), downloadDir, downloadProgress);

                    bool analysisProgressStarted = false;
                    while (caseInfo.Processing)
                    {
                        await Task.Delay(200);

                        frm.InvokeIfRequired(() =>
                        {
                            if (downloadProgress.code == "202")
                            {
                                frm.Message = _langs["文件解压中..."];
                            }
                            else if (downloadProgress.code == "400")
                            {
                                frm.Hide();
                                CommonFuns.PromptError("文件下载失败");
                                caseInfo.Processing = false;
                            }
                            else if (opState.Code == 0)
                            {
                                if (downloadProgress.percent == 100)
                                {
                                    object paraInfo = new { CaseNo = caseInfo.caseNo, PatientName = caseInfo.name };
                                    OperateLog.logParam("病例文件下载", paraInfo, sw.ElapsedMilliseconds, user.Loginer.userName);

                                    if (!analysisProgressStarted)
                                    {
                                        uploads(caseInfo, new DeviceInfo(Convert.ToInt32(caseInfo.deviceType), caseInfo.deviceModel), false, true, frm, downloadDir);
                                        analysisProgressStarted = true;
                                    }
                                }
                                else
                                {
                                    frm.Message = _langs["正在下载文件..."];
                                    frm.Progress = opState.ProgressValue;
                                }
                            }
                            frm.Refresh();
                        });
                    }
                };

                this.InvokeIfRequired(() =>
                {
                    AppCode.ShowProgress(_langs["正在下载文件..."], this, funAccess, null,
                        new Rectangle(PointToScreen(toolStrip1.Location), new Size(this.ClientSize.Width, this.ClientSize.Height - toolStrip1.Top)));
                });
            }
            else
            {
                enterAnalysisModule(caseInfo, false);
            }
        }

        /// <summary>
        /// 把分析子进程窗口提到最前并激活。
        /// 子进程自己无法可靠地抢占前台（Windows 前台锁限制，且其窗口不在任务栏显示），
        /// 必须由处于前台的主程序代劳；在进度框关闭、主窗体成为前台窗口之后调用。
        /// </summary>
        private void bringSubAppToFront()
        {
            if (_subAppSate == null || _subAppSate.Handle == IntPtr.Zero || !_subAppSate.ShouldDisplay) return;

            this.InvokeIfRequired(() =>
            {
                ShowWindow(_subAppSate.Handle, 0x03); //SW_SHOWMAXIMIZED，兼作"窗口被隐藏"时的兜底
                SetForegroundWindow(_subAppSate.Handle);
            });
        }

        //备份数据库
        private void backupDatabase()
        {
            try
            {
                string dbFileName = PubFuns.GetAbsolutePath("ecg.db");
                DirectoryInfo dirInfo = new DirectoryInfo(PubFuns.GetAbsolutePath("backup", true, true));
                File.Copy(dbFileName, PubFuns.GetAbsolutePath("backup\\" + DateTime.Today.ToString("yyyyMMdd") + "\\ecg.db", true), true);

                //保留近7次备份, 清除之前的备份
                var subDirs = dirInfo.GetDirectories().OrderByDescending(d => d.Name);
                for (int i = 0; i < subDirs.Count(); i++)
                {
                    if (i >= 7) { subDirs.ElementAt(i).Delete(true); }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "主程序");
            }
        }

        //加载[存储]磁盘信息
        private void loadDriveInfo()
        {
            Task.Run(() =>
            {
                //磁盘查询属于 I/O 操作，放在线程池执行；UI 线程只负责控件创建与赋值
                string rootPath = Path.GetPathRoot(CommonValues.SystemSettings.baseSets.holterDataPath);
                dynamic driveInfo = CommonFuns.GetDriveInfo(rootPath);

                this.InvokeIfRequired(() =>
                {
                    ZoncareProgressBar progressBar = new ZoncareProgressBar();
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.ShowWarningColor = true;
                    progressBar.Maximum = 100;
                    progressBar.Size = new Size(120, 18);
                    // 嵌入到 StatusStrip
                    ToolStripControlHost host = new ToolStripControlHost(progressBar);
                    host.Alignment = ToolStripItemAlignment.Right;
                    host.Size = new Size(120, progressBar.Height);
                    host.Margin = new Padding(1, 3, 1, 3);
                    this.statusStrip1.Items.Insert(2, host);

                    host.ToolTipText =
                        "磁盘盘符" + "：" + driveInfo.mounted + Environment.NewLine +
                        "磁盘总容量" + "：" + driveInfo.total.ToString("F1") + "GB" + Environment.NewLine +
                        "磁盘剩余空间" + "：" + driveInfo.available.ToString("F1") + "GB";
                    ToolTip toolTip = new ToolTip()
                    {
                        InitialDelay = 10,
                        ReshowDelay = 10
                    };
                    toolTip.SetToolTip(progressBar, host.ToolTipText);
                    progressBar.Value = (int)driveInfo.capacity;
                    progressBar.ForeColor = progressBar.Value < 90 ? Color.FromArgb(74, 122, 196) : Color.Red;

                    this.tsDriveUsed.Text = driveInfo.capacity.ToString("F1") + "%";
                });
            });
        }

        //检查是否闲置超时
        private void checkTimeout(object sender, EventArgs e)
        {
            TimeSpan idleTime = DateTime.Now - _lastActivityTime;

            if (idleTime.TotalSeconds >= SysConstants.TIMEOUT_SECONDS)
            {
                if (_subAppSate != null && _subAppSate.IsTimeout == false)
                {
                    //子进程处于活动状态
                    return;
                }
                _timeoutTimer.Stop();
                LoginForm newLoginForm = new LoginForm();
                newLoginForm.Show();
            }
        }

        //处理子程序消息
        private void processSubprogramMessage(string msg)
        {
            if (msg.StartsWith("refresh")) //刷新病例列表
            {
                this.caseList1.InvokeIfRequired(() =>
                {
                    tabControl1_SelectedIndexChanged(null, null);
                });
            }
            else if (msg.StartsWith("subprogramHwd")) //分析模块子进程主窗口句柄
            {
                string[] arr = msg.Split('|');
                int hwd = Convert.ToInt32(arr[1]);
                bool visible = bool.Parse(arr[2]);
                if (_subAppSate == null)
                {
                    _subAppSate = new SubProgramState(new IntPtr(hwd), visible);
                }
                else
                {
                    if (_subAppSate.Handle.ToInt32() != hwd) { _subAppSate.Handle = new IntPtr(hwd); }
                    _subAppSate.ShouldDisplay = visible;
                    if (!visible)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            // 推迟到消息队列下一轮：子窗口刚关闭、主窗体刚被激活，
                            // 若此时同步做重刷新（RefreshCache + 重绑列表），
                            // 主窗体会来不及绘制而先出白底/闪一下，再用 BeginInvoke 让首帧先画出来
                            this.BeginInvoke(new MethodInvoker(() =>
                            {
                                ZoncareEntities.Instance.RefreshCache();
                                tabControl1_SelectedIndexChanged(null, null);
                            }));
                        });
                    }
                }
            }
            else if (msg.StartsWith("dataloaded")) //数据是否已加载
            {
                string[] arr = msg.Split('|');
                string code = arr[1];
                int hwd = Convert.ToInt32(arr[2]);

                //_subAppSate = new SubProgramState(new IntPtr(hwd), isReady);
                if (code == "200")
                    _subAppSate.DataIsReady = true;
                else
                    _subAppSate.ErrorCode = code;
                _subAppSate.IsTimeout = false;
            }
            else if (msg == "timeout")
            {
                _subAppSate.IsTimeout = true;
            }
        }

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xF020;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                // 拦截最小化命令
                if ((int)m.WParam == SC_MINIMIZE)
                {
                    if (_subAppSate != null && _subAppSate.Handle != IntPtr.Zero)
                    {
                        if (_subAppSate.ShouldDisplay) return;
                    }
                }
            }
            base.WndProc(ref m);
        }

        private class SubProgramState
        {
            internal IntPtr Handle { get; set; }
            internal bool ShouldDisplay { get; set; }
            internal bool DataIsReady { get; set; }
            internal string ErrorCode { get; set; }
            internal bool IsTimeout { get; set; }
            public SubProgramState(IntPtr hWnd, bool visible)
            {
                Handle = hWnd;
                ShouldDisplay = visible;
            }
        }
    }
}
