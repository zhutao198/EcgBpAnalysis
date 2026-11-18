using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Zoncare.Common;
using AnalysisApp.Commons;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SkiaSharp.Views.Desktop;
using SkiaSharp;

namespace AnalysisApp.CommonControls
{
    public partial class SegmentChartControl : BaseDrawingControl
    {
        private string segmentFilePath = "";
        private Dictionary<DateTime, int> segmentsPerHour = new Dictionary<DateTime, int>();
        private DateTime startDateTime;
        private int totalSeconds;
        private int totalSegments = 0;
        private int selectedBarIndex = 0; // 当前选中的柱状图索引，-1表示未选中
        private int rightClickedBarIndex = -1; // 右键点击的柱状图索引
        private DateTime rightClickedHour; // 右键点击的时间

        private float margin = 0;
        private float canvasWidth = 0;
        private float canvasHeight = 0;
        private float chartWidth = 0;
        private float chartHeight = 0;

        private float barWidth = 0;
        private float barSpacing = 0;

        private int yAxisMax = 0;

        List<DateTime> allHours = new List<DateTime>();

        List<JObject> SegmentJsonArray = new List<JObject>();   // 片段图信息

        // 右键菜单
        private ContextMenuStrip contextMenuStrip;

        /// <summary>
        /// 筛选片段图事件
        /// </summary>
        public event Action<List<JObject>> OnSegmentTimeChanged;

        /// <summary>
        /// 删除片段图事件
        /// </summary>
        public event Action OnChartDeleteSegment;

        public SegmentChartControl()
        {
            InitializeComponent();
        }

        private void SegmentChartControl_Load(object sender, EventArgs e)
        {
            initControl();

            if (AnaValues.CurrentCase == null) return;
            if (!DesignMode)
            {
                initData();
            }
        }

        private void initData()
        {
            totalSegments = 0;
            SegmentJsonArray.Clear();
            segmentsPerHour.Clear();

            startDateTime = AnaValues.holterConfig.StartDateTime;
            totalSeconds = AnaValues.holterConfig.TotalSeconds;
            setAllHours();
            segmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            if (File.Exists(segmentFilePath))
            {
                // 读取现有JSON数组
                string jsonContent = File.ReadAllText(segmentFilePath);
                SegmentJsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

                if (SegmentJsonArray.Count == 0)
                {
                    _graphicsControl.ContextMenuStrip = null;
                }
                else
                {
                    _graphicsControl.ContextMenuStrip = contextMenuStrip;
                }

                // 统计片段图信息
                totalSegments = SegmentJsonArray.Count;

                for (int i = 0; i < SegmentJsonArray.Count; i++)
                {
                    JObject segment = SegmentJsonArray[i];

                    // 获取片段图的开始时间（基于样本数）
                    int startSample = segment["startSample"]?.Value<int>() ?? 0;
                    int sampleRate = AnaValues.holterConfig.SampleRate;

                    // 计算片段图的具体时间
                    double secondsFromStart = (double)startSample / sampleRate;
                    DateTime segmentTime = startDateTime.AddSeconds(secondsFromStart);

                    // 计算该片段图所属的自然小时（小时起始时间）
                    DateTime hourStart = new DateTime(segmentTime.Year, segmentTime.Month, segmentTime.Day, segmentTime.Hour, 0, 0);

                    // 统计每个自然小时的片段图数量
                    if (segmentsPerHour.ContainsKey(hourStart))
                    {
                        segmentsPerHour[hourStart]++;
                    }
                    else
                    {
                        segmentsPerHour[hourStart] = 1;
                    }
                }

                // 按时间顺序输出统计结果
                List<DateTime> sortedHours = segmentsPerHour.Keys.OrderBy(h => h).ToList();
                int hourCount = 1;
                foreach (DateTime hourStart in sortedHours)
                {
                    DateTime hourEnd = hourStart.AddHours(1);
                    Console.WriteLine($"  第{hourCount}小时 ({hourStart:yyyy-MM-dd HH:mm}) - {hourEnd:HH:mm}): {segmentsPerHour[hourStart]} 个片段图");
                    hourCount++;
                }

                // 计算平均每小时片段图数量
                if (sortedHours.Count > 0)
                {
                    double avgPerHour = (double)totalSegments / sortedHours.Count;
                    Console.WriteLine($"平均每小时片段图数量: {avgPerHour:F2}");
                }

                // 显示监测期间的所有自然小时（包括没有片段图的小时）
                if (sortedHours.Count > 0)
                {
                    Console.WriteLine($"监测期间所有自然小时:");
                    DateTime firstHour = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                    DateTime lastHour = new DateTime(startDateTime.AddSeconds(totalSeconds).Year,
                                                    startDateTime.AddSeconds(totalSeconds).Month,
                                                    startDateTime.AddSeconds(totalSeconds).Day,
                                                    startDateTime.AddSeconds(totalSeconds).Hour, 0, 0);

                    int totalNaturalHours = (int)Math.Ceiling((lastHour - firstHour).TotalHours);

                    for (int i = 0; i <= totalNaturalHours; i++)
                    {
                        DateTime currentHour = firstHour.AddHours(i);
                        int segmentCount = segmentsPerHour.ContainsKey(currentHour) ? segmentsPerHour[currentHour] : 0;
                        Console.WriteLine($"  {currentHour:yyyy-MM-dd HH:mm} - {currentHour.AddHours(1):HH:mm}: {segmentCount} 个片段图");
                    }
                }

                //Console.WriteLine($"==================================");
            }
            else
            {
                _graphicsControl.ContextMenuStrip = null;
            }
        }
        private void initControl()
        {
            if (!DesignMode)
            {
                _graphicsControl.MouseDown += OnCanvasMouseDown;
                panel1.Controls.Add(_graphicsControl);
                
                // 初始化右键菜单
                InitializeContextMenu();
            }
        }

