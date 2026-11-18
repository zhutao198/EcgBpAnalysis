using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    public class HourlistInfo
    {
        public string Time { get; set; } = "";
        public int TotalBeat { get; set; } = 0;

        public int MinHR { get; set; } = 0;
        public int AvgHR { get; set; } = 0;
        public int MaxHR { get; set; } = 0;

        public int SingleV { get; set; } = 0;
        public int PairedV { get; set; } = 0;
        public int TachycardiaV { get; set; } = 0;
        public int BigeminyV { get; set; } = 0;
        public int TrigeminyV { get; set; } = 0;
        public int TotalBeatV { get; set; } = 0;

        public int SingleS { get; set; } = 0;
        public int PairedS { get; set; } = 0;
        public int TachycardiaS { get; set; } = 0;
        public int BigeminyS { get; set; } = 0;
        public int TrigeminyS { get; set; } = 0;
        public int TotalBeatS { get; set; } = 0;

        public int Asystole { get; set; } = 0;
    }

    public static class CalcHourListInfo
    {
        private static List<HourlistInfo> s_hourDataList = new List<HourlistInfo>();
        private static object _lockObject = new object();
        private static AutoResetEvent _are = new AutoResetEvent(false);
        public static bool IsCalculating = false;

        public static List<HourlistInfo> HourDataList
        {
            get
            {
                //_are.WaitOne();
                lock (_lockObject)
                {
                    return s_hourDataList;
                }
            }
            set => s_hourDataList = value;
        }

        public static void CalculateHourlyData()
        {
            //_are.Reset();
            if (AnaValues.CachedAllBeats == null || AnaValues.CachedAllBeats.Count == 0)
            {
                return;
            }

            lock (_lockObject)
            {
                //Console.WriteLine("----CalculateHourlyData---");
                // 清空现有数据
                s_hourDataList.Clear();
                try
                {
                    List<BeatInfo> beatInfoList = AnaValues.CachedAllBeats;

                    //AnaEvent.GetFastestSlowestHrEvent(beatInfoList, AnaEvent.EventType.fastestHr);
                    //AnaEvent.GetFastestSlowestHrEvent(beatInfoList, AnaEvent.EventType.slowestHr);

                    int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;

                    List<DateTime> hourList = AnaEvent.HourTimeList;

                    //AnaEvent.GetEventByType(beatInfoList, "S");
                    //AnaEvent.GetEventByType(beatInfoList, "V");


                    for (int i = 0; i < hourList.Count; i++)
                    {
                        DateTime hourTime = AnaEvent.HourTimeList[i];
                        HourlistInfo hourInfo = new HourlistInfo();
                        if (i == 0 || hourTime.Hour == 0)
                        {
                            if (dateFormat == 0)
                            {
                                hourInfo.Time = $"{hourTime.Hour:00}:00 {hourTime.Month:00}-{hourTime.Day:00}";
                            }
                            else
                            {
                                hourInfo.Time = $"{hourTime.Hour:00}:00 {hourTime.Day:00}-{hourTime.Month:00}";
                            }
                        }
                        else
                        {
                            hourInfo.Time = $"{hourTime.Hour:00}:00";
                        }

                        // 计算当前小时的心搏数据
                        CalculateHourlyBeatMetrics(hourInfo, i);

                        s_hourDataList.Add(hourInfo);
                    }

                    int maxHR = s_hourDataList[0].MaxHR;
                    int minHR = s_hourDataList[0].MinHR;
                    HourlistInfo hourInfoTotal = new HourlistInfo();
                    hourInfoTotal.Time = CommonValues.Langs["总数"];
                    int avgHr = AnaEvent.CalcAvgHR(AnaValues.CachedAllBeats);
                    hourInfoTotal.TotalBeat = AnaEvent.TotalBeatCount;
                    for (int i = 0; i < s_hourDataList.Count; i++)
                    {
                        hourInfoTotal.SingleV += s_hourDataList[i].SingleV;
                        hourInfoTotal.PairedV += s_hourDataList[i].PairedV;
                        hourInfoTotal.TachycardiaV += s_hourDataList[i].TachycardiaV;
                        hourInfoTotal.BigeminyV += s_hourDataList[i].BigeminyV;
                        hourInfoTotal.TrigeminyV += s_hourDataList[i].TrigeminyV;
                        hourInfoTotal.TotalBeatV += s_hourDataList[i].TotalBeatV;
                        hourInfoTotal.SingleS += s_hourDataList[i].SingleS;
                        hourInfoTotal.PairedS += s_hourDataList[i].PairedS;
                        hourInfoTotal.TachycardiaS += s_hourDataList[i].TachycardiaS;
                        hourInfoTotal.BigeminyS += s_hourDataList[i].BigeminyS;
                        hourInfoTotal.TrigeminyS += s_hourDataList[i].TrigeminyS;
                        hourInfoTotal.TotalBeatS += s_hourDataList[i].TotalBeatS;
                        hourInfoTotal.Asystole += s_hourDataList[i].Asystole;
                        maxHR = Math.Max(maxHR, s_hourDataList[i].MaxHR);
                        if(s_hourDataList[i].MinHR == 0)
                        {
                            continue;
                        }
                        minHR = Math.Min(minHR, s_hourDataList[i].MinHR);
                    }
                    hourInfoTotal.MaxHR = maxHR;
                    hourInfoTotal.AvgHR = avgHr;
                    hourInfoTotal.MinHR = minHR;
                    s_hourDataList.Add(hourInfoTotal);
                }
                catch (Exception e)
                {
                    Logger.CreateLog(e, "小时数据表");
                }
                finally
                {
                    IsCalculating = false;
                }
            }
            //IsCalculating = false;
            //_are.Set();

            //Console.WriteLine("----CalculateHourlyData s_hourDataList.Count---"+ s_hourDataList.Count);
        }

        public static void CalculateHourlyBeatMetrics(HourlistInfo hourInfo, int hourIndex)
        {
            // 查找当前小时内的所有心搏
            if(hourIndex<0 || hourIndex > AnaEvent.AllHourlyBeats.Count-1)
            {
                return;
            }
            List<BeatInfo> hourlyBeats = AnaEvent.AllHourlyBeats[hourIndex];

            hourInfo.MaxHR = GetHourFastestLowestHr(hourIndex, "最快心率");
            hourInfo.MinHR = GetHourFastestLowestHr(hourIndex, "最慢心率");
            // 计算平均心率
            hourInfo.AvgHR = AnaEvent.CalcAvgHR(hourlyBeats);
            // 计算总心搏数
            hourInfo.TotalBeat = AnaEvent.TotalBeatCount;

            hourInfo.SingleV = CalcEventCount(hourIndex, "单发室早");
            hourInfo.PairedV = CalcEventCount(hourIndex, "成对室早");
            hourInfo.TachycardiaV = CalcEventCount(hourIndex, "室速");
            hourInfo.BigeminyV = CalcEventCount(hourIndex, "室性二联律");
            hourInfo.TrigeminyV = CalcEventCount(hourIndex, "室性三联律");
            hourInfo.TotalBeatV = AnaEvent.CalcBeatCount(hourlyBeats, 'V') + AnaEvent.CalcBeatCount(hourlyBeats, 'T');

            hourInfo.SingleS = CalcEventCount(hourIndex, "单发房早");
            hourInfo.PairedS = CalcEventCount(hourIndex, "成对房早");
            hourInfo.TachycardiaS = CalcEventCount(hourIndex, "房速");
            hourInfo.BigeminyS = CalcEventCount(hourIndex, "房性二联律");
            hourInfo.TrigeminyS = CalcEventCount(hourIndex, "房性三联律");
            hourInfo.TotalBeatS = AnaEvent.CalcBeatCount(hourlyBeats, 'S');

            hourInfo.Asystole = CalcEventCount(hourIndex, "停搏");
        }

        public static int GetHourFastestLowestHr(int hourIdx, string eventName)
        {
            if (AnaEvent.HourTimeList.Count == 0) return 0;
            List<int> hrList = new List<int>();
            
            DateTime hourTime = AnaEvent.HourTimeList[hourIdx];
            for (int i = 0; i < AnaEvent.AllEvents[eventName].Count; i++)
            {
                AnaEvent.EventInfo eventInfo = AnaEvent.AllEvents[eventName][i];
                DateTime eventTime = AnaCode.getDataTimeFromSample(eventInfo.StartSample);

                if (hourTime.Day == eventTime.Day && hourTime.Hour == eventTime.Hour)
                {
                    hrList.Add(eventInfo.AvgHr);
                }
            }

            int fastestLowestHr = 0;
            if (hrList.Count == 0) return fastestLowestHr;
            if(eventName == "最快心率")
            {
                fastestLowestHr = AnaEvent.AllEvents["最快心率"].Count > 0 ? AnaEvent.AllEvents["最快心率"][0].AvgHr : 0;
                fastestLowestHr = Math.Min(fastestLowestHr, hrList.Max());
            }
            else
            {
                fastestLowestHr = AnaEvent.AllEvents["最慢心率"].Count > 0 ? AnaEvent.AllEvents["最慢心率"][0].AvgHr : 0;
                fastestLowestHr = Math.Max(fastestLowestHr, hrList.Min());
            }
            return fastestLowestHr;
        }

        public static int CalcEventCount(int hourIdx, string eventName)
        {
            int count = 0;
            DateTime hourTime = AnaEvent.HourTimeList[hourIdx];
            for (int i = 0; i < AnaEvent.AllEvents[eventName].Count; i++)
            {
                AnaEvent.EventInfo eventInfo = AnaEvent.AllEvents[eventName][i];
                DateTime eventTime = AnaCode.getDataTimeFromSample(eventInfo.StartSample);

                if (hourTime.Day == eventTime.Day && hourTime.Hour == eventTime.Hour)
                {
                    ++count;
                }
            }
            return count;
        }

        public static void CalculateHeartRateMetrics(List<BeatInfo> beats, HourlistInfo hourInfo)
        {
            if (beats.Count < 2) return;

            int RRSum = 0;
            int RRCount = 0;

            // 计算每个心搏的心率（基于RR间期）
            for (int i = 1; i < beats.Count; i++)
            {
                if(AnaCode.IsInvalidBeat(beats[i]) || beats[i].RRInterval <= 0 || beats[i].BeatUiType == 'X')
                {
                    continue;
                }

                RRSum += beats[i].RRInterval;
                ++RRCount;
            }
            if (RRCount > 0)
            {
                int RRAvg = RRSum / RRCount;
                hourInfo.AvgHR = 60000 / RRAvg;
            }

        }
    }
}
