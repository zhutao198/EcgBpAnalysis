using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Zoncare.Common;

namespace ABPMRepository
{
    public class ABPMOperatorBle
    {
        private string _remoteBleMAC = string.Empty; //通过蓝牙连接的血压计设备内蓝牙元器件的MAC地址
        private SerialPort _comm;
        private List<byte> _sourceData;
        private List<KeyValuePair<string, string>> _bleReply;
        private List<DataPacket> _dataReply;
        private string _currentCommand; //RD,ST,ADV_DATA,FILEINFO,FILEDATA
        private AutoResetEvent _are = new AutoResetEvent(true);
        private CancellationTokenSource _cts;
        private static object _connLock = new object();
        private static object _lockObj = new object();

        private readonly string[] CommandHeads = { "<rd_", "<st_", "<ADV_DATA=", "FILEINFO=", "FILEDATA=", "FILEWRITE=", "FILECLR=", "DEVMODEL=", "SYNCTIME=" };
        private readonly byte[] dataHead = new byte[] { 0xAA, 0xAA, 0xAA, 0xAA };
        private readonly byte[] dataTail = new byte[] { 0xBB, 0xBB, 0xBB, 0xBB };
        private readonly string[] bleReply = { "rd_ble_mac", "st_scan_device_on", "rd_bond_client_mac", "st_bond_client_mac", "ADV_DATA" };
        private readonly string[] zqReply = { "FILEINFO", "FILEWRITE", "FILECLR", "DEVMODEL", "SYNCTIME" };

        private const int MAX_BYTE_RECEIVED = 1024;
        private const int BAUD_RATE = 1000000;
        private const int COMM_TIMEOUT = 1000;

        public bool IsConnectToBle { get; set; } = false;
        public bool IsReady { get; set; } //在串口打开的前提下, 连接通信是否正常
        public string BluetoothPrefix { get; set; } = "ZQBP";

        public ABPMOperatorBle()
        {
            //m_SyncRoot = this;

            init();
        }

        public ABPMOperatorBle(SerialPort sourceCom)
        {
            this._comm = sourceCom;
        }

        public void OpenPort(SerialPort com)
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

        public void ClosePort(SerialPort com = null)
        {
            _cts?.Cancel();
            if (com == null)
            {
                com = _comm;
            }

            if (com != null && com.IsOpen)
            {
                com.Close();
                com.Dispose();
            }
        }

        private void init()
        {
            _sourceData = new List<byte>();
            _bleReply = new List<KeyValuePair<string, string>>();
            _dataReply = new List<DataPacket>();

            string query = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3";
            ManagementEventWatcher mgtEventWatcher = new ManagementEventWatcher(query);
            mgtEventWatcher.EventArrived += new EventArrivedEventHandler(Win32Device_Changed);
            mgtEventWatcher.Start();
        }

        /// <summary>
        /// 设置通讯指令
        /// </summary>
        /// <param name="command">读取或获取数据指令: RD,ST,ADV_DATA,FILEINFO,FILEDATA等</param>
        public void SetFunction(string command)
        {
            //_sourceData.Clear();
            _bleReply.Clear();
            _dataReply.Clear();
            _currentCommand = command;
        }

        private void Win32Device_Changed(object sender, EventArrivedEventArgs e)
        {
            IsReady = false;
        }

