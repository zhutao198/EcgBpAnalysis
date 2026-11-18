using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    public class DataTransferHandler : DataTransfer
    {
        public string UploadData_ABPM(string serverAddr, string localFileName, string examId, string userId, out string errMsg, string deviceType = "")
        {
            string returnValue = "400";
            errMsg = string.Empty;

            this.DestinationAddress = serverAddr;
            TransferRequest = (HttpWebRequest)WebRequest.Create(serverAddr); //serverAddr + "/upload/blood"
            TransferRequest.Method = "POST";
            TransferRequest.KeepAlive = false;
            TransferRequest.Timeout = REQUEST_TIMEOUT;
            TransferRequest.Credentials = CredentialCache.DefaultCredentials;
            TransferRequest.Proxy = null;

            NameValueCollection data = new NameValueCollection();
            data.Add("id", examId);
            data.Add("userId", userId);
            data.Add("deviceType", deviceType);
            string respText = HttpUploadHelper.HttpUploadFile(new string[] { localFileName }, data, this);
            if (!string.IsNullOrEmpty(respText))
            {
                try
                {
                    bool result = HttpService.IsResponseOK(respText, out string respCode, out errMsg);
                    if (result == true)
                    {
                        this.TransferCompleted = true;
                    }
                    if (!string.IsNullOrEmpty(respCode)) returnValue = respCode;
                }
                catch
                {
                    throw new Exception("无法解析远程服务器应答信息");
                }
            }

            return returnValue;
        }

        public int UploadData_ECG(string serverAddr, string examId, string userId, string localDatFileName, string iniFileName)
        {
            int returnValue = 0;

            this.DestinationAddress = serverAddr;
            string remoteUri = serverAddr + "/api/upload/holter";
            TransferRequest = (HttpWebRequest)WebRequest.Create(remoteUri);
            TransferRequest.Method = "POST";
            TransferRequest.KeepAlive = false;
            TransferRequest.Timeout = REQUEST_TIMEOUT;
            TransferRequest.Credentials = CredentialCache.DefaultCredentials;
            TransferRequest.Proxy = null;

            NameValueCollection data = new NameValueCollection();
            data.Add("id", examId);
            data.Add("userId", userId);

            List<string> lstFiles = new List<string>();
            lstFiles.Add(localDatFileName);
            if (!string.IsNullOrWhiteSpace(iniFileName)) lstFiles.Add(iniFileName);

            string sReturnString = HttpUploadHelper.HttpUploadFile(lstFiles.ToArray(), data, this);
            if (!string.IsNullOrEmpty(sReturnString))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(sReturnString);
                    if ("200" == xmlDoc.GetElementsByTagName("rec")[0].InnerText)
                    {
                        this.TransferCompleted = true;
                        returnValue = 1;
                    }
                }
                catch
                {
                    throw new Exception("无法解析远程服务器应答信息");
                }
            }

            //int  n = UploadData_ECG0(serverUri + "/api/upload/holter", "test_001", localDatFileName);

            return returnValue;
        }

        public int UploadData_ABPM0(string serverUri, string examId, string localFileName)
        {
            int returnValue = 0;
            FileStream fs = new FileStream(localFileName, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fs, Encoding.UTF8);
            string strBoundary = "----------" + DateTime.Now.Ticks.ToString("x"); //时间戳
            byte[] boundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + strBoundary + "\r\n"); //请求头信息   
            byte[] foot_data = Encoding.UTF8.GetBytes("\r\n--" + strBoundary + "--\r\n");

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("--{0}", strBoundary);
            sb.Append("\r\n");
            sb.AppendFormat("Content-Disposition: form-data; name=\"examId\"");
            sb.Append("\r\n");
            sb.Append("\r\n");
            sb.Append(examId);
            sb.Append("\r\n");
            sb.AppendFormat("--{0}", strBoundary);
            sb.Append("\r\n");
            sb.Append("Content-Type:application/octet-stream");
            sb.Append("\r\n");
            sb.AppendFormat("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"", Path.GetFileName(localFileName));
            sb.Append("\r\n");
            sb.Append("\r\n");
            string strPostHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(serverUri)); //根据uri创建HttpWebRequest对象
            httpReq.Method = "POST";
            //httpReq.AllowWriteStreamBuffering = false; //对发送的数据不使用缓存    
            httpReq.Timeout = 300000; //设置获得响应的超时时间（300秒）
            httpReq.ContentType = "multipart/form-data;charset=utf-8;boundary=" + strBoundary;
            long length = fs.Length + postHeaderBytes.Length + foot_data.Length;
            this.StartTransfer(httpReq.ContentLength = length);

            try
            {
                int bufferLength = 4096; //每次上传4k
                byte[] buffer = new byte[bufferLength];
                long offset = 0;                        //已上传的字节数
                DateTime startTime = DateTime.Now;      //开始上传时间
                int size = binaryReader.Read(buffer, 0, bufferLength);
                Stream postStream = httpReq.GetRequestStream();
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length); //发送请求头信息

                while (size > 0)
                {
                    postStream.Write(buffer, 0, size);
                    offset += size;
                    this.TransferredLength = offset;

                    TimeSpan span = DateTime.Now - startTime;
                    double seconds = span.TotalSeconds;
                    Console.WriteLine("已用时: " + seconds.ToString("F2") + "秒");
                    if (seconds > 0.001)
                    {
                        Console.WriteLine("平均速度: " + (offset / 1024 / seconds).ToString("0.00") + "KB/秒");
                    }
                    else
                    {
                        Console.WriteLine("正在连接...");
                    }
                    Application.DoEvents();
                    size = binaryReader.Read(buffer, 0, bufferLength);
                }
                postStream.Write(foot_data, 0, foot_data.Length); //添加尾部的时间戳
                postStream.Close();

                WebResponse webResp = httpReq.GetResponse(); //获取服务器端的响应
                Stream respStream = webResp.GetResponseStream();

                StreamReader sr = new StreamReader(respStream, Encoding.UTF8);
                String sReturnString = sr.ReadLine();
                respStream.Close();
                sr.Close();
                if (sReturnString == "OK")
                {
                    returnValue = 1;
                }
            }
            catch (WebException ex)
            {
                var mResponse = ex.Response as HttpWebResponse;
                var responseStream = mResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    Console.WriteLine(streamReader.ReadToEnd());
                    streamReader.Close();
                    responseStream.Close();
                }
                returnValue = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                returnValue = 0;
            }
            finally
            {
                fs.Close();
                binaryReader.Close();
            }
            return returnValue;
        }

        public int UploadData_ECG0(string serverUri, string examId, string localDatFileName, string localAtrFileName = null)
        {
            int returnValue = 0;
            FileStream fs = new FileStream(localDatFileName, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fs);
            string strBoundary = "----------" + DateTime.Now.Ticks.ToString("x"); //时间戳
            byte[] boundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + strBoundary + "\r\n"); //请求头信息 
            byte[] foot_data = Encoding.UTF8.GetBytes("\r\n--" + strBoundary + "--\r\n");
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("--{0}", strBoundary);
            sb.Append("\r\n");
            sb.AppendFormat("Content-Disposition: form-data; name=\"examId\"");
            sb.Append("\r\n");
            sb.Append("\r\n");
            sb.Append(examId);
            sb.Append("\r\n");
            sb.AppendFormat("--{0}", strBoundary);
            sb.Append("\r\n");
            sb.Append("Content-Type: application/octet-stream");
            sb.Append("\r\n");
            sb.AppendFormat("Content-Disposition: form-data; name=\"DatFile\"; filename=\"{0}\"", Path.GetFileName(localDatFileName));
            sb.Append("\r\n");
            sb.Append("\r\n");

            string strPostHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(serverUri)); //根据uri创建HttpWebRequest对象
            httpReq.Method = "POST";
            httpReq.AllowWriteStreamBuffering = false;  //对发送的数据不使用缓存    
            httpReq.Timeout = 300000;   //设置获得响应的超时时间（300秒）
            httpReq.ContentType = "multipart/form-data;charset=utf-8;boundary=" + strBoundary;
            long length = fs.Length + postHeaderBytes.Length + foot_data.Length;
            long fileLength = fs.Length;
            this.StartTransfer(httpReq.ContentLength = length);

            try
            {
                int bufferLength = 4096; //每次上传4k
                byte[] buffer = new byte[bufferLength];
                long offset = 0;                        //已上传的字节数
                DateTime startTime = DateTime.Now;      //开始上传时间
                int size = binaryReader.Read(buffer, 0, bufferLength);
                Stream postStream = httpReq.GetRequestStream();
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length); //发送请求头信息

                while (size > 0)
                {
                    postStream.Write(buffer, 0, size);
                    offset += size;
                    this.TransferredLength = offset;

                    Console.WriteLine("已发送{0}%", this.TransferredPercent);
                    TimeSpan span = DateTime.Now - startTime;
                    double seconds = span.TotalSeconds;
                    Console.WriteLine("已用时: " + seconds.ToString("F2") + "秒");
                    if (seconds > 0.001)
                    {
                        Console.WriteLine("平均速度: " + (offset / 1024 / seconds).ToString("0.00") + "KB/秒");
                    }
                    else
                    {
                        Console.WriteLine("正在连接...");
                    }
                    Application.DoEvents();
                    size = binaryReader.Read(buffer, 0, bufferLength);
                }
                postStream.Write(foot_data, 0, foot_data.Length); //添加尾部的时间戳
                postStream.Close();

                WebResponse webResp = httpReq.GetResponse(); //获取服务器端的响应
                Stream s = webResp.GetResponseStream();

                StreamReader sr = new StreamReader(s);
                String sReturnString = sr.ReadLine();
                s.Close();
                sr.Close();
                if (sReturnString == "OK")
                {
                    Console.WriteLine("上传成功!");
                    returnValue = 1;
                }
            }
            catch (WebException ex)
            {
                var mResponse = ex.Response as HttpWebResponse;
                var responseStream = mResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    Console.WriteLine(streamReader.ReadToEnd());
                    streamReader.Close();
                    responseStream.Close();
                }
                returnValue = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                returnValue = 0;
            }
            finally
            {
                fs.Close();
                binaryReader.Close();
            }
            return returnValue;
        }

        public int Upload_Request(string serverUri, string localFileName, string remoteFileName)
        {
            int returnValue = 0;
            FileStream fs = new FileStream(localFileName, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fs);
            string strBoundary = "----------" + DateTime.Now.Ticks.ToString("x"); //时间戳
            byte[] boundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + strBoundary + "\r\n"); //请求头信息   
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(strBoundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append("file");
            sb.Append("\"; filename=\"");
            sb.Append(remoteFileName);
            sb.Append("\";");
            sb.Append("\r\n");
            sb.Append("Content-Type: ");
            sb.Append("application/octet-stream");
            sb.Append("\r\n");
            sb.Append("\r\n");
            string strPostHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(serverUri)); //根据uri创建HttpWebRequest对象
            httpReq.Method = "POST";
            httpReq.AllowWriteStreamBuffering = false;  //对发送的数据不使用缓存    
            httpReq.Timeout = 300000;   //设置获得响应的超时时间（300秒）
            httpReq.ContentType = "multipart/form-data;charset=utf-8;boundary=" + strBoundary;
            long length = fs.Length + postHeaderBytes.Length + boundaryBytes.Length;
            long fileLength = fs.Length;
            this.StartTransfer(httpReq.ContentLength = length);
            try
            {
                int bufferLength = 4096; //每次上传4k
                byte[] buffer = new byte[bufferLength];
                long offset = 0;                        //已上传的字节数
                DateTime startTime = DateTime.Now;      //开始上传时间
                int size = binaryReader.Read(buffer, 0, bufferLength);
                Stream postStream = httpReq.GetRequestStream();
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length); //发送请求头信息

                while (size > 0)
                {
                    postStream.Write(buffer, 0, size);
                    offset += size;
                    this.TransferredLength = offset;

                    TimeSpan span = DateTime.Now - startTime;
                    double seconds = span.TotalSeconds;
                    Console.WriteLine("已用时: " + seconds.ToString("F2") + "秒");
                    if (seconds > 0.001)
                    {
                        Console.WriteLine("平均速度: " + (offset / 1024 / seconds).ToString("0.00") + "KB/秒");
                    }
                    else
                    {
                        Console.WriteLine("正在连接...");
                    }
                    Application.DoEvents();
                    size = binaryReader.Read(buffer, 0, bufferLength);
                }

                postStream.Write(boundaryBytes, 0, boundaryBytes.Length); //添加尾部的时间戳
                postStream.Close();
                WebResponse webRespon = httpReq.GetResponse(); //获取服务器端的响应
                Stream s = webRespon.GetResponseStream();

                StreamReader sr = new StreamReader(s);
                String sReturnString = sr.ReadLine();
                s.Close();
                sr.Close();
                if (sReturnString == "Success")
                {
                    Console.WriteLine("上传成功!");
                    returnValue = 1;
                }
                else if (sReturnString == "Error")
                {
                    returnValue = 0;
                }
            }
            catch
            {
                returnValue = 0;
            }
            finally
            {
                fs.Close();
                binaryReader.Close();
            }
            return returnValue;
        }

        private class EnumMap<T>
        {
            private Dictionary<T, object> values { get; set; }
            public EnumMap()
            {
                values = new Dictionary<T, object>();
                Enum.GetValues(typeof(T)).Cast<T>().ToList().ForEach(x => values.Add(x, new object()));
            }
            public object this[T index]
            {
                get { return values[index]; }
                set { values[index] = value; }
            }
        }
    }


    public class DataResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public List<short[]> data { get; set; }

        public static string ZoncareJsonSerialize(DataResponse responseContent)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();

                writer.WritePropertyName("code");
                if (string.IsNullOrEmpty(responseContent.code))
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteValue(responseContent.code);
                }

                writer.WritePropertyName("message");
                if (string.IsNullOrEmpty(responseContent.message))
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteValue(responseContent.message);
                }

                if (responseContent.data != null)
                {
                    writer.WritePropertyName("data");
                    writer.WriteStartArray();
                    foreach (short[] arrItem in responseContent.data)
                    {

                        writer.WriteStartArray();
                        foreach (short dataVal in arrItem)
                        {
                            writer.WriteValue(dataVal);
                        }
                        writer.WriteEndArray();
                    }
                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }

            return sb.ToString();
        }

        public static string ZoncareJsonPackage(DataResponse responseContent)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            sb.Append($"\"rspCode\":\"{responseContent.code}\"").Append(",");
            sb.Append($"\"rspMsg\":\"{responseContent.message}\"").Append(",");
            sb.Append($"\"data\":");
            sb.Append("[");
            foreach (short[] dataArr in responseContent.data)
            {
                sb.Append("[");
                for (int i = 0; i < dataArr.Length; i++)
                {
                    sb.Append(dataArr[i]);
                    if (i < dataArr.Length - 1) sb.Append(",");
                }

                sb.Append("]");
            }
            sb.Append("]");

            return sb.ToString();
        }

    }
}
