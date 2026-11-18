using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zoncare.Common;
using Zoncare.DAL;

namespace AnalysisApp.Commons
{
    public static class CalcBPInfo
    {
        public static bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;

        public static DateTime DayTimeStart = DateTime.Parse(settingValues.daytimeStart);
        public static DateTime DayTimeEnd = DateTime.Parse(settingValues.daytimeEnd);

        /// <summary>
        /// 获取参与计数的记录
        /// </summary>
        /// <returns></returns>
        public static List<bpDetail> GetAllCountList()
        {
            //查询数据库中所有BP记录
            List<bpDetail> sortBPDetails = AnaValues.CurrentBpDetails.OrderBy(b => b.time).ToList();
            return sortBPDetails.FindAll(b => (b.errorCode == null || b.errorCode.Trim() == "0"));
        }

        /// <summary>
        /// 获取有效记录
        /// </summary>
        /// <returns></returns>
        public static List<bpDetail> GetEffectiveList(List<bpDetail> allCountList)
        {
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            return allCountList.FindAll(
            b => ((b.sysPressure / 1000 > settingValues.minErrorSys && b.sysPressure / 1000 < settingValues.maxErrorSys) &&
            (b.diaPressure / 1000 > settingValues.minErrorDia && b.diaPressure / 1000 < settingValues.maxErrorDia)));
        }

        /// <summary>
        /// 获取错误记录
        /// </summary>
        /// <returns></returns>
        public static List<bpDetail> GetErrorList()
        {
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;

            List<bpDetail> allCountList = GetAllCountList();
            return allCountList.FindAll(
            b => ((b.sysPressure / 1000 < settingValues.minErrorSys || b.sysPressure / 1000 > settingValues.maxErrorSys) ||
            (b.diaPressure / 1000 < settingValues.minErrorDia || b.diaPressure / 1000 > settingValues.maxErrorDia)));
        }

        /// <summary>
        /// 获取有效数据列表
        /// </summary>
        /// <returns></returns>
        public static List<bpDetail> GetValidDataList()
        {
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;

            List<bpDetail> allCountList = GetAllCountList();
            return allCountList.FindAll(
            b => ((b.sysPressure / 1000 > settingValues.minErrorSys && b.sysPressure / 1000 < settingValues.maxErrorSys) &&
            (b.diaPressure / 1000 > settingValues.minErrorDia && b.diaPressure / 1000 < settingValues.maxErrorDia) &&
            (b.dataStatus == 1)));
        }

        public static List<bpDetail> GetIncludeErrorDataList()
        {
            List<bpDetail> allCountList = GetAllCountList();
            return allCountList.FindAll(b => b.dataStatus == 1);
        }


        /// <summary>
        /// 判断是否在某个时间范围内
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool IsInTimeRange(DateTime dt, TimeSpan start, TimeSpan end)
        {
            TimeSpan time = dt.TimeOfDay;
            return time >= start && time <= end;
        }

        public static List<bpDetail> GetBpDetailList(int? type = null)
        {
            List<bpDetail> dataList = new List<bpDetail>();

            if (type == 0)
            {
                dataList = GetAllCountList();
            }
            else if (type == 1)
            {
                dataList = GetValidDataList();
            }
            else if (type == 2)
            {
                dataList = GetIncludeErrorDataList();
            }
            else
            {
                dataList = AnaValues.CurrentBpDetails.OrderBy(b => b.time).ToList();
            }
            return dataList;
        }

        /// <summary>
        /// 获取血压数据列
        /// </summary>
        /// <param name="type">0所有参与计数的记录 1有效的记录 2包含错误数据的记录</param>
        /// <returns></returns>
        public static List<BPDataInfo> GetBPDataList(int? type = null)
        {
            List<bpDetail> dataList = GetBpDetailList(type);
            List<BPDataInfo> bpDataList = CalcBPDataInfo(dataList);
            return bpDataList;
        }

        public static List<BPDataInfo> CalcBPDataInfo(List<bpDetail> dataList)
        {
            List<BPDataInfo> bpDataList = new List<BPDataInfo>();
            for (int i = 0; i < dataList.Count; i++)
            {
                BPDataInfo dataInfo = new BPDataInfo();
                dataInfo.Id = dataList[i].id;
                DateTime dateTime = DateTime.Parse(dataList[i].time);
                dataInfo.bpDateTime = dateTime;
                dataInfo.Date = dateTime.ToString("yyyy-MM-dd");
                dataInfo.Time = dateTime.ToString("HH:mm");
                int sbp = dataList[i].sysPressure.GetValueOrDefault();
                int dbp = dataList[i].diaPressure.GetValueOrDefault();
                int map = dataList[i].mapPressure;
                int pulse = dataList[i].pulse;
                dataInfo.SBP = sbp / 1000d;
                dataInfo.DBP = dbp / 1000d;
                dataInfo.MAP = map / 1000d;
                dataInfo.RPP = (sbp / 1000d * pulse) / 1000d;
                dataInfo.bDay = IsInTimeRange(dateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour));
                dataInfo.Pulse = pulse;
                dataInfo.DataStatus = dataList[i].dataStatus.GetValueOrDefault();
                dataInfo.MotionState = AnaCode.GetMotionStateText(AnaValues.CurrentCase.deviceType, dataList[i].motionState);
                dataInfo.Status = dataList[i].status.GetValueOrDefault();
                dataInfo.ErrorCode = dataList[i].errorCode;
                int measureType = dataList[i].measureType.GetValueOrDefault();
                dataInfo.Notes = GetNotes(dataList[i].notes, measureType, dataList[i].errorCode);
                dataInfo.MeasureType = measureType;
                bpDataList.Add(dataInfo);
            }
            return bpDataList;
        }

