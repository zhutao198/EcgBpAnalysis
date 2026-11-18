using SkiaSharp;
using System;
using System.Collections.Generic;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class DrawBpFitPlot
    {
        public float WaveLeft { get; set; } = 0;
        public float WaveTop { get; set; } = 0;
        public float WaveWidth { get; set; } = 0;
        public float WaveHeight { get; set; } = 0;

        private float PaddingLeft = 0;
        private float PaddingTop = 2 * 3.777f;

        public float CoorPaddingLeft = 0;
        public float CoorPaddingRight = 0;
        public float CoorPaddingTop = 0;
        public float CoorPaddingBottom = 0;
        private float CoorHeight = 0;
        private float CoorWidth = 0;
        private float RectHeight = 0;

        private float PreWidth = 0;
        private float PreHeightBp = 0;
        private float PreHeightPulse = 0;

        private float PixMM = 3.777f;

        private SKColor DayColor = SKColor.Parse("#FFB000");
        private SKColor NightColor = SKColor.Parse("#3B95C5");

        public float FontScaleFactor = 1;

        public bpDiagnoseSettingValues settingValues;

        public List<double> DaySbpList = new List<double>();
        public List<double> DayDbpList = new List<double>();
        public List<int> DayPulseList = new List<int>();

        public List<double> NightSbpList = new List<double>();
        public List<double> NightDbpList = new List<double>();
        public List<int> NightPulseList = new List<int>();

        public List<SKPoint> SbpSlopePoints = new List<SKPoint>(2);
        public List<SKPoint> PulseSlopePoints = new List<SKPoint>(2);

        public DrawBpFitPlot()
        {

        }

        public void DrawCoordinate(SKCanvas canvas)
        {
            // 创建画笔
            using (SKPaint linePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,
            })
            using (SKPaint dashLinePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.6f * FontScaleFactor,
                PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint rectPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,
                Color = SKColor.Parse("#E4E9ED"),
            })
            using (SKPaint fillRectPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            })
            using (SKFont labelFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 12f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            using (SKFont textFont = new SKFont
            {
                Size = 12f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            {

                RectHeight = (WaveHeight - PaddingTop * 2) / 2;
                CoorHeight = RectHeight - CoorPaddingTop - CoorPaddingBottom;
                CoorWidth = WaveWidth - CoorPaddingLeft - CoorPaddingRight;

                string unit = settingValues.unit;
                int[] bpAxisX = new int[6] { 0, 50, 100, 150, 200, 250 };
                if (unit == "kPa")
                {
                    bpAxisX = new int[6] { 0, 7, 14, 21, 28, 35 };
                }

                int[] bpAxisY = new int[5] { 0, 50, 100, 150, 200 };
                if (unit == "kPa")
                {
                    bpAxisY = new int[5] { 0, 7, 14, 21, 28 };
                }

                int[] pluseAxisY = { 0, 50, 100, 150, 200 };

                PreWidth = CoorWidth / bpAxisX[bpAxisX.Length - 1];
                PreHeightBp = CoorHeight / bpAxisY[bpAxisY.Length - 1];
                PreHeightPulse = CoorHeight / pluseAxisY[pluseAxisY.Length - 1];

                float preCoorHeight = CoorHeight / 4;
                float preCoorWidth = CoorWidth / 5;
                for (int i = 0; i < 2; i++)
                {
                    float baseX = WaveLeft + PaddingLeft;
                    float baseY = WaveTop + i * (PaddingTop + RectHeight);
                    if(FontScaleFactor == 1)
                    {
                        canvas.DrawRect(baseX, baseY, WaveWidth, RectHeight, rectPaint);
                    }
                    canvas.DrawLine(baseX + CoorPaddingLeft, baseY + CoorPaddingTop, baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight, linePaint);
                    canvas.DrawLine(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight, baseX + CoorPaddingLeft + CoorWidth, baseY + CoorPaddingTop + CoorHeight, linePaint);

                    if (i == 0)
                    {
                        canvas.DrawText("舒张压(" + settingValues.unit + ")", baseX + CoorPaddingLeft + 0.5f * PixMM * FontScaleFactor, baseY + 5.5f* PixMM * FontScaleFactor, SKTextAlign.Center, titleFont, textPaint);
                        canvas.DrawText("收缩压(" + settingValues.unit + ")", baseX + CoorPaddingLeft + CoorWidth + 1.6f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight, SKTextAlign.Left, titleFont, textPaint);
                    }
                    else
                    {
                        canvas.DrawText("脉率(次/分)", baseX + CoorPaddingLeft + 0.5f * PixMM * FontScaleFactor, baseY + 5.5f * PixMM * FontScaleFactor, SKTextAlign.Center, titleFont, textPaint);
                        canvas.DrawText("收缩压(" + settingValues.unit + ")", baseX + CoorPaddingLeft + CoorWidth + 1.6f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight, SKTextAlign.Left, titleFont, textPaint);
                    }

                    for (int j = 0; j < 5; j++)
                    {
                        if (j != 4)
                        {
                            canvas.DrawLine(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + preCoorHeight * j, baseX + CoorPaddingLeft + CoorWidth, baseY + CoorPaddingTop + preCoorHeight * j, dashLinePaint);
                        }
                        string label = "";
                        if (i == 0)
                        {
                            label = bpAxisY[j].ToString();
                        }
                        else
                        {
                            label = pluseAxisY[j].ToString();
                        }
                        canvas.DrawText(label, baseX + CoorPaddingLeft - 1.6f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * j + 1.6f * PixMM * FontScaleFactor, SKTextAlign.Right, textFont, textPaint);
                    }

                    for (int j = 0; j < 6; j++)
                    {
                        if (j != 0)
                        {
                            canvas.DrawLine(baseX + CoorPaddingLeft + j * preCoorWidth, baseY + CoorPaddingTop, baseX + CoorPaddingLeft + j * preCoorWidth, baseY + CoorPaddingTop + CoorHeight, dashLinePaint);
                        }
                        canvas.DrawText(bpAxisX[j].ToString(), baseX + CoorPaddingLeft + j * preCoorWidth + 1.6f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight + 3 * PixMM * FontScaleFactor, SKTextAlign.Right, textFont, textPaint);
                    }

                    fillRectPaint.Color = DayColor;
                    canvas.DrawRect(baseX + CoorPaddingLeft + CoorWidth + 1.6f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight / 2, 8 * PixMM * FontScaleFactor, 4 * PixMM * FontScaleFactor, fillRectPaint);
                    canvas.DrawText("白天", baseX + CoorPaddingLeft + CoorWidth + 11 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight / 2 + 3 * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);

                    fillRectPaint.Color = NightColor;
                    canvas.DrawRect(baseX + CoorPaddingLeft + CoorWidth + 1.6f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight / 2 + 6.3f * PixMM * FontScaleFactor, 8 * PixMM * FontScaleFactor, 4 * PixMM * FontScaleFactor, fillRectPaint);
                    canvas.DrawText("夜间", baseX + CoorPaddingLeft + CoorWidth + 11 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight / 2 + 9.5f * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);
                }
            }
        }

        public void DrawFitPlot(SKCanvas canvas)
        {
            using (SKPaint linePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,
            })
            using (SKPaint pointPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 4.0f * FontScaleFactor,  // 点的大小
            })
            {
                float baseX = WaveLeft + PaddingLeft + CoorPaddingLeft;
                for (int i = 0; i < 2; i++)
                {
                    float baseY = WaveTop + CoorPaddingTop + i * (PaddingTop + RectHeight);
                    List<SKPoint> dayPoints = new List<SKPoint>(DaySbpList.Count);
                    for (int j = 0; j < DaySbpList.Count; j++)
                    {
                        double dbp = DayDbpList[j];
                        double sbp = DaySbpList[j];
                        
                        if(settingValues.unit == "kPa")
                        {
                            dbp = CommonFuns.mmHg2KPa(Convert.ToInt32(DayDbpList[j]), 2);
                            sbp = CommonFuns.mmHg2KPa(Convert.ToInt32(DaySbpList[j]), 2);
                        }

                        float x = baseX + (float)sbp * PreWidth;
                        float y = 0f;
                        if (i == 0)
                        {
                            y = baseY + CoorHeight - (float)dbp * PreHeightBp;
                        }
                        else
                        {
                            y = baseY + CoorHeight - DayPulseList[j] * PreHeightPulse;
                        }
                        dayPoints.Add(new SKPoint(x, y));
                    }
                    pointPaint.Color = DayColor;
                    canvas.DrawPoints(SKPointMode.Points, dayPoints.ToArray(), pointPaint);

                    List<SKPoint> nightPoints = new List<SKPoint>(NightSbpList.Count);
                    for (int j = 0; j < NightSbpList.Count; j++)
                    {
                        double sbp = NightSbpList[j];
                        double dbp = NightDbpList[j];

                        if (settingValues.unit == "kPa")
                        {
                            sbp = CommonFuns.mmHg2KPa(Convert.ToInt32(NightSbpList[j]), 2);
                            dbp = CommonFuns.mmHg2KPa(Convert.ToInt32(NightDbpList[j]), 2);
                        }

                        float x = baseX + (float)sbp * PreWidth;
                        float y = 0f;
                        if (i == 0)
                        {
                            y = baseY + CoorHeight - (float)dbp * PreHeightBp;
                        }
                        else
                        {
                            y = baseY + CoorHeight - NightPulseList[j] * PreHeightPulse;
                        }
                        nightPoints.Add(new SKPoint(x, y));
                    }
                    pointPaint.Color = NightColor;
                    canvas.DrawPoints(SKPointMode.Points, nightPoints.ToArray(), pointPaint);

                    if (i == 0)
                    {
                        float lineX0 = SbpSlopePoints[0].X;
                        float lineY0 = SbpSlopePoints[0].Y;
                        float lineX1 = SbpSlopePoints[1].X;
                        float lineY1 = SbpSlopePoints[1].Y;
                        if (settingValues.unit == "kPa")
                        {
                            lineX0 = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(SbpSlopePoints[0].X), 2);
                            lineY0 = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(SbpSlopePoints[0].Y), 2);

                            lineX1 = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(SbpSlopePoints[1].X), 2);
                            lineY1 = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(SbpSlopePoints[1].Y), 2);
                        }

                        float x0 = baseX + (float)lineX0 * PreWidth;
                        float y0 = baseY + CoorHeight - (float)lineY0 * PreHeightBp;

                        float x1 = baseX + (float)lineX1 * PreWidth;
                        float y1 = baseY + CoorHeight - (float)lineY1 * PreHeightBp;

                        x0 = Math.Min(x0, baseX + CoorWidth);
                        x1 = Math.Min(x1, baseX + CoorWidth);

                        y0 = Math.Min(y0, baseY + CoorHeight);
                        y1 = Math.Min(y1, baseY + CoorHeight);

                        canvas.DrawLine(x0,y0,x1,y1,linePaint);
                    }
                    else
                    {
                        float lineX0 = PulseSlopePoints[0].X;
                        float lineY0 = PulseSlopePoints[0].Y;
                        float lineX1 = PulseSlopePoints[1].X;
                        float lineY1 = PulseSlopePoints[1].Y;
                        if (settingValues.unit == "kPa")
                        {
                            lineX0 = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(SbpSlopePoints[0].X), 2);
                            lineX1 = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(SbpSlopePoints[1].X), 2);
                        }

                        float x0 = baseX + (float)lineX0 * PreWidth;
                        float y0 = baseY + CoorHeight - (float)lineY0 * PreHeightPulse;

                        float x1 = baseX + (float)lineX1 * PreWidth;
                        float y1 = baseY + CoorHeight - (float)lineY1 * PreHeightPulse;

                        x0 = Math.Min(x0, baseX + CoorWidth);
                        x1 = Math.Min(x1, baseX + CoorWidth);

                        y0 = Math.Min(y0, baseY + CoorHeight);
                        y1 = Math.Min(y1, baseY + CoorHeight);

                        canvas.DrawLine(x0, y0, x1, y1, linePaint);
                    }
                }


                //canvas.DrawLine(PulseSlopePoints[0], PulseSlopePoints[1], linePaint);

            }
        }
    }
}
