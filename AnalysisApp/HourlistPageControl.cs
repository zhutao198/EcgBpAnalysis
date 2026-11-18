using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System;
using System.Drawing;
using System.Dynamic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component.Widgets;

namespace AnalysisApp
{
    public partial class HourlistPageControl : BaseUserControl
    {
        public override async Task RefreshData(bool resetUI = false)
        {
            //CalcHourListInfo.HourDataList.Clear();
            dgvEcg.DataSource = null;
            dgvBP.DataSource = null;
            initData();
        }

        public HourlistPageControl()
        {
            InitializeComponent();
        }

        private void HourlistPageControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (!IsUiInitialized) InitUI();
                    //initData();

                    Console.WriteLine("小时数据表界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "小时数据表");
            }
        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                initTable();
                initBPTable();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "小时数据表");
            }
        }

        private void initTable()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvEcg, true);
            this.dgvEcg.ColumnHeadersHeight = 50;
            this.dgvEcg.RowTemplate.Height = 26;

            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="time", HeaderText="时间", DataPropertyName="Time"},
                new DataGridViewTextBoxColumn { Name="totalBeat", HeaderText="总心搏", DataPropertyName="TotalBeat" },

                new DataGridViewTextBoxColumn { Name="minHR", HeaderText="最慢", DataPropertyName="MinHR" },
                new DataGridViewTextBoxColumn { Name="avgHR", HeaderText="平均", DataPropertyName="AvgHR" },
                new DataGridViewTextBoxColumn { Name="maxHR", HeaderText="最快", DataPropertyName="MaxHR" },

                new DataGridViewTextBoxColumn { Name="singleV", HeaderText="单发", DataPropertyName="SingleV" },
                new DataGridViewTextBoxColumn { Name="pairedV", HeaderText="成对", DataPropertyName="PairedV" },
                new DataGridViewTextBoxColumn { Name="bigeminyV", HeaderText="二联律", DataPropertyName="BigeminyV" },
                new DataGridViewTextBoxColumn { Name="trigeminyV", HeaderText="三联律", DataPropertyName="TrigeminyV" },
                new DataGridViewTextBoxColumn { Name="tachycardiaV", HeaderText="室速", DataPropertyName="TachycardiaV" },
                new DataGridViewTextBoxColumn { Name="totalBeatV", HeaderText="总数", DataPropertyName="TotalBeatV" },

                new DataGridViewTextBoxColumn { Name="singleS", HeaderText="单发", DataPropertyName="SingleS" },
                new DataGridViewTextBoxColumn { Name="pairedS", HeaderText="成对", DataPropertyName="PairedS" },
                new DataGridViewTextBoxColumn { Name="bigeminyS", HeaderText="二联律", DataPropertyName="BigeminyS" },
                new DataGridViewTextBoxColumn { Name="trigeminyS", HeaderText="三联律", DataPropertyName="TrigeminyS" },
                new DataGridViewTextBoxColumn { Name="tachycardiaS", HeaderText="房速", DataPropertyName="TachycardiaS" },
                new DataGridViewTextBoxColumn { Name="totalBeatS", HeaderText="总数", DataPropertyName="TotalBeatS" },

                new DataGridViewTextBoxColumn { Name="asystole", HeaderText="停搏", DataPropertyName="Asystole" },
            };

            foreach (DataGridViewColumn col in cols) { col.SortMode = DataGridViewColumnSortMode.NotSortable; }
            this.dgvEcg.Columns.AddRange(cols);

            // 为每列单独设置样式（覆盖默认）
            foreach (DataGridViewColumn col in this.dgvEcg.Columns)
            {
                col.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            }

            this.dgvEcg.AddSpanHeader(2, 3, "心率");
            this.dgvEcg.AddSpanHeader(5, 6, "室性早搏");
            this.dgvEcg.AddSpanHeader(11, 6, "房性早搏");
        }

        private void initBPTable()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvBP, true);
            this.dgvBP.ColumnHeadersHeight = 50;
            this.dgvBP.RowTemplate.Height = 26;

            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="date", HeaderText="日期", DataPropertyName="Date"},
                new DataGridViewTextBoxColumn { Name="time", HeaderText="时间", DataPropertyName="Time" },

                new DataGridViewTextBoxColumn { Name="sBP", HeaderText="收缩压(mmHg)", DataPropertyName="SBP" },
                new DataGridViewTextBoxColumn { Name="aBP", HeaderText="舒张压(mmHg)", DataPropertyName="DBP" },
                new DataGridViewTextBoxColumn { Name="mAP", HeaderText="平均动脉压(mmHg)", DataPropertyName="MAP" },

                new DataGridViewTextBoxColumn { Name="pulse", HeaderText="脉率(次/分)", DataPropertyName="Pulse" },
                new DataGridViewTextBoxColumn { Name="count", HeaderText="个数", DataPropertyName="Count" },
            };

            foreach (DataGridViewColumn col in cols) { col.SortMode = DataGridViewColumnSortMode.NotSortable; }
            this.dgvBP.Columns.AddRange(cols);

            // 为每列单独设置样式（覆盖默认）
            foreach (DataGridViewColumn col in this.dgvBP.Columns)
            {
                col.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            }

            this.dgvBP.CellFormatting += dgvBP_CellFormatting;

        }

        private async void initData()
        {
            if (!this.IsActive)
            {
                while (CalcHourListInfo.IsCalculating)
                {
                    await Task.Delay(10);
                    if (this.IsActive) break;
                }
            }

            if (AnaValues.CurrentCase.checkItem == 1)
            {
                this.headerlessTabControl1.ApplyHeaderlessStyle = true;
                this.headerlessTabControl1.SelectedIndex = 0;
            }
            else if (AnaValues.CurrentCase.checkItem == 2)
            {
                this.headerlessTabControl1.ApplyHeaderlessStyle = true;
                this.headerlessTabControl1.SelectedIndex = 1;
            }
            else
            {
                this.headerlessTabControl1.ApplyHeaderlessStyle = false;
                this.headerlessTabControl1.SelectedIndex = 0;
            }

            this.Refresh();
            //后台加载切换到前台加载时, 显示动画
            if (this.IsActive && CalcHourListInfo.IsCalculating)
            {
                Form parentForm = this.FindForm();
                BaseUserControl thisUC = this;
                AppCode.ShowProcessing("",
                    parentForm,
                    async (args) =>
                    {
                        dynamic paraInfo = args as ExpandoObject;
                        LoadingForm frm = paraInfo.Form as LoadingForm;
                        while (CalcHourListInfo.IsCalculating)
                        {
                            //if (thisUC.IsActive) frm.Visible = true;
                            await Task.Delay(10);
                        }
                    },
                    this,
                    new Rectangle(PointToScreen(this.dgvEcg.Location), this.dgvEcg.Size),
                    true);
            }

            BindingCollection<HourlistInfo> bindingList = new BindingCollection<HourlistInfo>(CalcHourListInfo.HourDataList, this);
            // 绑定数据到表格
            dgvEcg.DataSource = bindingList;

            //Console.WriteLine("--DataSource--", bindingList.Count);

            BindingCollection<BPHourListInfo> bindingBPList = new BindingCollection<BPHourListInfo>(CalcBPInfo.GetHourList(), this);
            // 绑定数据到表格
            dgvBP.DataSource = bindingBPList;

            if (CalcBPInfo.settingValues.unit == "kPa")
            {
                this.dgvBP.Columns["sBP"].HeaderText = "收缩压(kPa)";
                this.dgvBP.Columns["aBP"].HeaderText = "舒张压(kPa)";
                this.dgvBP.Columns["mAP"].HeaderText = "平均动脉压(kPa)";
            }
            else
            {
                this.dgvBP.Columns["sBP"].HeaderText = "收缩压(mmHg)";
                this.dgvBP.Columns["aBP"].HeaderText = "舒张压(mmHg)";
                this.dgvBP.Columns["mAP"].HeaderText = "平均动脉压(mmHg)";
            }
        }

        private void dgvBP_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewColumn col = this.dgvBP.Columns[e.ColumnIndex];
            DataGridViewRow row = this.dgvBP.Rows[e.RowIndex];
            BPHourListInfo rowData = row.DataBoundItem as BPHourListInfo;

            double sBP = rowData.SBP;
            double dBP = rowData.DBP;
            double mAP = rowData.MAP;

            string sBPStr = "";
            string dBPStr = "";
            string mAPStr = "";

            bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            if (settingValues.unit == "kPa")
            {
                sBPStr = CommonFuns.mmHg2KPa(Convert.ToInt32(sBP), 2).ToString("F2");
                dBPStr = CommonFuns.mmHg2KPa(Convert.ToInt32(dBP), 2).ToString("F2");
                mAPStr = CommonFuns.mmHg2KPa(Convert.ToInt32(mAP), 2).ToString("F2");
            }
            else
            {
                sBPStr = Convert.ToInt32(sBP).ToString();
                dBPStr = Convert.ToInt32(dBP).ToString();
                mAPStr = Convert.ToInt32(mAP).ToString();
            }

            if (col.Name == "sBP")
            {
                if (e.Value != null) { e.Value = sBPStr; }
            }
            if (col.Name == "aBP")
            {
                if (e.Value != null) { e.Value = dBPStr; }
            }
            if (col.Name == "mAP")
            {
                if (e.Value != null) { e.Value = mAPStr; }
            }
        }

    }
}
