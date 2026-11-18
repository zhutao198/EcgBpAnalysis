using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.DAL;
using Microsoft.EntityFrameworkCore;
using Zoncare.Component;
using Zoncare.Common;
using AnalysisApp.Commons;

namespace AnalysisApp.CommonControls
{
    public partial class BpCompareControl : BaseUserControl
    {
        [DefaultValue(0), Category("自定义属性"), Description("索引"), Browsable(true)]
        public int CaseIndex { get; set; } = 0;

        private string Name;
        private List<List<bpDetail>> BpDetailsList;

        public BpCompareControl()
        {
            InitializeComponent();
            initGrid();
        }

        public void InitData(string name)
        {
            Name = name;
            List<examCase> caseList = ZoncareEntities.Instance.ExamCases.AsNoTracking().Where(o => o.name == name).OrderByDescending(t=>t.appointmentTime).ToList();
            BpDetailsList = new List<List<bpDetail>>(caseList.Count);
            for (int i=0; i<caseList.Count; i++)
            {
                List<bpDetail> bpDetails = ZoncareEntities.Instance.BpDetails.AsNoTracking().Where(o => o.caseId == caseList[i].id).OrderBy(t=>t.time).ToList();
                BpDetailsList.Add(bpDetails);
            }
            fillNameListComboBox();
            this.bpTrendWaveControl1.UpdateDataRefresh();
        }

        private void fillNameListComboBox()
        {
            this.nameList.ValueMember = "ItemValue";
            this.nameList.DisplayMember = "ItemText";

            List<ListBoxItem> items = new List<ListBoxItem>();

            for(int i=0; i<BpDetailsList.Count; i++)
            {
                List<bpDetail> detailList = BpDetailsList[i];
                string itemText = Name + detailList[0].time;
                ListBoxItem item = new ListBoxItem(detailList[0].caseId, itemText);
                items.Add(item);
            }
            this.nameList.DataSource = items;

            if(CaseIndex == 0)
            {
                int caseId = BpDetailsList[0][0].caseId;
                this.nameList.SelectedValue = caseId;
                this.bpTrendWaveControl1.CaseId = caseId;
                initGridData(caseId);
            }
            else
            {
                int caseId = 0;
                if (BpDetailsList.Count > 1)
                {
                    caseId = BpDetailsList[1][0].caseId;
                }
                else
                {
                    caseId = BpDetailsList[0][0].caseId;
                }
                this.nameList.SelectedValue = caseId;
                this.bpTrendWaveControl1.CaseId = caseId;
                initGridData(caseId);
            }
        }

        private void initGrid()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvBP);
            this.dgvBP.ColumnHeadersHeight = 26;
            this.dgvBP.RowTemplate.Height = 26;

            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="type", HeaderText="类型", DataPropertyName="Type" },
                new DataGridViewTextBoxColumn { Name="all", HeaderText="全程", DataPropertyName="All" },
                new DataGridViewTextBoxColumn { Name="day", HeaderText="白天", DataPropertyName="Day" },
                new DataGridViewTextBoxColumn { Name="night", HeaderText="夜间", DataPropertyName="Night" },
            };
            foreach (DataGridViewColumn col in cols)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            this.dgvBP.Columns.AddRange(cols);

            // 为每列单独设置样式（覆盖默认）
            foreach (DataGridViewColumn col in this.dgvBP.Columns)
            {
                col.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            }
        }

        private void initGridData(int caseId)
        {
            this.dgvBP.DataSource = null;

            BindingCollection<BPCompareInfo> bindingBPList = new BindingCollection<BPCompareInfo>(CalcBPInfo.GetBPCompareInfo(caseId), this);
            // 绑定数据到表格
            this.dgvBP.DataSource = bindingBPList;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fillNameListComboBox();
        }

        private void nameList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectIdx = nameList.SelectedIndex;
            int caseId = BpDetailsList[selectIdx][0].caseId;
            initGridData(caseId);
            this.bpTrendWaveControl1.CaseId = caseId;
            this.bpTrendWaveControl1.UpdateDataRefresh();

        }
    }
}
