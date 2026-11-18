using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class DrawHourList
    {
        public float marginTop = 0;
        public float marginLeft = 0;
        public float width = 0;
        public float height = 0;

        // DPI缩放因子
        public float DpiScaleFactor = 1.0f;

        public DrawHourList() { }

        public DrawHourList(float dpiScaleFactor)
        {
            DpiScaleFactor = dpiScaleFactor;
        }

        public readonly Dictionary<int, string> keyMap = new Dictionary<int, string>
        {
            {0, "Time" },
            {1, "TotalBeat" }
        };

        public void PaintHourList(SKCanvas canvas, List<HourlistInfo> hourlistInfo)
        {
            if (hourlistInfo.Count == 0) return;
            using (SKPaint tablePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.4f * DpiScaleFactor,
            })
            using (SKFont textFont = new SKFont
            {
                Size = 9f * DpiScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                float timeColWidth = 60f * DpiScaleFactor;
                int col = 18;
                int row = hourlistInfo.Count + 2;
                float cellWidth = (width - timeColWidth) / (col - 1);
                float cellHeight = height / 26;


                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + cellHeight*row, tablePaint);
                canvas.DrawLine(marginLeft, marginTop, marginLeft + width, marginTop, tablePaint);
                canvas.DrawLine(marginLeft + width, marginTop, marginLeft + width, marginTop + cellHeight * row, tablePaint);
                canvas.DrawLine(marginLeft, marginTop + cellHeight * row, marginLeft + width, marginTop + cellHeight * row, tablePaint);


                canvas.DrawText(CommonValues.Langs["时间"], marginLeft + 0.5f * timeColWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["总心搏"], marginLeft + timeColWidth + 0.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["心率"], marginLeft + timeColWidth + 2.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["最慢"], marginLeft + timeColWidth + 1.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["平均"], marginLeft + timeColWidth + 2.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["最快"], marginLeft + timeColWidth + 3.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["单发"], marginLeft + timeColWidth + 4.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["成对"], marginLeft + timeColWidth + 5.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["二联律"], marginLeft + timeColWidth + 6.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["三联律"], marginLeft + timeColWidth + 7.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["室速"], marginLeft + timeColWidth + 8.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["总数"], marginLeft + timeColWidth + 9.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);

                canvas.DrawText(CommonValues.Langs["室性早搏"], marginLeft + timeColWidth + 7f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);

                canvas.DrawText(CommonValues.Langs["单发"], marginLeft + timeColWidth + 10.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["成对"], marginLeft + timeColWidth + 11.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["二联律"], marginLeft + timeColWidth + 12.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["三联律"], marginLeft + timeColWidth + 13.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["房速"], marginLeft + timeColWidth + 14.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["总数"], marginLeft + timeColWidth + 15.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);


                canvas.DrawText(CommonValues.Langs["房性早搏"], marginLeft + timeColWidth + 13f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["停搏"], marginLeft + timeColWidth + 16.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);


                for (int i=0; i< row-1; i++)
                {
                    if(i == 0)
                    {
                        canvas.DrawLine(marginLeft + timeColWidth + cellWidth, marginTop+cellHeight, marginLeft + width - cellWidth, marginTop + cellHeight, tablePaint);
                    }
                    else
                    {
                        canvas.DrawLine(marginLeft, marginTop + cellHeight*(i+1), marginLeft + width, marginTop + cellHeight * (i + 1), tablePaint);

                    }
                }

                for(int i=0; i<col-1; i++)
                {
                    if(i == 0 || i == 1 || i == 4 || i ==10 || i == 16)
                    {
                        canvas.DrawLine(marginLeft + timeColWidth + i*cellWidth, marginTop, marginLeft + timeColWidth + i * cellWidth, marginTop + cellHeight * row, tablePaint);
                    }
                    else
                    {
                        canvas.DrawLine(marginLeft + timeColWidth + i * cellWidth, marginTop + cellHeight, marginLeft + timeColWidth + i * cellWidth, marginTop + cellHeight * row, tablePaint);
                    }
                }

                for(int i=0; i<hourlistInfo.Count; i++)
                {
                    object[] array = typeof(HourlistInfo).GetProperties()
                    .Select(p => p.GetValue(hourlistInfo[i]))
                    .ToArray();
                    for (int j = 0; j < col; j++)
                    {
                        string cellValue = array[j].ToString();
                        if(j == 0)
                        {
                            canvas.DrawText(cellValue, marginLeft + 0.5f * timeColWidth, marginTop + cellHeight * (i + 2.8f), SKTextAlign.Center, textFont, tablePaint);

                        }
                        else
                        {
                            canvas.DrawText(cellValue, marginLeft + timeColWidth + (j-0.5f) * cellWidth, marginTop + cellHeight * (i + 2.8f), SKTextAlign.Center, textFont, tablePaint);

                        }

                    }
                }

            }
        }

        public void PaintPaceList(SKCanvas canvas, List<JObject> paceListInfo)
        {
            if (paceListInfo.Count == 0) return;
            using (SKPaint tablePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f * DpiScaleFactor,
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * DpiScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                float timeColWidth = 65f * DpiScaleFactor;
                int col = 13;
                int row = paceListInfo.Count + 2;
                float cellWidth = (width - timeColWidth) / (col - 1);
                float cellHeight = height / 26;

                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + cellHeight * row, tablePaint);
                canvas.DrawLine(marginLeft, marginTop, marginLeft + width, marginTop, tablePaint);
                canvas.DrawLine(marginLeft + width, marginTop, marginLeft + width, marginTop + cellHeight * row, tablePaint);
                canvas.DrawLine(marginLeft, marginTop + cellHeight * row, marginLeft + width, marginTop + cellHeight * row, tablePaint);


                canvas.DrawText(CommonValues.Langs["时间"], marginLeft + 0.5f * timeColWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["总心搏"], marginLeft + timeColWidth + 0.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["心率"], marginLeft + timeColWidth + 2.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["最慢"], marginLeft + timeColWidth + 1.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["平均"], marginLeft + timeColWidth + 2.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["最快"], marginLeft + timeColWidth + 3.5f * cellWidth, marginTop + cellHeight * 1.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["起搏总数"], marginLeft + timeColWidth + 4.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["夺获失败"], marginLeft + timeColWidth + 5.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["感知失败"], marginLeft + timeColWidth + 6.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["输出失败"], marginLeft + timeColWidth + 7.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["aP"], marginLeft + timeColWidth + 8.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["vP"], marginLeft + timeColWidth + 9.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["avP"], marginLeft + timeColWidth + 10.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["停搏"], marginLeft + timeColWidth + 11.5f * cellWidth, marginTop + cellHeight * 1.2f, SKTextAlign.Center, textFont, tablePaint);


                for (int i = 0; i < row - 1; i++)
                {
                    if (i == 0)
                    {
                        canvas.DrawLine(marginLeft + timeColWidth + cellWidth, marginTop + cellHeight, marginLeft + timeColWidth + cellWidth*4, marginTop + cellHeight, tablePaint);
                    }
                    else
                    {
                        canvas.DrawLine(marginLeft, marginTop + cellHeight * (i + 1), marginLeft + width, marginTop + cellHeight * (i + 1), tablePaint);

                    }
                }

                for (int i = 0; i < col - 1; i++)
                {
                    if(i == 2 || i == 3)
                    {
                        canvas.DrawLine(marginLeft + timeColWidth + i * cellWidth, marginTop + cellHeight, marginLeft + timeColWidth + i * cellWidth, marginTop + cellHeight * row, tablePaint);

                    }
                    else
                    {
                        canvas.DrawLine(marginLeft + timeColWidth + i * cellWidth, marginTop, marginLeft + timeColWidth + i * cellWidth, marginTop + cellHeight * row, tablePaint);
                    }
                }

                for (int i = 0; i < paceListInfo.Count; i++)
                {
                    JArray arrayObj = new JArray(paceListInfo[i].Properties().Select(p => p.Value));
                    object[] array = arrayObj.ToArray();
                    for (int j = 0; j < col; j++)
                    {
                        string cellValue = array[j].ToString();
                        if (j == 0)
                        {
                            canvas.DrawText(cellValue, marginLeft + 0.5f * timeColWidth, marginTop + cellHeight * (i + 2.8f), SKTextAlign.Center, textFont, tablePaint);

                        }
                        else
                        {
                            canvas.DrawText(cellValue, marginLeft + timeColWidth + (j - 0.5f) * cellWidth, marginTop + cellHeight * (i + 2.8f), SKTextAlign.Center, textFont, tablePaint);

                        }

                    }
                }

            }
        }
        }
}