        private void InitializeContextMenu()
        {
            contextMenuStrip = new ContextMenuStrip();
            
            // 添加删除菜单项
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem(CommonValues.Langs["删除片段图"]);
            deleteMenuItem.Click += DeleteSegmentsMenuItem_Click;
            contextMenuStrip.Items.Add(deleteMenuItem);
            
            //_graphicsControl.ContextMenuStrip = contextMenuStrip;
        }

        private void DeleteSegmentsMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedBarIndex == -1)
            {
                return;
            }

            // 确认删除操作
            DialogResult result = MessageBox.Show(
                CommonValues.Langs["确定删除片段图吗？此操作不可恢复。"],
                CommonValues.Langs["删除"],
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            // 执行删除操作
            DeleteSegmentsByTimePeriod();

            OnChartDeleteSegment?.Invoke();

        }

        private void DeleteSegmentsByTimePeriod()
        {
            if (!File.Exists(segmentFilePath))
            {
                return;
            }

            try
            {
                // 读取原始片段图数据
                string jsonContent = File.ReadAllText(segmentFilePath);
                List<JObject> allSegments = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

                // 确定要删除的时间范围
                DateTime startTime, endTime;

                if (rightClickedBarIndex == 0)
                {
                    // 删除所有片段图
                    startTime = startDateTime;
                    endTime = startDateTime.AddSeconds(totalSeconds);
                }
                else
                {
                    // 删除指定小时的片段图
                    startTime = rightClickedHour;
                    endTime = rightClickedHour.AddHours(1);
                }

                // 筛选要保留的片段图（不在删除时间范围内的片段图）
                List<JObject> segmentsToKeep = new List<JObject>();
                int sampleRate = AnaValues.holterConfig.SampleRate;
                int deletedCount = 0;

                foreach (JObject segment in allSegments)
                {
                    // 获取片段图的开始时间（基于样本数）
                    int startSample = segment["startSample"]?.Value<int>() ?? 0;

                    // 计算片段图的具体时间
                    double secondsFromStart = (double)startSample / sampleRate;
                    DateTime segmentTime = startDateTime.AddSeconds(secondsFromStart);

                    // 检查片段图是否在删除时间范围内
                    if (segmentTime >= startTime && segmentTime < endTime)
                    {
                        deletedCount++;
                    }
                    else
                    {
                        segmentsToKeep.Add(segment);
                    }
                }

                // 保存更新后的片段图数据
                string updatedJson = JsonConvert.SerializeObject(segmentsToKeep, Formatting.Indented);
                File.WriteAllText(segmentFilePath, updatedJson);

                // 更新统计信息
                RefreshDataAndDisplay();


            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除片段图错误：{ex.Message}");
            }
        }

