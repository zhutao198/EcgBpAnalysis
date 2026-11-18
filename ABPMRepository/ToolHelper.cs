using Force.Crc32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ABPMRepository
{
    public class ZqBPFuns
    {
        internal static void ShowErrors(Exception ex)
        {
            StringBuilder sbErrMsg = new StringBuilder();
            sbErrMsg.AppendLine(ex.Message);
            Exception innerEx = ex.InnerException;
            while (true)
            {
                if (innerEx != null)
                {
                    sbErrMsg.AppendLine("---> " + innerEx.Message);
                    innerEx = innerEx.InnerException;
                }
                else
                    break;
            }
            //throw new Exception(sbErrMsg.ToString());
            MessageBox.Show(sbErrMsg.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        internal static Image ShowLoadingImage()
        {
            Bitmap imgResource = Properties.Resources.loading5;
            //FrameDimension fd = new FrameDimension(imgResource.FrameDimensionsList[0]);
            //int count = imgResource.GetFrameCount(fd);
            //using (Graphics graphics = picConn.CreateGraphics())
            //{
            //    while (true)
            //    {
            //        for (int i = 0; i < count; i++)
            //        {
            //            imgResource.SelectActiveFrame(fd, i);
            //            graphics.DrawImage(imgResource, (picConn.Width - imgResource.Width) / 2, (picConn.Height - imgResource.Height) / 2);
            //            Thread.Sleep(100);
            //            Application.DoEvents();
            //        }
            //    }
            //}

            //using (MemoryStream ms = new MemoryStream())
            //{
            MemoryStream ms = new MemoryStream();
            imgResource.Save(ms, ImageFormat.Gif);
            Image img = Image.FromStream(ms);

            return img;
            //}
        }

        internal static Nullable<decimal> ConvertToDecimal(object obj)
        {
            decimal? dec = null;
            if (obj != null && obj != DBNull.Value)
            {
                decimal d;
                bool blnParsed = decimal.TryParse(obj.ToString(), out d);
                if (blnParsed) dec = d;
            }
            return dec;
        }

        /// <summary>
        /// 根据当前运行环境的字节存储顺序获取系统可正确解析的字节数组
        /// </summary>
        /// <param name="arrByte"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] GetBySystemEndian(byte[] arrByte, bool isLittleEndian = false)
        {
            //x86平台中运行时, 一般为小端序存储方式(BitConverter.IsLittleEndian==true)
            if (isLittleEndian)
            {
                if (!BitConverter.IsLittleEndian) Array.Reverse(arrByte);
            }
            else
            {
                if (BitConverter.IsLittleEndian) Array.Reverse(arrByte);
            }

            return arrByte;
        }

        /// <summary>
        /// 计算CRC16值
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ushort ComputeCRC16_CCITT(byte[] bytes)
        {
            ushort polynomial = 0x1021; //多项式0x1021
            ushort initialValue = 0x0000; //初始值
            ushort crc = initialValue;

            for (int pos = 0; pos < bytes.Length; pos++)
            {
                crc ^= (ushort)(bytes[pos] << 8);

                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ polynomial);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            return crc;
        }

        /// <summary>
        /// 基于文件内容计算CRC
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string CalculateFileCrc32(byte[] content)
        {
            Crc32Algorithm crc32 = new Crc32Algorithm();
            byte[] hash = crc32.ComputeHash(content);
            return BitConverter.ToString(hash).Replace("-", "").ToLower(); // 将字节数组转换为十六进制字符串
        }

        /// <summary>
        /// 以多线程形式异步处理指定的任务
        /// </summary>
        /// <param name="action">将在新的线程中执行的方法</param>
        /// <param name="duration">任务执行的时长</param>
        public static async Task AsyncDoWork(Action<CancellationToken> action, int duration = -1)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task prepareTask = Task.Run(() =>
            {
                action.Invoke(cts.Token);
            }, cts.Token)
            .ContinueWith(t => { }, TaskContinuationOptions.OnlyOnFaulted);

            await Task.WhenAny(prepareTask, Task.Delay(duration));
            cts.Cancel();
        }

        /// <summary>
        /// 获取应用程序根目录路径
        /// </summary>
        /// <returns></returns>
        public static string GetBaseDirectory()
        {
            //string dirPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string dirPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            return dirPath;
        }

        /// <summary>
        /// 在一个字节数组或列表中查找另一个字节数组出现的位置
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="subArray"></param>
        /// <param name="offset">源数组或列表中的查找开始位置/偏移量</param>
        /// <returns></returns>
        public static int FindSubArray(IList<byte> sourceList, byte[] subArray, int offset = 0)
        {
            if (subArray.Length > sourceList.Count - offset)
            {
                return -1;
            }

            for (int i = offset; i <= sourceList.Count - subArray.Length; i++)
            {
                bool matched = true;
                for (int j = 0; j < subArray.Length; j++)
                {
                    if (sourceList[i + j] != subArray[j])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return i; // 找到匹配的子数组  
                }
            }

            return -1; // 没有找到匹配的子数组  
        }
    }

    public class ConnectionInformation
    {
        public string BluetoothPrefix { get; set; }
        public string UsedSerialPort { get; set; }
        public string UsedChannel { get; set; }
        public string UsedBluetoothMAC { get; set; }
    }

    public class BluetoothInformation
    {
        public string BluetoothName { get; set; }
        public string BluetoothMAC { get; set; }
        [JsonIgnore]
        public bool IsUsed { get; set; }
    }

    public class StringIgnoreCaseComparer : IEqualityComparer<string>
    {
        public bool Equals(string s1, string s2)
        {
            if (string.Compare(s1, s2, true) == 0)
                return true;
            else
                return false;
        }
        public int GetHashCode(string s)
        {
            return s.GetHashCode();
        }
    }

    static class ABMP_Extensions
    {
        internal static string TrimEndNewlines(this string _input)
        {
            return _input.TrimEnd("\r\n".ToCharArray()).Trim();
        }
        internal static string ToCmdString(this string _input, bool bluetoothConn)
        {
            if (bluetoothConn)
                return "BT+" + _input;
            else
                return "AT+" + _input;
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        internal static string ToHexStr(this byte[] bytes, string splitStr = "")
        {
            StringBuilder sb = new StringBuilder();
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("X2"));
                    sb.Append(splitStr);
                }
                sb.Remove(sb.Length - splitStr.Length, splitStr.Length);
            }
            return sb.ToString();
        }
    }

}
