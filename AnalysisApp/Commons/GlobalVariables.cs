using System;
using System.Collections.Generic;
using System.Threading;
using Zoncare.Common;
using Zoncare.DAL;

namespace AnalysisApp.Commons
{
    public class AnaValues
    {
        private static int _dataSourceState;
        private static int _beatDataVersion;

        /// <summary>
        /// 心搏数据版本号(数据源每次变化时递增，用于分组等重计算结果的缓存失效判断)
        /// </summary>
        public static int BeatDataVersion
        {
            get { return _beatDataVersion; }
        }

        public static event Action<bool> OnDataSourceChanged;

        /// <summary>
        /// 当前操作者
        /// </summary>
        public static user CurrentUser;

        /// <summary>
        /// 当前待分析病例
        /// </summary>
        public static examCase CurrentCase;

        /// <summary>
        /// 当前血压详情
        /// </summary>
        public static List<bpDetail> CurrentBpDetails;

        /// <summary>
        /// 当前病例数据所在文件夹路径
        /// </summary>
        public static string RootPath;

        /// <summary>
        /// 主线程的同步上下文对象
        /// </summary>
        public static SynchronizationContext MainTreadContext;

        /// <summary>
        /// 数据源是否有变化(0:无变化; 1:有变化; 2:有变化且需重置界面显示)
        /// </summary>
        public static int DataSourceState
        {
            get => _dataSourceState;
            set
            {
                _dataSourceState = value;
                if (_dataSourceState > 0)
                {
                    //数据发生变化：递增版本号，使依赖心搏数据的缓存(分组/类型统计等)失效
                    Interlocked.Increment(ref _beatDataVersion);
                    OnDataSourceChanged?.Invoke(_dataSourceState == 1 ? false : true);
                }
            }
        }

        /// <summary>
        /// 当前是否正在加载或计算数据
        /// </summary>
        public static bool IsLoadingData { get; set; }

        /// <summary>
        /// 心搏数据缓存(提交到算法更新后同步更新此副本)
        /// </summary>
        public static List<BeatInfo> CachedAllBeats = new List<BeatInfo>();

        /// <summary>
        /// 与心搏相关的映射集(采样点-心拍序号)
        /// </summary>
        public static Dictionary<int, int> CurrentBeatMap = new Dictionary<int, int>();

        /// <summary>
        /// 界面上可见/可操作的心搏
        /// </summary>
        public static List<BeatInfo> AllValidBeats
        {
            get
            {
                return CachedAllBeats.FindAll(b => b.RRInterval > 0);
            }
        }


        /// <summary>
        /// 导联质量评分(数据质量从优到差的导联索引)
        /// </summary>
        public static List<int> LeadQualities;

        public static EcgDataReader HolterDataReader;

        public static List<AEcgHandler.SegmentInfo> UnanalyzableSegments = new List<AEcgHandler.SegmentInfo>();

        /// <summary>
        /// Holter配置实例
        /// </summary>
        public static HolterConfig holterConfig = new HolterConfig();

        /// <summary>
        /// 实例
        /// 片段图信息实例</summary>
        public static SegmentInfo segmentInfo = new SegmentInfo();

        /// <summary>
        /// 每分钟心率
        /// </summary>
        public static List<MinuteHRInfo> minuteHR = new List<MinuteHRInfo>();


        // 波形心搏属性颜色映射
        public static readonly Dictionary<char, string> WaveformColorMap = new Dictionary<char, string>
        {
            { 'N', "#000000" },
            { 'V', "#00AA00" },
            //{ "V2", "#00AA00" },
            //{ "V3", "#00AA00" },
            { 'J', "#00C8C8" },
            { 'S', "#C800C8" },
            //{ "S2", "#C800C8" },
            //{ "S3", "#C800C8" },
            { 'X', "#C8C800" },
            { 'E', "#008000" },
            { 'G', "#409c9c" },
            { 'W', "#800080" },
            { 'O', "#8080FF" },
            { 'P', "#FF8080" },
            { 'H', "#C880C8" },
            //{ 'F', "#D3D3D3" },
            { 'F', "#00AA66" },
            { 'C', "#000000" },
            { 'B', "#000000" },
            { 'K', "#000000" },
            { 'M', "#000000" },
            { 'T', "#00AA00" },
            { 'D', "#000000" },
            //{ "Ve", "#008000") },
            //{ "Je", "#409c9c") }
        };


        public static Dictionary<char, string> BeatTypes
        {
            get
            {
                bool bPace = holterConfig.AnalysisSetting.PaceAnalysisEnable;
                if (bPace)
                {
                    return PaceBeatTypes;
                }
                else
                {
                    return NoPaceBeatTypes;
                }
            }
        }

        public static readonly Dictionary<char, string> NoPaceBeatTypes = new Dictionary<char, string>
        {
            { 'N', "正常" },
            { 'V', "室性早搏" },
            { 'S', "房性早搏" },
            { 'J', "交界性早搏" },
            { 'X', "伪差" },
            { 'E', "室性逸搏" },
            { 'W', "房性逸搏" },
            { 'G', "交界性逸搏" },
            { 'F', "室性融合波" },
            { 'K', "房颤" },
            { 'M', "房扑" },
            { 'T', "R on T" }
        };

