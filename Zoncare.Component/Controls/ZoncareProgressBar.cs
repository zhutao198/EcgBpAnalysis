using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    /// <summary>
    /// 可自绘的进度条控件。默认使用自绘圆角风格；
    /// 把 Style 设为 ProgressBarStyle.Marquee 时，底部的轨道与进度保持不动，
    /// 表面有一道两端渐隐的光带自左向右扫过，只在当前进度的范围内可见；
    /// 扫动速度随进度自适应——进度越低扫得越慢。
    /// </summary>
    public partial class ZoncareProgressBar : ProgressBar
    {
        #region 常量与系统消息
        /// <summary>PBM_SETMARQUEE：开/关系统进度条自身的滚动动画</summary>
        private const int PBM_SETMARQUEE = 0x040A;

        /// <summary>预警阈值（与自绘模式保持一致）</summary>
        private const int WarningThreshold = 90;

        /// <summary>光带扫动的帧间隔（毫秒）</summary>
        private const int MarqueeFrameInterval = 50;

        /// <summary>
        /// 速度随 Value 自适应下调时的最低比例。满进度时为 1（最快），进度降到 0 时降到这个值。
        /// 取 0.45 表示最慢也只有满速的一半左右：属于"适当减慢"，
        /// 而不会像按 Value 等比那样在低进度下慢到几乎不动。
        /// </summary>
        private const double MarqueeMinSpeedRatio = 0.3;

        /// <summary>满进度（Value = Maximum）时每帧的基准位移量占控件宽度的比例</summary>
        private const double MarqueeBaseStepRatio = 1d / 30d;

        /// <summary>
        /// 进度区域至少占控件宽度的 1/4，才值得显示扫光。
        /// 进度太窄时行程太短，光带会在极短时间里反复循环，看起来像频闪。
        /// 用比例而不是固定像素，这样不同宽度的控件表现一致。
        /// </summary>
        private const int MinProgressRatioForLight = 4;

        /// <summary>光带亮度：半透明白，叠在轨道或进度填充上都能看出亮光。想更明显就调高 alpha</summary>
        private static readonly Color MarqueeLightColor = Color.FromArgb(170, 255, 255, 255);

        /// <summary>
        /// 光带两端的透明色。alpha = 0 时 RGB 必须也是白色：
        /// ColorBlend 对 RGBA 各通道分别插值，若用 Color.Transparent（0,0,0,0），渐变中会出现一圈灰边。
        /// </summary>
        private static readonly Color MarqueeLightFadeColor = Color.FromArgb(0, 255, 255, 255);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion


        private bool showWarningColor = false;

        /// <summary>扫光动画的定时器</summary>
        private readonly Timer marqueeTimer;

        /// <summary>
        /// 光带左边缘的当前位置。坐标是整个控件，可以超出当前进度区（超出部分被裁剪掉），
        /// 这样光带的行程与 Value 无关，速度只按 Value 平滑变化、不会因行程缩短而突变。
        /// </summary>
        private int lightX;

        [DefaultValue(false), Category("自定义属性"), Description("是否显示预警颜色"), Browsable(true)]
        public bool ShowWarningColor
        {
            get { return this.showWarningColor; }
            set
            {
                if (this.showWarningColor == value) return;
                this.showWarningColor = value;
            }
        }

        public ZoncareProgressBar()
        {
            InitializeComponent();

            //原有自绘方式；双缓冲用于抹平扫光动画的闪烁
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);

            //扫光动画定时器：由 components 托管，控件释放时一并释放
            this.marqueeTimer = new Timer();
            this.marqueeTimer.Interval = MarqueeFrameInterval;
            this.marqueeTimer.Tick += this.MarqueeTimer_Tick;
            if (components != null)
            {
                components.Add(this.marqueeTimer);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            //句柄创建/重建时基类会按 Style 重新开启系统自带的滚动动画，
            //这里先停掉自绘动画，让它在下一次绘制时重新启动，从而重新关闭系统动画。
            this.marqueeTimer.Stop();
            UpdateMarqueeAnimation();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            UpdateMarqueeAnimation();

            if (IsMarqueeMode)
            {
                DrawMarqueeStyle(pe);
                return;
            }

            DrawCustomStyle(pe);
        }

        /// <summary>
        /// 自绘圆角风格
        /// </summary>
        private void DrawCustomStyle(PaintEventArgs pe)
        {
            Rectangle barRect = new Rectangle(0, 0, this.Width, this.Height);
            Graphics g = pe.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            //using (SolidBrush brush = new SolidBrush(this.BackColor))
            //{
            //    g.FillRectangle(brush, barRect);
            //}

            int diameter = Height - 1; //直径
            Color fillColor = Color.FromArgb(74, 122, 196);
            if (showWarningColor)
            {
                diameter = 4;
                if (this.Value >= WarningThreshold) fillColor = Color.Red;
            }
            else { }

            using (GraphicsPath path = GraphicsPathHelper.CreatePath(barRect, diameter, RoundStyle.All, 1))
            using (SolidBrush brush = new SolidBrush(SystemColors.ControlLight))
            {
                g.FillPath(brush, path);
            }

            if (this.Value > 0)
            {
                using (GraphicsPath path = CreateProgressPath(diameter))
                using (Brush fillBrush = new SolidBrush(fillColor))
                {
                    g.FillPath(fillBrush, path);
                }
            }
        }


        #region 滚动闪烁（光线扫过）自绘

        /// <summary>
        /// 自绘的滚动闪烁表现：底层沿用原有绘制（轨道 + 按 Value 的静态进度，完全不参与动画），
        /// 上层在当前进度的范围内叠加一道自左向右循环扫过的光带。
        /// </summary>
        private void DrawMarqueeStyle(PaintEventArgs pe)
        {
            DrawCustomStyle(pe);
            DrawMarqueeLight(pe);
        }

        /// <summary>
        /// 绘制扫过的光带。光带两端渐隐到透明，所以看上去是一道"光线"而不是一根实心色块。
        /// 光带整体裁剪在"当前进度"的圆角路径内，因此不会出现在 Value 覆盖的范围之外。
        /// </summary>
        private void DrawMarqueeLight(PaintEventArgs pe)
        {
            if (this.Width <= 0 || this.Height <= 0) return;
            if (!HasRoomForLight()) return;

            int lightWidth = GetLightWidth();
            Graphics g = pe.Graphics;
            Rectangle lightRect = new Rectangle(this.lightX, 0, lightWidth, this.Height);

            //预警时圆角收小，与 DrawCustomStyle 保持一致
            int diameter = this.showWarningColor ? 4 : this.Height - 1;

            using (GraphicsPath path = CreateProgressPath(diameter))
            {
                GraphicsState state = g.Save();
                try
                {
                    g.SetClip(path, CombineMode.Intersect);

                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        lightRect, MarqueeLightFadeColor, MarqueeLightFadeColor, LinearGradientMode.Horizontal))
                    {
                        ColorBlend blend = new ColorBlend(3);
                        blend.Colors = new[] { MarqueeLightFadeColor, MarqueeLightColor, MarqueeLightFadeColor };
                        blend.Positions = new[] { 0f, 0.5f, 1f };
                        brush.InterpolationColors = blend;

                        g.FillRectangle(brush, lightRect);
                    }
                }
                finally
                {
                    g.Restore(state);
                }
            }
        }

        /// <summary>
        /// 当前 Value 对应的进度填充区域路径。绘制填充和裁剪光带都用它，
        /// 保证两者的边界完全一致（含圆角样式）。
        /// </summary>
        private GraphicsPath CreateProgressPath(int diameter)
        {
            Rectangle fillRect = new Rectangle(0, 0, GetProgressWidth(), this.Height);

            RoundStyle roundStyle = RoundStyle.Left;
            if (this.Value == this.Maximum) roundStyle = RoundStyle.All;

            return GraphicsPathHelper.CreatePath(fillRect, diameter, roundStyle, 1);
        }

        /// <summary>当前 Value 对应的进度宽度</summary>
        private int GetProgressWidth()
        {
            return (int)(this.Width * (this.Value / (double)this.Maximum));
        }

        /// <summary>光带宽度：取当前进度宽度的 1/3，因此不会超出 Value 的范围</summary>
        private int GetLightWidth()
        {
            return (int)(GetProgressWidth() / 3d);
        }

        /// <summary>
        /// 当前进度区域是否够宽、值得显示扫光。
        /// 进度太小时干脆不显示，也不会启动动画。
        /// </summary>
        private bool HasRoomForLight()
        {
            //return this.Width > 0 && GetProgressWidth() * MinProgressRatioForLight >= this.Width;
            return this.Width > 0 && GetProgressWidth() >= 50;
        }

        /// <summary>
        /// 每帧的位移量。以控件宽度为基准，再按当前 Value 做自适应：
        /// 进度越低，光带扫得越慢（最低降到 MarqueeMinSpeedRatio 倍），
        /// 这样低进度下光带不会在一小段可见区域里显得急促。
        /// 配合"行程固定为整个控件宽度"（见 MarqueeTimer_Tick），
        /// 速度只随 Value 平滑变化，不会因为行程缩短而突然变快。
        /// </summary>
        private int GetLightStep()
        {
            double progress = this.Maximum > 0 ? this.Value / (double)this.Maximum : 0d;
            if (progress < 0d) progress = 0d;
            else if (progress > 1d) progress = 1d;

            //满进度时最快，进度降低时线性减慢到 MarqueeMinSpeedRatio
            double factor = MarqueeMinSpeedRatio + (1d - MarqueeMinSpeedRatio) * progress;

            return Math.Max(2, (int)Math.Round(this.Width * MarqueeBaseStepRatio * factor));
        }

        /// <summary>
        /// 维护扫光动画的启停。
        /// 之所以放在绘制入口里判断：Style 是基类的非虚属性，无法拦截它的 setter，
        /// 而修改 Style 一定会触发重绘，因此在 OnPaint 里同步既可靠又几乎不花代价。
        /// </summary>
        private void UpdateMarqueeAnimation()
        {
            //进度区域连一条光带都放不下时，动画没有意义，直接不跑
            bool shouldRun = IsMarqueeMode && IsHandleCreated && Visible && !IsDesignTime
                             && HasRoomForLight();
            if (!shouldRun)
            {
                this.marqueeTimer.Stop();
                return;
            }

            //已在运行就直接返回，这样下面"关闭系统动画"的动作只会在启动时执行一次
            if (this.marqueeTimer.Enabled) return;

            //自绘时系统自带的滚动动画只会带来无意义的无效重绘，启动前关掉它
            DisableSystemMarquee();

            //光带从左边界之外进入，形成完整的一轮扫动
            this.lightX = -GetLightWidth();
            this.marqueeTimer.Start();
        }

        /// <summary>
        /// 关闭系统进度条自身的滚动动画。PBM_SETMARQUEE 的 wParam 为 0 即 KillTimer。
        /// 只在自绘动画启动时调用一次；句柄重建后由 OnHandleCreated 触发重新调用。
        /// </summary>
        private void DisableSystemMarquee()
        {
            if (!IsHandleCreated) return;

            SendMessage(this.Handle, PBM_SETMARQUEE, IntPtr.Zero, IntPtr.Zero);
        }

        private void MarqueeTimer_Tick(object sender, EventArgs e)
        {
            this.lightX += GetLightStep();

            //光带只在 Value 范围内扫动
            if (this.lightX > GetProgressWidth())
            {
                this.lightX = -GetLightWidth();
            }

            Invalidate();
        }

        /// <summary>当前是否需要用自绘的方式表现滚动闪烁</summary>
        private bool IsMarqueeMode
        {
            get { return this.Style == ProgressBarStyle.Marquee; }
        }

        /// <summary>是否处于设计态（设计器里不跑动画）</summary>
        private bool IsDesignTime
        {
            get
            {
                if (this.DesignMode) return true;

                ISite site = this.Site;
                return site != null && site.DesignMode;
            }
        }

        #endregion
    }
}
