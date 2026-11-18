using System;
using System.Collections.Generic;
using System.IO;

namespace AnalysisApp.Commons
{
    /// <summary>
    /// 心搏类型枚举
    /// </summary>
    public enum BeatType
    {
        /// <summary>窦性心律</summary>
        Sinus = 0,
        /// <summary>室性早搏</summary>
        PVC = 1,
        /// <summary>房性早搏</summary>
        PAC = 2,
        /// <summary>交界性早搏</summary>
        JPC = 3,
        /// <summary>室上性早搏</summary>
        SVP = 4,
        /// <summary>停搏</summary>
        Pause = 5,
        /// <summary>其他类型</summary>
        Other = 6
    }

    /// <summary>
    /// 心搏形态类型枚举
    /// </summary>
    public enum BeatMorphologyType
    {
        /// <summary>正常形态</summary>
        Normal = 0,
        /// <summary>异常形态</summary>
        Abnormal = 1
    }

    /// <summary>
    /// 心搏信息类
    /// 存储单个R波的相关信息
    /// </summary>
    public struct BeatInfo
    {
        /// <summary>
        /// 在心搏队列中的序号
        /// </summary>
        public int BeatIndex { get; set; }

        /// <summary>
        /// 心搏在数据文件中的绝对位置（采样点索引）
        /// </summary>
        public int Sample { get; set; }

        /// <summary>
        /// 当前心搏与前一个心搏的RR间期（毫秒）
        /// </summary>
        public int RRInterval { get; set; }

        /// <summary>
        /// 前一个心搏的RR间期
        /// </summary>
        public int RRPrevious { get; set; }

        /// <summary>
        /// 后一个心搏的RR间期
        /// </summary>
        public int RRLatter { get; set; }

        /// <summary>
        /// 心搏类型（ASCII字符，如'N'=正常, 'V'=室性早搏, 'A'=房性早搏, 'J'=交界性早搏, 'S'=室上性早搏, 'P'=停搏, 'O'=其他）
        /// </summary>
        public char BeatUiType { get; set; }

        /// <summary>
        /// 心搏形态类型（对应BeatMorphologyType枚举）
        /// </summary>
        public short MorphologyType { get; set; }

        public short MorphologyTypeEx { get; set; }
        /// <summary>
        /// 心拍起点位置
        /// </summary>
        public int StartIndex { get; set; }
        /// <summary>
        /// 心拍终点位置
        /// </summary>
        public int EndIndex { get; set; }

        /// <summary>
        /// 相关数据或状态(房早/室早提早率、间期比率等)
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 心拍相似度
        /// </summary>
        public double Similarity { get; set; }


        public static BeatInfo Empty = new BeatInfo(0, 0, char.MinValue, 0);

        ///// <summary>
        ///// 默认构造函数
        ///// </summary>
        //public BeatInfo()
        //{
        //    Sample = 0;
        //    RRInterval = 0;
        //    BeatUiType = 'N';
        //    MorphologyType = 0;
        //}

        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="position">心搏位置</param>
        /// <param name="rrInterval">RR间期</param>
        /// <param name="type">心搏类型（ASCII字符）</param>
        /// <param name="morphologyType">形态类型</param>
        public BeatInfo(int position, int rrInterval, char type, short morphologyType) : this()
        {
            Sample = position;
            RRInterval = rrInterval;
            BeatUiType = type;
            MorphologyType = morphologyType;
        }

        /// <summary>
        /// 从指定的心搏信息创建新的实例
        /// </summary>
        /// <param name="originalBeat"></param>
        public BeatInfo(BeatInfo originalBeat)
        {
            BeatIndex = originalBeat.BeatIndex;
            Sample = originalBeat.Sample;
            RRInterval = originalBeat.RRInterval;
            RRPrevious = originalBeat.RRPrevious;
            RRLatter = originalBeat.RRLatter;
            BeatUiType = originalBeat.BeatUiType;
            MorphologyType = originalBeat.MorphologyType;
            MorphologyTypeEx = originalBeat.MorphologyTypeEx;
            StartIndex = originalBeat.StartIndex;
            EndIndex = originalBeat.EndIndex;
            State = originalBeat.State;
            Similarity = originalBeat.Similarity;
        }

        public void SetBeatType(char @type)
        {
            BeatUiType = @type;
        }
        public void SetState(int state)
        {
            State = state;
        }

        public static bool operator ==(BeatInfo a, BeatInfo b)
        {
            return !(a != b);
        }

        public static bool operator !=(BeatInfo a, BeatInfo b)
        {
            return a.Sample != b.Sample || a.RRInterval != b.RRInterval
                || a.BeatUiType != b.BeatUiType || a.MorphologyType != b.MorphologyType;
        }
    }

    /// <summary>
    /// 心搏数据读取器
    /// 用于从beats.dat文件中读取R波检测结果
    /// 文件格式：8字节头 + 心搏个数 * 8字节（1个int + 1个short + 2个unsigned char）
    /// </summary>
    public class BeatDataReader
    {
        /// <summary>
        /// 文件头大小（字节）
        /// </summary>
        private const int HeaderSize = 8;

