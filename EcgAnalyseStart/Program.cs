using AnalysisSystem.Base;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    static class Program
    {
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        //private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int WS_SHOW = 5;//在窗口原来的位置以原来的尺寸激活和显示窗口
        private const int WS_RESTORE = 9;
        private const int SW_MAXIMIZE = 3;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            launchApplication(null);
        }

        private static void launchApplication(string[] Args)
        {
            //获得当前登录的Windows用户标示
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);

            //判断当前登录用户是否为管理员
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                AppCode.InitializeSystemSettings();

                Process instance = RunningInstance();
                if (instance == null)
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(CommonValues.LanguageID);
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(CommonValues.LanguageID);

                    PubFuns.AsyncDoWork(prepareDBContext);

                    //bool needLogin = true;
                    //while (needLogin)
                    //{
                    //    LoginForm loginForm = new LoginForm();
                    //    DialogResult loginResult = loginForm.ShowDialog();

                    //    if (loginResult == DialogResult.OK)
                    //    {
                    //        // 登录成功，显示MainForm
                    //        MainForm mainForm = new MainForm();
                    //        mainForm.ShowDialog();

                    //        // 当MainForm关闭时，检查是否需要重新登录
                    //        needLogin = mainForm.IsLoggingOut;
                    //    }
                    //    else
                    //    {
                    //        // 登录失败或用户取消登录，退出循环
                    //        needLogin = false;
                    //    }
                    //}
                    LoginForm loginForm = new LoginForm();
                    loginForm.ShowDialog();
                    Application.Run();
                }
                else
                {
                    HandleRunningInstance(instance);
                }
            }
            else
            {
                //创建启动对象
                ProcessStartInfo startInfo = new ProcessStartInfo();
                //设置运行文件
                startInfo.FileName = Application.ExecutablePath;
                ////设置启动参数
                //startInfo.Arguments = String.Join(" ", Args);

                //设置启动动作,确保以管理员身份运行
                startInfo.Verb = "runas";
                //如果不是管理员，则启动UAC
                Process.Start(startInfo);
                //退出
                Application.Exit();
            }
        }

        //激活framework的数据库连接
        private static void prepareDBContext()
        {
            string dbFileName = PubFuns.GetAbsolutePath("ecg.db");

            try
            {
                ZoncareEntities.Init(dbFileName);

                user oneUser = ZoncareEntities.Instance.User.FirstOrDefault();
                Console.WriteLine(oneUser.id);
                ZoncareEntities.IsReadyDbContext = true;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "用户登录");
            }
        }

        /// <summary>
        /// 获取正在运行的实例
        /// </summary>
        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                    {
                        return process;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 显示已运行的程序。
        /// </summary>
        public static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, SW_MAXIMIZE);    //激活窗口并显示最大化的窗口
            ShowWindowAsync(instance.MainWindowHandle, WS_RESTORE); //激活并显示窗口
            SetForegroundWindow(instance.MainWindowHandle);            //将窗口放到前端
        }

    }
}
