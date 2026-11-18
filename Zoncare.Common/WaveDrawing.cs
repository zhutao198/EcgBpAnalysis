using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Zoncare.Utilities;

namespace Zoncare.Common
{
    /// <summary>
    /// 波形相关像素点
    /// </summary>
    //public struct PointPX
    //{
    //    public int Index { get; set; }
    //    public float X { get; private set; }
    //    public float Y { get; private set; }
    //    public static readonly PointPX Empty = new PointPX(0, 0);

    //    public PointPX(float x, float y)
    //    {
    //        this.X = x;
    //        this.Y = y;
    //        this.Index = 0;
    //    }

    //    public void Move(float x, float y)
    //    {
    //        this.X = x;
    //        this.Y = y;
    //    }

    //    public void Offset(float x, float y)
    //    {
    //        this.X += x;
    //        this.Y += y;
    //    }

    //    public void paint(Graphics g)
    //    {
    //        g.DrawEllipse(Pens.Red, X - 1, Y - 1, 3, 3);
    //        g.SmoothingMode = SmoothingMode.AntiAlias;
    //    }

    //    public static bool operator ==(PointPX a, PointPX b)
    //    {
    //        return (a.X == b.X && a.Y == b.Y);
    //    }
    //    public static bool operator !=(PointPX a, PointPX b)
    //    {
    //        return (a.X != b.X || a.Y != b.Y);
    //    }
    //}

    /// <summary>
    /// 横轴或纵轴网格刻度信息
    /// </summary>
    public class GridMark
    {
        public int Index { get; set; }
        public float Position { get; set; }
        public Color GridColor { get; set; }
        public bool DashedLine { get; set; }

        public GridMark(float pos, bool dashedLine = false)
        {
            Position = pos;
            DashedLine = dashedLine;
            GridColor = SysConstants.SAMPLE_GRID_COLOR;
        }
    }

    /// <summary>
    /// 背景网格对象
    /// </summary>
    public class BackgroundGrid
    {
        /// <summary>
        /// 网格图像
        /// </summary>
        public Image GridImage { get; set; }
        /// <summary>
        /// 横轴网格刻度线
        /// </summary>
        public List<GridMark> XMarkList { get; set; }
        /// <summary>
        /// 纵轴网格刻度线
        /// </summary>
        public List<GridMark> YMarkList { get; set; }
    }

    /// <summary>
    /// 与波形绘制相关的图形元素信息
    /// </summary>
    public class GraphicsInfo
    {
        public Lead TheLead { get; set; }  //当前导联
        public int LeadIndex { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public bool LeadStatus { get; set; } //导联连接状态((true:已脱落; false:正常))
        public Rectangle Rect { get; set; } //绘图区域
        public Label LeadNameLabel { get; set; } //导联名称label
        public EcgCanvas Canvas { get; set; }
        public RenderUnitInfo CurrentRenderInfo { get; set; } //当前要渲染的信息(用于paint事件绘制波形)
        public PointD[] DataList { get; set; } //最近收到的数据点
    }

    /// <summary>
    /// 当前绘制单元的相关信息
    /// </summary>
    public class RenderUnitInfo : IDisposable
    {
        /// <summary>
        /// 需要刷新的工作区区域
        /// </summary>
        public Rectangle ClientRect { get; set; }

        /// <summary>
        /// 描点的工作区区域
        /// </summary>
        public RectangleF PointRect { get; set; }

        /// <summary>
        /// 当前波段描点绘线后所在区域的图像
        /// </summary>
        public Image PointImage { get; set; }

        /// <summary>
        /// 相关数据点集合
        /// </summary>
        public List<PointF> Points { get; set; } = new List<PointF>();

        /// <summary>
        /// 起搏信号位置
        /// </summary>
        public List<float> PacePositions { get; set; } = new List<float>();

        /// <summary>
        /// 导联绘制区域对象
        /// </summary>
        public EcgCanvas BaseCanvas { get; set; }

        /// <summary>
        /// 导联状态(是否脱落 true:已脱落; false:正常)
        /// </summary>
        public bool LeadStatus { get; set; }

        /// <summary>
        /// 绘制的相关导联波形的颜色
        /// </summary>
        public Color LeadColor { get; set; } = Color.LimeGreen;

        /// <summary>
        /// 走纸速度(mm/s)
        /// </summary>
        public float ScanSpeed { get; set; } = 25F;

        /// <summary>
        /// 增益(mm/mV)
        /// </summary>
        public float Gain { get; set; } = 10F;

        public RenderUnitInfo(EcgCanvas canvas)
        {
            BaseCanvas = canvas;
        }

        /// <summary>
        /// 重置绘图区域及相关数据点集合
        /// </summary>
        public void Reset()
        {
            ClientRect = Rectangle.Empty;
            PointRect = Rectangle.Empty;
            Points.Clear();

            PointImage.Dispose();
            PointImage = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); //通知垃圾回收器不用再调用终结器
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                //清理托管资源
                if (this.PointImage != null)
                {
                    this.PointImage.Dispose();
                    this.PointImage = null;
                }
            }

            //清理非托管资源
            //...
        }

        ~RenderUnitInfo()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// 绘制一个导联波形区域位图信息的对象
    /// </summary>
    public class EcgCanvas : IDisposable
    {
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest, // 目标设备的句柄
            int nXDest,     // 目标对象的左上角的X坐标
            int nYDest,     // 目标对象的左上角的X坐标
            int nWidth,     // 目标对象的矩形的宽度
            int nHeight,    // 目标对象的矩形的长度
            IntPtr hdcSrc,  // 源设备的句柄
            int nXSrc,      // 源对象的左上角的X坐标
            int nYSrc,      // 源对象的左上角的X坐标
            int dwRop       // 光栅的操作值
        );
        private const int SRCCOPY = 0xCC0020;

        public Lead CurLead { get; set; }
        public bool CurLeadStatus { get; set; }
        public bool LeadStatusChanged { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ChannelWidth { get; set; }
        public Color BackColor { get; set; }
        public Bitmap Image { get; private set; }   //基础板,画板(BasePlate)
        public Bitmap BaseGridImage { get; set; }   //网格线位图图像
        public List<PointF> AllPoints { get; private set; } //当前canvas包含的所有波形数据点
        public BackgroundGrid BaseGridInfo { get; set; }
        public EcgDeviceType DeviceType { get; set; } = EcgDeviceType.Device_unknow;
        public bool IsPaceOn { get; set; }
        public bool NeedReset { get; set; }

        /// <summary>
        /// 绘图区相对于外部画布的位置
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// 基线位置
        /// </summary>
        public int BaseLineY
        {
            get
            {
                return this.Height / 2;
            }
        }

        private Graphics _graphic;
        private List<PointD> _restPointDs { get; set; } //上一次描绘过程之后剩余未绘制的数据点(原始数据)
        private List<PointF> _restPoints { get; set; }  //上一次描绘过程之后剩余未绘制的数据点
        private static object _lockObj = new object();

        /// <summary>
        /// 初始化画板
        /// </summary>
        /// <param name="lead">相关导联</param>
        /// <param name="width">工作区宽度</param>
        /// <param name="height">工作区高度</param>
        /// <param name="channelWidth">波形显示区域高度</param>
        /// <param name="interval">数据点间距</param>
        /// <param name="basePoint">当前绘图区相对于界面的位置</param>
        public EcgCanvas(Lead lead, int width, int height, int channelWidth, Point basePoint)
        {
            this.CurLead = lead;
            this.Width = width;
            this.Height = height;
            this.ChannelWidth = Math.Min(channelWidth, height);
            this.Location = basePoint;

            this.Image = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            this.AllPoints = new List<PointF>();
            _graphic = Graphics.FromImage(this.Image);
            _graphic.SmoothingMode = SmoothingMode.AntiAlias;
            _graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            _graphic.Clear(Color.Transparent);
            //Init();
        }

        public void Init(bool drawGrid = false)
        {
            _graphic.Clear(Color.Transparent);
            if (drawGrid) drawGridLines();
        }

        public void Reset()
        {
            _restPoints?.Clear();
            _restPointDs?.Clear();
            _restPoints = null;
            _restPointDs = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); //通知垃圾回收器不用再调用终结器
        }

