using System;
using System.Runtime.InteropServices;
using Zoncare.Common;

namespace AnalysisSystem.Base.softdog
{
    public class ZoncareDog
    {
        #region 方法封装

        /// <summary>
        /// 动态链接库(用于各软件的Dll不同, 此处使用2877项目加密狗及Dll)
        /// </summary>
        private const string SoftDogDrivers = @"lib\dongle\ZonCare.DogDriver.dll";

        /// <summary>
        /// 加密狗初始化
        /// </summary>
        [DllImport(SoftDogDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_startCheckStatus")]
        private static extern void usb_startCheckStatus();

        /// <summary>
        /// 加密狗停止
        /// </summary>
        [DllImport(SoftDogDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_stopCheckStaus")]
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
        [DllImport(SoftDogDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "usb_registerCallback")]
        private static extern void usb_registerCallback(fnCallBackFunc _fnCallBackFunc);

        #endregion

        #region 方法调用

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public int startCheckStatus()
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

        public int stopCheckStaus()
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
        public int GetSoftDogStatus(fnCallBackFunc _fnCallBackFunc)
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
}
