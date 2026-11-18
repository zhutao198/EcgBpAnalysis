using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zoncare.Utilities;

namespace Zoncare.Common
{
    public static class ZQECGParseUtils
    {
        public static bool IsEncrypted = false;

        //读取目录数(实际保存的项的数量)
        public static int ReadCatalogCount(byte[] bytes)
        {
            byte[] count = new byte[4];
            Array.Copy(bytes, 8 + 33, count, 0, 4);
            return ByteUtils.BytesToInt2(count, 0);
        }
        //把编号code和地址page对应
        public static Dictionary<int, int> ReadCatalogContent(byte[] bytes)
        {
            int count = ReadCatalogCount(bytes);
            Dictionary<int, int> dic = new Dictionary<int, int>();
            int step = 0;
            int offset = 0;
            int chapter = 0;
            int pages = 0;
            //从128个字节后，8个字节一组，前4个字节表示code，后四个字节表示page(页码、地址）
            for (step = 0; step < count; step++)
            {
                offset = 128 + step * 8;
                byte[] chapterbyte = new byte[4];
                Array.Copy(bytes, offset, chapterbyte, 0, 4);
                chapter = ByteUtils.BytesToInt(chapterbyte, 0);
                offset = 128 + step * 8 + 4;
                byte[] pagesbyte = new byte[4];
                Array.Copy(bytes, offset, pagesbyte, 0, 4);
                pages = ByteUtils.BytesToInt(pagesbyte, 0);
                dic[chapter] = pages;
            }
            return dic;
        }
        //根据编号找到地址，先取前四位，确定size，然后再取接下来size长度的字节数组。
        public static byte[] GetOpt(string no, byte[] by)
        {
            byte[] resultBys = null;
            Dictionary<int, int> dic = ReadCatalogContent(by);
            int addr = 0;
            try
            {
                if (dic.ContainsKey(ByteUtils.HexToInt(no)))
                {
                    addr = dic[ByteUtils.HexToInt(no)];
                    byte[] sizebyte = new byte[4];
                    Array.Copy(by, addr, sizebyte, 0, 4);
                    int size = ByteUtils.BytesToInt(sizebyte, 0);
                    resultBys = new byte[size];
                    Array.Copy(by, addr + 4, resultBys, 0, size);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(new Exception("读取zqecg文件(编号 " + no + ")出错:" + ex.Message), "心电文件");
            }
            return resultBys;
        }
        public static int? GetInt(string no, byte[] b)
        {
            byte[] dataBytes = GetOpt(no, b);
            if (dataBytes != null)
                return ByteUtils.BytesToInt(dataBytes, 0);
            else
                return null;
        }
        public static float? GetFloat(string no, byte[] b)
        {
            byte[] dataBytes = GetOpt(no, b);
            if (dataBytes != null)
                return ByteUtils.ByteToFloat(dataBytes, 0);
            else
                return null;
        }
        public static string GetDate(string no, byte[] b)
        {
            return ByteToDate(GetOpt(no, b));
        }
        public static string GetCutString(string no, byte[] b)
        {
            string rawText = ByteUtils.StrCut(GetString(no, b));
            if (IsEncrypted && (no == ZQECGConfig.ECG_NO_PATIENT_ID || no == ZQECGConfig.ECG_NO_PATIENT_NAME))
            {
                return AESEncrypt.DecryptFromBase64(rawText, "");
            }
            else
            {
                return rawText;
            }
        }
        public static string GetString(string no, byte[] b)
        {
            byte[] content = GetOpt(no, b);
            return ByteUtils.ByteToString(content);
        }

        //读取zqecg文件的时间，长度为44，只取4*6个字节，精确到秒
        public static string ByteToDate(byte[] bytes)
        {
            string date = "";
            try
            {
                List<string> arrTime = new List<string>();
                for (int index = 0; index < 6; index++)
                {
                    int t = ByteUtils.BytesToInt(bytes, 0 + index * 4);
                    if (index == 4)
                    {
                        t++;
                    }
                    string str = t.ToString();
                    if (t >= 0 && t <= 9)
                    {
                        str = "0" + str;
                    }
                    arrTime.Add(str);
                }
                arrTime.Reverse();

                string[] ymd = new string[] { arrTime[0], arrTime[1], arrTime[2] };
                arrTime.RemoveRange(0, 3);
                date = string.Join("-", ymd) + " " + string.Join(":", arrTime.ToArray());

                if (date.StartsWith("0"))
                {
                    date = "0000-00-00 00:00:00";
                }
            }
            catch (Exception e)
            {
                return date;
            }
            return date;
        }

        /// <summary>
        /// 获取波形数据
        /// </summary>
        /// <param name="waveformBytes">波形数据字节数组</param>
        /// <param name="leadCnt">导联数</param>
        /// <param name="waveformDataNo">波形数据对应的编号</param>
        /// <param name="timeMs">数据总时长</param>
        /// <param name="ecgType">心电图类型</param>
        /// <param name="machineType">机器类型</param>
        /// <returns></returns>
        public static Dictionary<int, PointD[]> GetWaveformData(byte[] waveformBytes, int leadCnt, int timeMs, int ecgType, int machineType, int samplingRate, int rhythmLeadIndex = 0)
        {
            if (leadCnt <= 0) return null;
            Dictionary<int, PointD[]> waveformData = new Dictionary<int, PointD[]>();
            if (waveformBytes != null)
            {
                int gpCnt = timeMs * samplingRate / 1000; //数据组数(每组可能有1个或多个导联数据)
                if (ecgType == 6 || ecgType == 7) //心率变异性、节律分析
                {
                    PointD[] singleLead = new PointD[timeMs];
                    if (waveformBytes.Length == gpCnt * 2) //仅包含单个导联波形数据, 无状态
                    {
                        for (int i = 0; i < gpCnt; i++)
                        {
                            singleLead[i] = new PointD(ByteUtils.ByteToShort(waveformBytes, i * 2), 0);
                        }
                    }
                    else if (waveformBytes.Length == gpCnt * 13 * 2) //文件中保存了12导联数据及状态
                    {
                        //各组(12导联)数据在前, 状态在后
                        int gpSize = 13 * 2;
                        for (int i = 0; i < gpCnt; i++)
                        {
                            int offset = i * gpSize;
                            int statusVal = ByteUtils.ByteToShort(waveformBytes, offset + 12 * 2);
                            singleLead[i] = new PointD(ByteUtils.ByteToShort(waveformBytes, offset + rhythmLeadIndex * 2), statusVal, ECG.CHECK_STATU_PACE(statusVal));
                        }
                    }
                    else if (waveformBytes.Length >= gpCnt * 2 * 2) //包含单个导联波形数据及其状态
                    {
                        //所有的状态标识在前, 所有的数据在后
                        int dataOffset = gpCnt * 2;
                        for (int i = 0; i < gpCnt; i++)
                        {
                            int statusVal = ByteUtils.ByteToShort(waveformBytes, i * 2);
                            singleLead[i] = new PointD(ByteUtils.ByteToShort(waveformBytes, dataOffset + i * 2), statusVal, ECG.CHECK_STATU_PACE(statusVal));
                        }
                    }
                    waveformData.Add(rhythmLeadIndex, singleLead);
                }
                else
                {
                    //以下方法使一个时间点各导联只读取一次状态数据
                    //各组(n导联)数据在前, 状态在后
                    //先初始化各导联数据数组
                    int plus = 0;
                    for (int i = 0; i < leadCnt; i++)
                    {
                        int leadIndex = i;
                        if (leadCnt == 15 && i >= 12)
                        {
                            leadIndex += 3;
                        }
                        else if (leadCnt == 9 && i >= 6)
                        {
                            leadIndex = i + plus;
                            plus++;
                        }
                        waveformData[leadIndex] = new PointD[gpCnt];
                    }

                    int gpSize = (leadCnt + 1) * 2; //特定时间点一组数据的大小
                    for (int j = 0; j < gpCnt; j++)
                    {
                        int offset = j * gpSize;

                        int statusVal = ByteUtils.ByteToShort(waveformBytes, offset + leadCnt * 2);
                        bool pacing = ECG.CHECK_STATU_PACE(statusVal);
                        for (int i = 0; i < leadCnt; i++)
                        {
                            PointD[] oneLead = waveformData.ElementAt(i).Value;
                            oneLead[j] = new PointD(ByteUtils.ByteToShort(waveformBytes, offset + i * 2), statusVal, pacing);
                        }
                    }
                }
            }

            return waveformData;
        }

        ///// <summary>
        ///// 采集完成后，将采集的数据点按导联分组(不含状态位数据)
        ///// </summary>
        ///// <param name="dataArray"></param>
        ///// <param name="relatedLeads"></param>
        ///// <returns></returns>
        //public static Dictionary<int, List<short>> GetWaveData(short[] dataArray, Lead[] relatedLeads)
        //{
        //    Dictionary<int, List<short>> waveformData = new Dictionary<int, List<short>>();

        //    for (int i = 0; i < dataArray.Length; i += 19)
        //    {
        //        foreach (Lead leadN in relatedLeads)
        //        {
        //            int leadIndex = leadN.Index % 100;
        //            if (!waveformData.ContainsKey(leadIndex)) waveformData[leadIndex] = new List<short>();

        //            int dataIndex = i + 1 + leadIndex;
        //            waveformData[leadIndex].Add(dataArray[dataIndex]);
        //        }
        //    }

        //    return waveformData;
        //}

        /// <summary>
        /// 采集完成后，将采集的数据点按导联分组(含状态标识数据)
        /// </summary>
        /// <param name="rawData">采集的原始数据(状态在前, 数据紧随其后)</param>
        /// <param name="dataArray">采集的数据(状态在前, 数据紧随其后)</param>
        /// <param name="relatedLeads">相关导联</param>
        /// <param name="convertToShort">状态位是否用short保存(状态实际是int,诊断时需要int,保存时需要short)</param>
        /// <returns></returns>
        public static Dictionary<int, PointD[]> GetLeadWaveData(int[] rawData, short[] dataArray, Lead[] relatedLeads, bool convertToShort = true)
        {
            Dictionary<int, PointD[]> waveformData = new Dictionary<int, PointD[]>();
            //_aquiredData.CopyTo(0, dataArray, 0, dataArray.Length);

            int oneDataLen = rawData.Length / 19;
            for (int i = 0; i < rawData.Length; i += 19)
            {
                int statusVal = 0;
                bool isPacing = false;
                //if (CommonValues.EcgSettings.DeviceType == EcgDeviceType.Device_iMAC)
                //{
                dataArray[i] = (short)(rawData[i] & 0x7FFF);
                statusVal = dataArray[i];
                isPacing = ECG.CHECK_STATU_PACE(statusVal);
                //}
                //else if (CommonValues.EcgSettings.DeviceType == EcgDeviceType.Device_bluetoothDaq)
                //{
                //    isPacing = ECG_bluetooth.CHECK_STATU_PACE(rawData[i]);
                //    byte[] status = ByteUtils.IntToBytes(rawData[i]);
                //    dataArray[i] = ByteUtils.ByteToShort(new byte[] { status[0], (byte)(status[2] << 4) }, 0); //舍弃第2个字节, 并将第3个字节数据左移4位
                //    if (convertToShort)
                //    {
                //        statusVal = dataArray[i];
                //    }
                //    else
                //    {
                //        statusVal = rawData[i];
                //    }
                //}

                foreach (Lead leadN in relatedLeads)
                {
                    int leadIndex = leadN.Index % 100;
                    if (!waveformData.ContainsKey(leadIndex)) waveformData[leadIndex] = new PointD[oneDataLen];

                    int dataIndex = i + 1 + leadIndex;

                    if (rawData[dataIndex] < short.MinValue) rawData[dataIndex] = short.MinValue;
                    else if (rawData[dataIndex] > short.MaxValue) rawData[dataIndex] = short.MaxValue;

                    dataArray[dataIndex] = (short)rawData[dataIndex];
                    PointD pData = new PointD(dataArray[dataIndex], statusVal, isPacing);
                    waveformData[leadIndex][i / 19] = pData;
                }
            }

            return waveformData;
        }

        /// <summary>
        /// 将采集的数据(含状态标识数据) 转换为短整型数据集
        /// </summary>
        /// <param name="leadsData">含状态标识的数据</param>
        /// <param name="includeStatus">是否返回对应于数据值的一组状态数据</param>
        /// <returns></returns>
        public static Dictionary<int, short[]> ConvertEcgDataToShort(Dictionary<int, PointD[]> leadsData, bool includeStatus = false)
        {
            Dictionary<int, short[]> waveData = new Dictionary<int, short[]>();
            Converter<PointD, short> ecgConverter = (pData) => { return pData.Val; };

            foreach (KeyValuePair<int, PointD[]> data1 in leadsData)
            {
                short[] lstData = Array.ConvertAll(data1.Value, ecgConverter);
                waveData[data1.Key] = lstData;
            }

            if (includeStatus)
            {
                PointD[] arr0 = leadsData.ElementAt(0).Value;
                int dataLen = arr0.Length;
                waveData[18] = new short[dataLen]; //数据状态, 对应诊断算法中的ZQLeadType.ecgLead_status
                for (int i = 0; i < dataLen; i++)
                {
                    short statu = (short)arr0[i].Statu;
                    waveData[18][i] = statu;
                }
            }

            return waveData;
        }

        public static void SetOpt(ZQECGFileInfo ecgFile, string ecgNo, string data)
        {
            ECGData ecgData = new ECGData();
            ecgData.Catalog.Chapter = Convert.ToInt32(ecgNo, 16);
            ecgData.Content.Data = Encoding.UTF8.GetBytes(data);//用utf-8编码
            ecgData.Content.Size = ecgData.Content.Data.Length;
            ecgFile.DataList.Add(ecgData);
        }
        public static void SetOpt(ZQECGFileInfo ecgFile, string ecgNo, int data)
        {
            ECGData ecgData = new ECGData();
            ecgData.Catalog.Chapter = Convert.ToInt32(ecgNo, 16);
            ecgData.Content.Data = ByteUtils.IntToBytes(data);
            ecgData.Content.Size = 4;
            ecgFile.DataList.Add(ecgData);
        }
        public static void SetOpt(ZQECGFileInfo ecgFile, string ecgNo, float data)
        {
            ECGData ecgData = new ECGData();
            ecgData.Catalog.Chapter = Convert.ToInt32(ecgNo, 16);
            ecgData.Content.Data = BitConverter.GetBytes(data);
            ecgData.Content.Size = 4;
            ecgFile.DataList.Add(ecgData);
        }
        public static void SetOpt(ZQECGFileInfo ecgFile, string ecgNo, DateTime data)
        {
            ECGData ecgData = new ECGData();
            ecgData.Catalog.Chapter = Convert.ToInt32(ecgNo, 16);
            //data = new DateTime(2022, 4, 28, 10, 35, 35);
            //存时间数据至少是4*6个字节
            byte[] year = ByteUtils.IntToBytes(data.Year);
            byte[] month = ByteUtils.IntToBytes(data.Month - 1);//C++使用struct tm保存时间，月份是从0开始。
            byte[] day = ByteUtils.IntToBytes(data.Day);
            byte[] hour = ByteUtils.IntToBytes(data.Hour);
            byte[] minute = ByteUtils.IntToBytes(data.Minute);
            byte[] second = ByteUtils.IntToBytes(data.Second);
            //byte[] time = year.Concat(month).Concat(day).Concat(hour).Concat(minute).Concat(second).ToArray();
            byte[] time = second.Concat(minute).Concat(hour).Concat(day).Concat(month).Concat(year).ToArray();
            ecgData.Content.Data = time;
            ecgData.Content.Size = 24;
            ecgFile.DataList.Add(ecgData);
        }
        public static void SetOpt(ZQECGFileInfo ecgFile, string ecgNo, short[] data)
        {
            ECGData ecgData = new ECGData();
            ecgData.Catalog.Chapter = Convert.ToInt32(ecgNo, 16);
            byte[] bData = new byte[data.Length * 2];
            int j = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte[] tmpData = BitConverter.GetBytes(data[i]);
                bData[j] = tmpData[0];
                bData[j + 1] = tmpData[1];
                j += 2;
            }
            ecgData.Content.Data = bData;//Array.ConvertAll<short, byte>(data, Convert.ToByte);
            ecgData.Content.Size = data.Length * 2;
            ecgFile.DataList.Add(ecgData);
        }
        public static void SetOpt(ZQECGFileInfo ecgFile, string ecgNo, byte[] data)
        {
            if (data != null)
            {
                ECGData ecgData = new ECGData();
                ecgData.Catalog.Chapter = Convert.ToInt32(ecgNo, 16);
                ecgData.Content.Data = data;
                ecgData.Content.Size = ecgData.Content.Data.Length;
                ecgFile.DataList.Add(ecgData);
            }
        }

        public static void AddTagData(this ZQECGFileInfo ecgFile, string ecgNo, byte[] data)
        {
            if (data != null)
            {
                ECGData ecgData = new ECGData();
                ecgData.Catalog.Chapter = Convert.ToInt32(ecgNo, 16);
                ecgData.Content.Data = data;
                ecgData.Content.Size = data.Length;
                ecgFile.DataList.Add(ecgData);
            }
        }
    }
}
