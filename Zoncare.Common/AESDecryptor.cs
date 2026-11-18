using System;
using System.IO;
using System.Security.Cryptography;
using Zoncare.Utilities;

namespace Zoncare.Common
{
    public class AESDecryptor
    {
        private readonly byte[] _key;
        private readonly RijndaelManaged _aesAlg;
        private MemoryStream _msDecrypt;
        private CryptoStream _csDecrypt;
        private ICryptoTransform _decryptor; //解密器对象
        private byte[] _buffer = new byte[1024]; // 使用缓冲区提高性能

        public AESDecryptor(byte[] key)
        {
            _key = key;
            _aesAlg = new RijndaelManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.Zeros,
                Key = _key
            };
            _msDecrypt = new MemoryStream();

            //创建一个解密器来执行流转换
            _decryptor = _aesAlg.CreateDecryptor(_aesAlg.Key, null);
        }

        public byte[] Encrypt(byte[] plainData)
        {
            byte[] result = null;

            try
            {
                // 创建加密器
                ICryptoTransform encryptor = _aesAlg.CreateEncryptor(_aesAlg.Key, null); // 注意：IV在ECB模式下不会被使用

                // 创建一个内存流来保存加密后的数据
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // 将加密后的数据写入内存流
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainData, 0, plainData.Length);
                        csEncrypt.FlushFinalBlock(); // 完成加密操作
                    }
                    result = msEncrypt.ToArray();

                    // 将加密后的数据转换为Base64字符串以便存储或传输
                    //Convert.ToBase64String(msEncrypt.ToArray());
                    Console.WriteLine(result.ToHexStr(" "));
                }
            }
            catch (Exception ex) { }

            return result;
        }

        public byte[] Decrypt(byte[] cipherData)
        {
            byte[] result = null;
            try
            {
                //byte[] bIV = Encoding.UTF8.GetBytes(IV);

                _msDecrypt.SetLength(0);
                _msDecrypt.Position = 0;
                //if (_msDecrypt.CanRead) { _msDecrypt.Position = 0; }
                _msDecrypt.Write(cipherData, 0, cipherData.Length);
                _msDecrypt.Position = 0;  // 重置位置以便读取

                _csDecrypt = new CryptoStream(_msDecrypt, _decryptor, CryptoStreamMode.Read);

                using (MemoryStream msOutput = new MemoryStream())
                {
                    //_csDecrypt.CopyTo(msOutput);

                    int bytesRead;
                    while ((bytesRead = _csDecrypt.Read(_buffer, 0, _buffer.Length)) > 0)
                    {
                        msOutput.Write(_buffer, 0, bytesRead);
                    }
                    result = msOutput.ToArray();
                }
            }
            catch (Exception ex) { }

            return result;
        }

        public void Dispose()
        {
            _msDecrypt.Close();
            _msDecrypt.Dispose();

            _csDecrypt?.Clear();
            _csDecrypt?.Dispose();

            _aesAlg.Clear();
            _aesAlg.Dispose();
        }
    }
}
