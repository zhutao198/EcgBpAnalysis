using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EcgBpAnalysis.T2.Tests.Interop
{
    /// <summary>
    /// 把 C# 侧的 DllImport DLL 名（如 lib\unmanaged\libaecg.dll）解析为仓库内的实际文件路径。
    /// </summary>
    public static class NativeDllLocator
    {
        /// <summary>
        /// 候选搜索根，按优先级。
        /// </summary>
        private static readonly string[] SearchRoots = new[]
        {
            @"EcgAnalyseStart\bin\Debug",
            @"EcgAnalyseStart\bin\Release",
            @"EcgAnalyseStart\lib",
            @"algorithm\新版本",
            @"TEST\native-deps",
            @"TEST2\native-deps",
            @""
        };

        public static string Find(string repoRoot, string dllName)
        {
            // 归一化：把反斜杠换成斜杠，取文件名，再按文件名全仓搜索
            string fileName = dllName.Replace('/', '\\');
            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

            // 1) 先按"声明的相对路径"直接拼（含 lib\unmanaged 等子目录语义）
            foreach (var r in SearchRoots)
            {
                var p = Path.Combine(repoRoot, Path.Combine(r, dllName.Replace('\\', Path.DirectorySeparatorChar)));
                if (File.Exists(p)) return p;
            }

            // 2) 按文件名全仓搜索（排除 obj/bin 之外的优先目录）
            var hits = new List<string>();
            foreach (var r in SearchRoots)
            {
                var dir = Path.Combine(repoRoot, r);
                if (!Directory.Exists(dir)) continue;
                try
                {
                    foreach (var f in Directory.GetFiles(dir, fileName, SearchOption.AllDirectories))
                    {
                        if (f.Contains(@"\obj\")) continue;
                        hits.Add(f);
                    }
                }
                catch { }
                if (hits.Count > 0) break;
            }
            return hits.Count > 0 ? hits[0] : null;
        }

        /// <summary>
        /// 找出仓库中所有与该 DLL 名相关的实际文件（用于位数一致性检查）。
        /// </summary>
        public static List<string> FindAll(string repoRoot, string fileName)
        {
            var result = new List<string>();
            foreach (var r in SearchRoots)
            {
                var dir = Path.Combine(repoRoot, r);
                if (!Directory.Exists(dir)) continue;
                try
                {
                    foreach (var f in Directory.GetFiles(dir, fileName, SearchOption.AllDirectories))
                    {
                        if (f.Contains(@"\obj\") || f.Contains(@"\bin\Debug\obj\")) continue;
                        result.Add(f);
                    }
                }
                catch { }
            }
            return result;
        }
    }

    /// <summary>
    /// 从 C/C++ 头文件提取函数声明名，用于与 C# DllImport 交叉比对。
    /// </summary>
    public static class HeaderFunctionExtractor
    {
        // 形如: int aecgGetBeatsInfo(void *handle, BeatInfo **beatInfo, int *beatsNum);
        private static readonly Regex FuncDecl = new Regex(
            @"(?:^|[\s\*&])(?<ret>[A-Za-z_][\w:]*)\s+(?<name>[A-Za-z_]\w*)\s*\((?<args>[^;{)]*)\)\s*;",
            RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly HashSet<string> Keywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "if","else","for","while","return","sizeof","typedef","struct","enum","union",
            "extern","static","const","void","int","char","float","double","short","long","unsigned","signed"
        };

        public static HashSet<string> ExtractFunctionNames(string headerPath)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            if (!File.Exists(headerPath)) return names;

            string text;
            try { text = File.ReadAllText(headerPath); }
            catch { return names; }

            // 去掉注释，避免把注释里的东西当声明
            text = Regex.Replace(text, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            text = Regex.Replace(text, @"//[^\n]*", " ");

            foreach (Match m in FuncDecl.Matches(text))
            {
                string name = m.Groups["name"].Value;
                if (Keywords.Contains(name)) continue;
                if (name.StartsWith("aecg") || name.StartsWith("nibp") || name.StartsWith("usb_")
                    || name.StartsWith("TransZQ") || name.StartsWith("set") || name.StartsWith("get"))
                {
                    names.Add(name);
                }
            }
            return names;
        }

        /// <summary>
        /// 提取头文件中的结构体名与字段（用于结构体布局核对）。
        /// </summary>
        public static Dictionary<string, List<string>> ExtractStructs(string headerPath)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            if (!File.Exists(headerPath)) return result;

            string text;
            try { text = File.ReadAllText(headerPath); }
            catch { return result; }

            text = Regex.Replace(text, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            text = Regex.Replace(text, @"//[^\n]*", " ");

            var structRe = new Regex(@"typedef\s+struct[^;{]*\{(?<body>[^}]*)\}\s*(?<name>\w+)\s*;",
                RegexOptions.Singleline | RegexOptions.Compiled);
            var fieldRe = new Regex(@"(?<type>[\w:]+)\s+(?<name>\w+)\s*(?:\[[^\]]*\])?\s*;",
                RegexOptions.Compiled);

            foreach (Match sm in structRe.Matches(text))
            {
                string name = sm.Groups["name"].Value;
                if (string.IsNullOrEmpty(name)) continue;
                var fields = new List<string>();
                foreach (Match fm in fieldRe.Matches(sm.Groups["body"].Value))
                {
                    string fn = fm.Groups["name"].Value;
                    if (Keywords.Contains(fn)) continue;
                    fields.Add(fm.Groups["type"].Value + " " + fn);
                }
                result[name] = fields;
            }
            return result;
        }
    }
}
