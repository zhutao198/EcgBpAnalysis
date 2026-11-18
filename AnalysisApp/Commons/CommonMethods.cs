using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Utilities;

namespace AnalysisApp.Commons
{
    public static class AnaCode
    {
        private static readonly object _lockObj = new object();

        public static bool IsCalculatingHR = false;

        public static bool IsAnalysisST = false;

        /// <summary>
        /// 保存内存中所有的(算法)数据更新
        /// </summary>
        public static void SaveAnalyzeData(bool forceToSave = false)
        {
            if (AEcgHandler.UpdatesCount > 0 || forceToSave)
            {
                PubFuns.AsyncDoWork(AEcgHandler.SaveAnalyzeData);
            }
        }

        /// <summary>
        /// 将sample点转换成时间
        /// </summary>
        /// <returns></returns>
        public static DateTime getDataTimeFromSample(int sample)
        {
            // 解析Holter记录的起始时间
            if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
            {
                startTime = DateTime.Today;
            }

            double sampleOffsetSeconds = (double)sample / AnaValues.holterConfig.SampleRate * 1.0f;

            // 计算当前波形起始时间：Holter起始时间 + 波形时间偏移
            DateTime sampleDataTime = startTime.AddSeconds(sampleOffsetSeconds);

            return sampleDataTime;
        }

        /// <summary>
        /// 将时间转换成采样点序号
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int getSampleFromDateTime(DateTime dt)
        {
            HolterConfig cfg = AnaValues.holterConfig;
            int sample = (int)(dt.Subtract(cfg.StartDateTime).TotalSeconds * cfg.SampleRate);
            return sample;
        }

        /// <summary>
        /// 数据统计计算
        /// </summary>
        public async static void LoadStatisticsData()
        {
            //if (AnaValues.IsLoadingData) return;
            while (AnaValues.IsLoadingData)
            {
                await Task.Delay(5);
            }

            AnaEvent.IsCalculating = true;
            CalcHourListInfo.IsCalculating = true;
            AnaCode.IsCalculatingHR = true;

            AnaValues.IsLoadingData = true;
            Task[] loadingTasks = new Task[] {
                Task.Run(()=>AnaCode.CalcMinuteHRInfo()),
                Task.Run(async()=>
                {
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        await AnaEvent.CalcEventInfo();
                        Logger.LogRuntimeInfo("事件分析相关计算", sw.ElapsedMilliseconds);
                        sw.Restart();
                        AnaEvent.HourTimeList = AnaEvent.GetHourList();
                        AnaEvent.CalcHourlyBeats();
                        CalcHourListInfo.CalculateHourlyData();
                        Logger.LogRuntimeInfo("小时数据表相关计算", sw.ElapsedMilliseconds);
                    }
                    catch(Exception ex)
                    {
                        Logger.CreateLog(ex, "小时数据计算");

                    }
                })
            };
            AnaValues.IsLoadingData = false;
            //Task.WhenAll(loadingTasks).ContinueWith(t =>
            //{
            //    AnaValues.IsLoadingData = false;
            //});
        }

        /// <summary>
        /// 计算每分钟的平均心率、最快心率、最慢心率
        /// </summary>
        /// <returns></returns>
        public static void CalcMinuteHRInfo()
        {
            List<BeatInfo> cachedAllBeats = AnaValues.CachedAllBeats;
            if (cachedAllBeats == null || cachedAllBeats.Count < 2)
            {
                InitializeEmptyMinuteHR();
                return;
            }

            BeatInfo lastBeat = cachedAllBeats[cachedAllBeats.Count - 1];
            int sampleRate = AnaValues.holterConfig.SampleRate;
            int minuteSamples = sampleRate * 60;
            int minutes = (int)Math.Ceiling((double)AnaValues.holterConfig.Total / minuteSamples);

            lock (_lockObj)
            {
                // 预分配内存，避免重复扩容
                AnaValues.minuteHR.Clear();
                AnaValues.minuteHR.Capacity = minutes;

                // 跳过第一个心搏（i == 0），从第二个开始处理
                int beatIndex = 1;

                for (int minuteIndex = 0; minuteIndex < minutes; minuteIndex++)
                {
                    int startSample = minuteIndex * minuteSamples;
                    int endSample = (minuteIndex + 1) * minuteSamples;
                    if (startSample > lastBeat.StartIndex)
                    {
                        AnaValues.minuteHR.Add(new MinuteHRInfo { Min = 0, Avg = 0, Max = 0 });
                        continue;
                    }

                    int sampleCount = 0;
                    int sumHR = 0;
                    int minHR = int.MaxValue;
                    int maxHR = int.MinValue;

                    // 使用二分查找快速定位到当前分钟的开始位置
                    beatIndex = FindFirstBeatInRange(cachedAllBeats, beatIndex, startSample, endSample);

                    // 处理当前分钟内的所有心搏
                    while (beatIndex < cachedAllBeats.Count && cachedAllBeats[beatIndex].Sample < endSample)
                    {
                        var beat = cachedAllBeats[beatIndex];

                        // 跳过无效的心搏
                        if (beat.RRInterval > 0 && !IsInvalidBeat(beat))
                        {
                            int HR = (int)(60000D / beat.RRInterval);

                            // 更新统计信息
                            if (HR > 0)
                            {
                                sumHR += HR;
                                if (HR < minHR) minHR = HR;
                                if (HR > maxHR) maxHR = HR;
                                sampleCount++;
                            }
                        }

                        beatIndex++;
                    }

                    // 添加当前分钟的心率信息
                    if (sampleCount == 0)
                    {
                        AnaValues.minuteHR.Add(new MinuteHRInfo { Min = 0, Avg = 0, Max = 0 });
                    }
                    else
                    {
                        int avgHR = Math.Max(0, sumHR / sampleCount);
                        minHR = Math.Max(0, minHR);
                        maxHR = Math.Max(0, maxHR);
                        AnaValues.minuteHR.Add(new MinuteHRInfo { Min = minHR, Avg = avgHR, Max = maxHR });
                    }
                }
            }
            IsCalculatingHR = false;
        }

