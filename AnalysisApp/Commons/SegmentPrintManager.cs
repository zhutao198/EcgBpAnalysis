using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalysisApp.Commons
{
    class SegmentPrintManager
    {
        private List<Bitmap> ecgPages;

        private PrintDocument printDocument;
        private PrintSegmentWave waveRenderer;
        //是否横向打印
        private bool bHorizontalPrint = false;
        public double PaperWidthMm = 210.0; // A4纸宽度
        public double PaperHeightMm = 297.0; // A4纸高度
        private const float PixMM = 3.7795f;          // 1mm 3.7795个像素点

        private bool bUseOnDemandRendering = true;  //按需渲染
        private int currentPageIndex = 0;
        //总页数
        public int TotalPages;
        public List<List<JObject>> SegmentAllPageInfo = new List<List<JObject>>();
        public SegmentPrintManager(bool bHorizontalPrint, int totalPages)
        {
            ecgPages = new List<Bitmap>();
            printDocument = new PrintDocument();
            waveRenderer = new PrintSegmentWave();
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

        /// <summary>
        /// 从指定打印机的纸张列表中查找驱动支持的 A4 纸型。
        /// 必须使用驱动列表中的 PaperSize 实例，否则会被当作自定义纸张而被驱动忽略，
        /// 打印作业退回驱动默认纸型——若默认纸型与实际装入的 A4 纸不符，
        /// 内容会按小纸型整页缩放，落到大纸上就呈现为"整体缩小、缩到纸面一角"。
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
            // 个别驱动把 A4 报告为自定义纸型，退而按名称匹配
            foreach (PaperSize ps in printerSettings.PaperSizes)
            {
                if (string.Equals(ps.PaperName, "A4", StringComparison.OrdinalIgnoreCase))
                {
                    return ps;
                }
            }
            return null;
        }

        private void PrintDocument_QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            // 逐页强制方向（双向）：防止打印对话框或打印机驱动（读取自身默认 DEVMODE 时）
            // 把方向改成与内容不符，从而无需再手动到打印机属性里设置
            e.PageSettings.Landscape = bHorizontalPrint;
            e.PageSettings.Margins = new Margins(0, 0, 0, 0);

            // 逐页显式锁定 A4 纸型（按当前实际使用的打印机解析），
            // 避免驱动默认纸型与实际纸张不符导致内容被缩小
            PaperSize a4 = FindA4PaperSize(e.PageSettings.PrinterSettings);
            if (a4 != null)
            {
                e.PageSettings.PaperSize = a4;
            }
        }

        public void SetOnDemandRendering()
        {
            bUseOnDemandRendering = true;
            currentPageIndex = 0;
            waveRenderer.TotalPages = TotalPages;
            waveRenderer.SegmentAllPageInfo = SegmentAllPageInfo;
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
                        page = waveRenderer.RenderEcgPage(currentPageIndex, bHorizontalPrint);
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
                    // 关键修复：以打印机"实际可打印区域"（Graphics 可视裁剪区，单位 1/100 英寸）
                    // 作为缩放与定位基准，而不是 e.PageBounds（整张纸的名义尺寸）。
                    // e.PageBounds 来自驱动当前纸型且包含硬件不可打印边距：
                    //   1) 驱动默认纸型与实际纸张不符时，按它缩放会导致内容整体缩小并偏到纸面一角；
                    //   2) 绘图原点是可打印区左上角，用它定位还会造成内容偏移、右/下被裁剪。
                    RectangleF drawBounds = e.Graphics.VisibleClipBounds;
                    Rectangle imageBounds = new Rectangle(0, 0, page.Width, page.Height);

                    // 保持宽高比进行缩放
                    float scale = Math.Min(
                        drawBounds.Width / page.Width,
                        drawBounds.Height / page.Height
                    );

                    int scaledWidth = (int)(page.Width * scale);
                    int scaledHeight = (int)(page.Height * scale);

                    // 在可打印区域内居中
                    int x = (int)(drawBounds.Left + (drawBounds.Width - scaledWidth) / 2);
                    int y = (int)(drawBounds.Top + (drawBounds.Height - scaledHeight) / 2);

                    Rectangle destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

                    // 确保使用最高质量的图形设置
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    e.Graphics.DrawImage(page, destRect, imageBounds, GraphicsUnit.Pixel);

                    // 如果是按需渲染且不是预览模式，立即释放页面以节省内存
                    if (bUseOnDemandRendering && !IsPrintPreview(e))
                    {
                        page.Dispose();
                        page = null;
                    }

                    currentPageIndex++;

                    // 如果还有更多页面，继续打印。
                    // 注意：必须按"页码范围末页"(toPage)判断，而不是总页数——
                    // 否则当起始页==结束页时，画完范围内唯一一页后仍会返回
                    // HasMorePages=true，GDI 会提交一张新的物理页，下一次回调
                    // 发现超出范围直接返回，结果多打一张空白页。
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