        /// <summary>
        /// 将当前图像绘制到指定的画面中的指定位置
        /// </summary>
        /// <param name="g"></param>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public void PaintTo(Graphics g, RectangleF dest, RectangleF src)
        {
            if (this.Image == null) { return; }

            g.DrawImage(this.Image, dest, src, GraphicsUnit.Pixel);
        }

        public void PaintTo(Graphics g, Point pos)
        {
            if (this.Image == null) { return; }
            g.DrawImageUnscaled(this.Image, pos);
        }

        public void PaintTo(Graphics g, Rectangle dest, Rectangle src)
        {
            if (this.Image == null) { return; }

            //IntPtr dc1 = _graphic.GetHdc();
            //IntPtr dc2 = g.GetHdc();

            //BitBlt(dc2, dest.X, dest.Y, dest.Width, dest.Height, dc1, src.X, src.Y, SRCCOPY);
            //_graphic.ReleaseHdc(dc1);
            //g.ReleaseHdc(dc2);

            using (Bitmap invalidatedImage = new Bitmap(src.Width, src.Height))
            {
                using (Graphics g0 = Graphics.FromImage(invalidatedImage))
                {
                    g0.DrawImage(this.Image, 0, 0, src, GraphicsUnit.Pixel);
                }

                g.DrawImageUnscaled(invalidatedImage, dest);
            }
        }

        /// <summary>
        /// 根据一系列数据点确定绘图区域并绘制线段
        /// </summary>
        /// <param name="points">数据点集合</param>
        public RenderUnitInfo Render(short[] points, float gain, float speed, out bool arrivedMax,
            int samplingRate = SysConstants.SAMPLE_RATE, float pixScale = SysConstants.PIX_SCALE, bool drawInMemory = true)
        {
            arrivedMax = false;
            RenderUnitInfo rendInfo = null;

            if (points.Length > 0)
            {
                float x = 0;
                List<PointF> ps = new List<PointF>();

                float gap = speed / samplingRate; //数据点之间的水平间距mm
                for (int i = 0; i <= points.Length - 1; i++)
                {
                    x = x + gap * pixScale;
                    if (x >= Width)
                    {
                        _restPoints = null;
                        arrivedMax = true;
                        break;
                    }

                    float y = BaseLineY - points[i] / 1000f * gain * pixScale;
                    ps.Add(new PointF(x, y));
                }

                if (ps.Count > 0)
                {
                    float minX = ps[0].X;
                    float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);

                    rendInfo = new RenderUnitInfo(this) { ClientRect = new Rectangle(0, 0, Width, Height) };
                    rendInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                    rendInfo.Points = ps;

                    if (drawInMemory)
                    {
                        _graphic.Clear(Color.Transparent);
                        //if (BaseGridImage != null) _graphic.DrawImage(BaseGridImage, 0, 0);

                        drawPointLines(_graphic, ps, null, 0, gain, true);
                    }
                }
            }