        public static string GetNotes(string notes, int measureType, string errorCode)
        {
            string notesStr = "";
            if (string.IsNullOrEmpty(notes))
            {
                switch (measureType)
                {
                    case 0:
                        notesStr = "";
                        break;
                    case 1:
                        notesStr = "补测";
                        break;
                    case 2:
                        notesStr = "手动测量";
                        break;
                    case 3:
                        notesStr = "运动推迟";
                        break;
                    default:
                        notesStr = "";
                        break;
                }
            }
            else
            {
                notesStr = notes;
            }
            return notesStr;
        }

        /// <summary>
        /// 获取血压波形数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="refPR"></param>
        /// <returns></returns>
        public static PulseWaveData GetPulseWaveData(string filePath, int refPR)
        {
            PulseWaveData waveData = null;
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                if (fileBytes.Length > 16) { }

                byte[] waveBytes = new byte[fileBytes.Length - 16];
                Buffer.BlockCopy(fileBytes, 0, waveBytes, 0, waveBytes.Length);

                List<int> dataList = new List<int>(waveBytes.Length / 4);
                List<int> stageList = new List<int>(waveBytes.Length / 4);
                List<GroupInterval> groups = new List<GroupInterval>();
                StringBuilder sbData = new StringBuilder(), sbStage = new StringBuilder();
                int statu = 0;
                int idx = 0, startIdx = 0, endIdx = 0;
                for (int i = 0; i < waveBytes.Length; i += 4)
                {
                    int one = BitConverter.ToInt32(waveBytes, i) & 0x00ffffff; //清除高8位(高8位为帧序号)
                    if ((one & 0xc00000) == 0xc00000) //状态数据
                    {
                        statu = one >> 16 & 0x0f;
                        if (statu == 1) //充气结束
                        {
                        }
                        else if (statu == 3) //台阶放气开始
                        {
                            endIdx = idx - 1;
                            if (endIdx > 0) groups.Add(new GroupInterval(startIdx, endIdx));
                        }
                        else if (statu == 2) //台阶放气完成
                        {
                            startIdx = (idx == 0 ? 0 : (idx));
                        }
                        else if (statu == 15) //全速放气，bit16~bit23=0xFF
                        {
                            statu = 255;
                        }
                    }
                    else
                    {
                        int maskedData = one & 0x3fffff;
                        int mmHgValue = (maskedData - 100000); //转换为对应于mmHg*1000的值
                        dataList.Add(mmHgValue);
                        stageList.Add(statu);
                        sbData.AppendLine(mmHgValue.ToString());
                        sbStage.AppendLine(statu.ToString());
                        statu = 0; //除当前有状态变化的数据外, 其余状态值置0
                        idx++;
                    }
                }
                if (idx - 1 > endIdx) groups.Add(new GroupInterval(startIdx, idx - 1));

                //File.WriteAllText(@"C:\test\bpdata.txt", sbData.ToString(), new UTF8Encoding(false));
                //File.WriteAllText(@"C:\test\stagedata.txt", sbStage.ToString(), new UTF8Encoding(false));

                double[] dc, ac, stepDc;
                int[] stepDcLen;
                int ret = BpPulseWaveDataHandler.WaveDataPlot(dataList, stageList, refPR, out dc, out ac, out stepDc, out stepDcLen);
                if (ret == 0)
                {
                    waveData = new PulseWaveData(dc, ac, stepDc, stepDcLen, dc.Length);
                    waveData.stepGroups = groups.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压数据处理");
            }

            return waveData;
        }

