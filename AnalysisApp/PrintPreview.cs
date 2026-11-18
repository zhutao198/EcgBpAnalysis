using AnalysisApp.CommonControls;
using AnalysisApp.Commons;
using AnalysisSystem.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalysisApp
{
    public partial class PrintPreview : BaseDialogForm
    {
        /// <summary>
        /// 打印预览数据传递类
        /// </summary>
        public class PrintPreviewData
        {
            public int TotalPages { get; set; }
            public Dictionary<string, int> PageIdxs { get; set; }
            public Dictionary<string, List<List<JObject>>> AllPageInfo { get; set; }
            public List<List<JObject>> SummaryInfo { get; set; }
            public List<List<JObject>> HourListInfo { get; set; }
            public List<List<JObject>> SegmentAllPageInfo { get; set; }
            public string Advice { get; set; }
            public bool IsHorizontalPrint { get; set; }
        }

        private PrintPreviewData _printData;
        private int CurrentPageIndex = 0;

        //private PrintReportPage printReportPage;

        bool bFlag = true;
        private double _zoom;
        Bitmap pageBitmap;

        private DateTime _lastWheelTime;

        public PrintPreviewData PrintData
        {
            get { return _printData; }
            set
            {
                _printData = value;
                InitializePrintPreview();
            }
        }

        public PrintPreview() : base("", "打印预览")
        {
            InitializeComponent();

            // 设置窗口为全屏模式但不遮挡任务栏
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            // 设置窗口大小为工作区大小（不包括任务栏）
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            // 设置窗口位置为屏幕左上角
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;
        }

        private void PrintPreview_Load(object sender, EventArgs e)
        {
            this.TopMost = false;
            
            // 在窗体加载时重新初始化预览（确保PrintData已设置）
            if (_printData != null)
            {
                InitializePrintPreview();
            }
        }

        private void InitializePrintPreview()
        {
            if (_printData == null) return;

            // 设置纸张方向：与 printReport() 中的纵向/横向判定保持一致，
            // 否则横向片段在预览中会显示为纵向纸张
            bool bHorizontalPrint = false;
            if (!_printData.PageIdxs.ContainsKey("summary") && !_printData.PageIdxs.ContainsKey("hourlist")
                && _printData.PageIdxs.ContainsKey("segment") && _printData.AllPageInfo.ContainsKey("segment")
                && _printData.AllPageInfo["segment"].Count > 0)
            {
                int direction = Convert.ToInt32(_printData.AllPageInfo["segment"][0][0]["direction"]);
                bHorizontalPrint = direction == 0;
            }
            printDocument.DefaultPageSettings.Landscape = bHorizontalPrint;
            // PaperSize 单位为 1/100 英寸，标准 A4 = 827 x 1169
            //printDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

            // 设置打印文档
            printDocument.DocumentName = "心电图分析报告";
            printDocument.PrintPage += PrintDocument_PrintPageForPreview;

            // 设置预览控件
            printPreviewControl.Document = printDocument;
            printPreviewControl.StartPage = 0;
            printPreviewControl.Zoom = 0.83;
            //printPreviewControl.MouseWheel += PrintPreview_MouseWheel;

            _zoom = Math.Round(printPreviewControl.Zoom, 3, MidpointRounding.AwayFromZero);
            this.cbxPercent.Text = (_zoom * 100).ToString() + "%";
        }

        private void PrintDocument_PrintPageForPreview(object sender, PrintPageEventArgs e)
        {
            try
            {
                // 渲染当前页
                Bitmap page = printReport(CurrentPageIndex);

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

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.DrawImage(page, destRect, imageBounds, GraphicsUnit.Pixel);

                    // 释放资源
                    page.Dispose();

                    // 更新页码并判断是否有更多页面
                    CurrentPageIndex++;
                    e.HasMorePages = CurrentPageIndex < _printData.TotalPages;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
            catch (Exception ex)
            {
                e.HasMorePages = false;
                _ = MessageBox.Show($"渲染第{CurrentPageIndex + 1}页时发生错误: {ex.Message}",
                              "渲染错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void RenderEcgReport(int pageIndex)
        {
            pageBitmap = await Task.Run(() => printReport(pageIndex));
        }

        private Bitmap printReport(int pageIndex)
        {
            Bitmap page = null;
            if (pageIndex < _printData.TotalPages)
            {
                bool bHorizontalPrint = false;

                if (!_printData.PageIdxs.ContainsKey("summary") && !_printData.PageIdxs.ContainsKey("hourlist"))
                {
                    if(_printData.PageIdxs.ContainsKey("segment") && _printData.AllPageInfo.ContainsKey("segment"))
                    {
                        int direction = Convert.ToInt32(_printData.AllPageInfo["segment"][0][0]["direction"]);
                        bHorizontalPrint = direction == 0 ? true : false;
                    }

                }

                PrintReportPage printReportPage = new PrintReportPage();
                printReportPage.TotalPages = _printData.TotalPages;
                if ((_printData.PageIdxs.ContainsKey("summary") && _printData.AllPageInfo.ContainsKey("summary")) && (pageIndex >= _printData.PageIdxs["summary"] && pageIndex < _printData.PageIdxs["summary"] + _printData.AllPageInfo["summary"].Count))
                {
                    printReportPage.SummaryInfo = _printData.AllPageInfo["summary"];
                    printReportPage.advice = _printData.Advice;
                    page = printReportPage.RenderReportPage(pageIndex, "summary");
                }
                if ((_printData.PageIdxs.ContainsKey("hourlist") && _printData.AllPageInfo.ContainsKey("hourlist")) && (pageIndex >= _printData.PageIdxs["hourlist"] && pageIndex <= _printData.PageIdxs["hourlist"] + _printData.AllPageInfo["hourlist"].Count))
                {
                    page = printReportPage.RenderReportPage(pageIndex, "hourlist");
                }
                if ((_printData.PageIdxs.ContainsKey("segment") && _printData.AllPageInfo.ContainsKey("segment")) && (pageIndex >= _printData.PageIdxs["segment"] && pageIndex < _printData.PageIdxs["segment"] + _printData.AllPageInfo["segment"].Count))
                {
                    PrintSegmentWave SegmentWaveRenderer = new PrintSegmentWave();
                    SegmentWaveRenderer.TotalPages = _printData.TotalPages;
                    SegmentWaveRenderer.SegmentAllPageInfo = _printData.AllPageInfo["segment"];
                    page = SegmentWaveRenderer.RenderEcgPage(pageIndex - _printData.PageIdxs["segment"], bHorizontalPrint, pageIndex);
                }
            }

            return page;
        }

        private void UpdatePageInfo()
        {
            txtPage.Text = $"{ printPreviewControl.StartPage + 1}";
            labTotal.Text = _printData.TotalPages.ToString();

            // 更新按钮状态
            btnFirstPage.Enabled = printPreviewControl.StartPage > 0;
            btnLastPage.Enabled = printPreviewControl.StartPage > 0;
            btnNextPage.Enabled = printPreviewControl.StartPage < _printData.TotalPages - 1;
            btnEndPage.Enabled = printPreviewControl.StartPage < _printData.TotalPages - 1;
        }

        // 工具栏按钮事件处理
        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            _zoom = Math.Round(_zoom * 1.1, 3, MidpointRounding.AwayFromZero);
            cbxPercent.Text = (_zoom * 100).ToString() + "%";
            printPreviewControl.Zoom = _zoom;
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            _zoom = Math.Round(_zoom * 0.909, 3, MidpointRounding.AwayFromZero);
            cbxPercent.Text = (_zoom * 100).ToString() + "%";
            printPreviewControl.Zoom = _zoom;
        }

        private void cbxPercent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxPercent.SelectedItem != null)
            {
                string percentStr = cbxPercent.SelectedItem.ToString().Replace("%", "");
                if (double.TryParse(percentStr, out double percent))
                {
                    printPreviewControl.Zoom = percent / 100.0;
                }
            }
        }

        private void btnFirstPage_Click(object sender, EventArgs e)
        {
            if (_printData != null && printPreviewControl.StartPage > 0)
            {
                printPreviewControl.StartPage = 0;
                // 更新页面信息
                UpdatePageInfo();
            }
        }

        private void btnLastPage_Click(object sender, EventArgs e)
        {
            if (_printData != null && printPreviewControl.StartPage > 0)
            {
                printPreviewControl.StartPage--;
                // 更新页面信息
                UpdatePageInfo();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (_printData != null && printPreviewControl.StartPage < _printData.TotalPages - 1)
            {
                printPreviewControl.StartPage++;
                // 更新页面信息
                UpdatePageInfo();
            }
        }

        private void btnEndPage_Click(object sender, EventArgs e)
        {
            if (_printData != null && printPreviewControl.StartPage < _printData.TotalPages - 1)
            {
                printPreviewControl.StartPage = _printData.TotalPages - 1;
                // 更新页面信息
                UpdatePageInfo();
            }
        }

        private void PrintPreview_MouseWheel(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastWheelTime < TimeSpan.FromMilliseconds(30))
            {
                return;
            }
            // 检查是否正在加载页面，防止重复操作
            if (bFlag)
                return;

            // 根据滚轮方向翻页
            if (e.Delta > 0)
            {
                //// 向上滚动 - 上一页
                //_currentPageIndex--;
                //if (_currentPageIndex < 0)
                //{
                //    _currentPageIndex = 0;
                //    return;
                //}
                //bFlag = true;
                //// 强制刷新预览控件，触发PrintDocument_PrintPage
                //printPreviewControl.InvalidatePreview();

                //// 更新页面信息
                //UpdatePageInfo();
            }
            else if (e.Delta < 0)
            {
                //// 向下滚动 - 下一页
                //// 递增页面索引
                //_currentPageIndex++;
                //if (_currentPageIndex > _printData.TotalPages - 1)
                //{
                //    _currentPageIndex = _printData.TotalPages - 1;
                //    return;
                //}
                //bFlag = true;
                //// 强制刷新预览控件，触发PrintDocument_PrintPage
                //printPreviewControl.InvalidatePreview();

                //// 更新页面信息
                //UpdatePageInfo();
            }
            _lastWheelTime = DateTime.Now;

        }

        private void txtPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (int.TryParse(txtPage.Text, out int page) && _printData != null)
                {
                    if (page >= 1 && page <= _printData.TotalPages)
                    {
                        //_currentPageIndex = page - 1;
                        //bFlag = true;
                        //// 强制刷新预览控件，触发PrintDocument_PrintPage
                        //printPreviewControl.InvalidatePreview();
                        
                        //// 更新页面信息
                        //UpdatePageInfo();
                    }
                }
            }
        }


        private void btnPrint_Click(object sender, EventArgs e)
        {
            EcgReportControl reportPage = new EcgReportControl();
            reportPage.initPrintData();
            reportPage.printReport();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            CurrentPageIndex = 0;
            this.Close();
        }
    }
}
