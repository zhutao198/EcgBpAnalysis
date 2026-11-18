using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class DrawBpHistogramWave
    {
        private float PixMM = 3.777f;

        public float WaveLeft { get; set; } = 0;
        public float WaveTop { get; set; } = 0;
        public float WaveWidth { get; set; } = 0;
        public float WaveHeight { get; set; } = 0;

        private float PaddingLeft = 0;
        private float PaddingTop = 2 * 3.777f;

        public float FontScaleFactor = 1;

        public bpDiagnoseSettingValues settingValues;

        public List<BPDataInfo> BPDataInfoList = new List<BPDataInfo>();

        //int PreValueSBP = 0;
        //int PreValueDBP = 0;
        //int PreValuePulse = 0;

        float CoorPaddingLeft = 0;
        float CoorPaddingRight = 0;
        float CoorPaddingTop = 0;
        float CoorPaddingBottom = 0;
        float CoorHeight = 0;
        float CoorWidth = 0;
        float RectHeight = 0;

        float MaxValueX = 0;          //水平方向坐标最大值
        float PreValue = 10;

        int SBPRectCount = 0;
        int DBPRectCount = 0;
        int PulseRectCount = 0;

        public DrawBpHistogramWave()
        {

        }

        public void DrawCoordinate(SKCanvas canvas)
        {
            using (SKPaint LinePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,
            })
            using (SKPaint DashLinePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.6f * FontScaleFactor,
                PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
            })
            using (SKPaint RectPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,
                Color = SKColor.Parse("#E4E9ED"),
            })
            using (SKPaint TextPaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            using (SKFont TitleFont = new SKFont
            {
                Size = 12f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            using (SKFont TextFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                //int SBPMax = 0;
                //int DBPMax = 0;
                //int PulseMax = 0;

                //for (int i = 0; i < BPDataInfoList.Count; i++)
                //{
                //    SBPMax = Math.Max(BPDataInfoList[i].SBP, SBPMax);
                //    DBPMax = Math.Max(BPDataInfoList[i].DBP, DBPMax);
                //    PulseMax = Math.Max(BPDataInfoList[i].Pulse, PulseMax);
                //}
                //PreValueSBP = ClosestDivisibleBy9(SBPMax) / 9;
                //PreValueDBP = ClosestDivisibleBy9(DBPMax) / 9;
                //PreValuePulse = ClosestDivisibleBy9(PulseMax) / 9;
                int rectCount = 0;

                RectHeight = (WaveHeight - PaddingTop * 3 * FontScaleFactor) / 3;
                CoorPaddingLeft = 10 * PixMM * FontScaleFactor;
                CoorPaddingRight = 15 * PixMM * FontScaleFactor;
                CoorPaddingTop = 10 * PixMM * FontScaleFactor;
                CoorPaddingBottom = 5 * PixMM * FontScaleFactor;
                CoorHeight = RectHeight - CoorPaddingTop - CoorPaddingBottom;
                CoorWidth = WaveWidth - CoorPaddingLeft - CoorPaddingRight;
                float preCoorHeight = CoorHeight / 10;
                //float preCoorWidth = CoorWidth / 10;
                for (int i = 0; i < 3; i++)
                {
                    float baseX = WaveLeft + PaddingLeft;
                    float baseY = WaveTop + i * (PaddingTop + RectHeight);
                    if (FontScaleFactor == 1)
                    {
                        canvas.DrawRect(baseX, baseY, WaveWidth, RectHeight, RectPaint);
                    }
                    canvas.DrawLine(baseX + CoorPaddingLeft, baseY + CoorPaddingTop, baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight, LinePaint);
                    canvas.DrawLine(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + CoorHeight, baseX + CoorPaddingLeft + CoorWidth, baseY + CoorPaddingTop + CoorHeight, LinePaint);
                    if (i == 0)
                    {

                        canvas.DrawText("收缩压(%)", baseX + 6, baseY + 6*PixMM*FontScaleFactor, SKTextAlign.Left, TitleFont, TextPaint);
                        canvas.DrawText(settingValues.unit, baseX + CoorPaddingLeft + CoorWidth + 3 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight + 16, SKTextAlign.Left, TextFont, TextPaint);

                        MaxValueX = 250;
                        rectCount = 25;
                        SBPRectCount = 25;
                    }
                    else if (i == 1)
                    {
                        canvas.DrawText("舒张压(%)", baseX + 6, baseY + 6 * PixMM * FontScaleFactor, SKTextAlign.Left, TitleFont, TextPaint);
                        canvas.DrawText(settingValues.unit, baseX + CoorPaddingLeft + CoorWidth + 3 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight + 12, SKTextAlign.Left, TextFont, TextPaint);

                        MaxValueX = 200;
                        rectCount = 20;
                        DBPRectCount = 20;
                    }
                    else
                    {
                        canvas.DrawText("脉率(%)", baseX + 6, baseY + 6 * PixMM * FontScaleFactor, SKTextAlign.Left, TitleFont, TextPaint);
                        canvas.DrawText("次/分", baseX + CoorPaddingLeft + CoorWidth + 3 * PixMM * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight + 12, SKTextAlign.Left, TextFont, TextPaint);

                        MaxValueX = 250;
                        rectCount = 25;
                        PulseRectCount = 25;

                    }
                    if (settingValues.unit == "kPa" && i<2)
                    {
                        MaxValueX = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(MaxValueX), 1);
                    }
                    for (int j=0; j<10; j++)
                    {
                        if(j % 2 == 0)
                        {
                            canvas.DrawLine(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + preCoorHeight * j, baseX + CoorPaddingLeft + CoorWidth, baseY + CoorPaddingTop + preCoorHeight * j, DashLinePaint);
                            canvas.DrawText((100 - j * 10).ToString(), baseX + CoorPaddingLeft - 6*FontScaleFactor, baseY + CoorPaddingTop + preCoorHeight * j + 6 * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                        }
                        else
                        {
                            canvas.DrawLine(baseX + CoorPaddingLeft, baseY + CoorPaddingTop + preCoorHeight * j, baseX + CoorPaddingLeft + 3 * FontScaleFactor, baseY + CoorPaddingTop + preCoorHeight * j, LinePaint);
                        }

                    }
                    canvas.DrawText("0", baseX + CoorPaddingLeft - 6 * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight + 6 * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                    float preCoorWidth = CoorWidth / rectCount;
                    for (int j=0; j <= rectCount; j++)
                    {
                        if(j%5 == 0)
                        {
                            if(j > 0)
                            {
                                canvas.DrawLine(baseX + CoorPaddingLeft + j * preCoorWidth, baseY + CoorPaddingTop, baseX + CoorPaddingLeft + j * preCoorWidth, baseY + CoorPaddingTop + CoorHeight, DashLinePaint);
                            }
                            string coorText = "";
                            if(i<2 && j>0)
                            {
                                if(settingValues.unit == "kPa")
                                {
                                    coorText = Math.Round(MaxValueX / rectCount * j, 1).ToString("F1");
                                }
                                else
                                {
                                    coorText = Math.Round(MaxValueX / rectCount * j, 0).ToString();
                                }
                            }
                            else
                            {
                                coorText = Math.Round(MaxValueX / rectCount * j, 0).ToString();
                            }
                            canvas.DrawText(coorText, baseX + CoorPaddingLeft + j * preCoorWidth + 6 * FontScaleFactor, baseY + CoorPaddingTop + CoorHeight + 12 * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                        }
                        canvas.DrawLine(baseX + CoorPaddingLeft + j * preCoorWidth, baseY + CoorPaddingTop + CoorHeight, baseX + CoorPaddingLeft + j * preCoorWidth, baseY + CoorPaddingTop + CoorHeight + 3 * FontScaleFactor, LinePaint);

                    }
                }

            }
        }

        public void DrawHistogramWave(SKCanvas canvas)
        {
            using (SKPaint RectPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 1.0f * FontScaleFactor,
                Color = SKColor.Parse("#3B95C5"),
            })
            using (SKPaint TextPaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            using (SKFont TextFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                float preCoorHeight = CoorHeight / 100;
                float baseX = WaveLeft + PaddingLeft + CoorPaddingLeft;
                for (int i = 0; i < SBPRectCount; i++)
                {
                    float preCoorWidth = CoorWidth / SBPRectCount;

                    int count = BPDataInfoList.Count(x => x.SBP > i * PreValue && x.SBP <= (i + 1) * PreValue);
                    float SBPPrecent = (float)count / BPDataInfoList.Count * 100;
                    if (SBPPrecent > 0)
                    {
                        float baseY = WaveTop;
                        canvas.DrawRect(baseX + (i + 0.2f) * preCoorWidth, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * SBPPrecent, preCoorWidth * 0.6f, preCoorHeight * SBPPrecent, RectPaint);
                        canvas.DrawText(SBPPrecent.ToString("F1"), baseX + (i + 0.5f) * preCoorWidth, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * SBPPrecent - 6, SKTextAlign.Center, TextFont, TextPaint);
                    }
                }

                for(int i = 0; i<DBPRectCount; i++)
                {
                    float preCoorWidth = CoorWidth / DBPRectCount;

                    int count = BPDataInfoList.Count(x => x.DBP > i * PreValue && x.DBP <= (i + 1) * PreValue);
                    float DBPPrecent = (float)count / BPDataInfoList.Count * 100;
                    if (DBPPrecent > 0)
                    {
                        float baseY = WaveTop + PaddingTop + RectHeight;
                        canvas.DrawRect(baseX + (i + 0.2f) * preCoorWidth, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * DBPPrecent, preCoorWidth * 0.6f, preCoorHeight * DBPPrecent, RectPaint);
                        canvas.DrawText(DBPPrecent.ToString("F1"), baseX + (i + 0.5f) * preCoorWidth, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * DBPPrecent - 6, SKTextAlign.Center, TextFont, TextPaint);
                    }
                }

                for(int i = 0; i<PulseRectCount; i++)
                {
                    float preCoorWidth = CoorWidth / PulseRectCount;

                    int count = BPDataInfoList.Count(x => x.Pulse > i * PreValue && x.Pulse <= (i + 1) * PreValue);
                    float PulsePrecent = (float)count / BPDataInfoList.Count * 100;
                    if (PulsePrecent > 0)
                    {
                        float baseY = WaveTop + 2 * (PaddingTop + RectHeight);
                        canvas.DrawRect(baseX + (i + 0.2f) * preCoorWidth, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * PulsePrecent, preCoorWidth * 0.6f, preCoorHeight * PulsePrecent, RectPaint);
                        canvas.DrawText(PulsePrecent.ToString("F1"), baseX + (i + 0.5f) * preCoorWidth, baseY + CoorPaddingTop + CoorHeight - preCoorHeight * PulsePrecent - 6, SKTextAlign.Center, TextFont, TextPaint);
                    }
                }

            }
        }

        /// <summary>
        /// 计算最接近能被 9 整除的数
        /// </summary>
        public static int ClosestDivisibleBy9(double value)
        {
            int quotient = (int)value / 9;
            int remainder = (int)value % 9;

            // 余数 >= 5 则向上取整，否则向下取整
            return remainder >= 5 ? (quotient + 1) * 9 : quotient * 9;
        }
    }
}
