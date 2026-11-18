using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Zoncare.Common;
using static AnalysisApp.Commons.AnaEvent;

namespace AnalysisApp.Commons
{
    public class DrawEcgWave
    {

        //ECG波形数据
        public List<WaveInfo> EcgData = null;

        //心搏信息
        public List<BeatInfo> pageHeartbeatInfos = null;

        // ECG显示参数
        public float speed { get; set; } = 25;                       //纸速 25mm/s
        public float gain { get; set; } = 2.5f;                       //增益 2.5mm/mV
        public float scale { get; set; } = 1;

        public float LeftMargin { get; set; } = 60;
        public float TopMargin { get; set; } = 30f;

        private float RowHeight = 0f;
        private float RowWidth = 0f;
        private SKRect RowRect = SKRect.Empty;

        public readonly float PixMM = 3.777f;
        public int CurrentRows { get; set; } = 23;
        public int CurrentLeadCount { get; set; } = 1;
        public float WaveformTimeOffsetSeconds { get; set; } = 0;
        public bool ShowRrHrInfo { get; set; } = false;

        public bool bShowGrid { get; set; } = false;

        public List<int> leadIdxs = new List<int>();

        // 每行数据点
        public int RowPoints = 0;

        public int step = 1; // 进一步优化步长，提高性能

        public bool bDrawSmallGrid = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DrawEcgWave()
        {

        }