        public static readonly Dictionary<char, string> PaceBeatTypes = new Dictionary<char, string>
        {
            { 'N', "正常" },
            { 'V', "室性早搏" },
            { 'S', "房性早搏" },
            { 'J', "交界性早搏" },
            { 'X', "伪差" },
            { 'E', "室性逸搏" },
            { 'W', "房性逸搏" },
            { 'G', "交界性逸搏" },
            { 'O', "房性起搏" },
            { 'P', "室性起搏" },
            { 'H', "房室顺序起搏" },
            { 'F', "室性融合波" },
            { 'K', "房颤" },
            { 'M', "房扑" },
            { 'T', "R on T" }
        };

        public static readonly Dictionary<char, string> QRSBeatTypes = new Dictionary<char, string>
        {
            { 'N', "正常" },
            { 'V', "室性早搏" },
            { 'S', "房性早搏" },
            { 'J', "交界性早搏" },
            { 'X', "伪差" },
            { 'E', "室性逸搏" },
            { 'W', "房性逸搏" },
            { 'G', "交界性逸搏" },
            { 'F', "室性融合波" }
        };

        public static readonly Dictionary<string, string> heartbeatTypeMap = new Dictionary<string, string>
        {
            { "N", "N" },
            { "V", "V" },
            { "T", "R" },
            { "S", "S" },
            { "J", "J" },
            { "A", "Nca" },
            { "K", "Af" },
            { "D", "Vf" },
            { "M", "AF" },
            { "C", "Vf" },
            { "K/M", "Af" },
            { "D/C", "Vf" },
            { "O", "aP" },
            { "P", "vP" },
            { "H", "avP" },
            { "W", "aE" },
            { "E", "vE" },
            { "G", "jE" },
            { "F", "F" },
            { "X", "X" },
            { "Q","Q" }
        };

        public static Dictionary<string, int> LeadNameToIdxMap = new Dictionary<string, int>
        {
            { "I", 0 },
            { "II", 1 },
            { "III", 2 },
            { "aVR", 3 },
            { "aVL", 4 },
            { "aVF", 5 },
            { "V1", 6 },
            { "V2", 7 },
            { "V3", 8 },
            { "V4", 9 },
            { "V5", 10 },
            { "V6", 11 }
        };

        public static Dictionary<int, string> LeadIdxToNameMap = new Dictionary<int, string>
        {
            { 0, "I"},
            { 1, "II"},
            { 2, "III"},
            { 3, "aVR"},
            { 4, "aVL"},
            { 5, "aVF"},
            { 6, "V1"},
            { 7, "V2"},
            { 8, "V3"},
            { 9, "V4"},
            { 10,"V5"},
            { 11,"V6"}
        };

        public static readonly Dictionary<string, string> PacemakerType = new Dictionary<string, string>
        {
            { "DDD", "DDD"},
            { "VVI", "VVI"},
            { "DDDR", "DDDR"},
            { "VVIR", "VVIR"},
            { "AAIR", "AAIR"},
            { "AOO", "AOO"},
            { "VOO", "VOO"},
            { "AAI", "AAI"},
            { "AAT", "AAT"},
            { "VVT", "VVT"},
            { "VAT", "VAT"},
            { "VDD", "VDD"},
            { "DVI", "DVI"},
            { "ADD", "ADD"},
            { "DAI", "DAI"},
            { "DDI", "DDI"}
        };

        public static PersistDictionary<int> PaceMode = new PersistDictionary<int>
        {
            { 0, "单腔起搏"},
            { 1, "双腔起搏"},
            { 2, "三腔起搏"}
        };

        //心搏相关波形的显示布局形式
        public static readonly PersistDictionary<BeatWaveStyle> WaveLayouts = new PersistDictionary<BeatWaveStyle>(true)
        {
            { BeatWaveStyle.OneBeat, "单心搏"},
            { BeatWaveStyle.OneLead, "单导图"},
            { BeatWaveStyle.ThreeLeads, "三导图"},
            //{ BeatWaveStyle.OneLongLead, "单导长图"},
            //{ BeatWaveStyle.ThreeLongLeads, "三导长图"}
        };


        public static readonly PersistDictionary<BeatWaveStyle> EventWaveLayouts = new PersistDictionary<BeatWaveStyle>(true)
        {
            { BeatWaveStyle.OneBeat, "单导图"},
            { BeatWaveStyle.OneLead, "三导图"},
            //{ BeatWaveStyle.OneLongLead, "单导长图"},
            //{ BeatWaveStyle.ThreeLongLeads, "三导长图"}
        };

        public static readonly PersistDictionary<char> AfTypes = new PersistDictionary<char>
        {
            { 'K', "房颤" },
            { 'M', "房扑" }
        };

        public static readonly Dictionary<int, string> resortType = new Dictionary<int, string>
        {
            {0, "按时间排序" },
            {1, "按提前量排序" }
        };

        public static readonly string SegmentFileName = "holter-1.pic";
        public static readonly string SummaryFileName = "holter.params";
        public static readonly string DeleteEventFileName = "holter.custom";
        public static readonly string HRVAdviceFileName = "hrv.params";
        public static readonly string BPInfoFileName = "bp.params";
    }
}