        /// <summary>
        /// 尝试与主机设备进行串口通信
        /// </summary>
        /// <param name="errorCode">错误代码(400:通信失败; 401:直连失败; 402:蓝牙连接失败)</param>
        /// <param name="retryTimes">尝试连接串口的次数</param>
        /// <returns></returns>
        public async Task<string> TryConnectDevice(int retryTimes = 3)
        {
            string usedPortName = string.Empty;
            string remoteBleMac = string.Empty;

            string[] serialPorts = SerialPort.GetPortNames();
            if (serialPorts == null)
            {
                IsReady = false;
                return "401"; //"没有检测到本机通信串口！";
            }

            ConnectionInformation connInfo = getConnectionInfo();
            if (connInfo != null)
            {
                usedPortName = connInfo.UsedSerialPort;
                remoteBleMac = connInfo.UsedBluetoothMAC;
            }

            var checkPortList = serialPorts.OrderByDescending(o => o).AsEnumerable();
            for (int i = 0; i < retryTimes; i++)
            {
                if (serialPorts.Contains(usedPortName, new StringIgnoreCaseComparer()))
                {
                    _comm = await testCommunication(usedPortName);
                    if (_comm != null)
                    {
                        IsReady = true;
                        saveConnectionInfo();
                        return "";
                    }
                }

                if (!string.IsNullOrEmpty(usedPortName)) checkPortList = checkPortList.Where(o => string.Compare(o, usedPortName, true) != 0);
                foreach (string portName in checkPortList)
                {
                    _comm = await testCommunication(portName);
                    if (_comm != null)
                    {
                        IsReady = true;
                        saveConnectionInfo();
                        return "";
                    }
                }
            }

            return "402";  //串口通信失败
        }


        #region 具体操作入口
        //获取蓝牙列表
        public async Task<List<string>> GetBleDeviceList()
        {
            List<string> lstDevice = new List<string>();
            sendData(_comm, "<ST_SCAN_DEVICE_ON=1>");
            string scanEnabledResult = await getReplyResult("st_scan_device_on", COMM_TIMEOUT);
            if (scanEnabledResult?.ToLower() == "ok")
            {
                SetFunction("<ADV_DATA=");
                List<string> devices = await getReplyList("ADV_DATA", 3000);
                devices.ForEach(t =>
                {
                    string mac = t.Split(',')[0]; //MAC地址 + 扫描到的广播数据 + RSSI值
                    lstDevice.Add(mac);
                });
            }
            _are.Set();

            sendData(_comm, "<ST_SCAN_DEVICE_ON=0>");
            _are.Set();

            return lstDevice;
        }

        //绑定蓝牙设备
        public async Task<int> BoundDevice(string mac)
        {
            int result = 0;

            if (await UnBoundDevice() == 1)
            {
                await Task.Delay(100);
                sendData(_comm, $"<ST_BOND_CLIENT_MAC={mac}>");
                string bondResult = await getReplyResult("st_bond_client_mac", 2000, mac);
                if (bondResult == mac)
                {
                    result = 1;
                }
            }
            _are.Set();

            return result;
        }

        //检查当前绑定的蓝牙设备MAC地址
        public async Task<int> CheckBondMac(string mac)
        {
            int result = 0;
            sendData(_comm, "<RD_BOND_CLIENT_MAC>");
            string bondResult = await getReplyResult("rd_bond_client_mac", COMM_TIMEOUT, mac);
            if (bondResult == mac)
            {
                result = 1;
            }
            _are.Set();

            return result;
        }


        //取消蓝牙设备绑定
        public async Task<int> UnBoundDevice()
        {
            int result = 0;
            sendData(_comm, "<ST_BOND_CLIENT_MAC=0>");
            string bondResult = await getReplyResult("st_bond_client_mac", COMM_TIMEOUT);
            if (bondResult?.ToLower() == "ok")
            {
                result = 1;
            }
            _are.Set();

            return result;
        }

        //获取设备型号
        public async Task<string> GetDeviceModel()
        {
            sendData(_comm, $"DEVMODEL=?\r\n");
            string devModel = await getReplyResult("DEVMODEL", 1500);
            _are.Set();

            return devModel;
        }

        //同步系统时间
        public async Task<int> SyncSystemTime(string timeString)
        {
            int result = 0;
            sendData(_comm, $"SYNCTIME={timeString}" + "\r\n");
            string syncResult = await getReplyResult("SYNCTIME", 1500);
            if (syncResult?.ToLower() == "ok")
            {
                result = 1;
            }
            _are.Set();

            return result;
        }

