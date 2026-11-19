using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EcgBpAnalysis.T2.Tests.Interop
{
    /// <summary>
    /// 配置契约一致性检查：生产端写入的键名必须能被消费端（HolterConfig）识别。
    ///
    /// 价值：HolterConfig 反序列化使用 MissingMemberHandling.Ignore + NullValueHandling.Ignore，
    ///       键名不匹配时**不抛异常也不记日志**——只能通过静态比对发现。
    ///       一旦键名漂移，算法参数（如灵敏度）会静默丢失，直接影响分析结果。
    ///
    /// 关联缺陷：T2-DEF-036（rSensitivity 在消费端无对应属性）。
    /// </summary>
    [TestClass]
    public class ConfigContractTests
    {
        private static string _repoRoot;

        [TestInitialize]
        public void Setup()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            _repoRoot = null;
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "EcgBpAnalysis.sln"))) { _repoRoot = dir.FullName; break; }
                dir = dir.Parent;
            }
            Assert.IsFalse(string.IsNullOrEmpty(_repoRoot), "未能定位仓库根目录");
        }

        /// <summary>
        /// C-CONFIG-01：扫描器自检——必须能提取到足量的 JsonProperty 键与生产端动态键。
        /// 若此用例失败，说明提取规则失效，后续结论不可信。
        /// </summary>
        [TestMethod]
        public void C_CONFIG_01_ScannerExtractsSufficientKeys()
        {
            var configFile = Path.Combine(_repoRoot, @"Zoncare.Common\HolterConfig.cs");
            var producerFile = Path.Combine(_repoRoot, @"AnalysisSystem.Base\AppCode.cs");

            var consumer = ConfigContractScanner.ExtractConsumerKeys(configFile);
            var producer = ConfigContractScanner.ExtractProducerKeys(producerFile, "analysisSetting");

            Assert.IsTrue(consumer.Count >= 40,
                $"消费端应至少提取到 40 个 JsonProperty 键，实际 {consumer.Count} —— 提取规则可能失效");
            Assert.IsTrue(producer.Count >= 20,
                $"生产端应至少提取到 20 个 analysisSetting 键，实际 {producer.Count} —— 提取规则可能失效");

            // 抽查已知键
            Assert.IsTrue(consumer.Contains("sampleRate"), "应提取到 sampleRate");
            Assert.IsTrue(consumer.Contains("fileType"), "应提取到 fileType");
            Assert.IsTrue(producer.Contains("mainChannel"), "应提取到 mainChannel");
        }

        /// <summary>
        /// C-CONFIG-02：生产端写入的 analysisSetting 键必须全部能被消费端识别。
        /// 差异 = 配置参数在往返中静默丢失（缺陷锚定 T2-DEF-036）。
        /// </summary>
        [TestMethod]
        public void C_CONFIG_02_ProductionKeysAreAllConsumable()
        {
            var configFile = Path.Combine(_repoRoot, @"Zoncare.Common\HolterConfig.cs");
            var producerFile = Path.Combine(_repoRoot, @"AnalysisSystem.Base\AppCode.cs");

            var consumer = ConfigContractScanner.ExtractConsumerKeys(configFile);
            var producer = ConfigContractScanner.ExtractProducerKeys(producerFile, "analysisSetting");

            var lost = producer.Where(k => !consumer.Contains(k)).OrderBy(k => k).ToList();

            if (lost.Count > 0)
            {
                // 已知差异登记（当前已确认的 T2-DEF-036）
                var known = new List<string> { "rSensitivity" };
                var unknown = lost.Where(k => !known.Contains(k)).ToList();

                if (unknown.Count > 0)
                {
                    Assert.Fail("以下生产端写入的配置键在消费端 HolterConfig 中无对应 JsonProperty，"
                        + "往返时被静默丢弃（未登记）:\n  " + string.Join("\n  ", unknown));
                }

                Console.WriteLine("[已知缺陷 T2-DEF-036] 配置键往返丢失（已登记）: " + string.Join(", ", lost));
            }
        }

        /// <summary>
        /// C-CONFIG-03：消费端各嵌套类的 JsonProperty 键不得重复（重复键会被后者覆盖）。
        /// </summary>
        [TestMethod]
        public void C_CONFIG_03_ConsumerKeysAreUniquePerClass()
        {
            var configFile = Path.Combine(_repoRoot, @"Zoncare.Common\HolterConfig.cs");
            var byClass = ConfigContractScanner.ExtractConsumerKeysByClass(configFile);

            var problems = new List<string>();
            foreach (var kv in byClass)
            {
                var dup = kv.Value.GroupBy(k => k).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (dup.Count > 0)
                    problems.Add(kv.Key + " → " + string.Join(", ", dup));
            }

            if (problems.Count > 0)
            {
                Assert.Fail("以下配置类存在重复的 JsonProperty 键（后者会覆盖前者）:\n  "
                    + string.Join("\n  ", problems));
            }
        }

        /// <summary>
        /// C-CONFIG-04：JsonProperty 键与其 C# 属性名应对应到同一概念。
        /// 比较时忽略大小写与分隔符（_ -），因此 snake_case 与 PascalCase 混用不算问题，
        /// 但拼写错误（如 sensitvity vs Sensitivity）会被发现——那会导致读写错位。
        /// </summary>
        [TestMethod]
        public void C_CONFIG_04_JsonPropertyNamesDoNotCollideWithPropertyNames()
        {
            var configFile = Path.Combine(_repoRoot, @"Zoncare.Common\HolterConfig.cs");
            string text = File.ReadAllText(configFile);

            // 形如: [JsonProperty("x")] ... public type X
            var re = new System.Text.RegularExpressions.Regex(
                @"\[JsonProperty\s*\(\s*""(?<key>[^""]+)""\s*\)\s*\](?:\s*\[[^\]]*\])*\s*public\s+[\w\.\<\>\[\]?]+\s+(?<name>\w+)");
            var collisions = new List<string>();
            foreach (System.Text.RegularExpressions.Match m in re.Matches(text))
            {
                string key = m.Groups["key"].Value;
                string name = m.Groups["name"].Value;
                if (!string.Equals(Normalize(key), Normalize(name), StringComparison.Ordinal))
                    collisions.Add(key + " → " + name);
            }

            if (collisions.Count > 0)
            {
                Assert.Fail("以下 JsonProperty 键与其 C# 属性名不对应同一概念（忽略大小写与分隔符后仍不同），"
                    + "易造成读写错位:\n  " + string.Join("\n  ", collisions));
            }
        }

        /// <summary>
        /// 规范化：去掉分隔符并转小写，用于比较两个标识符是否指同一概念。
        /// </summary>
        private static string Normalize(string s)
        {
            return new string(s.Where(c => c != '_' && c != '-').ToArray()).ToLowerInvariant();
        }
    }
}
