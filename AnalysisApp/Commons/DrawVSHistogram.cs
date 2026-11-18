using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalysisApp.Commons
{
    class DrawVSHistogram
    {
        private int yAxisMax = 0;

        public float canvasWidth = 0;
        public float canvasHeight = 80f;
        public float chartWidth = 0;
        public float chartHeight = 0;
        public float paddingTop = 10f;
        public float paddingLeft = 25f;
        public float marginTop = 0;
        public float marginLeft = 0;
        public int tickCount = 5;

        // DPI缩放因子
        public float DpiScaleFactor = 1.0f;

        public DrawVSHistogram() { }

        public DrawVSHistogram(float dpiScaleFactor)
        {
            DpiScaleFactor = dpiScaleFactor;
            // 缩放固定尺寸
            canvasHeight = 85f * dpiScaleFactor;
            paddingTop = 10f * dpiScaleFactor;
            paddingLeft = 25f * dpiScaleFactor;
        }


        public void drawAxis(SKCanvas canvas, List<HourlistInfo> hourlistInfo, char type)
        {
            if (hourlistInfo.Count == 0) return;
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 11f * DpiScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            // 绘制Y轴刻度
            using (SKFont tickFont = new SKFont
            {
                Size = 9f * DpiScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                chartWidth = canvasWidth - paddingLeft;
                chartHeight = canvasHeight - (2 * paddingTop);
                // 计算Y轴最大值
                int maxValue = 0;
                if(type == 'V')
                {
                    maxValue = hourlistInfo.Take(hourlistInfo.Count - 1).Max(x => x.TotalBeatV);
                }
                else
                {
                    maxValue = hourlistInfo.Take(hourlistInfo.Count - 1).Max(x => x.TotalBeatS);
                }

                yAxisMax = Convert.ToInt32(Math.Ceiling(maxValue / 5.0) * 5);

                // X轴
                canvas.DrawLine(marginLeft + paddingLeft, marginTop + canvasHeight - paddingTop, marginLeft + paddingLeft + chartWidth, marginTop + canvasHeight - paddingTop, axisPaint);
                // Y轴
                canvas.DrawLine(marginLeft + paddingLeft, marginTop + canvasHeight - paddingTop, marginLeft + paddingLeft, marginTop + paddingTop, axisPaint);

                for (int i = 1; i <= tickCount; i++)
                {
                    float yValue = yAxisMax * i / (float)tickCount;
                    float yPos = marginTop + canvasHeight - paddingTop - (chartHeight * i / tickCount);

                    // 绘制刻度线
                    //canvas.DrawLine(marginLeft + margin + 5, yPos, marginLeft + margin, yPos, axisPaint);

                    // 绘制刻度值（整数显示）
                    string tickText = yValue % 1 == 0 ? $"{yValue:F0}" : $"{yValue:F1}";
                    canvas.DrawText(tickText, marginLeft + paddingLeft - 5, yPos + 3, SKTextAlign.Right, tickFont, axisPaint);
                }

                if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime dateTime))
                {
                    dateTime = DateTime.Today;
                }
                for (int i = 0; i < 25; i++)
                {
                    //HourlistInfo hour = hourlistInfo[i];

                    string labelText = dateTime.Hour.ToString("D2");
                    float x = 0;
                    float y = marginTop + canvasHeight - paddingTop;

                    //labelText = hour.Time.Split(':')[0];
                    x = marginLeft + paddingLeft + (i * (chartWidth) / 25);
                    // 绘制刻度线
                    //canvas.DrawLine(x, y, x, y - 5, axisPaint);

                    if (i % 2 == 0)
                    {
                        float labelY = marginTop + canvasHeight - paddingTop + 30;
                        canvas.DrawText(labelText, x, labelY, SKTextAlign.Center, tickFont, axisPaint);
                    }
                    dateTime = dateTime.AddHours(1);

                }
                //canvas.DrawText("(个)", margin - 5, margin - 5, SKTextAlign.Left, tickFont, axisPaint);

                //canvas.DrawText("(时)", chartWidth + margin + 5, chartHeight + margin + 5, SKTextAlign.Left, tickFont, axisPaint);
            }
        }
        public void drawHistogram(SKCanvas canvas, List<HourlistInfo> hourlistInfo, char type)
        {
            if (hourlistInfo.Count == 0) return;
            using (SKPaint barPaint = new SKPaint
            {
                Color = SKColor.Parse("#3498db"),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            using (SKPaint barBordPaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                // 绘制柱状图
                for (int i = 0; i < 25; i++)
                {
                    if (i > hourlistInfo.Count - 2) return;
                    int beatCount = 0;
                    if (type == 'V')
                    {
                        beatCount = hourlistInfo[i].TotalBeatV;
                    }
                    else
                    {
                        beatCount = hourlistInfo[i].TotalBeatS;
                    }

                    float barHeight = beatCount / (float)yAxisMax * chartHeight;
                    float barWidth = chartWidth / 25;

                    float x = marginLeft + paddingLeft + (i * barWidth);
                    float y = marginTop + canvasHeight - paddingTop - barHeight;

                    canvas.DrawRect(x, y, barWidth, barHeight, barPaint);
                    canvas.DrawRect(x, y, barWidth, barHeight, barBordPaint);
                }
            }
        }
    }

}