        /// <summary>
        /// 绘制背景
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawBackground(SKCanvas canvas, int width, int height)
        {
            using (SKPaint backgroundPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill
            })
            // 绘制白色背景
            canvas.DrawRect(0, TopMargin, width, height, backgroundPaint);
            if(bShowGrid)
            {
                // 绘制网格背景
                DrawGrid(canvas, width, height);
            }

        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawGrid(SKCanvas canvas, int width, int height)
        {
            using (SKPaint gridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#C1D6EB"),
                StrokeWidth = 0.6f,
                IsAntialias = true
            })
            using (SKPaint majorGridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke, 
                Color = SKColor.Parse("#77ACE0"),
                StrokeWidth = 0.6f,
                IsAntialias = true
            })
            {
                // 根据ECG医疗标准，只绘制5mm的主要网格
                // 水平网格线（5mm间隔）
                for (float y = TopMargin; y < height; y += 5 * PixMM * scale)
                {
                    canvas.DrawLine(0, y, width, y, majorGridPaint);
                }

                // 垂直网格线（5mm间隔）
                for (float x = 0; x < width; x += 5 * PixMM * scale)
                {
                    canvas.DrawLine(x, TopMargin, x, height, majorGridPaint);
                }

                if(bDrawSmallGrid)
                {
                    // 水平1mm网格线
                    for (float y = TopMargin-1; y < height-1; y += PixMM * scale)
                    {
                        if (Math.Abs(y % (5 * PixMM * scale)) > 0.1f) // 跳过5mm的位置
                        {
                            canvas.DrawLine(0, y, width, y, gridPaint);
                        }
                    }

                    // 垂直1mm网格线
                    for (float x = 1; x < width-1; x += PixMM * scale)
                    {
                        if (Math.Abs(x % (5 * PixMM * scale)) > 0.1f) // 跳过5mm的位置
                        {
                            canvas.DrawLine(x, 0, x, height, gridPaint);
                        }
                    }
                }
            }
        }

        public void DrawEcgWaveformEx(SKCanvas canvas, int width, int height)
        {
            if (EcgData == null || EcgData.Count == 0 || EcgData[0].DataLength < 2)
            {
                return;
            }

            float rowHeight = (height - TopMargin) / (float)CurrentRows;
            float rowWidth = width - LeftMargin;

            using (SKPath path = new SKPath())
            {
                for (int row = 0; row < CurrentRows; row++)
                {
                    int startIndex = (row * RowPoints) + Convert.ToInt32(WaveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
                    int endIndex = Math.Min(startIndex + RowPoints, AnaValues.holterConfig.Total - 1);

                    if (startIndex >= endIndex)
                    {
                        continue;
                    }
                    
                    float leadSpacing = rowHeight / CurrentLeadCount;
                    float baseYOffset = (rowHeight * row) + leadSpacing/2 + TopMargin;
                    float startX = LeftMargin;

                    for (int lead = 0; lead < CurrentLeadCount; lead++)
                    {
                        float yOffset = baseYOffset + (leadSpacing * lead);
                        float baseY = yOffset;

                        DrawWaveformForLeadEx(canvas, path, leadIdxs.Count > lead ? leadIdxs[lead] : lead, 
                                            startIndex, endIndex, startX, baseY);
                    }
                }
            }
        }

        /// <summary>
        /// 扩展的导联波形绘制方法（支持心搏类型颜色映射）
        /// </summary>
        private void DrawWaveformForLeadEx(SKCanvas canvas, SKPath path, int lead, int startIndex, int endIndex, float startX, float baseY)
        {
            if (lead >= EcgData.Count)
            {
                return;
            }

            float[] leadData = EcgData[lead].WaveDataF;
            if (leadData.Length == 0)
            {
                return;
            }

            // 计算实际数据范围
            int timeOffset = Convert.ToInt32(WaveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
            int dataStartIndex = Math.Max(0, startIndex - timeOffset);
            int dataEndIndex = Math.Min(leadData.Length - 1, endIndex - timeOffset);

            if (dataStartIndex >= dataEndIndex)
            {
                return;
            }

            // 如果没有心搏信息，使用默认颜色绘制整个波形
            if (pageHeartbeatInfos == null || pageHeartbeatInfos.Count == 0)
            {
                DrawWaveformSegment(canvas, path, leadData, startIndex, endIndex, startX, baseY, 
                                  timeOffset, dataStartIndex, dataEndIndex, 'N');
                return;
            }

            // 获取当前页面相关的心搏信息
            List<BeatInfo> relevantHeartbeats = pageHeartbeatInfos
                .Where(h => (h.EndIndex > startIndex && h.StartIndex < endIndex) && !AnaCode.IsInvalidBeatEx(h))
                .OrderBy(h => h.StartIndex)
                .ToList();

            if (relevantHeartbeats.Count == 0)
            {
                // 没有相关心搏，使用默认颜色绘制
                DrawWaveformSegment(canvas, path, leadData, startIndex, endIndex, startX, baseY, 
                                  timeOffset, dataStartIndex, dataEndIndex, 'N');
                return;
            }

            // 按心搏分段绘制波形
            int currentIndex = dataStartIndex;
            char prevBeatType = 'N'; // 默认类型

            for (int i = 0; i < relevantHeartbeats.Count; i++)
            {
                BeatInfo heartbeat = relevantHeartbeats[i];
                
                // 计算心搏在数据中的索引范围
                int heartbeatStartDataIndex = Math.Max(dataStartIndex, heartbeat.StartIndex - timeOffset);
                int heartbeatEndDataIndex = Math.Min(dataEndIndex, heartbeat.EndIndex - timeOffset);

                // 绘制心搏前的数据（使用前一个心搏类型）
                if (heartbeatStartDataIndex > currentIndex)
                {
                    DrawWaveformSegment(canvas, path, leadData, startIndex, endIndex, startX, baseY, 
                                      timeOffset, currentIndex, heartbeatStartDataIndex, prevBeatType);
                }

                // 绘制当前心搏数据
                if (heartbeatStartDataIndex <= heartbeatEndDataIndex)
                {
                    DrawWaveformSegment(canvas, path, leadData, startIndex, endIndex, startX, baseY, 
                                      timeOffset, heartbeatStartDataIndex, heartbeatEndDataIndex, heartbeat.BeatUiType);
                    prevBeatType = heartbeat.BeatUiType;
                    currentIndex = heartbeatEndDataIndex + 1;
                }
            }

            // 绘制剩余数据
            if (currentIndex <= dataEndIndex)
            {
                DrawWaveformSegment(canvas, path, leadData, startIndex, endIndex, startX, baseY, 
                                  timeOffset, currentIndex, dataEndIndex, prevBeatType);
            }
        }

        /// <summary>
        /// 绘制波形段（使用指定心搏类型的颜色）
        /// </summary>
        private void DrawWaveformSegment(SKCanvas canvas, SKPath path, float[] leadData, 
                                        int startIndex, int endIndex, float startX, float baseY,
                                        int timeOffset, int segmentStart, int segmentEnd, char beatType)
        {
            if (segmentStart >= segmentEnd)
            {
                return;
            }

            // 创建对应心搏类型的画笔
            using (var paint = CreateBeatTypePaint(beatType))
            {
                // 重置路径
                path.Reset();

                // 计算第一个点
                float firstX = startX + ((segmentStart + timeOffset - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate);
                float firstY = baseY - (leadData[segmentStart] * gain * scale * PixMM);
                path.MoveTo(firstX, firstY);

                // 高性能绘制循环
                for (int i = segmentStart + 1; i <= segmentEnd; i += step)
                {
                    float currentX = startX + ((i + timeOffset - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate);
                    float currentY = baseY - (leadData[i] * gain * scale * PixMM);
                    path.LineTo(currentX, currentY);
                }

                // 确保绘制最后一个点
                if ((segmentEnd - segmentStart) % step != 0)
                {
                    float lastX = startX + ((segmentEnd + timeOffset - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate);
                    float lastY = baseY - (leadData[segmentEnd] * gain * scale * PixMM);
                    path.LineTo(lastX, lastY);
                }

                // 绘制路径
                if (path.PointCount > 0)
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        /// <summary>
        /// 创建对应心搏类型的画笔
        /// </summary>
        private SKPaint CreateBeatTypePaint(char beatType)
        {
            var paint = new SKPaint
            {
                StrokeWidth = 1.2f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            // 使用WaveformColorMap获取颜色，如果不存在则使用默认黑色
            if (AnaValues.WaveformColorMap.ContainsKey(beatType))
            {
                paint.Color = SKColor.Parse(AnaValues.WaveformColorMap[beatType]);
            }
            else
            {
                paint.Color = SKColors.Black; // 默认颜色
            }

            return paint;
        }

        public void DrawEcgWaveform(SKCanvas canvas, int width, int height)
        {

            if (EcgData.Count == 0 || EcgData[leadIdxs[0]].DataLength < 2)
            {
                return;
            }

            //RowRect = new SKRect(LeftMargin, TopMargin, width, height);


            RowHeight = (height - TopMargin) / (float)CurrentRows;
            RowWidth = width - LeftMargin;

            //页扫描计算纸速，rowWidth是像素点个数，rowWidth/PixMM计算出已mm为单位的距离，用距离/时间就是速度

            using (SKPath path = new SKPath())
            {
                for (int row = 0; row < CurrentRows; row++)
                {
                    if(row == 0)
                    {
                        RowRect = new SKRect(LeftMargin, TopMargin + row * RowHeight, width, TopMargin + (row + 4) * RowHeight);
                    }
                    else if(row == 1)
                    {
                        RowRect = new SKRect(LeftMargin, TopMargin + (row - 1) * RowHeight, width, TopMargin + (row + 3) * RowHeight);
                    }
                    else
                    {
                        RowRect = new SKRect(LeftMargin, TopMargin + (row - 2) * RowHeight, width, TopMargin + (row + 2) * RowHeight);
                    }

                    int startIndex = (row * RowPoints) + Convert.ToInt32(WaveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
                    int endIndex = Math.Min(startIndex + RowPoints, AnaValues.holterConfig.Total - 1);

                    if (startIndex >= endIndex)
                    {
                        continue;
                    }
                    float leadSpacing = RowHeight / CurrentLeadCount;
                    float baseYOffset = (RowHeight * row) + leadSpacing/2 + TopMargin;
                    float startX = LeftMargin;

                    for (int lead = 0; lead < CurrentLeadCount; lead++)
                    {
                        float yOffset = baseYOffset + (leadSpacing * lead);
                        float baseY = yOffset;

                        DrawWaveformForLead(canvas, path, leadIdxs[lead], startIndex, endIndex, startX, baseY);
                    }
                }
            }
        }

        private void DrawWaveformForLead(SKCanvas canvas, SKPath path, int lead, int startIndex, int endIndex, float startX, float baseY)
        {
            int currentIndex = startIndex;
            float prevSegmentEndX = 0, prevSegmentEndY = 0;
            bool hasPrevSegmentEnd = false;
            char prevBeatType = 'N'; // 保存前一个心搏的类型

            // 优化：预先过滤当前页面的心搏类型
            List<BeatInfo> relevantHeartbeats = pageHeartbeatInfos
                .Where(h => h.EndIndex > startIndex && h.StartIndex < endIndex)
                .OrderBy(h => h.StartIndex)
                .ToList();

            int heartbeatIndex = 0;

            while (currentIndex < endIndex)
            {
                BeatInfo matchingHeartbeat = BeatInfo.Empty;

                // 优化：使用索引直接查找，避免重复遍历
                while (heartbeatIndex < relevantHeartbeats.Count)
                {
                    BeatInfo heartbeat = relevantHeartbeats[heartbeatIndex];
                    if (currentIndex >= heartbeat.StartIndex && currentIndex < heartbeat.EndIndex)
                    {
                        matchingHeartbeat = heartbeat;
                        break;
                    }
                    else if (heartbeat.StartIndex > currentIndex)
                    {
                        break;
                    }
                    heartbeatIndex++;
                }
                if (matchingHeartbeat.Sample != 0)
                {
                    char currentBeatType = matchingHeartbeat.BeatUiType;
                    int beatEndIdx = Math.Min(matchingHeartbeat.EndIndex, endIndex);
                    AEcgHandler.SegmentInfo invalidSegment = AnaCode.isInvalidWave(currentIndex, beatEndIdx);
                    if (invalidSegment.StartIndex != 0 && invalidSegment.EndIndex != 0)
                    {
                        beatEndIdx = Math.Min(beatEndIdx, invalidSegment.StartIndex);
                    }

                    DrawSegment(canvas, path, lead, startIndex, currentIndex, beatEndIdx,
                               startX, baseY, currentBeatType, ref prevSegmentEndX, ref prevSegmentEndY, ref hasPrevSegmentEnd);
                    prevBeatType = currentBeatType; // 保存当前心搏类型
                    currentIndex = beatEndIdx;
                    heartbeatIndex++; // 移动到下一个心搏
                }
                else
                {
                    BeatInfo nextHeartbeat = BeatInfo.Empty;

                    // 优化：从当前位置开始查找下一个心搏
                    for (int i = heartbeatIndex; i < relevantHeartbeats.Count; i++)
                    {
                        BeatInfo heartbeat = relevantHeartbeats[i];
                        if (heartbeat.StartIndex > currentIndex && heartbeat.StartIndex < endIndex)
                        {
                            nextHeartbeat = heartbeat;
                            break;
                        }
                    }

                    if (nextHeartbeat.Sample != 0)
                    {
                        int nextIndex = nextHeartbeat.StartIndex;
                        char beatType = prevBeatType;
                        AEcgHandler.SegmentInfo invalidSegment = AnaCode.isInvalidWave(currentIndex, nextHeartbeat.StartIndex);
                        if(invalidSegment.StartIndex !=0 && invalidSegment.EndIndex != 0)
                        {
                            beatType = 'N';
                            currentIndex = invalidSegment.StartIndex;
                            nextIndex = invalidSegment.EndIndex;
                        }
                        else
                        {
                            if(nextHeartbeat.RRInterval == -1)
                            {
                                beatType = 'N';
                            }
                        }
                        // 衔接处使用前一个心搏的类型
                        DrawSegment(canvas, path, lead, startIndex, currentIndex, nextIndex,
                                   startX, baseY, beatType, ref prevSegmentEndX, ref prevSegmentEndY, ref hasPrevSegmentEnd);
                        currentIndex = nextIndex;
                        heartbeatIndex = relevantHeartbeats.IndexOf(nextHeartbeat);
                    }
                    else
                    {
                        AEcgHandler.SegmentInfo invalidSegment = AnaCode.isInvalidWave(currentIndex, endIndex);
                        if (invalidSegment.StartIndex != 0 && invalidSegment.EndIndex != 0)
                        {
                            if (currentIndex < invalidSegment.EndIndex && endIndex >= invalidSegment.StartIndex)
                            {
                                prevBeatType = 'N';
                            }
                        }
                        // 剩余数据也使用前一个心搏的类型
                        DrawSegment(canvas, path, lead, startIndex, currentIndex, endIndex,
                                   startX, baseY, prevBeatType, ref prevSegmentEndX, ref prevSegmentEndY, ref hasPrevSegmentEnd);
                        break;
                    }
                }
            }

        }

        private void DrawSegment(SKCanvas canvas, SKPath path, int lead, int startIndex, int segmentStart, int segmentEnd,
                                float startX, float baseY, char beatType,
                                ref float prevSegmentEndX, ref float prevSegmentEndY, ref bool hasPrevSegmentEnd)
        {
            if (segmentEnd - segmentStart <= 0)
            {
                return;
            }

            List<SKPaint> paintList = new List<SKPaint>();
            Dictionary<char, SKPaint> waveformPaints = new Dictionary<char, SKPaint>();
            try
            {
                foreach (var item in AnaValues.WaveformColorMap)
                {
                    var paint = new SKPaint
                    {
                        Color = SKColor.Parse(item.Value),
                        StrokeWidth = 1.2f,
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke
                    };

                    waveformPaints[item.Key] = paint;
                    paintList.Add(paint);
                }

                path.Reset();

                // 预计算常量，避免重复计算
                int timeOffset = Convert.ToInt32(WaveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
                float[] leadData = EcgData[lead].WaveDataF;

                // 设置路径起始点
                if (hasPrevSegmentEnd)
                {
                    path.MoveTo(prevSegmentEndX, prevSegmentEndY);
                }
                else
                {
                    int dataIndex = segmentStart - timeOffset;
                    if (dataIndex >= 0 && dataIndex < leadData.Length)
                    {
                        path.MoveTo(startX + ((segmentStart - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate),
                                   baseY - (leadData[dataIndex] * gain * scale * PixMM));
                    }
                }

                // 高性能绘制循环：优化内存访问和计算
                int lastDrawnIndex = segmentStart;

                // 预计算循环边界
                int loopEnd = Math.Min(segmentEnd, startIndex + leadData.Length + timeOffset);

                // 使用局部变量优化循环性能
                float currentX, currentY;
                int currentDataIndex;

                for (int i = segmentStart; i < loopEnd; i += step)
                {
                    currentDataIndex = i - timeOffset;
                    if (currentDataIndex >= 0 && currentDataIndex < leadData.Length)
                    {
                        currentX = startX + ((i - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate);
                        currentY = baseY - (leadData[currentDataIndex] * gain * scale * PixMM);
                        path.LineTo(currentX, currentY);
                        lastDrawnIndex = i;
                    }
                }

                // 确保绘制最后一个点
                int lastDataIndex = loopEnd - 1 - timeOffset;
                if (lastDataIndex >= 0 && lastDataIndex < leadData.Length && lastDrawnIndex < loopEnd - 1)
                {
                    currentX = startX + ((loopEnd - 1 - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate);
                    currentY = baseY - (leadData[lastDataIndex] * gain * scale * PixMM);
                    path.LineTo(currentX, currentY);
                    lastDrawnIndex = loopEnd - 1;
                }

                if (path.PointCount > 0)
                {
                    SKPaint paint = waveformPaints.ContainsKey(beatType) ? waveformPaints[beatType] : waveformPaints['N'];
                    canvas.Save();
                    canvas.ClipRect(RowRect);
                    canvas.DrawPath(path, paint);
                    canvas.Restore();
                }

                // 保存段结束点
                if (lastDrawnIndex >= segmentStart)
                {
                    int endDataIndex = lastDrawnIndex - timeOffset;
                    if (endDataIndex >= 0 && endDataIndex < leadData.Length)
                    {
                        prevSegmentEndX = startX + ((lastDrawnIndex - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate);
                        prevSegmentEndY = baseY - (leadData[endDataIndex] * gain * scale * PixMM);
                        hasPrevSegmentEnd = true;
                    }
                }
            }
            finally
            {
                // 释放所有 SKPaint 资源
                foreach (var paint in paintList)
                {
                    paint?.Dispose();
                }
            }

 
        }

        public void DrawPaceStatus(SKCanvas canvas, int width, int height)
        {
            if (EcgData.Count == 0 || EcgData[leadIdxs[0]].DataLength < 2)
            {
                return;
            }

            RowHeight = (height - TopMargin) / (float)CurrentRows;
            RowWidth = width - LeftMargin;

            using (SKPaint linePaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f,
            })
            {
                for (int row = 0; row < CurrentRows; row++)
                {
                    float leadSpacing = RowHeight / CurrentLeadCount;
                    float baseYOffset = (RowHeight * row) + leadSpacing / 2 + TopMargin;
                    float startX = LeftMargin;

                    int lead = leadIdxs[0];
                    int[] statusData = EcgData[lead].StatusData;
                    for(int i=0; i< statusData.Length; i++)
                    {
                        if ((statusData[i] & 0x4000) == 0x4000)
                        {
                            float lineX = startX + (i * speed * scale * PixMM / AnaValues.holterConfig.SampleRate);

                            canvas.DrawLine(lineX, height - 2.5f*PixMM, lineX, height, linePaint);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 绘制时间轴
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawTimeAxis(SKCanvas canvas, int width, int height, int rowSeconds)
        {
            if (AnaValues.holterConfig == null || string.IsNullOrEmpty(AnaValues.holterConfig.StartTime))
            {
                return;
            }
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                // 解析起始时间
                if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
                {
                    startTime = DateTime.Today;
                }

                float rowHeight = (height - TopMargin) / (float)CurrentRows;

                for (int i = 0; i < CurrentRows; i++)
                {
                    if (rowSeconds == 30 && i % 2 != 0)
                    {
                        continue;
                    }
                    // 计算当前行对应的时间
                    float totalSeconds = WaveformTimeOffsetSeconds + (i * rowSeconds);
                    if(totalSeconds > AnaValues.holterConfig.TotalSeconds)
                    {
                        break;
                    }
                    // 格式化时间字符串
                    DateTime currentTime = startTime.AddSeconds(totalSeconds);
                    string timeStr = currentTime.ToString("HH:mm:ss");
                    // 绘制时间标签
                    float xPos = 5;
                    float yPos = (rowHeight * (float)(i)) + TopMargin + PixMM*5;
                    canvas.DrawText(timeStr, xPos, yPos, SKTextAlign.Left, textFont, textPaint);

                    int minute = Convert.ToInt32(totalSeconds / 60);
                    if (minute > AnaValues.minuteHR.Count - 1)
                    {
                        break;
                    }
                    
                    if(AnaValues.minuteHR[minute].Avg > 0)
                    {
                        string avgHR = (AnaValues.minuteHR[minute].Avg).ToString() + "bpm";
                        canvas.DrawText(avgHR, xPos, yPos + 3 * PixMM, SKTextAlign.Left, textFont, textPaint);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制当前波形起始时间
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawCurrentWaveformStartTime(SKCanvas canvas, int width, int height)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                if (AnaValues.holterConfig == null || string.IsNullOrEmpty(AnaValues.holterConfig.StartTime))
                {
                    return;
                }

                // 解析Holter记录的起始时间
                if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
                {
                    startTime = DateTime.Today;
                }

                // 计算当前波形起始时间：Holter起始时间 + 波形时间偏移
                DateTime currentWaveformStartTime = startTime.AddSeconds(WaveformTimeOffsetSeconds);

                // 格式化时间字符串
                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

                string timeStr = currentWaveformStartTime.ToString(dateFormat);

                // 计算文本位置（左下角，留出边距）
                float xPos = LeftMargin+PixMM; // 左边距
                float yPos = height - 6; // 底部边距

                // 绘制当前波形起始时间
                canvas.DrawText(timeStr, xPos, yPos, SKTextAlign.Left, textFont, textPaint);
            }
 
        }

        /// <summary>
        /// 绘制导联名称和定标电压
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawLeadNames(SKCanvas canvas, int width, int height, float rowSeconds)
        {
            if (AnaValues.holterConfig == null || leadIdxs.Count == 0)
            {
                return;
            }
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Green,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                // 计算每行的高度
                float rowHeight = (height - TopMargin) / (float)CurrentRows;

                for (int row = 0; row < CurrentRows; row++)
                {
                    // 计算当前行对应的时间
                    float totalSeconds = WaveformTimeOffsetSeconds + (row * rowSeconds);
                    if (totalSeconds > AnaValues.holterConfig.TotalSeconds)
                    {
                        break;
                    }
                    float leadSpacing = rowHeight / CurrentLeadCount;
                    float baseYOffset = (rowHeight * row) + leadSpacing / 2 + TopMargin;

                    for (int lead = 0; lead < CurrentLeadCount; lead++)
                    {
                        if (lead >= leadIdxs.Count)
                            continue;

                        int leadIndex = leadIdxs[lead];
                        float yOffset = baseYOffset + (leadSpacing * lead);

                        // 获取导联名称
                        string leadName = AnaValues.LeadIdxToNameMap.ContainsKey(leadIndex) ? AnaValues.LeadIdxToNameMap[leadIndex] : CommonValues.Langs["导联"] +"${ leadIndex + 1}";

                        // 计算文本位置（左侧，导联波形中间）

                        // 绘制导联名称
                        canvas.DrawText(leadName, LeftMargin+2*PixMM, yOffset-2*PixMM, SKTextAlign.Center, textFont, textPaint);
                    }
                }
            }

 
        }

        /// <summary>
        /// 绘制定标电压
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawCalibration(SKCanvas canvas, int width, int height)
        {
            if (AnaValues.holterConfig == null || leadIdxs.Count == 0)
            {
                return;
            }

            // 计算每行的高度
            float rowHeight = (height - TopMargin) / (float)CurrentRows;

            // 计算定标电压（1mV对应的像素高度）
            float calibrationHeight = gain * scale * PixMM; // 1mV = gain * 每毫米像素数

            for (int row = 0; row < CurrentRows; row++)
            {
                float leadSpacing = rowHeight / CurrentLeadCount;
                float baseYOffset = (rowHeight * row) + leadSpacing / 2 + TopMargin;

                for (int lead = 0; lead < CurrentLeadCount; lead++)
                {
                    if (lead >= leadIdxs.Count)
                        continue;

                    int leadIndex = leadIdxs[lead];
                    float yOffset = baseYOffset + (leadSpacing * lead);

                    // 可选：绘制定标电压图形标记（1mV的垂直线段）
                    float calibrationX = 2*PixMM; // 定标标记的水平位置
                    float calibrationStartY = yOffset - calibrationHeight / 2;
                    float calibrationEndY = yOffset + calibrationHeight / 2;

                    SKPaint calibrationPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        StrokeWidth = 1.5f,
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke
                    };

                    canvas.DrawLine(calibrationX - 1 * PixMM, calibrationEndY, calibrationX, calibrationEndY, calibrationPaint);

                    canvas.DrawLine(calibrationX, calibrationStartY, calibrationX, calibrationEndY, calibrationPaint);

                    canvas.DrawLine(calibrationX, calibrationStartY, calibrationX + 5 * PixMM, calibrationStartY, calibrationPaint);

                    canvas.DrawLine(calibrationX + 5 * PixMM, calibrationStartY, calibrationX + 5 * PixMM, calibrationEndY, calibrationPaint);

                    canvas.DrawLine(calibrationX + 5 * PixMM, calibrationEndY, calibrationX + 6 * PixMM, calibrationEndY, calibrationPaint);
                }
            }
        }

        /// <summary>
        /// 绘制心搏信息
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawBeatHeartInfo(SKCanvas canvas, int width, float height)
        {
            using (SKPaint beatHeartBorderRectPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(49, 56, 72, 255),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
            })
            using (SKPaint beatHeartFillRectPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(233, 236, 239, 255),
                Style = SKPaintStyle.Fill,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            using (SKPaint rectPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(108, 108, 108, 55),
                Style = SKPaintStyle.Fill,
            })
            using (SKPaint NcaPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 51, 204),
                Style = SKPaintStyle.Fill,
            })
            {
                List<EventInfo> events = ReadNcaOrFBeat("房早未下传");
                //List<EventInfo> events = new List<EventInfo>();
                //if (AllEvents.ContainsKey("房早未下传"))
                //{
                //    events = AllEvents["房早未下传"];
                //}
                Dictionary<char, SKPaint> beatHeartInfoPaints = new Dictionary<char, SKPaint>();

                foreach (var item in AnaValues.WaveformColorMap)
                {
                    beatHeartInfoPaints[item.Key] = new SKPaint
                    {
                        Color = SKColor.Parse(item.Value),
                        IsAntialias = true
                    };
                }

                float rowHeight = height;
                float rowWidth = width;
                SKRect borederRect = new SKRect(0, 0, width, height);
                canvas.DrawRect(borederRect, beatHeartBorderRectPaint);
                SKRect fillRect = new SKRect(1.5f, 1.5f, width - 1.5f, height - 1.5f);
                canvas.DrawRect(fillRect, beatHeartFillRectPaint);
                for (int row = 0; row < CurrentRows; row++)
                {
                    int absoluteRow = row;
                    int startIndex = (absoluteRow * RowPoints) + Convert.ToInt32(WaveformTimeOffsetSeconds * AnaValues.holterConfig.SampleRate);
                    int endIndex = Math.Min(startIndex + RowPoints, AnaValues.holterConfig.Total - 1);

                    if (startIndex >= endIndex)
                    {
                        continue;
                    }
                    float baseYOffset = (rowHeight * row) + (rowHeight * 0.1f) + TopMargin;
                    float startX = LeftMargin;

                    List<BeatInfo> relevantHeartbeats = pageHeartbeatInfos
                    .Where(h => (h.EndIndex > startIndex && h.StartIndex < endIndex))
                    .OrderBy(h => h.StartIndex)
                    .ToList();
                    // 绘制RR间期和HR值（从第二个心搏开始）
                    if (relevantHeartbeats.Count > 1)
                    {
                        // 绘制位置在波形上方
                        float textY = baseYOffset - 3 * PixMM;

                        for (int i = 0; i < relevantHeartbeats.Count; i++)
                        {
                            //Console.WriteLine(i + "----type: " + relevantHeartbeats[i].Type);
                            BeatInfo currentBeat = relevantHeartbeats[i];
                            // 计算Sample点（R波）对应的X坐标
                            float sampleX = startX + ((currentBeat.Sample - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate);
                            if (sampleX < LeftMargin)
                            {
                                continue;
                            }
                            // 获取心搏类型
                            char beatType = currentBeat.BeatUiType;
                            // 绘制心搏类型
                            canvas.DrawText(AnaValues.heartbeatTypeMap[beatType.ToString()], sampleX, textY, SKTextAlign.Center, textFont, beatHeartInfoPaints[beatType]);

                            if (currentBeat.RRInterval <= 0) continue;

                            for (int j = 0; j < events.Count; j++)
                            {
                                if (currentBeat.Sample == events[j].Sample && i > 0)
                                {
                                    canvas.DrawText("P", sampleX - (currentBeat.RRInterval - events[j].Offset) / 1000f * speed * scale * PixMM, textY, SKTextAlign.Left, textFont, NcaPaint);
                                }
                            }
                            
                            if (i > 0)
                            {
                                BeatInfo prevBeat = relevantHeartbeats[i - 1];
                                if (prevBeat.BeatUiType == 'X')
                                {
                                    continue;
                                }
                            }

                            if (beatType == 'X')
                            {
                                continue;
                            }

                            if (AnaCode.IsInvalidBeatEx(relevantHeartbeats[i]))
                            {
                                continue;
                            }

                            float middleX = startX + (currentBeat.Sample - startIndex) * speed * scale * PixMM / AnaValues.holterConfig.SampleRate - currentBeat.RRInterval / 1000f / 2 * speed * scale * PixMM;
                            if (middleX < LeftMargin)
                            {
                                continue;
                            }
                            // 计算心率
                            int hr = (int)(60000F / currentBeat.RRInterval);

                            // 绘制RR间期和HR值（不带单位）
                            string rrStr = $"{currentBeat.RRInterval}";
                            string hrStr = $"{hr}";

                            // 绘制RR间期（上方）
                            canvas.DrawText(rrStr, middleX, textY - 10, SKTextAlign.Center, textFont, textPaint);

                            // 绘制HR值（下方）
                            canvas.DrawText(hrStr, middleX, textY + 2, SKTextAlign.Center, textFont, textPaint);
                        }
                    }
                }
            }
 
        }

        /// <summary>
        /// 绘制纸速增益信息
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawSpeedGainInfo(SKCanvas canvas, int width, int height)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                string speedGainStr = $"{gain}mm/mV {speed}mm/s";
                canvas.DrawText(speedGainStr, width - 5*PixMM, height - 3*PixMM, SKTextAlign.Right, textFont, textPaint);
            }
        }
    }
}