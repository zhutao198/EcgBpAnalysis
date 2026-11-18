using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp.CommonControls
{
    public partial class UserEventListControl : UserControl
    {
        public UserEventListControl()
        {
            InitializeComponent();
        }

        private void UserEventListControl_Load(object sender, EventArgs e)
        {
            initTable();
        }

        private void initTable()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvData, true);
            this.dgvData.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 8F, FontStyle.Bold);
            this.dgvData.DefaultCellStyle.Font = new Font("微软雅黑", 7.5F);
            this.dgvData.RowTemplate.Height = 22;
            this.dgvData.ColumnHeadersHeight = 25;

            ////复选框
            //DataGridViewCheckBoxColumn col0 = this.dgvData.AddCheckBoxHeaderColumn((colIndex, state) =>
            //{
            //    this.dgvData.EndEdit();
            //    this.dgvData.Rows.OfType<DataGridViewRow>().ToList().ForEach(t => { t.Cells[colIndex].Value = state; });
            //});
            //col0.MinimumWidth = 30;
            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="eventLabel", HeaderText=CommonValues.Langs["事件"], DataPropertyName="eventLabel" },
                new DataGridViewTextBoxColumn { Name="eventStartTime", HeaderText=CommonValues.Langs["发生时间"], DataPropertyName="eventStartTime"},

            };
            this.dgvData.Columns.AddRange(cols);
        }
    }
}