        //获取设备内文件信息
        public async Task<string> GetFileInfo(string fileName)
        {
            sendData(_comm, $"FILEINFO={fileName}" + "\r\n");
            string fileInfo = await getReplyResult("FILEINFO", 1500);
            _are.Set();

            return fileInfo;
        }

        //写入文件内容(患者信息及戴机参数)
        public async Task<int> SetFileInfo(string fileName, string fileText)
        {
            int result = 0;
            byte[] fileBytes = Encoding.ASCII.GetBytes(fileText);

            List<byte> content = new List<byte>();
            content.AddRange(new byte[] { 0xAA, 0xAA, 0xAA, 0xAA });
            content.Add(1); //文件代码(固定为1)

            //长度
            byte[] lenBytes = BitConverter.GetBytes((ushort)fileBytes.Length);
            if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
            content.AddRange(lenBytes);

            //内容
            content.AddRange(fileBytes);

            //CRC
            byte[] crcData = new byte[fileBytes.Length + 3];
            crcData[0] = content[4];
            crcData[1] = content[5];
            crcData[2] = content[6];
            Buffer.BlockCopy(fileBytes, 0, crcData, 3, fileBytes.Length);

            ushort crc16 = ZqBPFuns.ComputeCRC16_CCITT(crcData);
            byte[] crcBytes = BitConverter.GetBytes(crc16);
            if (BitConverter.IsLittleEndian) Array.Reverse(crcBytes);
            content.AddRange(crcBytes);

            content.AddRange(new byte[] { 0xBB, 0xBB, 0xBB, 0xBB });

            Console.WriteLine($"写入戴机参数: {fileText}");
            sendData(_comm, "FILEWRITE=", content.ToArray()); //写入数据

            string writeResponseInfo = await getReplyResult("FILEWRITE", 5000);
            if (Regex.IsMatch(writeResponseInfo, @"^OK,1,\d+", RegexOptions.IgnoreCase))
            {
                result = 1;
            }
            _are.Set();

            return result;
        }

        //清除数据
        public async Task<int> ClearData()
        {
            int result = 0;
            sendData(_comm, "FILECLR=ALLDAT" + "\r\n");
            string clsResult = await getReplyResult("FILECLR", 1500);
            if (Regex.IsMatch(clsResult, @"^ALLDAT,OK", RegexOptions.IgnoreCase))
            {
                result = 1;
            }
            _are.Set();
            return result;
        }

        //读取文件内容
        public async Task<byte[]> GetFileData(ZqFileInfo fi, int start, int len = 1024000)
        {
            byte[] data = null;

            sendData(_comm, $"FILEDATA={fi.FileName},{start},{1024000}" + "\r\n");
            data = await getReplyData("FILEDATA", 30000, fi);

            //_comm.DiscardInBuffer();
            _are.Set();

            return data;
        }
        #endregion

        /// <summary>
        /// 测试串口通信连通性
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="oldway">是否沿用之前的连接通道尝试连接</param>
        /// <param name="oldIsBluetooth">先前的连接方式是否为蓝牙连接</param>
        /// <returns></returns>
        private async Task<SerialPort> testCommunication(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName)) return null;

            AutoResetEvent processEvent = new AutoResetEvent(false);
            SerialPort ret = null;
            SerialPort com = new SerialPort(portName) { BaudRate = BAUD_RATE, WriteTimeout = 800, ReadBufferSize = 1024000 };
            _sourceData.Clear();