        public static List<BPHourListInfo> GetHourList()
        {
            int hour = -1;
            int count = 0;
            double sBP = 0;
            double dBP = 0;
            double MAP = 0;
            int pulse = 0;
            string date = "";
            string time = "";
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;

            List<BPDataInfo> bpAllDataList = GetBPDataList(1);
            List<BPHourListInfo> hourList = new List<BPHourListInfo>();
            for (int i = 0; i < bpAllDataList.Count; i++)
            {
                DateTime dateTime = bpAllDataList[i].bpDateTime;
                if (hour == -1)
                {
                    hour = dateTime.Hour;
                }
                if (hour != dateTime.Hour)
                {
                    BPHourListInfo hourInfo = new BPHourListInfo();
                    hourInfo.Date = date;
                    hourInfo.Time = time;
                    hourInfo.SBP = sBP / count;
                    hourInfo.DBP = dBP / count;
                    hourInfo.MAP = MAP / count;
                    //if (settingValues.unit == "kPa")
                    //{
                    //    hourInfo.SBP = Math.Round(sBP / count, 2).ToString("F2");
                    //    hourInfo.DBP = Math.Round(dBP / count, 2).ToString("F2");
                    //    hourInfo.MAP = Math.Round(MAP / count, 2).ToString("F2");
                    //}
                    //else
                    //{
                    //    hourInfo.SBP = Convert.ToInt32(sBP / count).ToString();
                    //    hourInfo.DBP = Convert.ToInt32(dBP / count).ToString();
                    //    hourInfo.MAP = Convert.ToInt32(MAP / count).ToString();
                    //}
                    hourInfo.Pulse = pulse / count;
                    hourInfo.Count = count;
                    hourList.Add(hourInfo);

                    hour = dateTime.Hour;
                    date = dateTime.ToString("yyyy-MM-dd");
                    time = hour.ToString("D2") + ":00 - " + hour.ToString("D2") + ":59";
                    sBP = bpAllDataList[i].SBP;
                    dBP = bpAllDataList[i].DBP;
                    MAP = bpAllDataList[i].MAP;
                    pulse = bpAllDataList[i].Pulse;
                    count = 1;

                }
                else
                {
                    date = dateTime.ToString("yyyy-MM-dd");
                    time = hour.ToString("D2") + ":00 - " + hour.ToString("D2") + ":59";
                    sBP += bpAllDataList[i].SBP;
                    dBP += bpAllDataList[i].DBP;
                    MAP += bpAllDataList[i].MAP;
                    pulse += bpAllDataList[i].Pulse;
                    ++count;

                    if (i == bpAllDataList.Count - 1)
                    {
                        BPHourListInfo hourInfo = new BPHourListInfo();
                        hourInfo.Date = date;
                        hourInfo.Time = time;
                        hourInfo.SBP = sBP / count;
                        hourInfo.DBP = dBP / count;
                        hourInfo.MAP = MAP / count;
                        //if(settingValues.unit == "kPa")
                        //{
                        //    hourInfo.SBP = Math.Round(sBP / count, 2).ToString("F2");
                        //    hourInfo.DBP = Math.Round(dBP / count, 2).ToString("F2");
                        //    hourInfo.MAP = Math.Round(MAP / count, 2).ToString("F2");
                        //}
                        //else
                        //{
                        //    hourInfo.SBP = Convert.ToInt32(sBP/count).ToString();
                        //    hourInfo.DBP = Convert.ToInt32(dBP/count).ToString();
                        //    hourInfo.MAP = Convert.ToInt32(MAP/count).ToString();
                        //}

                        hourInfo.Pulse = pulse / count;
                        hourInfo.Count = count;
                        hourList.Add(hourInfo);
                    }
                }
            }

            return hourList;
        }

