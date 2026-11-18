using SkiaSharp;
using System;
using System.Collections.Generic;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class DrawBpTrendWave
    {
        public float FontScaleFactor = 1;
        private float PixMM = SysConstants.PIX_SCALE;

        public float WaveLeft { get; set; } = 0;
        public float WaveTop { get; set; } = 0;
        public float WaveWidth { get; set; } = 0;
        public float WaveHeight { get; set; } = 0;

        public int MouseX = 0;
        public int MouseY = 0;
        public SKPoint[] MapValuePoints; //平均动脉压数值点的位置
        private int MinDistanceIdx = -1;

        public bpDiagnoseSettingValues settingValues;
        public List<BPDataInfo> BPDataInfoList = new List<BPDataInfo>();

        public DrawBpTrendWave()
        {

        }

        public void DrawCoordinate(SKCanvas canvas)
        {
            if (BPDataInfoList.Count <= 0) return;

            // 创建画笔
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColor.Parse("#313848"),
                StrokeWidth = 1.0f * FontScaleFactor,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
            })

            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint rectPaint = new SKPaint
            {
                Color = new SKColor(74,122,196,25),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            })
            using (SKFont labelFont = new SKFont
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
                string time = BPDataInfoList[0].Time;
                string hour = time.Split(':')[0];
                string minute = time.Split(':')[1];
                string unit = settingValues.unit;
                int[] bpAxis = new int[5] { 50, 100, 150, 200, 250 };
                if (unit == "kPa")
                {
                    bpAxis = new int[5] { 7, 14, 21, 28, 35 };
                }
                int[] RPPAxis = { 5, 10, 15, 20, 25 };
                int[] pluseAxis = { 48, 96, 144, 192, 240 };

                float preHourWidth = WaveWidth / 24.0f;
                float preHeight = WaveHeight / 5.0f;

                int startHour = Convert.ToInt32(hour);
                int nightStartHour = Convert.ToInt32(settingValues.daytimeEnd.Split(':')[0]);
                int nightEndHour = Convert.ToInt32(settingValues.daytimeStart.Split(':')[0]);

                DateTime startTime = DateTime.Parse(BPDataInfoList[0].Time);
                DateTime nightTimeStart = DateTime.Parse(settingValues.daytimeEnd);
                DateTime nightTimeEnd = DateTime.Parse(settingValues.daytimeStart);

                float bpMax = 250f;
                float maxSBPDay = 140;
                float minSBPDay = 90;
                float maxSBPNight = 125;
                float minSBPNight = 80;

                if (unit == "kPa")
                {
                    bpMax = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(bpMax), 2);
                    maxSBPDay = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(maxSBPDay), 2);
                    minSBPDay = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(minSBPDay), 2);
                    maxSBPNight = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(maxSBPNight), 2);
                    minSBPNight = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(minSBPNight), 2);
                }

                float bpPreHeight = WaveHeight / bpMax;


                // X轴
                canvas.DrawLine(WaveLeft, WaveTop + WaveHeight, WaveLeft + WaveWidth, WaveTop + WaveHeight, axisPaint);
                for (int i = 0; i < 25; i++)
                {
                    canvas.DrawLine(WaveLeft + i * preHourWidth, WaveTop + WaveHeight, WaveLeft + i * preHourWidth, WaveTop + WaveHeight + 1.5f * FontScaleFactor * PixMM, axisPaint);
                    if (i % 2 == 0)
                    {
                        string timeStr = ((Convert.ToInt32(hour) + i) % 24).ToString("D2") + ":" + minute;
                        canvas.DrawText(timeStr, WaveLeft + i * preHourWidth, WaveTop + WaveHeight + 4.0f * FontScaleFactor * PixMM, SKTextAlign.Center, textFont, textPaint);
                    }
                }

                // Y轴
                canvas.DrawLine(WaveLeft, WaveTop, WaveLeft, WaveTop + WaveHeight, axisPaint);
                canvas.DrawLine(WaveLeft + WaveWidth, WaveTop, WaveLeft + WaveWidth, WaveTop + WaveHeight, axisPaint);
                for (int i = 0; i < 5; i++)
                {
                    canvas.DrawLine(WaveLeft - 1.5f * FontScaleFactor * PixMM, WaveTop + WaveHeight - (i + 1) * preHeight, WaveLeft, WaveTop + WaveHeight - (i + 1) * preHeight, axisPaint);
                    canvas.DrawText(bpAxis[i].ToString(), WaveLeft - 2.0f * FontScaleFactor * PixMM, WaveTop + WaveHeight - (i + 1) * preHeight + 1.0f * FontScaleFactor * PixMM, SKTextAlign.Right, textFont, textPaint);

                    canvas.DrawLine(WaveLeft + WaveWidth - 1.5f * FontScaleFactor * PixMM, WaveTop + WaveHeight - (i + 1) * preHeight, WaveLeft + WaveWidth, WaveTop + WaveHeight - (i + 1) * preHeight, axisPaint);
                    canvas.DrawText(RPPAxis[i].ToString(), WaveLeft + WaveWidth - 2.0f * FontScaleFactor * PixMM, WaveTop + WaveHeight - (i + 1) * preHeight + 1.0f * FontScaleFactor * PixMM, SKTextAlign.Right, textFont, textPaint);

                    canvas.DrawLine(WaveLeft + WaveWidth + 1.5f * FontScaleFactor * PixMM, WaveTop + WaveHeight - (i + 1) * preHeight, WaveLeft + WaveWidth, WaveTop + WaveHeight - (i + 1) * preHeight, axisPaint);
                    canvas.DrawText(pluseAxis[i].ToString(), WaveLeft + WaveWidth + 2.0f * FontScaleFactor * PixMM, WaveTop + WaveHeight - (i + 1) * preHeight + 1.0f * FontScaleFactor * PixMM, SKTextAlign.Left, textFont, textPaint);
                }
                canvas.DrawText("血压(" + unit + ")", WaveLeft, WaveTop - 3.0f * FontScaleFactor * PixMM, SKTextAlign.Center, labelFont, textPaint);

                canvas.DrawText("RPP", WaveLeft + WaveWidth - 8.0f * FontScaleFactor * PixMM, WaveTop - 3.0f * FontScaleFactor * PixMM, SKTextAlign.Center, labelFont, textPaint);

                canvas.DrawText("脉率(次/分)", WaveLeft + WaveWidth + 6.0f * FontScaleFactor * PixMM, WaveTop - 3.0f * FontScaleFactor * PixMM, SKTextAlign.Center, labelFont, textPaint);
            }
        }

        public void DrawNightRect(SKCanvas canvas)
        {
            if (BPDataInfoList.Count <= 0) return;

            // 创建画笔
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColor.Parse("#313848"),
                StrokeWidth = 1.0f * FontScaleFactor,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
            })

            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint rectPaint = new SKPaint
            {
                Color = SKColor.Parse("#eef2f5"),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            })
            using (SKFont labelFont = new SKFont
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
                string time = BPDataInfoList[0].Time;
                string hour = time.Split(':')[0];
                string minute = time.Split(':')[1];
                string unit = settingValues.unit;
                int[] bpAxis = new int[5] { 50, 100, 150, 200, 250 };
                if (unit == "kPa")
                {
                    bpAxis = new int[5] { 7, 14, 21, 28, 35 };
                }
                int[] RPPAxis = { 5, 10, 15, 20, 25 };
                int[] pluseAxis = { 48, 96, 144, 192, 240 };

                float preHourWidth = WaveWidth / 24.0f;
                float preHeight = WaveHeight / 5.0f;

                int startHour = Convert.ToInt32(hour);
                int nightStartHour = Convert.ToInt32(settingValues.daytimeEnd.Split(':')[0]);
                int nightEndHour = Convert.ToInt32(settingValues.daytimeStart.Split(':')[0]);

                DateTime startTime = DateTime.Parse(BPDataInfoList[0].Time);
                DateTime nightTimeStart = DateTime.Parse(settingValues.daytimeEnd);
                DateTime nightTimeEnd = DateTime.Parse(settingValues.daytimeStart);

                float bpMax = 250f;
                float maxSBPDay = 140;
                float minSBPDay = 90;
                float maxSBPNight = 125;
                float minSBPNight = 80;

                if (unit == "kPa")
                {
                    bpMax = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(bpMax), 2);
                    maxSBPDay = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(maxSBPDay), 2);
                    minSBPDay = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(minSBPDay), 2);
                    maxSBPNight = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(maxSBPNight), 2);
                    minSBPNight = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(minSBPNight), 2);
                }

                float bpPreHeight = WaveHeight / bpMax;
                float maxSBPDayY = WaveTop + WaveHeight - bpPreHeight * maxSBPDay;
                float minSBPDayY = WaveTop + WaveHeight - bpPreHeight * minSBPDay;
                float maxSBPNightY = WaveTop + WaveHeight - bpPreHeight * maxSBPNight;
                float minSBPNightY = WaveTop + WaveHeight - bpPreHeight * minSBPNight;

                if (nightStartHour < startHour)
                {
                    double nightHours = Math.Abs((nightTimeEnd - startTime).TotalHours);
                    canvas.DrawRect(WaveLeft, WaveTop, (float)nightHours * preHourWidth, WaveHeight, rectPaint);

                    double nightEndStart = (startTime - nightTimeStart).TotalHours;
                    canvas.DrawRect(WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, WaveTop, (float)nightEndStart * preHourWidth, WaveHeight, rectPaint);
                }
                else
                {
                    double nightStart = (nightTimeStart - startTime).TotalHours;
                    double nightHours = 0;
                    if (nightEndHour > nightStartHour)
                    {
                        nightHours = Math.Abs((nightTimeEnd - nightTimeStart).TotalHours);
                    }
                    else
                    {
                        nightHours = Math.Abs((nightTimeEnd.AddHours(24) - nightTimeStart).TotalHours);
                    }
                    canvas.DrawRect(WaveLeft + (float)nightStart * preHourWidth, WaveTop, (float)nightHours * preHourWidth, WaveHeight, rectPaint);
                }
            }
        }

        public void DrawMaxMinLine(SKCanvas canvas)
        {
            using (SKPaint dashLinePaint = new SKPaint
            {
                Color = SKColor.Parse("#313848"),
                StrokeWidth = 1.0f * FontScaleFactor,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 2, 5 }, 0)
            })
            {
                string time = BPDataInfoList[0].Time;
                string hour = time.Split(':')[0];

                float preHourWidth = WaveWidth / 24.0f;
                float preHeight = WaveHeight / 5.0f;

                int startHour = Convert.ToInt32(hour);
                int nightStartHour = Convert.ToInt32(settingValues.daytimeEnd.Split(':')[0]);
                int nightEndHour = Convert.ToInt32(settingValues.daytimeStart.Split(':')[0]);

                DateTime startTime = DateTime.Parse(BPDataInfoList[0].Time);
                DateTime nightTimeStart = DateTime.Parse(settingValues.daytimeEnd);
                DateTime nightTimeEnd = DateTime.Parse(settingValues.daytimeStart);

                float bpMax = 250f;
                float maxSBPDay = 140;
                float minSBPDay = 90;
                float maxSBPNight = 125;
                float minSBPNight = 80;

                if (settingValues.unit == "kPa")
                {
                    bpMax = 35f;
                    maxSBPDay = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(maxSBPDay), 2);
                    minSBPDay = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(minSBPDay), 2);
                    maxSBPNight = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(maxSBPNight), 2);
                    minSBPNight = (float)CommonFuns.mmHg2KPa(Convert.ToInt32(minSBPNight), 2);
                }

                float bpPreHeight = WaveHeight / bpMax;
                float maxSBPDayY = WaveTop + WaveHeight - bpPreHeight * maxSBPDay;
                float minSBPDayY = WaveTop + WaveHeight - bpPreHeight * minSBPDay;
                float maxSBPNightY = WaveTop + WaveHeight - bpPreHeight * maxSBPNight;
                float minSBPNightY = WaveTop + WaveHeight - bpPreHeight * minSBPNight;

                if (nightStartHour < startHour)
                {
                    double nightHours = Math.Abs((nightTimeEnd - startTime).TotalHours);

                    double nightEndStart = (startTime - nightTimeStart).TotalHours;

                    canvas.DrawLine(WaveLeft, maxSBPNightY, WaveLeft + (float)nightHours * preHourWidth, maxSBPNightY, dashLinePaint);
                    canvas.DrawLine(WaveLeft, minSBPNightY, WaveLeft + (float)nightHours * preHourWidth, minSBPNightY, dashLinePaint);

                    canvas.DrawLine(WaveLeft + (float)nightHours * preHourWidth, maxSBPNightY, WaveLeft + (float)nightHours * preHourWidth, maxSBPDayY, dashLinePaint);
                    canvas.DrawLine(WaveLeft + (float)nightHours * preHourWidth, minSBPNightY, WaveLeft + (float)nightHours * preHourWidth, minSBPDayY, dashLinePaint);

                    canvas.DrawLine(WaveLeft + (float)nightHours * preHourWidth, maxSBPDayY, WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, maxSBPDayY, dashLinePaint);
                    canvas.DrawLine(WaveLeft + (float)nightHours * preHourWidth, minSBPDayY, WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, minSBPDayY, dashLinePaint);

                    canvas.DrawLine(WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, maxSBPDayY, WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, maxSBPNightY, dashLinePaint);
                    canvas.DrawLine(WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, minSBPDayY, WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, minSBPNightY, dashLinePaint);

                    canvas.DrawLine(WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, maxSBPNightY, WaveLeft + WaveWidth, maxSBPNightY, dashLinePaint);
                    canvas.DrawLine(WaveLeft + WaveWidth - (float)nightEndStart * preHourWidth, minSBPNightY, WaveLeft + WaveWidth, minSBPNightY, dashLinePaint);

                }
                else
                {
                    double nightStart = (nightTimeStart - startTime).TotalHours;
                    double nightHours = 0;
                    if (nightEndHour > nightStartHour)
                    {
                        nightHours = Math.Abs((nightTimeEnd - nightTimeStart).TotalHours);
                    }
                    else
                    {
                        nightHours = Math.Abs((nightTimeEnd.AddHours(24) - nightTimeStart).TotalHours);
                    }

                    canvas.DrawLine(WaveLeft, maxSBPDayY, WaveLeft + (float)nightStart * preHourWidth, maxSBPDayY, dashLinePaint);
                    canvas.DrawLine(WaveLeft, minSBPDayY, WaveLeft + (float)nightStart * preHourWidth, minSBPDayY, dashLinePaint);

                    canvas.DrawLine(WaveLeft + (float)nightStart * preHourWidth, maxSBPDayY, WaveLeft + (float)nightStart * preHourWidth, maxSBPNightY, dashLinePaint);
                    canvas.DrawLine(WaveLeft + (float)nightStart * preHourWidth, minSBPDayY, WaveLeft + (float)nightStart * preHourWidth, minSBPNightY, dashLinePaint);

                    canvas.DrawLine(WaveLeft + (float)nightStart * preHourWidth, maxSBPNightY, WaveLeft + (float)nightStart * preHourWidth + (float)nightHours * preHourWidth, maxSBPNightY, dashLinePaint);
                    canvas.DrawLine(WaveLeft + (float)nightStart * preHourWidth, minSBPNightY, WaveLeft + (float)nightStart * preHourWidth + (float)nightHours * preHourWidth, minSBPNightY, dashLinePaint);

                    canvas.DrawLine(WaveLeft + (float)nightStart * preHourWidth + (float)nightHours * preHourWidth, maxSBPDayY, WaveLeft + (float)nightStart * preHourWidth + (float)nightHours * preHourWidth, maxSBPNightY, dashLinePaint);
                    canvas.DrawLine(WaveLeft + (float)nightStart * preHourWidth + (float)nightHours * preHourWidth, minSBPDayY, WaveLeft + (float)nightStart * preHourWidth + (float)nightHours * preHourWidth, minSBPNightY, dashLinePaint);

                    canvas.DrawLine(WaveLeft + (float)nightStart * preHourWidth + (float)nightHours * preHourWidth, maxSBPDayY, WaveLeft + WaveWidth, maxSBPDayY, dashLinePaint);
                    canvas.DrawLine(WaveLeft + (float)nightStart * preHourWidth + (float)nightHours * preHourWidth, minSBPDayY, WaveLeft + WaveWidth, minSBPDayY, dashLinePaint);
                }
            }
        }

        public void DrawTrendWave(SKCanvas canvas)
        {
            if (BPDataInfoList.Count <= 0) return;

            // 创建画笔
            using (SKPaint bpLinePaint = new SKPaint
            {
                Color = SKColors.Blue,
                StrokeWidth = 1.0f * FontScaleFactor,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
            })
            using (SKPaint fillPaint = new SKPaint
            {
                Color = SKColor.Parse("#9CCAE2"),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            })
            using (SKPaint circlePaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            })
            using (SKPath avgPath = new SKPath())
            using (SKPaint avgWavePaint = new SKPaint
            {
                Color = SKColors.Yellow,
                StrokeWidth = 1.0f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            using (SKPath pulsePath = new SKPath())
            using (SKPaint pulseWavePaint = new SKPaint
            {
                Color = SKColors.Purple,
                StrokeWidth = 1.0f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            using (SKPath rppPath = new SKPath())
            using (SKPaint rppWavePaint = new SKPaint
            {
                Color = SKColors.DarkGreen,
                StrokeWidth = 1.0f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                float bpMax = 250f;
                DateTime startTime = BPDataInfoList[0].bpDateTime;
                float preHourWidth = WaveWidth / 24.0f;
                float preBpHeight = WaveHeight / bpMax;
                float prePulseHeight = WaveHeight / 240.0f;
                float preRppHeight = WaveHeight / 25.0f;

                float lastPosX = -1;
                float lastPosY1 = -1;
                float lastPosY2 = -1;
                for (int i = 0; i < BPDataInfoList.Count; i++)
                {
                    BPDataInfo bpData = BPDataInfoList[i];
                    DateTime time = bpData.bpDateTime;
                    float posX = WaveLeft + (float)((time - startTime).TotalHours) * preHourWidth;
                    float posAvgBpY = WaveTop + WaveHeight - (float)bpData.MAP * preBpHeight;
                    float posPulseY = WaveTop + WaveHeight - (float)bpData.Pulse * prePulseHeight;
                    float posRppY = WaveTop + WaveHeight - (float)bpData.RPP * preRppHeight;
                    if (posX > WaveLeft + WaveWidth)
                    {
                        continue;
                    }
                    if (BPTrendFlag.bBp)
                    {
                        float posBpY1 = WaveTop + WaveHeight - (float)bpData.SBP * preBpHeight;
                        float posBpY2 = WaveTop + WaveHeight - (float)bpData.DBP * preBpHeight;


                        if (lastPosX != -1)
                        {
                            SKPoint[] points = new SKPoint[]
                            {
                                new SKPoint(lastPosX, lastPosY1),
                                new SKPoint(posX, posBpY1),
                                new SKPoint(posX, posBpY2),
                                new SKPoint(lastPosX, lastPosY2)
                            };
                            using (var path = new SKPath())
                            {
                                path.MoveTo(points[0]);

                                // 连接到其他点
                                for (int j = 1; j < points.Length; j++)
                                {
                                    path.LineTo(points[j]);
                                }

                                // 闭合路径（回到起点）
                                path.Close();
                                canvas.DrawPath(path, fillPaint);
                            }
                        }
                        canvas.DrawLine(posX, posBpY1, posX, posBpY2, bpLinePaint);
                        canvas.DrawCircle(posX, posAvgBpY, 2, circlePaint);

                        lastPosX = posX;
                        lastPosY1 = posBpY1;
                        lastPosY2 = posBpY2;
                    }
                    if (BPTrendFlag.bAvgBp)
                    {
                        if (i == 0)
                        {
                            avgPath.MoveTo(posX, posAvgBpY);
                        }
                        else
                        {
                            avgPath.LineTo(posX, posAvgBpY);
                        }
                    }
                    if (BPTrendFlag.bPulse)
                    {
                        if (i == 0)
                        {
                            pulsePath.MoveTo(posX, posPulseY);
                        }
                        else
                        {
                            pulsePath.LineTo(posX, posPulseY);
                        }
                    }
                    if (BPTrendFlag.bRpp)
                    {
                        if (i == 0)
                        {
                            rppPath.MoveTo(posX, posRppY);
                        }
                        else
                        {
                            rppPath.LineTo(posX, posRppY);
                        }
                    }
                }

                if (BPTrendFlag.bAvgBp)
                {
                    canvas.DrawPath(avgPath, avgWavePaint);
                }
                if (BPTrendFlag.bPulse)
                {
                    canvas.DrawPath(pulsePath, pulseWavePaint);
                }
                if (BPTrendFlag.bRpp)
                {
                    canvas.DrawPath(rppPath, rppWavePaint);
                }

                //获取所有平均动脉压数值点位置用于后续读取血压信息位置
                int pointCount = avgPath.PointCount;
                MapValuePoints = new SKPoint[pointCount];
                int copied = avgPath.GetPoints(MapValuePoints, pointCount);
            }
        }

        public void DrawNibpTips(SKCanvas canvas, SKRect renderArea, int dataIndex)
        {
            if (BPDataInfoList.Count <= 0) return;

            DateTime startTime = BPDataInfoList[0].bpDateTime;
            float preHourWidth = WaveWidth / 24.0f;

            if (renderArea != SKRect.Empty)
            {
                if (MinDistanceIdx != dataIndex)
                {
                    BPDataInfo bpData = BPDataInfoList[dataIndex];
                    DateTime time = bpData.bpDateTime;
                    float posX = WaveLeft + (float)((time - startTime).TotalHours) * preHourWidth;
                    float rectLeft = posX;
                    //float rectTop = WaveTop + WaveHeight / 2;
                    float rectTop = renderArea.Top;
                    if (WaveLeft + WaveWidth - posX < 160)
                    {
                        rectLeft = posX - 140;
                    }
                    double bpPressure = bpData.SBP - bpData.DBP;
                    string sbpStr = "";
                    string dbpStr = "";
                    string mapStr = "";
                    string bpPressureStr = "";
                    if (settingValues.unit == "kPa")
                    {
                        sbpStr = CommonFuns.mmHg2KPa(Convert.ToInt32(bpData.SBP), 2).ToString("F2");
                        dbpStr = CommonFuns.mmHg2KPa(Convert.ToInt32(bpData.DBP), 2).ToString("F2");
                        mapStr = CommonFuns.mmHg2KPa(Convert.ToInt32(bpData.MAP), 2).ToString("F2");
                        bpPressureStr = CommonFuns.mmHg2KPa(Convert.ToInt32(bpPressure), 2).ToString("F2");
                    }
                    else
                    {
                        sbpStr = bpData.SBP.ToString();
                        dbpStr = bpData.DBP.ToString();
                        mapStr = bpData.MAP.ToString();
                        bpPressureStr = bpPressure.ToString();
                    }

                    using (SKPaint rectPaint = new SKPaint
                    {
                        Color = SKColor.Parse("#F5F5F5"),
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true,
                    })
                    using (SKPaint dashLinePaint = new SKPaint
                    {
                        Color = SKColor.Parse("#313848"),
                        StrokeWidth = 1.0f * FontScaleFactor,
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = true,
                        PathEffect = SKPathEffect.CreateDash(new float[] { 2, 5 }, 0)
                    })
                    using (SKPaint textPaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        StrokeWidth = 1.0f * FontScaleFactor,
                        IsAntialias = true
                    })
                    using (SKFont labelFont = new SKFont
                    {
                        Size = 12f * FontScaleFactor,
                        Typeface = SKTypeface.FromFamilyName("微软雅黑")
                    })
                    {
                        canvas.DrawRect(renderArea, rectPaint);
                        canvas.DrawText(bpData.bpDateTime.ToString(), rectLeft + 6, rectTop + 20, SKTextAlign.Left, labelFont, textPaint);
                        canvas.DrawText("收缩压: " + sbpStr + settingValues.unit, rectLeft + 6, rectTop + 40, SKTextAlign.Left, labelFont, textPaint);
                        canvas.DrawText("舒张压: " + dbpStr + settingValues.unit, rectLeft + 6, rectTop + 60, SKTextAlign.Left, labelFont, textPaint);
                        canvas.DrawText("平均动脉压: " + mapStr + settingValues.unit, rectLeft + 6, rectTop + 80, SKTextAlign.Left, labelFont, textPaint);
                        canvas.DrawText("脉率: " + bpData.Pulse + "bpm", rectLeft + 6, rectTop + 100, SKTextAlign.Left, labelFont, textPaint);
                        canvas.DrawText("RPP: " + bpData.RPP.ToString("F2"), rectLeft + 6, rectTop + 120, SKTextAlign.Left, labelFont, textPaint);
                        canvas.DrawText("脉压差: " + bpPressureStr + settingValues.unit, rectLeft + 6, rectTop + 140, SKTextAlign.Left, labelFont, textPaint);

                        canvas.DrawLine(posX, WaveTop, posX, WaveTop + WaveHeight, dashLinePaint);
                    }
                }
            }
        }

        /// <summary>
        /// 获取鼠标当前光标对应的血压信息提示显示区域
        /// </summary>
        public SKRect GetCursorAssociatedTipArea(SKPoint[] bpPoints, out int dataIndex)
        {
            SKRect rect = SKRect.Empty;
            dataIndex = -1;

            if (BPDataInfoList.Count > 0)
            {
                DateTime startTime = BPDataInfoList[0].bpDateTime;
                float preHourWidth = WaveWidth / 24.0f;

                if ((MouseX >= WaveLeft && MouseX <= WaveLeft + WaveWidth)
                   && (MouseY >= WaveTop && MouseY <= WaveTop + WaveHeight))
                {
                    float minDistance = -1;
                    dataIndex = 0;

                    for (int i = 0; i < bpPoints.Length; i++)
                    {
                        SKPoint p = bpPoints[i];
                        if (i == 0)
                        {
                            minDistance = Math.Abs(p.X - MouseX);
                        }
                        else
                        {
                            if (Math.Abs(p.X - MouseX) < minDistance)
                            {
                                minDistance = Math.Abs(p.X - MouseX);
                                dataIndex = i;
                            }
                        }
                    }

                    if (MinDistanceIdx != dataIndex)
                    {
                        float rectWidth = 140, rectHeight = 160;
                        BPDataInfo bpData = BPDataInfoList[dataIndex];
                        DateTime time = bpData.bpDateTime;
                        float posX = WaveLeft + (float)((time - startTime).TotalHours) * preHourWidth;
                        float rectLeft = posX;
                        float rectTop = MouseY;
                        if (WaveLeft + WaveWidth - posX < 160)
                        {
                            rectLeft = posX - rectWidth;
                        }
                        if (rectTop <= WaveTop - rectHeight) rectTop = WaveTop;
                        if (rectTop >= WaveTop + WaveHeight - rectHeight) rectTop = WaveTop + WaveHeight - rectHeight;

                        rect = new SKRect(rectLeft, rectTop, rectLeft + rectWidth, rectTop + rectHeight);
                    }
                }
            }

            return rect;
        }

    }
}
