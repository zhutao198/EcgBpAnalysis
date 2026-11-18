using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zoncare.Common;
using static AnalysisApp.Commons.AEcgHandler;

namespace AnalysisApp.Commons
{
    public class HRVTimeDomainInfo
    {
        public string dataType { get; set; } = "";
        public string startTime { get; set; } = "";
        public string duration { get; set; } = "";
        public int start_position { get; set; } = 0;             ///< 分析的开始位置
        public int length_of_time { get; set; } = 0;             ///< 分析的时长
        public string nns { get; set; } = "";                        ///< NN间期个数
        public string meanNN { get; set; } = "";                    ///< 平均 NN 间期，单位：ms
        public string sdnn { get; set; } = "";                       ///< NN 间期的标准差, 正常值范围 141±39 ms

        public string sdnnIndex { get; set; } = "";                 ///< 5mins 段 NN 间期标准差的平均值
        public string sdann { get; set; } = "";                      ///< 5 mins 段 NN 间期平均值的标准差, 正常值范围 127±35 ms

        public string sdsd { get; set; } = "";                       ///< 相邻 NN 间期之差的标准差，单位 ms
        public string nn50Count { get; set; } = "";                 ///< 相邻 RR 间期差值 ＞ 50ms 的个数
        public string pnn50 { get; set; } = "";                    ///< 相邻RR间期差值＞50ms占总体的百分比
        public string rmssd { get; set; } = "";                      ///< 相邻 NN 间期差的均方根, 正常值范围 27±12 ms

        public string hrvTriangularIndex { get; set; } = "";     ///< 三角指数, 正常值范围 37±15
    }

    public class HRVFreqDomainInfo
    {
        public string dataType { get; set; } = "";
        public string startTime { get; set; } = "";
        public string duration { get; set; } = "";
        public string total_power { get; set; } = "";  ///< 该时间段内 NN 间期变化的总功率, 正常值范围 3466±1018，单位 ms2
        public string vlf { get; set; } = "";  ///< 超低频段（0.003-0.04 Hz）功率，单位 ms2
        public string hf { get; set; } = "";  ///< 高频段（0.15~0.4 Hz）功率, 正常值范围 975±203，单位 ms2
        public string lf { get; set; } = "";    ///< 低频段（0.04~0.15 Hz）功率,  正常值范围 1170±416，单位 ms2

        public int start_position { get; set; } = 0;             ///< 分析的开始位置
        public int length_of_time { get; set; } = 0;             ///< 分析的时长
        public string mean_nn { get; set; } = "";                    ///< 平均 NN 间期，单位：ms

        public string ulf { get; set; } = "";  ///< 极低频段（≤ 0.003 Hz）功率，单位 ms2
        public string lf_normal { get; set; } = "";  ///< 归一化低频段功率： LF/(total power-VLF)*100, 正常值范围 54±4
        public string hf_normal { get; set; } = "";     ///< 归一化高频段功率:HF/(total power-VLF)*100, 正常值范围 29±3
        public string ratio_lf_hf { get; set; } = "";     ///< 正常值范围 1.5-2.0
    }

