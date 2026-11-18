using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Zoncare.Common;
using Zoncare.Utilities;

namespace AnalysisSystem.Base.softdog
{
    public class WavesGUILib
    {
        #region 方法封装

        /// <summary>
        /// 动态链接库
        /// </summary>
        private const string wavesguiDrivers = @"lib\dongle\WavesGUILib.dll";

        /// <summary>
        /// 加密狗初始化
        /// </summary>
        [DllImport(wavesguiDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_startCheckStatus")]
        private static extern void usb_startCheckStatus();

        /// <summary>
        /// 加密狗停止
        /// </summary>
        [DllImport(wavesguiDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_stopCheckStaus")]
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
        [DllImport(wavesguiDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_registerCallback")]
        private static extern void usb_registerCallback(fnCallBackFunc _fnCallBackFunc);

        #endregion

        #region 方法调用

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public int StartWavesGUI()
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

        public int EndWavesGUI()
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
        public int CallBackWavesGUI(fnCallBackFunc _fnCallBackFunc)
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

    public class wavesGUI
    {
        private static WavesGUILib _wavesGUI = null;
        private static WavesGUILib.fnCallBackFunc _fnCallBackFunc = null;

        public static void RunWinCore()
        {
            try
            {
                if (null == _wavesGUI)
                {
                    _wavesGUI = new WavesGUILib();
                    _fnCallBackFunc = new WavesGUILib.fnCallBackFunc(GetWinCore);
                    _wavesGUI.CallBackWavesGUI(_fnCallBackFunc);
                }
                _wavesGUI.StartWavesGUI();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
        }

        /// <summary>
        /// 记录加密狗当前的各种状态(400:连接失败 404:连接成功 406:通信失败 408:通信成功)
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

                //加密狗返回时同时包含404和408两种状态，说明加密狗正常，只包含408说明加密狗被损坏或破解
                if (lstStatus.Contains(408))
                {
                    if (lstStatus.Contains(404))
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
                if (null != _wavesGUI)
                {
                    _wavesGUI.EndWavesGUI();

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

                        int count2 = days / 181;

                        string softStatus = "Disable";
                        if (status)
                            softStatus = "Enable";

                        ini.IniWriteValue("soft", "count2", ClientKey.AESEncrypt(count2.ToString()));
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
