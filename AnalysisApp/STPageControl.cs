using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp
{
    public partial class STPageControl : BaseUserControl
    {
        public class STEventInfo
        {
            public string StartTime { get; set; } = "";
            public double Duration { get; set; } = 0;
            public string StPointVoltage { get; set; } = "";
            public string STLead { get; set; } = "";
            public int StartSample { get; set; } = 0;
            public int EndSample { get; set; } = 0;
            public int LeadIdx { get; set; } = 0;
        }

        //private AddSTEventForm addSTEvent = null;
        private static object _lockObj = new object();

        public STPageControl()
        {
            InitializeComponent();
            this.drawEcgCommonWave1.currentLeadCount = 12;
            //if (addSTEvent == null)
            //{
            //    addSTEvent = new AddSTEventForm();
            //    addSTEvent.OnRefreshStTable += refreshListData;
            //}
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            this.dgvData.DataSource = null;

            refreshListData();

            this.stTrendWaveControl1.IsActive = true;
            if (this.Parent is TabPage parentPage)
            {
                if ((parentPage.Parent as TabControl).SelectedTab != parentPage)
                {
                    this.stTrendWaveControl1.IsActive = false;
                }
            }
            await this.stTrendWaveControl1.RefreshData();
            this.drawEcgCommonWave1.RefreshData();
        }

        private void STPageControl_Load(object sender, EventArgs e)
        {
            this.btnSTWave.BaseColor = Color.FromArgb(84, 132, 207);
            initTable();
            refreshListData();
            stTrendWaveControl1.OnStartTimeChanged += drawEcgCommonWave1.OnPageScanStartTimeChanged;
        }

        private void refreshListData()
        {
            List<AEcgHandler.AEcgStEventInfo> stEventInfoList = AEcgHandler.GetStEventInfo();
            initData(stEventInfoList);
        }

        //重置选择
        private void resetSelect()
        {
            this.btnSTWave.IsHighlight = true;
            this.btnSTWave.ForeColor = SystemColors.ButtonHighlight;
            this.btnTWave.IsHighlight = false;
            this.btnTWave.ForeColor = Color.FromArgb(84, 132, 207);
        }

        private void initTable()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvData, true);

            //复选框
            DataGridViewCheckBoxColumn col0 = this.dgvData.AddCheckBoxHeaderColumn((colIndex, state) =>
            {
                this.dgvData.EndEdit();
                this.dgvData.Rows.OfType<DataGridViewRow>().ToList().ForEach(t => { t.Cells[colIndex].Value = state; });
            });
            col0.FillWeight = 12;
            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="startTime", HeaderText=CommonValues.Langs["起始时间"], DataPropertyName="StartTime",FillWeight=30 },
                new DataGridViewTextBoxColumn { Name="duration", HeaderText=CommonValues.Langs["持续(分钟)"], DataPropertyName="Duration",FillWeight=23 },
                new DataGridViewTextBoxColumn { Name="ste", HeaderText="STE(mV)", DataPropertyName="StPointVoltage",FillWeight=20 },
                new DataGridViewTextBoxColumn { Name="stLead", HeaderText=CommonValues.Langs["导联"], DataPropertyName="STLead",FillWeight=13 },
                new DataGridViewTextBoxColumn { Name="startSample", DataPropertyName="StartSample", Visible=false},
                new DataGridViewTextBoxColumn { Name="endSample", DataPropertyName="EndSample", Visible=false},
                new DataGridViewTextBoxColumn { Name="leadIdx", DataPropertyName="LeadIdx", Visible=false},
            };

            this.dgvData.Columns.AddRange(cols);

            this.dgvData.CellMouseClick += dgvData_CellContentClick;

        }

        private void initData(List<AEcgHandler.AEcgStEventInfo> stEventInfoList)
        {
            int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;

            dgvData.Rows.Clear();
            //List<AEcgHandler.AEcgStEventInfo> stEventInfoList = AEcgHandler.GetStEventInfo();
            if (stEventInfoList == null)
            {
                return;
            }
            List<STEventInfo> events = new List<STEventInfo>();
            for (int i = 0; i < stEventInfoList.Count; i++)
            {
                STEventInfo stEventInfo = new STEventInfo();
                if (dateFormat == 0)
                {
                    stEventInfo.StartTime = AnaCode.getDataTimeFromSample(stEventInfoList[i].startPosition).ToString("HH:mm:ss MM-dd");

                }
                else
                {
                    stEventInfo.StartTime = AnaCode.getDataTimeFromSample(stEventInfoList[i].startPosition).ToString("HH:mm:ss dd-MM");
                }
                stEventInfo.Duration = Math.Round((double)stEventInfoList[i].durationTime / AnaValues.holterConfig.SampleRate / 60, 2);
                stEventInfo.StPointVoltage = ((float)Math.Round(stEventInfoList[i].stPointVoltage / 1000, 2)).ToString();
                stEventInfo.STLead = AnaValues.LeadIdxToNameMap[stEventInfoList[i].lead];
                stEventInfo.StartSample = stEventInfoList[i].startPosition;
                stEventInfo.EndSample = stEventInfoList[i].startPosition + stEventInfoList[i].durationTime;
                stEventInfo.LeadIdx = stEventInfoList[i].lead;
                events.Add(stEventInfo);
            }

            BindingCollection<STEventInfo> bindingList = new BindingCollection<STEventInfo>(events, this);

            // 绑定数据到表格
            dgvData.DataSource = bindingList;
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
                if (e.ColumnIndex == 0)
                {
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

                        dgv.InvalidateCell(e.ColumnIndex, e.RowIndex);
                    }
                }
                else
                {
                    DataGridViewCell curCell = clickedRow.Cells[5];
                    if (curCell is DataGridViewTextBoxCell)
                    {
                        int startSample = Convert.ToInt32(curCell.Value);
                        float STSecond = (float)startSample / AnaValues.holterConfig.SampleRate;
                        this.stTrendWaveControl1.ClickMiniute = Convert.ToInt32(STSecond / 60);
                        this.stTrendWaveControl1.reRendWave();

                        drawEcgCommonWave1.OnPageScanStartTimeChanged(STSecond);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "ST事件列表");
            }
        }

        private void btnSTWave_Click(object sender, EventArgs e)
        {
            resetSelect();

            this.stTrendWaveControl1.type = "ST";
            this.stTrendWaveControl1.ClickMiniute = -1;
            this.stTrendWaveControl1.updataTicks();
            this.stTrendWaveControl1.reRendWave();
        }

        private void btnTWave_Click(object sender, EventArgs e)
        {
            this.btnSTWave.IsHighlight = false;
            this.btnSTWave.ForeColor = Color.FromArgb(84, 132, 207);
            this.btnTWave.IsHighlight = true;
            this.btnTWave.ForeColor = SystemColors.ButtonHighlight;

            this.stTrendWaveControl1.type = "T";
            this.stTrendWaveControl1.ClickMiniute = -1;
            this.stTrendWaveControl1.updataTicks();
            this.stTrendWaveControl1.reRendWave();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            List<STEventRange> stEventList = new List<STEventRange>();
            int rows = this.dgvData.RowCount;
            for (int i = 0; i < rows; i++)
            {
                DataGridViewRow currentRow = this.dgvData.Rows[i];
                DataGridViewCell curCell = currentRow.Cells[0];
                if (curCell is DataGridViewCheckBoxCell)
                {
                    if (curCell.Value != null && (bool)curCell.Value)
                    {
                        STEventRange stEvent = new STEventRange();
                        stEvent.StartIndex = Convert.ToInt32(currentRow.Cells[5].Value);
                        stEvent.EndIndex = Convert.ToInt32(currentRow.Cells[6].Value);
                        stEvent.LeadIndex = (short)(Convert.ToInt32(currentRow.Cells[7].Value));
                        stEventList.Add(stEvent);
                    }
                }
            }

            AEcgHandler.DeleteSTEvents(stEventList.ToArray());
            List<AEcgHandler.AEcgStEventInfo> stEventInfoList = AEcgHandler.GetStEventInfo();
            initData(stEventInfoList);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();

            AddSTEventForm addSTEvent = new AddSTEventForm();
            addSTEvent.OnRefreshStTable += refreshListData;

            addSTEvent.ShowDialog(parentForm);
        }
    }
}
