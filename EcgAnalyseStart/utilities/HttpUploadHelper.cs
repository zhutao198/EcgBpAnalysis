using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    public static class HttpUploadHelper
    {
        private static readonly Encoding DEFAULT_ENCODE = Encoding.UTF8;
        private const int REQUEST_TIMEOUT = 36000000; //设置获得响应的超时时间（10小时）
        private const int BUFF_SIZE = 4096; //文件分块大小(4KB)
        private const int CHUNK_SIZE = BUFF_SIZE * 1024;

        /// <summary>
        /// HttpUploadFile
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HttpUploadFile(string url, string file, NameValueCollection data)
        {
            return HttpUploadFile(url, file, data, DEFAULT_ENCODE);
        }

        /// <summary>
        /// HttpUploadFile
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string HttpUploadFile(string url, string file, NameValueCollection data, Encoding encoding)
        {
            return HttpUploadFile(new string[] { file }, data, encoding, null);
        }

        /// <summary>
        /// HttpUploadFile
        /// </summary>
        /// <param name="url"></param>
        /// <param name="files"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HttpUploadFile(string[] files, NameValueCollection data, DataTransfer transfer)
        {
            return HttpUploadFile(files, data, DEFAULT_ENCODE, transfer);
        }

        /// <summary>
        /// HttpUploadFile
        /// </summary>
        /// <param name="url"></param>
        /// <param name="files"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string HttpUploadFile(string[] files, NameValueCollection data, Encoding encoding, DataTransfer transfer)
        {
            string strReturn = string.Empty;
            string boundary = "------------" + DateTime.Now.Ticks.ToString("x");
            string actualBoundary = "\r\n--" + boundary + "\r\n";
            byte[] boundarybytes = Encoding.UTF8.GetBytes(actualBoundary);
            byte[] endbytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            #region prepare binary data to write into Request Stream
            byte[] keyValueBytes = null; //文本键值对字节数组
            byte[][] fileHeaderBytes = new byte[files.Length][]; //数组(包含各文件头信息的字节数组)
            long sentLength = 0, contentLength = 0; //已发送的字节数, 待发送的总字节数

            string keyValueTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            if (data != null)
            {
                StringBuilder sbKeyValue = new StringBuilder();
                foreach (string key in data.Keys)
                {
                    string keyValueItem = string.Format(keyValueTemplate, key, data[key]);
                    sbKeyValue.Append(actualBoundary);
                    sbKeyValue.Append(keyValueItem);
                }
                keyValueBytes = encoding.GetBytes(sbKeyValue.ToString());
                contentLength += keyValueBytes.Length;
            }

            string fileHeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
            for (int i = 0; i < files.Length; i++)
            {
                string fileHeaderString = string.Format(fileHeaderTemplate, "file", Path.GetFileName(files[i]));
                string fileHeader = actualBoundary + fileHeaderString;
                fileHeaderBytes[i] = encoding.GetBytes(fileHeader);
                contentLength += fileHeaderBytes[i].Length;

                FileInfo fi = new FileInfo(files[i]);
                contentLength += fi.Length;
            }
            contentLength += endbytes.Length;
            transfer.StartTransfer(contentLength);
            #endregion

            try
            {
                //1.HttpWebRequest
                HttpWebRequest request = transfer.TransferRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.AllowWriteStreamBuffering = false;
                request.SendChunked = true; //支持分块上传以获取上传进度, 此时无需指定ContentLength
                //request.ContentLength = contentLength;
                request.ReadWriteTimeout = REQUEST_TIMEOUT;

                Console.WriteLine("start upload -- " + DateTime.Now.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    //1.1 key/value
                    if (keyValueBytes != null)
                    {
                        stream.Write(keyValueBytes, 0, keyValueBytes.Length);
                        sentLength += keyValueBytes.Length;
                    }

                    //1.2 file
                    byte[] buffer = new byte[BUFF_SIZE];
                    int bytesRead = 0;
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (fileHeaderBytes[i] != null)
                        {
                            stream.Write(fileHeaderBytes[i], 0, fileHeaderBytes[i].Length);
                            sentLength += fileHeaderBytes[i].Length;
                        }

                        using (FileStream fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                        {
                            int offset = transfer.TransferredBlockCount * BUFF_SIZE;
                            while ((bytesRead = fileStream.Read(buffer, offset, buffer.Length)) != 0)
                            {
                                stream.Write(buffer, 0, bytesRead);
                                stream.Flush();
                                sentLength += bytesRead;
                                if (transfer != null) transfer.TransferredLength = sentLength;
                                //Console.WriteLine("已发送{0}%", (int)(((double)offset / contentLength) * 100d));
                            }
                        }
                    }

                    //1.3 form end
                    stream.Write(endbytes, 0, endbytes.Length);
                    sentLength += endbytes.Length;
                }

                //2.WebResponse
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader stream = new StreamReader(response.GetResponseStream(), encoding))
                {
                    strReturn = stream.ReadToEnd();
                }
                Console.WriteLine("end upload -- " + DateTime.Now.ToString());
            }
            catch (WebException ex)
            {
                transfer.TransferredLength = 0;
                var exResponse = ex.Response as HttpWebResponse;
                var responseStream = exResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    Console.WriteLine(streamReader.ReadToEnd());
                    streamReader.Close();
                    responseStream.Close();
                }
                throw;
            }
            catch (Exception ex)
            {
                transfer.TransferredLength = 0;
                Console.WriteLine(ex.Message);
                throw;
            }
            return strReturn;
        }

        public static string HttpUploadFileByChunk(string[] files, List<MultipartData> data, DataTransferHandlerNew transfer)
        {
            string strReturn = string.Empty;

            string remoteAddr = transfer.DestinationAddress; //http://192.168.7.18:9000
            bool filesUploaded = false;
            foreach (string filePath in files)
            {
                bool chunksUploaded = true;
                FileInfo fi = new FileInfo(filePath);
                if (fi.Extension.ToLower() != ".dat") continue;

                //1. 文件上传前的注册
                string fileMD5 = FileOperator.GetFileMD5(fi.FullName);
                List<MultipartData> paraCheckFile = new List<MultipartData>(){
                    new MultipartData("fileMd5", fileMD5),
                    new MultipartData("fileName", fi.Name),
                    new MultipartData("fileSize", fi.Length.ToString()),
                    new MultipartData("mimetype", ""),
                    new MultipartData("fileExt", fi.Extension.TrimStart('.'))
                };
                string registerInfo = PostFormMultipart(remoteAddr + "/holter/upload/register", paraCheckFile, null);
                if (HttpService.IsResponseOK(registerInfo, out string code1, out string msg1))
                {
                    int chunkCount = (int)((fi.Length + CHUNK_SIZE - 1) / CHUNK_SIZE);
                    for (int i = 0; i < chunkCount; i++)
                    {
                        if (transfer.IsCancelled) //上传被取消
                        {
                            chunksUploaded = false;
                            break;
                        }

                        //2. 检查文件分块
                        //string chunkMD5 = FileOperator.GetChunkMD5(fi.FullName, CHUNK_SIZE, i);
                        List<MultipartData> paraCheckChunk = new List<MultipartData>(){
                            new MultipartData("fileMd5", fileMD5),
                            new MultipartData("chunk", i.ToString()),
                            new MultipartData("chunkSize", chunkCount.ToString())
                        };
                        string chunkCheckInfo = PostFormMultipart(remoteAddr + "/holter/upload/checkChunk", paraCheckChunk, null);
                        if (!HttpService.IsResponseOK(chunkCheckInfo, out string code2, out string msg2))
                        {
                            //3. 上传分块文件
                            NameValueCollection paraUploadChunk = new NameValueCollection();
                            paraUploadChunk.Add("fileMd5", fileMD5);
                            paraUploadChunk.Add("chunk", i.ToString());
                            string chunkUploadInfo = PostFileChunk(transfer, remoteAddr + "/holter/upload/uploadChunk", paraUploadChunk, null, filePath, i);
                            if (!HttpService.IsResponseOK(chunkUploadInfo, out string code3, out string msg3))
                            {
                                chunksUploaded = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    chunksUploaded = false;
                }

                //4. 合并上传的文件
                if (!chunksUploaded) { break; }

                FileInfo fiINI = null;
                foreach (string fPath in files)
                {
                    FileInfo fInfo = new FileInfo(fPath);
                    if (fInfo.Extension.ToLower() == ".ini") { fiINI = fInfo; break; }
                }

                List<MultipartData> paraMergeFile = new List<MultipartData>() {
                    new MultipartData("fileMd5", fileMD5),
                    new MultipartData("fileName", fi.Name),
                    new MultipartData("fileSize", fi.Length.ToString()),
                    new MultipartData("mimetype", ""),
                    new MultipartData("fileExt", fi.Extension.TrimStart('.')),
                };
                paraMergeFile.AddRange(data);
                paraMergeFile.Add(new MultipartData("iniFile", "")
                {
                    IsFile = true,
                    FileName = fiINI.Name,
                    FilePath = fiINI.FullName
                });
                string mergeInfo = PostFormMultipart(remoteAddr + "/holter/upload/mergeChunks", paraMergeFile, null);
                if (HttpService.IsResponseOK(mergeInfo, out string code4, out string msg4))
                {
                    filesUploaded = true;
                    break;
                }
            }

            if (filesUploaded) //所有文件成功上传后发送其余相关信息
            {
                //strReturn = PostFormMultipart(remoteAddr + "/holter/upload/mergeChunks", data, null);
                strReturn = "<root><rec>200</rec><msg>上传完毕</msg></root>";
            }

            return strReturn;
        }

        /// <summary>
        /// 默认表单提交
        /// </summary>
        /// <param name="request">WebRequest类</param>
        /// <param name="postData">提交数据</param>
        /// <param name="cookie">Cookie容器对象</param>
        /// <returns>字符串结果</returns>
        public static string PostForm(string uri, NameValueCollection postData, CookieContainer cookie)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = "post";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.CookieContainer = cookie ?? new CookieContainer();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string key in postData.Keys)
            {
                stringBuilder.AppendFormat("&{0}={1}", key, postData.Get(key));
            }
            byte[] buffer = Encoding.UTF8.GetBytes(stringBuilder.ToString().Trim('&'));
            Stream requestStream = webRequest.GetRequestStream();
            requestStream.Write(buffer, 0, buffer.Length);
            requestStream.Close();

            WebResponse response = webRequest.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 多部件表单提交
        /// </summary>
        /// <param name="webrequest">WebRequest类</param>
        /// <param name="postData">提交数据.注:如果是文件路径,代表是文件.</param>
        /// <param name="cookie">Cookie容器对象</param>
        /// <returns>字符串结果</returns>
        public static string PostFormMultipart(string uri, List<MultipartData> postData, CookieContainer cookie)
        {
            string strReturn = string.Empty;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);

                string boundary = string.Format("-----{0}", DateTime.Now.Ticks.ToString("x"));
                string actualBoundary = "\r\n--" + boundary + "\r\n";

                webRequest.UseDefaultCredentials = true;
                webRequest.CookieContainer = cookie ?? new CookieContainer();
                webRequest.Timeout = 120000;
                webRequest.Method = "POST";
                webRequest.ContentType = string.Format("multipart/form-data;charset=utf-8;boundary={0}", boundary);

                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (MultipartData entry in postData)
                    {
                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.Append(actualBoundary);
                        strBuilder.AppendFormat("Content-Disposition: form-data; name=\"{0}\"", entry.ParaKey);
                        if (entry.IsFile)
                        {
                            strBuilder.AppendFormat(";filename=\"{0}\"\r\nContent-Type: application/octet-stream\r\n\r\n", entry.FileName);
                            byte[] buffer = Encoding.UTF8.GetBytes(strBuilder.ToString());
                            requestStream.Write(buffer, 0, buffer.Length);

                            //获取文件流
                            using (FileStream fileStream = new FileStream(entry.FilePath, FileMode.Open, FileAccess.Read))
                            {
                                //using (BinaryReader binaryReader = new BinaryReader(fileStream))
                                //{
                                //    byte[] fileBuffer = binaryReader.ReadBytes((int)fileStream.Length);
                                //    requestStream.Write(fileBuffer, 0, fileBuffer.Length);
                                //    binaryReader.Close();
                                //}

                                fileStream.CopyTo(requestStream);
                                fileStream.Close();
                            }
                        }
                        else
                        {
                            strBuilder.AppendFormat("\r\n\r\n{0}", entry.ParaValue);
                            byte[] buff = Encoding.UTF8.GetBytes(strBuilder.ToString());
                            requestStream.Write(buff, 0, buff.Length);
                        }
                        sb.Append(strBuilder.ToString());
                    }

                    //发送结尾标识
                    byte[] boundaryBuffer = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));
                    requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }

                using (HttpWebResponse httpResponse = (HttpWebResponse)webRequest.GetResponse())
                using (Stream stream = httpResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    strReturn = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (WebException ex)
            {
                var exResponse = ex.Response as HttpWebResponse;
                var responseStream = exResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    Console.WriteLine(streamReader.ReadToEnd());
                    streamReader.Close();
                    responseStream.Close();
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return strReturn;
        }

        public static string PostFormMultipart(DataTransferHandlerNew transfer, List<MultipartData> postData)
        {
            string strReturn = string.Empty;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(transfer.DestinationAddress);

                string boundary = string.Format("-----{0}", DateTime.Now.Ticks.ToString("x"));
                string actualBoundary = "\r\n--" + boundary + "\r\n";

                webRequest.UseDefaultCredentials = true;
                webRequest.CookieContainer = new CookieContainer();
                webRequest.Timeout = REQUEST_TIMEOUT;
                webRequest.Method = "POST";
                webRequest.Proxy = null;
                webRequest.ContentType = string.Format("multipart/form-data;charset=utf-8;boundary={0}", boundary);
                transfer.TransferRequest = webRequest;

                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (MultipartData entry in postData)
                    {
                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.Append(actualBoundary);
                        strBuilder.AppendFormat("Content-Disposition: form-data; name=\"{0}\"", entry.ParaKey);
                        if (entry.IsFile)
                        {
                            strBuilder.AppendFormat(";filename=\"{0}\"\r\nContent-Type: application/octet-stream\r\n\r\n", entry.FileName);
                            byte[] buffer = Encoding.UTF8.GetBytes(strBuilder.ToString());
                            requestStream.Write(buffer, 0, buffer.Length);

                            //获取文件流
                            using (FileStream fileStream = new FileStream(entry.FilePath, FileMode.Open, FileAccess.Read))
                            {
                                //using (BinaryReader binaryReader = new BinaryReader(fileStream))
                                //{
                                //    byte[] fileBuffer = binaryReader.ReadBytes((int)fileStream.Length);
                                //    requestStream.Write(fileBuffer, 0, fileBuffer.Length);
                                //    binaryReader.Close();
                                //}

                                fileStream.CopyTo(requestStream);
                                if (transfer != null)
                                {
                                    transfer.TransferredLength += fileStream.Length;
                                }

                                fileStream.Close();
                            }
                        }
                        else
                        {
                            strBuilder.AppendFormat("\r\n\r\n{0}", entry.ParaValue);
                            byte[] buff = Encoding.UTF8.GetBytes(strBuilder.ToString());
                            requestStream.Write(buff, 0, buff.Length);
                        }
                        sb.Append(strBuilder.ToString());
                    }

                    //发送结尾标识
                    byte[] boundaryBuffer = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));
                    requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }

                using (HttpWebResponse httpResponse = (HttpWebResponse)webRequest.GetResponse())
                using (Stream stream = httpResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    strReturn = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (WebException ex)
            {
                var exResponse = ex.Response as HttpWebResponse;
                var responseStream = exResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    Console.WriteLine(streamReader.ReadToEnd());
                    streamReader.Close();
                    responseStream.Close();
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return strReturn;
        }

        public static string PostFileChunk(DataTransferHandlerNew transfer, string uri, NameValueCollection postData, CookieContainer cookie, string filePath, int chunkIndex)
        {
            string strReturn = string.Empty;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);

                string boundary = string.Format("-----{0}", DateTime.Now.Ticks.ToString("x"));
                string actualBoundary = "\r\n--" + boundary + "\r\n";
                webRequest.UseDefaultCredentials = true;
                webRequest.CookieContainer = cookie ?? new CookieContainer();
                webRequest.ReadWriteTimeout = REQUEST_TIMEOUT;
                webRequest.Method = "POST";
                webRequest.SendChunked = true;
                webRequest.ContentType = string.Format("multipart/form-data;charset=utf-8;boundary={0}", boundary);
                transfer.TransferRequest = webRequest;

                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    //发送键值对
                    foreach (string key in postData.Keys)
                    {
                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.Append(actualBoundary);
                        strBuilder.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", key, postData.Get(key));
                        byte[] buff = Encoding.UTF8.GetBytes(strBuilder.ToString());
                        requestStream.Write(buff, 0, buff.Length);
                    }

                    //发送文件块
                    StringBuilder strBuilder2 = new StringBuilder();
                    strBuilder2.Append(actualBoundary);
                    strBuilder2.AppendFormat("Content-Disposition: form-data; name=\"{0}\"", "file");
                    strBuilder2.AppendFormat(";filename=\"{0}\"\r\nContent-Type: application/octet-stream\r\n\r\n", Path.GetFileName(filePath)); //multipart/form-data application/octet-stream
                    byte[] buffer = Encoding.UTF8.GetBytes(strBuilder2.ToString());
                    requestStream.Write(buffer, 0, buffer.Length);

                    byte[] sendBuffer = new byte[BUFF_SIZE];
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        transfer.StartTransfer(fs.Length);
                        long offset = (long)CHUNK_SIZE * chunkIndex;
                        if (offset < fs.Length)
                            fs.Position = offset;
                        else
                            fs.Position = fs.Length - 1;

                        //对文件块Chunk再分成小片进行传输(每小片大小: BUFF_SIZE)
                        int sentBytes = 0;
                        while (sentBytes < CHUNK_SIZE)
                        {
                            int readBytes = fs.Read(sendBuffer, 0, BUFF_SIZE);
                            if (readBytes > 0)
                            {
                                int writeBytes = readBytes;
                                if (sentBytes + readBytes > CHUNK_SIZE) { writeBytes = CHUNK_SIZE - sentBytes; }

                                requestStream.Write(sendBuffer, 0, writeBytes);
                                requestStream.Flush();
                                sentBytes += writeBytes;

                                //获取已上传百分比
                                if (transfer != null && !filePath.EndsWith(".ini"))
                                {
                                    transfer.TransferredLength = offset + sentBytes;
                                }
                                //Console.WriteLine(transfer.TransferredPercent + "%");
                            }
                            else { break; }
                        }
                    }

                    //发送结尾标识
                    byte[] boundaryBuffer = Encoding.UTF8.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));
                    requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                    requestStream.Close();
                }

                using (HttpWebResponse httpResponse = (HttpWebResponse)webRequest.GetResponse())
                using (Stream stream = httpResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    strReturn = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (WebException ex)
            {
                var exResponse = ex.Response as HttpWebResponse;
                var responseStream = exResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    Console.WriteLine(streamReader.ReadToEnd());
                    streamReader.Close();
                    responseStream.Close();
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return strReturn;
        }

    }

    public class MultipartData
    {
        public string ParaKey { get; set; }
        public string ParaValue { get; set; }
        public bool IsFile { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public MultipartData(string key, string value)
        {
            ParaKey = key;
            ParaValue = value;
        }

        public MultipartData(string key, string value, bool isFile, string filePath, string fileName)
        {
            ParaKey = key;
            ParaValue = value;
            IsFile = isFile;
            FilePath = filePath;
            FileName = fileName;
        }
    }
}
