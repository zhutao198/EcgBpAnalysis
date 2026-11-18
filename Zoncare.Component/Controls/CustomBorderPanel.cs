using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    /// <summary>
    /// 带自绘边框的容器控件，支持直角/圆角。
    ///
    /// 结构与绘制约定：
    ///     CustomBorderPanel   —— 只负责在最外圈绘制边框
    ///       └── ContentPanel  —— Dock = Fill，承载全部内容子控件，自动内缩 BorderSize
    /// </summary>
    public class CustomBorderPanel : Panel
    {
        private readonly Panel _content = new Panel();

        private Color _borderColor = SystemColors.InactiveCaption;
        private int _borderSize = 1;
        private bool _useRoundStyle;
        private int _cornerRadius = 3;

        #region 属性

        /// <summary>
        /// 内容层（Dock = Fill，落点已内缩 BorderSize）
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel ContentPanel
        {
            get { return _content; }
        }

        /// <summary>边框颜色。</summary>
        [Category("CustomBorderPanel")]
        [DefaultValue(typeof(Color), "InactiveCaption")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (_borderColor == value) return;

                _borderColor = value;
                Invalidate();
            }
        }

        /// <summary>边框线宽（像素）。同时决定内容层相对客户区的内缩量。</summary>
        [Category("CustomBorderPanel")]
        [DefaultValue(1)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public int BorderSize
        {
            get { return _borderSize; }
            set
            {
                if (value < 0) value = 0;
                if (_borderSize == value) return;

                _borderSize = value;
                PerformLayout();    // 内容层要跟着重新内缩
                UpdateContentRegion();   // 半径随 BorderSize 变化
                Invalidate();
            }
        }

        /// <summary>true 画圆角边框，false 画直角边框。</summary>
        [Category("CustomBorderPanel")]
        [DefaultValue(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public bool UseRoundStyle
        {
            get { return _useRoundStyle; }
            set
            {
                if (_useRoundStyle == value) return;

                _useRoundStyle = value;
                UpdateContentRegion();
                Invalidate();
            }
        }

        /// <summary>圆角半径（像素），仅 UseRoundStyle = true 时生效。</summary>
        [Category("CustomBorderPanel")]
        [DefaultValue(3)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                if (value < 0) value = 0;
                if (_cornerRadius == value) return;

                _cornerRadius = value;
                UpdateContentRegion();
                Invalidate();
            }
        }
        #endregion

        public CustomBorderPanel()
        {
            // 原生边框属于非客户区，与本控件的自绘边框无关，这里禁用以避免叠加
            this.BorderStyle = BorderStyle.None;
            this.SetStyle(ControlStyles.UserPaint
                        | ControlStyles.AllPaintingInWmPaint
                        | ControlStyles.OptimizedDoubleBuffer, true);

            base.Padding = new Padding(_borderSize);

            // 设计期不挂内容层：设计器看到的结构必须与 .Designer.cs 完全一致，
            // 否则 VS 保存窗体时会把塞进内容层的子控件整段丢掉。
            if (!IsInDesignMode())
            {
                _content.Name = "ContentPanel";
                _content.BorderStyle = BorderStyle.None;
                _content.TabIndex = 0;
                _content.Dock = DockStyle.Fill;   // 落点由 DisplayRectangle 决定（内缩 BorderSize）
                _content.SizeChanged += ContentSizeChanged;

                this.Controls.Add(_content);
                UpdateContentRegion();
            }
        }

        /// <summary>
        /// 子控件的可用区域：从客户区四边各内缩 BorderSize，忽略 Padding。
        /// </summary>
        public override Rectangle DisplayRectangle
        {
            get
            {
                Rectangle cr = ClientRectangle;
                int b = _borderSize;

                return new Rectangle(
                    cr.X + b,
                    cr.Y + b,
                    Math.Max(0, cr.Width - b * 2),
                    Math.Max(0, cr.Height - b * 2));
            }
        }

        /// <summary>
        /// 把内容层裁成与边框内沿同心的圆角矩形。
        /// </summary>
        private void UpdateContentRegion()
        {
            if (!ReferenceEquals(_content.Parent, this)) return;

            Region old = _content.Region;
            int radius = _cornerRadius - _borderSize;

            if (!_useRoundStyle || _borderSize <= 0 || radius <= 0
                || _content.Width <= 0 || _content.Height <= 0)
            {
                _content.Region = null;
            }
            else
            {
                RectangleF rect = new RectangleF(0, 0, _content.Width, _content.Height);

                using (GraphicsPath path = GraphicsPathHelper.GetRoundedRectanglePath(rect, radius))
                {
                    _content.Region = new Region(path);
                }
            }

            //  释放 GDI+ Region对象
            if (old != null) old.Dispose();
        }

        private void ContentSizeChanged(object sender, EventArgs e)
        {
            UpdateContentRegion();
        }

        /// <summary>
        /// 设计期判定。LicenseManager.UsageMode 在构造函数阶段（InitializeComponent 反序列化时）
        /// 就已经可靠，Site.DesignMode 作为兜底。
        /// </summary>
        private bool IsInDesignMode()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;

            ISite site = this.Site;
            while (site != null)
            {
                if (site.DesignMode) return true;
                site = site.Container as ISite;
            }

            return false;
        }

        /// <summary>
        /// 运行期把直接挂到本控件上的子控件转交到内容层
        /// </summary>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e.Control == null) return;
            if (ReferenceEquals(e.Control, _content)) return;
            if (IsInDesignMode()) return;
            if (!ReferenceEquals(e.Control.Parent, this)) return;

            // 必须在挂进内容层之前补 Top：内容层的尺寸会从 Panel 默认的 200×100 缩到实际
            // 内容区（例如 275×26），Anchor 里没有 Top/Bottom 的控件会被布局引擎按
            // "保持相对父容器垂直中心的偏移"重新算 Y，直接算出负数。
            EnsureVerticalAnchor(e.Control);

            _content.Controls.Add(e.Control);

#if DEBUG
            if (_content.Width > 0 && !_content.ClientRectangle.IntersectsWith(e.Control.Bounds))
            {
                System.Diagnostics.Debug.WriteLine(string.Format(
                    "[CustomBorderPanel] 子控件 \"{0}\" 的边界 {1} 完全落在内容区 {2} 之外，运行时将不可见。",
                    e.Control.Name, e.Control.Bounds, _content.ClientRectangle));
            }

            HookNegativeTopDiagnostics(e.Control);
#endif
        }

        /// <summary>
        /// 内容层是 Dock = Fill，它的尺寸会从 Panel 的默认 200×100 变成实际内容区
        /// （例如 275×26，delta = +77 / −72）。子控件的 Anchor 若既没有 Top 也没有 Bottom
        /// （典型是只勾了 Left|Right 想横向拉伸），WinForms 会保持"相对父容器垂直中心的偏移"：
        /// 给这类子控件补一个 Top，把 Y 钉在原地。完全 Anchor.None（有意居中）的不动。
        /// </summary>
        private static void EnsureVerticalAnchor(Control child)
        {
            AnchorStyles anchor = child.Anchor;
            if ((anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) != 0) return;
            if ((anchor & (AnchorStyles.Left | AnchorStyles.Right)) == 0) return;

            child.Anchor = anchor | AnchorStyles.Top;

#if DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format(
                "[CustomBorderPanel] 子控件 \"{0}\" 的 Anchor 缺少 Top/Bottom，已自动补 Top 固定垂直位置。",
                child.Name));
