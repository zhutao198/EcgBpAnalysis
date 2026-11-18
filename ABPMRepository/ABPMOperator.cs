using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace ABPMRepository
{
    public class ABPMOperator
    {
        private SerialPort com;
        private string m_CurrentRemoteBleMAC = string.Empty; //通过蓝牙连接的血压计设备内蓝牙元器件的MAC地址
        private string m_UsedBleMAC = string.Empty;
        private readonly int m_ATbleTimeout = 4000; //等待AT指令执行完毕所经历的时间(ms)
        private readonly int m_BaudRate = 115200;
        private readonly int m_WriteTimeout = 800;
        private bool m_isReady; //在串口打开的前提下, 连接通信是否正常
        private const int MAX_BYTE_RECEIVED = 1024;
        private static object m_SyncRoot = new object();

        private int m_recordCount;
        public int RecordCount
        {
            get { return m_recordCount; }
            set { m_recordCount = value; }
        }

        private bool m_ConnectByBluetooth = false;
        public bool ConnectByBluetooth
        {
            get { return m_ConnectByBluetooth; }
            set { m_ConnectByBluetooth = value; }
        }

        private string m_bluetoothPrefix = "ZQBP";
        public string BluetoothPrefix
        {
            get { return m_bluetoothPrefix; }
            set { m_bluetoothPrefix = value; }
        }

        public ABPMOperator()
        {
            //m_SyncRoot = this;
        }

        public ABPMOperator(SerialPort sourceCom)
        {
            this.com = sourceCom;
        }

        public void OpenPort()
        {
            if (com != null)
            {
                if (!com.IsOpen)
                {
                    com.Open();
                    Console.WriteLine(com.PortName + " opened at " + DateTime.Now.ToString());
                }
            }
        }

        public void ClosePort()
        {
            if (com != null && com.IsOpen)
            {
                com.Close();
                com.Dispose();
            }
        }

        /// <summary>
        /// 尝试与设备进行连接通信
        /// </summary>
        /// <param name="errorCode">错误代码(400:通信失败; 401:直连失败; 402:蓝牙连接失败)</param>
        /// <param name="isSpecified">是否指定了连接方式</param>
        /// <param name="remoteMAC">远端MAC</param>
        /// <param name="retryTimes">尝试连接串口的次数</param>
        /// <returns></returns>
        public bool TryConnectDevice(out string errorCode, bool isSpecified = false, string remoteMAC = "", int retryTimes=3)
        {
            bool isUsedChannel = false, isOK = false;
            bool usedIsBluetooth = false;
            string usedPortName = string.Empty;
            string remoteBleMac = string.Empty;
            errorCode = "";

            string[] serialPorts = SerialPort.GetPortNames();
            if (serialPorts == null)
            {
                errorCode = "400"; //"没有检测到本机通信串口！";
                return false;
            }

            ConnectionInformation connInfo = getConnectionInfo();
            if (connInfo != null)
            {
                usedPortName = connInfo.UsedSerialPort;
                usedIsBluetooth = connInfo.UsedChannel.ToLower() == "bluetooth";
                remoteBleMac = connInfo.UsedBluetoothMAC;
                isUsedChannel = true;
            }

            var checkPortList = serialPorts.OrderByDescending(o => o).AsEnumerable();
            if (isSpecified) //按指定方式连接(此时远端MAC非空)
            {
                if (serialPorts.Contains(usedPortName, new StringIgnoreCaseComparer()))
                {
                    isOK = testCommunication(usedPortName, true, isUsedChannel, remoteMAC);
                    if (isOK)
                    {
                        saveConnectionInfo();
                        return m_isReady = true;
                    }
                }

                //用其它串口尝试连接
                if (usedPortName != "") checkPortList = checkPortList.Where(o => string.Compare(o, usedPortName, true) != 0);
                foreach (string portName in checkPortList)
                {
                    isOK = testCommunication(portName, true, isUsedChannel, remoteMAC);
                    if (isOK)
                    {
                        saveConnectionInfo();
                        return m_isReady = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < retryTimes; i++)
                {
                    //20191029更新: 先前的蓝牙连接MAC不能直接重新拿来使用
                    if (serialPorts.Contains(usedPortName, new StringIgnoreCaseComparer()))
                    {
                        isOK = testCommunication(usedPortName, false, isUsedChannel, remoteBleMac);
                        if (isOK)
                        {
                            saveConnectionInfo();
                            return m_isReady = true;
                        }
                    }

                    if (usedPortName.Length > 0) checkPortList = checkPortList.Where(o => string.Compare(o, usedPortName, true) != 0);
                    foreach (string portName in checkPortList)
                    {
                        isOK = testCommunication(portName, false, isUsedChannel, remoteBleMac);
                        if (isOK)
                        {
                            //com.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
                            saveConnectionInfo();
                            return m_isReady = true;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(remoteMAC) || remoteMAC == "0")
                errorCode = "401"; //串口通信失败
            else
                errorCode = "402"; //蓝牙设备连接失败

            return m_isReady = false;
        }

        public List<BluetoothInformation> GetBluetooths()
        {
            List<BluetoothInformation> lstBluetooth = new List<BluetoothInformation>();
            bool usedIsBluetooth = false;
            string usedPortName = string.Empty, usedBluetoothMac = string.Empty;

            string[] serialPorts = SerialPort.GetPortNames();
            if (serialPorts == null) { throw new Exception("没有检测到本机通信串口！"); }

            ConnectionInformation connInfo = getConnectionInfo();
            if (connInfo != null)
            {
                usedPortName = connInfo.UsedSerialPort;
                usedIsBluetooth = connInfo.UsedChannel.ToLower() == "bluetooth";
                usedBluetoothMac = connInfo.UsedBluetoothMAC;
            }

            //用之前的成功连接过的串口尝试通信
            if (serialPorts.Contains(usedPortName, new StringIgnoreCaseComparer()))
            {
                lstBluetooth = GetBluetooths_Port(usedPortName, usedBluetoothMac);
            }
            if (lstBluetooth.Count > 0) return lstBluetooth;

            //若未扫描到蓝牙设备, 则用其它串口尝试
            var checkPortList = serialPorts.OrderByDescending(o => o).AsEnumerable();
            if (usedPortName != "") checkPortList = checkPortList.Where(o => string.Compare(o, usedPortName, true) != 0);
            foreach (string portName in checkPortList)
            {
                lstBluetooth = GetBluetooths_Port(portName, usedBluetoothMac);
                if (lstBluetooth.Count > 0) return lstBluetooth;
            }

            return lstBluetooth;
        }

        private List<BluetoothInformation> GetBluetooths_Port(string portName, string usedBluetoothMac)
        {
            List<BluetoothInformation> lstBluetooth = new List<BluetoothInformation>();
            Stopwatch sw = new Stopwatch();
            string errMsg = string.Empty;

            com = new SerialPort(portName) { BaudRate = m_BaudRate, WriteTimeout = m_WriteTimeout };
            Application.DoEvents();
            try
            {
                OpenPort();

                byte[] sendData = Encoding.UTF8.GetBytes("AT+STATUS\r\n");
                com.Write(sendData, 0, sendData.Length);
                //Logger.WriteLog("write data and waiting for response data receiving");
                string strStatus = receiveData(out errMsg);
                //Logger.WriteLog("data received: " + strStatus);
                if (strStatus.TrimEndNewlines().ToUpper().StartsWith("[AT]OK"))
                {
                    //断开之前可能存在的蓝牙连接
                    if (matchStatus("conn"))
                    {
                        sendData = Encoding.UTF8.GetBytes("AT+DISCON\r\n");
                        com.Write(sendData, 0, sendData.Length);
                        string strDiscon = receiveData(out errMsg);
                    }

                    sw.Start();
                    while (true)
                    {
                        Application.DoEvents();
                        if (matchStatus("idle")) break;

                        if (sw.Elapsed.TotalMilliseconds > m_ATbleTimeout)
                        {
                            bool reset = resetBleDevice();
                            if (reset)
                                break;
                            else
                                return lstBluetooth;
                        }
                    }

                    sendData = Encoding.UTF8.GetBytes("AT+SCAN_BLE\r\n");
                    com.Write(sendData, 0, sendData.Length);
                    int timeGap = 1000, totalBytes = 0;
                    string strScanResponse = receiveData_Multiline(ref totalBytes, "", out errMsg, 1500, timeGap);
                    if (!string.IsNullOrEmpty(strScanResponse))
                    {
                        sw.Restart();
                        while (true)
                        {
                            strScanResponse += receiveData(out errMsg, timeGap);

                            Application.DoEvents();
                            if (!matchStatus("scan")) break; //等待蓝牙扫描结束, 避免脏读
                            if (sw.Elapsed.TotalMilliseconds > m_ATbleTimeout) break;
                        }

                        MatchCollection matches = Regex.Matches(strScanResponse, @"(?<=^\[\dth\]).+," + m_bluetoothPrefix + ".*,", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        foreach (Match match in matches)
                        {
                            string[] bleInfo = match.Value.Split(',');
                            lstBluetooth.Add(new BluetoothInformation { BluetoothMAC = bleInfo[0], BluetoothName = bleInfo[1], IsUsed = (bleInfo[0] == usedBluetoothMac) });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //throw;
            }
            finally
            {
                ClosePort();
            }

            return lstBluetooth;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string strReceive = string.Empty;
            if (com != null)
            {
                int n = com.BytesToRead;
                byte[] byteReceive = new byte[n];
                com.Read(byteReceive, 0, n);

                strReceive = Encoding.UTF8.GetString(byteReceive);
                Console.WriteLine("接收到串口数据: " + strReceive);
            }
        }

        #region 具体操作入口
        public string InputPatient(string patientInfo, bool keepConnection = false)
        {
            //AT+PATIENT=201905090105,F,张晓红,67,Y,16,,
            //应答：@OK\r\n

            string cmdString = string.Concat("PATIENT=", patientInfo, "\r\n").ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, true, false, keepConnection);

            return strResponse.TrimEndNewlines();
        }

        public string OutputPatient()
        {
            //AT + PATIENT ?\r\n
            //应答：PATIENT = 201905090105,F,张晓红,67,Y,16,,\r\n

            Func<string, string> funValidate = (response) =>
            {
                Match match = Regex.Match(response, @"(?<=PATIENT\s?=).*", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Value.TrimEndNewlines();
                else
                    return string.Empty;
            };
            string cmdString = "PATIENT?\r\n".ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, false, retryTimes: 3, validatedFun: funValidate);
            //string[] arr = strResponse.Split('=');
            //if (arr.Length == 2)
            //    return arr[1].TrimEndNewlines();
            //else
            //    return string.Empty;

            return strResponse;
        }

        public string SetSN(string sn, bool keepConnection = false)
        {
            //AT+SET_SN=XXYY001\r\n
            //应答：@OK\r\n

            string cmdString = string.Concat("SET_SN=", sn, "\r\n").ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, true, true, keepConnection);

            return strResponse.TrimEndNewlines();
        }

        public string GetSN(bool keepConnection = false)
        {
            //AT+SN=?\r\n
            //应答: SN=XXX\r\n

            Func<string, string> funValidate = (response) =>
            {
                Match match = Regex.Match(response, @"(?<=SN\s?=)\s?.*", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string sn = match.Value.TrimEndNewlines();
                    return (sn == "" ? " " : sn); //SN初始值可能为空
                }
                else
                    return string.Empty;
            };
            string cmdString = "SN=?\r\n".ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, false, false, keepConnection, retryTimes: 3, validatedFun: funValidate);

            return strResponse.Trim();
        }

        public int GetRecordCount(bool keepConnection = false)
        {
            //AT + RECORD_CNT ? \r\n
            //应答: RECORD_CNT = 48\r\n

            Func<string, string> funValidate = (response) =>
            {
                Match match = Regex.Match(response, @"(?<=RECORD_CNT\s?=)\s?\d+", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Value.TrimEndNewlines();
                else
                    return string.Empty;
            };
            string cmdString = "RECORD_CNT?\r\n".ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, false, false, keepConnection, retryTimes: 3, validatedFun: funValidate);
            //string[] arr = strResponse.Split('=');
            //if (arr.Length == 2)
            //    recordCount = Convert.ToInt32(arr[1].TrimEndNewlines());
            //else
            //    recordCount = 0;

            if (!string.IsNullOrEmpty(strResponse))
                m_recordCount = Convert.ToInt32(strResponse);
            else
                m_recordCount = 0;

            return m_recordCount;
        }

        public string ReadRecord(int index)
        {
            //读指定一条记录，从0开始，0表示时间最近的一条记录。
            //AT + RECORD = 0 ? \r\n
            //应答: RECORD = 2019-05-09 11:19:04 74 155 66 0 0\r\n

            //写入一条数据指令:
            //AT +SIM_RECORD=S0;A0;C00;M00;P141088106;R061;T0037;;bb

            Func<string, string> funValidate = (response) =>
            {
                Match match = Regex.Match(response, @"(?<=RECORD\s?=).*", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Value.TrimEndNewlines();
                else
                    return string.Empty;
            };
            string cmdString = $"RECORD={index}?\r\n".ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, false, false, true, retryTimes: 3, validatedFun: funValidate);
            //string[] arr = strResponse.Split('=');
            //if (arr.Length == 2)
            //    return arr[1].TrimEndNewlines();
            //else
            //    return string.Empty;

            return strResponse;
        }

        public string SetTimeSpan(string spanInfo, bool keepConnection = false)
        {
            //序号, 类型,开始,结束,间隔
            //AT+SET_SPAN = 0,0,07:30,22:30,10\r\n
            //应答：@OK\r\n

            int timeout = 3000; //写入配置信息需要消耗的时间较长, 故指定3秒+
            if (m_ConnectByBluetooth) timeout = 5000;

            string cmdString = string.Concat("SET_SPAN=", spanInfo, "\r\n").ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, true, true, keepConnection, timeout);

            //Task task = new Task(() =>
            //{
            //    ABPMOperator opNew = new ABPMOperator(com);
            //    string s = opNew.GetTimeSpan(0);
            //});
            //task.RunSynchronously();

            return strResponse.TrimEndNewlines();
        }

        public string GetTimeSpan(int index)
        {
            //AT+GET_SPAN = 0?\r\n
            //应答：SPAN=0,0,07:30,22:30,10\r\n

            string cmdString = $"GET_SPAN={index}?\r\n".ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, false, false, true);

            Match match = Regex.Match(strResponse, @"(?<=SPAN\s?=).*", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Value.TrimEndNewlines();
            else
                return string.Empty;
        }

        public string SetParameters(string parameters, bool keepConnection = false)
        {
            //键盘, 显示, 白天报警, 夜间报警, 单位 (0表示停用)
            //AT+SET_PARAM=2019-07-22 09:38:10,1,1,1,1,0,280,140,90,90,60\r\n
            //应答：@OK\r\n

            int timeout = 3000; //写入配置信息需要消耗的时间较长, 故指定3秒+
            if (m_ConnectByBluetooth) timeout = 5000;

            string cmdString = string.Concat("SET_PARAM=", parameters, "\r\n").ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, true, true, keepConnection, timeout);

            return strResponse.TrimEndNewlines();
        }

        public string GetParameters(bool keepConnection = false)
        {
            //AT+GET_PARAM = ?\r\n
            //应答：PARAM=1,0,0,0,0\r\n

            string cmdString = "GET_PARAM=?\r\n".ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, false, false, keepConnection);

            Match match = Regex.Match(strResponse, @"(?<=PARAM\s?=).*", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Value.TrimEndNewlines();
            else
                return string.Empty;
        }

        #region 蓝牙名称设置
        public string GetBluetoothName(bool keepConnection = false)
        {
            string cmdString = ">>AT+MAC\r\n".ToCmdString(m_ConnectByBluetooth);
            string strResponse = sendMessage(cmdString, false, true, keepConnection);

            Match match = Regex.Match(strResponse, @"(?<=^\[DA\])([0-9a-fA-F]{2})(:[0-9a-fA-F]{2}){5}.*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Value.TrimEndNewlines();
            else
                return string.Empty;
        }

        public string ExitCommand(bool keepConnection = false)
        {
            string strResponse = "";
            if (m_isReady == true)
            {
                string cmdString = ">>EXIT\r\n".ToCmdString(m_ConnectByBluetooth);
                strResponse = sendMessage(cmdString, false, false, keepConnection);
            }
            return strResponse;
        }

        public string SetBluetoothName(string newName, bool keepConnection = false)
        {
            Thread.Sleep(200); //在串口打开后需延迟一定时间执行后续动作
            string cmdString = $">>AT+DEV_NAME={newName}\r\n".ToCmdString(m_ConnectByBluetooth);
            string strResponse1 = sendMessage(cmdString, true, false, keepConnection);
            if (strResponse1.TrimEndNewlines().EndsWith("OK"))
            {
                cmdString = ">>AT+SAVE\r\n".ToCmdString(m_ConnectByBluetooth);
                string strResponse2 = sendMessage(cmdString, true, false, keepConnection);
                return strResponse2.TrimEndNewlines();
            }
            else
                return string.Empty;
        }

        public string SetBluetoothLanguageAndLogo(string Order, bool keepConnection = false)
        {
            Thread.Sleep(200); //在串口打开后需延迟一定时间执行后续动作

            string cmdString = "AT+MODEL=" + Order + "\r\n";
            string strResponse1 = sendMessage(cmdString, true, true, keepConnection);
            if (!string.IsNullOrWhiteSpace(strResponse1))
            {
                return strResponse1;
            }
            else
                return string.Empty;
        }

        #endregion

        #endregion

        /// <summary>
        /// 测试串口通信连通性
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="oldway">是否沿用之前的连接通道尝试连接</param>
        /// <param name="oldIsBluetooth">先前的连接方式是否为蓝牙连接</param>
        /// <returns></returns>
        private bool testCommunication(string portName, bool isSpecified, bool isUsedChannel, string remoteMac = "")
        {
            if (string.IsNullOrWhiteSpace(portName)) return false;
            com = new SerialPort(portName) { BaudRate = m_BaudRate, WriteTimeout = m_WriteTimeout };

            bool isOK = false;
            try
            {
                OpenPort();

                if (isSpecified)
                {
                    if ("0" == remoteMac) //直连
                    {
                        isOK = connectDirect();
                    }
                    else //蓝牙连接
                    {
                        isOK = setupBluetoothConnection(remoteMac);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(remoteMac)) //设备直连进行通讯
                    {
                        isOK = connectDirect();
                    }
                    else //用蓝牙连接, 并在失败时尝试直连
                    {
                        //isOK = setupBluetoothConnection(remoteMac);

                        if (!isOK)
                            isOK = connectDirect();
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                ClosePort();
            }

            return isOK;
        }

        private bool setupBluetoothConnection(string remoteMac = "")
        {
            if (string.IsNullOrEmpty(remoteMac)) return false;

            int totalBytes = 0;
            bool succeeded = false;
            bool tried = false, timedOut = false;
            string errMsg = string.Empty;
            string curStatus = string.Empty;

            com.DiscardInBuffer();

            byte[] sendData = Encoding.UTF8.GetBytes("AT+WSTA\r\n");
            com.Write(sendData, 0, sendData.Length);
            curStatus = receiveData_Multiline(ref totalBytes, "[DA]", out errMsg, 500, 200);
            if (string.IsNullOrEmpty(curStatus)) return false;

            Match matchConn = Regex.Match(curStatus, @"^(\[DA\].+,|\[DA\])conn(,|\r\n)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            // 1. 首先尝试直接握手连接, 以测试上一次已经建立的连接是否可用
            if (remoteMac.ToLower() == this.m_CurrentRemoteBleMAC.ToLower() && this.m_ConnectByBluetooth && m_isReady)
            {
                if (matchConn.Success)
                {
                    string strReceived = string.Empty;
                    sendData = Encoding.UTF8.GetBytes("BT+AMBP\r\n");
                    com.Write(sendData, 0, sendData.Length);
                    strReceived = receiveData(out errMsg, 1500);
                    if (strReceived.TrimEndNewlines().ToUpper() == "@OK")
                    {
                        return true;
                    }
                }
            }

            // 2. 若连接处于闲置状态, 直接连接
            Match matchIdle = Regex.Match(curStatus, @"^\[DA\].+,idle(,|\r\n)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (matchIdle.Success)
            {
                succeeded = connectByMac(remoteMac);
                if (succeeded) return true;
                tried = true;
            }

            // 3. 若主从处于连接状态, 则先断开连接, 再尝试用新的MAC连接
            if (matchConn.Success)
            {
                sendData = Encoding.UTF8.GetBytes("AT+DISCON\r\n"); //断开主从连接
                com.Write(sendData, 0, sendData.Length);

                Stopwatch sw = Stopwatch.StartNew();
                while (true)
                {
                    totalBytes = 0;
                    string strDisconnect = receiveData_Multiline(ref totalBytes, "Disconnected=", out errMsg, 500, 500);
                    if (strDisconnect.Contains("Disconnected=")) break;
                    if (sw.Elapsed.TotalMilliseconds > m_ATbleTimeout)
                    {
                        timedOut = true;
                        break;
                    }
                }

                if (!timedOut) succeeded = connectByMac(remoteMac);
                if (succeeded) return true;
                tried = true;
            }

            // 4. 若设备状态异常, 则复位其软件模块, 然后尝试连接
            if (!tried)
            {
                bool reset = resetBleDevice();
                if (reset) succeeded = connectByMac(remoteMac);

                if (succeeded) return true;
            }

            return false;
        }

        private bool connectDirect()
        {
            string cmdString = "AT+AMBP\r\n";
            string strReceived = string.Empty;
            byte[] sendData = Encoding.UTF8.GetBytes(cmdString);
            com.Write(sendData, 0, sendData.Length);
            strReceived = receiveData(out string errMsg);

            if (strReceived.TrimEndNewlines().ToUpper() == "@OK")
            {
                this.m_ConnectByBluetooth = false;
                this.m_CurrentRemoteBleMAC = "";
                return true;
            }
            else
                return false;
        }

        private bool connectByMac(string remoteMac)
        {
            if (string.IsNullOrEmpty(remoteMac)) return false;

            string errMsg = string.Empty;
            byte[] sendData;
            int totalBytes = 0;
            com.DiscardInBuffer();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                if (matchStatus("idle") == true) break;
                if (sw.Elapsed.TotalMilliseconds > m_ATbleTimeout) return false;
            }

            sendData = Encoding.UTF8.GetBytes("AT+CON_MAC=" + remoteMac + "\r\n"); //step 3 ------
            com.Write(sendData, 0, sendData.Length);

            sw.Restart();
            while (true)
            {
                totalBytes = 0;
                string strConnectResponse = receiveData_Multiline(ref totalBytes, "Connected=" + remoteMac + "\r\n", out errMsg, 500, 500);
                if (strConnectResponse.Contains("Connected=" + remoteMac + "\r\n"))
                {
                    com.DiscardInBuffer();
                    //com.DiscardOutBuffer();

                    Stopwatch sw2 = Stopwatch.StartNew();
                    string strHandshake = string.Empty;
                    while (strHandshake.Length <= 0)
                    {
                        Thread.Sleep(50);
                        Application.DoEvents();

                        sendData = Encoding.UTF8.GetBytes("BT+AMBP\r\n"); //step 4 ------
                        com.Write(sendData, 0, sendData.Length);
                        strHandshake = receiveData(out errMsg, 1500);
                        if (strHandshake.TrimEndNewlines().ToUpper() == "@OK")
                        {
                            this.m_ConnectByBluetooth = true;
                            this.m_CurrentRemoteBleMAC = remoteMac;
                            return true;
                        }
                        if (sw2.Elapsed.TotalMilliseconds > m_ATbleTimeout + 500) return false;
                    }

                    if (strHandshake.Length > 0)
                        return false;
                }

                if (sw.Elapsed.TotalMilliseconds > m_ATbleTimeout + 500) return false;
            }
        }

        /// <summary>
        /// 发送数据并同步接收返回结果
        /// </summary>
        /// <param name="message">发送的信息内容</param>
        /// <param name="isWrite">标识是否写入数据到设备</param>
        /// <param name="checkMultiline">标识是否检测一次请求之后返回的结果中的多行记录</param>
        /// <param name="keepConnection">标识串口是否处于打开状态, 避免重复打开串口</param>
        /// <param name="timeOut">获得请求的返回结果在超时之前经历的时间</param>
        /// <param name="doInSilence">是否按静默方式读写数据</param>
        /// <param name="retryTimes">重试次数</param>
        /// <param name="validatedFun">验证是否接收到期望的信息的方法</param>
        /// <returns></returns>
        private string sendMessage(string message, bool isWrite, bool checkMultiline = false, bool keepConnection = false, int timeOut = 0, bool doInSilence = false, int retryTimes = 1, Func<string, string> validatedFun = null)
        {
            string strReceived = string.Empty;
            byte[] sendData;
            int receivedbytes = 0;
            try
            {
                if (!keepConnection) OpenPort();
                if (timeOut <= 0) timeOut = getTimeout(isWrite);

                if (m_ConnectByBluetooth)
                {
                    //检查蓝牙连接是否正常或已变为数据线直连
                    if (matchStatus("conn") == false)
                    {
                        //m_ConnectByBluetooth = false;
                        throw new Exception("蓝牙设备连接已断开");
                    }
                }
                else
                {
                    //sendData = Encoding.UTF8.GetBytes("AT+AMBP\r\n");
                    //com.Write(sendData, 0, sendData.Length);
                    //string curStatus = receiveData(false, "");
                    //if (curStatus.TrimEndNewlines().ToUpper() != "@OK")
                    //    throw new Exception("本地设备连接已中断");
                }

                //发送操作指令
                for (int i = 1; i <= retryTimes; i++)
                {
                    string errMsg = string.Empty;
                    Thread.Sleep(10);

                    com.DiscardInBuffer();
                    sendData = Encoding.UTF8.GetBytes(message);
                    com.Write(sendData, 0, sendData.Length);
                    if (false == checkMultiline)
                        strReceived = receiveData(out errMsg, timeOut);
                    else
                        strReceived = receiveData_Multiline(ref receivedbytes, "@OK\r\n", out errMsg, timeOut, 1500);

                    if (strReceived == "[AT]ER\r\n") throw new Exception("本地血压设备连接已断开");

                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        if (i >= retryTimes && doInSilence == false)
                        {
                            throw new Exception(errMsg);
                        }
                    }
                    else
                    {
                        if (validatedFun != null)
                        {
                            string validatedResult = validatedFun.Invoke(strReceived);
                            if (string.IsNullOrEmpty(validatedResult))
                            {
                                //两种方案判断是否已获取期望的数据(当前为方案1):
                                //1. 重发指令并判断接收到的单行单条消息
                                //2. 持续接收多行消息并判断其中是否包含期望的回应信息
                                if (i >= retryTimes) throw new Exception("数据连接超时");
                            }
                            else
                            {
                                strReceived = validatedResult;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                return strReceived;
            }
            catch (Exception ex)
            {
                throw new Exception("处理数据出错", ex);
            }
            finally
            {
                if (!keepConnection) ClosePort();
            }
        }

        /// <summary>
        /// 获取串口的应答信息(单行)
        /// </summary>
        /// <param name="quiet">若超时是否抛出异常</param>
        /// <param name="timeOut">应答超时时间</param>
        /// <returns>返回的应答信息</returns>
        private string receiveData(out string errMsg, int timeOut = 500)
        {
            errMsg = "";
            string strReceived = string.Empty;
            string strSegment = string.Empty;
            DateTime dtBegin = DateTime.Now;
            Stopwatch sw = new Stopwatch();
            List<byte> lstReceived = new List<byte>(); //保存多次接收到的字节，以避免期间解析汉字组成部分字节时可能出现的乱码

            while (!strSegment.EndsWith("\r\n"))
            {
                bool exceedInnerTimeSpan = false;

                sw.Restart();
                while (com.BytesToRead <= 0)
                {
                    Thread.Sleep(1);
                    Application.DoEvents();
                    if (sw.ElapsedMilliseconds > timeOut) //若在指定的毫秒数之后仍无数据返回，则视为超时(或接收数据完毕)
                    {
                        Console.WriteLine("等待接收数据的时间: " + sw.ElapsedMilliseconds);
                        errMsg = "数据连接超时";
                        exceedInnerTimeSpan = true;
                        break;
                    }
                }
                if (exceedInnerTimeSpan) break;

                //Console.WriteLine(sw.ElapsedMilliseconds);
                do
                {
                    int count = com.BytesToRead;
                    if (count <= 0) break;
                    if (lstReceived.Count > MAX_BYTE_RECEIVED) break;

                    byte[] readBuffer = new byte[count];
                    Application.DoEvents();
                    com.Read(readBuffer, 0, count);
                    strSegment += Encoding.UTF8.GetString(readBuffer);
                    lstReceived.AddRange(readBuffer);
                } while (com.BytesToRead > 0);

                if (DateTime.Now.Subtract(dtBegin).TotalMilliseconds > m_ATbleTimeout && lstReceived.Count <= 0)
                {
                    //Logger.WriteLog("Time Out and Empty/Little chars received");
                    break;
                }
            }

            if (lstReceived.Count > 0)
            {
                strReceived = Encoding.UTF8.GetString(lstReceived.ToArray());
            }

            return strReceived;
        }

        /// <summary>
        /// 按递归方式获取一个请求过程的返回信息(连续接收多行信息时, 按递归方式用指定的时间间隔重置超时时间)
        /// </summary>
        /// <param name="receivedBytes">总共接收到的字节数</param>
        /// <param name="endFlag">指定用于代表已接收完应答信息的标识</param>
        /// <param name="quiet">若超时是否抛出异常</param>
        /// <param name="timeOut">应答超时时间</param>
        /// <param name="timeGap">连续两行应答信息之间的时间间隔</param>
        /// <returns>返回的应答信息</returns>
        private string receiveData_Multiline(ref int receivedBytes, string endFlag, out string errMsg, int timeOut, int timeGap)
        {
            errMsg = "";
            string strReceived = string.Empty;
            string strSegment = string.Empty;
            DateTime dtBegin = DateTime.Now;
            Stopwatch sw = new Stopwatch();
            List<byte> lstReceived = new List<byte>(); //保存多次接收到的字节，以避免期间解析汉字组成部分字节时可能出现的乱码

            while (!strSegment.EndsWith("\r\n"))
            {
                bool exceedInnerTimeSpan = false;

                sw.Restart();
                while (com.BytesToRead <= 0)
                {
                    Thread.Sleep(1);
                    Application.DoEvents();
                    if (sw.ElapsedMilliseconds > timeOut) //若在指定的毫秒数之后仍无数据返回，则视为超时(或接收数据完毕)
                    {
                        Console.WriteLine("等待接收数据的时间: " + sw.ElapsedMilliseconds);
                        errMsg = "数据连接超时";
                        exceedInnerTimeSpan = true;
                        break;
                    }
                }
                if (exceedInnerTimeSpan) break;

                do
                {
                    int count = com.BytesToRead;
                    if (count <= 0) break;
                    if (lstReceived.Count > MAX_BYTE_RECEIVED) break;

                    byte[] readBuffer = new byte[count];
                    Application.DoEvents();
                    com.Read(readBuffer, 0, count);
                    strSegment += Encoding.UTF8.GetString(readBuffer);
                    lstReceived.AddRange(readBuffer);
                } while (com.BytesToRead > 0);

                if (DateTime.Now.Subtract(dtBegin).TotalMilliseconds > m_ATbleTimeout && lstReceived.Count <= 0)
                {
                    //Logger.WriteLog("Time Out and Empty/Little chars received");
                    break;
                }
            }

            if (lstReceived.Count > 0)
            {
                strReceived = Encoding.UTF8.GetString(lstReceived.ToArray());
                receivedBytes += lstReceived.Count;

                if (endFlag != "" && strReceived.Contains(endFlag))
                {
                    return strReceived;
                }

                //继续接收同一请求过程中一并返回的额外数据
                if (receivedBytes < MAX_BYTE_RECEIVED * 3)
                {
                    //读取多行时, 递归调用的参数quiet置为true, 继续接收间隔时间在 timeGap以内的数据
                    string nextLine = receiveData_Multiline(ref receivedBytes, endFlag, out string subErrMsg, timeGap, timeGap);
                    strReceived += nextLine;
                }
            }

            return strReceived;
        }

        private int getTimeout(bool isWrite, bool bluetooth = false)
        {
            if (isWrite)
            {
                if (bluetooth || m_ConnectByBluetooth)
                    return 2000 * 2;
                else
                    return 2000;
            }
            else
            {
                int duration = 2000;
                if (bluetooth || m_ConnectByBluetooth)
                    return duration * 2;
                else
                    return duration;
            }
        }

        private ConnectionInformation getConnectionInfo()
        {
            string portName = string.Empty, connType = string.Empty, remoteMac = string.Empty;
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "ABPM_Connect.xml";

            if (!File.Exists(filePath)) return null;

            //using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
            //{
            //    portName = sr.ReadLine();   //本地串口名称
            //    connType = sr.ReadLine();   //连接血压计的方式(直连/蓝牙)
            //    remoteMac = sr.ReadLine();  //血压计内蓝牙模块的MAC地址
            //    sr.Close();
            //}
            //return new string[] { portName ?? "", connType ?? "", remoteMac ?? "" };

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            XmlNode blePrefix = xmlDoc.GetElementsByTagName("BluetoothPrefix")[0];
            XmlNode port = xmlDoc.GetElementsByTagName("UsedSerialPort")[0];
            XmlNode channel = xmlDoc.GetElementsByTagName("UsedChannel")[0];
            XmlNode mac = xmlDoc.GetElementsByTagName("UsedBluetoothMAC")[0];

            this.m_bluetoothPrefix = blePrefix.InnerText;
            return new ConnectionInformation
            {
                BluetoothPrefix = blePrefix.InnerText,
                UsedSerialPort = port.InnerText,
                UsedChannel = channel.InnerText,
                UsedBluetoothMAC = mac.InnerText
            };
        }

        private void saveConnectionInfo()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "ABPM_Connect.xml";
            //using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            //{
            //    fs.Seek(0, SeekOrigin.Begin);
            //    fs.SetLength(0);

            //    StreamWriter sw = new StreamWriter(fs);
            //    sw.WriteLine(com.PortName);
            //    sw.WriteLine(m_ConnectByBluetooth ? "bluetooth" : "local_device");
            //    sw.WriteLine(m_ConnectByBluetooth ? this.m_CurrentRemoteBleMAC : "");
            //    sw.Flush();
            //    sw.Close();
            //}

            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(filePath))
            {
                //更新xml
                xmlDoc.Load(filePath);

                //xmlDoc.SelectSingleNode("UsedConnection/UsedSerialPort");
                XmlNode port = xmlDoc.GetElementsByTagName("UsedSerialPort")[0];
                port.InnerText = com.PortName;

                XmlNode channel = xmlDoc.GetElementsByTagName("UsedChannel")[0];
                channel.InnerText = m_ConnectByBluetooth ? "bluetooth" : "local_device";

                XmlNode mac = xmlDoc.GetElementsByTagName("UsedBluetoothMAC")[0];
                mac.InnerText = m_ConnectByBluetooth ? this.m_CurrentRemoteBleMAC : "";

                xmlDoc.Save(filePath);
            }
            else
            {
                //新增xml
                XmlNode header = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(header);

                XmlElement root = xmlDoc.CreateElement("Connection");

                XmlElement blePrefix = xmlDoc.CreateElement("BluetoothPrefix");
                blePrefix.InnerText = this.m_bluetoothPrefix;
                root.AppendChild(blePrefix);

                XmlElement usedConnection = xmlDoc.CreateElement("UsedConnection");

                XmlElement port = xmlDoc.CreateElement("UsedSerialPort");
                port.InnerText = com.PortName;

                XmlElement channel = xmlDoc.CreateElement("UsedChannel");
                channel.InnerText = m_ConnectByBluetooth ? "bluetooth" : "local_device";

                XmlElement mac = xmlDoc.CreateElement("UsedBluetoothMAC");
                mac.InnerText = m_ConnectByBluetooth ? this.m_CurrentRemoteBleMAC : "";

                usedConnection.AppendChild(port);
                usedConnection.AppendChild(channel);
                usedConnection.AppendChild(mac);
                root.AppendChild(usedConnection);
                xmlDoc.AppendChild(root);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "   ";
                settings.Encoding = Encoding.UTF8;
                using (XmlWriter xmlWriter = XmlWriter.Create(filePath, settings))
                {
                    xmlDoc.Save(xmlWriter);
                }
            }
        }

        private bool matchStatus(string key)
        {
            byte[] sendData = Encoding.UTF8.GetBytes("AT+WSTA\r\n");
            com.Write(sendData, 0, sendData.Length);

            int receivedBytes = 0;
            string curStatus = receiveData_Multiline(ref receivedBytes, "[DA]", out string errMsg, 500, 200);

            //Match match = Regex.Match(curStatus, @"^(\[DA\].+,|\[DA\])" + key + "(,|\r\n)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            //"adv,idle", "adv,conn", "adv,conning", "adv,scan", "idle,idle"
            Match match = Regex.Match(curStatus, @"^\[DA\].+," + key + "(,|\r\n)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return match.Success;
        }

        private bool resetBleDevice()
        {
            Stopwatch sw = new Stopwatch();
            string errMsg = string.Empty;
            bool timedOut = false;
            int receivedBytes = 0;
            byte[] sendData = Encoding.UTF8.GetBytes("AT+RESET\r\n"); //复位软件模块
            com.Write(sendData, 0, sendData.Length);

            sw.Start();
            while (true)
            {
                //等待重置结束再进行下一步操作
                receivedBytes = 0;
                string strReset = receiveData_Multiline(ref receivedBytes, "Start BLE-UART", out errMsg, 500, 500);
                if (strReset.Contains("Start BLE-UART")) break;
                if (sw.Elapsed.TotalMilliseconds > m_ATbleTimeout)
                {
                    timedOut = true;
                    break;
                }
            }

            return !timedOut;
        }
    }

}