            return rendInfo;
        }

        /// <summary>
        /// 根据一系列数据点确定绘图区域并绘制线段
        /// </summary>
        /// <param name="points">数据点集合</param>
        /// <param name="amountLimit">要绘制的数据点的限制个数</param>
        public RenderUnitInfo Render(PointD[] points, float gain, float speed, out bool arrivedMax,
            int samplingRate = SysConstants.SAMPLE_RATE, float pixScale = SysConstants.PIX_SCALE, bool drawInMemory = true, int amountLimit = -1)
        {
            arrivedMax = false;
            RenderUnitInfo renderInfo = null;

            if (points.Length > 0)
            {
                float x = 0;
                List<PointF> ps = new List<PointF>();
                List<float> pacePos = new List<float>();

                float gap = speed / samplingRate; //数据点之间的水平间距mm
                for (int i = 0; i <= points.Length - 1; i++)
                {
                    x = x + gap * pixScale;
                    if (x >= Width)
                    {
                        _restPoints = null;
                        arrivedMax = true;
                        break;
                    }

                    short yVal = points[i].Val;
                    //判断起搏信号(蓝牙数据状态去除从低到高第二个字节之后, 起搏信号判断方法与iMac相同)
                    if (points[i].IsPacing)
                    {
                        pacePos.Add(x);

                        if (IsPaceOn)
                        {
                            //使起搏信号幅度至少为0.2mV
                            if (yVal >= 0 && yVal < 200)
                                yVal = (short)(CommonValues.PaceBoostHeight * 100);
                            else if (yVal <= 0 && yVal > -200)
                                yVal = (short)(CommonValues.PaceBoostHeight * -100);
                        }
                    }
                    if (points[i].Statu != 0) { }

                    float y = BaseLineY - yVal / 1000f * gain * pixScale;
                    ps.Add(new PointF(x, y));
                    if (amountLimit > 0 && ps.Count > amountLimit) break;
                }

                if (ps.Count > 0)
                {
                    float minX = ps[0].X;
                    float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);

                    renderInfo = new RenderUnitInfo(this) { ClientRect = new Rectangle(0, 0, Width, Height) };
                    renderInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                    renderInfo.Points = ps;
                    if (IsPaceOn)
                        renderInfo.PacePositions = pacePos;
                    else
                        renderInfo.PacePositions.Clear();

                    if (drawInMemory)
                    {
                        _graphic.Clear(Color.Transparent);
                        //if (BaseGridImage != null) _graphic.DrawImage(BaseGridImage, 0, 0);

                        drawPointLines(_graphic, ps, renderInfo.PacePositions, 0, gain, true);
                    }
                }
            }

            return renderInfo;
        }

        /// <summary>
        /// 根据一系列数据点确定绘图区域并绘制线段
        /// </summary>
        /// <param name="points">数据点集合</param>
        /// <param name="gain">增益</param>
        /// <param name="speed">走纸速度</param>
        /// <param name="arrivedMax">是否到达绘图区域右边界</param>
        /// <param name="leadOff">导联是否脱落</param>
        public RenderUnitInfo RenderNext(PointD[] points, float gain, float speed, out bool arrivedMax, bool leadOff = false, bool drawInMemory = false)
        {
            //lock (_lockObj)
            {
                arrivedMax = false;
                RenderUnitInfo renderInfo = null;

                if (points.Length > 0)
                {
                    int lastCount = (_restPoints?.Count ?? 0);
                    float x = 0;
                    List<PointF> ps = new List<PointF>();
                    List<float> pacePos = new List<float>();
                    if (lastCount > 0)
                    {
                        x = _restPoints[_restPoints.Count - 1].X;
                        for (int i = 0; i < _restPoints.Count; i++)
                        {
                            if (_restPointDs[i].IsPacing) pacePos.Add(_restPoints[i].X); //起搏信号
                            ps.Add(_restPoints[i]);
                        }
                        Reset();
                    }

                    int xMaxVal = 0, xMaxIndex = 0; //取整后的最大x值及其起始索引
                    float gap = speed / SysConstants.SAMPLE_RATE; //数据点之间的水平间距mm
                    for (int i = 0; i <= points.Length - 1; i++)
                    {
                        x = x + gap * SysConstants.PIX_SCALE;
                        if (x >= Width) //达到绘图区域右边界
                        {
                            arrivedMax = true;
                            Reset();
                            return null;
                        }

                        int xVal = (int)x;
                        if (xVal > xMaxVal) { xMaxVal = xVal; xMaxIndex = lastCount + i; }

                        short yVal = points[i].Val;
                        //if ((1 << 14) == (points[i].Y & (1 << 14))) { pacePos.Add(x); } //起搏信号
                        if (!leadOff)
                        {
                            if (points[i].IsPacing)
                            {
                                pacePos.Add(x);
                                if (IsPaceOn)
                                {
                                    if (yVal >= 0 && yVal < 200) //使起搏信号幅度至少为0.2mV
                                        yVal = (short)(CommonValues.PaceBoostHeight * 100);
                                    else if (yVal < 0 && yVal > -200)
                                        yVal = (short)(CommonValues.PaceBoostHeight * -100);
                                }
                            }
                        }
                        float y = BaseLineY - yVal / 1000f * gain * SysConstants.PIX_SCALE;
                        ps.Add(new PointF(x, y));
                    }

                    //保留x取整最大值之后的数据点(下次绘制)
                    List<PointF> restPoints = new List<PointF>();
                    List<PointD> restPointsDs = new List<PointD>();
                    for (int i = xMaxIndex; i < ps.Count; i++)
                    {
                        restPoints.Add(ps[i]);
                        restPointsDs.Add(points[i - lastCount]);
                    }
                    ps.RemoveRange(xMaxIndex, ps.Count - xMaxIndex);

                    //补一个边界点
                    if (restPoints.Count > 0 && ps.Count > 0)
                    {
                        float slope = (restPoints[0].Y - ps[ps.Count - 1].Y) / (restPoints[0].X - ps[ps.Count - 1].X);
                        PointF pMid = new PointF(xMaxVal, restPoints[0].Y - slope * (restPoints[0].X - xMaxVal));
                        ps.Add(pMid);
                        restPoints.Insert(0, pMid);
                        restPointsDs.Insert(0, new PointD(restPointsDs[0].Val, 0));
                    }

                    if (ps.Count > 0)
                    {
                        //确定绘图区域
                        int minX = (int)ps[0].X, maxX = xMaxVal;
                        float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);
                        Rectangle rect = new Rectangle(minX, 0, maxX - minX, this.Height);
                        renderInfo = new RenderUnitInfo(this) { ClientRect = rect, LeadStatus = leadOff };
                        renderInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                        renderInfo.Points = ps;
                        if (IsPaceOn)
                            renderInfo.PacePositions = pacePos;
                        else
                            renderInfo.PacePositions.Clear();

                        if (drawInMemory)
                        {
                            _graphic.FillRectangle(Brushes.Transparent, renderInfo.ClientRect);
                            //drawGridLines(_graphic, renderInfo);    //绘制网格线
                            //drawLeadName(_graphic, renderInfo);     //绘制导联名称
                            drawPointLines(_graphic, ps, renderInfo.PacePositions, 0, gain);
                        }
                    }
                    else
                    {
                    }

                    _restPoints = restPoints;
                    _restPointDs = restPointsDs;
                }
                else
                {
                }

                return renderInfo;
            }
        }

        /// <summary>
        /// 根据一系列数据点确定绘图区域并绘制线段
        /// </summary>
        /// <param name="points">数据点集合</param>
        /// <param name="lastP">上一次绘制的结尾位置</param>
        [Obsolete]
        public RenderUnitInfo RenderNext(List<short> points, float gain, float speed, out bool arrivedMax)
        {
            lock (_lockObj)
            {
                arrivedMax = false;
                RenderUnitInfo rendInfo = null;

                if (points.Count > 0)
                {
                    float x = 0;
                    List<PointF> ps = new List<PointF>();
                    if (_restPoints != null && _restPoints.Count > 0)
                    {
                        x = _restPoints[_restPoints.Count - 1].X;
                        ps.AddRange(_restPoints);
                        _restPoints = null;
                    }

                    int xMaxVal = 0, xMaxIndex = 0; //取整后的最大x值及其起始索引
                    float gap = speed / SysConstants.SAMPLE_RATE; //数据点之间的水平间距mm
                    for (int i = 0; i <= points.Count - 1; i++)
                    {
                        x = x + gap * SysConstants.PIX_SCALE;
                        if (x >= Width)
                        {
                            _restPoints = null;
                            arrivedMax = true;
                            return null;
                            ps.Clear();
                            x = 0; //描迹达到右边界后重新从起始位置绘制
                            xMaxVal = xMaxIndex = 0;
                        }

                        int xVal = (int)x;
                        if (xVal > xMaxVal) { xMaxVal = xVal; xMaxIndex = ps.Count; }

                        float y = BaseLineY - points[i] / 1000f * gain * SysConstants.PIX_SCALE;
                        ps.Add(new PointF(x, y));
                    }

                    //保留x取整最大值之后的数据点(下次绘制)
                    List<PointF> restPoints = new List<PointF>();
                    for (int i = xMaxIndex; i < ps.Count; i++)
                    {
                        restPoints.Add(ps[i]);
                    }
                    ps.RemoveRange(xMaxIndex, ps.Count - xMaxIndex);

                    //补一个边界点
                    if (restPoints.Count > 0 && ps.Count > 0)
                    {
                        float slope = (restPoints[0].Y - ps[ps.Count - 1].Y) / (restPoints[0].X - ps[ps.Count - 1].X);
                        PointF pMid = new PointF(xMaxVal, restPoints[0].Y - slope * (restPoints[0].X - xMaxVal));
                        ps.Add(pMid);
                        restPoints.Insert(0, pMid);
                    }

                    if (ps.Count > 0)
                    {
                        //确定绘图区域
                        int minX = (int)ps[0].X, maxX = xMaxVal;
                        float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);
                        Rectangle rect = new Rectangle(minX, 0, maxX - minX, this.Height);
                        rendInfo = new RenderUnitInfo(this) { ClientRect = rect };
                        rendInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                        rendInfo.Points = ps;

                        //rendInfo.PointImage = new Bitmap((int)Math.Ceiling(rect.Width), this.Height);
                        //using (Graphics g = Graphics.FromImage(rendInfo.PointImage))
                        //{
                        //    //清除绘图区域
                        //    //g.Clear(this.BackColor);

                        //    //复制网格线背景
                        //    if (this.BaseGridImage != null)
                        //    {
                        //        drawGridLines(g, rendInfo);
                        //    }

                        //    drawPointLines(g, points, minX);
                        //}
                    }
                    else
                    {

                    }

                    _restPoints = restPoints;
                }
                else
                {

                }

                return rendInfo;
            }
        }

        public RenderUnitInfo RenderNext2<T>(List<T> points, float gain, float speed, out bool arrivedMax)
        {
            lock (_lockObj)
            {
                arrivedMax = false;
                RenderUnitInfo rendInfo = null;

                if (points.Count > 0)
                {
                    float x = 0;
                    List<PointF> ps = new List<PointF>();
                    List<float> pacePos = new List<float>();
                    if (_restPoints != null && _restPoints.Count > 0)
                    {
                        x = _restPoints[_restPoints.Count - 1].X;
                        ps.AddRange(_restPoints);
                        _restPoints = null;
                    }

                    int xMaxVal = 0, xMaxIndex = 0; //取整后的最大x值及其起始索引
                    float gap = speed / SysConstants.SAMPLE_RATE; //数据点之间的水平间距mm
                    for (int i = 0; i <= points.Count - 1; i++)
                    {
                        x = x + gap * SysConstants.PIX_SCALE;
                        if (x >= Width)
                        {
                            arrivedMax = true;
                            _restPoints = null;
                            return null;

                            x = 0; //描迹达到右边界后重新从起始位置绘制
                            ps.Clear();
                            xMaxVal = xMaxIndex = 0;
                        }

                        int xVal = (int)x;
                        if (xVal > xMaxVal) { xMaxVal = xVal; xMaxIndex = ps.Count; }

                        short pVal = 0;
                        if (typeof(T) == typeof(short))
                        {
                            pVal = Convert.ToInt16(points[i]);
                        }
                        else if (typeof(T) == typeof(PointD))
                        {
                            pVal = ((PointD)Convert.ChangeType(points[i], typeof(PointD))).Val;
                        }

                        float y = BaseLineY - pVal / 1000f * gain * SysConstants.PIX_SCALE;
                        ps.Add(new PointF(x, y));
                    }

                    //保留x取整最大值之后的数据点(下次绘制)
                    List<PointF> restPoints = new List<PointF>();
                    for (int i = xMaxIndex; i < ps.Count; i++)
                    {
                        restPoints.Add(ps[i]);
                    }
                    ps.RemoveRange(xMaxIndex, ps.Count - xMaxIndex);

                    //补一个边界点
                    if (restPoints.Count > 0 && ps.Count > 0)
                    {
                        float slope = (restPoints[0].Y - ps[ps.Count - 1].Y) / (restPoints[0].X - ps[ps.Count - 1].X);
                        PointF pMid = new PointF(xMaxVal, restPoints[0].Y - slope * (restPoints[0].X - xMaxVal));
                        ps.Add(pMid);
                        restPoints.Insert(0, pMid);
                    }

                    if (ps.Count > 0)
                    {
                        //确定绘图区域
                        int minX = (int)ps[0].X, maxX = xMaxVal;
                        float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);
                        Rectangle rect = new Rectangle(minX, 0, maxX - minX, this.Height);
                        rendInfo = new RenderUnitInfo(this) { ClientRect = rect };
                        rendInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                        rendInfo.Points = ps;
                        rendInfo.PacePositions = pacePos;
                    }
                    else
                    {

                    }

                    _restPoints = restPoints;
                }
                else
                {

                }

                return rendInfo;
            }
        }

        /// <summary>
        /// 计算指定区域实际可绘制的波形区域大小
        /// </summary>
        /// <returns></returns>
        public static Rectangle CalculateRenderRectangle(Rectangle gridRect, float gap = SysConstants.PIX_SCALE, float sizeScale = 1)
        {
            Rectangle realGridRect;
            int xs = (int)(gridRect.Width / (gap * 5)); //水平方向大格子个数 (5mm)
            int ys = (int)(gridRect.Height / (gap * 5)); //垂直方向大格子个数
            float w = xs * gap * 5, h = ys * gap * 5;
            RectangleF rect = new RectangleF(gridRect.Left + (gridRect.Width - w) / 2, gridRect.Top, w, h);

            realGridRect = Rectangle.Truncate(rect);
            return realGridRect;
        }

        /// <summary>
        /// 计算绘图区域可绘制的波形数据点个数
        /// </summary>
        /// <returns></returns>
        public static int CalculateRenderLimit(Rectangle rect, float speed, int samplingRate = SysConstants.SAMPLE_RATE, float pixScale = SysConstants.PIX_SCALE)
        {
            float gap = speed / samplingRate * pixScale; //数据点之间的水平间距(px)
            return (int)(rect.Width / gap);
        }

        /// <summary>
        /// 绘制背景网格线
        /// </summary>
        /// <param name="allW"></param>
        /// <param name="allH"></param>
        /// <param name="gridColor"></param>
        /// <param name="xMarkList"></param>
        /// <param name="yMarkList"></param>
        /// <returns></returns>
        public static Bitmap DrawGridLines(int allW, int allH, out List<GridMark> xMarkList, out List<GridMark> yMarkList, bool forReport = false)
        {
            Bitmap img = new Bitmap(allW, allH);
            int idx = 0;
            float x = 0, y = 0; //横坐标, 纵坐标
            xMarkList = new List<GridMark>();
            yMarkList = new List<GridMark>();

            Color gridColor = SysConstants.SAMPLE_GRID_COLOR;
            Color dashColor = PubFuns.GetColorPlus(gridColor, 0, 30, 30, 30);
            if (forReport)
            {
                gridColor = CommonValues.ReportGridColor5;
                dashColor = CommonValues.ReportGridColor1;
                //gridColor = dashColor = CommonValues.ReportGridColor5;
            }

            using (Graphics g = Graphics.FromImage(img))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (Pen dashPen = new Pen(dashColor, 0.5f), solidPen = new Pen(gridColor, 1f))
                {
                    dashPen.DashPattern = new float[] { 1f, SysConstants.PIX_SCALE - 1f, 1f, SysConstants.PIX_SCALE - 1f };

                    //画竖线
                    do
                    {
                        if (idx % 5 == 0)
                        {
                            g.DrawLine(solidPen, x, 0, x, allH);
                            xMarkList.Add(new GridMark(x, false) { Index = xMarkList.Count });
                            idx = 0;
                        }
                        else
                        {
                            g.DrawLine(dashPen, x, 0, x, allH);
                            xMarkList.Add(new GridMark(x, true) { Index = xMarkList.Count });
                        }
                        x += SysConstants.PIX_SCALE * 5;
                        idx++;
                    } while (x < allW);

                    //画横线
                    idx = 0;
                    do
                    {
                        if (idx % 5 == 0)
                        {
                            g.DrawLine(solidPen, 0, y, allW, y);
                            yMarkList.Add(new GridMark(y, false) { Index = yMarkList.Count });
                            idx = 0;
                        }
                        else
                        {
                            g.DrawLine(dashPen, 0, y, allW, y);
                            yMarkList.Add(new GridMark(y, true) { Index = yMarkList.Count });
                        }
                        y += SysConstants.PIX_SCALE * 5;
                        idx++;
                    } while (y < allH);
                }
            }

            return img;
        }

        /// <summary>
        /// 绘制放大波形的背景网格
        /// </summary>
        /// <param name="allW"></param>
        /// <param name="allH"></param>
        /// <param name="pixScale">波形放大后与1mm相当的像素</param>
        /// <returns></returns>
        public static Bitmap DrawGridLines(int allW, int allH, float pixScale = SysConstants.PIX_SCALE)
        {
            Bitmap img = new Bitmap(allW, allH);
            int idx = 0;
            float x = 0, y = 0; //横坐标, 纵坐标
            List<float> lstDotX = new List<float>();

            Color gridColor = CommonValues.ReportGridColor5;
            Color dashColor = CommonValues.ReportGridColor1;

            using (Graphics g = Graphics.FromImage(img))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (Pen solidPen = new Pen(gridColor, 1f))
                {
                    //画竖线
                    do
                    {
                        if (idx % 5 == 0)
                        {
                            g.DrawLine(solidPen, x, 0, x, allH);
                            idx = 0;
                        }
                        else
                        {
                            lstDotX.Add(x);
                        }
                        x += pixScale;
                        idx++;
                    } while (x < allW);

                    //画横线
                    idx = 0;
                    using (SolidBrush brush = new SolidBrush(dashColor))
                    {
                        do
                        {
                            if (idx % 5 == 0)
                            {
                                g.DrawLine(solidPen, 0, y, allW, y);
                                idx = 0;
                            }
                            else
                            {
                                //绘制1mm单元点
                                foreach (float dotX in lstDotX)
                                {
                                    g.FillEllipse(brush, dotX - 1, y - 1, 2, 2);
                                }
                            }
                            y += pixScale;
                            idx++;
                        } while (y < allH);
                    }
                }
            }

            return img;
        }


        /// <summary>
        /// 在指定区域绘制网格(网格及网格点)
        /// </summary>
        public static void DrawGridForReport(Graphics g, Rectangle gridRect, out Rectangle realGridRect,
            float gap = SysConstants.PIX_SCALE, float sizeScale = 1, bool isPrint = false)
        {
            int xs = (int)(gridRect.Width / (gap * 5)); //水平方向大格子个数 (5mm)
            int ys = (int)(gridRect.Height / (gap * 5)); //垂直方向大格子个数
            float w = xs * gap * 5, h = ys * gap * 5;
            RectangleF rect = new RectangleF(gridRect.Left + (gridRect.Width - w) / 2, gridRect.Top, w, h);

            int rIndex = 0, cIndex = 0;
            decimal gapD = (decimal)gap, gapS = (decimal)(gap * 5);
            decimal x = (decimal)rect.Left, y = (decimal)rect.Top; //横坐标, 纵坐标
            float solidWidth = Math.Max(3F * sizeScale, 1F);
            float dotWidth = Math.Max(3F * sizeScale, 1F);
            Color gridColor = CommonValues.ReportGridColor5;
            Color dotColor = CommonValues.ReportGridColor1;

            if (isPrint)
            {
                //gridColor = SysConstants.PRINT_GRID_COLOR;
                //dotColor = Color.FromArgb(60, 60, 60); //Color.Black;
                if (CommonValues.ReportGridStyle == 1)
                {
                    DrawGridLinesForReport(g, gridRect, gridColor, out realGridRect, gap, sizeScale);
                    return;
                }
            }

            using (SolidBrush brush = new SolidBrush(dotColor))
            {
                using (Pen solidPen = new Pen(gridColor, solidWidth))
                {
                    //画竖线
                    int idx = 0;
                    do
                    {
                        g.DrawLine(solidPen, (float)x, rect.Top, (float)x, rect.Bottom);
                        x = x + gapS;
                        idx++;
                    } while (x < (decimal)(rect.Right + gap / 2));


                    //画横线
                    do
                    {
                        if (rIndex % 5 == 0)
                        {
                            g.DrawLine(solidPen, rect.Left, (float)y, rect.Right, (float)y);
                            rIndex = 0;
                        }
                        else
                        {
                            x = (decimal)rect.Left;
                            while (x + gapS < (decimal)(rect.Right + gap / 2)) //绘制大网格内部的点
                            {
                                for (int i = 1; i < 5; i++)
                                {
                                    g.FillEllipse(brush, (float)(x + i * gapD) - dotWidth / 2, (float)y - dotWidth / 2, dotWidth, dotWidth);
                                }
                                x = x + gapS;
                            }
                        }
                        y = y + (decimal)gap;
                        rIndex++;
                    } while (y < (decimal)(rect.Bottom + gap / 2));
                }
            }

            realGridRect = Rectangle.Truncate(rect);
        }

        /// <summary>
        /// 在指定区域绘制网格(网格线)
        /// </summary>
        public static void DrawGridLinesForReport(Graphics g, Rectangle gridRect, Color gridColor, out Rectangle realGridRect,
            float gap = SysConstants.PIX_SCALE, float sizeScale = 1)
        {
            int xs = (int)(gridRect.Width / (gap * 5)); //水平方向大格子个数 (5mm)
            int ys = (int)(gridRect.Height / (gap * 5)); //垂直方向大格子个数
            float w = xs * gap * 5, h = ys * gap * 5;
            RectangleF rect = new RectangleF(gridRect.Left + (gridRect.Width - w) / 2, gridRect.Top, w, h);

            int idx = 0;
            decimal x = (decimal)rect.Left, y = (decimal)rect.Top; //横坐标, 纵坐标
            float solidWidth = Math.Max(3F * sizeScale, 1F);
            float dashWidth = Math.Max(1F * sizeScale, 1F);
            Color dashColor = gridColor = PubFuns.GetColorPlus(gridColor, 0, -30, -30, -30); //颜色加深一点
            //Color dashColor = PubFuns.GetColorPlus(gridColor, 0, -60, -60, -60);
            //Color dashColor = Color.FromArgb(60, 60, 60);

            using (Pen dashPen = new Pen(dashColor, dashWidth), solidPen = new Pen(gridColor, solidWidth))
            {
                //画竖线
                do
                {
                    if (idx % 5 == 0)
                    {
                        g.DrawLine(solidPen, (float)x, rect.Top, (float)x, rect.Bottom);
                        idx = 0;
                    }
                    else
                    {
                        g.DrawLine(dashPen, (float)x, rect.Top, (float)x, rect.Bottom);
                    }
                    x = x + (decimal)gap;
                    idx++;
                } while (x < (decimal)(rect.Right + gap / 2));

                //idx--;
                //if (idx % 5 != 0)//没有画完整
                //{
                //    int gapCnt = idx % 5;//余数代表已经画的格数
                //    x = x - (decimal)gap;
                //    //如果实际宽度足够，凑足5格
                //    if (((decimal)gridRect.Right - x) - (5 - gapCnt) * (decimal)gap > 0)
                //    {
                //        for (int i = 0; i < 5 - gapCnt; i++)
                //        {
                //            x = x + (decimal)gap;
                //            if (i == 4 - gapCnt)
                //            {
                //                g.DrawLine(solidPen, (float)x, rect.Top, (float)x, rect.Bottom);
                //            }
                //            else
                //            {
                //                g.DrawLine(dashPen, (float)x, rect.Top, (float)x, rect.Bottom);
                //            }
                //        }
                //        rect.Width = (float)x - rect.Left;
                //    }
                //}


                //画横线
                idx = 0;
                do
                {
                    if (idx % 5 == 0)
                    {
                        g.DrawLine(solidPen, rect.Left, (float)y, rect.Right, (float)y);
                        idx = 0;
                    }
                    else
                    {
                        g.DrawLine(dashPen, rect.Left, (float)y, rect.Right, (float)y);
                    }
                    y = y + (decimal)gap;
                    idx++;
                } while (y < (decimal)(rect.Bottom + gap / 2));

                //idx--;
                //if (idx % 5 != 0)//没有画完整
                //{
                //    int gapCnt = idx % 5;
                //    y = y - (decimal)gap;
                //    if (((decimal)gridRect.Bottom - y) - (5 - gapCnt) * (decimal)gap > 0)
                //    {
                //        for (int i = 0; i < 5 - gapCnt; i++)
                //        {
                //            y = y + (decimal)gap;
                //            if (i == 4 - gapCnt)
                //            {
                //                g.DrawLine(solidPen, rect.Left, (float)y, rect.Right, (float)y);
                //            }
                //            else
                //            {
                //                g.DrawLine(dashPen, rect.Left, (float)y, rect.Right, (float)y);
                //            }
                //        }
                //        rect.Height = (float)y - rect.Top;
                //    }
                //}
            }

            //new Rectangle((int)rect.Left, (int)rect.Top, (int)Math.Ceiling(rect.Width)+1, (int)Math.Ceiling(rect.Height)+1) ;
            realGridRect = Rectangle.Truncate(rect);
        }

        private void drawGridLines()
        {
            int idx = 0;
            float x = 0, y = 0; //横坐标, 纵坐标
            using (Pen dashPen = new Pen(SysConstants.SAMPLE_GRID_COLOR, 0.5f), solidPen = new Pen(SysConstants.SAMPLE_GRID_COLOR))
            {
                dashPen.DashPattern = new float[] { 1f, SysConstants.PIX_SCALE - 1f, 1f, SysConstants.PIX_SCALE - 1f };

                //画竖线
                do
                {
                    if (idx % 5 == 0)
                    {
                        _graphic.DrawLine(solidPen, x, 0, x, this.Height);
                        idx = 0;
                    }
                    else
                    {
                        _graphic.DrawLine(dashPen, x, 0, x, this.Height);
                    }
                    x += SysConstants.PIX_SCALE * 5;
                    idx++;
                } while (x < this.Width);

                //画横线
                idx = 0;
                do
                {
                    if (idx % 5 == 0)
                    {
                        _graphic.DrawLine(solidPen, 0, y, this.Width, y);
                        idx = 0;
                    }
                    else
                    {
                        _graphic.DrawLine(dashPen, 0, y, this.Width, y);
                    }
                    y += SysConstants.PIX_SCALE * 5;
                    idx++;
                } while (y < this.Height);
            }
        }

        private void drawGridLines(Graphics g, RenderUnitInfo renderInfo)
        {
            RectangleF clientRect = renderInfo.ClientRect;
            clientRect.Offset(this.Location);

            using (Pen dashPen = new Pen(SysConstants.SAMPLE_GRID_COLOR, 0.5f), solidPen = new Pen(SysConstants.SAMPLE_GRID_COLOR))
            {
                dashPen.DashPattern = new float[] { 1f, SysConstants.PIX_SCALE - 1f, 1f, SysConstants.PIX_SCALE - 1f };

                #region 绘制将要渲染的区域的网格线
                if (this.BaseGridInfo != null)
                {
                    //绘制横轴上对应的网格线(垂直方向)
                    List<GridMark> lstMarkX = this.BaseGridInfo.XMarkList;
                    GridMark gridMarkX = lstMarkX.FirstOrDefault(o => o.Position >= clientRect.Left);
                    if (gridMarkX != null)
                    {
                        while (gridMarkX.Position <= clientRect.Right)
                        {
                            if (gridMarkX.DashedLine)
                                g.DrawLine(dashPen, gridMarkX.Position - this.Location.X, 0, gridMarkX.Position - this.Location.X, clientRect.Height);
                            else
                                g.DrawLine(solidPen, gridMarkX.Position - this.Location.X, 0, gridMarkX.Position - this.Location.X, clientRect.Height);

                            if (gridMarkX.Index < lstMarkX.Count - 1)
                                gridMarkX = lstMarkX[gridMarkX.Index + 1];
                            else
                                break;
                        }
                    }

                    //绘制纵轴上对应的网格线(水平方向)
                    List<GridMark> lstMarkY = this.BaseGridInfo.YMarkList;
                    GridMark gridMarkY = lstMarkY.FirstOrDefault(o => o.Position >= clientRect.Top);
                    if (gridMarkY != null)
                    {
                        while (gridMarkY.Position <= clientRect.Bottom)
                        {
                            if (gridMarkY.DashedLine)
                                g.DrawLine(dashPen, 0, gridMarkY.Position - this.Location.Y, clientRect.Width, gridMarkY.Position - this.Location.Y);
                            else
                                g.DrawLine(solidPen, 0, gridMarkY.Position - this.Location.Y, clientRect.Width, gridMarkY.Position - this.Location.Y);

                            if (gridMarkY.Index < lstMarkY.Count - 1)
                                gridMarkY = lstMarkY[gridMarkY.Index + 1];
                            else
                                break;
                        }
                    }
                }
                #endregion
            }
        }

        private void drawPointLines(Graphics g, List<PointF> points, List<float> pacePos, float offsetX, float gain, bool forReport = false)
        {
            Color pointColor = Color.LimeGreen, paceColor = Color.White;
            if (forReport)
            {
                pointColor = CommonValues.ReportWaveColor;
                paceColor = pointColor;
            }

            if (points.Count > 1)
            {
                using (Pen pointPen = new Pen(pointColor, 1.5F))
                {
                    PointF[] ps = new PointF[points.Count];
                    for (int n = 0; n < points.Count; n++)
                    {
                        PointF p = points[n].Offset(offsetX, 0);
                        ps[n] = p;
                    }
                    g.DrawLines(pointPen, ps);
                }

                if (IsPaceOn)
                {
                    if (pacePos != null)
                    {
                        using (Pen pacePen = new Pen(paceColor))
                        {
                            float paceFlagTop = 3;
                            float paceFlagHeight = CommonValues.PaceBoostHeight * SysConstants.PIX_SCALE; //起搏标识需至少为2mm高度
                            foreach (float pos in pacePos)
                            {
                                g.DrawLine(pacePen, offsetX + pos, paceFlagTop, offsetX + pos, paceFlagTop + paceFlagHeight);
                            }
                        }
                    }
                }
            }
        }

        private void drawLeadName(Graphics g, RenderUnitInfo renderInfo)
        {
            //绘制导联分割线
            Rectangle lineRect = new Rectangle(1, BaseLineY - ChannelWidth / 2 + 3, 2, ChannelWidth);
            if (renderInfo.ClientRect.IntersectsWith(lineRect))
            {
                using (Pen pen = new Pen(Color.Gold))
                {
                    pen.DashPattern = new float[] { 1.5f, 1.5f, 1.5f, 1.5f };
                    g.DrawLine(pen, lineRect.Left, lineRect.Top, lineRect.Left, lineRect.Top + (int)(this.Height * 0.8));
                }
            }

            //绘制导联名称
            string leadName = CurLead.RealName;
            using (Font leadFont = new Font("微软雅黑", 12))
            {
                SizeF textSize = g.MeasureString(leadName, leadFont);
                Rectangle textRect = new Rectangle(3, BaseLineY - ChannelWidth / 2, (int)Math.Ceiling(textSize.Width), (int)Math.Ceiling(textSize.Height));
                if (renderInfo.ClientRect.IntersectsWith(textRect))
                {
                    if (renderInfo.LeadStatus == false)
                    {
                        g.DrawString(leadName, leadFont, Brushes.White, textRect);
                    }
                    else
                    {
                        g.DrawString(leadName, leadFont, Brushes.Red, textRect);
                    }
                }
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                //清理托管资源
                if (this._graphic != null)
                {
                    this._graphic.Dispose();
                    this._graphic = null;
                }
                if (this.Image != null)
                {
                    this.Image.Dispose();
                    this.Image = null;
                }
                if (this.BaseGridImage != null)
                {
                    this.BaseGridImage.Dispose();
                    this.BaseGridImage = null;
                }
            }

            //清理非托管资源
            //...
        }

        ~EcgCanvas()
        {
            Dispose(false);
        }
    }

    public class EcgCanvasAdv : IDisposable
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int ChannelWidth { get; set; }
        public Color BackColor { get; set; }
        public Bitmap Image { get; private set; }   //基础板,画板(BasePlate)
        public Bitmap BaseGridImage { get; set; }   //网格线位图图像
        public List<PointF> AllPoints { get; private set; } //当前canvas包含的所有波形数据点
        public BackgroundGrid BaseGridInfo { get; set; }
        public PointF LastPoint { get; set; }
        public float LastBaseX { get; set; } //上一次绘制向量环的基点位置(水平方向)
        public bool NeedReset { get; set; } //resize后需要重置Canvas

        /// <summary>
        /// 绘图区相对于外部画布的位置
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// 基线位置
        /// </summary>
        public int BaseLineY
        {
            get
            {
                return this.Height / 2;
            }
        }
        private Graphics _graphic;
        private static object _lockObj = new object();

        /// <summary>
        /// 初始化画板
        /// </summary>
        /// <param name="lead">相关导联</param>
        /// <param name="width">工作区宽度</param>
        /// <param name="height">工作区高度</param>
        /// <param name="channelWidth">波形显示区域高度</param>
        /// <param name="interval">数据点间距</param>
        /// <param name="basePoint">当前绘图区相对于界面的位置</param>
        public EcgCanvasAdv(Lead lead, int width, int height, int channelWidth, Point basePoint)
        {
            this.Width = width;
            this.Height = height;
            this.ChannelWidth = Math.Min(channelWidth, height);
            this.Location = basePoint;

            this.Image = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            this.AllPoints = new List<PointF>();
            _graphic = Graphics.FromImage(this.Image);
            _graphic.SmoothingMode = SmoothingMode.AntiAlias;
            _graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            _graphic.Clear(Color.Transparent);
        }

        public void Init(bool drawGrid = false)
        {
            _graphic.Clear(Color.Transparent);
        }

        public void Reset()
        {
            LastPoint = Point.Empty;
            LastBaseX = 0;
            ClearRender();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); //通知垃圾回收器不用再调用终结器
        }

        /// <summary>
        /// 将当前图像绘制到指定的画面中的指定位置
        /// </summary>
        /// <param name="g"></param>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public void PaintTo(Graphics g, RectangleF dest, RectangleF src)
        {
            if (this.Image == null) { return; }
            g.DrawImage(this.Image, dest, src, GraphicsUnit.Pixel);
        }

        public void PaintTo(Graphics g, Point pos)
        {
            if (this.Image == null) { return; }
            g.DrawImageUnscaled(this.Image, pos);
        }

        /// <summary>
        /// 绘制"X", "Y", "Z"导联波形
        /// </summary>
        /// <param name="points">单导联将要绘制的波形数据</param>
        /// <param name="gain">增益</param>
        /// <param name="speed">纸速</param>
        /// <param name="arrivedMax">输出参数, 是否已到达绘制区域右边界</param>
        /// <param name="pixScale">心电图网格每一小格或每毫米对应的像素[1mm = 3.7795px]</param>
        /// <param name="isBrowse">是否在到达绘制区右边界保留数据显示</param>
        /// <returns></returns>
        public RenderUnitInfoAdv Render(List<PointD> points, float gain, float speed, out bool arrivedMax,
            int samplingRate = SysConstants.SAMPLE_RATE, float pixScale = SysConstants.PIX_SCALE, bool isBrowse = false)
        {
            lock (_lockObj)
            {
                arrivedMax = false;
                RenderUnitInfoAdv rendInfo = null;
                if (points != null && points.Count > 0)
                {
                    List<PointF> ps = new List<PointF>();
                    float x = this.LastPoint.X; //上一次绘制的最后一个点的位置
                    float y = 0;
                    float gap = speed / samplingRate; //数据点之间的水平间距mm
                    if (LastPoint.X > 0)
                    {
                        ps.Add(LastPoint);
                    }
                    for (int i = 0; i < points.Count; i++)
                    {
                        x = x + gap * pixScale;
                        y = points[i].Val * gain * pixScale / 1000;
                        if (x >= Width)
                        {
                            arrivedMax = true;
                            this.LastPoint = PointF.Empty;
                            if (!isBrowse)
                            {
                                ps.Clear();
                                _graphic.Clear(Color.Transparent);
                            }
                            break;
                        }
                        ps.Add(new PointF(x, BaseLineY - y));
                    }

                    if (ps.Count > 0)
                    {
                        float minX = ps[0].X, maxX = ps.Max(o => o.X);
                        float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);

                        //rendInfo = new RenderUnitInfoAdv(this) { ClientRect = new RectangleF(minX, minY, maxX - minX, Math.Max(2, (maxY - minY))) };
                        rendInfo = new RenderUnitInfoAdv(this) { ClientRect = new RectangleF(minX, 0, maxX - minX, this.Height) };
                        rendInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                        rendInfo.Points = ps;
                        drawPointLines(_graphic, ps, 0);
                        this.LastPoint = ps[ps.Count - 1];
                    }
                }
                return rendInfo;
            }
        }

        public RenderUnitInfoAdv RenderRpt(List<PointD> points, float gain, float speed, out bool arrivedMax,
            int samplingRate = SysConstants.SAMPLE_RATE, float pixScale = SysConstants.PIX_SCALE, bool isBrowse = false)
        {
            lock (_lockObj)
            {
                arrivedMax = false;
                RenderUnitInfoAdv rendInfo = null;
                if (points != null && points.Count > 0)
                {
                    List<PointF> ps = new List<PointF>();
                    float x = 0;
                    float y = 0;
                    float gap = speed / samplingRate; //数据点之间的水平间距mm
                    for (int i = 0; i < points.Count; i++)
                    {
                        x = x + gap * pixScale;
                        y = points[i].Val * gain * pixScale / 1000;
                        if (x >= Width)
                        {
                            arrivedMax = true;
                            break;
                        }
                        ps.Add(new PointF(x, BaseLineY - y));
                    }

                    if (ps.Count > 0)
                    {
                        float minX = ps[0].X, maxX = ps.Max(o => o.X);
                        float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);

                        rendInfo = new RenderUnitInfoAdv(this) { ClientRect = new RectangleF(minX, 0, maxX - minX, this.Height) };
                        rendInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                        rendInfo.Points = ps;
                        drawPointLines(_graphic, ps, 0);
                    }
                }
                return rendInfo;
            }
        }

        /// <summary>
        /// 绘制"X-Y", "X-Z", "Z-Y"导联波形
        /// </summary>
        public RenderUnitInfoAdv Render(List<PointF> points, float gain, float speed, out bool arrivedMax,
            int samplingRate = SysConstants.SAMPLE_RATE, float pixScale = SysConstants.PIX_SCALE, bool isBrowse = false)
        {
            lock (_lockObj)
            {
                arrivedMax = false;
                RenderUnitInfoAdv rendInfo = null;
                float scale = 1f;
                if (points != null && points.Count > 0)
                {
                    List<PointF> ps = new List<PointF>();
                    float x = 0, y = 0;
                    float gap = speed / samplingRate; //数据点之间的水平间距mm
                    if (LastPoint.X > 0)
                    {
                        ps.Add(LastPoint);
                    }

                    float lastPx = LastBaseX;
                    for (int i = 0; i < points.Count; i++)
                    {
                        float offset = i * gap * pixScale;
                        lastPx = lastPx + gap * pixScale;
                        //x = LastPoint.X + scale * points[i].X * gain * pixScale / 1000F + offset;
                        x = lastPx + scale * points[i].X * gain * pixScale / 1000F; //水平方向(基于垂直方向度量)的偏移量
                        if (lastPx >= Width)
                        {
                            arrivedMax = true;
                            LastPoint = PointF.Empty;
                            LastBaseX = 0;
                            if (!isBrowse)
                            {
                                ps.Clear();
                                _graphic.Clear(Color.Transparent);
                            }
                            break;
                        }

                        y = scale * points[i].Y * gain * pixScale / 1000;
                        ps.Add(new PointF(x, BaseLineY - y));
                    }

                    if (ps.Count > 0)
                    {
                        float minX = ps.Min(o => o.X), maxX = ps.Max(o => o.X);
                        float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);

                        //rendInfo = new RenderUnitInfoAdv(this) { ClientRect = new RectangleF(minX, minY, maxX - minX, Math.Max(2, (maxY - minY))) };
                        rendInfo = new RenderUnitInfoAdv(this) { ClientRect = new RectangleF(minX, 0, maxX - minX, this.Height) };
                        rendInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                        rendInfo.Points = ps;

                        drawPointLines(_graphic, ps, 0);
                        LastPoint = ps[ps.Count - 1];
                        LastBaseX = lastPx;
                    }
                }
                return rendInfo;
            }
        }

        public RenderUnitInfoAdv RenderRpt(List<PointF> points, float gain, float speed, out bool arrivedMax,
            int samplingRate = SysConstants.SAMPLE_RATE, float pixScale = SysConstants.PIX_SCALE, bool isBrowse = false)
        {
            lock (_lockObj)
            {
                arrivedMax = false;
                RenderUnitInfoAdv rendInfo = null;
                float scale = 1f;
                float xGain = 1000.0F * (1.0F / speed);// ms/mm
                if (points != null && points.Count > 0)
                {
                    List<PointF> ps = new List<PointF>();
                    float x = 0;
                    float y = 0;
                    float gap = speed / samplingRate; //数据点之间的水平间距mm

                    float lastPx = x;
                    for (int i = 0; i < points.Count; i++)
                    {
                        float offset = i * gap * pixScale;
                        lastPx = lastPx + gap * pixScale;
                        //x = LastPoint.X + scale * points[i].X * gain * pixScale / 1000F + offset;
                        x = lastPx + scale * points[i].X * gain * pixScale / 1000F;
                        if (x < 0 || x > Width) continue;
                        if (lastPx >= Width)
                        {
                            arrivedMax = true;
                            LastPoint = PointF.Empty;
                            LastBaseX = 0;
                            if (!isBrowse)
                            {
                                ps.Clear();
                                _graphic.Clear(Color.Transparent);
                            }
                            break;
                        }

                        y = scale * points[i].Y * gain * pixScale / 1000;
                        ps.Add(new PointF(x, BaseLineY - y));
                    }

                    if (ps.Count > 0)
                    {
                        float minX = ps.Min(o => o.X), maxX = ps.Max(o => o.X);
                        float minY = ps.Min(o => o.Y), maxY = ps.Max(o => o.Y);

                        //rendInfo = new RenderUnitInfoAdv(this) { ClientRect = new RectangleF(minX, minY, maxX - minX, Math.Max(2, (maxY - minY))) };
                        rendInfo = new RenderUnitInfoAdv(this) { ClientRect = new RectangleF(minX, 0, maxX - minX, this.Height) };
                        rendInfo.PointRect = new RectangleF(minX, minY, ps[ps.Count - 1].X - ps[0].X, maxY - minY);
                        rendInfo.Points = ps;

                        drawPointLines(_graphic, ps, 0);
                    }
                }
                return rendInfo;
            }
        }

        public void ClearRender()
        {
            _graphic.Clear(Color.Transparent);
        }

        private void drawPointLines(Graphics g, List<PointF> points, float offsetX)
        {
            if (points.Count > 1)
            {
                //PointF startP = points[0].Offset(-offsetX, 0);
                //for (int i = 1; i < points.Count; i++)
                //{
                //    PointF endP = points[i].Offset(-offsetX, 0);
                //    g.DrawLine(Pens.LimeGreen, startP.X, startP.Y, endP.X, endP.Y);
                //    startP = endP;
                //}
                using (Pen pen = new Pen(Color.LimeGreen, 1))
                {
                    g.DrawLines(pen, points.ToArray());
                }
            }
        }

        public static Bitmap DrawGridLines(int allW, int allH, Color gridColor, out List<GridMark> xMarkList, out List<GridMark> yMarkList)
        {
            Bitmap img = new Bitmap(allW, allH);
            int idx = 0;
            float x = 0, y = 0; //横坐标, 纵坐标
            xMarkList = new List<GridMark>();
            yMarkList = new List<GridMark>();

            using (Graphics g = Graphics.FromImage(img))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (Pen dashPen = new Pen(gridColor, 0.5f), solidPen = new Pen(gridColor))
                {
                    dashPen.DashPattern = new float[] { 1f, SysConstants.PIX_SCALE - 1f, 1f, SysConstants.PIX_SCALE - 1f };

                    //画竖线
                    do
                    {
                        if (idx % 5 == 0)
                        {
                            g.DrawLine(solidPen, x, 0, x, allH);
                            xMarkList.Add(new GridMark(x, false) { Index = xMarkList.Count });
                            idx = 0;
                        }
                        else
                        {
                            g.DrawLine(dashPen, x, 0, x, allH);
                            xMarkList.Add(new GridMark(x, true) { Index = xMarkList.Count });
                        }
                        x += SysConstants.PIX_SCALE * 5;
                        idx++;
                    } while (x < allW);

                    //画横线
                    idx = 0;
                    do
                    {
                        if (idx % 5 == 0)
                        {
                            g.DrawLine(solidPen, 0, y, allW, y);
                            yMarkList.Add(new GridMark(y, false) { Index = yMarkList.Count });
                            idx = 0;
                        }
                        else
                        {
                            g.DrawLine(dashPen, 0, y, allW, y);
                            yMarkList.Add(new GridMark(y, true) { Index = yMarkList.Count });
                        }
                        y += SysConstants.PIX_SCALE * 5;
                        idx++;
                    } while (y < allH);
                }
            }

            return img;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                //清理托管资源
                if (this._graphic != null)
                {
                    this._graphic.Dispose();
                    this._graphic = null;
                }
                if (this.Image != null)
                {
                    this.Image.Dispose();
                    this.Image = null;
                }
                if (this.BaseGridImage != null)
                {
                    this.BaseGridImage.Dispose();
                    this.BaseGridImage = null;
                }
            }

            //清理非托管资源
            //...
        }

        ~EcgCanvasAdv()
        {
            Dispose(false);
        }
    }

    public class GraphicsInfoAdv
    {
        public Lead TheLead { get; set; }  //当前导联
        public int LeadIndex { get; set; }
        public Rectangle Rect { get; set; } //绘图区域
        public Label LeadNameLabel { get; set; } //导联名称label
        public EcgCanvasAdv Canvas { get; set; }
        public RenderUnitInfoAdv CurrentRenderInfo { get; set; } //当前要渲染的信息
        //public List<PointD> DataList { get; set; } //最近收到的数据点
    }

    public class RenderUnitInfoAdv : IDisposable
    {
        /// <summary>
        /// 需要刷新的工作区区域
        /// </summary>
        public RectangleF ClientRect { get; set; }

        /// <summary>
        /// 描点的工作区区域
        /// </summary>
        public RectangleF PointRect { get; set; }

        /// <summary>
        /// 当前波段描点绘线后所在区域的图像
        /// </summary>
        public Image PointImage { get; set; }

        /// <summary>
        /// 相关数据点集合
        /// </summary>
        public List<PointF> Points { get; set; } = new List<PointF>();

        /// <summary>
        /// 导联绘制区域对象
        /// </summary>
        public EcgCanvasAdv BaseCanvas { get; set; }

        /// <summary>
        /// 导联状态(是否脱落 true:已脱落; false:正常)
        /// </summary>
        public bool LeadStatus { get; set; }

        /// <summary>
        /// 绘制的相关导联波形的颜色
        /// </summary>
        public Color LeadColor { get; set; } = Color.LimeGreen;

        public RenderUnitInfoAdv(EcgCanvasAdv canvas)
        {
            BaseCanvas = canvas;
        }

        /// <summary>
        /// 重置绘图区域及相关数据点集合
        /// </summary>
        public void Reset()
        {
            ClientRect = Rectangle.Empty;
            PointRect = Rectangle.Empty;
            Points.Clear();

            PointImage.Dispose();
            PointImage = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); //通知垃圾回收器不用再调用终结器
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                //清理托管资源
                if (this.PointImage != null)
                {
                    this.PointImage.Dispose();
                    this.PointImage = null;
                }
            }

            //清理非托管资源
            //...
        }

        ~RenderUnitInfoAdv()
        {
            Dispose(false);
        }
    }
}