        /// <summary>
        /// 每个心搏记录的字节数
        /// 1个int (4字节) + 1个short (2字节) + 2个unsigned char (1字节+1字节) = 8字节
        /// </summary>
        private const int BeatRecordSize = 8;
        private readonly string filePath;
        private readonly int samplingRate;
        private readonly List<BeatInfo> beats;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath">数据文件路径</param>
        /// <param name="samplingRate">采样率</param>
        public BeatDataReader(string filePath, int samplingRate)
        {
            this.filePath = filePath;
            this.samplingRate = samplingRate;
            beats = new List<BeatInfo>();
            BeatCount = 0;
        }

        /// <summary>
        /// 获取心搏总数
        /// </summary>
        public int BeatCount { get; private set; }

        /// <summary>
        /// 读取所有心搏数据
        /// </summary>
        /// <returns>心搏信息列表</returns>
        public List<BeatInfo> ReadAllBeats()
        {
            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"心搏数据文件未找到: {filePath}");
            }

            try
            {
                // 清空现有数据
                beats.Clear();

                // 打开文件流
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    // 读取文件头（8字节）
                    byte[] header = binaryReader.ReadBytes(HeaderSize);
                    if (header.Length < HeaderSize)
                    {
                        throw new IOException("文件头读取不完整");
                    }

                    // 计算心搏数量（文件大小 - 文件头） / 每个心搏大小
                    long fileSize = fileStream.Length;
                    long dataSize = fileSize - HeaderSize;
                    if (dataSize < 0)
                    {
                        throw new IOException("文件大小小于文件头大小");
                    }

                    BeatCount = (int)(dataSize / BeatRecordSize);
                    beats.Capacity = BeatCount;
                    // 读取所有心搏数据
                    for (int i = 0; i < BeatCount; i++)
                    {
                        BeatInfo beatInfo = ReadBeatRecord(binaryReader, samplingRate);
                        if (beatInfo != default(BeatInfo) && beatInfo.BeatUiType != 'D')
                        {
                            beats.Add(beatInfo);
                        }
                    }
                    BeatCount = beats.Count;

                    for (int i = 0; i < beats.Count; i++)
                    {
                        BeatInfo beatInfo = beats[i];
                        if (i == 0)
                        {
                            beatInfo.RRPrevious = beats[0].RRInterval;
                            beatInfo.RRLatter = beats[1].RRInterval;
                        }
                        else
                        {
                            if (i == 1)
                                beatInfo.RRPrevious = beatInfo.RRInterval;
                            else
                                beatInfo.RRPrevious = beats[i - 1].RRInterval;

                            if (i > 0 && i < beats.Count - 1)
                            {
                                beatInfo.RRLatter = beats[i + 1].RRInterval;
                            }
                            else
                            {
                                beatInfo.RRLatter = beats[i].RRInterval;
                            }
                        }
                        beatInfo.StartIndex = beatInfo.Sample - (int)(beatInfo.RRInterval * samplingRate / 1000.0 * 0.4);
                        beatInfo.StartIndex = Math.Max(0, beatInfo.StartIndex);
                        beatInfo.EndIndex = beatInfo.Sample + (int)(beatInfo.RRLatter * samplingRate / 1000.0 * 0.6);
                        beatInfo.EndIndex = Math.Min(AnaValues.holterConfig.Total - 1, beatInfo.EndIndex);

                        beats[i] = beatInfo;
                    }
                }

