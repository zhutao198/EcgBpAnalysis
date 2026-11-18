using ABPMRepository;
using AnalysisSystem.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    class DataServerListener
    {
        private HttpListener _listerner = new HttpListener();
        private DataTransfer _transfer = new DataTransfer();
        private ABPMxmlManager _abpmManager = new ABPMxmlManager(false, true);
        private readonly string _uploadFilePath = PubFuns.GetAbsolutePath("Upload", true, true);
        private Dictionary<string, List<ZqFileInfo>> _filesToUpload = new Dictionary<string, List<ZqFileInfo>>(); //<病例id, 待上传的数据文件>
        private static object _lockObj = new object();

        public DataServerListener()
        {
            this._transfer.TransferredLength = 0;
            ServicePointManager.DefaultConnectionLimit = 1000;
        }

        public void Start()
        {
            clearExpiredFiles();

            try
            {
                IPAddress localIP = getLocalIP();
                string port = ServerConfig.GetPort();

                if (localIP == null)
                {
                    localIP = IPAddress.Parse("127.0.0.1");
                }

                for (; true;)
                {
                    try
                    {
                        //AddFireWallPort(port);

                        _listerner.AuthenticationSchemes = AuthenticationSchemes.Anonymous; //按匿名身份验证方式访问
                        _listerner.Prefixes.Add($"http://{localIP}:{port}/");
                        _listerner.Prefixes.Add($"http://localhost:{port}/");
                        _listerner.Prefixes.Add($"http://127.0.0.1:{port}/");

                        Thread myThread = new Thread(ListenClientConnect);
                        myThread.IsBackground = true;
                        myThread.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("未能启动监听服务!");
                        _listerner = new HttpListener();
                        continue;
                    }
                    break;
                }
                Console.WriteLine("监听服务启动成功.");
            }
            catch (Exception e)
            {
                Logger.CreateLog(e, "错误信息记录");
            }
        }

        public void Dispose()
        {
            try
            {
                this._listerner.Stop();
                //DataHandler.UninitDll();
                this._transfer.IsCancelled = true;
            }
            catch (Exception ex)
            { }
        }

        private void ListenClientConnect()
        {
            _listerner.Start();
            Thread.Sleep(100);
            //DataHandler.InitDll();

            int maxThreadNum, portThreadNum;
            int minThreadNum;
            ThreadPool.GetMaxThreads(out maxThreadNum, out portThreadNum);
            ThreadPool.GetMinThreads(out minThreadNum, out portThreadNum);
            Console.WriteLine("最大线程数：{0}", maxThreadNum);
            Console.WriteLine("最小空闲线程数：{0}", minThreadNum);
            Console.WriteLine("\n\n等待客户端连接中......");
            while (true)
            {
                //等待请求连接
                //没有请求则GetContext处于阻塞状态
                if (_listerner.IsListening)
                {
                    try
                    {
                        HttpListenerContext ctx = _listerner.GetContext();
                        ThreadPool.QueueUserWorkItem(new WaitCallback(TaskProc), ctx);
                    }
                    catch
                    {
                        Console.WriteLine("监听服务已停止");
                        _listerner.Stop();
                        //DataHandler.UninitDll();
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private async void TaskProc(object context)
        {
            HttpListenerContext ctx = (HttpListenerContext)context;
            ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ctx.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");

            HttpListenerRequest request = ctx.Request;
            HttpListenerResponse response = ctx.Response;
            string requestPath = request.RawUrl.Trim('/').ToUpper();
            try
            {
                if (request.HttpMethod.ToUpper() == "OPTIONS")
                {
                    //让POST预检通过
                    response.Headers.Add("Access-Control-Allow-Headers", "authorization");
                    response.Headers.Add("Access-Control-Allow-Headers", "content-type");

                    response.StatusCode = 200;
                    response.ContentType = "application/json";
                    response.ContentEncoding = Encoding.UTF8;
                    byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { code = "200", message = "request is accepted" }));
                    response.ContentLength64 = buffer.Length;

                    using (Stream writer = ctx.Response.OutputStream)
                    {
                        writer.Write(buffer, 0, buffer.Length);
                        writer.Close();
                        response.Close();
                    }
                    return;
                }

                if (request.HttpMethod.ToUpper() == "POST") //处理POST请求
                {
                    string requestStr;
                    using (Stream reqStream = request.InputStream)
                    {
                        using (StreamReader reader = new StreamReader(reqStream, request.ContentEncoding))
                        {
                            requestStr = reader.ReadToEnd();
                        }
                    }

                    string[] arrDontDecode = { "EXPORTDATALIST", "EXPORTPDF", "SETNIBPINFO", "CLEARBPDATA" };
                    if (requestStr != "{}" && !arrDontDecode.Any(o => o == requestPath))
                    {
                        requestStr = System.Web.HttpUtility.UrlDecode(requestStr, Encoding.UTF8);
                    }

                    string logContent = $"收到新的请求：{requestPath}, HTTP方法: {request.HttpMethod}, 入参: {requestStr}";
                    if (requestPath != "CURRENT_UPLOAD_PROCEEDINGS" && requestPath != "GETDONGLESTATUS")
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ********* {logContent}");
                        Logger.CreateLog(logContent, "监听请求");
                    }

                    //对于GET方法直接用QueryString------
                    //string type = ctx.Request.QueryString["type"];
                    //string userId = ctx.Request.QueryString["userId"];
                    //string password = ctx.Request.QueryString["password"];
                    //string filename = Path.GetFileName(ctx.Request.RawUrl);
                    //string userName = HttpUtility.ParseQueryString(filename).Get("userName"); //避免中文乱码

                    int responseCode = 200;
                    string responseContentType = string.Empty;
                    ResponseInfo responseInfo = null;
                    string responeseContent = string.Empty;
                    string responseMessage = string.Empty;

                    switch (requestPath)
                    {
                        case "CHECKBPDEVICESTATUS": //检查血压设备的连接状态
                            bool connected = _abpmManager.IsConnectionReady(out string connErr, out bool isUsedBluetooth, 1);
                            responseCode = connected ? 200 : 400;
                            break;
                        case "SETBPDEVICEINFO":   //设置血压计设备参数
                            doSetBPDeviceInfo(requestStr, out responseCode, out responseMessage);
                            break;
                        case "GETPATIENTINFO":  //从血压设备读取病人信息
                            PatientInformation patient1 = retrievePatientInfoFromDevice(requestStr, out responseCode, out responseMessage);
                            responeseContent = JsonConvert.SerializeObject(new
                            {
                                code = responseCode.ToString(),
                                message = responseMessage,
                                data = patient1
                            });
                            break;
                        case "INPUTPATIENTINFO":  //写入病人信息至血压设备
                            doInputPatientInfo(requestStr, out responseCode, out responseMessage);
                            break;
                        case "ABPM_UPLOADDATA":   //上传血压数据文件
                            _transfer = new DataTransferHandler();
                            doUploadData_ABPM(requestStr, out responseCode, out responseMessage);
                            break;
                        case "ECG_UPLOADDATA":    //上传心电数据文件
                            _transfer = new DataTransferHandlerNew();
                            doUploadData_ECG(requestStr, out responseCode, out responseMessage);
                            break;
                        case "CURRENT_UPLOAD_PROCEEDINGS": //上传进度
                            responseCode = 200;
                            responseMessage = string.Format("{0}%", _transfer.TransferredPercent);
                            break;
                        case "CANCEL_UPLOAD":     //取消上传
                            doCancelUpload(requestStr, out responseCode, out responseMessage);
                            break;
                        case "GETBLUETOOTHLIST":  //获取蓝牙设备列表(血压)
                            getBluetoothList(out responseCode, out responseMessage);
                            break;
                        case "CONNECTBLUETOOTH":  //连接蓝牙设备进行通讯(血压)
                            string remoteBleMac = getQueryString(requestStr, "MAC");
                            connectBluetooth(remoteBleMac, out responseCode, out responseMessage);
                            break;
                        case "QUERYSN":           //获取心电或血压设备的序列号
                            string devType = getQueryString(requestStr, "type");
                            string devSN = getDeviceSN(devType, out responseCode, out responseMessage);
                            responeseContent = JsonConvert.SerializeObject(new
                            {
                                code = responseCode.ToString(),
                                message = responseMessage,
                                data = new { sn = devSN }
                            });
                            break;
                        case "GETDRIVEINFO":
                            object driveInfo = getDriveInfo(requestStr, out responseCode, out responseMessage);
                            responeseContent = JsonConvert.SerializeObject(new
                            {
                                code = responseCode.ToString(),
                                message = responseMessage,
                                data = new { driveInfo }
                            });
                            break;
                        case "GETWAVEDATA":
                            responseContentType = "application/x-www-form-urlencoded";
                            string indexPara = getQueryString(requestStr, "index");
                            string countPara = getQueryString(requestStr, "count");
                            int.TryParse(indexPara, out int idx);
                            int.TryParse(countPara, out int ct);
                            List<short[]> lstWaveData = DataHandler.GetWaveData(idx, ct);
                            responeseContent = DataResponse.ZoncareJsonSerialize(new DataResponse
                            {
                                code = responseCode.ToString(),
                                message = "OK",
                                data = lstWaveData
                            });
                            break;
                        case "GETPULSEWAVEDATA": //获取脉搏波数据
                            string datFilePath = getQueryString(requestStr, "filePath");
                            string prPara = getQueryString(requestStr, "pr");
                            int.TryParse(prPara, out int pr);
                            responseInfo = await getPulseWaveData(datFilePath, pr);
                            break;

                        #region 8337相关
                        case "SEARCHDEVICES": //查找血压监测仪设备(8337)
                            string connType = getQueryString(requestStr, "connectionType");
                            responseInfo = await searchDevices(Convert.ToInt32(connType));
                            break;
                        case "CONNECTBLE": //连接8337蓝牙设备
                            string connMac = getQueryString(requestStr, "mac");
                            responeseContent = JsonConvert.SerializeObject(await bondDevice(connMac));
                            break;
                        case "DISCONNECTBLE": //断开8337蓝牙设备连接
                            responeseContent = JsonConvert.SerializeObject(await unbondDevice());
                            break;
                        case "GETNIBPINFO":  //从8337设备读取病人信息和戴机参数
                            responseInfo = await getNibpInfo(getQueryString(requestStr, "mac"));
                            break;
                        case "SETNIBPINFO":  //写入病人信息及戴机参数至8337血压设备
                            responseInfo = await setNIBPInfo(requestStr);
                            break;
                        case "ABPDATAUPLOADCHECK": //8337设备数据上传检查

                            break;
                        case "ABPDATAUPLOAD": //上传8337设备内的文件
                            _transfer = new DataTransferHandlerNew();
                            responseInfo = await uploadFile8337(requestStr);
                            break;
                        case "CLEARBPDATA": //清空设备内的数据
                            responseInfo = await clearBpData(requestStr);
                            break;
                        #endregion

                        case "SENDRINGCOMMAND": //向心电血压二合一设备发送响铃指令
                            CommonFuns.SendToSerialPorts("AT%RING\r\n");
                            break;
                        case "GETDEVICEMODEL": //获取设备型号
                            responseInfo = await getDeviceModel(getQueryString(requestStr, "mac"));
                            break;
                        case "SETSYSTEMTIME8377": //为心电血压二合一设备设置系统时间
                            string timeString8377 = getQueryString(requestStr, "sysTime");
                            int setResult = AppCode.SetDeviceInfo(timeString8377);
                            responseCode = (setResult == 1 ? 200 : 400);
                            break;
                        case "SETSYSTEMTIME": //为8337设备设置系统时间
                            string timeString = getQueryString(requestStr, "sysTime");
                            string mac = getQueryString(requestStr, "mac");
                            string timeStringNew = timeString;
                            if (string.IsNullOrEmpty(mac)) timeStringNew = "a" + timeString;
                            responseInfo = await updateDeviceSetting(timeStringNew, mac);

                            await updateMaxPressure(mac); //校正不支持修改最大充气压力的监测仪压力值(300mmHg)
                            break;
                        case "SETDEVICENUMBER": //为心电血压二合一设备设置蓝牙编号
                            string bleNum = getQueryString(requestStr, "bleNum");
                            responseInfo = await updateDeviceSetting("b" + bleNum);
                            break;

                        case "SPLITECGFILE": //心电文件拆分(24小时)
                            responseInfo = await doSplitEcgFile(requestStr);
                            break;
                        case "EXPORTDATALIST": //导出病例数据列表(Excel)
                            doDataListExport(requestStr, out responseCode, out responseMessage);
                            break;
                        case "EXPORTPDF": //根据html内容生成pdf文件
                            doPdfExport(requestStr, out responseCode, out responseMessage);
                            break;
                        default:
                            responseCode = 400;
                            responseMessage = "无效的请求";
                            break;
                    }

                    if (responseInfo != null)
                    {
                        responeseContent = JsonConvert.SerializeObject(responseInfo);
                    }
                    else if (string.IsNullOrEmpty(responeseContent))
                    {
                        responeseContent = JsonConvert.SerializeObject(new { code = responseCode.ToString(), message = responseMessage });
                    }

                    bool responded = sendBackResponse(ctx, 200, responeseContent, responseContentType);

                    //记录请求日志
                    if (requestPath != "CURRENT_UPLOAD_PROCEEDINGS" && requestPath != "GETDONGLESTATUS")
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ********* {requestPath}的请求结果: {responeseContent}");
                        Logger.CreateLog($"HTTP请求的方法: {requestPath}, 结果: {responeseContent}", "监听请求");
                    }
                }
                else
                {
                    sendBackResponse(ctx, 400, JsonConvert.SerializeObject(new { code = "400", message = "不支持的请求类型" }));
                }
            }
            catch (Exception ex)
            {
                sendBackResponse(ctx, 400, JsonConvert.SerializeObject(new { code = "400", message = "发生未知的异常" }));
                Logger.CreateLog(ex, "错误信息记录");
            }
        }

        private bool sendBackResponse(HttpListenerContext ctx, int statusCode, string responeseContent, string responseContentType = "")
        {
            bool result = false;
            try
            {
                ctx.Response.ContentType = string.IsNullOrEmpty(responseContentType) ? "application/json" : responseContentType;
                ctx.Response.ContentEncoding = Encoding.UTF8;
                byte[] buffer = Encoding.UTF8.GetBytes(responeseContent);
                ctx.Response.ContentLength64 = buffer.Length;

                //using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8))
                using (Stream writer = ctx.Response.OutputStream)
                {
                    writer.Write(buffer, 0, buffer.Length);
                    writer.Close();
                }
                ctx.Response.StatusCode = statusCode; //设置返回给客户端的http状态代码
                ctx.Response.Close();
                result = true;
            }
            catch (Exception ex) { }

            return result;
        }


        private void doSetBPDeviceInfo(string requestStr, out int responseCode, out string responseMessage)
        {
            NameValueCollection reqInfo = System.Web.HttpUtility.ParseQueryString(requestStr);
            try
            {
                string remoteMAC = reqInfo.Get("remoteMAC");

                string[] arrTimeSpan = new string[] {
                        string.Join(",", 0, reqInfo.Get("select1"), reqInfo.Get("start1"), reqInfo.Get("end1"), reqInfo.Get("interval1")),
                        string.Join(",", 1, reqInfo.Get("select2"), reqInfo.Get("start2"), reqInfo.Get("end2"), reqInfo.Get("interval2")),
                        string.Join(",", 2, reqInfo.Get("select3"), reqInfo.Get("start3"), reqInfo.Get("end3"), reqInfo.Get("interval3")),
                        string.Join(",", 3, reqInfo.Get("select4"), reqInfo.Get("start4"), reqInfo.Get("end4"), reqInfo.Get("interval4")),
                        string.Join(",", 4, reqInfo.Get("select5"), reqInfo.Get("start5"), reqInfo.Get("end5"), reqInfo.Get("interval5")),
                    };

                string frontPanelData = string.Concat(reqInfo.Get("keyboard"), ",", reqInfo.Get("display"));
                string alarmData = string.Concat(reqInfo.Get("dayAlarm"), ",", reqInfo.Get("nightAlarm"));

                string strMaxPressure = reqInfo.Get("pressure"),
                    strMaxSystolic = reqInfo.Get("systolicMax"),
                    strMinSystolic = reqInfo.Get("systolicMin"),
                    strMaxDiastolic = reqInfo.Get("diastolicMax"),
                    strMinDiastolic = reqInfo.Get("diastolicMin");
                string strPressureThreshold = string.Join(",", strMaxPressure, strMaxSystolic, strMinSystolic, strMaxDiastolic, strMinDiastolic);
                string strDeviceTime = reqInfo.Get("time");
                if (string.IsNullOrEmpty(strDeviceTime))
                {
                    string curParas = _abpmManager.GetParameters();
                    if (!string.IsNullOrEmpty(curParas))
                    {
                        string[] paras = curParas.Split(',');
                        if (paras.Length > 0)
                        {
                            strDeviceTime = paras[0];
                        }
                    }

                    if (string.IsNullOrEmpty(strDeviceTime)) //读取不到当前血压配置
                    {
                        responseCode = 400;
                        responseMessage = "参数信息设置失败";
                        return;
                    }
                }

                PatientInformation patientInfo = new PatientInformation() //患者信息
                {
                    PatientID = reqInfo.Get("caseNo"),
                    Name = reqInfo.Get("name"),
                    Sex = reqInfo.Get("sex"),
                    Age = Convert.ToInt32(reqInfo.Get("age")),
                    AgeUnit = reqInfo.Get("ageUnit"),
                    ExamItem = 16,
                    examdeptCode = reqInfo.Get("checkDept"),
                    ResultStatus = 2
                };

                bool bluetoothEnabled = (remoteMAC != null && remoteMAC.Length > 0 && remoteMAC != "0");
                int opResult = _abpmManager.SetParameters(
                    arrTimeSpan,
                    strDeviceTime,
                    frontPanelData,
                    alarmData,
                    reqInfo.Get("pressureUnit"),
                    strPressureThreshold,
                    bluetoothEnabled,
                    patientInfo);
                if (0 == opResult)
                {
                    responseCode = 200;
                    responseMessage = "OK";
                }
                else if (2 == opResult)
                {
                    responseCode = 401;
                    responseMessage = "设备连接通信失败";
                }
                else
                {
                    responseCode = 400;
                    responseMessage = "参数信息设置失败";
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }
        }

        private void doInputPatientInfo(string requestStr, out int responseCode, out string responseMessage)
        {
            NameValueCollection reqInfo = System.Web.HttpUtility.ParseQueryString(requestStr);
            PatientInfo patientInfo = new PatientInfo()
            {
                PatientID = reqInfo.Get("examID"),
                Sex = reqInfo.Get("sex"),
                Name = reqInfo.Get("patientName"),
            };
            ExamInfo examInfo = new ExamInfo()
            {
                ExamItem = reqInfo.Get("examItem"),
                Age = reqInfo.Get("age"),
                AgeUnit = reqInfo.Get("ageUnit"),
                examdeptCode = reqInfo.Get("examdeptCode"),
                ResultStatus = reqInfo.Get("resultStatus")
            };

            try
            {
                bool opResult = _abpmManager.InputPatientInfo(
                    patientInfo.PatientID,
                    patientInfo.Sex,
                    patientInfo.Name,
                    Convert.ToInt32(examInfo.Age),
                    examInfo.AgeUnit,
                    Convert.ToInt32(examInfo.ExamItem),
                    examInfo.examdeptCode,
                    Convert.ToInt32(examInfo.ResultStatus));
                if (true == opResult)
                {
                    responseCode = 200;
                    responseMessage = "OK";
                }
                else
                {
                    responseCode = 400;
                    responseMessage = "血压计参数设置出错，请检查各项输入值";
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }
        }

        //上传血压数据文件
        private void doUploadData_ABPM(string requestStr, out int responseCode, out string responseMessage)
        {
            NameValueCollection reqInfo = System.Web.HttpUtility.ParseQueryString(requestStr);
            string serverUri = reqInfo.Get("url");
            string examId = reqInfo.Get("examId");
            string userId = reqInfo.Get("userId");
            string deviceType = reqInfo.Get("deviceType");
            string remoteMAC = reqInfo.Get("remoteMAC");
            PatientInfo patientInfo = new PatientInfo()
            {
                PatientID = reqInfo.Get("examId"),
                Sex = reqInfo.Get("sex"),
                Name = reqInfo.Get("patientName")
            };
            ExamInfo examInfo = new ExamInfo()
            {
                ExamItem = reqInfo.Get("examItem"),
                Age = reqInfo.Get("age"),
                AgeUnit = reqInfo.Get("ageUnit"),
                examdeptCode = reqInfo.Get("examdeptCode"),
                ResultStatus = reqInfo.Get("resultStatus")
            };

            if (string.IsNullOrWhiteSpace(serverUri) || string.IsNullOrWhiteSpace(examId))
            {
                responseCode = 400;
                responseMessage = "请求的参数不正确";
                return;
            }

            try
            {
                string strLocalFolder = _uploadFilePath + "\\ABPM";
                if (!Directory.Exists(strLocalFolder)) Directory.CreateDirectory(strLocalFolder);

                string strLocalFileName = string.Concat(strLocalFolder, $"\\{examId}.dat");
                bool bluetoothEnabled = (remoteMAC.Length > 0 && remoteMAC != "0");

                int generation = 0;
                if (!bluetoothEnabled)
                    generation = _abpmManager.SaveABPM(
                        strLocalFileName,
                        patientInfo.PatientID,
                        patientInfo.Sex,
                        patientInfo.Name,
                        getIntValue(examInfo.Age),
                        examInfo.AgeUnit,
                        getIntValue(examInfo.ExamItem),
                        examInfo.examdeptCode,
                        getIntValue(examInfo.ResultStatus));
                else
                    generation = _abpmManager.SaveABPM_Validated(
                        strLocalFileName,
                        patientInfo.PatientID,
                        patientInfo.Sex,
                        patientInfo.Name,
                        getIntValue(examInfo.Age),
                        examInfo.AgeUnit,
                        getIntValue(examInfo.ExamItem),
                        examInfo.examdeptCode,
                        getIntValue(examInfo.ResultStatus));

                if (0 == generation)
                {
                    string uploadResult = (_transfer as DataTransferHandler).UploadData_ABPM(serverUri, strLocalFileName, examId, userId, out string errMsg, deviceType);
                    if ("200" == uploadResult)
                    {
                        File.Delete(strLocalFileName);
                        responseCode = 200;
                        responseMessage = "OK";
                    }
                    else
                    {
                        responseCode = Convert.ToInt32(uploadResult);
                        responseMessage = (string.IsNullOrEmpty(errMsg) ? "文件上传失败" : errMsg);
                    }
                }
                else
                {
                    if (2 == generation)
                    {
                        responseCode = 401;
                        responseMessage = "设备连接通信失败";
                    }
                    else
                    {
                        responseCode = 400;
                        responseMessage = "血压数据文件生成失败";
                    }
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }
        }

        //上传心电数据文件
        private void doUploadData_ECG(string requestStr, out int responseCode, out string responseMessage)
        {
            string serverUri = getQueryString(requestStr, "url");
            string examId = getQueryString(requestStr, "examId");
            string userId = getQueryString(requestStr, "userId");

            if (string.IsNullOrWhiteSpace(serverUri) || string.IsNullOrWhiteSpace(examId))
            {
                responseCode = 400;
                responseMessage = "请求的参数不正确";
                return;
            }
            try
            {
                string strLocalFolder = _uploadFilePath + "\\ECG";
                if (!Directory.Exists(strLocalFolder)) Directory.CreateDirectory(strLocalFolder);

                string fdaFilePath = FileOperator.GetFileFromSD(out string sn);

                int uploadResult = 0;
                if (!string.IsNullOrEmpty(fdaFilePath))
                {
                    FileInfo fiFda = new FileInfo(fdaFilePath);
                    string inIfilePath = fiFda.DirectoryName.TrimEnd('\\') + "\\holter.ini";
                    uploadResult = (_transfer as DataTransferHandlerNew).UploadData_ECG(serverUri, examId, userId, fdaFilePath, inIfilePath);
                    if (1 == uploadResult)
                    {
                        responseCode = 200;
                        responseMessage = "OK";
                    }
                    else
                    {
                        responseCode = 400;
                        responseMessage = "文件上传失败";
                    }
                }
                else
                {
                    responseCode = 400;
                    responseMessage = "没有检测到相关的心电数据";
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }
        }

        private void doCancelUpload(string requestStr, out int responseCode, out string responseMessage)
        {
            try
            {
                _transfer.IsCancelled = true;

                string examItem = getQueryString(requestStr, "examItem");
                string examId = getQueryString(requestStr, "examId");
                if (examItem == "16") //动态血压
                {
                    string strLocalFile = string.Concat(_uploadFilePath, "\\ABPM\\", examId, ".dat");
                    if (File.Exists(strLocalFile)) File.Delete(strLocalFile);
                }
                else if ((new string[] { "6", "7", "12", "13", "14", "15" }).Contains(examItem)) //动态心电
                {
                    string strLocalFile1 = string.Concat(_uploadFilePath, "\\ECG\\", examId, ".dat");
                    string strLocalFile2 = string.Concat(_uploadFilePath, "\\ECG\\", examId, ".atr");
                    string strPatientXmlFile = string.Concat(_uploadFilePath, "\\ECG\\Patient\\", examId, ".xml");

                    if (File.Exists(strLocalFile1)) File.Delete(strLocalFile1);
                    if (File.Exists(strLocalFile2)) File.Delete(strLocalFile2);
                    if (File.Exists(strPatientXmlFile)) File.Delete(strPatientXmlFile);
                }

                responseCode = 200;
                responseMessage = "OK";
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
            }
        }

        private void doDataListExport(string requestStr, out int responseCode, out string responseMessage)
        {
            responseCode = 200;
            responseMessage = "";
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(requestStr);
                JArray cols = jo["cols"] as JArray;
                string[] arrCol = new string[cols.Count];
                for (int i = 0; i < cols.Count; i++)
                {
                    arrCol[i] = cols[i]?.ToString();
                }

                List<string[]> lstData = new List<string[]>();
                JArray dataArray = jo["data"] as JArray;
                for (int i = 0; i < dataArray.Count; i++)
                {
                    JArray rowData = dataArray[i] as JArray;
                    string[] arrRow = new string[arrCol.Length];
                    for (int j = 0; j < arrCol.Length; j++)
                    {
                        arrRow[j] = rowData[j]?.ToString();
                    }
                    lstData.Add(arrRow);
                }

                int exported = 0;
                Thread staThread = new Thread(() =>
                {
                    if (AppStartup.Instance != null)
                    {
                        AppStartup.Instance.Invoke(new Action(() =>
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.FileName = "病例列表数据_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                            saveFileDialog.Filter = "Excel文件|*.xls";
                            saveFileDialog.FilterIndex = 0;
                            saveFileDialog.RestoreDirectory = true;
                            saveFileDialog.Title = "导出Excel文件";
                            if (saveFileDialog.ShowDialog(AppStartup.Instance) == DialogResult.OK)
                            {
                                if (!string.IsNullOrEmpty(saveFileDialog.FileName))
                                {
                                    ExcelHelper.NpoiToEcel("病例列表数据", arrCol, lstData, saveFileDialog.FileName);
                                    MessageBox.Show(AppStartup.Instance, "病例数据列表导出成功", "导出提示");
                                    exported = 1;
                                }
                            }
                            else
                            {
                                exported = 2;
                            }
                        }));
                    }
                });

                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();

                if (exported == 0)
                {
                    responseCode = 200;
                    responseMessage = "OK";
                }
                else if (exported == 2)
                {
                    responseCode = 800;
                    responseMessage = "canceled";
                }
                else
                {
                    responseCode = 400;
                    responseMessage = "failed";
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
            }
        }

        private void doPdfExport(string requestStr, out int responseCode, out string responseMessage)
        {
            responseCode = 200;
            responseMessage = "";

            try
            {
                string htmlContent = getQueryString(requestStr, "content");
                string filePath = getQueryString(requestStr, "filePath");
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " 开始导出pdf：" + filePath);

                int exported = 0;

                //Task<bool> exportingTask = AppStartup.Instance.GeneratePDF(htmlContent, filePath);
                //bool res = exportingTask.GetAwaiter().GetResult();
                //if (!res) exported = 1;


                Task<bool> exportingTask = Task.Run(() =>
                {
                    return PdfConverter.ConvertWebpageToPDF(htmlContent, filePath, CommonValues.BaseURL);
                });
                bool res = exportingTask.GetAwaiter().GetResult();
                if (!res) exported = 1;

                if (exported == 0)
                {
                    responseCode = 200;
                    responseMessage = "OK";
                }
                else if (exported == 2)
                {
                    responseCode = 800;
                    responseMessage = "canceled";
                }
                else
                {
                    responseCode = 400;
                    responseMessage = "failed";
                }
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " 完成导出pdf：" + filePath);
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
            }
        }

        //心电文件拆分
        private async Task<ResponseInfo> doSplitEcgFile(string requestStr)
        {
            string responseCode = "200";
            string responseMessage = "";
            List<List<string>> fileGroups = new List<List<string>>();

            string dirPath = getQueryString(requestStr, "dirPath");
            int.TryParse(getQueryString(requestStr, "ecgType"), out int ecgType);

            if (!Directory.Exists(dirPath))
            {
                return new ResponseInfo("401", "directory not found");
            }

            string holterIniPath = Path.Combine(dirPath, "holter.ini");
            if (!File.Exists(holterIniPath))
            {
                return new ResponseInfo("402", "holter.ini not found");
            }

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                Dictionary<string, string> iniContent = INIFile.ReadIni(holterIniPath);
                List<FileInfo> ecgFiles = dirInfo.GetFiles("*.dat").OrderBy(f => f.Name).ToList();
                if (ecgFiles.Count <= 0)
                {
                    return new ResponseInfo("403", "no data files found");
                }

                int days = int.Parse(iniContent["Days"]);
                if (ecgType == 1) //iE95(按数据量拆分)
                {
                    int sampleRate = int.Parse(iniContent["Samplerate"]);
                    int leadType = int.Parse(iniContent["Lead"]);
                    int dataPerS = sampleRate * 16; //每秒数据长度(字节数)
                    if (leadType != 12) dataPerS = sampleRate * 8;

                    int dailyLength = 24 * 3600 * dataPerS;

                    Regex reg = new Regex(@"holter-\d+\.dat");
                    ecgFiles = ecgFiles.FindAll(f => reg.IsMatch(f.Name));
                    fileGroups = await splitFileByDataLength(ecgFiles, dailyLength, days, 1);
                }
                else //8337(按时间拆分处理)
                {
                    Regex reg = new Regex(@"holter-\d{14}\.dat");
                    ecgFiles = ecgFiles.FindAll(f => reg.IsMatch(f.Name));
                    fileGroups = await splitFileByDataTime(ecgFiles, days, 1);
                }
            }
            catch (Exception ex)
            {
                responseCode = "400";
                responseMessage = ex.Message;
            }

            ResponseInfo respInfo = new ResponseInfo(responseCode, responseMessage);
            respInfo.data = fileGroups;
            return respInfo;
        }

        private async Task<List<List<string>>> splitFileByDataLength(List<FileInfo> ecgFiles, int dailyLength, int days, int splitIndex, string fileNetName = "")
        {
            List<List<string>> fileGroupList = new List<List<string>>();

            long sumLength = 0;

            int exceedIdx = -1; //累计超出数据长度的文件的索引
            if (days > 1) //采集时长超过1天的才需要拆分
            {
                for (int i = 0; i < ecgFiles.Count; i++)
                {
                    FileInfo fi = ecgFiles[i];
                    sumLength += fi.Length;
                    if (sumLength > dailyLength)
                    {
                        exceedIdx = i;
                        break;
                    }
                }
            }

            if (exceedIdx >= 0)
            {
                FileInfo fi = ecgFiles[exceedIdx];
                if (string.IsNullOrEmpty(fileNetName)) fileNetName = Path.GetFileNameWithoutExtension(fi.FullName);
                //string destFile = Path.Combine(ecgFiles[exceedIdx].DirectoryName, fileNetName + $"({splitIndex}).dat");
                string destFile = fi.FullName;
                string destFilePlus = Path.Combine(fi.DirectoryName, fileNetName + $"({splitIndex + 1}).dat");

                long readOverNum = 0; //已读字节数

                for (int i = 0; i <= exceedIdx; i++)
                {
                    long splitPos = ecgFiles[i].Length;

                    if (dailyLength - readOverNum < ecgFiles[i].Length) //当前文件需要分割
                    {
                        string srcFileName = Path.Combine(fi.DirectoryName, fi.Name + ".src");
                        File.Move(fi.FullName, srcFileName); //先将源文件重命名

                        splitPos = dailyLength - readOverNum;
                        int res = await FileOperator.SplitFile(srcFileName, destFile, destFilePlus, splitPos);
                        if (splitIndex > 1) File.Delete(srcFileName);
                        break;
                    }

                    readOverNum += splitPos;
                }

                List<string> list1 = new List<string>();
                for (int i = 0; i <= exceedIdx; i++) { list1.Add(ecgFiles[i].Name); }
                fileGroupList.Add(list1);

                List<FileInfo> ecgFilesPlus = new List<FileInfo>();
                ecgFilesPlus.Add(new FileInfo(destFilePlus));
                for (int i = exceedIdx + 1; i < ecgFiles.Count; i++) { ecgFilesPlus.Add(ecgFiles[i]); }
                List<List<string>> otherGroupList = await splitFileByDataLength(ecgFilesPlus, dailyLength, days - 1, splitIndex + 1, fileNetName); //递归调用

                if (otherGroupList.Count > 0)
                {
                    fileGroupList.AddRange(otherGroupList);
                }
                else
                {
                    List<string> list2 = new List<string>();
                    list2.Add(fileNetName + $"({splitIndex + 1}).dat");
                    for (int i = exceedIdx + 1; i < ecgFiles.Count; i++) { list2.Add(ecgFiles[i].Name); }
                    fileGroupList.Add(list2);
                }
            }

            return fileGroupList;
        }

        private async Task<List<List<string>>> splitFileByDataTime(List<FileInfo> ecgFiles, int days, int splitIndex)
        {
            List<List<string>> fileGroupList = new List<List<string>>();
            if (days <= 1) return fileGroupList;

            long sumLength = 0;
            DateTime startTime = DateTime.MinValue;
            DateTime endTime = startTime.AddHours(24);
            byte[] headers = new byte[280];
            int sampleRate = 250, frameSize = 28;

            Func<string, DateTime> getFileTime = new Func<string, DateTime>((fileTimeString) =>
            {
                return DateTime.Now;
            });

            int lastIndexInDay = 0;
            DateTime lastFileTimeInDay = startTime;

            #region 获取数据相关信息并查找24小时内最后一个数据文件
            for (int i = 0; i < ecgFiles.Count; i++)
            {
                FileInfo fi = ecgFiles[i];
                string fileTimeString = Regex.Replace(Path.GetFileNameWithoutExtension(fi.FullName), "^holter-", "");
                bool dateParsed = DateTime.TryParseExact(fileTimeString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime dtFile);
                if (dateParsed)
                {
                    sumLength += fi.Length;
                    if (startTime == DateTime.MinValue)
                    {
                        startTime = dtFile;
                        using (FileStream fs = fi.OpenRead())
                        {
                            fs.Read(headers, 0, headers.Length);
                            int sampleRate0 = BitConverter.ToUInt16(headers, 8);
                            if (headers[0] == 0x02 && headers[1] == 0x00)
                            {
                                if (sampleRate0 < 500)
                                    frameSize = 20;
                                else
                                    frameSize = 28;
                            }
                            if (SysConstants.SampleRates.ContainsKey(sampleRate0))
                                sampleRate = SysConstants.SampleRates[sampleRate0];
                            else
                                sampleRate = sampleRate0;
                        }
                        endTime = startTime.AddHours(24);
                    }

                    if (dtFile <= endTime)
                    {
                        lastIndexInDay = i;
                        lastFileTimeInDay = dtFile;
                    }
                    else { break; }
                }
            }
            #endregion

            int exceedIdx = -1; //累计超出24小时数据长度的文件索引
            int dataPerS = sampleRate * frameSize; //每秒数据长度
            int maxDuration = (int)endTime.Subtract(lastFileTimeInDay).TotalSeconds;
            long maxLength = maxDuration * dataPerS; //24小时内最后一个文件的最大字节数
            FileInfo fiDayLast = ecgFiles[lastIndexInDay];
            if (fiDayLast.Length - headers.Length > maxLength)
            {
                exceedIdx = lastIndexInDay;
            }

            if (exceedIdx >= 0) //24小时内最后一个文件内容长度超出范围
            {
                FileInfo fi = ecgFiles[exceedIdx];
                string fileNetName = Path.GetFileNameWithoutExtension(fi.FullName);
                //string destFile = Path.Combine(fi.DirectoryName, fileNetName + $"({splitIndex}).dat");
                string destFile = fi.FullName;
                //string destFilePlus = Path.Combine(fi.DirectoryName, fileNetName + $"({splitIndex + 1}).dat");
                string destFilePlus = Path.Combine(fi.DirectoryName, "holter-" + endTime.ToString("yyyyMMddHHmmss") + ".dat");

                string srcFileName = Path.Combine(fi.DirectoryName, fi.Name + ".src");
                File.Move(fi.FullName, srcFileName); //先将源文件重命名
                await FileOperator.SplitFile(srcFileName, destFile, destFilePlus, headers.Length + maxLength, headers.Length);

                List<string> list1 = new List<string>();
                for (int i = 0; i <= exceedIdx; i++) { list1.Add(ecgFiles[i].Name); }
                fileGroupList.Add(list1);

                List<string> list2 = new List<string>();
                list2.Add("holter-" + endTime.ToString("yyyyMMddHHmmss") + ".dat");
                for (int i = exceedIdx + 1; i < ecgFiles.Count; i++) { list2.Add(ecgFiles[i].Name); }
                fileGroupList.Add(list2);
            }

            return fileGroupList;
        }


        private PatientInformation retrievePatientInfoFromDevice(string requestStr, out int responseCode, out string responseMessage)
        {
            PatientInformation patient = null;
            try
            {
                patient = _abpmManager.ReadPatientInfo();
                if (patient != null)
                {
                    responseCode = 200;
                    responseMessage = "OK";
                }
                else
                {
                    responseCode = 400;
                    responseMessage = "发生异常，无法读取患者信息";
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }

            return patient;
        }

        private void getBluetoothList(out int responseCode, out string responseMessage)
        {
            try
            {
                List<BluetoothInformation> lstBluetooth = _abpmManager.GetBluetoothList();
                if (lstBluetooth.Count > 0)
                {
                    //string strBluetooths = "\"BluetoothList\":[";
                    //foreach (BluetoothInformation bleInfo in lstBluetooth)
                    //{
                    //    strBluetooths += "{" +
                    //                    "\"BluetoothName\":" + "\"" + bleInfo.BluetoothName + "\"," +
                    //                    "\"BluetoothMAC\":" + "\"" + bleInfo.BluetoothMAC + "\"" +
                    //                    "},";
                    //}
                    //strBluetooths = strBluetooths.Substring(0, strBluetooths.Length - 1);
                    //strBluetooths += "]";
                    string strBluetooths = JsonConvert.SerializeObject(new { BluetoothList = lstBluetooth });

                    responseCode = 200;
                    responseMessage = strBluetooths;
                }
                else
                {
                    responseCode = 404;
                    responseMessage = "";
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }
        }

        private void connectBluetooth(string remoteMac, out int responseCode, out string responseMessage)
        {
            try
            {
                bool connected = _abpmManager.ConnectBluetooth(remoteMac);
                if (connected)
                {
                    responseCode = 200;
                    responseMessage = "OK";
                }
                else
                {
                    responseCode = 400;
                    responseMessage = "蓝牙设备连接失败";
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }
        }

        //获取设备SN
        private string getDeviceSN(string devType, out int responseCode, out string responseMessage)
        {
            string sn = "";
            int errCode = 0;
            responseCode = 200;
            responseMessage = "OK";

            try
            {
                if (devType == "1") //心电
                {
                    string ecgFilePath = FileOperator.GetFileFromSD(out sn);
                }
                else if (devType == "2") //血压
                {
                    sn = _abpmManager.GetSN(out errCode);
                    if (errCode != 0)
                    {
                        if (errCode == -2)
                        {
                            responseCode = 401;
                            responseMessage = "设备连接通信失败";
                        }
                        else
                        {
                            responseCode = 400;
                            responseMessage = "获取设备SN过程发生异常";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }

            return sn;
        }

        //获取指定磁盘的相关信息
        private object getDriveInfo(string requestStr, out int responseCode, out string responseMessage)
        {
            responseCode = 200;
            responseMessage = "OK";
            object retObj = null;
            try
            {
                string rootName = getQueryString(requestStr, "rootName");
                string volumnName = getQueryString(requestStr, "volumnName");
                retObj = FileOperator.GetDriveInfo(rootName, volumnName);
            }
            catch (Exception ex)
            {
                responseCode = 400;
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }

            return retObj;
        }

        //清除过期的文件
        private void clearExpiredFiles()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(_uploadFilePath, "ABPM"));
                DirectoryInfo[] dirs = dirInfo.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    bool dateParsed = DateTime.TryParseExact(dir.Name, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime dtFolder);
                    if (dateParsed)
                    {
                        if (DateTime.Today.Subtract(dtFolder).TotalDays >= 30)
                        {
                            dir.Delete(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("清除过期数据失败: " + ex.Message);
            }
        }

        //获取脉搏波(血压波形)数据
        private async Task<ResponseInfo> getPulseWaveData(string filePath, int refPR)
        {
            string responseCode = "200";
            string responseMessage = "";
            PulseWaveData waveData = null;
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                if (fileBytes.Length > 16) { }

                List<int> dataList = new List<int>();
                List<int> stageList = new List<int>();
                List<GroupInterval> groups = new List<GroupInterval>();
                byte[] waveBytes = new byte[fileBytes.Length - 16];
                Buffer.BlockCopy(fileBytes, 0, waveBytes, 0, waveBytes.Length);

                StringBuilder sbData = new StringBuilder(), sbStage = new StringBuilder();
                int statu = 0;
                int idx = 0, startIdx = 0, endIdx = 0;
                for (int i = 0; i < waveBytes.Length; i += 4)
                {
                    int one = BitConverter.ToInt32(waveBytes, i) & 0x00ffffff; //清除高8位(高8位为帧序号)
                    if ((one & 0xc00000) == 0xc00000) //状态数据
                    {
                        statu = one >> 16 & 0x0f;
                        if (statu == 1) //充气结束
                        {
                        }
                        else if (statu == 3) //台阶放气开始
                        {
                            endIdx = idx - 1;
                            if (endIdx > 0) groups.Add(new GroupInterval(startIdx, endIdx));
                        }
                        else if (statu == 2) //台阶放气完成
                        {
                            startIdx = (idx == 0 ? 0 : (idx));
                        }
                        else if (statu == 15) //全速放气，bit16~bit23=0xFF
                        {
                            statu = 255;
                        }
                    }
                    else
                    {
                        int maskedData = one & 0x3fffff;
                        int mmHgValue = (maskedData - 100000); //转换为对应于mmHg*1000的值
                        dataList.Add(mmHgValue);
                        stageList.Add(statu);
                        sbData.AppendLine(mmHgValue.ToString());
                        sbStage.AppendLine(statu.ToString());
                        statu = 0; //除当前有状态变化的数据外, 其余状态值置0
                        idx++;
                    }
                }
                if (idx - 1 > endIdx) groups.Add(new GroupInterval(startIdx, idx - 1));

                //File.WriteAllText(@"C:\test\bpdata.txt", sbData.ToString(), new UTF8Encoding(false));
                //File.WriteAllText(@"C:\test\stagedata.txt", sbStage.ToString(), new UTF8Encoding(false));

                double[] dc, ac, stepDc;
                int[] stepDcLen;
                int ret = PulseWaveDataHandler.WaveDataPlot(dataList, stageList, refPR, out dc, out ac, out stepDc, out stepDcLen);
                if (ret == 0)
                {
                    waveData = new PulseWaveData(dc, ac, stepDc, stepDcLen, dc.Length);
                    waveData.stepGroups = groups.ToArray();
                }
            }
            catch (Exception ex)
            {
                responseCode = "400";
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }

            ResponseInfo res = new ResponseInfo(responseCode, responseMessage);
            res.data = waveData;
            return res;
        }

        #region 8337接口功能
        private async Task<ResponseInfo> searchDevices(int connectionType)
        {
            string responseCode = "200";
            string responseMessage = "";
            List<BPdevice> deviceList = new List<BPdevice>();

            try
            {
                if (connectionType == 1)
                {
                    BPdevice directConnectDev = FileOperator.GetBpDevice();
                    if (directConnectDev != null) deviceList.Add(directConnectDev);
                }
                else if (connectionType == 2)
                {
                    List<string> devices = await ABPMDataManager.ScanDevice();
                    if (devices != null)
                    {
                        devices.ForEach(mac => deviceList.Add(new BPdevice(connectionType, mac)));
                    }
                }
            }
            catch (Exception ex)
            {
                responseCode = "400";
                responseMessage = ex.Message;
                if (ex.InnerException != null) responseMessage += "\r\n" + ex.InnerException.Message;
            }

            ResponseInfo ret = new ResponseInfo(responseCode, responseMessage);
            ret.data = deviceList;
            return ret;
        }

        private async Task<ResponseInfo> bondDevice(string mac)
        {
            string responseCode = "200";
            try
            {
                int bondResult = await ABPMDataManager.ConnectDevice(mac);
                if (bondResult != 1) responseCode = "401";

                ResponseInfo ret = new ResponseInfo(responseCode, "");
                ret.data = bondResult;
                return ret;
            }
            catch (Exception ex)
            {
                return new ResponseInfo("400", "");
            }
        }

        private async Task<ResponseInfo> unbondDevice()
        {
            string responseCode = "200";
            try
            {
                int unbondResult = await ABPMDataManager.DisconnectDevice();
                if (unbondResult != 1) responseCode = "401";

                ResponseInfo ret = new ResponseInfo(responseCode, "");
                ret.data = unbondResult;
                return ret;
            }
            catch (Exception ex)
            {
                return new ResponseInfo("400", "");
            }
        }

        private async Task<ResponseInfo> getNibpInfo(string mac, bool checkCRC = true)
        {
            ResponseInfo result = new ResponseInfo("200", "");
            try
            {
                if (await ABPMDataManager.CheckConnectedMac(mac) != 1)
                {
                    result.code = "401"; //蓝牙设备(从机)未连接或已连接的MAC地址不匹配
                    return result;
                }

                //ZqFileInfo cfg = await ABPMDataManager.FetchFileInfo("nibp.ini");
                ZqFileInfo cfg = new ZqFileInfo("nibp.ini");
                if (cfg != null)
                {
                    byte[] fileBytes = await ABPMDataManager.SaveFile(cfg);
                    if (fileBytes != null)
                    {
                        string nibpFileContent = Encoding.UTF8.GetString(fileBytes);

                        if (checkCRC)
                        {
                            List<ZqFileInfo> dataFiles = new List<ZqFileInfo>();
                            ZqFileInfo crc32File = await ABPMDataManager.FetchFileInfo("crc32.txt");
                            if (crc32File != null)
                            {
                                if (crc32File.FileSize > 0)
                                {
                                    //byte[] crc32fileBytes = await ABPMDataManager.SaveFile(crc32File, "", 2);
                                    //if (crc32fileBytes != null)
                                    //{
                                    //    dataFiles = FileOperator.GetDataFiles(crc32fileBytes);
                                    //}
                                    //result.data = new { nibpInfo = nibpFileContent, dataFileCount = dataFiles.Count };
                                    result.data = new { nibpInfo = nibpFileContent };
                                }
                                else
                                {
                                    result.code = "407"; //无数据(crc32.txt文件未找到或其内容为空)
                                }
                            }
                            else
                            {
                                result.code = "404"; //读取crc32.txt文件信息失败
                            }
                        }
                        else
                        {
                            result.data = new { nibpInfo = nibpFileContent };
                        }
                    }
                    else
                    {
                        result.code = "403"; //读取nibp.ini文件内容失败
                    }
                }
                else
                {
                    result.code = "402"; //未找到nibp.ini文件
                }
            }
            catch (Exception ex)
            {
                result.code = "400";
            }

            return result;
        }

        private async Task<ResponseInfo> setNIBPInfo(string requestStr)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(requestStr);
            string mac = jo["remoteMAC"].ToString();

            StringBuilder sb = new StringBuilder();
            List<string> originalKeys = new List<string>();

            //if ("200" == (await bondDevices(mac)).code)
            {
                //ZqFileInfo cfg = await ABPMDataManager.FetchFileInfo("nibp.ini");
                ZqFileInfo cfg = new ZqFileInfo("nibp.ini");
                if (cfg != null)
                {
                    byte[] fileBytes = await ABPMDataManager.SaveFile(cfg);
                    if (fileBytes != null)
                    {
                        string fileContent = Encoding.UTF8.GetString(fileBytes);
                        using (StringReader sr = new StringReader(fileContent))
                        {
                            string line;
                            while (!string.IsNullOrWhiteSpace(line = sr.ReadLine()))
                            {
                                string[] lineContent = line.Split("=".ToCharArray(), 2);
                                if (lineContent.Length == 2)
                                {
                                    string lineKey = lineContent[0];
                                    string lineValue = lineContent[1]; //.Trim('"')
                                    originalKeys.Add(lineKey);
                                    if (jo.ContainsKey(lineKey))
                                    {
                                        sb.AppendLine(lineKey + "=" + jo[lineKey]);
                                    }
                                    else
                                    {
                                        sb.AppendLine(lineKey + "=" + lineValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (sb.Length > 0)
            {
                var props = jo.Properties();
                foreach (JProperty prop in props)
                {
                    if (!originalKeys.Contains(prop.Name)) sb.AppendLine(prop.Name + "=" + prop.Value?.ToString());
                }

                string fileContent = sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
                int ret = await ABPMDataManager.WriteFileText("nibp.ini", fileContent);
                if (ret == 1) return new ResponseInfo("200", "");
            }

            return new ResponseInfo("400", "");
        }

        private async Task<ResponseInfo> setNIBPInfo_test(string requestStr)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(requestStr);
            string mac = jo["remoteMAC"].ToString();

            string fileContent = File.ReadAllText(PubFuns.GetAbsolutePath("test1.txt"));
            int ret = await ABPMDataManager.WriteFileText("nibp.ini", fileContent);
            if (ret == 1) return new ResponseInfo("200", "");


            return new ResponseInfo("400", "");
        }

        private async Task<ResponseInfo> uploadFile8337(string requestStr)
        {
            string mac = getQueryString(requestStr, "mac");
            string serverUri = getQueryString(requestStr, "url");
            string examId = getQueryString(requestStr, "examId");
            string userId = getQueryString(requestStr, "userId");
            string crc32Switch = getQueryString(requestStr, "crc32Switch");
            string ignoreCRC = getQueryString(requestStr, "ignoreCRC");
            bool.TryParse(getQueryString(requestStr, "uploadPulseWave"), out bool uploadPulseWave);

            if (string.IsNullOrWhiteSpace(serverUri) || string.IsNullOrWhiteSpace(examId))
            {
                return new ResponseInfo("402", "请求的参数不正确");
            }

            ResponseInfo result = new ResponseInfo("200", "");
            string localFolderPath = Path.Combine(_uploadFilePath, "ABPM", DateTime.Today.ToString("yyyyMMdd"), examId);
            if (!Directory.Exists(localFolderPath)) Directory.CreateDirectory(localFolderPath);

            try
            {
                if (await ABPMDataManager.CheckConnectedMac(mac) != 1)
                {
                    result.code = "401"; //蓝牙设备(从机)未连接或已连接的MAC地址不匹配
                    return result;
                }

                List<string> allFiles = new List<string>(); //待上传文件
                List<string> dataFiles = new List<string>();
                string[] inherentFiles = new string[] { "crc32.txt", "device.ini", "nibp.ini", "nibp_test_result.dat" };
                //string[] inherentFiles = new string[] { "crc32.txt", "device.ini", "nibp.ini" }; //用于注册版本

                if (_filesToUpload.ContainsKey(examId))
                {
                    List<ZqFileInfo> uploadList = _filesToUpload[examId];
                    foreach (ZqFileInfo fileInfo in uploadList)
                    {
                        allFiles.Add(fileInfo.FilePath);
                        if (fileInfo.FileName.StartsWith("nibp-") && fileInfo.FileName.EndsWith("dat"))
                        {
                            dataFiles.Add(fileInfo.FileName);
                        }
                    }
                }
                else
                {
                    List<ZqFileInfo> lstFile = new List<ZqFileInfo>();
                    foreach (string fileName in inherentFiles)
                    {
                        string filePath = Path.Combine(localFolderPath, fileName);
                        allFiles.Add(filePath);
                        lstFile.Add(new ZqFileInfo(fileName, filePath));
                    }

                    //保存固定名称的文件并获取数据文件清单
                    foreach (string fileName in inherentFiles)
                    {
                        string localFilePath = Path.Combine(localFolderPath, fileName);

                        ZqFileInfo cfg = new ZqFileInfo() { FileName = fileName };
                        //ZqFileInfo cfg = await ABPMDataManager.FetchFileInfo(fileName);
                        //if (cfg != null)
                        byte[] fileBytes = await ABPMDataManager.SaveFile(cfg, localFilePath, 2);
                        if (fileBytes == null)
                        {
                            result.code = "403"; //文件保存失败
                            return result;
                        }
                        else if (fileName == "crc32.txt")
                        {
                            List<ZqFileInfo> lstDataFile = FileOperator.GetDataFiles(fileBytes);
                            foreach (ZqFileInfo zqFile in lstDataFile)
                            {
                                dataFiles.Add(zqFile.FileName);

                                string filePath = Path.Combine(localFolderPath, zqFile.FileName);
                                if (uploadPulseWave)
                                {
                                    allFiles.Add(filePath);
                                    lstFile.Add(new ZqFileInfo(zqFile.FileName, filePath));
                                }
                            }
                        }
                    }

                    //获得了数据文件后, 记录待上传的文件
                    if (dataFiles.Count > 0) { _filesToUpload[examId] = lstFile; }
                }

                //保存脉搏波数据文件
                if (uploadPulseWave)
                {
                    _transfer.TotalPercent = 80; //数据读取及上传占总体进度的80%
                    _transfer.BaseOffset = 80;   //数据读取占TotalPercent的80%时间
                    for (int i = 0; i < dataFiles.Count; i++)
                    {
                        string fileName = dataFiles[i];
                        string dataFilePath = Path.Combine(localFolderPath, fileName);
                        if (!File.Exists(dataFilePath))
                        {
                            ZqFileInfo zqFile = new ZqFileInfo(fileName);
                            byte[] fileBytes = await ABPMDataManager.SaveFile(zqFile, dataFilePath, 2);
                            if (fileBytes == null)
                            {
                                result.code = "403"; //文件保存失败
                                return result;
                            }

                            //if ("true" == crc32Switch && "true" != ignoreCRC)
                            //{
                            //    string calcCRC = ZqBPFuns.CalculateFileCrc32(fileBytes);
                            //    bool crcMatched = (calcCRC.ToLower() == fi.CRC.PadLeft(8, '0').ToLower());
                            //    if (crcMatched == false)
                            //    {
                            //        result.code = "409"; //未通过CRC校验
                            //        return result;
                            //    }
                            //}
                        }
                        await Task.Delay(50);
                        _transfer.TransferredPercent = (int)(_transfer.BaseOffset * ((float)(i + 1) / dataFiles.Count));
                    }
                }

                //上传所有文件
                result.code = (_transfer as DataTransferHandlerNew).UploadDataFor8337(serverUri, examId, userId, mac, allFiles, ignoreCRC);

                //删除本地临时文件
                if (result.code == "200")
                {
                    Directory.Delete(localFolderPath, true);
                    _filesToUpload.Remove(examId);
                }
            }
            catch (Exception ex)
            {
                result.code = "400";
            }
            finally
            {
                //unbondDevice();
            }

            return result;
        }

        private async Task<ResponseInfo> clearBpData(string requestStr)
        {
            ResponseInfo result = new ResponseInfo("200", "");

            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(requestStr);

                string mac = jo["remoteMAC"].ToString();
                bool shouldInitialize = bool.Parse(jo["shouldInitialize"].ToString());

                if (await ABPMDataManager.CheckConnectedMac(mac) != 1)
                {
                    result.code = "401"; //蓝牙设备(从机)未连接或已连接的MAC地址不匹配
                    return result;
                }

                int ret = await ABPMDataManager.ClearData();
                if (ret == 1)
                {
                    result.code = "200";
                    if (shouldInitialize) //初始化nibp.ini
                    {
                        StringBuilder sb = new StringBuilder();
                        //ZqFileInfo cfg = await ABPMDataManager.FetchFileInfo("nibp.ini");
                        ZqFileInfo cfg = new ZqFileInfo("nibp.ini");
                        if (cfg != null)
                        {
                            byte[] fileBytes = await ABPMDataManager.SaveFile(cfg);
                            if (fileBytes != null)
                            {
                                string fileContent = Encoding.UTF8.GetString(fileBytes);
                                using (StringReader sr = new StringReader(fileContent))
                                {
                                    string line;
                                    while (!string.IsNullOrWhiteSpace(line = sr.ReadLine()))
                                    {
                                        string[] lineContent = line.Split("=".ToCharArray(), 2);
                                        if (lineContent.Length == 2)
                                        {
                                            string lineKey = lineContent[0];
                                            string lineValue = lineContent[1];
                                            JObject deviceSettings = (JObject)jo["bpDeviceSetting"];
                                            if (lineKey == "pressureUnit" || lineKey == "BPUnit")
                                            {
                                                if (deviceSettings.ContainsKey("pressureUnit")) lineValue = deviceSettings["pressureUnit"].ToString();
                                            }
                                            else if (deviceSettings.ContainsKey(lineKey))
                                            {
                                                lineValue = deviceSettings[lineKey].ToString();
                                            }
                                            switch (lineKey)
                                            {
                                                case "id":
                                                case "name":
                                                case "sex":
                                                case "age":
                                                case "ageUnit":
                                                case "birthday":
                                                case "height":
                                                case "weight":
                                                case "dept":
                                                case "bed":
                                                case "remoteMAC":
                                                case "dataId":
                                                    lineValue = "";
                                                    break;
                                                case "select1":
                                                    lineValue = "0";
                                                    break;
                                                case "select2":
                                                    lineValue = "1";
                                                    break;
                                                case "select3":
                                                    lineValue = "2";
                                                    break;
                                                case "select4":
                                                    lineValue = "3";
                                                    break;
                                                case "select5":
                                                    lineValue = "4";
                                                    break;
                                                case "keyboard":
                                                case "display":
                                                    lineValue = "1";
                                                    break;
                                            }
                                            sb.AppendLine(lineKey + "=" + lineValue);
                                        }
                                    }
                                }
                            }
                        }
                        if (sb.Length > 0)
                        {
                            string fileContent = sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
                            int nibpWrited = await ABPMDataManager.WriteFileText("nibp.ini", fileContent);
                            if (nibpWrited == 1)
                            {
                                result.code = "402"; //nibp.ini初始化失败
                            }
                        }
                    }
                }
                else
                {
                    result.code = "400";
                }
            }
            catch (Exception ex)
            {
                result.code = "400";
            }

            return result;
        }

        //根据设备的机型修正其最大充气压力
        private async Task updateMaxPressure(string mac = "")
        {
            int maxValue = 300;
            try
            {
                StringBuilder sb = new StringBuilder();
                if (string.IsNullOrEmpty(mac))
                {
                    ResponseInfo devModelInfo = await getDeviceModel();
                    string deviceModel = devModelInfo.data?.ToString();
                    if (deviceModel != "iE6Pro" && deviceModel != "iE7Pro")
                    {
                        BPdevice directConnectDev = FileOperator.GetBpDevice();
                        if (directConnectDev != null)
                        {
                            string nibpIniPath = Path.Combine(directConnectDev.DrivePath, "nibp.ini");
                            Dictionary<string, string> nibpMaps = INIFile.ReadIni(nibpIniPath);
                            if (nibpMaps["pressureUnit"].ToString() == "0")
                                nibpMaps["pressure"] = maxValue.ToString();
                            else
                                nibpMaps["pressure"] = CommonFuns.mmHg2KPa(maxValue).ToString("F1");

                            foreach (string key in nibpMaps.Keys)
                            {
                                sb.AppendLine(key + "=" + nibpMaps[key]);
                            }
                            string fileContent = sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
                            File.WriteAllText(nibpIniPath, fileContent, new UTF8Encoding(false));
                        }
                    }
                }
                else
                {
                    //获取机型
                    string deviceModel = string.Empty;
                    ZqFileInfo cfg = new ZqFileInfo("device.ini");
                    byte[] fileBytes = await ABPMDataManager.SaveFile(cfg);
                    if (fileBytes != null)
                    {
                        string iniContent = Encoding.UTF8.GetString(fileBytes);
                        Dictionary<string, string> devMaps = INIFile.ReadIniContent(iniContent);
                        deviceModel = devMaps["deviceModel"];
                    }

                    if (deviceModel != "iE6Pro" && deviceModel != "iE7Pro")
                    {
                        ResponseInfo nibpInfo = await getNibpInfo(mac, false);
                        dynamic nibpInfoData = nibpInfo.data;
                        string nibpIniText = nibpInfoData.nibpInfo;
                        Dictionary<string, string> nibpMaps = INIFile.ReadIniContent(nibpIniText);
                        if (nibpMaps["pressureUnit"].ToString() == "0")
                            nibpMaps["pressure"] = maxValue.ToString();
                        else
                            nibpMaps["pressure"] = CommonFuns.mmHg2KPa(maxValue).ToString("F1");


                        foreach (string key in nibpMaps.Keys)
                        {
                            sb.AppendLine(key + "=" + nibpMaps[key]);
                        }
                        string fileContent = sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
                        int ret = await ABPMDataManager.WriteFileText("nibp.ini", fileContent);
                    }
                }
            }
            catch { }
        }

        private async Task<ResponseInfo> getDeviceModel(string mac = "")
        {
            ResponseInfo result = new ResponseInfo("200", "");
            try
            {
                if (string.IsNullOrEmpty(mac))
                {
                    result.data = AppCode.GetDeviceModel();
                    return result;
                }


                if (await ABPMDataManager.CheckConnectedMac(mac) != 1)
                {
                    result.code = "401"; //蓝牙设备(从机)未连接或已连接的MAC地址不匹配
                    return result;
                }

                string devInfo = await ABPMDataManager.GetDeviceModel();
                if (!string.IsNullOrEmpty(devInfo))
                {
                    result.data = devInfo;
                }
                else
                {
                    result.code = "402";
                }
            }
            catch (Exception ex)
            {
                result.code = "400";
            }

            return result;
        }

        private async Task<ResponseInfo> updateDeviceSetting(string settingInfo, string mac = "")
        {
            ResponseInfo result = new ResponseInfo("200", "");
            try
            {
                //设置设备系统时间或蓝牙设备编号
                if (string.IsNullOrEmpty(mac))
                {
                    int setResult = AppCode.SetDeviceInfo(settingInfo);
                    result.code = (setResult == 1 ? "200" : "400");
                    return result;
                }
                else //以蓝牙通讯方式设置设备系统时间
                {
                    if (await ABPMDataManager.CheckConnectedMac(mac) != 1)
                    {
                        result.code = "401"; //蓝牙设备(从机)未连接或已连接的MAC地址不匹配
                        return result;
                    }

                    int syncResult = await ABPMDataManager.SyncTime(settingInfo);
                    result.code = (syncResult == 1 ? "200" : "402");
                }
            }
            catch (Exception ex)
            {
                result.code = "400";
            }

            return result;
        }

        #endregion


        private string getQueryString(string requestStr, string flag)
        {
            string queryStr = string.Empty;
            queryStr = System.Web.HttpUtility.ParseQueryString(requestStr, Encoding.UTF8).Get(flag);

            //Match match = Regex.Match(requestStr, $@"(?<=(^|&){flag}=)[^&]+", RegexOptions.IgnoreCase);
            //if (match.Success)
            //{
            //    queryStr = match.Value;
            //}

            return queryStr;
        }

        private int getIntValue(string input)
        {
            int intN = 0;
            int.TryParse(input, out intN);
            return intN;
        }

        /// <summary>
        /// 获取本地主机的IPv4地址
        /// </summary>
        /// <returns></returns>
        private IPAddress getLocalIP()
        {
            IPAddress localIp = null;
            try
            {
                IPAddress[] ipArray;
                ipArray = Dns.GetHostAddresses(Dns.GetHostName());

                localIp = ipArray.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            }
            catch (Exception ex)
            {
            }

            return localIp;
        }

        /// <summary>
        /// 开放防火墙对于指定端口的访问限制
        /// </summary>
        /// <param name="port">端口号</param>
        private void addFireWallPort(string port)
        {
            string argsDll = string.Format(@"firewall set portopening TCP {0} ENABLE", port);
            ProcessStartInfo psi = new ProcessStartInfo("netsh", argsDll);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            Process.Start(psi).WaitForExit();
        }
    }

    class ResponseInfo
    {
        public string code { get; set; }
        public string message { get; set; }
        public object data { get; set; }

        public ResponseInfo() { }
        public ResponseInfo(string respCode, string respMessage)
        {
            this.code = respCode;
            this.message = respMessage;
        }
    }
}
