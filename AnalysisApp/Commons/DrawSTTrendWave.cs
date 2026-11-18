using SkiaSharp;
using System;
using System.Collections.Generic;
using Zoncare.Common;
using static AnalysisApp.CommonControls.STTrendWaveControl;

namespace AnalysisApp.Commons
{
    class DrawSTTrendWave
    {
        private List<MinuteHRInfo> minuteHRData;

        public float WaveLeftMargin { get; set; } = 0;
        public float WaveTopMargin { get; set; } = 0;
        public float WaveRightMargin { get; set; } = 0;
        public float WaveBottomMargin { get; set; } = 0;

        public float FontScaleFactor = 1;

        public SKRect WaveRect { get; set; } = new SKRect();

        public bool bLastRect = false;

        List<MinStxInfo> StxInfo = new List<MinStxInfo>();

        public DrawSTTrendWave()
        {
            minuteHRData = AnaValues.minuteHR;

        }

        public void DrawWave(SKCanvas canvas, string label, List<float> ticks)
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
            canvas.DrawRect(0, 0, width, height, backgroundPaint);

        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawRect(SKCanvas canvas)
        {
            using (SKPaint rectPaint = new SKPaint())
            {
                rectPaint.IsAntialias = true;
                rectPaint.StrokeWidth = 0.3f * FontScaleFactor;
                rectPaint.Style = SKPaintStyle.Stroke;
                rectPaint.Color = SKColor.Parse("#313848");

                SKRect borderRect = new SKRect(WaveRect.Left, WaveRect.Top, WaveRect.Right, WaveRect.Bottom);
                canvas.DrawRect(borderRect, rectPaint);
            }
        }

        public void DrawCoordinateSystem(SKCanvas canvas, string label, List<float>ticks)
        {
            // 创建画笔
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColor.Parse("#313848"),
                StrokeWidth = 0.5f * FontScaleFactor,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKFont labelFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            using (SKFont textFont = new SKFont
            {
                Size = 9f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            {
                // 绘制坐标轴
                float x0 = WaveRect.Left + WaveLeftMargin;
                float y0 = WaveRect.Top + WaveTopMargin;
                float x1 = WaveRect.Right - WaveRightMargin;
                float y1 = WaveRect.Bottom - WaveBottomMargin;

                axisPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 2, 5 }, 0);

                // X轴（时间轴）
                canvas.DrawLine(x0, y1, x1, y1, axisPaint);

                // Y轴（心率轴）
                canvas.DrawLine(x0, y0, x0, y1, axisPaint);

                if (bLastRect)
                {
                    // 绘制X轴标签（小时）
                    // 解析起始时间
                    if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
                    {
                        startTime = DateTime.Today;
                    }
                    float preWidth = (float)(WaveRect.Width - WaveLeftMargin - WaveRightMargin) / 24;
                    int totalHours = (int)Math.Ceiling(minuteHRData.Count / 60f);
                    for (int hour = 1; hour <= 24; hour++)
                    {
                        float x = x0 + (hour * preWidth);
                        canvas.DrawLine(x, y1, x, y1 - 5, textPaint);
                        // 计算当前行对应的时间
                        int totalSeconds = hour * 60 * 60;
                        DateTime currentTime = startTime.AddSeconds(totalSeconds);
                        // 格式化时间字符串
                        string timeStr = currentTime.ToString("HH:mm");
                        if(FontScaleFactor > 1)
                        {
                            if(hour%2 == 0)
                            {
                                continue;
                            }
                        }
                        canvas.DrawText(timeStr, x, y1 + 8* FontScaleFactor, SKTextAlign.Center, textFont, textPaint);
                    }
                }

                float preTickHeight = (WaveRect.Height - WaveTopMargin - WaveBottomMargin) / (ticks.Count-1);
                // 绘制Y轴刻度和标签
                for (int i = 0; i < ticks.Count; i++)
                {

                    float y = y1 - i*preTickHeight;

                    if (i % ((ticks.Count -1)/2) == 0)
                    {
                        // 刻度标签
                        canvas.DrawText(ticks[i].ToString(), x0 - 6, y+2, SKTextAlign.Right, textFont, textPaint);
                    }
                    // 刻度线
                    canvas.DrawLine(x0, y, x0 + 3, y, textPaint);
                }

                // 绘制轴标题
                canvas.DrawText(label, WaveRect.Left + 3, WaveRect.Top + WaveTopMargin*3, SKTextAlign.Left, labelFont, textPaint);
            }
        }


