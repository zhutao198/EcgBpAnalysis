using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Zoncare.Common;
using Zoncare.DAL;

namespace AnalysisApp.Commons
{
    public class SegmentInfo
    {
        public string cbxLabel = "ECG";                 //片段图标签
        public string tbxStartTime = "";                //片段图起始时间
        public int cbxDuration = 7;                     //片段图持续时间,单位秒
        public int startSample = 1000;                  //片段图起始采样点
        public string tbxAvgHR = "80";                  //片段图前7秒的平均心率
        public int cbxGain = 10;                        //片段图显示和打印增益
        public int cbxDirection = 0;                    //片段图打印方向
        public List<int> leadIdxs = new List<int>();    //片段图显示和打印的导联索引
    }

    public static class OperateSegment
    {
        public static void EventSegmentExist(string segmentFilePath, List<AnaEvent.EventInfo> EventList, string eventName)
        {
            // 从segmentFilePath文件中查找并删除startSample相同的JSON对象
            if (File.Exists(segmentFilePath))
            {
                try
                {
                    // 读取现有JSON数组
                    string jsonContent = File.ReadAllText(segmentFilePath);
                    List<JObject> jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

                    for (int i = 0; i < EventList.Count; i++)
                    {
                        bool bExist = false;

                        for (int j = 0; j < jsonArray.Count; j++)
                        {
                            JObject obj = jsonArray[j];
                            int objSample = obj["sample"]?.Value<int>() ?? 0;
                            string label = obj["cbxLabel"].ToString();
                            if (objSample == EventList[i].Sample && label == eventName)
                            {
                                bExist = true;
                                break;
                            }
                        }
                        EventList[i].HasPic = bExist;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"处理{ AnaValues.SegmentFileName}文件时发生错误: {ex.Message}");
                }
            }
        }

        public static JObject SegmentExistByName(string segmentFilePath, string eventName)
        {
            JObject segmentObj = null;
            if (File.Exists(segmentFilePath))
            {
                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                List<JObject> jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

                for (int j = 0; j < jsonArray.Count; j++)
                {
                    JObject obj = jsonArray[j];
                    string label = obj["cbxLabel"].ToString();
                    if (label == eventName)
                    {
                        segmentObj = obj;
                        break;
                    }
                }
            }

            return segmentObj;
        }

        public static bool SegmentExistByNameSample(string segmentFilePath, string eventName, int eventSample)
        {
            bool bFlag = false;
            if (File.Exists(segmentFilePath))
            {
                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                List<JObject> jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

                for (int j = 0; j < jsonArray.Count; j++)
                {
                    JObject obj = jsonArray[j];
                    int segmentSample = obj["sample"]?.Value<int>() ?? 0;
                    string segmentEventName = obj["eventName"]?.Value<string>() ?? "";
                    string segmentLabel = obj["cbxLabel"]?.Value<string>() ?? "";

                    if (segmentSample == eventSample && segmentEventName == eventName)
                    {
                        bFlag = true;
                        break;
                    }

                }
            }

            return bFlag;
        }

        /// <summary>
        /// 处理holter.pic文件：判断文件是否存在，更新或新增JSON对象
        /// </summary>
        /// <param name="segmentInfoJson">当前段信息JSON对象</param>
        public static void ProcessHolterPicFile(JObject segmentInfoJson)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                string segmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(segmentFilePath));

                List<JObject> jsonArray = new List<JObject>();

                // 判断文件是否存在
                if (File.Exists(segmentFilePath))
                {
                    // 文件存在，读取现有JSON数组
                    string jsonContent = File.ReadAllText(segmentFilePath);
                    jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();
                }

                // 获取当前segmentInfoJson的sample值
                int currentSample = segmentInfoJson["startSample"]?.Value<int>() ?? 0;
                string eventName = segmentInfoJson["eventName"].ToString();

                // 查找是否存在相同sample的JSON对象
                JObject existingObject = null;
                int existingIndex = -1;