    public static class HRVCommonMethod
    {
        public static List<HRVTimeDomainInfo> GetHRVTimeDomainList()
        {
            int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;

            List<HRVTimeDomainInfo> hrvTimeDomainList = new List<HRVTimeDomainInfo>();

            List<AEcgHrvTimeDomainInfo> aEcgHrvTimeDomainList = GetHrvTimeDomain();
            for (int i = 0; i < aEcgHrvTimeDomainList.Count; i++)
            {
                AEcgHrvTimeDomainInfo aEcgHrvTimeDomainInfo = aEcgHrvTimeDomainList[i];
                HRVTimeDomainInfo hrvTimeDomainInfo = new HRVTimeDomainInfo();
                if (i == 0)
                {
                    hrvTimeDomainInfo.dataType = "全部";

                }
                else if (i == 1)
                {
                    hrvTimeDomainInfo.dataType = "白天";
                }
                else if (i == 2)
                {
                    hrvTimeDomainInfo.dataType = "夜间";
                }
                else
                {
                    hrvTimeDomainInfo.dataType = "小时";
                }
                if (dateFormat == 0)
                {
                    hrvTimeDomainInfo.startTime = AnaCode.getDataTimeFromSample(aEcgHrvTimeDomainInfo.start_position).ToString("MM-dd HH:mm");
                }
                else
                {
                    hrvTimeDomainInfo.startTime = AnaCode.getDataTimeFromSample(aEcgHrvTimeDomainInfo.start_position).ToString("dd-MM HH:mm");
                }


                double totalSeconds = (double)aEcgHrvTimeDomainInfo.length_of_time / AnaValues.holterConfig.SampleRate;
                TimeSpan totalTime = TimeSpan.FromSeconds(totalSeconds);
                hrvTimeDomainInfo.duration = (int)totalTime.TotalHours >= 1 ? $"{((int)totalTime.TotalHours).ToString()}h" : $"{((int)totalTime.TotalMinutes).ToString()}min";
                hrvTimeDomainInfo.nns = aEcgHrvTimeDomainInfo.nns == -100 ? "-" : aEcgHrvTimeDomainInfo.nns.ToString();
                hrvTimeDomainInfo.sdnn = aEcgHrvTimeDomainInfo.sdnn == -100 ? "-" : aEcgHrvTimeDomainInfo.sdnn.ToString();
                hrvTimeDomainInfo.hrvTriangularIndex = aEcgHrvTimeDomainInfo.hrv_triangular_index == -100f ? "-" : (Math.Round((double)aEcgHrvTimeDomainInfo.hrv_triangular_index, 2)).ToString();
                hrvTimeDomainInfo.pnn50 = aEcgHrvTimeDomainInfo.pnn50 == -100f ? "-" : (Math.Round((double)aEcgHrvTimeDomainInfo.pnn50, 2)).ToString();
                hrvTimeDomainInfo.sdsd = aEcgHrvTimeDomainInfo.sdsd == -100 ? "-" : aEcgHrvTimeDomainInfo.sdsd.ToString();
                hrvTimeDomainInfo.sdnnIndex = aEcgHrvTimeDomainInfo.sdnn_index == -100 ? "-" : aEcgHrvTimeDomainInfo.sdnn_index.ToString();
                hrvTimeDomainInfo.sdann = aEcgHrvTimeDomainInfo.sdann == -100 ? "-" : aEcgHrvTimeDomainInfo.sdann.ToString();
                hrvTimeDomainInfo.rmssd = aEcgHrvTimeDomainInfo.rmssd == -100 ? "-" : aEcgHrvTimeDomainInfo.rmssd.ToString();
                hrvTimeDomainInfo.nn50Count = aEcgHrvTimeDomainInfo.nn50_count == -100 ? "-" : aEcgHrvTimeDomainInfo.nn50_count.ToString();
                hrvTimeDomainInfo.meanNN = aEcgHrvTimeDomainInfo.mean_nn == -100 ? "-" : aEcgHrvTimeDomainInfo.mean_nn.ToString();
                hrvTimeDomainInfo.start_position = aEcgHrvTimeDomainInfo.start_position;
                hrvTimeDomainInfo.length_of_time = aEcgHrvTimeDomainInfo.length_of_time;

                hrvTimeDomainList.Add(hrvTimeDomainInfo);

            }
            return hrvTimeDomainList;
        }

