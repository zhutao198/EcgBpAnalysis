using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Zoncare.Common;
using Zoncare.Utilities;

namespace AnalysisSystem.Base.softdog
{
    public class ZoncareDogHandler
    {
        private static ZoncareDog _usbDog = null;
        private static ZoncareDog.fnCallBackFunc _fnCallBackFunc = null; //加密狗回调方法
        private static bool _isFirst = true; //用于程序启动后检查加密狗状态, 仅检查一次
        private static List<int> _statusList = null; //记录加密狗的各种状态
        private static string _softStatusFile = string.Empty; //INI文件

        /// <summary>
        /// 初始化加密狗及其相关回调方法
        /// </summary>
        /// <param name="recordFile">用于记录加密狗使用状态的文件</param>
        /// <param name="initSystemVariables">是否初始化系统内相关变量</param>
        /// <param name="waitforConnection">是否需要等待一段时间以便加密狗进行连接通讯</param>
        public static void Init(string recordFile, bool initSystemVariables = false, bool waitforConnection = false)
        {
            _softStatusFile = recordFile;
            _usbDog = new ZoncareDog();
            if (initSystemVariables)
            {
                CommonValues.SoftRunStatus = true;
                CommonValues.SoftDogStatus = -1;
                _statusList = null;
            }

            _fnCallBackFunc = new ZoncareDog.fnCallBackFunc(getDogStatus);
            _usbDog.GetSoftDogStatus(_fnCallBackFunc);
            _usbDog.startCheckStatus();
            CommonValues.SoftDogInitialized = true;

            if (waitforConnection)
            {
                Stopwatch sw = Stopwatch.StartNew();
                while (CommonValues.SoftRunStatus == false || CommonValues.SoftDogStatus < 0)
                {
                    if (sw.ElapsedMilliseconds < 1000)
                        System.Threading.Thread.Sleep(1);
                    else
                        break;
                }
            }
        }

        #region 加密狗验证相关
        private static void getDogStatus(int status)
        {
            if (CommonValues.VerificationMode == 0)
            {
                CommonValues.SoftDogStatus = status;
            }
            else
            {
                CommonValues.SoftRunStatus = true;
                CommonValues.SoftDogStatus = 2;
                _statusList = new List<int>() { 0, 2 };
                return;
            }

            if (null == _statusList) _statusList = new List<int>();
            if (!_statusList.Contains(status)) _statusList.Add(status);

            if (_isFirst)
            {
                //加密狗返回时同时包含0和2两种状态，说明加密狗正常，只包含2说明加密狗被损坏或破解
                if (_statusList.Contains(2))
                {
                    if (_statusList.Contains(0))
                    {
                        runRecord(true);
                    }
                    else
                    {
                        runRecord(false);
                    }
                    _isFirst = false;
                }
            }
        }

        private static void runRecord(bool status)
        {
            string softStartTime = "";
            string softStatus = "";
            int count1 = 0;
            int count2 = 0;
            try
            {
                if (File.Exists(_softStatusFile))
                {
                    INIFile iniKeyRead = new INIFile(_softStatusFile);
                    softStartTime = ClientKey.AESDecrypt(iniKeyRead.IniReadValue("soft", "time"));
                    softStatus = ClientKey.AESDecrypt(iniKeyRead.IniReadValue("soft", "status"));
                    count1 = Convert.ToInt32(ClientKey.AESDecrypt(iniKeyRead.IniReadValue("soft", "count1")));
                    count2 = Convert.ToInt32(ClientKey.AESDecrypt(iniKeyRead.IniReadValue("soft", "count2")));

                    if ("Disable" == softStatus) //配置文件已标记不可用
                    {
                        CommonValues.SoftRunStatus = false;
                    }
                    else
                    {
                        if (status)//正常状态
                        {
                            DateTime dtStart = Convert.ToDateTime(softStartTime);
                            int days = DateTime.Now.Subtract(dtStart).Days; //获取当前时间与软件开始使用加密狗时间的天数差

                            if (count1 < (days / 30))
                            {
                                if (null != _usbDog)
                                {
                                    _usbDog.stopCheckStaus();
                                    _fnCallBackFunc = null;
                                    _usbDog = null;
                                }

                                wincore.RunWinCore();
                                wincore.ReturnStatus += wincore_ReturnStatus;
                            }

                            if (count2 < (100 + days / 181))
                            {
                                if (null != _usbDog)
                                {
                                    _usbDog.stopCheckStaus();
                                    _fnCallBackFunc = null;
                                    _usbDog = null;
                                }

                                wavesGUI.RunWinCore();
                                wavesGUI.ReturnStatus += wavesGUI_ReturnStatus;
                            }
                        }
                        else //异常状态
                        {
                            modStatusFile();
                            CommonValues.SoftRunStatus = false;
                        }
                    }
                }
                else
                {
                    softStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    softStatus = "Enable";

                    using (StreamWriter sw = new StreamWriter(_softStatusFile, false, Encoding.GetEncoding("GB2312")))
                    {
                        sw.WriteLine("[soft]");
                        sw.WriteLine("time=" + ClientKey.AESEncrypt(softStartTime));//软件开始使用加密狗的时间
                        sw.WriteLine("count1=" + ClientKey.AESEncrypt("0"));//33天计算次数
                        sw.WriteLine("count2=" + ClientKey.AESEncrypt("100"));//187天计算次数
                        sw.WriteLine("status=" + ClientKey.AESEncrypt(softStatus));//软件状态
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.CreateLog(ex.StackTrace + ";" + ex.Message, "main interface");
            }
        }

        private static void wavesGUI_ReturnStatus(object sender, EventArgs e)
        {
            try
            {
                if (null == _usbDog)
                {
                    _usbDog = new ZoncareDog();
                }

                if (null == _fnCallBackFunc)
                {
                    _fnCallBackFunc = new ZoncareDog.fnCallBackFunc(getDogStatus);
                }

                _usbDog.GetSoftDogStatus(_fnCallBackFunc);
                _usbDog.startCheckStatus();

                if (sender is bool)
                {
                    CommonValues.SoftRunStatus = (bool)sender;
                }
            }
            catch (Exception ex)
            {
                //Logger.CreateLog(ex.StackTrace + ";" + ex.Message, "main interface");
            }
        }

        private static void wincore_ReturnStatus(object sender, EventArgs e)
        {
            try
            {
                if (null == _usbDog)
                {
                    _usbDog = new ZoncareDog();
                }

                if (null == _fnCallBackFunc)
                {
                    _fnCallBackFunc = new ZoncareDog.fnCallBackFunc(getDogStatus);
                }

                _usbDog.GetSoftDogStatus(_fnCallBackFunc);
                _usbDog.startCheckStatus();

                if (sender is bool)
                {
                    CommonValues.SoftRunStatus = (bool)sender;
                }
            }
            catch (Exception ex)
            {
                //Logger.CreateLog(ex.StackTrace + ";" + ex.Message, "main interface");
            }
        }

        private static void modStatusFile()
        {
            try
            {
                if (File.Exists(_softStatusFile))
                {
                    INIFile ini = new INIFile(_softStatusFile);
                    if (null != ini)
                    {
                        string softStatus = "Disable";
                        ini.IniWriteValue("soft", "status", ClientKey.AESEncrypt(softStatus));
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.CreateLog(ex.StackTrace + ";" + ex.Message, "main interface");
            }
        }
        #endregion

    }
}
