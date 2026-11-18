using SkiaSharp;
using System;
using System.Collections.Generic;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class PrintBpFitPlot
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

        private float PreWidth = 0;
        private float PreHeightBp = 0;

        private float[] bpAxisX = new float[11] { 60, 85, 100, 120, 140, 160, 180, 200, 220, 240, 260 };

        private int[] bpAxisY = new int[13] { 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160 };


        private float PixMM = 3.777f;

        private SKColor DayColor = SKColor.Parse("#FFB000");
        private SKColor NightColor = SKColor.Parse("#3B95C5");

        public float FontScaleFactor = 1;

        public bpDiagnoseSettingValues settingValues;

        public List<double> DaySbpList = new List<double>();
        public List<double> DayDbpList = new List<double>();

        public List<double> NightSbpList = new List<double>();
        public List<double> NightDbpList = new List<double>();

        public List<SKPoint> SbpSlopePoints = new List<SKPoint>(2);

        public PrintBpFitPlot()
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
                Color = SKColors.Black,
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
                CoorHeight = WaveHeight - CoorPaddingTop - CoorPaddingBottom;
                CoorWidth = WaveWidth - CoorPaddingLeft - CoorPaddingRight;

                string unit = settingValues.unit;
                if (unit == "kPa")
                {
                    bpAxisX = new float[11] {7, 11.3f, 13, 16, 19, 22, 25, 28, 31, 34, 37 };
                }

                if (unit == "kPa")
                {
                    bpAxisY = new int[13] { 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29 };
                }

                PreWidth = CoorWidth / (bpAxisX[bpAxisX.Length - 1] - bpAxisX[0]);
                PreHeightBp = CoorHeight / (bpAxisY[bpAxisY.Length - 1] - bpAxisY[0]);

                float preCoorWidth = CoorWidth / (bpAxisX[bpAxisX.Length - 1] - bpAxisX[0]);
                float preCoorHeight = CoorHeight / (bpAxisY[bpAxisY.Length - 1] - bpAxisY[0]);

                float baseX = WaveLeft + PaddingLeft;
                float baseY = WaveTop;
                canvas.DrawText("舒张压(" + settingValues.unit + ")", baseX + CoorPaddingLeft + 1.5f * PixMM * FontScaleFactor, baseY - 1.8f* PixMM * FontScaleFactor, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText("收缩压(" + settingValues.unit + ")", baseX + CoorPaddingLeft + CoorWidth + 1.6f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight, SKTextAlign.Left, textFont, textPaint);


                for (int j = 0; j < bpAxisY.Length; j++)
                {
                    canvas.DrawLine(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + preCoorHeight * (bpAxisY[j] - bpAxisY[0]), baseX + CoorPaddingLeft - 1.5f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + preCoorHeight * (bpAxisY[j] -bpAxisY[0]), linePaint);
                    canvas.DrawText(bpAxisY[j].ToString(), baseX + CoorPaddingLeft - 2.0f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[j] - bpAxisY[0]) + 1.6f * PixMM * FontScaleFactor, SKTextAlign.Right, textFont, textPaint);
                }

                for (int j = 0; j < bpAxisX.Length; j++)
                {
                    canvas.DrawLine(baseX + CoorPaddingLeft + (bpAxisX[j]- bpAxisX[0]) * preCoorWidth, baseY + CoorPaddingTop + CoorHeight, baseX + CoorPaddingLeft + (bpAxisX[j] - bpAxisX[0]) * preCoorWidth, baseY + CoorPaddingTop + CoorHeight + 1.5f * PixMM * FontScaleFactor, linePaint);
                    canvas.DrawText(bpAxisX[j].ToString(), baseX + CoorPaddingLeft + (bpAxisX[j] - bpAxisX[0]) * preCoorWidth + 1.6f * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight + 4.8f * PixMM * FontScaleFactor, SKTextAlign.Right, textFont, textPaint);
                }

                canvas.DrawRect(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[2] - bpAxisY[0]), (bpAxisX[1] - bpAxisX[0]) * preCoorWidth, preCoorHeight * (bpAxisY[2] - bpAxisY[0]),rectPaint);
                canvas.DrawText(CommonValues.Langs["血压水平偏低"], baseX + CoorPaddingLeft + 2* PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[2] - bpAxisY[0]) + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawRect(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[4] - bpAxisY[0]), (bpAxisX[3] - bpAxisX[0]) * preCoorWidth, preCoorHeight * (bpAxisY[4] - bpAxisY[0]), rectPaint);
                canvas.DrawText(CommonValues.Langs["血压水平正常"], baseX + CoorPaddingLeft + 2 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[4] - bpAxisY[0]) + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawRect(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[5] - bpAxisY[0]), (bpAxisX[4] - bpAxisX[0]) * preCoorWidth, preCoorHeight * (bpAxisY[5] - bpAxisY[0]), rectPaint);
                canvas.DrawText(CommonValues.Langs["正常高值血压"], baseX + CoorPaddingLeft + 2 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[5] - bpAxisY[0]) + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawRect(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[6] - bpAxisY[0]), (bpAxisX[5] - bpAxisX[0]) * preCoorWidth, preCoorHeight * (bpAxisY[6] - bpAxisY[0]), rectPaint);
                canvas.DrawText(CommonValues.Langs["轻度高血压(1级高血压)"], baseX + CoorPaddingLeft + 2 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[6] - bpAxisY[0]) + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawRect(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[7] - bpAxisY[0]), (bpAxisX[6] - bpAxisX[0]) * preCoorWidth, preCoorHeight * (bpAxisY[7] - bpAxisY[0]), rectPaint);
                canvas.DrawText(CommonValues.Langs["中度高血压(2级高血压)"], baseX + CoorPaddingLeft + 2 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * (bpAxisY[7] - bpAxisY[0]) + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["重度高血压(3级高血压)"], baseX + CoorPaddingLeft + 2 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);

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
                int sbpMax = 250;
                int dbpMax = 200;
                if (settingValues.unit == "kPa")
                {
                    sbpMax = 35;
                    dbpMax = 28;
                }
                float presbpPoint = CoorWidth / sbpMax;
                float predbpPoint = CoorHeight / dbpMax;

                float baseX = WaveLeft + PaddingLeft + CoorPaddingLeft;
                float baseY = WaveTop + CoorPaddingTop;
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

                    float x = baseX + (float)sbp * presbpPoint;
                    float y = baseY + CoorHeight - (float)dbp * predbpPoint;
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

                    float x = baseX + (float)sbp * presbpPoint;
                    float y = baseY + CoorHeight - (float)dbp * predbpPoint;
                    nightPoints.Add(new SKPoint(x, y));
                }
                pointPaint.Color = NightColor;
                canvas.DrawPoints(SKPointMode.Points, nightPoints.ToArray(), pointPaint);

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

                float x0 = baseX + CoorPaddingLeft + (float)lineX0 * presbpPoint;
                float y0 = baseY + CoorPaddingTop + CoorHeight - (float)lineY0 * predbpPoint;

                float x1 = baseX + CoorPaddingLeft + (float)lineX1 * presbpPoint;
                float y1 = baseY + CoorPaddingTop + CoorHeight - (float)lineY1 * predbpPoint;

                x0 = Math.Min(x0, baseX + CoorWidth);
                x1 = Math.Min(x1, baseX + CoorWidth);

                y0 = Math.Min(y0, baseY + CoorHeight);
                y1 = Math.Min(y1, baseY + CoorHeight);

                canvas.DrawLine(x0,y0,x1,y1,linePaint);
            }
        }
    }
}
