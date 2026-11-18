using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ABPMRepository
{
    public class SDUtils
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;

        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FSCTL_LOCK_VOLUME = 0x00090018;
        private const uint FSCTL_UNLOCK_VOLUME = 0x0009001c;

        private const uint OPEN_EXISTING = 3;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll")]
        private static extern int DeviceIoControl(IntPtr deviceHandle, uint ioControlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, ref int bytesReturned, IntPtr overlapped);

        private System.IO.FileStream _DriverStream;
        private long _SectorLength = 0;
        private SafeFileHandle _DriverHandle;

        /// <summary>
        /// 扇区数
        /// </summary>
        public long SectorLength { get { return _SectorLength; } }


        /// <summary>
        /// 初始化(获取磁盘扇区信息)
        /// </summary>
        /// <param name="DriverName">G:</param>
        public SDUtils(string DriverName)
        {
            try
            {
                if (DriverName == null && DriverName.Trim().Length == 0) return;
                _DriverHandle = CreateFileA("\\\\.\\" + DriverName.Trim(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                _DriverStream = new System.IO.FileStream(_DriverHandle, System.IO.FileAccess.ReadWrite);
                GetSectorCount();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        int number1;
        /// <summary>
        /// 扇区显示转换
        /// </summary>
        /// <param name="SectorBytes">扇区长度512</param>
        /// <returns>EB 52 90 ......55 AA</returns>
        public string Byte2String(byte[] SectorBytes)
        {
            if (SectorBytes == null || SectorBytes.Length != 512) return "Content is empty";
            StringBuilder returnText = new StringBuilder();

            for (number1 = 0; number1 < 16; number1++)
                returnText.Append(number1.ToString("X02") + " ");
            returnText.Append("\r\n");
            byte number = 0;
            int RowCount = 0;
            returnText.Append(number.ToString("X02") + ":  ");
            number++;
            for (int i = 0; i != 512; i++)
            {
                returnText.Append(SectorBytes[i].ToString("X02") + " ");
                if (RowCount == 15 && number <= 31)
                {
                    returnText.Append("\r\n");
                    returnText.Append(number.ToString("X02") + ":  ");
                    number++;
                    RowCount = -1;
                }
                RowCount++;
            }

            return returnText.ToString();
        }

        /// <summary>
        /// 获取扇区数
        /// </summary>
        public void GetSectorCount()
        {
            if (_DriverStream == null) return;
            _DriverStream.Position = 0;

            byte[] returnByte = new byte[512];
            _DriverStream.Read(returnByte, 0, 512); //获取第1扇区

            if (returnByte[0] == 0xEB && returnByte[1] == 0x58)          //DOS的好象都是32位
            {
                _SectorLength = (long)BitConverter.ToInt32(new byte[] { returnByte[32], returnByte[33], returnByte[34], returnByte[35] }, 0);
            }
            if (returnByte[0] == 0xEB && returnByte[1] == 0x52)          //NTFS好象是64位
            {
                _SectorLength = BitConverter.ToInt64(new byte[] { returnByte[40], returnByte[41], returnByte[42], returnByte[43], returnByte[44], returnByte[45], returnByte[46], returnByte[47] }, 0);
            }
            if (returnByte[0] == 0xEB && returnByte[1] == 0xFE)          //zoncare
            {
                _SectorLength = (long)BitConverter.ToInt32(new byte[] { returnByte[32], returnByte[33], returnByte[34], returnByte[35] }, 0);
            }
        }


        /// <summary>
        /// 读一个扇区
        /// </summary>
        /// <param name="SectorIndex">扇区号</param>
        /// <param name="readRaw">读取原始扇区数据(含空占位符)还是有效字节数据</param>
        /// <returns>如果扇区数字大于分区信息的扇区数返回NULL</returns>
        public byte[] ReadSector(long SectorIndex, bool readRaw = true)
        {
            //if (SectorIndex > _SectorLength) 
            //   return null;
            _DriverStream.Position = SectorIndex * 512;
            byte[] outputByte = new byte[512];
            _DriverStream.Read(outputByte, 0, 512); //获取扇区

            if (readRaw == false) //读取有效数据
            {
                int dataLen = 0;
                for (int i = 0; i < outputByte.Length; i++)
                {
                    if (outputByte[i] == 0) { break; }
                    dataLen++;
                }
                byte[] bytData = new byte[dataLen];
                Buffer.BlockCopy(outputByte, 0, bytData, 0, dataLen);
                return bytData;
            }
            else
            {
                return outputByte;
            }
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="SectorBytes">扇区长度512</param>
        /// <param name="SectorIndex">扇区位置</param>
        public void WriteSector(byte[] SectorBytes, long SectorIndex)
        {
            if (SectorBytes.Length != 512) return;
            if (SectorIndex > _SectorLength) return;

            IntPtr driverHandle = _DriverHandle.DangerousGetHandle(); //通过FSCTL_LOCK_VOLUME 来设置独占访问(写保护)
            int byteReturned = 0;
            int controlN = DeviceIoControl(driverHandle, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, ref byteReturned, IntPtr.Zero);

            _DriverStream.Position = SectorIndex * 512;
            _DriverStream.Write(SectorBytes, 0, 512); //写入扇区 
            _DriverStream.Flush();

            controlN = DeviceIoControl(driverHandle, FSCTL_UNLOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, ref byteReturned, IntPtr.Zero); //释放写保护
        }

        /// <summary>
        /// 关闭扇区读写
        /// </summary>
        public void Close()
        {
            _DriverStream?.Close();
        }
    }
}
