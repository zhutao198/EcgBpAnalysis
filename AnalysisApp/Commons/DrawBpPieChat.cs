using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class DrawBpPieChat
    {
        public float WaveLeft { get; set; } = 0;
        public float WaveTop { get; set; } = 0;
        public float WaveWidth { get; set; } = 0;
        public float WaveHeight { get; set; } = 0;
        public readonly float PixMM = 3.777f;

        private float PaddingLeft = 0;
        private float PaddingTop = 2 * 3.777f;

        public float FontScaleFactor = 1;

        public bpDiagnoseSettingValues settingValues;

        public List<BPDataInfo> BPDataInfoList = new List<BPDataInfo>();

        public string SBPMinStr = "";
        public string SBPMaxStr = "";
        public string DBPMinStr = "";
        public string DBPMaxStr = "";
        public string PulseMinStr = "";
        public string PulseMaxStr = "";

        public List<int> sbpMaxMin = new List<int>(3);
        public List<Double> sbpMaxMinPercent = new List<Double>(3);

        public List<int> dbpMaxMin = new List<int>(3);
        public List<Double> dbpMaxMinPercent = new List<Double>(3);

        public List<int> pulseMaxMin = new List<int>(3);
        public List<Double> pulseMaxMinPercent = new List<Double>(3);

        private float RectHeight = 0;

        public bool ShowMinMaxValue = false;

        public DrawBpPieChat()
        {
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            if (settingValues.unit == "mmHg")
            {
                SBPMinStr = settingValues.minValidSys.ToString();
                SBPMaxStr = settingValues.maxValidSys.ToString();
                DBPMinStr = settingValues.minValidDia.ToString();
                DBPMaxStr = settingValues.maxValidDia.ToString();
            }
            else
            {
                SBPMinStr = CommonFuns.mmHg2KPa(settingValues.minValidSys, 2).ToString("F2");
                SBPMaxStr = CommonFuns.mmHg2KPa(settingValues.maxValidSys, 2).ToString("F2");
                DBPMinStr = CommonFuns.mmHg2KPa(settingValues.minValidDia, 2).ToString("F2");
                DBPMaxStr = CommonFuns.mmHg2KPa(settingValues.maxValidDia, 2).ToString("F2");
            }

            PulseMinStr = "60";
            PulseMaxStr = "100";
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
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            using (SKFont TextFont = new SKFont
            {
                Size = 12f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                if (BPDataInfoList.Count == 0) return;
                double sbpMax = 0;
                double dbpMax = 0;
                int pulseMax = 0;
                double sbpMin = 0;
                double dbpMin = 0;
                int pulseMin = 0;
                double sbpSum = 0;
                double dbpSum = 0;
                int pulseSum = 0;
                for (int i = 0; i < BPDataInfoList.Count; i++)
                {
                   
                    sbpMax = Math.Max(BPDataInfoList[i].SBP, sbpMax);
                    dbpMax = Math.Max(BPDataInfoList[i].DBP, dbpMax);
                    pulseMax = Math.Max(BPDataInfoList[i].Pulse, pulseMax);

                    sbpSum += BPDataInfoList[i].SBP;
                    dbpSum += BPDataInfoList[i].DBP;
                    pulseSum += BPDataInfoList[i].Pulse;

                    if(i == 0)
                    {
                        sbpMin = BPDataInfoList[i].SBP;
                        dbpMin = BPDataInfoList[i].DBP;
                        pulseMin = BPDataInfoList[i].Pulse;
                    }
                    else
                    {
                        sbpMin = Math.Min(BPDataInfoList[i].SBP, sbpMin);
                        dbpMin = Math.Min(BPDataInfoList[i].DBP, dbpMin);
                        pulseMin = Math.Min(BPDataInfoList[i].Pulse, pulseMin);
                    }
                }

                string sbpMaxTime = BPDataInfoList.FirstOrDefault(b => b.SBP == sbpMax).bpDateTime.ToString("yyyy-MM-dd HH:mm");
                string dbpMaxTime = BPDataInfoList.FirstOrDefault(b => b.DBP == dbpMax).bpDateTime.ToString("yyyy-MM-dd HH:mm");
                string pulseMaxTime = BPDataInfoList.FirstOrDefault(b => b.Pulse == pulseMax).bpDateTime.ToString("yyyy-MM-dd HH:mm");

                string sbpMinTime = BPDataInfoList.FirstOrDefault(b => b.SBP == sbpMin).bpDateTime.ToString("yyyy-MM-dd HH:mm");
                string dbpMinTime = BPDataInfoList.FirstOrDefault(b => b.DBP == dbpMin).bpDateTime.ToString("yyyy-MM-dd HH:mm");
                string pulseMinTime = BPDataInfoList.FirstOrDefault(b => b.Pulse == pulseMin).bpDateTime.ToString("yyyy-MM-dd HH:mm");

                double sbpAvg = sbpSum / BPDataInfoList.Count;
                double dbpAvg = dbpSum / BPDataInfoList.Count;
                int pulseAvg = pulseSum / BPDataInfoList.Count;

                string sbpMinStr = "";
                string dbpMinStr = "";
                string sbpMaxStr = "";
                string dbpMaxStr = "";
                string sbpAvgStr = "";
                string dbpAvgStr = "";

                if (settingValues.unit == "mmHg")
                {
                    sbpMinStr = sbpMin.ToString();
                    dbpMinStr = dbpMin.ToString();
                    sbpMaxStr = sbpMax.ToString();
                    dbpMaxStr = dbpMax.ToString();
                    sbpAvgStr = Math.Round(sbpAvg, 0).ToString();
                    dbpAvgStr = Math.Round(dbpAvg, 0).ToString();
                }
                else
                {
                    sbpMinStr = CommonFuns.mmHg2KPa(Convert.ToInt32(sbpMin), 2).ToString("F2");
                    dbpMinStr = CommonFuns.mmHg2KPa(Convert.ToInt32(dbpMin), 2).ToString("F2");
                    sbpMaxStr = CommonFuns.mmHg2KPa(Convert.ToInt32(sbpMax), 2).ToString("F2");
                    dbpMaxStr = CommonFuns.mmHg2KPa(Convert.ToInt32(dbpMax), 2).ToString("F2");
                    sbpAvgStr = CommonFuns.mmHg2KPa(Convert.ToInt32(sbpAvg), 2).ToString("F2");
                    dbpAvgStr = CommonFuns.mmHg2KPa(Convert.ToInt32(dbpAvg), 2).ToString("F2");
                }

                RectHeight = (WaveHeight - PaddingTop * 3) / 3;

                for (int i = 0; i < 3; i++)
                {
                    float baseX = WaveLeft + PaddingLeft;
                    float baseY = WaveTop + i * (PaddingTop + RectHeight);

                    canvas.DrawRect(baseX, baseY, WaveWidth, RectHeight, RectPaint);

                    if (i == 0)
                    {
                        canvas.DrawText("收缩压" + "(" + settingValues.unit + ")", baseX + 2*PixMM*FontScaleFactor, baseY + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, TitleFont, TextPaint);

                        canvas.DrawText("最大值" + ":" + sbpMaxStr + "("+sbpMaxTime+")", baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 4 * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                        canvas.DrawText("最小值" + ":" + sbpMinStr + "(" + sbpMinTime + ")", baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 9.5f * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                        canvas.DrawText("平均值" + ":" + sbpAvgStr, baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 14.5f * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                    }
                    else if (i == 1)
                    {
                        canvas.DrawText("舒张压"+"("+ settingValues.unit + ")", baseX + 2 * PixMM * FontScaleFactor, baseY + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, TitleFont, TextPaint);

                        canvas.DrawText("最大值" + ":" + dbpMaxStr + "(" + dbpMaxTime + ")", baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 4 * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                        canvas.DrawText("最小值" + ":" + dbpMinStr + "(" + dbpMinTime + ")", baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 9.5f * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                        canvas.DrawText("平均值" + ":" + dbpAvgStr, baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 14.5f * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                    }
                    else
                    {
                        canvas.DrawText("脉率(次/分)", baseX + 2 * PixMM * FontScaleFactor, baseY + 4 * PixMM * FontScaleFactor, SKTextAlign.Left, TitleFont, TextPaint);

                        canvas.DrawText("最大值" + ":" + pulseMax.ToString() + "(" + pulseMaxTime + ")", baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 4 * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                        canvas.DrawText("最小值" + ":" + pulseMin.ToString() + "(" + pulseMinTime + ")", baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 9.5f * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                        canvas.DrawText("平均值" + ":" + pulseAvg.ToString(), baseX + WaveWidth - 2 * PixMM * FontScaleFactor, baseY + 14.5f * PixMM * FontScaleFactor, SKTextAlign.Right, TextFont, TextPaint);
                    }
                }

            }
        }

        public void DrawPieChat(SKCanvas canvas)
        {
            using (SKPaint LinePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.8f * FontScaleFactor,
            })
            using (SKPaint RectPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            })
            using (SKPaint TextPaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            using (SKFont TextFont = new SKFont
            {
                Size = 12f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                SKColor minColor = SKColor.Parse("#5470C6");
                SKColor minMaxColor = SKColor.Parse("#3B95C5");
                SKColor maxColor = SKColor.Parse("#FFB000");
                SKColor[] colors = new SKColor[]
                {
                    minColor,
                    minMaxColor,
                    maxColor
                };
                if (BPDataInfoList.Count == 0) return;

                for (int i = 0; i < 3; i++)
                {
                    float baseX = WaveLeft + PaddingLeft;
                    float baseY = WaveTop + i * (PaddingTop + RectHeight);
                    float baseRectY = WaveTop + (i+0.5f) * (PaddingTop + RectHeight);

                    RectPaint.Color = minColor;
                    canvas.DrawRect(baseX + 2 * PixMM * FontScaleFactor, baseRectY - 21* PixMM * FontScaleFactor, 8 * PixMM * FontScaleFactor, 4 * PixMM * FontScaleFactor, RectPaint);
                    RectPaint.Color = minMaxColor;
                    canvas.DrawRect(baseX + 2 * PixMM * FontScaleFactor, baseRectY - 14.8f * PixMM * FontScaleFactor, 8 * PixMM * FontScaleFactor, 4 * PixMM * FontScaleFactor, RectPaint);
                    RectPaint.Color = maxColor;
                    canvas.DrawRect(baseX + 2 * PixMM * FontScaleFactor, baseRectY - 8.5f * PixMM * FontScaleFactor, 8 * PixMM * FontScaleFactor, 4 * PixMM * FontScaleFactor, RectPaint);
                    if (i ==0)
                    {
                        if(ShowMinMaxValue)
                        {
                            canvas.DrawText("<" + SBPMinStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 18 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText(SBPMinStr + "-" + SBPMaxStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 12 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText(">" + SBPMaxStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 5.3f * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                        }
                        else
                        {
                            canvas.DrawText("偏低", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 18 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText("正常", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 12 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText("偏高", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 5.3f * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                        }

                        int[] counts = sbpMaxMin.ToArray();
                        double[] percents = sbpMaxMinPercent.ToArray();

                        DrawSectorsWithLabels(canvas, baseX + WaveWidth / 2, baseY + RectHeight / 2, (RectHeight - 21 * PixMM * FontScaleFactor) / 2, counts, percents, colors, RectPaint, TextFont, TextPaint, LinePaint);

                    }
                    else if(i == 1)
                    {
                        if (ShowMinMaxValue)
                        {
                            canvas.DrawText("<" + DBPMinStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 18 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText(DBPMinStr + "-" + DBPMaxStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 12 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText(">" + DBPMaxStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 5.3f * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                        }
                        else
                        {
                            canvas.DrawText("偏低", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 18 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText("正常", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 12 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText("偏高", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 5.3f * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                        }

                        int[] counts = dbpMaxMin.ToArray();
                        double[] percents = dbpMaxMinPercent.ToArray();

                        DrawSectorsWithLabels(canvas, baseX + WaveWidth / 2, baseY + RectHeight / 2, (RectHeight - 21 * PixMM * FontScaleFactor) / 2, counts, percents, colors, RectPaint, TextFont, TextPaint, LinePaint);

                    }
                    else
                    {
                        if (ShowMinMaxValue)
                        {
                            canvas.DrawText("<" + PulseMinStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 18 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText(PulseMinStr + "-" + PulseMaxStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 12 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText(">" + PulseMaxStr, baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 5.3f * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                        }
                        else
                        {
                            canvas.DrawText("偏低", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 18 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText("正常", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 12 * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                            canvas.DrawText("偏高", baseX + 10.5f * PixMM * FontScaleFactor, baseRectY - 5.3f * PixMM * FontScaleFactor, SKTextAlign.Left, TextFont, TextPaint);
                        }

                        int[] counts = pulseMaxMin.ToArray();
                        double[] percents = pulseMaxMinPercent.ToArray();
                        DrawSectorsWithLabels(canvas, baseX + WaveWidth / 2, baseY + RectHeight / 2, (RectHeight - 21 * PixMM * FontScaleFactor) / 2, counts, percents, colors, RectPaint, TextFont, TextPaint, LinePaint);
                    }
                }
            }
        }

        private void DrawSectorsWithLabels(SKCanvas canvas, float cx, float cy, float radius, int[] counts, double[] percents, SKColor[] colors, SKPaint paint, SKFont textFont, SKPaint textPaint, SKPaint linePaint)
        {
            SKRect rect = new SKRect(cx - radius, cy - radius, cx + radius, cy + radius);
            float startAngle = -90f;
            float labelRadius = radius + 30f;
            // 引导线参数
            float outerRadius = radius;             // 引导线起点半径（紧贴扇形边缘）
            float bendRadius = radius + 20f;        // 折点半径（拐弯位置）
            float horizontalLength = 20f;           // 水平段长度

            for (int i=0; i<percents.Length; i++)
            {
                float sweepAngle = (float)percents[i] * 360f;
                paint.Color = colors[i];
                canvas.DrawArc(rect, startAngle, sweepAngle, true, paint);

                string label = counts[i].ToString() + "("+ Math.Round(percents[i] * 100, 2).ToString("F1")+"%)";

                float midAngle = startAngle + sweepAngle / 2;

                float rad = midAngle * (float)Math.PI / 180;

                // ===== 1. 计算三个关键点 =====

                // 起点：扇形边缘外侧
                float startX = cx + outerRadius * (float)Math.Cos(rad);
                float startY = cy + outerRadius * (float)Math.Sin(rad);

                // 折点：向外延伸的中间点
                float bendX = cx + bendRadius * (float)Math.Cos(rad);
                float bendY = cy + bendRadius * (float)Math.Sin(rad);

                // 终点：水平延伸出去
                bool isRightSide = Math.Cos(rad) >= 0;
                float endX = isRightSide ? bendX + horizontalLength : bendX - horizontalLength;
                float endY = bendY;

                linePaint.Color = colors[i];
                // ===== 2. 绘制折线引导 =====
                using (var path = new SKPath())
                {
                    path.MoveTo(startX, startY);
                    path.LineTo(bendX, bendY);
                    path.LineTo(endX, endY);
                    canvas.DrawPath(path, linePaint);
                }

                // ===== 3. 绘制标签文字 =====

                if (isRightSide)
                {
                    // 标签在右侧，左对齐，文字起点在水平线末端 + 小间距
                    canvas.DrawText(label, endX + 1.3f * PixMM * FontScaleFactor, endY + 1.3f * PixMM * FontScaleFactor, SKTextAlign.Left, textFont, textPaint);
                }
                else
                {
                    // 标签在左侧，右对齐，文字终点在水平线末端 - 小间距
                    canvas.DrawText(label, endX - 1.3f * PixMM * FontScaleFactor, endY + 1.3f * PixMM * FontScaleFactor, SKTextAlign.Right, textFont, textPaint);
                }

                startAngle += sweepAngle;
            }
        }

    }
}