                for (int i = 0; i < jsonArray.Count; i++)
                {
                    JObject obj = jsonArray[i];
                    if(eventName == "最长RR间期" || eventName == "最快心率" || eventName == "最慢心率")
                    {
                        if ((eventName == obj["eventName"]?.ToString()))
                        {
                            existingObject = obj;
                            existingIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        int objSample = obj["startSample"]?.Value<int>() ?? 0;
                        if (objSample == currentSample && (eventName == obj["eventName"]?.ToString()))
                        {
                            existingObject = obj;
                            existingIndex = i;
                            break;
                        }
                    }

                    //if ((eventName== obj["cbxLabel"].ToString() && obj["cbxLabel"].ToString() == "最长RR间期")
                    //    || (eventName == obj["eventName"]?.ToString()))
                    //{
                    //    existingObject = obj;
                    //    existingIndex = i;
                    //    break;
                    //}
                }

                if (existingObject != null)
                {
                    // 存在相同sample的对象，更新属性
                    foreach (JProperty property in segmentInfoJson.Properties())
                    {
                        existingObject[property.Name] = property.Value;
                    }
                    jsonArray[existingIndex] = existingObject;
                }
                else
                {
                    // 不存在相同sample的对象，新增一个对象
                    jsonArray.Add(segmentInfoJson);
                }

                // 将更新后的JSON数组写回文件
                string updatedJson = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(segmentFilePath, updatedJson);

                object para = new { AnaValues.CurrentCase.caseNo };
                OperateLog.logParam("保存片段图", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);


            }
            catch (Exception ex)
            {
                // 处理异常，可以记录日志或显示错误信息
                Console.WriteLine($"处理{ AnaValues.SegmentFileName}文件时发生错误: {ex.Message}");
            }
        }

        public static void deleteSegment(string segmentFilePath, List<int> startSamples)
        {
            // 从segmentFilePath文件中查找并删除startSample相同的JSON对象
            if (File.Exists(segmentFilePath))
            {
                Stopwatch sw = Stopwatch.StartNew();

                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                List<JObject> jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

                for(int i=0; i< startSamples.Count; i++)
                {
                    // 查找并删除匹配的JSON对象
                    JObject itemToRemove = jsonArray.Find(item =>
                        item["startSample"]?.Value<int>() == startSamples[i]);

                    if (itemToRemove != null)
                    {
                        jsonArray.Remove(itemToRemove);
                    }
                    else
                    {
                        continue;
                    }

                    string eventName = itemToRemove["eventName"]?.ToString();
                    //if (eventName == "最快心率" || eventName == "最慢心率" || eventName == "最长RR间期")
                    //{
                    //    AnaCode.DeleteSaveInfo(eventName);
                    //}
                    if(eventName == "最快心率")
                    {
                        AnaEvent.GetFastestSlowestHrEvent(AnaValues.CachedAllBeats, AnaEvent.EventType.fastestHr);
                    }
                    else if(eventName == "最慢心率")
                    {
                        AnaEvent.GetFastestSlowestHrEvent(AnaValues.CachedAllBeats, AnaEvent.EventType.slowestHr);
                    }
                    else if (eventName == "最长RR间期")
                    {
                        AnaEvent.GetLongestRREvent(AnaValues.CachedAllBeats, AnaEvent.EventType.longestRR);
                    }
                }

                // 将更新后的JSON数组写回文件
                string updatedJson = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(segmentFilePath, updatedJson);

                object para = new { AnaValues.CurrentCase.caseNo, startSamples };
                OperateLog.logParam("通过startSample删除片段图", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
            }
        }

        public static void deleteSegmentByEventName(string eventName)
        {
            string segmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(segmentFilePath));
            // 从segmentFilePath文件中查找并删除startSample相同的JSON对象
            if (File.Exists(segmentFilePath))
            {
                Stopwatch sw = Stopwatch.StartNew();

                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                List<JObject> jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

                // 查找并删除匹配的JSON对象
                JObject itemToRemove = jsonArray.FirstOrDefault(item =>
                    item["eventName"]?.Value<string>() == eventName);

                if (itemToRemove != null)
                {
                    jsonArray.Remove(itemToRemove);
                }

                // 将更新后的JSON数组写回文件
                string updatedJson = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(segmentFilePath, updatedJson);

                object para = new { AnaValues.CurrentCase.caseNo, eventName };
                OperateLog.logParam("通过事件名称删除片段图", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
            }

        }

        public static void deleteSegmentBySample(string segmentFilePath, List<int> samples)
        {
            // 从segmentFilePath文件中查找并删除startSample相同的JSON对象
            if (File.Exists(segmentFilePath))
            {
                Stopwatch sw = Stopwatch.StartNew();

                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                List<JObject> jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

                for (int i = 0; i < samples.Count; i++)
                {
                    // 查找并删除匹配的JSON对象
                    JObject itemToRemove = jsonArray.FirstOrDefault(item =>
                        item["sample"]?.Value<int>() == samples[i]);

                    if (itemToRemove != null)
                    {
                        jsonArray.Remove(itemToRemove);
                    }
                }

                // 将更新后的JSON数组写回文件
                string updatedJson = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                File.WriteAllText(segmentFilePath, updatedJson);

                object para = new { AnaValues.CurrentCase.caseNo, samples };
                OperateLog.logParam("通过sample删除片段图", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);

            }
        }

        public static List<JObject> getAllSegmentObj(string segmentFilePath)
        {
            List<JObject> SegmentInfoJsons = new List<JObject>();
            // 从segmentFilePath文件中查找并删除startSample相同的JSON对象
            if (File.Exists(segmentFilePath))
            {
                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                SegmentInfoJsons = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();
            }

            return SegmentInfoJsons;
        }

        public static List<JObject> getSegmentObj(string segmentFilePath, List<int> StartSamples)
        {
            List<JObject> SegmentInfoJsons = new List<JObject>();
            // 从segmentFilePath文件中查找并删除startSample相同的JSON对象
            if (File.Exists(segmentFilePath))
            {
                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                List<JObject> jsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();
                for (int i = 0; i < StartSamples.Count; i++)
                {

                    int startSample = StartSamples[i];
                    // 查找并删除匹配的JSON对象
                    JObject segmentJsonObj = jsonArray.FirstOrDefault(item =>
                        item["startSample"]?.Value<int>() == startSample);

                    if (segmentJsonObj != null)
                    {
                        SegmentInfoJsons.Add(segmentJsonObj);
                    }
                }
            }

            return SegmentInfoJsons;
        }

        public static List<JObject> getSegmentPageInfo(List<JObject> SegmentInfoJsons)
        {
            List<JObject> SegmentPageInfos = new List<JObject>();

            int totalPages = 0;
            int remainingRows = 0; // 当前页面剩余行数
            int previousPrintDirection = -1; // 上一个片段图的打印方向

            for (int i = 0; i < SegmentInfoJsons.Count; i++)
            {
                JObject segmentInfoJson = SegmentInfoJsons[i];
                string segmentLabel = segmentInfoJson["cbxLabel"]?.ToString();
                int printDirection = Convert.ToInt32(segmentInfoJson["cbxDirection"]);
                int printSeconds = Convert.ToInt32(segmentInfoJson["cbxDuration"]);
                List<int> leadIdxList = segmentInfoJson["leadIdxs"].ToObject<List<int>>() ?? new List<int>();
                int leadCount = leadIdxList.Count;
                int segmentRowSeconds = printDirection == 1 ? 7 : 10;
                int eachLeadRows = (printSeconds + segmentRowSeconds - 1) / segmentRowSeconds; // 向上取整
                List<int> startSamples = new List<int>();
                for (int row = 0; row < eachLeadRows; row++)
                {
                    int startSample = Convert.ToInt32(segmentInfoJson["startSample"]) + (row * segmentRowSeconds * AnaValues.holterConfig.SampleRate);
                    startSamples.Add(startSample);
                }
                segmentInfoJson["startSamples"] = JToken.FromObject(startSamples);
                // 确定每页最大行数和每导联最大行数
                int maxRowsPerPage = leadCount < 6 ? 10 : leadCount;

                int totalSegmentRows = eachLeadRows * leadCount;
                // 检查打印方向是否发生变化
                bool directionChanged = i > 0 && printDirection != previousPrintDirection;

                JObject segmentPageInfo = new JObject();
                int beginPageIndex;

                // 关键逻辑：确保每个导联在同一张纸张上完整绘制
                // 如果打印方向发生变化，或者当前页面剩余行数不足以容纳当前片段图的所有导联
                if (directionChanged || remainingRows == 0 || remainingRows < leadCount)
                {
                    // 需要新页面开始绘制当前片段图
                    beginPageIndex = totalPages;

                    if (leadCount < 6)
                    {
                        // 节约纸张模式：计算需要的页数
                        int segmentPages = (totalSegmentRows + maxRowsPerPage - 1) / maxRowsPerPage;
                        totalPages += segmentPages;

                        // 计算剩余行数
                        remainingRows = maxRowsPerPage - (totalSegmentRows % maxRowsPerPage);
                        if (remainingRows == maxRowsPerPage)
                        {
                            remainingRows = 0;
                        }
                    }
                    else
                    {
                        // 不节约纸张模式：每个片段图独立占用页面
                        int segmentPages = (totalSegmentRows + maxRowsPerPage - 1) / maxRowsPerPage;
                        totalPages += segmentPages;
                        remainingRows = 0; // 不保留剩余行数
                    }
                }
                else
                {
                    // 当前片段图可以继续在当前页面绘制（打印方向相同且剩余行数足够）
                    beginPageIndex = totalPages - 1; // 从上一页继续

                    // 可以继续在当前页面绘制
                    int rowsUsedInCurrentPage = Math.Min(remainingRows / leadCount * leadCount, totalSegmentRows);
                    remainingRows -= rowsUsedInCurrentPage;
                    totalSegmentRows -= rowsUsedInCurrentPage;

                    // 如果还有剩余行数需要绘制，需要新页面
                    if (totalSegmentRows > 0)
                    {
                        int additionalPages = (totalSegmentRows + maxRowsPerPage - 1) / maxRowsPerPage;
                        totalPages += additionalPages;
                        remainingRows = maxRowsPerPage - (totalSegmentRows % maxRowsPerPage);
                        if (remainingRows == maxRowsPerPage)
                        {
                            remainingRows = 0;
                        }
                    }
                }

                int endPageIndex = totalPages - 1;

                segmentPageInfo["beginPageIndex"] = beginPageIndex;
                segmentPageInfo["endPageIndex"] = endPageIndex;
                segmentPageInfo["leadCount"] = leadCount;
                segmentPageInfo["eachLeadRows"] = eachLeadRows;
                segmentPageInfo["totalSegmentRows"] = eachLeadRows * leadCount;
                segmentPageInfo["printDirection"] = printDirection;
                segmentPageInfo["segmentRowSeconds"] = segmentRowSeconds;
                segmentPageInfo["pageMaxRows"] = maxRowsPerPage;

                SegmentPageInfos.Add(segmentPageInfo);

                // 更新上一个打印方向
                previousPrintDirection = printDirection;
            }

            Console.WriteLine($"getSegmentPageInfo完成，总页数: {totalPages}");

            return SegmentPageInfos;
        }
        private static int CalculateMultiPageRows(int currentPage, int beginPageIndex, int endPageIndex,
            int remainingRowsInPage, int pageMaxRows, int totalSegmentRows, int alreadyDrawnRows)
        {
            if (currentPage == beginPageIndex)
            {
                return Math.Min(remainingRowsInPage, totalSegmentRows);
            }
            else if (currentPage == endPageIndex)
            {
                return Math.Min(remainingRowsInPage, totalSegmentRows - alreadyDrawnRows);
            }
            else
            {
                int rowsNeeded = totalSegmentRows - alreadyDrawnRows;
                int result = Math.Min(remainingRowsInPage, Math.Min(pageMaxRows, rowsNeeded));
                return (remainingRowsInPage == pageMaxRows && rowsNeeded >= pageMaxRows) ? pageMaxRows : result;
            }
        }

        private static int AdjustRowsForLeadCount(int actualTotalRowsToDraw, int leadCount, bool isMultiPageSegment, int currentPage, int endPageIndex)
        {
            if (actualTotalRowsToDraw <= 0)
            {
                return 0;
            }

            int maxRowsPerLeadInPage = actualTotalRowsToDraw / leadCount;

            if (maxRowsPerLeadInPage == 0)
            {
                if (isMultiPageSegment && currentPage == endPageIndex && actualTotalRowsToDraw > 0)
                {
                    maxRowsPerLeadInPage = 1;
                    return Math.Min(actualTotalRowsToDraw, maxRowsPerLeadInPage * leadCount);
                }
                return 0;
            }

            return maxRowsPerLeadInPage * leadCount;
        }
        private static (int rowsPerLeadInThisPage, int actualUsedRows) CalculateRowsPerLead(int actualTotalRowsToDraw, int leadCount, int alreadyDrawnRows, int eachLeadRows)
        {
            double alreadyDrawnRowsPerLead = (double)alreadyDrawnRows / leadCount;
            int remainingRowsPerLead = eachLeadRows - (int)Math.Floor(alreadyDrawnRowsPerLead);

            int rowsPerLeadInThisPage = actualTotalRowsToDraw >= leadCount * remainingRowsPerLead ?
                remainingRowsPerLead : (int)Math.Floor((double)actualTotalRowsToDraw / leadCount);

            rowsPerLeadInThisPage = Math.Min(rowsPerLeadInThisPage, remainingRowsPerLead);
            int actualUsedRows = rowsPerLeadInThisPage * leadCount;

            return (rowsPerLeadInThisPage, actualUsedRows);
        }

        public static List<List<JObject>> CalcSegmentPrintPageData(List<JObject> SegmentPageInfos, List<JObject> SegmentInfoJsons)
        {

            List<List<JObject>> SegmentAllPageInfo = new List<List<JObject>>();
            List<List<JObject>> SegmentPageDatas = new List<List<JObject>>();

            if (SegmentPageInfos.Count == 0)
            {
                return SegmentAllPageInfo;
            }

            // 创建页面分组
            Dictionary<int, List<JObject>> pageGroups = new Dictionary<int, List<JObject>>();

            // 按beginPageIndex排序并创建页面分组
            var sortedPageInfos = SegmentPageInfos.Select((pageInfo, index) => new
            {
                PageInfo = pageInfo,
                SegmentInfo = SegmentInfoJsons[index],
                Index = index
            }).OrderBy(x => Convert.ToInt32(x.PageInfo["beginPageIndex"])).ToList();

            foreach (var item in sortedPageInfos)
            {
                JObject segmentPageInfo = item.PageInfo;
                JObject segmentInfoJson = item.SegmentInfo;
                int beginPageIndex = Convert.ToInt32(segmentPageInfo["beginPageIndex"]);
                int endPageIndex = Convert.ToInt32(segmentPageInfo["endPageIndex"]);

                for (int page = beginPageIndex; page <= endPageIndex; page++)
                {
                    JObject pageSpecificInfo = new JObject(segmentPageInfo)
                    {
                        ["currentPage"] = page,
                        ["isMultiPageSegment"] = beginPageIndex != endPageIndex
                    };

                    JObject combinedData = new JObject
                    {
                        ["segmentInfo"] = segmentInfoJson,
                        ["pageInfo"] = pageSpecificInfo
                    };

                    if (!pageGroups.ContainsKey(page))
                    {
                        pageGroups[page] = new List<JObject>();
                    }

                    pageGroups[page].Add(combinedData);
                }
            }

            // 将页面分组按页面顺序添加到SegmentPageDatas
            foreach (int page in pageGroups.Keys.OrderBy(p => p))
            {
                SegmentPageDatas.Add(pageGroups[page]);
            }

            Console.WriteLine($"分页完成，共{SegmentPageDatas.Count}个页面");


            // 初始化跟踪器
            Dictionary<int, int> segmentDrawnRowsTracker = new Dictionary<int, int>();
            Dictionary<int, int> pageRemainingRowsTracker = new Dictionary<int, int>();

            // 处理每个页面的片段图绘制
            for (int i = 0; i < SegmentPageDatas.Count; i++)
            {
                List<JObject> segmentOnePageInfo = new List<JObject>();

                List<JObject> group = SegmentPageDatas[i];
                int currentPage = i;

                // 初始化当前页面的剩余行数
                if (group.Count > 0 && !pageRemainingRowsTracker.ContainsKey(currentPage))
                {
                    int firstSegmentPageMaxRows = Convert.ToInt32(group[0]["pageInfo"]["pageMaxRows"]);
                    pageRemainingRowsTracker[currentPage] = firstSegmentPageMaxRows;
                }

                Console.WriteLine("  计算每页中每个片段图的每导联实际绘制行数:");

                foreach (JObject item in group)
                {
                    int sample = Convert.ToInt32(item["segmentInfo"]["sample"]);
                    string eventName = item["segmentInfo"]["eventName"]?.Value<string>() ?? "";
                    int startSample = Convert.ToInt32(item["segmentInfo"]["startSample"]);
                    string segmentLabel = item["segmentInfo"]["cbxLabel"]?.ToString();
                    int pageMaxRows = Convert.ToInt32(item["pageInfo"]["pageMaxRows"]);
                    int beginPageIndex = Convert.ToInt32(item["pageInfo"]["beginPageIndex"]);
                    int endPageIndex = Convert.ToInt32(item["pageInfo"]["endPageIndex"]);
                    int eachLeadRows = Convert.ToInt32(item["pageInfo"]["eachLeadRows"]);
                    int totalSegmentRows = Convert.ToInt32(item["pageInfo"]["totalSegmentRows"]);
                    bool isMultiPageSegment = beginPageIndex != endPageIndex;
                    List<int> leadIdxList = item["segmentInfo"]["leadIdxs"].ToObject<List<int>>() ?? new List<int>();
                    int leadCount = leadIdxList.Count;

                    // 初始化跟踪器
                    if (!segmentDrawnRowsTracker.ContainsKey(startSample))
                    {
                        segmentDrawnRowsTracker[startSample] = 0;
                    }

                    int remainingRowsInPage = pageRemainingRowsTracker[currentPage];
                    int alreadyDrawnRows = segmentDrawnRowsTracker[startSample];

                    // 计算当前页面实际需要绘制的总行数
                    int actualTotalRowsToDraw = isMultiPageSegment ?
                        CalculateMultiPageRows(currentPage, beginPageIndex, endPageIndex, remainingRowsInPage,
                            pageMaxRows, totalSegmentRows, alreadyDrawnRows) :
                        Math.Min(remainingRowsInPage, totalSegmentRows);

                    // 关键修正：片段图所有导联必须作为一个整体绘制
                    actualTotalRowsToDraw = AdjustRowsForLeadCount(actualTotalRowsToDraw, leadCount, isMultiPageSegment, currentPage, endPageIndex);
                    actualTotalRowsToDraw = Math.Max(0, actualTotalRowsToDraw);

                    // 计算每个导联在当前页面应该绘制的行数
                    if (actualTotalRowsToDraw > 0)
                    {
                        (int rowsPerLeadInThisPage, int actualUsedRows) = CalculateRowsPerLead(actualTotalRowsToDraw, leadCount, alreadyDrawnRows, eachLeadRows);

                        for (int row = 0; row < rowsPerLeadInThisPage; row++)
                        {
                            var pagePartInfo = new JObject();
                            pagePartInfo["sample"] = sample;
                            pagePartInfo["eventName"] = eventName;
                            pagePartInfo["segmentLabel"] = segmentLabel;
                            pagePartInfo["gain"] = item["segmentInfo"]["cbxGain"]?.Value<double>() ?? 0.0;
                            List<int> leadIdxs = item["segmentInfo"]["leadIdxs"].ToObject<List<int>>() ?? new List<int>();
                            pagePartInfo["leadIdx"] = JToken.FromObject(leadIdxs);
                            int direction = item["segmentInfo"]["cbxDirection"]?.Value<int>() ?? 0;
                            pagePartInfo["direction"] = direction;
                            int segmentRowSeconds = direction == 1 ? 7 : 10;
                            //每个导联已绘制的行数
                            int alreadyDrawnSamples = alreadyDrawnRows / leadCount * segmentRowSeconds * AnaValues.holterConfig.SampleRate;
                            pagePartInfo["startSample"] = startSample + alreadyDrawnSamples + row * segmentRowSeconds * AnaValues.holterConfig.SampleRate;
                            pagePartInfo["rowSeconds"] = segmentRowSeconds;
                            segmentOnePageInfo.Add(pagePartInfo);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"      当前页无需绘制行数");
                    }

                    // 更新跟踪器
                    segmentDrawnRowsTracker[startSample] += actualTotalRowsToDraw;
                    pageRemainingRowsTracker[currentPage] -= actualTotalRowsToDraw;
                }

                SegmentAllPageInfo.Add(segmentOnePageInfo);
            }

            return SegmentAllPageInfo;
        }

        public static int GetFastestLowestHR(string eventName, int sample)
        {
            int avgHR = -1;
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
                            int HRSample = Convert.ToInt32(jsonData[eventName]["sample"].ToString());

                            if (HRSample == sample)
                            {
                                avgHR = Convert.ToInt32(jsonData[eventName]["value"].ToString());
                            }
                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }
            return avgHR;
        }

        static void PrintSegmentInfo(string segmentLabel, int currentPage, int eachLeadRows, int totalSegmentRows,
            int pageMaxRows, int remainingRowsInPage, int actualTotalRowsToDraw, int alreadyDrawnRows)
        {
            Console.WriteLine($"    片段图 '{segmentLabel}' 在第{currentPage}页:");
            Console.WriteLine($"      每导联总行数: {eachLeadRows}, 片段图总行数: {totalSegmentRows}");
            Console.WriteLine($"      页面最大行数: {pageMaxRows}, 页面剩余行数: {remainingRowsInPage}");
            Console.WriteLine($"      实际绘制总行数: {actualTotalRowsToDraw}");
            Console.WriteLine($"      已累计绘制行数: {alreadyDrawnRows}");
        }

        static void PrintLeadDistribution(int rowsPerLeadInThisPage, int actualUsedRows, int alreadyDrawnRows,
            int leadCount, int eachLeadRows, List<int> leadIdxList)
        {
            double alreadyDrawnRowsPerLead = (double)alreadyDrawnRows / leadCount;
            int remainingRowsPerLead = eachLeadRows - (int)Math.Floor(alreadyDrawnRowsPerLead);

            Console.WriteLine($"      每个导联已绘制行数: {Math.Floor(alreadyDrawnRowsPerLead)}, 还需绘制行数: {remainingRowsPerLead}");
            Console.WriteLine($"      每个导联当前页绘制行数: {rowsPerLeadInThisPage}");
            Console.WriteLine($"      实际使用总行数: {actualUsedRows}");

            foreach (int leadIdx in leadIdxList)
            {
                Console.WriteLine($"      导联{leadIdx}: 当前页{rowsPerLeadInThisPage}行");
            }
        }

    }
    
}
