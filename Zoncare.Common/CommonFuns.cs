using Force.Crc32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Utilities;

namespace Zoncare.Common
{
    public static class CommonFuns
    {
        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool GetVolumeInformation(
            string lpRootPathName,
            StringBuilder lpVolumeNameBuffer,
            uint nVolumeNameSize,
            out uint lpVolumeSerialNumber,
            out uint lpMaximumComponentLength,
            out uint lpFileSystemFlags,
            StringBuilder lpFileSystemNameBuffer,
            uint nFileSystemNameSize);

        private static object _lockObj = new object();

        /// <summary>
        /// 弹出提示对话框
        /// </summary>
        /// <param name="msgText">提示信息的内容</param>
        /// <param name="msgType">信息提示类型(0:正常提示信息; 1:询问)</param>
        /// <param name="questionBtns">提示对话框上显示的按钮类型</param>
        /// <param name="values">与提示文字中格式化占位符对应的替换值</param>
        /// <returns></returns>
        public static DialogResult PromptMessage(string msgText, int msgType = 0, MessageBoxButtons questionBtns = MessageBoxButtons.OKCancel, params object[] values)
        {
            DialogResult dlgResult = DialogResult.None;
            msgText = CommonValues.Langs[msgText];
            if (values != null) msgText = string.Format(msgText, values);

            if (msgType == 0)
                dlgResult = MessageBox.Show(msgText, CommonValues.SystemName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (msgType == 1)
                dlgResult = MessageBox.Show(msgText, CommonValues.SystemName, questionBtns, MessageBoxIcon.Question);

            return dlgResult;
        }

        /// <summary>
        /// 弹出错误提示
        /// </summary>
        /// <param name="msgText">提示信息的内容</param>
        /// <param name="caption">标题文字</param>
        /// <param name="isWarning">是否是警告信息</param>
        public static void PromptError(string msgText, string caption = "", bool isWarning = false)
        {
            //提示、错误提示、数据处理出错
            string captionText = "";
            msgText = CommonValues.Langs[msgText];
            if (string.IsNullOrEmpty(caption))
            {
                captionText = CommonValues.SystemName;
            }
            else
            {
                captionText = CommonValues.Langs[caption];
            }

            MessageBox.Show(msgText, captionText, MessageBoxButtons.OK, (isWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Error));
        }

        /// <summary>
        /// 弹出错误提示
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="msgText"></param>
        /// <param name="caption"></param>
        /// <param name="isWarning"></param>
        public static void PromptError(IWin32Window owner, string msgText, string caption = "", bool isWarning = false)
        {
            if (owner == null)
            {
                PromptError(msgText, caption, isWarning);
                return;
            }

            //提示、错误提示、数据处理出错
            string captionText = "";
            msgText = CommonValues.Langs[msgText];
            if (string.IsNullOrEmpty(caption))
            {
                captionText = CommonValues.SystemName;
            }
            else
            {
                captionText = CommonValues.Langs[caption];
            }

            MessageBox.Show(owner, msgText, captionText, MessageBoxButtons.OK, (isWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Error));
        }

        /// <summary>
        /// 获取系统名称及LOGO文件路径
        /// </summary>
        /// <returns></returns>
        public static (string systemName, string logoPath) GetSystemName()
        {
            string sysName, logoPath;
            if (CommonValues.SystemType == "ecg")
            {
                sysName = CommonValues.Langs["动态心电图系统应用软件"];
                logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\ecgLogin.png");
            }
            else if (CommonValues.SystemType == "bp")
            {
                sysName = CommonValues.Langs["血压管理软件"];
                logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\bpLogin.png");
            }
            else
            {
                sysName = CommonValues.Langs["动态心电血压记录仪应用软件"];
                logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\ecgBpLogin.png");
            }
            return (sysName, logoPath);
        }

        public static void OpenPort(SerialPort com)
        {
            if (com != null)
            {
                if (!com.IsOpen)
                {
                    com.Open();
                }
            }
        }

        public static void ClosePort(SerialPort com)
        {
            if (com != null && com.IsOpen)
            {
                com.Close();
                com.Dispose();
            }
        }

        /// <summary>
        /// 向所有串口发送指令
        /// </summary>
        /// <returns></returns>
        public static int SendToSerialPorts(string cmd)
        {
            int res = 0;
            byte[] sendData = Encoding.UTF8.GetBytes(cmd); //响应 OK
            string[] commPorts = SerialPort.GetPortNames();
            foreach (string portName in commPorts)
            {
                try
                {
                    SerialPort com = new SerialPort(portName, 115200);
                    OpenPort(com);
                    com.Write(sendData, 0, sendData.Length);
                    ClosePort(com);
                }
                catch { }
            }

            return res;
        }

        /// <summary>
        /// 检查文件是否存在并可访问
        /// </summary>
        /// <param name="fileFullName">文件全路径</param>
        /// <param name="duration">检查过程的最大时长, 单位:毫秒</param>
        /// <returns></returns>
        public static async Task<bool> IsFileAvailable(string fileFullName, int duration)
        {
            Stopwatch watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds <= duration)
            {
                if (File.Exists(fileFullName))
                {
                    try
                    {
                        using (FileStream stream = File.Open(fileFullName, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            return true; // 文件可访问
                        }
                    }
                    catch { } // 文件被锁定或其他异常
                }

                await Task.Delay(10).ConfigureAwait(false);
            }

            return false;
        }

        public static (string drive, string driveErr, string sn) CheckDrive(string volumnName)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            string drive = "", driveErr = "", sn = "";

            var findResult = allDrives.Where(o => o.IsReady && o.VolumeLabel.ToUpper() == volumnName.ToUpper());
            int cnt = findResult.Count();
            if (cnt > 1)
            {
                drive = "";
                driveErr = "检测到多个记录卡设备";
                sn = "";
            }
            else
            {
                DriveInfo ret = findResult.FirstOrDefault();
                if (ret != null)
                {
                    string driveRootName = ret?.RootDirectory.FullName;
                    string driveLetter = driveRootName.TrimEnd('\\');
                    //查询磁盘或SD卡序列号
                    string diskSN = string.Empty;

                    try
                    {
                        StringBuilder fileSystemName = new StringBuilder(256);
                        StringBuilder volumeName = new StringBuilder(256);
                        bool result = GetVolumeInformation(
                            driveRootName,
                            volumeName,
                            (uint)volumeName.Capacity,
                            out uint serialNumber,
                            out uint maxComponentLength,
                            out uint fileSystemFlags,
                            fileSystemName,
                            (uint)fileSystemName.Capacity);
                        if (result == true) { diskSN = $"{serialNumber >> 16:X4}-{serialNumber & 0xFFFF:X4}"; }

                        drive = driveRootName ?? "";
                        driveErr = "";
                        sn = diskSN;
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex, "基础模块");
                    }
                }
            }
            return (drive, driveErr, sn);
        }

