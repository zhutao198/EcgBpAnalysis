using ABPMRepository;
using AnalysisSystem.Base.comm;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Widgets;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisSystem.Base
{
    public class AppCode
    {
        const int HWND_BROADCAST = 0xffff;
        const int WM_SETTINGCHANGE = 0x001A;
        const int SMTO_ABORTIFHUNG = 0x0002;
        const int SMTO_NOTIMEOUTIFNOTHUNG = 0x0008;
        const int HEADER_SIZE_8377 = 300;

        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        /// <summary>
        /// 加载系统控制参数
        /// </summary>
        private static void LoadAppConfig()
        {
            string filePath = Path.Combine(PubFuns.GetBaseDirectory(), "app-config.xml");
            if (!File.Exists(filePath)) return;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.Load(filePath);

                XmlNode root = xmlDoc.SelectSingleNode("Settings");
                foreach (XmlNode category in root.ChildNodes)
                {
                    if (category.NodeType == XmlNodeType.Element)
                    {
                        foreach (XmlNode ele in category.ChildNodes)
                        {
                            if (ele.NodeType == XmlNodeType.Element)
                            {
                                switch (ele.Name)
                                {
                                    case "SystemType":
                                        CommonValues.SystemType = ele.InnerText;
                                        break;
                                    case "HospitalCode":
                                        CommonValues.HospitalCode = ele.InnerText;
                                        break;
                                    case "VerificationMode":
                                        CommonValues.VerificationMode = Convert.ToInt32(ele.InnerText);
                                        break;
                                    case "IsRememberLoginAccount":
                                        CommonValues.IsRememberLoginAccount = ele.InnerText.Equals("1");
                                        break;
                                    case "CultureInfo":
                                        CommonValues.LanguageID = Convert.ToInt32(ele.InnerText);
                                        break;
                                    case "ShowPrinterPropertyBeforePrint":
                                        CommonValues.IsShowParamBeforePrint = ele.InnerText.Equals("1");
                                        break;
                                    case "ShowPreviewBeforePrint":
                                        CommonValues.IsPreviewBeforePrint = ele.InnerText.Equals("1");
                                        break;
                                    case "SaveOperateLog":
                                        CommonValues.IsSaveOperateLog = ele.InnerText.Equals("1");
                                        break;
                                    case "SaveRuntimeLog":
                                        CommonValues.IsSaveRuntimeLog = ele.InnerText.Equals("1");
                                        break;
                                    case "CommBaudRate":
                                        CommonValues.CommBaudRate = Convert.ToInt32(ele.InnerText);
                                        break;

                                    case "ReportGridStyle":
                                        CommonValues.ReportGridStyle = Convert.ToInt32(ele.InnerText);
                                        break;
                                    case "GridColor1":
                                        CommonValues.ReportGridColor1 = ColorTranslator.FromHtml(ele.InnerText);
                                        break;
                                    case "GridColor5":
                                        CommonValues.ReportGridColor5 = ColorTranslator.FromHtml(ele.InnerText);
                                        break;
                                    case "BackgroundColor":
                                        CommonValues.ReportBackgroundColor = ColorTranslator.FromHtml(ele.InnerText);
                                        break;
                                    case "WaveColor":
                                        CommonValues.ReportWaveColor = ColorTranslator.FromHtml(ele.InnerText);
                                        break;
                                    case "WebCaseSource":
                                        CommonValues.WebCaseSource = ele.InnerText;
                                        break;
                                    case "ThirdpartyURI":
                                        CommonValues.ThirdpartyAddress = ele.InnerText;
                                        break;
                                    case "RegionalRecognitionAbbr":
                                        CommonValues.RegionalRecognitionAbbr = ele.InnerText;
                                        break;
                                    case "RegionalRecognition":
                                        CommonValues.RegionalRecognition = ele.InnerText;
                                        break;
                                    case "DefaultEmptyText":
                                        CommonValues.DefaultEmptyValueText = ele.InnerText;
                                        break;
                                    case "PaceBoost":
                                        bool boostParsed = float.TryParse(ele.InnerText, out float paceBoostVal);
                                        if (boostParsed) CommonValues.PaceBoostHeight = paceBoostVal;
                                        break;
                                    case "VerifyFileCRC":
                                        CommonValues.VerifyFileCRC = ele.InnerText.Equals("1");
                                        break;
                                    case "ReduceSampleAble":
                                        CommonValues.ReduceSampleAble = ele.InnerText.Equals("1");
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 检查当前加密狗状态
        /// </summary>
        /// <returns></returns>
        public static bool CheckUsbDogStatus(IWin32Window owner = null)
        {
            if (CommonValues.SystemType == "bp") return true;

            bool result = false;
            try
            {
                if (!CommonValues.SoftRunStatus)
                {
                    CommonFuns.PromptError("加密狗验证失败");
                    return result;
                }

                switch (CommonValues.SoftDogStatus)
                {
                    case -1: CommonFuns.PromptError(owner, "未检测到加密狗"); break;
                    case 0: ScreenCenterTip.Show("加密狗验证成功，正在验证加密狗状态"); break;
                    case 1: CommonFuns.PromptError(owner, "加密狗验证失败"); break;
                    case 2: result = true; break;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "主程序");
            }
            return result;
        }

        /// <summary>
        /// 检查软件运行的注册码
        /// </summary>
        /// <param name="machineCode">机器码(计算机)</param>
        /// <param name="registerString">注册码(激活码)</param>
        /// <returns></returns>
        public static bool CheckActivationStatus(string machineCode, string registerString)
        {
            try
            {
                string encryptCode = string.Empty;
                string mixCode = machineCode + "ECG20NT";
                string shCode = ClientKey.SHA256EncryptString(mixCode);

                if (!string.IsNullOrWhiteSpace(shCode))
                {
                    encryptCode = ClientKey.GetMD5(shCode);
                }

                return (encryptCode.ToLower() == registerString.ToLower());
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
                return false;
            }
        }

        /// <summary>
        /// 初始化系统参数及外部相关设置
        /// </summary>
        /// <param name="readRunningInfo">是否对系统运行时参数进行读取并初始化</param>
        public static void InitializeSystemSettings()
        {
            LoadAppConfig();

            CommonValues.LogPath = PubFuns.GetBaseDirectory() + "\\log";
            CommonValues.WidthScale = Screen.PrimaryScreen.WorkingArea.Width / SysConstants.BEST_WIDTH;
            CommonValues.HeightScale = Screen.PrimaryScreen.WorkingArea.Height / SysConstants.BEST_HEIGHT;
            if (CommonValues.WidthScale > 1) CommonValues.WidthScale = 1;
            if (CommonValues.HeightScale > 1) CommonValues.HeightScale = 1;

            //LoadEcgSettings();
            foreach (int langKey in SysConstants.Locales.Keys)
            {
                if (langKey == 2052)
                {
                    CommonValues.LangsSettings[langKey] = new LanguageSettings()
                    {
                        AdjustFontScale = 1.0F,
                        LineHeightRatio = 1.0F
                    };
                }
                else
                {
                    CommonValues.LangsSettings[langKey] = new LanguageSettings()
                    {
                        AdjustFontScale = 0.85F,
                        LineHeightRatio = 1.5F
                    };
                }
            }

            CommonValues.Langs = LoadLanguage(CommonValues.LanguageID);
        }

        /// <summary>
        /// 修改系统参数设置
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="value"></param>
        public static void UpdateSystemSetting(string nodeName, string value)
        {
            string filePath = Path.Combine(PubFuns.GetBaseDirectory(), "app-config.xml");
            if (!File.Exists(filePath)) return;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(filePath);
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("Settings").ChildNodes;
            foreach (XmlNode node in nodeList)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.Name == nodeName)
                    {
                        subNode.InnerText = value;
                        break;
                    }
                }
            }
            xmlDoc.Save(filePath);
        }

        /// <summary>
        /// 修改系统参数设置
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="value"></param>
        public static void UpdateSystemSetting(NameValueCollection paraValues)
        {
            string filePath = Path.Combine(PubFuns.GetBaseDirectory(), "app-config.xml");
            if (!File.Exists(filePath)) return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(filePath);
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("Settings").ChildNodes;
            foreach (XmlNode node in nodeList)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (paraValues.AllKeys.Contains(subNode.Name, StringComparer.InvariantCultureIgnoreCase))
                    {
                        subNode.InnerText = paraValues[subNode.Name];
                    }
                }
            }
            xmlDoc.Save(filePath);
        }

        /// <summary>
        /// 加载与当前语言相关的词条资源
        /// </summary>
        /// <param name="lcid">语言区域标识</param>
        /// <returns></returns>
        public static PersistDictionary<string> LoadLanguage(int lcid)
        {
            PersistDictionary<string> dict = new PersistDictionary<string>();
            string langFile = Path.Combine(PubFuns.GetBaseDirectory(), "language", SysConstants.Locales[lcid] + ".xml");
            if (File.Exists(langFile))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(langFile);
                XmlNodeList childNodes = xmlDocument.GetElementsByTagName("language")[0].ChildNodes;
                foreach (var obj in childNodes)
                {
                    XmlNode xmlNode = (XmlNode)obj;
                    if (xmlNode.Name.Equals("Item", StringComparison.InvariantCulture))
                    {
                        string key = xmlNode.Attributes["key"].Value;
                        string val = xmlNode.Attributes["value"].Value;
                        if (!dict.TryGetValue(key, out string uiText))
                        {
                            dict[key] = val;
                        }
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// 进入分析模块
        /// </summary>
        /// <param name="commClient"></param>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public static bool EnterAnalysisApp(PipeClient commClient, int caseId)
        {
            if (Process.GetProcessesByName("AnalysisApp").Length == 0)
            {
                return false;
            }
            else
            {
                commClient.SendMessage($"caseId|{caseId}");
                return true;
            }
        }

        /// <summary>
        /// 创建病例数据文件夹
        /// </summary>
        /// <param name="caseInfo"></param>
        public static string CreateCaseDirectory(examCase caseInfo, bool isUpload = false)
        {
            //createTime 缺失时兜底，避免下面 createTime.Value 抛异常
            DateTime createTime = caseInfo.createTime ?? DateTime.Now;
            string dirName = PubFuns.SanitizeFileName(caseInfo.caseNo + "-" + caseInfo.id + "-" + createTime.ToString("yyMMddHHmmss"));
            string rootPath = Path.Combine(CommonValues.SystemSettings.baseSets.holterDataPath, dirName);

            if (!string.IsNullOrEmpty(caseInfo.filePath))
            {
                rootPath = caseInfo.filePath;
            }
            else
            {
                caseInfo.filePath = rootPath;
                caseInfo.fileDirectory = CommonValues.SystemSettings.baseSets.holterDataPath;
            }

            if (isUpload)
            {
                if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true);
                Directory.CreateDirectory(rootPath);
            }
            else
            {
                if (!Directory.Exists(rootPath)) { Directory.CreateDirectory(rootPath); }
            }

            return rootPath;
        }

        /// <summary>
        /// 保存病例信息
        /// </summary>
        /// <param name="registerType"></param>
        /// <param name="patient"></param>
        /// <param name="isAdd"></param>
        /// <param name="deviceType"></param>
        /// <param name="updateRecordTime"></param>
        /// <returns></returns>
        public static bool SaveCaseInfo(int? registerType, examCase patient, bool isAdd, DateTime nowTime,
            int? deviceType = null, bool updateRecordTime = false, DateTime? oldRecordTime = null, ZoncareEntities dbEntities = null)
        {
            bool result = false;
            bool isNewCase = false;
            bool isNewDbEntities = dbEntities == null;
            examCase dbPatient = null;
            try
            {
                if (dbEntities == null) dbEntities = ZoncareEntities.CreateIndependent();

                Action<examCase, int> saveNewPatient = (patInfo, status) =>
                {
                    isNewCase = true;
                    patient.id = 0;
                    patient.createTime = nowTime;
                    patient.createBy = user.Loginer.userName;
                    patient.status = status;
                    patient.appointmentTime = nowTime;
                    dbEntities.ExamCases.Add(patient);
                };

                if (isAdd)
                {
                    patient.deviceType = deviceType?.ToString();
                    saveNewPatient(patient, 1);
                }
                else if (registerType == 1) //读卡
                {
                    patient.deviceType = deviceType.ToString();
                    if (patient.id <= 0)
                    {
                        saveNewPatient(patient, 1);
                    }
                    else
                    {
                        dbPatient = dbEntities.ExamCases.FirstOrDefault(o => o.id == patient.id);
                        if (dbPatient != null)
                        {
                            if (dbPatient.status >= 2) //已上传/已诊断/已审核的不允许覆盖
                            {
                                saveNewPatient(patient, 1);
                            }
                            else { }
                        }
                        else
                        {
                            saveNewPatient(patient, 1);
                        }
                    }
                }

                if (!isNewCase)
                {
                    if (dbPatient == null) dbPatient = dbEntities.ExamCases.FirstOrDefault(o => o.id == patient.id);

                    PubFuns.CopyPropertyValues(patient, dbPatient);
                    //ZoncareEntities.Instance.Entry(dbPatient).CurrentValues.SetValues(patient);
                    patient = dbPatient;
                }

                examCase caseInfo = patient;
                if (updateRecordTime && !string.IsNullOrEmpty(caseInfo.filePath))
                {
                    Action updateStartTimeOfEcg = () =>
                    {
                        //病人信息页面更新记录时间时，需要同时更新holter.info文件startTime字段
                        string holerInfoPath = Path.Combine(caseInfo.filePath, "holter.info");
                        PubFuns.ModifyJsonFileNode(holerInfoPath, "startTime", caseInfo.recordTime?.ToString(SysConstants.DATETIME_FORMAT));
                        //更新片段图的相关时间
                        UpdateSegmentStartTime(caseInfo.filePath, caseInfo.recordTime.Value, oldRecordTime.Value);
                    };
                    Func<bool> updateStartTimeOfNibp = () =>
                    {
                        int upResult = 0;
                        if (caseInfo.recordTime.Value > oldRecordTime.Value)
                        {
                            TimeSpan span = caseInfo.recordTime.Value - oldRecordTime.Value;
                            upResult = dbEntities.ExecuteRawSql($"UPDATE bp_detail SET [time] = DATETIME([time], '+{span.TotalSeconds} seconds') WHERE ecg_id = {caseInfo.id}");
                        }
                        else
                        {
                            TimeSpan span = oldRecordTime.Value - caseInfo.recordTime.Value;
                            upResult = dbEntities.ExecuteRawSql($"UPDATE bp_detail SET [time] = DATETIME([time], '-{span.TotalSeconds} seconds') WHERE ecg_id = {caseInfo.id}");
                        }
                        return (upResult > 0);
                    };

                    if (caseInfo.checkItem == 1) //心电
                    {
                        updateStartTimeOfEcg();
                    }
                    if (caseInfo.checkItem == 2) //血压
                    {
                        bool updResult = updateStartTimeOfNibp();
                        if (!updResult) { return false; }
                    }
                    else if (caseInfo.checkItem == 3)
                    {
                        updateStartTimeOfEcg();
                        if (!updateStartTimeOfNibp()) { return false; }
                    }
                }

                //caseInfo.updateBy = user.Loginer.userName;
                //caseInfo.updateBy = AnaValues.CurrentUser.userName;
                caseInfo.updateTime = DateTime.Now;
                int ret = dbEntities.SaveChanges();

                if (string.IsNullOrEmpty(caseInfo.filePath) || !Directory.Exists(caseInfo.filePath))
                {
                    bool needSavePath = string.IsNullOrEmpty(caseInfo.filePath);
                    string newDirPath = AppCode.CreateCaseDirectory(caseInfo);
                    if (needSavePath)
                    {
                        //回写 file_path / file_directory 两个字段
                        try { dbEntities.SaveChanges(); }
                        catch
                        {
                            if (Directory.Exists(newDirPath)) Directory.Delete(newDirPath, true);
                            throw;
                        }
                    }
                }

                //保存病例信息至数据目录
                List<bpDetail> bpList = new List<bpDetail>();
                try { bpList = dbEntities.BpDetails.AsNoTracking().Where(o => o.caseId == caseInfo.id).ToList(); }
                catch (Exception ex) { Logger.CreateLog(ex, "患者信息"); }
                Task.Run(() =>
                {
                    SaveArchiveInfo(caseInfo, bpList);
                    if (!isAdd && registerType != 1)
                    {
                        if (!string.IsNullOrEmpty(caseInfo.webSourceId)) SyncWebPatientInfo(caseInfo);
                    }
                });

                result = true;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "患者信息");
            }
            finally
            {
                if (isNewDbEntities)
                {
                    dbEntities?.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 更新片段图起始时间
        /// </summary>
        public static void UpdateSegmentStartTime(string filePath, DateTime recordTime, DateTime oldRecordTime)
        {
            string segmentFilePath = Path.Combine(filePath, "holter-1.pic");
            if (File.Exists(segmentFilePath))
            {
                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                List<JObject> jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent, SysConstants.JsonSettings) ?? new List<JObject>();
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    JObject obj = jsonArray[i];
                    DateTime startTime = DateTime.Parse(obj["tbxStartTime"]?.ToString());
                    if (recordTime > oldRecordTime)
                    {
                        TimeSpan span = recordTime - oldRecordTime;
                        startTime = startTime + span;
                    }
                    else
                    {
                        TimeSpan span = oldRecordTime - recordTime;
                        startTime = startTime - span;
                    }

                    obj["tbxStartTime"] = startTime.ToString(SysConstants.DATETIME_FORMAT);
                }

                // 将更新后的JSON数组写回文件
                string updatedJson = JsonConvert.SerializeObject(jsonArray, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(segmentFilePath, updatedJson);
            }
        }

        /// <summary>
        /// 保存病例归档信息
        /// </summary>
        /// <param name="caseInfo"></param>
        public static void SaveArchiveInfo(examCase caseInfo, List<bpDetail> bpDetails = null)
        {
            string rootPath = caseInfo.filePath;
            long totalSize = 0;
            DirectoryInfo dir = new DirectoryInfo(rootPath);
            foreach (FileInfo fi in dir.GetFiles())
            {
                totalSize += fi.Length;
            }

            try
            {
                object archiveInfo = new
                {
                    ecg = caseInfo,
                    bpDetail = new { list = bpDetails ?? new List<bpDetail>() },
                    total = totalSize,
                    createTime = DateTime.Now.ToString(SysConstants.DATETIME_FORMAT)
                };
                string fileContent = JsonConvert.SerializeObject(archiveInfo, SysConstants.JsonSettings);
                File.WriteAllText(Path.Combine(rootPath, "archiver.info"), fileContent);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "患者信息");
            }
        }

        /// <summary>
        /// 同步更新服务端患者信息
        /// </summary>
        /// <param name="caseInfo"></param>
        public static bool SyncWebPatientInfo(examCase caseInfo)
        {
            string paraText = JsonConvert.SerializeObject(caseInfo, SysConstants.JsonSettings);
            string resp = HttpService.GetWebRequest(CommonValues.WebCaseSource + "/holter/client/patient/edit", "POST", null, paraText, out string errMsg);
            if (!string.IsNullOrEmpty(resp))
            {
                JObject jo = JObject.Parse(resp);
                if (jo["code"]?.ToString() == "0")
                {
                    return true;
                }
            }
            if (!string.IsNullOrEmpty(errMsg))
            {
                Logger.CreateLog("更新服务端患者信息失败：" + errMsg, "患者信息");
            }

            return false;
        }

        /// <summary>
        /// 根据检查类型获取相关设备的状态
        /// </summary>
        /// <param name="checkItem"></param>
        /// <returns></returns>
        public static async Task<DeviceStatus> CheckDeviceStatus(int checkItem)
        {
            int result = 0; // 0: 异常; 1:旧心电设备(IE90/IE95); 2:心电(8333); 3:血压(8337); 4:二合一设备(8377);
            string errMsg = null;
            string leadOffAlarm = "";

            var driveInfo = CommonFuns.CheckDrive("ZQ_HOLTER");
            string drive = driveInfo.drive, driveErr = driveInfo.driveErr;
            string diviceFilePath = Path.Combine(drive, "device.ini");
            string holterIniPath = Path.Combine(drive, "holter.ini");
            string nibpIniPath = Path.Combine(drive, "nibp.ini");

            string deviceModel = string.Empty;
            if (string.IsNullOrEmpty(drive))
            {
                errMsg = string.IsNullOrEmpty(driveErr) ? "获取磁盘失败" : driveErr;
            }
            else
            {
                Dictionary<string, string> deviceINI = null;

                if (File.Exists(diviceFilePath))
                {
                    deviceINI = INIFile.ReadIni(diviceFilePath);
                    deviceINI.TryGetValue("deviceModel", out deviceModel);
                }

                if (checkItem == 1) // 动态心电
                {
                    if (File.Exists(holterIniPath) && File.Exists(diviceFilePath))
                    {
                        int newDeviceType = GetNewDeviceType(deviceModel);
                        if (newDeviceType == 2 || newDeviceType == 4)
                            result = newDeviceType;
                        else
                            errMsg = "心电设备异常";
                    }
                    else if (File.Exists(holterIniPath))
                    {
                        result = 1;
                    }
                    else
                    {
                        errMsg = "心电设备异常"; // 可替换为国际化文本
                    }
                }
                else if (checkItem == 2) // 动态血压
                {
                    if (File.Exists(nibpIniPath) && File.Exists(diviceFilePath))
                    {
                        int newDeviceType = GetNewDeviceType(deviceModel);
                        if (newDeviceType == 3 || newDeviceType == 4)
                            result = newDeviceType;
                        else
                            errMsg = "血压设备异常";
                    }
                    else
                    {
                        errMsg = "血压设备异常";
                    }
                }
                else if (checkItem == 3) // 心电血压二合一
                {
                    if (File.Exists(holterIniPath) && File.Exists(nibpIniPath) && File.Exists(diviceFilePath))
                    {
                        int newDeviceType = GetNewDeviceType(deviceModel);
                        if (newDeviceType == 4)
                            result = 4;
                        else
                            errMsg = "心电血压二合一设备异常";
                    }
                    else
                    {
                        errMsg = "心电血压二合一设备异常";
                    }
                }

                // 获取心电导联脱落报警设置参数
                if ((checkItem == 1 || checkItem == 3))
                {
                    if (result == 2 || result == 4)
                    {
                        Dictionary<string, string> holterINI = INIFile.ReadIni(holterIniPath);
                        bool statu = holterINI.TryGetValue("leadAlarm", out leadOffAlarm);
                    }
                }
            }

            return new DeviceStatus
            {
                IsReady = (result > 0),
                DeviceInfo = result > 0 ? new DeviceInfo(result, deviceModel) : null,
                Msg = errMsg,
                LeadOffAlarm = leadOffAlarm,
                DrivePath = drive
            };
        }

        /// <summary>
        /// 读卡之前检查设备及数据状态
        /// </summary>
        /// <returns></returns>
        public static async Task<(bool result, int deviceType, int checkItem, string, examCase)> checkDeviceForRead()
        {
            bool result = false;
            string errMsg = string.Empty;
            int checkItem = 1;  //1:心电; 2:血压; 3:二合一
            int deviceType = 4; //1:旧心电设备(IE90/IE95); 2:心电(8333); 3:血压(8337); 4:二合一设备(8377);
            examCase patientInfo = null;

            try
            {
                var driveInfo = CommonFuns.CheckDrive("ZQ_HOLTER");
                string drive = driveInfo.drive;
                if (!string.IsNullOrEmpty(drive))
                {
                    string deviceIniPath = Path.Combine(drive, "device.ini");
                    string holterIniPath = Path.Combine(drive, "holter.ini");
                    string nibpIniPath = Path.Combine(drive, "nibp.ini");
                    if (File.Exists(deviceIniPath)) //新盒子(8377, 8337, 8333)
                    {
                        Dictionary<string, string> deviceINI = INIFile.ReadIni(deviceIniPath);
                        string wordmode = deviceINI["wordmode"];
                        string deviceModel = deviceINI["deviceModel"];
                        deviceType = GetNewDeviceType(deviceModel);
                        result = true;

                        List<FileInfo> dataFiles = CommonFuns.GetDataFiles(drive, "holter-", "nibp-");
                        if (dataFiles.Count == 0)
                        {
                            result = false;
                            errMsg = CommonValues.Langs["无数据"];
                        }
                        else
                        {
                            Dictionary<string, string> dataINI = null;
                            if (wordmode == "0")
                            {
                                dataINI = INIFile.ReadIni(holterIniPath);
                                checkItem = 1;
                            }
                            else if (wordmode == "1")
                            {
                                dataINI = INIFile.ReadIni(nibpIniPath);
                                checkItem = 2;
                            }
                            else if (wordmode == "2")
                            {
                                dataINI = INIFile.ReadIni(holterIniPath);
                                checkItem = 3;
                            }

                            // 查询病人信息
                            dataINI.TryGetValue("dataId", out string idText);
                            dataINI.TryGetValue("name", out string pName);

                            if (!string.IsNullOrEmpty(idText))
                            {
                                int id = Convert.ToInt32(idText);
                                //本方法运行在后台线程(LoadingForm 的工作线程)，不能使用单例上下文，否则与 UI 线程的查询并发冲突
                                using (var db = ZoncareEntities.CreateIndependent())
                                {
                                    patientInfo = db.ExamCases.AsNoTracking().FirstOrDefault(o => o.id == id && o.name == pName && o.delFlag == 0);
                                }
                            }
                            if (patientInfo == null)
                            {
                                patientInfo = new examCase
                                {
                                    checkItem = checkItem,
                                    caseNo = dataINI["id"],
                                    name = AESEncrypt.Decrypt(pName.Trim('"')),
                                    sex = dataINI["sex"],
                                    age = PubFuns.GetIntValue(AESEncrypt.Decrypt(dataINI["age"].Trim('"'))),
                                    ageUnit = dataINI["ageUnit"],
                                    birthday = AESEncrypt.Decrypt(dataINI["birthday"].Trim('"')),
                                    height = AESEncrypt.Decrypt(dataINI["height"].Trim('"')),
                                    weight = AESEncrypt.Decrypt(dataINI["weight"].Trim('"')),
                                    checkDept = AESEncrypt.Decrypt(dataINI["dept"].Trim('"')),
                                    bedNo = AESEncrypt.Decrypt(dataINI["bed"].Trim('"'))
                                };
                            }
                        }
                    }
                    else //IE90、IE95
                    {
                        result = true;
                        string dataPath = Path.Combine(drive, "holter-1.dat");
                        if (File.Exists(holterIniPath) && File.Exists(dataPath))
                        {
                            checkItem = 1;
                            deviceType = 1;
                        }
                        else
                        {
                            result = false;
                            errMsg = CommonValues.Langs["无数据"];
                        }
                    }
                }
                else //旧血压设备(IE75)
                {
                    result = false;
                    errMsg = CommonValues.Langs["未找到设备"];
                    //ABPMxmlManager abpmManager = new ABPMxmlManager(false, true);
                    //string errCode = await abpmManager.CheckDeviceStatus();
                    //if (!string.IsNullOrEmpty(errCode))
                    //{
                    //    result = false;
                    //    errMsg = CommonValues.Langs["未找到设备"];
                    //}
                    //else
                    //{
                    //    result = true;
                    //    deviceType = 2;
                    //    checkItem = 2;
                    //}
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "设备信息交互");
            }

            return (result, deviceType, checkItem, errMsg, patientInfo);
        }

        /// <summary>
        /// 根据机型确定新设备类型(黑盒子)
        /// </summary>
        /// <param name="deviceModel"></param>
        /// <returns></returns>
        public static int GetNewDeviceType(string deviceModel)
        {
            int deviceType = 0;
            if (SysConstants.DeviceModel8377.Contains(deviceModel, StringComparer.InvariantCultureIgnoreCase))
            {
                deviceType = 4;
            }
            else if (SysConstants.DeviceModel8337.Contains(deviceModel, StringComparer.InvariantCultureIgnoreCase))
            {
                deviceType = 3;
            }
            else if (SysConstants.DeviceModel8333.Contains(deviceModel, StringComparer.InvariantCultureIgnoreCase))
            {
                deviceType = 2;
            }

            return deviceType;
        }

        /// <summary>
        /// 初始化记录卡
        /// </summary>
        /// <param name="drive"></param>
        /// <param name="compressType"></param>
        /// <returns></returns>
        public static bool InitDevice(string drive, string compressType)
        {
            bool result = false;

            try
            {
                string deviceIniPath = Path.Combine(drive, "device.ini");
                string holterIniPath = Path.Combine(drive, "holter.ini");
                string holterCrcPath = Path.Combine(drive, "holter-1.crc");

                if (!File.Exists(deviceIniPath)) // 旧心电设备
                {
                    if (File.Exists(holterIniPath))
                    {
                        File.Delete(holterIniPath);
                    }
                    if (File.Exists(holterCrcPath))
                    {
                        File.Delete(holterCrcPath);
                    }
                }
                else // 二合一设备
                {
                    //恢复 device.ini 文件
                    //ModifyDeviceUsage("0", drive);

                    //恢复 holter.ini
                    examCase entity = new examCase();
                    WriteHolterSettingFile(drive, entity, compressType);

                    //恢复 nibp.ini
                    WriteNibpSettingFile(drive, entity, new BpSettingScheme());
                }

                // 删除 dat 文件
                ClearDeviceData(drive, 0, true);
                result = true;
            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine($"err:{ex.Message}, stack:{ex.StackTrace}");
                Logger.CreateLog(ex, "主程序");
            }
            return result;
        }

        /// <summary>
        /// 清空设备内的历史数据
        /// </summary>
        public static void ClearDeviceData(string drivePath, int checkItem, bool deleteAll = false)
        {
            if (deleteAll || checkItem == 3)
            {
                List<FileInfo> dataFiles = CommonFuns.GetDataFiles(drivePath, "holter-", "nibp-");
                foreach (FileInfo fi in dataFiles)
                {
                    if (fi.Exists) fi.Delete();
                }
            }
            else
            {
                if (checkItem == 1)
                {
                    List<FileInfo> dataFiles = CommonFuns.GetDataFiles(drivePath, "holter-");
                    foreach (FileInfo fi in dataFiles)
                    {
                        if (fi.Exists) fi.Delete();
                    }
                }
                else if (checkItem == 2)
                {
                    List<FileInfo> dataFiles = CommonFuns.GetDataFiles(drivePath, "nibp-");
                    foreach (FileInfo fi in dataFiles)
                    {
                        if (fi.Exists) fi.Delete();
                    }
                }
            }
        }

        /// <summary>
        /// IE95设备数据上传
        /// </summary>
        /// <param name="entity">病例信息</param>
        /// <param name="ignoreCRC">是否忽略CRC验证</param>
        /// <returns></returns>
        public static async Task<RETURN_CODE> UploadOldHolter(examCase entity, string sourcePath = "")
        {
            if (string.IsNullOrEmpty(sourcePath))
            {
                var driveInfo = CommonFuns.CheckDrive("ZQ_HOLTER");
                sourcePath = driveInfo.drive;
            }

            if (string.IsNullOrEmpty(sourcePath)) { return RETURN_CODE.NotFound; }

            //清空并创建病例数据文件夹
            string rootPath = CreateCaseDirectory(entity, true);

            //过滤需上传的文件
            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);
            FileInfo[] files = dirInfo.GetFiles();
            long totalSize = 0;
            bool iniFileExist = false, datFileExist = false;
            List<FileInfo> fileList = new List<FileInfo>();
            foreach (FileInfo fi in files)
            {
                if (fi.Name == "holter.ini")
                {
                    iniFileExist = true;
                    fileList.Add(fi);
                    totalSize += fi.Length;
                }
                else if (fi.Name.StartsWith("holter-1") && fi.Name.EndsWith("dat"))
                {
                    datFileExist = true;
                    fileList.Add(fi);
                    totalSize += fi.Length;
                }
            }
            if (!iniFileExist || !datFileExist) { return RETURN_CODE.FileNotFound; }

            CaseProcessState opState = entity.ProcessState;

            //校验CRC
            entity.ProcessState.Code = -2;
            if (!await FileDataHandler.Crc32Verify_IE95(files, Path.Combine(sourcePath, "holter-1.crc")))
            {
                return RETURN_CODE.VerfifyFail;
            }

            //开始上传
            opState.Code = 0;
            WorkProgress uploadProgress = opState.SubProgresses["upload"];
            Action dataProcess = () =>
            {
                try
                {
                    long totalReadBytes = 0;
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        FileInfo fi = fileList[i];
                        string targetFileName = Path.Combine(rootPath, fi.Name);
                        using (FileStream sourceStream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
                        using (FileStream destinationStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[40960]; //40KB, 可以调整缓冲区大小
                            int bytesRead;
                            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                destinationStream.Write(buffer, 0, bytesRead);
                                totalReadBytes += bytesRead;

                                // 计算进度并显示
                                double ps = (double)totalReadBytes / totalSize * 100;
                                uploadProgress.percent = Math.Min(99, (int)ps);
                            }
                        }

                        //文件重命名
                        if (fi.Name.StartsWith("holter-1") && fi.Name.EndsWith("dat"))
                        {
                            File.Move(targetFileName, Path.Combine(rootPath, "holter.dat"));
                        }

                        if (i == fileList.Count - 1)
                        {
                            //心电文件转换
                            List<FileInfo> holterFiles = CommonFuns.GetDataFiles(rootPath, "holter");

                            //生成holter.info文件
                            dynamic holterInfo = genHolterInfo(entity, holterFiles);
                            if (entity.recordTime == null) entity.recordTime = DateTime.Parse(holterInfo.startTime);
                            entity.dataType = holterInfo.lead;
                            entity.dataDuration = (holterInfo.total / holterInfo.sampleRate).ToString("F2");

                            if (entity.status < 2) entity.status = 2;
                            entity.filePath = rootPath;
                            entity.fileDirectory = Directory.GetParent(rootPath).FullName;
                            entity.deviceType = "1";
                            if (entity.uploadTime == null) entity.uploadTime = DateTime.Now;
                            entity.updateTime = DateTime.Now;
                            entity.updateBy = user.Loginer.userName;
                            //ZoncareEntities.Instance.SaveChanges();
                            SaveCaseInfo(null, entity, false, DateTime.Now);
                            uploadProgress.percent = 100;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "文件上传");
                    opState.Code = 400;
                    Task.Run(() => Directory.Delete(rootPath, true));
                }
            };
            Task.Run(dataProcess);

            return RETURN_CODE.OK;
        }

        /// <summary>
        /// 从二合一设备上传之前检查文件
        /// </summary>
        /// <param name="caseInfo"></param>
        /// <returns></returns>
        public static RETURN_CODE UploadCheck(examCase caseInfo)
        {
            var driveInfo = CommonFuns.CheckDrive("ZQ_HOLTER");
            string drive = driveInfo.drive;
            if (string.IsNullOrEmpty(drive))
            {
                return RETURN_CODE.NotFound; //动态心电血压二合一设备上传时未连接
            }
            if (!File.Exists(Path.Combine(drive, "device.ini")))
            {
                return RETURN_CODE.FileNotFound;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(drive);
            FileInfo[] files = dirInfo.GetFiles();
            if (caseInfo.checkItem == 1)
            {
                if (!files.Any(f => f.Name == "holter.ini")
                    || !files.Any(f => f.Name.StartsWith("holter-") && f.Name.EndsWith("dat")))
                {
                    return RETURN_CODE.FileNotFound;
                }
            }
            else if (caseInfo.checkItem == 2)
            {
                if (!files.Any(f => f.Name == "nibp.ini")
                    || !files.Any(f => f.Name.StartsWith("nibp-") && f.Name.EndsWith("dat")))
                {
                    return RETURN_CODE.FileNotFound;
                }
            }
            else if (caseInfo.checkItem == 3)
            {
                if (!files.Any(f => f.Name == "holter.ini")
                    || !files.Any(f => f.Name.StartsWith("holter-") && f.Name.EndsWith("dat"))
                    || !files.Any(f => f.Name.StartsWith("nibp-") && f.Name.EndsWith("dat")))
                {
                    return RETURN_CODE.FileNotFound;
                }
            }
            else
            {
                return RETURN_CODE.Err;
            }

            // 检查是否为同一份病例
            if (caseInfo.checkItem == 1)
            {
                Dictionary<string, string> holterINI = INIFile.ReadIni(Path.Combine(drive, "holter.ini"));
                if (!holterINI.ContainsKey("dataId") || holterINI["dataId"] != caseInfo.id.ToString())
                {
                    return RETURN_CODE.MisMatching;
                }
            }
            else if (caseInfo.checkItem == 2)
            {
                Dictionary<string, string> bpINI = INIFile.ReadIni(Path.Combine(drive, "nibp.ini"));
                if (!bpINI.ContainsKey("dataId") || bpINI["dataId"] != caseInfo.id.ToString())
                {
                    return RETURN_CODE.MisMatching;
                }
            }
            else
            {
                Dictionary<string, string> holterINI = INIFile.ReadIni(Path.Combine(drive, "holter.ini"));
                Dictionary<string, string> bpINI = INIFile.ReadIni(Path.Combine(drive, "nibp.ini"));
                if (holterINI.ContainsKey("dataId") && holterINI["dataId"] == caseInfo.id.ToString()
                    && bpINI.ContainsKey("dataId") && bpINI["dataId"] == caseInfo.id.ToString())
                {
                }
                else
                {
                    return RETURN_CODE.MisMatching;
                }
            }
            return RETURN_CODE.OK;
        }

        /// <summary>
        /// 8377二合一设备数据上传
        /// </summary>
        /// <param name="entity">病例信息</param>
        /// <param name="ignoreCRC">是否忽略CRC验证</param>
        /// <returns></returns>
        public static async Task<RETURN_CODE> UploadHolterBp(examCase entity, DeviceInfo deviceInfo, bool ignoreCRC = false, string sourcePath = "")
        {
            if (string.IsNullOrEmpty(sourcePath))
            {
                var driveInfo = CommonFuns.CheckDrive("ZQ_HOLTER");
                sourcePath = driveInfo.drive;
            }

            if (string.IsNullOrEmpty(sourcePath)) { return RETURN_CODE.NotFound; }

            //清空并创建病例数据文件夹
            string rootPath = CreateCaseDirectory(entity, true);

            //过滤需上传的文件
            DirectoryInfo dirInfo = new DirectoryInfo(sourcePath);
            FileInfo[] files = dirInfo.GetFiles();
            long totalSize = 0;
            List<FileInfo> fileList = new List<FileInfo>();
            foreach (FileInfo fi in files)
            {
                if (entity.checkItem == 1)
                {
                    if (fi.Name == "device.ini" || fi.Name == "holter.ini" || (fi.Name.StartsWith("holter-") && fi.Name.EndsWith("dat")))
                    {
                        fileList.Add(fi);
                        totalSize += fi.Length;
                    }
                }
                else if (entity.checkItem == 2)
                {
                    if (fi.Name == "device.ini" || fi.Name == "nibp.ini" || (fi.Name.StartsWith("nibp-") && fi.Name.EndsWith("dat")))
                    {
                        fileList.Add(fi);
                        totalSize += fi.Length;
                    }
                }
                else
                {
                    if (fi.Name == "device.ini" || fi.Name == "holter.ini" || fi.Name == "nibp.ini"
                        || (fi.Name.StartsWith("holter-") && fi.Name.EndsWith("dat"))
                        || (fi.Name.StartsWith("nibp-") && fi.Name.EndsWith("dat")))
                    {
                        fileList.Add(fi);
                        totalSize += fi.Length;
                    }
                }
            }

            CaseProcessState opState = entity.ProcessState;

            //校验CRC
            if (!ignoreCRC)
            {
                opState.Code = -2;
                if (!await FileDataHandler.Crc32Verify(files, Path.Combine(sourcePath, "crc32.txt"), entity.checkItem))
                {
                    return RETURN_CODE.VerfifyFail;
                }
            }

            int maxbpId = 0;
            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            IQueryable<bpDetail> bpDetailQuery = dbEntities.BpDetails.Where(o => o.caseId.Equals(entity.id));
            if (bpDetailQuery.Any()) maxbpId = bpDetailQuery.Max(o => o.id);

            //开始上传
            opState.Code = 0;
            WorkProgress uploadProgress = opState.SubProgresses["upload"];
            Action dataProcess = () =>
            {
                using (var tran = dbEntities.Database.BeginTransaction())
                {
                    try
                    {
                        DateTime bpStartTime = DateTime.Now;
                        long totalReadBytes = 0;
                        for (int i = 0; i < fileList.Count; i++)
                        {
                            FileInfo fi = fileList[i];
                            using (FileStream sourceStream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
                            using (FileStream destinationStream = new FileStream(Path.Combine(rootPath, fi.Name), FileMode.Create, FileAccess.Write))
                            {
                                byte[] buffer = new byte[40960]; //40KB, 可以调整缓冲区大小
                                int bytesRead;
                                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    destinationStream.Write(buffer, 0, bytesRead);
                                    totalReadBytes += bytesRead;

                                    // 计算进度并显示
                                    double ps = (double)totalReadBytes / totalSize * 100;
                                    uploadProgress.percent = Math.Min(99, (int)ps);
                                }
                            }

                            if (fi.Name.StartsWith("nibp-") && fi.Name.EndsWith(".dat"))
                            {
                                string timeString = fi.Name.Substring("nibp-".Length, 14);
                                bool timeParsed = DateTime.TryParseExact(timeString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime fileTime);
                                if (timeParsed && fileTime < bpStartTime) bpStartTime = fileTime;

                                //解析血压数据文件
                                FileDataHandler.ParseBpFile(dbEntities, fi, entity.id);
                            }

                            if (i == fileList.Count - 1)
                            {
                                if (entity.checkItem != (int)CheckItem.ECG)
                                {
                                    //删除历史血压数据
                                    var oldBpData = dbEntities.BpDetails.Where(o => o.caseId.Equals(entity.id) && o.id <= maxbpId);
                                    dbEntities.BpDetails.RemoveRange(oldBpData);
                                }

                                if (entity.checkItem != (int)CheckItem.BP)
                                {
                                    //心电文件转换
                                    List<FileInfo> holterFiles = CommonFuns.GetDataFiles(rootPath, "holter");

                                    //生成holter.info文件
                                    dynamic holterInfo = genHolterInfo(entity, holterFiles);
                                    if (entity.recordTime == null) entity.recordTime = DateTime.Parse(holterInfo.startTime);
                                    entity.dataType = holterInfo.lead;
                                    entity.dataDuration = (holterInfo.total / holterInfo.sampleRate).ToString("F2");
                                }
                                else if (entity.checkItem == (int)CheckItem.BP)
                                {
                                    if (entity.recordTime == null) entity.recordTime = bpStartTime;
                                }

                                if (entity.status < 2) entity.status = 2;
                                entity.filePath = rootPath;
                                entity.fileDirectory = Directory.GetParent(rootPath).FullName;
                                entity.deviceType = deviceInfo.DeviceType.ToString();
                                entity.deviceType = deviceInfo.DeviceModel;
                                if (entity.uploadTime == null) entity.uploadTime = DateTime.Now;
                                entity.updateTime = DateTime.Now;
                                entity.updateBy = user.Loginer.userName;

                                bool entitySaved = SaveCaseInfo(null, entity, false, DateTime.Now, dbEntities: dbEntities);
                                if (!entitySaved) throw new Exception($"文件上传失败(病历号:{entity.caseNo})");
                                uploadProgress.percent = 100;
                            }
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Logger.CreateLog(ex, "文件上传");
                        opState.Code = 400;
                        Task.Run(() => Directory.Delete(rootPath, true));
                    }
                }
            };
            Task.Run(dataProcess);

            return RETURN_CODE.OK;
        }

        /// <summary>
        /// 8337血压监测设备数据上传
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ignoreCRC"></param>
        /// <returns></returns>
        public static async Task<RETURN_CODE> UploadBpFor8337(examCase entity, DeviceInfo deviceInfo, bool ignoreCRC = false, string sourcePath = "")
        {
            string crcFilePath = "";
            if (string.IsNullOrEmpty(sourcePath))
            {
                var driveInfo = CommonFuns.CheckDrive("ZQ_HOLTER");
                sourcePath = driveInfo.drive;
            }

            if (string.IsNullOrEmpty(sourcePath)) { return RETURN_CODE.NotFound; }

            crcFilePath = Path.Combine(sourcePath, "crc32.txt");
            if (!File.Exists(crcFilePath)) { return RETURN_CODE.StatusErr; }

            Dictionary<string, string> crcs = INIFile.ReadIni(crcFilePath);
            List<FileInfo> files = FileDataHandler.GetFilesToBeUploadFor8337(sourcePath, crcs);
            if (files.Count == 0) { return RETURN_CODE.StatusErr; }


            //清空并创建病例数据文件夹
            string rootPath = CreateCaseDirectory(entity, true);

            //过滤需上传的文件
            long totalSize = 0;
            List<FileInfo> fileList = new List<FileInfo>();
            foreach (FileInfo fi in files)
            {
                if (fi.Name == "device.ini" || fi.Name == "nibp.ini" || fi.Name == "nibp_test_result.dat" || (fi.Name.StartsWith("nibp-") && fi.Name.EndsWith("dat")))
                {
                    fileList.Add(fi);
                    totalSize += fi.Length;
                }
            }

            CaseProcessState opState = entity.ProcessState;

            //校验CRC
            if (!ignoreCRC)
            {
                opState.Code = -2;
                if (!await FileDataHandler.Crc32Verify(files, crcFilePath, entity.checkItem))
                {
                    return RETURN_CODE.VerfifyFail;
                }
            }

            int maxbpId = 0;
            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            IQueryable<bpDetail> bpDetailQuery = dbEntities.BpDetails.Where(o => o.caseId.Equals(entity.id));
            if (bpDetailQuery.Any()) maxbpId = bpDetailQuery.Max(o => o.id);

            //开始上传
            opState.Code = 0;
            WorkProgress uploadProgress = opState.SubProgresses["upload"];
            Action dataProcess = () =>
            {
                using (var tran = dbEntities.Database.BeginTransaction())
                {
                    List<bpDetail> dataList = null;
                    try
                    {
                        DateTime bpStartTime = DateTime.Now;
                        long totalReadBytes = 0;
                        for (int i = 0; i < fileList.Count; i++)
                        {
                            FileInfo fi = fileList[i];
                            using (FileStream sourceStream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
                            using (FileStream destinationStream = new FileStream(Path.Combine(rootPath, fi.Name), FileMode.Create, FileAccess.Write))
                            {
                                byte[] buffer = new byte[40960]; //40KB, 可以调整缓冲区大小
                                int bytesRead;
                                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    destinationStream.Write(buffer, 0, bytesRead);
                                    totalReadBytes += bytesRead;

                                    // 计算进度并显示
                                    double ps = (double)totalReadBytes / totalSize * 100;
                                    uploadProgress.percent = Math.Min(99, (int)ps);
                                }
                            }

                            if (fi.Name == "nibp_test_result.dat")
                            {
                                dataList = FileDataHandler.ParseBpFileFor8337(dbEntities, fi, entity.id);
                            }

                            if (i == fileList.Count - 1)
                            {
                                //删除历史血压数据
                                var oldBpData = dbEntities.BpDetails.Where(o => o.caseId.Equals(entity.id) && o.id <= maxbpId);
                                dbEntities.BpDetails.RemoveRange(oldBpData);

                                if (entity.status < 2) entity.status = 2;
                                entity.filePath = rootPath;
                                entity.fileDirectory = Directory.GetParent(rootPath).FullName;
                                entity.deviceType = deviceInfo.DeviceType.ToString();
                                entity.deviceModel = deviceInfo.DeviceModel;
                                if (entity.uploadTime == null) entity.uploadTime = DateTime.Now;
                                entity.updateTime = DateTime.Now;
                                entity.updateBy = user.Loginer.userName;
                                if (dataList != null)
                                {
                                    DateTime minTime = DateTime.Parse(dataList.Min(o => o.time));
                                    DateTime maxTime = DateTime.Parse(dataList.Max(o => o.time));
                                    if (entity.recordTime == null) entity.recordTime = minTime;
                                    entity.dataDuration = maxTime.Subtract(minTime).TotalSeconds.ToString("F2");
                                }

                                bool entitySaved = SaveCaseInfo(null, entity, false, DateTime.Now, dbEntities: dbEntities);
                                if (dataList == null || !entitySaved) throw new Exception($"文件上传失败(病历号:{entity.caseNo})");
                                uploadProgress.percent = 100;
                            }
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Logger.CreateLog(ex, "文件上传");
                        opState.Code = 400;
                        Task.Run(() => Directory.Delete(rootPath, true));
                    }
                }
            };
            Task.Run(dataProcess);

            return RETURN_CODE.OK;
        }


        private static dynamic genHolterInfo(examCase caseInfo, List<FileInfo> holterFiles)
        {
            string rootPath = caseInfo.filePath;
            Dictionary<string, string> holterINI = INIFile.ReadIni(Path.Combine(rootPath, "holter.ini"));
            int leadCount = holterINI.GetIntValue("Lead", 12, true).Value;
            dynamic config = configTemplate(GetDefaultLeadInfo(leadCount));
            dynamic cf = getHolterConfig(rootPath, holterFiles, holterINI);

            string startTime = holterINI.GetStringValue("Starttime");
            if (!string.IsNullOrEmpty(startTime))
                startTime = CommonFuns.ValidateAndFormatDateTime(startTime);
            else
                startTime = CommonFuns.GetFileNameDate(cf.fileList[0]).ToString(SysConstants.DATETIME_FORMAT);
            config.startTime = startTime;

            config.days = holterINI.GetIntValue("Days", 1);
            //config.sampleRate = new Number(holterINI.Samplerate);
            config.sampleBits = holterINI.GetIntValue("Samplebits");
            config.lead = leadCount;
            config.channel = config.lead == 12 ? 8 : 3;
            config.adScale = holterINI.GetFloatValue("AD_SCALE");
            config.version = holterINI.GetStringValue("Version");
            //config.analysisSetting.paceAnalysisEnable = (holterINI.Pace || holterINI.pace) == "On";
            config.analysisSetting.paceAnalysisEnable = false;

            config.fileType = cf.fileType;
            config.fileListRange = cf.fileListRange;
            config.fileList = cf.fileList;
            config.sampleRate = cf.targetSampleRate;

            var dataRanges = cf.fileListRange as int[][];
            //config.total = AppConfig.calcTotal(id, config.channel);
            //config.total = dataRanges.Sum(r => r[1] - r[0] + 1);
            config.total = dataRanges[dataRanges.Length - 1][1] + 1;
            config.analysisSetting.endSample = config.total - 1;

            var hw = new Dictionary<Holter_File_Type, dynamic>
            {
                { Holter_File_Type.IE90_10T12L, new { line = 10, lead = 12 } },
                { Holter_File_Type.IE90_7T13L, new { line = 7, lead = 3 } },
                { Holter_File_Type.IE90_5T3L, new { line = 5, lead = 3 } },
                { Holter_File_Type.OLD_8377_10T12L, new { line = 10, lead = 12 } },
                { Holter_File_Type.NEW_8377_10T12L, new { line = 10, lead = 12 } },
                { Holter_File_Type.NEW_8377_7T12L, new { line = 7, lead = 12 } },
                { Holter_File_Type.NEW_8377_7T3L, new { line = 7, lead = 3 } },
                { Holter_File_Type.NEW_8377_5T3L, new { line = 5, lead = 3 } }
            };
            config.hw = hw[config.fileType];

            string jsonText = JsonConvert.SerializeObject(config, SysConstants.JsonSettings);
            File.WriteAllText(Path.Combine(rootPath, "holter.info"), jsonText);

            return config;
        }

        //从设备获取holter相关配置(对应原holter.Config内容)
        private static dynamic getHolterConfig(string rootPath, List<FileInfo> holterFiles, Dictionary<string, string> holterINI)
        {
            List<FileInfo> files = holterFiles.OrderBy(o => o.Name).ToList();
            List<string> fileList = files.Select(f => f.Name).ToList();
            dynamic config = new ExpandoObject();
            config.dataPath = rootPath;
            config.fileList = fileList;

            int sampleRate = holterINI.GetIntValue("Samplerate", 125).Value;
            int? lead = holterINI.GetIntValue("Lead");
            SampleMapping mappingInfo = default(SampleMapping);
            if (File.Exists(Path.Combine(rootPath, "device.ini"))) //8377
            {
                int? terminalNumber = holterINI.GetIntValue("terminalNumber");
                DateTime startTime = CommonFuns.GetFileNameDate(fileList[0]);
                if (lead == 3)
                {
                    if (terminalNumber == 7) //7芯
                        config.fileType = Holter_File_Type.NEW_8377_7T3L;
                    else
                        config.fileType = Holter_File_Type.NEW_8377_5T3L;
                }
                else
                {
                    if (terminalNumber == 7)
                        config.fileType = Holter_File_Type.NEW_8377_7T12L;
                    else
                        config.fileType = Holter_File_Type.NEW_8377_10T12L;
                }

                config.fileListRange = new int[files.Count][];
                mappingInfo = SysConstants.SampleMaps[sampleRate];
                for (int i = 0; i < files.Count; i++)
                {
                    FileInfo fi = files[i];
                    DateTime dtStart = CommonFuns.GetFileNameDate(fi.Name);

                    (int headerSize, int frameDataSize) = getFrameSize8377(fi.FullName, sampleRate);
                    int startSample = (int)(dtStart.Subtract(startTime).TotalMilliseconds * mappingInfo.mapping / 1000);
                    int endSample = startSample + Convert.ToInt32((double)(fi.Length - headerSize) / frameDataSize);
                    config.fileListRange[i] = new int[2] { startSample, endSample };
                }
                config.sampleRate = mappingInfo.mapping;
                config.targetSampleRate = CommonValues.ReduceSampleAble ? mappingInfo.target : mappingInfo.mapping;
            }
            else
            {
                config.fileListRange = new int[1][];
                Holter_FrameData_Size frameSize = Holter_FrameData_Size.IE90_12;
                if (lead == 12)
                {
                    config.fileType = Holter_File_Type.IE90_10T12L;
                    frameSize = Holter_FrameData_Size.IE90_12;
                }
                else
                {
                    //3导算法需要识别是7芯还是5芯,但没有对应的配置项,此处暂时默认5芯
                    config.fileType = Holter_File_Type.IE90_5T3L;
                    frameSize = Holter_FrameData_Size.IE90_3;
                }
                int endSample = (int)files[0].Length / (int)frameSize;
                config.fileListRange[0] = new int[2] { 0, endSample };
                config.sampleRate = sampleRate;
                config.targetSampleRate = config.sampleRate; ;
            }

            config.frequency = 0;
            config.baseline = 0;
            config.myoelectric = 0;
            return config;
        }

        //获取8377设备每组数据的字节大小
        private static (int headerSize, int frameDataSize) getFrameSize8377(string filePath, int sampleRate)
        {
            Holter_FrameData_Size dataSize = Holter_FrameData_Size.NEW_8377;
            int headerSize = 300;

            if (sampleRate >= 500) { dataSize = Holter_FrameData_Size.NEW_8377; }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[280];
                    fs.Read(buffer, 0, 280);

                    // Read the first 4 bytes as an integer
                    int headerValue = BitConverter.ToInt32(buffer, 0);
                    if (headerValue == 18350082) //2.0
                    {
                        headerSize = 280;
                        dataSize = Holter_FrameData_Size.NEW_8377_20;
                    }
                    else if (headerValue == 18362417) //1.0
                    {
                        headerSize = 280;
                        dataSize = Holter_FrameData_Size.NEW_8377;
                    }
                    else if (headerValue == 19673393) //1.1
                    {
                        headerSize = 300;
                        dataSize = Holter_FrameData_Size.NEW_8377_30;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions here
                Console.Error.WriteLine($"Exception: {ex.Message}");
            }

            return (headerSize, (int)dataSize);
        }

        private static dynamic configTemplate(LeadInfo leadInfo)
        {
            ecgSettingValues ecgSettings = CommonValues.SystemSettings.ecgSets;
            dynamic config = new ExpandoObject();

            // 直接添加属性
            config.fileType = 1; // 0(2023年之前), 1(新格式)
            config.fileList = new string[] { "holter-YYYYMMDDHHmmss.dat", "holter-YYYYMMDDHHmmss.dat" };
            config.fileListRange = new int[][] { new int[] { 0, 10000 }, new int[] { 0, 10000 } }; //文件起始和结束
            config.startTime = null; //开始时间
            config.days = null; //持续天数
            config.sampleRate = null; //采样率
            config.sampleBits = null;
            config.lead = 12; //导联
            config.adScale = null; //ad值
            config.version = null; //版本
            config.total = null; //点个数

            // 分析设置
            dynamic analysisSetting = new ExpandoObject();
            analysisSetting.mainChannel = leadInfo.channelSetting.mainChannel; //主通道
            analysisSetting.secondChannel1 = leadInfo.channelSetting.secondChannel1; //辅助通道1
            analysisSetting.secondChannel2 = leadInfo.channelSetting.secondChannel2; //辅助通道2
            analysisSetting.qrsSimilarityThreshold = 80; //QRS重分析相似度阈值
            analysisSetting.qrsRefractoryPeriod = 280; //QRS不应期入参
            analysisSetting.rSensitivity = 0; //灵敏度
            analysisSetting.stOffset = 60; //ST偏移ms
            analysisSetting.jOffset = 20; //j偏移ms
            analysisSetting.baseOffset = 40; //基线偏移ms
            analysisSetting.frequency = ecgSettings.filterSets.frequency; //工频
            analysisSetting.baseline = ecgSettings.filterSets.baseline;   //基线漂移
            analysisSetting.myoelectric = ecgSettings.filterSets.myoelectric; //肌电
            analysisSetting.paceAnalysisEnable = false; //是否起搏分析
            analysisSetting.paceEventEnable = false; //是否起搏事件分析
            analysisSetting.paceAnalysisType = "DDD"; //起搏器类型
            analysisSetting.paceMethod = 0; //起搏方式
            analysisSetting.pace2beat = 91; //起搏钉到心搏
            analysisSetting.beat2pace = 790; //心搏到起搏钉
            analysisSetting.checkHrUpperLimit = 120; //检查心率上限
            analysisSetting.checkHrLowerLimit = 63; //检查心率下限
            analysisSetting.sensitivity = 0; //伪差剔除灵敏度
            analysisSetting.disturbDelEnable = false; //智能干扰删除
            analysisSetting.multichannelEnable = true; //多通道
            analysisSetting.startSample = 0; //开始分析起始点
            analysisSetting.endSample = null; //开始分析结束点
            analysisSetting.invalidInterval = new int[0][]; //不需要或无效的分析区间
            config.analysisSetting = analysisSetting;
            // ECG参数设置
            config.paramsSetting = new ExpandoObject();
            var customSets = ecgSettings.customSets;
            var advanceSets = ecgSettings.advanceSets;
            var stUpFallSets = ecgSettings.stUpFallSets;
            config.paramsSetting.p6 = new { name = "心动过缓", hr1 = customSets.bradycardia, frequency = customSets.bradycardiaCalc };
            config.paramsSetting.p7 = new { name = "心动过速", hr1 = customSets.tachycardia, frequency = customSets.tachycardiaCalc };
            config.paramsSetting.p8 = new { name = "长RR间期", rr = customSets.longRR, includePrematureBeat = customSets.includePremature };
            config.paramsSetting.p14 = new { name = "最快/最慢心率", frequency = customSets.fastestSlowestHr, AllowEctopicBeat = customSets.allowEctopicBeat };
            config.paramsSetting.p15 = new { name = "ST", low = advanceSets.STLower, low_duration = advanceSets.STLowerDuration, high = advanceSets.STUpper, high_duration = advanceSets.STUpperDuration, stMode = advanceSets.STMode };
            config.paramsSetting.p16 = new { name = "起搏设置", sense_failed = advanceSets.senseFailed, taking_failed = advanceSets.takingFailed, output_failed = advanceSets.outputFailed };
            config.paramsSetting.p17 = new
            {
                name = "心率变异",
                step = advanceSets.analysisStep,
                high1 = advanceSets.highFrequency1,
                high2 = advanceSets.highFrequency2,
                low1 = advanceSets.lowFrequency1,
                low2 = advanceSets.lowFrequency2,
                very_low1 = advanceSets.lowestFrequency1,
                very_low2 = advanceSets.lowestFrequency2
            };
            config.paramsSetting.p18 = new { name = "停搏", rr = customSets.asystoleTime };
            config.paramsSetting.p19 = new { name = "房早", before = customSets.PAC };
            config.paramsSetting.p20 = new { name = "二联三联律设定", refValue = 3 };
            config.paramsSetting.p21 = new
            {
                name = "12导ST异常标准",
                standardValue = new AbnormalOfST[]{
                    new AbnormalOfST { index=0, fall=stUpFallSets.ILeadFall, up=stUpFallSets.ILeadUp },
                    new AbnormalOfST { index=1, fall=stUpFallSets.IILeadFall, up=stUpFallSets.IILeadUp },
                    new AbnormalOfST { index=2, fall=stUpFallSets.IIILeadFall, up=stUpFallSets.IIILeadUp },
                    new AbnormalOfST { index=3, fall=stUpFallSets.aVRLeadFall, up=stUpFallSets.aVRLeadUp },
                    new AbnormalOfST { index=4, fall=stUpFallSets.aVLLeadFall, up=stUpFallSets.aVLLeadUp },
                    new AbnormalOfST { index=5, fall=stUpFallSets.aVFLeadFall, up=stUpFallSets.aVFLeadUp },
                    new AbnormalOfST { index=6, fall=stUpFallSets.V1LeadFall, up=stUpFallSets.V1LeadUp },
                    new AbnormalOfST { index=7, fall=stUpFallSets.V2LeadFall, up=stUpFallSets.V2LeadUp },
                    new AbnormalOfST { index=8, fall=stUpFallSets.V3LeadFall, up=stUpFallSets.V3LeadUp },
                    new AbnormalOfST { index=9, fall=stUpFallSets.V4LeadFall, up=stUpFallSets.V4LeadUp },
                    new AbnormalOfST { index=10, fall=stUpFallSets.V5LeadFall, up=stUpFallSets.V5LeadUp },
                    new AbnormalOfST { index=11, fall=stUpFallSets.V6LeadFall, up=stUpFallSets.V6LeadUp }
                }
            };
            config.paramsSetting.sspeed = new { name = "房速", hr = customSets.ATHr };
            config.paramsSetting.vront = new { name = "室早RonT", rr = customSets.RonT };
            config.paramsSetting.af = new { name = "房颤", min_duration = customSets.minDuration, min_avg_hr = customSets.minAvgHr };
            config.paramsSetting.eb = new { name = "逸搏", spercent = customSets.atrialJunctional, vpercent = customSets.ventricularPrecent };
            config.paramsSetting.twa = new { name = "TWA分析", min = advanceSets.twaMinAvg, duration = advanceSets.twaEventDuration };

            // 导联配置
            config.leadConfig = leadInfo.leadConfig;

            // ST显示设置
            config.stSetting = new { showGrid = true, showThreshold = false };
            config.stOperationSetting = leadInfo.stOperationSetting; // ST 操作设置
            config.waveConfig = leadInfo.waveConfig; // 波形配置

            config.fileConversion = false; // 是否文件转换
            config.fileConversionTime = null; // 文件转换时间
            config.priorAnalysis = false; // 是否预分析
            config.priorAnalysisTime = null; // 预分析时间
            config.startReanalysis = false; // 开始分析状态
            config.startReanalysisFinishTime = null; // 分析完成时间
            config.startReanalysisTime = null; // 开始分析时间
            config.analysisHrv = false; // HRV 分析状态
            config.analysisHrvTime = null; // HRV 分析起始时间

            return config;
        }

        public static LeadInfo GetDefaultLeadInfo(int leadCount)
        {
            Dictionary<string, string[]> stOpSetting = new Dictionary<string, string[]>();
            for (int i = 0; i < leadCount; i++)
            {
                stOpSetting[i.ToString()] = new string[0];
            }

            if (leadCount == 12)
            {
                var result = new LeadInfo
                {
                    data = new LeadData
                    {
                        I = new string[0],
                        II = new string[0],
                        III = new string[0],
                        aVR = new string[0],
                        aVL = new string[0],
                        aVF = new string[0],
                        V1 = new string[0],
                        V2 = new string[0],
                        V3 = new string[0],
                        V4 = new string[0],
                        V5 = new string[0],
                        V6 = new string[0]
                    },
                    props = new[] { "I", "II", "III", "aVR", "aVL", "aVF", "V1", "V2", "V3", "V4", "V5", "V6" },
                    channelSetting = new ChannelSetting
                    {
                        mainChannel = 1,
                        secondChannel1 = 6,
                        secondChannel2 = 10
                    },
                    leadConfig = new LeadConfig
                    {
                        ChestOrg = { "V1", "V2", "V3", "V4", "V5", "V6" },
                        ChestLatest = { "V1", "V2", "V3", "V4", "V5", "V6" },
                        LfLatest = { "R", "L", "F" },
                        LfOrg = { "R", "L", "F" }
                    },
                    waveConfig = new WaveConfig
                    {
                        Overview = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                        Basicview = { 1, 6, 10 }
                    },
                    stOperationSetting = stOpSetting
                };

                return result;
            }
            else if (leadCount == 3)
            {
                var result = new LeadInfo
                {
                    data = new LeadData
                    {
                        I = new string[0], // CH1
                        II = new string[0], // CH2
                        III = new string[0] // CH3
                    },
                    props = new[] { "CH1", "CH2", "CH3" },
                    channelSetting = new ChannelSetting
                    {
                        mainChannel = 0,
                        secondChannel1 = 1,
                        secondChannel2 = 2
                    },
                    leadConfig = new LeadConfig
                    {
                        ChestOrg = { "V1", "V2", "V3", "V4", "V5", "V6" },
                        ChestLatest = { "V1", "V2", "V3", "V4", "V5", "V6" },
                        LfLatest = { "R", "L", "F" },
                        LfOrg = { "R", "L", "F" }
                    },
                    waveConfig = new WaveConfig
                    {
                        Overview = { 0, 1, 2 },
                        Basicview = { 0, 1, 2 }
                    },
                    stOperationSetting = stOpSetting
                };

                return result;
            }
            else
            {
                string errMsg = CommonValues.Langs["参数异常，名称：{0}，参数值：{1}"];
                errMsg = string.Format(errMsg, "lead", leadCount);
                throw new Exception(errMsg);
            }
        }

        public static HolterConfig GetHolterInfo(examCase caseInfo)
        {
            HolterConfig holterConfig = null;
            string holerInfoPath = GetHolterInfoPath(caseInfo);
            if (File.Exists(holerInfoPath))
            {
                holterConfig = HolterConfig.LoadFromFile(holerInfoPath);
            }
            return holterConfig;
        }

        public static string GetHolterInfoPath(examCase caseInfo)
        {
            string rootPath = caseInfo.filePath;
            return Path.Combine(rootPath, "holter.info");
        }

        public static string GetLeadName(int leadIndex)
        {
            LeadInfo defaultLeadInfo = GetDefaultLeadInfo(12);
            string[] leads = defaultLeadInfo.props;
            if (leadIndex < leads.Length)
                return leads[leadIndex];
            else
                return "";
        }

        /// <summary>
        /// IE70/IE75血压设备参数设置
        /// </summary>
        public static void SetBpMonitorParameters(BpSettingScheme settings, out string errMsg)
        {
            errMsg = "";
            try
            {
                ABPMxmlManager _abpmManager = new ABPMxmlManager(false, true);

                string[] arrTimeSpan = new string[] {
                    string.Join(",", 0, 0, settings.start1, settings.end1, settings.interval1),
                    string.Join(",", 1, 1, settings.start2, settings.end2, settings.interval2),
                    string.Join(",", 2, 2, settings.start3, settings.end3, settings.interval3),
                    string.Join(",", 3, 3, settings.start4, settings.end4, settings.interval4),
                    string.Join(",", 4, 4, settings.start5, settings.end5, settings.interval5)
                };
                string frontPanelData = "1,1"; //键盘按键, 屏幕显示
                string alarmData = string.Concat(settings.dayAlarm, ",", settings.nightAlarm);

                string strPressureThreshold = string.Join(",", "280", settings.systolicMax, settings.systolicMin, settings.diastolicMax, settings.diastolicMin);
                string strDeviceTime = (settings.syncTime == 1 ? DateTime.Now.ToString(SysConstants.DATETIME_FORMAT) : "");
                if (string.IsNullOrEmpty(strDeviceTime))
                {
                    string curParas = _abpmManager.GetParameters();
                    if (!string.IsNullOrEmpty(curParas))
                    {
                        string[] paras = curParas.Split(',');
                        if (paras.Length > 0) { strDeviceTime = paras[0]; }
                    }

                    if (string.IsNullOrEmpty(strDeviceTime)) //读取不到当前血压配置
                    {
                        errMsg = "血压设备参数设置失败";
                        return;
                    }
                }

                PatientInformation patientInfo = new PatientInformation() //患者信息
                {
                    PatientID = "100001",
                    Name = "default",
                    Sex = "F",
                    Age = 18,
                    AgeUnit = "Y",
                    ExamItem = 16,
                    examdeptCode = "001",
                    ResultStatus = 2
                };
                int opResult = _abpmManager.SetParameters(
                    arrTimeSpan,
                    strDeviceTime,
                    frontPanelData,
                    alarmData,
                    settings.pressureUnit.ToString(),
                    strPressureThreshold,
                    false,
                    patientInfo);
                if (0 == opResult)
                {
                }
                else if (2 == opResult)
                {
                    errMsg = "未检测到设备或设备通讯失败";
                }
                else
                {
                    errMsg = "血压设备参数设置失败";
                }
            }
            catch (Exception ex)
            {
                errMsg = "血压设备参数设置失败";
            }
        }

        /// <summary>
        /// 修改二合一设备的实际用途(测量心电/血压)
        /// </summary>
        /// <param name="wordMode"></param>
        /// <param name="drivePath"></param>
        public static void ModifyDeviceUsage(string wordMode, string drivePath)
        {
            string iniPath = Path.Combine(drivePath, "device.ini");
            Dictionary<string, string> deviceINI = INIFile.ReadIni(iniPath);
            deviceINI["wordmode"] = wordMode;

            INIFile.WriteIni(iniPath, deviceINI);
        }

        /// <summary>
        /// 写入二合一设备心电参数
        /// </summary>
        /// <param name="caseInfo"></param>
        /// <param name="drivePath"></param>
        /// <param name="compressType"></param>
        public static void WriteHolterSettingFile(string drivePath, examCase caseInfo, string compressType)
        {
            string filePath = Path.Combine(drivePath, "holter.ini");
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Empty);
            }

            // 读取现有的 INI 文件内容
            Dictionary<string, string> holterINI = INIFile.ReadIni(filePath);

            // 更新 INI 内容
            holterINI["id"] = caseInfo.caseNo ?? string.Empty;
            holterINI["name"] = !string.IsNullOrEmpty(caseInfo.name) ? AESEncrypt.Encrypt(caseInfo.name) : string.Empty;
            holterINI["sex"] = caseInfo.sex ?? string.Empty;
            holterINI["age"] = caseInfo.age.HasValue ? AESEncrypt.Encrypt(caseInfo.age.Value.ToString()) : string.Empty;
            holterINI["ageUnit"] = caseInfo.ageUnit ?? string.Empty;
            holterINI["birthday"] = !string.IsNullOrEmpty(caseInfo.birthday) ? AESEncrypt.Encrypt(caseInfo.birthday) : string.Empty;
            holterINI["height"] = !string.IsNullOrEmpty(caseInfo.height) ? AESEncrypt.Encrypt(caseInfo.height) : string.Empty;
            holterINI["weight"] = !string.IsNullOrEmpty(caseInfo.weight) ? AESEncrypt.Encrypt(caseInfo.weight) : string.Empty;
            holterINI["dept"] = !string.IsNullOrEmpty(caseInfo.checkDept) ? AESEncrypt.Encrypt(caseInfo.checkDept) : string.Empty;
            holterINI["bed"] = !string.IsNullOrEmpty(caseInfo.bedNo) ? AESEncrypt.Encrypt(caseInfo.bedNo) : string.Empty;

            holterINI["compress"] = compressType;

            if (caseInfo != null)
            {
                holterINI["leadAlarm"] = caseInfo.leadAlarm;
            }

            // 设置数据 ID
            holterINI["dataId"] = caseInfo.id > 0 ? caseInfo.id.ToString() : "";

            // 写入更新后的内容到 INI 文件
            INIFile.WriteIni(filePath, holterINI);
        }

        /// <summary>
        /// 写入二合一设备血压参数
        /// </summary>
        /// <param name="caseInfo"></param>
        /// <param name="drivePath"></param>
        /// <param name="compressType"></param>
        public static void WriteNibpSettingFile(string drivePath, examCase caseInfo, BpSettingScheme settings)
        {
            string filePath = Path.Combine(drivePath, "nibp.ini");
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Empty);
            }

            // 读取现有的 INI 文件内容
            Dictionary<string, string> nibpINI = INIFile.ReadIni(filePath);

            // 更新 INI 内容
            nibpINI["id"] = caseInfo.caseNo ?? string.Empty;
            nibpINI["name"] = !string.IsNullOrEmpty(caseInfo.name) ? AESEncrypt.Encrypt(caseInfo.name) : string.Empty;
            nibpINI["sex"] = caseInfo.sex ?? string.Empty;
            nibpINI["age"] = caseInfo.age.HasValue ? AESEncrypt.Encrypt(caseInfo.age.Value.ToString()) : string.Empty;
            nibpINI["ageUnit"] = caseInfo.ageUnit ?? string.Empty;
            nibpINI["birthday"] = !string.IsNullOrEmpty(caseInfo.birthday) ? AESEncrypt.Encrypt(caseInfo.birthday) : string.Empty;
            nibpINI["height"] = !string.IsNullOrEmpty(caseInfo.height) ? AESEncrypt.Encrypt(caseInfo.height) : string.Empty;
            nibpINI["weight"] = !string.IsNullOrEmpty(caseInfo.weight) ? AESEncrypt.Encrypt(caseInfo.weight) : string.Empty;
            nibpINI["dept"] = !string.IsNullOrEmpty(caseInfo.checkDept) ? AESEncrypt.Encrypt(caseInfo.checkDept) : string.Empty;
            nibpINI["bed"] = !string.IsNullOrEmpty(caseInfo.bedNo) ? AESEncrypt.Encrypt(caseInfo.bedNo) : string.Empty;

            nibpINI["pressure"] = "280";
            nibpINI["select1"] = "0";
            nibpINI["select2"] = "1";
            nibpINI["select3"] = "2";
            nibpINI["select4"] = "3";
            nibpINI["select5"] = "4";
            nibpINI["keyboard"] = "1";
            nibpINI["display"] = "1";

            foreach (PropertyInfo pinfo in typeof(BpSettingScheme).GetProperties())
            {
                string iniKey = pinfo.Name;
                if (pinfo.Name == "deviceNo") { continue; }
                if (pinfo.Name == "pressureUnit")
                {
                    nibpINI["pressureUnit"] = settings.pressureUnit.ToString();
                    nibpINI["BPUnit"] = settings.pressureUnit.ToString();
                }
                else
                {
                    nibpINI[iniKey] = pinfo.GetValue(settings, null).ToString();
                }
            }
            nibpINI["remoteMAC"] = "";

            // 设置数据 ID
            nibpINI["dataId"] = caseInfo.id > 0 ? caseInfo.id.ToString() : "";

            // 写入更新后的内容到 INI 文件
            INIFile.WriteIni(filePath, nibpINI);
        }


        /// <summary>
        /// 为二合一设备设置系统时间或蓝牙设备编号
        /// </summary>
        /// <returns></returns>
        public static int SetDeviceInfo(string dataText)
        {
            if (string.IsNullOrEmpty(dataText)) return -1;

            int result = 0;
            HidCommunicate hid = new HidCommunicate();

            try
            {
                bool isReady = hid.ConnectToDevice();
                if (isReady)
                {
                    byte[] data = Encoding.ASCII.GetBytes(dataText);
                    if (hid.WriteData(data)) result = 1;
                }
            }
            catch
            {
                result = -1;
            }
            finally
            {
                hid.CloseDevice();
            }

            return result;
        }

        /// <summary>
        /// 获取8337设备的型号
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceModel()
        {
            string result = "";
            HidCommunicate hid = new HidCommunicate();

            try
            {
                bool isReady = hid.ConnectToDevice();
                if (isReady)
                {
                    result = hid.GetManufacturerString();
                }
            }
            catch
            {
                result = "";
            }
            finally
            {
                hid.CloseDevice();
            }

            return result;
        }

        /// <summary>
        /// 读取SD卡的序列号(IE95, SN保存于驱动器中倒数第10个扇区)
        /// </summary>
        /// <param name="driveInfo"></param>
        /// <returns></returns>
        public static string GetDriveSN(DriveInfo driveInfo)
        {

            string sn = string.Empty;
            string driveName = driveInfo.Name.TrimEnd('\\');
            SDUtils sdUtil = new SDUtils(driveName);

            if (sdUtil.SectorLength >= 10)
            {
                byte[] bytOutput = sdUtil.ReadSector(sdUtil.SectorLength - 10, false);
                sn = Encoding.UTF8.GetString(bytOutput);
            }
            sdUtil.Close();

            return sn;
        }

        /// <summary>
        /// 显示数据处理(或加载数据)的提示
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="owner"></param>
        /// <param name="work"></param>
        /// <param name="workArg"></param>
        public static void ShowProcessing(string msg, Form owner, Func<object, Task> work, object workArg = null, Rectangle displayRegion = default(Rectangle), bool dataLoading = false)
        {
            LoadingForm processingForm = new LoadingForm(msg) { FormRectangle = displayRegion, ShowGifOnly = dataLoading };
            dynamic expObj = new System.Dynamic.ExpandoObject();
            expObj.Form = processingForm;
            expObj.WorkArg = workArg;
            processingForm.SetWorkAction(work, expObj);
            if (owner != null)
            {
                //processingForm.ShowDialog(owner);

                // 显示对话框（非模态）
                //foreach (Control control in owner.Controls) { control.Enabled = false; }
                Zoncare.Component.ModalDialogWithEnabledMinMax.Show(owner, processingForm);
                while (!processingForm.IsDisposed)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }
                //foreach (Control control in owner.Controls) { control.Enabled = true; }
            }
            else
            {
                processingForm.ShowDialog();
            }

            if (processingForm.WorkException != null)
            {
                throw processingForm.WorkException;
            }
        }

        /// <summary>
        /// 显示进度条
        /// </summary>
        public static void ShowProgress(string msg, Form owner, Func<object, Task> work, object workArg = null,
            Rectangle displayRegion = default(Rectangle), bool useMarqueeStyleBar = false)
        {
            ProgressForm progressForm = new ProgressForm(msg)
            {
                FormRectangle = displayRegion,
                UseMarqueeStyle = useMarqueeStyleBar
            };
            dynamic expObj = new System.Dynamic.ExpandoObject();
            expObj.Form = progressForm;
            expObj.WorkArg = workArg;
            progressForm.SetWorkAction(work, expObj);
            if (owner != null)
            {
                //processingForm.ShowDialog(owner);

                // 显示对话框（非模态）
                //foreach (Control control in owner.Controls) { control.Enabled = false; }
                Zoncare.Component.ModalDialogWithEnabledMinMax.Show(owner, progressForm);
                while (!progressForm.IsDisposed)
                {
                    Thread.Sleep(1);
                    Application.DoEvents();
                }
                //foreach (Control control in owner.Controls) { control.Enabled = true; }
            }
            else
            {
                progressForm.ShowDialog();
            }

            if (progressForm.WorkException != null)
            {
                throw progressForm.WorkException;
            }
        }

    }
}
