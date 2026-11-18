using AnalysisApp.Commons;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.DAL;

namespace AnalysisApp.CommonControls
{
    public partial class BpTrendWaveControl : BaseDrawingControl
    {
        private float PixMM = 3.777f;

        private float PaddingLeft = 0;
        private float PaddingTop = 0;
        private float PaddingRight = 0;
        private float PaddingBottom = 0;

        public bpDiagnoseSettingValues settingValues;

        DrawBpTrendWave drawBpTrendWave = null;

        public int CaseId;

        [DefaultValue(false), Category("自定义属性"), Description("是否用来比较"), Browsable(true)]
        public bool IsCompare { get; set; } = false;

        public BpTrendWaveControl()
        {
            InitializeComponent();

            PaddingLeft = 10 * PixMM;
            PaddingTop = 6 * PixMM;
            PaddingRight = 15 * PixMM;
            PaddingBottom = 5 * PixMM;

            drawBpTrendWave = new DrawBpTrendWave();
        }

        private void DrawBpTrendWave_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                initControl();
            }
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            try
            {
                // 检查是否是部分重绘
                bool isPartialRedraw = imageWidth < _graphicsControl.Width || imageWidth < _graphicsControl.Height;
                if (!isPartialRedraw)
                {
                    canvas.Clear(SKColors.White);

                    if (offscreenSurface == null || offscreenImage == null) { createOffscreenSurface(); }
                    if (offscreenImage != null) { canvas.DrawImage(offscreenImage, 0, 0); }
                }

                //绘制光标所在位置的血压信息
                SKRect renderRect = drawBpTrendWave.GetCursorAssociatedTipArea(drawBpTrendWave.MapValuePoints, out int dataIndex);
                drawBpTrendWave.DrawNibpTips(canvas, renderRect, dataIndex);
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("GPU内存访问冲突，已安全处理 - 主绘制循环");
                createOffscreenSurface();
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("绘制对象已释放，已安全处理");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPaintSurface错误: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            int mouseX = e.X;
            int mouseY = e.Y;

            drawBpTrendWave.MouseX = mouseX;
            drawBpTrendWave.MouseY = mouseY;

            //if (drawBpTrendWave.MapValuePoints != null)
            //{
            //    SKRect renderRect = drawBpTrendWave.GetCursorAssociatedTipArea(drawBpTrendWave.MapValuePoints, out int dataIndex);
            //    Rectangle invalidateRect = new Rectangle((int)Math.Floor(renderRect.Left), (int)Math.Floor(renderRect.Top), (int)Math.Ceiling(renderRect.Width), (int)Math.Ceiling(renderRect.Height));
            //    _graphicsControl?.Invalidate(invalidateRect);
            //}
            _graphicsControl?.Invalidate();
        }


        private void initControl()
        {
            this.checkItemControl1.initCtl(Color.LightBlue, "收缩压/舒张压");
            this.checkItemControl1.OnItemChecked += (d) => Item1Clicked(d);

            this.checkItemControl2.initCtl(Color.Purple, "脉率");
            this.checkItemControl2.OnItemChecked += (d) => Item2Clicked(d);

            this.checkItemControl3.initCtl(Color.Yellow, "平均动脉压");
            this.checkItemControl3.OnItemChecked += (d) => Item3Clicked(d);

            this.checkItemControl4.initCtl(Color.DarkGreen, "RPP");
            this.checkItemControl4.OnItemChecked += (d) => Item4Clicked(d);

            this.checkItemControl5.initCtl(Color.DarkRed, "错误数据");
            this.checkItemControl5.OnItemChecked += (d) => Item5Clicked(d);

            _graphicsControl.MouseMove += OnCanvasMouseMove;

            panel1.Controls.Add(_graphicsControl);
        }

        private void Item1Clicked(bool bFlag)
        {
            BPTrendFlag.bBp = bFlag;
            RefreshWave();
        }

        private void Item2Clicked(bool bFlag)
        {
            BPTrendFlag.bPulse = bFlag;
            RefreshWave();
        }

        private void Item3Clicked(bool bFlag)
        {
            BPTrendFlag.bAvgBp = bFlag;
            RefreshWave();
        }

        private void Item4Clicked(bool bFlag)
        {
            BPTrendFlag.bRpp = bFlag;
            RefreshWave();
        }

        private void Item5Clicked(bool bFlag)
        {
            BPTrendFlag.bError = bFlag;
            UpdateDataRefresh();
        }

        public void UpdateDataRefresh()
        {
            initData();
            RefreshWave();
        }

        public void RefreshWave()
        {
            createOffscreenSurface();
            _graphicsControl?.Invalidate();
        }

        private void initData()
        {
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            List<BPDataInfo> BPDataInfoList = new List<BPDataInfo>();

            if(IsCompare == false)
            {
                if (BPTrendFlag.bError)
                {
                    BPDataInfoList = CalcBPInfo.GetBPDataList(2);
                }
                else
                {
                    BPDataInfoList = CalcBPInfo.GetBPDataList(1);
                }
            }
            else
            {
                List<bpDetail> bpDetailList = ZoncareEntities.Instance.BpDetails.AsNoTracking().Where(o => o.caseId == CaseId).ToList();
                bpDetailList = bpDetailList.FindAll(b => (b.errorCode == null || b.errorCode.Trim() == "0"));
                if (BPTrendFlag.bError)
                {
                    bpDetailList = bpDetailList.FindAll(b => b.dataStatus == 1);
                }
                else
                {
                    bpDetailList = bpDetailList.FindAll(
                    b => ((b.sysPressure / 1000 > settingValues.minErrorSys && b.sysPressure / 1000 < settingValues.maxErrorSys) &&
                    (b.diaPressure / 1000 > settingValues.minErrorDia && b.diaPressure / 1000 < settingValues.maxErrorDia) &&
                    (b.dataStatus == 1)));
                }
                BPDataInfoList = CalcBPInfo.CalcBPDataInfo(bpDetailList);
            }

            drawBpTrendWave.settingValues = settingValues;
            drawBpTrendWave.BPDataInfoList = BPDataInfoList;
        }

        protected override void drawOffscreenImage(SKCanvas canvas, int imageWidth, int imageHeight)
        {
            drawBpTrendWave.WaveLeft = PaddingLeft;
            drawBpTrendWave.WaveTop = PaddingTop;
            drawBpTrendWave.WaveWidth = imageWidth - PaddingLeft - PaddingRight;
            drawBpTrendWave.WaveHeight = imageHeight - PaddingTop - PaddingBottom;

            drawBpTrendWave.DrawNightRect(canvas);
            drawBpTrendWave.DrawTrendWave(canvas);
            drawBpTrendWave.DrawMaxMinLine(canvas);
            drawBpTrendWave.DrawCoordinate(canvas);

        }

    }
}