                return beats;
            }
            catch (Exception ex)
            {
                throw new IOException($"读取心搏数据时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取指定索引的心搏信息
        /// </summary>
        /// <param name="index">心搏索引</param>
        /// <returns>心搏信息</returns>
        public BeatInfo GetBeat(int index)
        {
            // 如果数据未加载，先加载
            if (beats.Count == 0 && BeatCount > 0)
            {
                _ = ReadAllBeats();
            }

            return index < 0 || index >= beats.Count
                ? throw new ArgumentOutOfRangeException(nameof(index), $"索引超出范围: {index} (有效范围: 0-{beats.Count - 1})")
                : beats[index];
        }

        /// <summary>
        /// 根据位置查找心搏
        /// </summary>
        /// <param name="position">心搏位置</param>
        /// <returns>找到的心搏信息，未找到返回null</returns>
        public BeatInfo FindBeatByPosition(int position)
        {
            // 如果数据未加载，先加载
            if (beats.Count == 0 && BeatCount > 0)
            {
                _ = ReadAllBeats();
            }

            foreach (BeatInfo beat in beats)
            {
                if (beat.Sample == position)
                {
                    return beat;
                }
            }

            return default(BeatInfo);
        }

        /// <summary>
        /// 查找指定位置附近的心搏
        /// </summary>
        /// <param name="position">目标位置</param>
        /// <param name="tolerance">容差范围</param>
        /// <returns>找到的心搏信息，未找到返回null</returns>
        public BeatInfo FindNearestBeat(int position, int tolerance = 10)
        {
            // 如果数据未加载，先加载
            if (beats.Count == 0 && BeatCount > 0)
            {
                _ = ReadAllBeats();
            }

            BeatInfo nearestBeat = default(BeatInfo);
            int minDistance = int.MaxValue;

            foreach (BeatInfo beat in beats)
            {
                int distance = Math.Abs(beat.Sample - position);
                if (distance < minDistance && distance <= tolerance)
                {
                    minDistance = distance;
                    nearestBeat = beat;
                }
            }

            return nearestBeat;
        }

        /// <summary>
        /// 查找指定范围内的所有心搏
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        /// <param name="endPosition">结束位置</param>
        /// <returns>范围内的所有心搏列表</returns>
        public List<BeatInfo> FindBeatsInRange(int startPosition, int endPosition)
        {
            // 如果数据未加载，先加载
            if (beats.Count == 0 && BeatCount > 0)
            {
                _ = ReadAllBeats();
            }

            List<BeatInfo> result = new List<BeatInfo>();

            foreach (BeatInfo beat in beats)
            {
                if (beat.Sample >= startPosition && beat.Sample <= endPosition)
                {
                    result.Add(beat);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取指定类型的所有心搏
        /// </summary>
        /// <param name="type">心搏类型（ASCII字符，如'N', 'V', 'A', 'J', 'S', 'P', 'O'）</param>
        /// <returns>指定类型的心搏列表</returns>
        public List<BeatInfo> GetBeatsByType(char type)
        {
            // 如果数据未加载，先加载
            if (beats.Count == 0 && BeatCount > 0)
            {
                _ = ReadAllBeats();
            }

            List<BeatInfo> result = new List<BeatInfo>();

            foreach (BeatInfo beat in beats)
            {
                if (beat.BeatUiType == type)
                {
                    result.Add(beat);
                }
            }

            return result;
        }

        /// <summary>
        /// 读取单个心搏记录
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <param name="samplingRate">采样率</param>
        /// <returns>心搏信息</returns>
        private BeatInfo ReadBeatRecord(BinaryReader reader, int samplingRate)
        {
            try
            {
                // 检查是否还有足够的数据
                if (reader.BaseStream.Position + BeatRecordSize > reader.BaseStream.Length)
                {
                    return default(BeatInfo);
                }

                // 读取心搏位置（4字节int）
                int position = reader.ReadInt32();

                // 读取RR间期（2字节short）
                double temp = samplingRate / 1000.00;
                int rrInterval = Convert.ToInt32(reader.ReadInt16() / temp);

                // 读取心搏类型（1字节unsigned char）
                char type = (char)reader.ReadByte();

                // 读取形态类型（1字节unsigned char）
                sbyte morphologyTypeByte = reader.ReadSByte();

                return new BeatInfo(position, rrInterval, type, morphologyTypeByte);
            }
            catch (EndOfStreamException)
            {
                // 文件结束，返回null
                return default(BeatInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取心搏记录时发生错误: {ex.Message}");
                return default(BeatInfo);
            }
        }

        /// <summary>
        /// 计算平均RR间期
        /// </summary>
        /// <returns>平均RR间期（毫秒）</returns>
        public float CalculateAverageRRInterval()
        {
            // 如果数据未加载，先加载
            if (beats.Count == 0 && BeatCount > 0)
            {
                _ = ReadAllBeats();
            }

            if (beats.Count == 0)
            {
                return 0;
            }

            long totalRRInterval = 0;
            int count = 0;

            foreach (BeatInfo beat in beats)
            {
                // 排除第一个心搏（没有前一个心搏的RR间期）和无效片段
                if (beat.RRInterval <= 0 || AnaCode.IsInvalidBeat(beat))
                {
                    continue;
                }
                totalRRInterval += beat.RRInterval;
                count++;
            }

            return count == 0 ? 0 : (float)totalRRInterval / count;
        }

        /// <summary>
        /// 计算心率
        /// </summary>
        /// <returns>平均心率（次/分钟）</returns>
        public float CalculateHeartRate()
        {
            float averageRR = CalculateAverageRRInterval();
            if (averageRR <= 0)
            {
                return 0;
            }

            return 60000 / averageRR; // RR间期单位是毫秒
        }

        /// <summary>
        /// 统计各类心搏数量
        /// </summary>
        /// <returns>心搏类型统计字典</returns>
        public Dictionary<char, int> GetBeatTypeStatistics()
        {
            // 如果数据未加载，先加载
            if (beats.Count == 0 && BeatCount > 0)
            {
                _ = ReadAllBeats();
            }

            Dictionary<char, int> statistics = new Dictionary<char, int>();

            foreach (BeatInfo beat in beats)
            {
                if (statistics.ContainsKey(beat.BeatUiType))
                {
                    statistics[beat.BeatUiType]++;
                }
                else
                {
                    statistics[beat.BeatUiType] = 1;
                }
            }

            return statistics;
        }

        /// <summary>
        /// 清除缓存数据
        /// </summary>
        public void Clear()
        {
            beats.Clear();
            BeatCount = 0;
        }
    }
}