        public static List<HRVFreqDomainInfo> GetHrvFreqDomainList()
        {
            int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;

            List<HRVFreqDomainInfo> hrvFreqDomainList = new List<HRVFreqDomainInfo>();

            List<AEcgHandler.AEcgHrvFreqDomainInfo> aEcgHrvFreqDomainList = AEcgHandler.GetHrvFreqDomain();

            for (int i = 0; i < aEcgHrvFreqDomainList.Count; i++)
            {
                AEcgHandler.AEcgHrvFreqDomainInfo aEcgHrvFreqDomainInfo = aEcgHrvFreqDomainList[i];
                HRVFreqDomainInfo hrvFreqDomainInfo = new HRVFreqDomainInfo();
                if (i == 0)
                {
                    hrvFreqDomainInfo.dataType = "全部";
                    hrvFreqDomainInfo.duration = "-";
                }
                else
                {
                    hrvFreqDomainInfo.dataType = "小时";

                    double durationHour = (double)aEcgHrvFreqDomainInfo.length_of_time / AnaValues.holterConfig.SampleRate / 3600;
                    hrvFreqDomainInfo.duration = $"{(int)(durationHour * 60)}min";
                }
                if (dateFormat == 0)
                {
                    hrvFreqDomainInfo.startTime = AnaCode.getDataTimeFromSample(aEcgHrvFreqDomainInfo.start_position).ToString("MM-dd HH:mm");
                }
                else
                {
                    hrvFreqDomainInfo.startTime = AnaCode.getDataTimeFromSample(aEcgHrvFreqDomainInfo.start_position).ToString("dd-MM HH:mm");
                }
                hrvFreqDomainInfo.mean_nn = aEcgHrvFreqDomainInfo.mean_nn == -100 ? "-" : aEcgHrvFreqDomainInfo.mean_nn.ToString();
                hrvFreqDomainInfo.vlf = aEcgHrvFreqDomainInfo.vlf == -100f ? "-" : Math.Round((double)aEcgHrvFreqDomainInfo.vlf, 2).ToString();
                hrvFreqDomainInfo.ulf = aEcgHrvFreqDomainInfo.ulf == -100f ? "-" : Math.Round((double)aEcgHrvFreqDomainInfo.ulf, 2).ToString();
                hrvFreqDomainInfo.lf = aEcgHrvFreqDomainInfo.lf == -100f ? "-" : Math.Round((double)aEcgHrvFreqDomainInfo.lf, 2).ToString();
                hrvFreqDomainInfo.hf = aEcgHrvFreqDomainInfo.hf == -100f ? "-" : Math.Round((double)aEcgHrvFreqDomainInfo.hf, 2).ToString();
                hrvFreqDomainInfo.lf_normal = aEcgHrvFreqDomainInfo.lf_normal == -100f ? "-" : Math.Round((double)aEcgHrvFreqDomainInfo.lf_normal, 2).ToString();
                hrvFreqDomainInfo.hf_normal = aEcgHrvFreqDomainInfo.hf_normal == -100f ? "-" : Math.Round((double)aEcgHrvFreqDomainInfo.hf_normal, 2).ToString();
                hrvFreqDomainInfo.ratio_lf_hf = aEcgHrvFreqDomainInfo.ratio_lf_hf == -100f ? "-" : Math.Round((double)aEcgHrvFreqDomainInfo.ratio_lf_hf, 2).ToString();
                hrvFreqDomainInfo.total_power = aEcgHrvFreqDomainInfo.total_power == -100f ? "-" : Math.Round((double)aEcgHrvFreqDomainInfo.total_power, 2).ToString();

                hrvFreqDomainList.Add(hrvFreqDomainInfo);
            }
            return hrvFreqDomainList;
        }

        public static List<double> GetRRPlotInfo(int type)
        {
            int dayStart = 6;
            int dayEnd = 22;
            const double axis = 2;
            const double tick = 0.01;
            const int fixedDecimal = 2;

            // 创建分布图数组
            int plotLength = (int)(axis / tick) + 1;
            List<double> plot = new List<double>(plotLength);
            for (int i = 0; i < plotLength; i++)
            {
                plot.Add(0);
            }

            // 获取散点数据
            List<BeatInfo> beats = AnaValues.CachedAllBeats;

            // 填充数据
            foreach (var beat in beats)
            {
                int hour = AnaCode.getDataTimeFromSample(beat.Sample).Hour;

                if (type == 1)
                {
                    if (hour < dayStart || hour >= dayEnd)
                    {
                        continue;
                    }
                }
                else if (type == 2)
                {
                    if (hour >= dayStart && hour < dayEnd)
                    {
                        continue;
                    }
                }
                double val = beat.RRInterval / 1000.0;
                int index;

                if (Math.Abs(val) > axis)
                {
                    index = val < 0 ? 0 : plot.Count - 1;
                }
                else
                {
                    index = (int)Math.Floor(Math.Round(val / tick, 5));
                    // 确保索引在有效范围内
                    index = Math.Max(0, Math.Min(index, plot.Count - 1));
                }

                plot[index]++;
            }

            // 处理最后一个元素
            var lastItem = plot[plot.Count - 1];
            plot.RemoveAt(plot.Count - 1);

            if (plot.Count > 0)
            {
                plot[plot.Count - 1] += lastItem;
            }

            return plot;
        }


