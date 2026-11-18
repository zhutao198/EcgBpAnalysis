using AnalysisApp.Commons;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component.Controls;

namespace AnalysisApp.CommonControls
{
    public partial class HRTrendWaveControl : BaseDrawingControl
    {
        private int width = 0;
        private int height = 0;

        DrawHRTrendWave drawHRTrendWave = null;

        // 鼠标点击相关变量
        private float clickSeconds = 0;               //点击在第几秒
        private string clickTimeText = "";           // 点击时间文本
        private string clickAvgHRText = "";         // 点击位置的平均心率文本
        
        // 矩形选中框相关变量
        private bool isDrawingRect = false;          // 是否正在绘制矩形
        private SKPoint rectStartPoint;              // 矩形起始点
        private SKPoint rectEndPoint;                // 矩形结束点
        private SKRect selectionRect = SKRect.Empty; // 选中矩形区域

        private DateTime selectionStartTime;
        private DateTime selectionEndTime;

        public List<AfInfo> afs = new List<AfInfo>();

        public HRTrendWaveControl()
        {
            InitializeComponent();
        }

        public void RefreshData()
        {
            //_holterConfig = AnaValues.holterConfig;
            ForceWaveformRedraw();
        }

        private void HRTrendWaveControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                initControl();

                _graphicsControl?.Invalidate();

            }
        }

        private void initControl()
        {
            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            panel1.Controls.Add(_graphicsControl);

            // 初始化右键菜单
            InitializeContextMenu();

            drawHRTrendWave = new DrawHRTrendWave();

        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            // 清除画布
            canvas.Clear(SKColors.White);

            // 确保离屏缓冲区已更新
            width = imageWidth;
            height = imageHeight;


            drawHRTrendWave.TopMargin = 30;
            drawHRTrendWave.LeftMargin = 30;
            drawHRTrendWave.BottomMargin = 10;
            drawHRTrendWave.RightMargin = 15;
            // 绘制背景
            drawHRTrendWave.DrawBackground(canvas, width, height);
            // 绘制ECG波形
            drawHRTrendWave.DrawCoordinateSystem(canvas, width, height);
            drawHRTrendWave.DrawHRTrend(canvas, width, height);


            // 绘制点击位置的纵向直线
            DrawClickLines(canvas);

            // 绘制点击时间和平均心率信息
            DrawClickInfo(canvas);

            // 绘制矩形选中框
            DrawSelectionRectangles(canvas);

            //绘制房扑房颤矩形框
            DrawAFRect(canvas);
        }

        #region 鼠标事件

        // 初始化右键菜单
        private void InitializeContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem setAtrialFibrillationItem = new ToolStripMenuItem(CommonValues.Langs["设为房颤"]);
            setAtrialFibrillationItem.Click += SetAtrialFibrillationItem_Click;
            _ = contextMenu.Items.Add(setAtrialFibrillationItem);

            ToolStripMenuItem setAtrialFlutterItem = new ToolStripMenuItem(CommonValues.Langs["设为房扑"]);
            setAtrialFlutterItem.Click += SetAtrialFlutterItem_Click;
            _ = contextMenu.Items.Add(setAtrialFlutterItem);

            ToolStripMenuItem deleteAFItem = new ToolStripMenuItem(CommonValues.Langs["删除房颤/房扑"]);
            deleteAFItem.Click += DeleteAFItem_Click;
            contextMenu.Items.Add(deleteAFItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem setAllAtrialFibrillationItem = new ToolStripMenuItem(CommonValues.Langs["设为全程房颤"]);
            setAllAtrialFibrillationItem.Click += SetAllAtrialFibrillationItem_Click;
            _ = contextMenu.Items.Add(setAllAtrialFibrillationItem);

            ToolStripMenuItem setAllAtrialFlutterItem = new ToolStripMenuItem(CommonValues.Langs["设为全程房扑"]);
            setAllAtrialFlutterItem.Click += SetAllAtrialFlutterItem_Click;
            _ = contextMenu.Items.Add(setAllAtrialFlutterItem);

            ToolStripMenuItem deleteAllAFItem = new ToolStripMenuItem(CommonValues.Langs["删除所有房颤/房扑"]);
            deleteAllAFItem.Click += DeleteAllAFItem_Click;
            contextMenu.Items.Add(deleteAllAFItem);

            _graphicsControl.ContextMenuStrip = contextMenu;
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 清除上一次的矩形
                selectionRect = SKRect.Empty;
                
                // 开始绘制新矩形
                isDrawingRect = true;
                rectStartPoint = new SKPoint(e.X, e.Y);
                rectEndPoint = rectStartPoint;
                
                // 计算点击位置对应的时间偏移并设置波形起始时间
                float timeOffsetSeconds = CalculateTimeOffsetFromClick(e.X);                
                SetWaveformStartTime(timeOffsetSeconds);

                // 记录点击位置的时刻信息
                RecordClickTimeInfo(e.X);

                // 强制重绘以显示纵向直线
                ForceWaveformRedraw();
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            // 只有在绘制矩形模式下才更新矩形
            if (isDrawingRect)
            {
                // 更新矩形结束点
                rectEndPoint = new SKPoint(e.X, e.Y);

                float left = Math.Min(rectStartPoint.X, rectEndPoint.X);
                float top = drawHRTrendWave.TopMargin;
                float right = Math.Max(rectStartPoint.X, rectEndPoint.X);
                float bottom = height - drawHRTrendWave.BottomMargin - drawHRTrendWave.HourLabelHeight;

                selectionRect = new SKRect(left, top, right, bottom);

                ForceWaveformRedraw();
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawingRect)
            {
                // 结束绘制矩形
                isDrawingRect = false;
                // 记录选中区域的时间范围
                RecordSelectionTimeRange(selectionRect);
            }
        }
        private void SetAtrialFibrillationItem_Click(object sender, EventArgs e)
        {
            if (selectionRect != SKRect.Empty && selectionRect.Width > 0)
            {
                SegmentRange segment = AnaCode.GetSegment(selectionStartTime, selectionEndTime);
                int ret = AEcgHandler.AddAf(segment.StartIndex, segment.EndIndex, 'K');
                if (ret == 0)
                {
                    base.ChangeDataSource(null);
                }
                else { CommonFuns.PromptError(CommonValues.Langs["操作失败"]); } 
            }
        }
        private void SetAtrialFlutterItem_Click(object sender, EventArgs e)
        {
            if (selectionRect != SKRect.Empty && selectionRect.Width > 0)
            {
                SegmentRange segment = AnaCode.GetSegment(selectionStartTime, selectionEndTime);
                int ret = AEcgHandler.AddAf(segment.StartIndex, segment.EndIndex, 'M');
                if (ret == 0)
                    base.ChangeDataSource(null);
                else
                    CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
            }
        }
        private void DeleteAFItem_Click(object sender, EventArgs e)
        {
            if (selectionRect != SKRect.Empty && selectionRect.Width > 0)
            {
                SegmentRange segment = AnaCode.GetSegment(selectionStartTime, selectionEndTime);
                var segments = new SegmentRange[] { segment };
                int ret75 = AEcgHandler.DeleteAFEvents(segments, 'K');
                int ret77 = AEcgHandler.DeleteAFEvents(segments, 'M');
                if (ret75 == 0 && ret77 == 0)
                    base.ChangeDataSource(null);
                else
                    CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
            }
        }
        private void SetAllAtrialFibrillationItem_Click(object sender, EventArgs e)
        {
            int ret = AEcgHandler.AddAf(0, _holterConfig.Total, 'K');
            if (ret == 0)
                base.ChangeDataSource(null);
            else
                CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
        }
        private void SetAllAtrialFlutterItem_Click(object sender, EventArgs e)
        {
            int ret = AEcgHandler.AddAf(0, _holterConfig.Total, 'M');
            if (ret == 0)
                base.ChangeDataSource(null);
            else
                CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
        }
        private void DeleteAllAFItem_Click(object sender, EventArgs e)
        {
            DateTime endTime = _holterConfig.StartDateTime.AddSeconds(_holterConfig.TotalSeconds+1);
            SegmentRange segment = AnaCode.GetSegment(_holterConfig.StartDateTime, endTime);
            var segments = new SegmentRange[] { segment };
            int ret75 = AEcgHandler.DeleteAFEvents(segments, 'K');
            int ret77 = AEcgHandler.DeleteAFEvents(segments, 'M');
            if (ret75 == 0 && ret77 == 0)
                base.ChangeDataSource(null);
            else
                CommonFuns.PromptError(CommonValues.Langs["操作失败"]);
        }
        
        /// <summary>
        /// 绘制点击位置的纵向直线
        /// </summary>
        private void DrawClickLines(SKCanvas canvas)
        {
            if (clickSeconds == 0)
                return;
                
            using (var linePaint = new SKPaint
            {
                Color = SKColors.Red,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                // 计算点击位置对应的时间（分钟）
                int plotWidth = width - drawHRTrendWave.LeftMargin - drawHRTrendWave.RightMargin;
                float relativeX = drawHRTrendWave.LeftMargin + ((float)plotWidth / (24*3600)) * clickSeconds;

                // 绘制纵向直线
                canvas.DrawLine(relativeX, drawHRTrendWave.TopMargin, relativeX, height-drawHRTrendWave.BottomMargin-drawHRTrendWave.HourLabelHeight, linePaint);
            }
        }
        
        /// <summary>
        /// 记录点击位置的时刻信息
        /// </summary>
        private void RecordClickTimeInfo(float x)
        {
            // 计算对应的分钟数
            int minuteIndex = Convert.ToInt32(clickSeconds / 60);

            if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
            {
                startTime = DateTime.Today;
            }

            DateTime currentTime = startTime.AddSeconds(clickSeconds);
            // 格式化时间字符串
            string timeStr = currentTime.ToString("HH:mm");
            // 获取该分钟的平均心率
            int avgHR = GetAverageHRForMinute(minuteIndex);
            // 记录点击时间文本和平均心率文本
            clickTimeText = timeStr;
            clickAvgHRText = $"{avgHR} bpm";
        }
        
        /// <summary>
        /// 获取指定分钟的平均心率
        /// </summary>
        private int GetAverageHRForMinute(int minuteIndex)
        {
            if (AnaValues.minuteHR == null || minuteIndex < 0 || minuteIndex >= AnaValues.minuteHR.Count)
                return 0;
                
            // 获取该分钟的心率信息
            var minuteHRInfo = AnaValues.minuteHR[minuteIndex];
            return minuteHRInfo?.Avg ?? 0;
        }
        
        /// <summary>
        /// 绘制点击时间和平均心率信息
        /// </summary>
        private void DrawClickInfo(SKCanvas canvas)
        {
            if (string.IsNullOrEmpty(clickTimeText) || string.IsNullOrEmpty(clickAvgHRText))
                return;

            // 创建文本绘制画笔
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,

                IsAntialias = true
            })

            using (SKFont textFont = new SKFont
            {
                Size = 14,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                float pixMM = 3.777f;
                // 绘制时间文本
                canvas.DrawText(clickTimeText, drawHRTrendWave.LeftMargin + 3 * pixMM, drawHRTrendWave.TopMargin-2*pixMM, SKTextAlign.Left, textFont, textPaint);

                // 绘制平均心率文本
                canvas.DrawText(clickAvgHRText, drawHRTrendWave.LeftMargin + 15 * pixMM, drawHRTrendWave.TopMargin - 2 * pixMM, SKTextAlign.Left, textFont, textPaint);
            }
        }
        
        /// <summary>
        /// 强制重绘波形
        /// </summary>
        public void ForceWaveformRedraw()
        {
            _graphicsControl?.Invalidate();
        }
        
        /// <summary>
        /// 绘制矩形选中框
        /// </summary>
        private void DrawSelectionRectangles(SKCanvas canvas)
        {
            using (var rectPaint = new SKPaint
            {
                Color = new SKColor(74, 122, 196,128), // 半透明蓝色
                StrokeWidth = 2,
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                BlendMode = SKBlendMode.SrcOver
            })
            {
                // 绘制当前正在绘制的矩形
                if (isDrawingRect)
                {
                    canvas.DrawRect(selectionRect, rectPaint);
                }
            }
        }

        private void DrawAFRect(SKCanvas canvas)
        {
            if (afs != null && afs.Count > 0)
            {
                using (SKPaint afPaint = new SKPaint
                {
                    Color = new SKColor(128, 128, 128, 128),
                    StrokeWidth = 1,
                    StrokeJoin = SKStrokeJoin.Round,
                    Style = SKPaintStyle.StrokeAndFill,
                    IsAntialias = true,
                    BlendMode = SKBlendMode.SrcOver
                })
                using (SKPaint textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true
                })
                using (SKFont textFont = new SKFont
                {
                    Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Normal),
                    Size = 9f
                })
                {
                    if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
                    {
                        startTime = DateTime.Today;
                    }
                    float x0 = drawHRTrendWave.LeftMargin;
                    int plotWidth = width - drawHRTrendWave.LeftMargin - drawHRTrendWave.RightMargin;
                    float preWidthMin = (float)plotWidth / (24 * 60);
                    foreach (AfInfo af in afs)
                    {
                        double startMin = af.StartTime.Subtract(startTime).TotalMinutes;
                        double endMin = af.EndTime.Subtract(startTime).TotalMinutes;
                        double left = x0 + startMin * preWidthMin;
                        double right = x0 + endMin * preWidthMin;
                        float bottom = height - drawHRTrendWave.BottomMargin - drawHRTrendWave.HourLabelHeight;
                        SKRect rect = new SKRect((float)left, bottom - 15, (float)right, bottom - 1);
                        if (rect.Width == 0)
                        {
                            afPaint.Color = SKColors.Black;
                        }
                        else
                        {
                            afPaint.Color = new SKColor(128, 128, 128, 128);
                        }

                        canvas.DrawRect(rect, afPaint);

                        float textW = textFont.MeasureText(af.TypeName, out SKRect bound, textPaint);
                        if (textW < rect.Width)
                        {
                            SKPoint textPos = new SKPoint(rect.Left + (rect.Width - textW) / 2, rect.Top + rect.Height / 2 + bound.Height / 2);
                            canvas.DrawText(af.TypeName, textPos, SKTextAlign.Left, textFont, textPaint);
                        }

                    }
                }
            }
        }
        
        /// <summary>
        /// 记录选中区域的时间范围
        /// </summary>
        private void RecordSelectionTimeRange(SKRect rect)
        {
            if (drawHRTrendWave == null || AnaValues.minuteHR == null)
                return;
                
            // 计算选中区域的起始和结束时间
            int plotWidth = width - drawHRTrendWave.LeftMargin - drawHRTrendWave.RightMargin;
            
            // 计算起始X坐标对应的时间
            float startRelativeX = rect.Left - drawHRTrendWave.LeftMargin;
            float endRelativeX = rect.Right - drawHRTrendWave.LeftMargin;
            
            if (startRelativeX < 0) startRelativeX = 0;
            if (endRelativeX > plotWidth) endRelativeX = plotWidth;

            double preMinWidth = (double)60 * 24 / plotWidth;
            // 计算对应的分钟数
            int startMinuteIndex = (int)(preMinWidth * startRelativeX);
            int endMinuteIndex = (int)(preMinWidth * endRelativeX);
            
            startMinuteIndex = Math.Max(0, Math.Min(startMinuteIndex, AnaValues.minuteHR.Count - 1));
            endMinuteIndex = Math.Max(0, Math.Min(endMinuteIndex, AnaValues.minuteHR.Count - 1));

            selectionStartTime = _holterConfig.StartDateTime.AddSeconds(startMinuteIndex * 60);
            selectionEndTime = _holterConfig.StartDateTime.AddSeconds(endMinuteIndex * 60);

        }
        
        /// <summary>
        /// 清除点击标记
        /// </summary>
        public void ClearClickMarks()
        {
            clickSeconds = 0;
            ForceWaveformRedraw();
        }
        
        /// <summary>
        /// 清除矩形选中框
        /// </summary>
        public void ClearSelectionRects()
        {
            selectionRect = SKRect.Empty;
            ForceWaveformRedraw();
        }
        
        /// <summary>
        /// 波形起始时间设置事件
        /// </summary>
        public event Action<float> OnHRTrendStartTimeChanged;
        
        /// <summary>
        /// 计算点击位置对应的时间偏移（秒）
        /// </summary>
        private float CalculateTimeOffsetFromClick(float x)
        {
            if (drawHRTrendWave == null || AnaValues.minuteHR == null)
                return 0;
                
            // 计算点击位置对应的时间（分钟）
            int plotWidth = width - drawHRTrendWave.LeftMargin - drawHRTrendWave.RightMargin;
            float relativeX = x - drawHRTrendWave.LeftMargin;
            
            if (relativeX < 0) relativeX = 0;
            if (relativeX > plotWidth) relativeX = plotWidth;

            clickSeconds = ((float)24 * 3600 / plotWidth) * relativeX;
            clickSeconds = Math.Min(clickSeconds, AnaValues.holterConfig.TotalSeconds-60);

            return clickSeconds;
        }
        
        /// <summary>
        /// 设置波形起始时间并重绘波形
        /// </summary>
        public void SetWaveformStartTime(float timeOffsetSeconds)
        {
            // 触发事件通知pageScanWave设置波形起始时间
            OnHRTrendStartTimeChanged?.Invoke(timeOffsetSeconds);
        }
        
        public void StartTimeChanged(float timeOffsetSeconds)
        {
            clickSeconds = timeOffsetSeconds;

            if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
            {
                startTime = DateTime.Today;
            }
            DateTime currentTime = startTime.AddSeconds(timeOffsetSeconds);
            // 格式化时间字符串
            string timeStr = currentTime.ToString("HH:mm");
            // 获取该分钟的平均心率
            int avgHR = GetAverageHRForMinute(Convert.ToInt32(timeOffsetSeconds / 60));
            // 记录点击时间文本和平均心率文本
            clickTimeText = timeStr;
            clickAvgHRText = $"{avgHR} bpm";
            // 强制重绘以显示纵向直线
            ForceWaveformRedraw();
        }

        public void OnPageScanStartTimeChanged(int sample)
        {
            StartTimeChanged(sample/AnaValues.holterConfig.SampleRate);
        }

        /// <summary>
        /// 波形起始时间改变事件处理
        /// </summary>
        public void OnEcgCommonWaveStartTimeChanged(float rowSeconds, float timeOffsetSeconds, int sample)
        {
            StartTimeChanged(timeOffsetSeconds);
        }

        #endregion
    }
}