        public void RefreshDataAndDisplay()
        {
            // 重新初始化数据
            initData();
            
            // 重置选中状态
            selectedBarIndex = 0;
            rightClickedBarIndex = -1;
            
            // 重绘控件
            _graphicsControl.Invalidate();


            OnSegmentTimeChanged?.Invoke(SegmentJsonArray);
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                HandleBarChartClick(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                HandleBarChartRightClick(e.X, e.Y);
            }
        }

        private void HandleBarChartRightClick(int mouseX, int mouseY)
        {
            if (segmentsPerHour.Count == 0)
            {
                return;
            }

            if (allHours.Count == 0)
            {
                return;
            }

            // 获取控件尺寸
            System.Drawing.Size controlSize = _graphicsControl.Size;

            // 检查点击位置是否在某个柱状图内
            for (int i = 0; i < allHours.Count; i++)
            {
                float x1 = margin + (i * (barWidth + barSpacing));
                float x2 = margin + ((i + 1) * (barWidth + barSpacing));

                // 检查X坐标是否在柱状图范围内
                if (mouseX >= x1 && mouseX <= x2)
                {
                    // 检查Y坐标是否在柱状图上方且在坐标系内
                    float chartTop = margin;
                    float chartBottom = controlSize.Height - margin;

                    DateTime hour = allHours[i];
                    int segmentCount;

                    // 第一个位置显示总数统计
                    if (i == 0)
                    {
                        segmentCount = totalSegments;
                    }
                    else
                    {
                        // 24小时统计
                        segmentCount = segmentsPerHour.ContainsKey(hour) ? segmentsPerHour[hour] : 0;
                    }

                    if(segmentCount == 0)
                    {
                        return;
                    }

                    // 放宽Y坐标检测：只要在坐标系Y轴范围内都响应点击
                    if (mouseY >= chartTop && mouseY <= chartBottom)
                    {
                        // 保存右键点击的柱状图索引和时间
                        rightClickedBarIndex = i;
                        rightClickedHour = hour;
                        
                        // 显示右键菜单
                        contextMenuStrip.Show(_graphicsControl, new System.Drawing.Point(mouseX, mouseY));
                        return;
                    }
                }
            }
        }

        private void HandleBarChartClick(int mouseX, int mouseY)
        {
            if (segmentsPerHour.Count == 0)
            {
                return;
            }

            if (allHours.Count == 0)
            {
                return;
            }

            // 获取控件尺寸
            System.Drawing.Size controlSize = _graphicsControl.Size;

            // 检查点击位置是否在某个柱状图内
            for (int i = 0; i < allHours.Count; i++)
            {
                if(i == 0)
                {
                    int k = 0;
                }

                float x1 = margin + (i * (barWidth + barSpacing));
                float x2 = margin + ((i+1) * (barWidth + barSpacing));
                // 检查X坐标是否在柱状图范围内
                if (mouseX >= x1 && mouseX <= x2)
                {
                    // 检查Y坐标是否在柱状图上方且在坐标系内
                    // 放宽检测条件：只要在坐标系Y轴范围内都响应点击
                    float chartTop = margin;
                    float chartBottom = controlSize.Height - margin;

                    DateTime hour = allHours[i];
                    int segmentCount;

                    // 第一个位置显示总数统计
                    if (i == 0)
                    {
                        segmentCount = totalSegments;
                    }
                    else
                    {
                        // 24小时统计
                        segmentCount = segmentsPerHour.ContainsKey(hour) ? segmentsPerHour[hour] : 0;
                    }

                    if(segmentCount == 0)
                    {
                        continue;
                    }

                    // 放宽Y坐标检测：只要在坐标系Y轴范围内都响应点击
                    if (mouseY >= chartTop && mouseY <= chartBottom)
                    {
                        // 保存选中的柱状图索引
                        selectedBarIndex = i;
                        
                        // 触发重绘以更新颜色
                        _graphicsControl.Invalidate();
                        
                        // 触发柱状图点击事件
                        OnBarChartClicked(i, hour, segmentCount);
                        return;
                    }
                }
            }
        }

