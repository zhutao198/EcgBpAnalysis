using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Component.Controls;

namespace EcgAnalyseStart
{
    public partial class Form2 : Form
    {
        SKControl skiaControl;
        SafeSKGLControl overlayControl;

        private SKPoint[] points;
        private SKPaint pointPaint;
        private SKColor[] testColors;
        private Dictionary<int, List<SKPoint>> groups;

        private SKSurface offscreenSurface;
        private SKImage offscreenImage;
        private readonly object _surfaceLock = new object(); // 用于同步surface访问的锁对象

        // 圈选功能相关变量
        private bool isSelecting = false;
        private List<SKPoint> selectionPath = new List<SKPoint>();
        private ConcurrentBag<SKPoint> selectedPoints = new ConcurrentBag<SKPoint>();
        private ConcurrentBag<SKColor> selectedColors = new ConcurrentBag<SKColor>();
        private SKPaint selectionPaint;
        private SKPaint overlayPaint;

        // 用于存储坐标和计数的结构体，替代Tuple以提高性能
        private struct CoordinateData
        {
            public SKPoint Point;
            public int Count;
        }

        // ThreadLocal数据类，用于兼容Visual Studio 2017的语法
        private class ThreadLocalData
        {
            public int[,] Counts { get; set; }
            public SKPoint[,] Points { get; set; }
        }

        public Form2()
        {
            InitializeComponent();

            skiaControl = new SKControl();
            this.skiaControl.BackColor = Color.White;
            //this.skiaControl.VSync = true; //垂直同步避免撕裂
            this.panel1.Controls.Add(skiaControl);
            skiaControl.Dock = DockStyle.Fill;

            overlayControl = new SafeSKGLControl();
            this.overlayControl.BackColor = Color.White;
            this.overlayControl.VSync = true;
            this.panel2.Controls.Add(overlayControl);
            overlayControl.Dock = DockStyle.Fill;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            pointPaint = new SKPaint()
            {
                //IsAntialias = true,
                //Style = SKPaintStyle.Fill, // 设置为填充模式
                StrokeWidth = 2.0f
            };

            // 定义10组测试颜色
            testColors = new SKColor[]
            {
                SKColors.Red,
                SKColors.Green,
                SKColors.Blue,
                SKColors.Yellow,
                SKColors.Magenta,
                SKColors.Cyan,
                SKColors.Orange,
                SKColors.Purple,
                SKColors.Brown,
                SKColors.Pink
            };
            groups = new Dictionary<int, List<SKPoint>>();

            // 初始化圈选画笔
            selectionPaint = new SKPaint()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.0f,
                Color = SKColors.Black,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeCap = SKStrokeCap.Round
            };

            // 初始化叠加图画笔
            overlayPaint = new SKPaint()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black
            };


            // 注册事件处理程序
            skiaControl.MouseDown += OnMouseDown;
            skiaControl.MouseMove += OnMouseMove;
            skiaControl.MouseUp += OnMouseUp;
            skiaControl.MouseLeave += OnMouseLeave;

            skiaControl.PaintSurface += OnPaintSurface0;
            overlayControl.PaintSurface += OnOverlayPaintSurface;

            // 注册窗体大小变化事件
            this.SizeChanged += OnFormSizeChanged;

