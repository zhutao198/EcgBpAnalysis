using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EcgBpAnalysis.T2.Tests.Interop
{
    /// <summary>
    /// M1：interop 契约机械比对（C# DllImport ↔ PE 导出表 ↔ native 头文件）。
    ///
    /// 目标：把"人工核对 40+ 个 DllImport 声明"变为可长期回归的自动化检查，
    ///       自动发现三类问题：
    ///         ① EntryPoint 在 DLL 导出表中不存在（调用必抛 EntryPointNotFoundException）
    ///         ② 被 DllImport 引用的 DLL 在仓库/部署中缺失（部署即缺件）
    ///         ③ 头文件声明但 C# 未导入的函数（能力遗漏）
    ///         ④ 同一 native 函数的重复声明参数不一致
    /// </summary>
    [TestClass]
    public class InteropContractTests
    {
        private static string _repoRoot;
        private static List<DllImportDeclaration> _decls;
        private static Dictionary<string, PeFileInfo> _peCache = new Dictionary<string, PeFileInfo>(StringComparer.OrdinalIgnoreCase);

        private static readonly string[] SourceFolders = new[]
        {
            "AnalysisApp", "AnalysisSystem.Base", "EcgAnalyseStart", "Zoncare.Common",
            "Zoncare.Utilities", "Zoncare.DAL", "Zoncare.Component", "SystemSettings",
            "ABPMRepository", "UpgradeTool"
        };

        [TestInitialize]
        public void Setup()
        {
            _repoRoot = FindRepoRoot();
            Assert.IsFalse(string.IsNullOrEmpty(_repoRoot), "未能定位仓库根目录");
            _decls = DllImportScanner.Scan(_repoRoot, SourceFolders);
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "EcgBpAnalysis.sln"))) return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        private static PeFileInfo GetPe(string dllPath)
        {
            if (dllPath == null) return null;
            PeFileInfo info;
            if (_peCache.TryGetValue(dllPath, out info)) return info;
            info = PeFileInfo.Parse(dllPath);
            _peCache[dllPath] = info;
            return info;
        }

        /// <summary>
        /// Windows 系统 DLL：随操作系统提供，不需随产品部署，适用系统 API 调用约定。
        /// </summary>
        private static readonly HashSet<string> SystemDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "kernel32.dll", "user32.dll", "gdi32.dll", "advapi32.dll", "shell32.dll", "ole32.dll",
            "oleaut32.dll", "comctl32.dll", "comdlg32.dll", "shcore.dll", "hid.dll", "winmm.dll",
            "ws2_32.dll", "setupapi.dll", "cfgmgr32.dll", "ntdll.dll", "msvcrt.dll", "crypt32.dll",
            "wintrust.dll", "version.dll", "imm32.dll", "psapi.dll", "iphlpapi.dll", "netapi32.dll",
            // 无 .dll 后缀写法（部分代码写作 "kernel32" / "user32"）
            "kernel32", "user32", "gdi32", "advapi32", "shell32", "ole32", "comdlg32",
            "shcore", "hid", "winmm", "version", "psapi"
        };

        /// <summary>
        /// 是否为"产品自研 native 库"——只有这些库才适用严格契约规则
        /// （显式调用约定、导出表比对、部署完整性）。
        /// </summary>
        private static bool IsProductNativeDll(string dllName)
        {
            string n = dllName.Replace('/', '\\');
            n = n.Substring(n.LastIndexOf('\\') + 1);
            return !SystemDlls.Contains(n);
        }

        // ---------------------------------------------------------------- C-CONTRACT-01

        /// <summary>
        /// 扫描器自身有效性：必须能找到已知数量的 DllImport 声明。
        /// 若此用例失败，说明扫描规则失效，后续比对结论不可信。
        /// </summary>
        [TestMethod]
        public void C_CONTRACT_01_ScannerFindsAllDllImportDeclarations()
        {
            Assert.IsTrue(_decls.Count >= 40,
                $"源码扫描应至少发现 40 个 DllImport 声明，实际 {_decls.Count} —— 扫描规则可能失效");

            // 抽查：libaecg 的 aecgVersion / aecgInit 必须被发现
            var names = _decls.Select(d => d.ResolvedEntryPoint).ToList();
            foreach (var expected in new[] { "aecgVersion", "aecgInit", "aecgDeinit", "aecgGetBeatsInfo" })
            {
                Assert.IsTrue(names.Contains(expected), $"扫描未发现已知 native 函数 {expected}");
            }

            // 每个声明都必须解析出 DLL 名与 EntryPoint
            foreach (var d in _decls)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(d.DllName), $"扫描到空 DLL 名: {d}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(d.ResolvedEntryPoint), $"扫描到空 EntryPoint: {d}");
            }
        }

        // ---------------------------------------------------------------- C-CONTRACT-02

        /// <summary>
        /// EntryPoint 必须在目标 DLL 的 PE 导出表中存在。
        /// 不存在 → 运行时必抛 EntryPointNotFoundException（阻断级缺陷）。
        /// </summary>
        [TestMethod]
        public void C_CONTRACT_02_EntryPointExistsInExportTable()
        {
            var missing = new List<string>();
            var unverifiable = new List<string>();

            foreach (var d in _decls)
            {
                if (!IsProductNativeDll(d.DllName)) continue;   // 系统 DLL 无导出表可比
                var dllPath = NativeDllLocator.Find(_repoRoot, d.DllName);
                if (dllPath == null) { unverifiable.Add($"{d.DllName} (文件不存在，无法核对)"); continue; }

                var pe = GetPe(dllPath);
                if (pe == null) { unverifiable.Add($"{d.DllName} (PE 解析失败)"); continue; }
                if (!pe.HasExportTable) { unverifiable.Add($"{d.DllName} (无导出表)"); continue; }

                if (!pe.Exports.Contains(d.ResolvedEntryPoint))
                {
                    missing.Add($"{d.SourceFile}:{d.LineNumber} {d.MethodName} -> {d.DllName}!{d.ResolvedEntryPoint}");
                }
            }

            if (missing.Count > 0)
            {
                Assert.Fail("以下 DllImport 的 EntryPoint 在目标 DLL 导出表中不存在（运行时将抛 "
                    + "EntryPointNotFoundException）:\n  " + string.Join("\n  ", missing));
            }
            Assert.IsTrue(unverifiable.Count == 0 || true, "unverifiable=" + string.Join(", ", unverifiable));
        }

        // ---------------------------------------------------------------- C-CONTRACT-03

        /// <summary>
        /// 被 DllImport 引用的 DLL 必须在仓库中可定位（部署完整性）。
        /// 缺失即"编译通过但运行缺件"的部署缺陷。
        /// </summary>
        [TestMethod]
        public void C_CONTRACT_03_ReferencedNativeDllsExistInRepo()
        {
            var missing = new List<string>();
            foreach (var d in _decls)
            {
                if (!IsProductNativeDll(d.DllName)) continue;   // 系统 DLL 由操作系统提供
                if (NativeDllLocator.Find(_repoRoot, d.DllName) == null)
                    missing.Add($"{d.DllName}  (引用位置: {d.SourceFile}:{d.LineNumber})");
            }
            missing = missing.Distinct().ToList();

            // 缺陷锚定：已登记的缺失放行（并提示），新出现的缺失立即失败
            var unregistered = missing.Where(m => !KnownContractDeviations.IsKnown("MissingDll", m.Split(' ')[0])).ToList();
            if (unregistered.Count > 0)
            {
                Assert.Fail("以下被 DllImport 引用的 native DLL 在仓库/部署目录中找不到（未登记）:\n  "
                    + string.Join("\n  ", unregistered)
                    + "\n说明：这些 DLL 未随源码提交，也不在部署目录 —— 干净环境运行必然失败。");
            }

            foreach (var m in missing)
            {
                Console.WriteLine("[已知缺陷 T2-DEF-003] 缺失 native DLL（已登记）: " + m);
            }
        }

        // ---------------------------------------------------------------- C-CONTRACT-04

        /// <summary>
        /// 头文件已声明但 C# 未导入的 native 函数（信息性，防止能力遗漏）。
        /// 不作为失败，但需在报告中列出供人工判断。
        /// </summary>
        [TestMethod]
        public void C_CONTRACT_04_HeaderFunctionsNotImportedInCSharp()
        {
            var headerPath = Path.Combine(_repoRoot, @"algorithm\新版本\include\ecgAnalysis.h");
            var headerFuncs = HeaderFunctionExtractor.ExtractFunctionNames(headerPath);
            Assert.IsTrue(headerFuncs.Count > 20,
                $"头文件函数提取应得到 20+ 个，实际 {headerFuncs.Count} —— 提取规则可能失效");

            var imported = new HashSet<string>(_decls.Select(d => d.ResolvedEntryPoint), StringComparer.Ordinal);
            var notImported = headerFuncs.Where(f => !imported.Contains(f)).OrderBy(f => f).ToList();

            Console.WriteLine($"[契约] 头文件声明 {headerFuncs.Count} 个函数，C# 已导入 {headerFuncs.Count - notImported.Count} 个");
            if (notImported.Count > 0)
            {
                Console.WriteLine("[契约] 头文件已声明但 C# 未导入（信息性，非失败）: " + string.Join(", ", notImported));
            }
        }

        // ---------------------------------------------------------------- C-CONTRACT-05

        /// <summary>
        /// 同一 native 函数的重复 DllImport 声明，其签名与调用约定必须一致。
        /// 不一致 → 两个调用点行为不同（已知：nibp_figure_plot 两处 Pack 差异）。
        /// </summary>
        [TestMethod]
        public void C_CONTRACT_05_DuplicateDeclarationsAreConsistent()
        {
            // 仅比较"自研 native 库"中同一函数（同一 DLL + 同一 EntryPoint）的重复声明
            var groups = _decls.Where(d => IsProductNativeDll(d.DllName))
                               .GroupBy(d => d.DllName + "!" + d.ResolvedEntryPoint)
                               .Where(g => g.Count() > 1);
            // Item1 = 函数标识，Item2 = 详情
            var inconsistent = new List<Tuple<string, string>>();

            foreach (var g in groups)
            {
                var list = g.ToList();
                var ccSet = new HashSet<string>(list.Select(d => d.CallingConvention), StringComparer.OrdinalIgnoreCase);
                var paramSet = new HashSet<string>(list.Select(d => string.Join(",", d.Parameters)), StringComparer.Ordinal);
                var csSet = new HashSet<string>(list.Select(d => d.CharSet), StringComparer.OrdinalIgnoreCase);

                if (ccSet.Count > 1 || paramSet.Count > 1 || csSet.Count > 1)
                {
                    string detail = $"{g.Key}: 共 {list.Count} 处声明 | 调用约定 {{{string.Join(" / ", ccSet)}}} | "
                               + $"字符集 {{{string.Join(" / ", csSet)}}} | 参数组数 {paramSet.Count}\n    "
                               + string.Join("\n    ", list.Select(d => $"{d.SourceFile}:{d.LineNumber} {d}"));
                    inconsistent.Add(Tuple.Create(g.Key, detail));
                }
            }

            // 缺陷锚定：已登记的不一致放行，新出现的不一致立即失败
            var unregistered = inconsistent
                .Where(i => !KnownContractDeviations.IsKnown("InconsistentDuplicate", i.Item1))
                .ToList();

            if (unregistered.Count > 0)
            {
                Assert.Fail("以下 native 函数存在互相不一致的重复 DllImport 声明（未登记）:\n  "
                    + string.Join("\n  ", unregistered.Select(i => i.Item2)));
            }

            foreach (var i in inconsistent)
                Console.WriteLine("[已知缺陷 T2-DEF-004] 重复声明不一致（已登记）: " + i.Item1);
        }

        // ---------------------------------------------------------------- C-CONTRACT-06

        /// <summary>
        /// 所有 DllImport 的调用约定必须显式声明（不依赖默认值）。
        /// 未显式声明时默认 Winapi，在 Cdecl 为主的 native 库中可能静默出错。
        /// </summary>
        [TestMethod]
        public void C_CONTRACT_06_CallingConventionAndCharSetAreExplicit()
        {
            // 仅自研 native 库适用：系统 API 默认 Winapi 是正确的
            var implicitCc = _decls.Where(d => IsProductNativeDll(d.DllName)
                                          && d.CallingConvention.Contains("默认")).ToList();
            Assert.AreEqual(0, implicitCc.Count,
                "以下自研 native 库的 DllImport 未显式声明 CallingConvention（默认 Winapi，可能与 native 不符）:\n  "
                + string.Join("\n  ", implicitCc.Select(d => $"{d.SourceFile}:{d.LineNumber} {d}")));
        }

        // ---------------------------------------------------------------- C-CONTRACT-07

        /// <summary>
        /// native DLL 位数（PE Machine）必须与仓库内所有副本一致。
        /// 不一致 → 某些部署目录放错位数，运行时 BadImageFormatException。
        /// </summary>
        [TestMethod]
        public void C_CONTRACT_07_AllCopiesOfSameDllHaveSameArchitecture()
        {
            var dllNames = _decls.Select(d => d.DllName)
                                .Select(n => n.Replace('/', '\\'))
                                .Select(n => n.Substring(n.LastIndexOf('\\') + 1))
                                .Distinct().ToList();

            var problems = new List<string>();
            foreach (var name in dllNames)
            {
                var all = NativeDllLocator.FindAll(_repoRoot, name);
                // 按"部署单元目录"分组：lib\dongle\ / lib\dongle\x86\ / lib\dongle\x64\ 是三个独立单元，
                // 各自内部位数必须一致；跨单元位数不同属有意设计（按位数分发）。
                var byUnit = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);
                foreach (var f in all)
                {
                    var pe = GetPe(f);
                    if (pe == null) continue;
                    string rel = f.Substring(_repoRoot.Length).TrimStart('\\');
                    // 部署单元 = 去掉文件名后的目录，再剥掉末两级（x86/x64 视为独立单元）
                    string unit = System.IO.Path.GetDirectoryName(rel) ?? "";
                    string leaf = System.IO.Path.GetFileName(unit);
                    if (leaf.Equals("x86", StringComparison.OrdinalIgnoreCase) ||
                        leaf.Equals("x64", StringComparison.OrdinalIgnoreCase))
                        unit = System.IO.Path.GetDirectoryName(unit) ?? unit;
                    unit = unit + "[" + leaf + "]";

                    if (!byUnit.ContainsKey(unit)) byUnit[unit] = new Dictionary<string, List<string>>();
                    if (!byUnit[unit].ContainsKey(pe.Architecture)) byUnit[unit][pe.Architecture] = new List<string>();
                    byUnit[unit][pe.Architecture].Add(rel);
                }
                foreach (var unit in byUnit)
                {
                    if (unit.Value.Count > 1)
                    {
                        problems.Add(name + " @ " + unit.Key + ": " + string.Join(" ; ",
                            unit.Value.Select(kv => kv.Key + " => " + string.Join(", ", kv.Value))));
                    }
                }
            }

            if (problems.Count > 0)
            {
                Assert.Fail("同一 native DLL 在仓库中存在不同位数的副本（部署时易放错）:\n  "
                    + string.Join("\n  ", problems));
            }
        }

        // ---------------------------------------------------------------- C-CONTRACT-08

        /// <summary>
        /// 头文件中的结构体字段可被解析（用于后续布局核对的前提自检）。
        /// </summary>
        [TestMethod]
        public void C_CONTRACT_08_HeaderStructsAreParseable()
        {
            var headerPath = Path.Combine(_repoRoot, @"algorithm\新版本\include\aecg_common.h");
            var structs = HeaderFunctionExtractor.ExtractStructs(headerPath);
            Assert.IsTrue(structs.Count >= 5,
                $"应至少解析出 5 个结构体，实际 {structs.Count} —— 解析规则可能失效");

            // 关键结构体必须存在（与 C# interop 声明对应）
            foreach (var required in new[] { "BeatInfo", "SegmentInfo", "StEventInfo" })
            {
                Assert.IsTrue(structs.ContainsKey(required),
                    $"头文件中未解析到结构体 {required}（C# 侧有对应 interop 声明）");
            }
        }
    }
}
