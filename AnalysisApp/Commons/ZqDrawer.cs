using System;
using SkiaSharp;

namespace AnalysisApp.Commons
{
    public static class ZqDrawer
    {
        /// <summary>
        /// 绘制一个指向 target 点的数值指示框
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="target">箭头指向的目标点</param>
        /// <param name="value">显示的数字</param>
        /// <param name="offset">框相对目标点的偏移（决定框在哪个方向）</param>
        public static void DrawValueLabel(SKCanvas canvas, SKPoint target, string text, SKPoint offset, SKColor backColor)
        {
            // ---- 1. 计算框的位置和大小 ----
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
            })
            using (SKFont textFont = new SKFont()
            {
                Typeface = SKTypeface.FromFamilyName("微软雅黑"),
            })
            {
                // 测量文字宽度
                SKFontMetrics metrics = textFont.Metrics;
                float textWidth = textFont.MeasureText(text, textPaint);
                float textHeight = metrics.Descent - metrics.Ascent + metrics.Leading;

                float paddingX = 10f;      // 水平方向内边距
                float paddingY = 3f;       // 垂直方向内边距
                float boxWidth = textWidth + paddingX * 2;
                float boxHeight = textHeight + paddingY * 2;
                float radius = 6f;         // 圆角半径
                float arrowSize = 6f;      // 箭头尖角大小

                // 框的中心 = 目标点 + 偏移
                SKPoint boxCenter = new SKPoint(target.X + offset.X, target.Y + offset.Y);

                // 框的矩形区域
                var rect = new SKRect(
                    boxCenter.X - boxWidth / 2,
                    boxCenter.Y - boxHeight / 2,
                    boxCenter.X + boxWidth / 2,
                    boxCenter.Y + boxHeight / 2);


                // ---- 2. 构建带尖角的路径 ----
                using (SKPath path = new SKPath())
                {
                    path.AddRoundRect(rect, radius, radius);

                    // 在框朝向目标点的那条边添加一个三角尖角
                    // 判断偏移方向：目标点在框的哪个方向
                    // 这里简单地根据 offset 决定尖角贴在哪条边上
                    var arrowPath = new SKPath();

                    if (Math.Abs(offset.X) > Math.Abs(offset.Y))
                    {
                        // 水平方向偏移为主 -> 尖角在左/右边
                        float sideX = offset.X > 0 ? rect.Left : rect.Right;   // 目标点在框的另一侧
                        float margin = offset.X > 0 ? 5 : -5;
                        float cy = rect.MidY;
                        arrowPath.MoveTo(sideX, cy - arrowSize);
                        arrowPath.LineTo(sideX, cy + arrowSize);
                        arrowPath.LineTo(target.X + margin, target.Y);
                        arrowPath.Close();
                    }
                    else
                    {
                        // 垂直方向偏移为主 -> 尖角在上/下边
                        float sideY = offset.Y > 0 ? rect.Top : rect.Bottom;
                        float margin = offset.Y > 0 ? 5 : -5;
                        float cx = rect.MidX;
                        arrowPath.MoveTo(cx - arrowSize, sideY);
                        arrowPath.LineTo(cx + arrowSize, sideY);
                        arrowPath.LineTo(target.X, target.Y + margin);
                        arrowPath.Close();
                    }

                    // 合并框和尖角
                    path.AddPath(arrowPath);

                    // ---- 3. 绘制背景框 ----
                    using (var boxPaint = new SKPaint
                    {
                        Color = backColor,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawPath(path, boxPaint);
                    }

                    //// 可选：框边描边
                    //using (var borderPaint = new SKPaint
                    //{
                    //    Color = SKColors.White,
                    //    Style = SKPaintStyle.Stroke,
                    //    StrokeWidth = 1.5f,
                    //    IsAntialias = true
                    //})
                    //{
                    //    canvas.DrawPath(path, borderPaint);
                    //}
                }

                // ---- 4. 绘制数字（在框内居中）----
                // 垂直居中：基线位置 = 中心 + (ascent+descent的修正)
                float textY = boxCenter.Y - (metrics.Ascent + metrics.Descent) / 2;
                canvas.DrawText(text, boxCenter.X, textY, SKTextAlign.Center, textFont, textPaint);
            }

        }
    }
}
