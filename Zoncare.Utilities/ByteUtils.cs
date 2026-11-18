using System;
using System.Text;

namespace Zoncare.Utilities
{
    public class ByteUtils
    {
        //四字节转int(小端序)
        public static int BytesToInt(byte[] src, int offset)
        {
            int value;
            if (src != null)
            {
                if (src.Length == 4)
                {
                    value = (int)((src[offset] & 0xFF) | ((src[offset + 1] & 0xFF) << 8) | ((src[offset + 2] & 0xFF) << 16) | ((src[offset + 3] & 0xFF) << 24));
                    return value;
                }
                else
                {
                    value = (int)((src[offset] & 0xFF) | ((src[offset + 1] & 0xFF) << 8));
                    return value;
                }

            }
            return 0;
        }
        //四字节转int(大端序)
        public static int BytesToInt2(byte[] src, int offset)
        {
            int value;
            if (src != null)
            {
                value = (int)(((src[offset] & 0xFF) << 24) | ((src[offset + 1] & 0xFF) << 16) | ((src[offset + 2] & 0xFF) << 8) | (src[offset + 3] & 0xFF));
                return value;
            }
            return 0;
        }
        public static int HexToInt(string strHex)
        {
            return Convert.ToInt32(strHex, 16);
        }
        public static short ByteToShort(byte[] b, int offset)
        {
            return (short)((0xff & b[0 + offset]) | (0xff00 & (b[1 + offset] << 8)));
        }
        public static float ByteToFloat(byte[] b, int offset)
        {
            return BitConverter.ToSingle(b, offset);
        }
        public static string ByteToString(byte[] b)
        {
            if (b != null)
                return Encoding.UTF8.GetString(b);
            else
                return "";
        }
        public static string StrCut(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                int end = str.IndexOf("\0");
                if (end >= 0)
                {
                    return str.Substring(0, end);
                }
                else
                {
                    return str;
                }

            }
            return string.Empty;
        }
        public static byte[] IntToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);//高8位
            src[0] = (byte)(value & 0xFF);//低位
            return src;
        }
    }
}
