using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcgAnalyseStart
{
    public class ColoredPointsRenderer
    {
        // 输入：原始点坐标和颜色数组
        public SKPoint[] points;
        public SKColor[] colors;

        // 输出：生成的三角形顶点和颜色数组
        private SKPoint[] triangleVertices;
        private SKColor[] vertexColors;

        // 预计算三角形顶点（只需执行一次）
        public void GenerateTriangles(float radius = 2.0f)
        {
            if (points == null || colors == null || points.Length != colors.Length)
                throw new ArgumentException("Points and colors arrays must be non-null and of equal length.");

            int vertexCount = points.Length * 3; // 每个点对应3个顶点
            triangleVertices = new SKPoint[vertexCount];
            vertexColors = new SKColor[vertexCount];

            // 等边三角形的3个顶点偏移量（相对于中心点）
            float sqrt3 = (float)Math.Sqrt(3);
            SKPoint[] triangleOffsets = {
                new SKPoint(0, -radius),                     // 顶部顶点
                new SKPoint(-sqrt3 * radius / 2, radius / 2), // 左下
                new SKPoint(sqrt3 * radius / 2, radius / 2)   // 右下
            };

            for (int i = 0; i < points.Length; i++)
            {
                SKPoint center = points[i];
                SKColor color = colors[i];

                // 为当前点生成3个三角形顶点
                for (int j = 0; j < 3; j++)
                {
                    triangleVertices[i * 3 + j] = new SKPoint(
                        center.X + triangleOffsets[j].X,
                        center.Y + triangleOffsets[j].Y);
                    vertexColors[i * 3 + j] = color; // 3个顶点颜色相同
                }
            }
        }

        // 绘制生成的三角形顶点
        public void Draw(SKCanvas canvas, SKPaint paint)
        {
            if (triangleVertices == null || vertexColors == null)
                throw new InvalidOperationException("GenerateTriangles must be called before Draw.");

            // 创建顶点数据（三角形模式）
            using (var vertices = SKVertices.CreateCopy(
                SKVertexMode.Triangles,
                triangleVertices,
                vertexColors))
            {
                canvas.DrawVertices(vertices, SKBlendMode.SrcOver, paint);
            }
        }
    }
}
