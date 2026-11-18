using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Zoncare.Common;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    public class DataTransferHandlerNew : DataTransfer
    {
        //分块上传血压文件
        public int UploadData_ABPM(string serverAddr, string examId, string userId, string localFileName)
        {
            int returnValue = 0;

            this.DestinationAddress = serverAddr;

            List<MultipartData> data = new List<MultipartData>();
            data.Add(new MultipartData("id", examId));
            data.Add(new MultipartData("userId", userId));
            string respText = HttpUploadHelper.HttpUploadFileByChunk(new string[] { localFileName }, data, this);
            if (!string.IsNullOrEmpty(respText))
            {
                try
                {
                    //XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.LoadXml(respText);
                    //if ("200" == xmlDoc.GetElementsByTagName("rec")[0].InnerText)
                    bool result = HttpService.IsResponseOK(respText, out string code, out string msg);
                    if (result == true)
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

            return returnValue;
        }

        //分块上传心电文件
        public int UploadData_ECG(string serverAddr, string examId, string userId, string localDatFileName, string iniFileName)
        {
            int returnValue = 0;

            this.DestinationAddress = serverAddr;

            List<MultipartData> data = new List<MultipartData>(){
                new MultipartData("examId", examId),
                //new MultipartData("userId",userId)
            };

            List<string> lstFiles = new List<string>() { localDatFileName };
            if (!string.IsNullOrWhiteSpace(iniFileName)) lstFiles.Add(iniFileName);

            string sReturnString = HttpUploadHelper.HttpUploadFileByChunk(lstFiles.ToArray(), data, this);
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
                    throw new Exception("无法解析远程服务器的应答信息");
                }
            }

            return returnValue;
        }

        //上传8337血压监测设备内的相关文件
        public string UploadDataFor8337(string serverAddr, string examId, string userId, string mac, List<string> files, string ignoreCRC = "")
        {
            string returnCode = "200";

            long contentLength = 0;
            List<MultipartData> data = new List<MultipartData>();
            data.Add(new MultipartData("id", examId));
            data.Add(new MultipartData("userId", userId));
            data.Add(new MultipartData("mac", mac));
            foreach (string filePath in files)
            {
                FileInfo fi = new FileInfo(filePath);
                contentLength += fi.Length;
                data.Add(new MultipartData("files", fi.FullName, true, fi.FullName, fi.Name));
            }
            data.Add(new MultipartData("ignoreCRC", ignoreCRC));

            this.DestinationAddress = serverAddr;
            this.FileNum = files.Count;
            this.StartTransfer(contentLength);

            //上传
            string strReturn = HttpUploadHelper.PostFormMultipart(this, data);
            bool isOK = HttpService.IsResponseOK(strReturn, out string code, out string msg);
            if (isOK)
            {
                this.TransferCompleted = true;
            }
            returnCode = code;

            return returnCode;
        }

        /// <summary>
        /// 下载数据文件并解压
        /// </summary>
        /// <param name="uri">服务端地址</param>
        /// <param name="localZipFile">下载的数据压缩包文件</param>
        /// <param name="extractPath">文件解压的目录路径</param>
        /// <param name="downloadProgress"></param>
        /// <returns></returns>
        public static async Task<bool> DownloadFile(string uri, string localZipFile, string extractPath, WorkProgress downloadProgress)
        {
            bool result = false;
            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;
                    if (!Directory.Exists(extractPath)) Directory.CreateDirectory(extractPath);

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = new FileStream(
                        localZipFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        byte[] buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            // 更新进度
                            if (totalBytes.HasValue)
                            {
                                downloadProgress.percent = (int)(totalRead * 100 / totalBytes.Value);
                            }
                        }
                    }
                }

                using (ZipArchive archive = ZipFile.OpenRead(localZipFile))
                {
                    downloadProgress.code = "202";
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // 目录条目（以 / 结尾）跳过
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        string destinationPath = Path.Combine(extractPath, entry.FullName);

                        // 防止 Zip Slip 路径穿越攻击
                        string fullDest = Path.GetFullPath(destinationPath);
                        //if (!fullDest.StartsWith(Path.GetFullPath(extractPath) + Path.DirectorySeparatorChar))
                        //    throw new IOException("检测到非法路径: " + entry.FullName);

                        // 确保子目录存在
                        //Directory.CreateDirectory(Path.GetDirectoryName(fullDest));

                        // 解压单个文件
                        entry.ExtractToFile(fullDest, overwrite: true);
                    }
                }
                
                downloadProgress.code = "200";
                result = true;
                //File.Delete(localZipFile); //删除下载的压缩包
            }
            catch (Exception ex)
            {
                downloadProgress.code = "400";
                Logger.CreateLog(ex, "文件下载");
                result = false;
            }
            return result;
        }
    }
}
