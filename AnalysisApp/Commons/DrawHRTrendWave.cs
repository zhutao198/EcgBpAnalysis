using SkiaSharp;
using System;
using System.Collections.Generic;

namespace AnalysisApp.Commons
{
    /// <summary>
    /// 分钟心率类
    /// 存储每分钟的心率信息
    /// </summary>
    public class MinuteHRInfo
    {
        /// <summary>
        /// 每分钟心率最小值
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// 每分钟心率平均值
        /// </summary>
        public int Avg { get; set; }

        /// <summary>
        /// 每分钟心率最大值
        /// </summary>
        public int Max { get; set; }
    }

    class DrawHRTrendWave
    {
        private List<MinuteHRInfo> minuteHRData;

        // 坐标系参数
        public int LeftMargin = 0;      // 左边距
        public int RightMargin = 0;     // 右边距
        public int TopMargin = 0;       // 上边距
        public int BottomMargin = 0;    // 下边距
        public int HourLabelHeight = 10; // 小时标签高度

        // DPI缩放因子
        public float DpiScaleFactor = 1.0f;

        // 心率刻度值
        private readonly int[] hrTicks = { 0, 50, 100, 150, 200 };

        public DrawHRTrendWave()
        {
            minuteHRData = AnaValues.minuteHR;
        }

        public DrawHRTrendWave(float dpiScaleFactor)
        {
            minuteHRData = AnaValues.minuteHR;
            DpiScaleFactor = dpiScaleFactor;
        }
        /// <summary>
        /// 绘制背景
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void DrawBackground(SKCanvas canvas, int width, int height)
        {
            // 初始化背景画笔
            using (SKPaint backgroundPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill
            })
            // 绘制白色背景
            canvas.DrawRect(0, 0, width, height, backgroundPaint);
        }
        public void DrawCoordinateSystem(SKCanvas canvas, int width, int height)
        {
            // 计算绘图区域
            int plotWidth = width - LeftMargin - RightMargin;
            int plotHeight = height - TopMargin - BottomMargin - HourLabelHeight;

            // 创建画笔
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1 * DpiScaleFactor,
                IsAntialias = true
            })
            using (SKPaint gridPaint = new SKPaint
            {
                Color = SKColor.Parse("#313848"),
                StrokeWidth = 0.5f * DpiScaleFactor,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true   
            })
            using(SKFont textFont = new SKFont
            {
                Size = 10.5f * DpiScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                // 绘制坐标轴
                float x0 = LeftMargin;
                float y0 = TopMargin + plotHeight;
                float x1 = LeftMargin + plotWidth;
                float y1 = TopMargin;

                // X轴（时间轴）
                canvas.DrawLine(x0, y0, x1, y0, axisPaint);

                // Y轴（心率轴）
                canvas.DrawLine(x0, y0, x0, y1, axisPaint);

                // 绘制Y轴刻度和标签
                for (int i = 1; i < hrTicks.Length; i++)
                {
                    float y = y0 - (hrTicks[i] / 200f) * plotHeight;

                    // 刻度标签
                    string label = hrTicks[i].ToString();
                    canvas.DrawText(label, x0 - 6 * DpiScaleFactor, y + 4 * DpiScaleFactor, SKTextAlign.Right, textFont, textPaint);

                    // 网格线
                    gridPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 2 * DpiScaleFactor, 5 * DpiScaleFactor }, 0);
                    canvas.DrawLine(x0, y, x1, y, gridPaint);
                }

                // 绘制X轴标签（小时）
                // 解析起始时间
                if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
                {
                    startTime = DateTime.Today;
                }
                float preWidth = (float)plotWidth / 24;
                int totalHours = (int)Math.Ceiling(minuteHRData.Count / 60f);
                for (int hour = 1; hour <= 24; hour++)
                {
                    if (hour % 2 != 0)
                    {
                        continue;
                    }
                    // 计算当前行对应的时间
                    int totalSeconds = hour * 60 * 60;
                    DateTime currentTime = startTime.AddSeconds(totalSeconds);
                    // 格式化时间字符串
                    string timeStr = currentTime.ToString("HH:mm");
                    float x = x0 + (hour * preWidth);
                    canvas.DrawText(timeStr, x, y0 + 12 * DpiScaleFactor, SKTextAlign.Center, textFont, textPaint);                    
                    gridPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 2 * DpiScaleFactor, 5 * DpiScaleFactor }, 0);
                    canvas.DrawLine(x, y0, x, TopMargin, gridPaint);


                }
                // 绘制轴标题
                canvas.DrawText("(bpm)", x0, TopMargin - 10 * DpiScaleFactor, SKTextAlign.Right, textFont, textPaint);
            }
        }

        public void DrawHRTrend(SKCanvas canvas, int width, int height)
        {
            int plotWidth = width - LeftMargin - RightMargin;
            int plotHeight = height - TopMargin - BottomMargin - HourLabelHeight;

            using (SKPaint avgLinePaint = new SKPaint
            {
                Color = SKColor.Parse("f32525"),
                StrokeWidth = 1 * DpiScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            using (SKPaint minMaxLinePaint = new SKPaint
            {
                Color = SKColor.Parse("#c0c0c0"),
                StrokeWidth = 1.5f * DpiScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                float x0 = LeftMargin;
                float y0 = TopMargin + plotHeight;

                List<SKPoint> avgPoints = new List<SKPoint>();
                List<SKPoint> minPoints = new List<SKPoint>();
                List<SKPoint> maxPoints = new List<SKPoint>();
                float preWidth = (float)plotWidth / (24 * 60);
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

    }
}
