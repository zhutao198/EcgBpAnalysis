using System;
using System.Runtime.InteropServices;

namespace Zoncare.Utilities
{
    public static class WindowUtils
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);

        const int SW_RESTORE = 9;

        public static void ActivateAndShowWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return;

            if (IsIconic(hWnd))
            {
                ShowWindow(hWnd, SW_RESTORE);
            }

            SetForegroundWindow(hWnd);
        }

        public static IntPtr GetMainWindowHandle(int processId)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(processId);
                return process.MainWindowHandle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }

}
