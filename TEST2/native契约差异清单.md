# Native 契约差异清单 — TEST2 轮

> 产出方式：自动化机械比对（`TEST2/UnitTests/Interop/`）
> 方法：源码扫描全部 `[DllImport]` 声明 → 解析目标 DLL 的 PE 导出表 → 与 `algorithm/新版本/include/*.h` 交叉比对
> 复核日期：2026-11-19

---

## 一、比对范围

| 项 | 数量 |
|---|---|
| 扫描到的 DllImport 声明 | 90+（全部 10 个产品源码目录） |
| 其中指向自研 native 库 | 47（libaecg / libnibp / TransformZQ / testWave / dongle） |
| 其中为 Windows 系统 API | 43+（kernel32 / user32 / gdi32 / shcore / hid …，不适用导出表比对） |
| 解析出 PE 导出表的 DLL | libaecg.dll、libnibp_dynamic_d_d.dll、dongle 系列 |

---

## 二、差异汇总

### D-01 — 缺失的 native DLL（部署缺件）

| DLL | 引用位置 | 说明 | 缺陷单 |
|---|---|---|---|
| `TransformZQ-x32.dll` | `EcgAnalyseStart/sundries/DataFileHandler.cs:21` | ZQ Holter 数据格式转换（x86），仓库与部署目录均无 | T2-DEF-003 |
| `TransformZQ-x64.dll` | `EcgAnalyseStart/sundries/DataFileHandler.cs:24` | 同上（x64，按 `IntPtr.Size==8` 选择） | T2-DEF-003 |
| `testWave.dll` | `EcgAnalyseStart/sundries/DataHandler.cs:12,15,18,21,25,28,31,35`（8 处） | 设备波形读取（`initDll`/`getOneLeadSize`/`readOneLeadData`/`readWaveData`/`uninitDll`），仓库与部署目录均无，且无头文件 | T2-DEF-005 |

**影响**：三者均属"编译通过但运行缺件"。`testWave.dll` 承载 8 导联波形读取，缺失将无法读取设备波形；
`TransformZQ-*.dll` 承载 ZQ→MIT-BIH 格式转换，缺失将无法导入 ZQ 原始数据。
**待澄清**：这两个 DLL 是否随安装包分发？若是外部提供，需在部署清单中固化并加启动期校验。

### D-02 — 同一 native 函数的重复声明不一致

| 函数 | 声明位置与形态 | 问题 | 缺陷单 |
|---|---|---|---|
| `readOneLeadData` | `:18` `(int lead, IntPtr waveDataMap, ref int total)`<br>`:21` `(int lead, short[] waveData, ref int total)` | 同一导出函数两种不兼容原型，必有其一与 native 原型不符；`:21` 的 `[MarshalAs(LPArray, SizeParamIndex=1)]` 指向自身参数（索引 0=lead,1=waveData），SizeParamIndex 语义可疑 | T2-DEF-004 |
| `readWaveData` | `:25` `(int, int, IntPtr, ref int)`<br>`:28` `(int, int, byte[], ref int)`<br>`:31` `(int, int, ref WaveDataMap[], ref int)` | 三种不兼容原型；`size`/`total` 单位（字节/元素/点数）在无头文件时无法确定，三种形态的缓冲区容量算法必然不一致 | T2-DEF-004 |

### D-03 — 一致性核对通过项（无差异）

| 核对项 | 结果 |
|---|---|
| 所有自研 native 库的 `EntryPoint` 是否存在于 PE 导出表 | ✅ 全部存在（libaecg 的 `aecgVersion`/`aecgInit`/`aecgDeinit`/`aecgLoadData`/`aecgReanalyze`/`aecgGetBeatsInfo`/`aecgCalculateHRV` 等均已验证） |
| 自研 native 库是否显式声明 `CallingConvention` | ✅ 全部显式（Cdecl / StdCall） |
| 同一 DLL 在各部署单元内的位数是否一致 | ✅ 一致（libaecg / libnibp / dongle 均为 x64 或按 x86/x64 分目录分发） |
| `aecg_common.h` 结构体是否可解析 | ✅ 可解析，已识别 `BeatInfo`/`SegmentInfo`/`StEventInfo` 等 |

---

## 三、头文件能力差异（信息性）

`ecgAnalysis.h` 声明但 C# 侧未导入的函数（由 `C_CONTRACT_04` 自动列出）：
`aecgReanalyzePACSegment`、`aecgAnalyzeTWA`、`aecgAnalyzeQTC`、`aecgGetQRSInfo`、
`aecgGetQtcInfo`、`aecgAnalyzeSleepApnea`、`setEcgSublibDebugFlag`。

**说明**：这些是 native 已具备但产品未使用的能力（如 TWA、QTc、睡眠呼吸暂停分析）。
需与算法/产品确认：① 是刻意不启用，还是遗漏接入？② 若启用，C# 侧结构体布局需按头文件核对。

---

## 四、无法核对项（需 native 侧输入）

| 项 | 缺失输入 | 阻塞的审查项 |
|---|---|---|
| NIBP 全套契约 | `F:\temp\figurePlot\figurePlot.h/.c`、`fitCurveGet.h/.c`、`pulseWaveGet.h/.c` | T2-R-005 类缓冲区越界问题的定案 |
| `testWave.dll` 原型 | 该 DLL 及其头文件 | D-02 全部 |
| `TransformZQ-*.dll` 返回码与编码 | 该 DLL 及其头文件 | 格式转换链路的正确性验证 |
| `ecg_errors.h` | 该文件 | 返回码 `ERR_SUCCESS`/`AECG_TRUNCATED` 具体数值 |
| `out IntPtr` 输出的所有权 | 头文件未声明任何释放 API | 9 处 `T**` 输出的内存泄漏定案 |

---

## 五、维护方式

本清单由 `TEST2/UnitTests/Interop/InteropContractTests.cs` 自动校验：

| 用例 | 作用 | 新增未登记差异时 |
|---|---|---|
| `C_CONTRACT_01` | 扫描器自检（确保扫描规则有效） | 失败 → 扫描规则失效 |
| `C_CONTRACT_02` | EntryPoint ↔ 导出表 | **立即失败** |
| `C_CONTRACT_03` | 引用 DLL 的部署完整性 | **立即失败**（已登记项放行） |
| `C_CONTRACT_04` | 头文件能力差异 | 仅输出信息 |
| `C_CONTRACT_05` | 重复声明一致性 | **立即失败**（已登记项放行） |
| `C_CONTRACT_06` | 调用约定/字符集显式性 | **立即失败** |
| `C_CONTRACT_07` | 同名 DLL 位数一致性 | **立即失败** |
| `C_CONTRACT_08` | 头文件结构体可解析性（前提自检） | 失败 → 提取规则失效 |

**缺陷锚定**：已登记差异见 `KnownContractDeviations.cs`，修复后应从该清单移除，对应用例随即转为严格断言。