        public static BPSummaryData GetBPSummaryData()
        {
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            DayTimeStart = DateTime.Parse(settingValues.daytimeStart);
            DayTimeEnd = DateTime.Parse(settingValues.daytimeEnd);

            BPSummaryData bpSummaryData = new BPSummaryData();

            List<bpDetail> allCountList = GetAllCountList();
            if (allCountList.Count == 0) return bpSummaryData;
            List<bpDetail> dayCountList = new List<bpDetail>();
            List<bpDetail> nightCountList = new List<bpDetail>();
            dayCountList = allCountList.FindAll(b => (IsInTimeRange(DateTime.Parse(b.time), TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            nightCountList = allCountList.FindAll(b => (!IsInTimeRange(DateTime.Parse(b.time), TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));

            List<BPDataInfo> bpAllDataList = GetBPDataList(2);
            List<BPDataInfo> bpDayDataList = new List<BPDataInfo>();
            List<BPDataInfo> bpNightDataList = new List<BPDataInfo>();
            bpDayDataList = bpAllDataList.FindAll(b => (IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            bpNightDataList = bpAllDataList.FindAll(b => (!IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));

            CalcSummaryData(bpAllDataList, bpSummaryData.bpAllData, 0);
            CalcSummaryData(bpDayDataList, bpSummaryData.bpDayData, 1);
            CalcSummaryData(bpNightDataList, bpSummaryData.bpNightData, 2);

            double daySbpAvg = 0;
            double nightSbpAvg = 0;
            Double.TryParse(bpSummaryData.bpDayData.SbpAvg, out daySbpAvg);
            Double.TryParse(bpSummaryData.bpNightData.SbpAvg, out nightSbpAvg);
            bpSummaryData.bpAllData.SbpPercentage = GetSbpPercentage(daySbpAvg, nightSbpAvg);
            bpSummaryData.bpAllData.ZyjlType = GetDAN(bpSummaryData.bpAllData.SbpPercentage);
            bpSummaryData.bpAllData.SbpGtLimit = bpSummaryData.bpDayData.SbpGtLimit + bpSummaryData.bpNightData.SbpGtLimit;
            bpSummaryData.bpAllData.DbpGtLimit = bpSummaryData.bpDayData.DbpGtLimit + bpSummaryData.bpNightData.DbpGtLimit;
            int allEffectiveCount = GetEffectiveList(allCountList).Count;
            bpSummaryData.bpAllData.EffectiveCount = allEffectiveCount;
            bpSummaryData.bpAllData.EffectiveCountRate = Math.Round((double)allEffectiveCount / allCountList.Count * 100, 2);

            int dayEffectiveCount = GetEffectiveList(dayCountList).Count;
            bpSummaryData.bpDayData.EffectiveCount = dayEffectiveCount;
            bpSummaryData.bpDayData.EffectiveCountRate = Math.Round((double)dayEffectiveCount / dayCountList.Count * 100, 2);

            int nightEffectiveCount = GetEffectiveList(nightCountList).Count;
            bpSummaryData.bpNightData.EffectiveCount = nightEffectiveCount;
            bpSummaryData.bpNightData.EffectiveCountRate = Math.Round((double)nightEffectiveCount / nightCountList.Count * 100, 2);

            int errorCount = GetErrorList().Count;
            bpSummaryData.bpAllData.ErrorNum = errorCount;
            bpSummaryData.bpAllData.ErrorNumPercent = Math.Round((double)errorCount / allCountList.Count * 100, 2);
            bpSummaryData.bpAllData.StartTime = bpAllDataList[0].bpDateTime.ToString("yyyy-MM-dd HH:mm");
            bpSummaryData.bpAllData.EndTime = bpAllDataList[bpAllDataList.Count - 1].bpDateTime.ToString("yyyy-MM-dd HH:mm");

            bpSummaryData.bpAllData.Count = allCountList.Count;
            bpSummaryData.bpDayData.Count = dayCountList.Count;
            bpSummaryData.bpNightData.Count = nightCountList.Count;

            DateTime startTime = DateTime.Parse(allCountList[0].time);
            DateTime endTime = DateTime.Parse(allCountList[allCountList.Count - 1].time);
            TimeSpan diff = endTime - startTime;
            bpSummaryData.bpAllData.TotalTime = $"{diff.Hours} 小时 {diff.Minutes} 分钟";

            bpSummaryData.bpAllData.SbpLoadNum = Math.Round((double)bpSummaryData.bpAllData.SbpGtLimit / bpSummaryData.bpAllData.Count * 100, 1);
            bpSummaryData.bpAllData.DbpLoadNum = Math.Round((double)bpSummaryData.bpAllData.DbpGtLimit / bpSummaryData.bpAllData.Count * 100, 1);
            bpSummaryData.bpDayData.SbpLoadNum = Math.Round((double)bpSummaryData.bpDayData.SbpGtLimit / bpSummaryData.bpDayData.Count * 100, 1);
            bpSummaryData.bpDayData.DbpLoadNum = Math.Round((double)bpSummaryData.bpDayData.DbpGtLimit / bpSummaryData.bpDayData.Count * 100, 1);
            bpSummaryData.bpNightData.SbpLoadNum = Math.Round((double)bpSummaryData.bpNightData.SbpGtLimit / bpSummaryData.bpNightData.Count * 100, 1);
            bpSummaryData.bpNightData.DbpLoadNum = Math.Round((double)bpSummaryData.bpNightData.DbpGtLimit / bpSummaryData.bpNightData.Count * 100, 1);
            if(!string.IsNullOrEmpty(bpSummaryData.bpDayData.MorningSbpAvg) && !string.IsNullOrEmpty(bpSummaryData.bpNightData.SbpMin))
            {
                bpSummaryData.bpAllData.MorningPre = (double.Parse(bpSummaryData.bpDayData.MorningSbpAvg) - double.Parse(bpSummaryData.bpNightData.SbpMin)).ToString();
            }
            return bpSummaryData;
        }

        public static void CalcSummaryData(List<BPDataInfo> bpDataList, BPDetailData bpDetailData, int type)
        {
            if (bpDataList.Count == 0) return;
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;

            List<double> sbpList = new List<double>(bpDataList.Count);
            List<double> dbpList = new List<double>(bpDataList.Count);
            List<double> mapList = new List<double>(bpDataList.Count);
            List<int> pulseList = new List<int>(bpDataList.Count);

            double SbpMax = 0;
            double SbpMaxDbp = 0;
            double SbpMin = 0;
            double SbpMinDbp = 0;
            double DbpMax = 0;
            double DbpMin = 0;
            double MapMax = 0;
            double MapMin = 0;

            for (int i = 0; i < bpDataList.Count; i++)
            {
                sbpList.Add(bpDataList[i].SBP);
                dbpList.Add(bpDataList[i].DBP);
                mapList.Add(bpDataList[i].MAP);
                pulseList.Add(bpDataList[i].Pulse);
                if (type == 1)
                {
                    if (bpDataList[i].SBP > settingValues.maxWakeSys)
                    {
                        ++bpDetailData.SbpGtLimit;
                    }
                    if (bpDataList[i].DBP > settingValues.maxWakeDia)
                    {
                        ++bpDetailData.DbpGtLimit;
                    }
                }
                else if (type == 2)
                {
                    if (bpDataList[i].SBP > settingValues.maxSleepSys)
                    {
                        ++bpDetailData.SbpGtLimit;
                    }
                    if (bpDataList[i].DBP > settingValues.maxSleepDia)
                    {
                        ++bpDetailData.DbpGtLimit;
                    }
                }
                if (i == 0)
                {
                    SbpMax = bpDataList[i].SBP;
                    SbpMaxDbp = bpDataList[i].DBP;
                    bpDetailData.SbpMaxTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    SbpMin = bpDataList[i].SBP;
                    SbpMinDbp = bpDataList[i].DBP;
                    bpDetailData.SbpMinTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    DbpMax = bpDataList[i].DBP;
                    bpDetailData.DbpMaxTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    DbpMin = bpDataList[i].DBP;
                    bpDetailData.DbpMinTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    MapMax = bpDataList[i].MAP;
                    MapMin = bpDataList[i].MAP;
                    bpDetailData.PulseMax = bpDataList[i].Pulse;
                    bpDetailData.PulseMaxTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    bpDetailData.PulseMin = bpDataList[i].Pulse;
                    bpDetailData.PulseMinTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                }
                else
                {
                    if (bpDataList[i].SBP > SbpMax)
                    {
                        SbpMax = bpDataList[i].SBP;
                        SbpMaxDbp = bpDataList[i].DBP;
                        bpDetailData.SbpMaxTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    }
                    if (bpDataList[i].SBP < SbpMin)
                    {
                        SbpMin = bpDataList[i].SBP;
                        SbpMinDbp = bpDataList[i].DBP;
                        bpDetailData.SbpMinTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    }
                    if (bpDataList[i].DBP > DbpMax)
                    {
                        DbpMax = bpDataList[i].DBP;
                        bpDetailData.DbpMaxTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    }
                    if (bpDataList[i].DBP < DbpMin)
                    {
                        DbpMin = bpDataList[i].DBP;
                        bpDetailData.DbpMinTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    }
                    if (bpDataList[i].MAP > MapMax)
                    {
                        MapMax = bpDataList[i].MAP;
                    }
                    if (bpDataList[i].MAP < MapMin)
                    {
                        MapMin = bpDataList[i].MAP;
                    }
                    if (bpDataList[i].Pulse > bpDetailData.PulseMax)
                    {
                        bpDetailData.PulseMax = bpDataList[i].Pulse;
                        bpDetailData.PulseMaxTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    }
                    if (bpDataList[i].Pulse < bpDetailData.PulseMin)
                    {
                        bpDetailData.PulseMin = bpDataList[i].Pulse;
                        bpDetailData.PulseMinTime = bpDataList[i].bpDateTime.ToString("HH:mm");
                    }
                }

            }

            double morningSbpAvg = 0;
            double morningDbpAvg = 0;

            if (type == 1)
            {
                List<BPDataInfo> bpMorningDataList = bpDataList.FindAll(b => (IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeStart.Hour + 2))));

                List<double> morningSbpList = new List<double>(bpMorningDataList.Count);
                List<double> morningDbpList = new List<double>(bpMorningDataList.Count);

                for (int i = 0; i < bpMorningDataList.Count; i++)
                {
                    morningSbpList.Add(bpMorningDataList[i].SBP);
                    morningDbpList.Add(bpMorningDataList[i].DBP);
                }
                if(morningSbpList.Count > 0)
                {
                    morningSbpAvg = morningSbpList.Average();
                    morningDbpAvg = morningDbpList.Average();
                }
                if (settingValues.unit == "kPa")
                {
                    bpDetailData.MorningSbpAvg = CommonFuns.mmHg2KPa(Convert.ToInt32(morningSbpAvg), 2).ToString("F2");
                    bpDetailData.MorningDbpAvg = CommonFuns.mmHg2KPa(Convert.ToInt32(morningDbpAvg), 2).ToString("F2");
                }
                else
                {
                    bpDetailData.MorningSbpAvg = Convert.ToInt32(morningSbpAvg).ToString();
                    bpDetailData.MorningDbpAvg = Convert.ToInt32(morningDbpAvg).ToString();
                }

            }


            double sbpAvg = sbpList.Average();
            double dbpAvg = dbpList.Average();
            double mapAvg = mapList.Average();
            if (settingValues.unit == "kPa")
            {
                bpDetailData.SbpMax = CommonFuns.mmHg2KPa(Convert.ToInt32(SbpMax), 2).ToString("F2");
                bpDetailData.SbpMin = CommonFuns.mmHg2KPa(Convert.ToInt32(SbpMin), 2).ToString("F2");

                bpDetailData.SbpMaxDbp = CommonFuns.mmHg2KPa(Convert.ToInt32(SbpMaxDbp), 2).ToString("F2");
                bpDetailData.SbpMinDbp = CommonFuns.mmHg2KPa(Convert.ToInt32(SbpMinDbp), 2).ToString("F2");

                bpDetailData.DbpMax = CommonFuns.mmHg2KPa(Convert.ToInt32(DbpMax), 2).ToString("F2");
                bpDetailData.DbpMin = CommonFuns.mmHg2KPa(Convert.ToInt32(DbpMin), 2).ToString("F2");

                bpDetailData.MapMax = CommonFuns.mmHg2KPa(Convert.ToInt32(MapMax), 2).ToString("F2");
                bpDetailData.MapMin = CommonFuns.mmHg2KPa(Convert.ToInt32(MapMin), 2).ToString("F2");

                bpDetailData.SbpAvg = CommonFuns.mmHg2KPa(Convert.ToInt32(sbpAvg), 2).ToString("F2");
                bpDetailData.DbpAvg = CommonFuns.mmHg2KPa(Convert.ToInt32(dbpAvg), 2).ToString("F2");
                bpDetailData.MapAvg = CommonFuns.mmHg2KPa(Convert.ToInt32(mapAvg), 2).ToString("F2");
            }
            else
            {
                bpDetailData.SbpMax = SbpMax.ToString();
                bpDetailData.SbpMaxDbp = SbpMaxDbp.ToString();
                bpDetailData.SbpMin = SbpMin.ToString();
                bpDetailData.SbpMinDbp = SbpMinDbp.ToString();
                bpDetailData.DbpMax = DbpMax.ToString();
                bpDetailData.DbpMin = DbpMin.ToString();
                bpDetailData.MapMax = MapMax.ToString();
                bpDetailData.MapMin = MapMin.ToString();

                bpDetailData.SbpAvg = Convert.ToInt32(sbpAvg).ToString();
                bpDetailData.DbpAvg = Convert.ToInt32(dbpAvg).ToString();
                bpDetailData.MapAvg = Convert.ToInt32(mapAvg).ToString();
            }

            double sbpBzc = StandardDeviation(sbpList);
            bpDetailData.SbpBzc = Math.Round(sbpBzc, 2, MidpointRounding.AwayFromZero).ToString("F2");
            bpDetailData.SbpByxs = Math.Round((sbpBzc / sbpAvg) * 100, 2, MidpointRounding.AwayFromZero).ToString("F2");

            double dbpBzc = StandardDeviation(dbpList);
            bpDetailData.DbpBzc = Math.Round(dbpBzc, 2, MidpointRounding.AwayFromZero).ToString("F2");
            bpDetailData.DbpByxs = Math.Round((dbpBzc / dbpAvg) * 100, 2, MidpointRounding.AwayFromZero).ToString("F2");

            double mapBzc = StandardDeviation(mapList);
            bpDetailData.MapBzc = Math.Round(mapBzc, 2, MidpointRounding.AwayFromZero).ToString("F2");
            bpDetailData.MapByxs = Math.Round((mapBzc / mapAvg) * 100, 2, MidpointRounding.AwayFromZero).ToString("F2");

            bpDetailData.PulseAvg = Math.Round(pulseList.Average(), 0).ToString();
            double pulseBzc = StandardDeviation(pulseList);
            bpDetailData.PulseBzc = Math.Round(pulseBzc, 2, MidpointRounding.AwayFromZero).ToString("F2");
            bpDetailData.PulseByxs = Math.Round((pulseBzc / pulseList.Average()) * 100, 2, MidpointRounding.AwayFromZero).ToString("F2");

            bpDetailData.AASI = AASI(sbpList, dbpList, 1);
            bpDetailData.Correl = Math.Round(Slope(sbpList, dbpList, 1), 2);
            bpDetailData.RelationShip = RelationShip(sbpList, dbpList, 1, 0);
        }

        /// <summary>
        /// 总体标准差（支持 int, double, decimal 等）
        /// </summary>
        public static double StandardDeviation<T>(IEnumerable<T> values) where T : struct, IConvertible
        {
            if (values == null || !values.Any())
                return 0;

            var list = values.Select(v => Convert.ToDouble(v)).ToList();
            int count = list.Count;

            if (count <= 1)
                return 0;

            double avg = list.Average();
            double variance = list.Sum(v => Math.Pow(v - avg, 2)) / count;
            return Math.Sqrt(variance);
        }

        //获取收缩压与舒张压的关系式
        public static string RelationShip(List<double> sbpList, List<double> dbpList, int slo, int flag = 0)
        {
            double slopeVal = Slope(sbpList, dbpList, slo);

            double sbpAvg = sbpList.Average();
            double dbpAvg = dbpList.Average();
            double constVal = Math.Round(dbpAvg - sbpAvg * slopeVal, 2);

            string op = slopeVal < 0 ? "" : "+";
            string relationShip = "";
            if (flag == 0)
            {
                relationShip = $"DBP = { constVal}{ op}{ slopeVal.ToString("F2")}*SBP";
            }
            else
            {
                relationShip = $"Pulse = { constVal}{ op}{ slopeVal.ToString("F2")}*SBP";
            }
            return relationShip;
        }

        //计算相关性指数r (r 的绝对值越接近 1，表示线性关系越强；越接近 0，表示线性关系越弱)
        public static double Correl(List<double> sbpList, List<double> dbpList)
        {
            if (sbpList.Count == 0 || dbpList.Count == 0)
            {
                return 0.0;
            }
            double sbpAvg = sbpList.Average();
            double dbpAvg = dbpList.Average();
            double sbpEV = SumOfSquaredDeviations(sbpList); //收缩压离差平方和
            double dbpEV = SumOfSquaredDeviations(dbpList); //舒张压离差平方和

            double denoValue = Math.Sqrt(sbpEV * dbpEV);
            double corrValue = 0.0; //所有收缩压和舒张压离差乘积的和
            for (int i = 0; i < sbpList.Count; i++)
            {
                corrValue += (sbpList[i] - sbpAvg) * (dbpList[i] - dbpAvg);
            }

            if (corrValue == 0) return 0.0;
            return Math.Round(corrValue / denoValue, 2);
        }

        /// <summary>
        /// 计算离差平方和
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double SumOfSquaredDeviations<T>(IEnumerable<T> values) where T : struct, IConvertible
        {
            if (values == null || !values.Any())
                return 0;

            var list = values.Select(v => Convert.ToDouble(v)).ToList();
            int count = list.Count;

            if (count <= 1)
                return 0;

            double avg = list.Average();

            return list.Sum(v => Math.Pow(v - avg, 2));
        }

        /**
         * AASI 动态动脉硬化指数
         *
         * @param sbp
         * @param dbp
         * @param slo
         * @return
         */
        public static double AASI(List<double> sbpList, List<double> dbpList, int slo)
        {
            double slope = Slope(sbpList, dbpList, slo);
            double AASI = Math.Round(1.0 - slope, 2);
            return AASI;
        }

        /**
         * 斜率
         * @param sbp 收缩压数组
         * @param dbp 舒张压数组
         * @param slo 收缩压对舒张压 slo=0 舒张压对收缩压 slo=1 为其它
         * @return
         */
        public static double Slope(List<double> sbpList, List<double> dbpList, int slo)
        {
            if (sbpList.Count == 0 || dbpList.Count == 0) return 0;
            double sbpavg = sbpList.Average();
            double dbpavg = dbpList.Average();
            double num = 0;
            for (int i = 0; i < sbpList.Count; i++)
            {
                num += Math.Pow(sbpList[i] - sbpavg, 2);
            }

            double num2 = 0;
            for (int i = 0; i < dbpList.Count; i++)
            {
                num2 += Math.Pow(dbpList[i] - dbpavg, 2);
            }

            double num3 = 0.0;
            for (int i = 0; i < sbpList.Count; i++)
            {
                num3 += (sbpList[i] - sbpavg) * (dbpList[i] - dbpavg);
            }
            if (num3 == 0.0)
            {
                return 0.0;
            }
            if (slo == 0)
            {
                return num3 / num2;
            }
            return num3 / num;
        }

        /// <summary>
        /// 舒张压平均值-收缩压平均值*斜率(舒张压对收缩压)=y0
        /// </summary>
        /// <param name="sbpList"></param>
        /// <param name="dbpOrPulseList"></param>
        /// <param name="slo"></param>
        /// <returns></returns>
        public static double GetY0(List<double> sbpList, List<double> dbpOrPulseList, int slo)
        {
            if (sbpList.Count == 0 || dbpOrPulseList.Count == 0) return 0;

            //斜率
            double slope = Slope(sbpList, dbpOrPulseList, slo);
            //收缩压平均值
            double sbpAvg = sbpList.Average();
            //舒张压平均值(如果是心率相关图，计算的是心率的平均值)
            double dbpOrPulseAvg = dbpOrPulseList.Average();
            if (slo == 1)
            {
                double result = dbpOrPulseAvg - sbpAvg * slope;
                return result;
            }
            else
            {
                double result = sbpAvg - dbpOrPulseAvg * slope;
                return result;
            }
        }

        /// <summary>
        /// 250*斜率(舒张压对收缩压)+y0
        /// </summary>
        /// <param name="sbpList"></param>
        /// <param name="dbpOrPulseList"></param>
        /// <param name="slo"></param>
        /// <param name="y0"></param>
        /// <returns></returns>
        public static double GetY1(List<double> sbpList, List<double> dbpOrPulseList, int slo, double y0)
        {
            if (sbpList.Count == 0 || dbpOrPulseList.Count == 0) return 0;

            //斜率
            double slope = Slope(sbpList, dbpOrPulseList, slo);
            if (slo == 1)
            {
                double result = 250 * slope + y0;
                return result;
            }
            else
            {
                double result = 200 * slope + y0;
                return result;
            }
        }

        public static SKPoint GetSlopePoint1(double y0, double y1)
        {
            double result = 0;
            int t = 200;
            if (y0 < 0)
            {
                //Y值为负数，需要找与Y轴交点
                double tempy2 = ((t - 0) * (0 - y0)) / (y1 - y0);
                result = Math.Round(tempy2, 2);
                SKPoint point = new SKPoint(0, (float)result);
                return point;
            }
            else
            {
                result = Math.Round(y0, 2);
                SKPoint point = new SKPoint((float)result, 0);
                return point;
            }
        }

        public static SKPoint GetSlopePoint2(double y0, double y1)
        {
            double result = 0;
            int t = 200;
            if (y1 < 0)
            {
                //Y值为负数，需要找与Y轴交点
                result = Math.Round(((t - 0) * (0 - y0)) / (y1 - y0), 2);
                SKPoint point = new SKPoint(0, (float)result);
                return point;
            }
            else
            {
                result = Math.Round(y1, 2);
                SKPoint point = new SKPoint((float)result, 200);
                return point;
            }
        }

        /**
         * 收缩压百分比
         * 收缩压百分比 = (白天收缩压平均值-夜间收缩压平均值) / 白天收缩压平均值;
         */
        public static double GetSbpPercentage(double daySbpAvg, double nightSbpAvg)
        {
            if (daySbpAvg == 0)
            {
                return 0;
            }
            return Math.Round((daySbpAvg - nightSbpAvg) / daySbpAvg * 100, 2);
        }

        /**
         * 昼夜节律（血压波动类型）
         *
         * @param d 收缩压百分比
         * @return
         */
        public static string GetDAN(double percent)
        {
            if (percent < 0.0)
            {
                return "反勺型"; //反勺型
            }
            if (percent < 10.0 && percent >= 0.0)
            {
                return "非勺型血压"; //非勺型血压
            }
            if (percent >= 10.0 && percent <= 20.0)
            {
                return "勺型血压"; //勺型血压
            }
            if (percent > 20.0)
            {
                return "超勺型血压"; //超勺型血压
            }
            return "勺型血压"; //勺型血压
        }


        //白大衣高血压/隐匿性高血压判定,type 0 白大衣高血压 1 隐匿性高血压
        public static bool isWCHOrMasked(List<BPDataInfo> bpDataList, int type)
        {
            bool result = false;
            if (bpDataList.Count == 0) return result;
            //获取诊室血压
            BPDataInfo clinicBP = bpDataList[0];
            bpDataList.RemoveAt(0);

            List<double> sbpList = new List<double>(bpDataList.Count);
            for (int i = 0; i < bpDataList.Count; i++)
            {
                sbpList.Add(bpDataList[i].SBP);
            }
            double avg = sbpList.Average();
            if (type == 0)
            {
                if (avg < 130 && clinicBP.SBP > 140 && clinicBP.DBP > 90)
                {
                    result = true;
                }
            }
            else
            {
                if (avg >= 130 && clinicBP.SBP < 140 && clinicBP.DBP < 90)
                {
                    result = true;
                }
            }
            return result;
        }

        public static string GetNotes()
        {
            string notes = "";
            string bpInfoFilePath = "";
            if (!string.IsNullOrEmpty(AnaValues.RootPath))
            {
                bpInfoFilePath = Path.Combine(AnaValues.RootPath, AnaValues.BPInfoFileName);
            }

            // 读取notes信息
            try
            {
                if (File.Exists(bpInfoFilePath))
                {
                    // 读取JSON文件内容
                    string jsonString = File.ReadAllText(bpInfoFilePath, Encoding.UTF8);
                    var bpTrendNotesJson = JObject.Parse(jsonString);
                    if (bpTrendNotesJson["notes"] != null)
                    {
                        notes = bpTrendNotesJson["notes"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压");
            }
            return notes;
        }

        public static List<BPCompareInfo> GetBPCompareInfo(int caseId)
        {
            bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;

            List<BPCompareInfo> infoList = new List<BPCompareInfo>(4);
            List<bpDetail> bpDetailList = ZoncareEntities.Instance.BpDetails.AsNoTracking().Where(o => o.caseId == caseId).ToList();
            bpDetailList = bpDetailList.FindAll(b => (b.errorCode == null || b.errorCode.Trim() == "0"));
            //if (BPTrendFlag.bError)
            //{
                bpDetailList = bpDetailList.FindAll(b => b.dataStatus == 1);
            //}
            //else
            //{
            //    bpDetailList = bpDetailList.FindAll(
            //    b => ((b.sysPressure / 1000 > settingValues.minErrorSys && b.sysPressure / 1000 < settingValues.maxErrorSys) &&
            //    (b.diaPressure / 1000 > settingValues.minErrorDia && b.diaPressure / 1000 < settingValues.maxErrorDia) &&
            //    (b.dataStatus == 1)));
            //}

            List<BPDataInfo> bpAllDataList = CalcBPDataInfo(bpDetailList);
            List<BPDataInfo> bpDayDataList = new List<BPDataInfo>();
            List<BPDataInfo> bpNightDataList = new List<BPDataInfo>();
            bpDayDataList = bpAllDataList.FindAll(b => (IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            bpNightDataList = bpAllDataList.FindAll(b => (!IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));

            BPDetailData allData = new BPDetailData();
            BPDetailData dayData = new BPDetailData();
            BPDetailData nightData = new BPDetailData();

            CalcSummaryData(bpAllDataList, allData, 0);
            CalcSummaryData(bpDayDataList, dayData, 1);
            CalcSummaryData(bpNightDataList, nightData, 2);


            BPCompareInfo compareInfo0 = new BPCompareInfo();
            compareInfo0.Type = "收缩压/舒张压";
            compareInfo0.All = allData.SbpAvg + "/" + allData.DbpAvg;
            compareInfo0.Day = dayData.SbpAvg + "/" + dayData.DbpAvg;
            compareInfo0.Night = nightData.SbpAvg + "/" + nightData.DbpAvg;
            infoList.Add(compareInfo0);

            BPCompareInfo compareInfo1 = new BPCompareInfo();
            compareInfo1.Type = "血压正常值范围";

            string avgSys = "";
            string avgDia = "";
            string avgSysOfDay = "";
            string avgDiaOfDay = "";
            string avgSysOfNight = "";
            string avgDiaOfNight = "";
            if (settingValues.unit == "kPa")
            {
                avgSys = CommonFuns.mmHg2KPa(settingValues.avgSys, 2).ToString("F2");
                avgDia = CommonFuns.mmHg2KPa(settingValues.avgDia, 2).ToString("F2");

                avgSysOfDay = CommonFuns.mmHg2KPa(settingValues.avgSysOfDay, 2).ToString("F2");
                avgDiaOfDay = CommonFuns.mmHg2KPa(settingValues.avgDiaOfDay, 2).ToString("F2");

                avgSysOfNight = CommonFuns.mmHg2KPa(settingValues.avgSysOfNight, 2).ToString("F2");
                avgDiaOfNight = CommonFuns.mmHg2KPa(settingValues.avgDiaOfNight, 2).ToString("F2");
            }
            else
            {
                avgSys = settingValues.avgSys.ToString();
                avgDia = settingValues.avgDia.ToString();

                avgSysOfDay = settingValues.avgSysOfDay.ToString();
                avgDiaOfDay = settingValues.avgDiaOfDay.ToString();

                avgSysOfNight = settingValues.avgSysOfNight.ToString();
                avgDiaOfNight = settingValues.avgDiaOfNight.ToString();
            }

            compareInfo1.All = avgSys + "/" + avgDia;
            compareInfo1.Day = avgSysOfDay + "/" + avgDiaOfDay;
            compareInfo1.Night = avgSysOfNight + "/" + avgDiaOfNight;
            infoList.Add(compareInfo1);

            BPCompareInfo compareInfo2 = new BPCompareInfo();
            compareInfo2.Type = "脉率";
            compareInfo2.All = allData.PulseAvg;
            compareInfo2.Day = dayData.PulseAvg;
            compareInfo2.Night = nightData.PulseAvg;
            infoList.Add(compareInfo2);

            BPCompareInfo compareInfo3 = new BPCompareInfo();
            compareInfo3.Type = "记录次数";
            compareInfo3.All = bpAllDataList.Count.ToString();
            compareInfo3.Day = bpDayDataList.Count.ToString();
            compareInfo3.Night = bpNightDataList.Count.ToString();
            infoList.Add(compareInfo3);

            return infoList;
        }

    }
}
