using AnalysisApp.Commons;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component.Controls;

namespace AnalysisApp.CommonControls
{
    public partial class BaseDrawingControl : UserControl
    {
        protected HolterConfig _holterConfig;
        protected Control _graphicsControl;
        protected SKGLControl _skglControl = null;
        protected SKControl _skControl = null;
        protected Dictionary<char, SKColor> _beatColorMap;
        protected List<BeatInfo> _selectedBeats;
        protected SKSurface offscreenSurface;
        protected SKImage offscreenImage;
        protected PersistDictionary<string> _langs = CommonValues.Langs;
        protected readonly object _surfaceLock = new object(); // 用于同步surface访问的锁对象

        protected Size _firstActualSize;
        private bool _hasLayoutCompleted = false;

        [Browsable(false), DefaultValue(false), Description("当前控件是否已加载")]
        public bool IsLoaded { get; private set; }

        [Browsable(false), DefaultValue(false), Description("是否已改变数据源")]
        public bool DataSourceIsChanged { get; set; }

        /// <summary>
        /// 当控件中的数据源或其元素改变时发生, 主要用于通知前一(源)控件的刷新。
        /// </summary>
        public virtual event Action<object> OnDataChanged;

        public BaseDrawingControl()
        {
            InitializeComponent();

            this.Layout += (s, e) =>
            {
                if (!_hasLayoutCompleted && this.IsHandleCreated && this.Visible)
                {
                    _firstActualSize = this.Size;
                    _hasLayoutCompleted = true;
                    this.Layout -= null; // 移除事件处理程序
                }
            };
        }

        private void BaseDrawingControl_Load(object sender, EventArgs e)
        {
            Init();
            IsLoaded = true;
        }

        private void graphicsControl_KeyPress(object sender, KeyPressEventArgs e)
        {

            OnControlKeyPressed(sender, e);
        }

        private void graphicsControl_MouseDown(object sender, MouseEventArgs e)
        {
            _graphicsControl?.Focus();
        }

        protected virtual int OnControlKeyPressed(object sender, KeyPressEventArgs e)
        {
            char beatType = char.ToUpper(e.KeyChar);
            switch (beatType)
            {
                case '0':
                    beatType = 'X';
                    break;
                case '1':
                    beatType = 'V';
                    break;
                case '2':
                    beatType = 'S';
                    break;
                case '3':
                    beatType = 'N';
                    break;
                case '4':
                    beatType = 'P';
                    break;
                case '5':
                    beatType = 'O';
                    break;
                case '6':
                    beatType = 'H';
                    break;
            }
            int[] samples = _selectedBeats?.Select(o => o.Sample).ToArray();

            if (beatType == 'D' || beatType == '.')
            {
                return DeleteBeats(samples);
            }

            if (!AnaValues.BeatTypes.ContainsKey(beatType)) return -1;

            return ModifyBeatType(samples, beatType);
        }

        protected virtual void OnControlKeyDown(Keys keyData)
        {
            return;
        }

        protected virtual void OnMixKeyDown(Keys keyData)
        {
            return;
        }

        protected void OnSoftWarePaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            lock (_surfaceLock)
            {
                if (e.Surface == null || e.Surface.Canvas == null) return;

                OnPaintSurface(e.Surface.Canvas, e.Info, e.Info.Width, e.Info.Height);
            }
        }

        protected void OnGLPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            lock (_surfaceLock)
            {
                if (e.Surface == null || e.Surface.Canvas == null) return; // 防御性检查

                OnPaintSurface(e.Surface.Canvas, e.Info, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
            }
        }

        protected virtual void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {

        }


