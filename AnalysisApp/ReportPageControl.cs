using AnalysisApp.CommonControls;
using AnalysisApp.Commons;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalysisApp
{
    public partial class ReportPageControl : BaseUserControl
    {
        public ReportPageControl()
        {
            InitializeComponent();
        }

        private void ReportPageControl_Load(object sender, EventArgs e)
        {

        }

        public override async Task RefreshData(bool resetUI = false)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (AnaValues.CurrentCase.checkItem == 1)
            {
                if (tableLayoutPanel1.HasChildren)
                {
                    ClearAndDisposeControls(tableLayoutPanel1);
                }

                EcgReportControl ecgCtl = new EcgReportControl();
                ecgCtl.RefreshUI();
                ecgCtl.Dock = DockStyle.Fill;
                tableLayoutPanel1.Controls.Add(ecgCtl);
            }
            else if (AnaValues.CurrentCase.checkItem == 2)
            {
                if (tableLayoutPanel1.HasChildren)
                {
                    ClearAndDisposeControls(tableLayoutPanel1);
                }

                BpReportControl bpCtl = new BpReportControl();
                bpCtl.RefreshUI();
                bpCtl.Dock = DockStyle.Fill;
                tableLayoutPanel1.Controls.Add(bpCtl);
            }
            else
            {
                if (tableLayoutPanel1.HasChildren)
                {
                    ClearAndDisposeControls(tableLayoutPanel1);
                }

                EcgBpReportControl ecgBpCtl = new EcgBpReportControl();
                ecgBpCtl.RefreshUI();
                ecgBpCtl.Dock = DockStyle.Fill;
                tableLayoutPanel1.Controls.Add(ecgBpCtl);
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
    }
}
