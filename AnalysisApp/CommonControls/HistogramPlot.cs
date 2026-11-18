using AnalysisApp.Commons;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class HistogramPlot : BaseDrawingControl
    {
        private List<BeatInfo> _sourceBeats;

        private SKPaint blockRectPaint;
        private SKPaint selectionTextPaint;
        private float selectStartX;
        private int selectStartIndex = -1;
        private List<int> selectIndices = new List<int>();

        private bool _isSelecting;
        private float _stepWidth;
        private SKRect _innerRect;
        private float _duration; // 绘图区数据长度(s)
        private const float COL_PROPORTION = 0.6F; //柱形条占其所属区间的宽度比例

        private int _leadIndex;
        private BeatInfo _sourceBeat;
        private List<KeyValuePair<float, List<BeatInfo>>> _dataGroups = null;

        public event Action<object, List<BeatInfo>, bool> OnBeatSelected;

        public HistogramInfo ChartInfo { get; set; }


        public HistogramPlot()
        {
            InitializeComponent();
        }

        #region 事件
        private void HistogramPlot_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                init();

                deriveDrawSize(_graphicsControl.Width, _graphicsControl.Height);
            }
        }

        private void HistogramPlot_SizeChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (_graphicsControl != null && this.FindForm().WindowState != FormWindowState.Minimized)
                {
                    if (this.Size != _firstActualSize)
                    {
                        deriveDrawSize(_graphicsControl.Width, _graphicsControl.Height);
                        RefreshUI(this);
                        _firstActualSize = this.Size;
                    }
                }
            }
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                selectStartX = e.X;

                if (_innerRect.Top <= e.Y && e.Y < _innerRect.Bottom)
                {
                    selectIndices.Clear();
                    selectStartIndex = findClosestGroupData(e.X);
                }
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;

            if (Math.Abs(e.X - selectStartX) < _stepWidth * COL_PROPORTION) return;

            if (selectStartIndex >= 0)
            {
                bool selectChanged = false;
                int selCount = selectIndices.Count;
                bool isMovingToRight = e.X > selectStartX;
                int curIndex = findClosestGroupData(e.X, selectStartIndex, isMovingToRight, true);
                if (curIndex >= 0 && !selectIndices.Contains(curIndex))
                {
                    var dataList = _dataGroups[curIndex].Value;
                    if (dataList != null && dataList.Count > 0)
                    {
                        selectIndices.Add(curIndex);
                        selCount++;
                        //补充中间漏选的矩形区域
                        if (selCount > 0)
                        {
                            int start = selectStartIndex;
                            int end = curIndex;
                            if (curIndex < start)
                            {
                                start = curIndex;
                                end = selectStartIndex;
                            }
                            start = Math.Max(0, start);
                            end = Math.Max(0, end);
                            for (int m = start; m <= end; m++)
                            {
                                if (!selectIndices.Contains(m))
                                {
                                    var dataList1 = _dataGroups[m].Value;
                                    if (dataList1 != null && dataList1.Count > 0) selectIndices.Add(m);
                                }
                            }
                        }

                        selectChanged = true;
                    }
                }


                if (selectChanged) _graphicsControl.Invalidate();
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;

                if (selectStartIndex >= 0)
                {
                    if (selectIndices.Count == 0)
                    {
                        var dataList = _dataGroups[selectStartIndex].Value;
                        if (dataList != null && dataList.Count > 0)
                            selectIndices.Add(selectStartIndex);
                    }
                    executeSelect(sender);
                }

                _graphicsControl.Invalidate();
            }
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            try
            {
                // 检查是否是部分重绘（仅重绘圈选路径区域）
                bool isPartialRedraw = imageWidth < _graphicsControl.Width || imageHeight < _graphicsControl.Height;
                if (!isPartialRedraw)
                {
                    canvas.Clear(SKColors.White);

                    // 检查离屏surface状态，必要时重新创建
                    if (offscreenSurface == null || offscreenImage == null)
                    {
                        createOffscreenSurface();
                    }
                    if (offscreenImage != null)
                    {
                        canvas.DrawImage(offscreenImage, 0, 0);
                    }
                }

                //drawDeviationLines(canvas);
                drawDataGroupBlocks(canvas);

            }
            catch (AccessViolationException)
            {
                // GPU内存访问冲突，记录但不抛出异常
                Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
                // 重新创建surface以恢复正常状态
                createOffscreenSurface();
            }
            catch (ObjectDisposedException)
            {
                // 对象已释放，记录但不抛出异常
                Console.WriteLine("绘制对象已释放，已安全处理");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPaintSurface错误: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private void mnuBeatDelete_Click(object sender, EventArgs e)
        {
            List<BeatInfo> selectedBeats = getSelectedBeats();
            if (selectedBeats != null && selectedBeats.Count > 0)
            {
                int[] samples = selectedBeats.Select(b => b.Sample).ToArray();
                int ret = AEcgHandler.DeleteBeats(samples);
                if (ret == 0)
                {
                    //UpdateBeatSource(BaseBeatType, null);
                    ChangeDataSource(selectedBeats);
                }
            }
        }

        #endregion


        public void UpdateDataSource(object sender, HistogramInfo chartInfo, List<KeyValuePair<float, List<BeatInfo>>> groups, bool keepSelection = false)
        {
            string originalType = "";
            if (ChartInfo != null) originalType = ChartInfo.TypeId;

            ChartInfo = chartInfo;
            _dataGroups = groups;
            if (_dataGroups != null)
            {
                _stepWidth = getStepWidth();
                if (keepSelection)
                {
                    if (ChartInfo.TypeId != originalType)
                    {
                        RefreshUI(sender,false);
                    }
                    else
                    {
                        RefreshUI(sender, true);
                    }
                }
                else { RefreshUI(sender, false); }
            }
        }

        private void init()
        {
            _dataGroups = new List<KeyValuePair<float, List<BeatInfo>>>();
            _sourceBeats = new List<BeatInfo>(AnaValues.CachedAllBeats.Count);

            initMenuItems();

            blockRectPaint = new SKPaint()
            {
                //IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = new SKColor(59, 149, 197)
            };
            selectionTextPaint = new SKPaint()
            {
                Color = SKColors.Black,
                IsAntialias = true
            };

            _graphicsControl.ContextMenuStrip = this.ctxMenuStrip;
            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseMove += OnCanvasMouseMove;
            _graphicsControl.MouseUp += OnCanvasMouseUp;
            this.Controls.Add(_graphicsControl);
        }

        private void initMenuItems()
        {
            List<char> types = AnaValues.BeatTypes.Keys.ToList();
            for (int i = 0; i < types.Count; i++)
            {
                char beatType = types[i];
                ToolStripMenuItem mnuItem = new ToolStripMenuItem($"({beatType})" + _langs[AnaValues.BeatTypes[beatType]]);
                mnuItem.Tag = beatType;
                mnuItem.Click += (o, e) =>
                {
                    List<BeatInfo> selectedBeats = getSelectedBeats();
                    if (selectedBeats != null && selectedBeats.Count > 0)
                    {
                        int[] samples = selectedBeats?.Select(b => b.Sample).ToArray();
                        ModifyBeatType(samples, beatType);
                    }
                };
                this.ctxMenuStrip.Items.Insert(i, mnuItem);
            }

            this.mnuBeatDelete.Text = "(D)" + _langs["删除心搏"];
            this.mnuBeatDelete.Click += mnuBeatDelete_Click;
        }

        protected override int OnControlKeyPressed(object sender, KeyPressEventArgs e)
        {
            //_selectedBeats = getSelectedBeats();
            //return base.OnControlKeyPressed(sender, e);
            return 0;
        }

        protected override int ModifyBeatType(int[] samples, char beatUiType)
        {
            int ret = base.ModifyBeatType(samples, beatUiType);
            if (ret == 0)
            {
                //UpdateBeatSource(BaseBeatType, null);
                ChangeDataSource(samples);
            }
            else
            {
                CommonFuns.PromptError("操作失败");
            }

            return ret;
        }

        public void RefreshUI(object sender, bool keepSelection = false)
        {
            if (DesignMode || !IsLoaded) return;

            if (!keepSelection)
            {
                selectStartX = 0;
                selectIndices.Clear();
                for (int i = 0; i < _dataGroups.Count; i++)
                {
                    if (_dataGroups[i].Value.Count > 0)
                    {
                        selectIndices.Add(i);
                        break;
                    }
                }
            }

            //重新创建离屏缓存
            createOffscreenSurface();

            _graphicsControl?.Invalidate();

            executeSelect(sender, keepSelection);
        }

        //创建离屏缓存
        private void createOffscreenSurface()
        {
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
                SKImageInfo imgInfo = new SKImageInfo(_graphicsControl.Width, _graphicsControl.Height);
                SKSurface surface = SKSurface.Create(imgInfo);
                if (surface != null)
                {
                    HistogramDrawer.DrawCoordinateSystem(surface.Canvas, ChartInfo, _innerRect);
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

        //绘制矩形区块
        private void drawDataGroupBlocks(SKCanvas canvas)
        {
            if (ChartInfo == null) return;

            float blockWidth = _stepWidth * COL_PROPORTION; ;
            float xPerVal = _innerRect.Width / ChartInfo.MaxScaleX, yPerVal = _innerRect.Height / ChartInfo.MaxScaleY;
            float x0 = _innerRect.Left, y0 = _innerRect.Bottom;
            float offset = x0;

            float textLeft = -1, textRight = x0; //心搏数量文本绘制区左右边缘位置
            float textY = y0;
            int selectedCount = 0;
            for (int i = 0; i < _dataGroups.Count; i++)
            {
                List<BeatInfo> beats = _dataGroups[i].Value;
                float x = offset + (_stepWidth - blockWidth) / 2;
                if (beats.Count > 0)
                {
                    float rectH = Math.Max(beats.Count * yPerVal, 3);
                    float y = y0 - rectH;
                    if (selectIndices.Contains(i))
                    {
                        blockRectPaint.Color = new SKColor(255, 176, 0);
                        if (textLeft < 0) textLeft = offset;
                        textRight = offset + _stepWidth;
                        selectedCount += beats.Count;
                        if (selectIndices.Count == 1) { textY = y - 3; }
                    }
                    else
                    {
                        blockRectPaint.Color = new SKColor(59, 149, 197);
                    }

                    canvas.DrawRect(x, y, blockWidth, rectH, blockRectPaint);
                }
                offset += _stepWidth;
            }

            //绘制选中心搏数量
            if (selectIndices.Count > 0 && selectedCount > 0)
            {
                if (selectIndices.Count > 1) { textY = _innerRect.Top - 3; }
                float textX = textLeft + (textRight - textLeft) / 2;

                using (SKFont textFont = new SKFont
                {
                    Size = 10.5f,
                    Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
                })
                {
                    canvas.DrawText(selectedCount.ToString(), textX, textY, SKTextAlign.Center, textFont, selectionTextPaint);
                }
            }
        }


        // 仅重绘圈选路径区域
        private void redrawSelectionPathOnly(SKRect selectionRect)
        {
            // 计算圈选路径的边界区域
            if (selectionRect != SKRect.Empty)
            {
                // 添加一些边距确保路径完全可见
                Rectangle rect = Rectangle.Truncate(RectangleF.Inflate(selectionRect.ToDrawingRect(), 3, 3));

                // 仅重绘圈选路径区域
                _graphicsControl.Invalidate(rect);
            }
            else
            {
                // 如果没有路径，重绘整个控件
                _graphicsControl.Invalidate();
            }
        }

        // 获取内部波形绘制区
        private void deriveDrawSize(int width, int height)
        {
            int marginTop = 35, marginBottom = 45;
            int marginLeft = 45, marginRight = 70;
            _innerRect = new SKRect(marginLeft, marginTop, width - marginRight, height - marginBottom);
        }

        private void executeSelect(object sender, bool keepSelection = false)
        {
            List<BeatInfo> selectedBeats = getSelectedBeats();
            OnBeatSelected?.Invoke(sender, selectedBeats, keepSelection);
        }

        /// <summary>
        /// 查找最近的直方图柱形区域
        /// </summary>
        /// <param name="cursorX">当前光标横坐标位置</param>
        /// <param name="baseIndex">开始查找的柱形区域索引</param>
        /// <param name="fromLeftToRight">是否从左到右查找</param>
        /// <param name="accurateCheck">是否严格检查区域边界</param>
        /// <returns></returns>
        private int findClosestGroupData(int cursorX, int baseIndex = 0, bool fromLeftToRight = true, bool accurateCheck = false)
        {
            int groupIndex = -1;
            float columnWidth = _stepWidth * COL_PROPORTION;

            baseIndex = baseIndex < 0 ? 0 : baseIndex;
            int start = baseIndex, end = _dataGroups.Count - 1;
            if (fromLeftToRight == false)
            {
                start = 0;
                end = baseIndex;
            }
            for (int i = start; i <= end; i++)
            {
                float colLeft = _innerRect.Left + _stepWidth * i;
                float colRight = _innerRect.Left + _stepWidth * (i + 1);
                if (accurateCheck)
                {
                    colLeft = colLeft + (_stepWidth - columnWidth) / 2;
                    colRight = colLeft + columnWidth;
                }
                if (colLeft <= cursorX && cursorX < colRight)
                {
                    groupIndex = i;
                    break;
                }
            }
            return groupIndex;
        }

        private List<BeatInfo> getSelectedBeats()
        {
            List<BeatInfo> selectedBeats = new List<BeatInfo>();
            foreach (int idx in selectIndices)
            {
                if (_dataGroups[idx].Value.Count > 0)
                    selectedBeats.AddRange(_dataGroups[idx].Value);
            }
            return selectedBeats;
        }

        //获取每个柱状图所属区间宽度
        private float getStepWidth()
        {
            float stepWidth = _innerRect.Width / 100;
            if (ChartInfo != null)
            {
                int cntX = (int)((double)ChartInfo.MaxScaleX / ChartInfo.IntervalX);
                //int step = (int)((double)ChartInfo.IntervalX / ChartInfo.SubdivisionCount);
                stepWidth = _innerRect.Width / cntX / ChartInfo.SubdivisionCount;
            }
            return stepWidth;
        }

    }
}
