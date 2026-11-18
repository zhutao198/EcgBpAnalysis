using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Zoncare.Common;
using Zoncare.DAL;
using static AnalysisApp.CommonControls.STTrendWaveControl;
using static AnalysisApp.Commons.AEcgHandler;

namespace AnalysisApp.Commons
{
    internal class PrintReportPage
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

        public int STListIdx = 0;   //st事件列表从报告的第几页开始
        public int AFListIdx = 0;   //房颤/房扑事件列表从报告的第几页开始

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

        public string advice = "";

        private readonly SKColor _gridColor = SKColor.Parse("#db634f");
        private readonly SKColor _minorGridColor = SKColor.Parse("#f3aca0");
        private readonly SKColor _waveColor = new SKColor(0, 0, 0);
        private readonly SKColor _textColor = new SKColor(0, 0, 0);
        private readonly SKColor _backgroundColor = new SKColor(255, 255, 255);

        private readonly ecgSettingValues EcgSets = CommonValues.SystemSettings.ecgSets;
        private readonly reportSettingValues ReportSets = CommonValues.SystemSettings.reportSets;

        //总页数
        public int TotalPages;

        private double colWidthMm;

        private double subColWidthShortMm;
        private double subColWidthLongMm;

        public List<List<JObject>> SummaryInfo;
        public List<List<JObject>> HRVTimeDomainInfo;
        public List<List<JObject>> HRVFrequencyDomainInfo;
        public List<List<JObject>> HRVAnalysisInfo;
        public List<List<JObject>> STListInfo;
        public List<List<JObject>> STTrendWaveInfo;
        public List<List<JObject>> TTrendWaveInfo;
        public List<List<JObject>> AFListInfo;
        public List<List<JObject>> PaceListInfo;


        //每页中每个片段的网格区域
        public SKRect WaveRect { get; set; } = new SKRect();

