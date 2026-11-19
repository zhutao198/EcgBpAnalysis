# Native 接口契约文档（P1-2 交付物）

> 项目：EcgBpAnalysis（Holter 动态心电/动态血压分析系统）
> 版本：v1.0　更新日期：2026-11-19
> 用途：固化 C# ↔ native DLL 的 P/Invoke 契约，作为 R001/R002/R011/R012/R035/R040 等审查项的验证依据。
> 证据来源：`algorithm/新版本/include/aecg_common.h`、`ecgAnalysis.h`、`ZonCare.DogDriver.h`（头文件）；
> `aEcgHandler.cs`/`BpPulseWaveDataHandler.cs`/`DataHandler.cs`（C# 声明）；PE 头检查（位数）。

---

## 一、DLL 清单与位数（PE 头已验证）

| DLL | 用途 | 位数 | 验证方式 | C# 引用 |
|---|---|---|---|---|
| `algorithm/新版本/libaecg.dll` | ECG 核心算法 | **x64** | PE Machine=0x8664 | `aEcgHandler.cs:16` `lib\unmanaged\libaecg.dll` |
| `algorithm/新版本/libaecg_d.dll` | ECG 调试版 | **x64** | PE Machine=0x8664 | （调试用） |
| `EcgAnalyseStart/bin/Debug/lib/unmanaged/libaecg.dll` | 部署副本 | **x64** | PE Machine=0x8664 | 同上 |
| `.../lib/unmanaged/libnibp_dynamic_d_d.dll` | 血压脉搏波算法 | **x64** | PE Machine=0x8664 | `BpPulseWaveDataHandler.cs:10` |
| `.../lib/unmanaged/libnibp_dynamic_d_d_1012.dll` | 血压算法(1012版) | **x64** | PE Machine=0x8664 | （变体） |
| `TransformZQ-x32.dll` / `-x64.dll` | ZQ→MIT-BIH 格式转换 | x86/x64 | 文件名 + `IntPtr.Size==8` 选择（`DataFileHandler.cs:281-285`） | 按位数选 |
| `lib/dongle/{x86,x64}/ZonCare.DogDriver.dll` | 加密狗 | x86/x64 | 部署子目录区分 | `ZoncareDog.cs:14` |

> **关键结论（R012 部分解答）**：`libaecg.dll` 与 `libnibp_dynamic_d_d.dll` 均为 **x64**，
> 因此主程序/分析进程必须以 x64 运行（`Prefer32Bit=false`）。若部署为 x86 将 `BadImageFormatException`。
> C++ 工程产物名 `LibNibp.dll` 与调用名 `libnibp_dynamic_d_d.dll` 仍不一致——需确认构建输出重命名配置。

---

## 二、libaecg 契约（头文件 `ecgAnalysis.h` 可验证）

全部 `CallingConvention.Cdecl`，共 40 个 DllImport（`aEcgHandler.cs:196-320`）。

### 2.1 输出缓冲区契约（高风险区）

| 函数 | native 元素类型 | 内存所有权 | 长度契约 | 风险 |
|---|---|---|---|---|
| `aecgGetSegmentsWave` | `short *wave`, `int *status` | **调用者分配**（AllocHGlobal/FreeHGlobal） | **元素数**；`waveNum` 为 **in/out**：入=容量，出=实际点数（H:868） | **R035**：C# 按 native 改写后的 targetLen 回读，未校验 ≤ 分配容量 → 越界读 |
| `aecgGetBeatsInfo` | `BeatInfo **` | **头文件未定义**（疑 native 静态缓冲） | 元素数 `beatsNum` | **R040**：C# 9 处 out IntPtr 均不释放，疑泄漏 |
| `aecgGetStEventInfo` / `aecgGetStxInfo` / `aecgGetAfEvent` / `aecgGetPacemakerEvaluationInfo` / `aecgGetSegmentUnanalyzable` | `T **` | 头文件未定义 | 元素数 | R040（同上） |
| `aecgGetHrvPsd` | `double **dataX/dataY` | 头文件未定义 | double 元素数 `psdNum` | H:745 "262145/组" 仅描述性；`fftNum` 注释与 `psdNum` 返回值命名不一致 |
| `aecgGetLeadQuality` | `int *score` | **调用者分配** | 元素数=导联个数 | 契约清晰 |
| `aecgReclusterBeatsByLead` | `short *beatSubType` | **头文件明确调用者申请/释放**（H:367） | 元素数=`beatPosNum` | **R040**：C# 按 int(4B) 分配却按 short(2B) 读 → 容量算错 2 倍 |
| `aecgGetPaceSimilarity` | `double *similarity` | 调用者分配 | 元素数=`posNum` | 契约清晰 |
| `aecgVersion` | `const char *` | native 持有 | NUL 结尾字符串 | 只读，冒烟测试安全 |
| `aecgInit(ReportProgress)` | 返回 `void *` 句柄 | native | — | 回调有 GCHandle 固定（:350-357） |
| `aecgDeinit(ref IntPtr)` | — | native 释放 | — | — |

