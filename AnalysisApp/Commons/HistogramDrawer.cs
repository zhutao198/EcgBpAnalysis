using SkiaSharp;
using System;
using System.Collections.Generic;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    public class HistogramDrawer
    {
        /// <summary>
        /// 对心拍进行分组并设置Y轴最大值
        /// </summary>
        /// <param name="chartInfo"></param>
        /// <param name="beats"></param>
        /// <returns></returns>
        public static List<KeyValuePair<float, List<BeatInfo>>> GetData(HistogramInfo chartInfo, List<BeatInfo> beats)
        {
            int maxCount = 0;
            int subNum = chartInfo.SubdivisionCount;
            float step = (float)chartInfo.IntervalX / subNum;
            List<KeyValuePair<float, List<BeatInfo>>> groups = new List<KeyValuePair<float, List<BeatInfo>>>();
            if (beats.Count > 0)
            {
                for (int offset = 0; offset < chartInfo.MaxScaleX; offset += chartInfo.IntervalX)
                {
                    for (int j = 0; j < subNum; j++)
                    {
                        float start = offset + step * j, end = start + step;
                        List<BeatInfo> gpList = beats.FindAll(b => start <= b.State && b.State < end);
                        float midValue = start + step / 2F;
                        groups.Add(new KeyValuePair<float, List<BeatInfo>>(midValue, gpList));
                        if (maxCount < gpList.Count) maxCount = gpList.Count;
                    }
                }
            }
            chartInfo.MaxScaleY = maxCount <= 5 ? 5 : ((maxCount + 9) / 10) * 10;
            chartInfo.IntervalY = (int)Math.Max(chartInfo.MaxScaleY / 6d, maxCount <= 5 ? 1 : 2);

            return groups;
        }

        /// <summary>
        /// 获取每个柱状图所属区间宽度
        /// </summary>
        /// <param name="chartInfo"></param>
        /// <param name="innerRect"></param>
        /// <returns></returns>
        public static float GetStepWidth(HistogramInfo chartInfo, SKRect innerRect)
        {
            float stepWidth = innerRect.Width / 100;
            if (chartInfo != null)
            {
                int cntX = (int)((double)chartInfo.MaxScaleX / chartInfo.IntervalX);
                //int step = (int)((double)ChartInfo.IntervalX / ChartInfo.SubdivisionCount);
                stepWidth = innerRect.Width / cntX / chartInfo.SubdivisionCount;
            }
            return stepWidth;
        }

        /// <summary>
        /// 绘制坐标系(包含内部网格线)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="chartInfo"></param>
        /// <param name="innerRect"></param>
        /// <param name="dpiScale">与当前DPI对应的缩放系数</param>
        public static void DrawCoordinateSystem(SKCanvas canvas, HistogramInfo chartInfo, SKRect innerRect, float dpiScale = 1, bool bPrint = false)
        {
            if (chartInfo == null) return;
            // 计算绘图区域
            //int plotWidth = width - LeftMargin - RightMargin;
            //int plotHeight = height - TopMargin - BottomMargin - HourLabelHeight;

            SKPoint originalPos = new SKPoint(innerRect.Left, innerRect.Bottom);
            float x0 = innerRect.Left, y0 = originalPos.Y;
            float width = innerRect.Width, height = innerRect.Height;
            int maxScaleX = chartInfo.MaxScaleX, maxScaleY = chartInfo.MaxScaleY;
            int cntX = (int)((double)chartInfo.MaxScaleX / chartInfo.IntervalX);
            int cntY = (int)((double)chartInfo.MaxScaleY / chartInfo.IntervalY); ;

            string unit = string.Empty;
            if (chartInfo.ChartType == HistogramType.EarlyRate) { unit = "%"; }

            // 创建画笔
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1f * dpiScale,
                IsAntialias = true
            })
            using (SKPaint gridPaint = new SKPaint
            {
                Color = SKColor.Parse("#313848"),
                StrokeWidth = 0.5f * dpiScale,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * dpiScale
            })
            {
                gridPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 1.5F * dpiScale, 4F * dpiScale }, 0);

                SKFontMetrics metrics = textFont.Metrics;
                float fontHeight = metrics.Descent - metrics.Ascent + metrics.Leading;
                float intervalPx = (float)width / cntX, intervalPy = (float)height / cntY;

                textFont.Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold);
                canvas.DrawText(CommonValues.Langs[chartInfo.Description], innerRect.Left + width / 2, innerRect.Top - fontHeight, SKTextAlign.Center, textFont, textPaint);

                textFont.Typeface = SKTypeface.FromFamilyName("微软雅黑");
                // 绘制X轴、刻度、网格线及刻度值
                canvas.DrawLine(x0, y0, x0 + width, y0, axisPaint);
                for (int i = 0; i <= cntX; i++)
                {
                    float x = x0 + i * intervalPx;
                    //canvas.DrawLine(x, y0, x, y0 - 5, axisPaint); //刻度
                    canvas.DrawLine(x, y0, x, y0 - height, gridPaint); //网格线
                    SKTextAlign align = SKTextAlign.Center;
                    if (i == cntX) align = SKTextAlign.Right;

                    int markValue = (int)(i * chartInfo.IntervalX);
                    string markText = markValue.ToString() + unit;
                    if (markValue >= maxScaleX) markText = maxScaleX + unit + "+";

                    if(bPrint)
                    {
                        if (i < cntX)
                        {
                            canvas.DrawText(markText, x, y0 + Math.Abs(metrics.Ascent), align, textFont, textPaint);
                        }
                        else
                        {
                            canvas.DrawText(chartInfo.AxisXTitle, x + 8 * dpiScale, y0 + Math.Abs(metrics.Ascent), align, textFont, textPaint);
                        }
                    }
                    else
                    {
                        canvas.DrawText(markText, x, y0 + Math.Abs(metrics.Ascent), align, textFont, textPaint);
                        if (i == cntX) canvas.DrawText(chartInfo.AxisXTitle, x + 10 * dpiScale, y0 + Math.Abs(metrics.Ascent), SKTextAlign.Left, textFont, textPaint);
                    }


                    if (chartInfo.ChartType == HistogramType.RRInterval && i > 0)
                    {
                        if(bPrint)
                        {
                            if(i < cntX)
                            {
                                canvas.DrawText(((int)(60000F / markValue)).ToString(), x, y0 + fontHeight + dpiScale + Math.Abs(metrics.Ascent), align, textFont, textPaint);
                            }
                            else
                            {
                                canvas.DrawText(chartInfo.AxisX2Title, x + 10, y0 + fontHeight + dpiScale + Math.Abs(metrics.Ascent), align, textFont, textPaint);
                            }
                        }
                        else
                        {
                            canvas.DrawText(((int)(60000F / markValue)).ToString(), x, y0 + fontHeight + dpiScale + Math.Abs(metrics.Ascent), align, textFont, textPaint);
                            if (i == cntX) canvas.DrawText(chartInfo.AxisX2Title, x + 10, y0 + fontHeight + dpiScale + Math.Abs(metrics.Ascent), SKTextAlign.Left, textFont, textPaint);
                        }

                    }
                }
                // 绘制Y轴、刻度、网格线及刻度值
                canvas.DrawLine(x0, y0, x0, y0 - height, axisPaint);
                for (int i = 1; i <= cntY; i++)
                {
                    float y = y0 - i * intervalPy;
                    //canvas.DrawLine(x0, y, x0 + 5, y, axisPaint);
                    canvas.DrawLine(x0, y, x0 + width, y, gridPaint);
                    int markValue = (int)(i * chartInfo.IntervalY);
                    canvas.DrawText(markValue.ToString(), x0 - 2 * dpiScale, y + (float)Math.Floor(Math.Abs(metrics.Ascent) / 2), SKTextAlign.Right, textFont, textPaint);
                    if (i == cntY) canvas.DrawText(chartInfo.AxisYTitle, x0 - 2 * dpiScale, y - fontHeight, SKTextAlign.Right, textFont, textPaint);
                }
            }
        }

    }

    public class RRHistogram
    {
        private SKRect _innerRect;
        private HistogramInfo _chartInfo;
        private float _stepWidth;
        private float _dpiScale = 1.0f;
        private SKPaint blockRectPaint;

        private PersistDictionary<string> _langs = CommonValues.Langs;
        private List<KeyValuePair<float, List<BeatInfo>>> _dataGroups = null;
        private const float COL_PROPORTION = 0.6F; //柱形条占其所属区间的宽度比例

        public RRHistogram(float dpiScale, SKRect rect)
        {
            _dpiScale = dpiScale;
            _innerRect = rect;
        }

        private void init()
        {
            _chartInfo = new HistogramInfo(HistogramType.RRInterval)
            {
                TypeId = "RR",
                Description = "",
                BeatsTypeCode = "**",
            };
            _chartInfo.MaxScaleX = 4000;
            //_chartInfo.MaxScaleY = 200;
            _chartInfo.AxisXTitle = _langs["RR间期"] + "(ms)";
            _chartInfo.AxisX2Title = _langs["心率"] + "(bpm)";
            _chartInfo.AxisYTitle = _langs["心搏数"];
            _chartInfo.IntervalX = _chartInfo.MaxScaleX / 10;

            blockRectPaint = new SKPaint()
            {
                //IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = new SKColor(59, 149, 197)
            };

            _stepWidth = HistogramDrawer.GetStepWidth(_chartInfo, _innerRect);
        }

        public void DrawHistogram(SKCanvas canvas)
        {
            init();

            List<BeatInfo> beats = AnaCode.QueryHistogramData(_chartInfo);
            _dataGroups = HistogramDrawer.GetData(_chartInfo, beats);

            HistogramDrawer.DrawCoordinateSystem(canvas, _chartInfo, _innerRect, _dpiScale, true);
            drawDataGroupBlocks(canvas);
        }

        //绘制矩形区块
        private void drawDataGroupBlocks(SKCanvas canvas)
        {
            if (_chartInfo == null) return;

            float blockWidth = _stepWidth * COL_PROPORTION; ;
            float xPerVal = _innerRect.Width / _chartInfo.MaxScaleX, yPerVal = _innerRect.Height / _chartInfo.MaxScaleY;
            float x0 = _innerRect.Left, y0 = _innerRect.Bottom;
            float offset = x0;

            float textLeft = -1, textRight = x0; //心搏数量文本绘制区左右边缘位置
            float textY = y0;
            for (int i = 0; i < _dataGroups.Count; i++)
            {
                List<BeatInfo> beats = _dataGroups[i].Value;
                float x = offset + (_stepWidth - blockWidth) / 2;
                if (beats.Count > 0)
                {
                    float rectH = Math.Max(beats.Count * yPerVal, 3);
                    float y = y0 - rectH;

                    canvas.DrawRect(x, y, blockWidth, rectH, blockRectPaint);
                }
                offset += _stepWidth;
            }
        }

    }

}