        public void Init()
        {
            _holterConfig = AnaValues.holterConfig;
            _beatColorMap = AnaCode.ConvertToSkColor(AnaValues.WaveformColorMap);

            if (!IsLoaded)
            {
                if (DesignMode) return;
                if (CommonValues.IsGpuAvailable)
                {
                    try
                    {
                        _skglControl = new SafeSKGLControl
                        {
                            Dock = DockStyle.Fill,
                            Name = "skglControl1",
                            VSync = false
                        };
                        _skglControl.PaintSurface += OnGLPaintSurface;
                        _skglControl.KeyPress += graphicsControl_KeyPress;
                        _skglControl.MouseDown += graphicsControl_MouseDown;
                        // GL 上下文创建失败时降级为软件渲染（方案B），避免异常冒泡成弹窗
                        // （事件定义在派生类 SafeSKGLControl 上，需按派生类型订阅）
                        ((SafeSKGLControl)_skglControl).ContextCreationFailed += OnGlContextCreationFailed;
                        _graphicsControl = _skglControl;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("切换到硬件渲染失败: " + ex.Message);
                        useSoftwareRender();
                    }
                }
                else
                {
                    useSoftwareRender();
                }
            }
        }

        private void useSoftwareRender()
        {
            _skControl = new SKControl()
            {
                Dock = DockStyle.Fill,
                Name = "skControl1",
            };

            _skControl.PaintSurface += OnSoftWarePaintSurface;
            _skControl.KeyPress += graphicsControl_KeyPress;
            _skControl.MouseDown += graphicsControl_MouseDown;
            _graphicsControl = _skControl;
        }

        private bool _glFallbackTriggered = false;

