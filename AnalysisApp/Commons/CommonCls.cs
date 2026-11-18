using Newtonsoft.Json;
using System;

namespace AnalysisApp.Commons
{
    /// <summary>
    /// 心搏相关波形的显示样式
    /// </summary>
    public enum BeatWaveStyle
    {
        OneBeat = 1,
        OneLead = 2,
        OneLongLead = 3,
        ThreeLeads = 4,
        ThreeLongLeads = 5
    }

    /// <summary>
    /// 散点图类型(用于工具栏)
    /// </summary>
    public class ScatterType
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 相关心搏属性特征
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 提示或描述性文本
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 相关(心搏)数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 图标下方的文本
        /// </summary>
        public string Footer { get; set; }
    }

    /// <summary>
    /// 直方图类型
    /// </summary>
    public enum HistogramType
    {
        RRInterval, //RR间期
        AverageHR,  //平均心率
        EarlyRate,  //房早、室早、交界早提早率
        RRIntervalRatio, //间期比率
        Other
    }

    /// <summary>
    /// 心电数据更新的类型
    /// </summary>
    public enum EcgUpdateType {
        AddBeats,
        DeleteBeats,
        ModifyBeats,
        AddAf,
        CorrectLead,
        RangeReanalyze,
        DeleteAF,
        DetectAF,
        AddSTEvent,
        DeleteSTEvent,
        SetAsUnanalyzable,
        SetAsAnalyzable,
        AnalyzeST,
        Other
    }

    /// <summary>
    /// 片段的范围
    /// </summary>
    public struct SegmentRange
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        [JsonIgnore]
        public static SegmentRange Empty { get => new SegmentRange(0, 0); }

        public SegmentRange(int start, int end)
        {
            StartIndex = start;
            EndIndex = end;
        }
    }

    /// <summary>
    /// 区间范围
    /// </summary>
    public class GroupInterval
    {
        public int start { get; set; }
        public int end { get; set; }

        public GroupInterval(int startIndex, int endIndex)
        {
            start = startIndex;
            end = endIndex;
        }
    }

    /// <summary>
    /// QRS波相关信息
    /// </summary>
    public struct QrsInfo
    {
        public int Start { get; set; }
        public int Peak { get; set; } //波峰位置(采样点序号)
        public int End { get; set; }
    }

    /// <summary>
    /// 波形数据及状态信息
    /// </summary>
    public class WaveInfo
    {
        public int leadIndex { get; set; }
        public int DataLength { get; set; }
        public short[] WaveData { get; set; }
        public float[] WaveDataF { get; set; }
        public int[] StatusData { get; set; }
    }

    /// <summary>
    /// 房颤或房扑(室颤/室扑)的相关信息
    /// </summary>
    public class AfInfo
    {
        /// <summary>
        /// 相关心搏类型
        /// </summary>
        public char RefBeatType { get; set; }
        /// <summary>
        /// 颤动或扑动类型名称
        /// </summary>
        public string TypeName { get; set; }
        public int StartSample { get; set; }
        public int EndSample { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// 片段的范围
    /// </summary>
    public struct STEventRange
    {
        public short LeadIndex { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        [JsonIgnore]
        public static STEventRange Empty { get => new STEventRange(0, 0, 0); }

        public STEventRange(short lead, int start, int end)
        {
            LeadIndex = lead;
            StartIndex = start;
            EndIndex = end;
        }
    }

    /// <summary>
    /// 直方图相关信息
    /// </summary>
    public class HistogramInfo
    {
        /// <summary>
        /// 直方图类型
        /// </summary>
        public HistogramType ChartType { get; set; } = HistogramType.Other;

        /// <summary>
        /// 直方图类型唯一标识
        /// </summary>
        public string TypeId { get; set; }
        /// <summary>
        /// 类型描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 心搏属性标识
        /// </summary>
        public string BeatsTypeCode { get; set; }
        /// <summary>
        /// 是否存在当前直方图类型的心搏
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// X轴最大值
        /// </summary>
        public int MaxScaleX { get; set; } = 100;
        /// <summary>
        /// Y轴最大值
        /// </summary>
        public int MaxScaleY { get; set; } = 100;

        /// <summary>
        /// X轴标题
        /// </summary>
        public string AxisXTitle { get; set; }
        public string AxisX2Title { get; set; }
        /// <summary>
        /// Y轴标题
        /// </summary>
        public string AxisYTitle { get; set; }
        public string AxisY2Title { get; set; }

        /// <summary>
        /// X轴刻度的间隔
        /// </summary>
        public int IntervalX { get; set; } = 10;
        /// <summary>
        /// Y轴刻度的间隔
        /// </summary>
        public int IntervalY { get; set; } = 10;

        /// <summary>
        /// 刻度间细分的段数
        /// </summary>
        public int SubdivisionCount { get; set; } = 4;

        public HistogramInfo(HistogramType chartType)
        {
            ChartType = chartType;
        }
    }

    public class HRVReportInfo
    {
        public string sdnn { get; set; } = "";                       ///< NN 间期的标准差, 正常值范围 141±39 ms
        public string sdann { get; set; } = "";                      ///< 5 mins 段 NN 间期平均值的标准差, 正常值范围 127±35 ms

        public string sdnnIndex { get; set; } = "";                 ///< 5mins 段 NN 间期标准差的平均值

        public string pnn50 { get; set; } = "";                    ///< 相邻RR间期差值＞50ms占总体的百分比
        public string rmssd { get; set; } = "";                      ///< 相邻 NN 间期差的均方根, 正常值范围 27±12 ms

        public string hrvTriangularIndex { get; set; } = "";     ///< 三角指数, 正常值范围 37±15

        public string total_power { get; set; } = "";  ///< 该时间段内 NN 间期变化的总功率, 正常值范围 3466±1018，单位 ms2

        public string ulf { get; set; } = "";  ///< 极低频段（≤ 0.003 Hz）功率，单位 ms2
        public string vlf { get; set; } = "";  ///< 超低频段（0.003-0.04 Hz）功率，单位 ms2
        public string lf { get; set; } = "";    ///< 低频段（0.04~0.15 Hz）功率,  正常值范围 1170±416，单位 ms2

        public string hf { get; set; } = "";  ///< 高频段（0.15~0.4 Hz）功率, 正常值范围 975±203，单位 ms2
    }

    public class STReportInfo
    {
        public int stLowCount { get; set; } = 0;
        public string stLowest { get; set; } = "";
        public string stLowestTime { get; set; } = "";
        public string stLowestTimeLong { get; set; } = "";
        public string stLowestLead { get; set; } = "";
        public string stLowLeadDuration { get; set; } = "";
        public int stUpCount { get; set; } = 0;
        public string stUpest { get; set; } = "";
        public string stUpestTime { get; set; } = "";
        public string stUpestTimeLong { get; set; } = "";
        public string stUpestLead { get; set; } = "";
        public string stUpLeadDuration { get; set; } = "";
        public string stTotalDuration { get; set; } = "";
    }

    public class AFReportInfo
    {
        public string AFTotalDuration { get; set; } = "";
        public string AFPercent { get; set; } = "";
        public string AFDuration { get; set; } = "";
        public int AFCount { get; set; } = 0;
        public string AFPercent2 { get; set; } = "";
        public string AFDuration2 { get; set; } = "";
        public int AFCount2 { get; set; } = 0;
    }

    public class PaceReportInfo
    {
        public int APCount { get; set; } = 0;
        public int VPCount { get; set; } = 0;
        public int AVPCount { get; set; } = 0;
        public int TotalCount { get; set; } = 0;
        public double TotalPercent { get; set; } = 0d;
        public double APPercent { get; set; } = 0d;
        public double VPPercent { get; set; } = 0d;
        public double AVPPercent { get; set; } = 0d;
    }

    public struct TemplateFunctionInfo
    {
        public static readonly TemplateFunctionInfo Empty = new TemplateFunctionInfo("", "", "");
        public string Label { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }

        public TemplateFunctionInfo(string label, string value, string unit)
        {
            this.Label = label;
            this.Value = value;
            this.Unit = unit;
        }
    }
}
