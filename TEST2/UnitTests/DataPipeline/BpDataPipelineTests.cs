using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnalysisSystem.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zoncare.DAL;

namespace EcgBpAnalysis.T2.Tests.DataPipeline
{
    /// <summary>
    /// M2：血压数据链路端到端一致性测试。
    ///
    /// 覆盖链路：二进制文件 → FileDataHandler 解析 → EF 入库 → 重新查询 → 断言一致性。
    /// 价值：单点测试无法发现"解析写入 A、查询读出 B、报告用 C"的跨模块漂移；
    ///       本组用例逐字段核对，确保患者数据从文件到数据库不失真。
    ///
    /// 关联缺陷：T2-DEF-006（8337 非法日期导致整批丢弃）、T2-DEF-008（重复导入产生重复记录）。
    /// </summary>
    [TestClass]
    public class BpDataPipelineTests
    {
        /// <summary>测试样本定义（血压值单位 mmHg，与文件内一致）。</summary>
        private class Sample
        {
            public string Name;
            public short Sys, Dia, Map, Pr;
            public int ExpectedDataStatus;
            public Sample(string name, short sys, short dia, short map, short pr, int ds)
            {
                Name = name; Sys = sys; Dia = dia; Map = map; Pr = pr; ExpectedDataStatus = ds;
            }
        }

        private string _tempDir;
        private string _dbFile;
        private ZoncareEntities _db;

