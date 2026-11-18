using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace AnalysisApp.Commons
{
    public class EcgPrintManager
    {
        private List<Bitmap> ecgPages;
        private int currentPageIndex;
        private PrintDocument printDocument;
        private PrintEcgWaveRenderer waveRenderer;
        //是否按需渲染
        private bool bUseOnDemandRendering;
        private const float pixMM = 3.7795f;          // 1mm 3.7795个像素点

        // 按需渲染的数据字段
        public List<WaveInfo> EcgData;
        public List<BeatInfo> heartbeatInfos = new List<BeatInfo>();
        //总页数
        public int totalPages;
        //每页每导联绘制的行数
        public int currentRows = 1;
        //每行数据点的个数
        public int RowPoints;
        public double PaperWidthMm = 210.0; // A4纸宽度
        public double PaperHeightMm = 297.0; // A4纸高度
        public double speed = 25;
        public double gain = 10;
        public int startSample = 0;
        //标签
        public string label = "";
        //是否横向打印
        public bool bHorizontalPrint = false;
        //打印总采样点数
        public int totalPrintPoints = 0;
        //导联索引
        public List<int> leadIdxs = new List<int>();
        public EcgPrintManager(bool bHorizontalPrint)
        {
            ecgPages = new List<Bitmap>();
            currentPageIndex = 0;
            printDocument = new PrintDocument();
            waveRenderer = new PrintEcgWaveRenderer();
            bUseOnDemandRendering = false;
            this.bHorizontalPrint = bHorizontalPrint;
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
            // 设置纸张大小为A4
            //PaperSize paperSize = new PaperSize("A4", 827, 1169); // A4 in 1/100 inch (NOT render pixels)
            //printDocument.DefaultPageSettings.PaperSize = paperSize;

            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.QueryPageSettings += PrintDocument_QueryPageSettings;
            printDocument.EndPrint += PrintDocument_EndPrint;
        }

        private void PrintDocument_QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            // Force page orientation on every page (BOTH ways) so that neither the print
            // dialog nor the printer driver (which may re-apply its own default DEVMODE)
            // can flip the orientation to something that does not match the content.
            e.PageSettings.Landscape = bHorizontalPrint;
            e.PageSettings.Margins = new Margins(0, 0, 0, 0);

            // Lock the paper to A4 explicitly (resolved from the printer actually in use).
            // Without this, the print job follows the DRIVER'S DEFAULT paper size; if that
            // default differs from the physical A4 sheet, the whole page bitmap gets scaled
            // down to the default paper size and lands in one corner of the sheet.
            PaperSize a4 = FindA4PaperSize(e.PageSettings.PrinterSettings);
            if (a4 != null)
            {
                e.PageSettings.PaperSize = a4;
            }
        }

        /// <summary>
        /// Find an A4 PaperSize instance from the driver's own paper list.
        /// Must use an instance from PrinterSettings.PaperSizes, otherwise it is treated
        /// as a custom (driver-unknown) form and silently ignored by the driver.
        /// </summary>
        private PaperSize FindA4PaperSize(PrinterSettings printerSettings)
        {
            foreach (PaperSize ps in printerSettings.PaperSizes)
            {
                if (ps.Kind == PaperKind.A4)
                {
                    return ps;
                }
            }
            // Some drivers report A4 as a custom form, fall back to name matching
            foreach (PaperSize ps in printerSettings.PaperSizes)
            {
                if (string.Equals(ps.PaperName, "A4", StringComparison.OrdinalIgnoreCase))
                {
                    return ps;
                }
            }
            return null;
        }

        public void SetEcgPages(List<Bitmap> pages)
        {
            ecgPages = pages ?? new List<Bitmap>();
            currentPageIndex = 0;
            bUseOnDemandRendering = false;
        }

        public void SetOnDemandRendering()
        {
            bUseOnDemandRendering = true;
            currentPageIndex = 0;
            waveRenderer.ecgData = EcgData;
            waveRenderer.heartbeatInfos = heartbeatInfos;
            waveRenderer.totalPages = totalPages;
            waveRenderer.RowPoints = RowPoints;
            waveRenderer.currentRows = currentRows;
            waveRenderer.leadIdxs = leadIdxs;
            waveRenderer.speed = speed;
            waveRenderer.gain = gain;
            waveRenderer.label = label;
            waveRenderer.startSample = startSample;
            waveRenderer.bHorizontalPrint = bHorizontalPrint;
            waveRenderer.totalPrintPoints = totalPrintPoints;

            // 清理预先渲染的页面
            foreach (Bitmap page in ecgPages)
            {
                page?.Dispose();
            }
            ecgPages.Clear();
        }

        private bool HasPrintableData()
        {
            if (bUseOnDemandRendering)
            {
                // 按需渲染模式：检查是否有心电数据和总页数
                return EcgData != null &&
                       EcgData.Count > 0 &&
                       EcgData[0].DataLength > 0 &&
                       totalPages > 0;
            }
            else
            {
                // 预先渲染模式：检查是否有已渲染的页面
                return ecgPages.Count > 0;
            }
        }

        public void PrintEcgReport(string printerName = null)
        {
            if (!HasPrintableData())
            {
                Console.WriteLine("没有可打印的心电图数据");
                return;
            }

            if (!string.IsNullOrEmpty(printerName))
            {
                printDocument.PrinterSettings.PrinterName = printerName;
            }

            if (!printDocument.PrinterSettings.IsValid)
            {
                Console.WriteLine("指定的打印机无效");
                return;
            }

            try
            {
                printDocument.Print();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打印失败: {ex.Message}");
            }
        }

        public DialogResult ShowPrintPreview()
        {
            if (!HasPrintableData())
            {
                Console.WriteLine("没有可预览的心电图数据");
                return DialogResult.Cancel;
            }

            using (PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog())
            {
                printPreviewDialog.Document = printDocument;
                printPreviewDialog.WindowState = FormWindowState.Maximized;
                printPreviewDialog.PrintPreviewControl.Zoom = 1.0;

                return printPreviewDialog.ShowDialog();
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
                int totalPages = bUseOnDemandRendering ? this.totalPages : ecgPages.Count;
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

        public void SaveEcgPagesToFiles(string directoryPath, string filePrefix = "ECG_Page")
        {
            if (!HasPrintableData())
            {
                Console.WriteLine("没有可保存的心电图数据");
                return;
            }

            if (!System.IO.Directory.Exists(directoryPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"无法创建目录: {ex.Message}");
                    return;
                }
            }

            try
            {
                int savedPages = 0;

                if (bUseOnDemandRendering)
                {
                    // 按需渲染模式：渲染每页并保存
                    for (int i = 0; i < totalPages; i++)
                    {
                        using (Bitmap page = waveRenderer.RenderEcgPage(i))
                        {
                            string fileName = $"{filePrefix}_{i + 1:000}.png";
                            string fullPath = System.IO.Path.Combine(directoryPath, fileName);
                            page.Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);
                            savedPages++;
                        }
                    }
                }
                else
                {
                    // 预先渲染模式：直接保存已渲染的页面
                    for (int i = 0; i < ecgPages.Count; i++)
                    {
                        string fileName = $"{filePrefix}_{i + 1:000}.png";
                        string fullPath = System.IO.Path.Combine(directoryPath, fileName);
                        ecgPages[i].Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);
                        savedPages++;
                    }
                }

                Console.WriteLine($"成功保存 {savedPages} 页的心电图 {directoryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存失败: {ex.Message}");
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Bitmap page = null;

            try
            {

                // Page-range filtering: PrintDocument does NOT automatically honor
                // the page range selected in the print dialog. We must read
                // PrinterSettings here and skip pages outside [fromPage, toPage].
                int totalPageCount = bUseOnDemandRendering ? totalPages : ecgPages.Count;
                int fromPage = 1;
                int toPage = totalPageCount;
                if (printDocument.PrinterSettings.PrintRange == PrintRange.SomePages)
                {
                    fromPage = Math.Max(1, printDocument.PrinterSettings.FromPage);
                    toPage = Math.Min(totalPageCount, printDocument.PrinterSettings.ToPage);
                }

                // Pages before the range: jump straight to the first page in range
                // so no blank physical pages are printed
                if (currentPageIndex < fromPage - 1)
                {
                    currentPageIndex = fromPage - 1;
                }

                // Beyond the range: stop printing
                if (currentPageIndex >= toPage)
                {
                    e.HasMorePages = false;
                    return;
                }
                if (bUseOnDemandRendering)
                {
                    // 按需渲染模式：打印一页渲染一页
                    if (currentPageIndex < totalPages)
                    {
                        page = waveRenderer.RenderEcgPage(currentPageIndex);
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
                    // 计算打印区域，确保图像适应页面。
                    // Fix: scale/position against the ACTUAL printable area of the printer
                    // (Graphics visible clip, in 1/100 inch), not e.PageBounds.
                    // e.PageBounds is the nominal paper size reported by the driver (including
                    // hardware unprintable margins). If the driver's default paper differs from
                    // the physical sheet, scaling by it shrinks the whole content into a corner.
                    RectangleF drawBounds = e.Graphics.VisibleClipBounds;
                    Rectangle imageBounds = new Rectangle(0, 0, page.Width, page.Height);

                    // 保持宽高比进行缩放
                    float scale = Math.Min(
                        drawBounds.Width / page.Width,
                        drawBounds.Height / page.Height
                    );

                    int scaledWidth = (int)(imageBounds.Width * scale);
                    int scaledHeight = (int)(imageBounds.Height * scale);

                    // 居中显示
                    // Center within the printable area
                    int x = (int)(drawBounds.Left + (drawBounds.Width - scaledWidth) / 2);
                    int y = (int)(drawBounds.Top + (drawBounds.Height - scaledHeight) / 2);

                    Rectangle destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.DrawImage(page, destRect, imageBounds, GraphicsUnit.Pixel);

                    // 如果是按需渲染且不是预览模式，立即释放页面以节省内存
                    if (bUseOnDemandRendering && !IsPrintPreview(e))
                    {
                        page.Dispose();
                        page = null;
                    }

                    currentPageIndex++;

                    // 如果还有更多页面，继续打印。
                    // Fix: compare against toPage (end of the selected page range),
                    // not the total page count. Otherwise, when FromPage == ToPage,
                    // after rendering the single page in range HasMorePages stays
                    // true, GDI commits one more physical sheet and the next
                    // callback returns immediately -> an extra blank page.
                    // When no range is selected, toPage == total page count.
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

        public void Dispose()
        {
            foreach (Bitmap page in ecgPages)
            {
                page?.Dispose();
            }
            ecgPages.Clear();
            printDocument?.Dispose();
        }

        public List<string> GetAvailablePrinters()
        {
            List<string> printers = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
            return printers;
        }
    }
}
