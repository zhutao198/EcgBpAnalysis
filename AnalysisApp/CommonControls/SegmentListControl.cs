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
using System.IO;
using Zoncare.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnalysisApp.CommonControls
{
    public partial class SegmentListControl : UserControl
    {
        private bool bFirstLoad = true;
        private string SegmentFilePath = "";
        List<JObject> SegmentJsonArray = new List<JObject>();
        private SegmentPrintManager segmentPrintManager = null;
        public event Action OnModifySegment;

        public SegmentListControl()
        {
            InitializeComponent();
        }

        private void SegmentListControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                initTable();
                initData();
            }
        }

        private void initTable()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvData, true);

            //复选框
            DataGridViewCheckBoxColumn col0 = this.dgvData.AddCheckBoxHeaderColumn((colIndex, state) =>
            {
                this.dgvData.EndEdit();
                this.dgvData.Rows.OfType<DataGridViewRow>().ToList().ForEach(t => 
                { 
                    t.Cells[colIndex].Value = state;
                });
            });
            col0.MinimumWidth = 30;
            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="segmentStartTime", HeaderText=CommonValues.Langs["发生时间"], DataPropertyName="segmentStartTime"},
                new DataGridViewTextBoxColumn { Name="segmentLabel", HeaderText=CommonValues.Langs["片段图标签"], DataPropertyName="segmentLabel", SortMode= DataGridViewColumnSortMode.NotSortable },
                new DataGridViewTextBoxColumn { Name="segmentDuration", HeaderText=CommonValues.Langs["时长"] +"(s)", DataPropertyName="segmentDuration", SortMode= DataGridViewColumnSortMode.NotSortable },
                new DataGridViewTextBoxColumn { Name="segmentStartSample", DataPropertyName="segmentStartSample", Visible=false },
                new DataGridViewTextBoxColumn { Name="segmentDirection", DataPropertyName="segmentDirection", Visible=false },

            };
            this.dgvData.Columns.AddRange(cols);
            this.dgvData.CellMouseClick += dgvData_CellContentClick;
            this.dgvData.Sorted += dgvData_Sorted;
        }
        public void initData()
        {
            SegmentJsonArray.Clear();
            this.dgvData.DataSource = null;
            SegmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            // 判断文件是否存在
            if (!File.Exists(SegmentFilePath))
            {
                return;
            }

            // 文件存在，读取现有JSON数组
            string jsonContent = File.ReadAllText(SegmentFilePath);
            SegmentJsonArray = JsonConvert.DeserializeObject<List<JObject>>(jsonContent) ?? new List<JObject>();

            // 将片段图信息填充到dgvData列表中
            BindDataGridView(SegmentJsonArray);
            if (bFirstLoad)
            {
                this.dgvData.AutoResizeColumns();
                bFirstLoad = false;
            }
        }

        private void BindDataGridView(List<JObject> segmentJsonArray)
        {
            // 清空现有数据
            dgvData.Rows.Clear();

            if (segmentJsonArray == null || segmentJsonArray.Count == 0)
            {
                return;
            }
            List<DisplaySegment> displaySegmentList = new List<DisplaySegment>();
            foreach (JObject segment in segmentJsonArray)
            {
                string segmentStartTime = segment["tbxStartTime"]?.Value<string>() ?? "";
                DisplaySegment displaySegment = new DisplaySegment();
                displaySegment.segmentStartSample = segment["startSample"]?.Value<int>() ?? 0;
                displaySegment.segmentStartTime = segmentStartTime;
                displaySegment.segmentLabel = segment["cbxLabel"]?.Value<string>() ?? "";
                displaySegment.segmentDuration = segment["cbxDuration"]?.Value<int>() ?? 0;
                displaySegment.segmentDirection = segment["cbxDirection"]?.Value<int>() ?? 0;
                displaySegmentList.Add(displaySegment);
            }
            BindingCollection<DisplaySegment> bindingList = new BindingCollection<DisplaySegment>(displaySegmentList, this);
            this.dgvData.DataSource = bindingList;
            // 更新界面显示
            dgvData.Refresh();
        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }
            try
            {
                DataGridView dgv = sender as DataGridView;
                DataGridViewRow clickedRow = dgv.Rows[e.RowIndex];
                DataGridViewCell curCell = clickedRow.Cells[0];
                if (curCell is DataGridViewCheckBoxCell)
                {
                    if (curCell.Value == null)
                    {
                        curCell.Value = true;
                    }
                    else
                    {
                        bool isChecked = (bool)curCell.Value;
                        if (isChecked)
                        {
                            curCell.Value = false;
                            dgv.Columns[0].HeaderCell.Value = false;
                        }
                        else
                        {
                            curCell.Value = true;
                        }

                    }
                    bool bSelected = (bool)curCell.Value;

                    dgv.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "片段图列表");
            }
        }

        private void dgvData_Sorted(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (dgv.SortedColumn != null)
            {
                // 获取排序后的 startSample 数据（列索引4）
                var afterSortData = GetColumnData(4);

                // 重新排序 SegmentJsonArray 以匹配排序后的 startSample 顺序
                List<JObject> sortedSegmentJsonArray = new List<JObject>();
                foreach (var startSampleObj in afterSortData)
                {
                    int startSample = Convert.ToInt32(startSampleObj);
                    var matchedSegment = SegmentJsonArray.FirstOrDefault(s => s["startSample"]?.Value<int>() == startSample);
                    if (matchedSegment != null)
                    {
                        sortedSegmentJsonArray.Add(matchedSegment);
                    }
                }

                // 更新 SegmentJsonArray
                SegmentJsonArray = sortedSegmentJsonArray;

                // 将排序后的JSON写回文件
                string jsonContent = JsonConvert.SerializeObject(SegmentJsonArray, Formatting.Indented);
                File.WriteAllText(SegmentFilePath, jsonContent);

                OnModifySegment?.Invoke();

            }
        }

        /// <summary>
        /// 获取指定列的所有数据
        /// </summary>
        private List<object> GetColumnData(int columnIndex)
        {
            List<object> values = new List<object>();

            foreach (DataGridViewRow row in dgvData.Rows)
            {
                if (!row.IsNewRow)  // 跳过新行
                {
                    values.Add(row.Cells[columnIndex].Value);
                }
            }

            return values;
        }


        public class DisplaySegment {
            public string segmentStartTime { get; set; }
            public string segmentLabel { get; set; }
            public int segmentDuration { get; set; }
            public int segmentStartSample { get; set; }
            public int segmentDirection { get; set; }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            List<DisplaySegment> lstSelected = new List<DisplaySegment>();
            foreach (DataGridViewRow row in dgvData.Rows)
            {
                DataGridViewCell cell0 = row.Cells[0];
                if (cell0 is DataGridViewCheckBoxCell && cell0.Value is bool)
                {
                    if ((bool)cell0.Value == true) lstSelected.Add(row.DataBoundItem as DisplaySegment);
                }
            }

            if (lstSelected.Count == 0)
            {
                CommonFuns.PromptMessage(CommonValues.Langs["请选择要删除的片段图"], 1);
                return;
            }
            // 确认删除操作
            if (CommonFuns.PromptMessage(CommonValues.Langs["确定删除片段图吗？此操作不可恢复。"], 1, MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            List<int> startSamples = new List<int>();
            for(int i=0; i< lstSelected.Count; i++)
            {
                startSamples.Add(lstSelected[i].segmentStartSample);
            }
            OperateSegment.deleteSegment(SegmentFilePath, startSamples);

            initData();

            // 将全选复选框置为未勾选状态
            if (dgvData.Columns.Count > 0 && dgvData.Columns[0] is DataGridViewCheckBoxColumn)
            {
                dgvData.Columns[0].HeaderCell.Value = false;
            }

            OnModifySegment?.Invoke();

        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            List<DisplaySegment> lstSelected = new List<DisplaySegment>();
            foreach (DataGridViewRow row in dgvData.Rows)
            {
                DataGridViewCell cell0 = row.Cells[0];
                if (cell0 is DataGridViewCheckBoxCell && cell0.Value is bool)
                {
                    if ((bool)cell0.Value == true) lstSelected.Add(row.DataBoundItem as DisplaySegment);
                }
            }
            if (lstSelected.Count == 0)
            {
                CommonFuns.PromptMessage(CommonValues.Langs["请选择要打印的片段图"], 1);
                return;
            }
            List<int> startSamples = new List<int>();
            for (int i = 0; i < lstSelected.Count; i++)
            {
                startSamples.Add(lstSelected[i].segmentStartSample);
            }

            bool bHorizontalPrint = lstSelected[0].segmentDirection != 1;

            List<JObject> SegmentInfoJsons = OperateSegment.getSegmentObj(SegmentFilePath, startSamples);

            List<JObject> SegmentPageInfos = OperateSegment.getSegmentPageInfo(SegmentInfoJsons);

            List<List<JObject>> SegmentAllPageInfo = OperateSegment.CalcSegmentPrintPageData(SegmentPageInfos, SegmentInfoJsons);
            printSegmentWaves(SegmentAllPageInfo, bHorizontalPrint);
        }

        private void printSegmentWaves(List<List<JObject>> SegmentAllPageInfo, bool bHorizontalPrint)
        {
            int totalPages = SegmentAllPageInfo.Count;
            segmentPrintManager = new SegmentPrintManager(bHorizontalPrint, totalPages)
            {
                SegmentAllPageInfo = SegmentAllPageInfo,

            };
            segmentPrintManager.SetOnDemandRendering();
            segmentPrintManager.ShowPrintDialog();
        }

        public void OnControlDeleteSegment()
        {
            initData();
        }
    }
}
