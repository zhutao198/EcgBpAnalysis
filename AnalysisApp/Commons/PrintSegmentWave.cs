using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class PrintSegmentWave
    {
        public List<List<JObject>> SegmentAllPageInfo = new List<List<JObject>>();
        public int currentPageIndex = 0;

        public double PaperWidthMm = 210.0;      // A4纸宽度
        public double PaperHeightMm = 297.0;     // A4纸高度
        public double MarginLeftMm = 10.0;       // 左边距
        public double MarginRightMm = 15.0;      // 右边距
        public double MarginTopMm = 20.0;          //上边距
        public double MarginBottonMm = 17.0;       //下边距
        private const double GridSizeMm = 5.0;          // 5mm网格
        private const double MinorGridSizeMm = 1.0;     // 1mm小网格
                                                        //private const float PixMM = 3.7795f;             // 1mm 3.7795个像素点
        private const float PrintDPI = 300f; // 打印专用DPI
        private const float PixMM = PrintDPI / 25.4f; // 300 DPI = 11.81像素/mm

        // 字体大小缩放因子（300 DPI相对于96 DPI）
        private const float FontScaleFactor = PrintDPI / 96f; // 300/96 = 3.125倍

        // 坐标缩放工具方法
        private float ScaleX(double mm)
        {
            return (float)(mm * PixMM);
        }

        private float ScaleY(double mm)
        {
            return (float)(mm * PixMM);
        }

        private float Scale(double mm)
        {
            return (float)(mm * PixMM);
        }


        private bool bDrawSmallGrid = true;
        //波形的上边距
        public double WaveTopMargin { get; set; } = 5 * PixMM;
        private float WaveBottomMargin { get; set; } = 0;
        //波形的左边距
        public double WaveLeftMargin { get; set; } = 10 * PixMM;
        public double WaveRightMargin { get; set; } = 0;

        public double MinSpeed = 6.25;
        public double MinGain = 2.5;

        public double Speed = 25;
        public double Gain = 10.0;
        public int StartSample = 0;
        public List<int> LeadIdxs { get; set; } = new List<int>();                                //导联索引列表
        public List<WaveInfo> EcgData { get; set; } = new List<WaveInfo>();                                    //ECG波形数据
        private int PointCount { get; set; } = 0;
        public List<BeatInfo> HeartbeatInfos { get; set; } = new List<BeatInfo>();                            //心搏信息
        public string SegmentLabel = "";
        public string SegmentHR = "";
        private bool bPrintHR = true;
        private int TempSample = 0;
        private int Sample = 0;
        private string EventName = "";

        private readonly SKColor _gridColor = SKColor.Parse("#db634f");
        private readonly SKColor _minorGridColor = SKColor.Parse("#f3aca0");
        private readonly SKColor _waveColor = SKColor.Parse("#707070");
        private readonly SKColor _textColor = new SKColor(0, 0, 0);
        private readonly SKColor _backgroundColor = new SKColor(255, 255, 255);

        //总页数
        public int TotalPages;

        //每页中每个片段的网格区域
        public SKRect WaveRect { get; set; } = new SKRect();


        public PrintSegmentWave()
        {

        }

        public Bitmap RenderEcgPage(int pageNum, bool bHorizontalPrintLast, int reportPageIdx = 0)
        {
            Console.WriteLine("pageNum: " + pageNum + "--reportPageIdx: " + reportPageIdx);
            List<JObject> SegmentPageInfo = SegmentAllPageInfo[pageNum];
            int direction = Convert.ToInt32(SegmentPageInfo[0]["direction"]);

            bool bHorizontalPrint = direction == 0 ? true : false;
            float leadRowHeight = 0;
            if (bHorizontalPrint)
            {
                //横向
                PaperWidthMm = 297.0;
                PaperHeightMm = 210.0;
                MarginLeftMm = 15.0;       // 左边距
                MarginRightMm = 22.0;      // 右边距
                MarginTopMm = 25.0;          //上边距
                MarginBottonMm = 15.0;       //下边距
            }
            else
            {
                //纵向
                PaperWidthMm = 210.0;
                PaperHeightMm = 297.0;
                MarginLeftMm = 10.0;       // 左边距
                MarginRightMm = 15.0;      // 右边距
                MarginTopMm = 20.0;          //上边距
                MarginBottonMm = 17.0;       //下边距



            }

            double pageWidthPx = ScaleX(PaperWidthMm);
            double pageHeightPx = ScaleY(PaperHeightMm);

            float PageTopMargin = ScaleY(MarginTopMm);
            float PageBottomMargin = ScaleY(MarginBottonMm);

            float SegmentInfoHeight = ScaleY(10);
            float MinWaveHeight = ScaleY(10);
            float infoMinWaveHeight = 0;
            int sumLead = 0;
            for(int i=0; i < SegmentPageInfo.Count; i++)
            {
                var temp = JToken.FromObject(SegmentPageInfo[i]);
                List<int> leadIdxs = temp["leadIdx"].ToObject<List<int>>() ?? new List<int>();
                int leadCount = leadIdxs.Count;
                if(leadCount > 1)
                {
                    float tempH = SegmentInfoHeight + MinWaveHeight;
                    infoMinWaveHeight += tempH;
                }
                else
                {
                    infoMinWaveHeight += SegmentInfoHeight;
                }

                sumLead += leadCount;
            }
            sumLead = sumLead < 6 ? 10 : sumLead;
            leadRowHeight = (float)(pageHeightPx - PageTopMargin - PageBottomMargin - infoMinWaveHeight) / sumLead;

            // 创建SKSurface用于绘制
            using (var surface = SKSurface.Create(new SKImageInfo((int)pageWidthPx, (int)pageHeightPx)))
            {
                var canvas = surface.Canvas;
                // 清除画布为白色背景
                canvas.Clear(SKColors.White);
                DrawPageHeader(canvas, reportPageIdx != 0 ? reportPageIdx : pageNum);
                int leadSum = 0;
                float rowHeightSum = 0;
                for (int i = 0; i < SegmentPageInfo.Count; i++)
                {
                    var partInfo = JToken.FromObject(SegmentPageInfo[i]);
                    List<int> leadIdxs = partInfo["leadIdx"].ToObject<List<int>>() ?? new List<int>();
                    int leadCount = leadIdxs.Count;
                    int startSample = partInfo["startSample"]?.Value<int>() ?? 0;
                    StartSample = startSample;
                    if (partInfo["direction"]?.Value<int>() == 0)
                    {
                        StartSample = (startSample - Convert.ToInt32(1.5 * AnaValues.holterConfig.SampleRate)) > 0 ? startSample - Convert.ToInt32(1.5 * AnaValues.holterConfig.SampleRate) : startSample;
                    }
                    int rowSeconds = partInfo["rowSeconds"]?.Value<int>() ?? 0;
                    List<WaveInfo> ecgData = AnaCode.ReadAllWaveData(leadIdxs, StartSample, StartSample + rowSeconds * AnaValues.holterConfig.SampleRate - 1);

                    if (ecgData == null || ecgData.Count == 0)
                    {
                        continue;
                    }

                    if (leadCount > 1)
                    {
                        MinWaveHeight = ScaleY(10);
                    }
                    else
                    {
                        MinWaveHeight = 0;
                    }

                    rowHeightSum = leadSum * leadRowHeight + i * (SegmentInfoHeight + MinWaveHeight);

                    float left = ScaleX(MarginLeftMm);

                    //float top = ScaleY(MarginTopMm) + i * (SegmentInfoHeight + MinWaveHeight) + leadSum * leadRowHeight;
                    float top = ScaleY(MarginTopMm) + rowHeightSum;
                    float right = (float)(pageWidthPx - ScaleX(MarginRightMm));
                    float bottom = top + leadCount * leadRowHeight;
                    SKRect WaveRect = new SKRect(left, top, right, bottom);
                    this.WaveRect = WaveRect;
                    //DrawGridEx(canvas, 10);

                    DrawGrid(canvas);

                    Gain = partInfo["gain"]?.Value<double>() ?? 0.0;
                    EcgData = ecgData;
                    LeadIdxs = leadIdxs;

                    SegmentLabel = partInfo["segmentLabel"]?.Value<string>() ?? "";
                    Sample = partInfo["sample"]?.Value<int>() ?? 0;
                    EventName = partInfo["eventName"]?.Value<string>() ?? "";
                    bPrintHR = Sample != TempSample ? true : false;
                    DrawEcgWave(canvas);
                    if(leadCount > 1)
                    {
                        DrawSmallEcgWave(canvas, MinWaveHeight);
                    }

                    leadSum += leadCount;
                    TempSample = Sample;
                }

                // 保存为PNG文件
                //string outputPath = $"{ reportPageIdx} + black_curve.png";
                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (MemoryStream stream = new MemoryStream(data.ToArray()))
                //using (var streamdata = File.OpenWrite(outputPath))
                {
                    //data.SaveTo(streamdata);
                    Bitmap originalBitmap = new Bitmap(stream);

                    // 如果要求横向，将Bitmap顺时针旋转90度
                    if (bHorizontalPrint != bHorizontalPrintLast)
                    {
                        return RotateBitmap90Degrees(originalBitmap);
                    }
                    else
                    {
                        return originalBitmap;
                    }
                }
            }
        }
        /// <summary>
        /// 将Bitmap顺时针旋转90度
        /// </summary>
        private Bitmap RotateBitmap90Degrees(Bitmap original)
        {
            Bitmap rotated = new Bitmap(original.Height, original.Width);

            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // 顺时针旋转90度：将原点移动到右上角，然后旋转
                g.TranslateTransform(rotated.Width, 0);
                g.RotateTransform(90);

                g.DrawImage(original, 0, 0, original.Width, original.Height);
            }

            original.Dispose();
            return rotated;
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawGridEx(SKCanvas canvas, float TopRectHeight)
        {
            using (SKPaint gridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#9FC0E0"),
                StrokeWidth = 0.3f,
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
                // 根据ECG医疗标准，只绘制5mm的主要网格
                // 水平网格线（5mm间隔）
                for (float y = WaveRect.Top + TopRectHeight; y < WaveRect.Bottom; y += 5 * PixMM)
                {
                    canvas.DrawLine(WaveRect.Left, y, WaveRect.Right, y, majorGridPaint);
                }

                // 垂直网格线（5mm间隔）
                for (float x = WaveRect.Left; x < WaveRect.Right; x += 5 * PixMM)
                {
                    canvas.DrawLine(x, WaveRect.Top + TopRectHeight, x, WaveRect.Bottom, majorGridPaint);
                }

                if (bDrawSmallGrid)
                {
                    // 水平1mm网格线
                    for (float y = WaveRect.Top + TopRectHeight; y < WaveRect.Bottom; y += PixMM)
                    {
                        if (Math.Abs(y % (5 * PixMM)) > 0.1f) // 跳过5mm的位置
                        {
                            canvas.DrawLine(WaveRect.Left, y, WaveRect.Right, y, gridPaint);
                        }
                    }

                    // 垂直1mm网格线
                    for (float x = WaveRect.Left; x < WaveRect.Right; x += PixMM)
                    {
                        if (Math.Abs(x % (5 * PixMM)) > 0.1f) // 跳过5mm的位置
                        {
                            canvas.DrawLine(x, WaveRect.Top + TopRectHeight, x, WaveRect.Bottom, gridPaint);
                        }
                    }
                }


                //绘制边框
                canvas.DrawLine(WaveRect.Left, WaveRect.Top + TopRectHeight,
                    WaveRect.Left, WaveRect.Bottom, gridPaint);
                canvas.DrawLine(WaveRect.Left, WaveRect.Top + TopRectHeight,
                    WaveRect.Right, WaveRect.Top + TopRectHeight, gridPaint);
                canvas.DrawLine(WaveRect.Right, WaveRect.Top + TopRectHeight,
                    WaveRect.Right, WaveRect.Bottom, gridPaint);
                canvas.DrawLine(WaveRect.Left, WaveRect.Bottom,
                    WaveRect.Right, WaveRect.Bottom, gridPaint);
            }

        }

        public void DrawGrid(SKCanvas canvas)
        {
            using (SKPaint borderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = _gridColor,
                StrokeWidth = 0.7f * FontScaleFactor,
                IsAntialias = true
            })

            using (SKPaint gridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = _gridColor,
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true
            })

            using (SKPaint minorGridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = _minorGridColor,
                StrokeWidth = 0.8f * FontScaleFactor,
                IsAntialias = true
            })
            {
                double gridSizePx = 5 * PixMM;
                double minorGridSizePx = PixMM;

                int minorXIdx = 0;

                // 绘制小网格（使用点绘制）
                for (double x = WaveRect.Left + 1 * PixMM; x < WaveRect.Right; x += minorGridSizePx)
                {
                    int minorYIdx = 0;
                    ++minorXIdx;
                    if (minorXIdx % 5 == 0)
                    {
                        continue;
                    }
                    for (double y = WaveRect.Top + 1 * PixMM; y < WaveRect.Bottom; y += minorGridSizePx)
                    {
                        ++minorYIdx;
                        if (minorYIdx % 5 == 0)
                        {
                            continue;
                        }
                        canvas.DrawPoint((float)x, (float)y, minorGridPaint);
                    }
                }

                // 绘制大网格
                for (double x = WaveRect.Left + 5 * PixMM; x < WaveRect.Right; x += gridSizePx)
                {
                    gridPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 1, 1 }, 0);
                    canvas.DrawLine((float)x, WaveRect.Top, (float)x,
                                  WaveRect.Bottom, gridPaint);
                }

                for (double y = WaveRect.Top + 5 * PixMM; y < WaveRect.Bottom; y += gridSizePx)
                {
                    canvas.DrawLine(WaveRect.Left, (float)y,
                                  WaveRect.Right, (float)y, gridPaint);
                }

                //绘制边框
                canvas.DrawLine(WaveRect.Left, WaveRect.Top, WaveRect.Left, WaveRect.Bottom, borderPaint);
                canvas.DrawLine(WaveRect.Left, WaveRect.Top, WaveRect.Right, WaveRect.Top, borderPaint);
                canvas.DrawLine(WaveRect.Right, WaveRect.Top, WaveRect.Right, WaveRect.Bottom, borderPaint);
                canvas.DrawLine(WaveRect.Left, WaveRect.Bottom, WaveRect.Right, WaveRect.Bottom, borderPaint);
            }

        }

        public void DrawEcgWave(SKCanvas canvas)
        {
            if (EcgData.Count == 0)
            {
                return;
            }
            for (int i = 0; i < EcgData.Count; i++)
            {
                if (EcgData[i].DataLength > 0)
                {
                    PointCount = EcgData[i].DataLength;
                    break;
                }
            }

            float rowHeight = (float)(WaveRect.Height - WaveTopMargin - WaveBottomMargin);
            float rowWidth = (float)(WaveRect.Width - WaveLeftMargin - WaveRightMargin);

            getHeartBeatInfos();
            DrawTopInfo(canvas);
            DrawLeadNames(canvas);
            DrawCalibration(canvas);
            using (SKPath path = new SKPath())
            {
                float leadSpacing = rowHeight / LeadIdxs.Count;
                float baseYOffset = (float)(leadSpacing / 2 + WaveRect.Top + WaveTopMargin);
                float startX = (float)(WaveRect.Left + WaveLeftMargin);

                for (int lead = 0; lead < LeadIdxs.Count; lead++)
                {
                    float yOffset = baseYOffset + (leadSpacing * lead);
                    float baseY = yOffset;

                    WaveInfo leadData = EcgData[LeadIdxs[lead]];
                    DrawWaveformForLead(canvas, leadData.WaveDataF, startX, baseY, Speed, Gain);
                }
            }
        }

        public void DrawSmallEcgWave(SKCanvas canvas, float waveHeight)
        {
            float rowWidth = (float)(WaveRect.Width - WaveLeftMargin - WaveRightMargin);
            int points = Convert.ToInt32(AnaValues.holterConfig.SampleRate * rowWidth / (MinSpeed * PixMM));
            int segmentRowPoints = EcgData[0].DataLength;

            using (SKPaint rectPaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(255, 0, 0, 20),
                Style = SKPaintStyle.Fill,
            })
            {
                double segmentMinWidth = segmentRowPoints * MinSpeed * PixMM / AnaValues.holterConfig.SampleRate;
                SKRect rect = new SKRect(WaveRect.Left + (WaveRect.Width - (float)segmentMinWidth) / 2, WaveRect.Bottom - 1, WaveRect.Right - (WaveRect.Width - (float)segmentMinWidth) / 2, WaveRect.Bottom + waveHeight - 1);
                canvas.DrawRect(rect, rectPaint);
            }

            List<int> leadIdxs = new List<int>();
            leadIdxs.Add(AnaValues.holterConfig.MainChannel);
            int startSample = StartSample - (points - segmentRowPoints) / 2;
            startSample = Math.Max(startSample, 0);
            int endSample = startSample + points;
            endSample = Math.Min(endSample, AnaValues.holterConfig.Total - 1);
            List<WaveInfo> ecgData = AnaCode.ReadAllWaveData(leadIdxs, startSample, endSample);

            float startX = (float)(WaveRect.Left + WaveLeftMargin);
            float baseY = WaveRect.Bottom + waveHeight / 2;

            for (int lead = 0; lead < leadIdxs.Count; lead++)
            {
                WaveInfo leadData = ecgData[leadIdxs[lead]];
                DrawWaveformForLead(canvas, leadData.WaveDataF, startX, baseY, MinSpeed, MinGain);
            }


            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.4f * FontScaleFactor,
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                string leadName = AnaValues.LeadIdxToNameMap.ContainsKey(leadIdxs[0]) ? AnaValues.LeadIdxToNameMap[leadIdxs[0]] : CommonValues.Langs["导联"] + "1";
                canvas.DrawText(leadName, (float)ScaleX(MarginLeftMm+4), (float)baseY, SKTextAlign.Center, textFont, textPaint);

                //绘制边框
                canvas.DrawLine(WaveRect.Left, WaveRect.Bottom, WaveRect.Left, WaveRect.Bottom + waveHeight, textPaint);
                canvas.DrawLine(WaveRect.Right, WaveRect.Bottom, WaveRect.Right, WaveRect.Bottom + waveHeight, textPaint);
                canvas.DrawLine(WaveRect.Left, WaveRect.Bottom + waveHeight, WaveRect.Right, WaveRect.Bottom + waveHeight, textPaint);
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

        private void DrawTopInfo(SKCanvas canvas)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.2f * FontScaleFactor,
            })
            using (SKFont textFont = new SKFont
            {
                Size = 12f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont beatInfoFont = new SKFont
            {
                Size = 10f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                // 绘制纸速信息
                string speedText = $"{Speed}mm/s {Gain}mm/mV";
                canvas.DrawText(speedText, (float)WaveRect.Right, (float)WaveRect.Top - ScaleY(1), SKTextAlign.Right, textFont, textPaint);
                //起始时间
                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

                string dateText = AnaCode.getDataTimeFromSample(StartSample).ToString(dateFormat);
                canvas.DrawText(dateText, (float)WaveRect.Left,
                (float)WaveRect.Top - ScaleY(1), SKTextAlign.Left, textFont, textPaint);
                //绘制标签
                canvas.DrawText(SegmentLabel, ScaleX(PaperWidthMm / 2), (float)WaveRect.Top - ScaleY(4f), SKTextAlign.Center, textFont, textPaint);


                Dictionary<char, SKPaint> beatHeartInfoPaints = new Dictionary<char, SKPaint>();

                foreach (var item in AnaValues.WaveformColorMap)
                {
                    beatHeartInfoPaints[item.Key] = new SKPaint
                    {
                        Color = SKColor.Parse(item.Value),
                        IsAntialias = true
                    };
                }

                float leftMargin = (float)WaveLeftMargin;
                float textY = (float)(WaveRect.Top + WaveTopMargin / 2);

                //float textY = (float)(WaveRect.Top + WaveTopMargin / 2 + ScaleY(3));
                float startX = WaveRect.Left + leftMargin;

                int RRCount = 0;
                int RRSum = 0;
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
                        float sampleX = (float)(startX + ((currentBeat.Sample - StartSample) * Speed * PixMM / AnaValues.holterConfig.SampleRate));
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
                        canvas.DrawText(AnaValues.heartbeatTypeMap[beatType.ToString()], sampleX, textY, SKTextAlign.Center, beatInfoFont, beatHeartInfoPaints[beatType]);
                        if (beatType == 'X')
                        {
                            continue;
                        }
                        if (currentBeat.RRInterval < 0)
                        {
                            continue;
                        }
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

                        // 计算心率
                        int hr = (int)(60000F / currentBeat.RRInterval);

                        // 绘制RR间期和HR值（不带单位）
                        string rrStr = $"{currentBeat.RRInterval}";
                        string hrStr = $"{hr}";

                        float textWrr = textFont.MeasureText(rrStr, out SKRect bound1, textPaint);
                        float textWhr = textFont.MeasureText(hrStr, out SKRect bound2, textPaint);
                        float textW = Math.Max(textWrr, textWhr);

                        float middleX = (float)(startX + (currentBeat.Sample - StartSample) * Speed * PixMM / AnaValues.holterConfig.SampleRate - currentBeat.RRInterval / 1000f / 2 * Speed * PixMM);

                        if (middleX < startX + textW || middleX > WaveRect.Right)
                        {
                            continue;
                        }

                        // 绘制RR间期（上方）
                        canvas.DrawText(rrStr, middleX, textY, SKTextAlign.Center, beatInfoFont, textPaint);
                        // 绘制HR值（下方）
                        canvas.DrawText(hrStr, middleX, textY + ScaleY(3), SKTextAlign.Center, beatInfoFont, textPaint);
                    }

                    for (int i = 0; i < HeartbeatInfos.Count; i++)
                    {
                        if (HeartbeatInfos[i].BeatUiType == 'X')
                        {
                            continue;
                        }
                        ++RRCount;
                        RRSum += HeartbeatInfos[i].RRInterval;
                    }

                    if (bPrintHR)
                    {
                        string hrText = "";

                        if (EventName != "")
                        {
                            int avgHR = OperateSegment.GetFastestLowestHR(EventName, Sample);
                            if (avgHR != -1)
                            {
                                hrText = CommonValues.Langs["心率"] + $": {avgHR}" + CommonValues.Langs["次/分"];
                            }
                        }
                        else
                        {
                            int RRAvg = RRSum / RRCount;
                            int heartbeatAvg = 60 * 1000 / RRAvg;
                            //平均心率
                            hrText = CommonValues.Langs["心率"] + $": {heartbeatAvg}" + CommonValues.Langs["次/分"];
                        }

                        canvas.DrawText(hrText, ScaleX(PaperWidthMm / 2), (float)WaveRect.Top - ScaleY(1), SKTextAlign.Center, textFont, textPaint);

                    }
                }
            }
        }

        private void DrawWaveformForLead(SKCanvas canvas, float[] leadData, double xOffset, double yOffset, double speed, double gain)
        {
            using (SKPath path = new SKPath())
            using (SKPaint wavePaint = new SKPaint
            {
                Color = _waveColor,
                StrokeWidth = 1.0f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                bool firstPoint = true;
                int idx = 0;
                for (int i = 0; i < leadData.Length; i++)
                {
                    float x = (float)(xOffset + (idx * speed * PixMM / AnaValues.holterConfig.SampleRate));
                    float y = (float)(yOffset - (leadData[i] * gain * PixMM));
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
                canvas.DrawPath(path, wavePaint);
            }
        }
        /// <summary>
        /// 绘制导联名称和定标电压
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawLeadNames(SKCanvas canvas)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.2f * FontScaleFactor,
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                // 计算每行的高度
                float rowHeight = (float)(WaveRect.Height - WaveTopMargin - WaveBottomMargin);
                float rowWidth = (float)(WaveRect.Width - WaveLeftMargin - WaveRightMargin);
                double baseX = ScaleX(MarginLeftMm + 4);


                float leadSpacing = rowHeight / LeadIdxs.Count;
                float baseYOffset = (float)(leadSpacing / 2 + WaveRect.Top + WaveTopMargin + ScaleY(3));

                for (int leadIdx = 0; leadIdx < LeadIdxs.Count; leadIdx++)
                {
                    double yOffset = baseYOffset + (leadSpacing * leadIdx);
                    double baseY = yOffset;

                    // 获取导联名称
                    string leadName = AnaValues.LeadIdxToNameMap.ContainsKey(LeadIdxs[leadIdx]) ? AnaValues.LeadIdxToNameMap[LeadIdxs[leadIdx]] : CommonValues.Langs["导联"] + $"{leadIdx + 1}";

                    // 计算文本位置（左侧，导联波形中间）

                    // 绘制导联名称
                    canvas.DrawText(leadName, (float)baseX, (float)yOffset, SKTextAlign.Center, textFont, textPaint);

                }
            }


        }

        /// <summary>
        /// 绘制定标电压
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawCalibration(SKCanvas canvas)
        {
            // 计算定标电压（1mV对应的像素高度）
            float calibrationHeight = (float)Gain * PixMM; // 1mV = gain * 每毫米像素数

            // 计算每行的高度
            float rowHeight = (float)(WaveRect.Height - WaveTopMargin - WaveBottomMargin);
            float rowWidth = (float)(WaveRect.Width - WaveLeftMargin - WaveRightMargin);
            float calibrationX = (float)(MarginLeftMm + 2) * PixMM; // 定标标记的水平位置


            float leadSpacing = rowHeight / LeadIdxs.Count;
            float baseYOffset = (float)(leadSpacing / 2 + WaveRect.Top + WaveTopMargin);
            for (int leadIndex = 0; leadIndex < LeadIdxs.Count; leadIndex++)
            {
                if (leadIndex >= LeadIdxs.Count)
                    continue;

                //int leadIndex = leadIdxs[lead];
                float yOffset = (float)(baseYOffset + (leadSpacing * leadIndex));

                // 可选：绘制定标电压图形标记（1mV的垂直线段）

                float calibrationStartY = yOffset - calibrationHeight / 2;
                float calibrationEndY = yOffset + calibrationHeight / 2;

                using (SKPaint calibrationPaint = new SKPaint
                {
                    Color = _textColor,
                    StrokeWidth = 1.0f * FontScaleFactor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                })
                {
                    canvas.DrawLine(calibrationX - 1 * PixMM, calibrationEndY, calibrationX, calibrationEndY, calibrationPaint);

                    canvas.DrawLine(calibrationX, calibrationStartY, calibrationX, calibrationEndY, calibrationPaint);

                    canvas.DrawLine(calibrationX, calibrationStartY, calibrationX + 5 * PixMM, calibrationStartY, calibrationPaint);

                    canvas.DrawLine(calibrationX + 5 * PixMM, calibrationStartY, calibrationX + 5 * PixMM, calibrationEndY, calibrationPaint);

                    canvas.DrawLine(calibrationX + 5 * PixMM, calibrationEndY, calibrationX + 6 * PixMM, calibrationEndY, calibrationPaint);
                }

            }
        }
        public void DrawPageHeader(SKCanvas canvas, int pageNumber)
        {
            using (SKFont textFont = new SKFont
            {
                Size = 12.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont copyRightFont = new SKFont
            {
                Size = 9.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.4f * FontScaleFactor,
            })
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.6f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

                //患者姓名
                string nameText = CommonValues.Langs["姓名"] + $": {AnaValues.CurrentCase.name}";
                canvas.DrawText(nameText, ScaleX(MarginLeftMm),
                ScaleY(MarginTopMm - 8), SKTextAlign.Left, textFont, textPaint);
                string recordTimeText = CommonValues.Langs["记录时间"] + $": {AnaValues.CurrentCase.recordTime?.ToString(dateFormat)}";
                canvas.DrawText(recordTimeText, ScaleX(PaperWidthMm - MarginRightMm),
                ScaleY(MarginTopMm - 8), SKTextAlign.Right, textFont, textPaint);
                canvas.DrawLine(ScaleX(MarginLeftMm), ScaleY(MarginTopMm - 7), ScaleX(PaperWidthMm - MarginRightMm), ScaleY(MarginTopMm - 7), linePaint);
                //string pageFoot = CommonValues.SystemSettings.reportSets.pageFoot;
                //if (pageFoot != "")
                //{
                //    canvas.DrawText(pageFoot, ScaleX(MarginLeftMm), ScaleY(PaperHeightMm - MarginBottonMm), SKTextAlign.Left, copyRightFont, textPaint);
                //}
                //else
                //{
                //    string copyrightStr = CommonValues.Langs["*此报告仅供临床医生参考"];
                //    canvas.DrawText(copyrightStr, ScaleX(MarginLeftMm), ScaleY(PaperHeightMm - MarginBottonMm), SKTextAlign.Left, copyRightFont, textPaint);
                //}

                if (CommonValues.SystemSettings.reportSets.displayPageNum == 1)
                {
                    // 绘制页码
                    int pageNum = pageNumber + 1;
                    string footerText = $"{pageNum}/{TotalPages}";

                    canvas.DrawText(footerText, ScaleX(PaperWidthMm - MarginRightMm), ScaleY(PaperHeightMm - MarginBottonMm), SKTextAlign.Right, textFont, textPaint);
                }
            }
        }
    }
}