        private void OnBarChartClicked(int barIndex, DateTime hour, int segmentCount)
        {
            if (barIndex == 0)
            {
                // 总数统计点击 - 显示所有片段图
                FilterSegmentsByTimePeriod(null, null); // 显示所有片段图
            }
            else
            {
                // 小时统计点击 - 筛选该时间段的片段图
                DateTime endHour = hour.AddHours(1);
                FilterSegmentsByTimePeriod(hour, endHour);
            }

            OnSegmentTimeChanged?.Invoke(SegmentJsonArray);
        }

        private void FilterSegmentsByTimePeriod(DateTime? startTime, DateTime? endTime)
        {
            // 读取原始片段图数据
            if (!File.Exists(segmentFilePath))
            {
                SegmentJsonArray = new List<JObject>();
                return;
            }

            string jsonContent = File.ReadAllText(segmentFilePath);
            List<JObject> allSegments = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

            // 如果没有指定时间范围，显示所有片段图
            if (startTime == null || endTime == null)
            {
                SegmentJsonArray = allSegments;
                Console.WriteLine($"显示所有片段图，共 {SegmentJsonArray.Count} 个");
                return;
            }

            // 筛选指定时间段的片段图
            List<JObject> filteredSegments = new List<JObject>();
            int sampleRate = AnaValues.holterConfig.SampleRate;

            foreach (JObject segment in allSegments)
            {
                // 获取片段图的开始时间（基于样本数）
                int startSample = segment["startSample"]?.Value<int>() ?? 0;

                // 计算片段图的具体时间
                double secondsFromStart = (double)startSample / sampleRate;
                DateTime segmentTime = startDateTime.AddSeconds(secondsFromStart);

                // 检查片段图是否在指定时间范围内
                if (segmentTime >= startTime.Value && segmentTime < endTime.Value)
                {
                    filteredSegments.Add(segment);
                }
            }

            // 更新SegmentJsonArray为筛选后的片段图
            SegmentJsonArray = filteredSegments;
            Console.WriteLine($"筛选时间段 {startTime.Value:HH:mm} - {endTime.Value:HH:mm} 的片段图，共 {SegmentJsonArray.Count} 个");
        }

        private void setAllHours()
        {
            // 固定为24小时显示
            DateTime firstHour = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
            List<DateTime> allHours = new List<DateTime>
            {
                // 添加总数统计作为第一个柱状图
                firstHour.AddHours(-1) // 特殊位置用于总数统计
            };

            // 添加24小时的时间点
            for (int i = 0; i <= 25; i++)
            {
                allHours.Add(firstHour.AddHours(i));
            }

            this.allHours = allHours;
        }

