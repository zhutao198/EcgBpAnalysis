using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EcgBpAnalysis.T2.Tests.Interop
{
    /// <summary>
    /// 配置契约扫描器（M1 思想的延伸）：静态比对
    ///   生产端：AppCode.genHolterInfo 用 dynamic ExpandoObject 写入的键名
    ///   消费端：HolterConfig 各嵌套类的 [JsonProperty] 名
    /// 求差集 = 写入但读不回的键 → 该配置参数在往返中静默丢失。
    ///
    /// 动机：HolterConfig 使用 NullValueHandling.Ignore + MissingMemberHandling.Ignore，
    ///       键名不匹配时**不报错**（静默丢弃），只能靠静态比对发现。
    /// </summary>
    public static class ConfigContractScanner
    {
        // [JsonProperty("xxx")]  public type Name
        private static readonly Regex JsonProp = new Regex(
            @"\[JsonProperty\s*\(\s*""(?<key>[^""]+)""\s*\)\s*\]\s*(?:\[[^\]]*\]\s*)*public\s+[\w\.\<\>\[\]?]+\s+(?<name>\w+)",
            RegexOptions.Compiled);

        // ExpandoObject 动态赋值：xxx.yyy = value;
        private static readonly Regex DynamicAssign = new Regex(
            @"\b(?<obj>\w+)\.(?<key>[A-Za-z_]\w*)\s*=",
            RegexOptions.Compiled);

        /// <summary>
        /// 提取 HolterConfig 中所有 JsonProperty 名（消费端可识别的键）。
        /// </summary>
        public static HashSet<string> ExtractConsumerKeys(string configFile)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            if (!File.Exists(configFile)) return keys;

            string text;
            try { text = File.ReadAllText(configFile); }
            catch { return keys; }

            foreach (Match m in JsonProp.Matches(text))
                keys.Add(m.Groups["key"].Value);

            return keys;
        }

        /// <summary>
        /// 提取生产端对指定 dynamic 变量（如 analysisSetting / paramsSetting / leadConfig）
        /// 写入的键名。
        /// </summary>
        public static HashSet<string> ExtractProducerKeys(string producerFile, string dynamicVarName)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            if (!File.Exists(producerFile)) return keys;

            string text;
            try { text = File.ReadAllText(producerFile); }
            catch { return keys; }

            // 只取 dynamic 声明之后的部分（避免误采其它对象的同名赋值）
            int declIdx = text.IndexOf("dynamic " + dynamicVarName, StringComparison.Ordinal);
            if (declIdx < 0) return keys;

            foreach (Match m in DynamicAssign.Matches(text.Substring(declIdx)))
            {
                if (m.Groups["obj"].Value == dynamicVarName)
                    keys.Add(m.Groups["key"].Value);
            }
            return keys;
        }

        /// <summary>
        /// 提取 HolterConfig 中嵌套类名与其 JsonProperty 名的映射（用于定位差异归属）。
        /// </summary>
        public static Dictionary<string, List<string>> ExtractConsumerKeysByClass(string configFile)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            if (!File.Exists(configFile)) return result;

            string text;
            try { text = File.ReadAllText(configFile); }
            catch { return result; }

            var classRegex = new Regex(@"public\s+class\s+(?<cls>\w+)", RegexOptions.Compiled);
            var matches = classRegex.Matches(text);
            for (int i = 0; i < matches.Count; i++)
            {
                string cls = matches[i].Groups["cls"].Value;
                int start = matches[i].Index + matches[i].Length;
                int end = (i + 1 < matches.Count) ? matches[i + 1].Index : text.Length;
                var keys = new List<string>();
                foreach (Match pm in JsonProp.Matches(text.Substring(start, end - start)))
                    keys.Add(pm.Groups["key"].Value);
                result[cls] = keys;
            }
            return result;
        }
    }
}
