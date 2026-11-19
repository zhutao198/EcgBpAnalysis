using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EcgBpAnalysis.SystemTests
{
    /// <summary>
    /// 系统级 UI 自动化冒烟测试（P3-1）。
    ///
    /// 基于 .NET Framework 自带的 UIAutomation（无需 WinAppDriver / Appium）。
    /// 用例分两类：
    ///  - 冒烟类（本文件实际实现）：启动程序、验证窗口与关键控件存在——可在有桌面的会话运行。
    ///  - 真机类（标记 [Ignore]）：需要加密狗、SD 卡、蓝牙 ABPM、8337 设备等，提供脚本骨架与手工步骤。
    ///
    /// 运行前提：
    ///  - Windows 交互式桌面会话（CI 无头 runner 不支持，故 CI 不执行本项目）。
    ///  - 设置环境变量 ECG_EXE 指向 EcgBpAnalysis.exe（默认仓库 bin\Debug）。
    /// </summary>
    [TestClass]
    public class AppLaunchSmokeTests
    {
        private static string RepoRoot
        {
            get
            {
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (dir != null)
                {
                    if (File.Exists(Path.Combine(dir.FullName, "EcgBpAnalysis.sln"))) return dir.FullName;
                    dir = dir.Parent;
                }
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        private static string ExePath
        {
            get
            {
                var env = Environment.GetEnvironmentVariable("ECG_EXE");
                if (!string.IsNullOrEmpty(env) && File.Exists(env)) return env;
                return Path.Combine(RepoRoot, @"EcgAnalyseStart\bin\Debug\EcgBpAnalysis.exe");
            }
        }

        /// <summary>
        /// 在 STA 线程执行 Action（UIAutomation 要求 STA）。
        /// </summary>
        private static void RunOnSta(Action action)
        {
            Exception captured = null;
            var t = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { captured = ex; }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join(TimeSpan.FromSeconds(30));
            if (captured != null) throw captured;
        }

        /// <summary>
        /// 启动主程序并等待任一顶层窗口出现（登录框或主窗体）。
        /// 返回进程对象，测试结束负责清理。
        /// </summary>
        private static Process LaunchAndWaitForWindow(out IntPtr mainWindowHandle)
        {
            Assert.IsTrue(File.Exists(ExePath), "未找到主程序: " + ExePath);
            var psi = new ProcessStartInfo(ExePath) { UseShellExecute = false };
            var proc = Process.Start(psi);
            Assert.IsNotNull(proc, "Process.Start 返回 null");

            bool appeared = proc.WaitForInputIdle(10000);
            // 登录/主窗体可能晚于 InputIdle，轮询等待至少一个可见窗口
            var deadline = DateTime.Now.AddSeconds(15);
            while (DateTime.Now < deadline)
            {
                proc.Refresh();
                if (proc.HasExited)
                {
                    int code = proc.ExitCode;
                    mainWindowHandle = IntPtr.Zero;
                    Assert.Fail($"主程序启动后立即退出（退出码 {code}）。" +
                                "常见原因：缺加密狗、缺依赖 DLL、无交互式桌面会话或初始化失败。" +
                                "本用例需在具备完整运行环境的有桌面会话中执行。");
                }
                if (proc.MainWindowHandle != IntPtr.Zero) break;
                Thread.Sleep(200);
            }
            mainWindowHandle = proc.HasExited ? IntPtr.Zero : proc.MainWindowHandle;
            return proc;
        }

        /// <summary>
        /// 冒烟：主程序可启动，出现顶层窗口。
        /// 对应 TC-SYS-01 的前置步骤（不依赖患者数据/加密狗）。
        /// </summary>
        [TestMethod]
        [TestCategory("UI")]
        public void App_Launches_AndShowsTopLevelWindow()
        {
            RunOnSta(() =>
            {
                Process proc = null;
                try
                {
                    IntPtr hwnd;
                    proc = LaunchAndWaitForWindow(out hwnd);
                    Assert.IsTrue(hwnd != IntPtr.Zero,
                        "启动后 15s 内未出现顶层窗口（可能缺加密狗/依赖 DLL，或初始化失败）");

                    var root = AutomationElement.FromHandle(hwnd);
                    Assert.IsNotNull(root, "无法获取窗口的 AutomationElement");
                    string name = root.Current.Name;
                    Assert.IsFalse(string.IsNullOrWhiteSpace(name), "顶层窗口无标题");
                }
                finally
                {
                    if (proc != null && !proc.HasExited)
                    {
                        proc.Kill();
                        proc.WaitForExit(5000);
                    }
                }
            });
        }

        /// <summary>
        /// 冒烟：主窗体关键工具栏按钮存在（登记/读卡/设置/用户指南）。
        /// 需先登录进入主窗体；若停在登录框则跳过（Inconclusive）。
        /// 对应 MainForm.Designer.cs:188-270 的控件定义。
        /// </summary>
        [TestMethod]
        [TestCategory("UI")]
        public void MainForm_ContainsKeyToolbarButtons()
        {
            RunOnSta(() =>
            {
                Process proc = null;
                try
                {
                    IntPtr hwnd;
                    proc = LaunchAndWaitForWindow(out hwnd);
                    if (hwnd == IntPtr.Zero) Assert.Inconclusive("未出现窗口");

                    // 登录窗体：若存在"用户名/密码"输入框，说明未登录，则本用例不适用
                    var root = AutomationElement.FromHandle(hwnd);
                    var condUser = new PropertyCondition(AutomationElement.NameProperty, "用户名");
                    if (root.FindFirst(TreeScope.Descendants, condUser) != null)
                        Assert.Inconclusive("当前停在登录界面，需人工登录后才能验证主窗体工具栏");

                    string[] expected = { "登记", "读卡", "设置", "用户指南" };
                    foreach (var label in expected)
                    {
                        var cond = new PropertyCondition(AutomationElement.NameProperty, label);
                        var el = root.FindFirst(TreeScope.Descendants, cond);
                        Assert.IsNotNull(el, $"主窗体缺少关键按钮: {label}");
                    }
                }
                finally
                {
                    if (proc != null && !proc.HasExited) { proc.Kill(); proc.WaitForExit(5000); }
                }
            });
        }

        // ===== 以下为真机场景骨架：需加密狗 + SD 卡/蓝牙/8337 设备，默认 Ignore =====
        // 有真机环境时移除 [Ignore] 并按步骤补充断言即可执行。

        /// <summary>
        /// TC-SYS-01：SD 卡数据导入 → 分析 → 报告全链路。
        /// 手工步骤：
        ///  1. 插入含 holter-*.dat 的 SD 卡并登记患者
        ///  2. 点"读卡"导入数据，等待入库
        ///  3. 启动分析，等待完成
        ///  4. 打开报告预览，核对分页与数据
        /// 预期：数据入库、分析完成、报告正确（对应审查项 R001/R003）。
        /// </summary>
        [TestMethod]
        [Ignore("需真机：加密狗 + SD 卡 + 脱敏病例数据。手工步骤见方法注释。")]
        [TestCategory("UI")]
        public void TC_SYS_01_SdImportAnalyzeReport()
        {
            // TODO(真机): 用 UIAutomation 定位"登记""读卡"按钮，注入脱敏病例目录，
            // 等待分析完成事件，比对报告输出文件 hash。
            Assert.Inconclusive("需真机环境");
        }

        /// <summary>
        /// TC-SYS-02：双进程握手失败不应永久挂起（对应 R026/R028）。
        /// 手工：启动主程序后阻止 AnalysisApp.exe 启动，观察是否在合理时间内给出超时提示。
        /// </summary>
        [TestMethod]
        [Ignore("需真机/受控环境：需阻止子进程启动。手工步骤见方法注释。")]
        [TestCategory("UI")]
        public void TC_SYS_02_HandshakeFailureHasTimeout()
        {
            Assert.Inconclusive("需真机环境");
        }

        /// <summary>
        /// TC-SYS-07：kPa 单位模式下血压报告高低标记应与阈值单位一致（对应 R031）。
        /// 手工：切换 kPa，打开含已知高/低值的血压报告，核对标记。
        /// </summary>
        [TestMethod]
        [Ignore("需真机 + 构造血压数据。手工步骤见方法注释。")]
        [TestCategory("UI")]
        public void TC_SYS_07_KpaUnitReportMarkers()
        {
            Assert.Inconclusive("需真机环境");
        }
    }
}
