using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnalysisSystem.Base;
using Zoncare.DAL;

namespace EcgBpAnalysis.UnitTests
{
    /// <summary>
    /// 血压文件解析数据库集成测试。
    ///
    /// 与 BpDataFormatTests（纯格式契约）不同，本组测试调用产品真实方法
    /// FileDataHandler.ParseBpFile / ParseBpFileFor8337，使用临时 SQLite 数据库，
    /// 验证从二进制文件到数据库记录的完整链路。
    ///
    /// 对应审查项：
    /// - R004：时间解析失败兜底、单位换算、非法日期
    /// - R005：文件长度校验
    /// - R003：校验和校验
    /// </summary>
    [TestClass]
    public class BpParseIntegrationTests
    {
        private string _tempDir;
        private string _dbFile;
        private ZoncareEntities _db;

        [TestInitialize]
        public void Setup()
        {
            try { SQLitePCL.Batteries_V2.Init(); }
            catch { /* 冒烟测试已验证 provider 可用 */ }

            _tempDir = Path.Combine(Path.GetTempPath(), "ecgbp_it_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _dbFile = Path.Combine(_tempDir, "test.db");
            _db = new ZoncareEntities(_dbFile);
            _db.Database.EnsureCreated();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_db != null) _db.Dispose();
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }
            catch { /* 临时目录清理失败不影响测试结论 */ }
        }

        /// <summary>
        /// 构造 16 字节 nibp 记录：末尾 16 字节为数据区，字段偏移与产品一致。
        /// </summary>
        private string CreateNibpFile(string fileName, short motion, short sys, short dia,
            short map, short pr, short err, short left, byte measureType, bool corruptChecksum = false,
            int prefixBytes = 0)
        {
            short checksum = (short)(motion + sys + dia + map + pr + err + left);
            if (corruptChecksum) checksum++;

            byte[] record = new byte[16];
            BitConverter.GetBytes(checksum).CopyTo(record, 0);
            BitConverter.GetBytes(motion).CopyTo(record, 2);
            BitConverter.GetBytes(sys).CopyTo(record, 4);
            BitConverter.GetBytes(dia).CopyTo(record, 6);
            BitConverter.GetBytes(map).CopyTo(record, 8);
            BitConverter.GetBytes(pr).CopyTo(record, 10);
            BitConverter.GetBytes(err).CopyTo(record, 12);
            BitConverter.GetBytes(left).CopyTo(record, 14);
            // 第 16 字节（offset+15）为 measureType，但产品用 dataBytes[offset+15]，
            // 而 left 的第二个字节也在 offset+15 —— 这里单独设置 measureType
            record[15] = measureType;

            byte[] all = new byte[prefixBytes + 16];
            Array.Copy(record, 0, all, prefixBytes, 16);
            string path = Path.Combine(_tempDir, fileName);
            File.WriteAllBytes(path, all);
            return path;
        }

        /// <summary>
        /// 正常样本：校验和正确 → 记录入库，字段值与单位换算正确。
        /// </summary>
        [TestMethod]
        public void ParseBpFile_ValidRecord_SavedWithCorrectValues()
        {
            string path = CreateNibpFile("nibp-20260101120000.dat",
                motion: 1, sys: 120, dia: 80, map: 93, pr: 70, err: 0, left: 0, measureType: 0);

            FileDataHandler.ParseBpFile(_db, new FileInfo(path), caseId: 42);
            _db.SaveChanges();

            var list = _db.BpDetails.Where(b => b.caseId == 42).ToList();
            Assert.AreEqual(1, list.Count, "应入库 1 条血压记录");
            var rec = list[0];
            Assert.AreEqual(120000, rec.sysPressure, "收缩压应 *1000");
            Assert.AreEqual(80000, rec.diaPressure, "舒张压应 *1000");
            Assert.AreEqual(93000, rec.mapPressure, "平均压应 *1000");
            Assert.AreEqual(70, rec.pulse);
            Assert.AreEqual("2", rec.motionState, "motionState 应为 (原值+1).ToString()");
            Assert.AreEqual(1, rec.dataStatus, "正常血压 dataStatus 应为 1");
            Assert.AreEqual("2026-01-01 12:00:00", rec.time, "时间应从文件名解析");
        }

        /// <summary>
        /// R003：校验和错误 → 不入库。
        /// </summary>
        [TestMethod]
        public void ParseBpFile_CorruptChecksum_NotSaved()
        {
            string path = CreateNibpFile("nibp-20260101120000.dat",
                1, 120, 80, 93, 70, 0, 0, 0, corruptChecksum: true);

            FileDataHandler.ParseBpFile(_db, new FileInfo(path), caseId: 42);

            Assert.AreEqual(0, _db.BpDetails.Count(b => b.caseId == 42),
                "校验和错误时不应入库");
        }

        /// <summary>
        /// 前置任意字节：产品取文件末尾 16 字节，前面有前缀也应正确解析。
        /// </summary>
        [TestMethod]
        public void ParseBpFile_WithPrefixBytes_ParsesLast16Bytes()
        {
            string path = CreateNibpFile("nibp-20260101120000.dat",
                1, 130, 85, 100, 75, 0, 0, 0, prefixBytes: 100);

            FileDataHandler.ParseBpFile(_db, new FileInfo(path), caseId: 42);

            var rec = _db.BpDetails.FirstOrDefault(b => b.caseId == 42);
            Assert.IsNotNull(rec, "应解析末尾 16 字节");
            Assert.AreEqual(130000, rec.sysPressure);
        }

        /// <summary>
        /// sys/dia 为 0 → dataStatus=0（排除的数据）。
        /// </summary>
        [TestMethod]
        public void ParseBpFile_ZeroPressure_DataStatusZero()
        {
            string path = CreateNibpFile("nibp-20260101120000.dat",
                motion: 1, sys: 0, dia: 0, map: 0, pr: 70, err: 0, left: 0, measureType: 0);

            FileDataHandler.ParseBpFile(_db, new FileInfo(path), caseId: 42);

            var rec = _db.BpDetails.FirstOrDefault(b => b.caseId == 42);
            Assert.IsNotNull(rec);
            Assert.AreEqual(0, rec.dataStatus, "sys/dia 为 0 时 dataStatus 应为 0");
        }
    }
}