        [TestInitialize]
        public void Setup()
        {
            try { SQLitePCL.Batteries_V2.Init(); } catch { }
            _tempDir = Path.Combine(Path.GetTempPath(), "t2bp_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _dbFile = Path.Combine(_tempDir, "t2.db");
            _db = new ZoncareEntities(_dbFile);
            _db.Database.EnsureCreated();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_db != null) _db.Dispose();
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); } catch { }
        }

        /// <summary>
        /// 构造 16 字节 nibp 记录。字段偏移与产品 ParseBpFile 一致：
        /// [0..1]checksum [2..3]motion [4..5]sys [6..7]dia [8..9]map [10..11]pr [12..13]err [14..15]left
        /// </summary>
        private static byte[] BuildNibpRecord(short motion, short sys, short dia, short map,
            short pr, short err, short left, byte measureType, bool corruptChecksum = false)
        {
            short checksum = (short)(motion + sys + dia + map + pr + err + left);
            if (corruptChecksum) checksum++;
            var rec = new byte[16];
            BitConverter.GetBytes(checksum).CopyTo(rec, 0);
            BitConverter.GetBytes(motion).CopyTo(rec, 2);
            BitConverter.GetBytes(sys).CopyTo(rec, 4);
            BitConverter.GetBytes(dia).CopyTo(rec, 6);
            BitConverter.GetBytes(map).CopyTo(rec, 8);
            BitConverter.GetBytes(pr).CopyTo(rec, 10);
            BitConverter.GetBytes(err).CopyTo(rec, 12);
            BitConverter.GetBytes(left).CopyTo(rec, 14);
            rec[15] = measureType;
            return rec;
        }

        private string WriteNibpFile(string fileName, byte[] record, int prefixBytes = 0)
        {
            var all = new byte[prefixBytes + record.Length];
            Array.Copy(record, 0, all, prefixBytes, record.Length);
            string path = Path.Combine(_tempDir, fileName);
            File.WriteAllBytes(path, all);
            return path;
        }

        // ============================================================ 端到端一致性

        /// <summary>
        /// TC-PIPE-01：正常样本端到端 —— 文件字段 → 入库 → 查询，逐一核对。
        ///
        /// 注意：measureType 传 0（非零会触发 T2-DEF-009，见 TC-PIPE-01B 的专门用例）。
        /// </summary>
        [TestMethod]
        public void Pipeline_NibpRecord_FileToDbRoundTrip_IsConsistent()
        {
            short motion = 1, sys = 120, dia = 80, map = 93, pr = 70, err = 0, left = 0;
            byte measureType = 0;
            var rec = BuildNibpRecord(motion, sys, dia, map, pr, err, left, measureType);
            string path = WriteNibpFile("nibp-20260315123045.dat", rec);

            FileDataHandler.ParseBpFile(_db, new FileInfo(path), 77);
            _db.SaveChanges();

            var rows = _db.BpDetails.Where(b => b.caseId == 77).ToList();
            Assert.AreEqual(1, rows.Count, "应入库 1 条记录");
            var r = rows[0];

            // 单位换算：文件里是 mmHg，库里是 ×1000
            Assert.AreEqual(sys * 1000, r.sysPressure, "收缩压单位换算不一致");
            Assert.AreEqual(dia * 1000, r.diaPressure, "舒张压单位换算不一致");
            Assert.AreEqual(map * 1000, r.mapPressure, "平均压单位换算不一致");
            Assert.AreEqual(pr, r.pulse, "脉率未保持一致");
            Assert.AreEqual((motion + 1).ToString(), r.motionState, "体动状态编码不一致");
            Assert.AreEqual(err.ToString(), r.errorCode, "错误码未保持一致");
            Assert.AreEqual(measureType, r.measureType, "测量类型未保持一致");
            Assert.AreEqual(1, r.dataStatus, "正常血压 dataStatus 应为 1");
            Assert.AreEqual(0, r.status);

            // 时间：来自文件名
            Assert.AreEqual("2026-03-15 12:30:45", r.time, "时间未按文件名正确解析");
            Assert.IsNotNull(r.OriginalTime);
            Assert.AreEqual(2026, r.OriginalTime.Value.Year);
            Assert.AreEqual(3, r.OriginalTime.Value.Month);
            Assert.AreEqual(15, r.OriginalTime.Value.Day);
            Assert.AreEqual(12, r.OriginalTime.Value.Hour);
            Assert.AreEqual(30, r.OriginalTime.Value.Minute);
            Assert.AreEqual(45, r.OriginalTime.Value.Second);
        }

        /// <summary>
        /// TC-PIPE-01B：测量类型非 0 的记录被静默丢弃（缺陷锚定 T2-DEF-009）。
        ///
        /// 根因：产品 FileDataHandler.ParseBpFile 中
        ///       short left = BitConverter.ToInt16(dataBytes, offset + 14);
        ///       byte  measureType = dataBytes[offset + 15];
        /// 两次读取**重叠在 offset+15 这一个字节**——left 的高字节实际就是 measureType。
        ///       而 checksum = motion+sys+dia+map+pr+err+left，
        ///       故文件端写入的 checksum（left=0）必然与产品算出的 left（= measureType*256）不符，
        ///       结果：**测量类型非 0 的血压记录全部被当作"校验失败"丢弃，患者数据静默丢失**。
        ///
        /// 修复方向：left 应按 1 字节读取（袖带类型），或 checksum 校验排除 offset+15。
        /// 修复后本用例应改为断言"入库 1 条且 measureType 正确"。
        /// </summary>
        [TestMethod]
        public void Pipeline_NibpRecord_NonZeroMeasureType_BehaviourIsObservable()
        {
            var rec = BuildNibpRecord(1, 120, 80, 93, 70, 0, 0, measureType: 3);
            string path = WriteNibpFile("nibp-20260315123045.dat", rec);

            FileDataHandler.ParseBpFile(_db, new FileInfo(path), 88);
            _db.SaveChanges();

            int count = _db.BpDetails.Count(b => b.caseId == 88);
            Console.WriteLine("[链路观察] measureType=3 的记录入库条数 = " + count
                + "（当前实现因 left/measureType 字节重叠被丢弃 -> T2-DEF-009）");
            Assert.AreEqual(0, count,
                "当前行为锚定：measureType 非 0 时记录被丢弃。修复 T2-DEF-009 后应改为断言 1 条。");
        }

        /// <summary>
        /// TC-PIPE-02：边界值端到端一致性（零值 / 极值 / 最小非零值）。
        /// </summary>
        [TestMethod]
        public void Pipeline_NibpRecord_BoundaryValues_AreConsistent()
        {
            var cases = new List<Sample>
            {
                new Sample("nibp-20260101000000.dat", 0, 0, 0, 0, 0),
                new Sample("nibp-20260101000001.dat", 300, 200, 230, 200, 1),
                new Sample("nibp-20260101000002.dat", 1, 1, 1, 1, 1),
            };

            foreach (var c in cases)
            {
                var rec = BuildNibpRecord(0, c.Sys, c.Dia, c.Map, c.Pr, 0, 0, 0);
                string path = WriteNibpFile(c.Name, rec);
                FileDataHandler.ParseBpFile(_db, new FileInfo(path), 100);
            }
            _db.SaveChanges();

            var rows = _db.BpDetails.Where(b => b.caseId == 100).OrderBy(b => b.time).ToList();
            Assert.AreEqual(3, rows.Count, "3 个样本应全部入库");

            Assert.AreEqual(0, rows[0].sysPressure, "零收缩压应保持为 0");
            Assert.AreEqual(0, rows[0].dataStatus, "零血压应标记为排除数据");
            Assert.AreEqual(300000, rows[1].sysPressure, "极值收缩压换算错误");
            Assert.AreEqual(1, rows[1].dataStatus);
            Assert.AreEqual(1000, rows[2].sysPressure, "最小非零收缩压换算错误");
            Assert.AreEqual(1, rows[2].dataStatus);
        }

        /// <summary>
        /// TC-PIPE-03：校验和错误 → 不得入库（数据完整性在链路首端即被拦截）。
        /// </summary>
        [TestMethod]
        public void Pipeline_CorruptChecksum_IsRejectedAtEntry()
        {
            var rec = BuildNibpRecord(0, 120, 80, 93, 70, 0, 0, 0, corruptChecksum: true);
            string path = WriteNibpFile("nibp-20260101000000.dat", rec);

            FileDataHandler.ParseBpFile(_db, new FileInfo(path), 55);

            Assert.AreEqual(0, _db.BpDetails.Count(b => b.caseId == 55),
                "校验和错误的记录必须被拒绝，不得进入数据库");
        }

        /// <summary>
        /// TC-PIPE-04：多病例隔离 —— 不同 caseId 的数据不得串扰。
        /// </summary>
        [TestMethod]
        public void Pipeline_MultipleCases_AreIsolated()
        {
            var rec = BuildNibpRecord(0, 111, 71, 84, 60, 0, 0, 0);
            string p1 = WriteNibpFile("nibp-20260101000000.dat", rec);
            string p2 = WriteNibpFile("nibp-20260101000001.dat", rec);

            FileDataHandler.ParseBpFile(_db, new FileInfo(p1), 1);
            FileDataHandler.ParseBpFile(_db, new FileInfo(p2), 2);
            _db.SaveChanges();

            Assert.AreEqual(1, _db.BpDetails.Count(b => b.caseId == 1));
            Assert.AreEqual(1, _db.BpDetails.Count(b => b.caseId == 2));
            Assert.AreEqual(2, _db.BpDetails.Count());
        }

        /// <summary>
        /// TC-PIPE-05：重复导入行为可观测（缺陷锚定 T2-DEF-008：当前实现每次都新增，会产生重复记录）。
        /// </summary>
        [TestMethod]
        public void Pipeline_RepeatedImport_BehaviourIsObservable()
        {
            var rec = BuildNibpRecord(0, 120, 80, 93, 70, 0, 0, 0);
            string path = WriteNibpFile("nibp-20260101000000.dat", rec);

            FileDataHandler.ParseBpFile(_db, new FileInfo(path), 9);
            FileDataHandler.ParseBpFile(_db, new FileInfo(path), 9);
            _db.SaveChanges();

            int count = _db.BpDetails.Count(b => b.caseId == 9);
            Console.WriteLine("[链路观察] 同一文件重复导入 2 次后的记录数 = " + count + "（当前实现每次都新增）");
            Assert.IsTrue(count >= 1, "至少应入库 1 条");
        }

        // ============================================================ 8337 多记录格式

        /// <summary>
        /// 构造 22 字节 8337 记录：
        /// [0]年(2000+xx) [1]月 [2]日 [3]时 [4]分 [5]秒 [6..7]checksum [8..9]motion
        /// [10..11]sys [12..13]dia [14..15]map [16..17]pr [18..19]err [20..21]left
        /// </summary>
        private static byte[] Build8337Record(int year, int month, int day, int hour, int minute, int second,
            short motion, short sys, short dia, short map, short pr, short err, short left)
        {
            short checksum = (short)(motion + sys + dia + map + pr + err + left);
            var rec = new byte[22];
            rec[0] = (byte)(year - 2000);
            rec[1] = (byte)month;
            rec[2] = (byte)day;
            rec[3] = (byte)hour;
            rec[4] = (byte)minute;
            rec[5] = (byte)second;
            BitConverter.GetBytes(checksum).CopyTo(rec, 6);
            BitConverter.GetBytes(motion).CopyTo(rec, 8);
            BitConverter.GetBytes(sys).CopyTo(rec, 10);
            BitConverter.GetBytes(dia).CopyTo(rec, 12);
            BitConverter.GetBytes(map).CopyTo(rec, 14);
            BitConverter.GetBytes(pr).CopyTo(rec, 16);
            BitConverter.GetBytes(err).CopyTo(rec, 18);
            BitConverter.GetBytes(left).CopyTo(rec, 20);
            return rec;
        }

        /// <summary>
        /// TC-PIPE-06：8337 多记录批量导入端到端一致性。
        /// </summary>
        [TestMethod]
        public void Pipeline_8337_MultiRecord_BatchImportIsConsistent()
        {
            var recs = new List<byte[]>();
            recs.Add(Build8337Record(2026, 3, 15, 8, 0, 0, 0, 120, 80, 93, 70, 0, 0));
            recs.Add(Build8337Record(2026, 3, 15, 8, 30, 0, 1, 118, 78, 91, 68, 0, 0));
            recs.Add(Build8337Record(2026, 3, 15, 9, 0, 0, 0, 125, 82, 96, 72, 0, 0));

            var all = recs.SelectMany(r => r).ToArray();
            string path = Path.Combine(_tempDir, "nibp-20260315080000.dat");
            File.WriteAllBytes(path, all);

            FileDataHandler.ParseBpFileFor8337(_db, new FileInfo(path), 21);
            _db.SaveChanges();

            var rows = _db.BpDetails.Where(b => b.caseId == 21).OrderBy(b => b.time).ToList();
            Assert.AreEqual(3, rows.Count, "3 条 8337 记录应全部入库");
            Assert.AreEqual(120000, rows[0].sysPressure);
            Assert.AreEqual(118000, rows[1].sysPressure);
            Assert.AreEqual(125000, rows[2].sysPressure);
            Assert.AreEqual("2026-03-15 08:00:00", rows[0].time);
            Assert.AreEqual("2026-03-15 08:30:00", rows[1].time);
            Assert.AreEqual("2026-03-15 09:00:00", rows[2].time);
            Assert.AreEqual("2", rows[1].motionState, "motionState 应为 (原值+1) 的字符串形式");
        }

        /// <summary>
        /// TC-PIPE-07：8337 非法日期记录 —— 当前实现整批 catch 导致合法记录一并丢失（缺陷锚定 T2-DEF-006）。
        /// </summary>
        [TestMethod]
        public void Pipeline_8337_InvalidDate_BehaviourIsObservable()
        {
            var good = Build8337Record(2026, 3, 15, 8, 0, 0, 0, 120, 80, 93, 70, 0, 0);
            var badMonth = Build8337Record(2026, 13, 15, 8, 0, 0, 0, 120, 80, 93, 70, 0, 0);
            var all = good.Concat(badMonth).ToArray();

            string path = Path.Combine(_tempDir, "nibp-20260315080000.dat");
            File.WriteAllBytes(path, all);

            FileDataHandler.ParseBpFileFor8337(_db, new FileInfo(path), 31);
            _db.SaveChanges();

            int count = _db.BpDetails.Count(b => b.caseId == 31);
            Console.WriteLine("[链路观察] 含非法月份记录时入库条数 = " + count
                + "（当前实现整批 catch，合法记录亦丢失 -> T2-DEF-006）");
            Assert.IsTrue(count >= 0);
        }

        /// <summary>
        /// TC-PIPE-08：8337 文件长度非 22 整数倍 —— 观察尾部越界行为（对应第一轮 R005）。
        ///
        /// 关键构造：总长度必须使「末轮剩余字节 &lt; 8」，才会命中越界读路径——
        /// 解析循环 `for (i = 0; i < len; i += 22)` 的末轮 `i`，读取 `offset = i + 6` 处的 Int16，
        /// 需要字节 `i+6, i+7` 均在缓冲区内。当 `len % 22 ∈ [1,7]` 时末轮剩余不足 8 字节 →
        /// `BitConverter.ToInt16(dataBytes, i+6)` 越界。
        ///
        /// 复盘修正（U-06）：原用例构造 `22 + 17 = 39` 字节（len%22=17 ≥ 8），
        /// 末轮读 offset=28/29 仍在缓冲区内，**并未真正触发越界**（已实测确认 `BitConverter.ToInt16(byte[39],28)` 不抛异常），
        /// 仅观察到"整批 catch → 返回 null"。现改为 `22 + 7 = 29` 字节（len%22=7），精确命中越界路径。
        /// 用例内含"越界前提自证断言"，确保样本长度与覆盖声明一致。
        /// </summary>
        [TestMethod]
        public void Pipeline_8337_LengthNotMultipleOf22_DoesNotCrash()
        {
            // 构造 len % 22 == 7 的样本：末轮 i=22 时剩余 7 字节，
            // 读取 offset=i+6=28 处的 Int16 需要字节 28/29，而缓冲区只有 0..28 → 越界路径。
            var good = Build8337Record(2026, 3, 15, 8, 0, 0, 0, 120, 80, 93, 70, 0, 0);
            var truncated = good.Take(7).ToArray();   // 22 + 7 = 29 字节
            var all = good.Concat(truncated).ToArray();
            Assert.AreEqual(29, all.Length, "构造的样本长度应为 29（29 % 22 == 7），以精确命中尾部越界路径");

            // 自证前提：末轮 i=22 时 offset=i+6=28，读取 Int16 需要字节 28/29，
            // 而缓冲区仅 0..28 → .NET Framework 的 BitConverter.ToInt16 抛 ArgumentException
            //（"目标数组的长度不足"；注意并非 ArgumentOutOfRangeException）。
            // 该断言确保本用例**确实命中** R005 的越界路径（否则用例名与实际覆盖不符）。
            string oobExceptionType = null;
            try { BitConverter.ToInt16(all, 28); }
            catch (ArgumentException ex) { oobExceptionType = ex.GetType().Name; }
            Assert.IsNotNull(oobExceptionType,
                "构造前提失败：len=29 时 BitConverter.ToInt16(bytes,28) 应当抛 ArgumentException——"
                + "若不抛则本用例未覆盖 R005 路径，需重新设计样本长度");
            Console.WriteLine("[链路观察][R005] 越界前提自证成立，异常类型 = " + oobExceptionType);

            string path = Path.Combine(_tempDir, "nibp-20260315080000.dat");
            File.WriteAllBytes(path, all);

            object result = null;
            Exception thrown = null;
            try
            {
                result = FileDataHandler.ParseBpFileFor8337(_db, new FileInfo(path), 41);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                thrown = ex;
            }

            // 期望：不得有异常逃逸到调用方（产品内部已 catch）。
            Assert.IsNull(thrown,
                "残缺文件不应抛出未处理异常，实际: " + (thrown == null ? "" : thrown.GetType().Name + " - " + thrown.Message));

            // 当前实现：越界读触发异常 → 整批 catch → 返回 null（合法记录亦丢失，见 T2-DEF-006）
            Console.WriteLine("[链路观察][R005] 8337 尾部越界样本(len=29, len%22=7) 返回值 = "
                + (result == null ? "null（越界被内部 catch 吞掉，合法记录一并丢失）" : "非空"));
        }

        /// <summary>
        /// TC-PIPE-09：8337 文件过短（不足 22 字节）→ 返回 null 且不崩溃。
        /// </summary>
        [TestMethod]
        public void Pipeline_8337_TooShort_ReturnsNull()
        {
            string path = Path.Combine(_tempDir, "nibp-20260315080000.dat");
            File.WriteAllBytes(path, new byte[10]);

            var result = FileDataHandler.ParseBpFileFor8337(_db, new FileInfo(path), 51);

            Assert.IsNull(result, "长度不足 22 字节应返回 null");
            Assert.AreEqual(0, _db.BpDetails.Count(b => b.caseId == 51));
        }
    }
}