            com.DataReceived += new SerialDataReceivedEventHandler((sender, e) =>
            {
                if (!com.IsOpen) return;

                try
                {
                    int cnt = com.BytesToRead;
                    if (cnt > 0)
                    {
                        byte[] byteReceive = new byte[cnt];
                        com.Read(byteReceive, 0, cnt);
                        //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 接收到串口数据: " + byteReceive.ToHexStr(" "));

                        if (!string.IsNullOrEmpty(_currentCommand))
                        {
                            _sourceData.AddRange(byteReceive);
                        }
                        processEvent.Set();

                        if (cnt > com.ReadBufferSize) { Console.WriteLine($"接收到的数据长度超出缓冲区大小:{com.ReadBufferSize}"); }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("串口通讯发生错误: " + ex.Message + Environment.NewLine + ex.StackTrace);
                    IsReady = false;
                }
            });

            #region 不使用事件驱动
            //Action recvDataProcess = () =>
            //{
            //    lock (_connLock)
            //    {
            //        byte[] buffer = new byte[1024];
            //        while (!_cts.IsCancellationRequested)
            //        {
            //            if (com != null && com.IsOpen)
            //            {
            //                try
            //                {
            //                    int bytesRead = com.Read(buffer, 0, buffer.Length);
            //                    if (bytesRead > 0)
            //                    {
            //                        byte[] byteReceive = new byte[bytesRead];
            //                        Buffer.BlockCopy(buffer, 0, byteReceive, 0, bytesRead);
            //                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 接收到串口数据: " + byteReceive.ToHexStr(" "));
            //                        if (!string.IsNullOrEmpty(_currentCommand)) _sourceData.AddRange(byteReceive);
            //                        if (_sourceData.Count >= 4)
            //                        {
            //                            try
            //                            {
            //                                processData();
            //                            }
            //                            catch (Exception ex)
            //                            {
            //                                _bleReply.Clear();
            //                                _dataReply.Clear();
            //                            }
            //                        }
            //                    }
            //                }
            //                catch (IOException ex) when (!_cts.IsCancellationRequested)
            //                {
            //                    // 非取消导致的IO异常
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //};
            #endregion

            Action recvDataProcess = () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    processEvent.WaitOne();
                    if (_sourceData.Count >= 4)
                    {
                        try
                        {
                            processData();
                        }
                        catch (Exception ex)
                        {
                            _bleReply.Clear();
                            _dataReply.Clear();
                        }
                    }
                }
            };

            _cts = new CancellationTokenSource();
            bool connIsOK = false;
            try
            {
                OpenPort(com);

                sendData(com, "<RD_BLE_MAC>");

                Task.Run(recvDataProcess, _cts.Token);

                await ZqBPFuns.AsyncDoWork((state) =>
                {
                    while (!state.IsCancellationRequested && !connIsOK)
                    {
                        if (_bleReply.Any(o => o.Key == "RD_BLE_MAC"))
                        {
                            connIsOK = true;
                        }
                        Thread.Sleep(1);
                    }
                }, COMM_TIMEOUT);

                if (connIsOK) { ret = com; }
            }
            catch (UnauthorizedAccessException ex)
            {
                //return false;
            }
            catch (Exception ex)
            {
                //return false;
            }
            finally
            {
                _are.Set();
                if (!connIsOK)
                {
                    ClosePort(com);
                }
            }

            return ret;
        }

        private void processData()
        {
            if (string.IsNullOrEmpty(_currentCommand)) return;
            if (_sourceData.Count <= 0) return;

            int holdIndex = 0; //保留(数据索引)以标识剩余待处理消息
            lock (_lockObj)
            {
                //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " data process start......");
                string wrapedHead = CommandHeads.FirstOrDefault(s => _currentCommand.StartsWith(s.ToUpper()));

                //消息头
                byte[] startBytes = null;
                if (_currentCommand.StartsWith("FILEDATA") == false)
                    startBytes = Encoding.ASCII.GetBytes(wrapedHead);
                else
                    startBytes = dataHead;

                //消息尾
                byte[] endBytes = new byte[] { 0x3E }; //'>';
                if (_currentCommand.StartsWith("FILEINFO") || _currentCommand.StartsWith("FILEWRITE") || _currentCommand.StartsWith("FILECLR")
                    || _currentCommand.StartsWith("DEVMODEL") || _currentCommand.StartsWith("SYNCTIME"))
                    endBytes = new byte[] { 13 }; //'\r'
                else if (_currentCommand.StartsWith("FILEDATA"))
                    endBytes = dataTail;

                bool headFound = false;
                for (int i = 0; i <= _sourceData.Count - startBytes.Length; i++)
                {
                    //if (!_comm.IsOpen) break;

                    bool headMatched = true;
                    for (int n = 0; n < startBytes.Length; n++)
                    {
                        if (_sourceData[i + n] != startBytes[n])
                        {
                            headMatched = false;
                            break;
                        }
                    }
                    if (headMatched)
                    {
                        holdIndex = i;
                        headFound = true;

                        int tailSearchPos = i + startBytes.Length; //尾部标识的开始位置
                        if (_currentCommand.StartsWith("FILEDATA"))
                        {
                            //AA AA AA AA + 起始扇区(2字节) + 长度(2字节) + 数据块(N字节) + CRC16(2字节) + BB BB BB BB
                            if (_sourceData.Count < tailSearchPos + 4) break;
                            byte[] lenBytes = new byte[] { _sourceData[tailSearchPos + 2], _sourceData[tailSearchPos + 3] };
                            ushort dataLen = BitConverter.ToUInt16(ZqBPFuns.GetBySystemEndian(lenBytes), 0);
                            tailSearchPos = tailSearchPos + 4 + dataLen + 2;
                        }
                        if (_sourceData.Count <= tailSearchPos) break;

                        for (int j = tailSearchPos; j <= _sourceData.Count - endBytes.Length; j++)
                        {
                            bool tailMatched = true;
                            for (int k = 0; k < endBytes.Length; k++)
                            {
                                if (_sourceData[j + k] != endBytes[k])
                                {
                                    tailMatched = false;
                                    break;
                                }
                            }
                            if (tailMatched)
                            {
                                holdIndex = j + endBytes.Length;

                                byte[] resBytes = null;
                                int offset = 1;
                                if (_currentCommand.StartsWith("FILEINFO") || _currentCommand.StartsWith("FILEWRITE")
                                    || _currentCommand.StartsWith("FILEDATA") || _currentCommand.StartsWith("FILECLR")
                                    || _currentCommand.StartsWith("DEVMODEL") || _currentCommand.StartsWith("SYNCTIME"))
                                {
                                    resBytes = new byte[j - i]; //去掉尾部标识
                                    offset = i;
                                }
                                else
                                {
                                    resBytes = new byte[j - i - 1]; //去掉"<"与">"
                                    offset = i + 1;
                                }
                                _sourceData.CopyTo(offset, resBytes, 0, resBytes.Length);

                                dealWithData(resBytes);
                                break;
                            }
                        }
                    }

                    if (holdIndex > 0) { break; } //找到了消息头并已处理相关数据(找到头和尾)
                }
                //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " data process end......");

                if (_sourceData.Count > 10 && !headFound)
                {
                    //蓝牙扫描时不清除剩余的数据以免"<st_scan_device_on=ok>"后粘黏的"<ADV_DATA=xxx"被清除(一种请求指令, 两种应答头)
                    if (_currentCommand != "<ST_SCAN_DEVICE_ON=1>" && _currentCommand != "<ADV_DATA=") _sourceData.Clear();
                }
                if (holdIndex > 0)
                {
                    _sourceData.RemoveRange(0, holdIndex);
                    processData();
                }
            }
        }

        //处理各指令对应的响应消息
        private void dealWithData(byte[] content)
        {
            string replyText = Encoding.ASCII.GetString(content);

            try
            {
                if (zqReply.Any(s => _currentCommand.StartsWith(s) && replyText.StartsWith(s, StringComparison.InvariantCultureIgnoreCase)))
                {
                    string[] arr = replyText.Split('=');
                    string cmd = arr[0].ToUpper();
                    _bleReply.Add(new KeyValuePair<string, string>(cmd, arr[1]));
                }
                else if (_currentCommand.StartsWith("FILEDATA"))
                {
                    int len = content.Length;
                    if (content[0] == 0xAA && content[1] == 0xAA && content[2] == 0xAA && content[3] == 0xAA)
                    {
                        byte[] idxBytes = new byte[] { content[4], content[5] };
                        ushort idx = BitConverter.ToUInt16(ZqBPFuns.GetBySystemEndian(idxBytes), 0);

                        byte[] lenBytes = new byte[] { content[6], content[7] };
                        ushort dataLen = BitConverter.ToUInt16(ZqBPFuns.GetBySystemEndian(lenBytes), 0);
                        byte[] arrData = new byte[dataLen];
                        Buffer.BlockCopy(content, 8, arrData, 0, arrData.Length);

                        //CRC16校验
                        byte[] crcData = new byte[dataLen + 4];
                        Buffer.BlockCopy(content, 4, crcData, 0, crcData.Length);
                        ushort crc16 = ZqBPFuns.ComputeCRC16_CCITT(crcData);
                        byte[] crcBytes = new byte[] { content[len - 2], content[len - 1] };
                        if (BitConverter.ToUInt16(ZqBPFuns.GetBySystemEndian(crcBytes), 0) == crc16)
                        {
                            _dataReply.Add(new DataPacket()
                            {
                                Key = "FILEDATA",
                                SectorIndex = idx,
                                ContentLength = dataLen,
                                Content = arrData,
                                CRC = crc16
                            });
                            //Console.WriteLine($"序号:{idx}, CRC:{crc16}");
                        }
                        else { }
                    }
                }
                else
                {
                    if (bleReply.Any(s => replyText.Contains(s)))
                    {
                        string[] arr = replyText.Split('=');
                        string cmd = arr[0].ToUpper();
                        _bleReply.Add(new KeyValuePair<string, string>(cmd, arr[1]));
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        //发送指令数据
        private void sendData(SerialPort comm, string cmdText, byte[] content = null)
        {
            _are.WaitOne();

            if (comm != null && comm.IsOpen)
            {
                lock (_lockObj)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} 发送指令: {cmdText}");
                    if (content != null)
                    {
                        string contentText = Encoding.ASCII.GetString(content);
                        Logger.CreateLog($"发送指令:{cmdText}; 字节数:{content.Length}; 内容:{contentText}", "串口通讯");
                    }

                    SetFunction(cmdText.TrimEndNewlines());
                    comm.DiscardOutBuffer();
                    //comm.DiscardInBuffer();
                    if (content == null)
                    {
                        byte[] sendData = Encoding.ASCII.GetBytes(cmdText);
                        comm.Write(sendData, 0, sendData.Length);
                    }
                    else
                    {
                        comm.Write(content, 0, content.Length);

                        ////每个数据包最多256个字节
                        //int offset = 0;
                        //for (int i = 0; i < content.Length; i += 256)
                        //{
                        //    byte[] packetData = new byte[256];
                        //    Buffer.BlockCopy(content, i, packetData, 0, packetData.Length);
                        //    comm.Write(packetData, 0, packetData.Length);
                        //    offset = i;
                        //}
                        //if (offset < content.Length)
                        //{
                        //    int restCnt = content.Length - offset;
                        //    byte[] packetData = new byte[restCnt];
                        //    Buffer.BlockCopy(content, offset, packetData, 0, packetData.Length);
                        //    comm.Write(packetData, 0, packetData.Length);
                        //}
                    }
                }
            }
        }

        //获取指定响应头的内容
        private async Task<string> getReplyResult(string key, int duration, string value = "")
        {
            string result = string.Empty;
            await ZqBPFuns.AsyncDoWork((state) =>
            {
                while (!state.IsCancellationRequested && string.IsNullOrEmpty(result))
                {
                    if (_bleReply.Count > 0)
                    {
                        KeyValuePair<string, string> replyContent = _bleReply.FirstOrDefault(o => o.Key == key.ToUpper() && (string.IsNullOrEmpty(value) ? true : o.Value == value));
                        if (!string.IsNullOrEmpty(replyContent.Key))
                        {
                            result = replyContent.Value;
                            _bleReply.Remove(replyContent);
                        }
                    }
                    Thread.Sleep(1);
                }

            }, duration);

            return result;
        }

        //获取蓝牙设备信息列表
        private async Task<List<string>> getReplyList(string key, int duration)
        {
            List<string> result = new List<string>();

            await ZqBPFuns.AsyncDoWork((state) =>
            {
                while (!state.IsCancellationRequested)
                {
                    if (_bleReply.Count > 0)
                    {
                        List<KeyValuePair<string, string>> lst = _bleReply.FindAll(o => o.Key == key);
                        foreach (var replyContent in lst)
                        {
                            result.Add(replyContent.Value);
                        }
                        _bleReply.RemoveAll(o => o.Key == key);
                    }
                    Thread.Sleep(1);
                }
            }, duration);

            return result;
        }

        [Obsolete]
        private async Task<List<string>> getScanResultList(string key, int duration)
        {
            List<string> result = new List<string>();

            string ackKey = key;
            if (key == "ADV_DATA") ackKey = "ST_SCAN_DEVICE_ON";
            await ZqBPFuns.AsyncDoWork((state) =>
            {
                bool ack = false; //是否收到确认帧
                while (!state.IsCancellationRequested && !ack)
                {
                    if (_bleReply.Count > 0)
                    {
                        KeyValuePair<string, string> replyContent = _bleReply.FirstOrDefault(o => o.Key == ackKey);
                        if (!string.IsNullOrEmpty(replyContent.Key) && replyContent.Value == "timeout")
                        {
                            ack = true;
                            result = _bleReply.FindAll(o => o.Key == key).Select(o => o.Value).ToList();

                            _bleReply.Remove(replyContent);
                            _bleReply.RemoveAll(o => o.Key == key);
                        }
                    }
                    Thread.Sleep(1);
                }
            }, duration);

            return result;
        }

        //获取文件内数据
        private async Task<byte[]> getReplyData(string key, int duration, ZqFileInfo fi)
        {
            List<byte> result = new List<byte>();
            List<DataPacket> sortedPackets = new List<DataPacket>();
            List<ushort> missIndexes = new List<ushort>();

            uint fileLength = 0;
            ushort endPackIndex = BitConverter.ToUInt16(new byte[] { 0xFF, 0xFF }, 0);
            bool reachedEndPack = false, contentMatched = false;
            await ZqBPFuns.AsyncDoWork((state) =>
            {
                ushort i = 0; //扇区序号
                int replyIndex = 0; //已读取的数据包索引
                while (!state.IsCancellationRequested && !reachedEndPack && !contentMatched)
                {
                    for (int m = replyIndex; m < _dataReply.Count; m++)
                    {
                        if (state.IsCancellationRequested) break;

                        DataPacket pack = _dataReply[m];
                        if (pack.Key == key)
                        {
                            reachedEndPack = (pack.SectorIndex == endPackIndex && pack.ContentLength == 4);
                            if (reachedEndPack)
                            {
                                //校验整体文件的长度及内容
                                fileLength = BitConverter.ToUInt32(ZqBPFuns.GetBySystemEndian(pack.Content), 0);
                                if (result.Count == fileLength)
                                {
                                    contentMatched = true;
                                    //string calcCRC = PubFuns.CalculateFileCrc32(result.ToArray());
                                    //contentMatched = (calcCRC.ToLower() == fi.CRC.PadLeft(8, '0').ToLower());
                                    break;
                                }
                            }
                            else
                            {
                                result.AddRange(pack.Content);
                                if (pack.SectorIndex != i)
                                {
                                    for (ushort n = i; n < pack.SectorIndex; n += 2)
                                    {
                                        missIndexes.Add(i);
                                        i += 2;
                                    }
                                }
                                i += 2;
                            }
                        }
                        replyIndex++;
                    }

                    Thread.Sleep(1);
                }
            }, duration);

            _are.Set();

            if (!contentMatched)
            {
                if (missIndexes.Count == 0)
                {
                    if (!reachedEndPack) { Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} 未收到结束数据包"); }

                    return null;
                }

                sortedPackets.AddRange(_dataReply.Where(o => o.SectorIndex < endPackIndex));

                //尝试补充获取缺失的数据包
                for (int j = 0; j < missIndexes.Count; j++)
                {
                    await Task.Delay(10);
                    Console.WriteLine($"丢失的扇区序号: {missIndexes[j]}");
                    sendData(_comm, $"FILEDATA={fi.FileName},{missIndexes[j]},{1024}" + "\r\n");
                    await ZqBPFuns.AsyncDoWork((state) =>
                    {
                        bool received = false;
                        while (!state.IsCancellationRequested && !received)
                        {
                            if (_dataReply.Count > 0)
                            {
                                DataPacket pack = _dataReply.FirstOrDefault(o => o.Key == key && o.SectorIndex == missIndexes[j]);
                                if (pack != null)
                                {
                                    sortedPackets.Add(pack);
                                    received = true;
                                }
                            }
                            Thread.Sleep(1);
                        }
                    }, 1500);
                    _are.Set();
                }
                sortedPackets = sortedPackets.OrderBy(o => o.SectorIndex).ToList(); //排序
                result.Clear();
                for (int k = 0; k < sortedPackets.Count; k++)
                {
                    result.AddRange(sortedPackets[k].Content);
                }

                //校验整体文件内容
                if (result.Count > 0)
                {
                    if (fileLength > 0)
                    {
                        if (result.Count == fileLength)
                        {
                            contentMatched = true;
                            //string calcCRC = PubFuns.CalculateFileCrc32(result.ToArray());
                            //contentMatched = (calcCRC.ToLower() == fi.CRC.PadLeft(8, '0').ToLower());
                        }
                    }
                }
            }

            if (contentMatched)
                return result.ToArray();
            else
                return null;
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

            this.BluetoothPrefix = blePrefix.InnerText;
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
                port.InnerText = _comm.PortName;

                XmlNode channel = xmlDoc.GetElementsByTagName("UsedChannel")[0];
                channel.InnerText = IsConnectToBle ? "bluetooth" : "local_device";

                XmlNode mac = xmlDoc.GetElementsByTagName("UsedBluetoothMAC")[0];
                mac.InnerText = IsConnectToBle ? this._remoteBleMAC : "";

                xmlDoc.Save(filePath);
            }
            else
            {
                //新增xml
                XmlNode header = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(header);

                XmlElement root = xmlDoc.CreateElement("Connection");

                XmlElement blePrefix = xmlDoc.CreateElement("BluetoothPrefix");
                blePrefix.InnerText = this.BluetoothPrefix;
                root.AppendChild(blePrefix);

                XmlElement usedConnection = xmlDoc.CreateElement("UsedConnection");

                XmlElement port = xmlDoc.CreateElement("UsedSerialPort");
                port.InnerText = _comm.PortName;

                XmlElement channel = xmlDoc.CreateElement("UsedChannel");
                channel.InnerText = IsConnectToBle ? "bluetooth" : "local_device";

                XmlElement mac = xmlDoc.CreateElement("UsedBluetoothMAC");
                mac.InnerText = IsConnectToBle ? this._remoteBleMAC : "";

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
    }

    /// <summary>
    /// 数据包
    /// </summary>
    internal class DataPacket
    {
        public string Key { get; set; }
        public ushort SectorIndex { get; set; }
        public ushort ContentLength { get; set; }
        public byte[] Content { get; set; }
        public ushort CRC { get; set; }
    }

}
