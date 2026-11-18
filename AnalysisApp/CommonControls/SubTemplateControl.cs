using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnalysisApp.Commons;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Zoncare.Common;
using System.IO;

namespace AnalysisApp.CommonControls
{
    public partial class SubTemplateControl : BaseDrawingControl
    {
        public string BeatType = "N";
        
        public BeatGroup beatGroup = new BeatGroup();
        private List<KeyValuePair<string, List<BeatInfo>>> beatGroupDataList = new List<KeyValuePair<string, List<BeatInfo>>>();
        private DrawRectangleWave drawRectangleWave = null;
        // 右键菜单
        private ContextMenuStrip contextMenuStrip;

        private int width { get; set; } = 0;            //绘图宽度
        private int height { get; set; } = 0;          //绘图高度度

        private int rows { get; set; } = 10;              //行数
        private int cols { get; set; } = 1;              //列数
        private float rectWidth { get; set; } = 60f;         //每个矩形区域的宽度
        private float rectHeight { get; set; } = 60f;        //每个矩形区域的高度
        private int rectPadding { get; set; } = 3;      //每个矩形之间的间隙

        private int Pages { get; set; } = 0;                      //总页数
        public int PageIdx { get; set; } = 0;                    //当前页索引，从0开始
        public int PageWaveCount { get; set; } = 0;

        // 鼠标滚动防抖动相关变量
        private int pendingScrollPages;
        private DateTime _lastWheelTime = DateTime.MinValue;


        // 滚动条防抖动相关变量
        private int pendingScrollValue;
        private DateTime _lastScrollTime = DateTime.MinValue;

        private List<List<float[]>> PageSubTemplateEcgData = new List<List<float[]>>(); //当前页面所有子模板数据 每个子列表代表一个子模板数据
        public List<int> PageSubTemplateLeadIdxs = new List<int>();
        private List<int> PageSubTemplateStartSamples = new List<int>();
        private List<int> PageSubTemplateSamples = new List<int>();
        private List<SKRect> PageSubTemplateRects { get; set; } = new List<SKRect>();
        private List<bool> PageSubTemplateSelectStatus { get; set; } = new List<bool>();
        private List<int> PageHeartBeatCounts = new List<int>();
        private List<int> PageHeartBeatIdxs = new List<int>();

        private float Speed = 25f;
        private float Gain = 5.0f;
        private float PixMM = 3.777f;

        public int CurrentHeartBeatIdx = 0;
        public int ClickRectIdx = 0;

        private int MaxPointCount = 0;

        //public event Action<List<BeatInfo>, int> OnChangeSubTemplate;
        public event Action OnChangeSubTemplate;


        public SubTemplateControl()
        {
            InitializeComponent();

            
        }

        private void SubTemplateControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                initControl();

                if (AnaValues.CurrentCase == null) return;
                reSetLead();