        public void DrawHRTrend(SKCanvas canvas)
        {
            int plotWidth = (int)(WaveRect.Width-WaveLeftMargin-WaveRightMargin);
            int plotHeight = (int)(WaveRect.Height-WaveTopMargin-WaveBottomMargin);

            using (SKPaint avgLinePaint = new SKPaint
            {
                Color = SKColor.Parse("f32525"),
                StrokeWidth = 1 * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            using (SKPaint minMaxLinePaint = new SKPaint
            {
                Color = SKColor.Parse("#c0c0c0"),
                StrokeWidth = 1.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                float x0 = WaveRect.Left + WaveLeftMargin;
                float y0 = WaveRect.Top + WaveTopMargin + plotHeight;

                List<SKPoint> avgPoints = new List<SKPoint>();
                List<SKPoint> minPoints = new List<SKPoint>();
                List<SKPoint> maxPoints = new List<SKPoint>();

                float preWidth = (float)plotWidth / (24*60);
                // 计算每个数据点的坐标
                for (int minute = 0; minute < minuteHRData.Count; minute++)
                {
                    MinuteHRInfo hrInfo = minuteHRData[minute];

                    // 计算X坐标（时间，以分钟为单位）
                    float x = x0 + (minute * preWidth);

                    // 计算Y坐标（心率值，归一化到0-200范围）
                    float yAvg = y0 - (hrInfo.Avg / 200f) * plotHeight;
                    float yMin = y0 - (hrInfo.Min / 200f) * plotHeight;
                    float yMax = y0 - (hrInfo.Max / 200f) * plotHeight;

                    avgPoints.Add(new SKPoint(x, yAvg));
                    minPoints.Add(new SKPoint(x, yMin));
                    maxPoints.Add(new SKPoint(x, yMax));
                }

                // 绘制每分钟心率最小值和最大值的直线
                for (int i = 0; i < minPoints.Count; i++)
                {
                    canvas.DrawLine(minPoints[i], maxPoints[i], minMaxLinePaint);
                }

                // 绘制平均心率曲线
                if (avgPoints.Count > 1)
                {
                    using (var path = new SKPath())
                    {
                        path.MoveTo(avgPoints[0]);
                        for (int i = 1; i < avgPoints.Count; i++)
                        {
                            path.LineTo(avgPoints[i]);
                        }
                        canvas.DrawPath(path, avgLinePaint);
                    }
                }


            }
        }

        private void AddLineToPath(SKPath path, float x1, float y1, float x2, float y2)
        {
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            path.LineTo(x2 + 1, y2);
            path.MoveTo(x1, y1);
        }

        private void DrawPaths(SKCanvas canvas, SKPath redPath, SKPath greenPath)
        {
            using (SKPaint redPaint = new SKPaint
            {
                Color = SKColor.Parse("#A04118"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint greenPaint = new SKPaint
            {
                Color = SKColor.Parse("#003300"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f * FontScaleFactor,
                IsAntialias = true
            })
            {
                if (!redPath.IsEmpty)
                {
                    canvas.DrawPath(redPath, redPaint);
                }

                if (!greenPath.IsEmpty)
                {
                    canvas.DrawPath(greenPath, greenPaint);
                }
            }
        }

        public void DrawTTrend(SKCanvas canvas, string type, List<float> ticks, List<MinStxInfo> stxInfo)
        {
            StxInfo = stxInfo;
            float plotWidth = WaveRect.Width - WaveLeftMargin - WaveRightMargin;
            float plotHeight = WaveRect.Height - WaveTopMargin - WaveBottomMargin;
            int totalHours = 24;
            float prePlotWidth = plotWidth / (totalHours * 3600 * AnaValues.holterConfig.SampleRate);

            using (SKPaint linePaint = new SKPaint
            {
                Color = SKColor.Parse("#313848"),
                StrokeWidth = 0.5f * FontScaleFactor,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
            })
            using (SKPath redPath = new SKPath())
            using (SKPath greenPath = new SKPath())
            {
                canvas.DrawLine(WaveRect.Left + WaveLeftMargin, WaveRect.Top + WaveTopMargin + plotHeight / 2, WaveRect.Left + WaveLeftMargin + plotWidth, WaveRect.Top + WaveTopMargin + plotHeight / 2,linePaint);
                for (int i = 0; i < stxInfo.Count; i++)
                {
                    double ampValue = 0;
                    if (type == "ST")
                    {
                        ampValue = stxInfo[i].STAmp / 1000f;
                    }
                    else if (type == "T")
                    {
                        ampValue = stxInfo[i].TAmp / 1000f;
                    }
                    int sample = AnaCode.getSampleFromDateTime(stxInfo[i].StartTime);

                    float x1 = WaveRect.Left + WaveLeftMargin + sample * prePlotWidth;
                    float x2 = x1;
                    float y1 = WaveRect.Top + WaveTopMargin + plotHeight / 2;
                    float y2 = (float)(y1 - ampValue / Math.Abs(ticks[0]) * plotHeight / 2);
                    if (y2 > WaveRect.Bottom)
                    {
                        y2 = WaveRect.Bottom;
                    }
                    else if (y2 < WaveRect.Top)
                    {
                        y2 = WaveRect.Top;
                    }

                    if (ampValue > 0)
                    {
                        AddLineToPath(redPath, x1, y1, x1, y2);
                    }
                    else if (ampValue < 0)
                    {
                        AddLineToPath(greenPath, x1, y1, x1, y2);
                    }
                }

                DrawPaths(canvas, redPath, greenPath);
            }
        }

        public void DrawClickLine(SKCanvas canvas, string type, int minute)
        {
            using (SKPaint LinePaint = new SKPaint
            {
                Color = SKColor.Parse("#003300"),
                StrokeWidth = 1.0f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColor.Parse("#003300"),
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 9f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            {
                int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;

                int plotWidth = (int)(WaveRect.Width - WaveLeftMargin - WaveRightMargin);
                float preWidth = (float)plotWidth / (24 * 60);

                string lineInfo = "";
                if (type == "ST")
                {
                    minute = Math.Min(StxInfo.Count - 1, minute);
                    string STAmp = ((float)Math.Round(StxInfo[minute].STAmp / 1000, 2)).ToString();
                    string StartTime = "";
                    if (dateFormat == 0)
                    {
                        StartTime = StxInfo[minute].StartTime.ToString("HH:mm MM-dd");
                    }
                    else
                    {
                        StartTime = StxInfo[minute].StartTime.ToString("HH:mm dd-MM");
                    }

                    lineInfo = $"{STAmp}mV {StartTime}";
                }
                else if (type == "T")
                {
                    minute = Math.Min(StxInfo.Count - 1, minute);

                    string TAmp = ((float)Math.Round(StxInfo[minute].TAmp / 1000, 2)).ToString();
                    string StartTime = "";
                    if (dateFormat == 0)
                    {
                        StartTime = StxInfo[minute].StartTime.ToString("HH:mm MM-dd");
                    }
                    else
                    {
                        StartTime = StxInfo[minute].StartTime.ToString("HH:mm dd-MM");
                    }

                    lineInfo = $"{TAmp}mV {StartTime}";
                }
                else if (type == "bpm")
                {
                    minute = Math.Min(minuteHRData.Count - 1, minute);

                    lineInfo = $"{minuteHRData[minute].Avg}bpm";
                }
                // 计算X坐标（时间，以分钟为单位）
                float x0 = WaveRect.Left + WaveLeftMargin;
                float x = x0 + (minute * preWidth);

                canvas.DrawLine(x, WaveRect.Top, x, WaveRect.Bottom, LinePaint);

                float textW = textFont.MeasureText(lineInfo, out SKRect bound, textPaint);
                if(x + 6 + textW > plotWidth)
                {
                    canvas.DrawText(lineInfo, x - 6, WaveRect.Top + 12, SKTextAlign.Right, textFont, textPaint);

                }
                else
                {
                    canvas.DrawText(lineInfo, x + 6, WaveRect.Top + 12, SKTextAlign.Left, textFont, textPaint);

                }
            }
        }

    }
}
