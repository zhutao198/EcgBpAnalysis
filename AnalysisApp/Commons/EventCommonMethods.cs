using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zoncare.Common;
using Zoncare.DAL;
using static AnalysisApp.Commons.AEcgHandler;

namespace AnalysisApp.Commons
{
    public static class AnaEvent
    {
        public enum EventType
        {
            tachycardia,    // 默认值为 0 心动过速
            bradycardia,     // 1 心动过缓
            fastestHr,      //最快心率
            slowestHr,      //最慢心率
            longRR,         //长RR间期
            longestRR,      //最长RR间期
            asystole,       //停搏
        }

        public class EventInfo
        {
            public int BeatSize { get; set; } = 0;
            public double Duration { get; set; } = 0f;
            public string Type { get; set; } = "";
            public string StartTime { get; set; } = "";
            public int StartSample { get; set; } = 0;
            public int Sample { get; set; } = 0;
            public int EndSample { get; set; } = 0;
            public List<int> Samples { get; set; } = new List<int>();
            public List<int> EventSamples { get; set; } = new List<int>();
            public int AvgHr { get; set; } = 0;
            public int AvgRR { get; set; } = 0;
            public bool HasPic { get; set; } = false;
            //public bool Deleted { get; set; } = false;
            //st事件独有
            public float STPointVoltage { get; set; } = 0f;
            public int STLead { get; set; } = 0;
            public int STStatusFlag { get; set; } = 0;
            public int STSlope { get; set; } = 0;
            public int MinHr { get; set; } = 0;
            public int MaxHr { get; set; } = 0;
            //房早未下传独有
            public int Offset { get; set; } = 0;

        }

        public static ecgSettingValues EcgSets;
        //public static string BeatSequence = "";

        public static readonly Dictionary<string, List<EventInfo>> AllEvents = new Dictionary<string, List<EventInfo>>()
        {
            {"ST", new List<EventInfo>()},
            {"停搏", new List<EventInfo>()},
            {"长RR间期", new List<EventInfo>()},
            {"单发房早", new List<EventInfo>()},
            {"成对房早", new List<EventInfo>()},
            {"房性二联律", new List<EventInfo>()},
            {"房性三联律", new List<EventInfo>()},
            {"房速", new List<EventInfo>()},
            {"单发交界早", new List<EventInfo>()},
            {"成对交界早", new List<EventInfo>()},
            {"交界二联律", new List<EventInfo>()},
            {"交界三联律", new List<EventInfo>()},
            {"交界速", new List<EventInfo>()},
            {"单发室早", new List<EventInfo>()},
            {"R on T", new List<EventInfo>()},
            {"成对室早", new List<EventInfo>()},
            {"室性二联律", new List<EventInfo>()},
            {"室性三联律", new List<EventInfo>()},
            {"室速", new List<EventInfo>()},
            {"最快心率", new List<EventInfo>()},
            {"最慢心率", new List<EventInfo>()},
            {"最长RR间期", new List<EventInfo>()},
            {"心动过速", new List<EventInfo>()},
            {"心动过缓", new List<EventInfo>()},
            {"房性逸搏", new List<EventInfo>()},
            {"交界性逸搏", new List<EventInfo>()},
            {"室性逸搏", new List<EventInfo>()},
            {"房早未下传", new List<EventInfo>()},
            {"室性融合波", new List<EventInfo>()},
        };

        public static string SegmentFilePath = "";
        public static string DeletedEventsFilePath = "";
        public static string UpdateEvent = "";


        //public static bool[] validBeatPositions;
        public static int TotalBeatCount = 0;
        public static int TotalAvgHR = 0;
        public static bool IsCalculating = true;
        public static List<List<BeatInfo>> AllHourlyBeats = new List<List<BeatInfo>>();
        public static List<DateTime> HourTimeList;

        public async static Task CalcEventInfo()
        {
            while (AnaCode.IsAnalysisST)
            {
                await Task.Delay(10);
            }

            List<BeatInfo> beatInfoList = AnaValues.CachedAllBeats;
            try
            {
                InitBasicData(beatInfoList);
                if (UpdateEvent == "")
                {
                    ResetAllEvents();
                    GetLongRRAndAsystoleEvent(beatInfoList, EventType.asystole);
                    GetSTEvent();
                    GetEventByType(beatInfoList, "S");
                    GetEventByType(beatInfoList, "V");
                    GetEventByType(beatInfoList, "J");
                    GetLongRRAndAsystoleEvent(beatInfoList, EventType.longRR);
                    GetLongestRREvent(beatInfoList, EventType.longestRR);
                    GetFastestSlowestHrEvent(beatInfoList, EventType.fastestHr);
                    GetFastestSlowestHrEvent(beatInfoList, EventType.slowestHr);
                    GetTachycardiaBradycardiaEvent(beatInfoList, EventType.tachycardia);
                    GetTachycardiaBradycardiaEvent(beatInfoList, EventType.bradycardia);
                    ReadNcaOrFBeat("房早未下传");
                    ReadNcaOrFBeat("室性融合波");
                    GetEventByType(beatInfoList, "W");
                    GetEventByType(beatInfoList, "E");
                    GetEventByType(beatInfoList, "G");
                    GetEventByType(beatInfoList, "T");
                }
                else
                {
                    switch (UpdateEvent)
                    {
                        case "ST":
                            GetSTEvent();
                            break;
                        case "停搏":
                        case "长RR间期":
                        case "最长RR间期":
                            GetLongestRREvent(beatInfoList, EventType.longestRR);
                            GetLongRRAndAsystoleEvent(beatInfoList, EventType.asystole);
                            GetLongRRAndAsystoleEvent(beatInfoList, EventType.longRR);
                            break;
                        case "单发房早":
                        case "成对房早":
                        case "房性二联律":
                        case "房性三联律":
                        case "房速":
                            GetEventByType(beatInfoList, "S");
                            break;
                        case "单发交界早":
                        case "成对交界早":
                        case "交界性二联律":
                        case "交界性三联律":
                        case "交界速":
                            GetEventByType(beatInfoList, "J");
                            break;
                        case "单发室早":
                        case "成对室早":
                        case "室性二联律":
                        case "室性三联律":
                        case "室速":
                            GetEventByType(beatInfoList, "V");
                            break;
                        case "R on T":
                            GetEventByType(beatInfoList, "T");
                            break;
                        case "最快心率":
                            GetFastestSlowestHrEvent(beatInfoList, EventType.fastestHr);
                            break;
                        case "最慢心率":
                            GetFastestSlowestHrEvent(beatInfoList, EventType.slowestHr);
                            break;
                        case "心动过速":
                            GetTachycardiaBradycardiaEvent(beatInfoList, EventType.tachycardia);
                            break;
                        case "心动过缓":
                            GetTachycardiaBradycardiaEvent(beatInfoList, EventType.bradycardia);
                            break;
                        case "房早未下传":
                            ReadNcaOrFBeat("房早未下传");
                            break;
                        case "室性融合波":
                            ReadNcaOrFBeat("室性融合波");
                            break;
                        case "房性逸搏":
                            GetEventByType(beatInfoList, "W");
                            break;
                        case "室性逸搏":
                            GetEventByType(beatInfoList, "E");
                            break;
                        case "交界性逸搏":
                            GetEventByType(beatInfoList, "G");
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "事件");
            }

            IsCalculating = false;
            UpdateEvent = "";
        }

        public static void ResetAllEvents()
        {
            foreach (var key in AllEvents.Keys.ToList())
            {
                AllEvents[key] = new List<EventInfo>();
            }
        }

        public static List<DateTime> GetHourList()
        {
            if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
            {
                startTime = DateTime.Today;
            }
            // 获取所有相关的日期+整点时间
            double totalSeconds = (double)AnaValues.holterConfig.Total / AnaValues.holterConfig.SampleRate;
            DateTime endTime = startTime.AddSeconds(totalSeconds);
            List<DateTime> hourList = new List<DateTime>();
            DateTime curTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);
            while (curTime < endTime)
            {
                hourList.Add(curTime);
                curTime = curTime.AddHours(1);
            }

            return hourList;
        }

        public static int CalcAvgHR(List<BeatInfo> beatInfoList, bool bFlag = true)
        {
            int avgHR = 0;
            int totalRR = 0;
            int RRCount = 0;
            int fastestHrValue = AnaCode.GetHREventValue("最快心率");
            int slowestHrValue = AnaCode.GetHREventValue("最慢心率");
            for (int i = 0; i < beatInfoList.Count; i++)
            {
                if (beatInfoList[i].BeatUiType == 'X')
                {
                    continue;
                }
                if (bFlag)
                {
                    if (fastestHrValue > 0 && 60000 / beatInfoList[i].RRInterval > fastestHrValue)
                    {
                        continue;
                    }
                    if (slowestHrValue > 0 && 60000 / beatInfoList[i].RRInterval < slowestHrValue)
                    {
                        continue;
                    }
                }
                totalRR += beatInfoList[i].RRInterval;
                ++RRCount;
            }
            if (RRCount > 0)
            {
                int RRAvg = totalRR / RRCount;
                avgHR = 60000 / RRAvg;
                TotalBeatCount = RRCount;
                TotalAvgHR = avgHR;
            }

            return avgHR;
        }

        public static int CalcBeatCount(List<BeatInfo> beatInfoList, char beatType)
        {

            int count = 0;
            for (int i = 0; i < beatInfoList.Count; i++)
            {
                bool overlaps = AnaCode.IsInvalidBeatEx(beatInfoList[i]);
                if (overlaps)
                {
                    continue;
                }
                if (beatInfoList[i].BeatUiType == 'X')
                {
                    continue;
                }
                if (beatInfoList[i].BeatUiType == beatType)
                {
                    ++count;
                }
            }

            return count;
        }

