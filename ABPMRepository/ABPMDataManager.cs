using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace ABPMRepository
{
    /// <summary>
    /// 动态血压监测数据存储(arteria blood pressure monitoring)
    /// </summary>
    public class ABPMxmlManager
    {
        private ABPMOperator op;
        private bool showSettingWindow = false, doQuiet = false;
        private readonly Regex m_regOK = new Regex(@"^@OK((\r\n)|$)", RegexOptions.Multiline);

        private PatientInformation patientInfo;
        public PatientInformation PatientInfo
        {
            get { return patientInfo; }
            private set { patientInfo = value; }
        }

        public ABPMxmlManager()
        {
            op = new ABPMOperator();
        }

        /// <summary>
        /// 初始化类
        /// </summary>
        /// <param name="callSettings">标识是否在写入病人信息时打开参数设置界面</param>
        /// <param name="doInSilence">标识是否以静默方式调用接口方法</param>
        public ABPMxmlManager(bool callSettings, bool doInSilence)
        {
            op = new ABPMOperator();
            this.showSettingWindow = callSettings;
            this.doQuiet = doInSilence;
        }

        ~ABPMxmlManager()
        {
            //monitor.ClosePort();
        }

        /// <summary>
        /// 从血压设备读取患者信息
        /// </summary>
        /// <returns></returns>
        public PatientInformation ReadPatientInfo()
        {
            if (!IsConnectionReady(out string errMsg, out bool isUseBluetooth))
            {
                throw new Exception(errMsg);
            }
            string strOutput = op.OutputPatient();
            return traversePatientInfo(strOutput);
        }

        /// <summary>
        /// 录入病人信息至血压计
        /// </summary>
        /// <param name="patientID">病人ID，格式如"201905090105"</param>
        /// <param name="sex">性别，F标识女，M标识男</param>
        /// <param name="patientName">病人的姓名</param>
        /// <param name="age">病人的年龄</param>
        /// <param name="ageUnit">Y标识年，M标识月，D标识天，W标识周</param>
        /// <param name="examItem">16检查类型，固定使用16标识是血压</param>
        /// <param name="examdeptCode">检查的科室代号</param>
        /// <param name="resultStatus">病例状态，2标识未诊断，3标识已诊断，7标识已审核</param>
        /// <param name="callSettings">指示是否进入血压计参数设置界面</param>
        /// <returns>operation result(true: successful; false: failed)</returns>
        public bool InputPatientInfo(string patientID, string sex, string patientName, int age, string ageUnit, int examItem, string examdeptCode, int resultStatus)
        {
            bool blnReturn = false;

            if (!(new string[] { "F", "M", "O" }).Contains(sex, new StringIgnoreCaseComparer()))
            {
                if (!doQuiet) MessageBox.Show("性别参数格式不正确", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            if (string.IsNullOrWhiteSpace(patientName))
            {
                if (!doQuiet) MessageBox.Show("病人姓名不能为空", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            if (!(new string[] { "Y", "M", "D", "W", "H", "O" }).Contains(ageUnit, new StringIgnoreCaseComparer()))
            {
                if (!doQuiet) MessageBox.Show("年龄单位格式不正确", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            string remoteMac = string.Empty;
            if (showSettingWindow)
            {
                bool blnSetPara = SetParameters(out remoteMac);
                if (!blnSetPara) return false;
            }

            if (string.IsNullOrEmpty(remoteMac) || remoteMac == "0") //若未指定以蓝牙连接, 则在直连失败时提示是否尝试用蓝牙
            {
                string errMsg = string.Empty;
                if (!IsConnectionReady(out errMsg, out bool isUseBluetooth))
                {
                    throw new Exception(errMsg);
                }
            }

            string strPatient = string.Join(",", patientID, sex, patientName, age, ageUnit, examItem, examdeptCode, resultStatus);
            string opResult = op.InputPatient(strPatient);
            if (m_regOK.IsMatch(opResult.ToUpper()))
            {
                traversePatientInfo(strPatient);
                blnReturn = true;
            }

            return blnReturn;
        }

        /// <summary>
        /// append BP record into the specified .xml file
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        [Obsolete]
        public bool SaveABPM_Append(byte[] newData, string fullFileName)
        {
            bool OPResult = false;
            if (!File.Exists(fullFileName))
            {
                if (!doQuiet) MessageBox.Show("未找到xml文件");
                return false;
            }

            string strNewData = Encoding.Unicode.GetString(newData, 0, newData.Length);

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(fullFileName);
                //XmlNodeList lstRowNode = xmlDoc.GetElementsByTagName("row");
                XmlNode parentNode = xmlDoc.SelectSingleNode("root//RecordInfo");
                XmlElement newElement = xmlDoc.CreateElement("row");
                newElement.InnerText = strNewData;
                //xmlDoc.InsertAfter(newElement, lstRowNode[lstRowNode.Count - 1]);
                parentNode.AppendChild(newElement);
                xmlDoc.Save(fullFileName);
                OPResult = true;
            }
            catch (Exception ex)
            {
                if (!doQuiet) MessageBox.Show(ex.Message);
                OPResult = false;
            }

            return OPResult;
        }

        /// <summary>
        /// generate xml file to save the patient ABPM data
        /// </summary>
        /// <param name="dataObject">ABPM data instance, include the patient information</param>
        /// <param name="saveFilePath">xml file path</param>
        [Obsolete]
        public void SaveABPM_Local(ABMPInformation ABMPInfo, string saveFilePath = "")
        {
            if (saveFilePath.Trim().Length == 0)
                saveFilePath = System.AppDomain.CurrentDomain.BaseDirectory;

            string fileName = saveFilePath + @"\ABPMCase.dat";

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                XmlSerializer serializer = new XmlSerializer(ABMPInfo.GetType());

                //MemoryStream ms = new MemoryStream();
                //XmlWriterSettings xws = new XmlWriterSettings();
                //xws.Indent = true;
                //xws.OmitXmlDeclaration = true;
                //xws.Encoding = Encoding.UTF8;
                //XmlWriter xtw = XmlTextWriter.Create(ms, xws);
                XmlSerializerNamespaces _namespaces = new XmlSerializerNamespaces(
                    new XmlQualifiedName[] {
                        new XmlQualifiedName(string.Empty, "aa")
                 });

                TextWriter writer = new StreamWriter(fileName);
                serializer.Serialize(writer, ABMPInfo, _namespaces);
                //ms.Position = 0;
                writer.Close();

                //xmlDoc.Load(ms);
                ////给文档添加<?xml version="1.0" encoding="utf-8"?>
                //XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                //xmlDoc.InsertBefore(xmlDecl, xmlDoc.DocumentElement);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 读取血压计设备内采集到的数据并将数据以xml文件格式保存至指定的位置
        /// </summary>
        /// <param name="saveFileName">指定要保存的xml文件全路径</param>
        /// <param name="patientID">病人ID，格式如"201905090105"</param>
        /// <param name="sex">性别，F标识女，M标识男</param>
        /// <param name="patientName">病人的姓名</param>
        /// <param name="age">病人的年龄</param>
        /// <param name="ageUnit">Y标识年，M标识月，D标识天，W标识周</param>
        /// <param name="examItem">16检查类型，固定使用16标识是血压</param>
        /// <param name="examdeptCode">检查的科室代号</param>
        /// <param name="resultStatus">病例状态，2标识未诊断，3标识已诊断，7标识已审核</param>
        /// <returns>返回值(0:保存成功; -1:处理发生异常; 1:输入参数有误; 2:通信连接失败)</returns>
        public int SaveABPM(string saveFileName, string patientID, string sex, string patientName, int age, string ageUnit, int examItem, string examdeptCode, int resultStatus)
        {
            if (!(new string[] { "F", "M", "O" }).Contains(sex, new StringIgnoreCaseComparer()))
            {
                if (!doQuiet) MessageBox.Show("性别参数格式不正确", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 1;
            }
            if (string.IsNullOrWhiteSpace(patientName))
            {
                if (!doQuiet) MessageBox.Show("病人姓名不能为空", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 1;
            }
            if (!(new string[] { "Y", "M", "D", "W", "H", "O" }).Contains(ageUnit, new StringIgnoreCaseComparer()))
            {
                if (!doQuiet) MessageBox.Show("年龄的单位格式不正确", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 1;
            }

            this.patientInfo = new PatientInformation()
            {
                PatientID = patientID,
                Sex = sex,
                Name = patientName,
                Age = age,
                AgeUnit = ageUnit,
                ExamItem = examItem,
                examdeptCode = (examdeptCode.Length == 0 ? " " : examdeptCode),
                ResultStatus = resultStatus
            };

            int opResult = 0;
            if (saveFileName.Trim().Length == 0)
            {
                saveFileName = System.AppDomain.CurrentDomain.BaseDirectory +
                    string.Concat("ABPM_", DateTime.Now.ToString("yyyyMMddHHmmss") + ".dat");
            }

            try
            {
                string errMsg = string.Empty;
                if (!IsConnectionReady(out errMsg, out bool isUseBluetooth))
                {
                    if (!doQuiet) MessageBox.Show(errMsg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 2;
                }

                ABMPInformation ABPMCase = new ABMPInformation();
                ABPMCase.RecordInfo = getABPMRecords();
                ABPMCase.PatientInfo = patientInfo;

                XmlSerializer serializer = new XmlSerializer(ABPMCase.GetType());
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(
                    new XmlQualifiedName[] {
                        new XmlQualifiedName(string.Empty, "aa")
                 });

                TextWriter writer = new StreamWriter(saveFileName);
                serializer.Serialize(writer, ABPMCase, namespaces);
                writer.Close();

                opResult = 0;
            }
            catch (Exception ex)
            {
                if (!doQuiet) ZqBPFuns.ShowErrors(ex);
                opResult = -1;
            }

            return opResult;
        }

        /// <summary>
        /// 已验证血压计的通讯连接, 此时可直接读写数据
        /// </summary>
        public int SaveABPM_Validated(string saveFileName, string patientID, string sex, string patientName, int age, string ageUnit, int examItem, string examdeptCode, int resultStatus)
        {
            if (!(new string[] { "F", "M", "O" }).Contains(sex, new StringIgnoreCaseComparer()))
            {
                if (!doQuiet) MessageBox.Show("性别参数格式不正确", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 1;
            }
            if (string.IsNullOrWhiteSpace(patientName))
            {
                if (!doQuiet) MessageBox.Show("病人姓名不能为空", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 1;
            }
            if (!(new string[] { "Y", "M", "D", "W", "H", "O" }).Contains(ageUnit, new StringIgnoreCaseComparer()))
            {
                if (!doQuiet) MessageBox.Show("年龄的单位格式不正确", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 1;
            }

            this.patientInfo = new PatientInformation()
            {
                PatientID = patientID,
                Sex = sex,
                Name = patientName,
                Age = age,
                AgeUnit = ageUnit,
                ExamItem = examItem,
                examdeptCode = (examdeptCode.Length == 0 ? " " : examdeptCode),
                ResultStatus = resultStatus
            };

            int opResult = 0;
            if (saveFileName.Trim().Length == 0)
            {
                saveFileName = System.AppDomain.CurrentDomain.BaseDirectory +
                    string.Concat("ABPM_", DateTime.Now.ToString("yyyyMMddHHmmss") + ".dat");
            }

            try
            {
                ABMPInformation ABPMCase = new ABMPInformation();
                ABPMCase.RecordInfo = getABPMRecords();
                ABPMCase.PatientInfo = patientInfo;

                XmlSerializer serializer = new XmlSerializer(ABPMCase.GetType());
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(
                    new XmlQualifiedName[] {
                        new XmlQualifiedName(string.Empty, "aa")
                 });

                TextWriter writer = new StreamWriter(saveFileName);
                serializer.Serialize(writer, ABPMCase, namespaces);
                writer.Close();

                opResult = 0;
            }
            catch (Exception ex)
            {
                if (!doQuiet) ZqBPFuns.ShowErrors(ex);
                opResult = -1;
            }

            return opResult;
        }

        /// <summary>
        /// 设置血压计设备参数
        /// </summary>
        /// <param name="arrTimeSpan">时间段设置信息</param>
        /// <param name="deviceTime">设备的系统时间</param>
        /// <param name="frontPanelData">键盘及显示设置信息</param>
        /// <param name="alarmData">报警设置信息</param>
        /// <param name="pressureUnit">压力的单位</param>
        /// <param name="pressureThreshold">阈值信息</param>
        /// <param name="useBluetooth">是否指定用蓝牙连接模式</param>
        /// <returns>返回值(0:保存成功; -1:处理发生异常; 1:时间分配信息写入出错; 2:通信连接失败)</returns>
        public int SetParameters(string[] arrTimeSpan, string deviceTime, string frontPanelData, string alarmData, string pressureUnit, string pressureThreshold,
            bool useBluetooth = false, PatientInformation patientInfo = null)
        {
            int opResult = 0;

            if (!useBluetooth) //若未指定以蓝牙连接, 则在直连失败时提示是否尝试用蓝牙
            {
                string errMsg = string.Empty;
                if (!IsConnectionReady(out errMsg, out bool isUseBluetooth))
                {
                    //throw new Exception("设备连接通信失败!");
                    return 2;
                }
            }

            try
            {
                op.OpenPort();
                foreach (string timeSpan in arrTimeSpan)
                {
                    string result = op.SetTimeSpan(timeSpan, true);
                    if (!m_regOK.IsMatch(result.ToUpper()))
                    {
                        //throw new Exception("时间分配信息写入出错!");
                        return 1;
                    }
                }

                string responseMsg = op.SetParameters(string.Join(",", deviceTime, frontPanelData, alarmData, pressureUnit, pressureThreshold), true);
                if (m_regOK.IsMatch(responseMsg.ToUpper()))
                {
                    //写入病人信息至血压计, 此过程将清除历史血压数据
                    string strPatient = string.Join(",", "100001", "F", "default", 0, "Y", 16, "001", 2);
                    if (patientInfo != null)
                    {
                        strPatient = patientInfo.PatientID + "," +
                            patientInfo.Sex + "," +
                            patientInfo.Name + "," +
                            patientInfo.Age + "," +
                            patientInfo.AgeUnit + "," +
                            patientInfo.ExamItem + "," +
                            patientInfo.examdeptCode + "," +
                            patientInfo.ResultStatus;
                    }
                    string inputPatientResult = op.InputPatient(strPatient, true);
                    if (m_regOK.IsMatch(inputPatientResult.ToUpper()))
                    {
                        opResult = 0;
                    }
                    else
                    {
                        opResult = -1;
                    }
                }
                else
                {
                    //throw new Exception("参数信息写入出错!");
                    opResult = -1;
                }
            }
            catch (Exception ex)
            {
                opResult = -1;
            }
            finally
            {
                op.ClosePort();
            }

            return opResult;
        }

        /// <summary>
        /// 打开设备参数设置界面
        /// </summary>
        /// <returns>operation result(true: successful; false: failed)</returns>
        public bool SetParameters()
        {
            bool OPResult = false;
            try
            {
                DeviceSettings frmSettings = new DeviceSettings(op);
                frmSettings.ShowDialog();
                //OPResult = (frmSettings.ShowDialog() == DialogResult.OK);
                //if (frmSettings.OperationSucceeded)
                OPResult = true;
            }
            catch
            {
                OPResult = false;
                throw;
            }

            return OPResult;
        }

        public string GetParameters()
        {
            string paraInfo = string.Empty;
            string errMsg = string.Empty;
            if (!IsConnectionReady(out errMsg, out bool isUseBluetooth))
            {
                MessageBox.Show(errMsg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return "";
            }

            paraInfo = op.GetParameters();
            return paraInfo;
        }

        /// <summary>
        /// 设置设备的序列号
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="useBluetooth"></param>
        /// <returns></returns>
        public int SetSN(string sn, bool useBluetooth = false)
        {
            int opResult = 0;

            if (!useBluetooth) //若未指定以蓝牙连接, 则在直连失败时提示是否尝试用蓝牙
            {
                string errMsg = string.Empty;
                if (!IsConnectionReady(out errMsg, out bool isUseBluetooth))
                {
                    return 2;
                }
            }

            try
            {
                op.OpenPort();

                string responseMsg = op.SetSN(sn, true);
                if (false == m_regOK.IsMatch(responseMsg.ToUpper()))
                {
                    opResult = 1;
                }
            }
            catch
            {
                opResult = -1;
            }
            finally
            {
                op.ClosePort();
            }

            return opResult;
        }

        /// <summary>
        /// 获取设备序列号
        /// </summary>
        /// <returns></returns>
        public string GetSN(out int errCode)
        {
            string sn = string.Empty;
            errCode = 0;
            try
            {
                string errMsg = string.Empty;
                if (!IsConnectionReady(out errMsg, out bool isUseBluetooth))
                {
                    if (!doQuiet) MessageBox.Show(errMsg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    errCode = -2;
                }

                if (errCode == 0)
                {
                    sn = op.GetSN(false);
                }
            }
            catch (Exception ex)
            {
                if (!doQuiet) ZqBPFuns.ShowErrors(ex);
                errCode = -1;
            }

            return sn;
        }

        /// <summary>
        /// 获取可连接的蓝牙设备列表
        /// </summary>
        /// <returns></returns>
        public List<BluetoothInformation> GetBluetoothList()
        {
            return op.GetBluetooths();
        }

        /// <summary>
        /// 按蓝牙设备地址连接指定的蓝牙
        /// </summary>
        /// <param name="remoteMac"></param>
        /// <returns></returns>
        public bool ConnectBluetooth(string remoteMac)
        {
            string errCode = string.Empty;
            return op.TryConnectDevice(out errCode, true, remoteMac);
        }

        /// <summary>
        /// 检测是否处于设备直连模式并能通信, 若不通则提供蓝牙连接模式尝试连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnectionReady(out string errorMessage, out bool isUseBluetooth, int retryTimes = 3)
        {
            string errCode = string.Empty;
            bool connectable = op.TryConnectDevice(out errCode);
            isUseBluetooth = false;
            if (!connectable)
            {
                if ("400" == errCode)
                {
                    errorMessage = "没有检测到本机通信串口！";
                    return false;
                }
                else if ("401" == errCode)
                {
                    if (doQuiet) //无弹窗模式调用时直接返回
                    {
                        errorMessage = "设备通信连接失败！";
                        return false;
                    }

                    if (MessageBox.Show("未检测到本机可用设备, 是否选择蓝牙连接？", "提示信息", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        BluetoothConnection frmBluetoothConn = new BluetoothConnection(op);
                        frmBluetoothConn.ShowDialog();

                        if (!string.IsNullOrEmpty(frmBluetoothConn.ErrorMessage))
                        {
                            errorMessage = frmBluetoothConn.ErrorMessage;
                            return false;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(frmBluetoothConn.SelectedBluetooth))
                            {
                                errorMessage = "设备通信连接失败！";
                                return false;
                            }
                            else
                            {
                                isUseBluetooth = true;
                            }
                        }
                    }
                    else
                    {
                        errorMessage = "设备通信连接失败！";
                        return false;
                    }
                }
                else
                {
                    errorMessage = "设备通信连接失败！";
                    return false;
                }
            }

            errorMessage = "";
            return true;
        }

        public async Task<string> CheckDeviceStatus()
        {
            string code = await Task.Run(() =>
            {
                bool connectable = op.TryConnectDevice(out string errCode);
                return errCode;
            });
            return code;
        }

        private bool SetParameters(out string remoteMac)
        {
            bool OPResult = false;
            try
            {
                DeviceSettings frmSettings = new DeviceSettings(op);
                frmSettings.ShowDialog();

                remoteMac = frmSettings.SelectedBluetooth;
                //OPResult = (frmSettings.ShowDialog() == DialogResult.OK);
                //if (frmSettings.OperationSucceeded)
                OPResult = true;
            }
            catch
            {
                OPResult = false;
                throw;
            }

            return OPResult;
        }

        // 从血压计获取已记录的各项血压指标信息
        private string[] getABPMRecords()
        {
            string[] arrABPMRecords = new string[] { };
            try
            {
                op.OpenPort();
                int recordCount = op.GetRecordCount(true);
                arrABPMRecords = new string[recordCount];

                for (int i = 0; i < recordCount; i++)
                {
                    //血压计中序号为0的记录是最新的记录
                    arrABPMRecords[i] = op.ReadRecord(recordCount - 1 - i);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                op.ClosePort();
            }

            return arrABPMRecords;
        }

        private PatientInformation traversePatientInfo(string strPatientInfo)
        {
            this.patientInfo = null;
            string[] arrOutput = strPatientInfo.Split(',');
            if (arrOutput.Length == 8)
            {
                this.patientInfo = new PatientInformation()
                {
                    PatientID = arrOutput[0],
                    Sex = arrOutput[1],
                    Name = arrOutput[2],
                    Age = Convert.ToInt32(arrOutput[3]),
                    AgeUnit = arrOutput[4],
                    ExamItem = Convert.ToInt32(arrOutput[5]),
                    examdeptCode = (arrOutput[6].Length == 0 ? " " : arrOutput[6]),
                    ResultStatus = (arrOutput[7].Length == 0 ? 0 : Convert.ToInt32(arrOutput[7]))
                };
            }
            return this.patientInfo;
        }

    }

    [XmlRoot("root")]
    public class ABMPInformation
    {
        public ABMPInformation() { }

        [XmlArray("RecordInfo"), XmlArrayItem("row")]
        public string[] RecordInfo { get; set; }

        [XmlIgnore, XmlElement(ElementName = "RecordInfo")]
        public RowInformation RecordInfos { get; set; }

        [XmlElement(ElementName = "PatientInfo")]
        public PatientInformation PatientInfo { get; set; }
    }

    public class RowInformation
    {
        public RowInformation() { }

        [XmlElement(ElementName = "row")]
        public string[] RowData { get; set; }
    }

    public class PatientInformation
    {
        public PatientInformation() { }

        public string PatientID { get; set; }
        public string Sex { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string AgeUnit { get; set; }
        public int ExamItem { get; set; }
        public string examdeptCode { get; set; }
        public int ResultStatus { get; set; }
    }

}
