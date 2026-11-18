using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisApp.Commons
{
    public class AEcgHandler
    {
        private const string DLL_PATH = @"lib\unmanaged\libaecg.dll";

        #region 非托管参数结构与方法相关声明
        /// <summary>
        /// ECG 功能（操作）类型枚举
        /// </summary>
        public enum AEcgFuncType
        {
            AEcgNone = 0,                     // 未知
            AEcgVersion = 1,                  // 获取库版本信息
            AecgInit = 2,                     // 库初始化
            AecgDeinit = 3,                   // 库资源释放
            AecgSetConfig = 4,                // 重新配置动态心电，必须立即生效
            AecgLoadData = 5,                 // 加载已分析的病例文件，为后续其他操作加载数据到内存
            AecgReanalyze = 6,                // 开始分析
            AecgDetectAF = 7,                 // 房颤分析
            AecgAnalyzeST = 8,                // ST事件分析
            AecgCalculateHRV = 9,             // 心率变异性分析
            AecgReanalyzeSegment = 10,        // 区间重分析（R波和心搏类型）
            AecgReanalyzePACSegment = 11,     // 区间房早重分析
            AecgAnalyzeTWA = 12,              // 计算给定信号起点后的1min数据的TWA
            AecgAnalyzeQTC = 13,              // 计算主导联的QT和QTc值
            AecgReclassifySimilarBeatsInSubTemplate = 14, // 子模板下与该QRS形态相似的QRS波，归类为指定的模板类型
            AecgReclusterSubTemplatesByLead = 15, // 依据指定的心搏类型模板，重新划分子模板
            AecgMergeTemplates = 16,          // 合并子模板
            AecgModifyBeats = 17,             // 修改指定位置的心搏类型
            AecgDeleteBeats = 18,             // 删除指定位置的心搏类型
            AecgInsertBeat = 19,              // 指定位置精确插入心搏
            AecgInsertBeatBefore = 20,        // 在参考心搏之前插入心搏
            AecgInsertBeatAfter = 21,         // 在参考心搏之后插入心搏
            AecgAddAFEvent = 22,              // 新增房颤片段
            AecgDeleteAFEvents = 23,          // 批量删除房扑房颤
            AecgAddSTEvent = 24,              // 新增ST事件
            AecgDeleteSTEvents = 25,          // 删除ST事件
            AecgGetQRSInfo = 26,              // 查询波型位置信息
            AecgGetLeadQuality = 27,          // 获取导联的信号质量评分
            AecgMarkSegmentUnanalyzable = 28, // 设置为非分析区间
            AecgMarkSegmentAnalyzable = 29,   // 设置为可分析区间
            AecgGetBeatsInfo = 30,            // 获取所有心拍信息
            AecgGetStxInfo = 31,              // 获取ST的测量值
            AecgGetStEventInfo = 32,          // 获取ST事件信息
            AecgGetSegmentUnanalyzable = 33,  // 获取不可分析片段
            AecgGetHrvPsd = 34,               // 获取HRV的功率谱数据
            AecgGetHrvTimeDomain = 35,        // 获取HRV的5分钟和1小时的时域数据
            AecgGetHrvFreqDomain = 36,        // 获取HRV的频域数据
            AecgGetQtcInfo = 37,              // 获取QTc计算结果数据
            AecgGetAfEvent = 38,              // 获取房扑/房颤片段数据
            AecgGetSegmentsWave = 39,         // 获取指定导联多个片段的波形数据
            AecgGetNextQrsInfo = 40,          // 获取主导联指定采样点后的第一个波形qrs信息
            AecgGetPacemakerEvaluationInfo = 41, // 获取起搏器评估信息
            AecgGetPaceSimilarity = 42,       // 获取起搏相似性信息
            AecgSaveAnalyzeData = 43,         // 保存分析结果数据（退出病例时调用）
        }

        /// <summary>
        /// 片段的起止位置(采样点序号)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SegmentInfo
        {
            public int StartIndex;
            public int EndIndex;
        }

        //心搏信息
        [StructLayout(LayoutKind.Sequential)]
        public struct AEcgBeatInfo
        {
            public int rWavePos;            ///< R波位置数据
            public short rrDuration;        ///< RR间期(采样点个数)
            public short beatUiType;        ///< 心拍类型界面类型
            public short morphoType;        ///< 按照波形形态进行聚类的心拍类型,数量多的排序靠前
            public int startIdx;            ///< 心拍起点
            public int endIdx;              ///< 心拍终点
        }

        //ST信息
        [StructLayout(LayoutKind.Sequential)]
        public struct AEcgStEventInfo
        {
            public int statusFlag;                  ///< 状态位：比如已保存
            public int startPosition;               ///< 发生时间（与心搏单位相同：点，该值/采样率可换算成时间）
            public int durationTime;                ///< 持续时间（与心搏单位相同：点，该值/采样率可换算成时间）
            public float stPointVoltage;            ///< ST 值：mV
            public int lead;                        ///< 导联：0/1/2/3/4/5/6/7/8/9/10/11
            public int stSlope;                     ///< 斜率：0（水平）、1（上斜），2（下斜）
            public int hrMin;                       ///< 最慢心率：单位 bpm
            public int hrAvg;                       ///< 平均心率：单位 bpm
            public int hrMax;                       ///< 最快心率：单位 bpm
        }

        //ST测量值
        [StructLayout(LayoutKind.Sequential)]
        public struct AEcgStxInfo
        {
            public int stBeatPos;                   ///< ST事件心搏位置
            public short jAmp;                      ///< J点幅值（uV）
            public short stAmp;                     ///< ST点幅值（uV）
            public short slope;                     ///< 斜率。0（水平）、1（上斜），2（下斜）
            public short tWaveAmp;                  ///< T波幅值（uV）
            public short secTWaveAmp;               ///< T'波幅值（uV）
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LeadEvent
        {
            public short LeadIndex;
            public int StartIndex;///< 事件起点位置，-1表示无效
            public int EndIndex;///< 事件终点位置，-1表示无效
        }

        //QRS信息
        [StructLayout(LayoutKind.Sequential)]
        public struct AEcgQrsInfo
        {
            public int qrsStart;  ///< QRS波起始位置
            public int qrspeak;   ///< QRS波峰位置
            public int qrsEnd;    ///< QRS波终止位置
        }

        //HRV时域信息
        [StructLayout(LayoutKind.Sequential)]
        public struct AEcgHrvTimeDomainInfo
        {
            public int start_position;             ///< 分析的开始位置
            public int length_of_time;             ///< 分析的时长
            public int nns;                        ///< NN间期个数
            public int mean_nn;                    ///< 平均 NN 间期，单位：ms
            public int sdnn;                       ///< NN 间期的标准差, 正常值范围 141±39 ms

            public int sdnn_index;                 ///< 5mins 段 NN 间期标准差的平均值
            public int sdann;                      ///< 5 mins 段 NN 间期平均值的标准差, 正常值范围 127±35 ms

            public int sdsd;                       ///< 相邻 NN 间期之差的标准差，单位 ms
            public int nn50_count;                 ///< 相邻 RR 间期差值 ＞ 50ms 的个数
            public float pnn50;                    ///< 相邻RR间期差值＞50ms占总体的百分比
            public int rmssd;                      ///< 相邻 NN 间期差的均方根, 正常值范围 27±12 ms

            public float hrv_triangular_index;     ///< 三角指数, 正常值范围 37±15
        }

        //HRV频域信息
        [StructLayout(LayoutKind.Sequential)]
        public struct AEcgHrvFreqDomainInfo
        {
            public int start_position; ///< 分析的开始位置，单位：索引
            public int length_of_time; ///< 分析的时长，单位：分钟，
            public int mean_nn; ///< 分析时间段内 NN 间期的平均值，单位：ms

            public float total_power;  ///< 该时间段内 NN 间期变化的总功率, 正常值范围 3466±1018，单位 ms2
            public float ulf;  ///< 极低频段（≤ 0.003 Hz）功率，单位 ms2
            public float vlf;  ///< 超低频段（0.003-0.04 Hz）功率，单位 ms2
            public float lf;    ///< 低频段（0.04~0.15 Hz）功率,  正常值范围 1170±416，单位 ms2
            public float hf;  ///< 高频段（0.15~0.4 Hz）功率, 正常值范围 975±203，单位 ms2
            public float lf_normal;  ///< 归一化低频段功率： LF/(total power-VLF)*100, 正常值范围 54±4
            public float hf_normal;     ///< 归一化高频段功率:HF/(total power-VLF)*100, 正常值范围 29±3
            public float ratio_lf_hf;     ///< 正常值范围 1.5-2.0
        }

        //HRV功率谱数据
        [StructLayout(LayoutKind.Sequential)]
        public struct AEcgHrvPsdInfo
        {
            public AEcgHrvPsdInfo(double dataX, double dataY)
            {
                DataX = dataX;
                DataY = dataY;
            }
            public double DataX;         ///< 频谱图横坐标（频率轴），单位 Hz，数组长度由 `fftNum` 返回。
            public double DataY;         ///< 频谱图纵坐标（功率谱密度），单位 ms²/Hz，数组长度由 `fftNum` 返回。
        }

        //起搏功能评价结果
        [StructLayout(LayoutKind.Sequential)]
        public struct AEcgPacemakerEvaluation
        {
            public int rWavePosition;           ///< 起搏心搏所在R波位置（单位：样本点）
            public int failureType;             ///< 起搏评价类型：0-感知失败，1-输出失败，2-夺获失败
        }

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "aecgVersion")]
        public static extern IntPtr aecgVersion();

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "aecgInit")]
        public static extern IntPtr aecgInit(ReportProgress callback);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "aecgDeinit")]
        private static extern int aecgDeinit(ref IntPtr aecgPtr);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int aecgSetConfig(IntPtr aecgPtr, string pathName);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int aecgLoadData(IntPtr aecgPtr);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int aecgReanalyze(IntPtr aecgPtr);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgReanalyzeSegment(IntPtr aecgPtr, int startSample, int endSample, int leadIndex);


        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgGetSegmentsWave(IntPtr aecgPtr, int leadIndex, SegmentInfo[] segments, int segmentsNum, IntPtr wave, IntPtr status, ref int waveNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgGetBeatsInfo(IntPtr aecgPtr, out IntPtr beatInfo, out int beatsNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetSegmentUnanalyzable(IntPtr aecgPtr, out IntPtr segmentInfo, out int segmentInfoNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgMarkSegmentUnanalyzable(IntPtr aecgPtr, int startSample, int endSample);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgMarkSegmentAnalyzable(IntPtr aecgPtr, int startSample, int endSample);


        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgModifyBeats(IntPtr aecgPtr, int[] samples, int sampleLength, short beatType);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgDeleteBeats(IntPtr aecgPtr, int[] beatPos, int beatPosNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgInsertBeat(IntPtr aecgPtr, int[] beatPos, int beatPosNum, short beatType);

        //向前插入心搏
        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgInsertBeatBefore(IntPtr aecgPtr, int[] beatPos, int beatPosNum, int leadIndex, short beatType);

        //向后插入心搏
        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgInsertBeatAfter(IntPtr aecgPtr, int[] beatPos, int beatPosNum, int leadIndex, short beatType);


        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgAddAFEvent(IntPtr aecgPtr, int startSample, int endSample, short beatType);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgDeleteAFEvents(IntPtr aecgPtr, SegmentInfo[] segments, int segmentsNum, short beatType);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetAfEvent(IntPtr aecgPtr, out IntPtr segments, out int segmentNum, int beatType);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgDetectAF(IntPtr aecgPtr);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgGetLeadQuality(IntPtr aecgPtr, int[] leadIndices, int leadNum, IntPtr score);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgGetNextQrsInfo(IntPtr aecgPtr, int sample, ref AEcgQrsInfo qrsInfo);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgCorrectLead(IntPtr aecgPtr, int lFReverseFlag, int[] indexArray);


        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgSaveAnalyzeData(IntPtr aecgPtr);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetStEventInfo(IntPtr aecgPtr, out IntPtr stEventInfo, out int stEventInfoNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgDeleteSTEvents(IntPtr aecgPtr, LeadEvent[] leadEvent, int leadEventNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgAddSTEvent(IntPtr aecgPtr, AEcgStEventInfo stEventInfo);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgAnalyzeST(IntPtr aecgPtr);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetStxInfo(IntPtr aecgPtr, int leadIndex, out IntPtr stxInfo, out int stxInfoNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgCalculateHRV(IntPtr aecgPtr);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetHrvTimeDomain(IntPtr aecgPtr, short method, out IntPtr hrvTimeDomainInfo, out int hrvTimeDomainNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetHrvFreqDomain(IntPtr aecgPtr, out IntPtr hrvFreqDomainInfo, out int freqDomainNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetHrvPsd(IntPtr aecgPtr, out IntPtr dataX, out IntPtr dataY, int index, out int psdNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgReclusterSubTemplatesByLead(IntPtr aecgPtr, int leadIndex, short beatType);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgReclusterBeatsByLead(IntPtr aecgPtr, int leadIndex, int[] beatPos, int beatPosNum, IntPtr beatSubType);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgReclassifySimilarBeatsInSubTemplate(IntPtr aecgPtr, int sample, short beatType, int leadIndex, short morphoType);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern int aecgMergeTemplates(IntPtr aecgPtr, int[] fromShapes, int fromShapeNum, short toShape, short beatType);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetPacemakerEvaluationInfo(IntPtr aecgPtr, out IntPtr paceEvaluationInfo, out int paceEvaluationNum);

        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aecgGetPaceSimilarity(IntPtr aecgPtr, int leadIndex, int[] samples, int sampleLength, IntPtr paceSimilarity);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ReportProgress(AEcgFuncType type, int percent); //回调函数

        #endregion

        ///// <summary>
        ///// 当某一操作进度更新时发生的事件(操作完成后需取消订阅)
        ///// </summary>
        //public static event Action<AEcgFuncType, int> OnProgressUpdated;

        public static Dictionary<AEcgFuncType, int> FunctionPercents = new Dictionary<AEcgFuncType, int>();

        /// <summary>
        /// 当前累积的未保存的更新次数
        /// </summary>
        public static int UpdatesCount { get; private set; }
        /// <summary>
        /// 最后一次发生数据更新的时间
        /// </summary>
        public static DateTime LastUpdateTime { get; private set; }
        /// <summary>
        /// 数据更新的类别
        /// </summary>
        public static EcgUpdateType UpdateType { get; private set; }
        public static bool IsBusy { get; private set; }

        private static IntPtr _aecgPtr = IntPtr.Zero;
        private static GCHandle _gcHandle; //保持委托的引用, 防止垃圾回收器回收
        private static readonly object _lockObj = new object();

        public static void Init()
        {
            ReportProgress callbackFunc = new ReportProgress(processResult);
            _gcHandle = GCHandle.Alloc(callbackFunc);
            _aecgPtr = aecgInit(callbackFunc);
        }

        public static int DeInit()
        {
            _gcHandle.Free();
            return aecgDeinit(ref _aecgPtr);
        }

        public static string GetAEcgVersion()
        {
            IntPtr verPtr = aecgVersion();
            string versionText = Marshal.PtrToStringAnsi(verPtr);
            return versionText;
        }

        /// <summary>
        /// 设置文件所在目录路径(holter.info等)
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static int SetConfig(string dirPath)
        {
            return aecgSetConfig(_aecgPtr, dirPath);
        }

        /// <summary>
        /// 加载心电文件数据(若文件不存在则先生成文件)
        /// </summary>
        /// <returns></returns>
        public static int LoadData()
        {
            IsBusy = true;
            FunctionPercents[AEcgFuncType.AecgLoadData] = 0;
            int ret = aecgLoadData(_aecgPtr);
            IsBusy = false;

            return ret;
        }

        /// <summary>
        /// 导联纠错
        /// </summary>
        /// <returns></returns>
        public static int CorrectLeads(bool limbLeadChanged, int[] chestLeads)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.CorrectLead;
            try
            {
                if (chestLeads == null) chestLeads = new int[] { 1, 2, 3, 4, 5, 6 };
                Stopwatch sw = Stopwatch.StartNew();
                int ret = aecgCorrectLead(_aecgPtr, limbLeadChanged ? 1 : 0, chestLeads);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo };
                OperateLog.logParam("导联纠错", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }


        /// <summary>
        /// 重分析
        /// </summary>
        /// <returns></returns>
        public static int Reanalyze()
        {
            IsBusy = true;
            FunctionPercents[AEcgFuncType.AecgReanalyze] = 0;
            try
            {
                //通知算法重新获取配置信息
                int confSet = SetConfig(AnaValues.RootPath);
                if (confSet != 0) return confSet;

                int ret = aecgReanalyze(_aecgPtr);
                if (ret == 0)
                {
                    AnaValues.UnanalyzableSegments = GetSegmentUnanalyzable();
                    RemoveLocalFiles();
                    refreshDataState(resetUI: true);
                }
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        public static void RemoveLocalFiles()
        {
            string segmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            if (File.Exists(segmentFilePath))
            {
                File.Delete(segmentFilePath);
            }

            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath))
            {
                File.Delete(deletedEventsFilePath);
            }

            string summaryInfoFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SummaryFileName);
            if (File.Exists(summaryInfoFilePath))
            {
                File.Delete(summaryInfoFilePath);
            }

            string hRVAdviceFilePath = Path.Combine(AnaValues.RootPath, AnaValues.HRVAdviceFileName);
            if (File.Exists(hRVAdviceFilePath))
            {
                File.Delete(hRVAdviceFilePath);
            }

            string bpInfoFilePath = Path.Combine(AnaValues.RootPath, AnaValues.BPInfoFileName);
            if (File.Exists(bpInfoFilePath))
            {
                File.Delete(bpInfoFilePath);
            }
        }


        /// <summary>
        /// 区间重分析
        /// </summary>
        /// <returns></returns>
        public static int ReanalyzeSegment(int startSample, int endSample, int leadIndex)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.RangeReanalyze;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                int ret = aecgReanalyzeSegment(_aecgPtr, startSample, endSample, leadIndex);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, startSample, endSample };
                OperateLog.logParam("心电数据重分析", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 修改心搏属性
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="beatUiType"></param>
        /// <returns></returns>
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public static int ModifyBeats(int[] samples, char beatUiType)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.ModifyBeats;
            try
            {
                int ret = 0;
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);

                if (beatUiType == 'N' || beatUiType == 'S')
                {
                    List<int> AfSamples = new List<int>();
                    List<int> AFSamples = new List<int>();
                    List<int> otherSamples = new List<int>();
                    List<AfInfo> lstAf = AnaCode.GetAfList();
                    for (int i = 0; i < samples.Length; i++)
                    {
                        char typeName = AnaCode.GetAfTypeName(samples[i], lstAf);
                        if (typeName == 'K')
                            AfSamples.Add(samples[i]);
                        else if (typeName == 'M')
                            AFSamples.Add(samples[i]);
                        else
                            otherSamples.Add(samples[i]);
                    }
                    if (AfSamples.Count > 0)
                    {
                        ret = aecgModifyBeats(_aecgPtr, AfSamples.ToArray(), AfSamples.Count, Convert.ToInt16('K'));
                    }
                    if (AFSamples.Count > 0)
                    {
                        ret = aecgModifyBeats(_aecgPtr, AFSamples.ToArray(), AFSamples.Count, Convert.ToInt16('M'));
                    }
                    if (otherSamples.Count > 0)
                    {
                        ret = aecgModifyBeats(_aecgPtr, otherSamples.ToArray(), otherSamples.Count, beatType);
                    }
                }
                else
                {
                    ret = aecgModifyBeats(_aecgPtr, samples, samples.Length, beatType);
                }
                if (ret == 0)
                {
                    refreshDataState();
                }

                if (beatType != 'F')
                {
                    //删除事件相关室性融合波信息
                    AnaEvent.DeleteNcaOrFBeat(samples.ToList(), "室性融合波");
                }

                object para = new { AnaValues.CurrentCase.caseNo, samples = PubFuns.JoinWithEllipsis(samples), targetBeatType = beatUiType };
                OperateLog.logParam("心搏属性修改", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 删除指定位置(多个)的心搏
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static int DeleteBeats(int[] samples)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.DeleteBeats;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                int ret = aecgDeleteBeats(_aecgPtr, samples, samples.Length);
                if (ret == 0)
                {
                    refreshDataState();
                }

                //删除事件相关室性融合波信息
                AnaEvent.DeleteNcaOrFBeat(samples.ToList(), "室性融合波");

                object para = new { AnaValues.CurrentCase.caseNo, samples = PubFuns.JoinWithEllipsis(samples) };
                OperateLog.logParam("心搏删除", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 批量插入心搏
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="beatUiType"></param>
        /// <returns></returns>
        public static int AddBeats(int[] samples, char beatUiType)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.AddBeats;
            try
            {
                int ret = -1;
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);
                if (beatUiType == 'N' || beatUiType == 'S')
                {
                    List<int> AfSamples = new List<int>();
                    List<int> AFSamples = new List<int>();
                    List<int> otherSamples = new List<int>();
                    List<AfInfo> lstAf = AnaCode.GetAfList();
                    for (int i = 0; i < samples.Length; i++)
                    {
                        char typeName = AnaCode.GetAfTypeName(samples[i], lstAf);
                        if (typeName == 'K')
                            AfSamples.Add(samples[i]);
                        else if (typeName == 'M')
                            AFSamples.Add(samples[i]);
                        else
                            otherSamples.Add(samples[i]);
                    }
                    if (AfSamples.Count > 0)
                    {
                        ret = aecgInsertBeat(_aecgPtr, AfSamples.ToArray(), AfSamples.Count, Convert.ToInt16('K'));
                    }
                    if (AFSamples.Count > 0)
                    {
                        ret = aecgInsertBeat(_aecgPtr, AFSamples.ToArray(), AFSamples.Count, Convert.ToInt16('M'));
                    }
                    if (otherSamples.Count > 0)
                    {
                        ret = aecgInsertBeat(_aecgPtr, otherSamples.ToArray(), otherSamples.Count, beatType);
                    }
                }
                else
                {
                    if (samples.Length > 0)
                    {
                        ret = aecgInsertBeat(_aecgPtr, samples, samples.Length, beatType);
                    }
                }
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, samples = PubFuns.JoinWithEllipsis(samples), beatType = beatUiType };
                OperateLog.logParam("插入心搏", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// (批量)向前插入心搏
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="leadIndex"></param>
        /// <param name="beatUiType"></param>
        /// <returns></returns>
        public static int InsertBeatsBefore(int[] samples, int leadIndex, char beatUiType)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.AddBeats;
            try
            {
                int ret = -1;
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);

                if (samples.Length > 0)
                {
                    ret = aecgInsertBeatBefore(_aecgPtr, samples, samples.Length, leadIndex, beatType);
                }
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, samples = PubFuns.JoinWithEllipsis(samples), beatType = beatUiType };
                OperateLog.logParam("向前插入心搏", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// (批量)向后插入心搏
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="leadIndex"></param>
        /// <param name="beatUiType"></param>
        /// <returns></returns>
        public static int InsertBeatsAfter(int[] samples, int leadIndex, char beatUiType)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.AddBeats;
            try
            {
                int ret = -1;
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);

                if (samples.Length > 0)
                {
                    ret = aecgInsertBeatAfter(_aecgPtr, samples, samples.Length, leadIndex, beatType);
                }
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, samples = PubFuns.JoinWithEllipsis(samples), beatType = beatUiType };
                OperateLog.logParam("向后插入心搏", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }


        /// <summary>
        /// 获取指定(房颤/房扑)类型的片段
        /// </summary>
        /// <returns></returns>
        public static List<SegmentInfo> GetAfSegments(char beatUiType)
        {
            IsBusy = true;
            List<SegmentInfo> result = new List<SegmentInfo>();
            try
            {
                IntPtr segmentInfoPtr = IntPtr.Zero;
                int segmentCount = 0;
                int ret = aecgGetAfEvent(_aecgPtr, out segmentInfoPtr, out segmentCount, Convert.ToInt32(beatUiType));
                if (ret == 0 && segmentInfoPtr != IntPtr.Zero)
                {
                    for (int i = 0; i < segmentCount; i++)
                    {
                        IntPtr current = IntPtr.Add(segmentInfoPtr, i * Marshal.SizeOf(typeof(SegmentInfo)));
                        SegmentInfo segmentInfo = Marshal.PtrToStructure<SegmentInfo>(current);
                        result.Add(segmentInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return result;
        }

        /// <summary>
        /// 添加房颤/房扑
        /// </summary>
        /// <returns></returns>
        public static int AddAf(int startSample, int endSample, char beatUiType)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.AddAf;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);
                int ret = aecgAddAFEvent(_aecgPtr, startSample, endSample, beatType);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, startSample, endSample, beatType = beatUiType };
                OperateLog.logParam(beatUiType == 'K' ? "添加房颤" : "添加房扑", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 删除指定区间内的房颤或房扑
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="beatUiType"></param>
        /// <returns></returns>
        public static int DeleteAFEvents(SegmentRange[] segments, char beatUiType)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.DeleteAF;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);
                SegmentInfo[] arrSegments = Array.ConvertAll(segments, (input) => new SegmentInfo() { StartIndex = input.StartIndex, EndIndex = input.EndIndex });
                int ret = aecgDeleteAFEvents(_aecgPtr, arrSegments, segments.Length, beatType);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, segments, beatType = beatUiType };
                OperateLog.logParam("删除房颤或房扑", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 检测房颤
        /// </summary>
        /// <returns></returns>
        public static int DetectAF()
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.DetectAF;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                int ret = aecgDetectAF(_aecgPtr);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo };
                OperateLog.logParam("检测房颤", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }


        /// <summary>
        /// 获取指定导联多个片段的波形数据
        /// </summary>
        /// <param name="leadIndex"></param>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static WaveInfo[] GetWaveData(int leadIndex, SegmentRange[] segments)
        {
            IsBusy = true;
            WaveInfo[] arrResult = new WaveInfo[segments.Length];

            IntPtr wavePtr = IntPtr.Zero;
            IntPtr statuPtr = IntPtr.Zero;
            try
            {
                SegmentInfo[] arrSegments = Array.ConvertAll(segments, (input) => new SegmentInfo() { StartIndex = input.StartIndex, EndIndex = input.EndIndex });

                int targetLen = segments.Sum(o => o.EndIndex - o.StartIndex);
                wavePtr = Marshal.AllocHGlobal(targetLen * sizeof(short));
                statuPtr = Marshal.AllocHGlobal(targetLen * sizeof(int));
                int ret = aecgGetSegmentsWave(_aecgPtr, leadIndex, arrSegments, segments.Length, wavePtr, statuPtr, ref targetLen);
                if (ret == 0)
                {
                    int resolvedNum = 0;
                    for (int i = 0; i < segments.Length; i++)
                    {
                        short[] segmentData = null;
                        int[] statuData = null;
                        if (i < segments.Length - 1)
                        {
                            segmentData = new short[segments[i].EndIndex - segments[i].StartIndex];
                            statuData = new int[segments[i].EndIndex - segments[i].StartIndex];
                        }
                        else
                        {
                            segmentData = new short[targetLen - resolvedNum];
                            statuData = new int[targetLen - resolvedNum];
                        }
                        IntPtr ptr0 = new IntPtr(wavePtr.ToInt64() + 2 * resolvedNum);
                        Marshal.Copy(ptr0, segmentData, 0, segmentData.Length);

                        IntPtr ptr1 = new IntPtr(statuPtr.ToInt64() + 4 * resolvedNum);
                        Marshal.Copy(ptr1, statuData, 0, statuData.Length);

                        resolvedNum += segmentData.Length;
                        arrResult[i] = new WaveInfo()
                        {
                            leadIndex = leadIndex,
                            WaveData = segmentData,
                            StatusData = statuData,
                            DataLength = segmentData.Length
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally
            {
                Marshal.FreeHGlobal(wavePtr);
                Marshal.FreeHGlobal(statuPtr);
                IsBusy = false;
            }

            return arrResult;
        }

        /// <summary>
        /// 获取所有心搏信息
        /// </summary>
        /// <returns></returns>
        public static List<AEcgBeatInfo> GetBeatsInfo()
        {
            IsBusy = true;
            List<AEcgBeatInfo> lst = new List<AEcgBeatInfo>();
            try
            {
                IntPtr beatInfoPtr = IntPtr.Zero;
                int beatsNum = 0;
                int ret = aecgGetBeatsInfo(_aecgPtr, out beatInfoPtr, out beatsNum);
                if (ret == 0 && beatInfoPtr != IntPtr.Zero)
                {
                    lst.Capacity = beatsNum;
                    for (int i = 0; i < beatsNum; i++)
                    {
                        IntPtr current = IntPtr.Add(beatInfoPtr, i * Marshal.SizeOf(typeof(AEcgBeatInfo)));
                        AEcgBeatInfo beat = Marshal.PtrToStructure<AEcgBeatInfo>(current);
                        lst.Add(beat);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return lst;
        }

        /// <summary>
        /// 获取不可分析段
        /// </summary>
        /// <returns></returns>
        public static List<SegmentInfo> GetSegmentUnanalyzable()
        {
            IsBusy = true;
            List<SegmentInfo> result = new List<SegmentInfo>();
            try
            {
                IntPtr segmentInfoPtr = IntPtr.Zero;
                int segmentCount = 0;
                int ret = aecgGetSegmentUnanalyzable(_aecgPtr, out segmentInfoPtr, out segmentCount);
                if (ret == 0 && segmentInfoPtr != IntPtr.Zero)
                {
                    result = new List<SegmentInfo>(segmentCount);
                    for (int i = 0; i < segmentCount; i++)
                    {
                        IntPtr current = IntPtr.Add(segmentInfoPtr, i * Marshal.SizeOf(typeof(SegmentInfo)));
                        SegmentInfo segmentInfo = Marshal.PtrToStructure<SegmentInfo>(current);
                        result.Add(segmentInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return result.OrderBy(b => b.StartIndex).ToList();
        }

        /// <summary>
        /// 设为无效分析段(供分析完之后调用)
        /// </summary>
        /// <param name="startSample"></param>
        /// <param name="endSample"></param>
        /// <returns></returns>
        public static int MarkSegmentUnanalyzable(int startSample, int endSample)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.SetAsUnanalyzable;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                int ret = aecgMarkSegmentUnanalyzable(_aecgPtr, startSample, endSample);
                if (ret == 0)
                {
                    AnaValues.UnanalyzableSegments = GetSegmentUnanalyzable();
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, startSample, endSample };
                OperateLog.logParam("设为无效片段", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 接收无效分析段(设为有效分析段, 供分析完之后调用)
        /// </summary>
        /// <param name="startSample"></param>
        /// <param name="endSample"></param>
        /// <returns></returns>
        public static int MarkSegmentAnalyzable(int startSample, int endSample)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.SetAsAnalyzable;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                int ret = aecgMarkSegmentAnalyzable(_aecgPtr, startSample, endSample);
                if (ret == 0)
                {
                    AnaValues.UnanalyzableSegments = GetSegmentUnanalyzable();
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, startSample, endSample };
                OperateLog.logParam("恢复为可分析区间", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }


        /// <summary>
        /// 获取ST事件信息
        /// </summary>
        /// <returns></returns>
        public static List<AEcgStEventInfo> GetStEventInfo()
        {
            IsBusy = true;
            List<AEcgStEventInfo> result = null;
            try
            {
                IntPtr stEventInfoPtr = IntPtr.Zero;
                int stEventInfoCount = 0;
                int ret = aecgGetStEventInfo(_aecgPtr, out stEventInfoPtr, out stEventInfoCount);
                if (ret == 0 && stEventInfoPtr != IntPtr.Zero)
                {
                    result = new List<AEcgStEventInfo>(stEventInfoCount);
                    for (int i = 0; i < stEventInfoCount; i++)
                    {
                        IntPtr current = IntPtr.Add(stEventInfoPtr, i * Marshal.SizeOf(typeof(AEcgStEventInfo)));
                        AEcgStEventInfo stEventInfo = Marshal.PtrToStructure<AEcgStEventInfo>(current);
                        result.Add(stEventInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return result;
        }

        /// <summary>
        /// 删除ST事件
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public static int DeleteSTEvents(STEventRange[] events)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.DeleteSTEvent;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                LeadEvent[] arrEvents = Array.ConvertAll(events, (input) => new LeadEvent() { LeadIndex = input.LeadIndex, StartIndex = input.StartIndex, EndIndex = input.EndIndex });
                int ret = aecgDeleteSTEvents(_aecgPtr, arrEvents, events.Length);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, segments = events };
                OperateLog.logParam("删除ST事件", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        public static int AddSTEvent(AEcgStEventInfo stEvent)
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.AddSTEvent;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                int ret = aecgAddSTEvent(_aecgPtr, stEvent);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, STEventInfo = stEvent };
                OperateLog.logParam("新增ST事件", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        public static int AnalyzeST()
        {
            IsBusy = true;
            UpdateType = EcgUpdateType.AnalyzeST;
            try
            {
                int ret = aecgAnalyzeST(_aecgPtr);
                AnaCode.IsAnalysisST = false;
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 获取ST测量信息
        /// </summary>
        /// <returns></returns>
        public static List<AEcgStxInfo> GetStxInfo(int leadIndex)
        {
            IsBusy = true;
            List<AEcgStxInfo> result = null;
            try
            {
                IntPtr stxInfoPtr = IntPtr.Zero;
                int stxInfoCount = 0;
                int ret = aecgGetStxInfo(_aecgPtr, leadIndex, out stxInfoPtr, out stxInfoCount);
                if (ret == 0 && stxInfoPtr != IntPtr.Zero)
                {
                    result = new List<AEcgStxInfo>(stxInfoCount);
                    for (int i = 0; i < stxInfoCount; i++)
                    {
                        IntPtr current = IntPtr.Add(stxInfoPtr, i * Marshal.SizeOf(typeof(AEcgStxInfo)));
                        AEcgStxInfo stxInfo = Marshal.PtrToStructure<AEcgStxInfo>(current);
                        result.Add(stxInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }
            return result;
        }

        /// <summary>
        /// 获取导联质量评分
        /// </summary>
        /// <param name="leadIndices"></param>
        /// <returns></returns>
        public static int[] GetLeadQuality(int[] leadIndices)
        {
            IsBusy = true;
            int[] scores = null;
            int cnt = leadIndices.Length;
            IntPtr scorePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * cnt);
            try
            {
                int ret = aecgGetLeadQuality(_aecgPtr, leadIndices, cnt, scorePtr);
                if (ret == 0)
                {
                    scores = new int[cnt];
                    Marshal.Copy(scorePtr, scores, 0, cnt);
                }
            }
            catch
            {
            }
            finally
            {
                Marshal.FreeHGlobal(scorePtr);
                IsBusy = false;
            }

            return scores;
        }

        /// <summary>
        /// 获取指定采样点位置之后的第一个心拍信息
        /// </summary>
        /// <param name="refSample">相对采样点位置</param>
        /// <returns>前一个QRS波峰位置, 当前QRS波峰位置及后一个QRS波峰位置</returns>
        public static QrsInfo GetFollowingQrsInfo(int refSample)
        {
            IsBusy = true;
            QrsInfo result = default(QrsInfo);
            try
            {
                AEcgQrsInfo qrsIno = default(AEcgQrsInfo);
                int ret = aecgGetNextQrsInfo(_aecgPtr, refSample, ref qrsIno);
                if (ret == 0)
                {
                    result = new QrsInfo() { Start = qrsIno.qrsStart, Peak = qrsIno.qrspeak, End = qrsIno.qrsEnd };
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return result;
        }


        /// <summary>
        /// 执行心率变异性相关分析与计算
        /// </summary>
        /// <returns></returns>
        public static int CalculateHRV()
        {
            IsBusy = true;
            try
            {
                int ret = aecgCalculateHRV(_aecgPtr);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 按导联重排序(重新归类)
        /// </summary>
        /// <param name="leadIndex"></param>
        /// <param name="beatType"></param>
        /// <returns></returns>
        public static int ReclusterSubTemplatesByLead(int leadIndex, char beatUiType)
        {
            IsBusy = true;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);
                int ret = aecgReclusterSubTemplatesByLead(_aecgPtr, leadIndex, beatType);
                if (ret == 0)
                {
                    refreshDataState(false);
                }

                object para = new { AnaValues.CurrentCase.caseNo, leadIndex, beatType = beatUiType };
                OperateLog.logParam("子模板按导联重排序", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 按指定导联对心拍数据进行重聚类，获取聚类后的子类型
        /// </summary>
        public static short[] ReclusterBeatsByLead(int leadIndex, int[] arrBeatPos)
        {
            IsBusy = true;
            short[] subTypes = null;
            int cnt = arrBeatPos.Length;
            IntPtr subTypePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * cnt);
            try
            {
                int ret = aecgReclusterBeatsByLead(_aecgPtr, leadIndex, arrBeatPos, cnt, subTypePtr);
                if (ret == 0)
                {
                    subTypes = new short[cnt];
                    Marshal.Copy(subTypePtr, subTypes, 0, cnt);
                }
            }
            catch
            {
            }
            finally
            {
                Marshal.FreeHGlobal(subTypePtr);
                IsBusy = false;
            }

            return subTypes;
        }

        /// <summary>
        /// 仅针对指定的导联及 QRS 心搏，将其所在子模板下与该 QRS 形态相似的 QRS 波，归类为指定的模板类型，归类后默认的子模板为单独一类
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="beatUiType"></param>
        /// <param name="leadIndex"></param>
        /// <returns></returns>
        public static int ReclassifySimilarBeatsInSubTemplate(int sample, char beatUiType, int leadIndex, short morphoType)
        {
            IsBusy = true;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);
                int ret = aecgReclassifySimilarBeatsInSubTemplate(_aecgPtr, sample, beatType, leadIndex, morphoType);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, sample, leadIndex, morphoType, beatType = beatUiType };
                OperateLog.logParam("按QRS形态再分类", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }
        /// <summary>
        /// 合并子模板
        /// </summary>
        /// <param name="fromShapes"></param>
        /// <param name="toShape"></param>
        /// <param name="beatUiType"></param>
        /// <returns></returns>
        public static int MergeTemplates(int[] fromShapes, short toShape, char beatUiType)
        {
            IsBusy = true;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                short beatType = Convert.ToInt16(beatUiType);
                int ret = aecgMergeTemplates(_aecgPtr, fromShapes, fromShapes.Length, toShape, beatType);
                if (ret == 0)
                {
                    refreshDataState();
                }

                object para = new { AnaValues.CurrentCase.caseNo, fromShapes, toShape, beatType = beatUiType };
                OperateLog.logParam("合并子模板", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                return ret;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
                return 1;
            }
            finally { IsBusy = false; }
        }

        /// <summary>
        /// 获取hrv时域信息
        /// </summary>
        /// <returns></returns>
        public static List<AEcgHrvTimeDomainInfo> GetHrvTimeDomain()
        {
            IsBusy = true;
            short method = 1;       //0 获取5分钟的HRV的频域数据 1 获取1小时的HRV的频域数据
            List<AEcgHrvTimeDomainInfo> result = new List<AEcgHrvTimeDomainInfo>();
            try
            {
                IntPtr hrvTimeDomainInfoPtr = IntPtr.Zero;
                int hrvTimeDomainInfoCount = 0;
                int ret = aecgGetHrvTimeDomain(_aecgPtr, method, out hrvTimeDomainInfoPtr, out hrvTimeDomainInfoCount);
                if (ret == 0 && hrvTimeDomainInfoPtr != IntPtr.Zero)
                {
                    result = new List<AEcgHrvTimeDomainInfo>(hrvTimeDomainInfoCount);
                    for (int i = 0; i < hrvTimeDomainInfoCount; i++)
                    {
                        IntPtr current = IntPtr.Add(hrvTimeDomainInfoPtr, i * Marshal.SizeOf(typeof(AEcgHrvTimeDomainInfo)));
                        AEcgHrvTimeDomainInfo hrvTimeDomainInfo = Marshal.PtrToStructure<AEcgHrvTimeDomainInfo>(current);
                        result.Add(hrvTimeDomainInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return result;
        }

        /// <summary>
        /// 获取hrv频域信息
        /// </summary>
        /// <returns></returns>
        public static List<AEcgHrvFreqDomainInfo> GetHrvFreqDomain()
        {
            IsBusy = true;
            List<AEcgHrvFreqDomainInfo> result = new List<AEcgHrvFreqDomainInfo>();
            try
            {
                IntPtr hrvFreqDomainInfoPtr = IntPtr.Zero;
                int hrvFreqDomainInfoCount = 0;
                int ret = aecgGetHrvFreqDomain(_aecgPtr, out hrvFreqDomainInfoPtr, out hrvFreqDomainInfoCount);
                if (ret == 0 && hrvFreqDomainInfoPtr != IntPtr.Zero)
                {
                    result = new List<AEcgHrvFreqDomainInfo>(hrvFreqDomainInfoCount);
                    for (int i = 0; i < hrvFreqDomainInfoCount; i++)
                    {
                        IntPtr current = IntPtr.Add(hrvFreqDomainInfoPtr, i * Marshal.SizeOf(typeof(AEcgHrvFreqDomainInfo)));
                        AEcgHrvFreqDomainInfo hrvFreqDomainInfo = Marshal.PtrToStructure<AEcgHrvFreqDomainInfo>(current);
                        result.Add(hrvFreqDomainInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return result;
        }

        /// <summary>
        /// 获取hrv相关功率谱数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static List<AEcgHrvPsdInfo> GetHrvPsd(int index)
        {
            IsBusy = true;
            List<AEcgHrvPsdInfo> result = new List<AEcgHrvPsdInfo>();
            try
            {
                IntPtr hrvPsdDataXPtr = IntPtr.Zero;
                IntPtr hrvPsdDataYPtr = IntPtr.Zero;

                int hrvPsdInfoCount = 0;
                int ret = aecgGetHrvPsd(_aecgPtr, out hrvPsdDataXPtr, out hrvPsdDataYPtr, index, out hrvPsdInfoCount);
                if (ret == 0 && (hrvPsdDataXPtr != IntPtr.Zero && hrvPsdDataYPtr != IntPtr.Zero))
                {
                    result = new List<AEcgHrvPsdInfo>(hrvPsdInfoCount);
                    double[] hrvPsdDataX = new double[hrvPsdInfoCount];
                    Marshal.Copy(hrvPsdDataXPtr, hrvPsdDataX, 0, hrvPsdInfoCount);
                    double[] hrvPsdDataY = new double[hrvPsdInfoCount];
                    Marshal.Copy(hrvPsdDataYPtr, hrvPsdDataY, 0, hrvPsdInfoCount);
                    for (int i = 0; i < hrvPsdInfoCount; i++)
                    {
                        if (hrvPsdDataX[i] > 0.5)
                        {
                            break;
                        }

                        AEcgHrvPsdInfo hrvPsdInfo = new AEcgHrvPsdInfo(hrvPsdDataX[i], hrvPsdDataY[i]);
                        result.Add(hrvPsdInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return result;
        }

        /// <summary>
        /// 获取起搏功能评价结果
        /// </summary>
        /// <returns></returns>
        public static List<AEcgPacemakerEvaluation> GetPacemakerEvaluationInfo()
        {
            IsBusy = true;
            List<AEcgPacemakerEvaluation> result = new List<AEcgPacemakerEvaluation>();
            try
            {
                IntPtr pacemakerEvaluationInfoPtr = IntPtr.Zero;
                int pacemakerEvaluationInfoCount = 0;
                int ret = aecgGetPacemakerEvaluationInfo(_aecgPtr, out pacemakerEvaluationInfoPtr, out pacemakerEvaluationInfoCount);
                if (ret == 0 && pacemakerEvaluationInfoPtr != IntPtr.Zero)
                {
                    result = new List<AEcgPacemakerEvaluation>(pacemakerEvaluationInfoCount);
                    for (int i = 0; i < pacemakerEvaluationInfoCount; i++)
                    {
                        IntPtr current = IntPtr.Add(pacemakerEvaluationInfoPtr, i * Marshal.SizeOf(typeof(AEcgPacemakerEvaluation)));
                        AEcgPacemakerEvaluation hrvFreqDomainInfo = Marshal.PtrToStructure<AEcgPacemakerEvaluation>(current);
                        result.Add(hrvFreqDomainInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally { IsBusy = false; }

            return result;
        }

        /// <summary>
        /// 获取心拍相似度
        /// </summary>
        /// <returns></returns>
        public static double[] GetPaceSimilarity(int leadIndex, int[] samples)
        {
            IsBusy = true;
            double[] similaritys = null;
            int count = samples.Length;
            IntPtr similarityPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(double)) * count);
            try
            {
                //int count = cnt;
                int ret = aecgGetPaceSimilarity(_aecgPtr, leadIndex, samples, count, similarityPtr);
                if (ret == 0)
                {
                    similaritys = new double[count];
                    Marshal.Copy(similarityPtr, similaritys, 0, count);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法分析");
            }
            finally
            {
                Marshal.FreeHGlobal(similarityPtr);
                IsBusy = false;
            }

            return similaritys;
        }


        /// <summary>
        /// 保存所有更新的数据
        /// </summary>
        /// <returns></returns>
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public static void SaveAnalyzeData()
        {
            IsBusy = true;
            lock (_lockObj)
            {
                try
                {
                    int ret = aecgSaveAnalyzeData(_aecgPtr);
                    if (ret == 0)
                    {
                        UpdatesCount = 0;
                    }
                    else
                    {
                        AnalysisWindow.Instance.InvokeIfRequired(() =>
                        {
                            ScreenCenterTip.Show("数据更新或保存失败");
                        });
                        Logger.CreateLog(new Exception("数据更新或保存失败"), "算法分析");
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "算法分析");
                }
                finally { IsBusy = false; }
            }
        }

        //算法回调处理
        private static void processResult(AEcgFuncType funcType, int percent)
        {
            try
            {
                //OnProgressUpdated?.Invoke(funcType, percent);
                FunctionPercents[funcType] = percent;
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} 当前算法处理类别：{funcType}，进度：{percent}/100");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "算法回调处理");
            }
        }

        //数据更新
        private static void refreshDataState(bool calculate = true, bool resetUI = false)
        {
            //更新缓存的心搏信息
            List<AEcgBeatInfo> beats = GetBeatsInfo();
            AnaValues.CachedAllBeats = beats.ConvertToSystemBeats(AnaValues.holterConfig.SampleRate);
            AnaValues.CurrentBeatMap = AnaCode.GetBeatMap(AnaValues.CachedAllBeats);

            if (calculate) { AnaCode.LoadStatisticsData(); }

            LastUpdateTime = DateTime.Now;

            if (!resetUI)
            {
                UpdatesCount++;
                AnaValues.DataSourceState = 1;
            }
            else
            {
                AnaValues.DataSourceState = 2;
            }
        }

        public static string GetFuncName(AEcgFuncType funcType)
        {
            string funcName = "";
            if (funcType == AEcgFuncType.AecgLoadData)
                funcName = "文件加载...";
            else if (funcType == AEcgFuncType.AecgReanalyze)
                funcName = "开始分析...";
            else if (funcType == AEcgFuncType.AecgDetectAF)
                funcName = "房扑房颤分析中...";
            else if (funcType == AEcgFuncType.AecgAnalyzeST)
                funcName = "ST事件分析...";

            return funcName;
        }
    }
}
