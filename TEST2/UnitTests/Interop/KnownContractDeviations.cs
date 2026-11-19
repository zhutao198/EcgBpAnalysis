using System;
using System.Collections.Generic;

namespace EcgBpAnalysis.T2.Tests.Interop
{
    /// <summary>
    /// 已知契约差异登记簿。
    ///
    /// 设计意图（缺陷锚定 / defect anchoring）：
    ///   契约比对用例只允许"已登记"的差异通过；一旦出现**新**差异立即失败，
    ///   从而做到：① CI 保持绿（已知缺陷不阻断）；② 新增未登记缺陷必然暴露。
    ///   缺陷修复后应从本清单移除对应条目，届时用例转为严格断言。
    /// </summary>
    public static class KnownContractDeviations
    {
        public class Deviation
        {
            public string Kind;        // 类别
            public string Subject;     // 主体（DLL 名或函数名）
            public string Detail;      // 具体描述
            public string DefectId;    // 关联缺陷单
            public string FixPlan;     // 修复方案
        }

        /// <summary>
        /// 缺失的 native DLL（部署缺件）。
        /// </summary>
        public static readonly List<Deviation> MissingDlls = new List<Deviation>
        {
            new Deviation
            {
                Kind = "MissingDll",
                Subject = "TransformZQ-x32.dll",
                Detail = "被 EcgAnalyseStart\\sundries\\DataFileHandler.cs:21 引用（ZQ Holter 数据格式转换），"
                       + "仓库与部署目录中均不存在；测试Wave 为 x86 版本。",
                DefectId = "T2-DEF-003",
                FixPlan = "确认该 DLL 是否随安装包分发；若为外部提供，需在部署清单中固化并加校验；"
                        + "否则仓库补入或改用仓库内已有实现。"
            },
            new Deviation
            {
                Kind = "MissingDll",
                Subject = "TransformZQ-x64.dll",
                Detail = "被 EcgAnalyseStart\\sundries\\DataFileHandler.cs:24 引用（按 IntPtr.Size==8 选择 x64 版本），"
                       + "仓库与部署目录中均不存在。",
                DefectId = "T2-DEF-003",
                FixPlan = "同上。"
            },
            new Deviation
            {
                Kind = "MissingDll",
                Subject = "testWave.dll",
                Detail = "被 EcgAnalyseStart\\sundries\\DataHandler.cs:12,15,18,21,25,28,31,35 共 8 处引用"
                       + "（initDll/getOneLeadSize/readOneLeadData/readWaveData/uninitDll，用于从设备驱动读取 8 导联波形），"
                       + "仓库与部署目录中均不存在；该 DLL 无对应头文件，原型完全无法核对。",
                DefectId = "T2-DEF-005",
                FixPlan = "确认该 DLL 是否随安装包分发（波形读取依赖它，缺失将无法读取设备波形）；"
                        + "若为外部提供，需固化到部署清单并提供头文件以核对原型。"
            },
        };

        /// <summary>
        /// 同一 native 函数的重复 DllImport 声明不一致。
        /// </summary>
        public static readonly List<Deviation> InconsistentDuplicates = new List<Deviation>
        {
            new Deviation
            {
                Kind = "InconsistentDuplicate",
                Subject = "testWave.dll!readOneLeadData",
                Detail = "DataHandler.cs:18 声明为 (int lead, IntPtr waveDataMap, ref int total)，"
                       + "DataHandler.cs:21 声明为 (int lead, short[] waveData, ref int total)，"
                       + "同一 native 导出函数被声明为两种不兼容的原型，必有一个与 native 原型不符；"
                       + "且 :21 的 [MarshalAs(LPArray, SizeParamIndex=1)] 指向自身参数（参数索引 0=lead,1=waveData），"
                       + "SizeParamIndex 语义可疑。",
                DefectId = "T2-DEF-004",
                FixPlan = "取得 testWave.dll 的导出原型与头文件，统一为唯一正确签名；"
                        + "若 native 确实只支持指针形态，删除数组重载并修正 SizeParamIndex。"
            },
            new Deviation
            {
                Kind = "InconsistentDuplicate",
                Subject = "testWave.dll!readWaveData",
                Detail = "DataHandler.cs:25 声明为 (int index, int size, IntPtr waveDataMap, ref int total)，"
                       + ":28 声明为 (int index, int size, byte[] waveDataMap, ref int total)，"
                       + ":31 声明为 (int index, int size, ref WaveDataMap[] waveDataMap, ref int total)，"
                       + "同一 native 导出函数被声明为三种互不兼容的原型，必有与 native 原型不符者；"
                       + "且 size 参数的单位（字节/元素/点数）在头文件缺失时无法确定，三种形态的缓冲区容量算法必然不一致"
                       + "（:28 按 pointCount*2*8 字节分配，:25 按同样规则但语义不同）。",
                DefectId = "T2-DEF-004",
                FixPlan = "取得 testWave.dll 原型与头文件，统一为唯一正确签名并明确 size/total 单位；"
                        + "删除冗余重载，只保留指针形态 + 明确的 MarshalAs 数组形态。"
            },
        };

        /// <summary>
        /// 判断某个差异是否已登记。
        /// </summary>
        public static bool IsKnown(string kind, string subject)
        {
            var list = kind == "MissingDll" ? MissingDlls : InconsistentDuplicates;
            foreach (var d in list)
                if (d.Kind == kind && string.Equals(d.Subject, subject, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