        /// <summary>
        /// 使用二分查找快速定位到指定范围内的第一个心搏
        /// </summary>
        private static int FindFirstBeatInRange(List<BeatInfo> beats, int startIndex, int startSample, int endSample)
        {
            int left = startIndex;
            int right = beats.Count - 1;
            int result = startIndex;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (beats[mid].Sample >= startSample)
                {
                    result = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            // 确保找到的位置在有效范围内
            return Math.Min(result, beats.Count - 1);
        }

        /// <summary>
        /// 初始化空的心率数据（当没有心搏数据时）
        /// </summary>
        private static void InitializeEmptyMinuteHR()
        {
            int sampleRate = AnaValues.holterConfig.SampleRate;
            int minuteSamples = sampleRate * 60;
            int minutes = (int)Math.Ceiling((double)AnaValues.holterConfig.Total / minuteSamples);

            lock (_lockObj)
            {
                AnaValues.minuteHR.Clear();
                AnaValues.minuteHR.Capacity = minutes;
                for (int i = 0; i < minutes; i++)
                {
                    AnaValues.minuteHR.Add(new MinuteHRInfo { Min = 0, Avg = 0, Max = 0 });
                }
            }
        }

        /// <summary>
        /// 获取导联质量评分
        /// </summary>
        public static void ResolveLeadQuality()
        {
            HolterConfig cfg = AnaValues.holterConfig;
            if (cfg.StartReanalysis == false)
            {
                AnaValues.LeadQualities = null;

                int[] leadIndices = new int[cfg.Lead];
                for (int i = 0; i < cfg.Lead; i++) { leadIndices[i] = i; }
                int[] leadCores = AEcgHandler.GetLeadQuality(leadIndices);
                if (leadCores != null)
                {
                    List<int> sortedIndex3 = leadCores.Select((value, index) => new { Value = value, Index = index })
                        .OrderByDescending(x => x.Value)
                        .Take(3).Select(x => x.Index).ToList();
                    AnaValues.LeadQualities = sortedIndex3;
                }
            }
        }

        /// <summary>
        /// 心搏是否属于无效片段
        /// </summary>
        /// <param name="beat"></param>
        /// <returns></returns>
        public static bool IsInvalidBeat(BeatInfo beat)
        {
            var unanalyzableSegments = AnaValues.UnanalyzableSegments;
            return unanalyzableSegments.Any(segment => beat.Sample >= segment.StartIndex && beat.Sample < segment.EndIndex);

            //return unanalyzableSegments.Any(segment =>
            //    beat.StartIndex < segment.EndIndex && beat.EndIndex > segment.StartIndex);
        }

        public static bool IsInvalidBeatEx(BeatInfo beat)
        {
            var unanalyzableSegments = AnaValues.UnanalyzableSegments;

            return unanalyzableSegments.Any(segment =>
                beat.StartIndex < segment.EndIndex && beat.EndIndex > segment.StartIndex);
        }
        /// <summary>
        /// 心搏是否属于无效片段
        /// </summary>
        /// <param name="beat"></param>
        /// <returns></returns>
        public static AEcgHandler.SegmentInfo isInvalidWave(int startIndex, int endIndex)
        {
            AEcgHandler.SegmentInfo invalidSegment = new AEcgHandler.SegmentInfo();
            invalidSegment.StartIndex = 0;
            invalidSegment.EndIndex = 0;
            var unanalyzableSegments = AnaValues.UnanalyzableSegments;
            if (unanalyzableSegments.Any(segment =>
                 startIndex < segment.EndIndex && endIndex > segment.StartIndex))
                foreach (AEcgHandler.SegmentInfo segment in unanalyzableSegments)
                {
                    if (startIndex < segment.EndIndex && endIndex > segment.StartIndex)
                    {
                        invalidSegment = segment;
                    }
                }
            return invalidSegment;
        }

        /// <summary>
        /// 获取所有可分析区间内的心搏
        /// </summary>
        /// <param name="beats"></param>
        /// <returns></returns>
        public static List<BeatInfo> GetValidBeats(List<BeatInfo> beats)
        {
            if (beats == null) return null;

            List<BeatInfo> result = new List<BeatInfo>(beats.Count);
            var unanalyzableSegments = AnaValues.UnanalyzableSegments;
            HashSet<int> filterSet = new HashSet<int>();
            foreach (var segment in unanalyzableSegments)
            {
                for (int i = segment.StartIndex; i < segment.EndIndex; i++)
                {
                    filterSet.Add(i);
                }
            }

            foreach (BeatInfo item in beats)
            {
                bool overlaps = filterSet.Contains(item.Sample);
                if (!overlaps)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取各心搏类型拼接的字符串
        /// </summary>
        /// <param name="beats"></param>
        /// <returns></returns>
        public static string GetKeysOfBeats(List<BeatInfo> beats)
        {
            if (beats == null || beats.Count == 0) return string.Empty;

            StringBuilder sb = new StringBuilder(beats.Count);
            var unanalyzableSegments = AnaValues.UnanalyzableSegments;
            HashSet<int> filterSet = new HashSet<int>();
            //foreach (var segment in unanalyzableSegments)
            //{
            //    for (int i = segment.StartIndex; i < segment.EndIndex; i++)
            //    {
            //        filterSet.Add(i);
            //    }
            //}

            foreach (BeatInfo item in beats)
            {
                //bool overlaps = IsInvalidBeat(item);
                bool overlaps = filterSet.Contains(item.Sample);
                if (!overlaps)
                {
                    sb.Append(item.BeatUiType);
                }
                else
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 根据心搏类型获取心搏
        /// </summary>
        /// <param name="beats"></param>
        /// <param name="beatType"></param>
        /// <returns></returns>
        public static List<BeatInfo> GetBeatsWithType(List<BeatInfo> beats, string beatType = "")
        {
            List<BeatInfo> ret = beats;
            if (!string.IsNullOrEmpty(beatType))
            {
                var cs = new Dictionary<string, string[]>
                {
                    {"N", new[] {"N"}},
                    {"NN", new[] {"NN"}},
                    {"NV", new[] {"NV"}},
                    {"NS", new[] {"NS"}},
                    {"VN", new[] {"VN"}},
                    {"SN", new[] {"SN"}},
                    {"V", new[] {"V", "T"}},
                    {"V1", new[] {"V1"}},
                    {"V2", new[] {"VV"}},
                    {"V3", new[] {"VVV"}},
                    {"S", new[] {"S"}},
                    {"S1", new[] {"S1"}},
                    {"S2", new[] {"SS"}},
                    {"S3", new[] {"SSS"}},
                    {"X", new[] {"X"}},
                    {"K", new[] {"K", "M"}},
                    {"J", new[] {"J"}},
                    {"W", new[] {"W"}},
                    {"E", new[] {"E"}},
                    {"G", new[] {"G"}},
                    {"Pace", new[] {"O","P","H"}},
                    {"O", new[] {"O"}},
                    {"P", new[] {"P"}},
                    {"H", new[] {"H"}}
                };

                if (cs.TryGetValue(beatType, out string[] c))
                {
                    string allKeys = GetKeysOfBeats(beats);
                    HashSet<BeatInfo> result = new HashSet<BeatInfo>(); // 使用 HashSet 以避免重复
                    foreach (string key in c)
                    {
                        List<BeatInfo> matchedItems = Match3(allKeys, key, beats);
                        foreach (BeatInfo item in matchedItems)
                        {
                            result.Add(item);
                        }
                    }

                    ret = result.OrderBy(b => b.Sample).ToList();
                    //ret = new List<BeatInfo>(result);
                    //ret.Sort(new Comparison<BeatInfo>((a, b) => { return b.Sample - a.Sample; }));
                }
            }

            return ret;
        }

        /// <summary>
        /// 使用正则匹配指定类型的心搏
        /// </summary>
        /// <param name="input">心搏类型连接的字符串</param>
        /// <param name="key">指定的心搏类型</param>
        /// <param name="sc">心搏信息数据源</param>
        /// <returns></returns>
        public static List<BeatInfo> Match3(string input, string key, List<BeatInfo> sc)
        {
            string matchStr;
            switch (key)
            {
                case "NN":
                    matchStr = "(?<!N)N{2,}(?!N)";
                    break;
                case "V":
                    matchStr = "V";
                    break;
                case "V1":
                    matchStr = "(?<!V)V(?!V)";
                    break;
                case "VV":
                    matchStr = "(?<!V)VV(?!V)";
                    break;
                case "VVV":
                    matchStr = "(?<!V)V{3,}(?!V)";
                    break;
                case "S":
                    matchStr = "S";
                    break;
                case "S1":
                    matchStr = "(?<!S)S(?!S)";
                    break;
                case "SS":
                    matchStr = "(?<!S)SS(?!S)";
                    break;
                case "SSS":
                    matchStr = "(?<!S)S{3,}(?!S)";
                    break;
                default:
                    matchStr = key;
                    break;
            }

            List<int> ins = new List<int>(); //心搏序号集合
            Regex regex = new Regex(matchStr, RegexOptions.Compiled);
            MatchCollection matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                int index = match.Index;

                if (key.Length == 2 && key[0] == 'N' && key[1] != 'N')
                {
                    ins.Add(index);
                }
                else if (key.Length == 2 && key[0] != 'N' && key[1] == 'N')
                {
                    ins.Add(index + 1);
                }
                else if (key == "NN")
                {
                    ins.Add(++index);
                    while (index + 1 < sc.Count && sc[index + 1].BeatUiType == 'N')
                    {
                        ins.Add(++index);
                    }
                }
                else
                {
                    ins.Add(index);
                    if (key == "VV" || key == "SS" || key == "NN")
                    {
                        ins.Add(index + 1);
                        //TODO: 判断成对室早或房早中间是否有不可分析段
                    }
                    else if (key == "VVV")
                    {
                        while (index + 1 < sc.Count && sc[index + 1].BeatUiType == 'V')
                        {
                            ins.Add(index + 1);
                            index += 1;
                        }
                    }
                    else if (key == "SSS")
                    {
                        while (index + 1 < sc.Count && sc[index + 1].BeatUiType == 'S')
                        {
                            ins.Add(index + 1);
                            index += 1;
                        }
                    }
                }
            }

            List<BeatInfo> data = new List<BeatInfo>(ins.Count);
            foreach (int i in ins) { data.Add(sc[i]); }
            return data;
        }

        /// <summary>
        /// 过滤掉长RR间期和停搏事件相关心搏
        /// </summary>
        /// <param name="srcBeats"></param>
        /// <returns></returns>
        public static List<BeatInfo> FilterOutCustomEventsBeats(List<BeatInfo> srcBeats)
        {
            List<BeatInfo> beats = srcBeats.Clone();
            //var query = beats.Where(b => true);

            //string filePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            //if (File.Exists(filePath))
            //{
            //    string jsonContent = File.ReadAllText(filePath);
            //    JObject jo = (JObject)JsonConvert.DeserializeObject(jsonContent);
            //    JArray arrLongRR = jo["长RR间期"] as JArray;
            //    if (arrLongRR != null && arrLongRR.Count > 0)
            //    {
            //        query = query.Where(b => !arrLongRR.Any(o => Convert.ToInt32(o) == b.Sample));
            //    }

            //    JArray arrAsystole = jo["停搏"] as JArray;
            //    if (arrAsystole != null && arrAsystole.Count > 0)
            //    {
            //        query = query.Where(b => !arrAsystole.Any(o => Convert.ToInt32(o) == b.Sample));
            //    }
            //}
            //return query.ToList();
            return beats;
        }

        /// <summary>
        /// 根据心搏属性特征码获取相关的心搏属性
        /// </summary>
        /// <param name="beatTypeCode"></param>
        /// <returns></returns>
        public static string[] GetQueryKeys(string beatTypeCode)
        {
            if (beatTypeCode == "**") return null;

            if (beatTypeCode.IndexOf('*') < 0)
            {
                return new string[] { beatTypeCode };
            }
            else
            {
                char[] prob = new char[] { 'N', 'L', 'S', 'J', 'G', 'W', 'K', 'M', 'V', 'T', 'E', 'F', 'P', 'O', 'H', 'X', 'Q', 'A' };
                List<string> cts = new List<string>(prob.Length);
                foreach (char key in prob)
                {
                    cts.Add(beatTypeCode.Replace('*', key));
                }
                return cts.ToArray();
            }
        }

        /// <summary>
        /// 基于指定心搏属性值获取心搏
        /// </summary>
        /// <param name="srcBeats">源心搏集合</param>
        /// <param name="keys">数据源中所有心搏的属性串联文本</param>
        /// <param name="queryKeys">要查找的心搏属性</param>
        /// <param name="isSequential">是否用作连续的窦性、室速、房速、起搏计算</param>
        /// <returns></returns>
        public static List<BeatInfo> GetValidBeatsByCondition(List<BeatInfo> srcBeats, string keys, string[] queryKeys, bool isSequential = false, int sequenceNum = 4)
        {
            if (queryKeys == null)
            {
                return srcBeats.FindAll(e => e.BeatUiType != 'X' && e.BeatUiType != 'Q');
            }

            List<BeatInfo> matchedList = new List<BeatInfo>();
            foreach (string queryKey in queryKeys)
            {
                string[] keysToMatch = GetKeysToMatch(queryKey);
                foreach (string matchKey in keysToMatch)
                {
                    if (keys.IndexOf(matchKey, StringComparison.Ordinal) >= 0)
                    {
                        matchedList.AddRange(MatchSingleCharacter(srcBeats, keys, matchKey, isSequential, sequenceNum));
                    }
                }
            }
            return matchedList.FindAll(e => e.BeatUiType != 'X' && e.BeatUiType != 'Q');
        }

        /// <summary>
        /// 匹配出某一特征的心搏
        /// </summary>
        /// <param name="srcBeats"></param>
        /// <param name="keys"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<BeatInfo> MatchSingleCharacter(List<BeatInfo> srcBeats, string keys, string key, bool isSequential = false, int sequenceNum = 4)
        {
            List<int> ins = new List<int>();
            int keysLength = keys.Length;

            int offset = 0;
            while (offset < keysLength)
            {
                int index = keys.IndexOf(key, offset, StringComparison.Ordinal);
                if (index < 0) break;

                int nextIndex = index + 1;
                if (!isSequential)
                {
                    ins.Add(nextIndex); // 取后一个心搏作为主要显示心搏
                }
                else
                {
                    int beatIndex = index;
                    if (key[0] == ('N'))
                    {
                        //beatIndex = index + sequenceNum / 2;
                        nextIndex = index + 1;
                    }
                    else
                    {
                        // n+个连续的V或S, 只算一个室速或房速
                        for (int i = index + key.Length; ; i++)
                        {
                            if (i >= keysLength || keys[i] != key[0])
                            {
                                nextIndex = i;
                                break;
                            }
                        }
                    }
                    ins.Add(beatIndex);
                }
                offset = nextIndex;
            }

            List<BeatInfo> data = new List<BeatInfo>();
            foreach (int i in ins)
            {
                if (i >= 0 && i < srcBeats.Count) // 添加边界检查
                {
                    data.Add(srcBeats[i]);
                }
            }

            return data;
        }

        /// <summary>
        /// 获取指定心搏类型连续发生的心搏个数
        /// </summary>
        /// <param name="startBeatIndex"></param>
        /// <param name="beatUiType"></param>
        /// <returns></returns>
        public static int GetSequentialLength(int startBeatIndex, char beatUiType)
        {
            int count = 0;
            List<BeatInfo> src = AnaValues.CachedAllBeats;
            for (int i = startBeatIndex; i < src.Count; i++)
            {
                if (src[i].BeatUiType != beatUiType)
                {
                    break;
                }
                count++;
            }
            return count;
        }

        /// <summary>
        /// 基于指定的心搏特征获取相关属性组合(特别针对R on T)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string[] GetKeysToMatch(string key)
        {
            string[] keysToMatch = new string[] { key };
            if (key == "NV")
            {
                // NV和NT归为一类
                keysToMatch = new string[] { "NV", "NT" };
            }
            else if (key == "VN")
            {
                // VN和TN归为一类
                keysToMatch = new string[] { "VN", "TN" };
            }
            else if (key == "RV")
            {
                // RV和RT归为一类
                keysToMatch = new string[] { "RV", "RT" };
            }
            else if (key == "VR")
            {
                // VR和TR归为一类
                keysToMatch = new string[] { "VR", "TR" };
            }
            else if (key == "VV")
            {
                // VV和TT归为一类
                keysToMatch = new string[] { "VV", "TT" };
            }

            return keysToMatch;
        }


        /// <summary>
        /// 判断指定点的位置是否在一个多边形路径内
        /// </summary>
        public static bool IsPointInPolygon(double x, double y, List<SKPoint> polygonPoints)
        {
            if (polygonPoints == null || polygonPoints.Count < 3)
            {
                return false;
            }

            bool inside = false;
            int n = polygonPoints.Count;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = polygonPoints[i].X, yi = polygonPoints[i].Y;
                double xj = polygonPoints[j].X, yj = polygonPoints[j].Y;

                // 避免除零异常
                if (Math.Abs(yj - yi) < double.Epsilon)
                {
                    continue;
                }

                // 判断点是否在边的垂直范围内
                bool intersect = ((yi > y) != (yj > y)) &&
                                (x < ((xj - xi) * (y - yi) / (yj - yi)) + xi);

                if (intersect)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// 将16进制颜色集转换为Skia支持的颜色
        /// </summary>
        /// <param name="colorMap"></param>
        /// <returns></returns>
        public static Dictionary<char, SKColor> ConvertToSkColor(Dictionary<char, string> colorMap)
        {
            Dictionary<char, SKColor> retDict = new Dictionary<char, SKColor>();
            foreach (char key in colorMap.Keys)
            {
                retDict[key] = SKColor.Parse(colorMap[key]);
            }

            return retDict;
        }

        /// <summary>
        /// 将16进制颜色集转换为GDI+支持的颜色
        /// </summary>
        /// <param name="colorMap"></param>
        /// <returns></returns>
        public static Dictionary<char, Color> ConvertToGDIColor(Dictionary<char, string> colorMap)
        {
            Dictionary<char, Color> retDict = new Dictionary<char, Color>();
            foreach (char key in colorMap.Keys)
            {
                retDict[key] = ColorTranslator.FromHtml(colorMap[key]);
            }

            return retDict;
        }

        /// <summary>
        /// 计算某段区间的平均心率
        /// </summary>
        /// <returns></returns>
        public static int CalcAvgHR(int startSample, int endSample)
        {
            List<BeatInfo> cachedAllBeats = AnaValues.CachedAllBeats;
            int sampleRate = AnaValues.holterConfig.SampleRate;

            List<BeatInfo> relevantHeartbeats = cachedAllBeats
                .Where(h => (h.EndIndex > startSample && h.StartIndex < endSample) && !AnaCode.IsInvalidBeat(h))
                .OrderBy(h => h.StartIndex)
                .ToList();

            int RRCount = 0;
            int RRSum = 0;
            int avgHR = 0;
            for (int i = 0; i < relevantHeartbeats.Count; i++)
            {
                if (relevantHeartbeats[i].BeatUiType == 'X') { continue; }

                RRSum += relevantHeartbeats[i].RRInterval;
                RRCount += 1;
            }
            if (RRCount == 0)
            {
                return 0;
            }
            else
            {
                int RRAvg = RRSum / RRCount;
                avgHR = 60 * 1000 / RRAvg;
                return avgHR;
            }
        }

        /// <summary>
        /// 计算指定心搏(R波采样点)相关的平均RR间期
        /// </summary>
        /// <param name="srcBeats">心拍数据源</param>
        /// <param name="refBeat">指定的心拍R波位置</param>
        /// <param name="referBeatNum">计算平均心率参考的心搏个数</param>
        /// <param name="calNum">实际参与计算的心搏数</param>
        /// <param name="backward">是否反向遍历</param>
        /// <param name="beatMap">R波采样点与序号的映射集</param>
        /// <returns></returns>
        public static double CalcAvgRR(List<BeatInfo> srcBeats, BeatInfo refBeat, int referBeatNum, out int calNum, bool backward = false, Dictionary<int, int> beatMap = null)
        {
            int sumRR = 0;
            calNum = 0;

            if (beatMap == null) beatMap = GetBeatMap(srcBeats);

            double avgRR = refBeat.RRInterval;

            if (beatMap.TryGetValue(refBeat.Sample, out int index))
            {
                if (backward)
                {
                    for (int j = index; j >= 0 && calNum < referBeatNum; j--)
                    {
                        BeatInfo beat = srcBeats[j];
                        if (beat.BeatUiType == 'X' || beat.RRInterval < 0) //遇到伪差或不可分析段，重新开始计数
                        {
                            index = j;
                            sumRR = 0;
                            calNum = 0;
                            continue;
                        }
                        sumRR += beat.RRInterval;
                        calNum++;
                    }
                }
                else
                {
                    for (int j = index; j < srcBeats.Count && calNum < referBeatNum; j++)
                    {
                        BeatInfo beat = srcBeats[j];
                        if (beat.BeatUiType == 'X' || beat.RRInterval < 0) //遇到伪差或不可分析段，重新开始计数
                        {
                            index = j;
                            sumRR = 0;
                            calNum = 0;
                            continue;
                        }
                        sumRR += beat.RRInterval;
                        calNum++;
                    }
                }

                if (calNum > 0) avgRR = (double)sumRR / calNum;
            }

            return avgRR;
        }

        /// <summary>
        /// 计算心搏相关提前量
        /// </summary>
        /// <param name="beat"></param>
        /// <returns></returns>
        public static int CalValueAhead(BeatInfo beat, Dictionary<int, int> beatMap = null)
        {
            int calcValue = 0;
            if (beat.RRInterval > 0)
            {
                double avgRR = CalcAvgRR(AnaValues.CachedAllBeats, beat, 4, out int count, true, beatMap);
                //int count4RR = 0;
                //int breakCount = 0;
                //int calC = 4;
                //for (int j = beat.BeatIndex; j > 0 && count < calC; j--)
                //{
                //    //if (breakCount == 10)
                //    //{
                //    //    break;
                //    //}
                //    char beatType = AnaValues.cachedAllBeats[j - 1].BeatUiType;
                //    if (beatType == 'X' || AnaCode.CheckBeatIsSegmentUnanalyzable(AnaValues.cachedAllBeats[j - 1]))
                //    {
                //        //breakCount++;
                //        continue;
                //    }
                //    count4RR += AnaValues.cachedAllBeats[j - 1].RRInterval;
                //    count++;
                //}
                //if (count == 4)
                //{
                //    double avgRR = Math.Round((double)count4RR / count, MidpointRounding.AwayFromZero);
                //    calcValue = (int)(((avgRR - beat.RRInterval) / avgRR) * 100);
                //    return calcValue;
                //}
                if (count == 4)
                {
                    calcValue = (int)((avgRR - beat.RRInterval) / avgRR * 100);
                }
            }

            return calcValue;
        }

        /// <summary>
        /// 获取心拍的R波采样点与序号的映射集合
        /// </summary>
        /// <param name="srcBeats"></param>
        /// <returns></returns>
        public static Dictionary<int, int> GetBeatMap(List<BeatInfo> srcBeats)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>(srcBeats.Count);
            for (int i = 0; i < srcBeats.Count; i++)
            {
                dict[srcBeats[i].Sample] = i;
            };
            return dict;
        }


        /// <summary>
        /// 获取文件中存储的最快心率、最慢心率、最长RR间期值
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public static int GetHREventValue(string eventName)
        {
            int value = 0;
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath))
            {
                try
                {
                    //string jsonContent = File.ReadAllText(deletedEventsFilePath);
                    string jsonContent = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);

                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        JObject jsonData = JObject.Parse(jsonContent);
                        if (jsonData.ContainsKey(eventName))
                        {
                            value = Convert.ToInt32(jsonData[eventName]["value"].ToString());
                        }
                    }
                }
                catch { }
            }

            return value;
        }

