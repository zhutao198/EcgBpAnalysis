using AnalysisApp.CommonControls;
using AnalysisApp.Commons;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;

namespace AnalysisApp
{
    public partial class SummaryPageControl : BaseUserControl
    {
        public SummaryPageControl()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            RefreshUI();
        }

        private void SummaryPageControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (!IsUiInitialized) InitUI();


                    //RefreshUI();

                    Console.WriteLine("汇总界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "汇总界面");
            }

        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "汇总界面");
            }
        }

        /// <summary>
        /// 清除并释放所有子控件
        /// </summary>
        public static void ClearAndDisposeControls(Control parent)
        {
            // 从后往前遍历，避免索引变化问题
            for (int i = parent.Controls.Count - 1; i >= 0; i--)
            {
                Control child = parent.Controls[i];
                parent.Controls.RemoveAt(i);  // 从集合中移除
                child.Dispose();              // 释放资源
            }
        }

        private void RefreshUI()
        {
            if (AnaValues.CurrentCase.checkItem == 1)
            {
                if (tableLayoutPanel1.HasChildren)
                {
                    ClearAndDisposeControls(tableLayoutPanel1);
                }

                EcgDataSummaryControl ecgCtl = new EcgDataSummaryControl();
                ecgCtl.initDataAsync();
                ecgCtl.Dock = DockStyle.Fill;
                tableLayoutPanel1.Controls.Add(ecgCtl);
            }
            else if (AnaValues.CurrentCase.checkItem == 2)
            {
                if (tableLayoutPanel1.HasChildren)
                {
                    ClearAndDisposeControls(tableLayoutPanel1);
                }

                BpSummaryControl bpCtl = new BpSummaryControl();
                bpCtl.GetBPSummaryData();
                bpCtl.Dock = DockStyle.Fill;
                tableLayoutPanel1.Controls.Add(bpCtl);
            }
            else
            {
                if (tableLayoutPanel1.HasChildren)
                {
                    ClearAndDisposeControls(tableLayoutPanel1);
                }

                EcgBpSummaryControl ecgBpCtl = new EcgBpSummaryControl();
                ecgBpCtl.GetEcgBpData();
                ecgBpCtl.Dock = DockStyle.Fill;
                tableLayoutPanel1.Controls.Add(ecgBpCtl);
            }

        }

    }
}