#endif
        }

#if DEBUG
        /// <summary>
        /// 诊断用（仅 Debug）：子控件的 Top 一旦变负就会"凭空消失"，
        /// 但本控件从不修改子控件坐标，所以这里挂一个监听，
        /// 把时间点、父级链和**完整调用栈**打到输出窗口，用来确认到底是"谁"改的。
        /// </summary>
        private void HookNegativeTopDiagnostics(Control child)
        {
            child.LocationChanged += ChildLocationChangedDiagnostics;

            if (child.Top < 0) ReportNegativeTop(child);
        }

        private void ChildLocationChangedDiagnostics(object sender, EventArgs e)
        {
            Control c = sender as Control;
            if (c == null || c.Top >= 0) return;

            ReportNegativeTop(c);
        }

        private static void ReportNegativeTop(Control c)
        {
            string parent = c.Parent == null
                ? "<null>"
                : c.Parent.GetType().Name + " \"" + c.Parent.Name + "\"";

            System.Diagnostics.Debug.WriteLine(
                "[CustomBorderPanel] 子控件 \"" + c.Name + "\" 的 Top = " + c.Top
                + "（Bounds=" + c.Bounds + "，父级=" + parent + "），会被内容区裁掉。调用栈：\n"
                + Environment.StackTrace);
        }
#endif

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_borderSize <= 0 || Width <= 0 || Height <= 0) return;

            Graphics g = e.Graphics;
            SmoothingMode oldSmoothingMode = g.SmoothingMode;
            PixelOffsetMode oldPixelOffsetMode = g.PixelOffsetMode;

            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // GDI+ 的 Pen 以路径线为「中心线」向两侧各铺 BorderSize / 2，
                // 所以路径要整体向内缩半个笔宽，边框外沿才正好压在客户区边缘上。
                float half = _borderSize / 2f;
                float w = Width - _borderSize;
                float h = Height - _borderSize;

                if (w <= 0 || h <= 0) return;

                using (Pen borderPen = new Pen(_borderColor, _borderSize))
                {
                    if (_useRoundStyle)
                    {
                        using (GraphicsPath path = GraphicsPathHelper.GetRoundedRectanglePath(
                                   new RectangleF(half, half, w, h), _cornerRadius))
                        {
                            g.DrawPath(borderPen, path);
                        }
                    }
                    else
                    {
                        g.DrawRectangle(borderPen, half, half, w, h);
                    }
                }
            }
            finally
            {
                g.SmoothingMode = oldSmoothingMode;
                g.PixelOffsetMode = oldPixelOffsetMode;
            }
        }
    }
}