### 2.2 返回值码契约（H:68-77）

`ecg_errors.h` 定义 `ERR_SUCCESS` / `AECG_TRUNCATED` 等，**该文件不在工作区**，无法核对具体数值。
C# 侧多数 `aecgXxx` 返回 int 但调用方普遍不校验返回值 → 建议统一检查 `ERR_SUCCESS`。

### 2.3 头文件声明但 C# 未导入

`aecgReanalyzePACSegment`(H:263)、`aecgAnalyzeTWA`(H:286)、`aecgAnalyzeQTC`(H:309)、`aecgGetQRSInfo`(H:574)、
`aecgGetQtcInfo`(H:833)、`aecgAnalyzeSleepApnea`(H:1093)、`setEcgSublibDebugFlag`(H:995)。

---

## 三、NIBP 契约（头文件缺失，仅 C# 侧推断）

**LibNibp 源码不在工作区**（`LibNibp.vcxproj:150-157` 指向 `F:\temp\figurePlot\...`）。
`nibp_figure_plot` 的原型、结构体 Pack/字段顺序、长度单位、所有权、释放函数**均无法由头文件验证**。

| 项 | 实现A `BpPulseWaveDataHandler.cs` | 实现B `DataHandler.cs` |
|---|---|---|
| DLL | `lib\unmanaged\libnibp_dynamic_d_d.dll` | `libnibp_dynamic_d_d.dll` |
| 结构体布局 | 默认 Sequential，**无 Pack** | **Pack=1** |
| 风险 | x64 下两布局可能不同（**R011**）；回读 `outputWave.len`/`stepLen` 未校验容量（**R001**） | 同左 |

C# 推断：`nibpInData.preWave/stage` 由调用者按元素数(int)分配；`nibpOutExtractWave.dc/ac/stepDc` double 由调用者分配，
回读按 `len`/`stepLen`（**未校验 ≤ 容量 → R001 越界**）。

> **这是 R001（阻断项）的根源**：必须先取得 `F:\temp\figurePlot` 下的头文件/源码，明确 `len` 单位与缓冲区上限，
> 才能确定修复方案。当前 P/Invoke 测试只能做"能否加载/导出符号是否存在"，不能验证数据正确性。

---

## 四、加密狗契约（`ZonCare.DogDriver.h` 可验证）

| 函数 | native | C# | 证据 |
|---|---|---|---|
| `usb_startCheckStatus()` | `void __stdcall` | StdCall | H:17 / ZoncareDog.cs:19 |
| `usb_stopCheckStaus()` | `void __stdcall`（拼写 Staus） | StdCall | H:19 / :25 |
| `usb_registerCallback(fn)` | `typedef void(__stdcall *)(int status)` | delegate | H:13,15 / :33-42 |

回调 delegate 未见 GCHandle 固定（`ZoncareDog.cs:86-99`），native 若在调用后异步回调存在悬垂委托风险（头文件未定义生命周期）。

---

## 五、契约验证测试（本轮新增）

因 native 算法需要完整病例数据才能产生有意义输出，且多数输出缓冲区契约头文件未定义，
本轮采用**符号级冒烟测试**（`NativeContractTests.cs`）：
- 以 x64 进程加载 `libaecg.dll`，调用无副作用的 `aecgVersion()` 验证导出符号与 CallingConvention 正确。
- 验证 PE 位数为 x64（防止误部署 x86 导致 `BadImageFormatException`）。
- 不调用会写内存/需病例数据的函数（避免越界契约未明时触发崩溃）。

这锁定"DLL 可加载 + 导出符号存在 + 位数正确"这一层契约，是 golden data 回归（P2-1）的前置条件。

---

## 六、无法验证的契约缺口（需外部输入）

1. **NIBP 全套契约**（R001 阻断根源）：需 `F:\temp\figurePlot\figurePlot.h/.c`、`fitCurveGet.h/.c`、`pulseWaveGet.h/.c`。
2. **`aecgGetHrvPsd` 262145 点是否强制**：需算法团队确认 `fftNum`/`psdNum` 语义。
3. **`aecgCorrectLead` 的 indexArray 长度**：头文件无长度参数。
4. **`ecg_errors.h` 返回码数值**：需该文件定义。
5. **`testWave.dll` / TransformZQ**：DLL 与头文件均不在工作区。
6. **9 处 out IntPtr 的所有权**：需确认 native 是否有 `aecgFree*` 释放 API（头文件未声明）。

> 上述缺口中，1 是阻断项 R001 的前置——取得 NIBP 头文件后，P2-1 golden 回归与 R001 修复才能推进。
