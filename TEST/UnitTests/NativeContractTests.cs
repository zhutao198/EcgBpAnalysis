using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EcgBpAnalysis.UnitTests
{
    /// <summary>
    /// Native 接口契约的符号级冒烟测试（对应 P1-2 / P2-2）。
    ///
    /// 说明：libaecg 算法函数需要完整病例数据才有意义输出，且多数输出缓冲区契约头文件未定义，
    ///       故本组仅验证"DLL 可加载 + 位数正确 + 关键导出符号存在且可调用"这一层契约。
    ///       不调用会写内存/需病例数据的函数，避免越界契约未明时触发 native 崩溃。
    ///       完整数据回归见 P2-1（需 golden data）。
    /// </summary>
    [TestClass]
    public class NativeContractTests
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr VersionFn();

        /// <summary>
        /// libaecg.dll 依赖 MinGW 运行时(libgcc_s_sjlj-1/libgomp/libstdc++-6/libwinpthread-1)，
        /// 这些 DLL 与 libaecg.dll 同在部署目录。用绝对路径 LoadLibrary 时 Windows 不会自动
        /// 搜索该目录的依赖，需 SetDllDirectory 显式加入搜索路径（与产品实际加载方式一致）。
        /// </summary>
        private static void SetNativeDllSearchPath(string dllPath)
        {
            var dir = Path.GetDirectoryName(dllPath);
            if (!string.IsNullOrEmpty(dir)) SetDllDirectory(dir);
        }

        /// <summary>
        /// 从若干候选路径定位 libaecg.dll。
        /// </summary>
        private static string FindLibaecg(out string searched)
        {
            var root = FindRepoRoot();
            var candidates = new[]
            {
                // 部署目录优先：含 MinGW 运行时依赖，LoadLibrary 才能成功
                Path.Combine(root, @"EcgAnalyseStart\bin\Debug\lib\unmanaged\libaecg.dll"),
                Path.Combine(root, @"algorithm\新版本\libaecg.dll"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libaecg.dll"),
            };
            foreach (var c in candidates)
                if (File.Exists(c)) { searched = string.Join("; ", candidates); return c; }
            searched = string.Join("; ", candidates);
            return null;
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "EcgBpAnalysis.sln"))) return dir.FullName;
                dir = dir.Parent;
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 进程必须是 x64：native DLL 经 PE 头验证为 x64（Machine=0x8664），
        /// x86 进程加载会抛 BadImageFormatException。
        /// </summary>
        [TestMethod]
        public void Process_IsX64_AsRequiredByNativeDlls()
        {
            Assert.AreEqual(8, IntPtr.Size,
                "libaecg/libnibp 均为 x64，测试进程必须以 x64 运行（vstest /Platform:x64）");
        }

        /// <summary>
        /// libaecg.dll 可被加载，且 aecgVersion 导出符号存在、可调用、返回非空版本串。
        /// 这是"C# DllImport 声明 ↔ native 导出表"契约的最小验证。
        /// </summary>
        [TestMethod]
        public void Libaecg_LoadsAndExportsAecgVersion()
        {
            string searched;
            var dll = FindLibaecg(out searched);
            Assert.IsNotNull(dll, "未找到 libaecg.dll，搜索路径: " + searched);

            SetNativeDllSearchPath(dll);
            IntPtr h = LoadLibrary(dll);
            Assert.IsFalse(h == IntPtr.Zero,
                $"LoadLibrary 失败，Win32 错误码 {Marshal.GetLastWin32Error()}，DLL={dll}");

            try
            {
                IntPtr proc = GetProcAddress(h, "aecgVersion");
                Assert.IsFalse(proc == IntPtr.Zero,
                    "导出符号 aecgVersion 不存在——DllImport EntryPoint 与 native 导出表不一致");

                var fn = (VersionFn)Marshal.GetDelegateForFunctionPointer(proc, typeof(VersionFn));
                IntPtr pVer = fn();
                Assert.IsFalse(pVer == IntPtr.Zero, "aecgVersion() 返回空指针");
                string version = Marshal.PtrToStringAnsi(pVer);
                Assert.IsFalse(string.IsNullOrWhiteSpace(version), "aecgVersion() 返回空版本串");
                StringAssert.Contains(version, ".", $"版本串应含点号分隔符，实际='{version}'");
            }
            finally
            {
                FreeLibrary(h);
            }
        }

        /// <summary>
        /// PE 头 Machine 字段必须为 AMD64（0x8664），防止误部署 x86 版导致 BadImageFormatException。
        /// 同时验证核心导出符号集合存在（aecgInit/aecgDeinit/aecgLoadData 等）。
        /// </summary>
        [TestMethod]
        public void Libaecg_PeHeader_IsAmd64_AndCoreExportsExist()
        {
            string searched;
            var dll = FindLibaecg(out searched);
            Assert.IsNotNull(dll, "未找到 libaecg.dll，搜索路径: " + searched);

            // 读取 PE Machine 字段
            ushort machine;
            using (var fs = File.OpenRead(dll))
            using (var br = new BinaryReader(fs))
            {
                Assert.AreEqual((ushort)0x5A4D, br.ReadUInt16(), "非 PE 文件（缺 MZ 头）");
                fs.Seek(0x3C, SeekOrigin.Begin);
                uint peOff = br.ReadUInt32();
                fs.Seek(peOff, SeekOrigin.Begin);
                Assert.AreEqual((uint)0x00004550, br.ReadUInt32(), "非 PE 文件（缺 PE 签名）");
                machine = br.ReadUInt16();
            }
            Assert.AreEqual((ushort)0x8664, machine,
                "libaecg.dll 必须为 AMD64(x64)，实际 Machine=0x" + machine.ToString("X4"));

            // 验证核心导出符号
            SetNativeDllSearchPath(dll);
            IntPtr h = LoadLibrary(dll);
            Assert.IsFalse(h == IntPtr.Zero, $"LoadLibrary 失败，Win32 错误码 {Marshal.GetLastWin32Error()}");
            try
            {
                string[] coreExports = { "aecgVersion", "aecgInit", "aecgDeinit", "aecgLoadData",
                                         "aecgReanalyze", "aecgGetBeatsInfo", "aecgCalculateHRV" };
                foreach (var name in coreExports)
                {
                    IntPtr proc = GetProcAddress(h, name);
                    Assert.IsFalse(proc == IntPtr.Zero, $"核心导出符号 {name} 不存在");
                }
            }
            finally { FreeLibrary(h); }
        }
    }
}
