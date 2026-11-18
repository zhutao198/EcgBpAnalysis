using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zoncare.Common;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisSystem.Base
{
    public class FileDataHandler
    {
        //IE95数据文件CRC校验
        public static async Task<bool> Crc32Verify_IE95(FileInfo[] sourceFiles, string crcFilePath)
        {
            if (!CommonValues.VerifyFileCRC) return true;
            if (File.Exists(crcFilePath))
            {
                Dictionary<string, string> crcs = INIFile.ReadIni(crcFilePath);
                List<FileInfo> files = sourceFiles.Where(fi => (fi.Name.StartsWith("holter-1") && fi.Name.EndsWith("dat"))).ToList();
                foreach (FileInfo fi in files)
                {
                    if (fi.Length < 524288000)
                    {
                        byte[] dataFileBytes = await PubFuns.ReadAllBytesAsync(fi.FullName);
                        string crc32 = CommonFuns.CalculateFileCrc32(dataFileBytes);
                        if (crc32.PadLeft(8, '0').ToLower() != crcs["crc"].Substring(2).PadLeft(8, '0').ToLower())
                        {
                            return false;
                        }
                    }
                }
                await Task.Delay(1000);
            }
            return true;
        }

        //8333、8337或8377数据文件CRC校验
        public static async Task<bool> Crc32Verify(IEnumerable<FileInfo> sourceFiles, string crcFilePath, int checkItem)
        {
            if (!CommonValues.VerifyFileCRC) return true;
            if (!File.Exists(crcFilePath)) return false;

            Dictionary<string, string> crcs = INIFile.ReadIni(crcFilePath);
            var fileFilter = sourceFiles.Where(fi => true);
            if (checkItem == 1)
                fileFilter = sourceFiles.Where(fi => fi.Name.StartsWith("holter-") && fi.Name.EndsWith("dat"));
            else if (checkItem == 2)
                fileFilter = sourceFiles.Where(fi => fi.Name.StartsWith("nibp-") && fi.Name.EndsWith("dat"));
            else if (checkItem == 3)
                fileFilter = sourceFiles.Where(fi => (fi.Name.StartsWith("holter-") && fi.Name.EndsWith("dat"))
                         || (fi.Name.StartsWith("nibp-") && fi.Name.EndsWith("dat"))).ToList();

            List<FileInfo> files = fileFilter.ToList();
            foreach (FileInfo fi in files)
            {
                if (!crcs.ContainsKey(fi.Name)) continue;
                if (fi.Length < 524288000)
                {
                    byte[] dataFileBytes = await PubFuns.ReadAllBytesAsync(fi.FullName);
                    string crc32 = CommonFuns.CalculateFileCrc32(dataFileBytes);
                    if (crc32.PadLeft(8, '0').ToLower() != crcs[fi.Name].PadLeft(8, '0').ToLower())
                    {
                        return false;
                    }
                }
            }
            await Task.Delay(1000);
            return true;
        }

        /// <summary>
        /// 解析血压文件数据并保存到数据库
        /// </summary>
        /// <param name="dbEntities"></param>
        /// <param name="fi"></param>
        /// <param name="caseId"></param>
        public static void ParseBpFile(ZoncareEntities dbEntities, FileInfo fi, int caseId)
        {
            //ecgSettingValues ecgSettings = CommonValues.SystemSettings.ecgSets;
            byte[] dataBytes = File.ReadAllBytes(fi.FullName);
            int offset = dataBytes.Length - 16;
            short checksum = BitConverter.ToInt16(dataBytes, offset);
            short motionState = BitConverter.ToInt16(dataBytes, offset + 2);
            short systolic = BitConverter.ToInt16(dataBytes, offset + 4);
            short diastolic = BitConverter.ToInt16(dataBytes, offset + 6);
            short mapValue = BitConverter.ToInt16(dataBytes, offset + 8);
            short pr = BitConverter.ToInt16(dataBytes, offset + 10);
            short errorcode = BitConverter.ToInt16(dataBytes, offset + 12);
            short left = BitConverter.ToInt16(dataBytes, offset + 14); //保留字段(第15个字节:袖带类型; 第16个字节:测量类型)
            byte measureType = dataBytes[offset + 15];

            int actualSum = motionState + systolic + diastolic + mapValue + pr + errorcode + left;
            if (actualSum == checksum)
            {
                DateTime fileTime = DateTime.Now;
                string timeString = fi.Name.Substring("nibp-".Length, 14);
                bool timeParsed = DateTime.TryParseExact(timeString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out fileTime);

                bpDetail abpData = new bpDetail();
                abpData.caseId = caseId;
                abpData.OriginalTime = fileTime;
                abpData.time = fileTime.ToString(SysConstants.DATETIME_FORMAT);
                abpData.sysPressure = systolic * 1000;
                abpData.diaPressure = diastolic * 1000;
                abpData.pulse = pr;
                abpData.mapPressure = mapValue * 1000;
                abpData.motionState = (motionState + 1).ToString();
                abpData.status = 0;
                abpData.dataStatus = (systolic == 0 || diastolic == 0) ? 0 : 1;
                abpData.errorCode = errorcode.ToString();
                abpData.measureType = measureType;
                abpData.createTime = DateTime.Now;

                dbEntities.BpDetails.Add(abpData);
                dbEntities.SaveChanges();
            }
        }

        /// <summary>
        /// 解析8337设备血压文件数据并保存到数据库
        /// </summary>
        public static List<bpDetail> ParseBpFileFor8337(ZoncareEntities dbEntities, FileInfo fi, int caseId)
        {
            byte[] dataBytes = File.ReadAllBytes(fi.FullName);
            if (dataBytes.Length < 22) return null;

            List<bpDetail> bpDetails = new List<bpDetail>();
            try
            {
                for (int i = 0; i < dataBytes.Length; i += 22)
                {
                    int y = 2000 + dataBytes[i];
                    byte mth = dataBytes[i + 1];
                    byte d = dataBytes[i + 2];
                    byte h = dataBytes[i + 3];
                    byte m = dataBytes[i + 4];
                    byte s = dataBytes[i + 5];

                    int offset = i + 6;
                    short checksum = BitConverter.ToInt16(dataBytes, offset);
                    short motionState = BitConverter.ToInt16(dataBytes, offset + 2);
                    short systolic = BitConverter.ToInt16(dataBytes, offset + 4);
                    short diastolic = BitConverter.ToInt16(dataBytes, offset + 6);
                    short mapValue = BitConverter.ToInt16(dataBytes, offset + 8);
                    short pr = BitConverter.ToInt16(dataBytes, offset + 10);
                    short errorcode = BitConverter.ToInt16(dataBytes, offset + 12);
                    short left = BitConverter.ToInt16(dataBytes, offset + 14); //保留字段(第15个字节:袖带类型; 第16个字节:测量类型)
                    byte measureType = dataBytes[offset + 15];

                    int actualSum = motionState + systolic + diastolic + mapValue + pr + errorcode + left;
                    if (actualSum == checksum)
                    {
                        DateTime dataTime = new DateTime(y, mth, d, h, m, s);

                        bpDetail abpData = new bpDetail();
                        abpData.caseId = caseId;
                        abpData.OriginalTime = dataTime;
                        abpData.time = dataTime.ToString(SysConstants.DATETIME_FORMAT);
                        abpData.sysPressure = systolic * 1000;
                        abpData.diaPressure = diastolic * 1000;
                        abpData.pulse = pr;
                        abpData.mapPressure = mapValue * 1000;
                        abpData.motionState = (motionState + 1).ToString();
                        abpData.status = 0;
                        abpData.dataStatus = (systolic == 0 || diastolic == 0) ? 0 : 1;
                        abpData.errorCode = errorcode.ToString();
                        abpData.measureType = measureType;
                        abpData.createTime = DateTime.Now;

                        bpDetails.Add(abpData);
                    }
                }

                dbEntities.ChangeTracker.AutoDetectChangesEnabled = false; //批量插入/删除之前, 关闭自动检测更改，提升大量插入时的性能
                dbEntities.BpDetails.AddRange(bpDetails);
                dbEntities.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "文件上传");
                bpDetails = null;
            }
            finally
            {
                dbEntities.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return bpDetails;
        }

        /// <summary>
        /// 获取8337设备待上传文件列表
        /// </summary>
        /// <param name="drivePath"></param>
        /// <param name="crcs"></param>
        /// <returns></returns>
        public static List<FileInfo> GetFilesToBeUploadFor8337(string drivePath, Dictionary<string, string> crcs)
        {
            List<FileInfo> files = new List<FileInfo>();

            string deviceInIFilePath = Path.Combine(drivePath, "device.ini");
            string nibpInIFilePath = Path.Combine(drivePath, "nibp.ini");
            string bpListFilePath = Path.Combine(drivePath, "nibp_test_result.dat");
            if (File.Exists(deviceInIFilePath) && File.Exists(nibpInIFilePath) && File.Exists(bpListFilePath))
            {
                files = new List<FileInfo>();
                FileInfo fiDeviceIni = new FileInfo(deviceInIFilePath);
                FileInfo fiNibpIni = new FileInfo(nibpInIFilePath);
                FileInfo fiFileListIni = new FileInfo(bpListFilePath);
                files.Add(fiDeviceIni);
                files.Add(fiNibpIni);
                files.Add(fiFileListIni);

                foreach (string key in crcs.Keys)
                {
                    string dataFileName = Path.Combine(drivePath, key);
                    if (!File.Exists(dataFileName)) { return null; }

                    files.Add(new FileInfo(dataFileName));
                }
            }

            return files;
        }

    }
}
