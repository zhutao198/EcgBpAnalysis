using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Zoncare.Utilities
{
    public class HttpService
    {
        /// <summary>
        /// 尝试访问指定的url, 确定其是否可正常访问
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static bool CheckUrlavailability(string url, string cookie)
        {
            bool available = false;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Method = "HEAD";  //设置提交方式可以为"get","head"等
                myRequest.Timeout = 1000;  //设置网页响应时间长度
                //myRequest.AllowAutoRedirect = false; //是否允许自动重定向
                myRequest.UseDefaultCredentials = true;
                if (cookie != string.Empty)
                {
                    myRequest.Headers.Add("cookie", cookie);
                }
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                available = (myResponse.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
            }
            return available;
        }

        /// <summary>
        /// 常规方式访问并获取结果
        /// </summary>
        /// <returns></returns>
        public static string GetWebRequest(string uri, string method, List<KeyValuePair<string, string>> lstPara, out string errMsg)
        {
            errMsg = "";
            string strPara = string.Empty;
            if (lstPara != null)
            {
                foreach (var para in lstPara)
                {
                    strPara += $"&{para.Key}={para.Value}";
                }
                strPara = strPara.Remove(0, 1);
            }

            string result = "";
            try
            {
                if (method.Equals("GET", StringComparison.InvariantCultureIgnoreCase)) { uri += "?" + strPara; }

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                if (null != httpWebRequest)
                {
                    httpWebRequest.UseDefaultCredentials = true;
                    httpWebRequest.Method = method;

                    if (method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
                    {
                        httpWebRequest.ContentType = "application/x-www-form-urlencoded";

                        byte[] byteArray = Encoding.UTF8.GetBytes(strPara);
                        httpWebRequest.ContentLength = byteArray.Length;
                        using (Stream dataStream = httpWebRequest.GetRequestStream())
                        {
                            dataStream.Write(byteArray, 0, byteArray.Length);
                        }
                    }

                    using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        result = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }
            catch (WebException ex)
            {
                var exResponse = ex.Response as HttpWebResponse;
                var responseStream = exResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    string respText = streamReader.ReadToEnd();
                    Console.WriteLine(respText);
                    streamReader.Close();
                    responseStream.Close();
                }
                string stackInfo = new System.Diagnostics.StackTrace().ToString(); //记录调用堆栈信息
                errMsg = ex.Message + Environment.NewLine + stackInfo;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
            }

            return result;
        }

        /// <summary>
        /// 访问接口并获取相应信息
        /// </summary>
        /// <param name="uri">访问地址</param>
        /// <param name="method">访问方式(get/post)</param>
        /// <param name="lstHeader">http标头参数列表</param>
        /// <param name="json">json传参</param>
        /// /// <param name="errMsg">输出的错误信息</param>
        /// <returns></returns>
        public static string GetWebRequest(string uri, string method, List<KeyValuePair<string, string>> lstHeader, string json, out string errMsg)
        {
            errMsg = "";
            string result = string.Empty;
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
                if (null != httpWebRequest)
                {
                    httpWebRequest.UseDefaultCredentials = true;
                    httpWebRequest.Method = method.ToUpper();
                    httpWebRequest.ContentType = "application/json;charset=UTF-8";

                    //if (!string.IsNullOrWhiteSpace(CommonValues.WebServiceToken))
                    //{
                    //    httpWebRequest.Headers["Authorization"] = CommonValues.WebServiceToken;
                    //}
                    if (null != lstHeader && lstHeader.Count > 0)
                    {
                        foreach (var hf in lstHeader)
                            httpWebRequest.Headers[hf.Key] = hf.Value;
                    }
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        byte[] bys = Encoding.UTF8.GetBytes(json);
                        if (null != bys && bys.Length > 0)
                        {
                            httpWebRequest.ContentLength = bys.Length;
                            Stream strStream = httpWebRequest.GetRequestStream();
                            strStream.Write(bys, 0, bys.Length);
                            strStream.Close();
                        }
                    }

                    using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        result = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }
            catch (WebException ex)
            {
                var exResponse = ex.Response as HttpWebResponse;
                var responseStream = exResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    string respText = streamReader.ReadToEnd();
                    Console.WriteLine(respText);
                    streamReader.Close();
                    responseStream.Close();
                }
                string stackInfo = new System.Diagnostics.StackTrace().ToString(); //记录调用堆栈信息
                errMsg = ex.Message + Environment.NewLine + stackInfo;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
            }
            return result;
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="method">访问方式(get/post)</param>
        /// <param name="uri">访问的接口地址</param>
        /// <param name="file">要上传的文件</param>
        /// <param name="values">请求的表单参数信息</param>
        /// <returns></returns>
        public static string UploadFiles(string uri, string method, List<UploadFile> files, NameValueCollection values, out string errMsg)
        {
            errMsg = "";
            string result = "";
            try
            {
                HttpWebRequest _HttpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                if (null != _HttpWebRequest)
                {
                    _HttpWebRequest.UseDefaultCredentials = true;
                    _HttpWebRequest.Method = method;
                    var boundary = "-----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
                    _HttpWebRequest.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;

                    //if (!string.IsNullOrWhiteSpace(CommonValues.WebServiceToken))
                    //{
                    //    _HttpWebRequest.Headers["Authorization"] = CommonValues.WebServiceToken;
                    //}

                    boundary = "--" + boundary;

                    using (var requestStream = _HttpWebRequest.GetRequestStream())
                    {
                        if (values != null)
                        {
                            foreach (string name in values.Keys)
                            {
                                var buffer = Encoding.UTF8.GetBytes(boundary + Environment.NewLine);
                                requestStream.Write(buffer, 0, buffer.Length);
                                buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition:form-data;name=\"{0}\"{1}{1}", name, Environment.NewLine));
                                requestStream.Write(buffer, 0, buffer.Length);
                                buffer = Encoding.UTF8.GetBytes(values[name] + Environment.NewLine);
                                requestStream.Write(buffer, 0, buffer.Length);
                            }
                        }

                        foreach (UploadFile file in files)
                        {
                            var buffer = Encoding.UTF8.GetBytes(boundary + Environment.NewLine);
                            requestStream.Write(buffer, 0, buffer.Length);

                            buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition:form-data;name=\"{0}\";filename=\"{1}\"{2}", file.Name, file.Filename, Environment.NewLine));
                            requestStream.Write(buffer, 0, buffer.Length);

                            buffer = Encoding.UTF8.GetBytes(string.Format("Content-Type:{0}{1}{1}", file.ContentType, Environment.NewLine));
                            requestStream.Write(buffer, 0, buffer.Length);

                            file.Stream.CopyTo(requestStream);

                            buffer = Encoding.UTF8.GetBytes(Environment.NewLine);
                            requestStream.Write(buffer, 0, buffer.Length);
                        }

                        byte[] boundaryBuffer = Encoding.UTF8.GetBytes(boundary + "--");
                        requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);

                        requestStream.Close();
                        requestStream.Dispose();
                    }

                    using (var response = _HttpWebRequest.GetResponse())
                    using (var responseStream = response.GetResponseStream())
                    using (var stream = new MemoryStream())
                    {
                        responseStream.CopyTo(stream);
                        result = Encoding.UTF8.GetString(stream.ToArray());
                    }
                }
            }
            catch (WebException ex)
            {
                var exResponse = ex.Response as HttpWebResponse;
                var responseStream = exResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    string respText = streamReader.ReadToEnd();
                    Console.WriteLine(respText);
                    streamReader.Close();
                    responseStream.Close();
                }
                string stackInfo = new System.Diagnostics.StackTrace().ToString(); //记录调用堆栈信息
                errMsg = ex.Message + Environment.NewLine + stackInfo;
                result = "";
            }
            catch (Exception ex)
            {
                errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                result = "";
            }
            finally
            {
                foreach (var file in files)
                {
                    file.Stream.Close();
                    file.Stream.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="method">访问方式(get/post)</param>
        /// <param name="uri">访问的接口地址</param>
        /// <param name="destFilePath">下载后文件的保存路径</param>
        /// <param name="json">下载文件的请求参数信息</param>
        /// <returns></returns>
        public static string DownloadFile(string uri, string method, string destFilePath, string json, out string errMsg)
        {
            errMsg = "";
            string result = "";
            try
            {
                HttpWebRequest _HttpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                if (null != _HttpWebRequest)
                {
                    _HttpWebRequest.UseDefaultCredentials = true;
                    _HttpWebRequest.Method = method;
                    _HttpWebRequest.ContentType = "application/json";

                    //if (!string.IsNullOrWhiteSpace(CommonValues.WebServiceToken))
                    //{
                    //    _HttpWebRequest.Headers["Authorization"] = CommonValues.WebServiceToken;
                    //}

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        byte[] bys = Encoding.UTF8.GetBytes(json);
                        if (null != bys && bys.Length > 0)
                        {
                            _HttpWebRequest.ContentLength = bys.Length;
                            Stream reqStream = _HttpWebRequest.GetRequestStream();
                            reqStream.Write(bys, 0, bys.Length);
                            reqStream.Close();
                        }
                    }

                    int iTotalSize = 0;
                    string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); //Path.GetTempFileName
                    using (FileStream fs = new FileStream(tempFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        //发送请求并获取相应回应数据
                        HttpWebResponse response = _HttpWebRequest.GetResponse() as HttpWebResponse;
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            //创建本地文件写入流
                            byte[] bArr = new byte[1024];
                            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                            while (size > 0)
                            {
                                iTotalSize += size;
                                fs.Write(bArr, 0, size);
                                size = responseStream.Read(bArr, 0, (int)bArr.Length);
                            }

                            responseStream.Close();
                        }
                        fs.Close();
                    }

                    if (iTotalSize > 0)
                    {
                        string dirPath = Path.GetDirectoryName(destFilePath);
                        if (!Directory.Exists(dirPath)) { Directory.CreateDirectory(dirPath); }
                        File.Copy(tempFilePath, destFilePath, true);
                        File.Delete(tempFilePath);
                        result = "200";
                    }
                    else
                    {
                        result = "400"; //服务端未找到要下载的文件
                    }
                }
            }
            catch (WebException ex)
            {
                var exResponse = ex.Response as HttpWebResponse;
                var responseStream = exResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    string respText = streamReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(respText))
                    {
                        result = getResponseData(respText, "message", "500");
                    }
                    streamReader.Close();
                    responseStream.Close();
                }

                if (string.IsNullOrEmpty(result)) result = "400";
                string stackInfo = new System.Diagnostics.StackTrace().ToString(); //记录调用堆栈信息
                errMsg = ex.Message + Environment.NewLine + stackInfo;
            }
            catch (Exception ex)
            {
                //if (File.Exists(filePath)) { File.Delete(filePath); }
                errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                result = "400";
            }
            return result;
        }


        /// <summary>
        /// 根据请求返回结果获取其相关的内容或数据
        /// </summary>
        /// <param name="jsonResult">请求返回的结果</param>
        /// <param name="key">json文本的标签</param>
        /// <param name="conditionCode">返回数据的状态条件(正常情况:200; 其它情况:400/500/...;)</param>
        /// <returns></returns>
        private static string getResponseData(string jsonResult, string key, string conditionCode = "200")
        {
            string strData = "";

            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonResult);
            if ("200" == conditionCode)
            {
                string code = jo["code"].ToString();
                if (conditionCode == code.Trim())
                {
                    strData = jo[key]?.ToString();
                }
            }
            else
            {
                if (jo["code"] != null)
                {
                    strData = jo["code"].ToString();
                }
                else
                {
                    string message = jo.GetValue(key, StringComparison.InvariantCultureIgnoreCase)?.ToString();
                    if (!string.IsNullOrEmpty(message))
                    {
                        bool isCodeIn = System.Text.RegularExpressions.Regex.IsMatch(message, @"^\d+:");
                        if (isCodeIn)
                        {
                            string[] msgs = message.Split(':');
                            string errorCode = msgs[0];
                            string errorText = msgs[1];
                            strData = errorCode;
                        }
                    }
                }
            }
            return strData;
        }

        /// <summary>
        /// 检查向第三方上传文件时标准接口返回的消息
        /// </summary>
        /// <param name="respInfo"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool IsResponseOK(string respInfo, out string msg)
        {
            bool result = false;
            msg = "";

            if (!string.IsNullOrEmpty(respInfo))
            {
                try
                {
                    JObject jo = (JObject)JsonConvert.DeserializeObject(respInfo);
                    if (jo.ContainsKey("rec"))
                    {
                        string code = jo["rec"].ToString();
                        msg = jo["msg"]?.ToString();
                        if ("200" == code)
                        {
                            result = true;
                            if (jo["data"] != null) //检查具体的返回信息
                            {
                                JToken objData = jo["data"];
                                if (objData.SelectToken("msg") != null) msg = objData["msg"].ToString();
                                if (objData.SelectToken("code") != null)
                                {
                                    if ("200" != objData["code"].ToString())
                                    {
                                        result = false;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// 检查Zoncare服务端响应
        /// </summary>
        /// <param name="respInfo"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool IsResponseOK(string respInfo, out string code, out string msg)
        {
            bool result = false;
            code = "400";
            msg = "";

            if (!string.IsNullOrEmpty(respInfo))
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(respInfo);
                if (jo.ContainsKey("code"))
                {
                    code = jo["code"].ToString();
                    msg = jo["msg"]?.ToString();
                    if ("200" == code)
                    {
                        result = true;
                        if (jo["data"] != null) //检查具体的返回信息
                        {
                            JToken objData = jo["data"];
                            if (objData.SelectToken("msg") != null) msg = objData["msg"].ToString();
                            if (objData.SelectToken("code") != null)
                            {
                                code = objData["code"].ToString();
                                if ("200" != code)
                                {
                                    result = false;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

    }

    /// <summary>
    /// 上传的文件对象
    /// </summary>
    public class UploadFile
    {
        public string Name { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public Stream Stream { get; set; }
    }
}