using ABPMRepository;
using AnalysisSystem.Base;
using Force.Crc32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Zoncare.Common;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    static class FileOperator
    {
        [DllImport("TransformZQ-x32.dll", EntryPoint = "TransZQHolterToRencare", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TransZQHolterToRencareX86(string srcDatPath, string patXmlPath, string datFilePath, string atrFilePath);

        [DllImport("TransformZQ-x64.dll", EntryPoint = "TransZQHolterToRencare", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TransZQHolterToRencareX64(string srcDatPath, string patXmlPath, string datFilePath, string atrFilePath);

        /// <summary>
        /// 从SD卡读取心电数据
        /// </summary>
        /// <returns></returns>
        public static string GetFileFromSD(out string sn)
        {
            sn = string.Empty;
            string fileName = string.Empty; //将要上传的文件(临时文件全路径)
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            DriveInfo drInfo = allDrives.FirstOrDefault(o => o.IsReady && o.VolumeLabel.ToUpper() == "ZQ_HOLTER");
            if (drInfo != null)
            {
                //sn = AppCode.GetDriveSN(drInfo);
                DirectoryInfo dir = drInfo.RootDirectory;
                foreach (FileInfo fi in dir.GetFiles("*.dat").OrderBy(f => f.Name))
                {
                    fileName = fi.FullName;
                    //fi.CopyTo(@"E:\test\data\Holter Data\dat\" + fi.Name, true);
                    break;
                }
            }
            else
            {
                throw new Exception("没有检测到动态心电数据设备");
            }

            return fileName;
        }

        /// <summary>
        /// 根据磁盘根目录名或卷标获取磁盘信息
        /// </summary>
        /// <param name="rootName">磁盘根目录名</param>
        /// <param name="volumnName">磁盘卷标</param>
        /// <returns></returns>
        public static object GetDriveInfo(string rootName, string volumnName)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            if (!string.IsNullOrEmpty(rootName))
            {
                DriveInfo drInfo = allDrives.FirstOrDefault(o => o.IsReady && o.Name.Contains(rootName.ToUpper()));
                dynamic driveInfo = new
                {
                    mounted = drInfo.RootDirectory.FullName,
                    total = (drInfo.TotalSize / 1024d / 1024 / 1024).ToString("F1") + "gb",
                    used = ((drInfo.TotalSize - drInfo.AvailableFreeSpace) / 1024d / 1024 / 1024).ToString("F1") + "gb",
                    available = (drInfo.AvailableFreeSpace / 1024d / 1024 / 1024).ToString("F1") + "gb",
                    capacity = ((double)(drInfo.TotalSize - drInfo.AvailableFreeSpace) / drInfo.TotalSize * 100).ToString("F1") + "%"
                };
                return driveInfo;
            }
            else if (!string.IsNullOrEmpty(volumnName))
            {
                dynamic driveInfo = null;
                var findResult = allDrives.Where(o => o.IsReady && o.VolumeLabel.ToUpper() == volumnName.ToUpper());
                int cnt = findResult.Count();
                if (cnt > 1)
                {
                    driveInfo = new { drive = "", driveErr = cnt };
                }
                else
                {
                    DriveInfo ret = findResult.FirstOrDefault();
                    if (ret != null)
                    {
                        string driveRootName = ret?.RootDirectory.FullName;

                        //查询磁盘或SD卡序列号
                        string diskSN = string.Empty;
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                            $"SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '{driveRootName.TrimEnd('\\')}'");
                        foreach (ManagementObject disk in searcher.Get())
                        {
                            diskSN = disk["volumeserialnumber"]?.ToString().Trim();
                            break;
                        }

                        driveInfo = new
                        {
                            drive = driveRootName ?? "",
                            driveErr = 0,
                            sn = diskSN
                        };
                    }
                }
                return driveInfo;
            }
            return null;
        }


        /// <summary>
        /// 查找直连血压设备(8337)
        /// </summary>
        /// <returns></returns>
        public static DeviceInfo GetBpDevice()
        {
            DeviceInfo device = null;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            DriveInfo drInfo = allDrives.FirstOrDefault(o => o.IsReady && o.VolumeLabel.ToUpper() == "ZQ_HOLTER");
            if (drInfo != null)
            {
                DirectoryInfo dir = drInfo.RootDirectory;
                foreach (FileInfo fi in dir.GetFiles("device.ini"))
                {
                    Dictionary<string, string> iniContent = INIFile.ReadIni(fi.FullName);
                    device = new DeviceInfo(3, iniContent["deviceModel"]);
                    device.ConnectionMode = 1;
                    device.SN = iniContent["SN"];
                    device.MAC = iniContent["MAC"];
                    device.DrivePath = dir.FullName;
                    break;
                }
            }
            else
            {
                throw new Exception("没有找到动态血压监测设备");
            }

            return device;
        }

        /// <summary>
        /// 根据crc32.txt文件内容获取文件信息列表
        /// </summary>
        /// <param name="fileBytes"></param>
        public static List<ZqFileInfo> GetDataFiles(byte[] fileBytes)
        {
            List<ZqFileInfo> dataFiles = new List<ZqFileInfo>();
            string fileContent = Encoding.UTF8.GetString(fileBytes);
            using (StringReader reader = new StringReader(fileContent))
            {
                string line;
                while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
                {
                    // 将每行内容添加到 List 中
                    string[] arr = line.Split('=');
                    string dataFileName = arr[0];
                    if (Regex.IsMatch(dataFileName, @"^nibp-\d+\.dat$"))
                    {
                        dataFiles.Add(new ZqFileInfo()
                        {
                            FileName = dataFileName,
                            CRC = arr[1]
                        });
                    }
                }
            }

            return dataFiles;
        }


        /// <summary>
        /// 将一个大文件在指定字节偏移位置分割成两个文件
        /// </summary>
        /// <param name="sourceFilePath">源文件</param>
        /// <param name="firstPartPath"></param>
        /// <param name="secondPartPath"></param>
        /// <param name="splitPosition">分割位置</param>
        /// <param name="headerLength">文件头内容长度</param>
        public static async Task<int> SplitFile(string sourceFilePath, string firstPartPath, string secondPartPath, long splitPosition, int headerLength = 0)
        {
            // 验证输入参数
            if (!File.Exists(sourceFilePath))
            {
                return -1; //源文件不存在
            }

            if (splitPosition <= 0 || splitPosition >= new FileInfo(sourceFilePath).Length)
            {
                return 0; //拆分位置必须在文件范围内
            }

            // 读取源文件并写入第一部分
            using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                byte[] headerBytes = new byte[headerLength];
                if (headerLength > 0) { sourceStream.Read(headerBytes, 0, headerBytes.Length); }

                long len1 = splitPosition - headerBytes.Length; //第1部分文件内容长度(不含文件头)

                // 写入第一部分文件
                if (File.Exists(firstPartPath)) File.Delete(firstPartPath);
                using (var firstPartStream = new FileStream(firstPartPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[1048576]; // 1M缓冲区
                    int bytesRead;
                    long totalBytesRead = 0;

                    if (headerLength > 0) { await firstPartStream.WriteAsync(headerBytes, 0, headerBytes.Length); }

                    while (totalBytesRead < len1 &&
                          (bytesRead = await sourceStream.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, len1 - totalBytesRead))) > 0)
                    {
                        await firstPartStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                    }
                }

                // 写入第二部分文件
                if (File.Exists(secondPartPath)) File.Delete(secondPartPath);
                using (var secondPartStream = new FileStream(secondPartPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[1048576]; // 1M缓冲区
                    int bytesRead;

                    if (headerLength > 0) { await secondPartStream.WriteAsync(headerBytes, 0, headerBytes.Length); }

                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await secondPartStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }
            }
            return 1;
        }

        /// <summary>
        /// 生成病人数据文件(病人信息+心电检查信息)
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="patientInfo"></param>
        /// <param name="examInfo"></param>
        /// <returns></returns>
        public static string GeneratePatientXmlFile(string directoryPath, PatientInfo patientInfo, ExamInfo examInfo)
        {
            string result = string.Empty;
            string patientId = examInfo.ExamID;
            string patientFilePath = directoryPath + "\\Patient";
            if (!Directory.Exists(patientFilePath)) Directory.CreateDirectory(patientFilePath);

            root _root = new root()
            {
                _DocInfo = new DocInfo() { Version = "1.2" },
                _PatientInfo = patientInfo,
                _ExamInfo = examInfo
            };

            string fullFileName = patientFilePath + $"\\{patientId}.xml";
            if (true == creatPatientXML(_root, fullFileName))
            {
                result = fullFileName;
            }

            return result;
        }

        /// <summary>
        /// 转换文件内的数据格式(ZQ->Recare)
        /// </summary>
        /// <returns></returns>
        public static int TransferHolterToRencare(string srcDatPath, string patXmlPath, string datFilePath, string atrFilePath)
        {
            //if (Environment.Is64BitProcess) //Is64BitOperatingSystem
            if (IntPtr.Size == 8) //64位
                return TransZQHolterToRencareX64(srcDatPath, patXmlPath, datFilePath, atrFilePath);
            else
                return TransZQHolterToRencareX86(srcDatPath, patXmlPath, datFilePath, atrFilePath);
        }

        /// <summary>
        /// 获取文件的MD5码
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileMD5(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(fs);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 获取文件分块的MD5码
        /// </summary>
        /// <returns></returns>
        public static string GetChunkMD5(string filePath, int chunkSize, int chunkIndex)
        {
            byte[] chunkBuffer = new byte[chunkSize];
            byte[] chunkContent = new byte[chunkSize];
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                int readCount = fs.Read(chunkBuffer, chunkSize * chunkIndex, chunkSize);
                if (readCount < chunkBuffer.Length)
                {
                    chunkContent = new byte[readCount];
                    Array.Copy(chunkBuffer, 0, chunkContent, 0, readCount);
                }
                else
                {
                    chunkContent = chunkBuffer;
                }
            }

            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(chunkContent);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 计算文件的CRC码
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string CalculateFileCrc32(string filePath)
        {
            //uint result = 0xFFFFFFFF; // 初始值
            //uint[] table = GenerateCrc32Table(); // CRC32 表
            //byte[] bytes = File.ReadAllBytes(filePath);

            //// 计算 CRC32 值
            //for (int i = 0; i < bytes.Length; i++)
            //{
            //    result = (result >> 8) ^ table[(result & 0xFF) ^ bytes[i]];
            //}
            //return (~result).ToString("x"); // 取反

            using (FileStream stream = File.OpenRead(filePath))
            {
                Crc32Algorithm crc32 = new Crc32Algorithm();
                byte[] hash = crc32.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLower(); // 将字节数组转换为十六进制字符串
            }
        }

        private static bool creatPatientXML(root _root, string xmlFileName)
        {
            bool result = false;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));

                XmlElement root = xmlDoc.CreateElement("root");

                XmlElement DocInfo = xmlDoc.CreateElement("DocInfo");
                XmlElement Version = xmlDoc.CreateElement("Version");
                Version.InnerText = _root._DocInfo.Version;
                DocInfo.AppendChild(Version);

                XmlElement PatientInfo = xmlDoc.CreateElement("PatientInfo");
                XmlElement PatientID = xmlDoc.CreateElement("PatientID");
                PatientID.InnerText = _root._PatientInfo.PatientID.ToXmlString();
                XmlElement Name = xmlDoc.CreateElement("Name");
                Name.InnerText = _root._PatientInfo.Name.ToXmlString();
                XmlElement Sex = xmlDoc.CreateElement("Sex");
                Sex.InnerText = _root._PatientInfo.Sex.ToXmlString();
                XmlElement BirthDate = xmlDoc.CreateElement("BirthDate");
                BirthDate.InnerText = _root._PatientInfo.BirthDate.ToXmlString();
                XmlElement Race = xmlDoc.CreateElement("Race");
                Race.InnerText = _root._PatientInfo.Race.ToXmlString();
                XmlElement PhoneNumber = xmlDoc.CreateElement("PhoneNumber");
                PhoneNumber.InnerText = _root._PatientInfo.PhoneNumber.ToXmlString();
                XmlElement MailingAddress = xmlDoc.CreateElement("MailingAddress");
                MailingAddress.InnerText = _root._PatientInfo.MailingAddress.ToXmlString();
                XmlElement Unit = xmlDoc.CreateElement("Unit");
                Unit.InnerText = _root._PatientInfo.Unit.ToXmlString();
                PatientInfo.AppendChild(PatientID);
                PatientInfo.AppendChild(Name);
                PatientInfo.AppendChild(Sex);
                PatientInfo.AppendChild(BirthDate);
                PatientInfo.AppendChild(Race);
                PatientInfo.AppendChild(PhoneNumber);
                PatientInfo.AppendChild(MailingAddress);
                PatientInfo.AppendChild(Unit);

                XmlElement ExamInfo = xmlDoc.CreateElement("ExamInfo");
                XmlElement patient_source = xmlDoc.CreateElement("patient_source");
                patient_source.InnerText = _root._ExamInfo.patient_source.ToXmlString();
                XmlElement VisitID = xmlDoc.CreateElement("VisitID");
                VisitID.InnerText = _root._ExamInfo.VisitID.ToXmlString();
                XmlElement Age = xmlDoc.CreateElement("Age");
                Age.InnerText = _root._ExamInfo.Age.ToXmlString();
                XmlElement AgeUnit = xmlDoc.CreateElement("AgeUnit");
                AgeUnit.InnerText = _root._ExamInfo.AgeUnit.ToXmlString();
                XmlElement body_height = xmlDoc.CreateElement("body_height");
                body_height.InnerText = _root._ExamInfo.body_height.ToXmlString();
                XmlElement bodyHeight_unit = xmlDoc.CreateElement("bodyHeight_unit");
                bodyHeight_unit.InnerText = _root._ExamInfo.bodyHeight_unit.ToXmlString();
                XmlElement body_weight = xmlDoc.CreateElement("body_weight");
                body_weight.InnerText = _root._ExamInfo.body_weight.ToXmlString();
                XmlElement body_weightUnit = xmlDoc.CreateElement("body_weightUnit");
                body_weightUnit.InnerText = _root._ExamInfo.body_weightUnit.ToXmlString();
                XmlElement SystolicPressure = xmlDoc.CreateElement("SystolicPressure");
                SystolicPressure.InnerText = _root._ExamInfo.SystolicPressure.ToXmlString();
                XmlElement DiastolicPressure = xmlDoc.CreateElement("DiastolicPressure");
                DiastolicPressure.InnerText = _root._ExamInfo.DiastolicPressure.ToXmlString();
                XmlElement PressureUnit = xmlDoc.CreateElement("PressureUnit");
                PressureUnit.InnerText = _root._ExamInfo.PressureUnit.ToXmlString();
                XmlElement Pacemaker = xmlDoc.CreateElement("Pacemaker");
                Pacemaker.InnerText = _root._ExamInfo.Pacemaker.ToXmlString();
                XmlElement Ward = xmlDoc.CreateElement("Ward");
                Ward.InnerText = _root._ExamInfo.Ward.ToXmlString();
                XmlElement SickbedNumber = xmlDoc.CreateElement("SickbedNumber");
                SickbedNumber.InnerText = _root._ExamInfo.SickbedNumber.ToXmlString();
                XmlElement RequestID = xmlDoc.CreateElement("RequestID");
                RequestID.InnerText = _root._ExamInfo.RequestID.ToXmlString();
                XmlElement RequestDepartment = xmlDoc.CreateElement("RequestDepartment");
                RequestDepartment.InnerText = _root._ExamInfo.RequestDepartment.ToXmlString();
                XmlElement RequestDepartmentName = xmlDoc.CreateElement("RequestDepartmentName");
                RequestDepartmentName.InnerText = _root._ExamInfo.RequestDepartmentName.ToXmlString();
                XmlElement RequestPhysician = xmlDoc.CreateElement("RequestPhysician");
                RequestPhysician.InnerText = _root._ExamInfo.RequestPhysician.ToXmlString();
                XmlElement medicine_history = xmlDoc.CreateElement("medicine_history");
                medicine_history.InnerText = _root._ExamInfo.medicine_history.ToXmlString();
                XmlElement ExamReason = xmlDoc.CreateElement("ExamReason");
                ExamReason.InnerText = _root._ExamInfo.ExamReason.ToXmlString();
                XmlElement ClinDiag = xmlDoc.CreateElement("ClinDiag");
                ClinDiag.InnerText = _root._ExamInfo.ClinDiag.ToXmlString();
                XmlElement disease_history = xmlDoc.CreateElement("disease_history");
                disease_history.InnerText = _root._ExamInfo.disease_history.ToXmlString();
                XmlElement ExamID = xmlDoc.CreateElement("ExamID");
                ExamID.InnerText = _root._ExamInfo.ExamID.ToXmlString();
                XmlElement examdeptCode = xmlDoc.CreateElement("examdeptCode");
                examdeptCode.InnerText = _root._ExamInfo.examdeptCode.ToXmlString();
                XmlElement ExamDepartmentName = xmlDoc.CreateElement("ExamDepartmentName");
                ExamDepartmentName.InnerText = _root._ExamInfo.ExamDepartmentName.ToXmlString();
                XmlElement ExamItem = xmlDoc.CreateElement("ExamItem");
                ExamItem.InnerText = _root._ExamInfo.ExamItem.ToXmlString();
                XmlElement ExamItemName = xmlDoc.CreateElement("ExamItemName");
                ExamItemName.InnerText = _root._ExamInfo.ExamItemName.ToXmlString();
                XmlElement technician = xmlDoc.CreateElement("technician");
                technician.InnerText = _root._ExamInfo.technician.ToXmlString();
                XmlElement technicianName = xmlDoc.CreateElement("technicianName");
                technicianName.InnerText = _root._ExamInfo.technicianName.ToXmlString();
                XmlElement ExamDateTime = xmlDoc.CreateElement("ExamDateTime");
                ExamDateTime.InnerText = _root._ExamInfo.ExamDateTime.ToXmlString();
                XmlElement Diagnostician = xmlDoc.CreateElement("Diagnostician");
                Diagnostician.InnerText = _root._ExamInfo.Diagnostician.ToXmlString();
                XmlElement DiagnosticianName = xmlDoc.CreateElement("DiagnosticianName");
                DiagnosticianName.InnerText = _root._ExamInfo.DiagnosticianName.ToXmlString();
                XmlElement DiagnosisDateTime = xmlDoc.CreateElement("DiagnosisDateTime");
                DiagnosisDateTime.InnerText = _root._ExamInfo.DiagnosisDateTime.ToXmlString();
                XmlElement DiagnosticMessage = xmlDoc.CreateElement("DiagnosticMessage");
                DiagnosticMessage.InnerText = _root._ExamInfo.DiagnosticMessage.ToXmlString();
                XmlElement SeverityCode = xmlDoc.CreateElement("SeverityCode");
                SeverityCode.InnerText = _root._ExamInfo.SeverityCode.ToXmlString();
                XmlElement PrintStatus = xmlDoc.CreateElement("PrintStatus");
                PrintStatus.InnerText = _root._ExamInfo.PrintStatus.ToXmlString();
                XmlElement ResultStatus = xmlDoc.CreateElement("ResultStatus");
                ResultStatus.InnerText = _root._ExamInfo.ResultStatus.ToXmlString();
                XmlElement file_guid = xmlDoc.CreateElement("file_guid");
                file_guid.InnerText = _root._ExamInfo.file_guid.ToXmlString();
                XmlElement Doctor_Advice = xmlDoc.CreateElement("Doctor_Advice");
                Doctor_Advice.InnerText = _root._ExamInfo.Doctor_Advice.ToXmlString();
                XmlElement Custom1Title = xmlDoc.CreateElement("Custom1Title");
                Custom1Title.InnerText = _root._ExamInfo.Custom1Title.ToXmlString();
                XmlElement Custom1Content = xmlDoc.CreateElement("Custom1Content");
                Custom1Content.InnerText = _root._ExamInfo.Custom1Content.ToXmlString();
                XmlElement Custom2Title = xmlDoc.CreateElement("Custom2Title");
                Custom2Title.InnerText = _root._ExamInfo.Custom2Title.ToXmlString();
                XmlElement Custom2Content = xmlDoc.CreateElement("Custom2Content");
                Custom2Content.InnerText = _root._ExamInfo.Custom2Content.ToXmlString();
                XmlElement Priority = xmlDoc.CreateElement("Priority");
                Priority.InnerText = _root._ExamInfo.Priority.ToXmlString();
                XmlElement fee = xmlDoc.CreateElement("fee");
                fee.InnerText = _root._ExamInfo.fee.ToXmlString();
                XmlElement feeUnit = xmlDoc.CreateElement("feeUnit");
                feeUnit.InnerText = _root._ExamInfo.feeUnit.ToXmlString();
                XmlElement DeviceID = xmlDoc.CreateElement("DeviceID");
                DeviceID.InnerText = _root._ExamInfo.DeviceID.ToXmlString();
                XmlElement ICCID = xmlDoc.CreateElement("ICCID");
                ICCID.InnerText = _root._ExamInfo.ICCID.ToXmlString();
                XmlElement ExamDocotorName = xmlDoc.CreateElement("ExamDocotorName");
                ExamDocotorName.InnerText = _root._ExamInfo.ExamDocotorName.ToXmlString();
                XmlElement ExamLead = xmlDoc.CreateElement("ExamLead");
                ExamLead.InnerText = _root._ExamInfo.ExamLead.ToXmlString();

                ExamInfo.AppendChild(patient_source);
                ExamInfo.AppendChild(VisitID);
                ExamInfo.AppendChild(Age);
                ExamInfo.AppendChild(AgeUnit);
                ExamInfo.AppendChild(body_height);
                ExamInfo.AppendChild(bodyHeight_unit);
                ExamInfo.AppendChild(body_weight);
                ExamInfo.AppendChild(body_weightUnit);
                ExamInfo.AppendChild(SystolicPressure);
                ExamInfo.AppendChild(DiastolicPressure);
                ExamInfo.AppendChild(PressureUnit);
                ExamInfo.AppendChild(Pacemaker);
                ExamInfo.AppendChild(Ward);
                ExamInfo.AppendChild(SickbedNumber);
                ExamInfo.AppendChild(RequestID);
                ExamInfo.AppendChild(RequestDepartment);
                ExamInfo.AppendChild(RequestDepartmentName);
                ExamInfo.AppendChild(RequestPhysician);
                ExamInfo.AppendChild(medicine_history);
                ExamInfo.AppendChild(ExamReason);
                ExamInfo.AppendChild(ClinDiag);
                ExamInfo.AppendChild(disease_history);
                ExamInfo.AppendChild(ExamID);
                ExamInfo.AppendChild(examdeptCode);
                ExamInfo.AppendChild(ExamDepartmentName);
                ExamInfo.AppendChild(ExamItem);
                ExamInfo.AppendChild(ExamItemName);
                ExamInfo.AppendChild(technician);
                ExamInfo.AppendChild(technicianName);
                ExamInfo.AppendChild(ExamDateTime);
                ExamInfo.AppendChild(Diagnostician);
                ExamInfo.AppendChild(DiagnosticianName);
                ExamInfo.AppendChild(DiagnosisDateTime);
                ExamInfo.AppendChild(DiagnosticMessage);
                ExamInfo.AppendChild(SeverityCode);
                ExamInfo.AppendChild(PrintStatus);
                ExamInfo.AppendChild(ResultStatus);
                ExamInfo.AppendChild(file_guid);
                ExamInfo.AppendChild(Doctor_Advice);
                ExamInfo.AppendChild(Custom1Title);
                ExamInfo.AppendChild(Custom1Content);
                ExamInfo.AppendChild(Custom2Title);
                ExamInfo.AppendChild(Custom2Content);
                ExamInfo.AppendChild(Priority);
                ExamInfo.AppendChild(fee);
                ExamInfo.AppendChild(feeUnit);
                ExamInfo.AppendChild(DeviceID);
                ExamInfo.AppendChild(ICCID);
                ExamInfo.AppendChild(ExamDocotorName);
                ExamInfo.AppendChild(ExamLead);

                root.AppendChild(DocInfo);
                root.AppendChild(PatientInfo);
                root.AppendChild(ExamInfo);

                xmlDoc.AppendChild(root);
                //xmlDoc.Save(xmlName);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "   ";
                settings.Encoding = Encoding.UTF8;
                using (XmlWriter xmlWriter = XmlWriter.Create(xmlFileName, settings))
                {
                    xmlDoc.Save(xmlWriter);

                    //System.Xml.Serialization.XmlSerializerNamespaces namespaces =
                    //    new System.Xml.Serialization.XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName(string.Empty, "aa") });
                    //System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(_root.GetType());
                    //serializer.Serialize(xmlWriter, _root, namespaces);
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

    }

    class root
    {
        public DocInfo _DocInfo { get; set; }
        public PatientInfo _PatientInfo { get; set; }
        public ExamInfo _ExamInfo { get; set; }
    }

    class DocInfo
    {
        public string Version { get; set; }
    }

    class PatientInfo
    {
        public string PatientID { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public string BirthDate { get; set; }
        public string Race { get; set; }
        public string PhoneNumber { get; set; }
        public string MailingAddress { get; set; }
        public string Unit { get; set; }
    }

    class ExamInfo
    {
        public string patient_source { get; set; }
        public string VisitID { get; set; }
        public string Age { get; set; }
        public string AgeUnit { get; set; }
        public string body_height { get; set; }
        public string bodyHeight_unit { get; set; }
        public string body_weight { get; set; }
        public string body_weightUnit { get; set; }
        public string SystolicPressure { get; set; }
        public string DiastolicPressure { get; set; }
        public string PressureUnit { get; set; }
        public string Pacemaker { get; set; }
        public string Ward { get; set; }
        public string SickbedNumber { get; set; }
        public string RequestID { get; set; }
        public string RequestDepartment { get; set; }
        public string RequestDepartmentName { get; set; }
        public string RequestPhysician { get; set; }
        public string medicine_history { get; set; }
        public string ExamReason { get; set; }
        public string ClinDiag { get; set; }
        public string disease_history { get; set; }
        public string ExamID { get; set; }
        public string examdeptCode { get; set; }
        public string ExamDepartmentName { get; set; }
        public string ExamItem { get; set; }
        public string ExamItemName { get; set; }
        public string technician { get; set; }
        public string technicianName { get; set; }
        public string ExamDateTime { get; set; }
        public string Diagnostician { get; set; }
        public string DiagnosticianName { get; set; }
        public string DiagnosisDateTime { get; set; }
        public string DiagnosticMessage { get; set; }
        public string SeverityCode { get; set; }
        public string PrintStatus { get; set; }
        public string ResultStatus { get; set; }
        public string file_guid { get; set; }
        public string Doctor_Advice { get; set; }
        public string Custom1Title { get; set; }
        public string Custom1Content { get; set; }
        public string Custom2Title { get; set; }
        public string Custom2Content { get; set; }
        public string Priority { get; set; }
        public string fee { get; set; }
        public string feeUnit { get; set; }
        public string DeviceID { get; set; }
        public string ICCID { get; set; }
        public string ExamDocotorName { get; set; }
        public string ExamLead { get; set; }
    }

    static class XmlExtensions
    {
        public static string ToXmlString(this string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input == "null")
            {
                return string.Empty;
            }
            else
            {
                return input;
            }
        }

    }

}