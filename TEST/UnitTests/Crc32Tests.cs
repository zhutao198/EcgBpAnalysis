using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zoncare.Common;

namespace EcgBpAnalysis.UnitTests
{
    /// <summary>
    /// CRC32 校验单元测试。
    /// 对应产品代码 Zoncare.Common.CommonFuns.CalculateFileCrc32，
    /// 该方法用于 holter-*.dat / nibp-*.dat 数据文件完整性校验。
    /// </summary>
    [TestClass]
    public class Crc32Tests
    {
        /// <summary>
        /// 标准测试向量："123456789" 的 CRC32 为 0xCBF43926。
        /// 这是 CRC32 算法的业界标准校验值，验证算法实现正确性。
        /// </summary>
        [TestMethod]
        public void CalculateFileCrc32_StandardVector_123456789()
        {
            byte[] data = Encoding.ASCII.GetBytes("123456789");

            string crc = CommonFuns.CalculateFileCrc32(data);

            Assert.AreEqual("cbf43926", crc,
                "标准测试向量 '123456789' 的 CRC32 应为 cbf43926");
        }

        /// <summary>
        /// 空数组的 CRC32 应为 00000000。
        /// </summary>
        [TestMethod]
        public void CalculateFileCrc32_EmptyArray_ReturnsZero()
        {
            string crc = CommonFuns.CalculateFileCrc32(new byte[0]);

            Assert.IsTrue(crc == "00000000" || crc == string.Empty,
                $"空数组 CRC32 预期为 00000000 或空，实际为 {crc}");
        }

        /// <summary>
        /// 相同内容必须产生相同 CRC；不同内容必须产生不同 CRC（确定性 + 区分性）。
        /// </summary>
        [TestMethod]
        public void CalculateFileCrc32_DeterministicAndDistinct()
        {
            byte[] a = Encoding.ASCII.GetBytes("holter-data-sample");
            byte[] b = Encoding.ASCII.GetBytes("holter-data-sample");
            byte[] c = Encoding.ASCII.GetBytes("holter-data-sampleX");

            string crcA = CommonFuns.CalculateFileCrc32(a);
            string crcB = CommonFuns.CalculateFileCrc32(b);
            string crcC = CommonFuns.CalculateFileCrc32(c);

            Assert.AreEqual(crcA, crcB, "相同内容应产生相同 CRC");
            Assert.AreNotEqual(crcA, crcC, "不同内容应产生不同 CRC");
        }

        /// <summary>
        /// 返回值格式校验：8 位小写十六进制（对应 PadLeft(8,'0') 比较逻辑）。
        /// </summary>
        [TestMethod]
        public void CalculateFileCrc32_OutputFormat_LowercaseHex()
        {
            byte[] data = Encoding.ASCII.GetBytes("a");

            string crc = CommonFuns.CalculateFileCrc32(data);

            Assert.AreEqual(8, crc.Length, "CRC32 输出应为 8 位十六进制");
            StringAssert.DoesNotMatch(crc, new System.Text.RegularExpressions.Regex("[A-F]"),
                "CRC32 输出应为小写");
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(crc, "^[0-9a-f]{8}$"),
                $"CRC32 输出格式应为 8 位小写十六进制，实际为 {crc}");
        }

        /// <summary>
        /// 单字节差异敏感性：CRC32 应对 1 个 bit 的变化敏感（数据完整性校验核心要求）。
        /// </summary>
        [TestMethod]
        public void CalculateFileCrc32_SensitiveToOneByteChange()
        {
            byte[] original = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            byte[] corrupted = (byte[])original.Clone();
            corrupted[10] ^= 0x01; // 翻转 1 个 bit

            string crcOriginal = CommonFuns.CalculateFileCrc32(original);
            string crcCorrupted = CommonFuns.CalculateFileCrc32(corrupted);

            Assert.AreNotEqual(crcOriginal, crcCorrupted,
                "1 个 bit 的变化必须导致 CRC 变化，否则无法检出数据损坏");
        }
    }
}
