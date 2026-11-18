using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Zoncare.Common;
using Zoncare.Utilities;

namespace AnalysisSystem.Base.softdog
{
    public class wincorelib
    {
        #region 方法封装

        /// <summary>
        /// 动态链接库
        /// </summary>
        private const string winDrivers = @"lib\dongle\wincorelib.dll";

        /// <summary>
        /// 加密狗初始化
        /// </summary>
        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_startCheckStatus")]
        private static extern void usb_startCheckStatus();

        /// <summary>
        /// 加密狗停止
        /// </summary>
        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_stopCheckStaus")]
        private static extern void usb_stopCheckStaus();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void fnCallBackFunc(int status);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_fnCallBackFunc"></param>
        /// <returns></returns>
        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_registerCallback")]
        private static extern void usb_registerCallback(fnCallBackFunc _fnCallBackFunc);

        #endregion

        #region 方法调用

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public int StartWinCore()
        {
            int result = -1;
            try
            {
                usb_startCheckStatus();
                result = 0;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }

        public int EndWinCore()
        {
            int result = -1;
            try
            {
                usb_stopCheckStaus();
                result = 0;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public int CallBackWinCore(fnCallBackFunc _fnCallBackFunc)
        {
            int result = -1;
            try
            {
                usb_registerCallback(_fnCallBackFunc);
                result = 0;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }

        #endregion
    }

    public class wincore
    {
        private static wincorelib _wincorelib = null;
        private static wincorelib.fnCallBackFunc _fnCallBackFunc = null;

        public static void RunWinCore()
        {
            try
            {
                if (null == _wincorelib)
                {
                    _wincorelib = new wincorelib();
                    _fnCallBackFunc = new wincorelib.fnCallBackFunc(GetWinCore);
                    _wincorelib.CallBackWinCore(_fnCallBackFunc);
                }
                _wincorelib.StartWinCore();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
        }

        /// <summary>
        /// 记录加密狗当前的各种状态(28:连接失败 60:连接成功 38:通信失败 90:通信成功)
        /// </summary>
        private static List<int> lstStatus = null;

        private static int Count = 0;

        public static event EventHandler ReturnStatus;

        private static void GetWinCore(int status)
        {
            try
            {
                if (null == lstStatus)
                    lstStatus = new List<int>();

                if (!lstStatus.Contains(status))
                    lstStatus.Add(status);

                //加密狗返回时同时包含60和90两种状态，说明加密狗正常，只包含90说明加密狗被损坏或破解
                if (lstStatus.Contains(90))
                {
                    if (lstStatus.Contains(60))
                    {
                        changeStatus(true);
                        return;
                    }
                }

                Count++;

                if (Count >= 10)
                {
                    Count = 0;
                    changeStatus(false);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
        }

        private static void changeStatus(bool status)
        {
            try
            {
                if (null != _wincorelib)
                {
                    _wincorelib.EndWinCore();
                    _fnCallBackFunc = null;
                    _wincorelib = null;

                    ReturnStatus?.Invoke(status, null);

                    modSysConfigsFile(status);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
        }

        private static void modSysConfigsFile(bool status)
        {
            try
            {
                string path = PubFuns.GetBaseDirectory() + "\\config\\status.dat";
                if (File.Exists(path))
                {
                    INIFile ini = new INIFile(path);
                    if (null != ini)
                    {
                        string softStart = ClientKey.AESDecrypt(ini.IniReadValue("soft", "time"));
                        DateTime dtStart = Convert.ToDateTime(softStart);
                        int days = DateTime.Now.Subtract(dtStart).Days; //获取当前时间与软件开始使用加密狗时间的天数差

                        int count1 = days / 30;

                        string softStatus = "Disable";
                        if (status)
                            softStatus = "Enable";

                        ini.IniWriteValue("soft", "count1", ClientKey.AESEncrypt(count1.ToString()));
                        ini.IniWriteValue("soft", "status", ClientKey.AESEncrypt(softStatus));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
        }
    }
}
