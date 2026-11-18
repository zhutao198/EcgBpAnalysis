using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AnalysisApp
{
    static class Program
    {
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        [DllImport("SHCore.dll")]
        static extern int SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        enum PROCESS_DPI_AWARENESS
        {
            ProcessDPI_Unaware = 0,
            ProcessDPI_SystemAware = 1,
            ProcessDPI_PerMonitorAware = 2
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //// Windows 10 1703+ 使用 PerMonitorV2
            //if (Environment.OSVersion.Version.Major >= 10)
            //{
            //    SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.ProcessDPI_PerMonitorAware);
            //}
            //else
            //{
            //    SetProcessDPIAware();
            //}

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int.TryParse(args[0], out int ownerHandle);
            int.TryParse(args[1], out int userId);

            AnalysisWindow frmAnalysis = new AnalysisWindow(userId, ownerHandle);
            frmAnalysis.ShowAfterLoad = false;
            Application.Run(frmAnalysis);

            //AnalysisAppContext context = new AnalysisAppContext(frmAnalysis);
            //Application.Run(context);
        }

    }
}