        /// <summary>
        /// 计算每小时心搏信息
        /// </summary>
        public static void CalcHourlyBeats()
        {

            //Console.WriteLine("----CalcHourlyBeats----");
            AllHourlyBeats.Clear();
            try
            {
                ConcurrentBag<KeyValuePair<DateTime, List<BeatInfo>>> hourlyData = new ConcurrentBag<KeyValuePair<DateTime, List<BeatInfo>>>();

                Parallel.ForEach(HourTimeList, hourTime =>
                {
                    // 查找当前小时内的所有心搏
                    int capacity = (int)((double)AnaValues.CachedAllBeats.Count / (HourTimeList.Count > 1 ? HourTimeList.Count - 1 : 1));
                    List<BeatInfo> oneHourlyBeats = new List<BeatInfo>(capacity);
                    for (int i = 0; i < AnaValues.CachedAllBeats.Count; i++)
                    {
                        BeatInfo beat = AnaValues.CachedAllBeats[i];
                        int sample = beat.Sample;
                        DateTime sampleTime = AnaCode.getDataTimeFromSample(sample);
                        if (sampleTime.Day == hourTime.Day && sampleTime.Hour == hourTime.Hour)
                        {
                            oneHourlyBeats.Add(beat);
                        }
                    }
                    hourlyData.Add(new KeyValuePair<DateTime, List<BeatInfo>>(hourTime, oneHourlyBeats));
                });
                AllHourlyBeats = hourlyData.OrderBy(o => o.Key).Select(o => o.Value).ToList();
            }
            catch (Exception e)
            {
                Logger.CreateLog(e, "小时数据表");
            }
            //Console.WriteLine("----CalcHourlyBeats end----");
        }

        public static void InitBasicData(List<BeatInfo> beatInfoList)
        {
            SegmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            DeletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);

            EcgSets = CommonValues.SystemSettings.ecgSets;

            //AllEvents.Clear();

            //// 获取心搏序列（基础数据）
            //string beatSequence = GetBeatSequence(beatInfoList);
            //// 预计算不可分析心搏的位置和有效心搏位置
            //validBeatPositions = PrecomputeValidBeatPositions(beatInfoList, beatSequence, EcgSets.customSets.allowEctopicBeat);
        }

        public static void GetSTEvent()
        {
            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
            List<AEcgHandler.AEcgStEventInfo> stEventInfoList = AEcgHandler.GetStEventInfo();
            if (stEventInfoList == null) return;
            List<EventInfo> events = new List<EventInfo>();
            for (int i = 0; i < stEventInfoList.Count; i++)
            {
                AEcgStEventInfo stEventInfo = stEventInfoList[i];
                EventInfo eventInfo = new EventInfo
                {
                    Duration = stEventInfo.durationTime / AnaValues.holterConfig.SampleRate,
                    StartTime = AnaCode.getDataTimeFromSample(stEventInfo.startPosition).ToString(dateFormat),
                    StartSample = stEventInfo.startPosition,
                    EndSample = stEventInfo.startPosition + stEventInfo.durationTime,
                    AvgHr = stEventInfo.hrAvg,
                    STStatusFlag = stEventInfo.statusFlag,
                    STPointVoltage = (float)Math.Round(stEventInfo.stPointVoltage / 1000, 2),
                    STLead = stEventInfo.lead,
                    STSlope = stEventInfo.stSlope,
                    MinHr = stEventInfo.hrMin,
                    MaxHr = stEventInfo.hrMax,
                    Sample = stEventInfo.startPosition,

                };
                events.Add(eventInfo);

                events.Sort((a, b) => Math.Abs(b.STPointVoltage).CompareTo(Math.Abs(a.STPointVoltage)));
            }
            AllEvents["ST"] = events;
            //if (events.Count > 0)
            //{
            //    //// 检查键是否存在，如果存在则先移除再添加
            //    //if (AllEvents.ContainsKey("ST"))
            //    //{
            //    //    AllEvents.Remove("ST");
            //    //}
            //    //AllEvents.Add("ST", events);

            //    AllEvents["ST"] = events;
            //}
        }

        public static void GetEventByType(List<BeatInfo> beatInfoList, string type)
        {
            string beatSequence = GetBeatSequence(beatInfoList);

            switch (type.ToUpper())
            {
                case "S": // 房性事件（对应BeatUiType = 'S'）
                    DetectEventsWithExclusion(beatInfoList, beatSequence, 'S');
                    break;
                case "V": // 室性事件（对应BeatUiType = 'V'）
                    DetectEventsWithExclusion(beatInfoList, beatSequence, 'V');
                    break;
                case "J": // 交界性事件（对应BeatUiType = 'J'）
                    DetectEventsWithExclusion(beatInfoList, beatSequence, 'J');
                    break;
                case "E": // 室性逸搏（对应BeatUiType = 'E'）
                case "W": // 房性逸搏（对应BeatUiType = 'W'）
                case "G": // 交界性逸搏（对应BeatUiType = 'G'）
                case "T": // R on T（对应BeatUiType = 'T'）
                    GetEscapeEvents(beatInfoList, type);
                    break;
                default:
                    Console.WriteLine($"未知的事件类型: {type}");
                    break;
            }
        }

        public static void GetEscapeEvents(List<BeatInfo> beatInfoList, string type)
        {
            if (beatInfoList == null || beatInfoList.Count == 0)
            {
                return;
            }

            switch (type.ToUpper())
            {
                case "E": // 室性逸搏
                    DetectEscapeEvents(beatInfoList, 'E', "室性逸搏");
                    break;
                case "W": // 房性逸搏
                    DetectEscapeEvents(beatInfoList, 'W', "房性逸搏");
                    break;
                case "G": // 交界性逸搏
                    DetectEscapeEvents(beatInfoList, 'G', "交界性逸搏");
                    break;
                case "T": // R on T
                    DetectEscapeEvents(beatInfoList, 'T', "R on T");
                    break;
                default:
                    Console.WriteLine($"未知的逸搏事件类型: {type}");
                    break;
            }
        }

        public static void DetectEscapeEvents(List<BeatInfo> beatInfoList, char escapeTypeChar, string eventName)
        {
            List<EventInfo> events = new List<EventInfo>();
            string beatSequence = GetBeatSequence(beatInfoList);

            for (int i = 0; i < beatSequence.Length; i++)
            {
                // 检查当前心搏是否为逸搏类型
                if (beatSequence[i] == escapeTypeChar)
                {
                    // 单个逸搏算一个事件
                    EventInfo eventInfo = CreateBeatEventInfo(beatInfoList, i, 1, escapeTypeChar);
                    events.Add(eventInfo);
                }
            }

            AllEvents[eventName] = events;
        }

