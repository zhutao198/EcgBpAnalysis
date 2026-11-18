using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class PrintEcgWaveRenderer
    {
        public double PaperWidthMm = 210.0;      // A4纸宽度
        public double PaperHeightMm = 297.0;     // A4纸高度
        public double MarginLeftMm = 15.0;       // 左边距
        public double MarginRightMm = 15.0;      // 右边距
        public double MarginTopMm = 35.0;          //上边距
        public double MarginBottonMm = 17.0;       //下边距
        private const double GridSizeMm = 5.0;          // 5mm网格
        private const double MinorGridSizeMm = 1.0;     // 1mm小网格
        private const float pixMM = 3.7795f;             // 1mm 3.7795个像素点

        private readonly SKColor _gridColor = SKColor.Parse("#db634f");
        private readonly SKColor _minorGridColor = SKColor.Parse("#f3aca0");
        private readonly SKColor _waveColor = new SKColor(0, 0, 0);
        private readonly SKColor _textColor = new SKColor(0, 0, 0);
        private readonly SKColor _backgroundColor = new SKColor(255, 255, 255);

        public List<WaveInfo> ecgData = new List<WaveInfo>();
        public List<BeatInfo> heartbeatInfos = new List<BeatInfo>();
        //总页数
        public int totalPages;
        //每页每导联绘制的行数
        public int currentRows = 1;
        //每行点的个数
        public int RowPoints;

        public double speed = 25;
        public double gain = 10;
        public int startSample = 0;
        //标签
        public string label = "";
        //是否横向打印
        public bool bHorizontalPrint = false;
        public List<int> leadIdxs = new List<int>();
        //波形的上边距
        public double waveTopMargin = (20 + 5) * pixMM;
        //波形的左边距
        public double waveLeftMargin = (10 + 10) * pixMM;
        //打印总采样点数
        public int totalPrintPoints = 0;
        public PrintEcgWaveRenderer()
        {

        }

        public List<Bitmap> RenderEcgPages(List<List<float>> ecgData,
                                          int totalPages, 
                                          List<int> leadIdxs, double speed, double gain)
        {
            List<Bitmap> pages = new List<Bitmap>();

            for (int pageNum = 0; pageNum < totalPages; pageNum++)
            {
                Bitmap pageBitmap = RenderEcgPage(pageNum);
                pages.Add(pageBitmap);
            }

            return pages;
        }

        public Bitmap RenderEcgPage(int pageNum)
        {
            if (bHorizontalPrint)
            {
                //横向
                PaperWidthMm = 297.0;
                PaperHeightMm = 210.0;
                MarginLeftMm = 15.0;       // 左边距
                MarginRightMm = 22.0;      // 右边距
                MarginTopMm = 20.0;          //上边距
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

            waveTopMargin = (MarginTopMm+5) * pixMM;
            waveLeftMargin = (MarginLeftMm + 10) * pixMM;

            double pageWidthPx = PaperWidthMm * pixMM;
            double pageHeightPx = PaperHeightMm * pixMM;

            // 创建SKSurface用于绘制
            using (var surface = SKSurface.Create(new SKImageInfo((int)pageWidthPx, (int)pageHeightPx)))
            {
                var canvas = surface.Canvas;

                // 清除画布为白色背景
                canvas.Clear(SKColors.White);
                DrawPageHeader(canvas, pageNum, startSample);
                DrawBeatHeartInfo(canvas, pageNum, startSample, pageWidthPx, pageHeightPx);
                DrawGrid(canvas, pageWidthPx, pageHeightPx);
                DrawEcgWaveforms(canvas, pageNum, startSample,
                               pageWidthPx, pageHeightPx);
                DrawLeadNames(canvas, pageWidthPx, pageHeightPx);
                DrawCalibration(canvas, pageWidthPx, pageHeightPx);
                // 保存为PNG文件
                //string outputPath = $"{ pageNum} + black_curve.png";
                using (SKImage image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (MemoryStream stream = new MemoryStream(data.ToArray()))
                //using (var streamdata = File.OpenWrite(outputPath))
                {
                    //data.SaveTo(streamdata);
                    Bitmap originalBitmap = new Bitmap(stream);

                    // 如果要求横向，将Bitmap顺时针旋转90度
                    //if (bHorizontalPrint)
                    //{
                    //    return RotateBitmap90Degrees(originalBitmap);
                    //}
                    //else
                    //{
                    return originalBitmap;
                    //}
                    //return new Bitmap(stream);
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
        public void DrawGrid(SKCanvas canvas, double widthPx, double heightPx)
        {
            using (SKPaint borderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = _gridColor,
                StrokeWidth = 0.6f,
                IsAntialias = true
            })

            using (SKPaint gridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = _gridColor,
                StrokeWidth = 0.3f,
                IsAntialias = true
            })

            using (SKPaint minorGridPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = _minorGridColor,
                StrokeWidth = 0.8f,
                IsAntialias = true
            })
            {
                double gridSizePx = 5 * pixMM;
                double minorGridSizePx = pixMM;

                // 绘制小网格（使用点绘制）
                for (double x = (MarginLeftMm+1) * pixMM; x < widthPx - MarginRightMm * pixMM; x += minorGridSizePx)
                {
                    for (double y = (MarginTopMm+1) * pixMM; y < heightPx - MarginBottonMm * pixMM; y += minorGridSizePx)
                    {
                        canvas.DrawPoint((float)x, (float)y, minorGridPaint);
                    }
                }

                // 绘制大网格
                for (double x = (MarginLeftMm+5) * pixMM; x <= widthPx - (MarginRightMm+5) * pixMM; x += gridSizePx)
                {
                    gridPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 1, 1 }, 0);
                    canvas.DrawLine((float)x, (float)MarginTopMm * pixMM, (float)x,
                                  (float)(heightPx - MarginBottonMm * pixMM), gridPaint);
                }

                for (double y = (MarginTopMm+5) * pixMM; y <= heightPx - (MarginBottonMm+5) * pixMM; y += gridSizePx)
                {
                    canvas.DrawLine((float)MarginLeftMm * pixMM, (float)y,
                                  (float)(widthPx - MarginRightMm * pixMM), (float)y, gridPaint);
                }

                //绘制边框
                canvas.DrawLine((float)MarginLeftMm * pixMM, (float)MarginTopMm * pixMM,
                    (float)MarginLeftMm * pixMM, (float)(heightPx - MarginBottonMm * pixMM), borderPaint);
                canvas.DrawLine((float)MarginLeftMm * pixMM, (float)MarginTopMm * pixMM,
                    (float)(widthPx - MarginRightMm * pixMM), (float)((float)MarginTopMm * pixMM), borderPaint);
                canvas.DrawLine((float)(widthPx - MarginRightMm * pixMM), (float)MarginTopMm * pixMM,
                    (float)(widthPx - MarginRightMm * pixMM), (float)(heightPx - MarginBottonMm * pixMM), borderPaint);
                canvas.DrawLine((float)MarginLeftMm * pixMM, (float)(heightPx - MarginBottonMm * pixMM),
                    (float)(widthPx - MarginRightMm * pixMM), (float)(heightPx - MarginBottonMm * pixMM), borderPaint);
            }

        }

        public void DrawEcgWaveforms(SKCanvas canvas, int pageNumber,
                                    int startSample, double widthPx, double heightPx)
        {
            double waveformWidth = widthPx - (MarginLeftMm + MarginRightMm) * pixMM;
            double waveformHeight = heightPx - MarginBottonMm * pixMM - waveTopMargin;
            double rowHeight = waveformHeight / currentRows;
            int pageStartSample = startSample + pageNumber * RowPoints * currentRows;
            for (int row = 0; row < currentRows; row++)
            {
                int startIndex = (row * RowPoints) + pageStartSample;
                int endIndex = ((row + 1) * RowPoints) + pageStartSample;

                if (startIndex >= endIndex)
                {
                    continue;
                }

                double leadSpacing = rowHeight / leadIdxs.Count;
                double baseYOffset = (rowHeight * row) + leadSpacing / 2 + waveTopMargin;
                double startX = waveLeftMargin;

                for (int i = 0; i < leadIdxs.Count; i++)
                {
                    double yOffset = baseYOffset + (leadSpacing * i);
                    double baseY = yOffset;

                    WaveInfo leadData = ecgData[leadIdxs[i]];
                    DrawLeadWaveform(canvas, pageNumber, row, leadData.WaveDataF, startIndex, startX, baseY, waveformWidth, waveformHeight);
                }
            }
        }

        private void DrawLeadWaveform(SKCanvas canvas, int pageNumber, int row, float[] leadData,
                                    int startSample, double xOffset, double yOffset,
                                    double width, double height)
        {
            using (SKPath path = new SKPath())
            using (SKPaint wavePaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1.0f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                bool firstPoint = true;
                int idx = 0;

                int dataStartIdx = pageNumber * RowPoints * currentRows + RowPoints * row;
                int dataEndIdx = dataStartIdx + RowPoints;
                for (int i = dataStartIdx; i < dataEndIdx; i++)
                {
                    float x = (float)(xOffset + (idx * speed * pixMM / AnaValues.holterConfig.SampleRate));
                    float y = (float)(yOffset - (leadData[i] * gain * pixMM));
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
        public void DrawLeadNames(SKCanvas canvas, double widthPx, double heightPx)
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
                // 计算每行的高度
                double waveformWidth = widthPx - (MarginLeftMm + MarginRightMm) * pixMM;
                double waveformHeight = heightPx - MarginBottonMm * pixMM - waveTopMargin;
                double rowHeight = waveformHeight / currentRows;
                double baseX = (MarginLeftMm + 4) * pixMM;

                for (int row = 0; row < currentRows; row++)
                {
                    double leadSpacing = rowHeight / leadIdxs.Count;
                    double baseYOffset = (rowHeight * row) + leadSpacing / 2 + waveTopMargin;

                    for (int leadIdx = 0; leadIdx < leadIdxs.Count; leadIdx++)
                    {
                        double yOffset = baseYOffset + (leadSpacing * leadIdx);
                        double baseY = yOffset;

                        // 获取导联名称
                        string leadName = AnaValues.LeadIdxToNameMap.ContainsKey(leadIdxs[leadIdx]) ? AnaValues.LeadIdxToNameMap[leadIdxs[leadIdx]] : $"导联{leadIdx + 1}";

                        // 计算文本位置（左侧，导联波形中间）

                        // 绘制导联名称
                        canvas.DrawText(leadName, (float)baseX, (float)yOffset, SKTextAlign.Center, textFont, textPaint);

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
        public void DrawCalibration(SKCanvas canvas, double widthPx, double heightPx)
        {
            // 计算定标电压（1mV对应的像素高度）
            float calibrationHeight = (float)gain * pixMM; // 1mV = gain * 每毫米像素数

            // 计算每行的高度
            double waveformWidth = widthPx - (MarginLeftMm + MarginRightMm) * pixMM;
            double waveformHeight = heightPx - MarginBottonMm * pixMM - waveTopMargin;
            double rowHeight = waveformHeight / currentRows;
            float calibrationX = (float)(MarginLeftMm + 2) * pixMM; // 定标标记的水平位置

            for (int row = 0; row < currentRows; row++)
            {
                double leadSpacing = rowHeight / leadIdxs.Count;
                double baseYOffset = (rowHeight * row) + leadSpacing / 2 + waveTopMargin;
                for (int leadIndex = 0; leadIndex < leadIdxs.Count; leadIndex++)
                {
                    if (leadIndex >= leadIdxs.Count)
                        continue;

                    //int leadIndex = leadIdxs[lead];
                    float yOffset = (float)(baseYOffset + (leadSpacing * leadIndex));

                    // 可选：绘制定标电压图形标记（1mV的垂直线段）

                    float calibrationStartY = yOffset - calibrationHeight / 2;
                    float calibrationEndY = yOffset + calibrationHeight / 2;

                    using (SKPaint calibrationPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        StrokeWidth = 1.5f,
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke
                    })
                    {
                        canvas.DrawLine(calibrationX - 1 * pixMM, calibrationEndY, calibrationX, calibrationEndY, calibrationPaint);

                        canvas.DrawLine(calibrationX, calibrationStartY, calibrationX, calibrationEndY, calibrationPaint);

                        canvas.DrawLine(calibrationX, calibrationStartY, calibrationX + 5 * pixMM, calibrationStartY, calibrationPaint);

                        canvas.DrawLine(calibrationX + 5 * pixMM, calibrationStartY, calibrationX + 5 * pixMM, calibrationEndY, calibrationPaint);

                        canvas.DrawLine(calibrationX + 5 * pixMM, calibrationEndY, calibrationX + 6 * pixMM, calibrationEndY, calibrationPaint);
                    }

                }
            }
        }
        public void DrawPageHeader(SKCanvas canvas, int pageNumber, int startSample)
        {
            using (SKFont textFont = new SKFont
            {
                Size = 12.0f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            using(SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true
            })
            {
                // 绘制纸速信息
                string speedText = $"{speed}mm/s {gain}mm/mV";
                canvas.DrawText(speedText, (float)(PaperWidthMm - MarginRightMm) * pixMM, (float)(MarginTopMm - 2) * pixMM, SKTextAlign.Right, textFont, textPaint);
                //患者姓名
                string nameText = $"姓名: {AnaValues.CurrentCase.name}";
                canvas.DrawText(nameText, (float)(MarginLeftMm * pixMM),
                (float)(MarginTopMm - 8) * pixMM, SKTextAlign.Left, textFont, textPaint);
                //起始时间
                int pageStartSample = startSample + pageNumber * RowPoints * currentRows;
                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

                string dateText = AnaCode.getDataTimeFromSample(pageStartSample).ToString(dateFormat);
                canvas.DrawText(dateText, (float)(MarginLeftMm * pixMM),
                (float)(MarginTopMm - 2) * pixMM, SKTextAlign.Left, textFont, textPaint);
                //绘制标签
                canvas.DrawText(label, (float)(PaperWidthMm * pixMM / 2), (float)(MarginTopMm - 8) * pixMM, SKTextAlign.Center, textFont, textPaint);
                // 绘制页码
                int pageNum = pageNumber + 1;
                string footerText = $"{pageNum}/{totalPages}";

                canvas.DrawText(footerText, (float)(PaperWidthMm*pixMM/2), (float)(PaperHeightMm - MarginBottonMm + 5)*pixMM, SKTextAlign.Center, textFont, textPaint);
            }
        }

        /// <summary>
        /// 绘制心搏信息
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawBeatHeartInfo(SKCanvas canvas, int pageNumber, int startSample, double widthPx, double heightPx)
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
                List<BeatInfo> cachedAllBeats = AnaValues.CachedAllBeats;

                int RRCount = 0;
                int RRSum = 0;
                int pageStartSample = startSample + pageNumber * RowPoints * currentRows;

                double waveformWidth = widthPx - (MarginLeftMm + MarginRightMm) * pixMM;
                double waveformHeight = heightPx - (MarginTopMm + MarginBottonMm) * pixMM;
                double rowHeight = waveformHeight / currentRows;

                for (int row = 0; row < currentRows; row++)
                {
                    int startIndex = (row * RowPoints) + pageStartSample;
                    int endIndex = ((row + 1) * RowPoints) + pageStartSample;
                    if (startIndex >= endIndex)
                    {
                        continue;
                    }

                    List<BeatInfo> relevantHeartbeats = cachedAllBeats
                    .Where(h => (h.EndIndex > startIndex && h.StartIndex < endIndex) && !AnaCode.IsInvalidBeat(h))
                    .OrderBy(h => h.StartIndex)
                    .ToList();

                    double leadSpacing = rowHeight / leadIdxs.Count;
                    double startX = waveLeftMargin;
                    double baseY = (rowHeight * row) + waveTopMargin + 2*pixMM;

                    // 绘制RR间期和HR值（从第二个心搏开始）
                    if (relevantHeartbeats.Count > 1)
                    {
                        // 绘制位置在波形上方
                        double textY = baseY;

                        for (int i = 0; i < relevantHeartbeats.Count; i++)
                        {
                            BeatInfo currentBeat = relevantHeartbeats[i];
                            // 计算Sample点（R波）对应的X坐标
                            double sampleX = startX + ((currentBeat.Sample - startIndex) * speed * pixMM / AnaValues.holterConfig.SampleRate);
                            if (sampleX < waveLeftMargin || sampleX > widthPx - MarginRightMm * pixMM)
                            {
                                continue;
                            }
                            // 获取心搏类型
                            char beatType = currentBeat.BeatUiType;

                            // 绘制心搏类型
                            canvas.DrawText(AnaValues.heartbeatTypeMap[beatType.ToString()], (float)sampleX, (float)textY, SKTextAlign.Center, textFont, textPaint);

                            if (currentBeat.RRInterval < 0) continue;
                            if (beatType == 'X') continue;

                            if (i > 0)
                            {
                                BeatInfo prevBeat = relevantHeartbeats[i - 1];

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
                            int hr = (int)(60000 / currentBeat.RRInterval);

                            // 绘制RR间期和HR值（不带单位）
                            string rrStr = $"{currentBeat.RRInterval}";
                            string hrStr = $"{hr}";

                            float textWrr = textFont.MeasureText(rrStr, out SKRect bound1, textPaint);
                            float textWhr = textFont.MeasureText(hrStr, out SKRect bound2, textPaint);
                            float textW = Math.Max(textWrr, textWhr);

                            float middleX = (float)(startX + (currentBeat.Sample - startIndex) * speed * pixMM / AnaValues.holterConfig.SampleRate - currentBeat.RRInterval / 1000f / 2 * speed * pixMM);

                            if (middleX < waveLeftMargin + textW || middleX > widthPx - MarginRightMm * pixMM)
                            {
                                continue;
                            }

                            // 绘制RR间期（上方）
                            canvas.DrawText(rrStr, (float)middleX, (float)textY - 10, SKTextAlign.Center, textFont, textPaint);

                            // 绘制HR值（下方）
                            canvas.DrawText(hrStr, (float)middleX, (float)textY + 2, SKTextAlign.Center, textFont, textPaint);
                        }

                        for (int i = 0; i < relevantHeartbeats.Count; i++)
                        {
                            if (relevantHeartbeats[i].BeatUiType == 'X')
                            {
                                continue;
                            }
                            ++RRCount;
                            RRSum += relevantHeartbeats[i].RRInterval;
                        }
                    }
                }

                int RRAvg = RRSum / RRCount;
                int heartbeatAvg = 60 * 1000 / RRAvg;
                //平均心率
                string hrText = $"心率: {heartbeatAvg}次/分";
                canvas.DrawText(hrText, (float)(PaperWidthMm * pixMM / 2), (float)(MarginTopMm - 2) * pixMM, SKTextAlign.Center, textFont, textPaint);
            }

        }


    }
}