        public static (double Mean, double StdDev) CalculateMeanStdDev(List<Double> arr)
        {
            if (arr == null || arr.Count == 0)
                throw new ArgumentException("数组不能为空", nameof(arr));

            // 计算均值
            double mean = arr.Sum() / arr.Count;

            // 计算平方差
            var squaredDiffs = arr.Select(val => Math.Pow(val - mean, 2)).ToArray();

            // 计算方差
            double variance = squaredDiffs.Sum() / arr.Count;

            // 计算标准差
            double stdDev = Math.Sqrt(variance);

            return (mean, stdDev);
        }

        public static (List<List<AEcgHrvPsdInfo>>, int, int ) GetHrvPsdList(List<HRVFreqDomainInfo> hrvFreqDomainInfoList)
        {
            List<List<AEcgHrvPsdInfo>> hrvPsdInfoList = new List<List<AEcgHrvPsdInfo>>();

            double minValue = 0;
            double maxValue = 0;
            int maxIndex = 0;
            int minIndex = 0;
            for (int i=0; i< hrvFreqDomainInfoList.Count; i++)
            {
                HRVFreqDomainInfo hrvFreqDomainInfo = hrvFreqDomainInfoList[i];
                if (hrvFreqDomainInfo.lf == "-" || hrvFreqDomainInfo.hf == "-") continue;
                double lfhf = Math.Round(Convert.ToDouble(hrvFreqDomainInfo.lf) / Convert.ToDouble(hrvFreqDomainInfo.hf), 2);
                if(i==0)
                {
                    minValue = lfhf;
                    minIndex = 0;
                    maxValue = lfhf;
                    maxIndex = 0;
                }
                else
                {
                    if(lfhf < minValue)
                    {
                        minValue = lfhf;
                        minIndex = i;
                    }
                    if(lfhf > maxValue)
                    {
                        maxValue = lfhf;
                        maxIndex = i;
                    }
                }

            }

            List<AEcgHrvPsdInfo> maxList = AEcgHandler.GetHrvPsd(maxIndex);
            List<double> maxDataYList = new List<double>();
            for (int i = 0; i < maxList.Count; i++)
            {
                maxDataYList.Add(maxList[i].DataY);
            }
            var (meanMax, stdDevMax) = CalculateMeanStdDev(maxDataYList);
            List<AEcgHrvPsdInfo> maxFinalList = new List<AEcgHrvPsdInfo>();
            for (int i = 0; i < maxList.Count; i++)
            {
                if (maxList[i].DataY < 3 * stdDevMax)
                {
                    maxFinalList.Add(maxList[i]);
                }
            }

            List<AEcgHrvPsdInfo> minList = AEcgHandler.GetHrvPsd(minIndex);
            List<double> minDataYList = new List<double>();
            for (int i = 0; i < minList.Count; i++)
            {
                minDataYList.Add(minList[i].DataY);
            }
            var (meanMin, stdDevMin) = CalculateMeanStdDev(minDataYList);
            List<AEcgHrvPsdInfo> minFinalList = new List<AEcgHrvPsdInfo>();
            for (int i = 0; i < minList.Count; i++)
            {
                if (minList[i].DataY < 3 * stdDevMin)
                {
                    minFinalList.Add(minList[i]);
                }
            }

            hrvPsdInfoList.Add(maxFinalList);
            hrvPsdInfoList.Add(minFinalList);


            return (hrvPsdInfoList,maxIndex,minIndex);
        }
    }
}