            // 窗体加载时无需特殊初始化，散点将在OnPaintSurface中绘制
            if (points != null)
            {
                RedrawScatterPoints();
            }
        }

        // 窗体大小变化事件处理
        private void OnFormSizeChanged(object sender, EventArgs e)
        {
            // 窗口大小改变时重新创建离屏surface并重绘
            CreateOffscreenSurface();
            this.skiaControl.Invalidate();
        }

        // 创建离屏surface的专用方法，包含完整的错误处理
        private void CreateOffscreenSurface()
        {
            try
            {
                // 先释放现有的surface和image
                if (offscreenSurface != null)
                {
                    offscreenSurface.Dispose();
                    offscreenSurface = null;
                }
                if (offscreenImage != null)
                {
                    offscreenImage.Dispose();
                    offscreenImage = null;
                }

                // 检查控件是否有效
                if (skiaControl == null || skiaControl.IsDisposed || skiaControl.Width <= 0 || skiaControl.Height <= 0)
                {
                    return;
                }

                // 创建新的surface
                var info = new SKImageInfo(skiaControl.Width, skiaControl.Height);
                var surface = SKSurface.Create(info);

                if (surface != null)
                {
                    // 绘制散点到离屏surface
                    if (points != null && groups != null)
                    {
                        // 使用局部画笔避免全局状态冲突
                        using (var localPointPaint = new SKPaint())
                        {
                            // 配置画笔参数
                            localPointPaint.IsAntialias = false;
                            localPointPaint.Style = SKPaintStyle.Stroke;
                            localPointPaint.StrokeWidth = 2.0f;
                            localPointPaint.FilterQuality = SKFilterQuality.None;

                            // 按颜色组批量绘制点
                            foreach (var group in groups)
                            {
                                if (group.Value.Count > 0)
                                {
                                    localPointPaint.Color = testColors[group.Key];
                                    surface.Canvas.DrawPoints(SKPointMode.Points, group.Value.ToArray(), localPointPaint);
                                }
                            }
                        }
                    }

                    // 保存surface引用和创建快照
                    offscreenSurface = surface;
                    offscreenImage = offscreenSurface.Snapshot();
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出，避免影响UI线程
                System.Diagnostics.Debug.WriteLine($"CreateOffscreenSurface错误: {ex.GetType().Name}: {ex.Message}");

                // 确保在异常情况下清理所有资源
                if (offscreenImage != null)
                {
                    offscreenImage.Dispose();
                    offscreenImage = null;
                }
                if (offscreenSurface != null)
                {
                    offscreenSurface.Dispose();
                    offscreenSurface = null;
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 释放所有画笔资源
            pointPaint?.Dispose();
            selectionPaint?.Dispose();
            overlayPaint?.Dispose();

            // 释放离屏surface资源
            if (offscreenSurface != null)
            {
                offscreenSurface.Dispose();
                offscreenSurface = null;
            }

            // 图层资源已移除，无需释放
        }

        private void btnLoadPoints_Click(object sender, EventArgs e)
        {
            //生成散点
            GeneratePoints();

            // 使用新的专用方法创建离屏surface
            CreateOffscreenSurface();

            // 触发重绘
            this.skiaControl.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GenerateHeartbeatWaveforms();
            ResetSelection();

            //重新绘制散点
            RedrawScatterPoints();

            MessageBox.Show("已生成所有散点相关心电波形");
        }

        // 重置圈选
        private void ResetSelection()
        {
            isSelecting = false;
            selectionPath.Clear();
            selectedPoints = new ConcurrentBag<SKPoint>();
            selectedColors = new ConcurrentBag<SKColor>();
            overlayControl.Invalidate();
        }

        // 鼠标离开事件，停止圈选
        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                selectionPath.Clear();
                skiaControl.Invalidate();
            }
        }

        private void GeneratePoints()
        {
            // 检查控件是否已经初始化
            if (skiaControl == null || skiaControl.IsDisposed)
                return;

            Random rand = new Random();
            points = new SKPoint[100000];
            groups.Clear();

            // 预创建所有颜色组
            for (int i = 0; i < 10; i++)
            {
                if (!groups.ContainsKey(i))
                    groups[i] = new List<SKPoint>();
            }

            for (int i = 0; i < 100000; i++)
            {
                points[i] = new SKPoint(
                    (float)(rand.NextDouble() * skiaControl.ClientSize.Width),
                    (float)(rand.NextDouble() * skiaControl.ClientSize.Height));

                // 将点均匀分配到10组颜色中
                int colorIndex = i % 10;
                groups[colorIndex].Add(points[i]);
            }

            //renderer.points = points;
            //renderer.colors = colors;
            //renderer.GenerateTriangles(radius: 3.0f); // 设置点的大小（三角形外接圆半径）
        }

        // 模拟心搏波形数据结构
        // 优化版本：使用float[]替代double[]以减少内存占用
        private class HeartbeatWaveform
        {
            public float[] WaveformData { get; set; } // 心搏波形数据（使用float减少内存占用）
            public SKColor Color { get; set; } // 波形颜色
        }

        // 为所有点生成模拟心搏波形
        // 优化：使用更高效的缓存键，SKPoint作为键可能导致性能问题
        // 使用自定义的键生成函数，将SKPoint转换为唯一的字符串键
        private Dictionary<string, HeartbeatWaveform> heartbeatWaveforms = new Dictionary<string, HeartbeatWaveform>();

        // 优化：波形数据对象池，避免频繁创建和销毁数组
        private List<SKPoint[]> pointArrayPool = new List<SKPoint[]>();
        private const int MAX_POOL_SIZE = 100; // 最大缓存池大小

        // 优化：预计算的颜色渐变映射表 (从绿色到深红色)
        private const int COLOR_GRADIENT_STEPS = 64;
        private static SKColor[] colorGradient;

        // 静态构造函数，只在类首次使用时初始化颜色渐变表
        static Form2()
        {
            colorGradient = new SKColor[COLOR_GRADIENT_STEPS];
            for (int i = 0; i < COLOR_GRADIENT_STEPS; i++)
            {
                // 计算颜色比例 (0到1，0为绿色，1为深红色)
                float ratio = (float)i / (COLOR_GRADIENT_STEPS - 1);

                // 绿色到红色的渐变，随着ratio增加，绿色减少，红色增加
                // 深红色包含较多的红色和较少的绿色
                byte r = (byte)(255 * ratio + 100 * (1 - ratio)); // 从100到255
                byte g = (byte)(255 * (1 - ratio)); // 从255到0
                byte b = 0; // 保持蓝色为0

                colorGradient[i] = new SKColor(r, g, b, 255);
            }
        }

        // 标记是否应用了波形优化
        private bool waveformOptimizationApplied = true;

        // 优化：为SKPoint生成唯一字符串键的辅助方法
        private string GetPointKey(SKPoint point)
        {
            // 使用字符串格式化生成唯一键，比使用SKPoint对象更高效
            return $"{point.X:F2}_{point.Y:F2}";
        }

        // 优化：从对象池获取SKPoint数组
        private SKPoint[] GetPointArrayFromPool(int size)
        {
            lock (pointArrayPool)
            {
                for (int i = 0; i < pointArrayPool.Count; i++)
                {
                    if (pointArrayPool[i].Length >= size)
                    {
                        SKPoint[] array = pointArrayPool[i];
                        pointArrayPool.RemoveAt(i);
                        return array;
                    }
                }
            }
            // 如果没有合适大小的数组，创建新的
            return new SKPoint[size];
        }

        // 优化：将SKPoint数组返回对象池
        private void ReturnPointArrayToPool(SKPoint[] array)
        {
            if (array == null)
                return;

            lock (pointArrayPool)
            {
                if (pointArrayPool.Count < MAX_POOL_SIZE)
                {
                    pointArrayPool.Add(array);
                }
            }
        }

        // 生成模拟心电图波形（包含P波、QRS波群、T波）
        // 高度优化版本：减少计算复杂度，提高生成速度
        private float[] GenerateHeartbeatWaveform(SKPoint point, int samples = SamplePoints)
        {
            float[] waveform = new float[samples];
            float timeStep = 1.0f / samples;

            // 优化：使用更简单的随机种子生成方式
            int seed = (int)(point.X * 100) ^ (int)(point.Y * 100);
            float regionFactor = point.X / skiaControl.ClientSize.Width; // 0-1范围

            // 优化：预计算所有时间参数，减少乘法运算
            float baseDuration = 0.8f + regionFactor * 0.8f;
            float pStart = 0.08f * baseDuration;
            float pEnd = 0.18f * baseDuration;
            float qrsStart = 0.20f * baseDuration;
            float qrsEnd = qrsStart + 0.10f * baseDuration;
            float tStart = qrsEnd + 0.10f * baseDuration;
            float tEnd = tStart + 0.25f * baseDuration;

            // 优化：减少随机数生成次数
            float randomFactor1 = (seed % 100) / 100.0f;
            float randomFactor2 = ((seed / 100) % 100) / 100.0f;

            // 心电图振幅参数（标准化）
            float rAmplitude = 1.0f + randomFactor1 * 0.8f + regionFactor * 0.5f;
            float pAmplitude = 0.15f + randomFactor1 * 0.15f + regionFactor * 0.1f;
            float tAmplitude = rAmplitude * (0.2f + randomFactor2 * 0.4f);
            float qAmplitude = 0.1f + randomFactor1 * 0.2f;
            float sAmplitude = 0.3f + randomFactor2 * 0.4f;

            // 预计算所有时间点的波形值 - 优化数学计算
            for (int i = 0; i < samples; i++)
            {
                float t = i * timeStep;
                float value = 0.0f;

                // P波 - 简化计算
                if (t >= pStart && t <= pEnd)
                {
                    float pProgress = (t - pStart) / (pEnd - pStart);
                    float gauss = 1.0f - Math.Abs(pProgress - 0.5f) * 2.0f; // 简化高斯函数
                    gauss = gauss * gauss;
                    value += pAmplitude * gauss;
                }

                // QRS波群 - 简化计算
                if (t >= qrsStart && t <= qrsEnd)
                {
                    float qrsProgress = (t - qrsStart) / (qrsEnd - qrsStart);

                    if (qrsProgress < 0.15f)
                    {
                        value -= qAmplitude * qrsProgress * 6.666f; // 简化正弦波
                    }
                    else if (qrsProgress < 0.4f)
                    {
                        float rProgress = (qrsProgress - 0.15f) * 4.0f;
                        value += rAmplitude * rProgress;
                        if (rProgress > 1.0f)
                            value += rAmplitude * (2.0f - rProgress);
                    }
                    else
                    {
                        float sProgress = (qrsProgress - 0.4f) * 1.666f;
                        value -= sAmplitude * sProgress;
                    }
                }

                // T波 - 简化计算
                if (t >= tStart && t <= tEnd)
                {
                    float tProgress = (t - tStart) / (tEnd - tStart);
                    float gauss = 1.0f - Math.Abs(tProgress - 0.4f) * 2.5f; // 简化高斯函数
                    if (gauss > 0)
                        value += tAmplitude * gauss * gauss;
                }

                waveform[i] = value;
            }

            return waveform;
        }

        // 移除不再使用的波形缓存，因为现在为每个点生成独特的波形

        // 样本点数常量
        private const int SamplePoints = 256;

        // 波形数据结构 - 使用结构体提高内存效率
        private struct WaveformData
        {
            public float[] WaveformPoints;
            public SKColor Color;
            public double NoiseLevel;
        }

        // 预创建随机数生成器以提高性能
        private Random _waveformRandom = new Random();

        // 为所有点生成心搏波形 - 高度优化版本
        private void GenerateHeartbeatWaveforms()
        {
            if (points == null) return;

            // 优化：预分配字典容量
            heartbeatWaveforms = new Dictionary<string, HeartbeatWaveform>(points.Length);

            // 优化：使用并行计算，减少锁的使用
            Parallel.ForEach(groups, group =>
            {
                int colorIndex = group.Key;
                SKColor color = testColors[colorIndex];

                // 每个线程创建自己的临时字典
                Dictionary<string, HeartbeatWaveform> threadDict = new Dictionary<string, HeartbeatWaveform>();

                foreach (var point in group.Value)
                {
                    // 为每个点生成波形
                    float[] waveformData = GenerateHeartbeatWaveform(point, SamplePoints);
                    string key = GetPointKey(point);

                    threadDict[key] = new HeartbeatWaveform
                    {
                        WaveformData = waveformData,
                        Color = color
                    };
                }

                // 一次锁操作合并所有线程结果
                lock (heartbeatWaveforms)
                {
                    foreach (var item in threadDict)
                    {
                        heartbeatWaveforms[item.Key] = item.Value;
                    }
                }
            });
        }

        // 重新绘制散点（窗口大小改变或数据源变化时调用）
        private void RedrawScatterPoints()
        {
            // 直接触发重绘，散点将在OnPaintSurface中绘制
            skiaControl.Invalidate();
        }

        // 仅重绘圈选路径区域（圈选操作时调用）
        private void RedrawSelectionPathOnly()
        {
            // 计算圈选路径的边界区域
            if (selectionPath.Count > 1)
            {
                float minX = selectionPath.Min(p => p.X);
                float maxX = selectionPath.Max(p => p.X);
                float minY = selectionPath.Min(p => p.Y);
                float maxY = selectionPath.Max(p => p.Y);

                // 添加一些边距确保路径完全可见
                var rect = new Rectangle((int)(minX - 10), (int)(minY - 10),
                                        (int)(maxX - minX + 20), (int)(maxY - minY + 20));

                // 仅重绘圈选路径区域
                skiaControl.Invalidate(rect);
            }
            else
            {
                // 如果没有路径，重绘整个控件
                skiaControl.Invalidate();
            }
        }

        // 主绘制事件
        private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            if (e.Surface == null || e.Surface.Canvas == null) return; // 防御性检查
            if (points == null) return;

            lock (_surfaceLock)
            {
                try
                {
                    SKCanvas canvas = e.Surface.Canvas;

                    // 检查是否是部分重绘（仅重绘圈选路径区域）
                    bool isPartialRedraw = e.Info.Width < skiaControl.Width || e.Info.Height < skiaControl.Height;

                    if (!isPartialRedraw)
                    {
                        canvas.Clear(SKColors.White);

                        // 检查离屏surface状态，必要时重新创建
                        if (offscreenSurface == null || offscreenImage == null)
                        {
                            CreateOffscreenSurface();
                        }
                        //使用高效的SKSurface.Draw方法，但添加额外的安全检查
                        //if (offscreenSurface != null)
                        //{
                        //    offscreenSurface.Draw(canvas, 0, 0, pointPaint);
                        //}

                        // 使用离屏image绘制，这是最安全的方式
                        if (offscreenImage != null)
                        {
                            try
                            {
                                canvas.DrawImage(offscreenImage, 0, 0);
                            }
                            catch (AccessViolationException)
                            {
                                // 重新创建surface并尝试再次绘制
                                CreateOffscreenSurface();
                                if (offscreenImage != null)
                                {
                                    canvas.DrawImage(offscreenImage, 0, 0);
                                }
                            }
                        }
                    }

                    if (selectionPath.Count > 1) //绘制圈选路径（动态内容）
                    {
                        // 绘制开放路径
                        var pathArray = selectionPath.ToArray();
                        if (pathArray.Length > 0)
                        {
                            canvas.DrawPoints(SKPointMode.Polygon, pathArray, selectionPaint);
                        }

                        // 如果正在圈选，绘制从最后一个点到当前鼠标位置的线
                        if (isSelecting && MousePosition != null)
                        {
                            try
                            {
                                var mousePoint = skiaControl.PointToClient(MousePosition);
                                var lastPoint = selectionPath[selectionPath.Count - 1];
                                canvas.DrawLine(lastPoint.X, lastPoint.Y, mousePoint.X, mousePoint.Y, selectionPaint);
                            }
                            catch (ArgumentException)
                            {
                                // 坐标值无效，忽略此绘制操作
                            }
                        }
                        // 如果圈选已完成，闭合路径
                        else if (!isSelecting && selectionPath.Count >= 3)
                        {
                            try
                            {
                                var firstPoint = selectionPath[0];
                                var lastPoint = selectionPath[selectionPath.Count - 1];
                                canvas.DrawLine(lastPoint.X, lastPoint.Y, firstPoint.X, firstPoint.Y, selectionPaint);
                            }
                            catch (ArgumentException)
                            {
                                // 坐标值无效，忽略此绘制操作
                            }
                        }
                    }
                    
                    // 移除不必要的Flush调用，SkiaSharp会自动管理绘制刷新
                    // 手动调用Flush可能导致GPU内存访问冲突，尤其是在硬件加速环境下
                }
                catch (AccessViolationException)
                {
                    // GPU内存访问冲突，记录但不抛出异常
                    Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
                    // 重新创建surface以恢复正常状态
                    CreateOffscreenSurface();
                }
                catch (ObjectDisposedException)
                {
                    // 对象已释放，记录但不抛出异常
                    Console.WriteLine("绘制对象已释放，已安全处理");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnPaintSurface错误: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        private void OnPaintSurface0(object sender, SKPaintSurfaceEventArgs e)
        {
            if (e.Surface == null || e.Surface.Canvas == null) return; // 防御性检查
            if (points == null) return;

            lock (_surfaceLock)
            {
                try
                {
                    SKCanvas canvas = e.Surface.Canvas;

                    // 检查是否是部分重绘（仅重绘圈选路径区域）
                    bool isPartialRedraw = e.Info.Width < skiaControl.Width || e.Info.Height < skiaControl.Height;

                    if (!isPartialRedraw)
                    {
                        canvas.Clear(SKColors.White);

                        // 检查离屏surface状态，必要时重新创建
                        if (offscreenSurface == null || offscreenImage == null)
                        {
                            CreateOffscreenSurface();
                        }
                        //使用高效的SKSurface.Draw方法，但添加额外的安全检查
                        //if (offscreenSurface != null)
                        //{
                        //    offscreenSurface.Draw(canvas, 0, 0, pointPaint);
                        //}

                        // 使用离屏image绘制，这是最安全的方式
                        if (offscreenImage != null)
                        {
                            try
                            {
                                canvas.DrawImage(offscreenImage, 0, 0);
                            }
                            catch (AccessViolationException)
                            {
                                // 重新创建surface并尝试再次绘制
                                CreateOffscreenSurface();
                                if (offscreenImage != null)
                                {
                                    canvas.DrawImage(offscreenImage, 0, 0);
                                }
                            }
                        }
                    }

                    if (selectionPath.Count > 1) //绘制圈选路径（动态内容）
                    {
                        // 绘制开放路径
                        var pathArray = selectionPath.ToArray();
                        if (pathArray.Length > 0)
                        {
                            canvas.DrawPoints(SKPointMode.Polygon, pathArray, selectionPaint);
                        }

                        // 如果正在圈选，绘制从最后一个点到当前鼠标位置的线
                        if (isSelecting && MousePosition != null)
                        {
                            try
                            {
                                var mousePoint = skiaControl.PointToClient(MousePosition);
                                var lastPoint = selectionPath[selectionPath.Count - 1];
                                canvas.DrawLine(lastPoint.X, lastPoint.Y, mousePoint.X, mousePoint.Y, selectionPaint);
                            }
                            catch (ArgumentException)
                            {
                                // 坐标值无效，忽略此绘制操作
                            }
                        }
                        // 如果圈选已完成，闭合路径
                        else if (!isSelecting && selectionPath.Count >= 3)
                        {
                            try
                            {
                                var firstPoint = selectionPath[0];
                                var lastPoint = selectionPath[selectionPath.Count - 1];
                                canvas.DrawLine(lastPoint.X, lastPoint.Y, firstPoint.X, firstPoint.Y, selectionPaint);
                            }
                            catch (ArgumentException)
                            {
                                // 坐标值无效，忽略此绘制操作
                            }
                        }
                    }

                    // 移除不必要的Flush调用，SkiaSharp会自动管理绘制刷新
                    // 手动调用Flush可能导致GPU内存访问冲突，尤其是在硬件加速环境下
                }
                catch (AccessViolationException)
                {
                    // GPU内存访问冲突，记录但不抛出异常
                    Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
                    // 重新创建surface以恢复正常状态
                    CreateOffscreenSurface();
                }
                catch (ObjectDisposedException)
                {
                    // 对象已释放，记录但不抛出异常
                    Console.WriteLine("绘制对象已释放，已安全处理");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnPaintSurface错误: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        // 鼠标按下事件，开始圈选
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && points != null)
            {
                isSelecting = true;
                selectionPath.Clear();
                selectionPath.Add(new SKPoint(e.X, e.Y));

                // 触发重绘（仅绘制圈选路径）
                RedrawSelectionPathOnly();
            }
        }

        // 鼠标移动事件，绘制圈选路径
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting && points != null)
            {
                // 限制添加点的频率，提高性能
                var newPoint = new SKPoint(e.X, e.Y);
                if (selectionPath.Count == 0 ||
                    SKPoint.Distance(selectionPath[selectionPath.Count - 1], newPoint) > 2.0f)
                {
                    selectionPath.Add(newPoint);

                    // 仅重绘圈选路径（不重绘散点）
                    RedrawSelectionPathOnly();
                }
            }
        }

        // 鼠标释放事件，完成圈选并计算选中的点
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (isSelecting && points != null)
            {
                isSelecting = false;

                // 确保路径至少有3个点才能形成封闭区域
                if (selectionPath.Count >= 3)
                {
                    // 添加最后一个点，确保路径闭合
                    selectionPath.Add(selectionPath[0]);

                    // 将屏幕坐标转换为System.Drawing.Point用于IsPointInPolygon函数
                    List<Point> polygonPoints = new List<Point>();
                    foreach (var point in selectionPath)
                    {
                        polygonPoints.Add(new Point((int)point.X, (int)point.Y));
                    }

                    // 计算选中的点
                    selectedPoints = new ConcurrentBag<SKPoint>();
                    selectedColors = new ConcurrentBag<SKColor>();

                    try
                    {
                        // 优化性能：使用并行计算
                        Parallel.ForEach(groups, group =>
                        {
                            int colorIndex = group.Key;
                            SKColor color = testColors[colorIndex];
                            //List<SKPoint> groupSelectedPoints = new List<SKPoint>();
                            //List<SKColor> groupSelectedColors = new List<SKColor>();

                            foreach (var point in group.Value)
                            {
                                if (IsPointInPolygon(point.X, point.Y, polygonPoints))
                                {
                                    //groupSelectedPoints.Add(point);
                                    //groupSelectedColors.Add(color);
                                    selectedPoints.Add(point);
                                    selectedColors.Add(color);
                                }
                            }

                            //// 线程安全地合并结果
                            //lock (selectedPoints)
                            //{
                            //    selectedPoints.AddRange(groupSelectedPoints);
                            //    selectedColors.AddRange(groupSelectedColors);
                            //}
                        });
                    }
                    catch (Exception ex)
                    {
                        // 处理并行计算中的异常
                        MessageBox.Show("圈选过程中发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // 更新叠加图
                    overlayControl.Invalidate();
                }

                // 优化：圈选完成后重绘整个控件（包括背景色变化）
                skiaControl.Invalidate();
            }
        }

        // 叠加图绘制事件处理
        private void OnOverlayPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            if (e.Surface == null) return; // 防御性检查

            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            if (selectedPoints.Count == 0)
            {
                SKFont font = new SKFont(SKTypeface.FromFamilyName("微软雅黑"), 16);
                // 没有选中点时显示提示信息
                overlayPaint.Color = SKColors.Gray;
                canvas.DrawText("请圈选散点以查看心搏波形叠加图", 50, 50, font, overlayPaint);
                return;
            }

            // 确保已生成心搏波形
            if (heartbeatWaveforms.Count == 0 && points != null)
            {
                GenerateHeartbeatWaveforms();
            }

            // 绘制波形叠加图（所有波形叠加在同一个坐标系中）
            int waveformWidth = 500;
            int waveformHeight = 300;
            int startX = 50;
            int startY = 100;

            // 配置波形绘制笔
            overlayPaint.Style = SKPaintStyle.Stroke;
            overlayPaint.StrokeWidth = 1;
            overlayPaint.IsAntialias = true;

            // 绘制坐标轴
            overlayPaint.Color = SKColors.LightGray;
            overlayPaint.StrokeWidth = 2;
            canvas.DrawLine(startX, startY, startX + waveformWidth, startY, overlayPaint); // X轴
            canvas.DrawLine(startX, startY - waveformHeight / 2, startX, startY + waveformHeight / 2, overlayPaint); // Y轴

            // 绘制坐标轴刻度
            overlayPaint.Color = SKColors.DarkGray;
            overlayPaint.StrokeWidth = 1;
            overlayPaint.Style = SKPaintStyle.Fill;

            // 创建小字体用于刻度
            SKFont smallFont = new SKFont(SKTypeface.Default, 10);

            // X轴刻度（时间）
            for (int i = 0; i <= 5; i++)
            {
                int x = startX + (waveformWidth / 5) * i;
                canvas.DrawLine(x, startY - 5, x, startY + 5, overlayPaint);
                canvas.DrawText($"{i * 0.2}s", x - 10, startY + 20, smallFont, overlayPaint);
            }

            // Y轴刻度（振幅）
            for (int i = -1; i <= 1; i++)
            {
                int y = startY + (waveformHeight / 4) * i;
                canvas.DrawLine(startX - 5, y, startX + 5, y, overlayPaint);
                canvas.DrawText($"{i * 0.5}mV", startX - 30, y + 5, smallFont, overlayPaint);
            }

            // 首先从heartbeatWaveforms中提取所有相关的波形数据，暂存以便复用
            // 优化：使用更高效的键查找
            List<HeartbeatWaveform> selectedWaveforms = new List<HeartbeatWaveform>();
            foreach (var point in selectedPoints)
            {
                string key = GetPointKey(point);
                if (heartbeatWaveforms.TryGetValue(key, out HeartbeatWaveform waveform))
                {
                    selectedWaveforms.Add(waveform);
                }
            }

            // 热力图叠加效果实现 - 使用数组索引替代字典优化性能
            const int coordinateScale = 4; // 坐标缩放因子，减少数据点数量

            int maxCount = 1; // 最大叠加次数

            // 步骤1：计算所有波形的坐标范围，确定数组大小
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (var waveform in selectedWaveforms)
            {
                int actualPoints = waveform.WaveformData.Length;
                float timeStep = actualPoints > 1 ? 1.0f / (actualPoints - 1) : 0;
                float scaleY = waveformHeight * 0.5f;

                for (int i = 0; i < actualPoints; i++)
                {
                    float t = i * timeStep;
                    float px = startX + t * waveformWidth;
                    float py = startY - waveform.WaveformData[i] * scaleY;

                    // 将浮点坐标转换为整数索引
                    int qx = (int)(px * coordinateScale);
                    int qy = (int)(py * coordinateScale);

                    // 更新坐标范围
                    if (qx < minX) minX = qx;
                    if (qx > maxX) maxX = qx;
                    if (qy < minY) minY = qy;
                    if (qy > maxY) maxY = qy;
                }
            }

            // 计算二维数组的尺寸
            int arrayWidth = maxX - minX + 1;
            int arrayHeight = maxY - minY + 1;

            // 步骤2：为每个线程创建独立的计数和坐标数组
            var threadLocalArrays = new ThreadLocal<ThreadLocalData>(() =>
            {
                int[,] counts = new int[arrayHeight, arrayWidth];
                SKPoint[,] points = new SKPoint[arrayHeight, arrayWidth];
                return new ThreadLocalData { Counts = counts, Points = points };
            }, trackAllValues: true);

            // 步骤3：并行处理所有波形数据
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount) };

            Parallel.ForEach(selectedWaveforms, parallelOptions, waveform =>
            {
                var threadData = threadLocalArrays.Value;
                int[,] localCounts = threadData.Counts;
                SKPoint[,] localPoints = threadData.Points;
                int actualPoints = waveform.WaveformData.Length;

                float timeStep = actualPoints > 1 ? 1.0f / (actualPoints - 1) : 0;
                float scaleY = waveformHeight * 0.5f;

                for (int i = 0; i < actualPoints; i++)
                {
                    float t = i * timeStep;
                    float px = startX + t * waveformWidth;
                    float py = startY - waveform.WaveformData[i] * scaleY;

                    // 坐标量化和索引转换
                    int qx = (int)(px * coordinateScale);
                    int qy = (int)(py * coordinateScale);
                    int arrayX = qx - minX;
                    int arrayY = qy - minY;

                    // 边界检查后直接数组访问
                    if (arrayX >= 0 && arrayX < arrayWidth && arrayY >= 0 && arrayY < arrayHeight)
                    {
                        localCounts[arrayY, arrayX]++; // 叠加计数
                        localPoints[arrayY, arrayX] = new SKPoint(px, py); // 存储坐标
                    }
                }
            });

            // 步骤4：合并所有线程的数据到主数组
            int[,] mergedCounts = new int[arrayHeight, arrayWidth];
            SKPoint[,] mergedPoints = new SKPoint[arrayHeight, arrayWidth];

            foreach (var threadData in threadLocalArrays.Values)
            {
                int[,] threadCounts = threadData.Counts;
                SKPoint[,] threadPoints = threadData.Points;

                // 遍历二维数组，合并线程数据
                for (int y = 0; y < arrayHeight; y++)
                {
                    for (int x = 0; x < arrayWidth; x++)
                    {
                        if (threadCounts[y, x] > 0)
                        {
                            mergedCounts[y, x] += threadCounts[y, x];
                            mergedPoints[y, x] = threadPoints[y, x];

                            // 更新最大叠加次数
                            if (mergedCounts[y, x] > maxCount)
                            {
                                maxCount = mergedCounts[y, x];
                            }
                        }
                    }
                }
            }

            // 步骤5：按叠加次数将坐标点分组到不同颜色
            var colorGroups = new List<List<SKPoint>>(COLOR_GRADIENT_STEPS);
            for (int i = 0; i < COLOR_GRADIENT_STEPS; i++)
            {
                colorGroups.Add(new List<SKPoint>(1000)); // 预分配容量
            }

            // 遍历合并后的数组，根据叠加次数分配颜色索引
            for (int y = 0; y < arrayHeight; y++)
            {
                for (int x = 0; x < arrayWidth; x++)
                {
                    if (mergedCounts[y, x] > 0)
                    {
                        // 计算颜色索引：叠加次数越大，颜色越深（红色）
                        int colorIndex = mergedCounts[y, x] * (COLOR_GRADIENT_STEPS - 1) / maxCount;
                        colorGroups[colorIndex].Add(mergedPoints[y, x]);
                    }
                }
            }

            // 清理线程局部数组资源
            threadLocalArrays.Dispose();

            // 步骤6：渲染优化 - 使用DrawPoints批量绘制，提高渲染效率
            overlayPaint.Style = SKPaintStyle.Stroke;
            overlayPaint.StrokeWidth = 1.5F;
            overlayPaint.IsAntialias = false;
            overlayPaint.FilterQuality = SKFilterQuality.None; // 关闭滤镜以提高性能

            // 渲染优化：按颜色分组批量绘制，减少绘制状态切换
            int totalPoints = 0;

            // 预先过滤掉空列表，减少不必要的迭代
            var validColorGroups = colorGroups.Select((list, index) => new { Key = index, Value = list })
                                              .Where(kvp => kvp.Value.Count > 0).ToList();

            // 渲染优化：按点数排序，先绘制大量点的颜色组，减少绘制上下文切换的相对开销
            validColorGroups = validColorGroups.OrderByDescending(kvp => kvp.Value.Count).ToList();

            foreach (var kvp in validColorGroups)
            {
                int colorIndex = kvp.Key;
                List<SKPoint> pointsList = kvp.Value;
                int pointCount = pointsList.Count;

                if (pointCount == 0) continue;

                // 设置当前颜色 - 这是必要的状态切换，但我们已通过分组最小化
                overlayPaint.Color = colorGradient[colorIndex];

                // 直接转换为数组进行绘制，避免复杂的对象池管理
                SKPoint[] pointArray = pointsList.ToArray();

                // 渲染优化：使用DrawPoints的正确重载形式
                canvas.DrawPoints(SKPointMode.Points, pointArray, overlayPaint);

                totalPoints += pointCount;

                // 简单的内存优化：将数组引用置空，帮助GC回收
                pointArray = null;
            }

            // 绘制统计信息
            overlayPaint.Color = SKColors.Black;
            overlayPaint.Style = SKPaintStyle.Fill;

            // 创建统计信息字体
            SKFont infoFont = new SKFont(SKTypeface.FromFamilyName("微软雅黑"), 14);

            int infoY = startY + waveformHeight + 50;
            canvas.DrawText($"选中心搏总数: {selectedPoints.Count}", startX, infoY, infoFont, overlayPaint);
            canvas.DrawText($"显示波形数: {selectedPoints.Count}", startX, infoY + 20, infoFont, overlayPaint); // 显示所有选中心搏波形
        }

        private bool IsPointInPolygon(double x, double y, List<Point> polygonPoints)
        {
            if (polygonPoints == null || polygonPoints.Count < 3)
            {
                return false;
            }

            bool inside = false;
            int n = polygonPoints.Count;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = polygonPoints[i].X, yi = polygonPoints[i].Y;
                double xj = polygonPoints[j].X, yj = polygonPoints[j].Y;

                // 避免除零异常
                if (Math.Abs(yj - yi) < double.Epsilon)
                {
                    continue;
                }

                // 判断点是否在边的垂直范围内
                bool intersect = ((yi > y) != (yj > y)) &&
                                (x < ((xj - xi) * (y - yi) / (yj - yi)) + xi);

                if (intersect)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

    }
}