using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    public partial class SafeSKGLControl : SKGLControl
    {
        public bool IsGpuAccelerated { get; private set; }
        private bool _isDisposed = false;
        private readonly object _renderLock = new object();

        /// <summary>
        /// GL 上下文创建失败时触发（典型场景：句柄销毁重建时 wglCreateContext 失败）。
        /// 宿主可订阅此事件降级为软件渲染，避免异常向 WinForms 句柄创建递归中冒泡。
        /// </summary>
        public event EventHandler ContextCreationFailed;

        // SKGLControl 内部持有与 GL 上下文绑定的 Skia 资源（私有字段）。
        // 句柄销毁重建后这些资源不会自动失效，若被复用到新上下文上会导致
        // GPU 资源孤儿累积和 AccessViolationException，必须在旧上下文销毁前一并释放。
        private static readonly FieldInfo s_surfaceField = typeof(SKGLControl).GetField("surface", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo s_canvasField = typeof(SKGLControl).GetField("canvas", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo s_renderTargetField = typeof(SKGLControl).GetField("renderTarget", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo s_grContextField = typeof(SKGLControl).GetField("grContext", BindingFlags.Instance | BindingFlags.NonPublic);

        public SafeSKGLControl()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            try
            {
                base.OnHandleCreated(e);
            }
            catch (Exception ex)
            {
                // GL 上下文创建失败（如 "Context creation failed. Wgl.CreateContext() error"）。
                // 拦截异常，避免其沿 WinForms 的 CreateControl 递归冒泡成未处理异常弹窗；
                // 由宿主（BaseDrawingControl）决定是否降级为软件渲染。
                Console.WriteLine($"SafeSKGLControl GL 上下文创建失败: {ex.GetType().Name}: {ex.Message}");
                IsGpuAccelerated = false;
                RaiseContextCreationFailed();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // 在 WGL 上下文被 OpenTK 销毁之前，释放与旧上下文绑定的 Skia GPU 资源。
            // 下一次 OnPaint 会基于新上下文重新创建 grContext/renderTarget/surface。
            ReleaseSkiaResources();
            base.OnHandleDestroyed(e);
        }

        private void RaiseContextCreationFailed()
        {
            try
            {
                ContextCreationFailed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理 ContextCreationFailed 事件出错: {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// 通过反射释放 SKGLControl 内部与当前 GL 上下文绑定的 Skia 资源。
        /// 若反射不可用（SkiaSharp 版本字段名变更），静默退回原行为。
        /// </summary>
        private void ReleaseSkiaResources()
        {
            lock (_renderLock)
            {
                try
                {
                    if (s_grContextField == null || s_renderTargetField == null || s_surfaceField == null)
                        return;

                    // 尽量让 GL 上下文成为当前上下文，保证 grContext 的释放调用有效；
                    // 句柄/上下文已失效时忽略。
                    try { MakeCurrent(); } catch { }

                    // surface（其 Canvas 归 surface 所有，一并置空）
                    object surface = s_surfaceField.GetValue(this);
                    if (surface is IDisposable dSurface)
                    {
                        dSurface.Dispose();
                        s_surfaceField.SetValue(this, null);
                    }
                    s_canvasField?.SetValue(this, null);

                    // renderTarget
                    object renderTarget = s_renderTargetField.GetValue(this);
                    if (renderTarget is IDisposable dRenderTarget)
                    {
                        dRenderTarget.Dispose();
                        s_renderTargetField.SetValue(this, null);
                    }

                    // grContext
                    object grContext = s_grContextField.GetValue(this);
                    if (grContext is IDisposable dGrContext)
                    {
                        dGrContext.Dispose();
                        s_grContextField.SetValue(this, null);
                    }

                    IsGpuAccelerated = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"释放 Skia GL 资源失败: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (_isDisposed) return;

            lock (_renderLock)
            {
                try
                {
                    if (this.IsHandleCreated && !this.IsDisposed && this.Width > 0 && this.Height > 0)
                    {
                        base.OnPaint(pe);
                        IsGpuAccelerated = GRContext != null;
                    }
                }
                catch (AccessViolationException avex)
                {
                    // 内存访问冲突，可能是GPU资源已释放或损坏
                    System.Diagnostics.Debug.WriteLine($"GPU内存访问冲突: {avex.Message}");

                    // 尝试重置GPU上下文
                    try
                    {
                        if (GRContext != null && !GRContext.IsAbandoned)
                        {
                            GRContext.ResetContext();
                        }
                    }
                    catch (Exception resetEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"重置GPU上下文失败: {resetEx.Message}");
                    }

                    // 不重新抛出异常，避免程序崩溃
                }
                catch (ObjectDisposedException)
                {
                    // 对象已释放，这是正常情况
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnPaint 错误: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
