using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Zoncare.Common
{
    /// <summary>
    /// 系统日志
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// 保存程序运行记录
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="duration"></param>
        public static void LogRuntimeInfo(string msg, long duration)
        {
            if (!CommonValues.IsSaveRuntimeLog) return;

            writeLogFile(msg + $" 耗时:{duration}ms", "程序运行记录" + "--" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 记录应用程序日志
        /// </summary>
        /// <param name="msg">日志信息</param>
        /// <param name="module">模块名称</param>
        public static void CreateLog(string msg, string module)
        {
            writeLogFile(msg, module + "--" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 记录应用程序错误日志
        /// </summary>
        /// <param name="ex">程序执行时发生的错误</param>
        /// <param name="module">模块名称</param>
        /// <param name="checkInnerException">是否检查获取内部错误信息</param>
        public static void CreateLog(Exception ex, string module, bool checkInnerException = false)
        {
            string errMsg = string.Empty;
            if (ex != null)
            {
                if (checkInnerException)
                {
                    errMsg = getErrors(ex);
                }
                else
                {
                    errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                }
            }

            writeLogFile(errMsg, module + "--" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 写日志信息
        /// </summary>
        private static void writeLogFile(string msg, string filename)
        {
            string path = CommonValues.LogPath;
            if (!string.IsNullOrEmpty(path))
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                try
                {
                    StreamWriter sw = File.AppendText(path + "\\" + filename + ".txt");
                    lock (sw)
                    {
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "----" + msg);
                        sw.Close();
                    }
                }
                catch
                {
                }
            }
        }

        //获取错误信息(包含内部错误)
        private static string getErrors(Exception ex)
        {
            StringBuilder sbErrMsg = new StringBuilder();
            sbErrMsg.AppendLine(ex.Message + Environment.NewLine + ex.StackTrace);
            Exception innerEx = ex.InnerException;
            while (true)
            {
                if (innerEx != null)
                {
                    sbErrMsg.AppendLine("---> " + innerEx.Message + Environment.NewLine + innerEx.StackTrace);
                    innerEx = innerEx.InnerException;
                }
                else
                    break;
            }
            return sbErrMsg.ToString();
        }
    }
}
