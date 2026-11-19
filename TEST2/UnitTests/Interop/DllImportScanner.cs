using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EcgBpAnalysis.T2.Tests.Interop
{
    /// <summary>
    /// 通过源码扫描提取 DllImport 声明（M1 契约比对的第一步）。
    ///
    /// 为什么不使用反射：产品程序集（AnalysisApp.dll / EcgBpAnalysis.exe）依赖链庞大
    /// （SkiaSharp/OpenTK/EF Core/…），测试工程直接引用会引入依赖地狱且无法覆盖 exe 内的声明。
    /// 源码扫描 + PE 导出表解析是零依赖、可覆盖全仓库的方案。
    /// </summary>
    public class DllImportDeclaration
    {
        public string SourceFile;      // 相对路径
        public int LineNumber;        // DllImport 属性所在行
        public string DllName;         // DllImport 第一参数
        public string EntryPoint;      // EntryPoint（缺省则取方法名）
        public string MethodName;      // C# 方法名
        public string CallingConvention;
        public string CharSet;
        public string ReturnType;
        public List<string> Parameters = new List<string>();
        public bool HasEntryPointAttribute;

        public string ResolvedEntryPoint
        {
            get { return HasEntryPointAttribute ? EntryPoint : MethodName; }
        }

        public override string ToString()
        {
            return string.Format("{0}({1}) [{2}] cc={3} charset={4}",
                MethodName, ResolvedEntryPoint, DllName, CallingConvention, CharSet);
        }
    }

    /// <summary>
    /// 源码扫描器：解析所有 .cs 文件中的 [DllImport(...)] 声明。
    /// </summary>
    public static class DllImportScanner
    {
        // 匹配 [DllImport("dll", ...)] 整个特性块（支持跨行）
        private static readonly Regex DllImportAttr = new Regex(
            @"\[DllImport\s*\((?<args>.*?)\)\s*\]",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // 匹配紧随其后的方法签名
        private static readonly Regex MethodSig = new Regex(
            @"(?:public|private|internal|protected|static|extern|unsafe|\s)+"
            + @"(?:static\s+)?extern\s+(?<ret>[\w\.\<\>\[\]]+)\s+(?<name>\w+)\s*\((?<args>[^)]*)\)",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex StringArg = new Regex(@"""(?<v>[^""]*)""");
        private static readonly Regex NamedArg = new Regex(@"(?<k>\w+)\s*=\s*(?<v>[^,]+)");

        /// <summary>
        /// 匹配常量定义：const string NAME = "value"; / const string NAME = @"value";
        /// 用于解析 [DllImport(DLL_PATH, ...)] 这类以常量表达的 DLL 名。
        /// </summary>
        private static readonly Regex ConstDef = new Regex(
            @"(?:const|static\s+readonly)\s+string\s+(?<name>\w+)\s*=\s*(?<val>@?""(?<v>[^""]*)"")",
            RegexOptions.Compiled);

        private class ConstDefEntry
        {
            public string Name;
            public string Value;
            public int Line;
        }

        /// <summary>
        /// 解析 DLL 名：若首参是字符串字面量直接取；若是标识符，则查找**该位置之前最近的同名常量**。
        /// 注意：同一文件的不同类可以有同名常量（如 DataHandler.cs 中的两个 winDrivers，
        ///       分别指向 testWave.dll 与 libnibp_dynamic_d_d.dll），
        ///       故必须按行序就近解析，不能用"名称→值"的全局字典。
        /// </summary>
        private static string ResolveDllName(string args, List<ConstDefEntry> consts, int declLine)
        {
            // 取 DllImport 的第一个参数（顶层逗号前）
            int depth = 0, end = args.Length;
            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];
                if (c == '(') depth++;
                else if (c == ')') { if (depth == 0) { end = i; break; } depth--; }
                else if (c == ',' && depth == 0) { end = i; break; }
            }
            string first = args.Substring(0, end).Trim();

            var sm = StringArg.Match(first);
            if (sm.Success) return sm.Groups["v"].Value;

            // 标识符形式 → 找该行之前最近的同名常量
            string ident = first.Trim();
            string best = null;
            int bestLine = -1;
            foreach (var c in consts)
            {
                if (c.Name != ident) continue;
                if (c.Line > declLine) continue;      // 只能取之前定义的
                if (c.Line > bestLine) { bestLine = c.Line; best = c.Value; }
            }
            return best;
        }

        public static List<DllImportDeclaration> Scan(string repoRoot, string[] sourceFolders)
        {
            var result = new List<DllImportDeclaration>();
            foreach (var folder in sourceFolders)
            {
                var dir = Path.Combine(repoRoot, folder);
                if (!Directory.Exists(dir)) continue;

                foreach (var file in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
                {
                    if (file.Contains(@"\obj\") || file.Contains(@"\bin\") || file.Contains(@"packages\")) continue;
                    ScanFile(file, repoRoot, result);
                }
            }
            return result;
        }

        private static void ScanFile(string file, string repoRoot, List<DllImportDeclaration> sink)
        {
            string text;
            try { text = File.ReadAllText(file); }
            catch { return; }

            // 收集本文件的常量定义（保留行号，供作用域解析）
            var consts = new List<ConstDefEntry>();
            foreach (Match cm in ConstDef.Matches(text))
            {
                consts.Add(new ConstDefEntry
                {
                    Name = cm.Groups["name"].Value,
                    Value = cm.Groups["v"].Value,
                    Line = CountLines(text, cm.Index)
                });
            }

            foreach (Match m in DllImportAttr.Matches(text))
            {
                string args = m.Groups["args"].Value;
                int declLine = CountLines(text, m.Index);

                // DLL 名：第一个参数（可能是常量）
                string dllName = ResolveDllName(args, consts, declLine);
                if (string.IsNullOrEmpty(dllName)) continue;

                // 命名参数
                string entryPoint = null, cc = null, cs = null;
                bool hasEp = false;
                foreach (Match na in NamedArg.Matches(args))
                {
                    string k = na.Groups["k"].Value;
                    string v = na.Groups["v"].Value.Trim();
                    switch (k)
                    {
                        case "EntryPoint": entryPoint = StringArg.Match(v).Groups["v"].Value; hasEp = true; break;
                        case "CallingConvention": cc = v; break;
                        case "CharSet": cs = v; break;
                    }
                }

                // 方法签名（从特性结束后开始找）
                int searchFrom = m.Index + m.Length;
                if (searchFrom >= text.Length) continue;
                var tail = text.Substring(searchFrom, Math.Min(400, text.Length - searchFrom));
                var mm = MethodSig.Match(tail);
                if (!mm.Success) continue;

                string methodName = mm.Groups["name"].Value;
                string ret = mm.Groups["ret"].Value;
                string paramText = mm.Groups["args"].Value;

                var decl = new DllImportDeclaration
                {
                    SourceFile = file.Substring(repoRoot.Length).TrimStart('\\', '/'),
                    LineNumber = declLine,
                    DllName = dllName,
                    EntryPoint = entryPoint,
                    MethodName = methodName,
                    HasEntryPointAttribute = hasEp,
                    CallingConvention = cc ?? "(默认 Winapi)",
                    CharSet = cs ?? "(默认 None)",
                    ReturnType = ret
                };
                foreach (var p in paramText.Split(','))
                {
                    string t = p.Trim();
                    if (t.Length > 0) decl.Parameters.Add(t);
                }
                sink.Add(decl);
            }
        }

        private static int CountLines(string text, int index)
        {
            int line = 1;
            for (int i = 0; i < index && i < text.Length; i++)
                if (text[i] == '\n') line++;
            return line;
        }
    }
}