                initData();
            }
        }

        public void reSetLead()
        {
            PageSubTemplateLeadIdxs.Clear();
            PageSubTemplateLeadIdxs.Add(AnaValues.holterConfig.MainChannel);
        }

        private void initData()
        {
            PageSubTemplateStartSamples.Clear();
            PageSubTemplateSamples.Clear();
            PageSubTemplateEcgData.Clear();
            PageSubTemplateRects.Clear();
            PageSubTemplateSelectStatus.Clear();
            PageHeartBeatIdxs.Clear();
            PageHeartBeatCounts.Clear();

            if (PageIdx == -1)
            {
                return;
            }
            Dictionary<string, List<BeatInfo>> beatGroupData = beatGroup.Data;
            beatGroupDataList = beatGroupData.ToList();

            Pages = beatGroupDataList.Count % (rows * cols) != 0 ? (beatGroupDataList.Count / (rows * cols) + 1) : beatGroupDataList.Count / (rows * cols);
            if (PageIdx == Pages)
            {
                PageIdx = Math.Max(0, Pages - 1);
            }
            UpdateScrollBarMax();
            PageWaveCount = rows * cols;
            for (int i = PageIdx * PageWaveCount; i < (PageIdx + 1) * PageWaveCount; i++)
            {
                if (i >= beatGroupDataList.Count)
                {
                    break;
                }
                if((beatGroupDataList[i].Value).Count == 0)
                {
                    continue;
                }

                int currentBeatIdx = 0;
                if(i == ClickRectIdx)
                {
                    currentBeatIdx = CurrentHeartBeatIdx;
                }
                
                KeyValuePair<string, List<BeatInfo>> beatGroupDataTemp = beatGroupDataList[i];
                List<BeatInfo> HeartBeatIdxs = beatGroupDataTemp.Value;
                PageHeartBeatCounts.Add(HeartBeatIdxs.Count);
                BeatInfo beatInfo = HeartBeatIdxs[currentBeatIdx];
                PageHeartBeatIdxs.Add(currentBeatIdx + 1);
                BeatInfo result = AnaValues.CachedAllBeats.FirstOrDefault(b => b.Sample == beatInfo.Sample);
                int sample = beatInfo.Sample;
                int startSample = Math.Max(0, Convert.ToInt32(sample - 0.4 * AnaValues.holterConfig.SampleRate));
                int endSample = Math.Min(Convert.ToInt32(sample + 0.4 * AnaValues.holterConfig.SampleRate), AnaValues.holterConfig.Total-1);
                List<WaveInfo> EcgData = AnaCode.ReadAllWaveData(PageSubTemplateLeadIdxs, startSample, endSample);
                if (EcgData == null || EcgData.Count == 0)
                {
                    return;
                }

                List<float[]> waveData = EcgData.Select(o => o.WaveDataF).ToList();
                SKRect waveRect = new SKRect(rectPadding, rectPadding, rectWidth, rectHeight);
                PageSubTemplateRects.Add(waveRect);
                if (i == ClickRectIdx)
                {
                    PageSubTemplateSelectStatus.Add(true);
                }
                else
                {
                    PageSubTemplateSelectStatus.Add(false);
                }

                PageSubTemplateStartSamples.Add(startSample);
                PageSubTemplateSamples.Add(sample);
                PageSubTemplateEcgData.Add(waveData);
            }
            MaxPointCount = Convert.ToInt32(AnaValues.holterConfig.SampleRate * 0.8);
        }

        private void initControl()
        {

            _graphicsControl.MouseDown += OnCanvasMouseDown;
            _graphicsControl.MouseWheel += OnCanvasMouseWheel;

            panel1.Controls.Add(_graphicsControl);

            // 初始化右键菜单
            InitializeContextMenu();

            drawRectangleWave = new DrawRectangleWave();

        }

        // 更新滚动条的最大值
        private void UpdateScrollBarMax()
        {
            if (Pages > 1)
            {
                vScrollBar1.Show();
            }
            else
            {
                vScrollBar1.Hide();
            }

            vScrollBar1.Maximum = Math.Max(0, Pages - 1);
            if (Pages > 10)
            {
                vScrollBar1.LargeChange = Pages / 10;
            }
            else
            {
                vScrollBar1.LargeChange = 1;
            }

        }


        // 滚动条防抖动计时器事件处理
        private void ScrollBarScroll()
        {
            // 更新时间偏移
            PageIdx = pendingScrollValue;

            UpdateDataForceWaveformRedraw();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (DateTime.Now - _lastScrollTime < TimeSpan.FromMilliseconds(20)) return;

            // 根据滚动事件的类型处理
            if (e.Type == ScrollEventType.SmallIncrement) // 向下箭头
            {
                pendingScrollValue = Math.Min(++PageIdx, vScrollBar1.Maximum);
            }
            else if (e.Type == ScrollEventType.SmallDecrement) // 向上箭头
            {
                pendingScrollValue = Math.Max(--PageIdx, 0);
            }
            else if (e.Type == ScrollEventType.LargeDecrement) // 点击滚动条上方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = vScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.Y - 17) * 1.0f / vScrollBar1.Height) * vScrollBar1.Maximum);
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.LargeIncrement) // 点击滚动条下方空白区域
            {
                // 获取当前鼠标位置
                Point mousePos = Control.MousePosition;
                Point scrollBarPos = vScrollBar1.PointToScreen(Point.Empty);
                Point relativeMousePos = new Point(mousePos.X - scrollBarPos.X, mousePos.Y - scrollBarPos.Y);
                e.NewValue = Convert.ToInt32(((relativeMousePos.Y + 17) * 1.0f / vScrollBar1.Height) * vScrollBar1.Maximum);
                pendingScrollValue = e.NewValue;
            }
            else if (e.Type == ScrollEventType.ThumbTrack || e.Type == ScrollEventType.ThumbPosition)
            {
                // 拖动滑块时，直接设置待处理值为滑块位置
                pendingScrollValue = Math.Max(0, Math.Min(e.NewValue, vScrollBar1.Maximum));
            }
            ScrollBarScroll();
            _lastScrollTime = DateTime.Now;


            KeyValuePair<string, List<BeatInfo>> beatGroupDataTemp = beatGroupDataList[PageIdx * PageWaveCount];
            List<BeatInfo> HeartBeatIdxs = beatGroupDataTemp.Value;
            OnChangeSubTemplate?.Invoke();

            //OnChangeSubTemplate?.Invoke(HeartBeatIdxs, PageIdx * PageWaveCount);

        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            rectWidth = (imageWidth - rectPadding * (cols + 1)) / cols;
            rectHeight = (imageHeight - rectPadding * (rows + 1)) / rows;

            // 清除画布为白色背景
            canvas.Clear(SKColors.White);
            
            drawRectangleWave.WaveType = RectangleWaveType.SubTemplate;

            for (int i = 0; i < PageSubTemplateEcgData.Count; i++)
            {
                int points = PageSubTemplateEcgData[i][PageSubTemplateLeadIdxs[0]].Length;

                int rowIdx = i / cols;
                int colIdx = i % cols;
                PageSubTemplateRects[i] = new SKRect(colIdx * rectWidth + (colIdx + 1) * rectPadding, rowIdx * rectHeight + (rowIdx + 1) * rectPadding, (colIdx + 1) * rectWidth + (colIdx + 1) * rectPadding, (rowIdx + 1) * rectHeight + (rowIdx + 1) * rectPadding);
                drawRectangleWave.WaveRect = PageSubTemplateRects[i];
                drawRectangleWave.DrawRect(canvas, PageSubTemplateSelectStatus[i]);
                drawRectangleWave.WaveLeftMargin = (float)(rectWidth - (points * 1.0f / AnaValues.holterConfig.SampleRate) * Speed * PixMM) / 2;
                drawRectangleWave.WaveRightMargin = (float)(rectWidth - (points * 1.0f / AnaValues.holterConfig.SampleRate) * Speed * PixMM) / 2;
                drawRectangleWave.Speed = Speed;
                drawRectangleWave.Gain = Gain;
                drawRectangleWave.EcgData = PageSubTemplateEcgData[i];
                drawRectangleWave.leadIdxs = PageSubTemplateLeadIdxs;
                drawRectangleWave.StartSample = PageSubTemplateStartSamples[i];
                drawRectangleWave.Sample = PageSubTemplateSamples[i];
                drawRectangleWave.PageHeartBeatCount = PageHeartBeatCounts[i];
                drawRectangleWave.PageHeartBeatIdx = PageHeartBeatIdxs[i];
                drawRectangleWave.DrawEcgWave(canvas);
            }

        }

        // 重新加载数据及强制重绘波形
        public void UpdateDataForceWaveformRedraw()
        {
            initData();
            // 重绘控件
            _graphicsControl?.Invalidate();
        }

        public void ForceWaveformRedraw()
        {
            // 重绘控件
            _graphicsControl?.Invalidate();
        }

        private int FindRectIdx(float x, float y)
        {
            int rectIdx = -1;
            for (int i = 0; i < PageSubTemplateRects.Count; i++)
            {
                SKRect rect = PageSubTemplateRects[i];
                if ((x > rect.Left && x < rect.Right) && (y > rect.Top && y < rect.Bottom))
                {
                    rectIdx = i;
                    break;
                }
            }
            return rectIdx;
        }


        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ClickRectIdx = FindRectIdx(e.X, e.Y);
                if (ClickRectIdx >= 0)
                {
                    KeyValuePair<string, List<BeatInfo>> beatGroupDataTemp = beatGroupDataList[ClickRectIdx + PageIdx * PageWaveCount];
                    List<BeatInfo> HeartBeatIdxs = beatGroupDataTemp.Value;
                    
                    CurrentHeartBeatIdx = 0;


                    // 检查是否按住Ctrl键
                    bool isCtrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                    // 检查是否按住Shift键
                    bool isShiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

                    if (isCtrlPressed)
                    {
                        // Ctrl+点击：切换选择状态（多选）
                        PageSubTemplateSelectStatus[ClickRectIdx] = !PageSubTemplateSelectStatus[ClickRectIdx];
                    }
                    else
                    {
                        // 普通点击：清除所有选择，只选择当前项
                        for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
                        {
                            PageSubTemplateSelectStatus[i] = false;
                        }
                        PageSubTemplateSelectStatus[ClickRectIdx] = true;
                        OnChangeSubTemplate?.Invoke();

                        //OnChangeSubTemplate?.Invoke(HeartBeatIdxs, ClickRectIdx);
                    }


                    _graphicsControl?.Invalidate();



                    //UpdateDataForceWaveformRedraw();

                }
            }
        }

        private void OnCanvasMouseWheel(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - _lastWheelTime < TimeSpan.FromMilliseconds(20)) return;

            if (Pages < 2)
            {
                return;
            }
            // 计算滚动的秒数（根据滚动方向和当前显示行数）
            int scrollPages = e.Delta > 0 ? -1 : 1;

            // 累积待处理的滚动秒数
            pendingScrollPages += scrollPages;

            if (pendingScrollPages != 0)
            {
                // 更新时间偏移
                PageIdx += pendingScrollPages;
                PageIdx = Math.Max(0, Math.Min(PageIdx, vScrollBar1.Maximum));
                // 更新滚动条位置
                vScrollBar1.Value = Convert.ToInt32(PageIdx);


                UpdateDataForceWaveformRedraw();

                // 重置待处理滚动页数
                pendingScrollPages = 0;

                KeyValuePair<string, List<BeatInfo>> beatGroupDataTemp = beatGroupDataList[PageIdx * PageWaveCount];
                List<BeatInfo> HeartBeatIdxs = beatGroupDataTemp.Value;
                ClickRectIdx = 0;
                OnChangeSubTemplate?.Invoke();

                //OnChangeSubTemplate?.Invoke(HeartBeatIdxs, PageIdx * PageWaveCount);
            }
            _lastWheelTime = DateTime.Now;

        }

        private void InitializeContextMenu()
        {
            contextMenuStrip = new ContextMenuStrip();

            // 添加合并模板菜单项
            ToolStripMenuItem mergeTemplateMenuItem = new ToolStripMenuItem(CommonValues.Langs["合并模板"]);
            mergeTemplateMenuItem.Click += MergeTemplateMenuItem_Click;
            contextMenuStrip.Items.Add(mergeTemplateMenuItem);

            // 添加合并模板菜单项
            ToolStripMenuItem overlayWaveMenuItem = new ToolStripMenuItem(CommonValues.Langs["叠加图"]);
            overlayWaveMenuItem.Click += OverlayWaveMenuItem_Click;
            contextMenuStrip.Items.Add(overlayWaveMenuItem);

            _graphicsControl.ContextMenuStrip = contextMenuStrip;
        }

        private void MergeTemplateMenuItem_Click(object sender, EventArgs e)
        {
            int maxCount = 0;
            List<int> fromShapes = new List<int>();
            short toShape = 0;
            int shapeUnuse = 0;
            for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
            {
                if (PageSubTemplateSelectStatus[i] == true)
                {
                    string shapeStr = beatGroupDataList[i + PageIdx * PageWaveCount].Key;
                    int shape = Convert.ToInt32(shapeStr);
                    if(shape < 0 )
                    {
                        fromShapes.AddRange(QueryBeatGroup.smallSubType);
                    }
                    else
                    {
                        fromShapes.Add(shape);
                        List<BeatInfo> beats = beatGroupDataList[i + PageIdx * PageWaveCount].Value;
                        int beatCount = beats.Count;
                        if (beatCount > maxCount)
                        {
                            short.TryParse(shapeStr, out toShape);
                            shapeUnuse = Convert.ToInt32(shapeStr);
                            maxCount = beatCount;
                        }
                    }

                }
            }
            fromShapes = fromShapes.Where(s => s != shapeUnuse).ToList();

            char beatType;
            char.TryParse(BeatType, out beatType);

            AEcgHandler.MergeTemplates(fromShapes.ToArray(), toShape, beatType);
            beatGroup = QueryBeatGroup.beatGroupByType(BeatType);
            Dictionary<string, List<BeatInfo>> beatGroupData = beatGroup.Data;
            beatGroupDataList = beatGroupData.ToList();
            string toShapeStr = toShape.ToString();
            int index = beatGroupDataList.FindIndex(kvp => kvp.Key == toShapeStr);
            if (index != -1)
            {
                PageIdx = index / PageWaveCount;
                ClickRectIdx = index%PageWaveCount;
                UpdateDataForceWaveformRedraw();

                KeyValuePair<string, List<BeatInfo>> beatGroupDataTemp = beatGroupDataList[index];
                List<BeatInfo> HeartBeatIdxs = beatGroupDataTemp.Value;
                OnChangeSubTemplate?.Invoke();

                //OnChangeSubTemplate?.Invoke(HeartBeatIdxs, index);
            }


        }

        private void OverlayWaveMenuItem_Click(object sender, EventArgs e)
        {
            List<BeatInfo> beats = new List<BeatInfo>();
            for (int i = 0; i < PageSubTemplateSelectStatus.Count; i++)
            {
                if (PageSubTemplateSelectStatus[i] == true)
                {
                    beats.AddRange(beatGroupDataList[i + PageIdx * PageWaveCount].Value);
                }
            }

            TemplateOverlayForm.Instance.Show(beats, beats[0].BeatUiType, this.Parent.Parent, this.FindForm());
        }

        public void OnSampleIdxChanged(int sampleIdx)
        {
            CurrentHeartBeatIdx = sampleIdx;
            UpdateDataForceWaveformRedraw();
        }

        public void OnLeadChanged(List<int> leadIdxs, int sampleIdx)
        {
            PageSubTemplateLeadIdxs = leadIdxs;
            CurrentHeartBeatIdx = sampleIdx;
            UpdateDataForceWaveformRedraw();
        }

        public void OnGainChanged(float gain, int sampleIdx)
        {
            Gain = gain;
            CurrentHeartBeatIdx = sampleIdx;
            UpdateDataForceWaveformRedraw();
        }
    }
}