        public static void DetectEventsWithExclusion(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar)
        {
            if (string.IsNullOrEmpty(beatSequence))
            {
                return;
            }

            // 标记已使用的心搏位置
            bool[] usedPositions = new bool[beatSequence.Length];

            // 按照优先级顺序检测事件：速 > 对 > 三联 > 二联 > 单发
            DetectTachycardiaEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);    // 速：大于等于3个S
            DetectPairedEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);         // 对：SS
            DetectTrigeminyEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);      // 三联：大于等于3个NNS
            DetectBigeminyEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);       // 二联：大于等于3个NS
            DetectSingleEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);         // 单发：S
        }

        public static List<EventInfo> CalcTachycardiaEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = new List<EventInfo>(beatSequence.Length);
            for (int i = 0; i < beatSequence.Length; i++)
            {
                //// 检查当前心搏是否有效
                //if (!validBeatPositions[i])
                //{
                //    continue;
                //}

                if (usedPositions[i])
                {
                    continue;
                }

                // 检查当前心搏是否为指定类型
                if (beatSequence[i] == eventTypeChar)
                {
                    // 计算连续相同心搏的数量
                    int consecutiveCount = 1;
                    int j = i + 1;

                        while (j < beatSequence.Length - 1 && !usedPositions[j] && beatSequence[j] == eventTypeChar)
                        {
                            consecutiveCount++;
                            j++;
                        }



                    // 如果连续3个或更多，判定为速事件
                    if (consecutiveCount >= 3)
                    {
                        if (eventTypeChar == 'S')
                        {
                            while (j < beatSequence.Length - 1 && !usedPositions[j] && beatSequence[j] != 'N')
                            {
                                consecutiveCount++;
                                j++;
                            }
                        }
                        EventInfo eventInfo = CreateBeatEventInfo(beatInfoList, i, consecutiveCount, eventTypeChar);
                        events.Add(eventInfo);

                        // 标记已使用的位置
                        for (int k = i; k < i + consecutiveCount; k++)
                        {
                            if (k < usedPositions.Length)
                            {
                                usedPositions[k] = true;
                            }
                        }

                        // 跳过已检测的心搏
                        i = j - 1;
                    }
                }
            }

            return events;
        }

        public static void DetectTachycardiaEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            string eventName = GetTachycardiaEventName(eventTypeChar);
            List<EventInfo> events = CalcTachycardiaEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);
            //setHourListInfo(events, eventName);
            events.Sort((a, b) => b.Duration.CompareTo(a.Duration));
            AllEvents[eventName] = events;
            //if (events.Count > 0)
            //{
            //    setHourListInfo(events, eventName);

            //    events.Sort((a, b) => b.Duration.CompareTo(a.Duration));

            //    AllEvents[eventName] = events;
            //}
        }

        public static List<EventInfo> CalcPairedEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = new List<EventInfo>();
            for (int i = 0; i < beatSequence.Length - 1; i++)
            {
                //// 检查当前心搏是否有效
                //if (!validBeatPositions[i] || !validBeatPositions[i + 1])
                //{
                //    continue;
                //}

                if (usedPositions[i] || usedPositions[i + 1])
                {
                    continue;
                }

                // 检查连续2个心搏是否为指定类型
                if (beatSequence[i] == eventTypeChar && beatSequence[i + 1] == eventTypeChar)
                {
                    // 检查是否被更高级别事件包含（前面不能有相同类型的心搏）
                    if (i > 0 && !usedPositions[i - 1] && beatSequence[i - 1] == eventTypeChar)
                    {
                        continue; // 被更高级别事件包含，跳过
                    }

                    events.Add(CreateBeatEventInfo(beatInfoList, i, 2, eventTypeChar));

                    // 标记已使用的位置
                    usedPositions[i] = true;
                    usedPositions[i + 1] = true;
                }
            }

            return events;
        }

        public static void DetectPairedEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = CalcPairedEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);
            string eventName = GetPairedEventName(eventTypeChar);
            //setHourListInfo(events, eventName);
            AllEvents[eventName] = events;
            //if (events.Count > 0)
            //{
            //    string eventName = GetPairedEventName(eventTypeChar);

            //    setHourListInfo(events, eventName);
            //    AllEvents[eventName] = events;

            //    //// 检查键是否存在，如果存在则先移除再添加
            //    //if (AllEvents.ContainsKey(eventName))
            //    //{
            //    //    AllEvents.Remove(eventName);
            //    //}
            //    //AllEvents.Add(eventName, events);
            //}
        }

        public static List<EventInfo> CalcTrigeminyEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = new List<EventInfo>();
            for (int i = 0; i < beatSequence.Length - 2; i++)
            {
                //// 检查当前心搏是否有效
                //if (!validBeatPositions[i] || !validBeatPositions[i + 1] || !validBeatPositions[i + 2])
                //{
                //    continue;
                //}

                if (usedPositions[i] || usedPositions[i + 1] || usedPositions[i + 2])
                {
                    continue;
                }

                // 检查NNX模式
                if (beatSequence[i] == 'N' && beatSequence[i + 1] == 'N' && beatSequence[i + 2] == eventTypeChar)
                {
                    // 计算连续NNX模式的数量
                    int patternCount = 1;
                    int j = i + 3;

                    while (j <= beatSequence.Length - 3)
                    {
                        //// 检查当前心搏是否有效
                        //if (!validBeatPositions[j])
                        //{
                        //    break;
                        //}

                        if (usedPositions[j] || usedPositions[j + 1] || usedPositions[j + 2])
                        {
                            break;
                        }

                        if (beatSequence[j] == 'N' && beatSequence[j + 1] == 'N' && beatSequence[j + 2] == eventTypeChar)
                        {
                            patternCount++;
                            j += 3;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 严格按照规则：大于等于3个NNS模式，判定为三联律事件
                    if (patternCount >= 3)
                    {
                        int totalBeats = patternCount * 3;
                        events.Add(CreateBeatEventInfo(beatInfoList, i, totalBeats, eventTypeChar));

                        // 标记已使用的位置
                        for (int k = i; k < i + totalBeats; k++)
                        {
                            if (k < usedPositions.Length)
                            {
                                usedPositions[k] = true;
                            }
                        }

                        // 跳过已检测的心搏
                        i = j - 1;
                    }
                }
            }

            return events;
        }
        public static void DetectTrigeminyEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = CalcTrigeminyEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);
            string eventName = GetTrigeminyEventName(eventTypeChar);
            //setHourListInfo(events, eventName);
            AllEvents[eventName] = events;
            //if (events.Count > 0)
            //{
            //    string eventName = GetTrigeminyEventName(eventTypeChar);
            //    setHourListInfo(events, eventName);
            //    AllEvents[eventName] = events;
            //}
        }
        public static List<EventInfo> CalcBigeminyEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = new List<EventInfo>();
            for (int i = 0; i < beatSequence.Length - 1; i++)
            {
                //// 检查当前心搏是否有效
                //if (!validBeatPositions[i] || !validBeatPositions[i + 1])
                //{
                //    continue;
                //}

                if (usedPositions[i] || usedPositions[i + 1])
                {
                    continue;
                }

                // 检查NX模式
                if (beatSequence[i] == 'N' && beatSequence[i + 1] == eventTypeChar)
                {
                    // 计算连续NX模式的数量
                    int patternCount = 1;
                    int j = i + 2;

                    while (j <= beatSequence.Length - 2)
                    {
                        //if (!validBeatPositions[j] || !validBeatPositions[j + 1])
                        //{
                        //    break;
                        //}

                        if (usedPositions[j] || usedPositions[j + 1])
                        {
                            break;
                        }

                        if (beatSequence[j] == 'N' && beatSequence[j + 1] == eventTypeChar)
                        {
                            patternCount++;
                            j += 2;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 严格按照规则：大于等于3个NS模式，判定为二联律事件
                    if (patternCount >= 3)
                    {
                        int totalBeats = patternCount * 2;
                        events.Add(CreateBeatEventInfo(beatInfoList, i, totalBeats, eventTypeChar));

                        // 标记已使用的位置
                        for (int k = i; k < i + totalBeats; k++)
                        {
                            if (k < usedPositions.Length)
                            {
                                usedPositions[k] = true;
                            }
                        }

                        // 跳过已检测的心搏
                        i = j - 1;
                    }
                }
            }
            return events;
        }
        public static void DetectBigeminyEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = CalcBigeminyEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);
            string eventName = GetBigeminyEventName(eventTypeChar);
            //setHourListInfo(events, eventName);
            AllEvents[eventName] = events;

            //if (events.Count > 0)
            //{
            //    string eventName = GetBigeminyEventName(eventTypeChar);
            //    setHourListInfo(events, eventName);
            //    AllEvents[eventName] = events;
            //}
        }
        public static List<EventInfo> CalcSingleEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = new List<EventInfo>();
            for (int i = 0; i < beatSequence.Length; i++)
            {
                //if (!validBeatPositions[i])
                //{
                //    continue;
                //}

                if (usedPositions[i])
                {
                    continue;
                }

                // 检查单个心搏是否为指定类型
                if (beatSequence[i] == eventTypeChar)
                {
                    events.Add(CreateBeatEventInfo(beatInfoList, i, 1, eventTypeChar));

                    // 标记已使用的位置
                    usedPositions[i] = true;
                }
            }

            return events;
        }
        public static void DetectSingleEvents(List<BeatInfo> beatInfoList, string beatSequence, char eventTypeChar, bool[] usedPositions)
        {
            List<EventInfo> events = CalcSingleEvents(beatInfoList, beatSequence, eventTypeChar, usedPositions);
            string eventName = GetSingleEventName(eventTypeChar);
            //setHourListInfo(events, eventName);
            AllEvents[eventName] = events;
            //if (events.Count > 0)
            //{
            //    string eventName = GetSingleEventName(eventTypeChar);
            //    setHourListInfo(events, eventName);
            //    AllEvents[eventName] = events;
            //}
        }



        public static string GetBeatSequence(List<BeatInfo> beatInfoList)
        {
            if (beatInfoList == null || beatInfoList.Count == 0)
            {
                return "";
            }

            // 将BeatInfoList中的BeatUiType转换为字符串序列
            char[] sequence = new char[beatInfoList.Count];
            for (int i = 0; i < beatInfoList.Count; i++)
            {
                sequence[i] = beatInfoList[i].BeatUiType;
            }

            return new string(sequence);
        }

        public static EventInfo CreateBeatEventInfo(List<BeatInfo> beatInfoList, int startBeatIndex, int beatCount, char beatType)
        {
            EventInfo eventInfo = new EventInfo
            {
                BeatSize = beatCount,
                Duration = CalculateEventDuration(beatInfoList, startBeatIndex, beatCount),
                StartTime = GetStartTimeForBeat(beatInfoList, startBeatIndex),
                StartSample = GetStartSampleForBeat(beatInfoList, startBeatIndex),
                Sample = GetSampleForBeat(beatInfoList, startBeatIndex),
                EndSample = GetEndSampleForBeat(beatInfoList, startBeatIndex),
                Samples = GetEventSamples(beatInfoList, startBeatIndex, beatCount),
                EventSamples = GetEventSamplesByType(beatInfoList, startBeatIndex, beatCount, beatType),
                AvgRR = GetEventAvgRR(beatInfoList, startBeatIndex, beatCount),
                AvgHr = GetEventAvgHr(beatInfoList, startBeatIndex, beatCount),
            };

            return eventInfo;
        }

        public static string GetStartTimeForBeat(List<BeatInfo> beatInfoList, int beatIndex)
        {
            if (beatIndex < 0 || beatIndex >= beatInfoList.Count)
            {
                return "";
            }

            int startSample = beatInfoList[beatIndex].Sample;
            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

            return AnaCode.getDataTimeFromSample(startSample).ToString(dateFormat);
        }

        public static int GetStartSampleForBeat(List<BeatInfo> beatInfoList, int beatIndex)
        {
            if (beatIndex < 0 || beatIndex >= beatInfoList.Count)
            {
                return 0;
            }

            // 返回心搏的实际样本位置
            return beatInfoList[beatIndex].StartIndex;
        }

        public static int GetSampleForBeat(List<BeatInfo> beatInfoList, int beatIndex)
        {
            if (beatIndex < 0 || beatIndex >= beatInfoList.Count)
            {
                return 0;
            }

            // 返回心搏的实际样本位置
            return beatInfoList[beatIndex].Sample;
        }

        public static int GetEndSampleForBeat(List<BeatInfo> beatInfoList, int beatIndex)
        {
            if (beatIndex < 0 || beatIndex >= beatInfoList.Count)
            {
                return 0;
            }

            // 返回心搏的实际样本位置
            return beatInfoList[beatIndex].EndIndex;
        }

        public static List<int> GetEventSamples(List<BeatInfo> beatInfoList, int startBeatIndex, int beatCount)
        {
            List<int> samples = new List<int>(beatCount);
            for (int i = startBeatIndex; i < startBeatIndex + beatCount; i++)
            {
                samples.Add(beatInfoList[i].Sample);
            }
            return samples;
        }

        public static List<int> GetEventSamplesByType(List<BeatInfo> beatInfoList, int startBeatIndex, int beatCount, char eventType)
        {
            List<int> samples = new List<int>(beatCount);
            for (int i = startBeatIndex; i < startBeatIndex + beatCount; i++)
            {
                if (beatInfoList[i].BeatUiType == eventType)
                {
                    samples.Add(beatInfoList[i].Sample);
                }
            }
            return samples;
        }

        public static int GetEventAvgRR(List<BeatInfo> beatInfoList, int startBeatIndex, int beatCount)
        {
            // 添加边界检查
            if (startBeatIndex < 0 || startBeatIndex + beatCount > beatInfoList.Count)
            {
                return 0;
            }

            int RRSum = 0;
            for (int i = startBeatIndex; i < startBeatIndex + beatCount; i++)
            {
                RRSum += beatInfoList[i].RRInterval;
            }

            return RRSum / beatCount;
        }

        public static int GetEventAvgHr(List<BeatInfo> beatInfoList, int startBeatIndex, int beatCount)
        {
            // 添加边界检查
            if (startBeatIndex < 0 || startBeatIndex + beatCount > beatInfoList.Count)
            {
                return 0;
            }

            int RRSum = 0;
            for (int i = startBeatIndex; i < startBeatIndex + beatCount; i++)
            {
                RRSum += beatInfoList[i].RRInterval;
            }

            int RRAvg = RRSum / beatCount;
            return RRAvg > 0 ? 60 * 1000 / RRAvg : 0;
        }

        public static double CalculateEventDuration(List<BeatInfo> beatInfoList, int startIndex, int beatCount)
        {
            if (startIndex < 0 || startIndex + beatCount > beatInfoList.Count)
            {
                return 0f;
            }
            int startSample = beatInfoList[startIndex].StartIndex;
            int endSample = beatInfoList[startIndex + beatCount - 1].EndIndex;
            double value = Convert.ToDouble(endSample - startSample) / AnaValues.holterConfig.SampleRate;
            return Math.Round(value, 1);
        }

        public static string GetTachycardiaEventName(char eventTypeChar)
        {
            switch (eventTypeChar)
            {
                case 'S': return "房速";
                case 'V': return "室速";
                case 'J': return "交界速";
                default: return "未知速";
            }
        }

        public static string GetPairedEventName(char eventTypeChar)
        {
            switch (eventTypeChar)
            {
                case 'S': return "成对房早";
                case 'V': return "成对室早";
                case 'J': return "成对交界早";
                default: return "未知成对";
            }
        }

        public static string GetTrigeminyEventName(char eventTypeChar)
        {
            switch (eventTypeChar)
            {
                case 'S': return "房性三联律";
                case 'V': return "室性三联律";
                case 'J': return "交界三联律";
                default: return "未知三联律";
            }
        }

        public static string GetBigeminyEventName(char eventTypeChar)
        {
            switch (eventTypeChar)
            {
                case 'S': return "房性二联律";
                case 'V': return "室性二联律";
                case 'J': return "交界二联律";
                default: return "未知二联律";
            }
        }

        public static string GetSingleEventName(char eventTypeChar)
        {
            switch (eventTypeChar)
            {
                case 'S': return "单发房早";
                case 'V': return "单发室早";
                case 'J': return "单发交界早";
                default: return "未知单发";
            }
        }

        public static void GetTachycardiaBradycardiaEvent(List<BeatInfo> beatInfoList, EventType eventType)
        {
            if (beatInfoList == null || beatInfoList.Count == 0)
            {
                return;
            }
            // 获取心搏序列（基础数据）
            string beatSequence = GetBeatSequence(beatInfoList);
            // 预计算不可分析心搏的位置和有效心搏位置
            bool[] validBeatPositions = PrecomputeValidBeatPositions(beatInfoList, beatSequence, EcgSets.customSets.allowEctopicBeat);

            string eventName = eventType == EventType.tachycardia ? "心动过速" : "心动过缓";
            List<int> delSamples = ReadDeleteInfo(eventName + "_D");

            ecgSettingValues ecgSets = CommonValues.SystemSettings.ecgSets;

            int useCalcHR;
            int RRCount;
            if (eventType == EventType.tachycardia)
            {
                useCalcHR = ecgSets.customSets.tachycardia;
                RRCount = ecgSets.customSets.tachycardiaCalc;
            }
            else
            {
                useCalcHR = ecgSets.customSets.bradycardia;
                RRCount = ecgSets.customSets.bradycardiaCalc;
            }

            string jsonContent = "";
            // 检查holter.custom文件中是否包含当前事件的Sample值，如果包含则设置Deleted为true
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath))
            {
                jsonContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);

                //jsonContent = File.ReadAllText(deletedEventsFilePath);
            }

            // 第一步：检测连续的满足条件的心搏区间
            List<EventInfo> allEvents = new List<EventInfo>();

            // 遍历心搏序列，检测连续的满足条件的心搏区间
            int i = 0;
            while (i < beatInfoList.Count - RRCount - 2)
            {
                // 检查当前窗口内是否有无效心搏
                bool hasInvalidBeat = false;
                for (int j = i; j < i + RRCount; j++)
                {
                    if (!validBeatPositions[j])
                    {
                        hasInvalidBeat = true;
                        break;
                    }
                }

                // 如果窗口内有无效心搏，跳过该窗口
                if (hasInvalidBeat)
                {
                    i++;
                    continue;
                }

                // 计算连续RRCount个心搏的平均心率
                int totalRR = CalculateTotalRR(beatInfoList, i, RRCount, validBeatPositions);
                if (totalRR == 0)
                {
                    i++;
                    continue;
                }
                int avgRR = totalRR / RRCount;
                int avgHR = 60 * 1000 / avgRR; // 转换为bpm

                int maxAvgHR = avgHR;
                bool bHRMatched = eventType == EventType.tachycardia ? (avgHR >= useCalcHR) : (avgHR <= useCalcHR);

                // 如果满足心率条件，继续向后遍历直到不满足条件
                if (bHRMatched)
                {
                    int startIndex = i;
                    int beatCount = RRCount;

                    // 继续向后遍历，扩展事件区间
                    int j = i + 1;
                    while (j < beatInfoList.Count - RRCount)
                    {
                        // 检查当前心搏是否有效
                        if (!validBeatPositions[j])
                        {
                            break;
                        }

                        // 计算连续RRCount个心搏的平均心率
                        totalRR = CalculateTotalRR(beatInfoList, j, RRCount, validBeatPositions);
                        if (totalRR != 0)
                        {
                            avgRR = totalRR / RRCount;
                            avgHR = 60 * 1000 / avgRR; // 转换为bpm
                        }


                        bool stillMatched = eventType == EventType.tachycardia ? (avgHR >= useCalcHR) : (avgHR <= useCalcHR);
                        if (!stillMatched)
                        {
                            break;
                        }
                        else
                        {
                            if(eventType == EventType.tachycardia)
                            {
                                if(avgHR > maxAvgHR)
                                {
                                    maxAvgHR = avgHR;
                                    startIndex = j;
                                }
                            }
                            else
                            {
                                if(avgHR < maxAvgHR)
                                {
                                    maxAvgHR = avgHR;
                                    startIndex = j;
                                }
                            }
                            j++;
                        }

                    }
                    // 创建事件信息
                    EventInfo eventInfo = CreateTachycardiaBradycardiaEventInfo(beatInfoList, startIndex, beatCount, maxAvgHR, true, jsonContent, eventType);

                    bool isDel = false;
                    for (int k = 0; k < delSamples.Count; k++)
                    {
                        if (eventInfo.Sample == delSamples[k])
                        {
                            isDel = true;
                            break;
                        }
                    }
                    if (isDel == false)
                    {
                        allEvents.Add(eventInfo);
                    }

                    // 跳过已检测的心搏区间
                    i = j;
                }
                else
                {
                    i++;
                }
            }

            // 第二步：对最终事件进行排序
            //if (allEvents.Count > 0)
            //{
            if (eventType == EventType.tachycardia)
            {
                // 对心动过速事件按AvgHr降序排序
                SortEventsByAvgHr(allEvents, false); // false表示降序

                AllEvents["心动过速"] = allEvents;

                //// 检查键是否存在，如果存在则先移除再添加
                //if (AllEvents.ContainsKey("心动过速"))
                //{
                //    AllEvents.Remove("心动过速");
                //}
                //AllEvents.Add("心动过速", allEvents);
            }
            else
            {
                // 对心动过缓事件按AvgHr升序排序
                SortEventsByAvgHr(allEvents, true); // true表示升序
                AllEvents["心动过缓"] = allEvents;

                //// 检查键是否存在，如果存在则先移除再添加
                //if (AllEvents.ContainsKey("心动过缓"))
                //{
                //    AllEvents.Remove("心动过缓");
                //}
                //AllEvents.Add("心动过缓", allEvents);
            }
            //}
        }

        public static void SortEventsByAvgHr(List<EventInfo> eventsList, bool ascending, bool isEvent = true)
        {
            if (eventsList == null || eventsList.Count == 0)
            {
                return;
            }

            // 找出eventsList中HasPic为true的元素（已保存的片段）
            var savedEvents = eventsList.Where(e => e.HasPic).ToList();
            var unsavedEvents = eventsList.Where(e => !e.HasPic).ToList();

            // 根据排序方向分别对已保存和未保存的事件进行排序
            if (ascending)
            {
                // 最慢心率排序：已保存的片段排在最前面，其它按心率升序排序
                unsavedEvents.Sort((a, b) => a.AvgHr.CompareTo(b.AvgHr));
            }
            else
            {
                // 最快心率排序：已保存的片段排在最前面，其它按心率降序排序
                unsavedEvents.Sort((a, b) => b.AvgHr.CompareTo(a.AvgHr));
            }

            // 清空原列表并重新组合：已保存的事件在前，未保存的事件在后
            eventsList.Clear();
            if(isEvent)
            {
                eventsList.AddRange(savedEvents);
            }
            eventsList.AddRange(unsavedEvents);
        }

        public static bool[] PrecomputeValidBeatPositions(List<BeatInfo> beatInfoList, string beatSequence, int allowEctopicBeat)
        {
            int length = beatSequence.Length;
            bool[] validPositions = new bool[length];
            for (int i = 0; i < length; i++)
            {
                bool isValid = true;

                // 检查当前心搏本身是否为X类型
                if (beatSequence[i] == 'X')
                {
                    isValid = false;
                }
                if (beatInfoList[i].RRInterval == -1)
                {
                    isValid = false;
                }
                else
                {
                    // 检查上一个心搏是否为X类型
                    if (i > 0 && beatSequence[i - 1] == 'X')
                    {
                        isValid = false;
                    }

                    // 检查下一个心搏是否为X类型
                    if (i < length - 1 && beatSequence[i + 1] == 'X')
                    {
                        isValid = false;
                    }

                    // 如果allowEctopicBeat == 0，只允许N、K、M类型的心搏
                    if (allowEctopicBeat == 0 && isValid)
                    {
                        //isValid = beatSequence[i] == 'N';
                        //if (i > 0 && isValid)
                        //{
                        //    isValid = beatSequence[i - 1] == 'N';
                        //}
                        //if (i < length - 1 && isValid)
                        //{
                        //    isValid = beatSequence[i + 1] == 'N';
                        //}

                        isValid = beatSequence[i] == 'N' || beatSequence[i] == 'K' || beatSequence[i] == 'M';
                        if (i > 0 && isValid)
                        {
                            isValid = beatSequence[i - 1] == 'N' || beatSequence[i - 1] == 'K' || beatSequence[i - 1] == 'M';
                        }
                        if (i < length - 1 && isValid)
                        {
                            isValid = beatSequence[i + 1] == 'N' || beatSequence[i + 1] == 'K' || beatSequence[i + 1] == 'M';
                        }
                    }
                    bool overlaps = AnaCode.IsInvalidBeatEx(beatInfoList[i]);
                    if (overlaps)
                    {
                        isValid = false;
                    }
                }

                validPositions[i] = isValid;
            }
            return validPositions;
        }

        public static int CalculateTotalRR(List<BeatInfo> beatInfoList, int startIndex, int count, bool[] validBeatPositions)
        {
            int totalRR = 0;
            for (int i = startIndex; i < startIndex + count; i++)
            {
                // 检查当前心搏是否有效,RR间期大于2000的心搏不参与最慢心率计算
                if (!validBeatPositions[i] || beatInfoList[i].RRInterval > 2000)
                {
                    // 条件不满足，终止扩展
                    totalRR = 0;
                    break;
                }

                totalRR += beatInfoList[i].RRInterval;
            }
            return totalRR;
        }

        public static void ProcessEventGroup(List<EventInfo> eventGroup, List<EventInfo> finalEvents, EventType eventType)
        {
            if (eventGroup.Count == 0)
                return;

            if (eventGroup.Count == 1)
            {
                // 单个事件，直接添加到最终结果
                finalEvents.Add(eventGroup[0]);
            }
            else
            {
                // 多个连续事件，取最快或最慢心率
                EventInfo selectedEvent;

                if (eventType == EventType.tachycardia)
                {
                    // 心动过速：取平均心率最快的那个事件
                    selectedEvent = eventGroup.OrderByDescending(e => e.AvgHr).First();
                }
                else
                {
                    // 心动过缓：取平均心率最慢的那个事件
                    selectedEvent = eventGroup.OrderBy(e => e.AvgHr).First();
                }

                finalEvents.Add(selectedEvent);
            }
        }

        public static EventInfo CreateTachycardiaBradycardiaEventInfo(List<BeatInfo> beatInfoList, int startBeatIndex, int beatCount, int avgHR, bool bFlag, string jsonContent, EventType eventType)
        {
            EventInfo eventInfo = new EventInfo
            {
                BeatSize = beatCount,
                Duration = CalculateEventDuration(beatInfoList, startBeatIndex, beatCount),
                StartTime = GetStartTimeForBeat(beatInfoList, startBeatIndex),
                StartSample = GetStartSampleForBeat(beatInfoList, startBeatIndex),
                Sample = GetSampleForBeat(beatInfoList, startBeatIndex),
                EndSample = GetEndSampleForBeat(beatInfoList, startBeatIndex),
                Samples = GetEventSamples(beatInfoList, startBeatIndex, beatCount),
                AvgRR = GetEventAvgRR(beatInfoList, startBeatIndex, beatCount),
                AvgHr = avgHR,
            };
            //if (bFlag && !string.IsNullOrEmpty(jsonContent))
            //{
            //    string[] eventTypesToCheck = { "心动过速", "心动过缓" };
            //    readDeletedEventInfo(eventInfo, eventTypesToCheck, jsonContent, eventType);
            //}
            return eventInfo;
        }

        public static EventInfo CreateEventInfoBySegment(JObject segmentObj)
        {
            EventInfo eventInfo = new EventInfo
            {
                BeatSize = 1,
                Duration = Convert.ToInt32(segmentObj["cbxDuration"]),
                StartTime = segmentObj["tbxStartTime"]?.ToString(),
                StartSample = Convert.ToInt32(segmentObj["startSample"]),
                Sample = Convert.ToInt32(segmentObj["sample"]),
                EndSample = Convert.ToInt32(segmentObj["startSample"]) + Convert.ToInt32(segmentObj["cbxDuration"]) * AnaValues.holterConfig.SampleRate,
                Samples = new List<int> { Convert.ToInt32(segmentObj["sample"]) },
                AvgRR = 60000 / Convert.ToInt32(segmentObj["tbxAvgHR"]),
                AvgHr = Convert.ToInt32(segmentObj["tbxAvgHR"]),
                HasPic = true
            };

            return eventInfo;
        }

        public static List<EventInfo> CalcFastestSlowestHrEvent(List<BeatInfo> beatInfoList, EventType eventType, bool isEvent = false)
        {
            List<EventInfo> allEvents = new List<EventInfo>();

            try
            {
                // 获取心搏序列（基础数据）
                string beatSequence = GetBeatSequence(beatInfoList);
                // 预计算不可分析心搏的位置和有效心搏位置
                bool[] validBeatPositions = PrecomputeValidBeatPositions(beatInfoList, beatSequence, EcgSets.customSets.allowEctopicBeat);

                string eventName = eventType == EventType.fastestHr ? "最快心率" : "最慢心率";
                List<int> delSamples = ReadDeleteInfo(eventName + "_D");
                //int maxMinHr = GetMaxMinHR(eventName);
                int maxMinHr = -1;
                ecgSettingValues ecgSets = CommonValues.SystemSettings.ecgSets;
                int RRCount = ecgSets.customSets.fastestSlowestHr;
                // 第一步：检测连续的满足条件的心搏区间，当条件不满足或跨分钟时终止
                int i = 0;
                while (i < beatInfoList.Count - RRCount)
                {
                    // 检查当前窗口内是否有无效心搏
                    bool hasInvalidBeat = false;
                    for (int k = i; k < i + RRCount; k++)
                    {
                        //RR间期大于2000的心搏不参与最慢心率计算
                        if (!validBeatPositions[k] || beatInfoList[k].RRInterval > 2000)
                        {
                            hasInvalidBeat = true;
                            break;
                        }
                    }

                    // 如果窗口内有无效心搏，跳过该窗口
                    if (hasInvalidBeat)
                    {
                        i++;
                        continue;
                    }

                    // 计算连续RRCount个心搏的平均心率
                    int totalRR = CalculateTotalRR(beatInfoList, i, RRCount, validBeatPositions);
                    if (totalRR == 0)
                    {
                        i++;
                        continue;
                    }
                    double avgRR = 1.0d * totalRR / RRCount;
                    int avgHR = (int)(60 * 1000 / avgRR); // 转换为bpm

                    // 继续向后遍历，扩展事件区间，当条件不满足或跨分钟时终止
                    int startIndex = i;
                    int beatCount = RRCount;
                    int currentMaxAvgHR = avgHR; // 当前事件的最大/最小心率

                    // 获取起始分钟
                    DateTime startMinute = AnaCode.getDataTimeFromSample(beatInfoList[i].StartIndex);
                    startMinute = new DateTime(startMinute.Year, startMinute.Month, startMinute.Day, startMinute.Hour, startMinute.Minute, 0);

                    // 继续向后遍历，扩展事件区间
                    int j = i + 1;
                    while (j < beatInfoList.Count - RRCount - 1)
                    {
                        // 检查是否跨分钟
                        DateTime currentBeatTime = AnaCode.getDataTimeFromSample(beatInfoList[j].StartIndex);
                        DateTime currentBeatMinute = new DateTime(currentBeatTime.Year, currentBeatTime.Month, currentBeatTime.Day, currentBeatTime.Hour, currentBeatTime.Minute, 0);

                        if (currentBeatMinute != startMinute)
                        {
                            // 跨分钟，终止扩展
                            break;
                        }

                        // 检查当前心搏是否有效
                        if (!validBeatPositions[j])
                        {
                            // 条件不满足，终止扩展
                            break;
                        }

                        if(j == i+1 && j > RRCount)
                        {
                            for(int idx=j-RRCount-1; idx<j; idx++)
                            {
                                // 计算连续RRCount个心搏的平均心率
                                totalRR = CalculateTotalRR(beatInfoList, idx, RRCount, validBeatPositions);
                                if (totalRR != 0)
                                {
                                    avgRR = totalRR / RRCount;
                                    avgHR = (int)(60 * 1000 / avgRR); // 转换为bpm
                                }

                                // 更新当前事件的最大/最小心率
                                if (eventType == EventType.fastestHr)
                                {
                                    if (avgHR > currentMaxAvgHR)
                                    {
                                        currentMaxAvgHR = avgHR;
                                        startIndex = idx;
                                    }
                                }
                                else
                                {
                                    if (avgHR < currentMaxAvgHR)
                                    {
                                        currentMaxAvgHR = avgHR;
                                        startIndex = idx;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 计算连续RRCount个心搏的平均心率
                            totalRR = CalculateTotalRR(beatInfoList, j, RRCount, validBeatPositions);
                            if (totalRR != 0)
                            {
                                avgRR = totalRR / RRCount;
                                avgHR = (int)(60 * 1000 / avgRR); // 转换为bpm
                            }

                            // 更新当前事件的最大/最小心率
                            if (eventType == EventType.fastestHr)
                            {
                                if (avgHR > currentMaxAvgHR)
                                {
                                    currentMaxAvgHR = avgHR;
                                    startIndex = j;
                                }
                            }
                            else
                            {
                                if (avgHR < currentMaxAvgHR)
                                {
                                    currentMaxAvgHR = avgHR;
                                    startIndex = j;
                                }
                            }
                        }

                        j++;
                    }
                    // 创建事件信息
                    EventInfo eventInfo = CreateTachycardiaBradycardiaEventInfo(beatInfoList, startIndex, beatCount, currentMaxAvgHR, false, "", eventType);
                    bool isDel = false;
                    for (int k = 0; k < delSamples.Count; k++)
                    {
                        if (eventInfo.Sample == delSamples[k])
                        {
                            isDel = true;
                            break;
                        }
                    }
                    if (isDel == false)
                    {
                        if (maxMinHr < 0 || beatInfoList.Count < AnaValues.CachedAllBeats.Count)
                        {
                            allEvents.Add(eventInfo);
                        }
                        else
                        {
                            if (eventType == EventType.fastestHr)
                            {
                                if (eventInfo.AvgHr <= maxMinHr)
                                {
                                    allEvents.Add(eventInfo);
                                }
                            }
                            else
                            {

                                if (eventInfo.AvgHr >= maxMinHr)
                                {
                                    allEvents.Add(eventInfo);
                                }
                            }
                        }
                    }
                    // 跳过已检测的心搏区间
                    i = j;
                }

                // 第二步：每分钟只取一个最快或最慢心率
                if (allEvents.Count > 0)
                {
                    List<EventInfo> filteredEvents = FilterEventsByMinute(allEvents, eventType == EventType.fastestHr);
                    for (int idx = 0; idx < filteredEvents.Count; idx++)
                    {
                        filteredEvents[idx].HasPic = false;
                    }
                    // 第三步：对筛选后的事件进行排序
                    if (eventType == EventType.fastestHr)
                    {
                        if (maxMinHr < 0 || beatInfoList.Count < AnaValues.CachedAllBeats.Count)
                        {
                            EventSaved(filteredEvents, "最快心率");
                            // 对最快心率事件按AvgHr降序排序
                            SortEventsByAvgHr(filteredEvents, false, isEvent); // false表示降序
                        }
                        else
                        {
                            EventSaved(filteredEvents, "最快心率");
                            // 对最快心率事件按AvgHr降序排序
                            SortEventsByAvgHr(filteredEvents, false, isEvent); // false表示降序
                        }

                    }
                    else
                    {
                        if (maxMinHr < 0 || beatInfoList.Count < AnaValues.CachedAllBeats.Count)
                        {
                            EventSaved(filteredEvents, "最慢心率");
                            SortEventsByAvgHr(filteredEvents, true, isEvent); // true表示升序
                        }
                        else
                        {
                            EventSaved(filteredEvents, "最慢心率");
                            // 对最慢心率事件按AvgHr升序排序
                            SortEventsByAvgHr(filteredEvents, true, isEvent); // true表示升序
                        }

                    }

                    return filteredEvents;
                }
                
            }
            catch(Exception ex)
            {
                Logger.CreateLog(ex, "小时数据表");
            }
            return allEvents;
        }

        public static void GetFastestSlowestHrEvent(List<BeatInfo> beatInfoList, EventType eventType)
        {
            if (beatInfoList == null || beatInfoList.Count == 0)
            {
                return;
            }

            List<EventInfo> filteredEvents = CalcFastestSlowestHrEvent(beatInfoList, eventType, true);
            //if (filteredEvents.Count == 0) return;
            if (eventType == EventType.fastestHr)
            {
                AllEvents["最快心率"] = filteredEvents;

                //if (AllEvents.ContainsKey("最快心率"))
                //{
                //    AllEvents.Remove("最快心率");
                //}
                //AllEvents.Add("最快心率", filteredEvents);
            }
            else
            {
                AllEvents["最慢心率"] = filteredEvents;

                //// 检查键是否存在，如果存在则先移除再添加
                //if (AllEvents.ContainsKey("最慢心率"))
                //{
                //    AllEvents.Remove("最慢心率");
                //}
                //AllEvents.Add("最慢心率", filteredEvents);
            }


        }

        public static List<EventInfo> FilterEventsByMinute(List<EventInfo> events, bool getFastest)
        {
            if (events == null || events.Count == 0)
            {
                return new List<EventInfo>();
            }

            // 按分钟分组
            var eventsByMinute = new Dictionary<string, List<EventInfo>>();

            foreach (var eventInfo in events)
            {
                DateTime time = AnaCode.getDataTimeFromSample(eventInfo.StartSample);
                // 提取分钟级别的键（去掉秒和毫秒）
                string minuteKey = time.ToString("yyyy-MM-dd HH:mm");
                if (!eventsByMinute.ContainsKey(minuteKey))
                {
                    eventsByMinute[minuteKey] = new List<EventInfo>();
                }

                eventsByMinute[minuteKey].Add(eventInfo);
            }

            // 从每个分钟组中选择最快或最慢的心率事件
            var filteredEvents = new List<EventInfo>();

            foreach (var minuteGroup in eventsByMinute.Values)
            {
                if (minuteGroup.Count > 0)
                {
                    EventInfo selectedEvent;

                    if (getFastest)
                    {
                        // 选择最快心率（AvgHr最大）
                        selectedEvent = minuteGroup.OrderByDescending(e => e.AvgHr).First();
                    }
                    else
                    {
                        // 选择最慢心率（AvgHr最小）
                        selectedEvent = minuteGroup.OrderBy(e => e.AvgHr).First();
                    }

                    filteredEvents.Add(selectedEvent);
                }
            }

            return filteredEvents;
        }

        public static void EventSaved(List<EventInfo> EventList, string eventName)
        {
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath))
            {
                try
                {
                    string jsonContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);

                    //string jsonContent = File.ReadAllText(deletedEventsFilePath);
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        JObject jsonData = JObject.Parse(jsonContent);
                        if (jsonData.ContainsKey(eventName))
                        {
                            int longestRRSample = Convert.ToInt32(jsonData[eventName]["sample"].ToString());
                            bool bHasEvent = false;
                            for (int i = 0; i < EventList.Count; i++)
                            {
                                if (longestRRSample == EventList[i].Sample)
                                {
                                    EventList[i].HasPic = true;
                                    bHasEvent = true;
                                    break;
                                }
                            }
                            //此处用于处理在直方图界面保存的最快心率、最慢心率
                            if (bHasEvent == false)
                            {
                                if (eventName == "最快心率" || eventName == "最慢心率")
                                {
                                    JObject segmentObj = OperateSegment.SegmentExistByName(SegmentFilePath, eventName);
                                    EventList.Add(CreateEventInfoBySegment(segmentObj));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "最快最慢心率");
                }
            }
        }

        public static void GetLongestRREvent(List<BeatInfo> beatInfoList, EventType eventType)
        {
            if (beatInfoList == null || beatInfoList.Count == 0)
            {
                return;
            }
            // 获取心搏序列（基础数据）
            string beatSequence = GetBeatSequence(beatInfoList);
            // 预计算不可分析心搏的位置和有效心搏位置
            bool[] validBeatPositions = PrecomputeValidBeatPositions(beatInfoList, beatSequence, EcgSets.customSets.allowEctopicBeat);

            List<int> delSamples = AnaEvent.ReadDeleteInfo("最长RR间期_D");
            int longestRR = GetLongestRR("最长RR间期");
            List<EventInfo> events = new List<EventInfo>();
            try
            {
                // 按分钟分组，记录每分钟内的最长RR间期
                var longestRRByMinute = new Dictionary<string, EventInfo>();

                // 遍历所有心搏，检测最长RR间期事件
                for (int i = 1; i < beatInfoList.Count - 1; i++)
                {
                    if (!validBeatPositions[i])
                    {
                        continue;
                    }
                    // 创建事件信息
                    EventInfo eventInfo = CreateLongRREventInfo(beatInfoList, i, 1, beatInfoList[i].RRInterval, true, eventType);
                    bool isDel = false;
                    for (int j = 0; j < delSamples.Count; j++)
                    {
                        if (eventInfo.Sample == delSamples[j])
                        {
                            isDel = true;
                            break;
                        }
                    }
                    if (isDel == false)
                    {
                        if (longestRR < 0)
                        {
                            events.Add(eventInfo);
                        }
                        else
                        {
                            if (eventInfo.AvgRR <= longestRR)
                            {
                                events.Add(eventInfo);
                            }
                        }

                    }

                    //// 提取分钟级别的键
                    //DateTime time = AnaCode.getDataTimeFromSample(eventInfo.StartSample);
                    //string minuteKey = time.ToString("yyyy-MM-dd HH:mm");

                    //// 更新每分钟的最长RR间期
                    //if (!longestRRByMinute.ContainsKey(minuteKey))
                    //{
                    //    longestRRByMinute[minuteKey] = eventInfo;
                    //}
                    //else
                    //{
                    //    // 如果当前RR间期更长，则更新
                    //    if (currentRR >= longestRRByMinute[minuteKey].AvgRR)
                    //    {
                    //        longestRRByMinute[minuteKey] = eventInfo;
                    //    }
                    //}

                }

                //// 将所有每分钟的最长RR间期添加到事件列表
                //events.AddRange(longestRRByMinute.Values);

                //OperateSegment.EventSegmentExist(SegmentFilePath, events, "最长RR间期");
                EventSaved(events, "最长RR间期");

                // 按HasPic优先排序：HasPic为true的事件在前，然后按RR间期降序排序
                events.Sort((a, b) =>
                {
                    // 优先按HasPic排序：true在前，false在后
                    int picComparison = b.HasPic.CompareTo(a.HasPic);
                    if (picComparison != 0)
                    {
                        return picComparison;
                    }
                    return b.AvgRR.CompareTo(a.AvgRR);
                });

                // 取前2000个
                if (events.Count > 2000)
                {
                    events = events.Take(2000).ToList();
                }

                // 添加到AllEvents字典
                //if (events.Count > 0)
                //{
                AllEvents["最长RR间期"] = events;


                //if (AllEvents.ContainsKey("最长RR间期"))
                //{
                //    AllEvents.Remove("最长RR间期");
                //}
                //AllEvents.Add("最长RR间期", events);

                //}

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心电事件");
            }
        }

        public static List<EventInfo> CalcLongRRAndAsystoleEvent(List<BeatInfo> beatInfoList, EventType eventType)
        {
            string beatSequence = GetBeatSequence(beatInfoList);
            bool[] validBeatPositions = PrecomputeValidBeatPositions(beatInfoList, beatSequence, CommonValues.SystemSettings.ecgSets.customSets.allowEctopicBeat);

            string eventName = eventType == EventType.asystole ? "停搏" : "长RR间期";
            List<int> delSamples = AnaEvent.ReadDeleteInfo(eventName + "_D");
            int longestRR = GetLongestRR("最长RR间期");

            ecgSettingValues ecgSets = CommonValues.SystemSettings.ecgSets;

            int useRRTime = 0;
            if (eventType == EventType.asystole)
            {
                useRRTime = ecgSets.customSets.asystoleTime;
            }
            else
            {
                useRRTime = ecgSets.customSets.longRR;
            }
            int includePremature = (eventType == EventType.asystole) ? 1 : ecgSets.customSets.includePremature;

            List<EventInfo> events = new List<EventInfo>();

            // 遍历心搏序列，检测长RR间期和停搏事件
            for (int i = 1; i < beatInfoList.Count - 1; i++)
            {
                if (!validBeatPositions[i])
                {
                    continue;
                }
                // 检查当前心搏是否为异位心搏且includePremature==0
                if (includePremature == 0)
                {
                    char currentBeatType = (i - 1 > 0) ? beatSequence[i - 1] : 'N';

                    // 如果当前心搏是'S','V','J'类型，跳过该心搏与其下一个心搏之间的间期
                    if (currentBeatType == 'S' || currentBeatType == 'V' || currentBeatType == 'J')
                    {
                        continue;
                    }
                }

                // 获取当前心搏的RR间期
                int currentRR = beatInfoList[i].RRInterval;

                // 检查RR间期是否大于等于阈值
                if (currentRR >= useRRTime)
                {
                    bool isDel = false;
                    // 创建事件信息
                    EventInfo eventInfo = CreateLongRREventInfo(beatInfoList, i, 1, currentRR, true, eventType);
                    for (int j = 0; j < delSamples.Count; j++)
                    {
                        if (eventInfo.Sample == delSamples[j])
                        {
                            isDel = true;
                            break;
                        }
                    }
                    if (isDel == false)
                    {
                        if (longestRR < 0)
                        {
                            Console.WriteLine($"CalcLongRRAndAsystoleEvent: {longestRR}-beatIdx: {i}-startIndex: {eventInfo.StartSample } endIndex: {eventInfo.EndSample}");

                            events.Add(eventInfo);
                        }
                        else
                        {
                            if (eventInfo.AvgRR <= longestRR)
                            {
                                Console.WriteLine($"CalcLongRRAndAsystoleEvent: {longestRR}-beatIdx: {i}-startIndex: {eventInfo.StartSample } endIndex: {eventInfo.EndSample}");

                                events.Add(eventInfo);
                            }
                        }

                    }

                }
            }
            return events;
        }

        public static void GetLongRRAndAsystoleEvent(List<BeatInfo> beatInfoList, EventType eventType)
        {
            if (beatInfoList == null || beatInfoList.Count == 0)
            {
                return;
            }
            //// 获取心搏序列（基础数据）
            //string beatSequence = GetBeatSequence(beatInfoList);
            //// 预计算不可分析心搏的位置和有效心搏位置
            //bool[] validBeatPositions = PrecomputeValidBeatPositions(beatInfoList, beatSequence, EcgSets.customSets.allowEctopicBeat);

            string eventName = eventType == EventType.asystole ? "停搏" : "长RR间期";

            List<EventInfo> events = CalcLongRRAndAsystoleEvent(beatInfoList, eventType);

            events.Sort((a, b) => b.AvgRR.CompareTo(a.AvgRR));

            AllEvents[eventName] = events;

            //if (events.Count > 0)
            //{
            //    AllEvents[eventName] = events;
            //}
        }

        public static EventInfo CreateLongRREventInfo(List<BeatInfo> beatInfoList, int startBeatIndex, int beatCount, int rrInterval, bool bFlag, EventType eventType)
        {
            EventInfo eventInfo = new EventInfo
            {
                BeatSize = beatCount,
                Duration = CalculateEventDuration(beatInfoList, startBeatIndex, beatCount),
                StartTime = GetStartTimeForBeat(beatInfoList, startBeatIndex),
                StartSample = GetStartSampleForBeat(beatInfoList, startBeatIndex),
                Sample = GetSampleForBeat(beatInfoList, startBeatIndex),
                EndSample = GetEndSampleForBeat(beatInfoList, startBeatIndex),
                Samples = GetEventSamples(beatInfoList, startBeatIndex, beatCount),
                AvgRR = rrInterval,
                AvgHr = 60 * 1000 / rrInterval, // 根据RR间期计算心率
            };
            //if (bFlag && !string.IsNullOrEmpty(jsonContent))
            //{
            //    string[] eventTypesToCheck = {"停搏", "长RR间期"};
            //    readDeletedEventInfo(eventInfo, eventTypesToCheck, jsonContent, eventType);
            //}

            return eventInfo;
        }

        public static int GetMaxMinHR(string eventName)
        {
            int hr = -1;
            string jsonContent = "";
            // 检查holter.custom文件中是否包含当前事件的Sample值，如果包含则设置Deleted为true
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath))
            {
                //jsonContent = File.ReadAllText(deletedEventsFilePath);
                jsonContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);

                JObject jsonData = JObject.Parse(jsonContent);
                if (jsonData.ContainsKey(eventName))
                {
                    JObject jsonObj = (JObject)jsonData[eventName];
                    hr = Convert.ToInt32(jsonObj["value"]);
                }
            }
            return hr;
        }

        public static int GetLongestRR(string eventName)
        {
            int rr = -1;
            string jsonContent = "";
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath))
            {
                jsonContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);

                //jsonContent = File.ReadAllText(deletedEventsFilePath);
                JObject jsonData = JObject.Parse(jsonContent);
                if (jsonData.ContainsKey(eventName))
                {
                    JObject jsonObj = (JObject)jsonData[eventName];
                    rr = Convert.ToInt32(jsonObj["RR"]);
                }
            }
            return rr;
        }

        public static void DelMaxEventInfo(string eventName)
        {
            string jsonContent = "";
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath))
            {
                jsonContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);

                //jsonContent = File.ReadAllText(deletedEventsFilePath);
                JObject jsonData = JObject.Parse(jsonContent);
                if (jsonData.ContainsKey(eventName))
                {
                    jsonData.Remove(eventName);

                    // 写回文件
                    string updatedJson = jsonData.ToString();
                    FileOperationsWithMutex.SafeWriteToFile(updatedJson, AnaValues.DeleteEventFileName, deletedEventsFilePath);
                    //File.WriteAllText(deletedEventsFilePath, updatedJson);
                }
            }
        }

        /// <summary>
        /// 新增房早未下传
        /// </summary>
        /// <param name="beatInfoList">关联的心搏</param>
        /// <param name="distance">与当前心搏的前一个心搏的距离</param>
        public static void AddNcaOrFBeat(int[] samples, int distance, string eventName)
        {
            Stopwatch sw = Stopwatch.StartNew();

            string eventFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            JObject jsonData = new JObject();
            if (File.Exists(eventFilePath))
            {
                string existingContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, eventFilePath);

                //string existingContent = File.ReadAllText(eventFilePath);
                if (!string.IsNullOrEmpty(existingContent))
                {
                    jsonData = JObject.Parse(existingContent);
                }
            }

            if (!jsonData.ContainsKey(eventName)) { jsonData[eventName] = new JArray(); }
            JArray eventSamples = (JArray)jsonData[eventName];
            for (int i = 0; i < samples.Length; i++)
            {
                eventSamples.Add(JObject.FromObject(new { sample = samples[i], offset = distance, deleted = 0 }));
            }
            FileOperationsWithMutex.SafeWriteToFile(jsonData.ToString(), AnaValues.DeleteEventFileName, eventFilePath);
            //File.WriteAllText(eventFilePath, jsonData.ToString());
            ReadNcaOrFBeat(eventName); //更新内存中的相关数据
            AnaValues.DataSourceState = 1; //通知事件统计界面刷新显示

            object para = new { AnaValues.CurrentCase.caseNo, eventName };
            OperateLog.logParam("写入的事件信息", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);

        }

        /// <summary>
        /// 删除房早未下传或室性融合波
        /// </summary>
        /// <param name="beatInfoList">关联的心搏</param>
        public static void DeleteNcaOrFBeat(List<int> samples, string eventName)
        {
            Stopwatch sw = Stopwatch.StartNew();

            string eventFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (!File.Exists(eventFilePath)) return;

            JObject jsonData = new JObject();
            string existingContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, eventFilePath);

            //string existingContent = File.ReadAllText(eventFilePath);
            if (!string.IsNullOrEmpty(existingContent))
            {
                jsonData = JObject.Parse(existingContent);
                JArray eventSamples = jsonData[eventName] as JArray;
                if (eventSamples != null)
                {
                    for (int i = eventSamples.Count - 1; i >= 0; i--)
                    {
                        int sample = Convert.ToInt32(eventSamples[i]["sample"]);
                        if (samples.Exists(o => o == sample))
                        {
                            eventSamples.RemoveAt(i);
                        }
                    }
                    FileOperationsWithMutex.SafeWriteToFile(jsonData.ToString(), AnaValues.DeleteEventFileName, eventFilePath);
                    //File.WriteAllText(eventFilePath, jsonData.ToString());
                }
            }

            object para = new { AnaValues.CurrentCase.caseNo, eventName };
            OperateLog.logParam("删除的事件信息", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);

        }

        /// <summary>
        /// 删除房早未下传
        /// </summary>
        /// <param name="beatInfoList">关联的心搏</param>
        public static void DeleteAcceptNca(List<int> samples, int deleted, string eventName)
        {
            Stopwatch sw = Stopwatch.StartNew();

            string eventFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (!File.Exists(eventFilePath)) return;

            JObject jsonData = new JObject();
            string existingContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, eventFilePath);

            //string existingContent = File.ReadAllText(eventFilePath);
            if (!string.IsNullOrEmpty(existingContent))
            {
                jsonData = JObject.Parse(existingContent);
                JArray eventSamples = jsonData[eventName] as JArray;
                if (eventSamples != null)
                {
                    for (int i = eventSamples.Count - 1; i >= 0; i--)
                    {
                        int sample = Convert.ToInt32(eventSamples[i]["sample"]);
                        if (samples.Exists(o => o == sample))
                        {
                            eventSamples[i]["deleted"] = deleted;
                            //eventSamples.RemoveAt(i);
                        }
                    }
                    FileOperationsWithMutex.SafeWriteToFile(jsonData.ToString(), AnaValues.DeleteEventFileName, eventFilePath);

                    //File.WriteAllText(eventFilePath, jsonData.ToString());
                }
            }

            object para = new { AnaValues.CurrentCase.caseNo, eventName };
            OperateLog.logParam("删除的事件信息", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);

        }

        /// <summary>
        /// 清除掉已删除的房早未下传
        /// </summary>
        /// <param name="beatInfoList">关联的心搏</param>
        public static void ClearDeletedNca()
        {
            Stopwatch sw = Stopwatch.StartNew();

            string eventFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (!File.Exists(eventFilePath)) return;

            JObject jsonData = new JObject();
            string existingContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, eventFilePath);

            //string existingContent = File.ReadAllText(eventFilePath);
            if (!string.IsNullOrEmpty(existingContent))
            {
                jsonData = JObject.Parse(existingContent);
                JArray eventSamples = jsonData["房早未下传"] as JArray;
                if (eventSamples != null)
                {
                    for (int i = eventSamples.Count - 1; i >= 0; i--)
                    {
                        int deleted = Convert.ToInt32(eventSamples[i]["deleted"]);
                        if (deleted == 1)
                        {
                            eventSamples.RemoveAt(i);
                        }
                    }
                    FileOperationsWithMutex.SafeWriteToFile(jsonData.ToString(), AnaValues.DeleteEventFileName, eventFilePath);

                    //File.WriteAllText(eventFilePath, jsonData.ToString());
                }
            }

            object para = new { AnaValues.CurrentCase.caseNo };
            OperateLog.logParam("清除已删除的房早未下传的事件信息", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);

        }

        public static List<EventInfo> ReadNcaOrFBeat(string eventName)
        {
            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

            List<EventInfo> events = new List<EventInfo>();
            // 检查holter.custom文件中是否包含当前事件的Sample值，如果包含则设置Deleted为true
            string eventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(eventsFilePath))
            {
                try
                {
                    string jsonContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, eventsFilePath);

                    //string jsonContent = File.ReadAllText(eventsFilePath);
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        JObject jsonData = JObject.Parse(jsonContent);

                        JArray eventSamples = jsonData[eventName] as JArray;
                        if (eventSamples != null)
                        {
                            for (int i = eventSamples.Count - 1; i >= 0; i--)
                            {
                                if (Convert.ToInt32(eventSamples[i]["deleted"]) == 0)
                                {
                                    int sample = Convert.ToInt32(eventSamples[i]["sample"]);
                                    int offset = Convert.ToInt32(eventSamples[i]["offset"]);
                                    EventInfo eventInfo = new EventInfo
                                    {
                                        StartTime = AnaCode.getDataTimeFromSample(sample).ToString(dateFormat),
                                        Sample = sample,
                                        Offset = offset,
                                        BeatSize = 1,
                                    };
                                    events.Add(eventInfo);
                                }

                            }
                        }
                    }
                    //if (AllEvents.ContainsKey(eventName))
                    //{
                    //    AllEvents.Remove(eventName);
                    //}
                    // 添加到AllEvents字典
                    //if (events.Count > 0)
                    //{
                    AllEvents[eventName] = events;

                    //AllEvents[eventName] = events;
                    //}

                }
                catch (Exception ex)
                {
                    // 记录错误但不中断程序执行
                    System.Diagnostics.Debug.WriteLine($"读取holter.custom文件失败: {ex.Message}");
                }
            }
            return events;
        }

        /// <summary>
        /// 计算起搏个数 
        /// </summary>
        /// <param name="escapeTypeChar">
        /// O=>ap房性起搏，P=>vp室性起搏，H=avp房室顺序起搏
        /// </param>
        /// <returns></returns>
        public static int CalcPaceCount(string beatSequence, char escapeTypeChar)
        {
            int count = 0;
            for (int i = 0; i < beatSequence.Length; i++)
            {
                // 检查当前心搏是否为逸搏类型
                if (beatSequence[i] == escapeTypeChar)
                {
                    ++count;
                }
            }
            return count;
        }

        /// <summary>
        /// 根据已删除的长RR间期和停搏，重新更新最长RR间期
        /// </summary>
        public static void UpdateLongestRR()
        {
            List<EventInfo> LongestRREventList = AllEvents["最长RR间期"];

            if (LongestRREventList.Count == 0) return;
            string jsonContent = "";
            // 检查holter.custom文件中是否包含当前事件的Sample值，如果包含则设置Deleted为true
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath))
            {
                jsonContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);

                //jsonContent = File.ReadAllText(deletedEventsFilePath);
            }

            for (int i = 0; i < LongestRREventList.Count; i++)
            {
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    JObject jsonData = JObject.Parse(jsonContent);
                    string[] eventTypesToCheck = { "停搏", "长RR间期" };
                    foreach (string eventType in eventTypesToCheck)
                    {
                        if (jsonData.ContainsKey(eventType))
                        {
                            JArray samplesArray = (JArray)jsonData[eventType];
                            foreach (var sampleToken in samplesArray)
                            {
                                if (sampleToken.Value<int>() == LongestRREventList[i].Sample)
                                {
                                    LongestRREventList.RemoveAt(i);
                                }
                            }
                        }
                    }
                }

            }

            AllEvents["最长RR间期"] = LongestRREventList;

            AnaCode.DeleteSaveInfo("最长RR间期");

        }


        public static List<int> ReadDeleteInfo(string keyStr)
        {
            List<int> samples = new List<int>();
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);

            // 读取删除的事件sample信息
            if (!string.IsNullOrEmpty(deletedEventsFilePath))
            {
                try
                {
                    // 读取现有JSON文件内容
                    JObject jsonData = new JObject();
                    if (File.Exists(deletedEventsFilePath))
                    {
                        string existingContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);

                        //string existingContent = File.ReadAllText(deletedEventsFilePath);
                        if (!string.IsNullOrEmpty(existingContent))
                        {
                            jsonData = JObject.Parse(existingContent);
                            // 添加当前事件类型的样本
                            if (jsonData.ContainsKey(keyStr))
                            {
                                JArray eventSamples = (JArray)jsonData[keyStr];
                                foreach (var delSample in eventSamples)
                                {
                                    samples.Add(delSample.Value<int>());
                                }
                            }
                        }

                    }


                }
                catch (Exception ex)
                {
                    // 记录错误但不中断程序执行
                    Debug.WriteLine($"读取删除事件文件失败: {ex.Message}");
                }
            }
            return samples;
        }

    }
}
