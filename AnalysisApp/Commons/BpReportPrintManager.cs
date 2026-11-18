using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalysisApp.Commons
{
    class BpReportPrintManager
    {
        private List<Bitmap> ecgPages;

        private PrintDocument printDocument;

        private BpPrintReportPage printReport;

        //是否横向打印
        private bool bHorizontalPrint = false;
        public double PaperWidthMm = 210.0; // A4纸宽度
        public double PaperHeightMm = 297.0; // A4纸高度
        private const float PixMM = 3.7795f;          // 1mm 3.7795个像素点

        private bool bUseOnDemandRendering = true;  //按需渲染
        private int currentPageIndex = 0;
        //总页数
        public int TotalPages = 0;
        public Dictionary<string, List<List<JObject>>> AllPageInfo = new Dictionary<string, List<List<JObject>>>();
        public Dictionary<string, int> PageIdxs = new Dictionary<string, int>();
        public string advice = "";

        public BpReportPrintManager(bool bHorizontalPrint, int totalPages)
        {
            ecgPages = new List<Bitmap>();
            printDocument = new PrintDocument();
            printReport = new BpPrintReportPage();
            printReport.TotalPages = totalPages;
            this.bHorizontalPrint = bHorizontalPrint;
            TotalPages = totalPages;
            ConfigurePrintDocument();
        }

        private void ConfigurePrintDocument()
        {
            if (bHorizontalPrint)
            {
                PaperWidthMm = 297.0;
                PaperHeightMm = 210.0;
            }
            else
            {
                PaperWidthMm = 210.0;
                PaperHeightMm = 297.0;
            }
            // 设置默认打印机设置
            printDocument.DefaultPageSettings.Landscape = bHorizontalPrint; // 横向打印
            printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0); // 无边距
            // 设置纸张大小为标准A4：PaperSize 的宽高单位是 1/100 英寸，A4 = 827 x 1169
            // （注意：不能用渲染像素值，否则会变成驱动不识别的自定义纸张，导致方向设置被忽略）
            //printDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);

            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.QueryPageSettings += PrintDocument_QueryPageSettings;
            printDocument.EndPrint += PrintDocument_EndPrint;
        }

        private void PrintDocument_QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            // 逐页强制横向：防止打印对话框或打印机驱动（读取自身默认 DEVMODE 时）
            // 把方向改回纵向，从而无需再手动到打印机属性里设置横向
            if (bHorizontalPrint)
            {
                e.PageSettings.Landscape = true;
                //e.PageSettings.PaperSize = new PaperSize("A4", 827, 1169);
                e.PageSettings.Margins = new Margins(0, 0, 0, 0);
            }
        }

        public void SetOnDemandRendering()
        {
            bUseOnDemandRendering = true;
            currentPageIndex = 0;

            // 清理预先渲染的页面
            foreach (Bitmap page in ecgPages)
            {
                page?.Dispose();
            }
            ecgPages.Clear();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Bitmap page = null;
            try
            {
                // 页码范围过滤：PrintDocument 不会自动执行打印对话框选择的页码范围，
                // 需在此读取 PrinterSettings，跳过 [fromPage, toPage] 范围外的页
                int totalPageCount = bUseOnDemandRendering ? TotalPages : ecgPages.Count;
                int fromPage = 1;
                int toPage = totalPageCount;
                if (printDocument.PrinterSettings.PrintRange == PrintRange.SomePages)
                {
                    fromPage = Math.Max(1, printDocument.PrinterSettings.FromPage);
                    toPage = Math.Min(totalPageCount, printDocument.PrinterSettings.ToPage);
                }

                // 范围之前的页：直接跳到范围起始页（避免打印空白物理页）
                if (currentPageIndex < fromPage - 1)
                {
                    currentPageIndex = fromPage - 1;
                }

                // 超出范围：结束打印
                if (currentPageIndex >= toPage)
                {
                    e.HasMorePages = false;
                    return;
                }

                if (bUseOnDemandRendering)
                {
                    // 按需渲染模式：打印一页渲染一页
                    if (currentPageIndex < TotalPages)
                    {
                        if ((PageIdxs.ContainsKey("summary") && AllPageInfo.ContainsKey("summary")) && (currentPageIndex >= PageIdxs["summary"] && currentPageIndex <= PageIdxs["summary"] + AllPageInfo["summary"].Count))
                        {
                            printReport.SummaryInfo = AllPageInfo["summary"];
                            printReport.Advice = advice;
                            page = printReport.RenderReportPage(currentPageIndex, "summary");
                        }
                        if ((PageIdxs.ContainsKey("DataList") && AllPageInfo.ContainsKey("DataList")) && (currentPageIndex >= PageIdxs["DataList"] && currentPageIndex <= PageIdxs["DataList"] + AllPageInfo["DataList"].Count))
                        {
                            printReport.DataListInfo = AllPageInfo["DataList"];
                            printReport.DataListIdx = PageIdxs["DataList"];
                            page = printReport.RenderReportPage(currentPageIndex, "DataList");
                        }
                        if ((PageIdxs.ContainsKey("HourList") && AllPageInfo.ContainsKey("HourList")) && (currentPageIndex >= PageIdxs["HourList"] && currentPageIndex <= PageIdxs["HourList"] + AllPageInfo["HourList"].Count))
                        {
                            printReport.HourInfo = AllPageInfo["HourList"];
                            printReport.DataListIdx = PageIdxs["HourList"];
                            page = printReport.RenderReportPage(currentPageIndex, "HourList");
                        }
                        if ((PageIdxs.ContainsKey(("TrendWave")) && AllPageInfo.ContainsKey("TrendWave")) && (currentPageIndex >= PageIdxs["TrendWave"] && currentPageIndex <= PageIdxs["TrendWave"] + AllPageInfo["TrendWave"].Count))
                        {
                            printReport.TrendWaveInfo = AllPageInfo["TrendWave"];
                            page = printReport.RenderReportPage(currentPageIndex, "TrendWave");
                        }
                        if ((PageIdxs.ContainsKey(("Histogram")) && AllPageInfo.ContainsKey("Histogram")) && (currentPageIndex >= PageIdxs["Histogram"] && currentPageIndex <= PageIdxs["Histogram"] + AllPageInfo["Histogram"].Count))
                        {
                            printReport.HistogramInfo = AllPageInfo["Histogram"];
                            page = printReport.RenderReportPage(currentPageIndex, "Histogram");
                        }
                        if ((PageIdxs.ContainsKey("Pie") && AllPageInfo.ContainsKey("Pie")) && (currentPageIndex >= PageIdxs["Pie"] && currentPageIndex <= PageIdxs["Pie"] + AllPageInfo["Pie"].Count))
                        {
                            printReport.PieInfo = AllPageInfo["Pie"];
                            page = printReport.RenderReportPage(currentPageIndex, "Pie");
                        }
                        if ((PageIdxs.ContainsKey("Fit") && AllPageInfo.ContainsKey("Fit")) && (currentPageIndex >= PageIdxs["Fit"] && currentPageIndex <= PageIdxs["Fit"] + AllPageInfo["Fit"].Count))
                        {
                            printReport.FitInfo = AllPageInfo["Fit"];
                            page = printReport.RenderReportPage(currentPageIndex, "Fit");
                        }
                        if ((PageIdxs.ContainsKey("Arteriosclerosis") && AllPageInfo.ContainsKey("Arteriosclerosis")) && (currentPageIndex >= PageIdxs["Arteriosclerosis"] && currentPageIndex <= PageIdxs["Arteriosclerosis"] + AllPageInfo["Arteriosclerosis"].Count))
                        {
                            printReport.ArteriosclerosisInfo = AllPageInfo["Arteriosclerosis"];
                            page = printReport.RenderReportPage(currentPageIndex, "Arteriosclerosis");
                        }
                        // 如果是打印预览，可以缓存页面以便后续查看
                        if (IsPrintPreview(e))
                        {
                            ecgPages.Add(page);
                        }
                    }
                }
                else
                {
                    // 预先渲染模式：使用已渲染的页面
                    if (currentPageIndex < ecgPages.Count)
                    {
                        page = ecgPages[currentPageIndex];
                    }
                }

                if (page != null)
                {
                    // 计算打印区域，确保图像适应页面
                    Rectangle pageBounds = e.PageBounds;
                    Rectangle imageBounds = new Rectangle(0, 0, page.Width, page.Height);

                    // 保持宽高比进行缩放
                    float scale = Math.Min(
                        pageBounds.Width / (float)imageBounds.Width,
                        pageBounds.Height / (float)imageBounds.Height
                    );

                    int scaledWidth = (int)(imageBounds.Width * scale);
                    int scaledHeight = (int)(imageBounds.Height * scale);

                    // 居中显示
                    int x = (pageBounds.Width - scaledWidth) / 2;
                    int y = (pageBounds.Height - scaledHeight) / 2;

                    Rectangle destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

                    // 确保使用最高质量的图形设置
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    e.Graphics.DrawImage(page, destRect, imageBounds, GraphicsUnit.Pixel);

                    // 如果是按需渲染且不是预览模式，立即释放页面以节省内存
                    //if (bUseOnDemandRendering && !IsPrintPreview(e))
                    {
                        page.Dispose();
                        page = null;
                    }

                    currentPageIndex++;

                    // 如果还有更多页面，继续打印。
                    // 注意：必须按"页码范围末页"(toPage)判断，而不是总页数——
                    // 否则当起始页==结束页时，画完范围内唯一一页后仍会返回
                    // HasMorePages=true，GDI 会提交一张新的物理页，结果多打一张空白页。
                    // 未选择页码范围时 toPage == 总页数，行为不变。
                    e.HasMorePages = currentPageIndex < toPage;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
            catch (Exception ex)
            {
                e.HasMorePages = false;

                // 显示错误信息
                if (IsPrintPreview(e))
                {
                    Console.WriteLine($"渲染第{currentPageIndex + 1}页时发生错误: {ex.Message}");
                }

                // 确保资源被释放
                page?.Dispose();
            }
        }

        private bool IsPrintPreview(PrintPageEventArgs e)
        {
            // 在打印预览中，我们仍然使用按需渲染，不缓存页面
            // 返回false表示不缓存页面，实现真正的按需渲染
            return false;
        }

        private void PrintDocument_EndPrint(object sender, PrintEventArgs e)
        {
            // 打印完成后重置页码
            currentPageIndex = 0;

            if (e.PrintAction == PrintAction.PrintToPreview)
            {
                Console.WriteLine("预览完成");
            }
            else if (!e.Cancel)
            {
                Console.WriteLine($"成功打印 {ecgPages.Count} 页心电图");
            }
        }

        public DialogResult ShowPrintDialog()
        {
            if (!HasPrintableData())
            {
                Console.WriteLine("没有可打印的心电图数据");
                return DialogResult.Cancel;
            }

            using (PrintDialog printDialog = new PrintDialog())
            {
                printDialog.Document = printDocument;
                printDialog.AllowSomePages = true;
                int totalPages = bUseOnDemandRendering ? this.TotalPages : ecgPages.Count;
                printDialog.PrinterSettings.MinimumPage = 1;
                printDialog.PrinterSettings.MaximumPage = totalPages;
                printDialog.PrinterSettings.FromPage = 1;
                printDialog.PrinterSettings.ToPage = totalPages;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        printDocument.Print();
                        return DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"打印失败: {ex.Message}");
                        return DialogResult.Cancel;
                    }
                }

                return DialogResult.Cancel;
            }
        }

        private bool HasPrintableData()
        {
            if (bUseOnDemandRendering)
            {
                // 按需渲染模式：检查是否有心电数据和总页数
                return TotalPages > 0;
            }
            else
            {
                // 预先渲染模式：检查是否有已渲染的页面
                return ecgPages.Count > 0;
            }
        }
    }
}
