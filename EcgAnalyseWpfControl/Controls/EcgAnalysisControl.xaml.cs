using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EcgAnalyseWpfControl.Controls
{
    /// <summary>
    /// EcgAnalysisControl.xaml 的交互逻辑
    /// </summary>
    public partial class EcgAnalysisControl : UserControl
    {
        public EcgAnalysisControl()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            // 初始化控件事件 - 直接使用XAML中定义的控件名称
            btnLoadData.Click += BtnLoadData_Click;
            btnAnalyze.Click += BtnAnalyze_Click;
            btnClear.Click += BtnClear_Click;

            // 设置初始状态
            txtAnalysisStatus.Foreground = Brushes.Green;
            txtHeartRate.Foreground = Brushes.Black;
        }

        private void BtnLoadData_Click(object sender, RoutedEventArgs e)
        {
            // 模拟加载数据
            txtPatientInfo.Text = "患者ID: PAT-12345, 姓名: 张三, 年龄: 45";
            txtAnalysisStatus.Text = "数据已加载";
            txtAnalysisStatus.Foreground = Brushes.Blue;
            
            // 绘制简单的模拟心电图波形
            DrawSimulatedEcgWave();
        }

        private void BtnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            // 模拟分析过程
            txtAnalysisStatus.Text = "分析中...";
            txtAnalysisStatus.Foreground = Brushes.Orange;
            
            // 模拟分析完成
            Task.Delay(1000).ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtAnalysisStatus.Text = "分析完成";
                    txtAnalysisStatus.Foreground = Brushes.Green;
                    txtHeartRate.Text = "75 BPM";
                    txtHeartRate.Foreground = Brushes.Green;
                });
            });
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            // 清除数据
            txtPatientInfo.Text = "未加载患者数据";
            txtAnalysisStatus.Text = "准备就绪";
            txtAnalysisStatus.Foreground = Brushes.Green;
            txtHeartRate.Text = "-- BPM";
            txtHeartRate.Foreground = Brushes.Black;
            
            // 清空画布
            ecgCanvas.Children.Clear();
            ecgCanvas.Children.Add(new TextBlock { Text = "心电图波形显示区域", Margin = new Thickness(10) });
        }

        private void DrawSimulatedEcgWave()
        {
            // 清空画布
            ecgCanvas.Children.Clear();
            
            // 创建笔刷
            Brush waveBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204));
            Pen wavePen = new Pen(waveBrush, 2);
            
            // 获取画布尺寸
            double canvasWidth = ecgCanvas.ActualWidth;
            double canvasHeight = ecgCanvas.ActualHeight;
            
            // 确保有尺寸
            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                canvasWidth = 700; // 默认宽度
                canvasHeight = 300; // 默认高度
            }
            
            // 创建波形路径
            PathFigure figure = new PathFigure();
            figure.StartPoint = new Point(0, canvasHeight / 2);
            
            // 生成模拟心电图波形点
            for (int i = 1; i < canvasWidth; i += 2)
            {
                // 生成更真实的心电图波形模式
                double y;
                
                if (i % 50 == 0) // R波
                {
                    y = canvasHeight / 4;
                }
                else if (i % 50 == 5) // S波
                {
                    y = canvasHeight * 3 / 4;
                }
                else if (i % 50 == 10 || i % 50 == 40) // T波和P波
                {
                    y = canvasHeight / 2 - 30;
                }
                else // 基线和Q波
                {
                    y = canvasHeight / 2 + 5 * Math.Sin(i / 10.0); // 微小波动
                }
                
                LineSegment segment = new LineSegment(new Point(i, y), true);
                figure.Segments.Add(segment);
            }
            
            // 创建路径几何
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            
            // 创建路径并添加到画布
            Path path = new Path();
            path.Data = geometry;
            path.Stroke = wavePen.Brush;
            path.StrokeThickness = wavePen.Thickness;
            
            ecgCanvas.Children.Add(path);
            
            // 添加网格线
            DrawGridLines();
        }

        private void DrawGridLines()
        {
            // 获取画布尺寸
            double canvasWidth = ecgCanvas.ActualWidth;
            double canvasHeight = ecgCanvas.ActualHeight;
            
            // 确保有尺寸
            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                canvasWidth = 700; // 默认宽度
                canvasHeight = 300; // 默认高度
            }
            
            // 创建灰色网格线
            Pen gridPen = new Pen(new SolidColorBrush(Color.FromRgb(220, 220, 220)), 0.5);
            
            // 绘制水平网格线
            for (int i = 0; i <= canvasHeight; i += 20)
            {
                Line line = new Line();
                line.X1 = 0;
                line.Y1 = i;
                line.X2 = canvasWidth;
                line.Y2 = i;
                line.Stroke = gridPen.Brush;
                line.StrokeThickness = gridPen.Thickness;
                ecgCanvas.Children.Insert(0, line); // 插入到最底层
            }
            
            // 绘制垂直网格线
            for (int i = 0; i <= canvasWidth; i += 20)
            {
                Line line = new Line();
                line.X1 = i;
                line.Y1 = 0;
                line.X2 = i;
                line.Y2 = canvasHeight;
                line.Stroke = gridPen.Brush;
                line.StrokeThickness = gridPen.Thickness;
                ecgCanvas.Children.Insert(0, line); // 插入到最底层
            }
        }

        // 公共方法：设置患者信息
        public void SetPatientInfo(string patientInfo)
        {
            Dispatcher.Invoke(() =>
            {
                txtPatientInfo.Text = patientInfo;
            });
        }

        // 公共方法：开始分析
        public void StartAnalysis()
        {
            Dispatcher.Invoke(() =>
            {
                // 直接调用按钮点击处理程序
                BtnAnalyze_Click(null, null);
            });
        }
    }
}