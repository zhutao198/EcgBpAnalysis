using System;
using SkiaSharp;

namespace TestSKSurfaceDraw
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("测试 SKSurface.Draw 方法签名");
            
            // 创建一个离屏 surface
            var imageInfo = new SKImageInfo(100, 100);
            using (var surface = SKSurface.Create(imageInfo))
            {
                // 获取 canvas
                var canvas = surface.Canvas;
                
                // 测试 Draw 方法
                try
                {
                    // 尝试使用 SKCanvas 作为参数
                    surface.Draw(canvas, 0, 0, null);
                    Console.WriteLine("✓ SKSurface.Draw(SKCanvas, float, float, SKPaint) 方法存在");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ SKSurface.Draw(SKCanvas) 方法不存在: {ex.Message}");
                }
                
                // 测试正确的用法
                try
                {
                    using (var snapshot = surface.Snapshot())
                    {
                        surface.Draw(snapshot, 0, 0, null);
                        Console.WriteLine("✓ SKSurface.Draw(SKImage, float, float, SKPaint) 方法存在");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ SKSurface.Draw(SKImage) 方法错误: {ex.Message}");
                }
            }
            
            Console.WriteLine("测试完成");
        }
    }
}