using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zoncare.Common;
using Zoncare.DAL;

namespace AnalysisApp.Commons
{
    internal class BpPrintReportPage
    {
        public List<List<JObject>> AllPageInfo = new List<List<JObject>>();
        public int currentPageIndex = 0;
        //是否横向打印
        private readonly bool bHorizontalPrint = false;

        public double PaperWidthMm = 210.0;      // A4纸宽度
        public double PaperHeightMm = 297.0;     // A4纸高度
        public double MarginLeftMm = 10.0;       // 左边距
        public double MarginRightMm = 15.0;      // 右边距
        public double MarginTopMm = 20.0;          //上边距
        public double MarginBottonMm = 17.0;       //下边距
        private const double GridSizeMm = 5.0;          // 5mm网格
        private const double MinorGridSizeMm = 1.0;     // 1mm小网格
        private const float PrintDPI = 300f; // 打印专用DPI
        private const float PixMM = PrintDPI / 25.4f; // 300 DPI = 11.81像素/mm

        // 字体大小缩放因子（300 DPI相对于96 DPI）
        private const float FontScaleFactor = PrintDPI / 96f; // 300/96 = 3.125倍

        public int DataListIdx = 0;   //数据表表从报告的第几页开始

        // 坐标缩放工具方法
        private float ScaleX(double mm)
        {
            return (float)(mm * PixMM);
        }

        private float ScaleY(double mm)
        {
            return (float)(mm * PixMM);
        }

        private float Scale(double mm)
        {
            return (float)(mm * PixMM);
        }

        //private const float PixMM = 3.7795f;             // 1mm 3.7795个像素点
        private readonly bool bDrawSmallGrid = true;
        //波形的上边距
        public double WaveTopMargin { get; set; } = 5 * PixMM;
        private float WaveBottomMargin { get; set; } = 0;
        //波形的左边距
        public double WaveLeftMargin { get; set; } = 10 * PixMM;
        public double WaveRightMargin { get; set; } = 0;

        public double Speed = 25;
        public double Gain = 10.0;
        public int StartSample = 0;
        public List<int> LeadIdxs { get; set; } = new List<int>();                                //导联索引列表
        public List<List<float>> EcgData { get; set; } = new List<List<float>>();                                    //ECG波形数据
        private int PointCount { get; set; } = 0;
        public List<BeatInfo> HeartbeatInfos { get; set; } = new List<BeatInfo>();                            //心搏信息
        public string SegmentLabel = "";

        public string Advice = "";

        private readonly SKColor _gridColor = SKColor.Parse("#db634f");
        private readonly SKColor _minorGridColor = SKColor.Parse("#f3aca0");
        private readonly SKColor _waveColor = new SKColor(0, 0, 0);
        private readonly SKColor _textColor = new SKColor(0, 0, 0);
        private readonly SKColor _backgroundColor = new SKColor(255, 255, 255);

        private readonly reportSettingValues ReportSets = CommonValues.SystemSettings.reportSets;
        private bpDiagnoseSettingValues SettingValues;

        double MaxWakeSys = 0;
        double MinWakeSys = 0;
        double MaxSleepSys = 0;
        double MinSleepSys = 0;
        double MaxErrorSys = 0;
        double MinErrorSys = 0;

        double MaxWakeDia = 0;
        double MinWakeDia = 0;
        double MaxSleepDia = 0;
        double MinSleepDia = 0;
        double MaxErrorDia = 0;
        double MinErrorDia = 0;

        //总页数
        public int TotalPages;


        private double colWidthMm;

        private double subColWidthLongMm;
        private double subColWidthShortMm;

        private double subHeightMm = 42;

        public List<List<JObject>> SummaryInfo;
        public List<List<JObject>> DataListInfo;
        public List<List<JObject>> HourInfo;
        public List<List<JObject>> TrendWaveInfo;
        public List<List<JObject>> HistogramInfo;
        public List<List<JObject>> PieInfo;
        public List<List<JObject>> FitInfo;
        public List<List<JObject>> ArteriosclerosisInfo;

        //每页中每个片段的网格区域
        public SKRect WaveRect { get; set; } = new SKRect();


