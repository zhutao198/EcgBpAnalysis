using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    enum RectangleWaveType
    {
        SingleHeartbeat,        // 默认值为 0 单心搏
        MultipleHeartbeat,      // 1 条图
        Segment,                // 2 片段图
        SubTemplate             // 3 子模板
    }

    class DrawRectangleWave
    {
        private readonly float PixMM = SysConstants.PIX_SCALE;

        private readonly SKColor TopRectFillColor = new SKColor(245, 245, 245, 255);

        private readonly SKColor SelectRectBorderColor = new SKColor(0, 0, 255, 255);
        private readonly SKColor UnSelectRectBorderColor = new SKColor(159, 192, 224, 255);

        private readonly SKColor SelectRectFillColor = new SKColor(0, 100, 255, 20);
        private readonly SKColor UnSelectRectFillColor = new SKColor(231, 242, 255, 255);

        private readonly int TopRectHeight = 18;
        public float WaveLeftMargin { get; set; } = 0;
        private float WaveTopMargin { get; set; } = 0;
        public float WaveRightMargin { get; set; } = 0;
        private float WaveBottomMargin { get; set; } = 0;


        public float Speed { get; set; } = 25;                      //纸速 25mm/s
        public float Gain { get; set; } = 5f;                       //增益 2.5mm/mV

        public SKRect WaveRect { get; set; } = new SKRect();

        public RectangleWaveType WaveType { get; set; } = RectangleWaveType.MultipleHeartbeat;//条图类型

        public List<float[]> EcgData { get; set; } = new List<float[]>();                           //ECG波形数据
        public List<BeatInfo> HeartbeatInfos { get; set; } = new List<BeatInfo>();                  //心搏信息
        public List<string> BottomInfoList { get; set; } = new List<string>();      //所有矩形区域底部信息
        public List<string> BottomSTInfoList { get; set; } = new List<string>();

        public List<int> leadIdxs { get; set; } = new List<int>();                                  //导联索引列表
        public int StartSample { get; set; } = 0;
        public int Sample { get; set; } = 0;
        public int PageHeartBeatCount { get; set; } = 0;
        public int PageHeartBeatIdx { get; set; } = 0;

        private int PointCount { get; set; } = 0;

        public bool bDrawSmallGrid { get; set; } = false;
        public char subTemplateType = 'N';
        public bool bDeleted = false;

        public int CalcV = 0;


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
                canvas.DrawRect(0, 0, width, height, backgroundPaint);

        }
        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawRect(SKCanvas canvas, bool bSelect)
        {
            using (SKPaint rectPaint = new SKPaint())
            {
                rectPaint.IsAntialias = true;

                rectPaint.Style = SKPaintStyle.Stroke;
                if (bSelect)
                {
                    rectPaint.StrokeWidth = 2.0f;
                    rectPaint.Color = SelectRectBorderColor;
                }
                else
                {
                    rectPaint.StrokeWidth = 1.0f;
                    rectPaint.Color = UnSelectRectBorderColor;
                }
                SKRect borderRect = new SKRect(WaveRect.Left, WaveRect.Top, WaveRect.Right, WaveRect.Bottom);
                canvas.DrawRect(borderRect, rectPaint);

                rectPaint.Style = SKPaintStyle.Fill;

                SKRect fillRect = new SKRect();
                if (WaveType != RectangleWaveType.SingleHeartbeat)
                {
                    rectPaint.Color = TopRectFillColor;
                    SKRect topRect = new SKRect(WaveRect.Left + 1, WaveRect.Top + 1, WaveRect.Right - 1, WaveRect.Top + 19);
                    canvas.DrawRect(topRect, rectPaint);

                    fillRect = new SKRect(WaveRect.Left + 1, WaveRect.Top + 19, WaveRect.Right - 1, WaveRect.Bottom - 1);
                }
                else
                {
                    fillRect = new SKRect(WaveRect.Left + 1, WaveRect.Top + 1, WaveRect.Right - 1, WaveRect.Bottom - 1);
                }

                if (bSelect)
                {
                    rectPaint.Color = SelectRectFillColor;
                }
                else
                {
                    rectPaint.Color = UnSelectRectFillColor;
                }
                canvas.DrawRect(fillRect, rectPaint);
            }
        }

        private void getHeartBeatInfos()
        {
            HeartbeatInfos.Clear();
            List<BeatInfo> cachedAllBeats = AnaValues.CachedAllBeats;
            HeartbeatInfos = cachedAllBeats
            .Where(h => h.EndIndex > StartSample && h.StartIndex < StartSample + PointCount)
            .OrderBy(h => h.StartIndex)
            .ToList();

            for (int i = 0; i < HeartbeatInfos.Count; i++)
            {
                if (AnaCode.IsInvalidBeat(HeartbeatInfos[i]))
                {
                    BeatInfo beat = HeartbeatInfos[i];
                    beat.BeatUiType = 'N';
                    HeartbeatInfos[i] = beat;
                }
            }
        }

        public void DrawEcgWave(SKCanvas canvas)
        {
            int firstLead = leadIdxs[0];
            if (EcgData.Count == 0 || EcgData[firstLead].Length < 2) { return; }

            PointCount = EcgData[firstLead].Length;
            WaveTopMargin = TopRectHeight;

            if (WaveType == RectangleWaveType.SingleHeartbeat)
            {
                WaveTopMargin = 0;
            }
            if (WaveType == RectangleWaveType.Segment)
            {
                WaveBottomMargin = 4 * PixMM;
            }
            float rowHeight = WaveRect.Height - WaveTopMargin - WaveBottomMargin;

            if (WaveType != RectangleWaveType.SubTemplate && WaveType != RectangleWaveType.SingleHeartbeat)
            {
                getHeartBeatInfos();
            }
            else
            {
                if (AnaValues.CurrentBeatMap.ContainsKey(Sample))
                {
                    int beatIndex = AnaValues.CurrentBeatMap[Sample];
                    subTemplateType = AnaValues.CachedAllBeats[beatIndex].BeatUiType;
                }

            }

            float drawWaveLeft = WaveLeftMargin;
            float waveWidth = (PointCount * 1.0f / AnaValues.holterConfig.SampleRate) * Speed * PixMM;
            if (waveWidth < WaveRect.Width - (WaveLeftMargin + WaveRightMargin) * 1.2 && StartSample + PointCount != AnaValues.holterConfig.Total - 1)
            {
                float leftWaveWidth = waveWidth - (WaveRect.Width - WaveLeftMargin - WaveRightMargin) / 2;
                drawWaveLeft += (WaveRect.Width - WaveLeftMargin - WaveRightMargin) / 2 - leftWaveWidth;
            }

            float leadSpacing = rowHeight / leadIdxs.Count;
            float baseYOffset = leadSpacing / 2 + WaveRect.Top + WaveTopMargin;
            float startX = WaveRect.Left + drawWaveLeft;

            DrawTopInfo(canvas, drawWaveLeft);

            using (SKPath path = new SKPath())
            {
                for (int lead = 0; lead < leadIdxs.Count; lead++)
                {
                    float yOffset = baseYOffset + (leadSpacing * lead);
                    float baseY = yOffset;
                    if (WaveType != RectangleWaveType.SingleHeartbeat && WaveType != RectangleWaveType.SubTemplate)
                    {
                        DrawLeadNames(canvas, leadIdxs[lead], WaveRect.Left + 2 * PixMM, baseY - leadSpacing / 4);
                    }
                    if (WaveType == RectangleWaveType.SubTemplate || WaveType == RectangleWaveType.SingleHeartbeat)
                    {
                        DrawSubTemplateWaveformForLead(canvas, path, leadIdxs[lead], startX, baseY);
                    }
                    else
                    {
                        DrawWaveformForLead(canvas, path, leadIdxs[lead], startX, baseY);
                    }

                }
            }

            if (BottomInfoList.Count > 0)
            {
                DrawBottomInfo(canvas);
            }
            if (bDeleted)
            {
                DrawDeletedInfo(canvas);
            }
            if (CalcV >= 15)
            {
                DrawCalcInfo(canvas);
            }
        }

        private void DrawDeletedInfo(SKCanvas canvas)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 14f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            {

                float startX = WaveRect.Left + WaveLeftMargin + (WaveRect.Width - WaveLeftMargin - WaveRightMargin) / 2;
                canvas.DrawText("X", startX, WaveRect.Bottom - PixMM, SKTextAlign.Center, textFont, textPaint);

            }
        }

        private void DrawCalcInfo(SKCanvas canvas)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = new SKColor(59, 149, 197, 255),
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 9f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {

                float startX = WaveRect.Left + (WaveRect.Width - WaveRightMargin);
                canvas.DrawText($"{CalcV.ToString()}%", startX, WaveRect.Top + WaveTopMargin + 2 * PixMM, SKTextAlign.Right, textFont, textPaint);

            }
        }

        private void DrawTopInfo(SKCanvas canvas, float leftMargin)
        {
            using (SKPaint textPaint = new SKPaint
            {
                //Color = SKColor.Parse(AnaValues.WaveformColorMap[subTemplateType]),
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                if (WaveType == RectangleWaveType.Segment)
                    textFont.Size = 9.0F;
                else
                    textFont.Size = 9.5F;

                //float leftMargin = 3 * PixMM;
                float textY = WaveRect.Top + TopRectHeight / 2 + 1;

                float startX = WaveRect.Left + leftMargin;
                if (WaveType == RectangleWaveType.MultipleHeartbeat || WaveType == RectangleWaveType.Segment)
                {
                    Dictionary<char, SKPaint> beatHeartInfoPaints = new Dictionary<char, SKPaint>();
                    foreach (var item in AnaValues.WaveformColorMap)
                    {
                        beatHeartInfoPaints[item.Key] = new SKPaint
                        {
                            Color = SKColor.Parse(item.Value),
                            IsAntialias = true
                        };
                    }

                    // 绘制RR间期和HR值（从第二个心搏开始）
                    if (HeartbeatInfos.Count > 1)
                    {
                        for (int i = 0; i < HeartbeatInfos.Count; i++)
                        {
                            //Console.WriteLine(i + "----type: " + relevantHeartbeats[i].Type);
                            BeatInfo currentBeat = HeartbeatInfos[i];
                            if (AnaCode.IsInvalidBeat(currentBeat))
                            {
                                continue;
                            }
                            // 计算Sample点（R波）对应的X坐标
                            float sampleX = startX + ((currentBeat.Sample - StartSample) * Speed * PixMM / AnaValues.holterConfig.SampleRate);
                            if (sampleX < startX || sampleX > WaveRect.Right)
                            {
                                continue;
                            }

                            // 获取心搏类型
                            char beatType = currentBeat.BeatUiType;
                            if (beatType == 'D')
                            {
                                continue;
                            }
                            // 绘制心搏类型
                            canvas.DrawText(AnaValues.heartbeatTypeMap[beatType.ToString()], sampleX, textY, SKTextAlign.Center, textFont, beatHeartInfoPaints[beatType]);
                            if (i > 0)
                            {
                                BeatInfo prevBeat = HeartbeatInfos[i - 1];
                                if (prevBeat.BeatUiType == 'D')
                                {
                                    continue;
                                }
                                if (prevBeat.BeatUiType == 'X')
                                {
                                    continue;
                                }
                            }

                            if (beatType == 'X')
                            {
                                continue;
                            }
                            if (currentBeat.RRInterval < 0) continue;

                            // 计算心率
                            int hr = (int)(60000F / currentBeat.RRInterval);

                            // 绘制RR间期和HR值（不带单位）
                            string rrStr = $"{currentBeat.RRInterval}";
                            string hrStr = $"{hr}";

                            float textWrr = textFont.MeasureText(rrStr, out SKRect bound1, textPaint);
                            float textWhr = textFont.MeasureText(hrStr, out SKRect bound2, textPaint);
                            float textW = Math.Max(textWrr, textWhr);

                            float middleX = startX + (currentBeat.Sample - StartSample) * Speed * PixMM / AnaValues.holterConfig.SampleRate - currentBeat.RRInterval / 1000f / 2 * Speed * PixMM;

                            if (middleX < startX + textW || middleX > WaveRect.Right)
                            {
                                continue;
                            }
                            // 绘制RR间期（上方）
                            canvas.DrawText(rrStr, middleX, textY, SKTextAlign.Center, textFont, textPaint);
                            if (WaveType == RectangleWaveType.Segment)
                            {
                                // 绘制HR值（下方）
                                canvas.DrawText(hrStr, middleX, textY + 8, SKTextAlign.Center, textFont, textPaint);
                            }
                        }
                    }
                }
                else if (WaveType == RectangleWaveType.SubTemplate)
                {
                    canvas.DrawText(PageHeartBeatCount.ToString(), startX + 3, textY + 6, SKTextAlign.Center, textFont, textPaint);

                    canvas.DrawText(AnaValues.heartbeatTypeMap[subTemplateType.ToString()], WaveRect.Left + WaveRect.Width / 2, textY + 6, SKTextAlign.Center, textFont, textPaint);

                    canvas.DrawText(PageHeartBeatIdx.ToString(), WaveRect.Right - 3, textY + 6, SKTextAlign.Right, textFont, textPaint);
                }
            }

        }

        private void DrawBottomInfo(SKCanvas canvas)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = new SKColor(59, 149, 197, 255),
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                SKTextAlign textAlign = SKTextAlign.Left;
                float startX = WaveRect.Left + 2 * PixMM;
                for (int i = 0; i < BottomInfoList.Count; i++)
                {
                    if (i == 1)
                    {
                        startX = WaveRect.Left + WaveRect.Width / 2;
                        textAlign = SKTextAlign.Center;
                    }
                    else if (i == 2)
                    {
                        startX = WaveRect.Right - 2 * PixMM;
                        textAlign = SKTextAlign.Right;
                    }
                    canvas.DrawText(BottomInfoList[i], startX, WaveRect.Bottom - PixMM, textAlign, textFont, textPaint);
                }

                startX = WaveRect.Left + 2 * PixMM;
                textAlign = SKTextAlign.Left;
                for (int i = 0; i < BottomSTInfoList.Count; i++)
                {
                    if (i == 1)
                    {
                        startX = WaveRect.Left + WaveRect.Width / 2;
                    }
                    canvas.DrawText(BottomSTInfoList[i], startX, WaveRect.Bottom - 4 * PixMM, textAlign, textFont, textPaint);
                }
            }
        }

        /// <summary>
        /// 绘制导联名称和定标电压
        /// </summary>
        /// <param name="canvas">绘图对象</param>
        /// <param name="leadIdx">导联序号</param>
        /// <param name="startX">绘制导联名称的横坐标</param>
        /// <param name="baseY">绘制导联名称的纵坐标</param>
        private void DrawLeadNames(SKCanvas canvas, int leadIdx, float startX, float startY)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Green,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 9f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                // 获取导联名称
                string leadName = AnaValues.LeadIdxToNameMap.ContainsKey(leadIdx) ? AnaValues.LeadIdxToNameMap[leadIdx] : CommonValues.Langs["导联"] + $"{leadIdx + 1}";

                // 计算文本位置（左侧，导联波形中间）

                // 绘制导联名称
                canvas.DrawText(leadName, startX, startY, SKTextAlign.Left, textFont, textPaint);
            }
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawGrid(SKCanvas canvas)
        {
            using (SKPaint gridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#77ACE0"),
                StrokeWidth = 1.0f,
                IsAntialias = true
            })

            using (SKPaint majorGridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#C1D6EB"),
                StrokeWidth = 0.6f,
                IsAntialias = true
            })
            {
                float scale = Speed / 25f;
                // 根据ECG医疗标准，只绘制5mm的主要网格
                // 水平网格线（5mm间隔）
                for (float y = WaveRect.Top + TopRectHeight; y < WaveRect.Bottom; y += 5 * PixMM * scale)
                {
                    canvas.DrawLine(WaveRect.Left, y, WaveRect.Right, y, majorGridPaint);
                }

                // 垂直网格线（5mm间隔）
                for (float x = WaveRect.Left; x < WaveRect.Right; x += 5 * PixMM * scale)
                {
                    canvas.DrawLine(x, WaveRect.Top + TopRectHeight, x, WaveRect.Bottom, majorGridPaint);
                }

                if (bDrawSmallGrid)
                {
                    // 水平1mm网格线
                    for (float y = WaveRect.Top + TopRectHeight - 1; y < WaveRect.Bottom - 1; y += PixMM)
                    {
                        if (Math.Abs(y % (5 * PixMM)) > 0.1f) // 跳过5mm的位置
                        {
                            canvas.DrawLine(WaveRect.Left, y, WaveRect.Right, y, gridPaint);
                        }
                    }

                    // 垂直1mm网格线
                    for (float x = WaveRect.Left - 1; x < WaveRect.Right - 1; x += PixMM)
                    {
                        if (Math.Abs(x % (5 * PixMM)) > 0.1f) // 跳过5mm的位置
                        {
                            canvas.DrawLine(x, WaveRect.Top + TopRectHeight, x, WaveRect.Bottom, gridPaint);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 绘制定位中线
        /// </summary>
        /// <param name="canvas"></param>
        public void DrawMiddleLine(SKCanvas canvas)
        {
            using (SKPaint LinePaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1.0f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                float LineTopMargin = TopRectHeight;

                if (WaveType == RectangleWaveType.SingleHeartbeat)
                {
                    LineTopMargin = 0;
                }
                float startX = WaveRect.Left + WaveLeftMargin + (WaveRect.Width - WaveLeftMargin - WaveRightMargin) / 2;
                LinePaint.PathEffect = SKPathEffect.CreateDash(new float[] { 1, 4 }, 0);
                canvas.DrawLine(startX, WaveRect.Top + LineTopMargin, startX,
                              WaveRect.Bottom, LinePaint);
            }
        }

        private void DrawSubTemplateWaveformForLead(SKCanvas canvas, SKPath path, int lead, float startX, float baseY)
        {
            using (SKPaint wavePaint = new SKPaint
            {
                Color = SKColor.Parse(AnaValues.WaveformColorMap[subTemplateType]),
                StrokeWidth = 1.0f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                float[] leadData = EcgData[lead];

                bool firstPoint = true;
                int idx = 0;
                for (int i = 0; i < leadData.Length; i++)
                {
                    float x = (float)(startX + (idx * Speed * PixMM / AnaValues.holterConfig.SampleRate));
                    float y = (float)(baseY - (leadData[i] * Gain * PixMM));
                    if (firstPoint)
                    {
                        path.MoveTo(x, y);
                        firstPoint = false;
                    }
                    else
                    {
                        path.LineTo(x, y);
                    }
                    ++idx;
                }

                DrawWavePath(canvas, path, wavePaint);

            }

        }

        private void DrawWavePath(SKCanvas canvas, SKPath path, SKPaint wavePaint)
        {
            SKRect pathRect = path.Bounds;
            // 检查是否需要修正
            bool needsCorrection = pathRect.Top < WaveRect.Top ||
                                   pathRect.Bottom > WaveRect.Bottom;

            if (needsCorrection)
            {
                float offsetY = CalculateBestOffset(pathRect, WaveRect);
                path.Offset(0, offsetY);
            }

            // 保存当前画布状态
            canvas.Save();
            // 设置裁剪区域
            canvas.ClipRect(WaveRect);

            canvas.DrawPath(path, wavePaint);

            // 恢复画布状态
            canvas.Restore();
        }

        /// <summary>
        /// 计算最佳偏移量（尽可能居中）
        /// </summary>
        private float CalculateBestOffset(SKRect pathRect, SKRect WaveRect)
        {
            float pathHeight = pathRect.Height;
            float targetHeight = WaveRect.Height;

            // 如果路径高度小于目标高度，居中显示
            if (pathHeight <= targetHeight)
            {
                float pathCenterY = (pathRect.Top + pathRect.Bottom) / 2;
                float targetCenterY = (WaveRect.Top + WaveRect.Bottom) / 2;
                return targetCenterY - pathCenterY;
            }

            // 如果路径高度大于目标高度，优先保证顶部对齐
            if (pathRect.Top < WaveRect.Top)
                return WaveRect.Top - pathRect.Top;

            if (pathRect.Bottom > WaveRect.Bottom)
                return WaveRect.Bottom - pathRect.Bottom;

            return 0;
        }

        private void DrawWaveformForLead(SKCanvas canvas, SKPath path, int lead, float startX, float baseY)
        {
            int startIndex = StartSample;
            int endIndex = StartSample + PointCount;

            int currentIndex = startIndex;
            float prevSegmentEndX = 0, prevSegmentEndY = 0;
            bool hasPrevSegmentEnd = false;
            char prevBeatType = 'N'; // 保存前一个心搏的类型

            int heartbeatIndex = 0;
            while (currentIndex < endIndex)
            {
                BeatInfo matchingHeartbeat = BeatInfo.Empty;

                // 优化：使用索引直接查找，避免重复遍历
                while (heartbeatIndex < HeartbeatInfos.Count)
                {
                    BeatInfo heartbeat = HeartbeatInfos[heartbeatIndex];
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
                    DrawSegment(canvas, path, lead, startIndex, currentIndex, Math.Min(matchingHeartbeat.EndIndex, endIndex),
                               startX, baseY, currentBeatType, ref prevSegmentEndX, ref prevSegmentEndY, ref hasPrevSegmentEnd);
                    prevBeatType = currentBeatType; // 保存当前心搏类型
                    currentIndex = matchingHeartbeat.EndIndex;
                    heartbeatIndex++; // 移动到下一个心搏
                }
                else
                {
                    BeatInfo nextHeartbeat = BeatInfo.Empty;

                    // 优化：从当前位置开始查找下一个心搏
                    for (int i = heartbeatIndex; i < HeartbeatInfos.Count; i++)
                    {
                        BeatInfo heartbeat = HeartbeatInfos[i];
                        if (heartbeat.StartIndex > currentIndex && heartbeat.StartIndex < endIndex)
                        {
                            nextHeartbeat = heartbeat;
                            break;
                        }
                    }

                    if (nextHeartbeat.Sample != 0)
                    {
                        // 衔接处使用前一个心搏的类型
                        DrawSegment(canvas, path, lead, startIndex, currentIndex, nextHeartbeat.StartIndex,
                                   startX, baseY, prevBeatType, ref prevSegmentEndX, ref prevSegmentEndY, ref hasPrevSegmentEnd);
                        currentIndex = nextHeartbeat.StartIndex;
                        heartbeatIndex = HeartbeatInfos.IndexOf(nextHeartbeat);
                    }
                    else
                    {
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
                float[] leadData = EcgData[lead];

                // 设置路径起始点
                if (hasPrevSegmentEnd)
                {
                    path.MoveTo(prevSegmentEndX, prevSegmentEndY);
                }
                else
                {
                    int dataIndex = segmentStart - StartSample;
                    if (dataIndex >= 0 && dataIndex < leadData.Length)
                    {
                        path.MoveTo(startX + ((segmentStart - startIndex) * Speed * PixMM / AnaValues.holterConfig.SampleRate),
                                   baseY - (leadData[dataIndex] * Gain * PixMM));
                    }
                }

                // 高性能绘制循环：优化内存访问和计算
                int lastDrawnIndex = segmentStart;

                // 预计算循环边界
                int loopEnd = Math.Min(segmentEnd, startIndex + leadData.Length + StartSample);

                // 使用局部变量优化循环性能
                float currentX, currentY;
                int currentDataIndex;
                SKRect paintRect = new SKRect(startX, WaveRect.Top + WaveTopMargin + 1, WaveRect.Right, WaveRect.Bottom);
                for (int i = segmentStart; i < loopEnd; i++)
                {
                    currentDataIndex = i - StartSample;
                    if (currentDataIndex >= 0 && currentDataIndex < leadData.Length)
                    {
                        currentX = startX + ((i - startIndex) * Speed * PixMM / AnaValues.holterConfig.SampleRate);
                        currentY = baseY - (leadData[currentDataIndex] * Gain * PixMM);
                        if (currentY < WaveRect.Top + WaveTopMargin)
                        {
                            currentY = WaveRect.Top + WaveTopMargin;
                        }
                        if (currentY > WaveRect.Bottom - WaveBottomMargin)
                        {
                            currentY = WaveRect.Bottom - WaveBottomMargin;
                        }
                        if (paintRect.Contains(currentX, currentY))
                            path.LineTo(currentX, currentY);
                        else
                            path.MoveTo(currentX, currentY);

                        lastDrawnIndex = i;
                    }
                }

                // 确保绘制最后一个点
                int lastDataIndex = loopEnd - 1 - StartSample;
                if (lastDataIndex >= 0 && lastDataIndex < leadData.Length && lastDrawnIndex < loopEnd - 1)
                {
                    currentX = startX + ((loopEnd - 1 - startIndex) * Speed * PixMM / AnaValues.holterConfig.SampleRate);
                    currentY = baseY - (leadData[lastDataIndex] * Gain * PixMM);
                    if (currentY < WaveRect.Top + WaveTopMargin)
                    {
                        currentY = WaveRect.Top + WaveTopMargin;
                    }
                    if (currentY > WaveRect.Bottom - WaveBottomMargin)
                    {
                        currentY = WaveRect.Bottom - WaveBottomMargin;
                    }
                    if (paintRect.Contains(currentX, currentY))
                        path.LineTo(currentX, currentY);
                    else
                        path.MoveTo(currentX, currentY);

                    lastDrawnIndex = loopEnd - 1;
                }

                if (path.PointCount > 0)
                {
                    SKPaint paint = waveformPaints.ContainsKey(beatType) ? waveformPaints[beatType] : waveformPaints['N'];
                    canvas.DrawPath(path, paint);
                }

                // 保存段结束点
                if (lastDrawnIndex >= segmentStart)
                {
                    int endDataIndex = lastDrawnIndex - StartSample;
                    if (endDataIndex >= 0 && endDataIndex < leadData.Length)
                    {
                        prevSegmentEndX = startX + ((lastDrawnIndex - startIndex) * Speed * PixMM / AnaValues.holterConfig.SampleRate);
                        prevSegmentEndY = baseY - (leadData[endDataIndex] * Gain * PixMM);
                        if (prevSegmentEndY < WaveRect.Top + WaveTopMargin)
                        {
                            prevSegmentEndY = WaveRect.Top + WaveTopMargin;
                        }
                        if (prevSegmentEndY > WaveRect.Bottom - WaveBottomMargin)
                        {
                            prevSegmentEndY = WaveRect.Bottom - WaveBottomMargin;
                        }
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
    }
}