        /// <summary>
        /// GL 上下文创建失败（句柄重建时 wglCreateContext 失败等）的降级处理：
        /// 延迟到当前消息处理结束后，把该控件原位替换为软件渲染的 SKControl。
        /// 延迟是必要的：异常发生在 WinForms 句柄创建递归中，此时修改控件树不安全。
        /// </summary>
        private void OnGlContextCreationFailed(object sender, EventArgs e)
        {
            if (_glFallbackTriggered) return;
            _glFallbackTriggered = true;

            // 全局降级：后续新建的绘图控件直接走软件渲染，避免继续触发失败
            CommonValues.IsGpuAvailable = false;

            try
            {
                this.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        ReplaceGlWithSoftwareRender();
                        Console.WriteLine($"GL 上下文创建失败，已降级为软件渲染: {this.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"降级软件渲染失败: {ex.GetType().Name}: {ex.Message}");
                    }
                }));
            }
            catch (InvalidOperationException)
            {
                // 无句柄无法 BeginInvoke（极少数时序），直接同步替换
                ReplaceGlWithSoftwareRender();
            }
        }

        private void ReplaceGlWithSoftwareRender()
        {
            var old = _skglControl;
            _skglControl = null;
            if (old is SafeSKGLControl safeOld)
            {
                safeOld.ContextCreationFailed -= OnGlContextCreationFailed;
                old.PaintSurface -= OnGLPaintSurface;
                old.KeyPress -= graphicsControl_KeyPress;
                old.MouseDown -= graphicsControl_MouseDown;
            }

            useSoftwareRender();

            // 在原父容器、原 Z 顺序位置上替换，尽量保持原布局
            if (_skControl != null && old != null)
            {
                Control parent = old.Parent;
                if (parent != null && !parent.IsDisposed)
                {
                    int index = parent.Controls.Contains(old) ? parent.Controls.GetChildIndex(old) : -1;
                    if (parent.Controls.Contains(old)) parent.Controls.Remove(old);
                    parent.Controls.Add(_skControl);
                    if (index >= 0) parent.Controls.SetChildIndex(_skControl, index);
                    _skControl.Invalidate();
                }
            }

            old?.Dispose();
        }

        //创建离屏缓存
        protected void createOffscreenSurface(Func<bool> checkFunction = null, int? imageWidth = null, int? imageHeight = null)
        {
            if (checkFunction != null && checkFunction() == false) return;

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
                if (_graphicsControl == null || _graphicsControl.IsDisposed || _graphicsControl.Width <= 0 || _graphicsControl.Height <= 0)
                {
                    return;
                }

                // 创建新的surface
                int canvasWidth = imageWidth ?? _graphicsControl.Width;
                int canvasHeight = imageHeight ?? _graphicsControl.Height;
                SKImageInfo imgInfo = new SKImageInfo(canvasWidth, canvasHeight);
                SKSurface surface = SKSurface.Create(imgInfo);
                if (surface != null)
                {
                    drawOffscreenImage(surface.Canvas, canvasWidth, canvasHeight);

                    // 强制等待 GPU 完成渲染
                    surface.Canvas.Flush();

                    // 保存surface引用和创建快照
                    offscreenSurface = surface;
                    offscreenImage = offscreenSurface.Snapshot();
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出，避免影响UI线程
                System.Diagnostics.Debug.WriteLine($"CreateOffscreenSurface错误: {ex.GetType().Name}: {ex.Message}");

                // 在异常情况下清理所有资源
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

        //绘制离屏缓存图像
        protected virtual void drawOffscreenImage(SKCanvas canvas, int imageWidth, int imageHeight)
        {

        }

        protected void fillLeadComboBox(ComboBox cbx)
        {
            cbx.ValueMember = "Value";
            cbx.DisplayMember = "Text";

            List<ListItem> items = new List<ListItem>();
            string leads = SysConstants.ECG_LT_STANDARD[1];
            if (_holterConfig.Lead == 3) leads = SysConstants.ECG_LT_STANDARD[5];
            string[] arrLead = leads.Split(',');
            for (int i = 0; i < arrLead.Length; i++)
            {
                items.Add(new ListItem(i, arrLead[i]));
            };
            cbx.DataSource = items;
            cbx.SelectedValue = _holterConfig.MainChannel;
        }

        protected virtual int AddBeats(int[] samples, char beatUiType)
        {
            int ret = -1;
            if (samples != null && samples.Length > 0)
            {
                ret = AEcgHandler.AddBeats(samples, beatUiType);
                if (ret == 0)
                {
                    ChangeDataSource(samples);
                }
            }
            return ret;
        }

        protected virtual int ModifyBeatType(int[] samples, char beatUiType)
        {
            int ret = -1;
            if (samples != null && samples.Length > 0)
            {
                ret = AEcgHandler.ModifyBeats(samples, beatUiType);
                if (ret == 0)
                {
                    if (beatUiType == 'F')
                    {
                        AnaEvent.AddNcaOrFBeat(samples, 280, "室性融合波");
                    }
                    ChangeDataSource(samples);
                }
            }
            return ret;
        }

        protected virtual int DeleteBeats(int[] samples)
        {
            int ret = -1;
            if (samples != null && samples.Length > 0)
            {
                ret = AEcgHandler.DeleteBeats(samples);
                if (ret == 0)
                {
                    ChangeDataSource(samples);
                }
            }
            return ret;
        }


        protected virtual void ChangeDataSource(object source)
        {
            if (this is DrawEcgCommonWave)
            {
                //if (AEcgHandler.UpdateType != EcgUpdateType.AddBeats 
                //    && AEcgHandler.UpdateType != EcgUpdateType.ModifyBeats 
                //    && AEcgHandler.UpdateType !=  EcgUpdateType.DeleteBeats) DataSourceIsChanged = true;
                DataSourceIsChanged = false;
            }
            else if (this is BeatWaveList)
            {
                DataSourceIsChanged = false;
            }

            OnDataChanged?.Invoke(source);

            Control container = CommonFuns.GetSpecifiedParent(this, typeof(BaseUserControl));
            if (container != null)
            {
                (container as BaseUserControl).DataSourceTracked = true;
            }
            DataSourceIsChanged = false;
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_graphicsControl != null)
            {
                if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right
                    || keyData == Keys.F1 || keyData == Keys.F2 || keyData == Keys.F3)
                {
                    _graphicsControl.Focus();
                    OnControlKeyDown(keyData);
                    return true; // 返回 true 表示已处理，不再向下传递
                }
                if (keyData == (Keys.Control | Keys.N) || keyData == (Keys.Control | Keys.V) || keyData == (Keys.Control | Keys.S) || keyData == (Keys.Control | Keys.X))
                {
                    _graphicsControl.Focus();
                    OnMixKeyDown(keyData);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();

                // 移除事件处理程序以避免内存泄漏
                if (CommonValues.IsGpuAvailable && _skglControl is SafeSKGLControl safeGl)
                {
                    safeGl.PaintSurface -= OnGLPaintSurface;
                    safeGl.ContextCreationFailed -= OnGlContextCreationFailed;
                }

                if (_skControl != null)
                {
                    _skControl.PaintSurface -= OnSoftWarePaintSurface;
                }
            }
            base.Dispose(disposing);
        }
    }

}