        public Bitmap RenderReportPage(int pageNum, string pageName)
        {
            if (bHorizontalPrint)
            {
                //横向
                PaperWidthMm = 297.0;
                PaperHeightMm = 210.0;
                MarginLeftMm = 15.0;       // 左边距
                MarginRightMm = 22.0;      // 右边距
                MarginTopMm = 25.0;          //上边距
                MarginBottonMm = 15.0;       //下边距
            }
            else
            {
                //纵向
                PaperWidthMm = 210.0;
                PaperHeightMm = 297.0;
                MarginLeftMm = 8.0;       // 左边距
                MarginRightMm = 15.0;      // 右边距
                MarginTopMm = 10.0;          //上边距
                MarginBottonMm = 15.0;       //下边距

            }

            double pageWidthPx = PaperWidthMm * PixMM;
            double pageHeightPx = PaperHeightMm * PixMM;

            float PageTopMargin = (float)MarginTopMm * PixMM;
            float PageBottomMargin = (float)MarginBottonMm * PixMM;
            colWidthMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) / 3;
            subColWidthLongMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) / 6;
            subColWidthShortMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) / 6;

            SettingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            // 创建SKSurface用于绘制
            using (SKSurface surface = SKSurface.Create(new SKImageInfo((int)pageWidthPx, (int)pageHeightPx, SKColorType.Rgb888x, SKAlphaType.Opaque, SKColorSpace.CreateSrgb())))
            {
                SKCanvas canvas = surface.Canvas;
                // 清除画布为白色背景
                canvas.Clear(SKColors.White);
                switch (pageName)
                {
                    case "summary":
                        DrawSummaryPage(canvas, pageNum);
                        break;
                    case "DataList":
                        DrawDataList(canvas, pageNum);
                        break;
                    case "HourList":
                        DrawHourlistPage(canvas, pageNum);
                        break;
                    case "TrendWave":
                        DrawTrendWavePage(canvas, pageNum);
                        break;
                    case "Histogram":
                        DrawHistogramPage(canvas, pageNum);
                        break;
                    case "Pie":
                        DrawPiePage(canvas, pageNum);
                        break;
                    case "Fit":
                        DrawFitPage(canvas, pageNum);
                        break;
                    case "Arteriosclerosis":
                        DrawArteriosclerosisPage(canvas, pageNum);
                        break;
                    default:
                        break;
                }

                // 保存为PNG文件
                //string outputPath = $"{pageNum} + black_curve.png";
                using (SKImage image = surface.Snapshot())
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (MemoryStream stream = new MemoryStream(data.ToArray()))
                //using (FileStream streamdata = File.OpenWrite(outputPath))
                {
                    //_ = image.ToBitmap();
                    //data.SaveTo(streamdata);
                    Bitmap originalBitmap = new Bitmap(stream);
                    return originalBitmap;
                }
            }
        }

        public void DrawSummaryPage(SKCanvas canvas, int pageNumber)
        {
            DrawSummaryPageHeader(canvas, CommonValues.Langs["动态血压报告"], pageNumber);
            DrawPatientInfo(canvas);
            DrawSummaryInfo(canvas, 0);
            DrawSummaryInfo(canvas, 1);
            DrawSummaryInfo(canvas, 2);
            DrawSummaryAdviceInfo(canvas);
            DrawLines(canvas);
        }

        private void DrawSummaryPageHeader(SKCanvas canvas, string title, int pageNumber)
        {
            using (SKFont bigTitleFont = new SKFont
            {
                Size = 16.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体", SKFontStyle.Bold)
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 14.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体", SKFontStyle.Bold)
            })
            using (SKFont textFont = new SKFont
            {
                Size = 12.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont copyRightFont = new SKFont
            {
                Size = 9.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体"),
                Edging = SKFontEdging.SubpixelAntialias
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true,
            })
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.6f,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                string acceptFlag = CommonValues.SystemSettings.reportSets.acceptFlag;
                if (acceptFlag != "")
                {
                    canvas.DrawText(acceptFlag, ScaleX(PaperWidthMm - MarginRightMm),
                    ScaleY(MarginTopMm + 3), SKTextAlign.Right, textFont, textPaint);
                }
                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

                canvas.DrawText(CommonValues.SystemSettings.baseSets.hospitalName, ScaleX(PaperWidthMm) / 2,
                ScaleY(MarginTopMm - 2), SKTextAlign.Center, bigTitleFont, textPaint);
                canvas.DrawText(title, ScaleX(PaperWidthMm) / 2,
                ScaleY(MarginTopMm + 4), SKTextAlign.Center, titleFont, textPaint);
                string recordTimeText = $"打印时间: {DateTime.Now.ToString(dateFormat)}";
                //canvas.DrawText(recordTimeText, ScaleX(PaperWidthMm - MarginRightMm),
                //ScaleY(MarginTopMm + 8), SKTextAlign.Right, textFont, textPaint);

                string pageFoot = CommonValues.SystemSettings.reportSets.pageFoot;
                if (pageFoot != "")
                {
                    canvas.DrawText(pageFoot, ScaleX(MarginLeftMm), ScaleY(PaperHeightMm - MarginBottonMm), SKTextAlign.Left, copyRightFont, textPaint);
                }
                else
                {
                    string copyrightStr = CommonValues.Langs["*此报告仅供临床医生参考"];
                    canvas.DrawText(copyrightStr, ScaleX(MarginLeftMm), ScaleY(PaperHeightMm - MarginBottonMm), SKTextAlign.Left, copyRightFont, textPaint);
                }

                if (CommonValues.SystemSettings.reportSets.displayPageNum == 1)
                {

                    // 绘制页码
                    int pageNum = pageNumber + 1;
                    string footerText = $"{pageNum}/{TotalPages}";

                    canvas.DrawText(footerText, ScaleX(PaperWidthMm - MarginRightMm), ScaleY(PaperHeightMm - MarginBottonMm), SKTextAlign.Right, textFont, textPaint);
                }
            }
        }

        private void DrawPatientInfo(SKCanvas canvas)
        {
            using (SKFont titleFont = new SKFont
            {
                Size = 14.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont textFont = new SKFont
            {
                Size = 13.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.6f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 9) * PixMM, (float)MarginLeftMm * PixMM, (float)(MarginTopMm + 38) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 9) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 9) * PixMM, linePaint);
                canvas.DrawLine((float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 9) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 38) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 38) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 38) * PixMM, linePaint);

                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

                canvas.DrawText(CommonValues.Langs["患者信息"], ScaleX(PaperWidthMm) / 2,
                ScaleY(MarginTopMm + 13), SKTextAlign.Center, titleFont, textPaint);

                string nameText = CommonValues.Langs["姓名"] + $": {AnaValues.CurrentCase.name}";
                canvas.DrawText(nameText, ScaleX(MarginLeftMm + 1),
                ScaleY(MarginTopMm + 18), SKTextAlign.Left, textFont, textPaint);

                string genderText = CommonValues.Langs["性别"] + $": {SysConstants.Genders[AnaValues.CurrentCase.sex]}";
                canvas.DrawText(genderText, ScaleX(MarginLeftMm + 1),
                ScaleY(MarginTopMm + 24), SKTextAlign.Left, textFont, textPaint);

                string ageText = CommonValues.Langs["年龄"] + ": ";
                if (AnaValues.CurrentCase.age?.ToString() != null)
                {
                    ageText += $"{AnaValues.CurrentCase.age} {SysConstants.AgeUnits[AnaValues.CurrentCase.ageUnit]}";
                }
                canvas.DrawText(ageText, ScaleX(MarginLeftMm + 1),
                ScaleY(MarginTopMm + 30), SKTextAlign.Left, textFont, textPaint);

                string bedText = CommonValues.Langs["病区/床号"] + $": {AnaValues.CurrentCase.bedNo}";
                canvas.DrawText(bedText, ScaleX(MarginLeftMm + 1),
                ScaleY(MarginTopMm + 36), SKTextAlign.Left, textFont, textPaint);

                string caseNoText = CommonValues.Langs["病历号"] + $": {AnaValues.CurrentCase.caseNo}";
                canvas.DrawText(caseNoText, ScaleX(MarginLeftMm + colWidthMm),
                ScaleY(MarginTopMm + 18), SKTextAlign.Left, textFont, textPaint);

                string idcardText = CommonValues.Langs["ID号"] + $": {AnaValues.CurrentCase.idcard}";
                canvas.DrawText(idcardText, ScaleX(MarginLeftMm + colWidthMm),
                ScaleY(MarginTopMm + 24), SKTextAlign.Left, textFont, textPaint);

                string applyDeptText = CommonValues.Langs["申请科室"] + $": {AnaValues.CurrentCase.applyDept}";
                canvas.DrawText(applyDeptText, ScaleX(MarginLeftMm + colWidthMm),
                ScaleY(MarginTopMm + 30), SKTextAlign.Left, textFont, textPaint);

                string checkDeptText = CommonValues.Langs["检查科室"] + $": {AnaValues.CurrentCase.checkDept}";
                canvas.DrawText(checkDeptText, ScaleX(MarginLeftMm + colWidthMm),
                ScaleY(MarginTopMm + 36), SKTextAlign.Left, textFont, textPaint);

                string checkDoctorText = CommonValues.Langs["检查医生"] + $": {AnaValues.CurrentCase.checkDoctor}";
                canvas.DrawText(checkDoctorText, ScaleX(MarginLeftMm + (colWidthMm * 2)),
                ScaleY(MarginTopMm + 18), SKTextAlign.Left, textFont, textPaint);

                string recordTimeText = CommonValues.Langs["记录时间"] + $": {AnaValues.CurrentCase.recordTime?.ToString(dateFormat)}";
                canvas.DrawText(recordTimeText, ScaleX(MarginLeftMm + (colWidthMm * 2)),
                ScaleY(MarginTopMm + 24), SKTextAlign.Left, textFont, textPaint);

                string clinicalDiagnosisText = CommonValues.Langs["临床诊断"] + $": {AnaValues.CurrentCase.clinicalDiagnosis}";
                canvas.DrawText(clinicalDiagnosisText, ScaleX(MarginLeftMm + (colWidthMm * 2)),
                ScaleY(MarginTopMm + 30), SKTextAlign.Left, textFont, textPaint);

                string medicationText = CommonValues.Langs["治疗用药"] + $": {AnaValues.CurrentCase.medication}";
                canvas.DrawText(medicationText, ScaleX(MarginLeftMm + (colWidthMm * 2)),
                ScaleY(MarginTopMm + 36), SKTextAlign.Left, textFont, textPaint);
            }
        }

        private void DrawSummaryInfo(SKCanvas canvas, int type)
        {
            using (SKFont titleFont = new SKFont
            {
                Size = 14.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont textFont = new SKFont
            {
                Size = 13.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            {
                JObject allSummary = SummaryInfo[0][type];
                double baseYMm = 0;
                string allTime = "";
                int maxSys = SettingValues.maxValidSys;
                int maxDia = SettingValues.maxValidDia;

                if (type == 0)
                {
                    allTime = $"{CommonValues.Langs["全程"]}({allSummary["duration"]})";
                    baseYMm = MarginTopMm;
                }
                else if (type == 1)
                {
                    allTime = $"{CommonValues.Langs["白天"]}({allSummary["duration"]})";
                    baseYMm = MarginTopMm + subHeightMm + 12;

                    maxSys = SettingValues.maxWakeSys;
                    maxDia = SettingValues.maxWakeDia;
                }
                else
                {
                    allTime = $"{CommonValues.Langs["夜间"]}({allSummary["duration"]})";
                    baseYMm = MarginTopMm + subHeightMm * 2 + 12;

                    maxSys = SettingValues.maxSleepSys;
                    maxDia = SettingValues.maxSleepDia;
                }

                string maxSysStr = maxSys.ToString();
                string maxDiaStr = maxDia.ToString();
                if (SettingValues.unit == "kPa")
                {
                    maxSysStr = CommonFuns.mmHg2KPa(maxSys, 1).ToString("F1");
                    maxDiaStr = CommonFuns.mmHg2KPa(maxDia, 1).ToString("F1");
                }

                canvas.DrawText(allTime, ScaleX(PaperWidthMm) / 2,
                (float)(baseYMm + 44) * PixMM, SKTextAlign.Center, titleFont, textPaint);

                canvas.DrawText(CommonValues.Langs["收缩压"] + "(" + SettingValues.unit + ")", ScaleX(MarginLeftMm + 1),
                (float)(baseYMm + 55) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["舒张压"] + "(" + SettingValues.unit + ")", ScaleX(MarginLeftMm + 1),
                (float)(baseYMm + 61) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["平均压"] + "(" + SettingValues.unit + ")", ScaleX(MarginLeftMm + 1),
                (float)(baseYMm + 67) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["脉率"] + "(" + CommonValues.Langs["次/分"] + ")", ScaleX(MarginLeftMm + 1),
                (float)(baseYMm + 73) * PixMM, SKTextAlign.Left, textFont, textPaint);

                if (type == 0)
                {
                    canvas.DrawText($"{CommonValues.Langs["收缩压超出界限"]}", ScaleX(MarginLeftMm + 1),
                    (float)(baseYMm + 79) * PixMM, SKTextAlign.Left, textFont, textPaint);
                }
                else
                {
                    canvas.DrawText($"{CommonValues.Langs["收缩压"]}>{maxSysStr} {SettingValues.unit}", ScaleX(MarginLeftMm + 1),
                    (float)(baseYMm + 79) * PixMM, SKTextAlign.Left, textFont, textPaint);
                }


                canvas.DrawText(CommonValues.Langs["最小值"], ScaleX(MarginLeftMm + subColWidthLongMm),
                (float)(baseYMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(allSummary["SbpMin"] + "(" + allSummary["SbpMinTime"] + ")", ScaleX(MarginLeftMm + subColWidthLongMm),
                (float)(baseYMm + 55) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(allSummary["DbpMin"] + "(" + allSummary["DbpMinTime"] + ")", ScaleX(MarginLeftMm + subColWidthLongMm),
                (float)(baseYMm + 61) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["MapMin"]}", ScaleX(MarginLeftMm + subColWidthLongMm),
                (float)(baseYMm + 67) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(allSummary["PulseMin"] + "(" + allSummary["PulseMinTime"] + ")", ScaleX(MarginLeftMm + subColWidthLongMm),
                (float)(baseYMm + 73) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["SbpGtLimit"]}({allSummary["SbpLoadNum"]}%)", ScaleX(MarginLeftMm + subColWidthLongMm),
                (float)(baseYMm + 79) * PixMM, SKTextAlign.Left, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["平均值"], ScaleX(MarginLeftMm + subColWidthLongMm * 2),
                (float)(baseYMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["SbpAvg"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 2),
                (float)(baseYMm + 55) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["DbpAvg"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 2),
                (float)(baseYMm + 61) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["MapAvg"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 2),
                (float)(baseYMm + 67) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["PulseAvg"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 2),
                (float)(baseYMm + 73) * PixMM, SKTextAlign.Left, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["最大值"], ScaleX(MarginLeftMm + subColWidthLongMm * 2 + subColWidthShortMm),
                (float)(baseYMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(allSummary["SbpMax"] + "(" + allSummary["SbpMaxTime"] + ")", ScaleX(MarginLeftMm + subColWidthLongMm * 2 + subColWidthShortMm),
                (float)(baseYMm + 55) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(allSummary["DbpMax"] + "(" + allSummary["DbpMaxTime"] + ")", ScaleX(MarginLeftMm + subColWidthLongMm * 2 + subColWidthShortMm),
                (float)(baseYMm + 61) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["MapMax"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 2 + subColWidthShortMm),
                (float)(baseYMm + 67) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(allSummary["PulseMax"] + "(" + allSummary["PulseMaxTime"] + ")", ScaleX(MarginLeftMm + subColWidthLongMm * 2 + subColWidthShortMm),
                (float)(baseYMm + 73) * PixMM, SKTextAlign.Left, textFont, textPaint);

                if (type == 0)
                {
                    canvas.DrawText($"{CommonValues.Langs["舒张压超出界限"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 2 + subColWidthShortMm),
                    (float)(baseYMm + 79) * PixMM, SKTextAlign.Left, textFont, textPaint);
                }
                else
                {
                    canvas.DrawText($"{CommonValues.Langs["舒张压"]}>{maxDiaStr} {SettingValues.unit}", ScaleX(MarginLeftMm + subColWidthLongMm * 2 + subColWidthShortMm),
                    (float)(baseYMm + 79) * PixMM, SKTextAlign.Left, textFont, textPaint);
                }



                canvas.DrawText(CommonValues.Langs["标准差"], ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm),
                (float)(baseYMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["SbpBzc"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm),
                (float)(baseYMm + 55) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["DbpBzc"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm),
                (float)(baseYMm + 61) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["MapBzc"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm),
                (float)(baseYMm + 67) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["PulseBzc"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm),
                (float)(baseYMm + 73) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["DbpGtLimit"]}({allSummary["DbpLoadNum"]}%)", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm),
                (float)(baseYMm + 79) * PixMM, SKTextAlign.Left, textFont, textPaint);

                canvas.DrawText($"{CommonValues.Langs["变异系数"]}(%)", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm * 2),
                (float)(baseYMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["SbpByxs"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm * 2),
                (float)(baseYMm + 55) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["DbpByxs"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm * 2),
                (float)(baseYMm + 61) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["MapByxs"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm * 2),
                (float)(baseYMm + 67) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{allSummary["PulseByxs"]}", ScaleX(MarginLeftMm + subColWidthLongMm * 3 + subColWidthShortMm * 2),
                (float)(baseYMm + 73) * PixMM, SKTextAlign.Left, textFont, textPaint);

                if (type == 0)
                {
                    canvas.DrawText($"{CommonValues.Langs["测量总次数"]}/{CommonValues.Langs["有效记录数"]}:{allSummary["count"]}/{allSummary["effectiveCount"]}",
                    ScaleX(MarginLeftMm + 1), (float)(baseYMm + 85) * PixMM, SKTextAlign.Left, textFont, textPaint);
                    canvas.DrawText($"{CommonValues.Langs["有效数据个数及占比"]}:{allSummary["effectiveCount"]}({allSummary["effectiveCountRate"]}%)",
                    ScaleX(MarginLeftMm + 1), (float)(baseYMm + 91) * PixMM, SKTextAlign.Left, textFont, textPaint);

                    canvas.DrawText($"{CommonValues.Langs["白天次数"]}:{allSummary["dayCount"]}",
                    ScaleX(MarginLeftMm + colWidthMm), (float)(baseYMm + 85) * PixMM, SKTextAlign.Left, textFont, textPaint);
                    canvas.DrawText($"{CommonValues.Langs["错误数据个数及占比"]}:{allSummary["errorNum"]}({allSummary["errorNumPercent"]}%)",
                    ScaleX(MarginLeftMm + colWidthMm), (float)(baseYMm + 91) * PixMM, SKTextAlign.Left, textFont, textPaint);

                    canvas.DrawText($"{CommonValues.Langs["夜间次数"]}:{allSummary["nightCount"]}",
                    ScaleX(MarginLeftMm + colWidthMm * 2), (float)(baseYMm + 85) * PixMM, SKTextAlign.Left, textFont, textPaint);

                    baseYMm = MarginTopMm + 44 + subHeightMm * 3 + 12 + 1;
                    canvas.DrawText($"AASI:{allSummary["AASI"]}",
                    ScaleX(MarginLeftMm + 1), (float)(baseYMm) * PixMM, SKTextAlign.Left, textFont, textPaint);
                    canvas.DrawText($"(参考值 <0.55)",
                    ScaleX(MarginLeftMm + colWidthMm), (float)(baseYMm) * PixMM, SKTextAlign.Left, textFont, textPaint);

                    int morningSbpAvgMax = 135;
                    int morningSbpAvgMin = 85;
                    int MorningPre = 35;
                    string morningSbpAvgMaxStr = morningSbpAvgMax.ToString();
                    string morningSbpAvgMinStr = morningSbpAvgMin.ToString();
                    string MorningPreStr = MorningPre.ToString();
                    if (SettingValues.unit == "kPa")
                    {
                        morningSbpAvgMaxStr = CommonFuns.mmHg2KPa(morningSbpAvgMax, 1).ToString("F1");
                        morningSbpAvgMinStr = CommonFuns.mmHg2KPa(morningSbpAvgMin, 1).ToString("F1");
                        MorningPreStr = CommonFuns.mmHg2KPa(MorningPre, 1).ToString("F1");
                    }

                    canvas.DrawText($"{CommonValues.Langs["早间血压"]}:{allSummary["morningSbpAvg"]}/{allSummary["morningDbpAvg"]} {SettingValues.unit}",
                    ScaleX(MarginLeftMm + 1), (float)(baseYMm + 6) * PixMM, SKTextAlign.Left, textFont, textPaint);
                    canvas.DrawText($"(参考值 <{morningSbpAvgMaxStr}/{morningSbpAvgMinStr} {SettingValues.unit})",
                    ScaleX(MarginLeftMm + colWidthMm), (float)(baseYMm + 6) * PixMM, SKTextAlign.Left, textFont, textPaint);

                    canvas.DrawText($"{CommonValues.Langs["晨峰血压"]}:{allSummary["MorningPre"]} {SettingValues.unit}",
                    ScaleX(MarginLeftMm + 1), (float)(baseYMm + 12) * PixMM, SKTextAlign.Left, textFont, textPaint);
                    canvas.DrawText($"(参考值 <{MorningPreStr} {SettingValues.unit})",
                    ScaleX(MarginLeftMm + colWidthMm), (float)(baseYMm + 12) * PixMM, SKTextAlign.Left, textFont, textPaint);
                }
            }
        }

        private void DrawSummaryAdviceInfo(SKCanvas canvas)
        {
            using (SKFont titleFont = new SKFont
            {
                Size = 14.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont textFont = new SKFont
            {
                Size = 13.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.6f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                double baseYMm = MarginTopMm + 44 + subHeightMm * 3 + 24 + 8;

                canvas.DrawText(CommonValues.Langs["检查结论"], (float)(PaperWidthMm * PixMM) / 2,
                (float)(baseYMm) * PixMM, SKTextAlign.Center, titleFont, textPaint);

                int lineIdx = 0;
                string[] advices = Advice.Split('\n');
                for (int i = 0; i < advices.Length; i++)
                {
                    if (lineIdx > 11 || advices[i] == "") break;
                    string advice = "";
                    advice = (i + 1).ToString() + "." + advices[i];

                    List<string> lines = AnaCode.WrapText(advice, (float)((PaperWidthMm - MarginLeftMm - MarginRightMm) * PixMM), textFont);
                    foreach (string line in lines)
                    {
                        if (lineIdx > 11) break;

                        float y = (float)(baseYMm + 4 + (6 * lineIdx)) * PixMM;
                        canvas.DrawText(line, (float)((MarginLeftMm + 1) * PixMM), y, SKTextAlign.Left, textFont, textPaint);
                        ++lineIdx;
                    }
                }

                string diagnoseSign = "";
                user diagUser = ZoncareEntities.Instance.User.FirstOrDefault(u => u.userName == AnaValues.CurrentCase.diagnoseDoctor && (u.delFlag == null || u.delFlag == 0));
                if (diagUser != null)
                {
                    if (!string.IsNullOrEmpty(diagUser.sign))
                    {
                        diagnoseSign = diagUser.sign;
                    }
                }

                if (CommonValues.SystemSettings.reportSets.signMode == 1)
                {
                    DrawReportSignInfo(canvas, CommonValues.Langs["报告医生"], diagnoseSign, (PaperWidthMm - 75) * PixMM, (PaperWidthMm - 60) * PixMM, (PaperWidthMm - 60) * PixMM, textFont, textPaint, linePaint);
                }
                else
                {
                    DrawReportSignInfo(canvas, CommonValues.Langs["报告医生"], diagnoseSign, (PaperWidthMm / 3) * PixMM, (PaperWidthMm / 3 + 15) * PixMM, (PaperWidthMm / 3 + 15) * PixMM, textFont, textPaint, linePaint);
                    string auditSign = CommonValues.SystemSettings.reportSets.auditSign;
                    DrawReportSignInfo(canvas, CommonValues.Langs["审核医生"], auditSign, (PaperWidthMm - 75) * PixMM, (PaperWidthMm - 60) * PixMM, (PaperWidthMm - 60) * PixMM, textFont, textPaint, linePaint);
                }

            }
        }

        private void DrawReportSignInfo(SKCanvas canvas, string title, string sign, double titleLeft, double signLeft, double lineLeft, SKFont textFont, SKPaint textPaint, SKPaint linePaint)
        {
            DrawSignTitle(canvas, title, titleLeft, textFont, textPaint);
            if (AnaValues.CurrentCase.status >= 3 && !string.IsNullOrEmpty(sign))
            {
                DrawSign(canvas, sign, signLeft);
            }
            else
            {
                canvas.DrawLine((float)lineLeft, (float)(MarginTopMm + 265) * PixMM, (float)(lineLeft + 30 * PixMM), (float)(MarginTopMm + 265) * PixMM, linePaint);
            }
        }

        private void DrawSignTitle(SKCanvas canvas, string title, double left, SKFont textFont, SKPaint textPaint)
        {
            canvas.DrawText(title, (float)left,
            (float)(MarginTopMm + 265) * PixMM, SKTextAlign.Left, textFont, textPaint);
        }

        private void DrawSign(SKCanvas canvas, string sign, double left)
        {
            SKBitmap bitmap = AnaCode.Base64ToBitmap(sign);

            // 4. 将图片从屏幕DPI(96)缩放到打印DPI(300)
            float scaleFactor = PrintDPI / 96f / 2;
            float scaledWidth = bitmap.Width * scaleFactor;
            float scaledHeight = bitmap.Height * scaleFactor;

            // 5. 绘制缩放后的图片
            canvas.DrawBitmap(bitmap, new SKRect(
                (float)left,
                (float)(MarginTopMm + 260) * PixMM - scaledHeight / 4,
                (float)left + scaledWidth,
                (float)(MarginTopMm + 260) * PixMM + scaledHeight * 3 / 4));
        }

        private void DrawLines(SKCanvas canvas)
        {
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.6f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                double baseYMm = MarginTopMm + 40 + subHeightMm * 3 + 12;

                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)MarginLeftMm * PixMM, (float)(baseYMm) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, linePaint);
                canvas.DrawLine((float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(baseYMm) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(baseYMm) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(baseYMm) * PixMM, linePaint);

                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + subHeightMm + 52) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + subHeightMm + 52) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + subHeightMm * 2 + 52) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + subHeightMm * 2 + 52) * PixMM, linePaint);

                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(baseYMm + 20) * PixMM, (float)MarginLeftMm * PixMM, (float)(baseYMm + 88) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(baseYMm + 20) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(baseYMm + 20) * PixMM, linePaint);
                canvas.DrawLine((float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(baseYMm + 20) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(baseYMm + 88) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(baseYMm + 88) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(baseYMm + 88) * PixMM, linePaint);

            }
        }

        private void DrawDataList(SKCanvas canvas, int pageNumber)
        {
            int idx = pageNumber - DataListIdx;
            if (idx == DataListInfo.Count) return;

            GetSettings();
            DrawPageHeader(canvas, pageNumber);

            List<JObject> dataListInfo = DataListInfo[idx];

            using (SKPaint tablePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f * FontScaleFactor,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm);
                float height = ScaleY(PaperHeightMm - MarginTopMm - MarginBottonMm - 12);

                int col = 7;
                int row = 41;
                float cellWidth = width / col;
                float cellHeight = height / row;

                float marginTop = ScaleY(MarginTopMm + 6);
                float marginLeft = ScaleX(MarginLeftMm);

                canvas.DrawText(CommonValues.Langs["数据列表"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 3), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + cellHeight * (dataListInfo.Count + 1), tablePaint);
                canvas.DrawLine(marginLeft, marginTop, marginLeft + width, marginTop, tablePaint);
                canvas.DrawLine(marginLeft + width, marginTop, marginLeft + width, marginTop + cellHeight * (dataListInfo.Count + 1), tablePaint);
                canvas.DrawLine(marginLeft, marginTop + cellHeight * (dataListInfo.Count + 1), marginLeft + width, marginTop + cellHeight * (dataListInfo.Count + 1), tablePaint);

                for (int i = 0; i < row - 1; i++)
                {
                    if (i == dataListInfo.Count) break;
                    canvas.DrawLine(marginLeft, marginTop + cellHeight * (i + 1), marginLeft + width, marginTop + cellHeight * (i + 1), tablePaint);
                }

                for (int i = 0; i < col; i++)
                {
                    canvas.DrawLine(marginLeft + i * cellWidth, marginTop, marginLeft + i * cellWidth, marginTop + cellHeight * (dataListInfo.Count + 1), tablePaint);
                }

                canvas.DrawText(CommonValues.Langs["序号"], marginLeft + 0.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["日期"], marginLeft + 1.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["时间"], marginLeft + 2.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["收缩压"], marginLeft + 3.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["舒张压"], marginLeft + 4.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["平均动脉压"], marginLeft + 5.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["脉率"], marginLeft + 6.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                for (int i = 0; i < dataListInfo.Count; i++)
                {
                    JArray arrayObj = new JArray(dataListInfo[i].Properties().Select(p => p.Value));
                    object[] array = arrayObj.ToArray();
                    bool bDay = Convert.ToBoolean(array[1]);
                    for (int j = 0; j <= col; j++)
                    {
                        var cellValue = array[j];

                        if (j == 0)
                        {
                            canvas.DrawText(cellValue.ToString(), marginLeft + (j + 0.3f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, textPaint);
                        }
                        else if (j == 1)
                        {
                            if (bDay)
                            {
                                DrawSun(canvas, marginLeft + (j - 1 + 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.5f), cellHeight / 5);
                            }
                            else
                            {
                                DrawMoon(canvas, marginLeft + (j - 1 + 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.5f), cellHeight / 4);
                            }
                        }
                        else if (j == 4)
                        {
                            double cellValueDouble = Convert.ToDouble(cellValue);
                            string cellValueStr = cellValue.ToString();
                            if (cellValueDouble > MaxErrorSys || cellValueDouble < MinErrorSys)
                            {
                                cellValueStr = cellValueStr + "   E";
                            }
                            else
                            {
                                if (bDay)
                                {
                                    if (cellValueDouble > MaxWakeSys)
                                    {
                                        cellValueStr = cellValueStr + "   ↑";
                                    }
                                    if (cellValueDouble < MinWakeSys)
                                    {
                                        cellValueStr = cellValueStr + "   ↓";
                                    }
                                }
                                else
                                {
                                    if (cellValueDouble > MaxSleepSys)
                                    {
                                        cellValueStr = cellValueStr + "   ↑";
                                    }
                                    if (cellValueDouble < MinSleepSys)
                                    {
                                        cellValueStr = cellValueStr + "   ↓";
                                    }
                                }
                            }
                            canvas.DrawText(cellValueStr, marginLeft + (j - 1 + 0.4f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Left, textFont, textPaint);
                        }
                        else if (j == 5)
                        {
                            double cellValueDouble = Convert.ToDouble(cellValue);
                            string cellValueStr = cellValue.ToString();
                            if (cellValueDouble > MaxErrorDia || cellValueDouble < MinErrorDia)
                            {
                                cellValueStr = cellValueStr + "   E";
                            }
                            else
                            {
                                if (bDay)
                                {
                                    if (cellValueDouble > MaxWakeDia)
                                    {
                                        cellValueStr = cellValueStr + "   ↑";
                                    }
                                    if (cellValueDouble < MinWakeDia)
                                    {
                                        cellValueStr = cellValueStr + "   ↓";
                                    }
                                }
                                else
                                {
                                    if (cellValueDouble > MaxSleepDia)
                                    {
                                        cellValueStr = cellValueStr + "   ↑";
                                    }
                                    if (cellValueDouble < MinSleepDia)
                                    {
                                        cellValueStr = cellValueStr + "   ↓";
                                    }
                                }
                            }
                            canvas.DrawText(cellValueStr, marginLeft + (j - 1 + 0.4f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Left, textFont, textPaint);
                        }
                        else
                        {
                            canvas.DrawText(cellValue.ToString(), marginLeft + (j - 1 + 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, textPaint);
                        }
                    }
                }
            }
        }

        private void DrawHourlistPage(SKCanvas canvas, int pageNumber)
        {
            Console.WriteLine("------DrawHourlistPage------");
            int idx = pageNumber - DataListIdx;
            if (idx == HourInfo.Count) return;
            DrawPageHeader(canvas, pageNumber);

            List<JObject> hourInfo = HourInfo[idx];

            using (SKPaint tablePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f * FontScaleFactor,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm);
                float height = ScaleY(PaperHeightMm - MarginTopMm - MarginBottonMm - 12);

                int col = 8;
                int row = 41;
                float cellWidth = width / col;
                float cellHeight = height / row;

                float marginTop = ScaleY(MarginTopMm + 6);
                float marginLeft = ScaleX(MarginLeftMm);

                canvas.DrawText(CommonValues.Langs["小时数据表"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 3), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + cellHeight * (hourInfo.Count + 1), tablePaint);
                canvas.DrawLine(marginLeft, marginTop, marginLeft + width, marginTop, tablePaint);
                canvas.DrawLine(marginLeft + width, marginTop, marginLeft + width, marginTop + cellHeight * (hourInfo.Count + 1), tablePaint);
                canvas.DrawLine(marginLeft, marginTop + cellHeight * (hourInfo.Count + 1), marginLeft + width, marginTop + cellHeight * (hourInfo.Count + 1), tablePaint);

                for (int i = 0; i < row - 1; i++)
                {
                    if (i == hourInfo.Count) break;
                    canvas.DrawLine(marginLeft, marginTop + cellHeight * (i + 1), marginLeft + width, marginTop + cellHeight * (i + 1), tablePaint);
                }

                for (int i = 0; i < col; i++)
                {
                    canvas.DrawLine(marginLeft + i * cellWidth, marginTop, marginLeft + i * cellWidth, marginTop + cellHeight * (hourInfo.Count + 1), tablePaint);
                }

                canvas.DrawText(CommonValues.Langs["序号"], marginLeft + 0.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["日期"], marginLeft + 1.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["时间"], marginLeft + 2.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["收缩压"], marginLeft + 3.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["舒张压"], marginLeft + 4.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["平均动脉压"], marginLeft + 5.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["脉率"], marginLeft + 6.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["个数"], marginLeft + 7.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, textPaint);

                for (int i = 0; i < hourInfo.Count; i++)
                {
                    JArray arrayObj = new JArray(hourInfo[i].Properties().Select(p => p.Value));
                    object[] array = arrayObj.ToArray();
                    for (int j = 0; j < col; j++)
                    {
                        var cellValue = array[j];
                        canvas.DrawText(cellValue.ToString(), marginLeft + (j + 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, textPaint);
                    }
                }

            }
        }

        private void DrawTrendWavePage(SKCanvas canvas, int pageNumber)
        {
            DrawPageHeader(canvas, pageNumber);

            float width = ScaleX(PaperWidthMm);
            float height = ScaleY(PaperHeightMm);

            List<JObject> trendWaveInfo = TrendWaveInfo[0];
            using (SKPaint rectPaint1 = new SKPaint
            {
                Color = SKColors.LightBlue,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint rectPaint2 = new SKPaint
            {
                Color = SKColors.Purple,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint rectPaint3 = new SKPaint
            {
                Color = SKColors.Yellow,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint rectPaint4 = new SKPaint
            {
                Color = SKColors.DarkGreen,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint rectPaint5 = new SKPaint
            {
                Color = SKColors.DarkRed,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true
            })
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 1.0f * FontScaleFactor,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 14.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont noteFont = new SKFont
            {
                Size = 13.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {

                canvas.DrawText(CommonValues.Langs["趋势图"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 5), SKTextAlign.Center, titleFont, textPaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + 8), ScaleY(MarginTopMm + MarginBottonMm + 183), ScaleX(MarginLeftMm + 8), ScaleY(MarginTopMm + MarginBottonMm + 250), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + 8), ScaleY(MarginTopMm + MarginBottonMm + 183), width - ScaleX(MarginLeftMm + MarginRightMm), ScaleY(MarginTopMm + MarginBottonMm + 183), linePaint);
                canvas.DrawLine(width - ScaleX(MarginLeftMm + MarginRightMm), ScaleY(MarginTopMm + MarginBottonMm + 183), width - ScaleX(MarginLeftMm + MarginRightMm), ScaleY(MarginTopMm + MarginBottonMm + 250), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + 8), ScaleY(MarginTopMm + MarginBottonMm + 250), width - ScaleX(MarginLeftMm + MarginRightMm), ScaleY(MarginTopMm + MarginBottonMm + 250), linePaint);

                string notes = CalcBPInfo.GetNotes();
                canvas.DrawText(CommonValues.Langs["备注"], ScaleX(MarginLeftMm) + ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm) / 2, ScaleY(MarginTopMm + MarginBottonMm + 187), SKTextAlign.Center, titleFont, textPaint);
                //canvas.DrawText(notes, ScaleX(MarginLeftMm) + ScaleX(10), ScaleY(MarginTopMm + MarginBottonMm + 192), SKTextAlign.Left, textFont, textPaint);
                if (notes != "")
                {
                    int lineIdx = 0;
                    string[] noteArr = notes.Split('\n');

                    for (int i = 0; i < noteArr.Length; i++)
                    {
                        if (lineIdx > 9) break;
                        List<string> lines = AnaCode.WrapText((i + 1).ToString() + "." + noteArr[i], (float)((PaperWidthMm - MarginLeftMm - MarginRightMm - 20) * PixMM), noteFont);
                        foreach (string line in lines)
                        {
                            if (lineIdx > 9) break;

                            float y = (float)(MarginTopMm + 208 + (6 * lineIdx)) * PixMM;
                            canvas.DrawText(line, ScaleX(MarginLeftMm) + ScaleX(10), y, SKTextAlign.Left, noteFont, textPaint);
                            ++lineIdx;
                        }
                    }
                }

                //canvas.DrawRect(ScaleX(MarginLeftMm + 30), ScaleY(MarginTopMm + MarginBottonMm + 193), ScaleX(rectWidth), ScaleY(rectHeight), rectPaint1);
                //canvas.DrawText(CommonValues.Langs["收缩压/舒张压"], ScaleX(MarginLeftMm + 39), ScaleY(MarginTopMm + MarginBottonMm + 196), SKTextAlign.Left, textFont, textPaint);

                //canvas.DrawRect(ScaleX(MarginLeftMm + 60), ScaleY(MarginTopMm + MarginBottonMm + 193), ScaleX(rectWidth), ScaleY(rectHeight), rectPaint2);
                //canvas.DrawText(CommonValues.Langs["脉率"], ScaleX(MarginLeftMm + 69), ScaleY(MarginTopMm + MarginBottonMm + 196), SKTextAlign.Left, textFont, textPaint);

                //canvas.DrawRect(ScaleX(MarginLeftMm + 80), ScaleY(MarginTopMm + MarginBottonMm + 193), ScaleX(rectWidth), ScaleY(rectHeight), rectPaint3);
                //canvas.DrawText(CommonValues.Langs["平均动脉压"], ScaleX(MarginLeftMm + 89), ScaleY(MarginTopMm + MarginBottonMm + 196), SKTextAlign.Left, textFont, textPaint);
            }


            DrawBpTrendWave drawBpTrendWave = new DrawBpTrendWave();
            drawBpTrendWave.WaveLeft = ScaleX(MarginLeftMm + 8);
            drawBpTrendWave.WaveTop = ScaleY(MarginTopMm + 15);
            drawBpTrendWave.WaveWidth = width - ScaleX(MarginLeftMm + MarginRightMm + 16);
            drawBpTrendWave.WaveHeight = height - ScaleY(MarginTopMm + MarginBottonMm + 100);

            drawBpTrendWave.settingValues = SettingValues;
            JArray arr = JArray.FromObject(trendWaveInfo[0]["bpDataInfoList"]);
            drawBpTrendWave.BPDataInfoList = arr.ToObject<List<BPDataInfo>>();
            drawBpTrendWave.FontScaleFactor = FontScaleFactor;
            drawBpTrendWave.DrawNightRect(canvas);
            drawBpTrendWave.DrawTrendWave(canvas);
            drawBpTrendWave.DrawMaxMinLine(canvas);
            drawBpTrendWave.DrawCoordinate(canvas);
        }

        private void DrawHistogramPage(SKCanvas canvas, int pageNumber)
        {
            List<JObject> histogramInfo = HistogramInfo[0];

            DrawPageHeader(canvas, pageNumber);
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 14f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                canvas.DrawText(CommonValues.Langs["直方图"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 5), SKTextAlign.Center, titleFont, textPaint);
            }

            DrawBpHistogramWave drawBpHistogramWave = new DrawBpHistogramWave();
            drawBpHistogramWave.settingValues = SettingValues;
            JArray arr = JArray.FromObject(histogramInfo[0]["HistogramInfo"]);
            drawBpHistogramWave.BPDataInfoList = arr.ToObject<List<BPDataInfo>>();
            drawBpHistogramWave.FontScaleFactor = FontScaleFactor;
            drawBpHistogramWave.WaveLeft = ScaleX(MarginLeftMm + 16);
            drawBpHistogramWave.WaveTop = ScaleY(MarginTopMm + 8);
            drawBpHistogramWave.WaveWidth = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm - 32);
            drawBpHistogramWave.WaveHeight = ScaleY(PaperHeightMm - MarginTopMm - MarginBottonMm - 40);
            drawBpHistogramWave.DrawCoordinate(canvas);
            drawBpHistogramWave.DrawHistogramWave(canvas);
        }

        private void DrawPiePage(SKCanvas canvas, int pageNumber)
        {
            List<JObject> pieInfo = PieInfo[0];

            DrawPageHeader(canvas, pageNumber);
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 14f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                canvas.DrawText(CommonValues.Langs["饼图"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 5), SKTextAlign.Center, titleFont, textPaint);
            }

            DrawBpPieChat drawBpPieChat = new DrawBpPieChat();
            drawBpPieChat.settingValues = SettingValues;
            JArray bpDataInfoList = JArray.FromObject(pieInfo[0]["bpDataInfoList"]);
            drawBpPieChat.BPDataInfoList = bpDataInfoList.ToObject<List<BPDataInfo>>();

            JArray sbpMaxMin = JArray.FromObject(pieInfo[0]["sbpMaxMin"]);
            drawBpPieChat.sbpMaxMin = sbpMaxMin.ToObject<List<int>>();

            JArray sbpMaxMinPercent = JArray.FromObject(pieInfo[0]["sbpMaxMinPercent"]);
            drawBpPieChat.sbpMaxMinPercent = sbpMaxMinPercent.ToObject<List<double>>();

            JArray dbpMaxMin = JArray.FromObject(pieInfo[0]["dbpMaxMin"]);
            drawBpPieChat.dbpMaxMin = dbpMaxMin.ToObject<List<int>>();


            JArray dbpMaxMinPercent = JArray.FromObject(pieInfo[0]["dbpMaxMinPercent"]);
            drawBpPieChat.dbpMaxMinPercent = dbpMaxMinPercent.ToObject<List<double>>();

            JArray pulseMaxMin = JArray.FromObject(pieInfo[0]["pulseMaxMin"]);
            drawBpPieChat.pulseMaxMin = pulseMaxMin.ToObject<List<int>>();

            JArray pulseMaxMinPercent = JArray.FromObject(pieInfo[0]["pulseMaxMinPercent"]);
            drawBpPieChat.pulseMaxMinPercent = pulseMaxMinPercent.ToObject<List<double>>();

            drawBpPieChat.FontScaleFactor = FontScaleFactor;
            drawBpPieChat.WaveLeft = ScaleX(MarginLeftMm + 16);
            drawBpPieChat.WaveTop = ScaleY(MarginTopMm + 8);
            drawBpPieChat.WaveWidth = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm - 32);
            drawBpPieChat.WaveHeight = ScaleY(PaperHeightMm - MarginTopMm - MarginBottonMm - 40);
            drawBpPieChat.DrawCoordinate(canvas);
            drawBpPieChat.DrawPieChat(canvas);
        }

        private void DrawFitPage(SKCanvas canvas, int pageNumber)
        {
            List<JObject> fitInfo = FitInfo[0];

            DrawPageHeader(canvas, pageNumber);
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 14f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                canvas.DrawText(CommonValues.Langs["拟合图"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 5), SKTextAlign.Center, titleFont, textPaint);
            }

            DrawBpFitPlot drawBpFitPlot = new DrawBpFitPlot();
            drawBpFitPlot.settingValues = SettingValues;

            JArray DaySbpList = JArray.FromObject(fitInfo[0]["DaySbpList"]);
            drawBpFitPlot.DaySbpList = DaySbpList.ToObject<List<double>>();

            JArray DayDbpList = JArray.FromObject(fitInfo[0]["DayDbpList"]);
            drawBpFitPlot.DayDbpList = DayDbpList.ToObject<List<double>>();

            JArray DayPulseList = JArray.FromObject(fitInfo[0]["DayPulseList"]);
            drawBpFitPlot.DayPulseList = DayPulseList.ToObject<List<int>>();


            JArray NightSbpList = JArray.FromObject(fitInfo[0]["NightSbpList"]);
            drawBpFitPlot.NightSbpList = NightSbpList.ToObject<List<double>>();

            JArray NightDbpList = JArray.FromObject(fitInfo[0]["NightDbpList"]);
            drawBpFitPlot.NightDbpList = NightDbpList.ToObject<List<double>>();

            JArray NightPulseList = JArray.FromObject(fitInfo[0]["NightPulseList"]);
            drawBpFitPlot.NightPulseList = NightPulseList.ToObject<List<int>>();

            JArray SbpSlopePoints = JArray.FromObject(fitInfo[0]["SbpSlopePoints"]);
            drawBpFitPlot.SbpSlopePoints = SbpSlopePoints.ToObject<List<SKPoint>>();

            JArray PulseSlopePoints = JArray.FromObject(fitInfo[0]["PulseSlopePoints"]);
            drawBpFitPlot.PulseSlopePoints = PulseSlopePoints.ToObject<List<SKPoint>>();

            drawBpFitPlot.FontScaleFactor = FontScaleFactor;
            drawBpFitPlot.WaveLeft = ScaleX(MarginLeftMm + 16);
            drawBpFitPlot.WaveTop = ScaleY(MarginTopMm + 8);
            drawBpFitPlot.WaveWidth = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm - 32);
            drawBpFitPlot.WaveHeight = ScaleY(PaperHeightMm - MarginTopMm - MarginBottonMm - 40);
            drawBpFitPlot.CoorPaddingLeft = 10 * PixMM;
            drawBpFitPlot.CoorPaddingRight = 30 * PixMM;
            drawBpFitPlot.CoorPaddingTop = 10 * PixMM;
            drawBpFitPlot.CoorPaddingBottom = 5 * PixMM;
            drawBpFitPlot.DrawCoordinate(canvas);
            drawBpFitPlot.DrawFitPlot(canvas);
        }
        private void DrawArteriosclerosisPage(SKCanvas canvas, int pageNumber)
        {
            DrawPatientInfo(canvas);

            List<JObject> arteriosclerosisInfo = ArteriosclerosisInfo[0];
            JObject allInfoObj = arteriosclerosisInfo[0];
            JObject dayInfoObj = arteriosclerosisInfo[1];
            JObject nightInfoObj = arteriosclerosisInfo[2];

            DrawBpTrendWave drawBpTrendWave = new DrawBpTrendWave();
            drawBpTrendWave.WaveLeft = ScaleX(MarginLeftMm + 8);
            drawBpTrendWave.WaveTop = ScaleY(MarginTopMm + 106);
            drawBpTrendWave.WaveWidth = ScaleX(PaperWidthMm - MarginRightMm - MarginLeftMm - 16);
            drawBpTrendWave.WaveHeight = ScaleY(60);

            drawBpTrendWave.settingValues = SettingValues;
            JArray arr = JArray.FromObject(arteriosclerosisInfo[3]["bpDataInfoList"]);
            drawBpTrendWave.BPDataInfoList = arr.ToObject<List<BPDataInfo>>();
            drawBpTrendWave.FontScaleFactor = FontScaleFactor;
            drawBpTrendWave.DrawNightRect(canvas);
            drawBpTrendWave.DrawTrendWave(canvas);
            drawBpTrendWave.DrawMaxMinLine(canvas);
            drawBpTrendWave.DrawCoordinate(canvas);

            DrawPageHeader(canvas, pageNumber);
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont textFont = new SKFont
            {
                Size = 12.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 14f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.6f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            using (SKPaint rectPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,
                Color = SKColors.Black,
            })
            {
                double cellHeightMm = 6;
                double baseInfoMm = MarginTopMm + 56;
                int infoCells = 7;
                canvas.DrawText(CommonValues.Langs["动脉硬化指数分析报告"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 5), SKTextAlign.Center, titleFont, textPaint);
                canvas.DrawLine(ScaleX(MarginLeftMm), ScaleY(MarginTopMm + 40), ScaleX(MarginLeftMm), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm), ScaleY(MarginTopMm + 40), ScaleX(PaperWidthMm - MarginRightMm), ScaleY(MarginTopMm + 40), linePaint);
                canvas.DrawLine(ScaleX(PaperWidthMm - MarginRightMm), ScaleY(MarginTopMm + 40), ScaleX(PaperWidthMm - MarginRightMm), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm), ScaleY(baseInfoMm + cellHeightMm * infoCells), ScaleX(PaperWidthMm - MarginRightMm), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm), ScaleY(MarginTopMm + 48), ScaleX(PaperWidthMm - MarginRightMm), ScaleY(MarginTopMm + 48), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm), ScaleY(baseInfoMm), ScaleX(PaperWidthMm - MarginRightMm), ScaleY(baseInfoMm), linePaint);
                for (int i = 1; i < 8; i++)
                {
                    canvas.DrawLine(ScaleX(MarginLeftMm), ScaleY(baseInfoMm + i * cellHeightMm), ScaleX(PaperWidthMm - MarginRightMm), ScaleY(baseInfoMm + i * cellHeightMm), linePaint);
                }
                double singleCellMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) / 11d;
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 2), ScaleY(MarginTopMm + 40), ScaleX(MarginLeftMm + singleCellMm * 2), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 5), ScaleY(MarginTopMm + 40), ScaleX(MarginLeftMm + singleCellMm * 5), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 8), ScaleY(MarginTopMm + 40), ScaleX(MarginLeftMm + singleCellMm * 8), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);

                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 3), ScaleY(MarginTopMm + 48), ScaleX(MarginLeftMm + singleCellMm * 3), ScaleY(baseInfoMm), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 3), ScaleY(baseInfoMm + cellHeightMm), ScaleX(MarginLeftMm + singleCellMm * 3), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);

                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 4), ScaleY(MarginTopMm + 48), ScaleX(MarginLeftMm + singleCellMm * 4), ScaleY(baseInfoMm), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 4), ScaleY(baseInfoMm + cellHeightMm), ScaleX(MarginLeftMm + singleCellMm * 4), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);

                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 6), ScaleY(MarginTopMm + 48), ScaleX(MarginLeftMm + singleCellMm * 6), ScaleY(baseInfoMm), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 6), ScaleY(baseInfoMm + cellHeightMm), ScaleX(MarginLeftMm + singleCellMm * 6), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);

                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 7), ScaleY(MarginTopMm + 48), ScaleX(MarginLeftMm + singleCellMm * 7), ScaleY(baseInfoMm), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 7), ScaleY(baseInfoMm + cellHeightMm), ScaleX(MarginLeftMm + singleCellMm * 7), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);

                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 9), ScaleY(MarginTopMm + 48), ScaleX(MarginLeftMm + singleCellMm * 9), ScaleY(baseInfoMm), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 9), ScaleY(baseInfoMm + cellHeightMm), ScaleX(MarginLeftMm + singleCellMm * 9), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);

                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 10), ScaleY(MarginTopMm + 48), ScaleX(MarginLeftMm + singleCellMm * 10), ScaleY(baseInfoMm), linePaint);
                canvas.DrawLine(ScaleX(MarginLeftMm + singleCellMm * 10), ScaleY(baseInfoMm + cellHeightMm), ScaleX(MarginLeftMm + singleCellMm * 10), ScaleY(baseInfoMm + cellHeightMm * infoCells), linePaint);

                canvas.DrawText(CommonValues.Langs["白天清醒时段"], ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(MarginTopMm + 43.5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["duration"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(MarginTopMm + 47), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["夜间睡眠时段"], ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(MarginTopMm + 43.5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["duration"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(MarginTopMm + 47), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["24小时"], ScaleX(MarginLeftMm + singleCellMm * 9.5), ScaleY(MarginTopMm + 45), SKTextAlign.Center, textFont, textPaint);
                for (int i = 0; i < 9; i++)
                {
                    string titleText = "";
                    string unitText = "(mmHg)";
                    if (i % 3 == 0)
                    {
                        titleText = CommonValues.Langs["收缩压"];
                    }
                    else if (i % 3 == 1)
                    {
                        titleText = CommonValues.Langs["舒张压"];

                    }
                    else
                    {
                        titleText = CommonValues.Langs["脉率"];
                        unitText = CommonValues.Langs["(次/分)"];
                    }

                    canvas.DrawText(titleText, ScaleX(MarginLeftMm + singleCellMm * (2.5 + i)), ScaleY(MarginTopMm + 51.5), SKTextAlign.Center, textFont, textPaint);
                    canvas.DrawText(unitText, ScaleX(MarginLeftMm + singleCellMm * (2.5 + i)), ScaleY(MarginTopMm + 55), SKTextAlign.Center, textFont, textPaint);
                }
                canvas.DrawText(CommonValues.Langs["有效数据百分比"] + "(%)", ScaleX(MarginLeftMm + singleCellMm), ScaleY(baseInfoMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["effectiveCountRate"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(baseInfoMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["effectiveCountRate"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(baseInfoMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["effectiveCountRate"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 9.5), ScaleY(baseInfoMm + 5), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["最大值"], ScaleX(MarginLeftMm + singleCellMm), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["SbpMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 2.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["SbpMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 5.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["SbpMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 8.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["DbpMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["DbpMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["DbpMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 9.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["PulseMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 4.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["PulseMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 7.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["PulseMax"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 10.5), ScaleY(baseInfoMm + cellHeightMm + 5), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["最小值"], ScaleX(MarginLeftMm + singleCellMm), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["SbpMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 2.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["SbpMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 5.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["SbpMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 8.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["DbpMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["DbpMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["DbpMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 9.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["PulseMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 4.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["PulseMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 7.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["PulseMin"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 10.5), ScaleY(baseInfoMm + cellHeightMm * 2 + 5), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["平均值"], ScaleX(MarginLeftMm + singleCellMm), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["SbpAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 2.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["SbpAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 5.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["SbpAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 8.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["DbpAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["DbpAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["DbpAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 9.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["PulseAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 4.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["PulseAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 7.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["PulseAvg"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 10.5), ScaleY(baseInfoMm + cellHeightMm * 3 + 5), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["标准差"], ScaleX(MarginLeftMm + singleCellMm), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["SbpBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 2.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["SbpBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 5.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["SbpBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 8.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["DbpBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["DbpBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["DbpBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 9.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["PulseBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 4.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["PulseBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 7.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["PulseBzc"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 10.5), ScaleY(baseInfoMm + cellHeightMm * 4 + 5), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["变异系数"] + "(%)", ScaleX(MarginLeftMm + singleCellMm), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["SbpByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 2.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["SbpByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 5.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["SbpByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 8.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["DbpByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["DbpByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["DbpByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 9.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["PulseByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 4.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["PulseByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 7.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["PulseByxs"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 10.5), ScaleY(baseInfoMm + cellHeightMm * 5 + 5), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["血压负荷"] + "(%)", ScaleX(MarginLeftMm + singleCellMm), ScaleY(baseInfoMm + cellHeightMm * 6 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["SbpLoadNum"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 2.5), ScaleY(baseInfoMm + cellHeightMm * 6 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["SbpLoadNum"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 5.5), ScaleY(baseInfoMm + cellHeightMm * 6 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["SbpLoadNum"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 8.5), ScaleY(baseInfoMm + cellHeightMm * 6 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(dayInfoObj["DbpLoadNum"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 3.5), ScaleY(baseInfoMm + cellHeightMm * 6 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(nightInfoObj["DbpLoadNum"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 6.5), ScaleY(baseInfoMm + cellHeightMm * 6 + 5), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawText(allInfoObj["DbpLoadNum"]?.ToString(), ScaleX(MarginLeftMm + singleCellMm * 9.5), ScaleY(baseInfoMm + cellHeightMm * 6 + 5), SKTextAlign.Center, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["ABPM 血压相关图(24小时)"], ScaleX(MarginLeftMm + 8 + 58), ScaleY(MarginTopMm + 176), SKTextAlign.Center, textFont, textPaint);
                canvas.DrawRect(ScaleX(MarginLeftMm + 8), ScaleY(MarginTopMm + 178), 116 * PixMM, 80 * PixMM, rectPaint);


                PrintBpFitPlot printBpFitPlot = new PrintBpFitPlot();
                printBpFitPlot.settingValues = SettingValues;

                JArray DaySbpList = JArray.FromObject(arteriosclerosisInfo[3]["DaySbpList"]);
                printBpFitPlot.DaySbpList = DaySbpList.ToObject<List<double>>();

                JArray DayDbpList = JArray.FromObject(arteriosclerosisInfo[3]["DayDbpList"]);
                printBpFitPlot.DayDbpList = DayDbpList.ToObject<List<double>>();

                JArray NightSbpList = JArray.FromObject(arteriosclerosisInfo[3]["NightSbpList"]);
                printBpFitPlot.NightSbpList = NightSbpList.ToObject<List<double>>();

                JArray NightDbpList = JArray.FromObject(arteriosclerosisInfo[3]["NightDbpList"]);
                printBpFitPlot.NightDbpList = NightDbpList.ToObject<List<double>>();

                JArray SbpSlopePoints = JArray.FromObject(arteriosclerosisInfo[3]["SbpSlopePoints"]);
                printBpFitPlot.SbpSlopePoints = SbpSlopePoints.ToObject<List<SKPoint>>();

                printBpFitPlot.FontScaleFactor = FontScaleFactor;
                printBpFitPlot.WaveLeft = ScaleX(MarginLeftMm + 8);
                printBpFitPlot.WaveTop = ScaleY(MarginTopMm + 178);
                printBpFitPlot.WaveWidth = ScaleX(116);
                printBpFitPlot.WaveHeight = ScaleY(80);
                printBpFitPlot.CoorPaddingLeft = 0;
                printBpFitPlot.CoorPaddingRight = 0;
                printBpFitPlot.CoorPaddingTop = 0;
                printBpFitPlot.CoorPaddingBottom = 0;
                printBpFitPlot.DrawCoordinate(canvas);
                printBpFitPlot.DrawFitPlot(canvas);



                canvas.DrawText(CommonValues.Langs["注：ABPM：动态血压监测；SBP：收缩压；DBP：舒张压。1 mmHg = 0.133 kPa"], ScaleX(MarginLeftMm + 8), ScaleY(MarginTopMm + 268), SKTextAlign.Left, textFont, textPaint);

                canvas.DrawText("AASI: " + allInfoObj["AASI"]?.ToString(), ScaleX(MarginLeftMm + 128), ScaleY(MarginTopMm + 184), SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["有效测量次数"] + ": " + allInfoObj["effectiveCount"]?.ToString(), ScaleX(MarginLeftMm + 128), ScaleY(MarginTopMm + 190), SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["监测时长"] + ": " + allInfoObj["TotalTime"]?.ToString(), ScaleX(MarginLeftMm + 128), ScaleY(MarginTopMm + 196), SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(allInfoObj["RelationShip"]?.ToString(), ScaleX(MarginLeftMm + 128), ScaleY(MarginTopMm + 202), SKTextAlign.Left, textFont, textPaint);

                string htmlStr = CommonValues.Langs["SBP和DBP呈高度线性正相关性"];
                if (Convert.ToDouble(allInfoObj["Correl"]) <= 0.5)
                {
                    htmlStr = CommonValues.Langs["SBP和DBP线性正相关性较低"];
                }
                canvas.DrawText(CommonValues.Langs["SBP和DBP的线性相关性:"] + " r= " + allInfoObj["Correl"]?.ToString(), ScaleX(MarginLeftMm + 128), ScaleY(MarginTopMm + 208), SKTextAlign.Left, textFont, textPaint);

                canvas.DrawText(htmlStr, ScaleX(MarginLeftMm + 128), ScaleY(MarginTopMm + 214), SKTextAlign.Left, textFont, textPaint);



            }


        }
        private void DrawPageHeader(SKCanvas canvas, int pageNumber)
        {
            using (SKFont textFont = new SKFont
            {
                Size = 12.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont copyRightFont = new SKFont
            {
                Size = 9.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体"),
                Edging = SKFontEdging.SubpixelAntialias
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.6f,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

                //患者姓名
                string nameText = CommonValues.Langs["姓名"] + $": {AnaValues.CurrentCase.name}";
                canvas.DrawText(nameText, ScaleX(MarginLeftMm),
                ScaleY(MarginTopMm - 2), SKTextAlign.Left, textFont, textPaint);
                string recordTimeText = CommonValues.Langs["记录时间"] + $": {AnaValues.CurrentCase.recordTime?.ToString(dateFormat)}";
                canvas.DrawText(recordTimeText, ScaleX(PaperWidthMm - MarginRightMm),
                ScaleY(MarginTopMm - 2), SKTextAlign.Right, textFont, textPaint);
                canvas.DrawLine(ScaleX(MarginLeftMm), ScaleY(MarginTopMm - 1), ScaleX(PaperWidthMm - MarginRightMm), ScaleY(MarginTopMm - 1), linePaint);

                if (CommonValues.SystemSettings.reportSets.displayPageNum == 1)
                {
                    // 绘制页码
                    int pageNum = pageNumber + 1;
                    string footerText = $"{pageNum}/{TotalPages}";

                    canvas.DrawText(footerText, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(PaperHeightMm - MarginBottonMm) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

            }
        }
        public void DrawSun(SKCanvas canvas, float cx, float cy, float radius)
        {
            SKPaint fill = new SKPaint { Color = SKColors.DarkGray, IsAntialias = true, Style = SKPaintStyle.Fill };
            SKPaint line = new SKPaint { Color = SKColors.DarkGray, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = radius * 0.15f, StrokeCap = SKStrokeCap.Round };

            // 中间圆
            canvas.DrawCircle(cx, cy, radius, fill);

            // 8 条光芒
            int rays = 8;
            float inner = radius * 1.3f;
            float outer = radius * 1.8f;
            for (int i = 0; i < rays; i++)
            {
                float a = i * (360f / rays) * (float)Math.PI / 180;
                float cos = (float)Math.Cos(a), sin = (float)Math.Sin(a);
                canvas.DrawLine(cx + inner * cos, cy + inner * sin,
                                cx + outer * cos, cy + outer * sin, line);
            }
        }

        public void DrawMoon(SKCanvas canvas, float cx, float cy, float radius)
        {
            SKPaint paint = new SKPaint { Color = SKColors.DarkGray, IsAntialias = true, Style = SKPaintStyle.Fill };

            SKPath path = new SKPath();
            // 月牙的两个尖角在圆的上下两端
            var top = new SKPoint(cx, cy - radius);
            var bottom = new SKPoint(cx, cy + radius);


            // 外弧：右半圆（从左下尖角，经右侧，到上方尖角）
            path.MoveTo(bottom.X, bottom.Y);
            path.ArcTo(
                new SKRect(cx - radius, cy - radius, cx + radius, cy + radius),
                90,    // 起始角（正下方）
                -180,  // 扫过 180 度，经右侧到正上方
                false);

            // 内弧：向左凹进去，形成月牙的缺口
            // 内弧半径越大、圆心越靠左，月牙越细
            float innerRadius = radius * 1.1f;
            float innerCx = cx - radius * 0.5f;   // 内弧圆心向左偏移

            path.ArcTo(
                new SKRect(innerCx - innerRadius, cy - innerRadius,
                           innerCx + innerRadius, cy + innerRadius),
                -90,   // 从正上方开始
                180,   // 扫回正下方
                false);

            path.Close();
            canvas.DrawPath(path, paint);
        }

        private void GetSettings()
        {
            MaxWakeSys = SettingValues.maxWakeSys;
            MinWakeSys = SettingValues.minWakeSys;
            MaxSleepSys = SettingValues.maxSleepSys;
            MinSleepSys = SettingValues.minSleepSys;
            MaxErrorSys = SettingValues.maxErrorSys;
            MinErrorSys = SettingValues.minErrorSys;

            MaxWakeDia = SettingValues.maxWakeDia;
            MinWakeDia = SettingValues.minWakeDia;
            MaxSleepDia = SettingValues.maxSleepDia;
            MinSleepDia = SettingValues.minSleepDia;
            MaxErrorDia = SettingValues.maxErrorDia;
            MinErrorDia = SettingValues.minErrorDia;
            if (SettingValues.unit == "kPa")
            {
                MaxWakeSys = CommonFuns.mmHg2KPa(Convert.ToInt32(MaxWakeSys), 1);
                MinWakeSys = CommonFuns.mmHg2KPa(Convert.ToInt32(MinWakeSys), 1);

                MaxSleepSys = CommonFuns.mmHg2KPa(Convert.ToInt32(MaxSleepSys), 1);
                MinSleepSys = CommonFuns.mmHg2KPa(Convert.ToInt32(MinSleepSys), 1);

                MaxErrorSys = CommonFuns.mmHg2KPa(Convert.ToInt32(MaxErrorSys), 1);
                MinErrorSys = CommonFuns.mmHg2KPa(Convert.ToInt32(MinErrorSys), 1);

                MaxWakeDia = CommonFuns.mmHg2KPa(Convert.ToInt32(MaxWakeDia), 1);
                MinWakeDia = CommonFuns.mmHg2KPa(Convert.ToInt32(MinWakeDia), 1);

                MaxSleepDia = CommonFuns.mmHg2KPa(Convert.ToInt32(MaxSleepDia), 1);
                MinSleepDia = CommonFuns.mmHg2KPa(Convert.ToInt32(MinSleepDia), 1);

                MaxErrorDia = CommonFuns.mmHg2KPa(Convert.ToInt32(MaxErrorDia), 1);
                MinErrorDia = CommonFuns.mmHg2KPa(Convert.ToInt32(MinErrorDia), 1);
            }
        }
    }
}
