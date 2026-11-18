using AnalysisApp.Commons;
using AnalysisSystem.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp
{
    public partial class HRVPageControl : BaseUserControl
    {
        public HRVPageControl()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            dgvData.DataSource = null;
            dgvDataFreq.DataSource = null;
            await initData();
        }

        private void HRVPageControl_Load(object sender, EventArgs e)
        {
            initTable();
            //initData();
        }

        private void initTable()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvData, true);
            this.dgvData.ColumnHeadersHeight = 26;
            this.dgvData.RowTemplate.Height = 26;

            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="type", HeaderText="类型", DataPropertyName="dataType"},
                new DataGridViewTextBoxColumn { Name="startTime", HeaderText="起始时间", DataPropertyName="startTime" },

                new DataGridViewTextBoxColumn { Name="duration", HeaderText="时长", DataPropertyName="duration" },
                new DataGridViewTextBoxColumn { Name="nns", HeaderText="NNs", DataPropertyName="nns" },
                new DataGridViewTextBoxColumn { Name="meannn", HeaderText="NN均值(ms)", DataPropertyName="meanNN" },
                new DataGridViewTextBoxColumn { Name="sdnn", HeaderText="SDNN(ms)", DataPropertyName="sdnn" },

                new DataGridViewTextBoxColumn { Name="sdnnIndex", HeaderText="SDNNindex(ms)", DataPropertyName="sdnnIndex" },
                new DataGridViewTextBoxColumn { Name="sdann", HeaderText="SDANN(ms)", DataPropertyName="sdann" },
                new DataGridViewTextBoxColumn { Name="sdsd", HeaderText="SDSD(ms)", DataPropertyName="sdsd" },
                new DataGridViewTextBoxColumn { Name="nn50Count", HeaderText="NN50", DataPropertyName="nn50Count" },
                new DataGridViewTextBoxColumn { Name="rmssd", HeaderText="RMSSD(ms)", DataPropertyName="rmssd" },

                new DataGridViewTextBoxColumn { Name="TriIndex", HeaderText="Tri.index", DataPropertyName="hrvTriangularIndex" },
                new DataGridViewTextBoxColumn { Name="pnn50", HeaderText="pNN50(%)", DataPropertyName="pnn50" }
            };
            foreach (DataGridViewColumn col in cols) { col.SortMode = DataGridViewColumnSortMode.NotSortable; }
            this.dgvData.Columns.AddRange(cols);
        }

        private async Task initData()
        {
            int hrvCalcResult = 0;
            while (!IsShown) await Task.Delay(10);

            Form parentForm = this.FindForm();
            if (this.IsActive)
            {
                AppCode.ShowProcessing(
                    "",
                    parentForm,
                    (args) =>
                    {
                        hrvCalcResult = AEcgHandler.CalculateHRV();
                        return Task.CompletedTask;
                    },
                    this,
                    new Rectangle(this.dgvData.PointToScreen(new Point(0, 0)), this.dgvData.Size),
                    true
                );
            }
            else
            {
                hrvCalcResult = AEcgHandler.CalculateHRV();
            }

            
            BindingCollection<HRVTimeDomainInfo> bindingList = new BindingCollection<HRVTimeDomainInfo>(HRVCommonMethod.GetHRVTimeDomainList(), this);
            // 绑定数据到表格
            dgvData.DataSource = bindingList;

            BindingCollection<HRVFreqDomainInfo> bindingList2 = new BindingCollection<HRVFreqDomainInfo>(HRVCommonMethod.GetHrvFreqDomainList(), this);
            // 绑定数据到表格
            dgvDataFreq.DataSource = bindingList2;

            HRVReportInfo hrvReportInfo = AnaCode.GetHRVReportInfo(hrvCalcResult);
            if (hrvReportInfo == null) return;
            lblSDNN.Text = $"SDNN-24h(ms):{hrvReportInfo.sdnn}";
            lblSDANN.Text = $"SDANN(ms):{hrvReportInfo.sdann}";
            lblSDANNIndex.Text = $"SDNN Index(ms):{hrvReportInfo.sdnnIndex}";
            lblrMSSD.Text = $"rMSSD(ms):{hrvReportInfo.rmssd}";
            lblpNN50.Text = $"pNN50(%):{hrvReportInfo.pnn50}";
            lblTriindex.Text = $"Tri.index:{hrvReportInfo.hrvTriangularIndex}";
            lblTP.Text = $"Total Power(ms²):{hrvReportInfo.total_power}";
            lblVLF.Text = $"VLF(ms²2):{hrvReportInfo.vlf}";
            lblLF.Text = $"LF(ms²):{hrvReportInfo.lf}";
            lblHF.Text = $"HF(ms²):{hrvReportInfo.hf}";
            string lfhf = Math.Round(Convert.ToDouble(hrvReportInfo.lf) / Convert.ToDouble(hrvReportInfo.hf), 1).ToString();
            lblLFHF.Text = $"LF/HF:{lfhf}";
            string HrvFilePath = "";
            if (!string.IsNullOrEmpty(AnaValues.RootPath))
            {
                HrvFilePath = Path.Combine(AnaValues.RootPath, AnaValues.HRVAdviceFileName);
            }

            if (!File.Exists(HrvFilePath))
            {
                richTbxAdvice.Clear();
                richTbxAdvice.AppendText("SDNN " + hrvReportInfo.sdnn + "ms\n");
                int sdnnValue = Convert.ToInt32(hrvReportInfo.sdnn);
                if(sdnnValue>= 100)
                {
                    richTbxAdvice.AppendText("心率变异性正常，自主神经功能状态良好\n");
                }
                else if(sdnnValue >= 50 && sdnnValue <100)
                {
                    richTbxAdvice.AppendText("心率变异性降低，自主神经功能减退\n");
                }
                else
                {
                    richTbxAdvice.AppendText("心率变异性低，自主神经调节功能差，心血管存在风险\n");
                }

                richTbxAdvice.AppendText("请结合临床");
            }
            else
            {
                // 读取JSON文件内容
                string jsonString = File.ReadAllText(HrvFilePath, Encoding.UTF8);
                var hrvAdviceJson = JObject.Parse(jsonString);
                if (hrvAdviceJson["advice"] != null)
                {
                    richTbxAdvice.Text = hrvAdviceJson["advice"].ToString();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var hrvAdviceJson = new JObject();
            hrvAdviceJson["advice"] = richTbxAdvice.Text;
            string HrvFilePath = "";
            if (!string.IsNullOrEmpty(AnaValues.RootPath))
            {
                HrvFilePath = Path.Combine(AnaValues.RootPath, AnaValues.HRVAdviceFileName);
            }
            // 将hrvAdviceJson写入HrvFilePath
            try
            {
                string jsonString = hrvAdviceJson.ToString();
                File.WriteAllText(HrvFilePath, jsonString, Encoding.UTF8);
                ScreenCenterTip.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            if (this.dgvDataFreq.Columns.Count > 0)
            {
                this.dgvDataFreq.Columns.Clear();
            }
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvDataFreq, true);
            this.dgvDataFreq.ColumnHeadersHeight = 26;
            this.dgvDataFreq.RowTemplate.Height = 26;

            DataGridViewColumn[] cols2 = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="type", HeaderText="类型", DataPropertyName="dataType"},
                new DataGridViewTextBoxColumn { Name="startTime", HeaderText="起始时间", DataPropertyName="startTime" },

                new DataGridViewTextBoxColumn { Name="duration", HeaderText="时长", DataPropertyName="duration" },
                new DataGridViewTextBoxColumn { Name="total_power", HeaderText="TP(ms²)", DataPropertyName="total_power" },
                new DataGridViewTextBoxColumn { Name="meannn", HeaderText="NN均值(ms)", DataPropertyName="mean_nn" },
                new DataGridViewTextBoxColumn { Name="vlf", HeaderText="VLF(ms²)", DataPropertyName="vlf" },
                new DataGridViewTextBoxColumn { Name="ulf", HeaderText="ULF(ms²)", DataPropertyName="ulf" },

                new DataGridViewTextBoxColumn { Name="lf", HeaderText="LF(ms²)", DataPropertyName="lf" },
                new DataGridViewTextBoxColumn { Name="hf", HeaderText="HF(ms²)", DataPropertyName="hf" },

                new DataGridViewTextBoxColumn { Name="LFnorm", HeaderText="LFnorm(nu)", DataPropertyName="lf_normal" },
                new DataGridViewTextBoxColumn { Name="HFnorm", HeaderText="HFnorm(nu)", DataPropertyName="hf_normal" },
                new DataGridViewTextBoxColumn { Name="ratio_lf_hf", HeaderText="LF/HF", DataPropertyName="ratio_lf_hf" },

            };
            foreach (DataGridViewColumn col in cols2) { col.SortMode = DataGridViewColumnSortMode.NotSortable; }

            this.dgvDataFreq.Columns.AddRange(cols2);
        }
    }
}
