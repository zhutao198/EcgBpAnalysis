using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp
{
    public partial class PageScanControl : BaseUserControl
    {
        public PageScanControl()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            List<AfInfo> afs = AnaCode.GetAfList();
            // 建立三个控件之间的完整连接
            if (pageScanWave1 != null && hrTrendWaveControl1 != null && drawEcgCommonWave1 != null)
            {
                hrTrendWaveControl1.afs = afs;
                pageScanWave1.afs = afs;
                LoadData(resetUI);
            }
        }

        private void PageScanControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsUiInitialized) InitUI();

                Console.WriteLine("页扫描界面已加载");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "页扫描");
            }
        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                // 建立三个控件之间的完整连接
                if (pageScanWave1 != null && hrTrendWaveControl1 != null && drawEcgCommonWave1 != null)
                {
                    hrTrendWaveControl1.OnHRTrendStartTimeChanged += pageScanWave1.OnHRTrendWaveTimeChanged;
                    hrTrendWaveControl1.OnHRTrendStartTimeChanged += drawEcgCommonWave1.OnPageScanStartTimeChanged;

                    pageScanWave1.OnPageScanClickSampleChanged += drawEcgCommonWave1.OnWaveformStartTimeChanged;
                    pageScanWave1.OnPageScanClickSampleChanged += hrTrendWaveControl1.OnPageScanStartTimeChanged;
                    pageScanWave1.OnPageScanLeadChanged += drawEcgCommonWave1.OnPageScanLeadChanged;
                    pageScanWave1.OnCommonWaveShowChanged += OnCommonWaveShowChanged;

                    drawEcgCommonWave1.OnEcgCommonWaveClickChanged += pageScanWave1.OnEcgCommonWaveClickChanged;
                    drawEcgCommonWave1.OnEcgCommonWaveStartTimeChanged += pageScanWave1.OnWaveformStartTimeChanged;
                    drawEcgCommonWave1.OnEcgCommonWaveStartTimeChanged += hrTrendWaveControl1.OnEcgCommonWaveStartTimeChanged;

                    hrTrendWaveControl1.OnDataChanged += (d) => { RefreshUI(); };
                    drawEcgCommonWave1.OnDataChanged += (d) => { RefreshUI(); };
                    pageScanWave1.OnDataChanged += (d) => { RefreshUI(); };

                    //LoadData(true);
                    Console.WriteLine("页扫描界面已初始化完成");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "页扫描");
            }
        }

        private void LoadData(bool resetUI = false)
        {
            if (drawEcgCommonWave1.IsLoaded)
            {
                drawEcgCommonWave1.RefreshData(resetUI);
            }
            if (resetUI)
            {
                this.pageScanWave1.ResetUIState(resetUI);
            }

            hrTrendWaveControl1.RefreshData();
            pageScanWave1.RefreshData();
        }

        private async void RefreshUI()
        {
            while (AnaCode.IsCalculatingHR)
            {
                await Task.Delay(10);
            }
            List<AfInfo> afs = AnaCode.GetAfList();
            hrTrendWaveControl1.afs = afs;
            pageScanWave1.afs = afs;
            hrTrendWaveControl1.ForceWaveformRedraw();
            drawEcgCommonWave1.forceWaveformRedraw();
            pageScanWave1.ForceWaveformRedraw();
        }

        private void OnCommonWaveShowChanged(bool bShow)
        {
            if (bShow)
            {
                this.drawEcgCommonWave1.Show();
                this.tableLayoutPanel1.RowStyles[2].Height = 35;
            }
            else
            {
                this.drawEcgCommonWave1.Hide();
                this.tableLayoutPanel1.RowStyles[2].Height = 0;
            }
        }

        //private void HRRefreshUI()
        //{
        //    List<AfInfo> afs = AnaCode.GetAfList();
        //    hrTrendWaveControl1.afs = afs;
        //    hrTrendWaveControl1.ForceWaveformRedraw();
        //    drawEcgCommonWave1.forceWaveformRedraw();
        //}

        //private async void EcgRefreshUI()
        //{
        //    while (AnaCode.IsCalculatingHR)
        //    {
        //        await Task.Delay(10);
        //    }
        //    hrTrendWaveControl1.ForceWaveformRedraw();
        //    drawEcgCommonWave1.forceWaveformRedraw();
        //    pageScanWave1.ForceWaveformRedraw();
        //}

        //private async void ScanRefreshUI()
        //{
        //    while (AnaEvent.IsCalculating)
        //    {
        //        await Task.Delay(10);
        //    }
        //    drawEcgCommonWave1.forceWaveformRedraw();
        //    pageScanWave1.ForceWaveformRedraw();
        //}

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (pageScanWave1 != null)
            {
                pageScanWave1.OnKeyDown(sender, e);
            }
            //if(drawEcgCommonWave1 != null)
            //{
            //    drawEcgCommonWave1.OnKeyDown(sender, e);
            //}
        }
    }
}
