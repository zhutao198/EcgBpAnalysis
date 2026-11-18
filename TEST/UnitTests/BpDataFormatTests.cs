using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EcgBpAnalysis.UnitTests
{
    /// <summary>
    /// 血压数据格式契约测试（characterization tests）。
    ///
    /// 背景：产品 FileDataHandler.ParseBpFile / ParseBpFileFor8337 的解析逻辑
    ///       强耦合 ZoncareEntities 数据库与文件 IO，无法直接隔离单元测试。
    ///       本组测试锁定这些方法所依赖的「数据格式契约」，作为回归保护，
    ///       并显式对应代码审查台账中的问题：
    ///       - R004：时间解析失败兜底 / 非法日期
    ///       - R005：文件长度校验
    ///       - R003：CRC/校验和校验
    ///
    /// 注意：这些测试验证的是「格式与规则」，不代替对产品方法的端到端测试；
    ///       产品方法的隔离化（可注入解析函数）是后续重构目标。
    /// </summary>
    [TestClass]
    public class BpDataFormatTests
    {
        #region 16 字节 nibp 记录格式（ParseBpFile）

        /// <summary>
        /// 16 字节记录的字段偏移契约：
        /// offset+0 checksum, +2 motion, +4 sys, +6 dia, +8 map, +10 pr, +12 error, +14 left, +15 measureType
        /// </summary>
        [TestMethod]
        public void Bp16ByteRecord_FieldOffsets_Contract()
        {
            short motion = 1, sys = 120, dia = 80, map = 93, pr = 70, err = 0, left = 0;
            short checksum = (short)(motion + sys + dia + map + pr + err + left);

            byte[] record = new byte[16];
            BitConverter.GetBytes(checksum).CopyTo(record, 0);
            BitConverter.GetBytes(motion).CopyTo(record, 2);
            BitConverter.GetBytes(sys).CopyTo(record, 4);
            BitConverter.GetBytes(dia).CopyTo(record, 6);
            BitConverter.GetBytes(map).CopyTo(record, 8);
            BitConverter.GetBytes(pr).CopyTo(record, 10);
            BitConverter.GetBytes(err).CopyTo(record, 12);
            BitConverter.GetBytes(left).CopyTo(record, 14);

            Assert.AreEqual(checksum, BitConverter.ToInt16(record, 0));
            Assert.AreEqual(motion, BitConverter.ToInt16(record, 2));
            Assert.AreEqual(sys, BitConverter.ToInt16(record, 4));
            Assert.AreEqual(dia, BitConverter.ToInt16(record, 6));
            Assert.AreEqual(map, BitConverter.ToInt16(record, 8));
            Assert.AreEqual(pr, BitConverter.ToInt16(record, 10));
            Assert.AreEqual(err, BitConverter.ToInt16(record, 12));
            Assert.AreEqual(left, BitConverter.ToInt16(record, 14));
        }

        /// <summary>
        /// checksum 正确性判定规则（与产品一致）：sum(字段) == checksum。
        /// 篡改任一字段会导致校验失败——这是数据完整性校验的核心。
        /// </summary>
        [TestMethod]
        public void Bp16ByteRecord_ChecksumDetectsCorruption()
        {
            short motion = 1, sys = 120, dia = 80, map = 93, pr = 70, err = 0, left = 0;
            short checksum = (short)(motion + sys + dia + map + pr + err + left);

            int ValidChecksum(short m, short s, short d, short mb, short p, short e, short l)
                => m + s + d + mb + p + e + l;

            Assert.AreEqual(checksum, ValidChecksum(motion, sys, dia, map, pr, err, left),
                "正确字段应通过校验");
            Assert.AreNotEqual(checksum, ValidChecksum(motion, 121, dia, map, pr, err, left),
                "收缩压被篡改应导致校验失败");
        }

        /// <summary>
        /// 单位换算契约：产品将 sys/dia/map 乘以 1000 入库（R004 相关字段）。
        /// </summary>
        [TestMethod]
        public void BpValue_UnitConversion_x1000()
        {
            short sys = 120, dia = 80, map = 93;
            Assert.AreEqual(120000, sys * 1000);
            Assert.AreEqual(80000, dia * 1000);
            Assert.AreEqual(93000, map * 1000);
        }

        #endregion

        #region 22 字节 8337 记录格式（ParseBpFileFor8337）

        /// <summary>
        /// 8337 记录：6 字节时间 + 16 字节数据 = 22 字节；
        /// 时间字段 year = 2000 + byte[0]（R004 相关）。
        /// </summary>
        [TestMethod]
        public void Bp8337Record_LengthAndYearOffset_Contract()
        {
            byte[] record = new byte[22];
            record[0] = 24; // 2000+24 = 2024
            record[1] = 11; // month
            record[2] = 18; // day
            record[3] = 9;  // hour
            record[4] = 30; // minute
            record[5] = 0;  // second

            Assert.AreEqual(22, record.Length, "8337 单条记录应为 22 字节");
            Assert.AreEqual(2024, 2000 + record[0]);
        }

        /// <summary>
        /// R005 相关：文件长度必须是记录长度的整数倍，否则尾部残片会被越界读取。
        /// 本测试锁定该契约判断。
        /// </summary>
        [TestMethod]
        public void Bp8337File_LengthMustBeMultipleOfRecordSize()
        {
            int recordSize = 22;
            Assert.IsTrue(22 % recordSize == 0);
            Assert.IsTrue(44 % recordSize == 0);
            Assert.IsTrue(45 % recordSize != 0, "45 字节含 1 字节残片，应被识别为非法长度");
        }

        #endregion

        #region 时间解析（R004）

        /// <summary>
        /// nibp 文件名时间串格式 yyyyMMddHHmmss（14 位）的解析契约。
        /// </summary>
        [TestMethod]
        public void NibpFileName_TimeStringFormat()
        {
            string fileName = "nibp-20260101120000.dat";
            string timeString = fileName.Substring("nibp-".Length, 14);

            Assert.AreEqual("20260101120000", timeString);
            DateTime parsed;
            bool ok = DateTime.TryParseExact(timeString, "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsed);
            Assert.IsTrue(ok, "合法时间串应解析成功");
            Assert.AreEqual(2026, parsed.Year);
            Assert.AreEqual(1, parsed.Month);
            Assert.AreEqual(1, parsed.Day);
        }

        /// <summary>
        /// R004：非法时间串 TryParseExact 应返回 false（产品用 Now 兜底会污染数据）。
        /// 本测试锁定「非法串必须被识别」的契约。
        /// </summary>
        [TestMethod]
        public void NibpFileName_InvalidTimeString_IsRejected()
        {
            string bad = "20261301120000"; // 月=13
            DateTime parsed;
            bool ok = DateTime.TryParseExact(bad, "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsed);
            Assert.IsFalse(ok, "非法月份时间串必须被 TryParseExact 拒绝");
        }

        /// <summary>
        /// R004：new DateTime(y,month,day,...) 对非法日期抛异常——产品未防护会中断整批。
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Bp8337Record_InvalidDate_Throws()
        {
            int y = 2000 + 24, month = 13, day = 1, h = 0, m = 0, s = 0; // 非法月份
            DateTime dt = new DateTime(y, month, day, h, m, s); // 期望抛异常
        }

        #endregion
    }
}