        /// <summary>
        /// 将已保存的最快心率、最慢心率、最长RR间期写入到本地文件中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sample"></param>
        /// <param name="value"></param>
        public static void WriteSaveInfo(string key, int sample, int hr, double rr)
        {
            JObject jsonObject;
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath) && new FileInfo(deletedEventsFilePath).Length > 0)
            {
                //string existingJson = File.ReadAllText(deletedEventsFilePath);
                string existingJson = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);
                jsonObject = JObject.Parse(existingJson);
            }
            else
            {
                jsonObject = new JObject();
            }

            JObject infoObj = new JObject();
            infoObj["sample"] = sample;
            infoObj["value"] = hr;
            infoObj["RR"] = rr;
            // 添加或更新字段
            jsonObject[key] = infoObj;

            FileOperationsWithMutex.SafeWriteToFile(jsonObject.ToString(), AnaValues.DeleteEventFileName, deletedEventsFilePath);
            //File.WriteAllText(deletedEventsFilePath, jsonObject.ToString());
        }

        /// <summary>
        /// 从本地文件中将已保存的最快心率、最慢心率、最长RR间期删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sample"></param>
        /// <param name="value"></param>
        /// </summary>
        public static void DeleteSaveInfo(string key)
        {
            JObject jsonObject;
            string deletedEventsFilePath = Path.Combine(AnaValues.RootPath, AnaValues.DeleteEventFileName);
            if (File.Exists(deletedEventsFilePath) && new FileInfo(deletedEventsFilePath).Length > 0)
            {
                //string existingJson = File.ReadAllText(deletedEventsFilePath);
                string existingJson = FileOperationsWithMutex.SafeReadFromFile(AnaValues.DeleteEventFileName, deletedEventsFilePath);
                jsonObject = JObject.Parse(existingJson);
                jsonObject.Remove(key);
                File.WriteAllText(deletedEventsFilePath, jsonObject.ToString());
            }

        }

        /// <summary>
        /// 从内存读取指定导联的一段心电波形数据
        /// </summary>
        public static WaveInfo ReadWaveData(int leadIndex, int startIndex, int endIndex)
        {
            SegmentRange[] seg = new SegmentRange[]
            {
                new SegmentRange(startIndex, endIndex)
            };
            WaveInfo[] waveInfos = AEcgHandler.GetWaveData(leadIndex, seg);
            return waveInfos[0];
        }

        /// <summary>
        /// 读取多个导联的数据
        /// </summary>
        /// <param name="leadIdxs"></param>
        /// <param name="startSample"></param>
        /// <param name="endSample"></param>
        /// <returns></returns>
        public static List<WaveInfo> ReadAllWaveData(List<int> leadIdxs, int startSample, int endSample)
        {
            List<WaveInfo> ecgData = new List<WaveInfo>();
            for (int n = 0; n < AnaValues.holterConfig.Lead; n++)
            {
                WaveInfo waveInfo = new WaveInfo();
                if (leadIdxs.Contains(n))
                {
                    waveInfo = AnaCode.ReadWaveData(n, startSample, endSample);
                    if (waveInfo != null)
                    {
                        waveInfo.WaveDataF = Array.ConvertAll(waveInfo.WaveData, x => x / 1000F);
                    }
                }
                ecgData.Add(waveInfo);
            }

            return ecgData;
        }

        /// <summary>
        /// 获取房颤/房扑信息列表
        /// </summary>
        /// <returns></returns>
        public static List<AfInfo> GetAfList()
        {
            DateTime gStartTime = AnaValues.holterConfig.StartDateTime;
            int sampleRate = AnaValues.holterConfig.SampleRate;
            List<AfInfo> afs = new List<AfInfo>();
            var lstAF = AEcgHandler.GetAfSegments('K');
            foreach (AEcgHandler.SegmentInfo af in lstAF)
            {
                int validSamples = CalcValidSamples(af.StartIndex, af.EndIndex);
                afs.Add(new AfInfo()
                {
                    RefBeatType = 'K',
                    TypeName = AnaValues.AfTypes['K'],
                    StartSample = af.StartIndex,
                    EndSample = af.EndIndex,
                    StartTime = gStartTime.AddSeconds((double)af.StartIndex / sampleRate),
                    EndTime = gStartTime.AddSeconds((double)af.EndIndex / sampleRate),
                    Duration = (double)validSamples / sampleRate,
                });
            }
            var lstAFL = AEcgHandler.GetAfSegments('M');
            foreach (AEcgHandler.SegmentInfo afl in lstAFL)
            {
                int validSamples = CalcValidSamples(afl.StartIndex, afl.EndIndex);

                afs.Add(new AfInfo()
                {
                    RefBeatType = 'M',
                    TypeName = AnaValues.AfTypes['M'],
                    StartSample = afl.StartIndex,
                    EndSample = afl.EndIndex,
                    StartTime = gStartTime.AddSeconds((double)afl.StartIndex / sampleRate),
                    EndTime = gStartTime.AddSeconds((double)afl.EndIndex / sampleRate),
                    Duration = (double)validSamples / sampleRate,

                });
            }

            return afs.OrderBy(o => o.StartTime).ToList();
        }

        /// <summary>
        /// 查找当前心拍位置与房颤或房扑的关联性类型名称
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="afs"></param>
        /// <returns></returns>
        public static char GetAfTypeName(int sample, List<AfInfo> afs)
        {
            char TypeName = 'N';
            foreach (AfInfo af in afs)
            {
                int startSample = getSampleFromDateTime(af.StartTime);
                int endSample = getSampleFromDateTime(af.EndTime);
                if (sample >= startSample && sample <= endSample)
                {
                    TypeName = af.RefBeatType;
                    break;
                }
            }
            return TypeName;
        }

        /// <summary>
        /// 根据时间范围获取对应的心电数据片段区间
        /// </summary>
        public static SegmentRange GetSegment(DateTime startTime, DateTime endTime)
        {
            HolterConfig cfg = AnaValues.holterConfig;
            int startSample = (int)(startTime.Subtract(cfg.StartDateTime).TotalSeconds * cfg.SampleRate);
            int endSample = (int)(endTime.Subtract(cfg.StartDateTime).TotalSeconds * cfg.SampleRate);
            return new SegmentRange(startSample, endSample);
        }

        /// <summary>
        /// 检验某个心搏是否属于不可以分析段
        /// </summary>
        /// <param name="beat"></param>
        /// <returns>返回true表示心搏在不可分析段内（部分或全部），false表示心搏在可分析段内</returns>
        public static bool CheckBeatIsSegmentUnanalyzable(BeatInfo beat)
        {
            bool bFlag = false;

            // 查找与心搏有重叠的不可分析段
            List<AEcgHandler.SegmentInfo> overlappingSegments = AnaValues.UnanalyzableSegments
                .Where(h => h.EndIndex > beat.StartIndex && h.StartIndex < beat.EndIndex)
                .ToList();

            if (overlappingSegments.Count > 0)
            {
                bFlag = true;
            }

            return bFlag;
        }

        /// <summary>
        /// 查找前一个心搏
        /// </summary>
        /// <returns>前一个心搏及其序号</returns>
        public static AEcgHandler.AEcgBeatInfo findPreviousBeat(List<AEcgHandler.AEcgBeatInfo> aecgBeats, int curIndex, out int previousIndex)
        {
            previousIndex = -1;
            var thisBeat = aecgBeats[curIndex];
            if (curIndex > 0)
            {
                for (int i = curIndex - 1; i >= 0; i--)
                {
                    //if (thisBeat.beatUiType != 88) //非伪差
                    //{
                    //    if (aecgBeats[i].beatUiType != 88)
                    //    {
                    //        previousIndex = i;
                    //        return aecgBeats[i];
                    //    }
                    //}
                    //else
                    //{
                    previousIndex = i;
                    return aecgBeats[i];
                    //}
                }
            }
            return default(AEcgHandler.AEcgBeatInfo);
        }

        /// <summary>
        /// 查找后一个心搏
        /// </summary>
        /// <returns>后一个心搏及其序号</returns>
        public static AEcgHandler.AEcgBeatInfo findLatterBeat(List<AEcgHandler.AEcgBeatInfo> aecgBeats, int curIndex, out int latterIndex)
        {
            latterIndex = -1;
            var thisBeat = aecgBeats[curIndex];
            if (curIndex < aecgBeats.Count - 1)
            {
                for (int i = curIndex + 1; i < aecgBeats.Count; i++)
                {
                    //if (thisBeat.beatUiType != 88) //非伪差
                    //{
                    //    if (aecgBeats[i].beatUiType != 88)
                    //    {
                    //        latterIndex = i;
                    //        return aecgBeats[i];
                    //    }
                    //}
                    //else
                    //{
                    latterIndex = i;
                    return aecgBeats[i];
                    //}
                }
            }
            return default(AEcgHandler.AEcgBeatInfo);
        }

        /// <summary>
        /// 使用二分法查找当前采样点之前的心拍序号
        /// </summary>
        /// <param name="refSample"></param>
        /// <returns></returns>
        public static int FindPreviousBeatIndex(int refSample)
        {
            int left = 0, right = AnaValues.CachedAllBeats.Count;
            while (left < right)
            {
                int mid = left + (right - left) / 2;
                if (AnaValues.CachedAllBeats[mid].Sample.CompareTo(refSample) < 0)
                    left = mid + 1;
                else
                    right = mid;
            }
            return left;
        }

        /// <summary>
        /// 使用二分法查找当前采样点之后的心拍序号
        /// </summary>
        public static int FindLatterBeatIndex(int refSample)
        {
            int left = 0, right = AnaValues.CachedAllBeats.Count;
            while (left < right)
            {
                int mid = left + (right - left) / 2;
                if (AnaValues.CachedAllBeats[mid].Sample.CompareTo(refSample) <= 0)
                    left = mid + 1;
                else
                    right = mid;
            }
            return left;
        }

        /// <summary>
        /// 取中值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Median(List<short> data)
        {
            List<short> sorted = data.OrderBy(x => x).ToList();
            int count = sorted.Count;
            if (count == 0) return 0;

            return count % 2 == 1
                ? sorted[count / 2]
                : (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }

        /// <summary>
        /// 获取报告中HRV相关信息
        /// </summary>
        public static HRVReportInfo GetHRVReportInfo(int? hrvCalcResult = null)
        {
            HRVReportInfo hrvReportInfo = null;
            int calcResult = 0;
            if (hrvCalcResult.HasValue)
                calcResult = hrvCalcResult.Value;
            else
                calcResult = AEcgHandler.CalculateHRV();

            if (calcResult == 0)
            {
                List<AEcgHandler.AEcgHrvTimeDomainInfo> aEcgHrvTimeDomainList = AEcgHandler.GetHrvTimeDomain();
                List<AEcgHandler.AEcgHrvFreqDomainInfo> aEcgHrvFreqDomainList = AEcgHandler.GetHrvFreqDomain();
                if (aEcgHrvTimeDomainList.Count > 0 && aEcgHrvFreqDomainList.Count > 0)
                {
                    AEcgHandler.AEcgHrvTimeDomainInfo aEcgHrvTimeDomain = aEcgHrvTimeDomainList[0];
                    AEcgHandler.AEcgHrvFreqDomainInfo aEcgHrvFreqDomain = aEcgHrvFreqDomainList[0];

                    hrvReportInfo = new HRVReportInfo();

                    hrvReportInfo.sdnn = aEcgHrvTimeDomain.sdnn == -100 ? "-" : aEcgHrvTimeDomain.sdnn.ToString();
                    hrvReportInfo.sdann = aEcgHrvTimeDomain.sdann == -100 ? "-" : aEcgHrvTimeDomain.sdann.ToString();
                    hrvReportInfo.sdnnIndex = aEcgHrvTimeDomain.sdnn_index == -100 ? "-" : aEcgHrvTimeDomain.sdnn_index.ToString();
                    hrvReportInfo.pnn50 = aEcgHrvTimeDomain.pnn50 == -100f ? "-" : (Math.Round((double)aEcgHrvTimeDomain.pnn50, 2)).ToString();
                    hrvReportInfo.rmssd = aEcgHrvTimeDomain.rmssd == -100 ? "-" : aEcgHrvTimeDomain.rmssd.ToString();
                    hrvReportInfo.hrvTriangularIndex = aEcgHrvTimeDomain.hrv_triangular_index == -100f ? "-" : (Math.Round((double)aEcgHrvTimeDomain.hrv_triangular_index, 2)).ToString();

                    hrvReportInfo.total_power = (Math.Round((double)aEcgHrvFreqDomain.total_power, 2)).ToString();
                    hrvReportInfo.ulf = (Math.Round((double)aEcgHrvFreqDomain.ulf, 2)).ToString();
                    hrvReportInfo.vlf = (Math.Round((double)aEcgHrvFreqDomain.vlf, 2)).ToString();
                    hrvReportInfo.lf = (Math.Round((double)aEcgHrvFreqDomain.lf, 2)).ToString();
                    hrvReportInfo.hf = (Math.Round((double)aEcgHrvFreqDomain.hf, 2)).ToString();

                }

            }
            return hrvReportInfo;
        }

        public static STReportInfo GetSTReportInfo()
        {
            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";
            STReportInfo stReprotInfo = new STReportInfo();
            List<AEcgHandler.AEcgStEventInfo> stEventInfoList = AEcgHandler.GetStEventInfo();
            if (stEventInfoList != null && stEventInfoList.Count > 0)
            {
                int lowCount = 0;
                int upCount = 0;
                float lowMax = 0;
                float upMax = 0;
                DateTime lowTime = new DateTime();
                DateTime upTime = new DateTime();
                int lowLead = 0;
                int upLead = 0;
                double totalMinute = 0d;
                for (int i = 0; i < stEventInfoList.Count; i++)
                {
                    float StPointVoltage = (float)Math.Round(stEventInfoList[i].stPointVoltage / 1000, 2);
                    if (StPointVoltage < 0)
                    {
                        ++lowCount;
                        if (Math.Abs(StPointVoltage) > Math.Abs(lowMax))
                        {
                            lowMax = StPointVoltage;
                            lowLead = stEventInfoList[i].lead;
                            lowTime = getDataTimeFromSample(stEventInfoList[i].startPosition);
                        }
                    }
                    else if (StPointVoltage > 0)
                    {
                        ++upCount;
                        if (StPointVoltage > upMax)
                        {
                            upMax = StPointVoltage;
                            upLead = stEventInfoList[i].lead;
                            upTime = getDataTimeFromSample(stEventInfoList[i].startPosition);
                        }
                    }

                    totalMinute += (double)stEventInfoList[i].durationTime / AnaValues.holterConfig.SampleRate / 60;
                }

                stReprotInfo.stTotalDuration = Math.Round(totalMinute, 2).ToString("F2");
                stReprotInfo.stLowCount = lowCount;
                stReprotInfo.stLowest = lowMax.ToString();
                stReprotInfo.stLowestTime = lowTime.ToString("HH:mm:ss");
                stReprotInfo.stLowestTimeLong = lowTime.ToString(dateFormat);
                stReprotInfo.stLowestLead = AnaValues.LeadIdxToNameMap[lowLead];

                stReprotInfo.stUpCount = upCount;
                stReprotInfo.stUpest = upMax.ToString();
                stReprotInfo.stUpestTime = upTime.ToString("HH:mm:ss");
                stReprotInfo.stUpestTimeLong = upTime.ToString(dateFormat);
                stReprotInfo.stUpestLead = AnaValues.LeadIdxToNameMap[upLead];

                int lowDuration = 0;
                int upDuration = 0;
                for (int i = 0; i < stEventInfoList.Count; i++)
                {
                    float StPointVoltage = (float)Math.Round(stEventInfoList[i].stPointVoltage / 1000, 2);
                    int lead = stEventInfoList[i].lead;
                    if (StPointVoltage < 0 && lead == lowLead)
                    {
                        lowDuration += stEventInfoList[i].durationTime;
                    }
                    else if (StPointVoltage > 0 && lead == upLead)
                    {
                        upDuration += stEventInfoList[i].durationTime;
                    }
                }
                stReprotInfo.stLowLeadDuration = (Math.Round((double)lowDuration / AnaValues.holterConfig.SampleRate / 60, 2)).ToString();
                stReprotInfo.stUpLeadDuration = (Math.Round((double)upDuration / AnaValues.holterConfig.SampleRate / 60, 2)).ToString();
            }
            return stReprotInfo;
        }

        public static AFReportInfo GetAfReportInfo()
        {
            AFReportInfo afReportInfo = new AFReportInfo();
            int totalBeats = AnaEvent.TotalBeatCount;
            int AfSamples = 0;
            int afBeats = 0;
            var lstAF = AEcgHandler.GetAfSegments('K');
            foreach (AEcgHandler.SegmentInfo af in lstAF)
            {
                AfSamples += CalcValidSamples(af.StartIndex, af.EndIndex);
                afBeats += AnaValues.CachedAllBeats.Count(b => b.EndIndex >= af.StartIndex && b.StartIndex < af.EndIndex);
            }

            int AflSamples = 0;
            int aflBeats = 0;
            var lstAFL = AEcgHandler.GetAfSegments('M');
            foreach (AEcgHandler.SegmentInfo afl in lstAFL)
            {
                //AflSamples += (afl.EndIndex - afl.StartIndex + 1);
                AflSamples += CalcValidSamples(afl.StartIndex, afl.EndIndex);
                aflBeats += AnaValues.CachedAllBeats.Count(b => b.EndIndex >= afl.StartIndex && b.StartIndex < afl.EndIndex);

            }
            double afDurationSeconds = (double)AfSamples / AnaValues.holterConfig.SampleRate;
            double afDurationSeconds2 = (double)AflSamples / AnaValues.holterConfig.SampleRate;
            double totalSeconds = (double)(AfSamples + AflSamples) / AnaValues.holterConfig.SampleRate;

            afReportInfo.AFDuration = FormatTimeSpanTo24Hour(TimeSpan.FromSeconds(afDurationSeconds));
            afReportInfo.AFDuration2 = FormatTimeSpanTo24Hour(TimeSpan.FromSeconds(afDurationSeconds2));
            afReportInfo.AFTotalDuration = FormatTimeSpanTo24Hour(TimeSpan.FromSeconds(totalSeconds));


            afReportInfo.AFCount = lstAF.Count;
            afReportInfo.AFCount2 = lstAFL.Count;

            int segmentUnanalyzableTotal = 0;
            for (int i = 0; i < AnaValues.UnanalyzableSegments.Count; i++)
            {
                segmentUnanalyzableTotal += AnaValues.UnanalyzableSegments[i].EndIndex - AnaValues.UnanalyzableSegments[i].StartIndex;
            }
            int totalAnaSeconds = (AnaValues.holterConfig.Total - segmentUnanalyzableTotal) / AnaValues.holterConfig.SampleRate;

            afReportInfo.AFPercent = (Math.Round((double)afBeats / AnaValues.CachedAllBeats.Count * 100, 2)).ToString();
            afReportInfo.AFPercent2 = (Math.Round((double)aflBeats / AnaValues.CachedAllBeats.Count * 100, 2)).ToString();

            return afReportInfo;
        }

        /// <summary>
        /// 计算 [startIdx, endIdx) 区间内可分析的有效时长（单位：秒）。
        /// 当区间与 AnaValues.UnanalyzableSegments 中的元素有交集时，
        /// 扣除交集部分的采样点计数；多个不可分析段互相重叠时只扣除一次。
        /// 区间采用左闭右开，与 IsInvalidBeat / GetValidBeats 中的约定一致。
        /// </summary>
        /// <param name="startIdx">起始采样点序号（含）</param>
        /// <param name="endIdx">结束采样点序号（不含）</param>
        /// <returns>扣除不可分析段后的有效时长（秒）</returns>
        public static int CalcValidSamples(int startIdx, int endIdx)
        {
            // 采样点序号不能为负；区间无效时直接返回 0
            startIdx = Math.Max(0, startIdx);
            if (endIdx <= startIdx) return 0;

            // 1. 取出与 [startIdx, endIdx) 有交集的不可分析段，并裁剪到该区间内
            List<SegmentRange> overlaps = new List<SegmentRange>();
            var unanalyzableSegments = AnaValues.UnanalyzableSegments;
            if (unanalyzableSegments != null)
            {
                foreach (var segment in unanalyzableSegments)
                {
                    int overlapStart = Math.Max(startIdx, segment.StartIndex);
                    int overlapEnd = Math.Min(endIdx, segment.EndIndex);
                    if (overlapStart < overlapEnd)
                    {
                        overlaps.Add(new SegmentRange(overlapStart, overlapEnd));
                    }
                }
            }

            // 2. 合并相交/相邻的交集片段，避免同一采样点被重复扣除
            int invalidSamples = 0;
            if (overlaps.Count > 0)
            {
                overlaps.Sort((a, b) => a.StartIndex.CompareTo(b.StartIndex));
                int mergedStart = overlaps[0].StartIndex;
                int mergedEnd = overlaps[0].EndIndex;
                for (int i = 1; i < overlaps.Count; i++)
                {
                    if (overlaps[i].StartIndex <= mergedEnd)
                    {
                        mergedEnd = Math.Max(mergedEnd, overlaps[i].EndIndex);
                    }
                    else
                    {
                        invalidSamples += mergedEnd - mergedStart;
                        mergedStart = overlaps[i].StartIndex;
                        mergedEnd = overlaps[i].EndIndex;
                    }
                }
                invalidSamples += mergedEnd - mergedStart;
            }

            // 3. 总采样点计数扣除交集部分后，再按采样率换算成秒
            int validSamples = (endIdx - startIdx) - invalidSamples;

            return validSamples;
        }

        public static string FormatSeconds(double totalSeconds)
        {
            var ts = TimeSpan.FromSeconds(totalSeconds);
            if (ts.Days >= 1)
            {
                return $"{ts.Days * 24:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
            else
            {
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }

        public static double TryHmsToSeconds(string hms)
        {
            // 不能直接用 TimeSpan.TryParse：它会把 "24:00:00" 的前段当成天数（24 天 = 576 小时），
            // 这里统一按 "HH:mm:ss" 分段解析（小时位可大于 24）
            if (TimeDurationBox.TryParseDuration(hms, double.MaxValue, out TimeSpan ts))
            {
                return ts.TotalSeconds;
            }
            return 0;
        }

        public static PaceReportInfo GetPaceReportInfo(string beatSequence)
        {
            PaceReportInfo paceReportInfo = new PaceReportInfo();

            int apCount = AnaEvent.CalcPaceCount(beatSequence, 'O');
            int vpCount = AnaEvent.CalcPaceCount(beatSequence, 'P');
            int avpCount = AnaEvent.CalcPaceCount(beatSequence, 'H');
            int totalCount = apCount + vpCount + avpCount;

            paceReportInfo.APCount = apCount;
            paceReportInfo.VPCount = vpCount;
            paceReportInfo.AVPCount = avpCount;
            paceReportInfo.TotalCount = totalCount;

            int totalBeats = AnaEvent.TotalBeatCount;
            paceReportInfo.TotalPercent = Math.Round((double)totalCount / totalBeats * 100, 2);
            paceReportInfo.APPercent = Math.Round((double)apCount / totalBeats * 100, 2);
            paceReportInfo.VPPercent = Math.Round((double)vpCount / totalBeats * 100, 2);
            paceReportInfo.AVPPercent = Math.Round((double)avpCount / totalBeats * 100, 2);

            return paceReportInfo;
        }

        /// <summary>
        /// 获取血压测量时的运动状态
        /// </summary>
        /// <param name="deviceType">监测仪设备类型</param>
        /// <param name="motionState">运动状态标识代码</param>
        /// <returns></returns>
        public static string GetMotionStateText(string deviceType, string motionState)
        {
            string motionStateStr = "";
            if (deviceType == "3") //8337
            {
                switch (motionState)
                {
                    case "0":
                        motionStateStr = CommonValues.Langs["静止"];
                        break;
                    case "1":
                        motionStateStr = CommonValues.Langs["轻微活动"];
                        break;
                    case "2":
                        motionStateStr = CommonValues.Langs["运动"];
                        break;
                    case "3":
                        motionStateStr = CommonValues.Langs["剧烈运动"];
                        break;
                    default:
                        motionStateStr = "--";
                        break;
                }
            }
            else if (deviceType == "4") //8377
            {
                switch (motionState)
                {
                    case "0":
                        motionStateStr = "--";
                        break;
                    case "1":
                        motionStateStr = CommonValues.Langs["静止"];
                        break;
                    case "2":
                        motionStateStr = CommonValues.Langs["运动"];
                        break;
                    case "3":
                        motionStateStr = CommonValues.Langs["剧烈运动"];
                        break;
                    default:
                        motionStateStr = "--";
                        break;
                }
            }

            return motionStateStr;
        }

        /// <summary>
        /// 获取直方图相关心搏数据
        /// </summary>
        /// <param name="histInfo">直方图信息</param>
        /// <returns></returns>
        public static List<BeatInfo> QueryHistogramData(HistogramInfo histInfo)
        {
            int earlyRate = AnaValues.holterConfig.ParamsSetting.P19.Before;
            List<BeatInfo> srcBeats = FilterOutCustomEventsBeats(AnaValues.CachedAllBeats);
            srcBeats = GetValidBeats(srcBeats);
            Dictionary<int, int> beatMap = GetBeatMap(srcBeats);

            string allKeys = AnaCode.GetKeysOfBeats(srcBeats);
            string keys = allKeys;
            if (histInfo.BeatsTypeCode.IndexOf('P') >= 0) keys = allKeys.Replace('O', 'P').Replace('H', 'P');

            string[] queryKeys = AnaCode.GetQueryKeys(histInfo.BeatsTypeCode);
            bool isSequential = (histInfo.ChartType == HistogramType.AverageHR && histInfo.TypeId != "RRavg");
            int refNum = AnaValues.holterConfig.ParamsSetting.P14.Frequency;
            List<BeatInfo> sc = AnaCode.GetValidBeatsByCondition(srcBeats, keys, queryKeys, isSequential, refNum);

            List<BeatInfo> result = new List<BeatInfo>();
            int calNum = 0;
            if (histInfo.ChartType == HistogramType.RRInterval) //间期
            {
                if (histInfo.TypeId == "RR_Long")
                {
                    int longRr = AnaValues.holterConfig.ParamsSetting.P8.Rr;
                    result = sc.FindAll(b => b.RRInterval > longRr).Select(b => new BeatInfo(b) { State = b.RRInterval }).ToList();
                }
                else
                {
                    result = sc.Select(b => new BeatInfo(b) { State = b.RRInterval }).ToList();
                }
            }
            else if (histInfo.ChartType == HistogramType.AverageHR) //平均心率
            {
                if (histInfo.TypeId == "NNNavg") { if (refNum % 2 == 0) refNum += 1; }
                foreach (BeatInfo beat in sc)
                {
                    if (histInfo.TypeId == "SSSavg" || histInfo.TypeId == "VVVavg")
                    {
                        refNum = AnaCode.GetSequentialLength(beat.BeatIndex, beat.BeatUiType);
                    }
                    double avgRR = AnaCode.CalcAvgRR(srcBeats, beat, refNum, out calNum, false, beatMap);
                    int avgHr = (int)(60 * 1000 / avgRR);
                    result.Add(new BeatInfo(beat) { State = avgHr });
                }
            }
            else if (histInfo.ChartType == HistogramType.EarlyRate) //提早率
            {
                if (histInfo.TypeId == "S2") //房早提早率
                {
                    var earlyrates = from item in sc
                                         //let avgRR = avg4RR(srcBeats, beatMap[item.Sample])
                                     let avgRR = AnaCode.CalcAvgRR(srcBeats, item, 4, out calNum, true, beatMap)
                                     select new
                                     {
                                         item,
                                         avgRR,
                                         rate = (int)((double)(avgRR - item.RRInterval) / avgRR * 100)
                                     };
                    result = earlyrates.Where(o => o.rate >= earlyRate).Select(o => new BeatInfo(o.item) { State = o.rate }).ToList();
                }
                else
                {
                    result = (from item in sc
                                  //let avgRR = avg4RR(srcBeats, beatMap[item.Sample])
                              let avgRR = AnaCode.CalcAvgRR(srcBeats, item, 4, out calNum, true, beatMap)
                              select new BeatInfo(item)
                              {
                                  State = (int)((double)(avgRR - item.RRInterval) / avgRR * 100)
                              }).ToList();
                }
            }
            else if (histInfo.ChartType == HistogramType.RRIntervalRatio) //间期比
            {
                foreach (BeatInfo beat in sc)
                {
                    int index = beatMap[beat.Sample];
                    int firstIndex = index - 1;
                    int endIndex = firstIndex + (histInfo.BeatsTypeCode.Length - 1);
                    int preIndex = endIndex - 1;
                    int ratio = 100;
                    while (preIndex >= 0)
                    {
                        if (srcBeats[preIndex].BeatUiType == 'X') { preIndex--; continue; }
                        ratio = (int)((float)srcBeats[endIndex].RRInterval / srcBeats[preIndex].RRInterval * 100);
                        break;
                    }
                    result.Add(new BeatInfo(beat) { State = ratio });
                }
            }

            return result;
        }

        public static string FormatTimeSpanTo24Hour(TimeSpan ts)
        {
            // 获取总秒数（包含小数）
            double totalSeconds = ts.TotalSeconds;

            // 四舍五入到最近的秒数
            double roundedSeconds = Math.Round(totalSeconds, MidpointRounding.AwayFromZero);

            // 如果四舍五入后达到 86400 秒（24小时）
            if (roundedSeconds >= 86400)
            {
                return "24:00:00";
            }

            // 否则格式化为 HH:mm:ss
            TimeSpan roundedTs = TimeSpan.FromSeconds(roundedSeconds);
            return roundedTs.ToString(@"hh\:mm\:ss");
        }

        public static List<string> WrapText(string text, float maxWidth, SKFont font)
        {
            List<string> lines = new List<string>();

            if (string.IsNullOrEmpty(text))
            {
                return lines;
            }

            // 尝试按空格分割，如果没有空格则按字符分割
            if (text.Contains(' '))
            {
                // 有空格，按单词换行
                string[] words = text.Split(' ');
                string currentLine = "";

                foreach (string word in words)
                {
                    string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                    if (font.MeasureText(testLine) <= maxWidth)
                    {
                        currentLine = testLine;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currentLine))
                        {
                            lines.Add(currentLine);
                        }

                        currentLine = word;
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                }
            }
            else
            {
                // 无空格，按字符换行
                string currentLine = "";

                foreach (char ch in text)
                {
                    string testLine = currentLine + ch;
                    if (font.MeasureText(testLine) <= maxWidth)
                    {
                        currentLine = testLine;
                    }
                    else
                    {
                        lines.Add(currentLine);
                        currentLine = ch.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                }
            }

            return lines;
        }

        public static SKBitmap Base64ToBitmap(string base64)
        {
            // 1. 去除可能存在的 Data URL 前缀（如 "data:image/jpeg;base64,"）
            if (base64.Contains(","))
            {
                base64 = base64.Substring(base64.IndexOf(',') + 1);
            }

            // 2. Base64 解码为字节数组
            byte[] imageBytes = Convert.FromBase64String(base64);

            // 3. 直接从字节数组解码为 SKBitmap
            SKBitmap bitmap = SKBitmap.Decode(imageBytes);

            return bitmap;
        }
        public static Dictionary<string, TemplateFunctionInfo> GetTemplateFunctions()
        {
            int totalSeconds = AnaValues.holterConfig.Total / AnaValues.holterConfig.SampleRate;
            DateTime endTime = AnaValues.holterConfig.StartDateTime.AddSeconds(totalSeconds);
            TimeSpan recordTime = TimeSpan.FromSeconds(totalSeconds);
            string recordTimeStr = (int)recordTime.TotalHours + CommonValues.Langs["小时"] + recordTime.Minutes + CommonValues.Langs["分钟"];


            int segmentUnanalyzableTotal = 0;
            for (int i = 0; i < AnaValues.UnanalyzableSegments.Count; i++)
            {
                segmentUnanalyzableTotal += AnaValues.UnanalyzableSegments[i].EndIndex - AnaValues.UnanalyzableSegments[i].StartIndex;
            }
            int totalAnaSeconds = (AnaValues.holterConfig.Total - segmentUnanalyzableTotal) / AnaValues.holterConfig.SampleRate;
            TimeSpan AnaTime = TimeSpan.FromSeconds(totalAnaSeconds);
            string anaTimeStr = (int)AnaTime.TotalHours + CommonValues.Langs["小时"] + AnaTime.Minutes + CommonValues.Langs["分钟"];

            int maxHR = AnaEvent.AllEvents["最快心率"].Count > 0 ? AnaEvent.AllEvents["最快心率"][0].AvgHr : 0;
            string maxHRTime = AnaEvent.AllEvents["最快心率"].Count > 0 ? AnaEvent.AllEvents["最快心率"][0].StartTime.Split(' ')[1] : "";
            int minHR = AnaEvent.AllEvents["最慢心率"].Count > 0 ? AnaEvent.AllEvents["最慢心率"][0].AvgHr : 0;
            string minHRTime = AnaEvent.AllEvents["最慢心率"].Count > 0 ? AnaEvent.AllEvents["最慢心率"][0].StartTime.Split(' ')[1] : "";

            int asystoleCount = 0;
            double longestAsystoleDuration = 0;
            string longestAsystoleTime = "";
            if (AnaEvent.AllEvents["停搏"].Count > 0)
            {
                asystoleCount = AnaEvent.AllEvents["停搏"].Count;
                longestAsystoleDuration = AnaEvent.AllEvents["停搏"][0].Duration;
                longestAsystoleTime = AnaEvent.AllEvents["停搏"][0].StartTime;
            }

            double longestRR = 0;
            string longestRRTime = "";
            if (AnaEvent.AllEvents["最长RR间期"].Count > 0)
            {
                longestRR = Math.Round((double)AnaEvent.AllEvents["最长RR间期"][0].AvgRR / 1000d, 2);
                longestRRTime = AnaEvent.AllEvents["最长RR间期"][0].StartTime.Split(' ')[1];
            }

            int longRRCount = AnaEvent.AllEvents.ContainsKey("长RR间期") ? AnaEvent.AllEvents["长RR间期"].Count : 0;
            int longRRSet = CommonValues.SystemSettings.ecgSets.customSets.longRR;

            int totalBeatsV = AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'V') + AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'T');
            double precentV = Math.Round((double)totalBeatsV / AnaEvent.TotalBeatCount * 100, 2);
            int singleV = AnaEvent.AllEvents.ContainsKey("单发室早") ? AnaEvent.AllEvents["单发室早"].Count : 0;
            int pairedV = AnaEvent.AllEvents.ContainsKey("成对室早") ? AnaEvent.AllEvents["成对室早"].Count : 0;
            int bigeminyV = AnaEvent.AllEvents.ContainsKey("室性二联律") ? AnaEvent.AllEvents["室性二联律"].Count : 0;
            int trigeminyV = AnaEvent.AllEvents.ContainsKey("室性三联律") ? AnaEvent.AllEvents["室性三联律"].Count : 0;
            int tachycardiaV = AnaEvent.AllEvents.ContainsKey("室速") ? AnaEvent.AllEvents["室速"].Count : 0;
            int longestTachycardiaV = AnaEvent.AllEvents["室速"].Count > 0 ? AnaEvent.AllEvents["室速"][0].BeatSize : 0;
            string longestTachycardiaVTime = AnaEvent.AllEvents["室速"].Count > 0 ? AnaEvent.AllEvents["室速"][0].StartTime : "0";
            int escapeV = AnaEvent.AllEvents.ContainsKey("室性逸搏") ? AnaEvent.AllEvents["室性逸搏"].Count : 0;
            int RonT = AnaEvent.AllEvents.ContainsKey("R on T") ? AnaEvent.AllEvents["R on T"].Count : 0;

            int totalBeatsS = AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'S');
            double precentS = Math.Round((double)totalBeatsS / AnaEvent.TotalBeatCount * 100, 2);
            int singleS = AnaEvent.AllEvents.ContainsKey("单发房早") ? AnaEvent.AllEvents["单发房早"].Count : 0;
            int pairedS = AnaEvent.AllEvents.ContainsKey("成对房早") ? AnaEvent.AllEvents["成对房早"].Count : 0;
            int bigeminyS = AnaEvent.AllEvents.ContainsKey("房性二联律") ? AnaEvent.AllEvents["房性二联律"].Count : 0;
            int trigeminyS = AnaEvent.AllEvents.ContainsKey("房性三联律") ? AnaEvent.AllEvents["房性三联律"].Count : 0;
            int tachycardiaS = AnaEvent.AllEvents.ContainsKey("房速") ? AnaEvent.AllEvents["房速"].Count : 0;
            int longestTachycardiaS = AnaEvent.AllEvents["房速"].Count > 0 ? AnaEvent.AllEvents["房速"][0].BeatSize : 0;
            string longestTachycardiaSTime = AnaEvent.AllEvents["房速"].Count > 0 ? AnaEvent.AllEvents["房速"][0].StartTime : "0";
            int escapeS = AnaEvent.AllEvents.ContainsKey("房性逸搏") ? AnaEvent.AllEvents["房性逸搏"].Count : 0;
            int escapeG = AnaEvent.AllEvents.ContainsKey("交界性逸搏") ? AnaEvent.AllEvents["交界性逸搏"].Count : 0;


            STReportInfo stReportInfo = GetSTReportInfo();
            int stLowCount = stReportInfo.stLowCount;
            string stLowest = stReportInfo.stLowest;
            string stLowestTime = stReportInfo.stLowestTime;
            string stLowestLead = stReportInfo.stLowestLead;
            double stLowLeadDuration = 0d;
            if (!string.IsNullOrEmpty(stReportInfo.stLowLeadDuration))
            {
                stLowLeadDuration = double.Parse(stReportInfo.stLowLeadDuration);
            }
            int stUpCount = stReportInfo.stUpCount;
            string stUpest = stReportInfo.stUpest;
            string stUpestTime = stReportInfo.stUpestTime;
            string stUpestLead = stReportInfo.stUpestLead;
            double stUpLeadDuration = 0d;
            if (!string.IsNullOrEmpty(stReportInfo.stUpLeadDuration))
            {
                stUpLeadDuration = double.Parse(stReportInfo.stUpLeadDuration);
            }
            string stDuration = (stLowLeadDuration + stUpLeadDuration).ToString();

            AFReportInfo afReportInfo = GetAfReportInfo();
            string afTotalDuration = afReportInfo.AFTotalDuration;
            string afPercent = afReportInfo.AFPercent;
            string afDuration = afReportInfo.AFDuration;
            int afCount = afReportInfo.AFCount;
            string afPercent2 = afReportInfo.AFPercent2;
            string afDuration2 = afReportInfo.AFDuration2;
            int afCount2 = afReportInfo.AFCount2;

            List<BeatInfo> beatInfoList = AnaValues.CachedAllBeats;
            string beatSequence = AnaEvent.GetBeatSequence(beatInfoList);
            PaceReportInfo paceReportInfo = GetPaceReportInfo(beatSequence);
            int apCount = paceReportInfo.APCount;
            int vpCount = paceReportInfo.VPCount;
            int avpCount = paceReportInfo.AVPCount;
            double apPercent = paceReportInfo.APPercent;
            double vpPercent = paceReportInfo.VPPercent;
            double avpPercent = paceReportInfo.AVPPercent;

            int percepCount = 0;        //感知失败
            int outputCount = 0;        //输出失败
            int captureCount = 0;       //夺获失败
            List<AEcgHandler.AEcgPacemakerEvaluation> paceMakerList = AEcgHandler.GetPacemakerEvaluationInfo();
            for (int i = 0; i < paceMakerList.Count; i++)
            {
                //0-感知失败，1-输出失败，2-夺获失败
                switch (paceMakerList[i].failureType)
                {
                    case 0:
                        ++percepCount;
                        break;
                    case 1:
                        ++outputCount;
                        break;
                    case 2:
                        ++captureCount;
                        break;
                }
            }


            string SDNN = "";
            string SDANN = "";
            string SDANNIndex = "";
            string rMSSD = "";
            string pNN50 = "";
            string Triindex = "";
            string TP = "";
            string VLF = "";
            string LF = "";
            string HF = "";
            string lfhf = "";
            int hrvCalcResult = AEcgHandler.CalculateHRV();
            HRVReportInfo hrvReportInfo = AnaCode.GetHRVReportInfo(hrvCalcResult);
            if (hrvReportInfo != null)
            {
                SDNN = hrvReportInfo.sdnn;
                SDANN = hrvReportInfo.sdann;
                SDANNIndex = hrvReportInfo.sdnnIndex;
                rMSSD = hrvReportInfo.rmssd;
                pNN50 = hrvReportInfo.pnn50;
                Triindex = hrvReportInfo.hrvTriangularIndex;
                TP = hrvReportInfo.total_power;
                VLF = hrvReportInfo.vlf;
                LF = hrvReportInfo.lf;
                HF = hrvReportInfo.hf;
                lfhf = Math.Round(Convert.ToDouble(hrvReportInfo.lf) / Convert.ToDouble(hrvReportInfo.hf), 1).ToString();
            }

            Dictionary<string, TemplateFunctionInfo> functions = new Dictionary<string, TemplateFunctionInfo>();
            functions["<$TotalBeats$>"] = new TemplateFunctionInfo(CommonValues.Langs["总心搏数"], AnaEvent.TotalBeatCount.ToString(), CommonValues.Langs["个"]);
            functions["<$StartTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["起始时间"], AnaValues.holterConfig.StartTime, "");
            functions["<$EndTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["终止时间"], endTime.ToString("yyyy-MM-dd HH:mm:ss"), "");
            functions["<$RecordingDuration$>"] = new TemplateFunctionInfo(CommonValues.Langs["记录时长"], recordTimeStr, "");
            functions["<$AnalysisDuration$>"] = new TemplateFunctionInfo(CommonValues.Langs["分析时长"], anaTimeStr, "");
            functions["<$AnalysisTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["分析时间"], AnaValues.CurrentCase.uploadTime.Value.ToString(), "");
            functions["<$HRMax$>"] = new TemplateFunctionInfo(CommonValues.Langs["最快心率"], maxHR.ToString(), "");
            functions["<$HRMaxat$>"] = new TemplateFunctionInfo(CommonValues.Langs["最快心率发生时间"], maxHRTime, "");
            functions["<$HRMin$>"] = new TemplateFunctionInfo(CommonValues.Langs["最慢心率"], minHR.ToString(), "");
            functions["<$HRMinat$>"] = new TemplateFunctionInfo(CommonValues.Langs["最慢心率发生时间"], minHRTime, "");
            functions["<$HRAvg$>"] = new TemplateFunctionInfo(CommonValues.Langs["全程平均心率"], AnaEvent.TotalAvgHR.ToString(), "");
            functions["<$MAXRR$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长RR间期"], longestRR.ToString("F2"), CommonValues.Langs["秒"]);
            functions["<$MAXRRAT$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长RR间期发生时间"], longestRRTime, "");
            functions["<$LongRR.Count$>"] = new TemplateFunctionInfo(CommonValues.Langs["长RR间期次数"], longRRCount.ToString(), CommonValues.Langs["次"]);
            functions["<$LongRR.Thred$>"] = new TemplateFunctionInfo(CommonValues.Langs["长RR间期阈值"], longRRSet.ToString(), CommonValues.Langs["次"]);

            functions["<$Asystole$>"] = new TemplateFunctionInfo(CommonValues.Langs["停搏次数"], asystoleCount.ToString(), CommonValues.Langs["次"]);
            functions["<$AsystoleDuration$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长停搏"], longestAsystoleDuration.ToString(), CommonValues.Langs["秒"]);
            functions["<$AsystoleTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长停搏发生时间"], longestAsystoleTime, "");

            functions["<$SVCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性早搏个数"], totalBeatsS.ToString(), CommonValues.Langs["个"]);
            functions["<$SVPercent$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性早搏总占比"], precentS.ToString(), "");
            functions["<$SVISOCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["单发房早个数"], singleS.ToString(), CommonValues.Langs["个"]);
            functions["<$SVCoupleCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["成对房早个数"], pairedS.ToString(), CommonValues.Langs["阵"]);
            functions["<$SVBIGCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性二联律"], bigeminyS.ToString(), CommonValues.Langs["阵"]);
            functions["<$SVTRIGCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性三联律"], trigeminyS.ToString(), CommonValues.Langs["阵"]);
            functions["<$SVRUNCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["房速"], tachycardiaS.ToString(), CommonValues.Langs["阵"]);
            functions["<$SVRUNLongest$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长房速个数"], longestTachycardiaS.ToString(), CommonValues.Langs["次/阵"]);
            functions["<$SVRUNLongestat$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长房速发生时间"], longestTachycardiaSTime, "");

            functions["<$PVCCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性早搏个数"], totalBeatsV.ToString(), CommonValues.Langs["个"]);
            functions["<$PVCPercent$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性早搏总占比"], precentV.ToString(), "");
            functions["<$PVCISOCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["单发室早个数"], singleV.ToString(), CommonValues.Langs["个"]);
            functions["<$PVCCoupleCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["成对室早个数"], pairedV.ToString(), CommonValues.Langs["阵"]);
            functions["<$PVCBigCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性二联律"], bigeminyV.ToString(), CommonValues.Langs["阵"]);
            functions["<$PVCTrigCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性三联律"], trigeminyV.ToString(), CommonValues.Langs["阵"]);
            functions["<$PVCRunCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["室速"], tachycardiaV.ToString(), CommonValues.Langs["阵"]);
            functions["<$PVCRunLongest$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长室速个数"], longestTachycardiaV.ToString(), CommonValues.Langs["次/阵"]);
            functions["<$PVCRunLongestat$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长室速发生时间"], longestTachycardiaVTime, "");
            functions["<$RonTPrematureCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["R on T早搏个数"], RonT.ToString(), "");

            functions["<$AEsc$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性逸搏"], escapeS.ToString(), "");
            functions["<$NEsc$>"] = new TemplateFunctionInfo(CommonValues.Langs["交界性逸搏个数"], escapeG.ToString(), "");
            functions["<$VEsc$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性逸搏"], escapeV.ToString(), "");

            functions["<$STMAX$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大抬高"], stUpest, "");
            functions["<$STMAXAT$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大抬高发生时间"], stUpestTime, "");
            functions["<$STMAXCH$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大抬高发生通道"], stUpestLead, "");
            functions["<$STMIN$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大压低"], stLowest, "");
            functions["<$STMINAT$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大压低发生时间"], stLowestTime, "");
            functions["<$STMINCH$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大压低发生通道"], stLowestLead, "");
            functions["<$STALLTIME$>"] = new TemplateFunctionInfo(CommonValues.Langs["异常ST段总时间"], stDuration, CommonValues.Langs["分"]);

            functions["<$AF.Keep$>"] = new TemplateFunctionInfo(CommonValues.Langs["房颤房扑持续时间"], afTotalDuration, "");
            functions["<$AFIB.QRSPERCENT$>"] = new TemplateFunctionInfo(CommonValues.Langs["房颤负荷"], afPercent.ToString(), "%");
            functions["<$AFIB.Keep$>"] = new TemplateFunctionInfo(CommonValues.Langs["房颤持续时间"], afDuration, "");
            functions["<$AFIB.Times$>"] = new TemplateFunctionInfo(CommonValues.Langs["房颤阵数"], afCount.ToString(), CommonValues.Langs["阵"]);
            functions["<$AFLUT.QRSPERCENT$>"] = new TemplateFunctionInfo(CommonValues.Langs["房扑负荷"], afPercent2.ToString(), "%");
            functions["<$AFLUT.Keep$>"] = new TemplateFunctionInfo(CommonValues.Langs["房扑持续时间"], afDuration2, "");
            functions["<$AFLUT.Times$>"] = new TemplateFunctionInfo(CommonValues.Langs["房扑阵数"], afCount2.ToString(), CommonValues.Langs["阵"]);

            functions["<$PM.APaced$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性起搏数"], apCount.ToString(), CommonValues.Langs["次"]);
            functions["<$PM.APacedPer$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性起搏总占比"], apPercent.ToString(), "%");
            functions["<$PM.VPaced$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性起搏数"], vpCount.ToString(), CommonValues.Langs["次"]);
            functions["<$PM.VPacedPer$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性起搏总占比"], vpPercent.ToString(), "%");
            functions["<$PM.AVPaced$>"] = new TemplateFunctionInfo(CommonValues.Langs["房室顺序起搏数"], avpCount.ToString(), CommonValues.Langs["次"]);
            functions["<$PM.AVPacedPer$>"] = new TemplateFunctionInfo(CommonValues.Langs["房室顺序起搏总占比"], avpPercent.ToString(), "%");
            functions["<$PM.FTS$>"] = new TemplateFunctionInfo(CommonValues.Langs["起搏感知失败次数"], percepCount.ToString(), CommonValues.Langs["次"]);
            functions["<$PM.FTO$>"] = new TemplateFunctionInfo(CommonValues.Langs["起搏输出失败次数"], outputCount.ToString(), CommonValues.Langs["次"]);
            functions["<$PM.FTC$>"] = new TemplateFunctionInfo(CommonValues.Langs["起搏夺获失败次数"], captureCount.ToString(), CommonValues.Langs["次"]);
            functions["<$HRVSDNN$>"] = new TemplateFunctionInfo(CommonValues.Langs["R-R间期标准差"], SDNN, "ms");
            functions["<$HRVSDANN$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均R-R间期标准差"], SDANN, "ms");
            functions["<$HRVSDNNIndex$>"] = new TemplateFunctionInfo(CommonValues.Langs["5min R-R间期标准差的均值"], SDANNIndex, "ms");
            functions["<$HRVSDANN$>"] = new TemplateFunctionInfo(CommonValues.Langs["相邻R-R间期差值均方根"], rMSSD, "ms");
            functions["<$HRVPNN50$>"] = new TemplateFunctionInfo(CommonValues.Langs["相邻R-R间期差值"], pNN50, "%");
            functions["<$HRVTRIGIndex$>"] = new TemplateFunctionInfo(CommonValues.Langs["HRV三角指数"], Triindex, "");
            functions["<$HRVLF$>"] = new TemplateFunctionInfo(CommonValues.Langs["极低频功率"], VLF, "ms²");
            functions["<$HRVLFNORM$>"] = new TemplateFunctionInfo(CommonValues.Langs["低频功率"], LF, "ms²");
            functions["<$HRVHF$>"] = new TemplateFunctionInfo(CommonValues.Langs["高频功率"], HF, "ms²");
            functions["<$HRVLFHF$>"] = new TemplateFunctionInfo(CommonValues.Langs["低频功率/高频功率"], lfhf, "");

            return functions;
        }
    }

    public static class AppExtensions
    {
        public static void Check(this ToolStripItem tsItem)
        {
            ToolStripItemCollection siblings = null;
            if (tsItem.OwnerItem != null)
            {
                var parentItem = tsItem.OwnerItem as ToolStripDropDownItem;
                siblings = parentItem.DropDownItems;
            }
            else
            {
                var ctxMenuStrip = tsItem.GetCurrentParent() as ToolStrip;
                if (ctxMenuStrip != null)
                {
                    siblings = ctxMenuStrip.Items;
                }
                else if (tsItem.Owner != null)
                {
                    siblings = tsItem.Owner.Items;
                }
            }

            if (siblings != null)
            {
                foreach (ToolStripItem item in siblings)
                {
                    ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                    if (item == tsItem)
                    {
                        var parentItem = item.OwnerItem as IScatterToolStripItem;
                        if (parentItem != null)
                        {
                            parentItem.ZqScatterType.Title = parentItem.ZqScatterType.Title.TrimEnd('\'') + "'";
                            parentItem.UserSelected = true;
                        }
                        item.Image = Properties.Resources.ok16;
                        if (menuItem != null) menuItem.Checked = true;
                    }
                    else
                    {
                        item.Image = null;
                        if (menuItem != null) menuItem.Checked = false;
                    }
                }
            }
        }

        public static void CheckChildItem(this ToolStripDropDownItem dropdownItem, Predicate<ToolStripItem> predicate)
        {
            foreach (ToolStripItem item in dropdownItem.DropDownItems)
            {
                ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                if (predicate(item))
                {
                    var parentItem = item.OwnerItem as IScatterToolStripItem;
                    if (parentItem != null)
                    {
                        parentItem.ZqScatterType.Title = parentItem.ZqScatterType.Title.TrimEnd('\'') + "'";
                        parentItem.UserSelected = true;
                    }
                    item.Image = Properties.Resources.ok16;
                    if (menuItem != null) menuItem.Checked = true;
                }
                else
                {
                    item.Image = null;
                    if (menuItem != null) menuItem.Checked = false;
                }
            }
        }

        public static List<BeatInfo> ConvertToSystemBeats(this List<AEcgHandler.AEcgBeatInfo> aecgBeats, int sampleRate)
        {
            List<BeatInfo> sysBeats = new List<BeatInfo>();
            if (aecgBeats != null) sysBeats.Capacity = aecgBeats.Count;

            int toMs(int sample) { return (sample < 0 ? -1 : Convert.ToInt32((double)sample / sampleRate * 1000)); }

            //Func<int, int> getRR = (curIndex) =>
            //{
            //    var thisBeat = aecgBeats[curIndex];
            //    if (thisBeat.rrDuration < 0)
            //    {
            //        var previousBeat = AnaCode.findPreviousBeat(aecgBeats, curIndex, out int previousIndex);
            //        if (previousIndex >= 0)
            //            return thisBeat.rWavePos - previousBeat.rWavePos;
            //        else
            //            return thisBeat.rrDuration; //-1
            //    }
            //    else
            //    {
            //        return thisBeat.rrDuration;
            //    }
            //};
            int getRR2(AEcgHandler.AEcgBeatInfo theBeat, int previousIndex)
            {
                if (previousIndex >= 0)
                {
                    //if (aecgBeats[previousIndex].beatUiType == 'X') return -1;

                    //將不可分析段后的第一个心搏RR间期置为-1
                    foreach (var segment in AnaValues.UnanalyzableSegments)
                    {
                        if (segment.StartIndex > aecgBeats[previousIndex].startIdx && segment.EndIndex < theBeat.endIdx)
                        {
                            return -1;
                        }
                    }
                }
                return toMs(theBeat.rrDuration);
            }

            for (int i = 0; i < aecgBeats.Count; i++)
            {
                AEcgHandler.AEcgBeatInfo srcBeat = aecgBeats[i];
                short formType = srcBeat.morphoType; //扩展心搏形态值,使不同心搏类型的值不重复
                if (formType >= 0)
                    formType = (short)(srcBeat.beatUiType * 100 + formType);
                else
                    formType = (short)(srcBeat.beatUiType * -100);

                BeatInfo beat = new BeatInfo(srcBeat.rWavePos, 0, (char)srcBeat.beatUiType, srcBeat.morphoType)
                {
                    BeatIndex = i,
                    StartIndex = srcBeat.startIdx,
                    EndIndex = srcBeat.endIdx,
                    MorphologyTypeEx = formType
                };

                var previousBeat = AnaCode.findPreviousBeat(aecgBeats, i, out int previousIndex);
                var latterBeat = AnaCode.findLatterBeat(aecgBeats, i, out int latterIndex);
                if (i == 0)
                {
                    beat.RRInterval = -1;
                }
                else
                {
                    //beat.RRInterval = toMs(getRR(i));
                    beat.RRInterval = getRR2(srcBeat, previousIndex);
                }

                //beat.RRPrevious = previousIndex >= 0 ? toMs(getRR(previousIndex)) : -1;
                //beat.RRLatter = latterIndex >= 0 ? toMs(getRR(latterIndex)) : -1;
                beat.RRPrevious = previousIndex >= 0 ? sysBeats[i - 1].RRInterval : -1;
                beat.RRLatter = latterIndex >= 0 ? getRR2(aecgBeats[latterIndex], i) : -1;

                beat.StartIndex = Math.Max(0, beat.StartIndex);
                //beat.EndIndex = Math.Min(AnaValues.holterConfig.Total - 1, beat.EndIndex);

                sysBeats.Add(beat);
            }

            return sysBeats;
        }

    }

}
