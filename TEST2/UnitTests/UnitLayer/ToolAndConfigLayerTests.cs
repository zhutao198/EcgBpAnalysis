using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zoncare.Common;
using Zoncare.Utilities;

namespace EcgBpAnalysis.T2.Tests.UnitLayer
{
    /// <summary>
    /// 工具层与配置层单元测试（TC-UTIL-* / TC-CFG-*）。
    ///
    /// 选点依据：这两层是纯函数 / 纯数据对象，无 UI 依赖，天然可测；
    ///           且直接承载患者数据换算与配置读取，风险最高、性价比最优。
    ///
    /// 关联缺陷：T2-DEF-031（单位精度）、T2-DEF-032（区域依赖）、
    ///           T2-DEF-033（文件名解析）、T2-DEF-034（StartDateTime 兜底）、
    ///           T2-DEF-037（年龄折算误差）。
    /// </summary>
    [TestClass]
    public class ToolAndConfigLayerTests
    {
        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "t2util_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); } catch { }
        }

        private string WriteJson(string content)
        {
            string path = Path.Combine(_tempDir, "holter.info");
            File.WriteAllText(path, content, new System.Text.UTF8Encoding(false));
            return path;
        }

        // ============================================== TC-UTIL-UNIT 血压单位换算

        /// <summary>
        /// TC-UTIL-UNIT-01：mmHg → kPa 换算的正常与边界值。
        /// 系数 0.133322 为标准换算值（1 mmHg = 0.133322 kPa）。
        /// </summary>
        [TestMethod]
        public void TC_UTIL_UNIT_01_mmHg2kPa_正常值与边界值()
        {
            Assert.AreEqual(16.0, CommonFuns.mmHg2KPa(120), 1e-9, "120 mmHg 应约等于 16.0 kPa");
            Assert.AreEqual(0.0, CommonFuns.mmHg2KPa(0), 1e-9, "0 mmHg 应换算为 0");
            Assert.AreEqual(10.7, CommonFuns.mmHg2KPa(80), 1e-9, "80 mmHg 应约等于 10.7 kPa");
            Assert.AreEqual(16.0, CommonFuns.mmHg2KPa(120, 2), 1e-9, "指定 2 位小数应仍为 16.0");
            Assert.AreEqual(15.999, CommonFuns.mmHg2KPa(120, 3), 1e-6, "指定 3 位小数应为 15.999");
        }

        /// <summary>
        /// TC-UTIL-UNIT-02：kPa → mmHg 换算（缺陷锚定 T2-DEF-031）。
        /// 当前实现返回 int，小数部分被截断：12.98 kPa → 97（正确应为 97.3）。
        /// 修复后应改为返回 double，本用例期望随之更新。
        /// </summary>
        [TestMethod]
        public void TC_UTIL_UNIT_02_kPa2mmHg_小数被截断_缺陷锚定()
        {
            Assert.AreEqual(120, CommonFuns.kPa2mmHg(16.0), "16 kPa 应换算为 120 mmHg");
            Assert.AreEqual(120, CommonFuns.kPa2mmHg(16.0d), "16 kPa 应换算为 120 mmHg");

            // 缺陷锚定：12.98 kPa 精确应为 97.3 mmHg，当前返回 int 被截断为 97
            int actual = CommonFuns.kPa2mmHg(12.98);
            Console.WriteLine("[缺陷锚定 T2-DEF-031] kPa2mmHg(12.98) = " + actual
                + "（精确值应为 97.3，返回 int 导致小数丢失）");
            Assert.AreEqual(97, actual,
                "当前行为锚定：返回 int，小数丢失。修复 T2-DEF-031 后应改为断言 97.3（需改返回类型为 double）");
        }

        /// <summary>
        /// TC-UTIL-UNIT-03：mmHg → kPa 的负值与超大值不应抛异常。
        /// </summary>
        [TestMethod]
        public void TC_UTIL_UNIT_03_mmHg2kPa_负值与超大值不抛异常()
        {
            double neg = CommonFuns.mmHg2KPa(-1);
            double big = CommonFuns.mmHg2KPa(100000);
            Assert.IsTrue(neg < 0, "负值应保留符号");
            Assert.IsTrue(big > 0, "超大值应正常换算");
        }

        // ============================================== TC-UTIL-AGE 年龄折算

        /// <summary>
        /// TC-UTIL-AGE-01：年龄天数折算（缺陷锚定 T2-DEF-037）。
        /// 当前实现按 366 天/年、30 天/月折算，10 岁误差可达 +13 天，
        /// 影响按天检索/过滤病例。
        /// </summary>
        [TestMethod]
        public void TC_UTIL_AGE_01_按固定天数折算_误差可累积_缺陷锚定()
        {
            Assert.AreEqual(3660, CommonFuns.GetAgeDays(10, "Y"), "当前实现：10 岁 = 10*366 天");
            Assert.AreEqual(60, CommonFuns.GetAgeDays(2, "M"), "当前实现：2 月 = 2*30 天");
            Assert.AreEqual(5, CommonFuns.GetAgeDays(5, "D"), "天应原样返回");

            // 缺陷锚定：真实日历下 10 岁约 3652~3653 天，误差 7~8 天
            int approxReal = (DateTime.Now - DateTime.Now.AddYears(-10)).Days;
            int current = CommonFuns.GetAgeDays(10, "Y");
            Console.WriteLine("[缺陷锚定 T2-DEF-037] 10 岁折算天数 = " + current
                + "，真实日历约 " + approxReal + " 天，误差 " + (current - approxReal) + " 天");
            Assert.AreEqual(3660, current, "当前行为锚定：固定 366 天/年。修复后应改用日历差");
        }