        /// <summary>
        /// 根据磁盘根目录名获取磁盘信息
        /// </summary>
        /// <param name="rootName">磁盘根目录名</param>
        /// <returns></returns>
        public static object GetDriveInfo(string rootName)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            if (!string.IsNullOrEmpty(rootName))
            {
                DriveInfo drInfo = allDrives.FirstOrDefault(o => o.IsReady && o.Name.Contains(rootName.ToUpper()));
                dynamic driveInfo = new ExpandoObject();

                driveInfo.mounted = drInfo.RootDirectory.FullName;
                driveInfo.total = (drInfo.TotalSize / 1024d / 1024 / 1024);
                driveInfo.used = ((drInfo.TotalSize - drInfo.AvailableFreeSpace) / 1024d / 1024 / 1024);
                driveInfo.available = (drInfo.AvailableFreeSpace / 1024d / 1024 / 1024);
                driveInfo.capacity = ((double)(drInfo.TotalSize - drInfo.AvailableFreeSpace) / drInfo.TotalSize * 100);

                return driveInfo;
            }

            return null;
        }

        /// <summary>
        /// 获取采集设备中的数据文件信息
        /// </summary>
        /// <param name="dirPath">目录路径</param>
        /// <param name="prefixes">文件名前缀</param>
        /// <returns></returns>
        public static List<FileInfo> GetDataFiles(string dirPath, params string[] prefixes)
        {
            List<FileInfo> files = new List<FileInfo>();
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            foreach (FileInfo fi in dirInfo.GetFiles("*.dat"))
            {
                if (prefixes.Any(s => fi.Name.StartsWith(s)))
                {
                    files.Add(fi);
                }
            }
            return files;
        }

        /// <summary>
        /// 从数据文件的文件名中获取时间
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DateTime GetFileNameDate(string fileName)
        {
            int spliterIndex = fileName.IndexOf("-");
            string dateString = fileName.Substring(spliterIndex + 1, fileName.LastIndexOf(".") - (spliterIndex + 1));

            return DateTime.ParseExact(dateString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 获取时分秒格式的时长
        /// </summary>
        /// <param name="dataDuration"></param>
        /// <returns></returns>
        public static string GetTimeDurationText(string dataDuration)
        {
            string timeString = "";
            bool parsed = double.TryParse(dataDuration, out double val);
            if (parsed)
            {
                TimeSpan time = TimeSpan.FromSeconds(val);
                timeString = $"{(int)time.TotalHours}{CommonValues.Langs["小时"]}{time.Minutes}{CommonValues.Langs["分钟"]}{time.Seconds}{CommonValues.Langs["秒"]}";
            }
            return timeString;
        }

        /// <summary>
        /// 验证并获取格式化时间
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns></returns>
        public static string ValidateAndFormatDateTime(string dateTimeString)
        {
            // 解析输入字符串
            var parts = dateTimeString.Split(' ');

            if (parts.Length != 2)
            {
                return "1970-01-01 00:00:00";
            }

            var dateParts = Array.ConvertAll(parts[0].Split('-'), int.Parse);
            var timeParts = Array.ConvertAll(parts[1].Split(':'), int.Parse);

            // 解构出年、月、日、时、分、秒
            int year = dateParts[0];
            int month = dateParts[1];
            int day = dateParts[2];
            int hour = timeParts[0];
            int minute = timeParts[1];
            int second = timeParts[2];

            var currentDate = DateTime.Now;

            // 如果年不对，取当前年
            year = (year >= 1970 && year <= 9999) ? year : currentDate.Year;

            // 验证月和日，并补全
            month = (month >= 1 && month <= 12) ? month : 1; // 默认1月
            day = (day >= 1 && day <= DateTime.DaysInMonth(year, month)) ? day : 1; // 默认1日

            // 时分秒处理
            hour = (hour >= 0 && hour <= 23) ? hour : 0;
            minute = (minute >= 0 && minute <= 59) ? minute : 0;
            second = (second >= 0 && second <= 59) ? second : 0;

            // 返回格式化的字符串
            return new DateTime(year, month, day, hour, minute, second).ToString(SysConstants.DATETIME_FORMAT);
        }

        public static (int age, string ageUnit) ResolveAgeFromBirthDay(DateTime inputDate)
        {
            DateTime now = DateTime.Now;
            int age = 0;
            string ageUnit = "Y";

            TimeSpan dateDiff = now.Subtract(inputDate);
            int days = (int)Math.Floor(dateDiff.TotalMilliseconds / (1000 * 60 * 60 * 24));
            if (days <= 99)
            {
                age = days;
                ageUnit = "D";
            }
            else
            {
                int years = now.Year - inputDate.Year;
                int months = years * 12 + now.Month - inputDate.Month;

                if (now.Day < inputDate.Day)
                {
                    months -= 1;
                }

                if (months <= 11)
                {
                    age = months;
                    ageUnit = "M";
                }
                else
                {
                    if (now.Month < inputDate.Month ||
                        (now.Month == inputDate.Month && now.Day < inputDate.Day))
                    {
                        years--;
                    }

                    age = years;
                    ageUnit = "Y";
                }
            }
            return (age, ageUnit);
        }

        public static string ResolveGender(string genderStr)
        {
            foreach (string key in SysConstants.Genders.Keys)
            {
                if (key == genderStr)
                    return key;
                else if (SysConstants.Genders[key] == genderStr)
                    return key;
            }
            return "U";
        }

        public static string ResolveAgeUnit(string ageUnitStr)
        {
            if (CommonValues.LanguageID != 1049)
            {
                foreach (string key in SysConstants.AgeUnits.Keys)
                {
                    if (key == ageUnitStr)
                        return key;
                    else if (SysConstants.AgeUnits[key] == ageUnitStr)
                        return key;
                }
            }
            else
            {
                string[] yearVals = new string[] { SysConstants.RusAgeUnits0["Y"], SysConstants.RusAgeUnits1["Y"], SysConstants.RusAgeUnits2["Y"] };
                string[] monthVals = new string[] { SysConstants.RusAgeUnits0["M"], SysConstants.RusAgeUnits1["M"], SysConstants.RusAgeUnits2["M"] };
                string[] dayVals = new string[] { SysConstants.RusAgeUnits0["D"], SysConstants.RusAgeUnits1["D"], SysConstants.RusAgeUnits2["D"] };
                if (yearVals.Contains(ageUnitStr)) return "Y";
                if (monthVals.Contains(ageUnitStr)) return "M";
                if (dayVals.Contains(ageUnitStr)) return "D";
            }
            return "U";
        }

        public static int GetAgeDays(int ageValue, string ageUnit)
        {
            if (ageUnit == "Y")
                return ageValue * 366;
            else if (ageUnit == "M")
                return ageValue * 30;
            else
                return ageValue;
        }

        public static string GetAgeText(int? ageValue, string ageUnit)
        {
            if (ageValue == null) return "";

            if (ageUnit == "Y" || ageUnit == "M" || ageUnit == "D")
                return ageValue.Value + SysConstants.AgeUnits[ageUnit];
            else
                return ageValue.Value + SysConstants.AgeUnits["Y"];
        }

        public static string GetRace(string raceStr)
        {
            foreach (string key in SysConstants.Races.Keys)
            {
                if (key == raceStr)
                    return key;
                else if (SysConstants.Races[key] == raceStr)
                    return key;
            }
            return "U";
        }

        /// <summary>
        /// 基于文件内容计算CRC
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string CalculateFileCrc32(byte[] content)
        {
            Crc32Algorithm crc32 = new Crc32Algorithm();
            byte[] hash = crc32.ComputeHash(content);
            return BitConverter.ToString(hash).Replace("-", "").ToLower(); // 将字节数组转换为十六进制字符串
        }

        //根据年龄及其单位获取不同语言的年龄单位文本表示(俄语特殊处理)
        public static string GetAgeUnits(string ageUnit, string ageStr)
        {
            if (CommonValues.LanguageID != 1049)
            {
                return SysConstants.AgeUnits[ageUnit];
            }
            else
            {
                int age = 0;
                int.TryParse(ageStr, out age);
                if (age % 10 == 1 && SysConstants.RusAgeUnits1.ContainsKey(ageUnit))
                {
                    return SysConstants.RusAgeUnits1[ageUnit];
                }
                else if (age % 10 == 2 && SysConstants.RusAgeUnits2.ContainsKey(ageUnit))
                {
                    return SysConstants.RusAgeUnits2[ageUnit];
                }
                else if (SysConstants.RusAgeUnits0.ContainsKey(ageUnit))
                {
                    return SysConstants.RusAgeUnits0[ageUnit];
                }
                else
                {
                    return SysConstants.RusAgeUnits0["U"];
                }
            }
        }

        /// <summary>
        /// 获取对应于当前系统血压单位的血压(含血压单位)
        /// </summary>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        public static string GetBPText(string inputValue, string currentBpUnit, int decimals = 1)
        {
            if (string.IsNullOrWhiteSpace(inputValue)) return inputValue;

            string strReturn = string.Empty;
            if (currentBpUnit == "mmHg")
            {
                Regex regexBP = new Regex(@"((\d+[.]\d+\/\d+[.]\d+)|(\d+[.]\d+))?kPa"); //e.g. "12.98kPa/10.05kPa" 或 "12.98kPa"
                MatchEvaluator evaluator = new MatchEvaluator((m) =>
                {
                    //由于保存的数据均以mmHg为单位, 正常情况下不会出现此情形
                    string valueStr = m.Value.Replace("kPa", "");
                    if (!string.IsNullOrWhiteSpace(valueStr))
                    {
                        string[] values = valueStr.Split('/');
                        double.TryParse(values[0], out double value1);
                        valueStr = CommonFuns.kPa2mmHg(value1).ToString();
                        if (values.Length > 1)
                        {
                            double.TryParse(values[1], out double value2);
                            valueStr += "/" + CommonFuns.kPa2mmHg(value2).ToString();
                        }
                    }
                    return valueStr + "mmHg";
                });

                strReturn = regexBP.Replace(inputValue, evaluator);
                if (CommonValues.Langs.ContainsKey("mmHg")) strReturn = strReturn.Replace("mmHg", CommonValues.Langs["mmHg"]);
            }
            else if (currentBpUnit == "kPa")
            {
                Regex regexBP = new Regex(@"((\d+\/\d+)|(\d+))?mmHg"); //e.g. "120mmHg/80mmHg" 或 "120mmHg"
                MatchEvaluator evaluator = new MatchEvaluator((m) =>
                {
                    string valueStr = m.Value.Replace("mmHg", "");
                    if (!string.IsNullOrWhiteSpace(valueStr))
                    {
                        string[] values = valueStr.Split('/');
                        int.TryParse(values[0], out int value1);
                        valueStr = mmHg2KPa(value1, decimals).ToString("0." + new string('0', decimals));
                        if (values.Length > 1)
                        {
                            int.TryParse(values[1], out int value2);
                            valueStr = valueStr + "/" + mmHg2KPa(value2, decimals).ToString("0." + new string('0', decimals));
                        }
                    }
                    return valueStr + "kPa";
                });

                strReturn = regexBP.Replace(inputValue, evaluator);
                if (CommonValues.Langs.ContainsKey("kPa")) strReturn = strReturn.Replace("kPa", CommonValues.Langs["kPa"]);
            }
            else
            {
                strReturn = inputValue;
            }

            return strReturn;
        }

        /// <summary>
        /// 将血压数值从单位mmHg转换为kPa
        /// </summary>
        /// <param name="mmHgValue"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static double mmHg2KPa(int mmHgValue, int decimals = 1)
        {
            return Math.Round(mmHgValue * 0.133322d, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 将血压数值从单位kPa转换为mmHg
        /// </summary>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        public static int kPa2mmHg(double kPaValue)
        {
            double mmHgValue = Math.Round(kPaValue / 0.133322d, MidpointRounding.AwayFromZero);
            int bpValue = Convert.ToInt32(mmHgValue);

            return bpValue;
        }

        #region 控件相关
        /// <summary>
        /// 从容器中找出指定类型的子控件(含子控件的子控件)
        /// </summary>
        /// <typeparam name="T">要查找的子控件类型</typeparam>
        /// <param name="parent">父容器</param>
        /// <param name="reverted">是否倒序排列</param>
        /// <param name="includeSelf">是否包括自身</param>
        /// <returns></returns>
        public static List<T> RetriveSubControls<T>(Control parent, bool reverted = false, bool includeSelf = false) where T : Control
        {
            List<T> list = new List<T>();
            if (includeSelf)
            {
                list.Add(parent as T);
            }
            Stack<Control> stack = new Stack<Control>();
            stack.Push(parent);
            while (stack.Count > 0)
            {
                Control control = stack.Pop();
                foreach (object obj in control.Controls)
                {
                    Control item = (Control)obj;
                    stack.Push(item);
                }
                if (!control.Equals(parent) && control is T)
                {
                    if (!reverted)
                    {
                        list.Insert(0, control as T); //栈内元素后进先出
                    }
                    else
                    {
                        list.Add(control as T);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取控件的全路径形式名称
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="spliter"></param>
        /// <returns></returns>
        public static string GetControlFullPath(Control ctrl, string spliter = ".")
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (ctrl != null)
            {
                stringBuilder.Insert(0, spliter + ctrl.Name);
                ctrl = ctrl.Parent;
            }
            return stringBuilder.ToString().TrimStart(new char[]
            {
                '.'
            });
        }

        /// <summary>
        /// 获取指定类型的父级控件
        /// </summary>
        /// <param name="ctl">当前控件</param>
        /// <param name="parentType">父控件类型</param>
        /// <returns></returns>
        public static Control GetSpecifiedParent(Control ctl, Type parentType)
        {
            if (ctl.Parent == null) return null;

            if (parentType.IsInstanceOfType(ctl.Parent))
                return ctl.Parent;
            else
                return GetSpecifiedParent(ctl.Parent, parentType);
        }

        /// <summary>
        /// 初始化指定词典类型相关的下拉框数据
        /// </summary>
        /// <param name="cbo">下拉框</param>
        /// <param name="dict">数据源词典</param>
        /// <param name="includeAll">是否应用于筛选条件以包含所有选项</param>
        public static void InitCommboBoxItems<Tkey>(ComboBox cbo, Dictionary<Tkey, string> dict, bool includeAll = false)
        {
            List<ListItem> lst = new List<ListItem>();
            if (includeAll) lst.Add(new ListItem(-1, CommonValues.Langs["请选择"]));
            foreach (KeyValuePair<Tkey, string> item in dict)
            {
                lst.Add(new ListItem(item.Key, dict[item.Key]));
            }

            cbo.ValueMember = "Value";
            cbo.DisplayMember = "Text";
            cbo.DataSource = lst;

            if (includeAll) cbo.SelectedIndex = 0;
        }

        /// <summary>
        /// 初始化指定词典类型相关的下拉框数据
        /// </summary>
        /// <typeparam name="Tkey">数据源的列表项类型</typeparam>
        /// <param name="cbo">下拉框</param>
        /// <param name="dict">数据源</param>
        /// <param name="includeAll">是否包含"全部"项</param>
        /// <param name="firstValue">"全部"项的内容</param>
        public static void InitCommboBoxItems<Tkey>(ComboBox cbo, PersistDictionary<Tkey> dict, bool includeAll = false, object firstValue = null, string firstText = "请选择", object defaultValue = null)
        {
            List<ListItem> lst = new List<ListItem>();
            if (includeAll) lst.Add(new ListItem(firstValue, CommonValues.Langs[firstText]));
            foreach (KeyValuePair<Tkey, string> item in dict)
            {
                lst.Add(new ListItem(item.Key, dict[item.Key]));
            }

            cbo.ValueMember = "Value";
            cbo.DisplayMember = "Text";
            cbo.DataSource = lst;

            cbo.SelectedIndex = -1;
            if (defaultValue != null)
            {
                cbo.SelectedValue = defaultValue;
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 初始化列表显示样式
        /// </summary>
        public static void InitDataGridStyles(DataGridView dgv, bool highlightAlternatingRows = false)
        {
            // 设置表格基本样式
            dgv.AutoGenerateColumns = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            // 设置表头样式 - 缩小字体和高度
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            //dgv.ColumnHeadersHeight = 28;

            // 设置行样式 - 缩小行高和字体
            dgv.DefaultCellStyle.Font = new Font("微软雅黑", 9F);
            //dgv.RowTemplate.Height = 28;
            if (highlightAlternatingRows) dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;
        }

        /// <summary>
        /// 获取下拉选项显示的最大宽度
        /// </summary>
        /// <param name="cbx">组合框</param>
        /// <param name="maxWidth">组合框中的下拉部分的最大宽度</param>
        /// <returns></returns>
        public static int GetMaxWidthOfComboxItems(ComboBox cbx, int maxWidth = 0)
        {
            int maxTextLen = cbx.Width;

            if (cbx.Items.Count > 0)
            {
                using (Graphics g = cbx.CreateGraphics())
                {
                    g.PageUnit = GraphicsUnit.Pixel;
                    for (int i = 0; i < cbx.Items.Count; i++)
                    {
                        object item = cbx.Items[i];
                        string itemText = cbx.GetItemText(item);

                        SizeF itemTextSize = g.MeasureString(itemText, cbx.Font);
                        if (maxTextLen < itemTextSize.Width) maxTextLen = Convert.ToInt32(itemTextSize.Width);
                    }
                }
            }

            if (maxWidth > 0) maxTextLen = Math.Min(maxTextLen, maxWidth);
            return maxTextLen;
        }

        /// <summary>
        /// 按需调用控件的Invoke方法
        /// </summary>
        /// <param name="ctl"></param>
        /// <param name="action"></param>
        public static void InvokeIfRequired(this Control ctl, MethodInvoker action, bool asyncInvoke = false)
        {
            try
            {
                if (ctl != null && ctl.IsHandleCreated)
                {
                    if (ctl.InvokeRequired)
                    {
                        if (asyncInvoke)
                            ctl.BeginInvoke(action);
                        else
                            ctl.Invoke(action);
                    }
                    else
                    {
                        action();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "UI线程调用");
            }
        }
        #endregion

        #region 进程相关
        /// <summary>
        /// 获取进程相关端口的占用情况
        /// </summary>
        /// <returns>字典（pid，端口号）</returns>
        public static Dictionary<int, List<int>> GetProcessPorts()
        {
            Dictionary<int, List<int>> portinfo = new Dictionary<int, List<int>>();

            Process pro = new Process();
            try
            {
                pro.StartInfo.FileName = "netstat.exe";
                pro.StartInfo.Arguments = "-ano";
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.CreateNoWindow = true;
                pro.Start();

                string output = pro.StandardOutput.ReadToEnd();
                pro.WaitForExit();

                StringBuilder sb = new StringBuilder();
                Regex reg = new Regex("\\s+", RegexOptions.Compiled);

                byte[] txt = Encoding.Default.GetBytes(output);
                using (Stream readStream = new MemoryStream(txt))
                {
                    readStream.Position = 0;
                    using (StreamReader reader = new StreamReader(readStream))
                    {
                        string line = null;
                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.Trim();
                            sb.Append("\r\n" + line);

                            if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                            {
                                line = reg.Replace(line, ",");
                                string[] arr = line.Split(',');
                                int pos = arr[1].LastIndexOf(':');
                                int pot = int.Parse(arr[1].Substring(pos + 1));
                                int pid = int.Parse(arr[4]);
                                if (portinfo.ContainsKey(pid))
                                {
                                    if (!portinfo[pid].Contains(pot)) { portinfo[pid].Add(pot); }
                                }
                                else
                                {
                                    List<int> ls = new List<int>();
                                    ls.Add(pot);
                                    portinfo.Add(pid, ls);
                                }
                            }
                        }
                    }
                }

                //Logger.CreateLog("所有占用端口的进程ID: " + sb.ToString(), "程序退出");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "程序退出");
            }
            finally
            {
                pro.Close();
            }

            return portinfo;
        }

        /// <summary>
        /// 退出指定名称的进程
        /// </summary>
        /// <param name="processName"></param>
        public static void ExitProcessByCmd(string processName)
        {
            Process process = new Process();

            try
            {
                // 设置 ProcessStartInfo 对象，指定要执行的系统命令和参数
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "taskkill";
                startInfo.Arguments = "/im " + processName + " /t /f"; // 使用"/t"参数向指定的进程及其子进程发送信号, 使用"/f"参数来强制终止进程
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.Verb = "runas";

                // 将 ProcessStartInfo 对象赋值给 Process 对象的 StartInfo 属性
                process.StartInfo = startInfo;

                // 启动并等待命令执行完成
                process.Start();
                //process.WaitForExit();

                Console.WriteLine("成功发送 SIGTERM 信号给进程：" + processName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送 SIGTERM 信号给进程出错：" + ex.Message);
            }
            finally
            {
                // 最后关闭 Process 对象
                process.Close();
            }
        }

        /// <summary>
        /// 退出指定名称的进程
        /// </summary>
        /// <param name="processName"></param>
        public static void ExitProcess(string processName)
        {
            try
            {
                // 查找所有与指定名称匹配的进程
                Process[] processes = Process.GetProcessesByName(processName);

                if (processes.Length > 0)
                {
                    foreach (Process process in processes)
                    {
                        // 安全地退出进程
                        process.Kill();
                        process.WaitForExit(); // 等待进程退出
                        Console.WriteLine($"已退出进程: {process.ProcessName} (ID: {process.Id})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 按指定进程名称找出其占用某一特定端口的进程并退出
        /// </summary>
        public static void ExitProcess(string processName, int targetPort)
        {
            Dictionary<int, List<int>> relevantPorts = CommonFuns.GetProcessPorts();

            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                try
                {
                    // 发送关闭信号给进程
                    //process.CloseMainWindow();

                    if (relevantPorts.ContainsKey(process.Id))
                    {
                        if (relevantPorts[process.Id].Any(port => port == targetPort))
                        {
                            //process.Kill();
                            killProcessRecursively(process.Id);
                        }
                    }

                    Console.WriteLine($"已发送关闭信号给进程：{ process.ProcessName }（PID:{process.Id}）");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("关闭进程出错：" + ex.Message);
                }
            }

            //Console.ReadLine();
        }

        private static void killProcessRecursively(int pid)
        {
            Process process = new Process();

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "taskkill";
                startInfo.Arguments = "/PID " + pid + " /t /f"; // 使用"/t"参数向指定的进程及其子进程发送信号, 使用"/f"参数来强制终止进程
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.Verb = "runas";

                process.StartInfo = startInfo;

                process.Start();
                //process.WaitForExit();

                Console.WriteLine("成功发送 SIGTERM 信号给进程(PID)：" + pid);
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送 SIGTERM 信号给进程出错：" + ex.Message);
            }
            finally
            {
                process.Close();
            }
        }

        //用于退出算法分析程序
        public static void StopService(int processId)
        {
            AttachConsole(processId); //共享主程序的控制台窗口
            GenerateConsoleCtrlEvent(0, 0); //向指定的控制台进程发送控制信号: 发送ctrl+c（注意：这是向所有共享该console的进程发送）
        }
        #endregion

        #region 绘图或图像处理
        /// <summary>
        /// 设置(绘图对象)波形的绘制区域及相关信息
        /// </summary>
        public static void SetLeadGraphicInfo(List<GraphicsInfo> graphicsList, Tuple<int, int, int> layout, List<Lead> relatedLeads,
            Rectangle rect, int chanelWidth, float waveOffset = 0, bool setCanvas = false, int paceFlag = 0)
        {
            int rowC = layout.Item1, colC = layout.Item2, plusC = layout.Item3;
            if (colC > 0)
            {
                int oneW = (int)((rect.Width - waveOffset) / colC);
                int oneH = rect.Height / (rowC + plusC);

                int rowIndex = 0, colIndex = 0;
                foreach (Lead leadN in relatedLeads)
                {
                    bool colIsSplit = (rowIndex < rowC); //当前行是否需分列显示
                    GraphicsInfo gInfo = new GraphicsInfo()
                    {
                        TheLead = leadN,
                        LeadIndex = leadN.Index,
                        RowIndex = rowIndex,
                        ColIndex = colIndex
                    };
                    int graphicW = (colIsSplit ? oneW : (int)(rect.Width - waveOffset));
                    gInfo.Rect = new Rectangle((int)(rect.X + waveOffset + colIndex * oneW), rect.Location.Y + rowIndex * oneH, graphicW, oneH);
                    graphicsList.Add(gInfo);

                    if (colIsSplit == true)
                    {
                        if (rowIndex >= rowC - 1) //纵向排满后换至第二列
                        {
                            if (colIndex < colC - 1)
                            {
                                colIndex++;
                                rowIndex = 0;
                            }
                            else
                            {
                                rowIndex++;
                                colIndex = 0;
                            }
                        }
                        else
                        {
                            rowIndex++;
                        }
                    }
                    else
                    {
                        colIndex = 0;
                        rowIndex++;
                    }
                }
            }
            else // "a+b"布局
            {
                int oneW = (int)((rect.Width - waveOffset) / 2);
                for (int i = 0; i < rowC; i++)
                {
                    int oneH = rect.Height / rowC;
                    Lead leadN = relatedLeads[i];
                    GraphicsInfo gInfo = new GraphicsInfo()
                    {
                        TheLead = leadN,
                        LeadIndex = leadN.Index,
                        RowIndex = i,
                        ColIndex = 0,
                        Rect = new Rectangle((int)(rect.X + waveOffset), rect.Location.Y + i * oneH, oneW, oneH)
                    };
                    graphicsList.Add(gInfo);
                }
                for (int j = 0; j < plusC; j++)
                {
                    int oneH = rect.Height / plusC;
                    Lead leadN = relatedLeads[rowC + j];
                    GraphicsInfo gInfo = new GraphicsInfo()
                    {
                        TheLead = leadN,
                        LeadIndex = leadN.Index,
                        RowIndex = j,
                        ColIndex = 1,
                        Rect = new Rectangle((int)(rect.X + waveOffset + oneW), rect.Location.Y + j * oneH, oneW, oneH)
                    };
                    graphicsList.Add(gInfo);
                }
            }

            if (setCanvas)
            {
                foreach (var gInfo in graphicsList)
                {
                    gInfo.Canvas = new EcgCanvas(gInfo.TheLead, gInfo.Rect.Width, gInfo.Rect.Height, Math.Min(chanelWidth, gInfo.Rect.Height), gInfo.Rect.Location);
                    gInfo.Canvas.IsPaceOn = (paceFlag == 1);
                }
            }
        }

        /// <summary>
        /// 释放绘图资源
        /// </summary>
        /// <param name="graphics"></param>
        public static void DeinitGraphicsInfo(List<GraphicsInfo> graphics)
        {
            if (graphics != null && graphics.Count > 0)
            {
                foreach (GraphicsInfo gInfo in graphics)
                {
                    gInfo.Canvas?.Dispose();
                }
            }
            graphics?.Clear();
        }

        /// <summary>
        /// 释放绘图资源
        /// </summary>
        /// <param name="graphics"></param>
        public static void DeinitGraphicsInfo(List<GraphicsInfoAdv> graphics)
        {
            if (graphics != null && graphics.Count > 0)
            {
                foreach (GraphicsInfoAdv gInfo in graphics)
                {
                    gInfo.Canvas?.Dispose();
                }
            }
            graphics.Clear();
        }

        public static void DeinitGraphicsInfo(Dictionary<int, GraphicsInfo> graphics)
        {
            if (graphics != null && graphics.Count > 0)
            {
                foreach (var gInfo in graphics)
                {
                    gInfo.Value?.Canvas?.Dispose();
                }
            }
            graphics?.Clear();
        }

        /// <summary>
        /// 从图像文件获取指定宽度和高度的图像(基于所指定的宽、高较小的值按原始宽高比缩放)
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="keepOriginalSize">是否保持原图大小</param>
        /// <param name="keepOriginalRatio">是否保持原图宽高比</param>
        /// <param name="width">新的图像宽度参考值</param>
        /// <param name="height">新的图像高度参考值</param>
        /// <returns></returns>
        public static Image GetImageFromFile(string filePath, bool keepOriginalSize = false, bool keepOriginalRatio = false, int width = 0, int height = 0)
        {
            Image img = null;
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);

                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        if (bytes.Length > 0)
                        {
                            ms.Position = 0;

                            Image sourceImg = (ms == null ? null : Image.FromStream(ms));
                            int sWidth = sourceImg.Width;
                            int sHeigh = sourceImg.Height;
                            int newWidth = width, newHeight = height;
                            if (keepOriginalSize)
                            {
                                img = sourceImg;
                            }
                            else
                            {
                                if (width == 0 && height == 0)
                                {
                                    img = new Bitmap(sourceImg, (int)(150f * sWidth / sHeigh), 150);
                                }
                                else
                                {
                                    if (keepOriginalRatio)
                                    {
                                        if ((double)(sWidth / sHeigh) >= (double)(width / height))
                                        {
                                            newHeight = sHeigh * width / sWidth;
                                            newWidth = width;
                                        }
                                        else
                                        {
                                            newWidth = height * sWidth / sHeigh;
                                            newHeight = height;
                                        }
                                    }
                                    img = new Bitmap(sourceImg, newWidth, newHeight);
                                }

                                sourceImg.Dispose();
                                sourceImg = null;
                            }
                        }
                        ms.Close();
                    }
                    fs.Close();
                }
            }
            return img;
        }

        /// <summary>
        /// 基于指定区域的宽或高获取等比例缩放的图像
        /// </summary>
        /// <param name="imageFilePath">图像文件路径</param>
        /// <param name="wRefer">参照区域的宽度</param>
        /// <param name="hRefer">参照区域的高度</param>
        /// <returns></returns>
        public static Bitmap GetFittedImage(string imageFilePath, int wRefer, int hRefer)
        {
            Image sourceImg = CommonFuns.GetImageFromFile(imageFilePath, true);
            if (sourceImg == null) return null;

            int w = 0, h = 0;
            if (wRefer > 0)
            {
                w = wRefer;
                h = Convert.ToInt32(wRefer * ((float)sourceImg.Height / sourceImg.Width));
            }
            else if (hRefer > 0)
            {
                w = Convert.ToInt32(hRefer * ((float)sourceImg.Width / sourceImg.Height));
                h = hRefer;
            }
            else
            {
                return null;
            }

            Bitmap bmp = new Bitmap(sourceImg, w, h);
            sourceImg = null;
            return bmp;
        }

        public static Bitmap GetResizedImage(Bitmap srcImage, Size size)
        {
            Bitmap bmp = srcImage.Clone(new Rectangle(0, 0, size.Width, size.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            srcImage.Dispose();
            srcImage = null;
            return bmp;
        }

        public static Bitmap GetResizedImage(Bitmap srcImage, int targetWidth, int targetHeight)
        {
            Bitmap resizedImage = new Bitmap(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                // 高质量缩放
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                //// 计算缩放比例（保持宽高比）
                //float ratio = Math.Min(
                //    (float)targetWidth / bmp.Width,
                //    (float)targetHeight / bmp.Height
                //);
                //int newWidth = (int)(bmp.Width * ratio);
                //int newHeight = (int)(bmp.Height * ratio);

                //// 居中绘制
                //g.DrawImage(bmp,
                //    new Rectangle((targetSize - newWidth) / 2, (targetSize - newHeight) / 2, newWidth, newHeight));
                g.DrawImage(srcImage, 0, 0, targetWidth, targetHeight);
            }
            return resizedImage;
        }
        #endregion

        #region 导联、导联体系、导联布局等
        /// <summary>
        /// 根据导联模式获取相关布局方式
        /// </summary>
        /// <param name="leadMode"></param>
        /// <returns></returns>
        public static List<ListItem> GetLayouts(int leadMode)
        {
            List<ListItem> layouts = new List<ListItem>();
            switch (leadMode)
            {
                case 0: //6导
                    layouts.Add(new ListItem(12, SysConstants.Layouts[12]));
                    break;
                case 1: //12导
                    for (int i = 0; i <= 5; i++)
                    {
                        layouts.Add(new ListItem(i, SysConstants.Layouts[i]));
                    }
                    break;
                case 2: //15导
                    for (int i = 10; i <= 11; i++)
                    {
                        layouts.Add(new ListItem(i, SysConstants.Layouts[i]));
                    }
                    break;
                case 3: //18导
                    for (int i = 6; i <= 9; i++)
                    {
                        layouts.Add(new ListItem(i, SysConstants.Layouts[i]));
                    }
                    break;
                case 4: //9导
                    for (int i = 13; i <= 16; i++)
                    {
                        layouts.Add(new ListItem(i, SysConstants.Layouts[i]));
                    }
                    break;
            }

            return layouts;
        }

        /// <summary>
        /// 根据波形布局(x*y+z)获取布局各分量值
        /// </summary>
        /// <param name="layoutIndex"></param>
        /// <returns></returns>
        public static Tuple<int, int, int> GetLayoutTuple(int layoutIndex)
        {
            string layoutString = SysConstants.Layouts[layoutIndex];
            string[] arr = new string[3];
            if (layoutString.IndexOf('x') < 0) //9导可能有"a+b"布局
            {
                arr = layoutString.Split('+');
                return new Tuple<int, int, int>(Convert.ToInt32(arr[0]), 0, Convert.ToInt32(arr[1]));
            }
            else
            {
                arr = layoutString.Split(new char[] { 'x', '+' });
                return new Tuple<int, int, int>(Convert.ToInt32(arr[0]), Convert.ToInt32(arr[1]), (arr.Length > 2 ? Convert.ToInt32(arr[2]) : 0));
            }
        }

        /// <summary>
        /// 根据导联模式和导联体系获取相关导联
        /// </summary>
        /// <param name="leadMode">导联模式</param>
        /// <param name="Tissues">导联体系</param>
        /// <returns></returns>
        public static List<Lead> GetLeads(int leadMode, Tissues tissues)
        {
            List<Lead> lst = new List<Lead>();
            string relatedLeads = string.Empty;
            if (leadMode == 0) //6导
            {
                //lst = new List<Lead>() { Lead.I, Lead.II, Lead.III, Lead.aVR, Lead.aVL, Lead.aVF };
                relatedLeads = SysConstants.ECG_LT_STANDARD[0];
            }
            else if (leadMode == 1) //12导
            {
                relatedLeads = SysConstants.ECG_LT_SYSTEM[tissues];
            }
            else if (leadMode == 2) //15导
            {
                relatedLeads = SysConstants.ECG_LT_STANDARD[2];
            }
            else if (leadMode == 3) //18导
            {
                relatedLeads = SysConstants.ECG_LT_STANDARD[3];
            }
            else if (leadMode == 4) //9导
            {
                relatedLeads = SysConstants.ECG_LT_STANDARD[4];
            }

            string[] arr = relatedLeads.Split(',');
            int plus = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                int leadIndex = i;
                //调整导联序号枚举值, 使其与诊断算法中的ZQEcgLeadType一致
                if (leadMode == 2 && i >= 12)
                {
                    leadIndex += 3;
                }
                else if (leadMode == 4 && i >= 6)
                {
                    leadIndex = i + plus;
                    plus++;
                }

                Lead leadE = new Lead()
                {
                    Index = leadIndex,
                    Alias = SysConstants.LeadAlias[i],
                    RealName = arr[i]
                };
                lst.Add(leadE);
            }

            return lst;
        }

        /// <summary>
        /// 获取界面波形显示的相关导联
        /// </summary>
        /// <param name="leadMode"></param>
        /// <param name="LeadSystem"></param>
        /// <param name="layoutIndex"></param>
        /// <param name="singleRhythm"></param>
        /// <param name="threeRhythm1"></param>
        /// <param name="threeRhythm2"></param>
        /// <param name="threeRhythm3"></param>
        /// <param name="oneLeadOnly">是否仅用于1个导联(节律分析, 心率变异性)</param>
        /// <param name="rawLeadOnly">仅获取原始数据导联(不含单节律、三节律导联)</param>
        /// <returns></returns>
        public static List<Lead> GetRelatedLeads(int leadMode, Tissues LeadSystem, int layoutIndex, int singleRhythm, int threeRhythm1, int threeRhythm2, int threeRhythm3,
            bool oneLeadOnly = false, bool rawLeadOnly = false)
        {
            List<Lead> lstLead = new List<Lead>();
            Lead singleLead = new Lead()
            {
                Index = 100 + singleRhythm,
                Alias = SysConstants.LeadAlias[singleRhythm],
                RealName = SysConstants.LeadAlias[singleRhythm]
            };

            if (oneLeadOnly)
            {
                lstLead.Add(singleLead);
            }
            else
            {
                lstLead = CommonFuns.GetLeads(leadMode, LeadSystem);
                if (!rawLeadOnly)
                {
                    Tuple<int, int, int> layout = CommonFuns.GetLayoutTuple(layoutIndex);
                    if (layout.Item2 > 0)
                    {
                        if (layout.Item3 == 1)
                        {
                            singleLead.RealName = lstLead[singleRhythm].RealName;
                            lstLead.Add(singleLead);
                        }
                        else if (layout.Item3 == 3)
                        {
                            Lead lead1 = new Lead()
                            {
                                Index = 100 + threeRhythm1,
                                Alias = SysConstants.LeadAlias[threeRhythm1],
                                RealName = lstLead[threeRhythm1].RealName
                            };
                            lstLead.Add(lead1);

                            Lead lead2 = new Lead()
                            {
                                Index = 100 + threeRhythm2,
                                Alias = SysConstants.LeadAlias[threeRhythm2],
                                RealName = lstLead[threeRhythm2].RealName
                            };
                            lstLead.Add(lead2);

                            Lead lead3 = new Lead()
                            {
                                Index = 100 + threeRhythm3,
                                Alias = SysConstants.LeadAlias[threeRhythm3],
                                RealName = lstLead[threeRhythm3].RealName
                            };
                            lstLead.Add(lead3);
                        }
                    }
                }
            }

            return lstLead;
        }

        /// <summary>
        /// 从当前系统导出设置中获取波形显示设置信息
        /// </summary>
        /// <returns></returns>
        public static WaveSettings GetWaveSettingInfo(WaveSettings originalSetting)
        {
            WaveSettings waveSets = new WaveSettings();
            waveSets.ScanSpeed = CommonValues.EcgSettings.PaperSpeed;
            waveSets.Gain = CommonValues.EcgSettings.Gain;
            waveSets.GridVisible = (CommonValues.EcgSettings.ShowGrid == 1);
            waveSets.Layout_6 = CommonValues.EcgSettings.LeadModesInfo[0].LayoutIndex;
            waveSets.Layout_12 = CommonValues.EcgSettings.LeadModesInfo[1].LayoutIndex;
            waveSets.Layout_15 = CommonValues.EcgSettings.LeadModesInfo[2].LayoutIndex;
            waveSets.Layout_18 = CommonValues.EcgSettings.LeadModesInfo[3].LayoutIndex;
            waveSets.Layout_9 = CommonValues.EcgSettings.LeadModesInfo[4].LayoutIndex;
            if (originalSetting != null)
            {
                waveSets.ChannelWidth = originalSetting.ChannelWidth;
                waveSets.GridColor = originalSetting.GridColor;
            }

            return waveSets;
        }

        /// <summary>
        /// 基于一个导联获取数据总个数
        /// </summary>
        /// <param name="ecgData"></param>
        /// <param name="relatedLeads"></param>
        /// <returns></returns>
        public static int GetPointCount(Dictionary<int, PointD[]> ecgData, List<Lead> relatedLeads)
        {
            int totalNum = 0;
            foreach (Lead leadN in relatedLeads)
            {
                int dataIndex = leadN.Index % 100;
                if (!ecgData.ContainsKey(dataIndex)) continue;

                PointD[] srcPoints = ecgData[dataIndex];
                totalNum = srcPoints.Length;
                break;
            }

            return totalNum;
        }

        /// <summary>
        /// 获取高级功能中Frank导联体系(向量心电、时间向量、心室晚电位)相关导联
        /// </summary>
        /// <param name="leadMode"></param>
        /// <returns></returns>
        public static List<Lead> GetVectorLeads(int leadMode)
        {
            List<Lead> lst = new List<Lead>();
            string relatedLeads = string.Empty;
            if (leadMode == 0)
            {
                string[] leads = SysConstants.LeadAlias_ADV;
                for (int i = 0; i < leads.Length; i++)
                {
                    Lead leadE = new Lead()
                    {
                        Index = i,
                        Alias = leads[i],
                        RealName = leads[i]
                    };
                    lst.Add(leadE);
                }
            }
            return lst;
        }
        #endregion


        //获取心电文件采集相关的设备类型
        public static EcgDeviceType GetDeviceTypeFromZQECG(int machineType)
        {
            if (1 <= machineType && machineType <= 8)
                return EcgDeviceType.Device_iMAC;
            else
                return EcgDeviceType.Device_unknow;
        }

        //执行一系列方法
        public static void ExecuteMethods(List<Action> funs, string logModule)
        {
            string errMsg = string.Empty;
            int cnt = 0;
            foreach (Action fun in funs)
            {
                try
                {
                    fun.Invoke();
                    cnt++;
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                    Logger.CreateLog(ex, logModule);
                }
            }
            if (cnt <= 0) throw new Exception(errMsg);
        }


        /// <summary>
        /// 将Base64字符串转换为Image对象
        /// </summary>
        /// <param name="base64String">Base64字符串（可以包含data:image/xxx;base64,前缀）</param>
        /// <returns>Image对象</returns>
        public static Image Base64ToImage(string base64String)
        {
            try
            {
                // 移除可能的Base64前缀
                string cleanBase64 = CleanBase64String(base64String);

                // 将Base64字符串转换为字节数组
                byte[] imageBytes = Convert.FromBase64String(cleanBase64);

                // 使用MemoryStream创建Image对象
                using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    return Image.FromStream(ms, true); // true表示使用嵌入的颜色管理信息
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Base64字符串格式不正确", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("无效的图片数据", ex);
            }
        }
        /// <summary>
        /// 清理Base64字符串，移除前缀
        /// </summary>
        private static string CleanBase64String(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return base64String;

            // 检查并移除常见的Base64前缀
            if (base64String.Contains(","))
            {
                return base64String.Substring(base64String.IndexOf(",") + 1);
            }

            return base64String;
        }
        /// 文件转Base64
        public static string FileToBase64(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(fileBytes);
        }
    }
}
