using System;
using System.Collections.Generic;
using System.Drawing;
using SkiaSharp;

namespace TestSkiaSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("测试SkiaSharp基本功能...");
            
            // 测试SkiaSharp的基本功能
            try
            {
                // 创建一个简单的SkiaSharp位图
                using (var skBitmap = new SKBitmap(100, 100, SKColorType.Bgra8888, SKAlphaType.Premul))
                using (var skCanvas = new SKCanvas(skBitmap))
                {
                    // 清除画布
                    skCanvas.Clear(SKColors.White);
                    
                    // 绘制一个简单的圆
                    using (var paint = new SKPaint())
                    {
                        paint.Color = SKColors.Red;
                        paint.Style = SKPaintStyle.Fill;
                        skCanvas.DrawCircle(50, 50, 25, paint);
                    }
                    
                    Console.WriteLine("SkiaSharp基本功能测试成功！");
                }
                
                // 测试点生成功能
                TestPointGeneration();
                
                Console.WriteLine("所有测试通过！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
            
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
        
        static void TestPointGeneration()
        {
            Console.WriteLine("测试点生成功能...");
            
            Random random = new Random();
            List<SKPoint> points = new List<SKPoint>();
            List<SKColor> colors = new List<SKColor>();
            
            const int TEST_COUNT = 1000; // 测试1000个点而不是10万个，以加快测试速度
            
            for (int i = 0; i < TEST_COUNT; i++)
            {
                // 生成随机位置
                float x = (float)(random.NextDouble() * 800);
                float y = (float)(random.NextDouble() * 600);
                points.Add(new SKPoint(x, y));
                
                // 生成随机颜色
                byte r = (byte)random.Next(256);
                byte g = (byte)random.Next(256);
                byte b = (byte)random.Next(256);
                colors.Add(new SKColor(r, g, b));
            }
            
            Console.WriteLine($"成功生成 {points.Count} 个点和颜色");
            Console.WriteLine("点生成功能测试成功！");
        }
    }
}