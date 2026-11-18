using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Zoncare.Common
{
    public static class ClientKey
    {
        /// <summary>
        /// 获取密钥（长度为8的倍数，最大32位）
        /// </summary>
        private static string Key
        {
            get { return @"!@#$%^{}[]&*whx9384%^hyt90oeifu@"; }
        }

        /// <summary>
        /// 获取向量（长度不能超过密钥）
        /// </summary>
        private static string IV
        {
            get { return @"*)(&%$&*)(ULGewf"; }
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainStr">源字符串</param>
        /// <returns>加密字符串</returns>
        public static string AESEncrypt(string plainStr)
        {
            try
            {
                byte[] bKey = Encoding.UTF8.GetBytes(Key);
                byte[] bIV = Encoding.UTF8.GetBytes(IV);
                byte[] byteArray = Encoding.UTF8.GetBytes(plainStr);

                string encrypt = null;
                Rijndael aes = Rijndael.Create();

                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        encrypt = Convert.ToBase64String(mStream.ToArray());
                    }
                }

                return encrypt;
            }
            catch { return ""; }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="encryptStr">源字符串</param>
        /// <returns>解密字符串</returns>
        public static string AESDecrypt(string encryptStr)
        {
            try
            {
                byte[] bKey = Encoding.UTF8.GetBytes(Key);
                byte[] bIV = Encoding.UTF8.GetBytes(IV);
                byte[] byteArray = Convert.FromBase64String(encryptStr);

                string decrypt = null;
                Rijndael aes = Rijndael.Create();

                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
                aes.Clear();
                return decrypt;
            }
            catch { return ""; }
        }

        public static string GetMD5(string myString)
        {
            string byte2String = string.Empty;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(myString);
            byte[] targetData = md5.ComputeHash(fromData);

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String = byte2String + targetData[i].ToString("x2");
            }

            return byte2String;
        }

        public static string SHA256EncryptString(string data)
        {
            string result = "";
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                byte[] hash = SHA256Managed.Create().ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }
                result = builder.ToString();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }

            return result;
        }

        /// <summary>
        /// 生成Key文件
        /// </summary>
        /// <param name="sPath">生成路径（带文件名）</param>
        /// <param name="sDate">Key信息-生成日期</param>
        /// <param name="sKeyCode">Key信息-加密KeyCode</param>
        /// <returns>操作结果</returns>
        public static bool CreateFile(string sPath, string sDate, string sKeyCode)
        {
            try
            {
                FileStream fileStream = new FileStream(sPath, FileMode.Create);
                StreamWriter streamWrite = new StreamWriter(fileStream);
                streamWrite.WriteLine("[KeyInfo]");
                streamWrite.WriteLine("Date=" + sDate);
                streamWrite.WriteLine("Code=" + sKeyCode);
                streamWrite.Close();
                fileStream.Close();
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 验证15位身份证号是否有效
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool CheckIDCard15(string Id)
        {
            long n = 0;
            if (long.TryParse(Id, out n) == false || n < Math.Pow(10, 14))

            {
                return false;//数字验证

            }

            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";

            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }

            string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();

            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }

            return true;//符合15位身份证标准
        }

        /// <summary>
        /// 验证18位身份证号是否有效
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool CheckIDCard18(string Id)
        {
            long n = 0;
            if (long.TryParse(Id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }

            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";

            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = Id.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }
            return true;//符合GB11643-1999标准
        }

        /// <summary>
        /// 复制文件夹中的所有内容
        /// </summary>
        /// <param name="sourcePath">源文件夹目录</param>
        /// <param name="savePath">指定文件夹目录</param>
        public static bool CopyDirectory(string sourcePath, string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            #region //拷贝labs文件夹到savePath下
            try
            {
                string[] labDirs = Directory.GetDirectories(sourcePath);//目录
                string[] labFiles = Directory.GetFiles(sourcePath);//文件
                if (labFiles.Length > 0)
                {
                    for (int i = 0; i < labFiles.Length; i++)
                    {
                        if (Path.GetExtension(labFiles[i]) != ".lab")//排除.lab文件
                        {
                            File.Copy(sourcePath + "\\" + Path.GetFileName(labFiles[i]), savePath + "\\" + Path.GetFileName(labFiles[i]), true);
                        }
                    }
                }
                if (labDirs.Length > 0)
                {
                    for (int j = 0; j < labDirs.Length; j++)
                    {
                        Directory.GetDirectories(sourcePath + "\\" + Path.GetFileName(labDirs[j]));

                        //递归调用
                        CopyDirectory(sourcePath + "\\" + Path.GetFileName(labDirs[j]), savePath + "\\" + Path.GetFileName(labDirs[j]));
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            #endregion
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcdir">源文件夹目录</param>
        /// <param name="dstdir">指定文件夹目录</param>
        /// <param name="overwrite">是否覆盖</param>
        /// <returns></returns>
        public static bool CopyDirIntoDestDirectory(string srcdir, string dstdir, bool overwrite)
        {
            bool result = false;
            try
            {
                if (!Directory.Exists(srcdir))
                {
                    return true;
                }

                string todir = Path.Combine(dstdir, Path.GetFileName(srcdir));

                if (!Directory.Exists(todir))
                    Directory.CreateDirectory(todir);

                foreach (var s in Directory.GetFiles(srcdir))
                    File.Copy(s, Path.Combine(todir, Path.GetFileName(s)), overwrite);

                foreach (var s in Directory.GetDirectories(srcdir))
                    CopyDirIntoDestDirectory(s, todir, overwrite);

                result = true;
            }
            catch (Exception ex)
            {
            }
            return result;
        }
    }

    /// <summary>
    /// Generates a 16 byte Unique Identification code of a computer
    /// Example: 4876-8DB5-EE85-69D3-FE52-8CF7-395D-2EA9
    /// </summary>
    public class FingerPrint
    {
        private static string fingerPrint = string.Empty;
        public static string Value()
        {
            try
            {
                if (string.IsNullOrEmpty(fingerPrint))
                {
                    //fingerPrint = GetHash("CPU >> " + cpuId() + "\nBIOS >> " + biosId() + "\nBASE >> " + baseId()
                    //    + "\nDISK >> " + diskId() + "\nVIDEO >> " + videoId() + "\nMAC >> " + macId());
                    string strUnique = "CPU >> " + cpuId() + "\nBIOS >> " + biosId() + "\nBASE >> " + baseId();
                    fingerPrint = GetHash(strUnique);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return fingerPrint;
        }
        private static string GetHash(string s)
        {
            string result = "";
            try
            {
                MD5 sec = new MD5CryptoServiceProvider();
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] bt = enc.GetBytes(s);
                result = GetHexString(sec.ComputeHash(bt));
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }
        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            try
            {
                for (int i = 0; i < bt.Length; i++)
                {
                    byte b = bt[i];
                    int n, n1, n2;
                    n = (int)b;
                    n1 = n & 15;
                    n2 = (n >> 4) & 15;
                    if (n2 > 9)
                        s += ((char)(n2 - 10 + (int)'A')).ToString();
                    else
                        s += n2.ToString();
                    if (n1 > 9)
                        s += ((char)(n1 - 10 + (int)'A')).ToString();
                    else
                        s += n1.ToString();
                    if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return s;
        }

        #region Original Device ID Getting Code
        //Return a hardware identifier
        private static string identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            try
            {
                System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
                System.Management.ManagementObjectCollection moc = mc.GetInstances();
                foreach (System.Management.ManagementObject mo in moc)
                {
                    if (mo[wmiMustBeTrue].ToString() == "True")
                    {
                        //Only get the first one
                        if (result == "")
                        {
                            try
                            {
                                result = mo[wmiProperty].ToString();
                                break;
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }
        //Return a hardware identifier
        private static string identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            try
            {
                System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
                System.Management.ManagementObjectCollection moc = mc.GetInstances();
                foreach (System.Management.ManagementObject mo in moc)
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }
        private static string cpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            string retVal = identifier("Win32_Processor", "UniqueId");
            try
            {
                if (retVal == "") //If no UniqueID, use ProcessorID
                {
                    retVal = identifier("Win32_Processor", "ProcessorId");
                    if (retVal == "") //If no ProcessorId, use Name
                    {
                        retVal = identifier("Win32_Processor", "Name");
                        if (retVal == "") //If no Name, use Manufacturer
                        {
                            retVal = identifier("Win32_Processor", "Manufacturer");
                        }
                        //Add clock speed for extra security
                        retVal += identifier("Win32_Processor", "MaxClockSpeed");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }

            return retVal;
        }
        //BIOS Identifier
        private static string biosId()
        {
            return identifier("Win32_BIOS", "Manufacturer")
            + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
            + identifier("Win32_BIOS", "IdentificationCode")
            + identifier("Win32_BIOS", "SerialNumber")
            + identifier("Win32_BIOS", "ReleaseDate")
            + identifier("Win32_BIOS", "Version");
        }
        //Main physical hard drive ID
        private static string diskId()
        {
            return identifier("Win32_DiskDrive", "Model")
            + identifier("Win32_DiskDrive", "Manufacturer")
            + identifier("Win32_DiskDrive", "Signature")
            + identifier("Win32_DiskDrive", "TotalHeads");
        }
        //Motherboard ID
        private static string baseId()
        {
            return identifier("Win32_BaseBoard", "Model")
            + identifier("Win32_BaseBoard", "Manufacturer")
            + identifier("Win32_BaseBoard", "Name")
            + identifier("Win32_BaseBoard", "SerialNumber");
        }
        //Primary video controller ID
        private static string videoId()
        {
            return identifier("Win32_VideoController", "DriverVersion")
            + identifier("Win32_VideoController", "Name");
        }
        //First enabled network card ID
        private static string macId()
        {
            return identifier("Win32_NetworkAdapterConfiguration",
                "MACAddress", "IPEnabled");
        }
        #endregion
    }

}
