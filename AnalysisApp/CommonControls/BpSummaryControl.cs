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
using Newtonsoft.Json.Linq;
using Zoncare.Component.Controls;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class BpSummaryControl : UserControl
    {
        public BpSummaryControl()
        {
            InitializeComponent();
        }
        public void GetBPSummaryData()
        {
            BPSummaryData bpSummaryData = CalcBPInfo.GetBPSummaryData();
            var allSummary = JObject.FromObject(bpSummaryData.bpAllData);
            // 读取TextBoxEx控件数据
            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel32);
            foreach (TextBoxEx textBox in textBoxList)
            {
                string name = textBox.Name.Substring(3);
                if (allSummary[name] != null)
                {
                    textBox.Text = allSummary[name].ToString();
                }
                else
                {
                    textBox.Text = "-";
                }
            }

            var daySummary = JObject.FromObject(bpSummaryData.bpDayData);
            // 读取TextBoxEx控件数据
            List<TextBoxEx> textBoxDayList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel8);
            foreach (TextBoxEx textBox in textBoxDayList)
            {
                string name = textBox.Name.Substring(3);
                if (daySummary[name] != null)
                {
                    textBox.Text = daySummary[name].ToString();
                }
                else
                {
                    textBox.Text = "-";
                }
            }

            var nightSummary = JObject.FromObject(bpSummaryData.bpNightData);
            // 读取TextBoxEx控件数据
            List<TextBoxEx> textBoxNightList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel38);
            foreach (TextBoxEx textBox in textBoxNightList)
            {
                string name = textBox.Name.Substring(5);
                if (nightSummary[name] != null)
                {
                    textBox.Text = nightSummary[name].ToString();
                }
                else
                {
                    textBox.Text = "-";
                }
            }

        }
    }
}