        /// <summary>
        /// TC-UTIL-AGE-02：未知单位不应抛异常。
        /// </summary>
        [TestMethod]
        public void TC_UTIL_AGE_02_未知单位不抛异常()
        {
            int r = CommonFuns.GetAgeDays(10, "X");
            Console.WriteLine("[观察] 未知单位 'X' 返回 " + r + " 天");
            Assert.IsTrue(r >= 0);
        }

        // ============================================== TC-UTIL-REGION 区域依赖

        /// <summary>
        /// TC-UTIL-REGION-01：数值小数位数判定不得依赖当前区域（缺陷锚定 T2-DEF-032）。
        /// 产品支持俄语（区域 1049，小数点为 ','），当前实现用 ToString() 判定，
        /// 在该区域下小数位被判定为 0，且 "12.5" 会被解析为 125。
        /// </summary>
        [TestMethod]
        public void TC_UTIL_REGION_01_小数位数判定_俄语区域下失效_缺陷锚定()
        {
            var original = Thread.CurrentThread.CurrentCulture;
            try
            {
                // 中文/英文环境（正常）
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                int en = PubFuns.GetNumberOfDecimalPlaces(12.5d);
                Assert.AreEqual(1, en, "英文区域下应判定为 1 位小数");

                // 俄语环境（产品支持）：小数分隔符为 ','
                Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
                int ru = PubFuns.GetNumberOfDecimalPlaces(12.5d);
                Console.WriteLine("[缺陷锚定 T2-DEF-032] 12.5 在 ru-RU 区域下小数位判定 = " + ru
                    + "（英文区域为 1）");
                Assert.AreEqual(0, ru,
                    "当前行为锚定：区域依赖导致判定为 0。修复 T2-DEF-032 后应改为断言 1");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        /// <summary>
        /// TC-UTIL-REGION-02：字符串版小数位数判定不受区域影响（对照组）。
        /// </summary>
        [TestMethod]
        public void TC_UTIL_REGION_02_字符串版小数位判定_不受区域影响()
        {
            var original = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
                Assert.AreEqual(1, PubFuns.GetNumberOfDecimalPlaces("12.5"),
                    "字符串输入以 '.' 分割，不应受区域影响");
                Assert.AreEqual(0, PubFuns.GetNumberOfDecimalPlaces("12"),
                    "整数的小数位应为 0");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        // ============================================== TC-CFG 配置读取

        /// <summary>
        /// TC-CFG-01：holter.info 正常解析，字段值正确。
        /// </summary>
        [TestMethod]
        public void TC_CFG_01_正常配置解析_字段值正确()
        {
            string path = WriteJson(@"{
                ""fileType"": 3,
                ""days"": 7,
                ""sampleRate"": 250,
                ""total"": 604800000,
                ""lead"": 8,
                ""startTime"": ""2026-03-15 08:00:00"",
                ""fileList"": [""holter.dat""]
            }");

            var cfg = HolterConfig.LoadFromFile(path);

            Assert.AreEqual(3, cfg.FileType, "fileType 解析错误");
            Assert.AreEqual(7, cfg.Days, "days 解析错误");
            Assert.AreEqual(250, cfg.SampleRate, "sampleRate 解析错误");
            Assert.AreEqual(604800000, cfg.Total, "total 解析错误");
            Assert.AreEqual(8, cfg.Lead, "lead 解析错误");
            Assert.AreEqual("2026-03-15 08:00:00", cfg.StartTime, "startTime 解析错误");
            Assert.AreEqual(1, cfg.FileList.Count, "fileList 解析错误");
            Assert.AreEqual("holter.dat", cfg.FileList[0]);
        }

        /// <summary>
        /// TC-CFG-02：配置文件不存在时抛出 FileNotFoundException（明确失败而非静默）。
        /// </summary>
        [TestMethod]
        public void TC_CFG_02_配置文件不存在_抛FileNotFoundException()
        {
            string missing = Path.Combine(_tempDir, "not-exist.info");
            try
            {
                HolterConfig.LoadFromFile(missing);
                Assert.Fail("配置文件不存在时应抛出 FileNotFoundException");
            }
            catch (FileNotFoundException)
            {
                // 预期行为
            }
        }

        /// <summary>
        /// TC-CFG-04：缺少 sampleRate 字段时 SampleRate=0 → TotalSeconds 除零（缺陷锚定 T2-DEF-013）。
        /// Total&gt;0 时 0 分母得 Infinity；Total=0 时 0/0 得 NaN，两者都会污染报告数值。
        /// </summary>
        [TestMethod]
        public void TC_CFG_04_缺sampleRate字段_为0且时长计算异常_缺陷锚定()
        {
            string path = WriteJson(@"{ ""fileType"": 3, ""total"": 1000 }");

            var cfg = HolterConfig.LoadFromFile(path);

            Assert.AreEqual(0, cfg.SampleRate,
                "当前行为锚定：缺字段时 SampleRate=0（应给默认值或加载后校验）");

            // Total>0 时：1000/0 = Infinity
            double totalSeconds = (double)cfg.Total / cfg.SampleRate;
            Console.WriteLine("[缺陷锚定 T2-DEF-013] SampleRate=0 且 Total=1000 时 TotalSeconds = " + totalSeconds);
            Assert.IsTrue(double.IsInfinity(totalSeconds),
                "当前行为锚定：非零被除数除以 0 得 Infinity（转为 int 后为未定义值）");

            // Total=0 时：0/0 = NaN
            string path2 = WriteJson(@"{ ""fileType"": 3, ""total"": 0 }");
            var cfg2 = HolterConfig.LoadFromFile(path2);
            double nanSeconds = (double)cfg2.Total / cfg2.SampleRate;
            Console.WriteLine("[缺陷锚定 T2-DEF-013] SampleRate=0 且 Total=0 时 TotalSeconds = " + nanSeconds);
            Assert.IsTrue(double.IsNaN(nanSeconds), "当前行为锚定：0/0 得 NaN");
        }

        /// <summary>
        /// TC-CFG-07：子配置对象为 null 时**不会**覆盖默认值（复核结论：T2-R-032 为误报，已剔除）。
        /// 依据：HolterConfig.LoadFromFile 使用 NullValueHandling.Ignore（HolterConfig.cs:110），
        ///       JSON 中的 null 被忽略，属性保持初始化的默认实例。
        /// 本用例作为该结论的回归保护，防止将来去掉 NullValueHandling.Ignore 时静默失效。
        /// </summary>
        [TestMethod]
        public void TC_CFG_07_子配置为null_不覆盖默认值_保持非空()
        {
            string path = WriteJson(@"{
                ""fileType"": 3,
                ""analysisSetting"": null,
                ""paramsSetting"": null,
                ""leadConfig"": null
            }");

            var cfg = HolterConfig.LoadFromFile(path);

            Console.WriteLine("[复核 T2-R-032] analysisSetting=" + (cfg.AnalysisSetting == null ? "null" : "非null")
                + "，paramsSetting=" + (cfg.ParamsSetting == null ? "null" : "非null")
                + "，leadConfig=" + (cfg.LeadConfig == null ? "null" : "非null")
                + " → NullValueHandling.Ignore 生效，原审查结论 T2-R-032 判定为误报");
            Assert.IsNotNull(cfg.AnalysisSetting, "NullValueHandling.Ignore 生效，analysisSetting 应保持默认实例");
            Assert.IsNotNull(cfg.ParamsSetting, "paramsSetting 应保持默认实例");
            Assert.IsNotNull(cfg.LeadConfig, "leadConfig 应保持默认实例");
        }

        /// <summary>
        /// TC-CFG-09：保存 → 加载 往返，字段值应完全一致（配置写入链路的基本契约）。
        /// </summary>
        [TestMethod]
        public void TC_CFG_09_保存后重新加载_字段值一致()
        {
            var cfg = new HolterConfig
            {
                FileType = 5,
                Days = 3,
                SampleRate = 500,
                Total = 129600000,
                Lead = 12,
                StartTime = "2026-05-01 09:30:00"
            };
            cfg.FileList.Add("holter.dat");
            cfg.FileList.Add("holter-1.dat");

            string path = Path.Combine(_tempDir, "roundtrip.info");
            cfg.SaveToFile(path);
            Assert.IsTrue(File.Exists(path), "SaveToFile 应生成配置文件");

            var loaded = HolterConfig.LoadFromFile(path);

            Assert.AreEqual(cfg.FileType, loaded.FileType, "往返后 fileType 不一致");
            Assert.AreEqual(cfg.Days, loaded.Days, "往返后 days 不一致");
            Assert.AreEqual(cfg.SampleRate, loaded.SampleRate, "往返后 sampleRate 不一致");
            Assert.AreEqual(cfg.Total, loaded.Total, "往返后 total 不一致");
            Assert.AreEqual(cfg.Lead, loaded.Lead, "往返后 lead 不一致");
            Assert.AreEqual(cfg.StartTime, loaded.StartTime, "往返后 startTime 不一致");
            Assert.AreEqual(cfg.FileList.Count, loaded.FileList.Count, "往返后 fileList 数量不一致");
            Assert.AreEqual("holter-1.dat", loaded.FileList[1], "往返后 fileList 内容不一致");
        }
    }
}
