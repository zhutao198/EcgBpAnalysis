using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Zoncare.Common
{
    public class AESEncrypt
    {
        //填充类型
        private readonly static string AES_TYPE = "AES/ECB/PKCS5Padding";

        private readonly static string AES_KEY = "2021zoneCare6201";   //AES固定格式为128/192/256 bits

        public static string EncryptToBase64(string input, string key)
        {
            string result = string.Empty;
            byte[] textBytes = Encoding.UTF8.GetBytes(input);

            byte[] key32 = Encoding.UTF8.GetBytes(AES_KEY);
            //byte[] key32 = new byte[32];
            //byte[] byteKey = Encoding.UTF8.GetBytes(AES_KEY.PadRight(key32.Length));
            //Array.Copy(byteKey, key32, key32.Length);

            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key32;
            aes.IV = new byte[16];
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(textBytes, 0, textBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        result = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch { }
            finally
            {
                aes.Clear();
            }

            return result;
        }

        public static byte[] Encrypt0(string input, string key)
        {
            byte[] result = new byte[0];
            byte[] textBytes = Encoding.UTF8.GetBytes(input);

            byte[] key32 = Encoding.UTF8.GetBytes(AES_KEY);
            //byte[] key32 = new byte[32];
            //byte[] byteKey = Encoding.UTF8.GetBytes(AES_KEY.PadRight(key32.Length));
            //Array.Copy(byteKey, key32, key32.Length);

            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key32;
            aes.IV = new byte[16];
            //aes.KeySize = 256;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(textBytes, 0, textBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        result = ms.ToArray();
                    }
                }
            }
            catch { }
            finally
            {
                aes.Clear();
            }

            return result;
        }

        public static byte[] Encrypt(string input, string key)
        {
            byte[] result = new byte[0];
            byte[] textBytes = Encoding.UTF8.GetBytes(input);

            byte[] key32 = Encoding.UTF8.GetBytes(AES_KEY);
            //byte[] key32 = new byte[32];
            //byte[] byteKey = Encoding.UTF8.GetBytes(AES_KEY.PadRight(key32.Length));
            //Array.Copy(byteKey, key32, key32.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key32;
                //aes.IV = new byte[16];
                try
                {
                    ICryptoTransform cryptoTransform = aes.CreateEncryptor();
                    result = cryptoTransform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                }
                catch { }
                finally
                {
                    aes.Clear();
                }
            }

            return result;
        }

        /// <summary>
        /// AES-128-ECB 加密
        /// </summary>
        public static string Encrypt(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            using (var aes = Aes.Create())
            {
                // ECB模式不需要IV
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7; // 对应Node.js的autoPadding

                // 使用密钥生成实际的Key（AES-128需要16字节）
                var keyBytes = Encoding.UTF8.GetBytes(AES_KEY);
                Array.Resize(ref keyBytes, 16); // 确保是16字节

                using (var encryptor = aes.CreateEncryptor(keyBytes, null))
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(text);
                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string DecryptFromBase64(string input, string key)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(input))
            {
                byte[] encryptedBytes = Convert.FromBase64String(input);

                byte[] key32 = Encoding.UTF8.GetBytes(AES_KEY);
                //byte[] key32 = new byte[32];
                //byte[] byteKey = Encoding.UTF8.GetBytes(AES_KEY.PadRight(key32.Length));
                //Array.Copy(byteKey, key32, key32.Length);

                RijndaelManaged aes = new RijndaelManaged();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key32;
                aes.IV = new byte[16];
                try
                {
                    using (MemoryStream ms = new MemoryStream(encryptedBytes))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            byte[] tmp = new byte[encryptedBytes.Length + 32];
                            int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
                            byte[] ret = new byte[len];
                            Array.Copy(tmp, 0, ret, 0, len);
                            result = Encoding.UTF8.GetString(ret);
                        }
                    }
                }
                finally
                {
                    aes.Clear();
                }
            }

            return result;
        }

        public static string Decrypt(byte[] encryptedBytes, string key)
        {
            string result = string.Empty;

            if (encryptedBytes != null)
            {
                byte[] key32 = Encoding.UTF8.GetBytes(AES_KEY);
                //byte[] key32 = new byte[32];
                //byte[] byteKey = Encoding.UTF8.GetBytes(AES_KEY.PadRight(key32.Length));
                //Array.Copy(byteKey, key32, key32.Length);

                RijndaelManaged aes = new RijndaelManaged();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key32;
                aes.IV = new byte[16];
                try
                {
                    using (MemoryStream ms = new MemoryStream(encryptedBytes))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            byte[] tmp = new byte[encryptedBytes.Length + 32];
                            int len = cryptoStream.Read(tmp, 0, tmp.Length);
                            byte[] ret = new byte[len];
                            Array.Copy(tmp, 0, ret, 0, len);
                            result = Encoding.UTF8.GetString(ret);
                        }
                    }
                }
                catch (Exception ex) { }
                finally
                {
                    aes.Clear();
                }
            }

            return result;
        }

        /// <summary>
        /// AES-128-ECB 解密
        /// </summary>
        public static string Decrypt(string text)
        {
            if (string.IsNullOrEmpty(text)) { return string.Empty; }

            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                var keyBytes = Encoding.UTF8.GetBytes(AES_KEY);
                Array.Resize(ref keyBytes, 16);

                using (var decryptor = aes.CreateDecryptor(keyBytes, null))
                using (var ms = new MemoryStream(Convert.FromBase64String(text)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