        public Bitmap RenderReportPage(int pageNum, string pageName)
        {
            //List<JObject> SegmentPageInfo = SegmentAllPageInfo[pageNum];
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
            subColWidthShortMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) * 0.3;
            subColWidthLongMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) * 0.35;
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
                    case "hourlist":
                        DrawHourlistPage(canvas, pageNum);
                        break;
                    case "HRVTimeDomain":
                        DrawHRVTimeDomainPage(canvas, pageNum);
                        break;
                    case "HRVFrequencyDomain":
                        DrawHRVFrequencyDomainPage(canvas, pageNum);
                        break;
                    case "HRVAnalysis":
                        DrawHRVAnalysisPage(canvas, pageNum);
                        break;
                    case "STList":
                        DrawSTListPage(canvas, pageNum);
                        break;
                    case "STTrendWave":
                        DrawSTTrendWavePage(canvas, pageNum);
                        break;
                    case "TTrendWave":
                        DrawTTrendWavePage(canvas, pageNum);
                        break;
                    case "AFList":
                        DrawAFListPage(canvas, pageNum);
                        break;
                    case "pace":
                        DrawPaceListPage(canvas, pageNum);
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
            JObject summary = SummaryInfo[0][0];
            bool bPrintHrvData = (summary["bPrintHrvData"]?.Value<bool>()).Value;

            subColWidthShortMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) * 0.3;

            DrawSummaryPageHeader(canvas, CommonValues.Langs["动态心电报告"], pageNumber);
            DrawPatientInfo(canvas);
            DrawSummaryHRInfo(canvas, summary);
            DrawSummaryVInfo(canvas, summary);
            DrawSummarySInfo(canvas, summary);
            DrawSummaryHRVInfo(canvas, summary, bPrintHrvData);
            DrawSummarySTInfo(canvas, summary, bPrintHrvData);
            DrawSummaryAFInfo(canvas, summary, bPrintHrvData);
            DrawSummaryLines(canvas, bPrintHrvData);

            DrawSummaryAdviceInfo(canvas);

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
                StrokeWidth = 0.5f,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                string acceptFlag = CommonValues.SystemSettings.reportSets.acceptFlag;
                if(acceptFlag != "")
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
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
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

        private void DrawSummaryHRInfo(SKCanvas canvas, JObject summary)
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
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                canvas.DrawText(CommonValues.Langs["心率"], (float)(MarginLeftMm + (subColWidthShortMm / 2)) * PixMM,
                (float)(MarginTopMm + 44) * PixMM, SKTextAlign.Center, titleFont, textPaint);
                canvas.DrawText(CommonValues.Langs["总心搏数"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["TotalBeats"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                (float)(MarginTopMm + 49) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["平均心率"] + "(bpm)", (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 55.5) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["AvgHR"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                (float)(MarginTopMm + 55.5) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["最快心率"] + "(bpm)", (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 62) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["MaxHR"]}于{summary["DateStringMaxHR"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                (float)(MarginTopMm + 62) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["最慢心率"] + "(bpm)", (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 68) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["MinHR"]}于{summary["DateStringMinHR"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                (float)(MarginTopMm + 68) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["停搏数"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 74) * PixMM, SKTextAlign.Left, textFont, textPaint);
                if(Convert.ToInt32(summary["AsystoleCount"]) > 0)
                {
                    canvas.DrawText($"{summary["AsystoleCount"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                    (float)(MarginTopMm + 74) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

                canvas.DrawText(CommonValues.Langs["最长停搏"] + "(s)", (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 80) * PixMM, SKTextAlign.Left, textFont, textPaint);
                if(Convert.ToInt32(summary["AsystoleCount"]) >0)
                {
                    canvas.DrawText($"{summary["MaxAsystoleStr"]}于{summary["DateStringMaxAsystole"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                    (float)(MarginTopMm + 80) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

                canvas.DrawText(CommonValues.Langs["心动过速"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 86) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["Tachycardia"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                (float)(MarginTopMm + 86) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["心动过缓"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 92) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["Bradycardia"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                (float)(MarginTopMm + 92) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["记录时长"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 98) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["RecordTimeStr"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                (float)(MarginTopMm + 98) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["分析时长"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 104) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["AnaTimeStr"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                (float)(MarginTopMm + 104) * PixMM, SKTextAlign.Right, textFont, textPaint);

                //canvas.DrawText(CommonValues.Langs["分析通道"], (float)((MarginLeftMm + 1) * PixMM),
                //(float)(MarginTopMm + 110) * PixMM, SKTextAlign.Left, textFont, textPaint);
                //canvas.DrawText($"{summary["AnaLeadStr"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
                //(float)(MarginTopMm + 110) * PixMM, SKTextAlign.Right, textFont, textPaint);

            }

        }

        private void DrawSummaryVInfo(SKCanvas canvas, JObject summary)
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
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                canvas.DrawText(CommonValues.Langs["室性节律"], (float)(MarginLeftMm + subColWidthShortMm + (subColWidthLongMm / 2)) * PixMM,
                (float)(MarginTopMm + 44) * PixMM, SKTextAlign.Center, titleFont, textPaint);
                canvas.DrawText(CommonValues.Langs["总数(次)/总占比(%)"], (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                double precentV = Math.Round((double)Convert.ToInt32(summary["TotalBeatsV"]) / Convert.ToInt32(summary["TotalBeats"])*100, 2);
                canvas.DrawText($"{summary["TotalBeatsV"]}/{precentV}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 49) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["单发"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 55.5) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["SingleV"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 55.5) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["成对"] + "(" + CommonValues.Langs["阵"] + ")", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 62) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["PairedV"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 62) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["二联律"] + "(" + CommonValues.Langs["阵"] + ")", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 68) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["BigeminyV"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 68) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["三联律"] + "(" + CommonValues.Langs["阵"] + ")", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 74) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["TrigeminyV"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 74) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["室速"] + "(" + CommonValues.Langs["阵"] + ")", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 80) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["TachycardiaV"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 80) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["最长室速"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 86) * PixMM, SKTextAlign.Left, textFont, textPaint);
                int longestTachycardiaV = Convert.ToInt32(summary["LongestTachycardiaV"]);
                if(longestTachycardiaV > 0)
                {
                    canvas.DrawText($"{summary["LongestTachycardiaV"]}于{summary["DateTimeLongestTachycardiaV"]}持续{summary["LongestTachycardiaDurationV"]}秒", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                    (float)(MarginTopMm + 86) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

                canvas.DrawText(CommonValues.Langs["最快室速"] + "(bpm)", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 92) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["MaxHRV"]}于{summary["DateTimeMaxHRV"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 92) * PixMM, SKTextAlign.Right, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["最慢室速"] + "(bpm)", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 98) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["MinHRV"]}于{summary["DateTimeMinHRV"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 98) * PixMM, SKTextAlign.Right, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["室性逸搏"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 104) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["EscapeV"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 104) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText("R on T" + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM),
                (float)(MarginTopMm + 110) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["RonT"]}", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM),
                (float)(MarginTopMm + 110) * PixMM, SKTextAlign.Right, textFont, textPaint);

            }
        }

        private void DrawSummarySInfo(SKCanvas canvas, JObject summary)
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
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                canvas.DrawText(CommonValues.Langs["房性节律"], (float)(MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 1.5)) * PixMM,
                (float)(MarginTopMm + 44) * PixMM, SKTextAlign.Center, titleFont, textPaint);
                canvas.DrawText(CommonValues.Langs["总数(次)/总占比(%)"], (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                double precentS = Math.Round((double)Convert.ToInt32(summary["TotalBeatsS"]) / Convert.ToInt32(summary["TotalBeats"]) * 100, 2);
                canvas.DrawText($"{summary["TotalBeatsS"]}/{precentS}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 49) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["单发"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 55.5) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["SingleS"]}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 55.5) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["成对"] + "(" + CommonValues.Langs["阵"] + ")", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 62) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["PairedS"]}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 62) * PixMM, SKTextAlign.Right, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["二联律"] + "(" + CommonValues.Langs["阵"] + ")", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 68) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["BigeminyS"]}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 68) * PixMM, SKTextAlign.Right, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["三联律"] + "(" + CommonValues.Langs["阵"] + ")", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 74) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["TrigeminyS"]}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 74) * PixMM, SKTextAlign.Right, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["房速"] + "(" + CommonValues.Langs["阵"] + ")", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 80) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["TachycardiaS"]}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 80) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["最长房速"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 86) * PixMM, SKTextAlign.Left, textFont, textPaint);
                int longestTachycardiaS = Convert.ToInt32(summary["LongestTachycardiaS"]);
                if(longestTachycardiaS > 0)
                {
                    canvas.DrawText(summary["LongestTachycardiaS"] + CommonValues.Langs["于"] + summary["DateTimeLongestTachycardiaS"] + CommonValues.Langs["持续"] + summary["LongestTachycardiaDurationS"] + CommonValues.Langs["秒"], (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                    (float)(MarginTopMm + 86) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

                canvas.DrawText(CommonValues.Langs["最快房速"] + "(bpm)", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 92) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["MaxHRS"]}于{summary["DateTimeMaxHRS"]}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 92) * PixMM, SKTextAlign.Right, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["最慢房速"] + "(bpm)", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 98) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["MinHRS"]}于{summary["DateTimeMinHRS"]}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 98) * PixMM, SKTextAlign.Right, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["房性逸搏"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM),
                (float)(MarginTopMm + 104) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["EscapeS"]}", (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM),
                (float)(MarginTopMm + 104) * PixMM, SKTextAlign.Right, textFont, textPaint);

            }
        }

        private void DrawSummaryHRVInfo(SKCanvas canvas, JObject summary, bool bPrintHrvData)
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
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;
                if (bPace)
                {
                    DrawPaceInfo(canvas, summary, titleFont, textFont, textPaint);
                }
                else
                {
                    if (bPrintHrvData)
                    {
                        DrawHRVInfo(canvas, summary, titleFont, textFont, textPaint);
                    }
                }
            }
        }

        private void DrawSummarySTInfo(SKCanvas canvas, JObject summary, bool bPrintHrvData)
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
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                float titleX = 0f;
                float leftX = 0f;
                float rightX = 0f;
                bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;
                if (bPrintHrvData || bPace)
                {
                    titleX = (float)(MarginLeftMm + subColWidthShortMm + (subColWidthLongMm / 2)) * PixMM;
                    leftX = (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM);
                    rightX = (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm - 1) * PixMM);
                }
                else
                {
                    subColWidthShortMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) * 0.5;
                    titleX = (float)(MarginLeftMm + (subColWidthShortMm / 2)) * PixMM;
                    leftX = (float)((MarginLeftMm + 1) * PixMM);
                    rightX = (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM);
                }
                canvas.DrawText(CommonValues.Langs["ST段分析"], titleX,
                (float)(MarginTopMm + 116) * PixMM, SKTextAlign.Center, titleFont, textPaint);

                canvas.DrawText(CommonValues.Langs["压低事件总数"] + "(" + CommonValues.Langs["阵"] + ")", leftX,
                (float)(MarginTopMm + 121) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["stLowCount"]}", rightX,
                (float)(MarginTopMm + 121) * PixMM, SKTextAlign.Right, textFont, textPaint);

                int stLowCount = Convert.ToInt32(summary["stLowCount"]);
                canvas.DrawText(CommonValues.Langs["最大压低幅值"] + "(mV)", leftX,
                (float)(MarginTopMm + 128) * PixMM, SKTextAlign.Left, textFont, textPaint);
                string lowest = stLowCount > 0 ? $"{summary["stLowest"]}于{summary["stLowestTime"]}" : "";
                canvas.DrawText(lowest, rightX,
                (float)(MarginTopMm + 128) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["发生导联"], leftX,
                (float)(MarginTopMm + 134) * PixMM, SKTextAlign.Left, textFont, textPaint);
                if (stLowCount > 0)
                {
                    canvas.DrawText($"{summary["stLowestLead"]}", rightX,
                    (float)(MarginTopMm + 134) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

                canvas.DrawText(CommonValues.Langs["持续时间"] + "(" + CommonValues.Langs["分"] + ")", leftX,
                (float)(MarginTopMm + 140) * PixMM, SKTextAlign.Left, textFont, textPaint);
                if (stLowCount > 0)
                {
                    canvas.DrawText($"{summary["stLowLeadDuration"]}", rightX,
                    (float)(MarginTopMm + 140) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

                canvas.DrawText(CommonValues.Langs["抬高事件总数"] + "(" + CommonValues.Langs["阵"] + ")", leftX,
                (float)(MarginTopMm + 146) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["stUpCount"]}", rightX,
                (float)(MarginTopMm + 146) * PixMM, SKTextAlign.Right, textFont, textPaint);

                int stUpCount = Convert.ToInt32(summary["stUpCount"]);
                canvas.DrawText(CommonValues.Langs["最大抬高幅值"] + "(mV)", leftX,
                (float)(MarginTopMm + 152) * PixMM, SKTextAlign.Left, textFont, textPaint);
                string upest = stUpCount > 0 ? summary["stUpest"] + CommonValues.Langs["于"] + summary["stUpestTime"] : "";
                canvas.DrawText(upest, rightX,
                (float)(MarginTopMm + 152) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["发生导联"], leftX,
                (float)(MarginTopMm + 158) * PixMM, SKTextAlign.Left, textFont, textPaint);
                if (stUpCount > 0)
                {
                    canvas.DrawText($"{summary["stUpestLead"]}", rightX,
                    (float)(MarginTopMm + 158) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

                canvas.DrawText(CommonValues.Langs["持续时间"] + "(" + CommonValues.Langs["分"] + ")", leftX,
                (float)(MarginTopMm + 164) * PixMM, SKTextAlign.Left, textFont, textPaint);
                if (stUpCount > 0)
                {
                    canvas.DrawText($"{summary["stUpLeadDuration"]}", rightX,
                    (float)(MarginTopMm + 164) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }
            }
        }

        private void DrawSummaryAFInfo(SKCanvas canvas, JObject summary, bool bPrintHrvData)
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
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                float titleX = 0f;
                float leftX = 0f;
                float rightX = 0f;
                bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;

                if (bPrintHrvData || bPace)
                {
                    titleX = (float)(MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 1.5)) * PixMM;
                    leftX = (float)((MarginLeftMm + subColWidthShortMm + subColWidthLongMm + 1) * PixMM);
                    rightX = (float)((MarginLeftMm + subColWidthShortMm + (subColWidthLongMm * 2) - 1) * PixMM);
                }
                else
                {
                    subColWidthShortMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) * 0.5;
                    titleX = (float)(MarginLeftMm + subColWidthShortMm + subColWidthShortMm / 2) * PixMM;
                    leftX = (float)((MarginLeftMm + subColWidthShortMm + 1) * PixMM);
                    rightX = (float)((MarginLeftMm + subColWidthShortMm* 2 - 1) * PixMM);
                }

                canvas.DrawText(CommonValues.Langs["房颤/房扑"], titleX,
                (float)(MarginTopMm + 116) * PixMM, SKTextAlign.Center, titleFont, textPaint);

                canvas.DrawText(CommonValues.Langs["房颤"] + CommonValues.Langs["负荷"] + "(%)", leftX,
                (float)(MarginTopMm + 121) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["AFPercent"]}", rightX,
                (float)(MarginTopMm + 121) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["房颤总时长"], leftX,
                (float)(MarginTopMm + 128) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["AFDuration"]}", rightX,
                (float)(MarginTopMm + 128) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["房颤阵数"] + "(" + CommonValues.Langs["阵"] + ")", leftX,
                (float)(MarginTopMm + 134) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["AFCount"]}", rightX,
                (float)(MarginTopMm + 134) * PixMM, SKTextAlign.Right, textFont, textPaint);


                canvas.DrawText(CommonValues.Langs["房扑"] + CommonValues.Langs["负荷"] + "(%)", leftX,
                (float)(MarginTopMm + 140) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["AFPercent2"]}", rightX,
                (float)(MarginTopMm + 140) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["房扑总时长"], leftX,
                (float)(MarginTopMm + 146) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["AFDuration2"]}", rightX,
                (float)(MarginTopMm + 146) * PixMM, SKTextAlign.Right, textFont, textPaint);

                canvas.DrawText(CommonValues.Langs["房扑阵数"] + "(" + CommonValues.Langs["阵"] + ")", leftX,
                (float)(MarginTopMm + 152) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText($"{summary["AFCount2"]}", rightX,
                (float)(MarginTopMm + 152) * PixMM, SKTextAlign.Right, textFont, textPaint);

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
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                canvas.DrawText(CommonValues.Langs["结论"], (float)(PaperWidthMm * PixMM) / 2,
                (float)(MarginTopMm + 188) * PixMM, SKTextAlign.Center, titleFont, textPaint);

                int lineIdx = 0;
                string[] advices = advice.ToString().Split('\n');
                for (int i = 0; i < advices.Length; i++)
                {
                    if (lineIdx > 11) break;
                    List<string> lines = AnaCode.WrapText(advices[i], (float)((PaperWidthMm - MarginLeftMm - MarginRightMm) * PixMM), textFont);
                    foreach (string line in lines)
                    {
                        if (lineIdx > 11) break;

                        float y = (float)(MarginTopMm + 192 + (6 * lineIdx)) * PixMM;
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
                    DrawReportSignInfo(canvas, CommonValues.Langs["报告医生"], diagnoseSign, (PaperWidthMm/3) * PixMM, (PaperWidthMm/3+15) * PixMM, (PaperWidthMm / 3 + 15) * PixMM, textFont, textPaint, linePaint);
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

        private void DrawSummaryLines(SKCanvas canvas, bool bPrintHrvData)
        {
            bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;

            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)MarginLeftMm * PixMM, (float)(MarginTopMm + 180) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, linePaint);
                canvas.DrawLine((float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 180) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 180) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 180) * PixMM, linePaint);

                if(bPrintHrvData || bPace)
                {
                    subColWidthShortMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) * 0.3;

                    canvas.DrawLine((float)(MarginLeftMm + subColWidthShortMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(MarginLeftMm + subColWidthShortMm) * PixMM, (float)(MarginTopMm + 180) * PixMM, linePaint);
                    canvas.DrawLine((float)(MarginLeftMm + subColWidthShortMm + subColWidthLongMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(MarginLeftMm + subColWidthShortMm + subColWidthLongMm) * PixMM, (float)(MarginTopMm + 180) * PixMM, linePaint);
                    canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 112) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 112) * PixMM, linePaint);
                }
                else
                {
                    subColWidthShortMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) * 0.3;

                    canvas.DrawLine((float)(MarginLeftMm + subColWidthShortMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(MarginLeftMm + subColWidthShortMm) * PixMM, (float)(MarginTopMm + 112) * PixMM, linePaint);
                    canvas.DrawLine((float)(MarginLeftMm + subColWidthShortMm + subColWidthLongMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(MarginLeftMm + subColWidthShortMm + subColWidthLongMm) * PixMM, (float)(MarginTopMm + 112) * PixMM, linePaint);
                    canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 112) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 112) * PixMM, linePaint);

                    double colWidthMm = (PaperWidthMm - MarginLeftMm - MarginRightMm) * 0.5;
                    canvas.DrawLine((float)(MarginLeftMm + colWidthMm) * PixMM, (float)(MarginTopMm + 112) * PixMM, (float)(MarginLeftMm + colWidthMm) * PixMM, (float)(MarginTopMm + 180) * PixMM, linePaint);
                }

                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 182) * PixMM, (float)MarginLeftMm * PixMM, (float)(MarginTopMm + 268) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 182) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 182) * PixMM, linePaint);
                canvas.DrawLine((float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 182) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 268) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 268) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 268) * PixMM, linePaint);
            }
        }

        private void DrawHRVTimeDomainInfo(SKCanvas canvas)
        {
            using (SKFont titleFont = new SKFont
            {
                Size = 13.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont textFont = new SKFont
            {
                Size = 13.0f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont discFont = new SKFont
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
            using (SKPaint textRefPaint = new SKPaint
            {
                Color = SKColors.Gray,
                IsAntialias = true
            })
            using (SKPaint linePaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.5f * FontScaleFactor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill
            })
            {
                canvas.DrawText(CommonValues.Langs["心率变异性(HRV)分析总报告"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 3), SKTextAlign.Center, titleFont, textPaint);

                JObject HRVInfo = HRVAnalysisInfo[0][0];

                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)MarginLeftMm * PixMM, (float)(MarginTopMm + 198) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, linePaint);
                canvas.DrawLine((float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 40) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 198) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 198) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 198) * PixMM, linePaint);


                canvas.DrawText(CommonValues.Langs["HRV分析结果"], (float)(PaperWidthMm * PixMM) / 2,
                (float)(MarginTopMm + 44) * PixMM, SKTextAlign.Center, titleFont, textPaint);

                canvas.DrawText("SDNN-24h(ms):" + HRVInfo["sdnn"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 49) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":141 ± 39 ms)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 49) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["R-R间期标准差，反映24h总体心率变异性，包含交感和副交感神经的全部调节信息。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 55) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["SDANN(ms):"] + HRVInfo["sdann"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 63) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":127 ± 35 ms)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 63) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["平均R-R间期标准差，反映长周期（>5min）心率变化，与昼夜节律和神经内分泌节律相关。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 69) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["SDNN Index(ms):"] + HRVInfo["sdnnIndex"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 77) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":81 ± 24 ms)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 77) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["5min R-R间期标准差的均值，反映5min内心率变异性的平均水平，去除长周期趋势后的短周期变异性。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 83) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["rMSSD(ms):"] + HRVInfo["rmssd"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 91) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":27 ± 12 ms)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 91) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["相邻R-R间期差值均方根，反映短周期心率变化，主要由迷走神经（副交感）张力驱动。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 97) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["pNN50(%):"] + HRVInfo["pnn50"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 105) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":11 ± 8%(3–27%))", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 105) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["相邻R-R间期差值>50ms的百分比，直接计数相邻心跳间期变化>50ms的比例，反映副交感张力。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 111) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["Tri.index:"] + HRVInfo["hrvTriangularIndex"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 119) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":37 ± 15)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 119) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["HRV三角指数, 基于R-R间期直方图几何形态的总体HRV指标，反映R-R间期分布的离散程度。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 125) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["Total Power(ms²):"] + HRVInfo["total_power"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 133) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":3466 ± 1018 ms²)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 133) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["总功率, 反映整体自主神经调节活动，是HRV的'全景指标'。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 139) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["VLF(ms²):"] + HRVInfo["vlf"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 147) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":2621 ± 922 ms²)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 147) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["极低频功率, 与体温调节、肾素-血管紧张素系统及内分泌节律相关，部分含交感成分。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 153) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["LF(ms²):"] + HRVInfo["lf"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 161) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":519 ± 275 ms²)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 161) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["低频功率, 反映交感与副交感神经的混合调节（交感占主导），主要与压力反射（baroreflex）活动相关。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 167) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["HF(ms²):"] + HRVInfo["hf"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 175) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":654 ± 339 ms²)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 175) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["高频功率, 主要反映迷走神经（副交感）张力，与呼吸性窦性心律不齐（RSA）直接相关。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 181) * PixMM, SKTextAlign.Left, discFont, textPaint);

                canvas.DrawText(CommonValues.Langs["LF/HF:"] + HRVInfo["lf_hf"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 189) * PixMM, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText("(" + CommonValues.Langs["参考值"] + ":1.1–4.0)", (float)((PaperWidthMm - 18) * PixMM),
                (float)(MarginTopMm + 189) * PixMM, SKTextAlign.Right, discFont, textPaint);
                canvas.DrawText(CommonValues.Langs["LF/HF 比值, 反映交感-迷走平衡状态；比值升高→交感占优，降低→迷走占优。"], (float)((MarginLeftMm + 1) * PixMM),
                (float)(MarginTopMm + 195) * PixMM, SKTextAlign.Left, discFont, textPaint);


                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 200) * PixMM, (float)MarginLeftMm * PixMM, (float)(MarginTopMm + 265) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 200) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 200) * PixMM, linePaint);
                canvas.DrawLine((float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 200) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 265) * PixMM, linePaint);
                canvas.DrawLine((float)MarginLeftMm * PixMM, (float)(MarginTopMm + 265) * PixMM, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(MarginTopMm + 265) * PixMM, linePaint);

                canvas.DrawText(CommonValues.Langs["结论"], (float)(PaperWidthMm * PixMM) / 2,
                (float)(MarginTopMm + 205) * PixMM, SKTextAlign.Center, titleFont, textPaint);

                int lineIdx = 0;
                string[] advices = HRVInfo["advice"].ToString().Split('\n');
                for (int i = 0; i < advices.Length; i++)
                {
                    List<string> lines = AnaCode.WrapText(advices[i], (float)((PaperWidthMm - MarginLeftMm - MarginRightMm) * PixMM), textFont);
                    foreach (string line in lines)
                    {
                        float y = (float)(MarginTopMm + 210 + (6 * lineIdx)) * PixMM;
                        canvas.DrawText(line, (float)((MarginLeftMm + 1) * PixMM), y, SKTextAlign.Left, discFont, textPaint);
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

        private void DrawHRVInfo(SKCanvas canvas, JObject summary, SKFont titleFont, SKFont textFont, SKPaint textPaint)
        {
            canvas.DrawText("HRV", (float)(MarginLeftMm + (subColWidthShortMm / 2)) * PixMM,
            (float)(MarginTopMm + 116) * PixMM, SKTextAlign.Center, titleFont, textPaint);
            canvas.DrawText("SDNN(ms)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 121) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["SDNN"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 121) * PixMM, SKTextAlign.Right, textFont, textPaint);
            canvas.DrawText("SDANN(ms)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 128) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["SDANN"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 128) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText("SDNN Index(ms)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 134) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["SDNNIndx"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 134) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText("RMSSD(ms)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 140) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["RMSSD"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 140) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText("pNN50(%)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 146) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["pNN50"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 146) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText("Tri.index", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 152) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["Triindx"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 152) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText("LF(ms²)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 158) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["LF"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 158) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText("HF(ms²)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 164) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["HF"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 164) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText("LF/HF", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 170) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["LF_HF"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 170) * PixMM, SKTextAlign.Right, textFont, textPaint);
        }

        private void DrawPaceInfo(SKCanvas canvas, JObject summary, SKFont titleFont, SKFont textFont, SKPaint textPaint)
        {
            canvas.DrawText(CommonValues.Langs["起搏"], (float)(MarginLeftMm + (subColWidthShortMm / 2)) * PixMM,
            (float)(MarginTopMm + 116) * PixMM, SKTextAlign.Center, titleFont, textPaint);
            canvas.DrawText(CommonValues.Langs["起搏总数"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 121) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["SDNN"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 121) * PixMM, SKTextAlign.Right, textFont, textPaint);
            canvas.DrawText(CommonValues.Langs["占比"] + "(%)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 128) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["TP"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 128) * PixMM, SKTextAlign.Right, textFont, textPaint);
            canvas.DrawText(CommonValues.Langs["房性起搏"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 134) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["SDNNIndx"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 134) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText(CommonValues.Langs["占比"] + "(%)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 140) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["ULF"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 140) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText(CommonValues.Langs["室性起搏"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 146) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["pNN50"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 146) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText(CommonValues.Langs["占比"] + "(%)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 152) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["VLF"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 152) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText(CommonValues.Langs["房室顺序起搏"] + "(" + CommonValues.Langs["次"] + ")", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 158) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["RMSSD"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 158) * PixMM, SKTextAlign.Right, textFont, textPaint);

            canvas.DrawText(CommonValues.Langs["占比"] + "(%)", (float)((MarginLeftMm + 1) * PixMM),
            (float)(MarginTopMm + 164) * PixMM, SKTextAlign.Left, textFont, textPaint);
            canvas.DrawText($"{summary["LF"]}", (float)((MarginLeftMm + subColWidthShortMm - 1) * PixMM),
            (float)(MarginTopMm + 164) * PixMM, SKTextAlign.Right, textFont, textPaint);
        }

        public void DrawHourlistPage(SKCanvas canvas, int pageNumber)
        {
            List<HourlistInfo> hourlistInfo = CalcHourListInfo.HourDataList;
            DrawPageHeader(canvas, pageNumber);

            float titleHeight = ScaleY(5);
            float hrTrendTop = ScaleY(MarginTopMm + 7), hrTrendHeight = ScaleY(MarginTopMm + 30);
            float rrHistTop = titleHeight + hrTrendHeight, rrHistHeight = ScaleY(20);
            float chartLeft = ScaleX(MarginLeftMm + 8);
            using (SKFont titleFont = new SKFont
            {
                Size = 13.0f * FontScaleFactor,
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
                canvas.DrawText(CommonValues.Langs["小时数据图"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 3), SKTextAlign.Center, titleFont, textPaint);
                canvas.DrawText(CommonValues.Langs["心率趋势图"], ScaleX(MarginLeftMm), hrTrendTop, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["RR直方图"], ScaleX(MarginLeftMm), rrHistTop, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["室早直方图"], ScaleX(MarginLeftMm), rrHistTop + ScaleY(20) + rrHistHeight, SKTextAlign.Left, textFont, textPaint);
                canvas.DrawText(CommonValues.Langs["房早直方图"], ScaleX(PaperWidthMm) / 2, rrHistTop + ScaleY(20) + rrHistHeight, SKTextAlign.Left, textFont, textPaint);
            }
            DrawHRTrendWave drawHRTrendWave = new DrawHRTrendWave(FontScaleFactor);
            drawHRTrendWave.TopMargin = Convert.ToInt32(hrTrendTop + titleHeight);
            drawHRTrendWave.LeftMargin = Convert.ToInt32(chartLeft);
            drawHRTrendWave.BottomMargin = 0;
            drawHRTrendWave.RightMargin = 0;
            int width = Convert.ToInt32(ScaleX(PaperWidthMm - MarginRightMm));
            int height = Convert.ToInt32(hrTrendHeight);
            drawHRTrendWave.DrawHRTrend(canvas, width, height);
            drawHRTrendWave.DrawCoordinateSystem(canvas, width, height);

            float rrHistWidth = ScaleX(PaperWidthMm - MarginRightMm) - chartLeft;
            RRHistogram rrHistogram = new RRHistogram(FontScaleFactor,
                new SKRect(chartLeft, rrHistTop + ScaleY(8), chartLeft + rrHistWidth, rrHistTop + ScaleY(8) + rrHistHeight));
            rrHistogram.DrawHistogram(canvas);

            DrawVSHistogram drawVSHistogram = new DrawVSHistogram(FontScaleFactor);
            drawVSHistogram.marginTop = ScaleY(MarginTopMm + 76);
            drawVSHistogram.marginLeft = ScaleX(MarginLeftMm);
            drawVSHistogram.canvasWidth = ScaleX((PaperWidthMm - MarginLeftMm - MarginRightMm) / 2);
            drawVSHistogram.drawAxis(canvas, hourlistInfo, 'V');
            drawVSHistogram.drawHistogram(canvas, hourlistInfo, 'V');
            drawVSHistogram.marginLeft = ScaleX(MarginLeftMm) + drawVSHistogram.canvasWidth;
            drawVSHistogram.drawAxis(canvas, hourlistInfo, 'S');
            drawVSHistogram.drawHistogram(canvas, hourlistInfo, 'S');

            DrawHourList drawHourList = new DrawHourList(FontScaleFactor)
            {
                marginTop = ScaleY(MarginTopMm + 100),
                marginLeft = ScaleX(MarginLeftMm),
                width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm),
                height = 580 * FontScaleFactor
            };
            drawHourList.PaintHourList(canvas, hourlistInfo);
        }

        private void DrawHRVTimeDomainPage(SKCanvas canvas, int pageNumber)
        {
            DrawPageHeader(canvas, pageNumber);
            DrawRRPlot(canvas);
        }

        private void DrawRRPlot(SKCanvas canvas)
        {
            List<JObject> RRPlotInfoList = HRVTimeDomainInfo[0];
            List<HRVTimeDomainInfo> hRVTimeDomainInfoList = RRPlotInfoList[3]["ListInfo"]?.ToObject<List<HRVTimeDomainInfo>>();

            JObject allRRPlotInfoObj = RRPlotInfoList[0];
            List<double> allRRPlotInfo = allRRPlotInfoObj["RRPlotInfo"]?.ToObject<List<double>>();
            PaintRRPlot(canvas, allRRPlotInfo, 0);
            DrawHRVTimeDomainInfo(canvas, hRVTimeDomainInfoList[0], 0);

            JObject dayRRPlotInfoObj = RRPlotInfoList[1];
            List<double> dayRRPlotInfo = dayRRPlotInfoObj["RRPlotInfo"]?.ToObject<List<double>>();
            PaintRRPlot(canvas, dayRRPlotInfo, 1);
            DrawHRVTimeDomainInfo(canvas, hRVTimeDomainInfoList[1], 1);

            JObject nightRRPlotInfoObj = RRPlotInfoList[2];
            List<double> nightRRPlotInfo = nightRRPlotInfoObj["RRPlotInfo"]?.ToObject<List<double>>();
            PaintRRPlot(canvas, nightRRPlotInfo, 2);
            DrawHRVTimeDomainInfo(canvas, hRVTimeDomainInfoList[2], 2);

            PaintHRVTimeDomainInfoList(canvas, hRVTimeDomainInfoList);
        }

        private void PaintRRPlot(SKCanvas canvas, List<double> allRRPlotInfo, int type)
        {
            PaintRRPlotAxis(canvas, allRRPlotInfo, type);
            PaintRRPlotWave(canvas, allRRPlotInfo, type);
        }

        private void PaintRRPlotAxis(SKCanvas canvas, List<double> allRRPlotInfo, int type)
        {
            using (SKPaint axisPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 11f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont reportTitleFont = new SKFont
            {
                Size = 13f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            // 绘制Y轴刻度
            using (SKFont tickFont = new SKFont
            {
                Size = 9f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {

                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm) / 3;
                float height = 200f * FontScaleFactor;

                float marginTop = ScaleY(MarginTopMm + 8);
                float marginLeft = width * type + ScaleX(MarginLeftMm + 6);

                string title = "";
                switch (type)
                {
                    case 0:
                        title = CommonValues.Langs["全程"];
                        canvas.DrawText(CommonValues.Langs["HRV时域报告"], ScaleX(PaperWidthMm / 2), ScaleY(MarginTopMm + 3), SKTextAlign.Center, reportTitleFont, textPaint);
                        break;
                    case 1:
                        title = CommonValues.Langs["白天"];
                        break;
                    case 2:
                        title = CommonValues.Langs["夜间"];
                        break;
                }
                canvas.DrawText(title, marginLeft + width / 2, marginTop, SKTextAlign.Center, titleFont, textPaint);
                double maxCount = allRRPlotInfo.Max();
                int yAxisMax = Convert.ToInt32(Math.Ceiling(maxCount / 5.0) * 5);

                // X轴
                canvas.DrawLine(marginLeft, marginTop + height, marginLeft + width - ScaleX(6), marginTop + height, axisPaint);
                // Y轴
                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + height, axisPaint);

                int tickCount = 10;
                for (int i = 1; i <= tickCount; i++)
                {
                    if (i % 2 == 0)
                    {
                        float yValue = yAxisMax * i / (float)tickCount;
                        float yPos = marginTop + height - (height * i / tickCount);

                        // 绘制刻度线
                        canvas.DrawLine(marginLeft - 15, yPos, marginLeft, yPos, axisPaint);

                        // 绘制刻度值（整数显示）
                        string tickText = yValue % 1 == 0 ? $"{yValue:F0}" : $"{yValue:F1}";
                        canvas.DrawText(tickText, marginLeft - 5, yPos + 3, SKTextAlign.Right, tickFont, textPaint);
                    }

                }

                for (int i = 0; i <= allRRPlotInfo.Count / 10; i++)
                {
                    double second = Math.Round(i / 10f, 2);

                    string labelText;
                    float x = 0;
                    float y = marginTop + height;

                    labelText = second.ToString();
                    x = marginLeft + (i * (width - ScaleX(6)) / 20);

                    if (i % 2 == 0)
                    {
                        // 绘制刻度线
                        canvas.DrawLine(x, y, x, y + 15, axisPaint);
                    }
                    if (i % 4 == 0)
                    {
                        float labelY = y + 30;
                        canvas.DrawText(labelText, x, labelY, SKTextAlign.Center, tickFont, axisPaint);
                    }

                }
            }
        }

        private void PaintRRPlotWave(SKCanvas canvas, List<double> allRRPlotInfo, int type)
        {
            using (SKPaint axisPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,

            })
            using (SKFont titleFont = new SKFont
            {
                Size = 11f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            // 绘制Y轴刻度
            using (SKFont tickFont = new SKFont
            {
                Size = 9f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {

                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm) / 3;
                float height = 200f * FontScaleFactor;

                float marginTop = ScaleY(MarginTopMm + 8);
                float marginLeft = width * type + ScaleX(MarginLeftMm + 6);

                double maxCount = allRRPlotInfo.Max();
                if (maxCount == 0) return;

                int yAxisMax = Convert.ToInt32(Math.Ceiling(maxCount / 5.0) * 5);

                for (int i = 0; i < allRRPlotInfo.Count; i++)
                {
                    double count = allRRPlotInfo[i];
                    float x = 0;
                    int y = Convert.ToInt32(marginTop + height - count * (height / yAxisMax));
                    x = marginLeft + (i * (width - ScaleX(6)) / allRRPlotInfo.Count);
                    canvas.DrawLine(x, marginTop + height, x, y, axisPaint);
                }
            }
        }

        private void DrawHRVTimeDomainInfo(SKCanvas canvas, HRVTimeDomainInfo info, int type)
        {
            using (SKPaint axisPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.5f * FontScaleFactor,

            })
            using (SKFont tickFont = new SKFont
            {
                Size = 11f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {

                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm) / 3;
                float height = 200f * FontScaleFactor;

                float marginTop = ScaleY(MarginTopMm + 8);
                float marginLeft = width * type + ScaleX(MarginLeftMm + 6);

                List<String[]> infos = new List<string[]>();

                infos.Add(new string[] { "SDNN(ms)", info.sdnn});
                infos.Add(new string[] { "SDANN(ms)", info.sdann });
                infos.Add(new string[] { "SDNNIDX(ms)", info.sdnnIndex });

                infos.Add(new string[] { "rMSSD(ms)", info.rmssd });
                infos.Add(new string[] { "pNN50(%)", info.pnn50 });
                infos.Add(new string[] { "Tri.index", info.hrvTriangularIndex });

                for(int i=0; i<infos.Count; i++)
                {
                    float x = 0;
                    float y = marginTop + height + 25 * FontScaleFactor * (i / 2 + 1);
                    if (i % 2 == 0)
                    {
                        x = marginLeft;
                        canvas.DrawText(infos[i][0] + ": " + infos[i][1], x, y, SKTextAlign.Left, tickFont, axisPaint);
                    }
                    else
                    {
                        x = marginLeft + width - ScaleX(6);
                        canvas.DrawText(infos[i][0] + ": " + infos[i][1], x, y, SKTextAlign.Right, tickFont, axisPaint);
                    }

                }

            }
        }

        public void PaintHRVTimeDomainInfoList(SKCanvas canvas, List<HRVTimeDomainInfo> hRVTimeDomainInfoList)
        {
            if (hRVTimeDomainInfoList.Count == 0) return;
            using (SKPaint tablePaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.5f * FontScaleFactor,
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm);
                float height = 650f * FontScaleFactor;

                float marginTop = ScaleY(MarginTopMm + 85);
                float marginLeft = ScaleX(MarginLeftMm);

                float timeColWidth = 65f * FontScaleFactor;
                int col = 9;
                int row = hRVTimeDomainInfoList.Count + 1;
                float cellWidth = (width - timeColWidth) / (col - 1);
                float cellHeight = height / 28;

                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + cellHeight * row, tablePaint);
                canvas.DrawLine(marginLeft, marginTop, marginLeft + width, marginTop, tablePaint);
                canvas.DrawLine(marginLeft + width, marginTop, marginLeft + width, marginTop + cellHeight * row, tablePaint);
                canvas.DrawLine(marginLeft, marginTop + cellHeight * row, marginLeft + width, marginTop + cellHeight * row, tablePaint);

                canvas.DrawText(CommonValues.Langs["类型"], marginLeft + 0.5f * timeColWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["起始时间"], marginLeft + timeColWidth + 0.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["时长"], marginLeft + timeColWidth + 1.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["SDNN(ms)"], marginLeft + timeColWidth + 2.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["SDNNIDX(ms)"], marginLeft + timeColWidth + 3.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["SDANN(ms)"], marginLeft + timeColWidth + 4.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["pNN50(%)"], marginLeft + timeColWidth + 5.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["rMSSD(ms)"], marginLeft + timeColWidth + 6.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["Tri.index"], marginLeft + timeColWidth + 7.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);


                for (int i = 0; i < row - 1; i++)
                {

                    canvas.DrawLine(marginLeft, marginTop + cellHeight * (i + 1), marginLeft + width, marginTop + cellHeight * (i + 1), tablePaint);

                }

                for (int i = 0; i < col - 1; i++)
                {
                    canvas.DrawLine(marginLeft + timeColWidth + i * cellWidth, marginTop, marginLeft + timeColWidth + i * cellWidth, marginTop + cellHeight * row, tablePaint);
                }

                for (int i = 0; i < hRVTimeDomainInfoList.Count; i++)
                {
                    object[] array = typeof(HRVTimeDomainInfo).GetProperties()
                    .Select(p => p.GetValue(hRVTimeDomainInfoList[i]))
                    .ToArray();

                    List<string> cellValues = new List<string>();
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (j == 3 || j == 4 || j == 5 || j == 6 || j == 10 || j == 11)
                        {
                            continue;
                        }
                        else
                        {
                            cellValues.Add(array[j].ToString());
                        }
                    }

                    for(int j=0; j< cellValues.Count; j++)
                    {
                        if (j == 0)
                        {
                            canvas.DrawText(cellValues[j], marginLeft + 0.5f * timeColWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, tablePaint);

                        }
                        else
                        {
                            canvas.DrawText(cellValues[j], marginLeft + timeColWidth + (j - 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, tablePaint);
                        }
                    }
                }

            }
        }

        private void DrawHRVFrequencyDomainPage(SKCanvas canvas, int pageNumber)
        {
            DrawPageHeader(canvas, pageNumber);
            DrawHrvPsd(canvas);
        }
        private void DrawHrvPsd(SKCanvas canvas)
        {
            List<JObject> hrvFrequencyDomainInfo = HRVFrequencyDomainInfo[0];

            List<HRVFreqDomainInfo> hrvFreqDomainInfoList = hrvFrequencyDomainInfo[3]["info"]?.ToObject<List<HRVFreqDomainInfo>>();
            PaintHRVFreqDomainInfoList(canvas, hrvFreqDomainInfoList);

            List<HRVFreqDomainInfo> hrvMaxMinFreqDomainInfoList = hrvFrequencyDomainInfo[2]["info"]?.ToObject<List<HRVFreqDomainInfo>>();

            JObject hrvPsdObjMax = hrvFrequencyDomainInfo[0];
            List<AEcgHrvPsdInfo> hrvPsdInfoMax = hrvPsdObjMax["info"]?.ToObject<List<AEcgHrvPsdInfo>>();
            PaintHrvPsd(canvas, hrvPsdInfoMax, 0);
            DrawHRVFreqDomainInfo(canvas, hrvMaxMinFreqDomainInfoList[0], 0);

            JObject hrvPsdObjMin = hrvFrequencyDomainInfo[1];
            List<AEcgHrvPsdInfo> hrvPsdInfoMin = hrvPsdObjMin["info"]?.ToObject<List<AEcgHrvPsdInfo>>();
            PaintHrvPsd(canvas, hrvPsdInfoMin, 1);
            DrawHRVFreqDomainInfo(canvas, hrvMaxMinFreqDomainInfoList[1], 1);
        }

        private void PaintHrvPsd(SKCanvas canvas, List<AEcgHrvPsdInfo> hrvPsdInfo, int type)
        {
            PaintHrvPsdAxis(canvas, hrvPsdInfo, type);
            PaintHrvPsdWave(canvas, hrvPsdInfo, type);
        }

        private void PaintHrvPsdAxis(SKCanvas canvas, List<AEcgHrvPsdInfo> hrvPsdInfo, int type)
        {
            using (SKPaint axisPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 11f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont reportTitleFont = new SKFont
            {
                Size = 13f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            // 绘制Y轴刻度
            using (SKFont tickFont = new SKFont
            {
                Size = 9f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {

                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm) / 2;
                float height = 200f * FontScaleFactor;

                float marginTop = ScaleY(MarginTopMm + 8);
                float marginLeft = width * type + ScaleX(MarginLeftMm + 6);

                string title = "";
                switch (type)
                {
                    case 0:
                        title = CommonValues.Langs["最大LF/HF"];
                        canvas.DrawText(CommonValues.Langs["HRV频域报告"], ScaleX(PaperWidthMm / 2), ScaleY(MarginTopMm + 3), SKTextAlign.Center, reportTitleFont, textPaint);
                        break;
                    case 1:
                        title = CommonValues.Langs["最小LF/HF"];
                        break;
                }
                canvas.DrawText(title, marginLeft + width / 2, marginTop, SKTextAlign.Center, titleFont, textPaint);
                double maxCount = hrvPsdInfo.Max(arr => arr.DataY);
                int yAxisMax = Convert.ToInt32(Math.Ceiling(maxCount / 5.0) * 5);

                // X轴
                canvas.DrawLine(marginLeft, marginTop + height, marginLeft + width - ScaleX(6), marginTop + height, axisPaint);
                // Y轴
                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + height, axisPaint);

                int tickCount = 5;
                for (int i = 1; i <= tickCount; i++)
                {

                    float yValue = yAxisMax * i / (float)tickCount;
                    float yPos = marginTop + height - (height * i / tickCount);

                    // 绘制刻度线
                    canvas.DrawLine(marginLeft - 15, yPos, marginLeft, yPos, axisPaint);

                    // 绘制刻度值（整数显示）
                    string tickText = yValue % 1 == 0 ? $"{yValue:F0}" : $"{yValue:F1}";
                    canvas.DrawText(tickText, marginLeft - 5, yPos + 3, SKTextAlign.Right, tickFont, textPaint);

                }

                for (int i = 0; i <= 10; i++)
                {
                    double hz = Math.Round(i / 20f, 1);

                    string labelText;
                    float x = 0;
                    float y = marginTop + height;

                    labelText = hz.ToString();
                    x = marginLeft + (i * (width - ScaleX(6)) / 10);
                    // 绘制刻度线
                    canvas.DrawLine(x, y, x, y + 15, axisPaint);

                    if (i % 2 == 0)
                    {
                        float labelY = y + 30;
                        canvas.DrawText(labelText, x, labelY, SKTextAlign.Center, tickFont, axisPaint);
                    }

                }
            }
        }

        private void PaintHrvPsdWave(SKCanvas canvas, List<AEcgHrvPsdInfo> hrvPsdInfo, int type)
        {
            using (SKPaint axisPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f * FontScaleFactor,

            })
            using (SKFont titleFont = new SKFont
            {
                Size = 11f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            // 绘制Y轴刻度
            using (SKFont tickFont = new SKFont
            {
                Size = 9f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {

                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm) / 2;
                float height = 200f * FontScaleFactor;

                float marginTop = ScaleY(MarginTopMm + 8);
                float marginLeft = width * type + ScaleX(MarginLeftMm + 6);

                double maxCount = hrvPsdInfo.Max(arr => arr.DataY);
                int yAxisMax = Convert.ToInt32(Math.Ceiling(maxCount / 5.0) * 5);

                for (int i = 0; i < hrvPsdInfo.Count; i++)
                {
                    if (hrvPsdInfo[i].DataX > 0.5)
                    {
                        continue;
                    }
                    double dataY = hrvPsdInfo[i].DataY;
                    float x = 0;
                    int y = Convert.ToInt32(marginTop + height - dataY * (height / yAxisMax));
                    x = hrvPsdInfo[i].DataX == 0 ? marginLeft : (float) (marginLeft + hrvPsdInfo[i].DataX / 0.5 * (width - ScaleX(6)));
                    canvas.DrawLine(x, marginTop + height, x, y, axisPaint);
                }
            }
        }

        private void DrawHRVFreqDomainInfo(SKCanvas canvas, HRVFreqDomainInfo info, int type)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.4f * FontScaleFactor,

            })
            using (SKFont tickFont = new SKFont
            {
                Size = 11f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {

                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm) / 2;
                float height = 200f * FontScaleFactor;

                float marginTop = ScaleY(MarginTopMm + 8);
                float marginLeft = width * type + ScaleX(MarginLeftMm + 6);

                List<String[]> infos = new List<string[]>();

                double lfhf = Math.Round(Convert.ToDouble(info.lf) / Convert.ToDouble(info.hf), 2);


                infos.Add(new string[] { "Total Power(ms²)", info.total_power });
                infos.Add(new string[] { "VLF(ms²)", info.vlf });
                infos.Add(new string[] { "LF(ms²)", info.lf });

                infos.Add(new string[] { "HF(ms²)", info.hf });
                infos.Add(new string[] { "LF/HF", lfhf.ToString() });

                for (int i = 0; i < infos.Count; i++)
                {
                    float x = 0;
                    float y = marginTop + height + 25 * FontScaleFactor * (i / 2 + 1);
                    if (i % 2 == 0)
                    {
                        x = marginLeft + ScaleX(6);
                        canvas.DrawText(infos[i][0] + ": " + infos[i][1], x, y, SKTextAlign.Left, tickFont, textPaint);
                    }
                    else
                    {
                        x = marginLeft + width - ScaleX(12);
                        canvas.DrawText(infos[i][0] + ": " + infos[i][1], x, y, SKTextAlign.Right, tickFont, textPaint);
                    }

                }

            }
        }

        public void PaintHRVFreqDomainInfoList(SKCanvas canvas, List<HRVFreqDomainInfo> hRVTimeDomainInfoList)
        {
            if (hRVTimeDomainInfoList.Count == 0) return;
            using (SKPaint tablePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.5f * FontScaleFactor,
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm);
                float height = 650f * FontScaleFactor;

                float marginTop = ScaleY(MarginTopMm + 85);
                float marginLeft = ScaleX(MarginLeftMm);

                float timeColWidth = 65f * FontScaleFactor;
                int col = 7;
                int row = hRVTimeDomainInfoList.Count + 1;
                float cellWidth = (width - timeColWidth) / (col - 1);
                float cellHeight = height / 28;

                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + cellHeight * row, tablePaint);
                canvas.DrawLine(marginLeft, marginTop, marginLeft + width, marginTop, tablePaint);
                canvas.DrawLine(marginLeft + width, marginTop, marginLeft + width, marginTop + cellHeight * row, tablePaint);
                canvas.DrawLine(marginLeft, marginTop + cellHeight * row, marginLeft + width, marginTop + cellHeight * row, tablePaint);


                canvas.DrawText(CommonValues.Langs["类型"], marginLeft + 0.5f * timeColWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["起始时间"], marginLeft + timeColWidth + 0.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["时长"], marginLeft + timeColWidth + 1.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["Total Power(ms²)"], marginLeft + timeColWidth + 2.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["VLF(ms²)"], marginLeft + timeColWidth + 3.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["HF(ms²)"], marginLeft + timeColWidth + 4.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["LF/HF"], marginLeft + timeColWidth + 5.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                for (int i = 0; i < row - 1; i++)
                {
                    canvas.DrawLine(marginLeft, marginTop + cellHeight * (i + 1), marginLeft + width, marginTop + cellHeight * (i + 1), tablePaint);
                }

                for (int i = 0; i < col - 1; i++)
                {
                    canvas.DrawLine(marginLeft + timeColWidth + i * cellWidth, marginTop, marginLeft + timeColWidth + i * cellWidth, marginTop + cellHeight * row, tablePaint);
                }

                for (int i = 0; i < hRVTimeDomainInfoList.Count; i++)
                {
                    object[] array = typeof(HRVFreqDomainInfo).GetProperties()
                    .Select(p => p.GetValue(hRVTimeDomainInfoList[i]))
                    .ToArray();

                    for (int j = 0; j < col; j++)
                    {
                        if (j == 0)
                        {
                            canvas.DrawText(array[j].ToString(), marginLeft + 0.5f * timeColWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, tablePaint);

                        }
                        else if(j == col-1)
                        {
                            if(array[j].ToString()=="-" || array[j-1].ToString() == "-")
                            {
                                canvas.DrawText("-", marginLeft + timeColWidth + (j - 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, tablePaint);
                            }
                            else
                            {
                                double lfhf = Math.Round(Convert.ToDouble(array[j]) / Convert.ToDouble(array[j-1]), 2);
                                canvas.DrawText(lfhf.ToString(), marginLeft + timeColWidth + (j - 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, tablePaint);
                            }
                        }
                        else
                        {
                            canvas.DrawText(array[j].ToString(), marginLeft + timeColWidth + (j - 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, tablePaint);
                        }
                    }
                }

            }
        }
        private void DrawHRVAnalysisPage(SKCanvas canvas, int pageNumber)
        {
            //DrawSummaryPageHeader(canvas, CommonValues.Langs["心率变异性(HRV)分析总报告"], pageNumber);
            DrawPageHeader(canvas, pageNumber);
            DrawPatientInfo(canvas);
            DrawHRVTimeDomainInfo(canvas);
        }

        private void DrawSTListPage(SKCanvas canvas, int pageNumber)
        {
            int idx = pageNumber - STListIdx;
            if (idx == STListInfo.Count) return;

            //DrawSummaryPageHeader(canvas, CommonValues.Langs["ST事件列表"], pageNumber);
            DrawPageHeader(canvas, pageNumber);

            List<JObject> stListInfo = STListInfo[idx];

            using (SKPaint tablePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.5f * FontScaleFactor,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 13f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm);
                float height = ScaleY(PaperHeightMm - MarginTopMm - MarginBottonMm - 12);

                int col = 4;
                int row = 41;
                float cellWidth = width / col;
                float cellHeight = height / row;

                float marginTop = ScaleY(MarginTopMm + 6);
                float marginLeft = ScaleX(MarginLeftMm);

                canvas.DrawText(CommonValues.Langs["ST事件列表"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 3), SKTextAlign.Center, titleFont, textPaint);

                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + cellHeight * (stListInfo.Count + 1), tablePaint);
                canvas.DrawLine(marginLeft, marginTop, marginLeft + width, marginTop, tablePaint);
                canvas.DrawLine(marginLeft + width, marginTop, marginLeft + width, marginTop + cellHeight * (stListInfo.Count + 1), tablePaint);
                canvas.DrawLine(marginLeft, marginTop + cellHeight * (stListInfo.Count + 1), marginLeft + width, marginTop + cellHeight * (stListInfo.Count + 1), tablePaint);

                for (int i = 0; i < row - 1; i++)
                {
                    if (i == stListInfo.Count) break;
                    canvas.DrawLine(marginLeft, marginTop + cellHeight * (i + 1), marginLeft + width, marginTop + cellHeight * (i + 1), tablePaint);
                }

                for (int i = 0; i < col; i++)
                {
                    canvas.DrawLine(marginLeft + i * cellWidth, marginTop, marginLeft + i * cellWidth, marginTop + cellHeight * (stListInfo.Count + 1), tablePaint);
                }

                canvas.DrawText(CommonValues.Langs["开始时间"], marginLeft + 0.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["持续(分钟)"], marginLeft + 1.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["STE(mV)"], marginLeft + 2.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["导联"], marginLeft + 3.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);

                for (int i = 0; i < stListInfo.Count; i++)
                {
                    JArray arrayObj = new JArray(stListInfo[i].Properties().Select(p => p.Value));
                    object[] array = arrayObj.ToArray();
                    for (int j = 0; j < col; j++)
                    {
                        string cellValue = array[j].ToString();

                        canvas.DrawText(cellValue, marginLeft + (j + 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, tablePaint);

                    }
                }

            }

        }

        private void DrawSTTTrendWave(SKCanvas canvas, List<float> Ticks, string type)
        {
            float width = ScaleX(PaperWidthMm - MarginLeftMm - 6);
            float height = ScaleY(PaperHeightMm - MarginTopMm - MarginBottonMm - 15);
            float RectPadding = ScaleX(1);
            float BottomTimeHeight = ScaleY(3);
            float rectWidth = width - RectPadding * 2;

            DrawSTTrendWave drawSTTrendWave = new DrawSTTrendWave();
            drawSTTrendWave.FontScaleFactor = FontScaleFactor;
            drawSTTrendWave.WaveLeftMargin = ScaleX(MarginLeftMm);
            drawSTTrendWave.WaveTopMargin = ScaleY(2);
            drawSTTrendWave.WaveRightMargin = ScaleX(2);
            drawSTTrendWave.WaveBottomMargin = ScaleY(2);

            string titleStr = "";
            List<List<JObject>> infoList = new List<List<JObject>>();
            if(type == "ST")
            {
                infoList = STTrendWaveInfo;

                titleStr = CommonValues.Langs["ST趋势图"];
            }
            else
            {
                infoList = TTrendWaveInfo;

                titleStr = CommonValues.Langs["T波趋势图"];

            }

            for (int i = 0; i < infoList.Count; i++)
            {
                float rectHeight = (height - RectPadding * infoList.Count - BottomTimeHeight) / infoList.Count;
                SKRect waveRect = new SKRect();
                waveRect.Left = ScaleX(MarginLeftMm) + RectPadding;
                waveRect.Right = RectPadding + rectWidth;
                waveRect.Top = ScaleY(MarginTopMm + 6) + i * rectHeight + (i + 1) * RectPadding;

                if (i == infoList.Count - 1)
                {
                    rectHeight += BottomTimeHeight;
                    drawSTTrendWave.WaveBottomMargin = RectPadding + BottomTimeHeight;
                    drawSTTrendWave.bLastRect = true;
                }

                waveRect.Bottom = waveRect.Top + rectHeight;
                string strLabel = AnaValues.LeadIdxToNameMap[i];
                drawSTTrendWave.WaveRect = waveRect;
                drawSTTrendWave.DrawRect(canvas);

                List<MinStxInfo> result = infoList[i]
                .Select(j => j.ToObject<MinStxInfo>())
                .ToList();

                drawSTTrendWave.DrawTTrend(canvas, type, Ticks, result);
                drawSTTrendWave.DrawCoordinateSystem(canvas, strLabel, Ticks);

            }

            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 13f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                canvas.DrawText(titleStr, ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 3), SKTextAlign.Center, titleFont, textPaint);
            }

        }

        private void DrawSTTrendWavePage(SKCanvas canvas, int pageNumber)
        {
            //DrawSummaryPageHeader(canvas, CommonValues.Langs["ST趋势图"], pageNumber);
            DrawPageHeader(canvas, pageNumber);
            List<float> Ticks = new List<float>();
            float[] tickArr = { -0.5f, -0.4f, -0.3f, -0.2f, -0.1f, 0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };
            Ticks.AddRange(tickArr);

            DrawSTTTrendWave(canvas, Ticks, "ST");
        }

        private void DrawTTrendWavePage(SKCanvas canvas, int pageNumber)
        {
            //DrawSummaryPageHeader(canvas, CommonValues.Langs["T波趋势图"], pageNumber);
            DrawPageHeader(canvas, pageNumber);
            List<float> Ticks = new List<float>();
            float[] tickArr = { -1f, -0.8f, -0.6f, -0.4f, -0.2f, 0, 0.2f, 0.4f, 0.6f, 0.8f, 1f };
            Ticks.AddRange(tickArr);

            DrawSTTTrendWave(canvas, Ticks, "T");
        }

        private void DrawAFListPage(SKCanvas canvas, int pageNumber)
        {
            int idx = pageNumber - AFListIdx;
            if (idx == AFListInfo.Count) return;

            //DrawSummaryPageHeader(canvas, CommonValues.Langs["房颤/房扑事件列表"], pageNumber);
            DrawPageHeader(canvas, pageNumber);

            List<JObject> afListInfo = AFListInfo[idx];

            using (SKPaint tablePaint = new SKPaint
            {
                Color = _textColor,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = 0.5f * FontScaleFactor,
            })
            using (SKPaint textPaint = new SKPaint
            {
                Color = _textColor,
                StrokeWidth = 0.4f * FontScaleFactor,
                Style = SKPaintStyle.StrokeAndFill,
                IsAntialias = true
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 13f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            using (SKFont textFont = new SKFont
            {
                Size = 10.5f * FontScaleFactor,
                Typeface = SKTypeface.FromFamilyName("宋体")
            })
            {
                float width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm);
                float height = ScaleY(PaperHeightMm - MarginTopMm - MarginBottonMm - 12);

                int col = 3;
                int row = 41;
                float cellWidth = width / col;
                float cellHeight = height / row;

                float marginTop = ScaleY(MarginTopMm + 6);
                float marginLeft = ScaleX(MarginLeftMm);

                canvas.DrawText(CommonValues.Langs["房颤/房扑事件列表"], ScaleX(PaperWidthMm) / 2, ScaleY(MarginTopMm + 3), SKTextAlign.Center, titleFont, textPaint);

                canvas.DrawLine(marginLeft, marginTop, marginLeft, marginTop + cellHeight * (afListInfo.Count + 1), tablePaint);
                canvas.DrawLine(marginLeft, marginTop, marginLeft + width, marginTop, tablePaint);
                canvas.DrawLine(marginLeft + width, marginTop, marginLeft + width, marginTop + cellHeight * (afListInfo.Count + 1), tablePaint);
                canvas.DrawLine(marginLeft, marginTop + cellHeight * (afListInfo.Count + 1), marginLeft + width, marginTop + cellHeight * (afListInfo.Count + 1), tablePaint);

                for (int i = 0; i < row - 1; i++)
                {
                    if (i == afListInfo.Count) break;
                    canvas.DrawLine(marginLeft, marginTop + cellHeight * (i + 1), marginLeft + width, marginTop + cellHeight * (i + 1), tablePaint);
                }

                for (int i = 0; i < col; i++)
                {
                    canvas.DrawLine(marginLeft + i * cellWidth, marginTop, marginLeft + i * cellWidth, marginTop + cellHeight * (afListInfo.Count + 1), tablePaint);
                }

                canvas.DrawText(CommonValues.Langs["开始时间"], marginLeft + 0.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["持续时间"], marginLeft + 1.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);
                canvas.DrawText(CommonValues.Langs["事件类型"], marginLeft + 2.5f * cellWidth, marginTop + cellHeight * 0.8f, SKTextAlign.Center, textFont, tablePaint);

                for (int i = 0; i < afListInfo.Count; i++)
                {
                    JArray arrayObj = new JArray(afListInfo[i].Properties().Select(p => p.Value));
                    object[] array = arrayObj.ToArray();
                    for (int j = 0; j < col; j++)
                    {
                        string cellValue = array[j].ToString();

                        canvas.DrawText(cellValue, marginLeft + (j + 0.5f) * cellWidth, marginTop + cellHeight * (i + 1.8f), SKTextAlign.Center, textFont, tablePaint);

                    }
                }

            }

        }

        private void DrawPaceListPage(SKCanvas canvas, int pageNumber)
        {
            DrawPageHeader(canvas, pageNumber);
            using (SKFont titleFont = new SKFont
            {
                Size = 14.0f * FontScaleFactor,
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
                canvas.DrawText(CommonValues.Langs["起搏分析列表"], ScaleX(PaperWidthMm) / 2,
                ScaleY(MarginTopMm + 5), SKTextAlign.Center, titleFont, textPaint);
            }
            DrawHourList drawHourList = new DrawHourList(FontScaleFactor)
            {
                marginTop = ScaleY(MarginTopMm + 8),
                marginLeft = ScaleX(MarginLeftMm),
                width = ScaleX(PaperWidthMm - MarginLeftMm - MarginRightMm),
                height = 880 * FontScaleFactor
            };
            drawHourList.PaintPaceList(canvas, PaceListInfo[0]);
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

                //string pageFoot = CommonValues.SystemSettings.reportSets.pageFoot;
                //if (pageFoot != "")
                //{
                //    canvas.DrawText(pageFoot, ScaleX(MarginLeftMm), ScaleY(PaperHeightMm - MarginBottonMm), SKTextAlign.Left, copyRightFont, textPaint);
                //}
                //else
                //{
                //    string copyrightStr = CommonValues.Langs["*2024 此报告仅供临床医生参考"];
                //    canvas.DrawText(copyrightStr, ScaleX(MarginLeftMm), ScaleY(PaperHeightMm - MarginBottonMm), SKTextAlign.Left, copyRightFont, textPaint);
                //}

                if (CommonValues.SystemSettings.reportSets.displayPageNum == 1)
                {
                    // 绘制页码
                    int pageNum = pageNumber + 1;
                    string footerText = $"{pageNum}/{TotalPages}";

                    canvas.DrawText(footerText, (float)(PaperWidthMm - MarginRightMm) * PixMM, (float)(PaperHeightMm - MarginBottonMm) * PixMM, SKTextAlign.Right, textFont, textPaint);
                }

            }
        }

    }
}