        private void drawAxis(SKCanvas canvas)
        {
            using (SKPaint axisPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            })
            using (SKFont titleFont = new SKFont
            {
                Size = 11f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
            })
            // 绘制Y轴刻度
            using (SKFont tickFont = new SKFont
            {
                Size = 9f,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                string title = CommonValues.Langs["事件分布图"];
                canvas.DrawText(title, chartWidth / 2, margin - 5, SKTextAlign.Center, titleFont, axisPaint);

                // 计算Y轴最大值（取总数和24小时最大值的较大值，并向上取整到合适的刻度）
                int max24HourSegments = segmentsPerHour.Values.Count > 0 ? segmentsPerHour.Values.Max() : 1;
                int maxValue = Math.Max(totalSegments, max24HourSegments);


                yAxisMax = Convert.ToInt32(Math.Ceiling(maxValue / 5.0) * 5); 

                // X轴
                canvas.DrawLine(margin, canvasHeight - margin, canvasWidth - margin, canvasHeight - margin, axisPaint);
                // Y轴
                canvas.DrawLine(margin, canvasHeight - margin, margin, margin, axisPaint);

                int tickCount = 5;
                for (int i = 1; i <= tickCount; i++)
                {
                    float yValue = yAxisMax * i / (float)tickCount;
                    float yPos = canvasHeight - margin - (chartHeight * i / tickCount);

                    // 绘制刻度线
                    canvas.DrawLine(margin + 5, yPos, margin, yPos, axisPaint);

                    // 绘制刻度值（整数显示）
                    string tickText = yValue % 1 == 0 ? $"{yValue:F0}" : $"{yValue:F1}";
                    canvas.DrawText(tickText, margin - 5, yPos + 3, SKTextAlign.Right, tickFont, axisPaint);
                }

                for (int i = 0; i < allHours.Count; i++)
                {
                    DateTime hour = allHours[i];

                    int segmentCount;
                    string labelText;
                    float x = 0;
                    float y = canvasHeight - margin;
                    // 第一个位置显示总数统计
                    if (i == 0)
                    {
                        segmentCount = totalSegments;
                        labelText = CommonValues.Langs["总数"];
                        x = margin + (barWidth + barSpacing) / 2;
                        float labelY = canvasHeight - margin + 15;
                        canvas.DrawText(labelText, x, labelY, SKTextAlign.Center, tickFont, axisPaint);
                    }
                    else
                    {
                        // 24小时统计
                        segmentCount = segmentsPerHour.ContainsKey(hour) ? segmentsPerHour[hour] : 0;
                        labelText = hour.ToString("HH:ss");
                        x = margin + (i * (barWidth + barSpacing));
                        // 绘制刻度线
                        canvas.DrawLine(x, y, x, y - 5, axisPaint);

                        if (i % 2 == 1)
                        {
                            float labelY = canvasHeight - margin + 15;
                            canvas.DrawText(labelText, x, labelY, SKTextAlign.Center, tickFont, axisPaint);
                        }
                    }


                }
                //canvas.DrawText("(个)", margin - 5, margin - 5, SKTextAlign.Left, tickFont, axisPaint);

                //canvas.DrawText("(时)", chartWidth + margin + 5, chartHeight + margin + 5, SKTextAlign.Left, tickFont, axisPaint);
            }
        }
        private void drawHistogram(SKCanvas canvas)
        {
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            })
            using (SKFont valueFont = new SKFont
            {
                Size = 10,
                Typeface = SKTypeface.FromFamilyName("微软雅黑")
            })
            {
                // 绘制柱状图
                for (int i = 0; i < allHours.Count; i++)
                {
                    DateTime hour = allHours[i];
                    int segmentCount;

                    // 第一个位置显示总数统计
                    if (i == 0)
                    {
                        segmentCount = totalSegments;
                    }
                    else
                    {
                        // 24小时统计
                        segmentCount = segmentsPerHour.ContainsKey(hour) ? segmentsPerHour[hour] : 0;
                    }

                    // 计算柱状图位置（按比例缩放）
                    float x = margin + (barSpacing / 2) + (i * (barWidth + barSpacing));
                    float barHeight = segmentCount / (float)yAxisMax * chartHeight;
                    float y = canvasHeight - margin - barHeight;

                    // 根据是否被选中设置不同的填充颜色
                    SKColor barColor;

                    if (i == selectedBarIndex)
                    {
                        // 被选中的柱状图：使用高亮颜色
                        barColor = segmentCount > 0 ? SKColor.Parse("#e67e22") : SKColor.Parse("#f39c12"); // 小时统计：橙色
                    }
                    else
                    {
                        // 未选中的柱状图：使用正常颜色
                        barColor = segmentCount > 0 ? SKColor.Parse("#3498db") : SKColor.Parse("#bdc3c7"); // 小时统计：蓝色/灰色
                    }

                    using (SKPaint barPaint = new SKPaint
                    {
                        Color = barColor,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(x, y, barWidth, barHeight, barPaint);
                        if (segmentCount > 0)
                        {
                            string valueText = segmentCount.ToString();
                            canvas.DrawText(valueText, x+barWidth/2, y - 2, SKTextAlign.Center, valueFont, textPaint);
                        }
                    }
                }
            }
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            // 清除画布为白色背景
            canvas.Clear(SKColors.White);

            if (allHours.Count == 0)
            {
                return;
            }

            // 计算图表区域
            margin = 30;
            canvasWidth = imageWidth;
            canvasHeight = imageHeight;
            chartWidth = imageWidth - (2 * margin);
            chartHeight = imageHeight - (2 * margin);

            // 计算柱状图参数（25个位置：1个总数 + 24小时）
            barWidth = chartWidth / (allHours.Count-1) * 0.4f;
            barSpacing = chartWidth / (allHours.Count-1) * 0.6f;

            drawAxis(canvas);
            drawHistogram(canvas);
        }

        public void OnDeleteSegment()
        {
            RefreshDataAndDisplay();
        }

    }
}
